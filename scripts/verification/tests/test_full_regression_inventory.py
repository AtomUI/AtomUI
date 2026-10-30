"""Keep the release regression entry point aligned with real test projects."""

import os
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
        with tempfile.TemporaryDirectory() as directory:
            bin_dir = Path(directory)
            (bin_dir / "dotnet").write_text(
                '#!/bin/sh\ncase " $* " in\n'
                '  *" --list-tests "*) printf "    SampleTest\\n" ;;\n'
                '  *) printf "  Passed SampleTest\\n" ;;\n'
                'esac\n'
            )
            (bin_dir / "sleep").write_text("#!/bin/sh\nexit 0\n")
            (bin_dir / "dotnet").chmod(0o755)
            (bin_dir / "sleep").chmod(0o755)
            environment = os.environ.copy()
            environment["PATH"] = f"{bin_dir}:{environment['PATH']}"
            result = subprocess.run(
                [str(root / "scripts/run-full-regression.sh")],
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
