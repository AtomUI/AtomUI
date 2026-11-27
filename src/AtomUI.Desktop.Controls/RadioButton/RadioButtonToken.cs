using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens of the desktop radio button: the indicator and dot sizes, the checked fills, the content
/// margin and the hover border color.
/// </summary>
[ControlDesignToken]
public sealed partial class RadioButtonToken : AbstractControlDesignToken
{
    
    /// <summary>
    /// Width and height of the indicator circle beside the label.
    /// </summary>
    public partial double RadioSize { get; set; }

    /// <summary>
    /// Diameter of the inner mark shown while the radio button is checked and enabled.
    /// </summary>
    public partial double DotSize { get; set; }

    /// <summary>
    /// Fill of the inner mark when the checked radio button is disabled.
    /// </summary>
    public partial Color DotColorDisabled { get; set; }

    /// <summary>
    /// Fill of the dot inside the indicator, shown when the radio button is checked. Internal token.
    /// </summary>
    public partial Color RadioColor { get; set; }

    /// <summary>
    /// Fill of the indicator circle when the radio button is checked. Internal token.
    /// </summary>
    public partial Color RadioBgColor { get; set; }

    /// <summary>
    /// Space between the indicator edge and the dot of a checked, disabled radio button. The dot is drawn with a
    /// diameter of <see cref="RadioSize" /> minus twice this value. It has no effect while the radio button is enabled.
    /// </summary>
    public partial double DotPadding { get; set; }

    /// <summary>
    /// Space between the radio indicator and its label.
    /// </summary>
    public partial Thickness TextMargin { get; set; }

    /// <summary>
    /// The border of an unchecked indicator under the pointer.
    /// </summary>
    public partial Color IndicatorHoverBorderColor { get; set; }
    
}
