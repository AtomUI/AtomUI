using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The design tokens that the tree view adds to the shared tree presentation tokens: the width of the drop
/// indicator line shown while a node is dragged. The tree view token and the transfer tree view token derive
/// from this class.
/// </summary>
[ControlDesignToken]
public abstract partial class AbstractTreeViewToken : AbstractTreePresentationToken
{






    /// <summary>
    /// Thickness of the drop indicator line shown while dragging a tree node.
    /// </summary>







    public partial double DragIndicatorLineWidth { get; set; }

}
