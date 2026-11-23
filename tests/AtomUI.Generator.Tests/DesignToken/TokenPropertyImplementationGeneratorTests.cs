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
    public void Emits_Field_Backed_Getter_And_Freeze_Checking_Setter_For_Each_Token_Property()
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
        source.ShouldContain("get { OnRead(nameof(Height)); return field; }");
        source.ShouldContain("if (IsFrozen(nameof(Height))) return; field = value;");
        source.ShouldContain("if (IsFrozen(nameof(Label))) return; field = value;");
        source.ShouldContain("set { if (IsFrozen(nameof(Height))) return; field = value; }");
        source.ShouldNotContain("RecordWrite");
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
        source.ShouldContain("private set { if (IsFrozen(nameof(FooterHeight))) return; field = value; }");
    }

    [Fact]
    public void Emits_Only_Property_Halves_For_Abstract_Sealed_And_Global_Token_Declarations()
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
        var button = GeneratedSource(output, "Demo.ButtonToken.TokenProperties.g.cs");
        button.ShouldContain("if (IsFrozen(nameof(IconSize))) return; field = value;");
        button.ShouldNotContain("Height");
        GeneratedSource(output, "Demo.AbstractButtonToken.TokenProperties.g.cs")
            .ShouldContain("if (IsFrozen(nameof(Height))) return; field = value;");
        GeneratedSource(output, "AtomUI.Theme.DesignTokens.DesignToken.Seed.TokenProperties.g.cs")
            .ShouldContain("if (IsFrozen(nameof(FontSize))) return; field = value;");
        // The calculation entry point went away with the theme algorithms.
        GeneratedTrees(output).Select(static tree => tree.GetText(TestContext.Current.CancellationToken).ToString())
            .ShouldAllBe(static source => !source.Contains("RunDefaultCalculation"));
    }

    [Fact]
    public void Emits_Freeze_Checking_Setters_For_Language_Token_Properties()
    {
        var output = Run(Compile(("ShadcnToken.cs", """
            using AtomUI.Theme.DesignTokens;

            namespace Acme.Languages
            {
                [LanguageDesignToken]
                public sealed partial class ShadcnToken : LanguageDesignToken
                {
                    [DesignTokenKind(DesignTokenKind.Seed)]
                    public partial string BaseColor { get; set; }

                    public partial double InputRadius { get; set; }

                    [NotTokenDefinition] public double Scratch { get; set; }
                }
            }
            """)), out var diagnostics);

        diagnostics.ShouldBeEmpty();
        AssertNoErrors(output);
        GeneratedTrees(output).Select(static tree => Path.GetFileName(tree.FilePath))
            .ShouldBe(["Acme.Languages.ShadcnToken.TokenProperties.g.cs"]);
        var source = GeneratedSource(output, "Acme.Languages.ShadcnToken.TokenProperties.g.cs");
        source.ShouldContain("namespace Acme.Languages");
        source.ShouldContain("partial class ShadcnToken");
        // A language seed gets the same setter: seeds are frozen the same way as derived values.
        source.ShouldContain("public partial string BaseColor");
        source.ShouldContain("set { if (IsFrozen(nameof(BaseColor))) return; field = value; }");
        source.ShouldContain("public partial double InputRadius");
        source.ShouldContain("set { if (IsFrozen(nameof(InputRadius))) return; field = value; }");
        source.ShouldNotContain("Scratch");
    }

    [Fact]
    public void Emits_A_Derived_Language_Token_Without_Repeating_The_Base_Language_Properties()
    {
        var baseOutput = Run(Compile("Acme.AntDesign", ("AntDesignToken.cs", """
            using AtomUI.Theme.DesignTokens;

            namespace Acme.AntDesign
            {
                [LanguageDesignToken]
                public partial class AntDesignToken : LanguageDesignToken
                {
                    public partial double TableRadius { get; set; }
                }
            }
            """)), out var baseDiagnostics);
        baseDiagnostics.ShouldBeEmpty();
        AssertNoErrors(baseOutput);

        var derived = CSharpCompilation.Create(
            "Acme.Brand",
            [Parse("BrandToken.cs", """
                using AtomUI.Theme.DesignTokens;

                namespace Acme.Brand
                {
                    [LanguageDesignToken]
                    public sealed partial class BrandToken : Acme.AntDesign.AntDesignToken
                    {
                        public partial double BrandRadius { get; set; }
                    }
                }
                """)],
            PlatformReferences().Add(Emit(baseOutput)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var derivedOutput = Run(derived, out var derivedDiagnostics);

        derivedDiagnostics.ShouldBeEmpty();
        AssertNoErrors(derivedOutput);
        var generated = GeneratedTrees(derivedOutput);
        generated.Select(static tree => Path.GetFileName(tree.FilePath)).ShouldBe(["Acme.Brand.BrandToken.TokenProperties.g.cs"]);
        var source = generated[0].GetText(TestContext.Current.CancellationToken).ToString();
        source.ShouldContain("if (IsFrozen(nameof(BrandRadius))) return; field = value;");
        source.ShouldNotContain("TableRadius");
    }

    [Fact]
    public void Reports_ATOMUIGEN049_For_A_Non_Partial_Language_Token_Property()
    {
        var output = Run(Compile(("ShadcnToken.cs", """
            using AtomUI.Theme.DesignTokens;

            namespace Acme.Languages
            {
                [LanguageDesignToken]
                public sealed partial class ShadcnToken : LanguageDesignToken
                {
                    public partial double Ring { get; set; }
                    public double Input { get; set; }
                }
            }
            """)), out var diagnostics);

        var diagnostic = diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe(DiagnosticId);
        diagnostic.GetMessage(System.Globalization.CultureInfo.InvariantCulture).ShouldBe(
            "Token property 'Input' on 'Acme.Languages.ShadcnToken' must be declared partial, and 'Acme.Languages.ShadcnToken' must be a partial class");
        diagnostic.Location.GetLineSpan().StartLinePosition.Line.ShouldBe(8);
        var generated = GeneratedSource(output, "Acme.Languages.ShadcnToken.TokenProperties.g.cs");
        generated.ShouldContain("if (IsFrozen(nameof(Ring))) return; field = value;");
        generated.ShouldNotContain("Input");
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
            .ShouldContain("if (IsFrozen(nameof(BlockRadius))) return; field = value;");

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
        source.ShouldContain("if (IsFrozen(nameof(TitleHeight))) return; field = value;");
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
        seed.ShouldContain("if (IsFrozen(nameof(FontSize))) return; field = value;");
        seed.ShouldNotContain("LineHeight");
        seed.ShouldNotContain("Helper");
        seed.ShouldNotContain("ColorPalettes");
        var alias = GeneratedSource(output, "AtomUI.Theme.DesignTokens.DesignToken.Alias.TokenProperties.g.cs");
        alias.ShouldContain("if (IsFrozen(nameof(LineHeight))) return; field = value;");
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
        generated.ShouldContain("if (IsFrozen(nameof(Height))) return; field = value;");
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
        GeneratedSource(output, "Demo.Alpha.ButtonToken.TokenProperties.g.cs").ShouldContain("if (IsFrozen(nameof(Height))) return; field = value;");
        GeneratedSource(output, "Demo.Beta.ButtonToken.TokenProperties.g.cs").ShouldContain("if (IsFrozen(nameof(Width))) return; field = value;");
    }

    [Fact]
    public void Generated_Setter_Drops_A_Write_To_A_Frozen_Property_At_Run_Time()
    {
        // A runtime whose freeze check reads a set of frozen names, as Core's freeze bits do per slot.
        const string runtime = """
            namespace AtomUI.Theme.DesignTokens
            {
                [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false)]
                public sealed class ControlDesignTokenAttribute : System.Attribute { }

                [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false)]
                public sealed class LanguageDesignTokenAttribute : System.Attribute { }

                public abstract class AbstractDesignToken
                {
                    public System.Collections.Generic.HashSet<string> Frozen { get; } = new();
                    protected bool IsFrozen(string tokenName) => Frozen.Contains(tokenName);
                    protected void OnRead(string tokenName) { }
                }

                public abstract class AbstractControlDesignToken : AbstractDesignToken { }

                public abstract class LanguageDesignToken : AbstractDesignToken { }
            }
            """;
        var compilation = CSharpCompilation.Create("FreezeRuntime",
            [Parse("Runtime.cs", runtime), Parse("Tokens.cs", """
                using AtomUI.Theme.DesignTokens;

                namespace Demo
                {
                    [ControlDesignToken]
                    public sealed partial class ButtonToken : AbstractControlDesignToken
                    {
                        public partial double Height { get; set; }
                        public partial double Width { get; set; }
                    }

                    [LanguageDesignToken]
                    public sealed partial class DemoLanguageToken : LanguageDesignToken
                    {
                        public partial string Ring { get; set; }
                    }
                }
                """)],
            PlatformReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        var output = Run(compilation, out var diagnostics);
        diagnostics.ShouldBeEmpty();

        using var stream = new MemoryStream();
        var emitted = output.Emit(stream, cancellationToken: TestContext.Current.CancellationToken);
        emitted.Success.ShouldBeTrue(string.Join(Environment.NewLine, emitted.Diagnostics));
        var assembly = System.Reflection.Assembly.Load(stream.ToArray());

        var button = Activator.CreateInstance(assembly.GetType("Demo.ButtonToken", true)!)!;
        var frozen = (HashSet<string>)button.GetType().GetProperty("Frozen")!.GetValue(button)!;
        var height = button.GetType().GetProperty("Height")!;
        var width = button.GetType().GetProperty("Width")!;
        height.SetValue(button, 32d);
        frozen.Add("Height");
        height.SetValue(button, 40d);
        width.SetValue(button, 80d);
        // The frozen slot keeps the explicit value; the others are written normally.
        height.GetValue(button).ShouldBe(32d);
        width.GetValue(button).ShouldBe(80d);

        var language = Activator.CreateInstance(assembly.GetType("Demo.DemoLanguageToken", true)!)!;
        var ring = language.GetType().GetProperty("Ring")!;
        ((HashSet<string>)language.GetType().GetProperty("Frozen")!.GetValue(language)!).Add("Ring");
        ring.SetValue(language, "#FB7185");
        ring.GetValue(language).ShouldBeNull();
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

            [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
            public sealed class LanguageDesignTokenAttribute : System.Attribute { }

            [System.AttributeUsage(System.AttributeTargets.Property, Inherited = false)]
            public class NotTokenDefinitionAttribute : System.Attribute { }

            public enum DesignTokenKind { Seed, Map, Alias }

            [System.AttributeUsage(System.AttributeTargets.Property)]
            public sealed class DesignTokenKindAttribute : System.Attribute { public DesignTokenKindAttribute(DesignTokenKind kind) { } }

            public abstract class AbstractDesignToken
            {
                protected bool IsFrozen(string tokenName) => false;
                protected void OnRead(string tokenName) { }
            }

            public abstract class AbstractControlDesignToken : AbstractDesignToken
            {
            }

            public abstract class LanguageDesignToken : AbstractDesignToken
            {
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
