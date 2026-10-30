"""Reject permanent tests that only detect intentional source/content changes."""
import sys
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))

from affected.test_value_audit import audit_low_value_permanent_tests


class FakeRepository:
    def __init__(self, files, missing=None):
        self._files = files
        self.files = sorted(files)
        self._missing = set(missing or ())

    def text(self, path):
        return self._files[path]

    def is_file(self, path):
        return path not in self._missing


class PermanentTestValueAuditTests(unittest.TestCase):
    def audit(self, path, source=""):
        return audit_low_value_permanent_tests(FakeRepository({path: source}))

    def assert_rejected(self, category, path, source=""):
        errors = self.audit(path, source)
        self.assertTrue(errors, f"Expected {path} to be rejected")
        self.assertTrue(any(category in error for error in errors), errors)

    def test_rejects_gallery_example_snapshot_artifacts(self):
        self.assert_rejected(
            "gallery-copy-snapshot",
            "tests/AtomUIGallery.Tests/ShowCases/ButtonShowCaseExamples.snapshot",
            "<Button Content=\"Primary\" />")

    def test_ignores_tracked_files_deleted_from_the_worktree(self):
        path = "tests/AtomUIGallery.Tests/ShowCases/ButtonShowCaseExamples.snapshot"
        repository = FakeRepository({path: "old snapshot"}, missing={path})

        self.assertEqual([], audit_low_value_permanent_tests(repository))

    def test_rejects_gallery_approved_content_snapshot_methods(self):
        self.assert_rejected(
            "gallery-copy-snapshot",
            "tests/AtomUIGallery.Tests/ShowCases/ButtonShowCasePageTests.cs",
            """class Tests {
                [Fact]
                public void Button_ShowCase_Examples_Match_Approved_Control_Demo_Content() {}
            }""")

    def test_rejects_gallery_document_layout_source_mirrors(self):
        self.assert_rejected(
            "gallery-source-mirror",
            "tests/AtomUIGallery.Tests/ShowCases/ButtonShowCasePageTests.cs",
            """class Tests {
                [Fact]
                public void Button_ShowCase_Uses_Document_Layout_With_Examples() {}
            }""")

    def test_rejects_gallery_marketing_metadata_literals(self):
        self.assert_rejected(
            "gallery-marketing-copy",
            "tests/AtomUIGallery.Tests/ShowCases/ButtonShowCasePageTests.cs",
            'class Tests { [Fact] void Copy() { source.ShouldContain("ComponentCategory=\\\"General\\\""); source.ShouldContain("BadgeText=\\\"v6.2.0\\\""); } }')

    def test_rejects_manual_translation_truth_tables(self):
        self.assert_rejected(
            "localization-literal-table",
            "tests/Controls.Tests/Localization/CatalogCoverageTests.cs",
            """class Tests {
                [Fact] void Copy() { AssertCatalog(new ExpectedEntry(Kind.Ok, "OK", "确定", "確定")); }
                private record struct ExpectedEntry(Kind Kind, string En, string ZhCn, string ZhTw);
            }""")

    def test_rejects_hard_coded_repository_inventory_totals(self):
        self.assert_rejected(
            "repository-inventory-total",
            "tests/Gallery.Tests/Localization/GalleryCatalogCoverageTests.cs",
            "class Tests { [Fact] void Count() { sourceFiles.Sum(CountUnits).ShouldBe(4786); } }")

    def test_rejects_readme_prose_assertions(self):
        self.assert_rejected(
            "documentation-copy",
            "tests/Build.Tests/LanguagePackTemplateTests.cs",
            'class Tests { [Fact] void Copy() { var readme = File.ReadAllText("README.md"); readme.ShouldContain("optional"); } }')

    def test_rejects_completed_migration_source_tombstones(self):
        self.assert_rejected(
            "migration-tombstone",
            "tests/Gallery.Tests/ShowCases/SplashShowCasePageTests.cs",
            'class Tests { [Fact] void Splash_ShowCase_No_Longer_Ships_Dedicated_Window_Controls() { source.ShouldNotContain("OldWindow"); } }')

    def test_rejects_exact_enum_name_inventories(self):
        self.assert_rejected(
            "api-inventory",
            "tests/Controls.Tests/Upload/UploadContractTests.cs",
            'class Tests { [Fact] void Members() { Enum.GetNames<UploadKind>().ShouldBe(new[] { "File", "Folder" }); } }')

    def test_rejects_private_source_implementation_assertions(self):
        self.assert_rejected(
            "private-source-shape",
            "tests/Controls.Tests/Window/WindowResizeArtifactTests.cs",
            'class Tests { [Fact] void Uses_Internal_Hook() { var source = File.ReadAllText("Window.cs"); source.ShouldContain("_isDragging"); source.ShouldNotContain("NotifyResizeStarted"); } }')

    def test_allows_behavior_architecture_package_and_temporary_tests(self):
        allowed = {
            "tests/Controls.Tests/ButtonBehaviorTests.cs":
                "class Tests { [Fact] void Click_Raises_Command() { button.RaiseClick(); calls.ShouldBe(1); } }",
            "tests/Controls.Tests/SemanticPartTests.cs":
                'class Tests { [Fact] void StyleRoute() { var source = File.ReadAllText("Demo.axaml"); source.ShouldContain("TreeSelectPopupListStyle"); source.ShouldNotContain("Loaded=\\\""); } }',
            "tests/Localization.Tests/XliffContractTests.cs":
                "class Tests { [Fact] void Targets() { target.Value.ShouldNotBeEmpty(); ExtractPlaceholders(target.Value).ShouldBe(expected); } }",
            "tests/Integration.Tests/PackageConsumerTests.cs":
                "class Tests { [Fact] async Task ConsumesPackage() { await RunProcess(\"dotnet\", \"run\"); } }",
            "tests/AtomUI.Desktop.Controls.Tests/Menu/MenuTitleBarPopupTests.cs":
                'class Tests { [Fact] void Routing() { var source = File.ReadAllText("Menu.cs"); source.ShouldContain("_dismiss()"); } }',
            "tests/Controls.Tests/Upload/UploadFormValueTests.cs":
                "class Tests { [Fact] void Value() { files.Length.ShouldBe(1); } }",
            "tests/Controls.Tests/Temporary/bug-123/ProbeTests.cs":
                "class Tests { [Fact] void Button_ShowCase_Uses_Document_Layout_With_Examples() {} }",
        }

        self.assertEqual([], audit_low_value_permanent_tests(FakeRepository(allowed)))


if __name__ == "__main__":
    unittest.main()
