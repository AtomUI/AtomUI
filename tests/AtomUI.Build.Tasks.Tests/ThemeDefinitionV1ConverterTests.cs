using System.Runtime.CompilerServices;
using System.Xml.Linq;
using AtomUI.Theme.Build;
using Shouldly;
using Xunit;

namespace AtomUI.Build.Tasks.Tests;

/// <summary>
/// The one-shot v1 → v2 rewrite of theme definitions.
/// </summary>
public class ThemeDefinitionV1ConverterTests
{
    private static readonly XNamespace V2 = ThemeDefinitionV1Converter.V2Namespace;

    [Fact]
    public void A_Built_In_Chain_Becomes_The_AntDesign_Language_With_The_Axes_It_Fixed()
    {
        var result = Convert(("custom.theme.xml", V1("Custom", "Custom", "<Algorithm Id='Default'/><Algorithm Id='Compact'/><Algorithm Id='Dark'/>",
            "<Tokens><Token Name='ColorPrimary' Value='#52C41A'/></Tokens>", "IsDefault='true'")));

        var theme = Single(result);
        theme.Attribute("Language")!.Value.ShouldBe("AntDesign");
        theme.Attribute("Style").ShouldBeNull();
        theme.Attribute("Appearance")!.Value.ShouldBe("Dark");
        theme.Attribute("Density")!.Value.ShouldBe("Compact");
        theme.Attribute("IsDefault").ShouldBeNull();
        theme.Element(V2 + "Tokens")!.Element(V2 + "Token")!.Attribute("Value")!.Value.ShouldBe("#52C41A");
        result.Notes.ShouldContain(note => !note.IsError && note.Message.Contains("WithInitialTheme"));
    }

    [Fact]
    public void A_Package_Algorithm_Names_The_Language_And_The_Style()
    {
        var result = Convert(("nova-dark.theme.xml", V1("NovaDark", "Nova Dark", "<Algorithm Catalog='Shadcn' Id='Nova.Dark'/>", "")));

        var theme = Single(result);
        theme.Attribute("Language")!.Value.ShouldBe("Shadcn");
        theme.Attribute("Style")!.Value.ShouldBe("Nova");
        theme.Attribute("Appearance")!.Value.ShouldBe("Dark");
    }

    [Fact]
    public void The_Design_Language_Attribute_Splits_Into_Language_And_Style()
    {
        var result = Convert(("aura.theme.xml", V1("Aura", "Aura", "<Algorithm Catalog='PrimeNG' Id='Aura'/>", "",
            "DesignLanguage='PrimeNG.Aura'")));

        var theme = Single(result);
        theme.Attribute("Language")!.Value.ShouldBe("PrimeNG");
        theme.Attribute("Style")!.Value.ShouldBe("Aura");
        theme.Attribute("Appearance").ShouldBeNull();
    }

    [Fact]
    public void Four_Variants_Of_One_Language_Merge_Into_One_File_With_When_Sections()
    {
        var result = Convert(
            ("Nova.theme.xml", V1("Nova", "Nova", "<Algorithm Catalog='Shadcn' Id='Nova'/>",
                "<Tokens><Token Name='ColorPrimary' Value='#171717'/><Token Name='EnableWaveSpirit' Value='false'/></Tokens>")),
            ("Nova.Dark.theme.xml", V1("Nova.Dark", "Nova Dark", "<Algorithm Catalog='Shadcn' Id='Nova.Dark'/>",
                "<Tokens><Token Name='ColorPrimary' Value='#E5E5E5'/><Token Name='EnableWaveSpirit' Value='false'/></Tokens>")),
            ("Nova.Compact.theme.xml", V1("Nova.Compact", "Nova Compact", "<Algorithm Catalog='Shadcn' Id='Nova'/><Algorithm Id='Compact'/>",
                "<Tokens><Token Name='ColorPrimary' Value='#171717'/><Token Name='EnableWaveSpirit' Value='false'/></Tokens>")),
            ("Nova.DarkCompact.theme.xml", V1("Nova.DarkCompact", "Nova Dark Compact", "<Algorithm Catalog='Shadcn' Id='Nova.Dark'/><Algorithm Id='Compact'/>",
                "<Tokens><Token Name='ColorPrimary' Value='#E5E5E5'/><Token Name='EnableWaveSpirit' Value='false'/></Tokens>")));

        result.Files.ShouldHaveSingleItem().FileName.ShouldBe("Nova.theme.xml");
        result.MergedFileNames.ShouldBe(["Nova.Dark.theme.xml", "Nova.Compact.theme.xml", "Nova.DarkCompact.theme.xml"], ignoreOrder: true);
        var theme = Single(result);
        theme.Attribute("Id")!.Value.ShouldBe("Nova");
        theme.Attribute("Appearance").ShouldBeNull("a merged theme follows every variant");
        theme.Attribute("Density").ShouldBeNull();
        theme.Element(V2 + "Tokens")!.Elements(V2 + "Token").Select(static token => token.Attribute("Name")!.Value)
             .ShouldBe(["EnableWaveSpirit"]);
        var sections = theme.Elements(V2 + "When").ToArray();
        sections.Length.ShouldBe(4);
        sections.Single(section => section.Attribute("Appearance")!.Value == "Dark" && section.Attribute("Density")!.Value == "Compact")
                .Element(V2 + "Tokens")!.Element(V2 + "Token")!.Attribute("Value")!.Value.ShouldBe("#E5E5E5");
        result.Success.ShouldBeTrue();
    }

    [Fact]
    public void Fixed_Brand_And_Neutral_Colors_Become_Locked_Seeds()
    {
        var result = Convert(("fixed.theme.xml", V1("Fixed", "Fixed", "<Algorithm Catalog='Shadcn' Id='Nova'/>",
            "<Tokens><Token Name='ColorPrimary' Value='#171717'/><Token Name='ColorNeutral' Value='#737373'/></Tokens>",
            "BrandColor='Fixed' NeutralColor='Fixed'")));

        var theme = Single(result);
        theme.Element(V2 + "Tokens")!.Elements(V2 + "Token")
             .ShouldAllBe(token => token.Attribute("Locked")!.Value == "true");
        // v1's global ColorNeutral is the shadcn language seed BaseColor in v2.
        var neutral = theme.Element(V2 + "LanguageTokens")!.Elements(V2 + "Token").ShouldHaveSingleItem();
        neutral.Attribute("Name")!.Value.ShouldBe("BaseColor");
        neutral.Attribute("Locked")!.Value.ShouldBe("true");
    }

    [Fact]
    public void A_Disabled_Control_That_Overrides_A_Seed_Is_Reported_And_Kept()
    {
        var result = Convert(("controls.theme.xml", V1("Controls", "Controls", "<Algorithm Id='Default'/>",
            "<Controls><Control Catalog='AtomUI' Id='Button' Algorithm='Disabled'><Tokens><Token Name='ColorPrimary' Value='#FF0000'/></Tokens></Control>" +
            "<Control Catalog='AtomUI' Id='Select' Algorithm='Custom'><Algorithms><Algorithm Id='Default'/><Algorithm Id='Dark'/></Algorithms></Control></Controls>")));

        var controls = Single(result).Element(V2 + "Controls")!.Elements(V2 + "Control").ToArray();
        controls[0].Attribute("Algorithm").ShouldBeNull();
        controls[0].Element(V2 + "Tokens").ShouldNotBeNull();
        controls[1].Attribute("Appearance")!.Value.ShouldBe("Dark");
        result.Notes.ShouldContain(note => !note.IsError && note.Message.Contains("AtomUI:Button") && note.Message.Contains("re-derives"));
    }

    [Fact]
    public void A_V2_Document_Is_Skipped_And_Malformed_Input_Is_An_Error()
    {
        var result = Convert(
            ("v2.theme.xml", "<Theme xmlns='https://atomui.net/schemas/theme/v2' Id='T' Name='T' Language='AntDesign'/>"),
            ("broken.theme.xml", "<Theme"));

        result.Files.ShouldBeEmpty();
        result.Success.ShouldBeFalse();
        result.Notes.ShouldContain(note => note.FileName == "v2.theme.xml" && !note.IsError);
        result.Notes.ShouldContain(note => note.FileName == "broken.theme.xml" && note.IsError);
    }

    [Theory]
    [InlineData("tests/AtomUI.Build.Tasks.Tests/TestData/ThemeDefinitionsV1/Shadcn", 8)]
    [InlineData("tests/AtomUI.Build.Tasks.Tests/TestData/ThemeDefinitionsV1/PrimeNG", 3)]
    public void The_Former_V1_Theme_Packages_Convert_To_Valid_V2(string directory, int files)
    {
        var path = Path.Combine(RepositoryRoot(), directory);
        var sources = Directory.GetFiles(path, "*.theme.xml").OrderBy(static file => file, StringComparer.Ordinal)
                               .Select(static file => (Path.GetFileName(file), File.ReadAllText(file))).ToArray();

        var result = ThemeDefinitionV1Converter.Convert(sources);

        result.Notes.Where(static note => note.IsError).ShouldBeEmpty();
        result.Files.Count.ShouldBe(files);
        (result.Files.Count + result.MergedFileNames.Count).ShouldBe(sources.Length);
    }

    private static ThemeConversionResult Convert(params (string FileName, string Xml)[] documents) =>
        ThemeDefinitionV1Converter.Convert(documents);

    private static XElement Single(ThemeConversionResult result)
    {
        result.Notes.Where(static note => note.IsError).ShouldBeEmpty(string.Join("; ", result.Notes.Select(static note => note.Message)));
        return XDocument.Parse(result.Files.ShouldHaveSingleItem().Xml).Root!;
    }

    private static string V1(string id, string name, string algorithms, string body, string attributes = "") =>
        $"<Theme xmlns='{ThemeDefinitionV1Converter.V1Namespace}' Id='{id}' Name='{name}' Appearance='Light' {attributes}>" +
        $"<Algorithms>{algorithms}</Algorithms>{body}</Theme>";

    private static string RepositoryRoot([CallerFilePath] string sourceFile = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourceFile)!, "..", ".."));
}
