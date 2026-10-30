#!/usr/bin/env python3
"""Maintainer-only preparation of the portable managed compiler included in NuGet packages.

Consumers use the packaged bundle and their SDK-restored native helpers; they never run this.
"""
import argparse
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys
import uuid

REPOSITORY = Path(__file__).resolve().parents[2]
MODULE = REPOSITORY / "src" / "AtomUI.Toolchain" / "Backends" / "ILC8"


def run(arguments, cwd=None):
    subprocess.run(arguments, cwd=cwd, check=True)


def prepare_source(path, upstream):
    if path.exists():
        if not (path / ".git").exists():
            raise RuntimeError(f"Existing compiler source path is not a Git checkout: {path}")
        return
    path.parent.mkdir(parents=True, exist_ok=True)
    staging = path.with_name(path.name + ".fetch-" + uuid.uuid4().hex)
    staging.mkdir()
    try:
        run(["git", "init", "--quiet"], staging)
        # Runtime source paths exceed Windows MAX_PATH, especially in the staging directory.
        run(["git", "config", "--local", "core.longpaths", "true"], staging)
        run(["git", "remote", "add", "origin", upstream["repository"]], staging)
        run(["git", "sparse-checkout", "init", "--cone"], staging)
        run(["git", "sparse-checkout", "set", "eng", "src/coreclr/tools",
             "src/libraries/Common", "src/tools/illink/src/ILLink.Shared"], staging)
        run(["git", "fetch", "--depth=1", "--filter=blob:none", "origin", upstream["commit"]], staging)
        run(["git", "checkout", "--detach", upstream["commit"]], staging)
        try:
            staging.rename(path)
        except OSError:
            # A concurrent maintainer may have prepared the identical checkout. The build recipe
            # below independently validates its exact HEAD and tracked source before using it.
            if not (path / ".git").exists():
                raise
    finally:
        if staging.exists():
            shutil.rmtree(staging)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--source-root", type=Path)
    parser.add_argument("--bundle", type=Path, required=True)
    parser.add_argument("--dotnet", default="dotnet")
    args = parser.parse_args()
    upstream = json.loads((MODULE / "upstream.json").read_text())
    source = (args.source_root or Path(os.environ.get("ATOMUI_RUNTIME8_SOURCE",
        REPOSITORY / ".artifacts" / "sources" / ("dotnet-runtime-" + upstream["commit"])))).resolve()
    bundle = args.bundle.resolve()
    if bundle.name != "managed-bundle":
        parser.error("--bundle must name the managed-bundle directory produced by the compiler recipe")
    prepare_source(source, upstream)
    run([sys.executable, str(MODULE / "build" / "build_host.py"), "--source-root", str(source),
         "--output", str(bundle.parent), "--dotnet", args.dotnet, "--managed-only", "--ensure"])
    if not (bundle / "atomui-ilc8-capability.json").is_file():
        raise RuntimeError("Compiler recipe did not produce the package capability manifest")
    for original, packaged in [("LICENSE.TXT", "LICENSE.runtime.txt"),
                               ("THIRD-PARTY-NOTICES.TXT", "THIRD-PARTY-NOTICES.runtime.txt")]:
        shutil.copyfile(source / original, bundle.parent / packaged)
    print(f"Prepared portable managed ILC8 package bundle: {bundle}")


if __name__ == "__main__":
    try:
        main()
    except (OSError, RuntimeError, subprocess.CalledProcessError) as error:
        print(f"Native8 compiler preparation failed: {error}", file=sys.stderr)
        sys.exit(1)
