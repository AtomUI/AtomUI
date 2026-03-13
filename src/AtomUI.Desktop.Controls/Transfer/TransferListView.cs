using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Desktop.Controls.Primitives;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

public class TransferListView : ListBox, ITransferView
{
    #region 公共属性定义

    public static readonly DirectProperty<TransferListView, IList<EntityKey>?> SelectedKeysProperty =
        AvaloniaProperty.RegisterDirect<TransferListView, IList<EntityKey>?>(nameof(SelectedKeys), 
            o => o.SelectedKeys,
            (o, v) => o.SelectedKeys = v);
    
    public static readonly DirectProperty<TransferListView, TransferViewType> ViewTypeProperty =
        AvaloniaProperty.RegisterDirect<TransferListView, TransferViewType>(nameof(ViewType), 
            o => o.ViewType,
            (o, v) => o.ViewType = v);

    private IList<EntityKey>? _selectedKeys;
    public IList<EntityKey>? SelectedKeys
    {
        get => _selectedKeys;
        set => SetAndRaise(SelectedKeysProperty, ref _selectedKeys, value);
    }
    
    private TransferViewType _viewType;
    public TransferViewType ViewType
    {
        get => _viewType;
        set => SetAndRaise(ViewTypeProperty, ref _viewType, value);
    }

    public bool IsSupportItemTemplate => true;
    #endregion

    #region 公共事件定义

    public event EventHandler<TransferItemRemovedEventArgs>? ItemRemoved;
    public event EventHandler? SelectedKeyChanged;

    #endregion
    
    private bool _ignoreSyncSelection;
    private IList<EntityKey>? _selectedKeysBackup;
    
    private static readonly FuncTemplate<Panel?> DefaultPanel =
        new(() => new StackPanel());

    static TransferListView()
    {
        SelectionModeProperty.OverrideDefaultValue<TransferListView>(SelectionMode.Multiple | SelectionMode.Toggle);
        ItemsPanelProperty.OverrideDefaultValue<TransferListView>(DefaultPanel);
        SelectedKeysProperty.Changed.AddClassHandler<TransferListView>((view, args) => view.HandleSelectedKeysChanged());
        SelectionChangedEvent.AddClassHandler<TransferListView>((view, args) => view.HandleSelectionChanged(args));
        TransferRemoveItemButton.ClickEvent.AddClassHandler<TransferListView>((view, args) => view.HandleRemoveButtonClicked(args));
        AutoScrollToSelectedItemProperty.OverrideDefaultValue<TransferListView>(false);
    }
    
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new TransferListItem();
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<TransferListItem>(item, out recycleKey);
    }
    
    protected override void PrepareListBoxItem(ListBoxItem listItem, object? item, int index,
                                               CompositeDisposable disposables)
    {
        base.PrepareListBoxItem(listItem, item, index, disposables);
        if (listItem is TransferListItem transferListItem)
        {
            disposables.Add(BindUtils.RelayBind(this, IsItemSelectableProperty, transferListItem, TransferListItem.IsCheckableProperty));
        }
    }

    private void HandleSelectionChanged(SelectionChangedEventArgs e)
    {
        _ignoreSyncSelection = true;
        if (SelectedItems == null || SelectedItems.Count == 0)
        {
            SetCurrentValue(SelectedKeysProperty, null);
        }
        else
        {
            var selectedKeys = new List<EntityKey>();
            foreach (var item in SelectedItems)
            {
                if (ContainerFromItem(item) is TransferListItem container)
                {
                    if (!container.IsVisible || container.Classes.Contains(StdPseudoClass.Disabled))
                    {
                        this.MarkContainerSelected(container, false);
                        continue;
                    }
                    if (item is IItemKey itemKey)
                    {
                        selectedKeys.Add(itemKey.ItemKey ?? default);
                    }
                }
            }
            SetCurrentValue(SelectedKeysProperty, selectedKeys);
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ItemsSourceProperty)
        {
            Selection.Clear();
        }
    }

    private void HandleSelectedKeysChanged()
    {
        SelectedKeyChanged?.Invoke(this, EventArgs.Empty);
        if (_ignoreSyncSelection)
        {
            _ignoreSyncSelection = false;
            return;
        }
        var selectedItems = ItemsSource?.Cast<IItemKey>().Where(item => SelectedKeys?.Contains(item.ItemKey ?? default) ?? false).ToList();
        SetCurrentValue(SelectedItemsProperty, selectedItems);
    }
    
    public void DeselectAll()
    {
        Selection.Clear();
    }

    void ITransferView.NotifyAboutToTransfer(TransferDirection transferDirection)
    {
        _selectedKeysBackup = SelectedKeys;
    }

    void ITransferView.NotifyTransferCompleted(TransferDirection transferDirection)
    {
        if (ViewType == TransferViewType.Source && transferDirection == TransferDirection.ToSource)
        {
            SetCurrentValue(SelectedKeysProperty, _selectedKeysBackup);
        }
        else if (ViewType == TransferViewType.Target && transferDirection == TransferDirection.ToTarget)
        {
            SetCurrentValue(SelectedKeysProperty, _selectedKeysBackup);
        }
        _selectedKeysBackup = null;
    }

    void ITransferView.SetSelectionEnabled(bool enabled)
    {
        SetCurrentValue(IsItemSelectableProperty, enabled);
        if (enabled)
        {
            SetCurrentValue(SelectionModeProperty, SelectionMode.Multiple | SelectionMode.Toggle);
        }
    }

    private void HandleRemoveButtonClicked(RoutedEventArgs e)
    {
        if (e.Source is TransferRemoveItemButton && GetContainerFromEventSource(e.Source) is TransferListItem listItem)
        {
            if (listItem.DataContext is IItemKey itemKey)
            {
                ItemRemoved?.Invoke(this, new TransferItemRemovedEventArgs(itemKey));
            }
        }
    }
}