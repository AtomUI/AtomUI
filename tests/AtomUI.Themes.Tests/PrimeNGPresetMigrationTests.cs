using System.Xml.Linq;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Themes.Tests;

/// <summary>
/// One PrimeNG preset that has moved onto its <c>PrimeNG:&lt;Preset&gt;</c> / <c>PrimeNG:&lt;Preset&gt;.Dark</c>
/// algorithms, as the migration tests see it.
/// </summary>
/// <param name="Preset">The preset name (<c>Aura</c>, <c>Lara</c>); the profile ids are <c>PrimeNG&lt;Preset&gt;</c> plus the suffixes.</param>
/// <param name="CompactNonEquivalentSlots">
/// The README's "不等价项": slots where the base profile + <c>AtomUI:Compact</c> differs from the compact profile, as
/// snapshot keys. They are also the only control values the compact XML may still hold. Keep in sync with the README.
/// </param>
/// <param name="XmlTextExceptions">
/// Explicit tokens of the old XML that never compiled to the value the XML wrote, per profile; the explicit migration
/// check expects the XML text for them instead of the baseline (Task 18 ruling).
/// </param>
/// <param name="ExtraRuleTokens">Tokens beyond the shared list that the preset's rules produce, so its XML must not set them.</param>
internal sealed record PrimeNGPresetMigration(
    string Preset,
    string[] CompactNonEquivalentSlots,
    (string Profile, string Token)[] XmlTextExceptions,
    string[] ExtraRuleTokens)
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
/// The PrimeNG profiles after they moved onto their preset algorithms (spec 4.8), one descriptor per preset: the dark
/// profile is the light one with the dark chain, the compact profile is the base one with <c>AtomUI:Compact</c>
/// appended (except the slots the README lists as not equivalent), and the XML no longer spells out what the rules
/// produce.
/// </summary>
public class PrimeNGPresetMigrationTests
{
    private static readonly XNamespace ThemeNamespace = "https://atomui.net/schemas/theme/v1";

    /// <summary>
    /// In the old dark profiles the chain <c>[AtomUI:Dark]</c> re-derived the <c>ColorPrimary</c> and
    /// <c>ColorError</c> seeds in its Map stage, so the baseline holds <c>#2FB685</c> and <c>#D98F8F</c>; the
    /// algorithm reaches the author's values (<c>#34D399</c>, <c>#FCA5A5</c>) from the shared 500 seeds.
    /// </summary>
    private static (string Profile, string Token)[] DarkSeedExceptions(string preset) =>
    [
        ($"PrimeNG{preset}.Dark", "ColorPrimary"),
        ($"PrimeNG{preset}.Dark", "ColorError"),
        ($"PrimeNG{preset}.DarkCompact", "ColorPrimary"),
        ($"PrimeNG{preset}.DarkCompact", "ColorError")
    ];

    private static readonly Dictionary<string, PrimeNGPresetMigration> Presets = new[]
    {
        new PrimeNGPresetMigration("Aura", [], DarkSeedExceptions("Aura"), []),
        new PrimeNGPresetMigration("Lara", ["C AtomUI:Select PopupContentPadding"], DarkSeedExceptions("Lara"),
            ["ColorControlOutline"])
    }.ToDictionary(preset => preset.Preset, StringComparer.Ordinal);

    private static readonly string[] SharedRuleTokens =
    [
        "ColorPrimaryHover", "ColorPrimaryActive", "ColorPrimaryBg", "ColorPrimaryText", "ColorTextLightSolid",
        "ColorText", "ColorTextSecondary", "ColorTextDescription", "ColorTextPlaceholder", "ColorBgContainer",
        "ColorBgElevated", "ColorBgBase", "ColorTextBase", "ColorFillTertiary", "ColorBgContainerDisabled",
        "ColorBorderSecondary", "ColorBorder", "ColorBgMask", "ColorFocusBorder", "FocusVisualBorderThickness",
        "ControlOutlineWidth", "BorderRadiusXS", "BorderRadiusSM", "BorderRadiusLG", "ControlHeightSM",
        "ControlHeightLG", "PaddingContentHorizontal", "BoxShadows", "BoxShadowsSecondary"
    ];

    [AvaloniaTheory(Explicit = true)]
    [InlineData("PrimeNGAura")]
    [InlineData("PrimeNGAura.Dark")]
    [InlineData("PrimeNGAura.Compact")]
    [InlineData("PrimeNGAura.DarkCompact")]
    [InlineData("PrimeNGLara")]
    [InlineData("PrimeNGLara.Dark")]
    [InlineData("PrimeNGLara.Compact")]
    [InlineData("PrimeNGLara.DarkCompact")]
    public async Task Tokens_The_Old_Xml_Set_Explicitly_Keep_Their_Values(string profile)
    {
        // Temporary migration check (removed with the snapshot tool in Task 21). Reads the pre-change XML copy and
        // the baseline dump from ATOMUI_PRIMENG_BASELINE_DIR.
        var preset = Presets.Values.Single(preset => preset.Profiles.Contains(profile));
        var exceptions = preset.XmlTextExceptions
                               .Where(exception => exception.Profile == profile)
                               .Select(exception => exception.Token)
                               .ToArray();
        var snapshot = await Apply(new ThemeRequest(profile, null, ThemeTransitionReason.UserRequest));

        var mismatches = PrimeNGMigrationSnapshot.CompareExplicitXmlTokens(snapshot, profile, exceptions, out var checkedCount);

        checkedCount.ShouldBeGreaterThan(40);
        mismatches.ShouldBeEmpty();
    }

    [AvaloniaTheory]
    [InlineData("Aura")]
    [InlineData("Lara")]
    public async Task Swapping_The_Chain_To_The_Dark_Algorithm_Equals_The_Dark_Profile(string presetName)
    {
        var preset = Presets[presetName];
        var swapped = await Dump(preset.BaseProfile, preset.Dark);
        var dark = await Dump(preset.DarkProfile);
        PrimeNGMigrationSnapshot.Diff(dark, swapped).ShouldBeEmpty();

        var swappedCompact = await Dump(preset.CompactProfile, preset.Dark, ThemeAlgorithm.Compact);
        var darkCompact = await Dump(preset.DarkCompactProfile);
        PrimeNGMigrationSnapshot.Diff(darkCompact, swappedCompact).ShouldBeEmpty();
    }

    [AvaloniaTheory]
    [InlineData("Aura")]
    [InlineData("Lara")]
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
            var slots = PrimeNGMigrationSnapshot.Diff(compact, appended)
                                                .Select(SlotOf)
                                                .ToArray();
            slots.ShouldBe(preset.CompactNonEquivalentSlots, ignoreOrder: true, baseId);
        }
    }

    [AvaloniaTheory]
    [InlineData("Aura")]
    [InlineData("Lara")]
    public async Task Primary_Seed_Change_Moves_Focus_Ring_And_Selected_Background(string presetName)
    {
        var profile = Presets[presetName].BaseProfile;
        var emerald = await Apply(new ThemeRequest(profile, null, ThemeTransitionReason.UserRequest));
        var emeraldSelected = Global<Color>(emerald, "ControlItemBgActive");

        var config = new ThemeConfigBuilder().WithToken("ColorPrimary", "#3B82F6").Build();
        var blue = await Apply(new ThemeRequest(profile, config, ThemeTransitionReason.UserRequest));
        Global<Color>(blue, "ColorPrimary").ShouldBe(Color.Parse("#3B82F6"));
        Global<Color>(blue, "ColorFocusBorder").ShouldBe(Global<Color>(blue, "ColorPrimary"));
        Global<Color>(blue, "ControlItemBgActive").ShouldNotBe(emeraldSelected);
        Global<Color>(blue, "ControlItemBgActive").ShouldBe(Color.Parse("#EFF6FF"));
    }

    [Theory]
    [InlineData("Aura")]
    [InlineData("Lara")]
    public void Preset_Xml_No_Longer_Contains_Rule_Tokens(string presetName)
    {
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

            // Every per-control value is a control stage now, except the compact slots listed as not equivalent;
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
    public async Task Lara_Keeps_Its_Larger_Control_Height_And_Select_Padding()
    {
        var snapshot = await Apply(new ThemeRequest("PrimeNGLara", null, ThemeTransitionReason.UserRequest));
        Global<double>(snapshot, "ControlHeight").ShouldBe(44);
        Own<Thickness>(snapshot, "Select", "OptionPadding").ShouldBe(new Thickness(16, 10));
        Own<Thickness>(snapshot, "Select", "PopupContentPadding").ShouldBe(new Thickness(0, 8));
    }

    [AvaloniaFact]
    public async Task Lara_Focus_Glow_Follows_The_Primary_Seed()
    {
        var emerald = await Apply(new ThemeRequest("PrimeNGLara", null, ThemeTransitionReason.UserRequest));
        Global<Color>(emerald, "ColorControlOutline").ShouldBe(Color.Parse("#A7F3D0"));

        // Lara's focus glow is the primary 200 stop: it follows the seed (blue-200).
        var config = new ThemeConfigBuilder().WithToken("ColorPrimary", "#3B82F6").Build();
        var blue = await Apply(new ThemeRequest("PrimeNGLara", config, ThemeTransitionReason.UserRequest));
        Global<Color>(blue, "ColorControlOutline").ShouldBe(Color.Parse("#BFDBFE"));
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

    private static string SlotOf(string difference)
    {
        // "changed G X: a -> b", "added C A:B X = v", "removed G X = v"
        var body = difference[(difference.IndexOf(' ') + 1)..];
        var end = body.IndexOf(difference.StartsWith("changed", StringComparison.Ordinal) ? ": " : " = ",
            StringComparison.Ordinal);
        return body[..end];
    }

    private static async Task<string> Dump(string profile, params ThemeAlgorithm[] chain)
    {
        var config = chain.Length == 0 ? null : new ThemeConfigBuilder().WithAlgorithms(chain).Build();
        var snapshot = await Apply(new ThemeRequest(profile, config, ThemeTransitionReason.UserRequest));
        return PrimeNGMigrationSnapshot.Dump(snapshot);
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

    private static T Own<T>(ThemeSnapshot snapshot, string owner, string name)
    {
        snapshot.Registry.TryGetControl(new ControlTokenIdentity("AtomUI", owner), out var control).ShouldBeTrue();
        var token = control!.OwnTokens.Single(token => token.Name == name);
        return snapshot.Controls[control.Slot].ControlTokenValues.Get<T>(token.Slot);
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
