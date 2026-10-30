using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Threading;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.DataGrid.Virtualization;

public class DataGridIssue457Tests
{
    private static readonly DataGridFieldId IdField = new("id");
    private static readonly DataGridOperatorId GreaterThanOrEqualOperator = new("gte");

    static DataGridIssue457Tests() => AvaloniaTestApp.EnsureInitialized();

    [Fact]
    public void ItemsSource_Rebind_To_Fewer_Tall_Rows_Rebases_Height_And_Reaches_Last_Row()
    {
        using var initialSource = Source(ShortRows());
        using var replacementSource = Source(TallRows());
        var grid = Grid(initialSource);
        var window = Show(grid);

        try
        {
            AssertShortGeneration(grid);

            grid.ItemsSource = replacementSource;

            AssertTallGeneration(grid);
        }
        finally
        {
            Close(window);
        }
    }

    [Fact]
    public void Filter_To_Fewer_Tall_Rows_Rebases_Height_And_Reaches_Last_Row()
    {
        var rows = ShortRows().Concat(TallRows(startId: 48)).ToArray();
        using var source = Source(rows);
        var grid = Grid(source);
        var window = Show(grid);

        try
        {
            AssertShortGeneration(grid);

            grid.Query = DataGridQuery.Empty.WithFilters([
                new DataGridFilter(
                    IdField,
                    GreaterThanOrEqualOperator,
                    [DataGridScalar.FromInt64(48)])
            ]);

            AssertTallGeneration(grid);
        }
        finally
        {
            Close(window);
        }
    }

    [Fact]
    public void Last_Page_With_Fewer_Tall_Rows_Rebases_Height_And_Reaches_Last_Row()
    {
        var rows = ShortRows().Concat(TallRows(startId: 48)).ToArray();
        using var source = Source(rows);
        var grid = Grid(source);
        grid.PageRequest = new DataGridPageRequest(0, 48);
        var window = Show(grid);

        try
        {
            AssertShortGeneration(grid);

            grid.PageRequest = new DataGridPageRequest(48, 48);

            AssertTallGeneration(grid);
        }
        finally
        {
            Close(window);
        }
    }

    [Fact]
    public void Collection_Reset_To_Fewer_Tall_Rows_Rebases_Height_And_Reaches_Last_Row()
    {
        var rows = new ResettableCollection<Row>(ShortRows());
        using var source = Source(rows);
        var grid = Grid(source);
        var window = Show(grid);

        try
        {
            AssertShortGeneration(grid);

            rows.Reset(TallRows());

            AssertTallGeneration(grid);
        }
        finally
        {
            Close(window);
        }
    }

    private static DataGridLocalSource<Row> Source(IReadOnlyList<Row> rows)
    {
        var filters = ImmutableArray.Create(
            new DataGridLocalFilter<int>(
                GreaterThanOrEqualOperator,
                1,
                1,
                DataGridScalarKinds.SignedInteger,
                static (value, values) => DataGridScalar.FromInt64(value) >= values[0]));
        var descriptor = DataGridLocalSourceDescriptor.For<Row>(
                static row => DataGridRowKey.FromInt64(row.Id + 1L))
            .Field(
                IdField,
                static row => row.Id,
                Comparer<int>.Default,
                static value => DataGridScalar.FromInt64(value),
                filters);
        return DataGridLocalSource.Create(
            rows,
            descriptor,
            new DataGridLocalSourceOptions
            {
                PreferredRangeSize = 64,
                MaximumRangeSize = 64
            });
    }

    private static global::AtomUI.Desktop.Controls.DataGrid Grid(IDataGridSource source)
    {
        var grid = new global::AtomUI.Desktop.Controls.DataGrid
        {
            AutoGenerateColumns = false,
            IsMotionEnabled = false,
            ItemsSource = source,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Width = 420,
            Height = 400
        };
        grid.Columns.Add(new DataGridTemplateColumn
        {
            Header = "Value",
            CellTemplate = new FuncDataTemplate<Row>((row, _) => new Border
            {
                Height = row?.Height ?? 0,
                Child = new TextBlock { Text = row?.Id.ToString() }
            })
        });
        return grid;
    }

    private static AvaloniaWindow Show(global::AtomUI.Desktop.Controls.DataGrid grid)
    {
        var window = new AvaloniaWindow
        {
            Width = 500,
            Height = 480,
            Content = grid
        };
        window.Show();
        PumpUntil(() => grid.LoadState == DataGridLoadState.Ready && !grid.IsDataStale);
        return window;
    }

    private static void AssertShortGeneration(global::AtomUI.Desktop.Controls.DataGrid grid)
    {
        PumpUntil(() =>
            grid.LoadState == DataGridLoadState.Ready &&
            !grid.IsDataStale &&
            grid.SlotCount >= 48 &&
            grid.DisplayData.NumDisplayedScrollingElements > 0);
        grid.RowHeightEstimate.ShouldBeLessThan(100);
    }

    private static void AssertTallGeneration(global::AtomUI.Desktop.Controls.DataGrid grid)
    {
        PumpUntil(() =>
            grid.LoadState == DataGridLoadState.Ready &&
            !grid.IsDataStale &&
            grid.SlotCount == 2 &&
            grid.DisplayData.LastScrollingSlot == grid.LastVisibleSlot);

        var firstHeight = grid.GetRangeHeight(0);
        var scrollBar = grid.VerticalScrollBar.ShouldNotBeNull();

        firstHeight.ShouldBeGreaterThan(200);
        grid.RowHeightEstimate.ShouldBe(firstHeight, tolerance: 0.1);
        scrollBar.IsVisible.ShouldBeTrue();
        scrollBar.Maximum.ShouldBeGreaterThan(0);
        scrollBar.Maximum.ShouldBeGreaterThanOrEqualTo(scrollBar.Minimum);
        scrollBar.Maximum.ShouldBe(
            Math.Max(scrollBar.Minimum, grid.GetRangeExtent() - grid.CellsEstimatedHeight),
            tolerance: 0.1);

        scrollBar.Value = scrollBar.Maximum;
        grid.ProcessVerticalScroll(ScrollEventType.ThumbTrack);
        PumpUntil(() => Math.Abs(grid.VerticalOffset - scrollBar.Maximum) < 0.1);

        grid.DisplayData.LastScrollingSlot.ShouldBe(grid.LastVisibleSlot);
    }

    private static Row[] ShortRows() => Enumerable.Range(0, 48)
        .Select(static id => new Row(id, 28))
        .ToArray();

    private static Row[] TallRows(int startId = 0) =>
    [
        new Row(startId, 240),
        new Row(startId + 1, 240)
    ];

    private static void PumpUntil(Func<bool> condition)
    {
        SpinWait.SpinUntil(() =>
        {
            Dispatcher.UIThread.RunJobs();
            return condition();
        }, TimeSpan.FromSeconds(5)).ShouldBeTrue();
    }

    private static void Close(AvaloniaWindow window)
    {
        window.Content = null;
        Dispatcher.UIThread.RunJobs();
        window.Close();
    }

    private sealed record Row(int Id, double Height);

    private sealed class ResettableCollection<T>(IEnumerable<T> items) : ObservableCollection<T>(items)
    {
        public void Reset(IEnumerable<T> items)
        {
            Items.Clear();
            foreach (var item in items)
            {
                Items.Add(item);
            }
            OnPropertyChanged(new(nameof(Count)));
            OnPropertyChanged(new("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
