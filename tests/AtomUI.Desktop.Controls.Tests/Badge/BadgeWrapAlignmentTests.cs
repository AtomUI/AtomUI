using Avalonia.Controls;
using Avalonia.Threading;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Badge;

public class BadgeWrapAlignmentTests
{
    static BadgeWrapAlignmentTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void CountBadge_Wraps_Decorated_Target_Instead_Of_Stretching()
    {
        var badge = new AtomUI.Desktop.Controls.CountBadge
        {
            Count           = 5,
            IsMotionEnabled = false,
            DecoratedTarget = new Border
            {
                Width  = 96,
                Height = 64
            }
        };
        var host = new StackPanel { Children = { badge } };
        using var context = ShowInWindow(host, width: 400, height: 300);

        badge.HorizontalAlignment.ShouldBe(Avalonia.Layout.HorizontalAlignment.Left,
            "a decorating badge must align like DotBadge: wrapping its target, not stretching over the parent slot.");
        badge.VerticalAlignment.ShouldBe(Avalonia.Layout.VerticalAlignment.Top);
        badge.Bounds.Width.ShouldBe(96, 0.001,
            "the badge root must wrap the decorated target; a stretched root drags both the indicator anchor and the semantic root marker across the parent.");
        badge.Bounds.Height.ShouldBe(64, 0.001);
    }

    [Fact]
    public void RibbonBadge_Wraps_Decorated_Target_Instead_Of_Stretching()
    {
        var badge = new AtomUI.Desktop.Controls.RibbonBadge
        {
            Text            = "Semantic Part",
            DecoratedTarget = new Border
            {
                Width  = 180,
                Height = 96
            }
        };
        var host = new StackPanel { Children = { badge } };
        using var context = ShowInWindow(host, width: 400, height: 300);

        badge.HorizontalAlignment.ShouldBe(Avalonia.Layout.HorizontalAlignment.Left,
            "a decorating badge must align like DotBadge: wrapping its target, not stretching over the parent slot.");
        badge.VerticalAlignment.ShouldBe(Avalonia.Layout.VerticalAlignment.Top);
        badge.Bounds.Width.ShouldBe(180, 0.001,
            "the badge root must wrap the decorated target; a stretched root drags the semantic root marker across the parent.");
        badge.Bounds.Height.ShouldBe(96, 0.001);
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
