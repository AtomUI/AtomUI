using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reactive;
using AtomUI;
using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.TreeSelect;

public class TreeSelectViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "TreeSelect";

    public IScreen HostScreen { get; }

    public string? UrlPathSegment => ID.ToString();

    private List<ITreeItemNode>? _basicTreeNodes = [];

    public List<ITreeItemNode>? BasicTreeNodes
    {
        get => _basicTreeNodes;
        set => this.RaiseAndSetIfChanged(ref _basicTreeNodes, value);
    }

    private IList<ITreeItemNode>? _selectedItems;

    public IList<ITreeItemNode>? SelectedItems
    {
        get => _selectedItems;
        set => this.RaiseAndSetIfChanged(ref _selectedItems, value);
    }

    private List<ITreeItemNode>? _multiSelectionTreeNodes = [];

    public List<ITreeItemNode>? MultiSelectionTreeNodes
    {
        get => _multiSelectionTreeNodes;
        set => this.RaiseAndSetIfChanged(ref _multiSelectionTreeNodes, value);
    }

    private List<ITreeItemNode>? _bindingSingleTreeNodes = [];

    public List<ITreeItemNode>? BindingSingleTreeNodes
    {
        get => _bindingSingleTreeNodes;
        set => this.RaiseAndSetIfChanged(ref _bindingSingleTreeNodes, value);
    }

    private List<ITreeItemNode>? _semanticPreviewTreeNodes;

    public List<ITreeItemNode>? SemanticPreviewTreeNodes
    {
        get => _semanticPreviewTreeNodes;
        set => this.RaiseAndSetIfChanged(ref _semanticPreviewTreeNodes, value);
    }

    private IList<ITreeItemNode>? _semanticPreviewSelectedItems;

    public IList<ITreeItemNode>? SemanticPreviewSelectedItems
    {
        get => _semanticPreviewSelectedItems;
        set => this.RaiseAndSetIfChanged(ref _semanticPreviewSelectedItems, value);
    }

    private List<ITreeItemNode>? _styleClassTreeNodes;

    public List<ITreeItemNode>? StyleClassTreeNodes
    {
        get => _styleClassTreeNodes;
        set => this.RaiseAndSetIfChanged(ref _styleClassTreeNodes, value);
    }

    private List<ITreeItemNode>? _bindingMultipleTreeNodes = [];

    public List<ITreeItemNode>? BindingMultipleTreeNodes
    {
        get => _bindingMultipleTreeNodes;
        set => this.RaiseAndSetIfChanged(ref _bindingMultipleTreeNodes, value);
    }

    private ITreeItemNode? _boundSelectedItem;

    public ITreeItemNode? BoundSelectedItem
    {
        get => _boundSelectedItem;
        set
        {
            this.RaiseAndSetIfChanged(ref _boundSelectedItem, value);
            this.RaisePropertyChanged(nameof(BoundSelectedItemText));
        }
    }

    private IList<ITreeItemNode>? _boundSelectedItems;
    private INotifyCollectionChanged? _boundSelectedItemsCollectionChangedSource;

    public IList<ITreeItemNode>? BoundSelectedItems
    {
        get => _boundSelectedItems;
        set
        {
            if (ReferenceEquals(_boundSelectedItems, value))
            {
                this.RaisePropertyChanged(nameof(BoundSelectedItemsText));
                return;
            }

            if (_boundSelectedItemsCollectionChangedSource != null)
            {
                _boundSelectedItemsCollectionChangedSource.CollectionChanged -= HandleBoundSelectedItemsCollectionChanged;
            }

            this.RaiseAndSetIfChanged(ref _boundSelectedItems, value);

            _boundSelectedItemsCollectionChangedSource = value as INotifyCollectionChanged;
            if (_boundSelectedItemsCollectionChangedSource != null)
            {
                _boundSelectedItemsCollectionChangedSource.CollectionChanged += HandleBoundSelectedItemsCollectionChanged;
            }
            this.RaisePropertyChanged(nameof(BoundSelectedItemsText));
        }
    }

    public string BoundSelectedItemText => BoundSelectedItem?.Header?.ToString() ?? "-";

    public string BoundSelectedItemsText => BoundSelectedItems is { Count: > 0 }
        ? string.Join(", ", BoundSelectedItems.Select(item => item.Header?.ToString()))
        : "-";

    private List<ITreeItemNode>? _itemsSourceTreeNodes = [];

    public List<ITreeItemNode>? ItemsSourceTreeNodes
    {
        get => _itemsSourceTreeNodes;
        set => this.RaiseAndSetIfChanged(ref _itemsSourceTreeNodes, value);
    }

    private List<ITreeItemNode>? _checkableTreeNodes = [];

    public List<ITreeItemNode>? CheckableTreeNodes
    {
        get => _checkableTreeNodes;
        set => this.RaiseAndSetIfChanged(ref _checkableTreeNodes, value);
    }

    private List<ITreeItemNode>? _asyncLoadTreeNodes = [];

    public List<ITreeItemNode>? AsyncLoadTreeNodes
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

    private List<ITreeItemNode>? _showTreeLineTreeNodes = [];

    public List<ITreeItemNode>? ShowTreeLineTreeNodes
    {
        get => _showTreeLineTreeNodes;
        set => this.RaiseAndSetIfChanged(ref _showTreeLineTreeNodes, value);
    }

    private List<ITreeItemNode>? _contentLeftAddTreeNodes = [];

    public List<ITreeItemNode>? ContentLeftAddTreeNodes
    {
        get => _contentLeftAddTreeNodes;
        set => this.RaiseAndSetIfChanged(ref _contentLeftAddTreeNodes, value);
    }

    private List<ITreeItemNode>? _placementTreeNodes = [];

    public List<ITreeItemNode>? PlacementTreeNodes
    {
        get => _placementTreeNodes;
        set => this.RaiseAndSetIfChanged(ref _placementTreeNodes, value);
    }

    private SelectPopupPlacement _placement;

    public SelectPopupPlacement Placement
    {
        get => _placement;
        set => this.RaiseAndSetIfChanged(ref _placement, value);
    }

    private List<ITreeItemNode>? _maxSelectedTreeNodes = [];

    public List<ITreeItemNode>? MaxSelectedTreeNodes
    {
        get => _maxSelectedTreeNodes;
        set => this.RaiseAndSetIfChanged(ref _maxSelectedTreeNodes, value);
    }

    private List<ITreeItemNode>? _maxCheckedTreeNodes = [];

    public List<ITreeItemNode>? MaxCheckedTreeNodes
    {
        get => _maxCheckedTreeNodes;
        set => this.RaiseAndSetIfChanged(ref _maxCheckedTreeNodes, value);
    }

    private List<ITreeItemNode>? _sizeTypeTreeNodes = [];

    public List<ITreeItemNode>? SizeTypeTreeNodes
    {
        get => _sizeTypeTreeNodes;
        set => this.RaiseAndSetIfChanged(ref _sizeTypeTreeNodes, value);
    }

    private CustomizableSizeType _treeSelectSizeType = CustomizableSizeType.Middle;

    public CustomizableSizeType TreeSelectSizeType
    {
        get => _treeSelectSizeType;
        set => this.RaiseAndSetIfChanged(ref _treeSelectSizeType, value);
    }

    private bool _isShowTreeSelectIcon;

    public bool IsShowTreeSelectIcon
    {
        get => _isShowTreeSelectIcon;
        set => this.RaiseAndSetIfChanged(ref _isShowTreeSelectIcon, value);
    }

    private bool _isShowTreeSelectLeafIcon;

    public bool IsShowTreeSelectLeafIcon
    {
        get => _isShowTreeSelectLeafIcon;
        set => this.RaiseAndSetIfChanged(ref _isShowTreeSelectLeafIcon, value);
    }

    private bool _isShowTreeSelectLine = true;

    public bool IsShowTreeSelectLine
    {
        get => _isShowTreeSelectLine;
        set => this.RaiseAndSetIfChanged(ref _isShowTreeSelectLine, value);
    }

    public TreeSelectViewModel(IScreen screen)
    {
        HostScreen = screen;
        SetBoundSelectedItemCommand   = ReactiveCommand.Create(SetBoundSelectedItem);
        ClearBoundSelectedItemCommand = ReactiveCommand.Create(ClearBoundSelectedItem);
        SetBoundSelectedItemsCommand  = ReactiveCommand.Create(SetBoundSelectedItems);
        ClearBoundSelectedItemsCommand = ReactiveCommand.Create(ClearBoundSelectedItems);
    }

    public ReactiveCommand<Unit, Unit> SetBoundSelectedItemCommand { get; }

    public ReactiveCommand<Unit, Unit> ClearBoundSelectedItemCommand { get; }

    public ReactiveCommand<Unit, Unit> SetBoundSelectedItemsCommand { get; }

    public ReactiveCommand<Unit, Unit> ClearBoundSelectedItemsCommand { get; }

    private void SetBoundSelectedItem()
    {
        BoundSelectedItem = FindTreeItem(BindingSingleTreeNodes, "leaf2");
    }

    private void ClearBoundSelectedItem()
    {
        BoundSelectedItem = null;
    }

    private void SetBoundSelectedItems()
    {
        var firstItem  = FindTreeItem(BindingMultipleTreeNodes, "leaf1");
        var secondItem = FindTreeItem(BindingMultipleTreeNodes, "sss");
        var selectedItems = new[] { firstItem, secondItem }
            .OfType<ITreeItemNode>()
            .ToList();

        if (BoundSelectedItems is ObservableCollection<ITreeItemNode> collection)
        {
            collection.Clear();
            foreach (var item in selectedItems)
            {
                collection.Add(item);
            }
        }
        else
        {
            BoundSelectedItems = new ObservableCollection<ITreeItemNode>(selectedItems);
        }
    }

    private void ClearBoundSelectedItems()
    {
        if (BoundSelectedItems is ObservableCollection<ITreeItemNode> collection)
        {
            collection.Clear();
        }
        else
        {
            BoundSelectedItems = new ObservableCollection<ITreeItemNode>();
        }
    }

    private void HandleBoundSelectedItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        this.RaisePropertyChanged(nameof(BoundSelectedItemsText));
    }

    private static ITreeItemNode? FindTreeItem(IEnumerable<ITreeItemNode>? items, string value)
    {
        if (items == null)
        {
            return null;
        }

        foreach (var item in items)
        {
            if (item.Value?.ToString() == value || item.ItemKey?.ToString() == value)
            {
                return item;
            }

            var child = FindTreeItem(item.Children, value);
            if (child != null)
            {
                return child;
            }
        }
        return null;
    }
}
