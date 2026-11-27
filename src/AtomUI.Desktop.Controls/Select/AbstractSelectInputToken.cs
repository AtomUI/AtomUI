using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens that Select, TreeSelect and Cascader share: the tags of a multiple-selection field, its
/// disabled colors, the paddings of the selector and the font sizes of the selector and the options.
/// </summary>
[ControlDesignToken]
public abstract partial class AbstractSelectInputToken : AbstractControlDesignToken
{

    /// <summary>
    /// Background color of a tag in a multiple-selection field.
    /// </summary>
    public partial Color MultipleItemBg { get; set; }

    /// <summary>
    /// Height of a tag in a regular multiple-selection field.
    /// </summary>
    public partial double MultipleItemHeight { get; set; }

    /// <summary>
    /// Height of a tag in a small multiple-selection field.
    /// </summary>
    public partial double MultipleItemHeightSM { get; set; }

    /// <summary>
    /// Height of a tag in a large multiple-selection field.
    /// </summary>
    public partial double MultipleItemHeightLG { get; set; }

    /// <summary>
    /// Background color of the selector in a multiple-selection field while the select is disabled.
    /// </summary>
    public partial Color MultipleSelectorBgDisabled { get; set; }

    /// <summary>
    /// Text color of a tag in a multiple-selection field while the select is disabled.
    /// </summary>
    public partial Color MultipleItemColorDisabled { get; set; }
    
    /// <summary>
    /// Font size of an option in the dropdown list.
    /// </summary>
    public partial double OptionFontSize { get; set; }

    /// <summary>
    /// Font size of the selector in a small select. It equals <see cref="DesignToken.FontSize" />, the middle font
    /// size, by default.
    /// </summary>
    public partial double InputFontSizeSM { get; set; }
    
    /// <summary>
    /// Padding around the tag panel in a regular multiple-selection field.
    /// </summary>
    public partial Thickness MultiModePadding { get; set; }

    /// <summary>
    /// Padding around the tag panel in a small multiple-selection field.
    /// </summary>
    public partial Thickness MultiModePaddingSM { get; set; }

    /// <summary>
    /// Padding around the tag panel in a large multiple-selection field.
    /// </summary>
    public partial Thickness MultiModePaddingLG { get; set; }

    /// <summary>
    /// Padding around the selected value in a regular single-selection field.
    /// </summary>
    public partial Thickness SingleModePadding { get; set; }

    /// <summary>
    /// Padding around the selected value in a small single-selection field.
    /// </summary>
    public partial Thickness SingleModePaddingSM { get; set; }

    /// <summary>
    /// Padding around the selected value in a large single-selection field.
    /// </summary>
    public partial Thickness SingleModePaddingLG { get; set; }

    /// <summary>
    /// Additional inset that aligns the multiple-selection prefix with the single-selection prefix.
    /// </summary>
    public partial Thickness MultiModePrefixIndent { get; set; }

    /// <summary>
    /// Additional prefix inset in a small multiple-selection field.
    /// </summary>
    public partial Thickness MultiModePrefixIndentSM { get; set; }

    /// <summary>
    /// Additional prefix inset in a large multiple-selection field.
    /// </summary>
    public partial Thickness MultiModePrefixIndentLG { get; set; }

}
