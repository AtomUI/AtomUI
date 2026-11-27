using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Own tokens of the <see cref="Timeline" />: the indicator with its dot, margins and tail line, and the gap
/// between the items of a vertical timeline.
/// </summary>
[ControlDesignToken]
public sealed partial class TimelineToken : AbstractControlDesignToken
{
    /// <summary>
    /// Color of the line connecting timeline indicators.
    /// </summary>
    public partial Color IndicatorTailColor { get; set; }

    /// <summary>
    /// Thickness of the line connecting timeline indicators.
    /// </summary>
    public partial double IndicatorTailWidth { get; set; }

    /// <summary>
    /// Gap below each item to the next one in a vertical timeline. The gap beside the pending item uses
    /// <see cref="ItemPaddingBottomLG" /> instead, and the last item has no gap.
    /// </summary>
    public partial Thickness ItemPaddingBottom { get; set; }

    /// <summary>
    /// Larger gap between the items that surround the pending one in a vertical timeline. It replaces
    /// <see cref="ItemPaddingBottom" /> on the item before the pending item, or on the pending item itself when
    /// the timeline is reversed.
    /// </summary>
    public partial Thickness ItemPaddingBottomLG { get; set; }

    /// <summary>
    /// Cross-axis size of the timeline indicator: its width when the timeline is vertical and its height when it is
    /// horizontal. It is also the width and height of a custom indicator icon.
    /// </summary>
    public partial double IndicatorSize { get; set; }

    /// <summary>
    /// Diameter of the built-in indicator dot.
    /// </summary>
    public partial double IndicatorDotSize { get; set; }
    
    /// <summary>
    /// Margin around the indicator of each item in a vertical timeline whose mode is Start.
    /// </summary>
    public partial Thickness IndicatorStartModeMargin { get; set; }
    
    /// <summary>
    /// Margin around the indicator of each item in a vertical timeline whose mode is End.
    /// </summary>
    public partial Thickness IndicatorEndModeMargin { get; set; }
    
    /// <summary>
    /// Margin around the indicator of each item in a vertical timeline in Alternate mode, or in any mode while at
    /// least one item has a label.
    /// </summary>
    public partial Thickness IndicatorMiddleModeMargin { get; set; }

    /// <summary>
    /// Width of the indicator dot's border.
    /// </summary>
    public partial double IndicatorDotBorderWidth { get; set; }
    
}
