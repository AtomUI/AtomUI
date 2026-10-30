using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using AtomUI.Desktop.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.TreeView;

public class TreeViewViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "TreeView";

    public IScreen HostScreen { get; }

    public string? UrlPathSegment => ID.ToString();

    private IList<TreeNodePath>? _basicTreeViewDefaultExpandedPaths;
    public IList<TreeNodePath>? BasicTreeViewDefaultExpandedPaths
    {
        get => _basicTreeViewDefaultExpandedPaths;
        set => this.RaiseAndSetIfChanged(ref _basicTreeViewDefaultExpandedPaths, value);
    }

    private IList<TreeNodePath>? _basicTreeViewDefaultSelectedPaths;
    public IList<TreeNodePath>? BasicTreeViewDefaultSelectedPaths
    {
        get => _basicTreeViewDefaultSelectedPaths;
        set => this.RaiseAndSetIfChanged(ref _basicTreeViewDefaultSelectedPaths, value);
    }

    private IList<TreeNodePath>? _basicTreeViewDefaultCheckedPaths;
    public IList<TreeNodePath>? BasicTreeViewDefaultCheckedPaths
    {
        get => _basicTreeViewDefaultCheckedPaths;
        set => this.RaiseAndSetIfChanged(ref _basicTreeViewDefaultCheckedPaths, value);
    }

    private IList<ITreeItemNode>? _basicTreeNodes;
    public IList<ITreeItemNode>? BasicTreeNodes
    {
        get => _basicTreeNodes;
        set => this.RaiseAndSetIfChanged(ref _basicTreeNodes, value);
    }

    private ITreeItemNode? _boundSelectedTreeNode;
    public ITreeItemNode? BoundSelectedTreeNode
    {
        get => _boundSelectedTreeNode;
        set
        {
            this.RaiseAndSetIfChanged(ref _boundSelectedTreeNode, value);
            this.RaisePropertyChanged(nameof(BoundSelectedTreeNodeText));
        }
    }

    private IList? _boundSelectedTreeNodes;
    private INotifyCollectionChanged? _boundSelectedTreeNodesCollectionChangedSource;
    public IList? BoundSelectedTreeNodes
    {
        get => _boundSelectedTreeNodes;
        set
        {
            if (ReferenceEquals(_boundSelectedTreeNodes, value))
            {
                this.RaisePropertyChanged(nameof(BoundSelectedTreeNodesText));
                return;
            }

            if (_boundSelectedTreeNodesCollectionChangedSource != null)
            {
                _boundSelectedTreeNodesCollectionChangedSource.CollectionChanged -= HandleBoundSelectedTreeNodesCollectionChanged;
            }

            this.RaiseAndSetIfChanged(ref _boundSelectedTreeNodes, value);

            _boundSelectedTreeNodesCollectionChangedSource = value as INotifyCollectionChanged;
            if (_boundSelectedTreeNodesCollectionChangedSource != null)
            {
                _boundSelectedTreeNodesCollectionChangedSource.CollectionChanged += HandleBoundSelectedTreeNodesCollectionChanged;
            }
            this.RaisePropertyChanged(nameof(BoundSelectedTreeNodesText));
        }
    }

    public string BoundSelectedTreeNodeText => FormatSelectedNode(BoundSelectedTreeNode);

    public string BoundSelectedTreeNodesText => FormatSelectedNodes(BoundSelectedTreeNodes);

    private IList<TreeNodePath>? _customizeCollapseExpandTreeDefaultExpandedPaths;
    public IList<TreeNodePath>? CustomizeCollapseExpandTreeDefaultExpandedPaths
    {
        get => _customizeCollapseExpandTreeDefaultExpandedPaths;
        set => this.RaiseAndSetIfChanged(ref _customizeCollapseExpandTreeDefaultExpandedPaths, value);
    }

    private IList<TreeNodePath>? _semanticPartTreeDefaultSelectedPaths;
    public IList<TreeNodePath>? SemanticPartTreeDefaultSelectedPaths
    {
        get => _semanticPartTreeDefaultSelectedPaths;
        set => this.RaiseAndSetIfChanged(ref _semanticPartTreeDefaultSelectedPaths, value);
    }

    private IList<TreeNodePath>? _semanticPartPreviewDefaultSelectedPaths;
    public IList<TreeNodePath>? SemanticPartPreviewDefaultSelectedPaths
    {
        get => _semanticPartPreviewDefaultSelectedPaths;
        set => this.RaiseAndSetIfChanged(ref _semanticPartPreviewDefaultSelectedPaths, value);
    }

    private IList<ITreeItemNode>? _asyncLoadTreeNodes;
    public IList<ITreeItemNode>? AsyncLoadTreeNodes
    {
        get => _asyncLoadTreeNodes;
        set => this.RaiseAndSetIfChanged(ref _asyncLoadTreeNodes, value);
    }

    private ITreeItemNodeLoader? _asyncLoadTreeNodeLoader;
    public ITreeItemNodeLoader? AsyncLoadTreeNodeLoader
    {
        get => _asyncLoadTreeNodeLoader;
        set => this.RaiseAndSetIfChanged(ref _asyncLoadTreeNodeLoader, value);
    }

    private IList<ITreeItemNode>? _filterTreeNodes;
    public IList<ITreeItemNode>? FilterTreeNodes
    {
        get => _filterTreeNodes;
        set => this.RaiseAndSetIfChanged(ref _filterTreeNodes, value);
    }

    private bool _showLineSwitchChecked = true;
    public bool ShowLineSwitchChecked
    {
        get => _showLineSwitchChecked;
        set => this.RaiseAndSetIfChanged(ref _showLineSwitchChecked, value);
    }

    private bool _showIconSwitchChecked;
    public bool ShowIconSwitchChecked
    {
        get => _showIconSwitchChecked;
        set => this.RaiseAndSetIfChanged(ref _showIconSwitchChecked, value);
    }

    private bool _showLeafIconSwitchChecked;
    public bool ShowLeafIconSwitchChecked
    {
        get => _showLeafIconSwitchChecked;
        set => this.RaiseAndSetIfChanged(ref _showLeafIconSwitchChecked, value);
    }

    private bool _isContextMenuSelectOnRightClick = true;
    public bool IsContextMenuSelectOnRightClick
    {
        get => _isContextMenuSelectOnRightClick;
        set => this.RaiseAndSetIfChanged(ref _isContextMenuSelectOnRightClick, value);
    }

    private TreeItemHoverMode _treeViewNodeHoverMode = TreeItemHoverMode.Default;
    public TreeItemHoverMode TreeViewNodeHoverMode
    {
        get => _treeViewNodeHoverMode;
        set => this.RaiseAndSetIfChanged(ref _treeViewNodeHoverMode, value);
    }

    public TreeViewViewModel(IScreen screen)
    {
        HostScreen = screen;
    }

    public void SelectFirstBindingTreeNode()
    {
        if (!TryGetSelectionBindingNodes(out var firstChild, out _))
        {
            return;
        }

        BoundSelectedTreeNode = firstChild;
    }

    public void SelectSecondBindingTreeNode()
    {
        if (!TryGetSelectionBindingNodes(out _, out var secondChild))
        {
            return;
        }

        BoundSelectedTreeNode = secondChild;
    }

    public void ClearBindingTreeNodeSelection()
    {
        BoundSelectedTreeNode = null;
    }

    public void SelectFirstBindingTreeNodes()
    {
        if (!TryGetSelectionBindingNodes(out var firstChild, out _))
        {
            return;
        }

        BoundSelectedTreeNodes = new ObservableCollection<ITreeItemNode> { firstChild };
    }

    public void SelectSecondBindingTreeNodes()
    {
        if (!TryGetSelectionBindingNodes(out _, out var secondChild))
        {
            return;
        }

        BoundSelectedTreeNodes = new ObservableCollection<ITreeItemNode> { secondChild };
    }

    public void SelectBothBindingTreeNodes()
    {
        if (!TryGetSelectionBindingNodes(out var firstChild, out var secondChild))
        {
            return;
        }

        BoundSelectedTreeNodes = new ObservableCollection<ITreeItemNode> { firstChild, secondChild };
    }

    public void ClearBindingTreeNodesSelection()
    {
        BoundSelectedTreeNodes = new ObservableCollection<ITreeItemNode>();
    }

    private static string Lang(TreeViewShowCaseLangResourceKind kind)
    {
        return TreeViewShowCase.Lang(kind, FallbackLang(kind));
    }

    private static string FallbackLang(TreeViewShowCaseLangResourceKind kind)
    {
        return kind switch
        {
            _                                                                    => kind.ToString()
        };
    }

    private bool TryGetSelectionBindingNodes(out ITreeItemNode firstChild, out ITreeItemNode secondChild)
    {
        firstChild  = null!;
        secondChild = null!;
        if (BasicTreeNodes is not { Count: > 0 } nodes ||
            nodes[0] is not TreeItemNode root ||
            root.Children.Count < 2)
        {
            return false;
        }

        firstChild  = root.Children[0];
        secondChild = root.Children[1];
        return true;
    }

    private static string FormatSelectedNodes(IList? nodes)
    {
        if (nodes is not { Count: > 0 })
        {
            return "-";
        }

        var labels = new List<string>(nodes.Count);
        foreach (var node in nodes)
        {
            labels.Add(FormatSelectedNode(node));
        }
        return string.Join(", ", labels);
    }

    private static string FormatSelectedNode(object? node)
    {
        return node switch
        {
            ITreeItemNode treeItemNode => treeItemNode.Header?.ToString() ?? "-",
            null                       => "-",
            _                          => node.ToString() ?? "-"
        };
    }

    private void HandleBoundSelectedTreeNodesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        this.RaisePropertyChanged(nameof(BoundSelectedTreeNodesText));
    }
}

public class TreeItemDataLoader : ITreeItemNodeLoader
{
    public async Task<TreeItemLoadResult> LoadAsync(ITreeItemNode targetTreeItemData, CancellationToken token)
    {
        var level = 0;
        ITreeNode<ITreeItemNode>? current = targetTreeItemData;
        while (current != null)
        {
            level++;
            current = current.ParentNode;
        }
        await Task.Delay(TimeSpan.FromMilliseconds(600), token);
        var children = new List<TreeItemNode>();
        if (level < 3)
        {
            children.AddRange([
                new TreeItemNode()
                {
                    ItemKey = $"{targetTreeItemData.ItemKey}-0",
                    Header  = TreeViewShowCaseLanguage.Get(TreeViewShowCaseLangResourceKind.P2HeaderChildNode,
                        "Child Node")
                },
                new TreeItemNode()
                {
                    ItemKey = $"{targetTreeItemData.ItemKey}-1",
                    Header  = TreeViewShowCaseLanguage.Get(TreeViewShowCaseLangResourceKind.P2HeaderChildNode,
                        "Child Node")
                }
            ]);
        }
        return new TreeItemLoadResult()
        {
            IsSuccess = true,
            Data = children
        };
    }
}
