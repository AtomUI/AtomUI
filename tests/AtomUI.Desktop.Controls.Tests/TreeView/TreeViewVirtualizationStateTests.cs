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

    private static List<ITreeItemNode> CreateRoots(bool bindable)
    {
        var roots = new List<ITreeItemNode>();
        for (var i = 0; i < 40; i++)
        {
            var children = new List<ITreeItemNode>();
            for (var j = 0; j < 3; j++)
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
