namespace AtomUI.Desktop.Controls;

/// <summary>
/// The pseudo classes of tree nodes and their headers: which check indicator a node shows, and whether it is
/// the keyboard active node.
/// </summary>
public class TreeViewPseudoClass
{
    /// <summary>
    /// Set on a node and on its header while the node's <c>ItemToggleType</c> is <c>Radio</c>.
    /// </summary>
    public const string NodeToggleTypeRadio = ":radio";
    /// <summary>
    /// Set on a node and on its header while the node's <c>ItemToggleType</c> is <c>CheckBox</c>.
    /// </summary>
    public const string NodeToggleTypeCheckBox = ":toggle";
    /// <summary>
    /// Set on a node and on its header while the node is the keyboard active node of the tree view.
    /// </summary>
    public const string KeyboardActive = ":keyboard-active";
}