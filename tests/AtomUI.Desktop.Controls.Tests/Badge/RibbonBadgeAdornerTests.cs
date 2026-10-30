using AtomUI.Controls.Commons;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Badge;

public class RibbonBadgeAdornerTests
{
    static RibbonBadgeAdornerTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void CountBadge_Adorner_Uses_Native_Adorner_Layer()
    {
        var countBadge = new AtomUI.Desktop.Controls.CountBadge
        {
            Count           = 5,
            DecoratedTarget = new Border
            {
                Width  = 48,
                Height = 48
            }
        };
        using var context = ShowInAdornerHost(countBadge);

        var adorner = FindNativeAdorner(countBadge);

        adorner.ShouldNotBeNull();
        AdornerLayer.GetIsClipEnabled(adorner).ShouldBeTrue();
    }

    [Fact]
    public void DotBadge_Adorner_Uses_Native_Adorner_Layer()
    {
        var dotBadge = new AtomUI.Desktop.Controls.DotBadge
        {
            Status          = AtomUI.Controls.Commons.DotBadgeStatus.Success,
            DecoratedTarget = new Border
            {
                Width  = 48,
                Height = 48
            }
        };
        using var context = ShowInAdornerHost(dotBadge);

        var adorner = FindNativeAdorner(dotBadge);

        adorner.ShouldNotBeNull();
        AdornerLayer.GetIsClipEnabled(adorner).ShouldBeTrue();
    }

    [Fact]
    public void RibbonBadge_Target_Mode_Uses_Inline_Visual_Tree()
    {
        var ribbonBadge = new AtomUI.Desktop.Controls.RibbonBadge
        {
            Text            = "v6.0.5",
            DecoratedTarget = new Border
            {
                Width  = 160,
                Height = 80
            }
        };
        using var context = ShowInAdornerHost(ribbonBadge, width: 240, height: 160);

        var nativeAdorner = FindNativeAdorner(ribbonBadge);
        var inlineAdornerCount = ribbonBadge.GetVisualDescendants()
                                            .Count(visual => visual.GetType().Name == "RibbonBadgeAdorner");

        nativeAdorner.ShouldBeNull(
            "RibbonBadge should behave like Ant Design Badge.Ribbon: the ribbon belongs to the decorated wrapper tree, not a window-level adorner.");
        inlineAdornerCount.ShouldBe(1);
    }

    [Fact]
    public void RibbonBadge_Target_Mode_Keeps_Target_Visible_When_Badge_Is_Hidden()
    {
        var target = new Border
        {
            Width  = 160,
            Height = 80
        };
        var ribbonBadge = new AtomUI.Desktop.Controls.RibbonBadge
        {
            Text            = "v6.0.5",
            BadgeIsVisible  = false,
            DecoratedTarget = target
        };
        using var context = ShowInAdornerHost(ribbonBadge, width: 240, height: 160);

        var hasTarget = ribbonBadge.GetVisualDescendants().Any(visual => ReferenceEquals(visual, target));
        var inlineAdornerCount = ribbonBadge.GetVisualDescendants()
                                            .Count(visual => visual.GetType().Name == "RibbonBadgeAdorner");

        hasTarget.ShouldBeTrue("hiding a RibbonBadge should hide only the ribbon overlay, not the decorated content.");
        inlineAdornerCount.ShouldBe(0);
    }

    [Fact]
    public void RibbonBadge_Target_Mode_Applies_Offset_To_Ribbon_Position()
    {
        var defaultLabelX = GetRibbonLabelX(new Point());
        var insetLabelX   = GetRibbonLabelX(new Point(-8, 0));

        insetLabelX.ShouldBe(defaultLabelX - 8, 0.001);
    }

    [Fact]
    public void RibbonBadge_Target_Mode_Anchors_End_Ribbon_To_Final_Target_Right_Edge_In_Stretch_Layout()
    {
        var ribbonBadge = new AtomUI.Desktop.Controls.RibbonBadge
        {
            Text = "Hippies",
            DecoratedTarget = new Border
            {
                Padding         = new Thickness(10, 0),
                BorderThickness = new Thickness(1),
                Child = new StackPanel
                {
                    Children =
                    {
                        new TextBlock { Text = "Pushes open the window" },
                        new TextBlock { Text = "and raises the spyglass." }
                    }
                }
            }
        };
        var host = new StackPanel
        {
            Margin   = new Thickness(20, 0),
            Children = { ribbonBadge }
        };
        using var context = ShowInAdornerHost(host, width: 920, height: 180);

        var label = context.VisualLayerManager
                           .GetVisualDescendants()
                           .OfType<TextBlock>()
                           .Single(textBlock => textBlock.Text == "Hippies");

        label.Bounds.Right.ShouldBeGreaterThan(ribbonBadge.Bounds.Width,
            "the end ribbon should overhang the decorated target's final right edge instead of stopping inside it.");
    }

    [Fact]
    public void RibbonBadge_Standalone_Start_Fold_Stays_Inside_Desired_Visual_Bounds()
    {
        var adorner = CreateMeasuredRibbonAdorner(RibbonBadgePlacement.Start, isAdornerMode: false);
        using var context = ShowInAdornerHost(adorner, width: 160, height: 80);

        var foldBounds = GetFoldBounds(adorner);

        foldBounds.Left.ShouldBeGreaterThanOrEqualTo(-0.001,
            "standalone start ribbons must reserve their complete fold instead of drawing it outside the desired visual bounds.");
        foldBounds.Right.ShouldBeLessThanOrEqualTo(adorner.DesiredSize.Width + 0.001);
    }

    [Fact]
    public void RibbonBadge_Fold_Geometry_Uses_Updated_Corner_Transform()
    {
        var adorner = CreateMeasuredRibbonAdorner(RibbonBadgePlacement.End, isAdornerMode: true);
        using var context = ShowInAdornerHost(adorner, width: 160, height: 80);

        adorner.BadgeRibbonCornerTransform = new ScaleTransform(1, 0.25).ToImmutable();
        Dispatcher.UIThread.RunJobs();

        var foldBounds = GetFoldBounds(adorner);

        foldBounds.Height.ShouldBe(2, 0.001,
            "changing the corner transform must rebuild the fold geometry instead of reusing the previous cached triangle.");
    }

    private static double GetRibbonLabelX(Point offset)
    {
        var ribbonBadge = new AtomUI.Desktop.Controls.RibbonBadge
        {
            Text   = "v6.0.5",
            Offset = offset,
            DecoratedTarget = new Border
            {
                Width  = 160,
                Height = 80
            }
        };
        using var context = ShowInAdornerHost(ribbonBadge, width: 240, height: 160);

        return context.VisualLayerManager
                      .GetVisualDescendants()
                      .OfType<TextBlock>()
                      .Single(textBlock => textBlock.Text == "v6.0.5")
                      .Bounds
                      .X;
    }

    private static RibbonBadgeAdorner CreateMeasuredRibbonAdorner(
        RibbonBadgePlacement placement,
        bool isAdornerMode)
    {
        return new RibbonBadgeAdorner
        {
            Text                            = "Ribbon",
            Placement                       = placement,
            IsAdornerMode                   = isAdornerMode,
            BadgeRibbonOffset               = new Point(8, 8),
            BadgeRibbonCornerTransform      = new ScaleTransform(1, 0.75).ToImmutable(),
            BadgeRibbonCornerDarkenAmount   = 15,
            RibbonColor                     = new SolidColorBrush(Color.Parse("#1677ff"))
        };
    }

    private static Rect GetFoldBounds(RibbonBadgeAdorner adorner)
    {
        var drawingGroup = RenderToDrawingGroup(adorner);
        return EnumerateGeometryBounds(drawingGroup)
            .Where(bounds => bounds.Width > 0 && bounds.Height > 0)
            .OrderBy(bounds => bounds.Height)
            .First();
    }

    private static DrawingGroup RenderToDrawingGroup(Control control)
    {
        var drawingGroup = new DrawingGroup();
        using var context = drawingGroup.Open();
        control.Render(context);
        return drawingGroup;
    }

    private static IEnumerable<Rect> EnumerateGeometryBounds(Drawing drawing)
    {
        if (drawing is GeometryDrawing geometryDrawing)
        {
            var geometry = geometryDrawing.Geometry;
            if (geometry is not null)
            {
                yield return geometry.Bounds;
            }
        }
        else if (drawing is DrawingGroup drawingGroup)
        {
            foreach (var child in drawingGroup.Children.SelectMany(EnumerateGeometryBounds))
            {
                yield return child;
            }
        }
    }

    private static Control? FindNativeAdorner(Control badge)
    {
        var adornerLayer = AdornerLayer.GetAdornerLayer(badge);
        adornerLayer.ShouldNotBeNull();

        return adornerLayer.Children.SingleOrDefault(child =>
            ReferenceEquals(AdornerLayer.GetAdornedElement(child), badge));
    }

    private static WindowContext ShowInAdornerHost(Control control, double width = 120, double height = 100)
    {
        var visualLayerManager = new VisualLayerManager
        {
            EnableAdornerLayer = true,
            Child              = control
        };
        var window = new AvaloniaWindow
        {
            Width   = width,
            Height  = height,
            Content = visualLayerManager
        };

        window.Show();
        Dispatcher.UIThread.RunJobs();
        return new WindowContext(window, visualLayerManager);
    }

    private sealed class WindowContext : IDisposable
    {
        private readonly AvaloniaWindow _window;

        public WindowContext(AvaloniaWindow window, VisualLayerManager visualLayerManager)
        {
            _window            = window;
            VisualLayerManager = visualLayerManager;
        }

        public VisualLayerManager VisualLayerManager { get; }

        public void Dispose()
        {
            _window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
