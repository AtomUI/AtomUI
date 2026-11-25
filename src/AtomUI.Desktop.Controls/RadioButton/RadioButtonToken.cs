using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class RadioButtonToken : AbstractControlDesignToken
{
    
    /// <summary>Width and height of the indicator circle beside the label.</summary>
    public partial double RadioSize { get; set; }

    /// <summary>Diameter of the inner mark shown when checked.</summary>
    public partial double DotSize { get; set; }

    /// <summary>Fill of the inner mark when the checked radio button is disabled.</summary>
    public partial Color DotColorDisabled { get; set; }

    /// <summary>
    /// 选中时圆点颜色（内部使用）
    /// Dot color of a checked radio (internal)
    /// </summary>
    public partial Color RadioColor { get; set; }

    /// <summary>
    /// 选中时单选框背景色（内部使用）
    /// Background of a checked radio (internal)
    /// </summary>
    public partial Color RadioBgColor { get; set; }

    /// <summary>Inset used to position the inner mark within the radio indicator.</summary>
    public partial double DotPadding { get; set; }

    /// <summary>Space between the radio indicator and its label.</summary>
    public partial Thickness TextMargin { get; set; }

    /// <summary>The border of an unchecked indicator under the pointer.</summary>
    public partial Color IndicatorHoverBorderColor { get; set; }
    
}
