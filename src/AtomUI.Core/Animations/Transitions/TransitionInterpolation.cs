using AtomUI.Media;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Media.Transformation;

namespace AtomUI.Animations;

internal static class TransitionInterpolation
{
    
    public static Color ColorInterpolate(double progress, Color fromColor, Color toColor)
    {
        var a1 = fromColor.GetAlphaF();
        var r1 = fromColor.GetRedF() * a1;
        var g1 = fromColor.GetGreenF() * a1;
        var b1 = fromColor.GetBlueF() * a1;

        var a2 = toColor.GetAlphaF();
        var r2 = toColor.GetRedF() * a2;
        var g2 = toColor.GetGreenF() * a2;
        var b2 = toColor.GetBlueF() * a2;

        var r = DoubleInterpolate(progress, r1, r2);
        var g = DoubleInterpolate(progress, g1, g2);
        var b = DoubleInterpolate(progress, b1, b2);
        var a = DoubleInterpolate(progress, a1, a2);

        // 处理接近完全透明的情况
        if (a < 1e-5)
        {
            // 在这种情况下，我们可以选择直接使用目标颜色的RGB分量
            r = toColor.GetRedF();
            g = toColor.GetGreenF();
            b = toColor.GetBlueF();
        }
        else
        {
            // 如果alpha不为零，反预乘alpha
            r /= a;
            g /= a;
            b /= a;
        }

        // 防止颜色分量超出范围
        r = Math.Clamp(r, 0.0f, 1.0f);
        g = Math.Clamp(g, 0.0f, 1.0f);
        b = Math.Clamp(b, 0.0f, 1.0f);
        a = Math.Clamp(a, 0.0f, 1.0f);

        return ColorUtils.FromRgbF(a, r, g, b);
    }

    public static double DoubleInterpolate(double progress, double oldValue, double newValue)
    {
        return (newValue - oldValue) * progress + oldValue;
    }

    private static bool BoolInterpolate(double progress, bool oldValue, bool newValue)
    {
        return progress >= 1d ? newValue : oldValue;
    }

    public static BoxShadows BoxShadowsInterpolate(double progress, BoxShadows oldValue, BoxShadows newValue)
    {
        int cnt = progress >= 1d ? newValue.Count : oldValue.Count;
        if (cnt == 0)
        {
            return new BoxShadows();
        }
        BoxShadow first;
        if (oldValue.Count > 0 && newValue.Count > 0)
        {
            first = BoxShadowInterpolate(progress, oldValue[0], newValue[0]);
        }
        else if (oldValue.Count > 0)
        {
            first = oldValue[0];
        }
        else
        {
            first = newValue[0];
        }
        
        if (cnt == 1)
        {
            return new BoxShadows(first);
        }
        
        var rest = new BoxShadow[cnt - 1];
        for (var c = 0; c < rest.Length; c++)
        {
            var idx = c + 1;
            if (oldValue.Count > idx && newValue.Count > idx)
            {
                rest[c] = BoxShadowInterpolate(progress, oldValue[idx], newValue[idx]);
            }
            else if (oldValue.Count > idx)
            {
                rest[c] = oldValue[idx];
            }
            else
            {
                rest[c] = newValue[idx];
            }
        }

        return new BoxShadows(first, rest);
    }

    public static BoxShadow BoxShadowInterpolate(double progress, BoxShadow oldValue, BoxShadow newValue)
    {
        return new BoxShadow
        {
            OffsetX = DoubleInterpolate(progress, oldValue.OffsetX, newValue.OffsetX),
            OffsetY = DoubleInterpolate(progress, oldValue.OffsetY, newValue.OffsetY),
            Blur    = DoubleInterpolate(progress, oldValue.Blur, newValue.Blur),
            Spread  = DoubleInterpolate(progress, oldValue.Spread, newValue.Spread),
            Color   = ColorInterpolate(progress, oldValue.Color, newValue.Color),
            IsInset = BoolInterpolate(progress, oldValue.IsInset, newValue.IsInset)
        };
    }

    public static Point PointInterpolate(double progress, Point oldValue, Point newValue)
    { 
        return ((newValue - oldValue) * progress) + oldValue;
    }

    public static RelativePoint PointInterpolate(double progress, RelativePoint oldValue, RelativePoint newValue)
    {
        if (oldValue.Unit != newValue.Unit)
        {
            return progress >= 0.5 ? newValue : oldValue;
        }

        return new RelativePoint(PointInterpolate(progress, oldValue.Point, newValue.Point), oldValue.Unit);
    }

    public static RelativeScalar RelativeScalarInterpolate(double progress, RelativeScalar oldValue, RelativeScalar newValue)
    {
        if (oldValue.Unit != newValue.Unit)
        {
            return progress >= 0.5 ? newValue : oldValue;
        }

        return new RelativeScalar(DoubleInterpolate(progress, oldValue.Scalar, newValue.Scalar), oldValue.Unit);
    }

    #region 渐变笔刷插值

    public static IGradientBrush? GradientBrushInterpolate(double progress, IGradientBrush? oldValue, IGradientBrush? newValue)
    {
        if (oldValue is null || newValue is null)
        {
            return progress >= 0.5 ? newValue : oldValue;
        }

        switch (oldValue)
        {
            case IRadialGradientBrush oldRadial when newValue is IRadialGradientBrush newRadial:
                return new ImmutableRadialGradientBrush(
                    InterpolateStops(progress, oldValue.GradientStops, newValue.GradientStops),
                    DoubleInterpolate(progress, oldValue.Opacity, newValue.Opacity),
                    InterpolateTransform(progress, oldValue.Transform, newValue.Transform),
                    PointInterpolate(progress, oldValue.TransformOrigin, newValue.TransformOrigin),
                    oldValue.SpreadMethod,
                    PointInterpolate(progress, oldRadial.Center, newRadial.Center),
                    PointInterpolate(progress, oldRadial.GradientOrigin, newRadial.GradientOrigin),
                    RelativeScalarInterpolate(progress, oldRadial.RadiusX, newRadial.RadiusX),
                    RelativeScalarInterpolate(progress, oldRadial.RadiusY, newRadial.RadiusY)
                );

            case IConicGradientBrush oldConic when newValue is IConicGradientBrush newConic:
                return new ImmutableConicGradientBrush(
                    InterpolateStops(progress, oldValue.GradientStops, newValue.GradientStops),
                    DoubleInterpolate(progress, oldValue.Opacity, newValue.Opacity),
                    InterpolateTransform(progress, oldValue.Transform, newValue.Transform),
                    PointInterpolate(progress, oldValue.TransformOrigin, newValue.TransformOrigin),
                    oldValue.SpreadMethod,
                    PointInterpolate(progress, oldConic.Center, newConic.Center),
                    DoubleInterpolate(progress, oldConic.Angle, newConic.Angle));

            case ILinearGradientBrush oldLinear when newValue is ILinearGradientBrush newLinear:
                return new ImmutableLinearGradientBrush(
                    InterpolateStops(progress, oldValue.GradientStops, newValue.GradientStops),
                    DoubleInterpolate(progress, oldValue.Opacity, newValue.Opacity),
                    InterpolateTransform(progress, oldValue.Transform, newValue.Transform),
                    PointInterpolate(progress, oldValue.TransformOrigin, newValue.TransformOrigin),
                    oldValue.SpreadMethod,
                    PointInterpolate(progress, oldLinear.StartPoint, newLinear.StartPoint),
                    PointInterpolate(progress, oldLinear.EndPoint, newLinear.EndPoint));

            default:
                return progress >= 0.5 ? newValue : oldValue;
        }
    }
    
    private static ImmutableTransform? InterpolateTransform(double progress,
                                                            ITransform? oldTransform, ITransform? newTransform)
    {
        if (oldTransform is TransformOperations oldTransformOperations
            && newTransform is TransformOperations newTransformOperations)
        {

            return new ImmutableTransform(TransformOperations
                                          .Interpolate(oldTransformOperations, newTransformOperations, progress).Value);
        }

        if (oldTransform is not null)
        {
            return new ImmutableTransform(oldTransform.Value);
        }
            
        return null;
    }
    
    private static IReadOnlyList<ImmutableGradientStop> InterpolateStops(double progress, IReadOnlyList<IGradientStop> oldValue, IReadOnlyList<IGradientStop> newValue)
    {
        var resultCount = Math.Max(oldValue.Count, newValue.Count);
        var stops       = new ImmutableGradientStop[resultCount];

        for (int index = 0, oldIndex = 0, newIndex = 0; index < resultCount; index++)
        {
            stops[index] = new ImmutableGradientStop(
                DoubleInterpolate(progress, oldValue[oldIndex].Offset, newValue[newIndex].Offset),
                ColorInterpolate(progress, oldValue[oldIndex].Color, newValue[newIndex].Color));

            if (oldIndex < oldValue.Count - 1)
            {
                oldIndex++;
            }

            if (newIndex < newValue.Count - 1)
            {
                newIndex++;
            }
        }
            
        return stops;
    }

    internal static IGradientBrush ConvertSolidColorBrushToGradient(IGradientBrush gradientBrush, ISolidColorBrush solidColorBrush)
    {
        switch (gradientBrush)
        {
            case IRadialGradientBrush oldRadial:
                return new ImmutableRadialGradientBrush(
                    CreateStopsFromSolidColorBrush(solidColorBrush, oldRadial.GradientStops), solidColorBrush.Opacity,
                    oldRadial.Transform is { } ? new ImmutableTransform(oldRadial.Transform.Value) : null,
                    oldRadial.TransformOrigin,
                    oldRadial.SpreadMethod, oldRadial.Center, oldRadial.GradientOrigin,
                    oldRadial.RadiusX, oldRadial.RadiusY);

            case IConicGradientBrush oldConic:
                return new ImmutableConicGradientBrush(
                    CreateStopsFromSolidColorBrush(solidColorBrush, oldConic.GradientStops), solidColorBrush.Opacity,
                    oldConic.Transform is { } ? new ImmutableTransform(oldConic.Transform.Value) : null,
                    oldConic.TransformOrigin,
                    oldConic.SpreadMethod, oldConic.Center, oldConic.Angle);

            case ILinearGradientBrush oldLinear:
                return new ImmutableLinearGradientBrush(
                    CreateStopsFromSolidColorBrush(solidColorBrush, oldLinear.GradientStops), solidColorBrush.Opacity,
                    oldLinear.Transform is { } ? new ImmutableTransform(oldLinear.Transform.Value) : null,
                    oldLinear.TransformOrigin,
                    oldLinear.SpreadMethod, oldLinear.StartPoint, oldLinear.EndPoint);

            default:
                throw new NotSupportedException($"Gradient of type {gradientBrush?.GetType()} is not supported");
        }

        static IReadOnlyList<ImmutableGradientStop> CreateStopsFromSolidColorBrush(ISolidColorBrush solidColorBrush, IReadOnlyList<IGradientStop> baseStops)
        {
            var stops = new ImmutableGradientStop[baseStops.Count];
            for (int index = 0; index < baseStops.Count; index++)
            {
                stops[index] = new ImmutableGradientStop(baseStops[index].Offset, solidColorBrush.Color);
            }
            return stops;
        }
    }

    public static ISolidColorBrush? SolidColorBrushInterpolate(double progress, ISolidColorBrush? oldValue,
                                                               ISolidColorBrush? newValue)
    {
        if (oldValue is null || newValue is null)
        {
            return progress >= 0.5 ? newValue : oldValue;
        }

        return new ImmutableSolidColorBrush(
            ColorInterpolate(progress, oldValue.Color, newValue.Color),
            DoubleInterpolate(progress, oldValue.Opacity, newValue.Opacity));
    }

    public static IBrush? BrushInterpolate(double progress, IBrush? oldValue, IBrush? newValue)
    {
        if (oldValue is null || newValue is null)
        {
            return IncompatibleTransitionInterpolate(progress, oldValue, newValue);
        }
        if (oldValue is IGradientBrush oldGradient)
        {
            if (newValue is IGradientBrush newGradient)
            {
                return GradientBrushInterpolate(progress, oldGradient, newGradient);
            } 
            if (newValue is ISolidColorBrush newSolidColorBrushToConvert)
            {
                var convertedSolidColorBrush = ConvertSolidColorBrushToGradient(oldGradient, newSolidColorBrushToConvert);
                return GradientBrushInterpolate(progress, oldGradient, convertedSolidColorBrush);
            }
        }
        else if (newValue is IGradientBrush newGradient && oldValue is ISolidColorBrush oldSolidColorBrushToConvert)
        {
            var convertedSolidColorBrush = ConvertSolidColorBrushToGradient(newGradient, oldSolidColorBrushToConvert);
            return GradientBrushInterpolate(progress, convertedSolidColorBrush, newGradient);
        }
        
        if (oldValue is ISolidColorBrush oldSolidColorBrush && newValue is ISolidColorBrush newSolidColorBrush)
        {
            return SolidColorBrushInterpolate(progress, oldSolidColorBrush, newSolidColorBrush);
        }

        return  IncompatibleTransitionInterpolate(progress, oldValue, newValue);
    }

    private static IBrush? IncompatibleTransitionInterpolate(double progress, IBrush? oldValue, IBrush? newValue)
    {
        return progress >= 0.5 ? newValue : oldValue;
    }

    #endregion
}
