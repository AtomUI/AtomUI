namespace AtomUI.Theme;

/// <summary>
/// The appearance a user asks for: a fixed light or dark one, or the platform's.
/// </summary>
public enum ThemePreference : byte
{
    /// <summary>
    /// Always the light appearance, whatever the platform uses.
    /// </summary>
    Light,
    /// <summary>
    /// Always the dark appearance, whatever the platform uses.
    /// </summary>
    Dark,
    /// <summary>
    /// The appearance of the platform, including its later changes.
    /// </summary>
    System
}
