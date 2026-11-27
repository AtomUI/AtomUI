namespace AtomUI.Theme.Languages;

/// <summary>
/// A density step. The core defines <see cref="Default" /> and <see cref="Compact" />; a language may declare more.
/// </summary>
public readonly record struct DesignDensity(string Id)
{
    /// <summary>
    /// The density a variant resolves to when none is set; a language that does not declare it uses its first
    /// declared density instead.
    /// </summary>
    public static DesignDensity Default { get; } = new("Default");

    /// <summary>
    /// The compact step; a language that declares it derives smaller sizes for it.
    /// </summary>
    public static DesignDensity Compact { get; } = new("Compact");

    /// <summary>
    /// Returns the density identity.
    /// </summary>
    public override string ToString() => Id;
}
