using AtomUI.Toolkits.GalleryBase.Controls;
using AtomUIGallery.ShowCases.Steps;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;
using AtomUIWindow = AtomUI.Desktop.Controls.Window;

namespace AtomUIGallery.Tests.ShowCases;

public class StepsSemanticPartHighlightTests
{
    [Fact]
    public void Steps_Semantic_Preview_Highlights_Every_Part_Including_Both_Rails()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new StepsShowCase
        {
            DataContext = new StepsViewModel(new StepsTestScreen())
        };

        var window = new AtomUIWindow
        {
            Width = 1953,
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
                              .Single(static candidate => candidate.Name == "StepsSemanticPreview");
            var partsPane = preview.GetVisualDescendants()
                                   .OfType<Border>()
                                   .First(static border => border.Name == "PART_PartsPane");
            var cards = partsPane.GetVisualDescendants()
                                 .OfType<UserControl>()
                                 .Where(static card => card.GetType().Name.Contains("SemanticPartPreviewItem"))
                                 .ToArray();
            cards.Length.ShouldBe(9);

            // The preview hosts two demos: the default steps and a Panel steps
            // below it. Highlighting covers every owner instance in the preview
            // content, so parts present in both demos light up twice. Panel hides
            // the indicator and the connector, so those parts resolve to the
            // default demo only.
            HoverCard(cards, window, "root");
            GetHighlightCount(window).ShouldBe(2);

            HoverCard(cards, window, "item");
            GetHighlightCount(window).ShouldBe(6);

            HoverCard(cards, window, "itemWrapper");
            GetHighlightCount(window).ShouldBe(6);

            HoverCard(cards, window, "itemIcon");
            GetHighlightCount(window).ShouldBe(3);

            HoverCard(cards, window, "itemTitle");
            GetHighlightCount(window).ShouldBe(6);

            HoverCard(cards, window, "itemSubtitle");
            GetHighlightCount(window).ShouldBe(6);

            HoverCard(cards, window, "itemSection");
            var sectionAdorners = GetAdorners(window);
            sectionAdorners.Length.ShouldBe(6);
            foreach (var adorner in sectionAdorners)
            {
                var target = AdornerLayer.GetAdornedElement(adorner);
                target.ShouldNotBeNull();
                target!.GetType().Name.ShouldBe("StepsItemSectionPanel");
                target.Bounds.Width.ShouldBeGreaterThan(0);
            }

            HoverCard(cards, window, "itemContent");
            GetHighlightCount(window).ShouldBe(6);

            // The three default items share two connector rails: the first item
            // has none before it and the last item has none after it. Panel items
            // hide their connectors, so they contribute no rails.
            HoverCard(cards, window, "itemRail");
            var railAdorners = GetAdorners(window);
            railAdorners.Length.ShouldBe(2);
            foreach (var adorner in railAdorners)
            {
                var target = AdornerLayer.GetAdornedElement(adorner);
                target.ShouldNotBeNull();
                target!.GetType().Name.ShouldBe("PixelAlignedBorder");
                target.Bounds.Width.ShouldBeGreaterThan(0);
            }

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

    private static Visual[] GetAdorners(AtomUIWindow window)
    {
        return window.GetVisualDescendants()
                     .Where(static visual => visual.GetType().Name == "SemanticPartAdorner")
                     .ToArray();
    }

    private static int GetHighlightCount(AtomUIWindow window)
    {
        return GetAdorners(window).Length;
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

internal sealed class StepsTestScreen : IScreen
{
    public RoutingState Router { get; } = new();
}
