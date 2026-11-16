# SDD ledger — plan: docs/superpowers/plans/2026-11-15-shadcn-latest-alignment.md

执行方式：Native（superpowers:executing-plans），用户 2026-11-16 选定。
设计文档：docs/superpowers/specs/2026-11-15-shadcn-latest-alignment-design.md
取值附录：docs/superpowers/specs/2026-11-15-shadcn-latest-alignment-data.md

## 启动裁定（Ruling）

1. Ruling: 不建 worktree，直接在主工作区 `release/6.0` 执行 — 项目分支规范默认主线开发，用户在执行方式确认时未申请隔离 — 若错：污染主线历史，需用户指出后另行隔离。
2. Ruling: 台账放 `docs/superpowers/progress/` 而非技能默认的 `.superpowers/sdd/` — 项目产物存放强约束禁止写 `.superpowers/` — 若错：无实质成本，仅路径差异。
3. Ruling: 前置提交 — 用户 2026-11-16 明确授权把工作区 46 文件的 PrimeNG 未提交改动先行单独提交（不含本次 Shadcn 的 4 个 spec/plan 文档与 `tests/.../Temporary/`）；提交前先跑受影响 6 个测试项目作为绿色基线 — 若错：基线不绿会把 PrimeNG 的失败误归因给 Shadcn 任务。
4. Ruling: `tests/AtomUI.Desktop.Controls.Rendering.Tests/Temporary/FinalVisual/` 引用旧 Shadcn ID，属临时视觉测试（未跟踪文件），不入本次前置提交；Task 7 删除旧包时按临时测试生命周期改用新 ID 或删除 — 若错：Task 7 后该项目编译失败。

## Pre-flight（跨任务接口核对）

写作计划时已逐一核对（详见会话记录）：ABI 2 构造函数含 brandColor（CompiledThemeProgram.cs:34）；`ValidateSeedBrandTokens`（ThemeDocumentReader.cs:122）；ATMTHM3104 运行时分支（CompiledThemeCatalog.cs:349）；`BrandColorSupport`（ThemeSelection.cs:178）；`_brandByLanguage`/`WithToken` 应用点（GalleryWorkspaceViewModel）；附录 A/B 反引号表格；三个引用旧 ID 的 Gallery 测试文件。计划自审修了 5 处缺陷（Task 3 补生成器诊断文件、Task 5 补 Themes()、Task 7 补 ColorNeutral 存活断言、Task 9 补条件项、Task 12 补 Temporary 检查）。

消费链：T2←T1(ColorNeutral seed)；T3←T2(ThemeNeutralColorSupport/ThemeDocument.NeutralColor)；T4←T2(ThemeInfo.NeutralColor)；T5 独立；T6←T5(色板/surface)；T7←T1–T6；T8←T7(ShadcnStyles.Resolve)+T5(FamilyOf 反查)；T9←T8(两条 Skip 与条件项)；T10←T7–T9；T11←T2/4(NeutralAccentColor/NeutralSeed)；T12←全部；T13←全部。无未解决冲突。

## 基线验证

- 命令：6 个受影响测试项目（Core / Generator / Themes / Desktop.Controls / Rendering.ChoiceFamily / Gallery）串行 `dotnet test --framework net10.0 --no-restore`
- 首轮结果：AtomUI.Core.Tests 4 失败（605 过）——失败全在未提交批次新增的 `Catalog_Warns_When_Density_Disagrees_With_The_Algorithm_Chain`（ATMTHM3105 新功能的测试）。
- Ruling: 修复该测试的算法链 — 根因：测试 XML 声明 `Appearance="Dark"` 但链只写 `Default`/`Compact`（结果为 Light），被 ThemeManager 的 ATMTHM3003 外观校验拒绝；测试从未绿过。按仓库惯例（`LanguageXml` 助手与 Material Dark profile：Dark 替换 Default、Compact 追加在后）把四个用例的链改为 `Dark` / `Dark+Compact`。3105 产线代码不动。 — 若错：3105 用例的链形态与真实包不同，但被测行为（Compact 存在性 vs Density 声明）不受链中 Dark 影响。
- 修复后单测：`--filter Catalog_Warns_When_Density_Disagrees` → 4/4 通过。
- 第二轮：Core 608/609，唯一失败 `OwnerTokenResourceTests.Query_Hot_Path_Does_Not_Allocate`（分配量断言）；该测试不在本批改动内，单独跑 1/1 过、整类 17/17 过，判定为全套冷跑时的偶发（惰性初始化计入测量窗口），非本批引入。Generator 1115/1115。
- Ruling: 修复 Material Link 测试 — 根因：未提交批次只给 4 个 Material profile 钉了 `ColorLink`，新测试却断言 `ColorLinkHover == ColorPrimaryHover`；Ant 管线里 link 与 primary 的 hover 派生路径不同（同底色 #6750A4 得 #a599bd ≠ #8572b0），断言从未成立。按测试注释引用的 Material 3 规范（label 的 hover/focus/pressed 变体都取 primary 本身，悬停反馈走 state layer），改为 XML 钉 `ColorLinkHover/Active` = 种子、测试断言三者等于种子。 — 若错：Material 链接悬停有细微色变需求时需改回派生值。
- Ruling: 修复 ProgressBarInnerLabelEmptyOpacityTests 编译错 — 根因：缺 `using AtomUI.Desktop.Controls.DesignTokens;`（`ProgressBarTokens.Identity` 是生成类，存在于该命名空间）。只补 using，不改用例。 — 若错：无。
- Ruling: 补完 ProgressBar InnerLabelEmptyOpacity 功能（本批半成品） — 根因（探针二分定位）：`AbstractProgressBarTheme.axaml:39` 给 `Value` 挂了 `DoubleTransition(MotionDurationVerySlow)`，headless 时钟不走，设 10 后有效值冻结在 0，"离开 0 恢复不透明度"断言永不可能通过。修复三件套：(1) 主题选择器由 `[Value=0]` 属性匹配改为仓库惯用的 `:value-zero` 伪类（与 `:completed` 同法）；(2) `AbstractProgressBar.UpdatePseudoClasses` 设置 ValueZero，`OnPropertyChanged` 把 Value/Minimum/Maximum 纳入触发（顺带修正 `:completed` 在值变化时不刷新的潜在缺陷）；(3) 测试补 `WithToken("EnableMotion","false")`（既有模式，TransitionDurationTests 同法），值立即生效。诊断探针已删。 — 若错：无动画场景下伪类切换语义与值完全一致；风险极低。
- Ruling: 更正 ThemePresets 预览算法链测试期望 — 根因：批次把无模式 profile 的回退链改为与 `ThemeSelectionResolver` 规范一致（`FallbackChain`：定义链去内建 Dark/Compact → 追加 Compact → 追加 Dark，修正了旧实现丢掉 Default 导致预览与实际应用不一致的缺陷），但已有测试 `Seed_Only_Package_Uses_Orthogonal_Algorithms_When_Mode_Profiles_Are_Absent` 的期望 `[Dark, Compact]` 没同步。改为 `[Default, Compact, Dark]`（解析器 ThemeSelection.cs:119-132 的规范顺序）。 — 若错：预览与实际渲染脱节会重现。
- **基线全绿**：Core 609/609（分配量测试冷跑偶发已复验通过）、Generator 1115/1115、Themes 213/213、Desktop.Controls 4496/4496、Rendering.ChoiceFamily 152/152、Gallery 462/462。

## 任务进度

- 前置提交：PrimeNG 批次（46 个修改文件 + 4 个未跟踪源码/测试/进度文档，含本次 5 处修复）验证通过并提交，commit：`b61de2c62`
- Task 1: complete (commit f91d19f61；tests: ColorNeutralSeedTests 3/3 RED→GREEN，Core.Tests 612/612，Theme.ContractTests 构建绿〔共享工程，0 用例〕)
- Task 2: complete (commit f52d21ba9；tests: Reader 4 + 目录 2 新用例 RED→GREEN，Core.Tests 618/618 + 1 计划内 Skip；Ruling: ① ATMTHM3102 不存在（编号废弃），3107 的"算法链不提供缺省"实现为"仅内置算法链"判定（vendor 链可能自带缺省，静默），并把该行为钉进 `Seed_Neutral_Definition_With_Vendor_Chain_Without_Seed_Stays_Silent`；② `Runtime_And_Compiled_Readers_Agree_On_NeutralColor` 放 `CompiledThemeCatalogResolverTests`（BuildLanguageManager helper 所在），Skip 至 Task 3。中性派生集合 29 项按计划 Task 2 Interfaces。)
- Task 3: complete (commit f68e7b7e6；tests: Core ABI 3 用例 3 + 生成器 Abi3 排放与 ATMTHM3108 RED→GREEN，Generator 1116/1116、Core 621/621（Skip 已去）、Themes 213/213——四个预设包经 v7 传输重建为 ABI 3 端到端通过；Ruling: ABI 行为测试放手写子类 `CompiledThemeProgramAbiTests`（生成器测试只编译不执行），生成器侧钉 `base(3,` 排放与 3108；计划中的 `Abi3_Program_Publishes...` 与 `Abi2_Program_Keeps_Loading...` 均落位于此。)
- Task 4: complete (commit c9b4a50d6；tests: 解析器 4 新用例 RED→GREEN，ThemeSelectionResolverTests 16/16，Core 全量 625/625〔其间一次冷跑失败为基线期已记录的分配量偶发，复跑两次全绿〕)
- Task 5: complete (commit 256c14650；tests: ShadcnBaseColorTests 10/10 RED→GREEN〔7 家族×亮暗逐变量对附录、500 级=种子、自定义种子不误判、自定义暗色对比 ≥7〕，Themes 223/223。色板由临时脚本 gen_palette_cs.py 从 tailwindcss@4.3.5/theme.css 生成〔/tmp/shadcn-tw-tool，不入库〕；Ruling: 自定义色阶的色度锚点用色板换算的 zinc-500 实际色度（非手抄 0.016），种子=zinc-500 时精确复现 zinc，自洽性更强。)
- Task 6: complete (commit f0965470d；tests: ShadcnThemeColorTests 38/38 RED→GREEN〔17 主题×明暗对附录 B、neutral 跟随 Base Color、3 个自定义种子明暗对比 ≥4.5〕，Themes 261/261；Ruling: 计划钉死的两个固定前景候选（L0.97 近白、L0.42 深调）对中等亮度种子都不足 4.5（探针实测 #10B981 双双失败；#3B82F6 暗色更落入自身色相死区），与 Review Focus 1 的硬保证冲突——第二候选改为同色相同色度规则下自适应降明度（0.42 起步 0.02 下探），兜底退化纯白再纯黑，仍优先官方近白形态。若错：前景色比官方固定档更暗，仅影响自定义种子。)
- Task 7: complete (commit e6495cff4；tests: ShadcnPackageTests 24/24 RED→GREEN〔声明元数据、7 家族×明暗 5.4 全表对附录+种子存活、几何、暗色可读、追加链存活〕，Themes 269/269，包构建无诊断。Ruling 四条：① 种子"副本"以受控临时改动+finally 还原实现——`GeneratedThemeSchema.CopyGlobalValues` 为 internal 不可跨程序集，编译器对链内算法传同一 seed 实例且不对其记账，改动在同步 Evaluate 内不可观测；② 5.4 表写入必须按 Token 实际段位分家（Map 段写 Map Token，Alias 段 Token 如 ColorIcon/ColorSplit/ControlItem*/ColorTextHeading/Disabled 系在 Ant 公式后于 EvaluateAlias 重写），契约检查会拦截越段；③ Alias 阶段拿到的是按外观计算后的 ColorPrimary（暗色=800/500 级或 base 的 200 级），新增 `ShadcnThemeColor.ResolveComputed` 做计算值反查（neutral 判定=等于 surface.Primary）；④ 测试期望修正：stone 暗色 --ring 是 500 级 #79716B（我初版误写浅色 400 级）。另：全局 BoxShadows=卡片环+CardShadow、Secondary=xs 为裁定近似，控件阶段（Task 8）细化。)
- Task 8: complete (commit d7954b5fd；tests: ShadcnOverlayTests 16 + ShadcnControlTests 9 共 25/25 RED→GREEN，Themes 294/294。Ruling：① Dialog 无自身 BoxShadows Token——DialogSurfaceTheme.axaml:82 以 OwnerTokenResource 读全局 BoxShadows，Nova 档=仅描边环恰好等于全局值，不建 ShadcnDialogTokens 类，Task 10 出现 Dialog≠Card 阴影档的 Style 时再引入机制；② Toast/sonner 实测未做（需官网浏览器实测），暂沿用全局（Ant 默认），列入 Task 12 README 与 Task 13 待办；③ 选择类控件（CheckBox/Radio/Switch/菜单项）推迟到 Task 10 与附录 C 一起参数化；④ 控件阶段类命名经用户确认沿用 PrimeNG 的 `<Style><Control>Tokens` 后缀。)
- Task 9: complete (commit cab617092；tests: ToolTipShadowTests 2 + DrawerEdgeBorderTests 5 RED→GREEN，Desktop.Controls 4503/4503，Themes 294/294。Ruling 三条：① Tooltip 阴影方案=Token 默认等于全局 BoxShadowsSecondary〔改动前实际绘制值〕+ ControlTheme Setter 写 ToolTip.ToolTipShadows + 控件代码转发到运行期跨根 Popup 的 PopupRootShadow/OverlayHostShadow〔跨视觉根，ControlTheme 链不可达，代码注释已写明原因〕；② Drawer 边框=Color Token（EdgeBorderColor，样式属性 IBrush?）+ 标量厚度 Token，DrawerContainer 按方位组合单边 Thickness 后 SetCurrentValue 写表面〔生成器契约 ATOMUIGEN047 禁止模板子节点 LocalValue 直写；标量 Token 无法在 Setter 绑定表达单边 Thickness〕；Token 归属 Drawer（DrawerContainer 不在主题 Schema），照 PushOffsetPercent 链路绑定转发；③ 生命周期契约测试抓到真缺陷：CompositeDisposable 续行误报+需显式释放路径——改为两个独立 binding 字段并在 OnDetached 释放。Token 发布形态裁定：Color 类 Token 资源发布为 IBrush，样式属性用 IBrush?（同 AbstractLineProgress.PercentageLabelColor 惯例）。)
- Task 10: complete (commit ef3825ea3；tests: Shadcn 全部 124/124〔新增 8 风格弹层表参数化 7、几何 8、32 profile 元数据 7、Maia/Luma-Dialog/Lyra/Mira/Sera 命名 5〕，Themes 321/321，包构建零诊断。Ruling 三条：① Dialog 阴影机制=新增 ShadcnDialogTokens 控件阶段写控件级全局 BoxShadows（DialogSurfaceTheme 以 OwnerTokenResource 读取该作用域），Luma/Sera/Rhea 的 Dialog 阴影档独立于 Card；② FlyoutHost/ArrowDecoratedBox 无圆角 Token——Popover 圆角随全局 BorderRadius 是既定近似（Maia 全局 26 vs 官方 popover 18），记入 README 不支持清单，不新增控件 Token；③ 表中未标注的 menu/popover padding 取 Nova 缺省（4/10），仅断言设计表钉死的项。)



- Task 11: complete (commit f792abeeb；tests: Gallery 465/465〔中性色记忆/AntDesign 隔离/重置 3 新用例 + 旧 ID 用例全部改写〕，GalleryBase 219/219。Ruling：① 中性色组挂 WorkspaceWindow 菜单，经 WindowMenuItemKind.NeutralColor 注入 GalleryThemeMenu（第 4 参）；② xlf 为 XLIFF 2.0（unit/segment），source 必须是目录 canonical 英文、仅 target 本地化（ATOMUILOC006 教训）；③ Theme Presets 中性行复用 ThemePrimarySwatch 类型（仅展示命名），17 Theme 名与 7 中性名加入 Presets 目录（12+9 键 ×4 语言）；④ 预览 CreateConfig/导出链路加 neutral 形参，导出写 NeutralColor 属性，ColorNeutral 种子随 config.Tokens 流出；⑤ 旧 ID 引用更新：固定品牌示例改 Bootstrap/Material，Shadcn 预览断言改为按 Style 表（明 #171717/暗 primary=#E5E5E5、暗底 #0A0A0A、Compact-4）。⚠️ 失误：`git add -A` 将未跟踪的 Temporary/FinalVisual 临时视觉测试带进 f792abeeb（前置提交时有意排除）——未自行改历史，待用户审查时定夺（建议授权 amend 移除或后续提交删除）。)

## Task 13 - 视觉验收步骤（待用户回传证据；未回传前以下各项一律"待视觉验收"）

宿主要求：AtomUIGallery.Desktop（`atom:Window` 宿主）。每项操作在 8 种 Style（菜单「主题 → 设计语言」依次选 Shadcn Vega/Nova/Maia/Lyra/Mira/Luma/Sera/Rhea）× 亮暗（主题 → 外观）下各做一次，与 https://ui.shadcn.com/create 同 Style、同 Base Color（默认 Neutral）的对应组件对照。判定容差：功能正确 + 关键区域（环/阴影外沿/圆角/箭头）一致，通道差 ≤ 8，只看边缘。截图范围含弹层及其阴影外沿（弹层阴影不裁切）。

示例入口与分类（Gallery 左侧导航 → 分类 → 示例）：
1. Navigation 导航分类 → Menu 示例：右键/下拉打开菜单与子菜单。预期：Maia 菜单 shadow-2xl 完整、无裁切；Select/菜单面板暗色描边环可见（Maia/Luma/Rhea 浅色 5%、暗色 10%，其余 10%）。
2. DataDisplay 数据展示分类 → InfoFlyout 示例（带箭头 Popover/Flyout）：预期：框体与箭头一体描边、无双线、箭头底边无横线；Nova/Vega/Maia/Lyra/Mira Popover 为 md 档阴影（Maia 2xl、Luma/Rhea lg）。
3. DataEntry 数据录入分类 → Select 示例：下拉面板描边环与圆角（Lyra/Sera 圆角 0）。
4. Feedback 反馈分类 → Modal 示例：Nova/Vega/Maia/Lyra/Mira 无阴影仅描边环；Luma/Rhea xl、Sera md 阴影；遮罩浓度（Maia/Mira 80%、Luma/Rhea 30%、Sera 20%、其余 10%）。
5. Feedback 反馈分类 → Drawer 示例：四向抽屉阴影档（Luma/Rhea xl、Sera md、其余 lg）；贴边一侧 1px 边框。
6. DataDisplay 数据展示分类 → Tooltip 示例：反色（暗色为白底深字）、无阴影无环、圆角（Maia 18、Luma/Rhea 14、Lyra/Sera 0）。
7. General 通用分类 → Theme Presets 示例：主色色块行为（Shadcn 显示 17 个官方 Theme 色块 + 中性色 7 色块一行）；切中性色后整页表面色调随之变化、文字保持可读。
8. 任一含按钮/输入的示例（如 General → Theme Presets 预览区）：高度/圆角/字重按 Style（Sera 40 高、semibold 12；Lyra 方角；Maia 胶囊）；输入聚焦边框为 ring 灰色而非主色。

截图清单（每 Style 亮暗各一张，共 16 组）：菜单+子菜单、带箭头 Popover、Select 面板、Modal、Drawer（右向）、Tooltip、按钮+输入行。回传后按上表逐项判定并记录结论；判定后截图不入库。

## Task 13 - 全量回归（进行中）

- 2026-11-16 17:52 启动 scripts/run-full-regression.sh（计划 Task 13 Step 2）。
- 中途失败（渲染效果契约，等跑完统一修）：Drawer EdgeBorderColor/EdgeBorderThickness、NavMenu MenuPopupShadows/BoxShadowsSecondary、ProgressBar InnerLabelEmptyOpacity 的 `*_Token_Has_Effect` —— 新增 Token 默认值等于现状导致效果测试检测不到差异，需核对效果测试的契约语义后修（改测试期望或补效果）。
- 最终 [SUMMARY]：（待填）

## Task 13 - 回归收尾与遗留清单

- 用户真机报告：Shadcn 主题下胶囊形状严重错误 → 根因两层（commit b15c4bd54 修复）：① Maia/Luma 的 26 是官方 rounded-full 的 --radius 变量值而非实际按钮圆角，字面写入超过半高不收敛 → 按钮按半高绘制三档（Button/Input 控件阶段写作用域 BorderRadius/LG/SM）；② Drawer 贴边边框写在布局壳 PART_Surface 上不绘制 → 迁至信息容器（视觉表面）Frame 的 TemplateBinding。DrawerEdgeBorderTests 断言同步迁移。修复后 Drawer 效果行 32/32、Desktop.Controls Drawer/Tooltip 120/120、Shadcn 133/133。
- 全量回归 [SUMMARY]：13 项目 12,806 条，失败 11 → 修复 2（我的 Drawer EdgeBorderColor/Thickness：边框迁移 + 效果门 harness 新增 Companions 机制〔颜色类 Token 需伴随状态才可见〕+ DrawerRegion 指向 InfoContainer）；**剩余 9 个为前置批次（b61de2c62）遗留**，非本次引入：
  - MenuFlyoutPresenter ×4：MenuPopupShadows（attached 值无渲染端消费——菜单弹层 Token 化只做了 Setter 半截）、BoxShadowsSecondary、ColorBorderSecondary、MenuPopupBorderColor（ArrowDecoratedBox 有 BorderBrush 无厚度）。
  - Menu ×2 / NavMenu ×2：MenuPopupShadows（own）、BoxShadowsSecondary（global）——消费链断在 presenter。
  - ProgressBar ×1：InnerLabelEmptyOpacity（场景需安排 Value=0+IsInner 状态）。
  - 两轮修复尝试（presenter AXAML $self 绑定 → XAML 预编译降级；ScrollHost 逻辑树转发 → 弹层渲染被搅乱使更多行变红）均已回滚；需要按前置批次的既定架构（attached 继承链）完整接通菜单弹层消费端，属独立工作量，建议另立任务。
- 全量回归最终判定：**未全绿**——本次 Shadcn 工作相关全部通过；遗留 9 项属前置批次未完成项，清单如上。

## 终审（独立代码审阅）

- 审阅范围 b61de2c62..HEAD（13 提交）。结论：**无 Critical；可合入（附条件）**。Review Focus 5 项逐项核实通过（对比度数学、Ant 隔离无反例、种子存活、ABI 对齐、控件作用域隔离）；seed 临时改动无观测窗口；生成期/运行期 3108 判定顺序一致。
- Important 3：I-1 Vega Drawer 贴边边框遗漏（已修：Vega 写环色 1px，其余 Style 不画，测试 3 条锁定，043d0e91e）；I-2 混链 remarks 误导（已修：remarks+README 明确 [Shadcn:X, AtomUI:Dark] 的 Map 颜色是 Ant 公式值，暗色用 .Dark profile）；I-3 FinalVisual 临时测试（f792abeeb 误入库，等用户授权 amend 移除或提交删除；默认 dotnet test 会执行 8 行，Linux/CI 因 /private/tmp 路径会挂）。
- Minor：M-1 Compact 比例重算（README 记录）；M-2 Export 中性自保降级（已修）；M-3 lambda 参数遮蔽（已修）；M-4 Drawer 双通道赋值（已修：删 AXAML 冗余 Setter，保代码绑定同 PushOffsetPercent）；M-5 ShadcnAppendix 依赖仓库文件（知悉不修）。
- Declined to judge：Luma 小号档是否应区别于半高胶囊（用户真机确认后按半高修复，视为已裁定）；混链是否应 Alias 自愈（系统级取向，PrimeNG 同病，另立项）。
