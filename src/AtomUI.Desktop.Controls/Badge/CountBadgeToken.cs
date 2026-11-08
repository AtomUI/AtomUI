using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class CountBadgeToken : AbstractControlDesignToken
{
    public double IndicatorHeight { get; set; }

    public double IndicatorHeightSM { get; set; }

    public double TextFontSize { get; set; }

    public double TextFontSizeSM { get; set; }

    public Color TextColor { get; set; }

    public Color IndicatorColor { get; set; }

    public double ShadowSize { get; set; }

    public Color ShadowColor { get; set; }

    public Thickness TextPadding { get; set; }

    public CornerRadius CornerRadius { get; set; }

    public CornerRadius CornerRadiusSM { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);

        var lineWidth = EffectiveGlobalToken.LineWidth;

        if (!IsExplicit(CountBadgeTokenKind.IndicatorHeight))
        {
            IndicatorHeight = Math.Round(EffectiveGlobalToken.FontSize * EffectiveGlobalToken.RelativeLineHeight) -
                              2 * lineWidth;
        }

        if (!IsExplicit(CountBadgeTokenKind.IndicatorHeightSM))
        {
            IndicatorHeightSM = EffectiveGlobalToken.FontSize;
        }

        TextFontSize       = EffectiveGlobalToken.FontSizeSM;
        TextFontSizeSM     = EffectiveGlobalToken.FontSizeSM - 2;
        TextColor          = EffectiveGlobalToken.ColorTextLightSolid;
        IndicatorColor     = EffectiveGlobalToken.ColorError;
        ShadowSize         = lineWidth;
        ShadowColor        = EffectiveGlobalToken.ColorBorderBg;
        TextPadding        = new Thickness(EffectiveGlobalToken.UniformlyPaddingXXS, 0);
        CornerRadius       = new CornerRadius(IndicatorHeight);
        CornerRadiusSM     = new CornerRadius(IndicatorHeightSM);
    }
}
