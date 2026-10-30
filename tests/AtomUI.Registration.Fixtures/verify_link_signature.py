#!/usr/bin/env python3
"""Exercise the real publish preparation target, with controlled effective ILLink inputs.
Requires the supported SDK/WASM workload. Output is owned by this invocation; no prior app artifacts needed.
"""
import argparse
import json
import subprocess
import tempfile
import shutil
from pathlib import Path
from xml.sax.saxutils import escape

ROOT = Path(__file__).resolve().parents[2]
SCALARS = {
    "TrimMode": "TrimMode", "DefaultAction": "_TrimmerDefaultAction", "RemoveSymbols": "TrimmerRemoveSymbols",
    "PreserveSymbolPaths": "_TrimmerPreserveSymbolPaths", "BeforeFieldInit": "_TrimmerBeforeFieldInit",
    "OverrideRemoval": "_TrimmerOverrideRemoval", "UnreachableBodies": "_TrimmerUnreachableBodies",
    "UnusedInterfaces": "_TrimmerUnusedInterfaces", "IPConstProp": "_TrimmerIPConstProp", "Sealer": "_TrimmerSealer",
    "Warn": "ILLinkWarningLevel", "NoWarn": "NoWarn", "TreatWarningsAsErrors": "ILLinkTreatWarningsAsErrors",
    "WarningsAsErrors": "WarningsAsErrors", "WarningsNotAsErrors": "WarningsNotAsErrors", "SingleWarn": "TrimmerSingleWarn",
    "DumpDependencies": "_TrimmerDumpDependencies", "DependenciesFileFormat": "_TrimmerDependenciesFileFormat",
    "ExtraArgs": "_ExtraTrimmerArgs", "ToolExe": "_DotNetHostFileName", "ToolPath": "_DotNetHostDirectory",
}
ASSEMBLY_METADATA = ["TrimMode", "IsTrimmable", "BeforeFieldInit", "OverrideRemoval", "UnreachableBodies",
                     "UnusedInterfaces", "IPConstProp", "Sealer", "TrimmerSingleWarn"]

def run(args, log):
    completed = subprocess.run(args, cwd=ROOT, text=True, stdout=subprocess.PIPE, stderr=subprocess.STDOUT)
    with log.open("a") as stream:
        stream.write("$ " + " ".join(map(str, args)) + "\n" + completed.stdout)
    if completed.returncode:
        raise RuntimeError(f"Command failed ({completed.returncode}); see {log}")
    return completed.stdout

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output", type=Path, required=True)
    options = parser.parse_args()
    output = options.output.resolve(); output.mkdir(parents=True, exist_ok=True)
    log = output / "signature-target.log"
    run(["dotnet", "build", "src/AtomUI.Toolchain/AtomUI.Toolchain.csproj", "-t:BuildManagedToolchain", "-c", "Release"], log)
    browser = "tests/AtomUI.Registration.Fixtures/Browser/Browser.csproj"
    run(["dotnet", "restore", browser], log)
    names = "AtomUIBuildTasksAssembly,ILLinkTasksAssembly,WasmAppBuilderTasksAssemblyPath,NETCoreSdkVersion,DOTNET_HOST_PATH"
    props = json.loads(run(["dotnet", "msbuild", browser, "-p:Configuration=Release", "-p:RuntimeIdentifier=browser-wasm", "-getProperty:" + names], log))["Properties"]
    backend = ROOT / ".artifacts/bin/Release/toolchain/illink10/net10.0/AtomUI.TypeMap.Linker.dll"
    results = []
    with tempfile.TemporaryDirectory(prefix="atomui-link-signature-") as temporary:
        directory = Path(temporary)
        worker_copy = directory / "worker"
        shutil.copytree(Path(props["AtomUIBuildTasksAssembly"]).parent, worker_copy)
        props["AtomUIBuildTasksAssembly"] = str(worker_copy / "AtomUI.Build.Tasks.dll")
        backend_copy = directory / "backend"; backend_copy.mkdir()
        for name in ["AtomUI.TypeMap.Linker.dll", "AtomUI.TypeMap.Linker.deps.json"]:
            shutil.copy2(backend.parent / name, backend_copy / name)
        backend = backend_copy / "AtomUI.TypeMap.Linker.dll"
        descriptor = directory / "roots.xml"; descriptor.write_text("<linker />")
        substitution = directory / "substitution with spaces.xml"; substitution.write_text("<linker />")
        itemfile = directory / "input.dll"; itemfile.write_bytes(Path(props["AtomUIBuildTasksAssembly"]).read_bytes())
        xml = ['<Project><PropertyGroup>']
        for key, value in props.items(): xml.append(f"<{key}>{escape(value)}</{key}>")
        fixed = {"RuntimeIdentifier": "browser-wasm", "TargetFramework": "net10.0-browser", "PublishTrimmed": "true",
                 "OutputType": "Exe", "TrimMode": "full", "IntermediateOutputPath": str(directory) + "/", "IntermediateLinkDir": str(directory / "linked"),
                 "_LinkSemaphore": str(directory / "Link.semaphore"), "ProbeRootIdentity": "Root.Before", "ProbeRootMode": "All"}
        for key, value in fixed.items(): xml.append(f"<{key}>{escape(value)}</{key}>")
        xml.append('<_ExtraTrimmerArgs Condition="\'$(ProbeSubstitution)\' != \'\'">--substitutions &quot;$(ProbeSubstitution)&quot;</_ExtraTrimmerArgs>')
        xml.append('</PropertyGroup><ItemGroup>')
        xml.append(f'<AtomUIRegistrationToolCandidate Include="{escape(str(backend))}" Kind="Backend"/>')
        xml.append(f'<ManagedAssemblyToLink Include="{escape(str(itemfile))}">')
        for name in ASSEMBLY_METADATA: xml.append(f'<{name}>$(Assembly{name})</{name}>')
        xml.append('</ManagedAssemblyToLink>')
        xml.extend(['<TrimmerRootAssembly Include="$(ProbeRootIdentity)" RootMode="$(ProbeRootMode)"/>',
                    '<TrimmerRootDescriptor Include="$(ProbeDescriptor)" Condition="\'$(ProbeDescriptor)\' != \'\'"/>',
                    '<_TrimmerKeepMetadata Include="$(ProbeKeepMetadata)" Condition="\'$(ProbeKeepMetadata)\' != \'\'"/>',
                    '<_TrimmerFeatureSettings Include="Fixture.Feature" Value="$(ProbeFeature)" Condition="\'$(ProbeFeature)\' != \'\'"/>',
                    '<_TrimmerCustomData Include="Fixture.Data" Value="$(ProbeCustomData)" Condition="\'$(ProbeCustomData)\' != \'\'"/>',
                    f'<_TrimmerCustomSteps Include="{escape(str(backend))}" Type="$(ProbeStepType)" BeforeStep="$(ProbeStepBefore)" AfterStep="$(ProbeStepAfter)" Condition="\'$(ProbeStepType)\' != \'\'"/>'])
        xml.append(f'</ItemGroup><Import Project="{escape(str(ROOT / "build/AtomUI.Registration.targets"))}"/></Project>')
        project = directory / "Probe.proj"; project.write_text("".join(xml))
        signature = directory / "AtomUI.TypeMap.inputs.json"; receipt = directory / "AtomUI.TypeMap.receipt.json"; semaphore = directory / "Link.semaphore"
        def prepare(overrides):
            run(["dotnet", "msbuild", str(project), "-t:AtomUIPrepareTypeMapBackend", "-nologo"] + [f"-p:{k}={v}" for k,v in overrides.items()], log)
        def check(name, changed, base=None, mutate=None):
            prepare(base or {})
            receipt.write_text("prior-success"); semaphore.write_text("prior-success")
            before = signature.read_bytes(); stamp = signature.stat().st_mtime_ns
            prepare(base or {})
            assert signature.read_bytes() == before and signature.stat().st_mtime_ns == stamp and receipt.exists() and semaphore.exists(), "no-op: " + name
            if mutate: mutate()
            prepare(changed)
            invalidated = signature.read_bytes() != before and not receipt.exists() and not semaphore.exists()
            results.append({"case": name, "invalidated": invalidated})
            print(name, "PASS" if invalidated else "FAIL", flush=True)
        for name, property_name in SCALARS.items():
            if name == "TrimMode": continue # The product gate only permits full; per-assembly action is tested below.
            value = {"DefaultAction":"copy", "Warn":"3", "NoWarn":"IL2026", "WarningsAsErrors":"IL2026", "WarningsNotAsErrors":"IL2026",
                     "DependenciesFileFormat":"dgml", "ExtraArgs":"--verbose", "ToolExe":Path(props["DOTNET_HOST_PATH"]).name,
                     "ToolPath":str(Path(props["DOTNET_HOST_PATH"]).parent)}.get(name, "true")
            check(name, {property_name: value})
        for name in ASSEMBLY_METADATA:
            check("assembly:" + name, {"Assembly" + name: "copy" if name == "TrimMode" else "true"})
        for name, property_name, value in [("root", "ProbeRootIdentity", "Root.After"), ("root-mode", "ProbeRootMode", "Visible"),
                                           ("metadata", "ProbeKeepMetadata", "parametername"), ("feature", "ProbeFeature", "true"), ("custom-data", "ProbeCustomData", "value")]:
            check(name, {property_name:value})
        descriptor_args = {"ProbeDescriptor": str(descriptor)}
        check("descriptor-content", descriptor_args, descriptor_args, lambda: descriptor.write_text("<linker><!--changed--></linker>"))
        substitution_args = {"ProbeSubstitution": str(substitution)}
        check("extra-substitution-content", substitution_args, substitution_args, lambda: substitution.write_text("<linker><!--changed--></linker>"))
        for property_name, value in [("ProbeStepType", "Fixture.Other"), ("ProbeStepBefore", "MarkStep"), ("ProbeStepAfter", "SweepStep")]:
            base_step = {"ProbeStepType": "Fixture.Step"}
            check(property_name, {**base_step, property_name:value}, base_step)
        pdb = itemfile.with_suffix(".pdb"); pdb.write_bytes(b"initial-symbols")
        check("input-symbol-content", {}, mutate=lambda: pdb.write_bytes(b"changed-symbols"))
        check("implementation-content", {}, mutate=lambda: itemfile.write_bytes(itemfile.read_bytes() + b"probe"))
    (output / "signature-target-results.json").write_text(json.dumps(results, indent=2))
    assert all(r["invalidated"] for r in results), "Effective link inputs did not invalidate prior evidence"

if __name__ == "__main__": main()
