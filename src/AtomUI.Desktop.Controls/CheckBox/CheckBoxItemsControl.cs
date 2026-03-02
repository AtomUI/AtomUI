using System.Collections;
using System.Collections.Specialized;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;

namespace AtomUI.Desktop.Controls;

internal class CheckBoxItemsControl : SelectingItemsControl
{
    public static readonly StyledProperty<double> ItemSpacingProperty = 
        CheckBoxGroup.ItemSpacingProperty.AddOwner<CheckBoxItemsControl>();
    
    public static readonly StyledProperty<Orientation> OrientationProperty = 
        StackPanel.OrientationProperty.AddOwner<CheckBoxGroup>();

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<CheckBoxGroup>();
    
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
    
    private readonly Dictionary<CheckBox, CompositeDisposable> _itemsBindingDisposables = new();

    static CheckBoxItemsControl()
    {
        SelectionModeProperty.OverrideDefaultValue<CheckBoxItemsControl>(SelectionMode.Multiple | SelectionMode.Toggle);
        CheckBox.IsCheckedChangedEvent.AddClassHandler<CheckBoxItemsControl>((group, args) => group.HandleCheckBoxCheckedChanged(args));
    }
    
    public CheckBoxItemsControl()
    {
        LogicalChildren.CollectionChanged += HandleCollectionChanged;
    }
    
    private void HandleCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems.Count > 0)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is CheckBox checkbox)
                    {
                        if (_itemsBindingDisposables.TryGetValue(checkbox, out var disposable))
                        {
                            disposable.Dispose();
                            _itemsBindingDisposables.Remove(checkbox);
                        }
                    }
                }
            }
        }
    }
    
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new CheckBox();
    }
    
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<CheckBox>(item, out recycleKey);
    }
    
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is CheckBox checkbox)
        {
            var disposables = new CompositeDisposable(4);
            
            if (item != null && item is not Visual)
            {
                {
                    if (ItemTemplate != null)
                    {
                        checkbox.SetCurrentValue(CheckBox.ContentProperty, item);
                    }
                    else
                    {
                        if (item is ICheckBoxOption checkBoxOption)
                        {
                            checkbox.SetCurrentValue(CheckBox.ContentProperty, checkBoxOption.Content);
                        }
                    }
                }

                {
                    if (item is ICheckBoxOption checkBoxOption)
                    {
                        checkbox.SetCurrentValue(CheckBox.IsEnabledProperty, checkBoxOption.IsEnabled);
                        checkbox.SetCurrentValue(CheckBox.IsCheckedProperty, checkBoxOption.IsChecked);
                        if (checkBoxOption.IsChecked)
                        {
                            SelectedItems?.Add(checkBoxOption);
                        }
                    }
                }
                if (ItemTemplate != null)
                {
                    disposables.Add(BindUtils.RelayBind(this, ItemTemplateProperty, checkbox, CheckBox.ContentTemplateProperty));
                }
            }
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, checkbox, CheckBox.IsMotionEnabledProperty));
            
            PrepareRadioButton(checkbox, item, index, disposables);
            
            if (_itemsBindingDisposables.TryGetValue(checkbox, out var oldDisposables))
            {
                oldDisposables.Dispose();
                _itemsBindingDisposables.Remove(checkbox);
            }
            _itemsBindingDisposables.Add(checkbox, disposables);
        }  
        else
        {
            throw new ArgumentOutOfRangeException(nameof(container), "The container type is incorrect, it must be type CheckBox.");
        }
    }
    
    protected virtual void PrepareRadioButton(CheckBox checkbox, object? item, int index, CompositeDisposable disposables)
    {
    }
    
    private void HandleCheckBoxCheckedChanged(RoutedEventArgs args)
    {
        if (args.Source is CheckBox checkBox)
        {
            UpdateSelection(checkBox, checkBox.IsChecked == true, true, true);
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
        if (SelectedItems == null)
        {
            foreach (var item in Items)
            {
                if (item != null)
                {
                    if (ContainerFromItem(item) is CheckBox checkBox)
                    {
                        checkBox.SetCurrentValue(CheckBox.IsCheckedProperty, false);
                    }
                }
            }
        }
        else
        {
            foreach (var item in SelectedItems)
            {
                if (item != null)
                {
                    if (ContainerFromItem(item) is CheckBox checkBox)
                    {
                        checkBox.SetCurrentValue(CheckBox.IsCheckedProperty, true);
                    }
                }
            }
        }
    }
    
    internal IList? CheckedItems
    {
        get => SelectedItems;
        set => SelectedItems = value;
    }
}