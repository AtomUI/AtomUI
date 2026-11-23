using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class MarqueeLabelToken : AbstractControlDesignToken
{
    
    /// <summary>Gap between successive copies of the scrolling text.</summary>
    public partial double CycleSpace { get; set; }

    /// <summary>Default scrolling speed, in pixels per second.</summary>
    public partial double DefaultSpeed { get; set; }
    
}
