# 设计语言架构重构：任务总表与进度跟踪

- 架构方案：`docs/superpowers/specs/2026-11-22-design-language-architecture-design.md`
- 实施方案：`docs/superpowers/plans/2026-11-22-design-language-architecture-refactor.md`
- 分支与 worktree：`feature/design-language-products`，`/Users/chinboy/Projects/dotnet/AtomUIV6-design-language-products`
- 基准树：release/6.0 `8c946e4f5`

本文件是重构全程的唯一进度记录。每完成一项任务，在同一次改动里更新它的状态与证据；每做一个偏离实施方案的决定，写进
「裁定记录」。上下文丢失后以本文件为准继续，不凭记忆。

状态取值：`未开始`、`进行中`、`完成`、`阻塞`。「证据」写命令与结果，或产物路径；没有证据不得标 `完成`。

## 当前状态

- **所在里程碑：** M0 基线与护栏
- **正在做：** M0-5 评审停点：M0-1…4 完成，等待项目负责人评审 M0 改动并同意提交
- **下一个停点：** M0-5（M0-1…4 完成后停下评审；评审通过后提交一次，作为基线构建的固定提交）
- **最近更新：** 2026-11-22

## 里程碑总览

| 里程碑 | 内容 | 任务数 | 完成 | 状态 |
| --- | --- | --- | --- | --- |
| M0 | 基线与护栏 | 8 | 4 | 进行中 |
| M1 | 旧架构内的等价清理 | 14 | 0 | 未开始 |
| M2 | 核心：新契约与单遍管线 | 30 | 0 | 未开始 |
| M3 | Ant Design | 31 | 0 | 未开始 |
| M4 | Shadcn 与 PrimeNG | 39 | 0 | 未开始 |
| M5 | 主仓收尾 | 14 | 0 | 未开始 |
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
| M0-5 | **停下评审**；通过后提交 M0 的工具改动（重构中唯一一次门槛前的提交） | M0-4 | 未开始 | |
| M0-6 | 基线 worktree 与 Release 构建；单变体全量自检与耗时；**停下请示**全量 48 个变体的运行频率 | M0-5 | 未开始 | |
| M0-7 | 性能基线：编译与切换测量、启动采样，各两遍，记录分辨力 | M0-6 | 未开始 | |
| M0-G | **M0 门槛**：基线构建完毕、单变体全量自检 PASS、运行频率已定、性能基线已记录；停下评审 | M0-1…7 | 未开始 | |

## M1 旧架构内的等价清理

| ID | 任务 | 依赖 | 状态 | 证据 |
| --- | --- | --- | --- | --- |
| M1-1a | Button 族行为开关 Token 化（`ButtonThemeRecipe` 的七个属性） | M0-G | 未开始 | |
| M1-1b | SplitButton `InsetSeparator` | M0-G | 未开始 | |
| M1-1c | 输入族 `InputFrameAppearance` 字段 Token 化 | M0-G | 未开始 | |
| M1-1d | FloatButton `FloatButtonOutlineAppearance` 字段 Token 化 | M0-G | 未开始 | |
| M1-1e | 删除 `AppearanceSettings`；G-取值、G-像素 | M1-1a…d | 未开始 | |
| M1-2 | 逐控件全局改写清点表（22 个阶段类、125 处直接改写及辅助方法里的改写），每行定去向 A / B / C | M0-G | 未开始 | |
| M1-3-F1 | 消除改写·按钮族：`ShadcnButtonTokens`、`ShadcnFloatButtonTokens`、`PrimeNGButtonTokens` | M1-1e、M1-2 | 未开始 | |
| M1-3-F2 | 消除改写·输入与选择族：`ShadcnInputTokens`、`PrimeNGInputTokens`、`ShadcnCheckBoxTokens`、`PrimeNGCheckBoxTokens`、`ShadcnToggleSwitchTokens` | M1-2 | 未开始 | |
| M1-3-F3 | 消除改写·卡片与容器：`ShadcnCardTokens`、`ShadcnCardTabTokens`、`PrimeNGCardTokens`、`PrimeNGContainerTokens` | M1-2 | 未开始 | |
| M1-3-F4 | 消除改写·反馈类：`ShadcnDialogTokens`、`PrimeNGDialogTokens`、`PrimeNGFeedbackTokens`、`PrimeNGMessageCardTokens`、`PrimeNGNotificationCardTokens`、`PrimeNGToolTipTokens` | M1-2 | 未开始 | |
| M1-3-F5 | 消除改写·其余：`ShadcnEmptyTokens`、`ShadcnSimplePaginationTokens`、`PrimeNGDataDisplayTokens`、`PrimeNGMotion` | M1-2 | 未开始 | |
| M1-3-Z | 删除 `EvaluateGlobal` 成员与编译器路径；完整 G-取值、G-像素、G-回归 | M1-3-F1…F5 | 未开始 | |
| M1-4 | 去掉四行 `InternalsVisibleTo`，逐条处理编译错误，API 变更记入发布说明 | M1-3-Z | 未开始 | |
| M1-G | **M1 门槛**：四项门槛全过，Ant Design 零例外；停下评审并分组提交 | M1-1e、M1-3-Z、M1-4 | 未开始 | |

## M2 核心：新契约与单遍管线

开工前为本里程碑出步骤级计划：`docs/superpowers/plans/2026-11-22-design-language-m2-core.md`。

| ID | 任务 | 依赖 | 状态 | 证据 |
| --- | --- | --- | --- | --- |
| M2-0 | M2 步骤级计划 | M1-G | 未开始 | |
| M2-1a | Token 冻结语义：`Freeze`、`IsFrozen`，生成的 setter 检查冻结位 | M2-0 | 未开始 | |
| M2-1b | 读集：`TokenReadSet`、`BeginReadSet` / `EndReadSet`，生成的 getter 记录读取 | M2-1a | 未开始 | |
| M2-1c | `LanguageDesignToken` 基类与特性；生成器支持语言 Token 类（属性实现、Schema、种子标记） | M2-1a | 未开始 | |
| M2-1d | 删除 `CalculateTokenValues`、`CalculateAliasTokenValues`、`IsExplicit`、`RecordWrite`、`TokenWriteLog` | M2-1a…c | 未开始 | |
| M2-2a | 语言契约类型：`DesignLanguage<T>`、`DesignLanguageAxes`、`DesignVariant`、`DesignStyle`、`DesignDensity`、`DesignIdiom`、`DerivationContext<T>`、`IControlDerivation<TL,TT>` | M2-1c | 未开始 | |
| M2-2b | 登记：`DesignLanguageDescriptor`、`ControlDerivationDescriptor`、构建器入口、按控件槽位的登记表 | M2-2a | 未开始 | |
| M2-2c | 推导查找：沿语言 Token 类型继承链，三级各有测试；重复登记报错 | M2-2b | 未开始 | |
| M2-2d | 删除算法契约与算法描述（`IThemeAlgorithm` 等 9 个文件） | M2-2b、M2-3b | 未开始 | |
| M2-3a | 单遍编译·全局：种子 → 语言 Token → 全局 Token → 最终规范化 | M2-2b | 未开始 | |
| M2-3b | 单遍编译·控件：求值单元（作用域、覆盖种子或固定轴的控件）、惰性求值、每个推导恰好执行一次 | M2-3a | 未开始 | |
| M2-3c | 快照加语言 Token 值表；缓存键含语言、变体、种子、显式值；删 `StagedTokenEvaluation` 与逐控件全局副本 | M2-3b | 未开始 | |
| M2-3d | 错误模型：全局失败使整次应用失败；控件推导失败在首次查询时暴露 | M2-3b | 未开始 | |
| M2-4a | `ThemeConfig` 新模型与 Builder；删 `ControlAlgorithmMode` | M2-2a | 未开始 | |
| M2-4b | Normalizer 与 Merger：轴的确定、取值的分层、`Inherit=false` 两种含义 | M2-4a | 未开始 | |
| M2-4c | `ThemeSelection` 与解析（三条规则、固定的轴、锁定的种子、跟随系统） | M2-4a | 未开始 | |
| M2-4d | `IThemeManager` 新接口；语言切换；Review Focus 3 的测试 | M2-4b、M2-4c、M2-3c | 未开始 | |
| M2-5a | XML v2 参考规范、XSD、示例文件 | M2-0 | 未开始 | |
| M2-5b | Reader 与 `ThemeDocument`（含 `LanguageTokens`、`When`、`Locked`） | M2-5a | 未开始 | |
| M2-5c | Binder 与 `ThemeDefinition`；条件段对求值单元求值；全部语义校验的诊断测试 | M2-5b、M2-2b、M2-4b | 未开始 | |
| M2-5d | 编译主题 ABI v2：生成器 writer、工具链任务、运行时程序 | M2-5c | 未开始 | |
| M2-5e | v1 → v2 换写命令；Review Focus 5 的测试 | M2-5b | 未开始 | |
| M2-5f | 来源与目录适配；用户目录里的 v1 文件给出诊断（Review Focus 1） | M2-5c | 未开始 | |
| M2-6a | 控件包内的生成物调整与 `TokenSchemaDigest` | M2-1c | 未开始 | |
| M2-6b | 语言注册生成器、控件推导注册生成器、`CoversTokenSchema` | M2-2b | 未开始 | |
| M2-6c | 推导完整性分析器（未写全、先读后写） | M2-6b | 未开始 | |
| M2-6d | 覆盖校验与依赖方向校验（语言包不引控件包、控件包不引语言一侧、取值包内无控件） | M2-6b | 未开始 | |
| M2-6e | 配置提交校验：缺取值包（Review Focus 2）、摘要不符（Review Focus 4）、形态与轴范围 | M2-6a、M2-6b、M2-4d | 未开始 | |
| M2-6f | 删除旧分析器（`ATOMUIGEN048` 等）与诊断号整理 | M2-1d | 未开始 | |
| M2-G | **M2 门槛**：Core、Generator、Toolchain 及其测试工程构建并通过单元测试；停下评审 | 以上全部 | 未开始 | |

## M3 Ant Design

开工前出步骤级计划：`docs/superpowers/plans/2026-11-22-design-language-m3-antdesign.md`。

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
| M3-0 | M3 步骤级计划 | M2-G | 未开始 | |
| M3-1a | `AtomUI.Controls` 的 3 个 Token 类清为纯声明，公式存档 | M3-0 | 未开始 | |
| M3-1b | `AtomUI.Desktop.Controls` 的 142 个 Token 类清为纯声明，公式存档 | M3-0 | 未开始 | |
| M3-1c | ColorPicker 6 个、DataGrid 1 个 Token 类清为纯声明，公式存档 | M3-0 | 未开始 | |
| M3-1d | Token 属性的语义注释补全（英文） | M3-1a…c | 未开始 | |
| M3-2a | `src/DesignLanguages/AntDesign/` 目录、语言包工程、解决方案分组 | M3-0 | 未开始 | |
| M3-2b | `AntDesignLanguage`、`AntDesignToken`、轴、`DefineSeeds`（含原 DaybreakBlue 的种子） | M3-2a | 未开始 | |
| M3-2c | 全局推导·亮色 Map | M3-2b | 未开始 | |
| M3-2d | 全局推导·暗色 Map | M3-2c | 未开始 | |
| M3-2e | 全局推导·紧凑 | M3-2c | 未开始 | |
| M3-2f | 全局推导·Alias | M3-2c | 未开始 | |
| M3-2g | 核心的中性工具整理到 `Theme/Derivation/`；删除旧计算器与 `DaybreakBlue.theme.xml` | M3-2c…f | 未开始 | |
| M3-2h | 契约测试；全局 Token 的取值比对（4 个变体 × 种子矩阵） | M3-2c…f | 未开始 | |
| M3-3a | `AtomUI.Controls.Themes.AntDesign`（3 个推导 + 共享推导方法） | M3-1a、M3-2h | 未开始 | |
| M3-3-B1 | 桌面推导 B1 | M3-1b、M3-3a | 未开始 | |
| M3-3-B2 | 桌面推导 B2 | M3-3-B1 | 未开始 | |
| M3-3-B3 | 桌面推导 B3 | M3-3-B2 | 未开始 | |
| M3-3-B4 | 桌面推导 B4 | M3-3-B3 | 未开始 | |
| M3-3-B5 | 桌面推导 B5 | M3-3-B4 | 未开始 | |
| M3-3-B6 | 桌面推导 B6 | M3-3-B5 | 未开始 | |
| M3-3-B7 | 桌面推导 B7 | M3-3-B6 | 未开始 | |
| M3-3-B8 | 桌面推导 B8 | M3-3-B7 | 未开始 | |
| M3-3-B9 | 桌面推导 B9 | M3-3-B8 | 未开始 | |
| M3-3c | `AtomUI.Desktop.Controls.ColorPicker.Themes.AntDesign`（6） | M3-1c、M3-3-B9 | 未开始 | |
| M3-3d | `AtomUI.Desktop.Controls.DataGrid.Themes.AntDesign`（1） | M3-1c、M3-3-B9 | 未开始 | |
| M3-3e | 四个取值包的覆盖校验无报错 | M3-3a…d | 未开始 | |
| M3-4a | GalleryBase 的 8 个 Token 类清为纯声明 + 通用推导 | M3-2h | 未开始 | |
| M3-4b | Gallery 启动配置与主题菜单的最小接入（只接 Ant Design） | M3-3e、M3-4a | 未开始 | |
| M3-4c | 各测试工程的启动配置与受影响测试的改写 | M3-3e | 未开始 | |
| M3-4d | 基线工具改用新的 `ThemeSelection`；夹具主题文件换写为 v2 | M3-4b | 未开始 | |
| M3-G | **M3 门槛**：Ant Design 4 个变体 G-取值、G-像素、G-性能、G-回归，零例外；停下评审 | 以上全部 | 未开始 | |

## M4 Shadcn 与 PrimeNG

开工前出步骤级计划：`docs/superpowers/plans/2026-11-22-design-language-m4-shadcn-primeng.md`。桌面批次沿用 M3 的 B1–B9。

| ID | 任务 | 依赖 | 状态 | 证据 |
| --- | --- | --- | --- | --- |
| M4-0 | M4 步骤级计划 | M3-G | 未开始 | |
| M4-1a | Shadcn 语言包工程、目录与分组、八种风格、轴、种子（含语言种子 `BaseColor`，核心删 `ColorNeutral`） | M4-0 | 未开始 | |
| M4-1b | `ShadcnToken` 与 `DeriveLanguage`（由 `ShadcnSurface`、`ShadcnStyle`、`ShadcnOverlay`、`ShadcnPrimary` 公开而来） | M4-1a | 未开始 | |
| M4-1c | `DeriveGlobal`：复制 Ant 的 Map 与 Alias 为起点并并入 Shadcn 的覆盖；紧凑 | M4-1b | 未开始 | |
| M4-1d | 组件配方、README | M4-1b | 未开始 | |
| M4-1e | 全局 Token 取值比对（32 个变体 × 种子矩阵） | M4-1c | 未开始 | |
| M4-2a | `AtomUI.Controls.Themes.Shadcn` | M4-1e | 未开始 | |
| M4-2-B1 | Shadcn 桌面推导 B1 | M4-2a | 未开始 | |
| M4-2-B2 | Shadcn 桌面推导 B2 | M4-2-B1 | 未开始 | |
| M4-2-B3 | Shadcn 桌面推导 B3 | M4-2-B2 | 未开始 | |
| M4-2-B4 | Shadcn 桌面推导 B4 | M4-2-B3 | 未开始 | |
| M4-2-B5 | Shadcn 桌面推导 B5 | M4-2-B4 | 未开始 | |
| M4-2-B6 | Shadcn 桌面推导 B6 | M4-2-B5 | 未开始 | |
| M4-2-B7 | Shadcn 桌面推导 B7 | M4-2-B6 | 未开始 | |
| M4-2-B8 | Shadcn 桌面推导 B8 | M4-2-B7 | 未开始 | |
| M4-2-B9 | Shadcn 桌面推导 B9 | M4-2-B8 | 未开始 | |
| M4-2c | Shadcn 的 ColorPicker 与 DataGrid 取值包 | M4-2-B9 | 未开始 | |
| M4-2d | Shadcn 四个取值包的覆盖校验；README（映射表、不支持清单、各控件目标等级） | M4-2a…c | 未开始 | |
| M4-3a | PrimeNG 语言包工程、目录与分组、三种风格、轴、种子 | M4-0 | 未开始 | |
| M4-3b | `PrimeNGToken` 与 `DeriveLanguage`（由 `PrimeNGPreset`、`PrimeNGAppearanceRules`、`PrimeNGPalette`、`PrimeNGNoir` 公开而来） | M4-3a | 未开始 | |
| M4-3c | `DeriveGlobal`：复制 Ant 的 Map 与 Alias 为起点并并入 PrimeNG 的覆盖；紧凑 | M4-3b | 未开始 | |
| M4-3d | 组件配方、README | M4-3b | 未开始 | |
| M4-3e | 全局 Token 取值比对（12 个变体 × 种子矩阵） | M4-3c | 未开始 | |
| M4-4a | `AtomUI.Controls.Themes.PrimeNG` | M4-3e | 未开始 | |
| M4-4-B1 | PrimeNG 桌面推导 B1 | M4-4a | 未开始 | |
| M4-4-B2 | PrimeNG 桌面推导 B2 | M4-4-B1 | 未开始 | |
| M4-4-B3 | PrimeNG 桌面推导 B3 | M4-4-B2 | 未开始 | |
| M4-4-B4 | PrimeNG 桌面推导 B4 | M4-4-B3 | 未开始 | |
| M4-4-B5 | PrimeNG 桌面推导 B5 | M4-4-B4 | 未开始 | |
| M4-4-B6 | PrimeNG 桌面推导 B6（含 Lara 紧凑的 Select 条件） | M4-4-B5 | 未开始 | |
| M4-4-B7 | PrimeNG 桌面推导 B7 | M4-4-B6 | 未开始 | |
| M4-4-B8 | PrimeNG 桌面推导 B8 | M4-4-B7 | 未开始 | |
| M4-4-B9 | PrimeNG 桌面推导 B9 | M4-4-B8 | 未开始 | |
| M4-4c | PrimeNG 的 ColorPicker 与 DataGrid 取值包 | M4-4-B9 | 未开始 | |
| M4-4d | PrimeNG 四个取值包的覆盖校验；README | M4-4a…c | 未开始 | |
| M4-5a | 解决方案分组、打包脚本、包布局白名单；删除 `AtomUI.Themes.Shadcn`、`AtomUI.Themes.PrimeNG`、`/Themes/` 分组 | M4-2d、M4-4d | 未开始 | |
| M4-5b | Gallery 接入两种语言；基线工具 44 个变体改用新选择 | M4-5a | 未开始 | |
| M4-5c | 恢复 M3 门槛时跳过的测试；旧 `AtomUI.Themes.Tests` 的内容迁入各语言的测试工程 | M4-5a | 未开始 | |
| M4-G | **M4 门槛**：48 个变体 G-取值、G-像素（Ant Design 零例外，其余例外逐条批准）、G-性能、G-回归；停下评审并分组提交 | 以上全部 | 未开始 | |

## M5 主仓收尾

| ID | 任务 | 依赖 | 状态 | 证据 |
| --- | --- | --- | --- | --- |
| M5-1a | 读集随求值结果缓存；子求值单元与读集不相交时复用父结果 | M4-G | 未开始 | |
| M5-1b | 新增五个性能场景并与 M0 基线对照；收益不足时按方案待定点退回并记录数据 | M5-1a | 未开始 | |
| M5-1c | 启动：只启用 Ant Design 与启用三种只用一种的差值；超门槛时推迟程序集加载 | M4-G | 未开始 | |
| M5-2 | Gallery 的语言、风格、明暗、密度、品牌色、Base Color 菜单全部由 `IThemeManager.Languages` 生成 | M4-G | 未开始 | |
| M5-3a | 对等性样例：样例语言包、样例控件包、两个样例取值包及其测试 | M4-G | 未开始 | |
| M5-3b | 打包脚本产出全部新包；干净 NuGet 缓存下的最小应用构建并启动 | M5-3a | 未开始 | |
| M5-4a | 架构文档重写：`runtime.md` 相关章节、`compiled-theme-packages/*` | M4-G | 未开始 | |
| M5-4b | 指南重写：定制、编写设计语言、设计语言包、第三方控件包 | M4-G | 未开始 | |
| M5-4c | 参考与模块文档；方案提炼为 `docs/architecture/systems/theming/design-languages.md` | M5-4a | 未开始 | |
| M5-4d | API 变更说明（中英） | M5-4a…c | 未开始 | |
| M5-5a | 取值溯源工具（不计入完成定义） | M4-G | 未开始 | |
| M5-5b | 设计时预览与无头测试的语言启用入口（不计入完成定义） | M4-G | 未开始 | |
| M5-G | **M5 门槛**：四项门槛在最终的树上完整重跑；文档示例由测试编译并运行；停下评审并分组提交 | M5-1…4 | 未开始 | |
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
| M0-G | | 自检（同一构建作两边） | 自检（单变体全量） | 记录基线数值 | 不适用 | |
| M1-G | | | | | | |
| M2-G | | 不适用 | 不适用 | 不适用 | 核心单测 | |
| M3-G | | | | | | |
| M4-G | | | | | | |
| M5-G | | | | | | |

## 像素例外清单

Ant Design 的 4 个变体不允许出现在这里。其余每条须经批准。

| # | 变体 | showcase 与区域 | 原因 | 基线与候选截图 | 批准 |
| --- | --- | --- | --- | --- | --- |
| | | | | | |

## Token 改名与迁移映射

与 `tools/performances/design_language_token_renames.txt` 保持一致（该文件在第一次需要时建立）。

| 旧 | 新 | 发生在 |
| --- | --- | --- |
| | | |

## 裁定记录

格式：`<任务 ID>：裁定——<决定了什么>——<原因>——<错了的代价>`。

- 总则：裁定——进度记录放在本文件，不使用 `.superpowers/` 工作目录——项目规则禁止写 `.superpowers/`，`docs/superpowers/` 只放 Markdown——无。
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
