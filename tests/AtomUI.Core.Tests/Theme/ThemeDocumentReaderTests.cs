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
    [InlineData("dark")]
    [InlineData("Unknown")]
    [InlineData("1")]
    [InlineData("")]
    public void Read_Rejects_Non_Canonical_Algorithm_Values_With_Source_Position(string algorithm)
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
    public void Xsd_Algorithm_Values_Match_The_ThemeAlgorithm_Enum()
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
