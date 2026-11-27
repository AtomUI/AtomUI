using System.Collections.Specialized;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace AtomUI.Media;

/// <summary>
/// Creates and updates the pens that controls keep for their <c>Render</c> method.
/// </summary>
public static class PenUtils
{
   /// <summary>
   /// Brings a cached pen to the given brush and stroke settings, reusing the previous pen where that is possible.
   /// </summary>
   /// <param name="pen">
   /// The pen used so far. On return it is the pen to draw with, or <see langword="null" /> when
   /// <paramref name="brush" /> is <see langword="null" />.
   /// </param>
   /// <param name="brush">
   /// The brush that strokes the line. <see langword="null" /> clears the pen.
   /// </param>
   /// <param name="thickness">
   /// The stroke thickness.
   /// </param>
   /// <param name="strokeDashArray">
   /// The lengths of the dashes and gaps. <see langword="null" /> or an empty list draws a solid line.
   /// </param>
   /// <param name="strokeDaskOffset">
   /// The offset into the dash pattern.
   /// </param>
   /// <param name="lineCap">
   /// The line cap.
   /// </param>
   /// <param name="lineJoin">
   /// The line join.
   /// </param>
   /// <param name="miterLimit">
   /// The miter limit.
   /// </param>
   /// <returns>
   /// <see langword="true" /> when a new instance was created or the pen was cleared, which means the caller has to
   /// invalidate what it draws with the pen. <see langword="false" /> when the previous pen was kept, either because
   /// it already matched or because its properties were changed in place.
   /// </returns>
   internal static bool TryModifyOrCreate(ref IPen? pen,
                                          IBrush? brush,
                                          double thickness,
                                          IReadOnlyList<double>? strokeDashArray = null,
                                          double strokeDaskOffset = 0.0,
                                          PenLineCap lineCap = PenLineCap.Flat,
                                          PenLineJoin lineJoin = PenLineJoin.Miter,
                                          double miterLimit = 10.0)
    {
        var previousPen = pen;
        if (brush is null)
        {
            pen = null;
            return previousPen is not null;
        }

        if (IsSamePen(previousPen,
                brush,
                thickness,
                strokeDashArray,
                strokeDaskOffset,
                lineCap,
                lineJoin,
                miterLimit))
        {
            return false;
        }

        IDashStyle? dashStyle = null;
        if (strokeDashArray is { Count: > 0 })
        {
            // strokeDashArray can be IList (instead of AvaloniaList) in future
            // So, if it supports notification - create a mutable DashStyle
            dashStyle = strokeDashArray is INotifyCollectionChanged
                ? new DashStyle(strokeDashArray, strokeDaskOffset)
                : new ImmutableDashStyle(strokeDashArray, strokeDaskOffset);
        }

        if (brush is IImmutableBrush immutableBrush && dashStyle is null or ImmutableDashStyle)
        {
            pen = new ImmutablePen(
                immutableBrush,
                thickness,
                (ImmutableDashStyle?)dashStyle,
                lineCap,
                lineJoin,
                miterLimit);

            return true;
        }

        var mutablePen = previousPen as Pen ?? new Pen();
        mutablePen.Brush      = brush;
        mutablePen.Thickness  = thickness;
        mutablePen.LineCap    = lineCap;
        mutablePen.LineJoin   = lineJoin;
        mutablePen.DashStyle  = dashStyle;
        mutablePen.MiterLimit = miterLimit;

        pen = mutablePen;
        return !Equals(previousPen, pen);
    }

    private static bool IsSamePen(IPen? pen,
                                  IBrush brush,
                                  double thickness,
                                  IReadOnlyList<double>? strokeDashArray,
                                  double strokeDaskOffset,
                                  PenLineCap lineCap,
                                  PenLineJoin lineJoin,
                                  double miterLimit)
    {
        return pen is not null &&
               Equals(pen.Brush, brush) &&
               pen.Thickness.Equals(thickness) &&
               pen.LineCap == lineCap &&
               pen.LineJoin == lineJoin &&
               pen.MiterLimit.Equals(miterLimit) &&
               IsSameDashStyle(pen.DashStyle, strokeDashArray, strokeDaskOffset);
    }

    private static bool IsSameDashStyle(IDashStyle? dashStyle,
                                        IReadOnlyList<double>? strokeDashArray,
                                        double strokeDaskOffset)
    {
        if (strokeDashArray is not { Count: > 0 })
        {
            return dashStyle is null || dashStyle.Dashes is not { Count: > 0 };
        }

        if (dashStyle is null ||
            !dashStyle.Offset.Equals(strokeDaskOffset) ||
            dashStyle.Dashes is not { } dashes ||
            dashes.Count != strokeDashArray.Count)
        {
            return false;
        }

        for (var i = 0; i < strokeDashArray.Count; i++)
        {
            if (!dashes[i].Equals(strokeDashArray[i]))
            {
                return false;
            }
        }

        return true;
    }
}
