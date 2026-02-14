using AtomUI.Desktop.Controls.Primitives;
using Avalonia.Rendering;

namespace AtomUI.Desktop.Controls;

public interface INavMenu : INavMenuElement
{
    /// <summary>
    /// Gets or sets the currently selected submenu item.
    /// </summary>
    INavMenuNode? SelectedItem { get; }
    
    /// <summary>
    /// Gets the root of the visual tree, if the control is attached to a visual tree.
    /// </summary>
    IRenderRoot? VisualRoot { get; }
    
    /// <summary>
    /// List of paths opened by default
    /// </summary>
    IList<TreeNodePath>? DefaultOpenPaths { get; set; }
    
    /// <summary>
    /// Default selected path
    /// </summary>
    TreeNodePath? DefaultSelectedPath { get; set; }
    
    /// <summary>
    /// Close all nodes
    /// </summary>
    void Close();
}