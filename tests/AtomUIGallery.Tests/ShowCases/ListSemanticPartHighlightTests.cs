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
using ListShowCase = AtomUIGallery.ShowCases.List.ListShowCase;
using ListViewModel = AtomUIGallery.ShowCases.List.ListViewModel;
using AtomUIWindow = AtomUI.Desktop.Controls.Window;

namespace AtomUIGallery.Tests.ShowCases;

public class ListSemanticPartHighlightTests
{
    [Fact]
    public void List_Semantic_Preview_Hover_Draws_Highlights_For_Every_Part()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new ListShowCase
        {
            DataContext = new ListViewModel(new TestScreen())
        };

        var window = new AtomUIWindow
        {
            Width   = 1280,
            Height  = 900,
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
                              .Single(static candidate => candidate.Name == "ListViewSemanticPreview");
            var listView = preview.SemanticOwner.ShouldBeOfType<AtomUI.Desktop.Controls.ListView>();

            var layer = AdornerLayer.GetAdornerLayer(listView);
            layer.ShouldNotBeNull();
            layer!.Children.ShouldBeEmpty();

            var partsPane = preview.GetVisualDescendants()
                                   .OfType<Border>()
                                   .First(static border => border.Name == "PART_PartsPane");
            var cards = partsPane.GetVisualDescendants()
                                 .OfType<UserControl>()
                                 .Where(static card => card.GetType().Name.Contains("SemanticPartPreviewItem"))
                                 .ToArray();
            cards.Length.ShouldBe(3);

            // Hovering the root part card highlights the ListView itself.
            HoverCard(cards, window, "root");
            layer.Children.ShouldHaveSingleItem();
            layer.Children.ShouldAllBe(static child => child.GetType().Name == "SemanticPartAdorner");

            // Hovering the item part card highlights every realized ListViewItem.
            HoverCard(cards, window, "item");
            layer.Children.Count.ShouldBe(6);
            layer.Children.ShouldAllBe(static child => child.GetType().Name == "SemanticPartAdorner");

            // Hovering the groupHeader part card highlights every realized GroupHeaderItem.
            HoverCard(cards, window, "groupHeader");
            layer.Children.Count.ShouldBe(2);
            layer.Children.ShouldAllBe(static child => child.GetType().Name == "SemanticPartAdorner");

            // Leaving the Semantic Parts tab releases the highlight session.
            host.SelectedTab = GalleryShowCaseTab.Examples;
            Dispatcher.UIThread.RunJobs();
            layer.Children.ShouldBeEmpty();
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
        var center = card.TransformToVisual(window)!.Value
                         .Transform(new Point(card.Bounds.Width / 2, card.Bounds.Height / 2));
        window.MouseMove(new Point(center.X, center.Y));
        Dispatcher.UIThread.RunJobs();
        Dispatcher.UIThread.RunJobs();
        Dispatcher.UIThread.RunJobs();
    }
}

internal sealed class TestScreen : IScreen
{
    public RoutingState Router { get; } = new();
}
