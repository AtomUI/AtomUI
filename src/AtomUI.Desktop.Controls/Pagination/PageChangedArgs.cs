namespace AtomUI.Desktop.Controls;

public class PageChangedEventArgs
{
    public PageChangedEventArgs(int pageIndex, int totalPages, int pageSize)
    {
        PageIndex  = pageIndex;
        TotalPages = totalPages;
        PageSize   = pageSize;
    }
    
    public int PageSize { get; }
    public int TotalPages { get; }
    public int PageIndex { get; }
}