using AtomUI.Controls;
using Avalonia.Controls;
using Avalonia.Input;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Describes one entry of a menu that is built from data.
/// </summary>
/// <remarks>
/// When <see cref="Menu" />, <see cref="ContextMenu" />, <see cref="MenuItem" /> or <see cref="MenuItemGroup" />
/// generate a <see cref="MenuItem" /> for an item that implements this interface, the icon, key, enabled state,
/// input gesture and danger flag of the item are copied to the menu item. A value is copied only when the menu
/// item does not have one of its own.
/// </remarks>
public interface IMenuItemData : ITreeNode<IMenuItemData>
{
    /// <summary>
    /// The keyboard shortcut shown for the entry, such as <c>Ctrl+N</c>; <see langword="null" /> shows none.
    /// </summary>
    KeyGesture? InputGesture { get; }

    /// <summary>
    /// Whether the item is a danger (destructive) action, drawn with the menu's danger item colors.
    /// </summary>
    bool IsDanger => false;
}

/// <summary>
/// Describes one entry of a menu as data.
/// </summary>
/// <remarks>
/// A host that generates a container for the item copies the values to the menu item as described for
/// <see cref="IMenuItemData" />. <see cref="MenuItemGroupData" /> and <see cref="MenuSeparatorData" /> derive
/// from this class to describe a group or a separator.
/// </remarks>
public class MenuItemData : IMenuItemData
{
    /// <summary>
    /// The item that owns this item in its <see cref="Children" />, or <see langword="null" /> without a parent.
    /// </summary>
    /// <remarks>
    /// A parent assigns it to the <see cref="MenuItemData" /> entries found in <see cref="Children" /> while that
    /// list is initialized. Items added to the list afterwards keep <see langword="null" />.
    /// </remarks>
    public ITreeNode<IMenuItemData>? ParentNode { get; internal set; }
    /// <summary>
    /// The key that identifies the entry, or <see langword="null" /> to leave it without a key.
    /// </summary>
    /// <remarks>
    /// Copied to <see cref="MenuItem.ItemKey" /> of the generated menu item, but only when that has no key yet.
    /// </remarks>
    public EntityKey? ItemKey { get; init; }
    /// <summary>
    /// The label of the entry, and the title of the group for a <see cref="MenuItemGroupData" />.
    /// </summary>
    /// <remarks>
    /// The item itself, not this value, becomes the <c>Header</c> of a generated menu item; a data template that
    /// presents the item binds to this property for the label.
    /// </remarks>
    public object? Header { get; init; }
    /// <summary>
    /// The icon shown for the entry; <see langword="null" />, the default, shows no icon.
    /// </summary>
    /// <remarks>
    /// Copied to <see cref="MenuItem.Icon" /> of the generated menu item unless an icon is already set there.
    /// </remarks>
    public PathIcon? Icon { get; init; }
    /// <summary>
    /// Whether the entry accepts interaction; <see langword="true" /> by default.
    /// </summary>
    /// <remarks>
    /// Copied to the <c>IsEnabled</c> of the generated menu item unless a value is already set there.
    /// </remarks>
    public bool IsEnabled { get; init; } = true;
    /// <summary>
    /// The keyboard shortcut shown for the entry, such as <c>Ctrl+N</c>.
    /// <see langword="null" />, the default, shows none.
    /// </summary>
    /// <remarks>
    /// Copied to the <c>InputGesture</c> of the generated menu item unless a gesture is already set there.
    /// </remarks>
    public KeyGesture? InputGesture { get; init; }
    /// <summary>
    /// Whether the entry is a danger (destructive) action, drawn with the menu's danger item colors.
    /// </summary>
    /// <remarks>
    /// <see langword="false" /> by default. Copied to <see cref="MenuItem.IsDanger" /> of the generated menu item
    /// unless a value is already set there.
    /// </remarks>
    public bool IsDanger { get; init; }

    private IList<IMenuItemData> _children = [];
    /// <summary>
    /// The entries of the submenu of this item. The default is an empty list that can still be changed.
    /// </summary>
    /// <remarks>
    /// A data template that presents the item binds the submenu to this list. Items present while the list is
    /// initialized get this item as their <see cref="ParentNode" />; items added afterwards do not.
    /// </remarks>
    public IList<IMenuItemData> Children
    {
        get => _children;
        init
        {
            _children = value;
            foreach (var child in _children)
            {
                if (child is MenuItemData item)
                {
                    item.ParentNode = this;
                }
            }
        }
    }

    IEnumerable<IMenuItemData> ITreeNode<IMenuItemData>.Children => Children;
}
