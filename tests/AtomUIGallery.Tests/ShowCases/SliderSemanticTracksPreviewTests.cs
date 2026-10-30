using System.Collections;
using System.Reflection;
using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;
using SliderShowCase = AtomUIGallery.ShowCases.Slider.SliderShowCase;
using SliderViewModel = AtomUIGallery.ShowCases.Slider.SliderViewModel;

namespace AtomUIGallery.Tests.ShowCases;

public class SliderSemanticTracksPreviewTests
{
    [Fact]
    public void Tracks_Part_Highlights_The_First_To_Last_Value_Span_With_A_Visible_Adorner()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new SliderShowCase
        {
            DataContext = new SliderViewModel(new TestScreen())
        };

        var window = new AvaloniaWindow
        {
            Width  = 1280,
            Height = 900,
            Content = new VisualLayerManager { Child = page }
        };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();

            var preview = page.GetVisualDescendants()
                              .OfType<SemanticPartPreview>()
                              .Single(static candidate => candidate.Name == "SliderSemanticPreview");
            var owner = preview.SemanticOwner.ShouldNotBeNull();

            var rail = owner.GetVisualDescendants()
                            .OfType<Border>()
                            .Single(static candidate => candidate.Classes.Contains("semantic-rail"));
            var tracks = owner.GetVisualDescendants()
                              .OfType<Border>()
                              .Single(static candidate => candidate.Classes.Contains("semantic-tracks"));

            rail.Bounds.Width.ShouldBeGreaterThan(0);
            rail.Bounds.Height.ShouldBeGreaterThan(0);

            // antd `.ant-slider-tracks`：多段轨道容器横跨首值到末值。
            // range [20, 30, 50] → 从 20% 处开始、覆盖 30% 的轨道长度。
            // 容差覆盖 Avalonia 布局对整像素的取整：轨道起止边缘各自独立取整，
            // 宽度相对 rail × 0.3 的偏差可达 ±1px（页面是否出现滚动条会使内容
            // 宽度相差一个滚动条宽，rail 宽度随之平移一个取整边界）。
            tracks.Bounds.X.ShouldBe(rail.Bounds.X + rail.Bounds.Width * 0.2, 1.0);
            tracks.Bounds.Width.ShouldBe(rail.Bounds.Width * 0.3, 1.0);
            tracks.Bounds.Y.ShouldBe(rail.Bounds.Y, 0.6);
            tracks.Bounds.Height.ShouldBe(rail.Bounds.Height, 0.6);

            var tracksItem = GetPreviewItems(preview).Single(static item =>
                (string)item.GetType().GetProperty("Path")!.GetValue(item)! == "tracks");
            TogglePinnedPart(preview, tracksItem);
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            // 高亮会话把唯一目标（tracks 容器）挂到 AdornerLayer 上。主标记不画白色外环，
            // 每边负 Margin 外扩 2px 覆盖 2px 主笔宽的描边区域，adorner 自身尺寸比目标大 4px，
            // 保证描边不被 ClipToBounds 祖先裁剪。
            var layer = AdornerLayer.GetAdornerLayer(tracks).ShouldNotBeNull();
            var adorner = layer.Children
                               .Where(static child => child.GetType().Name == "SemanticPartAdorner")
                               .ToArray();
            adorner.ShouldHaveSingleItem();
            adorner[0].Bounds.Size.ShouldBe(new Size(tracks.Bounds.Width + 4, tracks.Bounds.Height + 4));
            AdornerLayer.GetAdornedElement(adorner[0]).ShouldBeSameAs(tracks);
        }
        finally
        {
            window.Close();
        }
    }

    private static IReadOnlyList<object> GetPreviewItems(SemanticPartPreview preview)
    {
        var itemsValue = typeof(SemanticPartPreview)
                         .GetProperty("Items", BindingFlags.Instance | BindingFlags.NonPublic)!
                         .GetValue(preview);
        return ((IEnumerable)itemsValue!).Cast<object>().ToArray();
    }

    private static void TogglePinnedPart(SemanticPartPreview preview, object item)
    {
        typeof(SemanticPartPreview)
            .GetMethod("TogglePinnedPart", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(preview, [item]);
    }

    private sealed class TestScreen : IScreen
    {
        public RoutingState Router { get; } = new();
    }
}
