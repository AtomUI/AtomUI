using AtomUI.Theme.Configuration;
using AtomUI.Theme.Schema;

namespace AtomUI.Theme.Compilation;

internal sealed class ControlCompilationCache : IDisposable
{
    private readonly BoundedLruCache<ControlCompilationCacheKey, ControlThemeSnapshot> _entries;
    private Action? _onMaterialized;

    internal ControlCompilationCache(ThemeCacheOptions options, Action? onMaterialized = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        _onMaterialized = onMaterialized;
        _entries = new BoundedLruCache<ControlCompilationCacheKey, ControlThemeSnapshot>(
            options.ControlEntryLimit,
            options.ControlRetainedBytesLimit,
            static snapshot => snapshot.EstimatedRetainedBytes,
            static key => key.EstimatedRetainedBytes,
            options.ProductionEntryLimit,
            options.ProductionRetainedBytesLimit);
    }

    internal int Count => _entries.Count;
    internal long RetainedBytes => _entries.RetainedBytes;
    internal int InFlightCount => _entries.InFlightCount;

    internal void NotifyMaterialized() => _onMaterialized?.Invoke();

    public void Dispose()
    {
        _onMaterialized = null;
        _entries.Dispose();
    }

    internal ControlThemeSnapshot GetOrCompile(
        GlobalCompilationCacheKey globalKey,
        ControlTokenDescriptor descriptor,
        NormalizedControlThemeConfig? controlConfig,
        ThemeAppearance appearance,
        Func<ControlThemeSnapshot> factory,
        long sourceRetainedBytes = 0)
    {
        ArgumentNullException.ThrowIfNull(globalKey);
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentOutOfRangeException.ThrowIfNegative(sourceRetainedBytes);

        var key = ControlCompilationCacheKey.Create(
            globalKey,
            descriptor,
            controlConfig,
            appearance);
        return _entries.GetOrCreateAsync(
                           key,
                           _ => ValueTask.FromResult(factory()),
                           CancellationToken.None,
                           allowUncachedAfterDispose: true,
                           productionInputRetainedBytesEstimator: () => ThemeRetainedBytesEstimator.Add(
                               sourceRetainedBytes, key.EstimatedInputRetainedBytes))
                       .GetAwaiter()
                       .GetResult();
    }
}
