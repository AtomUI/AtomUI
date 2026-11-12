# TreeView 键盘导航设计

本文档定义 TreeView 桌面版的键盘焦点、键盘活动项、按键语义、选择/勾选提交策略和宿主转发契约。公共设计主线见
[TreeView 桌面版架构设计](overview.md)，源码职责与维护入口见 [TreeView 桌面版实现原理](implementation.md)，
Token 语义见 [TreeView Token 设计](token.md)，变化记录见 [TreeView Changelog](changelog.md)。

## 1. 设计定位

键盘导航让用户在不使用指针的情况下浏览层级、展开收起节点并提交选择或勾选。它覆盖：

- 独立使用的 `TreeView`，以及自身获得焦点的派生树（`FloatableTreeView`、`TransferTreeView`）。
- 以 `TreeView` 为内容、键盘焦点留在宿主输入框中的浮层场景，即 TreeSelect 下拉树。

键盘导航只作用于已实现且可导航的 `TreeViewItem` 容器。它不负责数据源查询、远程加载分页、虚拟化、按字符搜索定位（type-ahead）
或业务命令绑定。

## 2. 设计原则

- **单一焦点目标**：`TreeView` 是树内唯一的键盘焦点目标，也是唯一 Tab 停靠点。`TreeViewItem`、`TreeViewItemHeader`、
  switcher 和 checkbox / radio 指示器不参与焦点。键盘位置由 TreeView 自己维护，不通过把焦点移到节点上来表达。
- **活动项与业务状态分离**：键盘活动项只表示“键盘操作的目标”。它不等于 selected、checked、expanded，也不等于 hover。
  只有提交策略明确规定时，移动活动项才会同时改变选择。
- **键盘与指针共用同一条状态路径**：键盘展开、异步加载、选择和勾选都走指针交互已有的入口。动效、`IsAutoExpandParent`、
  加载合并、勾选级联和事件派发对两种输入保持一致。
- **只遍历可见、可用的结构**：导航顺序就是视觉上的先序顺序。被过滤隐藏、被禁用或位于收起祖先之下的节点不可导航。
- **不新增公共 API**：键盘导航只是 TreeView 默认行为的组成部分，提交策略由现有 `IsSelectable`、`SelectionMode`、
  `ToggleType` 推导，不引入新的模式属性。
- **方向随 FlowDirection**：“进入子级 / 返回父级”是逻辑方向，`FlowDirection=RightToLeft` 时左右键互换。

## 3. 专项模型与 Public API

### 3.1 状态模型

| 状态 | Owner | 含义 | 与键盘的关系 |
| --- | --- | --- | --- |
| keyboard focus | `TreeView` | 树是否接收按键。 | 只有 TreeView 自身获得焦点或宿主转发时，按键才进入导航。 |
| active item | `TreeView`（内部） | 当前键盘操作目标节点容器。 | 方向键移动它；Enter / Space 对它提交。 |
| active indicator | `TreeView` 计算，`TreeViewItem` / `TreeViewItemHeader` 呈现 | 是否显示活动项视觉。 | 只在键盘交互会话中显示，指针交互时隐藏。 |
| selected | Avalonia 选择模型（`SelectedItem` / `SelectedItems`） | 业务选择结果。 | 由提交策略决定是否随活动项变化。 |
| checked | `TreeViewItem.IsChecked` + `CheckedItems` | 勾选结果。 | 只在勾选入口有效时由 Enter / Space 切换。 |
| expanded | `TreeViewItem.IsExpanded` | 子级是否可见。 | 由左右键切换，或触发异步加载。 |

active item 是内部状态，不作为公共 API 暴露。应用通过 `SelectedItem`、`SelectedItems`、`CheckedItems`、
`ItemExpanded` / `ItemCollapsed` 观察键盘操作的结果。

### 3.2 可导航节点

当 `TreeViewItem` 同时满足以下条件时，它是可导航节点：

- 容器已实现，并属于当前 TreeView。
- `IsVisible=true`。被过滤策略隐藏的节点不可导航。
- `IsEffectivelyEnabled=true`。被禁用节点及其子树不可导航。
- 所有祖先 `TreeViewItem` 的 `IsExpanded=true`。只看目标状态，不看收起动画是否已播放完。

### 3.3 按键语义

下表按 `FlowDirection=LeftToRight` 描述；`RightToLeft` 时“→”与“←”互换。

| 按键 | 行为 | 未处理条件 |
| --- | --- | --- |
| `↓` | 活动项移到先序顺序的下一个可导航节点。 | 已是最后一个可导航节点。 |
| `↑` | 活动项移到先序顺序的上一个可导航节点。 | 已是第一个可导航节点。 |
| `Home` / `End` | 活动项移到第一个 / 最后一个可导航节点。 | 没有可导航节点。 |
| `→` | 活动项可展开且已收起时展开它；未加载的异步节点触发加载请求。活动项已展开时，移到它的第一个可导航子节点。 | 叶子节点，或加载中节点。 |
| `←` | 活动项已展开时收起它；否则移到父节点。 | 顶层且已收起的节点，或顶层叶子节点。 |
| `Enter` / `Space` | 对活动项执行提交，见 §4。 | 活动项既不可勾选也不可选择，或其 header 处于禁用状态。 |
| `Ctrl` + `↑` / `↓` / `Home` / `End` | 只移动活动项，不改变选择。 | 同对应的无修饰键。 |
| `Shift` + `↑` / `↓` / `Home` / `End` | `SelectionMode` 包含 `Multiple` 时，从选择锚点到新活动项做范围选择。 | 非多选模式时与无修饰键相同。 |
| 平台 SelectAll 手势 | `SelectionMode` 包含 `Multiple` 且 `IsSelectable=true` 时全选。 | 非多选或不可选择。 |

未处理的按键不设置 `Handled`，继续冒泡给外层控件。带 `Alt` 修饰的按键不参与导航，留给宿主（例如 TreeSelect 的 `Alt+↓` 开关下拉）。

没有有效活动项时（例如宿主转发的第一次按键），导航键只确定初始活动项（§7.2），不移动、不改变选择。

“可展开”指节点有子项（`ItemCount > 0`），或节点有数据加载器且尚未加载（`HasTreeItemDataLoader && !AsyncLoaded`）。

## 4. 提交策略

移动活动项是否同时改变选择，以及 Enter / Space 提交什么，由现有属性按下表推导：

| 条件（按顺序匹配） | 方向键移动 | Enter / Space |
| --- | --- | --- |
| `IsSelectable=false`，且勾选入口无效 | 只移动活动项。 | 不处理。 |
| 活动项勾选入口有效（`ToggleType` 为 `CheckBox` / `Radio`，且该节点的 indicator 可见、可用） | 只移动活动项。 | 切换勾选：checkbox 在 true 与 false 之间切换，并按 `IsCheckStrictly` 级联；radio 置为选中。 |
| `ToggleType` 不为 `None`，但活动项勾选入口无效（例如 radio 模式下的非叶子节点），且 `IsSelectable=true` | 只移动活动项。 | 选择活动项。 |
| `SelectionMode` 包含 `Multiple`，且 `IsSelectable=true` | 只移动活动项；`Shift` 组合按 §3.3 做范围选择。 | 切换活动项的选中状态（相当于 Ctrl 点击）。 |
| 单选，`ToggleType=None`，且 `IsSelectable=true` | 选择跟随活动项。 | 选择活动项（已选中时不变）。 |

规则：

- “选择跟随”只在单选、无勾选入口时生效。在多选或勾选模式下，浏览本身不能改动业务结果。
- 宿主可以关闭“移动即选择”（单选跟随与 `Shift` 范围选择），只保留 Enter / Space 提交。TreeSelect 下拉树使用该模式，浏览选项不会提前写入 TreeSelect 的值。
- 活动项的 header 处于禁用状态时（例如宿主的选择数量上限禁用了未选中节点的 header），Enter / Space 不提交，与指针点击保持一致。
- 键盘提交使用与指针相同的选择、勾选入口，因此 `SelectionChanged`、`CheckedItemsChanged`、Form 值变化按原有语义派发。
- 键盘提交不派发 `ItemClicked`，也不触发 `ItemContextMenuRequest`。这两个事件只表达指针交互。
- `IsSelectable=false` 时，任何键盘路径都不写入 `IsSelected`。

## 5. 架构、文件结构与职责

| 源码边界 | 职责 | 不负责 |
| --- | --- | --- |
| `TreeView/TreeView.KeyboardNavigation.cs` | active item 状态、可导航节点遍历、按键分派、提交策略、指示可见性、Tab 导航接入、宿主转发入口、移动即选择开关。 | 修改节点结构、发起数据查询、绘制视觉。 |
| `TreeView/TreeViewItem.cs` | 统一的展开请求入口：切换展开，或对未加载节点发起加载请求；呈现 active indicator 伪类。 | 判断导航顺序、决定提交策略。 |
| `TreeView/NodeSwitcherButton.cs` | 指针点击 switcher 时调用 `TreeViewItem` 的展开请求入口。 | 自己判断异步加载或展开逻辑。 |
| `TreeView/TreeViewItemHeader.cs` | 呈现 active indicator 伪类。 | 持有 active 状态。 |
| `TreeView/Themes/*.axaml` | TreeView 可聚焦且不绘制整体焦点框；节点、header、switcher、indicator 不可聚焦；active indicator 视觉。 | 按键逻辑。 |
| `TreeSelect/TreeSelect.KeyboardNavigation.cs` | 宿主侧：下拉开关键、Escape、按键转发规则、提交后关闭单选下拉。 | 树内导航和提交策略。 |

TreeView 是 active item 的唯一 owner。节点容器只呈现 TreeView 推送的指示状态，不自行计算导航。

## 6. Template、组合与集成契约

### 6.1 焦点契约

| 节点 | Focusable | 说明 |
| --- | --- | --- |
| `TreeView` | `True` | 唯一焦点目标和 Tab 停靠点。`FocusAdorner` 为空，键盘位置由 active indicator 表达。 |
| `TreeViewItem` | `False` | 由 `TreeViewItemTheme` 设置。 |
| `TreeViewItemHeader#Header` | `False` | 固定值，不再继承 `TreeViewItem.Focusable`。 |
| `NodeSwitcherButton#PART_NodeSwitcherButton` | `False` | 点击 switcher 不转移焦点。 |
| `CheckBox#ToggleCheckbox` / `RadioButton#ToggleRadio` | `False` | 点击 indicator 不转移焦点，键盘仍由 TreeView 接收。 |

指针按下节点时，焦点落到最近的可聚焦祖先，也就是 TreeView 自身。header 模板中用户放入的可聚焦内容（例如输入框）保留自己的焦点和按键语义。
按键源不是 TreeView 自身时，TreeView 不对它做导航。

### 6.2 Tab 导航

TreeView 自己实现 Tab 导航的自定义钩子：Tab / Shift+Tab 进入时，焦点落在 TreeView 自身；从 TreeView 出发的 Tab / Shift+Tab
交给默认的 Tab 顺序，离开树。TreeView 不会把 Tab 焦点导向不可聚焦的节点容器。

### 6.3 伪类契约

| 伪类 | 所属 | 含义 |
| --- | --- | --- |
| `:keyboard-active` | `TreeViewItem`、`TreeViewItemHeader` | 该节点是 active item，且 active indicator 可见。 |

两个节点上的伪类同步设置。默认主题在 `TreeViewItemHeader` 上呈现：`Default` / `Block` 模式使用内容框背景，`WholeLine`
模式使用整行 header 背景，与 hover 背景范围的分层一致。`TreeViewItem` 上的伪类供应用样式使用。默认主题使用 `NodeHoverBg`
作为 active indicator 背景；它与 selected 背景叠加时，selected 视觉优先。Token 语义见 [TreeView Token 设计](token.md)。

### 6.3.1 键盘焦点描边

对应 Ant Design 树列表 `:focus-visible` 时给活动节点的 `node-content-wrapper` 加 `genFocusOutline`：

- **显示条件**：active indicator 可见，并且 TreeView 自身持有焦点。TreeView 计算后写入内部状态
  `TreeViewItem.IsKeyboardFocusOutlineVisible`，经模板投射到 `TreeViewItemHeader`。
- **宿主转发不描边**：TreeSelect 等宿主的焦点留在输入框，下拉树只显示 active indicator 背景，不显示描边。
- **描边位置**：header 用共享的 `ManagedFocusOutline` 在 `PART_HeaderContentFrame` 外侧绘制主题的 `DefaultFocusAdorner`，
  三种 `NodeHoverMode` 都描边内容框，圆角跟随内容框。TreeView 自身的 `FocusAdorner` 仍为空，不画整棵树的焦点框。
- **生命周期**：header 离开视觉树时移除描边，重新附加或重新应用模板时按当前状态恢复。

### 6.4 宿主转发契约

焦点留在宿主输入框的场景（TreeSelect 下拉）由宿主把按键转发给 TreeView 的内部入口 `HandleKeyDown(KeyEventArgs)`。
该入口与 TreeView 自身获得焦点时使用同一套按键语义和提交策略，但不做来源过滤。

- 宿主内的 TreeView 设置为不可聚焦，点击节点不会把焦点从宿主输入框移走。
- 宿主可以关闭移动即选择（§4），只让 Enter / Space 提交。
- 宿主决定转发哪些按键，以及输入法组合期间是否转发。TreeView 只定义按键语义。
- 宿主转发的按键处理后，active indicator 可见；宿主关闭浮层时调用 `ClearKeyboardActiveItem` 清除 active item。
- 宿主在转发前后的业务逻辑（例如提交后关闭下拉、开关下拉的按键）由宿主负责，TreeView 不感知宿主模式。

## 7. 核心算法、数据流与生命周期

### 7.1 遍历

导航顺序是从顶层容器开始、按 `IndexFromContainer` 顺序的先序遍历，只进入满足 §3.2 的节点。

- **下一个**：活动项已展开且有可导航子节点时，取第一个可导航子节点；否则取下一个可导航兄弟；没有时向上回溯，取最近祖先的下一个可导航兄弟。
- **上一个**：取前一个可导航兄弟，再沿它的已展开分支下降到最后一个可导航后代；没有前一个兄弟时取父节点。
- **第一个 / 最后一个**：在顶层上分别应用“下一个”和“上一个”的起点规则。
- 不可导航的节点连同其子树一起跳过，不会进入它的后代。

遍历只访问已实现容器，不访问 `ITreeItemNode` 数据树。已展开但子容器尚未生成时（例如刚展开、还没走过布局），对该节点执行一次布局
以生成子容器。生成后仍然没有可导航子节点，则按“没有子节点”处理，不等待、不轮询。

### 7.2 按键数据流

```text
KeyDown（TreeView 自身获得焦点，或宿主调用 HandleKeyDown）
      ↓
来源过滤：Source 不是 TreeView 且不是宿主转发 → 不处理
      ↓
校验 active item：无效时按 §7.3 修复；仍为空时取初始活动项（本次导航键到此结束）
      ↓
映射按键（按 FlowDirection 修正左右）
      ↓
移动：遍历得到目标 → 设置 active item → 按 §4 决定是否选择 → BringIntoView
展开 / 收起：TreeViewItem 展开请求入口（含异步加载）
提交：勾选入口 / 选择入口
      ↓
active indicator 可见；设置 Handled
```

初始活动项依次取：`SelectedItem` 对应的可导航容器（多选时取 `SelectedItems` 中先序最靠前的可导航容器）；第一个已勾选的可导航容器；
第一个可导航节点。

### 7.3 active item 生命周期

| 事件 | 处理 |
| --- | --- |
| TreeView 通过 Tab 或宿主转发进入键盘会话 | active item 为空或无效时取初始活动项；不改变选择。 |
| 指针按下节点 header | active item 设为该节点；active indicator 隐藏。 |
| 任意按键导航 | active indicator 显示。 |
| active item 的某个祖先被收起 | active item 立即移到这个被收起的节点。 |
| active item 被过滤隐藏、被禁用 | 下一次按键时修复为最近的可导航祖先；没有祖先时为空。 |
| active item 容器离开视觉树（容器清理或回收、ItemsSource 替换、拖拽移动重建容器） | 立即清空引用，不保留旧容器。 |
| TreeView 失去键盘焦点 | active indicator 与焦点描边隐藏，active item 保留。 |
| TreeView detach，或宿主关闭浮层 | 清空 active item。 |

active item 以容器引用保存，并只在上述路径中更新。TreeView 不为它订阅额外事件。

### 7.4 展开与异步加载

`→` 与 switcher 点击走同一个 `TreeViewItem` 展开请求入口：

- 节点有数据加载器且未加载：发起加载请求，由 TreeView 的加载合并器处理；加载中再按 `→` 不重复请求。加载成功后按现有流程展开，active item 不变。
- 其他情况：切换 `IsExpanded`，动效、`IsAutoExpandParent` 和 `ItemExpanded` / `ItemCollapsed` 派发与指针一致。

## 8. 资源、性能与 AOT 边界

- 不引入 DynamicResource、Token、binding、定时器、缓存或异步任务。异步加载复用现有加载合并器。
- active item 是单一容器引用，指示状态是一个布尔值；导航不分配集合，遍历按需访问相邻容器。
- 每次按键最多触发一次局部布局，用来生成刚展开节点的子容器。
- 不使用反射或动态发现。

## 9. 兼容性与定制边界

- `SelectedItem`、`SelectedItems`、`CheckedItems`、`IsExpanded` 和所有事件的语义不变；键盘只是新增的输入来源。
- 指针交互的选择、右键选择、拖拽、上下文菜单行为不变。
- TreeView 默认可聚焦，因此会出现在 Tab 顺序中。应用可以设置 `Focusable=False` 或 `IsTabStop=False` 退出键盘导航。
- `Enter` 提交活动项，不再沿用 Avalonia 基类“Enter 切换展开”的语义；展开收起由左右键负责。
- `:keyboard-active` 是稳定伪类。自定义 header 或 item 模板时，应用负责呈现该伪类；没有呈现时键盘行为仍然有效，只是没有活动项视觉。
- header 模板中的用户可聚焦内容保留自己的按键语义，TreeView 不拦截。
- 自定义 `ITreeViewInteractionHandler` 只接管指针交互，不影响键盘导航。

## 10. 验证要求

| 层次 | 验证内容 |
| --- | --- |
| 遍历（headless） | 先序顺序；跳过被过滤隐藏、被禁用、位于收起祖先之下的节点及其子树；首尾边界未处理；刚展开节点子容器的生成。 |
| 按键语义 | ↑↓ Home End；→ 展开 / 进入首个子节点 / 叶子未处理；← 收起 / 返回父节点 / 顶层未处理；RTL 左右互换；Ctrl 只移动活动项；未处理按键继续冒泡。 |
| 提交策略 | §4 每一行；`IsSelectable=false` 时任何按键都不抛异常、不写入选择；checkbox 级联与 strict；radio 只作用于叶子；多选 Shift 范围与 Enter 切换；SelectAll；关闭移动即选择后方向键不写入选择；header 禁用时不提交。 |
| 事件 | 键盘选择 / 勾选派发 `SelectionChanged`、`CheckedItemsChanged` 和 Form 值变化；不派发 `ItemClicked`。 |
| 异步加载 | → 触发加载请求；加载中不重复；成功后展开。 |
| 生命周期 | 收起祖先时活动项上移；容器清理、ItemsSource 替换、detach 后不保留旧容器。 |
| 焦点与 Tab | 树是唯一 Tab 停靠点；点击节点、switcher、indicator 后焦点在 TreeView；header 内的用户输入框按键不被拦截。 |
| 主题 | `:keyboard-active` 在三种 `NodeHoverMode` 下命中正确背景层；指针交互时不显示；Light / Dark。 |
| 宿主转发 | TreeSelect 下拉中焦点留在宿主时，转发按键按同一语义生效；方向键不改变 TreeSelect 的值，Enter 提交并关闭单选下拉；勾选模式 Space 切换勾选且下拉保持打开；Escape 关闭；关闭浮层后清除活动项。 |
| Gallery | TreeView ShowCase 提供键盘操作示例，覆盖单选、勾选、异步加载三类树。 |
