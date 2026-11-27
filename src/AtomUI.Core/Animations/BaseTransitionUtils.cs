using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;

namespace AtomUI.Animations;

/// <summary>
/// Base class for helpers that create Avalonia property transitions. It holds the factory for transitions with a
/// fixed duration.
/// </summary>
public class BaseTransitionUtils
{
    protected BaseTransitionUtils() {}

    /// <summary>
    /// Creates a transition for a property that runs with a fixed duration and easing.
    /// </summary>
    /// <typeparam name="T">
    /// The kind of transition to create, for example <see cref="DoubleTransition" />.
    /// </typeparam>
    /// <param name="targetProperty">
    /// The property the transition animates.
    /// </param>
    /// <param name="duration">
    /// How long the transition runs. <see langword="null" /> uses 300 milliseconds.
    /// </param>
    /// <param name="easing">
    /// The curve that shapes the progress of the transition. <see langword="null" /> uses a linear easing.
    /// </param>
    /// <returns>
    /// A new transition. It runs once it is added to the <c>Transitions</c> of an element.
    /// </returns>
    public static ITransition CreateTransition<T>(AvaloniaProperty targetProperty,
                                                  TimeSpan? duration = null,
                                                  Easing? easing = null)
        where T : TransitionBase, new()
    {
        easing ??= new LinearEasing();
        duration ??= TimeSpan.FromMilliseconds(300);
        var transition = new T
        {
            Property = targetProperty,
            Easing   = easing,
            Duration = duration.Value
        };
        return transition;
    }
}