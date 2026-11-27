using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;

namespace AtomUI.Controls.Commons;

/// <summary>
/// Base class of the items control that hosts the checkable tags of a checkable tag group;
/// <c>CheckableTagItemsControl</c> in AtomUI.Desktop.Controls derives from it.
/// </summary>
/// <remarks>
/// <c>ItemSpacing</c> and <c>LineSpacing</c> are the gaps between the tags and between their lines,
/// <c>Orientation</c> the direction in which the tags are laid out, and <c>IsMultiple</c> whether several tags can
/// be checked at once. The properties are shared with <c>AbstractCheckableTagGroup</c>, which adds them to the
/// control.
/// </remarks>
internal abstract class AbstractCheckableTagItemsControl : SelectingItemsControl
{
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

    internal event Action<CheckableTagOptionItem, bool>? ItemCheckedChanged;

    private bool _isUpdatingCheckedStates;

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
            checkableTag[!ContentControl.ContentTemplateProperty] = this[!ItemTemplateProperty];
        }

        checkableTag[!AbstractCheckableTag.IsMotionEnabledProperty] = this[!IsMotionEnabledProperty];
        SetContainerCheckedState(checkableTag, IsItemSelected(item));
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SelectedItemProperty || change.Property == SelectedItemsProperty)
        {
            UpdateCheckedStates();
        }
    }

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
