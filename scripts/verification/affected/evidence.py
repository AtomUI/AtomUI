"""Verification evidence is derived from results and bytes, not exit code alone."""
import hashlib
import json
import xml.etree.ElementTree as ET
from pathlib import Path


def fingerprint(value):
    return hashlib.sha256(json.dumps(value, sort_keys=True, ensure_ascii=True).encode()).hexdigest()


def output_manifest(repository, projects, configuration="Debug"):
    """Selected project graph + runtime assets; integration fixture outputs are separate."""
    root = repository.root / ".artifacts/bin" / configuration
    graph, queue = set(), list(projects)
    while queue:
        project = queue.pop()
        if project not in graph:
            graph.add(project)
            queue.extend(repository.projects.get(project, {}).get("references", []))
    names = set()
    for project in graph:
        names.add(Path(project).stem)
        if (repository.root / project).is_file():
            tree = ET.fromstring(repository.text(project))
            names.update(x.text for x in tree.iter("AssemblyName") if x.text and "$" not in x.text)
    files = set()
    if root.exists():
        for directory in (p for p in root.iterdir() if p.is_dir()):
            for name in names:
                for suffix in (".dll", ".pdb", ".deps.json", ".runtimeconfig.json", ".runtimeconfig.dev.json", ".exe", ""):
                    path = directory / (name + suffix)
                    if path.is_file():
                        files.add(path)
            for path in directory.glob("*"):
                if path.is_file() and path.name.startswith(("testhost.", "Microsoft.TestPlatform.", "xunit.runner.", "xunit.v3.")):
                    files.add(path)
        for path in list(files):
            if path.name.endswith(".deps.json"):
                deps = json.loads(path.read_text())
                for target in deps.get("targets", {}).values():
                    for library in target.values():
                        for section in ("runtime", "native", "resources", "runtimeTargets"):
                            for asset in library.get(section, {}):
                                candidates = [path.parent / asset, path.parent / Path(asset).name]
                                if section == "resources":
                                    candidates.append(path.parent / Path(asset).parent.name / Path(asset).name)
                                files.update(p for p in candidates if p.is_file())
    return sorted(p.relative_to(root).as_posix() for p in files)


def output_fingerprint(root, configuration="Debug", framework="net10.0", manifest=None):
    # Include generator/netstandard and all configured runtime outputs as well as the test TFM.
    output = Path(root) / ".artifacts" / "bin" / configuration
    digest = hashlib.sha256()
    count = 0
    if manifest is None:
        manifest = sorted(p.relative_to(output).as_posix() for p in output.rglob("*") if p.is_file()) if output.is_dir() else []
    for name in manifest:
        path = output / name
        digest.update(name.encode() + b"\0")
        if path.is_file():
            count += 1
            with path.open("rb") as stream:
                for chunk in iter(lambda: stream.read(1024 * 1024), b""):
                    digest.update(chunk)
        else:
            digest.update(b"<missing>")
    return {"files": count, "sha256": digest.hexdigest(), "manifest": manifest}


def read_trx(directory, expected_classes):
    files = list(Path(directory).rglob("*.trx"))
    if not files:
        raise ValueError("Test process produced no TRX evidence")
    total = passed = 0
    observed = set()
    for path in files:
        root = ET.parse(path).getroot()
        for item in root.iter():
            item.tag = item.tag.rsplit("}", 1)[-1]
        counters = root.find(".//Counters")
        if counters is None:
            raise ValueError("TRX contains no result counters")
        values = {name: int(counters.get(name, "0")) for name in ("total", "executed", "passed", "failed", "error", "timeout", "aborted", "inconclusive", "notExecuted")}
        if values["total"] == 0 or values["passed"] != values["total"] or values["executed"] != values["total"] or any(values[n] for n in ("failed", "error", "timeout", "aborted", "inconclusive", "notExecuted")):
            raise ValueError("Incomplete or failed test evidence: " + json.dumps(values, sort_keys=True))
        definitions = {}
        for definition in root.findall(".//UnitTest"):
            method = definition.find("TestMethod")
            if method is not None:
                definitions[definition.get("id")] = method.get("className", "").split(",", 1)[0]
        results = root.findall(".//UnitTestResult")
        if len(results) != values["total"]:
            raise ValueError("TRX counters do not agree with individual test results")
        for result in results:
            if result.get("outcome") != "Passed":
                raise ValueError("Selected test did not pass: " + str(result.get("testName", result.get("testId"))))
            observed.add(definitions.get(result.get("testId"), ""))
        total += values["total"]
        passed += values["passed"]
    missing = sorted(set(expected_classes) - observed)
    if missing:
        raise ValueError("Selected classes produced no passing evidence: " + ", ".join(missing))
    return {"total": total, "passed": passed, "classes": sorted(observed)}


def write_json(path, value):
    path = Path(path)
    path.parent.mkdir(parents=True, exist_ok=True)
    temporary = path.with_suffix(".tmp")
    temporary.write_text(json.dumps(value, indent=2, sort_keys=True) + "\n")
    temporary.replace(path)
