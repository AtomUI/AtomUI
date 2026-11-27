using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens that the color picker view and the gradient color picker view share for the editor panel:
/// the color spectrum, the sliders, the textual editors and the preset groups. A design language fills the
/// values in its token calculator.
/// </summary>
[ControlDesignToken]
public abstract partial class AbstractColorPickerViewToken : AbstractControlDesignToken
{
    /// <summary>
    /// Width of the editor panel that holds the spectrum, editors, and preset groups.
    /// </summary>
    public partial double ColorPickerWidth { get; set; }

    /// <summary>
    /// Height of the two-dimensional saturation and value spectrum.
    /// </summary>
    public partial double ColorSpectrumHeight { get; set; }

    /// <summary>
    /// Inset outline drawn over the hue, opacity, and gradient slider tracks.
    /// </summary>
    public partial BoxShadows ColorPickerInsetShadow { get; set; }

    /// <summary>
    /// Diameter of the outer spectrum selection handle.
    /// </summary>
    public partial double ColorPickerHandlerSizeLG { get; set; }

    /// <summary>
    /// Diameter of the normal spectrum selection handle.
    /// </summary>
    public partial double ColorPickerHandlerSize { get; set; }

    /// <summary>
    /// Small spectrum handle size used to derive the other handle sizes.
    /// </summary>
    [ReservedToken("A derivation input: the design language derives other tokens of this control from it, so overriding it moves them.")]
    public partial double ColorPickerHandlerSizeSM { get; set; }

    /// <summary>
    /// Light contour of the spectrum handle, paired with <see cref="ColorPickerHandlerDarkColor" /> for contrast
    /// against the spectrum.
    /// </summary>
    public partial Color ColorPickerHandlerLightColor { get; set; }

    /// <summary>
    /// Dark contour of the spectrum handle, paired with <see cref="ColorPickerHandlerLightColor" /> for contrast
    /// against the spectrum.
    /// </summary>
    public partial Color ColorPickerHandlerDarkColor { get; set; }

    /// <summary>
    /// Height of a color slider’s control box, including the space around its track and thumb.
    /// </summary>
    public partial double ColorPickerSliderSize { get; set; }

    /// <summary>
    /// Height of the painted slider track inside <see cref="ColorPickerSliderSize" />.
    /// </summary>
    public partial double ColorPickerSliderTrackSize { get; set; }

    /// <summary>
    /// Width and height of the handle that moves along a color slider.
    /// </summary>
    public partial double ColorPickerSliderThumbSize { get; set; }

    /// <summary>
    /// Width and height of the preview swatch beside the slider group.
    /// </summary>
    public partial double ColorPickerPreviewSize { get; set; }

    /// <summary>
    /// Width of the opacity editor in the panel’s input row.
    /// </summary>
    public partial double ColorPickerAlphaInputWidth { get; set; }

    /// <summary>
    /// Width of the increment and decrement button area in numeric color editors.
    /// </summary>
    public partial double ColorPickerInputNumberHandleWidth { get; set; }

    /// <summary>
    /// Width and height of a preset swatch, including the area for its selection mark.
    /// </summary>
    public partial double ColorPickerPresetColorSize { get; set; }

    /// <summary>
    /// Width used to wrap the swatches inside a preset group.
    /// </summary>
    public partial double ColorPickerPresetPanelWidth { get; set; }

    /// <summary>
    /// Width and height of the check mark over a selected preset color swatch.
    /// </summary>
    public partial double CheckedMarkSize { get; set; }

    /// <summary>
    /// Padding inside a preset color group around the swatch list.
    /// </summary>
    public partial Thickness ColorPickerPresetColorGroupPadding { get; set; }

    /// <summary>
    /// Space around the hue, opacity, and gradient slider group.
    /// </summary>
    public partial Thickness SliderContainerMargin { get; set; }

    /// <summary>
    /// Inset outline around a color swatch inside the editor panel.
    /// </summary>
    public partial BoxShadows ColorBlockInnerShadows { get; set; }

    /// <summary>
    /// Size of a tile in the checkerboard behind translucent editor swatches.
    /// </summary>
    public partial double TransparentBgSize { get; set; }

    /// <summary>
    /// Font size of the textual color-value editors.
    /// </summary>
    public partial double EditorFontSize { get; set; }
}
