using Avalonia.Input;
using Avalonia.LogicalTree;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The common base of a <see cref="NavMenu" /> and of its items: an element of the menu tree that has menu items
/// directly below it.
/// </summary>
public interface INavMenuElement : IInputElement, ILogical
{
    /// <summary>
    /// Gets the submenu items.
    /// </summary>
    IEnumerable<INavMenuItem> SubItems { get; }
}