using AtomUI.Desktop.Controls.Internal.Calendar;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System.Reflection;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;
using LunarCalendarControl = AtomUI.Desktop.Controls.LunarCalendar;
using CalendarViewControl = AtomUI.Desktop.Controls.Internal.DateViewer.DatePanel;

namespace AtomUI.Desktop.Controls.Tests.Calendar;

public class LunarCalendarRenderTests
{
    static LunarCalendarRenderTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Theory]
    [InlineData(true, CalendarMode.Month, 42)]
    [InlineData(false, CalendarMode.Month, 42)]
    [InlineData(true, CalendarMode.Year, 12)]
    [InlineData(false, CalendarMode.Year, 12)]
    public void AllLayoutModes_UseDedicatedLunarCells(bool fullscreen, CalendarMode mode, int expectedCount)
    {
        var calendar = new LunarCalendarControl
        {
            Value = new DateTime(2024, 2, 10),
            Fullscreen = fullscreen,
            Mode = mode
        };
        var window = Show(calendar, fullscreen ? 900 : 420, fullscreen ? 720 : 420);
        try
        {
            var cells = calendar.GetVisualDescendants().OfType<AtomUI.Desktop.Controls.Internal.DateViewer.DateViewerCell>().ToList();
            cells.Count.ShouldBe(expectedCount);
            foreach (var cell in cells)
            {
                (cell.Context as LunarCalendarCellContext).ShouldNotBeNull();
            }
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void DefaultDateCell_RendersGregorianValueAndLunarSecondaryText()
    {
        var calendar = new LunarCalendarControl
        {
            Value = new DateTime(2024, 2, 10),
            Fullscreen = false
        };
        var window = Show(calendar);
        try
        {
            var cell = calendar.GetVisualDescendants()
                .OfType<AtomUI.Desktop.Controls.Internal.DateViewer.DateViewerCell>()
                .Single(item => item.Model?.Value == calendar.Value);
            var value = cell.GetVisualDescendants()
                .OfType<AtomUI.Desktop.Controls.TextBlock>()
                .Single(text => text.Name == "PART_Value" && text.IsVisible);
            var secondary = cell.GetVisualDescendants()
                .OfType<AtomUI.Desktop.Controls.TextBlock>()
                .Single(text => text.Name == "PART_SecondaryText");

            value.Text.ShouldBe("10");
            secondary.Text.ShouldBe(((LunarCalendarCellContext)cell.Context!).SecondaryText);
            secondary.Text.ShouldNotBeEmpty();
            secondary.IsVisible.ShouldBeTrue();
            var lightText = BrushColor(GetThemeResource<IBrush>(SharedTokenKind.ColorTextLightSolid));
            BrushColor(value.Foreground).ShouldBe(lightText);
            BrushColor(secondary.Foreground).ShouldBe(lightText);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void MiniMonth_PreservesVerticalSpacingBetweenDoubleLineDateCells()
    {
        var calendar = new LunarCalendarControl
        {
            Value = new DateTime(2021, 4, 2),
            Fullscreen = false,
            Width = 450
        };
        var window = Show(calendar, 600, 600);
        try
        {
            var cells = calendar.GetVisualDescendants().OfType<AtomUI.Desktop.Controls.Internal.DateViewer.DateViewerCell>().ToList();
            var selectedInner = GetCellInner(cells.Single(cell => cell.Model?.Value == new DateTime(2021, 4, 2)));
            var nextWeekInner = GetCellInner(cells.Single(cell => cell.Model?.Value == new DateTime(2021, 4, 9)));
            var selectedTop = selectedInner.TranslatePoint(default, calendar).ShouldNotBeNull().Y;
            var nextWeekTop = nextWeekInner.TranslatePoint(default, calendar).ShouldNotBeNull().Y;
            var expectedGap = GetThemeResource<double>(SharedTokenKind.UniformlyMarginXS);

            (nextWeekTop - selectedTop - selectedInner.Bounds.Height)
                .ShouldBeGreaterThanOrEqualTo(expectedGap - 0.5);

            var view = calendar.GetVisualDescendants().OfType<CalendarViewControl>().Single();
            calendar.GetVisualDescendants().OfType<AtomUI.Desktop.Controls.DateViewer>().Single().Bounds.Height.ShouldBe(GetThemeResource<double>(LunarCalendarTokenKind.MiniContentHeight), 0.5);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void FullscreenDateCell_RightAlignsLunarSecondaryText()
    {
        var calendar = new LunarCalendarControl
        {
            Value = new DateTime(2024, 2, 10),
            Fullscreen = true
        };
        var window = Show(calendar, 900, 720);
        try
        {
            var cell = calendar.GetVisualDescendants()
                .OfType<AtomUI.Desktop.Controls.Internal.DateViewer.DateViewerCell>()
                .Single(item => item.Model?.Value == calendar.Value);
            var presenter = cell.GetVisualDescendants()
                .OfType<Avalonia.Controls.Grid>()
                .Single(control => control.Name == "PART_SecondaryPresenter");
            var secondary = cell.GetVisualDescendants()
                .OfType<AtomUI.Desktop.Controls.TextBlock>()
                .Single(text => text.Name == "PART_SecondaryText");
            var value = cell.GetVisualDescendants()
                .OfType<AtomUI.Desktop.Controls.TextBlock>()
                .Single(text => text.Name == "PART_Value" && text.IsVisible);

            presenter.HorizontalAlignment.ShouldBe(Avalonia.Layout.HorizontalAlignment.Right);
            secondary.TextAlignment.ShouldBe(Avalonia.Media.TextAlignment.Right);
            var primary = BrushColor(GetThemeResource<IBrush>(SharedTokenKind.ColorPrimary));
            var lightText = BrushColor(GetThemeResource<IBrush>(SharedTokenKind.ColorTextLightSolid));
            var selectedInner = GetCellInner(cell);
            BrushColor(selectedInner.Background).ShouldBe(primary);
            selectedInner.CornerRadius.TopLeft.ShouldBeGreaterThan(0);
            BrushColor(value.Foreground).ShouldBe(lightText);
            BrushColor(secondary.Foreground).ShouldBe(lightText);

            var pressedCell = calendar.GetVisualDescendants()
                .OfType<AtomUI.Desktop.Controls.Internal.DateViewer.DateViewerCell>()
                .Single(item => item.Model?.Value == new DateTime(2024, 2, 14));
            var normalCell = calendar.GetVisualDescendants()
                .OfType<AtomUI.Desktop.Controls.Internal.DateViewer.DateViewerCell>()
                .Single(item => item.Model?.Value == new DateTime(2024, 2, 15));
            var center = pressedCell.TranslatePoint(
                new Point(pressedCell.Bounds.Width / 2, pressedCell.Bounds.Height / 2), window)!.Value;
            window.MouseMove(center);
            window.MouseDown(center, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            calendar.Value.ShouldBe(new DateTime(2024, 2, 10));
            BrushColor(GetCellInner(pressedCell).GetBaseValue(Border.BorderBrushProperty).Value)
                .ShouldBe(BrushColor(GetCellInner(normalCell).GetBaseValue(Border.BorderBrushProperty).Value));
            window.MouseUp(center, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            calendar.Value.ShouldBe(new DateTime(2024, 2, 14));
            var newSelected = calendar.GetVisualDescendants()
                .OfType<AtomUI.Desktop.Controls.Internal.DateViewer.DateViewerCell>()
                .Single(item => item.Model?.Value == calendar.Value);
            BrushColor(GetCellInner(newSelected).GetBaseValue(Border.BackgroundProperty).Value)
                .ShouldBe(primary);
            var selectedContent = newSelected.GetVisualDescendants().OfType<LunarCalendarCellContent>().Single();
            BrushColor(selectedContent.GetVisualDescendants().OfType<AtomUI.Desktop.Controls.TextBlock>()
                    .Single(text => text.Name == "PART_SecondaryText").Foreground)
                .ShouldBe(lightText);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void CellTemplateReplacesSecondaryContent_AndFullCellTemplateTakesPriority()
    {
        var calendar = new LunarCalendarControl
        {
            Value = new DateTime(2024, 2, 10),
            Fullscreen = false,
            CellTemplate = MarkerTemplate("cell"),
            FullCellTemplate = MarkerTemplate("full")
        };
        var window = Show(calendar);
        try
        {
            var cell = calendar.GetVisualDescendants()
                .OfType<AtomUI.Desktop.Controls.Internal.DateViewer.DateViewerCell>()
                .Single(item => item.Model?.Value == calendar.Value);

            cell.GetVisualDescendants().OfType<Control>().Any(control => Equals(control.Tag, "full")).ShouldBeTrue();
            cell.GetVisualDescendants().OfType<Control>().Any(control => Equals(control.Tag, "cell")).ShouldBeFalse();
            cell.Context.ShouldBeOfType<LunarCalendarCellContext>();
            cell.GetVisualDescendants().OfType<Control>()
                .Where(control => control.Name is "PART_Value" or "PART_SecondaryText")
                .ShouldAllBe(control => !control.IsVisible);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void CellAutomationName_AppendsLunarPresentation()
    {
        var calendar = new LunarCalendarControl { Value = new DateTime(2024, 2, 10) };
        var window = Show(calendar);
        try
        {
            var cell = calendar.GetVisualDescendants()
                .OfType<AtomUI.Desktop.Controls.Internal.DateViewer.DateViewerCell>()
                .Single(item => item.Model?.Value == calendar.Value);
            var peer = ControlAutomationPeer.CreatePeerForElement(cell)
                .ShouldBeOfType<AtomUI.Desktop.Controls.Internal.DateViewer.DateViewerCellAutomationPeer>();

            peer.GetName().ShouldContain("2024");
            peer.GetName().ShouldContain(((LunarCalendarCellContext)cell.Context!).SecondaryText);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void FullscreenMonth_UsesLunarRangeBarOffsetAfterSecondaryLine()
    {
        var calendar = new LunarCalendarControl
        {
            Value = new DateTime(2024, 2, 10),
            Fullscreen = true,
            RangeBars =
            {
                new CalendarRangeBar
                {
                    StartDate = new DateTime(2024, 2, 10),
                    EndDate = new DateTime(2024, 2, 12),
                    Label = "Range"
                }
            }
        };
        var window = Show(calendar, 900, 720);
        try
        {
            var panel = calendar.GetVisualDescendants().OfType<CalendarRangeBarPanel>().Single();
            var cell = calendar.GetVisualDescendants()
                .OfType<AtomUI.Desktop.Controls.Internal.DateViewer.DateViewerCell>()
                .Single(item => item.Model?.Value == calendar.Value);
            var secondary = cell.GetVisualDescendants()
                .OfType<AtomUI.Desktop.Controls.TextBlock>()
                .Single(text => text.Name == "PART_SecondaryText");

            panel.RangeBarTopOffset.ShouldBeGreaterThan(secondary.Bounds.Bottom);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void SwitchingMode_ClearsInactiveLunarContextsInTheBoundedPool()
    {
        var calendar = new LunarCalendarControl
        {
            Value = new DateTime(2024, 2, 10),
            Mode = CalendarMode.Month
        };
        var window = Show(calendar);
        try
        {
            var view = calendar.GetVisualDescendants().OfType<CalendarViewControl>().Single();
            calendar.Mode = CalendarMode.Year;
            Dispatcher.UIThread.RunJobs();

            var pool = (IReadOnlyList<AtomUI.Desktop.Controls.Internal.DateViewer.DateViewerCell>)typeof(CalendarViewControl)
                .GetField("_pool", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(view)!;
            pool.Count.ShouldBe(42);
            foreach (var cell in pool.Take(12))
            {
                cell.Context.ShouldNotBeNull();
            }
            foreach (var cell in pool.Skip(12))
            {
                cell.Context.ShouldBeNull();
            }
        }
        finally
        {
            window.Close();
        }
    }

    private static IDataTemplate MarkerTemplate(string marker) =>
        new FuncDataTemplate<object?>((_, _) => new Border
        {
            Tag = marker,
            Width = 8,
            Height = 8
        });

    private static Border GetCellInner(AtomUI.Desktop.Controls.Internal.DateViewer.DateViewerCell cell) =>
        cell.GetVisualDescendants()
            .OfType<Border>()
            .Single(border => border.Name == "PART_CellInner");

    private static T GetThemeResource<T>(object key)
    {
        var application = Application.Current.ShouldNotBeNull();
        application!.TryGetResource(key, application.ActualThemeVariant, out var value).ShouldBeTrue();
        value.ShouldBeAssignableTo<T>();
        return (T)value!;
    }

    private static Color? BrushColor(IBrush? brush) => (brush as ISolidColorBrush)?.Color;

    private static AvaloniaWindow Show(Control content, double width = 420, double height = 420)
    {
        var window = new AvaloniaWindow
        {
            Width = width,
            Height = height,
            Content = content
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }
}
