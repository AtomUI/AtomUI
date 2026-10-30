using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Pagination;

public class PaginationNavigationModelTests
{
    [Theory]
    [InlineData(1, "PageIndicator:1:True|PageIndicator:2:False|PageIndicator:3:False|PageIndicator:4:False|PageIndicator:5:False|JumpNext:6:False|PageIndicator:50:False")]
    [InlineData(4, "PageIndicator:1:False|PageIndicator:2:False|PageIndicator:3:False|PageIndicator:4:True|PageIndicator:5:False|JumpNext:9:False|PageIndicator:50:False")]
    [InlineData(5, "PageIndicator:1:False|JumpPrevious:1:False|PageIndicator:4:False|PageIndicator:5:True|PageIndicator:6:False|JumpNext:10:False|PageIndicator:50:False")]
    [InlineData(6, "PageIndicator:1:False|JumpPrevious:1:False|PageIndicator:5:False|PageIndicator:6:True|PageIndicator:7:False|JumpNext:11:False|PageIndicator:50:False")]
    [InlineData(48, "PageIndicator:1:False|JumpPrevious:43:False|PageIndicator:46:False|PageIndicator:47:False|PageIndicator:48:True|PageIndicator:49:False|PageIndicator:50:False")]
    [InlineData(50, "PageIndicator:1:False|JumpPrevious:45:False|PageIndicator:46:False|PageIndicator:47:False|PageIndicator:48:False|PageIndicator:49:False|PageIndicator:50:True")]
    public void Default_Navigation_Keeps_Seven_Items_And_Matches_Ant_Design(
        int currentPage,
        string expected)
    {
        var items = PaginationNavigationModel.Build(
            currentPage,
            pageCount: 50,
            showLessItems: false,
            showPrevNextJumpers: true);

        Format(items).ShouldBe(expected);
    }

    [Fact]
    public void Seven_Or_Fewer_Pages_Are_Displayed_Without_Jump_Items()
    {
        var items = PaginationNavigationModel.Build(
            currentPage: 4,
            pageCount: 7,
            showLessItems: false,
            showPrevNextJumpers: true);

        Format(items).ShouldBe(
            "PageIndicator:1:False|PageIndicator:2:False|PageIndicator:3:False|PageIndicator:4:True|PageIndicator:5:False|PageIndicator:6:False|PageIndicator:7:False");
    }

    [Fact]
    public void Less_Items_Uses_One_Page_Buffer_And_Three_Page_Jump_Targets()
    {
        var items = PaginationNavigationModel.Build(
            currentPage: 6,
            pageCount: 50,
            showLessItems: true,
            showPrevNextJumpers: true);

        Format(items).ShouldBe(
            "PageIndicator:1:False|JumpPrevious:3:False|PageIndicator:5:False|PageIndicator:6:True|PageIndicator:7:False|JumpNext:9:False|PageIndicator:50:False");
    }

    [Fact]
    public void Hidden_Jumpers_Leave_Buffered_Page_Numbers_And_Boundary_Pages()
    {
        var items = PaginationNavigationModel.Build(
            currentPage: 6,
            pageCount: 50,
            showLessItems: false,
            showPrevNextJumpers: false);

        Format(items).ShouldBe(
            "PageIndicator:1:False|PageIndicator:4:False|PageIndicator:5:False|PageIndicator:6:True|PageIndicator:7:False|PageIndicator:8:False|PageIndicator:50:False");
    }

    private static string Format(IEnumerable<PaginationNavigationItem> items)
    {
        return string.Join(
            "|",
            items.Select(item => $"{item.ItemType}:{item.PageNumber}:{item.IsActive}"));
    }
}
