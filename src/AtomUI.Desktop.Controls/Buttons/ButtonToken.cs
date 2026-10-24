using AtomUI.Media;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class ButtonToken : AbstractControlDesignToken, IControlTokenDefaults
{
    
    /// <summary>
    /// 文字字重
    /// </summary>
    public double FontWeight { get; set; }

    /// <summary>
    /// 默认按钮阴影
    /// </summary>
    public BoxShadows DefaultShadow { get; set; }

    /// <summary>
    /// 主要按钮阴影
    /// </summary>
    public BoxShadows PrimaryShadow { get; set; }

    /// <summary>
    /// 危险按钮阴影
    /// </summary>
    public BoxShadows DangerShadow { get; set; }

    /// <summary>
    /// 主要按钮文本颜色
    /// </summary>
    public Color PrimaryColor { get; set; }

    /// <summary>
    /// 默认按钮文本颜色
    /// </summary>
    public Color DefaultColor { get; set; }

    /// <summary>
    /// 默认按钮背景色
    /// </summary>
    public Color DefaultBg { get; set; }

    /// <summary>
    /// 默认按钮边框颜色
    /// </summary>
    public Color DefaultBorderColor { get; set; }

    /// <summary>
    /// 默认的禁用边框颜色
    /// </summary>
    public Color DefaultBorderColorDisabled { get; set; }

    /// <summary>
    /// 危险按钮文本颜色
    /// </summary>
    public Color DangerColor { get; set; }

    /// <summary>
    /// 默认按钮悬浮态背景色
    /// </summary>
    public Color DefaultHoverBg { get; set; }

    /// <summary>
    /// 默认按钮悬浮态文本颜色
    /// </summary>
    public Color DefaultHoverColor { get; set; }

    /// <summary>
    /// 默认按钮悬浮态边框颜色
    /// </summary>
    public Color DefaultHoverBorderColor { get; set; }

    /// <summary>
    /// 默认按钮激活态背景色
    /// </summary>
    public Color DefaultActiveBg { get; set; }

    /// <summary>
    /// 默认按钮激活态文字颜色
    /// </summary>
    public Color DefaultActiveColor { get; set; }

    /// <summary>
    /// 默认按钮激活态边框颜色
    /// </summary>
    public Color DefaultActiveBorderColor { get; set; }

    /// <summary>
    /// 禁用状态边框颜色
    /// </summary>
    public Color BorderColorDisabled { get; set; }

    /// <summary>
    /// 默认幽灵按钮文本颜色
    /// </summary>
    public Color DefaultGhostColor { get; set; }

    /// <summary>
    /// 幽灵按钮背景色
    /// </summary>
    public Color GhostBg { get; set; }

    /// <summary>
    /// 默认幽灵按钮边框颜色
    /// </summary>
    public Color DefaultGhostBorderColor { get; set; }
    
    // 主要填充按钮的浅色背景颜色
    // Background color of primary filled button
    
    /// <summary>
    /// 默认实心按钮的文本色
    /// Default text color for solid buttons.
    /// </summary>
    public Color SolidTextColor { get; set; }
    
    /// <summary>
    /// 默认文本按钮的文本色
    /// Default text color for text buttons on hover
    /// </summary>
    public Color TextTextColor { get; set; }
    
    /// <summary>
    /// 默认文本按钮悬浮态文本颜色
    /// Default text color for text buttons on hover
    /// </summary>
    public Color TextTextHoverColor { get; set; }
    
    /// <summary>
    /// 默认文本按钮激活态文字颜色
    /// Default text color for text buttons on active
    /// </summary>
    public Color TextTextActiveColor { get; set; }

    /// <summary>
    /// 按钮内间距
    /// </summary>
    public Thickness ContentPadding { get; set; }

    /// <summary>
    /// 大号按钮内间距
    /// </summary>
    public Thickness ContentPaddingLG { get; set; }

    /// <summary>
    /// 小号按钮内间距
    /// </summary>
    public Thickness ContentPaddingSM { get; set; }

    /// <summary>
    /// 历史保留：额外内容间隔；当前 Button 无公开额外内容区域，不参与渲染。
    /// </summary>
    public double ExtraContentItemSpacing { get; set; }
    
    /// <summary>
    /// 历史保留：小号额外内容边距；当前 Button 无公开额外内容区域，不参与渲染。
    /// </summary>
    public Thickness ExtraContentMarginSM { get; set; }

    /// <summary>
    /// 历史保留：额外内容边距；当前 Button 无公开额外内容区域，不参与渲染。
    /// </summary>
    public Thickness ExtraContentMargin { get; set; }

    /// <summary>
    /// 历史保留：大号额外内容边距；当前 Button 无公开额外内容区域，不参与渲染。
    /// </summary>
    public Thickness ExtraContentMarginLG { get; set; }

    /// <summary>
    /// 圆形按钮内间距
    /// </summary>
    public Thickness CirclePadding { get; set; }

    /// <summary>
    /// 只有图标的按钮图标尺寸
    /// </summary>
    public double OnlyIconSize { get; set; }

    /// <summary>
    /// 大号只有图标的按钮图标尺寸
    /// </summary>
    public double OnlyIconSizeLG { get; set; }

    /// <summary>
    /// 小号只有图标的按钮图标尺寸
    /// </summary>
    public double OnlyIconSizeSM { get; set; }

    /// <summary>
    /// 完成 Icon 外边距
    /// </summary>
    public Thickness IconMargin { get; set; }

    /// <summary>
    /// 位于内容右侧的 Icon 外边距
    /// </summary>
    public Thickness IconEndMargin { get; set; }

    /// <summary>
    /// 历史保留：按钮组接缝颜色；当前 Button 无对应组区域，不参与渲染。
    /// </summary>
    public Color GroupBorderColor { get; set; }

    /// <summary>
    /// 链接按钮悬浮态背景色
    /// </summary>
    public Color LinkHoverBg { get; set; }

    /// <summary>
    /// 文本按钮悬浮态背景色
    /// </summary>
    public Color TextHoverBg { get; set; }

    /// <summary>
    /// 按钮内容字体大小
    /// </summary>
    public double ContentFontSize { get; set; }

    /// <summary>
    /// 大号按钮内容字体大小
    /// </summary>
    public double ContentFontSizeLG { get; set; }

    /// <summary>
    /// 小号按钮内容字体大小
    /// </summary>
    public double ContentFontSizeSM { get; set; }

    /// <summary>
    /// 按钮内容字体行高
    /// </summary>
    public double ContentLineHeight { get; set; }

    /// <summary>
    /// 大号按钮内容字体行高
    /// </summary>
    public double ContentLineHeightLG { get; set; }

    /// <summary>
    /// 小号按钮内容字体行高
    /// </summary>
    public double ContentLineHeightSM { get; set; }
    
    /// <summary>
    /// 按钮的下拉弹出菜单跟按钮之间的间距大小
    /// </summary>
    public double GutterToFlyout { get; set; }

    #region 内部 Token 定义

    /// <summary>
    /// IconOnly 按钮内间距
    /// </summary>
    public Thickness IconOnyPadding { get; set; }

    /// <summary>
    /// IconOnly 大号按钮内间距
    /// </summary>
    public Thickness IconOnyPaddingLG { get; set; }

    /// <summary>
    /// IconOnly 小号按钮内间距
    /// </summary>
    public Thickness IconOnyPaddingSM { get; set; }

    #endregion

    [ControlTokenDefault(nameof(FontWeight))]
    private static double DefaultForFontWeight(in ControlTokenDefaultContext context) =>
        400;

    [ControlTokenDefault(nameof(DefaultShadow))]
    private static BoxShadows DefaultForDefaultShadow(in ControlTokenDefaultContext context) =>
        new BoxShadows(new BoxShadow
        {
            OffsetX = 0,
            OffsetY = context.Global.ControlOutlineWidth,
            Blur = 3,
            Spread = 0,
            Color = context.Global.ColorControlOutline
        });

    [ControlTokenDefault(nameof(PrimaryShadow))]
    private static BoxShadows DefaultForPrimaryShadow(in ControlTokenDefaultContext context) =>
        new BoxShadows(new BoxShadow
        {
            OffsetX = 0,
            OffsetY = context.Global.ControlOutlineWidth,
            Blur = 3,
            Spread = 0,
            Color = context.Global.ColorControlOutline
        });

    [ControlTokenDefault(nameof(DangerShadow))]
    private static BoxShadows DefaultForDangerShadow(in ControlTokenDefaultContext context) =>
        new BoxShadows(new BoxShadow
        {
            OffsetX = 0,
            OffsetY = context.Global.ControlOutlineWidth,
            Blur = 3,
            Spread = 0,
            Color = context.Global.ColorErrorOutline
        });

    [ControlTokenDefault(nameof(PrimaryColor))]
    private static Color DefaultForPrimaryColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorTextLightSolid;

    [ControlTokenDefault(nameof(DefaultColor))]
    private static Color DefaultForDefaultColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorText;

    [ControlTokenDefault(nameof(DefaultBg))]
    private static Color DefaultForDefaultBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorBgContainer;

    [ControlTokenDefault(nameof(DefaultBorderColor))]
    private static Color DefaultForDefaultBorderColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorBorder;

    [ControlTokenDefault(nameof(DefaultBorderColorDisabled))]
    private static Color DefaultForDefaultBorderColorDisabled(in ControlTokenDefaultContext context) =>
        context.Global.ColorBorder;

    [ControlTokenDefault(nameof(DangerColor))]
    private static Color DefaultForDangerColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorTextLightSolid;

    [ControlTokenDefault(nameof(DefaultHoverBg))]
    private static Color DefaultForDefaultHoverBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorBgContainer;

    [ControlTokenDefault(nameof(DefaultHoverColor))]
    private static Color DefaultForDefaultHoverColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorPrimaryHover;

    [ControlTokenDefault(nameof(DefaultHoverBorderColor))]
    private static Color DefaultForDefaultHoverBorderColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorPrimaryHover;

    [ControlTokenDefault(nameof(DefaultActiveBg))]
    private static Color DefaultForDefaultActiveBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorBgContainer;

    [ControlTokenDefault(nameof(DefaultActiveColor))]
    private static Color DefaultForDefaultActiveColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorPrimaryActive;

    [ControlTokenDefault(nameof(DefaultActiveBorderColor))]
    private static Color DefaultForDefaultActiveBorderColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorPrimaryActive;

    [ControlTokenDefault(nameof(BorderColorDisabled))]
    private static Color DefaultForBorderColorDisabled(in ControlTokenDefaultContext context) =>
        context.Global.ColorBorder;

    [ControlTokenDefault(nameof(DefaultGhostColor))]
    private static Color DefaultForDefaultGhostColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorBgContainer;

    [ControlTokenDefault(nameof(GhostBg))]
    private static Color DefaultForGhostBg(in ControlTokenDefaultContext context) =>
        Colors.Transparent;

    [ControlTokenDefault(nameof(DefaultGhostBorderColor))]
    private static Color DefaultForDefaultGhostBorderColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorBgContainer;

    [ControlTokenDefault(nameof(SolidTextColor))]
    private static Color DefaultForSolidTextColor(in ControlTokenDefaultContext context) =>
        ColorUtils.IsBright(context.Global.ColorBgSolid, Colors.White) ? Colors.Black : Colors.White;

    [ControlTokenDefault(nameof(TextTextColor))]
    private static Color DefaultForTextTextColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorText;

    [ControlTokenDefault(nameof(TextTextHoverColor))]
    private static Color DefaultForTextTextHoverColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorText;

    [ControlTokenDefault(nameof(TextTextActiveColor))]
    private static Color DefaultForTextTextActiveColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorText;

    [ControlTokenDefault(nameof(ContentPadding), nameof(ContentLineHeight))]
    private static Thickness DefaultForContentPadding(in ControlTokenDefaultContext context, double contentLineHeight) =>
        new Thickness(context.Global.PaddingContentHorizontal - context.Global.LineWidth, Math.Max((context.Global.ControlHeight - contentLineHeight) / 2 - context.Global.LineWidth, 0));

    [ControlTokenDefault(nameof(ContentPaddingLG), nameof(ContentLineHeightLG))]
    private static Thickness DefaultForContentPaddingLG(in ControlTokenDefaultContext context, double contentLineHeight) =>
        new Thickness(context.Global.PaddingContentHorizontal - context.Global.LineWidth, Math.Max((context.Global.ControlHeightLG - contentLineHeight) / 2 - context.Global.LineWidth, 0));

    [ControlTokenDefault(nameof(ContentPaddingSM), nameof(ContentLineHeightSM))]
    private static Thickness DefaultForContentPaddingSM(in ControlTokenDefaultContext context, double contentLineHeight) =>
        new Thickness(8 - context.Global.LineWidth, Math.Max((context.Global.ControlHeightSM - contentLineHeight) / 2 - context.Global.LineWidth, 0));

    [ControlTokenDefault(nameof(ExtraContentItemSpacing))]
    private static double DefaultForExtraContentItemSpacing(in ControlTokenDefaultContext context) =>
        context.Global.UniformlyMarginXXS / 2;

    [ControlTokenDefault(nameof(ExtraContentMarginSM), nameof(ContentPaddingSM))]
    private static Thickness DefaultForExtraContentMarginSM(in ControlTokenDefaultContext context, Thickness contentPadding) =>
        new Thickness(contentPadding.Left / 2, 0, 0, 0);

    [ControlTokenDefault(nameof(ExtraContentMargin), nameof(ContentPadding))]
    private static Thickness DefaultForExtraContentMargin(in ControlTokenDefaultContext context, Thickness contentPadding) =>
        new Thickness(contentPadding.Left / 2, 0, 0, 0);

    [ControlTokenDefault(nameof(ExtraContentMarginLG), nameof(ContentPaddingLG))]
    private static Thickness DefaultForExtraContentMarginLG(in ControlTokenDefaultContext context, Thickness contentPadding) =>
        new Thickness(contentPadding.Left / 2, 0, 0, 0);

    [ControlTokenDefault(nameof(CirclePadding), nameof(ContentPaddingSM))]
    private static Thickness DefaultForCirclePadding(in ControlTokenDefaultContext context, Thickness contentPadding) =>
        new Thickness(contentPadding.Left / 2);

    [ControlTokenDefault(nameof(OnlyIconSize))]
    private static double DefaultForOnlyIconSize(in ControlTokenDefaultContext context) =>
        context.Global.IconSizeLG;

    [ControlTokenDefault(nameof(OnlyIconSizeLG))]
    private static double DefaultForOnlyIconSizeLG(in ControlTokenDefaultContext context) =>
        context.Global.IconSizeLG;

    [ControlTokenDefault(nameof(OnlyIconSizeSM))]
    private static double DefaultForOnlyIconSizeSM(in ControlTokenDefaultContext context) =>
        context.Global.IconSize;

    [ControlTokenDefault(nameof(IconMargin))]
    private static Thickness DefaultForIconMargin(in ControlTokenDefaultContext context) =>
        new Thickness(0, 0, context.Global.UniformlyPaddingXXS, 0);

    [ControlTokenDefault(nameof(IconEndMargin))]
    private static Thickness DefaultForIconEndMargin(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.UniformlyPaddingXXS, 0, 0, 0);

    [ControlTokenDefault(nameof(GroupBorderColor))]
    private static Color DefaultForGroupBorderColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorPrimaryHover;

    [ControlTokenDefault(nameof(LinkHoverBg))]
    private static Color DefaultForLinkHoverBg(in ControlTokenDefaultContext context) =>
        Colors.Transparent;

    [ControlTokenDefault(nameof(TextHoverBg))]
    private static Color DefaultForTextHoverBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorFillTertiary;

    [ControlTokenDefault(nameof(ContentFontSize))]
    private static double DefaultForContentFontSize(in ControlTokenDefaultContext context) =>
        context.Global.FontSize;

    [ControlTokenDefault(nameof(ContentFontSizeLG))]
    private static double DefaultForContentFontSizeLG(in ControlTokenDefaultContext context) =>
        context.Global.FontSizeLG;

    [ControlTokenDefault(nameof(ContentFontSizeSM))]
    private static double DefaultForContentFontSizeSM(in ControlTokenDefaultContext context) =>
        context.Global.FontSize;

    [ControlTokenDefault(nameof(ContentLineHeight), nameof(ContentFontSize))]
    private static double DefaultForContentLineHeight(in ControlTokenDefaultContext context, double contentFontSize) =>
        CalculatorUtils.CalculateLineHeight(contentFontSize) * contentFontSize;

    [ControlTokenDefault(nameof(ContentLineHeightLG), nameof(ContentFontSizeLG))]
    private static double DefaultForContentLineHeightLG(in ControlTokenDefaultContext context, double contentFontSize) =>
        CalculatorUtils.CalculateLineHeight(contentFontSize) * contentFontSize;

    [ControlTokenDefault(nameof(ContentLineHeightSM), nameof(ContentFontSizeSM))]
    private static double DefaultForContentLineHeightSM(in ControlTokenDefaultContext context, double contentFontSize) =>
        CalculatorUtils.CalculateLineHeight(contentFontSize) * contentFontSize;

    [ControlTokenDefault(nameof(GutterToFlyout))]
    private static double DefaultForGutterToFlyout(in ControlTokenDefaultContext context) =>
        context.Global.UniformlyMarginXXS;

    [ControlTokenDefault(nameof(IconOnyPadding), nameof(ContentLineHeight))]
    private static Thickness DefaultForIconOnyPadding(in ControlTokenDefaultContext context, double contentLineHeight) =>
        new Thickness(Math.Max((context.Global.ControlHeight - contentLineHeight) / 2 - context.Global.LineWidth, 0));

    [ControlTokenDefault(nameof(IconOnyPaddingLG), nameof(ContentLineHeightLG))]
    private static Thickness DefaultForIconOnyPaddingLG(in ControlTokenDefaultContext context, double contentLineHeight) =>
        new Thickness(Math.Max((context.Global.ControlHeightLG - contentLineHeight) / 2 - context.Global.LineWidth, 0));

    [ControlTokenDefault(nameof(IconOnyPaddingSM), nameof(ContentLineHeightSM))]
    private static Thickness DefaultForIconOnyPaddingSM(in ControlTokenDefaultContext context, double contentLineHeight) =>
        new Thickness(Math.Max((context.Global.ControlHeightSM - contentLineHeight) / 2 - context.Global.LineWidth, 0));
}
