using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens of the <see cref="Cascader" /> control: the size of the control, the dropdown popup and one
/// option column, and the look of an option in its candidate and expanded states.
/// </summary>
[ControlDesignToken]
public sealed partial class CascaderToken : AbstractSelectInputToken
{
    
    /// <summary>
    /// Minimum height of a cascader item header.
    /// </summary>
    public partial double HeaderHeight { get; set; }
    
    /// <summary>
    /// Width of the <see cref="Cascader" /> control. The theme applies it only while the control's
    /// <c>HorizontalAlignment</c> is not <c>Stretch</c>.
    /// </summary>
    public partial double ControlWidth { get; set; }
    
    /// <summary>
    /// Minimum width of one column of the dropdown, the panel that stacks the options of one level.
    /// </summary>
    public partial double ControlItemWidth { get; set; }
    
    /// <summary>
    /// Default maximum height of the dropdown popup.
    /// </summary>
    public partial double DropdownHeight { get; set; }
    
    /// <summary>
    /// Background of an expanded option, the one whose children are shown in the next column.
    /// </summary>
    public partial Color OptionSelectedBg { get; set; }
    
    /// <summary>
    /// Background of an option with <c>IsCandidateSelected</c> set that is not expanded.
    /// </summary>
    /// <remarks>
    /// The pointer-over state on its own paints a transparent background.
    /// </remarks>
    public partial Color OptionHoverBg { get; set; }
    
    /// <summary>
    /// Foreground color of an expanded option, the one whose children are shown in the next column.
    /// </summary>
    public partial Color OptionSelectedColor { get; set; }
    
    /// <summary>
    /// Font weight of an expanded option, the one whose children are shown in the next column.
    /// </summary>
    public partial FontWeight OptionSelectedFontWeight { get; set; }
    
    /// <summary>
    /// Padding inside an option, between the option edge and its content.
    /// </summary>
    public partial Thickness OptionPadding { get; set; }
    
    /// <summary>
    /// Margin around the stacked options of one dropdown column. It insets the options from the edge of the
    /// column, while <see cref="OptionPadding" /> insets the content of a single option.
    /// </summary>
    public partial Thickness MenuPadding { get; set; }

    /// <summary>
    /// Outer margin of a menu item (a gap between items when non-zero).
    /// </summary>
    public partial Thickness OptionMargin { get; set; }

    /// <summary>
    /// Corner radius of a menu item's background.
    /// </summary>
    public partial CornerRadius OptionBorderRadius { get; set; }
    
    /// <summary>
    /// Foreground color of text that matches the filter.
    /// </summary>
    public partial Color FilterHighlightColor { get; set; }
    
    /// <summary>
    /// Gap between elements inside a cascader item header.
    /// </summary>
    public partial double ItemHeaderSpacing { get; set; }
    
}
