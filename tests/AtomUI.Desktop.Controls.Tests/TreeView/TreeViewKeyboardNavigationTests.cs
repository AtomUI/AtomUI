using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomTreeView = AtomUI.Desktop.Controls.TreeView;
using AtomTreeViewItem = AtomUI.Desktop.Controls.TreeViewItem;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.TreeView;

public class TreeViewKeyboardNavigationTests
{
    static TreeViewKeyboardNavigationTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void TreeView_Is_The_Only_Focus_Target()
    {
        var treeView = CreateTreeView();
        var tree     = AddSampleTree(treeView);

        WithWindow(treeView, _ =>
        {
            treeView.Focusable.ShouldBeTrue();
            tree.Root.Focusable.ShouldBeFalse();
            foreach (var header in treeView.GetVisualDescendants().OfType<TreeViewItemHeader>())
            {
                header.Focusable.ShouldBeFalse();
            }
            foreach (var toggle in treeView.GetVisualDescendants().OfType<Avalonia.Controls.Primitives.ToggleButton>())
            {
                toggle.Focusable.ShouldBeFalse();
            }
        });
    }

    [Fact]
    public void Tab_Enters_TreeView_Itself_And_Leaves_It()
    {
        var treeView = CreateTreeView();
        AddSampleTree(treeView);
        var before = new TextBox { Width = 100 };
        var after  = new Avalonia.Controls.Button { Content = "after" };
        var panel  = new StackPanel();
        panel.Children.Add(before);
        panel.Children.Add(treeView);
        panel.Children.Add(after);

        WithWindow(panel, window =>
        {
            before.Focus().ShouldBeTrue();
            Dispatcher.UIThread.RunJobs();

            Press(window, Key.Tab);
            GetFocused(window).ShouldBeSameAs(treeView);

            Press(window, Key.Tab);
            GetFocused(window).ShouldBeSameAs(after);
        });
    }

    [Fact]
    public void Focus_Initializes_Active_Item_Without_Changing_Selection()
    {
        var treeView = CreateTreeView();
        var tree     = AddSampleTree(treeView);

        WithWindow(treeView, _ =>
        {
            FocusTree(treeView);

            treeView.KeyboardActiveItem.ShouldBeSameAs(tree.Root);
            treeView.SelectedItem.ShouldBeNull();
        });
    }

    [Fact]
    public void Focus_Initializes_Active_Item_From_Selected_Item()
    {
        var treeView = CreateTreeView();
        var tree     = AddSampleTree(treeView);
        treeView.SelectedItem = tree.SecondChild;

        WithWindow(treeView, _ =>
        {
            FocusTree(treeView);

            treeView.KeyboardActiveItem.ShouldBeSameAs(tree.SecondChild);
        });
    }

    [Fact]
    public void Arrow_Keys_Move_Selection_In_Preorder_In_Single_Mode()
    {
        var treeView = CreateTreeView();
        var tree     = AddSampleTree(treeView);

        WithWindow(treeView, window =>
        {
            FocusTree(treeView);

            Press(window, Key.Down);
            treeView.SelectedItem.ShouldBeSameAs(tree.FirstChild);

            Press(window, Key.Down);
            treeView.SelectedItem.ShouldBeSameAs(tree.SecondChild);

            Press(window, Key.Down);
            treeView.SelectedItem.ShouldBeSameAs(tree.SecondRoot);

            Press(window, Key.Up);
            treeView.SelectedItem.ShouldBeSameAs(tree.SecondChild);
            treeView.KeyboardActiveItem.ShouldBeSameAs(tree.SecondChild);
        });
    }

    [Fact]
    public void Up_Enters_Last_Visible_Descendant_Of_Previous_Sibling()
    {
        var treeView = CreateTreeView();
        var tree     = AddSampleTree(treeView);
        treeView.SelectedItem = tree.SecondRoot;

        WithWindow(treeView, window =>
        {
            FocusTree(treeView);

            Press(window, Key.Up);

            treeView.SelectedItem.ShouldBeSameAs(tree.SecondChild);
        });
    }

    [Fact]
    public void Navigation_Skips_Disabled_Hidden_And_Collapsed_Nodes()
    {
        var treeView = CreateTreeView();
        var tree     = AddSampleTree(treeView);
        tree.FirstChild.IsEnabled = false;
        tree.SecondRoot.IsVisible = false;
        var thirdRoot = new AtomTreeViewItem { Header = "third root" };
        thirdRoot.Items.Add(new AtomTreeViewItem { Header = "hidden by collapse" });
        treeView.Items.Add(thirdRoot);

        WithWindow(treeView, window =>
        {
            FocusTree(treeView);

            Press(window, Key.Down);
            treeView.SelectedItem.ShouldBeSameAs(tree.SecondChild);

            Press(window, Key.Down);
            treeView.SelectedItem.ShouldBeSameAs(thirdRoot);

            var handled = Press(window, Key.Down);
            handled.ShouldBeFalse();
            treeView.SelectedItem.ShouldBeSameAs(thirdRoot);
        });
    }

    [Fact]
    public void Home_And_End_Move_To_First_And_Last_Navigable_Nodes()
    {
        var treeView = CreateTreeView();
        var tree     = AddSampleTree(treeView);

        WithWindow(treeView, window =>
        {
            FocusTree(treeView);

            Press(window, Key.End);
            treeView.SelectedItem.ShouldBeSameAs(tree.SecondRoot);

            Press(window, Key.Home);
            treeView.SelectedItem.ShouldBeSameAs(tree.Root);
        });
    }

    [Fact]
    public void Right_Expands_Then_Enters_First_Child_And_Left_Returns_Then_Collapses()
    {
        var treeView = CreateTreeView();
        var tree     = AddSampleTree(treeView);
        tree.Root.IsExpanded = false;

        WithWindow(treeView, window =>
        {
            FocusTree(treeView);

            Press(window, Key.Right).ShouldBeTrue();
            tree.Root.IsExpanded.ShouldBeTrue();
            treeView.KeyboardActiveItem.ShouldBeSameAs(tree.Root);

            Press(window, Key.Right).ShouldBeTrue();
            treeView.KeyboardActiveItem.ShouldBeSameAs(tree.FirstChild);

            Press(window, Key.Right).ShouldBeFalse();
            treeView.KeyboardActiveItem.ShouldBeSameAs(tree.FirstChild);

            Press(window, Key.Left).ShouldBeTrue();
            treeView.KeyboardActiveItem.ShouldBeSameAs(tree.Root);

            Press(window, Key.Left).ShouldBeTrue();
            tree.Root.IsExpanded.ShouldBeFalse();

            Press(window, Key.Left).ShouldBeFalse();
        });
    }

    [Fact]
    public void Left_And_Right_Swap_In_Right_To_Left_Flow()
    {
        var treeView = CreateTreeView();
        treeView.FlowDirection = FlowDirection.RightToLeft;
        var tree = AddSampleTree(treeView);
        tree.Root.IsExpanded = false;

        WithWindow(treeView, window =>
        {
            FocusTree(treeView);

            Press(window, Key.Left).ShouldBeTrue();
            tree.Root.IsExpanded.ShouldBeTrue();

            Press(window, Key.Right).ShouldBeTrue();
            tree.Root.IsExpanded.ShouldBeFalse();
        });
    }

    [Fact]
    public void Ctrl_Arrow_Moves_Active_Item_Only_And_Enter_Commits_Selection()
    {
        var treeView = CreateTreeView();
        var tree     = AddSampleTree(treeView);
        var clicks   = 0;
        treeView.ItemClicked += (_, _) => clicks++;

        WithWindow(treeView, window =>
        {
            FocusTree(treeView);

            Press(window, Key.Down, RawInputModifiers.Control);
            treeView.KeyboardActiveItem.ShouldBeSameAs(tree.FirstChild);
            treeView.SelectedItem.ShouldBeNull();

            Press(window, Key.Enter);
            treeView.SelectedItem.ShouldBeSameAs(tree.FirstChild);
            clicks.ShouldBe(0);
        });
    }

    [Fact]
    public void CheckBox_Mode_Arrows_Do_Not_Select_And_Space_Toggles_Checked()
    {
        var treeView = CreateTreeView();
        treeView.ToggleType = ItemToggleType.CheckBox;
        var tree = AddSampleTree(treeView);

        WithWindow(treeView, window =>
        {
            FocusTree(treeView);

            Press(window, Key.Down);
            treeView.KeyboardActiveItem.ShouldBeSameAs(tree.FirstChild);
            treeView.SelectedItem.ShouldBeNull();

            Press(window, Key.Space);
            tree.FirstChild.IsChecked.ShouldBe(true);
            treeView.CheckedItems.Contains(tree.FirstChild).ShouldBeTrue();

            Press(window, Key.Enter);
            tree.FirstChild.IsChecked.ShouldBe(false);
            treeView.SelectedItem.ShouldBeNull();
        });
    }

    [Fact]
    public void CheckBox_Mode_Cascades_From_Keyboard()
    {
        var treeView = CreateTreeView();
        treeView.ToggleType = ItemToggleType.CheckBox;
        var tree = AddSampleTree(treeView);

        WithWindow(treeView, window =>
        {
            FocusTree(treeView);

            Press(window, Key.Space);

            tree.Root.IsChecked.ShouldBe(true);
            tree.FirstChild.IsChecked.ShouldBe(true);
            tree.SecondChild.IsChecked.ShouldBe(true);
        });
    }

    [Fact]
    public void Not_Selectable_TreeView_Never_Writes_Selection()
    {
        var treeView = CreateTreeView();
        treeView.IsSelectable = false;
        var tree = AddSampleTree(treeView);

        WithWindow(treeView, window =>
        {
            FocusTree(treeView);

            Should.NotThrow(() =>
            {
                Press(window, Key.Down);
                Press(window, Key.Enter).ShouldBeFalse();
                Press(window, Key.Space).ShouldBeFalse();
            });
            treeView.KeyboardActiveItem.ShouldBeSameAs(tree.FirstChild);
            treeView.SelectedItem.ShouldBeNull();
            tree.FirstChild.IsSelected.ShouldBeFalse();
        });
    }

    [Fact]
    public void Multiple_Mode_Arrows_Move_Only_And_Space_Toggles_Selection()
    {
        var treeView = CreateTreeView();
        treeView.SelectionMode = SelectionMode.Multiple;
        var tree = AddSampleTree(treeView);

        WithWindow(treeView, window =>
        {
            FocusTree(treeView);

            Press(window, Key.Space);
            Press(window, Key.Down);
            Press(window, Key.Down);
            Press(window, Key.Space);

            treeView.SelectedItems.Count.ShouldBe(2);
            treeView.SelectedItems.Contains(tree.Root).ShouldBeTrue();
            treeView.SelectedItems.Contains(tree.SecondChild).ShouldBeTrue();

            Press(window, Key.Space);
            treeView.SelectedItems.Contains(tree.SecondChild).ShouldBeFalse();
        });
    }

    [Fact]
    public void Multiple_Mode_Shift_Arrow_Selects_Range()
    {
        var treeView = CreateTreeView();
        treeView.SelectionMode = SelectionMode.Multiple;
        var tree = AddSampleTree(treeView);

        WithWindow(treeView, window =>
        {
            FocusTree(treeView);

            Press(window, Key.Space);
            Press(window, Key.Down, RawInputModifiers.Shift);
            Press(window, Key.Down, RawInputModifiers.Shift);

            treeView.SelectedItems.Count.ShouldBe(3);
            treeView.SelectedItems.Contains(tree.Root).ShouldBeTrue();
            treeView.SelectedItems.Contains(tree.FirstChild).ShouldBeTrue();
            treeView.SelectedItems.Contains(tree.SecondChild).ShouldBeTrue();
        });
    }

    [Fact]
    public void Collapsing_Ancestor_Moves_Active_Item_To_Collapsed_Node()
    {
        var treeView = CreateTreeView();
        var tree     = AddSampleTree(treeView);

        WithWindow(treeView, window =>
        {
            FocusTree(treeView);
            Press(window, Key.Down, RawInputModifiers.Control);
            Press(window, Key.Down, RawInputModifiers.Control);
            treeView.KeyboardActiveItem.ShouldBeSameAs(tree.SecondChild);

            tree.Root.IsExpanded = false;
            Dispatcher.UIThread.RunJobs();

            treeView.KeyboardActiveItem.ShouldBeSameAs(tree.Root);
        });
    }

    [Fact]
    public void Keyboard_Active_Pseudo_Class_Follows_Keyboard_Session()
    {
        var treeView = CreateTreeView();
        var tree     = AddSampleTree(treeView);
        var other    = new TextBox();
        var panel    = new StackPanel();
        panel.Children.Add(treeView);
        panel.Children.Add(other);

        WithWindow(panel, window =>
        {
            FocusTree(treeView);
            Press(window, Key.Down);

            tree.FirstChild.Classes.Contains(TreeViewPseudoClass.KeyboardActive).ShouldBeTrue();
            GetHeader(tree.FirstChild).Classes.Contains(TreeViewPseudoClass.KeyboardActive).ShouldBeTrue();
            tree.Root.Classes.Contains(TreeViewPseudoClass.KeyboardActive).ShouldBeFalse();

            other.Focus().ShouldBeTrue();
            Dispatcher.UIThread.RunJobs();

            tree.FirstChild.Classes.Contains(TreeViewPseudoClass.KeyboardActive).ShouldBeFalse();
            treeView.KeyboardActiveItem.ShouldBeSameAs(tree.FirstChild);
        });
    }

    [Fact]
    public void Keys_Raised_By_Header_Content_Are_Not_Intercepted()
    {
        var treeView = CreateTreeView();
        var tree     = AddSampleTree(treeView);

        WithWindow(treeView, _ =>
        {
            FocusTree(treeView);
            var args = new KeyEventArgs
            {
                RoutedEvent = InputElement.KeyDownEvent,
                Key         = Key.Down
            };

            // Bubbles to TreeView with a header descendant as the source, like a focused editor in a header.
            GetHeader(tree.Root).RaiseEvent(args);

            args.Handled.ShouldBeFalse();
            treeView.SelectedItem.ShouldBeNull();
            treeView.KeyboardActiveItem.ShouldBeSameAs(tree.Root);
        });
    }

    [Fact]
    public void Host_Forwarded_Keys_Navigate_While_Focus_Stays_In_Host()
    {
        var treeView = CreateTreeView();
        var tree     = AddSampleTree(treeView);
        var host     = new TextBox();
        var panel    = new StackPanel();
        panel.Children.Add(host);
        panel.Children.Add(treeView);

        WithWindow(panel, window =>
        {
            host.Focus().ShouldBeTrue();
            Dispatcher.UIThread.RunJobs();

            var args = new KeyEventArgs
            {
                RoutedEvent = InputElement.KeyDownEvent,
                Key         = Key.Down,
                Source      = host
            };
            treeView.HandleKeyDown(args);
            args.Handled.ShouldBeTrue();
            treeView.KeyboardActiveItem.ShouldBeSameAs(tree.Root);

            treeView.HandleKeyDown(new KeyEventArgs
            {
                RoutedEvent = InputElement.KeyDownEvent,
                Key         = Key.Down,
                Source      = host
            });

            treeView.SelectedItem.ShouldBeSameAs(tree.FirstChild);
            tree.FirstChild.Classes.Contains(TreeViewPseudoClass.KeyboardActive).ShouldBeTrue();
            GetFocused(window).ShouldBeSameAs(host);

            treeView.ClearKeyboardActiveItem();
            treeView.KeyboardActiveItem.ShouldBeNull();
            tree.FirstChild.Classes.Contains(TreeViewPseudoClass.KeyboardActive).ShouldBeFalse();
        });
    }

    [Fact]
    public void Right_On_Unloaded_Async_Node_Requests_Load_And_Expands()
    {
        var parent = new TreeItemNode { Header = "parent", ItemKey = "parent" };
        var loader = new StubLoader();
        var treeView = CreateTreeView();
        treeView.ItemsSource = new List<ITreeItemNode> { parent };
        treeView.DataLoader  = loader;

        WithWindow(treeView, window =>
        {
            FocusTree(treeView);

            Press(window, Key.Right).ShouldBeTrue();
            loader.Requests.ShouldBe(1);
            Press(window, Key.Right);
            loader.Requests.ShouldBe(1);

            loader.Complete(new TreeItemNode { Header = "child", ItemKey = "child" });
            RunJobsUntil(() => treeView.ContainerFromIndex(0) is AtomTreeViewItem { IsExpanded: true });

            var parentContainer = (AtomTreeViewItem)treeView.ContainerFromIndex(0)!;
            parentContainer.IsExpanded.ShouldBeTrue();

            Press(window, Key.Right).ShouldBeTrue();
            treeView.KeyboardActiveItem.ShouldNotBeSameAs(parentContainer);
            treeView.KeyboardActiveItem!.Header.ShouldBeOfType<TreeItemNode>().Header.ShouldBe("child");
        });
    }

    [Fact]
    public void Detach_Clears_Active_Item()
    {
        var treeView = CreateTreeView();
        AddSampleTree(treeView);

        var window = new AvaloniaWindow { Width = 420, Height = 320, Content = treeView };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            FocusTree(treeView);
            treeView.KeyboardActiveItem.ShouldNotBeNull();

            window.Content = null;
            Dispatcher.UIThread.RunJobs();

            treeView.KeyboardActiveItem.ShouldBeNull();
        }
        finally
        {
            window.Close();
        }
    }

    private static AtomTreeView CreateTreeView()
    {
        return new AtomTreeView
        {
            IsMotionEnabled      = false,
            IsShowEmptyIndicator = false
        };
    }

    private sealed record SampleTree(
        AtomTreeViewItem Root,
        AtomTreeViewItem FirstChild,
        AtomTreeViewItem SecondChild,
        AtomTreeViewItem SecondRoot);

    // root (expanded)
    //   first child
    //   second child
    // second root
    private static SampleTree AddSampleTree(AtomTreeView treeView)
    {
        var firstChild  = new AtomTreeViewItem { Header = "first child", ItemKey = "first" };
        var secondChild = new AtomTreeViewItem { Header = "second child", ItemKey = "second" };
        var root        = new AtomTreeViewItem { Header = "root", ItemKey = "root", IsExpanded = true };
        root.Items.Add(firstChild);
        root.Items.Add(secondChild);
        var secondRoot = new AtomTreeViewItem { Header = "second root", ItemKey = "second-root" };
        treeView.Items.Add(root);
        treeView.Items.Add(secondRoot);
        return new SampleTree(root, firstChild, secondChild, secondRoot);
    }

    private static void WithWindow(Control content, Action<AvaloniaWindow> assertion)
    {
        var window = new AvaloniaWindow
        {
            Width   = 420,
            Height  = 320,
            Content = content
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

    private static void FocusTree(AtomTreeView treeView)
    {
        treeView.Focus(NavigationMethod.Tab).ShouldBeTrue();
        Dispatcher.UIThread.RunJobs();
    }

    private static bool Press(AvaloniaWindow window, Key key, RawInputModifiers modifiers = RawInputModifiers.None)
    {
        var handled = false;
        void Observe(object? sender, KeyEventArgs e) => handled = e.Handled;
        window.AddHandler(InputElement.KeyDownEvent, Observe, RoutingStrategies.Bubble, handledEventsToo: true);
        try
        {
            window.KeyPress(key, modifiers, PhysicalKey.None, null);
            Dispatcher.UIThread.RunJobs();
        }
        finally
        {
            window.RemoveHandler(InputElement.KeyDownEvent, Observe);
        }
        return handled;
    }

    private static object? GetFocused(AvaloniaWindow window)
    {
        return TopLevel.GetTopLevel(window)?.FocusManager?.GetFocusedElement();
    }

    private static TreeViewItemHeader GetHeader(AtomTreeViewItem item)
    {
        return item.GetVisualDescendants().OfType<TreeViewItemHeader>().First();
    }

    private static void RunJobsUntil(Func<bool> condition, int maxPasses = 200)
    {
        // Async load completion hops through the thread pool before returning to the UI thread.
        for (var i = 0; i < maxPasses && !condition(); i++)
        {
            Dispatcher.UIThread.RunJobs();
            Thread.Sleep(5);
        }
    }

    private sealed class StubLoader : ITreeItemNodeLoader
    {
        private TaskCompletionSource<TreeItemLoadResult>? _pending;

        public int Requests { get; private set; }

        public Task<TreeItemLoadResult> LoadAsync(ITreeItemNode targetTreeItem, CancellationToken token)
        {
            Requests++;
            _pending = new TaskCompletionSource<TreeItemLoadResult>();
            return _pending.Task;
        }

        public void Complete(params ITreeItemNode[] children)
        {
            _pending!.SetResult(new TreeItemLoadResult { Data = children });
        }
    }
}
