using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class SpaceToken : AbstractControlDesignToken
{

    /// <summary>Gap between adjacent children in the small spacing preset.</summary>
    public partial double GapSmallSize  { get; set; }
    /// <summary>Gap between adjacent children in the middle spacing preset.</summary>
    public partial double GapMiddleSize  { get; set; }
    
    /// <summary>Gap between adjacent children in the large spacing preset.</summary>
    public partial double GapLargeSize  { get; set; }
    
}
