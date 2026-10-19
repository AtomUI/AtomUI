"""Permanent-test admission checks for known low-value test shapes."""
import re


_GALLERY_SHOWCASE_ROOT = "tests/AtomUIGallery.Tests/ShowCases/"
_TEMPORARY_SEGMENT = "/Temporary/"
_PRIVATE_SOURCE_ARCHITECTURE_ALLOWLIST = {
    "tests/AtomUI.Desktop.Controls.Tests/Menu/MenuTitleBarPopupTests.cs",
    "tests/AtomUI.Desktop.Controls.Tests/Upload/UploadTriggerTests.cs",
    "tests/AtomUIGallery.Tests/Controls/GalleryStickyTabsHostTests.cs",
    "tests/AtomUIGallery.Tests/ShowCases/TabOverflowSearchShowCaseTests.cs",
    "tests/AtomUIGallery.Tests/Workspace/BrowserGalleryConventionsTests.cs",
}


def _violation(category, path, detail):
    return f"Low-value permanent test [{category}]: {path}: {detail}"


def audit_low_value_permanent_tests(repository):
    """Return deterministic audit errors for prohibited permanent test shapes.

    These checks are deliberately path- and intent-specific. Source inspection,
    snapshots, reflection and literal assertions remain available for durable
    architecture or behavior contracts outside the prohibited shapes below.
    """
    errors = []
    for path in repository.files:
        if (not repository.is_file(path) or
                not path.startswith("tests/") or
                _TEMPORARY_SEGMENT in path):
            continue

        if (path.startswith(_GALLERY_SHOWCASE_ROOT) and
                path.endswith("Examples.snapshot")):
            errors.append(_violation(
                "gallery-copy-snapshot",
                path,
                "Gallery example markup is product copy, not a permanent contract"))
            continue

        if not path.endswith(".cs"):
            continue
        source = repository.text(path)

        if re.search(
                r"\b\w+_ShowCase_(?:Examples|Themes|Scenarios)_Match_Approved_"
                r"(?:Control_Demo|Gallery)_Content\s*\(",
                source):
            errors.append(_violation(
                "gallery-copy-snapshot",
                path,
                "approved demo-content snapshots duplicate editable Gallery markup"))

        if (path.startswith(_GALLERY_SHOWCASE_ROOT) and
                re.search(r"\b\w+_ShowCase_Uses_Document_Layout_\w*\s*\(", source)):
            errors.append(_violation(
                "gallery-source-mirror",
                path,
                "document-layout source mirrors freeze example composition and copy"))

        if (path.startswith(_GALLERY_SHOWCASE_ROOT) and
                re.search(r"(?:ComponentCategory|ComponentStatus|BadgeText)\s*=\\?\"", source) and
                "ShouldContain" in source):
            errors.append(_violation(
                "gallery-marketing-copy",
                path,
                "category, status and version badge text are non-contractual Gallery metadata"))

        if ("/Localization/" in path and
                re.search(r"record\s+struct\s+ExpectedEntry", source) and
                "new ExpectedEntry" in source):
            errors.append(_violation(
                "localization-literal-table",
                path,
                "manual multi-language truth tables duplicate the XLIFF catalogs"))

        if ("/Localization/" in path and re.search(
                r"(?:catalogTypes|files|sourceFiles)\.Length\.ShouldBe\(\d+\)|"
                r"sourceFiles\.Sum\(CountUnits\)\.ShouldBe\(\d+\)",
                source)):
            errors.append(_violation(
                "repository-inventory-total",
                path,
                "hard-coded repository totals fail on intentional catalog growth"))

        if re.search(r"\breadme\s*\.\s*ShouldContain\(", source, re.IGNORECASE):
            errors.append(_violation(
                "documentation-copy",
                path,
                "human-facing README prose is not a product behavior contract"))

        if (re.search(r"\b\w*(?:No_Longer|Without_The_Legacy)\w*\s*\(", source) and
                ("ShouldNotContain" in source or "ShouldBeFalse" in source)):
            errors.append(_violation(
                "migration-tombstone",
                path,
                "completed migrations do not need permanent old-symbol absence tests"))

        if ("Enum.GetNames" in source and re.search(
                r"Enum\.GetNames(?:<[^>]+>\(\)|\([^)]*\))\.ShouldBe\(\s*"
                r"(?:\[|new\s*\[|expectedKeys\b|memberOrderBaseline\b)",
                source)):
            errors.append(_violation(
                "api-inventory",
                path,
                "exact enum member inventories duplicate compilation and package ApiCompat"))

        if (path not in _PRIVATE_SOURCE_ARCHITECTURE_ALLOWLIST and
                "File.ReadAllText" in source and
                re.search(
                    r"Should(?:Not)?Contain\(\s*\"(?:_[A-Za-z]\w*|"
                    r"(?:Notify|Configure|TryTakeOver|Request|ApplyPending)[A-Za-z]\w*)",
                    source) and
                not re.search(
                    r"SemanticPart|Semantic Part|ControlTheme|DynamicResource|"
                    r"Style(?:Class|Route)|NoCodeFallback",
                    source)):
            errors.append(_violation(
                "private-source-shape",
                path,
                "private identifiers and call structure are implementation details"))

    return sorted(set(errors))
