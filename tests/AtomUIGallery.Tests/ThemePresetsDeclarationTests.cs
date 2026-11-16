using System.Xml.Linq;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Configuration;
using AtomUIGallery.ShowCases.ThemePresets;
using Shouldly;
using Xunit;

namespace AtomUIGallery.Tests;

/// <summary>
/// The Theme Presets page agrees with the theme selection rules: a definition without a design language falls back to
/// the <see cref="ThemeSelectionResolver"/> chain, and the exported XML declares only what the export really is.
/// </summary>
public class ThemePresetsDeclarationTests
{
    public static TheoryData<ThemeAlgorithm[], bool, bool> Chains() => new()
    {
        { [ThemeAlgorithm.Default], false, false },
        { [ThemeAlgorithm.Default], true, false },
        { [ThemeAlgorithm.Default], false, true },
        { [ThemeAlgorithm.Default], true, true },
        { [new ThemeAlgorithm("Vendor", "Base"), ThemeAlgorithm.Compact], true, false },
        { [new ThemeAlgorithm("Vendor", "Base"), ThemeAlgorithm.Dark, ThemeAlgorithm.Compact], false, true }
    };

    [Theory]
    [MemberData(nameof(Chains))]
    public void Fallback_Chain_Of_A_Definition_Without_A_Language_Follows_The_Resolver(
        ThemeAlgorithm[] baseChain, bool dark, bool compact)
    {
        // The same base definition declared in a language: the resolver's fallback chain is the reference.
        var catalog = new[]
        {
            new ThemeInfo("Base", "Base", ThemeAppearance.Light, false)
            {
                DesignLanguage = "Reference", Density = ThemeDensity.Normal, Algorithms = baseChain
            }
        };
        var expected = ThemeSelectionResolver.Resolve(catalog,
            new ThemeSelection("Reference", dark ? ThemeAppearance.Dark : ThemeAppearance.Light,
                compact ? ThemeDensity.Compact : ThemeDensity.Normal, null));

        var chain = ThemePresetsViewModel.FallbackChain(baseChain, dark, compact);

        if (expected.Algorithms is null)
        {
            // The light, normal selection is the base definition itself; its own chain without built-ins appended.
            chain.ShouldBe(baseChain.Where(algorithm => algorithm != ThemeAlgorithm.Dark && algorithm != ThemeAlgorithm.Compact));
        }
        else
        {
            chain.ShouldBe(expected.Algorithms);
        }
    }

    [Fact]
    public void Export_That_Must_Fall_Back_To_Fixed_Drops_The_Seed_Design_Language()
    {
        var config = new ThemeConfigBuilder().WithAlgorithms(ThemeAlgorithm.Default)
            .WithToken("ColorPrimary", "#1677FF").WithToken("ColorPrimaryHover", "#4096FF").Build();

        var xml = ThemePresetCatalog.Export("Pinned", "Pinned", config, designLanguage: "AntDesign",
            brandColor: ThemeBrandColorSupport.Seed);

        var document = XDocument.Parse(xml);
        var root     = document.Root!;
        root.Attribute("BrandColor")!.Value.ShouldBe("Fixed");
        root.Attribute("DesignLanguage").ShouldBeNull();
        root.Nodes().OfType<XComment>().ShouldContain(comment => comment.Value.Contains("DesignLanguage=\"AntDesign\" omitted"));
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(xml));
        AtomUI.Theme.Definitions.ThemeDocumentReader.Read(stream, "pinned.theme.xml").Diagnostics.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(ThemeBrandColorSupport.Seed)]
    [InlineData(ThemeBrandColorSupport.Fixed)]
    public void Export_That_Matches_Its_Language_Keeps_It(ThemeBrandColorSupport brandColor)
    {
        // A brand seed is what a Seed language accepts; a Fixed language exports its own pins unchanged.
        var config = new ThemeConfigBuilder().WithAlgorithms(ThemeAlgorithm.Default)
            .WithToken("ColorPrimary", "#52C41A").Build();

        var root = XDocument.Parse(ThemePresetCatalog.Export("Kept", "Kept", config, designLanguage: "Lang",
            brandColor: brandColor)).Root!;

        root.Attribute("DesignLanguage")!.Value.ShouldBe("Lang");
        root.Attribute("BrandColor")!.Value.ShouldBe(brandColor.ToString());
        root.Nodes().OfType<XComment>().ShouldBeEmpty();
    }
}
