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

### Task 9：全部 Token 改成 partial 属性，控件 Token 类改公开

- 改动：
  1. 一次性脚本（放在会话临时目录 `scratchpad/codemod/partial_token_properties.py`，不进仓库）只做文本替换，规则：
     - 作用范围：带 `[ControlDesignToken]`/`[GlobalDesignToken]` 的类，以及同项目里同名的无特性 partial 声明（即 `DesignToken.cs`）。
     - 改写内容：
       - 类声明加 `partial`；四个控件包里的类由 `internal` 改为 `public`。
       - 非 static、无 `[NotTokenDefinition]` 的自动属性，在类型前插入 `partial`。可访问性、特性、初始化器、多余空格原样保留。
       - `public override void CalculateTokenValues` 改为 `protected`。
     - 脚本先屏蔽注释、字符串和预处理行，再按大括号匹配成员。默认只报告，`--apply` 才写入。对已改过的代码再跑一遍，报告「0 个文件待改」。
  2. 改动顺序：先只改 `ToolTipToken.cs`，`dotnet build` 只报 CS0507，原因是基类此时还是 `public virtual`，partial 属性那部分编译通过。然后全量运行脚本。
  3. 手工改动：
     - `AbstractControlDesignToken.CalculateTokenValues` 改为 `protected virtual`。
     - `GeneratedThemeSchemaWriter` 生成的 Evaluate 改调 `RunDefaultCalculation`。
     - `ATOMUIGEN049` 的 `isEnabledByDefault` 改为 `true`；诊断文档去掉「暂默认关闭」。
  4. Task 8 审阅带入的两项：
     - 生成器先按语法判断属性归哪个声明，再读 `[NotTokenDefinition]`。非本声明的属性不再调用 `GetAttributes()`。
     - 生成文件名带命名空间：控件为 `<Namespace>.<Type>.TokenProperties.g.cs`，全局为 `<Namespace>.DesignToken.Seed.TokenProperties.g.cs`。同一程序集里两个同名 Token 类不再让 `AddSource` 抛异常。新增测试 `Same_Named_Token_Classes_In_Different_Namespaces_Get_Distinct_Files`。
  5. 测试：
     - 直接调用 `.CalculateTokenValues(` 的测试改调 `RunDefaultCalculation`（Core 2 处、DataGrid 1 处、Desktop 9 处）。Core.Tests 手写的 3 个 Token 类改为 `protected override`，并各自带一个内部 `RunDefaultCalculation`。
     - Generator.Tests 用真实 Core 编译的源码桩改为 `protected override`。被生成 Schema 引用的终端桩手写 `RunDefaultCalculation`，因为这些测试只运行 `TokenResourceKeyGenerator`。
     - 自带基类桩的 `ThemeSchemaGeneratorTests`、`TokenResourceKeyGeneratorTests`：桩基类改为 `protected virtual`，并加内部 `RunDefaultCalculation`；断言 Evaluate 文本的期望串同步修改。跨程序集引用桩基类的 2 个测试在终端类上手写该方法。
     - `TokenPropertyImplementationGeneratorTests` 去掉「默认关闭」断言和 `enable049` 参数，期望文件名带命名空间。
     - `TriggerControls.cs` 的 5 个类由脚本转换。
- 实际数量（脚本报告）：

  | 项目 | 类（sealed/abstract） | 改为 public | 属性 | 重写 |
  | --- | --- | --- | --- | --- |
  | AtomUI.Core（`DesignToken`，14 个带特性声明 + `DesignToken.cs`） | 1 个类 | — | 255（初始化器 34，`[NotTokenDefinition]` 跳过 1） | — |
  | AtomUI.Controls | 3（2/1） | 3 | 21 | 2 |
  | AtomUI.Desktop.Controls | 140（110/30） | 140 | 1011（`[ReservedToken]` 27） | 129 |
  | ColorPicker | 6（4/2） | 4（另 2 个原已公开） | 27 | 4 |
  | DataGrid | 1（1/0） | 1 | 78（`internal` 45 个保持，`[ReservedToken]` 11） | 1 |
  | AtomUI.Toolkits.GalleryBase | 8（8/0） | 保持 internal | 45（初始化器 1，`[ReservedToken]` 1） | 8 |
  | Registration.Fixtures/ThirdParty | 5（5/0） | 原已公开 | 5（初始化器 5） | — |

  控件 Token 共 158 个类、148 个改为 public、1182 个属性、144 处重写，`DesignToken` 255 个属性，与变基前的调查完全一致。Skeleton、Card 等基类上游搬到了共享程序集，但数量不受影响。其他：`[ReservedToken]` 39 个，初始化器 35 个（不含夹具），奇异空格 `{  get; set; }` 2 处，全部保留。
- 可访问性：公开后没有出现 CS0053/CS0050，因为没有任何 Token 属性的类型是内部类型。所以没有需要裁定的项，也没有把属性改成 `internal`。
- 脚本处理不了、改为手工的：无。干跑时发现脚本把 `#region` 行当成成员头（DataGrid 的 2 个 `internal` 属性会被插错位置），已修正脚本再运行。全量结果逐行核对：每处改动只属于「插入 partial」「internal→public」「public override→protected override」三类之一。
- 结果：
  - 三个值哈希与 Task 7 基线逐字相同：Default `80B8BAC5…3875311`、Dark `7CE1950A…FE080A`、Compact `81DBB30D…CDA20A34`。
  - `dotnet build AtomUI.slnx --no-incremental` 0 错误，除 NU1900 外没有警告，没有 049。
  - 解决方案外的测试项目（Desktop.Controls/DataGrid/Generator/Rendering/Gallery 等 8 个）与 `ThirdParty` 夹具也都是 0 错误。
  - `AtomUI.Registration.Fixtures/Complex`、`Minimal` 与 `tools/performances/AtomUI.Performance` 单独构建会报错（`UseAtomUI`、`SpaceItemsAlignment` 命名空间），与本任务无关，是原有问题。
  - Core.Tests 495/495；Generator.Tests 1095/1095（新增 1 个）；Themes.Tests 27/27（改前改后相同）。
  - 全量回归：第一轮 1 个失败，`WindowTitleBarTokenTests` 按源码文本断言旧写法 `public double LogoAndLeftAddOnSpacing { get; set; }`，简报没列出这处；断言改为 partial 写法后通过。第二轮 `[SUMMARY] TOTAL: 12405 个测试全部通过, 总耗时 15:40`，比上一任务的 12404 多 1 个新增生成器测试；Desktop.Controls.Tests 为 passed=4397 skipped=1。
- 遗留给 Task 23 的文档：`docs/guides/theming/third-party-control-packages.md`、`docs/architecture/systems/theming/control-design-token-inheritance.md`、`docs/modules/toolkits-gallery-base/theming-localization.md`、`.github/copilot-instructions.md` 仍写着旧写法（`internal sealed class`、`public override`）。按旧写法写的第三方 Token 现在会报 `ATOMUIGEN049`/CS0507。Task 23 的文件清单只列了 `control-token-guidelines.md`。
- 审阅结论：规格符合，质量通过，无严重或重要问题。Task 8 的提交为 `058f778f8`。
  - 主控用脚本对全部改动逐行归类：属性声明仅插入 `partial` 的 1442 处（150 个文件，含测试夹具的 5 处）；类声明改公开并加 `partial` 的 148 处；类声明只加 `partial` 的 15 处（GalleryBase 8、测试夹具 5、原本已公开的 2）；`CalculateTokenValues` 重写改 `protected` 的 165 处（`src` 下 144，其余在生成器测试的源码桩里）；仅空白变化 4 处。其余改动集中在 21 个文件（生成器、基类、测试、文档），没有一个是 Token 类文件。
  - 审阅读过归类规则并抽查了十多个不同形态的文件（抽象中间层、终端类、`DataGridToken` 的 45 个 internal 属性、带初始化器的 `DesignToken` 分部文件、未改动的 `DesignToken.cs`、GalleryBase、ColorPicker、测试夹具），未发现被归类掩盖的改动。
  - 完整性：全仓带 `[ControlDesignToken]` 或 `[GlobalDesignToken]` 的声明共 177 个（14 个 `DesignToken` 分部声明加 163 个类），全部是 `partial class`；没有遗漏。仓库里不再有 Token 类之外对 `CalculateTokenValues` 的调用。
  - 新公开的类只暴露类声明和 Token 属性，没有公开方法、字段或嵌套类型。
  - 三个单独构建不过的项目（`AtomUI.Registration.Fixtures` 的 Complex 与 Minimal、`tools/performances/AtomUI.Performance`）出错的文件都不在本次 194 个改动文件里，与本任务无关。
- 需要有意识确认的一项 API 承诺：32 个抽象中间层公开后，外部程序集可以继承它们，它们的 `protected` 辅助方法（例如 `AbstractButtonGeometryToken.CalculateContentPadding`）因此成为外部可达的 API。这与规格一致（规格把 `protected` 成员与 `EffectiveGlobalToken`、`IsExplicit` 同等看待），Task 23 的文档要写明。
- Task 23 要补的文档（它们还在展示旧的 Token 写法，原计划的文件清单没列）：`docs/guides/theming/third-party-control-packages.md`、`docs/architecture/systems/theming/control-design-token-inheritance.md`、`docs/modules/toolkits-gallery-base/theming-localization.md`、`.github/copilot-instructions.md`。
- 留待最终整体审阅统一处理的小项：
  1. 生成器测试的共享桩基类改了签名，但另外 4 个使用它的测试文件里的源码桩仍写 `public override`（`GeneratedThemeConsumptionWriterTests`、`OwnTokenReferenceAnalysisTests`、`ThemeAssetConsumptionAnalyzerTests`、`CompiledThemeAlgorithmContractTests`）。这些测试输入现在带编译错误，只因测试不检查编译错误才通过。
  2. `AtomUIDiagnosticDescriptors.cs` 里 049 上方「默认关闭」的注释已过时。
  3. `ThemeSchemaGeneratorTests` 的桩把 `RunDefaultCalculation` 放在抽象基类上，真实生成器只在密封终端类上生成它。
  4. 全局命名空间里的 Token 的生成文件名没有测试。

### Task 10：Token 记账改造后的性能门槛验收（BASE = `c0b74a816`；第一次测量的 NOW = `a059c4b01`，第二次测量的 NOW = `a059c4b01` 加补救 A，即本任务的提交）

**总结论：补救 A 之后门槛仍未通过，按第 3 步停止；随后用户裁决按实测结果放行（见本节末尾「用户裁决」）。**

- 第一次测量（改造原样）：冷编译慢了约 6.4%–6.9%（G1–G3 不过），启动分段指标落在 BASE 之外（G8 不过）。
- 按主控裁定实施「补救 A」（冷路径移出内联）后第二次测量：冷编译仍慢约 2.3%–2.7%，两棵树范围仍不重叠（G1–G3 不过）；G8 按字面规则仍有指标落在 BASE 范围外（见补救 A 一节）。其余门槛都通过。
- 一次冷编译约 3.16 万次 Token 属性写入，其中 94% 来自全局 Token 复制（`GeneratedThemeSchema.CopyGlobalValues`），剩余差异主要落在那里。

#### 第一次测量（改造原样，NOW = `a059c4b01`）：未通过

结论：冷编译三条链都慢了约 6.4%–6.9%，两棵树的取值范围完全不重叠（G1–G3 不过）；启动的 `Phase.Prepare` 等分段指标落在 BASE 波动之外（G8 不过）。分配、体积、值哈希、既有性能测试都通过。原因见「原因」一节：`RecordWrite` 跨程序集内联了，但它的冷路径（`TokenWriteLog.Add`，含 `HashSet.Add` 与 `List.Add`）也被一起内联进每一个 setter 调用点，编译期代码大幅膨胀，部分 setter 因此不再被内联。这一轮没有改任何生产代码。

- 机器与环境：MacBookPro18,2（Apple M1 Max，10 核，64 GB），macOS 26.6.2（25G83），.NET SDK 11.0.100，测试目标 net10.0（运行时 10.0.x），Release。与 Task 7 同一台机器、同一配置。
- 取得两棵树：`git archive --format=tar -o <S>/task10/base.tar c0b74a816`，然后 `tar -xf <S>/task10/base.tar -C <S>/task10/base`。NOW 是本工作树。测量文件 `ThemeCompileMeasurement.cs` 在两棵树里逐字相同（`diff` 无输出），`git diff c0b74a816 a059c4b01` 对它也无差异。两棵树都执行 `dotnet build tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj -c Release`，再 `dotnet build-server shutdown`；BASE 的 Default 冷编译分配（1,136,160 字节）与 Task 7 登记的 NOW 完全一致，说明 BASE 构建无误。
- 原始数据都在临时目录 `<S>/task10/`（`<S>` 为 `scratchpad/perf`），不提交。

**时间（G1–G5）**

- 命令：Task 7 规定的命令，`DOTNET_TieredCompilation=0`，冷编译 3001 次计时 / 300 次预热，切换 501 次 / 50 次。交错脚本 `<S>/task10/scripts/run_measure.sh <out> 7`（BASE、NOW、BASE、NOW…，采样数必须显式传入，脚本缺省会报错），判定脚本仍是 Task 7 的 `<S>/scripts/r2_verdict.py`（脚本里的 `pre` 即 BASE）。数据在 `<S>/task10/runs-a`。
- 外部负载：开始前约定「某轮开始时 1 分钟负载 > 15 即视为受干扰，整批重测」。本批 14 次运行开始时的 1 分钟负载为 5.78–8.86（逐轮：BASE 8.67/7.96/6.01/5.78/8.86/8.55/6.12，NOW 6.73/7.17/6.65/7.67/7.35/7.69/6.96），没有受干扰的轮次，整批保留，未丢弃任何批次。
- 统计量只看 BASE 自己的波动选定（与 Task 7 的固定选择一致）：

| 指标 | BASE 波动（每次运行中位数） | BASE 波动（每次运行 P25） | 门槛统计量 |
| --- | --- | --- | --- |
| 冷编译 Default | 7.28% | 2.28% | P25 |
| 冷编译 Dark | 2.83% | 2.70% | P25 |
| 冷编译 Compact | 3.00% | 2.26% | P25 |
| 切换往返 uncached | 1.78% | 2.43% | 中位数 |
| 切换往返 cached | 1.45% | 0.71% | P25 |

按所选统计量，每次运行的数值（ms）：

| 指标 | BASE 七次 | BASE 范围 / 中位数 / 波动 | NOW 七次 | NOW 范围 / 中位数 / 波动 | NOW − BASE |
| --- | --- | --- | --- | --- | --- |
| 冷编译 Default（P25） | 0.6924, 0.6947, 0.6928, 0.6872, 0.6898, 0.6946, 0.7030 | 0.6872–0.7030 / 0.6928 / 0.0158（2.28%） | 0.7340, 0.7376, 0.7343, 0.7395, 0.7435, 0.7377, 0.7435 | 0.7340–0.7435 / 0.7377 / 0.0095（1.29%） | +0.0449（+6.48%） |
| 冷编译 Dark（P25） | 0.7041, 0.6937, 0.7008, 0.6921, 0.6977, 0.6997, 0.7110 | 0.6921–0.7110 / 0.6997 / 0.0189（2.70%） | 0.7442, 0.7486, 0.7409, 0.7422, 0.7544, 0.7442, 0.7583 | 0.7409–0.7583 / 0.7442 / 0.0174（2.34%） | +0.0445（+6.36%） |
| 冷编译 Compact（P25） | 0.6946, 0.6923, 0.6941, 0.6908, 0.6896, 0.6937, 0.7053 | 0.6896–0.7053 / 0.6937 / 0.0157（2.26%） | 0.7416, 0.7415, 0.7356, 0.7333, 0.7481, 0.7402, 0.7497 | 0.7333–0.7497 / 0.7415 / 0.0164（2.21%） | +0.0478（+6.89%） |
| 切换往返 uncached（中位数） | 12.0573, 11.9999, 12.0778, 12.0796, 12.0455, 12.0308, 12.2142 | 11.9999–12.2142 / 12.0573 / 0.2143（1.78%） | 12.0528, 12.1706, 12.0613, 12.1347, 12.3158, 12.0895, 12.2787 | 12.0528–12.3158 / 12.1347 / 0.2630（2.17%） | +0.0774（+0.64%） |
| 切换往返 cached（P25） | 10.3004, 10.2968, 10.3078, 10.2851, 10.2782, 10.3226, 10.2498 | 10.2498–10.3226 / 10.2968 / 0.0728（0.71%） | 10.3070, 10.3026, 10.2135, 10.3210, 10.3894, 10.2557, 10.3405 | 10.2135–10.3894 / 10.3070 / 0.1759（1.71%） | +0.0102（+0.10%） |

判定与分辨率：

| 指标 | 范围重叠 | 差值绝对值 < 较大波动 | 判定 | 本机分辨率（较大波动） |
| --- | --- | --- | --- | --- |
| G1 冷编译 Default | 否 | 0.0449 ≥ 0.0158 | **存在 +6.48% 的差异，不过** | 2.28%：本批分辨不出 2%，但 6.5% 远大于它 |
| G2 冷编译 Dark | 否 | 0.0445 ≥ 0.0189 | **存在 +6.36% 的差异，不过** | 2.70%：分辨不出 2% |
| G3 冷编译 Compact | 否 | 0.0478 ≥ 0.0164 | **存在 +6.89% 的差异，不过** | 2.36%：分辨不出 2% |
| G4 切换往返 uncached | 是 | 0.0774 < 0.2630 | 差异在噪声范围内，通过 | 2.18%：分辨不出 2% |
| G5 切换往返 cached | 是 | 0.0102 < 0.1759 | 差异在噪声范围内，通过（较大波动 < 2%，差值 0.10% ≤ 2%） | 1.71%：本批能分辨 2% |

另一种统计量（不作门槛，仅供参考）：冷编译中位数 Default +7.08%、Dark +7.54%、Compact +7.94%；uncached 切换 P25 +1.20%（波动 2.43% / 1.63%）；cached 切换中位数 +0.64%。`defaultMemoised`（旧口径，不设门槛）P25 +6.71%，范围也不重叠。每次运行的「首次编译」（含 JIT，不设门槛）BASE 中位数 607.8 ms、NOW 702.5 ms（+15.6%）。uncached 切换只有约 +0.6%，与「每次切换编译一次、编译只占切换耗时的一小部分」相符；cached 切换不编译，不受影响。

**分配（G6）**

- Token 实例数是真实计数，不是公式推算：把 `base.tar` 和 `git archive a059c4b01` 分别解到临时目录的两份**独立副本**（`<S>/task10/count-base`、`count-now`），用脚本 `<S>/task10/scripts/patch_counter.py` 给副本里的 `AbstractDesignToken` 加一个受保护构造函数，对静态计数器做 `Interlocked.Increment`，并让副本里的测量在每个计时样本的编译区间（与分配同一区间）前后读计数。两份副本各自 Release 构建、运行 21 次计时样本。计时用的两棵树里没有这个计数器，本工作树也从未改动。
- 结果：两棵树、四条链、每个样本都恰好构造 **234** 个 Token（118 个 `DesignToken` + 116 个控件 Token，与测量里按公式算出的 `tokenInstances` 一致）；准备有效配置的区间构造 0 个。计数副本的分配数值与计时树逐字相同，说明计数器本身不分配。

| 链 | BASE（字节） | NOW（字节） | 增量 | 预算 234 × 8 = 1,872 | 每实例增量 |
| --- | --- | --- | --- | --- | --- |
| Default | 1,136,160 | 1,138,032 | +1,872 | 通过（恰好等于预算） | 8.00 |
| Dark | 1,141,352 | 1,143,224 | +1,872 | 通过 | 8.00 |
| Compact | 1,136,544 | 1,138,416 | +1,872 | 通过 | 8.00 |
| defaultMemoised（不设门槛） | 1,119,912 | 1,121,784 | +1,872 | — | 8.00 |

七次运行的分配完全相同（中位数、最小、最大都一致）。增量正好是每个 Token 实例多一个 8 字节的 `_writeLog` 引用字段，不记账时没有其他分配。

**既有性能测试（G7）**：`git diff c0b74a816 a059c4b01 -- tests/AtomUI.Core.Tests/Theme/Performance` 无输出，断言未改。`dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj` 495/495 通过；其中 `Theme.Performance` 下的 5 个测试（`ThemeResourceLookupPerformanceTests`、`ThemeScopeAndTransactionPerformanceTests`）单独运行也是 5/5。通过。

**启动（G8，默认分层，不设 `DOTNET_TieredCompilation`）**

- 命令：两棵树分别 `dotnet publish tools/performances/AtomUI.StartupPerformance/AtomUI.StartupPerformance.csproj -c Release -f net10.0 --self-contained false -o <S>/task10/startup-<base|now>`（两份 `AtomUI.Core.dll` 的 SHA-256 不同；工具对两边可执行文件哈希相同的提示来自 apphost，属正常）。每个场景一份 `<S>/task10/variants-<scenario>.json`，`atom-window`、`atom-splash` 交替各跑 3 次 `python3 tools/performances/AtomUI.StartupPerformance/run_samples.py --config … --output <S>/task10/startup/<scenario>-r<n> --rounds 10 --warmup 1 --timeout 90`。全部样本无失败。
- 丢弃的一批：第一批（`<S>/task10/startup-discarded1`）第 3 次 `atom-window` 开始时 1 分钟负载为 27.89，超过事先约定的 15，整批丢弃重测。重测时观察到负载在每次 `atom-splash` 运行期间从约 8 升到约 19，而重测前后机器上没有别的任务，所以这很可能是采样本身（每次运行启动 22 个带 UI 的进程）推高的负载，而不是外部负载；重测批的开始负载为 8.93、8.28、18.82、14.24、19.53、16.67，没有再丢弃。工具对所有样本都标了 `highLoadSamples`（负载 > 5），与 README 所说的「提示而非失败」相符。丢弃批的结论与下表一致（`Phase.Prepare` 在 splash 中同样 +1.3 ms 且三次配对都为正）。
- 结果（3 次运行中位数的中位数与波动）：

| 场景 / 指标 | BASE 三次（ms） | BASE 中位数 / 波动 | NOW 三次（ms） | NOW 中位数 / 波动 | NOW − BASE | NOW 中位数落在 BASE 范围内 | 各次配对 `medianDelta` |
| --- | --- | --- | --- | --- | --- | --- | --- |
| atom-window `Launch.ToWindowOpened` | 667.0, 677.5, 700.8 | 677.5 / 33.8（5.0%） | 700.4, 700.3, 681.6 | 700.3 / 18.8 | +22.8（+3.37%） | 是 | +19.0, +15.5, −0.1 |
| atom-window `Launch.ToFirstRenderedBatch` | 759.1, 774.8, 803.8 | 774.8 / 44.7（5.8%） | 801.5, 803.4, 777.9 | 801.5 / 25.5 | +26.7（+3.45%） | 是 | +17.3, +15.0, −5.6 |
| atom-window `Phase.Commit` | 96.7, 96.7, 97.7 | 96.7 / 1.0 | 97.6, 98.4, 96.9 | 97.6 / 1.4 | +0.9（+0.98%） | 是 | +0.2, +1.8, −0.1 |
| atom-window `Phase.Commit.Theme` | 51.7, 51.7, 52.0 | 51.7 / 0.3 | 52.2, 52.9, 51.7 | 52.2 / 1.2 | +0.5（+0.97%） | **否** | +0.7, +1.0, −0.2 |
| atom-window `Phase.Prepare` | 73.2, 74.3, 75.5 | 74.3 / 2.2 | 76.0, 76.2, 74.8 | 76.0 / 1.4 | +1.7（+2.28%） | **否** | +2.4, +2.1, −1.3 |
| atom-splash `Launch.ToSplashFirstFrame` | 680.6, 668.5, 658.4 | 668.5 / 22.3（3.3%） | 669.0, 677.3, 672.4 | 672.4 / 8.4 | +4.0（+0.59%） | 是 | −31.2, −5.3, +14.1 |
| atom-splash `Launch.ToFirstRenderedBatch` | 993.5, 968.3, 955.0 | 968.3 / 38.5（4.0%） | 967.2, 994.0, 983.9 | 983.9 / 26.8 | +15.6（+1.61%） | 是 | −41.1, +12.7, +7.4 |
| atom-splash `Launch.ToReady` | 1000.4, 979.9, 956.2 | 979.9 / 44.2（4.5%） | 973.8, 1005.2, 1001.6 | 1001.6 / 31.4 | +21.8（+2.22%） | **否**（超出 1.2 ms） | −40.2, +3.3, +21.6 |
| atom-splash `Phase.Commit` | 80.7, 81.6, 82.1 | 81.6 / 1.4 | 80.3, 81.4, 81.7 | 81.4 / 1.4 | −0.3 | 是 | −0.7, +0.1, +0.1 |
| atom-splash `Phase.Commit.Theme` | 49.6, 50.8, 50.4 | 50.4 / 1.1 | 50.0, 50.4, 50.6 | 50.4 / 0.6 | +0.0 | 是 | +0.3, −0.3, +0.3 |
| atom-splash `Phase.Prepare` | 69.8, 70.5, 70.0 | 70.0 / 0.7 | 71.1, 72.5, 70.9 | 71.1 / 1.6 | +1.1（+1.59%） | **否** | +1.2, +1.8, +0.9 |

`JitMethods.Phase.Prepare` 1654 → 1655（每次都 +1），`JitMethods.Phase.Commit.Theme` 725 → 725。

- 判定：**G8 不过**（按简报「每个指标的中位数落在 BASE 波动范围内」逐项判定）。`Launch.*` 六项里五项落在 BASE 范围内，`Launch.ToReady` 超出 1.2 ms（差值 21.8 ms 仍小于 BASE 波动 44.2 ms，按 Task 7 的写法「差值小于波动」会算通过）；这些指标的分辨率约 20–45 ms，看不出 1–2 ms 的变化。分段指标分辨率高得多，`Phase.Prepare` 在两个场景中都慢了 1.1–1.7 ms，splash 三次配对全为正、丢弃批也同样为正，是真实信号；`atom-window` 的 `Phase.Commit.Theme` +0.5 ms 也超出 BASE 的 0.3 ms 波动。

**体积（G9）**

| 产物 | BASE（字节） | NOW（字节） | 差 |
| --- | --- | --- | --- |
| AtomUI.Core.dll | 1,127,424 | 1,125,888 | −1,536（−0.14%） |
| AtomUI.Controls.dll | 910,848 | 911,872 | +1,024（+0.11%） |
| AtomUI.Desktop.Controls.dll | 10,841,088 | 10,830,848 | −10,240（−0.09%） |
| AtomUI.Desktop.Controls.DataGrid.dll | 1,053,184 | 1,052,672 | −512（−0.05%） |
| AtomUI.Desktop.Controls.ColorPicker.dll | 510,976 | 510,976 | 0 |
| Gallery NativeAOT 可执行文件（osx-arm64） | 93,121,880 | 93,237,640 | +115,760（+0.12%） |
| Gallery NativeAOT 发布目录（不含 .pdb/.dSYM） | 113,304,168 | 113,419,928 | +115,760（+0.10%） |

程序集取自 `.artifacts/bin/Release/net10.0`；AOT 命令与 Task 7 相同：`dotnet publish controlgallery/AtomUIGallery.Desktop/AtomUIGallery.Desktop.csproj -c Release -r osx-arm64 -p:GalleryPublishTrimmed=true -p:GalleryPublishAot=true -o <S>/task10/gallery-aot-<base|now>`。BASE 的 AOT 可执行文件与 Task 7 登记的 NOW 字节数相同。全部 ≤ 1%，**通过**。

**值哈希（G10）**：两棵树七次运行全部相同，与 Task 7 基线逐字一致——Default `80B8BAC59221D9A22EAD273E73F9A2E595F51B53322121576D311C9293875311`、Dark `7CE1950AA51CA44B2177A39F35CFA000D9AF2D7C1E9E86ED10FDC4BFD2FE080A`、Compact `81DBB30DA0BCB51D7BC1AD40698E1714D4411EA320584472C8FB963BCDA20A34`。**通过**。所有运行的 `sanity.*` 核对项与 Task 7 相同（三条门槛链每个样本的配置对象和缓存键都是新的）。

**原因（诊断，不是门槛）**

- 反汇编：直接运行测试可执行文件 `.artifacts/bin/Release/net10.0/AtomUI.Desktop.Controls.Tests -class …ThemeCompileMeasurement -explicit only`，设 `DOTNET_TieredCompilation=0`、`DOTNET_JitDisasm="*DesignToken:set_ColorPrimaryActive *ToolTipToken:CalculateTokenValues *DesignToken:.ctor"`、`DOTNET_JitStdOutFile=<S>/task10/disasm/now-jit.txt`（经 `dotnet test` 运行时拿不到输出；方法名要带 `*` 前缀才能匹配）。`set_ToolTipBackground`、`set_ColorPrimary` 在测量过程中从未被单独编译——它们在调用点全部被内联——所以改看内联了它们的 `ToolTipToken.CalculateTokenValues`，以及被单独编译的 `DesignToken.set_ColorPrimaryActive`。
- **`RecordWrite` 跨程序集内联了**：`ToolTipToken.CalculateTokenValues`（控件包）里没有对 `RecordWrite` 的调用，每次写入之后是 `ldr x20, [x19, #0x08]; cbz x20, …`，即一次字段读取加一次空判断。`DesignToken` 的 setter 同样如此。
- **但冷路径也一起内联了**：`[AggressiveInlining]` 的 `RecordWrite` 把 `log.Add(tokenName)` 带进来，`TokenWriteLog.Add` 又内联了 `HashSet.Add` 的调用和 `List.Add` 的快速路径与扩容调用。于是每次属性写入在调用点展开出约 100 字节的冷路径代码，setter 从 8 字节 IL 的「总是内联」变成 19 字节 IL、180 字节机器码、带栈帧的方法。
  - `ToolTipToken.CalculateTokenValues` 机器码 1048 字节（6 个属性）。
  - JIT 摘要（`DOTNET_JitDisasmSummary=1`，同一测量，两棵树对比，文件 `<S>/task10/disasm/summary-measure*.txt`）：两棵树都编译的 AtomUI 方法里有 354 个机器码变大，合计 +239,552 字节。例如 `GeneratedGlobalTokenAccessor.SetValue` 26,924 → 39,980，`DesignToken.CalculateAliasTokenValues` 2,524 → 14,264，`StepsToken.CalculateTokenValues` 1,104 → 11,628，`AbstractThemeVariantCalculator.CalculateColorMapTokenValues` 772 → 10,308，`NavMenuToken.CalculateTokenValues` 1,340 → 10,036，`GeneratedThemeSchema.CopyGlobalValues` 10,968 → 16,900。
  - 被单独编译（即至少有一处调用点没有内联）的 Token setter 从 44 个增至 208 个（`DesignToken` 192 个、控件 Token 16 个）：setter 变大后，大方法的内联预算耗尽，部分写入变成了真正的调用。
  - 这些都发生在编译期热路径上（全局公式、调色板、全局 Token 复制、控件公式），与只在冷编译上出现约 6.5% 的退化、`TieredCompilation=0` 下首次编译多出约 95 ms JIT 时间、默认分层下 `Phase.Prepare` 多出 1–2 ms 的现象一致。
- 规格 4.11 的小样实测（一次计算写 6 个属性）测不出这一点：代码膨胀和内联预算耗尽只在上千次写入、几十 KB 的真实编译方法里才显现。这是推断，本任务没有复现小样。
- 计划 Task 10 第 3 步列出的两种改法正好针对这里（生成器把空判断直接展开在 setter 里；或者冷路径放进 `NoInlining` 的辅助方法，让 setter 只剩「读字段、判空、调用」）。本任务按简报不做修改，也没有试验这些改法。

**第一次测量门槛汇总**

| 门槛 | 结果 |
| --- | --- |
| G1 冷编译 Default（R2，P25） | **不过**：+6.48%，范围不重叠；分辨率 2.28% |
| G2 冷编译 Dark（R2，P25） | **不过**：+6.36%，范围不重叠；分辨率 2.70% |
| G3 冷编译 Compact（R2，P25） | **不过**：+6.89%，范围不重叠；分辨率 2.36% |
| G4 切换往返 uncached（R2，中位数） | 通过：+0.64%，在噪声内；分辨率 2.18%，分辨不出 2% |
| G5 切换往返 cached（R2，P25） | 通过：+0.10%，在噪声内；分辨率 1.71% |
| G6 分配增量 ≤ Token 实例数 × 8 | 通过：实测 234 个实例，预算 1,872，三条链增量都是 +1,872（每实例 8.00 字节） |
| G7 既有性能测试 | 通过：断言未改，Core.Tests 495/495 |
| G8 启动 | **不过**：`Phase.Prepare` 两个场景 +1.1/+1.7 ms 超出 BASE 范围，window `Phase.Commit.Theme` +0.5 ms 超出，splash `Launch.ToReady` 超出 BASE 范围 1.2 ms；其余五项 `Launch.*` 在范围内 |
| G9 体积 ≤ 1% | 通过：程序集 −0.14%～+0.11%，AOT +0.12% |
| G10 值哈希 | 通过：三条链完全相同 |

#### 补救 A（主控裁定）与第二次测量：仍未通过

- 改动（唯一的生产代码改动，只在 `src/AtomUI.Core/Theme/DesignTokens/AbstractDesignToken.cs`）：
  - `RecordWrite` 仍是 `protected` 且 `[AggressiveInlining]`，函数体只剩「`_writeLog` 非空时调用私有辅助方法」。
  - 新增 `private`、`[MethodImpl(MethodImplOptions.NoInlining)]` 的 `AddRecordedWrite(string)`，执行原来记账分支的 `Add`。
  - 字段、`BeginRecordingWrites`/`EndRecordingWrites`/`IsRecordingWrites`、生成器与生成的 setter 文本都不变。
- 测试：Core.Tests 495/495（含 `TokenWriteRecordingTests`），Generator.Tests 1095/1095。
- 结构检查（`DOTNET_TieredCompilation=0`，JIT 摘要与反汇编，方法同第一次测量；文件 `<S>/task10/disasm/summary-measure-A.txt`、`nowA-jit.txt`）：

| 项目 | BASE | 改造原样 | 补救 A |
| --- | --- | --- | --- |
| `DesignToken.CalculateAliasTokenValues` 机器码（字节） | 2,524 | 14,264 | 7,920 |
| `StepsToken.CalculateTokenValues` | 1,104 | 11,628 | 5,016 |
| `NavMenuToken.CalculateTokenValues` | 1,340 | 10,036 | 4,420 |
| `ToolTipToken.CalculateTokenValues` | 100 | 1,048 | 392 |
| `AbstractThemeVariantCalculator.CalculateColorMapTokenValues` | 772 | 10,308 | 3,456 |
| `GeneratedGlobalTokenAccessor.SetValue` | 26,924 | 39,980 | 38,040 |
| `GeneratedThemeSchema.CopyGlobalValues` | 10,968 | 16,900 | 17,208 |
| 两棵树都编译的 AtomUI 方法，机器码合计增量 | — | +239,552（354 个方法） | +86,856（345 个方法） |
| 被单独编译的 Token setter 个数 | 44 | 208 | 138（全是 `DesignToken`） |

  - 反汇编：`DesignToken.set_ControlOutlineWidth`（64 字节）是 `str d0,[x0,#0xF8]; ldr x1,[x0,#8]; cbnz x1, →`，未记账时直接返回，记账时尾调用 `AddRecordedWrite`；仍有一对 `stp/ldp fp,lr` 的帧建立。`ToolTipToken.CalculateTokenValues`（控件包）中每次写入后是 `ldr x0,[x19,#8]; cbz x0`，只在跳转分支里有一次 `blr`（约 36 字节的调用桩），与预期一致。`set_ColorPrimaryActive` 这次没有被单独编译。
  - 剩余的膨胀来自每个写入点的调用桩；`CopyGlobalValues` 和 `GeneratedGlobalTokenAccessor.SetValue` 这两个大方法几乎没有缩小，被单独编译的 setter 仍比 BASE 多 94 个。
- 第二次时间测量（G1–G5）：命令、脚本、采样数与第一次相同，数据在 `<S>/task10/runs-b`。BASE 用同一份构建，NOW 重新构建。14 次运行开始时的 1 分钟负载为 5.10–11.30（BASE 8.54/7.76/8.73/5.10/11.30/7.43/8.76，NOW 6.24/7.96/6.95/7.74/7.69/6.22/8.06），都低于 15，整批保留。统计量由 `r2_verdict.py` 只按本批 BASE 的波动选定：

| 指标 | BASE 波动（中位数 / P25） | 门槛统计量 |
| --- | --- | --- |
| 冷编译 Default | 2.76% / 2.39% | P25 |
| 冷编译 Dark | 7.19% / 3.00% | P25 |
| 冷编译 Compact | 4.38% / 2.31% | P25 |
| 切换往返 uncached | 1.85% / 2.25% | 中位数 |
| 切换往返 cached | 5.24% / 4.84% | P25 |

| 指标 | BASE 七次 | BASE 范围 / 中位数 / 波动 | NOW 七次 | NOW 范围 / 中位数 / 波动 | NOW − BASE | 判定 | 分辨率 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| G1 冷编译 Default（P25） | 0.6975, 0.6850, 0.6882, 0.6909, 0.6928, 0.6854, 0.7015 | 0.6850–0.7015 / 0.6909 / 0.0165（2.39%） | 0.7209, 0.7087, 0.7050, 0.7148, 0.7069, 0.7120, 0.7032 | 0.7032–0.7209 / 0.7087 / 0.0177（2.50%） | +0.0178（+2.58%） | **不过**：范围不重叠，差值 ≥ 较大波动 | 2.56%，分辨不出 2% |
| G2 冷编译 Dark（P25） | 0.7061, 0.6910, 0.6993, 0.7005, 0.6920, 0.7000, 0.7120 | 0.6910–0.7120 / 0.7000 / 0.0210（3.00%） | 0.7250, 0.7162, 0.7154, 0.7253, 0.7136, 0.7212, 0.7125 | 0.7125–0.7253 / 0.7162 / 0.0128（1.79%） | +0.0162（+2.31%） | **不过**：范围不重叠（差值小于较大波动 0.0210，但重叠条件不满足） | 3.00% |
| G3 冷编译 Compact（P25） | 0.6990, 0.6868, 0.6916, 0.6937, 0.6864, 0.6955, 0.7024 | 0.6864–0.7024 / 0.6937 / 0.0160（2.31%） | 0.7251, 0.7127, 0.7092, 0.7153, 0.7126, 0.7159, 0.7047 | 0.7047–0.7251 / 0.7127 / 0.0204（2.86%） | +0.0190（+2.74%） | **不过**：范围不重叠 | 2.94% |
| G4 切换往返 uncached（中位数） | 12.0782, 12.0646, 11.9890, 12.0831, 12.0999, 12.1815, 11.9575 | 11.9575–12.1815 / 12.0782 / 0.2240（1.85%） | 12.1774, 12.1495, 12.2238, 11.9864, 12.1119, 12.1717, 12.0452 | 11.9864–12.2238 / 12.1495 / 0.2374（1.95%） | +0.0713（+0.59%） | 通过：在噪声内（较大波动 < 2%，差值 ≤ 2%） | 1.97%，能分辨 2% |
| G5 切换往返 cached（P25） | 10.1889, 10.2615, 10.3795, 10.2175, 10.2334, 10.6849, 10.2576 | 10.1889–10.6849 / 10.2576 / 0.4960（4.84%） | 10.3610, 10.2657, 10.3065, 10.2010, 10.2665, 10.3341, 10.2283 | 10.2010–10.3610 / 10.2665 / 0.1600（1.56%） | +0.0089（+0.09%） | 通过：在噪声内 | 4.84%（BASE 第 6 轮 10.68 拉大了波动；该轮开始负载 7.43，按事先约定不算受干扰） |

  另一种统计量（不作门槛）：冷编译中位数 Default +3.52%、Dark +3.42%、Compact +3.65%；uncached 切换 P25 +2.08%；cached 切换中位数 +1.66%；`defaultMemoised` P25 +2.68%（范围重叠，BASE 波动 7.94%）。「首次编译」（含 JIT）BASE 619.7 ms、NOW 630.2 ms（+1.7%，第一次测量为 +15.6%）。

- G6 分配：三条链（及 `defaultMemoised`）增量仍是 +1,872 字节，七次运行一致；Token 实例数 234 不变（写入计数副本中再次核对为 234），预算 1,872。**通过**。
- G8 启动：NOW 重新发布到 `<S>/task10/startup-now`（改造原样的发布移到 `startup-now-preremedy`），BASE 沿用同一份发布。两个场景交替各 3 次、每次 10 轮，数据在 `<S>/task10/startup-A`。开始前等 1 分钟负载降到 10 以下；6 次运行开始时负载为 8.97–12.55，结束时最高 13.82，未丢弃。

| 场景 / 指标 | BASE 三次（ms） | BASE 中位数 / 波动 | NOW 三次（ms） | NOW 中位数 | NOW − BASE | 落在 BASE 范围内 | 配对 `medianDelta` |
| --- | --- | --- | --- | --- | --- | --- | --- |
| window `Launch.ToWindowOpened` | 690.5, 691.9, 694.1 | 691.9 / 3.5 | 680.8, 657.9, 665.1 | 665.1 | −26.8（−3.88%） | 否（更快一侧） | −6.4, −25.0, −18.1 |
| window `Launch.ToFirstRenderedBatch` | 791.1, 795.5, 792.7 | 792.7 / 4.4 | 777.0, 753.2, 764.1 | 764.1 | −28.6（−3.61%） | 否（更快一侧） | −6.0, −27.8, −18.7 |
| window `Phase.Commit` | 98.3, 97.7, 98.3 | 98.3 / 0.6 | 97.4, 96.3, 97.8 | 97.4 | −0.9 | 否（更快一侧） | −1.4, −1.5, −1.0 |
| window `Phase.Commit.Theme` | 52.3, 52.0, 52.6 | 52.3 / 0.6 | 51.7, 50.7, 51.9 | 51.7 | −0.6 | 否（更快一侧） | −0.9, −1.0, −0.5 |
| window `Phase.Prepare` | 75.03, 75.20, 74.82 | 75.03 / 0.38 | 75.20, 74.59, 75.78 | 75.20 | +0.17 | 否（比 BASE 最大值高 0.005 ms） | +0.4, −0.6, +1.2 |
| splash `Launch.ToSplashFirstFrame` | 665.0, 657.6, 669.5 | 665.0 / 11.9 | 669.4, 666.5, 653.8 | 666.5 | +1.5 | 是 | +4.4, +19.8, −23.2 |
| splash `Launch.ToFirstRenderedBatch` | 974.3, 961.3, 984.4 | 974.3 / 23.1 | 975.8, 978.7, 954.3 | 975.8 | +1.5 | 是 | −0.3, +4.0, −28.6 |
| splash `Launch.ToReady` | 974.6, 975.7, 987.4 | 975.7 / 12.7 | 975.7, 981.4, 963.0 | 975.7 | −0.1 | 是 | −0.1, +12.1, −34.1 |
| splash `Phase.Commit` | 80.72, 81.04, 80.20 | 80.72 / 0.84 | 82.06, 81.21, 80.89 | 81.21 | +0.49（+0.60%） | 否（高出 0.17 ms；差值小于 BASE 波动） | +1.7, −0.6, +0.7 |
| splash `Phase.Commit.Theme` | 50.0, 50.3, 49.4 | 50.0 / 0.9 | 50.3, 50.4, 49.7 | 50.3 | +0.2 | 是 | +0.4, −0.0, +0.4 |
| splash `Phase.Prepare` | 70.2, 70.3, 71.4 | 70.3 / 1.2 | 71.0, 71.2, 70.8 | 71.0 | +0.8（+1.08%） | 是 | +0.9, +1.0, −0.7 |

  `JitMethods.Phase.Prepare` 1654 → 1655，`JitMethods.Phase.Commit.Theme` 725 → 725。判定：**按字面规则（NOW 中位数落在 BASE 范围内）不过**。window 场景 BASE 三次异常集中（波动 0.5%），NOW 在 `Launch.*` 和两个 Commit 分段上落在更快一侧的范围外。变慢一侧落在范围外的只有两项：window `Phase.Prepare` 高 0.005 ms，splash `Phase.Commit` 高 0.17 ms，后者的差值 +0.49 ms 小于 BASE 波动 0.84 ms。第一次测量中 `Phase.Prepare` 在两个场景都明显变慢（+1.7/+1.1 ms），这次是 +0.17/+0.8 ms，都在或几乎在 BASE 范围内。
- G9 体积：程序集大小与第一次测量相同（只比较了文件大小，没有逐字节比较；Core 1,125,888，−0.14%；Controls +0.11%；Desktop.Controls −0.09%；DataGrid −0.05%；ColorPicker 0）。Gallery NativeAOT 可执行文件 93,237,496（BASE 93,121,880，+115,616，+0.12%），发布目录 113,419,784（+0.10%）。**通过**。
- G10 值哈希：两棵树七次全部相同，与基线一致。**通过**。
- G7：补救只改了 `AbstractDesignToken`，没有改性能测试；Core.Tests 495/495。**通过**。

**一次冷编译的 Token 属性写入次数与来源**（只在临时副本 `<S>/task10/count-now` 中计数，脚本 `<S>/task10/scripts/patch_writes.py`：在 `RecordWrite` 里按「当前类别」累加计数，在 `ThemeCompiler` 的各调用点用作用域设定类别，读数区间与分配、计时相同；21 个样本完全一致。副本基于改造原样的代码，写入次数与补救 A 相同，因为补救没有改变哪些属性被写）：

| 来源 | Default | Dark | Compact |
| --- | --- | --- | --- |
| (a) 全局 Token 复制 `GeneratedThemeSchema.CopyGlobalValues`（117 次复制 × 255 个属性：算法 1 次 + 116 个控件各 1 份有效全局副本） | 29,835 | 29,835 | 29,835 |
| (a′) 非规范 Schema 的逐项复制、从冻结表重建（`CreateBuilder`） | 0 | 0 | 0 |
| (b) 算法求值（`Evaluate`，种子 → Map） | 121 | 202 | 154 |
| (b) 默认 Alias 计算（`CalculateAliasTokenValues`） | 118 | 118 | 118 |
| (b) `new DesignToken()` 构造期写入 | 8 | 8 | 8 |
| (b) `NormalizeFinalBehavior` | 0 | 0 | 0 |
| (c) 控件 Token 默认公式（`descriptor.Evaluate`） | 1,515 | 1,515 | 1,515 |
| (d) 显式值写入（`ApplyValues`、`MarkExplicit`、`ApplyAllValues`；测试应用只有默认字体） | 2 | 2 | 2 |
| 未归类（以上作用域之外） | 6 | 6 | 6 |
| 合计 | 31,605 | 31,686 | 31,638 |

剩余差异约 0.018 ms，按 31,600 次写入摊算约 0.56 ns/次。94% 的写入来自 (a) 全局复制。`CopyGlobalValues` 的机器码在补救 A 后反而从 16,900 增至 17,208 字节（BASE 10,968），所以如果还要再改，目标应该是全局复制路径，而不是 setter 本身。本任务按裁定没有尝试任何进一步的补救。

**第二次测量门槛汇总（补救 A）**

| 门槛 | 结果 |
| --- | --- |
| G1 冷编译 Default | **不过**：+2.58%（P25），范围不重叠；分辨率 2.56% |
| G2 冷编译 Dark | **不过**：+2.31%，范围不重叠；分辨率 3.00% |
| G3 冷编译 Compact | **不过**：+2.74%，范围不重叠；分辨率 2.94% |
| G4 切换往返 uncached | 通过：+0.59%（中位数）；分辨率 1.97% |
| G5 切换往返 cached | 通过：+0.09%（P25）；分辨率 4.84% |
| G6 分配 | 通过：+1,872 = 234 × 8 |
| G7 既有性能测试 | 通过：Core.Tests 495/495 |
| G8 启动 | **按字面规则不过**：window 的 `Launch.*` 与两个 Commit 分段落在更快一侧的范围外；变慢一侧只有 window `Phase.Prepare`（+0.005 ms 越界）与 splash `Phase.Commit`（+0.17 ms 越界，差值小于 BASE 波动） |
| G9 体积 | 通过：程序集 −0.14%～+0.11%，AOT +0.12% |
| G10 值哈希 | 通过 |

- 全量回归（补救 A 之后）：`[SUMMARY] TOTAL: 12405 个测试全部通过, 总耗时 15:35`，与 Task 9 相同；Desktop.Controls.Tests 为 passed=4397 skipped=1（跳过的是显式测量）。
- 工作树：生产代码只有补救 A 的这一处改动；计数器只存在于临时副本里；`git status` 显示 `AbstractDesignToken.cs`、本台账，以及本任务开始前就已存在的计划文件改动。

#### 用户裁决（2026-11-13）

- 主控在补救 A 之后仍未通过门槛时停止执行，把两次测量的数字、写入次数的分布和三条可选的路（把全局复制从 setter 上拿开后重测；退回写入器方案；接受当前差距）交给用户，并应用户要求补充了启动耗时的逐项数字。
- 用户的决定：**通过**。保留补救 A 的实现，继续执行后续任务。
- 这是对规格 4.11「不放宽门槛」的一次明确放宽，由用户作出；门槛本身没有达到。放宽的范围是这次没过的两项：
  - G1–G3 冷编译：按门槛统计量（25 分位）慢 2.3%–2.7%，约 0.018 毫秒；按中位数慢 3.52%、3.42%、3.65%（默认、暗色、紧凑；这组数字原本就在上文「另一种统计量」一行里）。按 25 分位数两棵树各轮的取值范围不重叠；按中位数两边范围有重叠。
  - G8 启动：按字面规则不过。越界大多在更快一侧；变慢一侧在登记的 11 个指标里有两项（window `Phase.Prepare` 高出 BASE 最大值 0.005 ms；splash `Phase.Commit` 高出 BASE 最大值 0.17 ms，差值 0.49 ms 小于 BASE 波动 0.84 ms）。未登记的分段指标里，splash `Phase.Commit.Localization` 也高出 BASE 最大值（10.579 对 10.454，差值 +0.279 大于 BASE 波动 0.166），它与 Token 无关，很可能是噪声，但前文「变慢一侧只有两项」只对登记的指标成立。
- 用户作决定时看到的是冷编译的 25 分位数字和启动的逐项数字。中位数的数字当时已在台账里，但主控没有一并交给用户；审阅指出后，主控随即告知了用户。
- 规格 4.11 末尾和决策表第 17 条已同步登记。
- 新的性能基线：本任务提交后的树（含补救 A）。后续任务（Task 11、14、23 的性能复测）与它对比，方法和判定规则不变，不得在此基础上再退化。
- 未采用的改进记录在案：把 `CopyGlobalValues` 的逐属性复制改成整体克隆，可让约 94% 的写入不再经过 setter。未实测，是否单独立项由用户决定。

### Task 11：Alias 阶段契约与编译器三步流程（未提交，待审阅）

- 改动：
  1. 新增公开类型 `IThemeAliasAlgorithm`、`ThemeAlgorithmContext`（只读结构体，内部构造函数；`default` 的 `Algorithms` 返回空数组）、`ThemeAlgorithmContractException`（`InvalidOperationException` 子类，带 `Algorithm` 与 `TokenName`），位于 `src/AtomUI.Core/Theme/Algorithms/`。
  2. 新增 `src/AtomUI.Core/Theme/Compilation/StagedTokenEvaluation.cs`：Alias 阶段与 Task 14 的 Own Token 阶段共用的三步流程执行器。每个算法单独开关一次记账，`EndRecordingWrites` 放在 `finally` 里；记下的名称逐个解析为描述符并检查阶段；第 3 步先标记算法写过的槽位、再写一次用户显式值、重跑默认计算、再跑各算法、最后写用户显式值；与上一轮比较写入的槽位集合与值（先 `Equals`，否则比较 `Format` 文本），不同则并入新槽位再来一轮，第 3 轮仍不同则抛「未稳定」。标记在所有出口（含异常）都清除。三条异常文本照简报逐字。
  3. `ThemeCompiler`：`EvaluateAlgorithms` 增加 `out List<AliasStageAlgorithm>?`，复用 Map 阶段创建的实例，实现了 `IThemeAliasAlgorithm` 的才收进列表，列表按需创建；`CalculateAlias` 在列表为空引用时执行与之前逐句相同的四步并返回，否则进入单独的 `RunAliasStage`（闭包只在这里创建，避免闭包类在 `CalculateAlias` 入口就分配）。`ReevaluateControlGlobal` 走同一条路，新增 `globalAppearance` 参数用于算出上下文外观（与 `CompileControl` 里 `ResolveAppearance(globalAppearance, chain)` 一致）。按名称查描述符用以全局描述符列表为键的 `ConditionalWeakTable` + `FrozenDictionary`，只在第一次运行 Alias 阶段时建立。
  4. `AbstractDesignToken.BeginRecordingWrites` 的 summary 补上「必须在 `finally` 里调用 `EndRecordingWrites`」的契约（Task 8 审阅遗留）。
  5. 新增 `tests/AtomUI.Core.Tests/Theme/ThemeAliasAlgorithmTests.cs`，简报列出的 12 个测试，名称逐字。
- 与简报的出入：
  1. `StagedTokenEvaluation.Run` 在 `token` 之后多一个 `string stage` 参数。简报的签名里没有阶段名，但三条异常文本都含 `{stage}`，Task 14 的 Own Token 阶段需要不同的名字。
  2. `BuiltIn_Chains_Create_No_Extra_Objects_And_Run_The_Default_Once` 不引用 Task 7 的字节数，改为两条自洽的断言（逐个检查 Default、Dark、Compact）：① 结构：内置算法的 Map 阶段返回的 Alias 算法列表为 `null`；② 分配：Alias 阶段入口在这条路径上的分配字节数（40 次交替采样取最小值）必须等于直接调用一次 `CalculateAliasTokenValues` 的字节数，且后者大于 0。为确认它真的会失败，临时做了两次变异：在内置路径上多跑一遍公式（312 → 624，失败）、多分配一个数组（312 → 352，失败），随后恢复原文件（`cmp` 一致）。没有采用「再注册一个无 Alias 阶段的算法后比较分配」：两次编译走同一条路径，内置路径多了分配也会两边一样多，测不出来。整个编译的分配不变由下面的测量证明。
  3. 违反契约的报告方式：规格 4.2/4.3 写的是「报编译诊断」，按主控说明改为异常走现有的失败通道。全局编译里异常被 `CompileCore` 捕获，原样放进 `ThemeCompileResult.Exception`，`ThemeManager` 等调用方再把它作为 `ThemeLoadException` 的 `InnerException`；控件编译是惰性的，异常在查询该控件快照时原样抛出，与其他控件编译失败相同。
- 内置路径核对（读代码）：Default 链编译现在比之前多的只有 `EvaluateAlgorithms` 里每个算法一次 `is IThemeAliasAlgorithm` 类型判断和 `CalculateAlias` 入口一次空引用判断；不创建对象、不打开记账、不查名称、公式只跑一遍。
- 测量（Task 7 的测量工具，`DOTNET_TieredCompilation=0`，冷编译 21 次 / 预热 10 次，切换 3 / 1 次，Release；只比较分配与哈希，不看耗时）。改动前（HEAD `3a87bba25` 原样）与改动后逐字相同（`diff` 无输出）：

  | 链 | 冷编译分配中位数（前 → 后） | 值哈希（前后相同） |
  | --- | --- | --- |
  | Default | 1,138,032 → 1,138,032 | `80B8BAC59221D9A22EAD273E73F9A2E595F51B53322121576D311C9293875311` |
  | Dark | 1,143,224 → 1,143,224 | `7CE1950AA51CA44B2177A39F35CFA000D9AF2D7C1E9E86ED10FDC4BFD2FE080A` |
  | Compact | 1,138,416 → 1,138,416 | `81DBB30DA0BCB51D7BC1AD40698E1714D4411EA320584472C8FB963BCDA20A34` |

  最小值、最大值也前后相同（最大值比中位数多 528 字节，前后一样）；`defaultMemoised` 为 1,121,784，前后相同。三个哈希与 Task 7 / Task 10 登记的基线一致。
- 测试：TDD 先红后绿（实现前 10 个失败，失败原因均为算法的 Alias 阶段没有运行；`Explicit_Theme_Value_Wins_Over_The_Alias_Stage` 与 `BuiltIn_…` 在实现前即通过，前者因为算法不运行时结果本来就是显式值，后者钉的就是原有路径）。`AtomUI.Core.Tests` 507/507，`AtomUI.Themes.Tests` 27/27，全量回归 `[SUMMARY] TOTAL: 12417 个测试全部通过, 总耗时 16:20`（12405 + 本任务新增 12）。
- 审阅结论：规格符合，质量通过，无严重或重要问题。Task 10 的提交为 `17bc167f5`（冷路径移出内联）与 `3a87bba25`（验收记录与放行裁决）。审阅逐行对照规格 4.3 核对了三步流程：
  - 优先级的四种情形（只有算法写；算法与主题显式值都写；主题显式值带动派生值；算法写带守卫的字段并带动派生值）结果都符合规格。为了让第二种情形里派生值读到显式值，实现在重跑默认公式之前多写了一次显式值；规格文字里没有这一步，但没有它结果就不对，已有测试钉住。
  - 显式标记在所有退出路径上都会清除（含提前返回、不稳定抛错、算法自身抛错）；第二遍默认计算用的是「用户显式」与「算法写入」的并集。
  - 记账只在算法阶段运行期间打开，在 `finally` 里关闭；默认公式和显式值写入都在记账关闭时进行。
  - 稳定性检查同时比较槽位和值，双向（新增或变化的写入、消失的写入）；上限 3 轮；异常里带算法身份和 Token 名。
  - 记账按链上每个算法单独进行，违规能归到正确的算法；后面的算法覆盖前面的。
  - 内置路径相比改动前只多了「每个算法一次类型判断」和「一次空判断」，不分配对象、不记账、不查名称；委托放在单独的方法里，没有算法阶段时不会分配闭包。
- 裁决（审阅提出的「无法从改动判断」一项）：控件 `Global` 与 `Custom` 模式下，根主题显式设置的 Map 与 Alias 值不参与该控件全局 Token 的重新推导，这是本方案之前就有的行为（见 Task 5 的记录）。实现 Alias 阶段后，这些值在该控件作用域里也可能被控件链上算法的 Alias 阶段取代，与「算法 < 主题显式值」的总顺序不一致。本任务不改这个行为 — 它不在计划范围内，改动会改变现有主题在这两种模式下的编译结果；主控记得 Ant Design 的组件级算法也是只保留组件自己的覆盖，但这一点没有核实 — 判断错了的代价是这两种模式下根显式值在控件作用域内不生效。规格 4.3 已补充说明这个例外，留给用户决定是否单独立项。
- 留待最终整体审阅统一处理的小项：
  1. 标记只增不减：算法第一轮写了 {A, W}、之后只写 {A} 时，带守卫的 W 会保留第一轮的值，结果不是「默认公式加最后一次算法写入」的不动点。要么每轮按「用户标记加本轮写入」重置，要么在 `Run` 的注释里写明并集语义。
  2. `ThemeAlgorithmContext.Algorithms` 暴露的是各步骤共用的数组，算法强转回数组就能改掉后续算法看到的内容；包一层只读视图代价很小。
  3. 测试缺口：成功或抛错后标记已清除没有直接断言；控件延迟编译路径上的失败如何抛出没有测试；根链带 Alias 算法时的 `ControlAlgorithmMode.Global` 没有测试（规格 4.6 要求四种模式）。
  4. 内置路径测试的分配断言只覆盖 `CalculateAlias` 入口，整次编译的分配只由一次性的测量覆盖。

### Task 12：可复用的算法契约测试（未提交，待审阅）

- 改动：
  1. 新建测试支持类库 `tests/AtomUI.Theme.ContractTests`（net10.0，不打包，不含测试用例，只引用 `AtomUI.Core`；`src/` 下无人引用）。加入 `AtomUI.slnx`；`AtomUI.Core` 增加 `InternalsVisibleTo AtomUI.Theme.ContractTests`；`AtomUI.Core.Tests` 引用它。
  2. `ThemeAlgorithmContract.cs`：`ThemeAlgorithmContractViolation`、`ThemeAlgorithmContractInput`（含 `Capture`）、`ThemeAlgorithmContract.Verify`，形状与简报一致；12 个检查名以公开常量给出，与规格 4.6 一一对应。`MapTokenSamples` 集中定义 Map 阶段 5 种值类型（Color、double、CornerRadius、Thickness、TimeSpan）各两个样本，遇到其他类型抛异常并点名类型和 Token。
  3. `ContrastRatio.cs`：WCAG 相对亮度与对比度，半透明前景先与背景合成；阈值常量 4.5 与 3。
  4. 测试：`BuiltInAlgorithmContractTests`（三算法，Theory）与 `ThemeAlgorithmContractSelfTests`（简报 9 个，名称逐字，另加 4 个）。
- 做法：Map 写满、确定性、不改输入直接跑 Map 阶段（跟随型算法以 `AtomUI:Default` 的 Map 结果为 `previousMap`）；其余检查经真实编译器编译。`Stable`、`StageBoundary` 不重写规则：编译结果里的 `ThemeAlgorithmContractException` 按 Token 所属阶段归类（Alias 阶段 Token 只可能是「未稳定」，否则为越界）。不属于契约违例的编译失败（包括 Token 名为空的「阶段内抛错」）直接抛 `InvalidOperationException` 中止校验。
- `Capture` 收集：Core 三个内置算法 + 注册经 `AddThemeAlgorithms` 加入的算法 + 经 `IThemeManagerBuilder.AddControlPackage` 加入的控件描述符。不收集经 `IAtomUIBuilder` 启用的控件包，调用方用 `input with { Controls = … }` 补；只构造 builder、不 Build，不需要 Avalonia 应用。
- 内置三算法结论：全部检查通过，没有登记的违例。Default 单独运行；Dark、Compact 接在 Default 后运行、跳过 Map 写满。对比度：Default 正文 16.48、实色上文字 4.10；Dark 13.31、5.19；Compact 在 `[Default, Compact]` 上同 Default，混合链 `[Default, Compact, Dark]` 上为 13.31、5.19（审阅修复一轮后）。不适用项：Compact 的外观一致（Preserve）、可组合（自身就是 Compact）；Dark 的混合链（链里已有 Default，同一链不得重复身份，`[Default, Dark]` 本身就是浅色基底后接暗色算法，由可读性检查覆盖）。
- 与简报的出入：
  1. 多加 4 个自测（Identity、Stable、ComposableWithCompact、HybridChainReadable），让 12 个检查每个都有失败用例。
  2. 混合链：浅色与 Preserve 算法后接 `AtomUI:Dark`（Preserve 的被验链是浅色，规格 4.2 要求 Alias 阶段处理明暗两种外观）；暗色算法后接 `AtomUI:Default`（规格未写，属解释）。
  3. 「控件高度」取 `ControlHeight`（Seed 阶段，Compact 会写）与 SM/LG/XS；「颜色类 Token」取类型为 `Color`/`Color?` 的全部全局 Token。
  4. 规格 4.6 中涉及 `EvaluateOwn` 的两项属于控件阶段（Task 14 起），本任务未覆盖；`Controls` 已随输入进入注册表，留给后续扩展。
  5. 「不支持的值类型」异常没有测试（需要自定义全局 Schema）。
- 测试：TDD 先红后绿（类库先放空实现，13 个自测失败，3 个内置用例对空实现必然通过）。`AtomUI.Core.Tests` 523/523（507 + 16）。整个解决方案构建 0 错误，警告只有离线环境的 NU1900。全量回归 `[SUMMARY] TOTAL: 存在失败, 总耗时 15:45`：共 12433 个（12417 + 16），1 个失败在 `AtomUI.Desktop.Controls.Tests` 的 `ImagePreviewerImageLoadingTests.Detach_Releases_Entries_Unsubscribes_Collection_And_Reattach_Reconciles_Changes`，是 Avalonia Headless 建立测试会话时的跨线程异常（Test Case Cleanup Failure），与本任务无关；单独重跑 ImagePreviewer 119/119、整个项目 4397/4397 通过。按规则全量回归不记为通过。主控在暂存树上重跑全量回归：12433 个全部通过，该失败为偶发。
- 审阅修复一轮（审阅结论「需修复」，校验器本身判定可靠）：
  1. 重要：Preserve 算法从未在暗色外观下检查可读性。改为 Preserve 也后接 `AtomUI:Dark`（链里已有 Dark 时跳过）。新增自测 `HybridChainReadable_Reports_A_Preserve_Algorithm_That_Is_Unreadable_Under_Dark`（跟随型 Preserve 算法的 Alias 阶段不看外观、写深灰 `ColorTextLightSolid`：浅色主色上约 3.2，暗色主色上约 2.5），修复前失败、修复后通过。内置 Compact 现在实际编译 `[Default, Compact, Dark]`，仍通过（正文 13.31，实色上文字 5.19）。
  2. 次要：Dark 跟随型算法跳过混合链的豁免写进 `HybridChainReadable` 与 `Verify` 的 XML 文档；`Verify(Default, standalone: false)` 的参数检查移到 `Verify` 开头；对比度自测里的注释由「#808080」更正为「#7F7F7F」。
  3. `AtomUI.Core.Tests` 524/524（523 + 1）。只改了测试支持库与 Core 测试，未重跑全量回归。
- 审阅结论：首轮审阅认为校验本身可靠（12 项检查逐一证明会因目标原因失败、在默认算法上不误报；对比度公式用已知颜色对核对无误；「稳定性」与「阶段越界」走真实编译器），指出一个重要问题后退回修正一轮；复审结论为全部解决，无新的严重或重要问题。Task 11 的提交为 `fde5ffe7d`。
  - 重要问题：外观效果为 Preserve 的算法从不在暗色外观下接受可读性检查，带颜色写入的第三方 Preserve 算法可以不经暗色检查就通过契约。已修：混合链检查对 Light 与 Preserve 算法都在链尾接 `AtomUI:Dark`，对 Dark 算法接 `AtomUI:Default`，只有那个内置算法已在链里时才跳过，不按身份或类别豁免。
  - 内置 Compact 现在确实按 `[Default, Compact, Dark]` 编译并检查：正文 13.31、实色上的文字 5.19，通过。
- 全量回归：实现者那一次有 1 个失败（`ImagePreviewerImageLoadingTests.Detach_Releases_Entries_Unsubscribes_Collection_And_Reattach_Reconciles_Changes`，Avalonia Headless 建立测试会话时的跨线程异常），该项目重跑 4397/4397。主控在同一棵树上重跑完整的全量回归：`[SUMMARY] TOTAL: 12433 个测试全部通过, 总耗时 16:20`。判定为偶发，与本任务无关（出错的测试项目不引用新库，测试程序集各在独立进程运行）。这是执行以来第一次出现偶发失败，记录在案。
- 带入 Task 14 的一项：契约校验靠 Token 所在的阶段区分「不稳定」与「阶段越界」，因为 `ThemeAlgorithmContractException` 没有类别字段。只有 Alias 阶段时这样分是对的；控件阶段加入后，不稳定的 Own Token 或 `EvaluateGlobal` 写出的不稳定 Map Token 会被错标为越界。Task 14 给异常加明确的类别。
- 带入 Task 17 审阅的一项：`standalone` 由调用方声明，不与描述符绑定。PrimeNG 的六个算法都可单独使用，必须用 `standalone: true` 校验，否则「Map 写满」会被悄悄跳过。
- 留待最终整体审阅统一处理的小项：
  1. 「后接 Compact」检查里的颜色只统计 `Color` 类型的槽位，阴影、画刷里的颜色不比较；高度那一半对「排在 Compact 之前」的算法实际不可能失败。
  2. 身份检查只测了重复注册；非法 `Catalog`/`Id` 和 `Revision <= 0` 的分支没有自测（构造函数已校验，可能本就走不到）。未支持的 Map 值类型抛错的分支没有自测。
  3. 「不改输入」不比较 `ColorPalettes`。
  4. 新增的 Preserve 自测只断言目标违例存在、`Readable` 不存在，没有断言列表里没有别的违例。

### 阶段四小结（Task 11–12）

算法的 Alias 阶段和编译器的三步求值流程已就位，并有一套可复用的契约校验，内置三算法全部通过。内置主题的编译结果和分配字节数与阶段三结束时完全相同。全量回归增至 12434（含 1 个只在显式指定时运行的测量）。

### Task 13：控件阶段的契约、声明、生成与注册（未提交，待审阅）

- 改动（Core）：
  1. 新增公开类型 `IControlTokenAlgorithm<TToken>`、`ControlTokenAlgorithmAttribute`（`AllowMultiple`，主构造函数 `(catalog, id, revision)`）、`IControlToken`（静态抽象 `ControlIdentity`）、`ControlTokenAlgorithmDescriptor`。`Create<TToken, TAlgorithm>` 只用泛型约束：控件身份取 `TToken.ControlIdentity`，持有一个 `new TAlgorithm()` 实例，经内部泛型 `Evaluator<TToken>` 调用两个阶段，不用反射，也不用 `Activator`；`EvaluateOwn` 收到类型不符的 Token 时抛 `ArgumentException`。Core 与控件包的目标框架是 net10.0/net8.0，都支持静态抽象接口成员。
  2. `IThemeManagerBuilder.AddControlTokenAlgorithms`；`ThemeManagerBuilder` 按引用保存描述符，`BuildCore` 交给 `ThemeManager.SetPackageControlAlgorithms`，`CreateStartupRegistry` 的三条路径（启动索引、全部带编译期指纹、逐字段）都传入。
  3. `ThemeSchemaRegistry`：三个构造函数各加一个带 `controlAlgorithms` 的重载（原签名保留，见出入 2）；按（算法身份，控件身份）序数排序；重复时抛 `Duplicate control algorithm '{algorithm}' for control '{control}'.`；新增 `ControlAlgorithms` 与 `TryGetControlAlgorithm`。算法或控件未注册的实现照样保留，只是编译器（Task 14）永远查不到它的组合。
  4. 修订号：在三条路径各自算出的修订号之后，`FoldControlAlgorithms` 用现有的 `SchemaFingerprintBuilder` 依次计入 `"AtomUI.ControlAlgorithmRevision"`、版本 1、原修订号、条数，以及排序后每条的算法 `Catalog`/`Id`、控件 `Catalog`/`Id`、`Revision`。没有登记任何控件阶段时直接返回原值，不新建对象（空表用 `Array.Empty` 和 `FrozenDictionary.Empty`）。
  5. 运行时追加包（`PreparePackages`）：新注册表沿用已提交的控件阶段，并加上这次 builder 登记的。追加时的接受规则留给 Task 15。
- 改动（生成器）：
  1. `IControlToken` 的实现由 `TokenResourceKeyGenerator` 输出（与 `<Control>Tokens.Identity` 同一处，绑定事实就是 `controlInfos`），文件 `GeneratedControlTokenIdentities.g.cs`，显式实现 `static ControlTokenIdentity IControlToken.ControlIdentity => <ns>.DesignTokens.<Control>Tokens.Identity;`，Token 类的公开成员不变。输出条件：编译里能找到 `IControlToken`、终端类、绑定了控件、顶层、非泛型、非抽象、所有声明都是 `partial`、类自己没有写这个接口。构建后核对：各包 `GeneratedThemeSchema` 的控件 Token 工厂数与生成的实现数逐包相同（AtomUI.Controls 2、Desktop.Controls 110、ColorPicker 4、DataGrid 1、GalleryBase 8）。
  2. `[ControlTokenAlgorithm]` 并入同一个生成器的声明收集。`ControlTokenStageModel.Collect` 逐类检查（诊断都是 `ATOMUITOKEN002` 自由文本，位置在类名上）：非抽象、非泛型、程序集内可访问；恰好实现一次 `IControlTokenAlgorithm<T>`；有公开无参构造函数（`'{type}' must have a public parameterless constructor.`）；`T` 是绑定了控件的终端 Token（`'{type}' targets '{token}', which is not bound to a control.`）。判断依据：`T` 在当前编译里时看 `controlInfos` 的绑定结果（此时看不到生成器加的接口），来自引用程序集时看元数据里有没有 `IControlToken`。Attribute 的标识符或修订号不合法时在 Attribute 位置报错。
  3. `ThemeAlgorithmRegistrationWriter`：有算法或有控件阶段任一项就输出注册类；有算法才输出 `AddThemeAlgorithms` 和 `<Catalog>Algorithms`；有控件阶段时输出一个静态只读数组，每个 Attribute 一条 `ControlTokenAlgorithmDescriptor.Create<Token, Class>(new ThemeAlgorithm("…", "…"), n)`，按（算法，Token 类型，实现类）排序，`Register` 里调用 `builder.AddControlTokenAlgorithms(...)`。数组只建一次，同一进程里的多个 builder 登记的是同一批描述符实例。
- 测试：
  - `ControlTokenAlgorithmRegistryTests`（Core，4 个，名称逐字）。包括：经 builder 进入启动注册表后，拿到的仍是同一实例；描述符只持有一个算法实例；重复项的异常文本逐字比对，覆盖注册表与 `Build()` 两个入口；修订号与输入顺序无关，并随修订号、算法身份、条数变化；钉住一个跨进程常量 `0x3C27BB46DFCE54C8`。
  - 没有控件阶段时修订号不变：逐字段路径传 `null` 和传空表都等于既有常量 `0x29D0F9BA9FF24C7F`；组合路径传空表与不传结果相同；启动印记路径仍是 `0x1234`；有控件阶段时这三条路径的修订号都改变。
  - `ControlTokenAlgorithmGeneratorTests`（生成器，5 个，名称逐字）。同一编译与引用程序集两种情况都覆盖：引用程序集用真实生成器输出编译。另外覆盖：抽象中间层、未绑定的终端类、缺少 `IControlToken` 的旧运行时都不输出实现；同一编译里的 Token 加控件阶段编译无错，说明泛型约束确实由生成的那一半满足。
- 与简报的出入：
  1. 简报列的修改文件里有 `TokenPropertyImplementationWriter.cs`，实际没有改它：那个轻量生成器拿不到控件绑定结果，`IControlToken` 改由 `TokenResourceKeyGenerator` 输出，简报允许这样选。代价是这一半只在完整编译时可见，IDE 设计期看不到。手写代码不会直接用到这个接口（`IControlTokenAlgorithm<TToken>` 没有约束它），生成的注册类同样只在完整编译里出现。
  2. 注册表构造函数用重载而不是可选参数。第一版用可选参数，`TypeMapIntegrationTests` 的 6 个测试按原参数个数反射调用构造函数，因此失败；改成重载后通过。
  3. 改了简报未列出的 `ThemeManager.PackageLoading.cs`（追加包时沿用已提交的控件阶段）和 `tests/AtomUI.Desktop.Controls.Tests/Theme/ThemeInitialModeTests.cs`（测试替身补上接口新方法）。
  4. 「没有 Own Token 类的控件不支持控件阶段」写进了 `IControlTokenAlgorithm` 的 XML 文档。「编写主题算法」指南还不存在，按计划在 Task 23 新建时写入。
  5. 规格 4.2 要求把（算法，控件，revision）计入所有持久指纹和缓存键。本任务只做到注册表修订号；控件编译缓存键归 Task 14。
- 测量（Task 7 的测量工具，`DOTNET_TieredCompilation=0`，冷编译 21 次 / 预热 10 次，切换 3 / 1 次，Release；只比较分配与哈希）：改动前（HEAD `a9dc15d00` 原样）与改动后，全部分配与哈希行逐字相同（`diff` 无输出）。冷编译分配中位数 Default 1,138,032、Dark 1,143,224、Compact 1,138,416，`defaultMemoised` 1,121,784；三个哈希与 Task 7/10/11 登记的相同。
- 测试结果：`AtomUI.Core.Tests` 528/528（524 + 4），`AtomUI.Generator.Tests` 1100/1100（1095 + 5），`AtomUI.Themes.Tests` 27/27，整个解决方案构建 0 错误（警告只有 NU1900）。全量回归第一次 `[SUMMARY] TOTAL: 存在失败, 总耗时 16:10`，1 个失败：`MasonryShowCasePageTests.Masonry_Semantic_Part_Example_Applies_Styles_To_Item_Containers`，`MasonryPanel.OnDetachedFromVisualTree` 第 272 行空引用，与本任务无关；单独重跑 Masonry 15/15 通过。整套重跑一次：`[SUMMARY] TOTAL: 12443 个测试全部通过, 总耗时 16:11`（12434 + 本任务新增 9）。
- 审阅结论：规格符合，质量通过，无严重或重要问题。Task 12 的提交为 `a9dc15d00`。审阅确认：
  - 描述符的构造不用反射也不用 `Activator`，NativeAOT 安全；类型不符时抛出的异常指明控件、期望类型和实际类型。
  - 没有注册任何控件阶段实现时，注册表的修订号与此前逐位相同，测试用独立钉死的常量锚定（无条件折叠会让它失败）；查找在空表上直接返回，不计算哈希。
  - 每个绑定了控件的终端 Token 恰好实现一次 `IControlToken`；同一编译里的 Token 与来自引用程序集的 Token 分别判定，两种情形都有测试。
  - 运行时追加包在本任务之后不比之前更糟：重复注册在准备阶段抛错，追加的实现目前只会改变注册表修订号。
- 全量回归：实现者第一遍有 1 个失败（`MasonryShowCasePageTests.Masonry_Semantic_Part_Example_Applies_Styles_To_Item_Containers`，`MasonryPanel.OnDetachedFromVisualTree` 里的空引用），单独运行通过；第二遍 `[SUMMARY] TOTAL: 12443 个测试全部通过, 总耗时 16:11`。审阅从改动判断与本任务无关。这是执行以来第二次偶发失败，与上一次不是同一个测试；这一次的报错位置在产品代码里，可能是一个偶尔才触发的真实缺陷，不在本计划范围内，未调查，留给用户。
- 裁决（执行顺序）：先做 Task 15，再做 Task 14 — Task 13 之后追加包会带入控件阶段实现但尚无规格 4.4 的接受规则；Task 14 一旦让编译器调用这些实现，为已存在的算法和已存在的控件追加实现就会改变已提交的结果。Task 15 只依赖注册表，可以先做 — 代价是 Task 15 的测试要在编译器尚未消费控件阶段的情况下，从注册表层面断言。
- IDE 设计期的已知限制：`IControlToken` 的实现只在完整构建时生成。手写 `ControlTokenAlgorithmDescriptor.Create<某控件Token, X>` 且 Token 在同一项目（审阅认为同一解决方案里的项目引用也一样）时，IDE 会显示一个假的 CS0315 错误，构建是成功的。正常使用由生成器产出这行代码，不需要手写；后续任务里手写它的测试会在 IDE 里显示红色。
- 带入 Task 23 的一项：「没有 Own Token 类的控件不支持控件阶段」目前只写在 XML 注释里，指南要写明。
- 留待最终整体审阅统一处理的小项：
  1. 生成器对多数不规范写法（抽象或泛型类、带特性但没实现接口、为两种 Token 实现接口、非法标识符、修订号不大于 0）都会报 `ATOMUITOKEN002`，但只有两个分支有测试。
  2. 同一程序集内的重复（一个类带两个相同的特性，或两个类针对同一个算法和 Token）只在运行时报错，错误信息不指明类；生成器报诊断更好。
  3. 注册表现在有七个构造函数，只是为了让 `TypeMapIntegrationTests` 能继续用反射按旧的参数个数调用；改掉那六处调用后可以收成一个可选参数。
  4. 启动索引那个构造函数在有控件阶段实现时把修订号折叠了两次（只在启动时，开销可忽略）。
  5. 组合路径的修订号断言拿新重载和旧重载互比，两者走同一段折叠，不独立；另外两个锚点已覆盖。
  6. 引用的程序集如果是用旧版 Core 构建的，Token 会被报成「未绑定控件」，信息有误导性；同一文件里 `GetSyntax()` 没传取消标记。
  7. 描述符在静态只读字段的初始化里创建，某个控件阶段类的构造函数抛异常会让整个注册类不可用；可在特性的注释里写明。
