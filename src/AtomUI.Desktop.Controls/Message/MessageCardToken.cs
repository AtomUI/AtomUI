using AtomUI.Theme.DesignTokens;
using AtomUI.Desktop.Controls.DesignTokens;
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

    /// <summary>
    /// Background of an information message.
    /// </summary>
    public partial Color InformationBg { get; set; }

    /// <summary>
    /// Icon colour of an information message.
    /// </summary>
    public partial Color InformationIconColor { get; set; }

    /// <summary>
    /// Text colour of an information message.
    /// </summary>
    public partial Color InformationColor { get; set; }

    /// <summary>
    /// Background of an success message.
    /// </summary>
    public partial Color SuccessBg { get; set; }

    /// <summary>
    /// Icon colour of an success message.
    /// </summary>
    public partial Color SuccessIconColor { get; set; }

    /// <summary>
    /// Text colour of an success message.
    /// </summary>
    public partial Color SuccessColor { get; set; }

    /// <summary>
    /// Background of an warning message.
    /// </summary>
    public partial Color WarningBg { get; set; }

    /// <summary>
    /// Icon colour of an warning message.
    /// </summary>
    public partial Color WarningIconColor { get; set; }

    /// <summary>
    /// Text colour of an warning message.
    /// </summary>
    public partial Color WarningColor { get; set; }

    /// <summary>
    /// Background of an error message.
    /// </summary>
    public partial Color ErrorBg { get; set; }

    /// <summary>
    /// Icon colour of an error message.
    /// </summary>
    public partial Color ErrorIconColor { get; set; }

    /// <summary>
    /// Text colour of an error message.
    /// </summary>
    public partial Color ErrorColor { get; set; }

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
        // The severity backgrounds below default to this card background, so an explicit value carries over.
        if (!IsExplicit(MessageCardTokenKind.ContentBg))
        {
            ContentBg = EffectiveGlobalToken.ColorBgElevated;
        }
        // The severity surfaces default to the plain card: only the icon carries the severity.
        InformationBg    = ContentBg;
        InformationIconColor = EffectiveGlobalToken.ColorPrimary;
        InformationColor = EffectiveGlobalToken.ColorText;
        SuccessBg    = ContentBg;
        SuccessIconColor = EffectiveGlobalToken.ColorSuccess;
        SuccessColor = EffectiveGlobalToken.ColorText;
        WarningBg    = ContentBg;
        WarningIconColor = EffectiveGlobalToken.ColorWarning;
        WarningColor = EffectiveGlobalToken.ColorText;
        ErrorBg    = ContentBg;
        ErrorIconColor = EffectiveGlobalToken.ColorError;
        ErrorColor = EffectiveGlobalToken.ColorText;
        ContentPadding = new Thickness(
            (EffectiveGlobalToken.ControlHeightLG -
             EffectiveGlobalToken.FontSize * EffectiveGlobalToken.RelativeLineHeight) / 2,
            EffectiveGlobalToken.UniformlyPaddingXS);
        MessageIconMargin = new Thickness(0, 0, EffectiveGlobalToken.UniformlyMarginXS, 0);
        MessageTopMargin  = new Thickness(EffectiveGlobalToken.UniformlyMarginXS);
        MessageIconSize   = EffectiveGlobalToken.FontSizeSM * EffectiveGlobalToken.RelativeLineHeightSM;
    }
    
}
