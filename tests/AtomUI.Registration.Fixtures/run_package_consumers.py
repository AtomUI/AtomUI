#!/usr/bin/env python3
"""Build/pack a source-only snapshot, then exercise ordinary cold NuGet consumers.
The output directory must not already exist. --serve also waits for a real browser's COLD_BROWSER_PASS.
"""
import argparse
import functools
import hashlib
import http.server
import json
import os
import shutil
import subprocess
import threading
import time
import zipfile
from pathlib import Path
from xml.sax.saxutils import escape

REPOSITORY = Path(__file__).resolve().parents[2]
FIXTURES = Path("tests/AtomUI.Registration.Fixtures")
PRODUCTS = ["AtomUI.Generator", "AtomUI.Localization", "AtomUI.Native", "AtomUI.Core", "AtomUI.Controls.Shared",
            "AtomUI.Fonts.AlibabaSans", "AtomUI.Icons.AntDesign", "AtomUI.Controls", "AtomUI.Desktop.Controls"]


def stage_sources(destination):
    listing = subprocess.check_output(["git", "ls-files", "-z", "--cached", "--others", "--exclude-standard"], cwd=REPOSITORY)
    copied = []
    for relative in sorted(set(listing.decode().split("\0")) - {""}):
        path = Path(relative)
        if any(part.lower() in {".artifacts", "bin", "obj", "node_modules", ".superpowers"} for part in path.parts):
            continue
        source = REPOSITORY / path
        if not source.is_file(): # Deleted tracked files are intentionally absent from the current snapshot.
            continue
        target = destination / path
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(source, target)
        copied.append(relative)
    consumers = [name for name in copied if name.startswith(str(FIXTURES / "PackageConsumers") + "/")]
    if len(consumers) != 20:
        raise RuntimeError(f"Expected all 20 deliverable consumer sources/configs, found {len(consumers)}")
    return copied


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output", type=Path, required=True, help="New owned directory for snapshot, feed, caches, logs and outputs")
    parser.add_argument("--rid", default="osx-arm64", help="Desktop host RID to publish/run (default: osx-arm64)")
    parser.add_argument("--serve", action="store_true", help="Serve Browser output and wait for its actual runtime assertion")
    parser.add_argument("--browser-timeout", type=int, default=300)
    parser.add_argument("--port", type=int, default=0)
    args = parser.parse_args()
    output = args.output.expanduser().resolve()
    output.mkdir(parents=True, exist_ok=False)
    source = output / "source"
    copied = stage_sources(source)
    (output / "source-inventory.json").write_text(json.dumps(copied, indent=2))
    logs = output / "logs"; logs.mkdir()
    feed = output / "feed"; feed.mkdir()
    env = os.environ.copy()
    env["NUGET_PACKAGES"] = str(output / "source-packages")
    results = {"source_artifacts_existed": (source / ".artifacts").exists(), "commands": [], "runtime": {}}
    if results["source_artifacts_existed"]:
        raise RuntimeError("The staged source must not contain existing .artifacts")

    def run(label, command, environment=env, expected_exit=0):
        log = logs / (label + ".log")
        started = time.monotonic()
        with log.open("w") as stream:
            stream.write("$ " + " ".join(map(str, command)) + "\n"); stream.flush()
            completed = subprocess.run(command, cwd=source, env=environment, stdout=stream, stderr=subprocess.STDOUT)
        results["commands"].append({"label": label, "command": list(map(str, command)), "exit": completed.returncode, "expected_exit": expected_exit,
                                    "seconds": round(time.monotonic() - started, 3), "log": str(log.relative_to(output))})
        (output / "results.json").write_text(json.dumps(results, indent=2))
        if completed.returncode != expected_exit:
            raise RuntimeError(f"{label} failed; see {log}")
        print(label + (" PASS" if expected_exit == 0 else " EXPECTED_FAILURE"), flush=True)
        return log.read_text()

    # Explicit source prerequisites. The staged checkout has no existing build outputs or package cache.
    run("build-adapter", ["dotnet", "build", str(FIXTURES / "BinaryAdapter/BinaryAdapter.csproj"), "-c", "Release"])
    run("build-linker", ["dotnet", "build", "src/AtomUI.Toolchain/AtomUI.Toolchain.csproj", "-t:BuildManagedToolchain", "-c", "Release"])
    version = subprocess.check_output(["dotnet", "msbuild", "src/AtomUI.Core/AtomUI.Core.csproj", "-getProperty:AtomUIVersion"], cwd=source, env=env, text=True).strip()
    for project in PRODUCTS:
        run("pack-" + project, ["dotnet", "pack", f"src/{project}/{project}.csproj", "-c", "Release", "--no-build", "--no-restore", "-o", str(feed)])
    inventory = {}
    for package in sorted(feed.glob("*.nupkg")):
        with zipfile.ZipFile(package) as archive:
            entries = archive.namelist()
        if any("LinkedRegistration" in entry or "LinkedPublish" in entry or entry.startswith("lib/net8.0/") or
               entry.endswith(("/illink.dll", "/Mono.Cecil.dll")) for entry in entries):
            raise RuntimeError("Unexpected package payload: " + package.name)
        inventory[package.name] = {"sha256": hashlib.sha256(package.read_bytes()).hexdigest(), "entries": entries}
    (output / "package-inventory.json").write_text(json.dumps(inventory, indent=2))

    consumer_env = env.copy()
    consumer_env["NUGET_PACKAGES"] = str(output / "consumer-packages")
    config = output / "NuGet.Config"
    config.write_text('<configuration><packageSources><clear/>'
                      f'<add key="atomui-local" value="{escape(str(feed))}"/>'
                      '<add key="nuget" value="https://api.nuget.org/v3/index.json"/></packageSources>'
                      '<packageSourceMapping><packageSource key="atomui-local"><package pattern="AtomUI.*"/></packageSource>'
                      '<packageSource key="nuget"><package pattern="*"/></packageSource></packageSourceMapping></configuration>')
    common = ["-p:RestoreConfigFile=" + str(config), "-p:AtomUIFixtureVersion=" + version]
    adapter = source / FIXTURES / "BinaryAdapter/bin/Release/net10.0/Fixture.BinaryAdapter.dll"
    ordinary = FIXTURES / "PackageConsumers"
    author_theme = source / ordinary / "Author/Themes/AuthorControlTheme.axaml"
    original_theme = author_theme.read_bytes()
    try:
        author_theme.write_text(original_theme.decode().replace(
            '    <ResourceDictionary.MergedDictionaries>\n        <ResourceInclude Source="SharedResourceTheme.axaml" />\n    </ResourceDictionary.MergedDictionaries>\n', ''))
        negative = run("named-unincluded-negative", ["dotnet", "build", str(ordinary / "Author/Author.csproj"), "-c", "Release", *common],
                       consumer_env, expected_exit=1)
        if "ATOMUIREG004" not in negative or "SharedTheme" not in negative or "AuthorControlTheme.axaml" not in negative:
            raise RuntimeError("Unincluded named theme did not report a located resource diagnostic")
    finally:
        author_theme.write_bytes(original_theme)
    run("author", ["dotnet", "build", str(ordinary / "Author/Author.csproj"), "-c", "Release", *common], consumer_env)
    # The ordinary author consumes an explicitly included named theme. The extra exported
    # unrelated theme is present before trim and absent afterwards, without authored roots.
    named_project = ordinary / "NamedResources/NamedResources.csproj"
    named = run("named-ordinary", ["dotnet", "run", "--project", str(named_project), "-c", "Release", *common], consumer_env)
    if "STATIC_RESOURCE_PASS trimmed=False" not in named:
        raise RuntimeError("Named resource ordinary consumer did not pass")
    named_output = output / "named-resources"
    run("named-publish", ["dotnet", "publish", str(named_project), "-c", "Release", "-r", args.rid,
                          "--self-contained", "true", "-p:PublishTrimmed=true", "-p:TrimMode=full", *common,
                          "-o", str(named_output)], consumer_env)
    named = run("named-trimmed", [str(named_output / ("NamedResources.exe" if args.rid.startswith("win-") else "NamedResources"))], consumer_env)
    if "STATIC_RESOURCE_PASS trimmed=True" not in named:
        raise RuntimeError("Named resource trimmed consumer did not pass")
    inspect = FIXTURES / "Inspect/Inspect.csproj"
    run("typed-key-il", ["dotnet", "run", "--project", str(inspect), "-c", "Release", "--", "typed-resource-condition",
                          str(source / ordinary / "Author/bin/Release/net10.0/Fixture.NuGetAuthor.dll"),
                          "TypedKeyHostTheme.axaml", "Fixture.NuGetAuthor.TypedKeyDependencyControl"])
    for label, assembly, present in [
        ("named-pretrim", source / ordinary / "Author/bin/Release/net10.0/Fixture.NuGetAuthor.dll", True),
        ("named-absence", named_output / "Fixture.NuGetAuthor.dll", False),
    ]:
        run(label, ["dotnet", "run", "--project", str(inspect), "-c", "Release", "--", "type-presence", str(assembly),
                    "Fixture.NuGetAuthor.UnrelatedResourceControl", str(present).lower()])
    results["runtime"]["named-resources"] = "ordinary and fulltrim pass; unrelated exported target absent"
    for mode in ["direct", "transitive", "binary"]:
        destination = output / mode
        run("publish-" + mode, ["dotnet", "publish", str(ordinary / "Host/Host.csproj"), "-c", "Release", "-r", args.rid,
                               "--self-contained", "true", "-p:PublishTrimmed=true", "-p:FixtureReferenceMode=" + mode,
                               "-p:FixtureBinaryAdapterPath=" + str(adapter), *common, "-o", str(destination)], consumer_env)
        app = destination / ("Host.exe" if args.rid.startswith("win-") else "Host")
        text = run("run-" + mode, [str(app)], consumer_env)
        if "COLD_CONSUMER_PASS" not in text or "trimmed=True" not in text:
            raise RuntimeError("Missing consumer runtime assertion: " + mode)
        results["runtime"][mode] = "pass"
    destination = output / "browser"
    run("publish-browser", ["dotnet", "publish", str(ordinary / "Browser/Browser.csproj"), "-c", "Release",
                            "-p:RunAOTCompilation=false", "-p:FixtureBinaryAdapterPath=" + str(adapter), *common,
                            "-o", str(destination)], consumer_env)
    receipt_path = source / ordinary / "Browser/obj/Release/net10.0-browser/AtomUI.TypeMap.receipt.json"
    receipt = json.loads(receipt_path.read_text())
    if receipt["status"] != "output-verified" or len(receipt["accessors"]) != 3:
        raise RuntimeError("Product package Browser receipt did not verify Common/Desktop/Author")
    shutil.copy2(receipt_path, output / "browser-receipt.json")
    results["runtime"]["browser"] = "pending-browser-run"
    if args.serve:
        passed = threading.Event()
        class Handler(http.server.SimpleHTTPRequestHandler):
            def send_head(self):
                if "If-Modified-Since" in self.headers: del self.headers["If-Modified-Since"]
                return super().send_head()
            def end_headers(self):
                self.send_header("Cache-Control", "no-store"); super().end_headers()
            def do_POST(self):
                body = self.rfile.read(min(int(self.headers.get("Content-Length", "0")), 256)).decode()
                valid = self.path == "/__fixture_result" and body == "COLD_BROWSER_PASS ordinary binary adapter template"
                self.send_response(200 if valid else 400); self.end_headers()
                if valid: passed.set()
            def log_message(self, *_): pass
        server = http.server.ThreadingHTTPServer(("127.0.0.1", args.port), functools.partial(Handler, directory=str(destination / "wwwroot")))
        thread = threading.Thread(target=server.serve_forever, daemon=True); thread.start()
        try:
            url = f"http://127.0.0.1:{server.server_port}/?report=1"
            (output / "browser-url.txt").write_text(url)
            print("Open in a browser: " + url, flush=True)
            if not passed.wait(args.browser_timeout): raise RuntimeError("Browser did not report COLD_BROWSER_PASS before timeout")
            results["runtime"]["browser"] = "pass"
        finally:
            server.shutdown(); server.server_close(); thread.join()
    (output / "results.json").write_text(json.dumps(results, indent=2))
    print("PACKAGE_CONSUMERS_PASS" if args.serve else "PACKAGE_PUBLISH_PASS (Browser runtime pending; rerun with --serve)", flush=True)

if __name__ == "__main__": main()
