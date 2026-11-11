using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Threading;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.TreeSelectControl;

public class TreeSelectKeyboardNavigationTests
{
    static TreeSelectKeyboardNavigationTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Single_Select_Arrows_Browse_And_Enter_Commits_And_Closes()
    {
        var (treeSelect, _, second) = CreateTreeSelect(checkable: false);

        ShowInWindow(treeSelect, window =>
        {
            FocusTreeSelect(treeSelect);

            Press(window, Key.Down);
            treeSelect.IsDropDownOpen.ShouldBeTrue();
            var treeView = GetPopupTreeView(treeSelect);
            treeView.Focusable.ShouldBeFalse();

            Press(window, Key.Down);
            Press(window, Key.Down);
            treeView.KeyboardActiveItem.ShouldNotBeNull();
            treeView.KeyboardActiveItem!.Header.ShouldBeSameAs(second);
            treeSelect.SelectedItem.ShouldBeNull();

            Press(window, Key.Enter);

            treeSelect.SelectedItem.ShouldBeSameAs(second);
            treeSelect.IsDropDownOpen.ShouldBeFalse();
            GetFocused(window).ShouldBeSameAs(treeSelect);
            treeView.KeyboardActiveItem.ShouldBeNull();
        });
    }

    [Fact]
    public void Escape_Closes_Drop_Down()
    {
        var (treeSelect, _, _) = CreateTreeSelect(checkable: false);

        ShowInWindow(treeSelect, window =>
        {
            FocusTreeSelect(treeSelect);
            Press(window, Key.Down);
            treeSelect.IsDropDownOpen.ShouldBeTrue();

            Press(window, Key.Escape);

            treeSelect.IsDropDownOpen.ShouldBeFalse();
        });
    }

    [Fact]
    public void Checkable_Space_Toggles_Check_And_Keeps_Drop_Down_Open()
    {
        var (treeSelect, first, _) = CreateTreeSelect(checkable: true);

        ShowInWindow(treeSelect, window =>
        {
            FocusTreeSelect(treeSelect);
            Press(window, Key.Down);
            Press(window, Key.Down);

            Press(window, Key.Space);

            treeSelect.IsDropDownOpen.ShouldBeTrue();
            treeSelect.SelectedItems.ShouldNotBeNull();
            treeSelect.SelectedItems!.Cast<ITreeItemNode>().ShouldContain(first);
        });
    }

    private static (Desktop.Controls.TreeSelect TreeSelect, TreeItemNode First, TreeItemNode Second) CreateTreeSelect(
        bool checkable)
    {
        var first  = new TreeItemNode { Header = "First", Value = "first", ItemKey = "first" };
        var second = new TreeItemNode { Header = "Second", Value = "second", ItemKey = "second" };
        var treeSelect = new Desktop.Controls.TreeSelect
        {
            Width           = 240,
            IsMotionEnabled = false,
            IsTreeCheckable = checkable,
            ItemsSource     = [first, second],
            PlaceholderText = "Please select"
        };
        return (treeSelect, first, second);
    }

    private static void ShowInWindow(Control content, Action<AvaloniaWindow> assertion)
    {
        var overlayPanel = new ScopeAwareOverlayLayerPanel
        {
            Width  = 420,
            Height = 320
        };
        overlayPanel.Children.Add(content);

        var visualLayerManager = new VisualLayerManager
        {
            Child = overlayPanel
        };
        var property = typeof(VisualLayerManager).GetProperty(
            "EnablePopupOverlayLayer",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        property.ShouldNotBeNull();
        property.SetValue(visualLayerManager, true);

        var window = new AvaloniaWindow
        {
            Width   = 420,
            Height  = 320,
            Content = visualLayerManager
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            assertion(window);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static void FocusTreeSelect(Desktop.Controls.TreeSelect treeSelect)
    {
        treeSelect.Focus(NavigationMethod.Tab).ShouldBeTrue();
        Dispatcher.UIThread.RunJobs();
    }

    private static void Press(AvaloniaWindow window, Key key, RawInputModifiers modifiers = RawInputModifiers.None)
    {
        window.KeyPress(key, modifiers, PhysicalKey.None, null);
        Dispatcher.UIThread.RunJobs();
    }

    private static object? GetFocused(AvaloniaWindow window)
    {
        return TopLevel.GetTopLevel(window)?.FocusManager?.GetFocusedElement();
    }

    private static Desktop.Controls.TreeView GetPopupTreeView(Desktop.Controls.TreeSelect treeSelect)
    {
        var field = typeof(Desktop.Controls.TreeSelect).GetField(
            "_treeView",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        field.ShouldNotBeNull();
        var treeView = field.GetValue(treeSelect) as Desktop.Controls.TreeView;
        treeView.ShouldNotBeNull();
        return treeView!;
    }
}
