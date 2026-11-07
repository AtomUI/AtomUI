"""Windows-only actual cold-package/bridge protocols; invoked by the specialist."""
import hashlib
import json
import os
from pathlib import Path
import subprocess
import struct
import zipfile
from xml.sax.saxutils import escape


PRODUCT_CHECKS = (
    "native window and title bar resource classes execute",
    "Button template executes",
    "ThemeManager initialized",
    "Common commits before Desktop",
    "local boxed-enum own Token override wins",
    "local boxed-enum own Token update republishes",
    "removing local own Token restores scoped value",
    "Expander string template executes",
    "Expander nonstring content executes",
    "cross-package ResourceInclude executes",
    "trimmed TextArea private ScrollViewer adapter executes",
    "trimmed Popup private parent adapter executes",
    "OTP resource class creates internal text box template",
    "NumericUpDown typed ButtonSpinner base theme executes",
    "NumericUpDown inherits application ButtonSpinner theme replacement",
    "NumericUpDown local customization survives inherited theme",
    "third-party package enabled",
    "token-only and typed-identity-only registration",
    "own-enum-only, TokenKey-only and extension-only registration",
    "boxed-enum-only own resource materializes",
    "token-only own resource materializes",
    "semantic-only metadata preserved",
    "dark theme commits",
    "scoped token update reaches template",
    "Registry frozen after startup",
    "selected registration runtime IsTrimmed true",
    "complete selected JSON writes identity",
    "complete selected JSON reads against actual schema requirements",
    "complete selected JSON retains selected Button override",
    "complete selected JSON write read roundtrip",
    "complete selected JSON applies",
    "complete JSON selected recipe commits identity",
    "theme update retains input text",
    "theme update retains input selection",
    "closed native window releases its theme context",
)


def verify_product_protocol(text, runtime):
    checks = tuple(line.removeprefix("PRODUCT_CHECK ") for line in text.splitlines()
                   if line.startswith("PRODUCT_CHECK "))
    runtimes = tuple(line for line in text.splitlines() if line.startswith("PRODUCT_RUNTIME "))
    if checks != PRODUCT_CHECKS or runtimes != ("PRODUCT_RUNTIME .NET " + runtime,):
        raise ValueError("Incomplete/wrong-runtime selected product protocol")


DELIVERY_STAGES = ("sdk-link-verified", "published-copy-verified", "linked-output", "published-output")


def require_no_delivery(root, destination):
    if (destination / "AtomUI.Registration.Fixtures.exe").exists():
        raise ValueError("Rejected input left a published executable")
    for path in root.rglob("*.json"):
        try:
            item = json.loads(read_bytes(path).decode("utf-8-sig"))
        except (ValueError, UnicodeError):
            continue
        if isinstance(item, dict) and item.get("stage") in DELIVERY_STAGES:
            raise ValueError("Rejected input left a successful delivery receipt: " + str(path))


def read_bytes(path):
    path = Path(path).absolute()
    value = str(path)
    if os.name == "nt" and len(value) >= 248 and not value.startswith("\\\\?\\"):
        value = "\\\\?\\" + value
    return Path(value).read_bytes()


def sha(path):
    return hashlib.sha256(read_bytes(path)).hexdigest()


def write(path, text):
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(text, encoding="utf-8")


def audit_outputs(root, destination, native, legacy, noscan=False):
    proofs = []
    for path in root.rglob("*.json"):
        try:
            item = json.loads(read_bytes(path).decode("utf-8-sig"))
        except (ValueError, UnicodeError):
            continue
        if not isinstance(item, dict):
            continue
        stage = item.get("stage")
        if stage not in DELIVERY_STAGES:
            continue
        if stage in ("linked-output", "published-output"):
            suffix = ".linked.json" if stage == "linked-output" else ".published.json"
            if not path.name.endswith(suffix):
                # The bridge's mutable phase pointer mirrors its latest immutable
                # proof. Only the immutable published proof binds the linked file.
                if read_bytes(path) != read_bytes(Path(str(path) + suffix)):
                    raise ValueError("Bridge phase pointer differs from its immutable proof")
                continue
        for key, hash_key in (("binaryPath", "binaryHash"), ("objectPath", "objectHash"),
                              ("exportsPath", "exportsHash"), ("reportPath", "reportHash"),
                              ("linkedReceiptPath", "linkedReceiptHash")):
            if key in item and sha(item[key]).lower() != item[hash_key].lower():
                raise ValueError("Changed immutable output: " + str(path))
        for output in item.get("outputs", []):
            if sha(output["path"]).lower() != output["sha256"].lower():
                raise ValueError("Changed published output: " + output["path"])
        if stage == "sdk-link-verified":
            inputs = json.loads(read_bytes(item["inputPath"]).decode("utf-8-sig"))
            report = json.loads(read_bytes(item["reportPath"]).decode("utf-8-sig"))
            if (report["Stage"] != "compiler-complete" or report["Scanner"] != (not noscan) or
                    report["InputHash"].lower() != sha(item["inputPath"]).lower() or
                    report["InvocationId"] != item["invocationId"]):
                raise ValueError("Wrong Native8 analysis mode/input/invocation")
            for entry in inputs["assemblies"] + inputs["additionalInputs"]:
                if sha(entry["path"]).lower() != entry["sha256"].lower():
                    raise ValueError("Changed frozen input: " + entry["path"])
        if stage == "published-output":
            linked = Path(str(path).replace(".published.json", ".linked.json"))
            if sha(linked).lower() != item["linkedReceiptHash"].lower():
                raise ValueError("Bridge published proof is not bound to the actual linked proof")
        proofs.append(dict(stage=stage, sha256=sha(path)))
    stages = {item["stage"] for item in proofs}
    if legacy and not stages.issuperset({"linked-output", "published-output"}):
        raise ValueError("Missing bridge immutable output proof")
    if native and not legacy and "net8" in root.name and not stages.issuperset({"sdk-link-verified", "published-copy-verified"}):
        raise ValueError("Missing Native8 final-copy proof")
    unused = (b"UnusedThirdPartyControl", b"UnusedExternalControl")
    if native:
        binary = read_bytes(destination / "AtomUI.Registration.Fixtures.exe")
        pe = struct.unpack_from("<I", binary, 0x3c)[0]
        if (binary[pe:pe + 4] != b"PE\0\0" or struct.unpack_from("<H", binary, pe + 4)[0] != 0x8664 or
                struct.unpack_from("<II", binary, pe + 24 + 112 + 14 * 8) != (0, 0)):
            raise ValueError("Actual native output is not an AMD64 PE without a CLR directory")
        maps = list(root.rglob("*.map.xml"))
        if not maps or (destination / "coreclr.dll").exists():
            raise ValueError("Missing native map or unexpected CoreCLR")
        for path in maps:
            content = read_bytes(path)
            if any(name in content for name in unused):
                raise ValueError("Unused native control retained: " + str(path))
    return proofs


def verify_layout(verification, cold):
    source, feed = cold / "source", cold / "feed"
    # An actual published baseline, downloaded independently of consumer caches.
    baseline = verification.root / "released-baseline"
    baseline.mkdir()
    import urllib.request
    for package in feed.glob("*.nupkg"):
        name = package.name.lower()
        package_id = name.removesuffix(".6.2.8.nupkg")
        if package_id == name:
            raise ValueError("Select and review the released baseline for the new version")
        url = "https://api.nuget.org/v3-flatcontainer/" + package_id + "/6.2.8/" + name
        with urllib.request.urlopen(url, timeout=90) as response:
            (baseline / name).write_bytes(response.read())
    verification.run("released-layout", ["pwsh", "-NoLogo", "-NoProfile", "-File",
                     source / "scripts/verification/verify-package-layout.ps1",
                     "-PackageDirectory", feed, "-BaselineVersion", "6.2.8", "-BaselineDirectory", baseline])
    generator = list(feed.glob("AtomUI.Generator.*.nupkg"))
    if len(generator) != 1:
        raise ValueError("Expected exactly one real Generator package")
    verification.run("worker-isolation", ["pwsh", "-NoLogo", "-NoProfile", "-File",
                     source / "scripts/verification/verify-build-task-isolation.ps1",
                     "-Configuration", "Release", "-PackageFile", generator[0]])


def conditional_routes(verification, cold):
    source, feed = cold / "source", cold / "feed"
    fixtures = source / "tests/AtomUI.Registration.Fixtures"
    root = verification.root / "packages"
    legacy = root / "legacy-feed"
    # A distinct restore graph is essential: packing a net8 DLL with the dual-TFM
    # assets file leaves an incorrect net10 nuspec group (NU5128).
    script = root / "pack8.ps1"
    write(script, """param([string]$Source,[string]$Feed)
$ErrorActionPreference='Stop'
. (Join-Path $Source 'scripts/NuGetPackageProjects.ps1')
foreach($package in $AtomUIReleasePackages | Where-Object {$_.PackageRole -in @('RuntimeProduct','Analyzer')}) {
    $common=@('-p:AtomUITargetFrameworks=net8.0','--disable-build-servers','-m:1','/nr:false')
    & dotnet restore $package.ProjectPath '-p:Configuration=Release' @common
    if($LASTEXITCODE){throw 'Legacy restore failed'}
    & dotnet build $package.ProjectPath -c Release --no-restore @common
    if($LASTEXITCODE){throw 'Legacy build failed'}
    & dotnet pack $package.ProjectPath -c Release --no-build --no-restore -o $Feed @common
    if($LASTEXITCODE){throw 'Legacy pack failed'}
}
""")
    verification.run("legacy-normal-pack", ["pwsh", "-NoLogo", "-NoProfile", "-File", script,
                     "-Source", source, "-Feed", legacy], extra={"NUGET_PACKAGES": str(cold / "source-packages")})
    if len(list(legacy.glob("*.nupkg"))) != 14:
        raise ValueError("Incomplete legacy product/analyzer inventory")
    for package in legacy.glob("*.nupkg"):
        with zipfile.ZipFile(package) as archive:
            names = archive.namelist()
            nuspec = archive.read(next(name for name in names if name.endswith(".nuspec"))).decode()
            if any(name.startswith("lib/net10") for name in names) or 'targetFramework="net10' in nuspec:
                raise ValueError("Legacy package contains modern implementation/metadata")
            with zipfile.ZipFile(feed / package.name) as canonical:
                for entry in names:
                    if entry.startswith("lib/net8.0/") and entry.endswith(".dll"):
                        if archive.read(entry) != canonical.read(entry):
                            raise ValueError("Legacy implementation differs from the actual canonical net8 DLL")

    def reference(package_id, version="6.2.8"):
        return f'<PackageReference Include="{package_id}" Version="{version}" />'

    def link(kind, path, name):
        return f'<{kind} Include="{escape(str(path))}" Link="{escape(name)}" />'

    configs = {}
    for family, target_feed, tfms in (("dual", feed, "net8.0;net10.0"), ("legacy", legacy, "net8.0")):
        config = root / family / "NuGet.Config"
        configs[family] = config
        write(config, '<configuration><packageSources><clear/><add key="local" value="' + escape(str(target_feed)) +
              '"/><add key="nuget" value="https://api.nuget.org/v3/index.json"/></packageSources>'
              '<packageSourceMapping><packageSource key="local"><package pattern="AtomUI.*"/></packageSource>'
              '<packageSource key="nuget"><package pattern="*"/></packageSource></packageSourceMapping></configuration>')
        for name in ("Resources", "ThirdParty"):
            items = reference("AtomUI.Core") + reference("AtomUI.Generator")
            if name == "ThirdParty":
                items += reference("AtomUI.Registration.Fixtures.Resources")
            for path in (fixtures / name).rglob("*"):
                if not path.is_file() or {"bin", "obj"}.intersection(path.relative_to(fixtures / name).parts):
                    continue
                kind = {".cs": "Compile", ".axaml": "AvaloniaXaml", ".tsv": "AdditionalFiles"}.get(path.suffix)
                if kind:
                    items += link(kind, path, str(path.relative_to(fixtures / name)))
            project = root / family / name / (name + ".csproj")
            write(project, f'<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFrameworks>{tfms}</TargetFrameworks>'
                  f'<AssemblyName>AtomUI.Registration.Fixtures.{name}</AssemblyName><Version>6.2.8</Version>'
                  f'<AtomUIRegistrationPackageId>Fixture.{name}</AtomUIRegistrationPackageId>'
                  f'<AtomUIThemeControlCatalog>Fixture.{name}</AtomUIThemeControlCatalog>'
                  '<ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><AllowUnsafeBlocks>true</AllowUnsafeBlocks>'
                  '<IsTrimmable>true</IsTrimmable></PropertyGroup><ItemGroup>' + items + '</ItemGroup></Project>')
            verification.run(family + "-" + name, ["dotnet", "pack", project, "-c", "Release", "-o", target_feed,
                             "-p:RestoreConfigFile=" + str(config)],
                             extra={"NUGET_PACKAGES": str(root / (family + "-producer-cache")),
                                    "NUGET_HTTP_CACHE_PATH": str(root / (family + "-producer-http"))})

    routes = (("net8-trim", "dual", "net8.0", False, False),
              ("net8-aot", "dual", "net8.0", True, False),
              ("net8-aot-noscan", "dual", "net8.0", True, True),
              ("net10-trim", "dual", "net10.0", False, False),
              ("net10-aot", "dual", "net10.0", True, False),
              ("bridge-trim", "legacy", "net10.0", False, False),
              ("bridge-aot", "legacy", "net10.0", True, False))
    results = {}
    for name, family, tfm, native, noscan in routes:
        directory = root / name
        items = reference("AtomUI.Desktop.Controls") + reference("AtomUI.Generator")
        items += reference("AtomUI.Registration.Fixtures.ThirdParty") + reference("Avalonia.Desktop", "12.1.5")
        for file, kind in (("ProductApp.cs", "Compile"), ("ProductFixtureStyles.axaml.cs", "Compile"),
                           ("ProductFixtureStyles.axaml", "AvaloniaXaml")):
            items += link(kind, fixtures / "Shared" / file, file)
        items += link("Compile", fixtures / "WindowsPackages/SelectedRecipe.cs", "SelectedRecipe.cs")
        items += '<Compile Include="Program.cs" /><Compile Include="BadRecord.cs" Condition="Exists(\'BadRecord.cs\')" />'
        if native and tfm == "net8.0":
            items += '<IlcArg Include="' + ("--noscan" if noscan else "--scan") + '" />'
        runtime = "8.0.33" if tfm == "net8.0" else "10.0.14"
        runtime_property = (f'<RuntimeFrameworkVersion>{runtime}</RuntimeFrameworkVersion>' if native else
                            f'<WindowsPackageRuntimeVersion Condition="\'$(WindowsPackageRuntimeVersion)\' == \'\'">{runtime}</WindowsPackageRuntimeVersion>')
        runtime_reference = '' if native else '<FrameworkReference Update="Microsoft.NETCore.App" RuntimeFrameworkVersion="$(WindowsPackageRuntimeVersion)" />'
        write(directory / "Consumer.csproj", f'<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>{tfm}</TargetFramework>'
              '<AssemblyName>AtomUI.Registration.Fixtures</AssemblyName><OutputType>Exe</OutputType>'
              '<DefineConstants>COMPLEX;REAL_DESKTOP</DefineConstants><ImplicitUsings>enable</ImplicitUsings>'
              '<EnableDefaultCompileItems>false</EnableDefaultCompileItems>'
              '<Nullable>enable</Nullable><AllowUnsafeBlocks>true</AllowUnsafeBlocks><PublishTrimmed>true</PublishTrimmed>'
              f'<TrimMode>full</TrimMode><PublishAot>{str(native).lower()}</PublishAot><RuntimeIdentifier>win-x64</RuntimeIdentifier>'
              f'{runtime_property}<IlcGenerateMapFile>true</IlcGenerateMapFile>'
              '</PropertyGroup>'
              '<ItemGroup><KnownILCompilerPack Update="Microsoft.DotNet.ILCompiler">'
              f'<ILCompilerPackVersion Condition="\'%(KnownILCompilerPack.TargetFramework)\' == \'{tfm}\' and '
              f'$([MSBuild]::VersionLessThan(\'%(KnownILCompilerPack.ILCompilerPackVersion)\', \'{runtime}\'))">'
              f'{runtime}</ILCompilerPackVersion></KnownILCompilerPack></ItemGroup>'
              f'<ItemGroup>{runtime_reference}{items}</ItemGroup></Project>')
        program = (fixtures / "Shared/DesktopProgram.cs").read_text(encoding="utf-8")
        needle = "var verification = app.VerifyAsync();"
        if program.count(needle) != 1:
            raise ValueError("Shared fixture entry point changed")
        program = program.replace(needle, "app.PrepareRetainedState();\n        var verification = VerifyAll();\n"
                                  "        async Task VerifyAll() { await app.VerifyAsync(); await app.VerifyExtraAsync(); }")
        program = program.replace('Console.WriteLine("PRODUCT_PASS "', 'Console.WriteLine("PRODUCT_RUNTIME " + System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);\n        Console.WriteLine("PRODUCT_PASS "')
        write(directory / "Program.cs", program)
        # Package contents include Localization/**/*.xlf. Keep NuGet caches outside
        # the application tree so normal source globs cannot import cached assets.
        cache = verification.root / "route-caches" / name / "packages"
        http = verification.root / "route-caches" / name / "http"
        if cache.exists() or http.exists():
            raise ValueError("Route cache was not initially absent")
        env = {"NUGET_PACKAGES": str(cache), "NUGET_HTTP_CACHE_PATH": str(http)}
        common = ["-p:RestoreConfigFile=" + str(configs[family])]
        verification.run(name + "-restore", ["dotnet", "restore", directory / "Consumer.csproj", "-r", "win-x64",
                         "-p:Configuration=Release", *common], extra=env)
        destination = directory / "publish"
        verification.run(name + "-publish", ["dotnet", "publish", directory / "Consumer.csproj", "-c", "Release",
                         "--no-restore", "-o", destination, *common], extra=env)
        text = verification.run(name + "-runtime", [destination / "AtomUI.Registration.Fixtures.exe"], extra=env,
                                marker="PRODUCT_PASS selected", timeout=180)
        verify_product_protocol(text, runtime)
        inspector = fixtures / "Inspect/bin/Release/net10.0/Inspect.dll"
        producer_tfm = "net8.0" if family == "legacy" else tfm
        for producer, full_type in (("Resources", "Fixture.Resources.UnusedExternalControl"),
                                    ("ThirdParty", "Fixture.ThirdParty.UnusedThirdPartyControl")):
            assembly = "AtomUI.Registration.Fixtures." + producer + ".dll"
            original = root / family / producer / "bin/Release" / producer_tfm / assembly
            verification.run(name + "-pretrim-" + producer,
                             ["dotnet", inspector, "type-presence", original, full_type, "true"],
                             marker="ROOT_SEMANTICS_PASS " + full_type + " present=True")
            if not native:
                # Build-time ConditionalRecord payload strings are allowed to
                # retain type names. Inspect TypeDef metadata, not raw bytes.
                verification.run(name + "-absence-" + producer,
                                 ["dotnet", inspector, "type-presence", destination / assembly, full_type, "false"],
                                 marker="ROOT_SEMANTICS_PASS " + full_type + " present=False")
        proofs = audit_outputs(directory, destination, native, family == "legacy", noscan)
        results[name] = dict(checks=len(PRODUCT_CHECKS), runtime=runtime, proofs=proofs,
                             executable=sha(destination / "AtomUI.Registration.Fixtures.exe"))
        if name in ("net8-trim", "net8-aot", "bridge-trim", "bridge-aot"):
            # Compile an invalid record normally; never modify a package ZIP,
            # restored DLL, frozen input or report to manufacture this negative.
            bad = directory / "BadRecord.cs"
            write(bad, '[assembly: AtomUI.Registration.ConditionalRegistrationRecord(999, "package", "bad-format", "{}")]')
            invalid = ["-p:BaseIntermediateOutputPath=" + str(directory / "invalid-obj") + os.sep,
                       "-p:BaseOutputPath=" + str(directory / "invalid-bin") + os.sep]
            verification.run(name + "-invalid-restore", ["dotnet", "restore", directory / "Consumer.csproj",
                             "-p:Configuration=Release", "-r", "win-x64", *common, *invalid], extra=env)
            text = verification.run(name + "-invalid-abi", ["dotnet", "publish", directory / "Consumer.csproj",
                                    "-c", "Release", "--no-restore", "-o", directory / "invalid-publish",
                                    *common, *invalid], extra=env, expected_exit=1)
            native8_rejection = name == "net8-aot" and "at ILCompiler.RegistrationInputBootstrap.Create(" in text
            if (("ATOMUIREG006" not in text and not native8_rejection) or
                    "Unknown or malformed conditional registration record envelope." not in text):
                raise ValueError("Invalid ABI failed for an unrelated preparation error")
            require_no_delivery(directory / "invalid-obj", directory / "invalid-publish")
            bad.unlink()
        if name == "bridge-trim":
            # Resolve a real, unsupported pack through the SDK. Do not fabricate
            # ResolvedRuntimePack items or modify restored package contents.
            rejected_root = directory / "rejected-runtime-obj"
            rejected = ["-p:WindowsPackageRuntimeVersion=10.0.8",
                        "-p:BaseIntermediateOutputPath=" + str(rejected_root) + os.sep,
                        "-p:BaseOutputPath=" + str(directory / "rejected-runtime-bin") + os.sep]
            verification.run("bridge-wrong-runtime-restore", ["dotnet", "restore", directory / "Consumer.csproj",
                             "-p:Configuration=Release", "-r", "win-x64", *common, *rejected], extra=env)
            resolved = verification.run("bridge-wrong-runtime-pack", ["dotnet", "msbuild", directory / "Consumer.csproj",
                                        "-t:ResolveFrameworkReferences", "-getItem:ResolvedRuntimePack",
                                        "-p:Configuration=Release", *common, *rejected], extra=env)
            packs = [item for item in json.loads(resolved)["Items"]["ResolvedRuntimePack"]
                     if item["Identity"] == "Microsoft.NETCore.App.Runtime.win-x64"]
            if len(packs) != 1 or packs[0]["NuGetPackageVersion"] != "10.0.8":
                raise ValueError("Wrong-runtime negative did not resolve the actual unsupported runtime pack")
            rejected_publish = directory / "rejected-runtime-publish"
            text = verification.run("bridge-wrong-runtime-rejected", ["dotnet", "publish", directory / "Consumer.csproj",
                                    "-c", "Release", "--no-restore", "-o", rejected_publish,
                                    *common, *rejected], extra=env, expected_exit=1)
            if "ATOMUIREG006" not in text or "runtime 10.0.14" not in text:
                raise ValueError("Unsupported runtime failed outside the bridge admission gate")
            require_no_delivery(rejected_root, rejected_publish)
            results[name]["unsupportedRuntimeRejected"] = "10.0.8"
    return results
