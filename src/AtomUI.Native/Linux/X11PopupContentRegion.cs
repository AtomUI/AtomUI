using System.Runtime.Versioning;
using Avalonia;

namespace AtomUI.Native.Linux;

/// <summary>
/// 把窗口型 Popup 的内容形状（圆角矩形 + 箭头三角形）光栅化为 device pixel 矩形的并集，
/// 作为 X11 SHAPE input region 提交。
/// 按像素行扫描，像素中心落在形状内即属于输入区域；相邻行区间相同则合并，矩形数量约为圆角与箭头所占行数。
/// </summary>
[SupportedOSPlatform("linux")]
internal static class X11PopupContentRegion
{
    /// <param name="shape">客户区 DIP 坐标的内容形状。</param>
    /// <param name="scaling">DIP 到 device pixel 的缩放。</param>
    public static List<PixelRect> Rasterize(PopupContentShape shape, double scaling)
    {
        var result = new List<PixelRect>();
        if (shape.Body.Width <= 0 || shape.Body.Height <= 0 || !double.IsFinite(scaling) || scaling <= 0)
        {
            return result;
        }

        var body         = shape.Body * scaling;
        var arrowSide    = shape.ArrowSide;
        var arrowBounds  = shape.ArrowBounds * scaling;
        var hasArrow     = arrowSide != PopupArrowSide.None && arrowBounds.Width > 0 && arrowBounds.Height > 0;
        var cornerRadius = shape.CornerRadius;
        var maxRadius    = Math.Min(body.Width, body.Height) / 2;
        var topLeft      = Math.Clamp(cornerRadius.TopLeft * scaling, 0, maxRadius);
        var topRight     = Math.Clamp(cornerRadius.TopRight * scaling, 0, maxRadius);
        var bottomRight  = Math.Clamp(cornerRadius.BottomRight * scaling, 0, maxRadius);
        var bottomLeft   = Math.Clamp(cornerRadius.BottomLeft * scaling, 0, maxRadius);

        var shapeTop    = hasArrow ? Math.Min(body.Top, arrowBounds.Top) : body.Top;
        var shapeBottom = hasArrow ? Math.Max(body.Bottom, arrowBounds.Bottom) : body.Bottom;
        var firstRow    = (int)Math.Floor(shapeTop);
        var lastRow     = (int)Math.Ceiling(shapeBottom);

        for (var row = firstRow; row < lastRow; row++)
        {
            var y     = row + 0.5;
            var left  = double.PositiveInfinity;
            var right = double.NegativeInfinity;

            if (y >= body.Top && y < body.Bottom)
            {
                var leftInset = Math.Max(
                    CornerInset(topLeft, body.Top + topLeft - y),
                    CornerInset(bottomLeft, y - (body.Bottom - bottomLeft)));
                var rightInset = Math.Max(
                    CornerInset(topRight, body.Top + topRight - y),
                    CornerInset(bottomRight, y - (body.Bottom - bottomRight)));
                left  = body.Left + leftInset;
                right = body.Right - rightInset;
            }

            if (hasArrow && TryGetArrowSpan(arrowBounds, arrowSide, y, out var arrowLeft, out var arrowRight))
            {
                // 箭头底边贴住主体，同一行的两个区间相连，取并集的外包即可
                left  = Math.Min(left, arrowLeft);
                right = Math.Max(right, arrowRight);
            }

            var x0 = (int)Math.Ceiling(left - 0.5);
            var x1 = (int)Math.Floor(right - 0.5) + 1;
            if (x1 <= x0)
            {
                continue;
            }

            if (result.Count > 0)
            {
                var last = result[^1];
                if (last.X == x0 && last.Right == x1 && last.Bottom == row)
                {
                    result[^1] = last.WithHeight(last.Height + 1);
                    continue;
                }
            }

            result.Add(new PixelRect(x0, row, x1 - x0, 1));
        }

        return result;
    }

    // 圆角在距角圆心纵向 dy 处的水平内缩量；dy <= 0 表示该行不在此圆角范围内
    private static double CornerInset(double radius, double dy)
    {
        if (radius <= 0 || dy <= 0)
        {
            return 0;
        }

        dy = Math.Min(dy, radius);
        return radius - Math.Sqrt(radius * radius - dy * dy);
    }

    private static bool TryGetArrowSpan(Rect arrow, PopupArrowSide side, double y, out double left, out double right)
    {
        left  = 0;
        right = 0;
        if (y < arrow.Top || y >= arrow.Bottom)
        {
            return false;
        }

        switch (side)
        {
            case PopupArrowSide.Top:
            case PopupArrowSide.Bottom:
            {
                // 顶点在外侧边中点，底边贴住主体
                var progress = side == PopupArrowSide.Top
                    ? (y - arrow.Top) / arrow.Height
                    : (arrow.Bottom - y) / arrow.Height;
                var halfWidth = progress * arrow.Width / 2;
                left  = arrow.Center.X - halfWidth;
                right = arrow.Center.X + halfWidth;
                return halfWidth > 0;
            }
            case PopupArrowSide.Left:
            case PopupArrowSide.Right:
            {
                var distance = Math.Abs(y - arrow.Center.Y) / (arrow.Height / 2);
                var cut      = Math.Clamp(distance, 0, 1) * arrow.Width;
                if (side == PopupArrowSide.Left)
                {
                    left  = arrow.Left + cut;
                    right = arrow.Right;
                }
                else
                {
                    left  = arrow.Left;
                    right = arrow.Right - cut;
                }
                return right > left;
            }
            default:
                return false;
        }
    }
}
