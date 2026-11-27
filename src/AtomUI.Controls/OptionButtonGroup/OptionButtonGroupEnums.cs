namespace AtomUI.Controls;

/// <summary>
/// Visual variant of the options in an option button group.
/// </summary>
public enum OptionButtonStyle
{
    /// <summary>
    /// Options are transparent and divided by separators; the checked option is marked by a border that
    /// the group draws and by its text color.
    /// </summary>
    Outline,
    /// <summary>
    /// The checked option is marked by its own filled background, and the separators next to it are not
    /// drawn.
    /// </summary>
    Solid
}

/// <summary>
/// Position of an option within its group. The group assigns it to each option button, and it decides
/// which corners of the option keep the corner radius.
/// </summary>
public enum OptionButtonPositionTrait
{
    /// <summary>
    /// The first of at least two options; only the corners at the start of the group are rounded.
    /// </summary>
    First,
    /// <summary>
    /// The last of at least two options; only the corners at the end of the group are rounded.
    /// </summary>
    Last,
    /// <summary>
    /// An option that is neither first nor last; all of its corners are square.
    /// </summary>
    Middle,
    /// <summary>
    /// The only option of the group; all of its corners are rounded.
    /// </summary>
    OnlyOne
}