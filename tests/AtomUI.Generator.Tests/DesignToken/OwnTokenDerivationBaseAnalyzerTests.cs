using System.Collections.Immutable;
using AtomUI.Generator.DesignToken;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Shouldly;
using Xunit;

namespace AtomUI.Generator.Tests.DesignToken;

public class OwnTokenDerivationBaseAnalyzerTests
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
    public async Task Reports_Unguarded_Assignment_Whose_Value_Is_Read_Later()
    {
        var diagnostics = await AnalyzeAsync(CreateToken("""
            A = 1;
            B = A * 2;
            """));

        var diagnostic = diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe("ATOMUIGEN048");
        diagnostic.Severity.ShouldBe(DiagnosticSeverity.Error);
        diagnostic.GetMessage().ShouldContain("'A'");
        diagnostic.GetMessage().ShouldContain("IsExplicit(DemoTokenKind.A)");
        var flagged = diagnostic.Location.SourceTree!.GetText(TestContext.Current.CancellationToken).ToString(diagnostic.Location.SourceSpan);
        flagged.ShouldBe("A = 1");
    }

    [Fact]
    public async Task Reports_When_Value_Is_Read_As_Constructor_Argument()
    {
        var diagnostics = await AnalyzeAsync(CreateToken("""
            A = 1;
            T = new Thickness(A);
            """));

        diagnostics.ShouldHaveSingleItem().Id.ShouldBe("ATOMUIGEN048");
    }

    [Fact]
    public async Task Reports_When_Value_Is_Read_Through_This_Access()
    {
        var diagnostics = await AnalyzeAsync(CreateToken("""
            this.A = 1;
            B = this.A * 2;
            """));

        diagnostics.ShouldHaveSingleItem().Id.ShouldBe("ATOMUIGEN048");
    }

    [Fact]
    public async Task Reports_Compound_Assignment_That_Is_Read_Later()
    {
        var diagnostics = await AnalyzeAsync(CreateToken("""
            if (!IsExplicit(DemoTokenKind.A)) { A = 1; }
            A += 2;
            B = A;
            """));

        var diagnostic = diagnostics.ShouldHaveSingleItem();
        var flagged = diagnostic.Location.SourceTree!.GetText(TestContext.Current.CancellationToken).ToString(diagnostic.Location.SourceSpan);
        flagged.ShouldBe("A += 2");
    }

    [Fact]
    public async Task Reports_Both_Branches_Of_If_Else_When_Read_Later()
    {
        var diagnostics = await AnalyzeAsync(CreateToken("""
            if (isDarkMode) { A = 1; } else { A = 2; }
            B = A;
            """));

        diagnostics.Length.ShouldBe(2);
        diagnostics.ShouldAllBe(static diagnostic => diagnostic.Id == "ATOMUIGEN048");
    }

    [Fact]
    public async Task Reports_Assignment_Under_Unrelated_Condition()
    {
        var diagnostics = await AnalyzeAsync(CreateToken("""
            if (!IsExplicit(DemoTokenKind.F)) { A = 1; }
            B = A;
            """));

        diagnostics.ShouldHaveSingleItem().Id.ShouldBe("ATOMUIGEN048");
    }

    [Fact]
    public async Task Does_Not_Report_Guarded_Assignment_With_Block()
    {
        var diagnostics = await AnalyzeAsync(CreateToken("""
            if (!IsExplicit(DemoTokenKind.A)) { A = 1; }
            B = A * 2;
            """));

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task Does_Not_Report_Guarded_Assignment_Without_Block()
    {
        var diagnostics = await AnalyzeAsync(CreateToken("""
            if (!IsExplicit(DemoTokenKind.A)) A = 1;
            B = A * 2;
            """));

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task Reports_When_Guard_Calls_A_Same_Named_Local_Method()
    {
        var diagnostics = await AnalyzeAsync(CreateToken(
            """
            if (!IsExplicit(DemoTokenKind.A)) { A = 1; }
            B = A;
            """,
            "private bool IsExplicit(DemoTokenKind kind) => false;"));

        diagnostics.ShouldHaveSingleItem().Id.ShouldBe("ATOMUIGEN048");
    }

    [Fact]
    public async Task Reports_When_Guard_Uses_A_Different_Enum_Type()
    {
        var diagnostics = await AnalyzeAsync(CreateToken(
            """
            if (!IsExplicit(OtherKind.A)) { A = 1; }
            B = A;
            """,
            "public enum OtherKind { A = 0 }"));

        diagnostics.ShouldHaveSingleItem().Id.ShouldBe("ATOMUIGEN048");
    }

    [Fact]
    public async Task Reports_When_Guard_Names_A_Different_Member()
    {
        var diagnostics = await AnalyzeAsync(CreateToken("""
            if (!IsExplicit(DemoTokenKind.B)) { A = 1; }
            B = A;
            """));

        diagnostics.ShouldHaveSingleItem().Id.ShouldBe("ATOMUIGEN048");
    }

    [Fact]
    public async Task Does_Not_Report_Assignment_That_Is_Never_Read()
    {
        var diagnostics = await AnalyzeAsync(CreateToken("A = 1;"));

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task Does_Not_Report_When_Read_Happens_Before_Assignment()
    {
        var diagnostics = await AnalyzeAsync(CreateToken("""
            B = A;
            A = 1;
            """));

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task Does_Not_Count_Self_Reference_In_Assigned_Value_As_Later_Read()
    {
        var diagnostics = await AnalyzeAsync(CreateToken("A = A + 1;"));

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task Does_Not_Report_Assignment_To_Local_Variable()
    {
        var diagnostics = await AnalyzeAsync(CreateToken("""
            var local = 1d;
            B = local;
            """));

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task Does_Not_Report_Outside_CalculateTokenValues()
    {
        var diagnostics = await AnalyzeAsync(CreateToken(
            "A = 1;",
            "private void Helper() { A = 1; B = A; }"));

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
                    public double B { get; set; }

                    public void CalculateTokenValues(bool isDarkMode)
                    {
                        A = 1;
                        B = A;
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
                    public double B { get; set; }
                    public float F { get; set; }
                    public Thickness T { get; set; }

                    {{extraMembers}}

                    public override void CalculateTokenValues(bool isDarkMode)
                    {
                        {{calculateBody}}
                    }
                }

                public readonly struct Thickness
                {
                    public Thickness(double uniform)
                    {
                    }
                }

                public enum DemoTokenKind
                {
                    A = 0,
                    B = 1,
                    F = 2,
                    T = 3,
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
            "OwnTokenDerivationBaseAnalyzerTests",
            trees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        compilation.GetDiagnostics(TestContext.Current.CancellationToken)
                   .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
                   .ShouldBeEmpty();

        var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(new OwnTokenDerivationBaseAnalyzer());
        var compilationWithAnalyzers = compilation.WithAnalyzers(analyzers);
        return await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync(TestContext.Current.CancellationToken);
    }
}
