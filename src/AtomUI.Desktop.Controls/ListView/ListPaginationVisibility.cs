namespace AtomUI.Desktop.Controls;

/// <summary>
/// Chooses which of the two pagers of a list are visible.
/// </summary>
public enum ListPaginationVisibility
{
    /// <summary>
    /// Hides both pagers.
    /// </summary>
    None,
    /// <summary>
    /// Shows only the top pager (<see cref="ListView.TopPagination" />).
    /// </summary>
    Top,
    /// <summary>
    /// Shows only the bottom pager (<see cref="ListView.BottomPagination" />).
    /// </summary>
    Bottom,
    /// <summary>
    /// Shows both pagers.
    /// </summary>
    Both
}