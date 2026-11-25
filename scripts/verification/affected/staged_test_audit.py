"""Check only the test changes that would enter the next local commit."""
import re
import subprocess
from pathlib import PurePosixPath

from .repository import csharp_comments, test_classes


# Keep rejecting additions to the retired permanent rendering suite.
_RENDERING_PROJECT = "tests/AtomUI.Desktop.Controls.Rendering.Tests/"
_CONTROL_TEST_PATHS = (
    "tests/AtomUI.Controls.Tests/",
    "tests/AtomUI.Desktop.Controls.Tests/",
    "tests/AtomUI.Desktop.Controls.DataGrid.Tests/",
    "tests/AtomUI.Toolkits.GalleryBase.Tests/Controls/",
    "tests/AtomUI.Toolkits.GalleryBase.Tests/Shell/",
    "tests/AtomUIGallery.Tests/",
    "tests/AtomUI.DesignLanguages.PrimeNG.Tests/PrimeNGBehaviorTokenTests.cs",
    "tests/AtomUI.DesignLanguages.PrimeNG.Tests/PrimeNGControlGlobalTests.cs",
    "tests/AtomUI.DesignLanguages.Shadcn.Tests/TokenValues/ShadcnControlTests.cs",
    "tests/AtomUI.DesignLanguages.Shadcn.Tests/TokenValues/ShadcnOverlayTests.cs",
)
_CORE_CONTRACT_PATHS = (
    # These paths cover shared mechanisms in suites that formerly also tested concrete controls.
    "tests/AtomUI.Desktop.Controls.DataGrid.Tests/Data/",
    "tests/AtomUI.Desktop.Controls.DataGrid.Tests/DataMemberAccessors/",
    "tests/AtomUI.Desktop.Controls.DataGrid.Tests/Lifecycle/DataGridLocalSourceLifetimeTests.cs",
    "tests/AtomUI.Desktop.Controls.DataGrid.Tests/Selection/DataGridSelectionStateTests.cs",
    "tests/AtomUI.Desktop.Controls.DataGrid.Tests/Virtualization/",
    "tests/AtomUI.Controls.Tests/ImageLoading/SvgImageCodecTests.cs",
    "tests/AtomUI.Controls.Tests/Motion/MotionGateTests.cs",
    "tests/AtomUI.Controls.Tests/FixedAvaloniaMemberRetentionTests.cs",
    "tests/AtomUI.Toolkits.GalleryBase.Tests/Controls/GalleryShowCaseRuntimeOptionsTests.cs",
    "tests/AtomUI.Toolkits.GalleryBase.Tests/Controls/SemanticPartHighlightSessionTests.cs",
    "tests/AtomUI.Toolkits.GalleryBase.Tests/Controls/SemanticPartTargetResolverTests.cs",
    "tests/AtomUI.Toolkits.GalleryBase.Tests/Shell/GalleryWorkspaceViewModelAppearanceTests.cs",
    "tests/AtomUI.Toolkits.GalleryBase.Tests/Shell/GalleryWorkspaceViewModelLanguageTests.cs",
    "tests/AtomUIGallery.Tests/Controls/GalleryReactiveUserControlTests.cs",
    "tests/AtomUIGallery.Tests/Localization/",
    "tests/AtomUIGallery.Tests/Theme/GeneratedStartupQueryTests.cs",
    "tests/AtomUIGallery.Tests/Toolkits/",
    "tests/AtomUIGallery.Tests/Workspace/GalleryDeveloperToolsConventionsTests.cs",
    "tests/AtomUIGallery.Tests/Workspace/GalleryInstallerAssetTests.cs",
    "tests/AtomUIGallery.Tests/Workspace/GalleryReleaseWorkflowTests.cs",
    "tests/AtomUIGallery.Tests/Workspace/GalleryVersionInfoTests.cs",
)
_BASELINE_DIRS = {"baseline", "baselines", "golden", "goldens", "screenshots", "snapshots", "visualbaselines"}
_IMAGE_SUFFIXES = {".bmp", ".gif", ".jpeg", ".jpg", ".png", ".svg", ".tif", ".tiff", ".webp"}
_TEMPORARY_TEST_MARKER = re.compile(r"^[\s/*]*临时验证(?=[\s，,：:。]|\*/|$)", re.M)


def audit_staged_tests(root):
    def git(*args):
        result = subprocess.check_output(["git", "-C", str(root), *args], stderr=subprocess.PIPE)
        return result.decode("utf-8", errors="surrogateescape")

    entries = git(
        "diff", "--cached", "--numstat", "-z", "--no-renames", "--no-ext-diff", "--no-textconv",
        "--ignore-space-change", "--ignore-blank-lines", "--diff-filter=AMT", "--", "tests/",
    ).split("\0")
    errors = []
    for entry in filter(None, entries):
        added, _, path = entry.split("\t", 2)
        parts = PurePosixPath(path)
        directories = {part.casefold() for part in parts.parts[:-1]}
        suffix = parts.suffix.casefold()
        rendering = path.startswith(_RENDERING_PROJECT)
        control = path.startswith(_CONTROL_TEST_PATHS) and not path.startswith(_CORE_CONTRACT_PATHS)
        snapshot = (rendering or control) and suffix in {".baseline", ".snapshot"}
        baseline_image = suffix in _IMAGE_SUFFIXES and (rendering or bool(directories & _BASELINE_DIRS))

        if "temporary" in directories:
            errors.append(f"{path}: remove temporary test files before committing")
        elif snapshot or baseline_image:
            errors.append(f"{path}: keep visual baselines with temporary acceptance files")
        elif suffix == ".cs":
            source = git("cat-file", "blob", f":{path}")
            if any(_TEMPORARY_TEST_MARKER.search(comment) for comment in csharp_comments(source)):
                errors.append(f"{path}: remove the code marked as temporary verification before committing")
            elif added != "0" and (rendering or (control and test_classes(source))):
                errors.append(f"{path}: run concrete-control tests temporarily and remove them after acceptance")
    return errors
