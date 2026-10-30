using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

internal sealed class TabOverflowMenu : ItemsControl
{
    internal static readonly DirectProperty<TabOverflowMenu, TabOverflowPopupContext?> ContextProperty =
        AvaloniaProperty.RegisterDirect<TabOverflowMenu, TabOverflowPopupContext?>(
            nameof(Context),
            control => control.Context,
            (control, value) => control.Context = value);

    private TabOverflowPopupContext? _context;
    private bool _isContextSubscribed;
    private List<TabOverflowMenuItem>? _cachedContainers;
    private int _containerCacheLimit;

    static TabOverflowMenu()
    {
        KeyDownEvent.AddClassHandler<TabOverflowMenu>((menu, args) => menu.HandleKeyDown(args));
    }

    internal TabOverflowPopupContext? Context
    {
        get => _context;
        set
        {
            if (ReferenceEquals(_context, value))
            {
                return;
            }

            UnsubscribeContext();
            _containerCacheLimit = 0;
            _cachedContainers = null;
            ItemsSource = null;
            SetAndRaise(ContextProperty, ref _context, value);
            if (this.IsAttachedToVisualTree())
            {
                SubscribeContext();
            }
            PublishItems();
        }
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        if (_cachedContainers is { Count: > 0 } containers)
        {
            var lastIndex = containers.Count - 1;
            var container = containers[lastIndex];
            containers.RemoveAt(lastIndex);
            return container;
        }
        return new TabOverflowMenuItem();
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        recycleKey = DefaultRecycleKey;
        return item is not TabOverflowMenuItem;
    }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is TabOverflowMenuItem menuItem && item is TabOverflowItem overflowItem)
        {
            menuItem.SetOverflowItem(overflowItem);
        }
    }

    protected override void ClearContainerForItemOverride(Control container)
    {
        base.ClearContainerForItemOverride(container);
        if (container is TabOverflowMenuItem menuItem)
        {
            menuItem.ClearOverflowItem();
            if (_containerCacheLimit > 0)
            {
                _cachedContainers ??= new List<TabOverflowMenuItem>(_containerCacheLimit);
                if (_cachedContainers.Count < _containerCacheLimit)
                {
                    _cachedContainers.Add(menuItem);
                }
            }
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        SubscribeContext();
        PublishItems();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        UnsubscribeContext();
        ItemsSource = null;
        base.OnDetachedFromVisualTree(e);
    }

    private void SubscribeContext()
    {
        if (_isContextSubscribed || _context is null)
        {
            return;
        }

        _context.PropertyChanged += HandleContextPropertyChanged;
        _isContextSubscribed = true;
    }

    private void UnsubscribeContext()
    {
        if (_isContextSubscribed && _context is not null)
        {
            _context.PropertyChanged -= HandleContextPropertyChanged;
        }
        _isContextSubscribed = false;
    }

    private void HandleContextPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TabOverflowPopupContext.Items))
        {
            PublishItems();
        }
    }

    private void PublishItems()
    {
        var items = this.IsAttachedToVisualTree() ? _context?.Items : null;
        if (items is { Count: > 0 })
        {
            // Ordinary close publishes an empty snapshot. Keep only enough empty
            // containers for the latest nonempty snapshot, including after shrink.
            _containerCacheLimit = items.Count;
            if (_cachedContainers is { } containers)
            {
                if (containers.Count > _containerCacheLimit)
                {
                    containers.RemoveRange(_containerCacheLimit, containers.Count - _containerCacheLimit);
                }
                if (containers.Capacity > _containerCacheLimit)
                {
                    containers.Capacity = _containerCacheLimit;
                }
            }
        }
        ItemsSource = items;
    }

    private void HandleKeyDown(KeyEventArgs args)
    {
        if (args.Handled)
        {
            return;
        }

        if (args.Key == Key.Escape)
        {
            _context?.Dismiss();
            args.Handled = true;
            return;
        }

        var direction = args.Key switch
        {
            Key.Down => 1,
            Key.Up => -1,
            _ => 0
        };
        if (direction != 0)
        {
            MoveFocus(direction);
            args.Handled = true;
        }
        else if (args.Key == Key.Home)
        {
            FocusItem(FindEnabledIndex(0, 1));
            args.Handled = true;
        }
        else if (args.Key == Key.End)
        {
            FocusItem(FindEnabledIndex(ItemCount - 1, -1));
            args.Handled = true;
        }
    }

    private void MoveFocus(int direction)
    {
        var focused = TopLevel.GetTopLevel(this)?.FocusManager.GetFocusedElement() as Control;
        var current = focused?.FindLogicalAncestorOfType<TabOverflowMenuItem>() ?? focused as TabOverflowMenuItem;
        var currentIndex = current is null ? -1 : IndexFromContainer(current);
        var start = currentIndex < 0
            ? direction > 0 ? 0 : ItemCount - 1
            : currentIndex + direction;
        FocusItem(FindEnabledIndex(start, direction));
    }

    private int FindEnabledIndex(int start, int direction)
    {
        for (var index = start; index >= 0 && index < ItemCount; index += direction)
        {
            if (ItemsView[index] is TabOverflowItem { IsEnabled: true })
            {
                return index;
            }
        }
        return -1;
    }

    private void FocusItem(int index)
    {
        if (index >= 0 && ContainerFromIndex(index) is TabOverflowMenuItem item)
        {
            item.Focus(NavigationMethod.Directional);
        }
    }
}
