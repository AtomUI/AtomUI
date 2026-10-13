using AtomUI.Theme.Schema;
using AtomUI.Theme.Compilation;

namespace AtomUI.Theme.Definitions;

internal sealed class ThemeDefinitionLoadCache : IDisposable
{
    private const int MaxEntriesPerKind = 256;
    private const long MaxBytesPerKind = 32L * 1024 * 1024;
    private readonly BoundedLruCache<ThemeSourceCacheKey, ThemeSourceReadCacheEntry> _reads = new(
        MaxEntriesPerKind, MaxBytesPerKind, EstimateRead,
        static key => EstimateStrings(key.SourceIdentity, key.ContentDigest));
    private readonly BoundedLruCache<ThemeBindingCacheKey, ThemeBindingCacheEntry> _bindings = new(
        MaxEntriesPerKind, MaxBytesPerKind,
        static entry => ThemeRetainedBytesEstimator.Add(
            ThemeRetainedBytesEstimator.EstimateDefinition(entry.Definition), EstimateDiagnostics(entry.Diagnostics)),
        static key => ThemeRetainedBytesEstimator.Add(64, EstimateStrings(key.SourceIdentity, key.ContentDigest)));

    internal bool TryGetRead(ThemeSourceCacheKey key, out ThemeSourceReadCacheEntry? entry) =>
        _reads.TryGetValue(key, out entry);

    internal void StoreRead(ThemeSourceCacheKey key, ThemeSourceReadCacheEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        _reads.StoreValue(key, entry);
    }

    internal bool TryGetBinding(ThemeBindingCacheKey key, out ThemeBindingCacheEntry? entry) =>
        _bindings.TryGetValue(key, out entry);

    internal void StoreBinding(ThemeBindingCacheKey key, ThemeBindingCacheEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        _bindings.StoreValue(key, entry);
    }

    public void Dispose()
    {
        try
        {
            _reads.Dispose();
        }
        finally
        {
            _bindings.Dispose();
        }
    }

    private static long EstimateRead(ThemeSourceReadCacheEntry entry)
    {
        var document = entry.Document;
        var bytes = ThemeRetainedBytesEstimator.Add(192, EstimateStrings(document.Id, document.Name));
        bytes = ThemeRetainedBytesEstimator.Add(bytes, EstimateLocation(document.Location));
        bytes = ThemeRetainedBytesEstimator.Add(bytes, EstimateTokens(document.Tokens));
        bytes = ThemeRetainedBytesEstimator.Add(bytes, EstimateAlgorithms(document.Algorithms));
        foreach (var control in document.Controls)
        {
            bytes = ThemeRetainedBytesEstimator.Add(bytes, 128);
            bytes = ThemeRetainedBytesEstimator.Add(bytes, EstimateStrings(control.Identity.Catalog, control.Identity.Id));
            bytes = ThemeRetainedBytesEstimator.Add(bytes, EstimateLocation(control.Location));
            bytes = ThemeRetainedBytesEstimator.Add(bytes, EstimateTokens(control.Tokens));
            bytes = ThemeRetainedBytesEstimator.Add(bytes, EstimateAlgorithms(control.Algorithms));
        }
        return ThemeRetainedBytesEstimator.Add(bytes, EstimateDiagnostics(entry.Diagnostics));
    }

    private static long EstimateTokens(IReadOnlyList<ThemeTokenDocument> tokens)
    {
        var bytes = 64L;
        foreach (var token in tokens)
        {
            bytes = ThemeRetainedBytesEstimator.Add(bytes, 64);
            bytes = ThemeRetainedBytesEstimator.Add(bytes, EstimateStrings(token.Name, token.Value));
            bytes = ThemeRetainedBytesEstimator.Add(bytes, EstimateLocation(token.Location));
        }
        return bytes;
    }

    private static long EstimateAlgorithms(IReadOnlyList<ThemeAlgorithmDocument> algorithms)
    {
        var bytes = 64L;
        foreach (var algorithm in algorithms)
        {
            bytes = ThemeRetainedBytesEstimator.Add(bytes, 64);
            bytes = ThemeRetainedBytesEstimator.Add(bytes, EstimateLocation(algorithm.Location));
        }
        return bytes;
    }

    private static long EstimateDiagnostics(IReadOnlyList<ThemeDiagnostic> diagnostics)
    {
        var bytes = 64L;
        foreach (var diagnostic in diagnostics)
        {
            bytes = ThemeRetainedBytesEstimator.Add(bytes, 64);
            bytes = ThemeRetainedBytesEstimator.Add(bytes,
                EstimateStrings(diagnostic.Code, diagnostic.Source, diagnostic.Path, diagnostic.Message));
        }
        return bytes;
    }

    private static long EstimateLocation(ThemeSourceLocation location) =>
        ThemeRetainedBytesEstimator.Add(48, EstimateStrings(location.Source, location.Path));

    private static long EstimateStrings(params string[] values)
    {
        var bytes = 0L;
        foreach (var value in values)
        {
            bytes = ThemeRetainedBytesEstimator.Add(bytes, ThemeRetainedBytesEstimator.EstimateValue(value));
        }
        return bytes;
    }
}

internal readonly record struct ThemeSourceCacheKey(
    string SourceIdentity,
    string ContentDigest);

internal readonly record struct ThemeBindingCacheKey(
    string SourceIdentity,
    string ContentDigest,
    ThemeSchemaRevision RegistryRevision,
    object RegistryIdentity);

internal sealed record ThemeSourceReadCacheEntry(
    ThemeDocument Document,
    IReadOnlyList<ThemeDiagnostic> Diagnostics);

internal sealed record ThemeBindingCacheEntry(
    PreparedThemeDefinition Definition,
    IReadOnlyList<ThemeDiagnostic> Diagnostics);
