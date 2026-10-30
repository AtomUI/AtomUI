using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class UploadToken : AbstractControlDesignToken
{
    /// <summary>
    /// 操作按扭颜色
    /// Action button color
    /// </summary>
    public Color ActionsColor { get; set; }
    
    /// <summary>
    /// 卡片类型文件列表项的尺寸（对 picture-card 和 picture-circle 生效）
    /// 
    /// </summary>
    public double PictureCardSize { get; set; }
    
    /// <summary>
    /// 文本列表的外间距
    /// </summary>
    public Thickness TextListItemMargin { get; set; }
    
    /// <summary>
    /// 文本列表文件名的内间距
    /// </summary>
    public Thickness TextListNamePadding { get; set; }

    /// <summary>
    /// 文本列表项的内间距（垂直方向）
    /// </summary>
    public Thickness TextListItemPadding { get; set; }

    /// <summary>
    /// 文本列表上传进度线的高度
    /// </summary>
    public double TextListProgressLineHeight { get; set; }

    /// <summary>
    /// 文本列表上传进度的内间距
    /// </summary>
    public Thickness TextListProgressPadding { get; set; }
    
    /// <summary>
    /// 文件类型图标大小
    /// </summary>
    public double UploadThumbnailSize { get; set; }
    
    /// <summary>
    /// 拖动上传区域图标大小
    /// </summary>
    public double DragIconSize { get; set; }
    
    /// <summary>
    /// 拖动上传区域图标外间距
    /// </summary>
    public Thickness DragIconMargin { get; set; }
    
    /// <summary>
    /// 拖动上传区域主标题的外间距
    /// </summary>
    public Thickness DragHeaderMargin { get; set; }
    
    /// <summary>
    /// 图片列表的外间距
    /// </summary>
    public Thickness PictureListItemMargin { get; set; }
    
    /// <summary>
    /// 图片列表图片预览大小
    /// </summary>
    public double PictureListPreviewerSize { get; set; }
    
    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        ActionsColor             = EffectiveGlobalToken.ColorIcon;
        PictureCardSize          = EffectiveGlobalToken.ControlHeightLG * 2.55;
        TextListItemMargin       = new Thickness(0, EffectiveGlobalToken.UniformlyMarginXS, 0, 0);
        TextListNamePadding      = new Thickness(EffectiveGlobalToken.UniformlyPaddingXS, 0);
        TextListItemPadding      = new Thickness(0, EffectiveGlobalToken.UniformlyPaddingXXS, 0, EffectiveGlobalToken.UniformlyPaddingXXS);
        TextListProgressLineHeight = 2;
        TextListProgressPadding  = new Thickness(EffectiveGlobalToken.FontSize + EffectiveGlobalToken.UniformlyPaddingXS, 0, 0, 0);
        UploadThumbnailSize      = EffectiveGlobalToken.FontSizeHeading2;
        DragIconSize             = EffectiveGlobalToken.FontSizeHeading3 * 2;
        DragIconMargin           = new Thickness(0, 0, 0, EffectiveGlobalToken.UniformlyMargin);
        DragHeaderMargin         = new Thickness(0, 0, 0, EffectiveGlobalToken.UniformlyMarginXXS);
        PictureListItemMargin    = new Thickness(0, EffectiveGlobalToken.UniformlyMarginXS, 0, 0);
        PictureListPreviewerSize = EffectiveGlobalToken.SizeXXL;
    }
    
}
