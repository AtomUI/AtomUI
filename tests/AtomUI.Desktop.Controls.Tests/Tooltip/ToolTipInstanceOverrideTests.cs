using AtomUI.Controls;
using AtomUI.Theme.Algorithms;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIWindow = AtomUI.Desktop.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Tooltip;

/// <summary>
/// Tip 直接传入 ToolTip 实例时的定制契约：实例上显式设置（IsSet）的呈现类附加属性
/// 优先于宿主控件的值，未设置的回落到宿主。
/// </summary>
public class ToolTipInstanceOverrideTests
{
    static ToolTipInstanceOverrideTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Tip_ToolTip_Instance_Is_Used_Directly()
    {
        var host = new Button();
        var tip  = new ToolTip { Content = "custom tip content" };
        ToolTip.SetTip(host, tip);
        ToolTip.SetIsOpen(host, true);

        var window = ShowInWindow(host);
        try
        {
            host.GetValue(ToolTip.ToolTipProperty).ShouldBeSameAs(tip);
            tip.Classes.Contains(ToolTipPseudoClass.Open).ShouldBeTrue();
            tip.AdornedControl.ShouldBeSameAs(host);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Instance_Placement_Overrides_Host_Placement()
    {
        var host = new Button();
        var tip  = new ToolTip { Content = "tip" };
        ToolTip.SetPlacement(tip, PlacementMode.Right);
        ToolTip.SetTip(host, tip);

        var window = ShowInWindow(host);
        try
        {
            ToolTip.SetIsOpen(host, true);
            Dispatcher.UIThread.RunJobs();

            // PlacementMode.Right -> 箭头在 tooltip 左边缘；宿主默认 Top 对应下边缘
            FindArrowDecoratedBox(tip).ArrowPosition.ShouldBe(ArrowPosition.Left);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Host_Placement_Wins_When_Instance_Not_Set()
    {
        var host = new Button();
        ToolTip.SetPlacement(host, PlacementMode.Left);
        var tip = new ToolTip { Content = "tip" };
        ToolTip.SetTip(host, tip);

        var window = ShowInWindow(host);
        try
        {
            ToolTip.SetIsOpen(host, true);
            Dispatcher.UIThread.RunJobs();

            // 实例未显式设置 Placement，回落到宿主的 Left -> 箭头在右边缘
            FindArrowDecoratedBox(tip).ArrowPosition.ShouldBe(ArrowPosition.Right);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Instance_PresetColor_Overrides_Host_Color()
    {
        var host = new Button();
        ToolTip.SetPresetColor(host, PresetColorType.Red);
        var tip = new ToolTip { Content = "tip" };
        ToolTip.SetPresetColor(tip, PresetColorType.Blue);
        ToolTip.SetTip(host, tip);

        var window = ShowInWindow(host);
        try
        {
            ToolTip.SetIsOpen(host, true);
            Dispatcher.UIThread.RunJobs();

            var expected = PresetPrimaryColor.GetColor(PresetColorType.Blue).Color();
            tip.Background.ShouldBeOfType<SolidColorBrush>().Color.ShouldBe(expected);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Instance_Background_Local_Value_Is_Preserved()
    {
        var host = new Button();
        var tip  = new ToolTip
        {
            Content    = "tip",
            Background = Brushes.DarkViolet
        };
        ToolTip.SetTip(host, tip);

        var window = ShowInWindow(host);
        try
        {
            ToolTip.SetIsOpen(host, true);
            Dispatcher.UIThread.RunJobs();

            tip.Background.ShouldBeSameAs(Brushes.DarkViolet);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Instance_TextWrapping_Overrides_Host_Value()
    {
        var host = new Button();
        var tip  = new ToolTip { Content = string.Join(' ', Enumerable.Repeat("long tooltip text", 40)) };
        ToolTip.SetTextWrapping(tip, TextWrapping.NoWrap);
        ToolTip.SetTip(host, tip);

        var window = ShowInWindow(host);
        try
        {
            ToolTip.SetIsOpen(host, true);
            Dispatcher.UIThread.RunJobs();

            var presenter = tip.GetVisualDescendants()
                               .OfType<Avalonia.Controls.Presenters.ContentPresenter>()
                               .Single(p => p.Name == "PART_ContentPresenter");
            presenter.UpdateChild();
            var textBlock = presenter.Child.ShouldBeOfType<Avalonia.Controls.TextBlock>();
            textBlock.TextWrapping.ShouldBe(TextWrapping.NoWrap);
        }
        finally
        {
            window.Close();
        }
    }

    private static ArrowDecoratedBox FindArrowDecoratedBox(ToolTip toolTip)
    {
        toolTip.Classes.Contains(ToolTipPseudoClass.Open).ShouldBeTrue();
        return toolTip.GetVisualDescendants().OfType<ArrowDecoratedBox>().Single();
    }

    private static AtomUIWindow ShowInWindow(Control content)
    {
        // headless 平台没有原生 popup 窗口实现，弹层只能走 overlay host
        ToolTip.SetIsUseOverlayHost(content, true);
        // 宿主居中，保证左右两侧都有空间，避免弹层翻转干扰箭头位置断言
        content.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
        content.VerticalAlignment   = Avalonia.Layout.VerticalAlignment.Center;
        var window = new AtomUIWindow
        {
            Width   = 800,
            Height  = 600,
            Content = new Avalonia.Controls.Grid { Children = { content } }
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }
}
