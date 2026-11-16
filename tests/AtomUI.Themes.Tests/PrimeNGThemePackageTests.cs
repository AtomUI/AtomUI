using System.Xml.Linq;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.ContractTests;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Themes.Tests;

/// <summary>
/// One PrimeNG preset family as the package tests see it: its four profiles, its two algorithms and the facts the
/// README states about it.
/// </summary>
/// <param name="Preset">The preset name (<c>Aura</c>, <c>Lara</c>, <c>Nora</c>); the profile ids are <c>PrimeNG&lt;Preset&gt;</c> plus the suffixes.</param>
/// <param name="CompactNonEquivalentSlots">
/// The README's "不等价项": slots where the base profile + <c>AtomUI:Compact</c> differs from the compact profile, as
/// snapshot keys. They are also the only control values the compact XML may hold. Keep in sync with the README.
/// </param>
/// <param name="ExtraRuleTokens">Tokens beyond the shared list that the preset's rules produce, so its XML must not set them.</param>
internal sealed record PrimeNGPresetFamily(string Preset, string[] CompactNonEquivalentSlots, string[] ExtraRuleTokens)
{
    public string BaseProfile => "PrimeNG" + Preset;
    public string DarkProfile => BaseProfile + ".Dark";
    public string CompactProfile => BaseProfile + ".Compact";
    public string DarkCompactProfile => BaseProfile + ".DarkCompact";
    public string[] Profiles => [BaseProfile, DarkProfile, CompactProfile, DarkCompactProfile];
    public ThemeAlgorithm Light => new("PrimeNG", Preset);
    public ThemeAlgorithm Dark => new("PrimeNG", Preset + ".Dark");
}

/// <summary>
/// The PrimeNG preset package (spec 4.7, 4.8): the twelve profiles, what they compile to, how they relate to their
/// <c>PrimeNG:&lt;Preset&gt;</c> / <c>PrimeNG:&lt;Preset&gt;.Dark</c> algorithms, and the readability guarantees.
/// </summary>
public class PrimeNGThemePackageTests
{
    private static readonly XNamespace ThemeNamespace = "https://atomui.net/schemas/theme/v1";

    private static readonly Dictionary<string, PrimeNGPresetFamily> Presets = new[]
    {
        new PrimeNGPresetFamily("Aura", [], []),
        new PrimeNGPresetFamily("Lara", ["C AtomUI:Select PopupContentPadding"], ["ColorControlOutline"]),
        new PrimeNGPresetFamily("Nora", [], [])
    }.ToDictionary(preset => preset.Preset, StringComparer.Ordinal);

    /// <summary>Tokens every preset's rules produce, so no preset XML may set them.</summary>
    private static readonly string[] SharedRuleTokens =
    [
        "ColorPrimaryHover", "ColorPrimaryActive", "ColorPrimaryBg", "ColorPrimaryText", "ColorTextLightSolid",
        "ColorText", "ColorTextSecondary", "ColorTextDescription", "ColorTextPlaceholder", "ColorBgContainer",
        "ColorBgElevated", "ColorBgBase", "ColorTextBase", "ColorFillTertiary", "ColorBgContainerDisabled",
        "ColorBorderSecondary", "ColorBorder", "ColorBgMask", "ColorFocusBorder", "FocusVisualBorderThickness",
        "ControlOutlineWidth", "BorderRadiusXS", "BorderRadiusSM", "BorderRadiusLG", "ControlHeightSM",
        "ControlHeightLG", "PaddingContentHorizontal", "BoxShadows", "BoxShadowsSecondary"
    ];

    [AvaloniaFact]
    public void UsePrimeNGThemes_Registers_Three_Preset_Families_Without_Changing_Default()
    {
        var manager = Application.Current!.GetThemeManager()!;
        var expected = new[] { "PrimeNGAura", "PrimeNGLara", "PrimeNGNora" }
            .SelectMany(id => new[] { id, id + ".Dark", id + ".Compact", id + ".DarkCompact" })
            .Append("DaybreakBlue");
        manager.AvailableThemes.Where(theme => theme.Id == "DaybreakBlue" || theme.Id.StartsWith("PrimeNG", StringComparison.Ordinal))
            .Select(theme => theme.Id).ShouldBe(expected, ignoreOrder: true);
        manager.AvailableThemes.Single(theme => theme.IsDefault).Id.ShouldBe("DaybreakBlue");
        manager.CurrentTheme!.ThemeId.ShouldBe("DaybreakBlue");
        manager.ThemeCatalogDiagnostics.ShouldBeEmpty();
    }

    [AvaloniaTheory]
    [InlineData("PrimeNGAura")]
    [InlineData("PrimeNGLara")]
    [InlineData("PrimeNGNora")]
    public async Task Each_Preset_Applies_Light_Dark_Compact(string id)
    {
        foreach (var suffix in new[] { "", ".Dark", ".Compact", ".DarkCompact" })
        {
            var manager = (ThemeManager)Application.Current!.GetThemeManager()!;
            var result = await manager.ApplyThemeAsync(new ThemeRequest(id + suffix, null, ThemeTransitionReason.UserRequest), TestContext.Current.CancellationToken);
            result.Status.ShouldBe(ThemeTransitionStatus.Committed);
            manager.CurrentTheme!.Appearance.ShouldBe(suffix.Contains("Dark")
                ? ThemeAppearance.Dark : ThemeAppearance.Light);
            var snapshot = manager.CurrentSnapshot!;
            // @primeuix/themes 3.1.1 form.field heights at a 14px font and 1.5 line height (primeng.dev v22).
            var normalHeight = id == "PrimeNGLara" ? 39d : 35d;
            Global<double>(snapshot, "ControlHeight").ShouldBe(normalHeight - (suffix.Contains("Compact") ? 4 : 0));
            if (suffix.Contains("Compact"))
            {
                var height = normalHeight - 4;
                Global<double>(snapshot, "ControlHeightSM").ShouldBe(height * 0.75);
                Global<double>(snapshot, "ControlHeightLG").ShouldBe(height * 1.25);
                Global<double>(snapshot, "PaddingContentHorizontal").ShouldBe(7);
                // The field padding and the Select option padding follow the size ladder (PrimeUIX has no compact).
                Global<Thickness>(snapshot, "InputPadding").Left.ShouldBe(6);
                Global<Thickness>(snapshot, "InputPaddingSM").Left.ShouldBe(4);
                Global<Thickness>(snapshot, "InputPaddingLG").Left.ShouldBe(8);
                Own<Thickness>(snapshot, "Select", "OptionPadding").ShouldBe(id == "PrimeNGLara" ? new Thickness(10, 4) : new Thickness(6, 4));
            }
            if (!suffix.Contains("Compact"))
            {
                Global<double>(snapshot, "ControlHeightSM").ShouldBe(normalHeight - 7);
                Global<double>(snapshot, "ControlHeightLG").ShouldBe(normalHeight + 7);
                // form.field.padding.x / sm / lg (10 / 8 / 12); the Button pads its label by the same 10.
                Global<Thickness>(snapshot, "InputPadding").Left.ShouldBe(10);
                Global<Thickness>(snapshot, "InputPaddingSM").Left.ShouldBe(8);
                Global<Thickness>(snapshot, "InputPaddingLG").Left.ShouldBe(12);
                Global<double>(snapshot, "PaddingContentHorizontal").ShouldBe(11);
                // A field is its font height plus vertical padding and border; PrimeNG fields are exactly the control
                // heights, and an odd remainder is split top/bottom in whole pixels.
                foreach (var (padding, height, fontHeight) in new[]
                         {
                             ("InputPadding", "ControlHeight", "FontHeight"),
                             ("InputPaddingSM", "ControlHeightSM", "FontHeightSM"),
                             ("InputPaddingLG", "ControlHeightLG", "FontHeightLG")
                         })
                {
                    var field = Global<Thickness>(snapshot, padding);
                    // The small field also insets its text by InputContentMargin.
                    var margin = padding == "InputPaddingSM" ? Global<Thickness>(snapshot, "InputContentMargin") : default;
                    (field.Top + field.Bottom + Global<double>(snapshot, fontHeight) + 2 * Global<double>(snapshot, "LineWidth") +
                     margin.Top + margin.Bottom)
                        .ShouldBe(Global<double>(snapshot, height), $"{id}{suffix} {padding}");
                    field.Top.ShouldBe(Math.Floor(field.Top));
                    field.Bottom.ShouldBe(Math.Floor(field.Bottom));
                }
            }
            if (id == "PrimeNGLara" && !suffix.Contains("Compact"))
            {
                Own<Thickness>(snapshot, "Select", "OptionPadding").ShouldBe(new Thickness(14, 8));
                Own<Thickness>(snapshot, "Select", "PopupContentPadding").ShouldBe(new Thickness(0, 8));
            }
            if (suffix.Contains("Dark"))
            {
                Global<Color>(snapshot, "ColorBgContainer").ShouldBe(Color.Parse("#09090B"));
                Global<Color>(snapshot, "ColorText").ShouldBe(Colors.White);
                Effective<Color>(snapshot, "Card", "ColorBgContainer").ShouldBe(Color.Parse("#18181B"));
                Own<Color>(snapshot, "ToolTip", "ToolTipColor").ShouldBe(Colors.White);
            }
        }
    }

    [AvaloniaTheory]
    [InlineData("Aura")]
    [InlineData("Lara")]
    [InlineData("Nora")]
    public async Task Dark_Profile_Equals_Light_Profile_With_The_Dark_Algorithm(string presetName)
    {
        var preset = Presets[presetName];
        var swapped = await Dump(preset.BaseProfile, preset.Dark);
        var dark = await Dump(preset.DarkProfile);
        ThemeSnapshotDump.Diff(dark, swapped).ShouldBeEmpty();

        var swappedCompact = await Dump(preset.CompactProfile, preset.Dark, ThemeAlgorithm.Compact);
        var darkCompact = await Dump(preset.DarkCompactProfile);
        ThemeSnapshotDump.Diff(darkCompact, swappedCompact).ShouldBeEmpty();
    }

    [AvaloniaTheory]
    [InlineData("Aura")]
    [InlineData("Lara")]
    [InlineData("Nora")]
    public async Task Global_Dark_Toggle_Yields_A_Readable_Hybrid_Chain(string presetName)
    {
        // The Gallery's global dark switch appends AtomUI:Dark to the profile's chain. The result is a hybrid chain,
        // not the PrimeNG dark profile (spec 4.4); the contract only promises that it stays readable.
        var preset = Presets[presetName];
        var config = new ThemeConfigBuilder().WithAlgorithms(preset.Light, ThemeAlgorithm.Dark).Build();
        var hybrid = await Apply(new ThemeRequest(preset.BaseProfile, config, ThemeTransitionReason.UserRequest));

        var ratio = ContrastRatio.Of(Global<Color>(hybrid, "ColorText"), Global<Color>(hybrid, "ColorBgContainer"));
        ratio.ShouldBeGreaterThanOrEqualTo(4.5, $"ColorText on ColorBgContainer: {ratio:F2}");
    }

    [AvaloniaTheory]
    [InlineData("Aura")]
    [InlineData("Lara")]
    [InlineData("Nora")]
    public async Task Appending_Compact_Equals_The_Compact_Profile_Except_Listed_Slots(string presetName)
    {
        var preset = Presets[presetName];
        foreach (var (baseId, chain, compactId) in new[]
                 {
                     (preset.BaseProfile, new[] { preset.Light, ThemeAlgorithm.Compact }, preset.CompactProfile),
                     (preset.DarkProfile, new[] { preset.Dark, ThemeAlgorithm.Compact }, preset.DarkCompactProfile)
                 })
        {
            var appended = await Dump(baseId, chain);
            var compact = await Dump(compactId);
            var slots = ThemeSnapshotDump.Diff(compact, appended)
                                         .Select(ThemeSnapshotDump.SlotOf)
                                         .ToArray();
            slots.ShouldBe(preset.CompactNonEquivalentSlots, ignoreOrder: true, baseId);
        }
    }

    [AvaloniaTheory]
    // Selected background: Aura and Lara light P50 (PrimeUIX highlight.background), Nora light the solid P600.
    [InlineData("Aura", "#3B82F6", "#EFF6FF")]
    [InlineData("Lara", "#3B82F6", "#EFF6FF")]
    [InlineData("Nora", "#2563EB", "#2563EB")]
    public async Task Primary_Seed_Change_Moves_Focus_Ring_And_Selected_Background(string presetName, string expectedPrimary,
                                                                                  string expectedSelected)
    {
        // The seed is the 500 stop; Aura and Lara take it as the light primary, Nora the 600 stop (blue-600).
        var profile = Presets[presetName].BaseProfile;
        var emerald = await ApplyEmerald(profile);
        var emeraldSelected = Global<Color>(emerald, "ControlItemBgActive");

        var config = new ThemeConfigBuilder().WithToken("ColorPrimary", "#3B82F6").Build();
        var blue = await Apply(new ThemeRequest(profile, config, ThemeTransitionReason.UserRequest));
        Global<Color>(blue, "ColorPrimary").ShouldBe(Color.Parse(expectedPrimary));
        Global<Color>(blue, "ColorFocusBorder").ShouldBe(Global<Color>(blue, "ColorPrimary"));
        Global<Color>(blue, "ControlItemBgActive").ShouldNotBe(emeraldSelected);
        Global<Color>(blue, "ControlItemBgActive").ShouldBe(Color.Parse(expectedSelected));
    }

    [Theory]
    [InlineData("Aura")]
    [InlineData("Lara")]
    [InlineData("Nora")]
    public void Preset_Xml_Holds_No_Rule_Tokens(string presetName)
    {
        // Guards against hand-writing derived values back into the XML: they would stop following the seeds, and
        // the light and dark files would drift apart.
        var preset = Presets[presetName];
        var ruleTokens = SharedRuleTokens.Concat(preset.ExtraRuleTokens).ToArray();
        var themes = Path.Combine(FindRepositoryRoot(), "src", "AtomUI.Themes.PrimeNG", "Themes");
        foreach (var profile in preset.Profiles)
        {
            var root = XDocument.Load(Path.Combine(themes, profile + ".theme.xml")).Root!;
            var names = root.Element(ThemeNamespace + "Tokens")!
                            .Elements(ThemeNamespace + "Token")
                            .Select(token => token.Attribute("Name")!.Value)
                            .ToArray();
            names.Intersect(ruleTokens).ShouldBeEmpty(profile);

            // Every per-control value is a control stage, except the compact slots listed as not equivalent;
            // a base profile has no Controls element at all.
            var controlsElement = root.Element(ThemeNamespace + "Controls");
            var isCompact = profile.EndsWith("Compact", StringComparison.Ordinal);
            if (!isCompact || preset.CompactNonEquivalentSlots.Length == 0)
            {
                controlsElement.ShouldBeNull(profile);
                continue;
            }
            var controls = controlsElement!.Elements(ThemeNamespace + "Control")
                                           .SelectMany(control => control.Element(ThemeNamespace + "Tokens")!
                                                                         .Elements(ThemeNamespace + "Token")
                                                                         .Select(token =>
                                                                             $"C {control.Attribute("Catalog")!.Value}:{control.Attribute("Id")!.Value} {token.Attribute("Name")!.Value}"))
                                           .ToArray();
            controls.ShouldBe(preset.CompactNonEquivalentSlots, ignoreOrder: true, profile);
        }

        // Light and dark differ only in the chain (and the profile metadata on the root element).
        StripMetadata(Path.Combine(themes, preset.BaseProfile + ".theme.xml"), preset.Light.Id)
            .ShouldBe(StripMetadata(Path.Combine(themes, preset.DarkProfile + ".theme.xml"), preset.Dark.Id));
        StripMetadata(Path.Combine(themes, preset.CompactProfile + ".theme.xml"), preset.Light.Id)
            .ShouldBe(StripMetadata(Path.Combine(themes, preset.DarkCompactProfile + ".theme.xml"), preset.Dark.Id));
    }

    [AvaloniaFact]
    public async Task Aura_Maps_Semantic_Tokens()
    {
        var snapshot = await ApplyEmerald("PrimeNGAura");
        foreach (var (name, value) in new Dictionary<string, string>
                 {
                     ["ColorPrimary"] = "#10B981", ["ColorPrimaryHover"] = "#059669",
                     ["ColorPrimaryActive"] = "#047857", ["ColorText"] = "#334155",
                     ["ColorTextSecondary"] = "#64748B", ["ColorTextDescription"] = "#64748B",
                     ["ColorBorder"] = "#CBD5E1", ["ColorBorderSecondary"] = "#E2E8F0",
                     ["ColorBgContainer"] = "#FFFFFF", ["ColorFillTertiary"] = "#F1F5F9",
                     ["ColorBgContainerDisabled"] = "#E2E8F0", ["ColorBgMask"] = "#66000000",
                     ["ColorPrimaryBg"] = "#ECFDF5", ["ColorPrimaryText"] = "#047857",
                     ["ColorTextLightSolid"] = "#FFFFFF", ["ColorError"] = "#F87171"
                 })
        {
            Global<Color>(snapshot, name).ShouldBe(Color.Parse(value));
        }
        foreach (var (name, value) in new Dictionary<string, double>
                 {
                     ["ControlHeight"] = 35, ["ControlHeightSM"] = 28, ["ControlHeightLG"] = 42,
                     ["PaddingContentHorizontal"] = 11, ["FontSizeSM"] = 12, ["FontSizeLG"] = 16,
                     ["ControlOutlineWidth"] = 0
                 })
        {
            Global<double>(snapshot, name).ShouldBe(value);
        }
        Global<CornerRadius>(snapshot, "BorderRadius").ShouldBe(new CornerRadius(6));
        Global<CornerRadius>(snapshot, "BorderRadiusXS").ShouldBe(new CornerRadius(2));
        Global<CornerRadius>(snapshot, "BorderRadiusSM").ShouldBe(new CornerRadius(4));
        Global<CornerRadius>(snapshot, "BorderRadiusLG").ShouldBe(new CornerRadius(8));
        Global<Thickness>(snapshot, "FocusVisualBorderThickness").ShouldBe(new Thickness(1));
        Global<BoxShadows>(snapshot, "InputActiveShadow")[0].Spread.ShouldBe(0);
        Global<TimeSpan>(snapshot, "MotionDurationMid").ShouldBe(TimeSpan.FromMilliseconds(200));
        Global<BoxShadows>(snapshot, "BoxShadowsSecondary").Count.ShouldBe(2);
    }

    [AvaloniaFact]
    public async Task Aura_Control_Overrides_Reach_Their_Owners()
    {
        var snapshot = await Apply("PrimeNGAura");
        Own<FontWeight>(snapshot, "Button", "FontWeight").ShouldBe(FontWeight.Medium);
        Own<double>(snapshot, "CheckBox", "CheckIndicatorSize").ShouldBe(18);
        Own<double>(snapshot, "CheckBox", "CheckedMarkSize").ShouldBe(12);
        Own<Thickness>(snapshot, "Select", "OptionPadding").ShouldBe(new Thickness(10, 4));
        Own<Thickness>(snapshot, "Select", "PopupContentPadding").ShouldBe(new Thickness(4));
        // The label sits at border + button.padding.x (10 / sm 8 / lg 12), as on primeng.dev.
        Own<Thickness>(snapshot, "Button", "ContentPadding").Left.ShouldBe(11);
        Own<Thickness>(snapshot, "Button", "ContentPaddingSM").Left.ShouldBe(9);
        Own<Thickness>(snapshot, "Button", "ContentPaddingLG").Left.ShouldBe(13);
        Own<Thickness>(snapshot, "Card", "BodyPadding").ShouldBe(new Thickness(18));
        Own<BoxShadows>(snapshot, "Card", "RestingShadows").Count.ShouldBe(2);
        Own<Color>(snapshot, "ToolTip", "ToolTipBackground").ShouldBe(Color.Parse("#334155"));
        Own<CornerRadius>(snapshot, "ToolTip", "ToolTipCornerRadius").ShouldBe(new CornerRadius(6));
        Own<Thickness>(snapshot, "ToolTip", "ContentPadding").ShouldBe(new Thickness(10, 6));
        Effective<CornerRadius>(snapshot, "Card", "BorderRadiusLG").ShouldBe(new CornerRadius(12));
        Effective<CornerRadius>(snapshot, "Dialog", "BorderRadiusLG").ShouldBe(new CornerRadius(12));
        Own<Thickness>(snapshot, "Dialog", "ContentPadding").ShouldBe(new Thickness(18));
        Own<double>(snapshot, "Dialog", "HeaderFontSize").ShouldBe(18);
    }

    [AvaloniaTheory]
    [InlineData("PrimeNGAura", "#3F000000", "#E2E8F0")]
    [InlineData("PrimeNGAura.Dark", "#3FFFFFFF", "#3F3F46")]
    [InlineData("PrimeNGAura.Compact", "#3F000000", "#E2E8F0")]
    [InlineData("PrimeNGAura.DarkCompact", "#3FFFFFFF", "#3F3F46")]
    [InlineData("PrimeNGLara", "#3F000000", "#E2E8F0")]
    [InlineData("PrimeNGLara.Dark", "#3FFFFFFF", "#3F3F46")]
    [InlineData("PrimeNGLara.Compact", "#3F000000", "#E2E8F0")]
    [InlineData("PrimeNGLara.DarkCompact", "#3FFFFFFF", "#3F3F46")]
    [InlineData("PrimeNGNora", "#3F000000", "#E2E8F0")]
    [InlineData("PrimeNGNora.Dark", "#3FFFFFFF", "#3F3F46")]
    [InlineData("PrimeNGNora.Compact", "#3F000000", "#E2E8F0")]
    [InlineData("PrimeNGNora.DarkCompact", "#3FFFFFFF", "#3F3F46")]
    public async Task Disabled_Text_Stays_As_Readable_As_The_Old_Profiles(
        string id, string oldDisabledText, string oldDisabledBackground)
    {
        // The two colours per row are ColorTextDisabled and ColorBgContainerDisabled as the profiles compiled them
        // before they moved onto the PrimeNG algorithms (taken from the compile snapshots of commit 9a958923f): Ant's
        // translucent disabled text on the background the old XML set explicitly. Their contrast is the floor: 1.79 light, 2.06 dark, the same for Aura, Lara and Nora.
        // Ant's formula on the PrimeNG neutrals would give 1.20 / 1.35.
        var floor = ContrastRatio.Of(Color.Parse(oldDisabledText), Color.Parse(oldDisabledBackground));
        var snapshot = await Apply(id);
        var ratio = ContrastRatio.Of(Global<Color>(snapshot, "ColorTextDisabled"),
            Global<Color>(snapshot, "ColorBgContainerDisabled"));
        ratio.ShouldBeGreaterThanOrEqualTo(floor, $"{id}: {ratio:F2} < {floor:F2}");
    }

    [AvaloniaTheory]
    [InlineData("PrimeNGAura")]
    [InlineData("PrimeNGAura.Compact")]
    [InlineData("PrimeNGLara")]
    [InlineData("PrimeNGLara.Compact")]
    [InlineData("PrimeNGNora")]
    [InlineData("PrimeNGNora.Compact")]
    public async Task Light_Tertiary_And_Quaternary_Text_Stay_As_Readable_As_The_Old_Profiles(string id)
    {
        // The floors are what the pre-algorithm light profiles had: Ant's 45% and 25% black text on white,
        // 3.32 and 1.82. ColorIcon follows ColorTextTertiary, so the tertiary floor also keeps icons readable.
        var snapshot = await Apply(id);
        var container = Global<Color>(snapshot, "ColorBgContainer");
        var tertiary = ContrastRatio.Of(Global<Color>(snapshot, "ColorTextTertiary"), container);
        var quaternary = ContrastRatio.Of(Global<Color>(snapshot, "ColorTextQuaternary"), container);
        tertiary.ShouldBeGreaterThanOrEqualTo(3.3, $"{id} ColorTextTertiary: {tertiary:F2}");
        quaternary.ShouldBeGreaterThanOrEqualTo(1.8, $"{id} ColorTextQuaternary: {quaternary:F2}");
    }

    [AvaloniaTheory]
    // Light: the floor is the pre-algorithm link colour, Ant blue #1677FF on white (4.10); hover and active must not be
    // less readable than the base, so all three share it. The links take the primary 700 / 800 / 900 stops for that.
    [InlineData("PrimeNGAura", 4.10, 4.10, 4.10)]
    [InlineData("PrimeNGAura.Compact", 4.10, 4.10, 4.10)]
    [InlineData("PrimeNGLara", 4.10, 4.10, 4.10)]
    [InlineData("PrimeNGLara.Compact", 4.10, 4.10, 4.10)]
    [InlineData("PrimeNGNora", 4.10, 4.10, 4.10)]
    [InlineData("PrimeNGNora.Compact", 4.10, 4.10, 4.10)]
    // Dark: the floors are the values of the algorithm profiles (Aura and Lara 400 / 300 / 200, Nora 500 / 400 / 300),
    // which are already far above the old profiles' 3.83 / 1.98 / 2.75.
    [InlineData("PrimeNGAura.Dark", 10.35, 13.05, 15.51)]
    [InlineData("PrimeNGAura.DarkCompact", 10.35, 13.05, 15.51)]
    [InlineData("PrimeNGLara.Dark", 10.35, 13.05, 15.51)]
    [InlineData("PrimeNGLara.DarkCompact", 10.35, 13.05, 15.51)]
    [InlineData("PrimeNGNora.Dark", 7.84, 10.35, 13.05)]
    [InlineData("PrimeNGNora.DarkCompact", 7.84, 10.35, 13.05)]
    public async Task Link_Colours_Stay_Readable_On_The_Container(string id, double link, double hover, double active)
    {
        // The floors are the emerald values; the noir default is covered by Noir_Stays_Readable.
        var snapshot = await ApplyEmerald(id);
        var container = Global<Color>(snapshot, "ColorBgContainer");
        foreach (var (name, floor) in new[] { ("ColorLink", link), ("ColorLinkHover", hover), ("ColorLinkActive", active) })
        {
            // Two decimals, as the floors are written.
            var ratio = Math.Round(ContrastRatio.Of(Global<Color>(snapshot, name), container), 2);
            ratio.ShouldBeGreaterThanOrEqualTo(floor, $"{id} {name}: {ratio:F2} < {floor:F2}");
        }
    }

    [AvaloniaFact]
    public async Task Explicit_ColorBgSpotlight_Is_The_ToolTip_Background()
    {
        // Priority "algorithm < theme explicit value": the ToolTip stage reads the global spotlight background
        // instead of recomputing the preset's stop, so a theme that sets it explicitly is honoured.
        var config = new ThemeConfigBuilder().WithToken("ColorBgSpotlight", "#FF123456").Build();
        var snapshot = await Apply(new ThemeRequest("PrimeNGAura", config, ThemeTransitionReason.UserRequest));
        Own<Color>(snapshot, "ToolTip", "ToolTipBackground").ShouldBe(Color.Parse("#FF123456"));

        var preset = await Apply("PrimeNGAura");
        Own<Color>(preset, "ToolTip", "ToolTipBackground").ShouldBe(Global<Color>(preset, "ColorBgSpotlight"));
    }

    [AvaloniaFact]
    public async Task Lara_Uses_Outline_Glow_And_Larger_Controls()
    {
        var snapshot = await ApplyEmerald("PrimeNGLara");
        Global<double>(snapshot, "ControlOutlineWidth").ShouldBe(3);
        Global<Color>(snapshot, "ColorControlOutline").ShouldBe(Color.Parse("#A7F3D0"));
        var inputActiveShadow = Global<BoxShadows>(snapshot, "InputActiveShadow")[0];
        inputActiveShadow.Spread.ShouldBe(3);
        inputActiveShadow.Color.ShouldBe(Color.Parse("#A7F3D0"));
        Global<Thickness>(snapshot, "FocusVisualBorderThickness").ShouldBe(new Thickness(0));
        Global<double>(snapshot, "ControlHeight").ShouldBe(39);
        Own<double>(snapshot, "CheckBox", "CheckIndicatorSize").ShouldBe(20);
        Own<double>(snapshot, "CheckBox", "CheckedMarkSize").ShouldBe(14);
        Own<Thickness>(snapshot, "Select", "OptionPadding").ShouldBe(new Thickness(14, 8));
        Own<Thickness>(snapshot, "Select", "PopupContentPadding").ShouldBe(new Thickness(0, 8));
        Own<FontWeight>(snapshot, "Button", "FontWeight").ShouldBe(FontWeight.SemiBold);
        // Lara's button.padding.x is 0.875rem, wider than its 0.625rem field padding.
        Own<Thickness>(snapshot, "Button", "ContentPadding").Left.ShouldBe(15);
        Own<Thickness>(snapshot, "Button", "ContentPaddingSM").Left.ShouldBe(9);
        Own<Thickness>(snapshot, "Button", "ContentPaddingLG").Left.ShouldBe(13);
        Own<Thickness>(snapshot, "Card", "BodyPadding").ShouldBe(new Thickness(20));
        Own<Thickness>(snapshot, "Dialog", "ContentPadding").ShouldBe(new Thickness(20));
        Own<Thickness>(snapshot, "ToolTip", "ContentPadding").ShouldBe(new Thickness(10, 8));
    }

    [AvaloniaFact]
    public async Task Nora_Uses_The_Standard_Nora_Densities()
    {
        var snapshot = await Apply("PrimeNGNora");
        Global<double>(snapshot, "ControlHeight").ShouldBe(35);
        Own<double>(snapshot, "CheckBox", "CheckIndicatorSize").ShouldBe(18);
        Own<double>(snapshot, "CheckBox", "CheckedMarkSize").ShouldBe(12);
        Own<Thickness>(snapshot, "Select", "OptionPadding").ShouldBe(new Thickness(10, 4));
        Own<Thickness>(snapshot, "Select", "PopupContentPadding").ShouldBe(new Thickness(0, 2));
        Own<Thickness>(snapshot, "Button", "ContentPadding").Left.ShouldBe(11);
        Own<Thickness>(snapshot, "Card", "BodyPadding").ShouldBe(new Thickness(18));
        Own<Thickness>(snapshot, "Dialog", "ContentPadding").ShouldBe(new Thickness(18));
        Own<Thickness>(snapshot, "ToolTip", "ContentPadding").ShouldBe(new Thickness(10, 6));
    }

    [AvaloniaFact]
    public async Task Lara_Focus_Glow_Follows_The_Primary_Seed()
    {
        // Lara's focus glow is the primary 200 stop (emerald-200 #A7F3D0 by default): it follows the seed (blue-200).
        var config = new ThemeConfigBuilder().WithToken("ColorPrimary", "#3B82F6").Build();
        var blue = await Apply(new ThemeRequest("PrimeNGLara", config, ThemeTransitionReason.UserRequest));
        Global<Color>(blue, "ColorControlOutline").ShouldBe(Color.Parse("#BFDBFE"));
    }

    [AvaloniaFact]
    public async Task Nora_Disables_Motion_And_Uses_Radius_2()
    {
        var snapshot = await Apply("PrimeNGNora");
        Global<bool>(snapshot, "EnableMotion").ShouldBeFalse();
        Global<CornerRadius>(snapshot, "BorderRadius").ShouldBe(new CornerRadius(2));
        Global<Color>(snapshot, "ColorText").ShouldBe(Color.Parse("#0F172A"));
        Global<Color>(snapshot, "ColorBorderSecondary").ShouldBe(Color.Parse("#00000000"));
        Own<FontWeight>(snapshot, "Button", "FontWeight").ShouldBe(FontWeight.Bold);
        Effective<CornerRadius>(snapshot, "Card", "BorderRadiusLG").ShouldBe(new CornerRadius(2));
        foreach (var profile in new[] { "PrimeNGNora.Dark", "PrimeNGNora.DarkCompact" })
        {
            var dark = await ApplyEmerald(profile);
            // The selected background is the solid primary (ControlItemBgActive) with surface-950 text; ColorPrimaryBg
            // is the primary at 16% (PrimeUIX button.text hover background) and the primary text, drawn on the page
            // background, is the primary 400 stop.
            Global<Color>(dark, "ControlItemBgActive").ShouldBe(Color.Parse("#10B981"));
            Own<Color>(dark, "Select", "OptionSelectedColor").ShouldBe(Color.Parse("#09090B"));
            Global<Color>(dark, "ColorPrimaryBg").ShouldBe(Color.Parse("#2910B981"));
            Global<Color>(dark, "ColorPrimaryText").ShouldBe(Color.Parse("#34D399"));
            Own<Color>(dark, "Steps", "ProcessIconColor").ShouldBe(Color.Parse("#09090B"));
            Own<Color>(dark, "Steps", "FinishTitleColor").ShouldBe(Colors.White);
        }
    }

    [AvaloniaFact]
    public async Task Nora_Takes_Darker_Primary_Stops()
    {
        // Light 600 / 700 / 800 and dark 500 / 400 / 300 of the shared emerald-500 seed (Aura: 500 / 600 / 700 and
        // 400 / 300 / 200).
        var light = await ApplyEmerald("PrimeNGNora");
        Global<Color>(light, "ColorPrimary").ShouldBe(Color.Parse("#059669"));
        Global<Color>(light, "ColorPrimaryHover").ShouldBe(Color.Parse("#047857"));
        Global<Color>(light, "ColorPrimaryActive").ShouldBe(Color.Parse("#065F46"));
        Global<Color>(light, "ColorFocusBorder").ShouldBe(Color.Parse("#059669"));

        var dark = await ApplyEmerald("PrimeNGNora.Dark");
        Global<Color>(dark, "ColorPrimary").ShouldBe(Color.Parse("#10B981"));
        Global<Color>(dark, "ColorPrimaryHover").ShouldBe(Color.Parse("#34D399"));
        Global<Color>(dark, "ColorPrimaryActive").ShouldBe(Color.Parse("#6EE7B7"));
        // The dark selected background is the solid primary with the zinc-950 contrast text (asserted by
        // Nora_Disables_Motion_And_Uses_Radius_2), and the dark focus ring is the field border (zinc-500), not the
        // primary.
        Global<Color>(dark, "ColorFocusBorder").ShouldBe(Color.Parse("#71717A"));
        Global<Color>(dark, "ColorFocusBorder").ShouldBe(Global<Color>(dark, "ColorBorder"));
    }

    [AvaloniaFact]
    public async Task Nora_Dark_Steps_Override_Comes_From_The_Control_Stage()
    {
        // The completed-step check is drawn on the solid primary, so the Nora.Dark Steps stage gives it the
        // contrast colour (PrimeNGStepsTokens).
        foreach (var profile in new[] { "PrimeNGNora.Dark", "PrimeNGNora.DarkCompact" })
        {
            var dark = await ApplyEmerald(profile);
            Own<Color>(dark, "Steps", "FinishIconColor").ShouldBe(Color.Parse("#09090B"), profile);
        }

        // The stage follows the contrast colour, not a constant: a light appearance after Nora.Dark gets the Nora
        // light Alias highlight (solid primary) and the light contrast colour on it.
        var hybridConfig = new ThemeConfigBuilder()
                           .WithAlgorithms(new ThemeAlgorithm("PrimeNG", "Nora.Dark"), ThemeAlgorithm.Default)
                           .Build();
        var hybrid = await Apply(new ThemeRequest("PrimeNGNora.Dark", hybridConfig, ThemeTransitionReason.UserRequest));
        Global<Color>(hybrid, "ControlItemBgActive").ShouldBe(Global<Color>(hybrid, "ColorPrimary"));
        Own<Color>(hybrid, "Steps", "FinishIconColor").ShouldBe(Global<Color>(hybrid, "ColorTextLightSolid"));

        // Under Aura's pale highlight the stage writes nothing and the Ant Design value (the primary) stays.
        var aura = await ApplyEmerald("PrimeNGAura");
        Own<Color>(aura, "Steps", "FinishIconColor").ShouldBe(Global<Color>(aura, "ColorPrimary"));

        var themes = Path.Combine(FindRepositoryRoot(), "src", "AtomUI.Themes.PrimeNG", "Themes");
        foreach (var profile in Presets["Nora"].Profiles)
        {
            var root = XDocument.Load(Path.Combine(themes, profile + ".theme.xml")).Root!;
            root.Descendants(ThemeNamespace + "Control")
                .Where(control => control.Attribute("Id")?.Value == "Steps")
                .ShouldBeEmpty(profile);
        }
    }

    [AvaloniaTheory]
    [InlineData("PrimeNGNora.Dark")]
    [InlineData("PrimeNGNora.DarkCompact")]
    public async Task Nora_Dark_Selected_Text_Stays_Readable(string profile)
    {
        // Nora dark makes the selected background the solid primary (PrimeUIX highlight.background) with the
        // surface-950 contrast text on it and on the focus background (P400).
        const double floor = 4.5;
        var dark = await ApplyEmerald(profile);
        var container = Global<Color>(dark, "ColorBgContainer");

        foreach (var background in new[] { "ControlItemBgActive", "ControlItemBgActiveHover" })
        {
            var selected = ContrastRatio.Of(Own<Color>(dark, "ListBox", "ItemSelectedColor"), Global<Color>(dark, background));
            selected.ShouldBeGreaterThanOrEqualTo(floor, $"ListBox selected text on {background}: {selected:F2}");
        }

        var menuSelected = ContrastRatio.Of(Own<Color>(dark, "NavMenu", "ItemSelectedColor"),
            ContrastRatio.Composite(Own<Color>(dark, "NavMenu", "ItemSelectedBg"), Own<Color>(dark, "NavMenu", "ItemBg")));
        menuSelected.ShouldBeGreaterThanOrEqualTo(floor, $"NavMenu selected item: {menuSelected:F2}");

        // The Filled primary button: ColorPrimary on the button's own ColorPrimaryBg / ColorPrimaryBgHover.
        var primary = Global<Color>(dark, "ColorPrimary");
        foreach (var background in new[] { "ColorPrimaryBg", "ColorPrimaryBgHover" })
        {
            var filled = ContrastRatio.Of(primary,
                ContrastRatio.Composite(Effective<Color>(dark, "Button", background), container));
            filled.ShouldBeGreaterThanOrEqualTo(floor, $"Filled button on {background}: {filled:F2}");
        }
    }

    [AvaloniaTheory]
    [InlineData("PrimeNGNora")]
    [InlineData("PrimeNGNora.Compact")]
    public async Task Nora_Light_Highlight_Is_The_Solid_Primary(string profile)
    {
        // @primeuix/themes 3.1.1 nora: highlight.background light-dark(primary.600, primary.500), focusBackground
        // light-dark(primary.700, primary.400), color light-dark(#ffffff, surface.950). The selected background is the
        // Alias ControlItemBgActive; ColorPrimaryBg stays the pale primary (button.text hover background primary.50).
        var light = await ApplyEmerald(profile);
        Global<Color>(light, "ControlItemBgActive").ShouldBe(Color.Parse("#059669"));
        Global<Color>(light, "ControlItemBgActiveHover").ShouldBe(Color.Parse("#047857"));
        Global<Color>(light, "ColorPrimaryBg").ShouldBe(Color.Parse("#ECFDF5"));
        Global<Color>(light, "ColorPrimaryBgHover").ShouldBe(Color.Parse("#D1FAE5"));
        // Primary-coloured text stays on the page-readable stops (it is drawn on the page background).
        Global<Color>(light, "ColorPrimaryText").ShouldBe(Color.Parse("#065F46"));
        Global<Color>(light, "ColorTextLightSolid").ShouldBe(Colors.White);
        Own<Color>(light, "Select", "OptionSelectedColor").ShouldBe(Colors.White);
        // The completed step's check is the contrast colour on the solid primary.
        Own<Color>(light, "Steps", "FinishIconColor").ShouldBe(Colors.White);
        Own<Color>(light, "Steps", "FinishIconBgColor").ShouldBe(Color.Parse("#059669"));
        // The Filled button keeps the global pale background.
        Effective<Color>(light, "Button", "ColorPrimaryBg").ShouldBe(Color.Parse("#ECFDF5"));
        // NavMenu follows PrimeUIX navigation, not the highlight: the neutral active background (slate 200).
        Own<Color>(light, "NavMenu", "ItemSelectedBg").ShouldBe(Color.Parse("#E2E8F0"));
    }

    // @primeuix/themes 3.1.1 navigation.item: active.background / active.color (Aura, Lara: surface 100 / 800 with
    // text.hover.color surface 800 / 0; Nora: surface 200 / 700 with text.hover.color 950 light, text.color 0 dark).
    // Aura and Lara focus on the same pair; Nora hovers on content.hover (the same pair) instead of its solid primary
    // focus. The primary seed (noir or a colour) does not change navigation.
    [AvaloniaTheory]
    [InlineData("PrimeNGAura", null, "#F1F5F9", "#1E293B")]
    [InlineData("PrimeNGAura.Dark", null, "#27272A", "#FFFFFF")]
    [InlineData("PrimeNGLara", null, "#F1F5F9", "#1E293B")]
    [InlineData("PrimeNGLara.Dark", null, "#27272A", "#FFFFFF")]
    [InlineData("PrimeNGNora", null, "#E2E8F0", "#020617")]
    [InlineData("PrimeNGNora.Dark", null, "#3F3F46", "#FFFFFF")]
    [InlineData("PrimeNGAura", "#10B981", "#F1F5F9", "#1E293B")]
    [InlineData("PrimeNGNora", "#6366F1", "#E2E8F0", "#020617")]
    [InlineData("PrimeNGNora.Dark", "#6366F1", "#3F3F46", "#FFFFFF")]
    public async Task NavMenu_Follows_PrimeUIX_Navigation(string profile, string? seed, string background, string text)
    {
        var snapshot = seed is null ? await Apply(profile) : await ApplySeed(profile, seed);
        foreach (var name in new[] { "ItemSelectedBg", "ItemHoverBg", "ItemActiveBg", "HorizontalItemSelectedBg", "HorizontalItemHoverBg" })
        {
            Own<Color>(snapshot, "NavMenu", name).ShouldBe(Color.Parse(background), name);
        }
        foreach (var name in new[] { "ItemSelectedColor", "ItemHoverColor", "HorizontalItemSelectedColor" })
        {
            Own<Color>(snapshot, "NavMenu", name).ShouldBe(Color.Parse(text), name);
        }
        // PrimeUIX menubar has no indicator bar; its items take the content radius.
        Own<double>(snapshot, "NavMenu", "ActiveBarHeight").ShouldBe(0);
        Own<CornerRadius>(snapshot, "NavMenu", "HorizontalItemBorderRadius").ShouldBe(Global<CornerRadius>(snapshot, "BorderRadius"));

        // The selected text is readable on the selected background and, for the selected-path headers, on the menu.
        var item = Own<Color>(snapshot, "NavMenu", "ItemBg");
        ContrastRatio.Of(Color.Parse(text), ContrastRatio.Composite(Color.Parse(background), item)).ShouldBeGreaterThanOrEqualTo(4.5);
        ContrastRatio.Of(Color.Parse(text), Global<Color>(snapshot, "ColorBgContainer")).ShouldBeGreaterThanOrEqualTo(4.5);
    }

    public static TheoryData<string> AllProfiles() =>
        new(new[] { "PrimeNGAura", "PrimeNGLara", "PrimeNGNora" }
            .SelectMany(id => new[] { id, id + ".Dark", id + ".Compact", id + ".DarkCompact" }));

    [AvaloniaTheory]
    [MemberData(nameof(AllProfiles))]
    public async Task Profiles_Default_To_Noir(string profile)
    {
        // primeng.dev (primeng 22.1.5) starts every preset on primary "noir": slate-950 is the seed, and the primary
        // scale is the surface scale (light slate 950 / 800 / 700, dark zinc 50 / 200 / 300).
        var manager = Application.Current!.GetThemeManager()!;
        manager.AvailableThemes.Single(theme => theme.Id == profile).AccentColor.ShouldBe(Color.Parse("#020617"));

        var snapshot = await Apply(profile);
        var dark = profile.Contains("Dark", StringComparison.Ordinal);
        var expected = dark
            ? new[] { "#FAFAFA", "#E4E4E7", "#D4D4D8", "#09090B" }
            : new[] { "#020617", "#1E293B", "#334155", "#FFFFFF" };
        Global<Color>(snapshot, "ColorPrimary").ShouldBe(Color.Parse(expected[0]), profile);
        Global<Color>(snapshot, "ColorPrimaryHover").ShouldBe(Color.Parse(expected[1]), profile);
        Global<Color>(snapshot, "ColorPrimaryActive").ShouldBe(Color.Parse(expected[2]), profile);
        Global<Color>(snapshot, "ColorTextLightSolid").ShouldBe(Color.Parse(expected[3]), profile);
        Global<Color>(snapshot, "SelectionForeground").ShouldBe(Color.Parse(expected[3]), profile);

        // PrimeUIX checkbox / radiobutton icon.checked.color is primary.contrast.color: the checked marks take the
        // contrast colour, so they stay visible on the near-white dark primary.
        Effective<Color>(snapshot, "CheckBox", "ColorWhite").ShouldBe(Color.Parse(expected[3]), profile);
        Own<Color>(snapshot, "RadioButton", "RadioColor").ShouldBe(Color.Parse(expected[3]), profile);
    }

    [AvaloniaTheory]
    [MemberData(nameof(AllProfiles))]
    public async Task Noir_Stays_Readable(string profile)
    {
        const double text = 4.5;
        var snapshot = await Apply(profile);
        var container = Global<Color>(snapshot, "ColorBgContainer");
        var primary = Global<Color>(snapshot, "ColorPrimary");

        void Check(string what, Color foreground, Color background, double floor)
        {
            var ratio = ContrastRatio.Of(foreground, ContrastRatio.Composite(background, container));
            ratio.ShouldBeGreaterThanOrEqualTo(floor, $"{profile} {what}: {ratio:F2}");
        }

        // Text on the primary fill (primary buttons, the text selection, checked marks).
        Check("ColorTextLightSolid on ColorPrimary", Global<Color>(snapshot, "ColorTextLightSolid"), primary, text);
        Check("CheckBox mark", Effective<Color>(snapshot, "CheckBox", "ColorWhite"), primary, text);
        Check("Radio dot", Own<Color>(snapshot, "RadioButton", "RadioColor"), Own<Color>(snapshot, "RadioButton", "RadioBgColor"), text);
        // Primary-coloured text on the page background (links, selected tab labels, ColorPrimaryText*).
        foreach (var name in new[] { "ColorPrimary", "ColorPrimaryHover", "ColorPrimaryActive", "ColorPrimaryText",
                                     "ColorPrimaryTextHover", "ColorPrimaryTextActive", "ColorLink", "ColorLinkHover",
                                     "ColorLinkActive" })
        {
            Check($"{name} on ColorBgContainer", Global<Color>(snapshot, name), container, text);
        }
        // The selected item: its selected-text token on the selected (highlight) and selected-hover (focus) backgrounds.
        foreach (var (owner, color, background) in SelectedTextTokens)
        {
            Check($"{owner}.{color} on {background}", Own<Color>(snapshot, owner, color), Own<Color>(snapshot, owner, background), text);
            Check($"{owner}.{color} on ControlItemBgActiveHover", Own<Color>(snapshot, owner, color),
                Effective<Color>(snapshot, owner, "ControlItemBgActiveHover"), text);
        }
        // The Filled primary button and the selected NavMenu item draw the primary on a primary background.
        Check("Filled button", primary, Effective<Color>(snapshot, "Button", "ColorPrimaryBg"), text);
        Check("Filled button hover", primary, Effective<Color>(snapshot, "Button", "ColorPrimaryBgHover"), text);
        Check("NavMenu selected item", Own<Color>(snapshot, "NavMenu", "ItemSelectedColor"), Own<Color>(snapshot, "NavMenu", "ItemSelectedBg"), text);
        // The completed step's check is an icon: the 3:1 non-text floor.
        Check("Steps finish icon", Own<Color>(snapshot, "Steps", "FinishIconColor"), Own<Color>(snapshot, "Steps", "FinishIconBgColor"), 3);
    }

    /// <summary>
    /// The controls that draw a selected item's text on the selected background, with their selected-text and
    /// selected-background Own tokens. The PrimeNG control stages set the text to PrimeUIX <c>highlight.color</c>.
    /// </summary>
    private static readonly (string Owner, string Color, string Background)[] SelectedTextTokens =
    [
        ("Select", "OptionSelectedColor", "OptionSelectedBg"),
        ("Cascader", "OptionSelectedColor", "OptionSelectedBg"),
        ("ComboBox", "ItemSelectedColor", "ItemSelectedBgColor"),
        ("ListBox", "ItemSelectedColor", "ItemSelectedBgColor"),
        ("ListView", "ItemSelectedColor", "ItemSelectedBgColor"),
        ("TransferListView", "ItemSelectedColor", "ItemSelectedBgColor"),
        ("TreeView", "NodeSelectedColor", "NodeSelectedBg"),
        ("TransferTreeView", "NodeSelectedColor", "NodeSelectedBg"),
        ("TreeSelect", "NodeSelectedColor", "NodeSelectedBg")
    ];

    // @primeuix/themes 3.1.1 highlight (background, focusBackground, color) and the site's noir highlight, at the
    // default noir seed and the emerald seed. Aura and Lara are pale: P50 / P100 with P700 in light, P400 at 16% / 24%
    // with white at 87% in dark. Nora and noir are solid with the contrast colour.
    [AvaloniaTheory]
    [InlineData("PrimeNGAura", null, "#020617", "#334155", "#FFFFFF")]
    [InlineData("PrimeNGAura.Dark", null, "#FAFAFA", "#D4D4D8", "#09090B")]
    [InlineData("PrimeNGLara", null, "#020617", "#334155", "#FFFFFF")]
    [InlineData("PrimeNGLara.Dark", null, "#FAFAFA", "#D4D4D8", "#09090B")]
    [InlineData("PrimeNGNora", null, "#020617", "#334155", "#FFFFFF")]
    [InlineData("PrimeNGNora.Dark", null, "#FAFAFA", "#D4D4D8", "#09090B")]
    [InlineData("PrimeNGAura", "#10B981", "#ECFDF5", "#D1FAE5", "#047857")]
    [InlineData("PrimeNGAura.Dark", "#10B981", "#2934D399", "#3D34D399", "#DEFFFFFF")]
    [InlineData("PrimeNGLara", "#10B981", "#ECFDF5", "#D1FAE5", "#047857")]
    [InlineData("PrimeNGLara.Dark", "#10B981", "#2934D399", "#3D34D399", "#DEFFFFFF")]
    [InlineData("PrimeNGNora", "#10B981", "#059669", "#047857", "#FFFFFF")]
    [InlineData("PrimeNGNora.Dark", "#10B981", "#10B981", "#34D399", "#09090B")]
    public async Task Highlight_Follows_PrimeUIX(string profile, string? seed, string background, string focus, string text)
    {
        var snapshot = seed is null ? await Apply(profile) : await ApplySeed(profile, seed);
        Global<Color>(snapshot, "ControlItemBgActive").ShouldBe(Color.Parse(background));
        Global<Color>(snapshot, "ControlItemBgActiveHover").ShouldBe(Color.Parse(focus));
        // highlight.color is written once, to the global ControlItemColorActive; the selected-text tokens default to it.
        Global<Color>(snapshot, "ControlItemColorActive").ShouldBe(Color.Parse(text));
        foreach (var (owner, color, selected) in SelectedTextTokens)
        {
            Own<Color>(snapshot, owner, color).ShouldBe(Color.Parse(text), $"{owner}.{color}");
            Own<Color>(snapshot, owner, selected).ShouldBe(Color.Parse(background), $"{owner}.{selected}");
        }
    }

    [AvaloniaTheory]
    [InlineData("PrimeNGAura", null, true)]
    [InlineData("PrimeNGNora", "#10B981", true)]
    [InlineData("PrimeNGNora.Dark", "#10B981", true)]
    [InlineData("PrimeNGAura", "#10B981", false)]
    public async Task Date_And_Time_Cells_Follow_The_Highlight(string profile, string? seed, bool solid)
    {
        // The pickers follow PrimeUIX datepicker: the range middle and the selected time cell are the highlight
        // (ControlItemBgActive with ControlItemColorActive). The content presentation of Calendar and DateViewer draws
        // today's / the selected cell in the primary on the highlight; under a solid highlight it takes the highlight text.
        var snapshot = seed is null ? await Apply(profile) : await ApplySeed(profile, seed);
        foreach (var owner in new[] { "DatePicker", "RangeDatePicker", "TimePicker", "RangeTimePicker" })
        {
            Effective<Color>(snapshot, owner, "ControlItemBgActive").ShouldBe(Global<Color>(snapshot, "ControlItemBgActive"), owner);
        }
        var expected = solid ? Global<Color>(snapshot, "ControlItemColorActive") : Global<Color>(snapshot, "ColorPrimary");
        Own<Color>(snapshot, "Calendar", "ItemActiveColor").ShouldBe(expected);
        Own<Color>(snapshot, "DateViewer", "CellActiveColor").ShouldBe(expected);
        Own<Color>(snapshot, "RangeDateViewer", "CellActiveColor").ShouldBe(expected);
        Own<Color>(snapshot, "ListBox", "ItemSelectedColor")
            .ShouldBe(Global<Color>(snapshot, "ControlItemColorActive"));
    }

    // PrimeUIX datepicker date.selected / month.selected: primary.color with primary.contrast.color. The LunarCalendar
    // draws its selected cells' text in the contrast colour, so its selected background is the primary in every preset
    // (Aura / Lara with a coloured seed would otherwise put white on primary 50).
    [AvaloniaTheory]
    [InlineData("PrimeNGAura", null)]
    [InlineData("PrimeNGAura.Dark", null)]
    [InlineData("PrimeNGAura", "#10B981")]
    [InlineData("PrimeNGLara.Dark", "#10B981")]
    [InlineData("PrimeNGNora", "#6366F1")]
    [InlineData("PrimeNGNora.Dark", "#6366F1")]
    public async Task LunarCalendar_Selected_Cells_Follow_The_Datepicker(string profile, string? seed)
    {
        var snapshot = seed is null ? await Apply(profile) : await ApplySeed(profile, seed);
        var primary  = Global<Color>(snapshot, "ColorPrimary");
        var contrast = Global<Color>(snapshot, "ColorTextLightSolid");
        Own<Color>(snapshot, "LunarCalendar", "ItemActiveBg").ShouldBe(primary);
        Own<Color>(snapshot, "LunarCalendar", "ItemActiveColor").ShouldBe(contrast);
        // The lunar cell content's selected text is the control's ColorTextLightSolid: the contrast colour.
        Effective<Color>(snapshot, "LunarCalendar", "ColorTextLightSolid").ShouldBe(contrast);
    }

    // Seeds other than emerald: PrimeUIX's solid highlight with the contrast colour. Measured, with the 3:1 floor of
    // large text; the values are listed in the README.
    [AvaloniaTheory]
    [InlineData("PrimeNGNora", "#3B82F6")]
    [InlineData("PrimeNGNora.Dark", "#3B82F6")]
    [InlineData("PrimeNGNora", "#6366F1")]
    [InlineData("PrimeNGNora.Dark", "#6366F1")]
    public async Task Solid_Highlight_Text_Stays_Readable_For_Other_Seeds(string profile, string seed)
    {
        var snapshot = await ApplySeed(profile, seed);
        foreach (var background in new[] { "ControlItemBgActive", "ControlItemBgActiveHover" })
        {
            var ratio = ContrastRatio.Of(Own<Color>(snapshot, "Select", "OptionSelectedColor"), Global<Color>(snapshot, background));
            ratio.ShouldBeGreaterThanOrEqualTo(3.0, $"{profile} {seed} on {background}: {ratio:F2}");
        }
    }

    // PrimeUIX 3.1.1 toggleswitch.handle.checked.background: Aura / Lara light-dark(surface.0, surface.900), Nora
    // primary.contrast.color.
    [AvaloniaTheory]
    [InlineData("PrimeNGAura", null, "#FFFFFF")]
    [InlineData("PrimeNGAura.Dark", null, "#18181B")]
    [InlineData("PrimeNGLara.Dark", null, "#18181B")]
    [InlineData("PrimeNGNora", null, "#FFFFFF")]
    [InlineData("PrimeNGNora.Dark", null, "#09090B")]
    [InlineData("PrimeNGAura.Dark", "#10B981", "#18181B")]
    [InlineData("PrimeNGNora.Dark", "#10B981", "#09090B")]
    public async Task Checked_Switch_Handle_Follows_PrimeUIX(string profile, string? seed, string handle)
    {
        var snapshot = seed is null ? await Apply(profile) : await ApplySeed(profile, seed);
        Own<Color>(snapshot, "ToggleSwitch", "HandleCheckedBg").ShouldBe(Color.Parse(handle));
        Own<Color>(snapshot, "ToggleSwitch", "HandleCheckedHoverBg").ShouldBe(Color.Parse(handle));
        var ratio = ContrastRatio.Of(Color.Parse(handle), Own<Color>(snapshot, "ToggleSwitch", "SwitchColor"));
        ratio.ShouldBeGreaterThanOrEqualTo(3.0, $"{profile} checked handle on the track: {ratio:F2}");
    }

    // PrimeUIX 3.1.1 toggleswitch, unchecked state and hovers. Aura / Lara: background light-dark(surface.300,
    // surface.700), hoverBackground light-dark(surface.400, surface.600), handle.background light-dark(surface.0,
    // surface.400), handle.hoverBackground light-dark(surface.0, surface.300), checkedHoverBackground primary.hover.color,
    // transparent borders. Nora: background = hoverBackground = form.field.background (surface.0 / surface.950), borderColor
    // form.field.border.color (surface.500), checkedBorderColor primary.color, handle form.field.border.color,
    // checkedHoverBackground primary.color.
    [AvaloniaTheory]
    [InlineData("PrimeNGAura", null, "#CBD5E1", "#94A3B8", "#FFFFFF", "#FFFFFF", null)]
    [InlineData("PrimeNGAura.Dark", null, "#3F3F46", "#52525B", "#A1A1AA", "#D4D4D8", null)]
    [InlineData("PrimeNGLara", null, "#CBD5E1", "#94A3B8", "#FFFFFF", "#FFFFFF", null)]
    [InlineData("PrimeNGLara.Dark", null, "#3F3F46", "#52525B", "#A1A1AA", "#D4D4D8", null)]
    [InlineData("PrimeNGNora", null, "#FFFFFF", "#FFFFFF", "#64748B", "#64748B", "#64748B")]
    [InlineData("PrimeNGNora.Dark", null, "#09090B", "#09090B", "#71717A", "#71717A", "#71717A")]
    [InlineData("PrimeNGAura", "#10B981", "#CBD5E1", "#94A3B8", "#FFFFFF", "#FFFFFF", null)]
    [InlineData("PrimeNGNora", "#6366F1", "#FFFFFF", "#FFFFFF", "#64748B", "#64748B", "#64748B")]
    public async Task Switch_Track_And_Handle_Follow_PrimeUIX(string profile, string? seed, string track, string trackHover,
                                                               string handle, string handleHover, string? border)
    {
        var snapshot = seed is null ? await Apply(profile) : await ApplySeed(profile, seed);
        var primary  = Global<Color>(snapshot, "ColorPrimary");
        Own<Color>(snapshot, "ToggleSwitch", "TrackBg").ShouldBe(Color.Parse(track));
        Own<Color>(snapshot, "ToggleSwitch", "TrackHoverBg").ShouldBe(Color.Parse(trackHover));
        Own<Color>(snapshot, "ToggleSwitch", "HandleBg").ShouldBe(Color.Parse(handle));
        Own<Color>(snapshot, "ToggleSwitch", "HandleHoverBg").ShouldBe(Color.Parse(handleHover));
        Own<Color>(snapshot, "ToggleSwitch", "SwitchColor").ShouldBe(primary);
        if (border is null)
        {
            Own<double>(snapshot, "ToggleSwitch", "TrackBorderThickness").ShouldBe(0);
            Own<Color>(snapshot, "ToggleSwitch", "TrackCheckedHoverBg").ShouldBe(Global<Color>(snapshot, "ColorPrimaryHover"));
        }
        else
        {
            Own<double>(snapshot, "ToggleSwitch", "TrackBorderThickness").ShouldBe(Global<double>(snapshot, "LineWidth"));
            Own<Color>(snapshot, "ToggleSwitch", "TrackBorderColor").ShouldBe(Color.Parse(border));
            Own<Color>(snapshot, "ToggleSwitch", "TrackCheckedBorderColor").ShouldBe(primary);
            Own<Color>(snapshot, "ToggleSwitch", "TrackCheckedHoverBg").ShouldBe(primary);
        }
        // The unchecked handle stays distinguishable from its track (non-text 3:1 except Aura / Lara light, where
        // PrimeUIX's white handle on surface 300 relies on its shadow: 1.48).
        var ratio = ContrastRatio.Of(Color.Parse(handle), Color.Parse(track));
        ratio.ShouldBeGreaterThanOrEqualTo(profile.Contains("Nora", StringComparison.Ordinal) || profile.Contains("Dark", StringComparison.Ordinal) ? 3.0 : 1.4,
            $"{profile} unchecked handle on the track: {ratio:F2}");
    }

    [AvaloniaTheory]
    [InlineData("PrimeNGAura.Dark")]
    [InlineData("PrimeNGLara.Dark")]
    [InlineData("PrimeNGNora.Dark")]
    public async Task Coloured_Primary_Keeps_White_Check_Marks(string profile)
    {
        // Only noir moves the checked marks to the contrast colour; a coloured primary keeps today's white marks.
        var snapshot = await ApplyEmerald(profile);
        Effective<Color>(snapshot, "CheckBox", "ColorWhite").ShouldBe(Colors.White);
        Own<Color>(snapshot, "RadioButton", "RadioColor").ShouldBe(Colors.White);
    }

    private static string StripMetadata(string path, string algorithmId)
    {
        var root = XDocument.Load(path).Root!;
        root.Attributes().Remove();
        var algorithm = root.Element(ThemeNamespace + "Algorithms")!
                            .Elements(ThemeNamespace + "Algorithm")
                            .Single(element => element.Attribute("Catalog")?.Value == "PrimeNG");
        algorithm.Attribute("Id")!.Value.ShouldBe(algorithmId);
        algorithm.Remove();
        return root.ToString(SaveOptions.DisableFormatting);
    }

    private static async Task<string> Dump(string profile, params ThemeAlgorithm[] chain)
    {
        var config = chain.Length == 0 ? null : new ThemeConfigBuilder().WithAlgorithms(chain).Build();
        var snapshot = await Apply(new ThemeRequest(profile, config, ThemeTransitionReason.UserRequest));
        return ThemeSnapshotDump.Dump(snapshot);
    }

    // The pre-noir default seed: emerald-500, the raw @primeuix/themes preset value. Tests of the coloured-primary
    // rules apply it explicitly.
    private static Task<ThemeSnapshot> ApplySeed(string id, string seed) =>
        Apply(new ThemeRequest(id, new ThemeConfigBuilder().WithToken("ColorPrimary", seed).Build(),
            ThemeTransitionReason.UserRequest));

    private static Task<ThemeSnapshot> ApplyEmerald(string id) =>
        Apply(new ThemeRequest(id, new ThemeConfigBuilder().WithToken("ColorPrimary", "#10B981").Build(),
            ThemeTransitionReason.UserRequest));

    private static Task<ThemeSnapshot> Apply(string id) =>
        Apply(new ThemeRequest(id, null, ThemeTransitionReason.UserRequest));

    [AvaloniaTheory]
    [InlineData("Aura")]
    [InlineData("Lara")]
    [InlineData("Nora")]
    public void Profiles_Declare_Design_Language_And_Brand_Support(string family)
    {
        var manager = Application.Current!.GetThemeManager()!;
        var expected = new (string Suffix, ThemeAppearance Appearance, ThemeDensity Density)[]
        {
            ("", ThemeAppearance.Light, ThemeDensity.Normal),
            (".Dark", ThemeAppearance.Dark, ThemeDensity.Normal),
            (".Compact", ThemeAppearance.Light, ThemeDensity.Compact),
            (".DarkCompact", ThemeAppearance.Dark, ThemeDensity.Compact)
        };
        foreach (var (suffix, appearance, density) in expected)
        {
            var info = manager.AvailableThemes.Single(theme => theme.Id == "PrimeNG" + family + suffix);
            info.DesignLanguage.ShouldBe("PrimeNG." + family);
            info.BrandColor.ShouldBe(ThemeBrandColorSupport.Seed);
            info.Appearance.ShouldBe(appearance);
            info.Density.ShouldBe(density);
        }

        manager.ThemeCatalogDiagnostics.ShouldBeEmpty();
    }

    [AvaloniaTheory]
    [InlineData("PrimeNGAura", "PrimeNGAura.Dark")]
    [InlineData("PrimeNGLara", "PrimeNGLara.Dark")]
    [InlineData("PrimeNGNora", "PrimeNGNora.Dark")]
    public async Task Brand_Seed_Drives_Derived_States_In_Both_Appearances(string light, string dark)
    {
        // #123456 is not a Tailwind 500 stop, so the palette formula must run and move every derived brand token.
        var seed = Color.Parse("#123456");
        var config = new ThemeConfigBuilder().WithToken("ColorPrimary", "#123456").Build();
        var lightDefault = await Apply(light);
        var darkDefault = await Apply(dark);
        var lightSnapshot = await Apply(new ThemeRequest(light, config, ThemeTransitionReason.UserRequest));
        var darkSnapshot = await Apply(new ThemeRequest(dark, config, ThemeTransitionReason.UserRequest));

        foreach (var (defaults, seeded) in new[] { (lightDefault, lightSnapshot), (darkDefault, darkSnapshot) })
        {
            foreach (var name in new[] { "ColorPrimary", "ColorPrimaryHover", "ColorPrimaryBg", "ColorControlOutline" })
            {
                Global<Color>(seeded, name).ShouldNotBe(Global<Color>(defaults, name), name);
            }
        }

        Global<Color>(darkSnapshot, "ColorPrimary").ShouldNotBe(Global<Color>(lightSnapshot, "ColorPrimary"));
        Global<Color>(lightSnapshot, "ColorPrimaryHover").ShouldNotBe(seed);
    }

    private static async Task<ThemeSnapshot> Apply(ThemeRequest request)
    {
        var manager = (ThemeManager)Application.Current!.GetThemeManager()!;
        (await manager.ApplyThemeAsync(request, TestContext.Current.CancellationToken))
            .Status.ShouldBe(ThemeTransitionStatus.Committed);
        return manager.CurrentSnapshot!;
    }

    private static T Global<T>(ThemeSnapshot snapshot, string name)
    {
        snapshot.Registry.TryGetGlobalToken(name, out var token).ShouldBeTrue();
        return snapshot.GlobalTokenValues.Get<T>(token!.Slot);
    }

    private static ControlTokenDescriptor Owner(ThemeSnapshot snapshot, string id)
    {
        snapshot.Registry.TryGetControl(new ControlTokenIdentity("AtomUI", id), out var control).ShouldBeTrue();
        return control!;
    }

    private static T Own<T>(ThemeSnapshot snapshot, string owner, string name)
    {
        var control = Owner(snapshot, owner);
        var token = control.OwnTokens.Single(token => token.Name == name);
        return snapshot.Controls[control.Slot].ControlTokenValues.Get<T>(token.Slot);
    }

    private static T Effective<T>(ThemeSnapshot snapshot, string owner, string name)
    {
        var control = Owner(snapshot, owner);
        snapshot.Registry.TryGetGlobalToken(name, out var token).ShouldBeTrue();
        return snapshot.Controls[control.Slot].GetEffectiveGlobalValue<T>(snapshot.GlobalTokenValues, token!.Slot);
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "src", "AtomUI.Themes.PrimeNG", "Themes")))
            {
                return directory.FullName;
            }
        }
        throw new DirectoryNotFoundException("The repository root (with src/AtomUI.Themes.PrimeNG/Themes) was not found.");
    }
}
