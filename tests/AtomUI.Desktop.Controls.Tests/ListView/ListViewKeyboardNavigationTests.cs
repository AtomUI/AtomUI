using AtomUI.Controls.Data;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomListViewItem = AtomUI.Desktop.Controls.ListViewItem;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.ListView;

public class ListViewKeyboardNavigationTests
{
    static ListViewKeyboardNavigationTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void MoveSelection_Uses_The_Owning_TopLevel_FocusManager()
    {
        var firstList = CreateList("first-a", "first-b");
        var secondList = CreateList("second-a", "second-b");
        var firstWindow = new AvaloniaWindow { Content = firstList, Width = 300, Height = 200 };
        var secondWindow = new AvaloniaWindow { Content = secondList, Width = 300, Height = 200 };
        firstWindow.Show();
        secondWindow.Show();
        Dispatcher.UIThread.RunJobs();

        try
        {
            var firstContainers = firstList.GetVisualDescendants().OfType<AtomListViewItem>().ToArray();
            var secondContainers = secondList.GetVisualDescendants().OfType<AtomListViewItem>().ToArray();
            firstContainers.Length.ShouldBe(2);
            secondContainers.Length.ShouldBe(2);

            firstContainers[1].Focus();
            secondContainers[0].Focus();
            Dispatcher.UIThread.RunJobs();

            firstList.MoveDown().ShouldBeFalse(
                "the first ListView must keep using its own TopLevel focus instead of the second window's focus");
        }
        finally
        {
            secondWindow.Close();
            firstWindow.Close();
        }
    }

    [Fact]
    public void MoveSelection_Does_Not_Consume_Global_Focus_When_Detached()
    {
        var detached = CreateList("a", "b");

        detached.MoveDown().ShouldBeFalse();
    }

    [Fact]
    public void ListView_Uses_Public_TopLevel_Focus_While_AutoComplete_Keeps_Scoped_Helper()
    {
        var repoRoot = GetRepoRoot();
        var listViewSource = File.ReadAllText(Path.Combine(
            repoRoot,
            "src/AtomUI.Desktop.Controls/ListView/ListView.Selecting.cs"));
        var autoCompleteSource = File.ReadAllText(Path.Combine(
            repoRoot,
            "src/AtomUI.Desktop.Controls/AutoComplete/AbstractAutoComplete.cs"));

        listViewSource.ShouldContain("TopLevel.GetTopLevel(this)?.FocusManager.GetFocusedElement()");
        listViewSource.ShouldNotContain("FocusManagerReflectionExtensions.GetFocusManager(this)?.GetFocusedElement()");
        autoCompleteSource.ShouldContain(
            "FocusManagerReflectionExtensions.GetFocusManager(this)?.GetFocusedElement(scope)");
    }

    private static TestListView CreateList(params string[] labels) => new()
    {
        Width = 240,
        Height = 160,
        ItemsSource = labels.Select(label => new ListItemData { Content = label }).ToArray()
    };

    private static string GetRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "src/AtomUI.Desktop.Controls")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Unable to locate the AtomUI repository root.");
    }

    private sealed class TestListView : AtomUI.Desktop.Controls.ListView
    {
        protected override Type StyleKeyOverride => typeof(AtomUI.Desktop.Controls.ListView);

        internal bool MoveDown() => MoveSelection(NavigationDirection.Down);
    }
}
