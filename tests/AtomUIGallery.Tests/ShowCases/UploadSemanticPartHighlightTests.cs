using AtomUI.Toolkits.GalleryBase.Controls;
using AtomUIGallery.ShowCases.Upload;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;
using AtomUIWindow = AtomUI.Desktop.Controls.Window;

namespace AtomUIGallery.Tests.ShowCases;

public class UploadSemanticPartHighlightTests
{
    [Fact]
    public void Upload_Semantic_Preview_Highlights_The_Root_List_And_Realized_File_Items()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new UploadShowCase
        {
            DataContext = new UploadViewModel(new UploadTestScreen())
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
                              .Single(static candidate => candidate.Name == "UploadSemanticPreview");
            var cards = GetPartCards(preview);
            cards.Length.ShouldBe(3);

            HoverCard(cards, window, "root");
            GetHighlightCount(window).ShouldBe(1);

            HoverCard(cards, window, "list");
            GetHighlightCount(window).ShouldBe(1);

            HoverCard(cards, window, "item");
            GetHighlightCount(window).ShouldBe(3);

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

    private static void HoverCard(UserControl[] cards, AtomUIWindow window, string path)
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

internal sealed class UploadTestScreen : IScreen
{
    public RoutingState Router { get; } = new();
}
