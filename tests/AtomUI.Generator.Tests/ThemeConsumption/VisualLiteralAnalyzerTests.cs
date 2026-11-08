using AtomUI.Generator.ThemeConsumption;
using Microsoft.CodeAnalysis;
using Shouldly;
using Xunit;
using static AtomUI.Generator.Tests.ThemeConsumption.ThemeConsumptionAnalyzerTestHost;

namespace AtomUI.Generator.Tests.ThemeConsumption;

public class VisualLiteralAnalyzerTests
{
    private const string Id = "ATOMUIGEN044";

    private static string[] FlaggedTexts(IEnumerable<Diagnostic> diagnostics) => [.. diagnostics.Select(Flagged)];

    [Fact]
    public async Task Reports_Color_FromRgb_In_Static_Readonly_Field_Initializer()
    {
        var diagnostics = await AnalyzeAsync<VisualLiteralAnalyzer>(ControlWith("""
            private static readonly Color BadgeRed = Color.FromRgb(0xF5, 0x22, 0x2D);
            """));

        var diagnostic = diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe(Id);
        diagnostic.Severity.ShouldBe(DiagnosticSeverity.Warning);
        Flagged(diagnostic).ShouldBe("Color.FromRgb(0xF5, 0x22, 0x2D)");
        diagnostic.GetMessage().ShouldContain("Color.FromRgb(0xF5, 0x22, 0x2D)");
    }

    [Fact]
    public async Task Reports_Color_Constructors_And_Parse_With_Constant_Arguments()
    {
        var diagnostics = await AnalyzeAsync<VisualLiteralAnalyzer>(ControlWith("""
            private static readonly Color[] Palette =
            {
                Color.FromArgb(128, 1, 2, 3),
                Color.FromUInt32(0xFF112233),
                Color.Parse("#FFAA00"),
            };
            """));

        diagnostics.ShouldAllBe(static d => d.Id == Id);
        FlaggedTexts(diagnostics).ShouldBe(
        [
            "Color.FromArgb(128, 1, 2, 3)",
            "Color.FromUInt32(0xFF112233)",
            "Color.Parse(\"#FFAA00\")",
        ]);
    }

    [Fact]
    public async Task Reports_Brushes_And_Colors_Members()
    {
        var diagnostics = await AnalyzeAsync<VisualLiteralAnalyzer>(ControlWith("""
            private IBrush Pick(bool flag) => flag ? Brushes.Red : new SolidColorBrush(Colors.White);
            """));

        diagnostics.ShouldAllBe(static d => d.Id == Id);
        FlaggedTexts(diagnostics).ShouldBe(["Brushes.Red", "Colors.White"]);
    }

    [Fact]
    public async Task Reports_SolidColorBrush_With_Literal_Argument()
    {
        var diagnostics = await AnalyzeAsync<VisualLiteralAnalyzer>(ControlWith("""
            private IBrush Make() => new SolidColorBrush(0xFF112233);
            """));

        var diagnostic = diagnostics.ShouldHaveSingleItem();
        Flagged(diagnostic).ShouldBe("new SolidColorBrush(0xFF112233)");
    }

    [Fact]
    public async Task Reports_Numeric_Literals_Assigned_To_Visual_Properties_And_Fields()
    {
        var diagnostics = await AnalyzeAsync<VisualLiteralAnalyzer>(ControlWith("""
            private static readonly double s_fontSize = 12;
            private double _strokeThickness = 1.5;

            private void Apply(Border border, TextBlock text)
            {
                text.FontSize = 14;
                border.BorderThickness = new Thickness(1);
                border.CornerRadius = new CornerRadius(4, 4, 0, 0);
                var child = new Border { CornerRadius = new CornerRadius(6) };
            }
            """));

        diagnostics.ShouldAllBe(static d => d.Id == Id);
        FlaggedTexts(diagnostics).ShouldBe(
        [
            "12",
            "1.5",
            "14",
            "new Thickness(1)",
            "new CornerRadius(4, 4, 0, 0)",
            "new CornerRadius(6)",
        ]);
    }

    [Fact]
    public async Task Reports_Pen_Thickness_And_SetCurrentValue_Literal()
    {
        var diagnostics = await AnalyzeAsync<VisualLiteralAnalyzer>(ControlWith("""
            private Pen Make(IBrush brush) => new Pen(brush, 1, null, PenLineCap.Round);

            private void Apply(TextBlock text)
            {
                text.SetCurrentValue(TextBlock.FontSizeProperty, 12d);
            }
            """));

        FlaggedTexts(diagnostics).ShouldBe(["1", "12d"]);
    }

    [Fact]
    public async Task Does_Not_Report_Transparent_Or_Zero()
    {
        var diagnostics = await AnalyzeAsync<VisualLiteralAnalyzer>(ControlWith("""
            private void Apply(Border border, TextBlock text)
            {
                border.Background = Brushes.Transparent;
                var color = Colors.Transparent;
                border.BorderThickness = new Thickness(0);
                border.CornerRadius = new CornerRadius(0);
                text.FontSize = 0;
            }
            """));

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task Does_Not_Report_Literal_Passed_Through_Local_Variable_Or_Named_Constant()
    {
        var diagnostics = await AnalyzeAsync<VisualLiteralAnalyzer>(ControlWith("""
            private const int Gap = 8;

            private void Apply(TextBlock text, Border border)
            {
                var size = 12;
                text.FontSize = size;
                border.BorderThickness = new Thickness(Gap);
                text.Width = 20;
                text.Margin = new Thickness(8);
            }
            """));

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task Does_Not_Report_Computed_Colors_And_Runtime_Parse()
    {
        var diagnostics = await AnalyzeAsync<VisualLiteralAnalyzer>(ControlWith("""
            private Color Blend(Color source, byte r, string text)
            {
                var withAlpha = Color.FromArgb(source.A, source.R, source.G, source.B);
                var computed = Color.FromRgb(r, r, r);
                Color.TryParse(text, out var parsed);
                return Color.Parse(text);
            }
            """));

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task Does_Not_Report_Inside_Control_Design_Token_Type()
    {
        var diagnostics = await AnalyzeAsync<VisualLiteralAnalyzer>($$"""
            {{Usings}}

            namespace Demo
            {
                [ControlDesignToken]
                public sealed class SampleToken
                {
                    public Color Primary { get; set; }
                    public double FontSize { get; set; } = 14;

                    public void CalculateTokenValues(bool dark)
                    {
                        Primary = Color.FromRgb(1, 2, 3);
                        FontSize = 12;
                    }
                }
            }
            """);

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task Does_Not_Report_Assembly_Without_SharedTokenKind_Reference()
    {
        var diagnostics = await AnalyzeAsync<VisualLiteralAnalyzer>("""
            using Avalonia.Controls;
            using Avalonia.Media;

            namespace Demo
            {
                public class Sample : Control
                {
                    private static readonly Color Red = Color.FromRgb(1, 2, 3);
                }
            }
            """,
            withStubs: false);

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task Does_Not_Report_Assembly_That_Defines_SharedTokenKind()
    {
        var diagnostics = await AnalyzeAsync<VisualLiteralAnalyzer>(
            ControlWith("""
                private static readonly Color Red = Color.FromRgb(1, 2, 3);
                """) + Stubs.Replace("using System;", string.Empty, StringComparison.Ordinal),
            withStubs: false);

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task Does_Not_Report_Application_Compilation_Without_Token_Types_Or_Opt_In()
    {
        var diagnostics = await AnalyzeAsync<VisualLiteralAnalyzer>(ControlWith("""
            private IBrush Pick() => Brushes.Red;
            private static readonly Color Blue = Color.FromRgb(1, 2, 3);
            """),
            consumptionAnalysis: null);

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task Reports_When_Compilation_Declares_Control_Design_Token_Without_Opt_In()
    {
        var diagnostics = await AnalyzeAsync<VisualLiteralAnalyzer>(ControlWith("""
            private IBrush Pick() => Brushes.Red;
            """,
            "[ControlDesignToken] public sealed class OtherToken { }"),
            consumptionAnalysis: null);

        FlaggedTexts(diagnostics).ShouldBe(["Brushes.Red"]);
    }

    [Fact]
    public async Task Matches_Exact_Visual_Member_Names_Only()
    {
        var diagnostics = await AnalyzeAsync<VisualLiteralAnalyzer>(ControlWith("""
            private double _legacyWindowsFrameBorderThickness = 1;
            private const double DefaultLabelFontSize = 12;
            private double _borderThickness = 1;
            private static readonly double s_fontSize = 14;

            private void Apply(Border other)
            {
                var fontSize = 12;
                double cornerRadius;
                cornerRadius = 4;
                other.CornerRadius = new CornerRadius(4);
                _legacyWindowsFrameBorderThickness = 2;
            }

            private void Parameter(double fontSize)
            {
                fontSize = 13;
            }
            """));

        FlaggedTexts(diagnostics).ShouldBe(["1", "14", "new CornerRadius(4)"]);
    }

    [Theory]
    [InlineData("false")]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Does_Not_Report_When_Opt_In_Is_False_Empty_Or_Whitespace(string value)
    {
        var diagnostics = await AnalyzeAsync<VisualLiteralAnalyzer>(ControlWith("""
            private IBrush Pick() => Brushes.Red;
            """),
            consumptionAnalysis: value);

        diagnostics.ShouldBeEmpty();
    }
}
