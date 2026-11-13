using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Controls;

[ControlDesignToken]
public sealed partial class IconToken : AbstractControlDesignToken
{
    
    /// <summary>
    /// 次要的描边颜色
    /// </summary>
    public partial Color SecondaryStrokeColor { get; set; }
    
    /// <summary>
    /// 次要的填充颜色
    /// </summary>
    public partial Color SecondaryFillColor { get; set; }
    
    /// <summary>
    /// 备用颜色
    /// </summary>
    public partial Color FallbackColor { get; set; }
    
    /// <summary>
    /// 默认的描边笔宽度
    /// </summary>
    public partial double StrokeWidth { get; set; }
    
    /// <summary>
    /// 默认的描边的 LineCap
    /// </summary>
    public partial PenLineCap StrokeLineCap { get; set; }
    
    /// <summary>
    /// 默认的描边的 LineJoin
    /// </summary>
    public partial PenLineJoin StrokeLineJoin { get; set; }
    
    public IconToken()

    {
        SecondaryStrokeColor = Colors.White;
        SecondaryFillColor   = Color.Parse("#43CCF8");
        FallbackColor        = Colors.White;
        StrokeWidth          = 4;
        StrokeLineCap        = PenLineCap.Round;
        StrokeLineJoin       = PenLineJoin.Round;
    }
    
}
