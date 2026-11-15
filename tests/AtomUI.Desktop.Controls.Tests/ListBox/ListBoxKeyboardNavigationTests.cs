using AtomUI.Controls.Data;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Shouldly;
using Xunit;
using AtomListBox = AtomUI.Desktop.Controls.ListBox;
using AtomListBoxItem = AtomUI.Desktop.Controls.ListBoxItem;
using Host = AtomUI.Desktop.Controls.Tests.ListKeyboardTestHost.Host;

namespace AtomUI.Desktop.Controls.Tests.ListBox;

// Keyboard contract of the standalone ListBox in the default theme with its default VirtualizingStackPanel: items are
// the focus targets, the list is one Tab stop, each arrow press moves exactly one navigable item (Avalonia's
// ListBox.OnKeyDown must not move it a second time), and keyboard focus shows the inset outline on the item frame.
public class ListBoxKeyboardNavigationTests
{
    private const int Count = 200;

    static ListBoxKeyboardNavigationTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Arrows_Move_Focus_And_Single_Selection_One_Item_Through_The_Virtualized_List()
    {
        var disabled = new HashSet<int> { 6, 7, 8, 100, 101, 102, Count - 1 };
        RunList(disabled, (host, listBox) =>
        {
            listBox.Focusable.ShouldBeFalse("the list itself is not a focus target");
            host.Before.Focus(NavigationMethod.Tab).ShouldBeTrue();
            host.Press(Key.Tab);
            ListKeyboardTestHost.FocusedIndex(listBox).ShouldBe(0, "Tab enters the list on its first item");
            listBox.SelectedIndex.ShouldBe(-1, "entering with Tab does not select");
            host.Press(Key.Tab);
            host.Focused.ShouldBeSameAs(host.After, "the list is one Tab stop");

            host.Click(listBox.ContainerFromIndex(0).ShouldNotBeNull());
            AssertCurrent(listBox, 0);
            ListKeyboardTestHost.FocusOutlines(listBox).ShouldBeEmpty("pointer focus shows no outline");

            var changes = 0;
            listBox.SelectionChanged += (_, _) => changes++;
            host.Press(Key.Down).ShouldBeTrue();
            changes.ShouldBe(1, "one Down press changes the selection once");
            AssertCurrent(listBox, 1);
            var outline = ListKeyboardTestHost.FocusOutlines(listBox).ShouldHaveSingleItem();
            var adorned = (Control)AdornerLayer.GetAdornedElement(outline)!;
            adorned.Name.ShouldBe("Frame");
            adorned.TemplatedParent.ShouldBeSameAs(listBox.ContainerFromIndex(1));
            FocusOutlineAdorner.GetIsInset(adorned).ShouldBeTrue();

            // Disabled 6..8 start outside the realized range and are skipped.
            host.Press(Key.Down, 5);
            AssertCurrent(listBox, 9);
            host.Press(Key.Down, 90);
            AssertCurrent(listBox, 99);
            host.Press(Key.Down);
            AssertCurrent(listBox, 103);
            host.Press(Key.End);
            AssertCurrent(listBox, Count - 2);
            host.Press(Key.Home);
            AssertCurrent(listBox, 0);

            host.Press(Key.Down, 5);
            ListKeyboardTestHost.ScrollToEnd(listBox);
            listBox.ContainerFromIndex(1).ShouldBeNull("scrolling to the end recycles the top containers");
            ListKeyboardTestHost.ScrollTo(listBox, 0);
            AssertCurrent(listBox, 5);
            host.Press(Key.Down);
            AssertCurrent(listBox, 9);
        });
    }

    [Fact]
    public void Multiple_Mode_Arrows_Browse_Space_Toggles_And_Shift_Selects_A_Range()
    {
        RunList(null, (host, listBox) =>
        {
            listBox.SelectionMode = SelectionMode.Multiple;
            host.Click(listBox.ContainerFromIndex(2).ShouldNotBeNull());
            SelectedIndexes(listBox).ShouldBe([2]);

            host.Press(Key.Down, RawInputModifiers.Shift);
            host.Press(Key.Down, RawInputModifiers.Shift);
            ListKeyboardTestHost.FocusedIndex(listBox).ShouldBe(4);
            SelectedIndexes(listBox).ShouldBe([2, 3, 4], "Shift selects the range from the anchor");

            host.Press(Key.Down, 2);
            ListKeyboardTestHost.FocusedIndex(listBox).ShouldBe(6);
            SelectedIndexes(listBox).ShouldBe([2, 3, 4], "browsing does not change a multiple selection");

            // Avalonia's ListBoxItem would replace the whole selection on Space / Enter.
            host.Press(Key.Space).ShouldBeTrue();
            SelectedIndexes(listBox).ShouldBe([2, 3, 4, 6]);
            host.Press(Key.Enter).ShouldBeTrue();
            SelectedIndexes(listBox).ShouldBe([2, 3, 4]);

            host.Press(Key.A, ListKeyboardTestHost.CommandModifier());
            listBox.SelectedItems!.Count.ShouldBe(Count);

            listBox.IsSelectable = false;
            host.Press(Key.Down);
            ListKeyboardTestHost.FocusedIndex(listBox).ShouldBe(7, "a non-selectable list still moves focus");
            host.Press(Key.Space);
            host.Press(Key.A, ListKeyboardTestHost.CommandModifier());
            listBox.SelectedIndex.ShouldBe(-1);
        });
    }

    private static int[] SelectedIndexes(AtomListBox listBox)
        => listBox.Selection.SelectedIndexes.OrderBy(i => i).ToArray();

    private static void AssertCurrent(AtomListBox listBox, int expected)
    {
        listBox.SelectedIndex.ShouldBe(expected, "selected index");
        var container = listBox.ContainerFromIndex(expected).ShouldNotBeNull(
            $"the container of the selected index {expected} must be realized");
        ((AtomListBoxItem)container).IsSelected.ShouldBeTrue();
        ListKeyboardTestHost.FocusedIndex(listBox).ShouldBe(expected, "keyboard focus follows the selection");
        listBox.GetRealizedContainers()
               .Where(c => !ReferenceEquals(c, container) && c is AtomListBoxItem { IsSelected: true })
               .ShouldBeEmpty("no other realized container may look selected");
    }

    private static void RunList(ISet<int>? disabled, Action<Host, AtomListBox> body)
    {
        var items = Enumerable.Range(0, Count)
                              .Select(i => new ListItemData
                              {
                                  Content   = $"Item {i}",
                                  IsEnabled = disabled?.Contains(i) != true
                              })
                              .ToArray();
        var listBox = new AtomListBox { Width = 240, Height = 160, ItemsSource = items };
        ListKeyboardTestHost.Run(listBox, host =>
        {
            ListKeyboardTestHost.ShouldBeVirtualizing(listBox);
            body(host, listBox);
        });
    }
}
