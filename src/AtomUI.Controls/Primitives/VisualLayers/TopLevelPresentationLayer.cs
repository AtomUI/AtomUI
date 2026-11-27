using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Primitives;

/// <summary>
/// T7 TopLevel presentation layer: hosts TopLevel Drawers (<c>OpenOn is TopLevel</c>, Window Drawers included).
/// It resolves the host of the TopLevel (skipping surface hosts) the same way for Window, PopupRoot and
/// EmbeddableControlRoot.
/// </summary>
internal sealed class TopLevelPresentationLayer : ScopeAwareAdornerLayer
{
    internal static TopLevelPresentationLayer? GetLayer(TopLevel topLevel)
    {
        Layoutable? host = LayerLadder.FindTopLevelLayerManager(topLevel);
        if (host == null)
        {
            // A TopLevel without a VisualLayerManager, for example the native Avalonia 12 Window whose layers live in
            // TopLevelHost: share the panel that wraps the TopLevel content with the T3 layer (ScopeAwareAdornerLayer),
            // and rank above T3 and T5 (ContentPresentationLayer) inside it by the same ladder number.
            host = ScopeAwareAdornerLayer.GetOrInjectLayer(topLevel)?.GetVisualParent() as Panel;
        }

        if (host == null)
        {
            return null;
        }

        foreach (var child in host.GetVisualChildren())
        {
            if (child is TopLevelPresentationLayer existing)
            {
                return existing;
            }
        }

        var layer = new TopLevelPresentationLayer
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment   = VerticalAlignment.Stretch,
            LayerHost           = host is VisualLayerManager ? host : topLevel
        };
        LayerLadder.AddLayer(host, layer, LayerLadder.TopLevelPresentation);
        return layer;
    }
}
