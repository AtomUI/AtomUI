using System.Text;
using System.Xml.Linq;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Definitions;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

public class ThemeDocumentReaderTests
{
    private const string Namespace = "https://atomui.net/schemas/theme/v1";
    private const string Source = "themes/test.theme.xml";

    [Fact]
    public void Read_Maps_Design_Language_Metadata()
    {
        var result = Read($$"""
                            <Theme xmlns="{{Namespace}}" Id="A.Dark" Name="A" Appearance="Dark"
                                   Density="Compact" DesignLanguage="PrimeNG.Aura" BrandColor="Seed">
                              <Algorithms><Algorithm Id="Default" /></Algorithms>
                            </Theme>
                            """);

        result.Success.ShouldBeTrue(result.DiagnosticsText());
        result.Document!.DesignLanguage.ShouldBe("PrimeNG.Aura");
        result.Document.Density.ShouldBe(ThemeDensity.Compact);
        result.Document.BrandColor.ShouldBe(ThemeBrandColorSupport.Seed);
    }

    [Fact]
    public void Read_Defaults_Missing_Metadata_To_Null_Normal_Fixed()
    {
        var result = Read($$"""
                            <Theme xmlns="{{Namespace}}" Id="A" Name="A" Appearance="Light">
                              <Algorithms><Algorithm Id="Default" /></Algorithms>
                            </Theme>
                            """);

        result.Success.ShouldBeTrue(result.DiagnosticsText());
        result.Document!.DesignLanguage.ShouldBeNull();
        result.Document.Density.ShouldBe(ThemeDensity.Normal);
        result.Document.BrandColor.ShouldBe(ThemeBrandColorSupport.Fixed);
    }

    [Theory]
    [InlineData("DesignLanguage=\"a b\"")]
    [InlineData("Density=\"Tight\"")]
    [InlineData("BrandColor=\"Auto\"")]
    public void Read_Rejects_Invalid_Design_Language_Metadata(string attribute)
    {
        var result = Read($$"""
                            <Theme xmlns="{{Namespace}}" Id="A" Name="A" Appearance="Light" {{attribute}}>
                              <Algorithms><Algorithm Id="Default" /></Algorithms>
                            </Theme>
                            """);

        result.Success.ShouldBeFalse();
        result.Diagnostics.ShouldContain(static item => item.Code == "ATMTHM1002");
    }

    [Fact]
    public void ThemeInfo_Equality_Compares_Algorithms_By_Sequence()
    {
        var left = new ThemeInfo("A", "A", ThemeAppearance.Light, false)
        {
            Algorithms = [ThemeAlgorithm.Default, ThemeAlgorithm.Compact]
        };
        var same = new ThemeInfo("A", "A", ThemeAppearance.Light, false)
        {
            Algorithms = new List<ThemeAlgorithm> { ThemeAlgorithm.Default, ThemeAlgorithm.Compact }
        };
        var different = left with { Algorithms = [ThemeAlgorithm.Default] };

        left.ShouldBe(same);
        left.GetHashCode().ShouldBe(same.GetHashCode());
        left.ShouldNotBe(different);
    }

    [Fact]
    public void Read_Maps_Control_Algorithm_Forms_To_The_Four_State_Enum()
    {
        var result = Read($$"""
                            <Theme xmlns="{{Namespace}}" Id="T" Name="T" Appearance="Light">
                              <Algorithms>
                                <Algorithm Id="Default" />
                              </Algorithms>
                              <Controls>
                                <Control Catalog="AtomUI" Id="Unspecified">
                                  <Tokens><Token Name="X" Value="1" /></Tokens>
                                </Control>
                                <Control Catalog="AtomUI" Id="Disabled" Algorithm="Disabled" />
                                <Control Catalog="AtomUI" Id="Global" Algorithm="Global" />
                                <Control Catalog="AtomUI" Id="Custom">
                                  <Algorithms><Algorithm Id="Compact" /></Algorithms>
                                </Control>
                              </Controls>
                            </Theme>
                            """);

        result.Success.ShouldBeTrue(result.DiagnosticsText());
        result.Document!.Controls.Select(static control => control.AlgorithmMode).ShouldBe([
            ControlAlgorithmMode.Unspecified,
            ControlAlgorithmMode.Disabled,
            ControlAlgorithmMode.Global,
            ControlAlgorithmMode.Custom
        ]);
        result.Document.Controls[3].Algorithms.Select(static algorithm => algorithm.Algorithm)
              .ShouldBe([ThemeAlgorithm.Compact]);
    }

    [Fact]
    public void Read_Maps_Exact_Algorithm_Names_To_Enum_Values()
    {
        var result = Read($$"""
                            <Theme xmlns="{{Namespace}}" Id="T" Name="T" Appearance="Dark">
                              <Algorithms>
                                <Algorithm Id="Default" />
                                <Algorithm Id="Compact" />
                                <Algorithm Id="Dark" />
                              </Algorithms>
                            </Theme>
                            """);

        result.Success.ShouldBeTrue(result.DiagnosticsText());
        result.Document!.Algorithms.Select(static algorithm => algorithm.Algorithm).ShouldBe([
            ThemeAlgorithm.Default,
            ThemeAlgorithm.Compact,
            ThemeAlgorithm.Dark
        ]);
    }

    [Theory]
    [InlineData("1")]
    [InlineData("")]
    [InlineData("a b")]
    public void Read_Rejects_Non_Identifier_Algorithm_Values_With_Source_Position(string algorithm)
    {
        var result = Read($$"""
                            <Theme xmlns="{{Namespace}}" Id="T" Name="T" Appearance="Light">
                              <Algorithms><Algorithm Id="{{algorithm}}" /></Algorithms>
                            </Theme>
                            """);

        result.Success.ShouldBeFalse();
        result.Document.ShouldBeNull();
        var diagnostic = result.Diagnostics.First(static item => item.Code == "ATMTHM1002");
        diagnostic.Line.ShouldBeGreaterThan(0);
        diagnostic.Column.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Read_Maps_Catalog_And_Id_To_Algorithm_Identity()
    {
        var result = Read($$"""
                            <Theme xmlns="{{Namespace}}" Id="T" Name="T" Appearance="Dark">
                              <Algorithms>
                                <Algorithm Catalog="PrimeNG" Id="Aura.Dark" />
                              </Algorithms>
                              <Controls>
                                <Control Catalog="AtomUI" Id="Button">
                                  <Algorithms><Algorithm Catalog="PrimeNG" Id="Aura.Dark" /></Algorithms>
                                </Control>
                              </Controls>
                            </Theme>
                            """);

        result.Success.ShouldBeTrue(result.DiagnosticsText());
        result.Document!.Algorithms.Select(static algorithm => algorithm.Algorithm)
              .ShouldBe([new ThemeAlgorithm("PrimeNG", "Aura.Dark")]);
        result.Document.Algorithms[0].Location.Path.ShouldBe(
            "/Theme/Algorithms/Algorithm[@Catalog='PrimeNG'][@Id='Aura.Dark']");
        result.Document.Controls[0].Algorithms.Select(static algorithm => algorithm.Algorithm)
              .ShouldBe([new ThemeAlgorithm("PrimeNG", "Aura.Dark")]);
    }

    [Fact]
    public void Read_Defaults_Omitted_Catalog_To_AtomUI()
    {
        var result = Read($$"""
                            <Theme xmlns="{{Namespace}}" Id="T" Name="T" Appearance="Light">
                              <Algorithms>
                                <Algorithm Id="Default" />
                                <Algorithm Catalog="AtomUI" Id="Compact" />
                                <Algorithm Id="Custom" />
                              </Algorithms>
                            </Theme>
                            """);

        result.Success.ShouldBeTrue(result.DiagnosticsText());
        result.Document!.Algorithms.Select(static algorithm => algorithm.Algorithm).ShouldBe([
            ThemeAlgorithm.Default,
            ThemeAlgorithm.Compact,
            new ThemeAlgorithm("AtomUI", "Custom")
        ]);
        result.Document.Algorithms[0].Location.Path.ShouldBe("/Theme/Algorithms/Algorithm[@Id='Default']");
    }

    [Theory]
    [InlineData("<Algorithms><Algorithm Catalog='AtomUI' Id='Dark'/><Algorithm Id='Dark'/></Algorithms>", "")]
    [InlineData("<Algorithms><Algorithm Id='Default'/></Algorithms>",
                "<Controls><Control Catalog='AtomUI' Id='Button'><Algorithms><Algorithm Catalog='AtomUI' Id='Dark'/><Algorithm Id='Dark'/></Algorithms></Control></Controls>")]
    public void Read_Rejects_Duplicate_When_One_Entry_Omits_The_Catalog(string algorithms, string body)
    {
        var result = Read($$"""
                            <Theme xmlns="{{Namespace}}" Id="T" Name="T" Appearance="Light">
                              {{algorithms}}
                              {{body}}
                            </Theme>
                            """);

        result.Success.ShouldBeFalse();
        result.Document.ShouldBeNull();
        var diagnostic = result.Diagnostics.ShouldHaveSingleItem();
        diagnostic.Code.ShouldBe("ATMTHM1002");
        diagnostic.Message.ShouldBe("Theme algorithm 'AtomUI:Dark' is duplicated.");
        diagnostic.Line.ShouldBeGreaterThan(0);
        diagnostic.Column.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Read_Accepts_Same_Id_Under_Different_Catalogs()
    {
        var result = Read($$"""
                            <Theme xmlns="{{Namespace}}" Id="T" Name="T" Appearance="Dark">
                              <Algorithms>
                                <Algorithm Id="Dark" />
                                <Algorithm Catalog="PrimeNG" Id="Dark" />
                              </Algorithms>
                              <Controls>
                                <Control Catalog="AtomUI" Id="Button">
                                  <Algorithms>
                                    <Algorithm Id="Dark" />
                                    <Algorithm Catalog="PrimeNG" Id="Dark" />
                                  </Algorithms>
                                </Control>
                              </Controls>
                            </Theme>
                            """);

        result.Success.ShouldBeTrue(result.DiagnosticsText());
        ThemeAlgorithm[] expected = [ThemeAlgorithm.Dark, new ThemeAlgorithm("PrimeNG", "Dark")];
        result.Document!.Algorithms.Select(static algorithm => algorithm.Algorithm).ShouldBe(expected);
        result.Document.Controls[0].Algorithms.Select(static algorithm => algorithm.Algorithm).ShouldBe(expected);
    }

    [Theory]
    [InlineData("Id='1Bad'")]
    [InlineData("Catalog='1Bad' Id='Dark'")]
    public void Read_Rejects_An_Invalid_Identifier_At_The_Reader_Boundary(string attributes)
    {
        var result = Read($$"""
                            <Theme xmlns="{{Namespace}}" Id="T" Name="T" Appearance="Light">
                              <Algorithms>
                                <Algorithm {{attributes}} />
                              </Algorithms>
                            </Theme>
                            """);

        result.Success.ShouldBeFalse();
        result.Document.ShouldBeNull();
        var diagnostic = result.Diagnostics.First(static item => item.Code == "ATMTHM1002");
        diagnostic.Line.ShouldBe(3);
        diagnostic.Column.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Xsd_Still_Offers_The_Three_BuiltIn_Ids()
    {
        const string resourceName = "AtomUI.Theme.Definitions.Schemas.atomui-theme-v1.xsd";
        using var stream = typeof(ThemeConfig).Assembly.GetManifestResourceStream(resourceName);
        stream.ShouldNotBeNull();
        var schema = XDocument.Load(stream!);
        XNamespace xs = "http://www.w3.org/2001/XMLSchema";

        var values = schema.Descendants(xs + "simpleType")
                           .Single(element => (string?)element.Attribute("name") == "ThemeAlgorithmType")
                           .Descendants(xs + "enumeration")
                           .Select(element => (string?)element.Attribute("value"))
                           .ToArray();
        values.ShouldBe([ThemeAlgorithm.Default.Id, ThemeAlgorithm.Dark.Id, ThemeAlgorithm.Compact.Id]);

        var algorithmType = schema.Descendants(xs + "complexType")
                                  .Single(element => (string?)element.Attribute("name") == "AlgorithmType");
        var idType = algorithmType.Elements(xs + "attribute")
                                  .Single(element => (string?)element.Attribute("name") == "Id")
                                  .Attribute("type")!.Value;
        var union = schema.Descendants(xs + "simpleType")
                          .Single(element => (string?)element.Attribute("name") == idType.Split(':')[1])
                          .Element(xs + "union");
        union.ShouldNotBeNull();
        union!.Attribute("memberTypes")!.Value.Split(' ')
              .ShouldBe(["theme:ThemeAlgorithmType", "theme:IdentifierType"]);
    }

    [Fact]
    public void Read_Produces_Immutable_Syntax_With_Source_Locations()
    {
        var result = Read($$"""
                            <Theme xmlns="{{Namespace}}"
                                   Id="DaybreakBlue"
                                   Name="Daybreak Blue"
                                   Appearance="Light"
                                   IsDefault="true">
                              <Algorithms>
                                <Algorithm Id="Default" />
                              </Algorithms>
                              <Tokens>
                                <Token Name="ColorPrimary" Value="#1677FF" />
                              </Tokens>
                              <Controls>
                                <Control Catalog="AtomUI" Id="Button" Algorithm="Global">
                                  <Tokens>
                                    <Token Name="ContentFontSize" Value="14" />
                                  </Tokens>
                                </Control>
                              </Controls>
                            </Theme>
                            """);

        result.Success.ShouldBeTrue(result.DiagnosticsText());
        var document = result.Document!;
        document.Id.ShouldBe("DaybreakBlue");
        document.Name.ShouldBe("Daybreak Blue");
        document.Appearance.ShouldBe(ThemeAppearance.Light);
        document.IsDefault.ShouldBeTrue();
        document.Algorithms[0].Location.Line.ShouldBe(7);
        document.Tokens[0].Location.Line.ShouldBe(10);
        document.Controls[0].Identity.ToString().ShouldBe("AtomUI:Button");
        document.Controls[0].Tokens[0].Location.Path.ShouldBe(
            "/Theme/Controls/Control[@Catalog='AtomUI'][@Id='Button']/Tokens/Token[@Name='ContentFontSize']");
        Should.Throw<NotSupportedException>(() =>
            ((IList<ThemeTokenDocument>)document.Tokens).Add(document.Tokens[0]));
    }

    [Theory]
    [InlineData("<Theme Id='T' Name='T' Appearance='Light'><Algorithms><Algorithm Id='Default'/></Algorithms></Theme>")]
    [InlineData("<Theme xmlns='https://atomui.net/schemas/theme/v1' Id='T' Name='T' Appearance='Light'><SharedTokens /></Theme>")]
    [InlineData("<Theme xmlns='https://atomui.net/schemas/theme/v1' Id='T' Name='T' Appearance='Light'><Tokens><Token Name='X' Value='1'/></Tokens><Algorithms><Algorithm Id='Default'/></Algorithms></Theme>")]
    [InlineData("<Theme xmlns='https://atomui.net/schemas/theme/v1' Id='T' Name='T' Appearance='Light'><Algorithms><Algorithm Id='Default'/></Algorithms><Tokens /></Theme>")]
    [InlineData("<Theme xmlns='https://atomui.net/schemas/theme/v1' Id='T' Name='T' Appearance='Light' Unknown='x'><Algorithms><Algorithm Id='Default'/></Algorithms></Theme>")]
    public void Read_Rejects_Documents_Outside_The_V1_Structure(string xml)
    {
        var result = Read(xml);

        result.Success.ShouldBeFalse();
        result.Document.ShouldBeNull();
        result.Diagnostics.ShouldContain(static diagnostic => diagnostic.Code == "ATMTHM1002");
    }

    [Theory]
    [InlineData("<Tokens><Token Name='X' Value='1'/><Token Name='X' Value='2'/></Tokens>")]
    [InlineData("<Controls><Control Catalog='AtomUI' Id='Button' Algorithm='Disabled'/><Control Catalog='AtomUI' Id='Button' Algorithm='Global'/></Controls>")]
    [InlineData("<Controls><Control Catalog='AtomUI' Id='Button'><Tokens><Token Name='X' Value='1'/><Token Name='X' Value='2'/></Tokens></Control></Controls>")]
    public void Read_Enforces_Xsd_Identity_Constraints(string body)
    {
        var result = Read($$"""
                            <Theme xmlns="{{Namespace}}" Id="T" Name="T" Appearance="Light">
                              <Algorithms><Algorithm Id="Default" /></Algorithms>
                              {{body}}
                            </Theme>
                            """);

        result.Success.ShouldBeFalse();
        result.Diagnostics.ShouldContain(static diagnostic => diagnostic.Code == "ATMTHM1002");
    }

    [Fact]
    public void Read_Prohibits_Dtd_And_Leaves_The_Stream_Open()
    {
        using var stream = CreateStream($$"""
                                         <!DOCTYPE Theme [<!ENTITY value "Default">]>
                                         <Theme xmlns="{{Namespace}}" Id="T" Name="T" Appearance="Light">
                                           <Algorithms><Algorithm Id="&value;" /></Algorithms>
                                         </Theme>
                                         """);

        var result = ThemeDocumentReader.Read(stream, Source);

        result.Success.ShouldBeFalse();
        result.Diagnostics.ShouldContain(static diagnostic => diagnostic.Code == "ATMTHM1001");
        stream.CanRead.ShouldBeTrue();
    }

    [Fact]
    public void Read_Leaves_The_Stream_Open_On_Success()
    {
        using var stream = CreateStream(MinimalTheme());

        ThemeDocumentReader.Read(stream, Source).Success.ShouldBeTrue();

        stream.CanRead.ShouldBeTrue();
    }

    [Fact]
    public void Read_Rejects_A_Seekable_Stream_Over_The_Byte_Limit()
    {
        using var stream = CreateStream(MinimalTheme());
        var options = new ThemeDocumentReaderOptions
        {
            MaxDocumentBytes = stream.Length - 1
        };

        var result = ThemeDocumentReader.Read(stream, Source, options);

        result.Success.ShouldBeFalse();
        result.Diagnostics.ShouldHaveSingleItem().Code.ShouldBe("ATMTHM1003");
        stream.Position.ShouldBe(0);
    }

    [Fact]
    public void Read_Rejects_Documents_Over_The_Element_Limit()
    {
        using var stream = CreateStream(MinimalTheme());
        var options = new ThemeDocumentReaderOptions
        {
            MaxElements = 2
        };

        var result = ThemeDocumentReader.Read(stream, Source, options);

        result.Success.ShouldBeFalse();
        result.Diagnostics.ShouldHaveSingleItem().Code.ShouldBe("ATMTHM1003");
    }

    [Fact]
    public void Reader_Options_Cannot_Raise_The_Portable_Profile_Hard_Limits()
    {
        using var stream = CreateStream(MinimalTheme());

        Should.Throw<ArgumentOutOfRangeException>(() =>
            ThemeDocumentReader.Read(stream, Source, new ThemeDocumentReaderOptions
            {
                MaxDocumentBytes = ThemeDocumentReaderOptions.DefaultMaxDocumentBytes + 1
            }));
        Should.Throw<ArgumentOutOfRangeException>(() =>
            ThemeDocumentReader.Read(stream, Source, new ThemeDocumentReaderOptions
            {
                MaxElements = ThemeDocumentReaderOptions.DefaultMaxElements + 1
            }));
    }

    [Theory]
    [InlineData("ColorPrimaryHover")]
    [InlineData("ColorControlOutline")]
    public void Read_Rejects_Seed_Theme_That_Pins_A_Brand_Token(string token)
    {
        var result = Read($$"""
                            <Theme xmlns="{{Namespace}}" Id="A" Name="A" Appearance="Light" BrandColor="Seed">
                              <Algorithms><Algorithm Id="Default" /></Algorithms>
                              <Tokens><Token Name="{{token}}" Value="#FF0000" /></Tokens>
                            </Theme>
                            """);

        result.Success.ShouldBeFalse();
        var diagnostic = result.Diagnostics.ShouldHaveSingleItem();
        diagnostic.Code.ShouldBe("ATMTHM3101");
        diagnostic.Severity.ShouldBe(ThemeDefinitionDiagnosticSeverity.Error);
        diagnostic.Line.ShouldBeGreaterThan(0);
        diagnostic.Message.ShouldContain(token);
    }

    [Fact]
    public void Read_Rejects_Seed_Theme_With_Control_Level_ColorPrimaryBg()
    {
        var result = Read($$"""
                            <Theme xmlns="{{Namespace}}" Id="A" Name="A" Appearance="Light" BrandColor="Seed">
                              <Algorithms><Algorithm Id="Default" /></Algorithms>
                              <Controls>
                                <Control Catalog="AtomUI" Id="Button">
                                  <Tokens><Token Name="ColorPrimaryBg" Value="#FF0000" /></Tokens>
                                </Control>
                              </Controls>
                            </Theme>
                            """);

        result.Success.ShouldBeFalse();
        result.Diagnostics.ShouldContain(d => d.Code == "ATMTHM3101");
    }

    [Fact]
    public void Read_Accepts_Seed_Theme_With_Only_ColorPrimary_And_ColorLink()
    {
        var result = Read($$"""
                            <Theme xmlns="{{Namespace}}" Id="A" Name="A" Appearance="Light" BrandColor="Seed">
                              <Algorithms><Algorithm Id="Default" /></Algorithms>
                              <Tokens>
                                <Token Name="ColorPrimary" Value="#1677FF" />
                                <Token Name="ColorLink" Value="#1677FF" />
                              </Tokens>
                            </Theme>
                            """);

        result.Success.ShouldBeTrue(result.DiagnosticsText());
    }

    [Fact]
    public void NeutralColor_Attribute_Defaults_To_Fixed_And_Reads_Seed()
    {
        var defaults = Read($$"""
                             <Theme xmlns="{{Namespace}}" Id="A" Name="A" Appearance="Light">
                               <Algorithms><Algorithm Id="Default" /></Algorithms>
                             </Theme>
                             """);
        defaults.Success.ShouldBeTrue(defaults.DiagnosticsText());
        defaults.Document!.NeutralColor.ShouldBe(ThemeNeutralColorSupport.Fixed);

        var seed = Read($$"""
                          <Theme xmlns="{{Namespace}}" Id="A" Name="A" Appearance="Light" NeutralColor="Seed">
                            <Algorithms><Algorithm Id="Default" /></Algorithms>
                            <Tokens><Token Name="ColorNeutral" Value="#71717B" /></Tokens>
                          </Theme>
                          """);
        seed.Success.ShouldBeTrue(seed.DiagnosticsText());
        seed.Document!.NeutralColor.ShouldBe(ThemeNeutralColorSupport.Seed);
    }

    [Fact]
    public void Seed_Neutral_Definition_Pinning_A_Neutral_Token_Reports_ATMTHM3106()
    {
        var result = Read($$"""
                            <Theme xmlns="{{Namespace}}" Id="A" Name="A" Appearance="Light"
                                   NeutralColor="Seed">
                              <Algorithms><Algorithm Id="Default" /></Algorithms>
                              <Tokens>
                                <Token Name="ColorNeutral" Value="#71717B" />
                                <Token Name="ColorBorder" Value="#E4E4E7" />
                              </Tokens>
                              <Controls>
                                <Control Catalog="AtomUI" Id="Button">
                                  <Tokens><Token Name="ControlItemBgHover" Value="#F4F4F5" /></Tokens>
                                </Control>
                              </Controls>
                            </Theme>
                            """);

        result.Success.ShouldBeFalse();
        var pinned = result.Diagnostics.Where(d => d.Code == "ATMTHM3106").ToArray();
        pinned.Length.ShouldBe(2);
        pinned.ShouldAllBe(d => d.Severity == ThemeDefinitionDiagnosticSeverity.Error);
        pinned.ShouldContain(d => d.Message.Contains("ColorBorder"));
        pinned.ShouldContain(d => d.Message.Contains("ControlItemBgHover"));
    }

    [Fact]
    public void Seed_Neutral_Definition_Without_Seed_Reports_ATMTHM3107_Warning()
    {
        var result = Read($$"""
                            <Theme xmlns="{{Namespace}}" Id="A" Name="A" Appearance="Light"
                                   NeutralColor="Seed">
                              <Algorithms><Algorithm Id="Default" /></Algorithms>
                            </Theme>
                            """);

        // A Warning: the definition still loads, but a built-in chain cannot derive a neutral family.
        result.Success.ShouldBeTrue(result.DiagnosticsText());
        var warning = result.Diagnostics.ShouldHaveSingleItem();
        warning.Code.ShouldBe("ATMTHM3107");
        warning.Severity.ShouldBe(ThemeDefinitionDiagnosticSeverity.Warning);
    }

    [Fact]
    public void Seed_Neutral_Definition_With_Vendor_Chain_Without_Seed_Stays_Silent()
    {
        var result = Read($$"""
                            <Theme xmlns="{{Namespace}}" Id="A" Name="A" Appearance="Light"
                                   NeutralColor="Seed">
                              <Algorithms><Algorithm Catalog="Shadcn" Id="Nova" /></Algorithms>
                            </Theme>
                            """);

        result.Success.ShouldBeTrue(result.DiagnosticsText());
        result.Diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public void Read_Accepts_Fixed_Theme_That_Pins_Brand_Tokens()
    {
        var result = Read($$"""
                            <Theme xmlns="{{Namespace}}" Id="A" Name="A" Appearance="Light" BrandColor="Fixed">
                              <Algorithms><Algorithm Id="Default" /></Algorithms>
                              <Tokens><Token Name="ColorPrimaryHover" Value="#FF0000" /></Tokens>
                            </Theme>
                            """);

        result.Success.ShouldBeTrue(result.DiagnosticsText());
    }

    private static ThemeDocumentReadResult Read(string xml)
    {
        using var stream = CreateStream(xml);
        return ThemeDocumentReader.Read(stream, Source);
    }

    private static string MinimalTheme()
    {
        return $$"""
                 <Theme xmlns="{{Namespace}}" Id="T" Name="T" Appearance="Light">
                   <Algorithms><Algorithm Id="Default" /></Algorithms>
                 </Theme>
                 """;
    }

    private static MemoryStream CreateStream(string xml)
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(xml));
    }
}

internal static class ThemeDocumentReadResultTestExtensions
{
    internal static string DiagnosticsText(this ThemeDocumentReadResult result)
    {
        return string.Join(Environment.NewLine, result.Diagnostics.Select(static diagnostic => diagnostic.Message));
    }
}
