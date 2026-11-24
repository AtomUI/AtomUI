using System.Text;
using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Generated.AtomUICore;
using AtomUI.Theme;
using AtomUI.Theme.Definitions;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

/// <summary>Binding of a v2 theme document to the registry and the semantic checks of spec 7.10.</summary>
public class ThemeDefinitionBinderTests
{
    private const string Namespace = "https://atomui.net/schemas/theme/v2";
    private const string Source = "themes/test.theme.xml";

    [Fact]
    public void Bind_Produces_The_Language_Fixed_Axes_Sections_Locked_Seeds_And_Classified_Control_Tokens()
    {
        var definition = BindSuccess($$"""
            <Theme xmlns="{{Namespace}}" Id="T" Name="Theme" Language="Binder" Style="Rounded" Appearance="Dark" Density="Compact">
              <Tokens>
                <Token Name="ColorPrimary" Value="#722ED1" Locked="true" />
                <Token Name="BorderRadius" Value="4" />
              </Tokens>
              <Controls>
                <Control Catalog="AtomUI" Id="Button" Appearance="Light">
                  <Tokens>
                    <Token Name="Height" Value="40" />
                    <Token Name="LineWidth" Value="2" />
                  </Tokens>
                </Control>
              </Controls>
              <When Appearance="Dark">
                <Tokens><Token Name="ColorBgBase" Value="#000000" /></Tokens>
              </When>
              <When Density="Compact" Idiom="Desktop">
                <Controls>
                  <Control Catalog="AtomUI" Id="Button"><Tokens><Token Name="Height" Value="28" /></Tokens></Control>
                </Controls>
              </When>
            </Theme>
            """);

        definition.Id.ShouldBe("T");
        definition.Name.ShouldBe("Theme");
        definition.Language.Id.ShouldBe("Binder");
        definition.Style.ShouldBe(new DesignStyle("Rounded"));
        definition.Appearance.ShouldBe(ThemeAppearance.Dark);
        definition.Density.ShouldBe(DesignDensity.Compact);
        definition.LockedSeeds.ShouldBe(["ColorPrimary"]);
        definition.Base.Tokens.Select(static token => token.Descriptor.Name).ShouldBe(["ColorPrimary", "BorderRadius"]);
        definition.Base.Tokens[0].Value.ShouldBe(Color.Parse("#722ED1"));
        definition.Base.Tokens[1].Value.ShouldBe(new CornerRadius(4));
        var button = definition.Base.Controls.ShouldHaveSingleItem();
        button.Appearance.ShouldBe(ThemeAppearance.Light);
        button.Density.ShouldBeNull();
        button.OwnTokens.ShouldHaveSingleItem().Value.ShouldBe(40d);
        button.GlobalTokens.ShouldHaveSingleItem().Descriptor.Name.ShouldBe(nameof(DesignToken.LineWidth));
        definition.Conditions.Count.ShouldBe(2);
        definition.Conditions[0].Appearance.ShouldBe(ThemeAppearance.Dark);
        definition.Conditions[1].Density.ShouldBe(DesignDensity.Compact);
        definition.Conditions[1].Idiom.ShouldBe(DesignIdiom.Desktop);
        definition.ControlIdentities.ShouldBe([CompilerButtonToken.ControlIdentity]);
    }

    [Fact]
    public void ValuesFor_Layers_The_Base_And_The_Matching_Conditions_In_Document_Order()
    {
        var definition = BindSuccess($$"""
            <Theme xmlns="{{Namespace}}" Id="T" Name="Theme" Language="Binder">
              <Tokens><Token Name="BorderRadius" Value="4" /></Tokens>
              <When Appearance="Dark"><Tokens><Token Name="BorderRadius" Value="5" /></Tokens></When>
              <When Density="Compact"><Tokens><Token Name="BorderRadius" Value="6" /></Tokens></When>
            </Theme>
            """);
        var style = new DesignStyle("Default");

        Radius(definition, new DesignVariant(style, ThemeAppearance.Light, DesignDensity.Default, DesignIdiom.Desktop)).ShouldBe(4);
        Radius(definition, new DesignVariant(style, ThemeAppearance.Dark, DesignDensity.Default, DesignIdiom.Desktop)).ShouldBe(5);
        Radius(definition, new DesignVariant(style, ThemeAppearance.Dark, DesignDensity.Compact, DesignIdiom.Desktop)).ShouldBe(6);

        static double Radius(PreparedThemeDefinition definition, DesignVariant variant) =>
            ((CornerRadius)definition.ValuesFor(variant).GlobalTokens
                                     .Last(static token => token.Descriptor.Name == nameof(DesignToken.BorderRadius)).Value!)
            .TopLeft;
    }

    [Theory]
    [InlineData("Language=\"Missing\"", "", "ATMTHM2001")]
    [InlineData("Language=\"Binder\"", "<Controls><Control Catalog='AtomUI' Id='Unknown'><Tokens><Token Name='LineWidth' Value='1'/></Tokens></Control></Controls>", "ATMTHM2002")]
    [InlineData("Language=\"Binder\"", "<Tokens><Token Name='Unknown' Value='1'/></Tokens>", "ATMTHM2003")]
    [InlineData("Language=\"Binder\"", "<Controls><Control Catalog='AtomUI' Id='Button'><Tokens><Token Name='NotRegistered' Value='1'/></Tokens></Control></Controls>", "ATMTHM2003")]
    [InlineData("Language=\"Binder\"", "<LanguageTokens><Token Name='Radius' Value='1'/></LanguageTokens>", "ATMTHM2004")]
    [InlineData("Language=\"Binder\"", "<Controls><Control Catalog='AtomUI' Id='Button'/></Controls>", "ATMTHM3001")]
    [InlineData("Language=\"Binder\"", "<When Appearance='Dark'><Controls><Control Catalog='AtomUI' Id='Button' Density='Compact'><Tokens><Token Name='Height' Value='1'/></Tokens></Control></Controls></When>", "ATMTHM3001")]
    [InlineData("Language=\"Binder\"", "<Tokens><Token Name='BorderRadius' Value='not-a-radius'/></Tokens>", "ATMTHM3002")]
    [InlineData("Language=\"Binder\" Style=\"Square\"", "", "ATMTHM3003")]
    [InlineData("Language=\"Binder\" Density=\"Spacious\"", "", "ATMTHM3003")]
    [InlineData("Language=\"Binder\"", "<When Idiom='Watch'><Tokens><Token Name='LineWidth' Value='1'/></Tokens></When>", "ATMTHM3003")]
    [InlineData("Language=\"Binder\"", "<Tokens><Token Name='BorderRadius' Value='4' Locked='true'/></Tokens>", "ATMTHM3004")]
    [InlineData("Language=\"Binder\"", "<When Appearance='Dark'><Tokens><Token Name='ColorPrimary' Value='#000000' Locked='true'/></Tokens></When>", "ATMTHM3004")]
    [InlineData("Language=\"Binder\"", "<Controls><Control Catalog='AtomUI' Id='Button'><Tokens><Token Name='Height' Value='1' Locked='true'/></Tokens></Control></Controls>", "ATMTHM3004")]
    [InlineData("Language=\"Binder\"", "<When><Tokens><Token Name='LineWidth' Value='1'/></Tokens></When>", "ATMTHM3005")]
    [InlineData("Language=\"Binder\"", "<When Appearance='Dark'/>", "ATMTHM3005")]
    public void Bind_Rejects_Invalid_Definitions_With_A_Located_Diagnostic(string attributes, string body, string code)
    {
        var document = Read($$"""<Theme xmlns="{{Namespace}}" Id="T" Name="Theme" {{attributes}}>{{body}}</Theme>""");

        var result = ThemeDefinitionBinder.Bind(document, CreateRegistry());

        result.Definition.ShouldBeNull();
        var diagnostic = result.Diagnostics.First(diagnostic => diagnostic.Code == code);
        diagnostic.Severity.ShouldBe(ThemeDefinitionDiagnosticSeverity.Error);
        diagnostic.FilePath.ShouldBe(Source);
        diagnostic.Line.ShouldBeGreaterThan(0);
        diagnostic.Path.ShouldStartWith("/Theme");
    }

    [Theory]
    [InlineData("Appearance=\"Dark\"", "<When Appearance='Dark'><Tokens><Token Name='LineWidth' Value='1'/></Tokens></When><When Appearance='Dark'><Tokens><Token Name='LineWidth' Value='2'/></Tokens></When>", "ATMTHM3008")]
    [InlineData("Appearance=\"Dark\"", "<When Appearance='Light'><Tokens><Token Name='LineWidth' Value='1'/></Tokens></When>", "ATMTHM3007")]
    [InlineData("", "<Tokens><Token Name='ColorPrimaryHover' Value='#000000'/></Tokens>", "ATMTHM3006")]
    public void Bind_Warns_But_Keeps_The_Definition(string attributes, string body, string code)
    {
        var document = Read($$"""<Theme xmlns="{{Namespace}}" Id="T" Name="Theme" Language="Binder" {{attributes}}>{{body}}</Theme>""");

        var result = ThemeDefinitionBinder.Bind(document, CreateRegistry());

        result.Definition.ShouldNotBeNull();
        result.Diagnostics.ShouldContain(diagnostic =>
            diagnostic.Code == code && diagnostic.Severity == ThemeDefinitionDiagnosticSeverity.Warning);
    }

    [Fact]
    public void A_Locked_Seed_Silences_The_Derived_Token_Warning()
    {
        var result = ThemeDefinitionBinder.Bind(Read($$"""
            <Theme xmlns="{{Namespace}}" Id="T" Name="Theme" Language="Binder">
              <Tokens>
                <Token Name="ColorPrimary" Value="#722ED1" Locked="true" />
                <Token Name="ColorPrimaryHover" Value="#000000" />
              </Tokens>
            </Theme>
            """), CreateRegistry());

        result.Definition.ShouldNotBeNull();
        result.Diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public void A_Global_Only_Registry_Binds_The_Values_Without_The_Controls()
    {
        var registry = ThemeSchemaRegistry.CreateGlobalOnly([BinderLanguage.Descriptor]);

        var result = ThemeDefinitionBinder.Bind(Read($$"""
            <Theme xmlns="{{Namespace}}" Id="T" Name="Theme" Language="Binder">
              <Tokens><Token Name="BorderRadius" Value="4" /></Tokens>
              <Controls><Control Catalog="AtomUI" Id="Button"><Tokens><Token Name="Height" Value="1" /></Tokens></Control></Controls>
            </Theme>
            """), registry);

        result.Diagnostics.ShouldBeEmpty();
        var definition = result.Definition.ShouldNotBeNull();
        definition.Base.Tokens.ShouldHaveSingleItem();
        definition.Base.Controls.ShouldBeEmpty();
    }

    private static PreparedThemeDefinition BindSuccess(string xml)
    {
        var result = ThemeDefinitionBinder.Bind(Read(xml), CreateRegistry());
        result.Diagnostics.ShouldBeEmpty(result.DiagnosticsText());
        return result.Definition.ShouldNotBeNull();
    }

    private static ThemeSchemaRegistry CreateRegistry() =>
        new(GeneratedThemeSchema.GetGlobalTokens(), [ThemeCompilerTests.CreateCompilerButtonDescriptor()],
            [BinderLanguage.Descriptor], null, TestControlTokenRules.All);

    private static ThemeDocument Read(string xml)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        var result = ThemeDocumentReader.Read(stream, Source);
        result.Success.ShouldBeTrue(string.Join(Environment.NewLine, result.Diagnostics.Select(static d => d.Message)));
        return result.Document!;
    }

    /// <summary>
    /// A language built on Ant Design's token with two styles, two densities, the adjustable <c>ColorPrimary</c> seed and
    /// its derived <c>ColorPrimaryHover</c>.
    /// </summary>
    private sealed class BinderLanguage : DesignLanguage<AntDesignToken>
    {
        internal static DesignLanguageDescriptor Descriptor { get; } = new(new BinderLanguage(), Array.Empty<TokenDescriptor>());

        public override string Id => "Binder";

        public override DesignLanguageAxes Axes { get; } = new(
            [new DesignStyle("Default"), new DesignStyle("Rounded")], new DesignStyle("Default"),
            [DesignDensity.Default, DesignDensity.Compact], [DesignIdiom.Desktop],
            [GlobalTokenSchema.Get(nameof(DesignToken.ColorPrimary))])
        {
            SeedDerivations = new Dictionary<string, IReadOnlySet<string>>
            {
                [nameof(DesignToken.ColorPrimary)] = new HashSet<string> { nameof(DesignToken.ColorPrimaryHover) }
            }
        };

        public override void DefineSeeds(in DesignVariant variant, DesignToken seed, AntDesignToken languageSeed)
        {
        }

        public override void DeriveLanguage(in DesignVariant variant, DesignToken seed, AntDesignToken language)
        {
        }

        public override void DeriveGlobal(in TokenEvaluationContext<AntDesignToken> context, DesignToken token)
        {
        }
    }
}

internal static class ThemeDefinitionBindResultTestExtensions
{
    internal static string DiagnosticsText(this ThemeDefinitionBindResult result)
    {
        return string.Join(Environment.NewLine, result.Diagnostics.Select(static diagnostic => diagnostic.Message));
    }
}
