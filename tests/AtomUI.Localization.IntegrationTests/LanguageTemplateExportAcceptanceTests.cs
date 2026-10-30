using System.Xml.Linq;
using Shouldly;
using Xunit;
using static AtomUI.Localization.IntegrationTests.LanguagePackEndToEndTests;

namespace AtomUI.Localization.IntegrationTests;

public sealed class LanguageTemplateExportAcceptanceTests
{
    [Fact(Timeout = 360_000)]
    public async Task Language_Template_Is_Exported_From_A_Module_Package_Into_The_Current_Project()
    {
        var repositoryRoot = FindRepositoryRoot();
        var fixtureRoot = Path.Combine(repositoryRoot, "tests", "fixtures", "LanguagePackEndToEnd");
        var templateProjectDirectory = Path.Combine(fixtureRoot, "TemplateExport");
        var exportedTemplate = Path.Combine(
            templateProjectDirectory,
            "Localization",
            "Welcome",
            "ja-JP.xlf");
        var temporaryRoot = Path.Combine(
            Path.GetTempPath(),
            "AtomUI.Localization.IntegrationTests",
            Guid.NewGuid().ToString("N"));
        var feed = Path.Combine(temporaryRoot, "feed");
        var packages = Path.Combine(temporaryRoot, "packages");
        var globalPackages = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".nuget",
            "packages");
        var packageVersion = "1.0.0-local." + Guid.NewGuid().ToString("N");
        Directory.CreateDirectory(feed);
        Directory.CreateDirectory(packages);

        try
        {
            File.Exists(exportedTemplate).ShouldBeFalse(
                $"The template export fixture must start empty: '{exportedTemplate}'.");
            await RunProcess(
                "Pack module for template export",
                repositoryRoot,
                temporaryRoot,
                "dotnet",
                "msbuild",
                Path.Combine(fixtureRoot, "Module", "Module.csproj"),
                "-restore",
                "-t:Pack",
                "-m:1",
                "-nr:false",
                "-p:Configuration=Debug",
                $"-p:PackageVersion={packageVersion}",
                $"-p:PackageOutputPath={feed}",
                $"-p:RestorePackagesPath={globalPackages}",
                "-p:NoPackageAnalysis=true");
            AssertRepositoryCoreAssemblyVersion(repositoryRoot, temporaryRoot);

            var templateProject = Path.Combine(templateProjectDirectory, "TemplateExport.csproj");
            await RunProcess(
                "Restore template export project",
                repositoryRoot,
                temporaryRoot,
                "dotnet",
                "restore",
                templateProject,
                "--disable-build-servers",
                $"-p:FixturePackageVersion={packageVersion}",
                $"-p:RestoreAdditionalProjectSources={feed}",
                $"-p:RestorePackagesPath={packages}",
                $"-p:RestoreAdditionalProjectFallbackFolders={globalPackages}");
            await RunProcess(
                "Export language template",
                repositoryRoot,
                temporaryRoot,
                "dotnet",
                "msbuild",
                templateProject,
                "-t:AtomUIExportLanguageTemplates",
                "-m:1",
                "-nr:false",
                "-p:Configuration=Debug",
                "-p:AtomUITargetLanguage=ja-JP",
                $"-p:FixturePackageVersion={packageVersion}",
                $"-p:RestorePackagesPath={packages}",
                $"-p:RestoreAdditionalProjectFallbackFolders={globalPackages}");

            File.Exists(exportedTemplate).ShouldBeTrue();
            var exported = XDocument.Load(exportedTemplate);
            XNamespace xliff = "urn:oasis:names:tc:xliff:document:2.0";
            var root = exported.Root.ShouldNotBeNull();
            ((string?)root.Attribute("version")).ShouldBe("2.1");
            ((string?)root.Attribute("srcLang")).ShouldBe("en-US");
            ((string?)root.Attribute("trgLang")).ShouldBe("ja-JP");

            var file = root.Element(xliff + "file").ShouldNotBeNull();
            ((string?)file.Attribute("id"))
                .ShouldBe("Acme.LocalizationComponent.Localization.WelcomeLangResourceKind");
            var units = file.Elements(xliff + "unit")
                            .ToDictionary(
                                static unit => (string)unit.Attribute("id")!,
                                StringComparer.Ordinal);
            AssertExportedUnit(units["Greeting"], xliff, "Welcome");
            AssertExportedUnit(units["ItemCount"], xliff, "You have {0} items.");

            var packageCacheTemplate = Path.Combine(
                packages,
                "acme.localizationcomponent",
                packageVersion,
                "contentFiles",
                "any",
                "any",
                "Localization",
                "Welcome",
                "ja-JP.xlf");
            File.Exists(packageCacheTemplate).ShouldBeFalse();
        }
        finally
        {
            if (File.Exists(exportedTemplate))
            {
                File.Delete(exportedTemplate);
            }
            if (Directory.Exists(temporaryRoot))
            {
                Directory.Delete(temporaryRoot, recursive: true);
            }
        }
    }
}
