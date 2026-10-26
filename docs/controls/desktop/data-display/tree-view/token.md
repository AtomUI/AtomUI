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
- `DirectoryNodeSelectedColor`
- `DirectoryNodeSelectedBg`

`NodeHoverBg` 用于节点 hover 背景。`NodeSelectedBg` 用于普通 selected / pressed 背景。`DirectoryNodeSelectedColor` 和 `DirectoryNodeSelectedBg` 是兼容保留项：当前没有目录树专用交互入口，默认 TreeView 主题不消费这两个 Token。保留其名称、类型、资源标识、默认值与配置往返；不能将其作为普通节点选中色的调节入口。

节点 disabled 文本色、图标弱化和 switcher 图标色优先使用 SharedToken，不在 TreeViewToken 中重复定义。

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
- switcher：`HeaderHeight` 控制 switcher hit size，`TreeNodeSwitcherMargin` 控制 toggle 模式下 switcher 间距。
- icon：`TreeNodeIconMargin` 控制节点 icon 与 header 内容间距，icon 尺寸来自 SharedToken。
- drag/drop：`DragIndicatorLineWidth` 控制 drop indicator 线宽，颜色来自 SharedToken 主色。
- filter：`FilterHighlightColor` 通过 `FilterHighlightForeground` 传递到 header highlighter。

Token 不参与 default selected / checked / expanded path 回放，不参与 `CheckedItems` 同步，不参与异步加载逻辑。

默认求值使用 `DependencyDefaultsV2`。12 个字段的默认函数均只读取当前作用域的有效 `Global`，没有 Own 字段之间的依赖；Control 专属 Global 配置也进入这些函数。具体派生关系如下，间距单位均为 DIP：

| Token | 有效 Global 输入与计算 |
| --- | --- |
| `HeaderHeight` | `ControlHeightSM` |
| `NodeHoverBg` / `NodeSelectedBg` | `ControlItemBgHover` / `ControlItemBgActive` |
| `DirectoryNodeSelectedColor` / `DirectoryNodeSelectedBg` | `ColorTextLightSolid` / `ColorPrimary`，兼容保留 |
| `TreeItemMargin` | `(0, 0, 0, UniformlyPaddingXXS / 2)` |
| `TreeItemHeaderPadding` | `(UniformlyPaddingXS, 0, UniformlyPaddingXS, 0)` |
| `TreeItemHeaderMargin` | `(UniformlyMarginXXS, 0, 0, 0)` |
| `TreeNodeSwitcherMargin` | `(0, 0, UniformlyPaddingXS / 2, 0)` |
| `TreeNodeIconMargin` | `(UniformlyPaddingXS / 2, 0, 0, 0)` |
| `FilterHighlightColor` | `ColorError` |
| `DragIndicatorLineWidth` | `LineWidthFocus` |

明确设置的 Own 值优先于默认函数，包含 `0`、透明色和零间距；移除 Own 配置后重新使用当前有效 Global 的默认结果。该规则不为字段增加统一数值范围，实际布局和状态仍按各消费者契约处理。

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
- 默认函数声明的 `Revision = 1` 参与生成器求值版本摘要，不等于运行时 descriptor 的 `EvaluatorRevision` 数值；资源枚举数值与 dense slot 必须分别保持并验证。
- Token 变更必须同步 Theme 引用和 token.md 语义说明。

## 6. 验证策略

TreeViewToken 变更验证：

- `TreeViewTokenKind` 与 token.md 语义说明保持一致。
- 对比默认函数迁移前后的 Light / Dark 实际资源值、类型、资源枚举数值和 dense slot，并验证 Global、Control 专属 Global、明确 Own、撤回及同实例重挂。
- `HeaderHeight` 影响 header 最小高度和 switcher 尺寸。
- `NodeHoverBg` / `NodeSelectedBg` 在三种 hover mode 下均命中正确背景层。
- `TreeItemMargin`、`TreeItemHeaderPadding`、`TreeItemHeaderMargin`、`TreeNodeSwitcherMargin`、`TreeNodeIconMargin` 不造成 header 内容重叠。
- `DragIndicatorLineWidth` 与实际 drop indicator render line width 一致。
- `FilterHighlightColor` 能传递到 filter highlight runs。
- Light / dark 主题下 hover、selected、disabled、filter 和 drag indicator 可读性稳定。
