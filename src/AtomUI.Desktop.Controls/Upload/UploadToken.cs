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
    
    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        ActionsColor        = SharedToken.ColorIcon;
        PictureCardSize     = SharedToken.ControlHeightLG * 2.55;
        TextListItemMargin  = new Thickness(0, SharedToken.UniformlyMarginXS, 0, 0);
        TextListNamePadding = new Thickness(SharedToken.UniformlyPaddingXS, 0);
        UploadThumbnailSize = SharedToken.FontSizeHeading2;
    }
}