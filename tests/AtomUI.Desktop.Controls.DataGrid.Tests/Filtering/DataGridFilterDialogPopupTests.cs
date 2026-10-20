using AtomUI.Controls.Data;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.DataGrid.Filtering;

public class DataGridFilterDialogPopupTests
{
    private static readonly DataGridFieldId NameField = new("name");
    private static readonly DataGridOperatorId EqualsOperator = new("equals");

    static DataGridFilterDialogPopupTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void DataGrid_Filter_In_Dialog_Uses_The_Owning_Window_Popup_Layer_And_Closes()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var column = new DataGridTextColumn
            {
                Header = "Name",
                FieldId = NameField,
                Binding = new Binding(nameof(DialogFilterRow.Name)),
                Filters = new[]
                {
                    new DataGridFilterItem { Text = "Alpha", Value = "Alpha" },
                    new DataGridFilterItem { Text = "Beta", Value = "Beta" }
                }
            };
            var grid = new global::AtomUI.Desktop.Controls.DataGrid
            {
                AutoGenerateColumns = false,
                CanUserFilterColumns = true,
                IsMotionEnabled = false,
                Width = 360,
                Height = 220,
                ItemsSource = CreateSource(new[]
                {
                    new DialogFilterRow("Alpha"),
                    new DialogFilterRow("Beta")
                })
            };
            grid.Columns.Add(column);
            var dialog = new Dialog
            {
                Content = grid,
                DialogHostType = DialogHostType.Overlay,
                IsModal = true,
                IsMotionEnabled = false,
                HostWidth = 420,
                HostHeight = 300
            };
            var window = new AtomUI.Desktop.Controls.Window
            {
                Width = 640,
                Height = 480,
                Content = new Avalonia.Controls.Grid { Children = { dialog } }
            };

            try
            {
                window.Show();
                Pump();
                dialog.IsOpen = true;
                PumpAndRender();

                TopLevel.GetTopLevel(grid).ShouldBeSameAs(window);
                var indicator = grid.GetVisualDescendants()
                                    .OfType<DataGridFilterIndicator>()
                                    .First(control => control.IsVisible && control.Flyout is not null);

                Click(window, indicator);
                var popupHost = FindPopupHost(window);
                TopLevel.GetTopLevel(popupHost).ShouldBeSameAs(window);
                AssertPopupLayer(grid, popupHost);
                indicator.Flyout.ShouldNotBeNull().IsOpen.ShouldBeTrue();

                Click(window, popupHost.GetVisualDescendants()
                                       .OfType<DataGridFilterMenuItem>()
                                       .First(item => item.Header?.ToString() == "Beta"));
                LightDismiss(window);

                indicator.Flyout.ShouldNotBeNull().IsOpen.ShouldBeFalse();
                AssertNoPopupHosts(window);
                dialog.IsOpen.ShouldBeTrue();
            }
            finally
            {
                dialog.IsOpen = false;
                Pump();
                window.Close();
            }
        });
    }

    [Fact]
    public void DataGrid_Pinned_Filter_Uses_First_Eligible_Column_And_Reopens_After_Detach()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var firstColumn = CreateFilterColumn("First");
            var secondColumn = CreateFilterColumn("Second");
            var grid = new global::AtomUI.Desktop.Controls.DataGrid
            {
                AutoGenerateColumns  = false,
                CanUserFilterColumns = true,
                IsMotionEnabled      = false,
                IsPopupPinnedOpen    = true,
                Width                = 360,
                Height               = 220,
                ItemsSource = CreateSource(new[]
                {
                    new DialogFilterRow("Alpha"),
                    new DialogFilterRow("Beta")
                })
            };
            grid.Columns.Add(firstColumn);
            grid.Columns.Add(secondColumn);
            var window = new AtomUI.Desktop.Controls.Window
            {
                Width   = 640,
                Height  = 480,
                Content = grid
            };

            try
            {
                window.Show();
                PumpAndRender();

                var firstIndicator  = FindFilterIndicator(grid, firstColumn);
                var secondIndicator = FindFilterIndicator(grid, secondColumn);
                var firstFlyout     = firstIndicator.Flyout.ShouldBeOfType<DataGridMenuFilterFlyout>();
                firstFlyout.IsOpen.ShouldBeTrue();
                firstFlyout.Popup.IsOpen.ShouldBeTrue();
                secondIndicator.Flyout.ShouldNotBeNull().IsOpen.ShouldBeFalse();

                firstFlyout.Hide();
                PumpAndRender();
                firstFlyout.IsOpen.ShouldBeTrue();
                firstFlyout.Popup.IsOpen.ShouldBeTrue();

                window.Content = null;
                PumpAndRender();
                firstFlyout.IsOpen.ShouldBeFalse();
                firstFlyout.Popup.IsOpen.ShouldBeFalse();

                window.Content = grid;
                PumpAndRender();
                var reopenedFlyout = FindFilterIndicator(grid, firstColumn)
                    .Flyout.ShouldBeOfType<DataGridMenuFilterFlyout>();
                reopenedFlyout.IsOpen.ShouldBeTrue();
                reopenedFlyout.Popup.IsOpen.ShouldBeTrue();

                grid.IsPopupPinnedOpen = false;
                PumpAndRender();
                reopenedFlyout.IsOpen.ShouldBeTrue();
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void DataGrid_Pinned_Filter_Replaces_Target_When_First_Column_Becomes_Ineligible()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var firstColumn = CreateFilterColumn("First");
            var secondColumn = CreateFilterColumn("Second");
            var grid = new global::AtomUI.Desktop.Controls.DataGrid
            {
                AutoGenerateColumns  = false,
                CanUserFilterColumns = true,
                IsMotionEnabled      = false,
                IsPopupPinnedOpen    = true,
                Width                = 360,
                Height               = 220,
                ItemsSource = CreateSource(new[]
                {
                    new DialogFilterRow("Alpha"),
                    new DialogFilterRow("Beta")
                })
            };
            grid.Columns.Add(firstColumn);
            grid.Columns.Add(secondColumn);
            var window = new AtomUI.Desktop.Controls.Window
            {
                Width   = 640,
                Height  = 480,
                Content = grid
            };

            try
            {
                window.Show();
                PumpAndRender();

                var firstIndicator  = FindFilterIndicator(grid, firstColumn);
                var secondIndicator = FindFilterIndicator(grid, secondColumn);
                var firstFlyout     = firstIndicator.Flyout.ShouldBeOfType<DataGridMenuFilterFlyout>();
                var secondFlyout    = secondIndicator.Flyout.ShouldBeOfType<DataGridMenuFilterFlyout>();
                firstFlyout.IsOpen.ShouldBeTrue();
                secondFlyout.IsOpen.ShouldBeFalse();

                firstColumn.IsVisible = false;
                PumpAndRender();
                firstFlyout.IsOpen.ShouldBeFalse();
                firstFlyout.Popup.IsOpen.ShouldBeFalse();
                secondFlyout.IsOpen.ShouldBeTrue();
                secondFlyout.Popup.IsOpen.ShouldBeTrue();

                firstColumn.IsVisible = true;
                PumpAndRender();
                secondFlyout.IsOpen.ShouldBeFalse();
                secondFlyout.Popup.IsOpen.ShouldBeFalse();
                firstFlyout.IsOpen.ShouldBeTrue();
                firstFlyout.Popup.IsOpen.ShouldBeTrue();
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void DataGrid_Pinned_Filter_Replaces_Flyout_When_Presenter_Mode_Changes()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var column = CreateFilterColumn("Name");
            var grid = new global::AtomUI.Desktop.Controls.DataGrid
            {
                AutoGenerateColumns  = false,
                CanUserFilterColumns = true,
                IsMotionEnabled      = false,
                IsPopupPinnedOpen    = true,
                Width                = 360,
                Height               = 220,
                ItemsSource = CreateSource(new[]
                {
                    new DialogFilterRow("Alpha"),
                    new DialogFilterRow("Beta")
                })
            };
            grid.Columns.Add(column);
            var window = new AtomUI.Desktop.Controls.Window
            {
                Width   = 640,
                Height  = 480,
                Content = grid
            };

            try
            {
                window.Show();
                PumpAndRender();

                var indicator = FindFilterIndicator(grid, column);
                var menuFlyout = indicator.Flyout.ShouldBeOfType<DataGridMenuFilterFlyout>();
                menuFlyout.IsOpen.ShouldBeTrue();

                column.FilterPresenterMode = DataGridFilterPresenterMode.Tree;
                PumpAndRender();

                menuFlyout.IsOpen.ShouldBeFalse();
                menuFlyout.Popup.IsOpen.ShouldBeFalse();
                var treeFlyout = indicator.Flyout.ShouldBeOfType<DataGridTreeFilterFlyout>();
                treeFlyout.IsOpen.ShouldBeTrue();
                treeFlyout.Popup.IsOpen.ShouldBeTrue();
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static DataGridTextColumn CreateFilterColumn(string header)
    {
        return new DataGridTextColumn
        {
            Header           = header,
            FieldId          = NameField,
            Binding          = new Binding(nameof(DialogFilterRow.Name)),
            Filters = new[]
            {
                new DataGridFilterItem { Text = "Alpha", Value = "Alpha" },
                new DataGridFilterItem { Text = "Beta", Value = "Beta" }
            }
        };
    }

    private static TestDataGridSource<DialogFilterRow> CreateSource(
        IReadOnlyList<DialogFilterRow> rows) =>
        new(
            rows,
            new DataGridSourceSchema(
                typeof(DialogFilterRow),
                [new DataGridFieldSchema(
                    NameField,
                    typeof(string),
                    DataGridSortDirections.All,
                    [new DataGridFilterOperatorSchema(
                        EqualsOperator,
                        1,
                        16,
                        DataGridScalarKinds.String)],
                    canGroup: false)],
                preferredRangeSize: 32,
                maximumRangeSize: 32));

    private static DataGridFilterIndicator FindFilterIndicator(
        global::AtomUI.Desktop.Controls.DataGrid grid,
        DataGridColumn column)
    {
        return grid.GetVisualDescendants()
                   .OfType<DataGridFilterIndicator>()
                   .Single(indicator => ReferenceEquals(indicator.OwningColumn, column));
    }

    private static OverlayPopupHost FindPopupHost(AtomUI.Desktop.Controls.Window window)
    {
        for (var attempt = 0; attempt < 20; attempt++)
        {
            PumpAndRender();
            if (window.GetVisualDescendants().OfType<OverlayPopupHost>().LastOrDefault() is { } host)
            {
                return host;
            }
        }

        OverlayPopupHost? missingHost = null;
        return missingHost.ShouldNotBeNull();
    }

    private static void AssertPopupLayer(Control dialogContent, OverlayPopupHost popupHost)
    {
        var overlayLayer = OverlayLayer.GetOverlayLayer(dialogContent).ShouldNotBeNull();
        var manager = overlayLayer.GetVisualAncestors().OfType<VisualLayerManager>().Single();
        popupHost.GetVisualAncestors().OfType<VisualLayerManager>().Single().ShouldBeSameAs(manager);
        AssertRendersAbove(popupHost, overlayLayer);
    }

    private static void AssertRendersAbove(Visual upper, Visual lower)
    {
        var upperPath = upper.GetSelfAndVisualAncestors().Reverse().ToList();
        var lowerPath = lower.GetSelfAndVisualAncestors().Reverse().ToList();
        var sharedCount = 0;
        while (sharedCount < upperPath.Count &&
               sharedCount < lowerPath.Count &&
               ReferenceEquals(upperPath[sharedCount], lowerPath[sharedCount]))
        {
            sharedCount++;
        }

        sharedCount.ShouldBeGreaterThan(0);
        sharedCount.ShouldBeLessThan(upperPath.Count);
        sharedCount.ShouldBeLessThan(lowerPath.Count);
        var commonParent = upperPath[sharedCount - 1];
        var upperBranch = upperPath[sharedCount];
        var lowerBranch = lowerPath[sharedCount];
        if (upperBranch.ZIndex != lowerBranch.ZIndex)
        {
            upperBranch.ZIndex.ShouldBeGreaterThan(lowerBranch.ZIndex);
            return;
        }

        commonParent.GetVisualChildren().ToList().IndexOf(upperBranch).ShouldBeGreaterThan(
            commonParent.GetVisualChildren().ToList().IndexOf(lowerBranch));
    }

    private static void AssertNoPopupHosts(AtomUI.Desktop.Controls.Window window)
    {
        for (var attempt = 0; attempt < 20; attempt++)
        {
            PumpAndRender();
            if (!window.GetVisualDescendants().OfType<OverlayPopupHost>().Any())
            {
                return;
            }
        }

        window.GetVisualDescendants().OfType<OverlayPopupHost>().ShouldBeEmpty();
    }

    private static void Click(AtomUI.Desktop.Controls.Window window, Control control)
    {
        PumpAndRender();
        control.IsAttachedToVisualTree().ShouldBeTrue();
        control.Bounds.Width.ShouldBeGreaterThan(0);
        control.Bounds.Height.ShouldBeGreaterThan(0);
        var point = control.TranslatePoint(
            new Point(control.Bounds.Width / 2, control.Bounds.Height / 2),
            window).ShouldNotBeNull();

        RaisePointerClick(window, control, point);
    }

    private static void LightDismiss(AtomUI.Desktop.Controls.Window window)
    {
        PumpAndRender();
        var dismissLayer = window.GetVisualDescendants()
                                 .Single(visual => visual.GetType().Name == "LightDismissOverlayLayer")
                                 .ShouldBeAssignableTo<InputElement>();
        dismissLayer.IsHitTestVisible.ShouldBeTrue();
        RaisePointerClick(window, dismissLayer, new Point(8, 8));
    }

    private static void RaisePointerClick(
        AtomUI.Desktop.Controls.Window window,
        InputElement target,
        Point point)
    {
        var pointer = new Pointer(Pointer.GetNextFreeId(), PointerType.Mouse, true);
        target.RaiseEvent(new PointerPressedEventArgs(
            target,
            pointer,
            window,
            point,
            0,
            new PointerPointProperties(RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed),
            KeyModifiers.None));
        var releaseTarget = pointer.Captured as InputElement ?? target;
        releaseTarget.RaiseEvent(new PointerReleasedEventArgs(
            releaseTarget,
            pointer,
            window,
            point,
            1,
            new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.LeftButtonReleased),
            KeyModifiers.None,
            MouseButton.Left));
        PumpAndRender();
    }

    private static void PumpAndRender()
    {
        Pump();
        AvaloniaHeadlessPlatform.ForceRenderTimerTick(1);
        Pump();
    }

    private static void Pump()
    {
        Dispatcher.UIThread.RunJobs();
    }
}

[GenerateDataMemberAccessors]
internal sealed partial record DialogFilterRow(string Name);
