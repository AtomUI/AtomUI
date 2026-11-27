using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens that the dialog surface is painted with, shared by the controls that present one: the
/// <see cref="Dialog" /> through <see cref="DialogToken" />, and the message box through its own token type.
/// Each design language fills the values in its token calculator, and the theme of the presenting control maps
/// them onto the surface. A window host hides the header of the surface because the native window owns the
/// title bar, so the header tokens take effect only in the overlay host.
/// </summary>
[ControlDesignToken]
public abstract partial class AbstractDialogToken : AbstractControlDesignToken
{

    /// <summary>
    /// Background color of the dialog header.
    /// </summary>
    public partial Color HeaderBg { get; set; }

    /// <summary>
    /// Space below the dialog header.
    /// </summary>
    public partial Thickness HeaderMarginBottom { get; set; }

    /// <summary>
    /// Width and height of the icon at the start of the dialog header. The icon is shown only when a title icon
    /// is set.
    /// </summary>
    public partial double LogoSize { get; set; }

    /// <summary>
    /// Font size of the dialog title.
    /// </summary>
    public partial double HeaderFontSize { get; set; }

    /// <summary>
    /// Foreground color of the dialog title.
    /// </summary>
    public partial Color HeaderColor { get; set; }

    /// <summary>
    /// Background color of the frame of the dialog surface. The header and the footer are drawn on top of it with
    /// their own backgrounds.
    /// </summary>
    public partial Color ContentBg { get; set; }

    /// <summary>
    /// Padding inside the dialog's title area.
    /// </summary>
    public partial Thickness HeaderPadding { get; set; }

    /// <summary>
    /// Padding inside the dialog content area.
    /// </summary>
    public partial Thickness ContentPadding { get; set; }

    /// <summary>
    /// Padding inside the content area of a dialog hosted in a native window.
    /// </summary>
    public partial Thickness WindowContentPadding { get; set; }

    /// <summary>
    /// Background color of the dialog footer, the strip below the content area that hosts the dialog buttons.
    /// </summary>
    public partial Color FooterBg { get; set; }

    /// <summary>
    /// Padding inside the dialog footer.
    /// </summary>
    public partial Thickness FooterPadding { get; set; }

    /// <summary>
    /// Space above the dialog footer.
    /// </summary>
    public partial Thickness FooterMarginTop { get; set; }

    /// <summary>
    /// Width and height of the caption buttons in the dialog header: the close button and the maximize button.
    /// </summary>
    public partial double CloseBtnSize { get; set; }

    /// <summary>
    /// Default minimum height of the dialog.
    /// </summary>
    public partial double MinHeight { get; set; }

    /// <summary>
    /// Default minimum width of the dialog.
    /// </summary>
    public partial double MinWidth { get; set; }

    /// <summary>
    /// Horizontal spacing between buttons in the dialog footer.
    /// </summary>
    public partial double ButtonGroupSpacing { get; set; }

    /// <summary>
    /// Margin around the dialog's loading indicator.
    /// </summary>
    public partial Thickness LoadingIndicatorMargin { get; set; }
}
