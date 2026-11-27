using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Primitives;

/// <summary>
/// T4 content floating layer: a canvas that floats content, such as FloatButtons, above the content of the scope
/// it belongs to, for example a ScrollViewer, a Drawer surface or the TopLevel. Within one host it ranks above
/// the T3 layer (<see cref="ScopeAwareAdornerLayer" />) and below the T5 layer
/// (<see cref="ContentPresentationLayer" />). <see cref="GetLayer" /> returns the layer of the scope a visual
/// is in and injects one when the scope has none.
/// </summary>
public class ScopeAwareOverlayLayer : Canvas
{
    /// <summary>
    /// Identifies the <see cref="LayerHost" /> property.
    /// </summary>
    public static readonly StyledProperty<Visual?> LayerHostProperty = AvaloniaProperty
        .Register<ScopeAwareOverlayLayer, Visual?>(nameof(LayerHost));

    /// <summary>
    /// The host this layer was injected for: the <see cref="ScopeAwareOverlayLayerPanel" />, the
    /// <c>VisualLayerManager</c> or the <see cref="TopLevel" /> that <see cref="GetLayer" /> resolved for the scope.
    /// It is assigned when <see cref="GetLayer" /> injects the layer; a layer created in another way keeps
    /// <see langword="null" />.
    /// </summary>
    public Visual? LayerHost
    {
        get => GetValue(LayerHostProperty);
        set => SetValue(LayerHostProperty, value);
    }
    
    /// <summary>
    /// Always <see langword="true" />: the layer is not mirrored when <c>FlowDirection</c> is right-to-left, so the
    /// content placed on it is rendered at the canvas coordinates it was arranged with.
    /// </summary>
    protected override bool BypassFlowDirectionPolicies => true;
    
    /// <summary>
    /// The size passed to the latest arrange pass of the layer. Children that need the full size of the layer while
    /// they are arranged read it here, because the <c>Bounds</c> of the layer are not updated in time. It is
    /// <c>default</c> before the first arrange pass.
    /// </summary>
    public Size AvailableSize { get; private set; }
    /// <summary>
    /// Whether <see cref="GetLayer" /> is injecting this layer into its host at the moment. The value is
    /// <see langword="true" /> from the assignment of <see cref="LayerHost" /> until the injection has finished, and
    /// it is reset even when adding the layer throws.
    /// </summary>
    public bool Injecting { get; private set; }
    
    /// <summary>
    /// Returns the overlay layer of the scope that <paramref name="visual" /> is in, injecting one into the scope
    /// when it has none. The scope is the nearest <see cref="ScopeAwareOverlayLayerPanel" />, or, when it is nearer,
    /// a surface host, that is a <c>VisualLayerManager</c> with <see cref="LayerLadder.IsSurfaceHostProperty" /> set.
    /// Without either, the scope is the nearest <c>VisualLayerManager</c> or the first one under the TopLevel, and
    /// finally the TopLevel itself.
    /// </summary>
    /// <param name="visual">
    /// The visual to resolve the scope for. The lookup of the scope panel starts with the visual itself.
    /// </param>
    /// <returns>
    /// The overlay layer of the scope, or <see langword="null" /> when neither a scope nor a TopLevel is found for
    /// <paramref name="visual" />.
    /// </returns>
    /// <remarks>
    /// A layer created for the TopLevel itself is returned without being inserted into it, because only a
    /// <c>VisualLayerManager</c> and a <see cref="ScopeAwareOverlayLayerPanel" /> take an injected layer.
    /// </remarks>
    public static ScopeAwareOverlayLayer? GetLayer(Visual visual)
    {
        Layoutable? layerHost = visual.FindAncestorOfType<ScopeAwareOverlayLayerPanel>(true);
        // Nearest scope wins: a surface host nearer than the viewport panel takes precedence, so a FloatButton
        // inside a Drawer in scrolling content stays on the Drawer surface.
        layerHost =   LayerLadder.PreferNearerSurfaceHost(visual, layerHost);
        layerHost ??= LayerLadder.FindContentLayerManager(visual);
        layerHost ??= TopLevel.GetTopLevel(visual);

        if (layerHost == null)
        {
            return null;
        }

        var layer = FindLayerInVisualChildren(layerHost);
        layer ??= InjectLayer(layerHost);

        return layer;
    }

    private static ScopeAwareOverlayLayer? FindLayerInVisualChildren(Visual visual)
    {
        foreach (var child in visual.GetVisualChildren())
        {
            if (child is ScopeAwareOverlayLayer layer)
            {
                return layer;
            }
        }

        return null;
    }
    
    private static ScopeAwareOverlayLayer InjectLayer(Layoutable layerHost)
    {
        var layer = FindLayer(layerHost);
        if (layer != null)
        {
            return layer;
        }
        layer = new ScopeAwareOverlayLayer
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment   = VerticalAlignment.Stretch,
            ZIndex              = LayerLadder.ContentFloating
        };
        try
        {
            layer.Injecting = true;
            layer.LayerHost = layerHost;
            if (layerHost is VisualLayerManager or ScopeAwareOverlayLayerPanel)
            {
                LayerLadder.AddLayer(layerHost, layer, LayerLadder.ContentFloating);
            }

            return layer;
        }
        finally
        {
            layer.Injecting = false;
        }
    }
     
    /// <summary>
    /// Returns the overlay layer that <paramref name="layerHost" /> already holds, without injecting one.
    /// </summary>
    /// <param name="layerHost">
    /// A <see cref="ScopeAwareOverlayLayerPanel" /> or a <c>VisualLayerManager</c>. A host of any other type yields
    /// <see langword="null" />.
    /// </param>
    /// <returns>
    /// The layer found in the host, or <see langword="null" /> when the host holds none.
    /// </returns>
    public static ScopeAwareOverlayLayer? FindLayer(Layoutable layerHost)
    {
        ScopeAwareOverlayLayer? layer = null;
        if (layerHost is ScopeAwareOverlayLayerPanel scopeAwareOverlayLayerPanel)
        {
            layer = scopeAwareOverlayLayerPanel.FindChildOfType<ScopeAwareOverlayLayer>();
        }
        else if (layerHost is VisualLayerManager visualLayerManager)
        {
            layer = visualLayerManager.FindChildOfType<ScopeAwareOverlayLayer>();
        }
        return layer;
    }
    
    /// <summary>
    /// Records <paramref name="finalSize" /> in <see cref="AvailableSize" /> and then arranges the children as
    /// <see cref="Canvas" /> does. An override that does not call the base member leaves the children unarranged and
    /// <see cref="AvailableSize" /> at its previous value.
    /// </summary>
    /// <param name="finalSize">
    /// The size the host assigned to the layer.
    /// </param>
    /// <returns>
    /// The size the layer takes up, as returned by the base implementation.
    /// </returns>
    protected override Size ArrangeOverride(Size finalSize)
    {
        // We are saving it here since child controls might need to know the entire size of the overlay
        // and Bounds won't be updated in time
        AvailableSize = finalSize;
        return base.ArrangeOverride(finalSize);
    }
}
