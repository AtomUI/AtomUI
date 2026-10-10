# AtomUI 空窗口启动性能问题与优化方案（源码深挖版）

分析日期：2026-10-10。状态：源码级定位与优化算法设计完成，**尚未实施任何优化**。

基线：HEAD `ee1db4001` + 工作树两项未提交改动（ThemeSchemaRegistry 字符指纹优化、ILC8 条件注册后端）。
测量方法见 [2026-10-10 启动性能分析](2026-10-10-startup-performance-analysis.md) 的"空窗口启动基线实测"一节；
本文只记录源码级问题定位与修复方案，不重复测量过程。

外部框架行为核对来源（Avalonia main 分支，版本 12.1.3 对应实现一致）：

- `ResourceInclude`：`Loaded` 属性惰性加载，`Source` 设置不触发加载，首次 `TryGetResource`/`AddOwner` 才加载。
- `ResourceDictionary.TryGetResource`：先查自身与 theme 字典，再对 `MergedDictionaries` **按索引逆序**遍历，
  合并字典的相对顺序即资源查找优先级。

## 实测阶段与热点总表

空窗口（en-US 单语言、默认主题、AlibabaSans、UseDesktopControls、空白 Avalonia Window，Release）：

| 阶段 | 实测 | 源码入口 |
|---|---|---|
| Main→App.Initialize 进入 | ~210-325ms | 运行时装载/JIT/Avalonia 平台初始化（非 AtomUI 可控） |
| UseLanguages | ~7ms | |
| UseAlibabaSansFont | ~20ms | `AlibabaSansThemeManagerBuilderExtensions.UseAlibabaSansFont` |
| UseDesktopControls | ~230ms ★ | `GeneratedTypeMapRegistration.Collect`（生成代码，src/AtomUI.Desktop.Controls GeneratedFiles:2569） |
| LocalizationBuilder.Build | ~33ms | `LanguageCatalogRegistry.Create` + `LanguageSnapshotBuilder.Build` |
| ThemeManagerBuilder.Build | ~87ms ★ | `ThemeManagerBuilder.BuildCore`（src/AtomUI.Core/Theme/ThemeManagerBuilder.cs:184） |
| LoadThemeCatalog | ~40ms | `CompiledThemeCatalog.LoadSource`（src/AtomUI.Core/Theme/Definitions/CompiledThemeCatalog.cs:330） |
| MountStaticResources | ~140-195ms ★★ | `ThemeManager.MountStaticResources`（src/AtomUI.Core/Theme/ThemeManager.cs:1722） |
| ApplyThemeAsync 首次编译 | ~120-160ms | `ThemeCompiler.Compile`；其中 CompileControls ~100ms ★ |
| Styles 挂载收尾 + ModuleInitializers | ~130ms | `ThemeManager.InitializeApplication` 尾部（ThemeManager.cs:195-200） |
| Window 构造→Opened | ~150-170ms | 框架边界（首个 TopLevel 平台初始化+首帧），不在 AtomUI 优化射程 |

UseAtomUI 同步段合计 ~460-490ms；本机空窗口 Main→窗口出现 ~1.2s（Release）。
Gallery 全量配置（四语言、五包、五主题、用户主题目录、WorkspaceWindow）按比例推算 ~1.8s，与用户观察一致。

---

## 问题 1（★★ 最重单项）：MountStaticResources 为惰性内容付出急切包装成本

### 源码定位

- 挂载循环：`ThemeManager.MountStaticResources`（src/AtomUI.Core/Theme/ThemeManager.cs:1722-1742）——
  遍历 `_stagedThemeResources`，逐个执行 `registration.Factory()`，再 `Resources.MergedDictionaries.Add`。
- 工厂本体：构建期任务 `GenerateThemeAssetWrappersTask`（src/AtomUI.Toolchain/Tasks/GenerateThemeAssetWrappersTask.cs:69-89）
  为每个主题资产生成两层包装：
  - C# 侧 `GeneratedThemeAssetResource_XXX : ResourceDictionary`，构造函数调用 `AvaloniaXamlLoader.Load(this)`
    （.artifacts/.../AtomUIThemeAssets/GeneratedThemeAssetResources.g.cs，Desktop 包 317 个类）；
  - AXAML 侧仅含一条 `ResourceInclude Source="..._Deferred.axaml"`，Deferred 文件才承载真实控件主题。
- `ControlThemeResourceRegistration` 携带 `CreateResource_XXX` 工厂委托（GeneratedTypeMapRegistration.g.cs:10167 形态）。

### 算法问题

实测该阶段物化 324 个资源工厂耗时 ~105-116ms（每包 ~0.33ms），加上 `MergedDictionaries.Add` 挂接共 ~140-195ms。
而 Avalonia `ResourceInclude.Loaded` 是惰性的：真实控件主题（TextBoxTheme 等，`_Deferred.axaml`）
**在启动期根本不会加载**。这 324 次 `AvaloniaXamlLoader.Load(this)` 加载的只是"包装字典自身"——
XAML 编译产物类型查找、ResourceDictionary 分配、单条 MergedDictionaries populate。
即：**为本身已设计为惰性的内容，急切构造了一整层包装**。空窗口未创建任何 AtomUI 控件，这 324 个包装
中绝大多数在进程生命周期内可能只被遍历、从不被命中。

### 修复方案：惰性代理挂载（Lazy Proxy Mount）

注册期与挂载期只创建轻量代理，首次资源访问才物化真实包装：

```
DeferredThemeResourceProxy : IResourceProvider, IThemeVariantProvider
  字段：AssetId、factory 委托、volatile IResourceProvider? _inner
  TryGetResource / HasResources / AddOwner / RemoveOwner / OwnerChanged：
      EnsureMaterialized() 后转发内部实例
  EnsureMaterialized()：factory() → 写 _inner（双检锁）
```

- `MountStaticResources` 改为 `Resources.MergedDictionaries.Add(new DeferredThemeResourceProxy(registration))`，
  324 次纯对象分配，零 XAML 调用。
- **顺序契约**：`ResourceDictionary.TryGetResource` 对合并字典按索引逆序遍历，代理占据原 registration 位置，
  包内与跨包资源优先级不变。
- **转发契约**：代理必须转发 `OwnerChanged`（add/remove 都转发到已物化实例；未物化时登记挂起订阅）与
  `IThemeVariantProvider.Key`；`AddOwner` 是最常见的首个触发点。
- **reload 路径**：`ReloadThemesAsync` 重建 catalog 后替换 registration 时换新代理；旧代理若已物化，
  沿用现有资源字典替换语义。
- **连带收益**：`MediaBreakPointThemeBootstrapper.Rebuild`（src/AtomUI.Desktop.Controls/Window/MediaBreakPointThemeBootstrapper.cs:68-72）
  首次 `TryGetResource(typeof(Window))` 只会物化 Window 所需的一个包装，而非全量。

### 验收要点

1. 页面测试断言：空窗口启动后实际物化资产数远小于 324（记录精确数字作为回归门限）。
2. 全部控件 ControlTheme 查找仍命中（复用现有资源查询/主题契约测试）。
3. `OwnerChanged`/ThemeVariant 转发正确；主题切换（Light/Dark）后 `ThemeTokenResourceProvider` 通知不丢。
4. reload：修改用户目录主题定义 → `ReloadThemesAsync` → 新定义生效且旧代理不复活。

预期收益：~120-180ms（该阶段近清零）。

---

## 问题 2（★）：包注册期 CreateGenerated 双重全量指纹与重复排序

### 源码定位

- Collect 入口：生成代码 `GeneratedControlPackageRegistration.Collect`（src/AtomUI.Desktop.Controls/GeneratedFiles/
  AtomUI.Generator/AtomUI.Generator.TokenResourceKeyGenerator/GeneratedTypeMapRegistration.g.cs:2569）——
  319 次 `new ControlFragment_XXX().Add(builder)`。
- 每个 fragment：物化 `ControlTokenDescriptor`（`ownTokens.OrderBy(slot).ToArray()`，
  src/AtomUI.Core/Theme/Schema/ControlTokenDescriptor.cs:52）+ Semantic descriptor + `AddAsset_XXX`。
- 资产 descriptor：`ControlThemeAssetDescriptor.CreateGenerated`
  （src/AtomUI.Core/Theme/Schema/ControlThemeAssetDescriptor.cs:56-86）。

### 算法问题

1. **双重全量指纹**：`CreateGenerated` 对每个资产执行
   `ComputeGeneratedContractFingerprint`（ThemeSchemaRegistry.cs:155-199）与
   `ComputeGeneratedResourceKeySchemaFingerprint`（ThemeSchemaRegistry.cs:141-153），
   两者都对 `CompiledGlobalTokenNames`（255 个名字的包级共享数组）逐字符 FNV。
   Desktop 包 318 资产 × 2 指纹 × 255 名字 × 平均 ~20 字符 ≈ **320 万次乘加哈希**。
   而生成器编译期已算出同样值并作为十六进制常量传入（如 `0x6E0C0417C86EC8B9UL`），运行时重算仅用于
   "过期生成资产"校验。
2. **构造器内三层 LINQ 排序**（ControlThemeAssetDescriptor.cs:43-51）：`exports.OrderBy(AQN).ThenBy(...)`、
   `Canonicalize` 字典化+排序、`bindings.Distinct().OrderBy×4`。生成器 emit 的本来就是确定顺序，
   运行时再排一次纯属重复；AQN 字符串每次拼接 100+ 字符。
3. `ControlTokenDescriptor` 构造对已按 slot 有序的生成数组再 `OrderBy(slot)`。

### 修复方案：共享前缀指纹 + 预排序直通 + 校验抽查

```
(1) 指纹重排共享前缀：
    调整两个指纹函数的字段顺序，使 [全局 token 名列表] 位于最前、资产专有字段在后。
    FNV-1a 是顺序敏感的链式哈希 → 名字段成为可共享前缀状态。
    每包构建一次 prefixState（缓存在包级静态字段），每资产从 prefixState 续算专有字段。
    复杂度 O(assets×names) → O(names + assets)：318×255 → 255+318。
    生成器与运行时同源修改、同步重发常量（同一次原子提交，无新旧混编兼容问题）。

(2) 预排序直通构造：
    生成器按已排序顺序 emit exports/owners/bindings；新增 internal 构造器入口接受
    PreSorted 标记，跳过 OrderBy/Canonicalize 排序，仅保留 O(n) 线性冲突查重
    （顺序敏感语义由生成器输出保证，运行时只验不排）。

(3) 指纹校验降级为抽查：
    每包首个资产做全量指纹重算校验（证明生成器-运行时一致性），其余资产只比对常量。
    常量不等仍抛 ThemeSchemaException（过期生成资产检测不消失）；
    放弃的仅是"每资产独立重算"这一冗余证明形式。
```

### 验收要点

1. 新旧指纹逐位相等测试（临时差分测试：旧算法结果 vs 新顺序结果，全资产枚举）。
2. 过期资产契约检测保留：手工构造错误常量的 descriptor 必须抛 `ThemeSchemaException`。
3. 冲突检测保留：重复 AssetId、冲突 owner、重复 export key 仍报错。
4. 涉及生成器与裁剪边界：跑对应 AOT 专项（TypeMap 契约不变）。

预期收益：~100-150ms。

---

## 问题 3（★）：CompileControls 逐控件全量复制全局 DesignToken

### 源码定位

- 循环：`ThemeCompiler.CompileControls`（src/AtomUI.Core/Theme/Compilation/ThemeCompiler.cs:234-314），187 控件。
- 单控件：`CompileControl`（ThemeCompiler.cs:351-438）第一行 `CopyBuilder(globalBuilder, 255 descriptors)`。
- 复制实现：`CopyBuilder`（ThemeCompiler.cs:648-673）——逐 token `GetValue`（装箱）→
  引用类型走 `Format`（装箱 ToString）→ `Parse`（字符串解析）往返 → `SetValue`（拆箱），
  再复制 10 组 `ColorPalettes`。
- 消费者：`controlBuilder.AssignEffectiveGlobalToken(effectiveGlobalBuilder)`
  （ThemeCompiler.cs:420 → AbstractControlDesignToken.cs:10-13）→
  控件 token 的 `CalculateTokenValues` **只读**引用（如 AlertToken.cs:67-82 只读
  `EffectiveGlobalToken.FontSizeHeading3` 等）。
- `EvaluateAlgorithms`（ThemeCompiler.cs:631-646）每个算法链也走一次 `CopyBuilder`。

### 算法问题

启动时所有控件 `config == null` → `effectiveGlobalMatchesRoot = true` → 不产生任何 delta。
即：183 个有 OwnToken 的控件各自复制一份 255 token 的全局 DesignToken，
**仅为给只读的 `CalculateTokenValues` 提供输入**；当前源码中不存在任何控件写这份副本。
Format/Parse 字符串往返的本意是"值规范化隔离"，对只读场景是纯开销：
~183 × (255 次装箱复制 + 引用类型字符串往返 + 10 次调色板复制) ≈ 100ms。

### 修复方案：共享只读全局 + 慢路径类型化克隆

```
(1) 快路径（config == null 且 appearance 与全局一致）：
    直接 controlBuilder.AssignEffectiveGlobalToken(globalBuilder)——全部控件共享根
    builder 实例，零复制。CompileControl 该分支不再调用 CopyBuilder。

(2) 只读契约守护：
    Debug 构建给 DesignToken 加 Freeze 后只读标记（setter 写已冻结实例时抛异常）；
    或由生成器对 [ControlDesignToken] 类的 CalculateTokenValues 做静态分析，
    禁止对 EffectiveGlobalToken 赋值。Release 构建零开销。

(3) 慢路径（控件级 Global/Custom 算法模式，ReevaluateControlGlobal）保留复制，但把
    Format/Parse 字符串往返替换为类型化克隆 switch（Color/Thickness/FontFamily/Brush/
    Easing 各自的不可变 Clone）。语义等价：不可变克隆是比字符串往返更强的规范化
    （Format/Parse 原本就假设值可无损往返）。EvaluateAlgorithms 的逐算法复制同步受益。
```

### 验收要点

1. 全控件 `CalculateTokenValues` 只读性静态检查通过（或 Debug 写守护在测试套件中零触发）。
2. 主题切换差分测试：Light/Dark/Compact/局部 scope 前后，全部控件 ThemeSnapshot 逐 token 与旧实现相等。
3. 控件级 Global/Custom 算法模式（Gallery 中对应 ShowCase）行为不变。
4. 控件间隔离回归：确认没有任何控件 token 类写 `EffectiveGlobalToken.*` 属性。

预期收益：CompileControls ~100ms → ~25-35ms。

---

## 问题 4：ThemeManagerBuilder/ThemeSchemaRegistry 重复排序与第三轮全量指纹

### 源码定位

- `ThemeManagerBuilder.BuildCore`（src/AtomUI.Core/Theme/ThemeManagerBuilder.cs:198-278）：
  descriptor 汇集、`HasSameMetadata` 去重（`SequenceEqual(Select(...))`，每次比较分配 LINQ 投影）、
  `PrepareRegistrationSchema`（:276）。
- `ThemeSchemaRegistry` 构造器（src/AtomUI.Core/Theme/Schema/ThemeSchemaRegistry.cs:20-100）：
  全量 `OrderBy` ×4（输入本就是 slot 有序的生成数组，`ValidateDenseSlots` 随后还要再断言有序）、
  `BuildUniqueMap` ×3、`ToFrozenDictionary` ×5。
- `ComputeRevision`（ThemeSchemaRegistry.cs:404-447）：对 183 控件逐个 AQN + 255 token 明细 +
  318 资产逐个 `AddAssetSchema`（又是 AQN），这是同一 schema 信息在启动路径的**第三轮**全量指纹
  （生成期 → CreateGenerated → ComputeRevision）。

### 算法问题

同一份信息三轮指纹化、两次排序、LINQ 分配遍布去重路径，总贡献 ~87ms 中除 descriptor 汇集外的主体。

### 修复方案

```
(1) 免排序直入：生成器保证 slot/Catalog/Id 有序（现状即如此）；registry 构造器增加
    internal 预排序入口，OrderBy 替换为已有的 ValidateDenseSlots O(n) 有序断言。
(2) Revision 编译期化：生成器为每包 emit 包级 schema revision 常量（构建时算好）；
    运行时 ComputeRevision 退化为组合各包常量的轻量哈希。
    Registry.Revision 仅用于进程内缓存失效判定，值只需进程内一致，跨进程稳定性不是契约。
(3) HasSameMetadata 去 LINQ 化：手写循环逐字段比较，零分配。
```

### 验收要点

1. 相同注册集合下 Registry 的查询行为（identity/type/slot/资源键）不变。
2. 主题切换缓存失效仍正确：Revision 变化触发重编译的路径保持（现有 ThemeStartup/ThemeFollowSystem 测试）。
3. 跨包重复注册、冲突 owner 失败行为不变。

预期收益：~50-65ms。

---

## 问题 5：LoadThemeCatalog 对内置来源执行 XSD 校验与过量缓冲分配

### 源码定位

- 读取路径：`CompiledThemeCatalog.LoadSource`（src/AtomUI.Core/Theme/Definitions/CompiledThemeCatalog.cs:330-444）：
  `ReadBounded`（:446-465，固定 81,920 字节缓冲，实际内置主题 XML 仅 385 字节）→ SHA256 → XmlReader。
- XSD 校验：`ThemeDocumentReader.CreateReaderSettings`（src/AtomUI.Core/Theme/Definitions/ThemeDocumentReader.cs:101-133）
  `ValidationType.Schema` + 首次触发 `Lazy<XmlSchemaSet>` 编译；对全部来源（含内置 avares）生效。

### 算法问题

内置 avares 来源是构建期产物，且其契约已被问题 2 的生成期指纹覆盖；对它执行运行时 XSD 校验
（含首次 schema 编译成本）与 80KB 固定缓冲读取属于重复防护。

### 修复方案

```
(1) 来源分级：CoreThemeDefinitionResolver/AvaloniaAssetThemeDefinitionResolver 的内置来源
    跳过 XSD 校验（仍保留 XML 良构性解析与读取上限）；UserDirectoryThemeDefinitionResolver
    保留全量 XSD 校验——用户输入才是运行时校验的目标对象。
(2) ReadBounded 缓冲区按流长度就近分配（CanSeek 时直接按 Length 分配一次性读），
    保留 maxBytes 上限与异常语义。
```

### 验收要点

1. 内置主题加载结果与诊断逐字节一致（临时差分测试）。
2. 用户目录非法 XML / 超 / 违反 XSD 的定义仍产生原有诊断与回退行为。
3. reload 变更识别（内容摘要路径）不受影响。

预期收益：~15-25ms。

---

## 问题 6：本地化 Snapshot 逐词条线性扫描

### 源码定位

- `LanguageCatalogRegistry.Create` 的 `ValidateEnglishSource`
  （src/AtomUI.Localization/Catalog/LanguageCatalogRegistry.cs:148-164）：每 unit 对全部 bundle `Any` 扫描，
  O(units × bundles)。
- `LanguageSnapshotBuilder.TryResolveValue`（src/AtomUI.Localization/LanguageSnapshotBuilder.cs:97-147）：
  每词条按 3 优先级 × 候选语言 × 全部 bundle 线性查找（`TryFindBundleValue` :149-172）；
  顶层再加 `s_sourcePriorities` 外层循环。
- Gallery 启动为全部支持语言构建 Snapshot（四语言），单语言 ~33ms 按比例 ~130ms。

### 算法问题

查找结构未预建索引：registry 已按 `(Language, SourceKind)` 排序 bundle 列表，但查询仍是逐 bundle 线性比较。

### 修复方案

```
(1) registry 冻结时构建 (LanguageTag, TranslationSourceKind) → bundle 的字典索引
    （同 key 多 bundle 保持有序列表，兼容同优先级非重叠互补语义）。
(2) 每 catalog 预计算 en-US 覆盖位图（ulong 位集，每 unit 一位），
    ValidateEnglishSource 与 requiresTranslatedCoverage 检查降为 O(units/64)。
(3) TryResolveValue 主循环改为：候选语言 × 优先级直接索引取 bundle，判 Values[unitSlot] 非空即返回；
    保留来源优先级先于语言具体程度、en-US 最终回退与诊断语义。
```

### 验收要点

1. 全部支持语言的 Snapshot 与旧实现逐词条相等（含 CompositeFormat 解析结果与覆盖异常）。
2. 语言覆盖检查（仅靠 en-US 回退的翻译缺失异常）与冲突诊断行为不变。
3. 复用 LanguageSnapshot/LanguageCatalogRegistry 现有测试。

预期收益：单语言 ~25ms、Gallery 四语言 ~100ms → 合计 ~30-40ms。

---

## 次级观察（不单列方案）

- `UseAlibabaSansFont` ~20ms：`EmbeddedFontCollection` 构造即扫描 avares 字体资产清单。属一次性必要成本，
  除非后续测量证明 6 个 ttf 的清单扫描异常，否则不动。
- ModuleInitializers ~27ms（动画器注册、ToolTipService、MediaBreakPointThemeBootstrapper.Attach）：
  其中 `Rebuild` 的 `TryGetResource(typeof(Window))` 沿 324 个合并字典逆序遍历查找，
  是问题 1 代理方案的直接受益者，不需要单独修复。
- Main→Initialize 的 ~210-325ms 与 Window→Opened 的 ~150-170ms 属运行时/装载与 Avalonia 首个 TopLevel/
  首帧成本，非算法问题；优化对比时必须单列，避免误归因。NativeAOT 发布是另一条改善路径，不在本文范围。

## 收益汇总与实施顺序

| 问题 | 实测 | 修复方案 | 预期后 |
|---|---|---|---|
| 1 MountStaticResources | 140-195ms | 惰性代理挂载 | ~5-15ms |
| 2 包注册指纹/排序 | ~230ms（UseDesktopControls 段） | 共享前缀指纹+预排序+抽查 | ~80-120ms |
| 4 Builder/Registry | ~87ms | 免排序+Revision 常量+去 LINQ | ~25-35ms |
| 3 CompileControls | ~100ms | 共享只读全局+类型化克隆 | ~25-35ms |
| 5 LoadThemeCatalog | ~40ms | 内置源免 XSD+按长缓冲 | ~15-25ms |
| 6 Localization 单语言 | ~33ms | bundle 索引+覆盖位图 | ~8-12ms |
| **UseAtomUI 合计** | **~460-490ms** | | **~160-240ms** |

骨架空窗口（本机 ~1.2s）预计降到 ~0.9-1.0s；Gallery 全量配置按比例从 ~1.8s 降到 ~1.0-1.2ms。

实施顺序（每轮单因素对照，前后用同一骨架探针复测）：

1. **问题 1**：收益最大且独立可测；先固化探针骨架为常驻测量 harness。
2. **问题 2 + 4**：同属"指纹与排序减量"主题，生成器与运行时原子提交；新旧指纹逐位比对。
3. **问题 3**：需要只读契约守护与全量快照差分，风险最高，放在工具链就绪后。
4. **问题 5 + 6**：低风险收尾。

通用验收约束：每轮固定相同输入与样本策略（≥10 次冷启动），无收益或回归即回退；清理临时差分测试后
做 focused 验证；涉及生成器/注册/裁剪边界的问题 2/3/4 完成 AOT 专项；全部实施遵守
[启动与注册链路](../../architecture/foundations/startup-and-registration.md)声明的失败行为与契约保留边界。

本文所有"预期收益"均为算法复杂度与实测占比推算，**未经实现验证，不声明已达成**；
每项实施后须回填实测数字并更新本文状态。
