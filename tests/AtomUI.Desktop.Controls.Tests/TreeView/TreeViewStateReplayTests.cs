using System.Collections;
using System.ComponentModel;
using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomTreeViewItem = AtomUI.Desktop.Controls.TreeViewItem;

namespace AtomUI.Desktop.Controls.Tests.TreeView;

public class TreeViewStateReplayTests
{
    static TreeViewStateReplayTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Loaded_Replays_Default_Selected_Checked_And_Expanded_Paths_In_Current_Order()
    {
        var treeView = new AtomUI.Desktop.Controls.TreeView
        {
            ToggleType            = ItemToggleType.CheckBox,
            IsMotionEnabled       = false,
            DefaultSelectedPaths  = [new TreeNodePath("0-0/0-0-1")],
            DefaultCheckedPaths   = [new TreeNodePath("0-0/0-0-1/0-0-1-1")],
            DefaultExpandedPaths  = [new TreeNodePath("0-0/0-0-0")],
            IsShowEmptyIndicator = false
        };
        var (root, firstChild, secondChild, checkedLeaf) = AddDefaultPathTreeItems(treeView);

        ShowInWindow(treeView, () =>
        {
            root.IsExpanded.ShouldBeTrue();
            firstChild.IsExpanded.ShouldBeTrue();
            secondChild.IsExpanded.ShouldBeFalse();
            secondChild.IsSelected.ShouldBeTrue();
            checkedLeaf.IsChecked.ShouldBe(true);
            treeView.SelectedItems.Contains(secondChild).ShouldBeTrue();
            treeView.CheckedItems.Contains(checkedLeaf).ShouldBeTrue();
        });
    }

    [Fact]
    public void ItemsSource_Change_During_Replay_Uses_Latest_Source()
    {
        var firstNodes = CreateDefaultPathDataNodes();
        var secondNodes = CreateDefaultPathDataNodes();
        var thirdNodes = CreateDefaultPathDataNodes();
        var firstSelection = firstNodes[0].Children.ElementAt(1);
        var secondSelection = secondNodes[0].Children.ElementAt(1);
        var thirdSelection = thirdNodes[0].Children.ElementAt(1);
        var treeView = new AtomUI.Desktop.Controls.TreeView
        {
            IsMotionEnabled = false,
            ItemsSource = firstNodes,
            SelectedItem = firstSelection
        };

        ShowInWindow(treeView, () =>
        {
            using var subscription = treeView.GetObservable(AtomUI.Desktop.Controls.TreeView.SelectedItemProperty)
                .Subscribe(item =>
                {
                    if (ReferenceEquals(item, secondSelection))
                    {
                        treeView.ItemsSource = thirdNodes;
                    }
                });

            treeView.ItemsSource = secondNodes;
            Dispatcher.UIThread.RunJobs();

            treeView.ItemsSource.ShouldBeSameAs(thirdNodes);
            treeView.SelectedItem.ShouldBeSameAs(thirdSelection);
        });
    }

    [Fact]
    public void IsDefaultExpandAll_Wins_Over_DefaultExpandedPaths_During_Loaded_Replay()
    {
        var treeView = new AtomUI.Desktop.Controls.TreeView
        {
            IsMotionEnabled      = false,
            IsDefaultExpandAll   = true,
            DefaultExpandedPaths = [new TreeNodePath("missing/path")]
        };
        var (root, firstChild, secondChild, _) = AddDefaultPathTreeItems(treeView);

        ShowInWindow(treeView, () =>
        {
            RunDispatcherJobsUntil(() => root.IsExpanded && firstChild.IsExpanded && secondChild.IsExpanded);

            root.IsExpanded.ShouldBeTrue();
            firstChild.IsExpanded.ShouldBeTrue();
            secondChild.IsExpanded.ShouldBeTrue();
        });
    }

    [Fact]
    public void ItemsSource_Change_Restores_Runtime_Checked_State_Including_Previous_Defaults()
    {
        var firstNodes = CreateDefaultPathDataNodes();
        var secondNodes = CreateDefaultPathDataNodes();
        var treeView = new AtomUI.Desktop.Controls.TreeView
        {
            ToggleType           = ItemToggleType.CheckBox,
            IsMotionEnabled      = false,
            ItemsSource          = firstNodes,
            DefaultSelectedPaths = [new TreeNodePath("0-0/0-0-0")],
            DefaultCheckedPaths  = [new TreeNodePath("0-0/0-0-0")]
        };

        ShowInWindow(treeView, () =>
        {
            var selectedNode = firstNodes[0].Children.ElementAt(1);
            var checkedNode  = firstNodes[0].Children.ElementAt(1).Children.ElementAt(0);
            treeView.SelectedItem = selectedNode;
            treeView.CheckedItems.Add(checkedNode);
            Dispatcher.UIThread.RunJobs();

            var checkedChanges = new List<TreeViewCheckedItemsChangedEventArgs>();
            treeView.CheckedItemsChanged += (_, args) => checkedChanges.Add(args);
            treeView.ItemsSource = secondNodes;
            RunDispatcherJobsUntil(() => ReferenceEquals(treeView.SelectedItem, secondNodes[0].Children.ElementAt(1)));

            treeView.SelectedItem.ShouldBeSameAs(secondNodes[0].Children.ElementAt(1));
            checkedChanges.Count.ShouldBe(1);
            checkedChanges[0].RemovedItems.Cast<object>().ShouldContain(checkedNode);
            checkedChanges[0].AddedItems.Cast<object>()
                          .ShouldContain(secondNodes[0].Children.ElementAt(1).Children.ElementAt(0));
            treeView.CheckedItems.Contains(secondNodes[0].Children.ElementAt(1).Children.ElementAt(0)).ShouldBeTrue();
            treeView.SelectedItems.Contains(secondNodes[0].Children.ElementAt(0)).ShouldBeFalse();
            treeView.CheckedItems.Contains(secondNodes[0].Children.ElementAt(0)).ShouldBeTrue();

            var thirdNodes = CreateDefaultPathDataNodes();
            var explicitCheckedNode = thirdNodes[0].Children.ElementAt(0);
            var addedCheckedNode = new TreeItemNode { Header = "extra", ItemKey = "extra" };
            thirdNodes.Add(addedCheckedNode);
            treeView.ItemsSource = thirdNodes;
            treeView.CheckedItems = new List<object> { explicitCheckedNode };
            treeView.CheckedItems.Add(addedCheckedNode);
            Dispatcher.UIThread.RunJobs();

            treeView.CheckedItems.Contains(explicitCheckedNode).ShouldBeTrue();
            treeView.CheckedItems.Contains(addedCheckedNode).ShouldBeTrue();
            treeView.CheckedItems.Contains(thirdNodes[0].Children.ElementAt(1).Children.ElementAt(0)).ShouldBeFalse();

            treeView.CheckedItems = new AvaloniaList<object>();
            Dispatcher.UIThread.RunJobs();
            checkedChanges.Clear();
            treeView.IsCheckStrictly = true;
            List<ITreeItemNode> fourthNodes =
            [
                new TreeItemNode { Header = "first", ItemKey = "flat-0" },
                new TreeItemNode { Header = "second", ItemKey = "flat-1" }
            ];
            var firstCheckedNode = fourthNodes[0];
            var secondCheckedNode = fourthNodes[1];
            treeView.ItemsSource = fourthNodes;
            treeView.CheckedItems.Add(firstCheckedNode);
            treeView.CheckedItems.Add(secondCheckedNode);
            treeView.CheckedItems.Contains(firstCheckedNode).ShouldBeTrue();
            treeView.CheckedItems.Contains(secondCheckedNode).ShouldBeTrue();
            Dispatcher.UIThread.RunJobs();

            treeView.CheckedItems.Contains(firstCheckedNode).ShouldBeTrue();
            treeView.CheckedItems.Contains(secondCheckedNode).ShouldBeTrue();
            checkedChanges.Count.ShouldBe(1);
        });
    }

    [Fact]
    public void ItemsSource_Change_In_Multiple_Mode_Restores_SelectedItems_Before_SelectedItem()
    {
        var firstNodes  = CreateDefaultPathDataNodes();
        var secondNodes = CreateDefaultPathDataNodes();
        var root        = firstNodes[0];
        var firstChild  = root.Children.ElementAt(0);
        var secondChild = root.Children.ElementAt(1);
        var treeView = new AtomUI.Desktop.Controls.TreeView
        {
            IsMotionEnabled      = false,
            ItemsSource          = firstNodes,
            SelectionMode        = SelectionMode.Multiple,
            DefaultExpandedPaths = [new TreeNodePath("0-0/0-0-0"), new TreeNodePath("0-0/0-0-1")]
        };

        ShowInWindow(treeView, () =>
        {
            treeView.SelectedItem  = firstChild;
            treeView.SelectedItems = new List<ITreeItemNode> { firstChild, secondChild };
            Dispatcher.UIThread.RunJobs();

            treeView.ItemsSource = secondNodes;
            RunDispatcherJobsUntil(() => treeView.SelectedItems.Count == 2);

            treeView.SelectedItems.Count.ShouldBe(2);
            treeView.SelectedItems.Contains(secondNodes[0].Children.ElementAt(0)).ShouldBeTrue();
            treeView.SelectedItems.Contains(secondNodes[0].Children.ElementAt(1)).ShouldBeTrue();
            Dispatcher.UIThread.RunJobs();

            var thirdNodes = CreateDefaultPathDataNodes();
            var explicitSelection = thirdNodes[0].Children.ElementAt(1);
            var addedSelection = thirdNodes[0].Children.ElementAt(0);
            treeView.ItemsSource = thirdNodes;
            treeView.SelectedItems = new List<ITreeItemNode> { explicitSelection };
            treeView.SelectedItems.Add(addedSelection);
            Dispatcher.UIThread.RunJobs();

            treeView.SelectedItems.Count.ShouldBe(2);
            treeView.SelectedItems[0].ShouldBeSameAs(explicitSelection);
            treeView.SelectedItems[1].ShouldBeSameAs(addedSelection);
        });
    }

    [Fact]
    public void Loaded_Replays_Bound_SelectedItems_In_Multiple_Mode()
    {
        var nodes       = CreateDefaultPathDataNodes();
        var root        = nodes[0];
        var firstChild  = root.Children.ElementAt(0);
        var secondChild = root.Children.ElementAt(1);
        IList selectedItems = new List<ITreeItemNode> { firstChild, secondChild };
        var treeView = new AtomUI.Desktop.Controls.TreeView
        {
            IsMotionEnabled      = false,
            ItemsSource          = nodes,
            SelectionMode        = SelectionMode.Multiple,
            SelectedItems        = selectedItems,
            DefaultExpandedPaths = [new TreeNodePath("0-0/0-0-0"), new TreeNodePath("0-0/0-0-1")]
        };

        ShowInWindow(treeView, () =>
        {
            RunDispatcherJobsUntil(() =>
                FindContainerForNode(treeView, firstChild) is not null &&
                FindContainerForNode(treeView, secondChild) is not null);

            FindContainerForNode(treeView, firstChild)!.IsSelected.ShouldBeTrue();
            FindContainerForNode(treeView, secondChild)!.IsSelected.ShouldBeTrue();
            treeView.SelectedItems.ShouldBeSameAs(selectedItems);
        });
    }

    [Fact]
    public void SelectedItems_Change_After_Load_Selects_Data_Node_Containers()
    {
        var nodes       = CreateDefaultPathDataNodes();
        var root        = nodes[0];
        var firstChild  = root.Children.ElementAt(0);
        var secondChild = root.Children.ElementAt(1);
        var treeView = new AtomUI.Desktop.Controls.TreeView
        {
            IsMotionEnabled      = false,
            ItemsSource          = nodes,
            SelectionMode        = SelectionMode.Multiple,
            DefaultExpandedPaths = [new TreeNodePath("0-0/0-0-0"), new TreeNodePath("0-0/0-0-1")]
        };

        ShowInWindow(treeView, () =>
        {
            RunDispatcherJobsUntil(() =>
                FindContainerForNode(treeView, firstChild) is not null &&
                FindContainerForNode(treeView, secondChild) is not null);

            IList selectedItems = new List<ITreeItemNode> { firstChild, secondChild };
            treeView.SelectedItems = selectedItems;
            Dispatcher.UIThread.RunJobs();

            FindContainerForNode(treeView, firstChild)!.IsSelected.ShouldBeTrue();
            FindContainerForNode(treeView, secondChild)!.IsSelected.ShouldBeTrue();
            treeView.SelectedItems.ShouldBeSameAs(selectedItems);
        });
    }

    [Fact]
    public void SelectedItems_Change_After_Load_Selects_All_Data_Node_Containers_When_ItemsSource_Is_Shared()
    {
        var nodes       = CreateDefaultPathDataNodes();
        var root        = nodes[0];
        var firstChild  = root.Children.ElementAt(0);
        var secondChild = root.Children.ElementAt(1);
        var singleTreeView = new AtomUI.Desktop.Controls.TreeView
        {
            IsMotionEnabled      = false,
            ItemsSource          = nodes,
            SelectedItem         = secondChild,
            DefaultExpandedPaths = [new TreeNodePath("0-0/0-0-0"), new TreeNodePath("0-0/0-0-1")]
        };
        var multipleTreeView = new AtomUI.Desktop.Controls.TreeView
        {
            IsMotionEnabled      = false,
            ItemsSource          = nodes,
            SelectionMode        = SelectionMode.Multiple,
            DefaultExpandedPaths = [new TreeNodePath("0-0/0-0-0"), new TreeNodePath("0-0/0-0-1")]
        };

        var panel = new StackPanel();
        panel.Children.Add(singleTreeView);
        panel.Children.Add(multipleTreeView);

        ShowInWindow(panel, () =>
        {
            RunDispatcherJobsUntil(() =>
                FindContainerForNode(multipleTreeView, firstChild) is not null &&
                FindContainerForNode(multipleTreeView, secondChild) is not null);

            IList selectedItems = new List<ITreeItemNode> { firstChild, secondChild };
            multipleTreeView.SelectedItems = selectedItems;
            Dispatcher.UIThread.RunJobs();

            FindContainerForNode(multipleTreeView, firstChild)!.IsSelected.ShouldBeTrue();
            FindContainerForNode(multipleTreeView, secondChild)!.IsSelected.ShouldBeTrue();
            multipleTreeView.SelectedItems.Count.ShouldBe(2);
            multipleTreeView.SelectedItems.Contains(firstChild).ShouldBeTrue();
            multipleTreeView.SelectedItems.Contains(secondChild).ShouldBeTrue();
        });
    }

    [Fact]
    public void SelectedItems_TwoWayBinding_Replaces_Previous_Multiple_Selection_With_All_ViewModel_Items()
    {
        var nodes       = CreateDefaultPathDataNodes();
        var root        = nodes[0];
        var firstChild  = root.Children.ElementAt(0);
        var secondChild = root.Children.ElementAt(1);
        var viewModel = new TreeViewSelectionBindingViewModel
        {
            SelectedItems = new List<ITreeItemNode> { secondChild }
        };
        var treeView = new AtomUI.Desktop.Controls.TreeView
        {
            IsMotionEnabled      = false,
            ItemsSource          = nodes,
            SelectionMode        = SelectionMode.Multiple,
            DefaultExpandedPaths = [new TreeNodePath("0-0/0-0-0"), new TreeNodePath("0-0/0-0-1")]
        };
        treeView.Bind(
            AtomUI.Desktop.Controls.TreeView.SelectedItemsProperty,
            new Binding(nameof(TreeViewSelectionBindingViewModel.SelectedItems))
            {
                Source = viewModel,
                Mode   = BindingMode.TwoWay
            });

        ShowInWindow(treeView, () =>
        {
            RunDispatcherJobsUntil(() =>
                FindContainerForNode(treeView, firstChild) is not null &&
                FindContainerForNode(treeView, secondChild) is not null);

            FindContainerForNode(treeView, secondChild)!.IsSelected.ShouldBeTrue();

            viewModel.SelectedItems = new List<ITreeItemNode> { firstChild, secondChild };
            Dispatcher.UIThread.RunJobs();

            viewModel.SelectedItems.Count.ShouldBe(2);
            viewModel.SelectedItems.Contains(firstChild).ShouldBeTrue();
            viewModel.SelectedItems.Contains(secondChild).ShouldBeTrue();
            treeView.SelectedItems.Count.ShouldBe(2);
            treeView.SelectedItems.Contains(firstChild).ShouldBeTrue();
            treeView.SelectedItems.Contains(secondChild).ShouldBeTrue();
            FindContainerForNode(treeView, firstChild)!.IsSelected.ShouldBeTrue();
            FindContainerForNode(treeView, secondChild)!.IsSelected.ShouldBeTrue();
        });
    }

    [Fact]
    public void SelectedItems_TwoWayBinding_Preserves_Common_Selected_Container_When_ViewModel_Collection_Replaced()
    {
        var nodes       = CreateDefaultPathDataNodes();
        var root        = nodes[0];
        var firstChild  = root.Children.ElementAt(0);
        var secondChild = root.Children.ElementAt(1);
        var viewModel = new TreeViewSelectionBindingViewModel
        {
            SelectedItems = new List<ITreeItemNode> { firstChild }
        };
        var treeView = new AtomUI.Desktop.Controls.TreeView
        {
            IsMotionEnabled      = false,
            ItemsSource          = nodes,
            SelectionMode        = SelectionMode.Multiple,
            DefaultExpandedPaths = [new TreeNodePath("0-0/0-0-0"), new TreeNodePath("0-0/0-0-1")]
        };
        treeView.Bind(
            AtomUI.Desktop.Controls.TreeView.SelectedItemsProperty,
            new Binding(nameof(TreeViewSelectionBindingViewModel.SelectedItems))
            {
                Source = viewModel,
                Mode   = BindingMode.TwoWay
            });

        ShowInWindow(treeView, () =>
        {
            RunDispatcherJobsUntil(() =>
                FindContainerForNode(treeView, firstChild) is not null &&
                FindContainerForNode(treeView, secondChild) is not null);

            var firstContainer = FindContainerForNode(treeView, firstChild)!;
            firstContainer.IsSelected.ShouldBeTrue();
            var becameUnselected = false;
            firstContainer.PropertyChanged += (_, args) =>
            {
                if (args.Property == AtomTreeViewItem.IsSelectedProperty &&
                    args.NewValue is false)
                {
                    becameUnselected = true;
                }
            };

            viewModel.SelectedItems = new List<ITreeItemNode> { firstChild, secondChild };
            Dispatcher.UIThread.RunJobs();

            becameUnselected.ShouldBeFalse();
            firstContainer.IsSelected.ShouldBeTrue();
            FindContainerForNode(treeView, secondChild)!.IsSelected.ShouldBeTrue();
        });
    }

    [Fact]
    public void SelectedItems_Change_After_Load_Clears_Data_Node_Containers()
    {
        var nodes       = CreateDefaultPathDataNodes();
        var root        = nodes[0];
        var firstChild  = root.Children.ElementAt(0);
        var secondChild = root.Children.ElementAt(1);
        IList selectedItems = new List<ITreeItemNode> { firstChild, secondChild };
        var treeView = new AtomUI.Desktop.Controls.TreeView
        {
            IsMotionEnabled      = false,
            ItemsSource          = nodes,
            SelectionMode        = SelectionMode.Multiple,
            SelectedItems        = selectedItems,
            DefaultExpandedPaths = [new TreeNodePath("0-0/0-0-0"), new TreeNodePath("0-0/0-0-1")]
        };

        ShowInWindow(treeView, () =>
        {
            RunDispatcherJobsUntil(() =>
                FindContainerForNode(treeView, firstChild) is not null &&
                FindContainerForNode(treeView, secondChild) is not null);

            treeView.SelectedItems = new List<ITreeItemNode>();
            Dispatcher.UIThread.RunJobs();

            FindContainerForNode(treeView, firstChild)!.IsSelected.ShouldBeFalse();
            FindContainerForNode(treeView, secondChild)!.IsSelected.ShouldBeFalse();
        });
    }

    [Fact]
    public void Default_Checked_Paths_Cascade_To_Parents_When_CheckStrictly_Is_False()
    {
        var treeView = new AtomUI.Desktop.Controls.TreeView
        {
            ToggleType           = ItemToggleType.CheckBox,
            IsMotionEnabled      = false,
            IsCheckStrictly      = false,
            DefaultCheckedPaths  = [new TreeNodePath("0-0/0-0-1/0-0-1-1")]
        };
        var (root, _, secondChild, checkedLeaf) = AddDefaultPathTreeItems(treeView);

        ShowInWindow(treeView, () =>
        {
            checkedLeaf.IsChecked.ShouldBe(true);
            secondChild.IsChecked.ShouldBe(true);
            root.IsChecked.ShouldBe(null);
            treeView.CheckedItems.Contains(checkedLeaf).ShouldBeTrue();
            treeView.CheckedItems.Contains(secondChild).ShouldBeTrue();
        });
    }

    [Fact]
    public void Strict_Default_Checked_State_Updates_Current_Item_Without_Cascade()
    {
        var nodes = CreateDefaultPathDataNodes();
        var treeView = new AtomUI.Desktop.Controls.TreeView
        {
            ToggleType           = ItemToggleType.CheckBox,
            IsMotionEnabled      = false,
            IsCheckStrictly      = true,
            ItemsSource          = nodes,
            DefaultCheckedPaths = [new TreeNodePath("0-0/0-0-1/0-0-1-1")]
        };
        var root        = nodes[0];
        var secondChild = root.Children.ElementAt(1);
        var checkedLeaf = secondChild.Children.ElementAt(0);

        ShowInWindow(treeView, () =>
        {
            treeView.CheckedItems.Contains(checkedLeaf).ShouldBeTrue();
            treeView.CheckedItems.Contains(secondChild).ShouldBeFalse();
            treeView.CheckedItems.Contains(root).ShouldBeFalse();
        });
    }

    private static (AtomTreeViewItem Root, AtomTreeViewItem FirstChild, AtomTreeViewItem SecondChild, AtomTreeViewItem CheckedLeaf)
        AddDefaultPathTreeItems(AtomUI.Desktop.Controls.TreeView treeView)
    {
        var checkedLeaf = new AtomTreeViewItem
        {
            Header  = "checked leaf",
            ItemKey = "0-0-1-1"
        };
        var secondChild = new AtomTreeViewItem
        {
            Header  = "second child",
            ItemKey = "0-0-1"
        };
        secondChild.Items.Add(checkedLeaf);
        var firstChild = new AtomTreeViewItem
        {
            Header  = "first child",
            ItemKey = "0-0-0"
        };
        firstChild.Items.Add(new AtomTreeViewItem
        {
            Header  = "first leaf",
            ItemKey = "0-0-0-0"
        });
        var root = new AtomTreeViewItem
        {
            Header  = "root",
            ItemKey = "0-0"
        };
        root.Items.Add(firstChild);
        root.Items.Add(secondChild);
        treeView.Items.Add(root);
        return (root, firstChild, secondChild, checkedLeaf);
    }

    private static List<ITreeItemNode> CreateDefaultPathDataNodes()
    {
        return
        [
            new TreeItemNode
            {
                Header  = "root",
                ItemKey = "0-0",
                Children =
                [
                    new TreeItemNode
                    {
                        Header  = "first child",
                        ItemKey = "0-0-0"
                    },
                    new TreeItemNode
                    {
                        Header  = "second child",
                        ItemKey = "0-0-1",
                        Children =
                        [
                            new TreeItemNode
                            {
                                Header  = "checked leaf",
                                ItemKey = "0-0-1-1"
                            }
                        ]
                    }
                ]
            }
        ];
    }

    private static AtomTreeViewItem? FindContainerForNode(Control root, ITreeItemNode node)
    {
        return root.GetVisualDescendants()
                   .OfType<AtomTreeViewItem>()
                   .FirstOrDefault(item => ReferenceEquals(item.Header, node));
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var window = new Avalonia.Controls.Window
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

    private sealed class TreeViewSelectionBindingViewModel : INotifyPropertyChanged
    {
        private IList? _selectedItems;

        public event PropertyChangedEventHandler? PropertyChanged;

        public IList? SelectedItems
        {
            get => _selectedItems;
            set
            {
                if (ReferenceEquals(_selectedItems, value))
                {
                    return;
                }
                _selectedItems = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedItems)));
            }
        }
    }
}
