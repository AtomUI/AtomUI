using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.DataGrid.Virtualization;

public class DataGridRowHeaderRecycleIssue475Tests
{
    static DataGridRowHeaderRecycleIssue475Tests() => AvaloniaTestApp.EnsureInitialized();

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ScrollingBeyondFirstViewportAndBack_ShouldKeepRowNumbers(bool frozenColumns)
    {
        using var source = CreateSource();
        var grid = CreateGrid(source);
        grid.RowHeaderContentTemplate = new FuncDataTemplate<Row>((_, _) => new TextBlock
        {
            [!TextBlock.TextProperty] = new Binding(nameof(DataGridRow.LogicIndex))
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor)
                {
                    AncestorType = typeof(DataGridRow)
                }
            }
        });
        if (frozenColumns)
        {
            grid.LeftFrozenColumnCount = 1;
            grid.RightFrozenColumnCount = 1;
        }
        var window = Show(grid);
        try
        {
            AssertHeaders(grid, row => row.Id.ToString());
            var initialViewportHeight = grid.CellsEstimatedHeight;

            foreach (var fraction in new[] { 0.4, 0.8, 1.0, 0.0 })
            {
                ScrollTo(grid, fraction);
                AssertHeaders(grid, row => row.Id.ToString());
                grid.Bounds.Height.ShouldBe(400);
                grid.CellsEstimatedHeight.ShouldBe(initialViewportHeight, tolerance: 0.1);
                grid.VerticalScrollBar!.IsVisible.ShouldBeTrue();
                grid.HorizontalScrollBar!.IsVisible.ShouldBeTrue();
            }

            DisplayedRows(grid).Min(row => ((Row)row.DataContext!).Id).ShouldBe(1);
        }
        finally
        {
            Close(window);
        }
    }

    [Fact]
    public void RecycledHeader_ShouldBindCurrentItemAndReleasePreviousItem()
    {
        using var source = CreateSource();
        var grid = CreateGrid(source);
        grid.RowHeaderContentTemplate = ItemHeaderTemplate();
        var window = Show(grid);
        try
        {
            AssertHeaders(grid, row => row.Label);
            var initialRows = DisplayedRows(grid);
            var initialHeaders = initialRows.ToDictionary(row => row, HeaderText);

            ScrollTo(grid, 0.5);

            var detachedRows = initialRows.Where(row => row.OwningGrid is null).ToArray();
            detachedRows.ShouldNotBeEmpty();
            foreach (var row in detachedRows)
            {
                initialHeaders[row].DataContext.ShouldBeNull();
                row.Header.ShouldBeNull();
            }
            AssertHeaders(grid, row => row.Label);

            var reusedRows = new HashSet<DataGridRow>();
            foreach (var fraction in new[] { 1.0, 0.0 })
            {
                foreach (var row in DisplayedRows(grid))
                {
                    initialHeaders.TryAdd(row, HeaderText(row));
                }
                ScrollTo(grid, fraction);
                AssertHeaders(grid, row => row.Label);
                foreach (var row in DisplayedRows(grid).Where(initialHeaders.ContainsKey))
                {
                    reusedRows.Add(row);
                    HeaderText(row).ShouldBeSameAs(initialHeaders[row]);
                }
            }
            reusedRows.ShouldNotBeEmpty();
        }
        finally
        {
            Close(window);
        }
    }

    [Theory]
    [InlineData(DataGridRowDetailsVisibilityMode.Collapsed)]
    [InlineData(DataGridRowDetailsVisibilityMode.Visible)]
    public void ChangingHeaderTemplate_ShouldUpdateRowsWithoutChangingDetailsLifecycle(
        DataGridRowDetailsVisibilityMode detailsVisibility)
    {
        using var source = CreateSource();
        var grid = CreateGrid(source);
        grid.RowHeaderContentTemplate = ItemHeaderTemplate();
        grid.RowDetailsTemplate = new FuncDataTemplate<Row>((_, _) => new Border { Height = 30 });
        grid.RowDetailsVisibilityMode = detailsVisibility;
        var unloadedDetails = 0;
        grid.UnloadingRowDetails += (_, _) => unloadedDetails++;
        var window = Show(grid);
        try
        {
            var rows = DisplayedRows(grid);
            var oldHeaders = rows.Select(HeaderText).ToArray();
            grid.RowHeaderContentTemplate = new FuncDataTemplate<Row>((_, _) => new TextBlock { Text = "new" });
            Dispatcher.UIThread.RunJobs();
            unloadedDetails.ShouldBe(0);
            AssertHeaders(grid, _ => "new");
            foreach (var header in oldHeaders)
            {
                header.DataContext.ShouldBeNull();
                header.GetVisualParent().ShouldBeNull();
            }

            grid.RowHeaderContentTemplate = ItemHeaderTemplate();
            Dispatcher.UIThread.RunJobs();
            AssertHeaders(grid, row => row.Label);
            unloadedDetails.ShouldBe(0);
        }
        finally
        {
            Close(window);
        }
    }

    [Theory]
    [InlineData(DataGridHeadersVisibility.All)]
    [InlineData(DataGridHeadersVisibility.Column)]
    public void ClearingHeaderTemplate_ShouldReleaseGeneratedContent(DataGridHeadersVisibility headersVisibility)
    {
        using var source = CreateSource();
        var grid = CreateGrid(source);
        grid.RowHeaderContentTemplate = ItemHeaderTemplate();
        var window = Show(grid);
        try
        {
            var oldHeaders = DisplayedRows(grid).Select(HeaderText).ToArray();
            grid.HeadersVisibility = headersVisibility;
            grid.RowHeaderContentTemplate = null;
            Dispatcher.UIThread.RunJobs();
            foreach (var row in DisplayedRows(grid))
            {
                row.Header.ShouldBeNull();
                row.HeaderCell!.Content.ShouldBeNull();
            }
            foreach (var header in oldHeaders)
            {
                header.DataContext.ShouldBeNull();
                header.GetVisualParent().ShouldBeNull();
            }
        }
        finally
        {
            Close(window);
        }
    }

    [Fact]
    public void NonRecyclingHeaderTemplate_ShouldBuildForCurrentItemAfterScrolling()
    {
        using var source = CreateSource();
        var grid = CreateGrid(source);
        grid.RowHeaderContentTemplate = new FuncDataTemplate<Row>((item, _) =>
            new TextBlock { Text = item!.Label });
        var window = Show(grid);
        try
        {
            AssertHeaders(grid, row => row.Label);
            foreach (var fraction in new[] { 0.4, 0.8, 1.0, 0.0 })
            {
                ScrollTo(grid, fraction);
                AssertHeaders(grid, row => row.Label);
            }
        }
        finally
        {
            Close(window);
        }
    }

    [Fact]
    public void ReapplyingUnchangedRowTemplate_ShouldPreserveExplicitHeader()
    {
        using var source = CreateSource();
        var grid = CreateGrid(source);
        var window = Show(grid);
        try
        {
            var row = DisplayedRows(grid)[0];
            row.HeaderContentTemplate = ItemHeaderTemplate();
            Dispatcher.UIThread.RunJobs();
            row.Header = "custom";

            grid.RowHeaderContentTemplate = ItemHeaderTemplate();
            Dispatcher.UIThread.RunJobs();
            row.Header.ShouldBe("custom");
            row.HeaderCell!.Content.ShouldBe("custom");

            grid.HeadersVisibility = DataGridHeadersVisibility.Column;
            grid.HeadersVisibility = DataGridHeadersVisibility.All;
            Dispatcher.UIThread.RunJobs();
            row.Header.ShouldBe("custom");
            row.HeaderCell.Content.ShouldBe("custom");
        }
        finally
        {
            Close(window);
        }
    }

    private static IDataTemplate ItemHeaderTemplate() => new FuncDataTemplate<Row>((_, _) => new TextBlock
    {
        [!TextBlock.TextProperty] = new Binding(nameof(Row.Label))
    }, supportsRecycling: true);

    private static DataGridLocalSource<Row> CreateSource() => DataGridLocalSource.Create(
        Enumerable.Range(1, 100).Select(id => new Row(id, $"Row {id}")).ToArray(),
        DataGridLocalSourceDescriptor.For<Row>(row => DataGridRowKey.FromInt64(row.Id)));

    private static global::AtomUI.Desktop.Controls.DataGrid CreateGrid(IDataGridSource source)
    {
        var grid = new global::AtomUI.Desktop.Controls.DataGrid
        {
            AutoGenerateColumns = false,
            IsMotionEnabled = false,
            ItemsSource = source,
            HeadersVisibility = DataGridHeadersVisibility.All,
            RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.Collapsed,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            Width = 640,
            Height = 400
        };
        for (var index = 0; index < 5; index++)
        {
            grid.Columns.Add(new DataGridTextColumn
            {
                Header = $"Column {index + 1}",
                Binding = new Binding(nameof(Row.Label)),
                Width = new DataGridLength(180)
            });
        }
        return grid;
    }

    private static AvaloniaWindow Show(global::AtomUI.Desktop.Controls.DataGrid grid)
    {
        var window = new AvaloniaWindow { Width = 680, Height = 440, Content = grid };
        window.Show();
        PumpUntil(() => grid.LoadState == DataGridLoadState.Ready &&
                        !grid.IsDataStale && DisplayedRows(grid).Length > 0);
        return window;
    }

    private static void ScrollTo(global::AtomUI.Desktop.Controls.DataGrid grid, double fraction)
    {
        var scrollBar = grid.VerticalScrollBar.ShouldNotBeNull();
        scrollBar.IsVisible.ShouldBeTrue();
        scrollBar.Maximum.ShouldBeGreaterThan(grid.CellsEstimatedHeight);
        var offset = scrollBar.Maximum * fraction;
        scrollBar.Value = offset;
        grid.ProcessVerticalScroll(ScrollEventType.ThumbTrack);
        PumpUntil(() => grid.LoadState == DataGridLoadState.Ready && !grid.IsDataStale &&
                        Math.Abs(grid.VerticalOffset - offset) < 0.1);
    }

    private static DataGridRow[] DisplayedRows(global::AtomUI.Desktop.Controls.DataGrid grid) =>
        grid.DisplayData.GetScrollingElements().OfType<DataGridRow>().ToArray();

    private static TextBlock HeaderText(DataGridRow row) => row.HeaderCell.ShouldNotBeNull()
        .GetVisualDescendants().OfType<TextBlock>().Single();

    private static void AssertHeaders(global::AtomUI.Desktop.Controls.DataGrid grid, Func<Row, string> expected)
    {
        var rows = DisplayedRows(grid);
        rows.ShouldNotBeEmpty();
        rows.Length.ShouldBeLessThan(100);
        foreach (var row in rows)
        {
            var header = row.HeaderCell.ShouldNotBeNull();
            header.IsVisible.ShouldBeTrue();
            header.Content.ShouldNotBeNull($"row {row.Index + 1} should have a visible header");
            var text = HeaderText(row);
            text.Text.ShouldBe(expected((Row)row.DataContext!));
            text.Bounds.Width.ShouldBeGreaterThan(0);
            text.Bounds.Height.ShouldBeGreaterThan(0);
            text.GetVisualAncestors().ShouldContain(row);
        }
    }

    private static void PumpUntil(Func<bool> condition) => SpinWait.SpinUntil(() =>
    {
        Dispatcher.UIThread.RunJobs();
        return condition();
    }, TimeSpan.FromSeconds(5)).ShouldBeTrue();

    private static void Close(AvaloniaWindow window)
    {
        window.Content = null;
        Dispatcher.UIThread.RunJobs();
        window.Close();
    }

    private sealed record Row(int Id, string Label);
}
