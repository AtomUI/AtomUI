using AtomUI.Theme.TokenSystem;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class DialogToken : AbstractControlDesignToken
{
    public const string ID = "Dialog";
    
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
    
    public DialogToken()
        : base(ID)
    {
    }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        HeaderBg       = Colors.Transparent;
        HeaderFontSize = SharedToken.FontSizeHeading5;
        ContentBg      = SharedToken.ColorBgElevated;
        HeaderColor    = SharedToken.ColorTextHeading;
        ContentPadding = new Thickness(SharedToken.PaddingContentHorizontalLG, 0,
            SharedToken.PaddingContentHorizontalLG, SharedToken.UniformlyPaddingMD);
        HeaderPadding = new Thickness(SharedToken.PaddingContentHorizontalLG, SharedToken.UniformlyPaddingSM,
            SharedToken.PaddingContentHorizontalSM, 0);
        HeaderMarginBottom = new Thickness(0, 0, 0, SharedToken.UniformlyMarginXS);
        LogoSize           = SharedToken.SizeLG;
        MinHeight = SharedToken.ControlHeightLG + HeaderPadding.Top + HeaderPadding.Bottom +
                    SharedToken.UniformlyMarginXS;
        MinWidth     = 200;
        CloseBtnSize = SharedToken.ControlHeight;
        FooterPadding = new Thickness(SharedToken.PaddingContentHorizontalLG, 0,
            SharedToken.PaddingContentHorizontalLG, SharedToken.UniformlyPaddingMD);
        FooterMarginTop    = new Thickness(0, SharedToken.UniformlyMarginXS, 0, 0);
        FooterBg           = Colors.Transparent;
        ButtonGroupSpacing = SharedToken.SpacingXS;
    }
}