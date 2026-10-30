using Avalonia.Animation.Easings;

namespace AtomUI.Animations;

internal static class MotionCurves
{
    internal static readonly Easing Linear = new LinearEasing();
    internal static readonly Easing OutCirc = new SplineEasing(0.08, 0.82, 0.17, 1);
    internal static readonly Easing InOutCirc = new SplineEasing(0.78, 0.14, 0.15, 0.86);
    internal static readonly Easing OutQuint = new SplineEasing(0.23, 1, 0.32, 1);
    internal static readonly Easing InQuint = new SplineEasing(0.755, 0.05, 0.855, 0.06);
    internal static readonly Easing InOut = new SplineEasing(0.645, 0.045, 0.355, 1);
    internal static readonly Easing Ease = new SplineEasing(0.25, 0.1, 0.25, 1);
}
