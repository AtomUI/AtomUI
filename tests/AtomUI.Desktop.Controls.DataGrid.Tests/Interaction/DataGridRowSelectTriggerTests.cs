using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.DataGrid.Interaction;

// https://github.com/AtomUI/AtomUI/issues/418
public class DataGridRowSelectTriggerTests
{
    private static readonly DataGridFieldId NameField = new("name");

    static DataGridRowSelectTriggerTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Extended_Row_Click_Toggles_Row_Without_Modifiers()
    {
        RunWithGrid(DataGridSelectionMode.Extended, DataGridSelectTriggerType.Row, (grid, window, rows) =>
        {
            Click(GetBodyCell(grid, rows[0]), window);
            Click(GetBodyCell(grid, rows[1]), window);

            AssertSelectedRows(grid, rows, true, true, false);

            Click(GetBodyCell(grid, rows[0], columnIndex: 2), window);

            AssertSelectedRows(grid, rows, false, true, false);
        });
    }

    [Fact]
    public void Extended_Row_Click_On_Selection_Cell_Blank_Area_Toggles_Row()
    {
        RunWithGrid(DataGridSelectionMode.Extended, DataGridSelectTriggerType.Row, (grid, window, rows) =>
        {
            Click(GetBodyCell(grid, rows[0]), window);
            Click(GetBodyCell(grid, rows[1], columnIndex: 0), window, new Point(2, 2));

            AssertSelectedRows(grid, rows, true, true, false);

            Click(GetBodyCell(grid, rows[1], columnIndex: 0), window, new Point(2, 2));

            AssertSelectedRows(grid, rows, true, false, false);
        });
    }

    [Fact]
    public void Extended_Row_Click_Selection_Indicator_Keeps_Toggle_Behavior()
    {
        RunWithGrid(DataGridSelectionMode.Extended, DataGridSelectTriggerType.Row, (grid, window, rows) =>
        {
            Click(GetIndicator(grid, rows[0]), window);
            Click(GetIndicator(grid, rows[2]), window);

            AssertSelectedRows(grid, rows, true, false, true);

            Click(GetIndicator(grid, rows[0]), window);

            AssertSelectedRows(grid, rows, false, false, true);
        });
    }

    [Fact]
    public void Extended_Row_Click_On_Current_Cell_To_Begin_Edit_Keeps_Row_Selected()
    {
        RunWithGrid(DataGridSelectionMode.Extended, DataGridSelectTriggerType.Row, (grid, window, rows) =>
        {
            Click(GetBodyCell(grid, rows[1]), window);
            Click(GetBodyCell(grid, rows[0]), window);
            Click(GetBodyCell(grid, rows[0]), window);

            grid.EditingColumnIndex.ShouldBe(1);
            AssertSelectedRows(grid, rows, true, true, false);
        }, editable: true);
    }

    [Fact]
    public void Extended_Row_Shift_Click_Selects_Range_From_Anchor()
    {
        RunWithGrid(DataGridSelectionMode.Extended, DataGridSelectTriggerType.Row, (grid, window, rows) =>
        {
            Click(GetBodyCell(grid, rows[0]), window);
            Click(GetBodyCell(grid, rows[2]), window, modifiers: RawInputModifiers.Shift);

            AssertSelectedRows(grid, rows, true, true, true);
        });
    }

    [Fact]
    public void Single_Row_Click_Moves_Selection_And_Keeps_Selected_Row()
    {
        RunWithGrid(DataGridSelectionMode.Single, DataGridSelectTriggerType.Row, (grid, window, rows) =>
        {
            Click(GetBodyCell(grid, rows[0]), window);
            Click(GetBodyCell(grid, rows[1]), window);

            AssertSelectedRows(grid, rows, false, true, false);

            Click(GetBodyCell(grid, rows[1], columnIndex: 2), window);

            AssertSelectedRows(grid, rows, false, true, false);
        });
    }

    [Fact]
    public void None_Row_Click_Does_Not_Select()
    {
        RunWithGrid(DataGridSelectionMode.None, DataGridSelectTriggerType.Row, (grid, window, rows) =>
        {
            Click(GetBodyCell(grid, rows[0]), window);

            grid.Selection.IsEmpty.ShouldBeTrue();
        });
    }

    [Fact]
    public void Extended_Cell_Click_Still_Replaces_Selection()
    {
        RunWithGrid(DataGridSelectionMode.Extended, DataGridSelectTriggerType.Cell, (grid, window, rows) =>
        {
            Click(GetBodyCell(grid, rows[0]), window);
            Click(GetBodyCell(grid, rows[1]), window);

            AssertSelectedRows(grid, rows, false, true, false);
        });
    }

    private static void RunWithGrid(
        DataGridSelectionMode selectionMode,
        DataGridSelectTriggerType triggerType,
        Action<global::AtomUI.Desktop.Controls.DataGrid, Window, GridRow[]> scenario,
        bool editable = false)
    {
        var rows = new[]
        {
            new GridRow(1, "Alice", "London"),
            new GridRow(2, "Bob", "Paris"),
            new GridRow(3, "Carol", "Berlin")
        };
        var grid = new global::AtomUI.Desktop.Controls.DataGrid
        {
            AutoGenerateColumns = false,
            SelectionMode       = selectionMode,
            SelectTriggerType   = triggerType,
            ItemsSource         = editable ? CreateEditableSource(rows) : new TestDataGridSource<GridRow>(rows),
            Width               = 520,
            Height              = 240
        };
        grid.Columns.Add(new DataGridSelectionColumn());
        grid.Columns.Add(new DataGridTextColumn
        {
            FieldId    = NameField,
            IsReadOnly = !editable,
            Header     = "Name",
            Binding    = CompiledBinding.Create<GridRow, string>(row => row.Name),
            Width      = new DataGridLength(160)
        });
        grid.Columns.Add(new DataGridTextColumn
        {
            Header  = "City",
            Binding = new Binding(nameof(GridRow.City)),
            Width   = new DataGridLength(160)
        });
        var window = new Window
        {
            Width   = 600,
            Height  = 320,
            Content = grid
        };

        try
        {
            window.Show();
            PumpUntil(() =>
                grid.LoadState == DataGridLoadState.Ready &&
                grid.GetVisualDescendants().OfType<DataGridRow>().Count() == rows.Length);

            scenario(grid, window, rows);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static IDataGridSource CreateEditableSource(IReadOnlyList<GridRow> rows) =>
        DataGridLocalSource.Create(
            rows,
            DataGridLocalSourceDescriptor.For<GridRow>(static row => DataGridRowKey.FromInt64(row.Id))
                                         .Field(
                                             NameField,
                                             static row => row.Name,
                                             setter: static (row, value) => row.Name = value));

    private static void AssertSelectedRows(
        global::AtomUI.Desktop.Controls.DataGrid grid,
        GridRow[] rows,
        params bool[] expected)
    {
        for (var index = 0; index < rows.Length; index++)
        {
            GetRow(grid, rows[index]).IsSelected.ShouldBe(expected[index], $"row {rows[index].Name}");
        }
    }

    private static DataGridCell GetBodyCell(
        global::AtomUI.Desktop.Controls.DataGrid grid,
        GridRow item,
        int columnIndex = 1) =>
        GetRow(grid, item).GetVisualDescendants()
                          .OfType<DataGridCell>()
                          .Single(cell => cell.ColumnIndex == columnIndex);

    private static Control GetIndicator(
        global::AtomUI.Desktop.Controls.DataGrid grid,
        GridRow item) =>
        GetRow(grid, item).GetVisualDescendants()
                          .OfType<SelectionCheckBox>()
                          .Single();

    private static DataGridRow GetRow(
        global::AtomUI.Desktop.Controls.DataGrid grid,
        GridRow item) =>
        grid.GetVisualDescendants()
            .OfType<DataGridRow>()
            .Single(row => ReferenceEquals(row.DataContext, item));

    private static void Click(
        Control control,
        Window window,
        Point? offset = null,
        RawInputModifiers modifiers = RawInputModifiers.None)
    {
        var local = offset ?? new Point(control.Bounds.Width / 2, control.Bounds.Height / 2);
        var point = control.TranslatePoint(local, window);

        point.ShouldNotBeNull();
        window.MouseMove(point.Value, modifiers);
        window.MouseDown(point.Value, MouseButton.Left, modifiers);
        window.MouseUp(point.Value, MouseButton.Left, modifiers);
        Dispatcher.UIThread.RunJobs();
    }

    private static void PumpUntil(Func<bool> condition)
    {
        if (!SpinWait.SpinUntil(() =>
            {
                Dispatcher.UIThread.RunJobs();
                return condition();
            }, TimeSpan.FromSeconds(5)))
        {
            throw new TimeoutException("The expected DataGrid rows were not realized.");
        }
    }

    private sealed class GridRow(long id, string name, string city)
    {
        public long Id { get; } = id;

        public string Name { get; set; } = name;

        public string City { get; } = city;
    }
}
