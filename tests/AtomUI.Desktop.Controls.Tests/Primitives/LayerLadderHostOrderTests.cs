using AtomUI.Controls.Primitives;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIWindow = AtomUI.Desktop.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Primitives;

/// <summary>
/// Guards the global layer ladder (docs/architecture/systems/rendering/visual-layers.md): every host type orders the
/// role layers by the <see cref="LayerLadder"/> numbers, and the order survives the operations after which Avalonia
/// could re-create or re-number its own layers. The ladder relies on Avalonia never writing the shared
/// <see cref="AdornerLayer"/>'s ZIndex back once AtomUI has moved it to T2; nothing else detects a regression there.
/// </summary>
public class LayerLadderHostOrderTests
{
    static LayerLadderHostOrderTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    private static void Settle(Layoutable visual)
    {
        Dispatcher.UIThread.RunJobs();
        visual.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
    }

    private static VisualLayerManager Manager(AtomUIWindow window)
    {
        return window.GetVisualDescendants().OfType<VisualLayerManager>()
                     .Single(m => m.Name == "PART_VisualLayerManager");
    }

    /// <summary>
    /// Resolves every role layer of the window host through the production resolvers and asserts parentage, numbers and
    /// the bottom-to-top order of the manager's children, including the full-screen title bar's placement in T6.
    /// </summary>
    private static void AssertWindowLadder(AtomUIWindow window, Control content)
    {
        var manager      = Manager(window);
        var overlay      = ScopeAwareAdornerLayer.GetLayer(content).ShouldNotBeNull();
        var floating     = ScopeAwareOverlayLayer.GetLayer(content).ShouldNotBeNull();
        var presentation = ContentPresentationLayer.GetLayer(content).ShouldNotBeNull();
        var topLevel     = TopLevelPresentationLayer.GetLayer(window).ShouldNotBeNull();
        var tour         = TourLayer.GetTourLayer(content).ShouldNotBeNull();
        var feedback     = WindowFeedbackLayer.GetLayer(window).ShouldNotBeNull();
        Settle(window);

        foreach (var layer in new Control[] { overlay, floating, presentation, topLevel, tour, feedback })
        {
            layer.GetVisualParent().ShouldBeSameAs(manager, layer.GetType().Name);
        }

        var children = manager.GetVisualChildren().Cast<Control>().ToList();
        children.OfType<AdornerLayer>().Single().ZIndex.ShouldBe(LayerLadder.ContentDecoration);
        overlay.ZIndex.ShouldBe(LayerLadder.ContentOverlay);
        floating.ZIndex.ShouldBe(LayerLadder.ContentFloating);
        presentation.ZIndex.ShouldBe(LayerLadder.ContentPresentation);
        topLevel.ZIndex.ShouldBe(LayerLadder.TopLevelPresentation);
        tour.ZIndex.ShouldBe(LayerLadder.Tour);
        feedback.ZIndex.ShouldBe(LayerLadder.WindowFeedback);

        // T6: exactly one chrome layer, presenting the template-declared title bar host through its surface host.
        var chrome = children.OfType<WindowChromeLayer>().ShouldHaveSingleItem();
        chrome.ZIndex.ShouldBe(LayerLadder.WindowChrome);
        var titleBarHost = chrome.HostedTitleBar.ShouldNotBeNull();
        titleBarHost.Name.ShouldBe("PART_FullScreenTitleBarHost");
        var surfaceHost = titleBarHost.GetVisualParent().ShouldBeOfType<VisualLayerManager>();
        LayerLadder.GetIsSurfaceHost(surfaceHost).ShouldBeTrue();
        surfaceHost.GetVisualParent().ShouldBeSameAs(chrome);

        var expected = new[]
        {
            "CONTENT", nameof(AdornerLayer), nameof(ScopeAwareAdornerLayer), nameof(ScopeAwareOverlayLayer),
            nameof(ContentPresentationLayer), nameof(WindowChromeLayer), nameof(TopLevelPresentationLayer),
            nameof(TourLayer), "OverlayLayer", "LightDismissOverlayLayer", "TextSelectorLayer", "PopupOverlayLayer",
            nameof(WindowFeedbackLayer)
        };
        var actual = children.OrderBy(c => c.ZIndex)
                             .Select(c => ReferenceEquals(c, manager.Child) ? "CONTENT" : c.GetType().Name)
                             .ToList();
        // Avalonia creates its overlay layers on demand, so only the layers present are compared, in ladder order.
        actual.ShouldBe(expected.Where(actual.Contains).ToList());
    }

    [Fact]
    public void Window_Host_Keeps_The_Ladder_Across_Popup_FullScreen_And_Template_Reapply()
    {
        var anchor = new Border { Width = 20, Height = 20 };
        var popup = new Popup
        {
            Child                 = new Border { Width = 30, Height = 30 },
            PlacementTarget       = anchor,
            ShouldUseOverlayLayer = true
        };
        var content = new Panel { Children = { anchor, popup } };
        var window = new AtomUIWindow { Width = 600, Height = 400, Title = "Ladder", Content = content };
        window.Show();
        Settle(window);
        try
        {
            // The shared AdornerLayer is at T2 as soon as the template is applied, before any layer is injected.
            Manager(window).GetVisualChildren().OfType<AdornerLayer>().Single().ZIndex
                           .ShouldBe(LayerLadder.ContentDecoration);
            AssertWindowLadder(window, content);

            popup.IsOpen = true;
            Settle(window);
            popup.IsOpen = false;
            Settle(window);
            window.WindowState = WindowState.FullScreen;
            Settle(window);
            window.WindowState = WindowState.Normal;
            Settle(window);
            AssertWindowLadder(window, content);

            // Windows has no IsCsdEnabled=False template: flip and flip back to force a template re-apply.
            var before      = Manager(window);
            var originalCsd = window.IsCsdEnabled;
            window.IsCsdEnabled = !originalCsd;
            Settle(window);
            window.IsCsdEnabled = originalCsd;
            Settle(window);
            Manager(window).ShouldNotBeSameAs(before);
            AssertWindowLadder(window, content);
            window.GetVisualDescendants().OfType<WindowChromeLayer>().Count(layer => layer.HostedTitleBar != null)
                  .ShouldBe(1);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void PopupRoot_Host_Uses_The_Same_Ladder()
    {
        var anchor       = new Border { Width = 20, Height = 20 };
        var popupContent = new Border { Width = 30, Height = 30 };
        var popup = new Popup
        {
            Child                 = popupContent,
            PlacementTarget       = anchor,
            ShouldUseOverlayLayer = false
        };
        var window = new AtomUIWindow
        {
            Width = 600, Height = 400, Title = "Ladder", Content = new Panel { Children = { anchor, popup } }
        };
        window.Show();
        Settle(window);
        try
        {
            popup.IsOpen = true;
            Settle(window);

            var overlay = ScopeAwareAdornerLayer.GetLayer(popupContent).ShouldNotBeNull();
            var manager = popupContent.FindAncestorOfType<VisualLayerManager>().ShouldNotBeNull();
            manager.ShouldNotBeSameAs(Manager(window));
            overlay.GetVisualParent().ShouldBeSameAs(manager);
            manager.GetVisualChildren().OfType<AdornerLayer>().Single().ZIndex.ShouldBe(LayerLadder.ContentDecoration);
            overlay.ZIndex.ShouldBe(LayerLadder.ContentOverlay);
        }
        finally
        {
            popup.IsOpen = false;
            window.Close();
        }
    }

    [Fact]
    public void ScrollViewer_Viewport_Injection_Point_Uses_The_Same_Ladder()
    {
        var inner  = new Border { Width = 100, Height = 50 };
        var scroll = new ScrollViewer { Content = inner, Width = 200, Height = 100 };
        var window = new AtomUIWindow { Width = 600, Height = 400, Title = "Ladder", Content = scroll };
        window.Show();
        Settle(window);
        try
        {
            var floating = ScopeAwareOverlayLayer.GetLayer(inner).ShouldNotBeNull();
            floating.GetVisualParent().ShouldBeOfType<ScopeAwareOverlayLayerPanel>();
            floating.ZIndex.ShouldBe(LayerLadder.ContentFloating);
        }
        finally
        {
            window.Close();
        }
    }
}
