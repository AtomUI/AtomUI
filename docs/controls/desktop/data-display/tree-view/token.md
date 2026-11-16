# TreeView Token 设计

本文档定义 `AtomUI.Desktop.Controls.TreeViewToken` 的 TreeView 专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。TreeView 整体架构见 [TreeView 桌面版架构设计](overview.md)，内部实现原理见 [TreeView 桌面版实现原理](implementation.md)，设计和契约变化记录见 [TreeView Changelog](changelog.md)。

## 1. 定位

TreeViewToken 是 TreeView 的组件级设计变量层。它把全局颜色、尺寸、间距、线宽和状态色转换为 TreeView 节点 header、switcher、icon、拖拽指示器和过滤高亮可消费的语义值。

TreeViewToken 服务以下主题和控件：

- `TreeViewTheme.axaml`
- `TreeViewItemTheme.axaml`
- `TreeViewItemHeaderTheme.axaml`
- `NodeSwitcherButtonTheme.axaml`
- `TreeView` drag indicator render state
- `TreeViewItem` line render state
- `TreeViewItemHeader` hover / selected / filter state

TreeViewToken 不承载 `SelectedItem`、`SelectedItems`、`CheckedItems`、`IsExpanded`、`IsChecked`、`IsFilterMode`、`FilterResultCount`、`IsDragging`、`DragIndicatorRenderInfo` 等实例状态。这些状态由 C# 状态模型、容器属性和主题 selector 处理。

## 2. Token 分类

TreeViewToken 当前按 TreeView 语义分为五类。

### 2.1 节点尺寸 Token

- `HeaderHeight`

`HeaderHeight` 控制节点 header 最小高度，同时作为 `NodeSwitcherButton` 的宽高默认值。它来自 `SharedToken.ControlHeightSM`，保持树节点紧凑可扫描。

### 2.2 节点状态色 Token

- `NodeHoverBg`
- `NodeSelectedBg`
- `NodeSelectedColor`
- `DirectoryNodeSelectedColor`
- `DirectoryNodeSelectedBg`

`NodeHoverBg` 用于节点 hover 背景。`NodeSelectedBg` 用于普通 selected / pressed 背景。`NodeSelectedColor` 是选中节点
（未禁用）的文字色，画在 `NodeSelectedBg` 上，默认取全局 `ControlItemColorActive`（默认 `ColorText`，antd Tree
`nodeSelectedColor`），所以 Ant Design 渲染不变；选中底色为实色的主题（PrimeNG）在全局设置对比色。TreeSelect 与 TransferTreeView 共用同一 Token。`DirectoryNodeSelectedColor` 和 `DirectoryNodeSelectedBg` 是保留 Token（`[ReservedToken]`，对应 antd Tree `directoryNodeSelectedColor` / `directoryNodeSelectedBg`）：AtomUI TreeView 没有目录树模式，设置后暂无视觉效果。

未选中节点的文字与图标色使用宿主的 `ColorText`，disabled 节点的文字、节点图标与 switcher 图标统一使用 `ColorTextDisabled`（与 antd 一致，此前图标与 switcher 以 0.3 不透明度弱化），不在 TreeViewToken 中重复定义。

### 2.3 节点结构间距 Token

- `TreeItemMargin`
- `TreeItemHeaderPadding`
- `TreeItemHeaderMargin`
- `TreeNodeSwitcherMargin`
- `TreeNodeIconMargin`

这些 Token 只表达 TreeView 节点结构间距：

- `TreeItemMargin` 控制节点行之间的垂直节奏。
- `TreeItemHeaderPadding` 控制 header 内容内边距。
- `TreeItemHeaderMargin` 控制 header 内容框与左侧结构区间距。
- `TreeNodeSwitcherMargin` 控制 switcher 与 checkbox / radio 之间的距离。
- `TreeNodeIconMargin` 控制节点 icon 与内容之间的距离。

缩进层级由 `TreeViewItemTheme.MarginMultiplierConverter` 控制，不属于 Token。

### 2.4 拖拽 Token

- `DragIndicatorLineWidth`

`DragIndicatorLineWidth` 控制 drop indicator 线宽。拖拽指示线颜色当前来自 `SharedToken.ColorPrimary`，由 TreeView Theme 绑定到 internal drag indicator brush。

拖拽位置、目标节点和 indicator 坐标属于运行时状态，不进入 Token。

### 2.5 过滤 Token

- `FilterHighlightColor`

`FilterHighlightColor` 控制 filter match 文字高亮前景，默认来自 `SharedToken.ColorError`。过滤是否命中、命中词和生成的 inline runs 属于实例状态，不进入 Token。

## 3. 控件专项模型中的 Token 使用

TreeView Token 主要参与以下专项模型：

- hover mode：`NodeHoverBg` 和 `NodeSelectedBg` 同时服务 `Default`、`Block`、`WholeLine` 三种 hover 背景范围。
- 键盘活动项：`:keyboard-active` 复用 `NodeHoverBg`，背景范围随 `NodeHoverMode`，与 `NodeSelectedBg` 叠加时 selected 优先；不新增专属 Token。设计见 [TreeView 键盘导航设计](keyboard-navigation-design.md)。
- switcher：`HeaderHeight` 控制 switcher hit size，`TreeNodeSwitcherMargin` 控制 toggle 模式下 switcher 间距。
- icon：`TreeNodeIconMargin` 控制节点 icon 与 header 内容间距，icon 尺寸来自 SharedToken。
- drag/drop：`DragIndicatorLineWidth` 控制 drop indicator 线宽，颜色来自 SharedToken 主色。
- filter：`FilterHighlightColor` 通过 `FilterHighlightForeground` 传递到 header highlighter。

Token 不参与 default selected / checked / expanded path 回放，不参与 `CheckedItems` 同步，不参与异步加载逻辑。

### 3.1 主题读法与 Token owner

- `TreeViewItem` 是 Token owner 透明的条目容器（`[TokenOwnerTransparent]`），无 identity 的 `TreeViewItemHeader` 与 `NodeSwitcherButton`
  继承宿主树的 owner，以 `OwnerTokenResource` 读取全局 Token：`Controls[TreeView]` 作用于节点高度、圆角、图标尺寸与颜色、拖拽指示线、
  树线与过渡时长，`Controls[TreeViewItem]` 不作用于节点。TransferTreeView 与 TreeSelect 的树复用这些主题，跟随各自的 owner。
- `TreeViewTheme` / `TreeViewItemTheme` 也是 TransferTreeView / TransferTreeViewItem 的基础主题，全局 Token 以 `OwnerTokenResource` 读取
  （主题前有 `<!-- owner: -->` 标记）。TreeView Own Token 仍以 `TreeViewTokenResource` 读取。
- 节点图标与 switcher 图标位于注册控件 `IconPresenter` 中，owner 读取在 header / switcher 上完成，再经 internal `IconSize` / `IconBrush`
  绑定到 IconPresenter（在 IconPresenter 上读取会解析到它自己的 identity）。

## 4. 控件家族影响

TreeViewToken 影响 TreeView 与 TreeViewItem 体系：

- `TreeView` root theme 绑定 filter highlight、empty padding、motion、drag indicator 线宽和颜色。
- `TreeViewItemHeader` 使用节点高度、行间距、header padding、hover / selected 背景和 filter highlight。
- `NodeSwitcherButton` 使用 `HeaderHeight` 作为 switcher 区域尺寸，并使用 `NodeHoverBg` 作为 pointerover 背景。
- `FloatableTreeView` 继承 TreeView 的 Token 语义，不定义独立 Token。

TreeViewToken 不影响 NavMenu、Masonry、GroupBox 或其他 Data Display 控件。

## 5. 兼容性要求

维护 TreeViewToken 时必须保持以下要求：

- 不擅自重命名、删除或改变 `TreeViewTokenKind` 对应 Token 语义。
- 不把节点运行时状态写入 Token。
- 不把过滤结果、拖拽目标、当前选择、当前勾选或展开状态写入 Token。
- 不把 `NodeHoverMode`、`ToggleType`、`FilterStrategy` 等实例配置拆成 Token。
- 默认值应继续从 SharedToken 派生，保持 light / dark 主题一致性。
- Token 变更必须同步 Theme 引用和 token.md 语义说明。

## 6. 验证策略

TreeViewToken 变更验证：

- `TreeViewTokenKind` 与 token.md 语义说明保持一致。
- `HeaderHeight` 影响 header 最小高度和 switcher 尺寸。
- `NodeHoverBg` / `NodeSelectedBg` 在三种 hover mode 下均命中正确背景层。
- `TreeItemMargin`、`TreeItemHeaderPadding`、`TreeItemHeaderMargin`、`TreeNodeSwitcherMargin`、`TreeNodeIconMargin` 不造成 header 内容重叠。
- `DragIndicatorLineWidth` 与实际 drop indicator render line width 一致。
- `FilterHighlightColor` 能传递到 filter highlight runs。
- Light / dark 主题下 hover、selected、disabled、filter 和 drag indicator 可读性稳定。

## 独立 Token 身份与共享计算

TreeView 与 TransferTreeView 独立拥有实际树 Own；TreeSelect 只声明其实际树展示字段并通过纯值函数复用默认计算。原目录树模式字段仅在 TreeView 保留 Reserved，不扩大新终端承诺。
