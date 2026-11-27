using Avalonia;

namespace AtomUI.Media;

/// <summary>
/// Helpers for scaling rectangles and for reducing a corner radius to a single value.
/// </summary>
public static class GeometryUtils
{
    /// <summary>
    /// Returns the rectangle that results from scaling <paramref name="rect" /> about <paramref name="origin" />:
    /// the origin stays where it is.
    /// </summary>
    /// <param name="rect">
    /// The rectangle to scale.
    /// </param>
    /// <param name="scaleFactorX">
    /// The factor applied to the width and to the horizontal distance from the origin.
    /// </param>
    /// <param name="scaleFactorY">
    /// The factor applied to the height and to the vertical distance from the origin.
    /// </param>
    /// <param name="origin">
    /// The fixed point of the scaling, in the same coordinate space as <paramref name="rect" />.
    /// </param>
    /// <returns>
    /// The scaled rectangle.
    /// </returns>
    public static Rect CalculateScaledRect(in Rect rect, double scaleFactorX, double scaleFactorY, in Point origin)
    {
        // Offset of the origin from the top-left corner of the rect.
        var originalOffsetX = origin.X - rect.X;
        var originalOffsetY = origin.Y - rect.Y;

        // The same offset after the scaling.
        var scaledOffsetX = originalOffsetX * scaleFactorX;
        var scaledOffsetY = originalOffsetY * scaleFactorY;

        // How far the scaling moved the offset.
        var offsetXChange = scaledOffsetX - originalOffsetX;
        var offsetYChange = scaledOffsetY - originalOffsetY;

        // Move the rect by the opposite amount, so the origin keeps its position.
        var newX = rect.X - offsetXChange;
        var newY = rect.Y - offsetYChange;

        // The scaled size.
        var newWidth  = rect.Width * scaleFactorX;
        var newHeight = rect.Height * scaleFactorY;

        return new Rect(newX, newY, newWidth, newHeight);
    }

    /// <summary>
    /// Reduces a corner radius to a single value.
    /// </summary>
    /// <param name="cornerRadius">
    /// The corner radius to reduce.
    /// </param>
    /// <param name="maxWhenNotUniform">
    /// Which corner to use when the corners differ: <see langword="true" /> takes the largest radius and
    /// <see langword="false" /> the smallest.
    /// </param>
    /// <returns>
    /// The common radius when all four corners are equal, otherwise the selected corner.
    /// </returns>
    public static double CornerRadiusScalarValue(CornerRadius cornerRadius, bool maxWhenNotUniform = true)
    {
        if (cornerRadius.IsUniform)
        {
            return cornerRadius.TopLeft;
        }

        return maxWhenNotUniform
            ? Math.Max(cornerRadius.TopLeft,
                Math.Max(cornerRadius.TopRight, Math.Max(cornerRadius.BottomLeft, cornerRadius.BottomRight)))
            : Math.Min(cornerRadius.TopLeft,
                Math.Min(cornerRadius.TopRight, Math.Min(cornerRadius.BottomLeft, cornerRadius.BottomRight)));
    }
}