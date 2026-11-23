namespace AtomUI.Theme.Languages;

/// <summary>
/// The device idiom the interface runs as. The core defines <see cref="Desktop"/> and <see cref="Mobile"/>; the
/// interface root decides it, not a theme.
/// </summary>
public readonly record struct DesignIdiom(string Id)
{
    public static DesignIdiom Desktop { get; } = new("Desktop");

    public static DesignIdiom Mobile { get; } = new("Mobile");

    public override string ToString() => Id;
}
