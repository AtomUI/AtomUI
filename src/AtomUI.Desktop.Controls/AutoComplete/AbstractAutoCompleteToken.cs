using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public abstract partial class AbstractAutoCompleteToken : AbstractControlDesignToken
{

    /// <summary>Padding inside the suggestion popup.</summary>
    public partial Thickness PopupContentPadding { get; set; }

    /// <summary>
    /// 选项高度
    /// Height of option
    /// </summary>
    public partial double OptionHeight { get; set; }

    /// <summary>
    /// Outer margin of a candidate row (a gap between rows when non-zero).
    /// </summary>
    public partial Thickness OptionMargin { get; set; }

    /// <summary>
    /// Corner radius of a candidate row's background.
    /// </summary>
    public partial CornerRadius OptionBorderRadius { get; set; }

    /// <summary>
    /// 候选项文字颜色
    /// Text colour of a candidate
    /// </summary>
    public partial Color OptionColor { get; set; }

    /// <summary>
    /// 选中候选项文字颜色
    /// Text colour of the selected candidate
    /// </summary>
    public partial Color OptionSelectedColor { get; set; }

    /// <summary>Minimum width of the suggestion popup.</summary>
    public partial double MinPopupWidth { get; set; }

    /// <summary>Maximum width of the suggestion popup when its width does not follow the anchor.</summary>
    public partial double MaxPopupWidth { get; set; }

}
