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
