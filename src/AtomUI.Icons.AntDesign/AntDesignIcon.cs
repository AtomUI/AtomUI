using AtomUI.Controls;
using Avalonia;

namespace AtomUI.Icons.AntDesign;

/// <summary>
/// Base class of the generated Ant Design icon classes. It zooms the glyph to the bounds of its shapes when the icon
/// is rendered, see <see cref="CalculateGlobalGeometryMatrix" />.
/// </summary>
public class AntDesignIcon : Icon
{
    private Rect? _geometryBounds;

    /// <summary>
    /// Returns a new <see cref="AntDesignIcon" /> in its default state, in place of reflection, so that code
    /// which builds a copy of an icon stays AOT-safe. The generated icon classes override it again to return
    /// their own type.
    /// </summary>
    /// <returns>
    /// A new instance of <see cref="AntDesignIcon" />.
    /// </returns>
    public override Icon CreateInstance()
    {
        return new AntDesignIcon();
    }
    
    /// <summary>
    /// Returns a uniform scale about the center of <see cref="Icon.ViewBox" /> that zooms the glyph to the
    /// bounds of its shapes, as <see cref="Icon.CalculateGeometryBounds" /> reports them.
    /// </summary>
    /// <returns>
    /// The transform, in the units of the view box; it is read by <see cref="Icon.Render" /> once per render. It is the
    /// identity when the computed scale is not positive or is greater than 1000.
    /// </returns>
    /// <remarks>
    /// The shape bounds are computed on the first call and kept from then on, so a later change of
    /// <see cref="Icon.DrawingInstructions" /> is not picked up.
    /// </remarks>
    protected override Matrix CalculateGlobalGeometryMatrix()
    {
        _geometryBounds ??= CalculateGeometryBounds();
        return CalculateZoomToFit(ViewBox, _geometryBounds ?? default);
    }
    
    private static Matrix CalculateZoomToFit(Rect viewbox, Rect iconBounds)
    {
        // Center of the view box.
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

        minDelta /= 2; // Keep half of it.
        
        // Signed distances from the four icon bounds to the center of the view box.
        double iconLeftDist   = iconBounds.Left - viewboxCenter.X - minDelta;
        double iconRightDist  = iconBounds.Right - viewboxCenter.X - minDelta;
        double iconTopDist    = iconBounds.Top - viewboxCenter.Y -  minDelta;
        double iconBottomDist = iconBounds.Bottom - viewboxCenter.Y - minDelta;
        
        // Distances from the four view box bounds to the center.
        double viewboxLeftDist   = viewbox.Left - viewboxCenter.X;
        double viewboxRightDist  = viewbox.Right - viewboxCenter.X;
        double viewboxTopDist    = viewbox.Top - viewboxCenter.Y;
        double viewboxBottomDist = viewbox.Bottom - viewboxCenter.Y;
        
        // The largest scale ratio that the four directions allow.
        double maxScale = double.MaxValue;
        
        // Left bound.
        if (Math.Abs(iconLeftDist) > 0.0001)
        {
            double scaleLeft = viewboxLeftDist / iconLeftDist;
            if (scaleLeft > 0 && scaleLeft < maxScale)
            {
                maxScale = scaleLeft;
            }
        }
        
        // Right bound.
        if (Math.Abs(iconRightDist) > 0.0001)
        {
            double scaleRight = viewboxRightDist / iconRightDist;
            if (scaleRight > 0 && scaleRight < maxScale)
            {
                maxScale = scaleRight;
            }
        }
        
        // Top bound.
        if (Math.Abs(iconTopDist) > 0.0001)
        {
            double scaleTop = viewboxTopDist / iconTopDist;
            if (scaleTop > 0 && scaleTop < maxScale)
            {
                maxScale = scaleTop;
            }
        }
        
        // Bottom bound.
        if (Math.Abs(iconBottomDist) > 0.0001)
        {
            double scaleBottom = viewboxBottomDist / iconBottomDist;
            if (scaleBottom > 0 && scaleBottom < maxScale)
            {
                maxScale = scaleBottom;
            }
        }
        
        // Make sure the scale ratio is sane.
        if (maxScale > 1000 || maxScale <= 0)
        {
            maxScale = 1.0;
        }

        // Create the transform matrix.
        Matrix transform = Matrix.Identity;
        transform *= Matrix.CreateTranslation(-viewboxCenter.X, -viewboxCenter.Y);
        transform *= Matrix.CreateScale(maxScale, maxScale);
        transform *= Matrix.CreateTranslation(viewboxCenter.X, viewboxCenter.Y);
        
        return transform;
    }
}
