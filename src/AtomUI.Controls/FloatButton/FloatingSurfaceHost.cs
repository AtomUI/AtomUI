using System.Reactive.Disposables;
using AtomUI.Controls.Primitives;
using AtomUI.Data;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;

namespace AtomUI.Controls.Commons;

/// <summary>
/// Provides the surface host of a floating control such as a FloatButton or a FloatButtonGroup. The floating
/// control is wrapped in a surface host, and that host is the direct child of the
/// <see cref="ScopeAwareOverlayLayer" />, so the focus adorner and the badge inside the floating control stay
/// close to its own surface and are not drawn above a sibling surface that is higher in the same layer.
/// <para>
/// The surface host is not declared in a ControlTheme because the floating control is created at run time by
/// FloatButtonHost or FloatButtonGroupHost and moved into the T4 layer of another visual root (the scoped
/// host), which the template of these hosts cannot reach. The template of FloatButton itself is also used for
/// inline scenarios (the child buttons of a FloatButtonGroup, IsEmbedMode), and declaring the surface host
/// there would give every inline button its own host.
/// </para>
/// <para>
/// The floating control still expresses its position in the layer through its own Canvas.Left and Canvas.Top.
/// The surface host follows these two values one way, and the layer arranges the host.
/// </para>
/// </summary>
internal static class FloatingSurfaceHost
{
    /// <summary>
    /// Wraps a floating control in a new surface host, adds the host to the layer and returns the host.
    /// </summary>
    /// <param name="layer">
    /// The layer that receives the surface host.
    /// </param>
    /// <param name="floating">
    /// The control to wrap.
    /// </param>
    /// <param name="disposables">
    /// Receives the two one-way bindings through which the surface host follows the position of the floating control.
    /// </param>
    /// <returns>
    /// The surface host, which <see cref="Detach" /> takes to remove the control from the layer.
    /// </returns>
    internal static VisualLayerManager Attach(ScopeAwareOverlayLayer layer,
                                              Control floating,
                                              CompositeDisposable disposables)
    {
        var host = LayerLadder.CreateSurfaceHost();
        disposables.Add(BindUtils.RelayBind(floating, Canvas.LeftProperty, host, Canvas.LeftProperty, BindingMode.OneWay));
        disposables.Add(BindUtils.RelayBind(floating, Canvas.TopProperty, host, Canvas.TopProperty, BindingMode.OneWay));
        // Add the host to the layer before attaching the child: when the parent of the floating control changes, the
        // host is already in the layer, and the floating control resolves its layer from that.
        layer.Children.Add(host);
        host.Child = floating;
        return host;
    }

    /// <summary>
    /// Removes the surface host from the layer and detaches the floating control from it, through which the
    /// floating control unsubscribes from the size changes of the layer. A <see langword="null" /> host is ignored.
    /// </summary>
    /// <param name="layer">
    /// The layer that holds the surface host. <see langword="null" /> skips the removal from the layer.
    /// </param>
    /// <param name="host">
    /// The surface host to remove.
    /// </param>
    internal static void Detach(ScopeAwareOverlayLayer? layer, VisualLayerManager? host)
    {
        if (host is null)
        {
            return;
        }

        layer?.Children.Remove(host);
        host.Child = null;
    }

    /// <summary>
    /// Returns the T4 layer that a floating control is in: the layer that is its logical parent, or the layer that
    /// holds the surface host which is its logical parent.
    /// </summary>
    /// <param name="parent">
    /// The logical parent of the floating control.
    /// </param>
    /// <returns>
    /// The layer, or <see langword="null" /> when the parent is neither a layer nor a surface host inside one.
    /// </returns>
    internal static ScopeAwareOverlayLayer? ResolveLayer(StyledElement? parent)
    {
        if (parent is ScopeAwareOverlayLayer layer)
        {
            return layer;
        }

        if (parent is VisualLayerManager manager && LayerLadder.GetIsSurfaceHost(manager))
        {
            return manager.Parent as ScopeAwareOverlayLayer;
        }

        return null;
    }
}
