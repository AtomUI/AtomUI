using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Xml.Linq;
using AtomUI.Theme;
using AtomUIGallery.Workspace.ViewModels;
using AtomUIGallery.Workspace.Views;
using Avalonia;
using Avalonia.Media;
using Avalonia.Threading;
using AtomUI.Toolkits.GalleryBase.Shell;
using ReactiveUI;
using System.Reactive.Threading.Tasks;
using MenuItem = AtomUI.Desktop.Controls.MenuItem;
using Shouldly;
using Xunit;

namespace AtomUIGallery.Tests.Workspace;

public class WorkspaceWindowLayoutTests
{
    [Fact]
    public void Title_Bar_Separator_Does_Not_Intercept_Pointer_Input()
    {
        var document = XDocument.Load(GetRepoFile("controlgallery/AtomUIGallery/Workspace/Views/WorkspaceWindow.axaml"));
        var separator = document.Descendants().Single(element =>
            (string?)element.Attribute("Name") == "TitleBarBottomSeparator");

        separator.Name.LocalName.ShouldBe("Border");
        ((string?)separator.Attribute("IsHitTestVisible")).ShouldBe("False");
    }

    [Fact]
    public void Workspace_Window_Menu_Keeps_Motion_And_WaveSpirit_Checks_In_Sync()
    {
        var source = File.ReadAllText(GetRepoFile("controlgallery/AtomUIGallery/Workspace/Views/WorkspaceWindow.axaml.cs"));

        source.ShouldContain("FindSiblingMenuItem(menuItem, WindowMenuItemKind.WaveSpirit)");
        source.ShouldContain("waveSpiritMenuItem.IsChecked = false");
        source.ShouldContain("FindSiblingMenuItem(menuItem, WindowMenuItemKind.Motion)");
        source.ShouldContain("motionMenuItem.IsChecked = true");
        source.ShouldNotContain("TitleBarWaveSpiritToggleButton");
    }

    [Fact]
    public void Workspace_Window_Caption_Menu_Controls_Button_Visibility_Without_Changing_Capability()
    {
        var source = File.ReadAllText(GetRepoFile("controlgallery/AtomUIGallery/Workspace/Views/WorkspaceWindow.axaml.cs"));

        source.ShouldContain("IsMinimizeCaptionButtonVisible = menuItem.IsChecked");
        source.ShouldContain("IsMaximizeCaptionButtonVisible = menuItem.IsChecked");
        source.ShouldNotContain("CanMinimize = menuItem.IsChecked");
        source.ShouldNotContain("CanMaximize = menuItem.IsChecked");
    }

    [Fact]
    public void Workspace_Window_Theme_Choices_Use_Available_Theme_Ids()
    {
        var source = File.ReadAllText(GetRepoFile("controlgallery/AtomUIGallery/Workspace/Views/GalleryThemeMenu.cs"));

        source.ShouldContain("_viewModel.AvailableThemes");
        source.ShouldContain("new StableCommand(viewModel.SwitchThemeCommand)");
        Regex.IsMatch(source, @"\bToggleType\s*=\s*MenuItemToggleType\.Radio\b").ShouldBeTrue();
        Regex.IsMatch(source, @"\bGroupName\s*=\s*OtherThemeGroupName\b").ShouldBeTrue();
        source.ShouldContain("string.Equals(theme.Id, _viewModel.CurrentThemeId");
        Regex.IsMatch(source, @"\bCommand\s*=\s*_switchThemeCommand\b").ShouldBeTrue();
        source.ShouldContain("CommandParameter = theme.Id");
    }

    [Fact]
    public void Workspace_Window_Theme_Command_Does_Not_Forward_CanExecuteChanged_To_Menu_Items()
    {
        var innerCommand  = new RecordingCommand();
        var stableCommand = CreateStableThemeCommand(innerCommand);
        var raiseCount    = 0;
        stableCommand.CanExecuteChanged += (_, _) => raiseCount++;

        innerCommand.RaiseCanExecuteChanged();

        raiseCount.ShouldBe(0);
        stableCommand.CanExecute("DaybreakBlue").ShouldBeTrue();
        stableCommand.Execute("DaybreakBlue");
        innerCommand.ExecuteParameters.ShouldBe(["DaybreakBlue"]);
    }

    [Fact]
    public async Task Theme_Menu_Groups_By_Design_Language()
    {
        AvaloniaTestApp.EnsureInitialized();
        var manager   = Application.Current!.GetThemeManager()!;
        var viewModel = new WorkspaceWindowViewModel();
        var (languageItem, brandItem, otherItem) = (new MenuItem(), new MenuItem(), new MenuItem());
        using var menu = new GalleryThemeMenu(viewModel, languageItem, brandItem, otherItem);
        try
        {
            languageItem.Items.OfType<MenuItem>().Select(item => (string?)item.Tag)
                        .ShouldBe(ThemeSelectionResolver.DesignLanguages(viewModel.AvailableThemes));
            otherItem.Items.OfType<MenuItem>().Select(item => (string?)item.Tag)
                     .ShouldBe(viewModel.AvailableThemes.Where(theme => theme.DesignLanguage is null)
                                        .Select(theme => theme.Id));

            await viewModel.SwitchDesignLanguageCommand.Execute("AntDesign").ToTask();
            Dispatcher.UIThread.RunJobs();

            languageItem.Items.OfType<MenuItem>().Single(item => item.IsChecked == true).Tag.ShouldBe("AntDesign");
            brandItem.IsEnabled.ShouldBeTrue();
            brandItem.Items.OfType<MenuItem>().Select(item => item.Tag).ShouldContain(Color.Parse("#52C41A"));
        }
        finally
        {
            viewModel.Dispose();
            await manager.ApplyThemeAsync(new ThemeRequest(
                IThemeManager.DEFAULT_THEME_ID, null, ThemeTransitionReason.UserRequest),
                TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task Brand_Swatches_Disabled_For_Fixed_Language()
    {
        AvaloniaTestApp.EnsureInitialized();
        var manager   = Application.Current!.GetThemeManager()!;
        var viewModel = new WorkspaceWindowViewModel();
        var (languageItem, brandItem, otherItem) = (new MenuItem(), new MenuItem(), new MenuItem());
        using var menu = new GalleryThemeMenu(viewModel, languageItem, brandItem, otherItem);
        try
        {
            await viewModel.SwitchDesignLanguageCommand.Execute("AntDesign").ToTask();
            Dispatcher.UIThread.RunJobs();
            brandItem.IsEnabled.ShouldBeTrue();

            await viewModel.SwitchDesignLanguageCommand.Execute("Shadcn.Zinc").ToTask();
            Dispatcher.UIThread.RunJobs();

            brandItem.IsEnabled.ShouldBeFalse();
            brandItem.Items.Count.ShouldBe(0);
        }
        finally
        {
            viewModel.Dispose();
            await manager.ApplyThemeAsync(new ThemeRequest(
                IThemeManager.DEFAULT_THEME_ID, null, ThemeTransitionReason.UserRequest),
                TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task Theme_Menu_Coalesces_Rebuilds_Into_One_Per_Dispatcher_Turn()
    {
        AvaloniaTestApp.EnsureInitialized();
        var manager   = Application.Current!.GetThemeManager()!;
        var viewModel = new WorkspaceWindowViewModel();
        var (languageItem, brandItem, otherItem) = (new MenuItem(), new MenuItem(), new MenuItem());
        var menu = new GalleryThemeMenu(viewModel, languageItem, brandItem, otherItem);
        try
        {
            await viewModel.SwitchDesignLanguageCommand.Execute("AntDesign").ToTask();
            Dispatcher.UIThread.RunJobs();
            var languageCount = languageItem.Items.Count;
            var brandCount    = brandItem.Items.Count;
            menu.RebuildCount = 0;

            viewModel.RaisePropertyChanged(nameof(GalleryWorkspaceViewModel.CurrentThemeId));
            viewModel.RaisePropertyChanged(nameof(GalleryWorkspaceViewModel.CurrentDesignLanguage));
            viewModel.RaisePropertyChanged(nameof(GalleryWorkspaceViewModel.CanChooseBrandColor));
            viewModel.RaisePropertyChanged(nameof(GalleryWorkspaceViewModel.BrandColor));
            menu.RebuildCount.ShouldBe(0);
            Dispatcher.UIThread.RunJobs();

            menu.RebuildCount.ShouldBe(1);
            languageItem.Items.Count.ShouldBe(languageCount);
            brandItem.Items.Count.ShouldBe(brandCount);

            viewModel.RaisePropertyChanged(nameof(GalleryWorkspaceViewModel.BrandColor));
            menu.Dispose();
            Dispatcher.UIThread.RunJobs();
            menu.RebuildCount.ShouldBe(1);
        }
        finally
        {
            menu.Dispose();
            viewModel.Dispose();
            await manager.ApplyThemeAsync(new ThemeRequest(
                IThemeManager.DEFAULT_THEME_ID, null, ThemeTransitionReason.UserRequest),
                TestContext.Current.CancellationToken);
        }
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

        throw new FileNotFoundException($"Could not find repository file: {relativePath}");
    }

    private static ICommand CreateStableThemeCommand(ICommand innerCommand)
    {
        var commandType = typeof(GalleryThemeMenu).GetNestedType("StableCommand", BindingFlags.NonPublic);
        commandType.ShouldNotBeNull();
        var constructor = commandType!.GetConstructor(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            types: [typeof(ICommand)],
            modifiers: null);
        constructor.ShouldNotBeNull();
        return (ICommand)constructor.Invoke([innerCommand]);
    }

    private sealed class RecordingCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public List<object?> ExecuteParameters { get; } = [];

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            ExecuteParameters.Add(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
