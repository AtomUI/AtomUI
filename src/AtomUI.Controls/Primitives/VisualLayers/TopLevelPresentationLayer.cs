using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Primitives;

/// <summary>
/// T7 TopLevel 呈现层：承载 TopLevel Drawer（<c>OpenOn is TopLevel</c>，含 Window Drawer）。
/// 解析到 TopLevel 的宿主（跳过表面宿主），对 Window、PopupRoot、EmbeddableControlRoot 一视同仁。
/// </summary>
internal sealed class TopLevelPresentationLayer : ScopeAwareAdornerLayer
{
    internal static TopLevelPresentationLayer? GetLayer(TopLevel topLevel)
    {
        Layoutable? host = LayerLadder.FindTopLevelLayerManager(topLevel);
        if (host == null)
        {
            // 没有 VisualLayerManager 的 TopLevel（例如 Avalonia 12 的原生 Window，层位于 TopLevelHost）：
            // 与 T3 共用 TopLevel 内容包裹面板，在其中按同一号码排在 T3/T5 之上。
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
