using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Primitives;

/// <summary>
/// T5 内容呈现层：承载作用域 Drawer 与作用域 Dialog（<c>OverlayScope</c>）。
/// 位于同一宿主的 T3 内容覆盖层（Watermark）与 T4 内容浮动层（FloatButton）之上，每个宿主最多一个实例。
/// 同一实例中的作用域 Drawer 与作用域 Dialog 按打开顺序排列（后打开的在上，与 antd DOM 顺序一致）。
/// 本层与 T3 一样遵循 FlowDirection 策略随宿主镜像，RTL 下作用域 Drawer 才能贴合其 target。
/// </summary>
internal sealed class ContentPresentationLayer : ScopeAwareAdornerLayer
{
    /// <summary>
    /// 最近一次排列得到的尺寸。作用域 Dialog 需要在 Bounds 尚未更新时就知道整层尺寸。
    /// </summary>
    internal Size AvailableSize { get; private set; }

    /// <summary>
    /// Drawer 入口：落在 <see cref="ScopeAwareAdornerLayer.GetLayer"/> 会解析出的同一个宿主
    /// （滚动内容包裹面板 / 内容 <see cref="VisualLayerManager"/>），随滚动移动。
    /// </summary>
    internal static new ContentPresentationLayer? GetLayer(Visual visual)
    {
        // 先解析（必要时注入）同宿主的 T3 层：它的视觉父级就是作用域宿主。滚动内容的包裹面板
        // 只由 T3 注入创建，这样两层共用同一个包裹面板，宿主解析规则也只保留一份。
        var overlayLayer = ScopeAwareAdornerLayer.GetLayer(visual);
        if (overlayLayer?.GetVisualParent() is not Layoutable host)
        {
            return null;
        }

        return GetOrInject(host, overlayLayer.LayerHost);
    }

    /// <summary>
    /// 作用域 Dialog 入口：落在 <see cref="ScopeAwareOverlayLayer.GetLayer"/> 会解析出的同一个宿主
    /// （<see cref="ScopeAwareOverlayLayerPanel"/> / 内容 <see cref="VisualLayerManager"/>），固定在视口。
    /// </summary>
    internal static ContentPresentationLayer? GetViewportLayer(Visual visual)
    {
        Layoutable? host = visual.FindAncestorOfType<ScopeAwareOverlayLayerPanel>(true);
        // 规则 2：比视口面板更近的表面宿主优先（滚动内容中 Drawer 内的作用域 Dialog 留在 Drawer 表面内）。
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
