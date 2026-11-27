using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens of the <see cref="Breadcrumb" /> control: the colors of item content and icons per state,
/// the padding of an item and the color and margin of the separators.
/// </summary>
/// <remarks>
/// An item that has a navigation target, that is a <see cref="BreadcrumbItem.NavigateUri" /> or a
/// <see cref="BreadcrumbItem.NavigateContext" />, is painted as a link.
/// </remarks>
[ControlDesignToken]
public sealed partial class BreadcrumbToken : AbstractControlDesignToken
{

    /// <summary>
    /// Foreground color of the content of an item that is neither a link nor the last one, and of the icon of
    /// every item at rest.
    /// </summary>
    public partial Color ItemColor { get; set; }

    /// <summary>
    /// Foreground color of the content of the last item while it is not a link.
    /// </summary>
    public partial Color LastItemColor { get; set; }

    /// <summary>
    /// Foreground color of the content of a link.
    /// </summary>
    public partial Color LinkColor { get; set; }

    /// <summary>
    /// Foreground color of the content and the icon of a link while the pointer is over the item.
    /// </summary>
    public partial Color LinkHoverColor { get; set; }

    /// <summary>
    /// Background color of a link while the pointer is over the item.
    /// </summary>
    public partial Color LinkHoverBgColor { get; set; }

    /// <summary>
    /// Padding inside the border around breadcrumb item content.
    /// </summary>
    public partial Thickness BreadcrumbItemContentPadding { get; set; }

    /// <summary>
    /// Foreground color of the breadcrumb separator.
    /// </summary>
    public partial Color SeparatorColor { get; set; }

    /// <summary>
    /// Margin around the breadcrumb separator.
    /// </summary>
    public partial Thickness SeparatorMargin { get; set; }
    
}
