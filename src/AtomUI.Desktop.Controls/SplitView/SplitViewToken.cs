using AtomUI.Theme.DesignTokens;
using Avalonia.Animation.Easings;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class SplitViewToken : AbstractControlDesignToken
{

    public SplitViewToken()

    {
    }
    
    public partial double OpenPaneThemeLength { get; set; }
    public partial double CompactPaneThemeLength { get; set; }
    public partial TimeSpan PaneOpenMotionDuration { get; set; }
    public partial TimeSpan PaneCloseMotionDuration { get; set; }
    public partial Easing? PaneMotionEasing { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        OpenPaneThemeLength     = 320;
        CompactPaneThemeLength  = 48;
        PaneOpenMotionDuration  = TimeSpan.FromMilliseconds(200);
        PaneCloseMotionDuration = TimeSpan.FromMilliseconds(100);
        PaneMotionEasing        = Easing.Parse("0.1,0.9,0.2,1.0");
    }
    
}
