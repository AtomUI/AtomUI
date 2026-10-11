# AtomUI 启动剩余热点源码归因与下一轮方案（macOS 优化后数据）

分析日期：2026-10-11。基线：`106a9659d`（含 `914ce40ef` 已实施的优化）。
本文基于优化**之后**的 macOS Release 空窗口细分测量（用户提供，Main → 根控件首次 Render 共 1021.5ms），
对每个耗时点做源码级归因；上一轮方案的实施状态与证伪结论见
[启动热点与优化方案处置](2026-10-10-startup-hotspots-and-fix-designs.md)；
Windows 轮验收数据见[Windows 结果报告](2026-10-11-windows-startup-algorithm-results.md)。

所有"归因"基于源码静态分析与上轮实测对照；本文**未实施任何新优化**，未新增测量。
标注为"首次执行/JIT"的条目指进程冷启动第一次执行该路径的解释器/tier-0 编译/运行时数据加载成本，
这类条目没有算法优化空间，只列 ReadyToRun/NativeAOT 类部署手段。

## 优化后实测树与归因总表

| 阶段 | 实测 | 归因类别 |
|---|---:|---|
| Main → App.Initialize | 292.6ms | 装载/JIT/平台初始化（非 AtomUI 算法） |
| configure 回调（UseDesktopControls） | 211.4ms | **算法**：descriptor 物化 + 指纹/排序（可优化） |
| LocalizationBuilder.Build | 29.8ms | 首次执行/JIT + ICU 初始化（少量可查证） |
| ThemeManagerBuilder.Build | 46.1ms | **算法**：registry 冻结 + Revision 指纹（可优化） |
| BuildOwnedServices | 7.1ms | 首次执行 |
| InitializeApplication | 221.0ms | 其中 MountStatic 89.2（**算法**）+ ApplyThemeAsync 109.6（**算法**） |
| ModuleInitializers | 23.5ms | ToolTipService + 断点注入（基本必要） |
| Framework/Window 构造 → Opened → Render | 79.4 + 42.4 + 41.6ms | 框架边界（非 AtomUI 算法） |
| 未覆盖衔接 | 22.0ms | 测量边界 |

UseAtomUI 同步段合计 543.6ms。对照上轮 macOS 基线（~460-490ms 配置回调前的各段之和 + 230ms configure），
`914ce40ef` 已兑现：UseDesktopControls 230→192.4、ThemeManagerBuilder 87→46.1、LoadThemeCatalog 40→12.4、
MountStatic 140-195→89.2、CompileControls ~100→72.3（其中 Format/Parse 复制已归零，见下）。

---

## 最终实测收益（2026-10-11，Tasks 1-5 落地后）

测量方法：git worktree 独立基线（106a9659d）+ 当前树两个 harness 二进制**逐次交替**采样各 10 次取中位数
（负载漂移在交替中抵消；采样期机器负载 28-39，绝对值偏高，相对比较有效）。
harness 为空窗口真实桌面注册（UseDesktopControls）。

| metric | baseline (106a9659d) | optimized | formula | improvement | conclusion |
|---|---:|---:|---|---:|---|
| UseAtomUI.SyncTotal | 595.3ms | 472.5ms | 10 次交替中位数差 | **−122.8ms（−20.6%）** | 同步启动路径总收益，已验证 |
| UseAtomUI.PostConfigure | 367.8ms | 265.6ms | 同上 | −102.2ms | 热点 B（惰性挂载）主体 + 热点 D |
| Mark.FrameworkCompleted | 851.5ms | 757.8ms | 同上 | −93.7ms | 框架就绪时点提前 |
| Configure.UseDesktopControls | 211.5ms | 187.3ms | 同上 | −24.2ms | 热点 A（指纹前缀+预排序） |
| TotalToFirstRender | 1055.6ms | 1048.9ms | 同上 | −6.7ms | 惰性挂载为成本转移：harness 窗口消费的资产在窗口创建期内联支付；未消费资产零付费。空窗口/少消费页面首帧收益趋近同步路径全额 |
| MainToInitialize | 234.3ms | 248.0ms | 同上 | +13.7ms | 反向离群（子段 Configure.Total 同期 −26.1ms），判为高负载进程孵化噪音 |

单点探针（负载无关精确值）：registry Revision 全量遍历 4.06–5.93ms → 包常量组合 0.94–1.25ms。

热点兑现核对：A 估 60-90ms 实测 −24.2ms（部分份额已被前轮 914ce40ef 取走）；B 估 50-70ms 实测
−71.7ms（达成，经方案 B 惰性挂载实现——原计划"生成等价 C# wrapper"机制因 Avalonia 编译期 include
展开为急切 Build 调用且运行时 ResourceInclude 在 AOT 下不可用而废弃）；D 估 20-30ms 实测 3.1-4.7ms
（估计基于 Task 2 前缀续算前的测量，主份额已被 Task 2 取走）；C 估 62-68ms 实测 −60.9ms（SyncTotal，已实施，见热点 C 节"实施与实测"）。

正确性验收：Core 528/528、Generator 458/458、Desktop 全套 4184 零崩溃（4 个失败为急切基线同样复现的
存量负载敏感 flaky）；Gallery osx-arm64 全裁剪 NativeAOT 发布通过 + 产物运行冒烟通过；整分支新鲜
上下文评审 1C/2I 已修复或裁定（详见 .superpowers 台账 progress.md）。临时测试按生命周期全部删除，
以上为删除前最终运行结果。

---

## 热点 A：UseDesktopControls 192.4ms —— 指纹遍历结构与构造器排序仍在

### 源码定位与归因

构成（src/AtomUI.Desktop.Controls GeneratedFiles `GeneratedTypeMapRegistration.g.cs`）：

1. **183 个 `CreateControlDescriptor_XXX`**：每个构造 `ControlTokenDescriptor`，内部
   `ownTokens.OrderBy(slot).ToArray()`（ControlTokenDescriptor.cs:52）——生成器输出的本就是 slot 有序数组。
   共 1032 个 own `TokenDescriptor`（含 codec 单例/accessor 单例引用）。
2. **318 个 `ControlThemeAssetDescriptor.CreateGenerated`**（ControlThemeAssetDescriptor.cs:56-86）：
   - 构造器内 `exports.OrderBy(AQN).ThenBy(...)`、`Canonicalize` 字典化、`bindings.Distinct().OrderBy×4`
     （:41-51）——`914ce40ef` 只优化了 Canonicalize 的空次遍历与 owner 合并（2016→1392B），
     **三条 OrderBy 链原样保留**；
   - `ComputeGeneratedContractFingerprint`（ThemeSchemaRegistry.cs:155-199）与
     `ComputeGeneratedResourceKeySchemaFingerprint`（:141-153）：每个资产都对 255 个
     `CompiledGlobalTokenNames` 全量遍历 FNV。`914ce40ef` 优化的是 FNV **编码本身**
   （ushort 零后缀折叠，每字符 8 次乘加→2 次），**没有改变"每资产×255 名"的遍历结构**：
   318 资产 × 2 指纹 × 255 名的单字 Add 仍逐个执行。
3. **319 个 fragment 类的 `Add` 方法 + 上述工厂 + accessor 单例的首次 JIT**：
   Windows 诊断轮显示 configure 段首进程 1058ms / 后续 326ms，冷 JIT 占比显著；
   macOS 稳态 192.4ms 中 JIT 之外的稳态工作即 1+2 两项。

### 修复方案（承接上轮方案 2 的未实施部分）

```
(1) 共享前缀指纹：两个指纹函数的字段顺序改为 [全局 token 名列表] 在前、资产专有字段在后；
    FNV-1a 顺序敏感 → 名字段成为可共享前缀。包级缓存 prefixState（对 CompiledGlobalTokenNames
    引用键控），每资产从 prefixState 续算专有字段。
    复杂度 O(318×255×2) → O(255×2 + 318)：遍历次数 16 万 → ~1.9 千。
    生成器同步改字段顺序并重发十六进制常量（同源原子提交，无新旧混编）。
(2) 预排序直通构造：生成器按排序规则 emit exports/owners/bindings；新增 internal PreSorted
    构造入口跳过三条 OrderBy 与 Distinct，保留 O(n) 冲突查重与重复 export key 校验。
(3) 首次 JIT 不在算法射程内（见热点 F）。
```

### 验收要点

1. 新旧指纹逐位相等差分测试（全 318 资产枚举比对）。
2. 过期资产契约保留：错误常量 descriptor 抛 `ThemeSchemaException`。
3. 重复 AssetId/冲突 owner/重复 export key 失败行为不变。
4. 涉及生成器：跑 AOT/裁剪专项（TypeMap 契约不变）。

预期收益：指纹遍历与排序约占本段稳态工作的 1/2-2/3，估 **~60-90ms**。
【已验证 2026-10-11：实测 −24.2ms（UseDesktopControls 段），低于估计——部分份额已被前轮 `914ce40ef` 取走；见上方"最终实测收益"。】

---

## 热点 B：MountStaticResources 89.2ms —— 262 个无类包装仍急切构造

### 源码定位与归因

`ThemeManager.MountStaticResources`（ThemeManager.cs:1722）对 315 个 Desktop 资产逐个执行工厂并挂入
`MergedDictionaries`。`914ce40ef` 后的构成（见 Windows 报告"Every finding's disposition"）：

- 49 个 deferred：`resources.AddDeferred(typeof(X), _ => new XTheme { TargetType = ... })`——
  构造的只是外壳字典，真实 ControlTheme 首次查找才实例化；
- 4 个 direct： authored `x:Class` 字典直接构造（原方案本来就直接）；
- **262 个 classless wrapper：`new GeneratedThemeAssetResource_XXX()` 即 `AvaloniaXamlLoader.Load(this)`**
  （GeneratedThemeAssetResources.g.cs，每类构造函数一行）。

89.2ms ÷ 315 ≈ 0.28ms/个。deferred 外壳（一个 ResourceDictionary + AddDeferred 委托）是纯托管分配，
成本可忽略；**成本主体是 262 个无类包装的 XamlIl populate 调用**：每个生成类有独立的编译期 populate
入口，构造时执行"ResourceDictionary + 单条 ResourceInclude(→_Deferred.axaml)"的填充。
Windows 轮已拒绝两种改造（generic x:Class 改写改变 `IRootObjectProvider` 派生资源值；typed x:Class
原型急切构造主题），262 个 wrapper 因此原样保留。

### 修复方案：为 classless wrapper 生成等价托管类（非 XAML populate）

```
生成器为 262 个无类包装生成 C# 类（与 49 个 deferred 同列直接产物）：
  internal sealed class GeneratedThemeAssetResource_XXX : ResourceDictionary
  {
      internal GeneratedThemeAssetResource_XXX()
      {
          MergedDictionaries.Add(new ResourceInclude(deferredUri));
      }
  }
与 XAML populate 的可观测语义逐项等价：内容同为 [ResourceDictionary(空) + MergedDictionaries[0] = ResourceInclude]，
无 x:Class 根对象服务、无主题资源键、无 IThemeVariantProvider（ResourceInclude 的 Key 为 null 与
populate 结果一致）。与被拒绝的两种做法的区别：
  - 不改已有 authored x:Class（4 个 direct 不动）；
  - 不构造真实主题（ResourceInclude 保持首查惰性，Avalonia 12.1.3 语义已核对）。
风险点：wrapper axaml 若含除 MergedDictionaries 以外的根属性/资源条目则不等价——生成器须逐文件
断言"根只有 MergedDictionaries 单条 ResourceInclude"，断言失败回退现有 XAML 路径。
```

### 验收要点

1. 逐资产可观测等价测试：新类与 XAML populate 的 `MergedDictionaries` 结构、`IRootObjectProvider`/主题变体
   行为、资源查找命中一致（复用资源查询契约测试 + 新增等价断言）。
2. 空 Application 下构造 262 个新类，比较 `ThemeManager.Resources` 查找顺序与结果。
3. reload/主题切换后旧包装不复活。
4. 生成器断言失败的回退路径有测试覆盖（构造一个含额外根条目的 wrapper）。

预期收益：262 × populate 调用降为 262 × 纯托管构造，估 **~50-70ms**。
【已验证 2026-10-11：实测 −71.7ms（PostConfigure 段），达成。注意：实施机制改为方案 B 惰性挂载
（代理推迟工厂到首次查找）——本节"生成等价托管类"机制因编译期 include 急切展开与 AOT 裁剪
不兼容而废弃，详见 .superpowers 台账。】

---

## 热点 C：CompileControls 72.3ms —— 全量预编译 vs 空窗口零消费

### 源码定位与归因（新数据修正上轮归因）

`ThemeCompiler.CompileControls`（ThemeCompiler.cs）对 187 个控件循环，实测细分：

| 子项 | 实测 | 源码 |
|---|---:|---|
| Own Token 计算（87 次） | 34.5ms | `descriptor.CreateBuilder()` + `descriptor.Evaluate` → 87 个 token 类的 `CalculateTokenValues`（ButtonToken 50 属性、StepsToken 82、NavMenuToken 68…共 1032 个 own token 的派生公式，如 ButtonToken.cs:279 起） |
| Token 冻结（87 次） | 17.5ms | `TokenValueTable.Freeze(controlBuilder, OwnTokens)`（TokenValueTable.cs:57-70，逐 token GetValue 装箱 + FreezeValue brush/brush clone switch）+ `ThemeRetainedBytesEstimator.EstimateControl`（ControlThemeSnapshot.cs:27，构造器内逐值估算保留字节） |
| 资源投影（87 次） | 9.9ms | `BuildResourceMap`（ThemeCompiler.cs，逐 token ProjectResourceValue 装箱 + FreezeResourceValue + `ToFrozenDictionary`） |
| CopyBuilder（87 次） | **0.9ms** | 上轮问题 3 的 Format/Parse 归因已被 `914ce40ef` 的生成式 `GeneratedThemeSchema.CopyGlobalValues`（强类型 255 属性复制，warm 16,880→3,936B）终结——**该归因证伪，问题关闭** |
| 循环及其余 | 9.5ms | FindControlConfig / CanReuseControl / 快照分配 |

**核心问题**：`ThemeSnapshot.Controls` 是编译期全量数组，但空窗口在首帧前**没有任何资源查询消费者**——
`ThemeTokenResourceProvider.TryGetResource` 只在控件实例解析 token 时访问
`snapshot.Controls[controlSlot]`（ThemeTokenResourceProvider.cs:59/87/102，另 ThemeTokenResolver.cs 同形态按槽访问）。空窗口把 87 个控件
（Own Token 计算 34.5 + 冻结 17.5 + 投影 9.9 ≈ 62ms）全部预编译，换来的是零次查询。

**消费者清单修正（2026-10-11 复核补充）**：上述"零消费者"仅指资源查询路径。`ThemeSnapshot` 构造器急切调用
`EstimateSnapshot(this)`（ThemeSnapshot.cs:36），`ThemeRetainedBytesEstimator.EstimateSnapshot`（:43）
foreach 全量枚举 `Controls` 汇总保留字节，估算值被 `ThemeSnapshotCache`(:27) 与
`ControlCompilationCache`(:16) 用于缓存容量记账。按 slot 惰性化必须同步把该估算改为
"全局部分 + 已物化槽位增量累计"（槽位编译时回填），否则快照构造即全量物化，惰性收益归零。
此外 `CanReuseControl`/复用路径（ThemeCompiler.cs:278）读取 `parent.Controls[slot]`，主题切换时需按
changeSet 惰性继承（未物化槽位不得以"读取父槽位"方式被动触发编译）。

### 修复方案：按 slot 惰性编译

```
ThemeSnapshot.Controls 由 IReadOnlyList<ControlThemeSnapshot> 改为按需槽位数组：
  internal sealed class LazyControlSnapshots
  {
      private readonly ControlThemeSnapshot?[] _materialized;   // 槽位缓存
      private readonly ThemeCompileInput _input;                // 全局值/配置/外观已冻结
      public ControlThemeSnapshot this[int slot]                // 首次访问该槽位时编译并缓存
  }
- 编译输入确定性：同一 snapshot 内 globalValues/globalResources/palettes/config/appearance 不变，
  槽位编译结果与全量预编译逐位相等（可差分测试证明）。
- 并发：资源查询可从任意线程；槽位用 LazyInitializer/双检锁，与现有 ControlCompilationCache 语义一致。
- 事务缓存键不变：snapshot 级 key 已覆盖全部输入；槽位编译不引入新 key。
- 保留字节估算：`ThemeSnapshot.EstimatedRetainedBytes` 改为全局部分急切 + 槽位增量累计（槽位物化时回填），
  不得在快照构造时枚举槽位（见上方"消费者清单修正"）。
- 主题切换：Compare/CanReuseControl 路径需要 Controls 计数与已编译槽位——切换时按 changeSet 惰性
  继承（未编译槽位直接标记惰性，已编译槽位走既有复用判断）。
```

### 契约修订决策点（必须显式批准）

[启动与注册链路](../../architecture/foundations/startup-and-registration.md)的"首帧完整主题提交"契约
目前按"全部控件快照编译完成"解释。本方案把契约修订为"全局 token/资源完整提交，控件级快照按需编译"：
首帧内首次实例化的控件在实例化时支付自己的编译（单控件 ~0.7ms，含 JIT），不改变任何资源查找结果，
但改变"切换主题后控件 token 立即可查"为"首次查询时可查"。这是一次**行为契约修订**，不是纯等价优化，
实施前需用户批准并同步修订架构文档。

### 验收要点

1. 差分测试：全量预编译 vs 惰性编译，全部 187 槽位逐 token/逐资源相等。
2. 空窗口断言：启动后实际编译槽位数 ≈ 0-2（仅 Window 链），记录精确回归门限。
3. 主题切换（Light/Dark/Compact/局部 scope）前后资源查询结果不变；切换耗时对照。
4. 并发查询：多线程首次访问同一槽位只编译一次。

预期收益：空窗口 CompileControls 72.3 → **~2-5ms**（仅 Window 所需槽位）；
代价：每控件首次实例化 +~0.5-0.7ms（Gallery 页面导航首批控件摊销）。

### 实施与实测（2026-10-11 已实施，契约修订经用户批准）

实施：新增 `LazyControlSnapshots`（Count + 索引器，不实现枚举）+ `LazyControlSnapshotSource`
（冻结编译输入）+ `ControlSlotState`（Materialized/ReuseParent/Uncompiled 构造期急切解析）；
`CompileControls` 改惰性解析循环，旧急切循环原样保留为 `CompileControlsEagerForTest` 差分钩子；
`ThemeSnapshot.EstimatedRetainedBytes` 改实时属性（构造期基线 + `MaterializedBytes` 增量），
估算器删全量枚举；契约修订落入 [runtime.md](../../architecture/systems/theming/runtime.md) §7/§8。
spec：`docs/superpowers/specs/2026-10-11-hotspot-c-lazy-control-snapshots-design.md`。

实测（交错 10× 采样中位数，对照 b3557e188 基线）：

| 指标 | 基线 | 惰性 | Δ |
|---|---:|---:|---:|
| UseAtomUI.SyncTotal | 452.2 | 391.2 | **−60.9** |
| UseAtomUI.PostConfigure | 255.9 | 189.8 | **−66.1** |
| Mark.FrameworkCompleted | 686.7 | 644.9 | −41.8 |
| Mark.FirstRender | 962.4 | 935.6 | −26.8 |
| Mark.DispatcherIdle | 1032.1 | 1001.1 | −31.0 |

验收：193 槽位真实注册表差分（主变体 + ReuseParent/Uncompiled 混合变体）逐 token/逐资源/逐保留字节
结构相等 ✓；启动 MaterializedCount 实测 **0**（精确棘轮）✓；并发单飞（同槽/异槽）✓；估算增量恒等 ✓；
主题切换 Committed + 首查契约 ✓。差分方法论修正：Easing/Brush/Transform 每编译克隆自有不可变副本
（所有权边界），引用不等是既有设计，差分按结构比较（eager-vs-eager 探针证实与惰性化无关）。

---

## 热点 D：ThemeManagerBuilder.Build 46.1ms —— Revision 指纹与冻结仍全量

### 源码定位与归因

- `ThemeSchemaRegistry` 构造器（ThemeSchemaRegistry.cs:20-100）：`OrderBy`×3（全局/控件/算法，
  输入本就有序）+ `BuildUniqueMap` + `ToFrozenDictionary`×5；
- `ComputeRevision`（:404-460）：对 255 全局 token + 187 控件 ×（identity + own token 全量）+ 318 资产 ×
  `AddAssetSchema` 做第三轮全量 FNV。`914ce40ef` 加了 typeNames 操作级缓存（重复 Type 不再重复拼
  AssemblyName），但遍历结构本身未变。
- `ThemeManagerBuilder.BuildCore`（ThemeManagerBuilder.cs:198-278）：descriptor 汇集 + `HasSameMetadata`
  去重 + `PrepareRegistrationSchema`（:276，registry 构造在 Build 内完成，进入 InitializeApplication 时
  只剩 0ms 的 `??=`）。

### 修复方案（承接上轮方案 4 的未实施部分）

```
(1) Revision 编译期常量：生成器为每包 emit 包级 schema revision 常量（构建时算好全部 token/控件/资产
    指纹），运行时 ComputeRevision 退化为组合各包常量的轻量哈希。Registry.Revision 仅用于进程内缓存
    失效判定，只需进程内一致。跨进程稳定性不是契约（binding cache 键已含 contentDigest）。
(2) 免排序直入：与热点 A(2) 同一 PreSorted 入口，OrderBy 换 ValidateDenseSlots 的 O(n) 有序断言。
(3) HasSameMetadata 手写逐字段比较（去 LINQ Select 投影分配）。
```

### 验收要点

同上轮方案 4：查询行为不变、Revision 变化触发重编译路径保持（ThemeStartup/ThemeFollowSystem 测试）、
跨包冲突失败行为不变。

预期收益：**~20-30ms**。
【已验证 2026-10-11：探针实测 3.1-4.7ms（Revision 全量遍历 4.06-5.93ms → 包常量组合 0.94-1.25ms），
远低于估计——估计基于 Task 2 前缀续算前的测量，该段主份额已被 Task 2 取走。】

---

## 热点 E：ApplyThemeAsync 非编译部分 16.0ms + 全局首次 CopyBuilder 7.2ms

### 源码定位与归因

- **16.0ms（配置合并、事务准备/提交等）**：`PrepareThemeCore`（ThemeManager.cs:1595-1665）——
  `CreateDefinitionDefaults`（CWT 缓存，首次 1.4ms）+ `ThemeConfigMerger.Merge`×2（:1618/:1622，
  默认值→应用配置→运行时配置两轮）+ `ThemeConfigMerger.Compare`（:1626，启动时 currentSnapshot 为 null
  已跳过）+ `ThemeSnapshotCacheKey.Create`（对 effectiveConfig 全量哈希）+ 事务入队/`CommitCore`/`Publish`
  （RequestedThemeVariant + `Resources.MergedDictionaries.Add(root provider)` + `RootThemeContext.Publish`）。
- **首次 CopyBuilder 7.2ms**：`EvaluateAlgorithms`（ThemeCompiler.cs:631-646）第一个算法链的
  seed→map 复制。`914ce40ef` 已改为生成式 `CopyGlobalValues` 强类型复制并跳过 canonical 种子重初始化，
  实测首次 18.4→7.4ms——**这就是优化后的残留值**，成分为 255 属性强类型复制的首次 JIT + 执行 +
  14 组调色板 CopyColorMap。warm 已无意义（每进程只执行一次）。

### 结论与处置

两者均为**首次执行成本**，算法上无可消除的重复工作（合并/哈希/事务各执行一次且必要）。
7.2ms 的生成式复制已到强类型极限。唯一可再压的是 Merge 双跳（defaults→applicationConfig→effective），
启动时 applicationConfig 与 effective 的输入几乎恒等（initial config 与 runtime config 同源），可加
"三方合一"快速路径，预计 ≤3ms，收益太小**不建议单独立项**，随热点 C 实施时顺手附带。

---

## 热点 F：Main → App.Initialize 292.6ms 与 Window/Opened/Render 163.4ms —— 非 AtomUI 算法射程

### 归因

- **Main → App.Initialize 292.6ms**：CoreCLR 启动 + 程序集装载（`UseAtomUIPlatformDetect` 触发
  Desktop.Controls→Controls→Core→Localization→Controls.Shared→Icons→Native 全链装载）+ JIT +
  Avalonia macOS 平台初始化（Avn 桥/Metal）。此段无 AtomUI 算法代码执行。
  Windows 纯 Avalonia 对照（531ms 全程）证明平台底噪量级；
  AtomUI 程序集图的装载部分是此段中 AtomUI 唯一可归因成分（数十 ms 量级）。
- **Framework 完成/Window 构造 79.4 + Opened 42.4 + Render 41.6ms**：首个 TopLevel 原生窗口创建、
  模板应用、渲染线程启动、首帧绘制。`MediaBreakPointThemeBootstrapper.Rebuild` 在 ModuleInitializers
  段触发的 `TryGetResource(typeof(Window))` 只会物化 Window 所需资产（49 deferred 的设计意图）。

### 处置

算法层面无动作项。部署层手段（单列，不计入方案收益）：

1. **ReadyToRun**：AtomUI 程序集 R2R 预编译直接压缩全链 JIT（热点 A/C/E 的"首次 JIT"成分 +
   本段装载 JIT），是 CoreCLR 冷启动收益最大的非算法手段；需要确认 NuGet 包发布管线支持。
2. **NativeAOT**：Gallery 已有发布路径（macOS Homebrew targets），AOT 下本段与全部 JIT 项消失。

---

## 热点 G：LocalizationBuilder.Build 其余 25.4ms —— ICU 与首次执行，方案空间小

### 源码定位与归因

`LocalizationBuilder.Build`（LocalizationBuilder.cs:66-111）实测 29.8ms，其中已优化的
`LanguageCatalogRegistry.Create` 2.7ms + `LanguageSnapshotBuilder.Build` 1.6ms（`914ce40ef` 的
MatchedBundleResolver + 覆盖/冲突合并扫描）。剩余 25.4ms 的构成（源码推演，无独立测量）：

1. `ResolveSupportedDefinitions` → `GeneratedStandardLanguageDefinitions.TryCreate` →
   **`CultureInfo.GetCultureInfo("en-US")` 首次调用**（LanguageTagsSourceWriter.cs 生成）——
   冷进程首次触达 ICU/Globalization 数据加载，量级 10-25ms，是本子项最大嫌疑；
2. `LanguageManager` 构造（definitionArray/definitionsByTag/snapshotCopies 三次复制，LanguageManager.cs:34-36）
   + `LanguageState/LanguageContext/LanguageResourceProvider/Localizer/LocalizationHost` 构造——纯托管，
   微秒级；
3. 上述全部类型的首次 JIT。

### 处置

ICU 初始化与 JIT 无算法空间。`LanguageManager` 的三次小复制已在上轮审查中因"独立所有权/校验必须保留"
被裁定保留。**诚实结论：本子项无可立项的优化**；若未来需要证据，单独测一次
`CultureInfo.GetCultureInfo` 冷调用即可定位 ICU 占比。

---

## 收益汇总与实施顺序（修订）

| 热点 | 实测 | 方案 | 预期后 |
|---|---:|---|---:|
| A UseDesktopControls | 192.4ms | 共享前缀指纹 + 预排序直通 | ~100-130ms |
| B MountStaticResources | 89.2ms | classless wrapper 生成等价类 | ~20-40ms |
| C CompileControls | 72.3ms | 按 slot 惰性编译（**契约修订**） | ~2-5ms |
| D ThemeManagerBuilder | 46.1ms | Revision 常量 + 免排序 + 去 LINQ | ~20-26ms |
| E 事务/全局复制 | 23.0ms | 首次执行成本，无立项项 | 不变 |
| F 装载/窗口/渲染 | 455.6ms | 非算法；ReadyToRun/AOT 另行立项 | 不变 |
| G Localization 其余 | 25.4ms | ICU/JIT，无立项项 | 不变 |
| **UseAtomUI 段合计** | **543.6ms** | | **~270-330ms** |

实施顺序（每轮单因素、同输入前后对照）：

1. **A**（无契约修订，纯等价：指纹逐位相等 + 排序免执行）。
2. **D**（与 A 同属注册/冻结层，可同批做差分）。
3. **B**（需要新的等价性证据链：262 个 wrapper 逐资产可观测断言 + 回退路径）。
4. **C**（契约修订决策 + 全量差分 + 槽位并发测试，风险最高，最后做）。

上轮方案对账：问题 1（通用惰性代理）已被 `914ce40ef` 的"有条件 AddDeferred/直接构造"实现并收敛；
问题 3 的 Format/Parse 归因被生成式复制证伪关闭，剩余全量预编译问题由本文件热点 C 接管；
问题 2/4 的未实施部分由本文件热点 A/D 接管；问题 5 已由内置源构建期校验实施（LoadThemeCatalog 40→12.4ms）；
问题 6 已由 MatchedBundleResolver 实施。

本文所有预期收益为源码复杂度推算，**未经实现验证**；每项实施后回填实测并更新状态。
【2026-10-11 状态更新：热点 A/B/C/D 已实施并实测回填（见上方"最终实测收益"表与热点 C 节"实施与实测"）；热点 E 维持"不立项"结论。】
