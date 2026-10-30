"""Serialized build/test execution, short-lived raw evidence, exact-input receipts."""
import json
import os
import platform
import signal
import subprocess
import sys
import tempfile
import time
import xml.etree.ElementTree as ET
from datetime import datetime, timezone
from pathlib import Path

from .evidence import fingerprint, output_fingerprint, output_manifest, read_trx, write_json


class RunLock:
    """OS-owned lock automatically releases on crash; never guess stale PIDs."""
    def __init__(self, directory):
        self.path = Path(directory) / "run.lock"
        self.stream = None

    def __enter__(self):
        self.path.parent.mkdir(parents=True, exist_ok=True)
        self.stream = self.path.open("a+b")
        try:
            if os.name == "nt":
                import msvcrt
                if self.path.stat().st_size == 0:
                    self.stream.write(b"0")
                    self.stream.flush()
                self.stream.seek(0)
                msvcrt.locking(self.stream.fileno(), msvcrt.LK_NBLCK, 1)
            else:
                import fcntl
                fcntl.flock(self.stream, fcntl.LOCK_EX | fcntl.LOCK_NB)
        except OSError:
            self.stream.close()
            raise RuntimeError("Another verification is using shared build outputs: " + str(self.path))
        return self

    def __exit__(self, *args):
        if os.name == "nt":
            import msvcrt
            self.stream.seek(0)
            msvcrt.locking(self.stream.fileno(), msvcrt.LK_UNLCK, 1)
        self.stream.close()


def execute(command, cwd, log, timeout, announce):
    """No shell; keep output on disk temporarily, surface progress and failure tail."""
    announce("Running: " + subprocess.list2cmdline([str(c) for c in command]))
    start = time.monotonic()
    with Path(log).open("w+") as stream:
        process = subprocess.Popen(command, cwd=cwd, stdout=stream, stderr=subprocess.STDOUT,
                                   start_new_session=os.name != "nt")
        try:
            while True:
                try:
                    code = process.wait(timeout=min(30, max(0.1, timeout - (time.monotonic() - start))))
                    break
                except subprocess.TimeoutExpired:
                    elapsed = time.monotonic() - start
                    if elapsed >= timeout:
                        raise TimeoutError("Command exceeded " + str(timeout) + " seconds")
                    announce("Still running (" + str(round(elapsed)) + "s): " + str(command[0]))
        except BaseException:
            if process.poll() is None:
                if os.name == "nt":
                    subprocess.run(["taskkill", "/PID", str(process.pid), "/T", "/F"], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
                else:
                    os.killpg(process.pid, signal.SIGTERM)
                try:
                    process.wait(timeout=5)
                except subprocess.TimeoutExpired:
                    if os.name != "nt":
                        os.killpg(process.pid, signal.SIGKILL)
                    else:
                        process.kill()
                    process.wait()
            raise
        if code:
            stream.seek(0, 2)
            size = stream.tell()
            stream.seek(max(0, size - 16000))
            raise RuntimeError("Command exited " + str(code) + ":\n" + stream.read())
    return round(time.monotonic() - start, 3)


def write_solution(path, repo, projects):
    solution = ET.Element("Solution")
    for project in sorted(set(projects)):
        ET.SubElement(solution, "Project", Path=str(repo.root / project))
    ET.ElementTree(solution).write(path, encoding="utf-8", xml_declaration=True)


def write_settings(path, classes):
    root = ET.Element("RunSettings")
    config = ET.SubElement(root, "RunConfiguration")
    # Settings avoid OS command-line limits when several hundred classes are selected.
    ET.SubElement(config, "TestCaseFilter").text = "|".join("FullyQualifiedName~" + name + "." for name in classes)
    ET.SubElement(config, "MaxCpuCount").text = "1"
    ET.ElementTree(root).write(path, encoding="utf-8", xml_declaration=True)


def specialist_context(repo):
    return fingerprint({"source": repo.snapshot(), "environment": dict(os.environ), "python": sys.version, "platform": platform.platform()})


def run_specialist_check(repo, check, command, output, timeout=3600, announce=None):
    """Record actual specialist commands; the command must implement the stated check.

    This records auditable execution evidence, not a manual 'acknowledged' flag.
    A multi-step obligation requires a verifier script that checks every step.
    """
    if not command:
        raise ValueError("A specialist check requires an executable verification command")
    if not check.get("verifier") or command != check["verifier"]:
        raise ValueError("Specialist completion requires the exact complete verifier registered in test-policy.json; unregistered or partial commands cannot discharge " + check["id"])
    output = Path(output)
    announce = announce or (lambda message: None)
    with RunLock(output):
        context = specialist_context(repo)
        result = {"check": check, "context": context, "command": command, "status": "failed", "time_utc": datetime.now(timezone.utc).isoformat()}
        with tempfile.TemporaryDirectory(prefix="atomui-specialist-") as temporary:
            try:
                log = Path(temporary) / "check.log"
                result["seconds"] = execute(command, repo.root, log, timeout, announce)
                if specialist_context(repo) != context:
                    raise ValueError("Inputs changed during specialist verification")
                import hashlib
                result["log_sha256"] = hashlib.sha256(log.read_bytes()).hexdigest()
                result["status"] = "passed"
            except (OSError, ValueError, RuntimeError, TimeoutError, subprocess.SubprocessError) as error:
                result["error"] = str(error)
        path = output / "specialist.json"
        try:
            evidence = json.loads(path.read_text()) if path.exists() else {}
        except (OSError, ValueError):
            evidence = {}
        evidence[check["id"]] = result
        write_json(path, evidence)
        return result


def matching_specialist_checks(repo, plan, output):
    if not plan["obligations"]:
        return []
    try:
        evidence = json.loads((output / "specialist.json").read_text())
    except (OSError, ValueError):
        return []
    context = specialist_context(repo)
    return [evidence[o["id"]] for o in plan["obligations"] if evidence.get(o["id"], {}).get("status") == "passed"
            and evidence[o["id"]].get("context") == context and evidence[o["id"]].get("check") == o]


def run_verification(repo, plan, output, dotnet=None, reuse=True, configuration="Debug", framework="net10.0", timeout=1800, announce=None, tests_only=False, preparation_seconds=0):
    announce = announce or (lambda message: None)
    dotnet = dotnet or ["dotnet"]
    output = Path(output)
    started = time.monotonic()
    report = {"version": 1, "status": "blocked", "time_utc": datetime.now(timezone.utc).isoformat(), "plan": plan, "test_results": [], "timings": {"preparation": round(preparation_seconds, 3)}, "errors": [], "reused": False}
    current_outputs = lambda: output_fingerprint(repo.root, configuration, framework, output_manifest(repo, plan["builds"], configuration)) if plan["builds"] else {"files": 0, "sha256": "no-dotnet-build"}
    with RunLock(output):
        try:
            if plan["gaps"]:
                report["errors"] = plan["gaps"]
                return report
            if not plan["changes"] and not plan.get("requested_tests") and plan["scope"] != "full":
                report["status"] = "no-changes"
                return report
            snapshot = repo.snapshot()
            if snapshot != plan.get("source_fingerprint", snapshot):
                raise ValueError("Plan is stale: repository inputs changed after impact discovery")
            info = subprocess.check_output(dotnet + ["--info"], cwd=repo.root, stderr=subprocess.STDOUT, text=True, timeout=30) if plan["builds"] else "No .NET checks"
            # Values are hashed, never persisted: environment can contain credentials.
            environment = fingerprint(dict(os.environ))
            identity = {"source": snapshot, "toolchain": info, "python": sys.version, "platform": platform.platform(), "environment": environment, "configuration": configuration, "framework": framework, "plan": plan}
            key = fingerprint(identity)
            receipt_path = output / "receipt.json"
            report["input_fingerprint"] = key
            report["specialist_results"] = matching_specialist_checks(repo, plan, output)
            satisfied = {r["check"]["id"] for r in report["specialist_results"]}
            report["remaining_obligations"] = [o for o in plan["obligations"] if o["id"] not in satisfied]

            def completion(reused=False):
                if tests_only:
                    return "tests-passed"
                if report["remaining_obligations"]:
                    return "pending"
                if plan["scope"] == "iterate":
                    return "iteration-passed"
                return "reused" if reused else "passed"

            if reuse and receipt_path.is_file():
                try:
                    receipt = json.loads(receipt_path.read_text())
                except (ValueError, OSError):
                    receipt = {}
                if receipt.get("input_fingerprint") == key and receipt.get("outputs") == current_outputs():
                    if repo.snapshot() != snapshot:
                        raise ValueError("Repository inputs changed during receipt validation")
                    report.update(status=completion(True), reused=True, test_results=receipt["test_results"], receipt_time_utc=receipt["time_utc"])
                    return report
            with tempfile.TemporaryDirectory(prefix="atomui-verification-") as temporary:
                temporary = Path(temporary)
                report["temporary_directory"] = str(temporary)
                if plan["tooling"]:
                    command = [sys.executable, "-B", "-m", "unittest", "discover", "-s", str(repo.root / "scripts/verification/tests"), "-v"]
                    report["timings"]["tooling"] = execute(command, repo.root, temporary / "tooling.log", timeout, announce)
                if plan["builds"]:
                    solution = temporary / "Affected.slnx"
                    write_solution(solution, repo, plan["builds"])
                    command = dotnet + ["build", str(solution), "--configuration", configuration, "--nologo", "--verbosity", "minimal", "-m:1", "-nr:false"]
                    report["timings"]["build_and_restore"] = execute(command, repo.root, temporary / "build.log", timeout, announce)
                built_outputs = current_outputs()
                for index, job in enumerate(plan["tests"]):
                    directory = temporary / ("results-" + str(index))
                    settings = temporary / ("tests-" + str(index) + ".runsettings")
                    write_settings(settings, job["classes"])
                    command = dotnet + ["test", str(repo.root / job["project"]), "--configuration", configuration, "--framework", framework, "--no-build", "--no-restore", "--nologo", "--settings", str(settings), "--logger", "trx", "--results-directory", str(directory), "-m:1", "-nr:false"]
                    duration = execute(command, repo.root, temporary / ("tests-" + str(index) + ".log"), timeout, announce)
                    evidence = read_trx(directory, job["classes"])
                    evidence.update(project=job["project"], seconds=duration)
                    report["test_results"].append(evidence)
                    announce(str(evidence["passed"]) + " tests passed: " + job["project"] + " (" + str(duration) + "s)")
                if repo.snapshot() != snapshot:
                    raise ValueError("Repository inputs changed during verification; results cannot certify the current snapshot")
                outputs = current_outputs()
                if outputs != built_outputs:
                    raise ValueError("Build outputs changed during test execution; untested replacement bytes cannot receive a passing receipt")
                if plan["builds"] and not outputs["files"]:
                    raise ValueError("No build outputs found; cannot certify or cache this build configuration")
                report["status"] = completion()
                # A receipt stores completed test evidence, never waives specialist obligations.
                receipt = {"version": 1, "status": "tests-passed", "input_fingerprint": key, "outputs": outputs, "time_utc": report["time_utc"], "test_results": report["test_results"]}
                write_json(receipt_path, receipt)
        except (OSError, ValueError, RuntimeError, TimeoutError, subprocess.SubprocessError, ET.ParseError) as error:
            report["status"] = "failed"
            report["errors"].append(str(error))
        finally:
            report["timings"]["total"] = round(preparation_seconds + time.monotonic() - started, 3)
            write_json(output / "latest.json", report)
            # Bounded compact history supports latency tracking without accumulating raw logs.
            history_path = output / "history.json"
            try:
                history = json.loads(history_path.read_text()) if history_path.exists() else []
            except (OSError, ValueError):
                history = []
            history.append({"time_utc": report["time_utc"], "status": report["status"], "scope": plan["scope"], "test_projects": len(plan["tests"]), "selected_classes": sum(len(j["classes"]) for j in plan["tests"]), "passed": sum(j["passed"] for j in report["test_results"]), "timings": report["timings"]})
            write_json(history_path, history[-100:])
    return report
