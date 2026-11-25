using System.Text;
using AtomUI.Theme;
using AtomUI.Theme.Definitions;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

/// <summary>The v2 theme document reader (spec 7.2): XSD-validated syntax with source locations, safe by construction.</summary>
public class ThemeDocumentReaderTests
{
    private const string Namespace = "https://atomui.net/schemas/theme/v2";
    private const string Source = "themes/test.theme.xml";

    [Fact]
    public void Read_Produces_Immutable_Syntax_With_Source_Locations()
    {
        var result = Read($$"""
                            <Theme xmlns="{{Namespace}}"
                                   Id="DaybreakBlue"
                                   Name="Daybreak Blue"
                                   Language="AntDesign"
                                   Style="Default"
                                   Appearance="Light"
                                   Density="Compact">
                              <Tokens>
                                <Token Name="ColorPrimary" Value="#1677FF" Locked="true" />
                              </Tokens>
                              <LanguageTokens>
                                <Token Name="Radius" Value="0.5rem" />
                              </LanguageTokens>
                              <Controls>
                                <Control Catalog="AtomUI" Id="Button" Appearance="Dark" Density="Default">
                                  <Tokens>
                                    <Token Name="ContentFontSize" Value="14" />
                                  </Tokens>
                                </Control>
                              </Controls>
                              <When Appearance="Dark" Idiom="Desktop">
                                <Tokens><Token Name="ColorBgBase" Value="#000000" /></Tokens>
                              </When>
                            </Theme>
                            """);

        result.Success.ShouldBeTrue(result.DiagnosticsText());
        var document = result.Document!;
        document.Id.ShouldBe("DaybreakBlue");
        document.Name.ShouldBe("Daybreak Blue");
        document.Language.ShouldBe("AntDesign");
        document.Style.ShouldBe("Default");
        document.Appearance.ShouldBe(ThemeAppearance.Light);
        document.Density.ShouldBe("Compact");
        document.Tokens.ShouldHaveSingleItem().Locked.ShouldBeTrue();
        document.Tokens[0].Location.Line.ShouldBe(9);
        document.LanguageTokens.ShouldHaveSingleItem().Name.ShouldBe("Radius");
        var button = document.Controls.ShouldHaveSingleItem();
        button.Identity.ToString().ShouldBe("AtomUI:Button");
        button.Appearance.ShouldBe(ThemeAppearance.Dark);
        button.Density.ShouldBe("Default");
        button.Tokens[0].Location.Path.ShouldBe(
            "/Theme/Controls/Control[@Catalog='AtomUI'][@Id='Button']/Tokens/Token[@Name='ContentFontSize']");
        var condition = document.Conditions.ShouldHaveSingleItem();
        condition.Appearance.ShouldBe(ThemeAppearance.Dark);
        condition.Idiom.ShouldBe("Desktop");
        condition.Style.ShouldBeNull();
        condition.HasCondition.ShouldBeTrue();
        condition.Values.Tokens.ShouldHaveSingleItem().Name.ShouldBe("ColorBgBase");
        Should.Throw<NotSupportedException>(() =>
            ((IList<ThemeTokenDocument>)document.Tokens).Add(document.Tokens[0]));
    }

    [Fact]
    public void Read_Leaves_Unwritten_Axes_And_Sections_Empty()
    {
        var result = Read(MinimalTheme());

        result.Success.ShouldBeTrue(result.DiagnosticsText());
        var document = result.Document!;
        document.Style.ShouldBeNull();
        document.Appearance.ShouldBeNull();
        document.Density.ShouldBeNull();
        document.Values.IsEmpty.ShouldBeTrue();
        document.Conditions.ShouldBeEmpty();
    }

    [Fact]
    public void Read_Rejects_A_V1_Document_And_Names_The_Conversion_Command()
    {
        var result = Read("""
                          <Theme xmlns="https://atomui.net/schemas/theme/v1" Id="T" Name="T" Appearance="Light">
                            <Algorithms><Algorithm Id="Default" /></Algorithms>
                          </Theme>
                          """);

        result.Success.ShouldBeFalse();
        result.Document.ShouldBeNull();
        var diagnostic = result.Diagnostics.ShouldHaveSingleItem();
        diagnostic.Code.ShouldBe("ATMTHM1004");
        diagnostic.Message.ShouldContain("v1");
        diagnostic.Message.ShouldContain("dotnet msbuild -t:ConvertAtomUIThemeDefinitions");
    }

    [Theory]
    [InlineData("<Theme Id='T' Name='T' Language='AntDesign' />")]
    [InlineData("<Theme xmlns='https://atomui.net/schemas/theme/v2' Id='T' Name='T' />")]
    [InlineData("<Theme xmlns='https://atomui.net/schemas/theme/v2' Id='T' Name='T' Language='AntDesign'><SharedTokens /></Theme>")]
    [InlineData("<Theme xmlns='https://atomui.net/schemas/theme/v2' Id='T' Name='T' Language='AntDesign'><Controls /></Theme>")]
    [InlineData("<Theme xmlns='https://atomui.net/schemas/theme/v2' Id='T' Name='T' Language='AntDesign'><When Appearance='Dark' /><Tokens><Token Name='X' Value='1'/></Tokens></Theme>")]
    [InlineData("<Theme xmlns='https://atomui.net/schemas/theme/v2' Id='T' Name='T' Language='AntDesign' Appearance='Dim' />")]
    [InlineData("<Theme xmlns='https://atomui.net/schemas/theme/v2' Id='T' Name='T' Language='AntDesign' Unknown='x' />")]
    [InlineData("<Theme xmlns='https://atomui.net/schemas/theme/v2' Id='T' Name='T' Language='AntDesign'><Tokens><Token Name='X' Value='1' Locked='yes'/></Tokens></Theme>")]
    [InlineData("<Theme xmlns='https://atomui.net/schemas/theme/v2' Id='T' Name='T' Language='Ant Design' />")]
    public void Read_Rejects_Documents_Outside_The_V2_Structure(string xml)
    {
        var result = Read(xml);

        result.Success.ShouldBeFalse();
        result.Document.ShouldBeNull();
        result.Diagnostics.ShouldContain(static diagnostic => diagnostic.Code == "ATMTHM1002" || diagnostic.Code == "ATMTHM1004");
    }

    [Theory]
    [InlineData("<Tokens><Token Name='X' Value='1'/><Token Name='X' Value='2'/></Tokens>")]
    [InlineData("<LanguageTokens><Token Name='X' Value='1'/><Token Name='X' Value='2'/></LanguageTokens>")]
    [InlineData("<Controls><Control Catalog='AtomUI' Id='Button' Appearance='Dark'/><Control Catalog='AtomUI' Id='Button' Density='Compact'/></Controls>")]
    [InlineData("<Controls><Control Catalog='AtomUI' Id='Button'><Tokens><Token Name='X' Value='1'/><Token Name='X' Value='2'/></Tokens></Control></Controls>")]
    [InlineData("<When Appearance='Dark'><Tokens><Token Name='X' Value='1'/><Token Name='X' Value='2'/></Tokens></When>")]
    public void Read_Enforces_Xsd_Identity_Constraints(string body)
    {
        var result = Read($$"""
                            <Theme xmlns="{{Namespace}}" Id="T" Name="T" Language="AntDesign">
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
                                         <!DOCTYPE Theme [<!ENTITY value "AntDesign">]>
                                         <Theme xmlns="{{Namespace}}" Id="T" Name="T" Language="&value;" />
                                         """);

        var result = ThemeDocumentReader.Read(stream, Source);

        result.Success.ShouldBeFalse();
        result.Diagnostics.ShouldContain(static diagnostic => diagnostic.Code == "ATMTHM1001");
        stream.CanRead.ShouldBeTrue();
    }

    [Fact]
    public void Read_Reports_Malformed_Xml_With_Its_Position()
    {
        var result = Read($"<Theme xmlns=\"{Namespace}\" Id=\"T\" Name=\"T\" Language=\"AntDesign\"><Tokens>");

        result.Success.ShouldBeFalse();
        var diagnostic = result.Diagnostics.ShouldHaveSingleItem();
        diagnostic.Code.ShouldBe("ATMTHM1001");
        diagnostic.FilePath.ShouldBe(Source);
        diagnostic.Line.ShouldBeGreaterThan(0);
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
        using var stream = CreateStream($$"""
                                         <Theme xmlns="{{Namespace}}" Id="T" Name="T" Language="AntDesign">
                                           <Tokens><Token Name="A" Value="1" /><Token Name="B" Value="2" /></Tokens>
                                         </Theme>
                                         """);
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
        return $$"""<Theme xmlns="{{Namespace}}" Id="T" Name="T" Language="AntDesign" />""";
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
