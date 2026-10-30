namespace AtomUI.Desktop.Controls;

internal readonly record struct PaginationNavigationItem(
    PaginationItemType ItemType,
    int PageNumber,
    bool IsActive = false);

internal static class PaginationNavigationModel
{
    public static IReadOnlyList<PaginationNavigationItem> Build(
        int currentPage,
        int pageCount,
        bool showLessItems,
        bool showPrevNextJumpers)
    {
        if (pageCount <= 0)
        {
            return [];
        }

        currentPage = Math.Clamp(currentPage, 1, pageCount);
        var pageBufferSize = showLessItems ? 1 : 2;
        var items = new List<PaginationNavigationItem>();

        if (pageCount <= 3 + pageBufferSize * 2)
        {
            for (var page = 1; page <= pageCount; page++)
            {
                items.Add(Page(page, currentPage));
            }

            return items;
        }

        var jumpSize = showLessItems ? 3 : 5;
        var left = Math.Max(1, currentPage - pageBufferSize);
        var right = Math.Min(currentPage + pageBufferSize, pageCount);

        if (currentPage - 1 <= pageBufferSize)
        {
            right = 1 + pageBufferSize * 2;
        }

        if (pageCount - currentPage <= pageBufferSize)
        {
            left = pageCount - pageBufferSize * 2;
        }

        var hasJumpPrevious = showPrevNextJumpers &&
                              currentPage - 1 >= pageBufferSize * 2 &&
                              currentPage != 3;
        var hasJumpNext = showPrevNextJumpers &&
                          pageCount - currentPage >= pageBufferSize * 2 &&
                          currentPage != pageCount - 2;

        if (!showLessItems && hasJumpPrevious && right != pageCount)
        {
            left++;
        }

        if (!showLessItems && hasJumpNext && left != 1)
        {
            right--;
        }

        if (left != 1)
        {
            items.Add(Page(1, currentPage));
        }

        if (hasJumpPrevious)
        {
            items.Add(new PaginationNavigationItem(
                PaginationItemType.JumpPrevious,
                Math.Max(1, currentPage - jumpSize)));
        }

        for (var page = left; page <= right; page++)
        {
            items.Add(Page(page, currentPage));
        }

        if (hasJumpNext)
        {
            items.Add(new PaginationNavigationItem(
                PaginationItemType.JumpNext,
                Math.Min(pageCount, currentPage + jumpSize)));
        }

        if (right != pageCount)
        {
            items.Add(Page(pageCount, currentPage));
        }

        return items;
    }

    private static PaginationNavigationItem Page(int pageNumber, int currentPage)
    {
        return new PaginationNavigationItem(
            PaginationItemType.PageIndicator,
            pageNumber,
            pageNumber == currentPage);
    }
}
