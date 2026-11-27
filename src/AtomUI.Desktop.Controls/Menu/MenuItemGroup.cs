using AtomUI.Generated.AtomUIDesktopControls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A titled list of entries inside a menu, matching the item group of Ant Design (<c>ant-menu-item-group</c>).
/// The header is drawn as a title row that does not react to input, and the items are menu items or separators.
/// </summary>
/// <remarks>
/// <see cref="Menu" /> and <see cref="MenuItem" /> create a group for a <see cref="MenuItemGroupData" /> item.
/// </remarks>
public class MenuItemGroup : ItemsControl
{
    /// <summary>
    /// Identifies the <see cref="Header" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<MenuItemGroup, object?>(nameof(Header));

    /// <summary>
    /// Identifies the <see cref="HeaderTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> HeaderTemplateProperty =
        AvaloniaProperty.Register<MenuItemGroup, IDataTemplate?>(nameof(HeaderTemplate));

    /// <summary>
    /// The title of the group, shown as a row above the entries.
    /// </summary>
    /// <remarks>
    /// The title row is always laid out: with the default, <see langword="null" />, it has no content.
    /// </remarks>
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <summary>
    /// The template that presents <see cref="Header" /> in the title row.
    /// </summary>
    /// <remarks>
    /// The default, <see langword="null" />, leaves the choice to the title presenter: a control is shown as it is,
    /// and other content is shown through a matching data template, or as text when none matches.
    /// </remarks>
    public IDataTemplate? HeaderTemplate
    {
        get => GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    private const string ItemTitleGroupClass = "semantic-item-title-group";

    // The semantic level of the group is handed down by its owner, a Menu or a MenuItem, when that owner prepares
    // the group. The menu items inside the group inherit it.
    private MenuSemanticLevel _semanticLevel;

    internal MenuSemanticLevel SemanticLevel
    {
        get => _semanticLevel;
        set
        {
            MenuSemanticLevelScope.ApplyGroupLevel(this, value);
            if (_semanticLevel != value)
            {
                _semanticLevel = value;
                MenuSemanticLevelScope.ApplyChildrenLevel(this, value);
            }
        }
    }

    public MenuItemGroup()
    {
        // The itemTitle semantic part is anchored to the group title element. The group container itself
        // carries the marker class in the middle of the cross-visual-root route, matching the itemTitle
        // SelectorRoute declared in DropdownButton.SemanticParts.cs. The title element is marked when the
        // template is applied.
        Classes.Add(ItemTitleGroupClass);
    }

    /// <summary>
    /// Creates the container for an item of the group.
    /// </summary>
    /// <returns>
    /// A <see cref="MenuSeparator" /> for a <see cref="MenuSeparatorData" />, otherwise a <see cref="MenuItem" />
    /// that carries the <c>item</c> marker class of a dropdown button and gets the <see cref="SemanticLevel" />
    /// of the group.
    /// </returns>
    /// <remarks>
    /// Called only for items for which <see cref="NeedsContainerOverride" /> returned <see langword="true" />.
    /// The base member is not called.
    /// </remarks>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        if (item is MenuSeparatorData)
        {
            return new MenuSeparator();
        }

        var menuItem = new MenuItem();
        menuItem.Classes.Add(DropdownButtonSemanticParts.ItemClass);
        ApplyChildSemanticLevel(menuItem);
        return menuItem;
    }

    /// <summary>
    /// Reports whether an item of the group needs a container of its own.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> for an item that needs a container, with <paramref name="recycleKey" /> set to
    /// <see cref="ItemsControl.DefaultRecycleKey" />.
    /// <see langword="false" /> when the item already is a <see cref="MenuItem" /> or a <see cref="MenuSeparator" />,
    /// which then serves as its own container; <paramref name="recycleKey" /> is <see langword="null" />.
    /// </returns>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        if (item is MenuItem or MenuSeparator)
        {
            recycleKey = null;
            return false;
        }

        recycleKey = DefaultRecycleKey;
        return true;
    }

    /// <summary>
    /// Fills a <see cref="MenuItem" /> container with the values of its item.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The container is neither a <see cref="MenuItem" /> nor a <see cref="MenuSeparator" />.
    /// </exception>
    /// <remarks>
    /// Calls the base member first. The menu item gets the <c>item</c> marker class of a dropdown button and the
    /// <see cref="SemanticLevel" /> of the group. An item that is not a <see cref="Visual" /> becomes the
    /// <c>Header</c> of the menu item unless it is already set. An item that implements <see cref="IMenuItemData" />
    /// also supplies the icon, key, enabled state, input gesture and danger flag, each unless the menu item already
    /// has a value. A <see cref="MenuSeparator" /> container is laid out horizontally.
    /// </remarks>
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is MenuItem menuItem)
        {
            menuItem.Classes.Add(DropdownButtonSemanticParts.ItemClass);
            ApplyChildSemanticLevel(menuItem);

            if (item != null && item is not Visual)
            {
                if (!menuItem.IsSet(MenuItem.HeaderProperty))
                {
                    menuItem.SetCurrentValue(MenuItem.HeaderProperty, item);
                }

                if (item is IMenuItemData menuItemData)
                {
                    if (!menuItem.IsSet(MenuItem.IconProperty))
                    {
                        menuItem.SetCurrentValue(MenuItem.IconProperty, menuItemData.Icon);
                    }

                    if (menuItem.ItemKey == null)
                    {
                        menuItem.ItemKey = menuItemData.ItemKey;
                    }

                    if (!menuItem.IsSet(MenuItem.IsEnabledProperty))
                    {
                        menuItem.SetCurrentValue(IsEnabledProperty, menuItemData.IsEnabled);
                    }

                    if (!menuItem.IsSet(MenuItem.InputGestureProperty))
                    {
                        menuItem.SetCurrentValue(MenuItem.InputGestureProperty, menuItemData.InputGesture);
                    }

                    if (!menuItem.IsSet(MenuItem.IsDangerProperty))
                    {
                        menuItem.SetCurrentValue(MenuItem.IsDangerProperty, menuItemData.IsDanger);
                    }
                }
            }
        }
        else if (container is MenuSeparator menuSeparator)
        {
            menuSeparator.Orientation = Orientation.Horizontal;
        }
        else if (container is not MenuSeparator)
        {
            throw new ArgumentOutOfRangeException(nameof(container),
                "The container type is incorrect, it must be type MenuItem or MenuSeparator.");
        }
    }

    /// <summary>
    /// Calls the base member, then passes the parent of the group to
    /// <see cref="MenuPinnedOpenScope.ContainersChanged" />, so that the menu item or flyout presenter that owns the
    /// group reconciles its pinned-open items.
    /// </summary>
    protected override void ContainerForItemPreparedOverride(Control container, object? item, int index)
    {
        base.ContainerForItemPreparedOverride(container, item, index);
        MenuPinnedOpenScope.ContainersChanged(Parent);
    }

    /// <summary>
    /// Calls the base member, then adds the itemTitle class of a dropdown button to the group title presenter of the
    /// template.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        e.NameScope.Find<ContentPresenter>("GroupTitlePresenter")?
         .Classes.Add(DropdownButtonSemanticParts.ItemTitleClass);
    }

    // Explicit containers do not always receive a generator clear callback.
    /// <summary>
    /// Resets <see cref="SemanticLevel" /> to <see cref="MenuSemanticLevel.None" /> when the group loses its parent.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ParentProperty && Parent is null)
        {
            SemanticLevel = MenuSemanticLevel.None;
        }
    }

    /// <summary>
    /// Resets the <see cref="SemanticLevel" /> of a <see cref="MenuItem" /> container to
    /// <see cref="MenuSemanticLevel.None" />, then calls the base member.
    /// </summary>
    protected override void ClearContainerForItemOverride(Control container)
    {
        if (container is MenuItem item)
        {
            item.SemanticLevel = MenuSemanticLevel.None;
        }
        base.ClearContainerForItemOverride(container);
    }

    private void ApplyChildSemanticLevel(MenuItem menuItem)
    {
        menuItem.SemanticLevel = SemanticLevel;
    }
}
