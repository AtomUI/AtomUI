using AtomUI.Toolkits.GalleryBase.Controls;
using AtomUIGallery.ShowCases.Steps;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUIGallery.Tests.ShowCases;

public class StepsShowCaseSemanticPreviewLayoutTests
{
    static StepsShowCaseSemanticPreviewLayoutTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Narrow_Window_Keeps_Preview_Visible_And_Bounds_The_Parts_Pane()
    {
        // Regression: below the 820 compact breakpoint the parts pane stacks under
        // the preview and grew to its full content height, so browsing the lower
        // parts scrolled the preview out of view. The pane must be height-bounded
        // and scroll on its own so the preview stays visible while selecting parts.
        var page = new StepsShowCase
        {
            DataContext = new StepsViewModel(new TestScreen())
        };

        ShowInWindow(page, 640, 900, () =>
        {
            SelectSemanticPartsTab(page);

            var preview = page.GetVisualDescendants().OfType<SemanticPartPreview>().Single();
            var stage = preview.GetVisualDescendants().OfType<Avalonia.Controls.Border>()
                                .Single(control => control.Name == "PART_PreviewStage");
            var pane = preview.GetVisualDescendants().OfType<Avalonia.Controls.Border>()
                              .Single(control => control.Name == "PART_PartsPane");

            stage.IsVisible.ShouldBeTrue();
            stage.Bounds.Height.ShouldBeGreaterThanOrEqualTo(279);
            pane.Bounds.Height.ShouldBeLessThanOrEqualTo(401);

            var paneScroller = pane.GetVisualDescendants()
                                   .OfType<AtomUI.Desktop.Controls.ScrollViewer>()
                                   .Single();
            paneScroller.Extent.Height.ShouldBeGreaterThan(paneScroller.Viewport.Height);
        });
    }

    [Fact]
    public void Wide_Window_With_Short_Height_Bounds_The_Parts_Pane_And_Scrolls_Internally()
    {
        // Regression: in the wide (side-by-side) layout the parts pane used to be
        // measured with unbounded height, so it grew to its full card-list height
        // and inflated the page beyond the viewport. Browsing the lower parts then
        // scrolled the page and pushed the preview off-screen. The pane must stay
        // height-bounded and scroll internally in the wide layout as well.
        var page = new StepsShowCase
        {
            DataContext = new StepsViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 640, () =>
        {
            SelectSemanticPartsTab(page);

            var preview = page.GetVisualDescendants().OfType<SemanticPartPreview>().Single();
            var pane = preview.GetVisualDescendants().OfType<Avalonia.Controls.Border>()
                              .Single(control => control.Name == "PART_PartsPane");

            pane.Bounds.Height.ShouldBeLessThanOrEqualTo(401);

            var paneScroller = pane.GetVisualDescendants()
                                   .OfType<AtomUI.Desktop.Controls.ScrollViewer>()
                                   .Single();
            paneScroller.Extent.Height.ShouldBeGreaterThan(paneScroller.Viewport.Height);
        });
    }

    [Fact]
    public void Wide_Window_Keeps_The_Side_By_Side_Layout_Unchanged()
    {
        var page = new StepsShowCase
        {
            DataContext = new StepsViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            SelectSemanticPartsTab(page);

            var preview = page.GetVisualDescendants().OfType<SemanticPartPreview>().Single();
            var stage = preview.GetVisualDescendants().OfType<Avalonia.Controls.Border>()
                                .Single(control => control.Name == "PART_PreviewStage");
            var pane = preview.GetVisualDescendants().OfType<Avalonia.Controls.Border>()
                              .Single(control => control.Name == "PART_PartsPane");

            pane.Bounds.X.ShouldBeGreaterThanOrEqualTo(stage.Bounds.Right - 0.01);
            pane.Bounds.Height.ShouldBeGreaterThanOrEqualTo(stage.Bounds.Height - 0.01);
        });
    }

    private static void SelectSemanticPartsTab(StepsShowCase page)
    {
        var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
        host.SelectedTab = GalleryShowCaseTab.SemanticParts;
        Dispatcher.UIThread.RunJobs();
    }

    private static void ShowInWindow(Control content, double width, double height, Action assertion)
    {
        var visualLayerManager = new VisualLayerManager
        {
            EnableAdornerLayer = true,
            Child = content
        };

        var window = new AvaloniaWindow
        {
            Content = visualLayerManager,
            Width = width,
            Height = height
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            assertion();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private sealed class TestScreen : IScreen
    {
        public RoutingState Router { get; } = new();
    }
}
