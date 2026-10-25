using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;
using TimelineShowCase = AtomUIGallery.ShowCases.Timeline.TimelineShowCase;
using TimelineViewModel = AtomUIGallery.ShowCases.Timeline.TimelineViewModel;
using AtomUIWindow = AtomUI.Desktop.Controls.Window;

namespace AtomUIGallery.Tests.ShowCases;

public class TimelineSemanticPartHighlightTests
{
    [Fact]
    public void Timeline_Semantic_Preview_Highlights_The_Nine_Part_Set()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new TimelineShowCase
        {
            DataContext = new TimelineViewModel(new TimelineTestScreen())
        };

        var window = new AtomUIWindow
        {
            Width  = 1280,
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

            var previews = page.GetVisualDescendants()
                               .OfType<SemanticPartPreview>()
                               .ToArray();
            previews.Length.ShouldBe(2);

            var preview = previews.Single(static candidate => candidate.Name == "TimelineSemanticPreview");
            var owners = preview.GetVisualDescendants()
                                .OfType<AtomUI.Desktop.Controls.Timeline>()
                                .ToArray();
            owners.Length.ShouldBe(1);

            var itemsPreview = previews.Single(static candidate => candidate.Name == "TimelineItemsSemanticPreview");
            itemsPreview.GetVisualDescendants()
                        .OfType<AtomUI.Desktop.Controls.Timeline>()
                        .ToArray()
                        .Length.ShouldBe(1);

            var cards = GetPartCards(preview);
            cards.Length.ShouldBe(9);

            HoverCard(cards, window, "root");
            GetHighlightCount(window).ShouldBe(1);
            HoverCard(cards, window, "item");
            GetHighlightCount(window).ShouldBe(4);
            HoverCard(cards, window, "itemWrapper");
            GetHighlightCount(window).ShouldBe(4);
            HoverCard(cards, window, "itemIcon");
            GetHighlightCount(window).ShouldBe(4);
            HoverCard(cards, window, "itemSection");
            GetHighlightCount(window).ShouldBe(4);
            HoverCard(cards, window, "itemHeader");
            GetHighlightCount(window).ShouldBe(4);
            HoverCard(cards, window, "itemTitle");
            GetHighlightCount(window).ShouldBe(3);
            HoverCard(cards, window, "itemContent");
            GetHighlightCount(window).ShouldBe(4);
            HoverCard(cards, window, "itemRail");
            GetHighlightCount(window).ShouldBe(3);

            var itemsCards = GetPartCards(itemsPreview);
            itemsCards.Length.ShouldBe(9);

            HoverCard(itemsCards, window, "root");
            GetHighlightCount(window).ShouldBe(1);
            HoverCard(itemsCards, window, "item");
            GetHighlightCount(window).ShouldBe(2);
            HoverCard(itemsCards, window, "itemWrapper");
            GetHighlightCount(window).ShouldBe(2);
            HoverCard(itemsCards, window, "itemIcon");
            GetHighlightCount(window).ShouldBe(2);
            HoverCard(itemsCards, window, "itemSection");
            GetHighlightCount(window).ShouldBe(2);
            HoverCard(itemsCards, window, "itemHeader");
            GetHighlightCount(window).ShouldBe(2);
            HoverCard(itemsCards, window, "itemTitle");
            GetHighlightCount(window).ShouldBe(2);
            HoverCard(itemsCards, window, "itemContent");
            GetHighlightCount(window).ShouldBe(2);
            HoverCard(itemsCards, window, "itemRail");
            GetHighlightCount(window).ShouldBe(1);

            // Leaving the Semantic Parts tab releases the highlight session.
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

    private static UserControl[] GetPartCards(SemanticPartPreview preview)
    {
        var partsPane = preview.GetVisualDescendants()
                               .OfType<Border>()
                               .First(static border => border.Name == "PART_PartsPane");
        return partsPane.GetVisualDescendants()
                        .OfType<UserControl>()
                        .Where(static card => card.GetType().Name.Contains("SemanticPartPreviewItem"))
                        .ToArray();
    }

    private static int GetHighlightCount(AtomUIWindow window)
    {
        return window.GetVisualDescendants()
                     .Count(static visual => visual.GetType().Name == "SemanticPartAdorner");
    }

    private static void HoverCard(
        UserControl[] cards,
        AtomUIWindow window,
        string path)
    {
        var card = cards.Single(candidate =>
            (string?)candidate.DataContext?.GetType().GetProperty("Path")!.GetValue(candidate.DataContext) == path);
        // Nested panes consume the card request; first reveal the enclosing preview.
        var preview = card.GetVisualAncestors().OfType<SemanticPartPreview>().First();
        var stickyContent = window.GetVisualDescendants().OfType<Control>()
            .Single(candidate => candidate.Name == "PART_StickyContentHost");
        var stickyHeight = stickyContent.Bounds.Height;
        preview.BringIntoView(new Rect(0, -stickyHeight, preview.Bounds.Width, preview.Bounds.Height + stickyHeight));
        Dispatcher.UIThread.RunJobs();
        card.BringIntoView();
        Dispatcher.UIThread.RunJobs();
        var center = card.TransformToVisual(window)!.Value
                         .Transform(new Point(card.Bounds.Width / 2, card.Bounds.Height / 2));
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        Dispatcher.UIThread.RunJobs();
        var hit = window.InputHitTest(center) as Visual;
        var hitCard = hit == card || hit?.GetVisualAncestors().Contains(card) == true;
        var failure = $"part '{path}' card must be visible and hit before hover";
        if (!hitCard)
        {
            var after = card.TransformToVisual(window)!.Value.Transform(new Point(card.Bounds.Width / 2, card.Bounds.Height / 2));
            var afterHit = window.InputHitTest(after) as Visual;
            object Node(Visual visual) => new
            {
                type = visual.GetType().FullName, name = (visual as Control)?.Name,
                bounds = visual.Bounds.ToString(), matrix = visual.TransformToVisual(window)?.ToString(),
                visual.ClipToBounds, clip = visual.Clip?.ToString(), visual.IsVisible, visual.Opacity,
                hitTest = (visual as Control)?.IsHitTestVisible,
                viewport = visual is ScrollViewer scroll ? $"{scroll.Viewport}|{scroll.Extent}|{scroll.Offset}" : null
            };
            failure += Environment.NewLine + System.Text.Json.JsonSerializer.Serialize(new
            {
                path, before = center.ToString(), after = after.ToString(),
                scale = window.RenderScaling, client = window.ClientSize.ToString(),
                theme = window.ActualThemeVariant.ToString(),
                card = Node(card), sticky = Node(stickyContent),
                hit = hit is null ? null : Node(hit), afterHit = afterHit is null ? null : Node(afterHit),
                ancestors = card.GetVisualAncestors().Select(Node).ToArray(),
                logicalAncestors = card.GetLogicalAncestors().Select(static node => node.GetType().FullName).ToArray(),
                hitAncestors = hit?.GetVisualAncestors().Select(Node).ToArray()
            });
        }
        hitCard.ShouldBeTrue(failure);
        window.MouseMove(new Point(center.X, center.Y));
        Dispatcher.UIThread.RunJobs();
        Dispatcher.UIThread.RunJobs();
        Dispatcher.UIThread.RunJobs();
    }
}

internal sealed class TimelineTestScreen : IScreen
{
    public RoutingState Router { get; } = new();
}
