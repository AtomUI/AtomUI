using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Media;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class WindowTitleBarToken : AbstractTitleBarActionToken
{
    /// <summary>
    /// 应用程序 Logo 和 标题之间的间距
    /// </summary>
    public partial double LogoAndTitleSpacing { get; set; }

    /// <summary>
    /// Windows/Linux 应用程序 Logo 和左侧附加内容之间的间距
    /// </summary>
    public partial double LogoAndLeftAddOnSpacing { get; set; }
    
    /// <summary>
    /// 应用程序标题栏
    /// </summary>
    public partial Thickness TitleBarPadding { get; set; }
    
    /// <summary>
    /// 窗口标题默认字体
    /// </summary>
    public partial double TitleFontSize { get; set; }
    
    /// <summary>
    /// 窗口标题默认字体粗细
    /// </summary>
    public partial FontWeight TitleFontWeight { get; set; }
    

    /// <summary>
    /// Windows caption glyph size aligned with Avalonia's native-style window decorations.
    /// </summary>
    public partial double WindowsCaptionIconSize { get; set; }
    
    /// <summary>
    /// 应用程序图标大小
    /// </summary>
    public partial double LogoSize { get; set; }
    
    
    
    
    
    
    /// <summary>
    /// Windows 窗口关闭按钮鼠标 hover 颜色
    /// </summary>
    public partial Color WindowsCloseButtonHoverColor { get; set; }
    
    /// <summary>
    /// Windows 窗口关闭按钮鼠标 hover 背景色
    /// </summary>
    public partial Color WindowsCloseButtonHoverBgColor { get; set; }
    
    /// <summary>
    /// Windows 窗口关闭按钮鼠标按下背景色
    /// </summary>
    public partial Color WindowsCloseButtonPressedBgColor { get; set; }
    
    
    


    
    /// <summary>
    /// 标题按钮间距
    /// </summary>
    public partial double CaptionGroupSpacing { get; set; }
    
    /// <summary>
    /// 标题高度
    /// </summary>
    public partial double Height { get; set; }

    /// <summary>
    /// 标题栏水平布局分隔（Linux 模板 DockPanel 用，取代直引 EffectiveGlobalToken.SpacingXS）
    /// </summary>
    public partial double HeaderHorizontalSpacing { get; set; }
    
    public WindowTitleBarToken()

    {
    }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);

        if (!IsExplicit(WindowTitleBarTokenKind.LogoAndTitleSpacing))
        {
            LogoAndTitleSpacing         = EffectiveGlobalToken.SizeUnit * 2;
        }

        LogoAndLeftAddOnSpacing     = EffectiveGlobalToken.SpacingXXS;
        TitleBarPadding             = new Thickness(LogoAndTitleSpacing * 1.8, 0);

        if (!IsExplicit(WindowTitleBarTokenKind.CaptionButtonIconSize))
        {
            CaptionButtonIconSize       = EffectiveGlobalToken.IconSize;
        }

        WindowsCaptionIconSize      = 11;
        LogoSize                    = EffectiveGlobalToken.SizeUnit * 4;

        
        if (!IsExplicit(WindowTitleBarTokenKind.CaptionButtonPadding))
        {
            CaptionButtonPadding = new Thickness(EffectiveGlobalToken.SizeUnit * 2);
        }

        CaptionGroupSpacing  = EffectiveGlobalToken.SizeUnit * 2;

        // managed caption button 按自身实测尺寸取 max(w, h) / 2 得到正圆圆角；
        // 内容尺寸固定为 icon + padding，这里用同一公式把该几何暴露给主题，
        // 使 macOS AddOn 按钮与相邻 caption 按钮共用同一圆角来源。


        // 窗口装饰语义不走密度算法：紧凑模式下 ControlHeightLG / SizeLG / SpacingXS 都会被拉小，
        // 标题栏 / DockPanel 间距不应跟着缩，因此使用跨平台稳定值。
        Height                  = 40;
        HeaderHorizontalSpacing = 8;

        if (!IsExplicit(WindowTitleBarTokenKind.WindowsCloseButtonHoverBgColor))
        {
            WindowsCloseButtonHoverBgColor   = Color.FromRgb(244, 67, 54); // #F44336
        }

        WindowsCloseButtonPressedBgColor = Color.FromArgb(190, 244, 67, 54);
        WindowsCloseButtonHoverColor     = ColorUtils.OnBackground(Colors.White, WindowsCloseButtonHoverBgColor);

        TitleFontSize = 13;
        TitleFontWeight = FontWeight.Bold;
    }

}
