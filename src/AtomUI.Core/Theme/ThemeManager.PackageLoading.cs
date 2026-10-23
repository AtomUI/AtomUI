using AtomUI.Generated.AtomUICore;
using AtomUI.Localization;
using AtomUI.Registration;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Definitions;
using AtomUI.Theme.Resources;
using AtomUI.Theme.Schema;
using AtomUI.Theme.Styling;
using AtomUI.Theme.Branding;
using Avalonia.Controls;

namespace AtomUI.Theme;

internal partial class ThemeManager
{
    private bool _loadingPackages;
    private ThemeStyleBatch? _packagePresentationCapture;
    private PackageUpdate? _packageReadUpdate;
    internal PackageUpdate? PackageReadUpdate => Volatile.Read(ref _packageReadUpdate);
    private HashSet<string> _enabledPackageIds = new(StringComparer.Ordinal);

    internal IEnumerable<string> EnabledPackageIds => _enabledPackageIds;
    internal void SetEnabledPackageIds(IEnumerable<string> ids) => _enabledPackageIds = ids.ToHashSet(StringComparer.Ordinal);

    internal void BeginPackageLoading()
    {
        VerifyTransitionAccess();
        lock (_transactionGate)
        {
            if (!_applicationInitialized || _activeTransaction is not null || _queuedTransaction is not null ||
                _catalogReloadOperationCount != 0 || _transactionExecutionUsers != 0 || _scopeUpdateProcessorRunning ||
                _pendingScopeUpdates.Count != 0)
            {
                throw new InvalidOperationException("Package loading requires an initialized, idle ThemeManager.");
            }
            _loadingPackages = true;
            _packagePresentationCapture = CaptureCommittedPresentation(_scopeGraph.CaptureAll());
        }
    }

    internal void EndPackageLoading()
    {
        _packagePresentationCapture?.Dispose();
        _packagePresentationCapture = null;
        _loadingPackages = false;
    }

    internal PackageUpdate PreparePackages(ThemeManagerBuilder builder)
    {
        builder.CompletePendingRegistrations();
        builder.ControlRegistrationState.Freeze();
        var manual = builder.ControlPackages;
        var manualControls = manual.SelectMany(package => package.Controls).ToArray();
        var manualRegistry = manualControls.Length == 0 ? null : new ThemeSchemaRegistry(
            GeneratedThemeSchema.GetGlobalTokens(), manualControls, GeneratedThemeSchema.GetAlgorithms());
        var newSemantics = manual.SelectMany(package => package.SemanticControls).ToArray();
        var newAssets = manual.SelectMany(package => package.ThemeAssets).ToArray();
        var schemaChanged = manual.Count != 0 || builder.PackageDeclarations.Count != 0;
        var oldIndex = _startupPackageIndex ?? StartupPackageIndex.Create([], null, _startupRegistry,
            SemanticParts.Controls, _controlThemeAssetDescriptors);
        var index = schemaChanged ? oldIndex.Append(builder.PackageDeclarations, manualRegistry, newSemantics, newAssets) : oldIndex;
        // Registering presentation factories alone does not change the Token schema or its revision domain.
        var registry = schemaChanged
            ? new ThemeSchemaRegistry(GeneratedThemeSchema.GetGlobalTokens(), index, GeneratedThemeSchema.GetAlgorithms())
            : _startupRegistry!;
        var semantics = schemaChanged ? new SemanticPartRegistry(index) : SemanticParts;
        var styleAssets = _styleAssetRegistry.Append(builder.StyleAssets, registry, semantics);
        _ = new ControlThemeAssetManifest(registry, newAssets);
        foreach (var asset in newAssets)
        {
            foreach (var binding in asset.SemanticThemeBindings)
            {
                if (!semantics.TryGetControlMetadata(binding.OwnerIdentity, out var owner) ||
                    !owner.Parts.Any(part => part.Theme is { } theme && theme.PropertyName == binding.PropertyName &&
                        theme.TargetType == binding.TargetType))
                {
                    throw new ThemeSchemaException($"Theme asset '{asset.AssetId}' has an invalid semantic binding.");
                }
            }
        }
        var catalog = schemaChanged
            ? CompiledThemeCatalog.AppendRegistry(_startupRegistry!, registry, _themeDefinitionResolvers)
            : CompiledThemeCatalog;
        var definition = catalog.Get(_lastCommittedRequest!.ThemeId);
        var capture = _scopeGraph.CaptureAll();
        var brandDiagnostics = ValidateBrandRequest(_lastCommittedRequest, registry.Revision, definition.Revision, styleAssets)
            .Concat(capture.Nodes.SelectMany(node => ValidateBrandConfig(node.LastValidConfig, registry.Revision,
                definition.Revision, styleAssets))).ToArray();
        if (brandDiagnostics.Length != 0)
        {
            throw new ThemeLoadException("Package append would invalidate an active brand recipe: " +
                string.Join(" ", brandDiagnostics.Select(static item => item.Message)));
        }
        var defaults = ThemeCompiler.CreateDefinitionDefaults(definition.Definition, registry);
        var runtime = _lastCommittedRequest.Config is null ? _normalizedRuntimeDefaultConfig! :
            ThemeConfigNormalizer.Normalize(AddDefaultFont(_lastCommittedRequest.Config)!, registry);
        if (!runtime.Success)
        {
            throw new ThemeLoadException("Package theme configuration validation failed.");
        }
        var effective = ThemeStartupCompilation.MergeEffectiveConfig(defaults, _normalizedInitialConfig!.Config!, runtime.Config!);
        var snapshot = CompilePackageSnapshot(definition, registry, effective, CurrentSnapshot!);
        var scopes = new List<(ThemeContext Context, ThemeSnapshot Snapshot)>();
        var parents = new Dictionary<long, ThemeSnapshot> { [0] = snapshot };
        foreach (var node in capture.Nodes)
        {
            var parent = parents[node.Stamp.ParentRegistrationId];
            var normalized = ThemeConfigNormalizer.Normalize(node.LastValidConfig, registry);
            if (!normalized.Success)
            {
                throw new ThemeLoadException("Package scope configuration validation failed.");
            }
            var scopeDefaults = node.LastValidConfig.Inherit ? defaults : ThemeCompiler.CreateLibraryDefaults(registry);
            var merge = ThemeConfigMerger.Merge(scopeDefaults, parent.EffectiveConfig, normalized.Config!);
            var child = CompilePackageSnapshot(definition, registry, merge.EffectiveConfig, node.Context.Snapshot);
            parents.Add(node.Stamp.RegistrationId, child);
            scopes.Add((node.Context, child));
        }
        var resources = _staticResourceProvider!.CurrentTable.Resources.ToList();
        var resourceIds = resources.OfType<DeferredControlThemeResourceProvider>().Select(resource => resource.AssetId)
            .Concat(index.Assets.Select(asset => asset.Data.Id)).ToHashSet(StringComparer.Ordinal);
        foreach (var package in manual)
        {
            resources.AddRange(package.ControlThemesProvider.ControlThemes);
            foreach (var resource in package.Resources)
            {
                if (!resourceIds.Add(resource.AssetId))
                {
                    throw new ThemeResourceRegisterException($"Theme resource '{resource.AssetId}' is already registered.");
                }
                resources.Add(new DeferredControlThemeResourceProvider(resource.AssetId, resource.Factory, resource.ResourceKeys, package.Id,
                    package.ThemeAssets.FirstOrDefault(asset => asset.AssetId == resource.AssetId)?.ExportedThemes
                        .Select(export => export.ResourceKey).ToArray()));
            }
        }
        var table = new StaticThemeResourceProvider.ResourceTable(resources.ToArray());
        var ids = _enabledPackageIds.Concat(builder.PackageDeclarations.Select(package => package.Definition.Id))
            .Concat(manual.Select(package => package.Id)).ToHashSet(StringComparer.Ordinal);
        return new PackageUpdate(this, index, registry, semantics, catalog, snapshot, capture, scopes, table, ids, styleAssets);
    }

    private ThemeSnapshot CompilePackageSnapshot(CompiledThemeDefinition definition, ThemeSchemaRegistry registry,
        NormalizedThemeConfig config, ThemeSnapshot previous)
    {
        var result = GetThemeCompiler().CompilePackageAppend(new ThemeCompileInput(definition.Definition,
            definition.Revision, config, registry, previous));
        return result.Success ? result.Snapshot! : throw new ThemeLoadException(
            "Package append could not preserve committed theme values.", result.Exception);
    }

    internal sealed class PackageUpdate
    {
        private readonly ThemeManager _manager;
        private readonly ThemeScopeCapture _capture;
        private readonly PackageState _previous;
        private readonly PackageState _candidate;
        private readonly int _threadId = Environment.CurrentManagedThreadId;
        private readonly Dictionary<ThemeContext, ThemeSnapshot> _previousContexts;
        private readonly Dictionary<ThemeContext, ThemeSnapshot> _candidateContexts;
        private readonly HashSet<IResourceProvider> _existingResources;
        private readonly HashSet<StartupAssetEntry> _existingAssets;
        private readonly StartupResourceProvider.ResourceIndex _previousResources;
        private readonly StartupResourceProvider.ResourceIndex _candidateResources;
        private PackagePublication? _publication;
        private Action<Action>? _runCommittedLanguageRead;
        private int _committedReadDepth;

        internal bool IsCandidateRead => _publication?.IsCommitted == true ||
            Environment.CurrentManagedThreadId == _threadId && _committedReadDepth == 0;
        private PackageState ReadState => IsCandidateRead ? _candidate : _previous;
        internal ThemeSnapshot Snapshot => ReadState.Snapshot;
        internal ThemeState Theme => ReadState.Theme;
        internal SemanticPartRegistry Semantics => ReadState.Semantics;
        internal CompiledThemeCatalog Catalog => ReadState.Catalog;
        internal ThemeStyleAssetRegistry StyleAssets => ReadState.StyleAssets;
        internal bool OwnsExistingResource(IResourceProvider resource) => _existingResources.Contains(resource);
        internal bool OwnsExistingAsset(StartupAssetEntry asset) => _existingAssets.Contains(asset);

        internal ThemeSnapshot? GetContextSnapshot(ThemeContext context) =>
            (IsCandidateRead ? _candidateContexts : _previousContexts).GetValueOrDefault(context);

        internal void RunCommittedRead(Action action)
        {
            _committedReadDepth++;
            try
            {
                _runCommittedLanguageRead!(action);
            }
            finally
            {
                _committedReadDepth--;
            }
        }

        internal PackageUpdate(ThemeManager manager, StartupPackageIndex index, ThemeSchemaRegistry registry,
            SemanticPartRegistry semantics, CompiledThemeCatalog catalog, ThemeSnapshot snapshot,
            ThemeScopeCapture capture, IReadOnlyList<(ThemeContext Context, ThemeSnapshot Snapshot)> scopes,
            StaticThemeResourceProvider.ResourceTable table, HashSet<string> ids, ThemeStyleAssetRegistry styleAssets)
        {
            _manager = manager;
            _capture = capture;
            _previous = new PackageState(manager._startupPackageIndex, manager._startupRegistry!, manager.SemanticParts,
                manager.CompiledThemeCatalog, manager.CurrentSnapshot!, manager._currentSnapshotKey, manager.CurrentTheme!,
                scopes.Select(scope => (scope.Context, scope.Context.Snapshot)).ToArray(),
                manager._staticResourceProvider!.CurrentTable, manager._enabledPackageIds, manager._styleAssetRegistry);
            var input = new ThemeCompileInput(snapshot.Definition, snapshot.DefinitionRevision, snapshot.EffectiveConfig, registry);
            _candidate = new PackageState(index, registry, semantics, catalog, snapshot, ThemeSnapshotCacheKey.Create(input),
                CreateScopeState(snapshot, manager.CurrentTheme!.TransitionId), scopes, table, ids, styleAssets);
            _previousContexts = _previous.Scopes.ToDictionary(scope => scope.Context, scope => scope.Snapshot);
            _candidateContexts = scopes.ToDictionary(scope => scope.Context, scope => scope.Snapshot);
            _previousContexts.Add(manager.RootContext, _previous.Snapshot);
            _candidateContexts.Add(manager.RootContext, snapshot);
            _existingResources = new HashSet<IResourceProvider>(_previous.Table.Resources, ReferenceEqualityComparer.Instance);
            _previousResources = manager._startupResourceProvider!.CurrentIndex;
            _candidateResources = StartupResourceProvider.PrepareIndex(index);
            _existingAssets = _previousResources.Index.Assets.ToHashSet();
        }

        internal void RunResourceUpdate(Action action) =>
            _manager._startupResourceProvider!.RunUpdate(() => _manager._staticResourceProvider!.RunUpdate(action));

        internal void Stage(PackagePublication publication, Action<Action> runCommittedLanguageRead)
        {
            _publication = publication;
            _runCommittedLanguageRead = runCommittedLanguageRead;
            Volatile.Write(ref _manager._packageReadUpdate, this);
            _manager._startupResourceProvider!.StageIndex(_candidateResources);
            _manager._staticResourceProvider!.StageTable(_candidate.Table);
            _manager._staticResourceProvider.PrepareTable(_candidate.Table.Resources);
        }

        internal void Commit() => Apply(_candidate);

        internal void CompleteCommit()
        {
            _manager._startupResourceProvider!.StageIndex(null);
            _manager._staticResourceProvider!.StageTable(null);
            Volatile.Write(ref _manager._packageReadUpdate, null);
            _runCommittedLanguageRead = null;
        }

        internal void EnsureCurrent()
        {
            _manager._packagePresentationCapture!.Validate();
            if (!_manager._scopeGraph.IsCurrent(_capture))
            {
                throw new InvalidOperationException("The theme scope graph changed during package loading.");
            }
        }

        internal void Rollback()
        {
            Apply(_previous);
            CompleteCommit();
            var failures = new List<Exception>();
            ThemeResourceOwnerBridge.TryCleanup(_manager._startupResourceProvider!.ReleaseUnusedResources, failures);
            ThemeResourceOwnerBridge.TryCleanup(_manager._staticResourceProvider!.ReleaseUnusedResources, failures);
            ThemeResourceOwnerBridge.ThrowFailures(failures);
        }

        private void Apply(PackageState state)
        {
            _manager._startupPackageIndex = state.Index;
            _manager._startupRegistry = state.Registry;
            _manager.SemanticParts = state.Semantics;
            _manager._compiledThemeCatalog = state.Catalog;
            _manager._currentSnapshot = state.Snapshot;
            _manager._currentSnapshotKey = state.Key;
            _manager._currentTheme = state.Theme;
            _manager._enabledPackageIds = state.Ids;
            _manager._styleAssetRegistry = state.StyleAssets;
            _manager.RootContext.Commit(state.Snapshot);
            foreach (var scope in state.Scopes)
            {
                scope.Context.Commit(scope.Snapshot);
            }
            _manager._startupResourceProvider!.CommitIndex(ReferenceEquals(state, _candidate) ? _candidateResources : _previousResources);
            _manager._staticResourceProvider!.CommitTable(state.Table);
        }

        internal void Publish()
        {
            // Root token provider publishes exactly one resources event. Observer failures are
            // diagnostics after commit and cannot turn a successful append into a failed operation.
            _manager.RootContext.Publish();
            foreach (var scope in _candidate.Scopes)
            {
                scope.Context.Publish(notifyResources: false);
            }
        }

        private sealed record PackageState(StartupPackageIndex? Index, ThemeSchemaRegistry Registry,
            SemanticPartRegistry Semantics, CompiledThemeCatalog Catalog, ThemeSnapshot Snapshot,
            ThemeSnapshotCacheKey? Key, ThemeState Theme,
            IReadOnlyList<(ThemeContext Context, ThemeSnapshot Snapshot)> Scopes,
            StaticThemeResourceProvider.ResourceTable Table, HashSet<string> Ids, ThemeStyleAssetRegistry StyleAssets);
    }
}
