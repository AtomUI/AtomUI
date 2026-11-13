using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Schema;
using AtomUI.Utils;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

public class ThemeAlgorithmIdentityTests
{
    [Fact]
    public void BuiltIns_Have_AtomUI_Catalog_And_Stable_Ids()
    {
        ThemeAlgorithm.Default.Catalog.ShouldBe("AtomUI");
        ThemeAlgorithm.Default.Id.ShouldBe("Default");
        ThemeAlgorithm.Dark.Catalog.ShouldBe("AtomUI");
        ThemeAlgorithm.Dark.Id.ShouldBe("Dark");
        ThemeAlgorithm.Dark.ToString().ShouldBe("AtomUI:Dark");
        ThemeAlgorithm.Compact.Catalog.ShouldBe("AtomUI");
        ThemeAlgorithm.Compact.Id.ShouldBe("Compact");
    }

    [Fact]
    public void Equality_Is_Ordinal_On_Catalog_And_Id()
    {
        var aura = new ThemeAlgorithm("PrimeNG", "Aura");

        (aura == new ThemeAlgorithm("PrimeNG", "Aura")).ShouldBeTrue();
        aura.Equals(new ThemeAlgorithm("PrimeNG", "Aura")).ShouldBeTrue();
        aura.GetHashCode().ShouldBe(new ThemeAlgorithm("PrimeNG", "Aura").GetHashCode());
        (aura != new ThemeAlgorithm("PrimeNG", "aura")).ShouldBeTrue();
        (aura != new ThemeAlgorithm("primeNG", "Aura")).ShouldBeTrue();
    }

    [Theory]
    [InlineData("", "A")]
    [InlineData("A", "")]
    [InlineData("1A", "B")]
    [InlineData("A", "B C")]
    public void Constructor_Rejects_Invalid_Identifiers(string catalog, string id)
    {
        Should.Throw<ArgumentException>(() => new ThemeAlgorithm(catalog, id));
    }

    [Fact]
    public void Default_Struct_Is_Rejected_By_Descriptor_And_Normalizer()
    {
        Should.Throw<ArgumentException>(() => new ThemeAlgorithmDescriptor(
            default,
            1,
            ThemeAppearanceEffect.Light,
            static () => throw new InvalidOperationException()));

        var input = new ThemeConfigBuilder()
                    .WithAlgorithms(default(ThemeAlgorithm))
                    .Build();

        var result = ThemeConfigNormalizer.Normalize(input, ThemeConfigTestSchema.Create());

        result.Success.ShouldBeFalse();
        result.Diagnostics.ShouldContain(static diagnostic =>
            diagnostic.Code == "ATMTHM4004" &&
            diagnostic.Path == "$.Algorithms[0]" &&
            diagnostic.Message == "Algorithm identity is uninitialized.");
    }

    [Theory]
    [InlineData("Default", "AtomUI:Default")]
    [InlineData("Dark", "AtomUI:Dark")]
    [InlineData("Compact", "AtomUI:Compact")]
    [InlineData("PrimeNG:Aura.Dark", "PrimeNG:Aura.Dark")]
    public void TypeConverter_Accepts_BuiltIn_Names_And_Catalog_Id(string text, string expected)
    {
        var converter = new ThemeAlgorithmTypeConverter();

        converter.ConvertFrom(null, null, text).ShouldBeOfType<ThemeAlgorithm>().ToString().ShouldBe(expected);
    }

    [Theory]
    [InlineData("DarkAlgorithm")]
    [InlineData("A:B:C")]
    [InlineData(":Dark")]
    public void TypeConverter_Rejects_Unknown_Forms(string text)
    {
        Should.Throw<Exception>(() => new ThemeAlgorithmTypeConverter().ConvertFrom(null, null, text));
    }

    [Fact]
    public void Compare_Orders_By_Catalog_Then_Id()
    {
        var algorithms = new List<ThemeAlgorithm>
        {
            new("PrimeNG", "Aura.Dark"),
            ThemeAlgorithm.Default,
            new("PrimeNG", "Aura"),
            ThemeAlgorithm.Dark,
            ThemeAlgorithm.Compact
        };

        algorithms.Sort(ThemeAlgorithm.Compare);

        algorithms.Select(static algorithm => algorithm.ToString()).ShouldBe(
        [
            "AtomUI:Compact",
            "AtomUI:Dark",
            "AtomUI:Default",
            "PrimeNG:Aura",
            "PrimeNG:Aura.Dark"
        ]);
    }
}
