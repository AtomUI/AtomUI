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
    public void Does_Not_Report_Matching_TokenResource_Kinds(string targetType, string value)
    {
        Analyze(Theme(targetType, $"""<Setter Property="Foreground" Value="{value}" />""")).ShouldBeEmpty();
    }

    [Theory]
    [InlineData("{atom:OwnerTokenResource ColorPrimary}")]
    [InlineData("{atom:OwnerTokenResource Kind=ColorPrimary}")]
    public void Reports_OwnerTokenResource_When_TargetType_Has_A_Token_Identity(string value)
    {
        var diagnostic = Analyze(Theme("atom:Button", $"""<Setter Property="Foreground" Value="{value}" />"""))
            .ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe(SharedRead);
        diagnostic.GetMessage().ShouldContain("OwnerTokenResource");
        diagnostic.GetMessage().ShouldContain("ButtonTokenResource");
        diagnostic.GetMessage().ShouldContain("owner:");
        diagnostic.GetMessage().ShouldContain("ColorPrimary");
    }

    [Fact]
    public void Reports_The_Element_Form_Of_OwnerTokenResource_On_An_Identity_TargetType()
    {
        var diagnostic = Analyze(Theme("atom:Button", """
            <Setter Property="Foreground">
                <atom:OwnerTokenResource Kind="ColorPrimary" />
            </Setter>
            """)).ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe(SharedRead);
        diagnostic.GetMessage().ShouldContain("ColorPrimary");
    }

    [Theory]
    [InlineData("<!-- owner: reused by an identity-less part -->")]
    [InlineData("<!--   OWNER: reused by an identity-less part -->")]
    public void An_Owner_Comment_Directly_Before_The_ControlTheme_Allows_OwnerTokenResource(string marker)
    {
        Analyze(marker + "\n" + Theme("atom:Button", """
            <Setter Property="Foreground" Value="{atom:OwnerTokenResource ColorPrimary}" />
            """)).ShouldBeEmpty();
    }

    [Fact]
    public void An_Owner_Comment_Before_A_Theme_In_A_Resource_Dictionary_Applies_To_That_Theme_Only()
    {
        var diagnostics = Analyze("""
            <ResourceDictionary xmlns="https://github.com/avaloniaui" xmlns:atom="using:Demo">
                <!-- owner: reused by an identity-less part -->
                <ControlTheme TargetType="atom:Button">
                    <Setter Property="Foreground" Value="{atom:OwnerTokenResource ColorPrimary}" />
                </ControlTheme>
                <ControlTheme TargetType="atom:Button">
                    <Setter Property="Foreground" Value="{atom:OwnerTokenResource ColorText}" />
                </ControlTheme>
            </ResourceDictionary>
            """);
        diagnostics.ShouldHaveSingleItem().GetMessage().ShouldContain("ColorText");
    }

    [Fact]
    public void An_Owner_Comment_Inside_The_ControlTheme_Does_Not_Allow_OwnerTokenResource()
    {
        Analyze(Theme("atom:Button", """
            <!-- owner: reused by an identity-less part -->
            <Setter Property="Foreground" Value="{atom:OwnerTokenResource ColorPrimary}" />
            """)).ShouldHaveSingleItem().Id.ShouldBe(SharedRead);
    }

    [Fact]
    public void OwnerTokenResource_On_A_TargetType_Without_Identity_Needs_No_Marker()
    {
        Analyze(Theme("atom:InputControlFrame", """
            <Setter Property="Foreground" Value="{atom:OwnerTokenResource ColorPrimary}" />
            <Setter Property="Background">
                <atom:OwnerTokenResource Kind="ColorBgContainer" />
            </Setter>
            """)).ShouldBeEmpty();
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

    [Theory]
    [InlineData("atom:Button")]
    [InlineData("atom:InputControlFrame")]
    [InlineData("Border")]
    public void Reports_SharedTokenResourceValue_In_Any_ControlTheme(string targetType)
    {
        var diagnostic = Analyze(Theme(targetType, """
            <Setter Property="Transitions">
                <Transitions>
                    <DoubleTransition Property="Opacity" Duration="{atom:SharedTokenResourceValue Kind=MotionDurationMid}" />
                </Transitions>
            </Setter>
            """)).ShouldHaveSingleItem();

        diagnostic.Id.ShouldBe(SharedRead);
        diagnostic.Severity.ShouldBe(DiagnosticSeverity.Warning);
        diagnostic.GetMessage().ShouldContain("SharedTokenResourceValue");
        diagnostic.GetMessage().ShouldContain("MotionDurationMid");
        diagnostic.GetMessage().ShouldContain("TokenTransitions");
    }

    [Fact]
    public void A_Shared_Comment_Does_Not_Allow_SharedTokenResourceValue()
    {
        var diagnostic = Analyze(Theme("atom:Button", """
            <!-- shared: every control animates alike -->
            <DoubleTransition Property="Opacity" Duration="{atom:SharedTokenResourceValue MotionDurationSlow}" />
            """)).ShouldHaveSingleItem();

        diagnostic.Id.ShouldBe(SharedRead);
        diagnostic.GetMessage().ShouldContain("MotionDurationSlow");
    }

    [Fact]
    public void SharedTokenResourceValue_Report_Is_Located_On_The_Offending_Attribute()
    {
        var axaml = Theme("atom:Button", """
            <DoubleTransition Property="Opacity"
                              Duration="{atom:SharedTokenResourceValue Kind=MotionDurationMid}" />
            """);
        var diagnostic = Analyze(axaml).ShouldHaveSingleItem();

        axaml.Substring(diagnostic.Location.SourceSpan.Start, diagnostic.Location.SourceSpan.Length)
             .ShouldBe("Duration=\"{atom:SharedTokenResourceValue Kind=MotionDurationMid}\"");
    }

    [Theory]
    [InlineData("atom:Button", "Duration", "{atom:SharedTokenResource MotionDurationMid}", "SharedTokenResource")]
    [InlineData("atom:InputControlFrame", "Duration", "{atom:OwnerTokenResource MotionDurationMid}", "OwnerTokenResource")]
    [InlineData("atom:Button", "Duration", "{atom:ButtonTokenResource Kind=MotionDurationMid}", "ButtonTokenResource")]
    [InlineData("atom:Button", "Delay", "{atom:ButtonTokenResource MotionDurationMid}", "ButtonTokenResource")]
    public void Reports_TokenResource_Reads_On_Transition_Durations_With_The_TokenTransitions_Remedy(
        string targetType, string attribute, string value, string extension)
    {
        var diagnostic = Analyze(Theme(targetType, $"""
            <Setter Property="Template">
                <ControlTemplate>
                    <Border Name="Frame">
                        <Border.Transitions>
                            <Transitions>
                                <atom:SolidColorBrushTransition Property="Background" {attribute}="{value}" />
                            </Transitions>
                        </Border.Transitions>
                    </Border>
                </ControlTemplate>
            </Setter>
            """)).ShouldHaveSingleItem();

        diagnostic.Id.ShouldBe(SharedRead);
        var message = diagnostic.GetMessage();
        message.ShouldContain(extension);
        message.ShouldContain("MotionDurationMid");
        message.ShouldContain("atom:TokenTransitions.Duration");
        // The generic remedies would bind a value the transition cannot resolve.
        message.ShouldNotContain("use OwnerTokenResource");
        message.ShouldNotContain("use ButtonTokenResource");
        message.ShouldNotContain("shared: reason");
    }

    [Fact]
    public void A_Shared_Comment_Does_Not_Allow_A_SharedTokenResource_Transition_Duration()
    {
        Analyze(Theme("atom:Button", """
            <!-- shared: every control animates alike -->
            <DoubleTransition Property="Opacity" Duration="{atom:SharedTokenResource MotionDurationMid}" />
            """)).ShouldHaveSingleItem().GetMessage().ShouldContain("atom:TokenTransitions.Duration");
    }

    [Fact]
    public void Does_Not_Report_Token_Declared_Or_Literal_Transition_Durations()
    {
        Analyze(Theme("atom:Button", """
            <Setter Property="Transitions">
                <atom:TokenTransitions>
                    <Transitions>
                        <DoubleTransition Property="Opacity" atom:TokenTransitions.Duration="MotionDurationMid" />
                        <DoubleTransition Property="Width" Duration="0:0:0.2" Delay="0:0:0.1" />
                    </Transitions>
                </atom:TokenTransitions>
            </Setter>
            """)).ShouldBeEmpty();
    }

    [Fact]
    public void Does_Not_Report_TokenTransitions_Nor_SharedTokenResourceValue_In_Application_Styles()
    {
        Analyze(Theme("atom:Button", """
            <Setter Property="Transitions">
                <atom:TokenTransitions>
                    <Transitions>
                        <DoubleTransition Property="Opacity" atom:TokenTransitions.Duration="MotionDurationMid" />
                    </Transitions>
                </atom:TokenTransitions>
            </Setter>
            """)).ShouldBeEmpty();
        Analyze("""
            <Styles xmlns="https://github.com/avaloniaui" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" xmlns:atom="using:Demo">
                <Style Selector="Border.demo">
                    <Setter Property="Transitions">
                        <Transitions>
                            <DoubleTransition Property="Opacity" Duration="{atom:SharedTokenResourceValue Kind=MotionDurationMid}" />
                        </Transitions>
                    </Setter>
                </Style>
            </Styles>
            """).ShouldBeEmpty();
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
    [InlineData("Spacing", "8")]
    [InlineData("ZIndex", "1000")]
    [InlineData("BadgeOffset", "-10, 4")]
    [InlineData("HorizontalOffset", "6")]
    [InlineData("Margin", "-1,0,0,0")]
    public void Reports_Spacing_ZIndex_Offset_And_Signed_Literals_Through_Setters(string property, string value)
    {
        var diagnostic = Analyze(Theme("atom:Button", $"""<Setter Property="{property}" Value="{value}" />"""))
            .ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe(VisualLiteral);
        diagnostic.GetMessage().ShouldContain(property);
    }

    [Theory]
    [InlineData("Spacing", "8")]
    [InlineData("ZIndex", "1000")]
    [InlineData("BadgeOffset", "-10, 4")]
    [InlineData("HorizontalOffset", "6")]
    public void A_Layout_Comment_Allows_Spacing_ZIndex_And_Offset_Literals(string property, string value)
    {
        Analyze(Theme("atom:Button", $"""
            <!-- layout: stacking and placement constants -->
            <Setter Property="{property}" Value="{value}" />
            """)).ShouldBeEmpty();
    }

    [Theory]
    [InlineData("Spacing", "0")]
    [InlineData("BadgeOffset", "0, 0")]
    [InlineData("OffsetX", "4")]
    public void Zero_Placements_And_Names_Not_Ending_In_Offset_Are_Not_Reported(string property, string value)
    {
        Analyze(Theme("atom:Button", $"""<Setter Property="{property}" Value="{value}" />""")).ShouldBeEmpty();
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

    // ---- retired ATOMUIGEN046 -----------------------------------------------------------

    [Fact]
    public void Template_Attribute_Defaults_Next_To_State_Setters_Report_Nothing()
    {
        // Former ATOMUIGEN046 positive case. ControlTemplate node values have Template priority, below the triggered
        // state Style, so they cannot mask the state Setter and the rule is retired.
        Analyze(Theme("atom:Button", """
            <Setter Property="Template">
                <ControlTemplate>
                    <Border Name="Frame" Background="{atom:ButtonTokenResource DefaultBg}" />
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
               .Where(static d => d.Id is SharedRead or VisualLiteral)
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
