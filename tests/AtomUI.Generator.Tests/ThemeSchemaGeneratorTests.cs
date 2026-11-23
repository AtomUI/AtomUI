using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;

namespace AtomUI.Generator.Tests;

public class ThemeSchemaGeneratorTests
{
    [Fact]
    public void Generates_Complete_Aot_Schema_Descriptors_With_Stable_Slots()
    {
        var outputCompilation = RunGenerator(CreateCompilation(TokenSource), out var diagnostics);

        diagnostics.ShouldBeEmpty();
        outputCompilation.GetDiagnostics(TestContext.Current.CancellationToken)
                         .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
                         .ShouldBeEmpty();

        var source = GetGeneratedSource(outputCompilation, "GeneratedThemeSchema.g.cs");

        source.ShouldContain("namespace AtomUI.Generated.ThemeSchemaGeneratorTests;");
        source.ShouldContain("internal static class GeneratedThemeSchema");
        source.ShouldNotContain("namespace AtomUI.Theme.Schema;");
        source.ShouldNotContain("class ThemeSchemaDescriptorPool");
        source.ShouldContain("typeof(global::Demo.Button)");
        source.ShouldContain("ControlTokenIdentity.ForControl(typeof(global::Demo.Button), \"AtomUI\", \"Button\")");
        source.ShouldContain("global::AtomUI.Theme.Resources.SharedTokenKind");
        source.ShouldContain("TokenDescriptor.CreateGenerated(\"Alpha\", 0, TokenStage.Seed, TokenValueCodec<global::System.Double>.Instance");
        source.ShouldContain("TokenDescriptor.CreateGenerated(\"Zeta\", 1, TokenStage.Alias");
        source.ShouldContain("TokenDescriptor.CreateGenerated(\"Height\", 0, TokenStage.Control");
        source.ShouldContain("TokenDescriptor.CreateGenerated(\"Label\", 1, TokenStage.Control");
        source.ShouldNotContain("SupportedGlobalToken");
        source.ShouldContain("case 0: ((global::Demo.ButtonToken)token).Height = (global::System.Double)value!; return;");
        source.ShouldContain("case 0: return global::AtomUI.Theme.Schema.ThemeResourceValue.Project(((global::Demo.ButtonToken)token).Height);");
        // Per-token delegates and their static caches scale with the whole package, not with retained controls.
        source.ShouldNotContain("static value =>");
        source.ShouldNotContain("static token =>");
        source.ShouldNotContain("static (token, value) =>");
        // The descriptor only creates the token: values come from the language's control derivations.
        source.ShouldContain("static () => new global::Demo.ButtonToken()" + Environment.NewLine);
        source.ShouldNotContain("RunDefaultCalculation");
        source.ShouldNotContain("static (token, appearance)");
        // Theme algorithms are gone from the schema.
        source.ShouldNotContain("ThemeAlgorithmDescriptor");
        source.ShouldNotContain("GetAlgorithms");
        source.ShouldNotContain("Activator.CreateInstance");
        source.ShouldNotContain("PropertyInfo");
        source.ShouldNotContain("typeof(global::Demo.ButtonToken)");
    }

    [Fact]
    public void Global_Schema_Fingerprint_Is_Emitted_Only_For_An_Assembly_With_Global_Tokens()
    {
        var withGlobals = GetGeneratedSource(RunGenerator(CreateCompilation(TokenSource), out _), "GeneratedThemeSchema.g.cs");
        withGlobals.ShouldContain("internal const ulong GlobalSchemaFingerprint = 0x");

        var controlsOnly = GetGeneratedSource(RunGenerator(CreateCompilation("""
            using AtomUI.Theme.DesignTokens;

            namespace Demo
            {
                [ControlDesignToken]
                internal sealed class ButtonToken : AbstractControlDesignToken
                {
                    public double Height { get; set; }
                }
            }
            """), out var diagnostics), "GeneratedThemeSchema.g.cs");

        diagnostics.ShouldBeEmpty();
        controlsOnly.ShouldNotContain("GlobalSchemaFingerprint");
        controlsOnly.ShouldNotContain("CopyGlobalValues");
    }

    [Fact]
    public void Descriptor_Output_Is_Independent_Of_Declaration_Order()
    {
        var first = GetGeneratedSource(
            RunGenerator(CreateCompilation(TokenSource), out _),
            "GeneratedThemeSchema.g.cs");
        var second = GetGeneratedSource(
            RunGenerator(CreateCompilation(ReorderedTokenSource), out _),
            "GeneratedThemeSchema.g.cs");

        second.ShouldBe(first);
    }

    [Theory]
    [InlineData("AtomUI.Desktop.Controls.DataGrid", "AtomUIDesktopControlsDataGrid")]
    [InlineData("acme-controls_package", "AcmeControlsPackage")]
    [InlineData("123.Controls", "Assembly123Controls")]
    public void Generated_Namespace_Uses_One_Pascal_Owner_Identifier_Without_Separators(
        string assemblyName,
        string expectedOwnerIdentifier)
    {
        var compilation = CreateCompilation(TokenSource, assemblyName);
        var outputCompilation = RunGenerator(compilation, out var diagnostics);

        diagnostics.ShouldBeEmpty();
        var source = GetGeneratedSource(outputCompilation, "GeneratedThemeSchema.g.cs");
        source.ShouldContain($"namespace AtomUI.Generated.{expectedOwnerIdentifier};");
        source.ShouldNotContain("namespace AtomUI.Generated._");
    }

    private static CSharpCompilation RunGenerator(CSharpCompilation compilation, out ImmutableArray<Diagnostic> diagnostics)
    {
        var driver = CSharpGeneratorDriver.Create([new TokenResourceKeyGenerator().AsSourceGenerator()],
            ImmutableArray<AdditionalText>.Empty, (CSharpParseOptions)compilation.SyntaxTrees[0].Options,
            new BuiltinCatalogOptionsProvider(null));
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out diagnostics, TestContext.Current.CancellationToken);
        // These fixtures declare Token shapes without theme assets; ATOMUIGEN042 has dedicated coverage in OwnTokenReferenceAnalysisTests.
        diagnostics = diagnostics.Where(static diagnostic => diagnostic.Id != "ATOMUIGEN042").ToImmutableArray();
        return (CSharpCompilation)output;
    }

    private static string GetGeneratedSource(CSharpCompilation compilation, string fileName)
    {
        return compilation.SyntaxTrees
                          .Single(tree => tree.FilePath.EndsWith(fileName, StringComparison.Ordinal))
                          .GetText(TestContext.Current.CancellationToken)
                          .ToString();
    }

    private static CSharpCompilation CreateCompilation(
        string source,
        string assemblyName = "ThemeSchemaGeneratorTests",
        string sourcePath = "TokenSource.cs")
    {
        var references = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))!
                         .Split(Path.PathSeparator)
                         .Select(path => MetadataReference.CreateFromFile(path))
                         .Cast<MetadataReference>()
                         .ToImmutableArray();

        return CSharpCompilation.Create(
            assemblyName,
            [CSharpSyntaxTree.ParseText(source, path: sourcePath), CSharpSyntaxTree.ParseText(AtomUIStubs)],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    private const string TokenSource = """
        using AtomUI.Theme.Schema;
        using AtomUI.Theme.DesignTokens;

        namespace Demo
        {
            [GlobalDesignToken]
            public partial class DesignToken : AbstractDesignToken
            {
                [DesignTokenKind(DesignTokenKind.Alias)]
                public string Zeta { get; set; } = string.Empty;

                [DesignTokenKind(DesignTokenKind.Seed)]
                public double Alpha { get; set; }
            }

            [ControlDesignToken]
            internal sealed class ButtonToken : AbstractControlDesignToken
            {
                public string Label { get; set; } = string.Empty;
                public double Height { get; set; }
            }
        }
        """;

    private const string ReorderedTokenSource = """
        using AtomUI.Theme.Schema;
        using AtomUI.Theme.DesignTokens;

        namespace Demo
        {
            [ControlDesignToken]
            internal sealed class ButtonToken : AbstractControlDesignToken
            {
                public double Height { get; set; }
                public string Label { get; set; } = string.Empty;
            }

            [GlobalDesignToken]
            public partial class DesignToken : AbstractDesignToken
            {
                [DesignTokenKind(DesignTokenKind.Seed)]
                public double Alpha { get; set; }

                [DesignTokenKind(DesignTokenKind.Alias)]
                public string Zeta { get; set; } = string.Empty;
            }
        }
        """;

    internal const string AtomUIStubs = """
        namespace Avalonia.Media
        {
            public sealed class FontFamily
            {
            }
        }

        namespace Avalonia.Controls
        {
            public class Control
            {
            }
        }

        namespace Demo
        {
            public sealed class Button : Avalonia.Controls.Control
            {
            }
        }

        namespace AtomUI.Theme.Resources
        {
            public abstract class TokenResourceExtension<TTokenKind>
                where TTokenKind : System.Enum
            {
                protected TokenResourceExtension()
                {
                }

                protected TokenResourceExtension(TTokenKind kind)
                {
                }

                protected virtual object GetResourceKey(TTokenKind kind) => kind;
            }

            public static class ControlTokenResourceKey
            {
                public static object Own(AtomUI.Theme.Schema.ControlTokenIdentity identity, object key) => key;
                public static object Global(
                    AtomUI.Theme.Schema.ControlTokenIdentity identity,
                    SharedTokenKind kind) => kind;
            }
        }

        namespace AtomUI.Theme
        {
            public enum ThemeAppearance : byte
            {
                Light,
                Dark
            }
        }

        namespace AtomUI.Theme.Schema
        {
            public enum TokenStage : byte
            {
                Seed,
                Map,
                Alias,
                Control,
                Language
            }

            public readonly record struct ControlTokenIdentity(string Catalog, string Id)
            {
                public static ControlTokenIdentity ForControl(System.Type controlType, string catalog, string id) => new(catalog, id);
            }

            public abstract class TokenValueCodec
            {
            }

            public sealed class TokenValueCodec<T> : TokenValueCodec
            {
                public static readonly TokenValueCodec<T> Instance = new();
                public static T Copy(T value) => value;
            }

            public abstract class TokenValueAccessor
            {
                public abstract object? GetValue(AtomUI.Theme.DesignTokens.AbstractDesignToken token, int slot);
                public abstract void SetValue(AtomUI.Theme.DesignTokens.AbstractDesignToken token, int slot, object? value);
                public abstract object? ProjectResourceValue(AtomUI.Theme.DesignTokens.AbstractDesignToken token, int slot);
            }

            public sealed class TokenDescriptor
            {
                public static TokenDescriptor CreateGenerated(
                    string name, int slot, TokenStage stage, TokenValueCodec codec, object resourceKey, TokenValueAccessor accessor) => null!;
            }

            public sealed class ControlTokenDescriptor
            {
                public ControlTokenDescriptor(System.Type controlType, ControlTokenIdentity identity)
                {
                }

                public ControlTokenDescriptor(
                    System.Type controlType,
                    ControlTokenIdentity identity,
                    System.Collections.Generic.IReadOnlyList<TokenDescriptor> ownTokens,
                    System.Func<AtomUI.Theme.DesignTokens.AbstractControlDesignToken> factory)
                {
                }
            }

            public static class ThemeTokenValueParser
            {
                public static T Parse<T>(string value)
                {
                    return default!;
                }
            }

            public static class ThemeTokenValueFormatter
            {
                public static string Format<T>(T value)
                {
                    return string.Empty;
                }
            }

            public static class ThemeResourceValue
            {
                public static object? Project<T>(T value)
                {
                    return value;
                }
            }
        }

        namespace AtomUI.Theme.DesignTokens
        {
            public enum DesignTokenKind
            {
                Seed,
                Map,
                Alias
            }

            [System.AttributeUsage(System.AttributeTargets.Property)]
            public sealed class DesignTokenKindAttribute : System.Attribute
            {
                public DesignTokenKindAttribute(DesignTokenKind kind)
                {
                }
            }

            [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false)]
            public sealed class GlobalDesignTokenAttribute : System.Attribute
            {
            }

            [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false)]
            public sealed class ControlDesignTokenAttribute : System.Attribute
            {
            }

            [System.AttributeUsage(System.AttributeTargets.Property)]
            public sealed class NotTokenDefinitionAttribute : System.Attribute
            {
            }

            [System.AttributeUsage(System.AttributeTargets.Property)]
            public sealed class ReservedTokenAttribute : System.Attribute
            {
                public ReservedTokenAttribute(string reason)
                {
                    Reason = reason;
                }

                public string Reason { get; }
            }

            public abstract class AbstractDesignToken
            {
                protected bool IsFrozen(string tokenName) => false;
                protected void OnRead(string tokenName) { }
            }

            public sealed class DesignToken : AbstractDesignToken
            {
                [NotTokenDefinition]
                public System.Collections.Generic.IDictionary<string, string> ColorPalettes { get; } =
                    new System.Collections.Generic.Dictionary<string, string>();

                public double ControlHeight { get; set; }
            }

            public abstract class AbstractControlDesignToken : AbstractDesignToken
            {
                // Core no longer has these members; OwnTokenReferenceAnalysis still recognizes them by name, so the
                // fixtures of its tests keep compiling against this stub.
                protected DesignToken EffectiveGlobalToken { get; } = new DesignToken();

                protected virtual void CalculateTokenValues(bool isDark)
                {
                }
            }
        }
        """;
}
