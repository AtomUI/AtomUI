using System.Xml.Linq;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Languages;
using AtomUI.Theme.Schema;
using AtomUIGallery.ShowCases.ThemePresets;
using Shouldly;
using Xunit;

namespace AtomUIGallery.Tests;

/// <summary>
/// The Theme Presets page exports the previewed configuration as a v2 theme definition that declares exactly what the
/// preview is: its language, the fixed axes and the token values, readable by the theme document reader.
/// </summary>
public class ThemePresetsDeclarationTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Export_Declares_The_Language_And_The_Previewed_Axes(bool dark, bool compact)
    {
        var appearance = dark ? ThemeAppearance.Dark : ThemeAppearance.Light;
        var density    = compact ? DesignDensity.Compact : DesignDensity.Default;
        var config = new ThemeConfigBuilder().WithInherit(false).WithLanguage("AntDesign")
                                             .WithAppearance(appearance).WithDensity(density)
                                             .WithToken("ColorPrimary", "#52C41A").Build();

        var xml  = ThemePresetCatalog.Export("Kept", "Kept", config);
        var root = XDocument.Parse(xml).Root!;

        root.Name.NamespaceName.ShouldBe("https://atomui.net/schemas/theme/v2");
        root.Attribute("Language")!.Value.ShouldBe("AntDesign");
        root.Attribute("Appearance")!.Value.ShouldBe(appearance.ToString());
        root.Attribute("Density")!.Value.ShouldBe(density.Id);
        root.Attribute("DesignLanguage").ShouldBeNull();
        root.Attribute("BrandColor").ShouldBeNull();
        root.Attribute("IsDefault").ShouldBeNull();
        root.Nodes().OfType<XComment>().ShouldBeEmpty();
        root.Descendants(root.Name.Namespace + "Token")
            .Single(token => token.Attribute("Name")!.Value == "ColorPrimary")
            .Attribute("Value")!.Value.ShouldBe("#52C41A");
        root.Element(root.Name.Namespace + "Controls").ShouldBeNull();
        ReadDiagnostics(xml).ShouldBeEmpty();
    }

    [Fact]
    public void Export_Writes_Control_Entries_With_Their_Fixed_Axes()
    {
        var config = new ThemeConfigBuilder().WithLanguage("AntDesign")
            .WithToken("ColorPrimary", "#1677FF")
            .WithControl(new ControlTokenIdentity("AtomUI", "Button"),
                new ControlThemeConfigBuilder().WithAppearance(ThemeAppearance.Dark)
                                               .WithToken("DefaultBg", "#FF00FF").Build())
            .Build();

        var xml     = ThemePresetCatalog.Export("Pinned", "Pinned", config);
        var ns      = XDocument.Parse(xml).Root!.Name.Namespace;
        var control = XDocument.Parse(xml).Root!.Element(ns + "Controls")!.Element(ns + "Control")!;

        control.Attribute("Catalog")!.Value.ShouldBe("AtomUI");
        control.Attribute("Id")!.Value.ShouldBe("Button");
        control.Attribute("Appearance")!.Value.ShouldBe("Dark");
        control.Element(ns + "Tokens")!.Element(ns + "Token")!.Attribute("Name")!.Value.ShouldBe("DefaultBg");
        ReadDiagnostics(xml).ShouldBeEmpty();
    }

    private static IReadOnlyList<string> ReadDiagnostics(string xml)
    {
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(xml));
        return AtomUI.Theme.Definitions.ThemeDocumentReader.Read(stream, "exported.theme.xml").Diagnostics
                     .Select(static diagnostic => diagnostic.ToString()).ToArray();
    }
}
