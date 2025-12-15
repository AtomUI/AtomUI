using AtomUI.Theme.TokenSystem;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class NotificationToken : AbstractControlDesignToken
{
    public const string ID = "Notification";

    public NotificationToken()
        : base(ID)
    {
    }

    /// <summary>
    /// 提醒框背景色
    /// </summary>
    public Color NotificationBg { get; set; }

    /// <summary>
    /// 提醒框内边距
    /// </summary>
    public Thickness NotificationPadding { get; set; }

    /// <summary>
    /// 提醒框图标尺寸
    /// </summary>
    public double NotificationIconSize { get; set; }

    /// <summary>
    /// 提醒框图标外边距
    /// </summary>
    public Thickness NotificationIconMargin { get; set; }

    /// <summary>
    /// 提醒框关闭按钮尺寸
    /// </summary>
    public double NotificationCloseButtonSize { get; set; }
    
    /// <summary>
    /// 提醒框关闭按钮内间距
    /// </summary>
    public Thickness NotificationCloseButtonPadding { get; set; }
        
    /// <summary>
    /// 提醒框底部外边距
    /// </summary>
    public Thickness NotificationMarginBottom { get; set; }

    /// <summary>
    /// 提醒框上边缘外边距
    /// </summary>
    public Thickness NotificationTopMargin { get; set; }

    /// <summary>
    /// 提醒框下边缘外边距
    /// </summary>
    public Thickness NotificationBottomMargin { get; set; }

    /// <summary>
    /// 提醒框进度条背景色
    /// </summary>
    public IImmutableBrush? NotificationProgressBg { get; set; }

    /// <summary>
    /// 提醒框进度条高度
    /// </summary>
    public double NotificationProgressHeight { get; set; }

    /// <summary>
    /// 进度条外边距
    /// </summary>
    public Thickness NotificationProgressMargin { get; set; }

    /// <summary>
    /// 提醒框宽度
    /// </summary>
    public double NotificationWidth { get; set; }

    /// <summary>
    /// 内容外边距
    /// </summary>
    public Thickness NotificationContentMargin { get; set; }

    /// <summary>
    /// 标题栏的外边距
    /// </summary>
    public Thickness HeaderMargin { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        NotificationProgressHeight = 2;
        NotificationProgressMargin = new Thickness(0, 0, 0, 1);
        NotificationContentMargin = new Thickness(0, 0, 0, SharedToken.UniformlyPaddingMD);
        NotificationPadding = new Thickness(SharedToken.UniformlyPaddingLG, SharedToken.UniformlyPaddingMD, SharedToken.UniformlyPaddingLG, 0);
        NotificationBg = SharedToken.ColorBgElevated;
        NotificationIconSize = SharedToken.FontSizeLG * SharedToken.RelativeLineHeightLG;
        NotificationCloseButtonSize = SharedToken.ControlHeightLG * 0.55;
        NotificationMarginBottom = new Thickness(0, 0, 0, SharedToken.UniformlyMargin);
        NotificationTopMargin = new Thickness(SharedToken.UniformlyMarginLG, SharedToken.UniformlyMarginLG, SharedToken.UniformlyMarginLG, 0);
        NotificationBottomMargin =
            new Thickness(SharedToken.UniformlyMarginLG, 0, SharedToken.UniformlyMarginLG, SharedToken.UniformlyMarginLG);

        NotificationProgressBg = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0.5, RelativeUnit.Relative),
            EndPoint   = new RelativePoint(1, 0.5, RelativeUnit.Relative),
            GradientStops = new GradientStops
            {
                new() { Color = SharedToken.ColorPrimaryHover, Offset = 0 },
                new() { Color = SharedToken.ColorPrimary, Offset      = 1 }
            }
        }.ToImmutable();
        NotificationWidth              = 384;
        HeaderMargin                   = new Thickness(0, 0, 0, SharedToken.UniformlyMarginXS);
        NotificationIconMargin         = new Thickness(0, 0, SharedToken.UniformlyMarginSM, 0);
        NotificationCloseButtonPadding = SharedToken.PaddingXXS;
    }
}