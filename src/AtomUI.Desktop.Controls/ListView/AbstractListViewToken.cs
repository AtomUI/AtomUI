using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens shared by the list-based controls: the padding of the list area, the padding and margin of
/// an item in each size, the item text and background colors for the resting, hovered, selected and disabled
/// states, and the color of a group heading. Each design language assigns the values in its own token
/// calculator.
/// </summary>
[ControlDesignToken]
public abstract partial class AbstractListViewToken : AbstractControlDesignToken
{
    /// <summary>
    /// Padding inside the list content area.
    /// </summary>
    public partial Thickness ContentPadding { get; set; }

    /// <summary>
    /// Resting text color of a list item over <see cref="ItemBgColor" />.
    /// </summary>
    public partial Color ItemColor { get; set; }

    /// <summary>
    /// Text color of a hovered list item over <see cref="ItemHoverBgColor" />.
    /// </summary>
    public partial Color ItemHoverColor { get; set; }

    /// <summary>
    /// Text color of a selected list item over <see cref="ItemSelectedBgColor" />.
    /// </summary>
    public partial Color ItemSelectedColor { get; set; }

    /// <summary>
    /// Text color of a disabled list item.
    /// </summary>
    public partial Color ItemDisabledColor { get; set; }

    /// <summary>
    /// Resting background color behind <see cref="ItemColor" />.
    /// </summary>
    public partial Color ItemBgColor { get; set; }

    /// <summary>
    /// Background color of a hovered list item behind <see cref="ItemHoverColor" />.
    /// </summary>
    public partial Color ItemHoverBgColor { get; set; }

    /// <summary>
    /// Background color of a selected list item behind <see cref="ItemSelectedColor" />.
    /// </summary>
    public partial Color ItemSelectedBgColor { get; set; }

    /// <summary>
    /// Padding inside a small list item.
    /// </summary>
    public partial Thickness ItemPaddingSM { get; set; }

    /// <summary>
    /// Padding inside a standard list item.
    /// </summary>
    public partial Thickness ItemPadding { get; set; }

    /// <summary>
    /// Padding inside a large list item.
    /// </summary>
    public partial Thickness ItemPaddingLG { get; set; }

    /// <summary>
    /// Margin around each list item.
    /// </summary>
    public partial Thickness ItemMargin { get; set; }

    /// <summary>
    /// Text color of a list group heading.
    /// </summary>

    public partial Color GroupHeaderColor { get; set; }

}
