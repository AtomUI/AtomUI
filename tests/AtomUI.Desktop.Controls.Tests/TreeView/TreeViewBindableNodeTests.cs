using System.Reflection;
using System.Runtime.CompilerServices;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Threading;
using Shouldly;
using Xunit;
using AtomTreeViewItem = AtomUI.Desktop.Controls.TreeViewItem;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.TreeView;

public class TreeViewBindableNodeTests
{
    private const string HeaderResourceKey = "TreeViewBindableNodeTests.Header";

    static TreeViewBindableNodeTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void BindableTreeItemNode_Children_Update_ParentNode()
    {
        var parent = new BindableTreeItemNode { Header = "Parent" };
        var child  = new BindableTreeItemNode { Header = "Child" };

        parent.Children.Add(child);

        child.ParentNode.ShouldBeSameAs(parent);

        parent.Children.Remove(child);

        child.ParentNode.ShouldBeNull();
    }

    [Fact]
    public void BindableTreeItemNode_Children_Clear_Detaches_Previous_ParentNode()
    {
        var parent = new BindableTreeItemNode { Header = "Parent" };
        var child  = new BindableTreeItemNode { Header = "Child" };
        parent.Children.Add(child);

        parent.Children.Clear();

        child.ParentNode.ShouldBeNull();
    }

    [Fact]
    public void BindableTreeItemNode_Property_Changes_Update_Realized_Container()
    {
        var node = new BindableTreeItemNode
        {
            Header             = "Root",
            ItemKey            = "root",
            IsChecked          = false,
            IsEnabled          = true,
            IsExpanded         = false,
            IsIndicatorEnabled = true,
            IsLeaf             = false,
            Value              = "root-value"
        };
        var treeView = new AtomUI.Desktop.Controls.TreeView
        {
            ToggleType            = ItemToggleType.CheckBox,
            IsMotionEnabled       = false,
            IsShowEmptyIndicator  = false,
            ItemsSource           = new[] { node }
        };

        ShowInWindow(treeView, () =>
        {
            var container = (AtomTreeViewItem)treeView.ContainerFromItem(node)!;

            node.ItemKey            = "updated-root";
            node.IsChecked          = true;
            node.IsEnabled          = false;
            node.IsExpanded         = true;
            node.IsIndicatorEnabled = false;
            node.IsLeaf             = true;
            node.Value              = "updated-value";
            Dispatcher.UIThread.RunJobs();

            container.Header.ShouldBeSameAs(node);
            container.ItemKey.ShouldBe("updated-root");
            container.IsChecked.ShouldBe(true);
            container.IsEnabled.ShouldBeFalse();
            container.IsExpanded.ShouldBeTrue();
            container.IsIndicatorEnabled.ShouldBeFalse();
            container.IsLeaf.ShouldBeTrue();
            container.Value.ShouldBe("updated-value");
        });
    }

    [Fact]
    public void Container_State_Changes_Write_Back_To_BindableTreeItemNode()
    {
        var node = new BindableTreeItemNode
        {
            Header     = "Root",
            IsChecked  = false,
            IsExpanded = false,
            IsSelected = false
        };
        var treeView = new AtomUI.Desktop.Controls.TreeView
        {
            ToggleType           = ItemToggleType.CheckBox,
            IsMotionEnabled      = false,
            IsShowEmptyIndicator = false,
            ItemsSource          = new[] { node }
        };

        ShowInWindow(treeView, () =>
        {
            var container = (AtomTreeViewItem)treeView.ContainerFromItem(node)!;

            container.SetCurrentValue(AtomTreeViewItem.IsCheckedProperty, true);
            container.SetCurrentValue(AtomTreeViewItem.IsExpandedProperty, true);
            container.SetCurrentValue(AtomTreeViewItem.IsSelectedProperty, true);
            Dispatcher.UIThread.RunJobs();

            node.IsChecked.ShouldBe(true);
            node.IsExpanded.ShouldBeTrue();
            node.IsSelected.ShouldBeTrue();
        });
    }

    [Fact]
    public void BindableNode_VisualTree_Reattach_Continues_To_Sync_Node_State()
    {
        var child = new BindableTreeItemNode
        {
            Header = "Child"
        };
        var parent = new BindableTreeItemNode
        {
            Header   = "Parent",
            IsExpanded = true,
            Children = [child]
        };
        var treeView = new AtomUI.Desktop.Controls.TreeView
        {
            ToggleType           = ItemToggleType.CheckBox,
            IsMotionEnabled      = false,
            IsShowEmptyIndicator = false,
            ItemsSource          = new[] { parent }
        };
        var window = new AvaloniaWindow
        {
            Width   = 420,
            Height  = 320,
            Content = treeView
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var parentContainer = (AtomTreeViewItem)treeView.ContainerFromItem(parent)!;
            parentContainer.ContainerFromItem(child).ShouldBeAssignableTo<AtomTreeViewItem>();

            window.Content = null;
            Dispatcher.UIThread.RunJobs();
            window.Content = treeView;
            Dispatcher.UIThread.RunJobs();

            var reopenedParentContainer = (AtomTreeViewItem)treeView.ContainerFromItem(parent)!;
            var reopenedChildContainer = (AtomTreeViewItem)reopenedParentContainer.ContainerFromItem(child)!;
            reopenedParentContainer.ShouldBeSameAs(parentContainer);
            reopenedChildContainer.SetCurrentValue(AtomTreeViewItem.IsCheckedProperty, true);
            Dispatcher.UIThread.RunJobs();

            child.IsChecked.ShouldBe(true);
        }
        finally
        {
            window.Content = null;
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Clearing_Expanded_Roots_Does_Not_Throw()
    {
        var grandChild = new BindableTreeItemNode
        {
            Header = "GrandChild"
        };
        var child = new BindableTreeItemNode
        {
            Header     = "Child",
            IsExpanded = true,
            Children   = [grandChild]
        };
        var parent = new BindableTreeItemNode
        {
            Header     = "Parent",
            IsExpanded = true,
            Children   = [child]
        };
        var roots = new AvaloniaList<ITreeItemNode> { parent };
        var treeView = new AtomUI.Desktop.Controls.TreeView
        {
            IsMotionEnabled      = false,
            IsShowEmptyIndicator = false,
            ItemsSource          = roots,
            ItemTemplate = new TreeDataTemplate
            {
                DataType    = typeof(ITreeItemNode),
                ItemsSource = new Binding("Children")
            }
        };

        ShowInWindow(treeView, () =>
        {
            var parentContainer = (AtomTreeViewItem)treeView.ContainerFromItem(parent)!;
            var childContainer  = (AtomTreeViewItem)parentContainer.ContainerFromItem(child)!;
            childContainer.ContainerFromItem(grandChild).ShouldBeAssignableTo<AtomTreeViewItem>();

            roots.Clear();
            Dispatcher.UIThread.RunJobs();

            treeView.ContainerFromItem(parent).ShouldBeNull();
        });
    }

    [Fact]
    public void Clearing_Expanded_Roots_With_Plain_Parent_Node_Does_Not_Throw()
    {
        var grandChild = new BindableTreeItemNode
        {
            Header = "GrandChild"
        };
        var child = new PlainTreeItemNode
        {
            Header     = "Child",
            IsExpanded = true,
            Children   = new List<ITreeItemNode> { grandChild }
        };
        var parent = new PlainTreeItemNode
        {
            Header     = "Parent",
            IsExpanded = true,
            Children   = new List<ITreeItemNode> { child }
        };
        var roots = new AvaloniaList<ITreeItemNode> { parent };
        var treeView = new AtomUI.Desktop.Controls.TreeView
        {
            IsMotionEnabled      = false,
            IsShowEmptyIndicator = false,
            ItemsSource          = roots,
            ItemTemplate = new TreeDataTemplate
            {
                DataType    = typeof(ITreeItemNode),
                ItemsSource = new Binding("Children")
            }
        };

        ShowInWindow(treeView, () =>
        {
            var parentContainer = (AtomTreeViewItem)treeView.ContainerFromItem(parent)!;
            var childContainer  = (AtomTreeViewItem)parentContainer.ContainerFromItem(child)!;
            childContainer.ContainerFromItem(grandChild).ShouldBeAssignableTo<AtomTreeViewItem>();

            roots.Clear();
            Dispatcher.UIThread.RunJobs();

            treeView.ContainerFromItem(parent).ShouldBeNull();
        });
    }

    private class PlainTreeItemNode : ITreeItemNode
    {
        public object? Header { get; set; }
        public PathIcon? Icon => null;
        public bool? IsChecked { get; set; }
        public bool IsSelected { get; set; }
        public bool IsExpanded { get; set; }
        public bool IsIndicatorEnabled { get; set; } = true;
        public string? GroupName => null;
        public bool IsLeaf => false;
        public object? Value { get; set; }
        public bool IsEnabled { get; set; } = true;
        public ITreeNode<ITreeItemNode>? ParentNode => null;
        public EntityKey? ItemKey { get; set; }
        public IEnumerable<ITreeItemNode> Children { get; set; } = new List<ITreeItemNode>();
        public void UpdateParentNode(ITreeItemNode? parentNode)
        {
        }
    }

    [Fact]
    public void Clearing_Plain_Subtree_Releases_Node_Bindings_Of_Discarded_Containers()
    {
        var leaf = new BindableTreeItemNode
        {
            Header = "Leaf"
        };
        var child = new PlainTreeItemNode
        {
            Header     = "Child",
            IsExpanded = true,
            Children   = new List<ITreeItemNode> { leaf }
        };
        var parent = new PlainTreeItemNode
        {
            Header     = "Parent",
            IsExpanded = true,
            Children   = new List<ITreeItemNode> { child }
        };
        var roots = new AvaloniaList<ITreeItemNode> { parent };
        var treeView = new AtomUI.Desktop.Controls.TreeView
        {
            IsMotionEnabled      = false,
            IsShowEmptyIndicator = false,
            ItemsSource          = roots,
            ItemTemplate = new TreeDataTemplate
            {
                DataType    = typeof(ITreeItemNode),
                ItemsSource = new Binding("Children")
            }
        };

        ShowInWindow(treeView, () =>
        {
            var parentContainer = (AtomTreeViewItem)treeView.ContainerFromItem(parent)!;
            var childContainer  = (AtomTreeViewItem)parentContainer.ContainerFromItem(child)!;
            childContainer.ContainerFromItem(leaf).ShouldBeAssignableTo<AtomTreeViewItem>();

            roots.Clear();
            Dispatcher.UIThread.RunJobs();

            // Containers discarded mid-teardown (including ones Avalonia re-realizes from a live
            // ItemsView) must not keep strong PropertyChanged subscriptions on the model node:
            // each leaked subscription roots its whole discarded container chain for as long as
            // the user-kept node lives. Weak-event subscriptions from Avalonia template bindings
            // do not root their subscriber and are collected with it, so only strong container
            // targets are asserted here.
            var leakedContainers = CollectStrongContainerSubscribers(leaf);
            leakedContainers.ShouldBeEmpty(
                "discarded containers still subscribed: " +
                string.Join(", ", leakedContainers.Select(c => c.GetHashCode().ToString("X8"))));
        });
    }

    private static IReadOnlyList<AtomTreeViewItem> CollectStrongContainerSubscribers(AvaloniaObject target)
    {
        var field = typeof(AvaloniaObject).GetField(
            "_propertyChanged",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        var handlers = (Delegate?)field.GetValue(target);
        if (handlers is null)
        {
            return [];
        }

        var containers = new HashSet<AtomTreeViewItem>();
        foreach (var handler in handlers.GetInvocationList())
        {
            if (handler.Target is AtomTreeViewItem container)
            {
                containers.Add(container);
                continue;
            }

            var owner = handler.Target?.GetType().GetField("<>4__this")?.GetValue(handler.Target);
            if (owner is AtomTreeViewItem ownerContainer)
            {
                containers.Add(ownerContainer);
            }
        }

        return containers.ToList();
    }

    [Fact]
    public void Dynamic_Resource_Header_Uses_Owner_TreeView_Resources()
    {
        var resourceKey = CreateResourceKey();
        Application.Current!.Resources[resourceKey] = "Application header";

        var node = new BindableTreeItemNode();
        BindDynamicResource(node, BindableTreeItemNode.HeaderProperty, resourceKey, Application.Current);

        var treeView = new AtomUI.Desktop.Controls.TreeView
        {
            IsShowEmptyIndicator = false,
            ItemsSource          = new[] { node }
        };
        treeView.Resources[resourceKey] = "TreeView header";

        ShowInWindow(treeView, () =>
        {
            node.Header.ShouldBe("TreeView header");
        });
    }

    [Fact]
    public void Removed_BindableTreeItemNode_Without_DynamicResource_Is_Not_Rooted()
    {
        var nodeReference = CreateRemovedPlainNodeReference();

        CollectGarbage();

        nodeReference.IsAlive.ShouldBeFalse(
            "TreeView container recycling must not keep a removed BindableTreeItemNode alive");
    }

    [Fact]
    public void Dynamic_Resource_Header_Does_Not_Root_Unowned_BindableTreeItemNode()
    {
        var nodeReference = CreateUnownedNodeReference(CreateResourceKey());

        CollectGarbage();

        nodeReference.IsAlive.ShouldBeFalse(
            "BindableTreeItemNode dynamic resources must not be rooted when no owner is attached");
    }

    [Fact]
    public void Dynamic_Resource_Header_Does_Not_Root_Removed_BindableTreeItemNode()
    {
        var nodeReference = CreateRemovedNodeReference(CreateResourceKey());

        CollectGarbage();

        nodeReference.IsAlive.ShouldBeFalse(
            "BindableTreeItemNode dynamic resources must not keep a removed node alive through Application.ResourcesChanged");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference CreateRemovedPlainNodeReference()
    {
        BindableTreeItemNode? node = new BindableTreeItemNode { Header = "Plain header" };
        AvaloniaList<BindableTreeItemNode>? nodes = new AvaloniaList<BindableTreeItemNode> { node };
        AtomUI.Desktop.Controls.TreeView? treeView = new AtomUI.Desktop.Controls.TreeView
        {
            IsShowEmptyIndicator = false,
            ItemsSource          = nodes
        };

        var window = new AvaloniaWindow
        {
            Width   = 420,
            Height  = 320,
            Content = treeView
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            nodes.Remove(node);
            RunDispatcherJobsUntil(() => treeView.ContainerFromItem(node) is null);

            var nodeReference = new WeakReference(node);
            treeView.ItemsSource = null;
            node                 = null;
            nodes                = null;
            treeView             = null;
            return nodeReference;
        }
        finally
        {
            window.Content = null;
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference CreateUnownedNodeReference(string resourceKey)
    {
        Application.Current!.Resources[resourceKey] = "Application header";

        var node = new BindableTreeItemNode();
        BindDynamicResource(node, BindableTreeItemNode.HeaderProperty, resourceKey, Application.Current);

        node.Header.ShouldBe("Application header");
        return new WeakReference(node);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference CreateRemovedNodeReference(string resourceKey)
    {
        Application.Current!.Resources[resourceKey] = "Application header";

        BindableTreeItemNode? node = new BindableTreeItemNode();
        BindDynamicResource(node, BindableTreeItemNode.HeaderProperty, resourceKey, Application.Current);

        AvaloniaList<BindableTreeItemNode>? nodes = new AvaloniaList<BindableTreeItemNode> { node };
        AtomUI.Desktop.Controls.TreeView? treeView = new AtomUI.Desktop.Controls.TreeView
        {
            IsShowEmptyIndicator = false,
            ItemsSource          = nodes
        };
        treeView.Resources[resourceKey] = "TreeView header";

        var window = new AvaloniaWindow
        {
            Width   = 420,
            Height  = 320,
            Content = treeView
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            node.Header.ShouldBe("TreeView header");
            nodes.Remove(node);
            RunDispatcherJobsUntil(() => treeView.ContainerFromItem(node) is null);
            node.Header.ShouldBe("Application header");
            var nodeReference = new WeakReference(node);
            treeView.ItemsSource = null;
            node                 = null;
            nodes                = null;
            treeView             = null;
            return nodeReference;
        }
        finally
        {
            window.Content = null;
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static void ShowInWindow(Control content, Action assertion)
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
            assertion();
        }
        finally
        {
            window.Close();
        }
    }

    private static string CreateResourceKey()
    {
        return $"{HeaderResourceKey}.{Guid.NewGuid():N}";
    }

    private static void BindDynamicResource(AvaloniaObject target, AvaloniaProperty property, object key, object? anchor)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var bindMethod = typeof(AvaloniaObject).GetMethod(
            "Bind",
            flags,
            binder: null,
            types: [typeof(AvaloniaProperty), typeof(BindingBase), typeof(object)],
            modifiers: null);

        bindMethod.ShouldNotBeNull();

        var extension   = new DynamicResourceExtension(key);
        var anchorField = typeof(DynamicResourceExtension).GetField("_anchor", flags);
        anchorField.ShouldNotBeNull();
        anchorField.SetValue(extension, anchor);

        bindMethod.Invoke(target, [property, extension, anchor]);
    }

    private static void CollectGarbage()
    {
        for (var i = 0; i < 3; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        Dispatcher.UIThread.RunJobs();
    }

    private static void RunDispatcherJobsUntil(Func<bool> condition, int maxPasses = 128)
    {
        for (var i = 0; i < maxPasses; i++)
        {
            Dispatcher.UIThread.RunJobs();
            if (condition())
            {
                return;
            }
        }
    }
}
