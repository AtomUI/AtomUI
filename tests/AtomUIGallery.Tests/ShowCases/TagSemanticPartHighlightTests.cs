using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;
using TagShowCase = AtomUIGallery.ShowCases.Tag.TagShowCase;
using TagViewModel = AtomUIGallery.ShowCases.Tag.TagViewModel;
using AtomUIWindow = AtomUI.Desktop.Controls.Window;

namespace AtomUIGallery.Tests.ShowCases;

public class TagSemanticPartHighlightTests
{
    [Fact]
    public void Tag_Semantic_Previews_Highlight_Both_Owner_Part_Sets()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new TagShowCase
        {
            DataContext = new TagViewModel(new TagTestScreen())
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

            var previews = page.GetVisualDescendants()
                               .OfType<SemanticPartPreview>()
                               .ToArray();
            previews.Length.ShouldBe(2);

            var tagPreview = previews.Single(static preview => preview.Name == "TagSemanticPreview");
            var tagOwners = tagPreview.GetVisualDescendants()
                                      .OfType<AtomUI.Desktop.Controls.Tag>()
                                      .ToArray();
            tagOwners.Length.ShouldBe(1);

            var tagCards = GetPartCards(tagPreview);
            tagCards.Length.ShouldBe(4);

            // Hovering each part card highlights the single tag part node.
            HoverCard(tagCards, window, "root");
            GetHighlightCount(window).ShouldBe(1);
            HoverCard(tagCards, window, "icon");
            GetHighlightCount(window).ShouldBe(1);
            HoverCard(tagCards, window, "content");
            GetHighlightCount(window).ShouldBe(1);
            HoverCard(tagCards, window, "close");
            GetHighlightCount(window).ShouldBe(1);

            var groupPreview = previews.Single(static preview => preview.Name == "CheckableTagGroupSemanticPreview");
            var groupOwners = groupPreview.GetVisualDescendants()
                                          .OfType<AtomUI.Desktop.Controls.CheckableTagGroup>()
                                          .ToArray();
            groupOwners.Length.ShouldBe(1);

            var groupCards = GetPartCards(groupPreview);
            groupCards.Length.ShouldBe(2);

            // The item part highlights all four checkable tag containers.
            HoverCard(groupCards, window, "root");
            GetHighlightCount(window).ShouldBe(1);
            HoverCard(groupCards, window, "item");
            GetHighlightCount(window).ShouldBe(4);

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

internal sealed class TagTestScreen : IScreen
{
    public RoutingState Router { get; } = new();
}
