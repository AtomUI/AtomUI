using AtomUI.Controls;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Styling;
using Avalonia.Threading;
using Shouldly;
using Xunit;
using AtomTreeView = AtomUI.Desktop.Controls.TreeView;
using AtomTreeViewItem = AtomUI.Desktop.Controls.TreeViewItem;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.TreeView;

// Virtualizing panels recycle containers and prepare new ones from the node data, so container state
// changed by the user must reach the node data, for plain TreeItemNode as for BindableTreeItemNode.
public class TreeViewVirtualizationStateTests
{
    static TreeViewVirtualizationStateTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Expanded_State_Survives_Container_Recycling(bool bindable)
    {
        var roots    = CreateRoots(bindable);
        var treeView = CreateVirtualizedTreeView(roots);

        WithWindow(treeView, () =>
        {
            Container(treeView, 0).SetCurrentValue(Avalonia.Controls.TreeViewItem.IsExpandedProperty, true);
            Dispatcher.UIThread.RunJobs();

            RecycleFirstRoot(treeView);

            Container(treeView, 0).IsExpanded.ShouldBeTrue();
            roots[0].IsExpanded.ShouldBeTrue();
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Selected_State_Survives_Container_Recycling(bool bindable)
    {
        var roots    = CreateRoots(bindable);
        var treeView = CreateVirtualizedTreeView(roots);

        WithWindow(treeView, () =>
        {
            treeView.SelectedItem = roots[0];
            Dispatcher.UIThread.RunJobs();

            RecycleFirstRoot(treeView);

            treeView.SelectedItem.ShouldBeSameAs(roots[0]);
            Container(treeView, 0).IsSelected.ShouldBeTrue();
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Checked_Cascade_Survives_Container_Recycling(bool bindable)
    {
        var roots    = CreateRoots(bindable);
        var treeView = CreateVirtualizedTreeView(roots);
        treeView.ToggleType = ItemToggleType.CheckBox;

        WithWindow(treeView, () =>
        {
            var root = Container(treeView, 0);
            root.SetCurrentValue(Avalonia.Controls.TreeViewItem.IsExpandedProperty, true);
            Dispatcher.UIThread.RunJobs();
            root.SetCurrentValue(AtomTreeViewItem.IsCheckedProperty, true);
            Dispatcher.UIThread.RunJobs();

            treeView.CheckedItems.Contains(roots[0]).ShouldBeTrue();
            treeView.CheckedItems.Contains(roots[0].Children.First()).ShouldBeTrue();

            RecycleFirstRoot(treeView);

            // The re-prepared containers take the checked state from the node data; re-applying a stale
            // unchecked value would start an uncheck cascade on a container that is not indexed yet.
            root = Container(treeView, 0);
            root.IsChecked.ShouldBe(true);
            ((AtomTreeViewItem)root.ContainerFromIndex(0)!).IsChecked.ShouldBe(true);
            roots[0].IsChecked.ShouldBe(true);
            roots[0].Children.First().IsChecked.ShouldBe(true);
        });
    }

    // The cascade follows the node data, so children the virtualizing panel has not realized are checked too.
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Checked_Cascade_Reaches_Unrealized_Children(bool bindable)
    {
        var roots    = CreateRoots(bindable, rootCount: 3, childCount: 15);
        var treeView = CreateVirtualizedTreeView(roots);
        treeView.ToggleType = ItemToggleType.CheckBox;

        WithWindow(treeView, () =>
        {
            var root = ExpandFirstRoot(treeView);
            CountRealizedChildren(root).ShouldBeLessThan(15);

            root.SetCurrentValue(AtomTreeViewItem.IsCheckedProperty, true);
            Dispatcher.UIThread.RunJobs();

            treeView.CheckedItems.Count.ShouldBe(16);
            root.IsChecked.ShouldBe(true);
            roots[0].IsChecked.ShouldBe(true);
            roots[0].Children.ShouldAllBe(child => child.IsChecked == true);

            var lastChild = roots[0].Children.Last();
            ScrollToNode(treeView, lastChild);
            ((AtomTreeViewItem)treeView.TreeContainerFromItem(lastChild)!).IsChecked.ShouldBe(true);

            ScrollToNode(treeView, roots[0]);
            root = Container(treeView, 0);
            root.SetCurrentValue(AtomTreeViewItem.IsCheckedProperty, false);
            Dispatcher.UIThread.RunJobs();

            treeView.CheckedItems.Count.ShouldBe(0);
            roots[0].Children.ShouldAllBe(child => child.IsChecked == false);
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Unchecking_Realized_Child_Makes_Parent_Indeterminate(bool bindable)
    {
        var roots    = CreateRoots(bindable, rootCount: 3, childCount: 15);
        var treeView = CreateVirtualizedTreeView(roots);
        treeView.ToggleType = ItemToggleType.CheckBox;

        WithWindow(treeView, () =>
        {
            var root = ExpandFirstRoot(treeView);
            root.SetCurrentValue(AtomTreeViewItem.IsCheckedProperty, true);
            Dispatcher.UIThread.RunJobs();

            ((AtomTreeViewItem)root.ContainerFromIndex(0)!).SetCurrentValue(AtomTreeViewItem.IsCheckedProperty, false);
            Dispatcher.UIThread.RunJobs();

            treeView.CheckedItems.Count.ShouldBe(14);
            treeView.CheckedItems.Contains(roots[0]).ShouldBeFalse();
            root.IsChecked.ShouldBeNull();
            roots[0].IsChecked.ShouldBeNull();

            // Checking the child again completes the parent, although most siblings are not realized.
            ((AtomTreeViewItem)root.ContainerFromIndex(0)!).SetCurrentValue(AtomTreeViewItem.IsCheckedProperty, true);
            Dispatcher.UIThread.RunJobs();

            treeView.CheckedItems.Count.ShouldBe(16);
            root.IsChecked.ShouldBe(true);
        });
    }

    // Recycling a container clears its ItemsSource; that is not a data removal and must keep the tree's checked set.
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CheckedItems_Survive_Container_Recycling(bool bindable)
    {
        var roots    = CreateRoots(bindable);
        var treeView = CreateVirtualizedTreeView(roots);
        treeView.ToggleType = ItemToggleType.CheckBox;

        WithWindow(treeView, () =>
        {
            var root = ExpandFirstRoot(treeView);
            root.SetCurrentValue(AtomTreeViewItem.IsCheckedProperty, true);
            Dispatcher.UIThread.RunJobs();
            var expected = new HashSet<object>(treeView.CheckedItems.Cast<object>());
            expected.Count.ShouldBe(4);

            treeView.ScrollIntoView(treeView.ItemCount - 1);
            treeView.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            treeView.ContainerFromIndex(0).ShouldBeNull();
            new HashSet<object>(treeView.CheckedItems.Cast<object>()).SetEquals(expected).ShouldBeTrue();

            treeView.ScrollIntoView(0);
            treeView.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            new HashSet<object>(treeView.CheckedItems.Cast<object>()).SetEquals(expected).ShouldBeTrue();
            Container(treeView, 0).IsChecked.ShouldBe(true);
        });
    }

    // Items added to CheckedItems from code reach unrealized nodes and their containers once realized.
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CheckedItems_Add_Applies_To_Unrealized_Node(bool bindable)
    {
        var roots    = CreateRoots(bindable);
        var treeView = CreateVirtualizedTreeView(roots);
        treeView.ToggleType = ItemToggleType.CheckBox;

        WithWindow(treeView, () =>
        {
            var lastRoot = roots[^1];
            treeView.TreeContainerFromItem(lastRoot).ShouldBeNull();

            treeView.CheckedItems.Add(lastRoot);
            Dispatcher.UIThread.RunJobs();

            treeView.CheckedItems.Count.ShouldBe(4);
            lastRoot.IsChecked.ShouldBe(true);
            lastRoot.Children.ShouldAllBe(child => child.IsChecked == true);

            ScrollToNode(treeView, lastRoot);
            ((AtomTreeViewItem)treeView.TreeContainerFromItem(lastRoot)!).IsChecked.ShouldBe(true);

            treeView.CheckedItems.Remove(lastRoot);
            Dispatcher.UIThread.RunJobs();

            treeView.CheckedItems.Count.ShouldBe(0);
            lastRoot.Children.ShouldAllBe(child => child.IsChecked == false);
            ((AtomTreeViewItem)treeView.TreeContainerFromItem(lastRoot)!).IsChecked.ShouldBe(false);
        });
    }

    // Clearing CheckedItems unchecks nodes whose containers are recycled, so they do not come back checked.
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CheckedItems_Clear_Applies_To_Unrealized_Nodes(bool bindable)
    {
        var roots    = CreateRoots(bindable);
        var treeView = CreateVirtualizedTreeView(roots);
        treeView.ToggleType = ItemToggleType.CheckBox;

        WithWindow(treeView, () =>
        {
            var root = ExpandFirstRoot(treeView);
            root.SetCurrentValue(AtomTreeViewItem.IsCheckedProperty, true);
            Dispatcher.UIThread.RunJobs();

            treeView.ScrollIntoView(treeView.ItemCount - 1);
            treeView.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            treeView.ContainerFromIndex(0).ShouldBeNull();

            treeView.CheckedItems.Clear();
            Dispatcher.UIThread.RunJobs();

            treeView.ScrollIntoView(0);
            treeView.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            treeView.CheckedItems.Count.ShouldBe(0);
            roots[0].IsChecked.ShouldBe(false);
            Container(treeView, 0).IsChecked.ShouldBe(false);
        });
    }

    // Data reset of a node's children is a real removal: the removed children leave CheckedItems, the rest stays.
    [Fact]
    public void Children_Reset_Removes_Only_Detached_Checked_Items()
    {
        var roots    = CreateRoots(bindable: false);
        var treeView = CreateVirtualizedTreeView(roots);
        treeView.ToggleType = ItemToggleType.CheckBox;

        WithWindow(treeView, () =>
        {
            foreach (var index in new[] { 1, 0 })
            {
                var root = Container(treeView, index);
                root.SetCurrentValue(Avalonia.Controls.TreeViewItem.IsExpandedProperty, true);
                treeView.UpdateLayout();
                Dispatcher.UIThread.RunJobs();
                root.SetCurrentValue(AtomTreeViewItem.IsCheckedProperty, true);
                Dispatcher.UIThread.RunJobs();
            }
            treeView.CheckedItems.Count.ShouldBe(8);

            ((TreeItemNode)roots[1]).Children.Clear();
            Dispatcher.UIThread.RunJobs();

            treeView.CheckedItems.Contains(roots[0]).ShouldBeTrue();
            roots[0].Children.ShouldAllBe(child => treeView.CheckedItems.Contains(child));
            treeView.CheckedItems.Count.ShouldBe(5);
        });
    }

    // IsCheckStrictly keeps nodes independent: no cascade, but code can still check a node that is not realized.
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Strict_Checked_Items_Apply_To_Unrealized_Node_Without_Cascade(bool bindable)
    {
        var roots    = CreateRoots(bindable);
        var treeView = CreateVirtualizedTreeView(roots);
        treeView.ToggleType      = ItemToggleType.CheckBox;
        treeView.IsCheckStrictly = true;

        WithWindow(treeView, () =>
        {
            var root = ExpandFirstRoot(treeView);
            root.SetCurrentValue(AtomTreeViewItem.IsCheckedProperty, true);
            Dispatcher.UIThread.RunJobs();
            treeView.CheckedItems.Count.ShouldBe(1);

            var lastRoot = roots[^1];
            treeView.CheckedItems.Add(lastRoot);
            Dispatcher.UIThread.RunJobs();

            treeView.CheckedItems.Count.ShouldBe(2);
            lastRoot.IsChecked.ShouldBe(true);
            lastRoot.Children.ShouldAllBe(child => child.IsChecked == false);

            RecycleFirstRoot(treeView);
            treeView.CheckedItems.Count.ShouldBe(2);
            Container(treeView, 0).IsChecked.ShouldBe(true);
        });
    }

    // Replacing a bindable node's Children drops the checked nodes that left with the old collection.
    [Fact]
    public void Replacing_Bindable_Children_Removes_Their_Checked_Items()
    {
        var roots    = CreateRoots(bindable: true);
        var treeView = CreateVirtualizedTreeView(roots);
        treeView.ToggleType = ItemToggleType.CheckBox;

        WithWindow(treeView, () =>
        {
            var root = ExpandFirstRoot(treeView);
            root.SetCurrentValue(AtomTreeViewItem.IsCheckedProperty, true);
            Dispatcher.UIThread.RunJobs();
            treeView.CheckedItems.Count.ShouldBe(4);

            var node     = (BindableTreeItemNode)roots[0];
            var keptNode = node.Children[0];
            node.Children = new Avalonia.Collections.AvaloniaList<ITreeItemNode> { keptNode };
            Dispatcher.UIThread.RunJobs();

            treeView.CheckedItems.Count.ShouldBe(2);
            treeView.CheckedItems.Contains(node).ShouldBeTrue();
            treeView.CheckedItems.Contains(keptNode).ShouldBeTrue();
        });
    }

    // A plain StackPanel realizes every child; the data cascade gives the same result there.
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Checked_Cascade_Without_Virtualization_Is_Unchanged(bool bindable)
    {
        var roots    = CreateRoots(bindable, rootCount: 3, childCount: 15);
        var treeView = new AtomTreeView
        {
            IsMotionEnabled      = false,
            IsShowEmptyIndicator = false,
            ToggleType           = ItemToggleType.CheckBox,
            ItemsSource          = roots
        };

        WithWindow(treeView, () =>
        {
            var root = ExpandFirstRoot(treeView);
            CountRealizedChildren(root).ShouldBe(15);

            root.SetCurrentValue(AtomTreeViewItem.IsCheckedProperty, true);
            Dispatcher.UIThread.RunJobs();
            treeView.CheckedItems.Count.ShouldBe(16);
            root.GetRealizedContainers().Cast<AtomTreeViewItem>().ShouldAllBe(child => child.IsChecked == true);

            ((AtomTreeViewItem)root.ContainerFromIndex(3)!).SetCurrentValue(AtomTreeViewItem.IsCheckedProperty, false);
            Dispatcher.UIThread.RunJobs();
            treeView.CheckedItems.Count.ShouldBe(14);
            root.IsChecked.ShouldBeNull();

            // A collapsed root's children are not realized; checking it still reaches them through the data.
            var collapsedRoot = Container(treeView, 1);
            collapsedRoot.SetCurrentValue(AtomTreeViewItem.IsCheckedProperty, true);
            Dispatcher.UIThread.RunJobs();
            collapsedRoot.IsExpanded.ShouldBeFalse();
            treeView.CheckedItems.Count.ShouldBe(30);
        });
    }

    private static List<ITreeItemNode> CreateRoots(bool bindable, int rootCount = 40, int childCount = 3)
    {
        var roots = new List<ITreeItemNode>();
        for (var i = 0; i < rootCount; i++)
        {
            var children = new List<ITreeItemNode>();
            for (var j = 0; j < childCount; j++)
            {
                children.Add(bindable
                    ? new BindableTreeItemNode { Header = $"Node {i}-{j}", IsLeaf = true }
                    : new TreeItemNode { Header         = $"Node {i}-{j}", IsLeaf = true });
            }
            roots.Add(bindable
                ? new BindableTreeItemNode { Header = $"Node {i}", Children = children }
                : new TreeItemNode { Header         = $"Node {i}", Children = children });
        }
        return roots;
    }

    private static AtomTreeView CreateVirtualizedTreeView(List<ITreeItemNode> roots)
    {
        var treeView = new AtomTreeView
        {
            IsMotionEnabled      = false,
            IsShowEmptyIndicator = false,
            Height               = 120,
            ItemsSource          = roots,
            ItemsPanel           = new FuncTemplate<Panel?>(() => new VirtualizingStackPanel())
        };
        treeView.Styles.Add(new Style(x => x.OfType<AtomTreeViewItem>())
        {
            Setters =
            {
                new Setter(ItemsControl.ItemsPanelProperty,
                    new FuncTemplate<Panel?>(() => new VirtualizingStackPanel()))
            }
        });
        return treeView;
    }

    // Scrolls far enough for the first root's container to be recycled, then back.
    private static void RecycleFirstRoot(AtomTreeView treeView)
    {
        treeView.ScrollIntoView(treeView.ItemCount - 1);
        treeView.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        treeView.ContainerFromIndex(0).ShouldBeNull();

        treeView.ScrollIntoView(0);
        treeView.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
    }

    private static AtomTreeViewItem ExpandFirstRoot(AtomTreeView treeView)
    {
        var root = Container(treeView, 0);
        root.SetCurrentValue(Avalonia.Controls.TreeViewItem.IsExpandedProperty, true);
        treeView.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        return root;
    }

    private static int CountRealizedChildren(AtomTreeViewItem treeViewItem)
    {
        return treeViewItem.GetRealizedContainers().Count();
    }

    // Brings a node of any depth into view through the keyboard navigation's container realization.
    private static void ScrollToNode(AtomTreeView treeView, ITreeItemNode node)
    {
        var path = new List<ITreeItemNode>();
        for (var current = node; current != null; current = current.ParentNode as ITreeItemNode)
        {
            path.Insert(0, current);
        }

        ItemsControl owner = treeView;
        foreach (var segment in path)
        {
            var index = owner.Items.IndexOf(segment);
            owner.ScrollIntoView(index);
            treeView.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            owner = (ItemsControl)owner.ContainerFromIndex(index)!;
        }
    }

    private static AtomTreeViewItem Container(AtomTreeView treeView, int index)
    {
        return treeView.ContainerFromIndex(index).ShouldBeOfType<AtomTreeViewItem>();
    }

    private static void WithWindow(Control content, Action assertion)
    {
        var window = new AvaloniaWindow { Width = 420, Height = 320, Content = content };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            assertion();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
