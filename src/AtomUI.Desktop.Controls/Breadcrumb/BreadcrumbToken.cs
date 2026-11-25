using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class BreadcrumbToken : AbstractControlDesignToken
{

    /// <summary>Foreground color of a breadcrumb item.</summary>
    public partial Color ItemColor { get; set; }

    /// <summary>Foreground color of the last breadcrumb item.</summary>
    public partial Color LastItemColor { get; set; }

    /// <summary>Foreground color of a breadcrumb link.</summary>
    public partial Color LinkColor { get; set; }

    /// <summary>Foreground color of a hovered breadcrumb link, paired with <see cref="LinkHoverBgColor"/>.</summary>
    public partial Color LinkHoverColor { get; set; }

    /// <summary>Background color of a hovered breadcrumb link, paired with <see cref="LinkHoverColor"/>.</summary>
    public partial Color LinkHoverBgColor { get; set; }

    /// <summary>Padding inside the border around breadcrumb item content.</summary>
    public partial Thickness BreadcrumbItemContentPadding { get; set; }

    /// <summary>Foreground color of the breadcrumb separator.</summary>
    public partial Color SeparatorColor { get; set; }

    /// <summary>Margin around the breadcrumb separator.</summary>
    public partial Thickness SeparatorMargin { get; set; }
    
}
