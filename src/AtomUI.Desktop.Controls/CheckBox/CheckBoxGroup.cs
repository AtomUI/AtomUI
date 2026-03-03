using System.Collections;
using System.Diagnostics;
using AtomUI.Controls;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Metadata;

using ItemCollection = AtomUI.Collections.ItemCollection;

namespace AtomUI.Desktop.Controls;

public class CheckBoxGroup: TemplatedControl,
                            IMotionAwareControl,
                            IControlSharedTokenResourcesHost,
                            IFormItemAware
{
    #region 公共属性定义
    
    public static readonly StyledProperty<double> ItemSpacingProperty = 
        AvaloniaProperty.Register<CheckBoxGroup, double>(nameof (ItemSpacing));
    
    public static readonly StyledProperty<Orientation> OrientationProperty = 
        StackPanel.OrientationProperty.AddOwner<CheckBoxGroup>();

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<CheckBoxGroup>();
    
    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        ItemsControl.ItemsSourceProperty.AddOwner<CheckBoxGroup>();
    
    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        ItemsControl.ItemTemplateProperty.AddOwner<CheckBoxGroup>();
    
    public static readonly DirectProperty<CheckBoxGroup, IList?> CheckedItemsProperty =
        AvaloniaProperty.RegisterDirect<CheckBoxGroup, IList?>(
            nameof(CheckedItems),
            o => o.CheckedItems,
            (o, v) => o.CheckedItems = v);
    
    public double ItemSpacing
    {
        get => GetValue(ItemSpacingProperty);
        set => SetValue(ItemSpacingProperty, value);
    }
    
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    [InheritDataTypeFromItems(nameof(ItemsSource))]
    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }
    
    private IList? _checkedItems;

    public IList? CheckedItems
    {
        get => _checkedItems;
        set => SetAndRaise(CheckedItemsProperty, ref _checkedItems, value);
    }
    
    [Content]
    public ItemCollection Items => _items;
    #endregion

    #region 公共事件定义

    public static readonly RoutedEvent<CheckBoxGroupCheckedChangedEventArgs> CheckedChangedEvent =
        RoutedEvent.Register<CheckBoxGroup, CheckBoxGroupCheckedChangedEventArgs>(
            nameof(CheckedChanged),
            RoutingStrategies.Bubble);

    public event EventHandler<CheckBoxGroupCheckedChangedEventArgs>? CheckedChanged
    {
        add => AddHandler(CheckedChangedEvent, value);
        remove => RemoveHandler(CheckedChangedEvent, value);
    }
    #endregion
    
    #region 内部属性定义
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => CheckBoxToken.ID;
    
    #endregion
    
    private readonly ItemCollection _items = new();
    private CheckBoxItemsControl? _itemsControl;
    private bool _ignoreSyncToItemsControl;
    
    static CheckBoxGroup()
    {
        OrientationProperty.OverrideDefaultValue<CheckBoxGroup>(Orientation.Horizontal);
        ItemsSourceProperty.Changed.AddClassHandler<CheckBoxGroup>((group, args) => group.HandleItemsSourceChanged(args));
    }
    
    public CheckBoxGroup()
    {
        this.RegisterResources();
    }
    
    private void HandleItemsSourceChanged(AvaloniaPropertyChangedEventArgs args)
    {
        _items.SetItemsSource(args.GetNewValue<IEnumerable?>());
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_itemsControl != null)
        {
            _itemsControl.SelectionChanged -= HandleItemsSelectedChanged;
        }
        _itemsControl = e.NameScope.Find<CheckBoxItemsControl>(CheckBoxGroupThemeConstants.CheckBoxItemsPart);
        if (_itemsControl != null)
        {
            _itemsControl.ItemsSource      =  _items;
            _itemsControl.SelectionChanged += HandleItemsSelectedChanged;
            _itemsControl.CheckedItems     =  CheckedItems;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == CheckedItemsProperty)
        {
            HandleCheckedItemsChanged(change.OldValue as IList, change.NewValue as IList);
        }
    }

    private void HandleItemsSelectedChanged(object? sender, SelectionChangedEventArgs change)
    {
        if (CheckedItems != null)
        {
            foreach (var item in change.RemovedItems)
            {
                CheckedItems.Remove(item);
            }

            foreach (var item in change.AddedItems)
            {
                CheckedItems.Add(item);
            }
        }
        else
        {
            IList? checkedItems = null;
            Debug.Assert(_itemsControl != null);
            if (_itemsControl.CheckedItems != null)
            {
                checkedItems = new AvaloniaList<object>();
                foreach (var item in _itemsControl.CheckedItems)
                {
                    checkedItems.Add(item);
                }
            }

            _ignoreSyncToItemsControl = true;
            CheckedItems              = checkedItems;
        }
        RaiseEvent(new CheckBoxGroupCheckedChangedEventArgs(CheckedChangedEvent, change.RemovedItems, change.AddedItems));
        _formValueChanged?.Invoke(this, EventArgs.Empty);
    }

    private void HandleCheckedItemsChanged(IList? oldValue, IList? newValue)
    {
        if (_ignoreSyncToItemsControl)
        {
            _ignoreSyncToItemsControl = false;
            return;
        }
        if (_itemsControl != null)
        {
            _itemsControl.CheckedItems = newValue;
        }
    }
    
    #region 实现 FormItem 接口
    private EventHandler? _formValueChanged;
    event EventHandler? IFormItemAware.ValueChanged
    {
        add => _formValueChanged += value;
        remove => _formValueChanged -= value;
    }

    void IFormItemAware.SetFormValue(object? value) => NotifySetFormValue((IList?)value);

    object? IFormItemAware.GetFormValue() => NotifyGetFormValue();
    void IFormItemAware.ClearFormValue() => NotifyClearFormValue();
    void IFormItemAware.NotifyValidateStatus(FormValidateStatus status) => NotifyValidateStatus(status);
    
    protected virtual void NotifySetFormValue(IList? value)
    {
        SetCurrentValue(CheckedItemsProperty, value);
    }

    protected virtual IList? NotifyGetFormValue()
    {
        return CheckedItems;
    }

    protected virtual void NotifyClearFormValue()
    {
        SetCurrentValue(CheckedItemsProperty, null);
    }
    
    protected virtual void NotifyValidateStatus(FormValidateStatus status)
    {
    }
    #endregion
}