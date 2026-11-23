using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class RateToken : AbstractControlDesignToken
{
    
    /// <summary>
    /// 星星颜色
    /// Star color
    /// </summary>
    public partial Color StarColor { get; set; }
    
    /// <summary>
    /// 星星尺寸
    /// Star size
    /// </summary>
    public partial double StarSize { get; set; }
    
    /// <summary>
    /// 小星星尺寸
    /// Small star size
    /// </summary>
    public partial double StarSizeSM { get; set; }
    
    /// <summary>
    /// 大星星尺寸
    /// Large star size
    /// </summary>
    public partial double StarSizeLG { get; set; }
    
    /// <summary>
    /// 星星悬浮时的缩放
    /// Scale of star when hover
    /// </summary>
    public partial double StarHoverScale { get; set; }
    
    /// <summary>
    /// 星星背景色
    /// Star background color
    /// </summary>
    public partial Color StarBg { get; set; }
    
}
