using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class ListBoxToken : AbstractControlDesignToken
{
    
    /// <summary>Padding around the list content.</summary>
    public partial Thickness ContentPadding { get; set; }

    /// <summary>Default foreground color of a list item, paired with <see cref="ItemBgColor"/>.</summary>
    public partial Color ItemColor { get; set; }

    /// <summary>Foreground color of a hovered list item, paired with <see cref="ItemHoverBgColor"/>.</summary>
    public partial Color ItemHoverColor { get; set; }

    /// <summary>Foreground color of a selected list item, paired with <see cref="ItemSelectedBgColor"/>.</summary>
    public partial Color ItemSelectedColor { get; set; }

    /// <summary>
    /// Colour of the check mark on a selected item.
    /// </summary>
    public partial Color ItemSelectedIndicatorColor { get; set; }

    /// <summary>Foreground color of a disabled list item.</summary>
    public partial Color ItemDisabledColor { get; set; }

    /// <summary>Default background color of a list item, paired with <see cref="ItemColor"/>.</summary>
    public partial Color ItemBgColor { get; set; }

    /// <summary>Background color of a hovered list item, paired with <see cref="ItemHoverColor"/>.</summary>
    public partial Color ItemHoverBgColor { get; set; }

    /// <summary>Background color of a selected list item, paired with <see cref="ItemSelectedColor"/>.</summary>
    public partial Color ItemSelectedBgColor { get; set; }
    
    /// <summary>Margin around the list item's selection indicator.</summary>
    public partial Thickness SelectedIndicatorMargin { get; set; }
    
    /// <summary>Foreground color of text that matches the filter.</summary>
    public partial Color FilterHighlightColor { get; set; }

    /// <summary>Padding inside a small list item.</summary>
    public partial Thickness ItemPaddingSM { get; set; }

    /// <summary>Padding inside a list item at the default size.</summary>
    public partial Thickness ItemPadding { get; set; }

    /// <summary>Padding inside a large list item.</summary>
    public partial Thickness ItemPaddingLG { get; set; }

    /// <summary>Margin around a list item.</summary>
    public partial Thickness ItemMargin { get; set; }
    
}
