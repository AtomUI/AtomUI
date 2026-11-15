using AtomUI.Controls.Data;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomListViewItem = AtomUI.Desktop.Controls.ListViewItem;
using AtomListView = AtomUI.Desktop.Controls.ListView;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.ListView;

public class ListViewKeyboardNavigationTests
{
    static ListViewKeyboardNavigationTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    // Focus held by another window's list is never used as the starting container: the list starts from its own
    // first item and leaves the other list alone (items are focusable, so both windows really hold a focused item).
    [Fact]
    public void MoveSelection_Ignores_Focus_Outside_The_List()
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

            firstContainers[1].Focus().ShouldBeTrue();
            secondContainers[0].Focus().ShouldBeTrue();
            Dispatcher.UIThread.RunJobs();

            firstList.MoveDown().ShouldBeTrue();
            firstList.SelectedIndex.ShouldBe(0, "a list without focus inside starts from its first item");
            secondList.SelectedIndex.ShouldBe(-1);
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

    private const int Count = 200;

    // Keyboard contract of the standalone ListView in the default theme with its default VirtualizingStackPanel: items
    // are the focus targets, the list is one Tab stop, single selection follows the focused item one item per press,
    // and keyboard focus shows the inset outline on the item frame.
    [Fact]
    public void Arrows_Move_Focus_And_Single_Selection_Through_The_Virtualized_List()
    {
        var disabled = new HashSet<int> { 6, 7, 8, Count - 1 };
        var items = Enumerable.Range(0, Count)
                              .Select(i => new ListItemData { Content = $"Item {i}", IsEnabled = !disabled.Contains(i) })
                              .ToArray();
        var listView = new AtomListView { Width = 240, Height = 160, ItemsSource = items };
        ListKeyboardTestHost.Run(listView, host =>
        {
            ListKeyboardTestHost.ShouldBeVirtualizing(listView);
            listView.Focusable.ShouldBeFalse("the list itself is not a focus target");
            host.Before.Focus(NavigationMethod.Tab).ShouldBeTrue();
            host.Press(Key.Tab);
            ListKeyboardTestHost.FocusedIndex(listView).ShouldBe(0, "Tab enters the list on its first item");
            listView.SelectedIndex.ShouldBe(-1, "entering with Tab does not select");
            host.Press(Key.Tab);
            host.Focused.ShouldBeSameAs(host.After, "the list is one Tab stop");

            host.Click(listView.ContainerFromIndex(0).ShouldNotBeNull());
            AssertCurrent(listView, 0);
            ListKeyboardTestHost.FocusOutlines(listView).ShouldBeEmpty("pointer focus shows no outline");
            host.Press(Key.Down).ShouldBeTrue();
            AssertCurrent(listView, 1);
            var outline = ListKeyboardTestHost.FocusOutlines(listView).ShouldHaveSingleItem();
            var adorned = (Control)Avalonia.Controls.Primitives.AdornerLayer.GetAdornedElement(outline)!;
            adorned.Name.ShouldBe("Frame");
            adorned.TemplatedParent.ShouldBeSameAs(listView.ContainerFromIndex(1));
            FocusOutlineAdorner.GetIsInset(adorned).ShouldBeTrue();

            host.Press(Key.Down, 5);
            AssertCurrent(listView, 9);
            host.Press(Key.Down, 21);
            AssertCurrent(listView, 30);
            host.Press(Key.End);
            AssertCurrent(listView, Count - 2);
            host.Press(Key.Home);
            AssertCurrent(listView, 0);

            host.Press(Key.Down, 5);
            ListKeyboardTestHost.ScrollToEnd(listView);
            listView.ContainerFromIndex(1).ShouldBeNull("scrolling to the end recycles the top containers");
            ListKeyboardTestHost.ScrollTo(listView, 0);
            AssertCurrent(listView, 5);
            host.Press(Key.Down);
            AssertCurrent(listView, 9);
        });
    }

    // Group headers are never focus targets (Tab and arrows skip them); in multiple mode arrows only browse,
    // Space / Enter toggle the focused item and Shift selects the range from the anchor.
    [Fact]
    public void Grouped_Multiple_List_Skips_Headers_And_Applies_The_Multiple_Commit_Policy()
    {
        var items = new[]
        {
            new ListItemData { Content = "Red", Group = "Warm" },
            new ListItemData { Content = "Orange", Group = "Warm" },
            new ListItemData { Content = "Blue", Group = "Cold" },
            new ListItemData { Content = "Cyan", Group = "Cold" }
        };
        var listView = new AtomListView
        {
            Width = 240, Height = 260, IsGroupEnabled = true, SelectionMode = SelectionMode.Multiple, ItemsSource = items
        };
        ListKeyboardTestHost.Run(listView, host =>
        {
            listView.GetRealizedContainers().OfType<GroupHeaderItem>().ShouldNotBeEmpty();
            listView.GetRealizedContainers().OfType<GroupHeaderItem>().ShouldAllBe(header => !header.Focusable);

            host.Before.Focus(NavigationMethod.Tab).ShouldBeTrue();
            host.Press(Key.Tab);
            FocusedContent(listView).ShouldBe("Red", "Tab skips the leading group header");
            host.Press(Key.Down);
            host.Press(Key.Down);
            FocusedContent(listView).ShouldBe("Blue", "Down skips the group header between the groups");
            listView.SelectedItems.ShouldBeEmpty("browsing does not change a multiple selection");

            host.Press(Key.Space).ShouldBeTrue();
            listView.SelectedItems.ShouldBe([items[2]]);
            host.Press(Key.Up, RawInputModifiers.Shift);
            host.Press(Key.Up, RawInputModifiers.Shift);
            FocusedContent(listView).ShouldBe("Red");
            listView.SelectedItems.Count.ShouldBe(3, "Shift selects the range from the anchor across the group header");
            host.Press(Key.Enter).ShouldBeTrue();
            listView.SelectedItems.Count.ShouldBe(2, "Enter toggles the focused item");

            host.Press(Key.A, ListKeyboardTestHost.CommandModifier());
            listView.SelectedItems.Count.ShouldBe(items.Length);
        });
    }

    private static string? FocusedContent(AtomListView listView)
    {
        var index = ListKeyboardTestHost.FocusedIndex(listView);
        return index >= 0 && listView.ContainerFromIndex(index) is AtomListViewItem { Content: ListItemData data }
            ? data.Content?.ToString()
            : null;
    }

    private static void AssertCurrent(AtomListView listView, int expected)
    {
        listView.SelectedIndex.ShouldBe(expected, "selected index");
        var container = listView.ContainerFromIndex(expected).ShouldNotBeNull(
            $"the container of the selected index {expected} must be realized");
        ((AtomListViewItem)container).IsSelected.ShouldBeTrue();
        ListKeyboardTestHost.FocusedIndex(listView).ShouldBe(expected, "keyboard focus follows the selection");
        listView.GetRealizedContainers()
                .Where(c => !ReferenceEquals(c, container) && c is AtomListViewItem { IsSelected: true })
                .ShouldBeEmpty("no other realized container may look selected");
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
