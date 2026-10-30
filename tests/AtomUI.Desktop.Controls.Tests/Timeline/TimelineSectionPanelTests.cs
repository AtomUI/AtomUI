using AtomUI.Controls;
using AtomUI.Controls.Commons;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Layout;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Timeline;

public class TimelineSectionPanelTests
{
    static TimelineSectionPanelTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Horizontal_Stacked_Start_Places_Axis_Above_Label_And_Content()
    {
        var layout = CreateLayout(
            Orientation.Horizontal,
            TimelineMode.Start,
            isOdd: false,
            isLabelLayout: true,
            indicatorSpacing: 5);

        layout.Panel.Measure(new Size(100, double.PositiveInfinity));
        layout.Panel.Arrange(new Rect(0, 0, 100, layout.Panel.DesiredSize.Height));

        layout.Panel.DesiredSize.ShouldBe(new Size(100, 70));
        layout.Indicator.Bounds.ShouldBe(new Rect(0, 0, 100, 10));
        layout.Header.Bounds.ShouldBe(new Rect(0, 15, 100, 20));
        layout.Label.Bounds.ShouldBe(new Rect(0, 0, 100, 20));
        layout.Content.Bounds.ShouldBe(new Rect(0, 40, 100, 30));
    }

    [Fact]
    public void Horizontal_Stacked_End_Places_Axis_Below_Label_And_Content()
    {
        var layout = CreateLayout(
            Orientation.Horizontal,
            TimelineMode.End,
            isOdd: false,
            isLabelLayout: true,
            indicatorSpacing: 5);

        layout.Panel.Measure(new Size(100, double.PositiveInfinity));
        layout.Panel.Arrange(new Rect(0, 0, 100, layout.Panel.DesiredSize.Height));

        layout.Panel.DesiredSize.ShouldBe(new Size(100, 70));
        layout.Header.Bounds.ShouldBe(new Rect(0, 0, 100, 20));
        layout.Label.Bounds.ShouldBe(new Rect(0, 0, 100, 20));
        layout.Content.Bounds.ShouldBe(new Rect(0, 25, 100, 30));
        layout.Indicator.Bounds.ShouldBe(new Rect(0, 60, 100, 10));
    }

    [Theory]
    [InlineData(false, 0, 50)]
    [InlineData(true, 50, 0)]
    public void Horizontal_Alternate_Uses_Visual_Parity(bool isOdd, double labelY, double contentY)
    {
        var layout = CreateLayout(
            Orientation.Horizontal,
            TimelineMode.Alternate,
            isOdd,
            isLabelLayout: false,
            indicatorSpacing: 5);

        layout.Panel.Measure(new Size(100, double.PositiveInfinity));
        layout.Panel.Arrange(new Rect(0, 0, 100, layout.Panel.DesiredSize.Height));

        layout.Header.Bounds.Y.ShouldBe(labelY);
        layout.Indicator.Bounds.ShouldBe(new Rect(0, 35, 100, 10));
        layout.Content.Bounds.Y.ShouldBe(contentY);
    }

    [Theory]
    [InlineData(TimelineMode.Start, 0, 15)]
    [InlineData(TimelineMode.End, 35, 0)]
    public void Horizontal_Single_Sided_Mode_Places_Content_On_Requested_Side(
        TimelineMode mode,
        double indicatorY,
        double contentY)
    {
        var layout = CreateLayout(
            Orientation.Horizontal,
            mode,
            isOdd: false,
            isLabelLayout: false,
            indicatorSpacing: 5);

        layout.Panel.Measure(new Size(100, double.PositiveInfinity));
        layout.Panel.Arrange(new Rect(0, 0, 100, layout.Panel.DesiredSize.Height));

        layout.Panel.DesiredSize.ShouldBe(new Size(100, 45));
        layout.Indicator.Bounds.ShouldBe(new Rect(0, indicatorY, 100, 10));
        layout.Content.Bounds.ShouldBe(new Rect(0, contentY, 100, 30));
    }

    [Theory]
    [InlineData(false, 0, 60)]
    [InlineData(true, 60, 0)]
    public void Vertical_Alternate_Starts_From_Start(bool isOdd, double labelX, double contentX)
    {
        var layout = CreateLayout(
            Orientation.Vertical,
            TimelineMode.Alternate,
            isOdd,
            isLabelLayout: false,
            indicatorSpacing: 5);

        layout.Panel.Measure(new Size(110, 40));
        layout.Panel.Arrange(new Rect(0, 0, 110, 40));

        layout.Header.Bounds.X.ShouldBe(labelX);
        layout.Indicator.Bounds.ShouldBe(new Rect(50, 0, 10, 40));
        layout.Content.Bounds.X.ShouldBe(contentX);
    }

    [Fact]
    public void Vertical_Label_Layout_Start_Stretches_Slots()
    {
        var layout = CreateLayout(
            Orientation.Vertical,
            TimelineMode.Start,
            isOdd: false,
            isLabelLayout: true,
            indicatorSpacing: 5);

        layout.Panel.Measure(new Size(110, 40));
        layout.Panel.Arrange(new Rect(0, 0, 110, 40));

        layout.Header.Bounds.ShouldBe(new Rect(0, 0, 50, 40));
        layout.Label.Bounds.Width.ShouldBe(50);
        layout.Content.Bounds.ShouldBe(new Rect(60, 0, 50, 40));
    }

    [Fact]
    public void Vertical_Label_Layout_End_Stretches_Slots()
    {
        var layout = CreateLayout(
            Orientation.Vertical,
            TimelineMode.End,
            isOdd: false,
            isLabelLayout: true,
            indicatorSpacing: 5);

        layout.Panel.Measure(new Size(110, 40));
        layout.Panel.Arrange(new Rect(0, 0, 110, 40));

        layout.Header.Bounds.ShouldBe(new Rect(60, 0, 50, 40));
        layout.Content.Bounds.ShouldBe(new Rect(0, 0, 50, 40));
    }

    [Fact]
    public void Vertical_Label_Layout_Keeps_A_Header_Slot_For_An_Empty_Label()
    {
        var layout = CreateLayout(
            Orientation.Vertical,
            TimelineMode.Start,
            isOdd: false,
            isLabelLayout: true,
            indicatorSpacing: 5);
        layout.Label.Text = null;

        layout.Panel.Measure(new Size(110, 40));
        layout.Panel.Arrange(new Rect(0, 0, 110, 40));

        layout.Header.Bounds.ShouldBe(new Rect(0, 0, 50, 40));
        layout.Label.Bounds.Width.ShouldBe(50);
    }

    [Fact]
    public void Vertical_Arrange_Extends_The_Indicator_By_The_Axis_Overflow()
    {
        var layout = CreateLayout(
            Orientation.Vertical,
            TimelineMode.Start,
            isOdd: false,
            isLabelLayout: false,
            indicatorSpacing: 5);
        SetProperty(layout.Panel, "AxisOverflow", new Avalonia.Thickness(0, 0, 0, 15));

        layout.Panel.Measure(new Size(110, 40));
        layout.Panel.Arrange(new Rect(0, 0, 110, 40));

        layout.Indicator.Bounds.ShouldBe(new Rect(0, 0, 10, 55));
    }

    private static LayoutParts CreateLayout(
        Orientation orientation,
        TimelineMode mode,
        bool isOdd,
        bool isLabelLayout,
        double indicatorSpacing)
    {
        var controlsAssembly = typeof(AbstractTimeline).Assembly;
        var panelType = controlsAssembly.GetType(
            "AtomUI.Controls.Commons.TimelineSectionPanel",
            throwOnError: true)!;
        var indicatorType = controlsAssembly.GetType(
            "AtomUI.Controls.Commons.TimelineIndicator",
            throwOnError: true)!;
        var panel     = (Panel)Activator.CreateInstance(panelType)!;
        var indicator = (Control)Activator.CreateInstance(indicatorType)!;
        var label = new TextBlock
        {
            Text      = "Label",
            MinHeight = 20
        };
        var header = new StackPanel
        {
            Children = { label }
        };
        var content = new ContentPresenter
        {
            Content   = "Content",
            MinHeight = 30
        };
        indicator.MinWidth  = 10;
        indicator.MinHeight = 10;

        SetProperty(panel, "Orientation", orientation);
        SetProperty(panel, "Mode", mode);
        SetProperty(panel, "IsOdd", isOdd);
        SetProperty(panel, "IsLabelLayout", isLabelLayout);
        SetProperty(panel, "IndicatorSpacing", indicatorSpacing);

        panel.Children.Add(header);
        panel.Children.Add(indicator);
        panel.Children.Add(content);
        return new LayoutParts(panel, header, label, indicator, content);
    }

    private static void SetProperty(Control control, string propertyName, object value)
    {
        var property = control.GetType().GetProperty(propertyName);
        property.ShouldNotBeNull().SetValue(control, value);
    }

    private sealed record LayoutParts(
        Panel Panel,
        StackPanel Header,
        TextBlock Label,
        Control Indicator,
        ContentPresenter Content);
}
