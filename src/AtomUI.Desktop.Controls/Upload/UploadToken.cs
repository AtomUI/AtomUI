using AtomUI.Theme.TokenSystem;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class UploadToken : AbstractControlDesignToken
{
    public const string ID = "Upload";

    public UploadToken()
        : this(ID)
    {
    }

    protected UploadToken(string id)
        : base(id)
    {
    }

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
        ActionsColor             = SharedToken.ColorIcon;
        PictureCardSize          = SharedToken.ControlHeightLG * 2.55;
        TextListItemMargin       = new Thickness(0, SharedToken.UniformlyMarginXS, 0, 0);
        TextListNamePadding      = new Thickness(SharedToken.UniformlyPaddingXS, 0);
        UploadThumbnailSize      = SharedToken.FontSizeHeading2;
        DragIconSize             = SharedToken.FontSizeHeading3 * 2;
        DragIconMargin           = new Thickness(0, 0, 0, SharedToken.UniformlyMargin);
        DragHeaderMargin         = new Thickness(0, 0, 0, SharedToken.UniformlyMarginXXS);
        PictureListItemMargin    = new Thickness(0, SharedToken.UniformlyMarginXS, 0, 0);
        PictureListPreviewerSize = SharedToken.SizeXXL;
    }
}