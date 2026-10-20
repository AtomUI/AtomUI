using AtomUI.Toolkits.GalleryBase.Controls;
using AtomUIGallery.ShowCases.TabControl;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUICardTabControl = AtomUI.Desktop.Controls.CardTabControl;
using AtomUIScrollViewer = AtomUI.Desktop.Controls.ScrollViewer;
using AtomUITabItem = AtomUI.Desktop.Controls.TabItem;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUIGallery.Tests.ShowCases;

public class TabControlSemanticPartPointerTests
{
    private const int WindowWidth  = 1280;
    private const int WindowHeight = 900;

    private static double? CurrentWindowHeight { get; set; }

    [Fact]
    public void Hover_On_TabItem_Preview_Icon_Row_Draws_Adorners_On_The_Demo_Icon()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new TabControlShowCase
        {
            DataContext = new TabControlViewModel(new TestScreen())
        };

        ShowInWindow(page, window =>
        {
            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            var preview = page.GetVisualDescendants()
                              .OfType<SemanticPartPreview>()
                              .Single(static candidate => candidate.Name == "TabItemSemanticPreview");
            var ownerTab = page.GetVisualDescendants()
                               .OfType<AtomUITabItem>()
                               .Single(static candidate => candidate.Name == "TabItemSemanticOwner");
            var cards = FindCards(preview);
            cards.Length.ShouldBe(4);

            var layer = AdornerLayer.GetAdornerLayer(ownerTab);
            layer.ShouldNotBeNull();
            layer!.Children.ShouldBeEmpty();

            // Bring the TabItem preview into the viewport the way a user would.
            var pageScroller = page.GetVisualDescendants()
                                   .OfType<AtomUIScrollViewer>()
                                   .Single(static candidate => candidate.Name == "PART_ScrollViewer");
            pageScroller.Offset = new Vector(0, pageScroller.Extent.Height - pageScroller.Viewport.Height);
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            // The live markup ships this preview with Title="TabItem".
            preview.Title.ShouldBe("TabItem");
            UserControl? iconCard = null;
            foreach (var horizontalPosition in new[] { 8d, 0.5, -8d })
            {
                iconCard = HoverCard(window, cards, "icon", horizontalPosition);
                AssertHoverVisual(iconCard);
                layer.Children.ShouldNotBeEmpty();
                layer.Children.ShouldAllBe(static child => child.GetType().Name == "SemanticPartAdorner");
            }

            HoverCard(window, cards, "close");
            layer.Children.ShouldNotBeEmpty();
            layer.Children.ShouldAllBe(static child => child.GetType().Name == "SemanticPartAdorner");

            // A/B against the reported regression: the Title must not affect row hover.
            preview.Title = null;
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            iconCard = HoverCard(window, cards, "icon");
            AssertHoverVisual(iconCard);
            layer.Children.ShouldNotBeEmpty();
            layer.Children.ShouldAllBe(static child => child.GetType().Name == "SemanticPartAdorner");
        });
    }

    [Fact]
    public void Wheel_Over_Second_Preview_Parts_Row_Scrolls_The_Pane_Then_Chains_To_The_Page()
    {
        AvaloniaTestApp.EnsureInitialized();
        CurrentWindowHeight = 480;

        var page = new TabControlShowCase
        {
            DataContext = new TabControlViewModel(new TestScreen())
        };

        ShowInWindow(page, window =>
        {
            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            var pageScroller = page.GetVisualDescendants()
                                   .OfType<AtomUIScrollViewer>()
                                   .Single(static candidate => candidate.Name == "PART_ScrollViewer");
            pageScroller.Extent.Height.ShouldBeGreaterThan(pageScroller.Viewport.Height);

            pageScroller.Offset = new Vector(0, 400);
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();
            var startOffset = pageScroller.Offset.Y;
            startOffset.ShouldBeGreaterThan(0);

            var preview = page.GetVisualDescendants()
                              .OfType<SemanticPartPreview>()
                              .Single(static candidate => candidate.Name == "CardTabControlSemanticPreview");
            var pane = preview.GetVisualDescendants()
                              .OfType<Border>()
                              .Single(static candidate => candidate.Name == "PART_PartsPane");
            var paneScroller = pane.GetVisualDescendants()
                                   .OfType<AtomUIScrollViewer>()
                                   .Single();
            paneScroller.Extent.Height.ShouldBeGreaterThan(paneScroller.Viewport.Height);
            var verticalScrollBar = paneScroller.GetVisualDescendants()
                                                .OfType<AtomUI.Desktop.Controls.ScrollBar>()
                                                .Single(static candidate =>
                                                    candidate.Name == "PART_VerticalScrollBar");
            verticalScrollBar.IsVisible.ShouldBeTrue();

            window.MouseWheel(PointInsideWindow(window, pane, new Point(80, 80)), new Vector(0, -120));
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            paneScroller.Offset.Y.ShouldBeGreaterThan(0);
            pageScroller.Offset.Y.ShouldBe(startOffset);

            paneScroller.Offset = new Vector(
                paneScroller.Offset.X,
                paneScroller.Extent.Height - paneScroller.Viewport.Height);
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            window.MouseWheel(PointInsideWindow(window, pane, new Point(80, 80)), new Vector(0, -120));
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            pageScroller.Offset.Y.ShouldBeGreaterThan(startOffset);
        });
    }

    [Fact]
    public void Horizontal_Wheel_Over_Second_Preview_Tab_Strip_Still_Scrolls_The_Tabs()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new TabControlShowCase
        {
            DataContext = new TabControlViewModel(new TestScreen())
        };

        ShowInWindow(page, window =>
        {
            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            var pageScroller = page.GetVisualDescendants()
                                   .OfType<AtomUIScrollViewer>()
                                   .Single(static candidate => candidate.Name == "PART_ScrollViewer");
            pageScroller.Offset = new Vector(0, 400);
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            var demo = page.GetVisualDescendants()
                           .OfType<AtomUICardTabControl>()
                           .Single(static candidate => candidate.Name == "CardTabControlSemanticOwner");
            var tabScroller = demo.GetVisualDescendants()
                                  .OfType<AtomUIScrollViewer>()
                                  .Single(static candidate => candidate.Name == "PART_CardTabStripScrollViewer");
            tabScroller.Extent.Width.ShouldBeGreaterThan(tabScroller.Viewport.Width);

            var pageOffset = pageScroller.Offset.Y;
            window.MouseWheel(PointInsideWindow(window, demo, new Point(210, 17)), new Vector(-120, 0));
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            tabScroller.Offset.X.ShouldBeGreaterThan(0);
            pageScroller.Offset.Y.ShouldBe(pageOffset);
        });
    }

    [Fact]
    public void Multiple_Previews_Keep_A_Bounded_Scrollable_Parts_Pane()
    {
        AvaloniaTestApp.EnsureInitialized();
        CurrentWindowHeight = 480;

        var page = new TabControlShowCase
        {
            DataContext = new TabControlViewModel(new TestScreen())
        };

        ShowInWindow(page, window =>
        {
            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            var pageScroller = page.GetVisualDescendants()
                                   .OfType<AtomUIScrollViewer>()
                                   .Single(static candidate => candidate.Name == "PART_ScrollViewer");
            pageScroller.Offset = new Vector(0, 400);
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            foreach (var preview in page.GetVisualDescendants().OfType<SemanticPartPreview>())
            {
                var pane = preview.GetVisualDescendants()
                                  .OfType<Border>()
                                  .Single(static candidate => candidate.Name == "PART_PartsPane");
                var paneScroller = pane.GetVisualDescendants()
                                       .OfType<AtomUIScrollViewer>()
                                       .Single();
                // 部件列表紧凑化后内容可能不超出视口；锁定的契约是
                // 视口有界且滚动条可见性与溢出状态一致
                paneScroller.Viewport.Height.ShouldBeLessThanOrEqualTo(401);
                var overflows = paneScroller.Extent.Height > paneScroller.Viewport.Height;
                paneScroller.GetVisualDescendants()
                            .OfType<AtomUI.Desktop.Controls.ScrollBar>()
                            .Single(static candidate => candidate.Name == "PART_VerticalScrollBar")
                            .IsVisible.ShouldBe(overflows);
            }

            var secondPane = page.GetVisualDescendants()
                                 .OfType<SemanticPartPreview>()
                                 .Single(static candidate => candidate.Name == "CardTabControlSemanticPreview")
                                 .GetVisualDescendants()
                                 .OfType<Border>()
                                 .Single(static candidate => candidate.Name == "PART_PartsPane");
            var secondPaneScroller = secondPane.GetVisualDescendants()
                                               .OfType<AtomUIScrollViewer>()
                                               .Single();
            var startPageOffset = pageScroller.Offset.Y;
            window.MouseWheel(
                PointInsideWindow(window, secondPane, new Point(secondPane.Bounds.Width - 30, 80)),
                new Vector(0, -120));
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            secondPaneScroller.Offset.Y.ShouldBeGreaterThan(0);
            pageScroller.Offset.Y.ShouldBe(startPageOffset);
        });
    }

    private static UserControl[] FindCards(SemanticPartPreview preview)
    {
        var partsPane = preview.GetVisualDescendants()
                               .OfType<Border>()
                               .Single(static candidate => candidate.Name == "PART_PartsPane");
        return partsPane.GetVisualDescendants()
                        .OfType<UserControl>()
                        .Where(static candidate => candidate.GetType().Name.Contains("SemanticPartPreviewItem"))
                        .ToArray();
    }

    private static UserControl HoverCard(
        AvaloniaWindow window,
        UserControl[] cards,
        string path,
        double horizontalPosition = 0.5)
    {
        var card = cards.Single(candidate =>
            (string?)candidate.DataContext?.GetType().GetProperty("Path")!.GetValue(candidate.DataContext) == path);
        var x = horizontalPosition switch
        {
            < 0 => card.Bounds.Width + horizontalPosition,
            <= 1 => card.Bounds.Width * horizontalPosition,
            _ => horizontalPosition
        };
        var center = card.TransformToVisual(window)!.Value
                         .Transform(new Point(x, card.Bounds.Height / 2));
        window.MouseMove(center);
        Dispatcher.UIThread.RunJobs();
        Dispatcher.UIThread.RunJobs();
        Dispatcher.UIThread.RunJobs();
        return card;
    }

    private static void AssertHoverVisual(UserControl card)
    {
        card.IsPointerOver.ShouldBeTrue();
        var row = card.GetVisualDescendants()
                      .OfType<Border>()
                      .Single(static candidate => candidate.Name == "PART_Row");
        row.Background.ShouldNotBe(Brushes.Transparent);
    }

    private static Point PointInsideWindow(AvaloniaWindow window, Control target, Point? relative = null)
    {
        relative ??= new Point(target.Bounds.Width / 2, target.Bounds.Height / 2);
        var point = target.TransformToVisual(window)!.Value.Transform(relative.Value);
        var x = Math.Clamp(point.X, 8, window.Bounds.Width - 8);
        var y = Math.Clamp(point.Y, 100, window.Bounds.Height - 16);
        return new Point(x, y);
    }

    private static void ShowInWindow(Control content, Action<AvaloniaWindow> assertion)
    {
        var visualLayerManager = new VisualLayerManager
        {
            EnableAdornerLayer = true,
            Child = content
        };

        var window = new AvaloniaWindow
        {
            Content = visualLayerManager,
            Width = WindowWidth,
            Height = CurrentWindowHeight ?? WindowHeight
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            assertion(window);
        }
        finally
        {
            window.Close();
            CurrentWindowHeight = null;
            Dispatcher.UIThread.RunJobs();
        }
    }
}
