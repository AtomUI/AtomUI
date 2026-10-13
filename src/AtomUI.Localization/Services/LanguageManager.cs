using System.Collections.ObjectModel;
using Avalonia.Threading;

namespace AtomUI.Localization;

internal sealed class LanguageManager : ILanguageManager, IDisposable
{
    private readonly LanguageContext _context;
    private IReadOnlyDictionary<LanguageTag, LanguageSnapshot> _snapshots;
    private IReadOnlyDictionary<LanguageTag, LanguageDefinition> _definitions;
    private ReadOnlyCollection<LanguageDefinition> _supportedLanguages;
    private readonly LanguageResourceProvider _resourceProvider;
    private readonly Func<bool> _checkAccess;
    private int _disposed;

    internal LanguageManager(
        LanguageContext context,
        IReadOnlyDictionary<LanguageTag, LanguageSnapshot> snapshots,
        IReadOnlyList<LanguageDefinition> supportedLanguages,
        LanguageResourceProvider resourceProvider,
        Func<bool>? checkAccess = null)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        ArgumentNullException.ThrowIfNull(snapshots);
        ArgumentNullException.ThrowIfNull(supportedLanguages);
        _resourceProvider = resourceProvider ?? throw new ArgumentNullException(nameof(resourceProvider));
        _checkAccess = checkAccess ?? (static () => Dispatcher.UIThread.CheckAccess());

        if (supportedLanguages.Count == 0)
        {
            throw new LanguageConfigurationException("At least one supported language is required.");
        }

        var definitionArray = new LanguageDefinition[supportedLanguages.Count];
        var definitionsByTag = new Dictionary<LanguageTag, LanguageDefinition>();
        var snapshotCopies = new Dictionary<LanguageTag, LanguageSnapshot>();
        for (var index = 0; index < supportedLanguages.Count; index++)
        {
            var definition = supportedLanguages[index] ?? throw new LanguageConfigurationException(
                "A supported language definition cannot be null.");
            if (!definitionsByTag.TryAdd(definition.Tag, definition))
            {
                throw new LanguageConfigurationException(
                    $"Language definition '{definition.Tag.Value}' is registered more than once.");
            }
            if (!snapshots.TryGetValue(definition.Tag, out var snapshot))
            {
                throw new LanguageConfigurationException(
                    $"Supported language '{definition.Tag.Value}' has no prebuilt Snapshot.");
            }
            if (snapshot.RequestedLanguage != definition.Tag)
            {
                throw new LanguageConfigurationException(
                    $"Snapshot '{snapshot.RequestedLanguage.Value}' cannot be registered for language " +
                    $"'{definition.Tag.Value}'.");
            }

            definitionArray[index] = definition;
            snapshotCopies.Add(definition.Tag, snapshot);
        }

        var currentLanguage = context.Current.State.CurrentLanguage;
        if (!definitionsByTag.ContainsKey(currentLanguage))
        {
            throw new LanguageConfigurationException(
                $"Initial language '{currentLanguage.Value}' is not supported.");
        }

        _supportedLanguages = Array.AsReadOnly(definitionArray);
        _definitions = new ReadOnlyDictionary<LanguageTag, LanguageDefinition>(definitionsByTag);
        _snapshots = new ReadOnlyDictionary<LanguageTag, LanguageSnapshot>(snapshotCopies);
    }

    public LanguageState Current => _context.Current.State;
    internal LanguageCatalogRegistry Registry => _context.Registry;
    internal IReadOnlyDictionary<LanguageTag, LanguageSnapshot> Snapshots => Volatile.Read(ref _snapshots);

    public IReadOnlyList<LanguageDefinition> SupportedLanguages => Volatile.Read(ref _supportedLanguages);

    public event EventHandler<LanguageChangedEventArgs>? LanguageChanged;

    public LanguageChangeResult ChangeLanguage(LanguageTag language)
    {
        VerifyAccess();
        ThrowIfDisposed();
        if (language == default)
        {
            throw new ArgumentException("A valid language tag is required.", nameof(language));
        }
        var definitions = Volatile.Read(ref _definitions);
        if (!definitions.TryGetValue(language, out var definition))
        {
            throw new LanguageNotSupportedException(language);
        }

        var oldRevision = _context.Current;
        if (oldRevision.State.CurrentLanguage == language)
        {
            return LanguageChangeResult.NoOp(oldRevision.State);
        }

        var newState = new LanguageState(
            language,
            definition.FormattingCulture,
            definition.TextDirection,
            checked(oldRevision.State.Revision + 1));
        var snapshots = Volatile.Read(ref _snapshots);
        var newRevision = new LanguageRevision(snapshots[language], newState);
        var result = LanguageChangeResult.Committed(oldRevision.State, newState);

        _context.Publish(newRevision);
        PublishResourcesChanged();
        PublishLanguageChanged(new LanguageChangedEventArgs(result));
        return result;
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 0)
        {
            LanguageChanged = null;
        }
    }

    private void VerifyAccess()
    {
        if (!_checkAccess())
        {
            throw new InvalidOperationException("Language changes must be committed on the UI thread.");
        }
    }

    private void ThrowIfDisposed()
    {
        if (Volatile.Read(ref _disposed) != 0)
        {
            throw new InvalidOperationException("LanguageManager has been disposed.");
        }
    }

    /// <summary>
    /// 分级启动：Stage 2 演化后的资源变更通知（合并单次）。既有键值不变，重估无视觉变化；
    /// Stage 2 才可解析的键（后置语言包/目录）对已建立的 DynamicResource 即时生效。
    /// 失败语义与语言切换路径一致（记日志继续，不上抛）。
    /// </summary>
    internal void PublishEvolvedResourcesChanged() => PublishResourcesChanged();

    internal LanguageEvolution PrepareEvolution(
        LanguageCatalogRegistry registry,
        IReadOnlyDictionary<LanguageTag, LanguageSnapshot> snapshots,
        IReadOnlyList<LanguageDefinition> supportedLanguages)
    {
        VerifyAccess();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(snapshots);
        ArgumentNullException.ThrowIfNull(supportedLanguages);
        if (supportedLanguages.Count == 0)
        {
            throw new LanguageConfigurationException("At least one supported language is required.");
        }

        var definitionArray = new LanguageDefinition[supportedLanguages.Count];
        var definitionsByTag = new Dictionary<LanguageTag, LanguageDefinition>();
        var snapshotCopies = new Dictionary<LanguageTag, LanguageSnapshot>(supportedLanguages.Count);
        for (var index = 0; index < supportedLanguages.Count; index++)
        {
            var definition = supportedLanguages[index] ?? throw new LanguageConfigurationException(
                "A supported language definition cannot be null.");
            if (!definitionsByTag.TryAdd(definition.Tag, definition))
            {
                throw new LanguageConfigurationException(
                    $"Language definition '{definition.Tag.Value}' is registered more than once.");
            }
            if (!snapshots.TryGetValue(definition.Tag, out var snapshot))
            {
                throw new LanguageConfigurationException(
                    $"Evolved localization snapshots do not contain supported language '{definition.Tag.Value}'.");
            }
            if (snapshot.RequestedLanguage != definition.Tag)
            {
                throw new LanguageConfigurationException(
                    $"Snapshot '{snapshot.RequestedLanguage.Value}' cannot be registered for language " +
                    $"'{definition.Tag.Value}'.");
            }
            definitionArray[index] = definition;
            snapshotCopies.Add(definition.Tag, snapshot);
        }
        var expectedView = _context.Capture();
        var currentLanguage = expectedView.Revision.State.CurrentLanguage;
        if (!definitionsByTag.ContainsKey(currentLanguage))
        {
            throw new LanguageConfigurationException(
                $"Evolved supported languages do not contain the current language '{currentLanguage.Value}'.");
        }
        var currentDefinition = definitionsByTag[currentLanguage];
        // 同一个 language tag 也可能在 Stage 2 获得显式 Culture/方向定义；当前状态须与新定义一致。
        // 保持既有语言切换计数，演化代际由 LanguageReadView 身份表达。
        var evolvedState = new LanguageState(currentLanguage, currentDefinition.FormattingCulture,
            currentDefinition.TextDirection, expectedView.Revision.State.Revision);

        return new LanguageEvolution(
            registry,
            new ReadOnlyDictionary<LanguageTag, LanguageSnapshot>(snapshotCopies),
            new ReadOnlyDictionary<LanguageTag, LanguageDefinition>(definitionsByTag),
            Array.AsReadOnly(definitionArray),
            expectedView,
            new LanguageReadView(registry, new LanguageRevision(snapshotCopies[currentLanguage], evolvedState)));
    }

    internal void CommitEvolution(LanguageEvolution evolution)
    {
        ValidateEvolution(evolution);
        CommitEvolutionCore(evolution);
    }

    internal void ValidateEvolution(LanguageEvolution evolution)
    {
        ArgumentNullException.ThrowIfNull(evolution);
        VerifyAccess();
        ThrowIfDisposed();
        if (!ReferenceEquals(_context.Capture(), evolution.ExpectedView))
        {
            throw new LanguageConfigurationException("Language state changed while startup evolution was being prepared.");
        }
    }

    internal void CommitEvolutionCore(LanguageEvolution evolution)
    {
        _context.Commit(evolution.PreparedView);
        Volatile.Write(
            ref _definitions,
            evolution.Definitions);
        Volatile.Write(ref _supportedLanguages, evolution.SupportedLanguages);
        Volatile.Write(ref _snapshots, evolution.Snapshots);
    }

    private void PublishResourcesChanged()
    {
        try
        {
            _resourceProvider.PublishResourcesChanged();
        }
        catch (Exception exception)
        {
            LocalizationLogger.LogPublishFailure(
                this,
                nameof(LanguageResourceProvider),
                exception);
        }
    }

    private void PublishLanguageChanged(LanguageChangedEventArgs args)
    {
        var handlers = LanguageChanged;
        if (handlers is null)
        {
            return;
        }

        foreach (var invocation in handlers.GetInvocationList())
        {
            if (invocation is not EventHandler<LanguageChangedEventArgs> handler)
            {
                continue;
            }

            try
            {
                handler(this, args);
            }
            catch (Exception exception)
            {
                LocalizationLogger.LogPublishFailure(
                    this,
                    nameof(LanguageChanged),
                    exception);
            }
        }
    }
}

internal sealed record LanguageEvolution(
    LanguageCatalogRegistry Registry,
    IReadOnlyDictionary<LanguageTag, LanguageSnapshot> Snapshots,
    IReadOnlyDictionary<LanguageTag, LanguageDefinition> Definitions,
    ReadOnlyCollection<LanguageDefinition> SupportedLanguages,
    LanguageReadView ExpectedView,
    LanguageReadView PreparedView);
