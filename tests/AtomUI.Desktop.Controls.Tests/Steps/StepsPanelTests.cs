using Avalonia;
using Avalonia.Layout;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Steps;

public class StepsPanelTests
{
    static StepsPanelTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Horizontal_Default_Keeps_Last_Item_AutoSized()
    {
        var panel = new Desktop.Controls.StepsPanel
        {
            Type = Desktop.Controls.StepsType.Default,
            Orientation = Orientation.Horizontal
        };
        panel.Children.Add(new FixedSizeControl(40, 24));
        panel.Children.Add(new FixedSizeControl(80, 24));
        panel.Children.Add(new FixedSizeControl(60, 24));

        panel.Measure(new Size(300, 100));
        panel.Arrange(new Rect(0, 0, 300, 100));

        panel.Children.Select(child => child.Bounds.Width).ShouldBe([120d, 120d, 60d]);
        panel.Children.Select(child => child.Bounds.X).ShouldBe([0d, 120d, 240d]);
    }

    [Theory]
    [InlineData(900d)]
    [InlineData(1400d)]
    public void Horizontal_Default_Requests_Content_Width_And_Stretches_When_Arranged(double availableWidth)
    {
        var panel = new Desktop.Controls.StepsPanel { UseLayoutRounding = false };
        for (var index = 0; index < 4; index++)
        {
            panel.Children.Add(new FixedSizeControl(100, 24));
        }

        panel.Measure(new Size(availableWidth, 100));
        panel.DesiredSize.ShouldBe(new Size(400, 24));

        panel.Arrange(new Rect(0, 0, availableWidth, 100));
        panel.Children[^1].Bounds.Width.ShouldBe(100d);
        panel.Children[^1].Bounds.Right.ShouldBe(availableWidth, 0.01);
        panel.Children[0].Bounds.Width.ShouldBe((availableWidth - 100) / 3, 0.01);
    }

    [Theory]
    [InlineData(StepsType.Default, Orientation.Horizontal, Orientation.Horizontal, 0, 220d)]
    [InlineData(StepsType.Default, Orientation.Horizontal, Orientation.Vertical, 2, 240d)]
    [InlineData(StepsType.Dot, Orientation.Horizontal, Orientation.Horizontal, 2, 240d)]
    [InlineData(StepsType.OutlineDot, Orientation.Horizontal, Orientation.Horizontal, 0, 240d)]
    [InlineData(StepsType.Navigation, Orientation.Horizontal, Orientation.Horizontal, 0, 240d)]
    [InlineData(StepsType.Panel, Orientation.Horizontal, Orientation.Horizontal, 0, 240d)]
    [InlineData(StepsType.Panel, Orientation.Vertical, Orientation.Horizontal, 0, 240d)]
    [InlineData(StepsType.Inline, Orientation.Horizontal, Orientation.Horizontal, 2, 400d)]
    public void Natural_Width_Reserves_The_Required_Item_Cells(
        StepsType type, Orientation orientation, Orientation titlePlacement, int offset, double expectedWidth)
    {
        var panel = new Desktop.Controls.StepsPanel
        {
            Type = type,
            Orientation = orientation,
            TitlePlacement = titlePlacement,
            Offset = offset,
            UseLayoutRounding = false
        };
        panel.Children.Add(new FixedSizeControl(40, 24));
        panel.Children.Add(new FixedSizeControl(80, 24));
        panel.Children.Add(new FixedSizeControl(60, 24));

        foreach (var availableWidth in new[] { double.PositiveInfinity, 900d, 1400d })
        {
            panel.Measure(new Size(availableWidth, 100));
            panel.DesiredSize.ShouldBe(new Size(expectedWidth, 24));
            panel.Arrange(new Rect(panel.DesiredSize));
            panel.Children[^1].Bounds.Right.ShouldBe(expectedWidth, 0.01);
            panel.Children[0].Bounds.Width.ShouldBeGreaterThanOrEqualTo(40);
            panel.Children[1].Bounds.Width.ShouldBeGreaterThanOrEqualTo(80);
            panel.Children[2].Bounds.Width.ShouldBeGreaterThanOrEqualTo(60);
        }
    }

    [Fact]
    public void Natural_Width_Tracks_Visibility_Content_And_Layout_Mode()
    {
        var panel = new Desktop.Controls.StepsPanel();
        panel.Children.Add(new FixedSizeControl(40, 24));
        panel.Children.Add(new FixedSizeControl(80, 24));
        panel.Children.Add(new FixedSizeControl(60, 24));
        var window = new Avalonia.Controls.Window { Width = 900, Height = 100, Content = panel };
        try
        {
            window.Show();
            window.UpdateLayout();
            panel.DesiredSize.Width.ShouldBe(220d);

            panel.Children[1].IsVisible = false;
            window.UpdateLayout();
            panel.DesiredSize.Width.ShouldBe(100d);

            panel.Type = StepsType.Navigation;
            window.UpdateLayout();
            panel.DesiredSize.Width.ShouldBe(120d);

            panel.Children[0].Width = 100;
            window.UpdateLayout();
            panel.DesiredSize.Width.ShouldBe(200d);

            panel.Children.Clear();
            window.UpdateLayout();
            panel.DesiredSize.ShouldBe(default);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Horizontal_Default_Centers_Common_Row_When_Final_Height_Exceeds_Desired_Height()
    {
        var panel = new Desktop.Controls.StepsPanel
        {
            Type        = Desktop.Controls.StepsType.Default,
            Orientation = Orientation.Horizontal
        };
        panel.Children.Add(new FixedSizeControl(40, 20));
        panel.Children.Add(new FixedSizeControl(80, 30));
        panel.Children.Add(new FixedSizeControl(60, 10));

        panel.Measure(new Size(300, 100));
        panel.Arrange(new Rect(0, 0, 300, 100));

        panel.Children.Select(child => child.Bounds.Y).ShouldBe([35d, 35d, 35d]);
        panel.Children.Select(child => child.Bounds.Height).ShouldBe([30d, 30d, 30d]);
    }

    [Fact]
    public void Layout_Panels_Are_Internal_Implementation_Details()
    {
        typeof(Desktop.Controls.StepsPanel).IsPublic.ShouldBeFalse();
        typeof(Desktop.Controls.StepsItemLayoutPanel).IsPublic.ShouldBeFalse();
    }

    [Fact]
    public void Horizontal_Navigation_Arranges_Items_Equally()
    {
        var panel = new Desktop.Controls.StepsPanel
        {
            Type = Desktop.Controls.StepsType.Navigation,
            Orientation = Orientation.Horizontal
        };
        panel.Children.Add(new FixedSizeControl(40, 24));
        panel.Children.Add(new FixedSizeControl(80, 24));
        panel.Children.Add(new FixedSizeControl(20, 24));

        panel.Measure(new Size(300, 100));
        panel.Arrange(new Rect(0, 0, 300, 100));

        panel.Children.Select(child => child.Bounds.Width)
             .ShouldBe([100d, 100d, 100d]);
    }

    [Fact]
    public void Panel_Is_Horizontal_And_Arranges_Items_Equally_Even_When_Orientation_Is_Vertical()
    {
        var panel = new Desktop.Controls.StepsPanel
        {
            Type        = Desktop.Controls.StepsType.Panel,
            Orientation = Avalonia.Layout.Orientation.Vertical
        };
        panel.Children.Add(new FixedSizeControl(40, 24));
        panel.Children.Add(new FixedSizeControl(80, 24));
        panel.Children.Add(new FixedSizeControl(20, 24));

        panel.Measure(new Size(300, 100));
        panel.Arrange(new Rect(0, 0, 300, 100));

        panel.Children.Select(child => child.Bounds.Width)
             .ShouldBe([100d, 100d, 100d]);
        panel.Children.Select(child => child.Bounds.X)
             .ShouldBe([0d, 100d, 200d]);
        panel.Children.Select(child => child.Bounds.Y)
             .ShouldBe([0d, 0d, 0d]);
    }

    [Theory]
    [InlineData(HorizontalAlignment.Left, 0d, 80d)]
    [InlineData(HorizontalAlignment.Center, 110d, 80d)]
    [InlineData(HorizontalAlignment.Right, 220d, 80d)]
    [InlineData(HorizontalAlignment.Stretch, 0d, 300d)]
    public void Vertical_Navigation_Honors_Horizontal_Content_Alignment(
        HorizontalAlignment alignment,
        double expectedX,
        double expectedWidth)
    {
        var panel = new Desktop.Controls.StepsPanel
        {
            Type                       = Desktop.Controls.StepsType.Navigation,
            Orientation                = Orientation.Vertical,
            HorizontalContentAlignment = alignment
        };
        panel.Children.Add(new FixedSizeControl(40, 20));
        panel.Children.Add(new FixedSizeControl(80, 30));
        panel.Children.Add(new FixedSizeControl(20, 10));

        panel.Measure(new Size(300, 100));
        panel.Arrange(new Rect(0, 0, 300, 100));

        panel.Children.Select(child => child.Bounds.X).ShouldBe([expectedX, expectedX, expectedX]);
        panel.Children.Select(child => child.Bounds.Width).ShouldBe([expectedWidth, expectedWidth, expectedWidth]);
    }

    [Fact]
    public void Horizontal_Inline_Offset_Reserves_Leading_Cells()
    {
        var panel = new Desktop.Controls.StepsPanel
        {
            Type           = Desktop.Controls.StepsType.Inline,
            Orientation    = Orientation.Horizontal,
            TitlePlacement = Orientation.Vertical,
            Offset         = 2
        };
        panel.Children.Add(new FixedSizeControl(40, 24));
        panel.Children.Add(new FixedSizeControl(40, 24));
        panel.Children.Add(new FixedSizeControl(40, 24));

        panel.Measure(new Size(500, 100));
        panel.Arrange(new Rect(0, 0, 500, 100));

        panel.Children.Select(child => child.Bounds.X).ShouldBe([200d, 300d, 400d]);
        panel.Children.Select(child => child.Bounds.Width).ShouldBe([100d, 100d, 100d]);
    }

    [Fact]
    public void Vertical_Stacks_Items_At_Their_Desired_Heights()
    {
        var panel = new Desktop.Controls.StepsPanel
        {
            Orientation = Orientation.Vertical
        };
        panel.Children.Add(new FixedSizeControl(40, 20));
        panel.Children.Add(new FixedSizeControl(80, 30));
        panel.Children.Add(new FixedSizeControl(20, 10));

        panel.Measure(new Size(300, 100));
        panel.Arrange(new Rect(0, 0, 300, 100));

        panel.Children.Select(child => child.Bounds.Y).ShouldBe([0d, 20d, 50d]);
        panel.Children.Select(child => child.Bounds.Width).ShouldBe([300d, 300d, 300d]);
    }

    [Fact]
    public void Runtime_Type_And_Orientation_Changes_Rearrange_Items()
    {
        var panel = new Desktop.Controls.StepsPanel
        {
            Type = Desktop.Controls.StepsType.Default,
            Orientation = Orientation.Horizontal
        };
        panel.Children.Add(new FixedSizeControl(40, 20));
        panel.Children.Add(new FixedSizeControl(80, 30));

        panel.Measure(new Size(300, 100));
        panel.Arrange(new Rect(0, 0, 300, 100));
        panel.Children.Select(child => child.Bounds.Width).ShouldBe([220d, 80d]);

        panel.Type = Desktop.Controls.StepsType.Navigation;
        panel.Measure(new Size(300, 100));
        panel.Arrange(new Rect(0, 0, 300, 100));
        panel.Children.Select(child => child.Bounds.Width).ShouldBe([150d, 150d]);

        panel.Orientation = Orientation.Vertical;
        panel.Measure(new Size(300, 100));
        panel.Arrange(new Rect(0, 0, 300, 100));
        panel.Children.Select(child => child.Bounds.Y).ShouldBe([0d, 20d]);
        panel.Children.Select(child => child.Bounds.X).ShouldBe([110d, 110d]);
        panel.Children.Select(child => child.Bounds.Width).ShouldBe([80d, 80d]);
    }

    [Fact]
    public void Horizontal_Default_Shrinks_Items_Proportionally_When_Width_Is_Insufficient()
    {
        var panel = new Desktop.Controls.StepsPanel
        {
            Type        = Desktop.Controls.StepsType.Default,
            Orientation = Orientation.Horizontal
        };
        panel.Children.Add(new FixedSizeControl(100, 24));
        panel.Children.Add(new FixedSizeControl(200, 24));
        panel.Children.Add(new FixedSizeControl(100, 24));

        panel.Measure(new Size(250, 100));
        panel.DesiredSize.Width.ShouldBe(250d);

        panel.Arrange(new Rect(0, 0, 250, 100));

        panel.Children.Select(child => child.Bounds.Width).ShouldBe([62.5d, 125d, 62.5d]);
        panel.Children.Select(child => child.Bounds.X).ShouldBe([0d, 62.5d, 187.5d]);
    }

    [Fact]
    public void Horizontal_Default_Single_Item_Shrinks_To_Available_Width_When_Narrow()
    {
        var panel = new Desktop.Controls.StepsPanel
        {
            Type        = Desktop.Controls.StepsType.Default,
            Orientation = Orientation.Horizontal
        };
        panel.Children.Add(new FixedSizeControl(60, 24));

        panel.Measure(new Size(40, 100));
        panel.Arrange(new Rect(0, 0, 40, 100));

        panel.Children.Single().Bounds.Width.ShouldBe(40d);
        panel.Children.Single().Bounds.X.ShouldBe(0d);
    }

    [Fact]
    public void Horizontal_Default_Single_Item_Keeps_Content_Width_When_Wide()
    {
        var panel = new Desktop.Controls.StepsPanel
        {
            Type        = Desktop.Controls.StepsType.Default,
            Orientation = Orientation.Horizontal
        };
        panel.Children.Add(new FixedSizeControl(60, 24));

        panel.Measure(new Size(300, 100));
        panel.Arrange(new Rect(0, 0, 300, 100));

        panel.Children.Single().Bounds.Width.ShouldBe(60d);
        panel.Children.Single().Bounds.X.ShouldBe(0d);
    }

    [Fact]
    public void Horizontal_Default_Respects_MinItemWidth_Floor()
    {
        var panel = new Desktop.Controls.StepsPanel
        {
            Type         = Desktop.Controls.StepsType.Default,
            Orientation  = Orientation.Horizontal,
            MinItemWidth = 30
        };
        panel.Children.Add(new FixedSizeControl(40, 24));
        panel.Children.Add(new FixedSizeControl(200, 24));
        panel.Children.Add(new FixedSizeControl(100, 24));

        panel.Measure(new Size(100, 100));
        panel.Arrange(new Rect(0, 0, 100, 100));

        panel.Children.Select(child => child.Bounds.Width).ShouldBe([30d, 40d, 30d]);
        panel.Children.Select(child => child.Bounds.X).ShouldBe([0d, 30d, 70d]);
    }

    [Fact]
    public void Horizontal_Default_Overflows_When_MinItemWidth_Exceeds_Available_Share()
    {
        var panel = new Desktop.Controls.StepsPanel
        {
            Type         = Desktop.Controls.StepsType.Default,
            Orientation  = Orientation.Horizontal,
            MinItemWidth = 30
        };
        panel.Children.Add(new FixedSizeControl(100, 24));
        panel.Children.Add(new FixedSizeControl(200, 24));
        panel.Children.Add(new FixedSizeControl(100, 24));

        panel.Measure(new Size(60, 100));
        panel.Arrange(new Rect(0, 0, 60, 100));

        panel.Children.Select(child => child.Bounds.Width).ShouldBe([30d, 30d, 30d]);
        panel.Children.Select(child => child.Bounds.X).ShouldBe([0d, 30d, 60d]);
    }

    [Fact]
    public void Horizontal_Navigation_Respects_MinItemWidth_Floor()
    {
        var panel = new Desktop.Controls.StepsPanel
        {
            Type         = Desktop.Controls.StepsType.Navigation,
            Orientation  = Orientation.Horizontal,
            MinItemWidth = 30
        };
        panel.Children.Add(new FixedSizeControl(40, 24));
        panel.Children.Add(new FixedSizeControl(80, 24));
        panel.Children.Add(new FixedSizeControl(20, 24));

        panel.Measure(new Size(60, 100));
        panel.Arrange(new Rect(0, 0, 60, 100));

        panel.Children.Select(child => child.Bounds.Width).ShouldBe([30d, 30d, 30d]);
        panel.Children.Select(child => child.Bounds.X).ShouldBe([0d, 30d, 60d]);
    }

    [Fact]
    public void Horizontal_TitleVertical_Respects_MinItemWidth_Floor()
    {
        var panel = new Desktop.Controls.StepsPanel
        {
            Type           = Desktop.Controls.StepsType.Inline,
            Orientation    = Orientation.Horizontal,
            TitlePlacement = Orientation.Vertical,
            MinItemWidth   = 30
        };
        panel.Children.Add(new FixedSizeControl(40, 24));
        panel.Children.Add(new FixedSizeControl(40, 24));
        panel.Children.Add(new FixedSizeControl(40, 24));

        panel.Measure(new Size(60, 100));
        panel.Arrange(new Rect(0, 0, 60, 100));

        panel.Children.Select(child => child.Bounds.Width).ShouldBe([30d, 30d, 30d]);
        panel.Children.Select(child => child.Bounds.X).ShouldBe([0d, 30d, 60d]);
    }

    private sealed class FixedSizeControl : Avalonia.Controls.Control
    {
        private readonly double _height;
        private readonly double _width;

        public FixedSizeControl(double width, double height)
        {
            _width             = width;
            _height            = height;
            UseLayoutRounding  = false;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return new Size(_width, _height);
        }
    }
}
