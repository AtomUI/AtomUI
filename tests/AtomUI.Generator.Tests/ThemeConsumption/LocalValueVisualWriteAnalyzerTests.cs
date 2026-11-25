using AtomUI.Generator.ThemeConsumption;
using Microsoft.CodeAnalysis;
using Shouldly;
using Xunit;
using static AtomUI.Generator.Tests.ThemeConsumption.ThemeConsumptionAnalyzerTestHost;

namespace AtomUI.Generator.Tests.ThemeConsumption;

public class LocalValueVisualWriteAnalyzerTests
{
    private const string Id = "ATOMUIGEN047";

    private static string[] FlaggedTexts(IEnumerable<Diagnostic> diagnostics) => [.. diagnostics.Select(Flagged)];

    [Fact]
    public async Task Reports_SetValue_Without_Priority_On_Visual_StyledProperty()
    {
        var diagnostics = await AnalyzeAsync<LocalValueVisualWriteAnalyzer>(ControlWith("""
            private void Apply(Border border, IBrush brush)
            {
                border.SetValue(Border.BackgroundProperty, brush);
            }
            """));

        var diagnostic = diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe(Id);
        diagnostic.Severity.ShouldBe(DiagnosticSeverity.Warning);
        Flagged(diagnostic).ShouldBe("border.SetValue(Border.BackgroundProperty, brush)");
        diagnostic.GetMessage().ShouldContain("Background");
        diagnostic.GetMessage().ShouldContain("SetCurrentValue");
    }

    [Fact]
    public async Task Reports_SetValue_With_LocalValue_Priority_Including_Constructors()
    {
        var diagnostics = await AnalyzeAsync<LocalValueVisualWriteAnalyzer>(ControlWith("""
            public Sample()
            {
                SetValue(OpacityProperty, 0.5, BindingPriority.LocalValue);
            }
            """));

        var diagnostic = diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe(Id);
        Flagged(diagnostic).ShouldBe("SetValue(OpacityProperty, 0.5, BindingPriority.LocalValue)");
    }

    [Fact]
    public async Task Reports_SetValue_For_Custom_Visual_StyledProperty_Declared_In_Control()
    {
        var diagnostics = await AnalyzeAsync<LocalValueVisualWriteAnalyzer>(ControlWith("""
            public static readonly StyledProperty<double> FontSizeProperty =
                AvaloniaProperty.Register<Sample, double>(nameof(FontSize));

            public double FontSize
            {
                get => GetValue(FontSizeProperty);
                set => SetValue(FontSizeProperty, value);
            }

            private void Apply(double size)
            {
                SetValue(FontSizeProperty, size);
            }
            """));

        // The CLR wrapper's own `set => SetValue(...)` is the standard StyledProperty accessor and is exempt.
        var diagnostic = diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe(Id);
        Flagged(diagnostic).ShouldBe("SetValue(FontSizeProperty, size)");
    }

    [Fact]
    public async Task Reports_Property_Assignments_On_Control_Instances_Outside_Constructors()
    {
        var diagnostics = await AnalyzeAsync<LocalValueVisualWriteAnalyzer>(ControlWith("""
            private void Apply(Border border, IBrush brush)
            {
                border.Background = brush;
                this.Opacity = 0.5;
                Margin = new Thickness(4);
            }
            """));

        diagnostics.ShouldAllBe(static d => d.Id == Id);
        FlaggedTexts(diagnostics).ShouldBe(
        [
            "border.Background = brush",
            "this.Opacity = 0.5",
            "Margin = new Thickness(4)",
        ]);
    }

    [Fact]
    public async Task Does_Not_Report_SetCurrentValue_Or_NonLocal_Priorities()
    {
        var diagnostics = await AnalyzeAsync<LocalValueVisualWriteAnalyzer>(ControlWith("""
            private void Apply(Border border, IBrush brush, BindingPriority runtimePriority)
            {
                border.SetCurrentValue(Border.BackgroundProperty, brush);
                border.SetValue(Border.BackgroundProperty, brush, BindingPriority.Template);
                border.SetValue(Border.BackgroundProperty, brush, priority: BindingPriority.Style);
                border.SetValue(Border.BackgroundProperty, brush, runtimePriority);
            }
            """));

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task Does_Not_Report_SetValue_On_Non_Visual_Property_Or_Wrong_Value_Type()
    {
        var diagnostics = await AnalyzeAsync<LocalValueVisualWriteAnalyzer>(ControlWith("""
            public static readonly StyledProperty<string?> BackgroundProperty =
                AvaloniaProperty.Register<Sample, string?>("Background");

            private void Apply()
            {
                SetValue(IsVisibleProperty, false);
                SetValue(BackgroundProperty, "named-background");
            }
            """));

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task Does_Not_Report_Assignment_In_Constructor_Or_Object_Initializer()
    {
        var diagnostics = await AnalyzeAsync<LocalValueVisualWriteAnalyzer>(ControlWith("""
            public Sample()
            {
                Opacity = 0.8;
                Margin = new Thickness(2);
            }

            private Border Create(IBrush brush)
            {
                return new Border { Background = brush, Opacity = 0.5 };
            }
            """));

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task Does_Not_Report_Assignment_To_Non_Visual_Property_Or_Non_Control_Instance()
    {
        var diagnostics = await AnalyzeAsync<LocalValueVisualWriteAnalyzer>(ControlWith("""
            private void Apply(SolidColorBrush brush, Border border)
            {
                brush.Opacity = 0.5;
                border.IsVisible = false;
                border.Width = 10;
            }
            """));

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task Does_Not_Report_Inside_Control_Design_Token_Type()
    {
        var diagnostics = await AnalyzeAsync<LocalValueVisualWriteAnalyzer>($$"""
            {{Usings}}

            namespace Demo
            {
                [ControlDesignToken]
                public sealed class SampleToken : Border
                {
                    public void Apply(IBrush brush)
                    {
                        Background = brush;
                        SetValue(BackgroundProperty, brush);
                    }
                }
            }
            """);

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task Does_Not_Report_Assembly_Without_SharedTokenKind_Reference()
    {
        var diagnostics = await AnalyzeAsync<LocalValueVisualWriteAnalyzer>("""
            using Avalonia.Controls;
            using Avalonia.Media;

            namespace Demo
            {
                public class Sample : Control
                {
                    private void Apply(Border border, IBrush brush)
                    {
                        border.Background = brush;
                        border.SetValue(Border.BackgroundProperty, brush);
                    }
                }
            }
            """,
            withStubs: false);

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task Does_Not_Report_Assembly_That_Defines_SharedTokenKind()
    {
        var diagnostics = await AnalyzeAsync<LocalValueVisualWriteAnalyzer>(
            ControlWith("""
                private void Apply(Border border, IBrush brush)
                {
                    border.Background = brush;
                }
                """) + Stubs.Replace("using System;", string.Empty, StringComparison.Ordinal),
            withStubs: false);

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task Does_Not_Report_Application_Compilation_Without_Token_Types_Or_Opt_In()
    {
        var diagnostics = await AnalyzeAsync<LocalValueVisualWriteAnalyzer>(ControlWith("""
            private void Apply(Border border, IBrush brush)
            {
                border.SetValue(Border.BackgroundProperty, brush);
                border.Background = brush;
            }
            """),
            consumptionAnalysis: null);

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task Reports_When_Compilation_Declares_Control_Design_Token_Without_Opt_In()
    {
        var diagnostics = await AnalyzeAsync<LocalValueVisualWriteAnalyzer>(ControlWith("""
            private void Apply(Border border, IBrush brush)
            {
                border.SetValue(Border.BackgroundProperty, brush);
            }
            """,
            "[ControlDesignToken] public sealed class OtherToken { }"),
            consumptionAnalysis: null);

        diagnostics.ShouldHaveSingleItem().Id.ShouldBe(Id);
    }

    [Theory]
    [InlineData("false")]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Does_Not_Report_When_Opt_In_Is_False_Empty_Or_Whitespace(string value)
    {
        var diagnostics = await AnalyzeAsync<LocalValueVisualWriteAnalyzer>(ControlWith("""
            private void Apply(Border border, IBrush brush)
            {
                border.SetValue(Border.BackgroundProperty, brush);
            }
            """),
            consumptionAnalysis: value);

        diagnostics.ShouldBeEmpty();
    }
}
