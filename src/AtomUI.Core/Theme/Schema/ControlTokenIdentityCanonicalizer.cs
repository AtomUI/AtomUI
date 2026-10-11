namespace AtomUI.Theme.Schema;

internal static class ControlTokenIdentityCanonicalizer
{
    internal static ControlTokenIdentity Merge(ControlTokenIdentity left, ControlTokenIdentity right)
    {
        if (left != right)
        {
            throw new ArgumentException("Only equal Token identities can be canonicalized.");
        }
        if (left.OwnerType is { } owner && right.OwnerType is { } other && owner != other)
        {
            throw new ThemeSchemaException(
                $"Control Token identity '{left}' has conflicting owners '{owner}' and '{other}'.");
        }
        return left.OwnerType is not null ? left : right;
    }

    internal static ControlTokenIdentity[] Canonicalize(IEnumerable<ControlTokenIdentity> identities)
    {
        var canonical = new Dictionary<ControlTokenIdentity, ControlTokenIdentity>();
        foreach (var identity in identities)
        {
            canonical[identity] = canonical.TryGetValue(identity, out var existing) ? Merge(existing, identity) : identity;
        }
        return canonical.Values.OrderBy(static identity => identity.Catalog, StringComparer.Ordinal)
                               .ThenBy(static identity => identity.Id, StringComparer.Ordinal).ToArray();
    }

    internal static bool Matches(ControlTokenIdentity identity, Type controlType) =>
        identity.OwnerType is null || identity.OwnerType == controlType;
}

// Canonical owner evidence lives in the value because Dictionary replacement retains its original key.
// Enumeration exposes those canonical identities to immutable configuration snapshots.
internal sealed class CanonicalControlTokenMap<T> : IReadOnlyDictionary<ControlTokenIdentity, T>
{
    private readonly Dictionary<ControlTokenIdentity, (ControlTokenIdentity Identity, T Value)> _entries = new();

    internal void Set(ControlTokenIdentity identity, T value)
    {
        if (_entries.TryGetValue(identity, out var previous))
        {
            identity = ControlTokenIdentityCanonicalizer.Merge(previous.Identity, identity);
        }
        _entries[identity] = (identity, value);
    }

    public int Count => _entries.Count;
    public T this[ControlTokenIdentity key] => _entries[key].Value;
    public IEnumerable<ControlTokenIdentity> Keys => _entries.Values.Select(static entry => entry.Identity);
    public IEnumerable<T> Values => _entries.Values.Select(static entry => entry.Value);
    public bool ContainsKey(ControlTokenIdentity key) => _entries.ContainsKey(key);

    public bool TryGetValue(ControlTokenIdentity key, out T value)
    {
        if (_entries.TryGetValue(key, out var entry))
        {
            value = entry.Value;
            return true;
        }
        value = default!;
        return false;
    }

    public IEnumerator<KeyValuePair<ControlTokenIdentity, T>> GetEnumerator()
    {
        foreach (var entry in _entries.Values)
        {
            yield return new KeyValuePair<ControlTokenIdentity, T>(entry.Identity, entry.Value);
        }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
