using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;

namespace AtomUI.Generator.Tests.DesignToken;

public class OwnTokenReferenceAnalysisTests
{
    private const string Unreferenced = "ATOMUIGEN042";

    // ---- Own Token states -----------------------------------------------------------------

    [Fact]
    public void Own_Token_Referenced_By_Theme_Axaml_Is_Active_And_Unreferenced_Fields_Are_Reported()
    {
        var run = Run(DemoTokens, ButtonTheme("""<Setter Property="Height" Value="{atom:ButtonTokenResource Height}" />"""));

        run.Entry("Button", "Height").State.ShouldBe("Active");
        run.Entry("Button", "Height").Reason.ShouldBeNull();
        var diagnostic = run.Diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe(Unreferenced);
        diagnostic.Severity.ShouldBe(DiagnosticSeverity.Warning);
        diagnostic.GetMessage().ShouldContain("Label");
        diagnostic.GetMessage().ShouldContain("Caption");
        run.Entry("Label", "Caption").State.ShouldBe("Unreferenced");
        run.Entry("Label", "Caption").Reason.ShouldBeNull();
    }

    [Fact]
    public void Unreferenced_Diagnostic_Points_At_The_Property_Declaration()
    {
        var run = Run(DemoTokens, ButtonTheme("""<Setter Property="Height" Value="{atom:ButtonTokenResource Height}" />"""));

        var diagnostic = run.Diagnostics.ShouldHaveSingleItem();
        var span = diagnostic.Location.GetLineSpan();
        span.Path.ShouldBe("TokenSource.cs");
        DemoTokens.Split('\n')[span.StartLinePosition.Line].ShouldContain("string Caption");
    }

    [Fact]
    public void ReservedToken_Is_Reserved_With_Its_Reason_And_Is_Not_Reported()
    {
        var run = Run(DemoTokens, ButtonTheme("""<Setter Property="Height" Value="{atom:ButtonTokenResource Height}" />"""));

        run.Entry("Label", "Gap").State.ShouldBe("Reserved");
        run.Entry("Label", "Gap").Reason.ShouldBe("等待 Group 实现");
        run.Diagnostics.ShouldNotContain(static diagnostic => diagnostic.GetMessage().Contains("Gap"));
    }

    [Fact]
    public void A_Referenced_ReservedToken_Is_Active()
    {
        var run = Run(DemoTokens, ButtonTheme("""<Setter Property="Height" Value="{atom:ButtonTokenResource Height}" />"""),
            LabelTheme("""<Setter Property="Margin" Value="{atom:LabelTokenResource Gap}" />"""));

        run.Entry("Label", "Gap").State.ShouldBe("Active");
        run.Entry("Label", "Gap").Reason.ShouldBeNull();
    }

    [Fact]
    public void Entries_Are_Sorted_By_Control_Then_Token()
    {
        var run = Run(DemoTokens);

        run.Entries.Select(static entry => $"{entry.ControlId}.{entry.Token}").ShouldBe(
            ["Button.Height", "Label.Caption", "Label.Gap"]);
        run.Entries.ShouldAllBe(static entry => entry.Catalog == "AtomUI");
    }

    // ---- reference forms ------------------------------------------------------------------

    [Theory]
    [InlineData("""<Setter Property="Height" Value="{atom:ButtonTokenResource Height}" />""")]
    [InlineData("""<Setter Property="Height" Value="{atom:ButtonTokenResource Kind=Height}" />""")]
    [InlineData("""<Setter Property="Height" Value="{atom:ButtonTokenResource   Height  }" />""")]
    [InlineData("""<Setter Property="Height" Value="{DynamicResource {atom:ButtonTokenResource Height}}" />""")]
    [InlineData("""<Border Height="{atom:ButtonTokenResource Height}" />""")]
    [InlineData("""<Setter Property="Height"><atom:ButtonTokenResource Kind="Height" /></Setter>""")]
    public void Axaml_Reference_Forms_Make_The_Own_Token_Active(string body)
    {
        var run = Run(DemoTokens, ButtonTheme(body));

        run.Entry("Button", "Height").State.ShouldBe("Active");
    }

    [Fact]
    public void Axaml_Reference_Through_Another_Controls_TokenResource_Does_Not_Activate()
    {
        var run = Run(DemoTokens, ButtonTheme("""<Setter Property="Height" Value="{atom:LabelTokenResource Height}" />"""));

        run.Entry("Button", "Height").State.ShouldBe("Unreferenced");
    }

    [Theory]
    [InlineData("var key = global::Demo.ButtonTokenKind.Height;")]
    [InlineData("var key = ButtonTokenKind.Height;")]
    [InlineData("var key = ButtonTokenKey.Height;")]
    [InlineData("var key = Demo.ButtonTokenKind.Height;")]
    [InlineData("var key = nameof(ButtonTokenKind.Height);")]
    public void Code_Member_Access_Makes_The_Own_Token_Active(string statement)
    {
        var run = Run(DemoTokens + ConsumerClass(statement));

        run.Entry("Button", "Height").State.ShouldBe("Active");
    }

    [Fact]
    public void Code_Reference_Of_Another_Controls_Kind_Does_Not_Activate()
    {
        var run = Run(DemoTokens + ConsumerClass("var key = LabelTokenKind.Caption;"));

        run.Entry("Button", "Height").State.ShouldBe("Unreferenced");
        run.Entry("Label", "Caption").State.ShouldBe("Active");
    }

    [Fact]
    public void Reads_Inside_The_Tokens_Own_Evaluator_Do_Not_Count()
    {
        // ButtonToken.CalculateTokenValues reads ButtonTokenKind.Height through IsExplicit only.
        var run = Run(DemoTokens);

        run.Entry("Button", "Height").State.ShouldBe("Unreferenced");
    }

    [Fact]
    public void Plain_Identifiers_Strings_And_Comments_Do_Not_Count()
    {
        var run = Run(DemoTokens + ConsumerClass("""var text = "ButtonTokenKind.Height"; var height = Height;"""),
            ButtonTheme("""<!-- ButtonTokenResource Height --><Setter Property="Height" Value="Height" />"""));

        run.Entry("Button", "Height").State.ShouldBe("Unreferenced");
    }

    // ---- derivation chains ----------------------------------------------------------------

    [Fact]
    public void A_Base_Field_Read_To_Derive_An_Active_Field_Is_Active()
    {
        var run = Run(DerivationTokens, ButtonTheme("""<Setter Property="Height" Value="{atom:ButtonTokenResource Derived}" />"""));

        run.Entry("Button", "Derived").State.ShouldBe("Active");
        run.Entry("Button", "Base").State.ShouldBe("Active");
    }

    [Fact]
    public void A_Base_Field_That_Only_Feeds_An_Unreferenced_Field_Stays_Unreferenced()
    {
        var run = Run(DerivationTokens, ButtonTheme("""<Setter Property="Height" Value="{atom:ButtonTokenResource Derived}" />"""));

        run.Entry("Button", "Dead").State.ShouldBe("Unreferenced");
        run.Entry("Button", "Orphan").State.ShouldBe("Unreferenced");
        run.Diagnostics.Select(static diagnostic => diagnostic.GetMessage())
           .ShouldContain(message => message.Contains("Button.Orphan"));
        run.Diagnostics.Select(static diagnostic => diagnostic.GetMessage())
           .ShouldNotContain(message => message.Contains("Button.Base"));
    }

    [Fact]
    public void Derivation_Is_Transitive_Regardless_Of_Statement_Order()
    {
        // Leaf = Mid + 1 is written before Mid = Root * 2; only Leaf is consumed.
        var run = Run(DerivationTokens, ButtonTheme("""<Setter Property="Height" Value="{atom:ButtonTokenResource Leaf}" />"""));

        run.Entry("Button", "Leaf").State.ShouldBe("Active");
        run.Entry("Button", "Mid").State.ShouldBe("Active");
        run.Entry("Button", "Root").State.ShouldBe("Active");
    }

    [Fact]
    public void An_Internal_Alias_Consumed_By_Axaml_Makes_Its_Source_Active()
    {
        var run = Run(DerivationTokens, ButtonTheme("""<Setter Property="Height" Value="{atom:ButtonTokenResource Alias}" />"""));

        run.Entry("Button", "AliasSource").State.ShouldBe("Active");
        run.Entry("Button", "Base").State.ShouldBe("Unreferenced");
    }

    [Fact]
    public void An_Alias_Consumed_By_Code_Makes_Its_Source_Active()
    {
        var run = Run(DerivationTokens + ConsumerClass("var key = ButtonTokenKey.Alias;"));

        run.Entry("Button", "AliasSource").State.ShouldBe("Active");
    }

    [Fact]
    public void Reads_Of_Another_Objects_Member_Do_Not_Create_A_Derivation()
    {
        // Shown = peer.Lone and Shown = this.Base: only the `this` form reads this token's field.
        var run = Run(DerivationTokens, ButtonTheme("""<Setter Property="Height" Value="{atom:ButtonTokenResource Shown}" />"""));

        run.Entry("Button", "Lone").State.ShouldBe("Unreferenced");
        run.Entry("Button", "Base").State.ShouldBe("Active");
    }

    // ---- global consumption map -----------------------------------------------------------

    [Fact]
    public void Axaml_TokenResource_Global_Reads_Are_Consumers_Of_The_Named_Control()
    {
        var run = Run(DemoTokens, ButtonTheme("""
            <Setter Property="Foreground" Value="{atom:ButtonTokenResource ColorPrimary}" />
            <Setter Property="Background" Value="{atom:ButtonTokenResource ColorPrimary}" />
            """));

        run.Consumers.ShouldContain(("AtomUI", "Button", "ColorPrimary", "axaml"));
        run.Consumers.Count(static consumer => consumer == ("AtomUI", "Button", "ColorPrimary", "axaml")).ShouldBe(1);
    }

    [Fact]
    public void OwnerTokenResource_Is_Attributed_To_The_TargetType_Owner()
    {
        var run = Run(DemoTokens, ButtonTheme("""<Setter Property="Foreground" Value="{atom:OwnerTokenResource ColorPrimary}" />"""));

        run.Consumers.ShouldContain(("AtomUI", "Button", "ColorPrimary", "axaml"));
        run.Consumers.ShouldNotContain(static consumer => consumer.Item2 == "shared-primitive");
    }

    [Fact]
    public void OwnerTokenResource_In_A_Theme_Without_Identity_Is_A_Shared_Primitive_Consumer()
    {
        var run = Run(DemoTokens, new Asset("Frame/Themes/FrameTheme.axaml", """
            <ControlTheme xmlns="https://github.com/avaloniaui" xmlns:atom="using:Demo" TargetType="atom:Frame">
                <Setter Property="Foreground" Value="{atom:OwnerTokenResource ColorPrimary}" />
            </ControlTheme>
            """));

        run.Consumers.ShouldContain(("AtomUI", "shared-primitive", "ColorPrimary", "axaml"));
    }

    [Fact]
    public void Own_Token_Reads_Are_Not_Global_Consumers()
    {
        var run = Run(DemoTokens, ButtonTheme("""<Setter Property="Height" Value="{atom:ButtonTokenResource Height}" />"""));

        run.Consumers.ShouldNotContain(static consumer => consumer.Item3 == "Height");
    }

    [Fact]
    public void Shared_And_Unknown_Global_Reads_Are_Not_Consumers()
    {
        var run = Run(DemoTokens, ButtonTheme("""
            <Setter Property="Foreground" Value="{atom:SharedTokenResource ColorPrimary}" />
            <Setter Property="Background" Value="{atom:ButtonTokenResource NotAGlobal}" />
            """));

        run.Consumers.Where(static consumer => consumer.Item4 == "axaml").ShouldBeEmpty();
    }

    [Fact]
    public void Code_Global_Reads_Are_Attributed_To_The_Enclosing_Control_Or_Shared_Primitive()
    {
        var run = Run(DemoTokens + """
            namespace Demo
            {
                public sealed partial class Button
                {
                    private void Apply(TokenResourceBinder binder, Accessor accessor)
                    {
                        var color = accessor.GetEffectiveGlobal<double>(SharedTokenKind.ColorPrimary);
                        binder.CreateControlTokenBinding(this, null, SharedTokenKind.ControlHeight);
                    }

                    private sealed class Tokens
                    {
                        internal T Global<T>(SharedTokenKind token) => default!;
                        private double Read() => Global<double>(SharedTokenKind.ColorText);
                    }
                }

                public static class Orphan
                {
                    public static object Read() => SharedTokenKind.ColorText;
                }
            }
            """);

        run.Consumers.ShouldContain(("AtomUI", "Button", "ColorPrimary", "code"));
        run.Consumers.ShouldContain(("AtomUI", "Button", "ControlHeight", "code"));
        run.Consumers.ShouldContain(("AtomUI", "Button", "ColorText", "code"));
        run.Consumers.ShouldContain(("AtomUI", "shared-primitive", "ColorText", "code"));
    }

    [Fact]
    public void EffectiveGlobalToken_Reads_Are_Attributed_To_The_Token_Class_Control()
    {
        var run = Run(DemoTokens, ButtonTheme("""<Setter Property="Height" Value="{atom:ButtonTokenResource Height}" />"""));

        run.Consumers.ShouldBe([("AtomUI", "Button", "ControlHeight", "code")]);
    }

    [Fact]
    public void A_Global_That_Only_Feeds_A_Field_Without_Consumer_Is_Not_Consumed()
    {
        // Height is Unreferenced here; LabelToken.Gap is Reserved. Neither makes its source global a consumer.
        var run = Run(DemoTokens.Replace(
            "public double Gap { get; set; }",
            "public double Gap { get; set; }\n\n                public override void CalculateTokenValues(bool isDark) { Gap = EffectiveGlobalToken.ControlHeight; }"));

        run.Consumers.ShouldBeEmpty();
    }

    [Fact]
    public void Global_Reads_Through_Locals_Or_Outside_Field_Assignments_Stay_Consumers()
    {
        var run = Run(DemoTokens.Replace(
            "Height = EffectiveGlobalToken.ControlHeight;",
            "var height = EffectiveGlobalToken.ControlHeight; Height = height;"));

        run.Consumers.ShouldBe([("AtomUI", "Button", "ControlHeight", "code")]);
    }

    [Fact]
    public void Consumers_Are_Sorted_And_Deduplicated_Across_Sources()
    {
        var run = Run(DemoTokens, ButtonTheme("""
            <Setter Property="Foreground" Value="{atom:ButtonTokenResource ColorText}" />
            <Setter Property="Background" Value="{atom:ButtonTokenResource ColorPrimary}" />
            <Setter Property="Height" Value="{atom:ButtonTokenResource Height}" />
            """), LabelTheme("""<Setter Property="Foreground" Value="{atom:LabelTokenResource ColorText}" />"""));

        run.Consumers.ShouldBe([
            ("AtomUI", "Button", "ColorPrimary", "axaml"),
            ("AtomUI", "Button", "ColorText", "axaml"),
            ("AtomUI", "Button", "ControlHeight", "code"),
            ("AtomUI", "Label", "ColorText", "axaml")
        ]);
    }

    // ---- infrastructure -------------------------------------------------------------------

    private static Asset ButtonTheme(string body) => new("Button/Themes/ButtonTheme.axaml", $"""
        <ControlTheme xmlns="https://github.com/avaloniaui" xmlns:atom="using:Demo" TargetType="atom:Button">
        {body}
        </ControlTheme>
        """);

    private static Asset LabelTheme(string body) => new("Label/Themes/LabelTheme.axaml", $"""
        <ControlTheme xmlns="https://github.com/avaloniaui" xmlns:atom="using:Demo" TargetType="atom:Label">
        {body}
        </ControlTheme>
        """);

    private static string ConsumerClass(string statement) => $$"""

        namespace Demo
        {
            internal static class Consumer
            {
                internal static void Use()
                {
                    {{statement}}
                }
            }
        }
        """;

    private static Result Run(string source, params Asset[] assets)
    {
        var references = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))!
                         .Split(Path.PathSeparator)
                         .Select(static path => MetadataReference.CreateFromFile(path))
                         .Cast<MetadataReference>()
                         .ToImmutableArray();
        var compilation = CSharpCompilation.Create(
            "OwnTokenReferenceAnalysisTests",
            [
                CSharpSyntaxTree.ParseText(source, path: "TokenSource.cs"),
                CSharpSyntaxTree.ParseText(ThemeSchemaGeneratorTests.AtomUIStubs)
            ],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var driver = CSharpGeneratorDriver.Create(
            [new TokenResourceKeyGenerator().AsSourceGenerator()],
            assets.Select(static asset => (AdditionalText)new InMemoryAdditionalText(asset.Path, asset.Text)).ToImmutableArray(),
            (CSharpParseOptions)compilation.SyntaxTrees[0].Options,
            new BuiltinCatalogOptionsProvider(null));
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics, TestContext.Current.CancellationToken);
        var generated = output.SyntaxTrees
                              .Single(static tree => tree.FilePath.EndsWith("GeneratedThemeConsumption.g.cs", StringComparison.Ordinal))
                              .GetText(TestContext.Current.CancellationToken).ToString();
        return new Result(
            diagnostics.Where(static diagnostic => diagnostic.Id == Unreferenced).ToImmutableArray(),
            ParseEntries(generated),
            ParseConsumers(generated));
    }

    // The generated arrays are asserted through their text form, one entry per line.
    private static List<OwnEntry> ParseEntries(string generated)
    {
        var result = new List<OwnEntry>();
        foreach (var line in generated.Split('\n'))
        {
            var match = Regex.Match(
                line.Trim(),
                "^new OwnTokenEntry\\(\"(?<catalog>[^\"]*)\", \"(?<control>[^\"]*)\", \"(?<token>[^\"]*)\", OwnTokenState\\.(?<state>\\w+), (?<reason>null|\"(?:[^\"\\\\]|\\\\.)*\")\\),$");
            if (!match.Success)
            {
                continue;
            }

            var reason = match.Groups["reason"].Value;
            result.Add(new OwnEntry(
                match.Groups["catalog"].Value,
                match.Groups["control"].Value,
                match.Groups["token"].Value,
                match.Groups["state"].Value,
                reason == "null" ? null : Regex.Unescape(reason.Substring(1, reason.Length - 2))));
        }

        return result;
    }

    private static List<(string, string, string, string)> ParseConsumers(string generated)
    {
        var result = new List<(string, string, string, string)>();
        foreach (var line in generated.Split('\n'))
        {
            var match = Regex.Match(
                line.Trim(),
                "^new GlobalConsumerEntry\\(\"(?<catalog>[^\"]*)\", \"(?<control>[^\"]*)\", \"(?<token>[^\"]*)\", \"(?<source>[^\"]*)\"\\),$");
            if (match.Success)
            {
                result.Add((match.Groups["catalog"].Value, match.Groups["control"].Value,
                    match.Groups["token"].Value, match.Groups["source"].Value));
            }
        }

        return result;
    }

    private sealed record Asset(string Path, string Text);

    private sealed record OwnEntry(string Catalog, string ControlId, string Token, string State, string? Reason);

    private sealed class Result(ImmutableArray<Diagnostic> diagnostics, List<OwnEntry> entries, List<(string, string, string, string)> consumers)
    {
        internal ImmutableArray<Diagnostic> Diagnostics { get; } = diagnostics;
        internal List<OwnEntry> Entries => entries;
        internal List<(string, string, string, string)> Consumers { get; } = consumers;

        internal OwnEntry Entry(string control, string token) =>
            entries.Single(entry => entry.ControlId == control && entry.Token == token);
    }

    private sealed class InMemoryAdditionalText : AdditionalText
    {
        private readonly Microsoft.CodeAnalysis.Text.SourceText _text;

        internal InMemoryAdditionalText(string path, string text)
        {
            Path = path;
            _text = Microsoft.CodeAnalysis.Text.SourceText.From(text);
        }

        public override string Path { get; }

        public override Microsoft.CodeAnalysis.Text.SourceText GetText(CancellationToken cancellationToken = default) => _text;
    }

    private const string DerivationTokens = """
        using AtomUI.Theme.DesignTokens;

        namespace Demo
        {
            [GlobalDesignToken]
            public partial class GlobalToken : AbstractDesignToken
            {
                public double ControlHeight { get; set; }
            }

            [ControlDesignToken]
            internal sealed class ButtonToken : AbstractControlDesignToken
            {
                public double Base { get; set; }
                public double Derived { get; set; }
                public double Orphan { get; set; }
                public double Dead { get; set; }
                public double Root { get; set; }
                public double Mid { get; set; }
                public double Leaf { get; set; }
                public double AliasSource { get; set; }
                internal double Alias { get; set; }
                public double Lone { get; set; }
                public double Shown { get; set; }

                public override void CalculateTokenValues(bool isDark)
                {
                    Leaf = Mid + 1;
                    Derived = Base * 2;
                    Dead = Orphan;
                    Mid = Root * 2;
                    Alias = AliasSource;
                    Shown = peer.Lone + this.Base;
                }
            }
        }
        """;

    private const string DemoTokens = """
        using AtomUI.Theme.DesignTokens;

        namespace Demo
        {
            [GlobalDesignToken]
            public partial class GlobalToken : AbstractDesignToken
            {
                public double ControlHeight { get; set; }
                public string ColorPrimary { get; set; } = string.Empty;
                public string ColorText { get; set; } = string.Empty;
            }

            public sealed class Label : Avalonia.Controls.Control
            {
            }

            [ControlDesignToken]
            internal sealed class ButtonToken : AbstractControlDesignToken
            {
                public double Height { get; set; }

                public override void CalculateTokenValues(bool isDark)
                {
                    if (!IsExplicit(ButtonTokenKind.Height))
                    {
                        Height = EffectiveGlobalToken.ControlHeight;
                    }
                }
            }

            [ControlDesignToken]
            internal sealed class LabelToken : AbstractControlDesignToken
            {
                public string Caption { get; set; } = string.Empty;

                [ReservedToken("等待 Group 实现")]
                public double Gap { get; set; }
            }
        }
        """;
}
