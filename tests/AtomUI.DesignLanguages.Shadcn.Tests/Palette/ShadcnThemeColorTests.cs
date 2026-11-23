using AtomUI.DesignLanguages.Shadcn.Derivation;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.DesignLanguages.Shadcn.Tests.Palette;

/// <summary>
/// <see cref="ShadcnThemeColor"/> resolves the seed into the official theme primaries (appendix B), the neutral
/// follow-the-base-color case, and readable derived primaries for custom seeds.
/// </summary>
public class ShadcnThemeColorTests
{
    public static TheoryData<string, bool> OfficialThemes()
    {
        var data = new TheoryData<string, bool>();
        foreach (var (theme, dark) in ShadcnAppendix.Themes().Keys)
        {
            data.Add(theme, dark);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(OfficialThemes))]
    public void Official_Themes_Match_Appendix(string theme, bool dark)
    {
        var themes = ShadcnAppendix.Themes();
        var expected = themes[(theme, dark)];
        // The seed a gallery swatch writes is the light primary (700; lime and yellow 400).
        var seed = themes[(theme, false)].Primary;

        var resolved = ShadcnThemeColor.Resolve(seed, ShadcnBaseColors.Resolve(null, dark), dark);

        resolved.Theme.ShouldBe(theme.ToLowerInvariant());
        resolved.Primary.ShouldBe(expected.Primary);
        resolved.Foreground.ShouldBe(expected.Foreground);
    }

    [Fact]
    public void Neutral_Seed_Follows_Base_Color()
    {
        foreach (var dark in new[] { false, true })
        {
            var zincSurface = ShadcnBaseColors.Resolve(ShadcnTailwindPalette.Stop("zinc", 500), dark);

            var resolved = ShadcnThemeColor.Resolve(ShadcnTailwindPalette.Stop("zinc", 900), zincSurface, dark);

            resolved.Theme.ShouldBe("neutral");
            resolved.Primary.ShouldBe(zincSurface.Primary);
            resolved.Foreground.ShouldBe(zincSurface.PrimaryForeground);
        }
    }

    [Theory]
    [InlineData("#3B82F6")]
    [InlineData("#F59E0B")]
    [InlineData("#10B981")]
    public void CustomPrimary_Dark_Foreground_Is_Readable(string hex)
    {
        var seed = Color.Parse(hex);
        foreach (var dark in new[] { false, true })
        {
            var resolved = ShadcnThemeColor.Resolve(seed, ShadcnBaseColors.Resolve(null, dark), dark);

            resolved.Theme.ShouldBeNull();
            ShadcnAppendix.Contrast(resolved.Foreground, resolved.Primary).ShouldBeGreaterThanOrEqualTo(4.5);
        }
    }
}
