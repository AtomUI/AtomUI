using AtomUI.Theme.DesignTokens;
using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal abstract class AbstractColorPickerViewToken : AbstractControlDesignToken
{
    public double ColorPickerWidth { get; set; }

    public double ColorSpectrumHeight { get; set; }

    public BoxShadows ColorPickerInsetShadow { get; set; }

    public double ColorPickerHandlerSizeLG { get; set; }

    public double ColorPickerHandlerSize { get; set; }

    public double ColorPickerHandlerSizeSM { get; set; }

    public Color ColorPickerHandlerLightColor { get; set; }

    public Color ColorPickerHandlerDarkColor { get; set; }

    public double ColorPickerSliderSize { get; set; }

    public double ColorPickerSliderTrackSize { get; set; }

    public double ColorPickerSliderThumbSize { get; set; }

    public double ColorPickerPreviewSize { get; set; }

    public double ColorPickerAlphaInputWidth { get; set; }

    public double ColorPickerInputNumberHandleWidth { get; set; }

    public double ColorPickerPresetColorSize { get; set; }

    public double ColorPickerPresetPanelWidth { get; set; }

    public double CheckedMarkSize { get; set; }

    public Thickness ColorPickerPresetColorGroupPadding { get; set; }

    public Thickness SliderContainerMargin { get; set; }

    public BoxShadows ColorBlockInnerShadows { get; set; }

    public double TransparentBgSize { get; set; }

    public double EditorFontSize { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        EditorFontSize = 11;
        ColorPickerWidth = 234;
        ColorPickerHandlerSizeLG = 32;
        ColorPickerHandlerSize = 16;
        // Keep the existing 48px alpha editor; the old unused token default was 44px.
        ColorPickerAlphaInputWidth = 48;
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
        CheckedMarkSize = presetSize * 0.6;
        ColorPickerSliderSize = handlerSize;
        ColorPickerSliderThumbSize = handlerSize + EffectiveGlobalToken.LineWidth * 2;
        ColorPickerPreviewSize = trackSize * 2 + EffectiveGlobalToken.UniformlyMarginSM;
    }
}
