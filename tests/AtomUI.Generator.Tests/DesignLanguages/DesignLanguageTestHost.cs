using System.Collections.Immutable;
using AtomUI.Generator.DesignLanguages;
using AtomUI.Generator.DesignToken;
using AtomUI.Theme.Build;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Shouldly;
using Xunit;

namespace AtomUI.Generator.Tests.DesignLanguages;

/// <summary>
/// Builds the package graph of the design-language architecture from source: a core runtime that declares the global
/// tokens and the theme build ABI, a language package, a control package, and the packages that consume them (theme
/// packages and value packages). Every package runs the real generators, so later packages read the build contracts
/// and the generated <c>IControlToken</c> halves from metadata exactly as they would in a build.
/// </summary>
internal static class DesignLanguageTestHost
{
    internal const string ContractsFile = "GeneratedThemeBuildContracts.g.cs";
    internal const string PackageFile = "ThemePackageRegistration.g.cs";
    internal const string LanguageSchemaFile = "GeneratedLanguageTokenSchema.g.cs";
    internal const string LanguageRegistrationFile = "DesignLanguageRegistration.g.cs";
    internal const string TokenRuleRegistrationFile = "GeneratedControlTokenRuleRegistration.g.cs";
    internal const string IdentitiesFile = "GeneratedControlTokenIdentities.g.cs";

    internal static readonly IIncrementalGenerator[] AllGenerators =
    [
        new TokenResourceKeyGenerator(),
        new TokenPropertyImplementationGenerator(),
        new DesignLanguageRegistrationGenerator(),
        new ControlTokenRuleRegistrationGenerator()
    ];

    // The schema-generator stubs plus the pieces of Core the design-language generators bind against: the compiled
    // theme ABI (7-parameter CreateGenerated, build contract attributes, program base types), the language contract,
    // the control token rule contract and the builder.
    internal static readonly string RuntimeStubs = ThemeSchemaGeneratorTests.AtomUIStubs
        .Replace("public sealed class TokenDescriptor", "public sealed partial class TokenDescriptor", StringComparison.Ordinal) + """

        namespace Avalonia.Media
        {
            public readonly struct Color
            {
                public Color(byte a, byte r, byte g, byte b)
                {
                }
            }
        }

        namespace AtomUI.Theme
        {
            public interface IThemeManagerBuilder
            {
                void AddCompiledThemePackage(AtomUI.Theme.Definitions.Compiled.CompiledThemePackage package);
                IThemeManagerBuilder AddDesignLanguage(AtomUI.Theme.Schema.DesignLanguageDescriptor language);
                IThemeManagerBuilder AddControlTokenRules(
                    System.Collections.Generic.IEnumerable<AtomUI.Theme.Schema.ControlTokenRuleDescriptor> rules);
            }
        }

        namespace AtomUI.Theme.DesignTokens
        {
            [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
            public sealed class LanguageDesignTokenAttribute : System.Attribute
            {
            }

            public abstract class LanguageDesignToken : AbstractDesignToken
            {
            }

            public interface IControlToken
            {
                static abstract AtomUI.Theme.Schema.ControlTokenIdentity ControlIdentity { get; }
                static virtual string? TokenSchemaDigest => null;
            }
        }

        namespace AtomUI.Theme.Languages
        {
            [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
            public sealed class DesignLanguageAttribute : System.Attribute
            {
            }

            [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
            public sealed class ControlTokenRuleAttribute : System.Attribute
            {
            }

            public sealed class DesignLanguageAxes
            {
            }

            public readonly ref struct TokenEvaluationContext<TLanguageToken>
                where TLanguageToken : AtomUI.Theme.DesignTokens.LanguageDesignToken
            {
                public TLanguageToken Language => null!;
            }

            public sealed class ControlTokenTarget<TToken>
                where TToken : AtomUI.Theme.DesignTokens.AbstractControlDesignToken
            {
                public TToken Own => null!;
            }

            public abstract class DesignLanguage
            {
                private protected DesignLanguage()
                {
                }

                public abstract string Id { get; }

                public abstract DesignLanguageAxes Axes { get; }
            }

            public abstract class DesignLanguage<TLanguageToken> : DesignLanguage
                where TLanguageToken : AtomUI.Theme.DesignTokens.LanguageDesignToken, new()
            {
                public abstract void DeriveLanguage(TLanguageToken language);
            }

            public interface IControlTokenRule<TLanguageToken, TToken>
                where TLanguageToken : AtomUI.Theme.DesignTokens.LanguageDesignToken
                where TToken : AtomUI.Theme.DesignTokens.AbstractControlDesignToken
            {
                void Apply(in TokenEvaluationContext<TLanguageToken> context, ControlTokenTarget<TToken> target);
            }
        }

        namespace AtomUI.Theme.Schema
        {
            [System.AttributeUsage(System.AttributeTargets.Assembly)]
            public sealed class TokenSchemaDigestAttribute(string digest) : System.Attribute
            {
                public string Digest { get; } = digest;
            }

            [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple = true)]
            public sealed class CoversTokenSchemaAttribute(string assemblyName, string digest) : System.Attribute
            {
                public string AssemblyName { get; } = assemblyName;
                public string Digest { get; } = digest;
            }

            public sealed partial class TokenDescriptor
            {
                public static TokenDescriptor CreateGenerated(string name, int slot, TokenStage stage, TokenValueCodec codec,
                    object resourceKey, TokenValueAccessor accessor, int valueContractVersion) => null!;
            }

            public sealed class DesignLanguageDescriptor
            {
                public DesignLanguageDescriptor(AtomUI.Theme.Languages.DesignLanguage language,
                    System.Collections.Generic.IReadOnlyList<TokenDescriptor> languageTokens)
                {
                }
            }

            public sealed class ControlTokenRuleDescriptor
            {
                public static ControlTokenRuleDescriptor Create<TLanguageToken, TToken, TRule>()
                    where TLanguageToken : AtomUI.Theme.DesignTokens.LanguageDesignToken
                    where TToken : AtomUI.Theme.DesignTokens.AbstractControlDesignToken, AtomUI.Theme.DesignTokens.IControlToken
                    where TRule : class, AtomUI.Theme.Languages.IControlTokenRule<TLanguageToken, TToken>, new() => null!;

                public static ControlTokenRuleDescriptor Create<TLanguageToken, TToken, TRule>(string expectedTokenSchemaDigest)
                    where TLanguageToken : AtomUI.Theme.DesignTokens.LanguageDesignToken
                    where TToken : AtomUI.Theme.DesignTokens.AbstractControlDesignToken, AtomUI.Theme.DesignTokens.IControlToken
                    where TRule : class, AtomUI.Theme.Languages.IControlTokenRule<TLanguageToken, TToken>, new() => null!;
            }
        }

        namespace AtomUI.Theme.Definitions.Compiled
        {
            [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple = true)]
            public sealed class ThemeBuildOwnerAttribute(int version, string catalog, string id, string ownerAssemblyIdentity,
                string ownerMetadataName, string identityProvider, string schemaDigest) : System.Attribute { }

            [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple = true)]
            public sealed class ThemeBuildTokenAttribute(int version, string catalog, string ownerId, string name, string valueType,
                string valueTypeAssemblyIdentity, string resourceKeyType, string resourceKeyAssemblyIdentity, string resourceKeyMember,
                int stage, int valueContractVersion) : System.Attribute { }

            [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple = true)]
            public sealed class ThemeBuildLanguageAttribute(int version, string id, string languageTokenType,
                string languageTokenAssemblyIdentity) : System.Attribute { }

            [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple = true)]
            public sealed class CompiledThemeManifestAttribute(int version, string packageId, string data) : System.Attribute { }

            public abstract class CompiledThemeProgram
            {
                protected CompiledThemeProgram(int abiVersion, string sourceIdentity, string sourceRevision, string contentDigest,
                    string programDigest, string id, string name, string language, string style, int appearance,
                    string density, string[] lockedSeeds, Avalonia.Media.Color? accentColor)
                {
                }

                protected virtual bool IsBackgroundPrepareSafe => false;

                protected abstract void Describe(CompiledThemeBindingBuilder builder);

                protected abstract void WriteStaticDefaults(CompiledThemeValueWriter writer);
            }

            public sealed class CompiledThemeBindingBuilder
            {
                public void BeginSection(int index, string style, int appearance, string density, string idiom) { }
                public void RequireGlobal<T>(int tokenId, object resourceKey, string expectedTokenName,
                    AtomUI.Theme.Schema.TokenStage stage, int valueContractVersion) { }
                public void RequireLanguage<T>(int tokenId, string expectedTokenName, AtomUI.Theme.Schema.TokenStage stage,
                    int valueContractVersion) { }
                public void RequireControlGlobal<T>(int controlId, int tokenId, object resourceKey, string expectedTokenName,
                    AtomUI.Theme.Schema.TokenStage stage, int valueContractVersion) { }
                public void RequireOwn<T>(int controlId, int tokenId, object resourceKey, string expectedTokenName,
                    int valueContractVersion) { }
                public void RequireControl(int controlId, AtomUI.Theme.Schema.ControlTokenIdentity identity, string expectedCatalog,
                    string expectedId, string ownerAssemblyIdentity, string ownerMetadataName, int appearance, string density) { }
            }

            public sealed class CompiledThemeValueWriter
            {
                public void BeginSection(int index) { }
                public void SetGlobal<T>(int tokenId, T value, string canonicalValue) { }
                public void SetLanguage<T>(int tokenId, T value, string canonicalValue) { }
                public void SetControlGlobal<T>(int controlId, int tokenId, T value, string canonicalValue) { }
                public void SetOwn<T>(int controlId, int tokenId, T value, string canonicalValue) { }
            }

            public sealed class CompiledThemePackage
            {
                public CompiledThemePackage(string id, System.Collections.Generic.IReadOnlyList<CompiledThemeProgram> programs)
                {
                }
            }
        }
        """;

    // Core's global tokens: a Color seed (the accent), a double seed, a double map token and a string alias.
    internal const string CoreTokenSource = """
        using AtomUI.Theme.DesignTokens;

        namespace AtomUI.Theme.Globals
        {
            [GlobalDesignToken]
            public partial class GlobalToken : AbstractDesignToken
            {
                [DesignTokenKind(DesignTokenKind.Seed)]
                public partial Avalonia.Media.Color ColorPrimary { get; set; }

                [DesignTokenKind(DesignTokenKind.Seed)]
                public partial double FontSize { get; set; }

                [DesignTokenKind(DesignTokenKind.Map)]
                public partial double BorderRadius { get; set; }

                [DesignTokenKind(DesignTokenKind.Alias)]
                public partial string ColorLink { get; set; }
            }
        }
        """;

    // A language package: the language token class (one language seed, two derived tokens) and its language.
    internal const string LanguageSource = """
        using AtomUI.Theme.DesignTokens;
        using AtomUI.Theme.Languages;

        namespace Acme.Shadcn
        {
            [LanguageDesignToken]
            public sealed partial class ShadcnToken : LanguageDesignToken
            {
                [DesignTokenKind(DesignTokenKind.Seed)]
                public partial double BaseScale { get; set; }

                public partial string Ring { get; set; }

                public partial double InputRadius { get; set; }
            }

            [DesignLanguage]
            public sealed class ShadcnLanguage : DesignLanguage<ShadcnToken>
            {
                public override string Id => "Shadcn";

                public override DesignLanguageAxes Axes => new DesignLanguageAxes();

                public override void DeriveLanguage(ShadcnToken language) { }
            }
        }
        """;

    // A control package: ToolTip with a terminal token that has one own token.
    internal const string ControlsSource = """
        using AtomUI.Theme.DesignTokens;

        namespace Acme.Controls
        {
            public sealed class ToolTip : Avalonia.Controls.Control
            {
            }

            [ControlDesignToken]
            public sealed partial class ToolTipToken : AbstractControlDesignToken
            {
                public partial double Padding { get; set; }
            }
        }
        """;

    internal static MetadataReference CoreReference() => Reference(CreateCore(), "AtomUI.Core");

    internal static CSharpCompilation CreateCore() =>
        CSharpCompilation.Create("AtomUI.Core",
            [Parse(RuntimeStubs, "RuntimeStubs.cs"), Parse(CoreTokenSource, "GlobalToken.cs")],
            PlatformReferences(), Options);

    internal static CSharpCompilation CreatePackage(string assemblyName, string source, params MetadataReference[] references) =>
        CSharpCompilation.Create(assemblyName, [Parse(source, assemblyName + ".cs")],
            PlatformReferences().AddRange(references), Options);

    /// <summary>
    /// Runs every generator over <paramref name="compilation" /> and returns it as a reference for the next package.
    /// </summary>
    internal static MetadataReference Reference(CSharpCompilation compilation, string description)
    {
        var output = Run(compilation, out var diagnostics);
        diagnostics.Where(static d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty(description);
        Errors(output).ShouldBeEmpty(description);
        return output.ToMetadataReference();
    }

    internal static CSharpCompilation Run(CSharpCompilation compilation, out ImmutableArray<Diagnostic> diagnostics,
        params CompiledThemeInput[] inputs) =>
        Run(compilation, AllGenerators, out diagnostics, inputs);

    internal static CSharpCompilation Run(CSharpCompilation compilation, IEnumerable<IIncrementalGenerator> generators,
        out ImmutableArray<Diagnostic> diagnostics, params CompiledThemeInput[] inputs)
    {
        var texts = inputs.Select((input, index) => (AdditionalText)new InMemoryAdditionalText(
            "/obj/theme" + index + CompiledThemeInput.Extension, input.Encode())).ToImmutableArray();
        var driver = CSharpGeneratorDriver.Create(generators.Select(static generator => generator.AsSourceGenerator()),
            texts, (CSharpParseOptions)compilation.SyntaxTrees[0].Options, new BuiltinCatalogOptionsProvider(null));
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out diagnostics, TestContext.Current.CancellationToken);
        // These fixtures declare Token shapes without theme assets; ATOMUIGEN042 has dedicated coverage elsewhere.
        diagnostics = diagnostics.Where(static diagnostic => diagnostic.Id != "ATOMUIGEN042").ToImmutableArray();
        return (CSharpCompilation)output;
    }

    internal static string GetGeneratedSource(CSharpCompilation compilation, string fileName) =>
        compilation.SyntaxTrees.Single(tree => tree.FilePath.EndsWith(fileName, StringComparison.Ordinal))
                   .GetText(TestContext.Current.CancellationToken).ToString();

    internal static bool HasGeneratedSource(CSharpCompilation compilation, string fileName) =>
        compilation.SyntaxTrees.Any(tree => tree.FilePath.EndsWith(fileName, StringComparison.Ordinal));

    internal static Diagnostic[] Errors(CSharpCompilation compilation) =>
        compilation.GetDiagnostics(TestContext.Current.CancellationToken)
                   .Where(static d => d.Severity == DiagnosticSeverity.Error).ToArray();

    internal static Diagnostic[] ErrorsIn(CSharpCompilation compilation, string fileName) =>
        Errors(compilation).Where(d => d.Location.SourceTree?.FilePath.EndsWith(fileName, StringComparison.Ordinal) == true).ToArray();

    internal static int CountOf(string source, string value)
    {
        var count = 0;
        for (var index = source.IndexOf(value, StringComparison.Ordinal); index >= 0;
             index = source.IndexOf(value, index + value.Length, StringComparison.Ordinal))
        {
            count++;
        }
        return count;
    }

    internal static ImmutableArray<MetadataReference> PlatformReferences() =>
        ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))!.Split(Path.PathSeparator)
            .Select(static path => (MetadataReference)MetadataReference.CreateFromFile(path)).ToImmutableArray();

    private static readonly CSharpCompilationOptions Options = new(OutputKind.DynamicallyLinkedLibrary);

    private static SyntaxTree Parse(string source, string path) =>
        CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview), path,
            cancellationToken: TestContext.Current.CancellationToken);

    private sealed class InMemoryAdditionalText(string path, string text) : AdditionalText
    {
        private readonly SourceText _text = SourceText.From(text);
        public override string Path { get; } = path;
        public override SourceText GetText(CancellationToken cancellationToken = default) => _text;
    }
}
