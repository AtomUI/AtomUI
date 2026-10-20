using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;
using AtomUIWindow = AtomUI.Desktop.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Popups;

public class PopupShadowTests
{
    static PopupShadowTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void ShadowsAwareContainer_Shadow_Renderer_Does_Not_Clip_BoxShadow()
    {
        var container = new ShadowsAwareContainer
        {
            BoxShadow = BoxShadows.Parse("0 8 24 0 #66000000"),
            Child     = new Border()
        };

        var shadowRenderer = GetFrameRenderer(container);

        shadowRenderer.ShouldNotBeNull();
        shadowRenderer!.ClipToBounds.ShouldBeFalse();
    }

    [Fact]
    public void Popup_Exposes_SurfaceBackground_As_A_Public_Styled_Property()
    {
        Popup.SurfaceBackgroundProperty.OwnerType.ShouldBe(typeof(Popup));
        typeof(Popup).GetProperty(nameof(Popup.SurfaceBackground)).ShouldNotBeNull();
        new Popup().SurfaceBackground.ShouldBeNull();
    }

    [Fact]
    public void ShadowsAwareContainer_Frame_Renderer_Draws_Configured_Surface()
    {
        var container = new ShadowsAwareContainer
        {
            BoxShadow         = BoxShadows.Parse("0 8 24 0 #66000000"),
            SurfaceBackground = Brushes.White,
            Child             = new Border()
        };

        var frameRenderer = GetFrameRenderer(container);

        frameRenderer.ShouldNotBeNull();
        frameRenderer!.GetType()
                      .GetProperty("SurfaceBackground", BindingFlags.Instance | BindingFlags.Public)
                      .ShouldNotBeNull()!
                      .GetValue(frameRenderer)
                      .ShouldBe(Brushes.White);
    }

    [Fact]
    public void ShadowsAwareContainer_Null_Surface_Preserves_Transparent_Frame_Fill()
    {
        var container = new ShadowsAwareContainer
        {
            BoxShadow         = BoxShadows.Parse("0 8 24 0 #66000000"),
            SurfaceBackground = null,
            Child             = new Border()
        };

        var frameRenderer = GetFrameRenderer(container);

        frameRenderer.ShouldNotBeNull();
        frameRenderer!.GetType()
                      .GetProperty("SurfaceBackground", BindingFlags.Instance | BindingFlags.Public)
                      .ShouldNotBeNull()!
                      .GetValue(frameRenderer)
                      .ShouldBeNull();
    }

    [Fact]
    public void ShadowsAwareContainer_Resolves_The_Final_Surface_Through_Nested_ContentPresenters()
    {
        var expected = new CornerRadius(9);
        var container = new ShadowsAwareContainer
        {
            Child = new ContentPresenter
            {
                Content = new ContentPresenter
                {
                    Content = new Border { CornerRadius = expected }
                }
            }
        };
        var window = new AvaloniaWindow { Content = container };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            container.CornerRadius.ShouldBe(expected);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void ShadowsAwareContainer_Releases_A_Replaced_Nested_Surface()
    {
        var oldSurface = new Border { CornerRadius = new CornerRadius(4) };
        var newSurface = new Border { CornerRadius = new CornerRadius(12) };
        var innerPresenter = new ContentPresenter { Content = oldSurface };
        var container = new ShadowsAwareContainer
        {
            Child = new ContentPresenter { Content = innerPresenter }
        };
        var window = new AvaloniaWindow { Content = container };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            container.CornerRadius.ShouldBe(new CornerRadius(4));

            innerPresenter.Content = newSurface;
            Dispatcher.UIThread.RunJobs();
            container.CornerRadius.ShouldBe(new CornerRadius(12));

            oldSurface.CornerRadius = new CornerRadius(20);
            Dispatcher.UIThread.RunJobs();
            container.CornerRadius.ShouldBe(new CornerRadius(12));

            newSurface.CornerRadius = new CornerRadius(16);
            Dispatcher.UIThread.RunJobs();
            container.CornerRadius.ShouldBe(new CornerRadius(16));
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void ToolTip_Shadow_Mask_Radius_Follows_Content_Decorator_Override()
    {
        var host = new Button();
        ToolTip.SetTip(host, "Object text");
        ToolTip.SetIsUseOverlayHost(host, true);

        var window = ShowInWindow(host);
        try
        {
            ToolTip.SetIsOpen(host, true);
            Dispatcher.UIThread.RunJobs();

            var toolTip = host.GetValue(ToolTip.ToolTipProperty);
            toolTip.ShouldNotBeNull();

            var arrowDecoratedBox = toolTip.GetVisualDescendants()
                                           .OfType<ArrowDecoratedBox>()
                                           .First();
            var decorator = arrowDecoratedBox.GetVisualDescendants()
                                             .OfType<Border>()
                                             .First(b => b.Name == "PART_ContentDecorator");

            // 未覆盖时蒙版圆角与容器 Border 一致（都来自 ToolTipCornerRadius 的模板绑定）
            arrowDecoratedBox.GetMaskCornerRadius().ShouldBe(decorator.CornerRadius);

            // 语义部件样式只覆盖容器 Border 的圆角时，阴影蒙版也必须跟随可见形状，
            // 否则角落会出现白底空隙（圆角不同步）。
            decorator.CornerRadius = new CornerRadius(12);
            Dispatcher.UIThread.RunJobs();

            arrowDecoratedBox.GetMaskCornerRadius().ShouldBe(new CornerRadius(12));
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void ShadowsAwareContainer_Radius_Tracks_Arrow_Child_Content_Decorator()
    {
        var host = new Button();
        ToolTip.SetTip(host, "Object text");
        ToolTip.SetIsUseOverlayHost(host, true);

        var window = ShowInWindow(host);
        try
        {
            ToolTip.SetIsOpen(host, true);
            Dispatcher.UIThread.RunJobs();

            var toolTip = host.GetValue(ToolTip.ToolTipProperty);
            toolTip.ShouldNotBeNull();

            var container = toolTip.GetVisualAncestors()
                                   .OfType<ShadowsAwareContainer>()
                                   .FirstOrDefault();
            container.ShouldNotBeNull("overlay host 主题中 ToolTip 应被 ShadowsAwareContainer 包裹");

            var decorator = toolTip.GetVisualDescendants()
                                   .OfType<Border>()
                                   .First(b => b.Name == "PART_ContentDecorator");

            decorator.CornerRadius = new CornerRadius(12);
            Dispatcher.UIThread.RunJobs();

            container.CornerRadius.ShouldBe(new CornerRadius(12),
                "弹层阴影圆角应跟随容器 Border 的实际圆角，而不是 ArrowDecoratedBox 自身的 CornerRadius");
        }
        finally
        {
            window.Close();
        }
    }

    private static Control? GetFrameRenderer(ShadowsAwareContainer container)
    {
        var field = typeof(ShadowsAwareContainer).GetField(
            "_frameRenderer",
            BindingFlags.Instance | BindingFlags.NonPublic);
        field.ShouldNotBeNull();
        return (Control?)field!.GetValue(container);
    }

    private static AtomUIWindow ShowInWindow(Control content)
    {
        var window = new AtomUIWindow
        {
            Width   = 800,
            Height  = 600,
            Content = content
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }
}
