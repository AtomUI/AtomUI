namespace AtomUI.Theme;

/// <summary>
/// The seriousness of a theme diagnostic: an error makes the operation that reported it fail, a warning does not.
/// </summary>
public enum ThemeDiagnosticSeverity : byte
{
    /// <summary>
    /// A problem the operation that found it reports without failing, such as a theme event handler that threw.
    /// </summary>
    Warning,
    /// <summary>
    /// A problem that makes the operation that found it fail or reject its result.
    /// </summary>
    Error
}

/// <summary>
/// One problem that the theme system found while it loaded, compiled, switched to or published a theme.
/// </summary>
/// <param name="Code">
/// Identifies the kind of problem, such as <c>ATMTHM7001</c>.
/// </param>
/// <param name="Severity">
/// Whether the problem makes the operation that found it fail.
/// </param>
/// <param name="Source">
/// The component or location that reported the problem, such as <c>ThemeManager</c> or
/// <c>ThemeScope[...].Resources</c>.
/// </param>
/// <param name="Path">
/// Locates within the source the item that the problem concerns; <c>$</c> stands for the source itself.
/// </param>
/// <param name="Message">
/// The text that describes the problem.
/// </param>
public sealed record ThemeDiagnostic(
    string Code,
    ThemeDiagnosticSeverity Severity,
    string Source,
    string Path,
    string Message);
