"""CLI contracts for the mandatory bug-fix verification phases."""
import importlib.util
import sys
import unittest
from pathlib import Path


MODULE_PATH = Path(__file__).resolve().parents[1] / "test.py"
sys.path.insert(0, str(MODULE_PATH.parent))
SPEC = importlib.util.spec_from_file_location("atomui_verification_cli", MODULE_PATH)
CLI = importlib.util.module_from_spec(SPEC)
assert SPEC.loader is not None
SPEC.loader.exec_module(CLI)


class VerificationCliTests(unittest.TestCase):
    def parse(self, *argv):
        return CLI.parser().parse_args(argv)

    def test_iterate_run_requires_explicit_bug_module_paths(self):
        args = self.parse("run", "--scope", "iterate")

        with self.assertRaisesRegex(ValueError, "--path"):
            CLI.validate_run_arguments(args)

    def test_iterate_run_accepts_one_or_more_focus_paths(self):
        args = self.parse(
            "run",
            "--scope", "iterate",
            "--path", "src/Controls/Button/Button.cs",
            "--path", "tests/UI.Tests/Button/ButtonTests.cs")

        CLI.validate_run_arguments(args)
        self.assertEqual(2, len(args.path))

    def test_focused_iteration_plans_only_supplied_paths_not_all_worktree_changes(self):
        class Repository:
            def changes(self, _base):
                return ["src/Controls/Popup/Popup.cs", "src/Controls/Select/Select.cs"]

            def focus_paths(self, paths):
                return paths

        args = self.parse(
            "run", "--scope", "iterate",
            "--path", "src/Controls/Button/Button.cs")

        self.assertEqual(
            ["src/Controls/Button/Button.cs"],
            CLI.verification_changes(args, Repository()))

    def test_focus_paths_cannot_be_used_to_claim_change_or_full_coverage(self):
        for scope in ("change", "full"):
            with self.subTest(scope=scope):
                args = self.parse("run", "--scope", scope, "--path", "src/Controls/Button/Button.cs")
                with self.assertRaisesRegex(ValueError, "only valid with --scope iterate"):
                    CLI.validate_run_arguments(args)

    def test_local_change_run_is_rejected_during_bug_fixing(self):
        args = self.parse("run", "--scope", "change")

        with self.assertRaisesRegex(ValueError, "--scope iterate --path"):
            CLI.validate_run_arguments(args)

    def test_branch_or_ci_change_validation_remains_available(self):
        for argv in (
            ("run", "--scope", "change", "--base", "origin/main"),
            ("run", "--scope", "change", "--tests-only"),
        ):
            with self.subTest(argv=argv):
                CLI.validate_run_arguments(self.parse(*argv))

    def test_local_full_run_uses_the_progress_aware_canonical_script(self):
        args = self.parse("run", "--scope", "full")

        with self.assertRaisesRegex(ValueError, "scripts/run-full-regression.sh"):
            CLI.validate_run_arguments(args)

    def test_ci_full_test_phase_remains_available(self):
        CLI.validate_run_arguments(self.parse("run", "--scope", "full", "--tests-only"))


if __name__ == "__main__":
    unittest.main()
