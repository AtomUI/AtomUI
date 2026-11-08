using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;

namespace AtomUI.Generator.Tests.ThemeAssets;

public class ThemeAssetConsumptionAnalyzerTests
{
    private const string SharedRead = "ATOMUIGEN041";
    private const string VisualLiteral = "ATOMUIGEN045";
    private const string MasksSetter = "ATOMUIGEN046";

    // ---- ATOMUIGEN041 -------------------------------------------------------------------

    [Fact]
    public void Reports_SharedTokenResource_When_TargetType_Has_A_Token_Identity()
    {
        var diagnostics = Analyze(Theme("atom:Button", """
            <Setter Property="Foreground" Value="{atom:SharedTokenResource ColorPrimary}" />
            """));

        var diagnostic = diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe(SharedRead);
        diagnostic.Severity.ShouldBe(DiagnosticSeverity.Warning);
        diagnostic.GetMessage().ShouldContain("ButtonTokenResource");
        diagnostic.GetMessage().ShouldContain("ColorPrimary");
    }

    [Fact]
    public void Reports_SharedTokenResource_When_TargetType_Has_No_Token_Identity()
    {
        var diagnostics = Analyze(Theme("atom:InputControlFrame", """
            <Setter Property="Foreground" Value="{atom:SharedTokenResource ColorPrimary}" />
            """));

        var diagnostic = diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe(SharedRead);
        diagnostic.GetMessage().ShouldContain("OwnerTokenResource");
    }

    [Fact]
    public void Reports_The_Kind_Argument_Form_And_The_Static_Type_TargetType_Form()
    {
        var diagnostics = Analyze(Theme("{x:Type atom:Button}", """
            <Setter Property="Foreground" Value="{atom:SharedTokenResource Kind=ColorPrimary}" />
            """));

        var diagnostic = diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe(SharedRead);
        diagnostic.GetMessage().ShouldContain("ColorPrimary");
        diagnostic.GetMessage().ShouldContain("ButtonTokenResource");
    }

    [Theory]
    [InlineData("atom:Button", "{atom:ButtonTokenResource ColorPrimary}")]
    [InlineData("atom:InputControlFrame", "{atom:OwnerTokenResource ColorPrimary}")]
    [InlineData("atom:Button", "{atom:OwnerTokenResource ColorPrimary}")]
    public void Does_Not_Report_Matching_TokenResource_Kinds(string targetType, string value)
    {
        Analyze(Theme(targetType, $"""<Setter Property="Foreground" Value="{value}" />""")).ShouldBeEmpty();
    }

    [Theory]
    [InlineData("atom:Button")]
    [InlineData("atom:InputControlFrame")]
    public void A_Shared_Comment_Directly_Above_The_Element_Allows_SharedTokenResource(string targetType)
    {
        Analyze(Theme(targetType, """
            <!-- shared: the popup surface is common to every control -->
            <Setter Property="Foreground" Value="{atom:SharedTokenResource ColorPrimary}" />
            """)).ShouldBeEmpty();
    }

    [Fact]
    public void The_Shared_Comment_Marker_Is_Case_Insensitive()
    {
        Analyze(Theme("atom:Button", """
            <!--   Shared: reason -->
            <Setter Property="Foreground" Value="{atom:SharedTokenResource ColorPrimary}" />
            """)).ShouldBeEmpty();
    }

    [Fact]
    public void A_Shared_Comment_Applies_To_The_Immediately_Following_Element_Only()
    {
        var diagnostics = Analyze(Theme("atom:Button", """
            <!-- shared: reason -->
            <Setter Property="Foreground" Value="{atom:SharedTokenResource ColorPrimary}" />
            <Setter Property="Background" Value="{atom:SharedTokenResource ColorBgContainer}" />
            """));

        var diagnostic = diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe(SharedRead);
        diagnostic.GetMessage().ShouldContain("ColorBgContainer");
    }

    [Fact]
    public void An_Unrelated_Comment_Does_Not_Allow_SharedTokenResource()
    {
        Analyze(Theme("atom:Button", """
            <!-- layout -->
            <Setter Property="Foreground" Value="{atom:SharedTokenResource ColorPrimary}" />
            """)).ShouldHaveSingleItem().Id.ShouldBe(SharedRead);
    }

    [Fact]
    public void Nested_Styles_Use_The_TargetType_Of_The_Nearest_Enclosing_ControlTheme()
    {
        var diagnostics = Analyze("""
            <ResourceDictionary xmlns="https://github.com/avaloniaui" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" xmlns:atom="using:Demo">
                <ControlTheme x:Key="{x:Type atom:Button}" TargetType="atom:Button">
                    <Style Selector="^:pointerover">
                        <Setter Property="Foreground" Value="{atom:SharedTokenResource ColorPrimary}" />
                    </Style>
                </ControlTheme>
                <ControlTheme x:Key="{x:Type atom:InputControlFrame}" TargetType="atom:InputControlFrame">
                    <Style Selector="^:pointerover">
                        <Setter Property="Foreground" Value="{atom:SharedTokenResource ColorText}" />
                    </Style>
                </ControlTheme>
            </ResourceDictionary>
            """);

        diagnostics.Select(static d => d.Id).ShouldBe([SharedRead, SharedRead]);
        diagnostics[0].GetMessage().ShouldContain("ButtonTokenResource");
        diagnostics[1].GetMessage().ShouldContain("OwnerTokenResource");
    }

    [Fact]
    public void A_TargetType_In_The_Avalonia_Namespace_Has_No_Token_Identity_Even_When_The_Name_Matches()
    {
        var diagnostics = Analyze(Theme("Button", """
            <Setter Property="Foreground" Value="{atom:SharedTokenResource ColorPrimary}" />
            """));

        diagnostics.ShouldHaveSingleItem().GetMessage().ShouldContain("OwnerTokenResource");
    }

    [Fact]
    public void Does_Not_Report_SharedTokenResource_Outside_A_ControlTheme()
    {
        Analyze("""
            <ResourceDictionary xmlns="https://github.com/avaloniaui" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" xmlns:atom="using:Demo">
                <SolidColorBrush x:Key="Demo" Color="{atom:SharedTokenResource ColorPrimary}" />
            </ResourceDictionary>
            """).ShouldBeEmpty();
    }

    [Fact]
    public void Reports_Are_Located_On_The_Offending_Attribute()
    {
        var axaml = Theme("atom:Button", """
            <Setter Property="Background" Value="{atom:ButtonTokenResource ColorBgContainer}" />
            <Setter Property="Foreground"
                    Value="{atom:SharedTokenResource ColorPrimary}" />
            """);
        var diagnostic = Analyze(axaml).ShouldHaveSingleItem();

        var lineSpan = diagnostic.Location.GetLineSpan();
        lineSpan.Path.ShouldBe("Button/Themes/ButtonTheme.axaml");
        // Line 0 is the ControlTheme root, 1 the first Setter, 2 the second Setter, 3 its Value attribute.
        lineSpan.StartLinePosition.Line.ShouldBe(3);
        axaml.Substring(diagnostic.Location.SourceSpan.Start, diagnostic.Location.SourceSpan.Length)
             .ShouldBe("Value=\"{atom:SharedTokenResource ColorPrimary}\"");
    }

    [Fact]
    public void Token_Identities_Published_By_Referenced_Packages_Count_As_Identities()
    {
        var reference = CreateOwnerReference("Dropdown");

        var shared = Analyze(Theme("atom:Dropdown", """
            <Setter Property="Foreground" Value="{atom:SharedTokenResource ColorPrimary}" />
            """), reference);
        var diagnostic = shared.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe(SharedRead);
        diagnostic.GetMessage().ShouldContain("DropdownTokenResource");
        diagnostic.GetMessage().ShouldNotContain("OwnerTokenResource");

        Analyze(Theme("atom:Dropdown", """
            <Setter Property="Foreground" Value="{atom:DropdownTokenResource ColorPrimary}" />
            """), reference).ShouldBeEmpty();

        // Names the referenced package does not publish remain primitives.
        Analyze(Theme("atom:DropdownItem", """
            <Setter Property="Foreground" Value="{atom:SharedTokenResource ColorPrimary}" />
            """), reference).ShouldHaveSingleItem().GetMessage().ShouldContain("OwnerTokenResource");
    }

    // ---- ATOMUIGEN045 -------------------------------------------------------------------

    [Theory]
    [InlineData("CornerRadius", "4")]
    [InlineData("BorderThickness", "1")]
    [InlineData("FontSize", "12")]
    [InlineData("StrokeThickness", "1.5")]
    [InlineData("Padding", "8,4")]
    [InlineData("Margin", "4 8 4 8")]
    [InlineData("Width", "24")]
    [InlineData("MinHeight", "32")]
    [InlineData("FontWeight", "600")]
    [InlineData("Opacity", "0.5")]
    public void Reports_Numeric_Visual_Literal_Attributes(string name, string value)
    {
        var diagnostic = Analyze(Theme("atom:Button", $"""<Border {name}="{value}" />""")).ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe(VisualLiteral);
        diagnostic.Severity.ShouldBe(DiagnosticSeverity.Warning);
        diagnostic.GetMessage().ShouldContain(name);
    }

    [Theory]
    [InlineData("Background", "#FFF")]
    [InlineData("Foreground", "#80FFFFFF")]
    [InlineData("BorderBrush", "#112233")]
    [InlineData("Fill", "#8112233")]
    [InlineData("Stroke", "Red")]
    [InlineData("Color", "red")]
    [InlineData("Background", "CornflowerBlue")]
    public void Reports_Colour_Literal_Attributes(string name, string value)
    {
        if (value == "#8112233")
        {
            // Seven hex digits is not a colour literal.
            Analyze(Theme("atom:Button", $"""<Border {name}="{value}" />""")).ShouldBeEmpty();
            return;
        }

        var diagnostic = Analyze(Theme("atom:Button", $"""<Border {name}="{value}" />""")).ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe(VisualLiteral);
    }

    [Fact]
    public void Reports_Literal_Values_Written_Through_Setters()
    {
        var diagnostics = Analyze(Theme("atom:Button", """
            <Setter Property="CornerRadius" Value="4" />
            <Setter Property="Border.BorderThickness" Value="1" />
            <Setter Property="Background" Value="#FFF" />
            <Setter Property="IsEnabled" Value="True" />
            """));

        diagnostics.Select(static d => d.Id).ShouldBe([VisualLiteral, VisualLiteral, VisualLiteral]);
    }

    [Fact]
    public void Reports_Literals_Inside_Template_Content_And_Nested_Styles()
    {
        var diagnostics = Analyze(Theme("atom:Button", """
            <Setter Property="Template">
                <ControlTemplate>
                    <Border CornerRadius="4" />
                </ControlTemplate>
            </Setter>
            <Style Selector="^:pointerover">
                <Setter Property="Background" Value="#FFF" />
            </Style>
            """));

        diagnostics.Select(static d => d.Id).ShouldBe([VisualLiteral, VisualLiteral]);
    }

    [Fact]
    public void A_Layout_Comment_Directly_Above_The_Element_Allows_Numeric_Literals()
    {
        Analyze(Theme("atom:Button", """
            <!-- layout -->
            <Border Width="1" />
            <!-- Layout: fixed gutter -->
            <Setter Property="Height" Value="8" />
            """)).ShouldBeEmpty();
    }

    [Fact]
    public void A_Layout_Comment_Applies_To_The_Immediately_Following_Element_Only()
    {
        var diagnostics = Analyze(Theme("atom:Button", """
            <!-- layout -->
            <Border Width="1" />
            <Border Height="1" />
            """));

        diagnostics.ShouldHaveSingleItem().GetMessage().ShouldContain("Height");
    }

    [Fact]
    public void A_Layout_Comment_Does_Not_Allow_Colour_Literals()
    {
        Analyze(Theme("atom:Button", """
            <!-- layout -->
            <Border Background="#FFF" />
            """)).ShouldHaveSingleItem().Id.ShouldBe(VisualLiteral);
    }

    [Theory]
    [InlineData("""<Border Background="{atom:ButtonTokenResource ColorBgContainer}" />""")]
    [InlineData("""<Border CornerRadius="{atom:ButtonTokenResource BorderRadius}" />""")]
    [InlineData("""<Border Opacity="0" />""")]
    [InlineData("""<Border Opacity="1" />""")]
    [InlineData("""<Border BorderThickness="0" />""")]
    [InlineData("""<Border Padding="0,0,0,0" />""")]
    [InlineData("""<Border Background="Transparent" />""")]
    [InlineData("""<Border Width="Auto" />""")]
    [InlineData("""<Border Width="{Binding Width}" />""")]
    [InlineData("""<Border Background="{x:Null}" />""")]
    [InlineData("""<Border Tag="4" />""")]
    [InlineData("""<Setter Property="Opacity" Value="0" />""")]
    public void Does_Not_Report_Token_Reads_Zero_Opacity_Extremes_Or_Non_Visual_Attributes(string element)
    {
        Analyze(Theme("atom:Button", element)).ShouldBeEmpty();
    }

    [Fact]
    public void Does_Not_Report_Literals_Outside_ControlTheme_And_Style_Subtrees()
    {
        Analyze("""
            <ResourceDictionary xmlns="https://github.com/avaloniaui" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" xmlns:atom="using:Demo">
                <SolidColorBrush x:Key="Demo" Color="#FFF" />
                <x:Double x:Key="Gap">4</x:Double>
            </ResourceDictionary>
            """).ShouldBeEmpty();
    }

    [Fact]
    public void Reports_Literals_In_A_Style_Root_Without_A_ControlTheme()
    {
        Analyze("""
            <Styles xmlns="https://github.com/avaloniaui" xmlns:atom="using:Demo">
                <Style Selector="Border.demo">
                    <Setter Property="Background" Value="#FFF" />
                </Style>
            </Styles>
            """).ShouldHaveSingleItem().Id.ShouldBe(VisualLiteral);
    }

    // ---- ATOMUIGEN046 -------------------------------------------------------------------

    [Fact]
    public void Reports_Template_Attribute_Masking_A_State_Setter()
    {
        var diagnostic = Analyze(Theme("atom:Button", """
            <Setter Property="Template">
                <ControlTemplate>
                    <Border Name="Frame" Background="{atom:ButtonTokenResource DefaultBg}" />
                </ControlTemplate>
            </Setter>
            <Style Selector="^:pointerover /template/ Border#Frame">
                <Setter Property="Background" Value="{atom:ButtonTokenResource HoverBg}" />
            </Style>
            """)).ShouldHaveSingleItem();

        diagnostic.Id.ShouldBe(MasksSetter);
        diagnostic.Severity.ShouldBe(DiagnosticSeverity.Warning);
        diagnostic.GetMessage().ShouldContain("Background");
        diagnostic.GetMessage().ShouldContain("Frame");
    }

    [Fact]
    public void Template_Masking_Is_Reported_On_The_Attribute_Of_The_Template_Node()
    {
        var axaml = Theme("atom:Button", """
            <Setter Property="Template">
                <ControlTemplate>
                    <Border Name="Frame" Background="{atom:ButtonTokenResource DefaultBg}" />
                </ControlTemplate>
            </Setter>
            <Style Selector="^:pointerover /template/ Border#Frame">
                <Setter Property="Background" Value="{atom:ButtonTokenResource HoverBg}" />
            </Style>
            """);
        var diagnostic = Analyze(axaml).ShouldHaveSingleItem();

        axaml.Substring(diagnostic.Location.SourceSpan.Start, diagnostic.Location.SourceSpan.Length)
             .ShouldBe("Background=\"{atom:ButtonTokenResource DefaultBg}\"");
        diagnostic.Location.GetLineSpan().StartLinePosition.Line.ShouldBe(3);
    }

    [Theory]
    [InlineData("^.primary /template/ Border#Frame")]
    [InlineData("^ /template/ Border#Frame:disabled")]
    [InlineData("^:pointerover /template/ #Frame")]
    public void Class_And_Pseudo_Class_Selectors_Are_State_Selectors(string selector)
    {
        Analyze(MaskTheme(selector, "Background", "Background", "Border")).ShouldHaveSingleItem().Id.ShouldBe(MasksSetter);
    }

    [Fact]
    public void Reports_A_Literal_Template_Attribute_That_Masks_A_State_Setter()
    {
        var diagnostics = Analyze(MaskTheme("^:pointerover /template/ Border#Frame", "Opacity", "Opacity", "Border"));

        diagnostics.Select(static d => d.Id).ShouldContain(MasksSetter);
    }

    [Fact]
    public void Does_Not_Report_When_The_Default_Lives_In_A_Plain_Style_Setter()
    {
        Analyze("""
            <ControlTheme xmlns="https://github.com/avaloniaui" xmlns:atom="using:Demo" TargetType="atom:Button">
                <Setter Property="Template">
                    <ControlTemplate>
                        <Border Name="Frame" />
                    </ControlTemplate>
                </Setter>
                <Style Selector="^ /template/ Border#Frame">
                    <Setter Property="Background" Value="{atom:ButtonTokenResource DefaultBg}" />
                </Style>
                <Style Selector="^:pointerover /template/ Border#Frame">
                    <Setter Property="Background" Value="{atom:ButtonTokenResource HoverBg}" />
                </Style>
            </ControlTheme>
            """).ShouldBeEmpty();
    }

    [Theory]
    // the state Setter targets another property
    [InlineData("^:pointerover /template/ Border#Frame", "Background", "BorderBrush", "Border")]
    // the selector resolves to a different node name
    [InlineData("^:pointerover /template/ Border#Other", "Background", "Background", "Border")]
    // the selector's type cannot match the named node
    [InlineData("^:pointerover /template/ TextBlock#Frame", "Background", "Background", "Border")]
    // a descendant of the named node is targeted, not the node itself
    [InlineData("^:pointerover /template/ Border#Frame > TextBlock", "Background", "Background", "Border")]
    // not the /template/ <Type>#<Name> form
    [InlineData("^:pointerover Border#Frame", "Background", "Background", "Border")]
    [InlineData("^:pointerover /template/ Border.frame", "Background", "Background", "Border")]
    public void Does_Not_Report_Selectors_That_Cannot_Be_Statically_Resolved_To_The_Node_Property(
        string selector,
        string templateAttribute,
        string setterProperty,
        string nodeType)
    {
        Analyze(MaskTheme(selector, templateAttribute, setterProperty, nodeType)).ShouldBeEmpty();
    }

    [Fact]
    public void Does_Not_Report_A_State_Style_Of_Another_ControlTheme()
    {
        Analyze("""
            <ResourceDictionary xmlns="https://github.com/avaloniaui" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" xmlns:atom="using:Demo">
                <ControlTheme x:Key="A" TargetType="atom:Button">
                    <Setter Property="Template">
                        <ControlTemplate>
                            <Border Name="Frame" Background="{atom:ButtonTokenResource DefaultBg}" />
                        </ControlTemplate>
                    </Setter>
                </ControlTheme>
                <ControlTheme x:Key="B" TargetType="atom:Button">
                    <Style Selector="^:pointerover /template/ Border#Frame">
                        <Setter Property="Background" Value="{atom:ButtonTokenResource HoverBg}" />
                    </Style>
                </ControlTheme>
            </ResourceDictionary>
            """).ShouldBeEmpty();
    }

    [Fact]
    public void Does_Not_Report_Template_Bindings_Or_Non_Token_Markup()
    {
        Analyze(Theme("atom:Button", """
            <Setter Property="Template">
                <ControlTemplate>
                    <Border Name="Frame" Background="{TemplateBinding Background}" />
                </ControlTemplate>
            </Setter>
            <Style Selector="^:pointerover /template/ Border#Frame">
                <Setter Property="Background" Value="{atom:ButtonTokenResource HoverBg}" />
            </Style>
            """)).ShouldBeEmpty();
    }

    // ---- infrastructure -----------------------------------------------------------------

    private static string Theme(string targetType, string body)
    {
        return $"""
            <ControlTheme xmlns="https://github.com/avaloniaui" xmlns:atom="using:Demo" TargetType="{targetType}">
            {body}
            </ControlTheme>
            """;
    }

    private static string MaskTheme(string selector, string templateAttribute, string setterProperty, string nodeType)
    {
        var value = templateAttribute == "Opacity" ? "0.5" : "{atom:ButtonTokenResource DefaultBg}";
        return Theme("atom:Button", $$"""
            <Setter Property="Template">
                <ControlTemplate>
                    <{{nodeType}} Name="Frame" {{templateAttribute}}="{{value}}" />
                </ControlTemplate>
            </Setter>
            <Style Selector="{{selector}}">
                <Setter Property="{{setterProperty}}" Value="{atom:ButtonTokenResource HoverBg}" />
            </Style>
            """);
    }

    private static ImmutableArray<Diagnostic> Analyze(string axaml, params MetadataReference[] additionalReferences)
    {
        var references = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))!
                         .Split(Path.PathSeparator)
                         .Select(static path => MetadataReference.CreateFromFile(path))
                         .Cast<MetadataReference>()
                         .ToImmutableArray();
        var compilation = CSharpCompilation.Create(
            "ThemeAssetConsumptionAnalyzerTests",
            [
                CSharpSyntaxTree.ParseText(TokenSource, path: "TokenSource.cs"),
                CSharpSyntaxTree.ParseText(ThemeSchemaGeneratorTests.AtomUIStubs)
            ],
            references.AddRange(additionalReferences),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var theme = new InMemoryAdditionalText("Button/Themes/ButtonTheme.axaml", axaml);
        var driver = CSharpGeneratorDriver.Create(
            [new TokenResourceKeyGenerator().AsSourceGenerator()],
            ImmutableArray.Create<AdditionalText>(theme),
            (CSharpParseOptions)compilation.SyntaxTrees[0].Options,
            new BuiltinCatalogOptionsProvider(null));
        driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out _,
            out var diagnostics,
            TestContext.Current.CancellationToken);
        return diagnostics
               .Where(static d => d.Id is SharedRead or VisualLiteral or MasksSetter)
               .OrderBy(static d => d.Location.SourceSpan.Start)
               .ToImmutableArray();
    }

    // A referenced package publishes its Token owners through assembly-level ThemeBuildOwnerAttribute metadata.
    private static MetadataReference CreateOwnerReference(string controlName)
    {
        var source = $$"""
            [assembly: AtomUI.Theme.Definitions.Compiled.ThemeBuildOwner(1, "AtomUI", "{{controlName}}", "Other, Version=1.0.0.0", "Other.{{controlName}}", "global::Other.{{controlName}}Tokens", "{{new string('0', 64)}}")]

            namespace AtomUI.Theme.Definitions.Compiled
            {
                [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple = true)]
                public sealed class ThemeBuildOwnerAttribute : System.Attribute
                {
                    public ThemeBuildOwnerAttribute(int version, string catalog, string id, string ownerAssembly,
                        string ownerType, string identityProvider, string digest)
                    {
                    }
                }
            }
            """;
        return CSharpCompilation.Create(
                "OtherPackage",
                [CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.Current.CancellationToken)],
                ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))!
                    .Split(Path.PathSeparator)
                    .Select(static path => MetadataReference.CreateFromFile(path)),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .ToMetadataReference();
    }

    private const string TokenSource = """
        using AtomUI.Theme.DesignTokens;

        namespace Demo
        {
            [ControlDesignToken]
            internal sealed class ButtonToken : AbstractControlDesignToken
            {
                public double Height { get; set; }

                public override void CalculateTokenValues(bool isDark)
                {
                    Height = EffectiveGlobalToken.ControlHeight;
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

        public override Microsoft.CodeAnalysis.Text.SourceText GetText(CancellationToken cancellationToken = default)
        {
            return _text;
        }
    }
}
