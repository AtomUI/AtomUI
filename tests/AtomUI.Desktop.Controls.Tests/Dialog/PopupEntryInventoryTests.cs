using System.Text.RegularExpressions;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Dialog;

public class PopupEntryInventoryTests
{
    private static readonly string[] RuntimeAxamlPopupPaths =
    [
        "src/AtomUI.Desktop.Controls.ColorPicker/Themes/ColorPickerTheme.axaml",
        "src/AtomUI.Desktop.Controls.ColorPicker/Themes/GradientColorPickerTheme.axaml",
        "src/AtomUI.Desktop.Controls/AutoComplete/Themes/AutoCompleteSearchEditTheme.axaml",
        "src/AtomUI.Desktop.Controls/AutoComplete/Themes/AutoCompleteTextAreaTheme.axaml",
        "src/AtomUI.Desktop.Controls/AutoComplete/Themes/AutoCompleteTheme.axaml",
        "src/AtomUI.Desktop.Controls/Cascader/Themes/CascaderTheme.axaml",
        "src/AtomUI.Desktop.Controls/ComboBox/Themes/ComboBoxTheme.axaml",
        "src/AtomUI.Desktop.Controls/DatePicker/Themes/RangeDatePickerTheme.axaml",
        "src/AtomUI.Desktop.Controls/Mentions/Themes/MentionsTheme.axaml",
        "src/AtomUI.Desktop.Controls/Menu/Themes/MenuItemTheme.axaml",
        "src/AtomUI.Desktop.Controls/Menu/Themes/TopLevelMenuItemTheme.axaml",
        "src/AtomUI.Desktop.Controls/NavMenu/Themes/NavMenuItemTheme.axaml",
        "src/AtomUI.Desktop.Controls/TabControl/Themes/TabScrollViewerTheme.axaml",
        "src/AtomUI.Desktop.Controls/Primitives/InfoPickerInput/Themes/InfoPickerInputTheme.axaml",
        "src/AtomUI.Desktop.Controls/Primitives/InfoPickerInput/Themes/RangeInfoPickerInputTheme.axaml",
        "src/AtomUI.Desktop.Controls/Select/Themes/SelectTheme.axaml",
        "src/AtomUI.Desktop.Controls/TimePicker/Themes/RangeTimePickerTheme.axaml",
        "src/AtomUI.Desktop.Controls/Tour/Themes/TourTheme.axaml",
        "src/AtomUI.Desktop.Controls/TreeSelect/Themes/TreeSelectTheme.axaml"
    ];

    private static readonly string[] RuntimeProjectDirectories =
    [
        "src/AtomUI.Desktop.Controls",
        "src/AtomUI.Desktop.Controls.ColorPicker",
        "src/AtomUI.Desktop.Controls.DataGrid",
        "src/AtomUI.Desktop.Controls.Extras"
    ];

    public static TheoryData<string, string, string, string[]> InventoryCases =>
        new()
        {
            {
                "AXAML AtomUI.Popup",
                "*.axaml",
                @"<atom:Popup(?:\s|>)",
                RuntimeAxamlPopupPaths
            },
            {
                "constructed AtomUI.Popup",
                "*.cs",
                @"\bnew\s+(?:(?:global::)?AtomUI\.Desktop\.Controls\.)?Popup\s*[({]",
                [
                    "src/AtomUI.Desktop.Controls/Menu/ContextMenu.cs",
                    "src/AtomUI.Desktop.Controls/Tooltip/ToolTip.cs"
                ]
            },
            {
                "PopupFlyoutBase family",
                "*.cs",
                @"\bclass\s+\w+\b[^:{]*:\s*(?:(?:(?:global::)?Avalonia\.Controls\.Primitives\.)?PopupFlyoutBase|(?:(?:global::)?AtomUI\.Desktop\.Controls\.)?\w*Flyout)\b",
                [
                    "src/AtomUI.Desktop.Controls.DataGrid/Column/Filters/DataGridMenuFilterFlyout.cs",
                    "src/AtomUI.Desktop.Controls.DataGrid/Column/Filters/DataGridTreeFilterFlyout.cs",
                    "src/AtomUI.Desktop.Controls/Flyouts/Flyout.cs",
                    "src/AtomUI.Desktop.Controls/Flyouts/MenuFlyout.cs",
                    "src/AtomUI.Desktop.Controls/Flyouts/TreeViewFlyout.cs",
                    "src/AtomUI.Desktop.Controls/PopupConfirm/PopupConfirmFlyout.cs"
                ]
            },
            {
                "direct ShowAt consumer",
                "*.cs",
                @"\.\s*ShowAt\s*\(",
                [
                    "src/AtomUI.Desktop.Controls/Flyouts/FlyoutStateHelper.cs",
                    "src/AtomUI.Desktop.Controls/Flyouts/MenuFlyoutPresenter.cs",
                    "src/AtomUI.Desktop.Controls/Transfer/TransferSelectDropdown.cs",
                    "src/AtomUI.Desktop.Controls/TreeView/FloatableTreeView.cs"
                ]
            },
            {
                "delegated Flyout construction",
                "*.cs",
                @"\bnew\s+(?:(?:(?:global::)?Avalonia\.Controls\.Primitives\.)?PopupFlyoutBase|(?:(?:global::)?AtomUI\.Desktop\.Controls\.)?(?:FlyoutStateHelper|FlyoutHost|Flyout|MenuFlyout|TreeViewFlyout|PopupConfirmFlyout|DataGridMenuFilterFlyout|DataGridTreeFilterFlyout))\b",
                [
                    "src/AtomUI.Desktop.Controls.DataGrid/Column/Filters/DataGridFilterIndicator.cs",
                    "src/AtomUI.Desktop.Controls/Avatar/AvatarGroup.cs",
                    "src/AtomUI.Desktop.Controls/DropdownButton/DropdownButton.cs",
                    "src/AtomUI.Desktop.Controls/Flyouts/FlyoutHost.cs",
                    "src/AtomUI.Desktop.Controls/PopupConfirm/PopupConfirm.cs",
                    "src/AtomUI.Desktop.Controls/SplitButton/SplitButton.cs",
                    "src/AtomUI.Desktop.Controls/Transfer/TransferSelectDropdown.cs"
                ]
            },
            {
                "popup type aliases",
                "*.cs",
                @"\busing\s+\w+\s*=\s*(?:(?:(?:global::)?Avalonia\.Controls\.Primitives\.)?PopupFlyoutBase|(?:(?:global::)?AtomUI\.Desktop\.Controls\.)?(?:Popup|FlyoutStateHelper|FlyoutHost|Flyout|MenuFlyout|TreeViewFlyout|PopupConfirmFlyout|DataGridMenuFilterFlyout|DataGridTreeFilterFlyout))\s*;",
                [
                    "src/AtomUI.Desktop.Controls.ColorPicker/AbstractColorPicker.cs",
                    "src/AtomUI.Desktop.Controls.DataGrid/Column/Filters/DataGridFilterIndicator.cs",
                    "src/AtomUI.Desktop.Controls/DropdownButton/DropdownButton.cs",
                    "src/AtomUI.Desktop.Controls/Flyouts/Flyout.cs",
                    "src/AtomUI.Desktop.Controls/Flyouts/FlyoutHost.cs",
                    "src/AtomUI.Desktop.Controls/Transfer/TransferSelectDropdown.cs"
                ]
            }
        };

    public static TheoryData<string, string> DelegatedFlyoutConsumerCases =>
        new()
        {
            { "src/AtomUI.Desktop.Controls/DropdownButton/DropdownButton.cs", "new FlyoutStateHelper" },
            { "src/AtomUI.Desktop.Controls/SplitButton/SplitButton.cs", "new FlyoutStateHelper" },
            { "src/AtomUI.Desktop.Controls/Avatar/AvatarGroup.cs", "new Flyout" },
            { "src/AtomUI.Desktop.Controls/Transfer/TransferSelectDropdown.cs", "new MenuFlyout" },
            { "src/AtomUI.Desktop.Controls/PopupConfirm/PopupConfirm.cs", "new PopupConfirmFlyout" },
            { "src/AtomUI.Desktop.Controls.DataGrid/Column/Filters/DataGridFilterIndicator.cs", "new FlyoutStateHelper" }
        };

    public static TheoryData<string> ConstructedPopupSurfaceOwnershipCases =>
        new()
        {
            { "src/AtomUI.Desktop.Controls/Flyouts/Flyout.cs" },
            { "src/AtomUI.Desktop.Controls/Menu/ContextMenu.cs" },
            { "src/AtomUI.Desktop.Controls/Tooltip/ToolTip.cs" }
        };

    [Theory]
    [MemberData(nameof(InventoryCases))]
    public void Popup_Entry_Inventory_Matches_The_Audited_Allowlist(
        string category,
        string searchPattern,
        string contentPattern,
        string[] expected)
    {
        AssertInventory(category, searchPattern, contentPattern, expected);
    }

    [Theory]
    [MemberData(nameof(DelegatedFlyoutConsumerCases))]
    public void Flyout_Consumers_Delegate_To_An_Audited_Shared_Primitive(string relativePath, string marker)
    {
        var source = File.ReadAllText(Path.Combine(
            GetRepositoryRoot(),
            relativePath.Replace('/', Path.DirectorySeparatorChar)));

        source.ShouldContain(marker);
    }

    [Fact]
    public void Runtime_Axaml_Popup_Entries_Do_Not_Repeat_The_Default_Surface()
    {
        foreach (var relativePath in RuntimeAxamlPopupPaths)
        {
            var source = ReadRepositoryFile(relativePath);
            var entries = Regex.Matches(
                source,
                @"<atom:Popup(?=[\s/>])[^>]*>",
                RegexOptions.CultureInvariant | RegexOptions.Multiline);

            entries.ShouldNotBeEmpty($"{relativePath} must contain an audited AtomUI Popup entry.");
            foreach (Match entry in entries)
            {
                entry.Value.ShouldNotContain("SurfaceBackground=\"{x:Null}\"");
            }
        }
    }

    [Theory]
    [MemberData(nameof(ConstructedPopupSurfaceOwnershipCases))]
    public void Runtime_Constructed_Popup_Entries_Do_Not_Repeat_The_Default_Surface(string relativePath)
    {
        var source = ReadRepositoryFile(relativePath);

        Regex.IsMatch(
            source,
            @"\bSurfaceBackground\s*=\s*null\b",
            RegexOptions.CultureInvariant).ShouldBeFalse(
            $"{relativePath} must inherit the Popup primitive's null surface default.");
    }

    [Fact]
    public void Popup_Theme_Does_Not_Provide_A_Default_Surface()
    {
        var popupThemeSource = ReadRepositoryFile(
            "src/AtomUI.Desktop.Controls/Popup/Themes/PopupTheme.axaml");
        popupThemeSource.ShouldNotContain("<Setter Property=\"SurfaceBackground\"");
    }

    private static void AssertInventory(
        string category,
        string searchPattern,
        string contentPattern,
        IReadOnlyCollection<string> expected)
    {
        var regex = new Regex(contentPattern, RegexOptions.CultureInvariant | RegexOptions.Multiline);
        var actual = RuntimeProjectDirectories
                     .SelectMany(directory => Directory.EnumerateFiles(
                          Path.Combine(GetRepositoryRoot(), directory),
                          searchPattern,
                          SearchOption.AllDirectories))
                     .Where(path => regex.IsMatch(File.ReadAllText(path)))
                     .Select(ToRepositoryRelativePath)
                     .Order(StringComparer.Ordinal)
                     .ToArray();
        var expectedSorted = expected.Order(StringComparer.Ordinal).ToArray();

        actual.ShouldBe(
            expectedSorted,
            $"{category} inventory changed. Classify every added path in the popup family matrix, " +
            "or remove obsolete allowlist entries together with their regression coverage.");
    }

    private static string ToRepositoryRelativePath(string path)
    {
        return Path.GetRelativePath(GetRepositoryRoot(), path).Replace('\\', '/');
    }

    private static string ReadRepositoryFile(string relativePath)
    {
        return File.ReadAllText(Path.Combine(
            GetRepositoryRoot(),
            relativePath.Replace('/', Path.DirectorySeparatorChar)));
    }

    private static string GetRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "AtomUI.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
               ?? throw new DirectoryNotFoundException("Unable to locate the AtomUI repository root.");
    }
}
