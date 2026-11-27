namespace AtomUI.Theme;

/// <summary>
/// The light or dark appearance that a theme is shown in.
/// </summary>
/// <remarks>
/// When a theme is published, its appearance selects the matching Avalonia theme variant. A selection that follows
/// the system uses the appearance that the system currently reports.
/// </remarks>
public enum ThemeAppearance : byte
{
    /// <summary>
    /// The light appearance.
    /// </summary>
    Light,
    /// <summary>
    /// The dark appearance.
    /// </summary>
    Dark
}
