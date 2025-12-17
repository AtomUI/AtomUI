using AtomUI.Theme.TokenSystem;
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
    
    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        ActionsColor    = SharedToken.ColorIcon;
        PictureCardSize = SharedToken.ControlHeightLG * 2.55;
    }
}