using System.Diagnostics;
using System.Xml.Linq;
using Shouldly;
using Xunit;

namespace AtomUI.Generator.Tests.Localization;

public class LocalizationBuildAssetsTests
{
    [Fact]
    public void Localization_Props_Defines_Module_Identity_And_Item_Defaults()
    {
        var props = XDocument.Load(GetRepoFile("build/AtomUI.Localization.props"));
        var propertyGroups = props.Root!.Elements()
                                  .Where(element => element.Name.LocalName == "PropertyGroup")
                                  .ToArray();

        propertyGroups.SelectMany(static group => group.Elements())
                      .Single(element =>
                          element.Name.LocalName == "AtomUILanguageModuleId" &&
                          element.Value == "$(PackageId)")
                      .ShouldNotBeNull();
        propertyGroups.SelectMany(static group => group.Elements())
             .Where(element => element.Name.LocalName == "AtomUILanguageModuleId")
             .Any(element =>
                 element.Value == "$(AssemblyName)" &&
                 (string?)element.Attribute("Condition") == "'$(AtomUILanguageModuleId)' == ''")
             .ShouldBeTrue();
        propertyGroups.SelectMany(static group => group.Elements())
                      .ShouldNotContain(element =>
                          element.Name.LocalName == "AtomUILanguageMinimumState");

        var languageDefaults = props.Descendants()
                                    .Single(element => element.Name.LocalName == "AtomUILanguage");
        languageDefaults.Elements()
                        .Single(element => element.Name.LocalName == "AtomUILanguageSourceKind")
                        .Value.ShouldBe("ModuleBuiltIn");
        languageDefaults.Elements()
                        .Single(element => element.Name.LocalName == "AtomUILanguageSourceIdentity")
                        .Value.ShouldBe("$(AtomUILanguageModuleId)");
        languageDefaults.Elements()
                        .Single(element => element.Name.LocalName == "AtomUILanguageModuleId")
                        .Value.ShouldBe("$(AtomUILanguageModuleId)");

        var overrideDefaults = props.Descendants()
                                    .Single(element => element.Name.LocalName == "AtomUILanguageOverride");
        overrideDefaults.Elements()
                        .Single(element => element.Name.LocalName == "AtomUILanguageSourceKind")
                        .Value.ShouldBe("ApplicationOverride");
    }

    [Fact]
    public void Generator_Entry_Points_Import_Localization_Build_Assets()
    {
        var props = XDocument.Load(GetRepoFile("build/AtomUI.Generator.props"));
        var targets = XDocument.Load(GetRepoFile("build/AtomUI.Generator.targets"));

        props.Descendants("Import")
             .Single(element =>
                 (string?)element.Attribute("Project") ==
                 "$(MSBuildThisFileDirectory)AtomUI.Localization.props")
             .ShouldNotBeNull();
        targets.Descendants("Import")
               .Single(element =>
                   (string?)element.Attribute("Project") ==
                   "$(MSBuildThisFileDirectory)AtomUI.Localization.targets")
               .ShouldNotBeNull();
    }

    [Fact]
    public void Localization_Targets_Discovers_Xliff_And_Exposes_Generator_Metadata()
    {
        var targets = XDocument.Load(GetRepoFile("build/AtomUI.Localization.targets"));
        var templateOutputRoot = targets.Descendants()
                                        .Single(element =>
                                            element.Name.LocalName ==
                                            "AtomUILanguageTemplateOutputRootDirectory");
        templateOutputRoot.Value.ShouldBe("$(MSBuildProjectDirectory)/Localization");
        ((string?)templateOutputRoot.Attribute("Condition"))
            .ShouldBe("'$(AtomUILanguageTemplateOutputRootDirectory)' == ''");
        var discoveredLanguages = targets.Descendants()
                                         .Where(element =>
                                             element.Name.LocalName == "AtomUILanguage" &&
                                             element.Attribute("Include") is not null)
                                         .ToArray();
        var discoveredModuleLanguage = discoveredLanguages.Single(element =>
            (string?)element.Attribute("Condition") == "'$(AtomUIBuildLanguagePackage)' != 'true'");
        var discoveredPackLanguage = discoveredLanguages.Single(element =>
            (string?)element.Attribute("Condition") == "'$(AtomUIBuildLanguagePackage)' == 'true'");
        var include = (string?)discoveredModuleLanguage.Attribute("Include");
        var exclude = (string?)discoveredModuleLanguage.Attribute("Exclude");
        var fileExcludes = string.Join(
            ";",
            targets.Descendants()
                   .Where(element => element.Name.LocalName == "_AtomUILanguageFileExcludes")
                   .Select(static element => element.Value));

        include.ShouldBe("$(MSBuildProjectDirectory)/**/Localization/**/*.xlf");
        exclude.ShouldNotBeNull();
        exclude.ShouldContain("$(_AtomUILanguageFileExcludes)");
        exclude.ShouldContain("@(AtomUILanguageOverride)");
        fileExcludes.ShouldContain("$(BaseOutputPath)");
        fileExcludes.ShouldContain("$(BaseIntermediateOutputPath)");
        fileExcludes.ShouldContain("GeneratedFiles");
        ((string?)discoveredPackLanguage.Attribute("Include"))
            .ShouldBe("$(MSBuildProjectDirectory)/Localization/**/*.xlf");
        discoveredPackLanguage.Elements()
                              .Single(element =>
                                  element.Name.LocalName == "AtomUILanguageSourceKind")
                              .Value.ShouldBe("StaticLanguagePack");
        discoveredPackLanguage.Elements()
                              .Single(element =>
                                  element.Name.LocalName == "AtomUILanguageSourceIdentity")
                              .Value.ShouldBe("$(PackageId)");
        discoveredPackLanguage.Elements()
                              .Single(element =>
                                  element.Name.LocalName == "AtomUILanguageModuleId")
                              .Value.ShouldBe("$(AtomUILanguageModuleId)");
        var additionalLanguageFiles = targets.Descendants()
                                             .Single(element =>
                                                 element.Name.LocalName == "_AtomUIAdditionalLanguageFile");
        ((string?)additionalLanguageFiles.Attribute("Include"))
            .ShouldBe("@(AtomUILanguage);@(AtomUILanguageOverride)");

        var additionalFiles = targets.Descendants()
                                     .Single(element =>
                                         element.Name.LocalName == "AdditionalFiles" &&
                                         (string?)element.Attribute("Include") ==
                                         "@(_AtomUIAdditionalLanguageFile)");
        ((string?)additionalFiles.Attribute("AtomUILanguage")).ShouldBe("true");
        ((string?)additionalFiles.Attribute("Visible")).ShouldBe("false");
        AssertKeepsGeneratorMetadata(additionalFiles);

        var visibleMetadata = targets.Descendants()
                                     .Where(element =>
                                         element.Name.LocalName == "CompilerVisibleItemMetadata" &&
                                         (string?)element.Attribute("Include") == "AdditionalFiles")
                                     .Select(element => (string?)element.Attribute("MetadataName"))
                                     .ToArray();
        visibleMetadata.ShouldBe(
        [
            "AtomUILanguage",
            "AtomUILanguageSourceKind",
            "AtomUILanguageSourceIdentity",
            "AtomUILanguageModuleId",
            "AtomUILanguageContractValidation",
            "AtomUILanguageSourceFingerprint"
        ],
            ignoreOrder: true);

        var visibleProperties = targets.Descendants()
                                       .Where(element => element.Name.LocalName == "CompilerVisibleProperty")
                                       .Select(element => (string?)element.Attribute("Include"))
                                       .ToArray();
        visibleProperties.ShouldBe(
            ["PackageId", "AssemblyName", "AtomUILanguageModuleId", "AtomUIBuildLanguagePackage"],
            ignoreOrder: true);

        targets.Descendants("Import").ShouldContain(element =>
            (string?)element.Attribute("Project") == "$(MSBuildThisFileDirectory)AtomUI.Toolchain.Tasks.targets");
        var invokedTasks = targets.Descendants().Select(element => element.Name.LocalName).ToHashSet();
        var usingTasks = XDocument.Load(GetRepoFile("build/AtomUI.Toolchain.Tasks.targets"))
                                .Descendants("UsingTask")
                                .Where(element => invokedTasks.Contains((string?)element.Attribute("TaskName") ?? string.Empty))
                                .Select(element => (string?)element.Attribute("TaskName"))
                                .ToArray();
        usingTasks.ShouldBe(
        [
            "AtomUI.Build.Tasks.ExportLanguageTemplatesTask",
            "AtomUI.Build.Tasks.PrepareLanguagePackageAssetsTask",
            "AtomUI.Build.Tasks.PrepareLanguagePackageTask",
            "AtomUI.Build.Tasks.GenerateLanguagePackagePropsTask"
        ],
            ignoreOrder: true);
        targets.Descendants("Target")
               .ShouldNotContain(element =>
                   (string?)element.Attribute("Name") == "AtomUIValidateLanguageFiles");
        targets.Descendants("Target")
               .ShouldNotContain(element =>
                   (string?)element.Attribute("Name") == "AtomUICollectLanguageCatalogs");
        targets.Descendants("_AtomUILanguageValidationMinimumState").ShouldBeEmpty();
        var prepareTask = targets.Descendants("Target")
                                 .Single(element =>
                                     (string?)element.Attribute("Name") == "AtomUIPrepareLanguagePackage")
                                 .Descendants()
                                 .Single(element =>
                                     element.Name.LocalName == "AtomUI.Build.Tasks.PrepareLanguagePackageTask");
        ((string?)prepareTask.Attribute("MinimumTargetState"))
            .ShouldBe("final");
        ((string?)prepareTask.Attribute("SourceLanguageFiles"))
            .ShouldBe("@(_AtomUILanguagePackageSourceFile)");
        ((string?)prepareTask.Attribute("RequireVerifiedContract"))
            .ShouldBe("$(_AtomUIRequireAuthoritativeLanguageContract)");
        var exportTarget = targets.Descendants("Target")
                                  .Single(element =>
                                      (string?)element.Attribute("Name") ==
                                  "AtomUIExportLanguageTemplates");
        ((string?)exportTarget.Attribute("DependsOnTargets"))
            .ShouldBe("AtomUIResolveLanguageContractProjectReferences");
        ((string?)exportTarget.Descendants()
                              .Single(element =>
                                  element.Name.LocalName ==
                                  "AtomUI.Build.Tasks.ExportLanguageTemplatesTask")
                              .Attribute("OutputRootDirectory"))
            .ShouldBe("$(AtomUILanguageTemplateOutputRootDirectory)");
    }

    [Fact]
    public void Localization_Targets_Resolve_Authoring_Project_References_As_Source_Contracts()
    {
        var targets = XDocument.Load(GetRepoFile("build/AtomUI.Localization.targets"));

        var configuredReference = targets.Root!
                                         .Elements("ItemGroup")
                                         .SelectMany(static group => group.Elements("ProjectReference"))
                                         .Single(element =>
                                             ((string?)element.Attribute("Update"))?.Contains(
                                                 "WithMetadataValue",
                                                 StringComparison.Ordinal) == true);
        var configuration = configuredReference.Parent.ShouldNotBeNull();
        ((string?)configuration.Attribute("Condition"))
            .ShouldNotBeNull()
            .ShouldContain("'$(AtomUIBuildLanguagePackage)' == 'true'");
        ((string?)configuredReference.Attribute("Update"))
            .ShouldNotBeNull()
            .ShouldContain("OutputItemType");
        configuredReference.Element("BuildReference").ShouldNotBeNull().Value.ShouldBe("false");
        configuredReference.Element("ReferenceOutputAssembly")
                           .ShouldNotBeNull()
                           .Value.ShouldBe("false");
        configuredReference.Element("PrivateAssets").ShouldNotBeNull().Value.ShouldBe("all");
        configuredReference.Element("SkipGetTargetFrameworkProperties")
                           .ShouldNotBeNull()
                           .Value.ShouldBe("true");

        var provider = targets.Descendants("Target")
                              .Single(element =>
                                  (string?)element.Attribute("Name") ==
                                  "AtomUIGetLanguageModuleSourceAssets");
        ((string?)provider.Attribute("Returns"))
            .ShouldBe("@(_AtomUILanguageModuleSourceAsset)");
        provider.Descendants("_AtomUILanguageModuleSourceAsset").ShouldHaveSingleItem();

        var resolver = targets.Descendants("Target")
                              .Single(element =>
                                  (string?)element.Attribute("Name") ==
                                  "AtomUIResolveLanguageContractProjectReferences");
        ((string?)resolver.Attribute("Condition"))
            .ShouldNotBeNull()
            .ShouldContain("'$(AtomUIBuildLanguagePackage)' == 'true'");
        var msbuild = resolver.Descendants("MSBuild").ShouldHaveSingleItem();
        ((string?)msbuild.Attribute("Projects")).ShouldBe("@(ProjectReference)");
        ((string?)msbuild.Attribute("Targets")).ShouldBe("AtomUIGetLanguageModuleSourceAssets");
        msbuild.Descendants("Output")
               .Single(element =>
                   (string?)element.Attribute("TaskParameter") == "TargetOutputs")
               .Attribute("ItemName")!.Value.ShouldBe("_AtomUIResolvedLanguageContractAsset");

        var imported = resolver.Descendants("AtomUILanguage").ShouldHaveSingleItem();
        ((string?)imported.Attribute("Include"))
            .ShouldBe("@(_AtomUIResolvedLanguageContractAsset)");
        AssertKeepsGeneratorMetadata(imported, includePackagePath: true);
    }

    [Fact]
    public void Localization_Targets_Infer_Strict_Contract_Validation_From_Normal_Project_References()
    {
        var targets = XDocument.Load(GetRepoFile("build/AtomUI.Localization.targets"));

        var policyTarget = targets.Descendants("Target")
                                  .Single(element =>
                                      (string?)element.Attribute("Name") ==
                                      "AtomUIInferLanguagePackageContractPolicy");
        ((string?)policyTarget.Attribute("BeforeTargets"))
            .ShouldNotBeNull()
            .ShouldContain("AtomUIPrepareLanguagePackage");
        var policy = policyTarget.Elements("PropertyGroup").ShouldHaveSingleItem();
        policy.Elements("_AtomUIRequireAuthoritativeLanguageContract")
              .Select(element => (string?)element.Attribute("Condition"))
              .ShouldContain("'@(ProjectReference->WithMetadataValue('OutputItemType', ''))' != ''");
    }

    [Fact]
    public void Localization_Targets_Define_The_Language_Pack_Project_Reference_Protocol()
    {
        var targets = XDocument.Load(GetRepoFile("build/AtomUI.Localization.targets"));

        var provider = targets.Descendants("Target")
                              .Single(element =>
                                  (string?)element.Attribute("Name") ==
                                  "AtomUIGetLanguagePackProjectAssets");
        ((string?)provider.Attribute("Condition"))
            .ShouldBe("'$(AtomUIBuildLanguagePackage)' == 'true'");
        ((string?)provider.Attribute("Returns"))
            .ShouldBe("@(_AtomUILanguagePackProjectAsset)");

        var prepare = provider.Descendants()
                              .Single(element =>
                                  element.Name.LocalName ==
                                  "AtomUI.Build.Tasks.PrepareLanguagePackageAssetsTask");
        ((string?)prepare.Attribute("PackageId")).ShouldBe("$(PackageId)");
        ((string?)prepare.Attribute("ExpectedLanguage")).ShouldBe("$(AtomUILanguageTag)");
        ((string?)prepare.Attribute("MinimumTargetState"))
            .ShouldBe("final");
        ((string?)prepare.Attribute("SourceLanguageFiles"))
            .ShouldBe("@(_AtomUILanguagePackProjectSourceFile)");
        ((string?)prepare.Attribute("RequireVerifiedContract"))
            .ShouldBe("$(_AtomUIRequireAuthoritativeLanguageContract)");
        ((string?)prepare.Attribute("LanguageFiles"))
            .ShouldBe("@(_AtomUILanguagePackProjectTargetFile)");
        prepare.Attribute("PackageFiles").ShouldBeNull();
        prepare.Attribute("OutputManifestPath").ShouldBeNull();
        prepare.Elements("Output")
               .Single(element =>
                   (string?)element.Attribute("TaskParameter") == "PreparedLanguageFiles")
               .Attribute("ItemName")!.Value.ShouldBe("_AtomUIPreparedLanguagePackProjectTargetFile");

        var returnedAssets = provider.Descendants("_AtomUILanguagePackProjectAsset").ToArray();
        returnedAssets.Length.ShouldBe(2);
        var returnedSource = returnedAssets.Single(element =>
            ((string?)element.Attribute("Include"))?.Contains(
                "_AtomUILanguagePackProjectSourceFile",
                StringComparison.Ordinal) == true);
        AssertKeepsGeneratorMetadata(returnedSource);
        var returnedTarget = returnedAssets.Single(element =>
            ((string?)element.Attribute("Include"))?.Contains(
                "_AtomUIPreparedLanguagePackProjectTargetFile",
                StringComparison.Ordinal) == true);
        AssertKeepsGeneratorMetadata(returnedTarget);

        var resolver = targets.Descendants("Target")
                              .Single(element =>
                                  (string?)element.Attribute("Name") ==
                                  "AtomUIResolveLanguagePackProjectReferences");
        var beforeTargets = ((string?)resolver.Attribute("BeforeTargets"))!
            .Split(';', StringSplitOptions.RemoveEmptyEntries);
        beforeTargets.ShouldContain("GenerateMSBuildEditorConfigFileShouldRun");
        beforeTargets.ShouldContain("CoreCompile");
        beforeTargets.Length.ShouldBe(2);

        var msbuild = resolver.Descendants("MSBuild").ShouldHaveSingleItem();
        ((string?)msbuild.Attribute("Projects"))
            .ShouldBe("@(AtomUILanguagePackProjectReference)");
        ((string?)msbuild.Attribute("Targets")).ShouldBe("AtomUIGetLanguagePackProjectAssets");
        msbuild.Elements("Output")
               .Single(element =>
                   (string?)element.Attribute("TaskParameter") == "TargetOutputs")
               .Attribute("ItemName")!.Value.ShouldBe("_AtomUIResolvedLanguagePackProjectAsset");

        var additionalFiles = resolver.Descendants("AdditionalFiles").ShouldHaveSingleItem();
        ((string?)additionalFiles.Attribute("Include"))
            .ShouldBe("@(_AtomUIResolvedLanguagePackProjectAsset)");
        ((string?)additionalFiles.Attribute("AtomUILanguage")).ShouldBe("true");
        ((string?)additionalFiles.Attribute("Visible")).ShouldBe("false");
        AssertKeepsGeneratorMetadata(additionalFiles);
    }

    [Fact]
    public async Task Project_Referenced_Language_Pack_Contributes_A_Generated_Bundle()
    {
        var repoRoot = Path.GetDirectoryName(GetRepoFile("AtomUI.slnx"))!;
        var configuration = typeof(LocalizationBuildAssetsTests).Assembly
            .GetCustomAttributes(typeof(System.Reflection.AssemblyConfigurationAttribute), false)
            .Cast<System.Reflection.AssemblyConfigurationAttribute>().Single().Configuration;
        var buildTasksAssembly = typeof(LocalizationBuildAssetsTests).Assembly
            .GetCustomAttributes(typeof(System.Reflection.AssemblyMetadataAttribute), false)
            .Cast<System.Reflection.AssemblyMetadataAttribute>()
            .Single(attribute => attribute.Key == "FixtureBuildTasksAssembly").Value!;
        var generatorAssembly = Path.Combine(AppContext.BaseDirectory, "AtomUI.Generator.dll");
        File.Exists(buildTasksAssembly).ShouldBeTrue();
        File.Exists(generatorAssembly).ShouldBeTrue();

        var fixtureRoot = Path.Combine(
            Path.GetTempPath(),
            $"atomui-language-pack-project-reference-{Guid.NewGuid():N}");
        Directory.CreateDirectory(fixtureRoot);
        try
        {
            var moduleDirectory = Path.Combine(fixtureRoot, "Module");
            var packDirectory = Path.Combine(fixtureRoot, "Pack");
            var consumerDirectory = Path.Combine(fixtureRoot, "Consumer");
            Directory.CreateDirectory(moduleDirectory);
            Directory.CreateDirectory(packDirectory);
            Directory.CreateDirectory(consumerDirectory);

            WriteModuleProject(
                Path.Combine(moduleDirectory, "Module.csproj"),
                repoRoot);
            await File.WriteAllTextAsync(
                Path.Combine(moduleDirectory, "Runtime.cs"),
                FixtureRuntimeSource,
                TestContext.Current.CancellationToken);

            var sourceXliffPath = Path.Combine(moduleDirectory, "Localization", "en-US.xlf");
            var targetXliffPath = Path.Combine(packDirectory, "Localization", "pt-BR.xlf");
            Directory.CreateDirectory(Path.GetDirectoryName(sourceXliffPath)!);
            Directory.CreateDirectory(Path.GetDirectoryName(targetXliffPath)!);
            await File.WriteAllTextAsync(
                sourceXliffPath,
                FixtureSourceXliff,
                TestContext.Current.CancellationToken);
            await File.WriteAllTextAsync(
                targetXliffPath,
                FixtureTargetXliff,
                TestContext.Current.CancellationToken);

            WriteLanguagePackProject(
                Path.Combine(packDirectory, "Pack.csproj"),
                repoRoot,
                buildTasksAssembly,
                Path.Combine(moduleDirectory, "Module.csproj"));
            WriteConsumerProject(
                Path.Combine(consumerDirectory, "Consumer.csproj"),
                repoRoot,
                generatorAssembly,
                Path.Combine(moduleDirectory, "Module.csproj"),
                Path.Combine(packDirectory, "Pack.csproj"));
            await File.WriteAllTextAsync(
                Path.Combine(consumerDirectory, "App.cs"),
                "namespace Fixture.App; public partial class App : Avalonia.Application { }",
                TestContext.Current.CancellationToken);

            var result = await RunDotNetBuildAsync(
                Path.Combine(consumerDirectory, "Consumer.csproj"),
                configuration);

            result.ExitCode.ShouldBe(0, result.Output);
            var generatedBootstrap = Directory.EnumerateFiles(
                                                  Path.Combine(consumerDirectory, "obj", "Generated"),
                                                  "GeneratedApplicationLanguageData.g.cs",
                                                  SearchOption.AllDirectories)
                                              .ShouldHaveSingleItem();
            var generatedSource = await File.ReadAllTextAsync(
                generatedBootstrap,
                TestContext.Current.CancellationToken);
            generatedSource.ShouldContain("builder.AddModuleData(");
            generatedSource.ShouldContain("Fixture.Module.I18n.PtBR");
            generatedSource.ShouldContain("Cancelar");
        }
        finally
        {
            Directory.Delete(fixtureRoot, recursive: true);
        }
    }

    [Fact]
    public void Localization_Targets_Registers_Build_Tasks_Before_Source_Outputs_Exist()
    {
        var targets = XDocument.Load(GetRepoFile("build/AtomUI.Localization.targets"));
        targets.Descendants("Import").ShouldContain(element =>
            (string?)element.Attribute("Project") == "$(MSBuildThisFileDirectory)AtomUI.Toolchain.Tasks.targets");
        var usingTasks = XDocument.Load(GetRepoFile("build/AtomUI.Toolchain.Tasks.targets"))
                                .Descendants("UsingTask")
                                .Where(element =>
                                    ((string?)element.Attribute("TaskName"))?.StartsWith(
                                        "AtomUI.Build.Tasks.",
                                        StringComparison.Ordinal) == true)
                                .ToArray();

        usingTasks.ShouldNotBeEmpty();
        foreach (var usingTask in usingTasks)
        {
            ((string?)usingTask.Attribute("AssemblyFile"))
                .ShouldBe("$(MSBuildToolsPath)/Microsoft.Build.Tasks.Core.dll");
            usingTask.Attribute("Condition").ShouldBeNull();
        }
    }

    [Fact]
    public void Module_Package_Exports_Only_The_Authoritative_EnUs_Catalog_Sources()
    {
        var targets = XDocument.Load(GetRepoFile("build/AtomUI.Localization.targets"));
        var moduleLanguageFiles = targets.Descendants()
                                         .Single(element =>
                                             element.Name.LocalName == "_AtomUIModuleLanguageFile");

        ((string?)moduleLanguageFiles.Attribute("Include")).ShouldBe("@(AtomUILanguage)");
        var condition = (string?)moduleLanguageFiles.Attribute("Condition");
        condition.ShouldNotBeNull();
        condition.ShouldContain("'%(AtomUILanguageSourceKind)' == 'ModuleBuiltIn'");
        condition.ShouldContain("'%(AtomUILanguageSourceIdentity)' == '$(AtomUILanguageModuleId)'");
        condition.ShouldContain("'%(Filename)%(Extension)' == 'en-US.xlf'");
    }

    [Fact]
    public void Generator_Package_Contains_Localization_BuildTransitive_Assets()
    {
        var project = XDocument.Load(GetRepoFile("src/AtomUI.Generator/AtomUI.Generator.csproj"));
        project.Descendants("None")
               .ShouldContain(element =>
                   (string?)element.Attribute("Include") == "@(AtomUINuGetBuildAsset)" &&
                   (string?)element.Attribute("Pack") == "true");
        project.Descendants("None")
               .ShouldContain(element =>
                   (string?)element.Attribute("Include") == "@(AtomUIGeneratorToolAsset)" &&
                   (string?)element.Attribute("Pack") == "true");

        var repositoryProps = XDocument.Load(GetRepoFile("build/AtomUI.Repository.props"));
        var buildAssets = repositoryProps.Descendants("AtomUINuGetBuildAsset").ShouldHaveSingleItem();
        var buildAssetIncludes = ((string?)buildAssets.Attribute("Include")).ShouldNotBeNull();
        buildAssetIncludes.ShouldContain("AtomUI.Localization.props");
        buildAssetIncludes.ShouldContain("AtomUI.Localization.targets");
        buildAssetIncludes.ShouldContain("AtomUI.Generator.props");
        buildAssetIncludes.ShouldContain("AtomUI.Generator.targets");
        buildAssets.Elements("PackagePath")
                   .ShouldHaveSingleItem()
                   .Value.ShouldBe("buildTransitive/%(Filename)%(Extension)");

        var toolAssets = repositoryProps.Descendants("AtomUIGeneratorToolAsset").First();
        var toolIncludes = ((string?)toolAssets.Attribute("Include")).ShouldNotBeNull();
        repositoryProps.Descendants("AtomUIGeneratorToolAsset").Single(element => element.Attribute("Include")!.Value.Contains("AtomUI.Build.Tasks.dll", StringComparison.Ordinal)).Attribute("Include")!.Value.ShouldContain("AtomUI.Build.Tasks.runtimeconfig.json");
        foreach (var dependency in new[]
                 {
                     "System.Reflection.Metadata.dll",
                     "System.Collections.Immutable.dll",
                     "System.Memory.dll",
                     "System.Buffers.dll",
                     "System.Numerics.Vectors.dll",
                     "System.Runtime.CompilerServices.Unsafe.dll"
                 })
        {
            toolIncludes.ShouldContain(dependency);
        }
        toolAssets.Elements("PackagePath")
                  .ShouldHaveSingleItem()
                  .Value.ShouldBe("tools/netstandard2.0/%(Filename)%(Extension)");

        var buildTasksProject = XDocument.Load(GetRepoFile(
            "src/AtomUI.Toolchain/AtomUI.Toolchain.csproj"));
        buildTasksProject.Descendants("CopyLocalLockFileAssemblies")
                         .Single()
                         .Value.Trim()
                         .ShouldBe("true");

        project.Descendants("ProjectReference")
               .Single(element =>
                   ((string?)element.Attribute("Include"))?.EndsWith(
                       "AtomUI.Toolchain/AtomUI.Toolchain.csproj",
                       StringComparison.Ordinal) == true &&
                   (string?)element.Attribute("ReferenceOutputAssembly") == "false")
               .ShouldNotBeNull();

        var generatorProps = XDocument.Load(GetRepoFile("build/AtomUI.Generator.props"));
        generatorProps.Descendants("Import")
                      .Single(element =>
                          ((string?)element.Attribute("Project"))?.EndsWith(
                              "AtomUI.Localization.props",
                              StringComparison.Ordinal) == true)
                      .ShouldNotBeNull();

        var generatorTargets = XDocument.Load(GetRepoFile("build/AtomUI.Generator.targets"));
        generatorTargets.Descendants("Import")
                        .Single(element =>
                            ((string?)element.Attribute("Project"))?.EndsWith(
                                "AtomUI.Localization.targets",
                                StringComparison.Ordinal) == true)
                        .ShouldNotBeNull();
    }

    [Fact]
    public void Generator_Project_Isolates_Runtime_Publish_Properties()
    {
        var project = XDocument.Load(GetRepoFile("src/AtomUI.Generator/AtomUI.Generator.csproj"));
        var isolatedProperties = ((string?)project.Root!.Attribute("TreatAsLocalProperty"))!
                                 .Split(';', StringSplitOptions.RemoveEmptyEntries);

        isolatedProperties.ShouldContain("PublishAot");
        isolatedProperties.ShouldContain("PublishTrimmed");
        isolatedProperties.ShouldContain("PublishSingleFile");
        isolatedProperties.ShouldContain("SelfContained");
        isolatedProperties.ShouldContain("RuntimeIdentifier");

        var properties = project.Descendants()
                                .Where(element => element.Parent?.Name.LocalName == "PropertyGroup")
                                .ToDictionary(
                                    element => element.Name.LocalName,
                                    element => element.Value,
                                    StringComparer.Ordinal);
        properties["PublishAot"].ShouldBe("false");
        properties["PublishTrimmed"].ShouldBe("false");
        properties["PublishSingleFile"].ShouldBe("false");
        properties["SelfContained"].ShouldBe("false");
        properties["RuntimeIdentifier"].ShouldBeEmpty();
    }

    [Fact]
    public void Official_PtBr_Language_Package_Projects_Match_The_Publishing_Contract()
    {
        var moduleProjects = new[]
        {
            new LanguagePackageProjectContract(
                "AtomUI.Controls.I18n.PtBR",
                "AtomUI.Controls",
                "../../../AtomUI.Controls/AtomUI.Controls.csproj"),
            new LanguagePackageProjectContract(
                "AtomUI.Desktop.Controls.I18n.PtBR",
                "AtomUI.Desktop.Controls",
                "../../../AtomUI.Desktop.Controls/AtomUI.Desktop.Controls.csproj"),
            new LanguagePackageProjectContract(
                "AtomUI.Desktop.Controls.DataGrid.I18n.PtBR",
                "AtomUI.Desktop.Controls.DataGrid",
                "../../../AtomUI.Desktop.Controls.DataGrid/AtomUI.Desktop.Controls.DataGrid.csproj"),
            new LanguagePackageProjectContract(
                "AtomUI.Desktop.Controls.ColorPicker.I18n.PtBR",
                "AtomUI.Desktop.Controls.ColorPicker",
                "../../../AtomUI.Desktop.Controls.ColorPicker/AtomUI.Desktop.Controls.ColorPicker.csproj")
        };

        foreach (var contract in moduleProjects)
        {
            AssertOfficialLanguageModuleProject(contract);
        }

        const string aggregatePackageId = "AtomUI.I18n.PtBR";
        var aggregateProjectPath = $"src/LanguagePacks/pt-BR/{aggregatePackageId}/{aggregatePackageId}.csproj";
        var aggregateProject = XDocument.Load(GetRepoFile(aggregateProjectPath));
        var aggregateProperties = GetProjectProperties(aggregateProject);

        aggregateProperties["TargetFramework"].ShouldBe("netstandard2.0");
        aggregateProperties["IsPackable"].ShouldBe("true");
        aggregateProperties["IncludeBuildOutput"].ShouldBe("false");
        aggregateProperties["SuppressDependenciesWhenPacking"].ShouldBe("false");
        aggregateProperties["PackageId"].ShouldBe(aggregatePackageId);
        aggregateProperties.ShouldNotContainKey("AtomUIBuildLanguagePackage");
        aggregateProperties["NuspecFile"].ShouldBe("AtomUI.I18n.PtBR.nuspec");
        aggregateProperties["NuspecBasePath"].ShouldBe("$(IntermediateOutputPath)nuspec-base");
        aggregateProperties["NuspecProperties"].ShouldBe("version=$(AtomUIVersion)");

        aggregateProject.Descendants()
                        .Where(element => element.Parent?.Name.LocalName == "ItemGroup" &&
                                          element.Name.LocalName != "ProjectReference")
                        .ShouldBeEmpty();
        var prepareNuspecBase = aggregateProject.Descendants("Target")
                                                  .Single(element =>
                                                      (string?)element.Attribute("Name") ==
                                                      "AtomUIPrepareAggregateNuspecBase");
        ((string?)prepareNuspecBase.Attribute("BeforeTargets")).ShouldBe("GenerateNuspec");
        ((string?)prepareNuspecBase.Descendants("RemoveDir").Single().Attribute("Directories"))
            .ShouldBe("$(NuspecBasePath)");
        ((string?)prepareNuspecBase.Descendants("MakeDir").Single().Attribute("Directories"))
            .ShouldBe("$(NuspecBasePath)");

        var expectedModuleProjectPaths = moduleProjects
            .Select(contract => $"../{contract.PackageId}/{contract.PackageId}.csproj")
            .ToArray();
        var aggregateProjectReferences = aggregateProject.Descendants("ProjectReference").ToArray();
        aggregateProjectReferences.Select(reference => (string)reference.Attribute("Include")!)
                                  .ShouldBe(expectedModuleProjectPaths, ignoreOrder: true);
        foreach (var projectReference in aggregateProjectReferences)
        {
            ((string?)projectReference.Attribute("ReferenceOutputAssembly")).ShouldBe("false");
            projectReference.Attribute("PrivateAssets").ShouldBeNull();
        }

        var aggregateNuspec = XDocument.Load(GetRepoFile(
            "src/LanguagePacks/pt-BR/AtomUI.I18n.PtBR/AtomUI.I18n.PtBR.nuspec"));
        var nuspecMetadata = aggregateNuspec.Descendants()
                                             .Single(element => element.Name.LocalName == "metadata");
        nuspecMetadata.Elements().Single(element => element.Name.LocalName == "id").Value
                      .ShouldBe(aggregatePackageId);
        nuspecMetadata.Elements().Single(element => element.Name.LocalName == "version").Value
                      .ShouldBe("$version$");
        nuspecMetadata.Descendants()
                      .Where(element => element.Name.LocalName == "group")
                      .ShouldBeEmpty();
        nuspecMetadata.Descendants()
                      .Where(element => element.Name.LocalName == "dependency")
                      .Select(element => new
                      {
                          Id = (string?)element.Attribute("id"),
                          Version = (string?)element.Attribute("version")
                      })
                      .ShouldBe(
                          moduleProjects.Select(contract => new
                          {
                              Id = (string?)contract.PackageId,
                              Version = (string?)"[$version$]"
                          }),
                          ignoreOrder: true);
        var aggregateFiles = aggregateNuspec.Descendants()
                                            .Where(element => element.Name.LocalName == "files")
                                            .ShouldHaveSingleItem();
        var aggregateReadme = aggregateFiles.Elements()
                                                .Where(element => element.Name.LocalName == "file")
                                                .ShouldHaveSingleItem();
        ((string?)aggregateReadme.Attribute("src")).ShouldBe("README.nuget.md");
        ((string?)aggregateReadme.Attribute("target")).ShouldBeEmpty();

        var solution = XDocument.Load(GetRepoFile("AtomUI.slnx"));
        var solutionProjectPaths = solution.Descendants("Project")
                                           .Select(element => (string?)element.Attribute("Path"))
                                           .ToArray();
        foreach (var projectPath in moduleProjects
                     .Select(contract =>
                         $"src/LanguagePacks/pt-BR/{contract.PackageId}/{contract.PackageId}.csproj")
                     .Append(aggregateProjectPath))
        {
            solutionProjectPaths.ShouldContain(projectPath);
        }
    }

    [Fact]
    public void Gallery_Hosts_Consume_Official_PtBr_Translations_Through_Project_References()
    {
        string[] expectedReferences =
        [
            "../../src/LanguagePacks/pt-BR/AtomUI.Controls.I18n.PtBR/AtomUI.Controls.I18n.PtBR.csproj",
            "../../src/LanguagePacks/pt-BR/AtomUI.Desktop.Controls.I18n.PtBR/AtomUI.Desktop.Controls.I18n.PtBR.csproj",
            "../../src/LanguagePacks/pt-BR/AtomUI.Desktop.Controls.DataGrid.I18n.PtBR/AtomUI.Desktop.Controls.DataGrid.I18n.PtBR.csproj",
            "../../src/LanguagePacks/pt-BR/AtomUI.Desktop.Controls.ColorPicker.I18n.PtBR/AtomUI.Desktop.Controls.ColorPicker.I18n.PtBR.csproj"
        ];
        foreach (var projectPath in new[]
                 {
                     "controlgallery/AtomUIGallery.Desktop/AtomUIGallery.Desktop.csproj",
                     "controlgallery/AtomUIGallery.Browser/AtomUIGallery.Browser.csproj",
                     "tests/AtomUIGallery.Tests/AtomUIGallery.Tests.csproj"
                 })
        {
            var project = XDocument.Load(GetRepoFile(projectPath));
            project.Descendants("AtomUILanguagePackProjectReference")
                   .Select(element => (string?)element.Attribute("Include"))
                   .ShouldBe(expectedReferences, ignoreOrder: true);

            var generatorReference = project.Descendants("ProjectReference")
                                            .Single(element =>
                                                ((string?)element.Attribute("Include"))?.EndsWith(
                                                    "src/AtomUI.Generator/AtomUI.Generator.csproj",
                                                    StringComparison.Ordinal) == true);
            ((string?)generatorReference.Attribute("OutputItemType")).ShouldBe("Analyzer");
            ((string?)generatorReference.Attribute("ReferenceOutputAssembly")).ShouldBe("false");
            ((string?)generatorReference.Attribute("PrivateAssets")).ShouldBe("all");
        }

        var galleryProject = XDocument.Load(
            GetRepoFile("controlgallery/AtomUIGallery/AtomUIGallery.csproj"));
        galleryProject.Descendants("AtomUILanguagePackProjectReference").ShouldBeEmpty();
    }

    private static void AssertOfficialLanguageModuleProject(LanguagePackageProjectContract contract)
    {
        var projectPath = $"src/LanguagePacks/pt-BR/{contract.PackageId}/{contract.PackageId}.csproj";
        var project = XDocument.Load(GetRepoFile(projectPath));
        var properties = GetProjectProperties(project);

        properties["TargetFramework"].ShouldBe("netstandard2.0");
        properties["IsPackable"].ShouldBe("true");
        properties["IncludeBuildOutput"].ShouldBe("false");
        properties["SuppressDependenciesWhenPacking"].ShouldBe("true");
        properties["PackageId"].ShouldBe(contract.PackageId);
        properties["AtomUIBuildLanguagePackage"].ShouldBe("true");
        properties["AtomUILanguageTag"].ShouldBe("pt-BR");
        properties["AtomUILanguageModuleId"].ShouldBe(contract.ModuleId);
        properties.ShouldNotContainKey("AtomUILanguageContractVersion");
        properties.ShouldNotContainKey("AtomUILanguageMinimumState");

        var projectReferences = project.Descendants("ProjectReference").ToArray();
        projectReferences.Length.ShouldBe(2);
        var generatorReference = projectReferences.Single(reference =>
            (string?)reference.Attribute("OutputItemType") == "Analyzer");
        ((string?)generatorReference.Attribute("Include"))
            .ShouldBe("../../../AtomUI.Generator/AtomUI.Generator.csproj");
        ((string?)generatorReference.Attribute("OutputItemType")).ShouldBe("Analyzer");
        ((string?)generatorReference.Attribute("ReferenceOutputAssembly")).ShouldBe("false");
        ((string?)generatorReference.Attribute("PrivateAssets")).ShouldBe("all");

        var componentReference = projectReferences.Single(reference =>
            (string?)reference.Attribute("OutputItemType") != "Analyzer");
        ((string?)componentReference.Attribute("Include")).ShouldBe(contract.ComponentProjectReference);
        componentReference.Attributes().Select(attribute => attribute.Name.LocalName)
                          .ShouldBe(["Include"]);

        project.Descendants("AtomUILanguage").ShouldBeEmpty();
    }

    private static Dictionary<string, string> GetProjectProperties(XDocument project)
    {
        return project.Descendants()
                      .Where(element => element.Parent?.Name.LocalName == "PropertyGroup")
                      .ToDictionary(
                          element => element.Name.LocalName,
                          element => element.Value,
                          StringComparer.Ordinal);
    }

    private static void AssertKeepsGeneratorMetadata(
        XElement item,
        bool includePackagePath = false)
    {
        var expectedMetadata = new List<string>
        {
            "AtomUILanguageSourceKind",
            "AtomUILanguageSourceIdentity",
            "AtomUILanguageModuleId",
            "AtomUILanguageContractValidation",
            "AtomUILanguageSourceFingerprint"
        };
        if (includePackagePath)
        {
            expectedMetadata.Add("AtomUILanguagePackagePath");
        }

        var keepMetadata = ((string?)item.Attribute("KeepMetadata")).ShouldNotBeNull();
        var values = keepMetadata
            .Replace("$(_AtomUILanguageGeneratorMetadata)", string.Join(";", expectedMetadata[..5]))
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        values.ShouldBe(expectedMetadata, ignoreOrder: true);
    }

    private static void WriteProject(string path, params XElement[] content)
    {
        new XDocument(new XElement("Project", new XAttribute("Sdk", "Microsoft.NET.Sdk"), content))
            .Save(path);
    }

    private static void WriteModuleProject(string path, string repoRoot)
    {
        new XDocument(
            new XElement(
                "Project",
                new XAttribute("Sdk", "Microsoft.NET.Sdk"),
                new XElement(
                    "PropertyGroup",
                    new XElement("TargetFramework", "net10.0"),
                    new XElement("Nullable", "enable"),
                    new XElement("PackageId", "Fixture.Module"),
                    new XElement("AtomUILanguageModuleId", "Fixture.Module")),
                new XElement(
                    "Import",
                    new XAttribute("Project", Path.Combine(repoRoot, "build", "AtomUI.Generator.props"))),
                new XElement(
                    "Import",
                    new XAttribute("Project", Path.Combine(repoRoot, "build", "AtomUI.Localization.targets")))))
            .Save(path);
    }

    private static void WriteLanguagePackProject(
        string path,
        string repoRoot,
        string buildTasksAssembly,
        string moduleProject)
    {
        new XDocument(
            new XElement(
                "Project",
                new XAttribute("Sdk", "Microsoft.NET.Sdk"),
                new XElement(
                    "PropertyGroup",
                    new XElement("TargetFramework", "netstandard2.0"),
                    new XElement("IncludeBuildOutput", "false"),
                    new XElement("PackageId", "Fixture.Module.I18n.PtBR"),
                    new XElement("AtomUIBuildLanguagePackage", "true"),
                    new XElement("AtomUILanguageTag", "pt-BR"),
                    new XElement("AtomUILanguageModuleId", "Fixture.Module"),
                    new XElement("AtomUIBuildTasksAssembly", buildTasksAssembly)),
                new XElement(
                    "Import",
                    new XAttribute("Project", Path.Combine(repoRoot, "build", "AtomUI.Localization.props"))),
                new XElement(
                    "ItemGroup",
                    new XElement(
                        "ProjectReference",
                        new XAttribute("Include", moduleProject))),
                new XElement(
                    "Import",
                    new XAttribute("Project", Path.Combine(repoRoot, "build", "AtomUI.Localization.targets")))))
            .Save(path);
    }

    private static void WriteConsumerProject(
        string path,
        string repoRoot,
        string generatorAssembly,
        string moduleProject,
        string languagePackProject)
    {
        new XDocument(
            new XElement(
                "Project",
                new XAttribute("Sdk", "Microsoft.NET.Sdk"),
                new XElement(
                    "PropertyGroup",
                    new XElement("TargetFramework", "net10.0"),
                    new XElement("Nullable", "enable"),
                    new XElement("EmitCompilerGeneratedFiles", "true"),
                    new XElement("CompilerGeneratedFilesOutputPath", "$(BaseIntermediateOutputPath)Generated")),
                new XElement(
                    "Import",
                    new XAttribute("Project", Path.Combine(repoRoot, "build", "AtomUI.Generator.props"))),
                new XElement(
                    "ItemGroup",
                    new XElement("ProjectReference", new XAttribute("Include", moduleProject)),
                    new XElement("Analyzer", new XAttribute("Include", generatorAssembly)),
                    new XElement(
                        "AtomUILanguagePackProjectReference",
                        new XAttribute("Include", languagePackProject))),
                new XElement(
                    "Import",
                    new XAttribute("Project", Path.Combine(repoRoot, "build", "AtomUI.Localization.targets")))))
            .Save(path);
    }

    private static async Task<BuildResult> RunDotNetBuildAsync(string projectPath, string configuration)
    {
        var startInfo = new ProcessStartInfo(Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") ?? "dotnet")
        {
            WorkingDirectory = Path.GetDirectoryName(projectPath)!,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        startInfo.ArgumentList.Add("build");
        startInfo.ArgumentList.Add(projectPath);
        startInfo.ArgumentList.Add("--configuration");
        startInfo.ArgumentList.Add(configuration);
        startInfo.ArgumentList.Add("--nologo");

        using var process = Process.Start(startInfo).ShouldNotBeNull();
        var standardOutput = process.StandardOutput.ReadToEndAsync(TestContext.Current.CancellationToken);
        var standardError = process.StandardError.ReadToEndAsync(TestContext.Current.CancellationToken);
        await process.WaitForExitAsync(TestContext.Current.CancellationToken);
        return new BuildResult(
            process.ExitCode,
            await standardOutput,
            await standardError);
    }

    private static string GetRepoFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException(relativePath);
    }

    private sealed record LanguagePackageProjectContract(
        string PackageId,
        string ModuleId,
        string ComponentProjectReference);

    private sealed record BuildResult(int ExitCode, string StandardOutput, string StandardError)
    {
        internal string Output => StandardOutput + StandardError;
    }

    private const string FixtureRuntimeSource = """
        using System.Reflection;

        [assembly: AssemblyMetadata("AtomUILanguageModuleId", "Fixture.Module")]

        namespace Avalonia
        {
            public abstract class Application { }
        }

        namespace AtomUI.Localization
        {
            [System.AttributeUsage(System.AttributeTargets.Enum, AllowMultiple = false)]
            public sealed class LanguageCatalogAttribute : System.Attribute;

            public interface IGeneratedApplicationLanguageBootstrap
            {
                void RegisterApplicationLanguages(ILocalizationBuilder builder);
            }

            [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple = true)]
            public sealed class LanguageModuleManifestAttribute : System.Attribute
            {
                public LanguageModuleManifestAttribute(int version, string moduleId, string data) { }
            }

            public interface ILocalizationBuilder
            {
                void AddModuleData(string moduleId, string data);
                void AddCatalog(LanguageCatalogDescriptor descriptor);
                void AddTranslationBundle(TranslationBundleDescriptor descriptor);
            }

            public abstract class LanguageCatalogDescriptor { }

            public sealed class LanguageCatalogDescriptor<TResourceKind> : LanguageCatalogDescriptor
                where TResourceKind : struct, System.Enum
            {
                public LanguageCatalogDescriptor(
                    string catalogId,
                    System.Collections.Generic.IReadOnlyList<LanguageCatalogUnitDescriptor> units,
                    System.Func<TResourceKind, int> unitSlotResolver) { }
            }

            public sealed class LanguageCatalogUnitDescriptor
            {
                public LanguageCatalogUnitDescriptor(string key, bool isFormatted = false) { }
            }

            public readonly struct LanguageTag
            {
                public static LanguageTag Parse(string value) => default;
            }

            public enum TranslationSourceKind : byte
            {
                ModuleBuiltIn,
                StaticLanguagePack,
                ApplicationOverride
            }

            public sealed class TranslationBundleDescriptor
            {
                public TranslationBundleDescriptor(
                    string catalogId,
                    LanguageTag language,
                    TranslationSourceKind sourceKind,
                    string sourceIdentity,
                    System.Collections.Generic.IReadOnlyList<string?> values) { }
            }
        }

        namespace Fixture.Localization
        {
            [AtomUI.Localization.LanguageCatalog]
            public enum Messages
            {
                Cancel
            }
        }
        """;

    private const string FixtureSourceXliff = """
        <xliff xmlns="urn:oasis:names:tc:xliff:document:2.0" version="2.1" srcLang="en-US">
          <file id="Fixture.Localization.Messages">
            <unit id="Cancel"><segment><source>Cancel</source></segment></unit>
          </file>
        </xliff>
        """;

    private const string FixtureTargetXliff = """
        <xliff xmlns="urn:oasis:names:tc:xliff:document:2.0" version="2.1" srcLang="en-US" trgLang="pt-BR">
          <file id="Fixture.Localization.Messages">
            <unit id="Cancel"><segment><source>Cancel</source><target state="final">Cancelar</target></segment></unit>
          </file>
        </xliff>
        """;
}
