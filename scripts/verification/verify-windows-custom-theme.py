#!/usr/bin/env python3
"""Actual Windows x64 specialist. No manual acknowledgement or imported receipts."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import platform
import re
import stat
import subprocess
import sys
import tempfile
import time

sys.dont_write_bytecode = True
from affected.repository import Repository
from affected.runner import specialist_context

ROOT = Path(__file__).resolve().parents[2]
FIXTURES = ROOT / "tests/AtomUI.Registration.Fixtures"
OUTPUT = ROOT / ".artifacts/verification/windows-custom-theme"
OWNER = "AtomUI.WindowsCustomTheme"


def _is_redirected(path):
    try:
        metadata = path.lstat()
        return (stat.S_ISLNK(metadata.st_mode) or
                (os.name == "nt" and bool(getattr(metadata, "st_file_attributes", 0) & stat.FILE_ATTRIBUTE_REPARSE_POINT)))
    except (OSError, ValueError):
        return True


def retire_previous_root(previous, current):
    """Bound retained payloads to the latest run per check; never force-delete Git caches."""
    value = previous.get("ownedRoot")
    if not isinstance(value, str) or not value:
        return
    requested = Path(value)
    if _is_redirected(requested):
        return
    candidate = requested.resolve()
    marker = candidate / ".windows-custom-theme-owned"
    if (candidate == current or candidate.parent != Path(tempfile.gettempdir()).resolve() or
            not candidate.name.startswith("aw-") or candidate.is_symlink() or not marker.is_file() or
            marker.read_text(encoding="utf-8") != OWNER):
        return
    # Files with read-only attributes and .git data are intentionally left alone.
    # Ordinary owned outputs need no privileged/forced recursive operation.
    traversal_root = candidate
    if os.name == "nt":
        value = str(candidate)
        if not value.startswith("\\\\?\\"):
            value = "\\\\?\\" + ("UNC\\" + value[2:] if value.startswith("\\\\") else value)
        traversal_root = Path(value)
    traversal_marker = traversal_root / marker.name
    directories = []
    for directory, children, files in os.walk(traversal_root):
        children[:] = [name for name in children if name != ".git" and
                       not _is_redirected(Path(directory) / name)]
        directories.append(Path(directory))
        for name in files:
            path = Path(directory) / name
            if path == traversal_marker or name == ".git" or _is_redirected(path):
                continue
            try:
                if os.name == "nt" and getattr(path.stat(), "st_file_attributes", 0) & stat.FILE_ATTRIBUTE_READONLY:
                    continue
                path.unlink()
            except OSError:
                pass
    for directory in reversed(directories):
        if directory == traversal_root:
            continue
        try:
            directory.rmdir()
        except OSError:
            pass
    remaining = [str(path.relative_to(candidate)) for path in candidate.iterdir() if path != marker]
    if remaining:
        print("Retained protected paths under " + str(candidate) + ": " + ", ".join(remaining), flush=True)
    else:
        marker.unlink()
        candidate.rmdir()


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def verify_scene(directory, native):
    directory = Path(directory)
    result = json.loads((directory / "result.json").read_text(encoding="utf-8"))
    expected = 1186 if native else 1185
    if result["DynamicCode"] == native or len(result["Checks"]) != expected:
        raise ValueError("Wrong execution mode or incomplete scene assertions")
    if not all(check["Pass"] for check in result["Checks"]):
        raise ValueError("Scene contains failed assertions")
    if len(result["Stages"]) != 12 or any(len(stage["Frames"]) != 3 for stage in result["Stages"]):
        raise ValueError("Incomplete twelve-stage / thirty-six-frame protocol")
    frames = [directory / name for stage in result["Stages"] for name in stage["Frames"]]
    if len(set(frames)) != 36 or any(path.parent != directory or not path.is_file() for path in frames):
        raise ValueError("Missing or invalid complete-window frame")
    return frames


def compare_frames(managed, native):
    left = verify_scene(managed, False)
    right = verify_scene(native, True)
    if [p.name for p in left] != [p.name for p in right]:
        raise ValueError("Paired frame identities differ")
    if any(a.read_bytes() != b.read_bytes() for a, b in zip(left, right)):
        raise ValueError("Full HWND managed/native pixel difference")
    return {a.name: sha(a) for a in left}


class Verification:
    def __init__(self):
        OUTPUT.mkdir(parents=True, exist_ok=True)
        self.context = specialist_context(Repository(ROOT))
        self.env = dict(os.environ, AtomUIRegistrationPython=sys.executable,
                        DOTNET_CLI_UI_LANGUAGE="en-US", PYTHONUTF8="1")
        for name in ("WIN_G1_ONLY", "WIN_INPUT_ONLY", "WIN31_RUN_DIR", "WIN31_BACKDROP"):
            self.env.pop(name, None)
        self.commands = []
        self.files = {}
        self.root = Path(tempfile.mkdtemp(prefix="aw-"))
        (self.root / ".windows-custom-theme-owned").write_text(OWNER, encoding="utf-8")

    def run(self, label, command, *, extra=None, cwd=ROOT, timeout=3600, marker=None, expected_exit=0):
        log = self.root / (label + ".log")
        start = time.monotonic()
        with log.open("w", encoding="utf-8") as stream:
            arguments = [str(x) for x in command]
            environment = dict(self.env, **(extra or {}))
            if label.rsplit("-", 1)[-1] in ("g1", "ime", "native31", "managed31"):
                from windows_desktop import run_prepared
                preparation = self.root / (label + "-desktop")
                result = run_prepared(arguments, cwd, environment, stream, timeout, preparation)
                for path in preparation.iterdir():
                    self.remember(path)
            else:
                result = subprocess.run(arguments, cwd=cwd, env=environment, stdout=stream,
                                        stderr=subprocess.STDOUT, timeout=timeout)
        content = log.read_text(encoding="utf-8", errors="replace")
        self.commands.append(dict(label=label, exit=result.returncode,
                                  seconds=round(time.monotonic() - start, 3), logSha256=sha(log)))
        self.files[str(log)] = sha(log)
        if result.returncode != expected_exit or (marker and marker not in content):
            print(content[-7000:], flush=True)
            raise RuntimeError(label + " failed; " + str(log))
        print(label + " PASS", flush=True)
        return content

    def remember(self, path):
        self.files[str(path)] = sha(path)

    def platform(self):
        pairs = {}
        project = FIXTURES / "WindowsTheme/WindowsTheme.csproj"
        for tfm in ("net8.0", "net10.0"):
            prefix = self.root / tfm
            native = prefix / "publish"
            self.run(tfm + "-publish", ["dotnet", "publish", project, "-c", "Release", "-f", tfm,
                     "-r", "win-x64", "--self-contained", "true", "-p:PublishAot=true",
                     "-p:WindowsThemeOutputRoot=" + str(prefix / "native"),
                     "-p:PublishDir=" + str(native) + os.sep])
            exe = native / "Win31.exe"
            self.remember(exe)
            for scenario, variable, marker in (
                ("g1", "WIN_G1_ONLY", "G1_PASS checks=135 roots=16 themes=2 sizes=4"),
                ("ime", "WIN_INPUT_ONLY", "INPUT_PASS checks=31 editors=3")):
                self.run(tfm + "-" + scenario, [exe], extra={variable: "1"}, marker=marker, timeout=180)
            native_scene = prefix / "native-scene"
            self.run(tfm + "-native31", [exe], extra={"WIN31_BACKDROP": "1", "WIN31_RUN_DIR": str(native_scene)},
                     marker="SUMMARY checks=1186 pass=1186 fail=0", timeout=240)
            managed_root = prefix / "managed"
            self.run(tfm + "-build", ["dotnet", "build", project, "-c", "Release", "-f", tfm,
                     "-r", "win-x64", "-p:PublishAot=false", "-p:WindowsThemeOutputRoot=" + str(managed_root)])
            managed_scene = prefix / "managed-scene"
            dll = managed_root / "bin/Release" / tfm / "win-x64/Win31.dll"
            self.run(tfm + "-managed31", ["dotnet", dll],
                     extra={"WIN31_BACKDROP": "1", "WIN31_RUN_DIR": str(managed_scene)},
                     marker="SUMMARY checks=1185 pass=1185 fail=0", timeout=240)
            pairs[tfm] = compare_frames(managed_scene, native_scene)
            for directory in (managed_scene, native_scene):
                self.remember(directory / "result.json")
                for frame in directory.glob("*.bmp"):
                    self.remember(frame)
        if specialist_context(Repository(ROOT)) != self.context:
            raise ValueError("Inputs changed during platform execution")
        return pairs

    def packages(self):
        cold = self.root / "cold"
        self.run("cold-packages", [sys.executable, "-B", FIXTURES / "run_package_consumers.py",
                 "--rid", "win-x64", "--desktop-only", "--output", cold],
                 marker="PACKAGE_DESKTOP_CONSUMERS_PASS", timeout=7200)
        from windows_packages import verify_layout
        verify_layout(self, cold)
        for path in (cold / "feed").glob("*.nupkg"):
            self.remember(path)
        self.remember(cold / "results.json")
        self.remember(cold / "package-inventory.json")
        if specialist_context(Repository(ROOT)) != self.context:
            raise ValueError("Inputs changed during package execution")
        return cold

    def gallery(self):
        text = self.run("gallery-official-publish", ["pwsh", "-NoLogo", "-NoProfile", "-File",
                 ROOT / "controlgallery/AtomUIGallery.Desktop/scripts/PublishToLocal.ps1",
                 "-publishRootPath", self.root / "gallery-official", "-runtime", "win-x64",
                 "-buildType", "Release", "-publishAot", "true"])
        if any(code in text for code in ("warning IL", "warning NU1903")):
            raise ValueError("Official Gallery publish has trimming/AOT/security diagnostics")
        receipt = self.root / "official-gallery-startup.json"
        self.run("gallery-official-startup", ["pwsh", "-NoLogo", "-NoProfile", "-File",
                 ROOT / "scripts/verification/launch-windows-gallery.ps1",
                 "-PublishRoot", self.root / "gallery-official", "-Receipt", receipt],
                 marker="OFFICIAL_GALLERY_STARTUP_PASS", timeout=60)
        self.remember(receipt)
        # The route host derives from the shipping GalleryApplication and uses its
        # exact configuration. All factories are checked in a real native HWND.
        destination = self.root / "gallery-routes"
        text = self.run("gallery-routes-publish", ["dotnet", "publish",
                        FIXTURES / "WindowsGallery/WindowsGallery.csproj", "-c", "Release", "-r", "win-x64",
                        "--self-contained", "true", "-p:GalleryPublishAot=true",
                        "-p:WindowsGalleryOutputRoot=" + str(self.root / "gallery-build"),
                        "-p:PublishDir=" + str(destination) + os.sep])
        if any(code in text for code in ("warning IL", "warning NU1903")):
            raise ValueError("Gallery still reports trimming/AOT/security diagnostics")
        exe = destination / "WindowsGallery.exe"
        self.remember(exe)
        text = self.run("gallery-routes", [exe], extra={"GALLERY_TYPES": str(self.root / "gallery-types.txt")},
                        marker="GALLERY_PASS routes=78 checks=316", timeout=240)
        if len(re.findall(r"^GALLERY_PASS routes=78 checks=316 actualControlTypes=[1-9][0-9]* "
                          r"dynamic=False framework=\.NET 10\.0\.14\s*$", text, re.MULTILINE)) != 1:
            raise ValueError("Gallery route proof must be NativeAOT on the approved .NET 10.0.14 runtime")

    def finish(self, check, details):
        if specialist_context(Repository(ROOT)) != self.context:
            raise ValueError("Source/environment changed during Windows verification")
        report = dict(check=check, context=self.context, status="passed", details=details,
                      commands=self.commands, files=self.files, ownedRoot=str(self.root))
        self.save_report(check, report)
        print("WINDOWS_SPECIALIST_PASS " + check, flush=True)

    def save_report(self, check, report):
        destination = OUTPUT / (check + ".json")
        try:
            previous = json.loads(destination.read_text(encoding="utf-8")) if destination.is_file() else {}
            if not isinstance(previous, dict):
                previous = {}
        except (OSError, ValueError):
            previous = {}
        destination.write_text(json.dumps(report, indent=2), encoding="utf-8")
        retire_previous_root(previous, self.root)

    def fail(self, check, exception):
        self.save_report(check, dict(check=check, context=self.context, status="failed", error=str(exception),
                                    commands=self.commands, files=self.files, ownedRoot=str(self.root)))


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--check", choices=("platform-interaction", "native-aot", "package-layout",
                                          "conditional-registration-engines"), required=True)
    args = parser.parse_args()
    if os.name != "nt" or platform.machine().lower() not in ("amd64", "x86_64"):
        raise RuntimeError("This verifier requires an interactive Windows x64 host")
    if args.check in ("platform-interaction", "native-aot"):
        unsupported = [path for path in Repository(ROOT).changes()
                       if path.startswith(("controlgallery/AtomUIGallery.Browser/",
                                           "src/AtomUI.Native/MacOS/", "src/AtomUI.Native/Linux/"))
                       or path == ".github/workflows/release-gallery.yml"]
        if unsupported:
            raise ValueError("Windows desktop evidence cannot certify these changed hosts/workflows: " +
                             ", ".join(unsupported))
    verification = Verification()
    try:
        run_check(args.check, verification)
    except Exception as exception:
        verification.fail(args.check, exception)
        raise


def run_check(check, verification):
    if check in ("platform-interaction", "native-aot"):
        details = verification.platform()
        if check == "native-aot":
            verification.gallery()
        verification.finish(check, details)
    else:
        from windows_packages import conditional_routes
        cold = verification.packages()
        details = {"coldDesktop": True, "releasedLayout": "6.2.8", "workerIsolation": True}
        if check == "conditional-registration-engines":
            details["routes"] = conditional_routes(verification, cold)
            source = cold / "source"
            compiler = source / ".artifacts/tools/registration/net8/ilc/managed-bundle"
            tasks = source / ".artifacts/bin/Release/toolchain/tasks/net10.0/AtomUI.Build.Tasks.dll"
            matrix = verification.root / "matrix"
            verification.run("native8-full-matrix", [sys.executable, "-B",
                             source / "src/AtomUI.Toolchain/Backends/ILC8/verification/verify_backend.py",
                             "--compiler-output", compiler, "--tasks-assembly", tasks,
                             "--rids", "win-x64", "--work-root", matrix],
                             marker="PASS NativeAOT mechanism matrix 46", timeout=3600)
            rows = json.loads((matrix / "logs/matrix-results.json").read_text(encoding="utf-8"))
            if len(rows) != 46 or any(row["runtimeExit"] for row in rows):
                raise ValueError("Incomplete actual Native8 matrix")
            details["matrix"] = rows
        verification.finish(check, details)


if __name__ == "__main__":
    main()
