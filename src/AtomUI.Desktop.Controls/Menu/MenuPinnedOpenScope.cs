using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

// One menu level owns one pin. Group containers are transparent to this level;
// descendants get their own scope, rather than inheriting a pin on every sibling.
internal sealed class MenuPinnedOpenScope(SelectingItemsControl owner)
{
    private MenuItem? _activeItem;

    public void Reconcile()
    {
        if (!owner.GetValue(Popup.IsPopupPinnedOpenProperty))
        {
            Release();
            return;
        }

        if (_activeItem is { HasSubMenu: true } && IsDirectItem(_activeItem))
        {
            return;
        }

        var items = EnumerateItems(owner).Where(static item => item.HasSubMenu && item.IsEnabled && item.IsVisible).ToArray();
        Activate(items.FirstOrDefault(static item => item.IsSubMenuOpen) ?? items.FirstOrDefault());
    }

    public void SelectCurrent()
    {
        if (!owner.GetValue(Popup.IsPopupPinnedOpenProperty) || owner.SelectedIndex < 0)
        {
            return;
        }

        if (owner.ContainerFromIndex(owner.SelectedIndex) is MenuItem item)
        {
            Activate(item.HasSubMenu ? item : null);
        }
    }

    public void SubmenuOpened(MenuItem item)
    {
        if (owner.GetValue(Popup.IsPopupPinnedOpenProperty) && IsDirectItem(item))
        {
            Activate(item);
        }
    }

    public void Release()
    {
        var previous = _activeItem;
        _activeItem = null;
        if (previous is not null)
        {
            previous.PropertyChanged -= HandleActiveItemChanged;
            previous.IsPopupPinnedOpen = false;
        }
    }

    private void Activate(MenuItem? item)
    {
        if (ReferenceEquals(_activeItem, item))
        {
            return;
        }

        var previous = _activeItem;
        Release();
        // Record the new owner before publishing any open/selection notifications.
        _activeItem = item;
        previous?.CloseForLifecycle();
        if (item is not null)
        {
            item.PropertyChanged += HandleActiveItemChanged;
            item.IsPopupPinnedOpen = true;
        }
    }

    private void HandleActiveItemChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == StyledElement.ParentProperty || e.Property == ItemsControl.ItemCountProperty)
        {
            Reconcile();
        }
    }

    private bool IsDirectItem(MenuItem item)
    {
        StyledElement? parent = item.Parent;
        while (parent is MenuItemGroup)
        {
            parent = parent.Parent;
        }

        return ReferenceEquals(parent, owner);
    }

    private static IEnumerable<MenuItem> EnumerateItems(ItemsControl container)
    {
        foreach (var child in container.GetRealizedContainers())
        {
            if (child is MenuItem item)
            {
                yield return item;
            }
            else if (child is MenuItemGroup group)
            {
                foreach (var groupedItem in EnumerateItems(group))
                {
                    yield return groupedItem;
                }
            }
        }
    }

    internal static void ContainersChanged(StyledElement? owner)
    {
        while (owner is MenuItemGroup)
        {
            owner = owner.Parent;
        }

        switch (owner)
        {
            case MenuItem item:
                item.ReconcilePinnedOpenChildren();
                break;
            case MenuFlyoutPresenter presenter:
                presenter.ReconcilePinnedOpenChildren();
                break;
        }
    }
}
