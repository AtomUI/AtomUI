using AtomUI.Media;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class DrawerToken : AbstractControlDesignToken, IControlTokenDefaults
{
    
/// <summary>
    /// 左边缘阴影
    /// </summary>
    public BoxShadows BoxShadowDrawerLeft { get; set; }

    /// <summary>
    /// 右边缘阴影
    /// </summary>
    public BoxShadows BoxShadowDrawerRight { get; set; }

    /// <summary>
    /// 上边缘阴影
    /// </summary>
    public BoxShadows BoxShadowDrawerUp { get; set; }

    /// <summary>
    /// 下边缘阴影
    /// </summary>
    public BoxShadows BoxShadowDrawerDown { get; set; }
    
    /// <summary>
    /// 信息显示头内间距
    /// </summary>
    public Thickness HeaderMargin { get; set; }
    
    /// <summary>
    /// Footer 的内间距
    /// </summary>
    public Thickness FooterPadding { get; set; }
    
    /// <summary>
    /// 中等尺寸大小的 Drawer
    /// </summary>
    public Dimension MiddleSize { get; set; }
    
    /// <summary>
    /// 大尺寸的 Drawer
    /// </summary>
    public Dimension LargeSize { get; set; }
    
    /// <summary>
    /// 小尺寸的 Drawer
    /// </summary>
    public Dimension SmallSize { get; set; }
    
    /// <summary>
    /// 关闭按钮内间距
    /// </summary>
    public Thickness CloseIconPadding { get; set; }
    
    /// <summary>
    /// 关闭按钮外间距
    /// </summary>
    public Thickness CloseIconMargin { get; set; }
    
    /// <summary>
    /// 内容内间距
    /// </summary>
    public Thickness ContentPadding { get; set; }
    
    /// <summary>
    /// 子 Drawer push 的 Offset 比率
    /// </summary>
    public double PushOffsetPercent { get; set; }

    [ControlTokenDefault(nameof(BoxShadowDrawerLeft))]
    private static BoxShadows DefaultForBoxShadowDrawerLeft(in ControlTokenDefaultContext context) => new BoxShadows(new BoxShadow()
        {
            OffsetX = 6,
            OffsetY = 0,
            Blur    = 16,
            Spread  = 0,
            Color   = ColorUtils.FromRgbF(0.08, 0, 0, 0)
        }, [
            new BoxShadow()
            {
                OffsetX = 3,
                OffsetY = 0,
                Blur    = 6,
                Spread  = -4,
                Color   = ColorUtils.FromRgbF(0.12, 0, 0, 0)
            },
            new BoxShadow()
            {
                OffsetX = 9,
                OffsetY = 0,
                Blur    = 28,
                Spread  = 8,
                Color   = ColorUtils.FromRgbF(0.05, 0, 0, 0)
            }
        ]);

    [ControlTokenDefault(nameof(BoxShadowDrawerRight))]
    private static BoxShadows DefaultForBoxShadowDrawerRight(in ControlTokenDefaultContext context) => new BoxShadows(new BoxShadow()
        {
            OffsetX = -6,
            OffsetY = 0,
            Blur    = 16,
            Spread  = 0,
            Color   = ColorUtils.FromRgbF(0.08, 0, 0, 0)
        }, [
            new BoxShadow()
            {
                OffsetX = -3,
                OffsetY = 0,
                Blur    = 6,
                Spread  = -4,
                Color   = ColorUtils.FromRgbF(0.12, 0, 0, 0)
            },
            new BoxShadow()
            {
                OffsetX = -9,
                OffsetY = 0,
                Blur    = 28,
                Spread  = 8,
                Color   = ColorUtils.FromRgbF(0.05, 0, 0, 0)
            }
        ]);

    [ControlTokenDefault(nameof(BoxShadowDrawerUp))]
    private static BoxShadows DefaultForBoxShadowDrawerUp(in ControlTokenDefaultContext context) => new BoxShadows(new BoxShadow()
        {
            OffsetX = 0,
            OffsetY = 6,
            Blur    = 16,
            Spread  = 0,
            Color   = ColorUtils.FromRgbF(0.08, 0, 0, 0)
        }, [
            new BoxShadow()
            {
                OffsetX = 0,
                OffsetY = 3,
                Blur    = 6,
                Spread  = -4,
                Color   = ColorUtils.FromRgbF(0.12, 0, 0, 0)
            },
            new BoxShadow()
            {
                OffsetX = 0,
                OffsetY = 9,
                Blur    = 28,
                Spread  = 8,
                Color   = ColorUtils.FromRgbF(0.05, 0, 0, 0)
            }
        ]);

    [ControlTokenDefault(nameof(BoxShadowDrawerDown))]
    private static BoxShadows DefaultForBoxShadowDrawerDown(in ControlTokenDefaultContext context) => new BoxShadows(new BoxShadow()
        {
            OffsetX = 0,
            OffsetY = -6,
            Blur    = 16,
            Spread  = 0,
            Color   = ColorUtils.FromRgbF(0.08, 0, 0, 0)
        }, [
            new BoxShadow()
            {
                OffsetX = 0,
                OffsetY = -3,
                Blur    = 6,
                Spread  = -4,
                Color   = ColorUtils.FromRgbF(0.12, 0, 0, 0)
            },
            new BoxShadow()
            {
                OffsetX = 0,
                OffsetY = -9,
                Blur    = 28,
                Spread  = 8,
                Color   = ColorUtils.FromRgbF(0.05, 0, 0, 0)
            }
        ]);

    [ControlTokenDefault(nameof(SmallSize))]
    private static Dimension DefaultForSmallSize(in ControlTokenDefaultContext context) => new Dimension(378);

    [ControlTokenDefault(nameof(MiddleSize))]
    private static Dimension DefaultForMiddleSize(in ControlTokenDefaultContext context) => new Dimension(520);

    [ControlTokenDefault(nameof(LargeSize))]
    private static Dimension DefaultForLargeSize(in ControlTokenDefaultContext context) => new Dimension(736);

    [ControlTokenDefault(nameof(HeaderMargin))]
    private static Thickness DefaultForHeaderMargin(in ControlTokenDefaultContext context) => new Thickness(context.Global.UniformlyMarginLG, context.Global.UniformlyMargin);

    [ControlTokenDefault(nameof(FooterPadding))]
    private static Thickness DefaultForFooterPadding(in ControlTokenDefaultContext context) => new Thickness(context.Global.UniformlyPadding, context.Global.UniformlyPaddingXS);

    [ControlTokenDefault(nameof(CloseIconPadding))]
    private static Thickness DefaultForCloseIconPadding(in ControlTokenDefaultContext context) => context.Global.PaddingXXS;

    [ControlTokenDefault(nameof(CloseIconMargin))]
    private static Thickness DefaultForCloseIconMargin(in ControlTokenDefaultContext context) => new Thickness(0, 0, context.Global.UniformlyMarginXS, 0);

    [ControlTokenDefault(nameof(ContentPadding))]
    private static Thickness DefaultForContentPadding(in ControlTokenDefaultContext context) => context.Global.PaddingLG;

    [ControlTokenDefault(nameof(PushOffsetPercent))]
    private static double DefaultForPushOffsetPercent(in ControlTokenDefaultContext context) => 0.4;
}
