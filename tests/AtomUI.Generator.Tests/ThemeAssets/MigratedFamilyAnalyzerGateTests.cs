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
        "Alert", "Popup", "Tooltip"
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
        "Alert", "Popup", "ToolTip"
    ];

    public static IEnumerable<object[]> FamilyAssets() =>
        s_familyDirectories
            .SelectMany(directory => Directory.EnumerateFiles(
                Path.Combine(RepositoryRoot, DesktopControls, directory), "*.axaml", SearchOption.AllDirectories))
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
