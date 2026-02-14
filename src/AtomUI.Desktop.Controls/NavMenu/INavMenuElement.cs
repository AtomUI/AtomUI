using Avalonia.Input;
using Avalonia.LogicalTree;

namespace AtomUI.Desktop.Controls;

public interface INavMenuElement : IInputElement, ILogical
{
    /// <summary>
    /// Gets the submenu items.
    /// </summary>
    IEnumerable<INavMenuItem> SubItems { get; }
}