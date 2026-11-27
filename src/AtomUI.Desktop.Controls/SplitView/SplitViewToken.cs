using AtomUI.Theme.DesignTokens;
using Avalonia.Animation.Easings;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design token of the <see cref="SplitView" />: the default length of the open pane and of the closed pane
/// in a compact display mode, and the duration and easing of the pane open and close animations.
/// </summary>
[ControlDesignToken]
public sealed partial class SplitViewToken : AbstractControlDesignToken
{
    
    /// <summary>
    /// Default length of the split pane when open.
    /// </summary>
    public partial double OpenPaneThemeLength { get; set; }
    /// <summary>
    /// Default length of the closed pane in a compact display mode.
    /// </summary>
    public partial double CompactPaneThemeLength { get; set; }
    /// <summary>
    /// Duration of the pane opening animation.
    /// </summary>
    public partial TimeSpan PaneOpenMotionDuration { get; set; }
    /// <summary>
    /// Duration of the pane closing animation.
    /// </summary>
    public partial TimeSpan PaneCloseMotionDuration { get; set; }
    /// <summary>
    /// Easing applied to the pane opening and closing animations.
    /// </summary>
    public partial Easing? PaneMotionEasing { get; set; }
    
}
