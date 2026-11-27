using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design token of the window title bar. Beyond the action colors inherited from
/// <see cref="AbstractTitleBarActionToken" />, it defines the height and padding of the bar, the title font, the
/// logo and its spacing, the spacing between the bar's regions and between its caption buttons, and the Windows
/// caption glyph size and close button colors.
/// </summary>
[ControlDesignToken]
public sealed partial class WindowTitleBarToken : AbstractTitleBarActionToken
{
    /// <summary>
    /// Gap between the application logo and window title.
    /// </summary>
    public partial double LogoAndTitleSpacing { get; set; }

    /// <summary>
    /// Gap between the application logo and the left add-on on Windows and Linux.
    /// </summary>
    public partial double LogoAndLeftAddOnSpacing { get; set; }
    
    /// <summary>
    /// Padding inside the window title bar.
    /// </summary>
    public partial Thickness TitleBarPadding { get; set; }
    
    /// <summary>
    /// Default font size of the window title.
    /// </summary>
    public partial double TitleFontSize { get; set; }
    
    /// <summary>
    /// Default font weight of the window title.
    /// </summary>
    public partial FontWeight TitleFontWeight { get; set; }
    

    /// <summary>
    /// Windows caption glyph size aligned with Avalonia's native-style window decorations.
    /// </summary>
    public partial double WindowsCaptionIconSize { get; set; }
    
    /// <summary>
    /// Size of the application logo in the title bar.
    /// </summary>
    public partial double LogoSize { get; set; }
    
    
    
    
    
    
    /// <summary>
    /// Icon color of the Windows close button on hover and press, paired with
    /// <see cref="WindowsCloseButtonHoverBgColor" /> and <see cref="WindowsCloseButtonPressedBgColor" />,
    /// respectively.
    /// </summary>
    public partial Color WindowsCloseButtonHoverColor { get; set; }
    
    /// <summary>
    /// Background color of the hovered Windows close button, paired with
    /// <see cref="WindowsCloseButtonHoverColor" />.
    /// </summary>
    public partial Color WindowsCloseButtonHoverBgColor { get; set; }
    
    /// <summary>
    /// Background color of the pressed Windows close button, paired with
    /// <see cref="WindowsCloseButtonHoverColor" />.
    /// </summary>
    public partial Color WindowsCloseButtonPressedBgColor { get; set; }
    
    
    


    
    /// <summary>
    /// Spacing between caption buttons and between items in the title bar's trailing group.
    /// </summary>
    public partial double CaptionGroupSpacing { get; set; }
    
    /// <summary>
    /// Height of the window title bar.
    /// </summary>
    public partial double Height { get; set; }

    /// <summary>
    /// Horizontal spacing between the title bar's layout regions.
    /// </summary>
    public partial double HeaderHorizontalSpacing { get; set; }

}
