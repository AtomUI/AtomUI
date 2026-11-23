using AtomUI.DesignLanguages.PrimeNG.Derivation;
using AtomUI.DesignLanguages.PrimeNG.Presets;
using AtomUI.Theme;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using AtomUI.Theme.Schema;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.DesignLanguages.PrimeNG.Tests;

/// <summary>
/// The PrimeNG map derivation against the colours the preset tables fix (spec 4.7 of the previous PrimeNG package: the
/// values the pre-algorithm Aura profiles wrote), the primary seed and primeng.dev's noir primary.
/// </summary>
public class PrimeNGMapDerivationTests
{
    [Fact]
    public void Aura_Map_Matches_The_Spec_Colours()
    {
        var light = Map(AuraSeed, PrimeNGLanguage.Aura, ThemeAppearance.Light);
        AssertColors(light, new Dictionary<string, string>
        {
            [nameof(DesignToken.ColorPrimary)]         = "#10B981",
            [nameof(DesignToken.ColorPrimaryHover)]    = "#059669",
            [nameof(DesignToken.ColorPrimaryActive)]   = "#047857",
            [nameof(DesignToken.ColorPrimaryBg)]       = "#ECFDF5",
            [nameof(DesignToken.ColorPrimaryText)]     = "#047857",
            [nameof(DesignToken.ColorText)]            = "#334155",
            [nameof(DesignToken.ColorTextSecondary)]   = "#64748B",
            [nameof(DesignToken.ColorBgContainer)]     = "#FFFFFF",
            [nameof(DesignToken.ColorFillTertiary)]    = "#F1F5F9",
            [nameof(DesignToken.ColorBorderSecondary)] = "#E2E8F0",
            [nameof(DesignToken.ColorBorder)]          = "#CBD5E1",
            [nameof(DesignToken.ColorError)]           = "#F87171",
            [nameof(DesignToken.ColorBgMask)]          = "#66000000"
        });

        var dark = Map(AuraSeed, PrimeNGLanguage.Aura, ThemeAppearance.Dark);
        AssertColors(dark, new Dictionary<string, string>
        {
            [nameof(DesignToken.ColorPrimary)]         = "#34D399",
            [nameof(DesignToken.ColorPrimaryHover)]    = "#6EE7B7",
            [nameof(DesignToken.ColorPrimaryActive)]   = "#A7F3D0",
            [nameof(DesignToken.ColorPrimaryBg)]       = "#2934D399",
            [nameof(DesignToken.ColorPrimaryText)]     = "#DEFFFFFF",
            [nameof(DesignToken.ColorText)]            = "#FFFFFF",
            [nameof(DesignToken.ColorTextSecondary)]   = "#A1A1AA",
            [nameof(DesignToken.ColorBgContainer)]     = "#09090B",
            [nameof(DesignToken.ColorBgElevated)]      = "#18181B",
            [nameof(DesignToken.ColorFillTertiary)]    = "#27272A",
            [nameof(DesignToken.ColorBorderSecondary)] = "#3F3F46",
            [nameof(DesignToken.ColorBorder)]          = "#52525B",
            [nameof(DesignToken.ColorError)]           = "#FCA5A5",
            [nameof(DesignToken.ColorBgMask)]          = "#99000000"
        });
        dark.ColorBgBase.ShouldBe(Color.Parse("#09090B"));
        dark.ColorTextBase.ShouldBe(Colors.White);
    }

    [Fact]
    public void Changing_The_Primary_Seed_Moves_Hover_Active_And_Bg()
    {
        var map = Map(seed => seed.ColorPrimary = Color.Parse("#3B82F6"), PrimeNGLanguage.Aura, ThemeAppearance.Light);
        map.ColorPrimary.ShouldBe(Color.Parse("#3B82F6"));
        map.ColorPrimaryHover.ShouldBe(Color.Parse("#2563EB"));
        map.ColorPrimaryActive.ShouldBe(Color.Parse("#1D4ED8"));
        map.ColorPrimaryBg.ShouldBe(Color.Parse("#EFF6FF"));
    }

    // primeng.dev's default primary "noir" (primeng 22.1.5 configurator): the primary scale is the surface scale,
    // light primary 950 / 800 / 700 with white contrast text, dark primary 50 / 200 / 300 with surface-950 text.
    [Theory]
    [InlineData("Aura", false, "#020617", "#1E293B", "#334155", "#FFFFFF")]
    [InlineData("Lara", false, "#020617", "#1E293B", "#334155", "#FFFFFF")]
    [InlineData("Nora", false, "#020617", "#1E293B", "#334155", "#FFFFFF")]
    [InlineData("Aura", true, "#FAFAFA", "#E4E4E7", "#D4D4D8", "#09090B")]
    [InlineData("Lara", true, "#FAFAFA", "#E4E4E7", "#D4D4D8", "#09090B")]
    [InlineData("Nora", true, "#FAFAFA", "#E4E4E7", "#D4D4D8", "#09090B")]
    public void Noir_Seed_Takes_The_Surface_Scale(string style, bool dark, string primary, string hover, string active,
                                                  string foreground)
    {
        var map = Map(seed => seed.ColorPrimary = Color.Parse("#020617"), new DesignStyle(style),
            dark ? ThemeAppearance.Dark : ThemeAppearance.Light);
        AssertColors(map, new Dictionary<string, string>
        {
            [nameof(DesignToken.ColorPrimary)]        = primary,
            [nameof(DesignToken.ColorPrimaryHover)]   = hover,
            [nameof(DesignToken.ColorPrimaryActive)]  = active,
            [nameof(DesignToken.SelectionBackground)] = primary,
            [nameof(DesignToken.SelectionForeground)] = foreground,
            // The link seed defaults to the primary, so the links are noir too: the primary states.
            [nameof(DesignToken.ColorLink)]           = primary,
            [nameof(DesignToken.ColorLinkHover)]      = hover,
            [nameof(DesignToken.ColorLinkActive)]     = active
        });
    }

    [Theory]
    [InlineData("#030712")] // gray-950
    [InlineData("#09090B")] // zinc-950
    [InlineData("#0A0A0A")] // neutral-950
    [InlineData("#0C0A09")] // stone-950
    public void Any_Neutral_950_Seed_Is_Noir_On_The_Preset_Surface(string seed)
    {
        // The trigger is the seed, the scale is the appearance's surface (slate in light, zinc in dark), so one
        // remembered seed works in both appearances.
        var light = Map(token => token.ColorPrimary = Color.Parse(seed), PrimeNGLanguage.Aura, ThemeAppearance.Light);
        light.ColorPrimary.ShouldBe(Color.Parse("#020617"));
        light.ColorPrimaryHover.ShouldBe(Color.Parse("#1E293B"));
        var dark = Map(token => token.ColorPrimary = Color.Parse(seed), PrimeNGLanguage.Aura, ThemeAppearance.Dark);
        dark.ColorPrimary.ShouldBe(Color.Parse("#FAFAFA"));
        dark.ColorPrimaryHover.ShouldBe(Color.Parse("#E4E4E7"));
    }

    [Fact]
    public void A_Seed_Next_To_Noir_Is_A_Colour()
    {
        // One step off slate-950 is an ordinary seed: the palette formula keeps it as the light 500 stop.
        var map = Map(token => token.ColorPrimary = Color.Parse("#020618"), PrimeNGLanguage.Aura, ThemeAppearance.Light);
        map.ColorPrimary.ShouldBe(Color.Parse("#020618"));
        map.ColorPrimaryHover.ShouldNotBe(Color.Parse("#1E293B"));
    }

    [Fact]
    public void An_Explicit_Link_Seed_Keeps_Its_Own_Scale_Under_Noir()
    {
        var map = Map(token =>
        {
            token.ColorPrimary = Color.Parse("#020617");
            token.ColorLink    = Color.Parse("#3B82F6");
        }, PrimeNGLanguage.Aura, ThemeAppearance.Light);
        map.ColorPrimary.ShouldBe(Color.Parse("#020617"));
        // The Aura light link row: blue 700 / 800 / 900.
        map.ColorLink.ShouldBe(Color.Parse("#1D4ED8"));
        map.ColorLinkHover.ShouldBe(Color.Parse("#1E40AF"));
        map.ColorLinkActive.ShouldBe(Color.Parse("#1E3A8A"));
    }

    [Fact]
    public void The_Language_Tokens_Are_The_Surface_Scale_Of_The_Appearance()
    {
        var language = Language(PrimeNGLanguage.Nora, ThemeAppearance.Dark);
        language.Surface50.ShouldBe(PrimeNGTailwindPalettes.Zinc[0]);
        language.Surface950.ShouldBe(PrimeNGTailwindPalettes.Zinc[10]);
        Language(PrimeNGLanguage.Lara, ThemeAppearance.Light).Surface500.ShouldBe(PrimeNGTailwindPalettes.Slate[5]);
    }

    // The previous Aura profiles pinned the primary seed to emerald-500 and the error seed to red-500.
    private static void AuraSeed(DesignToken seed)
    {
        seed.ColorPrimary = Color.Parse("#10B981");
        seed.ColorError   = Color.Parse("#EF4444");
    }

    private static PrimeNGToken Language(DesignStyle style, ThemeAppearance appearance)
    {
        var variant  = new DesignVariant(style, appearance, DesignDensity.Default, DesignIdiom.Desktop);
        var language = new PrimeNGToken();
        new PrimeNGLanguage().DeriveLanguage(in variant, new DesignToken(), language);
        return language;
    }

    private static DesignToken Map(Action<DesignToken> prepareSeed, DesignStyle style, ThemeAppearance appearance)
    {
        var variant = new DesignVariant(style, appearance, DesignDensity.Default, DesignIdiom.Desktop);
        var token   = new DesignToken();
        prepareSeed(token);
        PrimeNGMapDerivation.Derive(in variant, PrimeNGPresets.Of(style), Language(style, appearance), token);
        return token;
    }

    private static void AssertColors(DesignToken map, IReadOnlyDictionary<string, string> expected)
    {
        foreach (var (name, value) in expected)
        {
            GlobalTokenSchema.Get(name).GetValue(map).ShouldBe(Color.Parse(value), name);
        }
    }
}
