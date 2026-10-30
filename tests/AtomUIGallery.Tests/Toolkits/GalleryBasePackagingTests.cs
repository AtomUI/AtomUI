using Shouldly;
using Xunit;

namespace AtomUIGallery.Tests.Toolkits;

public class GalleryBasePackagingTests
{
    private const string PackageManifest = "scripts/NuGetPackageProjects.ps1";
    private const string PackageBuildScript = "scripts/BuildNuGetPackages.ps1";
    private const string GalleryBaseProject = "src/AtomUI.Toolkits.GalleryBase/AtomUI.Toolkits.GalleryBase.csproj";
    private const string ExtrasProject = "src/AtomUI.Desktop.Controls.Extras/AtomUI.Desktop.Controls.Extras.csproj";

    [Theory]
    [InlineData("AtomUI.Toolkits.GalleryBase", GalleryBaseProject)]
    [InlineData("AtomUI.Desktop.Controls.Extras", ExtrasProject)]
    public void Release_Package_Manifest_Includes_Product_Package(string packageId, string projectPath)
    {
        var manifest = ReadRepoFile(PackageManifest);

        manifest.ShouldContain($"PackageId = \"{packageId}\"");
        manifest.ShouldContain($"ProjectPath = \"{projectPath}\"");
    }

    [Fact]
    public void Main_Release_Workflow_Uses_Verified_Package_Build_Script()
    {
        var workflow = ReadRepoFile(".github/workflows/release-nuget-packages.yml");

        var buildStep = workflow.IndexOf("./scripts/BuildNuGetPackages.ps1", StringComparison.Ordinal);
        var localValidationStep = workflow.IndexOf(
            "-  name: Validate packages with local NuGet feed",
            StringComparison.Ordinal);
        var publicPushStep = workflow.IndexOf("-  name: Publish to nuget.org", StringComparison.Ordinal);

        buildStep.ShouldBeGreaterThanOrEqualTo(0);
        localValidationStep.ShouldBeGreaterThan(buildStep);
        publicPushStep.ShouldBeGreaterThan(localValidationStep);
    }

    [Fact]
    public void Main_Release_Workflow_Uploads_From_Visible_Package_Directory_And_Fails_When_No_Artifacts_Match()
    {
        var workflow = ReadRepoFile(".github/workflows/release-nuget-packages.yml");
        var baseOutputLine = workflow.Split('\n')
                                     .Single(line => line.TrimStart()
                                                         .StartsWith("BASE_OUTPUT_DIR:", StringComparison.Ordinal));
        var baseOutputDir = baseOutputLine.Split(':', 2)[1].Trim();
        var baseOutputLeaf = baseOutputDir.Split('/').Last();

        baseOutputLeaf.ShouldNotStartWith(".");
        var uploadStep = GetWorkflowStep(workflow, "-  name: Upload NuGet artifacts");
        uploadStep.ShouldContain("path: ${{ env.BASE_OUTPUT_DIR }}/*.nupkg");
        uploadStep.ShouldContain("if-no-files-found: error");
    }

    [Fact]
    public void Package_Build_Script_Builds_Prerequisites_Before_Packing_And_Verifies_All_Artifacts()
    {
        var manifest = ReadRepoFile(PackageManifest);
        var script = ReadRepoFile(PackageBuildScript);
        var prerequisiteBuild = script.IndexOf(
            "foreach ($project in $AtomUIReleaseBuildPrerequisiteProjects)",
            StringComparison.Ordinal);
        var packageBuild = script.IndexOf(
            "foreach ($project in $AtomUIReleasePackageProjects)",
            StringComparison.Ordinal);
        var packagePack = script.IndexOf(
            "foreach ($project in $AtomUIReleasePackageProjects)",
            packageBuild + 1,
            StringComparison.Ordinal);

        prerequisiteBuild.ShouldBeGreaterThanOrEqualTo(0);
        packageBuild.ShouldBeGreaterThan(prerequisiteBuild);
        packagePack.ShouldBeGreaterThan(packageBuild);
        manifest.ShouldContain("src/AtomUI.Toolchain/AtomUI.Toolchain.csproj");
        script.ShouldContain("--disable-build-servers");
        script.ShouldContain("-m:1");
        script.ShouldContain("/nr:false");
        script.ShouldContain("--no-build");
        script.ShouldContain("$AtomUIExpectedPackageIds");
        script.ShouldContain("Missing NuGet packages");
        script.ShouldContain("Unexpected NuGet packages");
    }

    [Fact]
    public void Local_NuGet_Publish_Script_Pushes_Only_After_Verified_Build()
    {
        var script = ReadRepoFile("scripts/PublishToLocalSources.ps1");

        var build = script.IndexOf("BuildNuGetPackages.ps1", StringComparison.Ordinal);
        var push = script.LastIndexOf("Push-NuGetPackages", StringComparison.Ordinal);

        build.ShouldBeGreaterThanOrEqualTo(0);
        push.ShouldBeGreaterThan(build);
        script.ShouldNotContain("$AtomUIBasePackageProjects");
        script.ShouldNotContain("$AtomUIExtensionPackageProjects");
        script.ShouldNotContain("$AtomUILanguagePackageProjects");
    }

    private static string ReadRepoFile(string relativePath)
    {
        var path = Path.Combine(GetRepoRoot(), relativePath);
        File.Exists(path).ShouldBeTrue($"Expected repository file to exist: {relativePath}");
        return File.ReadAllText(path);
    }

    private static string GetRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "AtomUI.slnx");
            if (File.Exists(candidate))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return AppContext.BaseDirectory;
    }

    private static string GetWorkflowStep(string workflow, string stepName)
    {
        var start = workflow.IndexOf(stepName, StringComparison.Ordinal);
        start.ShouldBeGreaterThanOrEqualTo(0);
        var nextStep = workflow.IndexOf("\n         -", start + stepName.Length, StringComparison.Ordinal);
        return nextStep < 0
            ? workflow[start..]
            : workflow[start..nextStep];
    }
}
