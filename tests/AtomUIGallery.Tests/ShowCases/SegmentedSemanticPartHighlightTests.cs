using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;
using SegmentedShowCase = AtomUIGallery.ShowCases.Segmented.SegmentedShowCase;
using SegmentedViewModel = AtomUIGallery.ShowCases.Segmented.SegmentedViewModel;
using AtomUIWindow = AtomUI.Desktop.Controls.Window;

namespace AtomUIGallery.Tests.ShowCases;

public class SegmentedSemanticPartHighlightTests
{
    [Fact]
    public void Segmented_Semantic_Preview_Highlights_Parts_Across_All_Owner_Instances()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new SegmentedShowCase
        {
            DataContext = new SegmentedViewModel(new SegmentedTestScreen())
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

            var preview = page.GetVisualDescendants()
                              .OfType<SemanticPartPreview>()
                              .Single(static candidate => candidate.Name == "SegmentedSemanticPreview");
            var owners = preview.GetVisualDescendants()
                                .OfType<AtomUI.Desktop.Controls.Segmented>()
                                .ToArray();
            owners.Length.ShouldBe(2);
            owners.ShouldContain(static owner => owner.Orientation == Avalonia.Layout.Orientation.Vertical);

            var partsPane = preview.GetVisualDescendants()
                                   .OfType<Border>()
                                   .First(static border => border.Name == "PART_PartsPane");
            var cards = partsPane.GetVisualDescendants()
                                 .OfType<UserControl>()
                                 .Where(static card => card.GetType().Name.Contains("SemanticPartPreviewItem"))
                                 .ToArray();
            cards.Length.ShouldBe(4);

            // Hovering the root part card highlights both Segmented instances.
            HoverCard(cards, window, "root");
            GetHighlightCount(window).ShouldBe(2);

            // Hovering the item part card highlights all six item containers.
            HoverCard(cards, window, "item");
            GetHighlightCount(window).ShouldBe(6);

            // Hovering the icon part card highlights all six icon presenters.
            HoverCard(cards, window, "icon");
            GetHighlightCount(window).ShouldBe(6);

            // Hovering the label part card highlights every visible label presenter:
            // five in total — the icon-only item keeps its collapsed label out of the highlight.
            HoverCard(cards, window, "label");
            GetHighlightCount(window).ShouldBe(5);

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

internal sealed class SegmentedTestScreen : IScreen
{
    public RoutingState Router { get; } = new();
}
