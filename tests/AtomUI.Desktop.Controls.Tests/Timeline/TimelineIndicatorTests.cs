using System.Reflection;
using AtomUI.Controls.Commons;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Timeline;

public class TimelineIndicatorTests
{
    private const string IconPresentPseudoClass = ":icon-present";

    static TimelineIndicatorTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Template_Exposes_Rail_Dot_IconHost_And_IconPresenter()
    {
        var indicator = CreateIndicator(Orientation.Vertical, isFirst: false, isLast: false);

        ShowIndicator(indicator, 20, 100, () =>
        {
            GetPart(indicator, "PART_Rail").ShouldNotBeNull();
            GetPart(indicator, "PART_Dot").ShouldNotBeNull();
            GetPart(indicator, "PART_IconHost").ShouldNotBeNull();
            GetPart(indicator, "PART_IconPresenter").ShouldNotBeNull();
        });
    }

    [Fact]
    public void Vertical_Middle_Item_Rail_Segments_From_Below_The_Node_To_The_Item_End()
    {
        var indicator = CreateIndicator(Orientation.Vertical, isFirst: false, isLast: false);

        ShowIndicator(indicator, 20, 100, () =>
        {
            var rail = (Border)GetPart(indicator, "PART_Rail");
            var dot  = (Border)GetPart(indicator, "PART_Dot");

            rail.Bounds.ShouldBe(new Rect(9, 14, 2, 90));
            dot.Bounds.ShouldBe(new Rect(5, 4, 10, 10));

            AssertRing(dot);
            AssertDotMaskOn(indicator, dot);
        });
    }

    [Fact]
    public void Vertical_First_Item_Rail_Segments_From_Below_The_Node()
    {
        var indicator = CreateIndicator(Orientation.Vertical, isFirst: true, isLast: false);

        ShowIndicator(indicator, 20, 100, () =>
        {
            var rail = (Border)GetPart(indicator, "PART_Rail");
            var dot  = (Border)GetPart(indicator, "PART_Dot");

            rail.Bounds.ShouldBe(new Rect(9, 14, 2, 90));
            dot.Bounds.ShouldBe(new Rect(5, 4, 10, 10));
        });
    }

    [Fact]
    public void Vertical_Last_Item_Rail_Collapses_To_Zero_Extent()
    {
        var indicator = CreateIndicator(Orientation.Vertical, isFirst: false, isLast: true);

        ShowIndicator(indicator, 20, 100, () =>
        {
            var rail = (Border)GetPart(indicator, "PART_Rail");
            var dot  = (Border)GetPart(indicator, "PART_Dot");

            rail.Bounds.ShouldBe(new Rect(9, 14, 2, 0));
            dot.Bounds.ShouldBe(new Rect(5, 4, 10, 10));
        });
    }

    [Fact]
    public void Vertical_Single_Item_Rail_Collapses_To_Zero_Extent()
    {
        var indicator = CreateIndicator(Orientation.Vertical, isFirst: true, isLast: true);

        ShowIndicator(indicator, 20, 100, () =>
        {
            var rail = (Border)GetPart(indicator, "PART_Rail");
            var dot  = (Border)GetPart(indicator, "PART_Dot");

            rail.Bounds.ShouldBe(new Rect(9, 14, 2, 0));
            dot.Bounds.ShouldBe(new Rect(5, 4, 10, 10));
        });
    }

    [Fact]
    public void Horizontal_Middle_Item_Rail_Spans_Full_Extent_At_The_Axis()
    {
        var indicator = CreateIndicator(Orientation.Horizontal, isFirst: false, isLast: false);

        ShowIndicator(indicator, 100, 20, () =>
        {
            var rail = (Border)GetPart(indicator, "PART_Rail");
            var dot  = (Border)GetPart(indicator, "PART_Dot");

            rail.Bounds.ShouldBe(new Rect(0, 9, 100, 2));
            dot.Bounds.ShouldBe(new Rect(45, 5, 10, 10));

            AssertRing(dot);
        });
    }

    [Theory]
    [InlineData(true, 50, 50)]
    [InlineData(false, 0, 50)]
    public void Horizontal_Edge_Items_Clip_Rail_At_The_Node_Center(
        bool isFirst,
        double railX,
        double railWidth)
    {
        var indicator = CreateIndicator(Orientation.Horizontal, isFirst, isLast: !isFirst);

        ShowIndicator(indicator, 100, 20, () =>
        {
            GetPart(indicator, "PART_Rail").Bounds.ShouldBe(new Rect(railX, 9, railWidth, 2));
        });
    }

    [Fact]
    public void Icon_Mode_Sets_Pseudo_Class_Masks_Rail_And_Hides_Dot()
    {
        var indicator = CreateIndicator(
            Orientation.Vertical,
            isFirst: true,
            isLast: false,
            icon: CreateIcon());

        ShowIndicator(indicator, 20, 100, () =>
        {
            indicator.Classes.Contains(IconPresentPseudoClass).ShouldBeTrue();

            var rail = (Border)GetPart(indicator, "PART_Rail");
            var dot  = (Border)GetPart(indicator, "PART_Dot");
            var host = (Border)GetPart(indicator, "PART_IconHost");

            dot.IsVisible.ShouldBeFalse();
            host.IsVisible.ShouldBeTrue();
            host.Background.ShouldNotBeNull();

            rail.Bounds.X.ShouldBe(9);
            rail.Bounds.Y.ShouldBe(host.Bounds.Bottom);
            rail.Bounds.Bottom.ShouldBe(104);
        });
    }

    [Fact]
    public void Icon_Mode_Single_Item_Rail_Collapses_At_The_Icon_Center()
    {
        var indicator = CreateIndicator(
            Orientation.Vertical,
            isFirst: true,
            isLast: true,
            icon: CreateIcon());

        ShowIndicator(indicator, 20, 100, () =>
        {
            var rail = (Border)GetPart(indicator, "PART_Rail");
            var host = (Border)GetPart(indicator, "PART_IconHost");

            rail.Bounds.Y.ShouldBe(host.Bounds.Bottom);
            rail.Bounds.Height.ShouldBe(0);
        });
    }

    private static void AssertDotMaskOn(Control indicator, Border dot)
    {
        indicator.Classes.Contains(IconPresentPseudoClass).ShouldBeFalse();
        dot.IsVisible.ShouldBeTrue();
        GetPart(indicator, "PART_IconHost").IsVisible.ShouldBeFalse();
    }

    private static void AssertRing(Border dot)
    {
        dot.CornerRadius.ShouldBe(new CornerRadius(5));
        dot.BorderThickness.ShouldBe(new Thickness(2));
        dot.Background.ShouldNotBeNull();
        GetBrushColor(dot.BorderBrush).ShouldBe(Colors.Red);
    }

    private static Color GetBrushColor(IBrush? brush)
    {
        return ((ISolidColorBrush)brush!).Color;
    }

    private static PathIcon CreateIcon()
    {
        return new PathIcon { Data = Geometry.Parse("M0,0 L10,0 L10,10 Z") };
    }

    private static Control CreateIndicator(
        Orientation orientation,
        bool isFirst,
        bool isLast,
        PathIcon? icon = null)
    {
        var indicatorType = typeof(AbstractTimeline).Assembly.GetType(
            "AtomUI.Controls.Commons.TimelineIndicator",
            throwOnError: true)!;
        var indicator = (Control)Activator.CreateInstance(indicatorType)!;
        SetProperty(indicator, "Orientation", orientation);
        SetProperty(indicator, "IndicatorColor", Brushes.Red);
        SetProperty(indicator, "DefaultIndicatorColor", Brushes.Red);
        SetProperty(indicator, "IndicatorTailColor", Brushes.Red);
        SetProperty(indicator, "IndicatorTailWidth", 2d);
        SetProperty(indicator, "IndicatorDotBorderWidth", 2d);
        SetProperty(indicator, "IndicatorDotSize", 8d);
        SetProperty(indicator, "IsFirst", isFirst);
        SetProperty(indicator, "IsLast", isLast);
        if (icon is not null)
        {
            SetProperty(indicator, "IndicatorIcon", icon);
        }

        return indicator;
    }

    private static void ShowIndicator(Control indicator, double width, double height, Action assertion)
    {
        indicator.Width  = width;
        indicator.Height = height;
        var window = new AvaloniaWindow
        {
            Width   = 760,
            Height  = 320,
            Content = new Canvas
            {
                Children = { indicator }
            }
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            // The theme computes IndicatorMinHeight from RelativeLineHeight on apply;
            // pin it locally so geometry assertions stay deterministic.
            SetProperty(indicator, "IndicatorMinHeight", 20d);
            Dispatcher.UIThread.RunJobs();
            assertion();
        }
        finally
        {
            window.Close();
        }
    }

    private static Control GetPart(Control indicator, string name)
    {
        return indicator.GetVisualDescendants()
                        .OfType<Control>()
                        .Single(part => part.Name == name);
    }

    private static void SetProperty(Control control, string propertyName, object value)
    {
        var property = control.GetType().GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        property.ShouldNotBeNull().SetValue(control, value);
    }
}
