namespace AtomUI.Controls;

/// <summary>
/// Event data that reports the current page of a pagination together with the page layout it belongs to.
/// </summary>
public class PageChangedEventArgs
{
    /// <summary>
    /// Initializes the event data with the current page and the page layout.
    /// </summary>
    /// <param name="pageIndex">
    /// The current page, counted from 1.
    /// </param>
    /// <param name="totalPages">
    /// The number of pages.
    /// </param>
    /// <param name="pageSize">
    /// The number of items per page.
    /// </param>
    public PageChangedEventArgs(int pageIndex, int totalPages, int pageSize)
    {
        PageIndex  = pageIndex;
        TotalPages = totalPages;
        PageSize   = pageSize;
    }
    
    /// <summary>
    /// The number of items per page. It is the page size of the pagination, or the default of 10 when that size is
    /// not positive.
    /// </summary>
    public int PageSize { get; }
    /// <summary>
    /// The number of pages the items are divided into. It is 0 when there are no items.
    /// </summary>
    public int TotalPages { get; }
    /// <summary>
    /// The current page, counted from 1. The pagination has already clamped it to the page count.
    /// </summary>
    public int PageIndex { get; }
}