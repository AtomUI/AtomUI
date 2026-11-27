namespace AtomUI.Desktop.Controls;

/// <summary>
/// Pseudo-class names that DataGrid controls set on themselves and on their parts, for use in style selectors.
/// </summary>
public static class DataGridPseudoClass
{
    /// <summary>
    /// Set on the grid while it has no entries.
    /// </summary>
    public const string EmptyRows = ":empty-rows";
    /// <summary>
    /// Set on the grid while it has no visible column.
    /// </summary>
    public const string EmptyColumns = ":empty-columns";
    /// <summary>
    /// Set on the header of the first visible column.
    /// </summary>
    public const string FirstColumnHeader = ":first-column-header";
    /// <summary>
    /// Set on the header of a visible column that is neither the first nor the last.
    /// </summary>
    public const string MiddleColumnHeader = ":middle-column-header";
    /// <summary>
    /// Set on the header of the last visible column.
    /// </summary>
    public const string LastColumnHeader = ":last-column-header";
    /// <summary>
    /// Set on a group header view item that presents a column rather than a column group.
    /// </summary>
    public const string GroupItemLeaf = ":is-leaf-item";
    /// <summary>
    /// Set on a row while the pointer is over it.
    /// </summary>
    public const string RowHover = ":row-hover";
}
