using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using AtomUI.Controls.Primitives;
using Avalonia.Controls;
using AtomUI.Desktop.Controls;
using Avalonia;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace AtomUIGallery.ShowCases.TreeView;

public partial class TreeViewShowCase : GalleryReactiveUserControl<TreeViewViewModel>
{
    public const string LanguageId = nameof(TreeViewShowCase);

    private AtomUI.Desktop.Controls.TreeViewItem? _contextMenuTargetItem;

    public TreeViewShowCase()
    {
        this.WhenActivated(disposables =>
        {
            if (DataContext is TreeViewViewModel viewModel)
            {
                InitBasicTreeViewData(viewModel);
                viewModel.TreeViewNodeHoverMode = TreeItemHoverMode.Default;
                RefreshLocalizedTreeNodes(viewModel);
                InitCustomizeCollapseExpandTreeDefaultExpandedPaths(viewModel);
                InitFilterTreeNodes(viewModel);
                viewModel.AsyncLoadTreeNodeLoader = new TreeItemDataLoader();

                var languageManager = GalleryLocalization.GetLanguageManager();
                if (languageManager != null)
                {
                    EventHandler<LanguageChangedEventArgs> handler = (_, _) => RefreshLocalizedTreeNodes(viewModel);
                    languageManager.LanguageChanged += handler;
                    Disposable.Create(() => languageManager.LanguageChanged -= handler)
                              .DisposeWith(disposables);
                }

                Disposable.Create(() =>
                {
                    viewModel.BasicTreeViewDefaultExpandedPaths = null;
                    viewModel.BasicTreeViewDefaultSelectedPaths = null;
                    viewModel.BasicTreeViewDefaultCheckedPaths  = null;
                    viewModel.BasicTreeNodes                    = null;
                    viewModel.BoundSelectedTreeNode             = null;
                    viewModel.BoundSelectedTreeNodes            = null;
                    viewModel.AsyncLoadTreeNodes                = null;
                    viewModel.AsyncLoadTreeNodeLoader           = null;
                    viewModel.FilterTreeNodes                   = null;
                }).DisposeWith(disposables);
            }
        });
        InitializeComponent();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _contextMenuTargetItem = null;
    }

    private void HandleHoverModeChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is AtomUIRadioButton radioButton &&
            radioButton.IsChecked == true &&
            radioButton.Tag is TreeItemHoverMode hoverMode &&
            DataContext is TreeViewViewModel viewModel)
        {
            viewModel.TreeViewNodeHoverMode = hoverMode;
        }
    }

    private void HandleSelectFirstBindingTreeNodeClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is TreeViewViewModel viewModel)
        {
            viewModel.SelectFirstBindingTreeNode();
        }
    }

    private void HandleSelectSecondBindingTreeNodeClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is TreeViewViewModel viewModel)
        {
            viewModel.SelectSecondBindingTreeNode();
        }
    }

    private void HandleSelectBothBindingTreeNodesClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is TreeViewViewModel viewModel)
        {
            viewModel.SelectBothBindingTreeNodes();
        }
    }

    private void HandleClearBindingTreeNodeSelectionClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is TreeViewViewModel viewModel)
        {
            viewModel.ClearBindingTreeNodeSelection();
        }
    }

    private void HandleSelectFirstBindingTreeNodesClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is TreeViewViewModel viewModel)
        {
            viewModel.SelectFirstBindingTreeNodes();
        }
    }

    private void HandleSelectSecondBindingTreeNodesClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is TreeViewViewModel viewModel)
        {
            viewModel.SelectSecondBindingTreeNodes();
        }
    }

    private void HandleClearBindingTreeNodesSelectionClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is TreeViewViewModel viewModel)
        {
            viewModel.ClearBindingTreeNodesSelection();
        }
    }

    private void HandleFilterItemsSourceTreeSearchRequested(object? sender, SearchRequestedEventArgs e)
    {
        if (sender is SearchEdit searchEdit &&
            TryFindTemplateTreeView(searchEdit, "SearchTreeViewByItemsSource", out var treeView))
        {
            treeView.FilterValue = e.Query.Trim();
        }
    }

    private void HandleFilterTreeSearchRequested(object? sender, SearchRequestedEventArgs e)
    {
        if (sender is SearchEdit searchEdit &&
            TryFindTemplateTreeView(searchEdit, "SearchTreeView", out var treeView))
        {
            treeView.FilterValue = e.Query.Trim();
        }
    }

    private static bool TryFindTemplateTreeView(Control source, string treeViewName, out AtomUITreeView treeView)
    {
        var parent = source.Parent as Control;
        while (parent is not null)
        {
            if (parent is AtomUITreeView directTreeView &&
                directTreeView.Name == treeViewName)
            {
                treeView = directTreeView;
                return true;
            }

            var descendantTreeView = parent.GetVisualDescendants()
                                           .OfType<AtomUITreeView>()
                                           .FirstOrDefault(candidate => candidate.Name == treeViewName);
            if (descendantTreeView is not null)
            {
                treeView = descendantTreeView;
                return true;
            }

            parent = parent.Parent as Control;
        }

        treeView = null!;
        return false;
    }

    private void HandleContextMenuTreeItemContextMenuRequest(object? sender, TreeItemContextMenuEventArgs e)
    {
        _contextMenuTargetItem = e.ViewItem;
        if (sender is AtomUITreeView treeView &&
            treeView.Resources.TryGetValue("TreeItemContextMenu", out var resource) &&
            resource is AtomUI.Desktop.Controls.MenuFlyout flyout)
        {
            flyout.ShowAt(e.ViewItem);
        }
    }

    private void HandleContextMenuNewNodeClick(object? sender, RoutedEventArgs e)
    {
        if (_contextMenuTargetItem is null)
        {
            return;
        }

        var header = _contextMenuTargetItem.Header?.ToString() ??
                     Lang(TreeViewShowCaseLangResourceKind.P2HeaderNodeFallback, "node");
        var newItem = new AtomUI.Desktop.Controls.TreeViewItem
        {
            Header = GalleryLocalization.Format(
                TreeViewShowCaseLangResourceKind.P2HeaderNewNodeFormat,
                "{0} / new ({1})",
                header,
                _contextMenuTargetItem.Items.Count + 1)
        };
        _contextMenuTargetItem.Items.Add(newItem);
        _contextMenuTargetItem.IsExpanded = true;
    }

    private void HandleContextMenuRenameClick(object? sender, RoutedEventArgs e)
    {
        if (_contextMenuTargetItem is null)
        {
            return;
        }

        var header = _contextMenuTargetItem.Header?.ToString() ??
                     Lang(TreeViewShowCaseLangResourceKind.P2HeaderNodeFallback, "node");
        _contextMenuTargetItem.Header = GalleryLocalization.Format(
            TreeViewShowCaseLangResourceKind.P2HeaderRenamedFormat,
            "{0} (renamed)",
            header);
    }

    private void HandleContextMenuDeleteClick(object? sender, RoutedEventArgs e)
    {
        if (_contextMenuTargetItem is null)
        {
            return;
        }

        if (_contextMenuTargetItem.Parent is AtomUI.Desktop.Controls.TreeViewItem parentItem)
        {
            parentItem.Items.Remove(_contextMenuTargetItem);
        }
        else if (_contextMenuTargetItem.Parent is AtomUITreeView parentTree)
        {
            parentTree.Items.Remove(_contextMenuTargetItem);
        }
        _contextMenuTargetItem = null;
    }

    private void RefreshLocalizedTreeNodes(TreeViewViewModel viewModel)
    {
        InitBasicTreeNodes(viewModel);
        InitSelectionBindingData(viewModel);
        InitAsyncLoadTreeNodes(viewModel);
    }

    internal static string Lang(TreeViewShowCaseLangResourceKind resourceKind, string fallback)
    {
        return TreeViewShowCaseLanguage.Get(resourceKind, fallback);
    }

    private void InitBasicTreeViewData(TreeViewViewModel viewModel)
    {
        viewModel.BasicTreeViewDefaultExpandedPaths = [
            new TreeNodePath("0-0/0-0-0"),
            new TreeNodePath("0-0/0-0-1/0-0-1-1")
        ];
        
        viewModel.BasicTreeViewDefaultSelectedPaths =
        [
            new TreeNodePath("0-0/0-0-1")
        ];
        
        viewModel.BasicTreeViewDefaultCheckedPaths =
        [
            new TreeNodePath("0-0/0-0-1/0-0-1-1")
        ];

        viewModel.SemanticPartTreeDefaultSelectedPaths =
        [
            new TreeNodePath("0-0/0-0-1")
        ];

        viewModel.SemanticPartPreviewDefaultSelectedPaths =
        [
            new TreeNodePath("0-0/0-0-0")
        ];
    }

    private void InitBasicTreeNodes(TreeViewViewModel viewModel)
    {
        viewModel.BasicTreeNodes = [
            new TreeItemNode()
            {
                Header  = Lang(TreeViewShowCaseLangResourceKind.P2HeaderParentN1, "parent 1"),
                ItemKey = "0-0",
                Children = [
                    new TreeItemNode()
                    {
                        Header  = Lang(TreeViewShowCaseLangResourceKind.P2HeaderParentN1N0, "parent 1-0"),
                        ItemKey = "0-0-0",
                        Children = [
                            new TreeItemNode()
                            {
                                Header    = Lang(TreeViewShowCaseLangResourceKind.P2HeaderLeafN1, "leaf 1"),
                                ItemKey   = "0-0-0-0",
                                IsEnabled = false
                            },
                            new TreeItemNode()
                            {
                                Header  = Lang(TreeViewShowCaseLangResourceKind.P2HeaderLeafN2, "leaf 2"),
                                ItemKey = "0-0-0-1"
                            }
                        ]
                    },
                    new TreeItemNode()
                    {
                        Header  = Lang(TreeViewShowCaseLangResourceKind.P2HeaderParentN1N1, "parent 1-1"),
                        ItemKey = "0-0-1",
                        Children = [
                            new TreeItemNode()
                            {
                                Header  = Lang(TreeViewShowCaseLangResourceKind.P2HeaderSss, "sss"),
                                ItemKey = "0-0-1-0",
                                Children = [
                                    new TreeItemNode()
                                    {
                                        Header  = Lang(TreeViewShowCaseLangResourceKind.P2HeaderCcc, "ccc"),
                                        ItemKey = "0-0-1-0-0"
                                    }
                                ]
                            },
                            new TreeItemNode()
                            {
                                Header  = Lang(TreeViewShowCaseLangResourceKind.P2HeaderXxx, "xxx"),
                                ItemKey = "0-0-1-1",
                                Children = [
                                    new TreeItemNode()
                                    {
                                        Header  = Lang(TreeViewShowCaseLangResourceKind.P2HeaderAaaa, "aaaa"),
                                        ItemKey = "0-0-1-1-0"
                                    }
                                ]
                            }
                        ]
                    }
                ]
            }
        ];
    }

    private static void InitSelectionBindingData(TreeViewViewModel viewModel)
    {
        if (viewModel.BasicTreeNodes is not { Count: > 0 } nodes ||
            nodes[0] is not TreeItemNode root ||
            root.Children.Count < 2)
        {
            viewModel.BoundSelectedTreeNode  = null;
            viewModel.BoundSelectedTreeNodes = null;
            return;
        }

        var firstChild  = root.Children[0];
        var secondChild = root.Children[1];

        viewModel.BoundSelectedTreeNode  = secondChild;
        viewModel.BoundSelectedTreeNodes = new ObservableCollection<ITreeItemNode> { firstChild, secondChild };
    }

    private void InitCustomizeCollapseExpandTreeDefaultExpandedPaths(TreeViewViewModel viewModel)
    {
        viewModel.CustomizeCollapseExpandTreeDefaultExpandedPaths = [
            new TreeNodePath("0-0/0-0-0")
        ];
    }

    private void InitAsyncLoadTreeNodes(TreeViewViewModel viewModel)
    {
        viewModel.AsyncLoadTreeNodes =
        [
            new TreeItemNode()
            {
                Header  = Lang(TreeViewShowCaseLangResourceKind.P2HeaderExpandToLoad, "Expand to load"),
                ItemKey = "0",
            },
            new TreeItemNode()
            {
                Header  = Lang(TreeViewShowCaseLangResourceKind.P2HeaderExpandToLoad, "Expand to load"),
                ItemKey = "1",
            },
            new TreeItemNode()
            {
                Header  = Lang(TreeViewShowCaseLangResourceKind.P2HeaderTreeNode, "Tree Node"),
                ItemKey = "2",
                IsLeaf  = true
            }
        ];
    }

    private void InitFilterTreeNodes(TreeViewViewModel viewModel)
    {
        viewModel.FilterTreeNodes =
        [
            new TreeItemNode()
            {
                Header  = "0-0",
                ItemKey = "0-0",
                Children = [
                    new TreeItemNode()
                    {
                        Header  = "0-0-0",
                        ItemKey = "0-0-0",
                        Children = [
                            new TreeItemNode()
                            {
                                Header  = "0-0-0-0",
                                ItemKey = "0-0-0-0",
                            },
                            new TreeItemNode()
                            {
                                Header  = "0-0-0-1",
                                ItemKey = "0-0-0-1",
                            },
                            new TreeItemNode()
                            {
                                Header  = "0-0-0-2",
                                ItemKey = "0-0-0-2",
                            }
                        ]
                    },
                    new TreeItemNode()
                    {
                        Header  = "0-0-1",
                        ItemKey = "0-0-1",
                        Children = [
                            new TreeItemNode()
                            {
                                Header  = "0-0-1-0",
                                ItemKey = "0-0-1-0",
                            },
                            new TreeItemNode()
                            {
                                Header  = "0-0-1-1",
                                ItemKey = "0-0-1-1",
                            },
                            new TreeItemNode()
                            {
                                Header  = "0-0-1-2",
                                ItemKey = "0-0-1-2",
                            }
                        ]
                    },
                    new TreeItemNode()
                    {
                        Header  ="0-0-2",
                        ItemKey ="0-0-2",
                    },
                ]
            },
            new TreeItemNode()
            {
                Header  = "0-1",
                ItemKey = "0-1",
                Children = [
                    new TreeItemNode()
                    {
                        Header  = "0-1-0",
                        ItemKey = "0-1-0",
                        Children = [
                            new TreeItemNode()
                            {
                                Header  = "0-1-0-0",
                                ItemKey = "0-1-0-0",
                            },
                            new TreeItemNode()
                            {
                                Header  = "0-1-0-1",
                                ItemKey = "0-1-0-1",
                            },
                            new TreeItemNode()
                            {
                                Header  = "0-1-0-2",
                                ItemKey = "0-1-0-2",
                            }
                        ]
                    },
                    new TreeItemNode()
                    {
                        Header  = "0-1-1",
                        ItemKey = "0-1-1",
                        Children = [
                            new TreeItemNode()
                            {
                                Header  = "0-1-1-0",
                                ItemKey = "0-1-1-0",
                            },
                            new TreeItemNode()
                            {
                                Header  = "0-1-1-1",
                                ItemKey = "0-1-1-1",
                            },
                            new TreeItemNode()
                            {
                                Header  = "0-1-1-2",
                                ItemKey = "0-1-1-2",
                            }
                        ]
                    },
                    new TreeItemNode()
                    {
                        Header  ="0-1-2",
                        ItemKey ="0-1-2",
                    },
                ]
            },
            new TreeItemNode()
            {
                Header  = "0-2",
                ItemKey = "0-2",
            }
        ];
    }

}

internal static class TreeViewShowCaseLanguage
{
    public static string Get(TreeViewShowCaseLangResourceKind resourceKind, string fallback)
    {
        if (Application.Current is null)
        {
            return fallback;
        }

        return GalleryLocalization.Get(resourceKind, fallback);
    }
}
