using AtomUI.Theme.TokenSystem;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class CollapseToken : AbstractControlDesignToken
{
    public const string ID = "Collapse";

    public CollapseToken()
        : base(ID)
    {
    }

    /// <summary>
    /// 折叠面板头部内边距
    /// </summary>
    public Thickness HeaderPadding { get; set; }

    /// <summary>
    /// 折叠面板头部背景
    /// </summary>
    public Color HeaderBg { get; set; }

    /// <summary>
    /// 折叠面板内容内边距
    /// </summary>
    public Thickness ContentPadding { get; set; }

    /// <summary>
    /// 折叠面板内容背景
    /// </summary>
    public Color ContentBg { get; set; }

    #region 内部 Token 定义

    public Thickness CollapseHeaderPaddingSM { get; set; }
    public Thickness CollapseHeaderPaddingLG { get; set; }
    public Thickness CollapseContentPaddingSM { get; set; }
    public Thickness CollapseContentPaddingLG { get; set; }
    public CornerRadius CollapsePanelBorderRadius { get; set; }
    public Thickness LeftExpandButtonMarginSM { get; set; }
    public Thickness LeftExpandButtonMargin { get; set; }
    public Thickness LeftExpandButtonMarginLG { get; set; }
    public Thickness RightExpandButtonMarginSM { get; set; }
    public Thickness RightExpandButtonMargin { get; set; }
    public Thickness RightExpandButtonMarginLG { get; set; }

    #endregion

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        HeaderPadding             = new Thickness(SharedToken.UniformlyPadding, SharedToken.UniformlyPaddingSM);
        HeaderBg                  = SharedToken.ColorFillAlter;
        ContentPadding            = new Thickness(16, SharedToken.UniformlyPadding);
        CollapseContentPaddingSM  = SharedToken.PaddingSM;
        CollapseContentPaddingLG  = SharedToken.PaddingLG;
        ContentBg                 = SharedToken.ColorBgContainer;
        CollapseHeaderPaddingSM   = new Thickness(SharedToken.UniformlyPaddingSM, SharedToken.UniformlyPaddingXS);
        CollapseHeaderPaddingLG   = new Thickness(SharedToken.UniformlyPaddingLG, SharedToken.UniformlyPadding);
        CollapsePanelBorderRadius = SharedToken.BorderRadiusLG;
        LeftExpandButtonMarginSM  = new Thickness(0, 0, SharedToken.UniformlyMarginXXS, 0);
        LeftExpandButtonMargin    = new Thickness(0, 0, SharedToken.UniformlyMarginXS, 0);
        LeftExpandButtonMarginLG  = new Thickness(0, 0, SharedToken.UniformlyMarginSM, 0);
        RightExpandButtonMarginSM = new Thickness(SharedToken.UniformlyMarginXXS, 0, 0, 0);
        RightExpandButtonMargin   = new Thickness(SharedToken.UniformlyMarginXS, 0, 0, 0);
        RightExpandButtonMarginLG   = new Thickness(SharedToken.UniformlyMarginSM, 0, 0, 0);
    }
}