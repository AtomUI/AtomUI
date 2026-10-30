using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;
using DrawerShowCase = AtomUIGallery.ShowCases.Drawer.DrawerShowCase;
using DrawerViewModel = AtomUIGallery.ShowCases.Drawer.DrawerViewModel;
using AtomUIWindow = AtomUI.Desktop.Controls.Window;

namespace AtomUIGallery.Tests.ShowCases;

public class DrawerSemanticPreviewStageGeometryTests
{
    // 回归一：ScopeAwareAdornerLayer 的追踪快照尺寸曾取 Math.Max(Bounds, DesiredSize)，
    // 而 DesiredSize 包含 Margin，会把容器放大成 margin box——遮罩与面板越出
    // 舞台可见边界（Footer 在舞台底边之下被裁）。容器尺寸必须贴合宿主的排列盒。
    //
    // 回归二：预览页签处于限高钳制模式时舞台画布随宿主收缩；舞台曾用固定
    // Height=320 且声明的 PreviewStageMinHeight 地板（360）没有覆盖舞台内容
    // 的真实期望（320+48 边距），舞台连同层内的遮罩/面板被视口裁掉，而高亮
    // adorner 按未裁剪 bounds 画框伸出可见区。舞台必须用 MinHeight+拉伸自适应，
    // 且 PreviewStageMinHeight（434）必须覆盖舞台内容期望（视口 = 地板 - 62 开销）。
    [Theory]
    [InlineData(960)]
    [InlineData(820)]
    [InlineData(700)]
    public void Semantic_Stage_Drawer_Stays_Inside_The_Visible_Stage_Box(int windowHeight)
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new DrawerShowCase { DataContext = new DrawerViewModel(new DrawerStageGeometryTestScreen()) };
        var window = new AtomUIWindow { Width = 1300, Height = windowHeight, Content = page };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            Rect InWindow(Visual v)
            {
                var topLeft = v.TranslatePoint(new Point(0, 0), window).ShouldNotBeNull();
                return new Rect(topLeft, v.Bounds.Size);
            }

            var stage = page.GetVisualDescendants().OfType<Border>()
                            .Single(b => b.Name == "DrawerSemanticStage");
            var stageBox = InWindow(stage);

            var container = window.GetVisualDescendants()
                                  .Single(v => v.GetType().Name == "DrawerContainer");
            var mask  = container.GetVisualDescendants().OfType<Border>()
                                 .Single(b => b.Name == "PART_Mask");
            var frame = container.GetVisualDescendants().OfType<Border>()
                                 .Single(b => b.Name == "Frame");

            // 舞台内容不得超出舞台视口（限高钳制下也不允许裁剪）。
            var stageScp = (ScrollContentPresenter)stage.GetVisualAncestors()
                               .First(a => a is ScrollContentPresenter);
            (stageScp.Extent.Height - stageScp.Bounds.Height).ShouldBe(0, 0.5,
                "舞台内容（含层内遮罩/面板）不得超出舞台视口，否则高亮框会伸出可见区外");

            var containerBox = InWindow(container);
            containerBox.X.ShouldBe(stageBox.X, 1, "容器左缘必须贴合舞台可见盒（Margin 不得放大容器）");
            containerBox.Y.ShouldBe(stageBox.Y, 1, "容器上缘必须贴合舞台可见盒（Margin 不得放大容器）");
            containerBox.Width.ShouldBe(stageBox.Width, 1);
            containerBox.Height.ShouldBe(stageBox.Height, 1);

            InWindow(mask).ShouldBe(containerBox);
            var frameBox = InWindow(frame);
            frameBox.Width.ShouldBe(300, 1, "面板宽度 = DialogSize");
            frameBox.Right.ShouldBe(stageBox.Right, 1, "右置面板右缘必须贴合舞台右缘");
            frameBox.Bottom.ShouldBe(stageBox.Bottom, 1, "面板底缘（Footer 所在）必须贴合舞台底缘，不得越出被裁");
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}

internal sealed class DrawerStageGeometryTestScreen : IScreen
{
    public RoutingState Router { get; } = new();
}
