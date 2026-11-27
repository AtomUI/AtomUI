# 设计语言架构重构：任务总表与进度跟踪

- 架构方案：`docs/developments/specs/2026-11-22-design-language-architecture-design.md`
- 实施方案：`docs/developments/plans/2026-11-22-design-language-architecture-refactor.md`
- 分支与 worktree：`feature/design-language-products`，`/Users/chinboy/Projects/dotnet/AtomUIV6-design-language-products`
- 基准树：release/6.0 `8c946e4f5`

本文件是重构全程的唯一进度记录。每完成一项任务，在同一次改动里更新它的状态与证据；每做一个偏离实施方案的决定，写进
「裁定记录」。上下文丢失后以本文件为准继续，不凭记忆。

状态取值：`未开始`、`进行中`、`完成`、`阻塞`。「证据」写命令与结果，或产物路径；没有证据不得标 `完成`。

## 当前状态

- **所在里程碑：** M5 性能门槛受阻
- **本轮证据：** 48 个变体、484 对 Token、最终回归及预发布包消费已核对。正式性能未通过
- **未结项：** 紧凑冷编译确认退化，未缓存切换未通过 2% 门槛。M5-6、M6 与 M7 继续等待 M5-G。[性能问题与原因分析](#性能问题与原因分析)
- **执行方式：** 按项目负责人指示连续执行全部任务、不中途停、不提交；需要选择时按上下文裁定并记入裁定记录
- **最近更新：** 2026-11-23

## 待决事项

- 性能待决项为读集的保留方式、冷编译剩余成本及未缓存往返的阶段归因。正式结果、已停止的假设和下一轮范围见[性能问题与原因分析](#性能问题与原因分析)。
- Theme Presets 是现有工具 76 个默认路由之外的独立页面。两边原 Release 二进制分别加载该路由，Ant 亮色常规同构建复拍尺寸稳定，基线 1300×1476，候选 1300×1411。基线从 11 个语言预览入口默认选 PrimeNG Aura，候选只呈现 Daybreak Blue 具名主题。用户选择保留 11 个入口。已恢复原名称与顺序、Aura 默认预览和 v2 导出。相同场景的原始亮色常规双侧复拍均为 1300×1476、逐像素相等，聚焦测试 20/20。
- Ant 紧凑亮色、PrimeNG Aura 亮色常规首轮的 Avatar 存活计数为 1→2。原路线同序再次对照，两版本各两次均为 2/3。堆证据指向正在握手的 HTTPS 工作，经共享图片加载进度回调保留已卸载控件。对应六个源码文件与 M0 相同。已验证共享加载器根因修复。临时用例 RED→GREEN、清理后 ImageLoading 146/146。只替换共享加载器程序集后，同一候选在原 PrimeNG Aura 路由次序下 `Avatar 0/3`、`Masonry 0/3`，全部 76 页 0 存活。最终候选还将验证 77 路由。
- M6、M7 依赖 M5 候选真实打包及门槛完成。AtomUI.Mobile、MachGrid、Charts 和 DockSystem 的本地仓库均存在。MachGrid 和 Charts 有各自的串行任务规则，后续视觉批准在对应仓库登记。

## 里程碑总览

| 里程碑 | 内容 | 任务数 | 完成 | 状态 |
| --- | --- | --- | --- | --- |
| M0 | 基线与护栏 | 8 | 8 | 完成 |
| M1 | 旧架构内的等价清理 | 9 | 7 | 进行中 |
| M2 | 核心：新契约与单遍管线 | 30 | 29 | 进行中 |
| M3 | Ant Design | 31 | 30 | 进行中 |
| M4 | Shadcn 与 PrimeNG | 39 | 38 | 阻塞 |
| M5 | 主仓收尾 | 14 | 9 | 阻塞 |
| M6 | 移动端（AtomUI.Mobile 仓库） | 6 | 0 | 未开始 |
| M7 | 商业产品（各产品仓库） | 8 | 0 | 未开始 |

---

## M0 基线与护栏

| ID | 任务 | 依赖 | 状态 | 证据 |
| --- | --- | --- | --- | --- |
| M0-1 | GalleryPerformance 按变体运行：`DesignLanguageVariants`、三个变体参数、`--list-variants`，经 Gallery 菜单命令切换 | — | 完成 | Step 1：Release 构建通过；`--list-variants` 退出 0、输出 48 行（`.artifacts/m0/variants.txt`）。核对方式定为指纹比对（临时机制，M5-6 删除）。Step 2：`Shadcn.Maia Dark Compact` 截 `button` 页退出 0，切到 `ShadcnMaia.DarkCompact`、指纹一致，截图确认暗色、紧凑、胶囊按钮（`.artifacts/m0/step2-maia-dark-compact/`）。Step 3：两边指纹相同（`e294937752e3a512`），但 `compare_snapshots.py` 退出 1：8 个像素不同，全部在「Loading」按钮的旋转图标上（`.artifacts/m0/step3/region.png`），属 README 已写明的「两遍截图遮罩覆盖不全」。按项目负责人决定换无动画页重做：breadcrumb、descriptions、separator 三页同一构建两遍 `unstable px` 均为 0，默认启动与 `AntDesign Light Normal` 对比全部 identical，`compare_snapshots.py` 退出 0（`.artifacts/m0/step3-static/compare-static.txt`）；tag 页因「processing」标签的旋转图标有动画，不作为 Step 3 页面 |
| M0-2 | ControlBaseline 按变体运行：注册 PrimeNG/Shadcn 主题包，经同一解析路径切换 | M0-1 | 完成 | Step 1：引用两个主题包、链接 `DesignLanguageVariants.cs`、三个变体参数与切换；Release 构建通过；`--mode fluent` 带变体参数、外观写成小写 `dark` 均以退出码 2 拒绝。Step 2：默认与 `AntDesign Light Normal` 各跑两遍（每遍约 19 秒），225 个类型中 222 个 identical；3 个不同（DateViewer、DropdownButton、QRCode）全部在自身动画旁（最大差 9）。共 10 个类型自带动画，其余 215 个无动画类型比对退出 0（`.artifacts/m0/m0-2-step2/compare-static.txt`）。四个类型（FlyoutHost、PopupConfirm、TabItem、TabStripItem）每遍都失败，去掉主题包注册后照样失败，属 M0 之前已有（`.artifacts/m0/m0-2-precheck.log`）。Step 3：`Shadcn.Maia Dark Compact` 切到 `ShadcnMaia.DarkCompact`，Button 截图为暗色配色与胶囊圆角（`.artifacts/m0/m0-2-step3/Button.png`） |
| M0-3 | 取值导出与比对：拆出 `ThemeSnapshotDumpTests.cs`、链接辅助类、Core 加一行 IVT、`--dump-tokens`（484 份）、`compare_token_dumps.py` | M0-1 | 完成 | Step 1：拆出 `ThemeSnapshotDumpTests.cs`（辅助文件只删 49 行：4 个测试用 using 与测试类），`dotnet test --filter ThemeSnapshotDump` 3 个通过。Step 2：Core 加一行 IVT、链接辅助类、`--dump-tokens`；484 个文件、退出 0、约 18 秒、46 MB；种子生效（452 份互不相同，32 份重复为各 Shadcn 变体的 `base-neutral`，其值即默认种子）。Step 3：同一构建两遍比对退出 0（导出可复现）；改一个值退出 1 并打印；候选多一个键严格模式退出 1、`--allow-added` 退出 0 并列出；全部文件改名后无映射退出 1、有映射退出 0；例外清单命中退出 0 并列出 |
| M0-4 | 守卫脚本：`run_perf_guards.sh` 可选变体参数、新增 `run_design_language_guards.sh`、README | M0-2、M0-3 | 完成 | 另加两个可选参数（见裁定记录）：`--freeze-loading-icons`（两个工具，截图前停住并强制复位加载图标旋转；Button 页同一构建连拍 3 次 0 像素差异，不冻结时每次差 28～78 像素）、`compare_snapshots.py --ignore-near-animation`。Step 1：同一构建作两边、`AntDesign Light Normal`、`button` 页：PASS，变体 168 秒、取值 484 份 26 秒（`.artifacts/m0/m0-4-step1/summary.txt`）；状态截图 4 个失败类型与 4 个泄漏类型两边相同，属既有。冻结做法两次修正后（见裁定记录），Step 1 与 Step 2 合并重跑：`AntDesign Light Normal` 与 Shadcn Nova 4 个变体全部 PASS（每个变体约 162 秒），484 份取值一致（26 秒），总计 838 秒（`.artifacts/m0/m0-4-final/summary.txt`）。`AtomUI.Themes.Tests` 全部 344 个测试通过 |
| M0-5 | **停下评审**；通过后提交 M0 的工具改动（重构中唯一一次门槛前的提交） | M0-4 | 完成 | 项目负责人同意分两次提交：`f45d9cd8f` docs（方案、计划、本表），`9f54cdf2a` M0 工具（基线提交） |
| M0-6 | 基线 worktree 与 Release 构建；单变体全量自检与耗时；**停下请示**全量 48 个变体的运行频率 | M0-5 | 完成 | Step 1：`/Users/chinboy/Projects/dotnet/AtomUIV6-design-language-baseline`（分离头指针 `9f54cdf2a`），两个工具 Release 构建通过。Step 2：基线对候选（同为 `9f54cdf2a` 的产品代码），`AntDesign Light Normal`、全部 showcase：像素 PASS（692 秒），484 份取值一致（29 秒），总计 721 秒（`.artifacts/design-language-guards/m0-selfcheck/summary.txt`）。Step 3：运行频率见裁定记录 |
| M0-7 | 性能基线：编译与切换测量、启动采样，各两遍，记录分辨力 | M0-6 | 完成 | 基线 worktree，MacBookPro18,2（Apple M1 Max，10 核），macOS 26.6.2；测量期间 1 分钟负载 24–53（本机同时有其他会话的进程）。数值与分辨力见门槛记录 M0-G 行 |
| M0-G | **M0 门槛**：基线构建完毕、单变体全量自检 PASS、运行频率已定、性能基线已记录；停下评审 | M0-1…7 | 完成 | 四项全部满足（见门槛记录）；按总则裁定不停下 |

## M1 旧架构内的等价清理

| ID | 任务 | 依赖 | 状态 | 证据 |
| --- | --- | --- | --- | --- |
| M1-1a | Button 族行为开关 Token 化（`ButtonThemeRecipe` 的七个属性） | M0-G | 完成 | `AbstractButtonGeometryToken` 新增七个属性，Ant 默认全为 `false`、`DisabledOpacity` 1.0；`ButtonThemeRecipe.cs` 删除，改为 `ButtonBehavior`（按快照与四个按钮身份缓存读取结果）；两个语言包改为逐个赋值 |
| M1-1b | SplitButton `InsetSeparator` | M0-G | 完成 | `SplitButtonToken.InsetSeparator`，Shadcn 设为 `true` |
| M1-1c | 输入族 `InputFrameAppearance` 字段 Token 化 | M0-G | 完成 | `LineEditToken`、`TextAreaToken`、`SelectToken` 各加 `TransparentOutlineBorder`、`BottomOutlineBorder`，`SelectToken` 另加 `MutedSelectIndicator`；`InputFrameAppearance` 枚举保留为 `InputControlFrame` 内部的计算结果 |
| M1-1d | FloatButton `FloatButtonOutlineAppearance` 字段 Token 化 | M0-G | 完成 | `FloatButtonToken` 新增六个属性（见裁定记录）；记录类型保留为从 Token 读出的结果 |
| M1-1e | 删除 `AppearanceSettings`；G-取值、G-像素 | M1-1a…d | 完成 | 删除 AppearanceSettings。最终候选 484 份取值对等，48 个视觉变体验收完成。历史 M1 原六个失败在原用例复验通过 |
| M1-2 | 逐控件全局改写清点表（22 个阶段类、125 处直接改写及辅助方法里的改写），（用途改为 M4 换写对照，见变更日志） | M0-G | 完成 | `docs/developments/progress/2026-11-22-control-global-rewrite-inventory.md`：改写 169 行（500 个语言×Token 类×全局 Token 写入对），读取与去向 442 行：A 10、B 376、C 56；B 中 148 行标 †（读取者是只接受全局 Token 的 `OwnerTokenResource` / `TokenTransitions.Duration`，多为被多个控件共用的部件主题）；30 行有 EvaluateOwn 依赖。† 的处理方式方案未覆盖，见待决事项 |
| M1-2b | （已撤回）共享 Own Token 契约 | — | 撤回 | 2026-11-22 实现后由项目负责人否决：复杂度无法维护。代码、测试、方案 4.3 与实施方案 M1-2b 全部撤回 |
| M1-4 | 去掉四行 `InternalsVisibleTo`，逐条处理编译错误，API 变更记入发布说明 | M1-2 | 完成 | 核心与控件包无第一方语言友元。PrimeNG、Shadcn 语言与取值包无第一方 IVT，PrimeNG 移除前后 48 份转储一致。Release 验证实际 15 个新语言产品 |
| M1-G | **M1 门槛**：四项门槛全过，Ant Design 零例外；停下评审并分组提交 | M1-1e、M1-4 | 进行中 | G-回归（M1 的树，`.artifacts/m1/regression.log`）：13067 个测试中 6 个失败，全部是 `ButtonFamilyEffectTests` 的「Token 有可见效果」检查，对象是 M1-1 新增的、只在开关打开时生效的 Token（Button、SubmitButton、ResetButton、DropdownButton 的 `DisabledOpacity`；FloatButton 的 `OutlineDescriptionLineHeight`、`DescriptionWeight`）。属测试缺口：Ant 默认开关关闭。修正：测试以伴随覆盖打开对应开关（`DisabledByOpacity`、`HasOutline`）并给出可见的扰动；因树已进入 M2，复验随 M3 的全量回归进行。G-像素全量 48 个变体在后台运行（`.artifacts/design-language-guards/m1-gate`）|

## M2 核心：新契约与单遍管线

开工前为本里程碑出步骤级计划：`docs/developments/plans/2026-11-22-design-language-m2-core.md`。

| ID | 任务 | 依赖 | 状态 | 证据 |
| --- | --- | --- | --- | --- |
| M2-0 | M2 步骤级计划 | M1-G | 完成 | 顺序：M2-1（冻结、语言 Token 基类、删旧计算入口）→ M2-2（语言契约与登记、控件视图）→ M2-3（在现有编译器上做减法：去掉算法链、分阶段求值与写入记账，保留快照、差量、惰性与缓存结构）→ M2-4（配置与选择）→ M2-5（XML v2）→ M2-6（生成器与校验）。每步先写测试再实现；切换窗口内只要求 Core、Generator、Toolchain 及其测试可构建。M1 门槛与回归在后台对 M1 的树运行，回归结束前不改产品代码 |
| M2-1a | Token 冻结语义：`Freeze`、`IsFrozen`，生成的 setter 检查冻结位 | M2-0 | 完成 | `AbstractDesignToken` 的 `TokenFreezeSet`（冻结、拒写、写入跟踪）；生成的 setter 检查冻结位。`TokenFreezeTests` 5 项通过 |
| M2-1b | 读集：`TokenReadSet`、`BeginReadSet` / `EndReadSet`，生成的 getter 记录读取 | M2-1a | 推迟到 M5-1 | |
| M2-1c | `LanguageDesignToken` 基类与特性；生成器支持语言 Token 类（属性实现、Schema、种子标记） | M2-1a | 完成 | `LanguageDesignToken` 与 `[LanguageDesignToken]`；生成器产出语言 Token 的属性实现与 Schema（种子保留 `Seed` 阶段）。Generator.Tests 的语言 Token 用例通过 |
| M2-1d | 删除 `CalculateTokenValues`、`CalculateAliasTokenValues`、`IsExplicit`、`RecordWrite`、`TokenWriteLog` | M2-1a…c | 完成 | `CalculateTokenValues`、`CalculateAliasTokenValues`、`IsExplicit`、`RecordWrite`、`TokenWriteLog` 已删；Alias 公式移入 `AntDesignAliasDerivation` |
| M2-2a | 语言契约类型：`DesignLanguage<T>`、`DesignLanguageAxes`、`DesignVariant`、`DesignStyle`、`DesignDensity`、`DesignIdiom`、`DerivationContext<T>`、`IControlDerivation<TL,TT>` | M2-1c | 完成 | `src/AtomUI.Core/Theme/Languages/` 全部契约类型 |
| M2-2b | 登记：`DesignLanguageDescriptor`、`ControlDerivationDescriptor`、构建器入口、按控件槽位的登记表 | M2-2a | 完成 | `DesignLanguageDescriptor`、`ControlDerivationDescriptor`（版本号取程序集 MVID）、`DesignLanguageBuilder<T>`、注册表按控件槽位的推导表 |
| M2-2c | 推导查找：沿语言 Token 类型继承链，三级各有测试；重复登记报错 | M2-2b | 完成 | `DesignLanguageRegistryTests`：三级继承链查找、最近者优先、精确登记优先、重复登记报错、追加校验 |
| M2-2d | 删除算法契约与算法描述（`IThemeAlgorithm` 等 9 个文件） | M2-2b、M2-3b | 完成 | `IThemeAlgorithm`、`ThemeAlgorithmDescriptor`、`ControlTokenAlgorithmDescriptor`、`StagedTokenEvaluation` 等已删；`Algorithms/` 余下的中性工具移到 `Derivation/` |
| M2-3a | 单遍编译·全局：种子 → 语言 Token → 全局 Token → 最终规范化 | M2-2b | 完成 | `ThemeCompiler.CompileUnitGlobal`；`DesignLanguageCompilationTests`（每步恰好一次、显式值冻结、种子输入、轴进入推导） |
| M2-3b | 单遍编译·控件：求值单元（作用域、覆盖种子或固定轴的控件）、惰性求值、每个推导恰好执行一次 | M2-3a | 完成 | `CompileControl`：固定轴或覆盖种子的控件自成求值单元；控件推导惰性且每个控件恰好一次（同上测试） |
| M2-3c | 快照加语言 Token 值表；缓存键含语言、变体、种子、显式值；删 `StagedTokenEvaluation` 与逐控件全局副本 | M2-3b | 完成 | 快照加语言 Token 值表与 `Variant`；`ThemeCacheKey` 含语言、变体与推导身份；`DenseThemeCompilerTests`、`TypedThemeSnapshotCacheTests` 通过 |
| M2-3d | 错误模型：全局失败使整次应用失败；控件推导失败在首次查询时暴露 | M2-3b | 完成 | `ThemeDerivationException`：全局失败使整次编译失败；控件推导失败与写种子在首次查询时暴露（测试同上） |
| M2-4a | `ThemeConfig` 新模型与 Builder；删 `ControlAlgorithmMode` | M2-2a | 完成 | `ThemeConfig` 与两个 Builder 的新字段；`ControlAlgorithmMode` 已删；`ThemeConfigImmutabilityTests` 通过 |
| M2-4b | Normalizer 与 Merger：轴的确定、取值的分层、`Inherit=false` 两种含义 | M2-4a | 完成 | `ThemeConfigNormalizerTests`、`ThemeConfigMergerTests`（轴的分层、`Inherit=false` 保留语言、切换语言丢弃语言 Token）通过 |
| M2-4c | `ThemeSelection` 与解析（三条规则、固定的轴、锁定的种子、跟随系统） | M2-4a | 完成 | `ThemeSelection`、`ThemeSelectionResolver`；`ThemeSelectionResolverTests` 14 项通过 |
| M2-4d | `IThemeManager` 新接口；语言切换；Review Focus 3 的测试 | M2-4b、M2-4c、M2-3c | 完成 | `IThemeManager.Languages`、`Themes`、`CurrentSelection`、`ApplyAsync(ThemeSelection)`；Review Focus 3 的告警 ATMTHM5003 |
| M2-5a | XML v2 参考规范、XSD、示例文件 | M2-0 | 完成 | XSD；`docs/reference/theming/theme-definition-xml-v2.md`、`compiled-theme-abi-v4.md`；v1 文档改为指引；文档示例经 xmllint 对 v2 XSD 校验 9/9 通过，`ThemeDocumentationExampleTests` 改读 v2 文档 |
| M2-5b | Reader 与 `ThemeDocument`（含 `LanguageTokens`、`When`、`Locked`） | M2-5a | 完成 | `ThemeDocumentReader` v2；v1 文件给 ATMTHM1004 并写出换写命令；`ThemeDocumentReaderTests` 通过 |
| M2-5c | Binder 与 `ThemeDefinition`；条件段对求值单元求值；全部语义校验的诊断测试 | M2-5b、M2-2b、M2-4b | 完成 | `ThemeDefinitionBinder` v2（2001–2004、3001–3008）、`PreparedThemeDefinition.ValuesFor`；`ThemeDefinitionBinderTests` 通过 |
| M2-5d | 编译主题 ABI v2：生成器 writer、工具链任务、运行时程序 | M2-5c | 完成 | 编译主题 ABI 4：生成器 writer、工具链 `CompiledThemeInput` 格式 8、运行时程序；`CompiledThemePackageWriterTests` 16 项、`CompiledThemeProgramAbiTests` 通过 |
| M2-5e | v1 → v2 换写命令；Review Focus 5 的测试 | M2-5b | 完成 | `ConvertThemeDefinitionsTask`（目标 `ConvertAtomUIThemeDefinitions`）与 `ThemeDefinitionV1Converter`：同语言四变体合并为一个文件、差异进 `<When>`、Fixed → Locked、IsDefault 删除、Disabled 控件覆盖种子给提示不改写（Review Focus 5）；`ThemeDefinitionV1ConverterTests` 9 项通过（含仓库 Shadcn、PrimeNG 主题包换写为合法 v2） |
| M2-5f | 来源与目录适配；用户目录里的 v1 文件给出诊断（Review Focus 1） | M2-5c | 完成 | 用户目录 v1 文件：应用启动、该目录不生效、诊断 ATMTHM1004 写出换写命令（`A_V1_User_Theme_Is_Not_Read_And_Its_Diagnostic_Names_The_Conversion_Command`，Review Focus 1） |
| M2-6a | 控件包内的生成物调整与 `TokenSchemaDigest` | M2-1c | 完成 | 控件包生成 `[assembly: TokenSchemaDigest]`（`TokenSchemaDigestWriter`） |
| M2-6b | 语言注册生成器、控件推导注册生成器、`CoversTokenSchema` | M2-2b | 完成 | 取值包生成 `[assembly: CoversTokenSchema]`；生成器缺陷 3 项已修（常量 Id、未注册语言不发布契约、本地 Token 须匹配控件），Generator.Tests 1119/1119 |
| M2-6c | 推导完整性分析器（未写全、先读后写） | M2-6b | 完成 | `DerivationAnalysis`：ATOMUIGEN050（未写全）、051（先读后写）；契约测试库对 Ant 全部桌面取值包逐变体检查完整性（`AntDesignControlContractTests`） |
| M2-6d | 覆盖校验与依赖方向校验（语言包不引控件包、控件包不引语言一侧、取值包内无控件） | M2-6b | 完成 | ATOMUIGEN052（取值包缺推导）、053（依赖方向：语言包引控件包、控件包引语言一侧、取值包含控件）；运行时覆盖校验指出缺的取值包名 |
| M2-6e | 配置提交校验：缺取值包（Review Focus 2）、摘要不符（Review Focus 4）、形态与轴范围 | M2-6a、M2-6b、M2-4d | 完成 | Review Focus 2 与 4：`TokenSchemaDigests.Validate` 在注册表覆盖校验时比对摘要，消息含两个包名与两个摘要（`TokenSchemaDigestTests`、`TokenSchemaDigestCoverageTests`） |
| M2-6f | 删除旧分析器（`ATOMUIGEN048` 等）与诊断号整理 | M2-1d | 完成 | 删除 ATOMUIGEN024、048 与 ATMTHM3103/3104/3105/3108；新增 ATOMUIGEN050–053。`OwnTokenReferenceAnalysis` 中对 `CalculateTokenValues` 的死分支留到 M5 清理 |
| M2-G | **M2 门槛**：Core、Generator、Toolchain 及其测试工程构建并通过单元测试；停下评审 | 以上全部 | 完成 | Core.Tests 566/566；Generator.Tests 1119/1119；Build.Tasks 107+9；其余见 M3-4c |

## M3 Ant Design

开工前出步骤级计划：`docs/developments/plans/2026-11-22-design-language-m3-antdesign.md`。

桌面控件的批次（按目录字母序，括号内为 Token 类个数，合计 142）：

| 批 | 目录 |
| --- | --- |
| B1（19） | AdornerLayer、Alert、AutoComplete、Avatar、Badge、BorderBeam、Breadcrumb、Buttons、ButtonSpinner |
| B2（15） | Calendar、Card、Carousel、Cascader、CheckBox、Collapse、ComboBox、DatePicker、DateViewer |
| B3（15） | Descriptions、Dialog、Drawer、DropdownButton、Empty、Expander、FloatButton、Flyouts、Form、GroupBox |
| B4（16） | ImagePreviewer、Input、ListBox、ListView、MarqueeLabel、Mentions、Menu |
| B5（14） | Message、MessageBox、NavMenu、Notifications、NumericUpDown、OptionButtonGroup、OtpLineEdit、Pagination、Popup、PopupConfirm、Primitives |
| B6（15） | ProgressBar、QRCode、RadioButton、Rate、Result、ScrollViewer、Segmented、Select |
| B7（16） | Separator、Skeleton、Slider、Space、Spin、SplitButton、Splitter、SplitView、Statistic、Steps、Switch |
| B8（15） | TabControl、Tag、Timeline、TimePicker |
| B9（17） | Tooltip、Tour、Transfer、TreeSelect、TreeView、Upload、Window、WindowTitleBar |

| ID | 任务 | 依赖 | 状态 | 证据 |
| --- | --- | --- | --- | --- |
| M3-0 | M3 步骤级计划 | M2-G | 完成 | 未单独写步骤级计划，见裁定记录 |
| M3-1a | `AtomUI.Controls` 的 3 个 Token 类清为纯声明，公式存档 | M3-0 | 完成 | FormulaConverter 剥离公式；公式存档于取值包 |
| M3-1b | `AtomUI.Desktop.Controls` 的 142 个 Token 类清为纯声明，公式存档 | M3-0 | 完成 | 142 个类清为纯声明；仅被推导读取的 Token 加 `[ReservedToken("A derivation input: …")]` |
| M3-1c | ColorPicker 6 个、DataGrid 1 个 Token 类清为纯声明，公式存档 | M3-0 | 完成 | ColorPicker 6 个、DataGrid 1 个；DataGrid 45 个 `internal` 属性改为 `public` |
| M3-1d | Token 属性的语义注释补全（英文） | M3-1a…c | 完成 | 本轮改进 1,030 个属性英文摘要。最后一批 92 个文件、585 个属性经 XML 校验，非注释文本完全相同，Release 编译器文档输出 585/585 |
| M3-2a | `src/DesignLanguages/AntDesign/` 目录、语言包工程、解决方案分组 | M3-0 | 完成 | `src/DesignLanguages/AntDesign/`，解决方案分组 `/DesignLanguages/AntDesign/` |
| M3-2b | `AntDesignLanguage`、`AntDesignToken`、轴、`DefineSeeds`（含原 DaybreakBlue 的种子） | M3-2a | 完成 | `AntDesignLanguage`、`AntDesignToken`、四个轴、可调种子 `ColorPrimary` |
| M3-2c | 全局推导·亮色 Map | M3-2b | 完成 | `AntDesignMapDerivation`（Default，依变体再叠 Compact、Dark） |
| M3-2d | 全局推导·暗色 Map | M3-2c | 完成 | 同上 |
| M3-2e | 全局推导·紧凑 | M3-2c | 完成 | 同上 |
| M3-2f | 全局推导·Alias | M3-2c | 完成 | `AntDesignAliasDerivation` |
| M3-2g | 核心的中性工具整理到 `Theme/Derivation/`；删除旧计算器与 `DaybreakBlue.theme.xml` | M3-2c…f | 完成 | 中性工具在 `Theme/Derivation/`；旧计算器与核心的 `DaybreakBlue.theme.xml` 已删，DaybreakBlue 迁入语言包（v2） |
| M3-2h | 契约测试；全局 Token 的取值比对（4 个变体 × 种子矩阵） | M3-2c…f | 完成 | `DesignLanguageContractTests.AntDesign_Satisfies_The_Contract_Under_Every_Variant`（8 个变体）通过；G-取值：`.artifacts/design-language-guards/m3-tokens/compare.txt` 20 个文件、0 改变、0 缺失、840 个新增键（M1-1 的开关 Token） |
| M3-3a | `AtomUI.Controls.Themes.AntDesign`（3 个推导 + 共享推导方法） | M3-1a、M3-2h | 完成 | 3 个推导 + `AddCommonControls` |
| M3-3-B1 | 桌面推导 B1 | M3-1b、M3-3a | 完成 | 由 FormulaConverter 从原 `CalculateTokenValues` 机械换写，142 个推导合计；Desktop.Controls.Tests 全部取值断言原值通过 |
| M3-3-B2 | 桌面推导 B2 | M3-3-B1 | 完成 | 由 FormulaConverter 从原 `CalculateTokenValues` 机械换写，142 个推导合计；Desktop.Controls.Tests 全部取值断言原值通过 |
| M3-3-B3 | 桌面推导 B3 | M3-3-B2 | 完成 | 由 FormulaConverter 从原 `CalculateTokenValues` 机械换写，142 个推导合计；Desktop.Controls.Tests 全部取值断言原值通过 |
| M3-3-B4 | 桌面推导 B4 | M3-3-B3 | 完成 | 由 FormulaConverter 从原 `CalculateTokenValues` 机械换写，142 个推导合计；Desktop.Controls.Tests 全部取值断言原值通过 |
| M3-3-B5 | 桌面推导 B5 | M3-3-B4 | 完成 | 由 FormulaConverter 从原 `CalculateTokenValues` 机械换写，142 个推导合计；Desktop.Controls.Tests 全部取值断言原值通过 |
| M3-3-B6 | 桌面推导 B6 | M3-3-B5 | 完成 | 由 FormulaConverter 从原 `CalculateTokenValues` 机械换写，142 个推导合计；Desktop.Controls.Tests 全部取值断言原值通过 |
| M3-3-B7 | 桌面推导 B7 | M3-3-B6 | 完成 | 由 FormulaConverter 从原 `CalculateTokenValues` 机械换写，142 个推导合计；Desktop.Controls.Tests 全部取值断言原值通过 |
| M3-3-B8 | 桌面推导 B8 | M3-3-B7 | 完成 | 由 FormulaConverter 从原 `CalculateTokenValues` 机械换写，142 个推导合计；Desktop.Controls.Tests 全部取值断言原值通过 |
| M3-3-B9 | 桌面推导 B9 | M3-3-B8 | 完成 | 由 FormulaConverter 从原 `CalculateTokenValues` 机械换写，142 个推导合计；Desktop.Controls.Tests 全部取值断言原值通过 |
| M3-3c | `AtomUI.Desktop.Controls.ColorPicker.Themes.AntDesign`（6） | M3-1c、M3-3-B9 | 完成 | 6 个推导 + `AddDesktopColorPicker` |
| M3-3d | `AtomUI.Desktop.Controls.DataGrid.Themes.AntDesign`（1） | M3-1c、M3-3-B9 | 完成 | 1 个推导 + `AddDesktopDataGrid` |
| M3-3e | 四个取值包的覆盖校验无报错 | M3-3a…d | 完成 | 注册表覆盖校验无报错（Gallery、ControlBaseline、各测试工程启动通过） |
| M3-4a | GalleryBase 的 8 个 Token 类清为纯声明 + 通用推导 | M3-2h | 完成 | 8 个 Token 类清为纯声明；通用推导在 `src/AtomUI.Toolkits.GalleryBase/Derivations/` |
| M3-4b | Gallery 启动配置与主题菜单的最小接入（只接 Ant Design） | M3-3e、M3-4a | 完成 | Gallery 构建通过；主题菜单按 `Language[.Style]` 列语言；Theme Presets 改用 v2 |
| M3-4c | 各测试工程的启动配置与受影响测试的改写 | M3-3e | 完成 | Desktop.Controls 4516/4516、DataGrid 336/336、Controls 246/246、Controls.Shared 201/201、GalleryBase 219/219、Gallery 444/461（17 项 M4 跳过）、Rendering 4063/4082（19 项 M4 跳过）、Localization 135、TypeMap.Linker 103、GalleryBase.Generator 10、Icons 1；Localization.IntegrationTests 的 2 项打包失败为沙箱网络（NU1301） |
| M3-4d | 基线工具改用新的 `ThemeSelection`；夹具主题文件换写为 v2 | M3-4b | 完成 | GalleryPerformance、ControlBaseline 已接入 `ThemeSelection`；Gallery 共享 v2 XSD 验证全部 9 个 `Fixtures/*.theme.xml` 通过。旧格式转换测试数据按原格式保留 |
| M3-G | **M3 门槛**：Ant Design 4 个变体 G-取值、G-像素、G-性能、G-回归，零例外；停下评审 | 以上全部 | 进行中 | G-取值：20 个 Ant 转储 0 改变 0 缺失。原 M3 守卫日志 `m3-ant/summary.txt` 四个变体返回 FAIL，不能作已通过证据。亮色/暗色常规指纹在较新候选上以原入口复验成功。当前像素见待决事项与 2026-11-23 记录 |

## M4 Shadcn 与 PrimeNG

开工前出步骤级计划：`docs/developments/plans/2026-11-22-design-language-m4-shadcn-primeng.md`。桌面批次沿用 M3 的 B1–B9。

| ID | 任务 | 依赖 | 状态 | 证据 |
| --- | --- | --- | --- | --- |
| M4-0 | M4 步骤级计划 | M3-G | 完成 | 两个并行代理按 `m4-brief` 执行（见裁定记录） |
| M4-1a | Shadcn 语言包工程、目录与分组、八种风格、轴、种子（含语言种子 `BaseColor`，核心删 `ColorNeutral`） | M4-0 | 完成 | `ShadcnLanguage`：八种风格（Nova 缺省）、常规与紧凑、桌面；启用种子 `ColorPrimary`、语言种子 `ShadcnToken.BaseColor`。旧全局 `ColorNeutral` 已移除 |
| M4-1b | `ShadcnToken` 与 `DeriveLanguage`（由 `ShadcnSurface`、`ShadcnStyle`、`ShadcnOverlay`、`ShadcnPrimary` 公开而来） | M4-1a | 完成 | `ShadcnToken`：16 个基础色角色、`ThemePrimary` 等、风格几何与浮层表；与全局 Token 同名的六个改名为 `StyleControlHeight*`、`StyleFontSize*` |
| M4-1c | `DeriveGlobal`：复制 Ant 的 Map 与 Alias 为起点并并入 Shadcn 的覆盖；紧凑 | M4-1b | 完成 | Ant Default/Dark 计算器副本 → Shadcn 颜色与尺寸 → `ColorBgMask`、`ColorNeutral` → 紧凑；Alias 公式副本 + Shadcn 覆盖（两个守卫 Token 原位写入） |
| M4-1d | 组件配方、README | M4-1b | 完成 | README、Table、ListItem、Overlay、Field、Chart 已落地。Chart 五个源 Token 可逐项覆盖。官方表格样式未定义条纹，Touch 属移动形态。临时配方测试清理后聚焦 11/11 |
| M4-1e | 全局 Token 取值比对（32 个变体 × 种子矩阵） | M4-1c | 完成 | Shadcn：416 份转储（8 风格 × 明暗 × 密度 × 13 种子用例）0 改变 0 缺失；`AtomUI.DesignLanguages.Shadcn.Tests` 135/135（含 86 项移植的快照测试与契约） |
| M4-2a | `AtomUI.Controls.Themes.Shadcn` | M4-1e | 完成 | 162 个推导由 Ant 复制改名，约 40 个并入 Shadcn 覆盖 |
| M4-2-B1 | Shadcn 桌面推导 B1 | M4-2a | 完成 | 并入 M4-2a / M4-4a 一次完成 |
| M4-2-B2 | Shadcn 桌面推导 B2 | M4-2-B1 | 完成 | 并入 M4-2a / M4-4a 一次完成 |
| M4-2-B3 | Shadcn 桌面推导 B3 | M4-2-B2 | 完成 | 并入 M4-2a / M4-4a 一次完成 |
| M4-2-B4 | Shadcn 桌面推导 B4 | M4-2-B3 | 完成 | 并入 M4-2a / M4-4a 一次完成 |
| M4-2-B5 | Shadcn 桌面推导 B5 | M4-2-B4 | 完成 | 并入 M4-2a / M4-4a 一次完成 |
| M4-2-B6 | Shadcn 桌面推导 B6 | M4-2-B5 | 完成 | 并入 M4-2a / M4-4a 一次完成 |
| M4-2-B7 | Shadcn 桌面推导 B7 | M4-2-B6 | 完成 | 并入 M4-2a / M4-4a 一次完成 |
| M4-2-B8 | Shadcn 桌面推导 B8 | M4-2-B7 | 完成 | 并入 M4-2a / M4-4a 一次完成 |
| M4-2-B9 | Shadcn 桌面推导 B9 | M4-2-B8 | 完成 | 并入 M4-2a / M4-4a 一次完成 |
| M4-2c | Shadcn 的 ColorPicker 与 DataGrid 取值包 | M4-2-B9 | 完成 | ColorPicker、DataGrid 取值包 |
| M4-2d | Shadcn 四个取值包的覆盖校验；README（映射表、不支持清单、各控件目标等级） | M4-2a…c | 完成 | 覆盖校验无 ATOMUIGEN050–053；五个 README（112 个控件 Token 类的目标等级） |
| M4-3a | PrimeNG 语言包工程、目录与分组、三种风格、轴、种子 | M4-0 | 完成 | `PrimeNGLanguage`：Aura（缺省）、Lara、Nora；种子按旧主题 XML（noir `#020617`、状态色、圆角与控件高度逐风格） |
| M4-3b | `PrimeNGToken` 与 `DeriveLanguage`（由 `PrimeNGPreset`、`PrimeNGAppearanceRules`、`PrimeNGPalette`、`PrimeNGNoir` 公开而来） | M4-3a | 完成 | `PrimeNGToken`：PrimeUIX 表面色阶 `Surface50`…`Surface950`；其余规则表仍为内部表 |
| M4-3c | `DeriveGlobal`：复制 Ant 的 Map 与 Alias 为起点并并入 PrimeNG 的覆盖；紧凑 | M4-3b | 完成 | 捕获原种子后按旧链顺序：Ant 计算器副本 → 预设颜色（noir）→ 过渡时长 → 尺寸阶梯 → 紧凑；Alias 三个回读 Token 原位写入 |
| M4-3d | 组件配方、README | M4-3b | 完成 | Table 提供浅色 Surface50、深色 Surface950 条纹角色，另有 ListItem、Overlay、Field。PrimeNG Chart 使用调用者提供的系列色。无第一方 IVT，临时配方测试清理后聚焦 16/16 |
| M4-3e | 全局 Token 取值比对（12 个变体 × 种子矩阵） | M4-3c | 完成 | PrimeNG：48 份转储 0 改变 0 缺失；`AtomUI.DesignLanguages.PrimeNG.Tests` 33/33（含契约、9 个写种子控件的视图全局值测试） |
| M4-4a | `AtomUI.Controls.Themes.PrimeNG` | M4-3e | 完成 | 154 个推导；旧控件阶段移植到 `Stages/`；Lara 紧凑 Select 条件进推导 |
| M4-4-B1 | PrimeNG 桌面推导 B1 | M4-4a | 完成 | 并入 M4-2a / M4-4a 一次完成 |
| M4-4-B2 | PrimeNG 桌面推导 B2 | M4-4-B1 | 完成 | 并入 M4-2a / M4-4a 一次完成 |
| M4-4-B3 | PrimeNG 桌面推导 B3 | M4-4-B2 | 完成 | 并入 M4-2a / M4-4a 一次完成 |
| M4-4-B4 | PrimeNG 桌面推导 B4 | M4-4-B3 | 完成 | 并入 M4-2a / M4-4a 一次完成 |
| M4-4-B5 | PrimeNG 桌面推导 B5 | M4-4-B4 | 完成 | 并入 M4-2a / M4-4a 一次完成 |
| M4-4-B6 | PrimeNG 桌面推导 B6（含 Lara 紧凑的 Select 条件） | M4-4-B5 | 完成 | 并入 M4-2a / M4-4a 一次完成 |
| M4-4-B7 | PrimeNG 桌面推导 B7 | M4-4-B6 | 完成 | 并入 M4-2a / M4-4a 一次完成 |
| M4-4-B8 | PrimeNG 桌面推导 B8 | M4-4-B7 | 完成 | 并入 M4-2a / M4-4a 一次完成 |
| M4-4-B9 | PrimeNG 桌面推导 B9 | M4-4-B8 | 完成 | 并入 M4-2a / M4-4a 一次完成 |
| M4-4c | PrimeNG 的 ColorPicker 与 DataGrid 取值包 | M4-4-B9 | 完成 | ColorPicker、DataGrid 取值包 |
| M4-4d | PrimeNG 四个取值包的覆盖校验；README | M4-4a…c | 完成 | 覆盖校验通过；五个 README |
| M4-5a | 解决方案分组、打包脚本、包布局白名单；删除 `AtomUI.Themes.Shadcn`、`AtomUI.Themes.PrimeNG`、`/Themes/` 分组 | M4-2d、M4-4d | 完成 | slnx 分组；打包清单换成新包；删除 `src/AtomUI.Themes.Shadcn`、`src/AtomUI.Themes.PrimeNG`（v1 主题文件留作换写命令测试数据 `tests/AtomUI.Build.Tasks.Tests/TestData/ThemeDefinitionsV1/`） |
| M4-5b | Gallery 接入两种语言；基线工具 44 个变体改用新选择 | M4-5a | 完成 | Gallery 与 ControlBaseline 启用三种语言；GalleryPerformance 列出 48 个变体 |
| M4-5c | 恢复 M3 门槛时跳过的测试；旧 `AtomUI.Themes.Tests` 的内容迁入各语言的测试工程 | M4-5a | 完成 | 清除已完成的 FinalVisual 临时渲染探针，恢复改写后的 Gallery 与 Rendering 用例进入选测。全量中 Rendering 4104/4104、Gallery 456 通过另有 1 个显式性能测试跳过 |
| M4-G | **M4 门槛**：48 个变体 G-取值、G-像素（Ant Design 零例外，其余例外逐条批准）、G-性能、G-回归；停下评审并分组提交 | 以上全部 | 阻塞 | 取值 484 对等，视觉 48 个变体结论齐全，最终回归零失败。[正式 R2 性能验收未通过](#性能问题与原因分析) |

## M5 主仓收尾

| ID | 任务 | 依赖 | 状态 | 证据 |
| --- | --- | --- | --- | --- |
| M5-1a | 读集随求值结果缓存；子求值单元与读集不相交时复用父结果 | M4-G | 完成 | 共享视图副本（`ControlViewPool`）与全局读集（`GlobalTokenReadSet`）；`DenseThemeCompilerTests` 4 项新测试，读集复用测试 RED→GREEN |
| M5-1b | 新增五个性能场景并与 M0 基线对照；收益不足时按方案待定点退回并记录数据 | M5-1a | 进行中 | 新场景各 15 样本，复用断言通过。正式冷编译与读集成本已量化，读集保留方式待定。[数据与诊断](#性能问题与原因分析) |
| M5-1c | 启动：只启用 Ant Design 与启用三种只用一种的差值；超门槛时推迟程序集加载 | M4-G | 完成 | 最终原生启动 30/30，三语言相对只启用 Ant 的首帧配对差 −10.597 ms，上一批 +2.104 ms。只启用 Ant 相对 M0 重复观测到约 +58 ms。[启动分段](#原生-jit-启动与局部复用) |
| M5-2 | Gallery 的语言、风格、明暗、密度、品牌色、Base Color 菜单全部由 `IThemeManager.Languages` 生成 | M4-G | 完成 | Gallery 语言与风格菜单由 `IThemeManager.Languages` 生成（`GalleryWorkspaceViewModel.DesignLanguages`） |
| M5-3a | 对等性样例：样例语言包、样例控件包、两个样例取值包及其测试 | M4-G | 完成 | `tests/fixtures/DesignLanguageParity/`：派生自 Ant Design 的样例语言 `Sample`、样例控件包（徽标、标签两个控件，只有声明）、Ant Design 与 Sample 两个取值包；`AtomUI.DesignLanguages.Parity.Tests` 3/3（样例语言给值、标签沿继承链取 Ant 值、只启用 Ant 时取 Ant 值、契约检查为空）。只引用公开 API |
| M5-3b | 打包脚本产出全部新包；干净 NuGet 缓存下的最小应用构建并启动 | M5-3a | 完成 | Release 产出并复核 36 个包，含 15 个新包，产品 TFM net8.0 / net10.0 / net11.0。两个直接包引用的临时 macOS 应用从空缓存恢复、构建、启动并验证 AntDesign Button 模板，均退出 0。发布包布局与任务宿主隔离检查通过 |
| M5-4a | 架构文档重写：`runtime.md` 相关章节、`compiled-theme-packages/*` | M4-G | 完成 | `runtime.md`、`compiled-theme-packages/*` 已核对改写 |
| M5-4b | 指南重写：定制、编写设计语言、设计语言包、第三方控件包 | M4-G | 完成 | 定制、编写设计语言、设计语言包（由 `preset-theme-packages.md` 改名）、第三方控件包 |
| M5-4c | 参考与模块文档；方案提炼为 `docs/architecture/systems/theming/design-languages.md` | M5-4a | 完成 | `docs/architecture/systems/theming/design-languages.md`（含与方案的差异表）；模块文档 |
| M5-4d | API 变更说明（中英） | M5-4a…c | 完成 | 迁移指南（中英）自 6.2.15 起，含 Shadcn、PrimeNG 与性能 |
| M5-5a | 取值溯源工具（不计入完成定义） | M4-G | 不做 | 不计入完成定义 |
| M5-5b | 设计时预览与无头测试的语言启用入口（不计入完成定义） | M4-G | 不做 | 不计入完成定义 |
| M5-G | **M5 门槛**：四项门槛在最终的树上完整重跑；文档示例由测试编译并运行；停下评审并分组提交 | M5-1…4 | 阻塞 | 本地包与普通/NativeAOT 消费、取值、视觉和最终回归完成。R2 紧凑冷编译 +16.31%，未缓存往返 +5.00%。[正式性能验收](#正式-r2-结果) |
| M5-6 | **验收后删除指纹比对**（项目负责人要求）：删除 GalleryPerformance `ApplyVariant` 里的解析器路径与指纹比对，保留菜单命令切换；单独提交 | M5-G | 未开始 | |

## M6 移动端（AtomUI.Mobile 仓库）

| ID | 任务 | 依赖 | 状态 | 证据 |
| --- | --- | --- | --- | --- |
| M6-0 | 移动端计划（在 AtomUI.Mobile 仓库内）；M5 的包以预发布版本发到本地源 | M5-G | 未开始 | |
| M6-1 | 65 个 Token 类改为 `public partial` 并清为纯声明 | M6-0 | 未开始 | |
| M6-2 | Ant Design 语言包声明 `Mobile` 形态；Mobile Core 的界面根给出 `DesignIdiom.Mobile` | M6-0 | 未开始 | |
| M6-3 | `AtomUI.Mobile.Controls.Themes.AntDesign`，公式按固定规则换写 | M6-1、M6-2 | 未开始 | |
| M6-4 | 等价验收：移动端渲染与迁移前逐像素一致 | M6-3 | 未开始 | |
| M6-5 | 移动形态的取值改为依据 Ant Design Mobile 官方；差异逐条列出并经批准 | M6-4 | 未开始 | |

## M7 商业产品（各产品仓库）

| ID | 任务 | 依赖 | 状态 | 证据 |
| --- | --- | --- | --- | --- |
| M7-0 | MachGrid 计划（在 MachGrid 仓库内） | M5-G | 未开始 | |
| M7-1 | MachGrid：Token 类改为 `public partial` 并清为纯声明 | M7-0 | 未开始 | |
| M7-2 | MachGrid：补齐并存状态各自的 Own Token 与成对的前景 | M7-1 | 未开始 | |
| M7-3 | MachGrid：`AtomUI.MachGrid.Themes.AntDesign`；Ant Design 下与迁移前逐像素一致 | M7-2 | 未开始 | |
| M7-4 | MachGrid：BrandWarm 改为具名主题或派生语言 | M7-3 | 未开始 | |
| M7-5 | MachGrid：`AtomUI.MachGrid.Themes.PrimeNG`（对标 DataTable）与 `.Themes.Shadcn` | M7-3 | 未开始 | |
| M7-6 | Charts：同样的四步（系列色对到 Shadcn 的 `--chart-1..5`） | M7-5 | 未开始 | |
| M7-7 | DockSystem 及其余产品：同样的四步 | M7-5 | 未开始 | |

---

## 门槛记录

每到一个门槛，在这里记下四项门槛的命令、结果与产物路径。

| 门槛 | 日期 | G-取值 | G-像素 | G-性能 | G-回归 | 评审结论 |
| --- | --- | --- | --- | --- | --- | --- |
| M0-G | 2026-11-22 | 自检 PASS：484 份取值逐项相等 | 自检 PASS：`AntDesign Light Normal` 全部 showcase 与全部控件状态 | 见下方「M0-7 性能基线」 | 不适用 | 通过 |
| M1-G | | | | | | |
| M2-G | | 不适用 | 不适用 | 不适用 | 核心单测 | |
| M3-G | | | | | | |
| M4-G | 2026-11-23 | 484 对等 | 48 个变体已核对 | R2 未通过 | 0 失败 | 阻塞 |
| M5-G | 2026-11-23 | 同一冻结候选 | 同一冻结候选 | R2 未通过 | 17 工程，0 失败 | 阻塞 |

### M0-7 性能基线

基线构建 `9f54cdf2a`，Release。两遍之间的差异就是本机在当时负载下的分辨力。

| 指标 | 第一遍 | 第二遍 |
| --- | ---: | ---: |
| 冷编译中位数 默认 / 暗色 / 紧凑（ms） | 2.62 / 2.54 / 2.43 | 6.26 / 5.58 / 4.49 |
| 冷编译分配中位数 默认 / 暗色 / 紧凑（字节） | 1,203,552 / 1,208,744 / 1,204,040 | 同左，逐字节相同 |
| 已缓存的明暗往返中位数（ms） | 43.5 | 53.4 |
| 未缓存的明暗往返中位数（ms） | 50.1 | 40.8 |
| 启动 `Launch.ToFirstRenderedBatch` 中位数（ms，`atom-window`，10 轮） | 1229.1 | 1515.8 |

编译耗时两遍相差到 2.4 倍，切换与启动波动也较大。两遍分配字节完全相同。正式判定使用下方固定口径。

## 性能问题与原因分析

截至 2026-11-23，Ant Design 紧凑主题的冷编译退化已确认，未缓存明暗往返高于 2% 门槛且实际差值仍受运行波动影响。两项独立阻塞 M4-G 和 M5-G。读集采集解释了部分冷编译开销。切换的剩余原因仍待定位。[设计要求与新增场景](../specs/2026-11-22-design-language-architecture-design.md#12-性能设计) · [M5 前置门槛裁定](#变更日志)

### 正式 R2 结果

- **输入与采样。** M0 用 `12cc0dabf`，与 `9f54cdf2a` 的 Git 树相同。最终候选用冻结 Release 产物。M0 / 候选 Core SHA256 前缀 `de3d8a753931` / `26ae8029abbc`，测量程序集 `974832df9626` / `9d71ede33adc`。同一台 Apple M1 Max、macOS 26.6.2，Release `net10.0`，直接执行 `Measure_Default_Theme_Compile_And_Switch`。两侧关闭分层 JIT（`DOTNET_TieredCompilation=0`），方向交错 `AB/BA/AB/BA/AB/BA/AB`，14 次均成功，保留全部样本。每进程每档冷编译 3001 样本、300 预热；每种缓存模式 501 次往返、50 次预热。原始汇总及 14 份进程数据在 `/private/tmp/atomui-formal-steady-switch-gate.GLJlmv/`。[R2 口径，见 Task 7 测量方法](2026-11-13-theme-algorithm-strategy.md) · [测量入口](../../../tests/AtomUI.Desktop.Controls.Tests/Theme/Performance/ThemeCompileMeasurement.cs#L53)
- **范围。** 冷编译在配置准备完后开始计时，执行 `ThemeCompiler.Compile` 并物化全部控件快照。切换使用 800×600 的无头窗口、10 个代表控件、Ant Design Daybreak Blue。未缓存模式在每次切换前清空主题快照缓存，清缓存和首次 Dispatcher 排空在计时窗外；计时窗内包含 `ApplyThemeAsync` 和尾部排空。正式数字适用于该 Ant 场景。Shadcn、PrimeNG 有候选侧跨语言样本，完整 Gallery Shell 的 JIT 启动仍缺同口径对照。计划中的稳态资源查询零分配由现有测试覆盖，和本表的冷编译分配是不同口径。[计时窗与宿主](../../../tests/AtomUI.Desktop.Controls.Tests/Theme/Performance/ThemeCompileMeasurement.cs#L202)

下表的两树中位是各自 7 个进程统计量的中位数。配对列取 7 对差值及相对变化各自的中位。分辨力按 R2 取两个进程范围中较大的跨度，以 M0 中位数为基准。其含义是这批对小幅差异的辨识能力。

| 指标 | M0 中位（范围），ms | 候选中位（范围），ms | 两树中位差 | 配对中位差 | 候选较慢 / 分辨力 | 判定 |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
| G1 默认冷编译 P25 | 1.1197（0.9563–1.2072） | 1.2494（1.0932–1.3135） | +0.1297 ms / +11.58% | +0.1701 ms / +15.08% | 6/7 · 22.41% | 偏慢，范围重叠，幅度未定 |
| G2 暗色冷编译 P25 | 1.1012（0.9845–1.2228） | 1.2942（1.1221–1.3770） | +0.1930 ms / +17.53% | +0.2110 ms / +18.67% | 7/7 · 23.15% | 每对偏慢，范围重叠，幅度未定 |
| G3 紧凑冷编译 P25 | 1.1086（1.0026–1.1514） | 1.2894（1.2536–1.3661） | +0.1808 ms / +16.31% | +0.2551 ms / +23.23% | 7/7 · 13.42% | 范围分离，确认退化 |
| G4 未缓存明暗往返 P50 | 12.8609（12.2133–16.0571） | 13.5044（12.9360–18.6350） | +0.6435 ms / +5.00% | +0.9634 ms / +7.88% | 6/7 · 44.31% | 两种汇总均高于 2%，范围重叠，门槛未通过 |
| G5 已缓存明暗往返 P25 | 11.3589（10.7761–11.6783） | 11.8038（10.7459–14.2237） | +0.4449 ms / +3.92% | +0.4449 ms / +3.92% | 5/7 · 30.62% | 偏慢，范围重叠，幅度未定 |

**分配收益稳定。** 14 个进程的冷编译分配中位数在各自树内完全一致。默认 `1,203,448 → 710,840 B`（−40.93%），暗色 `1,208,640 → 716,648 B`（−40.71%），紧凑 `1,203,832 → 711,048 B`（−40.93%）。这是编译线程在计时窗内的分配量。CPU 耗时和分配量是两个独立指标。

**环境影响了精度与早期方向。** 正式批次进程启动时的一分钟负载为 8.01–200.29。第 3–7 对的候选启动负载均低于同对基线，紧凑仍分别慢 26.50%、8.88%、14.03%、17.22%、23.23%。启动负载单独解释不了 G3；同时这只是开跑时的读数，无法复原每次采样中的线程调度。默认分层短批次的未缓存往返从 −2.81% 翻为 +5.47%，主要是两批 M0 中位数由 47.83 降至 43.46 ms，候选由 46.53 降至 46.16 ms。默认分层的四对 31 次批次在较忙时为 +47.37%，在负载 11.42–18.03 时为 −5.38%，后一批候选在前 15 次四对均慢、后 16 次均快。早期冷编译的改善来自短批次的 P50；正式 G1–G3 使用 R2 的 P25。各批数据保留，正式验收使用固定口径。原始诊断见 `/private/tmp/atomui-g-performance-final.qAHzRW/` 和 `/private/tmp/atomui-low-load-switch-gate.BjZemB/summary.txt`。

### 冷编译开销

**已验证的部分是每个带 Token 控件推导时的读集采集。** 同一个临时 Core 只切换采集，三档各两对。下表是从原始逐进程数据补核的结果。两对及三档的取值哈希相同，实际生产路径仍保留读集。原始对照在 `/private/tmp/atomui-compact-readset.tMXyiM/`。

| 场景 | 配对 1 P25（开启 → 关闭），ms | 配对 2 P25（开启 → 关闭），ms | 编译分配（开启 → 关闭） |
| --- | ---: | ---: | ---: |
| 默认 | 1.4022 → 1.2894（−8.04%） | 1.3866 → 1.3183（−4.93%） | 710,840 → 625,456 B |
| 暗色 | 1.4292 → 1.3323（−6.78%） | 1.3773 → 1.3340（−3.14%） | 716,648 → 630,472 B |
| 紧凑 | 1.4212 → 1.3045（−8.21%） | 1.3986 → 1.2925（−7.59%） | 711,048 → 625,664 B |

生成的 Token getter 在采集期间把名称写入每个控件的 `HashSet<string>`，`FromNames` 再用缓存的名称到槽位索引把已读名称写入位集。紧凑两对每次减少 0.1061–0.1167 ms，三档每次都减少 85,384 B。该成本出现在三档共用的控件推导路径。两对运行与正式 R2 的负载和采样不同，不能把两个批次的差值直接相减作为修复后的门槛结果。关闭开关也会失去基于读集的局部精确复用，取值相同只验证了这批冷编译输出。[控件编译](../../../src/AtomUI.Core/Theme/Compilation/ThemeCompiler.cs#L753) · [getter 采集](../../../src/AtomUI.Core/Theme/DesignTokens/AbstractDesignToken.cs#L19) · [读集构建](../../../src/AtomUI.Core/Theme/Compilation/GlobalTokenReadSet.cs#L16)

**配置准备已排除为 G1–G3 的计时来源。** 从 14 份原始记录重算的独立 `prepare` 两树中位数，默认 `0.0329 → 0.0226 ms`、暗色 `0.0492 → 0.0277 ms`、紧凑 `0.0488 → 0.0286 ms`。对应分配分别 `1,632 → 744 B`、`3,704 → 1,424 B`、`3,704 → 1,424 B`。两侧都是 191 控件槽、118 个待物化控件，全局槽位 257 → 258。诊断应优先看编译及物化做了哪些额外工作，三档的绝对中位差接近，应优先排查共用路径。[计时窗](../../../tests/AtomUI.Desktop.Controls.Tests/Theme/Performance/ThemeCompileMeasurement.cs#L202)

**剩余成本的源码假设。** 新流水线在控件推导前装配种子写入保护、记录接受写入，并通过 `ControlViewPool` 租还全局视图。多数控件没有改写全局时可以复用视图，省去了 M0 的逐控件拷贝，但仍需执行检查和状态清除。冷编译还包括根求值、创建惰性槽、逐控件派发与值/资源冻结。带控件缓存的切换路径还会为缓存键查询推导描述符，实际编译时再解析一次；当前索引按槽位查找，调用级别的计时缺失。这些是待分段测量的候选成本。现有 A/B 只量化了读集的整体成本，尚无法给其他操作分摊耗时。[控件视图池](../../../src/AtomUI.Core/Theme/Compilation/ControlViewPool.cs#L11) · [惰性编译和缓存](../../../src/AtomUI.Core/Theme/Compilation/LazyControlSnapshots.cs#L70)

### 从算法解释耗时与分配为何朝不同方向变化

| 路径 | M0 | 当前实现 |
| --- | --- | --- |
| Ant 根求值 | 通过旧算法链处理默认、暗色、紧凑 | 默认 Map 一次，按轴追加 Compact 或 Dark，然后执行 Alias 一次。Ant 的 `DefineSeeds` 和 `DeriveLanguage` 回调为空，Core 仍创建语言 Token，处理冻结并保留语言值表 |
| 控件全局视图 | `CompileControl` 为每个待编译控件复制根全局 Builder，包括场景不需要改写的控件 | 没有独立求值的控件优先从池中取视图。未配置覆盖且推导未写全局时可归还复用。接受全局写入的视图会丢弃，按槽生成它与根的差量 |
| 控件读写 | 旧生成 getter 直接读字段。M0 在无额外控件阶段的路径里调用控件取值逻辑，写入日志可以不启用 | 生成 getter 额外判断是否采集；开启时，每次全局 Token 读取调用 `HashSet.Add`，重复读取也要检查。结束后把不同名称映射到槽位再置位。种子写入保护和接受写入状态按每次控件推导装配 |
| 控件输出 | 按描述符冻结自有 Token 和资源 | 同样冻结两份输出。此步骤两侧都有，需先找到额外遍历或资源投影才能归因为新增开销 |

设一次冷编译物化 `N` 个控件，模式有 `G` 个全局槽，所有控件推导共读取 `R` 次全局 Token，各控件的不同已读名称累计为 `U`。M0 的逐控件视图复制随 `N × G` 增长。现实现的无覆盖、无全局写入串行路径可复用同一视图，但读集会执行约 `R` 次哈希集合操作和 `U` 次名称到槽位查找，另有每控件的集合和位集开销。现有 258 个全局槽需 5 个 `ulong` 保存一个位集，热点在采集与转换，最终按位比较最多检查这 5 个字。复制减少能解释分配下行；节省后的 CPU 时间仍取决于控件的读频次与其余新增工作。紧凑在 Ant 根派生只额外运行尺寸、字体、高度分支。三档冷编译同走上述控件路径，目前没有源码证据支持对紧凑专门改公式。[Ant 根派生](../../../src/DesignLanguages/AntDesign/AtomUI.DesignLanguages.AntDesign/AntDesignLanguage.cs#L33) · [轴分支](../../../src/DesignLanguages/AntDesign/AtomUI.DesignLanguages.AntDesign/Derivation/AntDesignMapDerivation.cs#L19) · [生成 getter](../../../src/AtomUI.Generator/DesignToken/TokenPropertyImplementationWriter.cs#L48) · [全局视图与资源冻结](../../../src/AtomUI.Core/Theme/Compilation/ThemeCompiler.cs#L689)

**读集的成本和收益发生在不同阶段。** 首次物化已经为记录付费，后续子作用域才有机会省掉整次控件派生和资源冻结。精确复用要求同一注册表版本、变体、定义和语言，语言值与色板相等，无种子变化，控件配置相同，父控件已经物化并有读集。比较时一次扫描全局槽生成变更位集，再按控件与其读集求交。原明暗往返改变变体，因此防护会拒绝上一个明暗快照之间的读集复用；新快照保留记录供将来的兼容子作用域使用。只改 `ColorTextLabel` 的作用域符合这些条件，才会得到局部收益。把开销从每次读取的字符串操作降到按槽置位，比直接丢弃精确复用更贴合目前证据。[复用条件](../../../src/AtomUI.Core/Theme/Compilation/ThemeCompiler.cs#L554) · [位集判交](../../../src/AtomUI.Core/Theme/Compilation/GlobalTokenReadSet.cs#L56)

**未缓存切换还有独立的资源求值链。** 旧 cell 在没有控件阶段和有效全局覆盖时可在编译前断定共享根值。新 cell 仅在物化后知道有效全局差量为空。于是只用全局角色 Token 的控件也可能在首次查询时触发锁、控件缓存查找、推导和资源冻结。若同一个控件随后本来就会查自有 Token，提前触发仅改变工作出现的位置。若控件只需共享 Token，才会净增加一次控件物化。这解释了绕过实验里冷访问大幅迁移但总数只小幅下降，以及该路径为何只能解释 G4 的一部分。要提前判定必须能证明注册的具体推导及其继承回退不会写全局 Map/Alias，且当前配置没有影响全局；仅凭语言 ID 或旧基线行为不足以证明。[资源查找](../../../src/AtomUI.Core/Theme/Resources/ThemeTokenResourceProvider.cs#L90) · [物化与缓存链](../../../src/AtomUI.Core/Theme/Compilation/LazyControlSnapshots.cs#L155)

**优化优先级。** 第一优先让已生成、可证明槽位映射一致的全局 Token 直接记录数值槽位，避免每个控件先建字符串集合再转换。代码必须保留正确的描述符映射与自定义 schema 的完整依赖捕获，避免因少记一次读取而复用错误的父值。第二优先检查热路径上可由同一次查找或状态装配复用的结果，例如缓存键与控件派生共同使用的描述符。根派生、资源冻结、事务发布分别看调用次数和数据规模，保持对全局覆盖以及各语言与变体的等价语义。仅在局部复用收益无法覆盖持续读集开销时，考虑按原计划退回到全局变更时保守重算。启动的新增配置 JIT 另看注册与初始化调用面，直接归到 Ant 调色或控件读集缺少依据。

### 未缓存往返的成本

| 因素 | 实验证据 | 当前判断 |
| --- | --- | --- |
| 发布期提前物化控件快照 | 默认分层采样记录发布栈 257→361 份，发布中物化 43→82 份，其中候选有 30 份落在 Effective Global 查找触发的 `GetValue`。单变量绕过在两对均把该入口的冷访问从 560 降至 0，自有 Token 的冷访问由 560 升至 1050，总和只减 70 次。未缓存耗时两对分别 +0.67%、−6.18% | 调用链真实存在，部分工作只是转移到下一次自有 Token 查询。该查找差异目前不足以解释 G4；任意控件推导仍可写入 Map/Alias，绕过只验证了临时 Ant 样本 |
| Owner 发布差量快路径 | 原明暗场景每侧 136 次因外观改变而做全量刷新，0 次可比较差量。单独启用快路径后无调用，刷新和解析计数相同 | 已排除该快路径对本场景的独立贡献。它依赖可比较的上一份快照 |
| Apply 与 Dispatcher 排空 | 原场景探针后 16 次 Apply 中位 M0 10.1220、候选 11.1117 ms，尾部排空 8.9929、9.1053 ms。由早段到晚段的下降主要出现在排空。该次负载约 228，整体耗时方向与正式批次相反 | 排空会放大早期走势。晚段 Apply 的约 +0.99 ms 是定位线索，当前无法外推至正式 G4 |
| 分层 JIT 与宿主调度 | 单变量关闭分层使快段提早；负载 66.4–80.66 时仍有两树走势差异。正式稳态关闭分层后 G3 和 G4 仍偏慢 | 默认分层与调度会改变短时采样结果，无法单独解释正式差异 |
| 冷编译、发布和其它路径的共同成本 | G4 含根快照失效与发布，G5 命中主题快照缓存也偏慢，正式两项均有范围重叠。读集 A/B 对 G4 的两对耗时方向相反 | 冷编译可能贡献部分未缓存延迟。两项使用不同分位统计量，差值无法相减成编译时间 |

调用栈采样只说明活动位置和样本数。探针依次覆盖准备与缓存未命中编译、事务提交、资源发布与惰性求值，以及尾部 Dispatcher 排空。Chart 新页、条纹示例和 Theme Presets 没有进入 10 控件宿主，共享图片修复也未被触发。诊断材料在 `/private/tmp/atomui-theme-switch-trace.Ph3shR/analysis-provider.txt`、`/private/tmp/atomui-root-global-diagnostic.NBMz4d/summary.txt`、`/private/tmp/atomui-owner-delta-diagnostic.N5AwOx/summary.txt`、`/private/tmp/atomui-apply-dispatcher-diagnostic.LOcnP2/summary.txt`、`/private/tmp/atomui-tiered-switch-diagnostic.pRbMXu/summary.txt`。[资源查找](../../../src/AtomUI.Core/Theme/Resources/ThemeTokenResourceProvider.cs#L90) · [全局值判定](../../../src/AtomUI.Core/Theme/Compilation/LazyControlSnapshots.cs#L155) · [发布差量](../../../src/AtomUI.Core/Theme/ThemePublishDelta.cs#L241) · [事务发布](../../../src/AtomUI.Core/Theme/ThemeManager.cs#L993)

### 原生 JIT 启动与局部复用

- **只启用 Ant 的启动偏慢可复现。** macOS 原生 JIT `atom-window` 最终 3 组各 10 次。M0、候选仅 Ant、候选三语言只使用 Ant 的首帧中位数分别 `835.096 / 883.230 / 876.072 ms`。候选仅 Ant 相对 M0 首帧配对 +58.220 ms，9/10 较慢，上一批 +58.194 ms。最大差值出现在 `Configure` 阶段，配对 +51.289 ms、10/10 较慢，其中该阶段 JIT 指标配对 +47.206 ms，编译方法 `269 → 978`。`Prepare` 配对 −9.357 ms，`Commit` +3.975 ms。配置与 JIT 的关联清楚，具体新增调用由哪些注册或初始化路径贡献仍待按方法归因。
- **启用但未使用额外语言的首帧差异未稳定。** 三语言相对仅 Ant 的首帧配对两批为 +2.104、−10.597 ms；最终批 `Configure` +9.500 ms、10/10 较慢，阶段 JIT +8.031 ms、方法 `978 → 1277`，`Commit` −0.236 ms。首帧范围重叠，能确认额外配置工作，尚未观察到一致的首帧延迟。最终批启动时负载 22.18–26.73。样本来自最小窗口真实启动，已有 NativeAOT 样例验证是包消费功能证据。启动原始数据见 `/private/tmp/atomui-g-performance-final.qAHzRW/paired-summary.txt`。[启动测量入口](../../../tools/performances/AtomUI.StartupPerformance/Program.cs)
- **局部重用已有收益。** 最终候选 Gallery 场景各 15 样本、3 次预热，跨语言冷切换 0/15 命中、缓存切换 15/15 命中。固定外观或覆盖种子分别只重算一个控件，其余 208/209 复用。修改 `ColorTextLabel` 时，127 个带 Token 控件仅 3 个读取且仅这 3 个实际重算，82 个空槽另计。对应冷/缓存切换至 Shadcn、PrimeNG、Ant 的中位数分别 `11.6634/10.6301`、`11.5159/10.3891`、`11.0939/11.0541 ms`。桌面/移动形态冷求值 `2.1855/2.1492 ms`，固定外观和覆写种子分别 `0.8376/0.8711 ms`，单 Alias `0.9487 ms`。这些是候选观测值和复用断言，桌面/移动形态是 Gallery 同机求值。需要保留这组收益作为读集取舍的对照。原始结果见 `/private/tmp/atomui-g-performance-final.qAHzRW/design-language-scenarios.txt`。

### 下一轮定位与结束条件

1. **先收敛冷编译。** 复用相同 Ant 默认/暗色/紧凑输入和冻结基线，先在原入口分别计时根编译与控件物化。只有测到控件段差值后才拆读集、写入保护和资源冻结。用同一临时驱动比较现实现与低开销读集方案。若局部重用收益仍抵不过成本，再比较设计方案允许的保守回退。每个候选同时核对三档取值、分配和 `ColorTextLabel` 重算成本及引用复用。
2. **再定位 G4 的剩余差值。** 按同一原宿主在安静负载窗口分段采样，分别观察未命中编译、发布及尾部排空，保留 G5 缓存对照。仅在分段支持新假设时改一处做 A/B。已排除的 Owner 快路径和已验证不足的 Effective Global 绕过停止重复试验。如果阶段差值随负载翻转且达不到 2% 分辨力，记录不可判定并保留阻塞。
3. **单独跟踪启动。** 若需修改启动，先从相同 `atom-window` 输入归因 `Configure` 新增 JIT 方法，区分语言/取值包注册、架构引入的通用初始化与首帧绘制，再用三配置复验。该项不替代 G3/G4，测试覆盖保持在已有启动目标。
4. **用最终产物复验。** 候选路径稳定后集中重跑原 R2。冷编译需按原口径复验三档，紧凑档须消除已确认的退化。未缓存往返需有足够精度通过已定 2% 门槛，同批核对缓存表现、分配与局部复用。M5-6、M6 和 M7 继续等待 M5-G。[M5-1 计划](../plans/2026-11-22-design-language-architecture-refactor.md#task-m5-1读集与结构共享性能验收)

## 像素例外清单

Ant Design 的逐像素门槛适用于规范化后的完整帧。任何新增的稳定渲染差异均须失败。

| # | 变体 | showcase 与区域 | 原因 | 基线与候选截图 | 批准 |
| --- | --- | --- | --- | --- | --- |
| 1 | Palette 使用同一命名空间标签的变体 | `palette` 的 `PresetPalettes` 头部 `Namespace` 一处文字 | `AtomUI.Theme.Algorithms` → `AtomUI.Theme.Derivation` 的目录迁移 | Ant Light Compact 原始复拍差 607 像素，边界 `(522, 224, 595, 238)`。候选抓图时统一这一文本后完整帧相同，尺寸仍 1300×2587 | 2026-11-23「批准这处标签差异」，请求 `call_5ff3fbf17b474dfe9d478527e46ac052` |

## Token 改名与迁移映射

与 `tools/performances/design_language_token_renames.txt` 保持一致（该文件在第一次需要时建立）。

| 旧 | 新 | 发生在 |
| --- | --- | --- |
| | | |

## 裁定记录

格式：`<任务 ID>：裁定——<决定了什么>——<原因>——<错了的代价>`。

- 总则：裁定——进度记录放在本文件，不使用 `.superpowers/` 工作目录——项目规则禁止写 `.superpowers/`，`docs/developments/` 只放 Markdown——无。
- 总则：裁定——执行过程中不提交，到门槛后停下评审再分组提交——项目负责人的既定工作方式——中途丢失工作区改动的风险由 worktree 独占使用来控制。

- M0-2：裁定——把「泵调度器直到任务完成」的 `WaitFor` 从 GalleryPerformance 的 `Program.cs` 移进共享的 `DesignLanguageVariants.cs`，两个工具共用——避免同一段等待逻辑出现两份——错了的代价：无，行为不变，M0-1 的调用点已同步改名。
- 总则：裁定——项目负责人 2026-11-22 要求「中途不要停，根据上下文选择最优的选项」，此后需要选择时自行决定并记入本节，不再停下请示；实施方案中写明的停点（M0-5 评审、M0-6 运行频率、各门槛）仍照常停下——停点是方案的一部分——错了的代价：若负责人本意包括这些停点，会多停一次。
- M0-2：裁定——控件状态比对沿用 M0-1 Step 3 的规则：同一构建两遍之间就会变化的类型（自带动画）不作为一致性判据，其余类型必须全部 identical——与项目负责人在 M0-1 的决定一致——错了的代价：动画类型上的真实变化会漏检，留待 M0-4 的动画遮罩改进处理。
- M0-2：裁定——ControlBaseline 状态截图固定画在白色底上，暗色变体肉眼看发白，不改现有工具——像素相等的判定不受底色影响，改动会让以前的截图全部作废——错了的代价：暗色变体的差异图人工复核时不直观。
- M0-2：记录——同一变体在 GalleryPerformance 与 ControlBaseline 里的指纹不同（例如 Maia 暗色紧凑 `85f0f003…` 与 `876c6143…`），因为两个工具注册的包与字体不同、指纹包含注册表。指纹只在同一个工具内比较。
- M0-4：裁定——给 `compare_snapshots.py` 加可选参数 `--ignore-near-animation`：离自带动画像素 `--near`（默认 6 像素）以内的变化像素照常列出但不计失败，更远的仍计失败；不加参数时行为不变。`run_perf_guards.sh` 通过环境变量 `SNAPSHOT_COMPARE_ARGS` 透传，`run_design_language_guards.sh` 默认带上——旋转图标、骨架屏等持续动画会在两遍截图的遮罩外漏出几个像素（M0-1 的 Button 页、M0-2 的 DateViewer 等），不处理则 48 个变体的全量守卫几乎每次都报失败；冻结动画要改产品代码，多截几遍只降低概率且成倍增加耗时——错了的代价：只出现在动画周围 6 像素内的真实变化会漏检，其背后的取值变化仍会被取值比对发现。
- M0-4：裁定——`run_design_language_guards.sh` 以环境变量 `ALLOW_ADDED_TOKENS=1` 给取值比对加 `--allow-added`，供新增 Token 的工作包（如 M1-1）使用；默认严格——实施方案要求这些工作包用 `--allow-added`，但脚本接口未写——错了的代价：无。

- M0-4：裁定——截图前冻结加载图标（`--freeze-loading-icons`，GalleryPerformance 与 ControlBaseline 共用 `SnapshotFreeze.cs`）：先把 `Icon.LoadingAnimation` 设为 `None` 并渲染一帧，再在下一批里把合成视觉的 `RotationAngle` 先设 0.001 再设 0，强制复位（两步分开：同一批里写角度时，渲染端可能先应用写入、后应用停止，Nova 暗色紧凑下按钮的加载图标仍停在相差几度的角度；分两批后 5 个按钮类型连拍 4 次、Gallery Button 页连拍 3 次中的同类情况均为 0 像素差异）——实测 `Icon` 停止旋转时设回 0 的那一步因客户端值本来就是 0 而没有发往渲染端，图标会停在随机角度；只设 `None` 时 Button 页仍有 38 个不稳定像素与 13 个变化像素，加上强制复位后连拍 3 次完全一致——错了的代价：工具里依赖合成属性的脏标记行为，Avalonia 升级若改变它，冻结会失效，表现为截图重新出现抖动，能立即发现。可选参数，默认关闭，原有用法不变。
- M0-4：记录——曾观察到 Gallery 整页截图的偶发抖动（某一行按钮下边缘与几条竖线的 1 像素抗锯齿差异），起初误判为可容忍的残余；Nova 亮色常规重跑时同一侧两遍截图都出现抖动导致误报，才查明是冻结本身造成的：冻结时也去碰了按钮模板里约 130 个隐藏的加载图标，使合成器只局部重绘这些区域，区域边界处的抗锯齿与整帧重绘不同。修正：只复位可见图标的角度，冻结后用1 像素的尺寸往返强制整帧重绘（两个工具都做）。修正后 Button 页连拍 6 次两两比对 15 组、5 个按钮类型连拍 4 次两两比对各 6 组，全部 0 像素差异（`.artifacts/m0/m0-4-determinism/`）。
- M0-4：记录——`AtomUI.Core` 的 `Icon` 停止加载旋转时角度不复位，是产品代码里的小缺陷；实际界面中停止旋转的图标通常同时隐藏，看不出来。未修改，留待项目负责人决定。
- M0-6：裁定——全量运行频率：单变体全部 showcase 约 12 分钟，48 个变体串行约 9 小时。(1) 每个工作包或控件族批次：
  受影响 showcase 跑全部 48 个变体，另加 `AntDesign Light Normal` 全部 showcase 一遍；(2) 里程碑门槛（M1-G、M3-G、M4-G、M5-G）：
  48 个变体 × 全部 showcase 完整跑一遍；(3) 守卫运行前把候选 Release 输出用 APFS 克隆（`cp -c`）复制成一份快照，守卫对快照
  运行，期间可以继续构建——每批 48 个变体跑全部页面要 9 小时，不可行；只跑受影响页面会漏掉「改了共享模板、影响到别的页面」
  的情况，所以另加一个 Ant 变体全页面兜底，门槛处再全量——错了的代价：一个批次误伤了非 Ant 变体的无关页面，要到下一个门槛
  才发现，定位范围是一个里程碑。
- 总则：裁定——项目负责人 2026-11-22 要求「继续把重构任务全部做完」：此后实施方案中的停点（M0-6 的运行频率、各里程碑门槛的评审）不再停下等待，由我按上下文决定并记入本节；门槛照常完整执行，通过后按变更组提交；未通过时先排查修复，不跳过——负责人明确要求一次做完——错了的代价：负责人可能希望在门槛处亲自评审，届时可从提交历史与本表逐项复核。
- M0-7：裁定——G-性能的判据：(1) 冷编译分配字节与基线比较，不得增加（它是确定值，可以精确比较）；(2) 耗时不与上表的绝对值比较，
  在门槛处把基线与候选放进同一次交错采样（启动用 `run_samples.py` 的两个变体与 `compare`，编译测量两边各跑两遍交替进行），
  看配对中位差——本机常有其他重负载，两遍之间的耗时相差可达一倍以上，跨时间的绝对值比较没有意义——错了的代价：交错采样
  仍受负载影响，小于约 25% 的耗时退化可能被噪声掩盖；分配字节比较能覆盖其中由多做事情造成的那部分。
- M1-1：裁定——Token 名照实施方案；FloatButton 的「是否有描边外观」本身也是开关，记录类型为空即 Ant 行为，所以新增
  `HasOutline`；记录里的行高与已有 Own Token `DescriptionLineHeight` 重名且语义不同（它是应用没有显式设置
  `DescriptionLineHeight` 时的取值），命名为 `OutlineDescriptionLineHeight`——一对一照搬会与已有 Token 冲突——错了的代价：
  名字在 M5 前还可以改，改名记入迁移映射。
- M1-1：裁定——控件从快照直接读这些 Token（`ThemeTokenResolver.GetOwn`，在快照变化时读一次），不改成 ControlTheme 里的
  资源 Setter——原来就是在同一时刻从快照读对象，这样读取时机与值的优先级都不变；Button 的读取结果按快照缓存，避免每个按钮
  重复查找——错了的代价：应用经资源字典（不是 `ThemeConfig`）覆盖这些 Token 时不生效，与原来的对象一样，不算退化。
- M1-1：裁定——Shadcn 的禁用透明度直接写 0.5，不再引用 Desktop.Controls 内部的 `ButtonThemeInputs.DisabledOpacity`——
  语言包不该依赖控件包的内部常量，M1-4 也要去掉这种依赖——错了的代价：两处数值将来可能不同步，但它们本来就是两个设计语言
  各自的取值。
- M1-4：裁定——M1-4 提前到 M1-3 之前做——实测只有一处依赖，提前去掉后 M1-3 新写的语言包代码若误用内部 API 会立刻编译报错，
  比最后再查更早发现——错了的代价：无。- （已撤回）M1-2b：裁定——契约的具体形态：(1) 契约是标 `[ControlTokenContract]` 的接口，成员由生成器为实现者生成，实现者不重复
  声明——生成器只认基类链，若要求实现者重复声明，15 个输入控件各要重写十几个属性；(2) 每个成员必须带 `[GlobalFallback]`，
  它同时给出缺省值、无 owner 与 owner 未实现契约时的取值——部件今天读的正是这些全局 Token，回退保持原行为，Ant Design
  无需推导代码；(3) AXAML 用生成的 `<契约>TokenResource` 扩展而不是给 `OwnerTokenResource` 加参数——保留枚举键的 IDE
  智能提示（项目约定）；(4) 解析到 owner 的原始 Own 枚举键，与 `<控件>TokenResource` 一致，作用域覆盖照常生效——
  错了的代价：生成器与 Core 的改动在 M2 新管线里要再接一次；契约形态本身与 M2 无关。
- M1-1：裁定——门槛中两个 Ant Design 失败都是截图时机问题，不是产品变化：DateViewer 禁用态的颜色过渡在机器繁忙时
  6 轮稳定截到中途（40 轮后两边逐字节相同）；Select 页聚焦输入框的光标闪烁（单页复测四次全部相同）。处理：守卫默认稳定
  40 轮；冻结步骤同时停止光标闪烁（`CaretBlinkInterval` 置零并显示光标）。基线 worktree 只替换守卫工具源文件后重建工具，
  产品代码仍是 `9f54cdf2a`——两边截图工具必须一致，冻结才对称——错了的代价：无，基线的产品程序集不变，可用文件哈希核对。- （已撤回）M1-3-F1：裁定——按钮代码（`Button.Variants.cs`）按按钮身份读新成员（扩展既有的身份映射），不改为按 owner 的契约读取；
  只有被多个身份共用的 AXAML（`ButtonTheme`）与焦点装饰器（原本就按 owner 读）使用契约读取——按钮内部的子按钮（如
  SplitButton 的两段）可能继承别的 owner，代码一直按自身身份读，改成按 owner 会改变行为——错了的代价：映射表多 45 行。
- （已撤回）M1-3-F1：裁定——SplitButton 的新 Token 与按钮族同名（`ButtonBorderRadius*`、`WaveSpiritEnabled`），不用清点表建议的
  `SplitButtonBorderRadius*`——按钮类控件共用一套词汇，主题与配置更易理解——错了的代价：无，名字在 M5 前可改。
- （已撤回）M1-3-F1：裁定——`LinkColor` 用 `Color?`，与全局 `ColorLink` 种子一致——不可为空会把「未设置」变成透明色，渲染会变——
  错了的代价：无。- 总则：裁定——项目负责人 2026-11-22 要求「请把所有的规划的任务全部完成，中途不要停，也不要 commit，如果需要选择你根据
  上下文选择最优的选项」：各门槛照常完整执行并记录，但一律不提交，改动全部留在工作区，最后统一由项目负责人评审后分组提交；
  需要选择时自行决定并记入本节——负责人的明确指令，取代此前「门槛通过后按变更组提交」的裁定——错了的代价：工作区改动量
  很大，中途出问题时回退范围大；以 worktree 独占与每个门槛的验证结果控制风险。- M2-0：裁定——冻结按 Token 名判断：生成的 setter 为 `if (IsFrozen(nameof(X))) return; field = value;`，编译器按描述的
  名字冻结。不引入按槽位的冻结表——抽象中间层的同一属性在不同终端类里槽位不同，按槽位要为每个终端生成映射表；名字是
  生成代码里的字面量，按引用比较即可，没有冻结时只是一次空判断——错了的代价：有冻结项时多一次小数组扫描。
- M2-0：裁定——读集（M2-1b）推迟到 M5-1：先按「全局一变、带控件 Token 的控件全部重算」实现，M5-1 测量后再决定是否加
  读集——方案第 18 章待定点 7 本来就允许退回；先不加就少一套机制——错了的代价：主题切换时多重算一些控件，M5-1 会量出来。
- M2-0：裁定——M2-3 在现有编译器上做减法，不另写新编译器：保留快照、相对全局的差量、惰性控件槽与两级缓存（方案 6.5 要求
  原样保留），只去掉算法链、分阶段求值、写入记账与重跑——改动面最小，运行时发布层的行为天然不变——错了的代价：无。

- M2/M3：裁定——M1 门槛的全量像素运行（`m1-gate`）在跑完 26 个变体后停止。理由：失败页面只有 alert、borderbeam、carousel、datagrid、skeleton，逐一看过差异图，分别是滚动字幕、动画相位、随机行数据，无一处渲染变化；M1 的树已并入 M2/M3 的改动，不再单独存在，M4 会对 48 个变体全量重跑——错了的代价：M1 单独的像素证据只覆盖 26 个变体。
- M3-0：裁定——M3 不另写步骤级计划，直接按实施方案的 M3 行执行；公式剥离与取值包由 FormulaConverter 机械产出——错了的代价：无。
- M3：裁定——`AddDesktopControls()` 等取值包入口只登记推导，不再调用 `UseDesktopControls()`。理由：方案 8 规定控件包由应用启用、`Add…()` 只登记取值；原实现在控件包已启用时抛「already registered」（ControlBaseline 启动失败暴露）——错了的代价：无，应用本来就要调用 `Use…()`。
- M3：裁定——GalleryPerformance 的变体检查让解析器路径与菜单路径用同一主题：菜单保留 Gallery 的命名主题（Ant 的 DaybreakBlue）时，解析器请求也选它。理由：两条路径取值相同（G-取值已证）但定义不同、指纹不同——错了的代价：无。
- M3-4c：裁定——控件级只写 Map/Alias 时不再重算 Alias（`GlobalAliasExplicitOverrideTests` 按方案 6.4 改写）。旧测试依赖已删除的控件算法模式 `Global`——错了的代价：依赖旧 `Global` 模式的控件级配置效果改变；方案 7.5 已说明。
- M3-4c：裁定——未物化的控件单元在发布差量里按「已变」处理（`ThemePublishDeltaTests` 改写）。理由：控件推导可以写 `view.Global`，物化前无法断定它是否沿用根值；物化后按值比较——错了的代价：发布差量变大，M5-1 量测。
- M3-4c：裁定——测试工程用 `TestDesignLanguages.AddAntDesign()`／`AttachWithAntDesign` 启用 Ant Design（方案 14「一行启用的测试入口」）；`AtomUI.DesignLanguages.AntDesign` 对 `AtomUI.Core.Tests` 开放内部成员，以直接测 Alias 公式——错了的代价：无。
- M3-4c：裁定——`ThemeCatalogReloadResult.AvailableThemes` 改名 `Themes`，与 `IThemeManager.Themes`、`ThemeCatalogChangedEventArgs.Themes` 一致——错了的代价：公开 API 改名，记入 API 变更说明。
- M3-4c：裁定——删除只测已删除机制的测试：算法契约自测、内置算法契约、Alias 阶段、控件算法编译与登记、算法登记与身份、写入记账、显式位（Global 与控件 Token 两处）、目录的设计语言分组与品牌色降级、`IsDefault`。其中仍有意义的行为改由新测试覆盖：`DesignLanguageCompilationTests`、`DesignLanguageRegistryTests`、`DesignLanguageContractTests`、`TokenFreezeTests`——错了的代价：无。

- M2-6c：裁定——构建期完整性检查只覆盖派生控件包自身声明的 Token 属性；基类在别的包里声明的属性由那个包的共享推导写，本编译看不到，交给契约测试库在运行时检查（Ant 已全部通过）——错了的代价：第三方漏写基类属性要到契约测试才发现。
- M2-6d：裁定——「取值包不含控件」只对自身不声明控件 Token 的编译生效；GalleryBase 这种自带通用推导的控件包豁免。控件包引用语言包（同一编译里既有控件又有某语言推导）判为违反依赖方向（方案 3.3）——错了的代价：无。
- M2-5e：裁定——换写命令缺省就地改写、删除被合并的变体文件，另可 `-p:AtomUIThemeDefinitionOutput` 输出到别处。理由：一次性换写在版本库里进行——错了的代价：用户未提交的 v1 文件被替换。
- M2-5f：裁定——用户目录里有任一无效文件（含 v1）时整个用户目录不生效，沿用现有规则——错了的代价：一个旧文件挡住同目录其他主题。
- M3：裁定——`IconToken` 构造函数里的取值移入 Ant 的 Icon 推导；另有 13 个 Ant 推导原本不写的 Token 按基线值显式写入（契约测试发现）。取值不变（基线转储为证）——错了的代价：无。
- M2-5d：裁定——编译主题 ABI 编号取 4（1–3 已用过），方案 7.10 的「ABI v2」指第二代格式——错了的代价：无。
- M2-5d：修复——编译主题里的 `<LanguageTokens>` 在应用链接清单中原本无法解码（作用域 `language`），解码与链接器已支持并有测试（`CompiledThemeManifestTests`）。
- M4：裁定（已推翻）——曾把 `ColorNeutral` 暂留核心。项目负责人要求不考虑兼容，按方案 5.3、16.1 迁为 Shadcn 的语言种子 `BaseColor`（见下文）。

- M5-3a：裁定——样例语言做成派生自 Ant Design 的品牌语言（方案 5.6、3.3「派生语言引用基语言除外」），而不是第二个独立语言。理由：独立语言只用公开 API 已由 Ant Design 语言包本身证明（核心不对它开 InternalsVisibleTo）；第三方最常见的是派生语言，样例要检验的是这条路——错了的代价：独立语言的第三方写法缺一个样例。
- M5-3a：裁定——派生语言复用基语言的种子、语言 Token 与全局推导，方式是持有基语言实例并以公开的 `DerivationContext` 构造函数委托，不为此给 Ant Design 增加公开 API；基语言的取值包用 `new DesignLanguageBuilder<AntDesignLanguage>(root)` 登记而不启用基语言——错了的代价：无，都是现有公开 API。
- M5-3a：修复——样例暴露生成器缺陷：ATOMUIGEN052 要求派生语言的取值包写全全部控件，与方案 5.6「只重写不同的部分」矛盾。改为对派生语言（语言 Token 的基类不是 `LanguageDesignToken`）不做构建期全覆盖检查，缺的推导由启动时沿继承链的覆盖校验发现（`A_Derived_Language_Value_Package_Covers_Only_The_Controls_It_Changes`，Generator.Tests 1120/1120）。
- M5-3a：裁定——样例工程放在 `tests/fixtures/DesignLanguageParity/` 下（计划写的是 `tests/fixtures/Sample.*`），与已有的 `LanguagePackEndToEnd` 夹具同级成组——错了的代价：无。
- M5-4d：裁定——迁移指南以 6.2.15 为起点（`v6.2.15` 是 HEAD 的祖先，中间的算法策略化从未发布），只写 6.2.15 到现在的净变化，算法阶段 API 不再出现——错了的代价：无。

- M4：裁定——Shadcn 与 PrimeNG 由两个并行代理各自移植（只写各自目录与测试工程），主会话负责核心改动与集成——理由：两种语言互不依赖，项目负责人要求并行——错了的代价：无。
- M4：裁定（已推翻）——曾允许控件推导在视图里写种子。项目负责人 2026-11-22 定为严格按方案 6.1 正文：视图只能覆盖 Map 与 Alias，写种子抛出（见下一条）。
- M4：裁定——两个语言测试工程加入核心的 InternalsVisibleTo，与其他测试工程相同；语言包与取值包仍不在其中——错了的代价：无。
- M4：裁定——Shadcn 的 `ColorLinkHover`/`ColorLinkActive` 只在 `ColorLink == ColorInfo`（即未显式设置 `ColorLink`）时改写，代替旧代码的 `IsConfigured` 判断；唯一差异是应用显式把 `ColorLink` 设成与 `ColorInfo` 相同的值时会被当作未设置（已写入 README 不支持清单）——错了的代价：这一极端配置下链接色变为主色。
- M4-5a：裁定——删除旧包时其中未提交的适配改动随之丢弃（删除前未先评估）；这些代码已整体退役，新包按 HEAD 移植，HEAD 仍保留原版——错了的代价：无功能影响。
- M5-1：裁定——读集只记在每个控件独占的全局视图上；语言 Token 对象由控件共享、可并发求值，不记读集，语言 Token 一变全部控件重算；种子或调色板变化同样全部重算（方案 12.3 的保守子集）——错了的代价：只改语言 Token 的局部作用域少一次复用。
- M5-1：裁定——同一作用域的控件共用一份推导起点的全局视图，推导没有写全局的控件把它还回去给下一个控件用；控件推导不写调色板（已核对全部推导）——错了的代价：若日后有推导就地修改调色板对象，后续控件会看到修改；需要时改为复制调色板。
- 总则：裁定——项目负责人 2026-11-22 要求「先重构完再测试」：剩余改动只做编译检查，全量测试与 48 个变体门槛在全部改完后统一运行——错了的代价：问题集中在最后暴露。
- M6、M7：说明——计划写明在 AtomUI.Mobile 与各产品仓库另立计划，不在本 worktree 内执行。



- M4-5c：裁定——移植跳过测试的代理由主会话接手（项目负责人要求「亲自做、最大程度并行」）；接手时 36 项跳过已全部去掉，审查其改动：DataGrid 控件级种子测试改为只比较框内区域并断言框自身 `ColorPrimary` 不变（方案 7.5 的已知行为变化），控件级 Alias 不再重新派生（方案 6.4），XML 夹具按 v2 换写（`BrandColor="Fixed"` → `Locked`），其余为 API 换写，期望值未改——错了的代价：无。
- M4：修复——放开视图写种子后，控件级配置的显式种子在视图里没有冻结，推导写种子会盖掉它（FloatButton 的三项 Token 效果测试失败）。改为控件级配置的全部显式全局值都在推导前冻结，种子也一样（`A_Control_Level_Seed_Survives_A_Derivation_That_Writes_The_Seed` RED→GREEN）。
- M4：裁定——语言缺省条目 `$<Lang>` 的 `ThemeInfo.AccentColor` 取该语言缺省变体下 `DefineSeeds` 的 `ColorPrimary`；`ThemeInfo.SeedValues` 给出主题自己的可调种子值（全局与语言种子；语言缺省条目取 `DefineSeeds`），Gallery 据此判断「选回主题自带颜色即清空选择」——错了的代价：多一个公开属性。
- M4：裁定——Shadcn 与 PrimeNG 不带具名主题（项目负责人：不考虑兼容）；Gallery 主题预设页因此只列 Ant Design 的具名主题与夹具，与基线不同，属于按计划的内容变化——错了的代价：无。
- M2-6f：裁定——删除 `OwnTokenReferenceAnalysisTests` 中 7 项测 Token 类内 `CalculateTokenValues` 推导链的测试，随所测机制删除——错了的代价：无。

- M4：裁定——严格按方案 6.1 正文（项目负责人选定）：控件视图只能覆盖 Map 与 Alias，写种子抛出。方案 6.1 示例与 4.2「控件 Token 不得只给全局换名」合起来，使「某控件上的值与全局不同」在种子层无合法表达；取舍为：模板、共享基元与控件代码中读种子的位置改读表达角色的 Map/Alias Token 或控件 Token，Ant Design 取原种子值。新增 Map `BorderRadiusMD`、Alias `InputActiveBorderColor`；控件 Token `LinkColor`、`DangerAccentColor`（按钮族）、Alert 四个状态图标色、MessageCard `LoadingIconColor`、Tag 四个状态色、ToolTip `ToolTipFontSize`/`ToolTipMinHeight`。清点方法：探针编译全部变体，列出视图里与全局不同的种子（Shadcn 24 对、PrimeNG 25 对），逐对找读取方；无读取方的（ToggleSwitch、NotificationCard、Alert 的 ColorInfo、候选列表选中标记——该列表不画标记）只在推导里改用局部值。验证：两种语言再探针为空；Shadcn 416 份、PrimeNG 48 份转储 0 改变 0 缺失；测试 135/135、33/33——错了的代价：全量像素门槛未跑完之前，模板改读新 Token 的位置可能有遗漏。
- M4：修复——控件级配置的显式种子在视图里冻结（之前只冻结 Map 与 Alias）；控件级种子使控件自成求值单元，其全局已带该种子。
- M4：裁定——`ColorNeutral` 移出核心，Shadcn 的基础色是语言种子 `ShadcnToken.BaseColor`（可调种子 `ColorPrimary`、`BaseColor`）；Gallery 中性色选择改为 `WithLanguageSeed("BaseColor")`；v1 换写把 `ColorNeutral` 写进 `<LanguageTokens>` 的 `BaseColor`。取值门槛把基线的 `G ColorNeutral` 列为例外键（`tools/performances/design_language_token_exceptions.txt`），其效果由全部派生 Token 比对——错了的代价：无。
- M4：裁定——删除 `IThemeManager.DEFAULT_THEME_ID`（核心写死了 Ant Design 的主题 id），改为 Ant Design 语言包的 `AntDesignThemes.DaybreakBlue`；核心在首个主题提交前回退到第一个语言缺省条目。`ApplyThemeAsync(ThemeRequest)` 保留：它承载完整运行时配置（含控件级取值），`ThemeSelection` 表达不了——错了的代价：无。
- M4：修复——注册夹具（`tests/AtomUI.Registration.Fixtures`）不在常规构建里，仍在用已删除的算法链 API、未启用任何语言；改为启用 Ant Design，ThirdParty 夹具的 Token 初始化器改为通用推导（方案 4.2 不允许初始化器）。
- 总则：裁定——项目负责人要求「所有重构都不考虑兼容」，此前因兼容或迁移成本做的折中逐项按方案改正（视图种子、`ColorNeutral`、具名兼容主题、`DEFAULT_THEME_ID`）。

## 变更日志

- 2026-11-22：建立本文件，任务总表覆盖 M0–M7 共 146 项。开始 M0-1。
- 2026-11-22：M0-1 的实现（新建工具工程 `tools/validation/AtomUI.DesignLanguageBaseline`，并擅自改动了实施方案的两处做法）
  未经项目负责人确认，按其要求全部撤回：删除该工程与拆出的测试文件，`AtomUI.Core.csproj`、`AtomUI.Themes.Tests.csproj`、
  `PrimeNGThemePackageTests.cs`、`ThemeSnapshotDump.cs` 恢复为基准树内容，清掉其构建产物。M0 暂停，基线做法待确认。
  此后凡实施方案未写到、需要自行决定的地方，先停下请示，不再自行裁定。
- 2026-11-22：项目负责人确认 M0 的基线做法并要求按提案修订：不新建工程、不在仓库里存基线文件；基线是 M0 工具改动提交后
  的一份 Release 构建（独立 worktree），每次验收现场对比；像素复用 `run_perf_guards.sh` 并补变体参数，取值在
  GalleryPerformance 里导出、用新脚本比对；种子组合共 484 份，只用于取值比对；全量运行频率待 M0-6 量出耗时后再定。
  实施方案 M0 一节重写为 M0-1…7，全局约束、门槛定义、文件结构与 M1-1、M2-3、M3-4、M4-1、M4-5 中的相关引用同步修订；
  本表 M0 由 5 项改为 8 项，总数 149 项。
- 2026-11-22：M0-1 的切换后核对方式由项目负责人定为指纹比对（菜单路径与解析器路径的主题内容指纹必须相同），并要求
  **验收后删除**：实施方案新增 M5-6（M5 门槛通过后删除 `ApplyVariant` 里的解析器路径与指纹比对），本表 M5 由 13 项改为
  14 项，总数 150 项。另记：我曾把「如果选指纹比对……」误读为已经选定并先写了 Step 2 的代码，已向项目负责人说明。
- 2026-11-22：M0-1 完成。Step 3 按项目负责人决定改用无动画页面（breadcrumb、descriptions、separator）重做并通过；Button 页与 Tag 页因持续动画（加载图标、processing 标签）会被两遍截图的遮罩漏网，此问题留到 M0-4 列出改进做法请示。
- 2026-11-22：M0-2 完成。开始 M0-3。
- 2026-11-22：M0-3 完成。开始 M0-4。
- 2026-11-22：M0-4 完成。到达 M0-5 评审停点。
- 2026-11-22：M0-5 完成（两次提交）。开始 M0-6；M1-2 清点与之并行。
- 2026-11-22：M0-6 完成（单变体全量自检 PASS，721 秒；运行频率已裁定）。开始 M0-7；M1-1 代码同时编写。
- 2026-11-22：M0-7 完成，M0 门槛通过。开始 M1-1。
- 2026-11-22：M1-2 完成。M1-3 发现方案缺口（† 读取者），停下与项目负责人讨论。
- 2026-11-22：项目负责人选定共享 Own Token 契约。方案新增 4.3，实施方案新增 M1-2b，本表 M1 由 14 项改为 15 项，总数 151 项。
- 2026-11-22：M1-2b 完成。开始 M1-3 F1。
- 2026-11-22：项目负责人否决共享 Own Token 契约及其带来的镜像 Token（「复杂度已经无法维护」「不要再搞出这种东西」）。已撤回：契约机制的全部代码与测试、按钮族 F1 与输入族 F2 的改动、方案 4.3、实施方案 M1-2b，本表 M1 回到 14 项、总数 150 项。保留：M1-1 行为开关 Token 化、M1-4 去掉内部 API 依赖、守卫工具的冻结与稳定轮数修正。M1-3 的做法暂停，待重新讨论。
- 2026-11-22：方案修订（项目负责人确认）：Own Token 改名为「控件 Token」；新增「控件视图」概念——控件推导输出该控件看到的全局 Token 覆盖与它的控件 Token，与主题、作用域的控件级配置是同一份视图、同一条优先级；控件 Token 不得镜像全局 Token。实施方案删除 M1-3（`EvaluateGlobal` 原样留到 M2，由新管线取代），M1-2 清点表改作 M4 换写对照，M2-2 接口改为 `Derive(context, ControlView<TToken> view)`。本表删除 M1-3 的 6 项，M1 由 14 项改为 8 项，总数 144 项。
- 2026-11-22：M2、M3 的代码落地（核心新契约与单遍管线、XML v2、ABI 4、Ant Design 语言包与 4 个取值包、Gallery 与工具）。G-取值通过。测试工程移植中：Core 553/557、Generator 1104/1107（3 项跳过）、Desktop 4511/4511、DataGrid 336/336。
- 2026-11-22：M2、M3 完成（门槛证据见各行）；文档 v2 化；开始 M4。
- 2026-11-22：全量回归通过（数字同 M3-4c）；M5-3a 对等性样例完成；M5-4d 迁移指南初稿；Shadcn、PrimeNG 由两个并行代理移植中。
- 2026-11-22：M4 两种语言移植完成（取值门槛全过）；控件视图可写种子；M5-1 共享视图与读集；旧包删除；文档完成。
- 2026-11-22：按方案 6.1 正文改控件视图种子覆盖为角色 Token；`ColorNeutral` → Shadcn `BaseColor`；删除 `DEFAULT_THEME_ID`；注册夹具修复。

- 2026-11-23：基线 worktree 的 `12cc0dabf` 和台账原 `9f54cdf2a` 拥有相同的 Git tree `60d17a6420f1cb49f6856da7681adbee8fe1822c`。用现有 Release 产物复现 M3 AntDesign Light/Dark Normal 指纹不一致（历史工具 SHA256 前缀 `e18f910f6640`，两次退出 1），随后同入口核对较新候选（工具 SHA256 前缀 `403b88416e69`，两次退出 0）。基线与较新候选 `--dump-tokens` 各 484 份，比较 0 改变、0 缺失，新增键沿用已登记的比较口径。该候选早于本轮代码修改。
- 2026-11-23：像素守卫修复 4 种误通过情况，即量化丢失单通道差异、空图集、重复中尺寸变化和重复输出中独有图。原比较器四项错误退出 0，修复后四项均退出 1，全同控制组退出 0。捕获与泄漏进程非零退出也会导致 guard 失败，注入退出 23/24 的探针前 0 后 1。脚本语法与同构成功对照通过。亮色紧凑复验 Button 逐像素一致，BorderBeam 同构建复拍中运动区域变化，Palette 稳定差异见待决项。
- 2026-11-23：将新 3 个语言/对等性测试工程纳入回归入口，为 `src/DesignLanguages/`、旧包删除以及语言契约与对等性夹具补选测归属。原 iterate 报告选测缺失；更新后同范围规划退出 0，覆盖 11 工程及 68 个测试类。将已失效的旧预设缺席断言移除，将私有实现字符串断言移入具名主题菜单实际行为的既有用例。原 FinalVisual 临时渲染探针已移出测试项目。
- 2026-11-23：Shadcn、PrimeNG 的四种配方接入桌面取值。配方前后 Shadcn 416、PrimeNG 48 份转储共有键一致；PrimeNG 取消第一方 IVT 后 33/33 测试通过，全 48 份同 Schema 转储逐字节相等。Shadcn 配方相关 87 项通过。两种语言的临时用例已删除。
- 2026-11-23：取值门槛改在截图前运行。空目录与空转储各由错误退出 0 修正为退出 1。对注入取值变化的候选，守卫先比较两边 Token 并退出 1，未启动视觉子进程。原 ColorNeutral 例外可掩盖 Shadcn 移入语言槽位的值，候选工具改为从 `ShadcnToken.BaseColor` 读取并按 M0 的原键输出，删除该键例外。丢失锌色一项精确复现失败，最终 Release 工具复验待执行。
- 2026-11-23：当前验证策略静态审计退出 0（18 个测试工程、922 个测试类）。Gallery 聚焦以重建的 Release 运行 39/39，Core 既有 Debug 二进制聚焦 35/35；既有 Debug 渲染二进制的 PrimeNG DataGrid 和 TabOverflow 测试 78/78，M1 原六个失败场景复验 6/6。需以最终候选的清理后回归覆盖这批已知历史场景。
- 2026-11-23：正式性能采样包括原生启动 3 组各 10 轮、M0 和候选冷编译/明暗往返交错测量，以及独立 Gallery 场景 15 样本。Gallery 固定外观和种子各触发一个 Button 控件重算，208/209 控件快照复用；只改 `ColorTextLabel` 时 127 个带 Token 控件有 3 个读取并恰好重算这 3 个，82 个空快照另计。冷/暖切换分别 0/15、15/15 命中。M0、候选 Ant、候选三语言原生首帧中位数分别为 865.187、935.288、937.857 ms。Ant 相对 M0 配对首帧 +58.194 ms，Configure +50.773 ms（主要为 JIT，10/10 较慢），仍在实测 M0 的首帧区间 835.610–1036.657 ms 内。背景负载 22.33–24.40，采样期间本会话未并行构建。此项是最小宿主证据，未测 Gallery Shell JIT。
- 2026-11-23：`BuildNuGetPackages.ps1 -BuildType Release` 留存检查 36 个实际归档通过，独立复核退出 0。实际布局与 6.2.15 对照 21/21 匹配，15 个首次发布 ID 有 NuGet 404，核对包内 Generator 的 build-task isolation 通过。最小项目仅直接引用 Desktop.Controls 与其 Ant Design 取值包，NuGet 全新隔离缓存，11 个 AtomUI 依赖均来自本地 feed，正常构建和实际 Cocoa 窗口 Button 模板验证退出 0。NativeAOT 消费正在独立验证。
- 2026-11-23：最小包消费在 macOS arm64 NativeAOT 验收通过，产出 50,046,960 字节的 Mach-O 原生可执行文件，入口和 Button 模板验证退出 0。发布仅报 `AtomUI.Core` 聚合警告 `IL2104`，本次日志无具体调用归因。与普通应用共用的临时 NuGet feed、消费项目、独立缓存及探针均已清理。
- 2026-11-23：最终 Release 工具从现有源码构建（工具 SHA256 前缀 `5dd3d9c77ac7` / `24578960b5fe`），列出 48 个变体；取值 484 对，0 改变、0 缺失。Shadcn `BaseColor` 经旧键直接比较，无例外键。此前原始紧凑亮色守卫比对 225 个状态图无差异，基线与候选各有同样 4 种未捕获控件，未见新增或扩大泄漏。
- 2026-11-23：启用 `NORMALIZE_PALETTE_NAMESPACE=1` 后，工具仅在 `palette` 截图期间替换已批准的文字，其余像素照常比对。相同变体另外采样的同构建帧将 BorderBeam 原 147 个未遮罩像素降至 0，说明本次差异来自采样相位。`REPEAT_UNSTABLE_PAGES=1` 只对首次失败且已观察到运动的页面每侧最多多拍两帧。
- 2026-11-23：`scripts/run-full-regression.sh` 退出 0，运行 17 个默认项目。脚本总计 12,296，逐工程统计为 12,293 通过、0 失败、3 跳过（Desktop、Gallery、PrimeNG 各一）。跳过项是两个显式性能测量和一个按参数运行的取值转储，已分别有独立实测。默认没有运行 Localization 集成项目。全部 runtime 源码验证后仅继续完善英文 XML 文档与守卫工具。
- 2026-11-23：补拍目录按显式名单逐个核对，缺文件直接失败。状态图校验固定已知的 4 个 M0 采集失败及成功覆盖，页面和控件泄漏检验校验成功输出行及完整 JSON 清单。合成重复缺图、新增状态缺图、页面泄漏缺行、控件缺行及新增测量错误均以非零退出，完整对照通过。使用实测 Shadcn Nova 已通过新版归档完整性检查。
- 2026-11-23 用户裁定：请求 `call_c06d04437b234e17a0efd5cd9af00e74` 答复「保留 11 个语言预览入口」。验收覆盖原入口与顺序、默认 Aura 局部预览、风格/明暗/紧凑切换、v2 XML 和根主题隔离。现有具名主题数据与注册能力仍按 v2 契约。
- 2026-11-23 用户裁定：请求 `call_fba757e7a2e74b8b989b2ec7dd979a26` 答复「纳入共享加载器修复」。先用可控挂起 HTTP 复现共享源工厂长期持有已取消等待者，再修正源、解码、借用任务的请求捕获边界。验证取消、余下等待者的进度和结果、转换资源释放和原 Gallery 路径。
- 2026-11-23：共享图片加载器只让等待方持有自身进度观察者，源、解码和借用共享工厂使用不含观察者的规范化请求。受控 HTTP 传输实测 RED 指向已取消观察者仍被持有，GREEN 同时证明其它等待者收到 Downloading/Decoding 与结果。临时测试清理后 ImageLoading 146/146，独立评审无可归因回归。使用原冻结 Release 仅替换 `AtomUI.Controls.Shared.dll`（SHA256 `cd9a338d95f3` → `a6a4d97858c4`）复验 PrimeNG Aura 亮色常规，真实网络、三次访问、同序 76 页，全部 0 存活，含 Avatar 和 Masonry。
- 2026-11-23：守卫路由与 Gallery 注册表逐项核对后，工具已纳入所有 77 个独立 showcase。实施计划中的 92 个文件包含 15 个子视图文件，不能当作独立路由数。旧基线二进制保持原样，临时路由入口适配器只给同一抓拍工具补入 Theme Presets 路由以做历史对照。
- 2026-11-23：Theme Presets RED 2 个预期失败、GREEN 2/2，保留原 11 个风格标签与排序。默认 Aura；合成选择键使用 `$Language.Style`，展示及 v2 导出使用原可读 ID；具名 XML 仍可解析，Daybreak Blue 在无语言风格时后备。清理后 Release ThemePresets 20/20。原场景仅替换 Gallery 程序集（SHA256 前缀 `d6a7568f067b`），两侧各两帧 1300×1476，全部像素相等。
- 2026-11-23：M4 的配方余项完成。Shadcn 按上游默认 chartColor neutral 提供 300/500/600/700/800 色阶的五个可覆盖语言 Token；PrimeNG 条纹按 PrimeUIX 3.1.1 三个预设读取明暗表面色。各两条临时用例 RED→GREEN，覆盖风格、明暗与覆盖值；清理后 PrimeNG 16/16、Shadcn 11/11。两种新增角色未改变当前控件模板。
- 2026-11-23：在原 76 页守卫中，之前 8 个样本的页面像素、225 个可抓取状态图均通过。四次旧候选泄漏返回失败均落在 Avatar、Masonry 的 HTTPS 保留，已由另行批准的加载器修复在原路由复验。旧矩阵在用户裁定后停止追加，已在执行的五组自然收尾。
- 2026-11-23：Theme Presets 边界定向修复保留 v2 锁定种子，合成预览遇已占导出 ID 使用 `.Preview` 后缀，具名主题省略种子时使用实际语言默认值。6 个临时用例 RED→GREEN，删除临时测试和 XML 后 20/20。冻结最终候选的工具 SHA256 前缀 `ca9e1219c10e` / `e603e82acb84`，Gallery `a831e8f678fa`，共享加载器 `a6a4d97858c4`；G-取值 484 对全部等价。
- 2026-11-23：最终候选 G-性能重新交错采样。原生启动 30/30，M0、Ant、三语言首帧中位数 835.096 / 883.230 / 876.072 ms；Ant 相对 M0 配对 +58.220 ms，三语言相对 Ant −10.597 ms。冷编译三档 −20.14% / −27.39% / −33.55%，分配约 −40%。缓存切换 −9.00%，未缓存 +5.47%（两轮均较慢，与上一批相反），保留定位待决。Gallery 每项 15 样本，重用计数依旧通过。新 77 页视觉矩阵已启动首批，保留其结果，后续批次待本轮安静性能排查。最终全量测试已重新启动。
- 2026-11-23：最终冻结候选的全量脚本退出 0，17 工程共 12,296 项，逐工程 12,293 通过、0 失败、3 个专项跳过，18:19。首批 5 个视觉变体通过 77 页、225 张可采状态图和严格泄漏覆盖，包括全部四个 Ant Design 变体。PrimeNG Aura 再证 Avatar、Masonry 与全部 77 页零存活。剩余 43 个变体复用同一冻结产物继续采样。
- 2026-11-23：同一 10 控件、Ant-only 宿主的未缓存性能队列预先确定为 AB/BA/BA/AB，每侧 31 次×4。四对 +75.52% / +6.17% / +49.20% / +45.54%，中位 +47.37% / +6.8183 ms。各组前 15 个样本的配对中位 +11.83%，后段有交叠。缓存切换中位 +1.40%，独立冷编译三档 −28.79% / −10.86% / −11.45%。比对早期和最终候选同一路径的 ThemeManager、ThemeCompiler、ThemeSnapshotCache、控件与测量方法，反编译内容一致。新版 Chart、条纹与 Theme Presets 未进入宿主，共享图片修复未由用例触发。总耗时证据尚不能归因，进入定向运行时采样。
- 2026-11-23：PrimeNG Lara 亮色紧凑原始 77 页矩阵中只有 Avatar 像素失败，范围 `(960, 360, 995, 395)`、1005 像素。对应页面内置 `avares` 的 AntDesign SVG 尚未就绪。用原抓拍的 `lineedit,icon,avatar` 导航前缀在同一冻结二进制复拍，两侧三页均逐像素相等，包含 Avatar 原尺寸 1300×1555。候选在两次运行之间的差异也正好是原 1005 像素，加载完成的候选与原基线一致。该变体状态及泄漏覆盖已通过，保留原失败和定向复验证据。
- 2026-11-23：未缓存切换栈采样增加最大的阶段为发布，M0→最终候选 257→361 份，其中惰性物化 43→82。30 份增量定位至 Effective Global 查找的 `ControlSnapshotCell.GetValue`。同一临时 Core 开关对照将该入口冷访问 560→0，Own 冷访问 560→1050，合计仅减少 70 次。两配对耗时 +0.67% / −6.18%，无法由该入口单独解释门槛差。派生运行时差量为零，生产源、共享产物和冻结视觉候选均未改变。以同一临时方案核对发布差量快路径的独立贡献后再裁定。
- 2026-11-23：明暗切换的 `ThemePublishDelta` 每侧触发 136 次因外观变化产生的全量刷新，0 次可比较差量。单独启用 Owner 差量快路径未产生调用，两组 Owner 刷新和解析计数完全相同。假设至此停止，未新增公开元数据。
- 2026-11-23：最后一组原宿主 JIT 定向诊断只改变分层编译开关。关闭后低耗时区提早，候选和基线的持续走势仍不一致，背景负载 66.4–80.66。结果不能定位候选独有的 JIT 延迟，也不能用作 2% 门槛验收。未缓存切换仍待解决。
- 2026-11-23：原 xUnit 场景的最后一组定向阶段探针只修改临时测量程序集。两侧各 31 次未缓存切换，分段和与原计时吻合。M0 早/晚段 Apply 中位数 23.59/10.12 ms，尾部 Dispatcher 排空 51.26/8.99 ms；候选 Apply 15.84/11.11 ms，排空 36.56/9.11 ms。由慢段转入快段主要来自排空变化，后段候选 Apply 高约 0.99 ms。系统负载约 228，整体方向与正式配对相反，未定位正式门槛的可修正根因。生产程序集与冻结产物均未修改，本轮诊断停止。
- 2026-11-23：M5 本地 `6.2.15-dlp.2` 首次打包到步骤 23 时，23 个共享 NuGet assets 在约半秒内被 Gallery Desktop 单目标恢复重写为 `net10.0`，此前获验证的 `net8.0`、`net11.0` 不在资产内，NETSDK1005，未生成归档。验证隔离路径中，其他进程及共享目录保持原状。
- 2026-11-23：最终产物矩阵已核对 19 个变体，包含四个 Ant、十二个 PrimeNG 和三个 Shadcn。PrimeNG Lara 亮色紧凑的原始失败仍在记录，由同候选同导航前缀的定向复验补足。观测主机 1 分钟负载 170.20，现有抓拍组自然收尾后以三路并行继续余下样本。
- 2026-11-23 用户裁定：请求 `call_156091b8ff724221aa9c4f35b28a9267` 答复「维持 M5 前置门槛」。M6、M7 继续等待完整 M5 门槛，未缓存切换的 2% 判定保持不变。
- 2026-11-23：最终 `6.2.15-dlp.2` 的隔离恢复复验通过。原失败 DataGrid Release 构建退出 0，三个目标齐全；随后正常打包入口退出 0，36 个归档含 15 个设计语言产品。空缓存两直接包引用的最小应用验证 11 个 AtomUI 依赖全部取自本地 feed，常规 Cocoa 和 macOS arm64 NativeAOT 的 AntDesign Button 模板实测均退出 0。NativeAOT 仍仅输出 AtomUI.Core `IL2104` 聚合警告，未归因到源码点。归档唯一保留于 `.artifacts/dlp-feed-m5/6.2.15-dlp.2`，搬移前后文件名与 SHA256 完全一致，总清单哈希 `085f3588cc1a332b75b6db7915d5bca64eb0c4ae08af94d6a831b8452638fb1c`。临时测试应用、专用恢复资产和缓存已清理。
- 2026-11-23：负载 11.42–18.03 的默认分层交错批次，四对 31 次未缓存往返的配对中位 −5.38% / −0.7914 ms，P25 −3.09%。候选前 15 次各组仍慢，后 16 次各组较快。复核旧验收契约发现 R2 对正式稳态测量明确关闭分层 JIT，并要求冷编译 3001/300、切换 501/50、每侧至少七次交错。前述默认分层批次属于诊断。2% 门槛及固定统计量保持，当前正式补测使用同两个冻结输入。
- 2026-11-23：正式 R2 稳态批次 7 对交错全部退出 0。G1/G2/G3 冷编译 P25 配对中位分别 +15.08% / +18.67% / +23.23%，紧凑冷编译两树范围无重叠（M0 1.0026–1.1514 ms，候选 1.2536–1.3661 ms）。未缓存往返两树中位 12.8609→13.5044 ms（+5.00%），配对 +7.88%，分辨力 44.31%。缓存 P25 配对 +3.92%，三档分配均约少 49 万字节。启动时负载 8.01–200.29，保留全批。未达到 2% 门槛。
- 2026-11-23：紧凑冷编译同一个临时 Core 只改变是否采集读集。两对 P25 为 1.4212→1.3045 ms、1.3986→1.2925 ms，哈希完全相同，分配 711048→625664 字节。读集解释约 8% 的开销，尚不足以闭合正式门槛，其它切换数据在并行视觉负载下波动。生产源未改，优先完成剩余矩阵。
- 2026-11-23：最终冻结候选 48 个变体全部取得视觉结论。47 组原守卫通过，PrimeNG Lara 亮色紧凑用相同导航前缀补足内置 SVG 就绪证据，三页复拍均零差异。每个变体验证 77 个独立路由、225 个可采集状态图、页面及控件泄漏完整性。另 4 个 M0 已知采集失败类型仍相同。无新增或扩大泄漏。主仓剩余实际阻塞是 R2 性能门槛，未删除临时指纹比对，M6/M7 未开展研发。
- 2026-11-23 应用注册 API：Desktop / Browser Gallery 改为显式注册三种语言，生成的启动映像按控件包与语言匹配取值包。Ant Design 配置 Daybreak Blue 并作为初始语言。目录读取声明的种子，明暗指向不同语言时只在选定外观后编译。三项临时探针通过并清理，最终 Core 52、Generator 27、Gallery 28、Desktop 4、Rendering 36 项；两宿主 Release 构建无告警，真实资产检查 374 项。原 R2 结果仍是上一冻结候选的记录。[API 与求值边界](../../architecture/systems/theming/design-languages.md#应用注册与启动-api)
- 2026-11-23 应用注册 API 的 macOS arm64 Gallery NativeAOT 本地发布退出 0。从当前 checkout 内的独立运行目录按精确路径启动并观察到主窗口，冒烟进程与运行目录已清理。发布报告 ReactiveUI / ReactiveUI.Avalonia 的 IL2104、IL3053 和 `GlobalClockReflection` 的 IL2075。
