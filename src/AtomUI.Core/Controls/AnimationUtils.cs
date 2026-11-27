using System.Diagnostics;
using AtomUI.Animations;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;

namespace AtomUI.Controls;

/// <summary>
/// Creates Avalonia property transitions. Besides the fixed-duration factory of
/// <see cref="BaseTransitionUtils" />, it offers one that takes its duration from a shared motion token.
/// </summary>
public sealed class TransitionUtils : BaseTransitionUtils
{
    /// <summary>
    /// Creates a transition for a property whose duration is read from a shared motion token.
    /// </summary>
    /// <typeparam name="T">
    /// The kind of transition to create, for example <see cref="DoubleTransition" />.
    /// </typeparam>
    /// <param name="targetProperty">
    /// The property the transition animates.
    /// </param>
    /// <param name="durationResourceKey">
    /// The token that holds the duration. It is looked up once, on the application and for the theme variant
    /// current at the call. <see langword="null" /> uses <see cref="SharedTokenKind.MotionDurationMid" />.
    /// When the token is not found or holds no <see cref="TimeSpan" />, the transition keeps the duration it was
    /// created with.
    /// </param>
    /// <param name="easing">
    /// The curve that shapes the progress of the transition. <see langword="null" /> uses a linear easing.
    /// </param>
    /// <param name="delay">
    /// How long the transition waits before it starts. <see langword="null" /> starts it without a delay.
    /// </param>
    /// <returns>
    /// A new transition. It animates changes of the property once it is added to the <c>Transitions</c> of an element.
    /// </returns>
    public static ITransition CreateTransition<T>(AvaloniaProperty targetProperty,
                                                  SharedTokenKind? durationResourceKey = null,
                                                  Easing? easing = null,
                                                  TimeSpan? delay = null)
        where T : TransitionBase, new()
    {
        easing              ??= new LinearEasing();
        durationResourceKey ??= SharedTokenKind.MotionDurationMid;
        var transition = new T
        {
            Property = targetProperty,
            Easing   = easing,
            Delay    = delay ??  TimeSpan.Zero,
        };
        var application = Application.Current;
        Debug.Assert(application != null);
        var themeVariant = application.ActualThemeVariant;
        if (application.TryGetResource(durationResourceKey, themeVariant, out var value))
        {
            if (value is TimeSpan duration)
            {
                transition.Duration = duration;
            }
        }
        return transition;
    }
}