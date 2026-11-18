using AtomUI.Media;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AtomUI.Controls.Utils;

/// <summary>
/// Contains helper methods for rendering a <see cref="Border" />'s background and border to a given context.
/// </summary>
internal class BorderRenderHelper
{
    private bool _useComplexRendering;
    private Geometry? _backgroundGeometryCache;
    private Geometry? _borderGeometryCache;
    private Size _size;
    private Thickness _borderThickness;
    private CornerRadius _cornerRadius;
    private BackgroundSizing _backgroundSizing;
    private bool _dashedEnabled;
    private bool _initialized;
    private IPen? _cachedPen;

    private void Update(Size finalSize, Thickness borderThickness, CornerRadius cornerRadius,
                        BackgroundSizing backgroundSizing,
                        bool dashedEnabled)
    {
        // CSS border-radius semantics: Skia's rounded rect (the box shadow and the simple drawing
        // path below) scales overlapping corner radii down by one uniform factor, while the WinUI
        // keypoint geometry used for the complex path partitions the overshoot into elliptical
        // arcs instead. Scale once here so the background, the border ring and the box shadow all
        // render the same shape and stay concentric under oversized theme radii (e.g. Luma/Maia
        // InputRadius 22/26 on a 32px-high input capsule).
        cornerRadius      = CornerRadiusUtils.ScaleToSize(cornerRadius, finalSize.Width, finalSize.Height);
        _size             = finalSize;
        _borderThickness  = borderThickness;
        _cornerRadius     = cornerRadius;
        _backgroundSizing = backgroundSizing;
        _dashedEnabled    = dashedEnabled;
        _initialized      = true;

        if (dashedEnabled ||
            (borderThickness.IsUniform &&
             cornerRadius.IsUniform &&
             backgroundSizing == BackgroundSizing.CenterBorder))
        {
            _backgroundGeometryCache = null;
            _borderGeometryCache     = null;
            _useComplexRendering     = false;
        }
        else
        {
            _useComplexRendering     = true;

            var boundRect = new Rect(finalSize);
            var innerRect = boundRect.Deflate(borderThickness);

            if (innerRect.Width != 0 && innerRect.Height != 0)
            {
                var backgroundOuterKeypoints = RoundRectGeometryBuilder.CalculateRoundedCornersRectangleWinUI(
                    boundRect,
                    borderThickness,
                    cornerRadius,
                    backgroundSizing);

                var backgroundGeometry = new StreamGeometry();
                using (var ctx = backgroundGeometry.Open())
                {
                    RoundRectGeometryBuilder.DrawRoundedCornersRectangle(ctx, ref backgroundOuterKeypoints);
                }

                _backgroundGeometryCache = backgroundGeometry;
            }
            else
            {
                _backgroundGeometryCache = null;
            }

            if (boundRect.Width != 0 && boundRect.Height != 0)
            {
                var borderInnerKeypoints = RoundRectGeometryBuilder.CalculateRoundedCornersRectangleWinUI(
                    boundRect,
                    borderThickness,
                    cornerRadius,
                    BackgroundSizing.InnerBorderEdge);
                var borderOuterKeypoints = RoundRectGeometryBuilder.CalculateRoundedCornersRectangleWinUI(
                    boundRect,
                    borderThickness,
                    cornerRadius,
                    BackgroundSizing.OuterBorderEdge);

                var borderInnerGeometry = new StreamGeometry();
                using (var ctx = borderInnerGeometry.Open())
                {
                    RoundRectGeometryBuilder.DrawRoundedCornersRectangle(ctx, ref borderInnerKeypoints);
                }

                var borderOuterGeometry = new StreamGeometry();
                using (var ctx = borderOuterGeometry.Open())
                {
                    RoundRectGeometryBuilder.DrawRoundedCornersRectangle(ctx, ref borderOuterKeypoints);
                }

                _borderGeometryCache =
                    new CombinedGeometry(GeometryCombineMode.Exclude, borderOuterGeometry, borderInnerGeometry);
            }
            else
            {
                _borderGeometryCache = null;
            }
        }
    }

    public void Render(
        DrawingContext context,
        Size finalSize,
        Thickness borderThickness,
        CornerRadius cornerRadius,
        BackgroundSizing backgroundSizing,
        IBrush? background,
        IBrush? borderBrush,
        IReadOnlyList<double>? strokeDashArray = null,
        double strokeDaskOffset = 0.0,
        BoxShadows boxShadows = default)
    {
        if (_size != finalSize
            || _borderThickness != borderThickness
            || _cornerRadius != cornerRadius
            || _backgroundSizing != backgroundSizing
            || _dashedEnabled != (strokeDashArray is { Count: > 0 })
            || !_initialized)
        {
            Update(finalSize, borderThickness, cornerRadius, backgroundSizing, strokeDashArray is { Count: > 0 });
        }

        if (_useComplexRendering)
        {
            if (boxShadows.Count > 0)
            {
                // Geometry drawing has no shadow overload; draw the outer shadows on the outer border edge first.
                BoxShadowRenderHelper.DrawRectangle(context, Brushes.Transparent, null,
                    new RoundedRect(new Rect(_size), _cornerRadius), boxShadows);
            }

            if (_backgroundGeometryCache != null)
            {
                context.DrawGeometry(background, null, _backgroundGeometryCache);
            }

            if (_borderGeometryCache != null)
            {
                context.DrawGeometry(borderBrush, null, _borderGeometryCache);
            }
        }
        else
        {
            var thickness = _borderThickness.Top;

            PenUtils.TryModifyOrCreate(ref _cachedPen, borderBrush, thickness, strokeDashArray, strokeDaskOffset);

            var rect = new Rect(_size);
            if (!MathUtils.IsZero(thickness))
            {
                rect = rect.Deflate(thickness * 0.5);
            }

            var rrect = new RoundedRect(rect, _cornerRadius.TopLeft, _cornerRadius.TopRight,
                _cornerRadius.BottomRight, _cornerRadius.BottomLeft);

            BoxShadowRenderHelper.DrawRectangle(context, background, _cachedPen, rrect, boxShadows);
        }
    }
}
