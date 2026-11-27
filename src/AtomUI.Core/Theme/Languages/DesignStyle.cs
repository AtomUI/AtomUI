namespace AtomUI.Theme.Languages;

/// <summary>
/// A style of a design language: an appearance family inside it (Shadcn "Nova", PrimeNG "Aura").
/// </summary>
public readonly record struct DesignStyle(string Id)
{
    /// <summary>
    /// Returns the style identity.
    /// </summary>
    public override string ToString() => Id;
}
