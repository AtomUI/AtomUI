using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.DataGrid.Layout;

public class DataGridStarColumnLayoutTests
{
    static DataGridStarColumnLayoutTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Star_Column_Consumes_Remaining_Width_Instead_Of_Leaving_Filler()
    {
        var descriptionColumn = new DataGridTextColumn
        {
            Header   = "Description",
            Binding  = new Binding(nameof(GridRow.Description)),
            MinWidth = 520,
            Width    = new DataGridLength(1, DataGridLengthUnitType.Star)
        };
        var grid = new global::AtomUI.Desktop.Controls.DataGrid
        {
            AutoGenerateColumns          = false,
            HeadersVisibility            = DataGridHeadersVisibility.Column,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            LeftFrozenColumnCount        = 1,
            ItemsSource = new TestDataGridSource<GridRow>(new[]
            {
                new GridRow("Size", "Used to control the component dimensions.", "double", "32")
            }),
            Width  = 1280,
            Height = 260
        };

        grid.Columns.Add(new DataGridTextColumn
        {
            Header  = "Property",
            Binding = new Binding(nameof(GridRow.Property)),
            Width   = new DataGridLength(160)
        });
        grid.Columns.Add(descriptionColumn);
        grid.Columns.Add(new DataGridTextColumn
        {
            Header  = "Type",
            Binding = new Binding(nameof(GridRow.Type)),
            Width   = new DataGridLength(180)
        });
        grid.Columns.Add(new DataGridTextColumn
        {
            Header  = "Default",
            Binding = new Binding(nameof(GridRow.Default)),
            Width   = new DataGridLength(120)
        });

        var window = new Window
        {
            Width   = 1320,
            Height  = 360,
            Content = grid
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            descriptionColumn.ActualWidth.ShouldBeGreaterThan(520);
            grid.ColumnsInternal.FillerColumn.ShouldNotBeNull();
            grid.ColumnsInternal.FillerColumn.FillerWidth.ShouldBe(0, 1);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void User_Resize_Of_Star_Column_Preserves_Resolved_Display_Width()
    {
        var addressColumn = CreateStarColumn("Address", 2);
        var roleColumn = CreateStarColumn("Role");
        var statusColumn = new DataGridTextColumn
        {
            Header        = "Status",
            Width         = new DataGridLength(90),
            CanUserResize = false
        };
        var operationColumn = new DataGridTextColumn
        {
            Header        = "Operation",
            Width         = new DataGridLength(240),
            CanUserResize = false
        };
        var grid = new global::AtomUI.Desktop.Controls.DataGrid
        {
            AutoGenerateColumns  = false,
            CanUserResizeColumns = true,
            HeadersVisibility    = DataGridHeadersVisibility.Column,
            ItemsSource = new TestDataGridSource<GridRow>(new[]
            {
                new GridRow("1", "Shanghai", "Administrator", "Active")
            }),
            Width  = 1000,
            Height = 260
        };

        grid.Columns.Add(new DataGridTextColumn
        {
            Header  = "ID",
            Binding = new Binding(nameof(GridRow.Property)),
            Width   = DataGridLength.Auto
        });
        grid.Columns.Add(addressColumn);
        grid.Columns.Add(roleColumn);
        grid.Columns.Add(statusColumn);
        grid.Columns.Add(operationColumn);

        var window = ShowGrid(grid);

        try
        {
            var originalAddressWidth = addressColumn.ActualWidth;
            var originalRoleWidth = roleColumn.ActualWidth;
            var originalVisibleColumnsWidth = grid.ColumnsInternal.VisibleEdgedColumnsWidth;
            var requestedAddressWidth = originalAddressWidth + 40;
            var expectedStarValue = addressColumn.Width.Value * requestedAddressWidth / originalAddressWidth;
            var oldWidth = addressColumn.Width;

            addressColumn.Resize(
                oldWidth,
                new DataGridLength(
                    oldWidth.Value,
                    oldWidth.UnitType,
                    oldWidth.DesiredValue,
                    requestedAddressWidth),
                userInitiated: true);

            addressColumn.Width.IsStar.ShouldBeTrue();
            addressColumn.Width.Value.ShouldBe(expectedStarValue, 0.001);
            addressColumn.Width.DesiredValue.ShouldBe(requestedAddressWidth, 1);
            addressColumn.Width.DisplayValue.ShouldBe(requestedAddressWidth, 1);
            addressColumn.ActualWidth.ShouldBe(requestedAddressWidth, 1);
            roleColumn.ActualWidth.ShouldBe(originalRoleWidth - 40, 1);
            statusColumn.ActualWidth.ShouldBe(90, 1);
            operationColumn.ActualWidth.ShouldBe(240, 1);
            grid.ColumnsInternal.VisibleEdgedColumnsWidth.ShouldBe(originalVisibleColumnsWidth, 1);
        }
        finally
        {
            window.Close();
        }
    }

    [Theory]
    [InlineData(DataGridLengthUnitType.Auto)]
    [InlineData(DataGridLengthUnitType.Pixel)]
    [InlineData(DataGridLengthUnitType.SizeToHeader)]
    [InlineData(DataGridLengthUnitType.SizeToCells)]
    public void Empty_Grid_Resolves_Star_Columns_After_NonStar_Width_Mode(DataGridLengthUnitType unitType)
    {
        var firstColumn = new DataGridTextColumn
        {
            Header = "Identifier",
            Width = unitType switch
            {
                DataGridLengthUnitType.Auto         => DataGridLength.Auto,
                DataGridLengthUnitType.Pixel        => new DataGridLength(160),
                DataGridLengthUnitType.SizeToHeader => DataGridLength.SizeToHeader,
                DataGridLengthUnitType.SizeToCells  => DataGridLength.SizeToCells,
                _                                   => throw new ArgumentOutOfRangeException(nameof(unitType))
            }
        };
        var grid = CreateEmptyGrid(firstColumn,
            CreateStarColumn("Primary"),
            CreateStarColumn("Secondary"));
        var window = ShowGrid(grid);

        try
        {
            AssertEmptyGridUsesAvailableWidth(grid);
            grid.Columns[1].ActualWidth.ShouldBeGreaterThan(grid.Columns[1].ActualMinWidth);
            grid.Columns[2].ActualWidth.ShouldBeGreaterThan(grid.Columns[2].ActualMinWidth);

            if (unitType is DataGridLengthUnitType.Auto or DataGridLengthUnitType.SizeToHeader)
            {
                firstColumn.Header = "Identifier header content that grows after the initial empty layout";
                Dispatcher.UIThread.RunJobs();
                AssertEmptyGridUsesAvailableWidth(grid);
            }
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Empty_AllStar_Columns_Respect_Weights_When_Viewport_Resizes()
    {
        var firstStarColumn  = CreateStarColumn("Primary", 1);
        var secondStarColumn = CreateStarColumn("Secondary", 2);
        var grid = CreateEmptyGrid(firstStarColumn, secondStarColumn);
        var window = ShowGrid(grid);

        try
        {
            AssertEmptyGridUsesAvailableWidth(grid);
            secondStarColumn.ActualWidth.ShouldBe(firstStarColumn.ActualWidth * 2, 1);

            grid.Width = 620;
            Dispatcher.UIThread.RunJobs();

            AssertEmptyGridUsesAvailableWidth(grid);
            secondStarColumn.ActualWidth.ShouldBe(firstStarColumn.ActualWidth * 2, 1);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Star_Columns_Keep_Using_Available_Width_Across_Empty_Add_Clear_Transitions()
    {
        var items = new ObservableCollection<GridRow>();
        var grid = CreateEmptyGrid(
            new DataGridTextColumn
            {
                Header  = "Property",
                Binding = new Binding(nameof(GridRow.Property)),
                Width   = DataGridLength.Auto
            },
            new DataGridTextColumn
            {
                Header  = "Description",
                Binding = new Binding(nameof(GridRow.Description)),
                Width   = new DataGridLength(1, DataGridLengthUnitType.Star)
            });
        var source = new TestDataGridSource<GridRow>(items);
        grid.ItemsSource = source;
        var window = ShowGrid(grid);

        try
        {
            AssertEmptyGridUsesAvailableWidth(grid);

            items.Add(new GridRow("Size", "Used to control the component dimensions.", "double", "32"));
            source.Invalidate();
            grid.Width = 700;
            Dispatcher.UIThread.RunJobs();
            AssertColumnsUseAvailableWidth(grid);

            items.Clear();
            source.Invalidate();
            grid.Width = 760;
            Dispatcher.UIThread.RunJobs();
            AssertEmptyGridUsesAvailableWidth(grid);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Populated_Star_Columns_Keep_Using_CellsWidth_With_Row_Header_And_Scrollbar()
    {
        var grid = new global::AtomUI.Desktop.Controls.DataGrid
        {
            AutoGenerateColumns           = false,
            HeadersVisibility             = DataGridHeadersVisibility.All,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            RowHeaderWidth                = 56,
            ItemsSource = new TestDataGridSource<GridRow>(Enumerable.Range(0, 40)
                                    .Select(index => new GridRow(
                                        $"Property {index}",
                                        "Description",
                                        "string",
                                        "Default"))
                                    .ToArray()),
            Width  = 880,
            Height = 260
        };
        grid.Columns.Add(new DataGridTextColumn
        {
            Header  = "Property",
            Binding = new Binding(nameof(GridRow.Property)),
            Width   = new DataGridLength(160)
        });
        grid.Columns.Add(new DataGridTextColumn
        {
            Header  = "Description",
            Binding = new Binding(nameof(GridRow.Description)),
            Width   = new DataGridLength(1, DataGridLengthUnitType.Star)
        });
        var window = ShowGrid(grid);

        try
        {
            var rowsPresenter = grid.GetVisualDescendants()
                                    .OfType<DataGridRowsPresenter>()
                                    .Single();

            rowsPresenter.IsVisible.ShouldBeTrue();
            grid.VerticalScrollBar.ShouldNotBeNull();
            grid.VerticalScrollBar.IsVisible.ShouldBeTrue();
            grid.ColumnsInternal.VisibleEdgedColumnsWidth.ShouldBe(grid.CellsWidth, 1);
            grid.ColumnsInternal.FillerColumn.ShouldNotBeNull();
            grid.ColumnsInternal.FillerColumn.FillerWidth.ShouldBe(0, 1);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Empty_Group_Header_Uses_The_Shared_Star_Column_Solver()
    {
        var grid = new global::AtomUI.Desktop.Controls.DataGrid
        {
            AutoGenerateColumns = false,
            HeadersVisibility   = DataGridHeadersVisibility.Column,
            ItemsSource              = new TestDataGridSource<GridRow>(Array.Empty<GridRow>()),
            Width               = 880,
            Height              = 260
        };
        grid.ColumnGroups.Add(new DataGridTextColumn
        {
            Header = "Identifier",
            Width  = DataGridLength.Auto
        });
        grid.ColumnGroups.Add(CreateStarColumn("Primary"));
        grid.ColumnGroups.Add(CreateStarColumn("Secondary"));
        var window = ShowGrid(grid);

        try
        {
            var presenter = grid.GetVisualDescendants()
                                .OfType<DataGridGroupColumnHeadersPresenter>()
                                .Single();

            AssertEmptyGridUsesAvailableWidth(grid, presenter.Bounds.Width);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Empty_Star_Columns_Use_Filler_Only_After_Reaching_MaxWidth()
    {
        var firstStarColumn = CreateStarColumn("Primary");
        firstStarColumn.MaxWidth = 180;
        var secondStarColumn = CreateStarColumn("Secondary");
        secondStarColumn.MaxWidth = 180;
        var grid = CreateEmptyGrid(firstStarColumn, secondStarColumn);
        var window = ShowGrid(grid);

        try
        {
            var presenter = grid.GetVisualDescendants()
                                .OfType<DataGridColumnHeadersPresenter>()
                                .Single();
            var rowsPresenter = grid.GetVisualDescendants()
                                    .OfType<DataGridRowsPresenter>()
                                    .Single();

            rowsPresenter.IsVisible.ShouldBeFalse();
            firstStarColumn.ActualWidth.ShouldBe(180, 1);
            secondStarColumn.ActualWidth.ShouldBe(180, 1);
            grid.ColumnsInternal.FillerColumn.ShouldNotBeNull();
            grid.ColumnsInternal.FillerColumn.FillerWidth.ShouldBe(
                presenter.Bounds.Width - grid.ColumnsInternal.VisibleEdgedColumnsWidth,
                1);
            grid.ColumnsInternal.FillerColumn.FillerWidth.ShouldBeGreaterThan(0);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Empty_Grid_Preserves_Infinite_Viewport_Star_Fallback()
    {
        var firstStarColumn = CreateStarColumn("Primary");
        firstStarColumn.MaxWidth = 240;
        var secondStarColumn = CreateStarColumn("Secondary");
        secondStarColumn.MaxWidth = 240;
        var grid = new global::AtomUI.Desktop.Controls.DataGrid
        {
            AutoGenerateColumns = false,
            HeadersVisibility   = DataGridHeadersVisibility.Column,
            ItemsSource              = new TestDataGridSource<GridRow>(Array.Empty<GridRow>()),
            Height              = 260
        };
        grid.Columns.Add(firstStarColumn);
        grid.Columns.Add(secondStarColumn);
        var window = new Window
        {
            Width  = 940,
            Height = 340,
            Content = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                Children =
                {
                    grid
                }
            }
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            firstStarColumn.ActualWidth.ShouldBe(240, 1);
            secondStarColumn.ActualWidth.ShouldBe(240, 1);
        }
        finally
        {
            window.Close();
        }
    }

    private static global::AtomUI.Desktop.Controls.DataGrid CreateEmptyGrid(params DataGridColumn[] columns)
    {
        var grid = new global::AtomUI.Desktop.Controls.DataGrid
        {
            AutoGenerateColumns = false,
            HeadersVisibility   = DataGridHeadersVisibility.Column,
            ItemsSource              = new TestDataGridSource<GridRow>(Array.Empty<GridRow>()),
            Width               = 880,
            Height              = 260
        };

        foreach (var column in columns)
        {
            grid.Columns.Add(column);
        }

        return grid;
    }

    private static DataGridTextColumn CreateStarColumn(string header, double weight = 1)
    {
        return new DataGridTextColumn
        {
            Header = header,
            Width  = new DataGridLength(weight, DataGridLengthUnitType.Star)
        };
    }

    private static Window ShowGrid(global::AtomUI.Desktop.Controls.DataGrid grid)
    {
        var window = new Window
        {
            Width   = 940,
            Height  = 340,
            Content = grid
        };

        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static void AssertEmptyGridUsesAvailableWidth(global::AtomUI.Desktop.Controls.DataGrid grid)
    {
        var presenter = grid.GetVisualDescendants()
                            .OfType<DataGridColumnHeadersPresenter>()
                            .Single();

        AssertEmptyGridUsesAvailableWidth(grid, presenter.Bounds.Width);
    }

    private static void AssertEmptyGridUsesAvailableWidth(global::AtomUI.Desktop.Controls.DataGrid grid,
                                                          double availableWidth)
    {
        var rowsPresenter = grid.GetVisualDescendants()
                                .OfType<DataGridRowsPresenter>()
                                .Single();

        rowsPresenter.IsVisible.ShouldBeFalse();
        AssertColumnsUseAvailableWidth(grid, availableWidth);
    }

    private static void AssertColumnsUseAvailableWidth(global::AtomUI.Desktop.Controls.DataGrid grid)
    {
        var presenter = grid.IsGroupHeaderMode
            ? (Control)grid.GetVisualDescendants().OfType<DataGridGroupColumnHeadersPresenter>().Single()
            : grid.GetVisualDescendants().OfType<DataGridColumnHeadersPresenter>().Single();

        AssertColumnsUseAvailableWidth(grid, presenter.Bounds.Width);
    }

    private static void AssertColumnsUseAvailableWidth(global::AtomUI.Desktop.Controls.DataGrid grid,
                                                       double availableWidth)
    {
        grid.ColumnsInternal.VisibleEdgedColumnsWidth.ShouldBe(availableWidth, 1);
        grid.ColumnsInternal.FillerColumn.ShouldNotBeNull();
        grid.ColumnsInternal.FillerColumn.FillerWidth.ShouldBe(0, 1);
    }

    private sealed record GridRow(string Property, string Description, string Type, string Default);
}
