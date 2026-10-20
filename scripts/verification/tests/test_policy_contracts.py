"""Guard real cross-project boundaries that have caused selection omissions."""
import json
import sys
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
from affected.planner import Planner
from affected.repository import Repository


class RealPolicyContracts(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.root = Path(__file__).resolve().parents[3]
        cls.planner = Planner(Repository(cls.root), json.loads((cls.root / "scripts/verification/test-policy.json").read_text()))

    def test_all_current_production_roots_and_test_projects_have_selection_owners(self):
        self.assertEqual([], self.planner.audit()["errors"])

    def test_full_regression_script_selects_verification_tooling_without_policy_gaps(self):
        changed = "scripts/run-full-regression.sh"
        plan = self.planner.plan([changed])
        self.assertEqual([], plan["gaps"])
        self.assertTrue(plan["tooling"])
        self.assertNotIn(changed, plan["ignored"])

    def test_source_reading_consumers_are_selected_across_project_boundaries(self):
        cases = [
            ("controlgallery/AtomUIGallery/ShowCases/DataEntry/LineEdit/Views/LineEditShowCase.axaml", "ShowCaseScenarioControllerConventionsTests"),
            ("controlgallery/AtomUIGallery/ShowCases/General/Button/Views/ButtonShowCase.axaml", "ButtonIconOnlyVisualContractTests"),
            ("tests/fixtures/LanguagePackEndToEnd/Module/Localization/Welcome/en-US.xlf", "LanguagePackEndToEndTests"),
            ("tests/AtomUI.Core.Tests/Localization/Generated/en-US.xlf", "ApplicationLocalizationStartupTests"),
            ("build/Versions.props", "GalleryVersionInfoTests"),
            (".github/workflows/release-nuget-packages.yml", "GalleryBasePackagingTests"),
            ("src/AtomUI.Toolkits.GalleryBase/Controls/Themes/ShowCasePanelTheme.axaml", "ShowCasePanelStructureTests"),
            ("controlgallery/AtomUIGallery.Desktop/Program.cs", "AtomUIGalleryModuleTests"),
        ]
        for changed, expected in cases:
            with self.subTest(changed=changed):
                plan = self.planner.plan([changed])
                names = {c.rsplit(".", 1)[-1] for job in plan["tests"] for c in job["classes"]}
                self.assertIn(expected, names)

    def test_localization_integration_acceptance_is_selected_by_contract_boundary(self):
        cases = [
            (
                "src/AtomUI.Localization/Services/LanguageManager.cs",
                set(),
            ),
            (
                "src/LanguagePacks/pt-BR/AtomUI.Controls.I18n.PtBR/Localization/Common/pt-BR.xlf",
                {"OfficialPtBrLanguagePackTests"},
            ),
            (
                "src/AtomUI.Controls/Localization/Common/en-US.xlf",
                {"OfficialPtBrLanguagePackTests"},
            ),
            (
                "src/AtomUI.LanguagePack.Template/content/AtomUI.LanguagePack/AtomUI.LanguagePack.csproj",
                {"LanguageTemplateExportAcceptanceTests"},
            ),
            (
                "tests/fixtures/LanguagePackEndToEnd/Module/Localization/Welcome/en-US.xlf",
                {
                    "LanguagePackEndToEndTests",
                    "LanguageTemplateExportAcceptanceTests",
                },
            ),
            (
                "build/OutputPaths.props",
                {
                    "LanguagePackEndToEndTests",
                    "LanguageTemplateExportAcceptanceTests",
                    "OfficialPtBrLanguagePackTests",
                },
            ),
        ]
        integration_project = (
            "tests/AtomUI.Localization.IntegrationTests/"
            "AtomUI.Localization.IntegrationTests.csproj"
        )
        for changed, expected in cases:
            with self.subTest(changed=changed):
                plan = self.planner.plan([changed])
                classes = {
                    test_class.rsplit(".", 1)[-1]
                    for job in plan["tests"]
                    if job["project"] == integration_project
                    for test_class in job["classes"]
                }
                self.assertEqual(expected, classes)

    def test_registration_and_shared_generator_inputs_preserve_publish_obligations(self):
        for changed in ["src/AtomUI.Core/Theme/ControlPackageRegistration.cs", "src/AtomUI.Desktop.Controls/AtomUI.Desktop.Controls.csproj", "src/AtomUI.Toolchain/Common/SourceGeneration/GeneratedCodeNamespace.cs"]:
            with self.subTest(changed=changed):
                self.assertIn("native-aot", {o["id"] for o in self.planner.plan([changed])["obligations"]})

    def test_typemap_backend_and_fixture_changes_select_backend_tests_and_publish_checks(self):
        for changed in ["src/AtomUI.Toolchain/Backends/ILLink10/RegistrationAbi.cs",
                        "tests/AtomUI.TypeMap.Linker.Tests/Fixtures/Contracts/FixtureContracts.cs"]:
            with self.subTest(changed=changed):
                plan = self.planner.plan([changed])
                self.assertEqual([], plan["gaps"])
                self.assertIn("tests/AtomUI.TypeMap.Linker.Tests/AtomUI.TypeMap.Linker.Tests.csproj",
                              {job["project"] for job in plan["tests"]})
                self.assertIn("native-aot", {item["id"] for item in plan["obligations"]})

    def test_ordinary_package_consumers_require_package_and_publish_validation(self):
        for changed in ["tests/AtomUI.Registration.Fixtures/PackageConsumers/Host/Host.csproj",
                        "tests/AtomUI.Registration.Fixtures/run_package_consumers.py"]:
            with self.subTest(changed=changed):
                plan = self.planner.plan([changed])
                self.assertEqual([], plan["gaps"])
                self.assertTrue({"native-aot", "package-layout"}.issubset({o["id"] for o in plan["obligations"]}))
                names = {c.rsplit(".", 1)[-1] for job in plan["tests"] for c in job["classes"]}
                self.assertIn("RegistrationSourceConsumerTests", names)

    def test_leaf_control_does_not_expand_to_repository_wide_tests(self):
        plan = self.planner.plan(["src/AtomUI.Desktop.Controls/Badge/CountBadge.cs"])
        projects = {j["project"] for j in plan["tests"]}
        self.assertNotIn("tests/AtomUI.Localization.IntegrationTests/AtomUI.Localization.IntegrationTests.csproj", projects)
        self.assertNotIn("tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj", projects)
        self.assertLess(len(projects), plan["inventory"]["projects"])


if __name__ == "__main__":
    unittest.main()
