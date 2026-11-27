using AtomUI.DesignLanguages.Shadcn.TokenEvaluation;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.DesignLanguages.Shadcn.Tests.Palette;

/// <summary>
/// <see cref="ShadcnBaseColors" /> resolves every base color from its seed
/// (the family's 500 stop) by the shadcn step rule, matching the official appendix exactly.
/// </summary>
public class ShadcnBaseColorTests
{
    private static readonly (string SurfaceField, string Variable)[] s_fields =
    [
        (nameof(ShadcnSurface.Background), "background"),
        (nameof(ShadcnSurface.Foreground), "foreground"),
        (nameof(ShadcnSurface.Card), "card"),
        (nameof(ShadcnSurface.Popover), "popover"),
        (nameof(ShadcnSurface.Primary), "primary"),
        (nameof(ShadcnSurface.PrimaryForeground), "primaryforeground"),
        (nameof(ShadcnSurface.Secondary), "secondary"),
        (nameof(ShadcnSurface.SecondaryForeground), "secondaryforeground"),
        (nameof(ShadcnSurface.Muted), "muted"),
        (nameof(ShadcnSurface.MutedForeground), "mutedforeground"),
        (nameof(ShadcnSurface.Accent), "accent"),
        (nameof(ShadcnSurface.AccentForeground), "accentforeground"),
        (nameof(ShadcnSurface.Destructive), "destructive"),
        (nameof(ShadcnSurface.Border), "border"),
        (nameof(ShadcnSurface.Input), "input"),
        (nameof(ShadcnSurface.Ring), "ring")
    ];

    [Theory]
    [InlineData("neutral")]
    [InlineData("stone")]
    [InlineData("zinc")]
    [InlineData("mauve")]
    [InlineData("olive")]
    [InlineData("mist")]
    [InlineData("taupe")]
    public void Official_Seed_Generates_Appendix_Values(string family)
    {
        var appendix = ShadcnAppendix.BaseColors();
        var seed = ShadcnAppendix.NeutralSeeds().Single(item => item.Family == family).Seed;

        foreach (var dark in new[] { false, true })
        {
            var surface = ShadcnBaseColors.Resolve(seed, dark);

            foreach (var (field, variable) in s_fields)
            {
                var expected = appendix[(family, dark, variable)];
                var actual = (Color)typeof(ShadcnSurface).GetProperty(field)!.GetValue(surface)!;
                actual.ShouldBe(expected, $"{family} {(dark ? "dark" : "light")} --{variable}");
            }
        }
    }

    [Fact]
    public void Palette_Five_Hundred_Stops_Are_The_Seeds()
    {
        foreach (var (family, seed) in ShadcnAppendix.NeutralSeeds())
        {
            ShadcnTailwindPalette.Stop(family, 500).ShouldBe(seed, family);
        }
    }

    [Fact]
    public void Custom_Seed_Is_Not_Mistaken_For_An_Official_Family()
    {
        foreach (var (family, seed) in ShadcnAppendix.NeutralSeeds())
        {
            ShadcnBaseColors.FamilyOf(seed).ShouldBe(family);
        }

        // zinc-500-like but not equal: a custom gray must generate its own family, not borrow zinc's.
        ShadcnBaseColors.FamilyOf(Color.Parse("#6B7280")).ShouldBeNull();
    }

    [Fact]
    public void Custom_Seed_Dark_Surface_Has_Light_Text()
    {
        var surface = ShadcnBaseColors.Resolve(Color.Parse("#6B7280"), true);

        ShadcnAppendix.Contrast(surface.Foreground, surface.Background).ShouldBeGreaterThanOrEqualTo(7);
    }
}
