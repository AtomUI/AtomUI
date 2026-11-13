using System.Globalization;
using System.Text;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Definitions;
using AtomUI.Theme.Schema;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

public class ThemeDefinitionBinderTests
{
    private const string Namespace = "https://atomui.net/schemas/theme/v1";
    private const string Source = "themes/test.theme.xml";

    [Fact]
    public void Bind_Produces_Typed_Immutable_Definition_And_Classifies_Control_Tokens()
    {
        var registry = CreateRegistry();
        var document = Read($$"""
                             <Theme xmlns="{{Namespace}}"
                                    Id="T"
                                    Name="Theme"
                                    Appearance="Dark"
                                    IsDefault="true">
                               <Algorithms>
                                 <Algorithm Id="Default" />
                                 <Algorithm Id="Dark" />
                               </Algorithms>
                               <Tokens>
                                 <Token Name="ColorPrimary" Value="12.5" />
                               </Tokens>
                               <Controls>
                                 <Control Catalog="AtomUI" Id="Button" Algorithm="Global">
                                   <Tokens>
                                     <Token Name="ColorPrimary" Value="20.5" />
                                     <Token Name="ContentFontSize" Value="14" />
                                   </Tokens>
                                 </Control>
                               </Controls>
                             </Theme>
                             """);

        var result = ThemeDefinitionBinder.Bind(document, registry);

        result.Success.ShouldBeTrue(result.DiagnosticsText());
        var definition = result.Definition!;
        definition.Id.ShouldBe("T");
        definition.Name.ShouldBe("Theme");
        definition.DeclaredAppearance.ShouldBe(ThemeAppearance.Dark);
        definition.EffectiveAppearance.ShouldBe(ThemeAppearance.Dark);
        definition.IsDefault.ShouldBeTrue();
        definition.Algorithms.Select(static algorithm => algorithm.Algorithm)
                  .ShouldBe([ThemeAlgorithm.Default, ThemeAlgorithm.Dark]);
        definition.Tokens.ShouldHaveSingleItem().Value.ShouldBe(12.5);

        var button = definition.Controls.ShouldHaveSingleItem();
        button.Descriptor.Identity.ShouldBe(new ControlTokenIdentity("AtomUI", "Button"));
        button.AlgorithmMode.ShouldBe(ControlAlgorithmMode.Global);
        button.GlobalTokens.ShouldHaveSingleItem().Value.ShouldBe(20.5);
        button.OwnTokens.ShouldHaveSingleItem().Descriptor.Name.ShouldBe("ContentFontSize");
        button.OwnTokens[0].Value.ShouldBe(14);
        Should.Throw<NotSupportedException>(() =>
            ((IList<BoundTokenValue>)definition.Tokens).Add(definition.Tokens[0]));
    }

    [Fact]
    public void Bind_Uses_Invariant_Descriptor_Parsing()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            var document = Read($$"""
                                 <Theme xmlns="{{Namespace}}" Id="T" Name="T" Appearance="Light">
                                   <Algorithms><Algorithm Id="Default" /></Algorithms>
                                   <Tokens><Token Name="ColorPrimary" Value="1.5" /></Tokens>
                                 </Theme>
                                 """);

            var result = ThemeDefinitionBinder.Bind(document, CreateRegistry());

            result.Success.ShouldBeTrue(result.DiagnosticsText());
            result.Definition!.Tokens.ShouldHaveSingleItem().Value.ShouldBe(1.5);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void Bind_Maps_Custom_Control_Algorithms()
    {
        var document = Read($$"""
                             <Theme xmlns="{{Namespace}}" Id="T" Name="T" Appearance="Light">
                               <Algorithms><Algorithm Id="Default" /></Algorithms>
                               <Controls>
                                 <Control Catalog="AtomUI" Id="Button">
                                   <Algorithms>
                                     <Algorithm Id="Default" />
                                     <Algorithm Id="Compact" />
                                   </Algorithms>
                                 </Control>
                               </Controls>
                             </Theme>
                             """);

        var result = ThemeDefinitionBinder.Bind(document, CreateRegistry());

        result.Success.ShouldBeTrue(result.DiagnosticsText());
        var button = result.Definition!.Controls.ShouldHaveSingleItem();
        button.AlgorithmMode.ShouldBe(ControlAlgorithmMode.Custom);
        button.Algorithms.Select(static algorithm => algorithm.Algorithm)
              .ShouldBe([ThemeAlgorithm.Default, ThemeAlgorithm.Compact]);
    }

    [Theory]
    [InlineData("<Algorithm Id='Compact'/>", "", "ATMTHM2001")]
    [InlineData("<Algorithm Id='Default'/>", "<Controls><Control Catalog='AtomUI' Id='Unknown' Algorithm='Disabled'/></Controls>", "ATMTHM2002")]
    [InlineData("<Algorithm Id='Default'/>", "<Tokens><Token Name='Unknown' Value='1'/></Tokens>", "ATMTHM2003")]
    [InlineData("<Algorithm Id='Default'/>", "<Controls><Control Catalog='AtomUI' Id='Button'><Tokens><Token Name='NotRegistered' Value='1'/></Tokens></Control></Controls>", "ATMTHM2003")]
    public void Bind_Rejects_Unknown_Registry_Identities(
        string algorithms,
        string body,
        string expectedCode)
    {
        var document = Read($$"""
                             <Theme xmlns="{{Namespace}}" Id="T" Name="T" Appearance="Light">
                               <Algorithms>{{algorithms}}</Algorithms>
                               {{body}}
                             </Theme>
                             """);

        var result = ThemeDefinitionBinder.Bind(document, CreateRegistry(includeCompact: expectedCode != "ATMTHM2001"));

        result.Success.ShouldBeFalse();
        result.Definition.ShouldBeNull();
        result.Diagnostics.ShouldContain(diagnostic => diagnostic.Code == expectedCode);
    }

    [Fact]
    public void Bind_Reports_An_Unregistered_Algorithm_With_Catalog_Id_And_Location()
    {
        var document = Read($$"""
                             <Theme xmlns="{{Namespace}}" Id="T" Name="T" Appearance="Light">
                               <Algorithms>
                                 <Algorithm Id="Default" />
                                 <Algorithm Catalog="PrimeNG" Id="Aura" />
                               </Algorithms>
                             </Theme>
                             """);

        var result = ThemeDefinitionBinder.Bind(document, CreateRegistry());

        result.Success.ShouldBeFalse();
        result.Definition.ShouldBeNull();
        var diagnostic = result.Diagnostics.ShouldHaveSingleItem();
        diagnostic.Code.ShouldBe("ATMTHM2001");
        diagnostic.Message.ShouldContain("PrimeNG:Aura");
        var location = document.Algorithms[1].Location;
        location.Line.ShouldBe(4);
        diagnostic.Line.ShouldBe(location.Line);
        diagnostic.Column.ShouldBe(location.Column);
        diagnostic.Path.ShouldBe(location.Path);
    }

    [Fact]
    public void Bind_Rejects_Algorithm_Attribute_With_A_Custom_Chain()
    {
        var document = Read($$"""
                             <Theme xmlns="{{Namespace}}" Id="T" Name="T" Appearance="Light">
                               <Algorithms><Algorithm Id="Default" /></Algorithms>
                               <Controls>
                                 <Control Catalog="AtomUI" Id="Button" Algorithm="Global">
                                   <Algorithms><Algorithm Id="Compact" /></Algorithms>
                                 </Control>
                               </Controls>
                             </Theme>
                             """);

        var result = ThemeDefinitionBinder.Bind(document, CreateRegistry());

        var diagnostic = result.Diagnostics.ShouldHaveSingleItem();
        result.Success.ShouldBeFalse();
        diagnostic.Code.ShouldBe("ATMTHM3001");
        diagnostic.Path.ShouldBe("/Theme/Controls/Control[@Catalog='AtomUI'][@Id='Button']");
    }

    [Fact]
    public void Bind_Rejects_Declared_Appearance_That_Differs_From_Algorithm_Result()
    {
        var document = Read($$"""
                             <Theme xmlns="{{Namespace}}" Id="T" Name="T" Appearance="Light">
                               <Algorithms>
                                 <Algorithm Id="Default" />
                                 <Algorithm Id="Dark" />
                               </Algorithms>
                             </Theme>
                             """);

        var result = ThemeDefinitionBinder.Bind(document, CreateRegistry());

        result.Success.ShouldBeFalse();
        result.Definition.ShouldBeNull();
        result.Diagnostics.ShouldHaveSingleItem().Code.ShouldBe("ATMTHM3003");
    }

    [Fact]
    public void Bind_Returns_A_Stable_Diagnostic_When_Descriptor_Conversion_Fails()
    {
        var document = Read($$"""
                             <Theme xmlns="{{Namespace}}" Id="T" Name="T" Appearance="Light">
                               <Algorithms><Algorithm Id="Default" /></Algorithms>
                               <Tokens><Token Name="ColorPrimary" Value="not-a-number" /></Tokens>
                             </Theme>
                             """);

        var result = ThemeDefinitionBinder.Bind(document, CreateRegistry());

        var diagnostic = result.Diagnostics.ShouldHaveSingleItem();
        result.Success.ShouldBeFalse();
        result.Definition.ShouldBeNull();
        diagnostic.Code.ShouldBe("ATMTHM3002");
        diagnostic.Line.ShouldBe(3);
        diagnostic.Path.ShouldBe("/Theme/Tokens/Token[@Name='ColorPrimary']");
        diagnostic.Message.ShouldBe("Token 'ColorPrimary' cannot be converted to 'Double'.");
    }

    private static ThemeSchemaRegistry CreateRegistry(bool includeCompact = true)
    {
        var colorPrimary = Token(
            "ColorPrimary",
            0,
            TokenStage.Seed,
            typeof(double),
            static value => double.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture));
        var borderRadius = Token(
            "BorderRadius",
            1,
            TokenStage.Map,
            typeof(int),
            static value => int.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture));
        var ownTokens = new List<TokenDescriptor>
        {
            Token(
            "ContentFontSize",
            0,
            TokenStage.Control,
            typeof(int),
            static value => int.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture))
        };

        var algorithms = new List<ThemeAlgorithmDescriptor>
        {
            Algorithm(ThemeAlgorithm.Dark, ThemeAppearanceEffect.Dark),
            Algorithm(ThemeAlgorithm.Default, ThemeAppearanceEffect.Preserve)
        };
        if (includeCompact)
        {
            algorithms.Add(Algorithm(ThemeAlgorithm.Compact, ThemeAppearanceEffect.Preserve));
        }

        return new ThemeSchemaRegistry(
            [colorPrimary, borderRadius],
            [new ControlTokenDescriptor(
                ThemeTestControlTypes.For("AtomUI", "Button"),
                new ControlTokenIdentity("AtomUI", "Button"),
                ownTokens,
                static () => throw new InvalidOperationException("The Binder must not create Token builders."),
                static (_, _) => throw new InvalidOperationException("The Binder must not evaluate Control Tokens."))],
            algorithms);
    }

    private static TokenDescriptor Token(
        string name,
        int slot,
        TokenStage stage,
        Type valueType,
        Func<string, object?> parser)
    {
        return new TokenDescriptor(
            name,
            slot,
            stage,
            valueType,
            name,
            parser,
            static value => ThemeTokenValueFormatter.Format(value),
            static _ => throw new InvalidOperationException("The Binder must not read Token builders."),
            static (_, _) => throw new InvalidOperationException("The Binder must not write Token builders."),
            static _ => throw new InvalidOperationException("The Binder must not project resources."));
    }

    private static ThemeAlgorithmDescriptor Algorithm(ThemeAlgorithm algorithm, ThemeAppearanceEffect effect)
    {
        return new ThemeAlgorithmDescriptor(
            algorithm,
            revision: 1,
            effect,
            static () => throw new InvalidOperationException("The Binder must not create algorithms."));
    }

    private static ThemeDocument Read(string xml)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        var result = ThemeDocumentReader.Read(stream, Source);
        result.Success.ShouldBeTrue(result.DiagnosticsText());
        return result.Document!;
    }
}

internal static class ThemeDefinitionBindResultTestExtensions
{
    internal static string DiagnosticsText(this ThemeDefinitionBindResult result)
    {
        return string.Join(Environment.NewLine, result.Diagnostics.Select(static diagnostic => diagnostic.Message));
    }
}
