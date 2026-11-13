using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class CountBadgeToken : AbstractControlDesignToken
{
    public partial double IndicatorHeight { get; set; }

    public partial double IndicatorHeightSM { get; set; }

    public partial double TextFontSize { get; set; }

    public partial double TextFontSizeSM { get; set; }

    public partial Color TextColor { get; set; }

    public partial Color IndicatorColor { get; set; }

    public partial double ShadowSize { get; set; }

    public partial Color ShadowColor { get; set; }

    public partial Thickness TextPadding { get; set; }

    public partial CornerRadius CornerRadius { get; set; }

    public partial CornerRadius CornerRadiusSM { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
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
