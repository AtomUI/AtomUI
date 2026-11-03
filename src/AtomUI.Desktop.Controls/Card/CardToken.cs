using AtomUI.Media;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class CardToken : AbstractControlDesignToken, IControlTokenDefaults
{
    
    /// <summary>
    /// 卡片头部背景色
    /// Background color of card header
    /// </summary>
    public Color HeaderBg { get; set; }
    
    /// <summary>
    /// 卡片头部文字大小，大号尺寸
    /// Font size of card header
    /// </summary>
    public double HeaderFontSizeLG { get; set; }
    
    /// <summary>
    /// 卡片头部文字大小，默认尺寸
    /// Font size of card header
    /// </summary>
    public double HeaderFontSize { get; set; }
    
    /// <summary>
    /// 小号卡片头部文字大小
    /// Font size of small card header
    /// </summary>
    public double HeaderFontSizeSM { get; set; }
    
    /// <summary>
    /// 卡片头部高度，大号尺寸
    /// Height of card header
    /// </summary>
    public double HeaderHeightLG { get; set; }
    
    /// <summary>
    /// 卡片头部高度，默认尺寸
    /// Height of card header
    /// </summary>
    public double HeaderHeight { get; set; }
    
    /// <summary>
    /// 小号卡片头部高度
    /// Height of small card header
    /// </summary>
    public double HeaderHeightSM { get; set; }
    
    /// <summary>
    /// 卡片内边距，大号
    /// Padding of card body
    /// </summary>
    public Thickness BodyPaddingLG { get; set; }
    
    /// <summary>
    /// 卡片内边距
    /// Padding of card body
    /// </summary>
    public Thickness BodyPadding { get; set; }
    
    /// <summary>
    /// 小号卡片内边距
    /// Padding of small card body
    /// </summary>
    public Thickness BodyPaddingSM { get; set; }
    
    /// <summary>
    /// 卡片头部内边距,大号
    /// Padding of card head
    /// </summary>
    public Thickness HeaderPaddingLG { get; set; }
    
    /// <summary>
    /// 卡片头部内边距
    /// Padding of card head
    /// </summary>
    public Thickness HeaderPadding { get; set; }
    
    /// <summary>
    /// 小号卡片头部内边距
    /// Padding of small card head
    /// </summary>
    public Thickness HeaderPaddingSM { get; set; }
    
    /// <summary>
    /// 操作区背景色
    /// Background color of card actions
    /// </summary>
    public Color ActionsBg { get; set; }
    
    /// <summary>
    /// 操作区每一项的间距
    ///  Margin of each item in card actions
    /// 历史保留资源，无独立生产消费者；不作为视觉调节入口。
    /// </summary>
    public double ActionsSpacing { get; set; }
    
    /// <summary>
    /// 内置标签页组件下间距
    /// Margin bottom of tabs component
    /// 历史保留资源，无独立生产消费者；不作为视觉调节入口。
    /// </summary>
    public Thickness TabsMarginBottom { get; set; }
    
    /// <summary>
    /// 额外区文字颜色
    /// Text color of extra area
    /// </summary>
    public Color ExtraColor { get; set; }
    
    /// <summary>
    /// 卡片阴影
    /// Shadow of card
    /// </summary>
    public BoxShadows CardShadows { get; set; }
    
    /// <summary>
    /// 卡片头部内边距
    /// Padding of card header
    /// 历史保留资源，无独立生产消费者；不作为视觉调节入口。
    /// </summary>
    public Thickness CardHeadPadding { get; set; }
    
    /// <summary>
    /// 卡片基础内边距
    /// Padding of base card
    /// 历史保留资源，无独立生产消费者；不作为视觉调节入口。
    /// </summary>
    public Thickness CardPaddingBase { get; set; }
    
    /// <summary>
    /// 卡片操作区图标大小
    /// Size of card actions icon
    /// </summary>
    public double CardActionsIconSize { get; set; }
    
    /// <summary>
    /// 表格内容的默认阴影
    /// </summary>
    public BoxShadows CardGridItemShadows { get; set; }
    
    [ControlTokenDefault(nameof(HeaderBg))]
    private static Color DefaultForHeaderBg(in ControlTokenDefaultContext context) =>
        Colors.Transparent;

    [ControlTokenDefault(nameof(HeaderFontSizeLG))]
    private static double DefaultForHeaderFontSizeLG(in ControlTokenDefaultContext context) =>
        context.Global.FontSizeLG;

    [ControlTokenDefault(nameof(HeaderFontSize))]
    private static double DefaultForHeaderFontSize(in ControlTokenDefaultContext context) =>
        context.Global.FontSize;

    [ControlTokenDefault(nameof(HeaderFontSizeSM))]
    private static double DefaultForHeaderFontSizeSM(in ControlTokenDefaultContext context) =>
        context.Global.FontSize;

    [ControlTokenDefault(nameof(HeaderHeightLG))]
    private static double DefaultForHeaderHeightLG(in ControlTokenDefaultContext context) =>
        context.Global.FontHeightLG + context.Global.UniformlyPadding * 2;

    [ControlTokenDefault(nameof(HeaderHeight))]
    private static double DefaultForHeaderHeight(in ControlTokenDefaultContext context) =>
        context.Global.FontHeight + context.Global.UniformlyPaddingXS * 2;

    [ControlTokenDefault(nameof(HeaderHeightSM))]
    private static double DefaultForHeaderHeightSM(in ControlTokenDefaultContext context) =>
        context.Global.FontHeightSM;

    [ControlTokenDefault(nameof(BodyPaddingLG))]
    private static Thickness DefaultForBodyPaddingLG(in ControlTokenDefaultContext context) =>
        context.Global.PaddingLG;

    [ControlTokenDefault(nameof(BodyPadding))]
    private static Thickness DefaultForBodyPadding(in ControlTokenDefaultContext context) =>
        context.Global.PaddingSM;

    [ControlTokenDefault(nameof(BodyPaddingSM))]
    private static Thickness DefaultForBodyPaddingSM(in ControlTokenDefaultContext context) =>
        context.Global.PaddingXS;

    [ControlTokenDefault(nameof(HeaderPaddingLG))]
    private static Thickness DefaultForHeaderPaddingLG(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.UniformlyPaddingLG, 0);

    [ControlTokenDefault(nameof(HeaderPadding))]
    private static Thickness DefaultForHeaderPadding(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.UniformlyPaddingSM, 0);

    [ControlTokenDefault(nameof(HeaderPaddingSM))]
    private static Thickness DefaultForHeaderPaddingSM(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.UniformlyPaddingXS, 0);

    [ControlTokenDefault(nameof(ActionsBg))]
    private static Color DefaultForActionsBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorBgContainer;

    [ControlTokenDefault(nameof(ActionsSpacing))]
    private static double DefaultForActionsSpacing(in ControlTokenDefaultContext context) =>
        context.Global.SpacingSM;

    [ControlTokenDefault(nameof(TabsMarginBottom))]
    private static Thickness DefaultForTabsMarginBottom(in ControlTokenDefaultContext context) =>
        new Thickness(0, 0, 0, -context.Global.UniformlyPadding - context.Global.LineWidth);

    [ControlTokenDefault(nameof(ExtraColor))]
    private static Color DefaultForExtraColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorText;

    [ControlTokenDefault(nameof(CardShadows))]
    private static BoxShadows DefaultForCardShadows(in ControlTokenDefaultContext context) =>
        new BoxShadows(new BoxShadow()
        {
            OffsetX = 0,
            OffsetY = 1,
            Blur    = 2,
            Spread  = -1,
            Color   = ColorUtils.FromRgbF(0.16, 0, 0, 0)
        }, [
            new BoxShadow
            {
                OffsetX = 0,
                OffsetY = 3,
                Blur    = 6,
                Spread  = 0,
                Color   = ColorUtils.FromRgbF(0.12, 0, 0, 0)
            },
            new BoxShadow
            {
                OffsetX = 0,
                OffsetY = 5,
                Blur    = 12,
                Spread  = 4,
                Color   = ColorUtils.FromRgbF(0.09, 0, 0, 0)
            }
        ]);

    [ControlTokenDefault(nameof(CardHeadPadding))]
    private static Thickness DefaultForCardHeadPadding(in ControlTokenDefaultContext context) =>
        context.Global.Padding;

    [ControlTokenDefault(nameof(CardPaddingBase))]
    private static Thickness DefaultForCardPaddingBase(in ControlTokenDefaultContext context) =>
        context.Global.PaddingLG;

    [ControlTokenDefault(nameof(CardActionsIconSize))]
    private static double DefaultForCardActionsIconSize(in ControlTokenDefaultContext context) =>
        context.Global.FontSize;

    [ControlTokenDefault(nameof(CardGridItemShadows))]
    private static BoxShadows DefaultForCardGridItemShadows(in ControlTokenDefaultContext context)
    {
        var lineWidth = context.Global.LineWidth;
        return new BoxShadows(new BoxShadow()
        {
            OffsetX = lineWidth,
            OffsetY = 0,
            Blur    = 0,
            Spread  = 0,
            Color   = context.Global.ColorBorderSecondary
        }, [
            new BoxShadow
            {
                OffsetX = 0,
                OffsetY = lineWidth,
                Blur    = 0,
                Spread  = 0,
                Color   = context.Global.ColorBorderSecondary
            },
            new BoxShadow
            {
                OffsetX = lineWidth,
                OffsetY = lineWidth,
                Blur    = 0,
                Spread  = 0,
                Color   = context.Global.ColorBorderSecondary
            },
            new BoxShadow
            {
                IsInset = true,
                OffsetX = lineWidth,
                OffsetY = 0,
                Blur    = 0,
                Spread  = 0,
                Color   = context.Global.ColorBorderSecondary
            },
            new BoxShadow
            {
                IsInset = true,
                OffsetX = 0,
                OffsetY = lineWidth,
                Blur    = 0,
                Spread  = 0,
                Color   = context.Global.ColorBorderSecondary
            },
        ]);
    }

}
