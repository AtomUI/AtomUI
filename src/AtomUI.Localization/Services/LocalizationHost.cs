namespace AtomUI.Localization;

internal sealed class LocalizationHost : IDisposable
{
    private int _disposed;

    internal LocalizationHost(
        LanguageCatalogRegistry registry,
        IReadOnlyDictionary<LanguageTag, LanguageSnapshot> snapshots,
        LanguageManager languageManager,
        Localizer localizer,
        LanguageResourceProvider resourceProvider)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(snapshots);
        LanguageManager = languageManager ?? throw new ArgumentNullException(nameof(languageManager));
        Localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        ResourceProvider = resourceProvider ?? throw new ArgumentNullException(nameof(resourceProvider));
    }

    internal LanguageCatalogRegistry Registry => LanguageManager.Registry;

    internal IReadOnlyDictionary<LanguageTag, LanguageSnapshot> Snapshots => LanguageManager.Snapshots;

    internal LanguageManager LanguageManager { get; }

    internal Localizer Localizer { get; }

    internal LanguageResourceProvider ResourceProvider { get; }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 0)
        {
            LanguageManager.Dispose();
        }
    }
}
