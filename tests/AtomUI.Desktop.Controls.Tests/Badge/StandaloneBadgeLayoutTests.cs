using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Badge;

public class StandaloneBadgeLayoutTests
{
    static StandaloneBadgeLayoutTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void DotBadge_Standalone_Measures_With_Style_Driven_Size()
    {
        var badge = new AtomUI.Desktop.Controls.DotBadge
        {
            Status = AtomUI.Controls.Commons.DotBadgeStatus.Success,
            Text   = "Success"
        };
        var host = new StackPanel { Children = { badge } };
        using var context = ShowInWindow(host, 400, 300);

        badge.DesiredSize.Width.ShouldBeGreaterThan(0,
            "a standalone DotBadge must measure its indicator and text; a zero desired size leaves the demo area blank.");
        badge.DesiredSize.Height.ShouldBeGreaterThan(0);
        badge.Bounds.Width.ShouldBeGreaterThan(0);
        badge.Bounds.Height.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void CountBadge_Standalone_Measures_With_Style_Driven_Size()
    {
        var badge = new AtomUI.Desktop.Controls.CountBadge
        {
            Count           = 5,
            IsMotionEnabled = false
        };
        var host = new StackPanel { Children = { badge } };
        using var context = ShowInWindow(host, 400, 300);

        badge.DesiredSize.Width.ShouldBeGreaterThan(0,
            "a standalone CountBadge must measure its count indicator; a zero desired size leaves the demo area blank.");
        badge.DesiredSize.Height.ShouldBeGreaterThan(0);
        badge.Bounds.Width.ShouldBeGreaterThan(0);
        badge.Bounds.Height.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void CountBadge_Standalone_Style_Recovery_Does_Not_Detach_Adorner()
    {
        var badge = new AtomUI.Desktop.Controls.CountBadge
        {
            Count           = 5,
            IsMotionEnabled = true
        };
        var host = new StackPanel { Children = { badge } };
        var window = new AvaloniaWindow { Width = 400, Height = 300, Content = host };
        try
        {
            window.Show();
            // 必须在消费 Loaded 优先级的重挂回调之前订阅，才能观测到 detach。
            var adorner = badge.GetVisualDescendants()
                               .OfType<Control>()
                               .Single(static c => c.GetType().Name == "CountBadgeAdorner");
            var detachCount = 0;
            adorner.DetachedFromVisualTree += (_, _) => detachCount++;
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            detachCount.ShouldBe(0,
                "recovering logical parenting for the standalone adorner must not detach it visually: " +
                "a detach cancels the pending show motion and leaves the indicator stuck at opacity 0.");
            badge.DesiredSize.Width.ShouldBeGreaterThan(0);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void RibbonBadge_Standalone_Measures_With_Style_Driven_Size()
    {
        var badge = new AtomUI.Desktop.Controls.RibbonBadge
        {
            Text = "Semantic Part"
        };
        var host = new StackPanel { Children = { badge } };
        using var context = ShowInWindow(host, 400, 300);

        badge.DesiredSize.Width.ShouldBeGreaterThan(0,
            "a standalone RibbonBadge must measure its ribbon visual; a zero desired size leaves the demo area blank.");
        badge.DesiredSize.Height.ShouldBeGreaterThan(0);
        badge.Bounds.Width.ShouldBeGreaterThan(0);
        badge.Bounds.Height.ShouldBeGreaterThan(0);
        badge.GetVisualDescendants()
             .OfType<TextBlock>()
             .ShouldContain(static text => text.Text == "Semantic Part",
                 "the standalone ribbon's label part requires the ControlTheme template to be applied.");
    }

    private static WindowContext ShowInWindow(Control control, double width, double height)
    {
        var window = new AvaloniaWindow
        {
            Width   = width,
            Height  = height,
            Content = control
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return new WindowContext(window);
    }

    private sealed class WindowContext : IDisposable
    {
        private readonly AvaloniaWindow _window;

        public WindowContext(AvaloniaWindow window)
        {
            _window = window;
        }

        public void Dispose()
        {
            _window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
