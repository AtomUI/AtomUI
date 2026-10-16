using System.IO.Compression;
using System.Text;
using System.Xml.Linq;
using Shouldly;
using Xunit;

namespace AtomUI.Localization.IntegrationTests;

public sealed partial class LanguagePackEndToEndTests
{
    private const string AggregatePackageId = "AtomUI.I18n.PtBR";
    private const string DataGridCatalogId =
        "AtomUI.Desktop.Controls.Localization.DataGridLangResourceKind";

    private static readonly OfficialLanguagePackage[] s_officialLanguagePackages =
    [
        new(
            "AtomUI.Controls.I18n.PtBR",
            "AtomUI.Controls",
            "src/LanguagePacks/pt-BR/AtomUI.Controls.I18n.PtBR/AtomUI.Controls.I18n.PtBR.csproj",
            "src/AtomUI.Controls/AtomUI.Controls.csproj",
            ["Common/pt-BR.xlf"]),
        new(
            "AtomUI.Desktop.Controls.I18n.PtBR",
            "AtomUI.Desktop.Controls",
            "src/LanguagePacks/pt-BR/AtomUI.Desktop.Controls.I18n.PtBR/AtomUI.Desktop.Controls.I18n.PtBR.csproj",
            "src/AtomUI.Desktop.Controls/AtomUI.Desktop.Controls.csproj",
            [
                "Calendar/pt-BR.xlf",
                "DatePicker/pt-BR.xlf",
                "DateViewer/pt-BR.xlf",
                "Dialog/pt-BR.xlf",
                "ImagePreviewer/pt-BR.xlf",
                "Pagination/pt-BR.xlf",
                "QRCode/pt-BR.xlf",
                "TimePicker/pt-BR.xlf",
                "Tour/pt-BR.xlf",
                "Transfer/pt-BR.xlf",
                "Upload/pt-BR.xlf"
            ]),
        new(
            "AtomUI.Desktop.Controls.DataGrid.I18n.PtBR",
            "AtomUI.Desktop.Controls.DataGrid",
            "src/LanguagePacks/pt-BR/AtomUI.Desktop.Controls.DataGrid.I18n.PtBR/" +
            "AtomUI.Desktop.Controls.DataGrid.I18n.PtBR.csproj",
            "src/AtomUI.Desktop.Controls.DataGrid/AtomUI.Desktop.Controls.DataGrid.csproj",
            ["pt-BR.xlf"]),
        new(
            "AtomUI.Desktop.Controls.ColorPicker.I18n.PtBR",
            "AtomUI.Desktop.Controls.ColorPicker",
            "src/LanguagePacks/pt-BR/AtomUI.Desktop.Controls.ColorPicker.I18n.PtBR/" +
            "AtomUI.Desktop.Controls.ColorPicker.I18n.PtBR.csproj",
            "src/AtomUI.Desktop.Controls.ColorPicker/AtomUI.Desktop.Controls.ColorPicker.csproj",
            ["pt-BR.xlf"])
    ];

    [Fact(Timeout = 600_000)]
    public async Task Official_PtBr_Package_Graph_Is_Consumable()
    {
        var repositoryRoot = FindRepositoryRoot();
        var temporaryRoot = Path.Combine(
            CanonicalTemporaryPath(),
            "AtomUI.Localization.IntegrationTests",
            Guid.NewGuid().ToString("N"));
        var feed = Path.Combine(temporaryRoot, "feed");
        var toolingFeed = Path.Combine(temporaryRoot, "tooling-feed");
        // Published versions may already exist in the global NuGet fallback folder.
        // Give this fixture a unique version so consumers must restore our local packages.
        var packageVersion = ReadAtomUiVersion(repositoryRoot) + "-integration." + Guid.NewGuid().ToString("N");
        var toolingPackageVersion = packageVersion + ".tooling";
        Directory.CreateDirectory(feed);
        Directory.CreateDirectory(toolingFeed);

        try
        {
            await PackOfficialPtBrPackages(
                repositoryRoot,
                temporaryRoot,
                feed,
                toolingFeed,
                packageVersion,
                toolingPackageVersion);
            AssertOfficialPackageLayouts(feed, packageVersion);

            var fullConsumer = CreateConsumerProject(
                repositoryRoot,
                temporaryRoot,
                "full-consumer",
                packageVersion,
                toolingPackageVersion,
                includeDataGrid: true,
                includeColorPicker: true);
            var fullOutput = await RestoreAndBuildConsumer(
                repositoryRoot,
                temporaryRoot,
                feed,
                toolingFeed,
                fullConsumer,
                "full");
            File.Exists(Path.Combine(fullOutput, "AtomUI.Desktop.Controls.DataGrid.dll"))
                .ShouldBeTrue();
            File.Exists(Path.Combine(fullOutput, "AtomUI.Desktop.Controls.ColorPicker.dll"))
                .ShouldBeTrue();

            var partialConsumer = CreateConsumerProject(
                repositoryRoot,
                temporaryRoot,
                "partial-consumer",
                packageVersion,
                toolingPackageVersion,
                includeDataGrid: false,
                includeColorPicker: false);
            var partialOutput = await RestoreAndBuildConsumer(
                repositoryRoot,
                temporaryRoot,
                feed,
                toolingFeed,
                partialConsumer,
                "partial");
            AssertPartialConsumerGraph(partialConsumer, partialOutput, packageVersion);

            var corruptedFeed = Path.Combine(temporaryRoot, "corrupted-feed");
            CopyDirectory(feed, corruptedFeed);
            CorruptDataGridCatalog(corruptedFeed, packageVersion);

            var dormantConsumer = CreateConsumerProject(
                repositoryRoot,
                temporaryRoot,
                "dormant-corrupted-consumer",
                packageVersion,
                toolingPackageVersion,
                includeDataGrid: false,
                includeColorPicker: false);
            await RestoreAndBuildConsumer(
                repositoryRoot,
                temporaryRoot,
                corruptedFeed,
                toolingFeed,
                dormantConsumer,
                "dormant-corrupted");

            var activeConsumer = CreateConsumerProject(
                repositoryRoot,
                temporaryRoot,
                "active-corrupted-consumer",
                packageVersion,
                toolingPackageVersion,
                includeDataGrid: true,
                includeColorPicker: false);
            await RestoreConsumer(
                repositoryRoot,
                temporaryRoot,
                corruptedFeed,
                toolingFeed,
                activeConsumer,
                "active-corrupted");
            var activeResult = await BuildConsumerUnchecked(
                repositoryRoot,
                temporaryRoot,
                activeConsumer,
                "active-corrupted");
            activeResult.ExitCode.ShouldNotBe(0);
            activeResult.Output.ShouldContain("ATOMUILOC006");
            activeResult.Output.ShouldContain("referenced Catalog");
        }
        finally
        {
            if (Directory.Exists(temporaryRoot))
            {
                Directory.Delete(temporaryRoot, recursive: true);
            }
        }
    }

    private static async Task PackOfficialPtBrPackages(
        string repositoryRoot,
        string temporaryRoot,
        string feed,
        string toolingFeed,
        string packageVersion,
        string toolingPackageVersion)
    {
        var globalPackages = GlobalPackagesPath();
        await RunProcess(
            "Pack AtomUI.Generator",
            repositoryRoot,
            temporaryRoot,
            "dotnet",
            "msbuild",
            Path.Combine(repositoryRoot, "src/AtomUI.Generator/AtomUI.Generator.csproj"),
            "-restore",
            "-t:Pack",
            "-m:1",
            "-nr:false",
            "-p:Configuration=Release",
            $"-p:Version={toolingPackageVersion}",
            $"-p:PackageOutputPath={toolingFeed}",
            $"-p:RestorePackagesPath={globalPackages}",
            "-p:NoPackageAnalysis=true");

        foreach (var package in s_officialLanguagePackages)
        {
            var result = await RunProcess(
                $"Pack {package.PackageId}",
                repositoryRoot,
                temporaryRoot,
                "dotnet",
                "msbuild",
                Path.Combine(repositoryRoot, package.ProjectPath),
                "-restore",
                "-t:Pack",
                "-m:1",
                "-nr:false",
                "-p:Configuration=Release",
                $"-p:AtomUIVersion={packageVersion}",
                $"-p:PackageOutputPath={feed}",
                $"-p:RestorePackagesPath={globalPackages}",
                "-p:NoPackageAnalysis=true");
            result.Output.ShouldNotContain("ATOMUILOC010");
        }

        await RunProcess(
            $"Pack {AggregatePackageId}",
            repositoryRoot,
            temporaryRoot,
            "dotnet",
            "msbuild",
            Path.Combine(
                repositoryRoot,
                "src/LanguagePacks/pt-BR/AtomUI.I18n.PtBR/AtomUI.I18n.PtBR.csproj"),
            "-restore",
            "-t:Pack",
            "-m:1",
            "-nr:false",
            "-p:Configuration=Release",
            $"-p:AtomUIVersion={packageVersion}",
            $"-p:PackageOutputPath={feed}",
            $"-p:RestorePackagesPath={globalPackages}",
            "-p:NoPackageAnalysis=true");
    }

    private static void AssertOfficialPackageLayouts(string feed, string packageVersion)
    {
        Directory.EnumerateFiles(feed, "*.nupkg", SearchOption.TopDirectoryOnly)
                 .Count()
                 .ShouldBe(5);

        foreach (var package in s_officialLanguagePackages)
        {
            var packagePath = PackagePath(feed, package.PackageId, packageVersion);
            var entries = PackageEntries(packagePath);
            var expectedXliffEntries = package.LanguagePaths
                .Select(static path => "contentFiles/any/any/" + path)
                .Order(StringComparer.Ordinal)
                .ToArray();
            entries.Where(static path => path.EndsWith(".xlf", StringComparison.OrdinalIgnoreCase))
                   .Order(StringComparer.Ordinal)
                   .ShouldBe(expectedXliffEntries);
            entries.ShouldContain("contentFiles/any/any/AtomUI.LanguagePack.xml");
            entries.ShouldContain($"buildTransitive/{package.PackageId}.props");
            entries.Count(IsBuildAsset).ShouldBe(1);
            entries.ShouldNotContain(static path => IsRuntimeAsset(path));
            entries.ShouldNotContain(static path => IsUnexpectedBuildOrRuntimeAsset(path));

            var dependencies = PackageDependencies(
                packagePath,
                $"{package.PackageId}.nuspec");
            dependencies.ShouldBeEmpty();

            var propsFingerprints = AssertOfficialLanguageProps(packagePath, package);
            var manifest = XDocument.Parse(PackageEntryText(
                packagePath,
                "contentFiles/any/any/AtomUI.LanguagePack.xml"));
            var manifestRoot = manifest.Root.ShouldNotBeNull();
            ((string?)manifestRoot.Attribute("packageId")).ShouldBe(package.PackageId);
            ((string?)manifestRoot.Attribute("language")).ShouldBe("pt-BR");
            var manifestCatalogs = manifestRoot.Elements("catalog").ToDictionary(
                static catalog => ((string?)catalog.Attribute("path")).ShouldNotBeNull(),
                StringComparer.Ordinal);
            manifestCatalogs.Count.ShouldBe(package.LanguagePaths.Count);
            foreach (var languagePath in package.LanguagePaths)
            {
                var catalog = manifestCatalogs[languagePath];
                ((string?)catalog.Attribute("moduleId")).ShouldBe(package.ModuleId);
                catalog.Attribute("contractValidation").ShouldBeNull();
                catalog.Attribute("contractVersion").ShouldBeNull();
                ((string?)catalog.Attribute("sourceFingerprint"))
                    .ShouldBe(propsFingerprints[languagePath]);

                var xliff = XDocument.Parse(PackageEntryText(
                    packagePath,
                    "contentFiles/any/any/" + languagePath));
                var xliffRoot = xliff.Root.ShouldNotBeNull();
                ((string?)xliffRoot.Attribute("trgLang")).ShouldBe("pt-BR");
                var xliffFile = xliffRoot.Elements().ShouldHaveSingleItem();
                ((string?)xliffFile.Attribute("id"))
                    .ShouldBe((string?)catalog.Attribute("catalogId"));
            }
        }

        var aggregatePath = PackagePath(feed, AggregatePackageId, packageVersion);
        var aggregateEntries = PackageEntries(aggregatePath);
        aggregateEntries.ShouldContain("README.nuget.md");
        aggregateEntries.ShouldAllBe(static path =>
            IsOfficialAggregateMetadataEntry(path) ||
            path.Equals("README.nuget.md", StringComparison.Ordinal));
        var aggregateNuspec = XDocument.Parse(PackageEntryText(
            aggregatePath,
            $"{AggregatePackageId}.nuspec"));
        aggregateNuspec.Descendants()
                       .Where(static element => element.Name.LocalName == "readme")
                       .ShouldHaveSingleItem()
                       .Value.ShouldBe("README.nuget.md");
        var aggregateDependencies = PackageDependencies(
            aggregatePath,
            $"{AggregatePackageId}.nuspec");
        aggregateDependencies.Count.ShouldBe(s_officialLanguagePackages.Length);
        foreach (var package in s_officialLanguagePackages)
        {
            aggregateDependencies[package.PackageId].ShouldBe($"[{packageVersion}]");
        }
    }

    private static IReadOnlyDictionary<string, string> AssertOfficialLanguageProps(
        string packagePath,
        OfficialLanguagePackage package)
    {
        var props = XDocument.Parse(PackageEntryText(
            packagePath,
            $"buildTransitive/{package.PackageId}.props"));
        var items = props.Descendants("AtomUILanguage").ToArray();
        items.Length.ShouldBe(package.LanguagePaths.Count);
        items.Select(static item => (string?)item.Attribute("AtomUILanguagePackagePath"))
             .Order(StringComparer.Ordinal)
             .ShouldBe(package.LanguagePaths.Order(StringComparer.Ordinal));
        items.ShouldAllBe(item =>
            string.Equals(
                (string?)item.Attribute("AtomUILanguageSourceKind"),
                "StaticLanguagePack",
                StringComparison.Ordinal) &&
            string.Equals(
                (string?)item.Attribute("AtomUILanguageSourceIdentity"),
                package.PackageId,
                StringComparison.Ordinal) &&
            string.Equals(
                (string?)item.Attribute("AtomUILanguageModuleId"),
                package.ModuleId,
                StringComparison.Ordinal) &&
            string.Equals(
                (string?)item.Attribute("AtomUILanguageContractValidation"),
                "Verified",
                StringComparison.Ordinal));
        return items.ToDictionary(
            static item => ((string?)item.Attribute("AtomUILanguagePackagePath"))
                .ShouldNotBeNull(),
            static item =>
            {
                var fingerprint = ((string?)item.Attribute("AtomUILanguageSourceFingerprint"))
                    .ShouldNotBeNull();
                fingerprint.ShouldMatch("^[0-9a-f]{64}$");
                return fingerprint;
            },
            StringComparer.Ordinal);
    }

    private static string CreateConsumerProject(
        string repositoryRoot,
        string temporaryRoot,
        string name,
        string packageVersion,
        string toolingPackageVersion,
        bool includeDataGrid,
        bool includeColorPicker)
    {
        var projectDirectory = Path.Combine(temporaryRoot, name);
        Directory.CreateDirectory(projectDirectory);
        var projectPath = Path.Combine(projectDirectory, name + ".csproj");

        var projectReferences = new List<XElement>
        {
            ProjectReference(repositoryRoot, "src/AtomUI.Controls/AtomUI.Controls.csproj"),
            ProjectReference(
                repositoryRoot,
                "src/AtomUI.Desktop.Controls/AtomUI.Desktop.Controls.csproj")
        };
        if (includeDataGrid)
        {
            projectReferences.Add(ProjectReference(
                repositoryRoot,
                "src/AtomUI.Desktop.Controls.DataGrid/AtomUI.Desktop.Controls.DataGrid.csproj"));
        }
        if (includeColorPicker)
        {
            projectReferences.Add(ProjectReference(
                repositoryRoot,
                "src/AtomUI.Desktop.Controls.ColorPicker/AtomUI.Desktop.Controls.ColorPicker.csproj"));
        }

        var project = new XDocument(
            new XElement(
                "Project",
                new XAttribute("Sdk", "Microsoft.NET.Sdk"),
                new XElement(
                    "PropertyGroup",
                    new XElement("TargetFramework", "net10.0"),
                    new XElement("Nullable", "enable"),
                    new XElement("ImplicitUsings", "enable")),
                new XElement(
                    "ItemGroup",
                    new XElement(
                        "PackageReference",
                        new XAttribute("Include", AggregatePackageId),
                        new XAttribute("Version", packageVersion)),
                    new XElement(
                        "PackageReference",
                        new XAttribute("Include", "AtomUI.Generator"),
                        new XAttribute("Version", toolingPackageVersion),
                        new XAttribute("PrivateAssets", "all"))),
                new XElement("ItemGroup", projectReferences),
                new XElement(
                    "ItemGroup",
                    AuthoritativeSourceItems(
                        repositoryRoot,
                        includeDataGrid,
                        includeColorPicker))));
        project.Save(projectPath);

        File.WriteAllText(
            Path.Combine(projectDirectory, "Consumer.cs"),
            ConsumerSource(includeDataGrid, includeColorPicker),
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        return projectPath;
    }

    private static XElement ProjectReference(string repositoryRoot, string relativePath)
    {
        return new XElement(
            "ProjectReference",
            new XAttribute("GlobalPropertiesToRemove", "OutputPath"),
            new XAttribute("Include", Path.Combine(repositoryRoot, relativePath)));
    }

    private static IEnumerable<XElement> AuthoritativeSourceItems(
        string repositoryRoot,
        bool includeDataGrid,
        bool includeColorPicker)
    {
        foreach (var package in s_officialLanguagePackages)
        {
            if ((!includeDataGrid &&
                 package.ModuleId == "AtomUI.Desktop.Controls.DataGrid") ||
                (!includeColorPicker &&
                 package.ModuleId == "AtomUI.Desktop.Controls.ColorPicker"))
            {
                continue;
            }

            var sourceProjectPath = Path.Combine(repositoryRoot, package.SourceProjectPath);
            var sourceDirectory = Path.GetDirectoryName(sourceProjectPath).ShouldNotBeNull();
            var sourceFiles = Directory.EnumerateFiles(
                                           sourceDirectory,
                                           "en-US.xlf",
                                           SearchOption.AllDirectories)
                                       .ToDictionary(
                                           path => GetLanguagePackagePath(sourceDirectory, path),
                                           StringComparer.Ordinal);
            foreach (var languagePath in package.LanguagePaths)
            {
                var sourcePackagePath = languagePath.Substring(
                                            0,
                                            languagePath.Length - "pt-BR.xlf".Length) +
                                        "en-US.xlf";
                var sourcePath = sourceFiles[sourcePackagePath];
                yield return new XElement(
                    "AtomUILanguage",
                    new XAttribute("Include", sourcePath),
                    new XElement("AtomUILanguageSourceKind", "ModuleBuiltIn"),
                    new XElement("AtomUILanguageSourceIdentity", package.ModuleId),
                    new XElement("AtomUILanguageModuleId", package.ModuleId),
                    new XElement("AtomUILanguageContractValidation", "Verified"),
                    new XElement(
                        "AtomUILanguagePackagePath",
                        "Localization/" + sourcePackagePath));
            }
        }
    }

    private static string GetLanguagePackagePath(string projectDirectory, string sourcePath)
    {
        var relativePath = Path.GetRelativePath(projectDirectory, sourcePath);
        return string.Join(
            '/',
            relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                        .Where(static segment =>
                            !string.Equals(segment, "Localization", StringComparison.Ordinal)));
    }

    private static string ConsumerSource(bool includeDataGrid, bool includeColorPicker)
    {
        var optionalCatalogs = new StringBuilder();
        if (includeDataGrid)
        {
            optionalCatalogs.AppendLine("        typeof(DataGridLangResourceKind),");
        }
        if (includeColorPicker)
        {
            optionalCatalogs.AppendLine("        typeof(ColorPickerLangResourceKind),");
        }

        return $$"""
            using AtomUI;
            using AtomUI.Controls.Localization;
            using AtomUI.Desktop.Controls.Localization;
            using AtomUI.Localization;
            using Avalonia;

            namespace OfficialPtBrConsumer;

            public sealed partial class FixtureApplication : Application
            {
                public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<FixtureApplication>()
                    .UseAtomUI(builder => builder.UseLanguages(
                        LanguageTags.PtBR,
                        [LanguageTags.EnUS, LanguageTags.PtBR]));
            }

            public static class CatalogProbe
            {
                public static Type[] Catalogs { get; } =
                [
                    typeof(CommonLangResourceKind),
                    typeof(DatePickerLangResourceKind),
            {{optionalCatalogs}}        ];
            }
            """;
    }

    private static async Task<string> RestoreAndBuildConsumer(
        string repositoryRoot,
        string temporaryRoot,
        string feed,
        string toolingFeed,
        string projectPath,
        string identity)
    {
        await RestoreConsumer(
            repositoryRoot,
            temporaryRoot,
            feed,
            toolingFeed,
            projectPath,
            identity);
        var result = await BuildConsumerUnchecked(
            repositoryRoot,
            temporaryRoot,
            projectPath,
            identity);
        result.ExitCode.ShouldBe(0, FormatFailure(result, "failed"));
        return ConsumerOutputPath(temporaryRoot, identity);
    }

    private static async Task RestoreConsumer(
        string repositoryRoot,
        string temporaryRoot,
        string feed,
        string toolingFeed,
        string projectPath,
        string identity)
    {
        await RunProcess(
            $"Restore {identity} consumer",
            repositoryRoot,
            temporaryRoot,
            "dotnet",
            "restore",
            projectPath,
            "--disable-build-servers",
            "-m:1",
            "-nr:false",
            $"-p:RestoreAdditionalProjectSources={feed}%3B{toolingFeed}",
            $"-p:RestorePackagesPath={ConsumerPackagesPath(temporaryRoot, identity)}",
            $"-p:RestoreAdditionalProjectFallbackFolders={GlobalPackagesPath()}");
    }

    private static Task<ProcessResult> BuildConsumerUnchecked(
        string repositoryRoot,
        string temporaryRoot,
        string projectPath,
        string identity)
    {
        return RunProcessUnchecked(
            $"Build {identity} consumer",
            repositoryRoot,
            temporaryRoot,
            "dotnet",
            "build",
            projectPath,
            "--no-restore",
            "--disable-build-servers",
            "-m:1",
            "-nr:false",
            "-p:Configuration=Debug",
            $"-p:OutputPath={ConsumerOutputPath(temporaryRoot, identity)}",
            $"-p:RestorePackagesPath={ConsumerPackagesPath(temporaryRoot, identity)}",
            $"-p:RestoreAdditionalProjectFallbackFolders={GlobalPackagesPath()}");
    }

    private static void AssertPartialConsumerGraph(
        string projectPath,
        string outputPath,
        string packageVersion)
    {
        var assetsPath = Path.Combine(Path.GetDirectoryName(projectPath)!, "obj", "project.assets.json");
        var assets = File.ReadAllText(assetsPath);
        foreach (var package in s_officialLanguagePackages)
        {
            assets.ShouldContain($"{package.PackageId}/{packageVersion}");
        }
        assets.ShouldNotContain($"AtomUI.Desktop.Controls.DataGrid/{packageVersion}");
        assets.ShouldNotContain($"AtomUI.Desktop.Controls.ColorPicker/{packageVersion}");
        File.Exists(Path.Combine(outputPath, "AtomUI.Desktop.Controls.DataGrid.dll"))
            .ShouldBeFalse();
        File.Exists(Path.Combine(outputPath, "AtomUI.Desktop.Controls.ColorPicker.dll"))
            .ShouldBeFalse();
    }

    private static void CorruptDataGridCatalog(string feed, string packageVersion)
    {
        var packagePath = PackagePath(
            feed,
            "AtomUI.Desktop.Controls.DataGrid.I18n.PtBR",
            packageVersion);
        using var archive = ZipFile.Open(packagePath, ZipArchiveMode.Update);
        const string entryPath = "contentFiles/any/any/pt-BR.xlf";
        var entry = archive.GetEntry(entryPath).ShouldNotBeNull();
        string content;
        using (var reader = new StreamReader(entry.Open()))
        {
            content = reader.ReadToEnd();
        }
        var corrupted = content.Replace(
            DataGridCatalogId,
            "AtomUI.Desktop.Controls.Localization.UnknownDataGridLangResourceKind",
            StringComparison.Ordinal);
        corrupted.ShouldNotBe(content);
        entry.Delete();
        var replacement = archive.CreateEntry(entryPath, CompressionLevel.Optimal);
        using var writer = new StreamWriter(
            replacement.Open(),
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        writer.Write(corrupted);
    }

    private static void CopyDirectory(string source, string destination)
    {
        Directory.CreateDirectory(destination);
        foreach (var sourcePath in Directory.EnumerateFiles(source))
        {
            File.Copy(sourcePath, Path.Combine(destination, Path.GetFileName(sourcePath)));
        }
    }

    private static bool IsOfficialAggregateMetadataEntry(string path)
    {
        const string corePropertiesPrefix = "package/services/metadata/core-properties/";
        if (path.Equals("_rels/.rels", StringComparison.Ordinal) ||
            path.Equals("[Content_Types].xml", StringComparison.Ordinal) ||
            path.Equals($"{AggregatePackageId}.nuspec", StringComparison.Ordinal))
        {
            return true;
        }

        if (!path.StartsWith(corePropertiesPrefix, StringComparison.Ordinal) ||
            !path.EndsWith(".psmdcp", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var fileName = path[corePropertiesPrefix.Length..];
        return fileName.IndexOf('/') < 0 &&
               Guid.TryParse(fileName[..^".psmdcp".Length], out _);
    }

    private static string ReadAtomUiVersion(string repositoryRoot)
    {
        var versionDocument = XDocument.Load(Path.Combine(
            repositoryRoot,
            "build/Versions.props"));
        return versionDocument.Descendants("AtomUIVersion").ShouldHaveSingleItem().Value;
    }

    private static string PackagePath(string feed, string packageId, string packageVersion)
    {
        return Path.Combine(feed, $"{packageId}.{packageVersion}.nupkg");
    }

    private static string GlobalPackagesPath()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".nuget",
            "packages");
    }

    private static string CanonicalTemporaryPath()
    {
        var temporaryPath = Path.GetTempPath();
        if (OperatingSystem.IsMacOS() &&
            temporaryPath.StartsWith("/var/", StringComparison.Ordinal))
        {
            return "/private" + temporaryPath;
        }

        return temporaryPath;
    }

    private static string ConsumerPackagesPath(string temporaryRoot, string identity)
    {
        return Path.Combine(temporaryRoot, "packages-" + identity);
    }

    private static string ConsumerOutputPath(string temporaryRoot, string identity)
    {
        return Path.Combine(temporaryRoot, "output-" + identity);
    }

    private sealed record OfficialLanguagePackage(
        string PackageId,
        string ModuleId,
        string ProjectPath,
        string SourceProjectPath,
        IReadOnlyList<string> LanguagePaths);
}
