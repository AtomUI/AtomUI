namespace AtomUI;

/// <summary>
/// Startup progress reported by the application. A <see langword="null" /> <c>Message</c> leaves the current
/// message unchanged. A <see langword="null" /> <c>Fraction</c> keeps the progress indeterminate; otherwise it
/// is a value from 0 to 1.
/// </summary>
public readonly record struct StartupProgress(string? Message, double? Fraction = null);
