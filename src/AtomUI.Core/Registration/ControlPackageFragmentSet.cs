using System.ComponentModel;

namespace AtomUI.Registration;

/// <summary>net8 conditional-backend bridge; preserves the selected collector ABI without eager descriptor execution.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ControlPackageFragmentSet
{
    private readonly Dictionary<string, ControlRegistrationFragmentAttribute> _byKey = new(StringComparer.Ordinal);

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

    public bool Contains(string key) => _byKey.ContainsKey(key);
    public ControlRegistrationFragmentAttribute? Resolve(string key) => _byKey.GetValueOrDefault(key);
}
