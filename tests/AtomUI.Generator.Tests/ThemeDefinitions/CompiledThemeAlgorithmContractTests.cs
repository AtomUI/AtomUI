using System.Collections.Immutable;
using AtomUI.Theme.Build;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Shouldly;
using Xunit;

namespace AtomUI.Generator.Tests.ThemeDefinitions;

/// <summary>
/// Contract tests for the algorithm records of the theme build contract: every assembly that declares
/// algorithms publishes them, a compiled theme package reads them from its direct references, and a theme
/// XML that references an unknown algorithm fails the build at the element's line and column.
/// </summary>
public class CompiledThemeAlgorithmContractTests
{
    private const string ContractsFile = "GeneratedThemeBuildContracts.g.cs";
    private const string PackageFile = "GeneratedThemePackageRegistration.g.cs";

    [Fact]
    public void Writes_Algorithm_Build_Contracts_For_Local_Algorithms()
    {
        // Core declares the contract attribute in source, so the written attribute must bind against it.
        var core = RunGenerator(CreateCoreCompilation(), out var coreDiagnostics);

        coreDiagnostics.Where(static d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        var coreContracts = GetGeneratedSource(core, ContractsFile);
        coreContracts.ShouldContain("[assembly: global::AtomUI.Theme.Definitions.Compiled.ThemeBuildAlgorithmAttribute(1, \"AtomUI\", \"Compact\", 1, 0)]");
        coreContracts.ShouldContain("[assembly: global::AtomUI.Theme.Definitions.Compiled.ThemeBuildAlgorithmAttribute(1, \"AtomUI\", \"Dark\", 1, 2)]");
        coreContracts.ShouldContain("[assembly: global::AtomUI.Theme.Definitions.Compiled.ThemeBuildAlgorithmAttribute(1, \"AtomUI\", \"Default\", 1, 1)]");
        CompileErrorsIn(core, ContractsFile).ShouldBeEmpty();

        // A third-party package publishes its own algorithms and does not repeat the ones it references.
        var package = RunGenerator(CreatePackageCompilation(PackageAlgorithmSource, GeneratedCoreReference()), out var packageDiagnostics);

        packageDiagnostics.Where(static d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        var packageContracts = GetGeneratedSource(package, ContractsFile);
        packageContracts.ShouldContain("[assembly: global::AtomUI.Theme.Definitions.Compiled.ThemeBuildAlgorithmAttribute(1, \"PrimeNG\", \"Aura\", 3, 1)]");
        packageContracts.ShouldContain("[assembly: global::AtomUI.Theme.Definitions.Compiled.ThemeBuildAlgorithmAttribute(1, \"PrimeNG\", \"Aura.Dark\", 1, 2)]");
        packageContracts.ShouldNotContain("\"AtomUI\", \"Default\"");
        packageContracts.IndexOf("\"Aura\"", StringComparison.Ordinal).ShouldBeLessThan(packageContracts.IndexOf("\"Aura.Dark\"", StringComparison.Ordinal));
        CompileErrorsIn(package, ContractsFile).ShouldBeEmpty();
    }

    [Fact]
    public void Reads_Algorithm_Build_Contracts_From_Referenced_Assemblies()
    {
        var runtime = CreateRuntimeReference("""
            [assembly: AtomUI.Theme.Definitions.Compiled.ThemeBuildAlgorithmAttribute(1, "AtomUI", "Default", 1, 1)]
            [assembly: AtomUI.Theme.Definitions.Compiled.ThemeBuildAlgorithmAttribute(1, "PrimeNG", "Aura", 3, 2)]
            [assembly: AtomUI.Theme.Definitions.Compiled.ThemeBuildAlgorithmAttribute(2, "PrimeNG", "Future", 1, 1)]
            [assembly: AtomUI.Theme.Definitions.Compiled.ThemeBuildAlgorithmAttribute(1, "PrimeNG", "1Bad", 1, 1)]
            """);
        var compilation = CreatePackageCompilation(PackageAlgorithmSource, runtime);
        var output = new GenerationOutput();

        var contracts = ThemeBuildContracts.Build(compilation, "Acme", [], [], Algorithms(compilation), output);

        contracts.Algorithms.Keys.OrderBy(static key => key, StringComparer.Ordinal)
                 .ShouldBe(["AtomUI\u001fDefault", "PrimeNG\u001fAura", "PrimeNG\u001fAura.Dark"]);
        contracts.Algorithms["PrimeNG\u001fAura"].Revision.ShouldBe(3);
        contracts.Algorithms["PrimeNG\u001fAura"].AppearanceEffect.ShouldBe(2);
        contracts.LocalAlgorithms.Select(static a => a.Catalog + ":" + a.Id).ShouldBe(["PrimeNG:Aura.Dark"]);
        // The local Aura repeats the referenced one; an unsupported version and an invalid identity are rejected.
        Messages(output).ShouldBe([
            "Invalid Token build metadata in 'AtomUI.Core, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null': Unsupported Token build metadata version.",
            "Invalid Token build metadata in 'AtomUI.Core, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null': Invalid algorithm identity/revision/appearance effect.",
            "Duplicate algorithm build contract: PrimeNG:Aura"
        ], ignoreOrder: true);
    }

    [Fact]
    public void Reports_Unknown_Algorithm_In_Theme_Xml_With_Line_And_Column()
    {
        var input = CreateInput();
        input.Algorithms =
        [
            new ThemeAlgorithmInput { Catalog = "AtomUI", Id = "Default", Line = 3, Column = 5 },
            new ThemeAlgorithmInput { Catalog = "PrimeNG", Id = "Missing", Line = 4, Column = 7 }
        ];

        var package = RunGenerator(CreatePackageCompilation(ConsumerSource, GeneratedCoreReference()), out var diagnostics, input);

        var failure = diagnostics.Where(static d => d.Id == "ATOMUITOKEN002").ShouldHaveSingleItem();
        failure.GetMessage(null).ShouldBe("Unknown algorithm 'PrimeNG:Missing'.");
        var span = failure.Location.GetLineSpan();
        span.Path.ShouldBe(input.SourcePath);
        span.StartLinePosition.ShouldBe(new LinePosition(3, 6));
        package.SyntaxTrees.ShouldNotContain(tree => tree.FilePath.EndsWith(PackageFile, StringComparison.Ordinal));
    }

    [Fact]
    public void Reports_Unknown_Algorithm_In_A_Control_Custom_Chain()
    {
        var input = CreateInput();
        input.Controls =
        [
            new ThemeControlInput
            {
                Catalog = "AtomUI", Id = "Button", Mode = 1, Line = 8, Column = 5,
                Algorithms =
                [
                    new ThemeAlgorithmInput { Catalog = "AtomUI", Id = "Compact", Line = 9, Column = 9 },
                    new ThemeAlgorithmInput { Catalog = "PrimeNG", Id = "Missing", Line = 10, Column = 11 }
                ]
            }
        ];

        var package = RunGenerator(CreatePackageCompilation(ConsumerSource + ControlSource, GeneratedCoreReference()), out var diagnostics, input);

        var failure = diagnostics.Where(static d => d.Id == "ATOMUITOKEN002").ShouldHaveSingleItem();
        failure.GetMessage(null).ShouldBe("Unknown algorithm 'PrimeNG:Missing'.");
        var span = failure.Location.GetLineSpan();
        span.Path.ShouldBe(input.SourcePath);
        span.StartLinePosition.ShouldBe(new LinePosition(9, 10));
        package.SyntaxTrees.ShouldNotContain(tree => tree.FilePath.EndsWith(PackageFile, StringComparison.Ordinal));
    }

    [Fact]
    public void Accepts_A_BuiltIn_Algorithm_From_The_Core_Contract()
    {
        var input = CreateInput();
        input.Algorithms =
        [
            new ThemeAlgorithmInput { Catalog = "AtomUI", Id = "Dark", Line = 3, Column = 5 },
            new ThemeAlgorithmInput { Catalog = "AtomUI", Id = "Compact", Line = 4, Column = 5 }
        ];

        var package = RunGenerator(CreatePackageCompilation(ConsumerSource, GeneratedCoreReference()), out var diagnostics, input);

        diagnostics.Where(static d => d.Id == "ATOMUITOKEN002").ShouldBeEmpty();
        GetGeneratedSource(package, PackageFile).ShouldContain(
            "builder.SetAlgorithms(new global::AtomUI.Theme.Algorithms.ThemeAlgorithm[] { new global::AtomUI.Theme.Algorithms.ThemeAlgorithm(\"AtomUI\", \"Dark\"), new global::AtomUI.Theme.Algorithms.ThemeAlgorithm(\"AtomUI\", \"Compact\") });");
    }

    private static CompiledThemeInput CreateInput() => new()
    {
        SourceIdentity = "Themes/Aura.xml",
        SourcePath = "/repo/Themes/Aura.xml",
        ContentDigest = new string('A', 64),
        Id = "Aura",
        Name = "Aura",
        Appearance = 0
    };

    private static ThemeAlgorithmInfo[] Algorithms(Compilation compilation) =>
        compilation.SyntaxTrees.SelectMany(tree => tree.GetRoot().DescendantNodes()
                       .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>()
                       .Select(node => compilation.GetSemanticModel(tree).GetDeclaredSymbol(node)))
                   .Where(static symbol => symbol is not null)
                   .Select(static symbol => ThemeAlgorithmInfo.Create(symbol!))
                   .Where(static info => info is not null)
                   .Select(static info => info!)
                   .ToArray();

    private static string[] Messages(GenerationOutput output)
    {
        var sink = new List<string>();
        foreach (var diagnostic in output.Freeze().Diagnostics) { sink.Add(diagnostic.Message); }
        return sink.ToArray();
    }

    internal static MetadataReference GeneratedCoreReference() =>
        RunGenerator(CreateCoreCompilation(), out _).ToMetadataReference();

    internal static CSharpCompilation RunGenerator(CSharpCompilation compilation, out ImmutableArray<Diagnostic> diagnostics,
        params CompiledThemeInput[] inputs)
    {
        var texts = inputs.Select((input, index) => (AdditionalText)new InMemoryAdditionalText(
            "/obj/theme" + index + CompiledThemeInput.Extension, input.Encode())).ToImmutableArray();
        var driver = CSharpGeneratorDriver.Create([new TokenResourceKeyGenerator().AsSourceGenerator()],
            texts, (CSharpParseOptions)compilation.SyntaxTrees[0].Options, new BuiltinCatalogOptionsProvider(null));
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out diagnostics, TestContext.Current.CancellationToken);
        // These fixtures declare Token shapes without theme assets; ATOMUIGEN042 has dedicated coverage elsewhere.
        diagnostics = diagnostics.Where(static diagnostic => diagnostic.Id != "ATOMUIGEN042").ToImmutableArray();
        return (CSharpCompilation)output;
    }

    private static string GetGeneratedSource(CSharpCompilation compilation, string fileName) =>
        compilation.SyntaxTrees.Single(tree => tree.FilePath.EndsWith(fileName, StringComparison.Ordinal))
                   .GetText(TestContext.Current.CancellationToken).ToString();

    private static Diagnostic[] CompileErrorsIn(CSharpCompilation compilation, string fileName) =>
        compilation.GetDiagnostics(TestContext.Current.CancellationToken)
                   .Where(d => d.Severity == DiagnosticSeverity.Error && d.Location.SourceTree?.FilePath.EndsWith(fileName, StringComparison.Ordinal) == true)
                   .ToArray();

    private static ImmutableArray<MetadataReference> PlatformReferences() =>
        ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))!.Split(Path.PathSeparator)
            .Select(path => (MetadataReference)MetadataReference.CreateFromFile(path)).ToImmutableArray();

    // Core: the runtime stubs, the contract attributes and the three built-in algorithms all come from source.
    private static CSharpCompilation CreateCoreCompilation() =>
        CSharpCompilation.Create("AtomUI.Core",
            [CSharpSyntaxTree.ParseText(RuntimeStubs, path: "RuntimeStubs.cs"), CSharpSyntaxTree.ParseText(BuiltInAlgorithmSource, path: "BuiltIn.cs")],
            PlatformReferences(), new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    // A hand-written runtime whose assembly attributes stand for metadata published by a referenced package.
    private static MetadataReference CreateRuntimeReference(string assemblyAttributes) =>
        CSharpCompilation.Create("AtomUI.Core",
            [CSharpSyntaxTree.ParseText(RuntimeStubs), CSharpSyntaxTree.ParseText(assemblyAttributes)],
            PlatformReferences(), new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)).ToMetadataReference();

    internal static CSharpCompilation CreatePackageCompilation(string source, MetadataReference runtime) =>
        CSharpCompilation.Create("AtomUI.Themes.PrimeNG",
            [CSharpSyntaxTree.ParseText(source, path: "PackageSource.cs")],
            PlatformReferences().Add(runtime), new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    // The schema-generator stubs plus the compiled-theme ABI: the 7-parameter CreateGenerated and the contract attributes.
    private static readonly string RuntimeStubs = ThemeSchemaGeneratorTests.AtomUIStubs.Replace(
        "public sealed class TokenDescriptor", "public sealed partial class TokenDescriptor", StringComparison.Ordinal) + """

        namespace AtomUI.Theme.Schema
        {
            public sealed partial class TokenDescriptor
            {
                public static TokenDescriptor CreateGenerated(string name, int slot, TokenStage stage, TokenValueCodec codec,
                    object resourceKey, TokenValueAccessor accessor, int contract) => null!;
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
            public sealed class ThemeBuildAlgorithmAttribute(int version, string catalog, string id, int revision, int appearanceEffect) : System.Attribute { }

            [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple = true)]
            public sealed class CompiledThemeManifestAttribute(int version, string packageId, string data) : System.Attribute { }
        }
        """;

    private const string BuiltInAlgorithmSource = """
        using AtomUI.Theme.Algorithms;
        using AtomUI.Theme.Schema;

        namespace AtomUI.Theme.Algorithms
        {
            [ThemeAlgorithm("AtomUI", "Default", 1, ThemeAppearanceEffect.Light)]
            internal sealed class DefaultThemeVariantCalculator : IThemeAlgorithm { }

            [ThemeAlgorithm("AtomUI", "Dark", 1, ThemeAppearanceEffect.Dark)]
            internal sealed class DarkThemeVariantCalculator : IThemeAlgorithm { }

            [ThemeAlgorithm("AtomUI", "Compact", 1, ThemeAppearanceEffect.Preserve)]
            internal sealed class CompactThemeVariantCalculator : IThemeAlgorithm { }
        }
        """;

    private const string PackageAlgorithmSource = """
        using AtomUI.Theme.Algorithms;
        using AtomUI.Theme.Schema;

        namespace Acme.Themes
        {
            [ThemeAlgorithm("PrimeNG", "Aura.Dark", 1, ThemeAppearanceEffect.Dark)]
            public sealed class AuraDarkAlgorithm : IThemeAlgorithm { }

            [ThemeAlgorithm("PrimeNG", "Aura", 3, ThemeAppearanceEffect.Light)]
            public sealed class AuraAlgorithm : IThemeAlgorithm { }
        }
        """;

    internal const string ConsumerSource = """
        namespace Acme.Themes
        {
            public sealed class Marker { }
        }
        """;

    private const string ControlSource = """

        namespace Demo
        {
            [AtomUI.Theme.DesignTokens.ControlDesignToken]
            internal sealed class ButtonToken : AtomUI.Theme.DesignTokens.AbstractControlDesignToken
            {
                public double Height { get; set; }

                public override void CalculateTokenValues(bool isDark)
                {
                    Height = EffectiveGlobalToken.ControlHeight;
                }
            }
        }
        """;

    private sealed class InMemoryAdditionalText(string path, string text) : AdditionalText
    {
        private readonly SourceText _text = SourceText.From(text);
        public override string Path { get; } = path;
        public override SourceText GetText(CancellationToken cancellationToken = default) => _text;
    }
}
