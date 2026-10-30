# Semantic Part 控件全量改造实施计划

> **供智能体执行者使用：** 必须使用 `superpowers:executing-plans` 在当前会话中逐个控件家族执行本计划，不得分派 subagent。每个控件在修改源码前和创建提交前都必须经过用户审核。

**目标：** 为所有适用的 AtomUI Desktop 控件家族建立基于代码事实的 Semantic Part 契约，同时保持性能、NativeAOT、主题、布局、Popup、容器和兼容性边界。

**架构：** 全局 Semantic Part 系统继续采用 selector-first 和生成器驱动方案。每个控件在自己的 `overview.md` 与 `implementation.md` 中拥有公共 Part 契约；只有这两份文档通过审核后才能开始实现。Gallery Preview 保持产品无关、Descriptor 驱动并真正延迟创建。

**技术栈：** .NET 10、Avalonia 12、AtomUI Desktop Controls、AtomUI Generator、AXAML ControlTheme、xUnit v3、Shouldly、Avalonia Headless、AtomUI Gallery、NativeAOT。

## 全局约束

- 每个控件家族在自己的独立 worktree 中工作（分支命名 `feature/semantic-<Control>`，worktree 路径 `.worktrees/<control>`），
  全部从 `feature/semantic` 派生；主 worktree `.worktrees/semantic` 的 `feature/semantic` 保留为集成分支。单控件改动不直接
  落在集成分支上，避免多控件并行时互相污染（2026-09-16 修订，此前的「只允许在 `.worktrees/semantic` 工作」不再适用）。
- 遵循[全量改造设计](../specs/2026-08-12-semantic-part-control-rollout-design.md)和正式的 [Semantic Part 系统架构](../../architecture/systems/theming/semantic-parts.md)。
- 准入只以 Ant Design 最新稳定发布源码中公开且实际消费的 Semantic DOM API 为准；当前基线为 2026-08-12 的 6.6.0。
  例外：经用户指令撤销排除、且已按「非 §2.1 Gate 通过」口径在设计文档 §2.4 日期化记录项中单独留痕的控件（当前仅
  `ComboBox`），其准入依据是用户指令而不是该 Gate；除这些已留痕项外，不得自行扩大范围。
- 官网展示、普通 `className` / `style`、ConfigProvider、internal schema、Props 间接继承或嵌套子组件透传不得作为准入证据。
- 每次只处理一个控件家族。当前控件等待文档或实现审核时，不得开始下一个控件，除非用户明确调整顺序。
- 修改源码、主题、测试、Gallery 或 changelog 前，必须先更新该控件的 `overview.md` 和 `implementation.md` 并获得批准。
- 只有主题符合仓库独立维护标准时，才能创建控件专项设计文档。
- 不得手工编辑 `docs/AI/generated`。
- 不得向生产控件引入运行时 VisualTree 搜索、反射、运行时 AXAML 解析、动态 marker binding 或永久监听器。
- AtomUI 默认主题不得使用 Semantic selector 驱动内置样式。
- 本分支的视觉验收范围原则上仅限 Semantic Part 新增视觉与本轮修复点；与 6.1.7 基线一致性相关的全量回归走查不作为逐项门槛，仅在怀疑 bug 时执行（2026-09-03 用户指示）。
- 单个控件实现完成后必须保持所有变更未提交；只有用户完成验证并明确授权后才能创建提交。

---

## 1. 计划文件与职责

| 文件 | 职责 |
| --- | --- |
| `docs/superpowers/specs/2026-08-12-semantic-part-control-rollout-design.md` | 稳定的改造范围、门禁、风险模型、性能和兼容性规则。 |
| `docs/superpowers/plans/2026-08-12-semantic-part-control-rollout.md` | 总体顺序、状态、通用执行循环和跨批次收尾。 |
| `docs/superpowers/plans/2026-08-12-semantic-part-batch-1-basic-controls.md` | 16 个基础视觉与状态控件家族。 |
| `docs/superpowers/plans/2026-08-12-semantic-part-batch-2-collections-containers.md` | 16 个集合、容器和导航控件家族，另有 2026-09-15 追加的 `Expander`。 |
| `docs/superpowers/plans/2026-08-12-semantic-part-batch-3-input-selection.md` | 15 个输入和选择控件家族，另有 2026-09-16 追加的 `ComboBox`。 |
| `docs/superpowers/plans/2026-08-12-semantic-part-batch-4-hosts-windows.md` | 10 个 Popup、Overlay 和服务宿主控件家族。 |
| `docs/superpowers/plans/2026-08-12-semantic-part-batch-5-high-density.md` | `NavMenu` 和 `DataGrid` 两个性能敏感控件家族。 |
| 第六批任务清单（见本文档任务 6） | `ButtonSpinner` 一个输入基座控件家族（2026-09-16 新增）。 |
| 第七批任务清单（见本文档任务 7） | `Splash` 一个桌面启动反馈控件家族（2026-09-16 新增）。 |

正式控件文档不得将这些计划作为唯一设计来源。它们应链接系统架构，并描述控件自身的当前契约。

## 2. 基线清单

### 已完成基线

- [x] `Button`：已具备 Descriptor、Desktop/Browser 静态 marker、selector/布局测试、现行设计文档和延迟创建的 Gallery Preview。

### 不适用

- [x] `Avatar`、`Carousel`、`Rate`、`Watermark`。
- [x] `Icon`、`FlexPanel`、`Grid / Row / Col`。
- [x] `BorderBeam`。
- [x] `WindowTitleBar`、`Window`。

> 范围变更（2026-09-15）：`SplitButton` 的原排除判定经用户指令撤销——AtomUI 需要让 `SplitButton` 支持 Semantic
> Part。它以自身 public owner 直接拥有下拉命令弹层（`Flyout`），按 `DropdownButton` 先例独立通过准入 Gate，
> 移入第四批计划执行（见下方批次清单与[第四批任务清单](2026-08-12-semantic-part-batch-4-hosts-windows.md)任务 11）。

> 范围变更（2026-09-15）：`Expander` 的原排除判定经用户指令撤销——AtomUI 需要让 `Expander` 支持 Semantic Part。原判定
> 以“上游只有 `Collapse` 一个 owner、`Collapse.Panel` 没有独立 API”为由拒绝映射，但该理由检验的是上游 owner 数量，而
> [全量改造设计 §2.1](../specs/2026-08-12-semantic-part-control-rollout-design.md)第 4 条要求的是“AtomUI 控件与该公开
> owner 的产品职责直接对应”。`Expander` 是单面板折叠容器，与上游 `Collapse` 的单个面板承担同一产品职责，上游已公开
> `root` / `header` / `icon` / `title` / `body` 五个语义键，因此映射成立。移入第二批计划执行（见下方批次清单与
> [第二批任务清单](2026-08-12-semantic-part-batch-2-collections-containers.md)任务 16）。

> 范围变更（2026-09-15）：`TabStrip` 与 `Menu` 的原排除判定经用户指令撤销——AtomUI 需要让二者支持 Semantic Part。
> 二者此前已随 `TabControl`（第二批）与 `NavMenu`（第五批）的提交一并实施，本次为**追认**并补齐范围记录，使文档与实现一致。
> 原判定均以“上游没有独立 public owner”为由拒绝映射（`TabStrip` 之于 `Tabs`、`Menu` 之于桌面命令菜单），但该理由检验的
> 是上游 owner 数量，而[全量改造设计 §2.1](../specs/2026-08-12-semantic-part-control-rollout-design.md)第 4 条要求的是
> “AtomUI 控件与该公开 owner 的产品职责直接对应”。二者分别移入第二批与第五批计划执行（见下方批次清单，以及
> [第二批任务清单](2026-08-12-semantic-part-batch-2-collections-containers.md)任务 17 与
> [第五批任务清单](2026-08-12-semantic-part-batch-5-high-density.md)任务 3）。

> 范围变更（2026-09-16）：`ButtonSpinner` 的原排除判定经用户指令撤销——AtomUI 需要让 `ButtonSpinner` 支持 Semantic
> Part。原判定以“Ant Design 没有职责直接对应的公开 Semantic DOM owner；`InputNumber` 的 handle 是其内部区域”为由
> 拒绝映射，但按当前稳定版 `antd@6.6.4` 源码，`InputNumberSemanticType` 公开并实际消费 `root` / `prefix` /
> `suffix` / `input` / **`actions`** 五个分区键，其中 `actions` 正是包裹上、下步进按钮的容器（由
> `@rc-component/input-number` 应用）。原判定的“handle 属内部区域”事实前提因此不再成立，按 [全量改造设计 §2.1](../specs/2026-08-12-semantic-part-control-rollout-design.md)
> 第 4 条的职责直接对应成立。移入第六批执行（见下方批次清单与第六批任务清单）。
> 注意：上游至今没有独立 spinner 组件 owner，本项准入依据的是上游 `actions` 键 + 用户指令，不是原触发条件
> 「新稳定版出现独立 spinner owner」自动满足；逐项证据与两点边界记录见设计文档 §2.4。

> 范围变更（2026-09-16）：`ComboBox` 的原排除判定经用户指令**彻底撤销**——用户要求把“ComboBox 被排除在 Semantic Part
> 范围之外”彻底解除。它从本文档「不适用」清单与设计文档 §2.4 移入 §2.3 纳入映射，随第三批执行（见下方批次清单与
> [第三批任务清单](2026-08-12-semantic-part-batch-3-input-selection.md)任务 16）。
>
> **本项不是 §2.1 Gate 通过，不得按 Gate 通过记录**：上游 6.6.0 确实没有公开 `ComboBox` owner，第 1 条不成立。准入依据
> 是用户指令 + AtomUI `ComboBox` 自身就是职责完整、可独立定制的 public owner（直接派生 Avalonia `ComboBox`，自有输入框、
> 下拉 handle、模板内 Popup 与候选容器创建路径，不复用 `Select` 的 internal combobox mode）。原判定中「`Select` 的
> internal combobox mode 不能作为公开 owner」继续有效。命名参考上游 `Select` 已公开的语义分组，不发明 `ComboBox` 不存在
> 的键。它是本清单唯一的「非 Gate 通过」例外——其余撤销项（`SplitButton`、`Expander`、`TabStrip`、`Menu`）均可在 §2.1
> 职责对应条款下自洽论证，`ComboBox` 不能，故不得被当作 Gate 通过的先例引用。

逐项公开 API 证据、产品职责映射和重新评估条件以全量改造设计的“排除映射”为准。不得因 AtomUI 模板内部存在
可定制节点而绕过准入 Gate。**引用本清单作排除依据前，先确认该项未被后续日期化撤销覆盖。**

> 范围变更（2026-09-16）：`GroupBox` 的原排除判定经用户指令撤销——AtomUI 需要让 `GroupBox` 支持 Semantic Part。
> 此项与 `SplitButton` / `Expander` / `TabStrip` / `Menu` 的撤销**判据不同**：那四项都能落到上游某个公开 owner 的
> 职责对应关系上，而 `GroupBox` 在 Ant Design 6.6.3 稳定发布源码中**没有**可映射的公开 owner（`components/` 下不存在
> fieldset、group 或 group-box 类组件，`GroupBox` 标识零命中）。因此本次纳入**不是**一次新的 §2.1 上游准入 Gate 通过，
> 而是用户直接指令下对排除判定的撤销，纳入依据是 AtomUI 自身需要，先例为 `SplitButton` 触发侧的能力补充。
> Part 从 AtomUI 自身模板职责设计（`root` / `header` / `icon` / `title` / `content`），移入第二批计划执行（见下方
> 批次清单与[第二批任务清单](2026-08-12-semantic-part-batch-2-collections-containers.md)任务 18）。

> 范围变更（2026-09-16）：`Splash` 的原排除判定经用户指令撤销——AtomUI 需要让 `Splash` 支持 Semantic Part。此项与
> `GroupBox` 同属“**无可映射上游 owner**、依据用户直接指令纳入”的一类，与 `SplitButton` / `Expander` / `TabStrip` /
> `Menu` 的“owner 数量 vs 职责对应”追认判据不同，不得混同：Splash 是桌面应用的启动反馈控件（“启动中但应用尚不可
> 交互”），上游稳定发布源码中不存在承载该职责的公开组件 owner，因此原触发条件「新稳定版出现对应公开 owner」并未发生。
> 纳入依据是用户直接指令与 AtomUI 自身需要，先例为 `SplitButton` 触发侧的能力补充。Part 从 AtomUI 自身模板职责设计
> （隐式 `root` 与 `logo` / `title` / `subtitle` / `content` / `spin` / `progressBar` / `message` / `detail` / `footer`），
> 移入第七批执行（见下方批次清单与本文档任务 7）。逐项证据见设计文档 §2.4。

### 批次进度

- [x] 第一批：基础控件，共 16 个家族。
- [x] 第二批：集合与容器，共 16 个家族。（2026-08-27 复核：全部家族均已按用户授权提交）
- [x] 第二批追加：`Expander`（2026-09-15 用户指令新增，原排除判定撤销；单面板折叠容器，映射上游 `Collapse` 面板 Semantic DOM，共 5 部件，见第二批任务 16）。（2026-09-15：已完成并经用户授权提交 `be7b8dc71`，含 Semantic Part 改造与圆角裁剪修复。）
- [x] 第二批追加：`TabStrip`（2026-09-15 用户指令撤销排除并追认；`TabStrip` / `CardTabStrip` / `TabStripItem` 三个 public owner 各自持有 descriptor，映射上游 `Tabs`，见第二批任务 17）。（2026-09-14：已随 `TabControl` 家族提交一并实施并持有独立 `semantic-part.md`；本次追认范围。）
- [x] 第三批：输入与选择，共 15 个家族。（2026-09-10 复核：15 个家族全部按用户授权提交；视觉验收 NumericUpDown、Form、Transfer、AutoComplete、Cascader 已关闭，ColorPicker、Select、DatePicker 待视觉验收，Mentions、TimePicker、TreeSelect 尚无验收文档；本批次收尾测试尚未执行。）
- [ ] 第三批追加：`ComboBox`（2026-09-16 用户指令，原排除判定彻底撤销；**实现完成、待用户验收与提交授权**；**非 §2.1 Gate 通过**——上游无公开 owner，准入依据为用户指令 + AtomUI `ComboBox` 自身为职责完整的独立 public owner，是本文档唯一的「非 Gate 通过」例外，见「不适用」段的范围变更说明与第三批任务 16）。
- [x] 第四批：Popup 与独立宿主，共 10 个家族。（2026-09-12 收尾：10 个家族全部按用户授权提交——ImagePreviewer、InfoFlyout、ToolTip、Tour、Drawer、DropdownButton、Message、PopupConfirm、Notification、Modal/Dialog；收尾验证 Desktop Controls 3737/3737、Generator 532/532、GalleryBase 181/181、Gallery 621/621、LLMS verify、NativeAOT `osx-arm64` 通过；真机视觉验收已关闭的家族见各自 `docs/superpowers/specs/` 验收记录，Modal/Dialog 悬停高亮由自动化回归覆盖。已知非阻塞：两条先于本批的间歇性测试抖动。）
- [x] 第四批追加：`SplitButton`（2026-09-15 用户指令新增，原排除判定撤销；弹层侧映射上游 `Dropdown` 5 部件，触发侧补充发布 `primary` / `secondary`，共 7 部件，见第四批任务 11）。（2026-09-15：已完成并经用户授权提交 `44cd1494a`「feat(Semantic): 增加 SplitButton 语义部件并修复弹层钉住与子菜单状态递归」；第四批任务 11 的逐项检查项待回填。）
- [x] 第五批：高密度控件，共 2 个家族。（2026-09-14：`NavMenu`（`1e22ed1a3`）与 `DataGrid`（`454cc0d00`）均已按用户授权提交，各自持有独立提交。`DataGrid` 属可选包，验证走独立工程 `tests/AtomUI.Desktop.Controls.DataGrid.Tests`。**遗留：** 三套控件的改造前后性能基线从未归档。详见[第五批任务清单](2026-08-12-semantic-part-batch-5-high-density.md)。）
- [x] 第五批追加：`Menu`（2026-09-15 用户指令撤销排除并追认；与 `NavMenu` 映射同一个上游 `Menu` owner，12 个公开键路径逐字相同，共 11 部件 + 隐式 `root`，见第五批任务 3）。（2026-09-14：已随 `1e22ed1a3` 提交一并实施并持有独立 `semantic-part.md`；本次追认范围。）
- [ ] 第二批追加：`GroupBox`（2026-09-16 用户指令新增，原排除判定撤销；**无上游 owner**，Part 按 AtomUI 自身模板职责设计，共 4 部件 + 隐式 `root`，见第二批任务 18）。
- [x] 第六批：`ButtonSpinner`，1 个输入基座控件家族（2026-09-16 用户指令新增，原排除判定撤销；映射上游 `InputNumber` 公开并实际消费的 `root` / `prefix` / `suffix` / `input` / `actions` 分区键，见任务 6）。（2026-09-16：已完成 Gate A / Gate B，并经用户授权提交。）
- [ ] 第七批：`Splash`，1 个桌面启动反馈控件家族（2026-09-16 用户指令新增，原排除判定撤销；**无上游 owner**，Part 按 AtomUI 自身模板职责设计，共 9 部件 + 隐式 `root`，见任务 7）。

合计待改造：67 个控件家族（原 59 个 + 2026-09-15 新增的 `SplitButton`、`Expander`、`TabStrip` 与 `Menu` + 2026-09-16 纳入的 `GroupBox`、`ButtonSpinner`、`ComboBox` 与 `Splash`）。**截至 2026-09-15，前 63 个家族全部已实现并经用户授权提交；`ButtonSpinner` 与 `Splash` 已完成 Gate A / Gate B 并经用户授权提交；`GroupBox` 已于 2026-09-16 完成真机视觉验收；`ComboBox` 已完成 Gate A（经用户批准）与 Gate B 实现。**

## 3. 单控件强制执行循环

以下流程适用于各批次清单中的每一个控件。批次任务会给出准确的文档、源码/主题范围、风险和附加验证要求。

### 单控件 Gate A：文档设计

**文件：**

- 修改：批次任务列出的准确 `overview.md`。
- 修改：批次任务列出的准确 `implementation.md`。
- 仅在符合创建条件时新增：与上述文件同目录的 `semantic-part.md`。

> **命名约定（2026-09-15 更正）：** 此处原文写的是 `semantic-part-design.md`，属笔误。
> `tools/AtomUI.Docs.LLMsGenerator/Catalog/ControlInventory.cs` 只按 `semantic-part.md` 发现控件语义契约
> （`GetOptionalFilePath(controlDirectory, "semantic-part.md")`），按原命名创建文件会被生成器完全忽略。已实测的
> 冲突记入[第五批任务清单](2026-08-12-semantic-part-batch-5-high-density.md)任务 2。
>
> 该文件是**可选**输入：缺失时生成器会回退到 `overview.md` 的内联「LLMS 语义区域」章节或默认占位表。
> 因此「文件不存在」不等于「该控件没有语义契约」——`DataGrid`、`OtpLineEdit` 就以 `overview.md` 内联表作为正式契约载体。
> 但回退路径也可能产出**与 descriptor 不符的占位内容**（已知实例：`SearchEdit`），评审时必须把生成的语义表与
> `*.SemanticParts.cs` 的实际声明逐条比对。

**输入证据：** 控件任务列出的 public/protected API、全部叶子主题、运行时 Visual 创建路径、子控件、Popup/Overlay 路径、item 生命周期、测试和 Gallery 证据。

**产出：** 经过批准的现行设计契约，其中包含准确的 Part/path、selector、ContractType、cardinality、customization、cross-root/runtime 标志、适用时的 ThemePropertyName、节点映射、兼容性边界和验证矩阵。

- [ ] 阅读控件文档、每个 public owner/子类型、每个叶子 `*Theme.axaml`、现有测试和 Gallery 页面。
- [ ] 建立 owner 类型、模板变体、`PART_*`、伪类、运行时 Visual、Popup/Overlay root、SizeType 接口和容器生命周期的事实表。
- [ ] 排除会暴露偶然 wrapper、internal 类型名或不稳定模板层级的候选区域。
- [ ] 在 `overview.md` 中写入最终 Semantic Parts 表以及定制与兼容性摘要。
- [ ] 在 `implementation.md` 中写入准确的节点 ownership、marker 放置位置、运行时创建路径、生命周期、布局、性能/AOT 和验证不变量。
- [ ] 对文档改动运行 LLMS verify 和 `git diff --check`。
- [ ] 停止并向用户展示文档差异。在获得明确批准前，不得编辑源码、主题、测试、Gallery 或 changelog。

### 单控件 Gate B：实现

**前置条件：** 用户已经明确批准该控件家族的 Gate A 文档设计。

**文件：**

- 修改：Gate A 已批准的准确 public owner 源码和主题家族。
- 测试：该控件批次任务列出的现有测试目录/工程和聚焦的 Semantic Part 测试文件。
- 修改：控件现有的 Gallery 页面与本地化资源。
- 修改：控件的 `changelog.md`。

**产出：** 与已批准文档一致的生成式 Descriptor 声明、静态/运行时 marker、聚焦测试和真正延迟创建的 Gallery Semantic Parts 示例。

- [ ] 先编写失败的 Descriptor 测试，验证准确的 Part 数量/顺序、ContractType、cardinality、标志和 `root` 行为。
- [ ] 为每个已批准 marker 和模板变体编写失败的模板/运行时测试。
- [ ] 向 public owner 类型添加 `[SemanticPart]` 声明，并依赖生成的常量和 Descriptor 注册。
- [ ] 为静态模板节点添加 `Classes.semantic-*="True"`，为运行时创建节点使用生成常量添加 marker。
- [ ] 当契约开放布局属性时，验证布局 Setter 与 SizeType、Min/Max、shape 和 owner Measure/Arrange 的关系。
- [ ] 当批次风险要求时，验证 Popup/Overlay 的打开-关闭-重新打开流程，或容器的 prepare-clear-recycle 流程。
- [ ] 向 Gallery 页面添加 `SemanticPartsContentTemplate`，首次选择 Semantic Parts Tab 前不得实例化任何 Semantic 内容。
- [ ] 更新 `changelog.md`；运行目标控件测试、Gallery 测试、LLMS verify 和 `git diff --check`。
- [ ] 当批次任务要求验证 Popup、Window、可选包或运行时注册时，执行 NativeAOT publish。
- [ ] 保持全部实现改动未提交并停止，向用户展示测试和视觉证据。

### 单控件 Gate C：提交

**前置条件：** 用户已经明确批准 Gate B 的未提交实现，并要求创建提交。

- [ ] 检查 `git status`，识别并隔离用户的无关改动。
- [ ] 只暂存已批准控件家族的文档、源码、主题、测试、Gallery，以及仓库有意跟踪的生成构建产物。
- [ ] 审查已暂存差异，并生成一条范围明确且符合 AtomUI 风格的提交信息。
- [ ] 只为该控件家族创建一个提交。
- [ ] 报告 commit hash，不触碰无关改动。

## 4. 批次顺序

### 任务 1：第一批 - 基础控件

**计划：** [第一批任务清单](2026-08-12-semantic-part-batch-1-basic-controls.md)

- [x] 完成 `Badge` 的 Gate A 至 Gate C 全流程。
- [x] 严格按照第一批计划中的顺序逐个处理控件家族。
- [x] 16 个家族全部提交后，运行完整 Desktop Controls、Gallery、Generator 和 LLMS 检查，再将该批次标记为完成。

### 任务 2：第二批 - 集合与容器

**计划：** [第二批任务清单](2026-08-12-semantic-part-batch-2-collections-containers.md)

**状态（2026-08-27）：** Calendar、Collapse、ListView / ListBox、Segmented、Tag、Timeline、TreeView、Slider、Masonry、Space、Splitter、Breadcrumb、Pagination、Steps、TabControl 家族（含 TabStrip、CardTabStrip）已全部完成经用户授权的提交。

**追加（2026-09-15）：** `Expander` 已纳入本批次（原排除判定撤销），完成 Gate A / Gate B / 真机视觉验收并经用户授权提交 `be7b8dc71`，本批次家族数由 16 增至 17。

**追加（2026-09-15）：** `TabStrip` 已纳入本批次（原排除判定撤销并追认）。它此前已随 `TabControl` 家族提交一并实施——
`TabStrip` / `CardTabStrip` / `TabStripItem` 三个 public owner 各自持有 descriptor，并有独立
`docs/controls/desktop/navigation/tab-strip/semantic-part.md`——本次补齐范围记录，本批次家族数由 17 增至 18。
去重说明：上段「TabControl 家族（含 TabStrip、CardTabStrip）」指的是三者共享 `ControlTheme` 资源边界；
按设计文档 §3「两个文档叶子即使共享源码目录，也必须分别完成 Gate A」，它们是**独立的控件家族**，因此单独计数。

**追加（2026-09-16）：** `GroupBox` 已纳入本批次（原排除判定撤销，用户直接指令）。与上面的追认不同，它是**全新待改造**家族
（不是补齐范围记录），且**没有**上游对应 owner，本批次家族数由 18 增至 19。见
[第二批任务清单](2026-08-12-semantic-part-batch-2-collections-containers.md)任务 18。

- [x] 只有第一批形成稳定审核节奏后才能开始，除非用户明确调整优先级。
- [x] 每个适用家族都必须提供容器和运行时创建 marker 的生命周期证据。
- [ ] 19 个家族全部提交后，运行集合/虚拟化回归测试和完整通用检查。
    - 2026-08-27 复跑受阻：`ImageLoaderDisposeTests.Dispose_On_UI_Thread_Does_Not_Block_An_InFlight_UI_Dispatch`（`d58243b15` 引入，非 Semantic Part 改动）在本机 headless 下稳定挂起并中止套件；其前 201 个用例通过。此外 Gallery 套件另有与本改造无关的存量失败（CustomizeTheme 算法断言）。上述存量问题修复后需完整重跑再勾选。
    - 2026-09-15 复核：上述两类存量问题已不复现——Desktop Controls **4031/4031** 全绿、Gallery 644/644 全部通过。批次收尾检查仍待其余追加范围（`SplitButton`、`TabStrip`）落定后统一执行。

### 任务 3：第三批 - 输入与选择

**计划：** [第三批任务清单](2026-08-12-semantic-part-batch-3-input-selection.md)

**追加（2026-09-16）：** `ComboBox` 已纳入本批次（原排除判定彻底撤销）。它不复用 `Select` 的 internal combobox mode，
而是直接派生 Avalonia `ComboBox` 并自有输入框、handle、模板内 Popup 与候选容器创建路径，因此以自身 public owner 身份
发布语义契约；上游 6.6.0 无公开 `ComboBox` owner，本项**不是 §2.1 Gate 通过**，准入依据为用户指令，是本清单唯一的
「非 Gate 通过」例外。本批次家族数由 15 增至 16。

**进度（2026-09-03）：** Upload、LineEdit（含 TextArea）、SearchEdit、OtpLineEdit、NumericUpDown、Form、Transfer、AutoComplete、Cascader 已提交；NumericUpDown、Form、Transfer、AutoComplete、Cascader 五家族视觉验收已关闭（NumericUpDown、Form 经用户截图走查后确认通过；Transfer、AutoComplete 由用户授权豁免关闭；Cascader 经用户授权按裁剪范围关闭——弹层钉住与 1.6 静态判定截图确认通过，基线回归走查项裁剪，记录见 docs/superpowers/specs/ 各验收文档）；AutoComplete 的暂存实现曾随专用工作树（`feature/semantic-AutoComplete`）丢失，后已按原计划重做并提交；ColorPicker、DatePicker、Mentions、Select、TimePicker、TreeSelect 未开始。

**进度（2026-09-10 复核）：** 上述“未开始”的 6 个家族随后均已按用户授权提交：ColorPicker（`9659cc7e0`，2026-09-04）、Mentions（`96930c96c`）、Select（`6f7c3bac2`）、TreeSelect（`bf570d14e`）、DatePicker（`1cc89992e`）、TimePicker（`683b7dc71`，均 2026-09-05～09-07）。视觉验收：ColorPicker、Select、DatePicker 仍为“待视觉验收”，对应 `docs/superpowers/specs/` 文档未收到用户回传截图；Mentions、TimePicker、TreeSelect 尚未建立验收文档。故 15 个家族已全部提交，但本批次收尾测试与完整视觉验收尚未关闭。

- [ ] 开放 input frame/content/icon 区域前，必须分析 SizeType 和布局 Setter。
- [ ] candidate、option、calendar 和 time panel 必须提供 Popup 打开-关闭-重新打开的证据。
- [ ] 16 个家族全部提交后，运行输入、选择、本地化和 Gallery NativeAOT 验证。（2026-09-16：家族数由 15 增至 16，含当日追加的 `ComboBox`。）

### 任务 4：第四批 - Popup 与独立宿主

**计划：** [第四批任务清单](2026-08-12-semantic-part-batch-4-hosts-windows.md)

**进度（2026-09-11 范围复核）：** ImagePreviewer（`08aa3e6ef`、`f32e2b258`）、InfoFlyout（`e067b12a4`）、ToolTip（`5fe34ab8a`）、Tour（`b6ee2e315`）、Drawer（`e415b81f9`）、DropdownButton（`6abaf6100`）、PopupConfirm（`62e4c4487`）已按用户授权提交，共 7/10；Message、Modal / Dialog、Notification 未开始。已交付家族的 LLMS 生成产物在 2026-09-10 复核时补齐（Drawer、Tour 原提交遗漏 `docs/AI/generated` 重生成，且 Drawer 文档存在禁用外部项目名）。DropdownButton 以自身 public owner 直接拥有下拉命令弹层，语义契约映射上游 `Dropdown`，不映射 deprecated `Dropdown.Button`。

- [ ] 每项设计获批前，必须明确 Visual root ownership 和释放路径。
- [ ] 测试多宿主隔离、关闭/detach 清理和 Gallery `AdditionalRoots`，不得引入生产 Preview API。
- [ ] 10 个家族全部提交后，运行 Popup/Overlay 检查和 Gallery NativeAOT publish。

### 任务 5：第五批 - 高密度控件

**计划：** [第五批任务清单](2026-08-12-semantic-part-batch-5-high-density.md)

**追加（2026-09-15）：** `Menu` 已纳入本批次（原排除判定撤销并追认）。它此前已随 `NavMenu` 提交 `1e22ed1a3` 一并实施——
与 `NavMenu` 映射同一个上游 `Menu` owner、12 个公开键路径逐字相同，持有独立
`docs/controls/desktop/navigation/menu/semantic-part.md`——本次补齐范围记录，本批次家族数由 2 增至 3。

- [ ] 每个控件进入 Gate B 前，记录改造前 marker、容器和性能基线。

> 2026-09-16 复核：已尝试补齐三套控件的性能与状态取证，结果见[第五批任务清单](2026-08-12-semantic-part-batch-5-high-density.md)批次收尾第 2 项。
> 已取得 NavMenu / DataGrid 的**现状**基线（各 8 场景）与 DataGrid 状态验证通过证据；**改造前基线仍缺**——
> `tools/performances/AtomUI.Performance` 在 `release/6.0` 上无法编译（引用已不存在的 `StepsStyle` 等类型），
> 需先修复该工具才能形成前后对比。本项**保持未完成**。

- [x] 必须证明可见实例数量受限、回收正确，并且没有永久监听器或索引。（2026-09-14：`NavMenuContainerLifetimeTests`、`NavMenuEntryContainerTests`、`MenuSemanticLevelTests`、`MenuSubmenuOpenStateRecursionTests`、`DataGridSemanticPartTests`（回收保持）、`DataGridSemanticPartHighlightTests` 已落地；三者均通过容器生命周期与 marker 稳定性断言。）
- [ ] 3 个家族全部提交后，运行各自完整测试工程、性能检查、Gallery 和 NativeAOT 验证。（2026-09-16 部分完成：Desktop Controls 4031/4031、DataGrid 293/293、Generator 528/528、GalleryBase 185/185、Gallery 644/644 全绿；**性能检查已执行**——NavMenu 与 DataGrid 各取得 8 场景现状基线，DataGrid 状态验证通过；Gallery NativeAOT publish 未能取证（沙箱阻止 AvaloniaUI BuildServices 写入，挂起 1 小时 6 分无 artifact，见下）。）

### 任务 6：第六批 - `ButtonSpinner`

**计划：** 本任务即第六批清单（单一控件家族，不另建批次文件）。

**状态（2026-09-16）：** Gate A 文档设计进行中。原排除判定经用户指令撤销，准入证据为上游 `InputNumber` 公开并实际
消费的 `actions` 分区键（`antd@6.6.4`），逐项理由、两点边界与计数调整见
[改造设计 §2.4](../specs/2026-08-12-semantic-part-control-rollout-design.md)。

本批次与第三批 `NumericUpDown` 存在**共享帧**关系，这是本批最重要的风险点：

- `ButtonSpinner` 与 `NumericUpDown` 共用 `ButtonSpinnerDecoratedBox` 帧模板；`NumericUpDownSpinner`
  （`ButtonSpinner` 的派生控件）在 `Mode=Spinner` 下使用自有模板。
- `NumericUpDown` 已发布的 `prefix` route 是宽松后代
  （`/template/ .semantic-scope-spinner >> .semantic-prefix`），容忍帧节点增加 class。因此 **ButtonSpinner 不得为
  帧内容槽复用 `semantic-prefix` / `semantic-suffix`**，否则该 route 会命中两个节点，破坏 `NumericUpDown` 已发布的
  `Single` 契约及其现有测试（该测试以 `.Single()` 解析目标）。
- 本批必须把 `NumericUpDownSemanticPartTests` 作为强制回归护栏，而不只是跑本控件的测试。

- [ ] 完成 `ButtonSpinner` 的 Gate A：`overview.md` / `implementation.md` 更新 + 新增 `semantic-part.md`，经用户审核。
- [ ] 完成 Gate B：`[SemanticPart]` 声明、`Classes.semantic-*` marker、失败优先的 descriptor/marker/生成 Style 命中测试、
      Gallery Semantic Parts Tab 与延迟创建、`changelog.md`。
- [ ] 实现期间逐步验证五个 `CrossNestedOwners` 部件（帧 `content` / `innerLeftContent` / `innerRightContent` 三个，
      手柄 `increaseButton` / `decreaseButton` 两个）的跨资产 marker 校验通过。
- [ ] 验证 `NumericUpDownSemanticPartTests` 仍全绿，且 `NumericUpDown` 四个部件的唯一目标解析未被破坏。
- [ ] 按 `semantic-part.md` §6 的尺寸基线验证布局型 Setter 与手柄占位、`ContentRightShift` 位移和帧裁剪的协调结果。
- [ ] 本家族全部提交后，运行 Desktop Controls、Gallery、Generator 与 LLMS 完整检查。
- [ ] 真机视觉验收按仓库全局强约束执行：先产出书面步骤（含 Gallery 分类与入口路径），以用户回传截图/录屏为唯一
      视觉证据。

### 任务 7：第七批 - `Splash`

**计划：** 本任务即第七批清单（单一控件家族，不另建批次文件）。

**状态（2026-09-16）：** Gate A / Gate B 实现进行中。原排除判定经用户指令撤销，纳入依据是用户直接指令与 AtomUI
自身的启动页职责——上游稳定发布源码中不存在承载“启动中但应用尚不可交互”职责的公开组件 owner，因此**不存在可映射的
上游 owner**，本批次不是一次 §2.1 上游准入 Gate 通过（与 `GroupBox` 同类）。逐项理由与计数调整见
[改造设计 §2.4](../specs/2026-08-12-semantic-part-control-rollout-design.md)。

本批次的核心事实与风险点：

- **Splash 是 `AtomUI.Desktop.Controls.Extras` 中首个采用 Semantic Part 的控件。** 该包此前生成的
  `GeneratedSemanticPartManifest` 返回空列表，也没有 `SemanticPartXmlnsDefinition.g.cs`；本批将首次为该包生成 descriptor、
  `Splash*Style` 类型与 `https://atomui.net` → `AtomUI.Theme.Styling` 的映射。必须确认新生成类型名与既有生成 Style 名
  无冲突（重名是 generation-blocking diagnostic）。
- 九个非 root Part 全部是 `SplashTheme.axaml` 单一模板内的静态节点，全部 `Single`，无 `Optional` / `Multiple`、无
  `RuntimeCreated`、无 `CrossVisualRoot`、无 `CrossNestedOwners`、无 `.semantic-scope-*` 锚点。
- 进度区刻意拆分为 `spin` 与 `progressBar` 两个 `Single` Part（不同控件、不同 Token、互斥可见），而不是合并为一个
  `Multiple` Part；后者会把 `ContractType` 放宽到 `TemplatedControl` 并丢失 `x:SetterTargetType` 上下文。
- `SplashWindow` 不发布 Semantic Part；其模板 `SplashWindowTheme.axaml` 不得出现任何 `Classes.semantic-*`。
- Gallery 专用的 `GalleryWindowSplash` 原本通过 `/template/` + `PART_*` 选择器穿透 Splash 模板，本批已迁移到生成的专用
  Semantic Part Style（系统设计 §5.4 的强约束），迁移必须保持 `:loading` / `:success` / `:error` 的可观察颜色不变。
- 进度区互斥可见与「预览跳过不可见目标」冲突：Semantic Parts 预览舞台必须放两个实例（确定态与不确定态），否则
  `spin` 与 `progressBar` 无法同时被定位高亮。

- [ ] 完成 `Splash` 的 Gate A（已完成设计并经用户审核）：`overview.md` / `implementation.md` 更新 + 新增
      `semantic-part.md`。
- [ ] 完成 Gate B：`Splash.SemanticParts.cs` 声明、`Classes.semantic-*` marker、失败优先的 descriptor/marker/生成 Style
      命中测试、状态矩阵与尺寸基线回归、Gallery Semantic Parts Tab 与专用 Style 演示、`changelog.md`。
- [ ] 验证 Extras 首次生成的 descriptor 与 `Splash*Style` 类型进入 PackageRegistration 与冻结 registry。
- [ ] 验证 `GalleryWindowSplash` 迁移后四个文本位的可观察前景色与状态色语义不变（既有 Gallery 测试已锁定）。
- [x] 修复预览舞台双实例被裁切的布局缺陷（2026-09-16）：单预览页签的画布高度在部分视口下被钳制，
      纵向堆叠的第二个实例被 `PART_PreviewContentHost` 静默裁掉。改为两列等分 `Grid` 并新增 12 档视口的
      参数化包含关系回归（实测 520×380 至 1920×1080 全部通过；把布局退回堆叠时该回归 12/12 失败）。
- [x] 修复样式示例徽标溢出 presenter 的缺陷（2026-09-16 复验发现）：`logo` Part 的尺寸作用在 presenter 上，
      而示例的 `LogoTemplate` 内容是固定 48×48，撑破 36×36 的 presenter（每边溢出 6 px），Part Setter 对可见尺寸失效。
      示例改为 `Stretch` 填满 presenter，并新增「可见徽标 Bounds 等于 Part 设定值」断言。
- [ ] 本家族全部提交后，运行 Desktop Controls、Gallery、Generator 与 LLMS 完整检查；Extras 首次纳入 Semantic Part，
      另需按 `semantic-part.md` §7 执行 Gallery NativeAOT publish 验证。
- [ ] 真机视觉验收按仓库全局强约束执行：先产出书面步骤（含 Gallery 分类与入口路径），以用户回传截图/录屏为唯一
      视觉证据。

## 5. 通用验证命令

文档门禁：

```bash
dotnet run --project tools/AtomUI.Docs.LLMsGenerator/AtomUI.Docs.LLMsGenerator.csproj -- verify --config docs/AI/generated/llms.config.json
git diff --check
```

Gate A 修改 `overview.md` / `implementation.md` / 新增 `semantic-part.md` 后，`verify` 会报告 `docs/AI/generated` 下的
`controls/<control>/index-cn.md`、`controls/<control>/semantic-cn.md`、`llms-full-cn.txt`、`llms-semantic-cn.md` 过期。
此时必须用生成器重生成，不得手工编辑 `docs/AI/generated`：

```bash
dotnet run --project tools/AtomUI.Docs.LLMsGenerator/AtomUI.Docs.LLMsGenerator.csproj -- generate --config docs/AI/generated/llms.config.json
```

Desktop 控件实现：

```bash
dotnet test tests/AtomUI.Generator.Tests/AtomUI.Generator.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~SemanticPart
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore
dotnet test tests/AtomUI.Toolkits.GalleryBase.Tests/AtomUI.Toolkits.GalleryBase.Tests.csproj --framework net10.0 --no-restore
dotnet test tests/AtomUIGallery.Tests/AtomUIGallery.Tests.csproj --framework net10.0 --no-restore
```

DataGrid 实现：

```bash
dotnet test tests/AtomUI.Desktop.Controls.DataGrid.Tests/AtomUI.Desktop.Controls.DataGrid.Tests.csproj --framework net10.0 --no-restore
```

Popup、Window、可选包或运行时敏感改动的 NativeAOT 验证：

```bash
export PATH="$HOME/.dotnet:$PATH"
export DOTNET_ROOT="$HOME/.dotnet"
pwsh -NoLogo -NoProfile -File controlgallery/AtomUIGallery.Desktop/scripts/PublishToLocal.ps1 -publishRootPath /tmp/atomui-gallery-aot-semantic -runtime osx-arm64 -buildType Release -publishAot true
```

> 前置说明（2026-09-15 实测）：脚本内部用 `& dotnet @Arguments` 调用 CLI，因此**必须先让 `dotnet` 进入 PATH**，
> 否则脚本尚未开始构建就在第 91 行抛出「`dotnet` is not recognized」。
> 另注：在受限沙箱/容器中该命令可能因拒绝 AvaloniaUI BuildServices 写临时文件而挂起（无输出、无 artifact），
> 需在允许写入的环境执行；判断依据是 `publishRootPath` 下是否出现实际产物，而不是进程是否仍在运行。

## 6. 全量改造收尾

- [x] 确认 61 个家族都通过各自经用户授权的提交达到 `Committed` 状态。（2026-09-15 复核：63 个家族全部已提交。按批次证据——第一批 16、第二批 16 + `Expander`（`be7b8dc71`）+ `TabStrip`（随 `TabControl` 提交）、第三批 15、第四批 10 + `SplitButton`（`44cd1494a`）、第五批 2 + `Menu`（随 `NavMenu` 提交 `1e22ed1a3`）。）

> 2026-09-16 追加：`GroupBox` 纳入后家族总数由 63 增至 64，`ButtonSpinner` 随后纳入为第六批总数 65，`ComboBox` 并入本分支
> 后在 `feature/semantic` 基线（已含 65）之上再计入，总数 66。其中 `ButtonSpinner` 已完成 Gate A / Gate B 并经用户授权提交；
> `GroupBox` 与 `ComboBox` 是当前未提交家族。因此本项的历史结论（63 个全部 `Committed`）仍成立，但“全部家族已提交”的当前
> 状态不再成立，需在二者完成验收并获授权提交后重新核对。
- [x] 重新扫描 public 控件和全部叶子主题，检查未声明的 `.semantic-*`、缺少的已批准 marker，以及 Descriptor 与文档不一致。（2026-09-15：两项扫描均已完成。
  **marker 差集：** 主题文件里 `Classes.semantic-*` 共 107 种，descriptor 声明的 `SelectorClass` 共 100 种，
  `semantic-scope-*` 锚点 23 种；主题 marker 集合与「声明 ∪ scope 锚点」的差集为**空**，即没有未声明的静态 marker。
  另有 16 个声明类只在代码中运行时注入（`semantic-body-row`、`semantic-item` 等），不出现在主题静态 marker 中，属预期而非缺失。
  **Descriptor 与文档一致性：** 全局集合比对（全部 116 个声明部件名 vs 全部目标家族生成文档的部件名）确认无遗漏；
  唯一不符合项 `SearchEdit` 已修正（其 `overview.md` 语义表原为生成器默认占位）。）
- [ ] 确认 13 个排除控件仍然没有通过最新稳定发布源码公开 API 准入 Gate。

> 2026-09-15 更新：`TabStrip` 与 `Menu` 的原排除判定已经用户指令撤销并追认，两项已从本文档「不适用」清单与设计文档 §2.4
> 移入 §2.3 纳入映射（`TabStrip` 随第二批、`Menu` 随第五批）。
>
> 2026-09-16 更新：`GroupBox`、`ButtonSpinner`、`ComboBox` 与 `Splash` 的原排除判定先后经用户指令撤销，从本文档「不适用」清单与设计文档
> §2.4 移入 §2.3 纳入映射（`GroupBox` 随第二批、`ComboBox` 随第三批、`ButtonSpinner` 随第六批、`Splash` 随第七批）。因此本项核对
> 范围由 16 个收缩为 14 个（`TabStrip` / `Menu` 外），再依次收缩为 13（`GroupBox`）、12（`ButtonSpinner`）、**11（`ComboBox`）**、
> **10（`Splash`）**。
>
> 四次撤销的判据各不相同，不得混同：`GroupBox`、`ComboBox` 与 `Splash` 在 Ant Design 稳定发布源码中**都没有**可映射的上游 owner，
> 纳入依据是用户直接指令（`ComboBox` 另有其自身即职责完整的独立 public owner 为依据），三者都**不是 §2.1 Gate 通过**；
> `ButtonSpinner` 有上游 `InputNumber` 的 `actions` 分区键支撑职责对应，但同样叠加了用户指令。逐项撤销理由与判据见设计文档
> §2.4 的日期化范围变更段，引用时不得省略，也不得据此推断其他排除项可被同样处理。

- [ ] 运行全部通用测试、DataGrid 测试、LLMS verify、NativeAOT publish 和 `git diff --check`。（2026-09-15 部分完成：Desktop Controls 4031/4031、DataGrid 293/293、Generator 528/528、GalleryBase 185/185、Gallery 644/644、Docs LLMsGenerator 23/23 全绿；LLMS verify 通过（79 控件 / 161 文件）；`git diff --check` 干净。
  **NativeAOT publish 未能取证：** 沙箱环境阻止 AvaloniaUI BuildServices 写临时文件，进程挂起 1 小时 6 分无任何 artifact 产出，已终止。详见[第五批任务清单](2026-08-12-semantic-part-batch-5-high-density.md)批次收尾第 3 项。该项需在非沙箱环境重跑。）
- [ ] 审核每个 Gallery 页面，确认保持 Examples-first 行为，并且选择 Tab 前不会实例化 Semantic Preview。
- [ ] 输出最终兼容性与性能摘要；除非用户明确要求，否则不得额外创建 squash 或批次提交。
