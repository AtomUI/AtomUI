using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class CollapseToken : AbstractControlDesignToken, IControlTokenDefaults
{
    
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

    [ControlTokenDefault(nameof(HeaderPadding))]
    private static Thickness DefaultForHeaderPadding(in ControlTokenDefaultContext context) => new Thickness(context.Global.UniformlyPadding, context.Global.UniformlyPaddingSM);

    [ControlTokenDefault(nameof(HeaderBg))]
    private static Color DefaultForHeaderBg(in ControlTokenDefaultContext context) => context.Global.ColorFillAlter;

    [ControlTokenDefault(nameof(ContentPadding))]
    private static Thickness DefaultForContentPadding(in ControlTokenDefaultContext context) => new Thickness(16, context.Global.UniformlyPadding);

    [ControlTokenDefault(nameof(CollapseContentPaddingSM))]
    private static Thickness DefaultForCollapseContentPaddingSM(in ControlTokenDefaultContext context) => context.Global.PaddingSM;

    [ControlTokenDefault(nameof(CollapseContentPaddingLG))]
    private static Thickness DefaultForCollapseContentPaddingLG(in ControlTokenDefaultContext context) => context.Global.PaddingLG;

    [ControlTokenDefault(nameof(ContentBg))]
    private static Color DefaultForContentBg(in ControlTokenDefaultContext context) => context.Global.ColorBgContainer;

    [ControlTokenDefault(nameof(CollapseHeaderPaddingSM))]
    private static Thickness DefaultForCollapseHeaderPaddingSM(in ControlTokenDefaultContext context) => new Thickness(context.Global.UniformlyPaddingSM, context.Global.UniformlyPaddingXS);

    [ControlTokenDefault(nameof(CollapseHeaderPaddingLG))]
    private static Thickness DefaultForCollapseHeaderPaddingLG(in ControlTokenDefaultContext context) => new Thickness(context.Global.UniformlyPaddingLG, context.Global.UniformlyPadding);

    [ControlTokenDefault(nameof(CollapsePanelBorderRadius))]
    private static CornerRadius DefaultForCollapsePanelBorderRadius(in ControlTokenDefaultContext context) => context.Global.BorderRadiusLG;

    [ControlTokenDefault(nameof(LeftExpandButtonMarginSM))]
    private static Thickness DefaultForLeftExpandButtonMarginSM(in ControlTokenDefaultContext context) => new Thickness(0, 0, context.Global.UniformlyMarginXXS, 0);

    [ControlTokenDefault(nameof(LeftExpandButtonMargin))]
    private static Thickness DefaultForLeftExpandButtonMargin(in ControlTokenDefaultContext context) => new Thickness(0, 0, context.Global.UniformlyMarginXS, 0);

    [ControlTokenDefault(nameof(LeftExpandButtonMarginLG))]
    private static Thickness DefaultForLeftExpandButtonMarginLG(in ControlTokenDefaultContext context) => new Thickness(0, 0, context.Global.UniformlyMarginSM, 0);

    [ControlTokenDefault(nameof(RightExpandButtonMarginSM))]
    private static Thickness DefaultForRightExpandButtonMarginSM(in ControlTokenDefaultContext context) => new Thickness(context.Global.UniformlyMarginXXS, 0, 0, 0);

    [ControlTokenDefault(nameof(RightExpandButtonMargin))]
    private static Thickness DefaultForRightExpandButtonMargin(in ControlTokenDefaultContext context) => new Thickness(context.Global.UniformlyMarginXS, 0, 0, 0);

    [ControlTokenDefault(nameof(RightExpandButtonMarginLG))]
    private static Thickness DefaultForRightExpandButtonMarginLG(in ControlTokenDefaultContext context) => new Thickness(context.Global.UniformlyMarginSM, 0, 0, 0);
}
