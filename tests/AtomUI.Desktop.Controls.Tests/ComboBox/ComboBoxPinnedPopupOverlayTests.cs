using AtomUI.Controls.Primitives;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIComboBox = AtomUI.Desktop.Controls.ComboBox;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.ComboBox;

/// <summary>
/// 钉住打开的弹层必须不留下 light-dismiss 遮罩。Avalonia 只在弹层打开瞬间读取
/// <c>IsLightDismissEnabled</c> 创建遮罩层，因此「打开」必须晚于「抑制」。
/// ComboBox 直接派生 Avalonia <c>ComboBox</c>，不在 <c>AbstractSelect</c> 继承链上
/// （Cascader / Select / TreeSelect 由该基类统一抑制），因此需要自持同形实现。
/// </summary>
public class ComboBoxPinnedPopupOverlayTests
{
    private const string DismissLayerTypeName = "LightDismissOverlayLayer";

    static ComboBoxPinnedPopupOverlayTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    /// <summary>
    /// 核心回归：预览式写法在挂载前就把 <c>IsDropDownOpen</c> 与钉住置为 true。
    /// 若弹层由模板绑定在模板充气阶段打开，遮罩会按模板默认值先建出来，钉住再抑制属性也
    /// 撤不掉已创建的遮罩层，结果整页只剩输入框可点。
    /// </summary>
    [Fact]
    public void Pinned_Popup_Opened_Before_Template_Apply_Leaves_No_Visible_Dismiss_Mask()
    {
        var comboBox = new AtomUIComboBox
        {
            Width             = 320,
            IsMotionEnabled   = false,
            IsDropDownOpen    = true,
            IsPopupPinnedOpen = true,
            PlaceholderText   = "select",
            ItemsSource       = new[] { "Alpha", "Beta", "Gamma" }
        };

        ShowInWindow(comboBox, window =>
        {
            var popup = comboBox.GetVisualDescendants().OfType<Popup>().Single();
            popup.IsOpen.ShouldBeTrue();
            popup.IsLightDismissEnabled.ShouldBeFalse();

            window.GetVisualDescendants()
                  .Where(static layer => layer.GetType().Name == DismissLayerTypeName)
                  .ShouldNotContain(static layer => layer.IsVisible);

            var hit = window.InputHitTest(new Point(620, 460));
            hit.ShouldNotBeNull();
            hit!.GetType().Name.ShouldNotBe(
                DismissLayerTypeName,
                "钉住弹层不得让 light-dismiss 遮罩接管整页输入。");
        });
    }

    /// <summary>
    /// 反向护栏：未钉住时必须保留常规遮罩行为，避免为了修钉住把 light-dismiss 一并关掉。
    /// </summary>
    [Fact]
    public void Unpinned_Popup_Keeps_The_Dismiss_Mask()
    {
        var comboBox = new AtomUIComboBox
        {
            Width           = 320,
            IsMotionEnabled = false,
            IsDropDownOpen  = true,
            PlaceholderText = "select",
            ItemsSource     = new[] { "Alpha", "Beta", "Gamma" }
        };

        ShowInWindow(comboBox, window =>
        {
            var popup = comboBox.GetVisualDescendants().OfType<Popup>().Single();
            popup.IsOpen.ShouldBeTrue();
            popup.IsLightDismissEnabled.ShouldBeTrue();

            window.GetVisualDescendants()
                  .Where(static layer => layer.GetType().Name == DismissLayerTypeName)
                  .ShouldContain(static layer => layer.IsVisible);

            window.InputHitTest(new Point(620, 460))?.GetType().Name.ShouldBe(DismissLayerTypeName);
        });
    }

    /// <summary>
    /// 弹层自身发起的关闭（light-dismiss 外点、Escape）必须回写 <c>IsDropDownOpen</c>。
    /// 此回传此前依赖模板上的 TwoWay <c>IsOpen</c> 绑定；改为代码接管开合后必须显式补齐，
    /// 否则状态会卡在 true。
    /// </summary>
    [Fact]
    public void Popup_Self_Close_Writes_Back_IsDropDownOpen()
    {
        var comboBox = new AtomUIComboBox
        {
            Width           = 320,
            IsMotionEnabled = false,
            PlaceholderText = "select",
            ItemsSource     = new[] { "Alpha", "Beta", "Gamma" }
        };

        ShowInWindow(comboBox, window =>
        {
            comboBox.SetCurrentValue(AtomUIComboBox.IsDropDownOpenProperty, true);
            Dispatcher.UIThread.RunJobs();

            var popup = comboBox.GetVisualDescendants().OfType<Popup>().Single();
            popup.IsOpen.ShouldBeTrue();
            comboBox.IsDropDownOpen.ShouldBeTrue();

            // 模拟弹层自行关闭，而不是由 IsDropDownOpen 驱动关闭。
            popup.Close();
            Dispatcher.UIThread.RunJobs();

            popup.IsOpen.ShouldBeFalse();
            comboBox.IsDropDownOpen.ShouldBeFalse(
                "弹层自行关闭后 IsDropDownOpen 必须同步回写为 false。");
        });
    }

    /// <summary>
    /// 打开弹层后再钉住：抑制仍然生效，且不得因缺少模板绑定而无法打开。
    /// </summary>
    [Fact]
    public void Pinning_After_Open_Keeps_The_Popup_Open()
    {
        var comboBox = new AtomUIComboBox
        {
            Width           = 320,
            IsMotionEnabled = false,
            PlaceholderText = "select",
            ItemsSource     = new[] { "Alpha", "Beta", "Gamma" }
        };

        ShowInWindow(comboBox, window =>
        {
            comboBox.SetCurrentValue(AtomUIComboBox.IsDropDownOpenProperty, true);
            Dispatcher.UIThread.RunJobs();

            var popup = comboBox.GetVisualDescendants().OfType<Popup>().Single();
            popup.IsOpen.ShouldBeTrue();

            comboBox.IsPopupPinnedOpen = true;
            Dispatcher.UIThread.RunJobs();

            popup.IsLightDismissEnabled.ShouldBeFalse();
            popup.IsOpen.ShouldBeTrue();
            comboBox.IsDropDownOpen.ShouldBeTrue();
        });
    }

    /// <summary>
    /// 取消钉住后恢复模板默认，使下一次打开回到常规遮罩行为（与 Cascader 同形）。
    /// </summary>
    [Fact]
    public void Unpin_Restores_The_Template_Light_Dismiss_Default()
    {
        var comboBox = new AtomUIComboBox
        {
            Width             = 320,
            IsMotionEnabled   = false,
            IsPopupPinnedOpen = true,
            PlaceholderText   = "select",
            ItemsSource       = new[] { "Alpha", "Beta", "Gamma" }
        };

        ShowInWindow(comboBox, window =>
        {
            var popup = comboBox.GetVisualDescendants().OfType<Popup>().Single();
            popup.IsLightDismissEnabled.ShouldBeFalse();

            comboBox.IsPopupPinnedOpen = false;
            Dispatcher.UIThread.RunJobs();

            popup.IsLightDismissEnabled.ShouldBeTrue();
        });
    }

    /// <summary>
    /// 开合改由代码接管后的回归护栏：钉住期间业务关闭请求被拒绝（既有契约），
    /// 取消钉住后可正常关闭并再次打开，且重开不残留遮罩。
    /// </summary>
    [Fact]
    public void Pinned_Popup_Reopens_Without_A_Dismiss_Mask_After_Unpin_And_Close()
    {
        var comboBox = new AtomUIComboBox
        {
            Width             = 320,
            IsMotionEnabled   = false,
            IsPopupPinnedOpen = true,
            PlaceholderText   = "select",
            ItemsSource       = new[] { "Alpha", "Beta", "Gamma" }
        };

        ShowInWindow(comboBox, window =>
        {
            var popup = comboBox.GetVisualDescendants().OfType<Popup>().Single();
            popup.IsOpen.ShouldBeTrue();

            // 钉住期间业务关闭被拒绝，这是已发布的 pinned 契约。
            comboBox.SetCurrentValue(AtomUIComboBox.IsDropDownOpenProperty, false);
            Dispatcher.UIThread.RunJobs();
            popup.IsOpen.ShouldBeTrue();
            comboBox.IsDropDownOpen.ShouldBeTrue();

            // 取消钉住后关闭请求生效。
            comboBox.IsPopupPinnedOpen = false;
            Dispatcher.UIThread.RunJobs();
            comboBox.SetCurrentValue(AtomUIComboBox.IsDropDownOpenProperty, false);
            Dispatcher.UIThread.RunJobs();
            popup.IsOpen.ShouldBeFalse();

            // 再次打开：走常规 light-dismiss 路径，遮罩正常出现。
            comboBox.SetCurrentValue(AtomUIComboBox.IsDropDownOpenProperty, true);
            Dispatcher.UIThread.RunJobs();
            popup.IsOpen.ShouldBeTrue();
            popup.IsLightDismissEnabled.ShouldBeTrue();
        });
    }

    [Fact]
    public void Unpin_Preserves_Explicitly_Disabled_Light_Dismiss_While_Open()
    {
        var comboBox = new AtomUIComboBox
        {
            Width = 320, IsMotionEnabled = false, ItemsSource = new[] { "Alpha", "Beta" }
        };
        ShowInWindow(comboBox, window =>
        {
            comboBox.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();
            var popup = comboBox.GetVisualDescendants().OfType<Popup>().Single();
            popup.IsLightDismissEnabled = false;
            comboBox.IsPopupPinnedOpen = true;
            comboBox.IsPopupPinnedOpen = false;
            popup.IsLightDismissEnabled.ShouldBeFalse();
            popup.IsOpen.ShouldBeTrue();
        });
    }

    private static void ShowInWindow(Control content, Action<AvaloniaWindow> assertion)
    {
        var overlayPanel = new ScopeAwareOverlayLayerPanel
        {
            Width  = 640,
            Height = 480
        };
        overlayPanel.Children.Add(new Border
        {
            Width = 640,
            Height = 480,
            Background = Brushes.Transparent
        });
        overlayPanel.Children.Add(content);
        var visualLayerManager = new VisualLayerManager
        {
            Child = overlayPanel
        };
        EnablePopupOverlayLayer(visualLayerManager);

        var window = new AvaloniaWindow
        {
            Width   = 640,
            Height  = 480,
            Content = visualLayerManager
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            content.ApplyTemplate();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
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
