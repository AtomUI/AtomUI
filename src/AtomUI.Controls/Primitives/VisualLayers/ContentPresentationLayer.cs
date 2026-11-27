using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Primitives;

/// <summary>
/// T5 content presentation layer: hosts scoped Drawers and scoped Dialogs (<c>OverlayScope</c>).
/// It sits above the T3 content overlay layer (Watermark) and the T4 content floating layer (FloatButton) of
/// the same host, with at most one instance per host. Within one instance, scoped Drawers and scoped Dialogs
/// follow the order in which they were opened (the later one on top, as in the antd DOM order).
/// Like T3, this layer mirrors its FlowDirection with the host, so that a scoped Drawer can align with its
/// target under RTL.
/// </summary>
internal sealed class ContentPresentationLayer : ScopeAwareAdornerLayer
{
    /// <summary>
    /// The size of the most recent arrange pass. A scoped Dialog needs the size of the whole layer before
    /// Bounds is updated.
    /// </summary>
    internal Size AvailableSize { get; private set; }

    /// <summary>
    /// Drawer entry point: lands on the same host that <see cref="ScopeAwareAdornerLayer.GetLayer" /> resolves
    /// (the scroll content wrapping panel or the content <see cref="VisualLayerManager" />) and moves with
    /// scrolling.
    /// </summary>
    internal static new ContentPresentationLayer? GetLayer(Visual visual)
    {
        // Resolve (injecting when needed) the T3 layer of the same host first: its visual parent is the scope
        // host. Only the T3 injection creates the wrapping panel of scrolled content, so both layers share that
        // panel and the rule that resolves the host is kept in one place.
        var overlayLayer = ScopeAwareAdornerLayer.GetLayer(visual);
        if (overlayLayer?.GetVisualParent() is not Layoutable host)
        {
            return null;
        }

        return GetOrInject(host, overlayLayer.LayerHost);
    }

    /// <summary>
    /// Scoped Dialog entry point: lands on the same host that <see cref="ScopeAwareOverlayLayer.GetLayer" />
    /// resolves (<see cref="ScopeAwareOverlayLayerPanel" /> or the content <see cref="VisualLayerManager" />)
    /// and stays fixed in the viewport.
    /// </summary>
    internal static ContentPresentationLayer? GetViewportLayer(Visual visual)
    {
        Layoutable? host = visual.FindAncestorOfType<ScopeAwareOverlayLayerPanel>(true);
        // Nearest scope wins: a surface host nearer than the viewport panel takes precedence (a scoped Dialog
        // inside a Drawer within scrolled content stays inside the Drawer surface).
        host =   LayerLadder.PreferNearerSurfaceHost(visual, host);
        host ??= LayerLadder.FindContentLayerManager(visual);
        if (host == null)
        {
            return null;
        }

        return GetOrInject(host, host);
    }

    private static ContentPresentationLayer? GetOrInject(Layoutable host, Visual? layerHost)
    {
        if (host is not (VisualLayerManager or Panel))
        {
            return null;
        }

        foreach (var child in host.GetVisualChildren())
        {
            if (child is ContentPresentationLayer existing)
            {
                return existing;
            }
        }

        var layer = new ContentPresentationLayer
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment   = VerticalAlignment.Stretch,
            LayerHost           = layerHost
        };
        LayerLadder.AddLayer(host, layer, LayerLadder.ContentPresentation);
        return layer;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        AvailableSize = finalSize;
        return base.ArrangeOverride(finalSize);
    }
}
