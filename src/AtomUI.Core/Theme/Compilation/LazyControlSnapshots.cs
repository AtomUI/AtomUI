using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Schema;

namespace AtomUI.Theme.Compilation;

/// <summary>
/// The read-only global compilation input shared by lazy control cells. It keeps only what control compilation
/// needs from the global compilation, never the parent snapshot or the whole control configuration table.
/// The registry is held (instead of only its global descriptor list) because control compilation also looks up the
/// control token stages there; the snapshot that owns the cells holds the same registry.
/// </summary>
internal sealed class LazyControlSnapshotSource
{
    private readonly ThemeSchemaRegistry _registry;
    private readonly IReadOnlyList<ThemeAlgorithmDescriptor> _globalAlgorithms;
    // The scope's explicit global values (the effective config's own list, not the control table). Only a control
    // stage reads them, to write the explicit Map/Alias values back over its EvaluateGlobal writes.
    private readonly IReadOnlyList<NormalizedTokenValue> _globalTokens;
    // Stored as an int, not a long: one source is allocated per compilation, so its object size is part of the
    // measured per-compile allocation, and the int keeps the object at its size before _globalTokens was added. No
    // reference this source already holds reaches the scope's explicit token list: the cache key exists only with a
    // cache and keeps per token only its name, slot, stage and canonical value text, not the typed values the stage
    // write-back needs; so the field above is needed. The estimate (global values, resources and palettes of one
    // compilation, a few hundred KB) cannot approach int.MaxValue; the constructor saturates it there anyway. It does
    // not count _globalTokens: that list belongs to the effective config, which the owning snapshot's estimate
    // already counts (EstimateSnapshotBase).
    private readonly int _estimatedRetainedBytes;
    private readonly ThemeAppearance _globalAppearance;
    private readonly object _builderGate = new();
    private DesignToken? _globalBuilder;
    private readonly IReadOnlyDictionary<PresetPrimaryColor, PaletteInfo> _globalPalettes;
    private readonly TokenValueTable _globalValues;
    private readonly IReadOnlyDictionary<object, object?> _globalResources;
    private readonly WeakReference<ControlCompilationCache>? _cache;
    private readonly GlobalCompilationCacheKey? _globalKey;

    internal LazyControlSnapshotSource(
        ThemeCompileInput input,
        ThemeAppearance globalAppearance,
        DesignToken? globalBuilder,
        TokenValueTable globalValues,
        IReadOnlyDictionary<object, object?> globalResources,
        IReadOnlyDictionary<PresetPrimaryColor, PaletteInfo> globalPalettes,
        ControlCompilationCache? cache)
    {
        ArgumentNullException.ThrowIfNull(input);
        _registry         = input.Registry;
        _globalAlgorithms = input.EffectiveConfig.Algorithms;
        _globalTokens     = input.EffectiveConfig.GlobalTokens;
        _globalAppearance = globalAppearance;
        _globalBuilder    = globalBuilder;
        _globalValues     = globalValues;
        _globalResources  = globalResources;
        _globalPalettes   = globalPalettes;
        _cache            = cache is null ? null : new WeakReference<ControlCompilationCache>(cache);
        _globalKey        = cache is null ? null : GlobalCompilationCacheKey.Create(input);
        _estimatedRetainedBytes = (int)Math.Min(int.MaxValue, ThemeRetainedBytesEstimator.EstimateLazySource(
            globalValues, globalResources, globalPalettes, _globalAlgorithms.Count, _globalKey?.EstimatedRetainedBytes ?? 0));
    }

    internal long EstimatedRetainedBytes => _estimatedRetainedBytes;

    private DesignToken GetGlobalBuilder()
    {
        if (Volatile.Read(ref _globalBuilder) is { } builder)
        {
            return builder;
        }
        lock (_builderGate)
        {
            if (_globalBuilder is null)
            {
                // 只捕获当前 source 的冻结值；不捕获 input、父 snapshot 或历史 source。
                Volatile.Write(ref _globalBuilder,
                    ThemeCompiler.CreateBuilder(_globalValues, _globalPalettes, _registry.GlobalTokens));
            }
            return _globalBuilder;
        }
    }

    internal ControlThemeSnapshot Compile(
        ControlTokenDescriptor descriptor,
        NormalizedControlThemeConfig? config,
        ControlThemeSnapshot? valueDonor = null)
    {
        ControlThemeSnapshot Factory()
        {
            return ThemeCompiler.CompileControl(
                descriptor,
                config,
                _globalAlgorithms,
                _globalAppearance,
                GetGlobalBuilder(),
                _globalValues,
                _globalResources,
                _registry,
                valueDonor,
                _globalTokens);
        }

        // The stages are resolved again inside CompileControl rather than captured here: capturing them would grow
        // the closure of every control, including those without stages.
        return _cache?.TryGetTarget(out var cache) == true
            ? cache.GetOrCompile(_globalKey!, descriptor, config, _globalAppearance, Factory, EstimatedRetainedBytes,
                _registry.HasControlAlgorithms
                    ? ThemeCompiler.ResolveControlStages(_registry, descriptor, config, _globalAlgorithms)
                    : null)
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
        UsesRootSharedResources = value.EffectiveGlobalTokenDelta.Count == 0;
    }

    internal ControlSnapshotCell(LazyControlSnapshotSource source, ControlTokenDescriptor descriptor,
        NormalizedControlThemeConfig? config, ControlThemeSnapshot? valueDonor = null, bool hasControlStage = false)
    {
        _source = source;
        _descriptor = descriptor;
        _config = config;
        _valueDonor = valueDonor;
        // 与 CompileControl 在 Own evaluator 之前冻结差量的分支相同。
        // Own evaluator 不参与 Shared 值计算，Global/Custom 算法仍走完整路径。
        // A control token stage may write the control's global copy (EvaluateGlobal), which is known only once the
        // control compiles, so a control with a stage never reads the root's shared resources directly.
        UsesRootSharedResources = !hasControlStage && (config is null ||
            (config.GlobalTokens.Count == 0 && config.AlgorithmMode is ControlAlgorithmMode.Unspecified or ControlAlgorithmMode.Disabled));
    }

    internal ControlSnapshotCell(LazyControlSnapshotSource source, Func<ControlTokenDescriptor> descriptorFactory,
        Func<NormalizedControlThemeConfig?> configFactory, bool usesRootSharedResources,
        ControlThemeSnapshot? reuseValue = null, NormalizedControlThemeConfig? reuseConfig = null,
        ControlThemeSnapshot? valueDonor = null)
    {
        _source = source;
        _descriptorFactory = descriptorFactory;
        _configFactory = configFactory;
        _reuseValue = reuseValue;
        _reuseConfig = reuseConfig;
        _valueDonor = valueDonor;
        UsesRootSharedResources = usesRootSharedResources;
    }

    internal bool UsesRootSharedResources { get; }
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
