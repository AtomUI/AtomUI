using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Primitives;

internal class VisualLayerManagerUtils
{
    internal const int ScopeAwareAdornerLayerZIndex = LayerLadder.ContentOverlay;
    internal const int ScopeAwareOverlayZIndex = LayerLadder.ContentFloating;
    
    internal static T? FindLayer<T>(VisualLayerManager visualLayerManager) where T : class
    {
        foreach (var layer in visualLayerManager.GetVisualChildren())
        {
            if (layer != visualLayerManager.Child && layer is T match)
            {
                return match;
            }
        }
        return null;
    }
}
