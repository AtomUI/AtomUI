using System.Collections.Immutable;
using AtomUI.Theme.Build;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;

namespace AtomUI.Generator.Tests.ThemeDefinitions;

/// <summary>
/// Covers the design-language metadata of compiled theme packages: ABI 2 emission and the in-package
/// cross-definition rules (duplicate variant, mixed brand color support).
/// </summary>
public class CompiledThemeMetadataTests
{
    private const string PackageFile = "GeneratedThemePackageRegistration.g.cs";

    [Fact]
    public void Generator_Emits_Abi3_With_Metadata()
    {
        var light = Input("AuraLight", 0, 0, 1);
        light.NeutralColor = 1;
        var dark = Input("AuraDark", 1, 0, 1);
        dark.NeutralColor = 1;
        var package = Run(out var diagnostics,
            light, dark);

        diagnostics.Where(static d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        var source = package.SyntaxTrees.Single(t => t.FilePath.EndsWith(PackageFile, StringComparison.Ordinal))
                            .GetText(TestContext.Current.CancellationToken).ToString();
        source.ShouldContain("base(3,");
        source.ShouldContain("\"PrimeNG.Aura\"");
        source.ShouldContain("(global::AtomUI.Theme.ThemeNeutralColorSupport)1");
        source.ShouldNotContain("base(1,");
        source.ShouldNotContain("base(2,");
    }

    [Fact]
    public void Generator_Reports_ATMTHM3108_For_Mixed_NeutralColor()
    {
        var seed = Input("AuraLight", 0, 0, 1);
        seed.NeutralColor = 1;
        Run(out var diagnostics, seed, Input("AuraDark", 1, 0, 1));

        var mixed = diagnostics.Where(static d => d.Severity == DiagnosticSeverity.Error)
                               .ShouldHaveSingleItem();
        mixed.Id.ShouldBe("ATMTHM3108");
        mixed.Descriptor.Title.ToString(null).ShouldBe("Mixed neutral color support in a design language");
        mixed.GetMessage(null).ShouldBe(
            "Design language 'PrimeNG.Aura' mixes Seed and Fixed neutral color support in this package; 'AuraDark' declares Fixed and is rejected");
    }

    [Fact]
    public void Generator_Reports_Duplicate_Variant_In_Package()
    {
        Run(out var diagnostics, Input("AuraLight", 0, 0, 1), Input("AuraLight2", 0, 0, 1));

        var duplicate = diagnostics.Where(static d => d.Severity == DiagnosticSeverity.Error)
                                   .ShouldHaveSingleItem();
        duplicate.Id.ShouldBe("ATMTHM3103");
        duplicate.Descriptor.Title.ToString(null).ShouldBe("Duplicate design language variant");
        duplicate.GetMessage(null).ShouldBe(
            "Design language 'PrimeNG.Aura' declares more than one definition for the Light/Normal variant in this package; 'AuraLight2' is rejected");
    }

    [Fact]
    public void Generator_Reports_Mixed_Brand_Support_In_Package()
    {
        Run(out var diagnostics, Input("AuraLight", 0, 0, 1), Input("AuraDark", 1, 0, 0));

        var mixed = diagnostics.Where(static d => d.Severity == DiagnosticSeverity.Error)
                               .ShouldHaveSingleItem();
        mixed.Id.ShouldBe("ATMTHM3104");
        mixed.Descriptor.Title.ToString(null).ShouldBe("Mixed brand color support in a design language");
        mixed.GetMessage(null).ShouldBe(
            "Design language 'PrimeNG.Aura' mixes Seed and Fixed brand color support in this package; 'AuraDark' declares Fixed and is rejected");
    }

    [Fact]
    public void Brand_Rejected_Definition_Does_Not_Take_A_Variant_Slot()
    {
        // AuraDark is rejected for 3104, so the later valid AuraDarkSeed owns the Dark/Normal slot (as at runtime).
        Run(out var diagnostics,
            Input("AuraLight", 0, 0, 1), Input("AuraDark", 1, 0, 0), Input("AuraDarkSeed", 1, 0, 1));

        var errors = diagnostics.Where(static d => d.Severity == DiagnosticSeverity.Error)
                                .Select(static d => d.Id).ToArray();
        errors.ShouldContain("ATMTHM3104");
        errors.ShouldNotContain("ATMTHM3103");
    }

    [Theory]
    [InlineData(1, false, true)]
    [InlineData(0, true, true)]
    [InlineData(1, true, false)]
    [InlineData(0, false, false)]
    public void Generator_Warns_When_Density_Disagrees_With_The_Algorithm_Chain(int density, bool compactAlgorithm, bool warns)
    {
        var input = Input("AuraLight", 0, density, 1);
        input.Algorithms = compactAlgorithm
            ? [new ThemeAlgorithmInput { Catalog = "AtomUI", Id = "Default" }, new ThemeAlgorithmInput { Catalog = "AtomUI", Id = "Compact" }]
            : [new ThemeAlgorithmInput { Catalog = "AtomUI", Id = "Default" }];

        Run(out var diagnostics, input);

        var mismatch = diagnostics.Where(static d => d.Id == "ATMTHM3105").ToArray();
        if (warns)
        {
            var warning = mismatch.ShouldHaveSingleItem();
            warning.Severity.ShouldBe(DiagnosticSeverity.Warning);
            warning.GetMessage(null).ShouldBe(
                $"Theme 'AuraLight' of design language 'PrimeNG.Aura' declares Density=\"{(density == 1 ? "Compact" : "Normal")}\" " +
                $"but its algorithm chain {(compactAlgorithm ? "contains" : "lacks")} the built-in Compact algorithm");
            // A Warning: the package still generates.
            diagnostics.Where(static d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        }
        else
        {
            mismatch.ShouldBeEmpty();
        }
    }

    private static CompiledThemeInput Input(string id, int appearance, int density, int brandColor) => new()
    {
        SourceIdentity = "Themes/" + id + ".xml",
        SourcePath = "/repo/Themes/" + id + ".xml",
        ContentDigest = new string('A', 64),
        Id = id,
        Name = id,
        Appearance = appearance,
        DesignLanguage = "PrimeNG.Aura",
        Density = density,
        BrandColor = brandColor
    };

    private static CSharpCompilation Run(out ImmutableArray<Diagnostic> diagnostics, params CompiledThemeInput[] inputs) =>
        CompiledThemeAlgorithmContractTests.RunGenerator(
            CompiledThemeAlgorithmContractTests.CreatePackageCompilation(CompiledThemeAlgorithmContractTests.ConsumerSource,
                CompiledThemeAlgorithmContractTests.GeneratedCoreReference()), out diagnostics, inputs);
}
