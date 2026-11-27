using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Utils;

/// <summary>
/// Extension methods that bind motion-aware controls to the Token that switches motion on or off.
/// </summary>
public static class MotionAwareControlExtensions
{
    /// <summary>
    /// Binds the <see cref="MotionAwareControlProperty.IsMotionEnabledProperty" /> property of the control to the
    /// shared <see cref="SharedTokenKind.EnableMotion" /> Token, so that the property follows the Token value.
    /// </summary>
    /// <param name="motionAwareControl">
    /// The control to bind. A <see cref="Control" /> is bound through the Token resource key of its own type; any
    /// other <see cref="StyledElement" /> is bound through the shared Token itself.
    /// </param>
    /// <returns>
    /// A handle that removes the binding when it is disposed, or <see langword="null" /> when
    /// <paramref name="motionAwareControl" /> is neither a <see cref="Control" /> nor a <see cref="StyledElement" />.
    /// </returns>
    /// <remarks>
    /// The binding is made at <c>BindingPriority.Template</c> priority, so a value set locally on the control wins
    /// over the Token.
    /// </remarks>
    public static IDisposable? ConfigureMotionBindingStyle(this IMotionAwareControl motionAwareControl)
    {
        if (motionAwareControl is Control control)
        {
            return TokenResourceBinder.CreateControlTokenBinding(
                control,
                MotionAwareControlProperty.IsMotionEnabledProperty,
                SharedTokenKind.EnableMotion);
        }

        return motionAwareControl is StyledElement styledElement
            ? TokenResourceBinder.CreateGlobalTokenBinding(
                styledElement,
                MotionAwareControlProperty.IsMotionEnabledProperty,
                SharedTokenKind.EnableMotion)
            : null;
    }
}
