"""Exercise real orchestration with a process fixture at the dotnet boundary."""
import json
import os
import sys
import tempfile
import unittest
from pathlib import Path

from test_planner import RepositoryCase

try:
    from affected.evidence import read_trx, output_fingerprint
    from affected.runner import run_verification, RunLock, run_specialist_check
except ImportError:
    read_trx = output_fingerprint = run_verification = RunLock = run_specialist_check = None


FAKE_DOTNET = r'''import sys, pathlib, xml.etree.ElementTree as E
root=pathlib.Path.cwd()
mode=(root/"behavior").read_text().strip() if (root/"behavior").exists() else "pass"
args=sys.argv[1:]
if args==["--info"]:
 print("SDK 10.0.300 test boundary");sys.exit(0)
state=root/".artifacts"
state.mkdir(exist_ok=True)
with (state/"calls").open("a") as f: f.write(args[0]+"\n")
if args[0]=="build":
 out=state/"bin"/"Debug"/"net10.0";out.mkdir(parents=True,exist_ok=True)
 (out/"UI.Tests.dll").write_text("fresh assembly")
 if mode=="build-fails": sys.exit(1)
elif args[0]=="test":
 if mode=="test-fails": sys.exit(1)
 output=pathlib.Path(args[args.index("--results-directory")+1]);output.mkdir(parents=True,exist_ok=True)
 settings=E.parse(args[args.index("--settings")+1])
 names=settings.find(".//TestCaseFilter").text.split("|")
 names=[n.split("~",1)[1].rstrip(".") for n in names]
 run=E.Element("TestRun");defs=E.SubElement(run,"TestDefinitions");results=E.SubElement(run,"Results")
 for i,name in enumerate(names):
  unit=E.SubElement(defs,"UnitTest",id=str(i));E.SubElement(unit,"TestMethod",className=name,name="Works")
  E.SubElement(results,"UnitTestResult",testId=str(i),outcome="NotExecuted" if mode=="skipped" else "Passed")
 count=0 if mode=="zero" else len(names)
 summary=E.SubElement(run,"ResultSummary",outcome="Completed")
 E.SubElement(summary,"Counters",total=str(count),executed=str(count),passed=str(count),failed="0",notExecuted="0")
 E.ElementTree(run).write(output/"result.trx")
 if mode=="source-changes": (root/"src/Controls/Button/Button.cs").write_text("class Button { int changedDuringTest; }")
 if mode=="output-changes": (state/"bin/Debug/net10.0/UI.Tests.dll").write_text("untested replacement assembly")
 if mode=="fixture-output": (state/"bin/Debug/net10.0/FixtureModule.dll").write_text("independent integration fixture")
'''


class RunnerTests(RepositoryCase):
    def setUp(self):
        super().setUp()
        self.write("fake_dotnet.py", FAKE_DOTNET)
        self.command = [sys.executable, str(self.root / "fake_dotnet.py")]
        self.report_dir = self.root / ".artifacts/verification"

    def run_plan(self, plan=None, reuse=True):
        self.assertIsNotNone(run_verification, "Verification runner has not been implemented")
        planner = self.planner()
        plan = plan or planner.plan(["src/Controls/Button/Button.cs"])
        return run_verification(planner.repo, plan, self.report_dir, dotnet=self.command, reuse=reuse)

    def test_success_uses_fresh_build_and_reuses_only_identical_receipt(self):
        first = self.run_plan()
        self.assertEqual("passed", first["status"])
        self.assertEqual(1, first["test_results"][0]["passed"])
        second = self.run_plan()
        self.assertEqual("reused", second["status"])
        self.assertEqual(["build", "test"], (self.root / ".artifacts/calls").read_text().splitlines())

    def test_changed_source_invalidates_receipt(self):
        self.run_plan()
        self.write("src/Controls/Button/Button.cs", "class Button { int changed; }")
        self.assertEqual("passed", self.run_plan()["status"])
        self.assertEqual(2, (self.root / ".artifacts/calls").read_text().splitlines().count("test"))

    def test_changed_assembly_invalidates_receipt(self):
        self.run_plan()
        self.write(".artifacts/bin/Debug/net10.0/UI.Tests.dll", "old assembly from another build")
        self.assertEqual("passed", self.run_plan()["status"])

    def test_new_runtime_configuration_invalidates_receipt(self):
        self.run_plan()
        self.write(".artifacts/bin/Debug/net10.0/UI.Tests.runtimeconfig.dev.json", '{"runtimeOptions":{"additionalProbingPaths":["different"]}}')
        self.assertEqual("passed", self.run_plan()["status"])

    def test_ci_test_phase_does_not_discharge_specialist_obligations(self):
        self.assertIsNotNone(run_verification)
        planner = self.planner()
        plan = planner.plan(["src/Controls/Button/Button.cs"])
        plan["obligations"] = [{"id": "native-aot", "description": "Real publish required"}]
        report = run_verification(planner.repo, plan, self.report_dir, dotnet=self.command, tests_only=True)
        self.assertEqual("tests-passed", report["status"])
        self.assertEqual(1, len(report["remaining_obligations"]))
        self.assertEqual("pending", self.run_plan(plan)["status"])

    def test_zero_tests_is_failure_despite_successful_process(self):
        self.write("behavior", "zero")
        self.assertEqual("failed", self.run_plan()["status"])

    def test_skipped_results_cannot_be_reported_as_full_coverage(self):
        self.write("behavior", "skipped")
        self.assertEqual("failed", self.run_plan()["status"])

    def test_build_failure_does_not_run_stale_tests(self):
        self.write("behavior", "build-fails")
        self.assertEqual("failed", self.run_plan()["status"])
        self.assertEqual(["build"], (self.root / ".artifacts/calls").read_text().splitlines())

    def test_source_change_during_test_invalidates_success(self):
        self.write("behavior", "source-changes")
        self.assertEqual("failed", self.run_plan()["status"])

    def test_output_change_during_test_cannot_certify_untested_assembly(self):
        self.write("behavior", "output-changes")
        self.assertEqual("failed", self.run_plan()["status"])

    def test_new_independent_fixture_output_does_not_invalidate_tested_dependencies(self):
        self.write("behavior", "fixture-output")
        self.assertEqual("passed", self.run_plan()["status"])

    def test_pending_publish_obligation_prevents_complete_receipt(self):
        plan = self.planner().plan(["src/Controls/Button/Button.cs"])
        plan["obligations"] = [{"id": "native-aot", "description": "Real publish required"}]
        self.assertEqual("pending", self.run_plan(plan)["status"])

    def test_successful_specialist_command_is_bound_to_current_inputs(self):
        self.assertIsNotNone(run_specialist_check, "Specialist command evidence is not implemented")
        planner = self.planner()
        check = {"id": "native-aot", "description": "Fixture specialist verification"}
        command = [sys.executable, "-c", "print('verified fixture')"]
        check["verifier"] = command
        result = run_specialist_check(planner.repo, check, command, self.report_dir)
        self.assertEqual("passed", result["status"])
        plan = planner.plan(["src/Controls/Button/Button.cs"])
        plan["obligations"] = [check]
        self.assertEqual("passed", self.run_plan(plan)["status"])
        self.write("src/Controls/Button/Button.cs", "class Button { int changed; }")
        plan = self.planner().plan(["src/Controls/Button/Button.cs"])
        plan["obligations"] = [check]
        self.assertEqual("pending", self.run_plan(plan)["status"])

    def test_failed_specialist_command_cannot_satisfy_obligation(self):
        self.assertIsNotNone(run_specialist_check, "Specialist command evidence is not implemented")
        check = {"id": "native-aot", "description": "Fixture specialist verification"}
        command = [sys.executable, "-c", "raise SystemExit(1)"]
        check["verifier"] = command
        result = run_specialist_check(self.planner().repo, check, command, self.report_dir)
        self.assertEqual("failed", result["status"])
        plan = self.planner().plan(["src/Controls/Button/Button.cs"])
        plan["obligations"] = [check]
        self.assertEqual("pending", self.run_plan(plan)["status"])

    def test_unregistered_successful_command_cannot_discharge_specialist_obligation(self):
        self.assertIsNotNone(run_specialist_check)
        with self.assertRaises(ValueError):
            run_specialist_check(self.planner().repo, {"id": "native-aot"}, [sys.executable, "-c", "print('ok')"], self.report_dir)

    def test_quick_variant_cannot_replace_registered_full_verifier(self):
        self.assertIsNotNone(run_specialist_check)
        check = {"id": "native-aot", "verifier": ["bash", "verify.sh", "--full"]}
        with self.assertRaises(ValueError):
            run_specialist_check(self.planner().repo, check, ["bash", "verify.sh", "--quick"], self.report_dir)

    def test_unmapped_changes_are_rejected_before_build(self):
        plan = self.planner().plan(["unknown.input"])
        self.assertEqual("blocked", self.run_plan(plan)["status"])
        self.assertFalse((self.root / ".artifacts/calls").exists())

    def test_a_plan_made_before_a_new_change_is_rejected(self):
        plan = self.planner().plan(["src/Controls/Button/Button.cs"])
        self.write("src/Controls/Select/Select.cs", "class Select { int changed; }")
        self.assertEqual("failed", self.run_plan(plan)["status"])
        self.assertFalse((self.root / ".artifacts/calls").exists())

    def test_report_survives_but_raw_results_are_cleaned(self):
        report = self.run_plan()
        self.assertTrue((self.report_dir / "latest.json").exists())
        self.assertEqual([], list(self.root.rglob("*.trx")))
        self.assertFalse(Path(report["temporary_directory"]).exists())

    def test_second_runner_cannot_mutate_shared_outputs(self):
        self.assertIsNotNone(RunLock, "Shared output lock has not been implemented")
        with RunLock(self.report_dir):
            with self.assertRaises(RuntimeError):
                with RunLock(self.report_dir):
                    self.fail("Concurrent runner acquired the same output lock")


class EvidenceTests(unittest.TestCase):
    def test_missing_trx_cannot_count_as_passing(self):
        self.assertIsNotNone(read_trx, "TRX evidence reader has not been implemented")
        with tempfile.TemporaryDirectory() as temp:
            with self.assertRaises(ValueError):
                read_trx(Path(temp), ["Tests.ButtonTests"])

    def test_unexecuted_selected_class_is_detected(self):
        self.assertIsNotNone(read_trx, "TRX evidence reader has not been implemented")
        with tempfile.TemporaryDirectory() as temp:
            Path(temp, "a.trx").write_text('<TestRun><TestDefinitions><UnitTest id="1"><TestMethod className="Tests.OtherTests"/></UnitTest></TestDefinitions><Results><UnitTestResult testId="1" outcome="Passed"/></Results><ResultSummary><Counters total="1" executed="1" passed="1" failed="0"/></ResultSummary></TestRun>')
            with self.assertRaises(ValueError):
                read_trx(Path(temp), ["Tests.ButtonTests"])


if __name__ == "__main__":
    unittest.main()
