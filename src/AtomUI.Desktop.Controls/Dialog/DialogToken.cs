using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class DialogToken : AbstractControlDesignToken, IControlTokenDefaults
{
    
    /// <summary>
    /// 顶部背景色
    /// Background color of header
    /// </summary>
    public Color HeaderBg { get; set; }
    
    /// <summary>
    /// 头部外边距
    /// </summary>
    public Thickness HeaderMarginBottom { get; set; }
    
    /// <summary>
    /// overlay dialog 的 logo 大小
    /// </summary>
    public double LogoSize { get; set; }
    
    /// <summary>
    /// 标题字体大小
    /// Font size of title
    /// </summary>
    public double HeaderFontSize { get; set; }
    
    /// <summary>
    /// 标题字体颜色
    /// Font color of title
    /// </summary>
    public Color HeaderColor { get; set; }
    
    /// <summary>
    /// 内容区域背景色
    /// Background color of content
    /// </summary>
    public Color ContentBg { get; set; }
    
    /// <summary>
    /// 标题区域内间距
    /// </summary>
    public Thickness HeaderPadding { get; set; }
    
    /// <summary>
    /// 内容区域内间距
    /// </summary>
    public Thickness ContentPadding { get; set; }

    /// <summary>
    /// 原生窗口宿主内容区域内间距
    /// </summary>
    public Thickness WindowContentPadding { get; set; }
    
    /// <summary>
    /// 底部区域背景色
    /// Background color of footer
    /// </summary>
    public Color FooterBg { get; set; }
    
    /// <summary>
    /// 底部区域内间距
    /// </summary>
    public Thickness FooterPadding { get; set; }
    
    /// <summary>
    /// 底部区域外间距
    /// </summary>
    public Thickness FooterMarginTop { get; set; }
    
    /// <summary>
    /// 关闭按钮大小
    /// </summary>
    public double CloseBtnSize { get; set; }
    
    /// <summary>
    /// 默认最小高度
    /// </summary>
    public double MinHeight { get; set; }
    
    /// <summary>
    /// 默认最小宽度
    /// </summary>
    public double MinWidth { get; set; }
    
    /// <summary>
    /// 底部按钮间隔大小
    /// </summary>
    public double ButtonGroupSpacing { get; set; }
    
    /// <summary>
    /// 加载器的外间距
    /// </summary>
    public Thickness LoadingIndicatorMargin { get; set; }
    
    [ControlTokenDefault(nameof(HeaderBg))]
    private static Color DefaultForHeaderBg(in ControlTokenDefaultContext context) =>
        Colors.Transparent;

    [ControlTokenDefault(nameof(HeaderMarginBottom))]
    private static Thickness DefaultForHeaderMarginBottom(in ControlTokenDefaultContext context) =>
        new Thickness(0, 0, 0, context.Global.UniformlyMarginXS);

    [ControlTokenDefault(nameof(LogoSize))]
    private static double DefaultForLogoSize(in ControlTokenDefaultContext context) =>
        context.Global.SizeLG;

    [ControlTokenDefault(nameof(HeaderFontSize))]
    private static double DefaultForHeaderFontSize(in ControlTokenDefaultContext context) =>
        context.Global.FontSizeHeading5;

    [ControlTokenDefault(nameof(HeaderColor))]
    private static Color DefaultForHeaderColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorTextHeading;

    [ControlTokenDefault(nameof(ContentBg))]
    private static Color DefaultForContentBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorBgElevated;

    [ControlTokenDefault(nameof(HeaderPadding))]
    private static Thickness DefaultForHeaderPadding(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.PaddingContentHorizontalLG, context.Global.UniformlyPaddingSM, context.Global.PaddingContentHorizontalSM, 0);

    [ControlTokenDefault(nameof(ContentPadding))]
    private static Thickness DefaultForContentPadding(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.PaddingContentHorizontalLG, 0, context.Global.PaddingContentHorizontalLG, 0);

    [ControlTokenDefault(nameof(WindowContentPadding))]
    private static Thickness DefaultForWindowContentPadding(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.PaddingContentHorizontalLG, context.Global.PaddingContentVerticalLG, context.Global.PaddingContentHorizontalLG, context.Global.PaddingContentVerticalLG);

    [ControlTokenDefault(nameof(FooterBg))]
    private static Color DefaultForFooterBg(in ControlTokenDefaultContext context) =>
        Colors.Transparent;

    [ControlTokenDefault(nameof(FooterPadding))]
    private static Thickness DefaultForFooterPadding(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.PaddingContentHorizontalLG, 0, context.Global.PaddingContentHorizontalLG, context.Global.UniformlyPaddingMD);

    [ControlTokenDefault(nameof(FooterMarginTop))]
    private static Thickness DefaultForFooterMarginTop(in ControlTokenDefaultContext context) =>
        new Thickness(0, context.Global.UniformlyMarginXS, 0, 0);

    [ControlTokenDefault(nameof(CloseBtnSize))]
    private static double DefaultForCloseBtnSize(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeight;

    [ControlTokenDefault(nameof(MinWidth))]
    private static double DefaultForMinWidth(in ControlTokenDefaultContext context) =>
        200d;

    [ControlTokenDefault(nameof(ButtonGroupSpacing))]
    private static double DefaultForButtonGroupSpacing(in ControlTokenDefaultContext context) =>
        context.Global.SpacingXS;

    [ControlTokenDefault(nameof(LoadingIndicatorMargin))]
    private static Thickness DefaultForLoadingIndicatorMargin(in ControlTokenDefaultContext context) =>
        new Thickness(0, 0, 0, context.Global.UniformlyMargin);

    [ControlTokenDefault(nameof(MinHeight), nameof(HeaderPadding))]
    private static double DefaultForMinHeight(in ControlTokenDefaultContext context, Thickness headerPadding) =>
        context.Global.ControlHeightLG + headerPadding.Top + headerPadding.Bottom + context.Global.UniformlyMarginXS;
}
