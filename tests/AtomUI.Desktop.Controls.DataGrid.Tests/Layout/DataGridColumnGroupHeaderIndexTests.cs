using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.DataGrid.Layout;

public class DataGridColumnGroupHeaderIndexTests
{
    static DataGridColumnGroupHeaderIndexTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Group_Header_With_Selection_Column_Keeps_Header_Indexes_When_Resized_And_Scrolled()
    {
        var grid = CreateIssue225Grid();
        var window = new Window
        {
            Width   = 420,
            Height  = 320,
            Content = grid
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var presenter = grid.GetVisualDescendants()
                                .OfType<DataGridGroupColumnHeadersPresenter>()
                                .Single();

            for (var i = 0; i < 20; i++)
            {
                if ((i % 2) == 0)
                {
                    window.Width  = 1280;
                    window.Height = 720;
                }
                else
                {
                    window.Width  = 420;
                    window.Height = 320;
                    grid.UpdateHorizontalOffset(10_000);
                }

                Dispatcher.UIThread.RunJobs();
                AssertHeaderViewItemIndexes(grid, presenter);
            }
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Issue225_Group_Header_With_Selection_And_Star_Column_Stays_Stable_When_Window_Size_Toggles()
    {
        var grid = CreateIssue225StarColumnGrid();
        var window = new Window
        {
            Width   = 420,
            Height  = 320,
            Content = grid
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var presenter = grid.GetVisualDescendants()
                                .OfType<DataGridGroupColumnHeadersPresenter>()
                                .Single();
            AssertHeaderViewItemIndexes(grid, presenter);

            for (var i = 0; i < 30; i++)
            {
                if ((i % 2) == 0)
                {
                    window.Width  = 1800;
                    window.Height = 720;
                    grid.UpdateHorizontalOffset(0);
                    Dispatcher.UIThread.RunJobs();
                    grid.Bounds.Width.ShouldBeGreaterThan(1500);
                }
                else
                {
                    window.Width  = 420;
                    window.Height = 320;
                    Dispatcher.UIThread.RunJobs();
                    grid.Bounds.Width.ShouldBeLessThan(600);
                    grid.UpdateHorizontalOffset(10_000);
                    Dispatcher.UIThread.RunJobs();
                }

                AssertHeaderViewItemIndexes(grid, presenter);
            }
        }
        finally
        {
            window.Close();
        }
    }

    private static void AssertHeaderViewItemIndexes(global::AtomUI.Desktop.Controls.DataGrid grid,
                                                    DataGridGroupColumnHeadersPresenter presenter)
    {
        var childIndexProvider = (IChildIndexProvider)presenter;
        var headerViewItems = presenter.Children
                                       .OfType<DataGridHeaderViewItem>()
                                       .ToArray();

        headerViewItems.Length.ShouldBe(grid.Columns.Count);
        for (var i = 0; i < headerViewItems.Length; i++)
        {
            childIndexProvider.GetChildIndex(headerViewItems[i]).ShouldBe(i);
        }

        childIndexProvider.TryGetTotalCount(out var totalCount).ShouldBeTrue();
        totalCount.ShouldBe(headerViewItems.Length);
    }

    private static global::AtomUI.Desktop.Controls.DataGrid CreateIssue225Grid()
    {
        var grid = new global::AtomUI.Desktop.Controls.DataGrid
        {
            AutoGenerateColumns           = false,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            ItemsSource = new TestDataGridSource<GridRow>(new[]
            {
                new GridRow("1", "Name", "20", "A", "B", "C", "D", "E")
            }),
            SelectionMode = DataGridSelectionMode.Extended,
            Width         = 380,
            Height        = 260
        };

        grid.ColumnGroups.Add(new DataGridSelectionColumn());
        grid.ColumnGroups.Add(new DataGridTextColumn
        {
            Header  = "ID",
            Binding = new Binding(nameof(GridRow.Id)),
            Width   = new DataGridLength(100)
        });
        grid.ColumnGroups.Add(new DataGridTextColumn
        {
            Header  = "Name",
            Binding = new Binding(nameof(GridRow.Name))
        });

        for (var i = 0; i < 6; i++)
        {
            grid.ColumnGroups.Add(new DataGridTextColumn
            {
                Header  = $"Value {i}",
                Binding = new Binding(GridRow.ValuePropertyNames[i]),
                Width   = new DataGridLength(100)
            });
        }

        return grid;
    }

    private static global::AtomUI.Desktop.Controls.DataGrid CreateIssue225StarColumnGrid()
    {
        var grid = new global::AtomUI.Desktop.Controls.DataGrid
        {
            AutoGenerateColumns           = false,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            ItemsSource = new TestDataGridSource<Issue225Row>(new[]
            {
                new Issue225Row()
            }),
            SelectionMode = DataGridSelectionMode.Extended
        };

        grid.ColumnGroups.Add(new DataGridSelectionColumn());
        grid.ColumnGroups.Add(new DataGridTextColumn
        {
            Header  = "ID",
            Binding = new Binding(nameof(Issue225Row.Id)),
            Width   = new DataGridLength(100)
        });
        grid.ColumnGroups.Add(new DataGridTextColumn
        {
            Header  = "Name",
            Binding = new Binding(nameof(Issue225Row.Name)),
            Width   = new DataGridLength(1, DataGridLengthUnitType.Star)
        });

        foreach (var propertyName in Issue225Row.ValuePropertyNames)
        {
            grid.ColumnGroups.Add(new DataGridTextColumn
            {
                Header  = "Age",
                Binding = new Binding(propertyName),
                Width   = new DataGridLength(100)
            });
        }

        return grid;
    }

    private sealed record GridRow(string Id,
                                  string Name,
                                  string Age,
                                  string Value1,
                                  string Value2,
                                  string Value3,
                                  string Value4,
                                  string Value5)
    {
        public static readonly string[] ValuePropertyNames =
        [
            nameof(Age),
            nameof(Value1),
            nameof(Value2),
            nameof(Value3),
            nameof(Value4),
            nameof(Value5)
        ];
    }

    private sealed class Issue225Row
    {
        public static readonly string[] ValuePropertyNames =
        [
            nameof(Age),
            nameof(Value1),
            nameof(Value2),
            nameof(Value3),
            nameof(Value4),
            nameof(Value5),
            nameof(Value6),
            nameof(Value7),
            nameof(Value8),
            nameof(Value9),
            nameof(Value10),
            nameof(Value11)
        ];

        public string Id { get; } = "1";

        public string Name { get; } = "Name";

        public string Age { get; } = "20";

        public string Value1 { get; } = "A";

        public string Value2 { get; } = "B";

        public string Value3 { get; } = "C";

        public string Value4 { get; } = "D";

        public string Value5 { get; } = "E";

        public string Value6 { get; } = "F";

        public string Value7 { get; } = "G";

        public string Value8 { get; } = "H";

        public string Value9 { get; } = "I";

        public string Value10 { get; } = "J";

        public string Value11 { get; } = "K";
    }
}
