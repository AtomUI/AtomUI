using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class ExpanderToken : AbstractControlDesignToken, IControlTokenDefaults
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

    /// <summary>
    /// 兼容保留：当前模板旋转 Header 并使用水平图标外边距，未消费此垂直资源。
    /// </summary>
    public Thickness LeftExpandButtonVMargin { get; set; }

    /// <summary>
    /// 兼容保留：当前模板旋转 Header 并使用水平图标外边距，未消费此垂直资源。
    /// </summary>
    public Thickness RightExpandButtonVMargin { get; set; }

    #endregion

    [ControlTokenDefault(nameof(HeaderPadding))]
    private static Thickness DefaultForHeaderPadding(in ControlTokenDefaultContext context) =>
        new(context.Global.UniformlyPadding, context.Global.UniformlyPaddingSM);

    [ControlTokenDefault(nameof(HeaderPaddingSM))]
    private static Thickness DefaultForHeaderPaddingSM(in ControlTokenDefaultContext context) =>
        new(context.Global.UniformlyPaddingSM, context.Global.UniformlyPaddingXS);

    [ControlTokenDefault(nameof(HeaderPaddingLG))]
    private static Thickness DefaultForHeaderPaddingLG(in ControlTokenDefaultContext context) =>
        new(context.Global.UniformlyPaddingLG, context.Global.UniformlyPadding);

    [ControlTokenDefault(nameof(HeaderBg))]
    private static Color DefaultForHeaderBg(in ControlTokenDefaultContext context) => context.Global.ColorFillAlter;

    [ControlTokenDefault(nameof(ContentPadding))]
    private static Thickness DefaultForContentPadding(in ControlTokenDefaultContext context) =>
        new(16, context.Global.UniformlyPadding);

    [ControlTokenDefault(nameof(ContentPaddingSM))]
    private static Thickness DefaultForContentPaddingSM(in ControlTokenDefaultContext context) => context.Global.PaddingSM;

    [ControlTokenDefault(nameof(ContentPaddingLG))]
    private static Thickness DefaultForContentPaddingLG(in ControlTokenDefaultContext context) => context.Global.PaddingLG;

    [ControlTokenDefault(nameof(ContentBg))]
    private static Color DefaultForContentBg(in ControlTokenDefaultContext context) => context.Global.ColorBgContainer;

    [ControlTokenDefault(nameof(ExpanderBorderRadius))]
    private static CornerRadius DefaultForExpanderBorderRadius(in ControlTokenDefaultContext context) =>
        context.Global.BorderRadiusLG;

    [ControlTokenDefault(nameof(LeftExpandButtonHMargin))]
    private static Thickness DefaultForLeftExpandButtonHMargin(in ControlTokenDefaultContext context) =>
        new(0, 0, context.Global.UniformlyMarginSM, 0);

    [ControlTokenDefault(nameof(RightExpandButtonHMargin))]
    private static Thickness DefaultForRightExpandButtonHMargin(in ControlTokenDefaultContext context) =>
        new(context.Global.UniformlyMarginSM, 0, 0, 0);

    [ControlTokenDefault(nameof(LeftExpandButtonVMargin))]
    private static Thickness DefaultForLeftExpandButtonVMargin(in ControlTokenDefaultContext context) =>
        new(0, 0, 0, context.Global.UniformlyMarginSM);

    [ControlTokenDefault(nameof(RightExpandButtonVMargin))]
    private static Thickness DefaultForRightExpandButtonVMargin(in ControlTokenDefaultContext context) =>
        new(0, context.Global.UniformlyMarginSM, 0, 0);
    
}
