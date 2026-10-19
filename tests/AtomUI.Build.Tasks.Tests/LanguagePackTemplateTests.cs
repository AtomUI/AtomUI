using System.Text.Json;
using System.Xml.Linq;
using Shouldly;
using Xunit;

namespace AtomUI.Build.Tasks.Tests;

public class LanguagePackTemplateTests
{
    [Fact]
    public void Template_Uses_Every_Declared_Replacement_Symbol()
    {
        var templateRoot = GetTemplateRoot();
        using var configuration = JsonDocument.Parse(File.ReadAllText(Path.Combine(
            templateRoot,
            ".template.config",
            "template.json")));
        var templateContent = string.Join(
            Environment.NewLine,
            Directory.GetFiles(templateRoot, "*", SearchOption.AllDirectories)
                     .Where(static path => !path.EndsWith("template.json", StringComparison.Ordinal))
                     .Select(File.ReadAllText));

        foreach (var symbol in configuration.RootElement.GetProperty("symbols").EnumerateObject())
        {
            var replacement = symbol.Value.GetProperty("replaces").GetString().ShouldNotBeNull();
            templateContent.Contains(replacement, StringComparison.Ordinal).ShouldBeTrue(
                $"Template symbol '{symbol.Name}' must replace at least one token in the template content.");
        }
    }

    [Fact]
    public void Template_Project_Produces_A_Content_Only_Static_Language_Pack()
    {
        var project = XDocument.Load(Path.Combine(GetTemplateRoot(), "AtomUI.LanguagePack.csproj"));
        ProjectProperty(project, "IncludeBuildOutput").ShouldBe("false");
        ProjectProperty(project, "SuppressDependenciesWhenPacking").ShouldBe("true");
        ProjectProperty(project, "AtomUIBuildLanguagePackage").ShouldBe("true");
        ProjectProperty(project, "AtomUILanguageTag").ShouldBe("__ATOMUI_LANGUAGE_TAG__");
        ProjectProperty(project, "AtomUILanguageModuleId").ShouldBe("__ATOMUI_LANGUAGE_MODULE_ID__");

        var generatorReference = project.Descendants("PackageReference")
                                        .Single(element =>
                                            (string?)element.Attribute("Include") == "AtomUI.Generator");
        ((string?)generatorReference.Attribute("PrivateAssets")).ShouldBe("all");
        ((string?)generatorReference.Attribute("Version")).ShouldBe("__ATOMUI_VERSION__");

        project.Descendants("AtomUILanguage").ShouldBeEmpty();

    }

    private static string ProjectProperty(XDocument project, string name)
    {
        return project.Root.ShouldNotBeNull()
                      .Elements("PropertyGroup")
                      .Elements(name)
                      .ShouldHaveSingleItem()
                      .Value;
    }

    private static string GetTemplateRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AtomUI.slnx")))
            {
                return Path.Combine(
                    directory.FullName,
                    "src",
                    "AtomUI.LanguagePack.Template",
                    "content",
                    "AtomUI.LanguagePack");
            }
            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("The AtomUI repository root could not be located.");
    }
}
