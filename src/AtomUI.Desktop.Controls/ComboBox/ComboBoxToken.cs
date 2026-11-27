using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens of the <see cref="ComboBox" />: the padding of the drop-down content area, the foreground and
/// background colors of a drop-down item in its normal, highlighted, selected and disabled states, the padding,
/// height and margin of an item, and the hover icon color of the drop-down handle.
/// </summary>
[ControlDesignToken]
public sealed partial class ComboBoxToken : AbstractControlDesignToken
{

    /// <summary>
    /// Padding inside the drop-down content area.
    /// </summary>
    public partial Thickness PopupContentPadding { get; set; }

    /// <summary>
    /// Default foreground color of a drop-down item, paired with <see cref="ItemBgColor" />.
    /// </summary>
    public partial Color ItemColor { get; set; }

    /// <summary>
    /// Foreground color of an enabled drop-down item that is highlighted as the candidate selection but is not
    /// selected: the candidate follows the mouse pointer and the Up and Down keys, and <c>Enter</c> commits it.
    /// Paired with <see cref="ItemHoverBgColor" />.
    /// </summary>
    public partial Color ItemHoverColor { get; set; }

    /// <summary>
    /// Foreground color of a selected drop-down item, paired with <see cref="ItemSelectedBgColor" />.
    /// </summary>
    public partial Color ItemSelectedColor { get; set; }

    /// <summary>
    /// Foreground color of a disabled drop-down item.
    /// </summary>
    public partial Color ItemDisabledColor { get; set; }

    /// <summary>
    /// Default background color of a drop-down item, paired with <see cref="ItemColor" />.
    /// </summary>
    public partial Color ItemBgColor { get; set; }

    /// <summary>
    /// Background color of an enabled drop-down item that is highlighted as the candidate selection but is not
    /// selected: the candidate follows the mouse pointer and the Up and Down keys, and <c>Enter</c> commits it.
    /// Paired with <see cref="ItemHoverColor" />.
    /// </summary>
    public partial Color ItemHoverBgColor { get; set; }

    /// <summary>
    /// Background color of a selected drop-down item, paired with <see cref="ItemSelectedColor" />.
    /// </summary>
    public partial Color ItemSelectedBgColor { get; set; }

    /// <summary>
    /// Padding inside a drop-down item.
    /// </summary>
    public partial Thickness ItemPadding { get; set; }

    /// <summary>
    /// Height of a drop-down item.
    /// </summary>
    public partial double ItemHeight { get; set; }

    /// <summary>
    /// Margin around a drop-down item.
    /// </summary>
    public partial Thickness ItemMargin { get; set; }

    /// <summary>
    /// Icon color of the drop-down handle on hover.
    /// </summary>
    public partial Color HandleHoverColor { get; set; }
    
}
