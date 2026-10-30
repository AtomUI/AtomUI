using AtomUI.Controls;
using AtomUI.Controls.Commons;
using AtomUI.Controls.Primitives;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.FloatButton;

public class BackTopFloatButtonProgressTests
{
    static BackTopFloatButtonProgressTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    private static AtomUI.Desktop.Controls.ScrollViewer CreateScrollContent(
        BackTopFloatButton backTop, double contentHeight, out Avalonia.Controls.Window window)
    {
        var panel = new Panel { Height = contentHeight };
        panel.Children.Add(new Avalonia.Controls.TextBlock { Text = "content" });
        panel.Children.Add(backTop);
        var scrollViewer = new AtomUI.Desktop.Controls.ScrollViewer
        {
            Width  = 320,
            Height = 240,
            Content = panel
        };
        backTop.Target = scrollViewer;
        window = new Avalonia.Controls.Window { Width = 320, Height = 240, Content = scrollViewer };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return scrollViewer;
    }

    [Fact]
    public void BackTop_IsShowProgress_Should_Default_To_False()
    {
        var backTop = new BackTopFloatButton();
        backTop.IsShowProgress.ShouldBeFalse();
    }

    [Fact]
    public void BackTop_ScrollProgress_Should_Be_Zero_When_Content_Shorter_Than_Viewport()
    {
        var backTop = new BackTopFloatButton { VisibilityHeight = 0 };
        var scrollViewer = CreateScrollContent(backTop, 100, out var window);
        try
        {
            scrollViewer.Offset = new Vector(0, 50);
            Dispatcher.UIThread.RunJobs();
            backTop.ScrollProgress.ShouldBe(0d);
        }
        finally
        {
            window.Close();
        }
    }

    [Theory]
    [InlineData(0d)]    // 名义 0 / 760 = 0
    [InlineData(190d)]  // 名义 190 / 760 = 0.25
    [InlineData(380d)]  // 名义 380 / 760 = 0.5
    [InlineData(760d)]  // 名义 760 / 760 = 1
    [InlineData(9999d)] // 超出范围夹取到 1
    public void BackTop_ScrollProgress_Should_Follow_Scroll_Offset(double offset)
    {
        var backTop = new BackTopFloatButton { VisibilityHeight = 0 };
        // 名义 viewport 240 + content 1000 => maxScroll = 760；实际 maxScroll 运行时读取，
        // 避免布局亚像素差异导致的期望值误报
        var scrollViewer = CreateScrollContent(backTop, 1000, out var window);
        try
        {
            scrollViewer.Offset = new Vector(0, offset);
            Dispatcher.UIThread.RunJobs();

            var maxScroll = Math.Max(
                scrollViewer.Extent.Height - scrollViewer.Viewport.Height, 0d);
            var expected = maxScroll > 0d
                ? Math.Clamp(offset / maxScroll, 0d, 1d)
                : 0d;
            backTop.ScrollProgress.ShouldBe(expected, 0.001d);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void BackTop_ScrollProgress_Should_Sync_On_Loaded()
    {
        var backTop = new BackTopFloatButton { VisibilityHeight = 0 };
        var panel = new Panel { Height = 1000 };
        panel.Children.Add(backTop);
        var scrollViewer = new AtomUI.Desktop.Controls.ScrollViewer
        {
            Width  = 320,
            Height = 240,
            Content = panel
        };
        backTop.Target = scrollViewer;
        var window = new Avalonia.Controls.Window { Width = 320, Height = 240, Content = scrollViewer };
        try
        {
            // 挂载前预设 Offset=190（名义 maxScroll=760 → 25%），由 OnLoaded 同步
            scrollViewer.Offset = new Vector(0, 190);
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var maxScroll = Math.Max(
                scrollViewer.Extent.Height - scrollViewer.Viewport.Height, 0d);
            backTop.ScrollProgress.ShouldBe(190d / maxScroll, 0.001d);
        }
        finally
        {
            window.Close();
        }
    }

    private static BackTopFloatButton CreateShownButton(FloatButtonShape shape,
                                                        out Avalonia.Controls.Window window)
    {
        var backTop = new BackTopFloatButton
        {
            Shape            = shape,
            IsShowProgress   = true,
            VisibilityHeight = 0,
            IsMotionEnabled  = false
        };
        var panel = new Panel { Height = 1000 };
        panel.Children.Add(backTop);
        var scrollViewer = new AtomUI.Desktop.Controls.ScrollViewer
        {
            Width   = 320,
            Height  = 240,
            Content = panel
        };
        backTop.Target = scrollViewer;
        window = new Avalonia.Controls.Window { Width = 320, Height = 240, Content = scrollViewer };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return backTop;
    }

    [Fact]
    public void BackTop_ProgressRing_Hidden_By_Default()
    {
        // IsShowProgress 保持默认 false（Task 1 已断言默认值），环部件应不可见
        var backTop = new BackTopFloatButton { VisibilityHeight = 0, IsMotionEnabled = false };
        CreateScrollContent(backTop, 1000, out var window);
        try
        {
            var ring = backTop.GetVisualDescendants().OfType<BackTopProgressRing>().Single();
            ring.IsVisible.ShouldBeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [Theory]
    [InlineData(FloatButtonShape.Circle)]
    [InlineData(FloatButtonShape.Square)]
    public void BackTop_ProgressRing_Shown_When_IsShowProgress(FloatButtonShape shape)
    {
        var backTop = CreateShownButton(shape, out var window);
        try
        {
            var ring = backTop.GetVisualDescendants().OfType<BackTopProgressRing>().Single();
            ring.IsVisible.ShouldBeTrue();
            ring.IndicatorBrush.ShouldNotBeNull();
            ring.TrackBrush.ShouldNotBeNull();
            ring.StrokeThickness.ShouldBe(2d); // LineWidthBold
            backTop.Target!.Offset = new Vector(0, 380);
            Dispatcher.UIThread.RunJobs();
            // 名义 viewport 240 + content 1000 => maxScroll = 760；实际 maxScroll 运行时读取，
            // 避免布局亚像素差异导致的期望值误报
            var scrollViewer = backTop.Target;
            var maxScroll    = scrollViewer.Extent.Height - scrollViewer.Viewport.Height;
            ring.Progress.ShouldBe(380d / maxScroll, 0.001d);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void BackTop_ProgressRing_CornerRadius_Should_Follow_Shape()
    {
        var circle = CreateShownButton(FloatButtonShape.Circle, out var window1);
        var square = CreateShownButton(FloatButtonShape.Square, out var window2);
        try
        {
            var circleRing = circle.GetVisualDescendants().OfType<BackTopProgressRing>().Single();
            var squareRing = square.GetVisualDescendants().OfType<BackTopProgressRing>().Single();
            circleRing.CornerRadius.TopLeft.ShouldBe(circle.Bounds.Height / 2, 0.01d);
            squareRing.CornerRadius.TopLeft.ShouldBeGreaterThan(0d);
            squareRing.CornerRadius.TopLeft.ShouldBeLessThan(square.Bounds.Height / 2);
        }
        finally
        {
            window1.Close();
            window2.Close();
        }
    }

    [Fact]
    public void BackTopHost_IsShowProgress_Should_Project_To_Overlay_Button()
    {
        var host = new BackTopFloatButtonHost
        {
            IsShowProgress   = true,
            IsMotionEnabled  = false,
            VisibilityHeight = 0
        };
        var panel = new ScopeAwareOverlayLayerPanel { Width = 320, Height = 240 };
        panel.Children.Add(host);
        var window = new Avalonia.Controls.Window { Width = 320, Height = 240, Content = panel };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var overlayLayer = AtomUI.Controls.Primitives.ScopeAwareOverlayLayer.FindLayer(panel);
            overlayLayer.ShouldNotBeNull();
            var overlayButton = overlayLayer.GetVisualDescendants()
                .OfType<BackTopFloatButton>()
                .Single();
            overlayButton.IsShowProgress.ShouldBeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void BackTopHost_Two_Hosts_Should_Not_Overlap_When_Offset_Configured()
    {
        // Gallery 的 BackTop 进度环示例并排展示圆形与方形按钮：Host 不参与布局
        // （desired=0,0），投影位置由 Placement + FloatOffset 决定，因此两个 Host
        // 若都用默认偏移会完全重合（阴影叠加、前者被遮挡）。此测试锁定该示例的
        // 偏移契约：Square 需与 Circle 分离且位于其左侧（对齐 antd demo 排列）。
        var circleHost = new BackTopFloatButtonHost
        {
            ButtonType = FloatButtonType.Default, IsShowProgress = true, VisibilityHeight = 0
        };
        var squareHost = new BackTopFloatButtonHost
        {
            ButtonType = FloatButtonType.Default, Shape = FloatButtonShape.Square,
            IsShowProgress = true, VisibilityHeight = 0, FloatOffsetX = 72
        };
        var panel = new Panel { Height = 1000 };
        panel.Children.Add(circleHost);
        panel.Children.Add(squareHost);
        var scrollViewer = new AtomUI.Desktop.Controls.ScrollViewer
        {
            Width   = 600,
            Height  = 300,
            Content = new Border { Child = panel }
        };
        var window = new Avalonia.Controls.Window { Width = 600, Height = 300, Content = scrollViewer };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var buttons = window.GetVisualDescendants()
                                .OfType<AbstractFloatButton>()
                                .ToList();
            buttons.Count.ShouldBe(2);

            var circle = buttons.Single(b => b.Shape == FloatButtonShape.Circle);
            var square = buttons.Single(b => b.Shape == FloatButtonShape.Square);

            var circleLeft = Canvas.GetLeft(circle);
            var squareLeft = Canvas.GetLeft(square);
            var circleTop  = Canvas.GetTop(circle);
            var squareTop  = Canvas.GetTop(square);

            squareTop.ShouldBe(circleTop, 0.01d);          // 同高
            squareLeft.ShouldBeLessThan(circleLeft);       // 方形在左
            (circleLeft - squareLeft).ShouldBeGreaterThanOrEqualTo(circle.Bounds.Width); // 不重合
        }
        finally
        {
            window.Close();
        }
    }

    [Theory]
    [InlineData(40d, 8d)]   // Square：CornerRadius = BorderRadiusLG
    [InlineData(40d, 20d)]  // Circle：CornerRadius = Height / 2
    public void BackTop_Ring_OuterEdge_Should_Match_Button_CornerRadius(double size, double cornerRadius)
    {
        // 环的外缘必须与按钮轮廓完全重合（半径 = 按钮圆角半径），
        // 否则圆角处会露出背景与环之间的缝隙。
        var metrics = BackTopProgressRing.CalculateRingMetrics(new Size(size, size), cornerRadius, 2d);

        metrics.OuterRadius.ShouldBe(cornerRadius, 0.0001d);
        metrics.CenterlineRadius.ShouldBe(cornerRadius - 1d, 0.0001d);
        metrics.InnerRadius.ShouldBe(cornerRadius - 2d, 0.0001d);
        metrics.OuterRect.ShouldBe(new Rect(new Size(size, size)));
    }

    [Theory]
    [InlineData(40d, 8d)]   // Square
    [InlineData(40d, 20d)]  // Circle
    public void BackTop_Ring_Band_Should_Cover_Button_Outline(double size, double cornerRadius)
    {
        // 沿按钮轮廓的四个圆角采样：这些点必须落在环带内（在外边界内、不在内孔里）。
        // 这是圆角缝隙的直接几何判据——缝隙正是「按钮轮廓上未被环带覆盖」的区域。
        // 使用自建圆角矩形包含判据而非 geometry.FillContains：后者在 headless 环境
        // 下对 StreamGeometry 的命中测试不可靠（几何中心点都会被判为外部）。
        const double thickness = 2d;
        var metrics = BackTopProgressRing.CalculateRingMetrics(new Size(size, size), cornerRadius, thickness);

        // 稍向内采样 0.05px，避免恰好落在边界上的浮点歧义。
        // 每个圆角只采样其「向外」的 90° 象限：这些点才真正落在按钮轮廓上。
        var sampleRadius = cornerRadius - 0.05d;
        var cornerSamples = new[]
        {
            (Center: new Point(cornerRadius, cornerRadius), StartAngle: 180d),                  // 左上
            (Center: new Point(size - cornerRadius, cornerRadius), StartAngle: 270d),           // 右上
            (Center: new Point(size - cornerRadius, size - cornerRadius), StartAngle: 0d),      // 右下
            (Center: new Point(cornerRadius, size - cornerRadius), StartAngle: 90d)             // 左下
        };

        foreach (var (cornerCenter, startAngle) in cornerSamples)
        {
            for (var offset = 0d; offset <= 90d; offset += 10d)
            {
                var rad   = MathUtils.Deg2Rad(startAngle + offset);
                var point = new Point(
                    cornerCenter.X + sampleRadius * Math.Cos(rad),
                    cornerCenter.Y + sampleRadius * Math.Sin(rad));

                InsideRoundedRect(metrics.OuterRect, metrics.OuterRadius, point)
                    .ShouldBeTrue($"outline point {point} must be covered by ring outer boundary");
                InsideRoundedRect(metrics.InnerRect, metrics.InnerRadius, point)
                    .ShouldBeFalse($"outline point {point} must not fall inside ring inner hole");
            }
        }
    }

    private static bool InsideRoundedRect(Rect rect, double radius, Point point)
    {
        const double epsilon = 1e-9;
        if (point.X < rect.Left - epsilon || point.X > rect.Right + epsilon ||
            point.Y < rect.Top - epsilon || point.Y > rect.Bottom + epsilon)
        {
            return false;
        }

        radius = Math.Clamp(radius, 0d, Math.Min(rect.Width, rect.Height) / 2);
        var centerX = Math.Clamp(point.X, rect.Left + radius, rect.Right - radius);
        var centerY = Math.Clamp(point.Y, rect.Top + radius, rect.Bottom - radius);
        var inCornerX = point.X < rect.Left + radius || point.X > rect.Right - radius;
        var inCornerY = point.Y < rect.Top + radius || point.Y > rect.Bottom - radius;
        if (inCornerX && inCornerY)
        {
            var dx = point.X - centerX;
            var dy = point.Y - centerY;
            return dx * dx + dy * dy <= radius * radius + epsilon;
        }

        return true;
    }
}
