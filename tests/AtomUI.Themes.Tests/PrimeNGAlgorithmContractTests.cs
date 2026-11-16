using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.ContractTests;
using AtomUI.Theme.DesignTokens;
using AtomUI.Themes.PrimeNG;
using AtomUI.Themes.PrimeNG.Algorithms;
using Avalonia;
using Avalonia.Media;
using Shouldly;
using Xunit;
using CoreSchema = AtomUI.Generated.AtomUICore.GeneratedThemeSchema;

namespace AtomUI.Themes.Tests;

/// <summary>
/// The PrimeNG preset algorithms against the reusable algorithm contract (spec 4.6), and the Aura Map stage against
/// the values spec 4.7 fixes for it (the "现值" rows: the values the pre-algorithm Aura XML wrote).
/// </summary>
public class PrimeNGAlgorithmContractTests
{
    private static readonly ThemeAlgorithm Aura = new("PrimeNG", "Aura");
    private static readonly ThemeAlgorithm AuraDark = new("PrimeNG", "Aura.Dark");

    [Theory]
    [InlineData("Aura")]
    [InlineData("Aura.Dark")]
    [InlineData("Lara")]
    [InlineData("Lara.Dark")]
    [InlineData("Nora")]
    [InlineData("Nora.Dark")]
    public void Preset_Algorithm_Satisfies_The_Contract(string id)
    {
        // A PrimeNG preset algorithm starts its chain, so it is verified standalone: it must write every Map token.
        var violations = ThemeAlgorithmContract.Verify(
            ThemeAlgorithmContractInput.Capture(builder => builder.UsePrimeNGThemes()),
            new ThemeAlgorithm("PrimeNG", id),
            standalone: true);

        violations.ShouldBeEmpty();
    }

    [Fact]
    public void Aura_Map_Matches_The_Spec_Colours()
    {
        var light = Map(AuraSeed, Aura);
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

        var dark = Map(AuraSeed, AuraDark);
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
        var map = Map(seed => seed.ColorPrimary = Color.Parse("#3B82F6"), Aura);
        map.ColorPrimary.ShouldBe(Color.Parse("#3B82F6"));
        map.ColorPrimaryHover.ShouldBe(Color.Parse("#2563EB"));
        map.ColorPrimaryActive.ShouldBe(Color.Parse("#1D4ED8"));
        map.ColorPrimaryBg.ShouldBe(Color.Parse("#EFF6FF"));
    }

    // primeng.dev's default primary "noir" (primeng 22.1.5 configurator): the primary scale is the surface scale,
    // light primary 950 / 800 / 700 with white contrast text, dark primary 50 / 200 / 300 with surface-950 text.
    [Theory]
    [InlineData("Aura", "#020617", "#1E293B", "#334155", "#FFFFFF")]
    [InlineData("Lara", "#020617", "#1E293B", "#334155", "#FFFFFF")]
    [InlineData("Nora", "#020617", "#1E293B", "#334155", "#FFFFFF")]
    [InlineData("Aura.Dark", "#FAFAFA", "#E4E4E7", "#D4D4D8", "#09090B")]
    [InlineData("Lara.Dark", "#FAFAFA", "#E4E4E7", "#D4D4D8", "#09090B")]
    [InlineData("Nora.Dark", "#FAFAFA", "#E4E4E7", "#D4D4D8", "#09090B")]
    public void Noir_Seed_Takes_The_Surface_Scale(string id, string primary, string hover, string active,
                                                  string foreground)
    {
        var map = Map(seed => seed.ColorPrimary = Color.Parse("#020617"), new ThemeAlgorithm("PrimeNG", id));
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
        var light = Map(token => token.ColorPrimary = Color.Parse(seed), Aura);
        light.ColorPrimary.ShouldBe(Color.Parse("#020617"));
        light.ColorPrimaryHover.ShouldBe(Color.Parse("#1E293B"));
        var dark = Map(token => token.ColorPrimary = Color.Parse(seed), AuraDark);
        dark.ColorPrimary.ShouldBe(Color.Parse("#FAFAFA"));
        dark.ColorPrimaryHover.ShouldBe(Color.Parse("#E4E4E7"));
    }

    [Fact]
    public void A_Seed_Next_To_Noir_Is_A_Colour()
    {
        // One step off slate-950 is an ordinary seed: the palette formula keeps it as the light 500 stop.
        var map = Map(token => token.ColorPrimary = Color.Parse("#020618"), Aura);
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
        }, Aura);
        map.ColorPrimary.ShouldBe(Color.Parse("#020617"));
        // The Aura light link row: blue 700 / 800 / 900.
        map.ColorLink.ShouldBe(Color.Parse("#1D4ED8"));
        map.ColorLinkHover.ShouldBe(Color.Parse("#1E40AF"));
        map.ColorLinkActive.ShouldBe(Color.Parse("#1E3A8A"));
    }

    [Fact]
    public void Dark_After_Another_Algorithm_Keeps_The_Previous_Sizes()
    {
        // A radius of 10 tells the Ant ladder (XS 2, SM 6) from the Aura ladder (XS 6, SM 8), and the compact
        // heights (21 / 35 from 28) from the Aura ones (24 / 36).
        Action<DesignToken> seed = token =>
        {
            AuraSeed(token);
            token.BorderRadius = new CornerRadius(10);
        };
        var previous = Map(seed, ThemeAlgorithm.Default, ThemeAlgorithm.Compact);
        var followed = Map(seed, ThemeAlgorithm.Default, ThemeAlgorithm.Compact, AuraDark);
        var standalone = Map(seed, AuraDark);

        followed.ControlHeight.ShouldBe(previous.ControlHeight);
        followed.ControlHeightSM.ShouldBe(previous.ControlHeightSM);
        followed.ControlHeightLG.ShouldBe(previous.ControlHeightLG);
        followed.ControlHeightXS.ShouldBe(previous.ControlHeightXS);
        followed.BorderRadiusXS.ShouldBe(previous.BorderRadiusXS);
        followed.BorderRadiusSM.ShouldBe(previous.BorderRadiusSM);
        followed.BorderRadiusLG.ShouldBe(previous.BorderRadiusLG);
        followed.FontSizeSM.ShouldBe(previous.FontSizeSM);
        followed.FontSizeLG.ShouldBe(previous.FontSizeLG);
        followed.SizeXS.ShouldBe(previous.SizeXS);

        // The colours are still recomputed, and standalone the dark algorithm writes its own ladder.
        followed.ColorPrimary.ShouldBe(Color.Parse("#34D399"));
        followed.ColorBgContainer.ShouldBe(Color.Parse("#09090B"));
        standalone.BorderRadiusXS.ShouldBe(new CornerRadius(6));
        standalone.BorderRadiusSM.ShouldBe(new CornerRadius(8));
        standalone.BorderRadiusLG.ShouldBe(new CornerRadius(12));
        standalone.ControlHeightSM.ShouldBe(standalone.ControlHeight - 7);
        standalone.ControlHeightLG.ShouldBe(standalone.ControlHeight + 7);
    }

    // The Aura profiles pin the primary seed to emerald-500 and the error seed to red-500.
    private static void AuraSeed(DesignToken seed)
    {
        seed.ColorPrimary = Color.Parse("#10B981");
        seed.ColorError   = Color.Parse("#EF4444");
    }

    private static DesignToken Map(Action<DesignToken> prepareSeed, params ThemeAlgorithm[] chain)
    {
        var input = ThemeAlgorithmContractInput.Capture(builder => builder.UsePrimeNGThemes());
        var descriptors = chain
                          .Select(algorithm => input.Algorithms.Single(descriptor => descriptor.Algorithm == algorithm))
                          .ToArray();
        var seed = new DesignToken();
        prepareSeed(seed);
        return ThemeCompiler.EvaluateAlgorithms(seed, descriptors, CoreSchema.GetGlobalTokens(), out _);
    }

    private static void AssertColors(DesignToken map, IReadOnlyDictionary<string, string> expected)
    {
        var descriptors = CoreSchema.GetGlobalTokens();
        foreach (var (name, value) in expected)
        {
            var descriptor = descriptors.Single(descriptor => descriptor.Name == name);
            descriptor.GetValue(map).ShouldBe(Color.Parse(value), name);
        }
    }
}
