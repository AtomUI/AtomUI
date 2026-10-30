using System.Collections.Immutable;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;

namespace AtomUI.Generator.Tests.SemanticParts;

public class SemanticPartGeneratorTests
{
    [Fact]
    public void Generates_Implicit_Root_Declared_Parts_Constants_And_Package_Registration()
    {
        var outputCompilation = RunGenerator(ButtonSource, out var diagnostics, ButtonTheme);

        diagnostics.ShouldBeEmpty();
        outputCompilation.GetDiagnostics(TestContext.Current.CancellationToken)
                         .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
                         .ShouldBeEmpty();

        var manifest = GetGeneratedSource(outputCompilation, "GeneratedSemanticPartManifest.g.cs");
        manifest.ShouldContain("internal static class GeneratedSemanticPartManifest");
        manifest.ShouldContain("typeof(global::Demo.Button)");
        manifest.ShouldContain("global::AtomUI.Theme.Schema.ControlTokenIdentity.ForControl(typeof(global::Demo.Button), \"AtomUI\", \"Button\")");
        manifest.ShouldContain("\"root\"");
        manifest.ShouldContain("null,");
        manifest.ShouldContain("SemanticPartCustomization.Root");
        manifest.ShouldContain("\"icon\"");
        manifest.ShouldContain("\"semantic-icon\"");
        manifest.ShouldContain("\"/template/ .semantic-icon\"");
        manifest.ShouldContain("typeof(global::Avalonia.Controls.Control)");
        manifest.ShouldContain("SemanticPartCardinality.Multiple");
        manifest.ShouldContain("\"content\"");
        manifest.ShouldContain("\"semantic-content\"");
        manifest.ShouldContain("typeof(global::Avalonia.Controls.Presenters.ContentPresenter)");
        manifest.ShouldContain("typeof(global::AtomUI.Theme.Styling.ButtonIconStyle)");
        manifest.ShouldContain("typeof(global::AtomUI.Theme.Styling.ButtonContentStyle)");
        manifest.ShouldNotContain("Activator.CreateInstance");
        manifest.ShouldNotContain("Assembly.GetTypes");

        var xmlns = GetGeneratedSource(outputCompilation, "SemanticPartXmlnsDefinition.g.cs");
        xmlns.ShouldContain("Avalonia.Metadata.XmlnsDefinition");
        xmlns.ShouldContain("https://atomui.net");
        xmlns.ShouldContain("AtomUI.Theme.Styling");

        var constants = GetGeneratedSource(outputCompilation, "ButtonSemanticParts.g.cs");
        constants.ShouldContain("internal static class ButtonSemanticParts");
        constants.ShouldContain("internal const string Icon = \"icon\"");
        constants.ShouldContain("internal const string IconClass = \"semantic-icon\"");
        constants.ShouldContain("internal const string IconSelectorRoute = \"/template/ .semantic-icon\"");
        constants.ShouldContain("internal const string Content = \"content\"");
        constants.ShouldContain("internal const string ContentClass = \"semantic-content\"");
        constants.ShouldContain("internal const string ContentSelectorRoute = \"/template/ .semantic-content\"");
        constants.ShouldNotContain("RootClass");

        manifest.ShouldContain("CreateSemanticDescriptor_");
        manifest.ShouldNotContain("GetDescriptors()");

    }

    /// <summary>
    /// 原先本用例用带控制字符的 <c>Since</c> 证明 manifest writer 会转义控制字符。引入
    /// <c>ATOMUIGEN038</c> 后该输入按契约非法（<c>Since</c> 必须是三段式发布版本），因此
    /// "合法声明携带控制字符" 这一前提不再成立：<c>Name</c> / <c>Path</c> / <c>SelectorClass</c>
    /// 只允许字母数字与连字符，<c>ThemePropertyName</c> 需解析到真实成员，都不接受控制字符。
    /// 该用例改为锁定新的拒绝契约；writer 的转义逻辑保留为纵深防御，但经声明字段已不可达。
    /// </summary>
    [Fact]
    public void Rejects_Control_Characters_In_Since_Metadata()
    {
        const string source = """
            using AtomUI.Theme;
            using Avalonia.Controls;

            namespace Demo;

            [SemanticPart(
                "content",
                SelectorClass = "semantic-content",
                ContractType = typeof(Control),
                RuntimeCreated = true,
                SelectorRoute = "> .semantic-content",
                Since = "6.0\n\t\u0001")]
            public partial class EscapedSinceOwner : Control
            {
            }
            """;

        _ = RunGenerator(source, out var diagnostics);

        diagnostics.ShouldContain(diagnostic =>
            diagnostic.Id == "ATOMUIGEN038" && diagnostic.Severity == DiagnosticSeverity.Error);
    }

    [Theory]
    // 隐式 root 的 Since 取该 Control 已声明 Part 中最早的发布版本：root 随 Control 的第一个 Part 一起引入。
    [InlineData("6.2.0", "6.2.0", "6.2.0")]
    [InlineData("6.2.0", "6.4.1", "6.2.0")]
    [InlineData("7.0.0", "6.10.3", "6.10.3")]
    public void Implicit_Root_Since_Uses_The_Earliest_Declared_Part_Version(
        string iconSince,
        string contentSince,
        string expectedRootSince)
    {
        var source = $$"""
            using AtomUI.Theme;
            using Avalonia.Controls;
            using Avalonia.Controls.Presenters;

            namespace Demo;

            [SemanticPart(
                "icon",
                SelectorClass = "semantic-icon",
                SelectorRoute = "/template/ .semantic-icon",
                ContractType = typeof(Control),
                RuntimeCreated = true,
                Since = "{{iconSince}}")]
            [SemanticPart(
                "content",
                SelectorClass = "semantic-content",
                SelectorRoute = "/template/ .semantic-content",
                ContractType = typeof(ContentPresenter),
                RuntimeCreated = true,
                Since = "{{contentSince}}")]
            public partial class Button : Control
            {
            }
            """;

        var outputCompilation = RunGenerator(source, out var diagnostics);

        diagnostics.ShouldBeEmpty();
        var manifest = GetGeneratedSource(outputCompilation, "GeneratedSemanticPartManifest.g.cs");

        // root 描述块形如 root/root/null/typeof(...)/Single/Root/null/false/<since>/false)
        manifest.ShouldContain($"                        \"{expectedRootSince}\",");
    }

    /// <summary>
    /// root 是生成器隐式加入的描述块，control 上没有声明可以承载它的版本，因此必须从声明的 Part 推导。
    /// 这里钉住它不能退化为硬编码版本线，否则会与 Part 的版本声明不一致，而 root 自身没有声明可承载该值。
    /// </summary>
    [Fact]
    public void Implicit_Root_Since_Is_A_Three_Part_Release_Version()
    {
        var outputCompilation = RunGenerator(ButtonSource, out _, ButtonTheme);
        var manifest = GetGeneratedSource(outputCompilation, "GeneratedSemanticPartManifest.g.cs");

        var rootBlockStart = manifest.IndexOf("SemanticPartCustomization.Root", StringComparison.Ordinal);
        rootBlockStart.ShouldBeGreaterThanOrEqualTo(0);
        var rootBlock = manifest[rootBlockStart..manifest.IndexOf("false)", rootBlockStart, StringComparison.Ordinal)];

        var since = Regex.Match(rootBlock, "\"(\\d+)\\.(\\d+)\\.(\\d+)\"");
        since.Success.ShouldBeTrue($"implicit root must carry a three-part release version, got: {rootBlock}");
    }

    [Fact]
    public void Generates_Public_Semantic_Style_Types_With_Owner_And_Fluent_Route()
    {
        var outputCompilation = RunGenerator(ButtonSource, out var diagnostics, ButtonTheme);

        diagnostics.ShouldBeEmpty();
        var iconStyle = GetGeneratedSource(outputCompilation, "ButtonIconStyle.g.cs");

        iconStyle.ShouldContain("namespace AtomUI.Theme.Styling;");
        iconStyle.ShouldContain("public sealed class ButtonIconStyle : global::Avalonia.Styling.Style");
        iconStyle.ShouldContain(".Nesting()");
        iconStyle.ShouldNotContain(".Is<global::Demo.Button>()");
        iconStyle.ShouldContain(".Template()");
        iconStyle.ShouldContain(".Class(\"semantic-icon\")");
        iconStyle.ShouldNotContain("x:SetterTargetType");

        var manifest = GetGeneratedSource(outputCompilation, "GeneratedSemanticPartManifest.g.cs");
        manifest.ShouldContain("typeof(global::AtomUI.Theme.Styling.ButtonIconStyle)");
        manifest.ShouldContain("typeof(global::AtomUI.Theme.Styling.ButtonContentStyle)");
    }

    [Fact]
    public void Reports_Style_Type_Name_Collisions_Within_One_Control()
    {
        const string source = """
            using AtomUI.Theme;
            using Avalonia.Controls;

            namespace Demo;

            [SemanticPart(
                "fooBar",
                Path = "fooBar",
                SelectorClass = "semantic-foo-bar",
                ContractType = typeof(Control),
                RuntimeCreated = true,
                SelectorRoute = "> .semantic-foo-bar",
                Since = "6.2.0")]
            [SemanticPart(
                "nestedFooBar",
                Path = "foo.bar",
                SelectorClass = "semantic-nested-foo-bar",
                ContractType = typeof(Control),
                RuntimeCreated = true,
                SelectorRoute = "> .semantic-nested-foo-bar",
                Since = "6.2.0")]
            public partial class CollisionOwner : Control
            {
            }
            """;

        var output = RunGenerator(source, out var diagnostics);

        diagnostics.Count(static diagnostic => diagnostic.Id == "ATOMUIGEN037").ShouldBe(2);
        diagnostics.ShouldAllBe(diagnostic =>
            diagnostic.Id != "ATOMUIGEN037" ||
            diagnostic.GetMessage().Contains(
                "AtomUI.Theme.Styling.CollisionOwnerFooBarStyle",
                StringComparison.Ordinal));
        HasGeneratedSource(output, "CollisionOwnerFooBarStyle.g.cs").ShouldBeFalse();
    }

    [Fact]
    public void Reports_Style_Type_Name_Collisions_Between_Controls_With_The_Same_Simple_Name()
    {
        const string source = """
            using AtomUI.Theme;
            using Avalonia.Controls;

            namespace First
            {
                [SemanticPart(
                    "content",
                    SelectorClass = "semantic-content",
                    ContractType = typeof(Control),
                    RuntimeCreated = true,
                    SelectorRoute = "> .semantic-content",
                    Since = "6.2.0")]
                public partial class Repeated : Control
                {
                }
            }

            namespace Second
            {
                [SemanticPart(
                    "content",
                    SelectorClass = "semantic-content",
                    ContractType = typeof(Control),
                    RuntimeCreated = true,
                    SelectorRoute = "> .semantic-content",
                    Since = "6.2.0")]
                public partial class Repeated : Control
                {
                }
            }
            """;

        var output = RunGenerator(source, out var diagnostics);

        diagnostics.Count(static diagnostic => diagnostic.Id == "ATOMUIGEN037").ShouldBe(2);
        HasGeneratedSource(output, "RepeatedContentStyle.g.cs").ShouldBeFalse();
    }

    [Fact]
    public void Reports_When_A_Source_Type_Already_Occupies_The_Generated_Style_Identity()
    {
        const string source = """
            using AtomUI.Theme;
            using Avalonia.Controls;

            namespace Demo
            {
                [SemanticPart(
                    "content",
                    SelectorClass = "semantic-content",
                    ContractType = typeof(Control),
                    RuntimeCreated = true,
                    SelectorRoute = "> .semantic-content",
                    Since = "6.2.0")]
                public partial class Occupied : Control
                {
                }
            }

            namespace AtomUI.Theme.Styling
            {
                internal sealed class OccupiedContentStyle
                {
                }
            }
            """;

        var output = RunGenerator(source, out var diagnostics);

        diagnostics.ShouldContain(diagnostic =>
            diagnostic.Id == "ATOMUIGEN037" &&
            diagnostic.GetMessage().Contains("existing source type", StringComparison.Ordinal));
        HasGeneratedSource(output, "OccupiedContentStyle.g.cs").ShouldBeFalse();
    }

    [Fact]
    public void Reports_When_A_Referenced_Canonical_Xmlns_Exports_The_Same_Public_Style_Name()
    {
        var reference = CreateSemanticStyleReference(
            "Referenced.Semantic.Styles",
            "https://atomui.net",
            "AtomUI.Theme.Styling",
            "ButtonIconStyle",
            Accessibility.Public);

        var output = RunGenerator(
            ButtonSource,
            [reference],
            out var diagnostics,
            ButtonTheme);

        diagnostics.ShouldContain(diagnostic =>
            diagnostic.Id == "ATOMUIGEN037" &&
            diagnostic.GetMessage().Contains("Referenced.Semantic.Styles", StringComparison.Ordinal));
        HasGeneratedSource(output, "ButtonIconStyle.g.cs").ShouldBeFalse();
        HasGeneratedSource(output, "ButtonContentStyle.g.cs").ShouldBeFalse();
    }

    [Fact]
    public void Ignores_Referenced_Style_Names_That_Are_Not_Exported_Through_The_Canonical_Xmlns()
    {
        var reference = CreateSemanticStyleReference(
            "Referenced.Private.Semantic.Styles",
            "https://atomui.net",
            "AtomUI.Theme.Styling",
            "ButtonIconStyle",
            Accessibility.Internal);

        var output = RunGenerator(
            ButtonSource,
            [reference],
            out var diagnostics,
            ButtonTheme);

        diagnostics.ShouldBeEmpty();
        HasGeneratedSource(output, "ButtonIconStyle.g.cs").ShouldBeTrue();
    }

    [Fact]
    public void Does_Not_Emit_A_Duplicate_Canonical_Xmlns_Definition_In_The_Source_Assembly()
    {
        const string source = """
            using AtomUI.Theme;
            using Avalonia.Controls;

            [assembly: Avalonia.Metadata.XmlnsDefinition("https://atomui.net", "AtomUI.Theme.Styling")]

            namespace Demo;

            [SemanticPart(
                "content",
                SelectorClass = "semantic-content",
                ContractType = typeof(Control),
                RuntimeCreated = true,
                SelectorRoute = "> .semantic-content",
                Since = "6.2.0")]
            public partial class ExistingXmlns : Control
            {
            }
            """;

        var output = RunGenerator(source, out var diagnostics);

        diagnostics.ShouldBeEmpty();
        HasGeneratedSource(output, "SemanticPartXmlnsDefinition.g.cs").ShouldBeFalse();
        HasGeneratedSource(output, "ExistingXmlnsContentStyle.g.cs").ShouldBeTrue();
    }

    [Fact]
    public void Emits_The_Canonical_Xmlns_When_The_Source_Assembly_Only_Declares_An_Alias()
    {
        const string source = """
            using AtomUI.Theme;
            using Avalonia.Controls;

            [assembly: Avalonia.Metadata.XmlnsDefinition("https://example.com/semantic", "AtomUI.Theme.Styling")]

            namespace Demo;

            [SemanticPart(
                "content",
                SelectorClass = "semantic-content",
                ContractType = typeof(Control),
                RuntimeCreated = true,
                SelectorRoute = "> .semantic-content",
                Since = "6.2.0")]
            public partial class AliasedXmlns : Control
            {
            }
            """;

        var output = RunGenerator(source, out var diagnostics);

        diagnostics.ShouldBeEmpty();
        HasGeneratedSource(output, "SemanticPartXmlnsDefinition.g.cs").ShouldBeTrue();
    }

    [Fact]
    public void Emits_The_Canonical_Xmlns_For_Each_Assembly_Even_When_A_Reference_Already_Declares_It()
    {
        var reference = CreateSemanticStyleReference(
            "Referenced.Semantic.Namespace",
            "https://atomui.net",
            "AtomUI.Theme.Styling",
            "UnrelatedStyle",
            Accessibility.Public);

        var output = RunGenerator(
            ButtonSource,
            [reference],
            out var diagnostics,
            ButtonTheme);

        diagnostics.ShouldBeEmpty();
        HasGeneratedSource(output, "SemanticPartXmlnsDefinition.g.cs").ShouldBeTrue();
    }

    [Fact]
    public void Runtime_Created_Part_Requires_An_Explicit_Owner_Relative_Selector_Route()
    {
        const string source = """
            using AtomUI.Theme;
            using Avalonia.Controls;

            namespace Demo;

            [SemanticPart(
                "item",
                SelectorClass = "semantic-item",
                ContractType = typeof(Control),
                RuntimeCreated = true,
                Since = "6.2.0")]
            public partial class RuntimeOwner : Control
            {
            }
            """;

        _ = RunGenerator(source, out var diagnostics);

        diagnostics.ShouldContain(diagnostic =>
            diagnostic.Id == "ATOMUIGEN027" &&
            diagnostic.GetMessage().Contains("SelectorRoute", StringComparison.Ordinal));
    }

    [Fact]
    public void Runtime_Created_Part_Emits_The_Declared_Selector_Route()
    {
        const string source = """
            using AtomUI.Theme;
            using Avalonia.Controls;

            namespace Demo;

            [SemanticPart(
                "item",
                SelectorClass = "semantic-item",
                SelectorRoute = "/template/ .semantic-scope-items > .semantic-scope-item /template/ .semantic-item",
                ContractType = typeof(Control),
                RuntimeCreated = true,
                Since = "6.2.0")]
            public partial class RuntimeOwner : Control
            {
            }
            """;

        var output = RunGenerator(source, out var diagnostics);

        diagnostics.ShouldBeEmpty();
        var manifest = GetGeneratedSource(output, "GeneratedSemanticPartManifest.g.cs");
        manifest.ShouldContain(
            "\"/template/ .semantic-scope-items > .semantic-scope-item /template/ .semantic-item\"");
        var constants = GetGeneratedSource(output, "RuntimeOwnerSemanticParts.g.cs");
        constants.ShouldContain(
            "internal const string ItemSelectorRoute = \"/template/ .semantic-scope-items > .semantic-scope-item /template/ .semantic-item\"");
    }

    [Theory]
    [InlineData(".semantic-item")]
    [InlineData("/template/ .semantic-scope-items .semantic-item")]
    [InlineData("/template/ #PART_Items /template/ .semantic-item")]
    [InlineData("/template/ Control /template/ .semantic-item")]
    [InlineData("/template/ :is(Control) /template/ .semantic-item")]
    [InlineData("/template/ [Tag=items] /template/ .semantic-item")]
    [InlineData("/template/ .items /template/ .semantic-item")]
    [InlineData("/template/ .semantic-scope-items > .semantic-other")]
    public void Reports_Invalid_Selector_Route_Tokens(string selectorRoute)
    {
        var source = $$"""
            using AtomUI.Theme;
            using Avalonia.Controls;

            namespace Demo;

            [SemanticPart(
                "item",
                SelectorClass = "semantic-item",
                SelectorRoute = "{{selectorRoute}}",
                ContractType = typeof(Control),
                RuntimeCreated = true,
                Since = "6.2.0")]
            public partial class RuntimeOwner : Control
            {
            }
            """;

        _ = RunGenerator(source, out var diagnostics);

        diagnostics.ShouldContain(diagnostic =>
            diagnostic.Id == "ATOMUIGEN027" &&
            diagnostic.GetMessage().Contains("SelectorRoute", StringComparison.Ordinal));
    }

    [Fact]
    public void Generated_Output_Is_Independent_Of_Attribute_Order()
    {
        var first = GetGeneratedSource(
            RunGenerator(ButtonSource, out _, ButtonTheme),
            "GeneratedSemanticPartManifest.g.cs");
        var reordered = GetGeneratedSource(
            RunGenerator(ReorderedButtonSource, out _, ButtonTheme),
            "GeneratedSemanticPartManifest.g.cs");

        reordered.ShouldBe(first);
    }

    [Fact]
    public void Supports_Static_Classes_Property_Semantic_Markers()
    {
        var theme = new InMemoryAdditionalText(
            "Button/Themes/ButtonTheme.axaml",
            """
            <ControlTheme xmlns="https://github.com/avaloniaui"
                          xmlns:atom="using:Demo"
                          TargetType="atom:Button">
                <Setter Property="Template">
                    <ControlTemplate>
                        <Panel>
                            <Control Classes.semantic-icon="True" />
                            <Control Classes.semantic-icon="true" />
                            <ContentPresenter Classes.semantic-content="True" />
                        </Panel>
                    </ControlTemplate>
                </Setter>
            </ControlTheme>
            """);

        _ = RunGenerator(ButtonSource, out var diagnostics, theme);

        diagnostics.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("False")]
    [InlineData("{Binding IsVisible}")]
    public void Reports_Non_Static_Classes_Property_Semantic_Markers(string markerValue)
    {
        const string source = """
            using AtomUI.Theme;
            using Avalonia.Controls;

            namespace Demo;

            [SemanticPart(
                "description",
                SelectorClass = "semantic-description",
                ContractType = typeof(Control),
                Cardinality = SemanticPartCardinality.Optional,
                Since = "6.2.0")]
            public partial class Button : Control
            {
            }
            """;
        var themeText = """
            <ControlTheme xmlns="https://github.com/avaloniaui"
                          xmlns:atom="using:Demo"
                          TargetType="atom:Button">
                <Setter Property="Template">
                    <ControlTemplate>
                        <Control Classes.semantic-description="$VALUE$" />
                    </ControlTemplate>
                </Setter>
            </ControlTheme>
            """.Replace("$VALUE$", markerValue, StringComparison.Ordinal);
        var theme = new InMemoryAdditionalText("Button/Themes/ButtonTheme.axaml", themeText);

        _ = RunGenerator(source, out var diagnostics, theme);

        diagnostics.ShouldContain(diagnostic => diagnostic.Id == "ATOMUIGEN036");
    }

    [Fact]
    public void Merges_Semantic_Parts_Declared_Across_Partial_Control_Declarations()
    {
        const string source = """
            using AtomUI.Theme;
            using Avalonia.Controls;
            using Avalonia.Controls.Presenters;

            namespace Demo;

            [SemanticPart(
                "icon",
                SelectorClass = "semantic-icon",
                ContractType = typeof(Control),
                Cardinality = SemanticPartCardinality.Multiple,
                Since = "6.2.0")]
            public partial class Button : Control
            {
            }

            [SemanticPart(
                "content",
                SelectorClass = "semantic-content",
                ContractType = typeof(ContentPresenter),
                Since = "6.2.0")]
            public partial class Button
            {
            }
            """;

        var output = RunGenerator(source, out var diagnostics, ButtonTheme);

        diagnostics.ShouldBeEmpty();
        output.GetDiagnostics(TestContext.Current.CancellationToken)
              .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
              .ShouldBeEmpty();
        output.SyntaxTrees.Count(tree => tree.FilePath.EndsWith(
            "ButtonSemanticParts.g.cs",
            StringComparison.Ordinal)).ShouldBe(1);
        var manifest = GetGeneratedSource(output, "GeneratedSemanticPartManifest.g.cs");
        manifest.ShouldContain("\"icon\"");
        manifest.ShouldContain("\"content\"");
    }

    [Fact]
    public void Reports_Invalid_And_Duplicate_Declarations()
    {
        const string source = """
            using AtomUI.Theme;
            using Avalonia.Controls;

            namespace Demo;

            [SemanticPart("Icon", SelectorClass = "icon", ContractType = typeof(Control), Since = "6.2.0")]
            [SemanticPart("content", SelectorClass = "semantic-content", ContractType = typeof(Control), Since = "6.2.0")]
            [SemanticPart("content", SelectorClass = "semantic-other", ContractType = typeof(Control), Since = "6.2.0")]
            public partial class Button : Control
            {
            }
            """;

        _ = RunGenerator(source, out var diagnostics, ButtonTheme);

        diagnostics.ShouldContain(diagnostic => diagnostic.Id == "ATOMUIGEN027");
        diagnostics.ShouldContain(diagnostic => diagnostic.Id == "ATOMUIGEN028");
    }

    [Fact]
    public void Reports_Reserved_Root_Path_And_Selector_Class()
    {
        const string source = """
            using AtomUI.Theme;
            using Avalonia.Controls;

            namespace Demo;

            [SemanticPart(
                "icon",
                Path = "root",
                SelectorClass = "semantic-root",
                ContractType = typeof(Control),
                Since = "6.2.0")]
            public partial class Button : Control
            {
            }
            """;

        _ = RunGenerator(source, out var diagnostics, ButtonTheme);

        diagnostics.Count(diagnostic => diagnostic.Id == "ATOMUIGEN027").ShouldBeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public void Reports_Explicitly_Empty_Path()
    {
        const string source = """
            using AtomUI.Theme;
            using Avalonia.Controls;

            namespace Demo;

            [SemanticPart(
                "icon",
                Path = "",
                SelectorClass = "semantic-icon",
                ContractType = typeof(Control),
                Since = "6.2.0")]
            public partial class Button : Control
            {
            }
            """;

        _ = RunGenerator(source, out var diagnostics, ButtonTheme);

        diagnostics.ShouldContain(diagnostic => diagnostic.Id == "ATOMUIGEN027");
    }

    [Fact]
    public void Generates_Strongly_Typed_Theme_Metadata_For_A_Public_Child_Control()
    {
        const string source = """
            using AtomUI.Theme;
            using Avalonia.Controls;
            using Avalonia.Styling;

            namespace Demo;

            [SemanticPart(
                "action",
                SelectorClass = "semantic-action",
                ContractType = typeof(Control),
                Customization = SemanticPartCustomization.SelectorAndTheme,
                ThemePropertyName = nameof(ActionTheme),
                Since = "6.2.0")]
            public partial class Button : Control
            {
                public ControlTheme? ActionTheme { get; set; }
            }
            """;
        var theme = new InMemoryAdditionalText(
            "Button/Themes/ButtonTheme.axaml",
            """
            <ControlTheme xmlns="https://github.com/avaloniaui"
                          xmlns:atom="using:Demo"
                          TargetType="atom:Button">
                <Setter Property="Template">
                    <ControlTemplate>
                        <Control Classes="semantic-action" />
                    </ControlTemplate>
                </Setter>
            </ControlTheme>
            """);

        var output = RunGenerator(source, out var diagnostics, theme);

        diagnostics.ShouldBeEmpty();
        var manifest = GetGeneratedSource(output, "GeneratedSemanticPartManifest.g.cs");
        manifest.ShouldContain("ControlThemeSemanticPartDescriptor(\"ActionTheme\", typeof(global::Avalonia.Controls.Control))");
    }

    [Fact]
    public void Uses_The_Actual_Compatible_Semantic_Theme_Asset_Target_Type()
    {
        const string source = """
            using AtomUI.Theme;
            using Avalonia.Controls;
            using Avalonia.Styling;

            namespace Demo;

            public class ActionControl : Control
            {
            }

            [SemanticPart(
                "action",
                SelectorClass = "semantic-action",
                ContractType = typeof(Control),
                Customization = SemanticPartCustomization.SelectorAndTheme,
                ThemePropertyName = nameof(ActionTheme),
                Since = "6.2.0")]
            public partial class Button : Control
            {
                public ControlTheme? ActionTheme { get; set; }
            }
            """;
        var ownerTheme = new InMemoryAdditionalText(
            "Button/Themes/ButtonTheme.axaml",
            """
            <ControlTheme xmlns="https://github.com/avaloniaui"
                          xmlns:atom="using:Demo"
                          TargetType="atom:Button">
                <Setter Property="ActionTheme"><ControlTheme TargetType="atom:ActionControl" /></Setter>
                <Setter Property="Template">
                    <ControlTemplate>
                        <atom:ActionControl Classes="semantic-action" />
                    </ControlTemplate>
                </Setter>
            </ControlTheme>
            """);
        var actionTheme = new InMemoryAdditionalText(
            "Button/Themes/ActionTheme.axaml",
            """
            <ControlTheme xmlns="https://github.com/avaloniaui"
                          xmlns:atom="using:Demo"
                          TargetType="atom:ActionControl" />
            """);

        var output = RunGenerator(source, out var diagnostics, ownerTheme, actionTheme);

        diagnostics.ShouldBeEmpty();
        var manifest = GetGeneratedSource(output, "GeneratedSemanticPartManifest.g.cs");
        manifest.ShouldContain(
            "ControlThemeSemanticPartDescriptor(\"ActionTheme\", typeof(global::Demo.ActionControl))");
    }

    [Fact]
    public void Reports_Incompatible_Semantic_Theme_Asset_Target_Type()
    {
        const string source = """
            using AtomUI.Theme;
            using Avalonia.Controls;
            using Avalonia.Styling;

            namespace Demo;

            public class UnrelatedElement : Avalonia.StyledElement
            {
            }

            [SemanticPart(
                "action",
                SelectorClass = "semantic-action",
                ContractType = typeof(Control),
                Customization = SemanticPartCustomization.SelectorAndTheme,
                ThemePropertyName = nameof(ActionTheme),
                Since = "6.2.0")]
            public partial class Button : Control
            {
                public ControlTheme? ActionTheme { get; set; }
            }
            """;
        var actionTheme = new InMemoryAdditionalText(
            "Button/Themes/ActionTheme.axaml",
            """
            <ControlTheme xmlns="https://github.com/avaloniaui"
                          xmlns:atom="using:Demo"
                          TargetType="atom:Button">
                <Setter Property="ActionTheme"><ControlTheme TargetType="atom:UnrelatedElement" /></Setter>
            </ControlTheme>
            """);

        _ = RunGenerator(source, out var diagnostics, ButtonTheme, actionTheme);

        diagnostics.ShouldContain(diagnostic => diagnostic.Id == "ATOMUIGEN030");
    }

    [Fact]
    public void Updated_Theme_Assets_Do_Not_Retain_Stale_Resolved_Target_Types()
    {
        const string source = """
            using AtomUI.Theme;
            using Avalonia.Controls;
            using Avalonia.Styling;

            namespace Demo;

            public class ActionControl : Control
            {
            }

            [SemanticPart(
                "action",
                SelectorClass = "semantic-action",
                ContractType = typeof(Control),
                Customization = SemanticPartCustomization.SelectorAndTheme,
                ThemePropertyName = nameof(ActionTheme),
                Since = "6.2.0")]
            public partial class Button : Control
            {
                public ControlTheme? ActionTheme { get; set; }
            }
            """;
        var ownerTheme = new InMemoryAdditionalText(
            "Button/Themes/ButtonTheme.axaml",
            """
            <ControlTheme xmlns="https://github.com/avaloniaui"
                          xmlns:atom="using:Demo"
                          TargetType="atom:Button">
                <Setter Property="ActionTheme"><ControlTheme TargetType="atom:ActionControl" /></Setter>
                <Setter Property="Template">
                    <ControlTemplate>
                        <atom:ActionControl Classes="semantic-action" />
                    </ControlTemplate>
                </Setter>
            </ControlTheme>
            """);
        var actionTheme = new InMemoryAdditionalText(
            "Button/Themes/ActionTheme.axaml",
            """
            <ControlTheme xmlns="https://github.com/avaloniaui"
                          xmlns:atom="using:Demo"
                          TargetType="atom:ActionControl" />
            """);
        var unrelatedTheme = new InMemoryAdditionalText(
            "Button/Themes/UnusedTheme.axaml",
            """
            <ResourceDictionary xmlns="https://github.com/avaloniaui" />
            """);
        var compilation = CreateCompilation(source);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [new TokenResourceKeyGenerator().AsSourceGenerator()],
            new AdditionalText[] { ownerTheme, actionTheme }.ToImmutableArray(),
            (CSharpParseOptions)compilation.SyntaxTrees[0].Options,
            new BuiltinCatalogOptionsProvider(null));

        driver = driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var firstOutput,
            out _,
            TestContext.Current.CancellationToken);
        GetGeneratedSource((CSharpCompilation)firstOutput, "GeneratedSemanticPartManifest.g.cs")
            .ShouldContain("ControlThemeSemanticPartDescriptor(\"ActionTheme\", typeof(global::Demo.ActionControl))");

        driver = driver.ReplaceAdditionalText(ownerTheme, new InMemoryAdditionalText(ownerTheme.Path, ownerTheme.GetText(TestContext.Current.CancellationToken)!.ToString().Replace(
                           "<Setter Property=\"ActionTheme\"><ControlTheme TargetType=\"atom:ActionControl\" /></Setter>", "")))
                       .RunGeneratorsAndUpdateCompilation(
                           compilation,
                           out var secondOutput,
                           out _,
                           TestContext.Current.CancellationToken);

        GetGeneratedSource((CSharpCompilation)secondOutput, "GeneratedSemanticPartManifest.g.cs")
            .ShouldContain(
                "ControlThemeSemanticPartDescriptor(\"ActionTheme\", typeof(global::Avalonia.Controls.Control))");
    }

    [Fact]
    public void Reports_Invalid_Contract_Type_And_Missing_Since()
    {
        const string source = """
            using AtomUI.Theme;
            using Avalonia.Controls;

            namespace Demo;

            [SemanticPart("icon", SelectorClass = "semantic-icon", ContractType = typeof(string))]
            public partial class Button : Control
            {
            }
            """;

        _ = RunGenerator(source, out var diagnostics, ButtonTheme);

        diagnostics.ShouldContain(diagnostic => diagnostic.Id == "ATOMUIGEN029");
        diagnostics.ShouldContain(diagnostic =>
            diagnostic.Id == "ATOMUIGEN031" && diagnostic.Severity == DiagnosticSeverity.Warning);
    }

    /// <summary>
    /// <c>Since</c> 必须是 <c>major.minor.patch</c> 三段式具体发布版本，例如 <c>6.2.0</c>。
    /// 只写版本线（<c>6.0</c>、<c>6.2</c>）或段数不符的值都不是可引用的发布版本，
    /// 会让 descriptor 与文档声称一个不存在的引入版本，因此按声明契约错误阻断构建。
    /// </summary>
    [Theory]
    [InlineData("6.0")]
    [InlineData("6.2")]
    [InlineData("6")]
    [InlineData("6.2.0.1")]
    [InlineData("v6.2.0")]
    [InlineData("6.2.0-beta.1")]
    [InlineData("latest")]
    public void Reports_Non_Semantic_Version_Since(string since)
    {
        var source = $$"""
            using AtomUI.Theme;
            using Avalonia.Controls;

            namespace Demo;

            [SemanticPart(
                "icon",
                SelectorClass = "semantic-icon",
                ContractType = typeof(Control),
                Since = "{{since}}")]
            public partial class Button : Control
            {
            }
            """;

        _ = RunGenerator(source, out var diagnostics, ButtonTheme);

        diagnostics.ShouldContain(diagnostic =>
            diagnostic.Id == "ATOMUIGEN038" && diagnostic.Severity == DiagnosticSeverity.Error);
    }

    [Theory]
    [InlineData("6.2.0")]
    [InlineData("6.0.0")]
    [InlineData("10.12.345")]
    public void Accepts_Three_Part_Since_Version(string since)
    {
        var source = $$"""
            using AtomUI.Theme;
            using Avalonia.Controls;

            namespace Demo;

            [SemanticPart(
                "icon",
                SelectorClass = "semantic-icon",
                ContractType = typeof(Control),
                Since = "{{since}}")]
            public partial class Button : Control
            {
            }
            """;

        _ = RunGenerator(source, out var diagnostics, ButtonTheme);

        diagnostics.ShouldNotContain(diagnostic => diagnostic.Id == "ATOMUIGEN038");
    }

    [Fact]
    public void Reports_Generic_Semantic_Control()
    {
        const string source = """
            using AtomUI.Theme;
            using Avalonia.Controls;

            namespace Demo;

            [SemanticPart(
                "item",
                SelectorClass = "semantic-item",
                ContractType = typeof(Control),
                RuntimeCreated = true,
                Since = "6.2.0")]
            public partial class GenericButton<T> : Control
            {
            }
            """;

        _ = RunGenerator(source, out var diagnostics);

        diagnostics.ShouldContain(diagnostic => diagnostic.Id == "ATOMUIGEN027");
    }

    [Fact]
    public void Reports_Invalid_Strongly_Typed_Theme_Property()
    {
        const string source = """
            using AtomUI.Theme;
            using Avalonia.Controls;

            namespace Demo;

            [SemanticPart(
                "action",
                SelectorClass = "semantic-action",
                ContractType = typeof(Control),
                Customization = SemanticPartCustomization.SelectorAndTheme,
                ThemePropertyName = "ActionTheme",
                Since = "6.2.0")]
            public partial class Button : Control
            {
                public object? ActionTheme { get; set; }
            }
            """;

        _ = RunGenerator(source, out var diagnostics, ButtonTheme);

        var diagnostic = diagnostics.Single(diagnostic => diagnostic.Id == "ATOMUIGEN030");
        diagnostic.GetMessage().ShouldContain("public getter and setter");
    }

    [Fact]
    public void Reports_Strongly_Typed_Theme_Property_Without_A_Public_Setter()
    {
        const string source = """
            using AtomUI.Theme;
            using Avalonia.Controls;
            using Avalonia.Styling;

            namespace Demo;

            [SemanticPart(
                "action",
                SelectorClass = "semantic-action",
                ContractType = typeof(Control),
                Customization = SemanticPartCustomization.SelectorAndTheme,
                ThemePropertyName = nameof(ActionTheme),
                Since = "6.2.0")]
            public partial class Button : Control
            {
                public ControlTheme? ActionTheme { get; private set; }
            }
            """;

        _ = RunGenerator(source, out var diagnostics, ButtonTheme);

        diagnostics.ShouldContain(diagnostic => diagnostic.Id == "ATOMUIGEN030");
    }

    [Fact]
    public void Reports_Strongly_Typed_Theme_Property_Without_A_Public_Getter()
    {
        const string source = """
            using AtomUI.Theme;
            using Avalonia.Controls;
            using Avalonia.Styling;

            namespace Demo;

            [SemanticPart(
                "action",
                SelectorClass = "semantic-action",
                ContractType = typeof(Control),
                Customization = SemanticPartCustomization.SelectorAndTheme,
                ThemePropertyName = nameof(ActionTheme),
                Since = "6.2.0")]
            public partial class Button : Control
            {
                public ControlTheme? ActionTheme { private get; set; }
            }
            """;

        _ = RunGenerator(source, out var diagnostics, ButtonTheme);

        diagnostics.ShouldContain(diagnostic => diagnostic.Id == "ATOMUIGEN030");
    }

    [Fact]
    public void Reports_Marker_Cardinality_And_Contract_Type_Mismatches()
    {
        var missingMarkerTheme = new InMemoryAdditionalText(
            "Button/Themes/ButtonTheme.axaml",
            """
            <ControlTheme xmlns="https://github.com/avaloniaui"
                          xmlns:atom="using:Demo"
                          TargetType="atom:Button">
                <Setter Property="Template">
                    <ControlTemplate>
                        <Panel>
                            <Control Classes="semantic-icon" />
                        </Panel>
                    </ControlTemplate>
                </Setter>
            </ControlTheme>
            """);
        var incompatibleTheme = new InMemoryAdditionalText(
            "Button/Themes/Browser/ButtonTheme.axaml",
            """
            <ControlTheme xmlns="https://github.com/avaloniaui"
                          xmlns:atom="using:Demo"
                          TargetType="atom:Button">
                <Setter Property="Template">
                    <ControlTemplate>
                        <Control Classes="semantic-content" />
                    </ControlTemplate>
                </Setter>
            </ControlTheme>
            """);

        _ = RunGenerator(ButtonSource, out var diagnostics, missingMarkerTheme, incompatibleTheme);

        diagnostics.ShouldContain(diagnostic => diagnostic.Id == "ATOMUIGEN032");
        diagnostics.ShouldContain(diagnostic => diagnostic.Id == "ATOMUIGEN033");
    }

    [Fact]
    public void Validates_A_Derived_Control_Against_Its_Typed_BasedOn_Template()
    {
        const string source = """
            using AtomUI.Theme;
            using Avalonia.Controls;
            using Avalonia.Controls.Presenters;

            namespace Demo;

            public class BaseButton : Control
            {
            }

            [SemanticPart(
                "content",
                SelectorClass = "semantic-content",
                ContractType = typeof(ContentPresenter),
                Since = "6.2.0")]
            public partial class DerivedButton : BaseButton
            {
            }
            """;
        var baseTheme = new InMemoryAdditionalText(
            "BaseButton/Themes/BaseButtonTheme.axaml",
            """
            <ControlTheme xmlns="https://github.com/avaloniaui"
                          xmlns:atom="using:Demo"
                          TargetType="atom:BaseButton">
                <Setter Property="Template">
                    <ControlTemplate>
                        <ContentPresenter Classes="semantic-content" />
                    </ControlTemplate>
                </Setter>
            </ControlTheme>
            """);
        var derivedTheme = new InMemoryAdditionalText(
            "DerivedButton/Themes/DerivedButtonTheme.axaml",
            """
            <ControlTheme xmlns="https://github.com/avaloniaui"
                          xmlns:atom="using:Demo"
                          TargetType="atom:DerivedButton"
                          BasedOn="{StaticResource {x:Type atom:BaseButton}}" />
            """);

        var output = RunGenerator(source, out var diagnostics, derivedTheme, baseTheme);

        diagnostics.ShouldBeEmpty();
        GetGeneratedSource(output, "GeneratedSemanticPartManifest.g.cs")
            .ShouldContain("typeof(global::Demo.DerivedButton)");
    }

    [Fact]
    public void Does_Not_Validate_The_Base_Template_When_A_Derived_Theme_Adds_Only_A_Conditional_Template()
    {
        const string source = """
            using AtomUI.Theme;
            using Avalonia.Controls;
            using Avalonia.Controls.Presenters;

            namespace Demo;

            public class BaseButton : Control
            {
            }

            [SemanticPart(
                "content",
                SelectorClass = "semantic-content",
                ContractType = typeof(ContentPresenter),
                Since = "6.2.0")]
            public partial class DerivedButton : BaseButton
            {
            }
            """;
        var baseTheme = new InMemoryAdditionalText(
            "BaseButton/Themes/BaseButtonTheme.axaml",
            """
            <ControlTheme xmlns="https://github.com/avaloniaui"
                          xmlns:atom="using:Demo"
                          TargetType="atom:BaseButton">
                <Setter Property="Template">
                    <ControlTemplate>
                        <Control />
                    </ControlTemplate>
                </Setter>
            </ControlTheme>
            """);
        var derivedTheme = new InMemoryAdditionalText(
            "DerivedButton/Themes/DerivedButtonTheme.axaml",
            """
            <ControlTheme xmlns="https://github.com/avaloniaui"
                          xmlns:atom="using:Demo"
                          TargetType="atom:DerivedButton"
                          BasedOn="{StaticResource {x:Type atom:BaseButton}}">
                <Style Selector="^:pointerover">
                    <Setter Property="Template">
                        <ControlTemplate>
                            <ContentPresenter Classes="semantic-content" />
                        </ControlTemplate>
                    </Setter>
                </Style>
            </ControlTheme>
            """);

        _ = RunGenerator(source, out var diagnostics, derivedTheme, baseTheme);

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public void Validates_The_Derived_Conditional_Template_But_Not_The_Base_Template()
    {
        const string source = """
            using AtomUI.Theme;
            using Avalonia.Controls;
            using Avalonia.Controls.Presenters;

            namespace Demo;

            public class BaseButton : Control
            {
            }

            [SemanticPart(
                "content",
                SelectorClass = "semantic-content",
                ContractType = typeof(ContentPresenter),
                Since = "6.2.0")]
            public partial class DerivedButton : BaseButton
            {
            }
            """;
        var baseTheme = new InMemoryAdditionalText(
            "BaseButton/Themes/BaseButtonTheme.axaml",
            """
            <ControlTheme xmlns="https://github.com/avaloniaui"
                          xmlns:atom="using:Demo"
                          TargetType="atom:BaseButton">
                <Setter Property="Template">
                    <ControlTemplate>
                        <ContentPresenter Classes="semantic-content" />
                    </ControlTemplate>
                </Setter>
            </ControlTheme>
            """);
        var derivedTheme = new InMemoryAdditionalText(
            "DerivedButton/Themes/DerivedButtonTheme.axaml",
            """
            <ControlTheme xmlns="https://github.com/avaloniaui"
                          xmlns:atom="using:Demo"
                          TargetType="atom:DerivedButton"
                          BasedOn="{StaticResource {x:Type atom:BaseButton}}">
                <Style Selector="^:pointerover">
                    <Setter Property="Template">
                        <ControlTemplate>
                            <Control />
                        </ControlTemplate>
                    </Setter>
                </Style>
            </ControlTheme>
            """);

        _ = RunGenerator(source, out var diagnostics, derivedTheme, baseTheme);

        diagnostics.ShouldContain(diagnostic =>
            diagnostic.Id == "ATOMUIGEN032" &&
            diagnostic.GetMessage().Contains("DerivedButtonTheme.axaml", StringComparison.Ordinal));
        diagnostics.ShouldNotContain(diagnostic =>
            diagnostic.GetMessage().Contains("BaseButtonTheme.axaml", StringComparison.Ordinal));
    }

    [Fact]
    public void Validates_The_Base_Template_For_A_Pure_Inheritance_Theme()
    {
        const string source = """
            using AtomUI.Theme;
            using Avalonia.Controls;
            using Avalonia.Controls.Presenters;

            namespace Demo;

            public class BaseButton : Control
            {
            }

            [SemanticPart(
                "content",
                SelectorClass = "semantic-content",
                ContractType = typeof(ContentPresenter),
                Since = "6.2.0")]
            public partial class DerivedButton : BaseButton
            {
            }
            """;
        var baseTheme = new InMemoryAdditionalText(
            "BaseButton/Themes/BaseButtonTheme.axaml",
            """
            <ControlTheme xmlns="https://github.com/avaloniaui"
                          xmlns:atom="using:Demo"
                          TargetType="atom:BaseButton">
                <Setter Property="Template">
                    <ControlTemplate>
                        <Control />
                    </ControlTemplate>
                </Setter>
            </ControlTheme>
            """);
        var derivedTheme = new InMemoryAdditionalText(
            "DerivedButton/Themes/DerivedButtonTheme.axaml",
            """
            <ControlTheme xmlns="https://github.com/avaloniaui"
                          xmlns:atom="using:Demo"
                          TargetType="atom:DerivedButton"
                          BasedOn="{StaticResource {x:Type atom:BaseButton}}" />
            """);

        _ = RunGenerator(source, out var diagnostics, derivedTheme, baseTheme);

        diagnostics.ShouldContain(diagnostic =>
            diagnostic.Id == "ATOMUIGEN032" &&
            diagnostic.GetMessage().Contains("BaseButtonTheme.axaml#1", StringComparison.Ordinal));
    }

    [Fact]
    public void Validates_A_Pure_Inheritance_Theme_Against_An_Element_Syntax_BasedOn()
    {
        const string source = """
            using AtomUI.Theme;
            using Avalonia.Controls;
            using Avalonia.Controls.Presenters;
            using Avalonia.Styling;

            namespace Demo;

            public class BaseButton : Control
            {
            }

            public class BaseButtonTheme : ControlTheme
            {
            }

            [SemanticPart(
                "content",
                SelectorClass = "semantic-content",
                ContractType = typeof(ContentPresenter),
                Since = "6.2.0")]
            public partial class DerivedButton : BaseButton
            {
            }
            """;
        var baseTheme = new InMemoryAdditionalText(
            "BaseButton/Themes/BaseButtonTheme.axaml",
            """
            <ControlTheme xmlns="https://github.com/avaloniaui"
                          xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                          xmlns:atom="using:Demo"
                          x:Class="Demo.BaseButtonTheme"
                          TargetType="atom:BaseButton">
                <Setter Property="Template">
                    <ControlTemplate>
                        <ContentPresenter Classes="semantic-content" />
                    </ControlTemplate>
                </Setter>
            </ControlTheme>
            """);
        var derivedTheme = new InMemoryAdditionalText(
            "DerivedButton/Themes/DerivedButtonTheme.axaml",
            """
            <ControlTheme xmlns="https://github.com/avaloniaui"
                          xmlns:atom="using:Demo"
                          TargetType="atom:DerivedButton">
                <ControlTheme.BasedOn>
                    <atom:BaseButtonTheme TargetType="atom:DerivedButton" />
                </ControlTheme.BasedOn>
            </ControlTheme>
            """);

        var output = RunGenerator(source, out var diagnostics, derivedTheme, baseTheme);

        diagnostics.ShouldBeEmpty();
        GetGeneratedSource(output, "GeneratedSemanticPartManifest.g.cs")
            .ShouldContain("typeof(global::Demo.DerivedButton)");
    }

    [Fact]
    public void Does_Not_Validate_A_Base_Template_Replaced_By_A_Direct_Derived_Theme_Setter()
    {
        const string source = """
            using AtomUI.Theme;
            using Avalonia.Controls;
            using Avalonia.Controls.Presenters;

            namespace Demo;

            public class BaseButton : Control
            {
            }

            [SemanticPart(
                "content",
                SelectorClass = "semantic-content",
                ContractType = typeof(ContentPresenter),
                Since = "6.2.0")]
            public partial class DerivedButton : BaseButton
            {
            }
            """;
        var baseTheme = new InMemoryAdditionalText(
            "BaseButton/Themes/BaseButtonTheme.axaml",
            """
            <ControlTheme xmlns="https://github.com/avaloniaui"
                          xmlns:atom="using:Demo"
                          TargetType="atom:BaseButton">
                <Setter Property="Template">
                    <ControlTemplate>
                        <Control />
                    </ControlTemplate>
                </Setter>
            </ControlTheme>
            """);
        var derivedTheme = new InMemoryAdditionalText(
            "DerivedButton/Themes/DerivedButtonTheme.axaml",
            """
            <ControlTheme xmlns="https://github.com/avaloniaui"
                          xmlns:atom="using:Demo"
                          TargetType="atom:DerivedButton"
                          BasedOn="{StaticResource {x:Type atom:BaseButton}}">
                <Setter Property="Template">
                    <ControlTemplate>
                        <ContentPresenter Classes="semantic-content" />
                    </ControlTemplate>
                </Setter>
            </ControlTheme>
            """);

        _ = RunGenerator(source, out var diagnostics, derivedTheme, baseTheme);

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public void Typed_BasedOn_Cycles_Report_A_Missing_Template_Without_Recursing_Forever()
    {
        const string source = """
            using AtomUI.Theme;
            using Avalonia.Controls;
            using Avalonia.Controls.Presenters;

            namespace Demo;

            public class BaseButton : Control
            {
            }

            [SemanticPart(
                "content",
                SelectorClass = "semantic-content",
                ContractType = typeof(ContentPresenter),
                Since = "6.2.0")]
            public partial class DerivedButton : BaseButton
            {
            }
            """;
        var baseTheme = new InMemoryAdditionalText(
            "BaseButton/Themes/BaseButtonTheme.axaml",
            """
            <ControlTheme xmlns="https://github.com/avaloniaui"
                          xmlns:atom="using:Demo"
                          TargetType="atom:BaseButton"
                          BasedOn="{StaticResource {x:Type atom:DerivedButton}}" />
            """);
        var derivedTheme = new InMemoryAdditionalText(
            "DerivedButton/Themes/DerivedButtonTheme.axaml",
            """
            <ControlTheme xmlns="https://github.com/avaloniaui"
                          xmlns:atom="using:Demo"
                          TargetType="atom:DerivedButton"
                          BasedOn="{StaticResource {x:Type atom:BaseButton}}" />
            """);

        _ = RunGenerator(source, out var diagnostics, derivedTheme, baseTheme);

        diagnostics.ShouldContain(diagnostic => diagnostic.Id == "ATOMUIGEN034");
    }

    [Fact]
    public void Template_Diagnostic_Names_Are_Unique_Within_A_Multi_Theme_Asset()
    {
        const string source = """
            using AtomUI.Theme;
            using Avalonia.Controls;
            using Avalonia.Controls.Presenters;

            namespace Demo;

            [SemanticPart(
                "content",
                SelectorClass = "semantic-content",
                ContractType = typeof(ContentPresenter),
                Since = "6.2.0")]
            public partial class Button : Control
            {
            }
            """;
        var theme = new InMemoryAdditionalText(
            "Button/Themes/ButtonVariants.axaml",
            """
            <ResourceDictionary xmlns="https://github.com/avaloniaui"
                                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                                xmlns:atom="using:Demo">
                <ControlTheme x:Key="First" TargetType="atom:Button">
                    <Setter Property="Template">
                        <ControlTemplate>
                            <Control />
                        </ControlTemplate>
                    </Setter>
                </ControlTheme>
                <ControlTheme x:Key="Second" TargetType="atom:Button">
                    <Setter Property="Template">
                        <ControlTemplate>
                            <Control />
                        </ControlTemplate>
                    </Setter>
                </ControlTheme>
            </ResourceDictionary>
            """);

        _ = RunGenerator(source, out var diagnostics, theme);

        var templateDiagnostics = diagnostics.Where(diagnostic => diagnostic.Id == "ATOMUIGEN032").ToArray();
        templateDiagnostics.Length.ShouldBe(2);
        templateDiagnostics.ShouldContain(diagnostic => diagnostic.GetMessage().Contains(
            "Button/Themes/ButtonVariants.axaml#1",
            StringComparison.Ordinal));
        templateDiagnostics.ShouldContain(diagnostic => diagnostic.GetMessage().Contains(
            "Button/Themes/ButtonVariants.axaml#2",
            StringComparison.Ordinal));
    }

    [Fact]
    public void Nested_Xml_Namespace_Declarations_Override_Outer_Prefixes()
    {
        const string source = """
            using AtomUI.Theme;
            using Avalonia.Controls;
            using Avalonia.Controls.Presenters;

            namespace Demo;

            [SemanticPart(
                "content",
                SelectorClass = "semantic-content",
                ContractType = typeof(ContentPresenter),
                Since = "6.2.0")]
            public partial class Button : Control
            {
            }
            """;
        var theme = new InMemoryAdditionalText(
            "Button/Themes/ButtonVariants.axaml",
            """
            <ResourceDictionary xmlns="https://github.com/avaloniaui"
                                xmlns:atom="using:Wrong.Namespace">
                <ControlTheme xmlns:atom="using:Demo" TargetType="atom:Button">
                    <Setter Property="Template">
                        <ControlTemplate>
                            <ContentPresenter Classes="semantic-content" />
                        </ControlTemplate>
                    </Setter>
                </ControlTheme>
            </ResourceDictionary>
            """);

        _ = RunGenerator(source, out var diagnostics, theme);

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public void Type_Resolver_Resolves_Clr_Namespace_Types_From_An_Explicit_Referenced_Assembly()
    {
        var references = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))!
                         .Split(Path.PathSeparator)
                         .Select(static path => MetadataReference.CreateFromFile(path))
                         .Cast<MetadataReference>()
                         .ToImmutableArray();
        var externalReference = CSharpCompilation.Create(
                "Semantic.Marker.Contracts",
                [CSharpSyntaxTree.ParseText(
                    "namespace External; public sealed class SemanticContent { }",
                    cancellationToken: TestContext.Current.CancellationToken)],
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .ToMetadataReference();
        var compilation = CSharpCompilation.Create(
            "SemanticPartTypeResolverTests",
            references: references.Add(externalReference),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var element = XElement.Parse(
            """
            <ControlTheme xmlns:external="clr-namespace:External;assembly=Semantic.Marker.Contracts"
                          TargetType="external:SemanticContent" />
            """);
        var reference = ThemeAssetTargetTypeReference.Create(
            element,
            element.Attribute("TargetType")!.Value);
        var resolver = new SemanticPartTypeResolver(compilation, Array.Empty<INamedTypeSymbol>());

        var resolved = resolver.ResolveTargetType(reference);

        resolved.ShouldNotBeNull();
        resolved.ToDisplayString().ShouldBe("External.SemanticContent");
        resolved.ContainingAssembly.Name.ShouldBe("Semantic.Marker.Contracts");
    }

    [Fact]
    public void Diagnostic_Order_Is_Independent_Of_Theme_Asset_Input_Order()
    {
        var firstTheme = CreateThemeWithoutContentMarker("A/Themes/ButtonTheme.axaml");
        var secondTheme = CreateThemeWithoutContentMarker("B/Themes/ButtonTheme.axaml");

        _ = RunGenerator(ButtonSource, out var forward, firstTheme, secondTheme);
        _ = RunGenerator(ButtonSource, out var reversed, secondTheme, firstTheme);

        forward.Select(static diagnostic => diagnostic.ToString())
               .ShouldBe(reversed.Select(static diagnostic => diagnostic.ToString()));
    }

    [Fact]
    public void Reports_One_Template_Node_Assigned_To_Multiple_Semantic_Parts()
    {
        const string source = """
            using AtomUI.Theme;
            using Avalonia.Controls;

            namespace Demo;

            [SemanticPart(
                "leading",
                SelectorClass = "semantic-leading",
                ContractType = typeof(Control),
                Since = "6.2.0")]
            [SemanticPart(
                "trailing",
                SelectorClass = "semantic-trailing",
                ContractType = typeof(Control),
                Since = "6.2.0")]
            public partial class Button : Control
            {
            }
            """;
        var theme = new InMemoryAdditionalText(
            "Button/Themes/ButtonTheme.axaml",
            """
            <ControlTheme xmlns="https://github.com/avaloniaui"
                          xmlns:atom="using:Demo"
                          TargetType="atom:Button">
                <Setter Property="Template">
                    <ControlTemplate>
                        <Control Classes="semantic-leading semantic-trailing" />
                    </ControlTemplate>
                </Setter>
            </ControlTheme>
            """);

        _ = RunGenerator(source, out var diagnostics, theme);

        diagnostics.ShouldContain(diagnostic => diagnostic.Id == "ATOMUIGEN035");
    }

    [Fact]
    public void Optional_Multiple_RuntimeCreated_And_CrossRoot_Metadata_Are_Validated_Without_Theme_Properties()
    {
        const string source = """
            using AtomUI.Theme;
            using Avalonia.Controls;

            namespace Demo;

            [SemanticPart(
                "icon",
                SelectorClass = "semantic-icon",
                ContractType = typeof(Control),
                Cardinality = SemanticPartCardinality.Multiple,
                Since = "6.2.0")]
            [SemanticPart(
                "description",
                SelectorClass = "semantic-description",
                ContractType = typeof(Control),
                Cardinality = SemanticPartCardinality.Optional,
                Since = "6.2.0")]
            [SemanticPart(
                "popup",
                SelectorClass = "semantic-popup",
                SelectorRoute = "> .semantic-popup",
                ContractType = typeof(Control),
                CrossVisualRoot = true,
                RuntimeCreated = true,
                Since = "6.2.0")]
            public partial class Button : Control
            {
            }
            """;
        var theme = new InMemoryAdditionalText(
            "Button/Themes/ButtonTheme.axaml",
            """
            <ControlTheme xmlns="https://github.com/avaloniaui"
                          xmlns:atom="using:Demo"
                          TargetType="atom:Button">
                <Setter Property="Template">
                    <ControlTemplate>
                        <Panel>
                            <Control Classes="semantic-icon" />
                            <Control Classes="semantic-icon" />
                        </Panel>
                    </ControlTemplate>
                </Setter>
            </ControlTheme>
            """);

        var output = RunGenerator(source, out var diagnostics, theme);

        diagnostics.ShouldBeEmpty();
        var manifest = GetGeneratedSource(output, "GeneratedSemanticPartManifest.g.cs");
        manifest.ShouldContain("\"popup\"");
        manifest.ShouldContain("                        true,");
        manifest.ShouldNotContain("PopupTheme");
    }

    [Fact]
    public void Resolves_Cross_Nested_Parts_Through_A_Runtime_Created_Sibling_Part()
    {
        const string source = """
            using AtomUI.Theme;
            using Avalonia.Controls;
            using Avalonia.Controls.Presenters;

            namespace Demo;

            public class TagBox : Control
            {
            }

            public class InputBox : Control
            {
            }

            [SemanticPart(
                "item",
                SelectorClass = "semantic-item",
                SelectorRoute = ">> .semantic-scope-host >> .semantic-item",
                CrossNestedOwners = true,
                ContractType = typeof(TagBox),
                Cardinality = SemanticPartCardinality.Multiple,
                RuntimeCreated = true,
                Since = "6.2.0")]
            [SemanticPart(
                "itemContent",
                SelectorClass = "semantic-item-content",
                SelectorRoute = ">> .semantic-scope-host >> .semantic-item /template/ .semantic-item-content",
                CrossNestedOwners = true,
                ContractType = typeof(ContentPresenter),
                Since = "6.2.0")]
            public partial class TagHost : Control
            {
            }
            """;
        var hostTheme = new InMemoryAdditionalText(
            "TagHost/Themes/TagHostTheme.axaml",
            """
            <ControlTheme xmlns="https://github.com/avaloniaui"
                          xmlns:atom="using:Demo"
                          TargetType="atom:TagHost">
                <Setter Property="Template">
                    <ControlTemplate>
                        <Panel>
                            <atom:InputBox Classes="semantic-scope-host" />
                        </Panel>
                    </ControlTemplate>
                </Setter>
            </ControlTheme>
            """);
        var inputTheme = new InMemoryAdditionalText(
            "InputBox/Themes/InputBoxTheme.axaml",
            """
            <ControlTheme xmlns="https://github.com/avaloniaui"
                          xmlns:atom="using:Demo"
                          TargetType="atom:InputBox">
                <Setter Property="Template">
                    <ControlTemplate>
                        <Border />
                    </ControlTemplate>
                </Setter>
            </ControlTheme>
            """);
        var tagTheme = new InMemoryAdditionalText(
            "TagBox/Themes/TagBoxTheme.axaml",
            """
            <ControlTheme xmlns="https://github.com/avaloniaui"
                          xmlns:atom="using:Demo"
                          TargetType="atom:TagBox">
                <Setter Property="Template">
                    <ControlTemplate>
                        <ContentPresenter Classes="semantic-item-content" />
                    </ControlTemplate>
                </Setter>
            </ControlTheme>
            """);

        var output = RunGenerator(source, out var diagnostics, hostTheme, inputTheme, tagTheme);

        diagnostics.ShouldBeEmpty();
        var manifest = GetGeneratedSource(output, "GeneratedSemanticPartManifest.g.cs");
        manifest.ShouldContain("\"semantic-item-content\"");
    }

    [Fact]
    public void Reports_Cross_Nested_Parts_Whose_Runtime_Sibling_Hop_Is_Missing()
    {
        const string source = """
            using AtomUI.Theme;
            using Avalonia.Controls;
            using Avalonia.Controls.Presenters;

            namespace Demo;

            public class TagBox : Control
            {
            }

            public class InputBox : Control
            {
            }

            [SemanticPart(
                "itemContent",
                SelectorClass = "semantic-item-content",
                SelectorRoute = ">> .semantic-scope-host >> .semantic-item /template/ .semantic-item-content",
                CrossNestedOwners = true,
                ContractType = typeof(ContentPresenter),
                Since = "6.2.0")]
            public partial class TagHost : Control
            {
            }
            """;
        var hostTheme = new InMemoryAdditionalText(
            "TagHost/Themes/TagHostTheme.axaml",
            """
            <ControlTheme xmlns="https://github.com/avaloniaui"
                          xmlns:atom="using:Demo"
                          TargetType="atom:TagHost">
                <Setter Property="Template">
                    <ControlTemplate>
                        <Panel>
                            <atom:InputBox Classes="semantic-scope-host" />
                        </Panel>
                    </ControlTemplate>
                </Setter>
            </ControlTheme>
            """);
        var inputTheme = new InMemoryAdditionalText(
            "InputBox/Themes/InputBoxTheme.axaml",
            """
            <ControlTheme xmlns="https://github.com/avaloniaui"
                          xmlns:atom="using:Demo"
                          TargetType="atom:InputBox">
                <Setter Property="Template">
                    <ControlTemplate>
                        <Border />
                    </ControlTemplate>
                </Setter>
            </ControlTheme>
            """);
        var tagTheme = new InMemoryAdditionalText(
            "TagBox/Themes/TagBoxTheme.axaml",
            """
            <ControlTheme xmlns="https://github.com/avaloniaui"
                          xmlns:atom="using:Demo"
                          TargetType="atom:TagBox">
                <Setter Property="Template">
                    <ControlTemplate>
                        <ContentPresenter Classes="semantic-item-content" />
                    </ControlTemplate>
                </Setter>
            </ControlTheme>
            """);

        _ = RunGenerator(source, out var diagnostics, hostTheme, inputTheme, tagTheme);

        diagnostics.ShouldContain(diagnostic => diagnostic.Id == "ATOMUIGEN032");
    }

    private static CSharpCompilation RunGenerator(
        string source,
        out ImmutableArray<Diagnostic> diagnostics,
        params AdditionalText[] additionalTexts)
    {
        return RunGenerator(source, [], out diagnostics, additionalTexts);
    }

    private static CSharpCompilation RunGenerator(
        string source,
        ImmutableArray<MetadataReference> additionalReferences,
        out ImmutableArray<Diagnostic> diagnostics,
        params AdditionalText[] additionalTexts)
    {
        var compilation = CreateCompilation(source, additionalReferences);
        var driver = CSharpGeneratorDriver.Create(
            [new TokenResourceKeyGenerator().AsSourceGenerator()],
            additionalTexts.ToImmutableArray(),
            (CSharpParseOptions)compilation.SyntaxTrees[0].Options,
            new BuiltinCatalogOptionsProvider(null));

        driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out diagnostics,
            TestContext.Current.CancellationToken);
        return (CSharpCompilation)outputCompilation;
    }

    private static MetadataReference CreateSemanticStyleReference(
        string assemblyName,
        string xmlNamespace,
        string clrNamespace,
        string typeName,
        Accessibility accessibility)
    {
        var accessibilityKeyword = accessibility == Accessibility.Public ? "public" : "internal";
        var source = $$"""
            [assembly: Avalonia.Metadata.XmlnsDefinition("{{xmlNamespace}}", "{{clrNamespace}}")]

            namespace Avalonia.Metadata
            {
                [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple = true)]
                public sealed class XmlnsDefinitionAttribute : System.Attribute
                {
                    public XmlnsDefinitionAttribute(string xmlNamespace, string clrNamespace)
                    {
                    }
                }
            }

            namespace {{clrNamespace}}
            {
                {{accessibilityKeyword}} sealed class {{typeName}}
                {
                }
            }
            """;

        return CSharpCompilation.Create(
                assemblyName,
                [CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.Current.CancellationToken)],
                GetPlatformReferences(),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .ToMetadataReference();
    }

    private static AdditionalText CreateThemeWithoutContentMarker(string path)
    {
        return new InMemoryAdditionalText(
            path,
            """
            <ControlTheme xmlns="https://github.com/avaloniaui"
                          xmlns:atom="using:Demo"
                          TargetType="atom:Button">
                <Setter Property="Template">
                    <ControlTemplate>
                        <Control Classes="semantic-icon" />
                    </ControlTemplate>
                </Setter>
            </ControlTheme>
            """);
    }

    private static CSharpCompilation CreateCompilation(
        string source,
        ImmutableArray<MetadataReference> additionalReferences = default)
    {
        var references = GetPlatformReferences();
        if (!additionalReferences.IsDefaultOrEmpty)
        {
            references = references.AddRange(additionalReferences);
        }

        return CSharpCompilation.Create(
            "SemanticPartGeneratorTests",
            [CSharpSyntaxTree.ParseText(source), CSharpSyntaxTree.ParseText(AtomUIStubs)],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    private static ImmutableArray<MetadataReference> GetPlatformReferences()
    {
        return ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))!
               .Split(Path.PathSeparator)
               .Select(static path => MetadataReference.CreateFromFile(path))
               .Cast<MetadataReference>()
               .ToImmutableArray();
    }

    private static bool HasGeneratedSource(CSharpCompilation compilation, string fileName)
    {
        return compilation.SyntaxTrees.Any(tree => tree.FilePath.EndsWith(fileName, StringComparison.Ordinal));
    }

    private static string GetGeneratedSource(CSharpCompilation compilation, string fileName)
    {
        return compilation.SyntaxTrees
                          .Single(tree => tree.FilePath.EndsWith(fileName, StringComparison.Ordinal))
                          .GetText(TestContext.Current.CancellationToken)
                          .ToString();
    }

    private const string ButtonSource = """
        using AtomUI.Theme;
        using Avalonia.Controls;
        using Avalonia.Controls.Presenters;

        namespace Demo;

        [SemanticPart(
            "icon",
            SelectorClass = "semantic-icon",
            ContractType = typeof(Control),
            Cardinality = SemanticPartCardinality.Multiple,
            Since = "6.2.0")]
        [SemanticPart(
            "content",
            SelectorClass = "semantic-content",
            ContractType = typeof(ContentPresenter),
            Since = "6.2.0")]
        public partial class Button : Control
        {
        }
        """;

    private const string ReorderedButtonSource = """
        using AtomUI.Theme;
        using Avalonia.Controls;
        using Avalonia.Controls.Presenters;

        namespace Demo;

        [SemanticPart(
            "content",
            SelectorClass = "semantic-content",
            ContractType = typeof(ContentPresenter),
            Since = "6.2.0")]
        [SemanticPart(
            "icon",
            SelectorClass = "semantic-icon",
            ContractType = typeof(Control),
            Cardinality = SemanticPartCardinality.Multiple,
            Since = "6.2.0")]
        public partial class Button : Control
        {
        }
        """;

    private static readonly AdditionalText ButtonTheme = new InMemoryAdditionalText(
        "Button/Themes/ButtonTheme.axaml",
        """
        <ControlTheme xmlns="https://github.com/avaloniaui"
                      xmlns:atom="using:Demo"
                      TargetType="atom:Button">
            <Setter Property="Template">
                <ControlTemplate>
                    <Panel>
                        <Control Classes="semantic-icon" />
                        <Control Classes="semantic-icon" />
                        <ContentPresenter Classes="semantic-content" />
                    </Panel>
                </ControlTemplate>
            </Setter>
        </ControlTheme>
        """);

    private const string AtomUIStubs = """
        namespace Avalonia
        {
            public class StyledElement
            {
            }
        }

        namespace Avalonia.Controls
        {
            public class Control : Avalonia.StyledElement
            {
            }
        }

        namespace Avalonia.Controls.Presenters
        {
            public class ContentPresenter : Avalonia.Controls.Control
            {
            }
        }

        namespace Avalonia.Styling
        {
            public class Selector
            {
            }

            public class Style
            {
                public Style(System.Func<Selector?, Selector> selector)
                {
                }
            }

            public static class Selectors
            {
                public static Selector Nesting(this Selector? previous) => new();
                public static Selector Is<T>(this Selector? previous) where T : Avalonia.StyledElement => new();
                public static Selector Template(this Selector? previous) => new();
                public static Selector Child(this Selector? previous) => new();
                public static Selector Class(this Selector? previous, string name) => new();
            }

            public class ControlTheme
            {
            }
        }

        namespace Avalonia.Metadata
        {
            [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple = true)]
            public sealed class XmlnsDefinitionAttribute : System.Attribute
            {
                public XmlnsDefinitionAttribute(string xmlNamespace, string clrNamespace)
                {
                }
            }
        }

        namespace AtomUI.Theme
        {
            public enum SemanticPartCardinality
            {
                Single,
                Optional,
                Multiple
            }

            public enum SemanticPartCustomization
            {
                Root,
                Selector,
                SelectorAndTheme
            }

            [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
            public sealed class SemanticPartAttribute : System.Attribute
            {
                public SemanticPartAttribute(string name)
                {
                }

                public string? Path { get; set; }
                public string? SelectorClass { get; set; }
                public string? SelectorRoute { get; set; }
                public System.Type? ContractType { get; set; }
                public SemanticPartCardinality Cardinality { get; set; }
                public SemanticPartCustomization Customization { get; set; }
                public string? ThemePropertyName { get; set; }
                public bool CrossVisualRoot { get; set; }
                public bool CrossNestedOwners { get; set; }
                public bool RestHidden { get; set; }
                public string? Since { get; set; }
                public bool RuntimeCreated { get; set; }
            }

            public enum ThemeAppearance : byte
            {
                Light,
                Dark
            }

        }

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

            public enum SharedTokenKind
            {
                ControlHeight
            }

            public static class ControlTokenResourceKey
            {
                public static object Own(AtomUI.Theme.Schema.ControlTokenIdentity identity, object key) => key;
                public static object Global(
                    AtomUI.Theme.Schema.ControlTokenIdentity identity,
                    SharedTokenKind kind) => kind;
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

            public sealed class TokenDescriptor
            {
            }

            public sealed class ControlTokenDescriptor
            {
                public ControlTokenIdentity Identity { get; }

                public ControlTokenDescriptor(System.Type controlType, ControlTokenIdentity identity)
                {
                    Identity = identity;
                }
            }

            public sealed class ControlThemeAssetDescriptor
            {
                public ControlTokenIdentity OwnerIdentity { get; }
                public System.Collections.Generic.IReadOnlyList<ControlTokenIdentity> ReferencedControlIdentities { get; } =
                    System.Array.Empty<ControlTokenIdentity>();
            }

            public sealed class ControlThemeSemanticPartDescriptor
            {
                public ControlThemeSemanticPartDescriptor(string propertyName, System.Type targetType)
                {
                }
            }

            public sealed class SemanticPartDescriptor
            {
                public SemanticPartDescriptor(
                    string name,
                    string path,
                    string? selectorClass,
                    System.Type contractType,
                    AtomUI.Theme.SemanticPartCardinality cardinality,
                    AtomUI.Theme.SemanticPartCustomization customization,
                    ControlThemeSemanticPartDescriptor? theme,
                    bool crossVisualRoot,
                    string since,
                    bool runtimeCreated,
                    string? selectorRoute = null,
                    System.Type? styleType = null,
                    bool crossNestedOwners = false,
                    bool restHidden = false)
                {
                }
            }

            public sealed class ControlSemanticDescriptor
            {
                public ControlTokenIdentity Identity { get; }

                public ControlSemanticDescriptor(
                    System.Type controlType,
                    ControlTokenIdentity identity,
                    System.Collections.Generic.IEnumerable<SemanticPartDescriptor> parts)
                {
                    Identity = identity;
                }
            }

            public sealed class ThemeAlgorithmDescriptor
            {
            }
        }

        namespace AtomUI.Theme.DesignTokens
        {
            [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false)]
            public sealed class ControlDesignTokenAttribute : System.Attribute
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
