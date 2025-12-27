using AtomUI.Controls;
using Avalonia;

namespace AtomUI.Icons.AntDesign;

public class AntDesignIcon : Icon
{
    private Rect? _geometryBounds;
    
    protected override Matrix CalculateGlobalGeometryMatrix()
    {
        _geometryBounds ??= CalculateGeometryBounds();
        return CalculateZoomToFit(ViewBox, _geometryBounds ?? default);
    }
    
    private static Matrix CalculateZoomToFit(Rect viewbox, Rect iconBounds)
    {
        // 计算 ViewBox 的中心点
        Point viewboxCenter = new Point(
            viewbox.Left + viewbox.Width / 2,
            viewbox.Top + viewbox.Height / 2
        );
        
        var leftDelta   = iconBounds.Left - viewbox.Left;
        var rightDelta  = viewbox.Right - iconBounds.Right;
        var topDelta    = iconBounds.Top - viewbox.Top;
        var bottomDelta = viewbox.Bottom - iconBounds.Bottom;

        var minDelta = leftDelta;
        if (rightDelta < minDelta)
        {
            minDelta = rightDelta;
        }

        if (topDelta < minDelta)
        {
            minDelta = topDelta;
        }

        if (bottomDelta < minDelta)
        {
            minDelta = bottomDelta;
        }

        minDelta /= 2; // 保留一半
        
        // 计算图标的四个边界到 ViewBox 中心的距离（带符号）
        double iconLeftDist   = iconBounds.Left - viewboxCenter.X - minDelta;
        double iconRightDist  = iconBounds.Right - viewboxCenter.X - minDelta;
        double iconTopDist    = iconBounds.Top - viewboxCenter.Y -  minDelta;
        double iconBottomDist = iconBounds.Bottom - viewboxCenter.Y - minDelta;
        
        // 计算 ViewBox 四个边界到中心的距离
        double viewboxLeftDist   = viewbox.Left - viewboxCenter.X;
        double viewboxRightDist  = viewbox.Right - viewboxCenter.X;
        double viewboxTopDist    = viewbox.Top - viewboxCenter.Y;
        double viewboxBottomDist = viewbox.Bottom - viewboxCenter.Y;
        
        // 计算四个方向上的最大缩放比例
        double maxScale = double.MaxValue;
        
        // 左边界
        if (Math.Abs(iconLeftDist) > 0.0001)
        {
            double scaleLeft = viewboxLeftDist / iconLeftDist;
            if (scaleLeft > 0 && scaleLeft < maxScale)
            {
                maxScale = scaleLeft;
            }
        }
        
        // 右边界
        if (Math.Abs(iconRightDist) > 0.0001)
        {
            double scaleRight = viewboxRightDist / iconRightDist;
            if (scaleRight > 0 && scaleRight < maxScale)
            {
                maxScale = scaleRight;
            }
        }
        
        // 上边界
        if (Math.Abs(iconTopDist) > 0.0001)
        {
            double scaleTop = viewboxTopDist / iconTopDist;
            if (scaleTop > 0 && scaleTop < maxScale)
            {
                maxScale = scaleTop;
            }
        }
        
        // 下边界
        if (Math.Abs(iconBottomDist) > 0.0001)
        {
            double scaleBottom = viewboxBottomDist / iconBottomDist;
            if (scaleBottom > 0 && scaleBottom < maxScale)
            {
                maxScale = scaleBottom;
            }
        }
        
        // 确保缩放比例合理
        if (maxScale > 1000 || maxScale <= 0)
        {
            maxScale = 1.0;
        }

        // 创建变换矩阵
        Matrix transform = Matrix.Identity;
        transform *= Matrix.CreateTranslation(-viewboxCenter.X, -viewboxCenter.Y);
        transform *= Matrix.CreateScale(maxScale, maxScale);
        transform *= Matrix.CreateTranslation(viewboxCenter.X, viewboxCenter.Y);
        
        return transform;
    }
}