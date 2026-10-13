using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Shouldly;
using Xunit;

namespace AtomUI.Generator.Tests;

public class TokenResourceKeyGeneratorTests
{
    [Fact]
    public void Does_Not_Generate_Theme_Sources_Without_Theme_Runtime_Contracts()
    {
        var references = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))!
                         .Split(Path.PathSeparator)
                         .Select(static path => MetadataReference.CreateFromFile(path))
                         .Cast<MetadataReference>()
                         .ToImmutableArray();
        var compilation = CSharpCompilation.Create(
            "NonThemeAssembly",
            [CSharpSyntaxTree.ParseText(
                "namespace Demo; public sealed class PlainType { }",
                cancellationToken: TestContext.Current.CancellationToken)],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var output = RunGenerator(compilation, out var diagnostics);

        diagnostics.ShouldBeEmpty();
        output.SyntaxTrees.Count().ShouldBe(1);
    }

    [Fact]
    public void Publishes_Configured_Control_Catalog_As_Assembly_Metadata()
    {
        var outputCompilation = RunGenerator(
            CreateCompilation(string.Empty, "Acme.Controls"),
            out var diagnostics,
            new TestAnalyzerConfigOptionsProvider(new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["build_property.AtomUIThemeControlCatalog"] = "Acme.Theme"
            }));

        diagnostics.ShouldBeEmpty();
        GetGeneratedSource(outputCompilation, "ThemeControlCatalogMetadata.g.cs")
            .ShouldContain(
                "[assembly: global::System.Reflection.AssemblyMetadata(\"AtomUIThemeControlCatalog\", \"Acme.Theme\")]");
    }

    [Fact]
    public void Generates_Control_Token_Extension_And_Descriptor_Without_Legacy_Shared_Extension()
    {
        var outputCompilation = RunGenerator(CreateCompilation("""
            using Avalonia.Controls;
            using AtomUI.Theme.DesignTokens;

            namespace Demo
            {
                public sealed class Button : Control
                {
                }

                [ControlDesignToken]
                internal sealed class ButtonToken : AbstractControlDesignToken
                {
                    public double Height { get; set; }
                }
            }
            """), out var diagnostics);

        diagnostics.ShouldBeEmpty();
        outputCompilation.GetDiagnostics(TestContext.Current.CancellationToken)
                         .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
                         .ShouldBeEmpty();

        var tokenResources = GetGeneratedSource(outputCompilation, "TokenResourceConst.g.cs");
        tokenResources.ShouldContain("using AtomUI.Theme.Resources;");
        tokenResources.ShouldContain("public enum ButtonTokenKey");
        tokenResources.ShouldContain("ColorPrimary = (int)SharedTokenKind.ColorPrimary");
        tokenResources.ShouldContain("Height = -1");
        tokenResources.ShouldContain("public static class ButtonTokens");
        tokenResources.ShouldContain("ControlTokenIdentity.ForControl(typeof(global::Demo.Button), \"AtomUI\", \"Button\")");
        tokenResources.ShouldContain("public class ButtonTokenResourceExtension : TokenResourceExtension<ButtonTokenKey>");
        tokenResources.ShouldContain("ControlTokenResourceKey.Global(ButtonTokens.Identity, (SharedTokenKind)slot)");
        tokenResources.ShouldContain("if ((uint)slot >= 2u)");
        tokenResources.ShouldContain("if ((uint)ownSlot < 1u)");
        tokenResources.ShouldNotContain("Enum.GetValues");
        tokenResources.ShouldNotContain("kind switch");
        tokenResources.ShouldNotContain("ButtonTokenKey.ColorPrimary =>");
        tokenResources.ShouldNotContain("ButtonTokenSharedTokenResourceExtension");
        tokenResources.ShouldNotContain("ControlSharedTokenResourceExtension");

        var descriptorPool = GetGeneratedSource(outputCompilation, "GeneratedThemeSchema.g.cs");
        descriptorPool.ShouldContain("typeof(global::Demo.Button)");
        descriptorPool.ShouldContain("ControlTokenIdentity.ForControl(typeof(global::Demo.Button), \"AtomUI\", \"Button\")");
        descriptorPool.ShouldContain("static () => new global::Demo.ButtonToken()");
        descriptorPool.ShouldNotContain("DynamicDependency");
        descriptorPool.ShouldNotContain("typeof(global::Demo.ButtonToken)");
    }

    [Fact]
    public void Derives_Control_Identity_From_Token_Type_Name_Without_Manual_Id()
    {
        var outputCompilation = RunGenerator(CreateCompilation("""
            using Avalonia.Controls;
            using AtomUI.Theme.DesignTokens;

            namespace Demo
            {
                public sealed class Rating : Control
                {
                }

                [ControlDesignToken]
                internal sealed class RatingToken : AbstractControlDesignToken
                {
                    public double StarGap { get; set; }
                }
            }
            """), out var diagnostics);

        diagnostics.ShouldBeEmpty();
        var descriptorPool = GetGeneratedSource(outputCompilation, "GeneratedThemeSchema.g.cs");
        descriptorPool.ShouldContain("ControlTokenIdentity.ForControl(typeof(global::Demo.Rating), \"AtomUI\", \"Rating\")");
        descriptorPool.ShouldContain("static () => new global::Demo.RatingToken()");
    }

    [Fact]
    public void Reports_Diagnostic_When_Own_Token_Has_No_Matching_Control()
    {
        RunGenerator(CreateCompilation("""
            using AtomUI.Theme.DesignTokens;

            namespace Demo
            {
                [ControlDesignToken]
                internal sealed class MissingToken : AbstractControlDesignToken
                {
                }
            }
            """), out var diagnostics);

        var diagnostic = diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe("ATOMUIGEN014");
        diagnostic.GetMessage().ShouldContain("matching public Control");
    }

    [Fact]
    public void Reports_Diagnostic_When_Control_Token_Name_Does_Not_Follow_Convention()
    {
        RunGenerator(CreateCompilation("""
            using AtomUI.Theme.DesignTokens;

            namespace Demo
            {
                [ControlDesignToken]
                internal sealed class RatingDesignValues : AbstractControlDesignToken
                {
                    public double StarGap { get; set; }
                }
            }
            """), out var diagnostics);

        var diagnostic = diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe("ATOMUIGEN012");
        diagnostic.GetMessage().ShouldContain("must end with 'Token'");
    }

    [Fact]
    public void Does_Not_Generate_Runtime_Artifacts_For_Abstract_Control_Token_Layer()
    {
        var outputCompilation = RunGenerator(CreateCompilation("""
            using AtomUI.Theme.DesignTokens;

            namespace Demo
            {
                [ControlDesignToken]
                internal abstract class CommonButtonToken : AbstractControlDesignToken
                {
                    public double ContentHeight { get; set; }
                }
            }
            """), out var diagnostics);

        diagnostics.ShouldBeEmpty();
        var tokenResources = GetGeneratedSource(outputCompilation, "TokenResourceConst.g.cs");
        tokenResources.ShouldNotContain("CommonButtonTokenKind");
        tokenResources.ShouldNotContain("CommonButtonTokens");
        outputCompilation.SyntaxTrees.ShouldNotContain(tree =>
            tree.FilePath.EndsWith("GeneratedThemeSchema.g.cs", StringComparison.Ordinal));
    }

    [Fact]
    public void Reports_Diagnostic_When_Concrete_Control_Token_Is_Not_Sealed()
    {
        RunGenerator(CreateCompilation("""
            using Avalonia.Controls;
            using AtomUI.Theme.DesignTokens;

            namespace Demo
            {
                public sealed class Button : Control
                {
                }

                [ControlDesignToken]
                internal class ButtonToken : AbstractControlDesignToken
                {
                }
            }
            """), out var diagnostics);

        var diagnostic = diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe("ATOMUIGEN020");
        diagnostic.GetMessage().ShouldContain("must be sealed");
    }

    [Fact]
    public void Reports_Diagnostic_When_Control_Token_Layer_Is_Generic()
    {
        RunGenerator(CreateCompilation("""
            using AtomUI.Theme.DesignTokens;

            namespace Demo
            {
                [ControlDesignToken]
                internal abstract class CommonButtonToken<TValue> : AbstractControlDesignToken
                {
                }
            }
            """), out var diagnostics);

        var diagnostic = diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe("ATOMUIGEN021");
        diagnostic.GetMessage().ShouldContain("must be non-generic");
    }

    [Fact]
    public void Reports_A_Shared_Generic_Layer_Only_Once_For_A_Concrete_Terminal()
    {
        RunGenerator(CreateCompilation("""
            using Avalonia.Controls;
            using AtomUI.Theme.DesignTokens;

            namespace Demo
            {
                public sealed class Button : Control
                {
                }

                [ControlDesignToken]
                internal abstract class CommonButtonToken<T> : AbstractControlDesignToken
                {
                }

                [ControlDesignToken]
                internal sealed class ButtonToken : CommonButtonToken<int>
                {
                }
            }
            """), out var diagnostics);

        var diagnostic = diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe("ATOMUIGEN021");
        diagnostic.GetMessage().ShouldContain("global::Demo.CommonButtonToken<T>");
    }

    [Fact]
    public void Reports_Generic_Diagnostic_When_Control_Token_Is_Nested_In_A_Generic_Type()
    {
        RunGenerator(CreateCompilation("""
            using Avalonia.Controls;
            using AtomUI.Theme.DesignTokens;

            namespace Demo
            {
                public sealed class Button : Control
                {
                }

                internal static class Holder<T>
                {
                    [ControlDesignToken]
                    internal sealed class ButtonToken : AbstractControlDesignToken
                    {
                    }
                }
            }
            """), out var diagnostics);

        var diagnostic = diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe("ATOMUIGEN021");
        diagnostic.GetMessage().ShouldContain("must be non-generic");
    }

    [Fact]
    public void Reports_Diagnostic_When_Control_Token_Is_Nested_In_A_Non_Generic_Type()
    {
        RunGenerator(CreateCompilation("""
            using Avalonia.Controls;
            using AtomUI.Theme.DesignTokens;

            namespace Demo
            {
                public sealed class Button : Control
                {
                }

                internal static class Holder
                {
                    [ControlDesignToken]
                    internal sealed class ButtonToken : AbstractControlDesignToken
                    {
                    }
                }
            }
            """), out var diagnostics);

        var diagnostic = diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe("ATOMUIGEN026");
        diagnostic.GetMessage().ShouldContain("top-level");
    }

    [Fact]
    public void Reports_A_Shared_Nested_Layer_Only_Once_For_A_Concrete_Terminal()
    {
        RunGenerator(CreateCompilation("""
            using Avalonia.Controls;
            using AtomUI.Theme.DesignTokens;

            namespace Demo
            {
                public sealed class Button : Control
                {
                }

                internal static class Holder
                {
                    [ControlDesignToken]
                    internal abstract class CommonButtonToken : AbstractControlDesignToken
                    {
                    }
                }

                [ControlDesignToken]
                internal sealed class ButtonToken : Holder.CommonButtonToken
                {
                }
            }
            """), out var diagnostics);

        var diagnostic = diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe("ATOMUIGEN026");
        diagnostic.GetMessage().ShouldContain("global::Demo.Holder.CommonButtonToken");
    }

    [Fact]
    public void Reports_A_Shared_Generic_Containing_Layer_Only_Once_For_A_Concrete_Terminal()
    {
        RunGenerator(CreateCompilation("""
            using Avalonia.Controls;
            using AtomUI.Theme.DesignTokens;

            namespace Demo
            {
                public sealed class Button : Control
                {
                }

                internal static class Holder<T>
                {
                    [ControlDesignToken]
                    internal abstract class CommonButtonToken : AbstractControlDesignToken
                    {
                    }
                }

                [ControlDesignToken]
                internal sealed class ButtonToken : Holder<int>.CommonButtonToken
                {
                }
            }
            """), out var diagnostics);

        var diagnostic = diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe("ATOMUIGEN021");
        diagnostic.GetMessage().ShouldContain("global::Demo.Holder<T>.CommonButtonToken");
    }

    [Fact]
    public void Reports_Abstract_Name_Diagnostic_Without_Matching_A_Control()
    {
        RunGenerator(CreateCompilation("""
            using AtomUI.Theme.DesignTokens;

            namespace Demo
            {
                [ControlDesignToken]
                internal abstract class CommonButtonValues : AbstractControlDesignToken
                {
                }
            }
            """), out var diagnostics);

        var diagnostic = diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe("ATOMUIGEN025");
        diagnostic.GetMessage().ShouldContain("abstract Control design token");
    }

    [Fact]
    public void Flattens_Multi_Level_Control_Token_Properties_Into_The_Terminal_Schema()
    {
        var outputCompilation = RunGenerator(CreateCompilation("""
            using Avalonia.Controls;
            using AtomUI.Theme.DesignTokens;

            namespace Demo
            {
                public sealed class Button : Control
                {
                }

                [ControlDesignToken]
                internal abstract class CommonInteractiveToken : AbstractControlDesignToken
                {
                    public double ContentHeight { get; set; }
                }

                [ControlDesignToken]
                internal abstract class CommonButtonToken : CommonInteractiveToken
                {
                    public double CornerRadius { get; set; }
                }

                [ControlDesignToken]
                internal sealed class ButtonToken : CommonButtonToken
                {
                    public string Label { get; set; } = string.Empty;
                }
            }
            """), out var diagnostics);

        diagnostics.ShouldBeEmpty();
        var tokenResources = GetGeneratedSource(outputCompilation, "TokenResourceConst.g.cs");
        tokenResources.ShouldContain("ContentHeight = -1");
        tokenResources.ShouldContain("CornerRadius = -2");
        tokenResources.ShouldContain("Label = -3");
        tokenResources.ShouldNotContain("CommonInteractiveTokenKind");
        tokenResources.ShouldNotContain("CommonButtonTokenKind");

        var schema = GetGeneratedSource(outputCompilation, "GeneratedThemeSchema.g.cs");
        schema.ShouldContain("return ((global::Demo.ButtonToken)token).ContentHeight;");
        schema.ShouldContain("return ((global::Demo.ButtonToken)token).CornerRadius;");
        schema.ShouldContain("return ((global::Demo.ButtonToken)token).Label;");
        schema.ShouldContain("static () => new global::Demo.ButtonToken()");
    }

    [Fact]
    public void Generates_Independent_Terminal_Schemas_For_Siblings_Sharing_An_Abstract_Layer()
    {
        var outputCompilation = RunGenerator(CreateCompilation("""
            using Avalonia.Controls;
            using AtomUI.Theme.DesignTokens;

            namespace Demo
            {
                public sealed class Button : Control
                {
                }

                public sealed class Link : Control
                {
                }

                [ControlDesignToken]
                internal abstract class CommonInteractiveToken : AbstractControlDesignToken
                {
                    public double FocusWidth { get; set; }
                }

                [ControlDesignToken]
                internal sealed class ButtonToken : CommonInteractiveToken
                {
                    public double ContentHeight { get; set; }
                }

                [ControlDesignToken]
                internal sealed class LinkToken : CommonInteractiveToken
                {
                    public double UnderlineOffset { get; set; }
                }
            }
            """), out var diagnostics);

        diagnostics.ShouldBeEmpty();
        var resources = GetGeneratedSource(outputCompilation, "TokenResourceConst.g.cs");
        resources.ShouldContain("public enum ButtonTokenKind");
        resources.ShouldContain("public enum LinkTokenKind");
        resources.ShouldContain("ControlTokenIdentity.ForControl(typeof(global::Demo.Button), \"AtomUI\", \"Button\")");
        resources.ShouldContain("ControlTokenIdentity.ForControl(typeof(global::Demo.Link), \"AtomUI\", \"Link\")");
        var schema = GetGeneratedSource(outputCompilation, "GeneratedThemeSchema.g.cs");
        schema.ShouldContain("((global::Demo.ButtonToken)token).FocusWidth");
        schema.ShouldContain("((global::Demo.LinkToken)token).FocusWidth");
        schema.ShouldContain("static () => new global::Demo.ButtonToken()");
        schema.ShouldContain("static () => new global::Demo.LinkToken()");
    }

    [Fact]
    public void Reports_Diagnostic_When_Control_Token_Chain_Contains_Unmarked_Layer()
    {
        RunGenerator(CreateCompilation("""
            using Avalonia.Controls;
            using AtomUI.Theme.DesignTokens;

            namespace Demo
            {
                public sealed class Button : Control
                {
                }

                internal abstract class CommonButtonToken : AbstractControlDesignToken
                {
                }

                [ControlDesignToken]
                internal sealed class ButtonToken : CommonButtonToken
                {
                }
            }
            """), out var diagnostics);

        var diagnostic = diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe("ATOMUIGEN013");
        diagnostic.GetMessage().ShouldContain("must declare [ControlDesignToken]");
    }

    [Fact]
    public void Reports_Diagnostic_When_Control_Token_Chain_Does_Not_Reach_The_Root()
    {
        RunGenerator(CreateCompilation("""
            using Avalonia.Controls;
            using AtomUI.Theme.DesignTokens;

            namespace Demo
            {
                public sealed class Button : Control
                {
                }

                [ControlDesignToken]
                internal sealed class ButtonToken
                {
                }
            }
            """), out var diagnostics);

        var diagnostic = diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe("ATOMUIGEN013");
        diagnostic.GetMessage().ShouldContain("must reach");
        diagnostic.GetMessage().ShouldContain("AbstractControlDesignToken");
    }

    [Fact]
    public void Reports_Inheritance_And_Sealing_Diagnostics_For_A_Concrete_Intermediate_Token()
    {
        RunGenerator(CreateCompilation("""
            using Avalonia.Controls;
            using AtomUI.Theme.DesignTokens;

            namespace Demo
            {
                public sealed class Button : Control
                {
                }

                [ControlDesignToken]
                internal class CommonButtonToken : AbstractControlDesignToken
                {
                }

                [ControlDesignToken]
                internal sealed class ButtonToken : CommonButtonToken
                {
                }
            }
            """), out var diagnostics);

        diagnostics.Select(static diagnostic => diagnostic.Id).ShouldContain("ATOMUIGEN020");
        diagnostics.Select(static diagnostic => diagnostic.Id).ShouldContain("ATOMUIGEN013");
        diagnostics.ShouldContain(static diagnostic =>
            diagnostic.GetMessage().Contains("intermediate", StringComparison.Ordinal));
    }

    [Fact]
    public void Reports_Diagnostic_When_Inherited_Control_Token_Properties_Conflict()
    {
        RunGenerator(CreateCompilation("""
            using Avalonia.Controls;
            using AtomUI.Theme.DesignTokens;

            namespace Demo
            {
                public sealed class Button : Control
                {
                }

                [ControlDesignToken]
                internal abstract class CommonButtonToken : AbstractControlDesignToken
                {
                    public double ContentHeight { get; set; }
                }

                [ControlDesignToken]
                internal sealed class ButtonToken : CommonButtonToken
                {
                    public new double ContentHeight { get; set; }
                }
            }
            """), out var diagnostics);

        var diagnostic = diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe("ATOMUIGEN022");
        diagnostic.GetMessage().ShouldContain("ContentHeight");
    }

    [Theory]
    [InlineData("[NotTokenDefinition] public new double Height { get; set; }")]
    [InlineData("public new double Height;")]
    [InlineData("public new void Height() { }")]
    public void Reports_Diagnostic_When_A_Derived_Member_Hides_An_Inherited_Token_Property(
        string memberDeclaration)
    {
        RunGenerator(CreateCompilation($$"""
            using Avalonia.Controls;
            using AtomUI.Theme.DesignTokens;

            namespace Demo
            {
                public sealed class Button : Control
                {
                }

                [ControlDesignToken]
                internal abstract class CommonButtonToken : AbstractControlDesignToken
                {
                    public double Height { get; set; }
                }

                [ControlDesignToken]
                internal sealed class ButtonToken : CommonButtonToken
                {
                    {{memberDeclaration}}
                }
            }
            """), out var diagnostics);

        var diagnostic = diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe("ATOMUIGEN022");
        diagnostic.GetMessage().ShouldContain("Height");
    }

    [Fact]
    public void Reports_Diagnostic_When_A_Token_Property_Hides_An_Excluded_Base_Member()
    {
        RunGenerator(CreateCompilation("""
            using Avalonia.Controls;
            using AtomUI.Theme.DesignTokens;

            namespace Demo
            {
                public sealed class Button : Control
                {
                }

                [ControlDesignToken]
                internal abstract class CommonButtonToken : AbstractControlDesignToken
                {
                    [NotTokenDefinition]
                    public double Height { get; set; }
                }

                [ControlDesignToken]
                internal sealed class ButtonToken : CommonButtonToken
                {
                    public new double Height { get; set; }
                }
            }
            """), out var diagnostics);

        var diagnostic = diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe("ATOMUIGEN022");
        diagnostic.GetMessage().ShouldContain("Height");
    }

    [Fact]
    public void Reports_A_Shared_Invalid_Layer_Diagnostic_Only_Once_Across_Siblings()
    {
        RunGenerator(CreateCompilation("""
            using Avalonia.Controls;
            using AtomUI.Theme.DesignTokens;

            namespace Demo
            {
                public sealed class Button : Control
                {
                }

                public sealed class Link : Control
                {
                }

                [ControlDesignToken]
                internal abstract class CommonButtonValues : AbstractControlDesignToken
                {
                }

                [ControlDesignToken]
                internal sealed class ButtonToken : CommonButtonValues
                {
                }

                [ControlDesignToken]
                internal sealed class LinkToken : CommonButtonValues
                {
                }
            }
            """), out var diagnostics);

        var diagnostic = diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe("ATOMUIGEN025");
        diagnostic.GetMessage().ShouldContain("CommonButtonValues");
    }

    [Fact]
    public void Reports_A_Shared_Invalid_Inheritance_Layer_Only_Once_Across_Siblings()
    {
        RunGenerator(CreateCompilation("""
            using Avalonia.Controls;
            using AtomUI.Theme.DesignTokens;

            namespace Demo
            {
                public sealed class Button : Control
                {
                }

                public sealed class Link : Control
                {
                }

                internal abstract class UnmarkedToken : AbstractControlDesignToken
                {
                }

                [ControlDesignToken]
                internal sealed class ButtonToken : UnmarkedToken
                {
                }

                [ControlDesignToken]
                internal sealed class LinkToken : UnmarkedToken
                {
                }
            }
            """), out var diagnostics);

        var diagnostic = diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe("ATOMUIGEN013");
        diagnostic.GetMessage().ShouldContain("global::Demo.UnmarkedToken");
    }

    [Fact]
    public void Reports_Independent_No_Root_Terminals_Separately()
    {
        RunGenerator(CreateCompilation("""
            using Avalonia.Controls;
            using AtomUI.Theme.DesignTokens;

            namespace Demo
            {
                public sealed class Button : Control
                {
                }

                public sealed class Link : Control
                {
                }

                [ControlDesignToken]
                internal sealed class ButtonToken
                {
                }

                [ControlDesignToken]
                internal sealed class LinkToken
                {
                }
            }
            """), out var diagnostics);

        diagnostics.Length.ShouldBe(2);
        diagnostics.ShouldAllBe(static diagnostic => diagnostic.Id == "ATOMUIGEN013");
        diagnostics.ShouldContain(static diagnostic =>
            diagnostic.GetMessage().Contains("global::Demo.ButtonToken", StringComparison.Ordinal));
        diagnostics.ShouldContain(static diagnostic =>
            diagnostic.GetMessage().Contains("global::Demo.LinkToken", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("public static double Height { get; set; }")]
    [InlineData("public virtual double Height { get; set; }")]
    [InlineData("public double this[int index] { get => 0; set { } }")]
    [InlineData("public double Height { get; protected set; }")]
    [InlineData("public double Height { get; init; }")]
    [InlineData("public required double Height { get; set; }")]
    public void Reports_Diagnostic_For_Invalid_Control_Token_Property_Shape(string propertyDeclaration)
    {
        RunGenerator(CreateCompilation($$"""
            using Avalonia.Controls;
            using AtomUI.Theme.DesignTokens;

            namespace Demo
            {
                public sealed class Button : Control
                {
                }

                [ControlDesignToken]
                internal sealed class ButtonToken : AbstractControlDesignToken
                {
                    {{propertyDeclaration}}
                }
            }
            """), out var diagnostics);

        var diagnostic = diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe("ATOMUIGEN023");
        diagnostic.GetMessage().ShouldContain("cannot define an Own Token");
    }

    [Fact]
    public void Reports_Diagnostic_For_An_Explicit_Interface_Control_Token_Property()
    {
        RunGenerator(CreateCompilation("""
            using Avalonia.Controls;
            using AtomUI.Theme.DesignTokens;

            namespace Demo
            {
                internal interface ITokenValues
                {
                    double Height { get; set; }
                }

                public sealed class Button : Control
                {
                }

                [ControlDesignToken]
                internal sealed class ButtonToken : AbstractControlDesignToken, ITokenValues
                {
                    double ITokenValues.Height { get; set; }
                }
            }
            """), out var diagnostics);

        var diagnostic = diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe("ATOMUIGEN023");
        diagnostic.GetMessage().ShouldContain("explicit interface");
    }

    [Fact]
    public void Excludes_Not_Token_Definition_Properties_From_Inherited_Schema()
    {
        var outputCompilation = RunGenerator(CreateCompilation("""
            using Avalonia.Controls;
            using AtomUI.Theme.DesignTokens;

            namespace Demo
            {
                public sealed class Button : Control
                {
                }

                [ControlDesignToken]
                internal abstract class CommonButtonToken : AbstractControlDesignToken
                {
                    [NotTokenDefinition]
                    public double InternalMeasurement { get; set; }

                    public double ContentHeight { get; set; }
                }

                [ControlDesignToken]
                internal sealed class ButtonToken : CommonButtonToken
                {
                }
            }
            """), out var diagnostics);

        diagnostics.ShouldBeEmpty();
        var tokenResources = GetGeneratedSource(outputCompilation, "TokenResourceConst.g.cs");
        tokenResources.ShouldContain("ContentHeight = -1");
        tokenResources.ShouldNotContain("InternalMeasurement");
    }

    [Fact]
    public void Accepts_A_Later_Calculation_Override_With_One_First_Base_Call()
    {
        var outputCompilation = RunGenerator(CreateCompilation("""
            using Avalonia.Controls;
            using AtomUI.Theme.DesignTokens;

            namespace Demo
            {
                public sealed class Button : Control
                {
                }

                [ControlDesignToken]
                internal abstract class CommonButtonToken : AbstractControlDesignToken
                {
                    public double ContentHeight { get; set; }

                    public override void CalculateTokenValues(bool isDarkMode)
                    {
                        ContentHeight = isDarkMode ? 40 : 32;
                    }
                }

                [ControlDesignToken]
                internal sealed class ButtonToken : CommonButtonToken
                {
                    public double CornerRadius { get; set; }

                    public override void CalculateTokenValues(bool isDarkMode)
                    {
                        base.CalculateTokenValues(isDarkMode);
                        CornerRadius = isDarkMode ? 8 : 6;
                    }
                }
            }
            """), out var diagnostics);

        diagnostics.ShouldBeEmpty();
        var schema = GetGeneratedSource(outputCompilation, "GeneratedThemeSchema.g.cs");
        schema.ShouldContain(
            "((global::Demo.ButtonToken)token).CalculateTokenValues(appearance == global::AtomUI.Theme.ThemeAppearance.Dark)");
        schema.ShouldNotContain("CommonButtonToken)token).CalculateTokenValues");
    }

    [Theory]
    [InlineData("public void CalculateTokenValues(ref bool isDarkMode) { }")]
    [InlineData("public void CalculateTokenValues<T>(bool isDarkMode) { }")]
    public void Ignores_Non_Target_Calculation_Overloads(string overloadDeclaration)
    {
        RunGenerator(CreateCompilation($$"""
            using Avalonia.Controls;
            using AtomUI.Theme.DesignTokens;

            namespace Demo
            {
                public sealed class Button : Control
                {
                }

                [ControlDesignToken]
                internal abstract class CommonButtonToken : AbstractControlDesignToken
                {
                    public override void CalculateTokenValues(bool isDarkMode)
                    {
                    }
                }

                [ControlDesignToken]
                internal sealed class ButtonToken : CommonButtonToken
                {
                    {{overloadDeclaration}}

                    public override void CalculateTokenValues(bool isDarkMode)
                    {
                        base.CalculateTokenValues(isDarkMode);
                    }
                }
            }
            """), out var diagnostics);

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public void Ignores_A_Base_Call_To_A_Non_Target_Calculation_Overload()
    {
        RunGenerator(CreateCompilation("""
            using Avalonia.Controls;
            using AtomUI.Theme.DesignTokens;

            namespace Demo
            {
                public sealed class Button : Control
                {
                }

                [ControlDesignToken]
                internal abstract class CommonButtonToken : AbstractControlDesignToken
                {
                    public void CalculateTokenValues(int mode)
                    {
                    }

                    public override void CalculateTokenValues(bool isDarkMode)
                    {
                    }
                }

                [ControlDesignToken]
                internal sealed class ButtonToken : CommonButtonToken
                {
                    public override void CalculateTokenValues(bool isDarkMode)
                    {
                        base.CalculateTokenValues(isDarkMode);
                        base.CalculateTokenValues(1);
                    }
                }
            }
            """), out var diagnostics);

        diagnostics.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("missing", "public override void CalculateTokenValues(bool isDarkMode) { }")]
    [InlineData("duplicate", "public override void CalculateTokenValues(bool isDarkMode) { base.CalculateTokenValues(isDarkMode); base.CalculateTokenValues(isDarkMode); }")]
    [InlineData("not first", "public override void CalculateTokenValues(bool isDarkMode) { CornerRadius = 4; base.CalculateTokenValues(isDarkMode); }")]
    [InlineData("conditional", "public override void CalculateTokenValues(bool isDarkMode) { if (isDarkMode) { base.CalculateTokenValues(isDarkMode); } }")]
    [InlineData("wrong argument", "public override void CalculateTokenValues(bool isDarkMode) { base.CalculateTokenValues(!isDarkMode); }")]
    [InlineData("expression body", "public override void CalculateTokenValues(bool isDarkMode) => base.CalculateTokenValues(isDarkMode);")]
    [InlineData("nested local function", "public override void CalculateTokenValues(bool isDarkMode) { base.CalculateTokenValues(isDarkMode); void Recalculate() { base.CalculateTokenValues(isDarkMode); } }")]
    public void Reports_Diagnostic_For_Invalid_Calculation_Base_Call(
        string expectedReason,
        string methodDeclaration)
    {
        RunGenerator(CreateCompilation($$"""
            using Avalonia.Controls;
            using AtomUI.Theme.DesignTokens;

            namespace Demo
            {
                public sealed class Button : Control
                {
                }

                [ControlDesignToken]
                internal abstract class CommonButtonToken : AbstractControlDesignToken
                {
                    public override void CalculateTokenValues(bool isDarkMode)
                    {
                    }
                }

                [ControlDesignToken]
                internal sealed class ButtonToken : CommonButtonToken
                {
                    public double CornerRadius { get; set; }

                    {{methodDeclaration}}
                }
            }
            """), out var diagnostics);

        var diagnostic = diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe("ATOMUIGEN024");
        diagnostic.GetMessage().ShouldContain(expectedReason);
    }

    [Fact]
    public void Flattens_A_Public_Abstract_Control_Token_From_A_Metadata_Reference()
    {
        var runtimeReference = CreateThemeRuntimeReference();
        var sharedCompilation = CreateCompilationWithReferences(
            """
            using AtomUI.Theme.DesignTokens;

            namespace SharedTokens
            {
                [ControlDesignToken]
                public abstract class CommonButtonToken : AbstractControlDesignToken
                {
                    public double ContentHeight { get; set; }
                }
            }
            """,
            "SharedTokens",
            runtimeReference);
        var sharedOutput = RunGenerator(sharedCompilation, out var sharedDiagnostics);
        sharedDiagnostics.ShouldBeEmpty();
        GetGeneratedSource(sharedOutput, "TokenResourceConst.g.cs")
            .ShouldNotContain("CommonButtonTokens");

        var platformCompilation = CreateCompilationWithReferences(
            """
            using Avalonia.Controls;
            using AtomUI.Theme.DesignTokens;
            using SharedTokens;

            namespace PlatformControls
            {
                public sealed class Button : Control
                {
                }

                [ControlDesignToken]
                internal sealed class ButtonToken : CommonButtonToken
                {
                    public double CornerRadius { get; set; }
                }
            }
            """,
            "PlatformControls",
            runtimeReference,
            EmitToPortableExecutableReference(sharedOutput));

        var platformOutput = RunGenerator(platformCompilation, out var platformDiagnostics);

        platformDiagnostics.ShouldBeEmpty();
        platformOutput.GetDiagnostics(TestContext.Current.CancellationToken)
                      .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
                      .ShouldBeEmpty();
        var resources = GetGeneratedSource(platformOutput, "TokenResourceConst.g.cs");
        resources.ShouldContain("ContentHeight = -1");
        resources.ShouldContain("CornerRadius = -2");
        var schema = GetGeneratedSource(platformOutput, "GeneratedThemeSchema.g.cs");
        schema.ShouldContain("return ((global::PlatformControls.ButtonToken)token).ContentHeight;");
    }

    [Fact]
    public void Changes_The_Terminal_Schema_When_The_Metadata_Base_Contract_Changes()
    {
        var runtimeReference = CreateThemeRuntimeReference();
        var firstSharedOutput = RunGenerator(CreateCompilationWithReferences(
            """
            using AtomUI.Theme.DesignTokens;

            namespace SharedTokens
            {
                [ControlDesignToken]
                public abstract class CommonButtonToken : AbstractControlDesignToken
                {
                    public double ContentHeight { get; set; }
                }
            }
            """,
            "SharedTokens",
            runtimeReference), out var firstSharedDiagnostics);
        var secondSharedOutput = RunGenerator(CreateCompilationWithReferences(
            """
            using AtomUI.Theme.DesignTokens;

            namespace SharedTokens
            {
                [ControlDesignToken]
                public abstract class CommonButtonToken : AbstractControlDesignToken
                {
                    public int ContentWidth { get; set; }
                }
            }
            """,
            "SharedTokens",
            runtimeReference), out var secondSharedDiagnostics);
        firstSharedDiagnostics.ShouldBeEmpty();
        secondSharedDiagnostics.ShouldBeEmpty();

        const string platformSource = """
            using Avalonia.Controls;
            using AtomUI.Theme.DesignTokens;
            using SharedTokens;

            namespace PlatformControls
            {
                public sealed class Button : Control
                {
                }

                [ControlDesignToken]
                internal sealed class ButtonToken : CommonButtonToken
                {
                }
            }
            """;
        var firstSharedReference = EmitToPortableExecutableReference(firstSharedOutput);
        var secondSharedReference = EmitToPortableExecutableReference(secondSharedOutput);
        var firstPlatformCompilation = CreateCompilationWithReferences(
            platformSource,
            "PlatformControls",
            runtimeReference,
            firstSharedReference);
        var secondPlatformCompilation = firstPlatformCompilation
                                        .RemoveReferences(firstSharedReference)
                                        .AddReferences(secondSharedReference);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [new TokenResourceKeyGenerator().AsSourceGenerator()],
            ImmutableArray<AdditionalText>.Empty,
            (CSharpParseOptions)firstPlatformCompilation.SyntaxTrees[0].Options,
            optionsProvider: null);
        driver = driver.RunGeneratorsAndUpdateCompilation(
            firstPlatformCompilation,
            out var firstOutputCompilation,
            out var firstPlatformDiagnostics,
            TestContext.Current.CancellationToken);
        driver.RunGeneratorsAndUpdateCompilation(
            secondPlatformCompilation,
            out var secondOutputCompilation,
            out var secondPlatformDiagnostics,
            TestContext.Current.CancellationToken);

        firstPlatformDiagnostics.ShouldBeEmpty();
        secondPlatformDiagnostics.ShouldBeEmpty();
        var firstSchema = GetGeneratedSource((CSharpCompilation)firstOutputCompilation, "GeneratedThemeSchema.g.cs");
        var secondSchema = GetGeneratedSource((CSharpCompilation)secondOutputCompilation, "GeneratedThemeSchema.g.cs");
        firstSchema.ShouldContain("global::System.Double");
        firstSchema.ShouldContain("ContentHeight");
        secondSchema.ShouldContain("global::System.Int32");
        secondSchema.ShouldContain("ContentWidth");
        secondSchema.ShouldNotBe(firstSchema);
    }

    [Fact]
    public void Generated_Descriptors_Execute_Inherited_Calculations_And_Isolate_Siblings()
    {
        var output = RunGenerator(CreateCompilation("""
            using Avalonia.Controls;
            using AtomUI.Theme.DesignTokens;

            namespace Demo
            {
                public sealed class Button : Control
                {
                }

                public sealed class Link : Control
                {
                }

                [ControlDesignToken]
                internal abstract class CommonInteractiveToken : AbstractControlDesignToken
                {
                    public double ContentHeight { get; set; }

                    public override void CalculateTokenValues(bool isDarkMode)
                    {
                        ContentHeight = isDarkMode ? 40 : 32;
                    }
                }

                [ControlDesignToken]
                internal sealed class ButtonToken : CommonInteractiveToken
                {
                    public double TerminalValue { get; set; }

                    public override void CalculateTokenValues(bool isDarkMode)
                    {
                        base.CalculateTokenValues(isDarkMode);
                        TerminalValue = isDarkMode ? 2 : 1;
                    }
                }

                [ControlDesignToken]
                internal sealed class LinkToken : CommonInteractiveToken
                {
                    public double TerminalValue { get; set; }

                    public override void CalculateTokenValues(bool isDarkMode)
                    {
                        base.CalculateTokenValues(isDarkMode);
                        TerminalValue = isDarkMode ? 20 : 10;
                    }
                }
            }
            """, "RuntimeControlTokenInheritance"), out var diagnostics);

        diagnostics.ShouldBeEmpty();
        output.GetDiagnostics(TestContext.Current.CancellationToken)
              .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
              .ShouldBeEmpty();

        using var stream = new MemoryStream();
        var emitResult = output.Emit(stream, cancellationToken: TestContext.Current.CancellationToken);
        emitResult.Success.ShouldBeTrue(
            string.Join(Environment.NewLine, emitResult.Diagnostics.Select(static diagnostic => diagnostic.ToString())));
        var assembly = System.Reflection.Assembly.Load(stream.ToArray());
        var schemaType = assembly.GetType("AtomUI.Generated.RuntimeControlTokenInheritance.GeneratedThemeSchemaDescriptorFactory", true)!;
        var descriptors = schemaType.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
            .Where(method => method.Name.StartsWith("CreateControlDescriptor_", StringComparison.Ordinal))
            .Select(method => method.Invoke(null, null)!).ToArray();
        descriptors.Length.ShouldBe(2);

        foreach (var descriptor in descriptors)
        {
            var descriptorType = descriptor.GetType();
            var factory = (Delegate)descriptorType.GetProperty("Factory")!.GetValue(descriptor)!;
            var evaluator = (Delegate)descriptorType.GetProperty("Evaluator")!.GetValue(descriptor)!;
            var appearanceType = assembly.GetType("AtomUI.Theme.ThemeAppearance", throwOnError: true)!;
            foreach (var appearanceName in new[] { "Light", "Dark" })
            {
                var token = factory.DynamicInvoke()!;
                evaluator.DynamicInvoke(token, Enum.Parse(appearanceType, appearanceName));
                var isDark = appearanceName == "Dark";
                var expectedTerminalValue = token.GetType().Name == "ButtonToken"
                    ? isDark ? 2d : 1d
                    : isDark ? 20d : 10d;
                token.GetType().GetProperty("ContentHeight")!.GetValue(token).ShouldBe(isDark ? 40d : 32d);
                token.GetType().GetProperty("TerminalValue")!.GetValue(token).ShouldBe(expectedTerminalValue);
            }
        }
    }

    [Fact]
    public void Reports_Diagnostic_For_Invalid_Abstract_Token_Name_From_Metadata()
    {
        var runtimeReference = CreateThemeRuntimeReference();
        var sharedReference = EmitToPortableExecutableReference(CreateCompilationWithReferences(
            """
            using AtomUI.Theme.DesignTokens;

            namespace SharedTokens
            {
                [ControlDesignToken]
                public abstract class CommonButtonValues : AbstractControlDesignToken
                {
                }
            }
            """,
            "SharedTokens",
            runtimeReference));
        var platformCompilation = CreateCompilationWithReferences(
            """
            using Avalonia.Controls;
            using AtomUI.Theme.DesignTokens;
            using SharedTokens;

            namespace PlatformControls
            {
                public sealed class Button : Control
                {
                }

                [ControlDesignToken]
                internal sealed class ButtonToken : CommonButtonValues
                {
                }
            }
            """,
            "PlatformControls",
            runtimeReference,
            sharedReference);

        RunGenerator(platformCompilation, out var diagnostics);

        var diagnostic = diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe("ATOMUIGEN025");
        diagnostic.GetMessage().ShouldContain("global::SharedTokens.CommonButtonValues");
    }

    [Fact]
    public void Reports_Diagnostic_For_Hidden_Calculation_Method_From_Metadata()
    {
        var runtimeReference = CreateThemeRuntimeReference();
        var sharedReference = EmitToPortableExecutableReference(CreateCompilationWithReferences(
            """
            using AtomUI.Theme.DesignTokens;

            namespace SharedTokens
            {
                [ControlDesignToken]
                public abstract class CommonButtonToken : AbstractControlDesignToken
                {
                    public new void CalculateTokenValues(bool isDarkMode)
                    {
                    }
                }
            }
            """,
            "SharedTokens",
            runtimeReference));
        var platformCompilation = CreateCompilationWithReferences(
            """
            using Avalonia.Controls;
            using AtomUI.Theme.DesignTokens;
            using SharedTokens;

            namespace PlatformControls
            {
                public sealed class Button : Control
                {
                }

                [ControlDesignToken]
                internal sealed class ButtonToken : CommonButtonToken
                {
                }
            }
            """,
            "PlatformControls",
            runtimeReference,
            sharedReference);

        RunGenerator(platformCompilation, out var diagnostics);

        var diagnostic = diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe("ATOMUIGEN024");
        diagnostic.GetMessage().ShouldContain("must be an override");
    }

    [Fact]
    public void Flattens_An_Internal_Metadata_Layer_When_Internals_Are_Visible()
    {
        var runtimeReference = CreateThemeRuntimeReference();
        var sharedReference = EmitToPortableExecutableReference(CreateCompilationWithReferences(
            """
            [assembly: System.Runtime.CompilerServices.InternalsVisibleTo("PlatformControls")]

            namespace SharedTokens
            {
                [AtomUI.Theme.DesignTokens.ControlDesignToken]
                internal abstract class CommonButtonToken : AtomUI.Theme.DesignTokens.AbstractControlDesignToken
                {
                    internal double ContentHeight { get; set; }
                }
            }
            """,
            "SharedTokens",
            runtimeReference));
        var platformCompilation = CreateCompilationWithReferences(
            """
            using Avalonia.Controls;
            using AtomUI.Theme.DesignTokens;
            using SharedTokens;

            namespace PlatformControls
            {
                public sealed class Button : Control
                {
                }

                [ControlDesignToken]
                internal sealed class ButtonToken : CommonButtonToken
                {
                }
            }
            """,
            "PlatformControls",
            runtimeReference,
            sharedReference);

        var output = RunGenerator(platformCompilation, out var diagnostics);

        diagnostics.ShouldBeEmpty();
        output.GetDiagnostics(TestContext.Current.CancellationToken)
              .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
              .ShouldBeEmpty();
        GetGeneratedSource(output, "GeneratedThemeSchema.g.cs")
            .ShouldContain("((global::PlatformControls.ButtonToken)token).ContentHeight");

        var inaccessibleCompilation = CreateCompilationWithReferences(
            """
            using SharedTokens;

            namespace OtherControls
            {
                internal sealed class OtherToken : CommonButtonToken
                {
                }
            }
            """,
            "OtherControls",
            runtimeReference,
            sharedReference);
        inaccessibleCompilation.GetDiagnostics(TestContext.Current.CancellationToken)
                               .ShouldContain(static diagnostic => diagnostic.Id == "CS0122");
    }

    [Fact]
    public void Produces_The_Same_Terminal_Schema_When_A_Property_Moves_To_An_Abstract_Layer()
    {
        const string directSource = """
            using Avalonia.Controls;
            using AtomUI.Theme.DesignTokens;

            namespace Demo
            {
                public sealed class Button : Control
                {
                }

                [ControlDesignToken]
                internal sealed class ButtonToken : AbstractControlDesignToken
                {
                    public double ContentHeight { get; set; }
                }
            }
            """;
        const string inheritedSource = """
            using Avalonia.Controls;
            using AtomUI.Theme.DesignTokens;

            namespace Demo
            {
                public sealed class Button : Control
                {
                }

                [ControlDesignToken]
                internal abstract class CommonButtonToken : AbstractControlDesignToken
                {
                    public double ContentHeight { get; set; }
                }

                [ControlDesignToken]
                internal sealed class ButtonToken : CommonButtonToken
                {
                }
            }
            """;

        var directOutput = RunGenerator(
            CreateCompilation(directSource, "SchemaEquivalent"),
            out var directDiagnostics);
        var inheritedOutput = RunGenerator(
            CreateCompilation(inheritedSource, "SchemaEquivalent"),
            out var inheritedDiagnostics);

        directDiagnostics.ShouldBeEmpty();
        inheritedDiagnostics.ShouldBeEmpty();
        GetGeneratedSource(inheritedOutput, "TokenResourceConst.g.cs")
            .ShouldBe(GetGeneratedSource(directOutput, "TokenResourceConst.g.cs"));
        GetGeneratedSource(inheritedOutput, "GeneratedThemeSchema.g.cs")
            .ShouldBe(GetGeneratedSource(directOutput, "GeneratedThemeSchema.g.cs"));
    }

    [Fact]
    public void Reports_Diagnostic_When_Own_Token_Name_Conflicts_With_Global_Token()
    {
        RunGenerator(CreateCompilation("""
            using Avalonia.Controls;
            using AtomUI.Theme.DesignTokens;

            namespace Demo
            {
                public sealed class Alert : Control
                {
                }

                [ControlDesignToken]
                internal sealed class AlertToken : AbstractControlDesignToken
                {
                    public double ColorPrimary { get; set; }
                }
            }
            """), out var diagnostics);

        var diagnostic = diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe("ATOMUIGEN019");
        diagnostic.GetMessage().ShouldContain("Own Token 'ColorPrimary'");
        diagnostic.GetMessage().ShouldContain("conflicts with a Global Token");
    }

    private static CSharpCompilation RunGenerator(
        CSharpCompilation compilation,
        out ImmutableArray<Diagnostic> diagnostics,
        params AdditionalText[] additionalTexts)
    {
        return RunGenerator(compilation, out diagnostics, null, additionalTexts);
    }

    private static CSharpCompilation RunGenerator(
        CSharpCompilation compilation,
        out ImmutableArray<Diagnostic> diagnostics,
        AnalyzerConfigOptionsProvider? optionsProvider,
        params AdditionalText[] additionalTexts)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var driver = CSharpGeneratorDriver.Create(
            [new TokenResourceKeyGenerator().AsSourceGenerator()],
            additionalTexts.ToImmutableArray(),
            (CSharpParseOptions)compilation.SyntaxTrees[0].Options,
            new BuiltinCatalogOptionsProvider(optionsProvider));

        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out diagnostics, cancellationToken);
        return (CSharpCompilation)outputCompilation;
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
        string assemblyName = "TokenResourceKeyGeneratorTests")
    {
        var references = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))!
                         .Split(Path.PathSeparator)
                         .Select(path => MetadataReference.CreateFromFile(path))
                         .Cast<MetadataReference>()
                         .ToImmutableArray();

        return CSharpCompilation.Create(
            assemblyName,
            [CSharpSyntaxTree.ParseText(source), CSharpSyntaxTree.ParseText(AtomUIStubs)],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    private static CSharpCompilation CreateCompilationWithReferences(
        string source,
        string assemblyName,
        params MetadataReference[] additionalReferences)
    {
        return CSharpCompilation.Create(
            assemblyName,
            [CSharpSyntaxTree.ParseText(source)],
            GetPlatformReferences().AddRange(additionalReferences),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    private static MetadataReference CreateThemeRuntimeReference()
    {
        return EmitToPortableExecutableReference(CSharpCompilation.Create(
            "AtomUI.Runtime.Stubs",
            [CSharpSyntaxTree.ParseText(AtomUIStubs)],
            GetPlatformReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)));
    }

    private static PortableExecutableReference EmitToPortableExecutableReference(
        CSharpCompilation compilation)
    {
        using var stream = new MemoryStream();
        var emitResult = compilation.Emit(
            stream,
            cancellationToken: TestContext.Current.CancellationToken);
        emitResult.Success.ShouldBeTrue(
            string.Join(Environment.NewLine, emitResult.Diagnostics.Select(static diagnostic => diagnostic.ToString())));
        return MetadataReference.CreateFromImage(stream.ToArray());
    }

    private static ImmutableArray<MetadataReference> GetPlatformReferences()
    {
        return ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))!
               .Split(Path.PathSeparator)
               .Select(static path => MetadataReference.CreateFromFile(path))
               .Cast<MetadataReference>()
               .ToImmutableArray();
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

    private static CSharpCompilation CreateCompilationWithReferencedAvaloniaControls(string source)
    {
        var references = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))!
                         .Split(Path.PathSeparator)
                         .Select(path => MetadataReference.CreateFromFile(path))
                         .Cast<MetadataReference>()
                         .ToImmutableArray();
        var avaloniaReference = CSharpCompilation.Create(
            "Avalonia.Controls",
            [CSharpSyntaxTree.ParseText("""
                namespace Avalonia.Controls
                {
                    public class Control
                    {
                    }

                    public class Button : Control
                    {
                    }
                }
                """)],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .ToMetadataReference();
        var stubsWithoutAvaloniaControl = AtomUIStubs.Replace(
            """
            namespace Avalonia.Controls
            {
                public class Control
                {
                }
            }
            """,
            string.Empty,
            StringComparison.Ordinal);

        return CSharpCompilation.Create(
            "TokenResourceKeyGeneratorTests",
            [CSharpSyntaxTree.ParseText(source), CSharpSyntaxTree.ParseText(stubsWithoutAvaloniaControl)],
            references.Add(avaloniaReference),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    private const string AtomUIStubs = """
        namespace AtomUI.Theme.Resources
        {
            public interface IControlThemesProvider
            {
                string Id { get; }
            }

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

        namespace AtomUI.Theme.Resources
        {
            public enum SharedTokenKind
            {
                ColorPrimary,
                ControlHeight
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
                public ControlTokenIdentity Identity { get; }
                public System.Func<AtomUI.Theme.DesignTokens.AbstractControlDesignToken>? Factory { get; }
                public System.Action<AtomUI.Theme.DesignTokens.AbstractControlDesignToken, AtomUI.Theme.ThemeAppearance>? Evaluator { get; }

                public ControlTokenDescriptor(
                    System.Type controlType,
                    ControlTokenIdentity identity)
                {
                    Identity = identity;
                }

                public ControlTokenDescriptor(
                    System.Type controlType,
                    ControlTokenIdentity identity,
                    System.Collections.Generic.IReadOnlyList<TokenDescriptor> ownTokens,
                    System.Func<AtomUI.Theme.DesignTokens.AbstractControlDesignToken> factory,
                    System.Action<AtomUI.Theme.DesignTokens.AbstractControlDesignToken, AtomUI.Theme.ThemeAppearance> evaluator)
                {
                    Identity = identity;
                    Factory = factory;
                    Evaluator = evaluator;
                }
            }

            public sealed class ControlThemeAssetDescriptor
            {
                public ControlTokenIdentity OwnerIdentity { get; }
                public System.Collections.Generic.IReadOnlyList<ControlTokenIdentity> ReferencedControlIdentities { get; } =
                    System.Array.Empty<ControlTokenIdentity>();
            }

            public sealed class ThemeAlgorithmDescriptor
            {
            }

            public static class ThemeTokenValueParser
            {
                public static T Parse<T>(string value) => default!;
            }

            public static class ThemeTokenValueFormatter
            {
                public static string Format<T>(T value) => string.Empty;
            }

            public static class ThemeResourceValue
            {
                public static object? Project<T>(T value) => value;
            }
        }

        namespace AtomUI.Theme.DesignTokens
        {
            [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false)]
            public sealed class ControlDesignTokenAttribute : System.Attribute
            {
            }

            public sealed class NotTokenDefinitionAttribute : System.Attribute
            {
            }

            public abstract class AbstractDesignToken
            {
            }

            public abstract class AbstractControlDesignToken : AbstractDesignToken
            {
                public virtual void CalculateTokenValues(bool isDark)
                {
                }
            }
        }

        namespace Avalonia.Controls
        {
            public class Control
            {
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
}
