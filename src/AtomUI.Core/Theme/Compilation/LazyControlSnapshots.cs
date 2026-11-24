using AtomUI.Theme.Configuration;
using AtomUI.Theme.Derivation;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Schema;

namespace AtomUI.Theme.Compilation;

/// <summary>
/// The read-only global compilation input shared by lazy control cells: the evaluation unit's inputs and the global
/// and language results of its compilation. It never holds the parent snapshot.
/// </summary>
internal sealed class LazyControlSnapshotSource
{
    private readonly ThemeSchemaRegistry _registry;
    private readonly ThemeCompiler.UnitInputs _unit;
    private readonly int _estimatedRetainedBytes;
    private readonly object _builderGate = new();
    private DesignToken? _globalBuilder;
    private LanguageDesignToken? _languageBuilder;
    private readonly IReadOnlyDictionary<PresetPrimaryColor, PaletteInfo> _globalPalettes;
    private readonly TokenValueTable _globalValues;
    private readonly TokenValueTable _languageValues;
    private readonly IReadOnlyDictionary<object, object?> _globalResources;
    private readonly WeakReference<ControlCompilationCache>? _cache;
    private readonly GlobalCompilationCacheKey? _globalKey;
    private ControlGlobalTokenPool? _globalPool;

    internal LazyControlSnapshotSource(
        ThemeCompileInput input,
        ThemeCompiler.UnitInputs unit,
        DesignToken? globalBuilder,
        LanguageDesignToken? languageBuilder,
        TokenValueTable globalValues,
        TokenValueTable languageValues,
        IReadOnlyDictionary<object, object?> globalResources,
        IReadOnlyDictionary<PresetPrimaryColor, PaletteInfo> globalPalettes,
        ControlCompilationCache? cache)
    {
        ArgumentNullException.ThrowIfNull(input);
        _registry        = input.Registry;
        _unit            = unit;
        _globalBuilder   = globalBuilder;
        _languageBuilder = languageBuilder;
        _globalValues    = globalValues;
        _languageValues  = languageValues;
        _globalResources = globalResources;
        _globalPalettes  = globalPalettes;
        _cache           = cache is null ? null : new WeakReference<ControlCompilationCache>(cache);
        _globalKey       = cache is null ? null : GlobalCompilationCacheKey.Create(input);
        _estimatedRetainedBytes = (int)Math.Min(int.MaxValue, ThemeRetainedBytesEstimator.EstimateLazySource(
            globalValues, globalResources, globalPalettes, 1, _globalKey?.EstimatedRetainedBytes ?? 0));
    }

    internal long EstimatedRetainedBytes => _estimatedRetainedBytes;

    private (DesignToken Global, LanguageDesignToken Language) GetBuilders()
    {
        if (Volatile.Read(ref _globalBuilder) is { } builder && Volatile.Read(ref _languageBuilder) is { } language)
        {
            return (builder, language);
        }
        lock (_builderGate)
        {
            // 只捕获当前 source 的冻结值；不捕获 input、父 snapshot 或历史 source。
            _globalBuilder ??= ThemeCompiler.CreateBuilder(_globalValues, _globalPalettes, _registry.GlobalTokens);
            _languageBuilder ??= ThemeCompiler.CreateLanguageBuilder(_unit.Language, _languageValues);
            return (_globalBuilder, _languageBuilder);
        }
    }

    internal ControlThemeSnapshot Compile(
        ControlTokenDescriptor descriptor,
        NormalizedControlThemeConfig? config,
        ControlThemeSnapshot? valueDonor = null)
    {
        ControlThemeSnapshot Factory()
        {
            var (global, language) = GetBuilders();
            var pool = Volatile.Read(ref _globalPool) ??
                       Interlocked.CompareExchange(ref _globalPool, new ControlGlobalTokenPool(global, _registry.GlobalTokens), null) ??
                       _globalPool!;
            return ThemeCompiler.CompileControl(
                descriptor,
                config,
                _unit,
                global,
                language,
                _globalValues,
                _globalResources,
                _registry,
                valueDonor,
                pool);
        }

        return _cache?.TryGetTarget(out var cache) == true
            ? cache.GetOrCompile(_globalKey!, descriptor, config,
                descriptor.HasOwnTokens ? _registry.FindControlTokenRule(_unit.Language, descriptor.Slot) : null,
                Factory, EstimatedRetainedBytes)
            : Factory();
    }

    internal void NotifyMaterialized()
    {
        if (_cache?.TryGetTarget(out var cache) == true)
        {
            cache.NotifyMaterialized();
        }
    }
}

/// <summary>
/// 独立惰性计算单元。跨快照直接共享 cell，不沿完整父快照链递归查找。
/// </summary>
internal sealed class ControlSnapshotCell
{
    private readonly object _gate = new();
    private LazyControlSnapshotSource? _source;
    private ControlTokenDescriptor? _descriptor;
    private Func<ControlTokenDescriptor>? _descriptorFactory;
    private Func<NormalizedControlThemeConfig?>? _configFactory;
    private NormalizedControlThemeConfig? _config;
    private ControlThemeSnapshot? _reuseValue;
    private NormalizedControlThemeConfig? _reuseConfig;
    private ControlThemeSnapshot? _valueDonor;
    private ControlThemeSnapshot? _value;
    private bool _isCompiling;

    internal ControlSnapshotCell(ControlThemeSnapshot value)
    {
        _value = value;
    }

    internal ControlSnapshotCell(LazyControlSnapshotSource source, ControlTokenDescriptor descriptor,
        NormalizedControlThemeConfig? config, ControlThemeSnapshot? valueDonor = null)
    {
        _source = source;
        _descriptor = descriptor;
        _config = config;
        _valueDonor = valueDonor;
    }

    internal ControlSnapshotCell(LazyControlSnapshotSource source, Func<ControlTokenDescriptor> descriptorFactory,
        Func<NormalizedControlThemeConfig?> configFactory,
        ControlThemeSnapshot? reuseValue = null, NormalizedControlThemeConfig? reuseConfig = null,
        ControlThemeSnapshot? valueDonor = null)
    {
        _source = source;
        _descriptorFactory = descriptorFactory;
        _configFactory = configFactory;
        _reuseValue = reuseValue;
        _reuseConfig = reuseConfig;
        _valueDonor = valueDonor;
    }

    /// <summary>
    /// Whether the control sees the scope's global values unchanged. A control rule may override a global token
    /// in its view, which is known only once the control compiles; until then this is <see langword="false"/> and a
    /// lookup materializes the cell.
    /// </summary>
    internal bool UsesRootSharedResources => MaterializedValue is { EffectiveGlobalTokenDelta.Count: 0 };

    internal ControlThemeSnapshot? MaterializedValue => Volatile.Read(ref _value);
    internal LazyControlSnapshotSource? Source => Volatile.Read(ref _source);
    internal long EstimatedConfigBytes => (_config ?? _reuseConfig) is { } config
        ? NormalizedControlConfigCacheKey.Create(config).EstimatedInputRetainedBytes
        : 0;
    internal ControlThemeSnapshot? ReuseCandidateValue => Volatile.Read(ref _reuseValue);

    /// <summary>
    /// The same control's previously materialized snapshot, held until this cell materializes so unchanged
    /// resource values keep their instances; never returned as the cell's value.
    /// </summary>
    internal ControlThemeSnapshot? ValueDonor => Volatile.Read(ref _valueDonor);

    internal ControlThemeSnapshot GetValue()
    {
        if (MaterializedValue is { } value)
        {
            return value;
        }
        LazyControlSnapshotSource source;
        ControlThemeSnapshot snapshot;
        lock (_gate)
        {
            if (_value is { } existing)
            {
                return existing;
            }
            if (_isCompiling)
            {
                throw new InvalidOperationException("A recursive theme Control snapshot query is not supported.");
            }
            // 失败不写缓存，也不丢弃输入，保留后续查询重新计算的既有语义。
            source = _source!;
            _isCompiling = true;
            try
            {
                var config = _configFactory?.Invoke() ?? _config;
                snapshot = _reuseValue is { } reusable && Equals(config, _reuseConfig)
                    ? reusable
                    : source.Compile(_descriptor ?? _descriptorFactory!(), config, _valueDonor ?? _reuseValue);
            }
            finally
            {
                _isCompiling = false;
            }
            Volatile.Write(ref _value, snapshot);
            _descriptor = null;
            _descriptorFactory = null;
            _configFactory = null;
            _config = null;
            _reuseValue = null;
            _reuseConfig = null;
            Volatile.Write(ref _valueDonor, null);
            Volatile.Write(ref _source, null);
        }
        // cache 扫描只读已发布值，不持有 cell 锁执行驱逐，避免锁反转。
        source.NotifyMaterialized();
        return snapshot;
    }
}

/// <summary>
/// 槽位数组只共享独立 cell；读取不会物化或保活前代 ThemeSnapshot。
/// </summary>
internal sealed class LazyControlSnapshots
{
    private readonly ControlSnapshotCell[] _cells;

    internal LazyControlSnapshots(ControlSnapshotCell[] cells)
    {
        _cells = cells;
    }

    internal int Count => _cells.Length;
    internal ControlSnapshotCell GetCell(int slot) => _cells[slot];
    internal ControlThemeSnapshot this[int slot] => _cells[slot].GetValue();

    internal long EstimatedRetainedBytes
    {
        get
        {
            var bytes = 0L;
            HashSet<LazyControlSnapshotSource>? sources = null;
            foreach (var cell in _cells)
            {
                // source 先读；并发物化至多重复计费，不会遗漏编译输入与新发布的值。
                if (cell.Source is { } source)
                {
                    sources ??= new HashSet<LazyControlSnapshotSource>();
                    if (sources.Add(source))
                    {
                        bytes = ThemeRetainedBytesEstimator.Add(bytes, source.EstimatedRetainedBytes);
                    }
                    bytes = ThemeRetainedBytesEstimator.Add(bytes, cell.EstimatedConfigBytes);
                }
                bytes = ThemeRetainedBytesEstimator.Add(bytes, cell.MaterializedValue?.EstimatedRetainedBytes ?? 0);
                bytes = ThemeRetainedBytesEstimator.Add(bytes, cell.ReuseCandidateValue?.EstimatedRetainedBytes ?? 0);
                // The value donor is released on materialization; until then it is retained like the reuse candidate.
                if (cell.ValueDonor is { } donor && !ReferenceEquals(donor, cell.ReuseCandidateValue))
                {
                    bytes = ThemeRetainedBytesEstimator.Add(bytes, donor.EstimatedRetainedBytes);
                }
            }
            return bytes;
        }
    }
}
