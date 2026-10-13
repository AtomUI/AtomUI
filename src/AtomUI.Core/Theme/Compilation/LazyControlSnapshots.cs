using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Schema;

namespace AtomUI.Theme.Compilation;

/// <summary>
/// 惰性槽共享的只读全局编译输入。只保留全局计算所需数据，不持有父快照或整张控件配置表。
/// </summary>
internal sealed class LazyControlSnapshotSource
{
    private readonly IReadOnlyList<TokenDescriptor> _globalDescriptors;
    private readonly IReadOnlyList<ThemeAlgorithmDescriptor> _globalAlgorithms;
    private readonly ThemeAppearance _globalAppearance;
    private readonly object _builderGate = new();
    private DesignToken? _globalBuilder;
    private readonly IReadOnlyDictionary<PresetPrimaryColor, PaletteInfo> _globalPalettes;
    private readonly TokenValueTable _globalValues;
    private readonly IReadOnlyDictionary<object, object?> _globalResources;
    private readonly ControlCompilationCache? _cache;
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
        _globalDescriptors = input.Registry.GlobalTokens;
        _globalAlgorithms = input.EffectiveConfig.Algorithms;
        _globalAppearance = globalAppearance;
        _globalBuilder    = globalBuilder;
        _globalValues     = globalValues;
        _globalResources  = globalResources;
        _globalPalettes   = globalPalettes;
        _cache            = cache;
        _globalKey        = cache is null ? null : GlobalCompilationCacheKey.Create(input);
        EstimatedRetainedBytes = ThemeRetainedBytesEstimator.EstimateLazySource(
            globalValues, globalResources, globalPalettes, _globalAlgorithms.Count, _globalKey?.EstimatedRetainedBytes ?? 0);
    }

    internal long EstimatedRetainedBytes { get; }

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
                    ThemeCompiler.CreateBuilder(_globalValues, _globalPalettes, _globalDescriptors));
            }
            return _globalBuilder;
        }
    }

    internal ControlThemeSnapshot Compile(ControlTokenDescriptor descriptor, NormalizedControlThemeConfig? config)
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
                _globalDescriptors);
        }

        return _cache is null
            ? Factory()
            : _cache.GetOrCompile(_globalKey!, descriptor, config, _globalAppearance, Factory);
    }

    internal void NotifyMaterialized() => _cache?.NotifyMaterialized();
}

/// <summary>
/// 独立惰性计算单元。跨快照直接共享 cell，不沿完整父快照链递归查找。
/// </summary>
internal sealed class ControlSnapshotCell
{
    private readonly object _gate = new();
    private LazyControlSnapshotSource? _source;
    private ControlTokenDescriptor? _descriptor;
    private NormalizedControlThemeConfig? _config;
    private ControlThemeSnapshot? _value;

    internal ControlSnapshotCell(ControlThemeSnapshot value)
    {
        _value = value;
        UsesRootSharedResources = value.EffectiveGlobalTokenDelta.Count == 0;
    }

    internal ControlSnapshotCell(LazyControlSnapshotSource source, ControlTokenDescriptor descriptor,
        NormalizedControlThemeConfig? config)
    {
        _source = source;
        _descriptor = descriptor;
        _config = config;
        // 与 CompileControl 在 Own evaluator 之前冻结差量的分支相同。
        // Own evaluator 不参与 Shared 值计算，Global/Custom 算法仍走完整路径。
        UsesRootSharedResources = config is null ||
            (config.GlobalTokens.Count == 0 && config.AlgorithmMode is ControlAlgorithmMode.Unspecified or ControlAlgorithmMode.Disabled);
    }

    internal bool UsesRootSharedResources { get; }
    internal ControlThemeSnapshot? MaterializedValue => Volatile.Read(ref _value);
    internal LazyControlSnapshotSource? Source => Volatile.Read(ref _source);
    internal long EstimatedConfigBytes => _config is { } config
        ? NormalizedControlConfigCacheKey.Create(config).EstimatedInputRetainedBytes
        : 0;

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
            // 失败不写缓存，也不丢弃输入，保留后续查询重新计算的既有语义。
            source = _source!;
            snapshot = source.Compile(_descriptor!, _config);
            Volatile.Write(ref _value, snapshot);
            _descriptor = null;
            _config = null;
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
            }
            return bytes;
        }
    }
}
