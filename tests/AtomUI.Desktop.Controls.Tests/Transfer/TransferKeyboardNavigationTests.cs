using AtomUI.Controls.Data;
using Avalonia.Input;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using Host = AtomUI.Desktop.Controls.Tests.ListKeyboardTestHost.Host;

namespace AtomUI.Desktop.Controls.Tests.Transfer;

// ListTransfer list panels (TransferListView, default VirtualizingStackPanel, Multiple|Toggle selection = checked
// items): the item is the keyboard focus target, arrows move focus without checking, Space / Enter toggles.
public class TransferKeyboardNavigationTests
{
    private const int Count = 200;

    static TransferKeyboardNavigationTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Item_Takes_Focus_Arrows_Browse_The_Virtualized_Panel_And_Space_Enter_Toggle()
    {
        RunTransfer((host, transfer, view) =>
        {
            CheckBoxOf(view, 0).Focusable.ShouldBeFalse("the item, not its check box, is the focus target");
            var first = view.ContainerFromIndex(0).ShouldBeOfType<TransferListItem>();
            host.Click(first);
            host.Focused.ShouldBeSameAs(first);
            SelectedIndexes(view).ShouldBe([0], "clicking an item checks it");

            // Before the fix the first Down moved focus out to the panel header check box.
            host.Press(Key.Down).ShouldBeTrue();
            ListKeyboardTestHost.FocusedIndex(view).ShouldBe(1);
            host.Press(Key.Down, 29);
            ListKeyboardTestHost.FocusedIndex(view).ShouldBe(30);
            host.Press(Key.End);
            ListKeyboardTestHost.FocusedIndex(view).ShouldBe(Count - 1);
            host.Press(Key.Home);
            ListKeyboardTestHost.FocusedIndex(view).ShouldBe(0);
            SelectedIndexes(view).ShouldBe([0], "moving focus does not check items");

            host.Press(Key.Down, 3);
            host.Press(Key.Space).ShouldBeTrue();
            SelectedIndexes(view).ShouldBe([0, 3]);
            CheckBoxOf(view, 3).IsChecked.ShouldBe(true);
            host.Press(Key.Enter).ShouldBeTrue();
            SelectedIndexes(view).ShouldBe([0]);
            CheckBoxOf(view, 3).IsChecked.ShouldBe(false);
        });
    }

    private static int[] SelectedIndexes(TransferListView view)
        => view.Selection.SelectedIndexes.OrderBy(i => i).ToArray();

    private static Desktop.Controls.CheckBox CheckBoxOf(TransferListView view, int index)
        => view.ContainerFromIndex(index).ShouldNotBeNull()
               .GetVisualDescendants().OfType<Desktop.Controls.CheckBox>().First(c => c.Name == "SelectedIndicator");

    private static void RunTransfer(Action<Host, ListTransfer, TransferListView> body)
    {
        var items = Enumerable.Range(0, Count)
                              .Select(i => new ListItemData { ItemKey = $"k{i}", Content = $"Item {i:000}" })
                              .ToArray();
        var transfer = new ListTransfer
        {
            Width       = 520,
            Height      = 260,
            ItemsSource = items
        };
        ListKeyboardTestHost.Run(transfer, host =>
        {
            var view = transfer.SourceView.ShouldBeOfType<TransferListView>();
            view.UpdateLayout();
            ListKeyboardTestHost.Pump();
            ListKeyboardTestHost.ShouldBeVirtualizing(view);
            body(host, transfer, view);
        }, width: 600, height: 380);
    }
}
