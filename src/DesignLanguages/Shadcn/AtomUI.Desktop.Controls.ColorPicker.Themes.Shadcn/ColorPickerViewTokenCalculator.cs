using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn values of the shared <see cref="AbstractColorPickerViewToken" /> layer.
/// </summary>
public static class ColorPickerViewTokenCalculator
{
    /// <summary>
    /// Fills the <see cref="AbstractColorPickerViewToken" /> layer with the shadcn values: the sizes of the
    /// editor, the handlers, the preset panel and the color spectrum, the alpha input width, the inset shadows,
    /// the handler colors and the margins.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<ShadcnToken> context, DesignToken global, AbstractColorPickerViewToken token)
    {
        // Ant Design formula kept as is: shadcn has no color picker, so the language has no values of its own.
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
    /// Sets the slider size to <paramref name="handlerSize" /> and then derives the sizes that follow the slider
    /// geometry through <see cref="CalculateDependentSizes" />.
    /// </summary>
    public static void CalculateSliderSizes(AbstractColorPickerViewToken token, DesignToken global, double handlerSize, double presetSize, double trackSize)
    {
        token.ColorPickerSliderSize = handlerSize;
        CalculateDependentSizes(token, global, handlerSize, presetSize, trackSize);
    }


    /// <summary>
    /// Derives the sizes that follow the slider geometry: the checked mark as 60 percent of
    /// <paramref name="presetSize" />, the slider thumb as <paramref name="handlerSize" /> plus twice the line
    /// width, and the preview as twice <paramref name="trackSize" /> plus the small uniform margin.
    /// </summary>
    public static void CalculateDependentSizes(AbstractColorPickerViewToken token, DesignToken global, double handlerSize, double presetSize, double trackSize)
    {
        token.CheckedMarkSize = presetSize * 0.6;
        token.ColorPickerSliderThumbSize = handlerSize + global.LineWidth * 2;
        token.ColorPickerPreviewSize = trackSize * 2 + global.UniformlyMarginSM;
    }
}
