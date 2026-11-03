using System.Reactive.Disposables;
using AtomUI.Data;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;

namespace AtomUI.Controls.Commons;

internal abstract class AbstractCheckableTagItemsControl : SelectingItemsControl
{
    #region 公共属性定义

    public static readonly StyledProperty<double> ItemSpacingProperty =
        AbstractCheckableTagGroup.ItemSpacingProperty.AddOwner<AbstractCheckableTagItemsControl>();

    public static readonly StyledProperty<double> LineSpacingProperty =
        AbstractCheckableTagGroup.LineSpacingProperty.AddOwner<AbstractCheckableTagItemsControl>();

    public static readonly StyledProperty<Orientation> OrientationProperty =
        AbstractCheckableTagGroup.OrientationProperty.AddOwner<AbstractCheckableTagItemsControl>();

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        AbstractCheckableTagGroup.IsMotionEnabledProperty.AddOwner<AbstractCheckableTagItemsControl>();

    public static readonly StyledProperty<bool> IsMultipleProperty =
        AbstractCheckableTagGroup.IsMultipleProperty.AddOwner<AbstractCheckableTagItemsControl>();

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

    public bool IsMultiple
    {
        get => GetValue(IsMultipleProperty);
        set => SetValue(IsMultipleProperty, value);
    }

    #endregion

    #region 内部事件定义

    internal event Action<CheckableTagOptionItem, bool>? ItemCheckedChanged;

    #endregion

    private bool _isUpdatingCheckedStates;
    private readonly Dictionary<AbstractCheckableTag, CompositeDisposable> _containerBindings = new();

    static AbstractCheckableTagItemsControl()
    {
        SelectionModeProperty.OverrideDefaultValue<AbstractCheckableTagItemsControl>(
            SelectionMode.Single | SelectionMode.Toggle);
        AbstractCheckableTag.IsCheckedChangedEvent.AddClassHandler<AbstractCheckableTagItemsControl>(
            (itemsControl, args) => itemsControl.HandleCheckableTagCheckedChanged(args));
        IsMultipleProperty.Changed.AddClassHandler<AbstractCheckableTagItemsControl>(
            (itemsControl, args) => itemsControl.UpdateSelectionMode(args.NewValue is true));
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<AbstractCheckableTag>(item, out recycleKey);
    }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);

        if (container is not AbstractCheckableTag checkableTag)
        {
            throw new ArgumentOutOfRangeException(
                nameof(container),
                "The container type must derive from AbstractCheckableTag.");
        }

        if (item is CheckableTagOptionItem optionItem)
        {
            checkableTag.SetCurrentValue(
                ContentControl.ContentProperty,
                ItemTemplate != null ? optionItem.Source : optionItem.Content);
        }

        ConfigureContainerBindings(checkableTag, item is CheckableTagOptionItem);
        SetContainerCheckedState(checkableTag, IsItemSelected(item));
    }

    protected override void ClearContainerForItemOverride(Control container)
    {
        if (container is AbstractCheckableTag tag && _containerBindings.Remove(tag, out var bindings))
        {
            bindings.Dispose();
        }
        base.ClearContainerForItemOverride(container);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        for (var index = 0; index < ItemCount; index++)
        {
            if (ContainerFromIndex(index) is AbstractCheckableTag tag && !_containerBindings.ContainsKey(tag))
            {
                ConfigureContainerBindings(tag, Items[index] is CheckableTagOptionItem);
            }
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        foreach (var bindings in _containerBindings.Values)
        {
            bindings.Dispose();
        }
        _containerBindings.Clear();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SelectedItemProperty || change.Property == SelectedItemsProperty)
        {
            UpdateCheckedStates();
        }
    }

    #region 内部协作 API

    internal void SetSelectedOptions(IReadOnlyList<CheckableTagOptionItem> optionItems)
    {
        if (IsMultiple)
        {
            var selectedItems = new AvaloniaList<object?>();
            foreach (var optionItem in optionItems)
            {
                selectedItems.Add(optionItem);
            }
            SelectedItems = selectedItems;
        }
        else
        {
            SelectedItem = optionItems.Count > 0 ? optionItems[0] : null;
        }

        UpdateCheckedStates();
    }

    #endregion

    private void ConfigureContainerBindings(AbstractCheckableTag tag, bool hasOptionItem)
    {
        if (_containerBindings.Remove(tag, out var previous))
        {
            previous.Dispose();
        }
        // Container projections are established at LocalValue during Prepare,
        // before logical membership. ControlTheme setters cannot preserve that
        // priority and timing; membership owns and retires these typed relays.
        var bindings = new CompositeDisposable
        {
            BindUtils.RelayBind(this, IsMotionEnabledProperty, tag, AbstractCheckableTag.IsMotionEnabledProperty)
        };
        if (hasOptionItem)
        {
            bindings.Add(BindUtils.RelayBind(this, ItemTemplateProperty, tag, ContentControl.ContentTemplateProperty));
        }
        _containerBindings.Add(tag, bindings);
    }

    private void UpdateSelectionMode(bool isMultiple)
    {
        SetCurrentValue(
            SelectionModeProperty,
            isMultiple
                ? SelectionMode.Multiple | SelectionMode.Toggle
                : SelectionMode.Single | SelectionMode.Toggle);
    }

    private void HandleCheckableTagCheckedChanged(RoutedEventArgs args)
    {
        if (args.Source is not AbstractCheckableTag checkableTag)
        {
            return;
        }

        if (_isUpdatingCheckedStates)
        {
            args.Handled = true;
            return;
        }

        var index = IndexFromContainer(checkableTag);
        if (index < 0 || Items[index] is not CheckableTagOptionItem optionItem)
        {
            return;
        }

        var isChecked = checkableTag.IsChecked == true;
        if (isChecked)
        {
            Selection.Select(index);
        }
        else
        {
            Selection.Deselect(index);
        }
        args.Handled = true;
        ItemCheckedChanged?.Invoke(optionItem, isChecked);
    }

    private void UpdateCheckedStates()
    {
        _isUpdatingCheckedStates = true;
        try
        {
            for (var index = 0; index < ItemCount; index++)
            {
                if (ContainerFromIndex(index) is AbstractCheckableTag checkableTag)
                {
                    checkableTag.SetCurrentValue(ToggleButton.IsCheckedProperty, IsItemSelected(Items[index]));
                }
            }
        }
        finally
        {
            _isUpdatingCheckedStates = false;
        }
    }

    private void SetContainerCheckedState(AbstractCheckableTag checkableTag, bool isChecked)
    {
        _isUpdatingCheckedStates = true;
        try
        {
            checkableTag.SetCurrentValue(ToggleButton.IsCheckedProperty, isChecked);
        }
        finally
        {
            _isUpdatingCheckedStates = false;
        }
    }

    private bool IsItemSelected(object? item)
    {
        return item != null && SelectedItems?.Contains(item) == true;
    }
}
