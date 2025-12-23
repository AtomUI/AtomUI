using AtomUI.Theme.TokenSystem;
using Avalonia.Animation.Easings;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class SplitViewToken : AbstractControlDesignToken
{
    public const string ID = "SplitView";

    public SplitViewToken()
        : base(ID)
    {
    }
    
    public double OpenPaneThemeLength { get; set; }
    public double CompactPaneThemeLength { get; set; }
    public TimeSpan PaneOpenMotionDuration { get; set; }
    public TimeSpan PaneCloseMotionDuration { get; set; }
    public Easing? PaneMotionEasing { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        OpenPaneThemeLength     = 320;
        CompactPaneThemeLength  = 48;
        PaneOpenMotionDuration  = TimeSpan.FromMilliseconds(200);
        PaneCloseMotionDuration = TimeSpan.FromSeconds(100);
        PaneMotionEasing        = Easing.Parse("0.1,0.9,0.2,1.0");
    }
}