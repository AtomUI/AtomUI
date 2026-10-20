using System.Collections.Immutable;
using System.Collections.ObjectModel;
using AtomUI.Desktop.Controls.Tests.DataGrid.Data.Source;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.DataGrid.Virtualization;

public class DataGridPagingGroupingTests
{
    private static readonly DataGridFieldId ValueField = new("value");
    private static readonly DataGridFieldId GroupField = new("group");

    static DataGridPagingGroupingTests() => AvaloniaTestApp.EnsureInitialized();

    [Fact]
    public void PageSize_Pages_Source_Without_Reusing_Previous_Page_Data()
    {
        var source = Source();
        var grid = new global::AtomUI.Desktop.Controls.DataGrid
        {
            AutoGenerateColumns = false,
            PageSize = 10,
            PaginationVisibility = DataGridPaginationVisibility.All,
            Width = 460,
            Height = 260
        };
        grid.Columns.Add(new DataGridTextColumn
        {
            FieldId = ValueField,
            Binding = new Binding(nameof(Row.Value))
        });
        grid.ItemsSource = source;
        var window = Show(grid);
        try
        {
            source.RequestAt(0).Request.PageRequest.ShouldBe(new DataGridPageRequest(0, 10));
            Complete(source, 0, totalDataCount: 100, "s1");
            PumpUntil(() => grid.LoadState == DataGridLoadState.Ready);
            grid.DisplayData.GetDisplayedRow(0)!.DataIndex.ShouldBe(0);

            var bottom = Paginations(grid).Single(control => control.Name == "PART_BottomPagination");
            bottom.CurrentPage = 2;
            source.WaitForRequestCount(2);
            source.RequestAt(1).Request.PageRequest.ShouldBe(new DataGridPageRequest(10, 10));
            Paginations(grid).ShouldAllBe(control => control.CurrentPage == 1);

            Complete(source, 1, totalDataCount: 100, "s2");
            PumpUntil(() => grid.LoadState == DataGridLoadState.Ready);
            grid.DisplayData.GetDisplayedRow(0)!.DataIndex.ShouldBe(10);
            Paginations(grid).ShouldAllBe(control => control.CurrentPage == 2);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void PageRequest_Is_Sent_To_Source_And_Both_Pagination_Parts_Project_Applied_State()
    {
        var source = Source();
        var grid = Grid(source, new DataGridPageRequest(20, 10));
        var window = Show(grid);
        try
        {
            source.RequestAt(0).Request.PageRequest.ShouldBe(new DataGridPageRequest(20, 10));
            Complete(source, 0, totalDataCount: 100, "s1");
            PumpUntil(() => grid.LoadState == DataGridLoadState.Ready);

            grid.AppliedPageRequest.ShouldBe(new DataGridPageRequest(20, 10));
            foreach (var pagination in Paginations(grid))
            {
                pagination.Total.ShouldBe(100);
                pagination.PageSize.ShouldBe(10);
                pagination.CurrentPage.ShouldBe(3);
            }

            var bottom = Paginations(grid).Single(control => control.Name == "PART_BottomPagination");
            bottom.CurrentPage = 4;
            source.WaitForRequestCount(2);
            source.RequestAt(1).Request.PageRequest.ShouldBe(new DataGridPageRequest(30, 10));
            Paginations(grid).ShouldAllBe(control => control.CurrentPage == 3);

            Complete(source, 1, totalDataCount: 100, "s2");
            PumpUntil(() => grid.LoadState == DataGridLoadState.Ready);
            Paginations(grid).ShouldAllBe(control => control.CurrentPage == 4);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void IsShowPageSizeSelector_Forwards_To_Both_Pagination_Parts()
    {
        var source = Source();
        var grid = Grid(source, new DataGridPageRequest(0, 10));
        var window = Show(grid);
        try
        {
            grid.IsShowPageSizeSelector.ShouldBeTrue();
            Paginations(grid).ShouldAllBe(control => control.IsShowSizeChanger);

            grid.IsShowPageSizeSelector = false;
            Dispatcher.UIThread.RunJobs();
            Paginations(grid).ShouldAllBe(control => !control.IsShowSizeChanger);

            grid.IsShowPageSizeSelector = true;
            Dispatcher.UIThread.RunJobs();
            Paginations(grid).ShouldAllBe(control => control.IsShowSizeChanger);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Pagination_ExtraContent_Uses_Opposite_Sides_For_Start_And_End()
    {
        var source = Source();
        var grid = Grid(source, new DataGridPageRequest(0, 10));
        grid.PaginationVisibility = DataGridPaginationVisibility.All;
        grid.TopPaginationAlign = PaginationAlign.Start;
        grid.BottomPaginationAlign = PaginationAlign.End;
        grid.TopPaginationExtraContent = new Button { Content = "top" };
        grid.BottomPaginationExtraContent = new Button { Content = "bottom" };

        var window = Show(grid);
        try
        {
            Complete(source, 0, totalDataCount: 100, "s1");
            PumpUntil(() => grid.LoadState == DataGridLoadState.Ready);
            var topPanel = grid.GetVisualDescendants()
                               .OfType<DockPanel>()
                               .Single(control => control.Name == "PART_TopPaginationPanel");
            var bottomPanel = grid.GetVisualDescendants()
                                  .OfType<DockPanel>()
                                  .Single(control => control.Name == "PART_BottomPaginationPanel");
            var topExtra = topPanel.Children.OfType<ContentPresenter>()
                                  .Single(control => control.Name == "PART_TopPaginationExtraContentPresenter");
            var bottomExtra = bottomPanel.Children.OfType<ContentPresenter>()
                                        .Single(control => control.Name == "PART_BottomPaginationExtraContentPresenter");
            var topPagination = topPanel.Children.OfType<Pagination>().Single();
            var bottomPagination = bottomPanel.Children.OfType<Pagination>().Single();

            DockPanel.GetDock(topExtra).ShouldBe(Dock.Right);
            DockPanel.GetDock(bottomExtra).ShouldBe(Dock.Left);
            topPanel.Margin.Left.ShouldBeGreaterThan(0);
            topPanel.Margin.Right.ShouldBeGreaterThan(0);
            bottomPanel.Margin.Left.ShouldBeGreaterThan(0);
            bottomPanel.Margin.Right.ShouldBeGreaterThan(0);
            topExtra.Bounds.X.ShouldBeGreaterThan(topPagination.Bounds.X);
            bottomExtra.Bounds.X.ShouldBeLessThan(bottomPagination.Bounds.X);

            var topContent = topExtra.GetVisualDescendants().OfType<Button>().Single();
            var bottomContent = bottomExtra.GetVisualDescendants().OfType<Button>().Single();
            var topContentCenter = topContent.TranslatePoint(
                new Point(0, topContent.Bounds.Height / 2), topPanel).ShouldNotBeNull();
            var topPaginationCenter = topPagination.TranslatePoint(
                new Point(0, topPagination.Bounds.Height / 2), topPanel).ShouldNotBeNull();
            var bottomContentCenter = bottomContent.TranslatePoint(
                new Point(0, bottomContent.Bounds.Height / 2), bottomPanel).ShouldNotBeNull();
            var bottomPaginationCenter = bottomPagination.TranslatePoint(
                new Point(0, bottomPagination.Bounds.Height / 2), bottomPanel).ShouldNotBeNull();
            Math.Abs(topContentCenter.Y - topPaginationCenter.Y)
                .ShouldBeLessThan(1);
            Math.Abs(bottomContentCenter.Y - bottomPaginationCenter.Y)
                .ShouldBeLessThan(1);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Pagination_ExtraContent_Center_Keeps_Extra_Content_On_Right()
    {
        var source = Source();
        var grid = Grid(source, new DataGridPageRequest(0, 10));
        grid.PaginationVisibility = DataGridPaginationVisibility.All;
        grid.TopPaginationAlign = PaginationAlign.Center;
        grid.TopPaginationExtraContent = new TextBlock { Text = "top" };

        var window = Show(grid);
        try
        {
            Complete(source, 0, totalDataCount: 100, "s1");
            PumpUntil(() => grid.LoadState == DataGridLoadState.Ready);

            var topPanel = grid.GetVisualDescendants()
                               .OfType<DockPanel>()
                               .Single(control => control.Name == "PART_TopPaginationPanel");
            var topExtra = topPanel.Children.OfType<ContentPresenter>()
                                  .Single(control => control.Name == "PART_TopPaginationExtraContentPresenter");
            var topPagination = topPanel.Children.OfType<Pagination>().Single();

            DockPanel.GetDock(topExtra).ShouldBe(Dock.Right);
            topPagination.Align.ShouldBe(PaginationAlign.Center);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Theory]
    [InlineData(DataGridPaginationVisibility.None, false, false)]
    [InlineData(DataGridPaginationVisibility.Top, true, false)]
    [InlineData(DataGridPaginationVisibility.Bottom, false, true)]
    [InlineData(DataGridPaginationVisibility.All, true, true)]
    public void Pagination_ExtraContent_Honors_Pagination_Visibility(
        DataGridPaginationVisibility visibility,
        bool topVisible,
        bool bottomVisible)
    {
        var source = Source();
        var grid = Grid(source, new DataGridPageRequest(0, 10));
        grid.PaginationVisibility = visibility;
        grid.TopPaginationExtraContent = new TextBlock { Text = "top" };
        grid.BottomPaginationExtraContent = new TextBlock { Text = "bottom" };
        var window = Show(grid);
        try
        {
            Complete(source, 0, totalDataCount: 100, "s1");
            PumpUntil(() => grid.LoadState == DataGridLoadState.Ready);

            var panels = grid.GetVisualDescendants()
                             .OfType<DockPanel>()
                             .Where(control => control.Name is "PART_TopPaginationPanel" or "PART_BottomPaginationPanel")
                             .ToDictionary(control => control.Name!);
            panels["PART_TopPaginationPanel"].IsVisible.ShouldBe(topVisible);
            panels["PART_BottomPaginationPanel"].IsVisible.ShouldBe(bottomVisible);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Pagination_ExtraContent_Template_Replacement_Detaches_Old_Visual()
    {
        var source = Source();
        var grid = Grid(source, new DataGridPageRequest(0, 10));
        grid.TopPaginationExtraContent = "top";
        grid.TopPaginationExtraContentTemplate = new FuncDataTemplate<string>(
            (_, _) => new TextBlock { Name = "FirstExtraVisual", Text = "first" });
        var window = Show(grid);
        TextBlock? firstVisual = null;
        try
        {
            Complete(source, 0, totalDataCount: 100, "s1");
            PumpUntil(() => grid.LoadState == DataGridLoadState.Ready);
            firstVisual = grid.GetVisualDescendants()
                              .OfType<TextBlock>()
                              .Single(control => control.Name == "FirstExtraVisual");

            grid.TopPaginationExtraContentTemplate = new FuncDataTemplate<string>(
                (_, _) => new TextBlock { Name = "SecondExtraVisual", Text = "second" });
            Dispatcher.UIThread.RunJobs();

            firstVisual.IsAttachedToVisualTree().ShouldBeFalse();
            var secondVisual = grid.GetVisualDescendants()
                                   .OfType<TextBlock>()
                                   .Single(control => control.Name == "SecondExtraVisual");
            secondVisual.Text.ShouldBe("second");

            grid.SetValue(TemplatedControl.TemplateProperty, null);
            grid.ApplyTemplate();
            Dispatcher.UIThread.RunJobs();

            secondVisual.IsAttachedToVisualTree().ShouldBeFalse();

            grid.ClearValue(TemplatedControl.TemplateProperty);
            grid.ApplyTemplate();
            Dispatcher.UIThread.RunJobs();
            grid.GetVisualDescendants()
                .OfType<TextBlock>()
                .Single(control => control.Name == "SecondExtraVisual")
                .ShouldNotBeSameAs(secondVisual);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
            firstVisual?.IsAttachedToVisualTree().ShouldBeFalse();
        }
    }

    [Fact]
    public void Pagination_ExtraContent_Does_Not_Materialize_When_Content_Is_Unset()
    {
        var source = Source();
        var grid = Grid(source, new DataGridPageRequest(0, 10));
        var window = Show(grid);
        try
        {
            Complete(source, 0, totalDataCount: 100, "s1");
            PumpUntil(() => grid.LoadState == DataGridLoadState.Ready);

            var extraPresenters = grid.GetVisualDescendants()
                                      .OfType<ContentPresenter>()
                                      .Where(control => control.Name is "PART_TopPaginationExtraContentPresenter"
                                                          or "PART_BottomPaginationExtraContentPresenter");
            extraPresenters.ShouldAllBe(control => !control.GetVisualChildren().Any());
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Pagination_ExtraContent_Hides_With_Single_Page_Pagination()
    {
        var source = Source();
        var grid = Grid(source, new DataGridPageRequest(0, 10));
        grid.PaginationVisibility = DataGridPaginationVisibility.All;
        grid.IsHideOnSinglePage = true;
        grid.TopPaginationExtraContent = new TextBlock { Text = "top" };
        grid.BottomPaginationExtraContent = new TextBlock { Text = "bottom" };
        var window = Show(grid);
        try
        {
            Complete(source, 0, totalDataCount: 5, "s1");
            PumpUntil(() => grid.LoadState == DataGridLoadState.Ready);

            grid.GetVisualDescendants()
                .OfType<DockPanel>()
                .Where(control => control.Name is "PART_TopPaginationPanel" or "PART_BottomPaginationPanel")
                .ShouldAllBe(control => !control.IsVisible);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void PageSizeSelector_Change_Updates_PageRequest_And_Applied_State()
    {
        var source = Source();
        var grid = Grid(source, new DataGridPageRequest(20, 10));
        var window = Show(grid);
        try
        {
            Complete(source, 0, totalDataCount: 100, "s1");
            PumpUntil(() => grid.LoadState == DataGridLoadState.Ready);

            var bottom = Paginations(grid).Single(control => control.Name == "PART_BottomPagination");
            bottom.PageSize = 25;

            source.WaitForRequestCount(2);
            source.RequestAt(1).Request.PageRequest.ShouldBe(new DataGridPageRequest(50, 25));
            grid.PageRequest!.Value.DataStartIndex.ShouldBe(50);
            grid.PageRequest.Value.DataCount.ShouldBe(25);
            grid.PageSize.ShouldBe(25);

            Complete(source, 1, totalDataCount: 100, "s2");
            PumpUntil(() => grid.LoadState == DataGridLoadState.Ready);
            grid.AppliedPageRequest.ShouldBe(new DataGridPageRequest(50, 25));
            Paginations(grid).ShouldAllBe(control => control.PageSize == 25 && control.CurrentPage == 3);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void PageSizeSelector_Change_Clamps_Page_When_New_Size_Reduces_Page_Count()
    {
        var source = Source();
        var grid = Grid(source, new DataGridPageRequest(90, 10));
        var window = Show(grid);
        try
        {
            Complete(source, 0, totalDataCount: 100, "s1");
            PumpUntil(() => grid.LoadState == DataGridLoadState.Ready);

            var bottom = Paginations(grid).Single(control => control.Name == "PART_BottomPagination");
            bottom.PageSize = 100;

            source.WaitForRequestCount(2);
            source.RequestAt(1).Request.PageRequest.ShouldBe(new DataGridPageRequest(0, 100));
            grid.PageRequest!.Value.DataStartIndex.ShouldBe(0);
            grid.PageRequest.Value.DataCount.ShouldBe(100);

            Complete(source, 1, totalDataCount: 100, "s2");
            PumpUntil(() => grid.LoadState == DataGridLoadState.Ready);
            Paginations(grid).ShouldAllBe(control => control.PageSize == 100 && control.CurrentPage == 1);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Query_Reset_And_Page_Failure_Rollback_Are_Atomic()
    {
        var source = Source();
        var grid = Grid(source, new DataGridPageRequest(20, 10));
        var window = Show(grid);
        try
        {
            Complete(source, 0, 100, "s1");
            PumpUntil(() => grid.LoadState == DataGridLoadState.Ready);

            grid.PageRequest = new DataGridPageRequest(30, 10);
            source.WaitForRequestCount(2);
            source.Fail(1, new InvalidOperationException("page failed"));
            PumpUntil(() => grid.LoadState == DataGridLoadState.Error);

            grid.PageRequest.ShouldBe(new DataGridPageRequest(20, 10));
            grid.AppliedPageRequest.ShouldBe(new DataGridPageRequest(20, 10));
            Paginations(grid).ShouldAllBe(control => control.CurrentPage == 3);

            grid.SetSort(ValueField, DataGridSortDirection.Ascending);
            source.WaitForRequestCount(3);
            source.RequestAt(2).Request.PageRequest.ShouldBe(new DataGridPageRequest(0, 10));
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Total_Shrink_Redirects_To_Final_Legal_Page_Without_Empty_Commit()
    {
        var source = Source();
        var grid = Grid(source, new DataGridPageRequest(90, 10));
        var window = Show(grid);
        try
        {
            Complete(source, 0, 100, "s1");
            PumpUntil(() => grid.LoadState == DataGridLoadState.Ready);
            var originalFirst = grid.DisplayData.GetDisplayedRow(0)!.RowKey;

            source.RaiseInvalidated();
            source.WaitForRequestCount(2);
            Complete(source, 1, 25, "s2");
            PumpUntil(() => source.Requests.Count >= 3);

            grid.LoadState.ShouldBe(DataGridLoadState.Refreshing);
            grid.AppliedPageRequest.ShouldBe(new DataGridPageRequest(90, 10));
            grid.DisplayData.GetDisplayedRow(0)!.RowKey.ShouldBe(originalFirst);
            source.RequestAt(2).Request.PageRequest.ShouldBe(new DataGridPageRequest(20, 10));

            Complete(source, 2, 25, "s2");
            PumpUntil(() => grid.LoadState == DataGridLoadState.Ready);
            grid.PageRequest.ShouldBe(new DataGridPageRequest(20, 10));
            grid.AppliedPageRequest.ShouldBe(new DataGridPageRequest(20, 10));
            grid.TotalItemCount.ShouldBe(25);
            grid.TotalEntryCount.ShouldBe(5);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void PageRequest_Preserves_Long_DataIndex_Beyond_Int32_Domain()
    {
        var source = Source();
        var start = (long)int.MaxValue + 10;
        var grid = Grid(source, new DataGridPageRequest(start, 1));
        var window = Show(grid);
        try
        {
            Complete(source, 0, start + 1, "s1");
            PumpUntil(() => grid.LoadState == DataGridLoadState.Ready);

            source.RequestAt(0).Request.PageRequest!.Value.DataStartIndex.ShouldBe(start);
            grid.DisplayData.GetDisplayedRow(0)!.DataIndex.ShouldBe(start);
            grid.TotalItemCount.ShouldBe(start + 1);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void GroupExpansion_Preserves_Acted_Header_Screen_Position()
    {
        var rows = Enumerable.Range(0, 90)
            .Select(index => new Row(index, $"g{index / 30}"))
            .ToArray();
        using var source = LocalSource(rows, preferredRangeSize: 32);
        var grid = LocalGrid(source, grouped: true);
        var window = Show(grid);
        try
        {
            PumpUntil(() => grid.LoadState == DataGridLoadState.Ready);
            grid.RequestRangeViewport(29, 8, 1);
            PumpUntil(() =>
                grid.LoadState == DataGridLoadState.Ready &&
                grid.DisplayData.FirstScrollingSlot == 29);

            var displayedElements = grid.DisplayData.GetScrollingElements().ToArray();
            var headers = displayedElements.OfType<DataGridRowGroupHeader>().ToArray();
            headers.Length.ShouldBe(
                1,
                $"first={grid.DisplayData.FirstScrollingSlot}, last={grid.DisplayData.LastScrollingSlot}, " +
                $"elements={string.Join(',', displayedElements.Select(static element => element.GetType().Name))}");
            var header = headers[0];
            var key = header.SourceGroup!.Key;
            var originalScreenY = grid.GetRangeOffset(header.DisplaySlot) - grid.VerticalOffset;

            grid.CollapseGroup(key);
            PumpUntil(() => grid.LoadState == DataGridLoadState.Ready);

            var restored = grid.DisplayData.GetScrollingElements()
                .OfType<DataGridRowGroupHeader>()
                .Single(candidate => candidate.SourceGroup!.Key == key);
            (grid.GetRangeOffset(restored.DisplaySlot) - grid.VerticalOffset)
                .ShouldBe(originalScreenY, tolerance: 0.001);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Invalidation_Restores_First_Complete_Row_By_Key()
    {
        var rows = new ObservableCollection<Row>(Enumerable.Range(0, 80)
            .Select(index => new Row(index, "g")));
        using var source = LocalSource(rows, preferredRangeSize: 32);
        var grid = LocalGrid(source);
        var window = Show(grid);
        try
        {
            PumpUntil(() => grid.LoadState == DataGridLoadState.Ready);
            grid.RequestRangeViewport(20, 6, 1);
            PumpUntil(() =>
                grid.LoadState == DataGridLoadState.Ready &&
                grid.DisplayData.FirstScrollingSlot == 20);

            PumpUntil(() => !grid.HasPendingRangeViewport);
            var anchor = grid.DisplayData.GetScrollingElements()
                .OfType<DataGridRow>()
                .First(row => grid.GetRangeOffset(row.Slot) >= grid.VerticalOffset - 0.001);
            var anchorKey = anchor.RowKey;
            var anchorSlot = anchor.Slot;
            var originalFirst = grid.DisplayData.FirstScrollingSlot;
            var originalLast = grid.DisplayData.LastScrollingSlot;
            var originalOffset = grid.VerticalOffset;
            var originalKeys = string.Join(',', grid.DisplayData.GetScrollingElements()
                .OfType<DataGridRow>()
                .Select(row => $"{row.Slot}:{row.RowKey}"));
            var originalScreenY = grid.GetRangeOffset(anchor.Slot) - grid.VerticalOffset;

            rows.Insert(0, new Row(10_000, "g"));
            PumpUntil(() => grid.LoadState == DataGridLoadState.Ready && !grid.IsDataStale);

            var displayedRows = grid.DisplayData.GetScrollingElements()
                .OfType<DataGridRow>()
                .ToArray();
            var restored = displayedRows.SingleOrDefault(row => row.RowKey == anchorKey);
            restored.ShouldNotBeNull(
                $"anchorSlot={anchorSlot}, anchorKey={anchorKey}, originalY={originalScreenY}, " +
                $"originalFirst={originalFirst}, originalLast={originalLast}, originalOffset={originalOffset}, " +
                $"originalKeys={originalKeys}; " +
                $"first={grid.DisplayData.FirstScrollingSlot}, last={grid.DisplayData.LastScrollingSlot}, " +
                $"offset={grid.VerticalOffset}, keys={string.Join(',', displayedRows.Select(row => row.RowKey))}");
            (grid.GetRangeOffset(restored.Slot) - grid.VerticalOffset)
                .ShouldBe(originalScreenY, tolerance: 0.001);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void RowDetails_State_Follows_RowKey_Across_Invalidation_And_Uses_Sparse_Height()
    {
        var rows = new ObservableCollection<Row>(Enumerable.Range(0, 40)
            .Select(index => new Row(index, "g")));
        using var source = LocalSource(rows, preferredRangeSize: 32);
        var grid = LocalGrid(source, withDetails: true);
        var window = Show(grid);
        try
        {
            PumpUntil(() => grid.LoadState == DataGridLoadState.Ready);
            var expanded = (DataGridRow)grid.DisplayData.GetDisplayedElement(0);
            var expandedKey = expanded.RowKey;
            expanded.IsDetailsVisible = true;
            Dispatcher.UIThread.RunJobs();
            grid.GetRowDetailsVisibility(0).ShouldBeTrue();

            rows.Insert(0, new Row(10_000, "g"));
            PumpUntil(() => grid.LoadState == DataGridLoadState.Ready && !grid.IsDataStale);
            grid.RequestRangeViewport(0, 4, 0);
            PumpUntil(() =>
                grid.LoadState == DataGridLoadState.Ready &&
                grid.DisplayData.LastScrollingSlot >= 1);

            var displayedRows = grid.DisplayData.GetScrollingElements()
                .OfType<DataGridRow>()
                .ToArray();
            var inserted = displayedRows.SingleOrDefault(row => row.RowKey == DataGridRowKey.FromInt64(10_001));
            var restored = displayedRows.SingleOrDefault(row => row.RowKey == expandedKey);
            inserted.ShouldNotBeNull(
                $"first={grid.DisplayData.FirstScrollingSlot}, last={grid.DisplayData.LastScrollingSlot}, " +
                $"keys={string.Join(',', displayedRows.Select(row => row.RowKey))}");
            restored.ShouldNotBeNull(
                $"first={grid.DisplayData.FirstScrollingSlot}, last={grid.DisplayData.LastScrollingSlot}, " +
                $"keys={string.Join(',', displayedRows.Select(row => row.RowKey))}");
            inserted.RowKey.ShouldNotBe(expandedKey);
            inserted.IsDetailsVisible.ShouldBeFalse();
            restored.RowKey.ShouldBe(expandedKey);
            restored.IsDetailsVisible.ShouldBeTrue();
            grid.RangeMeasuredHeightCount.ShouldBeGreaterThan(0);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Visible_RowDetails_Estimate_Contributes_To_All_Unrealized_Rows()
    {
        var rows = Enumerable.Range(0, 1_000)
            .Select(index => new Row(index, "g"))
            .ToArray();
        using var source = LocalSource(rows, preferredRangeSize: 32);
        var grid = LocalGrid(source, withDetails: true);
        grid.Columns.RemoveAt(0);
        grid.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.Visible;
        var window = Show(grid);
        try
        {
            PumpUntil(() =>
                grid.LoadState == DataGridLoadState.Ready &&
                grid.RowDetailsHeightEstimate > 0);

            grid.RangeMeasuredHeightCount.ShouldBeLessThan(grid.SlotCount);
            grid.GetRangeExtent().ShouldBeGreaterThanOrEqualTo(
                grid.SlotCount * (grid.RowHeight + grid.RowDetailsHeightEstimate) - 1);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static ControllableDataGridSource Source() => new(new DataGridSourceSchema(
        typeof(Row),
        [new DataGridFieldSchema(
            ValueField,
            typeof(long),
            DataGridSortDirections.All,
            ImmutableArray<DataGridFilterOperatorSchema>.Empty,
            canGroup: false)],
        preferredRangeSize: 32,
        maximumRangeSize: 32));

    private static DataGridLocalSource<Row> LocalSource(
        IReadOnlyList<Row> rows,
        int preferredRangeSize)
    {
        var descriptor = DataGridLocalSourceDescriptor.For<Row>(
                static row => DataGridRowKey.FromInt64(row.Value + 1))
            .Field(ValueField, static row => row.Value)
            .Field(GroupField, static row => row.Group, canGroup: true);
        return DataGridLocalSource.Create(
            rows,
            descriptor,
            new DataGridLocalSourceOptions
            {
                PreferredRangeSize = preferredRangeSize,
                MaximumRangeSize = 64
            });
    }

    private static global::AtomUI.Desktop.Controls.DataGrid LocalGrid(
        IDataGridSource source,
        bool grouped = false,
        bool withDetails = false)
    {
        var grid = new global::AtomUI.Desktop.Controls.DataGrid
        {
            AutoGenerateColumns = false,
            Query = grouped
                ? DataGridQuery.Empty.WithGroups(
                    [new DataGridGroup(GroupField, DataGridSortDirection.Ascending)])
                : DataGridQuery.Empty,
            ItemsSource = source,
            RowHeight = 32,
            Width = 460,
            Height = 180,
            IsMotionEnabled = false
        };
        if (withDetails)
        {
            grid.Columns.Add(new DataGridDetailExpanderColumn());
            grid.RowDetailsTemplate = new FuncDataTemplate<Row>((row, _) => new Border
            {
                Height = 48,
                Child = new TextBlock { Text = row?.Value.ToString() }
            });
        }
        grid.Columns.Add(new DataGridTextColumn
        {
            FieldId = ValueField,
            Binding = new Binding(nameof(Row.Value))
        });
        return grid;
    }

    private static global::AtomUI.Desktop.Controls.DataGrid Grid(
        IDataGridSource source,
        DataGridPageRequest pageRequest)
    {
        var grid = new global::AtomUI.Desktop.Controls.DataGrid
        {
            AutoGenerateColumns = false,
            ItemsSource = source,
            PageRequest = pageRequest,
            PaginationVisibility = DataGridPaginationVisibility.All,
            Width = 460,
            Height = 260
        };
        grid.Columns.Add(new DataGridTextColumn
        {
            FieldId = ValueField,
            Binding = new Binding(nameof(Row.Value))
        });
        return grid;
    }

    private static Window Show(global::AtomUI.Desktop.Controls.DataGrid grid)
    {
        var window = new Window { Width = 540, Height = 340, Content = grid };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static Pagination[] Paginations(global::AtomUI.Desktop.Controls.DataGrid grid) =>
        grid.GetVisualDescendants().OfType<Pagination>().ToArray();

    private static void Complete(
        ControllableDataGridSource source,
        int requestIndex,
        long totalDataCount,
        string snapshot)
    {
        var request = source.RequestAt(requestIndex).Request;
        var page = request.PageRequest;
        var pageStart = page?.DataStartIndex ?? 0;
        var windowCount = page is null
            ? checked((int)totalDataCount)
            : (int)Math.Min(page.Value.DataCount, Math.Max(0, totalDataCount - pageStart));
        var resultCount = Math.Min(
            request.Range.Count,
            Math.Max(0, windowCount - request.Range.StartIndex));
        var entries = ImmutableArray.CreateBuilder<DataGridSourceEntry>(resultCount);
        for (var offset = 0; offset < resultCount; offset++)
        {
            var windowIndex = request.Range.StartIndex + offset;
            var dataIndex = checked(pageStart + windowIndex);
            entries.Add(DataGridSourceEntry.CreateData(
                DataGridRowKey.FromInt64(checked(dataIndex + 1)),
                new Row(dataIndex),
                windowIndex,
                dataIndex));
        }
        source.Complete(requestIndex, new DataGridRangeResult(
            request.Range.StartIndex,
            entries.MoveToImmutable(),
            windowCount,
            windowCount,
            totalDataCount,
            new DataGridSnapshotId(snapshot)));
    }

    private static void PumpUntil(Func<bool> condition)
    {
        if (!SpinWait.SpinUntil(() =>
            {
                Dispatcher.UIThread.RunJobs();
                return condition();
            }, TimeSpan.FromSeconds(5)))
        {
            throw new TimeoutException("The expected grid state was not reached.");
        }
    }

    private sealed record Row(long Value, string Group = "");
}
