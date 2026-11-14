namespace AtomUI.Theme.Algorithms;

/// <summary>
/// Describes the algorithm chain an optional algorithm stage runs in: the appearance the whole chain resolves to
/// and the algorithms of the chain, in order.
/// </summary>
public readonly struct ThemeAlgorithmContext
{
    private readonly IReadOnlyList<ThemeAlgorithm>? _algorithms;

    internal ThemeAlgorithmContext(ThemeAppearance appearance, IReadOnlyList<ThemeAlgorithm> algorithms)
    {
        ArgumentNullException.ThrowIfNull(algorithms);
        Appearance  = appearance;
        _algorithms = algorithms;
    }

    /// <summary>
    /// The appearance resolved from the whole effective chain. A stage implementation must handle both
    /// appearances, because a chain such as <c>[Vendor:Light, AtomUI:Dark]</c> runs it under the dark one.
    /// </summary>
    public ThemeAppearance Appearance { get; }

    /// <summary>
    /// The effective algorithm chain, in order.
    /// </summary>
    public IReadOnlyList<ThemeAlgorithm> Algorithms => _algorithms ?? Array.Empty<ThemeAlgorithm>();
}
