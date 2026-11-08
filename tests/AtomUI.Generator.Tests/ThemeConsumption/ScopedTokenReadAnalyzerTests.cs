using AtomUI.Generator.ThemeConsumption;
using Microsoft.CodeAnalysis;
using Shouldly;
using Xunit;
using static AtomUI.Generator.Tests.ThemeConsumption.ThemeConsumptionAnalyzerTestHost;

namespace AtomUI.Generator.Tests.ThemeConsumption;

public class ScopedTokenReadAnalyzerTests
{
    private const string Id = "ATOMUIGEN043";

    [Fact]
    public async Task Reports_Application_Current_TryGetResource_With_SharedTokenKind()
    {
        var diagnostics = await AnalyzeAsync<ScopedTokenReadAnalyzer>(ControlWith("""
            private object? Read()
            {
                return Application.Current!.TryGetResource(SharedTokenKind.ColorPrimary, null, out var value) ? value : null;
            }
            """));

        var diagnostic = diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe(Id);
        diagnostic.Severity.ShouldBe(DiagnosticSeverity.Warning);
        Flagged(diagnostic).ShouldBe("Application.Current!.TryGetResource(SharedTokenKind.ColorPrimary, null, out var value)");
        diagnostic.GetMessage().ShouldContain("TryGetResource");
        diagnostic.GetMessage().ShouldContain("ControlTokenAccessor");
    }

    [Fact]
    public async Task Reports_Application_Current_TryFindResource_With_Generated_TokenKey()
    {
        var diagnostics = await AnalyzeAsync<ScopedTokenReadAnalyzer>(ControlWith("""
            private object? Read()
            {
                Application.Current!.TryFindResource(ButtonTokenKey.ColorPrimary, out var resource);
                return resource;
            }
            """));

        var diagnostic = diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe(Id);
        Flagged(diagnostic).ShouldBe("Application.Current!.TryFindResource(ButtonTokenKey.ColorPrimary, out var resource)");
    }

    [Fact]
    public async Task Reports_FindResource_On_Any_Object_With_ControlTokenResourceKey_Call()
    {
        var diagnostics = await AnalyzeAsync<ScopedTokenReadAnalyzer>(ControlWith("""
            private object? Read()
            {
                return this.FindResource(ControlTokenResourceKey.Global(null!, SharedTokenKind.ColorPrimary));
            }
            """));

        var diagnostic = diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe(Id);
        Flagged(diagnostic).ShouldBe("this.FindResource(ControlTokenResourceKey.Global(null!, SharedTokenKind.ColorPrimary))");
    }

    [Fact]
    public async Task Reports_Control_TryFindResource_With_Own_Token_Kind_Enum()
    {
        var diagnostics = await AnalyzeAsync<ScopedTokenReadAnalyzer>(ControlWith("""
            private object? Read()
            {
                return this.TryFindResource(SampleTokenKind.Gap, out var resource) ? resource : null;
            }
            """,
            "public enum SampleTokenKind { Gap }"));

        var diagnostic = diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe(Id);
        Flagged(diagnostic).ShouldBe("this.TryFindResource(SampleTokenKind.Gap, out var resource)");
    }

    [Fact]
    public async Task Reports_Read_Through_Conditional_Access_And_Local_Application_Variable()
    {
        var diagnostics = await AnalyzeAsync<ScopedTokenReadAnalyzer>(ControlWith("""
            private bool Read()
            {
                var application = Application.Current;
                return application?.TryGetResource(SharedTokenKind.ColorPrimary, null, out _) == true;
            }
            """));

        var diagnostic = diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe(Id);
        Flagged(diagnostic).ShouldBe(".TryGetResource(SharedTokenKind.ColorPrimary, null, out _)");
    }

    [Fact]
    public async Task Does_Not_Report_Control_Theme_Lookup_By_String_Key()
    {
        var diagnostics = await AnalyzeAsync<ScopedTokenReadAnalyzer>(ControlWith("""
            private ControlTheme? Read()
            {
                return Application.Current!.TryFindResource("TabItemTheme", out var resource) ? resource as ControlTheme : null;
            }
            """));

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task Does_Not_Report_Control_Theme_Lookup_By_Type_Key()
    {
        var diagnostics = await AnalyzeAsync<ScopedTokenReadAnalyzer>(ControlWith("""
            private object? Read()
            {
                return this.TryFindResource(typeof(Sample), out var resource) ? resource : null;
            }
            """));

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task Does_Not_Report_Forwarded_Key_Parameter()
    {
        // The key is only known at runtime (resource host forwarding); the analyzer cannot prove it is a Token key.
        var diagnostics = await AnalyzeAsync<ScopedTokenReadAnalyzer>(ControlWith("""
            public bool TryGetResource(object key, ThemeVariant? theme, out object? value)
            {
                if (Application.Current?.TryGetResource(key, theme, out value) == true)
                {
                    return true;
                }

                value = null;
                return false;
            }
            """));

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task Does_Not_Report_Token_Key_Passed_To_Non_Avalonia_Method_With_Same_Name()
    {
        var diagnostics = await AnalyzeAsync<ScopedTokenReadAnalyzer>(ControlWith("""
            private object? Read(Helper helper)
            {
                return helper.TryGetResource(SharedTokenKind.ColorPrimary, null, out var value) ? value : null;
            }
            """,
            """
            public sealed class Helper
            {
                public bool TryGetResource(object key, ThemeVariant? theme, out object? value)
                {
                    value = null;
                    return false;
                }
            }
            """));

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task Does_Not_Report_Token_Key_Held_In_Object_Variable()
    {
        // Boundary: data flow is not tracked, a key stored in an object-typed local is not recognised.
        var diagnostics = await AnalyzeAsync<ScopedTokenReadAnalyzer>(ControlWith("""
            private object? Read()
            {
                object key = SharedTokenKind.ColorPrimary;
                return this.TryFindResource(key, out var resource) ? resource : null;
            }
            """));

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task Does_Not_Report_Inside_Control_Design_Token_Type()
    {
        var diagnostics = await AnalyzeAsync<ScopedTokenReadAnalyzer>($$"""
            {{Usings}}

            namespace Demo
            {
                [ControlDesignToken]
                public sealed class SampleToken
                {
                    public object? Read() =>
                        Application.Current!.TryGetResource(SharedTokenKind.ColorPrimary, null, out var value) ? value : null;
                }
            }
            """);

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task Does_Not_Report_In_Assembly_That_Defines_SharedTokenKind()
    {
        // The assembly itself defines SharedTokenKind (AtomUI.Core): token infrastructure, not control code.
        var diagnostics = await AnalyzeAsync<ScopedTokenReadAnalyzer>(
            ControlWith("""
                private object? Read() =>
                    Application.Current!.TryGetResource(SharedTokenKind.ColorPrimary, null, out var value) ? value : null;
                """) + Stubs.Replace("using System;", string.Empty, StringComparison.Ordinal),
            withStubs: false);

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task Does_Not_Report_Assembly_Without_SharedTokenKind()
    {
        var diagnostics = await AnalyzeAsync<ScopedTokenReadAnalyzer>("""
            using Avalonia;
            using Avalonia.Controls;

            namespace Demo
            {
                public enum ButtonTokenKey { ColorPrimary }

                public class Sample : Control
                {
                    private object? Read() =>
                        Application.Current!.TryFindResource(ButtonTokenKey.ColorPrimary, out var value) ? value : null;
                }
            }
            """,
            withStubs: false);

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task Reports_Hard_Coded_Fallback_After_Positive_Read_That_Returns()
    {
        var diagnostics = await AnalyzeAsync<ScopedTokenReadAnalyzer>(ControlWith("""
            private TimeSpan Resolve()
            {
                var application = Application.Current;
                if (application?.TryGetResource(SharedTokenKind.MotionDurationMid, null, out var value) == true &&
                    value is TimeSpan duration)
                {
                    return duration;
                }

                return TimeSpan.FromMilliseconds(200);
            }
            """));

        diagnostics.Length.ShouldBe(2);
        diagnostics.ShouldAllBe(static d => d.Id == Id);
        Flagged(diagnostics[0]).ShouldStartWith(".TryGetResource(SharedTokenKind.MotionDurationMid");
        Flagged(diagnostics[1]).ShouldBe("TimeSpan.FromMilliseconds(200)");
        diagnostics[1].GetMessage().ShouldContain("fallback");
    }

    [Fact]
    public async Task Reports_Hard_Coded_Fallback_In_Negated_Read_Branch()
    {
        var diagnostics = await AnalyzeAsync<ScopedTokenReadAnalyzer>(ControlWith("""
            private double Resolve()
            {
                double size;
                if (!this.TryFindResource(SharedTokenKind.ColorPrimary, out var resource) || resource is not double d)
                {
                    size = 12;
                }
                else
                {
                    size = d;
                }

                return size;
            }
            """));

        diagnostics.Length.ShouldBe(2);
        Flagged(diagnostics[0]).ShouldStartWith("this.TryFindResource(SharedTokenKind.ColorPrimary");
        Flagged(diagnostics[1]).ShouldBe("12");
        diagnostics[1].GetMessage().ShouldContain("fallback");
    }

    [Fact]
    public async Task Reports_Hard_Coded_Fallback_In_Null_Coalescing_Right_Operand()
    {
        var diagnostics = await AnalyzeAsync<ScopedTokenReadAnalyzer>(ControlWith("""
            private IBrush Resolve()
            {
                return this.FindResource(SharedTokenKind.ColorPrimary) as IBrush ?? Brushes.Red;
            }
            """));

        diagnostics.Length.ShouldBe(2);
        Flagged(diagnostics[1]).ShouldBe("Brushes.Red");
    }

    [Fact]
    public async Task Reports_Hard_Coded_Fallback_In_Conditional_Expression()
    {
        var diagnostics = await AnalyzeAsync<ScopedTokenReadAnalyzer>(ControlWith("""
            private double Resolve()
            {
                return this.TryFindResource(SharedTokenKind.ColorPrimary, out var resource) && resource is double d ? d : 14d;
            }
            """));

        diagnostics.Length.ShouldBe(2);
        Flagged(diagnostics[1]).ShouldBe("14d");
    }

    [Fact]
    public async Task Does_Not_Report_Zero_Or_Null_Fallback_Only_The_Read()
    {
        var diagnostics = await AnalyzeAsync<ScopedTokenReadAnalyzer>(ControlWith("""
            private object? Resolve()
            {
                if (!this.TryFindResource(SharedTokenKind.ColorPrimary, out var resource))
                {
                    return null;
                }

                return resource;
            }

            private double ResolveZero()
            {
                if (!this.TryFindResource(SharedTokenKind.ColorPrimary, out var resource) || resource is not double d)
                {
                    return 0;
                }

                return d;
            }
            """));

        diagnostics.Length.ShouldBe(2);
        diagnostics.ShouldAllBe(static d => Flagged(d).StartsWith("this.TryFindResource(SharedTokenKind"));
    }

    [Fact]
    public async Task Does_Not_Report_Literal_After_Positive_Read_That_Does_Not_Exit()
    {
        var diagnostics = await AnalyzeAsync<ScopedTokenReadAnalyzer>(ControlWith("""
            private double Resolve()
            {
                var size = 0d;
                if (this.TryFindResource(SharedTokenKind.ColorPrimary, out var resource) && resource is double d)
                {
                    size = d;
                }

                size += 5;
                return size;
            }
            """));

        // Only the read itself: the code after the if also runs when the read succeeded, so it is not a fallback.
        diagnostics.ShouldHaveSingleItem().Id.ShouldBe(Id);
    }

    [Fact]
    public async Task Does_Not_Report_Fallback_Literal_For_Non_Token_Read()
    {
        var diagnostics = await AnalyzeAsync<ScopedTokenReadAnalyzer>(ControlWith("""
            private double Resolve()
            {
                double size;
                if (!this.TryFindResource("SomeKey", out var resource) || resource is not double d)
                {
                    size = 12;
                }
                else
                {
                    size = d;
                }

                return size;
            }
            """));

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task Does_Not_Report_Application_Compilation_Without_Token_Types_Or_Opt_In()
    {
        var diagnostics = await AnalyzeAsync<ScopedTokenReadAnalyzer>(ControlWith("""
            private object? Read() =>
                Application.Current!.TryFindResource(SharedTokenKind.ColorPrimary, out var value) ? value : null;
            """),
            consumptionAnalysis: null);

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task Reports_When_Compilation_Declares_Control_Design_Token_Without_Opt_In()
    {
        var diagnostics = await AnalyzeAsync<ScopedTokenReadAnalyzer>(ControlWith("""
            private object? Read() =>
                Application.Current!.TryFindResource(SharedTokenKind.ColorPrimary, out var value) ? value : null;
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
        var diagnostics = await AnalyzeAsync<ScopedTokenReadAnalyzer>(ControlWith("""
            private object? Read() =>
                Application.Current!.TryFindResource(SharedTokenKind.ColorPrimary, out var value) ? value : null;
            """),
            consumptionAnalysis: value);

        diagnostics.ShouldBeEmpty();
    }
}
