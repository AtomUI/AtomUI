using System.Collections;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;

namespace AtomUI.Controls.Commons;

/// <summary>
/// Base class of the items control that hosts the check boxes of a check box group and keeps their checked state
/// and the selection in step; <c>CheckBoxItemsControl</c> in AtomUI.Desktop.Controls derives from it.
/// </summary>
/// <remarks>
/// Every container is an <c>AbstractCheckBox</c>. For an item that implements <c>ICheckBoxOption</c>, the check
/// box takes the enabled state and the initial checked state from the option; it shows the item itself when
/// <c>ItemTemplate</c> is set and otherwise the content of the option. The checked state then follows the
/// selection of the control, which allows several items to be selected at once.
/// </remarks>
internal abstract class AbstractCheckBoxItemsControl : SelectingItemsControl
{
    public static readonly StyledProperty<double> ItemSpacingProperty = 
        AbstractCheckBoxGroup.ItemSpacingProperty.AddOwner<AbstractCheckBoxItemsControl>();
    
    public static readonly StyledProperty<double> LineSpacingProperty = 
        AbstractCheckBoxGroup.LineSpacingProperty.AddOwner<AbstractCheckBoxItemsControl>();
    
    public static readonly StyledProperty<Orientation> OrientationProperty = 
        StackPanel.OrientationProperty.AddOwner<AbstractCheckBoxItemsControl>();

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractCheckBoxItemsControl>();
    
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

    private bool _hasCheckedItemsSource;

    static AbstractCheckBoxItemsControl()
    {
        SelectionModeProperty.OverrideDefaultValue<AbstractCheckBoxItemsControl>(SelectionMode.Multiple | SelectionMode.Toggle);
        AbstractCheckBox.IsCheckedChangedEvent.AddClassHandler<AbstractCheckBoxItemsControl>((group, args) => group.HandleCheckBoxCheckedChanged(args));
    }
    
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<AbstractCheckBox>(item, out recycleKey);
    }
    
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is AbstractCheckBox checkbox)
        {
            if (item != null && item is not Visual)
            {
                {
                    if (ItemTemplate != null)
                    {
                        checkbox.SetCurrentValue(AbstractCheckBox.ContentProperty, item);
                    }
                    else
                    {
                        if (item is ICheckBoxOption checkBoxOption)
                        {
                            checkbox.SetCurrentValue(AbstractCheckBox.ContentProperty, checkBoxOption.Content);
                        }
                    }
                }

                {
                    if (item is ICheckBoxOption checkBoxOption)
                    {
                        checkbox.SetCurrentValue(AbstractCheckBox.IsEnabledProperty, checkBoxOption.IsEnabled);
                        var isChecked = IsItemSelected(item) ||
                                        (!_hasCheckedItemsSource && checkBoxOption.IsChecked);
                        checkbox.SetCurrentValue(AbstractCheckBox.IsCheckedProperty, isChecked);
                        if (!_hasCheckedItemsSource && checkBoxOption.IsChecked && SelectedItems != null &&
                            !SelectedItems.Contains(checkBoxOption))
                        {
                            SelectedItems.Add(checkBoxOption);
                        }
                    }
                }
                if (ItemTemplate != null)
                {
                    checkbox[!AbstractCheckBox.ContentTemplateProperty] = this[!ItemTemplateProperty];
                }
            }
            checkbox[!IsMotionEnabledProperty] = this[!IsMotionEnabledProperty];
            
            PrepareRadioButton(checkbox, item, index);
        }  
        else
        {
            throw new ArgumentOutOfRangeException(nameof(container), "The container type is incorrect, it must be type AbstractCheckBox.");
        }
    }
    
    protected virtual void PrepareRadioButton(AbstractCheckBox checkbox, object? item, int index)
    {
    }
    
    private void HandleCheckBoxCheckedChanged(RoutedEventArgs args)
    {
        if (args.Source is AbstractCheckBox checkBox)
        {
            var index = IndexFromContainer(checkBox);
            if (index != -1)
            {
                if (checkBox.IsChecked == true)
                {
                    Selection.Select(index);
                }
                else
                {
                    Selection.Deselect(index);
                }
            }
            args.Handled = true;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SelectedItemsProperty)
        {
            UpdateCheckBoxCheckedStates();
        }
    }

    private void UpdateCheckBoxCheckedStates()
    {
        foreach (var item in Items)
        {
            if (item != null && ContainerFromItem(item) is AbstractCheckBox checkBox)
            {
                checkBox.SetCurrentValue(AbstractCheckBox.IsCheckedProperty, IsItemSelected(item));
            }
        }
    }

    private bool IsItemSelected(object item)
    {
        return SelectedItems?.Contains(item) == true;
    }

    internal AvaloniaList<object?>? BuildCurrentCheckedItems()
    {
        var checkedItems = new AvaloniaList<object?>();
        foreach (var item in Items)
        {
            if (item == null)
            {
                continue;
            }

            var checkBox = ContainerFromItem(item) as AbstractCheckBox ??
                           item as AbstractCheckBox;
            if (checkBox?.IsChecked == true)
            {
                checkedItems.Add(item);
            }
        }

        return checkedItems.Count > 0 ? checkedItems : null;
    }
    
    internal IList? CheckedItems
    {
        get => SelectedItems;
        set
        {
            _hasCheckedItemsSource = value != null;
            SelectedItems          = value;
        }
    }
}
