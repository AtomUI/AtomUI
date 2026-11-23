using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn values of the shared <see cref="AbstractColorPickerViewToken"/> layer.</summary>
public static class ShadcnAbstractColorPickerViewDerivation
{
    public static void DeriveShared(in DerivationContext<ShadcnToken> context, DesignToken global, AbstractColorPickerViewToken token)
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


    public static void CalculateDerivedValues(AbstractColorPickerViewToken token, DesignToken global, double handlerSize, double presetSize, double trackSize)
    {
        token.ColorPickerSliderSize = handlerSize;
        CalculateDependentSizes(token, global, handlerSize, presetSize, trackSize);
    }


    public static void CalculateDependentSizes(AbstractColorPickerViewToken token, DesignToken global, double handlerSize, double presetSize, double trackSize)
    {
        token.CheckedMarkSize = presetSize * 0.6;
        token.ColorPickerSliderThumbSize = handlerSize + global.LineWidth * 2;
        token.ColorPickerPreviewSize = trackSize * 2 + global.UniformlyMarginSM;
    }
}
