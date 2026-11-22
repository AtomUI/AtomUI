using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class AlertToken : AbstractControlDesignToken
{

    public AlertToken()

    {
    }

    /// <summary>
    /// 默认内间距
    /// </summary>
    public partial Thickness DefaultPadding { get; set; }

    /// <summary>
    /// 带有描述的内间距
    /// </summary>
    public partial Thickness WithDescriptionPadding { get; set; }

    /// <summary>
    /// 图标默认外间距
    /// </summary>
    public partial Thickness IconDefaultMargin { get; set; }

    /// <summary>
    /// 图标带描述信息外间距
    /// </summary>
    public partial Thickness IconWithDescriptionMargin { get; set; }

    /// <summary>
    /// 没有描述时的图标尺寸
    /// </summary>
    public partial double DefaultIconSize { get; set; }

    /// <summary>
    /// 带有描述时的图标尺寸
    /// </summary>
    public partial double WithDescriptionIconSize { get; set; }

    /// <summary>
    /// 关闭按钮图标的大小（antd Alert close-icon 的 fontSizeIcon）
    /// </summary>
    public partial double CloseIconSize { get; set; }

    /// <summary>
    /// 额外元素的外间距
    /// </summary>
    public partial Thickness ExtraElementMargin { get; set; }
    
    /// <summary>
    /// 描述标签外间距
    /// </summary>
    public partial Thickness DescriptionLabelMargin { get; set; }

    /// <summary>
    /// 成功提示文字颜色
    /// Text colour of a success alert
    /// </summary>
    public partial Color SuccessMessageColor { get; set; }

    /// <summary>
    /// 信息提示文字颜色
    /// Text colour of an info alert
    /// </summary>
    public partial Color InfoMessageColor { get; set; }

    /// <summary>
    /// 警告提示文字颜色
    /// Text colour of a warning alert
    /// </summary>
    public partial Color WarningMessageColor { get; set; }

    /// <summary>
    /// 错误提示文字颜色
    /// Text colour of an error alert
    /// </summary>
    public partial Color ErrorMessageColor { get; set; }

    /// <summary>
    /// 提示文字字重
    /// Font weight of the alert text (message and description)
    /// </summary>
    public partial FontWeight TextFontWeight { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        const double paddingHorizontal = 12; // Fixed value here.
        WithDescriptionIconSize = EffectiveGlobalToken.FontSizeHeading3;
        DefaultPadding          = new Thickness(paddingHorizontal, EffectiveGlobalToken.PaddingContentVerticalSM);
        WithDescriptionPadding  = new Thickness(EffectiveGlobalToken.PaddingContentHorizontalLG, EffectiveGlobalToken.UniformlyPaddingMD);

        IconDefaultMargin            = new Thickness(0, 0, EffectiveGlobalToken.UniformlyMarginXS, 0);
        IconWithDescriptionMargin    = new Thickness(0, 0, EffectiveGlobalToken.UniformlyMarginSM, 0);
        ExtraElementMargin           = new Thickness(EffectiveGlobalToken.UniformlyMarginXS, 0, 0, 0);
        DescriptionLabelMargin       = new Thickness(0, EffectiveGlobalToken.UniformlyMarginXS, 0, 0);

        CloseIconSize = EffectiveGlobalToken.FontSizeIcon;
        DefaultIconSize = EffectiveGlobalToken.FontSizeLG;
        SuccessMessageColor = EffectiveGlobalToken.ColorText;
        InfoMessageColor    = EffectiveGlobalToken.ColorText;
        WarningMessageColor = EffectiveGlobalToken.ColorText;
        ErrorMessageColor   = EffectiveGlobalToken.ColorText;
        TextFontWeight      = FontWeight.Normal;
    }
    
}
