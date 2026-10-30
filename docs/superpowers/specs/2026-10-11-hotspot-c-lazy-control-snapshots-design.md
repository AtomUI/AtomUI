# 热点 C 设计：控件主题快照按 slot 惰性编译

日期：2026-10-11
状态：设计已获用户批准（含两个实施细节决策点）
上游材料：`docs/superpowers/progress/2026-10-11-macos-startup-remaining-hotspots.md` 热点 C 节；
`docs/superpowers/plans/2026-10-11-startup-remaining-hotspots-optimization.md` Task 6（契约修订批准门）

## 背景与基线

`ThemeCompiler.CompileControls`（ThemeCompiler.cs:237）在主题编译末期对注册表全部 187 个控件急切编译
`ControlThemeSnapshot`，实测 72.3ms：

| 子项 | 实测 |
|---|---:|
| Own Token 计算（87 个控件，1032 个派生公式） | 34.5ms |
| Token 冻结（TokenValueTable.Freeze + 逐值保留字节估算） | 17.5ms |
| 资源投影（BuildResourceMap 装箱/冻结/FrozenDictionary） | 9.9ms |
| 循环、复用判定、快照分配 | ~10.4ms |

运行时对控件快照的唯一消费是按槽位点查（`snapshot.Controls[slot]`），空窗口首帧前查询次数为零——
约 62ms 预编译换取零次查询。本设计把控件级快照改为首查时编译，预期同步启动路径再省 ~62-68ms。

## 契约修订（用户已在 Task 6 门批准）

现行（`docs/architecture/systems/theming/runtime.md` §7 编译器管线 "evaluate each registered Control →
freeze → ThemeSnapshot" 为一次同步完成，§8 ThemeSnapshot 的 `Controls[registry control slot]` 为编译期
全量数组；即计划 Task 6 所称"首帧完整主题提交"）：
`ApplyTheme` 返回时全部控件快照已编译、立即可查。

修订后：`ApplyTheme` 返回时**全局 token/资源完整提交**，控件级快照**首次查询时编译**。
runtime.md §7 的管线描述与 §8 的 `Controls` 语义随实施同步修订。

- 任何资源查询的**结果值不变**（差分测试机械锁定）；
- 公开 API 不变（`ThemeSnapshot`/`ControlThemeSnapshot` 均为 internal）；
- 首帧内首次实例化的控件各自一次性支付 ~0.7ms（含 JIT）内联编译；
- "切换主题后控件 token 立即可查"变为"首次查询时可查"，结果相同、时点不同。

## 消费者清单（源码核实，穷举）

`ThemeSnapshot.Controls` 的全部消费点共 7 处，形态只有 `Count` 检查与索引器点查两种：

1. `ThemeTokenResolver.cs:50-54`（token 取值）、`:65-80`（按名回退扫描）、`:107-112`（资源键回退）
2. `ThemeTokenResourceProvider.cs:53-59`、`:87`、`:100-102`（TryGetResource 三路径）
3. `ThemeRetainedBytesEstimator.cs:42-43`（**唯一的枚举消费者**——估算重写对象）
4. `ThemeCompiler.cs:278`（主题切换复用：`controls[slot] = ReusableParent.Controls[slot]`）

## 设计

### 1. LazyControlSnapshots（新 internal 类型）

替代 `IReadOnlyList<ControlThemeSnapshot>`，**只暴露 `Count` 与索引器 `[int slot]`，刻意不实现
`IEnumerable`**——枚举即全量物化，正是 `EstimateSnapshot` 的陷阱，从类型上禁绝。

```csharp
internal sealed class LazyControlSnapshots
{
    // 状态在快照构造期急切解析（廉价配置比较），编译推迟到首访。
    private enum SlotState : byte { Empty, ReuseParent, Uncompiled }

    private readonly SlotState[] _states;
    private readonly ControlThemeSnapshot?[] _materialized;   // 物化缓存
    private readonly ControlThemeSnapshot[] _eagerEmpty;      // Empty 槽位的急切共享实例
    private readonly LazyControlSnapshotSource _source;       // Uncompiled 槽位的编译输入（冻结）
    private readonly ThemeSnapshot? _reusableParent;          // ReuseParent 槽位的父快照
    private readonly object _gate = new();
    private long _materializedBytes;                          // 估算增量累计（Interlocked）
    private int _materializedCount;                           // 测试观测口

    internal int Count => _states.Length;
    internal int MaterializedCount => _materializedCount;     // 测试观测口
    internal long MaterializedBytes => Interlocked.Read(ref _materializedBytes);

    internal ControlThemeSnapshot this[int slot] =>
        _materialized[slot] ?? Materialize(slot);
}
```

`Materialize(slot)`：按状态分派——Empty 返回急切实例；ReuseParent 读父快照索引器
（父槽未物化则此时物化父槽并共享同一对象）；Uncompiled 进 `_gate` 双检后经
`ControlCompilationCache.GetOrCompile`（缓存键不变：GlobalCompilationCacheKey + descriptor +
config + appearance）编译，回填 `_materialized[slot]` 并 `Interlocked.Add` 估算字节与计数。

### 2. 急切解析 + 惰性编译（决策点 1，已批准）

`CompileControls` 保留每槽位廉价判定循环（FindControlConfig / IsControlChanged / CanReuseControl，
~9.5ms），把每槽位解析为三种状态；只推迟昂贵的 `CompileControl`（~62ms）。

被否决的备选"全惰性"（槽位判定也推迟）：可多省 ~8ms，但 FindControlConfig 的光标是跨槽位顺序
扫描优化，惰性化需改为每槽位二分查找并重排 changeSet 游标——复杂度高、收益小。

### 3. 主题切换：首访时父槽位物化复用

`ThemeCompiler.cs:278` 的复用改写为子槽位记录 `ReuseParent(parent, slot)`，切换期零编译。
首访时物化父槽并共享——复用语义本就是同一对象引用，物化父槽（~0.7ms）与子槽自编译成本相同。
未物化槽位在切换期不被被动触发（changeSet 惰性继承成立）。

### 4. 估算记账：急切基线 + 增量回填（决策点 2，已批准）

`ThemeSnapshot.EstimatedRetainedBytes` = 全局部分急切（现状）+ Empty 槽位急切 +
`controls.MaterializedBytes`（物化时增量）。ReuseParent 槽共享父对象只计引用字节。
`ThemeRetainedBytesEstimator.EstimateSnapshot` 不再枚举槽位（删除 :42-43 的 foreach，
改为读 `MaterializedBytes` 快照值）。

已知取舍：两个缓存（ThemeSnapshotCache / ControlCompilationCache）在**插入时**采样
`EstimatedRetainedBytes`（BoundedLruCache 尺寸委托），物化增长不回灌。估算是启发式容量阀，
漂移上界 = 实消费槽位字节（≤187 槽位 × 单槽 KB 级），可接受，写入代码注释。

### 5. 并发

查询可从任意线程进入：索引器先无锁读 `_materialized[slot]`，未命中进 `_gate` 双检。
`BoundedLruCache` 自带 `_gate` 锁与单飞创建（InFlightCount 语义），缓存路径安全。
编译输入（TokenValueTable 已 Freeze、NormalizedControlThemeConfig、算法列表、外观）在快照
构造期冻结，`CompileControl` 纯构造新对象，无共享突变。

## 等价性论证

同一 `ThemeSnapshot` 内编译输入（globalValues/globalResources/palettes/config/appearance/registry）
在构造期冻结，槽位延迟编译与批量编译走**同一个** `CompileControl`（ThemeCompiler.cs:354）函数与
同一个缓存键——逐位相等是结构性质，由差分测试机械证明。

## 测试矩阵（临时测试，验收后按生命周期删除）

1. **187 槽位差分**：真实桌面注册构建两个同输入快照——急切参考（测试钩子直调现有
   CompileControls 急切路径）与惰性——逐槽位比对 token 值、资源键值、保留字节。
2. **空窗口物化计数**：启动后 `MaterializedCount` ≈ 0–2（仅 Window 链），精确门限回归。
3. **主题切换**：Light/Dark/Compact/局部 scope 切换前后同一控件查询结果不变；切换耗时不回归
   （对照急切基线，容差内）。
4. **并发首查**：N 线程同时首访同一槽位，编译工厂调用计数 == 1，全部返回同一实例。
5. **估算增量**：物化 N 槽后 `EstimatedRetainedBytes` == 急切基线 + Σ 已物化槽位估算 +
   复用槽引用字节。
6. **复用共享**：切换后 ReuseParent 槽位首访返回的对象与父槽位 `ReferenceEquals`。

## 改动文件

- 新增：`src/AtomUI.Core/Theme/Compilation/LazyControlSnapshots.cs`
- 修改：`ThemeSnapshot.cs`（Controls 类型 + 估算接线）、`ThemeCompiler.cs`（CompileControls
  解析循环输出状态机、复用路径改 ReuseParent 标记）、`ThemeRetainedBytesEstimator.cs`
  （删枚举改读增量）、`ThemeTokenResolver.cs` / `ThemeTokenResourceProvider.cs`
  （类型跟随，索引器签名不变）
- 测试钩子：ThemeCompiler 暴露 internal 急切编译入口（IVT，仅供差分测试对照）
- 文档：`docs/architecture/systems/theming/runtime.md` §7 编译器管线与 §8 ThemeSnapshot
  `Controls` 语义修订（全局急切 + 控件级首查编译）

## 非目标

- 不改变任何查询结果、缓存键、公开 API；
- 不改 `BoundedLruCache`（漂移接受，见 §4）；
- 不动全局 token/资源的急切提交；
- 不做"全惰性"（槽位判定推迟）。

## 风险与回退

- 首查线程上 ~0.7ms 内联编译：可感知性极低（与控件实例化同帧既有成本同量级）；
- 估算漂移导致缓存提前/滞后驱逐：量级有界，驱逐只影响性能不影响正确性；
- 回退：单提交 revert 恢复急切 CompileControls（无跨模块状态迁移）。
