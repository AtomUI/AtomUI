using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using static AtomUI.Generator.Tests.DesignLanguages.DesignLanguageTestHost;

namespace AtomUI.Generator.Tests.DesignLanguages;

/// <summary>
/// The build-time checks: completeness and read-before-write of a rule, coverage of a value package, package
/// dependency direction, and the token schema digests a control package publishes and a value package records.
/// </summary>
public class ControlTokenRuleAnalysisTests
{
    private const string TwoControlsSource = """
        using AtomUI.Theme.DesignTokens;

        namespace Acme.Controls
        {
            public sealed class ToolTip : Avalonia.Controls.Control
            {
            }

            public sealed class Popover : Avalonia.Controls.Control
            {
            }

            [ControlDesignToken]
            public sealed partial class ToolTipToken : AbstractControlDesignToken
            {
                public partial double Padding { get; set; }
                public partial double Margin { get; set; }
            }

            [ControlDesignToken]
            public sealed partial class PopoverToken : AbstractControlDesignToken
            {
                public partial double ArrowSize { get; set; }
            }
        }
        """;

    [Fact]
    public void A_Complete_Value_Package_Passes_Every_Check()
    {
        var package = RunValuePackage(ToolTip("target.Own.Padding = 4; target.Own.Margin = 2;") + Popover("target.Own.ArrowSize = 8;"));

        package.Diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public void A_Token_No_Token_Rule_Writes_Is_Reported()
    {
        var package = RunValuePackage(ToolTip("target.Own.Padding = 4;") + Popover("target.Own.ArrowSize = 8;"));

        package.Diagnostics.ShouldHaveSingleItem().Id.ShouldBe("ATOMUIGEN050");
        package.Diagnostics[0].GetMessage(null).ShouldContain("'Margin'");
    }

    [Fact]
    public void A_Token_Written_By_A_Shared_Helper_Counts_As_Written()
    {
        var package = RunValuePackage(ToolTip("target.Own.Padding = 4; Shared.Margins(target.Own);") +
                                      Popover("target.Own.ArrowSize = 8;") + """
            namespace Acme.Shadcn.Values
            {
                internal static class Shared
                {
                    internal static void Margins(Acme.Controls.ToolTipToken token) => token.Margin = 2;
                }
            }
            """);

        package.Diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public void Reading_A_Token_Before_An_Explicit_Rule_Writes_It_Is_Reported()
    {
        var toolTip = ToolTip("target.Own.Padding = target.Own.Margin * 2; target.Own.Margin = 2;")
            .Replace("public void Apply(", "void IControlTokenRule<Acme.Shadcn.ShadcnToken, Acme.Controls.ToolTipToken>.Apply(",
                StringComparison.Ordinal);
        var package = RunValuePackage(toolTip + Popover("target.Own.ArrowSize = 8;"));

        Errors(package.Compilation).ShouldBeEmpty();
        var diagnostic = package.Diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe("ATOMUIGEN051");
        diagnostic.GetMessage(null).ShouldContain("'Margin'");
    }

    [Fact]
    public void Reading_A_Token_After_Writing_It_Is_Fine()
    {
        var package = RunValuePackage(ToolTip("target.Own.Margin = 2; target.Own.Padding = target.Own.Margin * 2;") +
                                      Popover("target.Own.ArrowSize = 8;"));

        package.Diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public void A_Token_Class_Of_A_Covered_Control_Package_Without_A_Token_Rule_Is_Reported()
    {
        var package = RunValuePackage(ToolTip("target.Own.Padding = 4; target.Own.Margin = 2;"));

        var diagnostic = package.Diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe("ATOMUIGEN052");
        diagnostic.GetMessage(null).ShouldContain("Acme.Controls.PopoverToken");
        diagnostic.GetMessage(null).ShouldContain("'Acme.Controls'");
    }

    [Fact]
    public void A_Value_Package_That_Declares_A_Control_Is_Reported()
    {
        var package = RunValuePackage(ToolTip("target.Own.Padding = 4; target.Own.Margin = 2;") + Popover("target.Own.ArrowSize = 8;") + """
            namespace Acme.Shadcn.Values
            {
                public sealed class Badge : Avalonia.Controls.Control
                {
                }
            }
            """);

        var diagnostic = package.Diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe("ATOMUIGEN053");
        diagnostic.GetMessage(null).ShouldContain("Badge");
    }

    [Fact]
    public void A_Control_Package_Publishes_Its_Token_Schema_Digest_And_The_Value_Package_Records_It()
    {
        var core = CoreReference();
        var controls = Run(CreatePackage("Acme.Controls", TwoControlsSource, core), out _);
        var digest = GetGeneratedSource(controls, "GeneratedTokenSchemaDigest.g.cs");
        digest.ShouldContain("[assembly: global::AtomUI.Theme.Schema.TokenSchemaDigestAttribute(\"");
        var value = digest[(digest.IndexOf('"') + 1)..digest.LastIndexOf('"')];
        value.Length.ShouldBe(64);
        GetGeneratedSource(controls, IdentitiesFile).ShouldContain($".TokenSchemaDigest => \"{value}\";");

        var package = RunValuePackage(ToolTip("target.Own.Padding = 4; target.Own.Margin = 2;") + Popover("target.Own.ArrowSize = 8;"));

        var registration = GetGeneratedSource(package.Compilation, TokenRuleRegistrationFile);
        registration.ShouldContain($"[assembly: global::AtomUI.Theme.Schema.CoversTokenSchemaAttribute(\"Acme.Controls\", \"{value}\")]");
        registration.ShouldContain($">(\"{value}\"),");
    }

    [Fact]
    public void The_Digest_Changes_When_A_Token_Is_Added()
    {
        var core = CoreReference();
        var before = GetGeneratedSource(Run(CreatePackage("Acme.Controls", TwoControlsSource, core), out _),
            "GeneratedTokenSchemaDigest.g.cs");
        var after = GetGeneratedSource(Run(CreatePackage("Acme.Controls",
            TwoControlsSource.Replace("public partial double ArrowSize { get; set; }",
                "public partial double ArrowSize { get; set; }\n        public partial double ArrowGap { get; set; }",
                StringComparison.Ordinal), core), out _), "GeneratedTokenSchemaDigest.g.cs");

        after.ShouldNotBe(before);
    }

    [Fact]
    public void A_Language_Package_That_References_A_Control_Package_Is_Reported()
    {
        var core = CoreReference();
        var controls = Reference(CreatePackage("Acme.Controls", TwoControlsSource, core), "Acme.Controls");

        Run(CreatePackage("Acme.Shadcn", LanguageSource, core, controls), out var diagnostics);

        diagnostics.ShouldContain(diagnostic => diagnostic.Id == "ATOMUIGEN053" &&
                                                diagnostic.GetMessage(null).Contains("'Acme.Controls'"));
    }

    [Fact]
    public void A_Control_Package_That_References_The_Language_Side_Is_Reported()
    {
        var core = CoreReference();
        var shadcn = Reference(CreatePackage("Acme.Shadcn", LanguageSource, core), "Acme.Shadcn");

        Run(CreatePackage("Acme.Controls", TwoControlsSource, core, shadcn), out var diagnostics);

        diagnostics.ShouldContain(diagnostic => diagnostic.Id == "ATOMUIGEN053" &&
                                                diagnostic.GetMessage(null).Contains("'Acme.Shadcn'"));
    }

    [Fact]
    public void A_Derived_Language_Value_Package_Covers_Only_The_Controls_It_Changes()
    {
        const string languages = """
            using AtomUI.Theme.DesignTokens;

            namespace Acme.Brand
            {
                [LanguageDesignToken]
                public partial class BaseToken : LanguageDesignToken
                {
                }

                [LanguageDesignToken]
                public partial class BrandToken : BaseToken
                {
                }
            }
            """;
        const string rules = """
            using AtomUI.Theme.Languages;

            namespace Acme.Brand.Values
            {
                [ControlTokenRule]
                internal sealed class BrandToolTipTokenRule : IControlTokenRule<Acme.Brand.BrandToken, Acme.Controls.ToolTipToken>
                {
                    public void Apply(in TokenEvaluationContext<Acme.Brand.BrandToken> context, ControlTokenTarget<Acme.Controls.ToolTipToken> target)
                    {
                        target.Own.Padding = 4;
                        target.Own.Margin  = 2;
                    }
                }

                [ControlTokenRule]
                internal sealed class BaseToolTipTokenRule : IControlTokenRule<Acme.Brand.BaseToken, Acme.Controls.ToolTipToken>
                {
                    public void Apply(in TokenEvaluationContext<Acme.Brand.BaseToken> context, ControlTokenTarget<Acme.Controls.ToolTipToken> target)
                    {
                        target.Own.Padding = 6;
                        target.Own.Margin  = 3;
                    }
                }
            }
            """;
        var core = CoreReference();
        var brand = Reference(CreatePackage("Acme.Brand", languages, core), "Acme.Brand");
        var controls = Reference(CreatePackage("Acme.Controls", TwoControlsSource, core), "Acme.Controls");
        Run(CreatePackage("Acme.Brand.Values", rules, core, brand, controls), out var diagnostics);

        // The derived language inherits the popover from its base language; the base language itself must cover it.
        var diagnostic = diagnostics.Where(static item => item.Severity == DiagnosticSeverity.Error).ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe("ATOMUIGEN052");
        diagnostic.GetMessage(null).ShouldContain("Acme.Brand.BaseToken");
        diagnostic.GetMessage(null).ShouldContain("Acme.Controls.PopoverToken");
    }

    private static (CSharpCompilation Compilation, Diagnostic[] Diagnostics) RunValuePackage(string rules)
    {
        var core = CoreReference();
        var shadcn = Reference(CreatePackage("Acme.Shadcn", LanguageSource, core), "Acme.Shadcn");
        var controls = Reference(CreatePackage("Acme.Controls", TwoControlsSource, core), "Acme.Controls");
        var compilation = Run(CreatePackage("Acme.Shadcn.Values", "using AtomUI.Theme.Languages;\n" + rules, core, shadcn, controls),
            out var diagnostics);
        return (compilation, diagnostics.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray());
    }

    private static string ToolTip(string body) => $$"""
        namespace Acme.Shadcn.Values
        {
            [ControlTokenRule]
            internal sealed class ToolTipTokenRule : IControlTokenRule<Acme.Shadcn.ShadcnToken, Acme.Controls.ToolTipToken>
            {
                public void Apply(in TokenEvaluationContext<Acme.Shadcn.ShadcnToken> context, ControlTokenTarget<Acme.Controls.ToolTipToken> target)
                {
                    {{body}}
                }
            }
        }

        """;

    private static string Popover(string body) => $$"""
        namespace Acme.Shadcn.Values
        {
            [ControlTokenRule]
            internal sealed class ShadcnPopoverTokenRule : IControlTokenRule<Acme.Shadcn.ShadcnToken, Acme.Controls.PopoverToken>
            {
                public void Apply(in TokenEvaluationContext<Acme.Shadcn.ShadcnToken> context, ControlTokenTarget<Acme.Controls.PopoverToken> target)
                {
                    {{body}}
                }
            }
        }

        """;
}
