using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class MessageCardToken : AbstractControlDesignToken
{

    /// <summary>
    /// 提示框背景色
    /// </summary>
    public partial Color ContentBg { get; set; }

    /// <summary>
    /// 提示框内边距
    /// </summary>
    public partial Thickness ContentPadding { get; set; }

    /// <summary>
    /// 提醒框图标尺寸
    /// </summary>
    public partial double MessageIconSize { get; set; }

    /// <summary>
    /// 提醒框图标外边距
    /// </summary>
    public partial Thickness MessageIconMargin { get; set; }
    public partial Thickness MessageTopMargin { get; set; }

    public MessageCardToken()

    {
    }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        ContentBg = EffectiveGlobalToken.ColorBgElevated;
        ContentPadding = new Thickness(
            (EffectiveGlobalToken.ControlHeightLG -
             EffectiveGlobalToken.FontSize * EffectiveGlobalToken.RelativeLineHeight) / 2,
            EffectiveGlobalToken.UniformlyPaddingXS);
        MessageIconMargin = new Thickness(0, 0, EffectiveGlobalToken.UniformlyMarginXS, 0);
        MessageTopMargin  = new Thickness(EffectiveGlobalToken.UniformlyMarginXS);
        MessageIconSize   = EffectiveGlobalToken.FontSizeSM * EffectiveGlobalToken.RelativeLineHeightSM;
    }
    
}
