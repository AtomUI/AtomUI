namespace AtomUI.Desktop.Controls;

/// <summary>
/// Chooses which selected entries are shown as tags in the multiple-selection input when a parent and its
/// children are selected together. It changes the tags only, not the stored selection. Set it through
/// <see cref="TreeSelect.ShowCheckedStrategy" /> and <see cref="Cascader.ShowCheckedStrategy" />.
/// </summary>
[Flags]
public enum TreeSelectCheckedStrategy
{
    /// <summary>
    /// Shows a selected parent in place of the selected entries below it, once all of its children are selected.
    /// </summary>
    ShowParent = 0x1,
    /// <summary>
    /// Shows only selected entries that have no children.
    /// </summary>
    ShowChild = 0x2,
    /// <summary>
    /// Shows every selected entry, parents and children alike. This is the default.
    /// </summary>
    All = ShowParent | ShowChild,
}