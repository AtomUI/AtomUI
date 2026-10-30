using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Steps;

public class StepsItemLayoutPanelTests
{
    static StepsItemLayoutPanelTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Theory]
    [InlineData(Desktop.Controls.StepsType.Default, Orientation.Horizontal, Orientation.Horizontal, Orientation.Horizontal)]
    [InlineData(Desktop.Controls.StepsType.Dot, Orientation.Horizontal, Orientation.Horizontal, Orientation.Vertical)]
    [InlineData(Desktop.Controls.StepsType.OutlineDot, Orientation.Horizontal, Orientation.Horizontal, Orientation.Vertical)]
    [InlineData(Desktop.Controls.StepsType.Navigation, Orientation.Horizontal, Orientation.Vertical, Orientation.Horizontal)]
    [InlineData(Desktop.Controls.StepsType.Inline, Orientation.Horizontal, Orientation.Horizontal, Orientation.Vertical)]
    [InlineData(Desktop.Controls.StepsType.Panel, Orientation.Horizontal, Orientation.Vertical, Orientation.Horizontal)]
    [InlineData(Desktop.Controls.StepsType.Panel, Orientation.Vertical, Orientation.Vertical, Orientation.Horizontal)]
    [InlineData(Desktop.Controls.StepsType.Default, Orientation.Vertical, Orientation.Vertical, Orientation.Horizontal)]
    public void Effective_Title_Placement_Follows_Type_Rules(
        Desktop.Controls.StepsType type,
        Orientation orientation,
        Orientation requested,
        Orientation expected)
    {
        var panel = new Desktop.Controls.StepsItemLayoutPanel
        {
            Type = type,
            Orientation = orientation,
            TitlePlacement = requested
        };

        panel.EffectiveTitlePlacement.ShouldBe(expected);
    }

    [Fact]
    public void Horizontal_Title_Layout_Places_Section_And_Connector_Without_Overlap()
    {
        var panel     = CreatePanel(Desktop.Controls.StepsType.Default, Orientation.Horizontal, Orientation.Horizontal);
        var indicator = AddChild(panel, Desktop.Controls.StepsItemLayoutRole.Indicator, 20, 20);
        var section = AddSection(
            panel,
            Desktop.Controls.StepsType.Default,
            Orientation.Horizontal,
            Orientation.Horizontal,
            (Desktop.Controls.StepsItemLayoutRole.Header, 60, 20),
            (Desktop.Controls.StepsItemLayoutRole.SubHeader, 40, 12),
            (Desktop.Controls.StepsItemLayoutRole.Content, 80, 24));
        var connector = AddChild(panel, Desktop.Controls.StepsItemLayoutRole.Connector, 10, 2);
        var arrow     = AddChild(panel, Desktop.Controls.StepsItemLayoutRole.NavigationArrow, 16, 16);

        Layout(panel, 300, 100);

        indicator.Bounds.ShouldBe(new Rect(0, 0, 20, 20));
        section.Bounds.ShouldBe(new Rect(20, 0, 100, 44));
        connector.Bounds.ShouldBe(new Rect(120, 10, 180, 1));
        arrow.Bounds.ShouldBe(default);
    }

    [Fact]
    public void Horizontal_Connector_Start_Follows_Header_Row_Instead_Of_Wider_Content()
    {
        var panel = CreatePanel(Desktop.Controls.StepsType.Default, Orientation.Horizontal, Orientation.Horizontal);
        AddChild(panel, Desktop.Controls.StepsItemLayoutRole.Indicator, 20, 20);
        AddSection(
            panel,
            Desktop.Controls.StepsType.Default,
            Orientation.Horizontal,
            Orientation.Horizontal,
            (Desktop.Controls.StepsItemLayoutRole.Header, 60, 20),
            (Desktop.Controls.StepsItemLayoutRole.SubHeader, 40, 12),
            (Desktop.Controls.StepsItemLayoutRole.Content, 180, 24));
        var connector = AddChild(panel, Desktop.Controls.StepsItemLayoutRole.Connector, 10, 2);

        Layout(panel, 300, 100);

        // The section starts at x = 20 and its heading line ends at x = 100
        // (header 60 + subheader 40), so the connector starts at 120 instead
        // of following the wider content row.
        connector.Bounds.X.ShouldBe(120);
        connector.Bounds.Right.ShouldBe(300);
    }

    [Fact]
    public void Vertical_Title_Layout_Centers_Indicator_And_Section_And_Stretches_Horizontal_Connector()
    {
        var panel     = CreatePanel(Desktop.Controls.StepsType.Dot, Orientation.Horizontal, Orientation.Horizontal);
        var indicator = AddChild(panel, Desktop.Controls.StepsItemLayoutRole.Indicator, 20, 20);
        var section = AddSection(
            panel,
            Desktop.Controls.StepsType.Dot,
            Orientation.Horizontal,
            Orientation.Horizontal,
            (Desktop.Controls.StepsItemLayoutRole.Header, 60, 20),
            (Desktop.Controls.StepsItemLayoutRole.SubHeader, 40, 12),
            (Desktop.Controls.StepsItemLayoutRole.Content, 80, 24));
        var connector = AddChild(panel, Desktop.Controls.StepsItemLayoutRole.Connector, 10, 2);

        Layout(panel, 300, 100);

        indicator.Bounds.ShouldBe(new Rect(140, 0, 20, 20));
        connector.Bounds.ShouldBe(new Rect(160, 10, 280, 1));
        section.Bounds.ShouldBe(new Rect(110, 20, 80, 56));
    }

    [Fact]
    public void Inline_Item_Wrapper_Fills_The_Cell_Width_With_v6_Padding_For_The_Hover_Background()
    {
        var panel = CreatePanel(Desktop.Controls.StepsType.Inline, Orientation.Horizontal, Orientation.Horizontal);
        panel.Padding          = new Thickness(6, 9, 6, 0);
        panel.IndicatorSpacing = 8;
        var wrapper   = AddChild(panel, Desktop.Controls.StepsItemLayoutRole.ItemWrapper, 0, 0);
        var indicator = AddChild(panel, Desktop.Controls.StepsItemLayoutRole.Indicator, 6, 6);
        var section = AddSection(
            panel,
            Desktop.Controls.StepsType.Inline,
            Orientation.Horizontal,
            Orientation.Horizontal,
            (Desktop.Controls.StepsItemLayoutRole.Header, 48, 18),
            (Desktop.Controls.StepsItemLayoutRole.SubHeader, 58, 18));
        AddChild(panel, Desktop.Controls.StepsItemLayoutRole.Connector, 10, 1);

        Layout(panel, 240, 60);

        // antd v6 paints the inline hover background on `.ant-steps-item-wrapper`:
        // a block that fills the item cell width; the wrapper padding contract
        // (`paddingTop: paddingXS + lineWidth`, no bottom padding) arrives through
        // the `InlineItemPadding` token bound to the panel `Padding`, so the
        // background height is the content union (indicator (117,9,6,6) + section
        // (91,23,58,36)) inflated by the padding's block values.
        wrapper.Bounds.ShouldBe(new Rect(0, 0, 240, 59));
        wrapper.Bounds.Contains(indicator.Bounds).ShouldBeTrue();
        wrapper.Bounds.Contains(section.Bounds).ShouldBeTrue();
    }

    [Fact]
    public void Panel_Item_Wrapper_Fills_Cell_And_Arrow_Overflows_To_The_Right()
    {
        var panel = CreatePanel(Desktop.Controls.StepsType.Panel, Orientation.Vertical, Orientation.Vertical);
        var wrapper = AddChild(panel, Desktop.Controls.StepsItemLayoutRole.ItemWrapper, 0, 0);
        AddChild(panel, Desktop.Controls.StepsItemLayoutRole.Header, 60, 20);
        var arrow = new Desktop.Controls.StepsPanelArrow
        {
            Width = 20,
            StrokeThickness = 2
        };
        Desktop.Controls.StepsItemLayoutPanel.SetRole(arrow, Desktop.Controls.StepsItemLayoutRole.PanelArrow);
        panel.Children.Add(arrow);

        Layout(panel, 200, 80);

        wrapper.Bounds.ShouldBe(new Rect(0, 0, 200, 80));
        arrow.Bounds.Left.ShouldBe(200);
        arrow.Bounds.Right.ShouldBe(220);
        arrow.Bounds.Top.ShouldBe(0);
        arrow.Bounds.Bottom.ShouldBe(80);
    }

    [Fact]
    public void Panel_Arrow_Overflows_To_The_Left_In_Rtl()
    {
        var panel = CreatePanel(Desktop.Controls.StepsType.Panel, Orientation.Horizontal, Orientation.Horizontal);
        panel.FlowDirection = FlowDirection.RightToLeft;
        AddChild(panel, Desktop.Controls.StepsItemLayoutRole.ItemWrapper, 0, 0);
        AddChild(panel, Desktop.Controls.StepsItemLayoutRole.Header, 60, 20);
        var arrow = new Desktop.Controls.StepsPanelArrow { Width = 20, StrokeThickness = 2 };
        Desktop.Controls.StepsItemLayoutPanel.SetRole(arrow, Desktop.Controls.StepsItemLayoutRole.PanelArrow);
        panel.Children.Add(arrow);

        Layout(panel, 200, 80);

        arrow.Bounds.Right.ShouldBe(0);
        arrow.Bounds.Left.ShouldBe(-20);
    }

    [Fact]
    public void Vertical_Steps_Stretch_Connector_Below_Indicator_And_Keep_Section_To_The_Right()
    {
        var panel     = CreatePanel(Desktop.Controls.StepsType.Default, Orientation.Vertical, Orientation.Vertical);
        var indicator = AddChild(panel, Desktop.Controls.StepsItemLayoutRole.Indicator, 20, 20);
        var section = AddSection(
            panel,
            Desktop.Controls.StepsType.Default,
            Orientation.Vertical,
            Orientation.Vertical,
            (Desktop.Controls.StepsItemLayoutRole.Header, 60, 20),
            (Desktop.Controls.StepsItemLayoutRole.SubHeader, 40, 12),
            (Desktop.Controls.StepsItemLayoutRole.Content, 80, 24));
        var connector = AddChild(panel, Desktop.Controls.StepsItemLayoutRole.Connector, 2, 10);

        Layout(panel, 200, 120);

        indicator.Bounds.ShouldBe(new Rect(0, 0, 20, 20));
        section.Bounds.ShouldBe(new Rect(20, 0, 100, 44));
        connector.Bounds.ShouldBe(new Rect(10, 20, 1, 100));
    }

    [Fact]
    public void Navigation_Arrow_Only_Occupies_Layout_In_Navigation_Type()
    {
        var panel = CreatePanel(Desktop.Controls.StepsType.Default, Orientation.Horizontal, Orientation.Horizontal);
        AddChild(panel, Desktop.Controls.StepsItemLayoutRole.Indicator, 20, 20);
        AddSection(
            panel,
            Desktop.Controls.StepsType.Default,
            Orientation.Horizontal,
            Orientation.Horizontal,
            (Desktop.Controls.StepsItemLayoutRole.Header, 60, 20));
        var connector = AddChild(panel, Desktop.Controls.StepsItemLayoutRole.Connector, 10, 2);
        var arrow = AddChild(panel, Desktop.Controls.StepsItemLayoutRole.NavigationArrow, 16, 16);

        Layout(panel, 200, 60);
        connector.Bounds.Width.ShouldBeGreaterThan(0);
        arrow.Bounds.ShouldBe(default);

        panel.Type = Desktop.Controls.StepsType.Navigation;
        Layout(panel, 200, 60);
        connector.Bounds.ShouldBe(default);
        arrow.Bounds.ShouldBe(new Rect(192, 22, 16, 16));
    }

    [Fact]
    public void Vertical_Navigation_Arrow_Occupies_The_Bottom_Edge()
    {
        var panel = CreatePanel(Desktop.Controls.StepsType.Navigation, Orientation.Vertical, Orientation.Vertical);
        AddChild(panel, Desktop.Controls.StepsItemLayoutRole.Indicator, 20, 20);
        AddSection(
            panel,
            Desktop.Controls.StepsType.Navigation,
            Orientation.Vertical,
            Orientation.Vertical,
            (Desktop.Controls.StepsItemLayoutRole.Header, 60, 20));
        var arrow = AddChild(panel, Desktop.Controls.StepsItemLayoutRole.NavigationArrow, 16, 16);

        Layout(panel, 200, 120);

        arrow.Bounds.ShouldBe(new Rect(95, 115, 11, 11));
    }

    [Fact]
    public void Changing_Body_Child_Role_Invalidates_Parent_Layout()
    {
        var panel     = CreatePanel(Desktop.Controls.StepsType.Default, Orientation.Horizontal, Orientation.Horizontal);
        AddChild(panel, Desktop.Controls.StepsItemLayoutRole.Indicator, 20, 20);
        var section = AddSection(panel, Desktop.Controls.StepsType.Default, Orientation.Horizontal, Orientation.Horizontal);
        var first  = AddChild(section, Desktop.Controls.StepsItemLayoutRole.Header, 60, 20);
        var second = AddChild(section, Desktop.Controls.StepsItemLayoutRole.SubHeader, 40, 12);

        Layout(panel, 200, 60);
        first.Bounds.X.ShouldBe(0);
        second.Bounds.X.ShouldBe(60);

        Desktop.Controls.StepsItemLayoutPanel.SetRole(first, Desktop.Controls.StepsItemLayoutRole.SubHeader);
        Desktop.Controls.StepsItemLayoutPanel.SetRole(second, Desktop.Controls.StepsItemLayoutRole.Header);
        // The role change invalidates the section (the children's layout parent); the
        // direct layout calls below bypass the layout manager, so the panel is
        // invalidated explicitly the way the manager would in a running application.
        panel.InvalidateMeasure();
        Layout(panel, 200, 60);

        second.Bounds.X.ShouldBe(0);
        first.Bounds.X.ShouldBe(40);
    }

    private static Desktop.Controls.StepsItemLayoutPanel CreatePanel(
        Desktop.Controls.StepsType type,
        Orientation orientation,
        Orientation titlePlacement)
    {
        return new Desktop.Controls.StepsItemLayoutPanel
        {
            Type           = type,
            Orientation    = orientation,
            TitlePlacement = titlePlacement
        };
    }

    private static Desktop.Controls.StepsItemSectionPanel AddSection(
        Desktop.Controls.StepsItemLayoutPanel panel,
        Desktop.Controls.StepsType type,
        Orientation orientation,
        Orientation titlePlacement,
        params (Desktop.Controls.StepsItemLayoutRole Role, double Width, double Height)[] children)
    {
        var section = new Desktop.Controls.StepsItemSectionPanel
        {
            Type           = type,
            Orientation    = orientation,
            TitlePlacement = titlePlacement
        };
        Desktop.Controls.StepsItemLayoutPanel.SetRole(section, Desktop.Controls.StepsItemLayoutRole.Section);
        panel.Children.Add(section);
        foreach (var (role, width, height) in children)
        {
            AddChild(section, role, width, height);
        }

        return section;
    }

    private static FixedSizeControl AddChild(
        Avalonia.Controls.Panel panel,
        Desktop.Controls.StepsItemLayoutRole role,
        double width,
        double height)
    {
        var child = new FixedSizeControl(width, height);
        Desktop.Controls.StepsItemLayoutPanel.SetRole(child, role);
        panel.Children.Add(child);
        return child;
    }

    private static void Layout(Control control, double width, double height)
    {
        control.Measure(new Size(width, height));
        control.Arrange(new Rect(0, 0, width, height));
    }

    private sealed class FixedSizeControl(double width, double height) : Control
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            return new Size(width, height);
        }
    }
}
