using AtomUI.Media;
using AtomUI.Theme.TokenSystem;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class DrawerToken : AbstractControlDesignToken
{
    public const string ID = "Drawer";

    public DrawerToken()
        : base(ID)
    {
    }

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

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);

        BoxShadowDrawerLeft = new BoxShadows(new BoxShadow()
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

        BoxShadowDrawerRight = new BoxShadows(new BoxShadow()
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
        
        BoxShadowDrawerUp = new BoxShadows(new BoxShadow()
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
        
        BoxShadowDrawerDown = new BoxShadows(new BoxShadow()
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

        SmallSize         = new Dimension(378);
        MiddleSize        = new Dimension(520);
        LargeSize         = new Dimension(736);
        HeaderMargin      = new Thickness(SharedToken.UniformlyMarginLG, SharedToken.UniformlyMargin);
        FooterPadding     = new Thickness(SharedToken.UniformlyPadding, SharedToken.UniformlyPaddingXS);
        CloseIconPadding  = SharedToken.PaddingXXS;
        CloseIconMargin   = new Thickness(0, 0, SharedToken.UniformlyMarginXS, 0);
        ContentPadding    = SharedToken.PaddingLG;
        PushOffsetPercent = 0.4;
    }
}