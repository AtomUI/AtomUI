namespace AtomUI.Theme.DesignTokens;

/// <summary>
/// The role of a token in the derivation from design input to consumed value.
/// </summary>
public enum DesignTokenKind
{
    /// <summary>
    /// A design input. Design language rules derive Map and Alias tokens from seeds and never write a seed.
    /// </summary>
    Seed,
    /// <summary>
    /// A token derived from seeds, typically one step of a gradient such as a color palette step or a size step.
    /// </summary>
    Map,
    /// <summary>
    /// A token derived from Map tokens for one purpose of use, such as a content padding or a margin.
    /// </summary>
    Alias
}