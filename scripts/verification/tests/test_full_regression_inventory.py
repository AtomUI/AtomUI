"""Keep the release regression entry point aligned with real test projects."""

import os
import shutil
import subprocess
import tempfile
import unittest
from pathlib import Path


class FullRegressionInventoryTests(unittest.TestCase):
    def test_default_run_includes_every_top_level_test_project(self):
        root = Path(__file__).resolve().parents[3]
        expected = {
            project.parent.name
            for project in (root / "tests").glob("*/*.csproj")
            if project.parent.name.endswith((".Tests", "IntegrationTests"))
        }
        bash = shutil.which("bash")
        self.assertIsNotNone(bash, "The inventory check requires Bash")
        with tempfile.TemporaryDirectory() as directory:
            bin_dir = Path(directory)
            with (bin_dir / "dotnet").open("w", encoding="utf-8", newline="\n") as stub:
                stub.write(
                    '#!/bin/sh\ncase " $* " in\n'
                    '  *" --list-tests "*) printf "    SampleTest\\n" ;;\n'
                    '  *) printf "  Passed SampleTest\\n" ;;\n'
                    'esac\n'
                )
            with (bin_dir / "sleep").open("w", encoding="utf-8", newline="\n") as stub:
                stub.write("#!/bin/sh\nexit 0\n")
            (bin_dir / "dotnet").chmod(0o755)
            (bin_dir / "sleep").chmod(0o755)
            environment = os.environ.copy()
            environment["PATH"] = os.pathsep.join([str(bin_dir), environment["PATH"]])
            result = subprocess.run(
                [
                    bash, "-c",
                    # Verify the command stubs before starting the inventory-only shell run.
                    'if [[ ! "$(command -v dotnet)" -ef "$1/dotnet" || '
                    '! "$(command -v sleep)" -ef "$1/sleep" ]]; then '
                    'echo "Inventory requires temporary dotnet and sleep stubs" >&2; exit 1; fi; '
                    'exec "$BASH" "$2"',
                    "inventory", bin_dir.as_posix(), (root / "scripts/run-full-regression.sh").as_posix(),
                ],
                cwd=root,
                env=environment,
                capture_output=True,
                text=True,
                errors="replace",
                timeout=30,
            )

        self.assertEqual(0, result.returncode, result.stdout + result.stderr)
        actual = {
            line.split()[1].rstrip(":")
            for line in result.stdout.splitlines()
            if line.startswith("[SUMMARY] ") and line.endswith(" tests")
        }
        self.assertEqual(expected, actual)


if __name__ == "__main__":
    unittest.main()
