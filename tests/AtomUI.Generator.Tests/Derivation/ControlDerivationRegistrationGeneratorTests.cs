using AtomUI.Generator.Derivation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using static AtomUI.Generator.Tests.Derivation.DesignLanguageTestHost;

namespace AtomUI.Generator.Tests.Derivation;

/// <summary>
/// The value package outputs: the generated <c>IControlToken</c> half of every terminal control token bound to a
/// control, and <c>GeneratedControlDerivationRegistration.Register</c>, which hands one
/// <c>ControlDerivationDescriptor.Create&lt;TLanguageToken, TToken, TDerivation&gt;()</c> per <c>[ControlDerivation]</c>
/// class to the builder. Token types are covered both when they are declared in the same compilation (their
/// <c>IControlToken</c> half is not visible to the derivation generator yet) and when they come from a referenced
/// control package built from real generator output.
/// </summary>
public class ControlDerivationRegistrationGeneratorTests
{
    private const string DiagnosticId = "ATOMUITOKEN002";
    private const string ControlTokenInterface = "AtomUI.Theme.DesignTokens.IControlToken";
    private const string CreatePrefix = "global::AtomUI.Theme.Schema.ControlDerivationDescriptor.Create<";

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
        // A control package without derivations gets no derivation registration.
        HasGeneratedSource(controls, DerivationRegistrationFile).ShouldBeFalse();
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
        legacyStubs.ShouldNotContain("IControlToken");
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
    public void Emits_One_Create_Call_Per_Derivation_For_Local_Control_Tokens()
    {
        // Controls, tokens and derivations in one compilation: the tokens get their IControlToken half from the token
        // generator in the same run, so the derivation generator accepts them by their [ControlDesignToken] mark.
        var source = BoundControlsSource + """

            namespace Acme.Controls.Values
            {
                using AtomUI.Theme.DesignTokens;
                using AtomUI.Theme.Languages;
                using Acme.Controls;
                using Acme.Shadcn;

                // Declared out of metadata-name order on purpose: the registration sorts by derivation type.
                [ControlDerivation]
                public sealed class ShadcnToolTipDerivation : IControlDerivation<ShadcnToken, ToolTipToken>
                {
                    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<ToolTipToken> view)
                    {
                        view.Token.Padding = 4;
                        view.Token.Elevation = 1;
                    }
                }

                [ControlDerivation]
                internal sealed class GenericToolTipDerivation : IControlDerivation<LanguageDesignToken, ToolTipToken>
                {
                    public void Derive(in DerivationContext<LanguageDesignToken> context, ControlView<ToolTipToken> view)
                    {
                        view.Token.Padding = 4;
                    }
                }

                [ControlDerivation]
                internal sealed class ShadcnPopoverDerivation : IControlDerivation<ShadcnToken, PopoverToken>
                {
                    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<PopoverToken> view)
                    {
                        view.Token.ArrowSize = 8;
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
        var registration = GetGeneratedSource(package, DerivationRegistrationFile);
        registration.ShouldContain("namespace AtomUI.Generated.AcmeControls;");
        registration.ShouldContain("public static class GeneratedControlDerivationRegistration");
        registration.ShouldContain("public static void Register(global::AtomUI.Theme.IThemeManagerBuilder builder)");
        registration.ShouldContain("global::System.ArgumentNullException.ThrowIfNull(builder);");
        registration.ShouldContain("builder.AddControlDerivations(s_derivations);");
        registration.ShouldContain("private static readonly global::AtomUI.Theme.Schema.ControlDerivationDescriptor[] s_derivations =");
        CountOf(registration, CreatePrefix).ShouldBe(3);
        var generic = registration.IndexOf(
            CreatePrefix + "global::AtomUI.Theme.DesignTokens.LanguageDesignToken, global::Acme.Controls.ToolTipToken, global::Acme.Controls.Values.GenericToolTipDerivation>(),",
            StringComparison.Ordinal);
        var popover = registration.IndexOf(
            CreatePrefix + "global::Acme.Shadcn.ShadcnToken, global::Acme.Controls.PopoverToken, global::Acme.Controls.Values.ShadcnPopoverDerivation>(),",
            StringComparison.Ordinal);
        var toolTip = registration.IndexOf(
            CreatePrefix + "global::Acme.Shadcn.ShadcnToken, global::Acme.Controls.ToolTipToken, global::Acme.Controls.Values.ShadcnToolTipDerivation>(),",
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
        var registration = GetGeneratedSource(package, DerivationRegistrationFile);
        registration.ShouldContain("namespace AtomUI.Generated.AcmeShadcnValues;");
        CountOf(registration, CreatePrefix).ShouldBe(1);
        registration.ShouldContain(
            CreatePrefix + "global::Acme.Shadcn.ShadcnToken, global::Acme.Controls.ToolTipToken, global::Acme.Shadcn.Values.ShadcnToolTipDerivation>(),");
        // A value package declares no tokens of its own.
        HasGeneratedSource(package, IdentitiesFile).ShouldBeFalse();
        HasGeneratedSource(package, LanguageRegistrationFile).ShouldBeFalse();
    }

    [Fact]
    public void Derivation_Generator_Alone_Accepts_Referenced_Bound_Tokens()
    {
        var core = CoreReference();
        var shadcn = Reference(CreatePackage("Acme.Shadcn", LanguageSource, core), "Acme.Shadcn");
        var controls = Reference(CreatePackage("Acme.Controls", ControlsSource, core), "Acme.Controls");

        var package = Run(CreatePackage("Acme.Shadcn.Values", ValueSource, core, shadcn, controls),
            [new ControlDerivationRegistrationGenerator()], out var diagnostics);

        diagnostics.ShouldBeEmpty();
        Errors(package).ShouldBeEmpty();
        CountOf(GetGeneratedSource(package, DerivationRegistrationFile), CreatePrefix).ShouldBe(1);
    }

    [Fact]
    public void Reports_A_Derivation_Whose_Token_Is_Not_Bound_To_A_Control()
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

                [ControlDerivation]
                internal sealed class LocalLayerDerivation : IControlDerivation<ShadcnToken, LocalLayerToken>
                {
                    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<LocalLayerToken> view) { }
                }

                [ControlDerivation]
                internal sealed class OrphanDerivation : IControlDerivation<ShadcnToken, Acme.Controls.OrphanToken>
                {
                    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<Acme.Controls.OrphanToken> view) { }
                }

                [ControlDerivation]
                internal sealed class PopupDerivation : IControlDerivation<ShadcnToken, Acme.Controls.AbstractPopupToken>
                {
                    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<Acme.Controls.AbstractPopupToken> view) { }
                }
            }
            """;

        var package = Run(CreatePackage("Acme.Shadcn.Values", source, core, shadcn, controls), out var diagnostics);

        var failures = diagnostics.Where(static d => d.Id == DiagnosticId).ToArray();
        failures.Select(static d => d.GetMessage(null)).ShouldBe([
            "'Acme.Shadcn.Values.LocalLayerDerivation' derives 'Acme.Shadcn.Values.LocalLayerToken', which is not a control token bound to a control.",
            "'Acme.Shadcn.Values.OrphanDerivation' derives 'Acme.Controls.OrphanToken', which is not a control token bound to a control.",
            "'Acme.Shadcn.Values.PopupDerivation' derives 'Acme.Controls.AbstractPopupToken', which is not a control token bound to a control."
        ], ignoreOrder: true);
        failures.ShouldAllBe(static d => d.Location.GetLineSpan().Path == "Acme.Shadcn.Values.cs");
        failures.ShouldAllBe(static d => d.Severity == DiagnosticSeverity.Error);
        HasGeneratedSource(package, DerivationRegistrationFile).ShouldBeFalse();
    }

    // Suspected generator bug: a local [ControlDesignToken] terminal class counts as bound without checking that it
    // matches a control, so the generator emits a Create<> call that fails the IControlToken constraint instead of
    // reporting the derivation.
    [Fact]
    public void Reports_A_Derivation_Of_A_Local_Token_That_Matches_No_Control()
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

                [ControlDerivation]
                internal sealed class LocalOrphanDerivation : IControlDerivation<ShadcnToken, LocalOrphanToken>
                {
                    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<LocalOrphanToken> view) { }
                }
            }
            """;

        var package = Run(CreatePackage("Acme.Shadcn.Values", source, core, shadcn), out var diagnostics);

        diagnostics.ShouldContain(static d => d.Id == DiagnosticId && d.GetMessage(null) ==
            "'Acme.Shadcn.Values.LocalOrphanDerivation' derives 'Acme.Shadcn.Values.LocalOrphanToken', which is not a control token bound to a control.");
        HasGeneratedSource(package, DerivationRegistrationFile).ShouldBeFalse();
        Errors(package).ShouldBeEmpty();
    }

    [Theory]
    [InlineData("public ShadcnToolTipDerivation(int scale) { }", "internal sealed class")]
    [InlineData("private ShadcnToolTipDerivation() { }", "internal sealed class")]
    [InlineData("", "internal abstract class")]
    public void Reports_A_Derivation_That_Cannot_Be_Instantiated(string constructor, string declaration)
    {
        var source = ValueSource
            .Replace("internal sealed class ShadcnToolTipDerivation", declaration + " ShadcnToolTipDerivation", StringComparison.Ordinal)
            .Replace("public void Derive(", constructor + "\n        public void Derive(", StringComparison.Ordinal);

        var package = Run(CreatePackage("Acme.Shadcn.Values", source, CoreAndPackages()), out var diagnostics);

        var failure = diagnostics.Where(static d => d.Id == DiagnosticId).ShouldHaveSingleItem();
        failure.GetMessage(null).ShouldBe(
            "'Acme.Shadcn.Values.ShadcnToolTipDerivation' must be a non-abstract, non-generic class with a public parameterless constructor, accessible within its assembly.");
        failure.Location.GetLineSpan().Path.ShouldBe("Acme.Shadcn.Values.cs");
        HasGeneratedSource(package, DerivationRegistrationFile).ShouldBeFalse();
    }

    [Theory]
    [InlineData("internal sealed class Holder { [ControlDerivation] private sealed class HiddenDerivation : IControlDerivation<Acme.Shadcn.ShadcnToken, Acme.Controls.ToolTipToken> { public void Derive(in DerivationContext<Acme.Shadcn.ShadcnToken> context, ControlView<Acme.Controls.ToolTipToken> view) { } } }",
        "Acme.Shadcn.Values.Holder.HiddenDerivation")]
    [InlineData("[ControlDerivation] internal sealed class GenericDerivation<T> : IControlDerivation<Acme.Shadcn.ShadcnToken, Acme.Controls.ToolTipToken> { public void Derive(in DerivationContext<Acme.Shadcn.ShadcnToken> context, ControlView<Acme.Controls.ToolTipToken> view) { } }",
        "Acme.Shadcn.Values.GenericDerivation<T>")]
    public void Reports_A_Derivation_That_Is_Generic_Or_Not_Accessible_Within_Its_Assembly(string declaration, string name)
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
        HasGeneratedSource(package, DerivationRegistrationFile).ShouldBeFalse();
    }

    [Fact]
    public void Reports_A_Derivation_That_Does_Not_Implement_IControlDerivation_Exactly_Once()
    {
        var source = """
            using AtomUI.Theme.Languages;
            using Acme.Shadcn;

            namespace Acme.Shadcn.Values
            {
                [ControlDerivation]
                internal sealed class NoDerivation
                {
                }

                [ControlDerivation]
                internal sealed class TwoDerivations :
                    IControlDerivation<ShadcnToken, Acme.Controls.ToolTipToken>,
                    IControlDerivation<AtomUI.Theme.DesignTokens.LanguageDesignToken, Acme.Controls.ToolTipToken>
                {
                    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<Acme.Controls.ToolTipToken> view) { }
                    public void Derive(in DerivationContext<AtomUI.Theme.DesignTokens.LanguageDesignToken> context, ControlView<Acme.Controls.ToolTipToken> view) { }
                }
            }
            """;

        var package = Run(CreatePackage("Acme.Shadcn.Values", source, CoreAndPackages()), out var diagnostics);

        diagnostics.Where(static d => d.Id == DiagnosticId).Select(static d => d.GetMessage(null)).ShouldBe([
            "'Acme.Shadcn.Values.NoDerivation' must implement IControlDerivation<TLanguageToken, TToken> exactly once.",
            "'Acme.Shadcn.Values.TwoDerivations' must implement IControlDerivation<TLanguageToken, TToken> exactly once."
        ], ignoreOrder: true);
        HasGeneratedSource(package, DerivationRegistrationFile).ShouldBeFalse();
    }

    [Fact]
    public void Reports_A_Second_Derivation_Of_The_Same_Token_For_The_Same_Language()
    {
        var source = ValueSource + """

            namespace Acme.Shadcn.Values
            {
                using AtomUI.Theme.Languages;

                [ControlDerivation]
                internal sealed class AnotherToolTipDerivation : IControlDerivation<Acme.Shadcn.ShadcnToken, Acme.Controls.ToolTipToken>
                {
                    public void Derive(in DerivationContext<Acme.Shadcn.ShadcnToken> context, ControlView<Acme.Controls.ToolTipToken> view) { }
                }
            }
            """;

        var package = Run(CreatePackage("Acme.Shadcn.Values", source, CoreAndPackages()), out var diagnostics);

        // The first one in metadata-name order is kept; the other is reported.
        diagnostics.Where(static d => d.Id == DiagnosticId).ShouldHaveSingleItem().GetMessage(null).ShouldBe(
            "'Acme.Shadcn.Values.ShadcnToolTipDerivation' and 'Acme.Shadcn.Values.AnotherToolTipDerivation' both derive 'Acme.Controls.ToolTipToken' for 'Acme.Shadcn.ShadcnToken'.");
        var registration = GetGeneratedSource(package, DerivationRegistrationFile);
        CountOf(registration, CreatePrefix).ShouldBe(1);
        registration.ShouldContain("global::Acme.Shadcn.Values.AnotherToolTipDerivation>()");
        registration.ShouldNotContain("ShadcnToolTipDerivation");
        Errors(package).ShouldBeEmpty();
    }

    [Fact]
    public void Emits_Nothing_When_The_Runtime_Has_No_Derivation_Contract()
    {
        var stubs = RuntimeStubs;
        var start = stubs.IndexOf("    public interface IControlDerivation<TLanguageToken, TToken>", StringComparison.Ordinal);
        var end = stubs.IndexOf("}\n}", start, StringComparison.Ordinal) + 2;
        start.ShouldBeGreaterThan(0);
        var oldCore = CSharpCompilation.Create("AtomUI.Core",
            [CSharpSyntaxTree.ParseText(stubs.Remove(start, end - start), new CSharpParseOptions(LanguageVersion.Preview),
                cancellationToken: TestContext.Current.CancellationToken)],
            PlatformReferences(), new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        oldCore.GetTypeByMetadataName("AtomUI.Theme.Languages.IControlDerivation`2").ShouldBeNull();
        var source = """
            namespace Acme.Shadcn.Values
            {
                [AtomUI.Theme.Languages.ControlDerivation]
                internal sealed class Unrelated
                {
                }
            }
            """;

        var package = Run(CreatePackage("Acme.Shadcn.Values", source, oldCore.ToMetadataReference()),
            [new ControlDerivationRegistrationGenerator()], out var diagnostics);

        diagnostics.ShouldBeEmpty();
        HasGeneratedSource(package, DerivationRegistrationFile).ShouldBeFalse();
    }

    private const string ValueSource = """
        using AtomUI.Theme.Languages;

        namespace Acme.Shadcn.Values
        {
            [ControlDerivation]
            internal sealed class ShadcnToolTipDerivation : IControlDerivation<Acme.Shadcn.ShadcnToken, Acme.Controls.ToolTipToken>
            {
                public void Derive(in DerivationContext<Acme.Shadcn.ShadcnToken> context, ControlView<Acme.Controls.ToolTipToken> view) =>
                    view.Token.Padding = 4;
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
