using System.Diagnostics;
using AtomUI.Controls.Primitives;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// T6 window chrome layer (<see cref="LayerLadder.WindowChrome"/>): hosts the full-screen title bar above every
/// content role (decorations, scoped overlays, floating surfaces, scoped presentation) and below the TopLevel roles
/// (Window Drawer, Tour, Dialog, popups, Message/Notification).
/// </summary>
/// <remarks>
/// The layer itself has no background, so only its children take part in hit testing. Its single child is a surface
/// host (an Adorner-only nested <see cref="VisualLayerManager"/>) whose child is the template-declared
/// <see cref="FullScreenTitleBarHost"/>; focus outlines and badges inside the title bar therefore render in the
/// layer's own decoration layer instead of the window's shared <see cref="AdornerLayer"/> below it.
/// The host stays declared in the Window templates, so its <c>TemplateBinding</c> contract is unchanged; the window
/// only moves it in here when the template is applied and gives it back on release. WindowChromeLayerTheme projects
/// the manager's Padding (OffScreenMargin) onto the layer's Margin, so the surface keeps the rect it had as part of the
/// manager's Child.
/// </remarks>
[global::System.Runtime.Versioning.UnsupportedOSPlatform("browser")]
internal sealed class WindowChromeLayer : Panel
{
    private readonly VisualLayerManager _surfaceHost;

    internal WindowChromeLayer()
    {
        HorizontalAlignment = HorizontalAlignment.Stretch;
        VerticalAlignment   = VerticalAlignment.Stretch;
        _surfaceHost        = LayerLadder.CreateSurfaceHost();
        Children.Add(_surfaceHost);
    }

    /// <summary>The title bar host currently presented by this layer.</summary>
    internal FullScreenTitleBarHost? HostedTitleBar => _surfaceHost.Child as FullScreenTitleBarHost;

    /// <summary>
    /// Moves <paramref name="host"/> from its template parent into this layer's surface host.
    /// </summary>
    internal void Host(FullScreenTitleBarHost host)
    {
        if (ReferenceEquals(_surfaceHost.Child, host))
        {
            return;
        }

        Release();
        switch (host.Parent)
        {
            case Panel panel:
                panel.Children.Remove(host);
                break;
            case Decorator decorator:
                decorator.Child = null;
                break;
            case { } other:
                // A custom template placed the host under a parent it cannot be detached from: leave it in place
                // (presented in the content root as before) instead of failing the template application.
                Debug.Assert(false, $"Unsupported full-screen title bar host parent: {other.GetType()}");
                return;
        }

        _surfaceHost.Child = host;
    }

    /// <summary>
    /// Detaches the hosted title bar host. The host is not returned to its template parent: release only happens
    /// when that template is discarded or the window closes.
    /// </summary>
    internal void Release()
    {
        _surfaceHost.Child = null;
    }
}
