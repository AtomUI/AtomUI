using Avalonia;
using Avalonia.Media;

namespace AtomUI.Controls;

/// <summary>
/// A control that can play a click wave: a ripple that expands from the outline or the center of the control and
/// fades out when the user clicks or toggles it.
/// </summary>
/// <remarks>
/// A control implements the switch by adding itself as owner of
/// <see cref="WaveSpiritAwareControlProperty.IsWaveSpiritEnabledProperty" /> and exposing a CLR wrapper. Its theme
/// forwards the value to the <c>WaveSpiritDecorator</c> that draws the wave.
/// </remarks>
public interface IWaveSpiritAwareControl : IMotionAwareControl
{
    /// <summary>
    /// Gets whether the click wave plays when the control is clicked or toggled.
    /// </summary>
    /// <remarks>
    /// The wave plays only while <see cref="IMotionAwareControl.IsMotionEnabled" /> is also <see langword="true" />.
    /// </remarks>
    bool IsWaveSpiritEnabled { get; }
}

/// <summary>
/// Holds the styled properties that are shared by controls which implement
/// <see cref="IWaveSpiritAwareControl" />. A control adds itself as owner of a property and exposes the value
/// through a CLR wrapper.
/// </summary>
public abstract class WaveSpiritAwareControlProperty : MotionAwareControlProperty
{
    /// <summary>
    /// The name used to register <see cref="IsWaveSpiritEnabledProperty" />.
    /// </summary>
    public const string IsWaveSpiritEnabledPropertyName = "IsWaveSpiritEnabled";
    /// <summary>
    /// The name used to register <see cref="WaveSpiritBrushProperty" />.
    /// </summary>
    public const string WaveSpiritBrushPropertyName = "WaveSpiritBrush";
    /// <summary>
    /// The name used to register <see cref="WaveSpiritTypeProperty" />.
    /// </summary>
    public const string WaveSpiritTypePropertyName = "WaveSpiritType";

    /// <summary>
    /// Identifies the <c>IsWaveSpiritEnabled</c> property: whether the control plays the click wave.
    /// </summary>
    /// <remarks>
    /// The property is registered on <see cref="StyledElement" /> and defaults to <see langword="false" />. A control
    /// adds itself as owner, and its theme forwards the value to the <c>WaveSpiritDecorator</c>, which plays the wave
    /// only while the value is <see langword="true" />. Control themes set the value from the
    /// <c>EnableWaveSpirit</c> shared token.
    /// </remarks>
    public static readonly StyledProperty<bool> IsWaveSpiritEnabledProperty =
        AvaloniaProperty.Register<StyledElement, bool>(IsWaveSpiritEnabledPropertyName);
    
    /// <summary>
    /// Identifies the <c>WaveSpiritBrush</c> property, an optional brush registered on <see cref="StyledElement" />.
    /// </summary>
    /// <remarks>
    /// The property has no wrapper and no built-in control adds itself as owner, so nothing reads its value. The
    /// built-in controls pass the wave brush to the <c>WaveBrush</c> property of their <c>WaveSpiritDecorator</c>.
    /// </remarks>
    public static readonly StyledProperty<IBrush?> WaveSpiritBrushProperty =
        AvaloniaProperty.Register<StyledElement, IBrush?>(WaveSpiritBrushPropertyName);
    
    /// <summary>
    /// Identifies the <c>WaveSpiritType</c> property: the shape of the click wave.
    /// </summary>
    /// <remarks>
    /// The property is registered on <see cref="StyledElement" /> with <see cref="WaveSpiritType.RoundRectWave" /> as
    /// the default. A control adds itself as owner, and its theme passes the value to the <c>WaveType</c> property of
    /// the <c>WaveSpiritDecorator</c>.
    /// </remarks>
    public static readonly StyledProperty<WaveSpiritType> WaveSpiritTypeProperty =
        AvaloniaProperty.Register<StyledElement, WaveSpiritType>(WaveSpiritTypePropertyName);
}