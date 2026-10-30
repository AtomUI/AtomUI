using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIMenu = AtomUI.Desktop.Controls.Menu;
using AtomUIMenuItem = AtomUI.Desktop.Controls.MenuItem;

namespace AtomUI.Desktop.Controls.Tests.Menu;

/// <summary>
/// 复现并锁定一个栈溢出缺陷：普通 Menu 的子菜单弹层无法呈现时（放置目标跑出 TopLevel 可视矩形，
/// 例如菜单位于长页面折叠线以下），弹层的 Opened / Closed 事件会把业务 open state 写回，
/// 该写回又驱动 <c>SyncSubMenuPopupOpenState</c> 再次开关弹层，形成无界同步递归直到栈溢出。
/// 业务打开状态与弹层呈现之间的传播必须是非重入的。
/// </summary>
public class MenuSubmenuOpenStateRecursionTests
{
    static MenuSubmenuOpenStateRecursionTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Opening_A_Submenu_Outside_The_Viewport_Does_Not_Recurse()
    {
        var file = new AtomUIMenuItem { Header = "File" };
        file.Items.Add(new AtomUIMenuItem { Header = "New" });

        var menu = new AtomUIMenu { IsMotionEnabled = false };
        menu.Items.Add(file);
        menu.Items.Add(new AtomUIMenuItem { Header = "Edit" });

        // 用很高的占位内容把菜单推到可视区之外：弹层放置目标不在 TopLevel 可视矩形内，
        // Popup 会按放置目标有效性规则拒绝保持打开。
        var filler = new Border { Height = 3000, Width = 400 };
        var panel = new StackPanel();
        panel.Children.Add(filler);
        panel.Children.Add(menu);

        var scroller = new ScrollViewer { Content = panel };
        var vlm = new VisualLayerManager { Child = scroller };
        typeof(VisualLayerManager)
            .GetProperty("EnablePopupOverlayLayer", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(vlm, true);

        var window = new AtomUI.Desktop.Controls.Window { Width = 600, Height = 400, Content = vlm };
        try
        {
            window.Show();
            for (var i = 0; i < 5; i++)
            {
                Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();
            }

            // 调用必须返回（不得栈溢出）；弹层无法呈现时业务状态允许退化。
            file.SetCurrentValue(Avalonia.Controls.MenuItem.IsSubMenuOpenProperty, true);
            for (var i = 0; i < 10; i++)
            {
                Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();
            }

            // 收敛之后仍然可以正常同步：显式关闭后不残留打开状态。
            file.SetCurrentValue(Avalonia.Controls.MenuItem.IsSubMenuOpenProperty, false);
            Dispatcher.UIThread.RunJobs();
            file.IsSubMenuOpen.ShouldBeFalse();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Opening_A_Visible_Submenu_Still_Opens_And_Closes_Normally()
    {
        var file = new AtomUIMenuItem { Header = "File" };
        file.Items.Add(new AtomUIMenuItem { Header = "New" });

        var menu = new AtomUIMenu { IsMotionEnabled = false };
        menu.Items.Add(file);

        var vlm = new VisualLayerManager { Child = menu };
        typeof(VisualLayerManager)
            .GetProperty("EnablePopupOverlayLayer", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(vlm, true);

        var window = new AtomUI.Desktop.Controls.Window { Width = 600, Height = 400, Content = vlm };
        try
        {
            window.Show();
            for (var i = 0; i < 5; i++)
            {
                Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();
            }

            file.SetCurrentValue(Avalonia.Controls.MenuItem.IsSubMenuOpenProperty, true);
            for (var i = 0; i < 10; i++)
            {
                Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();
            }

            file.IsSubMenuOpen.ShouldBeTrue();
            file.GetVisualDescendants()
                .OfType<Popup>()
                .ShouldContain(static popup => popup.IsOpen);

            file.SetCurrentValue(Avalonia.Controls.MenuItem.IsSubMenuOpenProperty, false);
            for (var i = 0; i < 10; i++)
            {
                Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();
            }

            file.IsSubMenuOpen.ShouldBeFalse();
            file.GetVisualDescendants()
                .OfType<Popup>()
                .ShouldNotContain(static popup => popup.IsOpen);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Declared_Open_Request_Follows_Dynamic_Submenu_Availability()
    {
        var file = new AtomUIMenuItem
        {
            Header = "File",
            IsSubMenuOpen = true
        };
        var menu = new AtomUIMenu { IsMotionEnabled = false };
        menu.Items.Add(file);

        var vlm = new VisualLayerManager { Child = menu };
        typeof(VisualLayerManager)
            .GetProperty("EnablePopupOverlayLayer", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(vlm, true);

        var window = new AtomUI.Desktop.Controls.Window { Width = 600, Height = 400, Content = vlm };
        try
        {
            window.Show();
            Settle(window);
            var popup = file.GetVisualDescendants().OfType<Popup>().Single();
            file.IsSubMenuOpen.ShouldBeFalse();
            popup.IsOpen.ShouldBeFalse();

            var child = new AtomUIMenuItem { Header = "Open" };
            file.Items.Add(child);
            Settle(window);
            file.IsSubMenuOpen.ShouldBeTrue();
            popup.IsOpen.ShouldBeTrue();

            file.Items.Remove(child);
            Settle(window);
            file.IsSubMenuOpen.ShouldBeFalse();
            popup.IsOpen.ShouldBeFalse();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static void Settle(Avalonia.Controls.Window window)
    {
        for (var i = 0; i < 5; i++)
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }
}
