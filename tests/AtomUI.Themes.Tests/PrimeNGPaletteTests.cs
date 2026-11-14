using AtomUI.Themes.PrimeNG.Algorithms;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Themes.Tests;

/// <summary>
/// The PrimeNG colour scale: the PrimeUIX <c>palette</c> formula (verified against <c>@primeuix/styled</c> 1.2.0)
/// and the Tailwind tables transcribed from the <c>@primeuix/themes</c> primitive tokens.
/// </summary>
public class PrimeNGPaletteTests
{
    private static readonly Color Emerald500 = Color.Parse("#10B981");

    [Fact]
    public void Generate_Keeps_The_Seed_At_500()
    {
        PrimeNGPalette.Generate(Emerald500)[5].ShouldBe(Emerald500);
    }

    [Fact]
    public void Generate_Tints_Toward_White_Below_500()
    {
        PrimeNGPalette.Generate(Emerald500)[0].ShouldBe(Color.Parse("#F3FCF9"));
    }

    [Fact]
    public void Generate_Shades_Toward_Black_Above_500()
    {
        PrimeNGPalette.Generate(Emerald500)[10].ShouldBe(Color.Parse("#042E20"));
    }

    [Fact]
    public void Mix_Rounds_Half_Up()
    {
        // Green: 255 * 0.95 + 185 * 0.05 = 251.5 -> 252. Red: 255 * 0.95 + 16 * 0.05 = 243.05 -> 243.
        var mixed = PrimeNGPalette.Mix(Colors.White, Emerald500, 95);
        mixed.G.ShouldBe((byte)252);
        mixed.R.ShouldBe((byte)243);
        // 251.5 rounds to 252 under banker's rounding as well; 1 * 0.5 = 0.5 tells the two apart:
        // half up gives 1, banker's rounding gives 0.
        PrimeNGPalette.Mix(Color.FromRgb(1, 1, 1), Colors.Black, 50).R.ShouldBe((byte)1);
    }

    [Fact]
    public void Resolve_Returns_The_Tailwind_Table_For_A_Known_500()
    {
        var palette = PrimeNGPalette.Resolve(Emerald500);
        palette.Length.ShouldBe(PrimeNGPalette.Stops.Length);
        palette[0].ShouldBe(Color.Parse("#ECFDF5"));
        palette[2].ShouldBe(Color.Parse("#A7F3D0"));
        palette[3].ShouldBe(Color.Parse("#6EE7B7"));
        palette[4].ShouldBe(Color.Parse("#34D399"));
        palette[5].ShouldBe(Emerald500);
        palette[6].ShouldBe(Color.Parse("#059669"));
        palette[7].ShouldBe(Color.Parse("#047857"));
    }

    [Fact]
    public void Resolve_Generates_For_An_Unknown_Seed()
    {
        var seed = Color.Parse("#123456");
        PrimeNGPalette.Resolve(seed).ShouldBe(PrimeNGPalette.Generate(seed));
    }

    [Theory]
    [InlineData("#FFFFFF")]
    [InlineData("#000000")]
    [InlineData("#8010B981")]
    public void Resolve_Does_Not_Throw_For_Extreme_Or_Translucent_Seeds(string seed)
    {
        var color = Color.Parse(seed);
        var palette = PrimeNGPalette.Resolve(color);
        palette.Length.ShouldBe(11);
        palette[5].ShouldBe(color);
        palette.ShouldAllBe(stop => stop.A == color.A);
    }
}
