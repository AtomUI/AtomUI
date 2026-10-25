using System.Diagnostics;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using System.Xml.Linq;
using Shouldly;
using Xunit;

namespace AtomUI.Localization.IntegrationTests;

public sealed class LanguagePackEndToEndTests
{
    private static readonly object s_stageTraceLock = new();
    private static readonly TimeSpan s_stageTimeout = TimeSpan.FromSeconds(120);
    // Isolated consumers compile the full control graph without shared checkout outputs.
    private static readonly TimeSpan s_buildStageTimeout = TimeSpan.FromMinutes(5);

    [Fact]
    public async Task Repository_Build_Path_Overrides_Isolate_Outputs_And_Tool_Assets()
    {
        var repositoryRoot = FindRepositoryRoot();
        var temporaryRoot = Path.Combine(Path.GetTempPath(), "AtomUI.Localization.IntegrationTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporaryRoot);
        try
        {
            string[] properties = ["OutputPathWithoutFramework", "OutputPath", "BaseIntermediateOutputPath",
                "IntermediateOutputPathWithoutFramework", "IntermediateOutputPath", "MSBuildProjectExtensionsPath",
                "PackageOutputPath", "AtomUIBuildTasksOutputDirectory", "AtomUIBuildTasksAssembly", "AtomUIToolsetCacheRoot"];
            var result = await RunProcess("Evaluate isolated paths", repositoryRoot, temporaryRoot,
                "dotnet", "msbuild", Path.Combine(repositoryRoot, "tests/fixtures/LanguagePackEndToEnd/Module/Module.csproj"),
                "-p:Configuration=Release", "-p:TargetFramework=net10.0",
                "-getProperty:" + string.Join(',', properties), "-getItem:AtomUIGeneratorToolAsset");
            using var output = JsonDocument.Parse(result.StandardOutput);
            var artifactsRoot = Path.Combine(temporaryRoot, "artifacts") + Path.DirectorySeparatorChar;
            foreach (var property in properties)
            {
                Path.GetFullPath(output.RootElement.GetProperty("Properties").GetProperty(property).GetString()!)
                    .ShouldStartWith(artifactsRoot, customMessage: property);
            }
            var tools = output.RootElement.GetProperty("Items").GetProperty("AtomUIGeneratorToolAsset").EnumerateArray().ToArray();
            tools.ShouldNotBeEmpty();
            foreach (var tool in tools)
            {
                Path.GetFullPath(tool.GetProperty("Identity").GetString()!).ShouldStartWith(artifactsRoot);
            }
        }
        finally
        {
            Directory.Delete(temporaryRoot, recursive: true);
        }
    }

    [Fact(Timeout = 360_000)]
    public async Task Fixture_Module_Pack_Does_Not_Modify_Repository_Dependency_Outputs()
    {
        var repositoryRoot = FindRepositoryRoot();
        var temporaryRoot = Path.Combine(Path.GetTempPath(), "AtomUI.Localization.IntegrationTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporaryRoot);
        var before = CaptureSharedDependencyOutputs(repositoryRoot);
        try
        {
            await RunProcess(
                "Pack module without changing shared dependencies", repositoryRoot, temporaryRoot,
                "dotnet", "msbuild",
                Path.Combine(repositoryRoot, "tests/fixtures/LanguagePackEndToEnd/Module/Module.csproj"),
                "-restore", "-t:Pack", "-m:1", "-nr:false", "-p:Configuration=Debug",
                $"-p:PackageVersion=1.0.0-local.{Guid.NewGuid():N}",
                $"-p:PackageOutputPath={Path.Combine(temporaryRoot, "feed")}",
                $"-p:RestorePackagesPath={GlobalPackagesPath()}", "-p:NoPackageAnalysis=true");

            AssertRepositoryCoreAssemblyVersion(repositoryRoot, temporaryRoot);
            var after = CaptureSharedDependencyOutputs(repositoryRoot);
            var changed = before.Keys.Union(after.Keys).Where(path =>
                !before.TryGetValue(path, out var original) ||
                !after.TryGetValue(path, out var current) || original != current).ToArray();
            changed.ShouldBeEmpty("Fixture builds must preserve every shared dependency output byte.");
        }
        finally
        {
            Directory.Delete(temporaryRoot, recursive: true);
        }
    }

    private static SortedDictionary<string, string> CaptureSharedDependencyOutputs(string repositoryRoot)
    {
        var outputs = new SortedDictionary<string, string>(StringComparer.Ordinal);
        var directory = Path.Combine(repositoryRoot, ".artifacts", "bin");
        if (!Directory.Exists(directory))
        {
            return outputs;
        }

        string[] assemblies = ["AtomUI.Core", "AtomUI.Localization", "AtomUI.Generator", "AtomUI.Build.Tasks"];
        string[] suffixes = [".dll", ".pdb", ".deps.json", ".runtimeconfig.json"];
        foreach (var path in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
        {
            if (assemblies.Any(assembly => suffixes.Any(suffix => Path.GetFileName(path) == assembly + suffix)))
            {
                outputs.Add(Path.GetRelativePath(directory, path), Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))));
            }
        }
        return outputs;
    }

    [Fact(Timeout = 360_000)]
    public async Task Aggregate_Language_Pack_Dormantly_Consumes_Unreferenced_Module_Through_NuGet()
    {
        var repositoryRoot = FindRepositoryRoot();
        var fixtureRoot = Path.Combine(repositoryRoot, "tests", "fixtures", "LanguagePackEndToEnd");
        var temporaryRoot = Path.Combine(
            Path.GetTempPath(),
            "AtomUI.Localization.IntegrationTests",
            Guid.NewGuid().ToString("N"));
        var feed = Path.Combine(temporaryRoot, "feed");
        var packages = Path.Combine(temporaryRoot, "packages");
        var packageVersion = "1.0.0-local." + Guid.NewGuid().ToString("N");
        var globalPackages = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".nuget",
            "packages");
        Directory.CreateDirectory(feed);
        Directory.CreateDirectory(packages);

        try
        {
            await RunProcess(
                "Pack module",
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
            var verifiedPackResult = await RunProcess(
                "Pack language pack",
                repositoryRoot,
                temporaryRoot,
                "dotnet",
                "msbuild",
                Path.Combine(fixtureRoot, "LanguagePack", "LanguagePack.csproj"),
                "-restore",
                "-t:Pack",
                "-m:1",
                "-nr:false",
                "-p:Configuration=Debug",
                $"-p:PackageVersion={packageVersion}",
                $"-p:PackageOutputPath={feed}",
                $"-p:RestorePackagesPath={globalPackages}");
            verifiedPackResult.Output.ShouldNotContain("ATOMUILOC010");
            await RunProcess(
                "Pack optional module",
                repositoryRoot,
                temporaryRoot,
                "dotnet",
                "msbuild",
                Path.Combine(fixtureRoot, "OptionalModule", "OptionalModule.csproj"),
                "-restore",
                "-t:Pack",
                "-m:1",
                "-nr:false",
                "-p:Configuration=Debug",
                $"-p:PackageVersion={packageVersion}",
                $"-p:PackageOutputPath={feed}",
                $"-p:RestorePackagesPath={globalPackages}",
                "-p:NoPackageAnalysis=true");
            var deferredPackResult = await RunProcess(
                "Pack optional language pack",
                repositoryRoot,
                temporaryRoot,
                "dotnet",
                "msbuild",
                Path.Combine(fixtureRoot, "OptionalLanguagePack", "OptionalLanguagePack.csproj"),
                "-restore",
                "-t:Pack",
                "-m:1",
                "-nr:false",
                "-p:Configuration=Debug",
                $"-p:PackageVersion={packageVersion}",
                $"-p:PackageOutputPath={feed}",
                $"-p:RestorePackagesPath={globalPackages}");
            CountOccurrences(deferredPackResult.Output, "ATOMUILOC010").ShouldBe(1);
            await RunProcess(
                "Pack aggregate language pack",
                repositoryRoot,
                temporaryRoot,
                "dotnet",
                "msbuild",
                Path.Combine(fixtureRoot, "AggregateLanguagePack", "AggregateLanguagePack.csproj"),
                "-restore",
                "-t:Pack",
                "-m:1",
                "-nr:false",
                "-p:Configuration=Debug",
                $"-p:PackageVersion={packageVersion}",
                $"-p:PackageOutputPath={feed}",
                $"-p:RestorePackagesPath={globalPackages}");

            AssertPackageLayouts(feed, packageVersion);

            var consumerProject = Path.Combine(fixtureRoot, "Consumer", "Consumer.csproj");
            await RunProcess(
                "Restore consumer",
                repositoryRoot,
                temporaryRoot,
                "dotnet",
                "restore",
                consumerProject,
                "--disable-build-servers",
                "-m:1",
                "-nr:false",
                $"-p:FixturePackageVersion={packageVersion}",
                $"-p:RestoreAdditionalProjectSources={feed}",
                $"-p:RestorePackagesPath={packages}",
                $"-p:RestoreAdditionalProjectFallbackFolders={globalPackages}");
            var assetsFile = Path.Combine(
                temporaryRoot,
                "artifacts",
                "Consumer",
                "obj",
                "project.assets.json");
            var assets = File.ReadAllText(assetsFile);
            assets.ShouldContain($"Acme.OptionalComponent.I18n.JaJP/{packageVersion}");
            assets.ShouldNotContain($"Acme.OptionalComponent/{packageVersion}");
            assets.ShouldNotContain("Acme.OptionalComponent.dll");
            var generatedPropsFile = Path.Combine(
                Path.GetDirectoryName(assetsFile)!,
                "Consumer.csproj.nuget.g.props");
            var generatedImports = XDocument.Load(generatedPropsFile)
                .Descendants()
                .Where(static element =>
                    string.Equals(element.Name.LocalName, "Import", StringComparison.Ordinal))
                .Select(static import => (string?)import.Attribute("Project"))
                .Where(static project => project is not null)
                .Select(static project => project!.Replace('\\', '/'))
                .ToArray();
            generatedImports.ShouldContain(
                $"$(NuGetPackageRoot)/acme.optionalcomponent.i18n.jajp/{packageVersion}/" +
                "buildTransitive/Acme.OptionalComponent.I18n.JaJP.props");
            var consumerOutput = Path.Combine(temporaryRoot, "consumer-output");
            await RunProcess(
                "Build consumer",
                repositoryRoot,
                temporaryRoot,
                "dotnet",
                "build",
                consumerProject,
                "--no-restore",
                "--disable-build-servers",
                "-m:1",
                "-nr:false",
                "-p:Configuration=Debug",
                $"-p:OutputPath={consumerOutput}",
                $"-p:FixturePackageVersion={packageVersion}",
                $"-p:RestorePackagesPath={packages}",
                $"-p:RestoreAdditionalProjectFallbackFolders={globalPackages}");

            var consumerExecutable = Path.Combine(
                consumerOutput,
                OperatingSystem.IsWindows() ? "Consumer.exe" : "Consumer");
            Directory.EnumerateFiles(
                    Path.GetDirectoryName(consumerExecutable)!,
                    "Acme.OptionalComponent.dll",
                    SearchOption.TopDirectoryOnly)
                .ShouldBeEmpty();
            var runResult = await RunProcess(
                "Run consumer",
                repositoryRoot,
                temporaryRoot,
                consumerExecutable);
            runResult.Output.ShouldContain("ja-JP:Welcome=ようこそ;ItemCount=3 件の項目があります。");
        }
        finally
        {
            if (Directory.Exists(temporaryRoot))
            {
                Directory.Delete(temporaryRoot, recursive: true);
            }
        }
    }

    internal static void AssertRepositoryCoreAssemblyVersion(string repositoryRoot, string temporaryRoot)
    {
        var versionDocument = XDocument.Load(Path.Combine(repositoryRoot, "build", "Versions.props"));
        var atomUIVersion = versionDocument.Descendants("AtomUIVersion").Single().Value;
        var expectedVersion = new Version($"{atomUIVersion}.0");
        var coreAssemblyPath = Path.Combine(
            temporaryRoot,
            "artifacts",
            "bin",
            "Debug",
            "net10.0",
            "AtomUI.Core.dll");

        File.Exists(coreAssemblyPath).ShouldBeTrue($"Expected '{coreAssemblyPath}' to be built by the fixture.");
        System.Reflection.AssemblyName.GetAssemblyName(coreAssemblyPath).Version.ShouldBe(expectedVersion);
    }

    private static void AssertPackageLayouts(string feed, string packageVersion)
    {
        var modulePackage = Path.Combine(
            feed,
            $"Acme.LocalizationComponent.{packageVersion}.nupkg");
        var languagePackage = Path.Combine(
            feed,
            $"Acme.LocalizationComponent.I18n.JaJP.{packageVersion}.nupkg");
        var optionalModulePackage = Path.Combine(
            feed,
            $"Acme.OptionalComponent.{packageVersion}.nupkg");

        var moduleEntries = PackageEntries(modulePackage);
        moduleEntries.ShouldContain("buildTransitive/Acme.LocalizationComponent.props");
        moduleEntries.ShouldContain("contentFiles/any/any/Localization/Welcome/en-US.xlf");
        moduleEntries.ShouldContain("lib/net10.0/Module.dll");
        moduleEntries.ShouldContain("lib/net10.0/Module.pdb");
        moduleEntries.Count(IsBuildAsset).ShouldBe(1);
        moduleEntries.Count(IsRuntimeAsset).ShouldBe(2);
        moduleEntries.ShouldNotContain(static path => IsUnexpectedBuildOrRuntimeAsset(path));

        var moduleFingerprint = AssertLanguageProps(
            modulePackage,
            "buildTransitive/Acme.LocalizationComponent.props",
            "ModuleBuiltIn",
            "Localization/Welcome/en-US.xlf",
            "Verified");

        var languageEntries = PackageEntries(languagePackage);
        languageEntries.ShouldContain("buildTransitive/Acme.LocalizationComponent.I18n.JaJP.props");
        languageEntries.ShouldContain("contentFiles/any/any/AtomUI.LanguagePack.xml");
        languageEntries.ShouldContain("contentFiles/any/any/Welcome/ja-JP.xlf");
        languageEntries.Count(IsBuildAsset).ShouldBe(1);
        languageEntries.ShouldNotContain(static path => IsRuntimeAsset(path));
        languageEntries.ShouldNotContain(static path => IsUnexpectedBuildOrRuntimeAsset(path));

        var languageFingerprint = AssertLanguageProps(
            languagePackage,
            "buildTransitive/Acme.LocalizationComponent.I18n.JaJP.props",
            "StaticLanguagePack",
            "Welcome/ja-JP.xlf",
            "Verified");
        languageFingerprint.ShouldBe(moduleFingerprint);

        var manifest = XDocument.Parse(PackageEntryText(
            languagePackage,
            "contentFiles/any/any/AtomUI.LanguagePack.xml"));
        var manifestCatalog = manifest.Descendants("catalog").ShouldHaveSingleItem();
        manifestCatalog.Attribute("contractValidation").ShouldBeNull();
        manifestCatalog.Attribute("contractVersion").ShouldBeNull();
        ((string?)manifestCatalog.Attribute("sourceFingerprint")).ShouldBe(languageFingerprint);

        var optionalModuleEntries = PackageEntries(optionalModulePackage);
        optionalModuleEntries.ShouldContain("lib/net10.0/Acme.OptionalComponent.dll");
        optionalModuleEntries.ShouldNotContain("lib/net10.0/OptionalModule.dll");

        AssertOptionalLanguagePackageLayout(feed, packageVersion);
        AssertAggregatePackageLayout(feed, packageVersion);
    }

    private static void AssertOptionalLanguagePackageLayout(string feed, string packageVersion)
    {
        var optionalLanguagePackage = Path.Combine(
            feed,
            $"Acme.OptionalComponent.I18n.JaJP.{packageVersion}.nupkg");
        var optionalLanguageEntries = PackageEntries(optionalLanguagePackage);
        optionalLanguageEntries.ShouldContain(
            "buildTransitive/Acme.OptionalComponent.I18n.JaJP.props");
        optionalLanguageEntries.ShouldContain("contentFiles/any/any/AtomUI.LanguagePack.xml");
        optionalLanguageEntries.ShouldContain("contentFiles/any/any/Optional/ja-JP.xlf");
        optionalLanguageEntries.Count(IsBuildAsset).ShouldBe(1);
        optionalLanguageEntries.ShouldNotContain(static path => IsRuntimeAsset(path));
        optionalLanguageEntries.ShouldNotContain(static path => IsUnexpectedBuildOrRuntimeAsset(path));

        var dependencies = PackageDependencies(
            optionalLanguagePackage,
            "Acme.OptionalComponent.I18n.JaJP.nuspec");
        dependencies.ShouldNotContainKey("Acme.OptionalComponent");

        var fingerprint = AssertLanguageProps(
            optionalLanguagePackage,
            "buildTransitive/Acme.OptionalComponent.I18n.JaJP.props",
            "StaticLanguagePack",
            "Optional/ja-JP.xlf",
            "Deferred");
        var manifest = XDocument.Parse(PackageEntryText(
            optionalLanguagePackage,
            "contentFiles/any/any/AtomUI.LanguagePack.xml"));
        var catalog = manifest.Descendants("catalog").ShouldHaveSingleItem();
        catalog.Attribute("contractValidation").ShouldBeNull();
        catalog.Attribute("contractVersion").ShouldBeNull();
        ((string?)catalog.Attribute("sourceFingerprint")).ShouldBe(fingerprint);
    }

    private static void AssertAggregatePackageLayout(string feed, string packageVersion)
    {
        var aggregatePackage = Path.Combine(
            feed,
            $"Acme.LocalizationAggregate.I18n.JaJP.{packageVersion}.nupkg");
        var aggregateEntries = PackageEntries(aggregatePackage);
        aggregateEntries.ShouldAllBe(static path => IsAggregatePackageMetadataEntry(path));

        var dependencies = PackageDependencies(
            aggregatePackage,
            "Acme.LocalizationAggregate.I18n.JaJP.nuspec");
        dependencies.Count.ShouldBe(2);
        dependencies["Acme.LocalizationComponent.I18n.JaJP"].ShouldBe($"[{packageVersion}]");
        dependencies["Acme.OptionalComponent.I18n.JaJP"].ShouldBe($"[{packageVersion}]");
    }

    internal static void AssertExportedUnit(
        XElement unit,
        XNamespace xliff,
        string expectedSource)
    {
        unit.Attribute("name").ShouldBeNull();
        var segment = unit.Element(xliff + "segment").ShouldNotBeNull();
        segment.Element(xliff + "source").ShouldNotBeNull().Value.ShouldBe(expectedSource);
        var target = segment.Element(xliff + "target").ShouldNotBeNull();
        ((string?)target.Attribute("state")).ShouldBe("initial");
        target.Value.ShouldBeEmpty();
    }

    private static string AssertLanguageProps(
        string packagePath,
        string entryPath,
        string sourceKind,
        string packageLanguagePath,
        string contractValidation)
    {
        var props = XDocument.Parse(PackageEntryText(packagePath, entryPath));
        var item = props.Descendants("AtomUILanguage").ShouldHaveSingleItem();
        ((string?)item.Attribute("AtomUILanguageSourceKind")).ShouldBe(sourceKind);
        ((string?)item.Attribute("AtomUILanguagePackagePath")).ShouldBe(packageLanguagePath);
        ((string?)item.Attribute("AtomUILanguageContractValidation")).ShouldBe(contractValidation);
        var fingerprint = ((string?)item.Attribute("AtomUILanguageSourceFingerprint"))
            .ShouldNotBeNull();
        fingerprint.ShouldMatch("^[0-9a-f]{64}$");
        return fingerprint;
    }

    private static int CountOccurrences(string value, string fragment)
    {
        return value.Split(fragment, StringSplitOptions.None).Length - 1;
    }

    internal static string[] PackageEntries(string packagePath)
    {
        File.Exists(packagePath).ShouldBeTrue($"Expected package '{packagePath}' was not created.");
        using var archive = ZipFile.OpenRead(packagePath);
        return archive.Entries.Select(static entry => entry.FullName).ToArray();
    }

    internal static string PackageEntryText(string packagePath, string entryPath)
    {
        using var archive = ZipFile.OpenRead(packagePath);
        var entry = archive.GetEntry(entryPath).ShouldNotBeNull(
            $"Expected package '{packagePath}' to contain '{entryPath}'.");
        using var reader = new StreamReader(entry.Open());
        return reader.ReadToEnd();
    }

    internal static IReadOnlyDictionary<string, string> PackageDependencies(
        string packagePath,
        string nuspecPath)
    {
        var nuspec = XDocument.Parse(PackageEntryText(packagePath, nuspecPath));
        return nuspec.Descendants().Where(static element =>
                string.Equals(element.Name.LocalName, "dependency", StringComparison.Ordinal))
            .ToDictionary(
            static dependency => ((string?)dependency.Attribute("id")).ShouldNotBeNull(),
            static dependency => ((string?)dependency.Attribute("version")).ShouldNotBeNull(),
            StringComparer.Ordinal);
    }

    internal static bool IsBuildAsset(string path)
    {
        return path.StartsWith("build/", StringComparison.OrdinalIgnoreCase) ||
               path.StartsWith("buildMultiTargeting/", StringComparison.OrdinalIgnoreCase) ||
               path.StartsWith("buildTransitive/", StringComparison.OrdinalIgnoreCase);
    }

    internal static bool IsRuntimeAsset(string path)
    {
        return path.StartsWith("lib/", StringComparison.OrdinalIgnoreCase) ||
               path.StartsWith("runtimes/", StringComparison.OrdinalIgnoreCase) ||
               path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) ||
               path.EndsWith(".pdb", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsAggregatePackageMetadataEntry(string path)
    {
        const string corePropertiesPrefix = "package/services/metadata/core-properties/";

        if (path.Equals("_rels/.rels", StringComparison.Ordinal) ||
            path.Equals("[Content_Types].xml", StringComparison.Ordinal) ||
            path.Equals(
                "Acme.LocalizationAggregate.I18n.JaJP.nuspec",
                StringComparison.Ordinal))
        {
            return true;
        }

        if (!path.StartsWith(corePropertiesPrefix, StringComparison.Ordinal) ||
            !path.EndsWith(".psmdcp", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var fileName = path[corePropertiesPrefix.Length..];
        return fileName.Equals("nuget.psmdcp", StringComparison.Ordinal) ||
               (fileName.IndexOf('/') < 0 &&
                Guid.TryParse(fileName[..^".psmdcp".Length], out _));
    }

    internal static bool IsUnexpectedBuildOrRuntimeAsset(string path)
    {
        return path.StartsWith("analyzers/", StringComparison.OrdinalIgnoreCase) ||
               path.StartsWith("tools/", StringComparison.OrdinalIgnoreCase) ||
               path.EndsWith(".targets", StringComparison.OrdinalIgnoreCase) ||
               path.Contains("AtomUI.Build.Tasks", StringComparison.OrdinalIgnoreCase) ||
               path.Contains("AtomUI.Generator", StringComparison.OrdinalIgnoreCase);
    }

    private static string CreateIsolatedBuildProps(string repositoryRoot, string temporaryRoot)
    {
        var propsPath = Path.Combine(temporaryRoot, "IsolatedDirectory.Build.props");
        if (File.Exists(propsPath))
        {
            return propsPath;
        }

        var artifactsRoot = Path.Combine(temporaryRoot, "artifacts");
        var repositoryPrefix = Path.TrimEndingDirectorySeparator(repositoryRoot) + Path.DirectorySeparatorChar;
        var repositoryCondition = $"$([System.String]::Copy('$(MSBuildProjectFullPath)').StartsWith('{repositoryPrefix}'))";
        new XDocument(new XElement("Project",
            new XElement("Import",
                new XAttribute("Project", Path.Combine(repositoryRoot, "Directory.Build.props")),
                new XAttribute("Condition", repositoryCondition)),
            new XElement("PropertyGroup", new XAttribute("Condition", repositoryCondition),
                new XElement("PackageOutputPath", Path.Combine(artifactsRoot, "Nuget", "$(Configuration)")),
                new XElement("OutputPathWithoutFramework", Path.Combine(artifactsRoot, "bin", "$(Configuration)")),
                new XElement("OutputPath", "$(OutputPathWithoutFramework)"),
                new XElement("BaseIntermediateOutputPath", Path.Combine(artifactsRoot, "$(MSBuildProjectName)", "obj") + Path.DirectorySeparatorChar),
                new XElement("BaseIntermediateOutputPath",
                    new XAttribute("Condition", "'$(MSBuildProjectName)' == 'AtomUI.Toolchain'"),
                    "$(BaseIntermediateOutputPath)$(AtomUIToolchainProfile)/"),
                new XElement("IntermediateOutputPathWithoutFramework", "$(BaseIntermediateOutputPath)"),
                new XElement("IntermediateOutputPath", "$(BaseIntermediateOutputPath)$(Configuration)/"),
                // Repository.props computed these properties before the test path overrides.
                // Its tool asset Items are evaluated afterwards and use the new output root.
                new XElement("AtomUIToolchainOutputDirectory", "$(OutputPathWithoutFramework)/toolchain"),
                new XElement("AtomUIBuildTasksOutputDirectory", "$(AtomUIToolchainOutputDirectory)/tasks"),
                new XElement("AtomUITypeMapBackendOutputDirectory", "$(AtomUIToolchainOutputDirectory)/illink10/net10.0"),
                new XElement("AtomUINet8LinkerBackendAssembly", "$(AtomUIToolchainOutputDirectory)/illink8/net10.0/AtomUI.Registration.ILLink8.dll"),
                new XElement("AtomUIConditionalBridgeAssembly", "$(AtomUITypeMapBackendOutputDirectory)/AtomUI.TypeMap.Linker.dll"),
                new XElement("AtomUINative8ManagedCompilerBundle", Path.Combine(artifactsRoot, "tools", "registration", "net8", "ilc", "managed-bundle")),
                new XElement("AtomUIBuildTasksAssembly", "$(AtomUIBuildTasksOutputDirectory)/net10.0/AtomUI.Build.Tasks.dll"),
                new XElement("AtomUIToolsetCacheRoot", Path.Combine(artifactsRoot, "tools")))))
            .Save(propsPath);
        return propsPath;
    }

    internal static async Task<ProcessResult> RunProcess(
        string stage,
        string workingDirectory,
        string temporaryRoot,
        string executable,
        params string[] arguments)
    {
        var result = await RunProcessUnchecked(
            stage,
            workingDirectory,
            temporaryRoot,
            executable,
            arguments);
        result.ExitCode.ShouldBe(0, FormatFailure(result, "failed"));
        return result;
    }

    internal static async Task<ProcessResult> RunProcessUnchecked(
        string stage,
        string workingDirectory,
        string temporaryRoot,
        string executable,
        params string[] arguments)
    {
        if (executable == "dotnet")
        {
            // The global override follows ProjectReferences; external consumer projects
            // keep their defaults while repository projects use isolated output paths.
            arguments = [.. arguments, $"-p:DirectoryBuildPropsPath={CreateIsolatedBuildProps(workingDirectory, temporaryRoot)}"];
        }
        var startInfo = new ProcessStartInfo(executable)
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        startInfo.Environment["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1";
        startInfo.Environment["DOTNET_NOLOGO"] = "1";
        startInfo.Environment["DOTNET_CLI_HOME"] = Path.Combine(temporaryRoot, "cli");
        startInfo.Environment["NUGET_PACKAGES"] = Path.Combine(temporaryRoot, "packages");
        startInfo.Environment["DOTNET_CLI_USE_MSBUILD_SERVER"] = "0";
        startInfo.Environment["MSBUILDUSESERVER"] = "0";
        startInfo.Environment["UseSharedCompilation"] = "false";
        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        var command = FormatCommand(executable, arguments);
        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException(
            $"The '{stage}' process could not be started.");
        var stopwatch = Stopwatch.StartNew();
        var stageId = Guid.NewGuid().ToString("N");
        WriteProcessStageTrace("start", stageId, stage, command, process.Id, TimeSpan.Zero, null);
        var standardOutput = process.StandardOutput.ReadToEndAsync();
        var standardError = process.StandardError.ReadToEndAsync();
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(
            TestContext.Current.CancellationToken);
        timeout.CancelAfter(executable == "dotnet" && arguments.FirstOrDefault() == "build"
            ? s_buildStageTimeout
            : s_stageTimeout);
        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
            await process.WaitForExitAsync();
            var timedOutStandardOutput = await standardOutput;
            var timedOutStandardError = await standardError;
            var outcome = TestContext.Current.CancellationToken.IsCancellationRequested
                ? "was cancelled"
                : "timed out";
            WriteProcessStageTrace(outcome == "was cancelled" ? "cancelled" : "timed_out",
                stageId, stage, command, process.Id, stopwatch.Elapsed, null);
            throw new TimeoutException(
                FormatFailure(
                    stage,
                    command,
                    stopwatch.Elapsed,
                    exitCode: null,
                    timedOutStandardOutput,
                    timedOutStandardError,
                    outcome));
        }

        var result = new ProcessResult(
            stage,
            command,
            process.Id,
            stopwatch.Elapsed,
            process.ExitCode,
            await standardOutput,
            await standardError);
        WriteProcessStageTrace("completed", stageId, stage, command, process.Id, result.Elapsed, result.ExitCode);
        return result;
    }

    private static void WriteProcessStageTrace(
        string eventName, string stageId, string stage, string command, int processId,
        TimeSpan elapsed, int? exitCode)
    {
        var configured = Environment.GetEnvironmentVariable("ATOMUI_LOCALIZATION_STAGE_TRACE");
        if (string.IsNullOrWhiteSpace(configured))
        {
            return;
        }
        try
        {
            var path = Path.GetFullPath(configured);
            var entry = JsonSerializer.Serialize(new
            {
                version = 1,
                @event = eventName,
                stage_id = stageId,
                stage,
                command,
                process_id = processId,
                time_utc = DateTimeOffset.UtcNow,
                elapsed_seconds = elapsed.TotalSeconds,
                exit_code = exitCode
            });
            lock (s_stageTraceLock)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.AppendAllText(path, entry + Environment.NewLine);
            }
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            // Optional diagnostics must preserve the original child result or timeout.
            Console.Error.WriteLine($"Stage journal could not be written: {error.Message}");
        }
    }

    private static string FormatCommand(string executable, IReadOnlyList<string> arguments)
    {
        return executable + " " + string.Join(' ', arguments);
    }

    internal static string FormatFailure(ProcessResult result, string outcome)
    {
        return FormatFailure(
            result.Stage,
            result.Command,
            result.Elapsed,
            result.ExitCode,
            result.StandardOutput,
            result.StandardError,
            outcome);
    }

    private static string FormatFailure(
        string stage,
        string command,
        TimeSpan elapsed,
        int? exitCode,
        string standardOutput,
        string standardError,
        string outcome)
    {
        return $"{stage} {outcome} after {elapsed.TotalSeconds:F1}s.{Environment.NewLine}" +
               $"Command: {command}{Environment.NewLine}" +
               $"Exit code: {exitCode?.ToString() ?? "not available"}{Environment.NewLine}" +
               $"stdout:{Environment.NewLine}{standardOutput}{Environment.NewLine}" +
               $"stderr:{Environment.NewLine}{standardError}";
    }

    internal static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AtomUI.slnx")))
            {
                return directory.FullName;
            }
            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("The AtomUI repository root could not be located.");
    }

    private static string GlobalPackagesPath()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".nuget",
            "packages");
    }

    internal sealed record ProcessResult(
        string Stage,
        string Command,
        int ProcessId,
        TimeSpan Elapsed,
        int ExitCode,
        string StandardOutput,
        string StandardError)
    {
        public string Output => StandardOutput + StandardError;
    }
}
