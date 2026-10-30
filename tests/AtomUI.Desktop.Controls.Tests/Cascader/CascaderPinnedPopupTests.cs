using AtomUI.Controls.Primitives;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUICascader = AtomUI.Desktop.Controls.Cascader;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Cascader;

public class CascaderPinnedPopupTests
{
    private const string PopupListClass = "semantic-popup-list";
    private const string PopupListItemClass = "semantic-popup-list-item";

    static CascaderPinnedPopupTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Pinned_Popup_Suppresses_Light_Dismiss_Before_The_Popup_Opens()
    {
        var cascader = new AtomUICascader
        {
            Width = 320,
            IsMotionEnabled = false,
            IsDropDownOpen = true,
            IsPopupPinnedOpen = true,
            OptionsSource = new List<CascaderOption>
            {
                new()
                {
                    Header = "Zhejiang",
                    Children = { new CascaderOption { Header = "Hangzhou" } }
                },
                new()
                {
                    Header = "Jiangsu",
                    Children = { new CascaderOption { Header = "Nanjing" } }
                }
            }
        };

        ShowInWindow(cascader, window =>
        {
            var popup = cascader.GetVisualDescendants().OfType<Popup>().Single();
            popup.IsOpen.ShouldBeTrue();
            // Avalonia 只在弹层打开瞬间读取 IsLightDismissEnabled 创建遮罩层；
            // 钉住的弹层忽略 light-dismiss 关闭请求，必须在打开前抑制遮罩。
            popup.IsLightDismissEnabled.ShouldBeFalse();

            // 属性被抑制不代表遮罩未创建：模板充气阶段的先开后抑会留下一个
            // IsVisible=true 的 LightDismissOverlayLayer 挡住页面其余交互。
            // 与 AbstractAutoComplete 相同机制下，钉住打开不得出现可见遮罩层。
            window.GetVisualDescendants()
                  .Where(static layer => layer.GetType().Name == "LightDismissOverlayLayer")
                  .ShouldNotContain(static layer => layer.IsVisible);

            // 根列 marker 来自 CascaderView 模板的静态 marker，popup.list 部件
            // 至少命中根列；过滤列表为非过滤态下的隐藏替代实现，弹层内容位于
            // 独立可视根，需要从 window 搜索。
            var lists = window.GetVisualDescendants()
                              .OfType<Control>()
                              .Where(control => control.Classes.Contains(PopupListClass))
                              .ToArray();
            lists.Length.ShouldBeGreaterThanOrEqualTo(1);
            lists.ShouldContain(static list => list.IsVisible);

            // popup.listItem marker 在 CascaderViewItem 容器创建时注入（与 ListBox
            // 的 semantic-item 相同模式）。
            CountItems(window).ShouldBeGreaterThanOrEqualTo(2);
        });
    }

    [Fact]
    public void Unpin_Restores_The_Template_Light_Dismiss_Default()
    {
        var cascader = new AtomUICascader
        {
            Width = 320,
            IsMotionEnabled = false,
            IsPopupPinnedOpen = true
        };

        ShowInWindow(cascader, window =>
        {
            var popup = cascader.GetVisualDescendants().OfType<Popup>().Single();
            popup.IsLightDismissEnabled.ShouldBeFalse();

            cascader.IsPopupPinnedOpen = false;
            Dispatcher.UIThread.RunJobs();

            popup.IsLightDismissEnabled.ShouldBeTrue();
        });
    }

    [Fact]
    public void Popup_Items_Keep_Markers_Across_Reopen_And_Source_Reset()
    {
        var cascader = new AtomUICascader
        {
            Width = 320,
            IsMotionEnabled = false,
            OptionsSource = new List<CascaderOption>
            {
                new()
                {
                    Header = "Zhejiang",
                    Children = { new CascaderOption { Header = "Hangzhou" } }
                }
            }
        };

        ShowInWindow(cascader, window =>
        {
            cascader.SetCurrentValue(AtomUICascader.IsDropDownOpenProperty, true);
            Dispatcher.UIThread.RunJobs();
            CountItems(window).ShouldBeGreaterThanOrEqualTo(1);

            cascader.SetCurrentValue(AtomUICascader.IsDropDownOpenProperty, false);
            Dispatcher.UIThread.RunJobs();

            cascader.SetCurrentValue(AtomUICascader.IsDropDownOpenProperty, true);
            Dispatcher.UIThread.RunJobs();
            CountItems(window).ShouldBeGreaterThanOrEqualTo(1);

            cascader.OptionsSource = new List<CascaderOption>
            {
                new()
                {
                    Header = "Jiangsu",
                    Children = { new CascaderOption { Header = "Nanjing" } }
                }
            };
            Dispatcher.UIThread.RunJobs();
            CountItems(window).ShouldBeGreaterThanOrEqualTo(1);
        });
    }

    private static int CountItems(AvaloniaWindow window)
    {
        return window.GetVisualDescendants()
                     .OfType<Control>()
                     .Count(control => control.Classes.Contains(PopupListItemClass));
    }

    private static void ShowInWindow(Control content, Action<AvaloniaWindow> assertion)
    {
        var overlayPanel = new ScopeAwareOverlayLayerPanel
        {
            Width = 640,
            Height = 480
        };
        overlayPanel.Children.Add(content);
        var visualLayerManager = new VisualLayerManager
        {
            Child = overlayPanel
        };
        EnablePopupOverlayLayer(visualLayerManager);

        var window = new AvaloniaWindow
        {
            Width = 640,
            Height = 480,
            Content = visualLayerManager
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            content.ApplyTemplate();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            assertion(window);
        }
        finally
        {
            window.Close();
        }
    }

    private static void EnablePopupOverlayLayer(VisualLayerManager visualLayerManager)
    {
        var property = typeof(VisualLayerManager).GetProperty(
            "EnablePopupOverlayLayer",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        property.ShouldNotBeNull();
        property.SetValue(visualLayerManager, true);
    }
}
