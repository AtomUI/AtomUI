using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.DataGrid.Sorting;

public class DataGridSortingTests
{
    private static readonly DataGridFieldId NameField = new("name");
    private static readonly DataGridFieldId AgeField = new("age");

    static DataGridSortingTests() => AvaloniaTestApp.EnsureInitialized();

    [Fact]
    public void Sort_Reorders_Range_Rows_From_Typed_Local_Source()
    {
        var context = CreateGrid(DefaultRows());
        try
        {
            context.Grid.Sort(1, ListSortDirection.Ascending);
            PumpUntil(() => DisplayedAges(context.Grid).SequenceEqual([30, 32, 42]));

            context.Grid.Query.Sorts.ShouldBe([
                new DataGridSort(AgeField, DataGridSortDirection.Ascending)]);
        }
        finally
        {
            context.Close();
        }
    }

    [Fact]
    public void Clicking_Sort_Indicator_Reorders_Range_Rows()
    {
        var context = CreateGrid(DefaultRows());
        try
        {
            Click(FindSortIndicator(context.Grid), context.Window);
            PumpUntil(() => DisplayedAges(context.Grid).SequenceEqual([30, 32, 42]));
        }
        finally
        {
            context.Close();
        }
    }

    [Fact]
    public void Column_Level_Sort_Works_When_Grid_Level_Sorting_Is_Disabled()
    {
        var context = CreateGrid(DefaultRows(), gridCanSort: false, columnCanSort: true);
        try
        {
            Click(FindSortIndicator(context.Grid), context.Window);
            PumpUntil(() => DisplayedAges(context.Grid).SequenceEqual([30, 32, 42]));
        }
        finally
        {
            context.Close();
        }
    }

    [Fact]
    public void Clicking_Right_Edge_Of_Sort_Indicator_Works_When_Column_Can_Resize()
    {
        var context = CreateGrid(DefaultRows(), columnCanResize: true, width: 360);
        try
        {
            var indicator = FindSortIndicator(context.Grid);
            Click(
                indicator,
                context.Window,
                new Point(indicator.Bounds.Width - 1, indicator.Bounds.Height / 2));
            PumpUntil(() => DisplayedAges(context.Grid).SequenceEqual([30, 32, 42]));
        }
        finally
        {
            context.Close();
        }
    }

    [Fact]
    public void Sort_Keeps_Source_Ordinal_As_Tie_Break_For_Duplicate_Ages()
    {
        var context = CreateGrid([
            new SortRow(1, "John Brown", 32),
            new SortRow(2, "Jim Green", 42),
            new SortRow(3, "Joe Black", 32)]);
        try
        {
            Click(FindSortIndicator(context.Grid), context.Window);
            PumpUntil(() => DisplayedNames(context.Grid).SequenceEqual([
                "John Brown", "Joe Black", "Jim Green"]));
        }
        finally
        {
            context.Close();
        }
    }

    [Fact]
    public void Sort_Reorders_Rows_When_Cell_Bindings_Are_Compiled()
    {
        var context = CreateGrid([
            new SortRow(1, "John Brown", 32),
            new SortRow(2, "Jim Green", 42),
            new SortRow(3, "Joe Black", 32)], compiledBindings: true);
        try
        {
            Click(FindSortIndicator(context.Grid), context.Window);
            PumpUntil(() => DisplayedNames(context.Grid).SequenceEqual([
                "John Brown", "Joe Black", "Jim Green"]));
        }
        finally
        {
            context.Close();
        }
    }

    private static GridContext CreateGrid(
        IReadOnlyList<SortRow> rows,
        bool gridCanSort = true,
        bool? columnCanSort = null,
        bool columnCanResize = false,
        bool compiledBindings = false,
        double width = 480)
    {
        var descriptor = DataGridLocalSourceDescriptor.For<SortRow>(
                static row => DataGridRowKey.FromInt64(row.Id))
            .Field(NameField, static row => row.Name, StringComparer.Ordinal)
            .Field(AgeField, static row => row.Age);
        var source = DataGridLocalSource.Create(rows, descriptor);
        var grid = new global::AtomUI.Desktop.Controls.DataGrid
        {
            AutoGenerateColumns = false,
            CanUserSortColumns = gridCanSort,
            CanUserResizeColumns = columnCanResize,
            ItemsSource = source,
            Width = width,
            Height = 240
        };
        grid.Columns.Add(new DataGridTextColumn
        {
            Header = "Name",
            FieldId = NameField,
            Binding = compiledBindings
                ? CompiledBinding.Create<SortRow, string>(row => row.Name)
                : new Binding(nameof(SortRow.Name)),
            Width = new DataGridLength(160)
        });
        grid.Columns.Add(new DataGridTextColumn
        {
            Header = "Age",
            FieldId = AgeField,
            Binding = compiledBindings
                ? CompiledBinding.Create<SortRow, int>(row => row.Age)
                : new Binding(nameof(SortRow.Age)),
            CanUserSort = columnCanSort,
            CanUserResize = columnCanResize,
            Width = new DataGridLength(120)
        });
        var window = new AvaloniaWindow { Width = width + 40, Height = 280, Content = grid };
        window.Show();
        PumpUntil(() => grid.LoadState == DataGridLoadState.Ready && DisplayedAges(grid).Length == rows.Count);
        return new GridContext(grid, window, source);
    }

    private static IReadOnlyList<SortRow> DefaultRows() =>
    [
        new(1, "John Brown", 32),
        new(2, "Jim Green", 42),
        new(3, "Joe Black", 30)
    ];

    private static DataGridSortIndicator FindSortIndicator(
        global::AtomUI.Desktop.Controls.DataGrid grid) =>
        grid.GetVisualDescendants()
            .OfType<DataGridSortIndicator>()
            .Last(indicator => indicator.IsVisible && indicator.Bounds.Width > 0 && indicator.Bounds.Height > 0);

    private static void Click(Control control, AvaloniaWindow window, Point? localPoint = null)
    {
        var point = control.TranslatePoint(
            localPoint ?? new Point(control.Bounds.Width / 2, control.Bounds.Height / 2),
            window).ShouldNotBeNull();
        window.MouseMove(point);
        window.MouseDown(point, MouseButton.Left);
        window.MouseUp(point, MouseButton.Left);
    }

    private static int[] DisplayedAges(global::AtomUI.Desktop.Controls.DataGrid grid) =>
        DisplayedRows(grid).Select(row => row.Age).ToArray();

    private static string[] DisplayedNames(global::AtomUI.Desktop.Controls.DataGrid grid) =>
        DisplayedRows(grid).Select(row => row.Name).ToArray();

    private static SortRow[] DisplayedRows(global::AtomUI.Desktop.Controls.DataGrid grid) =>
        grid.GetVisualDescendants()
            .OfType<DataGridRow>()
            .Select(row => new { Row = row, Position = row.TranslatePoint(default, grid) })
            .Where(item => item.Position is not null && item.Row.DataContext is SortRow)
            .OrderBy(item => item.Position!.Value.Y)
            .Select(item => (SortRow)item.Row.DataContext!)
            .ToArray();

    private static void PumpUntil(Func<bool> condition)
    {
        if (!SpinWait.SpinUntil(() =>
            {
                Dispatcher.UIThread.RunJobs();
                return condition();
            }, TimeSpan.FromSeconds(5)))
        {
            throw new TimeoutException("The expected sorted presentation was not reached.");
        }
    }

    private sealed record SortRow(long Id, string Name, int Age);

    private sealed record GridContext(
        global::AtomUI.Desktop.Controls.DataGrid Grid,
        AvaloniaWindow Window,
        DataGridLocalSource<SortRow> Source)
    {
        public void Close()
        {
            Window.Close();
            Dispatcher.UIThread.RunJobs();
            Source.Dispose();
        }
    }
}
