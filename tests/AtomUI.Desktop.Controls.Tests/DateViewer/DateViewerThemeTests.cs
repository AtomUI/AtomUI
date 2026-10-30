// Generated Semantic Style and Automation use the same realized cell containers.
using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Avalonia.Media;
using AtomUI.Desktop.Controls.Internal.DateViewer;
using AtomUI.Theme.Resources;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.DateViewers;

public class DateViewerThemeTests
{
    static DateViewerThemeTests() => AvaloniaTestApp.EnsureInitialized();

    [Fact]
    public void Dedicated_Cell_Style_Hits_All_Realized_Cells()
    {
        var styleType = typeof(DateViewer).Assembly.GetTypes().SingleOrDefault(type => type.Name == "DateViewerCellStyle");
        styleType.ShouldNotBeNull("public owner must generate its dedicated cell style");
        var part = (Style)Activator.CreateInstance(styleType!)!;
        part.Setters.Add(new Setter(Control.TagProperty, "dedicated-style-hit"));
        var owner = new Style(selector => selector.OfType<DateViewer>());
        owner.Children.Add(part);
        var viewer = new DateViewer { DisplayDate = new DateTime(2026, 7, 15) };
        viewer.Styles.Add(owner);
        var window = Show(viewer);
        try { viewer.GetVisualDescendants().OfType<DateViewerCell>().ShouldAllBe(cell => Equals(cell.Tag, "dedicated-style-hit")); }
        finally { window.Close(); Dispatcher.UIThread.RunJobs(); }
    }

    [Fact]
    public void Content_Today_Keeps_Its_Surface_When_Disabled_And_Reenabled()
    {
        var today = DateTime.Today;
        var viewer = new DateViewer
        {
            Width = 560,
            DisplayDate = today,
            Presentation = DateViewerPresentation.Content,
            MinDate = today.AddDays(1),
            DisabledDate = date => date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday,
            CellTemplate = new Avalonia.Controls.Templates.FuncDataTemplate<DateViewerCellContext>(
                (_, _) => new TextBlock { Text = "Available" })
        };
        var window = new Avalonia.Controls.Window { Width = 700, Height = 800, Content = viewer };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            DateViewerCell TodayCell() => viewer.GetVisualDescendants().OfType<DateViewerCell>()
                .Single(cell => cell.Model?.Value == today && cell.Model.IsInView);
            Border Surface(DateViewerCell cell) => cell.GetVisualDescendants().OfType<Border>()
                .Single(border => border.Name == "PART_CellSurface");
            Border ValueFrame(DateViewerCell cell) => cell.GetVisualDescendants().OfType<Border>()
                .Single(border => border.Name == "PART_ValueFrame");
            TextBlock ValueText(DateViewerCell cell) => cell.GetVisualDescendants().OfType<TextBlock>()
                .Single(text => text.Name == "PART_Value");
            Color? BrushColor(IBrush? brush) => (brush as ISolidColorBrush)?.Color;

            var disabledToday = TodayCell();
            disabledToday.Model!.IsDisabled.ShouldBeTrue();
            var otherDisabled = viewer.GetVisualDescendants().OfType<DateViewerCell>()
                .First(cell => cell.Model is { IsDisabled: true, IsToday: false, IsInView: true });
            var disabledLineColor = BrushColor(Surface(otherDisabled).BorderBrush);
            var todayLineColor = BrushColor(Surface(disabledToday).BorderBrush);
            todayLineColor.ShouldNotBeNull();
            todayLineColor.ShouldNotBe(disabledLineColor);
            var todayBackground = BrushColor(Surface(disabledToday).Background);
            todayBackground.ShouldNotBeNull();
            todayBackground.ShouldNotBe(BrushColor(Surface(otherDisabled).Background));
            BrushColor(ValueText(disabledToday).Foreground).ShouldBe(todayLineColor);
            Surface(disabledToday).BorderThickness.ShouldBe(new Thickness(0, 2, 0, 0));
            ValueFrame(disabledToday).BorderThickness.ShouldBe(new Thickness(0));
            BrushColor(ValueFrame(disabledToday).BorderBrush).ShouldBe(Colors.Transparent);
            disabledToday.Activate();
            viewer.Value.ShouldBeNull();

            viewer.MinDate = null;
            viewer.DisabledDate = null;
            Dispatcher.UIThread.RunJobs();
            var enabledToday = TodayCell();
            enabledToday.Model!.IsDisabled.ShouldBeFalse();
            BrushColor(Surface(enabledToday).BorderBrush).ShouldBe(todayLineColor);
            BrushColor(Surface(enabledToday).Background).ShouldBe(todayBackground);
            BrushColor(ValueText(enabledToday).Foreground).ShouldBe(todayLineColor);
            BrushColor(ValueFrame(enabledToday).BorderBrush).ShouldBe(Colors.Transparent);

            Application.Current!.TryGetResource(SharedTokenKind.MotionDurationSlow,
                Application.Current.ActualThemeVariant, out var slowDuration).ShouldBeTrue();
            var selectable = viewer.GetVisualDescendants().OfType<DateViewerCell>()
                .First(cell => cell.Model is { IsDisabled: false, IsToday: false, IsInView: true, IsSelected: false });
            BrushColor(Surface(selectable).Background).ShouldBe(Colors.Transparent);
            Surface(selectable).Transitions.ShouldNotBeNull()
                .OfType<AtomUI.Animations.SolidColorBrushTransition>()
                .Single(transition => transition.Property == Border.BackgroundProperty)
                .Duration.ShouldBe((TimeSpan)slowDuration!);
            ValueText(selectable).Transitions.ShouldNotBeNull()
                .OfType<AtomUI.Animations.SolidColorBrushTransition>()
                .Single(transition => transition.Property == TextBlock.ForegroundProperty)
                .Duration.ShouldBe((TimeSpan)slowDuration!);
            selectable.IsMotionEnabled = false;
            Dispatcher.UIThread.RunJobs();
            Surface(selectable).Transitions.ShouldBeNull();
            ValueText(selectable).Transitions.ShouldBeNull();
        }
        finally { window.Close(); Dispatcher.UIThread.RunJobs(); }
    }

    [Fact]
    public void Automation_Uses_The_Same_Date_Selection_Path()
    {
        var viewer = new DateViewer { DisplayDate = new DateTime(2026, 7, 15) };
        var window = Show(viewer);
        try
        {
            var cell = viewer.GetVisualDescendants().OfType<DateViewerCell>().Single(c => c.Model!.Value == new DateTime(2026, 7, 20));
            var peer = ControlAutomationPeer.CreatePeerForElement(cell);
            peer.ShouldBeAssignableTo<ISelectionItemProvider>().ShouldNotBeNull();
            ((ISelectionItemProvider)peer!).Select(); Dispatcher.UIThread.RunJobs();
            viewer.Value.ShouldBe(new DateTime(2026, 7, 20));
            ((ISelectionItemProvider)peer!).SelectionContainer.ShouldNotBeNull();
        }
        finally { window.Close(); Dispatcher.UIThread.RunJobs(); }
    }

    [Fact]
    public void Range_Preview_Paints_A_Continuous_Band_Without_Replacing_The_Candidate()
    {
        var start = new DateTime(2026, 7, 8);
        var end = new DateTime(2026, 7, 20);
        var viewer = new RangeDateViewer
        {
            DisplayDate = new DateTime(2026, 7, 1),
            Value = new DateViewerRange(start, null)
        };
        var window = new Avalonia.Controls.Window { Width = 700, Height = 400, Content = viewer };
        try
        {
            window.Show(); Dispatcher.UIThread.RunJobs();
            viewer.PanelSession.Apply(new DatePanelAction.Hover(end));
            Dispatcher.UIThread.RunJobs();
            var cells = viewer.GetVisualDescendants().OfType<DateViewerCell>().ToArray();
            var middle = cells.Single(c => c.Model!.Value == new DateTime(2026, 7, 15));
            var band = middle.GetVisualDescendants().OfType<Border>().SingleOrDefault(border => border.Name == "PART_RangeMiddle");
            band.ShouldNotBeNull("range states must reach the actual template");
            band!.IsVisible.ShouldBeTrue();
            band.Bounds.Width.ShouldBe(middle.Bounds.Width);
            viewer.Value.ShouldBe(new DateViewerRange(start, null));

            var startCell = cells.Single(c => c.Model!.Value == start && c.Model.IsInView);
            var endCell = cells.Single(c => c.Model!.Value == end && c.Model.IsInView);
            startCell.Model!.IsSelected.ShouldBeFalse();
            startCell.Model.IsRangePreviewStart.ShouldBeTrue();
            endCell.Model!.IsRangePreviewEnd.ShouldBeTrue();
            ((ISolidColorBrush)startCell.Background!).Color
                .ShouldBe(((ISolidColorBrush)endCell.Background!).Color);
            Border ValueFrame(DateViewerCell cell) => cell.GetVisualDescendants().OfType<Border>()
                .Single(border => border.Name == "PART_ValueFrame");
            var valueTransition = ValueFrame(startCell).Transitions?
                .OfType<AtomUI.Animations.SolidColorBrushTransition>()
                .SingleOrDefault(transition => transition.Property == Border.BackgroundProperty);
            valueTransition.ShouldNotBeNull();
            valueTransition.Duration.ShouldBeGreaterThan(TimeSpan.Zero);
            startCell.IsMotionEnabled = false;
            Dispatcher.UIThread.RunJobs();
            ValueFrame(startCell).Transitions.ShouldBeNull();
            ValueFrame(startCell).CornerRadius.ShouldBe(new CornerRadius(
                startCell.CornerRadius.TopLeft, 0, 0, startCell.CornerRadius.BottomLeft));
            ValueFrame(endCell).CornerRadius.ShouldBe(new CornerRadius(
                0, endCell.CornerRadius.TopRight, endCell.CornerRadius.BottomRight, 0));
        }
        finally { window.Close(); Dispatcher.UIThread.RunJobs(); }
    }

    [Fact]
    public void Week_Endpoints_Paint_All_Eight_Columns_Including_The_Week_Number()
    {
        var viewer = new RangeDateViewer
        {
            DisplayDate = new DateTime(2026, 7, 1), SelectionUnit = DateViewerSelectionUnit.Week,
            FirstDayOfWeek = DayOfWeek.Monday,
            Value = new DateViewerRange(new DateTime(2026, 7, 6), new DateTime(2026, 7, 20))
        };
        var window = Show(viewer);
        try
        {
            var row = viewer.GetVisualDescendants().OfType<DateViewerCell>()
                .Where(c => c.Model!.IsWeekSelectionStart || c.Model.IsWeekSelectionMiddle || c.Model.IsWeekSelectionEnd).Take(8).ToArray();
            row.Length.ShouldBe(8);
            row[0].Model!.Kind.ShouldBe(DateViewerCellType.Week);
            foreach (var cell in row)
            {
                var band = cell.GetVisualDescendants().OfType<Border>().SingleOrDefault(border => border.Name == "PART_WeekSelection");
                band.ShouldNotBeNull("week selection must cover the whole row");
                band!.IsVisible.ShouldBeTrue();
                band.Bounds.Width.ShouldBe(cell.Bounds.Width);
                var weekTransition = band.Transitions?
                    .OfType<AtomUI.Animations.SolidColorBrushTransition>()
                    .SingleOrDefault(transition => transition.Property == Border.BackgroundProperty);
                weekTransition.ShouldNotBeNull();
                weekTransition.Duration.ShouldBeGreaterThan(TimeSpan.Zero);
            }
            row[0].IsMotionEnabled = false;
            Dispatcher.UIThread.RunJobs();
            row[0].GetVisualDescendants().OfType<Border>()
                .Single(border => border.Name == "PART_WeekSelection").Transitions.ShouldBeNull();
            var middleWeek = viewer.GetVisualDescendants().OfType<DateViewerCell>()
                .Single(cell => cell.Model?.Value == new DateTime(2026, 7, 13) &&
                                cell.Model.Kind == DateViewerCellType.Week);
            var middleWeekBand = middleWeek.GetVisualDescendants().OfType<Border>()
                .Single(border => border.Name == "PART_WeekSelection");
            middleWeekBand.IsVisible.ShouldBeTrue();
            middleWeek.GetVisualDescendants().OfType<Border>()
                .Single(border => border.Name == "PART_RangeMiddle").IsVisible.ShouldBeFalse();
        }
        finally { window.Close(); Dispatcher.UIThread.RunJobs(); }
    }

    [Fact]
    public void Week_Automation_Name_Contains_The_Actual_Week_Number()
    {
        var viewer = new DateViewer { DisplayDate = new DateTime(2026, 7, 1), ShowWeek = true };
        var window = Show(viewer);
        try
        {
            foreach (var cell in viewer.GetVisualDescendants().OfType<DateViewerCell>().Where(c => c.Model!.Kind == DateViewerCellType.Week))
                ControlAutomationPeer.CreatePeerForElement(cell)!.GetName()!.ShouldContain(cell.DisplayText);
        }
        finally { window.Close(); Dispatcher.UIThread.RunJobs(); }
    }

    private static Avalonia.Controls.Window Show(Control control)
    {
        var window = new Avalonia.Controls.Window { Width = 360, Height = 400, Content = control };
        window.Show(); Dispatcher.UIThread.RunJobs(); return window;
    }
}
