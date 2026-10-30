using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;
using DrawerShowCase = AtomUIGallery.ShowCases.Drawer.DrawerShowCase;
using DrawerViewModel = AtomUIGallery.ShowCases.Drawer.DrawerViewModel;
using AtomUIWindow = AtomUI.Desktop.Controls.Window;

namespace AtomUIGallery.Tests.ShowCases;

public class DrawerSemanticPartHighlightTests
{
    [Fact]
    public void Drawer_Semantic_Preview_Highlights_All_Parts_Inside_The_Inline_Stage()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new DrawerShowCase { DataContext = new DrawerViewModel(new DrawerTestScreen()) };
        var window = new AtomUIWindow { Width = 1280, Height = 900, Content = page };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            var preview = page.GetVisualDescendants().OfType<SemanticPartPreview>()
                              .Single(p => p.Name == "DrawerSemanticPreview");
            var owner = preview.GetVisualDescendants()
                               .OfType<AtomUI.Desktop.Controls.Drawer>()
                               .Single(d => d.Name == "DrawerSemanticOwner");
            owner.IsOpen.ShouldBeTrue("进入语义页签后，舞台内抽屉必须保持常开");
            owner.GetCrossRoots().ShouldHaveSingleItem();

            var partsPane = preview.GetVisualDescendants().OfType<Border>()
                                   .First(b => b.Name == "PART_PartsPane");
            var cards = partsPane.GetVisualDescendants().OfType<UserControl>()
                                 .Where(c => c.GetType().Name.Contains("SemanticPartPreviewItem"))
                                 .ToArray();
            cards.Length.ShouldBe(9);

            foreach (var path in new[]
                     {
                         "root", "mask", "section", "header",
                         "title", "extra", "body", "footer", "close"
                     })
            {
                HoverCard(cards, window, path);
                GetHighlightCount(window).ShouldBe(1);
            }

            // 钉住常开：语义页签内点击遮罩不关闭。
            var mask = window.GetVisualDescendants().OfType<Border>()
                             .Single(b => b.Name == "PART_Mask");
            RaisePointerReleased(mask);
            Dispatcher.UIThread.RunJobs();
            owner.IsOpen.ShouldBeTrue("钉住的语义预览抽屉必须忽略遮罩点击");

            host.SelectedTab = GalleryShowCaseTab.Examples;
            Dispatcher.UIThread.RunJobs();
            GetHighlightCount(window).ShouldBe(0);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static int GetHighlightCount(AtomUIWindow window)
    {
        return window.GetVisualDescendants()
                     .Count(v => v.GetType().Name == "SemanticPartAdorner");
    }

    private static void HoverCard(UserControl[] cards, AtomUIWindow window, string path)
    {
        var card = cards.Single(c =>
            (string?)c.DataContext?.GetType().GetProperty("Path")!.GetValue(c.DataContext) == path);
        card.BringIntoView();
        Dispatcher.UIThread.RunJobs();
        var center = card.TransformToVisual(window)!.Value
                         .Transform(new Point(card.Bounds.Width / 2, card.Bounds.Height / 2));
        window.MouseMove(new Point(center.X, center.Y));
        Dispatcher.UIThread.RunJobs();
        Dispatcher.UIThread.RunJobs();
        Dispatcher.UIThread.RunJobs();
    }

    private static void RaisePointerReleased(Control source)
    {
        source.RaiseEvent(new PointerReleasedEventArgs(
            source,
            new Pointer(Pointer.GetNextFreeId(), PointerType.Mouse, true),
            source,
            default,
            0,
            new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.LeftButtonReleased),
            KeyModifiers.None,
            MouseButton.Left));
    }
}

internal sealed class DrawerTestScreen : IScreen
{
    public RoutingState Router { get; } = new();
}
