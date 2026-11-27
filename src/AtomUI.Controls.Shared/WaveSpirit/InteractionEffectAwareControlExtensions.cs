using System.Reactive.Disposables;
using AtomUI.Data;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Controls;

/// <summary>
/// Helper extensions for binding interaction-effect settings on controls.
/// </summary>
public static class InteractionEffectAwareControlExtensions
{
    /// <summary>
    /// Binds motion and interaction-effect enabled flags to their corresponding shared design tokens.
    /// </summary>
    public static IDisposable? ConfigureInteractionEffectBindingStyle(this IInteractionEffectAwareControl waveSpiritAwareControl)
    {
        if (waveSpiritAwareControl is Control control)
        {
            return new CompositeDisposable
            {
                TokenResourceBinder.CreateControlTokenBinding(
                    control,
                    MotionAwareControlProperty.IsMotionEnabledProperty,
                    SharedTokenKind.EnableMotion),
                TokenResourceBinder.CreateControlTokenBinding(
                    control,
                    InteractionEffectAwareControlProperty.IsInteractionEffectEnabledProperty,
                    SharedTokenKind.EnableInteractionEffect)
            };
        }

        return waveSpiritAwareControl is StyledElement styledElement
            ? new CompositeDisposable
            {
                TokenResourceBinder.CreateGlobalTokenBinding(
                    styledElement,
                    MotionAwareControlProperty.IsMotionEnabledProperty,
                    SharedTokenKind.EnableMotion),
                TokenResourceBinder.CreateGlobalTokenBinding(
                    styledElement,
                    InteractionEffectAwareControlProperty.IsInteractionEffectEnabledProperty,
                    SharedTokenKind.EnableInteractionEffect)
            }
            : null;
    }
}
