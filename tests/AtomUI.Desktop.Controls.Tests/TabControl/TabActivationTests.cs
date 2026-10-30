using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Threading;
using Shouldly;
using Xunit;
using AtomTabControl = AtomUI.Desktop.Controls.TabControl;
using AtomTabItem = AtomUI.Desktop.Controls.TabItem;
using AtomTabStrip = AtomUI.Desktop.Controls.TabStrip;
using AtomTabStripItem = AtomUI.Desktop.Controls.TabStripItem;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.TabControl;

public class TabActivationTests
{
    static TabActivationTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void TabControl_Default_Activates_Tab_On_Same_Tab_Release()
    {
        var tabControl = CreateTabControl();

        ShowInWindow(tabControl, window =>
        {
            var target = GetContainer<AtomTabItem>(tabControl, 1);
            var point  = TranslateToWindow(target, new Point(target.Bounds.Width / 2, target.Bounds.Height / 2), window);

            window.MouseMove(point);
            window.MouseDown(point, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            tabControl.SelectedIndex.ShouldBe(0);

            window.MouseUp(point, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            tabControl.SelectedIndex.ShouldBe(1);
        });
    }

    [Fact]
    public void TabControl_PointerPressed_Mode_Activates_Tab_On_Press()
    {
        var tabControl = CreateTabControl();
        tabControl.TabActivationTrigger = AtomUI.Desktop.Controls.TabActivationTrigger.PointerPressed;

        ShowInWindow(tabControl, window =>
        {
            var target = GetContainer<AtomTabItem>(tabControl, 1);
            var point  = TranslateToWindow(target, new Point(target.Bounds.Width / 2, target.Bounds.Height / 2), window);

            window.MouseMove(point);
            window.MouseDown(point, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            tabControl.SelectedIndex.ShouldBe(1);
        });
    }

    [Fact]
    public void TabControl_Default_Does_Not_Activate_When_Released_Over_Different_Tab()
    {
        var tabControl = CreateTabControl();

        ShowInWindow(tabControl, window =>
        {
            var pressed  = GetContainer<AtomTabItem>(tabControl, 1);
            var released = GetContainer<AtomTabItem>(tabControl, 2);
            var start    = TranslateToWindow(pressed, new Point(pressed.Bounds.Width / 2, pressed.Bounds.Height / 2), window);
            var end      = TranslateToWindow(released, new Point(released.Bounds.Width / 2, released.Bounds.Height / 2), window);

            window.MouseMove(start);
            window.MouseDown(start, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            window.MouseMove(end);
            Dispatcher.UIThread.RunJobs();
            window.MouseUp(end, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            tabControl.SelectedIndex.ShouldBe(0);
        });
    }

    [Fact]
    public void TabControl_Reorder_Enabled_Activates_Tab_On_Same_Tab_Release_When_Not_Dragging()
    {
        var tabControl = CreateTabControl();
        tabControl.IsTabReorderEnabled = true;

        ShowInWindow(tabControl, window =>
        {
            var target = GetContainer<AtomTabItem>(tabControl, 1);
            var point  = TranslateToWindow(target, new Point(target.Bounds.Width / 2, target.Bounds.Height / 2), window);

            window.MouseMove(point);
            window.MouseDown(point, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            tabControl.SelectedIndex.ShouldBe(0);

            window.MouseUp(point, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            tabControl.SelectedIndex.ShouldBe(1);
        });
    }

    [Fact]
    public void TabControl_Reorder_Enabled_Keeps_Interactive_Foreground_While_Pressed_After_Pointer_Leaves_Tab()
    {
        var tabControl = CreateTabControl();
        tabControl.IsTabReorderEnabled = true;
        tabControl.IsMotionEnabled = false;

        ShowInWindow(tabControl, window =>
        {
            var target = GetContainer<AtomTabItem>(tabControl, 1);
            var point  = TranslateToWindow(target, new Point(target.Bounds.Width / 2, target.Bounds.Height / 2), window);
            var normalForeground = target.Foreground;

            window.MouseMove(point);
            Dispatcher.UIThread.RunJobs();
            window.MouseDown(point, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            window.MouseMove(new Point(point.X + target.Bounds.Width + 24, point.Y));
            Dispatcher.UIThread.RunJobs();

            target.Foreground.ShouldNotBe(normalForeground);
        });
    }

    [Fact]
    public void TabItem_Base_Theme_Defines_Pressed_Interactive_Foreground()
    {
        var source = ReadRepoFile("src/AtomUI.Desktop.Controls/TabControl/Themes/BaseTabItemTheme.axaml");

        source.ShouldContain("<Style Selector=\"^:pressed\">");
        source.ShouldContain("<Setter Property=\"Foreground\" Value=\"{atom:SharedTokenResource ColorPrimaryActive}\" />");
        source.ShouldNotContain("ControlTokenScope.Identity");
    }

    [Fact]
    public void TabStrip_Default_Activates_Tab_On_Same_Tab_Release()
    {
        var tabStrip = CreateTabStrip();

        ShowInWindow(tabStrip, window =>
        {
            var target = GetContainer<AtomTabStripItem>(tabStrip, 1);
            var point  = TranslateToWindow(target, new Point(target.Bounds.Width / 2, target.Bounds.Height / 2), window);

            window.MouseMove(point);
            window.MouseDown(point, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            tabStrip.SelectedIndex.ShouldBe(0);

            window.MouseUp(point, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            tabStrip.SelectedIndex.ShouldBe(1);
        });
    }

    [Fact]
    public void TabStrip_PointerPressed_Mode_Activates_Tab_On_Press()
    {
        var tabStrip = CreateTabStrip();
        tabStrip.TabActivationTrigger = AtomUI.Desktop.Controls.TabActivationTrigger.PointerPressed;

        ShowInWindow(tabStrip, window =>
        {
            var target = GetContainer<AtomTabStripItem>(tabStrip, 1);
            var point  = TranslateToWindow(target, new Point(target.Bounds.Width / 2, target.Bounds.Height / 2), window);

            window.MouseMove(point);
            window.MouseDown(point, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            tabStrip.SelectedIndex.ShouldBe(1);
        });
    }

    [Fact]
    public void TabStrip_Default_Does_Not_Activate_When_Released_Over_Different_Tab()
    {
        var tabStrip = CreateTabStrip();

        ShowInWindow(tabStrip, window =>
        {
            var pressed  = GetContainer<AtomTabStripItem>(tabStrip, 1);
            var released = GetContainer<AtomTabStripItem>(tabStrip, 2);
            var start    = TranslateToWindow(pressed, new Point(pressed.Bounds.Width / 2, pressed.Bounds.Height / 2), window);
            var end      = TranslateToWindow(released, new Point(released.Bounds.Width / 2, released.Bounds.Height / 2), window);

            window.MouseMove(start);
            window.MouseDown(start, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            window.MouseMove(end);
            Dispatcher.UIThread.RunJobs();
            window.MouseUp(end, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            tabStrip.SelectedIndex.ShouldBe(0);
        });
    }

    [Fact]
    public void TabStrip_Reorder_Enabled_Activates_Tab_On_Same_Tab_Release_When_Not_Dragging()
    {
        var tabStrip = CreateTabStrip();
        tabStrip.IsTabReorderEnabled = true;

        ShowInWindow(tabStrip, window =>
        {
            var target = GetContainer<AtomTabStripItem>(tabStrip, 1);
            var point  = TranslateToWindow(target, new Point(target.Bounds.Width / 2, target.Bounds.Height / 2), window);

            window.MouseMove(point);
            window.MouseDown(point, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            tabStrip.SelectedIndex.ShouldBe(0);

            window.MouseUp(point, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            tabStrip.SelectedIndex.ShouldBe(1);
        });
    }

    [Fact]
    public void TabStrip_Reorder_Enabled_Keeps_Interactive_Foreground_While_Pressed_After_Pointer_Leaves_Tab()
    {
        var tabStrip = CreateTabStrip();
        tabStrip.IsTabReorderEnabled = true;
        tabStrip.IsMotionEnabled = false;

        ShowInWindow(tabStrip, window =>
        {
            var target = GetContainer<AtomTabStripItem>(tabStrip, 1);
            var point  = TranslateToWindow(target, new Point(target.Bounds.Width / 2, target.Bounds.Height / 2), window);
            var normalForeground = target.Foreground;

            window.MouseMove(point);
            Dispatcher.UIThread.RunJobs();
            window.MouseDown(point, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            window.MouseMove(new Point(point.X + target.Bounds.Width + 24, point.Y));
            Dispatcher.UIThread.RunJobs();

            target.Foreground.ShouldNotBe(normalForeground);
        });
    }

    [Fact]
    public void TabStripItem_Base_Theme_Defines_Pressed_Interactive_Foreground()
    {
        var source = ReadRepoFile("src/AtomUI.Desktop.Controls/TabControl/Themes/TabStrip/BaseTabStripItemTheme.axaml");

        source.ShouldContain("<Style Selector=\"^:pressed\">");
        source.ShouldContain("<Setter Property=\"Foreground\" Value=\"{atom:SharedTokenResource ColorPrimaryActive}\" />");
        source.ShouldNotContain("ControlTokenScope.Identity");
    }

    private static AtomTabControl CreateTabControl()
    {
        var tabControl = new AtomTabControl
        {
            Width         = 360,
            Height        = 140,
            SelectedIndex = 0
        };
        tabControl.Items.Add(new AtomTabItem { Header = "Tab 1", Content = "Content 1" });
        tabControl.Items.Add(new AtomTabItem { Header = "Tab 2", Content = "Content 2" });
        tabControl.Items.Add(new AtomTabItem { Header = "Tab 3", Content = "Content 3" });
        return tabControl;
    }

    private static AtomTabStrip CreateTabStrip()
    {
        var tabStrip = new AtomTabStrip
        {
            Width         = 360,
            Height        = 80,
            SelectedIndex = 0
        };
        tabStrip.Items.Add(new AtomTabStripItem { Content = "Tab 1" });
        tabStrip.Items.Add(new AtomTabStripItem { Content = "Tab 2" });
        tabStrip.Items.Add(new AtomTabStripItem { Content = "Tab 3" });
        return tabStrip;
    }

    private static T GetContainer<T>(ItemsControl owner, int index)
        where T : Control
    {
        RunJobsUntil(() => owner.ContainerFromIndex(index) is T);
        return owner.ContainerFromIndex(index).ShouldBeOfType<T>();
    }

    private static Point TranslateToWindow(Control source, Point point, AvaloniaWindow window)
    {
        var translated = source.TranslatePoint(point, window);
        translated.ShouldNotBeNull();
        return translated.Value;
    }

    private static void ShowInWindow(Control content, Action<AvaloniaWindow> assertion)
    {
        var window = new AvaloniaWindow
        {
            Width   = 520,
            Height  = 360,
            Content = content
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            assertion(window);
        }
        finally
        {
            window.MouseUp(new Point(0, 0), MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            window.Close();
        }
    }

    private static void RunJobsUntil(Func<bool> condition, int maxPasses = 128)
    {
        for (var i = 0; i < maxPasses; i++)
        {
            Dispatcher.UIThread.RunJobs();
            if (condition())
            {
                return;
            }
        }
    }

    private static void WaitForForegroundChanged(Func<object?> foregroundAccessor, object? normalForeground)
    {
        for (var i = 0; i < 32; i++)
        {
            Dispatcher.UIThread.RunJobs();
            if (!Equals(foregroundAccessor(), normalForeground))
            {
                return;
            }

            // The pressed foreground is applied through a theme transition driven by
            // the render clock. Force a tick so the wait does not depend on wall-clock
            // scheduling while other test collections run in parallel.
            AvaloniaHeadlessPlatform.ForceRenderTimerTick(1);
        }
    }
    private static string ReadRepoFile(string relativePath)
    {
        var repoRoot = FindRepoRoot(AppContext.BaseDirectory);
        return File.ReadAllText(Path.Combine(repoRoot, relativePath));
    }

    private static string FindRepoRoot(string start)
    {
        var current = new DirectoryInfo(start);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "AtomUI.slnx")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate AtomUI repository root.");
    }
}
