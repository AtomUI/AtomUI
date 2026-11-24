using AtomUI.Generator.DesignLanguages;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using static AtomUI.Generator.Tests.DesignLanguages.DesignLanguageTestHost;

namespace AtomUI.Generator.Tests.DesignLanguages;

/// <summary>
/// The value package outputs: the generated <c>IControlToken</c> half of every terminal control token bound to a
/// control, and <c>GeneratedControlTokenRuleRegistration.Register</c>, which hands one
/// <c>ControlTokenRuleDescriptor.Create&lt;TLanguageToken, TToken, TRule&gt;()</c> per <c>[ControlTokenRule]</c>
/// class to the builder. Token types are covered both when they are declared in the same compilation (their
/// <c>IControlToken</c> half is not visible to the rule generator yet) and when they come from a referenced
/// control package built from real generator output.
/// </summary>
public class ControlTokenRuleRegistrationGeneratorTests
{
    private const string DiagnosticId = "ATOMUITOKEN002";
    private const string ControlTokenInterface = "AtomUI.Theme.DesignTokens.IControlToken";
    private const string CreatePrefix = "global::AtomUI.Theme.Schema.ControlTokenRuleDescriptor.Create<";

    // Two terminal tokens bound to controls and an abstract layer they share.
    private const string BoundControlsSource = """
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
            public abstract partial class AbstractPopupToken : AbstractControlDesignToken
            {
                public partial double Elevation { get; set; }
            }

            [ControlDesignToken]
            public sealed partial class ToolTipToken : AbstractPopupToken
            {
                public partial double Padding { get; set; }
            }

            [ControlDesignToken]
            public sealed partial class PopoverToken : AbstractPopupToken
            {
                public partial double ArrowSize { get; set; }
            }
        }
        """;

    // The bound tokens plus a terminal token that matches no control.
    private const string RichControlsSource = BoundControlsSource + """

        namespace Acme.Controls
        {
            [AtomUI.Theme.DesignTokens.ControlDesignToken]
            public sealed partial class OrphanToken : AtomUI.Theme.DesignTokens.AbstractControlDesignToken
            {
                public partial double Width { get; set; }
            }
        }
        """;

    [Fact]
    public void Generated_Terminal_Token_Implements_IControlToken_With_Its_Control_Identity()
    {
        var controls = Run(CreatePackage("Acme.Controls", RichControlsSource, CoreReference()), out var diagnostics);

        // OrphanToken matches no control named Orphan, so it stays unbound.
        diagnostics.Where(static d => d.Severity == DiagnosticSeverity.Error).ShouldHaveSingleItem()
                   .GetMessage(null).ShouldContain("OrphanToken");
        Errors(controls).ShouldBeEmpty();
        var source = GetGeneratedSource(controls, IdentitiesFile);
        source.ShouldContain("partial class ToolTipToken : global::AtomUI.Theme.DesignTokens.IControlToken");
        source.ShouldContain(
            "static global::AtomUI.Theme.Schema.ControlTokenIdentity global::AtomUI.Theme.DesignTokens.IControlToken.ControlIdentity => global::Acme.Controls.DesignTokens.ToolTipTokens.Identity;");
        source.ShouldContain(
            "static global::AtomUI.Theme.Schema.ControlTokenIdentity global::AtomUI.Theme.DesignTokens.IControlToken.ControlIdentity => global::Acme.Controls.DesignTokens.PopoverTokens.Identity;");
        // Exactly once per bound terminal token, and never for the abstract layer or the unbound token.
        CountOf(source, "IControlToken.ControlIdentity").ShouldBe(2);
        source.ShouldNotContain("OrphanToken");
        source.ShouldNotContain("AbstractPopupToken");

        Implements(controls, "Acme.Controls.ToolTipToken").ShouldBeTrue();
        Implements(controls, "Acme.Controls.PopoverToken").ShouldBeTrue();
        Implements(controls, "Acme.Controls.OrphanToken").ShouldBeFalse();
        Implements(controls, "Acme.Controls.AbstractPopupToken").ShouldBeFalse();
        // A control package without rules gets no rule registration.
        HasGeneratedSource(controls, TokenRuleRegistrationFile).ShouldBeFalse();
    }

    [Fact]
    public void A_Runtime_Without_IControlToken_Gets_No_Identity_Half()
    {
        var legacyStubs = RuntimeStubs
            .Replace("""
                public interface IControlToken
                {
                    static abstract AtomUI.Theme.Schema.ControlTokenIdentity ControlIdentity { get; }
                }
            """, "", StringComparison.Ordinal)
            .Replace(", AtomUI.Theme.DesignTokens.IControlToken", "", StringComparison.Ordinal);
        System.Text.RegularExpressions.Regex.IsMatch(legacyStubs, @"\bIControlToken\b").ShouldBeFalse();
        var legacyCore = CSharpCompilation.Create("AtomUI.Core",
            [CSharpSyntaxTree.ParseText(legacyStubs, new CSharpParseOptions(LanguageVersion.Preview), "RuntimeStubs.cs",
                cancellationToken: TestContext.Current.CancellationToken),
             CSharpSyntaxTree.ParseText(CoreTokenSource, new CSharpParseOptions(LanguageVersion.Preview), "GlobalToken.cs",
                cancellationToken: TestContext.Current.CancellationToken)],
            PlatformReferences(), new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var controls = Run(CreatePackage("Acme.Controls", ControlsSource, Reference(legacyCore, "legacy core")), out _);

        HasGeneratedSource(controls, IdentitiesFile).ShouldBeFalse();
        Errors(controls).ShouldBeEmpty();
    }

    [Fact]
    public void Emits_One_Create_Call_Per_Token_Rule_For_Local_Control_Tokens()
    {
        // Controls, tokens and rules in one compilation: the tokens get their IControlToken half from the token
        // generator in the same run, so the rule generator accepts them by their [ControlDesignToken] mark.
        var source = BoundControlsSource + """

            namespace Acme.Controls.Values
            {
                using AtomUI.Theme.DesignTokens;
                using AtomUI.Theme.Languages;
                using Acme.Controls;
                using Acme.Shadcn;

                // Declared out of metadata-name order on purpose: the registration sorts by rule type.
                [ControlTokenRule]
                public sealed class ToolTipTokenRule : IControlTokenRule<ShadcnToken, ToolTipToken>
                {
                    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<ToolTipToken> target)
                    {
                        target.Own.Padding = 4;
                        target.Own.Elevation = 1;
                    }
                }

                [ControlTokenRule]
                internal sealed class GenericToolTipTokenRule : IControlTokenRule<LanguageDesignToken, ToolTipToken>
                {
                    public void Apply(in TokenEvaluationContext<LanguageDesignToken> context, ControlTokenTarget<ToolTipToken> target)
                    {
                        target.Own.Padding = 4;
                    }
                }

                [ControlTokenRule]
                internal sealed class ShadcnPopoverTokenRule : IControlTokenRule<ShadcnToken, PopoverToken>
                {
                    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<PopoverToken> target)
                    {
                        target.Own.ArrowSize = 8;
                    }
                }
            }
            """;
        source.ShouldNotContain("OrphanToken");
        var core = CoreReference();
        var shadcn = Reference(CreatePackage("Acme.Shadcn", LanguageSource, core), "Acme.Shadcn");

        var package = Run(CreatePackage("Acme.Controls", source, core, shadcn), out var diagnostics);

        // A control package that references a language package breaks the dependency direction (spec 3.3); the
        // registration is still emitted so the rest of the build reports its own errors.
        diagnostics.Where(static d => d.Severity == DiagnosticSeverity.Error).ShouldAllBe(d => d.Id == "ATOMUIGEN053");
        // The Create<> constraints require IControlToken: the emitted calls bind against the same-run identity halves.
        Errors(package).ShouldBeEmpty();
        var registration = GetGeneratedSource(package, TokenRuleRegistrationFile);
        registration.ShouldContain("namespace AtomUI.Generated.AcmeControls;");
        CountOf(registration, "public static class GeneratedControlTokenRuleRegistration").ShouldBe(1);
        registration.ShouldContain("public static void Register(global::AtomUI.Theme.IThemeManagerBuilder builder)");
        registration.ShouldContain("global::System.ArgumentNullException.ThrowIfNull(builder);");
        registration.ShouldContain("builder.AddControlTokenRules(s_tokenRules);");
        registration.ShouldContain("private static readonly global::AtomUI.Theme.Schema.ControlTokenRuleDescriptor[] s_tokenRules =");
        CountOf(registration, CreatePrefix).ShouldBe(3);
        var generic = registration.IndexOf(
            CreatePrefix + "global::AtomUI.Theme.DesignTokens.LanguageDesignToken, global::Acme.Controls.ToolTipToken, global::Acme.Controls.Values.GenericToolTipTokenRule>(),",
            StringComparison.Ordinal);
        var popover = registration.IndexOf(
            CreatePrefix + "global::Acme.Shadcn.ShadcnToken, global::Acme.Controls.PopoverToken, global::Acme.Controls.Values.ShadcnPopoverTokenRule>(),",
            StringComparison.Ordinal);
        var toolTip = registration.IndexOf(
            CreatePrefix + "global::Acme.Shadcn.ShadcnToken, global::Acme.Controls.ToolTipToken, global::Acme.Controls.Values.ToolTipTokenRule>(),",
            StringComparison.Ordinal);
        generic.ShouldBeGreaterThan(0);
        popover.ShouldBeGreaterThan(generic);
        toolTip.ShouldBeGreaterThan(popover);
        registration.ShouldNotContain("Activator");
        registration.ShouldNotContain("typeof(");
    }

    [Fact]
    public void Emits_Registration_For_A_Value_Package_That_References_The_Control_And_Language_Packages()
    {
        var core = CoreReference();
        var shadcn = Reference(CreatePackage("Acme.Shadcn", LanguageSource, core), "Acme.Shadcn");
        var controls = Reference(CreatePackage("Acme.Controls", ControlsSource, core), "Acme.Controls");

        var package = Run(CreatePackage("Acme.Shadcn.Values", ValueSource, core, shadcn, controls), out var diagnostics);

        diagnostics.ShouldBeEmpty();
        Errors(package).ShouldBeEmpty();
        var registration = GetGeneratedSource(package, TokenRuleRegistrationFile);
        registration.ShouldContain("namespace AtomUI.Generated.AcmeShadcnValues;");
        CountOf(registration, CreatePrefix).ShouldBe(1);
        registration.ShouldContain(
            CreatePrefix + "global::Acme.Shadcn.ShadcnToken, global::Acme.Controls.ToolTipToken, global::Acme.Shadcn.Values.ToolTipTokenRule>(),");
        // A value package declares no tokens of its own.
        HasGeneratedSource(package, IdentitiesFile).ShouldBeFalse();
        HasGeneratedSource(package, LanguageRegistrationFile).ShouldBeFalse();
    }

    [Fact]
    public void Token_Rule_Generator_Alone_Accepts_Referenced_Bound_Tokens()
    {
        var core = CoreReference();
        var shadcn = Reference(CreatePackage("Acme.Shadcn", LanguageSource, core), "Acme.Shadcn");
        var controls = Reference(CreatePackage("Acme.Controls", ControlsSource, core), "Acme.Controls");

        var package = Run(CreatePackage("Acme.Shadcn.Values", ValueSource, core, shadcn, controls),
            [new ControlTokenRuleRegistrationGenerator()], out var diagnostics);

        diagnostics.ShouldBeEmpty();
        Errors(package).ShouldBeEmpty();
        CountOf(GetGeneratedSource(package, TokenRuleRegistrationFile), CreatePrefix).ShouldBe(1);
    }

    [Fact]
    public void Reports_A_Token_Rule_Whose_Token_Is_Not_Bound_To_A_Control()
    {
        var core = CoreReference();
        var shadcn = Reference(CreatePackage("Acme.Shadcn", LanguageSource, core), "Acme.Shadcn");
        var controlsOutput = Run(CreatePackage("Acme.Controls", RichControlsSource, core), out _);
        var controls = controlsOutput.ToMetadataReference();
        // A referenced token that matches no control, a referenced abstract layer, and a local abstract layer.
        var source = """
            using AtomUI.Theme.DesignTokens;
            using AtomUI.Theme.Languages;
            using Acme.Shadcn;

            namespace Acme.Shadcn.Values
            {
                [ControlDesignToken]
                public abstract partial class LocalLayerToken : AbstractControlDesignToken
                {
                    public partial double Width { get; set; }
                }

                [ControlTokenRule]
                internal sealed class LocalLayerTokenRule : IControlTokenRule<ShadcnToken, LocalLayerToken>
                {
                    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<LocalLayerToken> target) { }
                }

                [ControlTokenRule]
                internal sealed class OrphanTokenRule : IControlTokenRule<ShadcnToken, Acme.Controls.OrphanToken>
                {
                    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<Acme.Controls.OrphanToken> target) { }
                }

                [ControlTokenRule]
                internal sealed class PopupTokenRule : IControlTokenRule<ShadcnToken, Acme.Controls.AbstractPopupToken>
                {
                    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<Acme.Controls.AbstractPopupToken> target) { }
                }
            }
            """;

        var package = Run(CreatePackage("Acme.Shadcn.Values", source, core, shadcn, controls), out var diagnostics);

        var failures = diagnostics.Where(static d => d.Id == DiagnosticId).ToArray();
        failures.Select(static d => d.GetMessage(null)).ShouldBe([
            "'Acme.Shadcn.Values.LocalLayerTokenRule' writes 'Acme.Shadcn.Values.LocalLayerToken', which is not a control token bound to a control.",
            "'Acme.Shadcn.Values.OrphanTokenRule' writes 'Acme.Controls.OrphanToken', which is not a control token bound to a control.",
            "'Acme.Shadcn.Values.PopupTokenRule' writes 'Acme.Controls.AbstractPopupToken', which is not a control token bound to a control."
        ], ignoreOrder: true);
        failures.ShouldAllBe(static d => d.Location.GetLineSpan().Path == "Acme.Shadcn.Values.cs");
        failures.ShouldAllBe(static d => d.Severity == DiagnosticSeverity.Error);
        HasGeneratedSource(package, TokenRuleRegistrationFile).ShouldBeFalse();
    }

    // Suspected generator bug: a local [ControlDesignToken] terminal class counts as bound without checking that it
    // matches a control, so the generator emits a Create<> call that fails the IControlToken constraint instead of
    // reporting the rule.
    [Fact]
    public void Reports_A_Token_Rule_Of_A_Local_Token_That_Matches_No_Control()
    {
        var core = CoreReference();
        var shadcn = Reference(CreatePackage("Acme.Shadcn", LanguageSource, core), "Acme.Shadcn");
        var source = """
            using AtomUI.Theme.DesignTokens;
            using AtomUI.Theme.Languages;
            using Acme.Shadcn;

            namespace Acme.Shadcn.Values
            {
                [ControlDesignToken]
                public sealed partial class LocalOrphanToken : AbstractControlDesignToken
                {
                    public partial double Width { get; set; }
                }

                [ControlTokenRule]
                internal sealed class LocalOrphanTokenRule : IControlTokenRule<ShadcnToken, LocalOrphanToken>
                {
                    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<LocalOrphanToken> target) { }
                }
            }
            """;

        var package = Run(CreatePackage("Acme.Shadcn.Values", source, core, shadcn), out var diagnostics);

        diagnostics.ShouldContain(static d => d.Id == DiagnosticId && d.GetMessage(null) ==
            "'Acme.Shadcn.Values.LocalOrphanTokenRule' writes 'Acme.Shadcn.Values.LocalOrphanToken', which is not a control token bound to a control.");
        HasGeneratedSource(package, TokenRuleRegistrationFile).ShouldBeFalse();
        Errors(package).ShouldBeEmpty();
    }

    [Theory]
    [InlineData("public ToolTipTokenRule(int scale) { }", "internal sealed class")]
    [InlineData("private ToolTipTokenRule() { }", "internal sealed class")]
    [InlineData("", "internal abstract class")]
    public void Reports_A_Token_Rule_That_Cannot_Be_Instantiated(string constructor, string declaration)
    {
        var source = ValueSource
            .Replace("internal sealed class ToolTipTokenRule", declaration + " ToolTipTokenRule", StringComparison.Ordinal)
            .Replace("public void Apply(", constructor + "\n        public void Apply(", StringComparison.Ordinal);

        var package = Run(CreatePackage("Acme.Shadcn.Values", source, CoreAndPackages()), out var diagnostics);

        var failure = diagnostics.Where(static d => d.Id == DiagnosticId).ShouldHaveSingleItem();
        failure.GetMessage(null).ShouldBe(
            "'Acme.Shadcn.Values.ToolTipTokenRule' must be a non-abstract, non-generic class with a public parameterless constructor, accessible within its assembly.");
        failure.Location.GetLineSpan().Path.ShouldBe("Acme.Shadcn.Values.cs");
        HasGeneratedSource(package, TokenRuleRegistrationFile).ShouldBeFalse();
    }

    [Theory]
    [InlineData("internal sealed class Holder { [ControlTokenRule] private sealed class HiddenTokenRule : IControlTokenRule<Acme.Shadcn.ShadcnToken, Acme.Controls.ToolTipToken> { public void Apply(in TokenEvaluationContext<Acme.Shadcn.ShadcnToken> context, ControlTokenTarget<Acme.Controls.ToolTipToken> target) { } } }",
        "Acme.Shadcn.Values.Holder.HiddenTokenRule")]
    [InlineData("[ControlTokenRule] internal sealed class GenericTokenRule<T> : IControlTokenRule<Acme.Shadcn.ShadcnToken, Acme.Controls.ToolTipToken> { public void Apply(in TokenEvaluationContext<Acme.Shadcn.ShadcnToken> context, ControlTokenTarget<Acme.Controls.ToolTipToken> target) { } }",
        "Acme.Shadcn.Values.GenericTokenRule<T>")]
    public void Reports_A_Token_Rule_That_Is_Generic_Or_Not_Accessible_Within_Its_Assembly(string declaration, string name)
    {
        var source = $$"""
            using AtomUI.Theme.Languages;

            namespace Acme.Shadcn.Values
            {
                {{declaration}}
            }
            """;

        var package = Run(CreatePackage("Acme.Shadcn.Values", source, CoreAndPackages()), out var diagnostics);

        diagnostics.Where(static d => d.Id == DiagnosticId).ShouldHaveSingleItem().GetMessage(null).ShouldBe(
            $"'{name}' must be a non-abstract, non-generic class with a public parameterless constructor, accessible within its assembly.");
        HasGeneratedSource(package, TokenRuleRegistrationFile).ShouldBeFalse();
    }

    [Fact]
    public void Reports_A_Token_Rule_That_Does_Not_Implement_IControlTokenRule_Exactly_Once()
    {
        var source = """
            using AtomUI.Theme.Languages;
            using Acme.Shadcn;

            namespace Acme.Shadcn.Values
            {
                [ControlTokenRule]
                internal sealed class NoTokenRule
                {
                }

                [ControlTokenRule]
                internal sealed class TwoTokenRules :
                    IControlTokenRule<ShadcnToken, Acme.Controls.ToolTipToken>,
                    IControlTokenRule<AtomUI.Theme.DesignTokens.LanguageDesignToken, Acme.Controls.ToolTipToken>
                {
                    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<Acme.Controls.ToolTipToken> target) { }
                    public void Apply(in TokenEvaluationContext<AtomUI.Theme.DesignTokens.LanguageDesignToken> context, ControlTokenTarget<Acme.Controls.ToolTipToken> target) { }
                }
            }
            """;

        var package = Run(CreatePackage("Acme.Shadcn.Values", source, CoreAndPackages()), out var diagnostics);

        diagnostics.Where(static d => d.Id == DiagnosticId).Select(static d => d.GetMessage(null)).ShouldBe([
            "'Acme.Shadcn.Values.NoTokenRule' must implement IControlTokenRule<TLanguageToken, TToken> exactly once.",
            "'Acme.Shadcn.Values.TwoTokenRules' must implement IControlTokenRule<TLanguageToken, TToken> exactly once."
        ], ignoreOrder: true);
        HasGeneratedSource(package, TokenRuleRegistrationFile).ShouldBeFalse();
    }

    [Fact]
    public void Reports_A_Second_Token_Rule_Of_The_Same_Token_For_The_Same_Language()
    {
        var source = ValueSource + """

            namespace Acme.Shadcn.Values
            {
                using AtomUI.Theme.Languages;

                [ControlTokenRule]
                internal sealed class AnotherToolTipTokenRule : IControlTokenRule<Acme.Shadcn.ShadcnToken, Acme.Controls.ToolTipToken>
                {
                    public void Apply(in TokenEvaluationContext<Acme.Shadcn.ShadcnToken> context, ControlTokenTarget<Acme.Controls.ToolTipToken> target) { }
                }
            }
            """;

        var package = Run(CreatePackage("Acme.Shadcn.Values", source, CoreAndPackages()), out var diagnostics);

        // The first one in metadata-name order is kept; the other is reported.
        diagnostics.Where(static d => d.Id == DiagnosticId).ShouldHaveSingleItem().GetMessage(null).ShouldBe(
            "'Acme.Shadcn.Values.ToolTipTokenRule' and 'Acme.Shadcn.Values.AnotherToolTipTokenRule' both register a token rule for 'Acme.Controls.ToolTipToken' under 'Acme.Shadcn.ShadcnToken'.");
        var registration = GetGeneratedSource(package, TokenRuleRegistrationFile);
        CountOf(registration, CreatePrefix).ShouldBe(1);
        registration.ShouldContain("global::Acme.Shadcn.Values.AnotherToolTipTokenRule>()");
        registration.ShouldNotContain("Acme.Shadcn.Values.ToolTipTokenRule>");
        Errors(package).ShouldBeEmpty();
    }

    [Fact]
    public void Emits_Nothing_When_The_Runtime_Has_No_Token_Rule_Contract()
    {
        var stubs = RuntimeStubs;
        var start = stubs.IndexOf("    public interface IControlTokenRule<TLanguageToken, TToken>", StringComparison.Ordinal);
        var end = stubs.IndexOf("}\n}", start, StringComparison.Ordinal) + 2;
        start.ShouldBeGreaterThan(0);
        var oldCore = CSharpCompilation.Create("AtomUI.Core",
            [CSharpSyntaxTree.ParseText(stubs.Remove(start, end - start), new CSharpParseOptions(LanguageVersion.Preview),
                cancellationToken: TestContext.Current.CancellationToken)],
            PlatformReferences(), new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        oldCore.GetTypeByMetadataName("AtomUI.Theme.Languages.IControlTokenRule`2").ShouldBeNull();
        var source = """
            namespace Acme.Shadcn.Values
            {
                [AtomUI.Theme.Languages.ControlTokenRule]
                internal sealed class Unrelated
                {
                }
            }
            """;

        var package = Run(CreatePackage("Acme.Shadcn.Values", source, oldCore.ToMetadataReference()),
            [new ControlTokenRuleRegistrationGenerator()], out var diagnostics);

        diagnostics.ShouldBeEmpty();
        HasGeneratedSource(package, TokenRuleRegistrationFile).ShouldBeFalse();
    }

    private const string ValueSource = """
        using AtomUI.Theme.Languages;

        namespace Acme.Shadcn.Values
        {
            [ControlTokenRule]
            internal sealed class ToolTipTokenRule : IControlTokenRule<Acme.Shadcn.ShadcnToken, Acme.Controls.ToolTipToken>
            {
                public void Apply(in TokenEvaluationContext<Acme.Shadcn.ShadcnToken> context, ControlTokenTarget<Acme.Controls.ToolTipToken> target) =>
                    target.Own.Padding = 4;
            }
        }
        """;

    private static MetadataReference[] CoreAndPackages()
    {
        var core = CoreReference();
        return
        [
            core,
            Reference(CreatePackage("Acme.Shadcn", LanguageSource, core), "Acme.Shadcn"),
            Reference(CreatePackage("Acme.Controls", ControlsSource, core), "Acme.Controls")
        ];
    }

    private static bool Implements(CSharpCompilation compilation, string tokenMetadataName) =>
        compilation.GetTypeByMetadataName(tokenMetadataName)!.AllInterfaces
                   .Any(static type => type.ToDisplayString() == ControlTokenInterface);
}
