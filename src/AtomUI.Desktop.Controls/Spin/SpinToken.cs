using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens of the <see cref="Spin" /> control: the size of the indicator and of its dots for each size
/// preset, and the duration of one cycle of the loading animation.
/// </summary>
[ControlDesignToken]
public sealed partial class SpinToken : AbstractControlDesignToken
{

    /// <summary>
    /// Diameter of an individual loading dot at the standard size.
    /// </summary>
    public partial double DotSize { get; set; }

    /// <summary>
    /// Diameter of an individual loading dot at the small size.
    /// </summary>
    public partial double DotSizeSM { get; set; }

    /// <summary>
    /// Diameter of an individual loading dot at the large size.
    /// </summary>
    public partial double DotSizeLG { get; set; }
    
    /// <summary>
    /// Width and height of the large loading indicator.
    /// </summary>
    public partial double IndicatorSizeLG { get; set; }
    
    /// <summary>
    /// Width and height of the standard loading indicator.
    /// </summary>
    public partial double IndicatorSize { get; set; }
    
    /// <summary>
    /// Width and height of the small loading indicator.
    /// </summary>
    public partial double IndicatorSizeSM { get; set; }

    /// <summary>
    /// Duration of one cycle of the loading indicator animation.
    /// </summary>
    public partial TimeSpan IndicatorDuration { get; set; }
    
}
