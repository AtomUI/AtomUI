using AtomUI.Toolkits.GalleryBase.Localization;
using AtomUI.Toolkits.GalleryBase.Navigation;
using Shouldly;
using Xunit;

namespace AtomUIGallery.Tests.Toolkits;

public class AtomUIGalleryModuleTests
{
    [Fact]
    public void AtomUI_Gallery_Module_Registers_All_Navigation_Pages_As_Routes()
    {
        var configuration = AtomUIGalleryModule.CreateConfiguration();

        configuration.DefaultRoute.Value.ShouldBe("Overview");
        configuration.NavigationNodes.Count.ShouldBeGreaterThan(0);
        configuration.Routes.Routes.Count.ShouldBeGreaterThan(60);
        var dateViewerRoute = configuration.Routes.GetRequiredRoute("DateViewer");
        dateViewerRoute.ViewType.ShouldBe(typeof(AtomUIGallery.ShowCases.DateViewer.DateViewerShowCase));
        dateViewerRoute.ViewModelType.ShouldBe(typeof(AtomUIGallery.ShowCases.DateViewer.DateViewerViewModel));

        foreach (var node in Walk(configuration.NavigationNodes))
        {
            if (node.IsRoute)
            {
                configuration.Routes.ContainsRoute(node.Key)
                             .ShouldBeTrue($"Expected route for navigation node '{node.Key}'.");
            }
        }
    }

    [Fact]
    public void GalleryBase_Does_Not_Reference_AtomUI_Gallery_Product_Module()
    {
        var sourceRoot = GetRepoPath("src/AtomUI.Toolkits.GalleryBase");
        var sourceFiles = Directory.EnumerateFiles(sourceRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}GeneratedFiles{Path.DirectorySeparatorChar}"));

        foreach (var sourceFile in sourceFiles)
        {
            var source = File.ReadAllText(sourceFile);
            source.ShouldNotContain("AtomUIGallery");
        }
    }

    [Fact]
    public void AtomUI_Gallery_Workspace_Delegates_Navigation_Runtime_To_GalleryBase()
    {
        var viewModelSource = File.ReadAllText(
            GetRepoPath("controlgallery/AtomUIGallery/Workspace/ViewModels/CaseNavigationViewModel.cs"));
        var viewSource = File.ReadAllText(
            GetRepoPath("controlgallery/AtomUIGallery/Workspace/Views/CaseNavigation.axaml"));
        var registerSource = File.ReadAllText(
            GetRepoPath("controlgallery/AtomUIGallery/ShowCases/ShowCaseRegister.cs"));

        viewModelSource.ShouldContain("GalleryNavigationViewModel");
        viewModelSource.ShouldNotContain("RegisterShowCaseViewModels");
        viewModelSource.ShouldNotContain("new ButtonViewModel");
        viewModelSource.ShouldNotContain("new DataGridViewModel");

        viewSource.ShouldContain("Name=\"ShowCaseNavMenu\"");
        viewSource.ShouldNotContain("<atom:NavMenuNode");

        registerSource.ShouldContain("AtomUIGalleryModule.RegisterViews(locator)");
        registerSource.ShouldNotContain("locator.Map<ButtonViewModel, ButtonShowCase>");
    }

    [Fact]
    public void AtomUI_Gallery_Module_Uses_Localized_Navigation_Headers()
    {
        var configuration = AtomUIGalleryModule.CreateConfiguration();

        foreach (var node in Walk(configuration.NavigationNodes))
        {
            node.Header.ShouldBeAssignableTo<IGalleryLocalizedText>();
        }
    }

    [Fact]
    public void Desktop_And_Browser_Register_Views_Through_Product_Module()
    {
        var desktopProgram = File.ReadAllText(GetRepoPath("controlgallery/AtomUIGallery.Desktop/Program.cs"));
        var browserProgram = File.ReadAllText(GetRepoPath("controlgallery/AtomUIGallery.Browser/Program.cs"));

        desktopProgram.ShouldContain("AtomUIGalleryModule.RegisterViews(locator)");
        browserProgram.ShouldContain("AtomUIGalleryModule.RegisterViews(locator)");
        desktopProgram.ShouldNotContain("new ShowCaseViewModule()");
        browserProgram.ShouldNotContain("new ShowCaseViewModule()");
    }

    [Fact]
    public void Workspace_ViewModel_Uses_GalleryBase_Shell_ViewModel()
    {
        var workspaceViewModel = File.ReadAllText(
            GetRepoPath("controlgallery/AtomUIGallery/Workspace/ViewModels/WorkspaceWindowViewModel.cs"));
        var galleryBaseShellViewModel = GetRepoPath(
            "src/AtomUI.Toolkits.GalleryBase/Shell/GalleryWorkspaceViewModel.cs");

        File.Exists(galleryBaseShellViewModel).ShouldBeTrue();
        workspaceViewModel.ShouldContain("GalleryWorkspaceViewModel");
        workspaceViewModel.ShouldNotContain("ToggleDarkModeCommand = ReactiveCommand.Create");
        workspaceViewModel.ShouldNotContain("SwitchToZhCNCommand = ReactiveCommand.Create");
    }

    private static IEnumerable<GalleryNavigationNode> Walk(IEnumerable<GalleryNavigationNode> nodes)
    {
        foreach (var node in nodes)
        {
            yield return node;
            foreach (var child in Walk(node.Children))
            {
                yield return child;
            }
        }
    }

    private static string GetRepoPath(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate) || Directory.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        return Path.Combine(AppContext.BaseDirectory, relativePath);
    }
}
