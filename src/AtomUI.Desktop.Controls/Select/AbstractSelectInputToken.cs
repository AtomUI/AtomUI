using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public abstract partial class AbstractSelectInputToken : AbstractControlDesignToken
{

    /// <summary>
    /// 多选标签背景色
    /// Background color of multiple tag
    /// </summary>
    public partial Color MultipleItemBg { get; set; }

    /// <summary>
    /// 多选标签高度
    /// Height of multiple tag
    /// </summary>
    public partial double MultipleItemHeight { get; set; }

    /// <summary>
    /// 小号多选标签高度
    /// Height of multiple tag with small size
    /// </summary>
    public partial double MultipleItemHeightSM { get; set; }

    /// <summary>
    /// 大号多选标签高度
    /// Height of multiple tag with large size
    /// </summary>
    public partial double MultipleItemHeightLG { get; set; }

    /// <summary>
    /// 多选框禁用背景
    /// Background color of multiple selector when disabled
    /// </summary>
    public partial Color MultipleSelectorBgDisabled { get; set; }

    /// <summary>
    /// 多选标签禁用文本颜色
    /// Text color of multiple tag when disabled
    /// </summary>
    public partial Color MultipleItemColorDisabled { get; set; }
    
    /// <summary>
    /// 选项字体大小
    /// Font size of option
    /// </summary>
    public partial double OptionFontSize { get; set; }

    /// <summary>
    /// 小号选择框字体大小，默认等于 <see cref="DesignToken.FontSize"/>（与 antd 一致）
    /// Font size of the small selector, the middle font size by default (as in antd)
    /// </summary>
    public partial double InputFontSizeSM { get; set; }
    
    /// <summary>Padding around the tag panel in a regular multiple-selection field.</summary>
    public partial Thickness MultiModePadding { get; set; }

    /// <summary>Padding around the tag panel in a small multiple-selection field.</summary>
    public partial Thickness MultiModePaddingSM { get; set; }

    /// <summary>Padding around the tag panel in a large multiple-selection field.</summary>
    public partial Thickness MultiModePaddingLG { get; set; }

    /// <summary>Padding around the selected value in a regular single-selection field.</summary>
    public partial Thickness SingleModePadding { get; set; }

    /// <summary>Padding around the selected value in a small single-selection field.</summary>
    public partial Thickness SingleModePaddingSM { get; set; }

    /// <summary>Padding around the selected value in a large single-selection field.</summary>
    public partial Thickness SingleModePaddingLG { get; set; }

    /// <summary>Additional inset that aligns the multiple-selection prefix with the single-selection prefix.</summary>
    public partial Thickness MultiModePrefixIndent { get; set; }

    /// <summary>Additional prefix inset in a small multiple-selection field.</summary>
    public partial Thickness MultiModePrefixIndentSM { get; set; }

    /// <summary>Additional prefix inset in a large multiple-selection field.</summary>
    public partial Thickness MultiModePrefixIndentLG { get; set; }

}
