namespace AtomUI.Controls;

/// <summary>
/// The responsive breakpoints, ordered from the narrowest to the widest.
/// </summary>
/// <remarks>
/// The value of a member is the minimum width of its breakpoint in device-independent pixels in the built-in
/// design languages. Code only uses the values to order the breakpoints and to compare them with the
/// <c>&gt;=</c> operator; the widths at which a host switches breakpoint come from the screen size tokens of
/// the theme.
/// </remarks>
public enum MediaBreakPoint
{
    /// <summary>
    /// The breakpoint for widths below <see cref="Small" />, written <c>xs</c> in responsive values.
    /// </summary>
    ExtraSmall = 0,
    /// <summary>
    /// The breakpoint for widths below <see cref="Medium" />, written <c>sm</c> in responsive values.
    /// </summary>
    Small = 576,
    /// <summary>
    /// The breakpoint for widths below <see cref="Large" />, written <c>md</c> in responsive values.
    /// </summary>
    Medium = 768,
    /// <summary>
    /// The breakpoint for widths below <see cref="ExtraLarge" />, written <c>lg</c> in responsive values. It is the
    /// default value of <see cref="MediaBreakAwareControlProperty.MediaBreakPointProperty" />.
    /// </summary>
    Large = 992,
    /// <summary>
    /// The breakpoint for widths below <see cref="ExtraExtraLarge" />, written <c>xl</c> in responsive values.
    /// </summary>
    ExtraLarge = 1200,
    /// <summary>
    /// The breakpoint for widths below <see cref="ExtraExtraExtraLarge" />, written <c>xxl</c> in responsive values.
    /// </summary>
    ExtraExtraLarge = 1600,
    /// <summary>
    /// The widest breakpoint, written <c>xxxl</c> in responsive values.
    /// </summary>
    ExtraExtraExtraLarge = 1920,
}
