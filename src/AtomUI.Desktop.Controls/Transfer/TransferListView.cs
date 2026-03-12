using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace AtomUI.Desktop.Controls;

public class TransferListView : List,
                                ITransferView
{
    #region 公共属性定义

    public static readonly DirectProperty<TransferListView, IList<EntityKey>?> SelectedKeysProperty =
        AvaloniaProperty.RegisterDirect<TransferListView, IList<EntityKey>?>(nameof(SelectedKeys), 
            o => o.SelectedKeys,
            (o, v) => o.SelectedKeys = v);
    
    private IList<EntityKey>? _selectedKeys;

    public IList<EntityKey>? SelectedKeys
    {
        get => _selectedKeys;
        set => SetAndRaise(SelectedKeysProperty, ref _selectedKeys, value);
    }
    #endregion

    #region 公共事件定义

    public event EventHandler<TransferItemDeletedEventArgs>? ItemDeleted;
    public event EventHandler? SelectedKeyChanged;

    #endregion
    
    private bool _ignoreSyncSelection;
    
    private static readonly FuncTemplate<Panel?> DefaultPanel =
        new(() => new StackPanel());

    static TransferListView()
    {
        SelectionModeProperty.OverrideDefaultValue<TransferListView>(SelectionMode.Multiple | SelectionMode.Toggle);
        ItemsPanelProperty.OverrideDefaultValue<TransferListView>(DefaultPanel);
        SelectedKeysProperty.Changed.AddClassHandler<TransferListView>((view, args) => view.HandleSelectedKeysChanged());
        SelectionChangedEvent.AddClassHandler<TransferListView>((view, args) => view.HandleSelectionChanged(args));
    }
    
    protected internal override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new TransferListItem();
    }

    protected internal override bool? NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<TransferListItem>(item, out recycleKey);
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
                if (item is IItemKey itemKey)
                {
                    selectedKeys.Add(itemKey.ItemKey ?? default);
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
            Selection?.Clear();
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

    public void SelectAll()
    {
        if (Selection != null)
        {
            Selection.SelectAll();
        }
    }

    public void DeselectAll()
    {
        if (Selection != null)
        {
            Selection.Clear();
        }
    }
}