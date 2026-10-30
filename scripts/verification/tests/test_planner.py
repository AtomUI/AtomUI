"""Regression tests for missed changes and false verification scope."""
import json
import subprocess
import sys
import tempfile
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))

try:
    from affected.repository import Repository, test_classes
    from affected.planner import Planner
except ImportError:
    Repository = Planner = test_classes = None


class RepositoryCase(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory(prefix="atomui-selection-test-")
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.git("init", "-q")
        self.git("config", "user.email", "test@example.invalid")
        self.git("config", "user.name", "Test")
        self.write(".gitignore", ".artifacts/\n__pycache__/\n")
        self.write("src/Controls/Controls.csproj", '<Project Sdk="Microsoft.NET.Sdk"/>')
        self.write("src/Controls/Button/Button.cs", "namespace Product; public class Button {}")
        self.write("src/Controls/Popup/Popup.cs", "namespace Product; public class Popup {}")
        self.write("src/Controls/Select/Select.cs", "namespace Product; public class Select { Popup popup; }")
        self.write("tests/UI.Tests/UI.Tests.csproj", '<Project><ItemGroup><PackageReference Include="xunit.v3"/><ProjectReference Include="../../src/Controls/Controls.csproj"/></ItemGroup></Project>')
        for name, uses in [("Button", "Button"), ("Popup", "Popup"), ("Select", "Select")]:
            self.write("tests/UI.Tests/" + name + "/" + name + "Tests.cs", "namespace Tests; public class " + name + "Tests { [Fact] public void Works() { new " + uses + "(); } }")
        self.write("tests/Integration.Tests/Integration.Tests.csproj", '<Project><ItemGroup><PackageReference Include="xunit.v3"/></ItemGroup></Project>')
        self.write("tests/Integration.Tests/Checks.cs", "namespace Integration; public class Checks { [Theory] [InlineData(1)] public void Works(int a) {} }")
        self.policy = {
            "version": 1,
            "areas": [{"id": "controls", "roots": ["src/Controls"], "depth": 1, "tests": ["tests/UI.Tests/**/*.cs"]}],
            "rules": [{"id": "integration", "changes": ["inputs/**"], "tests": ["tests/Integration.Tests/**/*.cs"]}],
            "ignore": ["docs/**", ".gitignore"],
            "tooling": ["scripts/verification/**"],
            "obligations": [],
        }
        self.write("scripts/verification/test-policy.json", json.dumps(self.policy))
        self.git("add", ".")
        self.git("commit", "-qm", "baseline")

    def git(self, *args):
        return subprocess.check_output(["git", "-C", str(self.root), *args], text=True).strip()

    def write(self, path, text):
        destination = self.root / path
        destination.parent.mkdir(parents=True, exist_ok=True)
        destination.write_text(text)

    def planner(self):
        self.assertIsNotNone(Planner, "Affected verification planner has not been implemented")
        return Planner(Repository(self.root), self.policy)

    def selected(self, plan):
        return {name for job in plan["tests"] for name in job["classes"]}


class PlannerTests(RepositoryCase):
    def test_control_change_selects_its_cross_project_tests_without_unrelated_controls(self):
        plan = self.planner().plan(["src/Controls/Button/Button.cs"])
        self.assertEqual({"Tests.ButtonTests"}, self.selected(plan))
        self.assertFalse(plan["gaps"])

    def test_unchanged_sibling_type_does_not_expand_consumers(self):
        self.write("src/Controls/Button/OtherControl.cs", "class OtherControl {}")
        self.write("src/Controls/Select/Select.cs", "class Select { OtherControl item; }")
        plan = self.planner().plan(["src/Controls/Button/Button.cs"])
        self.assertNotIn("Tests.SelectTests", self.selected(plan))

    def test_consumer_directory_does_not_pull_in_unrelated_sibling_types(self):
        self.write("src/Controls/Primitives/ButtonPresenter.cs", "class ButtonPresenter { Button item; }")
        self.write("src/Controls/Primitives/UnrelatedBase.cs", "class UnrelatedBase {}")
        self.write("tests/UI.Tests/Primitives/UnrelatedTests.cs", "namespace Tests; class UnrelatedTests { [Fact] void Works() { new UnrelatedBase(); } }")
        self.write("tests/UI.Tests/Primitives/PresenterTests.cs", "namespace Tests; class PresenterTests { [Fact] void Works() { new ButtonPresenter(); } }")
        plan = self.planner().plan(["src/Controls/Button/Button.cs"])
        self.assertIn("Tests.PresenterTests", self.selected(plan))
        self.assertNotIn("Tests.UnrelatedTests", self.selected(plan))

    def test_shared_popup_change_includes_transitive_consumer_tests(self):
        plan = self.planner().plan(["src/Controls/Popup/Popup.cs"])
        self.assertEqual({"Tests.PopupTests", "Tests.SelectTests"}, self.selected(plan))

    def test_iterate_scope_defers_cross_module_explicit_rules(self):
        self.policy["rules"].append({
            "id": "popup-integration",
            "changes": ["src/Controls/Popup/**"],
            "tests": ["tests/Integration.Tests/**/*.cs"],
        })

        plan = self.planner().plan(["src/Controls/Popup/Popup.cs"], scope="iterate")

        self.assertIn("Tests.PopupTests", self.selected(plan))
        self.assertNotIn("Integration.Checks", self.selected(plan))

    def test_theme_target_change_includes_control_consumers(self):
        self.write("src/Controls/Popup/Themes/PopupTheme.axaml", '<ControlTheme TargetType="atom:Popup" x:Class="Themes.PopupTheme"/>')
        plan = self.planner().plan(["src/Controls/Popup/Themes/PopupTheme.axaml"])
        self.assertIn("Tests.SelectTests", self.selected(plan))

    def test_in_family_inheritance_still_reaches_external_consumers(self):
        self.write("src/Controls/Popup/DerivedPopup.cs", "class DerivedPopup : Popup {}")
        self.write("src/Controls/Select/Select.cs", "class Select { DerivedPopup popup; }")
        plan = self.planner().plan(["src/Controls/Popup/Popup.cs"])
        self.assertIn("Tests.SelectTests", self.selected(plan))

    def test_in_family_composition_selects_its_tests_outside_family_directory(self):
        self.write("src/Controls/Popup/PopupPresenter.cs", "class PopupPresenter { Popup popup; }")
        self.write("tests/UI.Tests/PresentationTests.cs", "namespace Tests; class PresentationTests { [Fact] void Works() { new PopupPresenter(); } }")
        plan = self.planner().plan(["src/Controls/Popup/Popup.cs"])
        self.assertIn("Tests.PresentationTests", self.selected(plan))

    def test_composed_control_inheritance_is_selected_without_second_composition_hop(self):
        self.write("src/Controls/Button/ButtonPolicy.cs", "class ButtonPolicy {}")
        self.write("src/Controls/Button/Button.cs", "class Button { ButtonPolicy policy; }")
        self.write("src/Controls/Dropdown/Dropdown.cs", "class Dropdown : Button {}")
        self.write("src/Controls/Toolbar/Toolbar.cs", "class Toolbar { Dropdown control; }")
        for name in ["Dropdown", "Toolbar"]:
            self.write("tests/UI.Tests/" + name + "Tests.cs", "namespace Tests; class " + name + "Tests { [Fact] void Works() { new " + name + "(); } }")
        plan = self.planner().plan(["src/Controls/Button/ButtonPolicy.cs"])
        self.assertIn("Tests.DropdownTests", self.selected(plan))
        self.assertNotIn("Tests.ToolbarTests", self.selected(plan))

    def test_code_in_a_string_is_not_a_runtime_consumer_edge(self):
        self.write("src/Controls/Select/Select.cs", 'class Select { string example = "new Popup()"; }')
        plan = self.planner().plan(["src/Controls/Popup/Popup.cs"])
        self.assertNotIn("Tests.SelectTests", self.selected(plan))

    def test_unreferenced_project_cannot_create_a_reverse_dependency(self):
        self.write("tools/Generator/Generator.csproj", "<Project/>")
        self.write("tools/Generator/Generator.cs", "class Generator { Popup symbol; }")
        self.policy["areas"].append({"id": "generator", "roots": ["tools/Generator"], "depth": 0, "tests": ["tests/Integration.Tests/**/*.cs"]})
        plan = self.planner().plan(["src/Controls/Popup/Popup.cs"])
        self.assertNotIn("Integration.Checks", self.selected(plan))

    def test_explicit_rule_selects_integration_project_without_project_reference(self):
        plan = self.planner().plan(["inputs/catalog.xlf"])
        self.assertEqual({"Integration.Checks"}, self.selected(plan))

    def test_test_helper_change_expands_to_its_entire_project(self):
        self.write("tests/UI.Tests/TestHost.cs", "class TestHost {}")
        plan = self.planner().plan(["tests/UI.Tests/TestHost.cs"])
        self.assertEqual({"Tests.ButtonTests", "Tests.PopupTests", "Tests.SelectTests"}, self.selected(plan))

    def test_changed_test_selects_actual_class_name_not_file_name(self):
        plan = self.planner().plan(["tests/Integration.Tests/Checks.cs"])
        self.assertEqual({"Integration.Checks"}, self.selected(plan))

    def test_unknown_source_is_reported_instead_of_successfully_skipped(self):
        plan = self.planner().plan(["src/NewPackage/Feature.cs"])
        self.assertTrue(plan["gaps"])

    def test_missing_rule_suite_is_a_gap_even_when_another_suite_matches(self):
        self.policy["rules"][0]["tests"].append("tests/Missing.Tests/**/*.cs")
        plan = self.planner().plan(["inputs/catalog.xlf"])
        self.assertTrue(plan["gaps"])

    def test_deleted_source_uses_baseline_symbols_to_find_consumers(self):
        (self.root / "src/Controls/Popup/Popup.cs").unlink()
        plan = self.planner().plan(["src/Controls/Popup/Popup.cs"])
        self.assertIn("Tests.SelectTests", self.selected(plan))

    def test_deleted_orphan_path_is_ignored_not_a_gap(self):
        self.write("tools/legacy/Legacy.cs", "namespace Legacy; public class LegacyTool {}")
        self.git("add", "tools/legacy/Legacy.cs")
        self.git("commit", "-qm", "add legacy tool")
        (self.root / "tools/legacy/Legacy.cs").unlink()
        plan = self.planner().plan(["tools/legacy/Legacy.cs"])
        self.assertFalse(plan["gaps"])
        self.assertIn("tools/legacy/Legacy.cs", plan["ignored"])

    def test_full_scope_discovers_projects_missing_from_solution(self):
        self.write("AtomUI.slnx", "<Solution/>")
        plan = self.planner().plan([], scope="full")
        self.assertEqual(2, len(plan["tests"]))
        self.assertIn("Integration.Checks", self.selected(plan))

    def test_explicit_extra_tests_are_additive(self):
        plan = self.planner().plan(["src/Controls/Button/Button.cs"], extra_tests=["tests/Integration.Tests/**/*.cs"])
        self.assertEqual({"Tests.ButtonTests", "Integration.Checks"}, self.selected(plan))

    def test_stale_plan_records_its_original_snapshot(self):
        planner = self.planner()
        plan = planner.plan(["src/Controls/Button/Button.cs"])
        self.write("src/Controls/Select/Select.cs", "class Select { int changed; }")
        self.assertNotEqual(planner.repo.snapshot(), plan.get("source_fingerprint", planner.repo.snapshot()))

    def test_change_between_repository_discovery_and_planning_is_rejected(self):
        self.planner()
        repository = Repository(self.root)
        self.write("tests/UI.Tests/AddedTests.cs", "namespace Tests; class AddedTests { [Fact] void Works() {} }")
        with self.assertRaises(ValueError):
            Planner(repository, self.policy)

    def test_file_reading_test_is_selected_without_project_reference(self):
        self.write("tests/Integration.Tests/Checks.cs", 'namespace Integration; class Checks { [Fact] void Works() { File.ReadAllText(Path.Combine("docs", "build-contract.md")); } }')
        plan = self.planner().plan(["docs/build-contract.md"])
        self.assertIn("Integration.Checks", self.selected(plan))

    def test_inheritance_upgrade_propagates_after_composition_was_visited(self):
        for name, body in [("BaseType", ""), ("Middle", ": BaseType"), ("Derived", ": Middle"), ("Consumer", "")]:
            field = "BaseType item;" if name == "Derived" else "Derived item;" if name == "Consumer" else ""
            self.write("src/Controls/" + name + "/" + name + ".cs", "class " + name + " " + body + " { " + field + " }")
            self.write("tests/UI.Tests/" + name + "Tests.cs", "namespace Tests; class " + name + "Tests { [Fact] void Works() { new " + name + "(); } }")
        plan = self.planner().plan(["src/Controls/BaseType/BaseType.cs"])
        self.assertIn("Tests.ConsumerTests", self.selected(plan))

    def test_runtime_source_reading_rules_take_precedence_over_document_ignores(self):
        self.policy["rules"][0]["changes"] = ["docs/catalog/**"]
        plan = self.planner().plan(["docs/catalog/overview.md"])
        self.assertIn("Integration.Checks", self.selected(plan))

    def test_publish_obligation_is_preserved_separately_from_unit_tests(self):
        self.policy["obligations"] = [{"id": "aot", "changes": ["src/Controls/Popup/**"], "description": "Run real publish and startup"}]
        plan = self.planner().plan(["src/Controls/Popup/Popup.cs"])
        self.assertEqual(["aot"], [x["id"] for x in plan["obligations"]])

    def test_audit_reports_unowned_production_file_and_dangling_rule(self):
        self.write("src/Unknown/Other.cs", "class Other {}")
        self.policy["rules"][0]["tests"] = ["tests/Absent/**/*.cs"]
        audit = self.planner().audit()
        self.assertTrue(any("src/Unknown/Other.cs" in x for x in audit["errors"]))
        self.assertTrue(any("tests/Absent" in x for x in audit["errors"]))


class ChangeDiscoveryTests(RepositoryCase):
    def test_focused_module_directory_expands_only_its_git_eligible_files(self):
        self.write("src/Controls/Button/NewButtonPart.cs", "class NewButtonPart {}")
        repository = Repository(self.root)

        focused = repository.focus_paths(["src/Controls/Button"])

        self.assertIn("src/Controls/Button/Button.cs", focused)
        self.assertIn("src/Controls/Button/NewButtonPart.cs", focused)
        self.assertNotIn("src/Controls/Popup/Popup.cs", focused)

    def test_staged_change_hidden_by_worktree_revert_is_still_reported(self):
        self.planner()
        original = (self.root / "src/Controls/Button/Button.cs").read_text()
        self.write("src/Controls/Button/Button.cs", "class Button { int staged; }")
        self.git("add", ".")
        self.write("src/Controls/Button/Button.cs", original)
        self.assertIn("src/Controls/Button/Button.cs", Repository(self.root).changes())

    def test_staged_unstaged_untracked_and_both_rename_sides_are_included(self):
        self.planner()
        self.write("src/Controls/Button/Button.cs", "class Button { int staged; }")
        self.git("add", ".")
        self.write("src/Controls/Button/Button.cs", "class Button { int unstaged; }")
        self.git("mv", "src/Controls/Popup/Popup.cs", "src/Controls/Popup/Renamed.cs")
        self.write("src/Controls/Popup/New file.cs", "class NewPopup {}")
        changes = Repository(self.root).changes()
        self.assertTrue({"src/Controls/Button/Button.cs", "src/Controls/Popup/Popup.cs", "src/Controls/Popup/Renamed.cs", "src/Controls/Popup/New file.cs"}.issubset(set(changes)))

    def test_explicit_base_includes_committed_and_working_changes(self):
        self.planner()
        base = self.git("rev-parse", "HEAD")
        self.write("src/Controls/Button/Button.cs", "class Button { int changed; }")
        self.git("add", ".")
        self.git("commit", "-qm", "change")
        self.write("src/Controls/Select/New.cs", "class NewSelect {}")
        changes = Repository(self.root).changes(base)
        self.assertIn("src/Controls/Button/Button.cs", changes)
        self.assertIn("src/Controls/Select/New.cs", changes)


class ClassDiscoveryTests(unittest.TestCase):
    def test_combined_attribute_list_discovers_fact(self):
        self.assertIsNotNone(test_classes)
        self.assertEqual(["Tests.Combined"], test_classes('namespace Tests; class Combined { [Trait("Area", "Controls"), Fact] public void Works() {} }'))

    def test_partial_class_and_helper_are_not_inferred_from_filename(self):
        self.assertIsNotNone(test_classes, "C# test discovery has not been implemented")
        source = '''namespace Integration;
        public sealed partial class EndToEndTests {
          [Fact] public void Works() {}
          public class Helper { public void Help() {} }
        }
        [CollectionDefinition("suite")] public class Definition {}
        // [Fact] class FakeTests {}
        '''
        self.assertEqual(["Integration.EndToEndTests"], test_classes(source))

    def test_strings_with_fake_tests_and_braces_do_not_change_class_scope(self):
        self.assertIsNotNone(test_classes, "C# test discovery has not been implemented")
        source = '''namespace Tests { public class Outer {
          string sample = "class Fake { [Fact] }";
          public class Inner { [AvaloniaFact] public void Works() {} }
        }}'''
        self.assertEqual(["Tests.Outer+Inner"], test_classes(source))


if __name__ == "__main__":
    unittest.main()
