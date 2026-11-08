using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;

namespace AtomUI.Generator.Tests.ThemeSchema;

public class GeneratedThemeConsumptionWriterTests
{
    [Fact]
    public void Emits_A_Compilable_Internal_Class_With_Own_Token_And_Global_Consumer_Arrays()
    {
        var output = RunGenerator(Source, out _, new InMemoryAdditionalText("Button/Themes/ButtonTheme.axaml", """
            <ControlTheme xmlns="https://github.com/avaloniaui" xmlns:atom="using:Demo" TargetType="atom:Button">
                <Setter Property="Foreground" Value="{atom:ButtonTokenResource ColorPrimary}" />
                <Setter Property="Height" Value="{atom:ButtonTokenResource Height}" />
            </ControlTheme>
            """));

        var tree = output.SyntaxTrees.Single(static tree => tree.FilePath.EndsWith("GeneratedThemeConsumption.g.cs", StringComparison.Ordinal));
        var source = tree.GetText(TestContext.Current.CancellationToken).ToString();

        source.ShouldContain("namespace AtomUI.Generated.GeneratedThemeConsumptionWriterTests;");
        source.ShouldContain("internal static class GeneratedThemeConsumption");
        source.ShouldContain("public static readonly OwnTokenEntry[] OwnTokens");
        source.ShouldContain("public static readonly GlobalConsumerEntry[] GlobalConsumers");
        source.ShouldContain("internal readonly record struct OwnTokenEntry(string Catalog, string ControlId, string Token, OwnTokenState State, string? ReservedReason);");
        source.ShouldContain("internal readonly record struct GlobalConsumerEntry(string Catalog, string ControlId, string GlobalToken, string Source);");
        source.ShouldContain("internal enum OwnTokenState");
        source.ShouldContain("Active,");
        source.ShouldContain("Reserved,");
        source.ShouldContain("Unreferenced");
        source.ShouldContain("new OwnTokenEntry(\"AtomUI\", \"Button\", \"Height\", OwnTokenState.Active, null),");
        source.ShouldContain("new OwnTokenEntry(\"AtomUI\", \"Button\", \"Label\", OwnTokenState.Unreferenced, null),");
        source.ShouldContain("new OwnTokenEntry(\"AtomUI\", \"Button\", \"Gap\", OwnTokenState.Reserved, \"quote \\\" and \\\\ backslash\"),");
        source.ShouldContain("new GlobalConsumerEntry(\"AtomUI\", \"Button\", \"ColorPrimary\", \"axaml\"),");
        source.ShouldContain("new GlobalConsumerEntry(\"AtomUI\", \"Button\", \"ControlHeight\", \"code\"),");
        output.GetDiagnostics(TestContext.Current.CancellationToken)
              .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error && diagnostic.Location.SourceTree == tree)
              .ShouldBeEmpty();
    }

    [Fact]
    public void Entries_Are_Ordered_By_Control_And_Token_Independent_Of_Declaration_Order()
    {
        var first = Generated(RunGenerator(Source, out _));
        var second = Generated(RunGenerator(Source.Replace("public string Label { get; set; } = string.Empty;", "")
                                                  .Replace("public double Height { get; set; }",
                                                      "public double Height { get; set; }\n        public string Label { get; set; } = string.Empty;"),
            out _));

        first.ShouldBe(second);
        var gap = first.IndexOf("\"Gap\"", StringComparison.Ordinal);
        var height = first.IndexOf("\"Height\"", StringComparison.Ordinal);
        var label = first.IndexOf("\"Label\"", StringComparison.Ordinal);
        gap.ShouldBeGreaterThan(0);
        gap.ShouldBeLessThan(height);
        height.ShouldBeLessThan(label);
    }

    [Fact]
    public void A_Package_Without_Own_Tokens_Or_Consumers_Still_Emits_Empty_Compilable_Arrays()
    {
        var output = RunGenerator("""
            namespace Demo
            {
                public sealed class Empty
                {
                }
            }
            """, out _);

        var tree = output.SyntaxTrees.Single(static tree => tree.FilePath.EndsWith("GeneratedThemeConsumption.g.cs", StringComparison.Ordinal));
        var source = tree.GetText(TestContext.Current.CancellationToken).ToString();

        source.ShouldNotContain("new OwnTokenEntry(");
        source.ShouldNotContain("new GlobalConsumerEntry(");
        output.GetDiagnostics(TestContext.Current.CancellationToken)
              .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error && diagnostic.Location.SourceTree == tree)
              .ShouldBeEmpty();
    }

    private static string Generated(CSharpCompilation compilation) =>
        compilation.SyntaxTrees
                   .Single(static tree => tree.FilePath.EndsWith("GeneratedThemeConsumption.g.cs", StringComparison.Ordinal))
                   .GetText(TestContext.Current.CancellationToken).ToString();

    private static CSharpCompilation RunGenerator(string source, out ImmutableArray<Diagnostic> diagnostics, params AdditionalText[] additionalTexts)
    {
        var references = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))!
                         .Split(Path.PathSeparator)
                         .Select(static path => MetadataReference.CreateFromFile(path))
                         .Cast<MetadataReference>()
                         .ToImmutableArray();
        var compilation = CSharpCompilation.Create(
            "GeneratedThemeConsumptionWriterTests",
            [CSharpSyntaxTree.ParseText(source, path: "TokenSource.cs"), CSharpSyntaxTree.ParseText(ThemeSchemaGeneratorTests.AtomUIStubs)],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var driver = CSharpGeneratorDriver.Create(
            [new TokenResourceKeyGenerator().AsSourceGenerator()],
            additionalTexts.ToImmutableArray(),
            (CSharpParseOptions)compilation.SyntaxTrees[0].Options,
            new BuiltinCatalogOptionsProvider(null));
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out diagnostics, TestContext.Current.CancellationToken);
        return (CSharpCompilation)output;
    }

    private const string Source = """
        using AtomUI.Theme.DesignTokens;

        namespace Demo
        {
            [GlobalDesignToken]
            public partial class GlobalToken : AbstractDesignToken
            {
                public double ControlHeight { get; set; }
                public string ColorPrimary { get; set; } = string.Empty;
            }

            [ControlDesignToken]
            internal sealed class ButtonToken : AbstractControlDesignToken
            {
                public string Label { get; set; } = string.Empty;
                public double Height { get; set; }

                [ReservedToken("quote \" and \\ backslash")]
                public double Gap { get; set; }

                public override void CalculateTokenValues(bool isDark)
                {
                    Height = EffectiveGlobalToken.ControlHeight;
                }
            }
        }
        """;

    private sealed class InMemoryAdditionalText : AdditionalText
    {
        private readonly Microsoft.CodeAnalysis.Text.SourceText _text;

        internal InMemoryAdditionalText(string path, string text)
        {
            Path = path;
            _text = Microsoft.CodeAnalysis.Text.SourceText.From(text);
        }

        public override string Path { get; }

        public override Microsoft.CodeAnalysis.Text.SourceText GetText(CancellationToken cancellationToken = default) => _text;
    }
}
