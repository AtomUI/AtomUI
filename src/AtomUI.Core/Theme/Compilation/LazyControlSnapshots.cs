using AtomUI.Theme.Configuration;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Schema;

namespace AtomUI.Theme.Compilation;

/// <summary>
/// 控件快照槽位状态，快照构造期急切解析（见 ThemeCompiler.CompileControls）。
/// </summary>
internal enum ControlSlotState : byte
{
    /// <summary>构造期已物化（无配置且无 Own Token 的槽位在构造期拿到廉价空快照）。</summary>
    Materialized,

    /// <summary>复用父快照同槽位对象：切换期零编译，首访时物化父槽并共享同一引用。</summary>
    ReuseParent,

    /// <summary>编译输入已冻结，首访时经 CompileControl / ControlCompilationCache 编译。</summary>
    Uncompiled,
}

/// <summary>
/// Uncompiled 槽位的冻结编译输入。快照构造期由 ThemeCompiler 填充；CompileControl 纯读这些
/// 输入构造新对象、无共享突变，因此惰性编译与急切编译逐位等价（同一函数、同一缓存键）。
/// </summary>
internal sealed class LazyControlSnapshotSource
{
    private readonly ThemeSchemaRegistry _registry;
    private readonly NormalizedControlThemeConfig?[] _configs;
    private readonly IReadOnlyList<ThemeAlgorithmDescriptor> _globalAlgorithms;
    private readonly ThemeAppearance _globalAppearance;
    private readonly DesignToken _globalBuilder;
    private readonly TokenValueTable _globalValues;
    private readonly IReadOnlyDictionary<object, object?> _globalResources;
    private readonly ControlCompilationCache? _cache;
    private readonly GlobalCompilationCacheKey _globalKey;

    internal LazyControlSnapshotSource(
        ThemeCompileInput input,
        NormalizedControlThemeConfig?[] configs,
        ThemeAppearance globalAppearance,
        DesignToken globalBuilder,
        TokenValueTable globalValues,
        IReadOnlyDictionary<object, object?> globalResources,
        ControlCompilationCache? cache)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(configs);
        _registry         = input.Registry;
        _configs          = configs;
        _globalAlgorithms = input.EffectiveConfig.Algorithms;
        _globalAppearance = globalAppearance;
        _globalBuilder    = globalBuilder;
        _globalValues     = globalValues;
        _globalResources  = globalResources;
        _cache            = cache;
        // 与急切路径的 globalCompilationCacheKey ??= 语义等价：source 只在存在 Uncompiled 槽位时构造。
        _globalKey        = GlobalCompilationCacheKey.Create(input);
    }

    internal ControlThemeSnapshot Compile(int slot)
    {
        var descriptor = _registry.Controls[slot];
        var config = _configs[slot];

        ControlThemeSnapshot Factory()
        {
            return ThemeCompiler.CompileControl(
                descriptor,
                config,
                _globalAlgorithms,
                _globalAppearance,
                _globalBuilder,
                _globalValues,
                _globalResources,
                _registry.GlobalTokens);
        }

        return _cache is null
            ? Factory()
            : _cache.GetOrCompile(_globalKey, descriptor, config, _globalAppearance, Factory);
    }
}

/// <summary>
/// 控件主题快照的按槽位惰性容器。只暴露 Count 与索引器，刻意不实现 IEnumerable——
/// 枚举即全量物化，正是快照保留字节估算要避免的形态（见 ThemeRetainedBytesEstimator）。
/// 状态与编译输入在快照构造期急切解析并冻结；昂贵的 CompileControl 推迟到首访
/// （空窗口首帧前对控件快照的查询次数为零，急切预编译是纯启动损耗）。
/// 线程契约：索引器可从任意线程进入；未物化槽位经 _gate 双检单飞物化，无锁快读命中
/// 已物化槽位（引用写入原子、无撕裂；读到过期 null 只是多进一次锁，双检兜底）。
/// </summary>
internal sealed class LazyControlSnapshots
{
    // 与 ThemeRetainedBytesEstimator.ReferenceBytes 对齐：复用槽共享父对象，只计引用字节。
    private const long SharedSlotReferenceBytes = 8;

    private readonly ControlSlotState[] _states;
    private readonly ControlThemeSnapshot?[] _materialized;
    private readonly ThemeSnapshot? _reusableParent;
    private readonly LazyControlSnapshotSource? _source;
    private readonly object _gate = new();
    private long _materializedBytes;
    private int _materializedCount;

    internal LazyControlSnapshots(
        ControlSlotState[] states,
        ControlThemeSnapshot?[] preMaterialized,
        ThemeSnapshot? reusableParent,
        LazyControlSnapshotSource? source)
    {
        ArgumentNullException.ThrowIfNull(states);
        ArgumentNullException.ThrowIfNull(preMaterialized);
        if (states.Length != preMaterialized.Length)
        {
            throw new ArgumentException("The state and pre-materialized arrays must have the same length.");
        }
        if (source is null && Array.IndexOf(states, ControlSlotState.Uncompiled) >= 0)
        {
            throw new ArgumentException("Uncompiled slots require a compilation source.");
        }
        if (reusableParent is null && Array.IndexOf(states, ControlSlotState.ReuseParent) >= 0)
        {
            throw new ArgumentException("ReuseParent slots require a reusable parent snapshot.");
        }

        _states         = states;
        _materialized   = preMaterialized;
        _reusableParent = reusableParent;
        _source         = source;
        // 构造期已物化的 Empty 槽位计入估算基线；Materialize 只累计首访物化的增量
        // （设计文档 §4：急切基线 + 增量回填）。
        foreach (var snapshot in preMaterialized)
        {
            if (snapshot is not null)
            {
                _materializedBytes += snapshot.EstimatedRetainedBytes;
            }
        }
    }

    internal int Count => _states.Length;

    // 测试观测口：首访物化（ReuseParent + Uncompiled）的槽位计数，不含构造期 Empty 实例。
    internal int MaterializedCount => Volatile.Read(ref _materializedCount);

    internal long MaterializedBytes => Interlocked.Read(ref _materializedBytes);

    internal ControlThemeSnapshot this[int slot]
    {
        get
        {
            if ((uint)slot >= (uint)Count)
            {
                throw new ArgumentOutOfRangeException(nameof(slot));
            }
            return _materialized[slot] ?? Materialize(slot);
        }
    }

    private ControlThemeSnapshot Materialize(int slot)
    {
        lock (_gate)
        {
            if (_materialized[slot] is { } existing)
            {
                return existing;
            }

            ControlThemeSnapshot snapshot;
            if (_states[slot] == ControlSlotState.ReuseParent)
            {
                // 物化父槽（父槽未物化则级联物化）并共享同一对象——复用语义本就是同一引用。
                snapshot = _reusableParent!.Controls[slot];
            }
            else
            {
                snapshot = _source!.Compile(slot);
            }

            _materialized[slot] = snapshot;
            var delta = _states[slot] == ControlSlotState.ReuseParent
                ? SharedSlotReferenceBytes
                : snapshot.EstimatedRetainedBytes;
            // 饱和语义与估算器 Add 对齐：溢出回绕（需 ~9.2e18 保留字节，物理不可达）不允许
            // 产生负值——负尺寸会让 BoundedLruCache 的容量记账抛异常。
            if (Interlocked.Add(ref _materializedBytes, delta) < 0)
            {
                Interlocked.Exchange(ref _materializedBytes, long.MaxValue);
            }
            Interlocked.Increment(ref _materializedCount);
            return snapshot;
        }
    }
}
