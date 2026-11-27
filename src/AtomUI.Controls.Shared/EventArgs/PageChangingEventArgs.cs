using System.ComponentModel;

namespace AtomUI.Controls;

/// <summary>
/// Event data for a request to move a paged collection view to another page. It is raised before the page index
/// changes; setting <see cref="CancelEventArgs.Cancel" /> to <see langword="true" /> keeps the current page.
/// </summary>
public sealed class PageChangingEventArgs : CancelEventArgs
{
    /// <summary>
    /// Initializes the event data for a request to move to another page.
    /// </summary>
    /// <param name="newPageIndex">
    /// The zero-based index of the requested page.
    /// </param>
    public PageChangingEventArgs(int newPageIndex)
    {
        NewPageIndex = newPageIndex;
    }

    /// <summary>
    /// The zero-based index of the requested page.
    /// </summary>
    public int NewPageIndex
    {
        get;
        private set;
    }
}