namespace AtomUI.Localization;

internal sealed partial class LanguageManager
{
    internal void BeginPackageLoading()
    {
        VerifyAccess();
        ThrowIfDisposed();
        if (_loadingPackages)
        {
            throw new InvalidOperationException("Package loading is already active.");
        }
        _loadingPackages = true;
    }

    internal void EndPackageLoading() => _loadingPackages = false;

    internal PackageUpdate PreparePackages(LocalizationBuilder builder)
    {
        var previous = _context.Capture();
        var modules = new Dictionary<string, string>(_moduleData, StringComparer.Ordinal);
        foreach (var pair in builder.ModuleData)
        {
            if (!modules.TryAdd(pair.Key, pair.Value))
            {
                throw new LanguageCatalogException($"Language module '{pair.Key}' is registered more than once.");
            }
        }
        builder.Close();
        if (builder.Catalogs.Count == 0 && builder.TranslationBundles.Count == 0)
        {
            return new PackageUpdate(this, previous, _snapshots, modules);
        }
        var registry = LanguageCatalogRegistry.Create(
            previous.Registry.Catalogs.Concat(builder.Catalogs).ToArray(),
            Enumerable.Range(0, previous.Registry.Catalogs.Count).SelectMany(previous.Registry.GetBundles)
                .Concat(builder.TranslationBundles).ToArray(), previous.Registry);
        foreach (var definition in SupportedLanguages)
        {
            LanguageSnapshotBuilder.ValidateManualCoverage(registry, definition);
        }
        var snapshots = new Dictionary<LanguageTag, LanguageSnapshot>();
        foreach (var pair in _snapshots)
        {
            snapshots.Add(pair.Key, LanguageSnapshotBuilder.Build(registry, pair.Key, _definitions[pair.Key],
                pair.Value, deferCatalogs: true));
        }
        var oldState = previous.Revision.State;
        var state = new LanguageState(oldState.CurrentLanguage, oldState.FormattingCulture,
            oldState.TextDirection, checked(oldState.Revision + 1));
        var view = new LanguageReadView(registry, new LanguageRevision(snapshots[state.CurrentLanguage], state));
        return new PackageUpdate(this, view, snapshots, modules);
    }

    internal sealed class PackageUpdate
    {
        private readonly LanguageManager _manager;
        private readonly LanguageReadView _oldView;
        private readonly LanguageReadView _newView;
        private readonly IReadOnlyDictionary<LanguageTag, LanguageSnapshot> _oldSnapshots;
        private readonly IReadOnlyDictionary<LanguageTag, LanguageSnapshot> _newSnapshots;
        private readonly IReadOnlyDictionary<string, string> _oldModules;
        private readonly IReadOnlyDictionary<string, string> _newModules;
        private PackagePublication? _publication;

        internal PackageUpdate(LanguageManager manager, LanguageReadView view,
            IReadOnlyDictionary<LanguageTag, LanguageSnapshot> snapshots, IReadOnlyDictionary<string, string> modules)
        {
            _manager = manager;
            _oldView = manager._context.Capture();
            _newView = view;
            _oldSnapshots = manager._snapshots;
            _newSnapshots = snapshots;
            _oldModules = manager._moduleData;
            _newModules = modules;
        }

        internal void Stage(PackagePublication publication)
        {
            _publication = publication;
            _manager._context.Stage(_newView, publication);
        }

        internal void RunCommittedRead(Action action)
        {
            _manager._context.Stage(_oldView);
            try
            {
                action();
            }
            finally
            {
                _manager._context.Stage(_newView, _publication);
            }
        }

        internal void Commit()
        {
            _manager._snapshots = _newSnapshots;
            _manager._moduleData = _newModules;
            _manager._context.Commit(_newView);
        }

        internal void CompleteCommit() => _manager._context.EndStaging();

        internal void Rollback()
        {
            _manager._snapshots = _oldSnapshots;
            _manager._moduleData = _oldModules;
            _manager._context.Commit(_oldView);
            _manager._context.EndStaging();
        }
    }
}
