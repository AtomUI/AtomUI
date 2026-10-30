using System.Diagnostics;
using AtomUI.Registration;
using AtomUI.Generated.AtomUICore;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Definitions;
using AtomUI.Theme.Resources;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Threading;

namespace AtomUI.Theme;

internal partial class ThemeManager : Styles, IThemeManager, IDisposable
{
    public FontFamily? FontFamily { get; internal set; }
    internal ThemeSnapshot? CurrentSnapshot => PackageReadUpdate?.Snapshot ?? Volatile.Read(ref _currentSnapshot);
    public ThemeState? CurrentTheme => PackageReadUpdate?.Theme ?? Volatile.Read(ref _currentTheme);
    public IReadOnlyList<ThemeInfo> AvailableThemes =>
        (PackageReadUpdate?.Catalog ?? Volatile.Read(ref _compiledThemeCatalog))?.AvailableThemes ?? Array.Empty<ThemeInfo>();
    public IReadOnlyList<ThemeDiagnostic> ThemeCatalogDiagnostics =>
        Volatile.Read(ref _themeCatalogDiagnostics);
    private SemanticPartRegistry _semanticParts = SemanticPartRegistry.Empty;
    public SemanticPartRegistry SemanticParts
    {
        get => PackageReadUpdate?.Semantics ?? Volatile.Read(ref _semanticParts);
        internal set => Volatile.Write(ref _semanticParts, value);
    }
    internal CompiledThemeCatalog CompiledThemeCatalog =>
        PackageReadUpdate?.Catalog ?? Volatile.Read(ref _compiledThemeCatalog) ??
        throw new InvalidOperationException("The compiled theme catalog has not been initialized.");

    public event EventHandler<ThemeChangedEventArgs>? ThemeChanged;
    public event EventHandler<ThemeChangeFailedEventArgs>? ThemeChangeFailed;
    public event EventHandler<ThemeCatalogChangedEventArgs>? ThemeCatalogChanged;
    internal event Action<object, ControlTheme>? ControlThemeResolved;

    internal void NotifyControlThemeResolved(object key, ControlTheme theme) => ControlThemeResolved?.Invoke(key, theme);

    private readonly List<ControlTokenDescriptor> _controlTokenDescriptors;
    private readonly List<ControlThemeAssetDescriptor> _controlThemeAssetDescriptors;
    private readonly List<IControlThemesProvider> _controlThemesProviders;
    private ThemeCompiler? _themeCompiler;
    private ThemeSnapshotCache? _themeSnapshotCache;
    private readonly object _transactionGate;
    private readonly SemaphoreSlim _transactionExecutionGate;
    private readonly Func<bool> _checkTransitionAccess;
    private readonly ThemePrepareDelegate _prepareTheme;
    private ThemeTransaction? _activeTransaction;
    private ThemeTransaction? _queuedTransaction;
    private readonly Dictionary<long, PendingThemeScopeUpdate> _pendingScopeUpdates;
    private readonly HashSet<ThemeContextLease> _contextLeases;
    private bool _scopeUpdateProcessorRunning;
    private int _catalogReloadOperationCount;
    private int _transactionExecutionUsers;
    private bool _disposeRequested;
    private bool _disposeCompleted;
    private ThemeRequestCacheKey? _lastCommittedRequestKey;
    private ThemeRequest? _lastCommittedRequest;
    private ThemeSnapshot? _currentSnapshot;
    private ThemeSnapshotCacheKey? _currentSnapshotKey;
    private ThemeConfigNormalizeResult? _normalizedInitialConfig;
    private ThemeConfigNormalizeResult? _normalizedRuntimeDefaultConfig;
    private ThemeContext? _rootContext;
    private readonly ThemeScopeGraph _scopeGraph;
    private ThemeState? _currentTheme;
    private Application? _application;
    private IResourceDictionary? _applicationResources;
    private Style? _rootContextStyle;
    private readonly Dictionary<IControlThemesProvider, List<ControlThemeResourceRegistration>> _stagedThemeResources = new();
    private readonly List<IControlThemesProvider> _registeredControlThemesProviders = [];
    private readonly Dictionary<IControlThemesProvider, IReadOnlyList<IResourceProvider>>
        _controlThemeProviderBaseResources = new(ReferenceEqualityComparer.Instance);
    private StaticThemeResourceProvider? _staticResourceProvider;
    // 语义部件描述符登记表（提交时由 BuildCore 一次写入）：SemanticParts 由其按需重建。
    private readonly List<ControlSemanticDescriptor> _stagedSemanticControls = [];
    private ThemeSchemaRegistry? _startupRegistry;
    private IReadOnlyList<PackageSchemaFingerprint>? _packageSchemaFingerprints;
    private CompiledThemeCatalog? _compiledThemeCatalog;
    private readonly ThemeDefinitionLoadCache _themeDefinitionLoadCache;
    private IReadOnlyList<ThemeDefinitionRegistration> _themeDefinitionResolvers =
        Array.AsReadOnly(new[] { new ThemeDefinitionRegistration(Generated.AtomUICore.GeneratedThemePackageRegistration.CreatePackage()) });
    private string _applicationId = typeof(ThemeManager).Assembly.GetName().Name!;
    private string _applicationDataRoot = string.Empty;
    private IReadOnlyList<ThemeDiagnostic> _themeCatalogDiagnostics = Array.Empty<ThemeDiagnostic>();
    private ThemeRequest _initialRequest = new(
        IThemeManager.DEFAULT_THEME_ID,
        null,
        ThemeTransitionReason.Startup);
    private ThemeRequest? _followSystemLightRequest;
    private ThemeRequest? _followSystemDarkRequest;
    private ThemeSnapshot? _startupGlobalParent;
    private StartupPackageIndex? _startupPackageIndex;
    private StartupResourceProvider? _startupResourceProvider;
    private bool _applicationInitialized;
    private long _generation;
    private long _nextTransitionId;
    private long _nextCatalogGeneration;
    
    internal ThemeManager(
        Func<bool>? themeTransitionAccessCheck = null,
        ThemePrepareDelegate? prepareTheme = null)
    {
        _controlTokenDescriptors = new List<ControlTokenDescriptor>();
        _controlThemeAssetDescriptors = new List<ControlThemeAssetDescriptor>();
        _controlThemesProviders  = new List<IControlThemesProvider>();
        _transactionGate         = new object();
        _transactionExecutionGate = new SemaphoreSlim(1, 1);
        _scopeGraph              = new ThemeScopeGraph(this);
        _pendingScopeUpdates     = new Dictionary<long, PendingThemeScopeUpdate>();
        _contextLeases           = new HashSet<ThemeContextLease>(ReferenceEqualityComparer.Instance);
        _themeDefinitionLoadCache = new ThemeDefinitionLoadCache();
        _checkTransitionAccess   = themeTransitionAccessCheck ?? (static () => Dispatcher.UIThread.CheckAccess());
        _prepareTheme            = prepareTheme ?? PrepareThemeAsync;
    }

    internal ThemeContext RootContext => _rootContext ??
        throw new InvalidOperationException("The root ThemeContext has not been initialized.");

    internal ThemeScopeGraph ScopeGraph => _scopeGraph;
    internal void ConfigureStartup(
        ThemeRequest initialRequest,
        ThemeRequest? followSystemLightRequest,
        ThemeRequest? followSystemDarkRequest)
    {
        ArgumentNullException.ThrowIfNull(initialRequest);
        if ((followSystemLightRequest is null) != (followSystemDarkRequest is null))
        {
            throw new ArgumentException(
                "FollowSystem requires both Light and Dark requests.");
        }

        _initialRequest            = initialRequest;
        _followSystemLightRequest  = followSystemLightRequest;
        _followSystemDarkRequest   = followSystemDarkRequest;
    }

    internal void ConfigureThemeDefinitions(
        IReadOnlyList<ThemeDefinitionRegistration> resolvers,
        string applicationId,
        string applicationDataRoot)
    {
        ArgumentNullException.ThrowIfNull(resolvers);
        ThemeApplicationIdentity.Validate(applicationId, nameof(applicationId));
        ArgumentNullException.ThrowIfNull(applicationDataRoot);

        _themeDefinitionResolvers = Array.AsReadOnly(resolvers.ToArray());
        _applicationId            = applicationId;
        _applicationDataRoot      = applicationDataRoot;
    }

    internal void InitializeApplication(
        Application application,
        ThemeAppearance? initialSystemAppearance = null,
        ThemeSnapshot? startupGlobalParent = null)
    {
        ArgumentNullException.ThrowIfNull(application);
        if (_applicationInitialized)
        {
            throw new InvalidOperationException("ThemeManager is already initialized.");
        }

        ObjectDisposedException.ThrowIf(_disposeRequested, this);
        _application = application;
        try
        {
            PrepareRegistrationSchema();
            var registry = _startupRegistry!;
            var (runtimeConfig, initialConfig, catalogResult) = PrepareRegistrationThemeInputs(registry, _themeDefinitionResolvers);
            _normalizedRuntimeDefaultConfig = runtimeConfig;
            _normalizedInitialConfig = initialConfig;
            _compiledThemeCatalog = catalogResult.Catalog;
            _themeCatalogDiagnostics = catalogResult.Diagnostics;
            MountStaticResources();

            var request = _followSystemLightRequest is null
                ? _initialRequest
                : (initialSystemAppearance ?? ResolveSystemAppearance(application)) == ThemeAppearance.Dark
                    ? _followSystemDarkRequest!
                    : _followSystemLightRequest;
            _startupGlobalParent = startupGlobalParent;
            ThemeTransitionResult result;
            try
            {
                result = ApplyThemeAsync(request).GetAwaiter().GetResult();
            }
            finally
            {
                _startupGlobalParent = null;
            }
            if (result.Status is not ThemeTransitionStatus.Committed and not ThemeTransitionStatus.NoOp)
            {
                var exceptionMessage = result.Exception is null
                    ? string.Empty
                    : $" {result.Exception.GetBaseException().Message}";
                throw new ThemeLoadException(
                    $"Initial theme '{request.ThemeId}' failed with status '{result.Status}': " +
                    string.Join(" ", result.Diagnostics.Select(static diagnostic => diagnostic.Message)) +
                    exceptionMessage,
                    result.Exception);
            }

            // Framework resources precede external theme children without outranking the
            // application's existing merged dictionaries or its explicit Resources overrides.
            _applicationResources = application.Styles.Resources;
            _applicationResources.MergedDictionaries.Insert(0, Resources);
            _rootContextStyle = new Style(selector => selector.OfType<TopLevel>());
            _rootContextStyle.Setters.Add(new Setter(ThemeScope.ContextProperty, RootContext));
            application.Styles.Add(_rootContextStyle);
            _applicationInitialized = true;
            SubscribeSystemAppearance(application);
        }
        catch (Exception initializationException)
        {
            try
            {
                Dispose();
            }
            catch (Exception cleanupException)
            {
                throw new AggregateException("ThemeManager initialization and cleanup failed.",
                    initializationException, cleanupException);
            }
            throw;
        }
    }

    internal void CompleteStartupRegistration()
    {
        if (_startupPackageIndex is null)
        {
            _startupPackageIndex = StartupPackageIndex.Create([], null, _startupRegistry,
                SemanticParts.Controls, _controlThemeAssetDescriptors);
        }
        // 注册终态之后，查询由 registry、SemanticParts 和稳定资源表承担，不保留第二套构建数据库。
        _controlTokenDescriptors.Clear();
        _controlThemeAssetDescriptors.Clear();
        _stagedSemanticControls.Clear();
        _controlThemesProviders.Clear();
        _registeredControlThemesProviders.Clear();
        _controlThemeProviderBaseResources.Clear();
        _stagedThemeResources.Clear();
        _packageSchemaFingerprints = null;
    }

    internal Task<ThemeTransitionResult> ApplySystemAppearanceAsync(
        ThemeAppearance appearance,
        CancellationToken cancellationToken = default)
    {
        var request = appearance == ThemeAppearance.Dark
            ? _followSystemDarkRequest
            : _followSystemLightRequest;
        if (request is null)
        {
            throw new InvalidOperationException("FollowSystem is not configured.");
        }
        return ApplyThemeAsync(request, cancellationToken);
    }

    public Task<ThemeTransitionResult> ApplyThemeAsync(
        ThemeRequest request,
        CancellationToken cancellationToken = default)
    {
        VerifyTransitionAccess();
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ThemeId);

        var requestKey = ThemeRequestCacheKey.Create(request);
        ThemeTransaction? transaction;
        ThemeTransitionResult? immediateResult = null;
        var startProcessor = false;
        lock (_transactionGate)
        {
            if (_queuedTransaction is not null &&
                _queuedTransaction.RequestKey.Equals(requestKey))
            {
                transaction = _queuedTransaction;
            }
            else if (_queuedTransaction is null &&
                     _activeTransaction is not null &&
                     _activeTransaction.RequestKey.Equals(requestKey))
            {
                transaction = _activeTransaction;
            }
            else if (_activeTransaction is null &&
                     _lastCommittedRequestKey is not null &&
                     _lastCommittedRequestKey.Equals(requestKey))
            {
                transaction = null;
                immediateResult = new ThemeTransitionResult(
                    NextTransitionId(),
                    ThemeTransitionStatus.NoOp,
                    _currentTheme,
                    Array.Empty<ThemeDiagnostic>(),
                    Array.Empty<ThemeDiagnostic>(),
                    null);
            }
            else
            {
                var generation = ++_generation;
                transaction = new ThemeTransaction(
                    NextTransitionId(),
                    generation,
                    request,
                    requestKey)
                {
                    ScopeCapture = _scopeGraph.CaptureAll()
                };
                if (_activeTransaction is null)
                {
                    _activeTransaction = transaction;
                    startProcessor = true;
                }
                else
                {
                    if (_queuedTransaction is not null)
                    {
                        _queuedTransaction.Complete(CreateSupersededResult(_queuedTransaction));
                    }
                    _queuedTransaction = transaction;
                }
            }
        }

        if (immediateResult is not null)
        {
            return WaitForCallerAsync(Task.FromResult(immediateResult), cancellationToken);
        }

        if (startProcessor)
        {
            _ = ProcessTransactionQueueAsync(transaction!);
        }

        return WaitForCallerAsync(transaction!.Completion, cancellationToken);
    }

    public Task<ThemeCatalogReloadResult> ReloadThemesAsync(
        CancellationToken cancellationToken = default)
    {
        VerifyTransitionAccess();
        if (!_applicationInitialized ||
            _compiledThemeCatalog is null ||
            _startupRegistry is null ||
            _currentSnapshot is null ||
            _lastCommittedRequest is null)
        {
            throw new InvalidOperationException(
                "ThemeManager must be initialized before its theme catalog can be reloaded.");
        }

        long mutationGeneration;
        long reloadGeneration;
        long transitionId;
        lock (_transactionGate)
        {
            mutationGeneration = ++_generation;
            reloadGeneration = ++_nextCatalogGeneration;
            _catalogReloadOperationCount++;
            transitionId = NextTransitionId();

        }

        var transaction = ProcessCatalogReloadAsync(mutationGeneration, reloadGeneration, transitionId);
        return cancellationToken.CanBeCanceled
            ? transaction.WaitAsync(cancellationToken)
            : transaction;
    }

    private async Task<ThemeCatalogReloadResult> ProcessCatalogReloadAsync(
        long mutationGeneration,
        long reloadGeneration,
        long transitionId)
    {
        await EnterTransactionExecutionGateAsync().ConfigureAwait(true);
        try
        {
            ThemeScopeCapture scopeCapture;
            CompiledThemeCatalog currentCatalog;
            ThemeSnapshot currentSnapshot;
            ThemeSnapshotCacheKey? currentSnapshotKey;
            ThemeRequest currentRequest;
            IReadOnlyList<ThemeDiagnostic> currentDiagnostics;
            lock (_transactionGate)
            {
                if (mutationGeneration != _generation)
                {
                    return new ThemeCatalogReloadResult(reloadGeneration, ThemeCatalogReloadStatus.Superseded,
                        AvailableThemes, _currentTheme, [], [], null);
                }
                // 排队期间可能有新的主题变更提交，使本次请求的代际过期。进入执行门后一起捕获完整基线，
                // 避免旧目录静态切片与新 resolver 集合混代；请求代际仍保留调用顺序。
                scopeCapture = _scopeGraph.CaptureAll();
                currentCatalog = _compiledThemeCatalog!;
                currentSnapshot = _currentSnapshot!;
                currentSnapshotKey = _currentSnapshotKey;
                currentRequest = _lastCommittedRequest!;
                currentDiagnostics = _themeCatalogDiagnostics;
            }
            CatalogReloadPreparation prepared;
            try
            {
                prepared = await Task.Run(() => PrepareCatalogReload(
                    mutationGeneration,
                    reloadGeneration,
                    transitionId,
                    scopeCapture,
                    currentCatalog,
                    currentSnapshot,
                    currentSnapshotKey,
                    currentRequest,
                    currentDiagnostics)).ConfigureAwait(true);
            }
            catch (Exception exception)
            {
                prepared = CatalogReloadPreparation.Failed(
                    [new ThemeDiagnostic(
                        "ATMTHM4201",
                        ThemeDiagnosticSeverity.Error,
                        nameof(ThemeManager),
                        "$",
                        $"Theme catalog reload failed during preparation: " +
                        exception.GetBaseException().Message)],
                    exception);
            }

            if (prepared.Success && prepared.HasChanges && prepared.SnapshotChanged)
            {
                try
                {
                    PrepareCommitPayload(prepared.ThemeTransaction!);
                }
                catch (Exception exception)
                {
                    prepared = CatalogReloadPreparation.Failed(
                        prepared.Diagnostics.Append(new ThemeDiagnostic(
                            "ATMTHM4202",
                            ThemeDiagnosticSeverity.Error,
                            nameof(ThemeManager),
                            "$",
                            "Theme catalog reload commit payload could not be staged: " +
                            exception.GetBaseException().Message)).ToArray(),
                        exception);
                }
            }

            lock (_transactionGate)
            {
                if (mutationGeneration != _generation ||
                    !_scopeGraph.IsCurrent(scopeCapture))
                {
                    return new ThemeCatalogReloadResult(
                        reloadGeneration,
                        ThemeCatalogReloadStatus.Superseded,
                        AvailableThemes,
                        _currentTheme,
                        prepared.Diagnostics,
                        Array.Empty<ThemeDiagnostic>(),
                        null);
                }

                if (!prepared.Success)
                {
                    _themeCatalogDiagnostics = prepared.Diagnostics;
                    return new ThemeCatalogReloadResult(
                        reloadGeneration,
                        ThemeCatalogReloadStatus.Failed,
                        AvailableThemes,
                        _currentTheme,
                        prepared.Diagnostics,
                        Array.Empty<ThemeDiagnostic>(),
                        prepared.Exception);
                }

                if (!prepared.HasChanges)
                {
                    _themeCatalogDiagnostics = prepared.Diagnostics;
                    return new ThemeCatalogReloadResult(
                        reloadGeneration,
                        ThemeCatalogReloadStatus.NoOp,
                        AvailableThemes,
                        _currentTheme,
                        prepared.Diagnostics,
                        Array.Empty<ThemeDiagnostic>(),
                        null);
                }

                _compiledThemeCatalog = prepared.Catalog;
                _themeCatalogDiagnostics = prepared.Diagnostics;
                if (prepared.SnapshotChanged)
                {
                    CommitCore(prepared.ThemeTransaction!);
                }
            }

            var publishDiagnostics = new List<ThemeDiagnostic>();
            if (prepared.SnapshotChanged)
            {
                publishDiagnostics.AddRange(Publish(prepared.ThemeTransaction!));
            }

            PublishCatalogChanged(reloadGeneration, publishDiagnostics);
            return new ThemeCatalogReloadResult(
                reloadGeneration,
                ThemeCatalogReloadStatus.Committed,
                AvailableThemes,
                CurrentTheme,
                prepared.Diagnostics,
                publishDiagnostics,
                null);
        }
        finally
        {
            lock (_transactionGate)
            {
                _catalogReloadOperationCount--;
            }
            ExitTransactionExecutionGate();
            StartPendingScopeUpdateProcessor();
        }
    }

    private void PublishCatalogChanged(long generation, List<ThemeDiagnostic> publishDiagnostics)
    {
        ThemeEventDispatcher.Dispatch(
            ThemeCatalogChanged,
            this,
            new ThemeCatalogChangedEventArgs(generation, AvailableThemes, CurrentTheme),
            publishDiagnostics,
            nameof(ThemeCatalogChanged));
    }

    private CatalogReloadPreparation PrepareCatalogReload(
        long mutationGeneration,
        long reloadGeneration,
        long transitionId,
        ThemeScopeCapture scopeCapture,
        CompiledThemeCatalog currentCatalog,
        ThemeSnapshot currentSnapshot,
        ThemeSnapshotCacheKey? currentSnapshotKey,
        ThemeRequest currentRequest,
        IReadOnlyList<ThemeDiagnostic> currentDiagnostics)
    {
        var catalogResult = CompiledThemeCatalog.LoadReload(
            _startupRegistry!,
            currentCatalog,
            _themeDefinitionResolvers,
            new ThemeDefinitionResolveContext(
                _applicationId,
                _applicationDataRoot,
                true,
                reloadGeneration),
            _themeDefinitionLoadCache);
        if (!catalogResult.Success)
        {
            return CatalogReloadPreparation.Failed(
                catalogResult.Diagnostics,
                catalogResult.Exception);
        }

        var catalog = catalogResult.Catalog!;
        var targetThemeId = catalog.Contains(currentRequest.ThemeId)
            ? currentRequest.ThemeId
            : catalog.DefaultDefinition.Info.Id;
        var request = currentRequest with
        {
            ThemeId = targetThemeId,
            Reason = ThemeTransitionReason.CatalogReload
        };
        var transaction = new ThemeTransaction(
            transitionId,
            mutationGeneration,
            request,
            ThemeRequestCacheKey.Create(request))
        {
            ScopeCapture = scopeCapture
        };
        var themePreparation = PrepareCompiledTheme(request, currentSnapshot, catalog);
        transaction.Preparation = themePreparation;
        if (!themePreparation.Success)
        {
            return CatalogReloadPreparation.Failed(
                catalogResult.Diagnostics.Concat(themePreparation.Diagnostics).ToArray(),
                themePreparation.Exception);
        }

        var snapshotChanged = currentSnapshotKey is null ||
                              !currentSnapshotKey.Value.Equals(themePreparation.SnapshotKey);
        if (snapshotChanged)
        {
            var scopePreparation = PrepareScopeCapture(
                themePreparation.Snapshot!,
                scopeCapture,
                useLastValidConfig: true);
            if (!scopePreparation.Success)
            {
                return CatalogReloadPreparation.Failed(
                    catalogResult.Diagnostics.Concat(scopePreparation.Diagnostics).ToArray(),
                    scopePreparation.Exception);
            }
            transaction.PreparedScopes = scopePreparation.Snapshots;
        }

        var catalogChanged = !currentCatalog.ContentEquals(catalog);
        var diagnosticsChanged = !currentDiagnostics.SequenceEqual(catalogResult.Diagnostics);
        return CatalogReloadPreparation.Succeeded(
            catalog,
            transaction,
            snapshotChanged,
            catalogChanged || snapshotChanged || diagnosticsChanged,
            catalogResult.Diagnostics);
    }

    private async Task ProcessTransactionQueueAsync(ThemeTransaction transaction)
    {
        var current = transaction;
        while (true)
        {
            var result = await ExecuteTransactionAsync(current).ConfigureAwait(true);
            ThemeTransaction? next;
            lock (_transactionGate)
            {
                Debug.Assert(ReferenceEquals(_activeTransaction, current));
                next = _queuedTransaction;
                _queuedTransaction = null;
                _activeTransaction = next;
            }

            current.Complete(result);
            if (next is null)
            {
                StartPendingScopeUpdateProcessor();
                return;
            }
            current = next;
        }
    }

    private async ValueTask<ThemeTransitionResult> ExecuteTransactionAsync(
        ThemeTransaction transaction)
    {
        await EnterTransactionExecutionGateAsync().ConfigureAwait(true);
        try
        {
            return await ExecuteTransactionCoreAsync(transaction).ConfigureAwait(true);
        }
        finally
        {
            ExitTransactionExecutionGate();
        }
    }

    private async ValueTask<ThemeTransitionResult> ExecuteTransactionCoreAsync(
        ThemeTransaction transaction)
    {
        var committed = false;
        try
        {
            transaction.Phase = ThemeTransactionPhase.Prepare;
            // 启动首次编译时 CurrentSnapshot 为空：使用后台 Prepare 的全局层作为 ReusableParent，
            // ThemeCompiler.CanReuseGlobal 成立时直接复用全局 Token 值表与资源表。
            var preparation = await _prepareTheme(
                transaction.Request,
                CurrentSnapshot ?? _startupGlobalParent,
                CancellationToken.None).ConfigureAwait(true);
            transaction.Preparation = preparation;
            lock (_transactionGate)
            {
                if (!IsTransactionCurrent(transaction))
                {
                    return CreateSupersededResult(transaction);
                }
            }

            if (!preparation.Success)
            {
                return CreateFailedResult(transaction, preparation.Diagnostics, preparation.Exception);
            }

            var scopePreparation = PrepareScopeCapture(
                preparation.Snapshot!,
                transaction.ScopeCapture!,
                useLastValidConfig: true);
            if (!scopePreparation.Success)
            {
                lock (_transactionGate)
                {
                    if (!IsTransactionCurrent(transaction))
                    {
                        return CreateSupersededResult(transaction);
                    }
                }

                return CreateFailedResult(
                    transaction,
                    scopePreparation.Diagnostics,
                    scopePreparation.Exception);
            }
            transaction.PreparedScopes = scopePreparation.Snapshots;

            lock (_transactionGate)
            {
                if (!IsTransactionCurrent(transaction))
                {
                    return CreateSupersededResult(transaction);
                }

                if (_currentSnapshotKey is ThemeSnapshotCacheKey currentKey &&
                    currentKey.Equals(preparation.SnapshotKey))
                {
                    return new ThemeTransitionResult(
                        transaction.TransitionId,
                        ThemeTransitionStatus.NoOp,
                        _currentTheme,
                        preparation.Diagnostics,
                        Array.Empty<ThemeDiagnostic>(),
                        null);
                }
            }

            PrepareCommitPayload(transaction);
            lock (_transactionGate)
            {
                if (!IsTransactionCurrent(transaction))
                {
                    return CreateSupersededResult(transaction);
                }

                transaction.Phase = ThemeTransactionPhase.CommitCore;
                CommitCore(transaction);
                committed = true;
            }

            transaction.Phase = ThemeTransactionPhase.Publish;
            var publishDiagnostics = Publish(transaction);
            return new ThemeTransitionResult(
                transaction.TransitionId,
                ThemeTransitionStatus.Committed,
                transaction.PreparedState,
                preparation.Diagnostics,
                publishDiagnostics,
                null);
        }
        catch (Exception exception)
        {
            if (committed)
            {
                ThemeRuntimeLogger.LogPublishFailure(
                    this,
                    nameof(ThemeManager),
                    exception);
                return new ThemeTransitionResult(
                    transaction.TransitionId,
                    ThemeTransitionStatus.Committed,
                    transaction.PreparedState,
                    transaction.Preparation?.Diagnostics ?? Array.Empty<ThemeDiagnostic>(),
                    [new ThemeDiagnostic(
                        "ATMTHM7006",
                        ThemeDiagnosticSeverity.Warning,
                        nameof(ThemeManager),
                        "$",
                        $"Theme publish failed after commit: {exception.GetBaseException().Message}")],
                    null);
            }

            return CreateFailedResult(
                transaction,
                [new ThemeDiagnostic(
                    "ATMTHM7002",
                    ThemeDiagnosticSeverity.Error,
                    nameof(ThemeManager),
                    "$",
                    $"Theme transition failed before commit: {exception.GetBaseException().Message}")],
                exception);
        }
    }

    private void PrepareCommitPayload(ThemeTransaction transaction)
    {
        var snapshot = transaction.Preparation!.Snapshot!;
        transaction.PreparedState = new ThemeState(
            transaction.Request.ThemeId,
            snapshot.EffectiveConfig.Algorithms.Select(static algorithm => algorithm.Algorithm).ToArray(),
            snapshot.Appearance,
            snapshot.ContentFingerprint.Value,
            transaction.TransitionId);
        transaction.AddsResourceProvider = _rootContext is null;
        transaction.PreparedRootContext = _rootContext ??
                                          new ThemeContext(this, snapshot, 0);
        transaction.PreparedResourceProvider = transaction.PreparedRootContext.ResourceProvider;
    }

    private void CommitCore(ThemeTransaction transaction)
    {
        var preparation = transaction.Preparation!;
        var rootContext = transaction.PreparedRootContext!;
        if (!transaction.AddsResourceProvider)
        {
            rootContext.Commit(preparation.Snapshot!);
        }

        foreach (var scope in transaction.PreparedScopes)
        {
            scope.Context.Commit(scope.Snapshot);
        }

        _rootContext               = rootContext;
        _currentSnapshot           = preparation.Snapshot;
        _currentSnapshotKey        = preparation.SnapshotKey;
        _currentTheme              = transaction.PreparedState;
        _lastCommittedRequestKey   = transaction.RequestKey;
        _lastCommittedRequest      = transaction.Request;
    }

    private IReadOnlyList<ThemeDiagnostic> Publish(ThemeTransaction transaction)
    {
        var diagnostics = new List<ThemeDiagnostic>();
        var snapshot = transaction.Preparation!.Snapshot!;
        var avaloniaVariant = snapshot.Appearance == ThemeAppearance.Dark
            ? Avalonia.Styling.ThemeVariant.Dark
            : Avalonia.Styling.ThemeVariant.Light;

        ThemePublishBoundary.Dispatch(
            () =>
            {
                if (_application is not null)
                {
                    _application.RequestedThemeVariant = avaloniaVariant;
                }
            },
            this,
            diagnostics,
            "ApplicationThemeVariant");

        if (transaction.AddsResourceProvider)
        {
            ThemePublishBoundary.Dispatch(
                () => Resources.MergedDictionaries.Add(transaction.PreparedResourceProvider!),
                this,
                diagnostics,
                "ThemeResources");
        }

        // Publish all subtree variants before any context/resource notification.
        foreach (var scope in transaction.PreparedScopes)
        {
            if (_scopeGraph.TryGetNode(scope.Stamp.RegistrationId, out var node) &&
                node is not null)
            {
                ThemePublishBoundary.Dispatch(
                    () => diagnostics.AddRange(node.Provider.SetCommittedVariant(scope.Context)),
                    this,
                    diagnostics,
                    $"ThemeScope[{scope.Stamp.RegistrationId}].ThemeVariant");
            }
        }

        ThemePublishBoundary.Dispatch(
            () => diagnostics.AddRange(transaction.PreparedRootContext!.Publish(
                notifyResources: !transaction.AddsResourceProvider)),
            this,
            diagnostics,
            "RootThemeContext");

        for (var index = 0; index < transaction.PreparedScopes.Count; index++)
        {
            var scope = transaction.PreparedScopes[index];
            if (!_scopeGraph.TryGetNode(scope.Stamp.RegistrationId, out var node) ||
                node is null)
            {
                continue;
            }

            ThemePublishBoundary.Dispatch(
                () => diagnostics.AddRange(node.Provider.PublishCommittedContext(
                    scope.Context,
                    notifyResources: false)),
                this,
                diagnostics,
                $"ThemeScope[{scope.Stamp.RegistrationId}]");
        }

        ThemeEventDispatcher.Dispatch(
            ThemeChanged,
            this,
            new ThemeChangedEventArgs(
                transaction.Request,
                transaction.PreparedState!,
                diagnostics),
            diagnostics,
            nameof(ThemeChanged));
        return diagnostics.AsReadOnly();
    }

    private bool IsTransactionCurrent(ThemeTransaction transaction)
    {
        return transaction.Generation == _generation &&
               transaction.ScopeCapture is not null &&
               _scopeGraph.IsCurrent(transaction.ScopeCapture);
    }

    private ThemeTransitionResult CreateFailedResult(
        ThemeTransaction transaction,
        IReadOnlyList<ThemeDiagnostic> diagnostics,
        Exception? exception)
    {
        var publishDiagnostics = new List<ThemeDiagnostic>();
        ThemeEventDispatcher.Dispatch(
            ThemeChangeFailed,
            this,
            new ThemeChangeFailedEventArgs(transaction.Request, diagnostics, exception),
            publishDiagnostics,
            nameof(ThemeChangeFailed));
        return new ThemeTransitionResult(
            transaction.TransitionId,
            ThemeTransitionStatus.Failed,
            _currentTheme,
            diagnostics,
            publishDiagnostics,
            exception);
    }

    private ThemeTransitionResult CreateSupersededResult(ThemeTransaction transaction)
    {
        return new ThemeTransitionResult(
            transaction.TransitionId,
            ThemeTransitionStatus.Superseded,
            _currentTheme,
            transaction.Preparation?.Diagnostics ?? Array.Empty<ThemeDiagnostic>(),
            Array.Empty<ThemeDiagnostic>(),
            null);
    }

    private static Task<ThemeTransitionResult> WaitForCallerAsync(
        Task<ThemeTransitionResult> transaction,
        CancellationToken cancellationToken)
    {
        return cancellationToken.CanBeCanceled
            ? transaction.WaitAsync(cancellationToken)
            : transaction;
    }

    private long NextTransitionId()
    {
        return ++_nextTransitionId;
    }

    private async ValueTask EnterTransactionExecutionGateAsync()
    {
        lock (_transactionGate)
        {
            ObjectDisposedException.ThrowIf(_disposeRequested, this);
            _transactionExecutionUsers++;
        }

        try
        {
            await _transactionExecutionGate.WaitAsync().ConfigureAwait(true);
        }
        catch
        {
            var disposeNow = false;
            lock (_transactionGate)
            {
                _transactionExecutionUsers--;
                if (_disposeRequested && _transactionExecutionUsers == 0 && !_disposeCompleted)
                {
                    _disposeCompleted = true;
                    disposeNow = true;
                }
            }
            if (disposeNow)
            {
                DisposeCore();
            }
            throw;
        }
    }

    private void ExitTransactionExecutionGate()
    {
        _transactionExecutionGate.Release();
        var disposeNow = false;
        lock (_transactionGate)
        {
            _transactionExecutionUsers--;
            if (_disposeRequested && _transactionExecutionUsers == 0 && !_disposeCompleted)
            {
                _disposeCompleted = true;
                disposeNow = true;
            }
        }
        if (disposeNow)
        {
            DisposeCore();
        }
    }

    private void VerifyTransitionAccess()
    {
        if (!_checkTransitionAccess())
        {
            throw new InvalidOperationException("Theme transitions must be captured on the UI thread.");
        }
        ObjectDisposedException.ThrowIf(_disposeRequested, this);
        if (_loadingPackages)
        {
            throw new InvalidOperationException("Theme mutations cannot run during package loading.");
        }
    }

    internal void RegisterContextLease(ThemeContextLease lease)
    {
        ArgumentNullException.ThrowIfNull(lease);
        VerifyTransitionAccess();
        lock (_transactionGate)
        {
            ObjectDisposedException.ThrowIf(_disposeRequested, this);
            _contextLeases.Add(lease);
        }
    }

    internal void UnregisterContextLease(ThemeContextLease lease)
    {
        ArgumentNullException.ThrowIfNull(lease);
        lock (_transactionGate)
        {
            _contextLeases.Remove(lease);
        }
    }

    public void Dispose()
    {
        if (_loadingPackages)
        {
            throw new InvalidOperationException("ThemeManager cannot be disposed during package loading.");
        }
        if (!_checkTransitionAccess())
        {
            throw new InvalidOperationException("ThemeManager must be disposed on the UI thread.");
        }

        ThemeTransaction? queuedTransaction;
        var disposeNow = false;
        lock (_transactionGate)
        {
            if (_disposeRequested)
            {
                return;
            }

            _disposeRequested = true;
            _generation++;
            _pendingScopeUpdates.Clear();
            queuedTransaction = _queuedTransaction;
            _queuedTransaction = null;
            if (_transactionExecutionUsers == 0)
            {
                _disposeCompleted = true;
                disposeNow = true;
            }
        }

        queuedTransaction?.Complete(CreateSupersededResult(queuedTransaction));
        if (disposeNow)
        {
            DisposeCore();
        }
    }

    private void DisposeCore()
    {
        Debug.Assert(_disposeRequested);
        Debug.Assert(_disposeCompleted);
        Debug.Assert(_transactionExecutionUsers == 0);

        var application = _application;
        if (application?.PlatformSettings is { } settings)
        {
            settings.ColorValuesChanged -= HandleSystemColorValuesChanged;
        }

        foreach (var lease in _contextLeases.ToArray())
        {
            DisposeBoundary(lease.Dispose);
        }
        _contextLeases.Clear();

        DisposeBoundary(_scopeGraph.DisposeAll);
        DisposeBoundary(() => _startupPackageIndex?.Release());
        if (_rootContextStyle is { } rootContextStyle && application is not null)
        {
            DisposeBoundary(() => application.Styles.Remove(rootContextStyle));
        }
        _rootContextStyle = null;
        var applicationResources = _applicationResources;
        _applicationResources = null;
        if (applicationResources is not null)
        {
            DisposeBoundary(() => applicationResources.MergedDictionaries.Remove(Resources));
            if (application is not null)
            {
                DisposeBoundary(() => ((IResourceProvider)Resources).RemoveOwner(application));
                // ResourceDictionary stops removing child owners if a notification throws.
                // Snapshot our root providers so callbacks cannot interrupt sibling cleanup.
                foreach (var resource in Resources.MergedDictionaries.Concat(Resources.ThemeDictionaries.Values).ToArray())
                {
                    DisposeBoundary(() => resource.RemoveOwner(application));
                }
            }
        }
        DisposeBoundary(Resources.MergedDictionaries.Clear);
        DisposeBoundary(Resources.Clear);
        DisposeBoundary(Clear);

        _themeDefinitionLoadCache.Dispose();
        if (_themeSnapshotCache is { } snapshotCache)
        {
            DisposeBoundary(snapshotCache.Dispose);
        }
        _controlTokenDescriptors.Clear();
        _controlThemeAssetDescriptors.Clear();
        _controlThemesProviders.Clear();
        _themeDefinitionResolvers = Array.Empty<ThemeDefinitionRegistration>();
        _pendingScopeUpdates.Clear();
        _scopeUpdateProcessorRunning = false;

        _application = null;
        _applicationInitialized = false;
        _stagedThemeResources.Clear();
        _registeredControlThemesProviders.Clear();
        _controlThemeProviderBaseResources.Clear();
        _staticResourceProvider = null;
        _stagedSemanticControls.Clear();
        _startupRegistry = null;
        _startupPackageIndex = null;
        _startupResourceProvider = null;
        _packageSchemaFingerprints = null;
        _compiledThemeCatalog = null;
        _themeCatalogDiagnostics = Array.Empty<ThemeDiagnostic>();
        _themeCompiler = null;
        _themeSnapshotCache = null;
        _rootContext = null;
        _currentSnapshot = null;
        _currentSnapshotKey = null;
        _currentTheme = null;
        _lastCommittedRequest = null;
        _lastCommittedRequestKey = null;
        _followSystemLightRequest = null;
        _followSystemDarkRequest = null;

        ThemeChanged = null;
        ControlThemeResolved = null;
        ThemeChangeFailed = null;
        ThemeCatalogChanged = null;
        _transactionExecutionGate.Dispose();
    }

    private static void DisposeBoundary(Action action)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
        }
    }

    internal void EnsureRegistrationCapacity(
        int controlTokenCount,
        int controlThemeAssetCount,
        int controlThemesProviderCount)
    {
        EnsureListCapacity(_controlTokenDescriptors, controlTokenCount);
        EnsureListCapacity(_controlThemeAssetDescriptors, controlThemeAssetCount);
        EnsureListCapacity(_controlThemesProviders, controlThemesProviderCount);

    }

    private static void EnsureListCapacity<T>(List<T> list, int capacity)
    {
        if (list.Capacity < capacity)
        {
            list.Capacity = capacity;
        }
    }

    internal void RegisterControlThemesProvider(IControlThemesProvider controlThemesProvider)
    {
        ArgumentNullException.ThrowIfNull(controlThemesProvider);
        if (!_registeredControlThemesProviders.Any(provider =>
                ReferenceEquals(provider, controlThemesProvider)))
        {
            _registeredControlThemesProviders.Add(controlThemesProvider);
        }
        if (!_controlThemesProviders.Any(provider => ReferenceEquals(provider, controlThemesProvider)))
        {
            _controlThemesProviders.Add(controlThemesProvider);
        }
    }

    internal void RegisterControlTokenDescriptor(ControlTokenDescriptor descriptor)
    {
        _controlTokenDescriptors.Add(descriptor);
    }

    internal void RegisterControlThemeAssetDescriptor(ControlThemeAssetDescriptor descriptor)
    {
        _controlThemeAssetDescriptors.Add(descriptor);
    }

    /// <summary>
    /// 语义部件描述符登记：BuildCore 一次登记全部包贡献。SemanticParts 由其按需重建。
    /// </summary>
    internal void StageSemanticControlDescriptors(IReadOnlyList<ControlSemanticDescriptor> descriptors)
    {
        ArgumentNullException.ThrowIfNull(descriptors);
        _stagedSemanticControls.AddRange(descriptors);
    }

    internal ThemeScopeRegistration RegisterScope(
        ThemeConfigProvider provider,
        ThemeContext parentContext,
        ThemeConfig config,
        out ThemeScopeUpdateResult result)
    {
        VerifyTransitionAccess();
        var transitionId = NextTransitionId();
        var registration = _scopeGraph.Register(provider, parentContext, config);
        var request = CreateScopeRequest(config);
        var capture = _scopeGraph.CaptureSubtree(registration.RegistrationId);
        var preparation = PrepareScopeCapture(
            parentContext.Snapshot,
            capture,
            useLastValidConfig: false);
        if (!preparation.Success)
        {
            result = ThemeScopeUpdateResult.Failed(
                transitionId,
                request,
                CreateScopeState(parentContext.Snapshot, transitionId),
                preparation.Diagnostics,
                preparation.Exception);
            return registration;
        }

        if (!_scopeGraph.IsCurrent(capture))
        {
            result = ThemeScopeUpdateResult.Failed(
                transitionId,
                request,
                CreateScopeState(parentContext.Snapshot, transitionId),
                [new ThemeDiagnostic(
                    "ATMTHM8001",
                    ThemeDiagnosticSeverity.Error,
                    nameof(ThemeScopeGraph),
                    "$",
                    "Theme scope topology changed before the initial snapshot could be committed.")],
                null);
            return registration;
        }

        foreach (var staged in preparation.Snapshots)
        {
            staged.Context.Commit(staged.Snapshot);
            _scopeGraph.AcceptConfig(staged.Stamp.RegistrationId);
        }
        result = ThemeScopeUpdateResult.Succeeded(
            transitionId,
            request,
            CreateScopeState(registration.Context.Snapshot, transitionId));
        return registration;
    }

    internal void ReplaceScopeConfig(
        ThemeConfigProvider provider,
        ThemeConfig config)
    {
        VerifyTransitionAccess();
        if (!_scopeGraph.TryGetNode(provider, out var node))
        {
            throw new InvalidOperationException("The ThemeConfigProvider is not registered.");
        }

        _scopeGraph.ReplaceConfig(node!.RegistrationId, config);
        var startProcessor = false;
        lock (_transactionGate)
        {
            _pendingScopeUpdates[node.RegistrationId] = new PendingThemeScopeUpdate(
                provider,
                node.RegistrationId,
                node.ConfigRevision,
                config);
            if (CanStartPendingScopeUpdateProcessor())
            {
                _scopeUpdateProcessorRunning = true;
                startProcessor = true;
            }
        }

        if (startProcessor)
        {
            _ = ProcessPendingScopeUpdatesAsync();
        }
    }

    private void StartPendingScopeUpdateProcessor()
    {
        var startProcessor = false;
        lock (_transactionGate)
        {
            if (CanStartPendingScopeUpdateProcessor())
            {
                _scopeUpdateProcessorRunning = true;
                startProcessor = true;
            }
        }

        if (startProcessor)
        {
            _ = ProcessPendingScopeUpdatesAsync();
        }
    }

    private bool CanStartPendingScopeUpdateProcessor()
    {
        return !_disposeRequested &&
               !_scopeUpdateProcessorRunning &&
               _activeTransaction is null &&
               _catalogReloadOperationCount == 0 &&
               _pendingScopeUpdates.Count != 0;
    }

    private async Task ProcessPendingScopeUpdatesAsync()
    {
        while (true)
        {
            PendingThemeScopeUpdate pending;
            long generation;
            long transitionId;
            lock (_transactionGate)
            {
                if (_disposeRequested ||
                    _activeTransaction is not null ||
                    _catalogReloadOperationCount != 0 ||
                    _pendingScopeUpdates.Count == 0)
                {
                    _scopeUpdateProcessorRunning = false;
                    return;
                }

                pending = _pendingScopeUpdates.Values
                                              .OrderBy(static update => update.RegistrationId)
                                              .First();
                generation = _generation;
                transitionId = NextTransitionId();
            }

            var result = await ProcessScopeUpdateAsync(
                pending,
                generation,
                transitionId,
                pending.Provider.DispatchResult).ConfigureAwait(true);

            lock (_transactionGate)
            {
                if (_pendingScopeUpdates.TryGetValue(pending.RegistrationId, out var currentPending) &&
                    ReferenceEquals(currentPending, pending))
                {
                    if (!_scopeGraph.TryGetNode(pending.RegistrationId, out var currentNode) ||
                        !ReferenceEquals(currentNode!.Provider, pending.Provider) ||
                        (result.Status != ThemeTransitionStatus.Superseded &&
                         currentNode.ConfigRevision == pending.ConfigRevision))
                    {
                        _pendingScopeUpdates.Remove(pending.RegistrationId);
                    }
                }
            }
        }
    }

    private async Task<ThemeScopeUpdateResult> ProcessScopeUpdateAsync(
        PendingThemeScopeUpdate pending,
        long generation,
        long transitionId,
        Action<ThemeScopeUpdateResult> dispatchResult)
    {
        ArgumentNullException.ThrowIfNull(dispatchResult);
        await EnterTransactionExecutionGateAsync().ConfigureAwait(true);
        try
        {
            ThemeScopeUpdateResult result;
            lock (_transactionGate)
            {
                if (generation != _generation ||
                    !_scopeGraph.TryGetNode(pending.RegistrationId, out var current) ||
                    current!.ConfigRevision != pending.ConfigRevision ||
                    !ReferenceEquals(current.Provider, pending.Provider))
                {
                    result = ThemeScopeUpdateResult.Superseded(
                        transitionId,
                        CreateScopeRequest(pending.Config),
                        CurrentTheme ?? throw new InvalidOperationException("Root theme is not initialized."));
                    dispatchResult(result);
                    return result;
                }
            }

            result = ApplyScopeConfig(pending, generation, transitionId);
            dispatchResult(result);
            return result;
        }
        catch (Exception exception)
        {
            var result = ThemeScopeUpdateResult.Failed(
                transitionId,
                CreateScopeRequest(pending.Config),
                CurrentTheme ?? throw new InvalidOperationException("Root theme is not initialized."),
                [new ThemeDiagnostic(
                    "ATMTHM8003",
                    ThemeDiagnosticSeverity.Error,
                    nameof(ThemeManager),
                    "$",
                    $"Theme scope update failed: {exception.GetBaseException().Message}")],
                exception);
            dispatchResult(result);
            return result;
        }
        finally
        {
            ExitTransactionExecutionGate();
        }
    }

    private ThemeScopeUpdateResult ApplyScopeConfig(
        PendingThemeScopeUpdate pending,
        long generation,
        long transitionId)
    {
        if (!_scopeGraph.TryGetNode(pending.RegistrationId, out var node) ||
            !ReferenceEquals(node!.Provider, pending.Provider))
        {
            return ThemeScopeUpdateResult.Superseded(
                transitionId,
                CreateScopeRequest(pending.Config),
                CurrentTheme ?? throw new InvalidOperationException("Root theme is not initialized."));
        }

        var config = node.Config;
        var request = CreateScopeRequest(config);
        var capture = _scopeGraph.CaptureSubtree(node.RegistrationId);
        var parentSnapshot = ResolveParentSnapshot(capture.Nodes[0].Stamp.ParentRegistrationId);
        var preparation = PrepareScopeCapture(
            parentSnapshot,
            capture,
            useLastValidConfig: false);
        if (!preparation.Success)
        {
            return ThemeScopeUpdateResult.Failed(
                transitionId,
                request,
                CreateScopeState(node.Context.Snapshot, transitionId),
                preparation.Diagnostics,
                preparation.Exception);
        }
        var noOp = preparation.Snapshots.All(static staged =>
            SnapshotsEquivalent(staged.Context.Snapshot, staged.Snapshot));
        lock (_transactionGate)
        {
            if (generation != _generation || !_scopeGraph.IsCurrent(capture))
            {
                return ThemeScopeUpdateResult.Superseded(
                    transitionId,
                    request,
                    CreateScopeState(node.Context.Snapshot, transitionId));
            }

            if (noOp)
            {
                foreach (var staged in preparation.Snapshots)
                {
                    _scopeGraph.AcceptConfig(staged.Stamp.RegistrationId);
                }
                return ThemeScopeUpdateResult.NoOp(
                    transitionId,
                    request,
                    CreateScopeState(node.Context.Snapshot, transitionId));
            }

            foreach (var staged in preparation.Snapshots)
            {
                staged.Context.Commit(staged.Snapshot);
                _scopeGraph.AcceptConfig(staged.Stamp.RegistrationId);
            }
        }

        var publishDiagnostics = new List<ThemeDiagnostic>();
        foreach (var staged in preparation.Snapshots)
        {
            if (_scopeGraph.TryGetNode(staged.Stamp.RegistrationId, out var stagedNode) &&
                stagedNode is not null)
            {
                ThemePublishBoundary.Dispatch(
                    () => publishDiagnostics.AddRange(stagedNode.Provider.SetCommittedVariant(staged.Context)),
                    this,
                    publishDiagnostics,
                    $"ThemeScope[{staged.Stamp.RegistrationId}].ThemeVariant");
            }
        }

        for (var index = 0; index < preparation.Snapshots.Count; index++)
        {
            var staged = preparation.Snapshots[index];
            if (!_scopeGraph.TryGetNode(staged.Stamp.RegistrationId, out var stagedNode))
            {
                continue;
            }
            ThemePublishBoundary.Dispatch(
                () => publishDiagnostics.AddRange(stagedNode!.Provider.PublishCommittedContext(
                    staged.Context,
                    notifyResources: index == 0)),
                this,
                publishDiagnostics,
                $"ThemeScope[{staged.Stamp.RegistrationId}]");
        }

        return ThemeScopeUpdateResult.Succeeded(
            transitionId,
            request,
            CreateScopeState(preparation.Snapshots[0].Snapshot, transitionId),
            publishDiagnostics.AsReadOnly());
    }

    private static bool SnapshotsEquivalent(ThemeSnapshot left, ThemeSnapshot right)
    {
        return string.Equals(left.ThemeId, right.ThemeId, StringComparison.Ordinal) &&
               left.DefinitionRevision == right.DefinitionRevision &&
               left.RegistryRevision == right.RegistryRevision &&
               left.Appearance == right.Appearance &&
               left.EffectiveConfig.Equals(right.EffectiveConfig);
    }

    private static ThemeState CreateScopeState(ThemeSnapshot snapshot, long transitionId)
    {
        return new ThemeState(
            snapshot.ThemeId,
            snapshot.EffectiveConfig.Algorithms.Select(static algorithm => algorithm.Algorithm).ToArray(),
            snapshot.Appearance,
            snapshot.ContentFingerprint.Value,
            transitionId);
    }

    private ScopePreparation PrepareScopeCapture(
        ThemeSnapshot rootParentSnapshot,
        ThemeScopeCapture capture,
        bool useLastValidConfig)
    {
        if (capture.Nodes.Count == 0)
        {
            return ScopePreparation.Succeeded(Array.Empty<ThemeScopeStagedSnapshot>());
        }

        var staged = new ThemeScopeStagedSnapshot[capture.Nodes.Count];
        var snapshots = new Dictionary<long, ThemeSnapshot>(capture.Nodes.Count);
        for (var index = 0; index < capture.Nodes.Count; index++)
        {
            var node = capture.Nodes[index];
            ThemeSnapshot parentSnapshot;
            if (node.Stamp.ParentRegistrationId == 0)
            {
                parentSnapshot = rootParentSnapshot;
            }
            else if (!snapshots.TryGetValue(node.Stamp.ParentRegistrationId, out parentSnapshot!))
            {
                parentSnapshot = ResolveParentSnapshot(node.Stamp.ParentRegistrationId);
            }

            var config = useLastValidConfig ? node.LastValidConfig : node.Config;
            var compileResult = CompileScopeSnapshot(parentSnapshot, config);
            if (!compileResult.Success)
            {
                return ScopePreparation.Failed(
                    ConvertDiagnostics(compileResult.Diagnostics),
                    compileResult.Exception);
            }

            var snapshot = compileResult.Snapshot!;
            staged[index] = new ThemeScopeStagedSnapshot(node.Stamp, node.Context, snapshot);
            snapshots.Add(node.Stamp.RegistrationId, snapshot);
        }
        return ScopePreparation.Succeeded(Array.AsReadOnly(staged));
    }

    private ThemeCompileResult CompileScopeSnapshot(
        ThemeSnapshot parentSnapshot,
        ThemeConfig config)
    {
        if (config.Inherit &&
            config.Algorithms is null &&
            config.Tokens.Count == 0 &&
            config.Controls.Count == 0)
        {
            return new ThemeCompileResult(
                parentSnapshot,
                Array.Empty<ThemeDefinitionDiagnostic>(),
                null);
        }

        var normalized = ThemeConfigNormalizer.Normalize(config, parentSnapshot.Registry);
        if (!normalized.Success)
        {
            return new ThemeCompileResult(null, normalized.Diagnostics, null);
        }

        var defaults = config.Inherit
            ? ThemeCompiler.CreateDefinitionDefaults(
                parentSnapshot.Definition,
                parentSnapshot.Registry)
            : ThemeCompiler.CreateLibraryDefaults(parentSnapshot.Registry);
        var merge = ThemeConfigMerger.Merge(
            defaults,
            parentSnapshot.EffectiveConfig,
            normalized.Config!);
        if (merge.ChangeSet.IsEmpty)
        {
            return new ThemeCompileResult(
                parentSnapshot,
                Array.Empty<ThemeDefinitionDiagnostic>(),
                null);
        }

        var input = new ThemeCompileInput(
            parentSnapshot.Definition,
            parentSnapshot.DefinitionRevision,
            merge.EffectiveConfig,
            parentSnapshot.Registry,
            parentSnapshot,
            merge.ChangeSet);
        var key = ThemeSnapshotCacheKey.Create(input);
        return GetThemeSnapshotCache().GetOrCompile(key, input, GetThemeCompiler());
    }

    private ThemeSnapshot ResolveParentSnapshot(long parentRegistrationId)
    {
        if (parentRegistrationId == 0)
        {
            return RootContext.Snapshot;
        }
        if (_scopeGraph.TryGetNode(parentRegistrationId, out var parent))
        {
            return parent!.Context.Snapshot;
        }
        throw new InvalidOperationException(
            $"Parent theme scope '{parentRegistrationId}' is not active.");
    }

    private ThemeRequest CreateScopeRequest(ThemeConfig config)
    {
        return new ThemeRequest(
            CurrentTheme?.ThemeId ?? IThemeManager.DEFAULT_THEME_ID,
            config,
            ThemeTransitionReason.LocalConfigChanged);
    }

    private ValueTask<ThemeTransactionPreparation> PrepareThemeAsync(
        ThemeRequest request,
        ThemeSnapshot? currentSnapshot,
        CancellationToken cancellationToken)
    {
        Debug.Assert(cancellationToken == CancellationToken.None);
        if (_compiledThemeCatalog is null || _startupRegistry is null)
        {
            return ValueTask.FromResult(ThemeTransactionPreparation.Failed(
                [new ThemeDiagnostic(
                    "ATMTHM7006",
                    ThemeDiagnosticSeverity.Error,
                    nameof(ThemeManager),
                    "$",
                    "ThemeManager has not initialized its schema registry and compiled catalog.")]));
        }

        return ValueTask.FromResult(PrepareCompiledTheme(request, currentSnapshot));
    }

    private ThemeTransactionPreparation PrepareCompiledTheme(
        ThemeRequest request,
        ThemeSnapshot? currentSnapshot,
        CompiledThemeCatalog? catalog = null,
        ThemeSchemaRegistry? registry = null,
        ThemeConfigNormalizeResult? runtimeDefaultConfig = null,
        ThemeConfigNormalizeResult? initialConfig = null)
    {
        registry ??= _startupRegistry!;
        try
        {
            var entry = (catalog ?? _compiledThemeCatalog!).Get(request.ThemeId);
            var normalizedRuntime = request.Config is null
                ? runtimeDefaultConfig ?? _normalizedRuntimeDefaultConfig!
                : ThemeConfigNormalizer.Normalize(AddDefaultFont(request.Config)!, registry);
            if (!normalizedRuntime.Success)
            {
                return ThemeTransactionPreparation.Failed(
                    ConvertDiagnostics(normalizedRuntime.Diagnostics));
            }

            var defaults = ThemeCompiler.CreateDefinitionDefaults(
                entry.Definition,
                registry);
            var normalizedInitial = initialConfig ?? _normalizedInitialConfig!;
            if (!normalizedInitial.Success)
            {
                return ThemeTransactionPreparation.Failed(
                    ConvertDiagnostics(normalizedInitial.Diagnostics));
            }

            var effective = ThemeStartupCompilation.MergeEffectiveConfig(
                defaults,
                normalizedInitial.Config!,
                normalizedRuntime.Config!);
            var changeSet = currentSnapshot is null
                ? null
                : ThemeConfigMerger.Compare(currentSnapshot.EffectiveConfig, effective);
            var input = new ThemeCompileInput(
                entry.Definition,
                entry.Revision,
                effective,
                registry,
                currentSnapshot,
                changeSet);
            var key = ThemeSnapshotCacheKey.Create(input);
            if (_currentSnapshotKey is ThemeSnapshotCacheKey currentKey &&
                currentSnapshot is not null &&
                currentKey.Equals(key))
            {
                return ThemeTransactionPreparation.Succeeded(currentSnapshot, key);
            }

            var result = GetThemeSnapshotCache().GetOrCompile(key, input, GetThemeCompiler());
            return result.Success
                ? ThemeTransactionPreparation.Succeeded(
                    result.Snapshot!,
                    key,
                    ConvertDiagnostics(result.Diagnostics))
                : ThemeTransactionPreparation.Failed(
                    ConvertDiagnostics(result.Diagnostics),
                    result.Exception);
        }
        catch (Exception exception)
        {
            return ThemeTransactionPreparation.Failed(
                [new ThemeDiagnostic(
                    "ATMTHM7005",
                    ThemeDiagnosticSeverity.Error,
                    nameof(ThemeManager),
                    "$",
                    $"Theme preparation failed: {exception.GetBaseException().Message}")],
                exception);
        }
    }

    private (ThemeConfigNormalizeResult Runtime, ThemeConfigNormalizeResult Initial, ThemeCatalogLoadResult Catalog)
        PrepareRegistrationThemeInputs(ThemeSchemaRegistry registry, IReadOnlyList<ThemeDefinitionRegistration> resolvers)
    {
        var runtimeConfig = ThemeConfigNormalizer.Normalize(AddDefaultFont(null) ?? ThemeConfig.Empty, registry);
        var initialConfig = _initialRequest.Config is null
            ? runtimeConfig
            : ThemeConfigNormalizer.Normalize(AddDefaultFont(_initialRequest.Config)!, registry);
        var catalog = CompiledThemeCatalog.LoadInitial(registry, resolvers,
            new ThemeDefinitionResolveContext(_applicationId, _applicationDataRoot, false, 0), _themeDefinitionLoadCache);
        if (!catalog.Success)
        {
            throw new ThemeLoadException(CompiledThemeCatalog.Describe(catalog.Diagnostics), catalog.Exception);
        }
        return (runtimeConfig, initialConfig, catalog);
    }

    private ThemeConfig? AddDefaultFont(ThemeConfig? config) =>
        ThemeStartupCompilation.AddDefaultFont(config, FontFamily);

    private static IReadOnlyList<ThemeDiagnostic> ConvertDiagnostics(
        IReadOnlyList<Definitions.ThemeDefinitionDiagnostic> diagnostics)
    {
        var converted = new ThemeDiagnostic[diagnostics.Count];
        for (var index = 0; index < converted.Length; index++)
        {
            var diagnostic = diagnostics[index];
            converted[index] = new ThemeDiagnostic(
                diagnostic.Code,
                diagnostic.Severity == Definitions.ThemeDefinitionDiagnosticSeverity.Error
                    ? ThemeDiagnosticSeverity.Error
                    : ThemeDiagnosticSeverity.Warning,
                diagnostic.FilePath,
                diagnostic.Path,
                diagnostic.Message);
        }
        return Array.AsReadOnly(converted);
    }

    private ThemeCompiler GetThemeCompiler()
    {
        return _themeCompiler ??= new ThemeCompiler();
    }

    private ThemeSnapshotCache GetThemeSnapshotCache()
    {
        return _themeSnapshotCache ??= new ThemeSnapshotCache();
    }

    internal void PrepareRegistrationSchema()
    {
        if (_startupRegistry is null)
        {
            var registry = CreateStartupRegistry();
            var semantics = CreateSemanticRegistry(registry);
            if (_startupPackageIndex is null)
            {
                _ = new ControlThemeAssetManifest(registry);
            }
            else if (_controlThemeAssetDescriptors.Count != 0)
            {
                _ = new ControlThemeAssetManifest(registry, _controlThemeAssetDescriptors);
            }
            _startupRegistry = registry;
            SemanticParts = semantics;
            _stagedSemanticControls.Clear();
        }
    }

    /// <summary>
    /// 全部生成包携带的编译期 schema 指纹；null 表示至少一个包缺失（启动 Revision 走全量算法）。
    /// 必须在 PrepareRegistrationSchema 之前调用。
    /// </summary>
    internal void SetPackageSchemaFingerprints(IReadOnlyList<PackageSchemaFingerprint>? packageFingerprints) =>
        _packageSchemaFingerprints = packageFingerprints;

    internal ThemeSchemaRevision? StartupRegistryRevision => PackageReadUpdate?.Snapshot.RegistryRevision ?? _startupRegistry?.Revision;

    internal void ConfigureStartupPackages(IReadOnlyList<EnabledControlPackage> packages, StartupImage? image)
    {
        var manualRegistry = _controlTokenDescriptors.Count == 0 ? null : new ThemeSchemaRegistry(
            GeneratedThemeSchema.GetGlobalTokens(), _controlTokenDescriptors, GeneratedThemeSchema.GetAlgorithms());
        _startupPackageIndex = StartupPackageIndex.Create(packages, image, manualRegistry, _stagedSemanticControls, _controlThemeAssetDescriptors);
        _startupResourceProvider = new StartupResourceProvider(_startupPackageIndex, this);
    }

    internal void StageControlThemeResources(IControlThemesProvider provider, IReadOnlyList<ControlThemeResourceRegistration> resources)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(resources);
        // 提交一次：每个 provider 只登记一次（Builder 按完整排序键准备好资源）。Build() 每次新建 ThemeManager，
        // 表在本次提交内不会出现第二个输入，故直接登记，不做归并。Provider 必须在登记资源前进入注册表，
        // 因为静态资源表按已注册 provider 构建。
        RegisterControlThemesProvider(provider);
        _stagedThemeResources[provider] = resources.ToList();
    }

    private void MountStaticResources()
    {
        _startupResourceProvider ??= new StartupResourceProvider(StartupPackageIndex.Create([]), this);
        if (_startupResourceProvider is not null && !Resources.MergedDictionaries.Contains(_startupResourceProvider))
        {
            Resources.MergedDictionaries.Add(_startupResourceProvider);
        }
        // 即使没有任何包也先固定静态层，保证它相对 Token provider 的优先级稳定。
        if (_staticResourceProvider is null)
        {
            _staticResourceProvider = new StaticThemeResourceProvider(this);
            Resources.MergedDictionaries.Add(_staticResourceProvider);
        }
        _staticResourceProvider.CommitTable(PrepareStaticResourceTable());
        _controlThemesProviders.Clear();
    }

    private StaticThemeResourceProvider.ResourceTable PrepareStaticResourceTable()
    {
        if (_controlThemesProviders.Count == 0)
        {
            return _staticResourceProvider!.CurrentTable;
        }

        var resources = new List<IResourceProvider>();

        foreach (var provider in _registeredControlThemesProviders)
        {
            if (!_controlThemeProviderBaseResources.TryGetValue(provider, out var baseResources))
            {
                baseResources = provider.ControlThemes
                    .Where(static resource => resource is not DeferredControlThemeResourceProvider)
                    .ToArray();
                _controlThemeProviderBaseResources.Add(provider, baseResources);
            }
            var existingDeferred = provider.ControlThemes
                .OfType<DeferredControlThemeResourceProvider>()
                .ToDictionary(static resource => resource.AssetId, StringComparer.Ordinal);
            provider.ControlThemes.Clear();
            foreach (var resource in baseResources)
            {
                provider.ControlThemes.Add(resource);
            }
            if (_stagedThemeResources.TryGetValue(provider, out var registrations))
            {
                foreach (var registration in registrations)
                {
                    // 惰性挂载：注册工厂推迟到首次资源查找执行。wrapper 字典的构造即编译期
                    // populate（急切构建合并主题），直接执行会让启动为全部资产付费；代理保持
                    // 合并位置与查找顺序不变，空窗口为零消费资产零付费。
                    provider.ControlThemes.Add(existingDeferred.TryGetValue(registration.AssetId, out var existing)
                        ? existing
                        : new DeferredControlThemeResourceProvider(
                            registration.AssetId,
                            registration.Factory,
                            registration.ResourceKeys, provider.Id,
                            _controlThemeAssetDescriptors.FirstOrDefault(asset => asset.AssetId == registration.AssetId)?.ExportedThemes
                                .Select(export => export.ResourceKey).ToArray()));
                }
            }
            foreach (var resource in provider.ControlThemes)
            {
                resources.Add(resource);
            }
        }
        return _staticResourceProvider!.PrepareTable(resources);
    }

    private ThemeSchemaRegistry CreateStartupRegistry()
    {
        if (_startupPackageIndex is { } index)
        {
            return new ThemeSchemaRegistry(GeneratedThemeSchema.GetGlobalTokens(), index, GeneratedThemeSchema.GetAlgorithms());
        }
        var registry = _packageSchemaFingerprints is { } packageFingerprints
            ? new ThemeSchemaRegistry(
                GeneratedThemeSchema.GetGlobalTokens(),
                _controlTokenDescriptors,
                GeneratedThemeSchema.GetAlgorithms(),
                _controlThemeAssetDescriptors,
                GeneratedThemeSchema.GlobalSchemaFingerprint,
                packageFingerprints)
            : new ThemeSchemaRegistry(
                GeneratedThemeSchema.GetGlobalTokens(),
                _controlTokenDescriptors,
                GeneratedThemeSchema.GetAlgorithms(),
                _controlThemeAssetDescriptors);
        return registry;
    }

    private SemanticPartRegistry CreateSemanticRegistry(ThemeSchemaRegistry registry)
    {
        var semanticParts = _startupPackageIndex is { } index ? new SemanticPartRegistry(index) : _stagedSemanticControls.Count == 0
            ? SemanticParts
            : new SemanticPartRegistry(SemanticParts.Controls.Concat(_stagedSemanticControls));
        foreach (var semantic in _startupPackageIndex is not null ? _stagedSemanticControls : semanticParts.Controls)
        {
            var raw = new Schema.ControlTokenIdentity(semantic.Identity.Catalog, semantic.Identity.Id);
            if (!ControlTokenIdentityCanonicalizer.Matches(semantic.Identity, semantic.ControlType) ||
                (registry.TryGetControl(raw, out var token) && token.ControlType != semantic.ControlType) ||
                (registry.TryGetControl(semantic.ControlType, out token) && token.Identity != semantic.Identity))
            {
                throw new ThemeSchemaException($"Semantic Control '{semantic.Identity}' conflicts with the registered Token owner.");
            }
        }
        foreach (var asset in _controlThemeAssetDescriptors)
        {
            foreach (var binding in asset.SemanticThemeBindings)
            {
                if (!semanticParts.TryGetControlMetadata(binding.OwnerIdentity, out var semantic) ||
                    !semantic.Parts.Any(part => part.Theme is { } theme &&
                        theme.PropertyName == binding.PropertyName &&
                        theme.TargetType == binding.TargetType))
                {
                    throw new ThemeSchemaException($"Theme asset '{asset.AssetId}' has an invalid semantic binding '{binding.OwnerIdentity}.{binding.PropertyName}'.");
                }
            }
        }
        return semanticParts;
    }

    internal static ThemeAppearance ResolveSystemAppearance(Application application)
    {
        if (application.PlatformSettings is { } settings)
        {
            return settings.GetColorValues().ThemeVariant == PlatformThemeVariant.Dark
                ? ThemeAppearance.Dark
                : ThemeAppearance.Light;
        }
        return application.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark
            ? ThemeAppearance.Dark
            : ThemeAppearance.Light;
    }

    private void SubscribeSystemAppearance(Application application)
    {
        if (_followSystemLightRequest is not null && application.PlatformSettings is { } settings)
        {
            settings.ColorValuesChanged += HandleSystemColorValuesChanged;
        }
    }

    private async void HandleSystemColorValuesChanged(object? sender, PlatformColorValues values)
    {
        try
        {
            await ApplySystemAppearanceAsync(
                values.ThemeVariant == PlatformThemeVariant.Dark
                    ? ThemeAppearance.Dark
                    : ThemeAppearance.Light).ConfigureAwait(true);
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
        }
    }

    private sealed class ScopePreparation
    {
        private ScopePreparation(
            IReadOnlyList<ThemeScopeStagedSnapshot> snapshots,
            IReadOnlyList<ThemeDiagnostic> diagnostics,
            Exception? exception)
        {
            Snapshots   = snapshots;
            Diagnostics = diagnostics;
            Exception   = exception;
        }

        internal IReadOnlyList<ThemeScopeStagedSnapshot> Snapshots { get; }
        internal IReadOnlyList<ThemeDiagnostic> Diagnostics { get; }
        internal Exception? Exception { get; }
        internal bool Success => Exception is null &&
                                 Diagnostics.All(static diagnostic =>
                                     diagnostic.Severity != ThemeDiagnosticSeverity.Error);

        internal static ScopePreparation Succeeded(
            IReadOnlyList<ThemeScopeStagedSnapshot> snapshots)
        {
            return new ScopePreparation(
                snapshots,
                Array.Empty<ThemeDiagnostic>(),
                null);
        }

        internal static ScopePreparation Failed(
            IReadOnlyList<ThemeDiagnostic> diagnostics,
            Exception? exception)
        {
            return new ScopePreparation(
                Array.Empty<ThemeScopeStagedSnapshot>(),
                diagnostics,
                exception);
        }
    }

    private sealed class CatalogReloadPreparation
    {
        private CatalogReloadPreparation(
            CompiledThemeCatalog? catalog,
            ThemeTransaction? themeTransaction,
            bool snapshotChanged,
            bool hasChanges,
            IReadOnlyList<ThemeDiagnostic> diagnostics,
            Exception? exception)
        {
            Catalog          = catalog;
            ThemeTransaction = themeTransaction;
            SnapshotChanged  = snapshotChanged;
            HasChanges       = hasChanges;
            Diagnostics      = Array.AsReadOnly(diagnostics.ToArray());
            Exception        = exception;
        }

        internal CompiledThemeCatalog? Catalog { get; }
        internal ThemeTransaction? ThemeTransaction { get; }
        internal bool SnapshotChanged { get; }
        internal bool HasChanges { get; }
        internal IReadOnlyList<ThemeDiagnostic> Diagnostics { get; }
        internal Exception? Exception { get; }
        internal bool Success => Catalog is not null && ThemeTransaction is not null && Exception is null &&
                                 Diagnostics.All(static diagnostic =>
                                     diagnostic.Severity != ThemeDiagnosticSeverity.Error);

        internal static CatalogReloadPreparation Succeeded(
            CompiledThemeCatalog catalog,
            ThemeTransaction transaction,
            bool snapshotChanged,
            bool hasChanges,
            IReadOnlyList<ThemeDiagnostic> diagnostics)
        {
            return new CatalogReloadPreparation(
                catalog,
                transaction,
                snapshotChanged,
                hasChanges,
                diagnostics,
                null);
        }

        internal static CatalogReloadPreparation Failed(
            IReadOnlyList<ThemeDiagnostic> diagnostics,
            Exception? exception)
        {
            return new CatalogReloadPreparation(
                null,
                null,
                false,
                false,
                diagnostics,
                exception);
        }
    }
}
