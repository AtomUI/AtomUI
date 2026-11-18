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

    /// <summary>
    /// 提醒框信息样式阴影
    /// </summary>
    public partial BoxShadows InformationShadows { get; set; }

    /// <summary>
    /// 提醒框成功样式阴影
    /// </summary>
    public partial BoxShadows SuccessShadows { get; set; }

    /// <summary>
    /// 提醒框警告样式阴影
    /// </summary>
    public partial BoxShadows WarningShadows { get; set; }

    /// <summary>
    /// 提醒框错误样式阴影
    /// </summary>
    public partial BoxShadows ErrorShadows { get; set; }

    /// <summary>
    /// 提醒框加载样式阴影
    /// </summary>
    public partial BoxShadows LoadingShadows { get; set; }

    public MessageCardToken()

    {
    }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        // Every MessageType has a severity shadow slot, so there is no resting shadow: the theme sets the card shadow
        // from the slot matching the current MessageType.
        InformationShadows  = EffectiveGlobalToken.BoxShadows;
        SuccessShadows      = EffectiveGlobalToken.BoxShadows;
        WarningShadows      = EffectiveGlobalToken.BoxShadows;
        ErrorShadows        = EffectiveGlobalToken.BoxShadows;
        LoadingShadows      = EffectiveGlobalToken.BoxShadows;
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
