using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

public class ThemeSelectionResolverTests
{
    private static ThemeInfo Info(
        string id,
        string? language,
        ThemeAppearance appearance,
        ThemeDensity density,
        ThemeBrandColorSupport brand,
        params ThemeAlgorithm[] algorithms)
    {
        return new ThemeInfo(id, id, appearance, false)
        {
            DesignLanguage = language,
            Density        = density,
            BrandColor     = brand,
            Algorithms     = algorithms
        };
    }

    private static List<ThemeInfo> CreateCatalog()
    {
        return
        [
            Info("DaybreakBlue", "AntDesign", ThemeAppearance.Light, ThemeDensity.Normal, ThemeBrandColorSupport.Seed,
                 ThemeAlgorithm.Default),
            Info("Loose", null, ThemeAppearance.Light, ThemeDensity.Normal, ThemeBrandColorSupport.Fixed),
            Info("PrimeNGAura", "PrimeNG.Aura", ThemeAppearance.Light, ThemeDensity.Normal, ThemeBrandColorSupport.Seed,
                 new ThemeAlgorithm("PrimeNG", "Aura")),
            Info("PrimeNGAura.Dark", "PrimeNG.Aura", ThemeAppearance.Dark, ThemeDensity.Normal, ThemeBrandColorSupport.Seed,
                 new ThemeAlgorithm("PrimeNG", "Aura.Dark")),
            Info("PrimeNGAura.Compact", "PrimeNG.Aura", ThemeAppearance.Light, ThemeDensity.Compact,
                 ThemeBrandColorSupport.Seed, new ThemeAlgorithm("PrimeNG", "Aura"), ThemeAlgorithm.Compact),
            Info("PrimeNGAura.DarkCompact", "PrimeNG.Aura", ThemeAppearance.Dark, ThemeDensity.Compact,
                 ThemeBrandColorSupport.Seed, new ThemeAlgorithm("PrimeNG", "Aura.Dark"), ThemeAlgorithm.Compact),
            Info("Bootstrap", "Bootstrap", ThemeAppearance.Light, ThemeDensity.Normal, ThemeBrandColorSupport.Fixed,
                 ThemeAlgorithm.Default)
        ];
    }

    [Fact]
    public void Exact_Variant_Uses_The_Profile_Chain()
    {
        var result = ThemeSelectionResolver.Resolve(
            CreateCatalog(),
            new ThemeSelection("PrimeNG.Aura", ThemeAppearance.Dark, ThemeDensity.Compact, null));

        result.ThemeId.ShouldBe("PrimeNGAura.DarkCompact");
        result.Algorithms.ShouldBeNull();
    }

    [Fact]
    public void Missing_Variant_Falls_Back_To_Base_Chain_Plus_BuiltIns()
    {
        var result = ThemeSelectionResolver.Resolve(
            CreateCatalog(),
            new ThemeSelection("AntDesign", ThemeAppearance.Dark, ThemeDensity.Compact, null));

        result.ThemeId.ShouldBe("DaybreakBlue");
        result.Algorithms.ShouldNotBeNull();
        result.Algorithms.ShouldBe([ThemeAlgorithm.Default, ThemeAlgorithm.Compact, ThemeAlgorithm.Dark]);
    }

    [Fact]
    public void Fallback_Removes_BuiltIn_Dark_And_Compact_From_The_Base_Chain()
    {
        var catalog = new List<ThemeInfo>
        {
            Info("Base", "Lang", ThemeAppearance.Light, ThemeDensity.Normal, ThemeBrandColorSupport.Fixed,
                 ThemeAlgorithm.Default, ThemeAlgorithm.Dark, ThemeAlgorithm.Compact)
        };

        var result = ThemeSelectionResolver.Resolve(
            catalog, new ThemeSelection("Lang", ThemeAppearance.Dark, ThemeDensity.Compact, null));

        result.ThemeId.ShouldBe("Base");
        result.Algorithms.ShouldBe([ThemeAlgorithm.Default, ThemeAlgorithm.Compact, ThemeAlgorithm.Dark]);
    }

    [Fact]
    public void Seed_Language_Returns_The_Brand_Seed()
    {
        var seed = Color.Parse("#52C41A");
        var result = ThemeSelectionResolver.Resolve(
            CreateCatalog(),
            new ThemeSelection("AntDesign", ThemeAppearance.Light, ThemeDensity.Normal, seed));

        result.BrandSeed.ShouldBe(seed);
        result.BrandColorIgnored.ShouldBeFalse();
    }

    [Fact]
    public void Fixed_Language_Ignores_The_Brand()
    {
        var result = ThemeSelectionResolver.Resolve(
            CreateCatalog(),
            new ThemeSelection("Bootstrap", ThemeAppearance.Light, ThemeDensity.Normal, Color.Parse("#52C41A")));

        result.BrandSeed.ShouldBeNull();
        result.BrandColorIgnored.ShouldBeTrue();
    }

    [Fact]
    public void Null_Brand_Is_Not_Ignored()
    {
        var result = ThemeSelectionResolver.Resolve(
            CreateCatalog(),
            new ThemeSelection("Bootstrap", ThemeAppearance.Light, ThemeDensity.Normal, null));

        result.BrandSeed.ShouldBeNull();
        result.BrandColorIgnored.ShouldBeFalse();
    }

    [Fact]
    public void Unknown_Language_Throws()
    {
        Should.Throw<ArgumentException>(() => ThemeSelectionResolver.Resolve(
                                            CreateCatalog(),
                                            new ThemeSelection("Nope", ThemeAppearance.Light, ThemeDensity.Normal, null)));
    }

    [Fact]
    public void Dark_Only_Language_Requested_In_Light_Falls_Back_To_Its_Definition()
    {
        var seed = Color.Parse("#52C41A");
        var catalog = new List<ThemeInfo>
        {
            Info("MyNight", "MyBrand", ThemeAppearance.Dark, ThemeDensity.Normal, ThemeBrandColorSupport.Seed,
                 ThemeAlgorithm.Default, ThemeAlgorithm.Dark)
        };

        var light = ThemeSelectionResolver.Resolve(
            catalog, new ThemeSelection("MyBrand", ThemeAppearance.Light, ThemeDensity.Normal, seed));
        var lightCompact = ThemeSelectionResolver.Resolve(
            catalog, new ThemeSelection("MyBrand", ThemeAppearance.Light, ThemeDensity.Compact, null));

        light.ThemeId.ShouldBe("MyNight");
        light.Algorithms.ShouldBe([ThemeAlgorithm.Default]);
        light.BrandSeed.ShouldBe(seed);
        lightCompact.ThemeId.ShouldBe("MyNight");
        lightCompact.Algorithms.ShouldBe([ThemeAlgorithm.Default, ThemeAlgorithm.Compact]);
    }

    [Fact]
    public void Compact_Only_Language_Requested_Normal_Falls_Back_To_The_Same_Appearance()
    {
        var catalog = new List<ThemeInfo>
        {
            Info("Tight.Dark", "Tight", ThemeAppearance.Dark, ThemeDensity.Compact, ThemeBrandColorSupport.Fixed,
                 ThemeAlgorithm.Default, ThemeAlgorithm.Compact, ThemeAlgorithm.Dark),
            Info("Tight", "Tight", ThemeAppearance.Light, ThemeDensity.Compact, ThemeBrandColorSupport.Fixed,
                 ThemeAlgorithm.Default, ThemeAlgorithm.Compact)
        };

        var light = ThemeSelectionResolver.Resolve(
            catalog, new ThemeSelection("Tight", ThemeAppearance.Light, ThemeDensity.Normal, null));
        var dark = ThemeSelectionResolver.Resolve(
            catalog, new ThemeSelection("Tight", ThemeAppearance.Dark, ThemeDensity.Normal, null));

        light.ThemeId.ShouldBe("Tight");
        light.Algorithms.ShouldBe([ThemeAlgorithm.Default]);
        dark.ThemeId.ShouldBe("Tight.Dark");
        dark.Algorithms.ShouldBe([ThemeAlgorithm.Default, ThemeAlgorithm.Dark]);
    }

    [Fact]
    public void Fallback_Prefers_Same_Density_Then_Catalog_Order_When_No_Appearance_Matches()
    {
        var catalog = new List<ThemeInfo>
        {
            Info("Night.Compact", "Night", ThemeAppearance.Dark, ThemeDensity.Compact, ThemeBrandColorSupport.Fixed,
                 ThemeAlgorithm.Default, ThemeAlgorithm.Compact, ThemeAlgorithm.Dark),
            Info("Night", "Night", ThemeAppearance.Dark, ThemeDensity.Normal, ThemeBrandColorSupport.Fixed,
                 ThemeAlgorithm.Default, ThemeAlgorithm.Dark)
        };

        ThemeSelectionResolver.Resolve(catalog, new ThemeSelection("Night", ThemeAppearance.Light, ThemeDensity.Normal, null))
                              .ThemeId.ShouldBe("Night");
        ThemeSelectionResolver.Resolve(catalog, new ThemeSelection("Night", ThemeAppearance.Light, ThemeDensity.Compact, null))
                              .ThemeId.ShouldBe("Night.Compact");

        var single = new List<ThemeInfo> { catalog[0] };
        ThemeSelectionResolver.Resolve(single, new ThemeSelection("Night", ThemeAppearance.Light, ThemeDensity.Normal, null))
                              .ThemeId.ShouldBe("Night.Compact");
    }

    [Fact]
    public void DesignLanguages_Skips_Ungrouped_Themes()
    {
        ThemeSelectionResolver.DesignLanguages(CreateCatalog())
                              .ShouldBe(["AntDesign", "PrimeNG.Aura", "Bootstrap"]);
    }

    [Fact]
    public void BrandColorSupport_Reads_The_Language_Declaration()
    {
        var catalog = CreateCatalog();

        ThemeSelectionResolver.BrandColorSupport(catalog, "AntDesign").ShouldBe(ThemeBrandColorSupport.Seed);
        ThemeSelectionResolver.BrandColorSupport(catalog, "Bootstrap").ShouldBe(ThemeBrandColorSupport.Fixed);
        Should.Throw<ArgumentException>(() => ThemeSelectionResolver.BrandColorSupport(catalog, "Nope"));
    }
}
