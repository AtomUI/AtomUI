using AtomUI.Media;
using AtomUI.Theme.TokenSystem;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class ImagePreviewerToken : AbstractControlDesignToken
{
    public const string ID = "ImagePreviewer";
    
    /// <summary>
    /// 预览操作图标大小
    /// Size of preview operation icon
    /// </summary>
    public double PreviewOperationSize { get; set; }
    
    /// <summary>
    /// 预览操作图标颜色
    /// Color of preview operation icon
    /// </summary>
    public Color PreviewOperationColor { get; set; }
    
    /// <summary>
    /// 预览操作图标悬浮颜色
    /// Color of hovered preview operation icon
    /// </summary>
    public Color PreviewOperationHoverColor { get; set; }
    
    /// <summary>
    /// 预览操作图标禁用颜色
    /// Disabled color of preview operation icon
    /// </summary>
    public Color PreviewOperationColorDisabled { get; set; }
    
    /// <summary>
    /// 预览切换按钮尺寸
    /// Size of preview switch button
    /// </summary>
    public double ImagePreviewSwitchSize { get; set; }
    
    /// <summary>
    /// 封面的背景颜色
    /// </summary>
    public Color MaskBgColor { get; set; }
    
    /// <summary>
    /// 预览窗口最小宽度
    /// </summary>
    public double DialogMinWidth { get; set; }
    
    /// <summary>
    /// 预览窗口最小高度
    /// </summary>
    public double DialogMinHeight { get; set; }
    
    /// <summary>
    /// 预览窗口标题栏背景色
    /// </summary>
    public Color TitleBarBackgroundColor { get; set; }
    
    /// <summary>
    /// 浮动导航按钮背景颜色
    /// </summary>
    public Color NavButtonBgColor { get; set; }
    
    /// <summary>
    /// 浮动导航按钮 Hover 背景颜色
    /// </summary>
    public Color NavButtonBgHoverColor { get; set; }
    
    /// <summary>
    /// 浮动工具栏内间距
    /// </summary>
    public Thickness FloatToolbarPadding { get; set; }
    
    /// <summary>
    /// 浮动工具栏索引指示器内间距
    /// </summary>
    public Thickness FloatToolbarIndicatorPadding { get; set; }
    
    /// <summary>
    /// 封面默认的宽度
    /// </summary>
    public double CoverImageWidth { get; set; }
    
    public ImagePreviewerToken()
        : base(ID)
    {
    }
    
    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        PreviewOperationColor         = SharedToken.ColorTextLightSolid.SetAlphaF(0.85);
        PreviewOperationHoverColor    = SharedToken.ColorTextLightSolid;
        PreviewOperationColorDisabled = SharedToken.ColorTextDisabled;
        PreviewOperationSize          = SharedToken.FontSizeIcon * 1.5;
        ImagePreviewSwitchSize        = SharedToken.ControlHeightLG;
        MaskBgColor                   = ColorUtils.FromRgbF(0.3, 0, 0, 0);
        DialogMinWidth                = 710;
        DialogMinHeight               = 240;
        CoverImageWidth               = 200;
        
        if (isDarkMode)
        {
            TitleBarBackgroundColor       = SharedToken.ColorBorderSecondary;
        }
        else
        {
            TitleBarBackgroundColor       = SharedToken.ColorBorderSecondary.Darken(1);
        }

        NavButtonBgColor             = SharedToken.ColorBgMask.SetAlphaF(0.1);
        NavButtonBgHoverColor        = SharedToken.ColorBgMask.SetAlphaF(0.2);
        FloatToolbarPadding          = new Thickness(SharedToken.UniformlyPaddingLG / 2, 0);
        FloatToolbarIndicatorPadding = new Thickness(SharedToken.UniformlyPaddingXS, 0);
    }
}