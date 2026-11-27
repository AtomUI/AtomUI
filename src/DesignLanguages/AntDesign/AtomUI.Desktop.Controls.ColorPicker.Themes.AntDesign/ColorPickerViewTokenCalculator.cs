using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign values of the shared <see cref="AbstractColorPickerViewToken" /> layer.
/// </summary>
public static class ColorPickerViewTokenCalculator
{
    /// <summary>
    /// Assigns the Ant Design values of the color picker view layer to <paramref name="token" />. The width of
    /// the view and of the preset panel, the editor font size, the handler sizes and the width of the input
    /// number handle are fixed numbers that do not depend on the global tokens. The remaining values, among them
    /// the alpha input width, the shadows, the spectrum height and the margins, are read from the global tokens;
    /// the dark handler color is the literal <c>#22075e</c>.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<AntDesignToken> context, DesignToken global, AbstractColorPickerViewToken token)
    {
        token.EditorFontSize = 11;
        token.ColorPickerWidth = 234;
        token.ColorPickerHandlerSizeLG = 32;
        token.ColorPickerHandlerSize = 16;
        // The 48px Ant alpha editor leaves a 30px text viewport once the border, the small input padding and the
        // 1px content inset are taken from each side; a theme with wider small padding widens the editor so that
        // "100%" keeps that viewport instead of being clipped.
        token.ColorPickerAlphaInputWidth = 30 + 2 * (global.LineWidth + global.InputPaddingSM.Left + 1);
        token.ColorPickerInputNumberHandleWidth = 16;
        token.ColorPickerPresetPanelWidth = 220;
        token.ColorPickerInsetShadow = new BoxShadows(new BoxShadow { IsInset = true, Blur = 1, Color = global.ColorTextQuaternary });
        token.ColorBlockInnerShadows = new BoxShadows(new BoxShadow { IsInset = true, Spread = global.LineWidth, Color = global.ColorFillSecondary });
        token.ColorSpectrumHeight = global.ControlHeightLG * 4;
        token.ColorPickerHandlerLightColor = global.ColorBgElevated;
        token.ColorPickerHandlerDarkColor = Color.Parse("#22075e");
        token.SliderContainerMargin = new Thickness(0, 0, global.UniformlyMarginSM, 0);
        token.TransparentBgSize = global.SizeXS;
        token.ColorPickerPresetColorGroupPadding = new Thickness(0, global.UniformlyPaddingXXS);
    }


    /// <summary>
    /// Sets <c>ColorPickerSliderSize</c> to the handler size and then derives the dependent sizes with
    /// <see cref="CalculateDependentSizes" />, which documents the parameters.
    /// </summary>
    public static void CalculateSliderSizes(AbstractColorPickerViewToken token, DesignToken global, double handlerSize, double presetSize, double trackSize)
    {
        token.ColorPickerSliderSize = handlerSize;
        CalculateDependentSizes(token, global, handlerSize, presetSize, trackSize);
    }


    /// <summary>
    /// Writes the three sizes that follow from the slider geometry: <c>CheckedMarkSize</c> is 60 percent of the
    /// preset size, <c>ColorPickerSliderThumbSize</c> is the handler size plus one line width on each side, and
    /// <c>ColorPickerPreviewSize</c> is twice the track size plus the small margin.
    /// </summary>
    /// <param name="token">
    /// The token layer that receives the three sizes.
    /// </param>
    /// <param name="global">
    /// The global tokens the line width and the small margin come from.
    /// </param>
    /// <param name="handlerSize">
    /// The size of the slider handler.
    /// </param>
    /// <param name="presetSize">
    /// The size of one preset color.
    /// </param>
    /// <param name="trackSize">
    /// The size of the slider track.
    /// </param>
    public static void CalculateDependentSizes(AbstractColorPickerViewToken token, DesignToken global, double handlerSize, double presetSize, double trackSize)
    {
        token.CheckedMarkSize = presetSize * 0.6;
        token.ColorPickerSliderThumbSize = handlerSize + global.LineWidth * 2;
        token.ColorPickerPreviewSize = trackSize * 2 + global.UniformlyMarginSM;
    }
}
