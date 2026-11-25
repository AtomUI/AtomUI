using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class WindowToken : AbstractControlDesignToken
{

    /// <summary>Default fill of the window surface. Pair with <see cref="DefaultForeground"/>.</summary>
    public partial Color DefaultBackground { get; set; }

    /// <summary>Default foreground inherited by window content on <see cref="DefaultBackground"/>.</summary>
    public partial Color DefaultForeground { get; set; }
    
    /// <summary>Corner radius of the window frame.</summary>
    public partial CornerRadius CornerRadius { get; set; }
    
    /// <summary>Height of the window title bar area.</summary>
    public partial double TitleBarHeight { get; set; }
    
    /// <summary>Brush supplied to the platform system bar by the top-level theme.</summary>
    public partial SolidColorBrush? SystemBarColor { get; set; }
    
    /// <summary>Elevation shadow of the window frame.</summary>
    public partial BoxShadows FrameShadows { get; set; }
    
    /// <summary>
    /// Shadow of the full-screen title bar surface in the AutoHide and Pinned overlay modes.
    /// </summary>
    public partial BoxShadows FullscreenPopoverShadows { get; set; }
    
    /// <summary>Top-edge stroke drawn in the client area of an active Windows 10 window with client-side decorations.</summary>
    public partial Color FrameBorderActive { get; set; }

    /// <summary>
    /// Top-edge stroke drawn in the client area of an inactive Windows 10 window with client-side decorations.
    /// </summary>
    public partial Color FrameBorderInactive { get; set; }

}
