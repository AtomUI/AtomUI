using System.Reactive.Disposables;
using AtomUI.Data;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace AtomUI.Desktop.Controls;

internal class SelectOptions : List
{
    public static readonly StyledProperty<int> MaxCountProperty =
        Select.MaxCountProperty.AddOwner<SelectOptions>();
    
    public int MaxCount
    {
        get => GetValue(MaxCountProperty);
        set => SetValue(MaxCountProperty, value);
    }
    
    public Select? Select { get; set; }

    private ISelectOption? _activeOption;
    private int _activeIndex = -1;
    
    internal override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        if (item is ListGroupData)
        {
            return new SelectGroupHeader();
        }
        return new SelectOptionItem();
    }
    
    internal void EnsureActiveOption()
    {
        if (ListDefaultView == null)
        {
            return;
        }
        
        if (_activeOption != null)
        {
            var existingIndex = FindIndexForOption(_activeOption);
            if (existingIndex >= 0)
            {
                _activeIndex = existingIndex;
                return;
            }
        }

        ISelectOption? optionToActivate = null;
        if (SelectedItems != null)
        {
            foreach (var item in SelectedItems)
            {
                if (item is ISelectOption opt && opt.IsEnabled)
                {
                    optionToActivate = opt;
                    break;
                }
            }
        }

        optionToActivate ??= FindFirstEnabledOption();
        SetActiveOption(optionToActivate);
    }
    
    internal void SetActiveOption(ISelectOption? option)
    {
        if (ReferenceEquals(_activeOption, option))
        {
            return;
        }

        var oldOption = _activeOption;
        _activeOption = option;
        
        if (ListDefaultView == null)
        {
            _activeIndex = -1;
            return;
        }
        
        _activeIndex = option != null ? FindIndexForOption(option) : -1;
        UpdateContainerActiveState(oldOption, false);
        UpdateContainerActiveState(option, true);
        ScrollActiveIntoView();
    }

    internal void MoveActiveBy(int delta)
    {
        if (ListDefaultView == null || delta == 0)
        {
            return;
        }
        
        var itemCount = ItemCount;
        if (itemCount <= 0)
        {
            return;
        }

        var startIndex = _activeIndex;
        if (startIndex < 0 || startIndex >= itemCount)
        {
            EnsureActiveOption();
            startIndex = _activeIndex;
        }

        if (startIndex < 0 || startIndex >= itemCount)
        {
            return;
        }
        
        var nextIndex = FindNextEnabledOptionIndex(startIndex, delta);
        if (nextIndex < 0)
        {
            return;
        }
        
        if (ListDefaultView.Items[nextIndex] is ISelectOption option)
        {
            SetActiveOption(option);
        }
    }

    internal void CommitActiveSelection()
    {
        if (_activeOption == null || !_activeOption.IsEnabled || Select == null)
        {
            return;
        }
        
        Select.NotifyLogicalSelectOption(_activeOption);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        var handled = false;
        if (Select != null)
        {
            if (e.Key == Key.Down)
            {
                MoveActiveBy(1);
                handled = true;
            }
            else if (e.Key == Key.Up)
            {
                MoveActiveBy(-1);
                handled = true;
            }
            else if (e.Key == Key.Enter || e.Key == Key.Space)
            {
                CommitActiveSelection();
                if (Select.Mode == SelectMode.Single)
                {
                    Select.SetCurrentValue(Select.IsDropDownOpenProperty, false);
                }
                handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                Select.SetCurrentValue(Select.IsDropDownOpenProperty, false);
                handled = true;
            }
        }
        
        if (!handled)
        {
            base.OnKeyDown(e);
        }
        else
        {
            e.Handled = true;
        }
    }
    
    private void LogicalSelectOption(Control container)
    {
        if (ListDefaultView != null && Select != null)
        {
            var item = ListDefaultView.ItemFromContainer(container);
            if (item is ISelectOption option)
            {
                SetActiveOption(option);
                Select.NotifyLogicalSelectOption(option);
            }
        }
    }
    
    internal override bool UpdateSelectionFromPointerEvent(Control source, PointerEventArgs e)
    {
        var container = GetContainerFromEventSource(source);
        if (container != null)
        {
            LogicalSelectOption(container);
            e.Handled = true;
            return true;
        }
        return false;
    }
    
    internal override void PrepareContainerForItemOverride(CompositeDisposable disposables, Control container, object? item, int index)
    {
        if (container is SelectOptionItem listItem)
        {
            if (item != null && item is not Visual)
            {
                if (ItemTemplate != null)
                {
                    listItem.SetCurrentValue(SelectOptionItem.ContentProperty, item);
                }
                else if (item is ISelectOption selectOption)
                {
                    listItem.SetCurrentValue(SelectOptionItem.ContentProperty, selectOption.Header);
                }
                if (item is ISelectOption data)
                {
                    if (!listItem.IsSet(SelectOptionItem.IsEnabledProperty))
                    {
                        listItem.SetCurrentValue(IsEnabledProperty, data.IsEnabled);
                    }
                }
            }

            listItem.SetCurrentValue(SelectOptionItem.IsActiveProperty, item is ISelectOption opt && ReferenceEquals(opt, _activeOption));
            
            if (ItemTemplate != null)
            {
                disposables.Add(BindUtils.RelayBind(this, ItemTemplateProperty, listItem, SelectOptionItem.ContentTemplateProperty));
            }
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, listItem, SelectOptionItem.IsMotionEnabledProperty));
            disposables.Add(BindUtils.RelayBind(this, SizeTypeProperty, listItem, SelectOptionItem.SizeTypeProperty));
            disposables.Add(BindUtils.RelayBind(this, IsShowSelectedIndicatorProperty, listItem, SelectOptionItem.IsShowSelectedIndicatorProperty));
            disposables.Add(BindUtils.RelayBind(this, DisabledItemHoverEffectProperty, listItem,
                SelectOptionItem.DisabledItemHoverEffectProperty));
        }
        else if (container is SelectGroupHeader groupItem)
        {
            if (item != null && item is not Visual)
            {
                if (!groupItem.IsSet(SelectGroupHeader.ContentProperty))
                {
                    if (GroupItemTemplate != null)
                    {
                        groupItem.SetCurrentValue(SelectGroupHeader.ContentProperty, item);
                    }
                    else if (item is ListGroupData groupData)
                    {
                        groupItem.SetCurrentValue(SelectGroupHeader.ContentProperty, groupData.Header);
                    }
                }
            }
            
            if (GroupItemTemplate != null)
            {
                disposables.Add(BindUtils.RelayBind(this, GroupItemTemplateProperty, groupItem, SelectGroupHeader.ContentTemplateProperty));
            }
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, groupItem, SelectGroupHeader.IsMotionEnabledProperty));
            disposables.Add(BindUtils.RelayBind(this, SizeTypeProperty, groupItem, SelectGroupHeader.SizeTypeProperty));
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == MaxCountProperty || change.Property == SelectedItemsProperty)
        {
            ConfigureOptionsForMaxCount();
        }
    }

    private void ConfigureOptionsForMaxCount()
    {
        if (ListDefaultView != null)
        {
            if (SelectedItems?.Count >= MaxCount)
            {
                for (var i = 0; i < ItemCount; i++)
                {
                    var item      = ListDefaultView.Items[i];
                    var container = ListDefaultView.ContainerFromIndex(i);
                    if (!SelectedItems.Contains(item))
                    {
                        container?.SetCurrentValue(IsEnabledProperty, false);
                    }
                    else
                    {
                        container?.SetCurrentValue(IsEnabledProperty, true);
                    }
                }
            }
            else
            {
                for (var i = 0; i < ItemCount; i++)
                {
                    var item      = ListDefaultView.Items[i];
                    var container = ListDefaultView.ContainerFromIndex(i);
                    if (item is ISelectOption selectOption)
                    {
                        container?.SetCurrentValue(IsEnabledProperty, selectOption.IsEnabled);
                    }
                }
            }
        }
    }

    private int FindIndexForOption(ISelectOption option)
    {
        if (ListDefaultView == null)
        {
            return -1;
        }
        
        for (var i = 0; i < ItemCount; i++)
        {
            if (ReferenceEquals(ListDefaultView.Items[i], option))
            {
                return i;
            }
        }
        return -1;
    }

    private ISelectOption? FindFirstEnabledOption()
    {
        if (ListDefaultView == null)
        {
            return null;
        }
        
        for (var i = 0; i < ItemCount; i++)
        {
            if (ListDefaultView.Items[i] is ISelectOption option && option.IsEnabled)
            {
                return option;
            }
        }
        return null;
    }

    private int FindNextEnabledOptionIndex(int startIndex, int delta)
    {
        if (ListDefaultView == null)
        {
            return -1;
        }
        
        var index = startIndex;
        while (true)
        {
            index += delta;
            if (index < 0 || index >= ItemCount)
            {
                return -1;
            }
            
            if (ListDefaultView.Items[index] is ISelectOption option && option.IsEnabled)
            {
                return index;
            }
        }
    }

    private void UpdateContainerActiveState(ISelectOption? option, bool isActive)
    {
        if (option == null || ListDefaultView == null)
        {
            return;
        }

        var index = FindIndexForOption(option);
        if (index < 0)
        {
            return;
        }

        if (ListDefaultView.ContainerFromIndex(index) is SelectOptionItem optionItem)
        {
            optionItem.SetCurrentValue(SelectOptionItem.IsActiveProperty, isActive);
            if (isActive)
            {
                optionItem.BringIntoView();
            }
        }
    }

    private void ScrollActiveIntoView()
    {
        if (_activeIndex < 0 || ListDefaultView?.Scroll == null || Select == null)
        {
            return;
        }

        var scroll = ListDefaultView.Scroll;
        if (ListDefaultView.ContainerFromIndex(_activeIndex) is Control container)
        {
            container.BringIntoView();
            return;
        }

        var itemHeight = Select.ItemHeight;
        if (double.IsNaN(itemHeight) || itemHeight <= 0)
        {
            return;
        }

        var offset   = scroll.Offset;
        var viewport = scroll.Viewport;
        var extent   = scroll.Extent;

        var targetTop    = _activeIndex * itemHeight;
        var targetBottom = targetTop + itemHeight;
        var viewTop      = offset.Y;
        var viewBottom   = offset.Y + viewport.Height;

        double newOffsetY = viewTop;
        if (targetTop < viewTop)
        {
            newOffsetY = targetTop;
        }
        else if (targetBottom > viewBottom)
        {
            newOffsetY = targetBottom - viewport.Height;
        }

        var maxOffset = Math.Max(0, extent.Height - viewport.Height);
        newOffsetY = Math.Clamp(newOffsetY, 0, maxOffset);

        if (Math.Abs(newOffsetY - offset.Y) > 0.1)
        {
            scroll.Offset = new Vector(offset.X, newOffsetY);
        }
    }
}
