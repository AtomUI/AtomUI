using System.Collections.Immutable;
using AtomUI.Theme.Build;
using AtomUI.Theme.Definitions.Compiled;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Shouldly;
using Xunit;
using static AtomUI.Generator.Tests.DesignLanguages.DesignLanguageTestHost;

namespace AtomUI.Generator.Tests.ThemeDefinitions;

/// <summary>
/// The compiled theme ABI 4 programs of theme definition format v2: a theme names its language, fixes or leaves open
/// its axes, and holds a base section plus <c>When</c> condition sections. Every token is bound at build time against
/// the build contracts of the referenced packages (Core's global tokens, the language package's language tokens, the
/// control package's control tokens), and the programs compile against the runtime ABI.
/// </summary>
public class CompiledThemePackageWriterTests
{
    private const string DiagnosticId = "ATOMUITOKEN002";

    [Fact]
    public void Emits_An_Abi4_Program_With_The_Base_Section_And_Its_Condition_Sections()
    {
        var input = Input();
        input.Base = new ThemeSectionInput
        {
            Tokens = [Color("ColorPrimary", 0xFF1677FF, locked: true), Number("BorderRadius", 6)],
            LanguageTokens = [Text("Ring", "#FB7185"), Number("BaseScale", 1.5)],
            Controls =
            [
                new ThemeControlInput
                {
                    Catalog = "AtomUI", Id = "ToolTip", Appearance = 1, Line = 9, Column = 5,
                    Tokens = [Number("Padding", 4), Number("BorderRadius", 2)]
                }
            ]
        };
        input.Conditions =
        [
            new ThemeConditionInput
            {
                Appearance = 1, Line = 20, Column = 3,
                Section = new ThemeSectionInput
                {
                    Tokens = [Text("ColorLink", "#0B0B0F")],
                    Controls = [new ThemeControlInput { Catalog = "AtomUI", Id = "ToolTip", Tokens = [Number("Padding", 6)] }]
                }
            },
            new ThemeConditionInput
            {
                Idiom = "Mobile", Line = 30, Column = 3,
                Section = new ThemeSectionInput { Tokens = [Number("FontSize", 16)], LanguageTokens = [Number("InputRadius", 8)] }
            }
        ];

        var package = RunTheme(out var diagnostics, input);

        diagnostics.Where(static d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        ErrorsIn(package, PackageFile).ShouldBeEmpty();
        var source = GetGeneratedSource(package, PackageFile);
        source.ShouldContain("[assembly: global::AtomUI.Theme.Definitions.Compiled.CompiledThemeManifestAttribute(1, \"Acme.Themes\", ");
        source.ShouldContain("namespace AtomUI.Generated.AcmeThemes;");
        source.ShouldContain("public static class ThemePackageRegistration");
        source.ShouldContain("public static class GeneratedThemePackageRegistration");
        source.ShouldContain("global::System.ComponentModel.EditorBrowsableState.Never");
        source.ShouldContain("CreatePackage() => ThemePackageRegistration.CreatePackage();");
        source.ShouldContain("builder) => ThemePackageRegistration.Register(builder);");
        source.ShouldContain(
            "public static global::AtomUI.Theme.Definitions.Compiled.CompiledThemePackage CreatePackage() => new global::AtomUI.Theme.Definitions.Compiled.CompiledThemePackage(\"Acme.Themes\", new global::AtomUI.Theme.Definitions.Compiled.CompiledThemeProgram[] { new CompiledThemeProgram_0() });");
        source.ShouldContain(
            "public static void Register(global::AtomUI.Theme.IThemeManagerBuilder builder) => builder.AddCompiledThemePackage(CreatePackage());");
        source.ShouldContain("internal sealed class CompiledThemeProgram_0 : global::AtomUI.Theme.Definitions.Compiled.CompiledThemeProgram");
        // ABI 4: identity, language, open axes, the locked seed and the accent from the base ColorPrimary.
        source.ShouldContain(
            "internal CompiledThemeProgram_0() : base(4, \"Themes/Acme.xml\", \"" + Digest + "\", \"" + Digest + "\", \"");
        source.ShouldContain(
            "\", \"Acme\", \"Acme Brand\", \"Shadcn\", null, -1, null, new string[] { \"ColorPrimary\" }, new global::Avalonia.Media.Color(255, 22, 119, 255)) { }");
        source.ShouldContain("protected override bool IsBackgroundPrepareSafe => true;");
        source.ShouldNotContain("SetAlgorithms");
        source.ShouldNotContain("ThemeAlgorithm");

        var describe = Lines(source, "protected override void Describe(", "protected override void WriteStaticDefaults(");
        describe.ShouldBe([
            "builder.BeginSection(0, null, -1, null, null);",
            "builder.RequireGlobal<global::Avalonia.Media.Color>(0, global::AtomUI.Theme.Resources.SharedTokenKind.ColorPrimary, \"ColorPrimary\", (global::AtomUI.Theme.Schema.TokenStage)0, 1);",
            "builder.RequireGlobal<global::System.Double>(1, global::AtomUI.Theme.Resources.SharedTokenKind.BorderRadius, \"BorderRadius\", (global::AtomUI.Theme.Schema.TokenStage)1, 1);",
            "builder.RequireLanguage<global::System.String>(2, \"Ring\", (global::AtomUI.Theme.Schema.TokenStage)4, 1);",
            "builder.RequireLanguage<global::System.Double>(3, \"BaseScale\", (global::AtomUI.Theme.Schema.TokenStage)0, 1);",
            "builder.RequireControl(0, global::Acme.Controls.DesignTokens.ToolTipTokens.Identity, \"AtomUI\", \"ToolTip\", \"Acme.Controls, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\", \"Acme.Controls.ToolTip\", 1, null);",
            "builder.RequireOwn<global::System.Double>(0, 4, global::Acme.Controls.DesignTokens.ToolTipTokenKind.Padding, \"Padding\", 1);",
            "builder.RequireControlGlobal<global::System.Double>(0, 5, global::AtomUI.Theme.Resources.SharedTokenKind.BorderRadius, \"BorderRadius\", (global::AtomUI.Theme.Schema.TokenStage)1, 1);",
            "builder.BeginSection(1, null, 1, null, null);",
            "builder.RequireGlobal<global::System.String>(6, global::AtomUI.Theme.Resources.SharedTokenKind.ColorLink, \"ColorLink\", (global::AtomUI.Theme.Schema.TokenStage)2, 1);",
            "builder.RequireControl(1, global::Acme.Controls.DesignTokens.ToolTipTokens.Identity, \"AtomUI\", \"ToolTip\", \"Acme.Controls, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\", \"Acme.Controls.ToolTip\", -1, null);",
            "builder.RequireOwn<global::System.Double>(1, 7, global::Acme.Controls.DesignTokens.ToolTipTokenKind.Padding, \"Padding\", 1);",
            "builder.BeginSection(2, null, -1, null, \"Mobile\");",
            "builder.RequireGlobal<global::System.Double>(8, global::AtomUI.Theme.Resources.SharedTokenKind.FontSize, \"FontSize\", (global::AtomUI.Theme.Schema.TokenStage)0, 1);",
            "builder.RequireLanguage<global::System.Double>(9, \"InputRadius\", (global::AtomUI.Theme.Schema.TokenStage)4, 1);"
        ]);
        var write = Lines(source, "protected override void WriteStaticDefaults(", "\n}");
        write.ShouldBe([
            "writer.BeginSection(0);",
            "writer.SetGlobal<global::Avalonia.Media.Color>(0, new global::Avalonia.Media.Color(255, 22, 119, 255), \"#FF1677FF\");",
            "writer.SetGlobal<global::System.Double>(1, 6D, \"6\");",
            "writer.SetLanguage<global::System.String>(2, \"#FB7185\", \"#FB7185\");",
            "writer.SetLanguage<global::System.Double>(3, 1.5D, \"1.5\");",
            "writer.SetOwn<global::System.Double>(0, 4, 4D, \"4\");",
            "writer.SetControlGlobal<global::System.Double>(0, 5, 2D, \"2\");",
            "writer.BeginSection(1);",
            "writer.SetGlobal<global::System.String>(6, \"#0B0B0F\", \"#0B0B0F\");",
            "writer.SetOwn<global::System.Double>(1, 7, 6D, \"6\");",
            "writer.BeginSection(2);",
            "writer.SetGlobal<global::System.Double>(8, 16D, \"16\");",
            "writer.SetLanguage<global::System.Double>(9, 8D, \"8\");"
        ]);
    }

    [Theory]
    [InlineData("Nova", 1, "Compact", "\"Nova\", 1, \"Compact\"")]
    [InlineData("Nova", -1, "", "\"Nova\", -1, null")]
    [InlineData("", 0, "", "null, 0, null")]
    [InlineData("", -1, "Compact", "null, -1, \"Compact\"")]
    public void Writes_Fixed_And_Open_Axes_Into_The_Base_Constructor(string style, int appearance, string density, string expected)
    {
        var input = Input();
        input.Style = style;
        input.Appearance = appearance;
        input.Density = density;

        var package = RunTheme(out var diagnostics, input);

        diagnostics.Where(static d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        ErrorsIn(package, PackageFile).ShouldBeEmpty();
        // No tokens: no locked seeds and no accent.
        GetGeneratedSource(package, PackageFile).ShouldContain(
            "\"Acme\", \"Acme Brand\", \"Shadcn\", " + expected + ", new string[] {  }, null) { }");
    }

    [Fact]
    public void Writes_Every_Condition_Attribute_Of_A_Section_In_Document_Order()
    {
        var input = Input();
        input.Conditions =
        [
            Condition(new ThemeConditionInput { Style = "Nova", Appearance = 0, Density = "Compact", Idiom = "Desktop" }, Number("FontSize", 12)),
            Condition(new ThemeConditionInput { Density = "Compact" }, Number("FontSize", 13)),
            Condition(new ThemeConditionInput { Appearance = 1 }, Number("FontSize", 14))
        ];

        var source = GetGeneratedSource(RunTheme(out var diagnostics, input), PackageFile);

        diagnostics.Where(static d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        var describe = Lines(source, "protected override void Describe(", "protected override void WriteStaticDefaults(");
        describe.Where(static line => line.StartsWith("builder.BeginSection(", StringComparison.Ordinal)).ShouldBe([
            "builder.BeginSection(0, null, -1, null, null);",
            "builder.BeginSection(1, \"Nova\", 0, \"Compact\", \"Desktop\");",
            "builder.BeginSection(2, null, -1, \"Compact\", null);",
            "builder.BeginSection(3, null, 1, null, null);"
        ]);
        var write = Lines(source, "protected override void WriteStaticDefaults(", "\n}");
        write.ShouldBe([
            "writer.BeginSection(0);",
            "writer.BeginSection(1);",
            "writer.SetGlobal<global::System.Double>(0, 12D, \"12\");",
            "writer.BeginSection(2);",
            "writer.SetGlobal<global::System.Double>(1, 13D, \"13\");",
            "writer.BeginSection(3);",
            "writer.SetGlobal<global::System.Double>(2, 14D, \"14\");"
        ]);
    }

    [Fact]
    public void Locks_Global_And_Language_Seeds_Of_The_Base_Section()
    {
        var input = Input();
        input.Base = new ThemeSectionInput
        {
            Tokens = [Number("FontSize", 14, locked: true)],
            LanguageTokens = [Number("BaseScale", 2, locked: true), Text("Ring", "#FB7185")]
        };

        var source = GetGeneratedSource(RunTheme(out var diagnostics, input), PackageFile);

        diagnostics.Where(static d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        // Locked seeds in binding order; without a base ColorPrimary there is no accent.
        source.ShouldContain("new string[] { \"FontSize\", \"BaseScale\" }, null) { }");
    }

    [Fact]
    public void The_Accent_Comes_Only_From_The_Base_Section_ColorPrimary()
    {
        var input = Input();
        input.Conditions = [Condition(new ThemeConditionInput { Appearance = 1 }, Color("ColorPrimary", 0xFF0958D9))];
        input.Base = new ThemeSectionInput
        {
            Controls = [new ThemeControlInput { Catalog = "AtomUI", Id = "ToolTip", Tokens = [Color("ColorPrimary", 0xFFE11D48)] }]
        };

        var package = RunTheme(out var diagnostics, input);

        diagnostics.Where(static d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        ErrorsIn(package, PackageFile).ShouldBeEmpty();
        var source = GetGeneratedSource(package, PackageFile);
        source.ShouldContain("new string[] {  }, null) { }");
        source.ShouldContain(
            "builder.RequireControlGlobal<global::Avalonia.Media.Color>(0, 0, global::AtomUI.Theme.Resources.SharedTokenKind.ColorPrimary, \"ColorPrimary\", (global::AtomUI.Theme.Schema.TokenStage)0, 1);");
        source.ShouldContain(
            "builder.RequireGlobal<global::Avalonia.Media.Color>(1, global::AtomUI.Theme.Resources.SharedTokenKind.ColorPrimary, \"ColorPrimary\", (global::AtomUI.Theme.Schema.TokenStage)0, 1);");
    }

    [Theory]
    [InlineData("base-map")]
    [InlineData("condition-seed")]
    [InlineData("control-seed")]
    [InlineData("language-derived")]
    public void Rejects_Locked_Outside_A_Seed_Of_The_Base_Section(string placement)
    {
        var input = Input();
        var name = "FontSize";
        switch (placement)
        {
            case "base-map":
                name = "BorderRadius";
                input.Base = new ThemeSectionInput { Tokens = [Number(name, 4, locked: true, line: 7, column: 9)] };
                break;
            case "condition-seed":
                input.Conditions = [Condition(new ThemeConditionInput { Appearance = 1 }, Number(name, 14, locked: true, line: 7, column: 9))];
                break;
            case "control-seed":
                input.Base = new ThemeSectionInput
                {
                    Controls = [new ThemeControlInput { Catalog = "AtomUI", Id = "ToolTip", Tokens = [Number(name, 14, locked: true, line: 7, column: 9)] }]
                };
                break;
            default:
                name = "InputRadius";
                input.Base = new ThemeSectionInput { LanguageTokens = [Number(name, 4, locked: true, line: 7, column: 9)] };
                break;
        }

        var package = RunTheme(out var diagnostics, input);

        var failure = diagnostics.Where(static d => d.Id == DiagnosticId).ShouldHaveSingleItem();
        failure.GetMessage(null).ShouldBe("Locked applies only to a seed in the base section; '" + name + "' is not one.");
        AssertAt(failure, input.SourcePath, 7, 9);
        HasGeneratedSource(package, PackageFile).ShouldBeFalse();
    }

    [Fact]
    public void Reports_A_Language_That_No_Referenced_Package_Declares()
    {
        var input = Input();
        input.Language = "PrimeNG";
        input.Line = 2;
        input.Column = 3;

        var package = RunTheme(out var diagnostics, input);

        var failure = diagnostics.Where(static d => d.Id == DiagnosticId).ShouldHaveSingleItem();
        failure.GetMessage(null).ShouldBe("Design language 'PrimeNG' is not declared by any referenced language package.");
        AssertAt(failure, input.SourcePath, 2, 3);
        HasGeneratedSource(package, PackageFile).ShouldBeFalse();
    }

    [Fact]
    public void Language_Tokens_Bind_Only_To_The_Themes_Own_Language()
    {
        var input = Input();
        // A global token name in LanguageTokens, and a language token name in Tokens.
        input.Base = new ThemeSectionInput
        {
            Tokens = [Text("Ring", "#FB7185", line: 4, column: 5)],
            LanguageTokens = [Number("FontSize", 14, line: 6, column: 5), Number("Missing", 1, line: 7, column: 5)]
        };

        var package = RunTheme(out var diagnostics, input);

        var failures = diagnostics.Where(static d => d.Id == DiagnosticId).ToArray();
        failures.Select(static d => d.GetMessage(null)).ShouldBe([
            "Unknown Token 'Ring'.",
            "'FontSize' is not a language Token of 'Shadcn'.",
            "'Missing' is not a language Token of 'Shadcn'."
        ]);
        AssertAt(failures[0], input.SourcePath, 4, 5);
        AssertAt(failures[1], input.SourcePath, 6, 5);
        AssertAt(failures[2], input.SourcePath, 7, 5);
        HasGeneratedSource(package, PackageFile).ShouldBeFalse();
    }

    [Fact]
    public void Reports_Unknown_Repeated_And_Axis_Fixing_Controls()
    {
        var input = Input();
        input.Base = new ThemeSectionInput
        {
            Controls =
            [
                new ThemeControlInput { Catalog = "AtomUI", Id = "Missing", Line = 3, Column = 5, Tokens = [Number("Padding", 1)] },
                new ThemeControlInput { Catalog = "AtomUI", Id = "ToolTip", Line = 4, Column = 5, Tokens = [Number("Padding", 1)] },
                new ThemeControlInput { Catalog = "AtomUI", Id = "ToolTip", Line = 5, Column = 5, Tokens = [Number("Padding", 2)] }
            ]
        };
        input.Conditions =
        [
            new ThemeConditionInput
            {
                Appearance = 1,
                Section = new ThemeSectionInput
                {
                    Controls = [new ThemeControlInput { Catalog = "AtomUI", Id = "ToolTip", Density = "Compact", Line = 8, Column = 7 }]
                }
            }
        ];

        var package = RunTheme(out var diagnostics, input);

        var failures = diagnostics.Where(static d => d.Id == DiagnosticId).ToArray();
        failures.Select(static d => d.GetMessage(null)).ShouldBe([
            "Unknown or repeated Control 'AtomUI:Missing'.",
            "Unknown or repeated Control 'AtomUI:ToolTip'.",
            "Control 'AtomUI:ToolTip' cannot fix an axis in a condition section."
        ]);
        AssertAt(failures[0], input.SourcePath, 3, 5);
        AssertAt(failures[1], input.SourcePath, 5, 5);
        AssertAt(failures[2], input.SourcePath, 8, 7);
        HasGeneratedSource(package, PackageFile).ShouldBeFalse();
    }

    [Fact]
    public void Reports_An_Unknown_Token_And_A_Literal_The_Codec_Rejected()
    {
        var input = Input();
        var wrongType = Number("ColorLink", 3, line: 5, column: 7);
        input.Base = new ThemeSectionInput { Tokens = [Number("Unknown", 1, line: 4, column: 5), wrongType] };

        var package = RunTheme(out var diagnostics, input);

        var failures = diagnostics.Where(static d => d.Id == DiagnosticId).ToArray();
        failures.Select(static d => d.GetMessage(null)).ShouldBe([
            "Unknown Token 'Unknown'.",
            "Invalid static literal for 'ColorLink': Core codec rejected the literal for 'System.String'."
        ]);
        AssertAt(failures[1], input.SourcePath, 5, 7);
        HasGeneratedSource(package, PackageFile).ShouldBeFalse();
    }

    [Fact]
    public void Reports_A_Duplicate_Theme_Id_And_Keeps_The_First_Definition()
    {
        var first = Input();
        var second = Input();
        second.SourcePath = "/repo/Themes/Copy.xml";

        var package = RunTheme(out var diagnostics, first, second);

        diagnostics.Where(static d => d.Id == DiagnosticId).ShouldHaveSingleItem().GetMessage(null)
                   .ShouldBe("Duplicate compiled theme Id: Acme");
        var source = GetGeneratedSource(package, PackageFile);
        CountOf(source, "internal sealed class CompiledThemeProgram_").ShouldBe(1);
    }

    [Fact]
    public void Emits_One_Program_Per_Theme_And_A_Deterministic_Program_Digest()
    {
        var light = Input();
        light.Id = "AcmeLight";
        light.Appearance = 0;
        var dark = Input();
        dark.Id = "AcmeDark";
        dark.Appearance = 1;

        var first = GetGeneratedSource(RunTheme(out var diagnostics, light, dark), PackageFile);
        var again = GetGeneratedSource(RunTheme(out _, light, dark), PackageFile);

        diagnostics.Where(static d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        first.ShouldContain("new CompiledThemeProgram_0(), new CompiledThemeProgram_1()");
        first.ShouldContain("\"AcmeLight\", \"Acme Brand\", \"Shadcn\", null, 0, null");
        first.ShouldContain("\"AcmeDark\", \"Acme Brand\", \"Shadcn\", null, 1, null");
        again.ShouldBe(first);
        ProgramDigest(first, "AcmeLight").ShouldNotBe(ProgramDigest(first, "AcmeDark"));
    }

    [Fact]
    public void The_Program_Digest_Depends_On_The_Order_Of_Condition_Sections()
    {
        var input = Input();
        input.Conditions =
        [
            Condition(new ThemeConditionInput { Appearance = 1 }, Number("FontSize", 12)),
            Condition(new ThemeConditionInput { Density = "Compact" }, Number("FontSize", 13))
        ];
        var swapped = Input();
        swapped.Conditions = [input.Conditions[1], input.Conditions[0]];

        var original = GetGeneratedSource(RunTheme(out _, input), PackageFile);
        var reordered = GetGeneratedSource(RunTheme(out _, swapped), PackageFile);

        ProgramDigest(reordered, "Acme").ShouldNotBe(ProgramDigest(original, "Acme"));
    }

    [Fact]
    public void Rejects_A_Theme_Input_Written_By_An_Older_Reader()
    {
        // Format 7 carried v1 theme definitions (algorithm chains); the generator asks for a rebuild instead of guessing.
        var stale = Convert.ToBase64String(BitConverter.GetBytes(7));
        var package = RunRaw(CreatePackage("Acme.Themes", MarkerSource, ThemeReferences()), out var diagnostics,
            new RawThemeInput(stale));

        var failure = diagnostics.Where(static d => d.Id == DiagnosticId).ShouldHaveSingleItem();
        failure.GetMessage(null).ShouldBe("Compiled theme input format is out of date; rebuild the project.");
        failure.Location.GetLineSpan().Path.ShouldBe("/obj/stale" + CompiledThemeInput.Extension);
        HasGeneratedSource(package, PackageFile).ShouldBeFalse();
    }

    [Fact]
    public void Reads_The_Language_Build_Contracts_Of_Every_Referenced_Language_Package()
    {
        var core = CoreReference();
        var broken = CreatePackage("Acme.Broken", """
            [assembly: AtomUI.Theme.Definitions.Compiled.ThemeBuildLanguageAttribute(2, "Future", "Acme.FutureToken", "Acme.Broken")]
            [assembly: AtomUI.Theme.Definitions.Compiled.ThemeBuildLanguageAttribute(1, "Bad Id", "Acme.BadToken", "Acme.Broken")]
            [assembly: AtomUI.Theme.Definitions.Compiled.ThemeBuildLanguageAttribute(1, "Shadcn", "Acme.OtherShadcnToken", "Acme.Broken")]
            namespace Acme { public sealed class BrokenMarker { } }
            """, core).ToMetadataReference();
        var input = Input();
        input.Base = new ThemeSectionInput { LanguageTokens = [Text("Ring", "#FB7185")] };

        var package = Run(CreatePackage("Acme.Themes", MarkerSource, [.. ThemeReferences(core), broken]), out var diagnostics, input);

        // An unsupported version, an invalid identity and a second "Shadcn" are rejected; the real Shadcn still binds.
        diagnostics.Where(static d => d.Id == DiagnosticId).Select(static d => d.GetMessage(null)).ShouldBe([
            "Invalid Token build metadata in 'Acme.Broken, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null': Unsupported Token build metadata version.",
            "Invalid Token build metadata in 'Acme.Broken, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null': Invalid language identity.",
            "Invalid Token build metadata in 'Acme.Broken, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null': Duplicate language build contract: Shadcn"
        ], ignoreOrder: true);
        GetGeneratedSource(package, PackageFile).ShouldContain("builder.RequireLanguage<global::System.String>(0, \"Ring\", (global::AtomUI.Theme.Schema.TokenStage)4, 1);");
    }

    [Fact]
    public void A_Language_Package_Compiles_Its_Own_Themes_Against_Its_Local_Language()
    {
        var input = Input();
        input.Base = new ThemeSectionInput { LanguageTokens = [Number("BaseScale", 2, locked: true)], Tokens = [Number("FontSize", 15)] };

        var package = Run(CreatePackage("Acme.Shadcn", LanguageSource, CoreReference()), out var diagnostics, input);

        diagnostics.Where(static d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        ErrorsIn(package, PackageFile).ShouldBeEmpty();
        var source = GetGeneratedSource(package, PackageFile);
        source.ShouldContain("builder.RequireLanguage<global::System.Double>(1, \"BaseScale\", (global::AtomUI.Theme.Schema.TokenStage)0, 1);");
        source.ShouldContain("new string[] { \"BaseScale\" }");
    }

    private const string Digest = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
    private const string MarkerSource = "namespace Acme.Themes { public sealed class Marker { } }";

    private static CompiledThemeInput Input() => new()
    {
        SourceIdentity = "Themes/Acme.xml",
        SourcePath = "/repo/Themes/Acme.xml",
        ContentDigest = Digest,
        Id = "Acme",
        Name = "Acme Brand",
        Language = "Shadcn"
    };

    private static ThemeConditionInput Condition(ThemeConditionInput condition, params ThemeLiteralInput[] tokens)
    {
        condition.Section = new ThemeSectionInput { Tokens = tokens };
        return condition;
    }

    private static ThemeLiteralInput Number(string name, double value, bool locked = false, int line = 0, int column = 0)
    {
        var text = value.ToString("R", System.Globalization.CultureInfo.InvariantCulture);
        return new ThemeLiteralInput
        {
            Name = name, Value = text, Locked = locked, Line = line, Column = column,
            Values = [new BuiltInThemeLiteralValue { Type = "System.Double", Kind = "double", Numbers = [value], Canonical = text }]
        };
    }

    private static ThemeLiteralInput Text(string name, string value, int line = 0, int column = 0) => new()
    {
        Name = name, Value = value, Line = line, Column = column,
        Values = [new BuiltInThemeLiteralValue { Type = "System.String", Kind = "string", Text = value, Canonical = value }]
    };

    private static ThemeLiteralInput Color(string name, long argb, bool locked = false)
    {
        var canonical = "#" + argb.ToString("X8", System.Globalization.CultureInfo.InvariantCulture);
        return new ThemeLiteralInput
        {
            Name = name, Value = canonical, Locked = locked,
            Values = [new BuiltInThemeLiteralValue { Type = "Avalonia.Media.Color", Kind = "color", Integer = argb, Canonical = canonical }]
        };
    }

    private static MetadataReference[] ThemeReferences(MetadataReference? core = null)
    {
        core ??= CoreReference();
        return
        [
            core,
            Reference(CreatePackage("Acme.Shadcn", LanguageSource, core), "Acme.Shadcn"),
            Reference(CreatePackage("Acme.Controls", ControlsSource, core), "Acme.Controls")
        ];
    }

    private static CSharpCompilation RunTheme(out ImmutableArray<Diagnostic> diagnostics, params CompiledThemeInput[] inputs) =>
        Run(CreatePackage("Acme.Themes", MarkerSource, ThemeReferences()), out diagnostics, inputs);

    private static CSharpCompilation RunRaw(CSharpCompilation compilation, out ImmutableArray<Diagnostic> diagnostics,
        RawThemeInput input)
    {
        var driver = CSharpGeneratorDriver.Create([new TokenResourceKeyGenerator().AsSourceGenerator()],
            [input], (CSharpParseOptions)compilation.SyntaxTrees[0].Options, new BuiltinCatalogOptionsProvider(null));
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out diagnostics, TestContext.Current.CancellationToken);
        return (CSharpCompilation)output;
    }

    // The statement lines of one generated method body, trimmed.
    private static string[] Lines(string source, string startMarker, string endMarker)
    {
        var start = source.IndexOf(startMarker, StringComparison.Ordinal);
        start.ShouldBeGreaterThanOrEqualTo(0);
        start = source.IndexOf('{', start) + 1;
        var end = source.IndexOf(endMarker, start, StringComparison.Ordinal);
        end.ShouldBeGreaterThan(start);
        return source.Substring(start, end - start).Split('\n').Select(static line => line.Trim())
                     .Where(static line => line.Length != 0 && line != "{" && line != "}").ToArray();
    }

    private static string ProgramDigest(string source, string themeId)
    {
        // base(4, sourceIdentity, sourceRevision, contentDigest, programDigest, id, ...)
        var marker = "\", \"" + themeId + "\", \"Acme Brand\"";
        var end = source.IndexOf(marker, StringComparison.Ordinal);
        end.ShouldBeGreaterThan(0);
        var start = source.LastIndexOf('"', end - 1) + 1;
        return source.Substring(start, end - start);
    }

    private static void AssertAt(Diagnostic diagnostic, string path, int line, int column)
    {
        var span = diagnostic.Location.GetLineSpan();
        span.Path.ShouldBe(path);
        span.StartLinePosition.ShouldBe(new LinePosition(line - 1, column - 1));
    }

    private sealed class RawThemeInput(string text) : AdditionalText
    {
        private readonly SourceText _text = SourceText.From(text);
        public override string Path => "/obj/stale" + CompiledThemeInput.Extension;
        public override SourceText GetText(CancellationToken cancellationToken = default) => _text;
    }
}
