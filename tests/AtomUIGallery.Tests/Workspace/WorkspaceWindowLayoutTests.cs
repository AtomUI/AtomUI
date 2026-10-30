using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Xml.Linq;
using AtomUIGallery.Workspace.Views;
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
        var source = File.ReadAllText(GetRepoFile("controlgallery/AtomUIGallery/Workspace/Views/WorkspaceWindow.axaml.cs"));

        source.ShouldContain("ViewModel.AvailableThemes");
        source.ShouldContain("new StableCommand(ViewModel.SwitchThemeCommand)");
        Regex.IsMatch(source, @"\bToggleType\s*=\s*MenuItemToggleType\.Radio\b").ShouldBeTrue();
        Regex.IsMatch(source, @"\bGroupName\s*=\s*ThemeColorGroupName\b").ShouldBeTrue();
        source.ShouldContain("string.Equals(theme.Id, ViewModel.CurrentThemeId");
        Regex.IsMatch(source, @"\bCommand\s*=\s*switchThemeCommand\b").ShouldBeTrue();
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
        stableCommand.CanExecute("PolarGreen").ShouldBeTrue();
        stableCommand.Execute("PolarGreen");
        innerCommand.ExecuteParameters.ShouldBe(["PolarGreen"]);
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
        var commandType = typeof(WorkspaceWindow).GetNestedType("StableCommand", BindingFlags.NonPublic);
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
