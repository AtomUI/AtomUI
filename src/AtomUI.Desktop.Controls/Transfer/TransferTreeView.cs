using System.Collections;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls.Templates;
using Avalonia.Threading;

namespace AtomUI.Desktop.Controls;

public class TransferTreeView : TreeView, 
                                ITransferTreeView,
                                ITransferDecoratorProvider
{
    #region 公共属性定义
    public static readonly DirectProperty<TransferTreeView, IList<EntityKey>?> SelectedKeysProperty =
        AvaloniaProperty.RegisterDirect<TransferTreeView, IList<EntityKey>?>(nameof(SelectedKeys), 
            o => o.SelectedKeys,
            (o, v) => o.SelectedKeys = v);
    
    public static readonly DirectProperty<TransferTreeView, IList<EntityKey>?> MaskKeysProperty =
        AvaloniaProperty.RegisterDirect<TransferTreeView, IList<EntityKey>?>(nameof(MaskKeys), 
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
    
    private IList<EntityKey>? _maskKeys;
    public IList<EntityKey>? MaskKeys
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

    public event EventHandler<TransferItemRemovedEventArgs>? ItemRemoved;
    public event EventHandler? SelectedKeyChanged;
    public event EventHandler<ItemCountChangedEventArgs>? ItemCountChanged;

    #endregion
    
    private bool _ignoreSyncSelection;
    
    static TransferTreeView()
    {
        SelectedKeysProperty.Changed.AddClassHandler<TransferTreeView>((view, args) => view.HandleSelectedKeysChanged());
        ItemCountProperty.Changed.AddClassHandler<TransferTreeView>((view, args) => view.HandleItemCountChanged());
    }

    public TransferTreeView()
    {
        CheckedItemsChanged += HandleCheckedItemsChanged;
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
                    selectedKeys.Add(treeItemNode.ItemKey ?? default);
                }
            }
            SetCurrentValue(SelectedKeysProperty, selectedKeys);
        }
    }

    private int CalculateAllNodesCount(ITreeItemNode node)
    {
        var count = 0;
        foreach (var child in node.Children)
        {
            count += CalculateAllNodesCount(child);
        }
        return count + 1;
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
    
    public void NotifySelectAction(TransferSelectAction selectAction)
    {
    }
    
    void ITransferDecoratorProvider.ProvideTransferDecorator(TransferItemDecorator decorator)
    {
        decorator.IsShowSelectDropdownMenu = false;
    }
}