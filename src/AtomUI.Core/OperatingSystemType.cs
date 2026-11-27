namespace AtomUI;

/// <summary>
/// The operating system that a window or title bar runs on. Window and title bar themes select styles by
/// this value, for example with the selector <c>^[OsType=Linux]</c>.
/// </summary>
public enum OsType
{
    /// <summary>
    /// Detected with <c>OperatingSystem.IsWindows()</c>.
    /// </summary>
    Windows,
    /// <summary>
    /// Detected with <c>OperatingSystem.IsMacOS()</c> or <c>OperatingSystem.IsMacCatalyst()</c>.
    /// </summary>
    macOS,
    /// <summary>
    /// Detected with <c>OperatingSystem.IsLinux()</c>.
    /// </summary>
    Linux,
    /// <summary>
    /// Android. The platform detection in this library never assigns this value.
    /// </summary>
    Android,
    /// <summary>
    /// iOS. The platform detection in this library never assigns this value.
    /// </summary>
    iOS,
    /// <summary>
    /// FreeBSD. The platform detection in this library never assigns this value; on FreeBSD a control reports
    /// <see cref="Unknown" /> instead.
    /// </summary>
    FreeBSD,
    /// <summary>
    /// The system is not Windows, macOS or Linux, or no value has been assigned to the control yet.
    /// This is the default of the <c>OsType</c> properties.
    /// </summary>
    Unknown
}