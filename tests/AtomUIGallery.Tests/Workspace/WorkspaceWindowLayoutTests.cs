using AtomUI.DesignLanguages.AntDesign;
using System.Reflection;
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
        var (languageItem, brandItem, neutralItem, otherItem) = (new MenuItem(), new MenuItem(), new MenuItem(), new MenuItem());
        using var menu = new GalleryThemeMenu(viewModel, languageItem, brandItem, neutralItem, otherItem);
        try
        {
            // Single-style languages stay top-level; the language items follow the enabled design languages.
            var menuItems = languageItem.Items.OfType<MenuItem>().ToList();
            var topLevelTags = menuItems.Where(item => item.Items.Count == 0).Select(item => (string?)item.Tag).ToList();
            topLevelTags.ShouldBe(viewModel.DesignLanguages.Where(language => !language.Contains('.')));
            topLevelTags.ShouldContain("AntDesign");
            // The named themes are listed under the other themes item; the implicit language defaults are not.
            var namedThemes = otherItem.Items.OfType<MenuItem>().ToList();
            namedThemes.Select(item => (string?)item.CommandParameter)
                       .ShouldBe(viewModel.AvailableThemes.Where(theme => !theme.IsLanguageDefault)
                                          .Select(theme => theme.Id));
            var preview = namedThemes.Single(item => Equals(item.CommandParameter, "GalleryPreviewFixture"));
            preview.Command!.Execute(preview.CommandParameter);
            Dispatcher.UIThread.RunJobs();
            viewModel.CurrentThemeId.ShouldBe("GalleryPreviewFixture");
            otherItem.Items.OfType<MenuItem>()
                     .Single(item => item.IsChecked == true).CommandParameter.ShouldBe("GalleryPreviewFixture");

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
                AntDesignThemes.DaybreakBlue, null, ThemeTransitionReason.UserRequest),
                TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task Theme_Menu_Folds_Multi_Style_Families()
    {
        AvaloniaTestApp.EnsureInitialized();
        var manager   = Application.Current!.GetThemeManager()!;
        var viewModel = new WorkspaceWindowViewModel();
        var (languageItem, brandItem, neutralItem, otherItem) = (new MenuItem(), new MenuItem(), new MenuItem(), new MenuItem());
        using var menu = new GalleryThemeMenu(viewModel, languageItem, brandItem, neutralItem, otherItem);
        try
        {
            // Multi-style families (Shadcn, PrimeNG) fold into a submenu whose children carry the language keys.
            var familyItems = languageItem.Items.OfType<MenuItem>().Where(item => item.Items.Count > 0).ToList();
            familyItems.Select(item => (string)item.Header!).ShouldBe(new[] { "PrimeNG", "Shadcn" });
            familyItems.Single(f => (string)f.Header! == "Shadcn").Items.OfType<MenuItem>()
                       .Select(item => (string?)item.Tag)
                       .ShouldBe(viewModel.DesignLanguages.Where(l => l.StartsWith("Shadcn.", StringComparison.Ordinal)));

            // A folded family: switching to one of its styles checks the child radio inside the submenu and
            // bolds the family parent.
            await viewModel.SwitchDesignLanguageCommand.Execute("Shadcn.Nova").ToTask();
            Dispatcher.UIThread.RunJobs();
            var shadcn = languageItem.Items.OfType<MenuItem>().Single(item => (string?)item.Tag == "Shadcn");
            shadcn.Items.OfType<MenuItem>().Single(item => item.IsChecked == true).Tag.ShouldBe("Shadcn.Nova");
            shadcn.FontWeight.ShouldBe(Avalonia.Media.FontWeight.Bold);
            languageItem.Items.OfType<MenuItem>().Where(item => item.IsChecked && item.Items.Count == 0)
                         .ShouldBeEmpty("top-level radios clear when a folded style is chosen");
        }
        finally
        {
            viewModel.Dispose();
            await manager.ApplyThemeAsync(new ThemeRequest(
                AntDesignThemes.DaybreakBlue, null, ThemeTransitionReason.UserRequest),
                TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task Brand_Swatches_Disabled_For_A_Locked_Brand_Seed()
    {
        AvaloniaTestApp.EnsureInitialized();
        var manager   = Application.Current!.GetThemeManager()!;
        var viewModel = new WorkspaceWindowViewModel();
        var (languageItem, brandItem, neutralItem, otherItem) = (new MenuItem(), new MenuItem(), new MenuItem(), new MenuItem());
        using var menu = new GalleryThemeMenu(viewModel, languageItem, brandItem, neutralItem, otherItem);
        try
        {
            await viewModel.SwitchDesignLanguageCommand.Execute("AntDesign").ToTask();
            Dispatcher.UIThread.RunJobs();
            brandItem.IsEnabled.ShouldBeTrue();

            // A theme that locks the brand seed disables the swatches and explains why.
            await viewModel.SwitchThemeCommand.Execute("GalleryFixedBrandFixture").ToTask();
            Dispatcher.UIThread.RunJobs();

            brandItem.IsEnabled.ShouldBeFalse();
            brandItem.Items.Count.ShouldBe(0);
            var fixedHint = ResolveTipText(brandItem);
            fixedHint.ShouldNotBeNullOrEmpty();

            // A named theme that leaves the seed open offers the swatches again.
            await viewModel.SwitchThemeCommand.Execute("GalleryPreviewFixture").ToTask();
            Dispatcher.UIThread.RunJobs();

            brandItem.IsEnabled.ShouldBeTrue();
            brandItem.Items.Count.ShouldBeGreaterThan(0);
        }
        finally
        {
            viewModel.Dispose();
            await manager.ApplyThemeAsync(new ThemeRequest(
                AntDesignThemes.DaybreakBlue, null, ThemeTransitionReason.UserRequest),
                TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task Theme_Menu_Coalesces_Rebuilds_Into_One_Per_Dispatcher_Turn()
    {
        AvaloniaTestApp.EnsureInitialized();
        var manager   = Application.Current!.GetThemeManager()!;
        var viewModel = new WorkspaceWindowViewModel();
        var (languageItem, brandItem, neutralItem, otherItem) = (new MenuItem(), new MenuItem(), new MenuItem(), new MenuItem());
        var menu = new GalleryThemeMenu(viewModel, languageItem, brandItem, neutralItem, otherItem);
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
                AntDesignThemes.DaybreakBlue, null, ThemeTransitionReason.UserRequest),
                TestContext.Current.CancellationToken);
        }
    }

    // The hint binds a localized resource; host it briefly so the dynamic resource resolves.
    private static string? ResolveTipText(MenuItem item)
    {
        var tip = (Avalonia.Controls.TextBlock)AtomUI.Desktop.Controls.ToolTip.GetTip(item)!;
        var host = new Avalonia.Controls.Window { Content = tip };
        host.Show();
        Dispatcher.UIThread.RunJobs();
        var text = tip.Text;
        host.Close();
        host.Content = null;
        return text;
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
