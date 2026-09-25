using AtomUI.Desktop.Controls;
using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;
using Button = Avalonia.Controls.Button;
using TextBlock = Avalonia.Controls.TextBlock;

namespace AtomUI.Toolkits.GalleryBase.Tests.Controls;

public class ShowCaseZoomOverlayTests
{
    public ShowCaseZoomOverlayTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Template_Contains_Contract_Parts()
    {
        // 打开态断言：关闭态根 Border 隐藏时模板部分部件不在视觉树
        ShowInWindow(new ShowCaseZoomOverlay { IsOpen = true }, overlay =>
        {
            overlay.GetVisualDescendants()
                   .OfType<Border>()
                   .Single(border => border.Name == "PART_RootBorder")
                   .ShouldNotBeNull();
            overlay.GetVisualDescendants()
                   .OfType<Border>()
                   .Single(border => border.Name == "PART_CardBorder")
                   .ShouldNotBeNull();
            overlay.GetVisualDescendants()
                   .OfType<ShowCaseZoomStagePanel>()
                   .Single(panel => panel.Name == "PART_StagePanel")
                   .ShouldNotBeNull();
            FindCloseButton(overlay).ShouldNotBeNull();
        });
    }

    [Fact]
    public void Root_Border_Is_Hidden_While_Closed_And_Visible_While_Open()
    {
        ShowInWindow(new ShowCaseZoomOverlay(), overlay =>
        {
            var root = FindRootBorder(overlay);
            root.IsVisible.ShouldBeFalse();

            overlay.IsOpen = true;
            Dispatcher.UIThread.RunJobs();

            root.IsVisible.ShouldBeTrue();
        });
    }

    [Fact]
    public void StageContent_Is_Hosted_By_The_Stage_Panel()
    {
        var content = new Border();
        ShowInWindow(new ShowCaseZoomOverlay { StageContent = content, IsOpen = true }, overlay =>
        {
            var panel = overlay.GetVisualDescendants()
                               .OfType<ShowCaseZoomStagePanel>()
                               .Single(candidate => candidate.Name == "PART_StagePanel");
            panel.Children.ShouldHaveSingleItem();
            panel.Children[0].ShouldBeSameAs(content);
        });
    }

    [Fact]
    public void Stage_Centers_Fixed_Size_Content_Without_Any_Scaling()
    {
        var content = new Border { Width = 100, Height = 50 };
        ShowInWindow(new ShowCaseZoomOverlay { StageContent = content, IsOpen = true }, overlay =>
        {
            var stageRoot = FindStageRoot(overlay);
            Dispatcher.UIThread.RunJobs();

            stageRoot.Bounds.Width.ShouldBeGreaterThan(200);
            stageRoot.Bounds.Height.ShouldBeGreaterThan(200);

            // 零缩放：内容按自然尺寸水平垂直居中，舞台对内容不施加任何变换
            content.Bounds.Width.ShouldBe(100, 0.5);
            content.Bounds.Height.ShouldBe(50, 0.5);
            content.Bounds.X.ShouldBe((stageRoot.Bounds.Width - 100) / 2, 0.5);
            content.Bounds.Y.ShouldBe((stageRoot.Bounds.Height - 50) / 2, 0.5);
            content.RenderTransform.ShouldBeNull();
        });
    }

    [Fact]
    public void Stage_Lets_Fill_Affine_Content_Fill_The_Stage_Width()
    {
        // 铺满型内容（期望宽度跟随所给宽度，如 24 栅格、表格）：以舞台宽度重新布局并铺满
        var content = new FluidControl { Height = 40 };
        ShowInWindow(new ShowCaseZoomOverlay { StageContent = content, IsOpen = true }, overlay =>
        {
            var stageRoot = FindStageRoot(overlay);
            Dispatcher.UIThread.RunJobs();

            content.Bounds.Width.ShouldBe(stageRoot.Bounds.Width, 0.5);
            content.Bounds.X.ShouldBe(0, 0.5);
            content.RenderTransform.ShouldBeNull();
        });
    }

    [Fact]
    public void Stage_Does_Not_Apply_Any_RenderTransform_To_The_Content()
    {
        var ownTransform = new RotateTransform(15);
        var content = new Border { Width = 100, Height = 50, RenderTransform = ownTransform };
        ShowInWindow(new ShowCaseZoomOverlay { StageContent = content, IsOpen = true }, overlay =>
        {
            Dispatcher.UIThread.RunJobs();

            content.RenderTransform.ShouldBeSameAs(ownTransform);

            // 舞台内不存在承载缩放的包裹层：零缩放机制，防止回归
            overlay.GetVisualDescendants()
                   .OfType<Avalonia.Controls.Grid>()
                   .ShouldNotContain(grid => grid.Name == "PART_ZoomTransformHost");
        });
    }

    [Fact]
    public void Description_Is_Displayed_At_The_Card_Bottom()
    {
        ShowInWindow(
            new ShowCaseZoomOverlay { IsOpen = true, Description = "demo description" },
            overlay =>
            {
                var description = overlay.GetVisualDescendants()
                                         .OfType<TextBlock>()
                                         .Single(text => text.Name == "PART_DescriptionText");
                description.Text.ShouldBe("demo description");
                description.IsVisible.ShouldBeTrue();
            });
    }

    [Fact]
    public void Card_Border_Fills_The_Root_Edge_To_Edge()
    {
        ShowInWindow(new ShowCaseZoomOverlay { IsOpen = true }, overlay =>
        {
            var root = overlay.GetVisualDescendants()
                              .OfType<Border>()
                              .Single(border => border.Name == "PART_RootBorder");
            var card = overlay.GetVisualDescendants()
                              .OfType<Border>()
                              .Single(border => border.Name == "PART_CardBorder");
            Dispatcher.UIThread.RunJobs();

            // 贴边铺满：卡片容器与蒙层根同尺寸，四周无可见间距
            card.Bounds.Width.ShouldBe(root.Bounds.Width, 0.5);
            card.Bounds.Height.ShouldBe(root.Bounds.Height, 0.5);
            card.Bounds.X.ShouldBe(0, 0.5);
            card.Bounds.Y.ShouldBe(0, 0.5);
        });
    }

    [Fact]
    public void Escape_Key_Raises_CloseRequested_While_Open()
    {
        ShowInWindow(new ShowCaseZoomOverlay { IsOpen = true }, overlay =>
        {
            var requested = 0;
            overlay.CloseRequested += (_, _) => requested++;

            overlay.RaiseEvent(new KeyEventArgs
            {
                RoutedEvent = InputElement.KeyDownEvent,
                Key         = Key.Escape,
                Source      = overlay
            });
            Dispatcher.UIThread.RunJobs();

            requested.ShouldBe(1);
        });
    }

    [Fact]
    public void Close_Button_Click_Raises_CloseRequested()
    {
        ShowInWindow(new ShowCaseZoomOverlay { IsOpen = true }, overlay =>
        {
            var requested = 0;
            overlay.CloseRequested += (_, _) => requested++;

            var closeButton = FindCloseButton(overlay);
            closeButton.ShouldNotBeNull();
            closeButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent) { Source = closeButton });
            Dispatcher.UIThread.RunJobs();

            requested.ShouldBe(1);
        });
    }

    private static Border FindRootBorder(ShowCaseZoomOverlay overlay)
    {
        return overlay.GetVisualDescendants()
                      .OfType<Border>()
                      .Single(border => border.Name == "PART_RootBorder");
    }

    private static Grid FindStageRoot(ShowCaseZoomOverlay overlay)
    {
        return overlay.GetVisualDescendants()
                      .OfType<Grid>()
                      .Single(grid => grid.Name == "PART_StageRoot");
    }

    private static IconButton? FindCloseButton(ShowCaseZoomOverlay overlay)
    {
        return overlay.GetVisualDescendants()
                      .OfType<IconButton>()
                      .FirstOrDefault(button => button.Name == "PART_CloseButton");
    }

    private static void ShowInWindow(Control content, Action<ShowCaseZoomOverlay> assertion)
    {
        var window = new AvaloniaWindow { Width = 500, Height = 500, Content = content };
        try
        {
            window.Show();
            content.ApplyTemplate();
            Dispatcher.UIThread.RunJobs();
            assertion((ShowCaseZoomOverlay)content);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private sealed class FluidControl : Control
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            var width = double.IsInfinity(availableSize.Width) || double.IsNaN(availableSize.Width)
                ? 0
                : availableSize.Width;
            return new Size(width, Height);
        }
    }
}
