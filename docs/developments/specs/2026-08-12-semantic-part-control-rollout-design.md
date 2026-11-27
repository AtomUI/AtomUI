# AtomUI Desktop Controls Semantic Part 全量改造设计

本文档定义 AtomUI Desktop Controls 全量接入 Semantic Part 的范围、设计方法、审核关卡、性能边界和交付纪律。
Semantic Part 的公共模型、Selector 契约和生成器规则分别由
[Semantic Part 系统设计](../../architecture/systems/theming/semantic-parts.md) 与
[Semantic Part Generator](../../modules/generator/semantic-part-generator.md) 维护；本文档只负责把这些系统级能力稳定地
落到每一个控件家族。

## 1. 目标与完成定义

本轮改造的目标不是让每个控件都拥有相同数量或相同名称的 Part，而是逐一审计控件的真实视觉职责，只公开能够跨主题、
状态、平台和后续模板重构长期保持的 Semantic Part 契约。

一个控件家族只有同时满足以下条件，才算完成：

1. `overview.md`、`semantic-part.md` 与 `implementation.md` 已根据真实源码、叶子主题、测试和 Gallery 分别定义
   支持摘要、公共 Part 契约和真实节点映射。
2. 控件设计文档已经由用户审核通过，之后才开始实现。
3. 所有声明均由 `[SemanticPart]` 和生成式 descriptor 表达，所有静态节点均使用 `Classes.semantic-*="True"`。
4. Desktop、Browser、派生主题、运行时节点、Popup、Overlay、独立 TopLevel 和 ItemContainer 路径按实际适用范围完成。
5. descriptor、marker、selector、布局、容器回收、Popup、性能和 Gallery 延迟创建验证覆盖实际风险。
6. `changelog.md` 与 LLMS 人工源文档同步，`docs/AI/generated` 未被手工修改。
7. Gallery Semantic 示例的标题、说明和版本 Tag 已按 8.1 的文案与版本门禁逐项验收，不包含 React/DOM 专属术语或上游
   组件版本号。
8. 实现保持未提交状态，用户验收通过并明确要求后，才为该控件家族创建一个独立 commit。

## 2. 范围基线

截至 2026-08-12，npm `latest` 指向 Ant Design 6.6.0；本轮范围以该稳定发布源码为审计基线。`docs/controls/desktop`
下共有 78 个正式控件文档叶子。

| 状态 | 数量 | 范围 |
| --- | ---: | --- |
| 已完成基线 | 1 | `Button` |
| 本轮待改造 | 67 | 五个批次中与稳定版公开 Semantic DOM API 对应的控件家族（含 2026-09-15 新增的 `SplitButton`、`Expander`、`TabStrip` 与 `Menu`，2026-09-16 新增的 `GroupBox`、`ButtonSpinner`、`ComboBox` 与 `Splash`） |
| 不适用 | 10 | 没有对应公开 API、只有内部/间接能力或产品职责不对应的控件家族 |

> 2026-09-15 计数核对：本节三行合计 78，与 78 个正式控件文档叶子一致；`本轮待改造` 与 `不适用` 已与 §2.3、§2.4 的行数
> 对齐。原记录为 58 / 19，在 `SplitButton` 增补时未同步，随后随 `Expander` 撤销排除校正为 61 / 16；`TabStrip` 与
> `Menu` 撤销排除后调整为 63 / 14。每次撤销排除都必须重算本表，不要增量累加后忘记总数。
>
> 2026-09-16 计数核对（当日两次纳入合并）：`GroupBox` 与 `ButtonSpinner` 先后纳入后，三行为 1 / 65 / 12，合计仍为 78。
> 两次纳入的判据不同，不得混同：`GroupBox` **没有**可映射的上游 owner（见 §2.4 该行的日期化记录），纳入依据是用户直接
> 指令，而非 §2.1 的上游准入 Gate；`ButtonSpinner` 有上游 `InputNumber` 的 `actions` 分区键支撑职责对应，但同样叠加了
> 用户指令。重算本表时不要把前者错误归因于上游 owner 出现。
>
> 2026-09-16 计数核对（当日四次纳入：`GroupBox`、`ButtonSpinner`、`ComboBox`、`Splash`）：四者先后撤销排除后，三行为
> 1 / 67 / 10，合计仍为 78。四次纳入的判据各不相同，不得混同：`GroupBox`、`ComboBox` 与 `Splash` 在 Ant Design 稳定发布
> 源码中**都没有**可映射的上游 owner，纳入依据是用户直接指令（`ComboBox` 另有其自身即职责完整的独立 public owner 为依据），
> 三者都**不是 §2.1 Gate 通过**；`ButtonSpinner` 有上游 `InputNumber` 的 `actions` 分区键支撑职责对应，但同样叠加了用户指令。
> 重算本表时不得把前三者错误归因于上游 owner 出现，也不得据此推断其他排除项可被同样处理。

`Button` 是首个完整样例，用于校验 descriptor、静态 marker、Selector、尺寸协调和 Gallery Preview 的全链路；它不作为
其他控件 Part 命名的机械模板。

### 2.1 唯一准入 Gate

一个 AtomUI 控件家族只有同时满足以下条件，才能进入 Semantic Part 改造：

1. Ant Design 当前最新稳定发布源码中存在可从组件包公开入口访问的对应组件 owner。
2. 该 owner 的公开 Props 明确声明分区式 `classNames` / `styles` Semantic DOM API，而不是只有普通 `className` / `style`。
3. 组件实现实际读取、合并并应用调用方传入的 Semantic DOM 值；仅类型继承但实现丢弃参数不算支持。
4. AtomUI 控件与该公开 owner 的产品职责直接对应；内部 mode、嵌套子组件、Popup 内容或名称相似不能建立映射。

下列证据不能单独构成准入资格：

- 官网示例、Semantic DOM 图示或文案。
- `ConfigProvider` 中按组件名配置的 `classNames` / `styles`。
- internal context、helper、schema、测试夹具或私有组件使用的 Semantic 类型。
- 普通 root `className` / `style`、deprecated overlay class/style 或底层 DOM 属性。
- 从另一个组件继承到 Props、但当前组件实现没有消费的 `classNames` / `styles`。
- 通过组合的 `Button`、`Menu`、`Select`、`Popover` 等子组件间接获得的能力。

新的 Ant Design 稳定版本发布后，不自动扩大 AtomUI 范围。必须重新审计其公开 Props、包导出和实现消费点，并先更新本
spec 与任务清单。预发布分支、未发布主干和内部实验 API 不参与判定。

### 2.2 稳定版公开 API 盘点

Ant Design 6.6.0 稳定发布源码中，以下 61 个组件目录具有可验证的公开 Semantic DOM owner：

```text
alert anchor auto-complete badge breadcrumb button calendar card cascader checkbox collapse
color-picker date-picker descriptions divider drawer dropdown empty float-button form image input
input-number layout list listy masonry mentions menu message modal notification pagination popconfirm
popover progress qr-code radio result segmented select skeleton slider space spin splitter statistic
steps switch table tabs tag time-picker timeline tooltip tour transfer tree tree-select typography upload
```

目录名不等于目录内所有公开组件都获得 API：

- `layout` 中只有公开 `Layout.Sider` owner 具有 Semantic DOM API。
- `list` 中公开分区 API 位于 `List.Item`，并且只覆盖 `actions`、`extra`；不能把它扩大为 `List` root 的完整 API。
- `collapse` 的 schema 属于 `Collapse`；`Collapse.Panel` 没有独立公开 Semantic DOM Props。
- `dropdown` 的 schema 属于 `Dropdown`；deprecated `Dropdown.Button` 虽在类型上继承 `DropdownProps`，实现没有消费调用方传入
  的 `classNames` / `styles`，不能作为独立 Semantic DOM owner。
- `tabs` 的 schema 属于 `Tabs`；不存在独立公开 `TabStrip` Semantic DOM owner。AtomUI `TabStrip` / `CardTabStrip` / `TabStripItem`
  因此与 `TabControl` 映射同一个上游 `Tabs` owner，作为独立文档叶子单独通过 Gate A（2026-09-15 用户指令纳入）。

### 2.3 AtomUI 纳入映射

下表覆盖 68 个准入家族，其中 `Button` 已完成，其余 67 个进入实施批次；`SplitButton`（原排除判定撤销，随第四批执行）与
`Expander`（原排除判定撤销，随第二批执行）为 2026-09-15 用户指令新增，`GroupBox`（无上游 owner，纳入依据与本表其他行不同，
见 §2.4）、`ButtonSpinner`（原排除判定撤销，随第六批执行）、`ComboBox`（原排除判定撤销，随第三批执行）与 `Splash`
（无上游 owner，纳入依据见 §2.4）为 2026-09-16 用户指令新增。映射只证明“允许进入 Gate A”，不预先
承诺具体 Part 名称或数量；每个 Part 仍必须从 AtomUI 自身源码、主题和生命周期事实中设计。

> 计数核对（2026-09-16）：本节表格行数 68，与 §2 的「已完成基线 1 + 本轮待改造 67」一致；68 + §2.4 的 10 = 78，与
> 78 个正式控件文档叶子一致。原文「覆盖 63 个准入家族 / 其余 62 个进入实施批次」在 `SplitButton`、`Expander`、`TabStrip`
> 与 `Menu` 相继纳入后未同步，本次一并校正；`GroupBox`、`ButtonSpinner`、`ComboBox`、`Splash` 四次纳入后再次重算。

| AtomUI 控件家族 | Ant Design 6.6.0 公开 owner | 结论 |
| --- | --- | --- |
| `Button` | `Button` | 已完成基线 |
| `Badge` | `Badge`、`Badge.Ribbon` | Batch 1 |
| `Card` | `Card`、`Card.Meta` | Batch 1 |
| `Descriptions` | `Descriptions`、`Descriptions.Item` | Batch 1 |
| `Empty` | `Empty` | Batch 1 |
| `QRCode` | `QRCode` | Batch 1 |
| `Statistic` | `Statistic` | Batch 1 |
| `Alert` | `Alert` | Batch 1 |
| `ProgressBar` | `Progress` | Batch 1 |
| `Result` | `Result` | Batch 1 |
| `Skeleton` | `Skeleton` | Batch 1 |
| `Spin` | `Spin` | Batch 1 |
| `FloatButton` | `FloatButton`、`FloatButton.Group` | Batch 1 |
| `Separator` | `Divider` | Batch 1 |
| `CheckBox` | `Checkbox` | Batch 1 |
| `RadioButton` | `Radio` | Batch 1 |
| `ToggleSwitch` | `Switch` | Batch 1 |
| `Calendar` | `Calendar` | Batch 2 |
| `Collapse` | `Collapse` | Batch 2 |
| `Expander` | `Collapse` | Batch 2 范围新增（2026-09-15 用户指令）；AtomUI 单面板折叠容器与上游 `Collapse` 的单个面板承担同一产品职责，映射上游 `Collapse` 已公开的 `root` / `header` / `icon` / `title` / `body` 五个语义键；上游 owner 数量不是准入必要条件，职责直接对应才是（§2.1 第 4 条） |
| `ListBox` | `Listy` | Batch 2；Gate A 按 `root` / `item` 语义键设计（无分组，`groupHeader` 不适用） |
| `ListView` | `Listy` | Batch 2；Gate A 按 `root` / `item` / `groupHeader` 语义键设计（6.6.0 新组件，旧 `List` 已 deprecated） |
| `Segmented` | `Segmented` | Batch 2 |
| `Tag` | `Tag` | Batch 2 |
| `Timeline` | `Timeline` | Batch 2 |
| `TreeView` | `Tree` | Batch 2 |
| `Slider` | `Slider` | Batch 2 |
| `Masonry` | `Masonry` | Batch 2 |
| `Space` | `Space` | Batch 2 |
| `Splitter` | `Splitter` | Batch 2 |
| `Breadcrumb` | `Breadcrumb` | Batch 2 |
| `Pagination` | `Pagination` | Batch 2 |
| `Steps` | `Steps` | Batch 2 |
| `TabControl` | `Tabs` | Batch 2 |
| `TabStrip` | `Tabs` | Batch 2 范围新增（2026-09-15 用户指令，原排除判定撤销）；与 `TabControl` 映射同一个上游 `Tabs` owner，但作为独立文档叶子单独完成 Gate A：`TabStrip` / `CardTabStrip` 是独立 public owner 的页签条，`TabStripItem` 是二者的 item container。上游 owner 数量不是准入必要条件，职责直接对应才是（§2.1 第 4 条） |
| `GroupBox` | 无上游 owner | Batch 2 范围新增（2026-09-16 用户指令，原排除判定撤销）；Ant Design 6.6.3 稳定发布源码中不存在职责对应的公开 Semantic DOM owner，纳入依据是用户直接指令与 AtomUI 自身的分组容器职责，**不是** §2.1 的上游准入 Gate。Part 按 AtomUI 自身模板职责设计（`root` / `header` / `icon` / `title` / `content`），参照上游 `Card` 的分区键命名，不借用 `Card` 的 owner 资格，先例为 `SplitButton` 触发侧的能力补充；详见 §2.4 |
| `AutoComplete` | `AutoComplete` | Batch 3 |
| `Cascader` | `Cascader` | Batch 3 |
| `ColorPicker` | `ColorPicker` | Batch 3 |
| `ComboBox` | 无（AtomUI 自有 public owner） | Batch 3 范围新增（2026-09-16 用户指令，原排除判定撤销）；**不是 §2.1 Gate 通过**。上游没有公开 `ComboBox` owner，本项不以上游 API 为依据，而是承认 AtomUI `ComboBox` 作为独立 public owner 自身的输入框、弹层与候选列表职责，其区域分组与上游 `Select` 已公开的 `classNames` / `styles` 分组直接对应；命名参考 `Select`，不发明上游没有的键。原排除依据「`Select` 的 internal combobox mode 不能作为公开 owner」**仍然成立**——本次依据的不是 `Select` 的内部 mode，而是 `ComboBox` 自己就是公开 owner。`GroupBox` 存在同类的「非 Gate 通过」撤销诉求，但其记录在并行的 `feature/semantic-groupbox` 工作中，本分支尚未登记，不作为本项先例引用 |
| `DatePicker` | `DatePicker`、`RangePicker` | Batch 3 |
| `Form` | `Form` | Batch 3 |
| `LineEdit` | `Input`、`Input.Password`、`Input.TextArea` | Batch 3 |
| `Mentions` | `Mentions` | Batch 3 |
| `NumericUpDown` | `InputNumber` | Batch 3 |
| `ButtonSpinner` | `InputNumber` | Batch 6 范围新增（2026-09-16 用户指令，原排除判定撤销）；AtomUI 输入基座，映射上游 `InputNumber` 公开并实际消费的 `root` / `prefix` / `suffix` / `input` / `actions` 分区键。不复制键名：帧内主内容区发布为 `content`、帧内容左/右槽发布为 `innerLeftContent` / `innerRightContent`，避免与 `NumericUpDown` 已发布的 `Single` route 冲突；单个上/下按钮为显式能力补充。理由与边界见 §2.4 日期化撤销段 |
| `OtpLineEdit` | `Input.OTP` | Batch 3 |
| `SearchEdit` | `Input.Search` | Batch 3 |
| `Select` | `Select` | Batch 3 |
| `TimePicker` | `TimePicker` | Batch 3 |
| `Transfer` | `Transfer` | Batch 3 |
| `TreeSelect` | `TreeSelect` | Batch 3 |
| `Upload` | `Upload` | Batch 3 |
| `DropdownButton` | `Dropdown` | Batch 4；AtomUI owner 直接拥有 `DropdownFlyout` / `MenuFlyout` 命令弹层，映射上游 `Dropdown` 的弹层 Semantic DOM，不映射 deprecated `Dropdown.Button` 的 split-trigger 组合结构 |
| `SplitButton` | `Dropdown` | Batch 4 范围新增（2026-09-15 用户指令）；AtomUI owner 直接拥有 `Flyout` / `MenuFlyout` 命令弹层，映射上游 `Dropdown` 的弹层 Semantic DOM，不映射 deprecated `Dropdown.Button` 的 split-trigger 组合结构；触发侧补充发布 `primary` / `secondary`——上游消费者自持触发按钮，AtomUI 触发按钮为模板内部件，属显式能力补充 |
| `ImagePreviewer` | `Image`、`Image.PreviewGroup` | Batch 4 |
| `InfoFlyout` | `Popover` | Batch 4 |
| `ToolTip` | `Tooltip` | Batch 4 |
| `Tour` | `Tour` | Batch 4 |
| `Drawer` | `Drawer` | Batch 4 |
| `Message` | `message` | Batch 4 |
| `Modal / Dialog` | `Modal` | Batch 4 |
| `Notification` | `notification` | Batch 4 |
| `PopupConfirm` | `Popconfirm` | Batch 4 |
| `NavMenu` | `Menu` | Batch 5；两者均是层级页面/模块导航 owner |
| `Menu` | `Menu` | Batch 5 范围新增（2026-09-15 用户指令，原排除判定撤销）；与 `NavMenu` 映射同一个上游 `Menu` owner、公开键路径逐字相同（`root` / `itemTitle` / `list` / `item` / `itemIcon` / `itemContent` / `subMenu.*` / `popup.root`，按 6.6.3 稳定发布源码审计）。AtomUI `Menu` 是桌面命令、ContextMenu 与 MenuFlyout 家族，与上游 `Menu` 共用同一套菜单语义键，职责直接对应（§2.1 第 4 条） |
| `DataGrid` | `Table` | Batch 5 |
| `Splash` | 无上游 owner | Batch 7 范围新增（2026-09-16 用户指令，原排除判定撤销）；上游稳定发布源码中不存在承载桌面启动反馈职责的公开 Semantic DOM owner，纳入依据是用户直接指令与 AtomUI 自身的启动页职责，**不是** §2.1 的上游准入 Gate。Part 按 AtomUI 自身模板职责设计（`root` 隐式，以及 `logo` / `title` / `subtitle` / `content` / `spin` / `progressBar` / `message` / `detail` / `footer`），命名对齐 `Result` / `Empty` / `Alert` / `GroupBox` 的同职责键；详见 §2.4 日期化撤销段 |

### 2.4 排除映射

以下控件当前不新增 Semantic Part：

| 控件 | 判定依据 | 重新评估触发条件 |
| --- | --- | --- |
| `Avatar` | Ant Design `AvatarProps` 与 `AvatarGroupProps` 没有公开分区式 `classNames` / `styles`；ConfigProvider 测试不构成组件 API。 | 新稳定版公开并实际消费对应 API。 |
| `Carousel` | Ant Design 稳定版 `Carousel` 没有公开 Semantic DOM Props。 | 新稳定版公开并实际消费对应 API。 |
| `Rate` | Ant Design 稳定版 `Rate` 没有公开 Semantic DOM Props。 | 新稳定版公开并实际消费对应 API。 |
| `Watermark` | Ant Design 稳定版 `Watermark` 没有公开 Semantic DOM Props。 | 新稳定版公开并实际消费对应 API。 |
| `Icon` | Ant Design Icons 不提供与 AtomUI `Icon` 对应的公开 Semantic DOM owner。 | 稳定发布出现对应公开组件 API。 |
| `FlexPanel` | Ant Design 稳定版没有与该布局 Panel 对应的公开 Semantic DOM owner。 | 新稳定版出现职责直接对应的公开 owner。 |
| `Grid / Row / Col` | Ant Design Grid 没有公开 Semantic DOM Props；`Layout.Sider` 不能映射到通用 Grid。 | 新稳定版公开 Grid/Row/Col 对应 API。 |
| ~~`ButtonSpinner`~~ | 排除判定已于 2026-09-16 经用户指令撤销，移入 §2.3 纳入映射，随第六批执行（理由见本节末）。 | 已撤销。 |
| ~~`ComboBox`~~ | 排除判定已于 2026-09-16 经用户指令撤销，移入 §2.3 纳入映射，随第三批执行（理由见本节末）。 | 已撤销。 |
| `BorderBeam` | Ant Design 稳定版没有该公开组件或对应 Semantic DOM API。 | 稳定版出现职责直接对应的公开 owner。 |
| ~~`Splash`~~ | 排除判定已于 2026-09-16 经用户指令撤销，移入 §2.3 纳入映射，随第七批执行（理由见本节末）。 | 已撤销。 |
| `WindowTitleBar` | Ant Design Web 组件体系没有对应的公开 Semantic DOM owner。 | 稳定版出现职责直接对应的公开 owner。 |
| `Window` | Ant Design Web 组件体系没有对应的公开 Semantic DOM owner；Modal 不能替代 TopLevel Window。 | 稳定版出现职责直接对应的公开 owner。 |

排除不是根据 AtomUI 模板复杂度做出的判断，也不能因为某控件内部有可定制节点而改变。只有新的稳定发布公开 API 通过
2.1 的完整 Gate 后，才能重新进入任务规划。

范围变更（2026-09-15）：`Expander` 的原排除判定经用户指令撤销，从本表移入 §2.3 纳入映射，随第二批执行。原判定以
“Ant Design 只有 `Collapse` owner；`Collapse.Panel` 也没有独立 API，不能为独立 Expander 建立映射”为由拒绝映射，但该
理由检验的是上游 owner 数量，而 §2.1 第 4 条要求的是“AtomUI 控件与该公开 owner 的产品职责直接对应”。AtomUI `Expander`
是单面板折叠容器，与上游 `Collapse` 的单个面板承担同一产品职责，上游 `Collapse` 已公开 `root` / `header` / `icon` /
`title` / `body` 五个语义键，因此映射成立。该撤销不改变其他排除项的判定依据。

范围变更（2026-09-15）：`TabStrip` 与 `Menu` 的原排除判定经用户指令撤销，从本表移入 §2.3 纳入映射。二者此前已随
`TabControl`（第二批）与 `NavMenu`（第五批）的提交一并实施，本次为**追认**并补齐范围记录，使文档与实现一致：

- `TabStrip`：原判定以“Ant Design 只在 `Tabs` owner 上公开 API，没有独立 `TabStrip` owner”为由拒绝映射。该理由同
  `Expander` 一样检验的是上游 owner 数量，而 §2.1 第 4 条要求的是产品职责直接对应。`TabStrip` / `CardTabStrip` 是
  独立 public owner 的页签条、`TabStripItem` 是二者的 item container，与上游 `Tabs` 映射成立，随第二批执行。
- `Menu`：原判定以“AtomUI `Menu` 是桌面命令、ContextMenu 与 MenuFlyout 家族；Ant Design `Menu` 是页面/模块导航，
  直接对应 AtomUI `NavMenu`”为由拒绝映射。实际 AtomUI `Menu` 与上游 `Menu` 共用同一套菜单语义键，
  `docs/controls/desktop/navigation/menu/semantic-part.md` 记录的 12 个公开键路径（`root`、`itemTitle`、`list`、`item`、
  `itemIcon`、`itemContent`、`subMenu.*`、`popup.root`，按 6.6.3 稳定发布源码审计）与 `NavMenu` 逐字相同，职责直接对应，
  随第五批执行。

范围变更（2026-09-16）：`ComboBox` 的原排除判定经用户指令**彻底撤销**（用户原话：把「ComboBox 被排除在 Semantic Part
范围之外」彻底解除），从本表移入 §2.3 纳入映射，随第三批执行。原判定以“Ant Design 没有公开 `ComboBox` 组件；`Select` 的
internal combobox mode 不能作为公开 owner”为由拒绝映射。该理由的前半段（上游无公开 owner）在 6.6.0 基线上**依然属实**，
因此本项**不是一次 §2.1 Gate 通过**，不能按 Gate 通过记录：

- 本次准入依据不是上游 API，而是 AtomUI `ComboBox` 自身就是职责完整、可独立定制的 public owner——它不复用 `Select`
  的 internal combobox mode，而是直接派生 Avalonia `ComboBox`，并自有输入框、下拉 handle、模板内 Popup 与候选容器
  创建路径。
- 原判定后半段（`Select` 的 internal combobox mode 不能作为公开 owner）不做修改，继续有效；§2.1 第 1 条对 `ComboBox`
  不成立，其准入以用户指令为依据。此项因此是**本表唯一的「非 Gate 通过」例外**：其余各次撤销（`SplitButton`、
  `Expander`、`TabStrip`、`Menu`）都能在 §2.1 的职责对应条款下自洽论证，`ComboBox` 不能——引用本项时不得把它
  当作 Gate 通过的先例。（`GroupBox` 有同类诉求，但其记录在并行工作中，本分支未登记，故不并入本表。）
- 命名与区域分组参考上游 `Select` 已公开的 `classNames` / `styles` 分组，只发布 `ComboBox` 自身确实拥有的区域，
  不为对齐上游而发明 `ComboBox` 不存在的键（尤其不发布 `ComboBox` 从未实现的清除部件）。
- 该撤销不改变其他排除项的判定依据，也不放开被排除控件之间的相互隔离要求（如 `Pagination`、`DataGrid` 内部嵌套的
  `ComboBox` 仍不得因其自身纳入而被借用为宿主语义区域；见 `2026-08-12-semantic-part-batch-2-collections-containers.md`
  任务 13 与 `2026-08-12-semantic-part-batch-5-high-density.md` 的 owner 隔离说明）。

上述四次撤销中，`SplitButton`、`Expander`、`TabStrip` 与 `Menu` 的共同判据是：**上游 owner 数量不是准入必要条件，
§2.1 第 4 条的产品职责直接对应才是**。`ComboBox` 不属于这一判据——它连第 1 条也不成立，是唯一的「非 Gate 通过」例外，
已在上方单独记录，引用时不得与其余四次混同。引用本表作
排除依据前，先确认该项未被后续日期化撤销覆盖。

范围变更（2026-09-16）：`GroupBox` 的原排除判定经用户指令撤销，从本表移入 §2.3 纳入映射，随第二批执行。此项与上述
三次撤销**判据不同，必须区分**：

- 三次追认/撤销的共同前提是“AtomUI 控件与上游公开 owner 的产品职责直接对应”，检验的只是 owner 数量还是职责对应。
  `GroupBox` 没有这个前提：Ant Design 6.6.3 稳定发布源码 `components/` 下不存在 fieldset、group 或 group-box 类组件，
  `GroupBox` 标识在组件源码中零命中，**不存在可映射的上游 owner**。
- 原排除判定登记的正向触发条件是“新稳定版出现职责直接对应的公开 owner”。该条件**未发生**，因此本次纳入不是一次新的
  §2.1 上游 Gate 通过，而是用户直接指令下对排除判定的撤销，纳入依据是 AtomUI 需要让 `GroupBox` 支持 Semantic Part。
- 既有先例是 `SplitButton` 的触发侧按键：上游没有对应键时，AtomUI 承认自身模板结构并把 Part 作为**显式能力补充**发布，
  而不是因为“上游没有”就拒绝任何定制。`GroupBox` 沿用同一处理：Part 从 AtomUI 自身源码与模板职责设计，`root` /
  `header` / `icon` / `title` / `content` 五个名称中 `header` / `title` 与上游 `Card` 的分区键职责对应，`icon` /
  `content` 按 GroupBox 的 Header API 与 `ContentControl.Content` 命名。
- 最近的产品职责参照是上游 `Card`（`root` / `header` / `title` / `body` / `extra` / `cover` / `actions`），它是“带标题的
  边框容器”这一职责的分区式参照。`GroupBox` 只借鉴键名语义，**不借用** `Card` 的 owner 准入资格，也不把 `Card` 专属的
  `extra` / `cover` / `actions` 虚构到自身模板上（见全量改造设计 §5.1 对虚构 Part 的禁止）。
- 契约细节见 [GroupBox Semantic Part 契约](../../controls/desktop/data-display/group-box/semantic-part.md) §1.1。

该撤销不改变其他排除项的判定依据；引用 §2.4 作排除依据时，仍需先确认该项未被日期化撤销覆盖。
范围变更（2026-09-16）：`ButtonSpinner` 的原排除判定经用户指令撤销，从本表移入 §2.3 纳入映射，随第六批执行。

原判定为“Ant Design 没有职责直接对应的公开 Semantic DOM owner；`InputNumber` 的 handle 是其内部区域”，重新评估触
发条件写作“新稳定版出现独立 spinner owner”。按当前稳定版 `antd@6.6.4` 源码复核，该判定的**事实前提已经过时**：

- `InputNumberSemanticType` 明确公开并实际消费五个分区键 `root` / `prefix` / `suffix` / `input` / **`actions`**
  （`packages/antd/es/input-number/index.d.ts`，实现把 `mergedClassNames` / `mergedStyles` 透传给
  `@rc-component/input-number`）。
- 下游 `@rc-component/input-number` 的 `InputNumber.js` 在 `mode === 'input'` 分支把上、下步进按钮包进同一个
  `-actions` 容器（`className: clsx(\`${prefixCls}-actions\`, classNames?.actions)`），并按 `prefix` / `suffix` /
  `input` 分别应用其余键。
- 即步进按钮区并非“纯内部区域”，而是上游**公开且实际消费**的语义键；ButtonSpinner 的步进手柄与其职责直接对应。

需要如实记录的两点边界：

1. 严格按 §2.1 措辞，触发条件写的是“出现独立 spinner owner”，而上游至今**没有**独立 spinner 组件 owner——`actions`
   是 `InputNumber` 上的分区键。本次准入因此不是由触发条件自动满足，而是：上游公开消费的 `actions` 键证明了原判定
   的事实前提（handle 属内部区域）不成立，**叠加用户 2026-09-16 的直接指令**，按 §2.1 第 4 条职责直接对应成立。
2. AtomUI `ButtonSpinner` 不拥有文本编辑面，且其帧内容左/右槽与 `NumericUpDown` 的 `prefix` / `suffix` 共享同一帧节点。
   因此本次映射只借用上游 `root` / `prefix` / `suffix` / `input` / `actions` 的**职责**，不复制其键名：帧内主内容区发布为
   `content`，帧内容左/右槽发布为 `innerLeftContent` / `innerRightContent`，以避免与 `NumericUpDown` 已发布的
   `Single` route 冲突（详见 [ButtonSpinner Semantic Part 契约](../../../controls/desktop/navigation/button-spinner/semantic-part.md)）。
   上、下按钮上游没有独立语义键，属显式能力补充。

本合同不影响其他排除项的判定依据。`ButtonSpinner` 与 `GroupBox` 同日纳入后，排除控件由 14 个收缩为 12 个，准入家族由 63 个增至 65 个。

范围变更（2026-09-16）：`Splash` 的原排除判定经用户指令撤销，从本表移入 §2.3 纳入映射，随第七批执行。该项与
`GroupBox` 同属“**无可映射上游 owner**、依据用户直接指令纳入”的一类，与上述三次“owner 数量 vs 职责对应”的追认
判据不同，不得混同：

- Splash 是桌面应用的启动反馈控件，职责是“启动中但应用尚不可交互”。上游稳定发布源码中不存在承载该职责的公开组件
  owner，因此原排除判定登记的正向触发条件（“新稳定版出现对应公开 owner”）**未发生**，本次纳入不是一次新的 §2.1 上游
  Gate 通过。
- 纳入依据是用户 2026-09-16 的直接指令与 AtomUI 自身的启动页职责。既有先例同样是 `SplitButton` 的触发侧按键：
  上游没有对应键时，AtomUI 按自身模板结构把 Part 作为**显式能力补充**发布。
- 十个 Part 全部从 AtomUI 自身模板与 API 职责设计：`root` 隐式；`logo` / `message` / `detail` 按 Splash 自身 API
  （`Logo` / `Message` / `Detail`）命名；`title` / `subtitle` / `content` / `footer` 与已改造的 `Result` / `Empty` /
  `Alert` / `GroupBox` 同职责键一致；`spin` / `progressBar` 按真实承载控件命名。Splash 不借用任何其他控件的 owner 准入
  资格，也不为对称而虚构模板中不存在的区域。
- 进度区刻意拆分为 `spin` 与 `progressBar` 两个 `Single` Part，而不是合并为一个 `Multiple` Part；理由与契约细节见
  [Splash Semantic Part 契约](../../controls/desktop/feedback/splash/semantic-part.md) §1.1、§2.6。
- `SplashWindow` 不发布 Semantic Part（窗口壳层已是 public API，表面阴影与宿主圆角属 Token 语义），见同一契约 §6.1。
- Splash 是 `AtomUI.Desktop.Controls.Extras` 中首个采用 Semantic Part 的控件。

该撤销不改变其他排除项的判定依据；引用 §2.4 作排除依据时，仍需先确认该项未被日期化撤销覆盖。

> 累计结论（2026-09-16，`GroupBox`、`ButtonSpinner`、`ComboBox`、`Splash` 四次纳入合并后）：排除控件由 16 个收缩为 **10 个**，
> 准入家族由 63 个增至 **67 个**，三行为 1 / 67 / 10，合计仍为 78。上文本节的逐条日期化段落记录的是各次纳入*当时*的中间
> 状态（14 → 12 → 11 与 63 → 65 → 66），不得作为当前计数引用；当前计数以本段与 §2 的表格为准。

## 3. 最小交付单位

任务以正式控件文档叶子代表的“控件家族”而不是单个 `.cs` 文件为单位。一个家族包含共同维护同一用户能力的 public
owner、public child control、internal presenter、item container、runtime-created visual、Popup/Overlay host、所有叶子主题、
测试和 Gallery。两个文档叶子即使共享源码目录，也必须分别完成 Gate A；共享基础文件只有在相关设计均获批后才能修改。

典型家族边界：

- `LineEdit` 包含 `LineEdit`、`TextBox`、`TextArea` 及其内部 clear/reveal/resize 节点；`SearchEdit` 使用同一源码目录，
  但作为独立文档叶子另行审核其搜索按钮和 decorated box 契约。
- `Modal / Dialog` 包含 `Dialog`、`DialogSurface`、Overlay host、Window host、header、resizer 和 button box。
- `DataGrid` 包含 grid、row、cell、header、presenter、filter flyout 和虚拟化/回收路径。
- 组合控件即使复用已支持的 Button，也不能继承准入资格；公开 owner 必须独立通过 2.1 的 Gate。
- `DropdownButton` 虽复用 Button 与 MenuFlyout 基础设施，仍以自己的 public owner 直接拥有下拉命令弹层，并已独立通过
  2.1 的 Gate；其语义契约映射上游 `Dropdown`，不借用 deprecated `Dropdown.Button` 的组合 owner 资格。
- `SplitButton` 同理：虽复用 Button 与共享 MenuFlyout 基础设施，仍以自己的 public owner 直接拥有下拉命令弹层
  （`Flyout`，Gallery 全部示例均为 `MenuFlyout`），2026-09-15 经用户指令撤销原排除判定并独立通过 2.1 的 Gate；
  其弹层侧映射上游 `Dropdown` 的 Semantic DOM，不借用 deprecated `Dropdown.Button` 的组合 owner 资格。触发侧
  补充发布 `primary` / `secondary` 是上游没有的显式能力补充：上游 DropdownButton 消费者自持两个触发 Button，
  而 AtomUI 的两个触发 Button 是模板内部件，不发布则完全不可定制——DropdownButton 因继承 Button 天然继承
  触发侧 `icon` / `content` 部件，SplitButton 是 ContentControl，无此继承路径。

同一家族的 descriptor 与模板 marker 必须一起审核和实现，不能让父控件与其容器、Popup 或派生模板在不同 commit 中短暂
形成不完整公共契约。

## 4. 三阶段审核状态机

每个控件家族严格按以下状态推进：

```text
已规划
  -> 文档设计中
  -> 文档审核中
  -> 文档已批准
  -> 实现中
  -> 实现审核中
  -> 实现已批准
  -> 已授权提交
  -> 已提交
```

### 4.1 Gate A：控件设计文档

进入 Gate A 前必须读取：

- public/protected API、默认值、继承关系和 `[TemplatePart]`。
- 所有叶子 `*Theme.axaml`，包括 typed `BasedOn`、派生模板和 Browser 变体。
- public child control、internal presenter、runtime-created visual 和 adorner。
- Popup、Flyout、Overlay、独立 Window/TopLevel 与 resource bridge。
- ItemsControl 的 container 创建、prepare、clear、recycle 和虚拟化路径。
- 既有测试、Gallery API/Token/ShowCase 和当前控件文档。

现有控件文档中的 `LLMS 语义区域` 或 `Semantic Parts` 表只能作为待核对的旧文档内容，不能证明运行时 descriptor 已存在。
只有 public owner 的 `[SemanticPart]`、生成 descriptor、真实模板 marker 和对应测试共同成立时，才表示控件已经支持 Semantic
Part。旧分类模型推导出的 `item/header/content/motion` 等通用区域必须重新从源码事实审计，不能直接转写为公共契约。

先更新：

先更新对应批次任务中列出的该控件 `overview.md`、`semantic-part.md` 与 `implementation.md` 精确路径。

`semantic-part.md` 是完整公共契约的唯一来源；`overview.md` 只保留 public owner、Part 名称、职责摘要和入口链接，
`implementation.md` 只保留 descriptor、marker、模板或运行时节点映射及生命周期约束。正式控件文档只描述最终设计，
不写任务状态、候选方案、排期或 checklist。

`semantic-part.md` 的 Semantic Parts 表必须逐项定义：

| 字段 | 要求 |
| --- | --- |
| `Part` / `Path` | camelCase 语义名；描述职责，不泄露节点层级。 |
| `Selector` | `root` 使用 owner；其余使用唯一 `.semantic-*`。 |
| `ContractType` | Setter 可稳定依赖的最低 public 类型。 |
| `Cardinality` | `Single`、`Optional` 或 `Multiple`，与所有模板/状态一致。 |
| `Customization` | `Root`、`Selector` 或经论证的 `SelectorAndTheme`。 |
| `CrossVisualRoot` | 是否需要穿过 owner 可达的 Popup/Overlay/TopLevel。 |
| `RuntimeCreated` | 是否由 C# 或容器生命周期创建。 |
| `ThemePropertyName` | 仅真实 public 子 Control 支持完整替换时存在。 |
| `AtomUI Node` | 对用户解释 Part 对应的抽象视觉职责；真实节点映射由 `implementation.md` 维护。 |
| 兼容边界 | 哪些内部节点明确不属于公共 Part。 |

`semantic-part.md` 必须逐 Part 说明存在条件、适合定制的属性、状态与 cardinality 关系、Selector 用法和排除边界。
`implementation.md` 必须定义 marker 所有权、模板映射、运行时创建点、生命周期、Popup 路径、尺寸协调、性能/AOT 和验证
不变量。文档完成后立即停止，等待用户审核；未获批准不得修改源码、主题、测试或 Gallery。

Gate A 还必须完成尺寸/状态基线审计。审计至少包含尺寸档默认含义、尺寸属性 owner、Token 映射、状态替代节点、模板路径、
Measure/Arrange 约束，以及外部组件 `default` 到 AtomUI 完整尺寸分支的事实映射。必须准备一个最小失败回归，证明缺少
完整尺寸基线或混用不同尺寸档位会产生布局问题；未完成该审计不得进入 Gate B。

### 4.2 Gate B：实现与验收

文档获批后，按测试先行完成：

1. 为 descriptor、cardinality、ContractType 和 selector 命中增加失败测试。
2. 在 public owner 上增加 `[SemanticPart]`；不为 `root` 显式声明 marker。
3. 在所有静态叶子模板上使用 `Classes.semantic-*="True"`，禁止动态 Binding marker。
4. 运行时节点只使用生成的 selector class 常量，不写重复字符串，不扫描 VisualTree 维护 Part。
5. 覆盖 Desktop、Browser、派生模板、状态替代节点、Popup 和 container 生命周期。
6. 为布局型 Setter 验证 owner 与 Part 的 Measure/Arrange、Min/Max、Padding、shape 和 SizeType 协调。
7. 先运行尺寸基线失败回归确认红灯，再以单一根因修复恢复通过；不得用示例固定 Height、MinHeight、Padding 或像素偏移
   替代完整尺寸映射。
8. 为 Gallery 增加真正延迟创建的 Semantic Parts Tab 内容，未打开 Tab 时不创建 demo、descriptor item 或 Preview。
9. 按 8.1 校验 Semantic 示例文案、全部本地化资源和当前 AtomUI 版本 Tag，并增加阻止 React/DOM 术语和陈旧版本号回归的
   测试。
10. 更新该控件 `changelog.md`，运行 LLMS verify，但不手改生成文档。

实现完成后必须保持未提交，向用户报告文件、测试和已知风险，等待实际运行与视觉验收。

### 4.3 Gate C：用户授权提交

只有用户明确确认该控件实现没有问题并要求创建 commit 后，才能：

- 只 stage 当前控件家族的文档、源码、主题、测试和 Gallery 文件。
- 检查工作树中用户或其他任务的改动，不混入无关文件。
- 创建一个 scoped commit。

“完成代码”“测试通过”“文档审核通过”均不等于提交授权。批次完成也不自动创建聚合 commit。

## 5. Part 设计方法

### 5.1 从职责而不是节点名称出发

候选 Part 必须同时满足：

1. 用户能够理解并有实际定制价值。
2. 对应职责能在所有受支持主题、状态和平台上保持。
3. AtomUI 愿意把名称、ContractType 和 cardinality 作为版本兼容契约。
4. 不要求用户知道 `PART_*`、internal 类型或偶然的 Grid/Border 层级。

如果控件只有 root 职责，Gate A 必须用源码和模板事实证明，并把该控件重新归类为“不适用”。当前生成器只为至少声明
一个非 root Part 的 owner 隐式加入 `root`，不生成独立的 root-only descriptor。不得为了覆盖率虚构 `content`、`item`、
`wrapper` 或 `container`，也不得只为获得 root descriptor 创建无价值 Part。

### 5.2 Owner 边界

父控件只承诺自己拥有的区域。嵌套 public 子 Control 具有独立行为、主题或替换价值时，应由子 Control 声明自己的
descriptor；父控件不能用多层 `/template/` 穿透其内部实现。

同一用户控件家族可以包含多个 descriptor owner。控件文档必须明确：

- 哪个 public type 拥有哪个 Part。
- Part 是父 owner 的 region，还是 public child control 的 root/region。
- runtime-created container 如何获得生成 marker。
- container recycle 后 marker、状态和订阅是否保持正确。

### 5.3 Popup、Overlay 与独立宿主

Popup 不因跨视觉根而自动需要额外 Theme 属性。默认仍使用 selector 模型；`CrossVisualRoot=true` 只记录 owner 可达的
实际视觉根和 Gallery/工具解析方式。

设计必须区分：

- owner 模板内可达的 `Popup`：从 owner 的模板作用域定位 Popup，再从公开 `Popup.Child` 进入目标根。
- Flyout presenter：确认 presenter 是 owner 的实际 public/内部视觉职责，不能仅按全局 TopLevel 搜索。
- Overlay layer：确认 overlay visual 的 owner、attach/detach 和 marker 创建位置。
- 服务创建的 Message、Notification、Dialog、Splash Window：由具体 host/session 明确 ownership；Gallery 通过显式
  `AdditionalRoots` 演示，不给生产控件增加 Preview 专用接口。

只有真实 public 子 Control 需要完整 `ControlTheme` 替换时，才使用 `SelectorAndTheme` 和 `ThemePropertyName`。

## 6. 布局与 SizeType 协调

Semantic Style 使用 Avalonia 原生优先级。Part Setter 已命中不代表父级布局边界会自动让出空间；布局型 Part 必须把
owner 根、尺寸档和中间节点作为一个完整测量系统审计。

每个涉及 `Padding`、`Margin`、`Width`、`Height`、Min/Max 或字体/图标尺寸的 Part，文档必须覆盖：

- owner 是否实现 `ISizeTypeAware` 或 `ICustomizableSizeTypeAware`。
- 预设尺寸是否以 `MinHeight` 建立基线，还是存在有意不可扩展的固定 `Height`。
- `SizeType=Custom` 的自然测量和本地属性优先级。
- 中间 wrapper 的固定尺寸、clip、padding 和自定义 Measure/Arrange。
- Circle、Round、icon-only、loading、空内容和多行内容。
- Desktop、Browser 与派生主题的尺寸一致性。

默认方向是使用 Min/Max 建立设计基线，让合法 Semantic Setter 参与自然测量。若控件的公共几何必须固定，文档必须明确
其不可扩展理由和允许定制的属性边界，不能让用户误以为布局型 Setter 能改变最终外形。

每个控件在进入实现前必须提交尺寸/状态基线矩阵：

| 项目 | 内容 |
| --- | --- |
| 完整尺寸分支 | `Large`、`Middle`、`Small`、`Custom` 的存在性、默认含义和 Token 映射。 |
| 布局 owner | owner、Part、中间节点的 Height/MinHeight/MaxHeight、Width/MinWidth/MaxWidth、Padding、Margin、字体和图标尺寸。 |
| 状态矩阵 | loading、disabled、shape、variant、icon-only、空态、替代节点和派生主题。 |
| 外部映射 | Ant Design `default`/`small` 等名称对应 AtomUI 哪个完整分支，以及依据。 |
| 失败回归 | 缺少尺寸映射或混用基线时的最小可复现失败。 |

实现只允许采用“完整尺寸基线 + Semantic 增量覆盖”的组合。控件若不存在尺寸档，必须明确记录不适用；不能以默认值
不明为理由直接复制外部示例的局部 Padding、字体或高度。

## 7. 性能与 NativeAOT 边界

生产控件的 Semantic Part 增量成本只允许来自 Avalonia 已有 class 匹配和静态 descriptor 注册：

- 不在控件实例构造时读取 registry。
- 不为 Semantic Part 增加 VisualTree 扫描、反射、运行时 AXAML 解析或程序集扫描。
- 不增加常驻 layout、scroll、pointer、timer 或 TopLevel 监听器。
- marker 必须静态存在，不能在状态变化时反复增删。
- runtime-created visual 在既有创建路径一次性添加生成常量，不建立额外索引。
- 默认 AtomUI ControlTheme 不使用 `.semantic-*` 驱动内置样式，避免把公共 marker 变成主题内部热路径依赖。

高密度或虚拟化控件还必须提供以下证据：

- 单 container 的 marker 数量和新增 class 匹配范围。
- prepare/clear/recycle 不产生重复 class、重复订阅或 retained owner。
- 大数据、滚动和展开/收起前后的实例数量与布局性能无明显回退。
- Gallery Preview 只处理当前可见实例，并遵守 32 个 Adorner 预算。

所有实现禁止依赖反射发现 Part，确保 trimming 和 NativeAOT 下 descriptor、常量和注册路径确定。

## 8. Gallery 契约

每个适用控件最终都应提供一个 Semantic Parts Tab，遵循
[Semantic Part Preview](../../gallery/authoring/semantic-part-preview.md)：

- Examples 仍是默认 Tab。
- Semantic 内容由 `IDataTemplate` 或显式 factory 延迟创建。
- 未首次进入 Semantic Parts Tab 前，不创建 demo control、registry query、Part item、snippet 或 Adorner。
- Part 说明来自该控件 descriptor 和本地化职责描述，不复制 selector/cardinality 元数据。
- Popup、Overlay 或独立 host 示例只显式提供该 demo 自己创建的 additional roots。
- Info 打开后才创建技术元数据和代码片段。

Gallery 接入是控件交付的一部分，因为它同时验证用户可理解性、真实 marker、跨视觉根和延迟创建；Gallery 不参与生产
控件的运行时依赖闭包。

### 8.1 Semantic 示例文案与版本 Tag 门禁

Ant Design 示例只作为控件状态、数值、视觉排列和样式效果的上游基线，不能把 Web/React 文档外壳原样复制进 AtomUI
Gallery。每个控件在视觉验收前必须逐项满足：

1. Semantic 示例标题使用仓库统一文案 `Custom Semantic Part styling`；说明使用 AtomUI 的 owner-scoped Style、公开
   Semantic Part 和该控件真实 selector 能力表述。
2. 文案中不得出现只属于 Ant Design Web API 的 `semantic dom`、`classNames`、`styles`、`objects/functions`、React Props
   或 DOM 节点措辞；上游原文只能作为理解示例意图的证据。
3. `en-US.xlf` 的 `<source>` 与 `zh-CN`、`zh-TW`、`pt-BR` 的 source/target 必须同步，不能只修改当前界面语言。
4. Semantic 示例的 `BadgeText` 必须读取 `GalleryVersionInfo.DisplayVersion`，其值来自 `build/Version.props` 的
   `AtomUIVersion` 并带 `v` 前缀；禁止硬编码上游 Ant Design 的 introduced version 或复制旧 AtomUI 版本。当前版本为
   `v6.1.3`。
5. Gallery 定向测试必须断言版本来源、标准标题和 AtomUI 说明文案，并显式拒绝 React/DOM 专属术语。版本、文案、示例
   主体和最终可见 Semantic Style 效果是四个独立验收项，任一项不符合都不能报告实现完成。
6. 最终截图必须同时核对 Tag 显示值、标题、说明、本地化语言和示例主体；不能因为进度条、颜色或布局正确而跳过卡片
   文案与 Tag。

## 9. 验证矩阵

每个控件至少执行：

```bash
dotnet test tests/AtomUI.Generator.Tests/AtomUI.Generator.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~SemanticPart
dotnet test tests/AtomUI.Toolkits.GalleryBase.Tests/AtomUI.Toolkits.GalleryBase.Tests.csproj --framework net10.0 --no-restore
dotnet run --project tools/AtomUI.Docs.LLMsGenerator/AtomUI.Docs.LLMsGenerator.csproj -- verify --config docs/AI/generated/llms.config.json
git diff --check
```

控件与 Gallery 的定向测试命令由对应批次任务给出的真实测试目录、项目和 namespace 确定；执行前先用 `rg` 定位现有
测试类，不能运行一个示意 filter 并把零测试误报为通过。风险附加验证：

| 风险 | 附加验证 |
| --- | --- |
| Browser theme | Desktop 与 Browser 每个叶子模板 marker/cardinality 一致。 |
| Layout/SizeType | Large/Middle/Small/Custom、Min/Max、shape 和布局型 Setter。 |
| ItemContainer | 多 item、空集合、replace/reset、虚拟化滚动、prepare/clear/recycle。 |
| Popup/Flyout | closed/open/reopen、placement、Popup.Child、关闭后无订阅/引用。 |
| Overlay/Window | host attach/detach、session close、additional roots 和多窗口隔离。 |
| High density | 大数据滚动、marker/class 数量、容器回收和性能基线。 |
| Optional package | 对应项目测试、主题注册、Gallery 引用和 NativeAOT publish。 |
| Gallery copy/version | 标准 Semantic Part 标题与平台文案、全部本地化资源、`GalleryVersionInfo.DisplayVersion` 和当前 `AtomUIVersion`。 |

涉及新的 AXAML 控件、运行时注册、Popup/Window 或 optional package 时，实施阶段还必须执行 Gallery NativeAOT publish。

## 10. 批次策略

| 批次 | 数量 | 目标 | 主要风险 |
| --- | ---: | --- | --- |
| Batch 1 | 16 | 基础视觉与状态控件，建立可复用审核节奏。 | 派生主题、尺寸、adorner、状态替代节点。 |
| Batch 2 | 16 + 3 | 集合、容器与导航结构（`Expander` 与 `TabStrip` 均为 2026-09-15 用户指令新增，`GroupBox` 为 2026-09-16 用户指令新增，随本批次计划执行）。 | container、runtime-created、虚拟化、多重 cardinality。 |
| Batch 3 | 15 | 输入、选择与日期/时间类控件。 | SizeType、Popup、内部 editor、候选项容器。 |
| Batch 4 | 10 + 1 | Popup、Overlay 与服务宿主（原 10 个家族已于 2026-09-12 收尾；`SplitButton` 为 2026-09-15 用户指令新增，随本批次计划执行）。 | 跨视觉根、session 生命周期、多宿主隔离。 |
| Batch 5 | 2 + 1 | 高密度复合控件（`NavMenu` 2026-09-14 提交 `1e22ed1a3`；`DataGrid` 2026-09-14 提交 `454cc0d00`，可选包独立验证工程；`Menu` 为 2026-09-15 用户指令新增并追认，随本批次执行）。 | 大量 container、Popup、虚拟化和性能。 |

批次表达审核顺序，不构成批量提交边界。始终一次只推进一个控件家族，并在 Gate A 与 Gate B 后等待用户确认。

> 2026-09-15 状态同步：五个批次的 63 个家族**全部已实现并经用户授权提交**，无剩余待改造家族。遗留项集中在验收与证据侧，
> 不涉及控件是否纳入范围：第三批的 ColorPicker / Select / DatePicker 待视觉验收，Mentions / TimePicker / TreeSelect
> 尚无验收文档；第五批三套控件的改造前后性能基线未归档。逐批次明细见
> [全量改造总计划](2026-08-12-semantic-part-control-rollout.md) 的「批次进度」。
>
> 2026-09-16 状态更新：`GroupBox`、`ButtonSpinner`、`ComboBox` 与 `Splash` 先后经用户指令纳入（均为原排除判定撤销，分别随
> 第二批、第六批、第三批、第七批），家族总数由 63 增至 67。上段“63 个家族全部已实现”的历史结论不变，但“无剩余待改造家族”
> 不再成立。其中：`ButtonSpinner` 与 `Splash` 已完成 Gate A / Gate B 并经用户授权提交，`GroupBox` 已完成
> 真机视觉验收，`ComboBox` 已完成 Gate A（经用户批准）与 Gate B 实现。范围记录见 §2.3 与 §2.4 的
> 日期化范围变更段。
>
> 2026-09-15 基线版本说明：§2 登记的上游审计基线为 2026-08-12 时的 Ant Design 6.6.0。此后个别家族的契约文档已在
> 6.6.3 稳定发布源码上重新对齐（如 `DataGrid`、`Menu` 的语义表明确写「对齐 Ant Design 6.6.3」），但 §2.2 的组件目录清单
> 与 §2.3 映射表仍按 6.6.0 登记。这不改变任何家族的准入结论，但引用上游键路径时需以各控件 `semantic-part.md` /
> `overview.md` 中写明的版本为准。若后续要统一升级基线，应按 §2.1 重新审计公开 Props、包导出与实现消费点。

## 11. 兼容性纪律

Semantic Part 是新的公共主题契约。以下变化默认属于破坏性变更：

- 删除或重命名 Part/path/selector class。
- 收窄或更换不兼容的 `ContractType`。
- 把 `Single` 变成 `Optional`/`Multiple`，或反向改变可观察数量语义。
- 默认模板、Browser 模板、派生模板或 Popup 路径丢失 marker。
- 把原本 owner 可达的 Part 移入不可达独立 host，或改变 `CrossVisualRoot`。
- 取消 typed theme property 或改变 public child control 的替换边界。

因此每个 Part 在文档审核时必须按长期 API 的标准评估。没有足够事实支持的候选区域应留在 Composition Model，而不是先
公开再修正。
