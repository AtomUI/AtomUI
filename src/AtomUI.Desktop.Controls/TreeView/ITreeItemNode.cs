using AtomUI.Controls;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A node of the data that a <see cref="TreeView" /> presents. Besides the members of
/// <see cref="ITreeNode{TChild}" />, a node carries the state that the tree shows and writes back: selection,
/// expansion, the check state and the flags that control the check indicator and the leaf icon.
/// </summary>
public interface ITreeItemNode : ITreeNode<ITreeItemNode>
{
    /// <summary>
    /// The check state of the node: <see langword="true" /> for checked and <see langword="false" /> for unchecked.
    /// <see cref="TreeView.CheckedItems" /> owns the checked state and copies the state of a node into this
    /// property.
    /// </summary>
    bool? IsChecked { get; set; }
    /// <summary>
    /// Whether the node is selected. The <see cref="TreeView" /> writes its selection into this property.
    /// </summary>
    bool IsSelected { get; set; }
    /// <summary>
    /// Whether the node is expanded and shows its children. Keyboard navigation of the <see cref="TreeView" />
    /// reads the value to decide whether it may descend into the node.
    /// </summary>
    bool IsExpanded { get; set; }
    /// <summary>
    /// Whether the node shows its check indicator, the check box or radio button of the node. The
    /// <see cref="TreeView" /> treats a node without the indicator as not checkable.
    /// </summary>
    bool IsIndicatorEnabled { get; set; }
    /// <summary>
    /// The group of the radio button that serves as check indicator of the node. Nodes that share a group name
    /// accept only one checked node among them.
    /// </summary>
    string? GroupName { get; }
    /// <summary>
    /// Whether the node is a leaf, a node without children. A leaf node shows the leaf icon instead of the switcher
    /// that expands a node.
    /// </summary>
    bool IsLeaf { get; }
    /// <summary>
    /// A value that the application stores with the node.
    /// </summary>
    object? Value { get; set; }
    /// <summary>
    /// Whether the node accepts interaction. The presenting <see cref="TreeView" /> copies the value to its own
    /// <c>IsEnabled</c> property and skips disabled nodes during keyboard navigation. Declared again here to add a
    /// setter to the read-only member of <see cref="ITreeNode{TChild}" />.
    /// </summary>
    new bool IsEnabled { get; set; }

    /// <summary>
    /// Updates the parent of this node. It is called when the <see cref="TreeView" /> moves a node and when a node
    /// is added to or removed from the children of another node. The default implementation throws
    /// <c>NotImplementedException</c>, so every implementation must provide the method.
    /// </summary>
    /// <param name="parentNode">
    /// The new parent node, or <see langword="null" /> when the node no longer has a parent.
    /// </param>
    void UpdateParentNode(ITreeItemNode? parentNode) => throw new NotImplementedException();
}
