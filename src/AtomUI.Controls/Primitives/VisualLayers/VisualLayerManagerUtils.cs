using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Primitives;

internal class VisualLayerManagerUtils
{
    internal const int ScopeAwareAdornerLayerZIndex = int.MaxValue - 99;
    internal const int ScopeAwareOverlayZIndex = int.MaxValue - 990;
    
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
