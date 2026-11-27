using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens of <see cref="MarqueeLabel" />: the gap between successive copies of its scrolling text and
/// its scrolling speed.
/// </summary>
[ControlDesignToken]
public sealed partial class MarqueeLabelToken : AbstractControlDesignToken
{
    
    /// <summary>
    /// Gap between successive copies of the scrolling text, in DIPs.
    /// </summary>
    public partial double CycleSpace { get; set; }

    /// <summary>
    /// Default value of the <c>MoveSpeed</c> property of a <see cref="MarqueeLabel" />. The text travels a quarter
    /// of it in DIPs per second.
    /// </summary>
    public partial double DefaultSpeed { get; set; }
    
}
