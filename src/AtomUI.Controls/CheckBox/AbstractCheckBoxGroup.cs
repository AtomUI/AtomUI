using System.Collections;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Metadata;
using Avalonia.VisualTree;

using ItemCollection = AtomUI.Collections.ItemCollection;

namespace AtomUI.Controls.Commons;

/// <summary>
/// Base class of controls that show a collection of items as check boxes and track the checked items in
/// <see cref="CheckedItems" />. Each item is hosted by a check box; an item that implements
/// <see cref="ICheckBoxOption" /> provides the label, the enabled state and the initial checked state of its
/// check box.
/// </summary>
[TemplatePart("PART_CheckBoxItems", typeof(SelectingItemsControl))]
public abstract class AbstractCheckBoxGroup: TemplatedControl,
                                             IMotionAwareControl,
                                             IFormItemAware
{
    #region 公共属性定义
    
    /// <summary>
    /// Identifies the <see cref="ItemSpacing" /> property.
    /// </summary>
    public static readonly StyledProperty<double> ItemSpacingProperty = 
        AvaloniaProperty.Register<AbstractCheckBoxGroup, double>(nameof (ItemSpacing));
    
    /// <summary>
    /// Identifies the <see cref="LineSpacing" /> property.
    /// </summary>
    public static readonly StyledProperty<double> LineSpacingProperty = 
        AvaloniaProperty.Register<AbstractCheckBoxGroup, double>(nameof (LineSpacing));
    
    /// <summary>
    /// Identifies the <see cref="AbstractCheckBoxGroup.Orientation" /> property.
    /// </summary>
    public static readonly StyledProperty<Orientation> OrientationProperty = 
        StackPanel.OrientationProperty.AddOwner<AbstractCheckBoxGroup>();

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractCheckBoxGroup>();
    
    /// <summary>
    /// Identifies the <see cref="ItemsSource" /> property.
    /// </summary>
    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        ItemsControl.ItemsSourceProperty.AddOwner<AbstractCheckBoxGroup>();
    
    /// <summary>
    /// Identifies the <see cref="ItemTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        ItemsControl.ItemTemplateProperty.AddOwner<AbstractCheckBoxGroup>();
    
    /// <summary>
    /// Identifies the <see cref="CheckedItems" /> property.
    /// </summary>
    public static readonly DirectProperty<AbstractCheckBoxGroup, IList?> CheckedItemsProperty =
        AvaloniaProperty.RegisterDirect<AbstractCheckBoxGroup, IList?>(
            nameof(CheckedItems),
            o => o.CheckedItems,
            (o, v) => o.CheckedItems = v,
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);
    
    /// <summary>
    /// Gap between adjacent check boxes on the same line, in the direction chosen by
    /// <see cref="AbstractCheckBoxGroup.Orientation" />. When the value is not set, the theme supplies it from the
    /// <c>SpacingXS</c> token.
    /// </summary>
    public double ItemSpacing
    {
        get => GetValue(ItemSpacingProperty);
        set => SetValue(ItemSpacingProperty, value);
    }
    
    /// <summary>
    /// Gap between the lines that arise when a line of check boxes has no more space and wraps. When the value is
    /// not set, the theme supplies it from the <c>SpacingXS</c> token.
    /// </summary>
    public double LineSpacing
    {
        get => GetValue(LineSpacingProperty);
        set => SetValue(LineSpacingProperty, value);
    }
    
    /// <summary>
    /// Direction in which the check boxes are laid out: <c>Horizontal</c> (the default) lays them out in rows,
    /// <c>Vertical</c> in columns.
    /// </summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    /// <see langword="true" /> lets the check boxes animate their state changes. The value is passed to every check
    /// box of the group. When the value is not set, the theme supplies it from the <c>EnableMotion</c> token.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    /// <summary>
    /// Source of the items shown as check boxes, or <see langword="null" /> (the default) to show <see cref="Items" />.
    /// Setting it replaces the content of <see cref="Items" />: it throws an <see cref="InvalidOperationException" />
    /// while <see cref="Items" /> holds items, and clearing it afterwards leaves <see cref="Items" /> empty.
    /// </summary>
    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// Template that renders an item inside its check box. The default template of the theme shows the
    /// <see cref="ICheckBoxOption.Content" /> of an item; set a template to show items of another type.
    /// </summary>
    [InheritDataTypeFromItems(nameof(ItemsSource))]
    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }
    
    private IList? _checkedItems;

    /// <summary>
    /// The items that are checked. Setting a list checks these items and unchecks the others. When the value is
    /// <see langword="null" /> (the default), the checked state comes from the items: a check box is checked when
    /// it is prepared for an item that implements <see cref="ICheckBoxOption" /> with
    /// <see cref="ICheckBoxOption.IsChecked" /> set to <see langword="true" />. Binds two-way by default.
    /// </summary>
    /// <remarks>
    /// While the group is attached, it follows changes of the assigned collection when that collection implements
    /// <see cref="INotifyCollectionChanged" />. When the user toggles a check box, the group assigns a new list to
    /// this property instead of changing the assigned collection.
    /// </remarks>
    public IList? CheckedItems
    {
        get => _checkedItems;
        set => SetAndRaise(CheckedItemsProperty, ref _checkedItems, value);
    }
    
    /// <summary>
    /// The items to show, declared in XAML or added in code. Used only while <see cref="ItemsSource" /> is
    /// <see langword="null" />; changing the collection throws while <see cref="ItemsSource" /> supplies the items.
    /// </summary>
    [Content]
    public ItemCollection Items => _items;
    #endregion

    #region 公共事件定义

    /// <summary>
    /// Identifies the <see cref="CheckedChanged" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<CheckBoxGroupCheckedChangedEventArgs> CheckedChangedEvent =
        RoutedEvent.Register<AbstractCheckBoxGroup, CheckBoxGroupCheckedChangedEventArgs>(
            nameof(CheckedChanged),
            RoutingStrategies.Bubble);

    /// <summary>
    /// Raised after the set of checked items changed, whether by assigning <see cref="CheckedItems" />, by a change
    /// of the assigned collection or by the user toggling a check box. It is raised only when items were added or
    /// removed, after <see cref="CheckedItems" /> holds the new set.
    /// </summary>
    public event EventHandler<CheckBoxGroupCheckedChangedEventArgs>? CheckedChanged
    {
        add => AddHandler(CheckedChangedEvent, value);
        remove => RemoveHandler(CheckedChangedEvent, value);
    }
    #endregion
    
    private readonly ItemCollection _items = new();
    private AbstractCheckBoxItemsControl? _itemsControl;
    private INotifyCollectionChanged? _checkedItemsCollectionChangedSource;
    private IList? _checkedItemsSnapshot;
    
    static AbstractCheckBoxGroup()
    {
        OrientationProperty.OverrideDefaultValue<AbstractCheckBoxGroup>(Orientation.Horizontal);
        ItemsSourceProperty.Changed.AddClassHandler<AbstractCheckBoxGroup>((group, args) => group.HandleItemsSourceChanged(args));
    }
    
    private void HandleItemsSourceChanged(AvaloniaPropertyChangedEventArgs args)
    {
        _items.SetItemsSource(args.GetNewValue<IEnumerable?>());
    }

    /// <summary>
    /// Connects the <c>PART_CheckBoxItems</c> part: hands it <see cref="Items" />, follows its selection changes and
    /// pushes <see cref="CheckedItems" /> to it. An override must call the base method, otherwise no items are shown.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_itemsControl != null)
        {
            _itemsControl.SelectionChanged -= HandleItemsSelectedChanged;
        }
        _itemsControl = e.NameScope.Find<AbstractCheckBoxItemsControl>("PART_CheckBoxItems");
        if (_itemsControl != null)
        {
            _itemsControl.ItemsSource      =  _items;
            _itemsControl.SelectionChanged += HandleItemsSelectedChanged;
            SyncCheckedItemsToItemsControl();
        }
    }

    /// <summary>
    /// Starts following changes of the collection assigned to <see cref="CheckedItems" />. An override must call the
    /// base method to keep <see cref="CheckedChanged" /> in step with that collection.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ConfigureCheckedItemsCollectionChangedSource(CheckedItems);
    }

    /// <summary>
    /// Stops following changes of the collection assigned to <see cref="CheckedItems" />. An override must call the
    /// base method to release the subscription.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ReleaseCheckedItemsCollectionChangedSource();
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Pushes a new <see cref="CheckedItems" /> value to the check boxes and raises <see cref="CheckedChanged" /> for
    /// the difference. An override must call the base method, otherwise the check boxes stop following
    /// <see cref="CheckedItems" />.
    /// </summary>
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
        var checkedItems = _itemsControl?.BuildCurrentCheckedItems() ??
                           CopyCheckedItems(CheckedItems) ??
                           new AvaloniaList<object?>();
        foreach (var item in change.RemovedItems)
        {
            checkedItems.Remove(item);
        }

        foreach (var item in change.AddedItems)
        {
            if (!checkedItems.Contains(item))
            {
                checkedItems.Add(item);
            }
        }

        CheckedItems = checkedItems;
    }

    private void HandleCheckedItemsChanged(IList? oldValue, IList? newValue)
    {
        ConfigureCheckedItemsCollectionChangedSource(newValue);
        SyncCheckedItemsToItemsControl();
        _checkedItemsSnapshot = CopyCheckedItems(newValue);
        NotifyCheckedItemsChanged(oldValue, newValue);
        _formValueChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ConfigureCheckedItemsCollectionChangedSource(IList? checkedItems)
    {
        if (!this.IsAttachedToVisualTree())
        {
            ReleaseCheckedItemsCollectionChangedSource();
            _checkedItemsSnapshot = CopyCheckedItems(checkedItems);
            return;
        }

        if (ReferenceEquals(_checkedItemsCollectionChangedSource, checkedItems))
        {
            _checkedItemsSnapshot = CopyCheckedItems(checkedItems);
            return;
        }

        ReleaseCheckedItemsCollectionChangedSource();

        _checkedItemsCollectionChangedSource = checkedItems as INotifyCollectionChanged;
        if (_checkedItemsCollectionChangedSource != null)
        {
            _checkedItemsCollectionChangedSource.CollectionChanged += HandleCheckedItemsCollectionChanged;
        }

        _checkedItemsSnapshot = CopyCheckedItems(checkedItems);
    }

    private void ReleaseCheckedItemsCollectionChangedSource()
    {
        if (_checkedItemsCollectionChangedSource != null)
        {
            _checkedItemsCollectionChangedSource.CollectionChanged -= HandleCheckedItemsCollectionChanged;
            _checkedItemsCollectionChangedSource = null;
        }
    }

    private void HandleCheckedItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
    {
        if (!ReferenceEquals(sender, _checkedItemsCollectionChangedSource))
        {
            return;
        }

        var oldSnapshot = _checkedItemsSnapshot;
        var newSnapshot = CopyCheckedItems(CheckedItems);
        _checkedItemsSnapshot = newSnapshot;

        SyncCheckedItemsToItemsControl();
        NotifyCheckedItemsChanged(oldSnapshot, newSnapshot);
        _formValueChanged?.Invoke(this, EventArgs.Empty);
    }

    private void SyncCheckedItemsToItemsControl()
    {
        if (_itemsControl == null)
        {
            return;
        }

        _itemsControl.SelectionChanged -= HandleItemsSelectedChanged;
        try
        {
            _itemsControl.CheckedItems = CopyCheckedItems(CheckedItems);
        }
        finally
        {
            _itemsControl.SelectionChanged += HandleItemsSelectedChanged;
        }
    }

    private void NotifyCheckedItemsChanged(IList? oldValue, IList? newValue)
    {
        var removedItems = BuildRemovedItems(oldValue, newValue);
        var addedItems   = BuildRemovedItems(newValue, oldValue);
        if (removedItems.Count == 0 && addedItems.Count == 0)
        {
            return;
        }

        RaiseEvent(new CheckBoxGroupCheckedChangedEventArgs(CheckedChangedEvent, removedItems, addedItems));
    }

    private static IList BuildRemovedItems(IList? source, IList? target)
    {
        var items = new AvaloniaList<object?>();
        if (source == null)
        {
            return items;
        }

        foreach (var item in source)
        {
            if (target == null || !target.Contains(item))
            {
                items.Add(item);
            }
        }
        return items;
    }

    private static AvaloniaList<object?>? CopyCheckedItems(IEnumerable? source)
    {
        if (source == null)
        {
            return null;
        }

        var checkedItems = new AvaloniaList<object?>();
        foreach (var item in source)
        {
            checkedItems.Add(item);
        }
        return checkedItems;
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
    
    /// <summary>
    /// Applies a value assigned by the form system. The default sets <see cref="CheckedItems" /> with
    /// <c>SetCurrentValue</c>, so a binding on the property is kept.
    /// </summary>
    /// <param name="value">
    /// The items to check, or <see langword="null" /> to clear <see cref="CheckedItems" />, which leaves the
    /// checked state to the items as described for that property.
    /// </param>
    protected virtual void NotifySetFormValue(IList? value)
    {
        SetCurrentValue(CheckedItemsProperty, value);
    }

    /// <summary>
    /// Resets the group on behalf of the form system. The default sets <see cref="CheckedItems" /> to
    /// <see langword="null" /> with <c>SetCurrentValue</c>, so the checked state comes from the items as described
    /// for that property.
    /// </summary>
    protected virtual IList? NotifyGetFormValue()
    {
        return CheckedItems ?? _itemsControl?.BuildCurrentCheckedItems();
    }

    /// <summary>
    /// Resets the group on behalf of the form system. The default sets <see cref="CheckedItems" /> to
    /// <see langword="null" /> with <c>SetCurrentValue</c>, which unchecks every item.
    /// </summary>
    protected virtual void NotifyClearFormValue()
    {
        SetCurrentValue(CheckedItemsProperty, null);
    }
    
    /// <summary>
    /// Called when the form system reports a new validation status. The default ignores the status.
    /// </summary>
    /// <param name="status">
    /// The validation status reported by the form system.
    /// </param>
    protected virtual void NotifyValidateStatus(FormValidateStatus status)
    {
    }
    #endregion
}
