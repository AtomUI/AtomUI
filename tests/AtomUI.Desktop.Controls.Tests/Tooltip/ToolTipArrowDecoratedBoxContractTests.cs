using System.Reflection;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIWindow = AtomUI.Desktop.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Tooltip;

/// <summary>
/// IArrowAwareShadowMaskInfoProvider.GetArrowDecoratedBox 契约回归测试（issue #477）。
/// ToolTip 模板尚未应用、或主题缺少 PART_ArrowDecorator 时，箭头部件可能不可用；
/// 提供方与消费者（ShadowsAwareContainer / Popup 定位）都必须按无箭头降级，不允许 NRE。
/// </summary>
public class ToolTipArrowDecoratedBoxContractTests
{
    static ToolTipArrowDecoratedBoxContractTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Unstyled_ToolTip_Arrow_Contract_Methods_Degrade_Without_Throwing()
    {
        var toolTip  = new ToolTip();
        var provider = (IArrowAwareShadowMaskInfoProvider)toolTip;

        provider.GetArrowDecoratedBox().ShouldBeNull();
        provider.IsArrowVisible().ShouldBeFalse();
        provider.GetArrowPosition().ShouldBe(default(ArrowPosition));
        provider.GetArrowIndicatorBounds().ShouldBe(default(Rect));
        provider.GetArrowIndicatorLayoutBounds().ShouldBe(default(Rect));
        Should.NotThrow(() => provider.SetArrowOpacity(0.5));

        var maskProvider = (IShadowMaskInfoProvider)toolTip;
        maskProvider.GetMaskCornerRadius().ShouldBe(new CornerRadius(0));
        maskProvider.GetMaskBounds().ShouldBe(new Rect(toolTip.Bounds.Size));
    }

    [Fact]
    public void Unstyled_MenuFlyoutPresenter_Arrow_Contract_Methods_Degrade_Without_Throwing()
    {
        var presenter = new MenuFlyoutPresenter();
        var provider  = (IArrowAwareShadowMaskInfoProvider)presenter;

        provider.GetArrowDecoratedBox().ShouldBeNull();
        provider.IsArrowVisible().ShouldBeFalse();
        provider.GetArrowPosition().ShouldBe(default(ArrowPosition));
        provider.GetArrowIndicatorBounds().ShouldBe(default(Rect));
        provider.GetArrowIndicatorLayoutBounds().ShouldBe(default(Rect));
        Should.NotThrow(() => provider.SetArrowOpacity(0.5));
    }

    [Fact]
    public void ToolTip_With_Template_Missing_Arrow_Part_Opens_And_Degrades_To_No_Arrow()
    {
        var toolTip = new ToolTip { Content = "tip", Template = BuildTemplateWithoutArrowPart() };
        var host    = new Button();
        ToolTip.SetTip(host, toolTip);

        var window = ShowInWindow(host);
        try
        {
            Should.NotThrow(() => ToolTip.SetIsOpen(host, true));
            Dispatcher.UIThread.RunJobs();

            var popup = GetPopup(toolTip);
            popup.IsOpen.ShouldBeTrue();

            var container = FindShadowsAwareContainer(window);
            container.IsArrowVisible.ShouldBeFalse("主题缺少 PART_ArrowDecorator 时弹层必须按无箭头降级，不允许崩溃");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Arrow_Bindings_Are_Established_When_Template_Later_Provides_The_Part()
    {
        var toolTip = new ToolTip { Content = "tip", Template = BuildTemplateWithoutArrowPart() };
        var host    = new Button();
        ToolTip.SetTip(host, toolTip);

        var window = ShowInWindow(host);
        try
        {
            ToolTip.SetIsOpen(host, true);
            Dispatcher.UIThread.RunJobs();
            var container = FindShadowsAwareContainer(window);
            container.IsArrowVisible.ShouldBeFalse();

            toolTip.Template = BuildTemplateWithArrowPart();
            toolTip.ApplyTemplate();
            Dispatcher.UIThread.RunJobs();

            container.IsArrowVisible.ShouldBeTrue("模板后续提供 PART_ArrowDecorator 后应重建箭头绑定");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Default_ToolTip_Theme_Wires_Arrow_Into_Popup_Container()
    {
        var host = new Button();
        ToolTip.SetTip(host, "tip");

        var window = ShowInWindow(host);
        try
        {
            ToolTip.SetIsOpen(host, true);
            Dispatcher.UIThread.RunJobs();

            var toolTip = host.GetValue(ToolTip.ToolTipProperty);
            toolTip.ShouldNotBeNull();
            var container = FindShadowsAwareContainer(window);
            container.IsArrowVisible.ShouldBeTrue("默认主题提供 PART_ArrowDecorator，箭头应接入弹层容器");
        }
        finally
        {
            window.Close();
        }
    }

    private static FuncControlTemplate<ToolTip> BuildTemplateWithoutArrowPart()
    {
        return new FuncControlTemplate<ToolTip>((_, _) => new Border
        {
            Child = new ContentPresenter()
        });
    }

    private static FuncControlTemplate<ToolTip> BuildTemplateWithArrowPart()
    {
        return new FuncControlTemplate<ToolTip>((_, scope) =>
        {
            var box = new ArrowDecoratedBox
            {
                Name    = AbstractArrowDecoratedBox.ArrowDecoratorPart,
                Content = new ContentPresenter()
            };
            scope.Register(box.Name, box);
            return box;
        });
    }

    private static AtomUIWindow ShowInWindow(Control content)
    {
        // headless 平台没有原生 popup 窗口实现，弹层只能走 overlay host
        ToolTip.SetIsUseOverlayHost(content, true);
        var window = new AtomUIWindow
        {
            Width   = 300,
            Height  = 160,
            Content = content
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static ShadowsAwareContainer FindShadowsAwareContainer(AtomUIWindow window)
    {
        return window.GetVisualDescendants().OfType<ShadowsAwareContainer>().Single(c => c.IsOverlayMode);
    }

    private static Popup GetPopup(ToolTip toolTip)
    {
        var field = typeof(ToolTip).GetField("_popup", BindingFlags.Instance | BindingFlags.NonPublic);
        field.ShouldNotBeNull();
        return field.GetValue(toolTip).ShouldBeOfType<Popup>();
    }
}
