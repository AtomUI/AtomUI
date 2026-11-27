using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Primitives;

/// <summary>
/// The single source of the numbers of the global visual layer ladder. Every layer injected into a
/// VisualLayerManager or a layer host Panel must use the numbers defined here. The numbers of the Avalonia
/// built-in layers (Overlay −98 / LightDismiss −97 / TextSelector −96 / PopupOverlay −95) stay unchanged.
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
    /// Marks a <see cref="VisualLayerManager" /> as a surface host (for example a promoted content surface).
    /// A surface host carries only content-role layers (decoration, overlay, floating, presentation);
    /// window-level roles (Tour, window feedback, ...) skip it when resolving their host.
    /// </summary>
    public static readonly AttachedProperty<bool> IsSurfaceHostProperty =
        AvaloniaProperty.RegisterAttached<LayerLadder, VisualLayerManager, bool>("IsSurfaceHost");

    /// <summary>
    /// Gets whether a <see cref="VisualLayerManager" /> is a surface host.
    /// </summary>
    public static bool GetIsSurfaceHost(VisualLayerManager manager)
    {
        return manager.GetValue(IsSurfaceHostProperty);
    }

    /// <summary>
    /// Sets whether a <see cref="VisualLayerManager" /> is a surface host.
    /// </summary>
    public static void SetIsSurfaceHost(VisualLayerManager manager, bool value)
    {
        manager.SetValue(IsSurfaceHostProperty, value);
    }

    private LayerLadder()
    {
    }

    /// <summary>
    /// Creates a surface host: a nested <see cref="VisualLayerManager" /> with only the AdornerLayer enabled
    /// (Avalonia does not enable the Overlay / TextSelector layers by default). Meant for promoted surfaces
    /// whose host is created in code and cannot be declared in a ControlTheme.
    /// </summary>
    internal static VisualLayerManager CreateSurfaceHost()
    {
        var host = new VisualLayerManager();
        SetIsSurfaceHost(host, true);
        return host;
    }

    /// <summary>
    /// Rule 2 (nearest scope wins): picks, by ancestor distance, the nearer of a ScrollViewer-kind host
    /// (ScrollContentPresenter / ScopeAwareOverlayLayerPanel) and a marked surface host. Returns the surface
    /// host when it is nearer, otherwise returns scrollHost unchanged. Only marked surface hosts are compared;
    /// an unmarked nested VisualLayerManager still ranks below a ScrollViewer-kind host (the original priority
    /// is kept).
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
    /// Content role resolution: the nearest VisualLayerManager ancestor (surface hosts included), or, when
    /// there is none, the first non-surface host under the TopLevel.
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
                // The fallback skips surface hosts: a surface host scopes only its descendants. A visual outside one
                // would otherwise land in a surface (for example on a TopLevel without a VisualLayerManager, where the
                // surface host of an opened Drawer is the first VisualLayerManager); the Drawer container would be
                // moved into a layer inside itself and the visual tree would form a cycle.
                if (descendant is VisualLayerManager manager && !GetIsSurfaceHost(manager))
                {
                    return manager;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Window-level role resolution: the nearest non-surface-host VisualLayerManager ancestor, or, when there
    /// is none, the first non-surface host under the TopLevel (template descendants first, then visual
    /// descendants). An unmarked nested VisualLayerManager is still a valid scope.
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
    /// Moves the manager's own AdornerLayer to the content decoration number. This is idempotent: Avalonia
    /// assigns ZIndex once in its private AddLayer and never rewrites it, so the value set here is kept.
    /// </summary>
    internal static void EnsureContentDecorationOrder(VisualLayerManager manager)
    {
        var anchor = manager.Child;
        if (anchor == null)
        {
            // Intentional no-op. The call is idempotent; the next call after Child is ready renumbers the layer.
            return;
        }

        var adornerLayer = AdornerLayer.GetAdornerLayer(anchor);
        // When Child is itself a VisualLayerManager, GetAdornerLayer resolves to a layer of that Child. The parent
        // check below rejects it, so this is an intentional no-op.
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
