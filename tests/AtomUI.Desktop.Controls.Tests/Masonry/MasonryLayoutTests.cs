using System.Collections.ObjectModel;
using System.Diagnostics;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUI.Controls.Primitives;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Masonry;

public class MasonryLayoutTests
{
    static MasonryLayoutTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Default_Layout_Property_Values_Are_Stable()
    {
        var masonry = new AtomUI.Desktop.Controls.Masonry();

        masonry.ColumnCount.ShouldBe(0);
        masonry.ColumnInfo.ShouldBeNull();
        masonry.MinColumnWidth.ShouldBe(320d);
        masonry.MaxColumnCount.ShouldBe(4);
        masonry.ColumnGap.ShouldBe(16d);
        masonry.RowGap.ShouldBe(16d);
        masonry.Gutter.ShouldBeNull();
        masonry.LayoutStrategy.ShouldBe(MasonryLayoutStrategy.StableColumns);
    }

    [Fact]
    public void Arrange_Reuses_Measured_Layout_When_Width_Is_Unchanged()
    {
        var panel = new TestMasonryPanel
        {
            ColumnCount = 1,
            ColumnGap = 0,
            RowGap = 0
        };
        var child = new VariableHeightControl(40);
        panel.Children.Add(child);

        panel.MeasureForTest(new Size(100, double.PositiveInfinity));
        child.MeasuredHeight = 80;
        child.InvalidateMeasure();
        child.Measure(new Size(100, double.PositiveInfinity));
        panel.ArrangeForTest(new Size(100, 100));

        child.Bounds.Height.ShouldBe(40,
            "Arrange must reuse the layout produced by Measure when the effective width is unchanged");
    }

    [Fact]
    public void Arrange_Recalculates_Layout_When_Width_Changes()
    {
        var panel = new TestMasonryPanel
        {
            ColumnCount = 1,
            ColumnGap = 0,
            RowGap = 0
        };
        var child = new VariableHeightControl(40);
        panel.Children.Add(child);

        panel.MeasureForTest(new Size(100, double.PositiveInfinity));
        child.MeasuredHeight = 80;
        child.InvalidateMeasure();
        child.Measure(new Size(200, double.PositiveInfinity));
        panel.ArrangeForTest(new Size(200, 100));

        child.Bounds.Height.ShouldBe(80,
            "a changed effective width must invalidate the cached Masonry layout");
    }

    [Fact]
    public void Arrange_ReMeasures_Children_When_Unbounded_Measure_Width_Differs_From_Final_Width()
    {
        var panel = new TestMasonryPanel
        {
            ColumnCount = 2,
            ColumnGap = 0,
            RowGap = 0
        };
        var first = new WidthDependentHeightControl();
        var second = new WidthDependentHeightControl();
        panel.Children.Add(first);
        panel.Children.Add(second);

        panel.MeasureForTest(new Size(double.PositiveInfinity, double.PositiveInfinity));
        panel.ArrangeForTest(new Size(400, 400));

        first.Bounds.Width.ShouldBe(200, 0.01);
        first.Bounds.Height.ShouldBe(100, 0.01,
            "a child measured with an unbounded width must be re-measured for the final Masonry column width");
        second.Bounds.Height.ShouldBe(100, 0.01);
    }

    [Fact]
    public void StableColumns_Strategy_Keeps_Item_Columns_When_Width_Changes()
    {
        var narrowFirstPanel = CreateGalleryImagePanel();
        var narrowColumns = ArrangeAtColumnWidth(narrowFirstPanel, 261.49);
        narrowColumns.ShouldBe(new[] { 0, 1, 2, 3, 0, 2, 3, 3, 1, 1, 3, 2, 0, 1, 2, 3 });

        var widerColumns = ArrangeAtColumnWidth(narrowFirstPanel, 261.51);
        widerColumns.ShouldBe(narrowColumns,
            "StableColumns must not reassign existing cards while the effective column count is unchanged");

        var wideFirstPanel = CreateGalleryImagePanel();
        var wideColumns = ArrangeAtColumnWidth(wideFirstPanel, 261.51);
        wideColumns.ShouldBe(new[] { 0, 1, 2, 3, 0, 2, 3, 3, 1, 3, 1, 2, 0, 3, 2, 1 });

        var narrowerColumns = ArrangeAtColumnWidth(wideFirstPanel, 261.49);
        narrowerColumns.ShouldBe(wideColumns,
            "StableColumns must preserve assignments while resizing in either direction");
    }

    [Fact]
    public void Reflow_Strategy_Recomputes_Item_Columns_When_Width_Changes()
    {
        var panel = CreateGalleryImagePanel();
        panel.LayoutStrategy = MasonryLayoutStrategy.Reflow;

        var narrowColumns = ArrangeAtColumnWidth(panel, 261.49);
        var widerColumns = ArrangeAtColumnWidth(panel, 261.51);

        widerColumns.ShouldBe(new[] { 0, 1, 2, 3, 0, 2, 3, 3, 1, 3, 1, 2, 0, 3, 2, 1 });
        widerColumns.ShouldNotBe(narrowColumns,
            "Reflow must recompute the shortest-column assignment after a resize");
    }

    [Fact]
    public void LayoutStrategy_Is_Forwarded_To_The_Default_ItemsPanel()
    {
        var masonry = new AtomUI.Desktop.Controls.Masonry
        {
            Width = 400,
            Height = 300,
            ColumnCount = 2,
            LayoutStrategy = MasonryLayoutStrategy.Reflow
        };
        masonry.Items.Add(new Border { Height = 40 });
        var window = new AvaloniaWindow
        {
            Width = 400,
            Height = 300,
            Content = masonry
        };

        try
        {
            window.Show();
            RunLayoutJobs();

            var panel = masonry.GetVisualDescendants().OfType<MasonryPanel>().Single();
            panel.LayoutStrategy.ShouldBe(MasonryLayoutStrategy.Reflow);

            masonry.LayoutStrategy = MasonryLayoutStrategy.StableColumns;
            RunLayoutJobs();

            panel.LayoutStrategy.ShouldBe(MasonryLayoutStrategy.StableColumns);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void StableColumns_Tracks_Existing_Items_By_Control_Instance()
    {
        var panel = new TestMasonryPanel
        {
            ColumnCount = 2,
            ColumnGap = 0,
            RowGap = 0
        };
        var first = new Border { Height = 100 };
        var second = new Border { Height = 200 };
        var third = new Border { Height = 20 };
        panel.Children.Add(first);
        panel.Children.Add(second);
        panel.Children.Add(third);

        panel.MeasureForTest(new Size(200, double.PositiveInfinity));
        panel.ArrangeForTest(new Size(200, 400));
        new[] { first.Bounds.X, second.Bounds.X, third.Bounds.X }.ShouldBe(new[] { 0d, 100d, 0d });

        panel.Children.Insert(0, new Border { Height = 10 });
        panel.MeasureForTest(new Size(200, double.PositiveInfinity));
        panel.ArrangeForTest(new Size(200, 400));

        new[] { first.Bounds.X, second.Bounds.X, third.Bounds.X }.ShouldBe(new[] { 0d, 100d, 0d },
            "inserting an item must not move existing controls merely because their indexes changed");
    }

    [Fact]
    public void StableColumns_Recomputes_Assignments_When_Column_Count_Changes()
    {
        var panel = new TestMasonryPanel
        {
            ColumnCount = 2,
            ColumnGap = 0,
            RowGap = 0
        };
        for (var i = 0; i < 4; i++)
        {
            panel.Children.Add(new Border { Height = 100 });
        }

        panel.MeasureForTest(new Size(200, double.PositiveInfinity));
        panel.ArrangeForTest(new Size(200, 400));

        panel.ColumnCount = 3;
        panel.MeasureForTest(new Size(300, double.PositiveInfinity));
        panel.ArrangeForTest(new Size(300, 400));

        panel.Children.Select(child => child.Bounds.X).ShouldBe(new[] { 0d, 100d, 200d, 0d });
    }

    [Fact]
    public void Root_Chrome_Properties_Are_Projected_Through_The_Default_Theme()
    {
        var masonry = new AtomUI.Desktop.Controls.Masonry
        {
            Background = Brushes.WhiteSmoke,
            BorderBrush = Brushes.DodgerBlue,
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(16, 12),
            ColumnCount = 2,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        masonry.Items.Add(new Border
        {
            Height = 40,
            Background = Brushes.White
        });
        masonry.Items.Add(new Border
        {
            Height = 50,
            Background = Brushes.White
        });

        var window = new AvaloniaWindow
        {
            Width   = 420,
            Height  = 260,
            Content = masonry
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var rootBorder = masonry.GetVisualDescendants()
                                    .OfType<PixelAlignedBorder>()
                                    .Single(border => border.Name == "PART_RootBorder");
            rootBorder.Background.ShouldBe(Brushes.WhiteSmoke);
            rootBorder.BorderBrush.ShouldBe(Brushes.DodgerBlue);
            rootBorder.BorderThickness.ShouldBe(new Thickness(2));
            rootBorder.CornerRadius.ShouldBe(new CornerRadius(12));
            rootBorder.Padding.ShouldBe(new Thickness(16, 12));
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    /// <summary>
    /// Regression: a fixed ColumnCount=4 with many varying-height items must arrange the
    /// generated containers into 4 distinct x-columns and never overlap. Previously the
    /// Masonry items collapsed into a single solid block in the Gallery.
    /// </summary>
    [Fact]
    public void Fixed_Column_Count_Produces_Non_Overlapping_Columns()
    {
        var masonry = new AtomUI.Desktop.Controls.Masonry
        {
            ColumnCount = 4,
            ColumnGap   = 16,
            RowGap      = 16,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ItemTemplate = new FuncDataTemplate<_Item>(
                _ => true,
                item => new Border
                {
                    Height = item!.Height,
                    Background = Brushes.White,
                    Child = new TextBlock { Text = item.Index.ToString() }
                })
        };

        var items = new ObservableCollection<_Item>();
        var heights = new double[] { 150, 50, 90, 70, 180, 150, 130, 80, 50, 90, 100, 150, 60, 50, 80 };
        for (var i = 0; i < heights.Length; i++)
        {
            items.Add(new _Item(i + 1, heights[i]));
        }
        masonry.ItemsSource = items;

        var window = new AvaloniaWindow
        {
            Width   = 1024,
            Height  = 800,
            Content = masonry
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            // Collect every generated item container (ContentPresenter wrapping the template).
            var presenters = masonry
                .GetLogicalDescendants()
                .OfType<ContentPresenter>()
                .ToArray();

            presenters.Length.ShouldBe(heights.Length,
                "Masonry should generate one container per item");

            // After layout, every container must have a non-zero arranged rect.
            foreach (var p in presenters)
            {
                p.Bounds.Width.ShouldBeGreaterThan(0);
                p.Bounds.Height.ShouldBeGreaterThan(0);
            }

            // Group containers by their column (x position bucket). Expect <= 4 columns.
            var xs = presenters.Select(p => Math.Round(p.Bounds.X)).Distinct().OrderBy(v => v).ToArray();
            xs.Length.ShouldBeLessThanOrEqualTo(4);
            xs.Length.ShouldBeGreaterThan(1,
                "Items must spread across multiple columns, not collapse into one");

            // No two same-column items may vertically overlap.
            foreach (var group in presenters.GroupBy(p => Math.Round(p.Bounds.X)))
            {
                var ordered = group.OrderBy(p => p.Bounds.Y).ToArray();
                for (var i = 1; i < ordered.Length; i++)
                {
                    var prevBottom = ordered[i - 1].Bounds.Bottom;
                    ordered[i].Bounds.Y.ShouldBeGreaterThanOrEqualTo(prevBottom,
                        $"item at x={group.Key} overlaps previous item (prevBottom={prevBottom})");
                }
            }
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Async_Image_Loading_Content_Provides_The_Initial_Masonry_Height()
    {
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var source = new StreamImageSource(
            async token =>
            {
                started.TrySetResult();
                await release.Task.WaitAsync(token);
                return new MemoryStream(new byte[] { 1 });
            },
            $"masonry-loading-{Guid.NewGuid():N}",
            "v1");
        AsyncImage? image = null;
        AtomUI.Desktop.Controls.Skeleton? skeleton = null;
        var masonry = new AtomUI.Desktop.Controls.Masonry
        {
            ColumnCount = 4,
            ColumnGap = 16,
            RowGap = 16,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ItemsSource = new[] { source },
            ItemTemplate = new FuncDataTemplate<ImageSource>(
                _ => true,
                item => image = new AsyncImage
                {
                    Source = item,
                    MinHeight = 210,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    LoadingContent = new Border
                    {
                        Padding = new Thickness(16),
                        Child = skeleton = new AtomUI.Desktop.Controls.Skeleton
                        {
                            IsLoading = true,
                            IsActive = true,
                            IsShowAvatar = false,
                            IsShowTitle = true,
                            ParagraphRows = 3,
                            IsRound = true
                        }
                    }
                })
        };
        var window = new AvaloniaWindow
        {
            Width = 1024,
            Height = 800,
            Content = masonry
        };

        try
        {
            window.Show();
            RunLayoutJobs();

            image.ShouldNotBeNull();
            image.IsLoading.ShouldBeTrue();
            image.DesiredSize.Height.ShouldBeGreaterThanOrEqualTo(210);
            image.GetVisualDescendants()
                 .OfType<ContentPresenter>()
                 .Single(control => control.Name == "PART_LoadingPresenter")
                 .IsVisible.ShouldBeTrue();
            skeleton.ShouldNotBeNull();
            skeleton.Bounds.Width.ShouldBeGreaterThan(0);
            skeleton.GetVisualDescendants()
                    .OfType<SkeletonLine>()
                    .ShouldContain(line => line.Bounds.Width > 0);
            WaitUntil(() => started.Task.IsCompleted, "Masonry async image request start");
        }
        finally
        {
            release.TrySetResult();
            window.Close();
        }
    }

    /// <summary>
    /// Regression: an item whose template contains a child with an intrinsic (large) desired
    /// size — e.g. an Image whose source loads asynchronously — must NOT inflate the masonry
    /// card beyond its bound Height. Previously the cover image's native size leaked into the
    /// card's DesiredSize, so MasonryPanel (which lays out by child.DesiredSize.Height) placed
    /// the card with the wrong height and the image overflowed onto neighbouring cards.
    /// The fix pins the cover region to a fixed-height border so the card height is known up
    /// front and does not change when the image arrives.
    /// </summary>
    [Fact]
    public void Cover_Image_Does_Not_Inflate_Card_Height()
    {
        // A child that reports a large intrinsic desired size, mirroring an Image whose source
        // has loaded with a native pixel size.
        Control BigCoverFactory() => new Border
        {
            Height = 100,                 // fixed cover region (the fix)
            ClipToBounds = true,
            Child = new Border { Width = 523, Height = 392, Background = Brushes.CornflowerBlue }
        };

        var masonry = new AtomUI.Desktop.Controls.Masonry
        {
            ColumnCount = 4,
            ColumnGap   = 16,
            RowGap      = 16,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ItemTemplate = new FuncDataTemplate<_Item>(
                _ => true,
                item =>
                {
                    // Card-like host: a fixed Height wrapping a content presenter.
                    return new Border
                    {
                        Height = item!.Height,
                        ClipToBounds = true,
                        Child = item.IsSpecial
                            ? new StackPanel { Children = { BigCoverFactory() } }
                            : (Control)new TextBlock { Text = item.Index.ToString() }
                    };
                })
        };

        var items = new ObservableCollection<_Item>
        {
            new _Item(1, 150, false),
            new _Item(2, 50, false),
            new _Item(3, 90, false),
            new _Item(4, 70, false),
            new _Item(5, 180, true),   // special card with the big cover
            new _Item(6, 150, false),
        };
        masonry.ItemsSource = items;

        var window = new AvaloniaWindow { Width = 1024, Height = 800, Content = masonry };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var presenters = masonry.GetLogicalDescendants().OfType<ContentPresenter>().ToArray();
            presenters.Length.ShouldBe(items.Count);

            // Each container's arranged height must equal the bound item height — never the
            // inflated 392px intrinsic cover size.
            foreach (var p in presenters)
            {
                p.Bounds.Height.ShouldBeLessThanOrEqualTo(200,
                    "card height must stay bounded by the bound Height, not the cover's intrinsic size");
                p.Bounds.Height.ShouldBeGreaterThan(0);
            }

            // The 4-column invariant still holds.
            var xs = presenters.Select(p => Math.Round(p.Bounds.X)).Distinct().OrderBy(v => v).ToArray();
            xs.Length.ShouldBeLessThanOrEqualTo(4);
            xs.Length.ShouldBeGreaterThan(1);

            // No vertical overlap within a column.
            foreach (var group in presenters.GroupBy(p => Math.Round(p.Bounds.X)))
            {
                var ordered = group.OrderBy(p => p.Bounds.Y).ToArray();
                for (var i = 1; i < ordered.Length; i++)
                {
                    ordered[i].Bounds.Y.ShouldBeGreaterThanOrEqualTo(ordered[i - 1].Bounds.Bottom);
                }
            }
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Responsive_ColumnInfo_Cascades_To_Current_Breakpoint()
    {
        var host = new TestMediaBreakHost(MediaBreakPoint.Large)
        {
            Width = 900,
            Height = 600
        };
        var masonry = new AtomUI.Desktop.Controls.Masonry
        {
            ColumnInfo = ResponsiveInt.Parse("xs: 1, md: 3"),
            ColumnGap = 0,
            RowGap = 0,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ItemTemplate = new FuncDataTemplate<_Item>(
                _ => true,
                item => new Border { Height = item!.Height, Child = new TextBlock { Text = item.Index.ToString() } })
        };

        masonry.ItemsSource = new ObservableCollection<_Item>
        {
            new(1, 40),
            new(2, 40),
            new(3, 40),
            new(4, 40),
            new(5, 40),
            new(6, 40)
        };
        host.Children.Add(masonry);

        var window = new AvaloniaWindow { Width = 900, Height = 600, Content = host };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var presenters = masonry.GetLogicalDescendants().OfType<ContentPresenter>().ToArray();
            var xs = presenters.Select(p => Math.Round(p.Bounds.X)).Distinct().OrderBy(v => v).ToArray();
            xs.Length.ShouldBe(3);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Responsive_ColumnInfo_Falls_Back_To_ColumnCount_When_Current_Breakpoint_Is_Not_Configured()
    {
        var host = new TestMediaBreakHost(MediaBreakPoint.Large)
        {
            Width = 900,
            Height = 600
        };
        var masonry = new AtomUI.Desktop.Controls.Masonry
        {
            ColumnInfo = ResponsiveInt.Parse("xl: 4"),
            ColumnCount = 2,
            ColumnGap = 0,
            RowGap = 0,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ItemTemplate = new FuncDataTemplate<_Item>(
                _ => true,
                item => new Border { Height = item!.Height, Child = new TextBlock { Text = item.Index.ToString() } })
        };

        masonry.ItemsSource = new ObservableCollection<_Item>
        {
            new(1, 40),
            new(2, 40),
            new(3, 40),
            new(4, 40)
        };
        host.Children.Add(masonry);

        var window = new AvaloniaWindow { Width = 900, Height = 600, Content = host };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var presenters = masonry.GetLogicalDescendants().OfType<ContentPresenter>().ToArray();
            presenters.Select(p => Math.Round(p.Bounds.X)).Distinct().Count().ShouldBe(2);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Responsive_Gutter_Cascades_Horizontal_And_Vertical_Independently()
    {
        var host = new TestMediaBreakHost(MediaBreakPoint.Large)
        {
            Width = 500,
            Height = 600
        };
        var masonry = new AtomUI.Desktop.Controls.Masonry
        {
            ColumnInfo = 2,
            Gutter = ResponsiveGutter.Parse("xs: 8, md: 20; xs: 4, md: 12"),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ItemTemplate = new FuncDataTemplate<_Item>(
                _ => true,
                item => new Border { Height = item!.Height, Child = new TextBlock { Text = item.Index.ToString() } })
        };

        masonry.ItemsSource = new ObservableCollection<_Item>
        {
            new(1, 40),
            new(2, 40),
            new(3, 40)
        };
        host.Children.Add(masonry);

        var window = new AvaloniaWindow { Width = 500, Height = 600, Content = host };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var presenters = masonry.GetLogicalDescendants().OfType<ContentPresenter>().OrderBy(p => p.Bounds.Y).ThenBy(p => p.Bounds.X).ToArray();
            presenters[1].Bounds.X.ShouldBe(260, 0.5);
            presenters[2].Bounds.Y.ShouldBe(52, 0.5);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Responsive_Gutter_Falls_Back_To_ColumnGap_And_RowGap_When_Current_Breakpoint_Is_Not_Configured()
    {
        var host = new TestMediaBreakHost(MediaBreakPoint.Large)
        {
            Width = 500,
            Height = 600
        };
        var masonry = new AtomUI.Desktop.Controls.Masonry
        {
            ColumnInfo = 2,
            Gutter = ResponsiveGutter.Parse("xl: 30; xl: 30"),
            ColumnGap = 20,
            RowGap = 8,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ItemTemplate = new FuncDataTemplate<_Item>(
                _ => true,
                item => new Border { Height = item!.Height, Child = new TextBlock { Text = item.Index.ToString() } })
        };

        masonry.ItemsSource = new ObservableCollection<_Item>
        {
            new(1, 40),
            new(2, 40),
            new(3, 40)
        };
        host.Children.Add(masonry);

        var window = new AvaloniaWindow { Width = 500, Height = 600, Content = host };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var presenters = masonry.GetLogicalDescendants().OfType<ContentPresenter>().OrderBy(p => p.Bounds.Y).ThenBy(p => p.Bounds.X).ToArray();
            presenters[1].Bounds.X.ShouldBe(260, 0.5);
            presenters[2].Bounds.Y.ShouldBe(48, 0.5);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Responsive_Gutter_Scalar_Applies_To_Column_And_Row_Gap()
    {
        var host = new TestMediaBreakHost(MediaBreakPoint.Large)
        {
            Width = 500,
            Height = 600
        };
        var masonry = new AtomUI.Desktop.Controls.Masonry
        {
            ColumnInfo = 2,
            Gutter = ResponsiveGutter.Parse("16"),
            RowGap = 4,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ItemTemplate = new FuncDataTemplate<_Item>(
                _ => true,
                item => new Border { Height = item!.Height, Child = new TextBlock { Text = item.Index.ToString() } })
        };

        masonry.ItemsSource = new ObservableCollection<_Item>
        {
            new(1, 40),
            new(2, 40),
            new(3, 40)
        };
        host.Children.Add(masonry);

        var window = new AvaloniaWindow { Width = 500, Height = 600, Content = host };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var presenters = masonry.GetLogicalDescendants().OfType<ContentPresenter>().OrderBy(p => p.Bounds.Y).ThenBy(p => p.Bounds.X).ToArray();
            presenters[1].Bounds.X.ShouldBe(258, 0.5);
            presenters[2].Bounds.Y.ShouldBe(56, 0.5);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Responsive_Breakpoint_Change_Recalculates_Columns_And_Gutter()
    {
        var host = new TestMediaBreakHost(MediaBreakPoint.Small)
        {
            Width = 500,
            Height = 600
        };
        var masonry = new AtomUI.Desktop.Controls.Masonry
        {
            ColumnInfo = ResponsiveInt.Parse("xs: 1, md: 2"),
            Gutter = ResponsiveGutter.Parse("xs: 8, md: 20; xs: 4, md: 12"),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ItemTemplate = new FuncDataTemplate<_Item>(
                _ => true,
                item => new Border { Height = item!.Height, Child = new TextBlock { Text = item.Index.ToString() } })
        };

        masonry.ItemsSource = new ObservableCollection<_Item>
        {
            new(1, 40),
            new(2, 40),
            new(3, 40)
        };
        host.Children.Add(masonry);

        var window = new AvaloniaWindow { Width = 500, Height = 600, Content = host };
        try
        {
            window.Show();
            RunLayoutJobs();

            var presenters = masonry.GetLogicalDescendants().OfType<ContentPresenter>().OrderBy(p => p.Bounds.Y).ThenBy(p => p.Bounds.X).ToArray();
            presenters.Select(p => Math.Round(p.Bounds.X)).Distinct().Count().ShouldBe(1);
            presenters[2].Bounds.Y.ShouldBe(88, 0.5);

            host.SetMediaBreakPoint(MediaBreakPoint.Medium);
            RunLayoutJobs();

            presenters = masonry.GetLogicalDescendants().OfType<ContentPresenter>().OrderBy(p => p.Bounds.Y).ThenBy(p => p.Bounds.X).ToArray();
            presenters.Select(p => Math.Round(p.Bounds.X)).Distinct().Count().ShouldBe(2);
            presenters[1].Bounds.X.ShouldBe(260, 0.5);
            presenters[2].Bounds.Y.ShouldBe(52, 0.5);
        }
        finally
        {
            window.Close();
        }
    }

    private sealed class _Item
    {
        public int Index { get; }
        public double Height { get; }
        public bool IsSpecial { get; }
        public _Item(int index, double height, bool isSpecial = false)
        {
            Index = index;
            Height = height;
            IsSpecial = isSpecial;
        }
    }

    private sealed class _Vm
    {
        public ObservableCollection<_Item> Items { get; }
        public _Vm()
        {
            var heights = new double[] { 150, 50, 90, 70, 180, 150, 130, 80, 50, 90, 100, 150, 60, 50, 80 };
            var list = new ObservableCollection<_Item>();
            for (var i = 0; i < heights.Length; i++) list.Add(new _Item(i + 1, heights[i]));
            Items = list;
        }
    }

    /// <summary>
    /// Gallery-faithful reproduction: ItemsSource bound via DataContext (like the showcase's
    /// DeferredContentTemplate), and the Masonry hosted inside a ScrollViewer that offers an
    /// unconstrained (infinite) measure width — the real nesting path. Must still produce
    /// 4 distinct, non-overlapping columns rather than collapsing.
    /// </summary>
    [Fact]
    public void Gallery_Nesting_DataContext_Binding_And_Scroll_Host_Still_Columns()
    {
        var masonry = new AtomUI.Desktop.Controls.Masonry
        {
            ColumnCount = 4,
            ColumnGap   = 16,
            RowGap      = 16,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ItemTemplate = new FuncDataTemplate<_Item>(
                _ => true,
                item => new TextBlock { Text = item!.Index.ToString() })
        };
        masonry.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(_Vm.Items)) { Source = new _Vm() });

        // ScrollViewer is the gallery-like host that gives an infinite measure width to content.
        var scroll = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility   = ScrollBarVisibility.Auto,
            Width = 1024,
            Height = 800,
            Content = masonry
        };

        var window = new AvaloniaWindow { Width = 1100, Height = 900, Content = scroll };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var presenters = masonry.GetLogicalDescendants().OfType<ContentPresenter>().ToArray();
            presenters.Length.ShouldBe(15);

            var xs = presenters.Select(p => Math.Round(p.Bounds.X)).Distinct().OrderBy(v => v).ToArray();
            xs.Length.ShouldBe(4, $"expected exactly 4 columns, got {xs.Length} at x={string.Join(",", xs)}");

            // Column width must be the masonry-allocated column width, not stretched full-width.
            var colWidths = presenters.Select(p => p.Bounds.Width).Distinct().ToArray();
            foreach (var w in colWidths)
            {
                w.ShouldBeLessThan(900, "column width must be a fraction of the panel, not the full viewport");
            }

            // No vertical overlap within a column.
            foreach (var group in presenters.GroupBy(p => Math.Round(p.Bounds.X)))
            {
                var ordered = group.OrderBy(p => p.Bounds.Y).ToArray();
                for (var i = 1; i < ordered.Length; i++)
                {
                    ordered[i].Bounds.Y.ShouldBeGreaterThanOrEqualTo(ordered[i - 1].Bounds.Bottom);
                }
            }
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Direct_Child_Attached_Property_Change_Invalidates_Layout()
    {
        var first  = new Border { Height = 100, Background = Brushes.White };
        var second = new Border { Height = 100, Background = Brushes.White };
        var third  = new Border { Height = 20, Background = Brushes.White };

        var masonry = new AtomUI.Desktop.Controls.Masonry
        {
            ColumnCount = 2,
            ColumnGap   = 10,
            RowGap      = 0,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        masonry.Items.Add(first);
        masonry.Items.Add(second);
        masonry.Items.Add(third);

        var window = new AvaloniaWindow
        {
            Width   = 210,
            Height  = 300,
            Content = masonry
        };

        try
        {
            window.Show();
            RunLayoutJobs();

            Math.Round(third.Bounds.X).ShouldBe(0);

            AtomUI.Desktop.Controls.Masonry.SetColumn(third, 1);
            RunLayoutJobs();

            Math.Round(third.Bounds.X).ShouldBe(110,
                "changing Masonry.Column on a direct item container must trigger a new layout pass");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void LayoutChanged_Notifies_Once_When_ItemsSource_Becomes_Empty()
    {
        var items = new ObservableCollection<_Item>
        {
            new(1, 60),
            new(2, 60)
        };
        var notifications = new List<MasonryLayoutChangedEventArgs>();
        var masonry = new AtomUI.Desktop.Controls.Masonry
        {
            ColumnCount = 2,
            ColumnGap   = 10,
            RowGap      = 0,
            ItemsSource = items,
            ItemTemplate = new FuncDataTemplate<_Item>(
                _ => true,
                item => new Border
                {
                    Height = item!.Height,
                    Background = Brushes.White
                })
        };
        masonry.LayoutChanged += (_, args) => notifications.Add(args);

        var window = new AvaloniaWindow
        {
            Width   = 210,
            Height  = 300,
            Content = masonry
        };

        try
        {
            window.Show();
            RunLayoutJobs();
            notifications.Count.ShouldBe(1);
            notifications[0].Items.Count.ShouldBe(2);

            items.Clear();
            RunLayoutJobs();

            notifications.Count.ShouldBe(2,
                "transition from a non-empty layout to an empty layout must dispatch exactly one empty notification");
            notifications[1].Items.Count.ShouldBe(0);

            RunLayoutJobs();
            notifications.Count.ShouldBe(2,
                "a layout that remains empty must not repeatedly dispatch equivalent empty notifications");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Replacing_ItemsPanel_Replaces_Default_Masonry_Layout_Engine()
    {
        var first  = new Border { Height = 40, Background = Brushes.White };
        var second = new Border { Height = 40, Background = Brushes.White };

        var masonry = new AtomUI.Desktop.Controls.Masonry
        {
            ColumnCount = 2,
            ColumnGap   = 10,
            RowGap      = 0,
            ItemsPanel  = new FuncTemplate<Panel?>(() => new StackPanel()),
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        masonry.Items.Add(first);
        masonry.Items.Add(second);

        var window = new AvaloniaWindow
        {
            Width   = 210,
            Height  = 300,
            Content = masonry
        };

        try
        {
            window.Show();
            RunLayoutJobs();

            Math.Round(first.Bounds.X).ShouldBe(0);
            Math.Round(second.Bounds.X).ShouldBe(0,
                "an explicit ItemsPanel must be honored instead of forcing the internal MasonryPanel");
            second.Bounds.Y.ShouldBeGreaterThan(0);
        }
        finally
        {
            window.Close();
        }
    }

    private static void RunLayoutJobs()
    {
        Dispatcher.UIThread.RunJobs();
        Dispatcher.UIThread.RunJobs();
    }

    private static TestMasonryPanel CreateGalleryImagePanel()
    {
        var panel = new TestMasonryPanel
        {
            ColumnCount = 4,
            ColumnGap = 16,
            RowGap = 16,
            UseLayoutRounding = false
        };
        var imageHeights = new[] { 349, 785, 349, 349, 930, 785, 349, 349, 294, 349, 697, 349, 732, 784, 349, 349 };
        foreach (var imageHeight in imageHeights)
        {
            panel.Children.Add(new AspectRatioHeightControl(imageHeight / 523d)
            {
                UseLayoutRounding = false
            });
        }
        return panel;
    }

    private static int[] ArrangeAtColumnWidth(TestMasonryPanel panel, double columnWidth)
    {
        var panelWidth = columnWidth * panel.ColumnCount + panel.ColumnGap * (panel.ColumnCount - 1);
        panel.MeasureForTest(new Size(panelWidth, double.PositiveInfinity));
        panel.ArrangeForTest(new Size(panelWidth, 2000));
        return panel.Children
                    .Select(child => (int)Math.Round(child.Bounds.X / (columnWidth + panel.ColumnGap)))
                    .ToArray();
    }

    private static void WaitUntil(Func<bool> predicate, string description)
    {
        var timeout = Stopwatch.StartNew();
        while (!predicate())
        {
            Dispatcher.UIThread.RunJobs();
            if (timeout.Elapsed >= TimeSpan.FromSeconds(5))
            {
                throw new TimeoutException($"Timed out waiting for {description}.");
            }
            Thread.Yield();
        }
        Dispatcher.UIThread.RunJobs();
    }

    private sealed class TestMediaBreakHost : Panel, IMediaBreakAwareControl
    {
        public TestMediaBreakHost(MediaBreakPoint breakPoint)
        {
            MediaBreakPoint = breakPoint;
        }

        public MediaBreakPoint MediaBreakPoint { get; private set; }

        public event EventHandler<MediaBreakPointChangedEventArgs>? MediaBreakPointChanged;

        public void SetMediaBreakPoint(MediaBreakPoint mediaBreakPoint)
        {
            MediaBreakPoint = mediaBreakPoint;
            MediaBreakPointChanged?.Invoke(this, new MediaBreakPointChangedEventArgs(mediaBreakPoint));
        }
    }

    private sealed class VariableHeightControl : Control
    {
        public VariableHeightControl(double height)
        {
            MeasuredHeight = height;
        }

        public double MeasuredHeight { get; set; }

        protected override Size MeasureOverride(Size availableSize)
        {
            return new Size(Math.Min(availableSize.Width, 100), MeasuredHeight);
        }
    }

    private sealed class WidthDependentHeightControl : Control
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            var width = double.IsFinite(availableSize.Width) ? availableSize.Width : 656;
            return new Size(width, width / 2);
        }
    }

    private sealed class AspectRatioHeightControl(double heightPerWidth) : Control
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            return new Size(availableSize.Width, availableSize.Width * heightPerWidth);
        }
    }

    private sealed class TestMasonryPanel : MasonryPanel
    {
        public Size MeasureForTest(Size availableSize)
        {
            return base.MeasureOverride(availableSize);
        }

        public Size ArrangeForTest(Size finalSize)
        {
            return base.ArrangeOverride(finalSize);
        }
    }
}
