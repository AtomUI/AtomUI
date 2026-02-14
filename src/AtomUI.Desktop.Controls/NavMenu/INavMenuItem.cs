using AtomUI.Controls;

namespace AtomUI.Desktop.Controls;

public interface INavMenuItem: INavMenuElement
{
    /// <summary>
    /// Gets or sets a value that item key.
    /// </summary>
    TreeNodeKey? ItemKey { get; }
    
    INavMenuNode? Node { get; }
    
    /// <summary>
    /// Gets or sets a value that indicates whether the item has a submenu.
    /// </summary>
    bool HasSubMenu { get; }
    
    /// <summary>
    /// Gets a value indicating whether the mouse is currently over the menu item's submenu.
    /// </summary>
    bool IsPointerOverSubMenu { get; }
    
    /// <summary>
    /// Gets or sets a value that indicates whether the submenu of the <see cref="NavMenuItem"/> is
    /// open.
    /// </summary>
    bool IsSubMenuOpen { get; set; }
    
    /// <summary>
    /// Gets or sets a value that indicates the submenu that this <see cref="NavMenuItem"/> is
    /// within should not close when this item is clicked.
    /// </summary>
    bool StaysOpenOnClick { get; set; }
    
    /// <summary>
    /// Gets a value that indicates whether the <see cref="NavMenuItem"/> is a top-level main menu item.
    /// </summary>
    bool IsTopLevel { get; }
    
    /// <summary>
    /// Gets the parent <see cref="INavMenuElement"/>.
    /// </summary>
    INavMenuElement? Parent { get; }
    
    /// <summary>
    /// Opens the menu or menu item.
    /// </summary>
    void Open();
    
    /// <summary>
    /// Closes the menu or menu item.
    /// </summary>
    void Close();
}