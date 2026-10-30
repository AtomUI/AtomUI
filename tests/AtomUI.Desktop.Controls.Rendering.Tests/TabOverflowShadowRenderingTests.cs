using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Configuration;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using SkiaSharp;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;
using AvaloniaScrollViewer = Avalonia.Controls.ScrollViewer;

namespace AtomUI.Desktop.Controls.Rendering.Tests;

public class TabOverflowShadowRenderingTests
{
    public static TheoryData<int, Dock, bool, double> Cases
    {
        get
        {
            var cases = new TheoryData<int, Dock, bool, double>();
            for (var owner = 0; owner < 4; owner++)
            foreach (var placement in new[] { Dock.Top, Dock.Bottom, Dock.Left, Dock.Right })
            foreach (var dark in new[] { false, true })
                foreach (var scaling in new[] { 1d, 2d })
                cases.Add(owner, placement, dark, scaling);
            return cases;
        }
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task Outer_Shadow_Touches_Only_The_Overflowing_Viewport_Edges(int kind, Dock placement, bool dark, double scaling)
    {
        var manager = Application.Current!.GetThemeManager().ShouldNotBeNull();
        await manager.ApplyThemeAsync(new ThemeRequest(IThemeManager.DEFAULT_THEME_ID,
            new ThemeConfigBuilder().WithAlgorithms(dark ? ThemeAlgorithm.Dark : ThemeAlgorithm.Default).Build(),
            ThemeTransitionReason.UserRequest), TestContext.Current.CancellationToken);
        manager.CurrentTheme!.Appearance.ShouldBe(dark ? ThemeAppearance.Dark : ThemeAppearance.Light);
        var owner = CreateOwner(kind, placement);
        var window = new AvaloniaWindow
        {
            Width = 568,
            Height = 298,
            RequestedThemeVariant = dark ? ThemeVariant.Dark : ThemeVariant.Light,
            Content = new Border
            {
                Padding = new Thickness(24),
                Background = dark ? new SolidColorBrush(Color.Parse("#141414")) : Brushes.White,
                Child = owner
            }
        };
        window.Show();
        window.SetRenderScaling(scaling);
        try
        {
            Refresh(window);
            var viewer = owner.GetVisualDescendants().OfType<AvaloniaScrollViewer>()
                              .Single(control => control.GetType().Name == "TabScrollViewer");
            var viewport = Find<Panel>(viewer, "ScrollContentViewport");
            var start = Find<Control>(viewer, "PART_ScrollStartEdgeIndicator");
            var end = Find<Control>(viewer, "PART_ScrollEndEdgeIndicator");
            var horizontal = placement is Dock.Top or Dock.Bottom;

            foreach (var size in new[] { new Size(520, 250), new Size(420, 210) })
            {
                owner.Width = size.Width;
                owner.Height = size.Height;
                Refresh(window);
                var maximum = horizontal ? viewer.Extent.Width - viewer.Viewport.Width
                                         : viewer.Extent.Height - viewer.Viewport.Height;
                maximum.ShouldBeGreaterThan(0);
                foreach (var fraction in new[] { 0d, 0.5d, 1d })
                {
                    viewer.Offset = horizontal ? new Vector(maximum * fraction, 0)
                                               : new Vector(0, maximum * fraction);
                    Refresh(window);
                    start.IsVisible.ShouldBe(fraction > 0);
                    end.IsVisible.ShouldBe(fraction < 1);
                    AssertPixels(window, viewport, start, end, horizontal);
                }
            }

            foreach (var nextPlacement in new[] { Dock.Top, Dock.Left, Dock.Bottom, Dock.Right, placement })
            {
                if (owner is BaseTabControl tabs) tabs.TabStripPlacement = nextPlacement;
                else ((BaseTabStrip)owner).TabStripPlacement = nextPlacement;
                Refresh(window);
                var nextHorizontal = nextPlacement is Dock.Top or Dock.Bottom;
                viewer.Offset = nextHorizontal
                    ? new Vector((viewer.Extent.Width - viewer.Viewport.Width) / 2, 0)
                    : new Vector(0, (viewer.Extent.Height - viewer.Viewport.Height) / 2);
                Refresh(window);
                start.IsVisible.ShouldBeTrue();
                end.IsVisible.ShouldBeTrue();
                AssertPixels(window, viewport, start, end, nextHorizontal);
            }

            while (owner.Items.Count > 1) owner.Items.RemoveAt(owner.Items.Count - 1);
            Refresh(window);
            start.IsVisible.ShouldBeFalse();
            end.IsVisible.ShouldBeFalse();
            Find<Control>(viewer, "PART_ScrollMenuIndicator").IsVisible.ShouldBeFalse();
            AssertPixels(window, viewport, start, end, horizontal);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Theory]
    [InlineData(Dock.Top)]
    [InlineData(Dock.Bottom)]
    [InlineData(Dock.Left)]
    [InlineData(Dock.Right)]
    public void Shadow_Follows_Display_And_Ancestor_Scaling_Without_Replacing_Tokens(Dock placement)
    {
        var owner = CreateOwner(1, placement);
        owner.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
        owner.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top;
        var host = new Border { Padding = new Thickness(24), Background = Brushes.White, Child = owner };
        var window = new AvaloniaWindow { Width = 768, Height = 480, Content = host };
        window.Show();
        try
        {
            Refresh(window);
            var viewer = owner.GetVisualDescendants().OfType<AvaloniaScrollViewer>()
                .Single(control => control.GetType().Name == "TabScrollViewer");
            var viewport = Find<Panel>(viewer, "ScrollContentViewport");
            var start = Find<Control>(viewer, "PART_ScrollStartEdgeIndicator");
            var end = Find<Control>(viewer, "PART_ScrollEndEdgeIndicator");
            var token = end.GetValue(Border.BoxShadowProperty);
            var horizontal = placement is Dock.Top or Dock.Bottom;
            viewer.Offset = horizontal
                ? new Vector((viewer.Extent.Width - viewer.Viewport.Width) / 2, 0)
                : new Vector(0, (viewer.Extent.Height - viewer.Viewport.Height) / 2);
            foreach (var scaling in new[] { 1d, 1.25d, 1.5d, 2d, 3d, 1d })
            {
                window.SetRenderScaling(scaling);
                Refresh(window);
                start.IsVisible.ShouldBeTrue();
                end.IsVisible.ShouldBeTrue();
                AssertPixels(window, viewport, start, end, horizontal);
                end.GetValue(Border.BoxShadowProperty).ShouldBe(token);
            }
            owner.RenderTransformOrigin = RelativePoint.TopLeft;
            owner.RenderTransform = new ScaleTransform(1.25, 1.25);
            window.SetRenderScaling(2);
            AssertPixels(window, viewport, start, end, horizontal);

            host.Child = null;
            window.Content = null;
            var nextWindow = new AvaloniaWindow { Width = 768, Height = 480, Content = host };
            nextWindow.Show();
            try
            {
                nextWindow.SetRenderScaling(1.5);
                host.Child = owner;
                Refresh(nextWindow);
                AssertPixels(nextWindow, viewport, start, end, horizontal);
                end.GetValue(Border.BoxShadowProperty).ShouldBe(token);
            }
            finally
            {
                nextWindow.Close();
            }
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    internal static void AssertPixels(AvaloniaWindow window, Panel viewport, Control start, Control end, bool horizontal)
    {
        // Capture the compositor's real Skia frame. RenderTargetBitmap's immediate
        // renderer culls off-viewport casters using Bounds without their BoxShadow.
        using var actual = Capture(window);
        SKBitmap baseline;
        try
        {
            start.SetValue(Border.BoxShadowProperty, default);
            end.SetValue(Border.BoxShadowProperty, default);
            baseline = Capture(window);
        }
        finally
        {
            // Restore the style binding, including its placement/theme updates.
            start.ClearValue(Border.BoxShadowProperty);
            end.ClearValue(Border.BoxShadowProperty);
        }
        using var baselinePixels = baseline;

        var scale = window.RenderScaling;
        var transform = viewport.TransformToVisual(window).ShouldNotBeNull();
        var bounds = new Rect(viewport.Bounds.Size).TransformToAABB(transform);
        // Contrast uses fully covered pixels. The separate leak check includes
        // the partially covered physical pixels touched by a fractional clip edge.
        var clipLeft = (int)Math.Floor(bounds.X * scale);
        var clipTop = (int)Math.Floor(bounds.Y * scale);
        var clipRight = (int)Math.Ceiling(bounds.Right * scale);
        var clipBottom = (int)Math.Ceiling(bounds.Bottom * scale);
        var left = (int)Math.Ceiling(bounds.X * scale);
        var top = (int)Math.Ceiling(bounds.Y * scale);
        var width = (int)Math.Floor(bounds.Right * scale) - left;
        var height = (int)Math.Floor(bounds.Bottom * scale) - top;
        scale *= horizontal ? bounds.Width / viewport.Bounds.Width : bounds.Height / viewport.Bounds.Height;
        var crossLength = horizontal ? height : width;
        foreach (var (indicator, atStart) in new[] { (start, true), (end, false) })
        {
            var changed = 0;
            var clearlyDarkened = 0;
            var lightPixels = 0;
            for (var cross = crossLength / 4; cross < crossLength * 3 / 4; cross++)
            for (var distance = 0; distance < 6 * scale; distance++)
            {
                var x = left + (horizontal ? (atStart ? distance : width - 1 - distance) : cross);
                var y = top + (horizontal ? cross : (atStart ? distance : height - 1 - distance));
                var a = actual.GetPixel(x, y);
                var b = baseline.GetPixel(x, y);
                if (a.Red < b.Red || a.Green < b.Green || a.Blue < b.Blue) changed++;
                if (distance == 0 && b.Red > 100)
                {
                    lightPixels++;
                    if (b.Red - a.Red >= b.Red * 0.025) clearlyDarkened++;
                }
            }
            if (indicator.IsVisible)
            {
                if (lightPixels > 0) clearlyDarkened.ShouldBeGreaterThan(lightPixels / 4,
                    $"the overflow shadow must remain visibly dark: scale={window.RenderScaling}, bounds={bounds}, start={atStart}, changed={changed}, strong={clearlyDarkened}, sampled={lightPixels}");
                changed.ShouldBeGreaterThan(crossLength / 2,
                    "the shadow must darken the first six DIPs inside the overflow boundary");
            }
            else
                changed.ShouldBe(0, "a non-overflowing edge must not cast a shadow");
        }

        // No detached stripe deeper in the tabs, nor shadow over the more button,
        // neighboring content, or outside the viewport's cross-axis bounds.
        for (var y = 0; y < actual.Height; y++)
        for (var x = 0; x < actual.Width; x++)
        {
            if (actual.GetPixel(x, y) == baseline.GetPixel(x, y)) continue;
            var inViewport = x >= clipLeft && x < clipRight && y >= clipTop && y < clipBottom;
            var atEdge = horizontal ? x < left + 16 * scale || x >= left + width - 16 * scale
                                    : y < top + 16 * scale || y >= top + height - 16 * scale;
            (inViewport && atEdge).ShouldBeTrue($"unexpected shadow pixel at ({x}, {y}), viewport {left},{top},{width},{height}");
        }
    }

    private static SKBitmap Capture(AvaloniaWindow window)
    {
        using var frame = window.CaptureRenderedFrame().ShouldNotBeNull();
        using var stream = new MemoryStream();
        frame.Save(stream, PngBitmapEncoderOptions.Default);
        stream.Position = 0;
        return SKBitmap.Decode(stream);
    }

    private static void Refresh(AvaloniaWindow window)
    {
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
    }

    private static T Find<T>(Control root, string name) where T : Control =>
        root.GetVisualDescendants().OfType<T>().Single(control => control.Name == name);

    private static ItemsControl CreateOwner(int kind, Dock placement)
    {
        ItemsControl owner = kind switch
        {
            0 => new TabControl { TabStripPlacement = placement, IsMotionEnabled = false },
            1 => new CardTabControl { TabStripPlacement = placement, IsMotionEnabled = false },
            2 => new TabStrip { TabStripPlacement = placement, IsMotionEnabled = false },
            _ => new CardTabStrip { TabStripPlacement = placement, IsMotionEnabled = false }
        };
        owner.Width = 520;
        owner.Height = 250;
        for (var i = 0; i < 20; i++)
        {
            owner.Items.Add(kind < 2 ? (Control)new TabItem { Header = $"Tab-{i}", Content = "Content of tab" }
                                    : new TabStripItem { Content = $"Tab-{i}" });
        }
        return owner;
    }
}
