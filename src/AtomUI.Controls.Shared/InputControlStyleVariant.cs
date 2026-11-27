namespace AtomUI.Controls;

/// <summary>
/// The frame style of an input control: which edges carry a border and whether the background is filled.
/// </summary>
public enum InputControlStyleVariant
{
    /// <summary>
    /// A border on all four edges. This is the default.
    /// </summary>
    Outlined,
    /// <summary>
    /// A filled background without a visible border. A transparent border keeps the control as high as
    /// <see cref="Outlined" />.
    /// </summary>
    Filled,
    /// <summary>
    /// No border is drawn.
    /// </summary>
    Borderless,
    /// <summary>
    /// A border on the bottom edge only.
    /// </summary>
    Underlined
}
