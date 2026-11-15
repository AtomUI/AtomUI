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
    public void Generator_Emits_Abi2_With_Metadata()
    {
        var package = Run(out var diagnostics,
            Input("AuraLight", 0, 0, 1), Input("AuraDark", 1, 0, 1));

        diagnostics.Where(static d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        var source = package.SyntaxTrees.Single(t => t.FilePath.EndsWith(PackageFile, StringComparison.Ordinal))
                            .GetText(TestContext.Current.CancellationToken).ToString();
        source.ShouldContain("base(2,");
        source.ShouldContain("\"PrimeNG.Aura\"");
        source.ShouldNotContain("base(1,");
    }

    [Fact]
    public void Generator_Reports_Duplicate_Variant_In_Package()
    {
        Run(out var diagnostics, Input("AuraLight", 0, 0, 1), Input("AuraLight2", 0, 0, 1));

        diagnostics.Where(static d => d.Severity == DiagnosticSeverity.Error)
                   .ShouldContain(d => d.GetMessage(null).Contains("ATMTHM3103"));
    }

    [Fact]
    public void Generator_Reports_Mixed_Brand_Support_In_Package()
    {
        Run(out var diagnostics, Input("AuraLight", 0, 0, 1), Input("AuraDark", 1, 0, 0));

        diagnostics.Where(static d => d.Severity == DiagnosticSeverity.Error)
                   .ShouldContain(d => d.GetMessage(null).Contains("ATMTHM3104"));
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
