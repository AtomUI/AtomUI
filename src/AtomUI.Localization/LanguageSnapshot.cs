using System.Text;

namespace AtomUI.Localization;

public sealed class LanguageSnapshot
{
    private readonly LanguageSnapshotEntry[][] _entries;
    private readonly object _entryLock = new();

    internal LanguageSnapshot(
        LanguageTag requestedLanguage,
        LanguageSnapshotEntry[][] entries,
        LanguageCatalogRegistry? registry = null,
        IReadOnlyList<LanguageTag>? fallbackCandidates = null)
    {
        RequestedLanguage = requestedLanguage;
        _entries = entries;
        Registry = registry;
        FallbackCandidates = fallbackCandidates;
    }

    public LanguageTag RequestedLanguage { get; }
    internal LanguageCatalogRegistry? Registry { get; }
    internal IReadOnlyList<LanguageTag>? FallbackCandidates { get; }
    internal LanguageSnapshotEntry[]? GetMaterializedCatalogEntries(int slot) => Volatile.Read(ref _entries[slot]);
    internal LanguageSnapshotEntry[] GetCatalogEntries(int catalogSlot)
    {
        if ((uint)catalogSlot >= (uint)_entries.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(catalogSlot));
        }
        var entries = Volatile.Read(ref _entries[catalogSlot]);
        if (entries is not null)
        {
            return entries;
        }
        lock (_entryLock)
        {
            entries = _entries[catalogSlot];
            if (entries is null)
            {
                entries = LanguageSnapshotBuilder.BuildCatalog(Registry!, catalogSlot, RequestedLanguage, FallbackCandidates!);
                Volatile.Write(ref _entries[catalogSlot], entries);
            }
            return entries;
        }
    }

    internal LanguageSnapshotEntry GetEntry(int catalogSlot, int unitSlot)
    {
        if ((uint)catalogSlot >= (uint)_entries.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(catalogSlot));
        }
        var entries = GetCatalogEntries(catalogSlot);
        if ((uint)unitSlot >= (uint)entries.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(unitSlot));
        }

        return entries[unitSlot];
    }
}

internal readonly struct LanguageSnapshotEntry
{
    internal LanguageSnapshotEntry(
        string text,
        LanguageTag resolvedLanguage,
        CompositeFormat? compositeFormat)
    {
        Text = text;
        ResolvedLanguage = resolvedLanguage;
        CompositeFormat = compositeFormat;
    }

    internal string Text { get; }

    internal LanguageTag ResolvedLanguage { get; }

    internal CompositeFormat? CompositeFormat { get; }
}
