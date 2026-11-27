using System.ComponentModel;

namespace AtomUI.Registration;

/// <summary>
/// net8 conditional-backend bridge; preserves the selected collector ABI without eager descriptor execution.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ControlPackageFragmentSet
{
    private readonly Dictionary<string, ControlRegistrationFragmentAttribute> _byKey = new(StringComparer.Ordinal);

    /// <summary>
    /// Indexes the fragments that <paramref name="collect" /> declares, without running them, under lookup
    /// keys. Each pair in <paramref name="keys" /> maps a lookup key to a fragment ID; a key whose fragment
    /// was not declared is left out.
    /// </summary>
    public ControlPackageFragmentSet(Action<ControlPackageRegistrationBuilder> collect,
        IReadOnlyList<KeyValuePair<string, string>> keys)
    {
        ArgumentNullException.ThrowIfNull(collect);
        ArgumentNullException.ThrowIfNull(keys);
        var builder = new ControlPackageRegistrationBuilder(deferFragments: true);
        collect(builder);
        var byId = builder.ExtractFragments().ToDictionary(fragment => fragment.FragmentId, StringComparer.Ordinal);
        foreach (var key in keys)
        {
            if (byId.TryGetValue(key.Value, out var fragment))
            {
                _byKey.Add(key.Key, fragment);
            }
        }
    }

    /// <summary>
    /// Returns whether a declared fragment is stored under the lookup <paramref name="key" />.
    /// </summary>
    public bool Contains(string key) => _byKey.ContainsKey(key);
    /// <summary>
    /// Returns the fragment stored under the lookup <paramref name="key" />, or <see langword="null" /> when
    /// no declared fragment is stored under it.
    /// </summary>
    public ControlRegistrationFragmentAttribute? Resolve(string key) => _byKey.GetValueOrDefault(key);
}
