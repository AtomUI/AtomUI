using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class CollapseToken : AbstractControlDesignToken
{
    
    public CollapseToken()

    {
    }

    /// <summary>
    /// 折叠面板头部内边距
    /// </summary>
    public partial Thickness HeaderPadding { get; set; }

    /// <summary>
    /// 折叠面板头部背景
    /// </summary>
    public partial Color HeaderBg { get; set; }

    /// <summary>
    /// 折叠面板内容内边距
    /// </summary>
    public partial Thickness ContentPadding { get; set; }

    /// <summary>
    /// 折叠面板内容背景
    /// </summary>
    public partial Color ContentBg { get; set; }

    #region 内部 Token 定义

    public partial Thickness CollapseHeaderPaddingSM { get; set; }
    public partial Thickness CollapseHeaderPaddingLG { get; set; }
    public partial Thickness CollapseContentPaddingSM { get; set; }
    public partial Thickness CollapseContentPaddingLG { get; set; }
    public partial CornerRadius CollapsePanelBorderRadius { get; set; }
    public partial Thickness LeftExpandButtonMarginSM { get; set; }
    public partial Thickness LeftExpandButtonMargin { get; set; }
    public partial Thickness LeftExpandButtonMarginLG { get; set; }
    public partial Thickness RightExpandButtonMarginSM { get; set; }
    public partial Thickness RightExpandButtonMargin { get; set; }
    public partial Thickness RightExpandButtonMarginLG { get; set; }

    #endregion

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        HeaderPadding             = new Thickness(EffectiveGlobalToken.UniformlyPadding, EffectiveGlobalToken.UniformlyPaddingSM);
        HeaderBg                  = EffectiveGlobalToken.ColorFillAlter;
        ContentPadding            = new Thickness(16, EffectiveGlobalToken.UniformlyPadding);
        CollapseContentPaddingSM  = EffectiveGlobalToken.PaddingSM;
        CollapseContentPaddingLG  = EffectiveGlobalToken.PaddingLG;
        ContentBg                 = EffectiveGlobalToken.ColorBgContainer;
        CollapseHeaderPaddingSM   = new Thickness(EffectiveGlobalToken.UniformlyPaddingSM, EffectiveGlobalToken.UniformlyPaddingXS);
        CollapseHeaderPaddingLG   = new Thickness(EffectiveGlobalToken.UniformlyPaddingLG, EffectiveGlobalToken.UniformlyPadding);
        CollapsePanelBorderRadius = EffectiveGlobalToken.BorderRadiusLG;
        LeftExpandButtonMarginSM  = new Thickness(0, 0, EffectiveGlobalToken.UniformlyMarginXXS, 0);
        LeftExpandButtonMargin    = new Thickness(0, 0, EffectiveGlobalToken.UniformlyMarginXS, 0);
        LeftExpandButtonMarginLG  = new Thickness(0, 0, EffectiveGlobalToken.UniformlyMarginSM, 0);
        RightExpandButtonMarginSM = new Thickness(EffectiveGlobalToken.UniformlyMarginXXS, 0, 0, 0);
        RightExpandButtonMargin   = new Thickness(EffectiveGlobalToken.UniformlyMarginXS, 0, 0, 0);
        RightExpandButtonMarginLG   = new Thickness(EffectiveGlobalToken.UniformlyMarginSM, 0, 0, 0);
    }
    
}
