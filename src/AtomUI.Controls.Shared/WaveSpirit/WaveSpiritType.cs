namespace AtomUI.Controls;

/// <summary>
/// The shape of the expanding ripple that a control plays as click feedback when
/// <see cref="IWaveSpiritAwareControl.IsWaveSpiritEnabled" /> is true.
/// </summary>
public enum WaveSpiritType
{
    /// <summary>
    /// A rounded rectangle ring that grows around the outline of the control. Its corner radius starts at the corner
    /// radius of the control.
    /// </summary>
    RoundRectWave,
    /// <summary>
    /// A circle ring that expands from the center of the control.
    /// </summary>
    CircleWave,
    /// <summary>
    /// A pill-shaped ring that grows around the outline of the control. Its corner radius is half the height of the
    /// ring.
    /// </summary>
    PillWave
}