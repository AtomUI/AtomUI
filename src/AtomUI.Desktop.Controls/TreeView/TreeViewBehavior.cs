using AtomUI.Controls;
using Avalonia.Input;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The semantic facts of a released gesture on a tree header.
/// </summary>
public readonly record struct TreeViewHeaderContext(KeyModifiers Modifiers, bool IsLeaf, bool IsExpanded,
    bool IsSelected, bool IsSelectable);

/// <summary>
/// Chooses whether an eligible header gesture activates a node.
/// </summary>
public class TreeViewBehavior : ControlBehavior<TreeView>
{
    /// <summary>
    /// Default tree view behavior instance used when no language override is registered.
    /// </summary>
    public static TreeViewBehavior Default { get; } = new();

    /// <summary>
    /// Determines whether a pointer press on a tree item header activates selection or expansion.
    /// </summary>
    public virtual bool ShouldActivateHeader(in TreeViewHeaderContext context) => true;
}
