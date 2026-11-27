namespace AtomUI.Theme.Languages;

/// <summary>
/// The device idiom the interface runs as. The core defines <see cref="Desktop" /> and <see cref="Mobile" />; the
/// interface root decides it, not a theme.
/// </summary>
public readonly record struct DesignIdiom(string Id)
{
    /// <summary>
    /// The idiom of desktop applications; the one a variant resolves to when none is set.
    /// </summary>
    public static DesignIdiom Desktop { get; } = new("Desktop");

    /// <summary>
    /// The idiom of mobile applications. A language must declare it, or a variant with it is unsupported.
    /// </summary>
    public static DesignIdiom Mobile { get; } = new("Mobile");

    /// <summary>
    /// Returns the idiom identity.
    /// </summary>
    public override string ToString() => Id;
}
