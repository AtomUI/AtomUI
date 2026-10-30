using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class DialogToken : AbstractControlDesignToken
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
    
    public DialogToken()

    {
    }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        HeaderBg       = Colors.Transparent;
        HeaderFontSize = EffectiveGlobalToken.FontSizeHeading5;
        ContentBg      = EffectiveGlobalToken.ColorBgElevated;
        HeaderColor    = EffectiveGlobalToken.ColorTextHeading;
        ContentPadding = new Thickness(EffectiveGlobalToken.PaddingContentHorizontalLG, 0,
            EffectiveGlobalToken.PaddingContentHorizontalLG, 0);
        WindowContentPadding = new Thickness(
            EffectiveGlobalToken.PaddingContentHorizontalLG,
            EffectiveGlobalToken.PaddingContentVerticalLG,
            EffectiveGlobalToken.PaddingContentHorizontalLG,
            EffectiveGlobalToken.PaddingContentVerticalLG);
        HeaderPadding = new Thickness(EffectiveGlobalToken.PaddingContentHorizontalLG, EffectiveGlobalToken.UniformlyPaddingSM,
            EffectiveGlobalToken.PaddingContentHorizontalSM, 0);
        HeaderMarginBottom = new Thickness(0, 0, 0, EffectiveGlobalToken.UniformlyMarginXS);
        LogoSize           = EffectiveGlobalToken.SizeLG;
        MinHeight = EffectiveGlobalToken.ControlHeightLG + HeaderPadding.Top + HeaderPadding.Bottom +
                    EffectiveGlobalToken.UniformlyMarginXS;
        MinWidth     = 200;
        CloseBtnSize = EffectiveGlobalToken.ControlHeight;
        FooterPadding = new Thickness(EffectiveGlobalToken.PaddingContentHorizontalLG, 0,
            EffectiveGlobalToken.PaddingContentHorizontalLG, EffectiveGlobalToken.UniformlyPaddingMD);
        FooterMarginTop    = new Thickness(0, EffectiveGlobalToken.UniformlyMarginXS, 0, 0);
        FooterBg           = Colors.Transparent;
        ButtonGroupSpacing = EffectiveGlobalToken.SpacingXS;
        LoadingIndicatorMargin = new Thickness(0, 0, 0, EffectiveGlobalToken.UniformlyMargin);
    }
    
}
