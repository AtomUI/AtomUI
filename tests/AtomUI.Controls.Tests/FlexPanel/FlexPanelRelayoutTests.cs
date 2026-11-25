using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;
using Xunit;

namespace AtomUI.Controls.Tests.FlexPanel;

// Child size changes must invalidate the FlexPanel even when the child's clamped DesiredSize stays the same.
public class FlexPanelRelayoutTests
{
    private const int Precision = 2;

    static FlexPanelRelayoutTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    private static void Pump(Window window)
    {
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
    }

    private static Border Item(double width)
    {
        return new Border { Width = width, Height = 20 };
    }

    private static Window Host(double panelWidth, params Control[] children)
    {
        var panel = new AtomUI.Controls.FlexPanel
        {
            Width               = panelWidth,
            HorizontalAlignment = HorizontalAlignment.Left,
            UseLayoutRounding   = false
        };
        foreach (var child in children)
        {
            panel.Children.Add(child);
        }

        var window = new Window { Width = 600, Height = 200, Content = panel };
        window.Show();
        Pump(window);
        return window;
    }

    [Fact]
    public void Overflowing_child_width_change_moves_following_sibling()
    {
        var first  = Item(200);
        var second = Item(200);
        var window = Host(300, first, second);

        first.Width = 250;
        Pump(window);

        Assert.Equal(0, first.Bounds.X, Precision);
        Assert.Equal(250, first.Bounds.Width, Precision);
        Assert.Equal(250, second.Bounds.X, Precision);
    }

    [Fact]
    public void Overflowing_child_margin_change_moves_following_sibling()
    {
        var first  = Item(200);
        var second = Item(200);
        var window = Host(300, first, second);

        first.Margin = new Thickness(0, 0, 50, 0);
        Pump(window);

        Assert.Equal(0, first.Bounds.X, Precision);
        Assert.Equal(250, second.Bounds.X, Precision);
    }

    [Fact]
    public void Child_width_change_recomputes_growing_sibling_slot()
    {
        var first  = Item(100);
        var second = new Border { Height = 20 };
        Flex.SetGrow(second, 1);
        var window = Host(300, first, second);

        first.Width = 150;
        Pump(window);

        Assert.Equal(0, first.Bounds.X, Precision);
        Assert.Equal(150, first.Bounds.Width, Precision);
        Assert.Equal(150, second.Bounds.X, Precision);
        Assert.Equal(150, second.Bounds.Width, Precision);
    }

    [Fact]
    public void Child_min_width_change_lets_it_shrink()
    {
        var first  = Item(200);
        var second = Item(200);
        var window = Host(300, first, second);

        first.MinWidth = 50;
        Pump(window);

        // The second item keeps its measured size as its automatic minimum, so the first absorbs the overflow.
        Assert.Equal(100, second.Bounds.X, Precision);
    }

    [Fact]
    public void Nested_content_width_change_moves_following_sibling()
    {
        var content = Item(200);
        var first   = new Border { Child = content };
        var second  = Item(200);
        var window  = Host(300, first, second);

        content.Width = 250;
        Pump(window);

        Assert.Equal(250, first.Bounds.Width, Precision);
        Assert.Equal(250, second.Bounds.X, Precision);
    }
}
