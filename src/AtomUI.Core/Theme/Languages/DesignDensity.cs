namespace AtomUI.Theme.Languages;

/// <summary>
/// A density step. The core defines <see cref="Default"/> and <see cref="Compact"/>; a language may declare more.
/// </summary>
public readonly record struct DesignDensity(string Id)
{
    public static DesignDensity Default { get; } = new("Default");

    public static DesignDensity Compact { get; } = new("Compact");

    public override string ToString() => Id;
}
