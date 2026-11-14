using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;

namespace AtomUI.Generator.Tests.ThemeSchema;

/// <summary>
/// The control token stage of a theme algorithm: the generated <c>IControlToken</c> half of every terminal control
/// token bound to a control, and the descriptors that <c>GeneratedThemeAlgorithmRegistration.Register</c> hands to
/// the builder for each <c>[ControlTokenAlgorithm]</c>. Token types are covered both when they are declared in the
/// same compilation (the interface is not visible to the generator yet) and when they come from a referenced
/// assembly built from real generator output (the interface is in metadata).
/// </summary>
public class ControlTokenAlgorithmGeneratorTests
{
    private const string RegistrationFile = "GeneratedThemeAlgorithmRegistration.g.cs";
    private const string IdentitiesFile = "GeneratedControlTokenIdentities.g.cs";
    private const string ControlTokenInterface = "AtomUI.Theme.DesignTokens.IControlToken";
    private const string CreatePrefix = "global::AtomUI.Theme.Schema.ControlTokenAlgorithmDescriptor.Create<";

    [Fact]
    public void Generated_Terminal_Token_Implements_IControlToken_With_Its_Control_Identity()
    {
        var controls = RunGenerator(CreateCompilation("Acme.Controls", ControlsSource, RuntimeReference()), out var diagnostics);

        // OrphanToken matches no control named Orphan, so it stays unbound.
        diagnostics.Where(static d => d.Severity == DiagnosticSeverity.Error).ShouldHaveSingleItem()
                   .GetMessage(null).ShouldContain("OrphanToken");
        Errors(controls).ShouldBeEmpty();
        var source = GetGeneratedSource(controls, IdentitiesFile);
        source.ShouldContain("partial class ToolTipToken : global::AtomUI.Theme.DesignTokens.IControlToken");
        source.ShouldContain(
            "static global::AtomUI.Theme.Schema.ControlTokenIdentity global::AtomUI.Theme.DesignTokens.IControlToken.ControlIdentity => global::Acme.Controls.DesignTokens.ToolTipTokens.Identity;");
        // Exactly once, and only for the bound terminal token.
        CountOf(source, "IControlToken.ControlIdentity").ShouldBe(1);
        source.ShouldNotContain("OrphanToken");
        source.ShouldNotContain("AbstractPopupToken");

        Implements(controls, "Acme.Controls.ToolTipToken").ShouldBeTrue();
        Implements(controls, "Acme.Controls.OrphanToken").ShouldBeFalse();
        Implements(controls, "Acme.Controls.AbstractPopupToken").ShouldBeFalse();

        // A compilation without the runtime interface (an older AtomUI.Core) gets no identity half.
        var legacy = RunGenerator(CreateCompilation("Acme.Controls", ControlsSource, RuntimeReference(withControlStage: false)), out _);
        legacy.SyntaxTrees.ShouldNotContain(tree => tree.FilePath.EndsWith(IdentitiesFile, StringComparison.Ordinal));
        Errors(legacy).ShouldBeEmpty();
    }

    [Fact]
    public void Emits_One_Descriptor_Per_ControlTokenAlgorithm_Attribute()
    {
        // Token, control, algorithms and the control stage all in one compilation.
        var package = RunGenerator(CreateCompilation("Acme.Themes", ControlsSource + LocalStageSource, RuntimeReference()),
            out var diagnostics);

        diagnostics.Where(static d => d.Id == "ATOMUITOKEN002").ShouldBeEmpty();
        Errors(package).ShouldBeEmpty();
        var registration = GetGeneratedSource(package, RegistrationFile);
        registration.ShouldContain("builder.AddThemeAlgorithms(GeneratedThemeSchema.GetAlgorithms());");
        registration.ShouldContain("builder.AddControlTokenAlgorithms(");
        CountOf(registration, CreatePrefix).ShouldBe(3);
        var aura = registration.IndexOf(
            CreatePrefix + "global::Acme.Controls.ToolTipToken, global::Acme.Themes.PrimeNGToolTipTokens>(new global::AtomUI.Theme.Algorithms.ThemeAlgorithm(\"PrimeNG\", \"Aura\"), 2)",
            StringComparison.Ordinal);
        var auraDark = registration.IndexOf(
            CreatePrefix + "global::Acme.Controls.ToolTipToken, global::Acme.Themes.PrimeNGToolTipTokens>(new global::AtomUI.Theme.Algorithms.ThemeAlgorithm(\"PrimeNG\", \"Aura.Dark\"), 1)",
            StringComparison.Ordinal);
        var lara = registration.IndexOf(
            CreatePrefix + "global::Acme.Controls.ToolTipToken, global::Acme.Themes.LaraToolTipTokens>(new global::AtomUI.Theme.Algorithms.ThemeAlgorithm(\"PrimeNG\", \"Lara\"), 1)",
            StringComparison.Ordinal);
        aura.ShouldBeGreaterThan(0);
        // Ordered by algorithm identity, independent of declaration order.
        auraDark.ShouldBeGreaterThan(aura);
        lara.ShouldBeGreaterThan(auraDark);
        registration.ShouldNotContain("Activator");
        registration.ShouldNotContain("typeof(");
    }

    [Fact]
    public void Emits_Registration_For_An_Assembly_With_Only_Control_Stage_Classes()
    {
        var runtime = RuntimeReference();
        var controls = RunGenerator(CreateCompilation("Acme.Controls", ControlsSource, runtime), out _).ToMetadataReference();

        var package = RunGenerator(CreateCompilation("Acme.Themes", ReferencedStageSource, runtime, controls), out var diagnostics);

        diagnostics.ShouldBeEmpty();
        Errors(package).ShouldBeEmpty();
        var registration = GetGeneratedSource(package, RegistrationFile);
        registration.ShouldContain("namespace AtomUI.Generated.AcmeThemes;");
        registration.ShouldContain("public static class GeneratedThemeAlgorithmRegistration");
        registration.ShouldContain("public static void Register(global::AtomUI.Theme.IThemeManagerBuilder builder)");
        registration.ShouldNotContain("AddThemeAlgorithms");
        CountOf(registration, CreatePrefix).ShouldBe(1);
        registration.ShouldContain(
            CreatePrefix + "global::Acme.Controls.ToolTipToken, global::Acme.Themes.PrimeNGToolTipTokens>(new global::AtomUI.Theme.Algorithms.ThemeAlgorithm(\"PrimeNG\", \"Aura\"), 1)");
        package.SyntaxTrees.ShouldNotContain(tree => tree.FilePath.EndsWith("PrimeNGAlgorithms.g.cs", StringComparison.Ordinal));
        package.SyntaxTrees.ShouldNotContain(tree => tree.FilePath.EndsWith(IdentitiesFile, StringComparison.Ordinal));
    }

    [Fact]
    public void Reports_A_Control_Stage_Class_Whose_Token_Type_Is_Not_A_Bound_Terminal_Token()
    {
        var runtime = RuntimeReference();
        var controls = RunGenerator(CreateCompilation("Acme.Controls", ControlsSource, runtime), out _).ToMetadataReference();
        // A local token that matches no control, a referenced token that matches no control, and an abstract layer.
        var source = """
            using AtomUI.Theme.Algorithms;
            using AtomUI.Theme.DesignTokens;

            namespace Acme.Themes
            {
                [ControlDesignToken]
                public sealed partial class LocalOrphanToken : AbstractControlDesignToken
                {
                    public double Width { get; set; }
                }

                [ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
                internal sealed class LocalOrphanTokens : IControlTokenAlgorithm<LocalOrphanToken>
                {
                    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal) { }
                    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, LocalOrphanToken token) { }
                }

                [ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
                internal sealed class OrphanTokens : IControlTokenAlgorithm<Acme.Controls.OrphanToken>
                {
                    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal) { }
                    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, Acme.Controls.OrphanToken token) { }
                }

                [ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
                internal sealed class PopupTokens : IControlTokenAlgorithm<Acme.Controls.AbstractPopupToken>
                {
                    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal) { }
                    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, Acme.Controls.AbstractPopupToken token) { }
                }
            }
            """;

        var package = RunGenerator(CreateCompilation("Acme.Themes", source, runtime, controls), out var diagnostics);

        var failures = diagnostics.Where(static d => d.Id == "ATOMUITOKEN002").ToArray();
        failures.Select(static d => d.GetMessage(null)).ShouldBe([
            "'Acme.Themes.LocalOrphanTokens' targets 'Acme.Themes.LocalOrphanToken', which is not bound to a control.",
            "'Acme.Themes.OrphanTokens' targets 'Acme.Controls.OrphanToken', which is not bound to a control.",
            "'Acme.Themes.PopupTokens' targets 'Acme.Controls.AbstractPopupToken', which is not bound to a control."
        ], ignoreOrder: true);
        failures.ShouldAllBe(static d => d.Location.GetLineSpan().Path == "Acme.Themes.cs");
        failures.ShouldAllBe(static d => d.Severity == DiagnosticSeverity.Error);
        package.SyntaxTrees.ShouldNotContain(tree => tree.FilePath.EndsWith(RegistrationFile, StringComparison.Ordinal));
    }

    [Fact]
    public void Reports_A_Control_Stage_Class_Without_A_Public_Parameterless_Constructor()
    {
        var source = ControlsSource + """

            namespace Acme.Themes
            {
                using AtomUI.Theme.Algorithms;
                using AtomUI.Theme.DesignTokens;

                [ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
                internal sealed class PrimeNGToolTipTokens : IControlTokenAlgorithm<Acme.Controls.ToolTipToken>
                {
                    public PrimeNGToolTipTokens(int scale) { }
                    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal) { }
                    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, Acme.Controls.ToolTipToken token) { }
                }

                [ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
                internal sealed class HiddenToolTipTokens : IControlTokenAlgorithm<Acme.Controls.ToolTipToken>
                {
                    private HiddenToolTipTokens() { }
                    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal) { }
                    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, Acme.Controls.ToolTipToken token) { }
                }
            }
            """;

        var package = RunGenerator(CreateCompilation("Acme.Themes", source, RuntimeReference()), out var diagnostics);

        var failures = diagnostics.Where(static d => d.Id == "ATOMUITOKEN002").ToArray();
        failures.Select(static d => d.GetMessage(null)).ShouldBe([
            "'Acme.Themes.HiddenToolTipTokens' must have a public parameterless constructor.",
            "'Acme.Themes.PrimeNGToolTipTokens' must have a public parameterless constructor."
        ], ignoreOrder: true);
        failures.ShouldAllBe(static d => d.Location.GetLineSpan().Path == "Acme.Themes.cs");
        package.SyntaxTrees.ShouldNotContain(tree => tree.FilePath.EndsWith(RegistrationFile, StringComparison.Ordinal));
    }

    private const string ControlsSource = """
        using AtomUI.Theme.DesignTokens;

        namespace Acme.Controls
        {
            public sealed class ToolTip : Avalonia.Controls.Control
            {
            }

            [ControlDesignToken]
            public abstract partial class AbstractPopupToken : AbstractControlDesignToken
            {
                public double Elevation { get; set; }
            }

            [ControlDesignToken]
            public sealed partial class ToolTipToken : AbstractPopupToken
            {
                public double Padding { get; set; }

                protected override void CalculateTokenValues(bool isDark)
                {
                    base.CalculateTokenValues(isDark);
                    Padding = EffectiveGlobalToken.ControlHeight;
                }
            }

            [ControlDesignToken]
            public sealed partial class OrphanToken : AbstractControlDesignToken
            {
                public double Width { get; set; }
            }
        }
        """;

    // Declared out of identity order on purpose: the registration sorts by algorithm identity.
    private const string LocalStageSource = """

        namespace Acme.Themes
        {
            using AtomUI.Theme.Algorithms;
            using AtomUI.Theme.DesignTokens;
            using AtomUI.Theme.Schema;

            [ThemeAlgorithm("PrimeNG", "Aura", 1, ThemeAppearanceEffect.Light)]
            public sealed class AuraAlgorithm : IThemeAlgorithm
            {
            }

            [ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
            public sealed class LaraToolTipTokens : IControlTokenAlgorithm<Acme.Controls.ToolTipToken>
            {
                public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal) { }
                public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, Acme.Controls.ToolTipToken token) { }
            }

            [ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
            [ControlTokenAlgorithm("PrimeNG", "Aura", 2)]
            internal sealed class PrimeNGToolTipTokens : IControlTokenAlgorithm<Acme.Controls.ToolTipToken>
            {
                public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal) { }
                public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, Acme.Controls.ToolTipToken token) { }
            }
        }
        """;

    private const string ReferencedStageSource = """
        using AtomUI.Theme.Algorithms;
        using AtomUI.Theme.DesignTokens;

        namespace Acme.Themes
        {
            [ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
            internal sealed class PrimeNGToolTipTokens : IControlTokenAlgorithm<Acme.Controls.ToolTipToken>
            {
                public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal) { }
                public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, Acme.Controls.ToolTipToken token) { }
            }
        }
        """;

    // The control stage contract as AtomUI.Core declares it; the builder lives here, so generated registrations
    // are only emitted for the assemblies that reference this one.
    private const string ControlStageStubs = """

        namespace AtomUI.Theme
        {
            public interface IThemeManagerBuilder
            {
                IThemeManagerBuilder AddThemeAlgorithms(
                    System.Collections.Generic.IEnumerable<AtomUI.Theme.Schema.ThemeAlgorithmDescriptor> algorithms);
                IThemeManagerBuilder AddControlTokenAlgorithms(
                    System.Collections.Generic.IEnumerable<AtomUI.Theme.Schema.ControlTokenAlgorithmDescriptor> algorithms);
            }
        }

        namespace AtomUI.Theme.DesignTokens
        {
            public interface IControlToken
            {
                static abstract AtomUI.Theme.Schema.ControlTokenIdentity ControlIdentity { get; }
            }
        }

        namespace AtomUI.Theme.Algorithms
        {
            public readonly struct ThemeAlgorithmContext
            {
            }

            public interface IControlTokenAlgorithm<TToken>
                where TToken : AtomUI.Theme.DesignTokens.AbstractControlDesignToken
            {
                void EvaluateGlobal(in ThemeAlgorithmContext context, AtomUI.Theme.DesignTokens.DesignToken controlGlobal);
                void EvaluateOwn(in ThemeAlgorithmContext context, AtomUI.Theme.DesignTokens.DesignToken effectiveGlobal, TToken token);
            }

            [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
            public sealed class ControlTokenAlgorithmAttribute : System.Attribute
            {
                public ControlTokenAlgorithmAttribute(string catalog, string id, int revision)
                {
                }
            }
        }

        namespace AtomUI.Theme.Schema
        {
            public sealed class ControlTokenAlgorithmDescriptor
            {
                public static ControlTokenAlgorithmDescriptor Create<TToken, TAlgorithm>(
                    AtomUI.Theme.Algorithms.ThemeAlgorithm algorithm, int revision)
                    where TToken : AtomUI.Theme.DesignTokens.AbstractControlDesignToken, AtomUI.Theme.DesignTokens.IControlToken
                    where TAlgorithm : class, AtomUI.Theme.Algorithms.IControlTokenAlgorithm<TToken>, new() => null!;
            }
        }
        """;

    // An older runtime without the control stage: only the algorithm half of the builder.
    private const string LegacyBuilderStub = """

        namespace AtomUI.Theme
        {
            public interface IThemeManagerBuilder
            {
                IThemeManagerBuilder AddThemeAlgorithms(
                    System.Collections.Generic.IEnumerable<AtomUI.Theme.Schema.ThemeAlgorithmDescriptor> algorithms);
            }
        }
        """;

    // Core generates this enum from its global token; referencing assemblies resolve it from Core.
    private const string SharedTokenKindStub = """

        namespace AtomUI.Theme.Resources
        {
            public enum SharedTokenKind
            {
                ControlHeight
            }
        }
        """;

    private static MetadataReference RuntimeReference(bool withControlStage = true) =>
        CSharpCompilation.Create(
            "AtomUI.Core",
            [CSharpSyntaxTree.ParseText(ThemeSchemaGeneratorTests.AtomUIStubs + SharedTokenKindStub +
                                        (withControlStage ? ControlStageStubs : LegacyBuilderStub))],
            PlatformReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)).ToMetadataReference();

    private static CSharpCompilation CreateCompilation(string assemblyName, string source, params MetadataReference[] references) =>
        CSharpCompilation.Create(
            assemblyName,
            [CSharpSyntaxTree.ParseText(source, path: assemblyName + ".cs")],
            PlatformReferences().AddRange(references),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    private static ImmutableArray<MetadataReference> PlatformReferences() =>
        ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))!.Split(Path.PathSeparator)
            .Select(path => (MetadataReference)MetadataReference.CreateFromFile(path)).ToImmutableArray();

    private static CSharpCompilation RunGenerator(CSharpCompilation compilation, out ImmutableArray<Diagnostic> diagnostics)
    {
        var driver = CSharpGeneratorDriver.Create([new TokenResourceKeyGenerator().AsSourceGenerator()],
            ImmutableArray<AdditionalText>.Empty, (CSharpParseOptions)compilation.SyntaxTrees[0].Options,
            new BuiltinCatalogOptionsProvider(null));
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out diagnostics, TestContext.Current.CancellationToken);
        // These fixtures declare Token shapes without theme assets; ATOMUIGEN042 has dedicated coverage elsewhere.
        diagnostics = diagnostics.Where(static diagnostic => diagnostic.Id != "ATOMUIGEN042").ToImmutableArray();
        return (CSharpCompilation)output;
    }

    // The shared runtime stubs declare RunDefaultCalculation on the base class as internal to the stub assembly; in
    // AtomUI the token's own generated half provides it, so that one access error is not about this output.
    private static Diagnostic[] Errors(CSharpCompilation compilation) =>
        compilation.GetDiagnostics(TestContext.Current.CancellationToken)
                   .Where(static d => d.Severity == DiagnosticSeverity.Error &&
                                      !(d.Id == "CS0122" && d.GetMessage(null).Contains("RunDefaultCalculation")))
                   .ToArray();

    private static bool Implements(CSharpCompilation compilation, string tokenMetadataName) =>
        compilation.GetTypeByMetadataName(tokenMetadataName)!.AllInterfaces
                   .Any(static type => type.ToDisplayString() == ControlTokenInterface);

    private static string GetGeneratedSource(CSharpCompilation compilation, string fileName) =>
        compilation.SyntaxTrees.Single(tree => tree.FilePath.EndsWith(fileName, StringComparison.Ordinal))
                   .GetText(TestContext.Current.CancellationToken).ToString();

    private static int CountOf(string source, string value)
    {
        var count = 0;
        for (var index = source.IndexOf(value, StringComparison.Ordinal); index >= 0;
             index = source.IndexOf(value, index + value.Length, StringComparison.Ordinal))
        {
            count++;
        }
        return count;
    }
}
