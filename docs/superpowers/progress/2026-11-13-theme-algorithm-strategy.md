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

## 阶段一小结

Task 1–4 完成：算法身份开放、XML 接受任意身份、包里的算法进入注册表、构建期校验未知算法。全量回归从基线 12317 增至 12360，全部通过。
- 审阅结论：规格符合，质量通过，无严重或重要问题。Task 4 的提交为 `ba3a6c32f`。审阅确认：
  - 守卫清单完整：通读 `CalculateAliasTokenValues` 全文，本方法内「先赋值、后面又被读取」的 Alias 字段只有已加守卫的 8 个，没有第 9 个。
  - 显式标记不会泄漏：计算所用的 Token 对象总是 `CopyBuilder` 新建的副本，方法抛异常时该对象随即不可达。
  - 没有显式 Alias 值时不分配位图，默认计算仍只跑一次。
- CHANGELOG：规格 4.5 要求在 CHANGELOG 里说明这次修复。按计划 CHANGELOG 由 Task 23 统一写，届时要列出上表受影响的派生关系（`ControlOutlineWidth`、`ColorControlOutline`、`ColorErrorOutline`、`ColorWarningOutline` → 三个 `Input*ActiveShadow`；`LineWidthFocus` → `FocusVisualBorderThickness`；`ControlPaddingHorizontal`、`ControlPaddingHorizontalSM`、`UniformlyPaddingSM` → `InputPadding*`）。
- 留待最终整体审阅统一处理的小项：
  1. `Without_Explicit_Alias_Values_The_Result_Is_Unchanged` 钉的是 255 个槽位的 SHA-256，失败时看不出是哪个槽位变了，以后新增全局 Token 也会让它无故失败。最终审阅时决定：改成钉可逐项对比的文本，或作为本计划的一次性验证删除。
  2. `ThemeCompiler` 里新增的 `MarkExplicit(DesignToken, …)` 与 `ApplyValues`、控件版 `MarkExplicit` 三段循环近乎重复；显式 Alias 值被深拷贝两次。
- 记录在案的既有问题（不在本计划范围内，未处理）：控件使用 `Global` 或 `Custom` 算法模式时，`ReevaluateControlGlobal` 从根快照重跑算法与 Alias 计算，只重新写入该控件自己的全局覆盖。主题在根上显式设置的 Map 与 Alias 值在这个控件的作用域里会退回默认公式的结果。本任务前后行为相同，但现在更容易被看见（例如把 Bootstrap 下某个控件设为 `Global` 模式，它作用域内的 `ControlOutlineWidth` 会回到 2）。需要用户决定是否单独立项。
