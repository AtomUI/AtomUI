using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class TimelineToken : AbstractControlDesignToken
{
    /// <summary>Color of the line connecting timeline indicators.</summary>
    public partial Color IndicatorTailColor { get; set; }

    /// <summary>Thickness of the line connecting timeline indicators.</summary>
    public partial double IndicatorTailWidth { get; set; }

    /// <summary>Bottom padding of an item in a vertical timeline.</summary>
    public partial Thickness ItemPaddingBottom { get; set; }

    /// <summary>Larger bottom padding used around a pending item in a vertical timeline.</summary>
    public partial Thickness ItemPaddingBottomLG { get; set; }

    /// <summary>Cross-axis size of the timeline indicator.</summary>
    public partial double IndicatorSize { get; set; }

    /// <summary>Diameter of the built-in indicator dot.</summary>
    public partial double IndicatorDotSize { get; set; }
    
    /// <summary>Margin around the indicator in Start mode.</summary>
    public partial Thickness IndicatorStartModeMargin { get; set; }
    
    /// <summary>Margin around the indicator in End mode.</summary>
    public partial Thickness IndicatorEndModeMargin { get; set; }
    
    /// <summary>Margin around the indicator in Middle mode.</summary>
    public partial Thickness IndicatorMiddleModeMargin { get; set; }

    /// <summary>Width of the indicator dot's border.</summary>
    public partial double IndicatorDotBorderWidth { get; set; }
    
}
