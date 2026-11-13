using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class TextBoxToken : AbstractControlDesignToken
{

    public TextBoxToken()

    {
    }

    /// <summary>
    /// 默认内边距
    /// </summary>
    public partial Thickness ContentPadding { get; set; }

    /// <summary>
    /// 小号内边距
    /// </summary>
    public partial Thickness ContentPaddingSM { get; set; }

    /// <summary>
    /// 大号内边距
    /// </summary>
    public partial Thickness ContentPaddingLG { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);

        var fontSize     = EffectiveGlobalToken.FontSize;
        var fontSizeSM   = EffectiveGlobalToken.FontSizeSM;
        var fontSizeLG   = EffectiveGlobalToken.FontSizeLG;
        var lineHeight   = EffectiveGlobalToken.RelativeLineHeight;
        var lineHeightSM = EffectiveGlobalToken.RelativeLineHeightSM;
        var lineHeightLG = EffectiveGlobalToken.RelativeLineHeightLG;
        var lineWidth    = EffectiveGlobalToken.LineWidth;

        ContentPadding = new Thickness(EffectiveGlobalToken.UniformlyPaddingSM - lineWidth,
            Math.Round((EffectiveGlobalToken.ControlHeight - fontSize * lineHeight) / 2 * 10) / 10 - lineWidth);
        ContentPaddingSM = new Thickness(EffectiveGlobalToken.ControlPaddingHorizontalSM - lineWidth,
            Math.Round((EffectiveGlobalToken.ControlHeightSM - fontSizeSM * lineHeightSM) / 2 * 10) / 10 -
            lineWidth);
        ContentPaddingLG = new Thickness(EffectiveGlobalToken.ControlPaddingHorizontal - lineWidth,
            Math.Ceiling((EffectiveGlobalToken.ControlHeightLG - fontSizeLG * lineHeightLG) / 2 * 10) / 10 -
            lineWidth);
    }

}
