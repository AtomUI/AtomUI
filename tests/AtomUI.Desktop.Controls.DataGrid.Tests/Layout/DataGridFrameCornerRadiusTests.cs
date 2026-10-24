using AtomUI.Controls.Primitives;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.DataGrid.Layout;

public class DataGridFrameCornerRadiusTests
{
    static DataGridFrameCornerRadiusTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Frame_Uses_Top_Only_CornerRadius_When_Frame_Border_Is_Hidden()
    {
        var cornerRadius = new CornerRadius(3, 5, 7, 11);
        var borderThickness = new Thickness(1, 2, 3, 4);
        var grid = new global::AtomUI.Desktop.Controls.DataGrid
        {
            AutoGenerateColumns = false,
            BorderThickness     = borderThickness,
            CornerRadius        = cornerRadius,
            Width               = 360,
            Height              = 180
        };

        var window = new AvaloniaWindow
        {
            Width   = 420,
            Height  = 260,
            Content = grid
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var frame = FindTemplatePixelAlignedBorder(grid, "Frame");
            frame.CornerRadius.ShouldBe(GetTopOnlyCornerRadius(cornerRadius));
            frame.BorderThickness.ShouldBe(new Thickness(0));

            var headerFrame = FindTemplateBorder(grid, "ColumnHeadersPresenterFrame");
            headerFrame.CornerRadius.ShouldBe(GetTopOnlyCornerRadius(cornerRadius));
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Frame_Keeps_Full_CornerRadius_When_Frame_Border_Is_Visible()
    {
        var cornerRadius = new CornerRadius(3, 5, 7, 11);
        var borderThickness = new Thickness(1, 2, 3, 4);
        var grid = new global::AtomUI.Desktop.Controls.DataGrid
        {
            AutoGenerateColumns  = false,
            BorderThickness      = borderThickness,
            CornerRadius         = cornerRadius,
            IsFrameBorderVisible = true,
            Width                = 360,
            Height               = 180
        };

        var window = new AvaloniaWindow
        {
            Width   = 420,
            Height  = 260,
            Content = grid
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var frame = FindTemplatePixelAlignedBorder(grid, "Frame");
            frame.CornerRadius.ShouldBe(cornerRadius);
            frame.BorderThickness.ShouldBe(borderThickness);
            frame.BorderBrush.ShouldNotBeNull();

            var contentClip = FindTemplateBorder(grid, "FrameContentClip");
            contentClip.CornerRadius.ShouldBe(cornerRadius);
            contentClip.ClipToBounds.ShouldBeTrue();

            var headerFrame = FindTemplateBorder(grid, "ColumnHeadersPresenterFrame");
            headerFrame.CornerRadius.ShouldBe(GetTopOnlyCornerRadius(cornerRadius));
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Frame_And_Header_CornerRadius_Update_When_Runtime_Properties_Change()
    {
        var firstCornerRadius  = new CornerRadius(3, 5, 7, 11);
        var secondCornerRadius = new CornerRadius(13, 17, 19, 23);
        var borderThickness    = new Thickness(1, 2, 3, 4);
        var grid = new global::AtomUI.Desktop.Controls.DataGrid
        {
            AutoGenerateColumns = false,
            BorderThickness     = borderThickness,
            CornerRadius        = firstCornerRadius,
            Width               = 360,
            Height              = 180
        };

        var window = new AvaloniaWindow
        {
            Width   = 420,
            Height  = 260,
            Content = grid
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var frame       = FindTemplatePixelAlignedBorder(grid, "Frame");
            var headerFrame = FindTemplateBorder(grid, "ColumnHeadersPresenterFrame");
            frame.CornerRadius.ShouldBe(GetTopOnlyCornerRadius(firstCornerRadius));
            frame.BorderThickness.ShouldBe(new Thickness(0));
            headerFrame.CornerRadius.ShouldBe(GetTopOnlyCornerRadius(firstCornerRadius));

            grid.IsFrameBorderVisible = true;
            Dispatcher.UIThread.RunJobs();
            frame.CornerRadius.ShouldBe(firstCornerRadius);
            frame.BorderThickness.ShouldBe(borderThickness);
            headerFrame.CornerRadius.ShouldBe(GetTopOnlyCornerRadius(firstCornerRadius));

            grid.CornerRadius = secondCornerRadius;
            Dispatcher.UIThread.RunJobs();
            frame.CornerRadius.ShouldBe(secondCornerRadius);
            headerFrame.CornerRadius.ShouldBe(GetTopOnlyCornerRadius(secondCornerRadius));
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Header_Separator_And_Header_Cell_Separators_Apply_Border_Brushes()
    {
        var grid = CreateGridWithRows(isFrameBorderVisible: false);
        var window = new AvaloniaWindow
        {
            Width   = 420,
            Height  = 260,
            Content = grid
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var headerBottomSeparator = FindTemplatePixelAlignedBorder(grid, "ColumnHeadersAndRowsSeparator");
            headerBottomSeparator.BorderThickness.ShouldBe(new Thickness(0, 0, 0, 1));
            headerBottomSeparator.BorderBrush.ShouldNotBeNull();

            var headerSeparators = grid.GetVisualDescendants()
                                       .OfType<PixelAlignedBorder>()
                                       .Where(border => border.Name == "PART_VerticalSeparator")
                                       .ToList();
            headerSeparators.ShouldNotBeEmpty();
            headerSeparators.ShouldAllBe(separator => separator.BorderBrush != null);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Last_Displayed_Row_Does_Not_Draw_Bottom_Grid_Line_When_Frame_Owns_Bottom_Border()
    {
        var grid = CreateGridWithRows(isFrameBorderVisible: true);
        var window = new AvaloniaWindow
        {
            Width   = 420,
            Height  = 260,
            Content = grid
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var frame = FindTemplatePixelAlignedBorder(grid, "Frame");
            frame.CornerRadius.ShouldBe(grid.CornerRadius);
            frame.BorderThickness.ShouldBe(grid.BorderThickness);
            frame.BorderBrush.ShouldNotBeNull();

            var rows = GetDisplayedRows(grid);
            rows.Length.ShouldBe(3);
            foreach (var row in rows.Take(rows.Length - 1))
            {
                FindBottomGridLine(row).IsVisible.ShouldBeTrue();
            }

            FindBottomGridLine(rows[^1]).IsVisible.ShouldBeFalse();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Last_Displayed_Row_Keeps_Bottom_Grid_Line_When_Frame_Border_Is_Hidden()
    {
        var grid = CreateGridWithRows(isFrameBorderVisible: false);
        var window = new AvaloniaWindow
        {
            Width   = 420,
            Height  = 260,
            Content = grid
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var rows = GetDisplayedRows(grid);
            rows.Length.ShouldBe(3);
            FindBottomGridLine(rows[^1]).IsVisible.ShouldBeTrue();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static Border FindTemplateBorder(Control owner, string name)
    {
        return owner.GetVisualDescendants()
                    .OfType<Border>()
                    .Single(border => border.Name == name && ReferenceEquals(border.TemplatedParent, owner));
    }

    private static PixelAlignedBorder FindTemplatePixelAlignedBorder(Control owner, string name)
    {
        return owner.GetVisualDescendants()
                    .OfType<PixelAlignedBorder>()
                    .Single(border => border.Name == name && ReferenceEquals(border.TemplatedParent, owner));
    }

    private static CornerRadius GetTopOnlyCornerRadius(CornerRadius cornerRadius)
    {
        return new CornerRadius(cornerRadius.TopLeft, cornerRadius.TopRight, 0, 0);
    }

    private static global::AtomUI.Desktop.Controls.DataGrid CreateGridWithRows(bool isFrameBorderVisible)
    {
        var grid = new global::AtomUI.Desktop.Controls.DataGrid
        {
            AutoGenerateColumns  = false,
            BorderThickness      = new Thickness(1),
            CornerRadius         = new CornerRadius(6),
            IsFrameBorderVisible = isFrameBorderVisible,
            ItemsSource = new TestDataGridSource<GridRow>(new[]
            {
                new GridRow("John"),
                new GridRow("Jim"),
                new GridRow("Joe")
            }),
            Width = 360
        };

        grid.Columns.Add(new DataGridTextColumn
        {
            Header  = "Name",
            Binding = new Binding(nameof(GridRow.Name)),
            Width   = new DataGridLength(1, DataGridLengthUnitType.Star)
        });

        return grid;
    }

    private static global::AtomUI.Desktop.Controls.DataGridRow[] GetDisplayedRows(Control owner)
    {
        return owner.GetVisualDescendants()
                    .OfType<global::AtomUI.Desktop.Controls.DataGridRow>()
                    .OrderBy(row => row.Index)
                    .ToArray();
    }

    private static Rectangle FindBottomGridLine(Control row)
    {
        return row.GetVisualDescendants()
                  .OfType<Rectangle>()
                  .Single(rectangle => rectangle.Name == "PART_BottomGridLine");
    }

    private sealed record GridRow(string Name);
}
