using AtomUI.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The contract of a <see cref="NavMenu" /> as its interaction handlers use it: the selected node, the default
/// open and selected paths, and closing the menu.
/// </summary>
public interface INavMenu : INavMenuElement
{
    /// <summary>
    /// Gets the currently selected node, or <see langword="null" /> when no node is selected.
    /// </summary>
    INavMenuNode? SelectedItem { get; }

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