using Avalonia;

namespace AtomUI.Desktop.Controls;

internal readonly record struct ImagePreviewTransformState(
    double X,
    double Y,
    double Scale,
    double Rotate,
    bool FlipX,
    bool FlipY)
{
    internal static ImagePreviewTransformState Initial { get; } = new(0, 0, 1, 0, false, false);
}

internal static class ImagePreviewTransformMath
{
    private const double BaseImageHeightRatio = 0.70;

    internal static Size CalculateBaseImageSize(Size imageSize, Size viewportSize)
    {
        if (!IsPositiveFinite(imageSize.Width) ||
            !IsPositiveFinite(imageSize.Height) ||
            !IsPositiveFinite(viewportSize.Width) ||
            !IsPositiveFinite(viewportSize.Height))
        {
            return new Size(0, 0);
        }

        var fitScale = Math.Min(
            viewportSize.Width / imageSize.Width,
            viewportSize.Height * BaseImageHeightRatio / imageSize.Height);
        if (!IsPositiveFinite(fitScale))
        {
            return new Size(0, 0);
        }

        return new Size(imageSize.Width * fitScale, imageSize.Height * fitScale);
    }

    internal static (double Min, double Max) NormalizeScaleRange(double minScale, double maxScale)
    {
        var min = IsPositiveFinite(minScale) ? minScale : 1.0;
        var max = IsPositiveFinite(maxScale) ? Math.Max(min, maxScale) : min;
        return (min, max);
    }

    internal static ImagePreviewTransformState Zoom(
        ImagePreviewTransformState state,
        double ratio,
        double minScale,
        double maxScale)
    {
        if (!IsPositiveFinite(ratio))
        {
            return state;
        }

        var (lowerBound, upperBound) = NormalizeScaleRange(minScale, maxScale);
        var scale = Clamp(state.Scale * ratio, lowerBound, upperBound);
        return state with { Scale = scale };
    }

    internal static ImagePreviewTransformState ZoomAt(
        ImagePreviewTransformState state,
        Size baseImageSize,
        Size viewportSize,
        Point pointer,
        double ratio,
        double minScale,
        double maxScale)
    {
        var next = Zoom(state, ratio, minScale, maxScale);
        if (!IsPositiveFinite(state.Scale) ||
            !IsPositiveFinite(next.Scale) ||
            !IsPositiveFinite(baseImageSize.Width) ||
            !IsPositiveFinite(baseImageSize.Height) ||
            !IsPositiveFinite(viewportSize.Width) ||
            !IsPositiveFinite(viewportSize.Height))
        {
            return Clamp(next, baseImageSize, viewportSize);
        }

        var viewportCenter = new Point(viewportSize.Width / 2, viewportSize.Height / 2);
        // Uniform zoom scales the screen-space vector, including rotation and flips.
        var pointerOffset = pointer - viewportCenter - new Vector(state.X, state.Y);
        var nextTranslation = pointer - viewportCenter - pointerOffset * (next.Scale / state.Scale);
        return Clamp(next with { X = nextTranslation.X, Y = nextTranslation.Y }, baseImageSize, viewportSize);
    }

    internal static ImagePreviewTransformState Clamp(
        ImagePreviewTransformState state,
        Size baseImageSize,
        Size viewportSize)
    {
        if (!IsPositiveFinite(baseImageSize.Width) ||
            !IsPositiveFinite(baseImageSize.Height) ||
            !IsPositiveFinite(viewportSize.Width) ||
            !IsPositiveFinite(viewportSize.Height))
        {
            return state with { X = 0, Y = 0 };
        }

        var scale = IsPositiveFinite(state.Scale) ? state.Scale : 1.0;
        var angle = Math.Abs(state.Rotate) % 180;
        var rotated = Math.Abs(angle - 90) < 0.000001;
        var width = (rotated ? baseImageSize.Height : baseImageSize.Width) * scale;
        var height = (rotated ? baseImageSize.Width : baseImageSize.Height) * scale;
        var maxX = Math.Max(0, (width - viewportSize.Width) / 2);
        var maxY = Math.Max(0, (height - viewportSize.Height) / 2);
        return state with
        {
            Scale = scale,
            X = Clamp(state.X, -maxX, maxX),
            Y = Clamp(state.Y, -maxY, maxY)
        };
    }

    internal static double CalculateWheelRatio(double deltaY, double scaleStep)
    {
        if (Math.Abs(deltaY) < 0.000001)
        {
            return 1.0;
        }

        var step = IsPositiveFinite(scaleStep) ? scaleStep : 0.5;
        var delta = Math.Min(Math.Abs(deltaY / 100.0), 1.0);
        var ratio = 1.0 + delta * step;
        return deltaY > 0 ? ratio : 1.0 / ratio;
    }

    internal static PixelSize CalculateDecodeSize(Size baseImageSize, double scale, double renderScaling)
    {
        if (!IsPositiveFinite(baseImageSize.Width) || !IsPositiveFinite(baseImageSize.Height) ||
            !IsPositiveFinite(scale) || !IsPositiveFinite(renderScaling))
        {
            return default;
        }

        return new PixelSize(
            Quantize(baseImageSize.Width * scale * renderScaling),
            Quantize(baseImageSize.Height * scale * renderScaling));
    }

    private static int Quantize(double value)
    {
        // Saturate arithmetic overflow; the shared loader enforces the configured image budgets.
        const int largestBucket = int.MaxValue / 16 * 16;
        return (int)Math.Min(largestBucket, Math.Ceiling(value / 16.0) * 16.0);
    }

    private static bool IsPositiveFinite(double value)
    {
        return double.IsFinite(value) && value > 0;
    }

    private static double Clamp(double value, double min, double max)
    {
        return Math.Min(Math.Max(value, min), max);
    }
}
