using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AtomUI.Utils;
using System.Reflection;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.DataGrid.Interaction;

public class DataGridDetailExpanderColumnRecycleTests
{
    static DataGridDetailExpanderColumnRecycleTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Programmatic_Expander_Checked_State_Sync_Does_Not_Overwrite_Row_Details_State()
    {
        var grid = CreateGrid([new GridRow("Row 1")]);
        var window = new Window
        {
            Width   = 360,
            Height  = 220,
            Content = grid
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var row = CreateDetachedRow(grid, index: 0);
            row.IsDetailsVisible = true;

            var expander = new TestDataGridRowExpander
            {
                IsChecked = true
            };
            expander.NotifyLoadingRow(row);

            expander.IsChecked = false;

            row.IsDetailsVisible.ShouldBeTrue();
            grid.GetRowDetailsVisibility(0).ShouldBeTrue();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void User_Toggle_Updates_Row_Details_State()
    {
        var grid = CreateGrid([new GridRow("Row 1")]);
        var window = new Window
        {
            Width   = 360,
            Height  = 220,
            Content = grid
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var row      = CreateDetachedRow(grid, index: 0);
            var expander = new TestDataGridRowExpander();

            expander.NotifyLoadingRow(row);
            expander.InvokeToggle();

            row.IsDetailsVisible.ShouldBeTrue();
            grid.GetRowDetailsVisibility(0).ShouldBeTrue();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Scrolling_Up_Through_Expanded_Row_Details_Does_Not_Bounce_Back()
    {
        var rows = CreateOrders(80);
        var grid = CreateOrderGrid(rows);
        SetRowDetailsVisibility(grid, 5, true);
        SetRowDetailsVisibility(grid, 13, true);
        SetRowDetailsVisibility(grid, 23, true);

        var window = new Window
        {
            Width   = 1120,
            Height  = 760,
            Content = grid
        };

        try
        {
            window.Show();
            RunLayoutJobs();

            var scrollStates = new List<string>();
            for (var i = 0; i < 80; i++)
            {
                var previousOffset = grid.VerticalOffset;
                grid.UpdateScroll(new Vector(0, -120));
                RunLayoutJobs();
                scrollStates.Add(GetRangeScrollState(grid));
                if (MathUtils.AreClose(grid.VerticalOffset, previousOffset))
                {
                    break;
                }
            }

            grid.DisplayData.FirstScrollingSlot.ShouldBeGreaterThan(
                23,
                string.Join(Environment.NewLine, scrollStates));

            ScrollUpUntilSlotIsVisible(grid, 23);

            grid.DisplayData.FirstScrollingSlot.ShouldBeLessThanOrEqualTo(23);
            grid.DisplayData.LastScrollingSlot.ShouldBeGreaterThanOrEqualTo(23);

            var row = GetDisplayedRow(grid, 23);
            row.GetVisualDescendants()
               .OfType<TextBlock>()
               .Select(textBlock => textBlock.Text)
               .ShouldContain("Product 024-1");

            if (grid.DisplayData.FirstScrollingSlot == 23 && MathUtils.GreaterThan(grid.NegVerticalOffset, 0))
            {
                grid.UpdateScroll(new Vector(0, 60));
                RunLayoutJobs();
                row = GetDisplayedRow(grid, 23);
            }

            var beforeOffset = grid.VerticalOffset;

            var detailsGrid = row.GetVisualDescendants()
                                 .OfType<global::AtomUI.Desktop.Controls.DataGrid>()
                                 .Single();
            var wheelPoint = detailsGrid.TranslatePoint(
                new Point(detailsGrid.Bounds.Width / 2, detailsGrid.Bounds.Height / 2),
                window);
            wheelPoint.ShouldNotBeNull();

            window.MouseWheel(wheelPoint.Value, new Vector(0, 1));
            RunLayoutJobs();

            grid.VerticalOffset.ShouldBeLessThan(
                beforeOffset,
                $"outer={grid.VerticalOffset:0.##}/{grid.NegVerticalOffset:0.##}/f{grid.DisplayData.FirstScrollingSlot}/l{grid.DisplayData.LastScrollingSlot} " +
                $"inner={detailsGrid.VerticalOffset:0.##}/{detailsGrid.NegVerticalOffset:0.##}/f{detailsGrid.DisplayData.FirstScrollingSlot}/l{detailsGrid.DisplayData.LastScrollingSlot}");
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Scroll_Gesture_Over_Nested_Row_Details_Does_Not_Rebound_Outer_Offset()
    {
        var rows = CreateOrders(80);
        var grid = CreateOrderGrid(rows);
        SetRowDetailsVisibility(grid, 5, true);
        SetRowDetailsVisibility(grid, 13, true);
        SetRowDetailsVisibility(grid, 23, true);

        var window = new Window
        {
            Width   = 1120,
            Height  = 760,
            Content = grid
        };

        try
        {
            window.Show();
            RunLayoutJobs();

            for (var i = 0; i < 80; i++)
            {
                var previousOffset = grid.VerticalOffset;
                grid.UpdateScroll(new Vector(0, -120));
                RunLayoutJobs();
                if (MathUtils.AreClose(grid.VerticalOffset, previousOffset))
                {
                    break;
                }
            }

            grid.DisplayData.FirstScrollingSlot.ShouldBeGreaterThan(23);

            ScrollUpUntilSlotIsVisible(grid, 23);

            var row = GetDisplayedRow(grid, 23);
            var detailsRowsPresenter = row.GetVisualDescendants()
                                          .OfType<DataGridRowsPresenter>()
                                          .Single();
            var gestureId = ScrollGestureEventArgs.GetNextFreeId();

            for (var i = 0; i < 16; i++)
            {
                if (!grid.IsSlotVisible(23))
                {
                    ScrollUpUntilSlotIsVisible(grid, 23);
                    row = GetDisplayedRow(grid, 23);
                    detailsRowsPresenter = row.GetVisualDescendants()
                                              .OfType<DataGridRowsPresenter>()
                                              .Single();
                }

                var beforeOffset = grid.VerticalOffset;

                detailsRowsPresenter.RaiseEvent(new ScrollGestureEventArgs(gestureId, new Vector(0, -30)));
                RunLayoutJobs();

                grid.VerticalOffset.ShouldBeLessThanOrEqualTo(beforeOffset);
            }
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Repeated_Wheel_Over_Nested_Row_Details_Does_Not_Bounce_Back_During_Layout()
    {
        var rows = CreateOrders(80);
        var grid = CreateOrderGrid(rows);
        SetRowDetailsVisibility(grid, 5, true);
        SetRowDetailsVisibility(grid, 13, true);
        SetRowDetailsVisibility(grid, 23, true);

        var window = new Window
        {
            Width   = 1120,
            Height  = 760,
            Content = grid
        };

        try
        {
            window.Show();
            RunLayoutJobs();

            ScrollToBottom(grid);
            grid.DisplayData.FirstScrollingSlot.ShouldBeGreaterThan(23);
            ScrollUpUntilSlotIsVisible(grid, 23);

            var row = GetDisplayedRow(grid, 23);
            var detailsGrid = row.GetVisualDescendants()
                                 .OfType<global::AtomUI.Desktop.Controls.DataGrid>()
                                 .Single();
            var wheelPoint = detailsGrid.TranslatePoint(
                new Point(detailsGrid.Bounds.Width / 2, detailsGrid.Bounds.Height / 2),
                window);
            wheelPoint.ShouldNotBeNull();

            var previousOffset = grid.VerticalOffset;
            var samples = new List<double> { previousOffset };
            for (var i = 0; i < 12; i++)
            {
                window.MouseWheel(wheelPoint.Value, new Vector(0, 1));
                DrainLayout(samples, grid);

                var currentOffset = grid.VerticalOffset;
                currentOffset.ShouldBeLessThanOrEqualTo(
                    previousOffset,
                    string.Join(", ", samples.Select(offset => offset.ToString("0.##"))));
                previousOffset = currentOffset;

                if (!grid.IsSlotVisible(23))
                {
                    break;
                }
            }
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Runtime_Expanded_Row_Details_Do_Not_Rebound_When_Scrolled_Back_Into_View()
    {
        var rows = CreateOrders(80);
        var grid = CreateOrderGrid(rows);

        var window = new Window
        {
            Width   = 1120,
            Height  = 760,
            Content = grid
        };

        try
        {
            window.Show();
            RunLayoutJobs();

            ToggleRowDetails(grid, 5);
            ScrollDownUntilSlotIsVisible(grid, 13);
            ToggleRowDetails(grid, 13);
            ScrollDownUntilSlotIsVisible(grid, 23);
            ToggleRowDetails(grid, 23);

            ScrollToBottom(grid);
            grid.DisplayData.FirstScrollingSlot.ShouldBeGreaterThan(23);

            var samples = new List<string>();
            var previousOffset = grid.VerticalOffset;
            for (var i = 0; i < 80; i++)
            {
                grid.UpdateScroll(new Vector(0, 60));
                RunLayoutJobs();

                samples.Add($"{grid.VerticalOffset:0.##}/f{grid.DisplayData.FirstScrollingSlot}/l{grid.DisplayData.LastScrollingSlot}");
                grid.VerticalOffset.ShouldBeLessThanOrEqualTo(previousOffset, string.Join(", ", samples));
                previousOffset = grid.VerticalOffset;

                if (grid.DisplayData.FirstScrollingSlot <= 23 && grid.DisplayData.LastScrollingSlot >= 23)
                {
                    break;
                }
            }

            previousOffset = grid.VerticalOffset;
            for (var i = 0; i < 16; i++)
            {
                var row = GetDisplayedRow(grid, 23);
                var detailsRowsPresenter = row.GetVisualDescendants()
                                              .OfType<DataGridRowsPresenter>()
                                              .Single();
                var gestureId = ScrollGestureEventArgs.GetNextFreeId();
                detailsRowsPresenter.RaiseEvent(new ScrollGestureEventArgs(gestureId, new Vector(0, -30)));
                RunLayoutJobs();

                samples.Add($"{grid.VerticalOffset:0.##}/f{grid.DisplayData.FirstScrollingSlot}/l{grid.DisplayData.LastScrollingSlot}");
                grid.VerticalOffset.ShouldBeLessThanOrEqualTo(previousOffset, string.Join(", ", samples));
                previousOffset = grid.VerticalOffset;

                if (!grid.IsSlotVisible(23))
                {
                    break;
                }
            }
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Realizing_Row_Details_Near_Viewport_Bottom_Keeps_First_Row_Offset_Normalized()
    {
        var rows = CreateOrders(80);
        var grid = CreateOrderGrid(rows);

        var window = new Window
        {
            Width   = 1120,
            Height  = 760,
            Content = grid
        };

        var invalidStates = new List<string>();

        void CaptureState(string source)
        {
            if (grid.DisplayData.FirstScrollingSlot < 0)
            {
                return;
            }

            var firstElement = grid.DisplayData.GetDisplayedElement(grid.DisplayData.FirstScrollingSlot);
            var firstHeight = firstElement is null ? 0 : grid.GetDisplayedElementHeight(firstElement);
            if (MathUtils.GreaterThan(grid.NegVerticalOffset, firstHeight))
            {
                invalidStates.Add(
                    $"{source}: offset={grid.VerticalOffset:0.##} neg={grid.NegVerticalOffset:0.##} " +
                    $"first={grid.DisplayData.FirstScrollingSlot} last={grid.DisplayData.LastScrollingSlot} " +
                    $"firstHeight={firstHeight:0.##} max={grid.VerticalScrollBar?.Maximum:0.##}");
            }
        }

        try
        {
            window.Show();
            RunLayoutJobs();

            grid.LayoutUpdated += (_, _) => CaptureState("layout");
            grid.VerticalScrollBar.ShouldNotBeNull();
            grid.VerticalScrollBar.PropertyChanged += (_, args) =>
            {
                if (args.Property == RangeBase.ValueProperty ||
                    args.Property == RangeBase.MaximumProperty)
                {
                    CaptureState($"scrollbar:{args.Property.Name}");
                }
            };

            ToggleRowDetails(grid, 5);
            ScrollDownUntilSlotIsVisible(grid, 13);
            ToggleRowDetails(grid, 13);
            ScrollDownUntilSlotIsVisible(grid, 23);
            ToggleRowDetails(grid, 23);

            ScrollToBottom(grid);
            ScrollUpUntilSlotIsVisible(grid, 23);

            for (var i = 0; i < 24; i++)
            {
                var verticalScrollBar = grid.VerticalScrollBar!;
                verticalScrollBar.Value = Math.Max(0, verticalScrollBar.Value - 2);
                RunLayoutJobs();

                if (grid.DisplayData.FirstScrollingSlot <= 16)
                {
                    break;
                }
            }

            invalidStates.ShouldBeEmpty(string.Join(Environment.NewLine, invalidStates));
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Scroll_By_Height_Normalizes_First_Row_Offset_Without_Losing_Remainder()
    {
        var rows = CreateOrders(80);
        var grid = CreateOrderGrid(rows);

        var window = new Window
        {
            Width   = 1120,
            Height  = 760,
            Content = grid
        };

        try
        {
            window.Show();
            RunLayoutJobs();

            ScrollDownUntilSlotIsVisible(grid, 20);
            grid.DisplayData.FirstScrollingSlot.ShouldBeGreaterThan(0);

            var firstSlot   = grid.DisplayData.FirstScrollingSlot;
            var firstHeight = grid.GetDisplayedElementHeight(grid.DisplayData.GetDisplayedElement(firstSlot));
            firstHeight.ShouldBeGreaterThan(0);

            // 偏移与首行负偏移必须互相一致：VerticalOffset = 首行顶部 + NegVerticalOffset
            SetNegVerticalOffset(grid, firstHeight + 12);
            InvokeSetVerticalOffset(grid, grid.GetRangeOffset(firstSlot) + firstHeight + 12);

            grid.ScrollSlotsByHeight(-1);
            RunLayoutJobs();

            grid.DisplayData.FirstScrollingSlot.ShouldBe(grid.GetNextVisibleSlot(firstSlot));
            grid.NegVerticalOffset.ShouldBe(11, tolerance: 0.1);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Recycled_Row_Details_Keep_Explicit_Visibility_State()
    {
        var rows = CreateOrders(80);
        var grid = CreateOrderGrid(rows);

        var window = new Window
        {
            Width   = 1120,
            Height  = 760,
            Content = grid
        };

        try
        {
            window.Show();
            RunLayoutJobs();

            ToggleRowDetails(grid, 5);
            ScrollDownUntilSlotIsVisible(grid, 13);
            ToggleRowDetails(grid, 13);
            ScrollDownUntilSlotIsVisible(grid, 23);
            ToggleRowDetails(grid, 23);

            ScrollToBottom(grid);
            ScrollUpUntilSlotIsVisible(grid, 23);

            for (var i = 0; i < 80; i++)
            {
                grid.UpdateScroll(new Vector(0, 12));
                RunLayoutJobs();

                grid.GetRowDetailsVisibility(DataGridRowKey.FromInt64(6)).ShouldBeTrue();
                grid.GetRowDetailsVisibility(DataGridRowKey.FromInt64(14)).ShouldBeTrue();
                grid.GetRowDetailsVisibility(DataGridRowKey.FromInt64(24)).ShouldBeTrue();

                if (grid.DisplayData.FirstScrollingSlot <= 17)
                {
                    break;
                }
            }
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Expanded_Row_Details_Remain_In_Vertical_Extent_While_Scrolling()
    {
        var rows = CreateOrders(80);
        var grid = CreateOrderGrid(rows);

        var window = new Window
        {
            Width   = 1120,
            Height  = 760,
            Content = grid
        };

        try
        {
            window.Show();
            RunLayoutJobs();

            var baseMaximum = grid.VerticalScrollBar!.Maximum;

            ToggleRowDetails(grid, 5);
            ScrollDownUntilSlotIsVisible(grid, 13);
            ToggleRowDetails(grid, 13);
            ScrollDownUntilSlotIsVisible(grid, 23);
            ToggleRowDetails(grid, 23);

            var minimumExpectedMaximum = baseMaximum + (grid.RowDetailsHeightEstimate * 3) - 1;

            ScrollToBottom(grid);
            for (var i = 0; i < 160; i++)
            {
                grid.VerticalScrollBar!.Value = Math.Max(0, grid.VerticalScrollBar.Value - 12);
                RunLayoutJobs();

                grid.VerticalScrollBar!.Maximum.ShouldBeGreaterThanOrEqualTo(
                    minimumExpectedMaximum,
                    $"base={baseMaximum:0.##} details={grid.RowDetailsHeightEstimate:0.##} " +
                    $"offset={grid.VerticalOffset:0.##} neg={grid.NegVerticalOffset:0.##} " +
                    $"first={grid.DisplayData.FirstScrollingSlot} last={grid.DisplayData.LastScrollingSlot}");

                if (grid.DisplayData.FirstScrollingSlot <= 17)
                {
                    break;
                }
            }
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Vertical_Extent_Does_Not_Depend_On_Current_First_Row_Offset()
    {
        var rows = CreateOrders(80);
        var grid = CreateOrderGrid(rows);

        var window = new Window
        {
            Width   = 1120,
            Height  = 760,
            Content = grid
        };

        try
        {
            window.Show();
            RunLayoutJobs();

            ToggleRowDetails(grid, 5);
            ScrollDownUntilSlotIsVisible(grid, 13);
            ToggleRowDetails(grid, 13);
            ScrollDownUntilSlotIsVisible(grid, 23);
            ToggleRowDetails(grid, 23);
            RunLayoutJobs();

            var expectedMaximum = grid.VerticalScrollBar!.Maximum;

            SetNegVerticalOffset(grid, grid.NegVerticalOffset + 120);
            InvokeSetVerticalOffset(grid, grid.VerticalOffset + 12);
            grid.UpdateVerticalScrollBar();

            grid.VerticalScrollBar.Maximum.ShouldBe(expectedMaximum, tolerance: 0.1);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Rows_Measure_Normalizes_First_Row_Offset_Even_Without_Pending_Scroll()
    {
        var rows = CreateOrders(80);
        var grid = CreateOrderGrid(rows);

        var window = new Window
        {
            Width   = 1120,
            Height  = 760,
            Content = grid
        };

        try
        {
            window.Show();
            RunLayoutJobs();

            ScrollDownUntilSlotIsVisible(grid, 20);
            var firstSlot   = grid.DisplayData.FirstScrollingSlot;
            var firstHeight = grid.GetDisplayedElementHeight(grid.DisplayData.GetDisplayedElement(firstSlot));

            SetNegVerticalOffset(grid, firstHeight + 8);
            InvokeSetVerticalOffset(grid, grid.VerticalOffset + firstHeight + 8);

            grid.OnRowsMeasure();

            grid.DisplayData.FirstScrollingSlot.ShouldBe(grid.GetNextVisibleSlot(firstSlot));
            grid.NegVerticalOffset.ShouldBe(8, tolerance: 0.1);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Vertical_Scroll_Extent_Remains_Stable_When_Expanded_Row_Details_Are_Realized()
    {
        var rows = CreateOrders(80);
        var grid = CreateOrderGrid(rows);
        SetRowDetailsVisibility(grid, 5, true);
        SetRowDetailsVisibility(grid, 13, true);
        SetRowDetailsVisibility(grid, 23, true);

        var window = new Window
        {
            Width   = 1120,
            Height  = 760,
            Content = grid
        };

        try
        {
            window.Show();
            RunLayoutJobs();

            var minMaximum = grid.VerticalScrollBar!.Maximum;
            var maxMaximum = minMaximum;
            var maximumSamples = new List<string>
            {
                $"{minMaximum:0.##}/r{grid.RowHeightEstimate:0.##}/d{grid.RowDetailsHeightEstimate:0.##}/f{grid.DisplayData.FirstScrollingSlot}/l{grid.DisplayData.LastScrollingSlot}"
            };

            for (var i = 0; i < 80; i++)
            {
                grid.UpdateScroll(new Vector(0, -60));
                RunLayoutJobs();

                minMaximum = Math.Min(minMaximum, grid.VerticalScrollBar!.Maximum);
                maxMaximum = Math.Max(maxMaximum, grid.VerticalScrollBar!.Maximum);
                maximumSamples.Add($"{grid.VerticalScrollBar!.Maximum:0.##}/r{grid.RowHeightEstimate:0.##}/d{grid.RowDetailsHeightEstimate:0.##}/f{grid.DisplayData.FirstScrollingSlot}/l{grid.DisplayData.LastScrollingSlot}");

                if (grid.DisplayData.FirstScrollingSlot > 26)
                {
                    break;
                }
            }

            (maxMaximum - minMaximum).ShouldBeLessThan(
                300,
                $"row={grid.RowHeightEstimate:0.##} details={grid.RowDetailsHeightEstimate:0.##} " +
                string.Join(", ", maximumSamples));
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Cold_Row_Details_Measure_Does_Not_Reduce_Slot_Height_Estimate_Below_Global_Estimate()
    {
        var rows = CreateOrders(80);
        var grid = CreateOrderGrid(rows);
        SetRowDetailsVisibility(grid, 5, true);
        SetRowDetailsVisibility(grid, 13, true);
        SetRowDetailsVisibility(grid, 23, true);

        var window = new Window
        {
            Width   = 1120,
            Height  = 760,
            Content = grid
        };

        try
        {
            window.Show();
            RunLayoutJobs();

            ScrollDownUntilSlotIsVisible(grid, 5);
            RunLayoutJobs();
            ScrollDownUntilSlotIsVisible(grid, 13);
            RunLayoutJobs();
            ScrollDownUntilSlotIsVisible(grid, 23);
            RunLayoutJobs();

            grid.RowDetailsHeightEstimate.ShouldBeGreaterThan(100);

            grid.UpdateRowDetailsHeightEstimateFromMeasuredDetails(23, 33);

            ReadRowDetailsHeightEstimate(grid, 23).ShouldBe(grid.RowDetailsHeightEstimate);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Vertical_Scroll_Extent_Remains_Stable_When_Scrolling_Up_From_Bottom_Through_Expanded_Row_Details()
    {
        var rows = CreateOrders(80);
        var grid = CreateOrderGrid(rows);
        SetRowDetailsVisibility(grid, 5, true);
        SetRowDetailsVisibility(grid, 13, true);
        SetRowDetailsVisibility(grid, 23, true);

        var window = new Window
        {
            Width   = 1120,
            Height  = 760,
            Content = grid
        };

        try
        {
            window.Show();
            RunLayoutJobs();

            ScrollToBottom(grid);

            var minMaximum = grid.VerticalScrollBar!.Maximum;
            var maxMaximum = minMaximum;
            var previousOffset = grid.VerticalOffset;
            var samples = new List<string>
            {
                $"{grid.VerticalOffset:0.##}/{minMaximum:0.##}/f{grid.DisplayData.FirstScrollingSlot}/l{grid.DisplayData.LastScrollingSlot}"
            };

            for (var i = 0; i < 80; i++)
            {
                grid.UpdateScroll(new Vector(0, 60));
                RunLayoutJobs();

                minMaximum = Math.Min(minMaximum, grid.VerticalScrollBar!.Maximum);
                maxMaximum = Math.Max(maxMaximum, grid.VerticalScrollBar!.Maximum);
                samples.Add($"{grid.VerticalOffset:0.##}/{grid.VerticalScrollBar!.Maximum:0.##}/f{grid.DisplayData.FirstScrollingSlot}/l{grid.DisplayData.LastScrollingSlot}");

                grid.VerticalOffset.ShouldBeLessThanOrEqualTo(previousOffset, string.Join(", ", samples));
                previousOffset = grid.VerticalOffset;

                if (grid.DisplayData.FirstScrollingSlot <= 20)
                {
                    break;
                }
            }

            (maxMaximum - minMaximum).ShouldBeLessThan(
                300,
                $"row={grid.RowHeightEstimate:0.##} details={grid.RowDetailsHeightEstimate:0.##} " +
                string.Join(", ", samples));
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Vertical_Offset_Is_In_Sync_With_Displayed_Window_After_Scrolling_To_Bottom()
    {
        var rows = CreateOrders(80);
        var grid = CreateOrderGrid(rows);
        SetRowDetailsVisibility(grid, 5, true);
        SetRowDetailsVisibility(grid, 13, true);
        SetRowDetailsVisibility(grid, 23, true);

        var window = new Window
        {
            Width   = 1120,
            Height  = 760,
            Content = grid
        };

        try
        {
            window.Show();
            RunLayoutJobs();

            ScrollToBottom(grid);

            var displayedOffset = ReadDisplayedRowsVerticalOffsetEstimate(grid);
            Math.Abs(displayedOffset - grid.VerticalOffset).ShouldBeLessThanOrEqualTo(
                1.0,
                $"{grid.VerticalOffset:0.##}/{displayedOffset:0.##}/max{grid.VerticalScrollBar!.Maximum:0.##}/cell{grid.CellsEstimatedHeight:0.##}/row{grid.RowHeightEstimate:0.##}/details{grid.RowDetailsHeightEstimate:0.##}/n{grid.NegVerticalOffset:0.##}/f{grid.DisplayData.FirstScrollingSlot}/l{grid.DisplayData.LastScrollingSlot}/last{grid.LastVisibleSlot}/slots{grid.SlotCount}/visible{grid.VisibleSlotCount}/heights{GetDisplayedHeights(grid)}");
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Vertical_Offset_Stays_In_Sync_With_Displayed_Window_When_Scrolling_Through_Expanded_Row_Details()
    {
        var rows = CreateOrders(80);
        var grid = CreateOrderGrid(rows);
        SetRowDetailsVisibility(grid, 5, true);
        SetRowDetailsVisibility(grid, 13, true);
        SetRowDetailsVisibility(grid, 23, true);

        var window = new Window
        {
            Width   = 1120,
            Height  = 760,
            Content = grid
        };

        try
        {
            window.Show();
            RunLayoutJobs();

            ScrollToBottom(grid);
            grid.DisplayData.FirstScrollingSlot.ShouldBeGreaterThan(23);

            var samples = new List<string>();
            for (var i = 0; i < 80; i++)
            {
                grid.UpdateScroll(new Vector(0, 24));
                RunLayoutJobs();

                var displayedOffset = ReadDisplayedRowsVerticalOffsetEstimate(grid);
                var difference      = Math.Abs(displayedOffset - grid.VerticalOffset);
                samples.Add(
                    $"{grid.VerticalOffset:0.##}/{displayedOffset:0.##}/d{difference:0.##}/n{grid.NegVerticalOffset:0.##}/f{grid.DisplayData.FirstScrollingSlot}/l{grid.DisplayData.LastScrollingSlot}/last{grid.LastVisibleSlot}/slots{grid.SlotCount}/visible{grid.VisibleSlotCount}");

                difference.ShouldBeLessThanOrEqualTo(1.0, string.Join(", ", samples));

                if (grid.DisplayData.FirstScrollingSlot <= 17)
                {
                    break;
                }
            }
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Recycled_Expanded_Row_Height_Is_Not_Used_For_Collapsed_Row_During_Scroll()
    {
        var rows = CreateOrders(80);
        var grid = CreateOrderGrid(rows);
        SetRowDetailsVisibility(grid, 5, true);
        SetRowDetailsVisibility(grid, 11, true);
        SetRowDetailsVisibility(grid, 23, true);

        var window = new Window
        {
            Width   = 1120,
            Height  = 760,
            Content = grid
        };

        var invalidSamples = new List<string>();

        void CaptureDisplayedCollapsedRowHeights(string source)
        {
            if (grid.DisplayData.FirstScrollingSlot < 0 ||
                grid.RowDetailsHeightEstimate <= 0)
            {
                return;
            }

            for (var slot = grid.DisplayData.FirstScrollingSlot;
                 slot >= 0 && slot <= grid.DisplayData.LastScrollingSlot;
                 slot = grid.GetNextVisibleSlot(slot))
            {
                if (slot < 0 || !grid.IsSlotVisible(slot))
                {
                    continue;
                }

                if (grid.DisplayData.GetDisplayedElement(slot) is not DataGridRow row ||
                    grid.GetRowDetailsVisibility(row.Index))
                {
                    continue;
                }

                var staleHeightThreshold = grid.RowHeightEstimate + grid.RowDetailsHeightEstimate / 2;
                var displayedHeight = grid.GetDisplayedElementHeight(row);
                if (MathUtils.GreaterThan(displayedHeight, staleHeightThreshold))
                {
                    invalidSamples.Add(
                        $"{source}: row={row.Index} displayed={displayedHeight:0.##} desired={row.DesiredSize.Height:0.##} " +
                        $"target={ReadDataGridRowTargetHeight(row):0.##} " +
                        $"measureValid={row.IsMeasureValid} " +
                        $"visible={row.IsVisible} " +
                        $"parent={row.Parent?.GetType().Name ?? "null"} " +
                        $"visualParent={row.GetVisualParent()?.GetType().Name ?? "null"} " +
                        $"detailsVisible={row.IsDetailsVisible} " +
                        $"detailsFrame={ReadDetailsFrameState(row)} " +
                        $"headerDesired={ReadRowHeaderDesiredHeight(row):0.##} " +
                        $"detailsContentHeight={ReadDetailsPresenterContentHeight(row):0.##} " +
                        $"detailsChildren={ReadDetailsPresenterChildCount(row)} " +
                        $"cellsDesired={ReadCellsPresenterDesiredHeight(row):0.##} " +
                        $"rowEstimate={grid.RowHeightEstimate:0.##} details={grid.RowDetailsHeightEstimate:0.##} " +
                        $"offset={grid.VerticalOffset:0.##} neg={grid.NegVerticalOffset:0.##} " +
                        $"first={grid.DisplayData.FirstScrollingSlot} last={grid.DisplayData.LastScrollingSlot}");
                }
            }
        }

        try
        {
            window.Show();
            RunLayoutJobs();

            grid.VerticalScrollBar.ShouldNotBeNull();
            grid.VerticalScrollBar.PropertyChanged += (_, args) =>
            {
                if (args.Property == RangeBase.ValueProperty)
                {
                    CaptureDisplayedCollapsedRowHeights("value");
                }
            };

            for (var i = 0; i < 90; i++)
            {
                grid.UpdateScroll(new Vector(0, -120));
                RunLayoutJobs();

                if (grid.DisplayData.FirstScrollingSlot > 35)
                {
                    break;
                }
            }

            invalidSamples.ShouldBeEmpty(string.Join(Environment.NewLine, invalidSamples));
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Expanded_Row_Cells_Are_Arranged_To_Primary_Row_Height()
    {
        var rows = Enumerable.Range(1, 4)
                             .Select(index => new GridRow($"Row {index:00}"))
                             .ToArray();
        var grid = CreateDetailsTextGrid(rows, height: 360);

        var window = new Window
        {
            Width   = 640,
            Height  = 420,
            Content = grid
        };

        try
        {
            window.Show();
            RunLayoutJobs();

            ToggleRowDetails(grid, 0);
            RunLayoutJobs();

            var row            = GetDisplayedRow(grid, 0);
            var cellsPresenter = row.GetVisualDescendants()
                                    .OfType<DataGridCellsPresenter>()
                                    .Single();
            var detailsFrame = row.GetVisualDescendants()
                                  .OfType<Border>()
                                  .Single(border => border.Name == "DetailsPresenterFrame");
            var cellsDesiredHeight = ReadCellsPresenterDesiredHeight(row);

            cellsPresenter.Bounds.Height.ShouldBe(cellsDesiredHeight, tolerance: 1);
            detailsFrame.Bounds.Top.ShouldBe(cellsDesiredHeight, tolerance: 1);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Expanded_Row_Cells_Are_Not_Stretched_By_Row_Details_Height_Estimate()
    {
        var rows = Enumerable.Range(1, 4)
                             .Select(index => new GridRow($"Row {index:00}"))
                             .ToArray();
        var grid = CreateDetailsTextGrid(rows, height: 360);

        var window = new Window
        {
            Width   = 640,
            Height  = 420,
            Content = grid
        };

        try
        {
            window.Show();
            RunLayoutJobs();

            grid.UpdateRowDetailsHeightEstimateFromMeasuredDetails(180);
            ToggleRowDetails(grid, 0);
            RunLayoutJobs();

            var row            = GetDisplayedRow(grid, 0);
            var cellsPresenter = row.GetVisualDescendants()
                                    .OfType<DataGridCellsPresenter>()
                                    .Single();
            var detailsFrame = row.GetVisualDescendants()
                                  .OfType<Border>()
                                  .Single(border => border.Name == "DetailsPresenterFrame");
            var cellsDesiredHeight = ReadCellsPresenterDesiredHeight(row);

            cellsPresenter.Bounds.Height.ShouldBe(cellsDesiredHeight, tolerance: 1);
            detailsFrame.Bounds.Top.ShouldBe(cellsDesiredHeight, tolerance: 1);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Initial_Row_Height_Estimate_Excludes_Visible_Row_Details()
    {
        var rows = CreateOrders(20);
        var grid = CreateOrderGrid(rows);
        SetRowDetailsVisibility(grid, 0, true);

        var window = new Window
        {
            Width   = 1120,
            Height  = 760,
            Content = grid
        };

        try
        {
            window.Show();
            RunLayoutJobs();

            grid.RowDetailsHeightEstimate.ShouldBeGreaterThan(0);
            grid.RowHeightEstimate.ShouldBeLessThan(grid.RowDetailsHeightEstimate);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Recycled_Row_Details_Content_Is_Rebuilt_For_New_Data_Item()
    {
        var rows = Enumerable.Range(1, 8)
                             .Select(index => new GridRow($"Row {index:00}"))
                             .ToArray();
        var grid = CreateDetailsTextGrid(rows, height: 92);
        SetRowDetailsVisibility(grid, 0, true);
        SetRowDetailsVisibility(grid, 1, true);

        var window = new Window
        {
            Width   = 420,
            Height  = 160,
            Content = grid
        };

        try
        {
            window.Show();
            RunLayoutJobs();

            ScrollDownUntilSlotIsVisible(grid, 1);

            var row = GetDisplayedRow(grid, 1);
            row.GetVisualDescendants()
               .OfType<TextBlock>()
               .Select(textBlock => textBlock.Text)
               .ShouldContain("Details: Row 02");
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Recycled_Row_Details_Content_Is_Rebuilt_When_Returning_To_Earlier_Expanded_Row()
    {
        var rows = CreateOrders(80);
        var grid = CreateOrderGrid(rows);

        var window = new Window
        {
            Width   = 1120,
            Height  = 760,
            Content = grid
        };

        try
        {
            window.Show();
            RunLayoutJobs();

            ToggleRowDetails(grid, 5);
            ScrollDownUntilSlotIsVisible(grid, 13);
            ToggleRowDetails(grid, 13);
            ScrollDownUntilSlotIsVisible(grid, 23);
            ToggleRowDetails(grid, 23);

            ScrollToBottom(grid);
            ScrollToTop(grid);

            var row = GetDisplayedRow(grid, 5);
            var detailTexts = row.GetVisualDescendants()
                                 .OfType<TextBlock>()
                                 .Select(textBlock => textBlock.Text)
                                 .Where(text => text is not null)
                                 .ToArray();

            detailTexts.ShouldContain("Product 006-1");
            detailTexts.ShouldNotContain("Product 024-1");
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Thumb_Track_Maps_Absolute_Offset_Through_Expanded_Row_Details()
    {
        var rows = CreateOrders(80);
        var grid = CreateOrderGrid(rows);

        var window = new Window
        {
            Width   = 1120,
            Height  = 760,
            Content = grid
        };

        try
        {
            window.Show();
            RunLayoutJobs();

            ToggleRowDetails(grid, 5);
            ScrollDownUntilSlotIsVisible(grid, 11);
            ToggleRowDetails(grid, 11);
            ScrollDownUntilSlotIsVisible(grid, 23);
            ToggleRowDetails(grid, 23);
            ScrollToBottom(grid);

            grid.GetRowDetailsVisibility(DataGridRowKey.FromInt64(6)).ShouldBeTrue();
            grid.GetRowDetailsVisibility(DataGridRowKey.FromInt64(12)).ShouldBeTrue();
            grid.GetRowDetailsVisibility(DataGridRowKey.FromInt64(24)).ShouldBeTrue();
            grid.GetRangeRowDetailsHeightEstimate(DataGridRowKey.FromInt64(6)).ShouldBeGreaterThan(100);
            grid.GetRangeRowDetailsHeightEstimate(DataGridRowKey.FromInt64(12)).ShouldBeGreaterThan(100);
            grid.GetRangeRowDetailsHeightEstimate(DataGridRowKey.FromInt64(24)).ShouldBeGreaterThan(100);

            for (double value = grid.VerticalScrollBar!.Value; value > 450; value -= 8)
            {
                grid.VerticalScrollBar.Value = value;
                grid.ProcessVerticalScroll(ScrollEventType.ThumbTrack);
                RunLayoutJobs();
            }

            grid.VerticalScrollBar.Value = 450;
            grid.ProcessVerticalScroll(ScrollEventType.ThumbTrack);
            RunLayoutJobs();

            grid.DisplayData.FirstScrollingSlot.ShouldBeLessThanOrEqualTo(
                6,
                $"offset={grid.VerticalOffset:0.##} neg={grid.NegVerticalOffset:0.##} " +
                $"first={grid.DisplayData.FirstScrollingSlot} last={grid.DisplayData.LastScrollingSlot} " +
                $"row={grid.RowHeightEstimate:0.##} details={grid.RowDetailsHeightEstimate:0.##}");
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Thumb_Track_Small_Delta_Does_Not_Rebuild_Displayed_Rows()
    {
        var rows = CreateOrders(80);
        var grid = CreateOrderGrid(rows);

        var window = new Window
        {
            Width   = 1120,
            Height  = 760,
            Content = grid
        };

        try
        {
            window.Show();
            RunLayoutJobs();

            var loadingRows = 0;
            var unloadingRows = 0;
            grid.LoadingRow += (_, _) => loadingRows++;
            grid.UnloadingRow += (_, _) => unloadingRows++;

            grid.VerticalScrollBar!.Value = grid.VerticalOffset + 2;
            grid.ProcessVerticalScroll(ScrollEventType.ThumbTrack);
            RunLayoutJobs();

            loadingRows.ShouldBe(0);
            unloadingRows.ShouldBe(0);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static DataGridRow CreateDetachedRow(global::AtomUI.Desktop.Controls.DataGrid grid, int index)
    {
        return new DataGridRow
        {
            OwningGrid = grid,
            Index      = index,
            Slot       = index
        };
    }

    private static void SetRowDetailsVisibility(
        global::AtomUI.Desktop.Controls.DataGrid grid,
        int dataIndex,
        bool isVisible) =>
        grid.NotifyRowDetailsVisibilityPropertyChanged(
            DataGridRowKey.FromInt64(dataIndex + 1L),
            dataIndex,
            isVisible);

    private static global::AtomUI.Desktop.Controls.DataGrid CreateGrid(IReadOnlyList<GridRow> rows)
    {
        var grid = new global::AtomUI.Desktop.Controls.DataGrid
        {
            AutoGenerateColumns       = false,
            ItemsSource                     = new TestDataGridSource<GridRow>(rows),
            RowDetailsVisibilityMode   = DataGridRowDetailsVisibilityMode.Collapsed,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility   = ScrollBarVisibility.Disabled
        };

        grid.Columns.Add(new DataGridDetailExpanderColumn());
        grid.Columns.Add(new DataGridTextColumn
        {
            Header  = "Name",
            Binding = new Binding(nameof(GridRow.Name)),
            Width   = new DataGridLength(160)
        });
        grid.RowDetailsTemplate = new FuncDataTemplate<GridRow>((row, _) => new TextBlock
        {
            Text = row?.Name
        });

        return grid;
    }

    private static global::AtomUI.Desktop.Controls.DataGrid CreateDetailsTextGrid(IReadOnlyList<GridRow> rows,
                                                                                  double height = 220)
    {
        var grid = new global::AtomUI.Desktop.Controls.DataGrid
        {
            AutoGenerateColumns         = false,
            ItemsSource                       = new TestDataGridSource<GridRow>(rows),
            RowDetailsVisibilityMode     = DataGridRowDetailsVisibilityMode.Collapsed,
            IsMotionEnabled              = false,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Width                        = 360,
            Height                       = height
        };

        grid.Columns.Add(new DataGridDetailExpanderColumn());
        grid.Columns.Add(new DataGridTextColumn
        {
            Header  = "Name",
            Binding = new Binding(nameof(GridRow.Name)),
            Width   = new DataGridLength(160)
        });
        grid.RowDetailsTemplate = new FuncDataTemplate<GridRow>((row, _) => new TextBlock
        {
            Text   = $"Details: {row?.Name}",
            Height = 48
        });

        return grid;
    }

    private static global::AtomUI.Desktop.Controls.DataGrid CreateOrderGrid(IReadOnlyList<OrderViewModel> rows)
    {
        var grid = new global::AtomUI.Desktop.Controls.DataGrid
        {
            AutoGenerateColumns         = false,
            ItemsSource                       = new TestDataGridSource<OrderViewModel>(rows),
            RowDetailsVisibilityMode     = DataGridRowDetailsVisibilityMode.Collapsed,
            IsMotionEnabled              = false,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Width                        = 1040,
            Height                       = 620
        };

        grid.Columns.Add(new DataGridDetailExpanderColumn());
        grid.Columns.Add(new DataGridTextColumn
        {
            Header  = "OrderID",
            Binding = new Binding("Order.Id"),
            Width   = new DataGridLength(120)
        });
        grid.Columns.Add(new DataGridTextColumn
        {
            Header  = "Customer",
            Binding = new Binding("Order.CustomerName"),
            Width   = new DataGridLength(180)
        });
        grid.Columns.Add(new DataGridTextColumn
        {
            Header  = "Date",
            Binding = new Binding("Order.OrderDate")
            {
                StringFormat = "yyyy-MM-dd"
            },
            Width = new DataGridLength(160)
        });
        grid.Columns.Add(new DataGridTextColumn
        {
            Header  = "Items",
            Binding = new Binding("Order.Items.Count"),
            Width   = new DataGridLength(100)
        });
        grid.Columns.Add(new DataGridTextColumn
        {
            Header  = "Total",
            Binding = new Binding("Total")
            {
                StringFormat = "C"
            },
            Width = new DataGridLength(140)
        });

        grid.RowDetailsTemplate = new FuncDataTemplate<OrderViewModel>((order, _) =>
        {
            var detailsGrid = new global::AtomUI.Desktop.Controls.DataGrid
            {
                AutoGenerateColumns = false,
                IsReadOnly          = true,
                HeadersVisibility   = DataGridHeadersVisibility.None,
                ItemsSource              = order is null
                    ? null
                    : new TestDataGridSource<OrderItem>(order.Order.Items),
                Margin              = new Thickness(10, 0, 0, 0)
            };

            detailsGrid.Columns.Add(new DataGridTextColumn
            {
                Header  = "Product",
                Binding = new Binding(nameof(OrderItem.ProductName)),
                Width   = new DataGridLength(280)
            });
            detailsGrid.Columns.Add(new DataGridTextColumn
            {
                Header  = "Quantity",
                Binding = new Binding(nameof(OrderItem.Quantity)),
                Width   = new DataGridLength(120)
            });
            detailsGrid.Columns.Add(new DataGridTextColumn
            {
                Header  = "Unit Price",
                Binding = new Binding(nameof(OrderItem.UnitPrice))
                {
                    StringFormat = "C"
                },
                Width = new DataGridLength(160)
            });
            detailsGrid.Columns.Add(new DataGridTextColumn
            {
                Header  = "Total",
                Binding = new Binding(nameof(OrderItem.Total))
                {
                    StringFormat = "C"
                },
                Width = new DataGridLength(160)
            });

            return detailsGrid;
        });

        return grid;
    }

    private static void ScrollUpUntilSlotIsVisible(global::AtomUI.Desktop.Controls.DataGrid grid, int slot)
    {
        for (var i = 0; i < 80; i++)
        {
            if (grid.DisplayData.FirstScrollingSlot <= slot && grid.DisplayData.LastScrollingSlot >= slot)
            {
                return;
            }

            var previousOffset = grid.VerticalOffset;
            grid.UpdateScroll(new Vector(0, 60));
            RunLayoutJobs();
            grid.VerticalOffset.ShouldBeLessThan(previousOffset, GetRangeScrollState(grid));
        }

        throw new InvalidOperationException(
            $"Slot {slot} did not become visible. {GetRangeScrollState(grid)}");
    }

    private static string GetRangeScrollState(global::AtomUI.Desktop.Controls.DataGrid grid) =>
        $"offset={grid.VerticalOffset:0.##}, desired={ReadRangeDesiredOffset(grid):0.##}, " +
        $"extent={grid.VerticalScrollBar?.Maximum + grid.CellsEstimatedHeight:0.##}, " +
        $"first={grid.DisplayData.FirstScrollingSlot}, last={grid.DisplayData.LastScrollingSlot}, " +
        $"h5={grid.GetRangeHeight(5):0.##}, o6={grid.GetRangeOffset(6):0.##}, " +
        $"h23={grid.GetRangeHeight(23):0.##}, o23={grid.GetRangeOffset(23):0.##}, " +
        $"row={grid.RowHeightEstimate:0.##}, details={grid.RowDetailsHeightEstimate:0.##}, " +
        $"dir={ReadRangeScrollDirection(grid)}, target={ReadRangeScrollTarget(grid):0.##}, " +
        $"pending={grid.HasPendingRangeViewport}, load={grid.LoadState}";

    private static double ReadRangeDesiredOffset(global::AtomUI.Desktop.Controls.DataGrid grid)
    {
        var field = typeof(global::AtomUI.Desktop.Controls.DataGrid).GetField(
            "_rangeDesiredOffset",
            BindingFlags.Instance | BindingFlags.NonPublic);
        field.ShouldNotBeNull();
        return (double)field.GetValue(grid)!;
    }

    private static int ReadRangeScrollDirection(global::AtomUI.Desktop.Controls.DataGrid grid)
    {
        var field = typeof(global::AtomUI.Desktop.Controls.DataGrid).GetField(
            "_rangeMeasurementScrollDirection",
            BindingFlags.Instance | BindingFlags.NonPublic);
        field.ShouldNotBeNull();
        return (int)field.GetValue(grid)!;
    }

    private static double ReadRangeScrollTarget(global::AtomUI.Desktop.Controls.DataGrid grid)
    {
        var field = typeof(global::AtomUI.Desktop.Controls.DataGrid).GetField(
            "_rangeMeasurementScrollTarget",
            BindingFlags.Instance | BindingFlags.NonPublic);
        field.ShouldNotBeNull();
        return (double)field.GetValue(grid)!;
    }

    private static void ScrollToBottom(global::AtomUI.Desktop.Controls.DataGrid grid)
    {
        for (var i = 0; i < 120; i++)
        {
            var previousOffset = grid.VerticalOffset;
            grid.UpdateScroll(new Vector(0, -120));
            RunLayoutJobs();
            if (MathUtils.AreClose(grid.VerticalOffset, previousOffset))
            {
                break;
            }
        }
    }

    private static void ScrollToTop(global::AtomUI.Desktop.Controls.DataGrid grid)
    {
        for (var i = 0; i < 160; i++)
        {
            var previousOffset = grid.VerticalOffset;
            grid.UpdateScroll(new Vector(0, 120));
            RunLayoutJobs();
            if (MathUtils.AreClose(grid.VerticalOffset, previousOffset) &&
                MathUtils.AreClose(grid.VerticalOffset, 0))
            {
                break;
            }
        }
    }

    private static void ScrollDownUntilSlotIsVisible(global::AtomUI.Desktop.Controls.DataGrid grid, int slot)
    {
        for (var i = 0; i < 80; i++)
        {
            if (grid.DisplayData.FirstScrollingSlot <= slot && grid.DisplayData.LastScrollingSlot >= slot)
            {
                return;
            }

            grid.UpdateScroll(new Vector(0, -80));
            RunLayoutJobs();
        }

        throw new InvalidOperationException($"Slot {slot} did not become visible.");
    }

    private static DataGridRow GetDisplayedRow(global::AtomUI.Desktop.Controls.DataGrid grid, int rowIndex)
    {
        foreach (var row in grid.GetAllRows())
        {
            if (row.Index == rowIndex)
            {
                return row;
            }
        }

        throw new InvalidOperationException($"Row {rowIndex} is not displayed.");
    }

    private static double ReadRowDetailsHeightEstimate(global::AtomUI.Desktop.Controls.DataGrid grid, int slot)
    {
        var method = typeof(global::AtomUI.Desktop.Controls.DataGrid).GetMethod(
            "GetRowDetailsHeightEstimate",
            BindingFlags.Instance | BindingFlags.NonPublic);
        method.ShouldNotBeNull();
        return (double)method.Invoke(grid, [slot])!;
    }

    private static double ReadDisplayedRowsVerticalOffsetEstimate(global::AtomUI.Desktop.Controls.DataGrid grid)
    {
        var method = typeof(global::AtomUI.Desktop.Controls.DataGrid).GetMethod(
            "GetDisplayedRowsVerticalOffsetEstimate",
            BindingFlags.Instance | BindingFlags.NonPublic);
        method.ShouldNotBeNull();
        return (double)method.Invoke(grid, null)!;
    }

    private static double ReadDataGridRowTargetHeight(DataGridRow row)
    {
        var property = typeof(DataGridRow).GetProperty(
            "TargetHeight",
            BindingFlags.Instance | BindingFlags.NonPublic);
        property.ShouldNotBeNull();
        return (double)property.GetValue(row)!;
    }

    private static string GetDisplayedHeights(global::AtomUI.Desktop.Controls.DataGrid grid)
    {
        var values = new List<string>();
        for (var slot = grid.DisplayData.FirstScrollingSlot;
             slot >= 0 && slot <= grid.DisplayData.LastScrollingSlot;
             slot = grid.GetNextVisibleSlot(slot))
        {
            var element = grid.DisplayData.GetDisplayedElement(slot);
            values.Add($"{slot}:{grid.GetDisplayedElementHeight(element):0.##}");
        }

        return string.Join("|", values);
    }

    private static double ReadDetailsPresenterContentHeight(DataGridRow row)
    {
        var field = typeof(DataGridRow).GetField(
            "_detailsElement",
            BindingFlags.Instance | BindingFlags.NonPublic);
        field.ShouldNotBeNull();
        if (field.GetValue(row) is not DataGridDetailsPresenter presenter)
        {
            return 0;
        }

        return presenter.ContentHeight;
    }

    private static int ReadDetailsPresenterChildCount(DataGridRow row)
    {
        var field = typeof(DataGridRow).GetField(
            "_detailsElement",
            BindingFlags.Instance | BindingFlags.NonPublic);
        field.ShouldNotBeNull();
        if (field.GetValue(row) is not DataGridDetailsPresenter presenter)
        {
            return 0;
        }

        return presenter.Children.Count;
    }

    private static string ReadDetailsFrameState(DataGridRow row)
    {
        var detailsFrame = row.GetVisualDescendants()
                              .OfType<Border>()
                              .FirstOrDefault(border => border.Name == "DetailsPresenterFrame");
        if (detailsFrame is null)
        {
            return "missing";
        }

        return $"{detailsFrame.IsVisible}/{detailsFrame.DesiredSize.Height:0.##}";
    }

    private static double ReadRowHeaderDesiredHeight(DataGridRow row)
    {
        var field = typeof(DataGridRow).GetField(
            "_headerElement",
            BindingFlags.Instance | BindingFlags.NonPublic);
        field.ShouldNotBeNull();
        return field.GetValue(row) is Control header ? header.DesiredSize.Height : 0;
    }

    private static double ReadCellsPresenterDesiredHeight(DataGridRow row)
    {
        var field = typeof(DataGridRow).GetField(
            "_cellsElement",
            BindingFlags.Instance | BindingFlags.NonPublic);
        field.ShouldNotBeNull();
        var cellsPresenter = field.GetValue(row);
        if (cellsPresenter is null)
        {
            return 0;
        }

        var desiredHeightField = cellsPresenter.GetType().GetField(
            "_desiredHeight",
            BindingFlags.Instance | BindingFlags.NonPublic);
        desiredHeightField.ShouldNotBeNull();
        return (double)desiredHeightField.GetValue(cellsPresenter)!;
    }

    private static void SetNegVerticalOffset(global::AtomUI.Desktop.Controls.DataGrid grid, double offset)
    {
        var field = typeof(global::AtomUI.Desktop.Controls.DataGrid).GetField(
            "<NegVerticalOffset>k__BackingField",
            BindingFlags.Instance | BindingFlags.NonPublic);
        field.ShouldNotBeNull();
        field.SetValue(grid, offset);
    }

    private static void InvokeSetVerticalOffset(global::AtomUI.Desktop.Controls.DataGrid grid, double offset)
    {
        var method = typeof(global::AtomUI.Desktop.Controls.DataGrid).GetMethod(
            "SetVerticalOffset",
            BindingFlags.Instance | BindingFlags.NonPublic);
        method.ShouldNotBeNull();
        method.Invoke(grid, [offset]);
    }

    private static void ToggleRowDetails(global::AtomUI.Desktop.Controls.DataGrid grid, int rowIndex)
    {
        var row = GetDisplayedRow(grid, rowIndex);
        var expander = row.GetVisualDescendants()
                          .OfType<DataGridRowExpander>()
                          .Single();
        var toggleMethod = typeof(DataGridRowExpander).GetMethod(
            "Toggle",
            BindingFlags.Instance | BindingFlags.NonPublic);
        toggleMethod.ShouldNotBeNull();
        toggleMethod.Invoke(expander, null);
        RunLayoutJobs();
        grid.GetRowDetailsVisibility(rowIndex).ShouldBeTrue();
    }

    private static void RunLayoutJobs()
    {
        Dispatcher.UIThread.RunJobs();
        Dispatcher.UIThread.RunJobs(DispatcherPriority.SystemIdle);
    }

    private static void DrainLayout(ICollection<double> offsetSamples, global::AtomUI.Desktop.Controls.DataGrid grid)
    {
        for (var i = 0; i < 6; i++)
        {
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs(DispatcherPriority.Background);
            Dispatcher.UIThread.RunJobs(DispatcherPriority.SystemIdle);
            offsetSamples.Add(grid.VerticalOffset);
        }
    }

    private static IReadOnlyList<OrderViewModel> CreateOrders(int count)
    {
        var orders = new List<OrderViewModel>();
        for (var i = 1; i <= count; i++)
        {
            var items = new List<OrderItem>();
            for (var j = 1; j <= 4; j++)
            {
                items.Add(new OrderItem(
                    ProductName: $"Product {i:000}-{j}",
                    Quantity: j + i % 3,
                    UnitPrice: 12.5m + i + j));
            }

            orders.Add(new OrderViewModel(new Order(
                Id: $"ORD-{i:0000}",
                CustomerName: $"Customer {i:00}",
                OrderDate: new DateTime(2026, 7, 1).AddDays(i),
                Items: items)));
        }

        return orders;
    }

    private sealed class TestDataGridRowExpander : DataGridRowExpander
    {
        public void InvokeToggle()
        {
            Toggle();
        }
    }

    private sealed record GridRow(string Name);

    private sealed record OrderViewModel(Order Order)
    {
        public decimal Total => Order.Items.Sum(item => item.Total);
    }

    private sealed record Order(string Id, string CustomerName, DateTime OrderDate, IReadOnlyList<OrderItem> Items);

    private sealed record OrderItem(string ProductName, int Quantity, decimal UnitPrice)
    {
        public decimal Total => Quantity * UnitPrice;
    }
}
