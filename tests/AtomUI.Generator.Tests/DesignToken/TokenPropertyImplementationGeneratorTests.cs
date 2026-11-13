using System.Collections.Immutable;
using AtomUI.Generator.DesignToken;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;

namespace AtomUI.Generator.Tests.DesignToken;

public class TokenPropertyImplementationGeneratorTests
{
    private const string DiagnosticId = "ATOMUIGEN049";

    [Fact]
    public void Emits_Field_Backed_Getter_And_Recording_Setter_For_Each_Token_Property()
    {
        var output = Run(Compile(("ButtonToken.cs", """
            using AtomUI.Theme.DesignTokens;

            namespace Demo
            {
                [ControlDesignToken]
                public sealed partial class ButtonToken : AbstractControlDesignToken
                {
                    public partial double Height { get; set; }
                    public partial string? Label { get; set; }
                }
            }
            """)), out var diagnostics);

        diagnostics.ShouldBeEmpty();
        AssertNoErrors(output);
        var source = GeneratedSource(output, "Demo.ButtonToken.TokenProperties.g.cs");
        source.ShouldContain("namespace Demo");
        source.ShouldContain("partial class ButtonToken");
        source.ShouldContain("public partial double Height");
        source.ShouldContain("public partial string? Label");
        source.ShouldContain("get => field;");
        source.ShouldContain("RecordWrite(nameof(Height));");
        source.ShouldContain("RecordWrite(nameof(Label));");
        source.ShouldContain("set { field = value; RecordWrite(nameof(Height)); }");
    }

    [Fact]
    public void Emits_Matching_Accessibility_For_Internal_Token_Properties()
    {
        var output = Run(Compile(("DataGridToken.cs", """
            using AtomUI.Theme.DesignTokens;

            namespace Demo
            {
                [ControlDesignToken]
                internal sealed partial class DataGridToken : AbstractControlDesignToken
                {
                    internal partial double RowHeight { get; set; }
                    public partial double HeaderHeight { get; set; }
                    public partial double FooterHeight { get; private set; }
                }
            }
            """)), out var diagnostics);

        diagnostics.ShouldBeEmpty();
        AssertNoErrors(output);
        var source = GeneratedSource(output, "Demo.DataGridToken.TokenProperties.g.cs");
        source.ShouldContain("internal partial double RowHeight");
        source.ShouldContain("public partial double HeaderHeight");
        source.ShouldContain("private set { field = value; RecordWrite(nameof(FooterHeight)); }");
    }

    [Fact]
    public void Emits_RunDefaultCalculation_Only_On_Sealed_Control_Tokens()
    {
        var output = Run(Compile(
            ("AbstractButtonToken.cs", """
                using AtomUI.Theme.DesignTokens;

                namespace Demo
                {
                    [ControlDesignToken]
                    public abstract partial class AbstractButtonToken : AbstractControlDesignToken
                    {
                        public partial double Height { get; set; }
                    }

                    [ControlDesignToken]
                    public sealed partial class ButtonToken : AbstractButtonToken
                    {
                        public partial double IconSize { get; set; }

                        protected override void CalculateTokenValues(bool isDarkMode)
                        {
                            base.CalculateTokenValues(isDarkMode);
                            IconSize = Height / 2;
                        }
                    }
                }
                """),
            ("DesignToken.Seed.cs", """
                using AtomUI.Theme.DesignTokens;

                namespace AtomUI.Theme.DesignTokens
                {
                    [GlobalDesignToken]
                    public partial class DesignToken : AbstractDesignToken
                    {
                        public partial double FontSize { get; set; }
                    }
                }
                """)), out var diagnostics);

        diagnostics.ShouldBeEmpty();
        AssertNoErrors(output);
        const string entry = "internal void RunDefaultCalculation(bool isDarkMode) => CalculateTokenValues(isDarkMode);";
        GeneratedSource(output, "Demo.ButtonToken.TokenProperties.g.cs").ShouldContain(entry);
        GeneratedSource(output, "Demo.AbstractButtonToken.TokenProperties.g.cs").ShouldNotContain("RunDefaultCalculation");
        GeneratedSource(output, "AtomUI.Theme.DesignTokens.DesignToken.Seed.TokenProperties.g.cs").ShouldNotContain("RunDefaultCalculation");
    }

    [Fact]
    public void Emits_Implementations_For_Abstract_Layers_In_Their_Own_Assembly()
    {
        var baseOutput = Run(Compile("Demo.Controls", ("AbstractSkeletonToken.cs", """
            using AtomUI.Theme.DesignTokens;

            namespace Demo.Controls
            {
                [ControlDesignToken]
                public abstract partial class AbstractSkeletonToken : AbstractControlDesignToken
                {
                    public partial double BlockRadius { get; set; }
                }
            }
            """)), out var baseDiagnostics);
        baseDiagnostics.ShouldBeEmpty();
        AssertNoErrors(baseOutput);
        GeneratedSource(baseOutput, "Demo.Controls.AbstractSkeletonToken.TokenProperties.g.cs")
            .ShouldContain("RecordWrite(nameof(BlockRadius));");

        var derived = CSharpCompilation.Create(
            "Demo.Desktop.Controls",
            [Parse("SkeletonToken.cs", """
                using AtomUI.Theme.DesignTokens;

                namespace Demo.Desktop.Controls
                {
                    [ControlDesignToken]
                    public sealed partial class SkeletonToken : Demo.Controls.AbstractSkeletonToken
                    {
                        public partial double TitleHeight { get; set; }
                    }
                }
                """)],
            PlatformReferences().Add(Emit(baseOutput)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var derivedOutput = Run(derived, out var derivedDiagnostics);

        derivedDiagnostics.ShouldBeEmpty();
        AssertNoErrors(derivedOutput);
        var generated = GeneratedTrees(derivedOutput);
        generated.Select(static tree => Path.GetFileName(tree.FilePath)).ShouldBe(["Demo.Desktop.Controls.SkeletonToken.TokenProperties.g.cs"]);
        var source = generated[0].GetText(TestContext.Current.CancellationToken).ToString();
        source.ShouldContain("RecordWrite(nameof(TitleHeight));");
        source.ShouldNotContain("BlockRadius");
    }

    [Fact]
    public void Skips_NotTokenDefinition_Properties()
    {
        var output = Run(Compile(
            ("DesignToken.cs", """
                using System.Collections.Generic;
                using AtomUI.Theme.DesignTokens;

                namespace AtomUI.Theme.DesignTokens
                {
                    public partial class DesignToken : AbstractDesignToken
                    {
                        [NotTokenDefinition] public IDictionary<string, string> ColorPalettes { get; set; } = new Dictionary<string, string>();
                    }
                }
                """),
            ("DesignToken.Seed.cs", """
                using AtomUI.Theme.DesignTokens;

                namespace AtomUI.Theme.DesignTokens
                {
                    [GlobalDesignToken]
                    public partial class DesignToken
                    {
                        public partial double FontSize { get; set; }
                        [NotTokenDefinition] public double Helper { get; set; }
                    }
                }
                """),
            ("DesignToken.Alias.cs", """
                using AtomUI.Theme.DesignTokens;

                namespace AtomUI.Theme.DesignTokens
                {
                    [GlobalDesignToken]
                    public partial class DesignToken
                    {
                        public partial double LineHeight { get; set; }
                    }
                }
                """),
            ("ButtonToken.cs", """
                using AtomUI.Theme.DesignTokens;

                namespace Demo
                {
                    [ControlDesignToken]
                    public sealed partial class ButtonToken : AbstractControlDesignToken
                    {
                        public partial double Height { get; set; }
                        [NotTokenDefinition] public double Scratch { get; set; }
                    }
                }
                """)), out var diagnostics);

        diagnostics.ShouldBeEmpty();
        AssertNoErrors(output);
        GeneratedTrees(output).Select(static tree => Path.GetFileName(tree.FilePath)).OrderBy(static name => name, StringComparer.Ordinal)
            .ShouldBe([
                "AtomUI.Theme.DesignTokens.DesignToken.Alias.TokenProperties.g.cs",
                "AtomUI.Theme.DesignTokens.DesignToken.Seed.TokenProperties.g.cs",
                "Demo.ButtonToken.TokenProperties.g.cs"
            ]);
        var seed = GeneratedSource(output, "AtomUI.Theme.DesignTokens.DesignToken.Seed.TokenProperties.g.cs");
        seed.ShouldContain("RecordWrite(nameof(FontSize));");
        seed.ShouldNotContain("LineHeight");
        seed.ShouldNotContain("Helper");
        seed.ShouldNotContain("ColorPalettes");
        var alias = GeneratedSource(output, "AtomUI.Theme.DesignTokens.DesignToken.Alias.TokenProperties.g.cs");
        alias.ShouldContain("RecordWrite(nameof(LineHeight));");
        alias.ShouldNotContain("FontSize");
        GeneratedSource(output, "Demo.ButtonToken.TokenProperties.g.cs").ShouldNotContain("Scratch");
    }

    [Fact]
    public void Reports_ATOMUIGEN049_For_A_Non_Partial_Token_Property()
    {
        const string source = """
            using AtomUI.Theme.DesignTokens;

            namespace Demo
            {
                [ControlDesignToken]
                public sealed partial class ButtonToken : AbstractControlDesignToken
                {
                    public partial double Height { get; set; }
                    public double Width { get; set; }
                }
            }
            """;

        var output = Run(Compile(("ButtonToken.cs", source)), out var diagnostics);

        var diagnostic = diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe(DiagnosticId);
        diagnostic.Severity.ShouldBe(DiagnosticSeverity.Error);
        diagnostic.GetMessage(System.Globalization.CultureInfo.InvariantCulture).ShouldBe(
            "Token property 'Width' on 'Demo.ButtonToken' must be declared partial, and 'Demo.ButtonToken' must be a partial class");
        diagnostic.Location.GetLineSpan().Path.ShouldEndWith("ButtonToken.cs");
        diagnostic.Location.GetLineSpan().StartLinePosition.Line.ShouldBe(8);
        var generated = GeneratedSource(output, "Demo.ButtonToken.TokenProperties.g.cs");
        generated.ShouldContain("RecordWrite(nameof(Height));");
        generated.ShouldNotContain("Width");
    }

    [Fact]
    public void Reports_ATOMUIGEN049_For_A_Non_Partial_Token_Class()
    {
        var output = Run(Compile(("ButtonToken.cs", """
            using AtomUI.Theme.DesignTokens;

            namespace Demo
            {
                [ControlDesignToken]
                public sealed class ButtonToken : AbstractControlDesignToken
                {
                    public double Height { get; set; } = 32;
                    public double Width { get; set; }
                }
            }
            """)), out var diagnostics);

        diagnostics.Select(static diagnostic => diagnostic.Id).ShouldBe([DiagnosticId, DiagnosticId]);
        diagnostics.Select(static diagnostic => diagnostic.GetMessage(System.Globalization.CultureInfo.InvariantCulture))
            .ShouldBe([
                "Token property 'Height' on 'Demo.ButtonToken' must be declared partial, and 'Demo.ButtonToken' must be a partial class",
                "Token property 'Width' on 'Demo.ButtonToken' must be declared partial, and 'Demo.ButtonToken' must be a partial class"
            ]);
        GeneratedTrees(output).ShouldBeEmpty();
        AssertNoErrors(output);
    }

    [Fact]
    public void Keeps_An_Initializer_On_The_Declaring_Half_Compilable()
    {
        var output = Run(Compile(("ButtonToken.cs", """
            using AtomUI.Theme.DesignTokens;

            namespace Demo
            {
                [ControlDesignToken]
                public sealed partial class ButtonToken : AbstractControlDesignToken
                {
                    public partial double Height { get; set; } = 32;
                    public partial string Label { get; set; } = "OK";
                }
            }
            """)), out var diagnostics);

        diagnostics.ShouldBeEmpty();
        output.GetDiagnostics(TestContext.Current.CancellationToken).ShouldBeEmpty();
        GeneratedSource(output, "Demo.ButtonToken.TokenProperties.g.cs").ShouldNotContain("= 32");
    }

    [Fact]
    public void Same_Named_Token_Classes_In_Different_Namespaces_Get_Distinct_Files()
    {
        var output = Run(Compile(
            ("Alpha/ButtonToken.cs", """
                using AtomUI.Theme.DesignTokens;

                namespace Demo.Alpha
                {
                    [ControlDesignToken]
                    public sealed partial class ButtonToken : AbstractControlDesignToken
                    {
                        public partial double Height { get; set; }
                    }
                }
                """),
            ("Beta/ButtonToken.cs", """
                using AtomUI.Theme.DesignTokens;

                namespace Demo.Beta
                {
                    [ControlDesignToken]
                    public sealed partial class ButtonToken : AbstractControlDesignToken
                    {
                        public partial double Width { get; set; }
                    }
                }
                """)), out var diagnostics);

        diagnostics.ShouldBeEmpty();
        AssertNoErrors(output);
        GeneratedSource(output, "Demo.Alpha.ButtonToken.TokenProperties.g.cs").ShouldContain("RecordWrite(nameof(Height));");
        GeneratedSource(output, "Demo.Beta.ButtonToken.TokenProperties.g.cs").ShouldContain("RecordWrite(nameof(Width));");
    }

    [Fact]
    public void Pipeline_Output_Is_Cached_When_An_Unrelated_File_Changes()
    {
        var compilation = Compile(("ButtonToken.cs", """
            using AtomUI.Theme.DesignTokens;

            namespace Demo
            {
                [ControlDesignToken]
                public sealed partial class ButtonToken : AbstractControlDesignToken
                {
                    public partial double Height { get; set; }
                }
            }
            """), ("Other.cs", "namespace Demo { public sealed class Other { } }"));
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [new TokenPropertyImplementationGenerator().AsSourceGenerator()],
            parseOptions: ParseOptions,
            driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true));
        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        var other = compilation.SyntaxTrees.Single(static tree => tree.FilePath.EndsWith("Other.cs", StringComparison.Ordinal));
        var edited = compilation.ReplaceSyntaxTree(other,
            Parse("Other.cs", "namespace Demo { public sealed class Other { public int Value; } }"));
        driver = driver.RunGenerators(edited, TestContext.Current.CancellationToken);

        var steps = driver.GetRunResult().Results.Single().TrackedSteps[TokenPropertyImplementationGenerator.ModelStepName];
        steps.SelectMany(static step => step.Outputs).Select(static output => output.Reason)
            .ShouldAllBe(static reason => reason == IncrementalStepRunReason.Cached || reason == IncrementalStepRunReason.Unchanged);
    }

    private static readonly CSharpParseOptions ParseOptions = new(LanguageVersion.Latest);

    private const string Stubs = """
        namespace AtomUI.Theme.DesignTokens
        {
            [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
            public sealed class ControlDesignTokenAttribute : System.Attribute { }

            [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
            public class GlobalDesignTokenAttribute : System.Attribute { }

            [System.AttributeUsage(System.AttributeTargets.Property, Inherited = false)]
            public class NotTokenDefinitionAttribute : System.Attribute { }

            public abstract class AbstractDesignToken
            {
                protected void RecordWrite(string tokenName) { }
            }

            public abstract class AbstractControlDesignToken : AbstractDesignToken
            {
                protected virtual void CalculateTokenValues(bool isDarkMode) { }
            }
        }
        """;

    private static CSharpCompilation Compile(params (string Path, string Source)[] files) =>
        Compile("TokenPropertyImplementationGeneratorTests", files);

    private static CSharpCompilation Compile(string assemblyName, params (string Path, string Source)[] files) =>
        CSharpCompilation.Create(
            assemblyName,
            files.Select(static file => Parse(file.Path, file.Source)).Append(Parse("Stubs.cs", Stubs)),
            PlatformReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

    private static SyntaxTree Parse(string path, string source) =>
        CSharpSyntaxTree.ParseText(source, ParseOptions, "/src/" + path,
            cancellationToken: TestContext.Current.CancellationToken);

    private static ImmutableArray<MetadataReference> PlatformReferences() =>
        ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))!
        .Split(Path.PathSeparator)
        .Select(static path => (MetadataReference)MetadataReference.CreateFromFile(path))
        .ToImmutableArray();

    private static CSharpCompilation Run(CSharpCompilation compilation, out ImmutableArray<Diagnostic> diagnostics)
    {
        var driver = CSharpGeneratorDriver.Create([new TokenPropertyImplementationGenerator().AsSourceGenerator()],
            parseOptions: ParseOptions);
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out diagnostics,
            TestContext.Current.CancellationToken);
        return (CSharpCompilation)output;
    }

    private static SyntaxTree[] GeneratedTrees(CSharpCompilation compilation) =>
        compilation.SyntaxTrees.Where(static tree => tree.FilePath.EndsWith(".g.cs", StringComparison.Ordinal)).ToArray();

    private static string GeneratedSource(CSharpCompilation compilation, string fileName) =>
        GeneratedTrees(compilation)
            .Single(tree => Path.GetFileName(tree.FilePath) == fileName)
            .GetText(TestContext.Current.CancellationToken)
            .ToString();

    private static void AssertNoErrors(CSharpCompilation compilation) =>
        compilation.GetDiagnostics(TestContext.Current.CancellationToken)
                   .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
                   .ShouldBeEmpty();

    private static MetadataReference Emit(CSharpCompilation compilation)
    {
        using var stream = new MemoryStream();
        var result = compilation.Emit(stream, cancellationToken: TestContext.Current.CancellationToken);
        result.Success.ShouldBeTrue(string.Join(Environment.NewLine, result.Diagnostics));
        return MetadataReference.CreateFromImage(stream.ToArray());
    }
}
