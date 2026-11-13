using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class PopupConfirmToken : AbstractControlDesignToken
{
    
    public PopupConfirmToken()

    {
    }

    /// <summary>
    /// 按钮的外边距
    /// </summary>
    public partial double ButtonSpacing { get; set; }

    /// <summary>
    /// Icon 外边距
    /// </summary>
    public partial Thickness IconMargin { get; set; }

    /// <summary>
    /// 主内容区域外边距
    /// </summary>
    public partial Thickness ContentContainerMargin { get; set; }

    /// <summary>
    /// 按钮内容区域外边距
    /// </summary>
    public partial Thickness ButtonContainerMargin { get; set; }

    /// <summary>
    /// 标题栏外边距
    /// </summary>
    public partial Thickness TitleMargin { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        ButtonSpacing          = EffectiveGlobalToken.UniformlyMarginXS;
        IconMargin             = new Thickness(0, EffectiveGlobalToken.UniformlyMarginXS / 2, EffectiveGlobalToken.UniformlyMarginXS, 0);
        ContentContainerMargin = new Thickness(0, 0, 0, EffectiveGlobalToken.UniformlyMarginXS);
        TitleMargin            = new Thickness(0, 0, 0, EffectiveGlobalToken.UniformlyMarginXS);
        ButtonContainerMargin  = new Thickness(0, EffectiveGlobalToken.UniformlyMarginXS, 0, 0);
    }
    
}
