using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// 分隔符字形墨迹居中计算。字形墨迹盒来自字体表的 glyph metrics（设计单位），
/// 相对基线原点描述：XBearing 为墨迹左缘水平偏移，YBearing 为墨迹顶缘
/// 在基线上方的高度（向上为正）。目标是把墨迹中心平移到行盒中心：
/// 水平方向对齐 advance 的中点，垂直方向对齐行盒的几何中心。
/// </summary>
internal static class OtpSeparatorInkMath
{
    public static (double OffsetX, double OffsetY) ComputeInkCenterOffset(
        GlyphMetrics metrics,
        double advance,
        double baseline,
        double lineHeight,
        double fontRenderingEmSize,
        double designEmHeight)
    {
        if (designEmHeight <= 0 || fontRenderingEmSize <= 0 || lineHeight <= 0)
        {
            return (0, 0);
        }

        var scale = fontRenderingEmSize / designEmHeight;
        var inkWidth  = metrics.Width * scale;
        var inkHeight = metrics.Height * scale;
        if (inkWidth <= 0 || inkHeight <= 0)
        {
            return (0, 0);
        }

        var inkLeft  = metrics.XBearing * scale;
        var inkTop   = metrics.YBearing * scale;
        var inkCenterX = inkLeft + inkWidth / 2;
        var offsetX    = advance / 2 - inkCenterX;

        // 垂直：inkCenterAboveBaseline 为正表示墨迹中心高于基线。
        var inkCenterAboveBaseline = inkTop - inkHeight / 2;
        var boxCenterAboveBaseline = baseline - lineHeight / 2;
        var offsetY                = inkCenterAboveBaseline - boxCenterAboveBaseline;

        return (offsetX, offsetY);
    }
}
