using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class ExpanderToken : AbstractControlDesignToken
{
    
    public ExpanderToken()

    {
    }

    /// <summary>
    /// 折叠面板头部内边距
    /// </summary>
    public Thickness HeaderPadding { get; set; }

    /// <summary>
    /// 折叠面板头部小号内边距
    /// </summary>
    public Thickness HeaderPaddingSM { get; set; }

    /// <summary>
    /// 折叠面板头部大号内边距
    /// </summary>
    public Thickness HeaderPaddingLG { get; set; }

    /// <summary>
    /// 折叠面板头部背景
    /// </summary>
    public Color HeaderBg { get; set; }

    /// <summary>
    /// 折叠面板内容内边距
    /// </summary>
    public Thickness ContentPadding { get; set; }

    /// <summary>
    /// 折叠面板内容小号内边距
    /// </summary>
    public Thickness ContentPaddingSM { get; set; }

    /// <summary>
    /// 折叠面板内容大号内边距
    /// </summary>
    public Thickness ContentPaddingLG { get; set; }

    /// <summary>
    /// 折叠面板内容背景
    /// </summary>
    public Color ContentBg { get; set; }

    #region 内部 Token 定义

    public CornerRadius ExpanderBorderRadius { get; set; }
    public Thickness LeftExpandButtonHMargin { get; set; }
    public Thickness RightExpandButtonHMargin { get; set; }

    #endregion

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        HeaderPadding            = new Thickness(EffectiveGlobalToken.UniformlyPadding, EffectiveGlobalToken.UniformlyPaddingSM);
        HeaderBg                 = EffectiveGlobalToken.ColorFillAlter;
        ContentPadding           = new Thickness(16, EffectiveGlobalToken.UniformlyPadding);
        ContentPaddingSM         = EffectiveGlobalToken.PaddingSM;
        ContentPaddingLG         = EffectiveGlobalToken.PaddingLG;
        ContentBg                = EffectiveGlobalToken.ColorBgContainer;
        HeaderPaddingSM          = new Thickness(EffectiveGlobalToken.UniformlyPaddingSM, EffectiveGlobalToken.UniformlyPaddingXS);
        HeaderPaddingLG          = new Thickness(EffectiveGlobalToken.UniformlyPaddingLG, EffectiveGlobalToken.UniformlyPadding);
        ExpanderBorderRadius     = EffectiveGlobalToken.BorderRadiusLG;
        LeftExpandButtonHMargin  = new Thickness(0, 0, EffectiveGlobalToken.UniformlyMarginSM, 0);
        RightExpandButtonHMargin = new Thickness(EffectiveGlobalToken.UniformlyMarginSM, 0, 0, 0);
    }
    
}
