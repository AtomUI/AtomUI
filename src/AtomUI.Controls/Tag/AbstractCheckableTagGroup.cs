using AtomUI.Theme.Resources;
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

[TemplatePart("PART_CheckableTagItems", typeof(SelectingItemsControl))]
public abstract class AbstractCheckableTagGroup : TemplatedControl,
                                                  IMotionAwareControl,
                                                  IFormItemAware, IThemeInheritanceSource
{
    #region 公共属性定义

    public static readonly StyledProperty<IEnumerable?> OptionsProperty =
        AvaloniaProperty.Register<AbstractCheckableTagGroup, IEnumerable?>(nameof(Options));

    public static readonly StyledProperty<bool> IsMultipleProperty =
        AvaloniaProperty.Register<AbstractCheckableTagGroup, bool>(nameof(IsMultiple));

    public static readonly StyledProperty<object?> CheckedItemProperty =
        AvaloniaProperty.Register<AbstractCheckableTagGroup, object?>(
            nameof(CheckedItem),
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);

    public static readonly DirectProperty<AbstractCheckableTagGroup, IList?> CheckedItemsProperty =
        AvaloniaProperty.RegisterDirect<AbstractCheckableTagGroup, IList?>(
            nameof(CheckedItems),
            group => group.CheckedItems,
            (group, value) => group.CheckedItems = value,
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);

    public static readonly StyledProperty<object?> DefaultCheckedItemProperty =
        AvaloniaProperty.Register<AbstractCheckableTagGroup, object?>(nameof(DefaultCheckedItem));

    public static readonly StyledProperty<IEnumerable?> DefaultCheckedItemsProperty =
        AvaloniaProperty.Register<AbstractCheckableTagGroup, IEnumerable?>(nameof(DefaultCheckedItems));

    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        ItemsControl.ItemTemplateProperty.AddOwner<AbstractCheckableTagGroup>();

    public static readonly StyledProperty<double> ItemSpacingProperty =
        AvaloniaProperty.Register<AbstractCheckableTagGroup, double>(nameof(ItemSpacing));

    public static readonly StyledProperty<double> LineSpacingProperty =
        AvaloniaProperty.Register<AbstractCheckableTagGroup, double>(nameof(LineSpacing));

    public static readonly StyledProperty<Orientation> OrientationProperty =
        StackPanel.OrientationProperty.AddOwner<AbstractCheckableTagGroup>();

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractCheckableTagGroup>();

    public IEnumerable? Options
    {
        get => GetValue(OptionsProperty);
        set => SetValue(OptionsProperty, value);
    }

    public bool IsMultiple
    {
        get => GetValue(IsMultipleProperty);
        set => SetValue(IsMultipleProperty, value);
    }

    public object? CheckedItem
    {
        get => GetValue(CheckedItemProperty);
        set => SetValue(CheckedItemProperty, value);
    }

    private IList? _checkedItems;

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

    public object? DefaultCheckedItem
    {
        get => GetValue(DefaultCheckedItemProperty);
        set => SetValue(DefaultCheckedItemProperty, value);
    }

    public IEnumerable? DefaultCheckedItems
    {
        get => GetValue(DefaultCheckedItemsProperty);
        set => SetValue(DefaultCheckedItemsProperty, value);
    }

    [InheritDataTypeFromItems(nameof(Options))]
    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public double ItemSpacing
    {
        get => GetValue(ItemSpacingProperty);
        set => SetValue(ItemSpacingProperty, value);
    }

    public double LineSpacing
    {
        get => GetValue(LineSpacingProperty);
        set => SetValue(LineSpacingProperty, value);
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

    #endregion

    #region 公共事件定义

    public static readonly RoutedEvent<CheckableTagGroupCheckedChangedEventArgs> CheckedChangedEvent =
        RoutedEvent.Register<AbstractCheckableTagGroup, CheckableTagGroupCheckedChangedEventArgs>(
            nameof(CheckedChanged),
            RoutingStrategies.Bubble);

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

    private EventHandler? _formValueChanged;

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

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _isControlInitialized = true;
        RebuildOptionItems();
        InitializeSelectionIfNeeded();
    }

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

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ReleaseOptionsCollectionChangedSource();
        ReleaseCheckedItemsCollectionChangedSource();
        UnsubscribeFromItemsControl();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == CheckedItemsProperty)
        {
            HandleCheckedItemsChanged();
        }
    }

    #region 实现 IThemeInheritanceSource 接口

    AvaloniaObject? IThemeInheritanceSource.ThemeInheritanceParent => InheritanceParent;

    #endregion

    #region 实现 FormItem 接口

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
