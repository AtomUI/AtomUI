using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The design tokens of the tree view. Both values are reserved for the directory-node mode of antd Tree, which
/// is not implemented.
/// </summary>
[ControlDesignToken]
public sealed partial class TreeViewToken : AbstractTreeViewToken
{
    /// <summary>
    /// Reserved foreground color for selected directory nodes, paired with
    /// <see cref="DirectoryNodeSelectedBg" />.
    /// </summary>
    [ReservedToken("antd Tree directory-node mode is not implemented.")]
    public partial Color DirectoryNodeSelectedColor { get; set; }

    /// <summary>
    /// Reserved background color for selected directory nodes, paired with
    /// <see cref="DirectoryNodeSelectedColor" />.
    /// </summary>
    [ReservedToken("antd Tree directory-node mode is not implemented.")]
    public partial Color DirectoryNodeSelectedBg { get; set; }
}
