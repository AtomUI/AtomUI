using AtomUI.Media;
using AtomUI.Theme.TokenSystem;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class ChromeToken : AbstractControlDesignToken
{
    public const string ID = "Chrome";
    
    /// <summary>
    /// Hover 的背景色
    /// </summary>
    public Color HoverBackgroundColor { get; set; }

    /// <summary>
    /// 鼠标按下的背景色
    /// </summary>
    public Color PressedBackgroundColor { get; set; }

    /// <summary>
    /// 关闭按钮的背景颜色
    /// </summary>
    public Color CloseHoverBackgroundColor { get; set; }

    /// <summary>
    /// 关闭按钮鼠标按下的背景颜色
    /// </summary>
    public Color ClosePressedBackgroundColor { get; set; }

    /// <summary>
    /// 按钮的前景色
    /// </summary>
    public Color ForegroundColor { get; set; }
    
    /// <summary>
    /// 应用程序 Logo 和 标题之间的间距
    /// </summary>
    public double LogoAndTitleSpacing { get; set; }
    
    /// <summary>
    /// 应用程序标题栏
    /// </summary>
    public Thickness TitleBarPadding { get; set; }
    
    /// <summary>
    /// 窗口标题默认字体
    /// </summary>
    public double TitleFontSize { get; set; }
    
    /// <summary>
    /// 窗口标题默认字体粗细
    /// </summary>
    public FontWeight TitleFontWeight { get; set; }
    
    /// <summary>
    /// 标题按钮的大小
    /// </summary>
    public double CaptionButtonSize {  get; set; }
    
    /// <summary>
    /// 应用程序图标大小
    /// </summary>
    public double LogoSize { get; set; }
    
    /// <summary>
    /// 窗口激活状态下的颜色
    /// </summary>
    public Color ActiveColor { get; set; }
    
    /// <summary>
    /// 窗口未激活状态下的颜色
    /// </summary>
    public Color InactiveColor { get; set; }
    
    /// <summary>
    /// 窗口激活状态下的颜色
    /// </summary>
    public Color ActiveBgColor { get; set; }
    
    /// <summary>
    /// 窗口激活状态下鼠标划过的颜色
    /// </summary>
    public Color ActiveHoverBgColor { get; set; }
    
    /// <summary>
    /// 窗口激活状态下鼠标按下的颜色
    /// </summary>
    public Color ActivePressedBgColor { get; set; }
    
    /// <summary>
    /// Windows 窗口关闭按钮鼠标 hover 颜色
    /// </summary>
    public Color WindowsCloseButtonHoverColor { get; set; }
    
    /// <summary>
    /// Windows 窗口关闭按钮鼠标 hover 背景色
    /// </summary>
    public Color WindowsCloseButtonHoverBgColor { get; set; }
    
    /// <summary>
    /// Windows 窗口关闭按钮鼠标按下背景色
    /// </summary>
    public Color WindowsCloseButtonPressedBgColor { get; set; }
    
    /// <summary>
    /// 窗口未激活状态下的颜色
    /// </summary>
    public Color InactiveBgColor { get; set; }
    
    /// <summary>
    /// 窗口未激活状态下鼠标划过的颜色
    /// </summary>
    public Color InactiveHoverBgColor { get; set; }
    
    /// <summary>
    /// 标题按钮的内间距
    /// </summary>
    public Thickness CaptionButtonPadding { get; set; }
    
    /// <summary>
    /// 标题按钮间距
    /// </summary>
    public double CaptionGroupSpacing { get; set; }
    
    /// <summary>
    /// 标题最小高度
    /// </summary>
    public double MinHeight { get; set; }
    
    public ChromeToken()
        : base(ID)
    {
    }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        CloseHoverBackgroundColor   = SharedToken.ColorErrorTextActive;
        ClosePressedBackgroundColor = SharedToken.ColorErrorTextHover;
        ForegroundColor             = SharedToken.ColorTextSecondary;
        HoverBackgroundColor        = SharedToken.ColorBgTextHover;
        PressedBackgroundColor      = SharedToken.ColorBgTextActive;
        LogoAndTitleSpacing         = SharedToken.SizeUnit * 2;
        TitleBarPadding             = new Thickness(LogoAndTitleSpacing * 1.8, LogoAndTitleSpacing);
        CaptionButtonSize           = SharedToken.IconSize;
        LogoSize                    = SharedToken.SizeUnit * 4;
        
        ActiveColor   = SharedToken.ColorTextSecondary;
        InactiveColor = SharedToken.ColorTextQuaternary;

        ActiveBgColor        = SharedToken.ColorFillTertiary;
        ActiveHoverBgColor   = SharedToken.ColorFillSecondary;
        ActivePressedBgColor = SharedToken.ColorFill;

        InactiveBgColor      = SharedToken.ColorFillQuaternary;
        InactiveHoverBgColor = SharedToken.ColorFillTertiary;
        
        CaptionButtonPadding = new Thickness(SharedToken.SizeUnit * 2);
        CaptionGroupSpacing  = SharedToken.SizeUnit * 2;
        
        MinHeight = 40;

        WindowsCloseButtonHoverBgColor   = Color.FromRgb(244, 67, 54); // #F44336
        WindowsCloseButtonPressedBgColor = Color.FromArgb(190, 244, 67, 54);
        WindowsCloseButtonHoverColor     = ColorUtils.OnBackground(Colors.White, WindowsCloseButtonHoverBgColor);

        TitleFontSize = 13;
        TitleFontWeight = FontWeight.Bold;
    }
}