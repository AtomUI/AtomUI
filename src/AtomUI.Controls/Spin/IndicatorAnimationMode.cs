namespace AtomUI.Controls.Commons;

/// <summary>
/// Selects which animation <see cref="AbstractSpinIndicator" /> plays.
/// </summary>
public enum IndicatorAnimationMode
{
    /// <summary>
    /// The base class runs the built-in animations: the rotation of the shown indicator and the fading of the dots of
    /// the built-in one. This is the default.
    /// </summary>
    Rotate,

    /// <summary>
    /// A derived control supplies the animation by overriding <c>StartCustomAnimation</c> and
    /// <c>StopCustomAnimation</c>; the base class still decides when it starts and stops.
    /// </summary>
    Custom
}
