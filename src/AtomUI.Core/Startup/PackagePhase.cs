namespace AtomUI;

/// <summary>
/// Diagnostic phases of a control package, kept apart from the global <see cref="StartupPhase" /> values.
/// </summary>
internal enum PackagePhase
{
    Enable,
    Commit,
    FirstUse
}
