using Avalonia;
using Avalonia.Media;

namespace AtomUI.Media;

/// <summary>
/// Measures the space a box shadow needs around the element it belongs to.
/// </summary>
public static class BoxShadowExtensions
{
    /// <summary>
    /// Measures how far the shadow extends beyond the element on each side.
    /// </summary>
    /// <returns>
    /// The space around the element that the shadow reaches. A side is 0 when the offset moves the shadow
    /// back inside the element on that side. <c>IsInset</c> is ignored: an inset shadow is measured
    /// like a shadow drawn outside the element.
    /// </returns>
    public static Thickness Thickness(this BoxShadow boxShadow)
    {
        var offsetX      = boxShadow.OffsetX;
        var offsetY      = boxShadow.OffsetY;
        var blurRadius   = boxShadow.Blur;
        var spreadRadius = boxShadow.Spread;

        // Blur and spread can have opposite signs and cancel each other out.
        var value  = Math.Max(blurRadius + spreadRadius, 0.0);
        var left   = Math.Max(value - offsetX, 0.0);
        var right  = Math.Max(value + offsetX, 0.0);
        var top    = Math.Max(value - offsetY, 0.0);
        var bottom = Math.Max(value + offsetY, 0.0);
        return new Thickness(left, top, right, bottom);
    }

    /// <summary>
    /// Measures how far the shadows extend beyond the element on each side.
    /// </summary>
    /// <returns>
    /// The space around the element that covers every shadow; all sides are 0 when there is no shadow.
    /// </returns>
    public static Thickness Thickness(this BoxShadows boxShadows)
    {
        double leftThickness   = 0;
        double topThickness    = 0;
        double rightThickness  = 0;
        double bottomThickness = 0;
        foreach (var shadow in boxShadows)
        {
            var thickness = shadow.Thickness();
            leftThickness   = Math.Max(leftThickness, thickness.Left);
            topThickness    = Math.Max(topThickness, thickness.Top);
            rightThickness  = Math.Max(rightThickness, thickness.Right);
            bottomThickness = Math.Max(bottomThickness, thickness.Bottom);
        }

        return new Thickness(leftThickness, topThickness, rightThickness, bottomThickness);
    }
}