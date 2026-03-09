using AtomUI.Theme.TokenSystem;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class TextAreaToken : AbstractControlDesignToken
{
    public const string ID = "TextArea";
    
    public TextAreaToken()
        : base(ID)
    {
    }
    
    /// <summary>
    /// 字体大小
    /// </summary>
    public double FontSize { get; set; }

    /// <summary>
    /// 大号字体大小
    /// </summary>
    public double FontSizeLG { get; set; }

    /// <summary>
    /// 小号字体大小
    /// </summary>
    public double FontSizeSM { get; set; }
    
    /// <summary>
    /// Resize 指示器颜色
    /// </summary>
    public Color ResizeIndicatorLineColor { get; set; }
    
    /// <summary>
    /// Resize 指示器大小
    /// </summary>
    public double ResizeHandleSize { get; set; }
    
    /// <summary>
    /// 输入框内边距
    /// </summary>
    public Thickness RightAddOnPadding { get; set; }

    /// <summary>
    /// 小号输入框内边距
    /// </summary>
    public Thickness RightAddOnPaddingSM { get; set; }

    /// <summary>
    /// 大号输入框内边距
    /// </summary>
    public Thickness RightAddOnPaddingLG { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        FontSize                 = SharedToken.FontSize;
        FontSizeLG               = SharedToken.FontSizeLG;
        FontSizeSM               = SharedToken.FontSizeSM;
        ResizeIndicatorLineColor = SharedToken.ColorTextDescription;
        ResizeHandleSize         = SharedToken.SizeXS;
        
        var lineWidth    = SharedToken.LineWidth;
        RightAddOnPadding   = new Thickness(0, 0, SharedToken.UniformlyPaddingSM - lineWidth, 0);
        RightAddOnPaddingSM = new Thickness(0, 0, SharedToken.ControlPaddingHorizontalSM - lineWidth, 0);
        RightAddOnPaddingLG = new Thickness(0, 0, SharedToken.ControlPaddingHorizontal - lineWidth, 0);
    }
}