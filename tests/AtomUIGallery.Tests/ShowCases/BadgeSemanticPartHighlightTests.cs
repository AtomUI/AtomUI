using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;
using BadgeShowCase = AtomUIGallery.ShowCases.Badge.BadgeShowCase;
using BadgeViewModel = AtomUIGallery.ShowCases.Badge.BadgeViewModel;
using AtomUIWindow = AtomUI.Desktop.Controls.Window;

namespace AtomUIGallery.Tests.ShowCases;

public class BadgeSemanticPartHighlightTests
{
    [Fact]
    public void Badge_Semantic_Owners_And_Root_Markers_Wrap_The_Decorated_Target()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new BadgeShowCase
        {
            DataContext = new BadgeViewModel(new BadgeTestScreen())
        };

        var window = new AtomUIWindow
        {
            Width = 1280,
            Height = 900,
            Content = page
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();

            var countOwner = page.GetVisualDescendants()
                                 .OfType<AtomUI.Desktop.Controls.CountBadge>()
                                 .Single(static badge => badge.Name == "CountBadgeSemanticOwner");
            countOwner.Bounds.Width.ShouldBe(96, 0.001,
                "the CountBadge semantic owner must wrap its 96px preview card instead of stretching across the stage.");
            countOwner.Bounds.Height.ShouldBe(64, 0.001);

            var dotOwner = page.GetVisualDescendants()
                               .OfType<AtomUI.Desktop.Controls.DotBadge>()
                               .Single(static badge => badge.Name == "DotBadgeSemanticOwner");
            dotOwner.Bounds.Width.ShouldBe(96, 0.001,
                "the DotBadge semantic owner must wrap its 96px preview card instead of stretching across the stage.");

            var ribbonOwner = page.GetVisualDescendants()
                                  .OfType<AtomUI.Desktop.Controls.RibbonBadge>()
                                  .Single(static badge => badge.Name == "RibbonBadgeSemanticOwner");
            ribbonOwner.Bounds.Width.ShouldBe(180, 0.001,
                "the RibbonBadge semantic owner must wrap its 180px preview card instead of stretching across the stage.");

            // 指示器红点必须锚定在卡片右上角：runtime adorner 的舞台尺寸等于包裹后的
            // badge 尺寸（96 宽），私有动效表面在其中右对齐。AdornerLayer 的窗口级
            // 粘贴由合成器 AdornedVisual 跟踪，headless 下只断言布局层事实。
            var runtimeAdorner = AdornerLayer.GetAdornerLayer(countOwner)!.Children
                                             .Single(child => ReferenceEquals(
                                                 AdornerLayer.GetAdornedElement(child), countOwner));
            runtimeAdorner.Bounds.Width.ShouldBe(96, 0.001,
                "the runtime adorner stage must equal the wrapped badge size, anchoring the indicator at the card corner.");
            var indicator = runtimeAdorner.GetVisualDescendants()
                                          .Single(static visual =>
                                              visual.Classes.Contains("semantic-indicator"));
            // The semantic part is inside its private motion boundary. Compare the
            // same rendered rectangle in adorner coordinates rather than its parent-local Bounds.
            var indicatorOrigin = indicator.TranslatePoint(default, runtimeAdorner)!.Value;
            var indicatorBounds = new Rect(indicatorOrigin, indicator.Bounds.Size);
            indicatorBounds.Right.ShouldBe(runtimeAdorner.Bounds.Width + 10, 1,
                "the count indicator must sit at the top-right corner of the wrapped card.");
            indicatorBounds.Top.ShouldBe(-10, 1);

            var partsPane = page.GetVisualDescendants()
                                .OfType<Border>()
                                .First(static border => border.Name == "PART_PartsPane");
            var cards = partsPane.GetVisualDescendants()
                                 .OfType<UserControl>()
                                 .Where(static card => card.GetType().Name.Contains("SemanticPartPreviewItem"))
                                 .ToArray();

            HoverCard(cards, window, "root");
            var rootMarker = window.GetVisualDescendants()
                                   .OfType<Control>()
                                   .Single(static visual => visual.GetType().Name == "SemanticPartAdorner");
            rootMarker.Bounds.Width.ShouldBeLessThan(120,
                "the root marker must outline the wrapped owner (96px card), not the stretched stage slot.");
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static void HoverCard(
        UserControl[] cards,
        AtomUIWindow window,
        string path)
    {
        var card = cards.Single(candidate =>
            (string?)candidate.DataContext?.GetType().GetProperty("Path")!.GetValue(candidate.DataContext) == path);
        card.BringIntoView();
        Dispatcher.UIThread.RunJobs();
        var center = card.TransformToVisual(window)!.Value
                         .Transform(new Point(card.Bounds.Width / 2, card.Bounds.Height / 2));
        window.MouseMove(new Point(center.X, center.Y));
        Dispatcher.UIThread.RunJobs();
        Dispatcher.UIThread.RunJobs();
        Dispatcher.UIThread.RunJobs();
    }
}

internal sealed class BadgeTestScreen : IScreen
{
    public RoutingState Router { get; } = new();
}
