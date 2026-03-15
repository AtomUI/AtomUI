using System.ComponentModel;

namespace AtomUI.Controls;

public sealed class PageChangingEventArgs : CancelEventArgs
{
    /// <summary>
    /// Constructor that takes the target page index
    /// </summary>
    /// <param name="newPageIndex">Index of the requested page</param>
    public PageChangingEventArgs(int newPageIndex)
    {
        NewPageIndex = newPageIndex;
    }

    /// <summary>
    /// Gets the index of the requested page
    /// </summary>
    public int NewPageIndex
    {
        get;
        private set;
    }
}