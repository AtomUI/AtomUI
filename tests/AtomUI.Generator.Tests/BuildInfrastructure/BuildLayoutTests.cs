using System.Xml.Linq;
using Shouldly;
using Xunit;

namespace AtomUI.Generator.Tests.BuildInfrastructure;

public sealed class BuildLayoutTests
{
    private static readonly string[] s_expectedNuGetBuildAssets =
    [
        "AtomUI.Generator.props",
        "AtomUI.Toolchain.Tasks.targets",
        "AtomUI.Generator.targets",
        "AtomUI.Registration.targets",
        "AtomUI.Registration.Framework.targets",
        "AtomUI.Registration.Net8.targets",
        "AtomUI.Registration.Net8.Host.targets",
        "AtomUI.Registration.Net8.Transforms.targets",
        "AtomUI.Registration.Native8.targets",
        "AtomUI.Registration.Bridge.targets",
        "AtomUI.Net8.ILLink.Task.cs",
        "AtomUI.Localization.props",
        "AtomUI.Localization.targets",
        "AtomUI.ThemeAssets.targets",
        "AtomUI.CompiledThemes.targets",
        "AtomUI.Build.Tasks.Process.cs"
    ];

    [Fact]
    public void Directory_Build_Files_Import_Only_Repository_Entry_Points()
    {
        GetImports("Directory.Build.props")
            .ShouldBe(["$(MSBuildThisFileDirectory)build/AtomUI.Repository.props"]);
        GetImports("Directory.Build.targets")
            .ShouldBe(["$(MSBuildThisFileDirectory)build/AtomUI.Repository.targets"]);
    }

    [Fact]
    public void Repository_Props_Import_Configuration_And_Generator_Entry_Point()
    {
        const string repositoryPropsPath = "build/AtomUI.Repository.props";
        GetImports(repositoryPropsPath).ShouldBe([
            "$(MSBuildThisFileDirectory)Versions.props",
            "$(MSBuildThisFileDirectory)ProjectDefaults.props",
            "$(MSBuildThisFileDirectory)PackageMetadata.props",
            "$(MSBuildThisFileDirectory)OutputPaths.props",
            "$(MSBuildThisFileDirectory)PackageValidation.props",
            "$(MSBuildThisFileDirectory)AtomUI.Generator.props"
        ]);
    }

    [Fact]
    public void Package_Validation_Is_Opt_In_And_Mechanically_Enforced()
    {
        // The props file must be imported only for validation runs. An unconditional import would
        // be evaluated for every project in the ordinary Debug loop, so keep it gated.
        var repositoryProps = XDocument.Load(GetRepoFile("build/AtomUI.Repository.props"));
        var validationImport = repositoryProps.Descendants("Import")
                                              .Single(element =>
                                                  ((string?)element.Attribute("Project") ?? string.Empty)
                                                  .EndsWith("PackageValidation.props", StringComparison.Ordinal));
        var importCondition = (string?)validationImport.Attribute("Condition");
        importCondition.ShouldNotBeNullOrWhiteSpace();
        importCondition.ShouldContain("AtomUIPackageValidationBaselineVersion");

        var validationProps = XDocument.Load(GetRepoFile("build/PackageValidation.props"));

        // Opt-in: validation only engages when the release flow supplies the baseline version, so
        // ordinary development builds stay fast and need no baseline packages.
        var enableValidation = validationProps.Descendants("EnablePackageValidation").ShouldHaveSingleItem();
        enableValidation.Value.ShouldBe("true");
        ((string?)enableValidation.Parent?.Attribute("Condition"))
            .ShouldBe("'$(AtomUIPackageValidationBaselineVersion)' != ''");

        var baselineVersion = validationProps.Descendants("PackageValidationBaselineVersion")
                                             .ShouldHaveSingleItem();
        ((string?)baselineVersion.Attribute("Condition"))
            .ShouldBe("'$(PackageValidationBaselineVersion)' == ''");
        baselineVersion.Value.ShouldBe("$(AtomUIPackageValidationBaselineVersion)");

        // Avalonia's private-API target replaces the facade with real split assemblies only before
        // CoreCompile. Package validation stops at ResolveReferences, so extend the SDK's collected
        // per-TFM reference paths after _GetReferencePathFromInnerProjects has populated them.
        var validationDependsOn = validationProps.Descendants("RunPackageValidationDependsOn")
                                                   .ShouldHaveSingleItem();
        validationDependsOn.Value.ShouldContain("AtomUIAddAvaloniaPackageValidationReferences");
        validationDependsOn.Value.ShouldContain("$(RunPackageValidationDependsOn)");

        var avaloniaReferencesTarget = validationProps.Descendants("Target")
                                                       .Single(element =>
                                                           (string?)element.Attribute("Name") ==
                                                           "AtomUIAddAvaloniaPackageValidationReferences");
        var avaloniaTargetCondition = ((string?)avaloniaReferencesTarget.Attribute("Condition"))
            .ShouldNotBeNull();
        avaloniaTargetCondition.ShouldContain("AvaloniaAccessUnstablePrivateApis");
        var avaloniaReferences = avaloniaReferencesTarget
                                 .Descendants("_AtomUIAvaloniaPackageValidationReference")
                                 .ShouldHaveSingleItem();
        ((string?)avaloniaReferences.Attribute("Include"))
            .ShouldBe("$(NuGetPackageRoot)avalonia/$(AvaloniaVersion)/lib/$(_AtomUIAvaloniaPackageValidationFramework)/*.dll");

        var packageValidationReferences = avaloniaReferencesTarget
                                          .Descendants("PackageValidationReferencePath")
                                          .ShouldHaveSingleItem();
        ((string?)packageValidationReferences.Attribute("Update"))
            .ShouldBe("@(PackageValidationReferencePath)");
        var referencePath = packageValidationReferences.Element("ReferencePath")
                                                       .ShouldNotBeNull()
                                                       .Value;
        referencePath.ShouldContain("%(PackageValidationReferencePath.ReferencePath)");
        referencePath.ShouldContain("@(_AtomUIAvaloniaPackageValidationReference->'%(FullPath)', ',')");
        avaloniaReferencesTarget.Descendants("Error").ShouldHaveSingleItem();

        // The release script must accept the baseline and must run both gates. ApiCompat only reads
        // lib/, so verify-package-layout.ps1 is what covers tools/ and buildTransitive/.
        var buildScript = File.ReadAllText(GetRepoFile("scripts/BuildNuGetPackages.ps1"));
        buildScript.ShouldContain("$PackageValidationBaselineVersion");
        buildScript.ShouldContain("AtomUIPackageValidationBaselineVersion=");
        buildScript.ShouldContain("verification/verify-package-layout.ps1");

        // Prerequisite tool projects are not published packages and must never be validated against
        // a released baseline.
        var prerequisiteBuild = buildScript.IndexOf(
            "foreach ($project in $AtomUIReleaseBuildPrerequisiteProjects)", StringComparison.Ordinal);
        var prerequisiteBlockEnd = buildScript.IndexOf(
            "foreach ($project in $AtomUIReleasePackageProjects)", prerequisiteBuild, StringComparison.Ordinal);
        prerequisiteBuild.ShouldBeGreaterThanOrEqualTo(0);
        prerequisiteBlockEnd.ShouldBeGreaterThan(prerequisiteBuild);
        buildScript[prerequisiteBuild..prerequisiteBlockEnd].ShouldNotContain("$validationArguments");

        // The release workflow must expose the baseline input and forward it to the script.
        var workflow = File.ReadAllText(GetRepoFile(".github/workflows/release-nuget-packages.yml"));
        workflow.ShouldContain("PackageValidationBaselineVersion:");
        workflow.ShouldContain("-PackageValidationBaselineVersion");
    }

    [Fact]
    public void Repository_Targets_Reuse_The_Generator_Entry_Point()
    {
        var repositoryTargets = XDocument.Load(GetRepoFile("build/AtomUI.Repository.targets"));

        GetImports("build/AtomUI.Repository.targets")
            .ShouldBe(["$(MSBuildThisFileDirectory)AtomUI.Generator.targets"]);
        GetTargetNames(repositoryTargets)
            .ShouldContain("AtomUIPrepareGeneratorConsumerPackageAssets");
    }

    [Fact]
    public void Generator_Build_Assets_Have_One_Explicit_Manifest()
    {
        var repositoryProps = XDocument.Load(GetRepoFile("build/AtomUI.Repository.props"));
        var buildAssets = repositoryProps.Descendants("AtomUINuGetBuildAsset").ShouldHaveSingleItem();
        var includes = ((string?)buildAssets.Attribute("Include"))
            .ShouldNotBeNull()
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(include => include.Replace("$(MSBuildThisFileDirectory)", string.Empty,
                                               StringComparison.Ordinal))
            .ToArray();
        includes.ShouldBe(s_expectedNuGetBuildAssets);
        buildAssets.Attribute("Exclude").ShouldBeNull();
        buildAssets.Elements("PackagePath")
                   .ShouldHaveSingleItem()
                   .Value.ShouldBe("buildTransitive/%(Filename)%(Extension)");

        var toolAssets = repositoryProps.Descendants("AtomUIGeneratorToolAsset").Single(element => ((string?)element.Attribute("Include"))?.Contains("AtomUI.Build.Tasks.dll", StringComparison.Ordinal) == true);
        var toolIncludes = ((string?)toolAssets.Attribute("Include")).ShouldNotBeNull();
        toolIncludes.ShouldContain("AtomUI.Build.Tasks.runtimeconfig.json");
        toolIncludes.ShouldContain("AtomUI.Build.Tasks.dll");
        toolAssets.Elements("PackagePath")
                  .ShouldHaveSingleItem()
                  .Value.ShouldBe("tools/net10.0/%(Filename)%(Extension)");

        var generatorProject = XDocument.Load(GetRepoFile("src/AtomUI.Generator/AtomUI.Generator.csproj"));
        generatorProject.Descendants("ProjectReference")
                        .ShouldNotContain(element =>
                            ((string?)element.Attribute("Include") ?? string.Empty)
                            .Contains("AtomUI.Generator.LinkedPublish", StringComparison.Ordinal));
        generatorProject.Descendants("None")
                        .ShouldContain(element =>
                            (string?)element.Attribute("Include") == "@(AtomUINuGetBuildAsset)");
        generatorProject.Descendants("None")
                        .ShouldContain(element =>
                            (string?)element.Attribute("Include") == "@(AtomUIGeneratorToolAsset)");

        var repositoryTargets = XDocument.Load(GetRepoFile("build/AtomUI.Repository.targets"));
        repositoryTargets.Descendants("None")
                         .ShouldContain(element =>
                             (string?)element.Attribute("Include") == "@(AtomUINuGetBuildAsset)");
        repositoryTargets.Descendants("None")
                         .ShouldContain(element =>
                             (string?)element.Attribute("Include") == "@(AtomUIGeneratorToolAsset)");

        // Normal generator builds depend only on the worker. Packaging explicitly asks the
        // single toolchain owner to restore/build all engine profiles before collecting files.
        var reference = generatorProject.Descendants("ProjectReference").ShouldHaveSingleItem();
        ((string?)reference.Attribute("Include")).ShouldBe("../AtomUI.Toolchain/AtomUI.Toolchain.csproj");
        ((string?)reference.Attribute("ReferenceOutputAssembly")).ShouldBe("false");
        ((string?)reference.Attribute("PrivateAssets")).ShouldBe("all");
        generatorProject.Descendants("MSBuild").ShouldBeEmpty();
        var prepare = repositoryTargets.Descendants("Target")
            .Single(element => (string?)element.Attribute("Name") == "AtomUIPrepareRepositoryPackageTools")
            .Descendants("MSBuild").ShouldHaveSingleItem();
        ((string?)prepare.Attribute("Projects")).ShouldBe("$(AtomUIToolchainProject)");
        ((string?)prepare.Attribute("Targets")).ShouldBe("BuildToolchain");
    }

    [Fact]
    public void NuGet_Features_Use_The_Shared_Build_Tasks_Assembly()
    {
        var repositoryProps = XDocument.Load(GetRepoFile("build/AtomUI.Repository.props"));
        repositoryProps.Descendants("AtomUIBuildTasksAssembly")
                       .ShouldHaveSingleItem()
                       .Value.ShouldContain("AtomUI.Build.Tasks.dll");

        var generatorProps = XDocument.Load(GetRepoFile("build/AtomUI.Generator.props"));
        var fallback = generatorProps.Descendants("AtomUIBuildTasksAssembly").ShouldHaveSingleItem();
        ((string?)fallback.Attribute("Condition"))
            .ShouldBe("'$(AtomUIBuildTasksAssembly)' == ''");
        fallback.Value.Trim().ShouldBe(
            "$(MSBuildThisFileDirectory)../tools/net10.0/AtomUI.Build.Tasks.dll");

        var buildRoot = Path.Combine(GetRepositoryRoot(), "build");
        var featureFiles = s_expectedNuGetBuildAssets
            .Where(file => Path.GetExtension(file) == ".targets")
            .Select(file => Path.Combine(buildRoot, file));
        var usingTasks = featureFiles.SelectMany(file => XDocument.Load(file).Descendants("UsingTask"))
                                     .ToArray();
        usingTasks.ShouldNotBeEmpty();
        usingTasks.ShouldAllBe(element =>
            (string?)element.Attribute("AssemblyFile") == "$(MSBuildToolsPath)/Microsoft.Build.Tasks.Core.dll");

        File.Exists(Path.Combine(buildRoot, "BuildTasks.props")).ShouldBeFalse();
        var buildText = string.Join('\n', Directory.EnumerateFiles(buildRoot).Select(File.ReadAllText));
        buildText.ShouldNotContain("AtomUILocalizationBuildTasksAssembly");
        buildText.ShouldNotContain("AtomUILinkedRegistrationBuildTasksAssembly");
    }

    [Fact]
    public void NuGet_Build_Tasks_Use_The_SDK_Source_Adapter()
    {
        var buildRoot = Path.Combine(GetRepositoryRoot(), "build");
        var usingTasks = s_expectedNuGetBuildAssets
            .Where(file => Path.GetExtension(file) == ".targets")
            .Select(file => Path.Combine(buildRoot, file))
            .SelectMany(file => XDocument.Load(file)
                .Descendants()
                .Where(element => element.Name.LocalName == "UsingTask"))
            .Where(element =>
                (string?)element.Attribute("AssemblyFile") == "$(MSBuildToolsPath)/Microsoft.Build.Tasks.Core.dll")
            .ToArray();

        usingTasks.ShouldNotBeEmpty();
        usingTasks.ShouldAllBe(element =>
            element.Attribute("Runtime") == null);
        usingTasks.ShouldAllBe(element =>
            (string?)element.Attribute("TaskFactory") == "RoslynCodeTaskFactory");
    }

    [Fact]
    public void Repository_Test_Project_Flag_Has_A_Canonical_Boolean_Value()
    {
        var defaults = XDocument.Load(GetRepoFile("build/ProjectDefaults.props"));
        var isTestProject = defaults.Descendants("IsTestProject").ShouldHaveSingleItem();

        isTestProject.Value.ShouldBe("true");
    }

    [Fact]
    public void Repository_Output_Paths_Centralize_Binaries_And_Intermediate_Files()
    {
        var outputPaths = XDocument.Load(GetRepoFile("build/OutputPaths.props"));

        outputPaths.Descendants("PackageOutputPath")
                   .ShouldHaveSingleItem()
                   .Value.ShouldBe("$(MSBuildThisFileDirectory)../.artifacts/Nuget/$(Configuration)");
        outputPaths.Descendants("OutputPathWithoutFramework")
                   .ShouldHaveSingleItem()
                   .Value.ShouldBe("$(MSBuildThisFileDirectory)../.artifacts/bin/$(Configuration)");
        outputPaths.Descendants("OutputPath")
                   .ShouldHaveSingleItem()
                   .Value.ShouldBe("$(OutputPathWithoutFramework)");
        outputPaths.Descendants("BaseIntermediateOutputPath")
                   .Where(element => element.Attribute("Condition") is null)
                   .ShouldHaveSingleItem()
                   .Value.ShouldBe(
                       "$(MSBuildThisFileDirectory)../.artifacts/$(MSBuildProjectName)/obj");
    }

    [Fact]
    public void Repository_Test_Runs_Clean_VSTest_Temporary_Results()
    {
        var outputPaths = XDocument.Load(GetRepoFile("build/OutputPaths.props"));
        outputPaths.Descendants("AtomUITestResultsRoot")
                   .ShouldHaveSingleItem()
                   .Value.ShouldBe("$(MSBuildThisFileDirectory)../.artifacts/TestResults");
        outputPaths.Descendants("AtomUITestResultsDirectory")
                   .ShouldHaveSingleItem()
                   .Value.ShouldBe("$(AtomUITestResultsRoot)/$(MSBuildProjectName)");
        outputPaths.Descendants("VSTestResultsDirectory")
                   .ShouldHaveSingleItem()
                   .Value.ShouldBe("$(AtomUITestResultsDirectory)");

        var repositoryTargets = XDocument.Load(GetRepoFile("build/AtomUI.Repository.targets"));
        AssertTestResultsCleanupTarget(
            repositoryTargets,
            "AtomUICleanTestResultsBeforeRun",
            "BeforeTargets",
            "_AtomUITestResultsDirectoryToCleanBeforeRun");
        AssertTestResultsCleanupTarget(
            repositoryTargets,
            "AtomUICleanTestResultsAfterRun",
            "AfterTargets",
            "_AtomUITestResultsDirectoryToCleanAfterRun");
    }

    [Fact]
    public void Repository_Test_Runs_Forbid_Persistent_VSTest_Dumps()
    {
        var outputPaths = XDocument.Load(GetRepoFile("build/OutputPaths.props"));
        var hangDumpType = outputPaths.Descendants("VSTestBlameHangDumpType")
                                      .ShouldHaveSingleItem();
        hangDumpType.Value.ShouldBe("none");
        ((string?)hangDumpType.Attribute("Condition")).ShouldBe(
            "'$(IsTestProject)' == 'true' and '$(VSTestBlameHang)' == 'true' and '$(VSTestBlameHangDumpType)' == ''");

        var crashDumpType = outputPaths.Descendants("VSTestBlameCrashDumpType")
                                       .ShouldHaveSingleItem();
        crashDumpType.Value.ShouldBe("none");
        ((string?)crashDumpType.Attribute("Condition")).ShouldBe(
            "'$(IsTestProject)' == 'true' and '$(VSTestBlameCrash)' == 'true' and '$(VSTestBlameCrashDumpType)' == ''");

        var repositoryTargets = XDocument.Load(GetRepoFile("build/AtomUI.Repository.targets"));
        var guardTarget = repositoryTargets.Descendants("Target")
                                           .Single(element =>
                                               (string?)element.Attribute("Name") ==
                                               "AtomUIRejectPersistentVSTestDumps");

        ((string?)guardTarget.Attribute("BeforeTargets")).ShouldBe("VSTest");
        ((string?)guardTarget.Attribute("Condition")).ShouldBe("'$(IsTestProject)' == 'true'");

        var errors = guardTarget.Descendants("Error").ToArray();
        errors.Length.ShouldBe(2);
        errors.Select(element => (string?)element.Attribute("Condition"))
              .ShouldBe([
                  "'$(VSTestBlameHangDumpType)' == 'full' or '$(VSTestBlameHangDumpType)' == 'mini'",
                  "'$(VSTestBlameCrashDumpType)' == 'full' or '$(VSTestBlameCrashDumpType)' == 'mini'"
              ]);
        errors.Select(element => (string?)element.Attribute("Text"))
              .ShouldAllBe(text => text != null &&
                                   text.Contains("must not collect persistent", StringComparison.Ordinal));
    }

    private static void AssertTestResultsCleanupTarget(
        XDocument repositoryTargets,
        string targetName,
        string timingAttribute,
        string itemName)
    {
        var cleanupTarget = repositoryTargets.Descendants("Target")
                                             .Single(element =>
                                                 (string?)element.Attribute("Name") == targetName);

        ((string?)cleanupTarget.Attribute(timingAttribute)).ShouldBe("VSTest");
        ((string?)cleanupTarget.Attribute("Condition")).ShouldBe("'$(IsTestProject)' == 'true'");

        var directoriesToClean = cleanupTarget.Descendants(itemName)
                                              .Select(element => (string?)element.Attribute("Include"))
                                              .ToArray();
        directoriesToClean.ShouldBe([
            "$(MSBuildProjectDirectory)/TestResults",
            "$(AtomUITestResultsDirectory)"
        ]);

        var removeDir = cleanupTarget.Descendants("RemoveDir").ShouldHaveSingleItem();
        ((string?)removeDir.Attribute("Directories")).ShouldBe($"@({itemName})");
        removeDir.Attribute("ContinueOnError").ShouldBeNull();
    }

    [Fact]
    public void Repository_Targets_Exclude_Compiler_Generated_Files_Once()
    {
        const string generatedFilesPattern = "$(CompilerGeneratedFilesOutputPath)/**/*.cs";
        var repositoryTargets = XDocument.Load(GetRepoFile("build/AtomUI.Repository.targets"));
        var removal = repositoryTargets.Descendants("Compile")
                                       .Where(element =>
                                           (string?)element.Attribute("Remove") == generatedFilesPattern)
                                       .ShouldHaveSingleItem();

        ((string?)removal.Parent?.Attribute("Condition"))
            .ShouldBe("'$(CompilerGeneratedFilesOutputPath)' != ''");

        var repositoryRoot = GetRepositoryRoot();
        var projectFiles = new[] { "src", "controlgallery", "tests", "tools" }
            .Select(directory => Path.Combine(repositoryRoot, directory))
            .Where(Directory.Exists)
            .SelectMany(directory => Directory.EnumerateFiles(
                directory,
                "*.csproj",
                SearchOption.AllDirectories));
        foreach (var projectFile in projectFiles)
        {
            var project = XDocument.Load(projectFile);
            if (!project.Descendants("CompilerGeneratedFilesOutputPath").Any())
            {
                continue;
            }

            project.Descendants("Compile")
                   .ShouldNotContain(element =>
                       (string?)element.Attribute("Remove") == generatedFilesPattern,
                       projectFile);
        }
    }

    [Fact]
    public void Repository_Default_Items_Always_Exclude_Stale_Generated_Files()
    {
        var projectDefaults = XDocument.Load(GetRepoFile("build/ProjectDefaults.props"));
        var excludes = projectDefaults.Descendants("DefaultItemExcludes")
                                      .ShouldHaveSingleItem()
                                      .Value;

        excludes.ShouldContain("$(MSBuildProjectDirectory)/obj/**");
        excludes.ShouldContain("$(MSBuildProjectDirectory)/GeneratedFiles/**");
    }

    [Fact]
    public void NuGet_Generator_Entry_Points_Import_Flat_Feature_Files()
    {
        GetImports("build/AtomUI.Generator.props").ShouldBe([
            "$(MSBuildThisFileDirectory)AtomUI.Localization.props"
        ]);
        GetImports("build/AtomUI.Generator.targets").ShouldBe([
            "$(MSBuildThisFileDirectory)AtomUI.Registration.targets",
            "$(MSBuildThisFileDirectory)AtomUI.Localization.targets",
            "$(MSBuildThisFileDirectory)AtomUI.ThemeAssets.targets",
            "$(MSBuildThisFileDirectory)AtomUI.CompiledThemes.targets"
        ]);
    }

    [Fact]
    public void Localization_Targets_Own_The_Complete_Build_Integration()
    {
        var targets = XDocument.Load(GetRepoFile("build/AtomUI.Localization.targets"));

        GetImports("build/AtomUI.Localization.targets").ShouldBe(["$(MSBuildThisFileDirectory)AtomUI.Toolchain.Tasks.targets"]);
        targets.Descendants("AdditionalFiles").ShouldNotBeEmpty();
        targets.Descendants("CompilerVisibleItemMetadata").ShouldNotBeEmpty();
        GetTargetNames(targets).ShouldBe([
            "AtomUIGetLanguageModuleSourceAssets",
            "AtomUIResolveLanguageContractProjectReferences",
            "AtomUIInferLanguagePackageContractPolicy",
            "AtomUIGetLanguagePackProjectAssets",
            "AtomUIResolveLanguagePackProjectReferences",
            "AtomUIExportLanguageTemplates",
            "AtomUIPrepareLanguagePackage",
            "AtomUIPrepareLanguageModuleAssets"
        ]);
    }

    private static string[] GetImports(string relativePath)
    {
        return XDocument.Load(GetRepoFile(relativePath))
                        .Descendants()
                        .Where(element => element.Name.LocalName == "Import")
                        .Select(element => (string?)element.Attribute("Project"))
                        .Where(static path => path is not null)
                        .Cast<string>()
                        .ToArray();
    }

    private static string[] GetTargetNames(XDocument document)
    {
        return document.Descendants()
                       .Where(element => element.Name.LocalName == "Target")
                       .Select(element => (string?)element.Attribute("Name"))
                       .Where(static name => name is not null)
                       .Cast<string>()
                       .ToArray();
    }

    private static string GetRepoFile(string relativePath)
    {
        var repositoryRoot = GetRepositoryRoot();
        var candidate = Path.Combine(repositoryRoot, relativePath);
        if (File.Exists(candidate))
        {
            return candidate;
        }

        throw new FileNotFoundException(relativePath);
    }

    private static string GetRepositoryRoot()
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

        throw new DirectoryNotFoundException("Could not locate the AtomUI repository root.");
    }
}
