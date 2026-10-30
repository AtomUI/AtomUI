using AtomUI.Desktop.Controls;
using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;
using AvaloniaWindow = AtomUI.Desktop.Controls.Window;
using AtomUIButton = AtomUI.Desktop.Controls.Button;
using TourShowCasePage = AtomUIGallery.ShowCases.Tour.TourShowCase;
using TourShowCaseViewModel = AtomUIGallery.ShowCases.Tour.TourViewModel;

namespace AtomUIGallery.Tests.ShowCases;

// 锁定「自定义语义结构的样式」示例的函数样式按钮定制形态：
// 对齐上游 style-class demo 的 btnProps（prev 紫底白字 / next 白底紫描边紫字），
// 同时锁定用户全局约束的“专用 Style 声明式形态”——禁止退化为代码回退。
public class TourSemanticStylesButtonTests
{
    [Fact]
    public void Function_Tour_Button_Styles_Apply_The_Upstream_BtnProps_Look()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new TourShowCasePage
        {
            DataContext = new TourShowCaseViewModel(new TourButtonTestScreen())
        };

        Tour? tour = null;
        var vlm = new VisualLayerManager { Child = page };
        EnablePopupOverlayLayer(vlm);
        var window = new AvaloniaWindow
        {
            Width = 1280,
            Height = 1000,
            Content = vlm
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var panel = page.GetVisualDescendants().OfType<ShowCasePanel>().Single();
            var item = panel.Children
                            .OfType<ShowCaseItem>()
                            .Last();
            item.MaterializeDeferredContent();
            Dispatcher.UIThread.RunJobs();

            tour = page.GetVisualDescendants()
                       .OfType<Tour>()
                       .Single(t => t.Name == "SemanticStylesFunctionTour");
            tour.IsPopupPinnedOpen = true;
            tour.SetCurrentValue(Tour.CurrentIndexProperty, 1); // Save 步：Middle 显示 Previous + Next
            tour.IsOpen = true;
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            // 弹层在 Popup 的独立视觉根中，经 PART_Popup 的 Child 根下钻。
            var popupRoot = tour.GetVisualDescendants()
                                .OfType<Avalonia.Controls.Primitives.Popup>()
                                .FirstOrDefault()
                                .ShouldNotBeNull("tour popup missing")
                                .Child;
            popupRoot.ShouldNotBeNull();

            var buttons = popupRoot.GetVisualDescendants()
                                   .OfType<AtomUIButton>()
                                   .Where(b => b.Classes.Contains("previous-btn") || b.Classes.Contains("next-btn"))
                                   .ToArray();
            var describe = () => string.Join("; ",
                buttons.Select(b => $"{b.Name} visible={b.IsVisible}"));

            var prev = buttons.FirstOrDefault(b => b.Classes.Contains("previous-btn") && b.IsVisible);
            var next = buttons.FirstOrDefault(b => b.Classes.Contains("next-btn")
                                                   && b.IsVisible && b.Name == "NextButton");
            prev.ShouldNotBeNull($"previous-btn not visible; buttons: {describe()}");
            next.ShouldNotBeNull($"next-btn not visible; buttons: {describe()}");

            (prev.Background as ISolidColorBrush)?.Color.ShouldBe(Color.Parse("#CDC1FF"));
            (prev.Foreground as ISolidColorBrush)?.Color.ShouldBe(Colors.White);
            (next.Background as ISolidColorBrush)?.Color.ShouldBe(Colors.White);
            (next.Foreground as ISolidColorBrush)?.Color.ShouldBe(Color.Parse("#CDC1FF"));
            (next.BorderBrush as ISolidColorBrush)?.Color.ShouldBe(Color.Parse("#CDC1FF"));

            // hover 态对齐上游：demo 内联色不变 + tour primary 的 :hover css 叠加 ——
            // prev 仅边框变化（白@15% → transparent），next 仅背景变化（PrimaryNextBtnHoverBg）。
            // owner 作用域的无激活 Setter 会盖过 ControlTheme 的激活式 Setter，
            // 伪类态必须在示例 Styles 中显式声明，这里锁定该声明形态。
            ((IPseudoClasses)prev.Classes).Set(":pointerover", true);
            Dispatcher.UIThread.RunJobs();
            (prev.Background as ISolidColorBrush)?.Color.ShouldBe(Color.Parse("#CDC1FF"));
            (prev.Foreground as ISolidColorBrush)?.Color.ShouldBe(Colors.White);
            (prev.BorderBrush as ISolidColorBrush)?.Color.ShouldBe(Colors.Transparent);

            ((IPseudoClasses)next.Classes).Set(":pointerover", true);
            Dispatcher.UIThread.RunJobs();
            var nextHoverBg = (next.Background as ISolidColorBrush)?.Color;
            nextHoverBg.ShouldNotBeNull();
            nextHoverBg!.Value.A.ShouldBe((byte)255);
            nextHoverBg!.Value.ShouldNotBe(Color.Parse("#FFFFFF")); // 必须真的变深：ColorBgTextHover 叠白
            (next.Foreground as ISolidColorBrush)?.Color.ShouldBe(Color.Parse("#CDC1FF"));
            (next.BorderBrush as ISolidColorBrush)?.Color.ShouldBe(Color.Parse("#CDC1FF"));

            // 遮罩：#73000000（0.45 对齐上游 colorBgMask），且不得再叠加 Opacity 覆盖
            // （Opacity 0.5 × alpha 0.30 = 实际 0.15，页面几乎不暗，曾偏离上游视觉）。
            var mask = vlm.GetVisualDescendants()
                          .OfType<Control>()
                          .FirstOrDefault(c => c.Classes.Contains("semantic-popup-mask") && c.IsVisible);
            mask.ShouldNotBeNull("semantic-popup-mask layer not visible");
            mask.Opacity.ShouldBe(1.0);
            // 遮罩层是 internal TourLayer，颜色经 CurrentMaskColor 中继到其 Background。
            var maskBrush = mask.GetType().GetProperty("Background")!.GetValue(mask) as ISolidColorBrush;
            maskBrush.ShouldNotBeNull();
            maskBrush.Color.ShouldBe(Color.Parse("#73000000"));
        }
        finally
        {
            if (tour is not null)
            {
                tour.IsOpen = false;
            }
            window.Close();
            Dispatcher.UIThread.RunJobs();
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

    private sealed class TourButtonTestScreen : IScreen
    {
        public RoutingState Router { get; } = new();
    }
}
