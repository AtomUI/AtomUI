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

namespace AtomUI.Themes.Tests.Shadcn;

/// <summary>
/// The Shadcn Nova profiles: declaration metadata, base-color-driven tokens against the appendix, style geometry,
/// readable dark neutrals, and the survival of the neutral seed through appended Compact/Dark chains.
/// </summary>
public class ShadcnPackageTests
{
    [AvaloniaFact]
    public void Nova_Profiles_Declare_Language_And_Seeds()
    {
        var manager = (ThemeManager)Application.Current!.GetThemeManager()!;
        foreach (var suffix in new[] { "", ".Dark", ".Compact", ".DarkCompact" })
        {
            var info = manager.AvailableThemes.Single(theme => theme.Id == "ShadcnNova" + suffix);
            info.DesignLanguage.ShouldBe("Shadcn.Nova");
            info.BrandColor.ShouldBe(ThemeBrandColorSupport.Seed);
            info.NeutralColor.ShouldBe(ThemeNeutralColorSupport.Seed);
            info.Density.ShouldBe(suffix.Contains("Compact") ? ThemeDensity.Compact : ThemeDensity.Normal);
            info.Appearance.ShouldBe(suffix.Contains("Dark") ? ThemeAppearance.Dark : ThemeAppearance.Light);
            info.AccentColor.ShouldBe(Color.Parse("#171717"));
            info.NeutralAccentColor.ShouldBe(Color.Parse("#737373"));
        }

        manager.AvailableThemes.Single(theme => theme.IsDefault).Id.ShouldBe(IThemeManager.DEFAULT_THEME_ID);
        manager.ThemeCatalogDiagnostics.ShouldBeEmpty();
    }

    public static TheoryData<string, bool> FamilyAppearances()
    {
        var data = new TheoryData<string, bool>();
        foreach (var (family, _) in ShadcnAppendix.NeutralSeeds())
        {
            data.Add(family, false);
            data.Add(family, true);
        }

        return data;
    }

    [AvaloniaTheory]
    [MemberData(nameof(FamilyAppearances))]
    public async Task Nova_Colors_Follow_Base_Color(string family, bool dark)
    {
        var seed = ShadcnAppendix.NeutralSeeds().Single(item => item.Family == family).Seed;
        var config = new ThemeConfigBuilder().WithToken("ColorNeutral", Hex(seed)).Build();
        var snapshot = await Apply(dark ? "ShadcnNova.Dark" : "ShadcnNova", config);
        var table = ShadcnAppendix.BaseColors();
        Color Expect(string variable) => table[(family, dark, variable)];

        // The seed survives the whole chain for the later stages to re-read.
        Global<Color>(snapshot, "ColorNeutral").ShouldBe(seed);
        Global<Color>(snapshot, "ColorBgLayout").ShouldBe(Expect("background"));
        Global<Color>(snapshot, "ColorBgContainer").ShouldBe(Expect("background"));
        Global<Color>(snapshot, "ColorBgElevated").ShouldBe(Expect("popover"));
        Global<Color>(snapshot, "ColorBgSpotlight").ShouldBe(Expect("foreground"));
        Global<Color>(snapshot, "ColorText").ShouldBe(Expect("foreground"));
        Global<Color>(snapshot, "ColorTextHeading").ShouldBe(Expect("foreground"));
        Global<Color>(snapshot, "ColorTextSecondary").ShouldBe(Expect("mutedforeground"));
        Global<Color>(snapshot, "ColorTextDescription").ShouldBe(Expect("mutedforeground"));
        Global<Color>(snapshot, "ColorTextPlaceholder").ShouldBe(Expect("mutedforeground"));
        Global<Color>(snapshot, "ColorIcon").ShouldBe(Expect("foreground"));
        Global<Color>(snapshot, "ColorIconHover").ShouldBe(Expect("foreground"));
        Global<Color>(snapshot, "ColorFill").ShouldBe(Expect("accent"));
        Global<Color>(snapshot, "ColorFillSecondary").ShouldBe(Expect("secondary"));
        Global<Color>(snapshot, "ColorFillTertiary").ShouldBe(Expect("muted"));
        Global<Color>(snapshot, "ColorFillQuaternary").ShouldBe(Expect("muted"));
        Global<Color>(snapshot, "ColorFillContent").ShouldBe(Expect("muted"));
        Global<Color>(snapshot, "ColorBgContainerDisabled").ShouldBe(Expect("muted"));
        Global<Color>(snapshot, "ColorBorder").ShouldBe(Expect("border"));
        Global<Color>(snapshot, "ColorBorderSecondary").ShouldBe(Expect("border"));
        Global<Color>(snapshot, "ColorSplit").ShouldBe(Expect("border"));
        Global<Color>(snapshot, "ControlItemBgHover").ShouldBe(Expect("accent"));
        Global<Color>(snapshot, "ControlItemBgActive").ShouldBe(Expect("accent"));
        Global<Color>(snapshot, "ControlItemColorActive").ShouldBe(Expect("accentforeground"));
        Global<Color>(snapshot, "ColorError").ShouldBe(Expect("destructive"));
        // The XML seed is neutral-900, so the theme follows the base color.
        Global<Color>(snapshot, "ColorPrimary").ShouldBe(Expect("primary"));
        Global<Color>(snapshot, "ColorTextLightSolid").ShouldBe(Expect("primaryforeground"));
        // Alias: the focus outline is the ring variable at Nova's 50% focus alpha, in both appearances.
        var ring = Expect("ring");
        Global<Color>(snapshot, "ColorControlOutline").ShouldBe(Color.FromArgb(0x80, ring.R, ring.G, ring.B));
    }

    [AvaloniaFact]
    public async Task Nova_Geometry()
    {
        var snapshot = await Apply("ShadcnNova", null);

        Global<double>(snapshot, "ControlHeight").ShouldBe(32);
        Global<double>(snapshot, "ControlHeightSM").ShouldBe(28);
        Global<double>(snapshot, "ControlHeightLG").ShouldBe(36);
        Global<double>(snapshot, "ControlHeightXS").ShouldBe(24);
        Global<CornerRadius>(snapshot, "BorderRadius").ShouldBe(new CornerRadius(10));
        Global<double>(snapshot, "FontSize").ShouldBe(14);
        Global<double>(snapshot, "FontSizeSM").ShouldBe(12.8);
        Global<bool>(snapshot, "EnableWaveSpirit").ShouldBeFalse();
    }

    [AvaloniaTheory]
    [InlineData("neutral")]
    [InlineData("stone")]
    [InlineData("zinc")]
    [InlineData("mauve")]
    [InlineData("olive")]
    [InlineData("mist")]
    [InlineData("taupe")]
    public async Task Dark_Neutral_Tokens_Are_Light_On_Dark(string family)
    {
        var seed = ShadcnAppendix.NeutralSeeds().Single(item => item.Family == family).Seed;
        var config = new ThemeConfigBuilder().WithToken("ColorNeutral", Hex(seed)).Build();
        var snapshot = await Apply("ShadcnNova.Dark", config);

        var background = Global<Color>(snapshot, "ColorBgContainer");
        foreach (var name in new[]
                 {
                     "ColorText", "ColorTextHeading", "ColorTextSecondary", "ColorTextDescription",
                     "ColorIcon", "ColorIconHover", "ControlItemColorActive"
                 })
        {
            var token = Global<Color>(snapshot, name);
            ShadcnAppendix.Contrast(token, background).ShouldBeGreaterThanOrEqualTo(3, name);
            Luminance(token).ShouldBeGreaterThan(Luminance(background), name);
        }
    }

    [AvaloniaFact]
    public async Task Neutral_Seed_Survives_Appended_Compact_And_Dark()
    {
        var config = new ThemeConfigBuilder()
                     .WithAlgorithms(new ThemeAlgorithm("Shadcn", "Nova"), ThemeAlgorithm.Compact, ThemeAlgorithm.Dark)
                     .WithToken("ColorNeutral", "#79716B")
                     .Build();
        var snapshot = await Apply("ShadcnNova", config);

        // Stone's dark ring variable is its 500 stop (#79716B, appendix A.2) at Nova's 50% focus alpha: the Alias
        // stage follows the resolved dark appearance of the chain, and the neutral seed reached it through the
        // appended built-in algorithms.
        Global<Color>(snapshot, "ColorControlOutline").ShouldBe(Color.FromArgb(0x80, 0x79, 0x71, 0x6B));
        Global<double>(snapshot, "ControlHeight").ShouldBe(28);
        Global<Color>(snapshot, "ColorNeutral").ShouldBe(Color.Parse("#79716B"));
    }

    private static async Task<ThemeSnapshot> Apply(string id, ThemeConfig? config)
    {
        var manager = (ThemeManager)Application.Current!.GetThemeManager()!;
        var result = await manager.ApplyThemeAsync(
            new ThemeRequest(id, config, ThemeTransitionReason.UserRequest));
        result.Status.ShouldBe(ThemeTransitionStatus.Committed);
        return manager.CurrentSnapshot!;
    }

    private static T Global<T>(ThemeSnapshot snapshot, string name)
    {
        snapshot.Registry.TryGetGlobalToken(name, out var token).ShouldBeTrue(name);
        return snapshot.GlobalTokenValues.Get<T>(token!.Slot);
    }

    private static string Hex(Color color) => $"#{color.R:X2}{color.G:X2}{color.B:X2}";

    private static double Luminance(Color color)
    {
        double Channel(byte value)
        {
            var linear = value / 255d;
            return linear <= 0.03928 ? linear / 12.92 : Math.Pow((linear + 0.055) / 1.055, 2.4);
        }

        return 0.2126 * Channel(color.R) + 0.7152 * Channel(color.G) + 0.0722 * Channel(color.B);
    }
}
