using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
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
        source.ShouldContain("static () => new global::Demo.ButtonToken()");
        source.ShouldContain("((global::Demo.ButtonToken)token).CalculateTokenValues(appearance == global::AtomUI.Theme.ThemeAppearance.Dark)");
        source.ShouldContain("new ThemeAlgorithmDescriptor(global::AtomUI.Theme.Algorithms.ThemeAlgorithm.Dark, 1, ThemeAppearanceEffect.Dark");
        source.ShouldContain("new ThemeAlgorithmDescriptor(global::AtomUI.Theme.Algorithms.ThemeAlgorithm.Default, 1, ThemeAppearanceEffect.Light");
        source.ShouldNotContain("Activator.CreateInstance");
        source.ShouldNotContain("PropertyInfo");
        source.ShouldNotContain("typeof(global::Demo.ButtonToken)");
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

    [Fact]
    public void Ignores_An_Undefined_ThemeAlgorithm_Attribute_Value()
    {
        var invalidSource = TokenSource.Replace(
            "ThemeAlgorithm.Dark, 1, ThemeAppearanceEffect.Dark",
            "(ThemeAlgorithm)999, 1, ThemeAppearanceEffect.Dark",
            StringComparison.Ordinal);

        var outputCompilation = RunGenerator(CreateCompilation(invalidSource), out var diagnostics);

        diagnostics.ShouldBeEmpty();
        var source = GetGeneratedSource(outputCompilation, "GeneratedThemeSchema.g.cs");
        source.ShouldContain("ThemeAlgorithm.Default");
        source.ShouldNotContain("ThemeAlgorithm.Dark");
        source.ShouldNotContain("999");
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

    [Fact]
    public void Ignores_Non_Token_Effective_Global_Calculation_Helpers()
    {
        var outputCompilation = RunGenerator(CreateCompilation("""
            using AtomUI.Theme.DesignTokens;

            namespace Demo
            {
                [ControlDesignToken]
                internal sealed class ButtonToken : AbstractControlDesignToken
                {
                    public double Height { get; set; }

                    public override void CalculateTokenValues(bool isDark)
                    {
                        _ = EffectiveGlobalToken.ColorPalettes.Count;
                        Height = EffectiveGlobalToken.ControlHeight;
                    }
                }
            }
            """), out var diagnostics);

        diagnostics.ShouldBeEmpty();
        var source = GetGeneratedSource(outputCompilation, "GeneratedThemeSchema.g.cs");
        source.ShouldNotContain("\"ColorPalettes\"");
    }

    private static CSharpCompilation RunGenerator(CSharpCompilation compilation, out ImmutableArray<Diagnostic> diagnostics,
        params AdditionalText[] additionalTexts)
    {
        var driver = CSharpGeneratorDriver.Create([new TokenResourceKeyGenerator().AsSourceGenerator()],
            additionalTexts.ToImmutableArray(), (CSharpParseOptions)compilation.SyntaxTrees[0].Options,
            new BuiltinCatalogOptionsProvider(null));
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out diagnostics, TestContext.Current.CancellationToken);
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
        using AtomUI.Theme.Algorithms;
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

                public override void CalculateTokenValues(bool isDark)
                {
                    Height = EffectiveGlobalToken.ControlHeight;
                }
            }

            [ThemeAlgorithm(ThemeAlgorithm.Default, 1, ThemeAppearanceEffect.Light)]
            internal sealed class DefaultAlgorithm : IThemeAlgorithm
            {
                public DefaultAlgorithm()
                {
                }
            }

            [ThemeAlgorithm(ThemeAlgorithm.Dark, 1, ThemeAppearanceEffect.Dark)]
            internal sealed class DarkAlgorithm : IThemeAlgorithm
            {
                public DarkAlgorithm()
                {
                }
            }
        }
        """;

    private const string ReorderedTokenSource = """
        using AtomUI.Theme.Algorithms;
        using AtomUI.Theme.Schema;
        using AtomUI.Theme.DesignTokens;

        namespace Demo
        {
            [ThemeAlgorithm(ThemeAlgorithm.Dark, 1, ThemeAppearanceEffect.Dark)]
            internal sealed class DarkAlgorithm : IThemeAlgorithm
            {
                public DarkAlgorithm()
                {
                }
            }

            [ThemeAlgorithm(ThemeAlgorithm.Default, 1, ThemeAppearanceEffect.Light)]
            internal sealed class DefaultAlgorithm : IThemeAlgorithm
            {
                public DefaultAlgorithm()
                {
                }
            }

            [ControlDesignToken]
            internal sealed class ButtonToken : AbstractControlDesignToken
            {
                public double Height { get; set; }
                public string Label { get; set; } = string.Empty;

                public override void CalculateTokenValues(bool isDark)
                {
                    Height = EffectiveGlobalToken.ControlHeight;
                }
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

    private const string AtomUIStubs = """
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
                Control
            }

            public enum ThemeAppearanceEffect : byte
            {
                Preserve,
                Light,
                Dark
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

                public TokenDescriptor(
                    string name,
                    int slot,
                    TokenStage stage,
                    System.Type valueType,
                    object resourceKey,
                    System.Func<string, object?> parser,
                    System.Func<object?, string> formatter,
                    System.Func<AtomUI.Theme.DesignTokens.AbstractDesignToken, object?> getter,
                    System.Action<AtomUI.Theme.DesignTokens.AbstractDesignToken, object?> setter,
                    System.Func<AtomUI.Theme.DesignTokens.AbstractDesignToken, object?> resourceProjector)
                {
                }
            }

            public sealed class ControlTokenDescriptor
            {
                public ControlTokenDescriptor(
                    System.Type controlType,
                    ControlTokenIdentity identity,
                    System.Collections.Generic.IReadOnlyList<TokenDescriptor> ownTokens,
                    System.Func<AtomUI.Theme.DesignTokens.AbstractControlDesignToken> factory,
                    System.Action<AtomUI.Theme.DesignTokens.AbstractControlDesignToken, AtomUI.Theme.ThemeAppearance> evaluator)
                {
                }
            }

            public sealed class ThemeAlgorithmDescriptor
            {
                public ThemeAlgorithmDescriptor(
                    AtomUI.Theme.Algorithms.ThemeAlgorithm algorithm,
                    int revision,
                    ThemeAppearanceEffect appearanceEffect,
                    System.Func<AtomUI.Theme.Algorithms.IThemeAlgorithm> factory)
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

        namespace AtomUI.Theme.Algorithms
        {
            public enum ThemeAlgorithm
            {
                Default = 0,
                Dark = 1,
                Compact = 2
            }

            public interface IThemeAlgorithm
            {
            }

            [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false)]
            public sealed class ThemeAlgorithmAttribute : System.Attribute
            {
                public ThemeAlgorithmAttribute(
                    ThemeAlgorithm algorithm,
                    int revision,
                    AtomUI.Theme.Schema.ThemeAppearanceEffect appearanceEffect)
                {
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

            public abstract class AbstractDesignToken
            {
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
                protected DesignToken EffectiveGlobalToken { get; } = new DesignToken();

                public virtual void CalculateTokenValues(bool isDark)
                {
                }
            }
        }
        """;

    private sealed class InMemoryAdditionalText : AdditionalText
    {
        private readonly Microsoft.CodeAnalysis.Text.SourceText _text;

        internal InMemoryAdditionalText(string path, string text)
        {
            Path = path;
            _text = Microsoft.CodeAnalysis.Text.SourceText.From(text);
        }

        public override string Path { get; }

        public override Microsoft.CodeAnalysis.Text.SourceText GetText(
            CancellationToken cancellationToken = default)
        {
            return _text;
        }
    }

    private sealed class TestAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
    {
        private static readonly AnalyzerConfigOptions s_empty =
            new TestAnalyzerConfigOptions(new Dictionary<string, string>());
        private readonly AnalyzerConfigOptions _globalOptions;

        internal TestAnalyzerConfigOptionsProvider(IReadOnlyDictionary<string, string> globalOptions)
        {
            _globalOptions = new TestAnalyzerConfigOptions(globalOptions);
        }

        public override AnalyzerConfigOptions GlobalOptions => _globalOptions;

        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => s_empty;

        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => s_empty;
    }

    private sealed class TestAnalyzerConfigOptions : AnalyzerConfigOptions
    {
        private readonly IReadOnlyDictionary<string, string> _values;

        internal TestAnalyzerConfigOptions(IReadOnlyDictionary<string, string> values)
        {
            _values = values;
        }

        public override bool TryGetValue(string key, out string value)
        {
            return _values.TryGetValue(key, out value!);
        }
    }
}
