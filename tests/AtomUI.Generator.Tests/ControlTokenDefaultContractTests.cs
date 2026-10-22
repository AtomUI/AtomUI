// Shared generator behavior: compiled dependency order and explicit-value preservation.
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using static AtomUI.Generator.Tests.Registration.TypeMapRegistrationGeneratorTests;

namespace AtomUI.Generator.Tests;

public sealed class ControlTokenDefaultContractTests
{
    [Fact]
    public void Generated_Plan_Should_Execute_With_Effective_Upstream_And_Explicit_Downstream()
    {
        var input = Compilation(Source, [], "AtomUI.Registration.Fixtures");
        var output = Generate(input, out var diagnostics);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        using var stream = new MemoryStream();
        var emitted = output.Emit(stream, cancellationToken: TestContext.Current.CancellationToken);
        emitted.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        LoadCore();
        var assembly = System.Reflection.Assembly.Load(stream.ToArray());
        var values = (double[])assembly.GetType("Demo.Runner")!.GetMethod("Run")!.Invoke(null, null)!;
        values.ShouldBe([20d, 40d, 3d, 7d, 0d, 23d, 14d, 28d, 9d]);
    }

    private static CSharpCompilation Generate(CSharpCompilation input, out System.Collections.Immutable.ImmutableArray<Diagnostic> diagnostics)
    {
        GeneratorDriver driver = CSharpGeneratorDriver.Create([new TokenResourceKeyGenerator().AsSourceGenerator()],
            parseOptions: (CSharpParseOptions)input.SyntaxTrees.First().Options, optionsProvider: new Options());
        driver.RunGeneratorsAndUpdateCompilation(input, out var output, out diagnostics, TestContext.Current.CancellationToken);
        return (CSharpCompilation)output;
    }

    private const string Source = """
        using System;
        using Avalonia;
        using AtomUI.Theme;
        using AtomUI.Theme.DesignTokens;
        namespace Demo;
        public sealed class Button : Avalonia.Controls.Control { }
        [ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
        internal sealed partial class ButtonToken : AbstractControlDesignToken, IControlTokenDefaults
        {
            public Thickness ContentPadding { get; set; }
            public double ContentLineHeight { get; set; }
            public double ContentFontSize { get; set; }
            [ControlTokenDefault(nameof(ContentFontSize))]
            private static double DefaultFont(in ControlTokenDefaultContext context) => context.Global.FontSize;
            [ControlTokenDefault(nameof(ContentLineHeight), nameof(ContentFontSize))]
            private static double DefaultLine(in ControlTokenDefaultContext context, double font) => font * 2;
            [ControlTokenDefault(nameof(ContentPadding), nameof(ContentLineHeight))]
            private static Thickness DefaultPadding(in ControlTokenDefaultContext context, double lineHeight) =>
                new(context.Global.PaddingContentHorizontal, Math.Max((context.Global.ControlHeight-lineHeight)/2-context.Global.LineWidth, 0));
        }
        public static class Runner
        {
            public static double[] Run()
            {
                var global = new DesignToken { FontSize=14, ControlHeight=50, LineWidth=2, PaddingContentHorizontal=16 };
                var context = new ControlTokenDefaultContext(global, ThemeAppearance.Light);
                var value = new ButtonToken { ContentFontSize=20 };
                ButtonToken.EvaluateDefaults(value, in context, [1UL]);
                var a=value.ContentFontSize; var b=value.ContentLineHeight; var c=value.ContentPadding.Top;
                value.ContentPadding=new Thickness(3,7);
                ButtonToken.EvaluateDefaults(value, in context, [5UL]);
                var d=value.ContentPadding.Top;
                value.ContentFontSize=0;
                ButtonToken.EvaluateDefaults(value, in context, [1UL]);
                var e=value.ContentLineHeight; var f=value.ContentPadding.Top;
                ButtonToken.EvaluateDefaults(value, in context, []);
                return [a,b,c,d,e,f,value.ContentFontSize,value.ContentLineHeight,value.ContentPadding.Top];
            }
        }
        """;
}
