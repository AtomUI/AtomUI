using System.Collections.ObjectModel;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;

namespace AtomUI.Theme.Algorithms;

/// <summary>
/// Describes the algorithm chain an optional algorithm stage runs in: the appearance the whole chain resolves to
/// and the algorithms of the chain, in order.
/// </summary>
public readonly struct ThemeAlgorithmContext
{
    private readonly IReadOnlyList<ThemeAlgorithm>? _algorithms;
    private readonly IReadOnlyList<NormalizedTokenValue>? _scopeTokens;
    private readonly IReadOnlyList<NormalizedTokenValue>? _controlTokens;

    // The chain is held read-only: a stage must not be able to cast it back to an array and change what later
    // algorithms see. A read-only collection is kept as is; anything else is copied into one.
    internal ThemeAlgorithmContext(
        ThemeAppearance appearance,
        IReadOnlyList<ThemeAlgorithm> algorithms,
        IReadOnlyList<NormalizedTokenValue>? scopeTokens = null,
        IReadOnlyList<NormalizedTokenValue>? controlTokens = null)
    {
        ArgumentNullException.ThrowIfNull(algorithms);
        Appearance  = appearance;
        _algorithms = algorithms as ReadOnlyCollection<ThemeAlgorithm> ?? Array.AsReadOnly(algorithms.ToArray());
        _scopeTokens = scopeTokens;
        _controlTokens = controlTokens;
    }

    /// <summary>
    /// The appearance resolved from the whole effective chain. A stage implementation must handle both
    /// appearances, because a chain such as <c>[Vendor:Light, AtomUI:Dark]</c> runs it under the dark one.
    /// </summary>
    public ThemeAppearance Appearance { get; }

    /// <summary>
    /// The effective algorithm chain, in order. The list is read-only.
    /// </summary>
    public IReadOnlyList<ThemeAlgorithm> Algorithms => _algorithms ?? Array.Empty<ThemeAlgorithm>();

    // A compiled seed may have the same value whether it was supplied or derived.
    internal bool HasConfiguredValue(SharedTokenKind kind) =>
        ContainsConfiguredValue(_controlTokens, (int)kind) || ContainsConfiguredValue(_scopeTokens, (int)kind);

    private static bool ContainsConfiguredValue(IReadOnlyList<NormalizedTokenValue>? values, int slot)
    {
        if (values is null)
        {
            return false;
        }

        for (var index = 0; index < values.Count; index++)
        {
            var value = values[index];
            if (value.Descriptor.Slot == slot && value.Value is not null)
            {
                return true;
            }
        }
        return false;
    }
}
