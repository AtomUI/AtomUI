using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using AtomUI.Startup.Data;
using AtomUI.Theme;
using Avalonia;

namespace AtomUI.Registration;

/// <summary>
/// Generated, immutable package definition. Enabling it records declarations only.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ControlPackageDefinition
{
    private readonly Lazy<StartupPackageData> _data;
    private readonly Func<string, ControlRegistrationFragmentAttribute?> _resolve;
    private readonly Func<string, bool> _contains;
    private readonly Func<string, bool>? _containsAsset;
    private readonly Action<IAtomUIBuilder>? _declare;
    private readonly Action? _warmSelection;

    /// <summary>
    /// Creates a package definition in which every asset of the manifest counts as part of the application,
    /// and that does not warm fragment selection in the background.
    /// </summary>
    public ControlPackageDefinition(string id, string data,
        Func<string, ControlRegistrationFragmentAttribute?> resolve, Func<string, bool> contains,
        Action<IAtomUIBuilder>? declare = null) : this(id, data, resolve, contains, declare, null)
    {
    }

    /// <summary>
    /// Creates a package definition that does not warm fragment selection in the background.
    /// <paramref name="containsAsset" /> decides per asset ID whether the asset belongs to the application;
    /// pass <see langword="null" /> to count every asset.
    /// </summary>
    public ControlPackageDefinition(string id, string data,
        Func<string, ControlRegistrationFragmentAttribute?> resolve, Func<string, bool> contains,
        Action<IAtomUIBuilder>? declare, Func<string, bool>? containsAsset)
        : this(id, data, resolve, contains, declare, containsAsset, null)
    {
    }

    /// <summary>
    /// Generated package entry. <paramref name="warmSelection" /> may only prepare immutable selection
    /// data on a worker thread; it must not activate fragments, resources or lifecycle hooks.
    /// It must be safe alongside synchronous selection after a preparation timeout.
    /// The five- and six-argument constructors do not opt into background selection warming.
    /// </summary>
    public ControlPackageDefinition(string id, string data,
        Func<string, ControlRegistrationFragmentAttribute?> resolve, Func<string, bool> contains,
        Action<IAtomUIBuilder>? declare, Func<string, bool>? containsAsset, Action? warmSelection)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(resolve);
        ArgumentNullException.ThrowIfNull(contains);
        Id = id;
        EncodedManifest = data;
        _resolve = resolve;
        _contains = contains;
        _declare = declare;
        _containsAsset = containsAsset;
        _warmSelection = warmSelection;
        _data = new Lazy<StartupPackageData>(() =>
        {
            var manifest = StartupManifestCodec.Decode(data);
            if (manifest.Id != id)
            {
                throw new InvalidDataException("Startup package definition and manifest identities differ.");
            }
            return manifest;
        });
    }

    /// <summary>
    /// Identity of the package. It must equal the identity encoded in the manifest, which is checked when the
    /// manifest is decoded, and the builder tracks the package by it.
    /// </summary>
    public string Id { get; }
    internal string EncodedManifest { get; }
    internal StartupPackageData Data => _data.Value;
    internal bool ContainsAsset(string id) => _containsAsset?.Invoke(id) ?? true;
    internal bool Contains(string key) => _contains(key);
    internal ControlRegistrationFragmentAttribute? Resolve(string key) => _resolve(key);
    internal void WarmSelection() => _warmSelection?.Invoke();

    /// <summary>
    /// Enables the package on <paramref name="builder" />. It runs <paramref name="configure" />, enables the
    /// dependencies declared with <c>DependsOn</c>, and records the package's declarations and lifecycle
    /// hooks. Neither kind of hook runs during this call. The builder is returned for chaining.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="builder" /> is not an <see cref="AtomUIBuilder" />.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the package is already enabled or is being enabled. When called from inside a
    /// <c>DependsOn</c> action, a package that is already enabled is skipped instead.
    /// </exception>
    public IAtomUIBuilder Register(IAtomUIBuilder builder, Action<ControlPackageOptions>? configure = null) =>
        RegisterCore(builder, configure, false);

    /// <summary>
    /// Enables the package like <c>Register</c>, except that a package which is already enabled is left as it
    /// is: the builder is returned without running <paramref name="configure" />, so the options given to the
    /// first call apply.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="builder" /> is not an <see cref="AtomUIBuilder" />.
    /// </exception>
    public IAtomUIBuilder Ensure(IAtomUIBuilder builder, Action<ControlPackageOptions>? configure = null) =>
        RegisterCore(builder, configure, true);

    private IAtomUIBuilder RegisterCore(IAtomUIBuilder builder, Action<ControlPackageOptions>? configure, bool ensure)
    {
        ArgumentNullException.ThrowIfNull(builder);
        if (builder is not AtomUIBuilder root)
        {
            throw new ArgumentException("Control packages require the AtomUI application builder.", nameof(builder));
        }
        var state = root.ControlRegistrationState;
        if ((ensure || state.IsDependencyRegistration) && state.IsStaged(Id))
        {
            return builder;
        }
        var started = StartupEventSource.Log.IsEnabled() ? Stopwatch.GetTimestamp() : 0;
        try
        {
            state.Enter(Id);
            var options = new ControlPackageOptions();
            configure?.Invoke(options);
            foreach (var dependency in options.Dependencies)
            {
                state.EnableDependency(dependency, builder);
            }
            _declare?.Invoke(builder);
            root.ThemeManagerBuilder.AddPackageDeclaration(new EnabledControlPackage(this, options));
            state.Stage(Id);
            return builder;
        }
        catch (Exception error)
        {
            state.Fail(error);
            throw;
        }
        finally
        {
            if (started != 0)
            {
                StartupEventSource.Log.PackageTiming(Id, PackagePhase.Enable, Stopwatch.GetElapsedTime(started).TotalMilliseconds);
            }
        }
    }
}

/// <summary>
/// Package dependency declarations and the two supported runtime lifecycle hooks.
/// </summary>
public sealed class ControlPackageOptions
{
    private readonly List<Action<IAtomUIBuilder>> _dependencies = [];
    private readonly List<Action<ControlPackageContext>> _commit = [];
    private readonly List<Action<ControlPackageContext>> _firstUse = [];

    internal IReadOnlyList<Action<IAtomUIBuilder>> Dependencies => _dependencies;
    internal IReadOnlyList<Action<ControlPackageContext>> Commit => _commit;
    internal IReadOnlyList<Action<ControlPackageContext>> FirstUse => _firstUse;

    /// <summary>
    /// Declares a package that must be enabled before this one. <paramref name="enable" /> is called with the
    /// builder while this package is enabled, before its own declarations and in the order the dependencies
    /// were added. Pass the builder extension that enables the dependency, for example
    /// <c>builder.UseDesktopControls()</c>. A dependency that is already enabled is skipped.
    /// </summary>
    /// <returns>
    /// This object, for chaining.
    /// </returns>
    public ControlPackageOptions DependsOn(Action<IAtomUIBuilder> enable)
    {
        ArgumentNullException.ThrowIfNull(enable);
        _dependencies.Add(enable);
        return this;
    }

    /// <summary>
    /// Adds an initializer that runs once, when the application commits the enabled packages. That is after
    /// every package was enabled on the builder and before any first-use initializer. Use it for state that
    /// must exist before any control or resource of the package is used, and pass disposables to
    /// <see cref="ControlPackageContext.Own{T}(T)" /> to keep them for the package's lifetime. An exception the
    /// initializer throws is rethrown by every later commit or first use of the package.
    /// </summary>
    /// <returns>
    /// This object, for chaining.
    /// </returns>
    public ControlPackageOptions OnCommit(Action<ControlPackageContext> initialize)
    {
        ArgumentNullException.ThrowIfNull(initialize);
        _commit.Add(initialize);
        return this;
    }

    /// <summary>
    /// Adds an initializer that runs once, the first time a control or a resource of the package is used.
    /// Reading package metadata does not trigger it. It runs after the commit initializers, and an exception
    /// it throws is rethrown by every later first use of the package.
    /// </summary>
    /// <returns>
    /// This object, for chaining.
    /// </returns>
    public ControlPackageOptions OnFirstUse(Action<ControlPackageContext> initialize)
    {
        ArgumentNullException.ThrowIfNull(initialize);
        _firstUse.Add(initialize);
        return this;
    }
}

/// <summary>
/// What the commit and first-use initializers of a package receive: the application, its theme manager,
/// and a way to hand disposables to the package so that they are disposed when the package is released.
/// </summary>
public sealed class ControlPackageContext
{
    private readonly Action<IDisposable>? _own;
    internal ControlPackageContext(Application application, IThemeManager themeManager, Action<IDisposable>? own = null)
    {
        Application = application;
        ThemeManager = themeManager;
        _own = own;
    }

    /// <summary>
    /// The application that is committing the package or using it for the first time.
    /// </summary>
    public Application Application { get; }
    /// <summary>
    /// The theme manager of that application.
    /// </summary>
    public IThemeManager ThemeManager { get; }

    /// <summary>
    /// Transfers a service or subscription to the package's application lifetime.
    /// </summary>
    public T Own<T>(T resource) where T : class, IDisposable
    {
        ArgumentNullException.ThrowIfNull(resource);
        if (_own is null)
        {
            resource.Dispose();
            throw new InvalidOperationException("Package resource ownership requires an active lifecycle context.");
        }
        _own(resource);
        return resource;
    }
}

internal sealed class EnabledControlPackage
{
    private readonly Action<ControlPackageContext>[] _commit;
    private readonly Action<ControlPackageContext>[] _firstUse;
    private readonly object _firstUseGate = new();
    private WeakReference<Application>? _application;
    private WeakReference<IThemeManager>? _themeManager;
    private bool _committed;
    private bool _committing;
    private bool _released;
    private readonly List<IDisposable> _resources = [];
    private ExceptionDispatchInfo? _failure;
    private bool _used;
    private bool _initializing;

    internal EnabledControlPackage(ControlPackageDefinition definition, ControlPackageOptions options)
    {
        Definition = definition;
        _commit = options.Commit.ToArray();
        _firstUse = options.FirstUse.ToArray();
    }

    internal ControlPackageDefinition Definition { get; }

    internal void PrepareContext(Application application, IThemeManager themeManager)
    {
        ArgumentNullException.ThrowIfNull(application);
        ArgumentNullException.ThrowIfNull(themeManager);
        lock (_firstUseGate)
        {
            if (_released || _application is not null || _themeManager is not null)
            {
                throw new InvalidOperationException("Control package lifecycle context is already assigned: " + Definition.Id);
            }
            _application = new WeakReference<Application>(application);
            _themeManager = new WeakReference<IThemeManager>(themeManager);
        }
    }

    internal void Commit(ControlPackageContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        lock (_firstUseGate)
        {
            if (_released)
            {
                throw new InvalidOperationException("Control package has been released: " + Definition.Id);
            }
            if (_application is null || _themeManager is null)
            {
                PrepareContext(context.Application, context.ThemeManager);
            }
            _failure?.Throw();
            if (_committed)
            {
                return;
            }
            if (_committing)
            {
                throw new InvalidOperationException("Recursive control package commit: " + Definition.Id);
            }
            _committing = true;
            var started = StartupEventSource.Log.IsEnabled() ? Stopwatch.GetTimestamp() : 0;
            try
            {
                foreach (var initialize in _commit)
                {
                    initialize(new ControlPackageContext(context.Application, context.ThemeManager, Own));
                }
                _committed = true;
            }
            catch (Exception error)
            {
                _failure = ExceptionDispatchInfo.Capture(error);
                throw;
            }
            finally
            {
                _committing = false;
                if (started != 0)
                {
                    StartupEventSource.Log.PackageTiming(Definition.Id, PackagePhase.Commit, Stopwatch.GetElapsedTime(started).TotalMilliseconds);
                }
            }
        }
    }

    internal void FirstUse()
    {
        // Terminal state: once the first-use initializers completed, nothing can fail, recurse or
        // wait for the lifecycle context any more, and a released package returns early as well.
        if (Volatile.Read(ref _used))
        {
            return;
        }
        lock (_firstUseGate)
        {
            if (_released)
            {
                return;
            }
            _failure?.Throw();
            if (_used)
            {
                return;
            }
            if (_initializing)
            {
                throw new InvalidOperationException("Recursive package first-use initialization: " + Definition.Id);
            }
            // Metadata-only framework queries must use their explicit no-first-use path.
            // Actual consumption cannot return a descriptor before its lifecycle context exists.
            if (_application is not { } applicationReference || !applicationReference.TryGetTarget(out var application) ||
                _themeManager is not { } managerReference || !managerReference.TryGetTarget(out var themeManager))
            {
                throw new InvalidOperationException("Control package was consumed before its lifecycle context was ready: " + Definition.Id);
            }
            Commit(new ControlPackageContext(application, themeManager));
            _initializing = true;
            var started = StartupEventSource.Log.IsEnabled() ? Stopwatch.GetTimestamp() : 0;
            try
            {
                foreach (var initialize in _firstUse)
                {
                    initialize(new ControlPackageContext(application, themeManager, Own));
                }
                Volatile.Write(ref _used, true);
            }
            catch (Exception error)
            {
                _failure = ExceptionDispatchInfo.Capture(error);
                throw;
            }
            finally
            {
                _initializing = false;
                if (started != 0)
                {
                    StartupEventSource.Log.PackageTiming(Definition.Id, PackagePhase.FirstUse, Stopwatch.GetElapsedTime(started).TotalMilliseconds);
                }
            }
        }
    }

    internal void Release()
    {
        lock (_firstUseGate)
        {
            if (_released)
            {
                return;
            }
            _released = true;
            _application = null;
            _themeManager = null;
            _failure = null;
            Array.Clear(_commit);
            Array.Clear(_firstUse);
            List<Exception>? failures = null;
            for (var index = _resources.Count - 1; index >= 0; index--)
            {
                try
                {
                    _resources[index].Dispose();
                }
                catch (Exception error)
                {
                    (failures ??= []).Add(error);
                }
            }
            _resources.Clear();
            if (failures is not null)
            {
                throw new AggregateException("Control package resource disposal failed: " + Definition.Id, failures);
            }
        }
    }

    private void Own(IDisposable resource)
    {
        lock (_firstUseGate)
        {
            if (_released)
            {
                resource.Dispose();
                throw new ObjectDisposedException(Definition.Id);
            }
            if (!_resources.Contains(resource, ReferenceEqualityComparer.Instance))
            {
                _resources.Add(resource);
            }
        }
    }
}
