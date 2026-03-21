using System.Collections;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

public class TransferTreeView : TreeView, ITransferTreeView, ITransferDecoratorProvider
{
    #region 公共属性定义
    public static readonly DirectProperty<TransferTreeView, IList<EntityKey>?> SelectedKeysProperty =
        AvaloniaProperty.RegisterDirect<TransferTreeView, IList<EntityKey>?>(nameof(SelectedKeys), 
            o => o.SelectedKeys,
            (o, v) => o.SelectedKeys = v);
    
    public static readonly DirectProperty<TransferTreeView, ISet<EntityKey>?> MaskKeysProperty =
        AvaloniaProperty.RegisterDirect<TransferTreeView, ISet<EntityKey>?>(nameof(MaskKeys), 
            o => o.MaskKeys,
            (o, v) => o.MaskKeys = v);
    
    public static readonly DirectProperty<TransferTreeView, TransferViewType> ViewTypeProperty =
        AvaloniaProperty.RegisterDirect<TransferTreeView, TransferViewType>(nameof(ViewType), 
            o => o.ViewType,
            (o, v) => o.ViewType = v);
    
    private IList<EntityKey>? _selectedKeys;
    public IList<EntityKey>? SelectedKeys
    {
        get => _selectedKeys;
        set => SetAndRaise(SelectedKeysProperty, ref _selectedKeys, value);
    }
    
    private ISet<EntityKey>? _maskKeys;
    public ISet<EntityKey>? MaskKeys
    {
        get => _maskKeys;
        set => SetAndRaise(MaskKeysProperty, ref _maskKeys, value);
    }
    
    private TransferViewType _viewType;
    public TransferViewType ViewType
    {
        get => _viewType;
        set => SetAndRaise(ViewTypeProperty, ref _viewType, value);
    }
    
    public bool IsSupportItemTemplate => true;
    public bool IsSupportPagination => false;
    #endregion
    
    #region 公共事件定义

    public event EventHandler<TransferItemsRemovedEventArgs>? ItemsRemoved;
    public event EventHandler? SelectedKeyChanged;
    public event EventHandler<ItemCountChangedEventArgs>? ItemCountChanged;
    public event EventHandler<SelectionCountChangedEventArgs>? SelectionCountChanged;

    #endregion
    
    private bool _ignoreSyncSelection;
    
    static TransferTreeView()
    {
        SelectedKeysProperty.Changed.AddClassHandler<TransferTreeView>((view, args) => view.HandleSelectedKeysChanged());
        TransferTreeViewItem.ClickEvent.AddClassHandler<TransferTreeView>((view, args) => view.HandleItemClicked(args));
    }

    public TransferTreeView()
    {
        CheckedItemsChanged += HandleCheckedItemsChanged;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == MaskKeysProperty ||
            change.Property == ItemsSourceProperty)
        {
            MaskNodes();
            HandleItemCountChanged();
        }

        if (change.Property == MaskKeysProperty)
        {
            ConfigureSelectedKeys();
        }
    }

    private void HandleItemCountChanged()
    {
        var nodes      = Items.Cast<ITreeItemNode>().ToList();
        var totalCount = 0;
        foreach (var node in nodes)
        {
            totalCount += CalculateAllNodesCount(node);
        }
        ItemCountChanged?.Invoke(this, new ItemCountChangedEventArgs(totalCount));
    }

    private void HandleSelectedKeysChanged()
    {
        SelectedKeyChanged?.Invoke(this, EventArgs.Empty);
        if (_ignoreSyncSelection)
        {
            _ignoreSyncSelection = false;
            return;
        }
        var checkedItems = ItemsSource?.Cast<ITreeItemNode>().Where(item => SelectedKeys?.Contains(item.ItemKey ?? default) ?? false).ToList();
        CheckedItems.Clear();
        if (checkedItems != null)
        {
            foreach (var item in checkedItems)
            {
                CheckedItems.Add(item);
            }
        }
    }
    
    private void HandleCheckedItemsChanged(object? sender, EventArgs e)
    {
        ConfigureSelectedKeys();
    }

    private void ConfigureSelectedKeys()
    {
        _ignoreSyncSelection = true;
    
        if (CheckedItems.Count == 0)
        {
            SetCurrentValue(SelectedKeysProperty, null);
        }
        else
        {
            var selectedKeys = new List<EntityKey>();
            foreach (var item in CheckedItems)
            {
                if (item is ITreeItemNode treeItemNode && treeItemNode.IsEnabled)
                {
                    var itemKey = treeItemNode.ItemKey ?? default;
                    if (MaskKeys == null || MaskKeys?.Contains(itemKey) == false)
                    {
                        selectedKeys.Add(treeItemNode.ItemKey ?? default); 
                    }
                }
            }
            SetCurrentValue(SelectedKeysProperty, selectedKeys);
        }
        SelectionCountChanged?.Invoke(this, new SelectionCountChangedEventArgs(SelectedKeys?.Count ?? 0));
    }

    private int CalculateAllNodesCount(ITreeItemNode treeNode)
    {
        var count = 0;
        if (MaskKeys == null || MaskKeys?.Contains(treeNode.ItemKey ?? default) == false)
        {
            count += 1;
        }
        foreach (var child in treeNode.Children)
        {
            count += CalculateAllNodesCount(child);
        }
        return count;
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new TransferTreeViewItem();
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<TransferTreeViewItem>(item, out recycleKey);
    }
    
    protected override bool RecursiveCheckNodePredicate(TreeViewItem treeViewItem)
    {
        if (treeViewItem is TransferTreeViewItem transferTreeViewItem)
        {
            return !transferTreeViewItem.IsMasked;
        }
        return true;
    }
    
    protected override bool RecursiveUnCheckNodePredicate(TreeViewItem treeViewItem)
    {
        if (treeViewItem is TransferTreeViewItem transferTreeViewItem)
        {
            return !transferTreeViewItem.IsMasked;
        }
        return true;
    }
    
    public new void SelectAll()
    {
        for (var i = 0; i < ItemCount; i++)
        {
            if (ContainerFromIndex(i) is TreeViewItem treeItem)
            {
                CheckedSubTree(treeItem);
            }
        }
    }

    public void DeselectAll()
    {
        for (var i = 0; i < ItemCount; i++)
        {
            if (ContainerFromIndex(i) is TreeViewItem treeItem)
            {
                UnCheckedSubTree(treeItem);
            }
        }
    }

    private void MaskNodes()
    {
        var nodes = Items.Cast<ITreeItemNode>().ToList();
        foreach (var node in nodes)
        {
            MaskNodeRecursively(node);
        }
    }

    private void MaskNodeRecursively(ITreeItemNode item)
    {
        var container = TreeContainerFromItem(item);
        if (container is TransferTreeViewItem treeItem)
        {
            if (MaskKeys?.Contains(item.ItemKey ?? default) == true)
            {
                treeItem.IsMasked = true;
            }
            else
            {
                treeItem.IsMasked = false;
            }
        }
        foreach (var child in item.Children)
        {
            MaskNodeRecursively(child);
        }
    }

    void ITransferView.SetItemsSource(IEnumerable? itemsSource)
    {
        SetCurrentValue(ItemsSourceProperty, itemsSource);
    }
    
    void ITransferView.NotifyAboutToTransfer(TransferDirection transferDirection)
    {
    }
    
    void ITransferView.NotifyTransferCompleted(TransferDirection transferDirection)
    {
    }

    void ITransferView.SetSelectionEnabled(bool enabled)
    {
    }

    void ITransferView.SetPageSize(int pageSize)
    {
    }

    void ITransferView.SetItemTemplate(IDataTemplate? itemTemplate)
    {
        if (itemTemplate is ITreeDataTemplate treeDataTemplate)
        {
            SetCurrentValue(ItemTemplateProperty, treeDataTemplate);
        }
        else
        {
            SetCurrentValue(ItemTemplateProperty, itemTemplate);
        }
    }
    
    void ITransferView.NotifySelectAction(TransferSelectAction selectAction)
    {
    }

    void ITransferTreeView.SetMaskedItems(IList<EntityKey>? maskedItems)
    {
        SetCurrentValue(MaskKeysProperty, maskedItems?.ToHashSet());
    }
    
    void ITransferDecoratorProvider.ProvideTransferDecorator(TransferItemDecorator decorator)
    {
        decorator.IsShowSelectDropdownMenu = false;
    }

    private void HandleItemClicked(RoutedEventArgs e)
    {
        if (e.Source is TransferTreeViewItem treeViewItem)
        {
            if (!treeViewItem.IsMasked)
            {
                var item = TreeItemFromContainer(treeViewItem);
                if (treeViewItem.IsChecked != true)
                {
                    CheckedItems.Add(item);
                }
                else
                {
                    CheckedItems.Remove(item);
                }
            }
        }
    }
}