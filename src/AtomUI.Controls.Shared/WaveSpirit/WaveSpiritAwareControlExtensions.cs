using System.Reactive.Disposables;
using AtomUI.Data;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Controls;

/// <summary>
/// Provides the token bindings for controls that implement <see cref="IWaveSpiritAwareControl" />.
/// </summary>
public static class WaveSpiritAwareControlExtensions
{
    /// <summary>
    /// Binds <c>IsMotionEnabled</c> and <c>IsWaveSpiritEnabled</c> of a control to the <c>EnableMotion</c> and
    /// <c>EnableWaveSpirit</c> shared tokens, so that both follow resource changes.
    /// </summary>
    /// <param name="waveSpiritAwareControl">
    /// The control to bind.
    /// </param>
    /// <returns>
    /// A disposable that removes both bindings, or <see langword="null" /> when the control is neither a
    /// <see cref="Control" /> nor a <see cref="StyledElement" />.
    /// </returns>
    /// <remarks>
    /// The bindings use template priority, so a local value on the control wins and a style setter loses.
    /// </remarks>
    public static IDisposable? ConfigureWaveSpiritBindingStyle(this IWaveSpiritAwareControl waveSpiritAwareControl)
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
                    WaveSpiritAwareControlProperty.IsWaveSpiritEnabledProperty,
                    SharedTokenKind.EnableWaveSpirit)
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
                    WaveSpiritAwareControlProperty.IsWaveSpiritEnabledProperty,
                    SharedTokenKind.EnableWaveSpirit)
            }
            : null;
    }
}
