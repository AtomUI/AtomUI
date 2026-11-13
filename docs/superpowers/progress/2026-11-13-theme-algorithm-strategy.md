# 主题算法策略化 执行台账

- 规格：`docs/superpowers/specs/2026-11-13-theme-algorithm-strategy-design.md`
- 计划：`docs/superpowers/plans/2026-11-13-theme-algorithm-strategy.md`
- 分支：`feature/design-language`，基线 `release/6.0`

每个任务结束时登记：提交号、测试结果、性能数字、与计划的偏差。裁决写成「裁决：决定 — 原因 — 判断错了的代价」。

## 执行前

### 用户的决定（2026-11-13）

- 执行方式：子代理逐任务执行，子代理一律使用 Opus。
- 提交方式：每个任务审阅通过后由主控提交到本分支，不推送；实现者不提交。
- 变基：先变基到 `release/6.0` 再开始。

### 变基到改写历史后的 `release/6.0`

`release/6.0` 在 2026-11-13 被改写过提交信息（内容未变：旧基点 `c6820d19b` 与新历史中的对应提交 `65e266c76` 树完全相同），之后新增到 `f11459de0`（Avalonia 12.1.6）。

- 做法：`git rebase --onto release/6.0 c6820d19b`，只重放本分支自己的 226 个提交。变基前的分支顶端保留在本地分支 `backup/design-language-pre-rebase-20261113`。
- 上游新增 9 个提交：Windows 渲染循环按 DWM 出帧、SDK 11 构建支持、生成器包定义静态状态调整、Native8 文档、Toolchain ILLink8 兼容、README、TextArea 高度修复、6.2.14 发版准备、Avalonia 12.1.6。
- 两边都改过的文件 9 个。真正冲突的只有 `CHANGELOG.md` 与 `CHANGELOG.zh-CN.md`，冲突两次，都是本分支的 `Unreleased` 条目与上游新增的 `6.2.14` 段落落在同一位置。
- 裁决：`Unreleased` 段保留在 `6.2.14` 段之上 — 本分支的条目尚未发布 — 判断错了只需挪动段落。
- 核对：变基后两份 CHANGELOG 的 `Unreleased` 段与变基前逐行相同，`6.2.14`、`6.2.13` 段与上游逐行相同；全仓没有残留的冲突标记。`TextAreaDecoratedBox.cs` 及其主题 axaml 由 Git 自动合并。
- 变基后分支领先 `release/6.0` 226 个提交，落后 0。

### 变基后的全量回归（执行基线）

- `scripts/run-full-regression.sh`：`[SUMMARY] TOTAL: 12317 个测试全部通过, 总耗时 15:19`（14 个项目，未含默认排除的本地化集成测试）。
- `AtomUI.Themes.Tests` 不在全量脚本的默认项目里，单独运行：27/27 通过。之后凡涉及主题包的任务都要单独跑它。

### 计划的执行前修订

- PrimeNG 改造用的编译快照不放进 `docs/superpowers`（该目录只放 Markdown），改放仓库外的临时目录，不提交。
- 计划里的文件行号取自变基之前，只作定位提示。

## 任务记录


### Task 1：`ThemeAlgorithm` 改为 `(Catalog, Id)` 结构体（审阅通过）

- 审阅结论：规格符合，质量通过，无严重或重要问题。留待最终整体审阅统一处理的小项：
  1. `ThemeAlgorithmTypeConverter` 对 `PrimeNG:1Bad` 这类输入抛 `ArgumentException`，其余非法输入抛 `NotSupportedException`，异常类型不一致；对应测试只断言了 `Exception`。
  2. `ThemeBuildIdentifier` 与 `SchemaIdentifier` 是两份相同的规则，只靠注释保持同步，缺一个对照测试。
  3. 中间文件「格式过期」的报错路径没有测试。
  4. 未初始化身份的检查只测了全局链，控件自定义链的路径没测。
- 裁决：性能基线除了在 Task 7 的提交上量，还要在执行基线 `0bf534726`（Task 1 之前）上用同一套测量再量一次，阶段一、二的改动也要过 4.11 的门槛 — 计划原本只在 Task 7 量基线，阶段一、二对性能的影响会被算进基线而看不出来 — 判断错了的代价是多一次测量。
- 裁决：生成的主题程序里内置算法现在写成 `new ThemeAlgorithm("AtomUI", "Dark")`，每次绑定多两次短字符串校验，不产生堆分配。先不改；如果上一条的对比测量显示它有影响，改为输出 `ThemeAlgorithm.Dark` 这类静态成员 — 没有测量依据就加特例是多余的复杂度 — 判断错了由 Task 7 的测量暴露。

- 改动：`ThemeAlgorithm` 由枚举改为只读结构体（仿 `ControlTokenIdentity`，`SchemaIdentifier` 校验、序数相等、`ToString()` 为 `Catalog:Id`，内部 `IsUninitialized` 与 `Compare`）；`ThemeAlgorithmAttribute` 改为 `(catalog, id, revision, appearanceEffect)`；三个内置计算器的 `const Algorithm` 删除；描述符与规范化器的 `Enum.IsDefined` 改为未初始化检查（规范化器诊断 ATMTHM4004「Algorithm identity is uninitialized.」）；注册表按 `Catalog`、`Id` 序数排序，修订号与配置指纹改计 `Catalog`、`Id`、`Revision`、`AppearanceEffect`（沿用现有 FNV 构建器逐字符计入）；类型转换器接受 `Default/Dark/Compact` 与 `Catalog:Id`，删掉 `DefaultAlgorithm` 旧拼写。
- 生成器：`ThemeAlgorithmInfo` 读两个字符串参数，标识符不合法或 `revision <= 0` 静默忽略；排序与 `ComputeGlobalSchema` 改按 `Catalog`、`Id`（全局段格式号 1→2）；输出 `new global::AtomUI.Theme.Algorithms.ThemeAlgorithm("AtomUI", "Dark")`，不再有整数强转。生成的内置算法顺序由 Default/Dark/Compact 变为 Compact/Dark/Default（与运行时排序一致，注册表仍走免排序路径）。
- Toolchain：链接 `SchemaIdentifier.cs`；中间文件算法项改为 `ThemeAlgorithmInput { Catalog, Id, Line, Column }`，格式号 4→5，读到旧版本抛「Compiled theme input format is out of date; rebuild the project.」；新增 `Common/ThemeDefinitions/ThemeBuildIdentifier.cs`（生成器无法引用 Core，镜像标识符规则，供解码校验与 `ThemeAlgorithmInfo` 使用）。`CompiledThemePackageWriter` 摘要字段改为 `Catalog:Id`。
- Gallery：`GalleryWorkspaceViewModel` 常量模式改 `!=`；`ThemePresetCatalog` 导出 XML 写 `algorithm.Id`（缓存键本就经 `ToString()`）。
- 测试：新增 `ThemeAlgorithmIdentityTests`（简报 5 个 + 类型转换器 2 个）；注册表、配置指纹各加一个钉死常量的确定性测试（`0x29D0F9BA9FF24C7F`、`0xE1D2F0E3E24383E2`，两次独立进程取值一致）。
- 结果：Core.Tests 457/457，Generator.Tests 1068/1068，Themes.Tests 27/27；全量回归 `[SUMMARY] TOTAL: 12334 个测试全部通过, 总耗时 15:35`（基线 12317，+17 为新增用例）。
- 偏差：
  - 注册表确定性测试不带 Token 与控件——它们的程序集限定类型名会让钉死的常量随框架/测试程序集版本漂移。
  - 删除 `Algorithm_Descriptor_Rejects_An_Undefined_Algorithm_Value`（被 `Default_Struct_Is_Rejected_By_Descriptor_And_Normalizer` 覆盖）；`Normalize_Rejects_An_Undefined_Algorithm_Value` 改名为 `Normalize_Rejects_An_Unregistered_Algorithm`，用 `new ThemeAlgorithm("Test", "Unregistered")`。
  - `ThemeDocumentReader` 无需改动即可编译。
  - `tools/performances/AtomUI.Performance` 有既存编译错误（`SpaceItemsAlignment` 不存在），与本任务无关，未处理。

### Task 2：主题 XML 与 XSD 接受任意算法身份

- 改动：XSD 的 `AlgorithmType` 增加可选 `Catalog`（`IdentifierType`），`Id` 改为新类型 `AlgorithmIdType`（`ThemeAlgorithmType` 与 `IdentifierType` 的 `xs:union`，保留三个内置值的 IDE 补全）；删除 `UniqueControlAlgorithmId`、`UniqueGlobalAlgorithmId`。`ThemeDocumentReader.ReadAlgorithm` 不再按三个 Id 分支：`Catalog` 省略时取 `AtomUI`，用新增的 `SchemaIdentifier.IsValid` 预检后构造 `ThemeAlgorithm`；全局链与每个控件的自定义链各用一个 `HashSet<ThemeAlgorithm>` 判重（控件的集合按需创建）。两条诊断均为 ATMTHM1002：「Theme algorithm identity '{catalog}:{id}' is not a valid identifier.」「Theme algorithm '{catalog}:{id}' is duplicated.」。写了 `Catalog` 的元素路径为 `Algorithm[@Catalog='X'][@Id='Y']`，省略时路径不变。Binder 无需改代码（ATMTHM2001 已经经 `ToString()` 输出 `Catalog:Id`）。`ThemePresetCatalog` 导出时只在 `Catalog` 不是 `AtomUI` 时写 `Catalog` 属性。
- 测试：Reader 新增简报列出的 6 个（重复、非法标识符两个为 Theory，各覆盖全局链/控件链、`Id`/`Catalog` 非法），`Xsd_Still_Offers_The_Three_BuiltIn_Ids` 取代旧的 XSD 枚举测试并额外校验 union 成员；Binder 新增 1 个；Gallery 新增 `Export_Writes_The_Algorithm_Catalog_Only_Outside_AtomUI`（导出结果按 XSD 校验）。
- 结果：Core.Tests 464/464，Gallery.Tests 422/422，Themes.Tests 27/27，Build.Tasks.Tests 107/107；全量回归 `[SUMMARY] TOTAL: 12342 个测试全部通过, 总耗时 16:25`（上一任务 12334，+8）。
- 偏差：
  - 旧 Theory `Read_Rejects_Non_Canonical_Algorithm_Values_With_Source_Position` 的 `dark`、`Unknown` 现在是合法标识符，阅读器接受、交给 Binder 报 ATMTHM2001；Theory 改名为 `Read_Rejects_Non_Identifier_Algorithm_Values_With_Source_Position`，数据改为 `1`、空串、`a b`。
  - 非法标识符正常情况下先由 XSD 报 ATMTHM1002，阅读器沿用原有「同一行已有 ATMTHM1002 就不再追加」的去重，所以「is not a valid identifier」文本只在 XSD 未拦截时出现（防御性兜底）；测试断言的是代码、行列与不抛异常，没有断言该文本。
- 审阅结论：规格符合，质量通过，无严重或重要问题。Task 1 的提交为 `506fd5e73`。留待最终整体审阅统一处理的小项：
  1. 重复算法的测试只断言了行列大于 0，两个条目写在同一行，没有钉住「报在第二个条目上」。
  2. 判重集合全局链是立即创建、控件链是按需创建，写法不一致（无功能影响）。
  3. Toolchain 没有「主题 XML 带外部 `Catalog`」的端到端测试；这条路径由 Task 17–20 的 PrimeNG 主题包真实构建覆盖。

### Task 3：包里的算法进入注册表

- 改动：`IThemeManagerBuilder.AddThemeAlgorithms`（返回 builder）；`ThemeManagerBuilder` 原样保存描述符实例（只做 `ThrowIfUnavailable`，追加包的 builder 也能登记），交给 `ThemeManager.SetPackageAlgorithms` 与 `CaptureStartupInputs`（`ThemeStartupInputs.Algorithms`）。`ThemeSchemaRegistry.CombineAlgorithms` 无包算法时直接返回 Core 的生成列表（默认路径零额外分配）；`ThemeManager` 四处启动注册表、`PreparePackages` 两处（取已提交注册表的算法 + 新 builder 的算法，保持引用）、`CreateGlobalOnly(packageAlgorithms)` 全部改用合并列表。`ExtendsGlobalSchemaOf` 取代 `HasSameGlobalSchema`（全局 Token 按槽位同引用，previous 的每个算法描述符按引用存在；等长时先走顺序比较快路径）。`LoadPackages` 只登记算法时不再提前返回。
- Revision：普通路径本就计入算法；启动索引（编译期 stamp 或组合指纹）与组合证据构造函数看不到包算法，新增 `FoldPackageAlgorithms` 在运行时把非 Core 描述符（Catalog、Id、Revision、AppearanceEffect，确定性构建器）折入；只有 Core 算法时 Revision 不变。
- 生成器：新增 `ThemeAlgorithmRegistrationWriter`，程序集声明了算法且 `IThemeManagerBuilder` 来自被引用程序集（即不是 Core）时输出 `GeneratedThemeAlgorithmRegistration` 与每个 Catalog 一个 `<Catalog>Algorithms`（命名空间取 `RootNamespace`，缺省取程序集名；成员名去掉 `.`、`-`，关键字加 `@`）；撞名报 ATOMUITOKEN002。`build/AtomUI.ThemeAssets.targets` 新增 `CompilerVisibleProperty RootNamespace`（原先不可见）。
- 测试：新增 `ThemeAlgorithmRegistrationTests` 6 个；注册表测试新增 4 个（含控制方追加的 `Registry_Revision_Changes_When_A_Package_Algorithm_Is_Added`，覆盖普通、组合、索引无 stamp、索引有 stamp 四条路径）；生成器新增 3 个；`ThemeInitialModeTests` 的测试替身补接口成员。
- 结果：Core.Tests 474/474，Generator.Tests 1071/1071，Themes.Tests 27/27；全量回归 `[SUMMARY] TOTAL: 12355 个测试全部通过, 总耗时 15:50`（上一任务 12342，+13）。
- 偏差：删除无参 `CreateGlobalOnly()` 与 `HasSameGlobalSchema`（唯一调用方已改）。现有 `src` 里的包都还没声明算法，所以没有包调用 `GeneratedThemeAlgorithmRegistration.Register`，留给后续加算法的任务接线。运行时追加的算法不会让已加载的用户目录 XML 主题重新解析（要等目录重载），不在本任务范围。
- 审阅结论：规格符合，质量通过，无严重或重要问题。Task 2 的提交为 `3c835e1dd`。审阅确认的两点：
  - `ExtendsGlobalSchemaOf` 是可靠的：全局 Token 按引用逐槽比较，旧注册表的每个算法描述符都必须按引用出现在新注册表里，同身份但被替换的描述符通不过。
  - 运行时追加的包带来新算法后，此前因缺该算法而加载失败的用户目录主题要等主题目录重新加载才恢复。这与「引用后来才追加的控件」的现有行为相同（`CompiledThemeCatalog.AppendRegistry` 只重绑编译主题包，不重跑解析器），符合规格第 9 节的一致性要求，不是缺口。
- 留待最终整体审阅统一处理的小项：
  1. 同一程序集里两个 Catalog 去掉分隔符后撞名（例如 `Prime-NG` 与 `PrimeNG`）会产生同名的生成文件，生成器整体报 CS8785，而不是给出 `ATOMUITOKEN002`。
  2. 算法成员名撞名的诊断没有源码位置。
  3. 「只追加算法」的 `LoadPackages` 路径只在编译器和注册表层面有测试，新的提前返回条件没有端到端测试。
  4. 启动复用只测了前提（描述符同一实例、注册表可扩展），没有实际跑 `ThemeStartupPreparation.Prepare` 后断言全局快照被复用。这关系到启动性能，Task 7 与 Task 10 的启动耗时对比是它的实测依据。

### Task 4：构建契约携带算法，XML 引用未知算法时构建期报错

- 改动：Core 新增 `ThemeBuildAlgorithmAttribute(version, catalog, id, revision, appearanceEffect)`（程序集级、`EditorBrowsable(Never)`）。`ThemeBuildContracts` 新增 `ThemeBuildAlgorithm` 记录与 `Algorithms`（键 `Catalog\u001fId`）/`LocalAlgorithms`；`Build` 多一个 `IEnumerable<ThemeAlgorithmInfo>` 参数，按 Owner/Token 的写法从直接引用读取（版本 1、参数 5 个、标识符合法、`revision > 0`、效果 0–2，异常转「Invalid Token build metadata in '<asm>': …」），再加本地算法（按 `ThemeAlgorithmInfo.Order` 排序）；重复报「Duplicate algorithm build contract: <Catalog>:<Id>」。`Write` 在 `GeneratedThemeBuildContracts.g.cs` 末尾输出本地算法。`HasAbi` 增加要求 `ThemeBuildAlgorithmAttribute` 存在（否则写出的特性无法编译）。`CompiledThemePackageWriter` 校验主题链与每个已知控件的自定义链，未知算法在该元素行列报 ATOMUITOKEN002「Unknown algorithm '<Catalog>:<Id>'.」，该主题不生成程序。
- Core 自身编译时特性来自源码：本地算法直接由源码收集，写出的特性绑定到源码声明；实测 Core 的生成文件含 `AtomUI:Compact/Dark/Default` 三条（效果 0/2/1）。
- 测试：新增 `ThemeDefinitions/CompiledThemeAlgorithmContractTests` 5 个（简报所列）。桩：沿用 `ThemeSchemaGeneratorTests.AtomUIStubs`，把 `TokenDescriptor` 改成 partial 补 7 参数 `CreateGenerated`，加四个契约特性；「Core」测试编译在源码里声明特性与三个内置算法并跑生成器，其输出作为包编译的引用。现有生成器测试的桩都不满足 `HasAbi`，不受新增门槛影响，无需改动。RED：编译失败（`Build` 无 6 参重载、无 `Algorithms`）；另做变异验证——注释掉校验与写出后 3 个失败、恢复后 5/5 通过。
- 结果：Generator.Tests 1076/1076，Themes.Tests 27/27，Core.Tests 474/474；全量回归 `[SUMMARY] TOTAL: 12360 个测试全部通过, 总耗时 15:40`（上一任务 12355，+5）。
- 偏差：
  - 本地重复（同一程序集两个类型同身份，或本地重声明了引用里已有的算法）报 ATOMUITOKEN002（无位置）并跳过该条，不抛出——现有 Owner 的本地 `Add` 不在 try 里，照抄会让生成器崩溃。
  - 未知控件仍先报「Unknown, repeated or invalid Control」并跳过，不再校验其算法链。
  - 算法的 Revision/AppearanceEffect 没有计入编译主题程序的摘要字段（简报未要求；摘要仍只计 `Catalog:Id`）。
- 审阅结论：规格符合，质量通过，无严重或重要问题。Task 3 的提交为 `9d59e3551`。审阅确认：
  - 编译主题程序的摘要只记 `Catalog:Id`、不记算法修订号，不构成过期风险：摘要唯一的运行时用途是 `ThemeContentFingerprint`，同一个指纹还计入注册表修订号，而注册表修订号和 `AlgorithmCacheKey` 都计入了算法的 `Revision` 与 `AppearanceEffect`。
  - 四个预设包与 Gallery 的真实构建由 `AtomUI.Themes.Tests`（27/27）和全量回归覆盖，Core 生成的契约含三个内置算法。
- 留待最终整体审阅统一处理的小项：
  1. 同一程序集内重复声明算法的报错没有源码位置；「两个本地类型同身份」没有测试。
  2. 控件本身未知时，它的自定义算法链不再检查，链里的未知算法要等控件修好后的下一次构建才报。
  3. `ThemeBuildContracts` 把无法识别的 `AppearanceEffect` 文本静默当作 Preserve，改成显式匹配加抛错更严格。

### 阶段一小结（Task 1–4）

Task 1–4 完成：算法身份开放、XML 接受任意身份、包里的算法进入注册表、构建期校验未知算法。全量回归从基线 12317 增至 12360，全部通过。

### Task 5：显式 Alias 值带动派生 Alias

- 改动：`DesignToken` 新增按全局槽位记录的显式位图（懒分配的 `ulong[]? _explicitSlots`，`MarkExplicit`/`IsExplicitSlot`/`ResetExplicit`/私有 `IsExplicit(SharedTokenKind)`，做法照 `AbstractControlDesignToken`）。`CalculateAliasTokenValues` 给 8 个「先赋值、后被读取」的字段加守卫：`ColorErrorOutline`、`ColorWarningOutline`、`LineWidthFocus`、`ControlOutlineWidth`、`ColorControlOutline`、`ControlPaddingHorizontal`、`ControlPaddingHorizontalSM`、`UniformlyPaddingSM`。逐行核对方法体，没有第 9 个。`ThemeCompiler` 新增 `CalculateAlias`：写入并标记 Alias 显式值 → `CalculateAliasTokenValues()` → 再写一次 → `ResetExplicit()`；`CompileGlobalBuilder` 与 `ReevaluateControlGlobal` 都改用它。`CopyBuilder` 新建 `DesignToken`，不复制位图。
- 测试：新增 `GlobalAliasExplicitOverrideTests`，含简报列出的 10 项（`Without_Explicit_Alias_Values_The_Result_Is_Unchanged` 写成 4 行 Theory），另加 1 个位图单元测试，共 14 个用例。RED：前 6 项、Zero、控件作用域 Global 共 8 个失败，派生值仍按默认公式计算；显式派生值优先、4 条无显式值链按预期通过。GREEN：14/14。新增生效测试 `InputFamilyEffectTests.ControlOutlineWidth_Drives_The_Focused_Input_Outline`，覆盖根作用域与控件 Global 作用域、宽度 0 与 6，共 4 行，比较 LineEdit 聚焦外框周围的一圈像素；宽度 0 时与未聚焦帧相同。临时去掉标记步骤时这 4 行全部失败，恢复后全部通过。
- 不变性快照：改动前用临时测试（已删除）编译 Default、Dark、Default+Compact、Dark+Compact 四条链，把全部 255 个全局槽位按「名称=`descriptor.Format(值)`」逐行拼接后取 SHA-256，固定在测试里；改动后 4 个哈希都不变。
- 预设主题变化：用临时测试（已删除）应用全部可用主题，导出全局槽位和每个控件的有效全局值。在临时去掉标记步骤与正常两种状态下各导出一次，差异只有下表 48 项全局值，没有控件作用域独有的差异（控件有效值只是跟随全局）。Material、Shadcn 与内置 DaybreakBlue 不变。格式为 `OffsetX OffsetY [Blur] Spread Color`，「0 0 #…」表示 Spread 为 0。

  | 主题 | Token | 旧值 | 新值 |
  | --- | --- | --- | --- |
  | Bootstrap / Bootstrap.Compact | InputActiveShadow | 0 0 0 2 #190591ff | 0 0 0 4 #400d6efd |
  | Bootstrap / Bootstrap.Compact | InputErrorActiveShadow | 0 0 0 2 #0fff1605 | 0 0 0 4 #0fff1605 |
  | Bootstrap / Bootstrap.Compact | InputWarningActiveShadow | 0 0 0 2 #19ffeb05 | 0 0 0 4 #19ffeb05 |
  | Bootstrap.Dark / Bootstrap.DarkCompact | InputActiveShadow | 0 0 0 2 #72003b96 | 0 0 0 4 #400d6efd |
  | Bootstrap.Dark / Bootstrap.DarkCompact | InputErrorActiveShadow | 0 0 0 2 #752c0002 | 0 0 0 4 #752c0002 |
  | Bootstrap.Dark / Bootstrap.DarkCompact | InputWarningActiveShadow | 0 0 0 2 #93342200 | 0 0 0 4 #93342200 |
  | PrimeNGAura / PrimeNGAura.Compact | InputActiveShadow | 0 0 0 2 #1411e682 | 0 0 #1411e682 |
  | PrimeNGAura / PrimeNGAura.Compact | InputErrorActiveShadow | 0 0 0 2 #0fff2605 | 0 0 #0fff2605 |
  | PrimeNGAura / PrimeNGAura.Compact | InputWarningActiveShadow | 0 0 0 2 #19ffd705 | 0 0 #19ffd705 |
  | PrimeNGAura.Dark / PrimeNGAura.DarkCompact | InputActiveShadow | 0 0 0 2 #2934d399 | 0 0 #2934d399 |
  | PrimeNGAura.Dark / PrimeNGAura.DarkCompact | InputErrorActiveShadow | 0 0 0 2 #23fcada1 | 0 0 #23fcada1 |
  | PrimeNGAura.Dark / PrimeNGAura.DarkCompact | InputWarningActiveShadow | 0 0 0 2 #23fcb436 | 0 0 #23fcb436 |
  | PrimeNGLara / PrimeNGLara.Compact | InputActiveShadow | 0 0 0 2 #1411e682 | 0 0 0 3 #ffa7f3d0 |
  | PrimeNGLara / PrimeNGLara.Compact | InputErrorActiveShadow | 0 0 0 2 #0fff2605 | 0 0 0 3 #0fff2605 |
  | PrimeNGLara / PrimeNGLara.Compact | InputWarningActiveShadow | 0 0 0 2 #19ffd705 | 0 0 0 3 #19ffd705 |
  | PrimeNGLara.Dark / PrimeNGLara.DarkCompact | InputActiveShadow | 0 0 0 2 #2934d399 | 0 0 0 3 #3334d399 |
  | PrimeNGLara.Dark / PrimeNGLara.DarkCompact | InputErrorActiveShadow | 0 0 0 2 #23fcada1 | 0 0 0 3 #23fcada1 |
  | PrimeNGLara.Dark / PrimeNGLara.DarkCompact | InputWarningActiveShadow | 0 0 0 2 #23fcb436 | 0 0 0 3 #23fcb436 |
  | PrimeNGNora / PrimeNGNora.Compact | InputActiveShadow | 0 0 0 2 #1411e682 | 0 0 #1411e682 |
  | PrimeNGNora / PrimeNGNora.Compact | InputErrorActiveShadow | 0 0 0 2 #0fff2605 | 0 0 #0fff2605 |
  | PrimeNGNora / PrimeNGNora.Compact | InputWarningActiveShadow | 0 0 0 2 #19ffd705 | 0 0 #19ffd705 |
  | PrimeNGNora.Dark / PrimeNGNora.DarkCompact | InputActiveShadow | 0 0 0 2 #b713fdaf | 0 0 #b713fdaf |
  | PrimeNGNora.Dark / PrimeNGNora.DarkCompact | InputErrorActiveShadow | 0 0 0 2 #23fc827d | 0 0 #23fc827d |
  | PrimeNGNora.Dark / PrimeNGNora.DarkCompact | InputWarningActiveShadow | 0 0 0 2 #23fcb436 | 0 0 #23fcb436 |

  原有断言都没有检查派生值，所以没有断言失败。新增 3 条断言：Aura 的 `InputActiveShadow` Spread 为 0；Lara 的为 3，颜色 #A7F3D0；Bootstrap 的为 4，颜色 #400D6EFD。
- 结果：Core.Tests 488/488，Themes.Tests 27/27，Rendering.Tests 4662/4662；全量回归 `[SUMMARY] TOTAL: 12378 个测试全部通过, 总耗时 15:45`（上一任务 12360，+18 = Core 14 + 生效测试 4）。
- 偏差与说明：
  1. Alias 段抽成私有方法 `CalculateAlias`，顺序与简报一致。
  2. 每个 `DesignToken` 多一个 8 字节引用字段，在「Token 实例数 × 8 字节」的门槛内。没有显式 Alias 值时不分配位图、不多跑默认公式，只多遍历一次配置值列表。
  3. 按测试生命周期规范，`Without_Explicit_Alias_Values_The_Result_Is_Unchanged` 属于快照对比。它按简报保留为常驻测试，但以后任何合法的公式调整都会让它失败，建议最终审阅时决定是否删除。
  4. 本任务之前就存在一个问题，未改：控件作用域使用 `Global`/`Custom` 时，`ReevaluateControlGlobal` 以根快照为基线重跑算法与 Alias 计算，只重写控件自己的覆盖值。因此根主题显式写入的 Alias 值（以及 Map 值）在该控件作用域内会被默认公式覆盖。
- 审阅结论：规格符合，质量通过，无严重或重要问题。Task 4 的提交为 `ba3a6c32f`。审阅确认：
  - 守卫清单完整：通读 `CalculateAliasTokenValues` 全文，本方法内「先赋值、后面又被读取」的 Alias 字段只有已加守卫的 8 个，没有第 9 个。
  - 显式标记不会泄漏：计算所用的 Token 对象总是 `CopyBuilder` 新建的副本，方法抛异常时该对象随即不可达。
  - 没有显式 Alias 值时不分配位图，默认计算仍只跑一次。
- CHANGELOG：规格 4.5 要求在 CHANGELOG 里说明这次修复。按计划 CHANGELOG 由 Task 23 统一写，届时要列出上表受影响的派生关系（`ControlOutlineWidth`、`ColorControlOutline`、`ColorErrorOutline`、`ColorWarningOutline` → 三个 `Input*ActiveShadow`；`LineWidthFocus` → `FocusVisualBorderThickness`；`ControlPaddingHorizontal`、`ControlPaddingHorizontalSM`、`UniformlyPaddingSM` → `InputPadding*`）。
- 留待最终整体审阅统一处理的小项：
  1. `Without_Explicit_Alias_Values_The_Result_Is_Unchanged` 钉的是 255 个槽位的 SHA-256，失败时看不出是哪个槽位变了，以后新增全局 Token 也会让它无故失败。最终审阅时决定：改成钉可逐项对比的文本，或作为本计划的一次性验证删除。
  2. `ThemeCompiler` 里新增的 `MarkExplicit(DesignToken, …)` 与 `ApplyValues`、控件版 `MarkExplicit` 三段循环近乎重复；显式 Alias 值被深拷贝两次。
- 记录在案的既有问题（不在本计划范围内，未处理）：控件使用 `Global` 或 `Custom` 算法模式时，`ReevaluateControlGlobal` 从根快照重跑算法与 Alias 计算，只重新写入该控件自己的全局覆盖。主题在根上显式设置的 Map 与 Alias 值在这个控件的作用域里会退回默认公式的结果。本任务前后行为相同，但现在更容易被看见（例如把 Bootstrap 下某个控件设为 `Global` 模式，它作用域内的 `ControlOutlineWidth` 会回到 2）。需要用户决定是否单独立项。

### Task 6：ATOMUIGEN040 与 048 覆盖全局 Alias 计算方法

- 改动：新增 `TokenCalculationScope`（生成器内部类），两个分析器共用。范围增加 `[GlobalDesignToken]` 类型的实例方法 `CalculateAliasTokenValues`：只检查带 `[DesignTokenKind(Alias)]` 的属性；守卫须解析到 Token 类型（或基类）声明的 `IsExplicit`，实参为 `AtomUI.Theme.Resources.SharedTokenKind` 中与属性同名的成员。控件范围的判定逻辑不变。两条诊断的消息改为带方法名和 Kind 枚举名（`IsExplicit(SharedTokenKind.X)` / `IsExplicit(ButtonTokenKind.X)`），标题和消息去掉了「Own」。`compiler-diagnostics-guidelines.md` 的 040、048 两行写明新范围。
- 测试：048 新增 7 条（简报 4 条 + 守卫成员名不符要报、无 `[GlobalDesignToken]` 不报、其它方法不报），040 新增 2 条（简报 1 条 + 方法外不报）。先失败（3 条报告类测试拿到空诊断）后通过。
- 核对 Task 5：`dotnet build src/AtomUI.Core -warnaserror:ATOMUIGEN048` 0 诊断。临时去掉 `ControlOutlineWidth` 的守卫后，构建在 `DesignToken.cs(188,9)` 报 ATOMUIGEN048，已恢复。Task 5 的 8 个守卫完整，无需补。
- 结果：Generator.Tests 1085/1085，Core.Tests 488/488；`AtomUI.Desktop.Controls` 构建无 ATOMUIGEN040/048；全量回归 `[SUMMARY] TOTAL: 12387 个测试全部通过, 总耗时 15:40`（上一任务 12378，+9）。
- 偏差与说明：
  1. 040 在全局范围也只针对 Alias 属性（与 048 一致）；对 Map/Seed 属性调 `IsNaN` 不报。
  2. 040/048 的消息参数由 `{1}=控件名` 改为 `{1}=Kind 枚举名, {2}=方法名`，诊断 id 和严重级别不变。
- 审阅结论：规格符合，质量通过，无严重或重要问题。Task 5 的提交为 `a3eb12765`。审阅确认：
  - 控件 Token 这条原有路径没有变：方法匹配、属性范围、守卫方法、枚举命名规则、读取顺序判断逐项对比与改动前等价。
  - 全局范围里，写错成员名（`SharedTokenKind.C` 包住对 `A` 的赋值）或用错枚举的守卫都不被接受。
  - 诊断文本的措辞变了（指明枚举和方法名），ID 与级别不变；仓库里没有其他地方匹配旧文本。
- 留待最终整体审阅统一处理的小项：
  1. 全局范围里 040 只检查 Alias 属性，这个取舍没有反例测试钉住（`IsNaN` 用在 Map 属性上应被忽略）。
  2. 「同名成员但来自别的枚举」在全局范围被拒绝，代码正确但没有测试。
  3. `TokenCalculationScope` 里 `"IsExplicit"` 是内联字面量，其余名称都是常量。
  4. `AnalyzeMethod` 的方法名预筛与 `scope.Match` 重复，可收成一个辅助方法。

### 阶段二小结（Task 5–6）

显式设置的全局 Alias 值现在带动派生值，并由分析器防止以后漏加守卫。内置 Default、Dark、Compact 三条链的编译结果不变；预设主题有 48 个全局值按预期变化（见 Task 5 的表）。全量回归增至 12387，全部通过。

### Task 7：性能基线（PRE = `0bf534726`，NOW = `7f58c7a05` + 本任务改动）

- 改动：
  1. 新增显式运行的测量 `tests/AtomUI.Desktop.Controls.Tests/Theme/Performance/ThemeCompileMeasurement.cs`。它用 `[Fact(Explicit = true)]`，不含断言，每项输出一行 `MEASURE key=value`。本项目的测试都用 `[Fact]` 加程序集级 `AvaloniaTestFramework`，xunit v3 的 `Explicit` 可以直接用。同一源文件原样复制进 PRE 树编译运行，只用两棵树都有的 API（`ThemeAlgorithm.Default/Dark/Compact`）。
  2. 生产代码修正 ①（控制方裁定）：`ThemeCompiler.MarkExplicit(DesignToken, IReadOnlyList<NormalizedTokenValue>, TokenStage)` 是 Task 5 新增的方法，其中的 `foreach` 改为按 `values.Count` 的下标循环，每次编译少分配一个 32 字节的装箱枚举器。只改了这一个方法，`ApplyValues` 与控件版 `MarkExplicit` 不动。
  3. 生产代码修正 ②（控制方裁定）：`AlgorithmCacheKey`（`src/AtomUI.Core/Theme/Compilation/ThemeCacheKey.cs`）去掉 `ThemeAlgorithm Algorithm` 分量，只保留 `Revision`、`AppearanceEffect`、`CompilationIdentity`，`Create` 相应调整。同一文件里因此不再用到的 `using AtomUI.Theme.Algorithms;` 一并删除，缓存键代码的其他部分不变。
  4. `TypedThemeSnapshotCacheTests` 新增 `Algorithm_Cache_Key_Is_Identified_By_The_Descriptor_Instance`，钉住键的语义：
     - 同一个描述符实例创建的两个键相等，哈希也相等；
     - 两个不同的描述符实例即使身份、修订号、外观效果都相同，创建的键也不相等。

     改键之前运行，通过（它记录的是原有语义）；改键之后仍然通过。

- 口径：
  - **冷编译。** 计时部分是 `new ThemeCompiler().Compile(...)` 加物化全部 116 个惰性控件快照。编译时没有父快照、没有变更集、没有快照缓存、也没有控件编译缓存。每个样本前 `GC.Collect` 两次，分配取同一区间的 `GC.GetAllocatedBytesForCurrentThread()` 差值。核对项：Default 链编译结果的内容指纹与管理器根快照一致。
  - **每个样本的有效配置都是新建的（控制方裁定 R1）。** 有效配置按 `ThemeStartupCompilation.CompileInitial` 的分层准备，依次是：定义默认值、应用层（测试应用只有默认字体）、运行时请求。准备时间单独记为 `prepare`，不计入冷编译。
    - 定义默认值不走带缓存的 `ThemeCompiler.CreateDefinitionDefaults`，而是用反射调用私有的 `CreateDefinitionDefaultsCore`，它生成内容相同的新对象。
    - 因此每个样本编译的 `NormalizedThemeConfig` 对象、它的 `NormalizedThemeConfigCacheKey`、以及延迟解析出的控件配置对象，都没有被前面的样本用过。
    - 测量会核对并输出这一点。三条门槛链在全部 14 次运行里都是 `configObjectReusedAcrossSamples=false`、`configKeyReusedAcrossSamples=false`，而 `configContentEqualsMemoised=true`（内容与缓存路径相同）。
  - **改动前的口径有缺口，现单列为 `defaultMemoised`，不设门槛。** 改动前，Default 链的运行时层为空，合并时直接返回按 (定义, 注册表) 缓存的同一个定义默认值对象，配置缓存键也只在第一个样本里建一次。Task 1 增加的开销恰好在这个键上，所以旧口径会漏掉它。Dark、Compact 两条链原先就每次新建有效配置，不受影响。
  - **仍保持热状态的进程级缓存。** 下面这些在真实应用里同样只在第一次编译时建一次，之后一直复用：
    - `ThemeCompiler.GlobalSchemas`：按注册表的全局描述符列表缓存。
    - `ThemeDefinitionCacheKey`：按 `PreparedThemeDefinition` 缓存。定义对象在主题目录里存活整个应用期。
    - 启动包索引里已物化的控件描述符（`StartupPackageIndex` 条目的 `GetDescriptor()`）。
    - 生成的静态 Schema 列表。
    - JIT 代码与类型加载。

    保留字节估算按新建的资源字典与调色板字典缓存，每个样本都是冷的。
  - **Token 实例数按编译器的构造规则计算**，不是插桩计数；编译器改变构造方式时，要同步修改测量里的公式。规则如下：
    - 全局：1 个种子，加每个算法 1 个副本；
    - 每个惰性控件：1 个有效全局副本，Global/Custom 模式下再加每个算法 1 个；有自有 Token 时再加 1 个控件 Token。

    三条链都是 `DesignToken` 118 个、控件 Token 116 个。
  - **明暗切换。** 显示一个 `atom:Window`，内放 Button、LineEdit、CheckBox、RadioButton、ToggleSwitch、Slider、Select、Alert、Pagination、Card。经 `ApplyThemeAsync` 切暗再切亮，计时到 `Dispatcher.UIThread.RunJobs()` 返回，一个往返记一个样本。分两个口径：
    - `cached`：保留管理器的快照缓存，对应应用里反复切换；
    - `uncached`：每次切换前丢弃快照缓存，使每次切换都要编译，对应第一次切到某个主题。

    **测量通过反射使用两个私有成员：`ThemeManager._themeSnapshotCache`（`uncached` 口径）与 `ThemeCompiler.CreateDefinitionDefaultsCore`（冷编译）。任何一个改名或改签名，测量都会抛异常，需要同步修改。**
  - **值哈希。** DaybreakBlue 在 Default、Dark、Compact 三条链下分别计算。全局槽位按名称、控件按 identity、自有 Token 按名称排序（均为 ordinal），逐行拼接 `G|名称=Format(值)` 与 `C|控件|名称=Format(值)` 后取 SHA-256，不依赖槽位编号。
  - **关闭分层编译（`DOTNET_TieredCompilation=0`）。** 默认分层下，样本会随 tier-0 → tier-1 升级在一次运行内漂移（切换从约 49 ms 降到约 15 ms），得不到稳定的中位数；关闭后，两棵树都在全部方法一次编译为全优化代码的条件下比较。代价是这个口径看不到以下几点：
    - 动态 PGO 的效果；
    - tier-0 代码与分层升级的开销；
    - ReadyToRun 框架代码在默认分层下会被重新编译成优化版本，在本口径下只用 R2R 原版本。

    默认分层这一维由启动采样覆盖：`run_samples.py` 用默认设置运行，分辨率约 20–30 ms。
  - **样本数。** 冷编译单个样本在约 0.67 ms 与 0.8–0.9 ms 两档之间成段切换，两棵树都如此。很可能是 M1 Max 上线程在性能核与能效核之间迁移，或者同一簇里有其他负载；本任务没有证实是哪一种。这种切换会推动中位数，但对 P25 影响小。15/3（简报默认，保留为环境变量的缺省值）、41/10、101/20、201/30 几组都不够，最终取冷编译 3001 次计时、300 次预热，切换 501 次计时、50 次预热。

- **以后每次比较都必须使用的流程（Tasks 9–23 沿用，控制方裁定 R2）。**
  - 命令：

    ```bash
    DOTNET_TieredCompilation=0 \
    ATOMUI_MEASURE_COMPILE_ITERATIONS=3001 ATOMUI_MEASURE_COMPILE_WARMUPS=300 \
    ATOMUI_MEASURE_ITERATIONS=501 ATOMUI_MEASURE_WARMUPS=50 \
    ATOMUI_MEASURE_LABEL=<tree>-<n> ATOMUI_MEASURE_OUTPUT=<file> \
    dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj -c Release --no-build \
      --filter "FullyQualifiedName~ThemeCompileMeasurement" -- xUnit.Explicit=only
    ```

    采样前两棵树都先执行 `dotnet build tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj -c Release`，再执行 `dotnet build-server shutdown`。采样期间不跑构建和其他测试。
  - 判定规则（控制方裁定 R2，原文）：
    - at least 7 interleaved runs per tree (PRE, NOW, PRE, NOW, …), same command, same sample counts, `DOTNET_TieredCompilation=0`;
    - the gated statistic per metric is chosen from {median, P25} as the one with the smaller run-to-run spread ON THE PRE TREE ALONE — pick it before looking at any PRE→NOW difference, write the choice and both spreads in the ledger, and never change it afterwards;
    - verdict "within noise" requires: the two trees' ranges of the per-run statistic overlap AND |difference of the per-tree medians of that statistic| < the larger of the two spreads; where the larger spread is below 2%, the difference must also be ≤ 2%;
    - the ledger states, per metric, the achieved resolution (the larger spread) in plain words. If a metric cannot resolve 2% on this machine, say exactly that; do not write that it can.
  - 有缓存的切换这一项在本机的分辨率约为 3.4%，2% 的门槛对它无法判定；后续对比按『两棵树的取值范围重叠且中位数之差小于较大的那个波动』判定，至少各 7 轮交替测量。（控制方第二次裁定原文。按本轮 R2 选出的统计量，它这次的分辨率为 2.06%，见下表。）
  - 统计量的选择已在下表固定，以后不再改变。脚本 `<S>/scripts/r2_verdict.py <out>` 按这条规则计算。

- 机器：MacBookPro18,2（Apple M1 Max，10 核，64 GB），macOS 26.6.2（25G83），.NET SDK 11.0.100（`global.json` 为 10.0.300，`rollForward: latestMajor`），测试目标框架 net10.0，Release。机器上有其他常驻负载，无法排除，1 分钟负载约 6–15。本轮第 1 轮 PRE 开始时负载为 25，不是本任务造成的，那一轮 PRE 的数值偏高，已包含在下面的统计里。
- 其他命令（`<S>` 表示临时目录 `scratchpad/perf`，原始数据都在那里，不提交）：
  - PRE 树：`git archive --format=tar -o <S>/pre.tar 0bf534726`，然后 `tar -xf <S>/pre.tar -C <S>/pre`，再把测量文件复制到同一相对路径。
  - 交错脚本：`<S>/scripts/run_measure.sh <out> 7 "pre now"`，它按上面的命令依次运行 PRE 与 NOW。本轮数据在 `<S>/runs-r1`。
  - 启动：两棵树分别执行 `dotnet publish tools/performances/AtomUI.StartupPerformance/AtomUI.StartupPerformance.csproj -c Release -f net10.0 --self-contained false -o <S>/startup-<tree>`。每个场景一份 `variants-<scenario>.json`（变体 pre/now，`compare: [pre, now]`），各跑 3 次 `python3 tools/performances/AtomUI.StartupPerformance/run_samples.py --config <S>/variants-<scenario>.json --output <S>/startup-baseline/<scenario>-r<n> --rounds 10 --warmup 1 --timeout 90`。
  - NativeAOT：`dotnet publish controlgallery/AtomUIGallery.Desktop/AtomUIGallery.Desktop.csproj -c Release -r osx-arm64 -p:GalleryPublishTrimmed=true -p:GalleryPublishAot=true -o <S>/gallery-aot-<tree>`。这是仓库文档里的 Gallery AOT 发布命令，RID 换成了本机。

**时间指标：最终基线（两项修正之后；冷编译为真正的冷路径；PRE、NOW 交错各 7 次）**

统计量的选择只看 PRE 自己的波动：

| 指标 | PRE 波动（每次运行中位数） | PRE 波动（每次运行 P25） | 门槛统计量 |
| --- | --- | --- | --- |
| 冷编译 Default | 6.66% | 2.81% | P25 |
| 冷编译 Dark | 8.58% | 3.80% | P25 |
| 冷编译 Compact | 6.03% | 3.29% | P25 |
| 切换往返 uncached | 2.83% | 3.23% | 中位数 |
| 切换往返 cached | 3.58% | 2.06% | P25 |

按所选统计量，每次运行的数值（ms）：

| 指标 | PRE 七次 | PRE 范围 / 中位数 / 波动 | NOW 七次 | NOW 范围 / 中位数 / 波动 | NOW − PRE |
| --- | --- | --- | --- | --- | --- |
| 冷编译 Default（P25） | 0.7036, 0.6915, 0.6879, 0.6945, 0.6865, 0.6842, 0.6914 | 0.6842–0.7036 / 0.6914 / 0.0194（2.81%） | 0.6961, 0.6912, 0.6882, 0.6876, 0.6882, 0.6957, 0.6905 | 0.6876–0.6961 / 0.6905 / 0.0085（1.23%） | −0.0009（−0.13%） |
| 冷编译 Dark（P25） | 0.7197, 0.7013, 0.6970, 0.7035, 0.6959, 0.6932, 0.6967 | 0.6932–0.7197 / 0.6970 / 0.0265（3.80%） | 0.7029, 0.6987, 0.6969, 0.7012, 0.7024, 0.6978, 0.7018 | 0.6969–0.7029 / 0.7012 / 0.0060（0.86%） | +0.0042（+0.60%） |
| 冷编译 Compact（P25） | 0.7069, 0.6942, 0.6921, 0.6945, 0.6882, 0.6841, 0.6923 | 0.6841–0.7069 / 0.6923 / 0.0228（3.29%） | 0.6949, 0.6926, 0.6921, 0.6957, 0.6922, 0.6951, 0.6956 | 0.6921–0.6957 / 0.6949 / 0.0036（0.52%） | +0.0026（+0.38%） |
| 切换往返 uncached（中位数） | 12.265, 12.160, 12.142, 12.063, 12.066, 11.921, 12.133 | 11.921–12.265 / 12.133 / 0.344（2.83%） | 12.253, 12.070, 12.042, 11.976, 12.035, 12.060, 12.175 | 11.976–12.253 / 12.060 / 0.278（2.30%） | −0.074（−0.61%） |
| 切换往返 cached（P25） | 10.486, 10.302, 10.349, 10.276, 10.285, 10.274, 10.374 | 10.274–10.486 / 10.302 / 0.212（2.06%） | 10.309, 10.405, 10.200, 10.234, 10.196, 10.280, 10.242 | 10.196–10.405 / 10.242 / 0.208（2.03%） | −0.060（−0.58%） |

判定与分辨率：

| 指标 | 范围重叠 | 差值绝对值 < 较大波动 | 判定 | 本机分辨率（较大波动） |
| --- | --- | --- | --- | --- |
| 冷编译 Default | 是 | 0.0009 < 0.0194 | 差异在噪声范围内 | 2.81%：本机分辨不出 2% |
| 冷编译 Dark | 是 | 0.0042 < 0.0265 | 差异在噪声范围内 | 3.80%：本机分辨不出 2% |
| 冷编译 Compact | 是 | 0.0026 < 0.0228 | 差异在噪声范围内 | 3.29%：本机分辨不出 2% |
| 切换往返 uncached | 是 | 0.074 < 0.344 | 差异在噪声范围内 | 2.83%：本机分辨不出 2% |
| 切换往返 cached | 是 | 0.060 < 0.212 | 差异在噪声范围内 | 2.06%：本机分辨不出 2%（与 2% 很接近） |

**本机上，这五项都分辨不出 2% 的差异。** 能分辨的大小约为 2.1%–3.8%。比这更小的退化可能被噪声掩盖，从而通过判定。

另一种统计量的数值（不作门槛，仅供参考）：

- 冷编译中位数：Default PRE 0.7368 / NOW 0.7332 ms（−0.49%，波动 6.66% / 2.89%）；Dark 0.7405 / 0.7456（+0.69%）；Compact 0.7382 / 0.7415（+0.45%）。
- uncached 切换 P25：11.730 / 11.520（−1.80%，波动 3.23% / 1.85%）。
- cached 切换中位数：10.766 / 10.621（−1.35%，波动 3.58% / 3.04%）。
- `defaultMemoised`（旧口径，不设门槛）：P25 0.6821 / 0.6792 ms（−0.43%）。

NOW 切换的单向中位数：cached 切暗 5.404 ms、切亮 5.234 ms；uncached 切暗 5.985 ms、切亮 6.023 ms。

**分配（真正的冷路径；七次运行完全相同）**

| 链 | PRE（字节） | NOW（字节） | 增量 | 预算 118 × 8 = 944 |
| --- | --- | --- | --- | --- |
| Default | 1,135,224 | 1,136,160 | +936 | 通过 |
| Dark | 1,140,416 | 1,141,352 | +936 | 通过 |
| Compact | 1,135,608 | 1,136,544 | +936 | 通过 |
| defaultMemoised（旧口径，不设门槛） | 1,118,968 | 1,119,912 | +944 | — |

新口径下 Default 的绝对值比旧口径多约 16 KB，因为现在每个样本都要新建配置缓存键与控件配置对象。两棵树都多出这一部分，增量不受影响。`DesignToken` / 控件 Token 实例数为 118 / 116，两棵树相同。

**两项修正的来源与效果**

- 确认方法：用临时诊断测试（已删除）在两棵树中分段测量分配，并设 `DOTNET_JitObjectStackAllocation=0`，排除 JIT 逃逸分析把枚举器放到栈上造成的差异。测得的事实：
  - `DesignToken` 对象本身（不含调色板字典）由 2488 字节变为 2496 字节，正好 +8；
  - 物化 116 个控件 +928 = 116 × 8；
  - 全局构建（`CompileGlobal`）+16 = 2 × 8；
  - `CompileControls` +0。
- 修正 ①：多出的 32 字节是 `MarkExplicit` 的装箱枚举器，由 Task 5 引入。
- 修正 ②：每个算法多出的 8 字节由 Task 1 引入，来源与改法如下。
  - `ThemeSnapshot` 构造时估算保留字节，会调用 `NormalizedThemeConfigCacheKey.Create(EffectiveConfig)`，这个键里的 `AlgorithmCacheKey` 带一个 `ThemeAlgorithm` 字段。
  - Task 1 把 `ThemeAlgorithm` 从 4 字节枚举改为含两个引用的结构体，键由 24 字节变为 32 字节。创建一个配置键的分配，PRE 为 720 字节，修正前的 NOW 为 728 字节。
  - 可以去掉这个字段的依据：
    - `ThemeAlgorithmDescriptor.CompilationIdentity` 是 sealed 类上的只读自动属性 `= new()`，每个实例各有一个，没有复制或替换的途径；描述符的 `Algorithm` 同样只读。
    - `AlgorithmCacheKey` 只用于三处：键数组的 `SequenceEqual`、`CacheKeyArray.Add` 哈希、`CacheKeyArray.Estimate`（只用数组长度）。没有地方读取 `.Algorithm`，也没有 `ToString()`、解构或诊断输出。
    - 所以 `Algorithm` 分量对相等与哈希是多余的，去掉后语义不变。键缩为 16 字节，每个键还少两次字符串哈希与比较。
  - 这项修正同样作用于 cached 切换：每遇到一个新的有效配置对象，切换都要建一次配置缓存键。效果有利，但小到可以忽略。

| 阶段 | 增量：旧口径 Default（缓存）/ Dark / Compact（字节） | 增量：新口径 Default / Dark / Compact（字节） |
| --- | --- | --- |
| 修正前 | +976 / +984 / +984 | 未测 |
| 只有修正 ① | +944 / +952 / +952 | 未测 |
| 修正 ① + ② | +944 / +936 / +936 | +936 / +936 / +936 |

**启动（每个场景 3 次 `run_samples.py`，每次 10 轮交错；表中为 3 次中位数的中位数与波动）**

这一节在两项修正之前的 NOW 上测得，之后没有重测。两项修正只减少每次编译 32 字节和每个算法 8 字节的分配。启动采样使用默认分层编译。

| 场景 / 指标 | PRE 三次（ms） | PRE 中位数 / 波动 | NOW 三次（ms） | NOW 中位数 / 波动 | NOW − PRE | 各次配对 `medianDelta` |
| --- | --- | --- | --- | --- | --- | --- |
| atom-window `Launch.ToWindowOpened` | 675.7, 666.7, 693.3 | 675.7 / 26.6（3.9%） | 689.8, 694.1, 677.6 | 689.8 / 16.5（2.4%） | +14.0（+2.08%） | +7.9, +22.2, −15.0 |
| atom-window `Launch.ToFirstRenderedBatch` | 777.3, 762.0, 791.5 | 777.3 / 29.5（3.8%） | 789.7, 794.8, 777.9 | 789.7 / 16.9（2.1%） | +12.4（+1.59%） | +2.9, +18.2, −21.4 |
| atom-window `Phase.Commit.Theme` | 51.0, 49.8, 51.7 | 51.0 / 1.8 | 51.7, 52.2, 51.4 | 51.7 / 0.8 | +0.7 | +1.2, +2.8, −0.5 |
| atom-splash `Launch.ToSplashFirstFrame` | 669.1, 673.0, 665.2 | 669.1 / 7.8（1.2%） | 669.6, 666.6, 661.3 | 666.6 / 8.2（1.2%） | −2.4（−0.36%） | +0.7, −11.2, +3.8 |
| atom-splash `Launch.ToFirstRenderedBatch` | 973.0, 992.4, 987.7 | 987.7 / 19.4（2.0%） | 975.5, 979.3, 961.5 | 975.5 / 17.8（1.8%） | −12.3（−1.24%） | +14.8, −21.3, −13.8 |
| atom-splash `Launch.ToReady` | 974.8, 1007.2, 1005.5 | 1005.5 / 32.4（3.2%） | 981.0, 986.2, 969.2 | 981.0 / 17.1（1.7%） | −24.5（−2.44%） | −4.6, −32.0, −34.2 |

`JitMethods.Phase.Prepare` 由 1634 增至 1655（每次都 +21），`JitMethods.Phase.Commit.Theme` 由 724 增至 725，来自 Task 1–6 新增的代码。启动指标三次运行的波动为 1.2%–3.9%，所以启动门槛实际能分辨的变化约为 20–30 ms。

**体积（字节，Release net10.0；PRE → NOW 不设门槛）**

表中「NOW」一列在两项修正之前测得。

| 产物 | PRE | NOW（修正前） | 差 |
| --- | --- | --- | --- |
| AtomUI.Core.dll | 1,121,792 | 1,127,936 | +6,144（+0.55%） |
| AtomUI.Controls.dll | 910,848 | 911,360 | +512（+0.06%） |
| AtomUI.Desktop.Controls.dll | 10,841,088 | 10,841,088 | 0 |
| AtomUI.Desktop.Controls.DataGrid.dll | 1,053,184 | 1,053,184 | 0 |
| AtomUI.Desktop.Controls.ColorPicker.dll | 510,976 | 510,976 | 0 |
| Gallery NativeAOT 可执行文件（osx-arm64） | 93,072,248 | 93,121,880 | +49,632（+0.05%） |
| Gallery NativeAOT 发布目录（不含 .pdb/.dSYM） | 113,254,536 | 113,304,168 | +49,632（+0.04%） |

**PRE → NOW 门槛（最终）**

| 门槛 | 结果 |
| --- | --- |
| 冷编译 Default / Dark / Compact（R2） | 三条链都是「差异在噪声范围内」：−0.13% / +0.60% / +0.38%（P25）。本机分辨率为 2.81% / 3.80% / 3.29%，分辨不出 2% |
| 切换往返 uncached（R2） | 差异在噪声范围内：−0.61%（中位数）。分辨率 2.83%，分辨不出 2% |
| 切换往返 cached（R2） | 差异在噪声范围内：−0.58%（P25）。分辨率 2.06%，分辨不出 2% |
| 冷编译分配增量 ≤ `DesignToken` 实例数 × 8 字节（944） | 通过：三条链都是 +936 |
| 启动中位数在基线波动内 | 通过：六项差值都小于 PRE 三次的波动 |
| 值哈希不变 | 通过：三条链 PRE 与 NOW 相同，本轮也不变 |

**NOW 值哈希（Tasks 9–23 以此为准）**

- Default：`80B8BAC59221D9A22EAD273E73F9A2E595F51B53322121576D311C9293875311`
- Dark：`7CE1950AA51CA44B2177A39F35CFA000D9AF2D7C1E9E86ED10FDC4BFD2FE080A`
- Compact：`81DBB30DA0BCB51D7BC1AD40698E1714D4411EA320584472C8FB963BCDA20A34`

- 结果：
  - 两项修正之后：`AtomUI.Core.Tests` 489/489（新增 1 个缓存键语义测试）；`AtomUI.Desktop.Controls.Tests` 4397/4397 通过，测量只列为跳过，没有执行。
  - 全量回归 `[SUMMARY] TOTAL: 12389 个测试全部通过, 总耗时 15:30`（上一任务为 12387）。这个总数由脚本用 `--list-tests` 统计，会把显式运行的测量算进去；该项目实际是 `passed=4397 failed=0 skipped=1`。所以真正执行并通过的是 12388 个（12387 加新增的 1 个），另外 1 个是不执行的测量。
  - 本轮只改了测量文件（冷路径与 `MedianLong`）和台账，没有动生产代码。改完后 `AtomUI.Desktop.Controls.Tests` 仍是 4397/4397，测量被跳过。
- 偏差与说明：
  1. 采用 `[Fact(Explicit = true)]`，没有用简报写的 `[AvaloniaFact(Explicit = true)]`，原因是本项目其他测试都用 `[Fact]`，靠程序集级 `AvaloniaTestFramework` 放到 UI 线程上运行。
  2. 简报默认的 15 次样本 / 3 次预热保留为环境变量的缺省值，基线与以后的比较都按上面的流程。
  3. 冷编译把物化全部控件快照计入，`prepare` 单独输出。`MedianLong` 与 `Median` 统一了偶数长度时的取法：取中间两个值的均值。
  4. 本任务修改了两处生产代码：`ThemeCompiler.MarkExplicit(DesignToken, …)` 与 `AlgorithmCacheKey`，都是按控制方的裁定修改的，见「改动」第 2、3 条。

#### 审阅结论与主控补充（Task 7）

- 首轮审阅：两处产品代码修复（按下标遍历、精简 `AlgorithmCacheKey`）正确且语义安全；测量方法有三个重要问题（默认链的「冷编译」复用了配置与缓存键、暗色链被自行豁免、台账夸大了分辨率），退回修正一轮。复审结论：全部解决，无新的严重或重要问题。Task 6 的提交为 `7f58c7a05`。
- 复审用独立脚本从原始输出重算了五项耗时指标的中位数、波动、范围重叠与差值，与上表逐项一致；统计量的选择都对应 PRE 上较小的波动，而且换用另一个统计量时五项同样通过，不存在事后挑选。
- 关于 PRE 第 1 轮的外部负载高峰：这一轮在每个序列里都是 PRE 的最大值，计入后放大了 PRE 的波动，使上表的「本机分辨率」（2.1%–3.8%）比安静时差。把两棵树的第 1 轮都去掉重算：较大波动变为 1.49%、1.48%、1.50%、1.97%、2.02%，差值为 −0.04%、+0.44%、+0.24%、−0.40%、−0.54%，范围仍重叠。五项结论都不变，其中四项同时满足「不超过 2%」。也就是说，没有外部负载时这台机器对冷编译和无缓存切换能分辨到约 1.5%–2.0%。
- 裁决：正式结论仍按计入全部 7 轮的数字（规则里没有剔除条款，事后剔除等于放宽规则）；去掉高峰轮的重算只作为佐证记录 — 两种算法结论一致 — 判断错了也不改变结论。
- 裁决：后续对比（Task 10 及之后）若某一轮明显受外部负载影响，整批 7 轮重测，而不是剔除单轮。
- 复现测量的必要说明：辅助脚本 `run_measure.sh` 的默认采样数（201/100/101/20）小于规定值，必须显式传入 `CITER=3001 CWARM=300 ITER=501 WARM=50`。这些脚本和判定脚本 `r2_verdict.py` 只存在于本次会话的临时目录，不在仓库里；以本节前面写明的环境变量和命令为准。
- 裁决（计划要求的无断言测试）：`ThemeCompileMeasurement` 是测量工具，只在显式指定时运行，不含断言是设计如此 — 它不承担回归职责 — 最终整体审阅可再议。
- 留待最终整体审阅统一处理的小项：新增的缓存键测试钉的是「描述符实例决定键」这一契约，改动前后都通过，不能单独证明字段已被移除（审阅认为这是合适的常驻契约测试，无需改）。

### Task 8：记账基类 API 与生成器产出 partial 属性的另一半

- 改动：
  1. `AbstractDesignToken` 只新增一个字段 `TokenWriteLog? _writeLog`（null 即不记账），加上 `[AggressiveInlining] protected RecordWrite`（不记账时只是一次空引用判断，无调用、无分配）、`internal BeginRecordingWrites`（分配记录；重复开启抛 `InvalidOperationException`）、`internal EndRecordingWrites`（返回去重后按首次写入顺序的名称并置空字段；未开启时抛异常）、`internal IsRecordingWrites`。新增 `TokenWriteLog`（`List` + `HashSet`，只在记账时分配）。其他 Token 类型一个字段都没加。
  2. 新生成器 `TokenPropertyImplementationGenerator`（`RegisterSourceOutput`，IDE 设计期可见）与 `TokenPropertyImplementationWriter`。管线：`ForAttributeWithMetadataName` 分别接 `ControlDesignToken`、`GlobalDesignToken`，转换只产出值相等的记录（字符串、布尔、`ValueArray`、`TextSpan`/`LinePositionSpan`），不含符号、语法节点或 `Compilation`；每个带特性的类型声明一个输出。生成的 getter 恰为 `get => field;`，setter 为 `set { field = value; RecordWrite(nameof(P)); }`；属性与访问器的修饰符原样照抄声明（`internal`、`private set` 等一致），类型用带可空标注的全限定名。
  3. 属性归属：每个带特性的声明负责自己里面声明的属性；按（文件路径, 位置）排第一的带特性声明另外负责该类型无特性声明里的属性（如 `DesignToken.cs`），保证每个属性只产出一次。规则与现有生成器一致：非 static 实例属性、不带 `[NotTokenDefinition]`、不限可访问性。嵌套或泛型 Token 类型不产出（已有诊断覆盖）。
  4. 文件名：控件 Token 为 `<TypeName>.TokenProperties.g.cs`；全局 Token 按声明所在源文件命名（`DesignToken.Seed.cs` → `DesignToken.Seed.TokenProperties.g.cs`），同名时追加序号。
  5. `RunDefaultCalculation` 只在 sealed 且 partial 的控件 Token 上产出。
  6. `ATOMUIGEN049`（Error，`isEnabledByDefault: false`），按属性报告，位置为属性标识符；类或属性不是 partial 时该属性不产出任何代码。诊断文档已加 049 一行（注明暂默认关闭）。
- TDD：RED 时 Generator.Tests 编译失败（`TokenPropertyImplementationGenerator` 不存在），Core.Tests 编译失败（`RecordWrite`/`BeginRecordingWrites` 等不存在）。GREEN：新增的 6 + 9 条测试全部通过。
- 测试：简报列出的 6 条 Core、8 条生成器测试名称照用；另加 1 条 `Pipeline_Output_Is_Cached_When_An_Unrelated_File_Changes`（开启步骤跟踪，改无关文件后模型步骤全部为 Cached/Unchanged），用来钉住管线输出可比较相等。049 的测试同时断言默认不报告。
- 结果：Core.Tests 495/495，Generator.Tests 1094/1094；`dotnet build AtomUI.slnx` 0 错误，警告只有 NU1900（离线），没有 049、没有 CS8785；全量回归 `[SUMMARY] TOTAL: 12404 个测试全部通过, 总耗时 15:55`（12389 + 15；Desktop.Controls.Tests 仍是 passed=4397 skipped=1）。
- 偏差与说明：
  1. 049 用外部文件位置（`Location.Create(path, span, lineSpan)`）报告，以免模型持有语法树；IDE 按路径映射，显示正常。
  2. `EndRecordingWrites` 在未开启记账时抛异常（简报没写这种情况）。
  3. 本任务没有转换任何 Token 类，仓库构建与之前完全相同；性能测量留给 Task 10。
- 审阅结论：规格符合，质量通过，无严重或重要问题。Task 7 的提交为 `596b57b70`（两处性能修复）与 `c0b74a816`（测量工具与基线）。审阅逐条核对了性能硬约束：生成的 getter 是 `get => field;`；setter 只多一句 `RecordWrite(nameof(P))`；不记账时只有一次空引用判断；`AbstractDesignToken` 只新增一个字段；管线模型只含按值比较的数据。`RecordWrite` 跨程序集是否被内联无法从代码确认，由 Task 10 的测量回答。
- 带入 Task 9 一并处理的两项（Task 9 会让真实代码依赖这个生成器）：
  1. `DesignToken` 的 14 个带特性的声明，每个都会遍历全部约 255 个成员并读取特性，之后才判断属性归哪个声明。IDE 里每次按键都会跑。改为先判断归属，再读特性。
  2. 控件 Token 的生成文件名只用类型简单名。同一程序集里两个同名 Token 类在不同命名空间时生成器会整体崩溃，Task 9 之后那会让该程序集所有 partial 属性报「缺少实现」。改为文件名带命名空间，或报诊断。
- 带入 Task 11 的一项：`BeginRecordingWrites` 与 `EndRecordingWrites` 之间的代码抛异常时 Token 会停在记账状态，下一次 `Begin` 抛出的异常会盖住原始错误。调用方必须在 `finally` 里调用 `EndRecordingWrites`；Task 11 实现时同时把这条约定写进方法注释。
- 留待最终整体审阅统一处理的小项：
  1. `ATOMUIGEN049` 的位置没有语法树，`#pragma` 和按文件生效的 `.editorconfig` 对它不起作用，只能用项目级设置；文档里应写明。台账里「IDE 显示正常」是未经验证的说法。
  2. 全局命名空间、`init`、`required` 等形态代码已处理但没有测试。
  3. `EndRecordingWrites` 在未记账时抛异常，行为合理但没有测试。
  4. 缓存测试用 `ShouldAllBe`，序列为空时也会通过，应先断言非空；只覆盖了控件 Token、无诊断的情形。
  5. 用户手写了实现部分时，生成器仍会再生成一份，得到的是编译器的重复实现错误，而不是清楚的诊断。
