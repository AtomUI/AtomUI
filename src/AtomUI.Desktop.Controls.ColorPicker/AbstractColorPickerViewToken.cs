using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public abstract partial class AbstractColorPickerViewToken : AbstractControlDesignToken
{
    public partial double ColorPickerWidth { get; set; }

    public partial double ColorSpectrumHeight { get; set; }

    public partial BoxShadows ColorPickerInsetShadow { get; set; }

    public partial double ColorPickerHandlerSizeLG { get; set; }

    public partial double ColorPickerHandlerSize { get; set; }

    public partial double ColorPickerHandlerSizeSM { get; set; }

    public partial Color ColorPickerHandlerLightColor { get; set; }

    public partial Color ColorPickerHandlerDarkColor { get; set; }

    public partial double ColorPickerSliderSize { get; set; }

    public partial double ColorPickerSliderTrackSize { get; set; }

    public partial double ColorPickerSliderThumbSize { get; set; }

    public partial double ColorPickerPreviewSize { get; set; }

    public partial double ColorPickerAlphaInputWidth { get; set; }

    public partial double ColorPickerInputNumberHandleWidth { get; set; }

    public partial double ColorPickerPresetColorSize { get; set; }

    public partial double ColorPickerPresetPanelWidth { get; set; }

    public partial double CheckedMarkSize { get; set; }

    public partial Thickness ColorPickerPresetColorGroupPadding { get; set; }

    public partial Thickness SliderContainerMargin { get; set; }

    public partial BoxShadows ColorBlockInnerShadows { get; set; }

    public partial double TransparentBgSize { get; set; }

    public partial double EditorFontSize { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        EditorFontSize = 11;
        ColorPickerWidth = 234;
        ColorPickerHandlerSizeLG = 32;
        ColorPickerHandlerSize = 16;
        // The 48px Ant alpha editor leaves a 30px text viewport once the border, the small input padding and the
        // 1px content inset are taken from each side; a theme with wider small padding widens the editor so that
        // "100%" keeps that viewport instead of being clipped.
        ColorPickerAlphaInputWidth = 30 + 2 * (EffectiveGlobalToken.LineWidth + EffectiveGlobalToken.InputPaddingSM.Left + 1);
        ColorPickerInputNumberHandleWidth = 16;
        ColorPickerPresetPanelWidth = 220;
        ColorPickerInsetShadow = new BoxShadows(new BoxShadow { IsInset = true, Blur = 1, Color = EffectiveGlobalToken.ColorTextQuaternary });
        ColorBlockInnerShadows = new BoxShadows(new BoxShadow { IsInset = true, Spread = EffectiveGlobalToken.LineWidth, Color = EffectiveGlobalToken.ColorFillSecondary });
        ColorSpectrumHeight = EffectiveGlobalToken.ControlHeightLG * 4;
        ColorPickerHandlerLightColor = EffectiveGlobalToken.ColorBgElevated;
        ColorPickerHandlerDarkColor = Color.Parse("#22075e");
        SliderContainerMargin = new Thickness(0, 0, EffectiveGlobalToken.UniformlyMarginSM, 0);
        TransparentBgSize = EffectiveGlobalToken.SizeXS;
        ColorPickerPresetColorGroupPadding = new Thickness(0, EffectiveGlobalToken.UniformlyPaddingXXS);
    }

    protected void CalculateDerivedValues(double handlerSize, double presetSize, double trackSize)
    {
        ColorPickerSliderSize = handlerSize;
        CalculateDependentSizes(handlerSize, presetSize, trackSize);
    }

    private protected void CalculateDependentSizes(double handlerSize, double presetSize, double trackSize)
    {
        CheckedMarkSize = presetSize * 0.6;
        ColorPickerSliderThumbSize = handlerSize + EffectiveGlobalToken.LineWidth * 2;
        ColorPickerPreviewSize = trackSize * 2 + EffectiveGlobalToken.UniformlyMarginSM;
    }
}
