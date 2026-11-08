using System.Collections.Immutable;
using AtomUI.Generator.DesignToken;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Shouldly;
using Xunit;

namespace AtomUI.Generator.Tests.DesignToken;

public class OwnTokenSentinelAnalyzerTests
{
    private const string Stubs = """
        using System;

        namespace AtomUI.Theme.DesignTokens
        {
            [AttributeUsage(AttributeTargets.Class)]
            public sealed class ControlDesignTokenAttribute : Attribute
            {
            }

            public abstract class AbstractControlDesignToken
            {
                protected bool IsExplicit<TKind>(TKind kind) where TKind : struct, Enum => false;
                public virtual void CalculateTokenValues(bool isDarkMode)
                {
                }
            }
        }
        """;

    [Fact]
    public async Task Reports_DoubleIsNaN_Of_Own_Property_In_CalculateTokenValues()
    {
        var diagnostics = await AnalyzeAsync(CreateToken("A = !double.IsNaN(A) ? A : 1;"));

        var diagnostic = diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe("ATOMUIGEN040");
        diagnostic.Severity.ShouldBe(DiagnosticSeverity.Error);
        diagnostic.GetMessage().ShouldContain("'A'");
        diagnostic.GetMessage().ShouldContain("IsExplicit(DemoTokenKind.A)");
        var flagged = diagnostic.Location.SourceTree!.GetText(TestContext.Current.CancellationToken).ToString(diagnostic.Location.SourceSpan);
        flagged.ShouldBe("double.IsNaN(A)");
    }

    [Fact]
    public async Task Reports_FloatIsNaN_Through_This_Access()
    {
        var diagnostics = await AnalyzeAsync(CreateToken("if (float.IsNaN(this.F)) { F = 1; }"));

        var diagnostic = diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe("ATOMUIGEN040");
        diagnostic.GetMessage().ShouldContain("'F'");
    }

    [Fact]
    public async Task Reports_IsNaN_Of_Property_Declared_On_Base_Control_Token_Layer()
    {
        var diagnostics = await AnalyzeAsync("""
            using AtomUI.Theme.DesignTokens;

            namespace Demo
            {
                [ControlDesignToken]
                public class BaseToken : AbstractControlDesignToken
                {
                    public double B { get; set; }
                }

                [ControlDesignToken]
                public sealed class DemoToken : BaseToken
                {
                    public override void CalculateTokenValues(bool isDarkMode)
                    {
                        if (double.IsNaN(B)) { B = 2; }
                    }
                }
            }
            """);

        diagnostics.ShouldHaveSingleItem().Id.ShouldBe("ATOMUIGEN040");
    }

    [Fact]
    public async Task Does_Not_Report_IsExplicit_Pattern()
    {
        var diagnostics = await AnalyzeAsync(CreateToken("if (!IsExplicit(DemoTokenKind.A)) { A = 1; }"));

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task Does_Not_Report_IsNaN_Outside_CalculateTokenValues()
    {
        var diagnostics = await AnalyzeAsync(CreateToken(
            "A = 1;",
            "private bool Helper() => double.IsNaN(A);"));

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task Does_Not_Report_IsNaN_Of_Local_Variable_In_CalculateTokenValues()
    {
        var diagnostics = await AnalyzeAsync(CreateToken("""
            var local = 1.0d;
            A = double.IsNaN(local) ? 0 : local;
            """));

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task Does_Not_Report_Type_Without_ControlDesignToken_Attribute()
    {
        var diagnostics = await AnalyzeAsync("""
            namespace Demo
            {
                public sealed class PlainToken
                {
                    public double A { get; set; }

                    public void CalculateTokenValues(bool isDarkMode)
                    {
                        A = !double.IsNaN(A) ? A : 1;
                    }
                }
            }
            """);

        diagnostics.ShouldBeEmpty();
    }

    private static string CreateToken(string calculateBody, string extraMembers = "")
    {
        return $$"""
            {{Stubs}}

            namespace Demo
            {
                using AtomUI.Theme.DesignTokens;

                [ControlDesignToken]
                public sealed class DemoToken : AbstractControlDesignToken
                {
                    public double A { get; set; }
                    public float F { get; set; }

                    {{extraMembers}}

                    public override void CalculateTokenValues(bool isDarkMode)
                    {
                        {{calculateBody}}
                    }
                }

                public enum DemoTokenKind
                {
                    A = 0,
                    F = 1,
                }
            }
            """;
    }

    private static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(string source)
    {
        var trees = new List<SyntaxTree>();
        if (!source.Contains("class ControlDesignTokenAttribute", StringComparison.Ordinal))
        {
            trees.Add(CSharpSyntaxTree.ParseText(Stubs));
        }

        trees.Add(CSharpSyntaxTree.ParseText(source));

        var references = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))!
                         .Split(Path.PathSeparator)
                         .Select(path => MetadataReference.CreateFromFile(path))
                         .Cast<MetadataReference>()
                         .ToImmutableArray();

        var compilation = CSharpCompilation.Create(
            "OwnTokenSentinelAnalyzerTests",
            trees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        compilation.GetDiagnostics(TestContext.Current.CancellationToken)
                   .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
                   .ShouldBeEmpty();

        var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(new OwnTokenSentinelAnalyzer());
        var compilationWithAnalyzers = compilation.WithAnalyzers(analyzers);
        return await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync(TestContext.Current.CancellationToken);
    }
}
