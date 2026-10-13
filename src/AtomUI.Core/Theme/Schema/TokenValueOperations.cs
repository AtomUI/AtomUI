using Avalonia.Animation.Easings;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace AtomUI.Theme.Schema;

/// <summary>
/// Typed ownership boundary shared by compilation, generated copies and snapshot consumers.
/// Unknown reference types remain the responsibility of their descriptor's dynamic codec.
/// </summary>
internal static class TokenValueOperations
{
    internal static bool IsCompiledType(Type type)
    {
        if (Nullable.GetUnderlyingType(type) is { } underlying)
        {
            return underlying == typeof(Color) || underlying == typeof(TextDecorationInfo);
        }
        return type == typeof(string) || type == typeof(int) || type == typeof(double) || type == typeof(float) ||
               type == typeof(bool) || type == typeof(Color) || type == typeof(TimeSpan) || type == typeof(FontFamily) ||
               type == typeof(Avalonia.Thickness) || type == typeof(Avalonia.CornerRadius) || type == typeof(Avalonia.Point) ||
               type == typeof(Avalonia.Size) || type == typeof(BoxShadow) || type == typeof(BoxShadows) ||
               type == typeof(TextDecorationInfo) || type == typeof(Dimension) ||
               type == typeof(Easing) || type == typeof(IBrush) || type == typeof(IImmutableBrush) ||
               type == typeof(SolidColorBrush) || type == typeof(ImmutableTransform) || type == typeof(LineStyle) ||
               type == typeof(FontWeight) || type == typeof(PenLineCap) || type == typeof(PenLineJoin);
    }

    internal static bool TryCopy(object? value, out object? copy)
    {
        copy = value;
        if (value is BoxShadows shadows)
        {
            if (shadows.Count > 1)
            {
                var rest = new BoxShadow[shadows.Count - 1];
                for (var index = 1; index < shadows.Count; index++)
                {
                    rest[index - 1] = shadows[index];
                }
                copy = new BoxShadows(shadows[0], rest);
            }
            return true;
        }
        if (value is null || value is string || value.GetType().IsValueType ||
            value is FontFamily or ImmutableTransform)
        {
            return true;
        }

        switch (value)
        {
            case SolidColorBrush brush:
                copy = ThemeResourceValue.CloneSolidColorBrush(brush);
                return true;
            case IBrush brush:
                copy = brush.ToImmutable();
                return true;
            case SplineEasing easing when value.GetType() == typeof(SplineEasing):
                copy = new SplineEasing(new Avalonia.Animation.KeySpline(easing.X1, easing.Y1, easing.X2, easing.Y2));
                return true;
            case SpringEasing easing when value.GetType() == typeof(SpringEasing):
                copy = new SpringEasing(easing.Mass, easing.Stiffness, easing.Damping, easing.InitialVelocity);
                return true;
            case Easing:
                // Only these exact framework types have no mutable instance state. A user subtype
                // may carry parameters or an application reference and must not inherit this rule.
                return IsStatelessEasing(value.GetType());
            default:
                return false;
        }
    }

    private static bool IsStatelessEasing(Type type) =>
        type == typeof(LinearEasing) ||
        type == typeof(BackEaseIn) || type == typeof(BackEaseOut) || type == typeof(BackEaseInOut) ||
        type == typeof(BounceEaseIn) || type == typeof(BounceEaseOut) || type == typeof(BounceEaseInOut) ||
        type == typeof(CircularEaseIn) || type == typeof(CircularEaseOut) || type == typeof(CircularEaseInOut) ||
        type == typeof(CubicEaseIn) || type == typeof(CubicEaseOut) || type == typeof(CubicEaseInOut) ||
        type == typeof(ElasticEaseIn) || type == typeof(ElasticEaseOut) || type == typeof(ElasticEaseInOut) ||
        type == typeof(ExponentialEaseIn) || type == typeof(ExponentialEaseOut) || type == typeof(ExponentialEaseInOut) ||
        type == typeof(QuadraticEaseIn) || type == typeof(QuadraticEaseOut) || type == typeof(QuadraticEaseInOut) ||
        type == typeof(QuarticEaseIn) || type == typeof(QuarticEaseOut) || type == typeof(QuarticEaseInOut) ||
        type == typeof(QuinticEaseIn) || type == typeof(QuinticEaseOut) || type == typeof(QuinticEaseInOut) ||
        type == typeof(SineEaseIn) || type == typeof(SineEaseOut) || type == typeof(SineEaseInOut);
}
