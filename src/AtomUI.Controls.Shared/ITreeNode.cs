using Avalonia.Controls;

namespace AtomUI.Controls;

/// <summary>
/// A node of the hierarchical item data that a <c>TreeView</c>, a menu, a <c>NavMenu</c> or a cascader presents.
/// The control that presents the data creates one item per node from the values of this interface.
/// </summary>
/// <typeparam name="TChild">
/// The node type of the children, which is normally the implementing type itself.
/// </typeparam>
public interface ITreeNode<TChild> : IItemKey
    where TChild : class, ITreeNode<TChild>
{
    /// <summary>
    /// The parent node, or <see langword="null" /> when the node is a root node.
    /// </summary>
    ITreeNode<TChild>? ParentNode { get; }
    /// <summary>
    /// The content that the presenting control shows as the caption of the node.
    /// </summary>
    object? Header { get; }
    /// <summary>
    /// The icon shown in front of the caption, or <see langword="null" /> for no icon. The presenting control copies
    /// the value to its own <c>Icon</c> property.
    /// </summary>
    PathIcon? Icon { get; }
    /// <summary>
    /// Whether the node accepts interaction. The presenting control copies the value to its own <c>IsEnabled</c>
    /// property, and keyboard navigation in a <c>TreeView</c> skips disabled nodes.
    /// </summary>
    bool IsEnabled { get; }
    /// <summary>
    /// The child nodes in display order, empty for a node without children.
    /// </summary>
    IEnumerable<TChild> Children { get; }
}