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

namespace AtomUI.Controls.Commons;

/// <summary>
/// Base class for a group of checkable tags. It builds one tag for every entry of <see cref="Options" /> in the
/// <c>PART_CheckableTagItems</c> template part and keeps the checked values in <see cref="CheckedItem" /> or in
/// <see cref="CheckedItems" />, depending on <see cref="IsMultiple" />.
/// </summary>
/// <remarks>
/// The template must provide the <c>PART_CheckableTagItems</c> part, an items control derived from
/// <c>AbstractCheckableTagItemsControl</c>; without it no tag is shown. As <c>IFormItemAware</c>, the group
/// reports <see cref="CheckedItems" /> in multiple mode and <see cref="CheckedItem" /> otherwise as its form
/// value, and clearing the form value sets that property to <see langword="null" />.
/// </remarks>
[TemplatePart("PART_CheckableTagItems", typeof(SelectingItemsControl))]
public abstract class AbstractCheckableTagGroup : TemplatedControl,
                                                  IMotionAwareControl,
                                                  IFormItemAware
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="Options" /> property.
    /// </summary>
    public static readonly StyledProperty<IEnumerable?> OptionsProperty =
        AvaloniaProperty.Register<AbstractCheckableTagGroup, IEnumerable?>(nameof(Options));

    /// <summary>
    /// Identifies the <see cref="IsMultiple" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMultipleProperty =
        AvaloniaProperty.Register<AbstractCheckableTagGroup, bool>(nameof(IsMultiple));

    /// <summary>
    /// Identifies the <see cref="CheckedItem" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> CheckedItemProperty =
        AvaloniaProperty.Register<AbstractCheckableTagGroup, object?>(
            nameof(CheckedItem),
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);

    /// <summary>
    /// Identifies the <see cref="CheckedItems" /> property.
    /// </summary>
    public static readonly DirectProperty<AbstractCheckableTagGroup, IList?> CheckedItemsProperty =
        AvaloniaProperty.RegisterDirect<AbstractCheckableTagGroup, IList?>(
            nameof(CheckedItems),
            group => group.CheckedItems,
            (group, value) => group.CheckedItems = value,
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);

    /// <summary>
    /// Identifies the <see cref="DefaultCheckedItem" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> DefaultCheckedItemProperty =
        AvaloniaProperty.Register<AbstractCheckableTagGroup, object?>(nameof(DefaultCheckedItem));

    /// <summary>
    /// Identifies the <see cref="DefaultCheckedItems" /> property.
    /// </summary>
    public static readonly StyledProperty<IEnumerable?> DefaultCheckedItemsProperty =
        AvaloniaProperty.Register<AbstractCheckableTagGroup, IEnumerable?>(nameof(DefaultCheckedItems));

    /// <summary>
    /// Identifies the <see cref="ItemTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        ItemsControl.ItemTemplateProperty.AddOwner<AbstractCheckableTagGroup>();

    /// <summary>
    /// Identifies the <see cref="ItemSpacing" /> property.
    /// </summary>
    public static readonly StyledProperty<double> ItemSpacingProperty =
        AvaloniaProperty.Register<AbstractCheckableTagGroup, double>(nameof(ItemSpacing));

    /// <summary>
    /// Identifies the <see cref="LineSpacing" /> property.
    /// </summary>
    public static readonly StyledProperty<double> LineSpacingProperty =
        AvaloniaProperty.Register<AbstractCheckableTagGroup, double>(nameof(LineSpacing));

    /// <summary>
    /// Identifies the <see cref="Orientation" /> property.
    /// </summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        StackPanel.OrientationProperty.AddOwner<AbstractCheckableTagGroup>();

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractCheckableTagGroup>();

    /// <summary>
    /// The entries shown as tags, set on the group. One tag is created for each entry. <see langword="null" />
    /// leaves the group without tags.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown while the tags are rebuilt when an entry or its value is <see langword="null" />, or when a value occurs
    /// more than once.
    /// </exception>
    /// <remarks>
    /// An entry that implements <c>ICheckableTagOption</c> is identified by its <c>Value</c> and displays its
    /// <c>Content</c>; any other entry is used as both. Values are compared with <c>Equals</c>. The tags are rebuilt
    /// when the group is initialized or attached, when this property changes, and when a collection that implements
    /// <c>INotifyCollectionChanged</c> raises a change while the group is attached.
    /// </remarks>
    public IEnumerable? Options
    {
        get => GetValue(OptionsProperty);
        set => SetValue(OptionsProperty, value);
    }

    /// <summary>
    /// Selects whether more than one tag can be checked. When <see langword="true" />, any number of tags can be
    /// checked and their values are kept in <see cref="CheckedItems" />; when <see langword="false" />, the default,
    /// one tag can be checked and its value is kept in <see cref="CheckedItem" />.
    /// </summary>
    /// <remarks>
    /// A tag click updates only the property of the active mode. After the selection is initialized, a change of the
    /// mode converts the selection: to multiple mode the checked item becomes the only checked value; to single mode
    /// only the first checked value is kept. <see cref="CheckedChanged" /> is raised once for the difference.
    /// </remarks>
    public bool IsMultiple
    {
        get => GetValue(IsMultipleProperty);
        set => SetValue(IsMultipleProperty, value);
    }

    /// <summary>
    /// The value of the checked tag while <see cref="IsMultiple" /> is <see langword="false" />.
    /// <see langword="null" />, the default, means that no tag is checked.
    /// </summary>
    /// <remarks>
    /// A tag whose value equals this value is shown as checked; a value that matches no tag is kept, but shown and
    /// reported as <see langword="null" />. Clicking a tag sets its value and clicking the checked tag again sets
    /// <see langword="null" />; clearing the value leaves no tag checked. While <see cref="IsMultiple" /> is
    /// <see langword="true" /> the tags ignore this property and a tag click does not change it. Bindings default to
    /// <see cref="BindingMode.TwoWay" />, so a click also updates the bound source.
    /// </remarks>
    public object? CheckedItem
    {
        get => GetValue(CheckedItemProperty);
        set => SetValue(CheckedItemProperty, value);
    }

    private IList? _checkedItems;

    /// <summary>
    /// The values of the checked tags while <see cref="IsMultiple" /> is <see langword="true" />.
    /// <see langword="null" />, the default, or an empty list means that no tag is checked.
    /// </summary>
    /// <remarks>
    /// A tag whose value equals a value in the list is shown as checked; other entries are kept but not shown or
    /// reported. A tag click assigns a new list that holds the checked values with the clicked value added or
    /// removed, so the list object is replaced rather than changed. A list that implements
    /// <c>INotifyCollectionChanged</c> updates the tags and raises <see cref="CheckedChanged" /> on every change
    /// while the group is attached to a visual tree. While <see cref="IsMultiple" /> is <see langword="false" /> the
    /// tags ignore this property and a tag click does not change it. Bindings default to
    /// <see cref="BindingMode.TwoWay" />, so a click also updates the bound source.
    /// </remarks>
    public IList? CheckedItems
    {
        get => _checkedItems;
        set
        {
            if (!_isApplyingDefaultCheckedItems)
            {
                _hasExplicitCheckedItems = true;
            }
            SetAndRaise(CheckedItemsProperty, ref _checkedItems, value);
        }
    }

    /// <summary>
    /// The value assigned to <see cref="CheckedItem" /> when the selection is initialized in single mode, that is
    /// while <see cref="IsMultiple" /> is <see langword="false" /> and <see cref="CheckedItem" /> has not been set.
    /// </summary>
    /// <remarks>
    /// It is applied once, as soon as the group is initialized and has at least one tag, without replacing a
    /// binding on <see cref="CheckedItem" />. It is ignored in multiple mode, after <see cref="CheckedItem" /> has
    /// been set, and after the selection has been initialized.
    /// </remarks>
    public object? DefaultCheckedItem
    {
        get => GetValue(DefaultCheckedItemProperty);
        set => SetValue(DefaultCheckedItemProperty, value);
    }

    /// <summary>
    /// The values assigned to <see cref="CheckedItems" /> when the selection is initialized in multiple mode, that
    /// is while <see cref="IsMultiple" /> is <see langword="true" /> and <see cref="CheckedItems" /> has not been
    /// assigned.
    /// </summary>
    /// <remarks>
    /// A copy without <see langword="null" /> entries and duplicates is assigned once the group is initialized and
    /// has at least one tag; a binding on <see cref="CheckedItems" /> stays in place. Changing this property after
    /// that has no effect. It is ignored in single mode.
    /// </remarks>
    public IEnumerable? DefaultCheckedItems
    {
        get => GetValue(DefaultCheckedItemsProperty);
        set => SetValue(DefaultCheckedItemsProperty, value);
    }

    /// <summary>
    /// The template that presents each tag. It is set on the group and handed to the items control that hosts the
    /// tags.
    /// </summary>
    /// <remarks>
    /// When a template is set it is applied to the original entry of <see cref="Options" />; when it is
    /// <see langword="null" />, the tag shows the entry's content without a template. An item template declared
    /// inline in XAML infers its data type from <see cref="Options" />.
    /// </remarks>
    [InheritDataTypeFromItems(nameof(Options))]
    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    /// <summary>
    /// The gap between two adjacent tags along the layout direction, in device-independent pixels. The theme hands
    /// the value to the panel that arranges the tags. <see cref="LineSpacing" /> sets the gap between the wrapped
    /// rows or columns.
    /// </summary>
    /// <remarks>
    /// When the value is not set or is cleared, the theme uses the tag group's <c>SpacingXS</c> token.
    /// </remarks>
    public double ItemSpacing
    {
        get => GetValue(ItemSpacingProperty);
        set => SetValue(ItemSpacingProperty, value);
    }

    /// <summary>
    /// The gap between the rows the tags wrap onto, or between the columns when <see cref="Orientation" /> is
    /// vertical, in device-independent pixels. The theme hands the value to the panel that arranges the tags.
    /// </summary>
    /// <remarks>
    /// When the value is not set or is cleared, the theme uses the tag group's <c>SpacingXS</c> token.
    /// </remarks>
    public double LineSpacing
    {
        get => GetValue(LineSpacingProperty);
        set => SetValue(LineSpacingProperty, value);
    }

    /// <summary>
    /// The direction in which the tags are arranged before they wrap. <c>Horizontal</c>, the default, places them
    /// from left to right and wraps onto a new row; <c>Vertical</c> places them from top to bottom and wraps onto a
    /// new column. The theme hands the value to the panel that arranges the tags.
    /// </summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    /// Enables the color transitions of the tag background and foreground when <see langword="true" />. The value
    /// is passed on to every tag.
    /// </summary>
    /// <remarks>
    /// When the value is not set or is cleared, the theme uses the tag group's <c>EnableMotion</c> token.
    /// </remarks>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    #endregion

    #region 公共事件定义

    /// <summary>
    /// Identifies the <see cref="CheckedChanged" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<CheckableTagGroupCheckedChangedEventArgs> CheckedChangedEvent =
        RoutedEvent.Register<AbstractCheckableTagGroup, CheckableTagGroupCheckedChangedEventArgs>(
            nameof(CheckedChanged),
            RoutingStrategies.Bubble);

    /// <summary>
    /// Raised after the checked values changed and the tags show the new state. The event bubbles up the visual
    /// tree and cannot be canceled.
    /// </summary>
    /// <remarks>
    /// It is raised when a tag is clicked, when <see cref="CheckedItem" /> or <see cref="CheckedItems" /> changes
    /// from code or a binding, when the <see cref="CheckedItems" /> collection changes, and when
    /// <see cref="IsMultiple" /> converts the selection, even if no value was added or removed. It is not raised
    /// while the selection is initialized from <see cref="DefaultCheckedItem" /> or
    /// <see cref="DefaultCheckedItems" />, nor when <see cref="Options" /> changes. The event data tells the mode and
    /// carries the previous and the new checked item in single mode, or the added and the removed values in multiple
    /// mode. Afterwards <c>IFormItemAware.ValueChanged</c> is raised.
    /// </remarks>
    public event EventHandler<CheckableTagGroupCheckedChangedEventArgs>? CheckedChanged
    {
        add => AddHandler(CheckedChangedEvent, value);
        remove => RemoveHandler(CheckedChangedEvent, value);
    }

    #endregion

    private AvaloniaList<CheckableTagOptionItem> _optionItems = new();
    private Dictionary<object, CheckableTagOptionItem> _optionIndex = new();
    private AbstractCheckableTagItemsControl? _itemsControl;
    private INotifyCollectionChanged? _optionsCollectionChangedSource;
    private INotifyCollectionChanged? _checkedItemsCollectionChangedSource;
    private IList? _checkedItemsSnapshot;
    private object? _checkedItemSnapshot;
    private bool _hasExplicitCheckedItems;
    private bool _isApplyingDefaultCheckedItems;
    private bool _isControlInitialized;
    private bool _isSelectionInitialized;
    private bool _isItemsControlSubscribed;
    private bool _isConvertingMode;

    static AbstractCheckableTagGroup()
    {
        OrientationProperty.OverrideDefaultValue<AbstractCheckableTagGroup>(Orientation.Horizontal);
        OptionsProperty.Changed.AddClassHandler<AbstractCheckableTagGroup>(
            (group, _) => group.HandleOptionsChanged());
        IsMultipleProperty.Changed.AddClassHandler<AbstractCheckableTagGroup>(
            (group, args) => group.HandleIsMultipleChanged(args.NewValue is true));
        CheckedItemProperty.Changed.AddClassHandler<AbstractCheckableTagGroup>(
            (group, _) => group.HandleCheckedItemChanged());
        ItemTemplateProperty.Changed.AddClassHandler<AbstractCheckableTagGroup>(
            (group, _) => group.ConfigureItemsControl());
    }

    /// <summary>
    /// Builds the tags from <see cref="Options" /> and initializes the selection once the control is initialized.
    /// An override must call the base method, otherwise the tags are not built.
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        _isControlInitialized = true;
        RebuildOptionItems();
        InitializeSelectionIfNeeded();
    }

    /// <summary>
    /// Connects the <c>PART_CheckableTagItems</c> part of the applied template: the part receives the tags, the item
    /// template and the current selection, and the group starts listening for tag clicks. An override must call the
    /// base method, otherwise no tag is shown.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        UnsubscribeFromItemsControl();
        base.OnApplyTemplate(e);

        _itemsControl = e.NameScope.Find<AbstractCheckableTagItemsControl>("PART_CheckableTagItems");
        if (_itemsControl != null)
        {
            _itemsControl.ItemsSource = _optionItems;
            ConfigureItemsControl();
            SubscribeToItemsControl();
            SyncSelectionToItemsControl();
        }
    }

    /// <summary>
    /// Refreshes the tags and the selection when the group is attached to a visual tree, and from then on observes
    /// changes of the <see cref="Options" /> and <see cref="CheckedItems" /> collections and the tag clicks. An
    /// override must call the base method, otherwise changes of those collections are not picked up.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ConfigureOptionsCollectionChangedSource();
        ConfigureCheckedItemsCollectionChangedSource();
        RebuildOptionItems();
        InitializeSelectionIfNeeded();
        UpdateEffectiveSelectionSnapshots();
        SubscribeToItemsControl();
        SyncSelectionToItemsControl();
    }

    /// <summary>
    /// Stops observing the <see cref="Options" /> and <see cref="CheckedItems" /> collections and the tag clicks
    /// when the group is detached from a visual tree. An override must call the base method, otherwise the group
    /// stays subscribed to the collections after detachment.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ReleaseOptionsCollectionChangedSource();
        ReleaseCheckedItemsCollectionChangedSource();
        UnsubscribeFromItemsControl();
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Updates the tags and raises <see cref="CheckedChanged" /> when <see cref="CheckedItems" /> changes. An
    /// override must call the base method, otherwise such a change is not picked up.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == CheckedItemsProperty)
        {
            HandleCheckedItemsChanged();
        }
    }

    #region 实现 FormItem 接口

    private EventHandler? _formValueChanged;

    event EventHandler IFormItemAware.ValueChanged
    {
        add => _formValueChanged += value;
        remove => _formValueChanged -= value;
    }

    void IFormItemAware.SetFormValue(object? value)
    {
        if (IsMultiple)
        {
            SetCurrentValue(CheckedItemsProperty, value as IList);
        }
        else
        {
            SetCurrentValue(CheckedItemProperty, value);
        }
    }

    object? IFormItemAware.GetFormValue()
    {
        return IsMultiple ? CheckedItems : CheckedItem;
    }

    void IFormItemAware.ClearFormValue()
    {
        if (IsMultiple)
        {
            SetCurrentValue(CheckedItemsProperty, null);
        }
        else
        {
            SetCurrentValue(CheckedItemProperty, null);
        }
    }

    void IFormItemAware.NotifyValidateStatus(FormValidateStatus status)
    {
    }

    #endregion

    private void HandleOptionsChanged()
    {
        ConfigureOptionsCollectionChangedSource();
        RebuildOptionItems();
        InitializeSelectionIfNeeded();
    }

    private void HandleIsMultipleChanged(bool isMultiple)
    {
        ConfigureItemsControlSelectionMode();

        if (!_isSelectionInitialized)
        {
            return;
        }

        var oldCheckedItem  = GetEffectiveCheckedItem();
        var oldCheckedItems = BuildEffectiveCheckedItems();
        _isConvertingMode = true;
        try
        {
            if (isMultiple)
            {
                var checkedItems = new AvaloniaList<object?>();
                if (oldCheckedItem != null)
                {
                    checkedItems.Add(oldCheckedItem);
                }
                SetCurrentValue(CheckedItemsProperty, checkedItems);
            }
            else
            {
                SetCurrentValue(CheckedItemProperty, FirstOrDefault(oldCheckedItems));
            }
        }
        finally
        {
            _isConvertingMode = false;
        }

        SyncSelectionToItemsControl();
        NotifyModeConverted(isMultiple, oldCheckedItem, oldCheckedItems);
    }

    private void HandleCheckedItemChanged()
    {
        var oldValue = _checkedItemSnapshot;
        var newValue = GetEffectiveCheckedItem();
        _checkedItemSnapshot = newValue;

        if (!_isSelectionInitialized || IsMultiple || _isConvertingMode)
        {
            return;
        }

        SyncSelectionToItemsControl();
        NotifySingleSelectionChanged(oldValue, newValue);
    }

    private void HandleCheckedItemsChanged()
    {
        ConfigureCheckedItemsCollectionChangedSource();

        var oldSnapshot = _checkedItemsSnapshot;
        var newSnapshot = BuildEffectiveCheckedItems();
        _checkedItemsSnapshot = newSnapshot;

        if (!_isSelectionInitialized || !IsMultiple || _isConvertingMode)
        {
            return;
        }

        SyncSelectionToItemsControl();
        NotifyMultipleSelectionChanged(oldSnapshot, newSnapshot);
    }

    private void InitializeSelectionIfNeeded()
    {
        if (_isSelectionInitialized || !_isControlInitialized || _optionItems.Count == 0)
        {
            return;
        }

        if (IsMultiple)
        {
            if (!_hasExplicitCheckedItems && DefaultCheckedItems != null)
            {
                _isApplyingDefaultCheckedItems = true;
                try
                {
                    SetCurrentValue(CheckedItemsProperty, CopyDistinctValues(DefaultCheckedItems));
                }
                finally
                {
                    _isApplyingDefaultCheckedItems = false;
                }
            }
        }
        else if (!IsSet(CheckedItemProperty) && DefaultCheckedItem != null)
        {
            SetCurrentValue(CheckedItemProperty, DefaultCheckedItem);
        }

        _isSelectionInitialized = true;
        UpdateEffectiveSelectionSnapshots();
        SyncSelectionToItemsControl();
    }

    private void RebuildOptionItems()
    {
        var optionItems = new AvaloniaList<CheckableTagOptionItem>();
        var optionIndex = new Dictionary<object, CheckableTagOptionItem>();

        if (Options != null)
        {
            foreach (var source in Options)
            {
                if (source == null)
                {
                    throw new ArgumentException("CheckableTagGroup option values cannot be null.", nameof(Options));
                }

                var value   = source is ICheckableTagOption option ? option.Value : source;
                var content = source is ICheckableTagOption checkableOption ? checkableOption.Content : source;
                if (value == null)
                {
                    throw new ArgumentException("CheckableTagGroup option values cannot be null.", nameof(Options));
                }
                var optionItem = new CheckableTagOptionItem(source, value, content);
                if (!optionIndex.TryAdd(value, optionItem))
                {
                    throw new ArgumentException(
                        $"CheckableTagGroup option values must be unique. Duplicate value: {value}.",
                        nameof(Options));
                }

                optionItems.Add(optionItem);
            }
        }

        _optionItems = optionItems;
        _optionIndex = optionIndex;
        if (_itemsControl != null)
        {
            UnsubscribeFromItemsControl();
            _itemsControl.ItemsSource = _optionItems;
            SubscribeToItemsControl();
        }
        if (_isSelectionInitialized)
        {
            UpdateEffectiveSelectionSnapshots();
        }
        SyncSelectionToItemsControl();
    }

    private void SyncSelectionToItemsControl()
    {
        if (_itemsControl == null)
        {
            return;
        }

        var selectedOptions = new List<CheckableTagOptionItem>();
        if (IsMultiple)
        {
            foreach (var value in BuildEffectiveCheckedItems())
            {
                if (value != null && _optionIndex.TryGetValue(value, out var optionItem))
                {
                    selectedOptions.Add(optionItem);
                }
            }
        }
        else if (GetEffectiveCheckedItem() is { } checkedItem &&
                 _optionIndex.TryGetValue(checkedItem, out var selectedOption))
        {
            selectedOptions.Add(selectedOption);
        }

        UnsubscribeFromItemsControl();
        try
        {
            ConfigureItemsControl();
            _itemsControl.SetSelectedOptions(selectedOptions);
        }
        finally
        {
            SubscribeToItemsControl();
        }
    }

    private void ConfigureItemsControl()
    {
        if (_itemsControl == null)
        {
            return;
        }

        _itemsControl.SetCurrentValue(ItemsControl.ItemTemplateProperty, ItemTemplate);
        ConfigureItemsControlSelectionMode();
    }

    private void ConfigureItemsControlSelectionMode()
    {
        if (_itemsControl == null)
        {
            return;
        }

        _itemsControl.SetCurrentValue(
            AbstractCheckableTagItemsControl.IsMultipleProperty,
            IsMultiple);
    }

    private void HandleItemCheckedChanged(CheckableTagOptionItem optionItem, bool isChecked)
    {
        if (IsMultiple)
        {
            var checkedItems = BuildEffectiveCheckedItems();
            if (isChecked)
            {
                if (!ContainsValue(checkedItems, optionItem.Value))
                {
                    checkedItems.Add(optionItem.Value);
                }
            }
            else
            {
                RemoveValue(checkedItems, optionItem.Value);
            }
            SetCurrentValue(CheckedItemsProperty, checkedItems);
        }
        else
        {
            var checkedItem = isChecked
                ? optionItem.Value
                : ValueEquals(CheckedItem, optionItem.Value)
                    ? null
                    : CheckedItem;
            SetCurrentValue(CheckedItemProperty, checkedItem);
        }
    }

    private AvaloniaList<object?> BuildEffectiveCheckedItems()
    {
        return BuildEffectiveCheckedItems(CheckedItems);
    }

    private AvaloniaList<object?> BuildEffectiveCheckedItems(IEnumerable? source)
    {
        var values = new AvaloniaList<object?>();
        if (source == null)
        {
            return values;
        }

        var seenValues = new HashSet<object>();
        foreach (var value in source)
        {
            if (value != null &&
                _optionIndex.TryGetValue(value, out var optionItem) &&
                seenValues.Add(optionItem.Value))
            {
                values.Add(optionItem.Value);
            }
        }
        return values;
    }

    private object? GetEffectiveCheckedItem()
    {
        return CheckedItem != null && _optionIndex.TryGetValue(CheckedItem, out var optionItem)
            ? optionItem.Value
            : null;
    }

    private void UpdateEffectiveSelectionSnapshots()
    {
        _checkedItemSnapshot  = GetEffectiveCheckedItem();
        _checkedItemsSnapshot = BuildEffectiveCheckedItems();
    }

    private void ConfigureOptionsCollectionChangedSource()
    {
        if (!this.IsAttachedToVisualTree())
        {
            ReleaseOptionsCollectionChangedSource();
            return;
        }
        if (ReferenceEquals(_optionsCollectionChangedSource, Options))
        {
            return;
        }

        ReleaseOptionsCollectionChangedSource();
        _optionsCollectionChangedSource = Options as INotifyCollectionChanged;
        if (_optionsCollectionChangedSource != null)
        {
            _optionsCollectionChangedSource.CollectionChanged += HandleOptionsCollectionChanged;
        }
    }

    private void ReleaseOptionsCollectionChangedSource()
    {
        if (_optionsCollectionChangedSource != null)
        {
            _optionsCollectionChangedSource.CollectionChanged -= HandleOptionsCollectionChanged;
            _optionsCollectionChangedSource = null;
        }
    }

    private void HandleOptionsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
    {
        if (ReferenceEquals(sender, _optionsCollectionChangedSource))
        {
            RebuildOptionItems();
            InitializeSelectionIfNeeded();
        }
    }

    private void ConfigureCheckedItemsCollectionChangedSource()
    {
        if (!this.IsAttachedToVisualTree())
        {
            ReleaseCheckedItemsCollectionChangedSource();
            return;
        }
        if (ReferenceEquals(_checkedItemsCollectionChangedSource, CheckedItems))
        {
            return;
        }

        ReleaseCheckedItemsCollectionChangedSource();
        _checkedItemsCollectionChangedSource = CheckedItems as INotifyCollectionChanged;
        if (_checkedItemsCollectionChangedSource != null)
        {
            _checkedItemsCollectionChangedSource.CollectionChanged += HandleCheckedItemsCollectionChanged;
        }
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
        var newSnapshot = BuildEffectiveCheckedItems();
        _checkedItemsSnapshot = newSnapshot;
        if (_isSelectionInitialized && IsMultiple)
        {
            SyncSelectionToItemsControl();
            NotifyMultipleSelectionChanged(oldSnapshot, newSnapshot);
        }
    }

    private void SubscribeToItemsControl()
    {
        if (_itemsControl == null || _isItemsControlSubscribed || !this.IsAttachedToVisualTree())
        {
            return;
        }
        _itemsControl.ItemCheckedChanged += HandleItemCheckedChanged;
        _isItemsControlSubscribed = true;
    }

    private void UnsubscribeFromItemsControl()
    {
        if (_itemsControl == null || !_isItemsControlSubscribed)
        {
            return;
        }
        _itemsControl.ItemCheckedChanged -= HandleItemCheckedChanged;
        _isItemsControlSubscribed = false;
    }

    private void NotifySingleSelectionChanged(object? oldValue, object? newValue)
    {
        if (ValueEquals(oldValue, newValue))
        {
            return;
        }

        var addedItems   = new AvaloniaList<object?>();
        var removedItems = new AvaloniaList<object?>();
        if (oldValue != null)
        {
            removedItems.Add(oldValue);
        }
        if (newValue != null)
        {
            addedItems.Add(newValue);
        }

        RaiseEvent(new CheckableTagGroupCheckedChangedEventArgs(
            CheckedChangedEvent,
            false,
            oldValue,
            newValue,
            addedItems,
            removedItems));
        _formValueChanged?.Invoke(this, EventArgs.Empty);
    }

    private void NotifyMultipleSelectionChanged(IList? oldValues, IList? newValues)
    {
        if (SequenceEquals(oldValues, newValues))
        {
            return;
        }

        var addedItems   = BuildDifference(newValues, oldValues);
        var removedItems = BuildDifference(oldValues, newValues);
        RaiseEvent(new CheckableTagGroupCheckedChangedEventArgs(
            CheckedChangedEvent,
            true,
            null,
            null,
            addedItems,
            removedItems));
        _formValueChanged?.Invoke(this, EventArgs.Empty);
    }

    private void NotifyModeConverted(bool isMultiple, object? oldCheckedItem, IList? oldCheckedItems)
    {
        if (isMultiple)
        {
            var oldValues = new AvaloniaList<object?>();
            if (oldCheckedItem != null)
            {
                oldValues.Add(oldCheckedItem);
            }
            var newValues    = BuildEffectiveCheckedItems();
            var addedItems   = BuildDifference(newValues, oldValues);
            var removedItems = BuildDifference(oldValues, newValues);
            RaiseEvent(new CheckableTagGroupCheckedChangedEventArgs(
                CheckedChangedEvent,
                true,
                null,
                null,
                addedItems,
                removedItems));
        }
        else
        {
            var oldValue = FirstOrDefault(oldCheckedItems);
            var newValue = GetEffectiveCheckedItem();
            var addedItems   = new AvaloniaList<object?>();
            var removedItems = new AvaloniaList<object?>();
            if (!ValueEquals(oldValue, newValue))
            {
                if (oldValue != null)
                {
                    removedItems.Add(oldValue);
                }
                if (newValue != null)
                {
                    addedItems.Add(newValue);
                }
            }
            RaiseEvent(new CheckableTagGroupCheckedChangedEventArgs(
                CheckedChangedEvent,
                false,
                oldValue,
                newValue,
                addedItems,
                removedItems));
        }

        _formValueChanged?.Invoke(this, EventArgs.Empty);
    }

    private static AvaloniaList<object?>? CopyDistinctValues(IEnumerable? source)
    {
        if (source == null)
        {
            return null;
        }

        var values = new AvaloniaList<object?>();
        var seenValues = new HashSet<object>();
        foreach (var value in source)
        {
            if (value != null && seenValues.Add(value))
            {
                values.Add(value);
            }
        }
        return values;
    }

    private static AvaloniaList<object?> BuildDifference(IList? source, IList? target)
    {
        var values = new AvaloniaList<object?>();
        if (source == null)
        {
            return values;
        }
        var targetValues = new HashSet<object>();
        if (target != null)
        {
            foreach (var value in target)
            {
                if (value != null)
                {
                    targetValues.Add(value);
                }
            }
        }
        foreach (var value in source)
        {
            if (value != null && !targetValues.Contains(value))
            {
                values.Add(value);
            }
        }
        return values;
    }

    private static bool SequenceEquals(IList? left, IList? right)
    {
        var leftCount  = left?.Count ?? 0;
        var rightCount = right?.Count ?? 0;
        if (leftCount != rightCount)
        {
            return false;
        }
        for (var index = 0; index < leftCount; index++)
        {
            if (!ValueEquals(left![index], right![index]))
            {
                return false;
            }
        }
        return true;
    }

    private static bool ContainsValue(IEnumerable? values, object? value)
    {
        if (values == null)
        {
            return false;
        }
        foreach (var candidate in values)
        {
            if (ValueEquals(candidate, value))
            {
                return true;
            }
        }
        return false;
    }

    private static void RemoveValue(IList values, object value)
    {
        for (var index = values.Count - 1; index >= 0; index--)
        {
            if (ValueEquals(values[index], value))
            {
                values.RemoveAt(index);
            }
        }
    }

    private static object? FirstOrDefault(IEnumerable? values)
    {
        if (values == null)
        {
            return null;
        }
        foreach (var value in values)
        {
            return value;
        }
        return null;
    }

    private static bool ValueEquals(object? left, object? right)
    {
        return Equals(left, right);
    }
}
