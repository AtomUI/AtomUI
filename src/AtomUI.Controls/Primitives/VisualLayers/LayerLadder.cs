using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Primitives;

/// <summary>
/// 全局视觉层级阶梯的唯一号码真源。所有注入到 VisualLayerManager / 层宿主 Panel 的层都必须使用这里的号码。
/// Avalonia 内置层号码（Overlay −98 / LightDismiss −97 / TextSelector −96 / PopupOverlay −95）保持不变。
/// </summary>
public sealed class LayerLadder
{
    internal const int ContentDecoration   = int.MaxValue - 1000;
    internal const int ContentOverlay      = int.MaxValue - 900;
    internal const int ContentFloating     = int.MaxValue - 800;
    internal const int ContentPresentation = int.MaxValue - 700;
    internal const int WindowChrome        = int.MaxValue - 600;
    internal const int TopLevelPresentation = int.MaxValue - 500;
    internal const int Tour                = int.MaxValue - 400;
    internal const int WindowFeedback      = int.MaxValue - 50;

    /// <summary>
    /// 标记一个 <see cref="VisualLayerManager"/> 为“表面宿主”（例如被提升的内容表面）。
    /// 表面宿主只承载内容角色的层（装饰/覆盖/浮动/呈现）；窗口级角色（Tour、窗口反馈等）解析时会跳过它。
    /// </summary>
    public static readonly AttachedProperty<bool> IsSurfaceHostProperty =
        AvaloniaProperty.RegisterAttached<LayerLadder, VisualLayerManager, bool>("IsSurfaceHost");

    /// <summary>获取 <see cref="VisualLayerManager"/> 是否为表面宿主。</summary>
    public static bool GetIsSurfaceHost(VisualLayerManager manager)
    {
        return manager.GetValue(IsSurfaceHostProperty);
    }

    /// <summary>设置 <see cref="VisualLayerManager"/> 是否为表面宿主。</summary>
    public static void SetIsSurfaceHost(VisualLayerManager manager, bool value)
    {
        manager.SetValue(IsSurfaceHostProperty, value);
    }

    private LayerLadder()
    {
    }

    /// <summary>
    /// 创建一个表面宿主：只启用 AdornerLayer 的嵌套 <see cref="VisualLayerManager"/>（Avalonia 默认即不启用
    /// Overlay / TextSelector 层）。供宿主由代码创建、无法在 ControlTheme 中声明的提升表面使用。
    /// </summary>
    internal static VisualLayerManager CreateSurfaceHost()
    {
        var host = new VisualLayerManager();
        SetIsSurfaceHost(host, true);
        return host;
    }

    /// <summary>
    /// 规则 2（最近作用域优先）：在 ScrollViewer 类宿主（ScrollContentPresenter / ScopeAwareOverlayLayerPanel）
    /// 与带标记的表面宿主之间按祖先距离取较近者。表面宿主更近时返回它，否则原样返回 scrollHost。
    /// 只比较带标记的表面宿主；未标记的嵌套 VisualLayerManager 仍低于 ScrollViewer 类宿主（保持原有优先级）。
    /// </summary>
    internal static Layoutable? PreferNearerSurfaceHost(Visual visual, Layoutable? scrollHost)
    {
        if (scrollHost is null || ReferenceEquals(visual, scrollHost))
        {
            return scrollHost;
        }

        foreach (var ancestor in visual.GetVisualAncestors())
        {
            if (ReferenceEquals(ancestor, scrollHost))
            {
                return scrollHost;
            }

            if (ancestor is VisualLayerManager manager && GetIsSurfaceHost(manager))
            {
                return manager;
            }
        }

        return scrollHost;
    }

    /// <summary>
    /// 内容角色解析：最近的 VisualLayerManager 祖先（含表面宿主），找不到时退回 TopLevel 下第一个非表面宿主。
    /// </summary>
    internal static VisualLayerManager? FindContentLayerManager(Visual visual)
    {
        if (visual.FindAncestorOfType<VisualLayerManager>() is { } ancestor)
        {
            return ancestor;
        }

        if (TopLevel.GetTopLevel(visual) is { } topLevel)
        {
            foreach (var descendant in topLevel.GetVisualDescendants())
            {
                // 退回路径跳过表面宿主：表面宿主只是其后代的作用域。不在它内部的视觉若落进某个表面
                // （例如没有 VisualLayerManager 的 TopLevel 上，已打开 Drawer 的表面宿主是第一个
                // VisualLayerManager），Drawer container 会被移入自己内部的层，视觉树成环。
                if (descendant is VisualLayerManager manager && !GetIsSurfaceHost(manager))
                {
                    return manager;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// 窗口级角色解析：最近的非表面宿主 VisualLayerManager 祖先，找不到时退回 TopLevel 下第一个非表面宿主
    /// （先模板子树，再视觉子树）。未标记的嵌套 VisualLayerManager 仍然是合法的作用域。
    /// </summary>
    internal static VisualLayerManager? FindTopLevelLayerManager(Visual visual)
    {
        foreach (var ancestor in visual.GetVisualAncestors())
        {
            if (ancestor is VisualLayerManager manager && !GetIsSurfaceHost(manager))
            {
                return manager;
            }
        }

        if (TopLevel.GetTopLevel(visual) is not { } topLevel)
        {
            return null;
        }

        foreach (var descendant in topLevel.GetTemplateDescendants())
        {
            if (descendant is VisualLayerManager manager && !GetIsSurfaceHost(manager))
            {
                return manager;
            }
        }

        foreach (var descendant in topLevel.GetVisualDescendants())
        {
            if (descendant is VisualLayerManager manager && !GetIsSurfaceHost(manager))
            {
                return manager;
            }
        }

        return null;
    }

    /// <summary>
    /// 幂等：把 manager 自己的 AdornerLayer 重排到内容装饰号码。
    /// Avalonia 只在私有 AddLayer 中赋一次 ZIndex，之后不会改写，所以这里设置的值会保留。
    /// </summary>
    internal static void EnsureContentDecorationOrder(VisualLayerManager manager)
    {
        var anchor = manager.Child;
        if (anchor == null)
        {
            // 有意空操作：幂等，Child 就绪后的下一次调用会重新编号。
            return;
        }

        var adornerLayer = AdornerLayer.GetAdornerLayer(anchor);
        // 若 Child 本身是 VisualLayerManager，GetAdornerLayer 解析到的是 Child 自己的层，父级校验不通过，有意空操作。
        if (adornerLayer != null && ReferenceEquals(adornerLayer.GetVisualParent(), manager))
        {
            adornerLayer.ZIndex = ContentDecoration;
        }
    }

    internal static void AddLayer(Layoutable host, Control layer, int zIndex)
    {
        switch (host)
        {
            case VisualLayerManager manager:
                EnsureContentDecorationOrder(manager);
                layer.ZIndex = zIndex;
                manager.AddLayer(layer, zIndex);
                break;
            case Panel panel:
                layer.ZIndex = zIndex;
                panel.Children.Add(layer);
                break;
            default:
                throw new ArgumentException($"Unsupported layer host type: {host.GetType()}", nameof(host));
        }
    }
}
