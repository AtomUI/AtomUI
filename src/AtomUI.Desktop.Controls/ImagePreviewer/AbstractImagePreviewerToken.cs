using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens shared by the <see cref="ImagePreviewer" /> and the
/// <see cref="ImageGroupPreviewer" />: preview operation icons, navigation buttons, floating toolbar,
/// preview dialog and cover.
/// </summary>
[ControlDesignToken]
public abstract partial class AbstractImagePreviewerToken : AbstractControlDesignToken
{

    /// <summary>
    /// Size, in device-independent pixels, of the icons on the preview operation buttons, such as the
    /// floating toolbar actions and the previous and next navigation buttons.
    /// </summary>
    public partial double PreviewOperationSize { get; set; }

    /// <summary>
    /// Color of the preview operation icons in the normal state.
    /// </summary>
    public partial Color PreviewOperationColor { get; set; }

    /// <summary>
    /// Color of a preview operation icon while its button is hovered.
    /// </summary>
    public partial Color PreviewOperationHoverColor { get; set; }

    /// <summary>
    /// Color of a preview operation icon while the operation is disabled.
    /// </summary>
    public partial Color PreviewOperationColorDisabled { get; set; }


    /// <summary>
    /// Size of the placeholder icon shown when an image failed to load. This is an AtomUI extension: antd
    /// shows a fallback image instead.
    /// </summary>
    public partial double ErrorIconSize { get; set; }

    /// <summary>
    /// Background color of the mask over the preview cover.
    /// </summary>
    public partial Color MaskBgColor { get; set; }

    /// <summary>
    /// Minimum width of the image preview dialog.
    /// </summary>
    public partial double DialogMinWidth { get; set; }

    /// <summary>
    /// Minimum height of the image preview dialog.
    /// </summary>
    public partial double DialogMinHeight { get; set; }

    /// <summary>
    /// Background color of the preview window's title bar.
    /// </summary>
    public partial Color TitleBarBackgroundColor { get; set; }

    /// <summary>
    /// Default background color of the floating preview navigation buttons, paired with
    /// <see cref="PreviewOperationColor" />.
    /// </summary>
    public partial Color NavButtonBgColor { get; set; }

    /// <summary>
    /// Background color of the floating preview navigation buttons on hover or press, paired with
    /// <see cref="PreviewOperationHoverColor" />.
    /// </summary>
    public partial Color NavButtonBgHoverColor { get; set; }

    /// <summary>
    /// Padding inside the floating toolbar.
    /// </summary>
    public partial Thickness FloatToolbarPadding { get; set; }

    /// <summary>
    /// Padding around the image index indicator in the floating toolbar.
    /// </summary>
    public partial Thickness FloatToolbarIndicatorPadding { get; set; }

    /// <summary>
    /// Default width of the preview cover image.
    /// </summary>
    public partial double CoverImageWidth { get; set; }

}
