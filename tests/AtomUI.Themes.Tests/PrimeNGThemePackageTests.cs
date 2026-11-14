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
            var normalHeight = id == "PrimeNGLara" ? 44d : 40d;
            Global<double>(snapshot, "ControlHeight").ShouldBe(normalHeight - (suffix.Contains("Compact") ? 4 : 0));
            if (suffix.Contains("Compact"))
            {
                var height = normalHeight - 4;
                Global<double>(snapshot, "ControlHeightSM").ShouldBe(height * 0.75);
                Global<double>(snapshot, "ControlHeightLG").ShouldBe(height * 1.25);
                Global<double>(snapshot, "PaddingContentHorizontal").ShouldBe(8);
            }
            if (id == "PrimeNGLara" && !suffix.Contains("Compact"))
            {
                Own<Thickness>(snapshot, "Select", "OptionPadding").ShouldBe(new Thickness(16, 10));
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
    [InlineData("Aura", "#3B82F6")]
    [InlineData("Lara", "#3B82F6")]
    [InlineData("Nora", "#2563EB")]
    public async Task Primary_Seed_Change_Moves_Focus_Ring_And_Selected_Background(string presetName, string expectedPrimary)
    {
        // The seed is the 500 stop; Aura and Lara take it as the light primary, Nora the 600 stop (blue-600).
        var profile = Presets[presetName].BaseProfile;
        var emerald = await Apply(profile);
        var emeraldSelected = Global<Color>(emerald, "ControlItemBgActive");

        var config = new ThemeConfigBuilder().WithToken("ColorPrimary", "#3B82F6").Build();
        var blue = await Apply(new ThemeRequest(profile, config, ThemeTransitionReason.UserRequest));
        Global<Color>(blue, "ColorPrimary").ShouldBe(Color.Parse(expectedPrimary));
        Global<Color>(blue, "ColorFocusBorder").ShouldBe(Global<Color>(blue, "ColorPrimary"));
        Global<Color>(blue, "ControlItemBgActive").ShouldNotBe(emeraldSelected);
        Global<Color>(blue, "ControlItemBgActive").ShouldBe(Color.Parse("#EFF6FF"));
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
        var snapshot = await Apply("PrimeNGAura");
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
                     ["ControlHeight"] = 40, ["ControlHeightSM"] = 36, ["ControlHeightLG"] = 48,
                     ["PaddingContentHorizontal"] = 12, ["FontSizeSM"] = 12, ["FontSizeLG"] = 16,
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
        Own<double>(snapshot, "CheckBox", "CheckIndicatorSize").ShouldBe(20);
        Own<double>(snapshot, "CheckBox", "CheckedMarkSize").ShouldBe(14);
        Own<Thickness>(snapshot, "Select", "OptionPadding").ShouldBe(new Thickness(12, 8));
        Own<Thickness>(snapshot, "Select", "PopupContentPadding").ShouldBe(new Thickness(4));
        Own<Thickness>(snapshot, "Card", "BodyPadding").ShouldBe(new Thickness(20));
        Own<BoxShadows>(snapshot, "Card", "RestingShadows").Count.ShouldBe(2);
        Own<Color>(snapshot, "ToolTip", "ToolTipBackground").ShouldBe(Color.Parse("#334155"));
        Own<CornerRadius>(snapshot, "ToolTip", "ToolTipCornerRadius").ShouldBe(new CornerRadius(6));
        Own<Thickness>(snapshot, "ToolTip", "ContentPadding").ShouldBe(new Thickness(12, 8));
        Effective<CornerRadius>(snapshot, "Card", "BorderRadiusLG").ShouldBe(new CornerRadius(12));
        Effective<CornerRadius>(snapshot, "Dialog", "BorderRadiusLG").ShouldBe(new CornerRadius(12));
        Own<Thickness>(snapshot, "Dialog", "ContentPadding").ShouldBe(new Thickness(20));
        Own<double>(snapshot, "Dialog", "HeaderFontSize").ShouldBe(20);
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

    [AvaloniaFact]
    public async Task Lara_Uses_Outline_Glow_And_Larger_Controls()
    {
        var snapshot = await Apply("PrimeNGLara");
        Global<double>(snapshot, "ControlOutlineWidth").ShouldBe(3);
        Global<Color>(snapshot, "ColorControlOutline").ShouldBe(Color.Parse("#A7F3D0"));
        var inputActiveShadow = Global<BoxShadows>(snapshot, "InputActiveShadow")[0];
        inputActiveShadow.Spread.ShouldBe(3);
        inputActiveShadow.Color.ShouldBe(Color.Parse("#A7F3D0"));
        Global<Thickness>(snapshot, "FocusVisualBorderThickness").ShouldBe(new Thickness(0));
        Global<double>(snapshot, "ControlHeight").ShouldBe(44);
        Own<double>(snapshot, "CheckBox", "CheckIndicatorSize").ShouldBe(24);
        Own<Thickness>(snapshot, "Select", "OptionPadding").ShouldBe(new Thickness(16, 10));
        Own<Thickness>(snapshot, "Select", "PopupContentPadding").ShouldBe(new Thickness(0, 8));
        Own<FontWeight>(snapshot, "Button", "FontWeight").ShouldBe(FontWeight.SemiBold);
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
            var dark = await Apply(profile);
            Global<Color>(dark, "ColorPrimaryBg").ShouldBe(Color.Parse("#10B981"));
            Global<Color>(dark, "ColorPrimaryText").ShouldBe(Color.Parse("#09090B"));
            Own<Color>(dark, "Steps", "ProcessIconColor").ShouldBe(Color.Parse("#09090B"));
            Own<Color>(dark, "Steps", "FinishTitleColor").ShouldBe(Colors.White);
        }
    }

    [AvaloniaFact]
    public async Task Nora_Takes_Darker_Primary_Stops()
    {
        // Light 600 / 700 / 800 and dark 500 / 400 / 300 of the shared emerald-500 seed (Aura: 500 / 600 / 700 and
        // 400 / 300 / 200).
        var light = await Apply("PrimeNGNora");
        Global<Color>(light, "ColorPrimary").ShouldBe(Color.Parse("#059669"));
        Global<Color>(light, "ColorPrimaryHover").ShouldBe(Color.Parse("#047857"));
        Global<Color>(light, "ColorPrimaryActive").ShouldBe(Color.Parse("#065F46"));
        Global<Color>(light, "ColorFocusBorder").ShouldBe(Color.Parse("#059669"));

        var dark = await Apply("PrimeNGNora.Dark");
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
            var dark = await Apply(profile);
            Own<Color>(dark, "Steps", "FinishIconColor").ShouldBe(Color.Parse("#09090B"), profile);
        }

        // The stage follows the contrast colour, not a constant: a light appearance after Nora.Dark leaves the
        // Ant Design value.
        var hybridConfig = new ThemeConfigBuilder()
                           .WithAlgorithms(new ThemeAlgorithm("PrimeNG", "Nora.Dark"), ThemeAlgorithm.Default)
                           .Build();
        var hybrid = await Apply(new ThemeRequest("PrimeNGNora.Dark", hybridConfig, ThemeTransitionReason.UserRequest));
        Own<Color>(hybrid, "Steps", "FinishIconColor").ShouldBe(Global<Color>(hybrid, "ColorPrimary"));

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
        // Nora dark makes the selected background the solid primary (PrimeUIX highlight.background); AtomUI controls
        // draw ColorText, or the primary colour, on the selected and selected-hover backgrounds, so those pairs keep
        // body-text contrast.
        const double floor = 4.5;
        var dark = await Apply(profile);
        var container = Global<Color>(dark, "ColorBgContainer");

        var selectedHover = ContrastRatio.Of(Global<Color>(dark, "ColorText"), Global<Color>(dark, "ControlItemBgActiveHover"));
        selectedHover.ShouldBeGreaterThanOrEqualTo(floor, $"ColorText on ControlItemBgActiveHover: {selectedHover:F2}");

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

    [AvaloniaFact]
    public async Task Nora_Dark_NavMenu_Stage_Writes_Nothing_Under_A_Light_Appearance()
    {
        var hybridConfig = new ThemeConfigBuilder()
                           .WithAlgorithms(new ThemeAlgorithm("PrimeNG", "Nora.Dark"), ThemeAlgorithm.Default)
                           .Build();
        var hybrid = await Apply(new ThemeRequest("PrimeNGNora.Dark", hybridConfig, ThemeTransitionReason.UserRequest));
        // The Ant Design value: the menu's selected background is ControlItemBgActive.
        Own<Color>(hybrid, "NavMenu", "ItemSelectedBg").ShouldBe(Global<Color>(hybrid, "ControlItemBgActive"));
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

    private static Task<ThemeSnapshot> Apply(string id) =>
        Apply(new ThemeRequest(id, null, ThemeTransitionReason.UserRequest));

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
