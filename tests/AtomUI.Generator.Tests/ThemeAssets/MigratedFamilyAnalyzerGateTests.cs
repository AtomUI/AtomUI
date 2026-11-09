using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Shouldly;
using Xunit;

namespace AtomUI.Generator.Tests.ThemeAssets;

/// <summary>
/// Error gate for control families whose theme migration is complete. The control-code analyzers (ATOMUIGEN043/044/047)
/// are raised to errors per directory in <c>src/AtomUI.Desktop.Controls/.editorconfig</c>; the generator-reported
/// diagnostics (ATOMUIGEN041/042/045) do not take path-scoped <c>.editorconfig</c> severities, so this test fails
/// the build for them instead: it runs the theme-asset analysis over the family's real AXAML with the package's real
/// Token identities, and reads the generated consumption map for Unreferenced Own Tokens. The test project references
/// AtomUI.Controls and AtomUI.Desktop.Controls for build order only, so those generated files are current whenever the
/// tests are built.
/// </summary>
public class MigratedFamilyAnalyzerGateTests
{
    private const string DesktopControls = "src/AtomUI.Desktop.Controls";

    /// <summary>Directories (relative to the package) of migrated families.</summary>
    private static readonly string[] s_familyDirectories =
    [
        // Button family
        "Buttons", "DropdownButton", "SplitButton", "FloatButton", "OptionButtonGroup",
        // Input family (InputControlFrame's theme lives with AddOnDecoratedBox)
        "Input", "ButtonSpinner", "NumericUpDown", "AutoComplete", "Primitives/AddOnDecoratedBox",
        // Select family
        "Select", "ComboBox", "Cascader", "TreeSelect",
        // Choice family
        "CheckBox", "RadioButton", "Switch", "Slider", "Rate",
        // Steps / Progress family
        "ProgressBar", "Steps",
        // Tag / Badge family
        "Tag", "Badge",
        // Feedback family
        "Alert", "Popup", "Tooltip", "Dialog", "MessageBox", "Message", "Notifications", "PopupConfirm",
        // Navigation family
        "NavMenu", "Menu", "TabControl", "Breadcrumb", "Pagination", "Tour",
        // Layout / container family
        "Card", "Collapse", "Expander", "GroupBox", "Splitter", "SplitView", "Separator", "Space", "Masonry",
        // Data family (first half)
        "ListBox", "ListView", "TreeView", "Skeleton"
    ];

    /// <summary>Single theme assets (relative to the package) of migrated families whose directory holds other families.</summary>
    private static readonly string[] s_familyFiles =
    [
        // Navigation family: the drop-down flyout menu presenter (Flyouts also holds FlyoutHost and the TreeView flyout).
        "Flyouts/Themes/MenuFlyoutPresenterTheme.axaml"
    ];

    /// <summary>Token identities declared in those directories.</summary>
    private static readonly string[] s_familyControls =
    [
        "Button", "HyperLinkButton", "IconButton", "ToggleIconButton", "DropdownButton", "SplitButton", "FloatButton",
        "BackTopFloatButton", "FloatButtonGroup", "FloatButtonHost", "FloatButtonGroupHost", "BackTopFloatButtonHost",
        "OptionButton", "OptionButtonGroup",
        "LineEdit", "TextBox", "TextArea", "SearchEdit", "ButtonSpinner", "NumericUpDown", "AutoComplete",
        "AutoCompleteSearchEdit", "AutoCompleteTextArea",
        "Select", "ComboBox", "ComboBoxItem", "Cascader", "CascaderView", "CascaderViewItem", "TreeSelect",
        "CheckBox", "CheckBoxGroup", "RadioButton", "RadioButtonGroup", "ToggleSwitch", "Slider", "SliderTrack",
        "SliderThumb", "Rate",
        "ProgressBar", "StepsProgressBar", "CircleProgress", "DashboardProgress", "Steps", "StepsItem",
        "Tag", "CheckableTag", "CheckableTagGroup", "CountBadge", "DotBadge", "RibbonBadge",
        "Alert", "Popup", "ToolTip", "Dialog", "DialogButtonBox", "MessageBox", "MessageCard", "WindowMessageManager",
        "NotificationCard", "WindowNotificationManager", "PopupConfirm",
        "NavMenu", "HorizontalNavMenuItemHeader", "InlineNavMenuItemHeader", "Menu", "MenuItem", "MenuItemGroup",
        "MenuSeparator", "ContextMenu", "MenuFlyoutPresenter",
        "TabControl", "CardTabControl", "TabStrip", "CardTabStrip", "TabItem", "TabStripItem",
        "Breadcrumb", "BreadcrumbItem", "Pagination", "SimplePagination", "Tour", "TourStep", "DefaultTourIndicator",
        "TextTourIndicator",
        "Card", "CardActionButton", "CardGridContent", "CardGridItem", "CardMetaContent", "CardTabsContent", "Collapse",
        "CollapseItem", "Expander", "GroupBox", "Splitter", "SplitView", "Separator", "Space", "CompactSpace",
        "CompactSpaceAddOn", "Masonry",
        "ListBox", "ListBoxItem", "ListView", "ListViewItem", "TreeView", "TreeViewItem", "Skeleton", "SkeletonAvatar",
        "SkeletonButton", "SkeletonElement", "SkeletonImage", "SkeletonInput", "SkeletonLine", "SkeletonNode",
        "SkeletonParagraph"
    ];

    public static IEnumerable<object[]> FamilyAssets() =>
        s_familyDirectories
            .SelectMany(directory => Directory.EnumerateFiles(
                Path.Combine(RepositoryRoot, DesktopControls, directory), "*.axaml", SearchOption.AllDirectories))
            .Concat(s_familyFiles.Select(static file => Path.Combine(RepositoryRoot, DesktopControls, file)))
            .Select(path => Path.GetRelativePath(RepositoryRoot, path).Replace('\\', '/'))
            .Order(StringComparer.Ordinal)
            .Select(static path => new object[] { path });

    [Theory]
    [MemberData(nameof(FamilyAssets))]
    public void Family_Theme_Assets_Have_No_Theme_Consumption_Diagnostics(string relativePath)
    {
        var text = new FileText(Path.Combine(RepositoryRoot, relativePath));
        var asset = ThemeAssetInfo.Create(text, Path.Combine(RepositoryRoot, DesktopControls), null,
            TestContext.Current.CancellationToken, out var root);
        root.ShouldNotBeNull($"{relativePath} is not well-formed AXAML.");

        var diagnostics = new List<Diagnostic>();
        ThemeAssetConsumptionAnalyzer.Analyze(asset, root, IdentityControls.Value, diagnostics.Add);

        diagnostics.Select(static d => $"{d.Id} {d.Location.GetLineSpan().StartLinePosition}: {d.GetMessage()}")
                   .ShouldBeEmpty();
    }

    [Fact]
    public void Family_Transition_Durations_Are_Declared_With_TokenTransitions()
    {
        // ATOMUIGEN041 reports SharedTokenResourceValue through the theory above; this guards that the family really
        // carries Token-declared durations, so the gate cannot pass because the transitions were dropped.
        var texts = FamilyAssets().Select(static row => File.ReadAllText(Path.Combine(RepositoryRoot, (string)row[0])))
                                  .ToArray();
        texts.ShouldAllBe(static text => !text.Contains("SharedTokenResourceValue"));
        texts.Sum(static text => Regex.Matches(text, @"atom:TokenTransitions\.Duration=""MotionDuration\w+""").Count)
             .ShouldBeGreaterThan(0);
    }

    [Fact]
    public void The_Identity_Set_Contains_Identity_Only_Family_Controls()
    {
        // Sanity: a control without Own Tokens still has a Token identity, so its theme must use its own TokenResource.
        IdentityControls.Value.ShouldContain("IconButton");
        IdentityControls.Value.ShouldContain("OptionButtonGroup");
        IdentityControls.Value.ShouldContain("SearchEdit");
        IdentityControls.Value.ShouldContain("NumericUpDown");
        IdentityControls.Value.ShouldNotContain("FloatButtonItemsControl");
        // Input primitives have no identity and read their owner's globals.
        IdentityControls.Value.ShouldNotContain("InputControlFrame");
        IdentityControls.Value.ShouldNotContain("AddOnDecoratedBox");
        // Select family: the drop-down view, its items and the ComboBox item are registered; the shared select parts are not.
        IdentityControls.Value.ShouldContain("ComboBoxItem");
        IdentityControls.Value.ShouldContain("CascaderViewItem");
        IdentityControls.Value.ShouldNotContain("SelectHandle");
        IdentityControls.Value.ShouldNotContain("SelectCandidateListItem");
        // Choice family: the Slider's track and thumb are registered; the indicators, the knob and the rate items are not.
        IdentityControls.Value.ShouldContain("SliderThumb");
        IdentityControls.Value.ShouldContain("SliderTrack");
        IdentityControls.Value.ShouldNotContain("CheckBoxIndicator");
        IdentityControls.Value.ShouldNotContain("RadioIndicator");
        IdentityControls.Value.ShouldNotContain("SwitchKnob");
        IdentityControls.Value.ShouldNotContain("RateItem");
        // Steps / Progress family: every progress shape is registered; the shared abstract bases are not.
        IdentityControls.Value.ShouldContain("CircleProgress");
        IdentityControls.Value.ShouldContain("StepsProgressBar");
        IdentityControls.Value.ShouldNotContain("AbstractProgressBar");
        IdentityControls.Value.ShouldNotContain("AbstractLineProgress");
        // StepsItem is registered (Token-owner transparent); the indicator, panels and arrows are not.
        IdentityControls.Value.ShouldContain("StepsItem");
        IdentityControls.Value.ShouldNotContain("StepsItemIndicator");
        IdentityControls.Value.ShouldNotContain("StepsPanelArrow");
        // Tag / Badge family: the tags and badges are registered; the group's items control and the badge indicators
        // (adorners, dot indicator) are not.
        IdentityControls.Value.ShouldContain("CheckableTag");
        IdentityControls.Value.ShouldContain("RibbonBadge");
        IdentityControls.Value.ShouldNotContain("CheckableTagItemsControl");
        IdentityControls.Value.ShouldNotContain("CountBadgeAdorner");
        IdentityControls.Value.ShouldNotContain("DotBadgeIndicator");
        // Feedback family: Popup (Token-owner transparent) and ToolTip are registered; the popup hosts and surface are not.
        IdentityControls.Value.ShouldContain("Popup");
        IdentityControls.Value.ShouldContain("ToolTip");
        IdentityControls.Value.ShouldNotContain("PopupSurface");
        IdentityControls.Value.ShouldNotContain("OverlayPopupHost");
        // Dialog session controls and the cards are registered; the dialog surface parts and the confirm container are not.
        IdentityControls.Value.ShouldContain("DialogButtonBox");
        IdentityControls.Value.ShouldContain("WindowMessageManager");
        IdentityControls.Value.ShouldNotContain("DialogSurface");
        IdentityControls.Value.ShouldNotContain("OverlayDialogHeader");
        IdentityControls.Value.ShouldNotContain("DialogCaptionButton");
        IdentityControls.Value.ShouldNotContain("MessageBoxContent");
        IdentityControls.Value.ShouldNotContain("PopupConfirmContainer");
        IdentityControls.Value.ShouldNotContain("NotificationProgressBar");
        // Navigation family: the NavMenu's horizontal and inline headers are registered (Token-owner transparent); its
        // items, groups, dividers, popup frame and vertical header are not. Menu items, groups, separators, context menus and
        // the flyout presenter are registered; the popup scroll host is not.
        IdentityControls.Value.ShouldContain("HorizontalNavMenuItemHeader");
        IdentityControls.Value.ShouldContain("InlineNavMenuItemHeader");
        IdentityControls.Value.ShouldNotContain("NavMenuItem");
        IdentityControls.Value.ShouldNotContain("VerticalNavMenuItemHeader");
        IdentityControls.Value.ShouldNotContain("NavMenuPopupFrame");
        IdentityControls.Value.ShouldNotContain("NavMenuGroupItem");
        IdentityControls.Value.ShouldContain("MenuItem");
        IdentityControls.Value.ShouldContain("MenuFlyoutPresenter");
        IdentityControls.Value.ShouldNotContain("MenuPopupScrollHost");
        // Tab controls: the hosts and items are registered; the shared bases, the scroll viewer and the overflow menu are not.
        IdentityControls.Value.ShouldContain("CardTabStrip");
        IdentityControls.Value.ShouldContain("TabStripItem");
        IdentityControls.Value.ShouldNotContain("BaseTabControl");
        IdentityControls.Value.ShouldNotContain("BaseTabStrip");
        IdentityControls.Value.ShouldNotContain("TabScrollViewer");
        IdentityControls.Value.ShouldNotContain("TabOverflowMenu");
        IdentityControls.Value.ShouldNotContain("TabOverflowMenuItem");
        // Breadcrumb, pagination and tour: the controls, items, steps and indicators are registered; the page items, quick
        // jumper, steps view and mask layer are not.
        IdentityControls.Value.ShouldContain("BreadcrumbItem");
        IdentityControls.Value.ShouldContain("SimplePagination");
        IdentityControls.Value.ShouldContain("TourStep");
        IdentityControls.Value.ShouldNotContain("PaginationNavItem");
        IdentityControls.Value.ShouldNotContain("QuickJumperBar");
        IdentityControls.Value.ShouldNotContain("TourStepsView");
        IdentityControls.Value.ShouldNotContain("TourLayer");
        // Layout / container family: the card parts, collapse items and compact space parts are registered (the card grid
        // items, tabs content and collapse items are Token-owner transparent); the card action panel and the splitter
        // handle, drag bar and panel are not.
        IdentityControls.Value.ShouldContain("CardGridItem");
        IdentityControls.Value.ShouldContain("CardTabsContent");
        IdentityControls.Value.ShouldContain("CollapseItem");
        IdentityControls.Value.ShouldContain("CompactSpaceAddOn");
        IdentityControls.Value.ShouldNotContain("CardActionPanel");
        IdentityControls.Value.ShouldNotContain("SplitterHandle");
        IdentityControls.Value.ShouldNotContain("SplitterDragBar");
        IdentityControls.Value.ShouldNotContain("SplitterPanel");
        // Data family: the list and tree items and the Skeleton placeholders are registered (the items and the Skeleton are
        // Token-owner transparent); the tree node header and switcher and the list group header are not.
        IdentityControls.Value.ShouldContain("ListBoxItem");
        IdentityControls.Value.ShouldContain("ListViewItem");
        IdentityControls.Value.ShouldContain("TreeViewItem");
        IdentityControls.Value.ShouldContain("SkeletonParagraph");
        IdentityControls.Value.ShouldNotContain("TreeViewItemHeader");
        IdentityControls.Value.ShouldNotContain("NodeSwitcherButton");
        IdentityControls.Value.ShouldNotContain("GroupHeaderItem");
        IdentityControls.Value.ShouldNotContain("SkeletonTitle");
    }

    [Fact]
    public void Family_Own_Tokens_Are_Active_Or_Reserved()
    {
        var map = File.ReadAllText(Path.Combine(RepositoryRoot, DesktopControls,
            "GeneratedFiles/AtomUI.Generator/AtomUI.Generator.TokenResourceKeyGenerator/GeneratedThemeConsumption.g.cs"));
        var familyEntries = Regex.Matches(map,
                                     @"new OwnTokenEntry\(""AtomUI"", ""(?<control>\w+)"", ""(?<token>\w+)"", OwnTokenState\.(?<state>\w+)")
                                 .Where(static match => s_familyControls.Contains(match.Groups["control"].Value))
                                 .ToArray();
        // Guards the parse itself: a format change of the generated map must not make the gate pass vacuously.
        familyEntries.Count(static match => match.Groups["state"].Value == "Active").ShouldBeGreaterThan(0,
            "No Active Own Token entries of the family were found in the generated consumption map.");

        var unreferenced = familyEntries
                           .Where(static match => match.Groups["state"].Value == "Unreferenced")
                           .Select(static match => $"{match.Groups["control"].Value}.{match.Groups["token"].Value}")
                           .ToArray();
        unreferenced.ShouldBeEmpty("ATOMUIGEN042: consume, [ReservedToken], or delete these Own Tokens.");
    }

    /// <summary>
    /// Token identities visible to AtomUI.Desktop.Controls: its own and those of AtomUI.Controls, read from the
    /// generated <c>ThemeBuildOwnerAttribute</c> metadata as the generator reads it from referenced packages.
    /// </summary>
    private static readonly Lazy<HashSet<string>> IdentityControls = new(static () =>
    {
        var result = new HashSet<string>(StringComparer.Ordinal);
        foreach (var package in new[] { DesktopControls, "src/AtomUI.Controls" })
        {
            var contracts = File.ReadAllText(Path.Combine(RepositoryRoot, package,
                "GeneratedFiles/AtomUI.Generator/AtomUI.Generator.TokenResourceKeyGenerator/GeneratedThemeBuildContracts.g.cs"));
            foreach (Match match in Regex.Matches(contracts, @"ThemeBuildOwnerAttribute\(1, ""AtomUI"", ""(?<id>\w+)"""))
            {
                result.Add(match.Groups["id"].Value);
            }
        }

        return result;
    });

    private static readonly Lazy<string> s_repositoryRoot = new(FindRepositoryRoot);

    private static string RepositoryRoot => s_repositoryRoot.Value;

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AtomUI.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Unable to locate the repository root.");
    }

    private sealed class FileText(string path) : AdditionalText
    {
        public override string Path { get; } = path;

        public override SourceText GetText(CancellationToken cancellationToken = default) =>
            SourceText.From(File.ReadAllText(Path));
    }
}
