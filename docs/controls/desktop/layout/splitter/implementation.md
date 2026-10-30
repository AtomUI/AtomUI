# Splitter 桌面版实现原理

本文档描述 Splitter 桌面版的内部实现范围、源码职责、状态流、生命周期、资源边界和维护规则。公共设计与 API 契约见 [Splitter 桌面版架构设计](overview.md)，Semantic Part 契约见 [Splitter Semantic Part 契约](semantic-part.md)，变化记录见 [Splitter Changelog](changelog.md)。涉及控件 Token 的实现应同时阅读 [Splitter Token 设计](token.md)。

## 1. 实现定位

Splitter 的实现由一个 public 根控件和三个 internal 协作层组成：

```text
Splitter
  -> SplitterTheme / PART_SplitterPanel
  -> SplitterPanel
  -> SplitterHandle
  -> SplitterDragBar
```

`Splitter` 暴露用户 API、Children、附加属性和 resize 事件；`SplitterPanel` 负责布局算法、面板集合、handle 生成和折叠状态；`SplitterHandle` 负责单条分割边界的状态、折叠按钮和 drag bar 协调；`SplitterDragBar` 负责 pointer drag 入口和 grip 视觉。

实现文档不复述完整属性清单。完整 public surface 以源码和 API 契约摘要为准。

## 2. 源码文件结构

主要源码文件：

- `src/AtomUI.Desktop.Controls/Splitter/Splitter.cs`
- `src/AtomUI.Desktop.Controls/Splitter/Splitter.SemanticParts.cs`
- `src/AtomUI.Desktop.Controls/Splitter/SplitterPanel.cs`
- `src/AtomUI.Desktop.Controls/Splitter/SplitterHandle.cs`
- `src/AtomUI.Desktop.Controls/Splitter/SplitterDragBar.cs`
- `src/AtomUI.Desktop.Controls/Splitter/SplitterPanelCollapsible.cs`
- `src/AtomUI.Desktop.Controls/Splitter/SplitterResizeEventArgs.cs`
- `src/AtomUI.Desktop.Controls/Splitter/SplitterDraggerDoubleClickedEventArgs.cs`
- `src/AtomUI.Desktop.Controls/Splitter/SplitterToken.cs`
- `src/AtomUI.Desktop.Controls/Splitter/Themes/SplitterTheme.axaml`
- `src/AtomUI.Desktop.Controls/Splitter/Themes/SplitterHandleTheme.axaml`
- `src/AtomUI.Desktop.Controls/Splitter/Themes/SplitterDragBarTheme.axaml`

Gallery 与文档结构：

- `controlgallery/AtomUIGallery/ShowCases/Layout/Splitter`
- `tests/AtomUI.Desktop.Controls.Tests/Splitter/SplitterSemanticPartTests.cs`
- `tests/AtomUIGallery.Tests/ShowCases/SplitterShowCasePageTests.cs`
- `tests/AtomUIGallery.Tests/ShowCases/SplitterShowCaseExamples.snapshot`
- `docs/controls/desktop/layout/splitter/`

职责边界：

- `Splitter.cs` 保留 public API、附加属性、Children 同步、template part 获取、resize 和 dragger 双击事件抛出。
- `Splitter.SemanticParts.cs` 只承载 `[SemanticPart]` descriptor 声明，生成 `SplitterSemanticParts` 常量与
  `SplitterPanelStyle` / `SplitterDraggerStyle` Semantic Style。
- `SplitterPanel.cs` 保留布局、拖拽、折叠、尺寸约束、handle 状态刷新和 Semantic Part marker 维护。
- `SplitterHandle.cs` 保留单个 handle 的按钮、hover、dragging、collapse request 和 drag event 转发。
- `SplitterDragBar.cs` 保留 Thumb 输入事件、双击识别、第二次 drag start 抑制和禁用拖拽拦截。
- Theme 文件负责静态视觉结构、template binding、selector、TokenResource 映射和静态 semantic marker 声明。
- Token 文件只提供组件视觉变量，不保存实例状态。

## 3. 核心类职责

### 3.1 Splitter

`Splitter` 是唯一 public 控件入口：

- 持有 `Children` 集合。
- 注册根控件属性和面板附加属性。
- 在 `OnApplyTemplate` 中获取 `PART_SplitterPanel`。
- 把 `Children` 同步到 `SplitterPanel.Children`。
- 通过 `RaiseResizeStarted`、`RaiseResizeDelta`、`RaiseResizeCompleted` 把 internal drag 状态转换为 public resize 事件。
- 通过 `RaiseDraggerDoubleClicked` 把 internal dragger 双击转换为 public `DraggerDoubleClicked`，只暴露 handle index。
- 注册 `SplitterToken` scope。

维护规则：

- 不把 internal handle 或 panel 对象暴露给用户代码。
- 模板重套用时必须保证 Children 同步不会重复持有旧视觉对象。
- 新增 Splitter 实例级样式 API 时，入口必须先定义在 `Splitter` 上，再由 `SplitterPanel` 传递给 internal handle。

### 3.2 SplitterPanel

`SplitterPanel` 是布局和状态 owner：

- 从 `Children` 中识别用户面板。
- 在相邻面板之间创建 `SplitterHandle`。
- 监听面板附加属性变化并触发布局或 handle 状态刷新。
- 根据 `Orientation`、`HandleSize`、尺寸约束和折叠状态执行 measure/arrange。
- 处理 drag started/delta/completed，按 `IsLazy` 决定实时布局或延迟提交。
- 处理折叠和恢复，维护可见面板边界。
- 维护 Semantic Part marker：为新跟踪面板添加 `semantic-panel`，为新建 handle 添加 `semantic-scope-handle`，
  面板离开时回收 Splitter 自己添加的 marker。

维护规则：

- `_panels` 是用户面板列表，`_handles` 是 internal handle 列表，两者不能混淆。
- handle 的创建、事件订阅和移除必须成对维护。
- 面板属性变化必须从 `SplitterPanel` 统一归一，不允许 handle 直接修改用户面板业务状态。
- 尺寸计算必须同时考虑固定值、百分比、默认尺寸、最小值、最大值和折叠尺寸。
- `_semanticPanelMarkersAdded` 只记录「由 Splitter 添加」的 `semantic-panel` marker；面板离开时只回收该集合
  中的条目，用户预先声明的 `semantic-panel` 类必须保留。

### 3.3 SplitterHandle

`SplitterHandle` 是 internal 分割边界：

- 接收 `Orientation`、`IsDragEnabled`、line brush、line thickness 等视觉状态。
- 获取 `PART_DragBar` 和折叠按钮。
- 转发 `SplitterDragBar` 的 drag event。
- 转发 `SplitterDragBar` 的双击通知，并沿模板重套用生命周期成对订阅和解除。
- 根据可折叠状态、hover side 和 icon display mode 更新折叠按钮。
- 使用 `:pointerover` 和 `:dragging` 驱动主题 selector。

维护规则：

- `SplitterHandle` 不应成为用户样式 API。
- 折叠按钮点击只抛出 request，由 `SplitterPanel` 决定是否真正折叠或恢复。
- `LineBrush`、`LineThickness`、`LineCornerRadius` 这类分割线视觉状态必须可由 `Splitter` 根控件统一传入，不能只停留在 internal handle 默认值。

### 3.4 SplitterDragBar

`SplitterDragBar` 继承 AtomUI Thumb：

- 提供 pointer drag 入口。
- 通过 `IsDragEnabled` 禁用拖拽输入。
- 使用平台提供的 pointer `ClickCount` 识别第二次左键按下，在释放阶段发出双击通知。
- 第二次按下不进入 Thumb drag 流；捕获丢失会取消待释放的双击通知。
- 使用 `Orientation` 设置 resize cursor。
- 使用 line brush、line thickness 和 line corner radius 展示 grip。
- 使用 `:dragging` 表达拖拽状态。

维护规则：

- drag bar 负责输入和 grip 视觉，不负责面板尺寸计算。
- grip 的厚度应来自 `LineThickness`，不能绕过 handle 状态直接写死 token。
- drag bar 命中区域尺寸和可见 grip 尺寸必须分离。

## 4. 状态与数据流

Splitter 的主状态流：

```text
User children / attached panel properties
  -> Splitter.Children
  -> SplitterPanel.Children
  -> _panels + generated _handles
  -> Measure / Arrange / collapse state
```

拖拽状态流：

```text
Pointer drag on SplitterDragBar
  -> SplitterHandle.DragStarted / DragDelta / DragCompleted
  -> SplitterPanel.HandleDragStarted / HandleDragDelta / HandleDragCompleted
  -> Apply panel sizes
  -> Splitter.ResizeStarted / ResizeDelta / ResizeCompleted
```

双击状态流：

```text
Second left PointerPressed on SplitterDragBar
  -> suppress second Thumb DragStarted
  -> matching left PointerReleased
  -> SplitterHandle.DraggerDoubleClicked
  -> SplitterPanel forwards HandleIndex
  -> Splitter.DraggerDoubleClicked
  -> application-owned reset policy
```

折叠状态流：

```text
Collapse IconButton click
  -> SplitterHandle.CollapsePreviousRequested / CollapseNextRequested
  -> SplitterPanel ExpandLeftAtHandle / ExpandRightAtHandle
  -> attached IsCollapsed + panel size update
  -> handle visibility and button state refresh
```

样式状态流：

```text
Splitter public style API / SplitterToken
  -> SplitterPanel owner state
  -> SplitterHandle line state
  -> PART_HandleLine + SplitterDragBar
  -> PART_Grip
```

维护要求：

- 外部设置的 Avalonia 属性必须在模板应用前后保持一致。
- `HandleSize` 影响布局和 hit area；line thickness 只影响可见线条和 grip。
- `IsResizable=False` 只禁用对应边界拖拽，不应移除分割线视觉。
- `IsLazy=True` 时中间 drag feedback 不应提前写入面板 `Size`。
- overview.md 的 API 契约说明应与源码实际状态流一致。

## 5. 生命周期与模板接入

`Splitter` 生命周期：

- 构造阶段订阅 `Children.CollectionChanged` 并注册 Token scope。
- `ChildrenChanged` 把用户集合变更同步到当前 `SplitterPanel`。
- `OnApplyTemplate` 获取 `PART_SplitterPanel`，设置 owner，并同步现有 Children。

`SplitterPanel` 生命周期：

- 构造阶段订阅自身 `Children.CollectionChanged`。
- `RefreshPanelsAndHandles` 移除旧 handle、解除 drag、double-click 和 collapse 事件订阅、重建面板列表、创建新 handle、建立 handle 事件。
- `SyncTrackedPanels` 跟踪用户面板属性变化，为新面板添加 `semantic-panel` marker，并在面板离开时解绑、
  回收 Splitter 自己添加的 marker。
- `CreateHandle` 为每个新 handle 添加 `semantic-scope-handle` marker；handle 重建时 marker 随新实例。
- 布局变化和属性变化触发 measure/arrange 或 handle state update。

Semantic Part marker 的获取与释放成对存在：

- 面板 marker：`SyncTrackedPanels` 添加，面板离开 `_panels` 时在 `SyncTrackedPanels` 中移除；只有
  `_semanticPanelMarkersAdded` 记录的条目会被回收。
- handle marker：`CreateHandle` 添加，handle 随 `RefreshPanelsAndHandles` 销毁时随实例释放，无独立回收路径。
- 静态 marker（`semantic-scope-panel` / `semantic-dragger`）由主题声明，模板重套用时随模板节点重建。

`SplitterHandle` 生命周期：

- `OnApplyTemplate` 先解除旧 `SplitterDragBar` 的 drag/double-click 事件和按钮事件，再获取新 part 并重新订阅。
- `SetDragging` 同步 handle 和 drag bar 的 `:dragging` 状态。
- `UpdateCollapseButtons` 在 hover、orientation、collapse state 变化后刷新按钮。

稳定接入点：

| 接入点 | Owner | 维护规则 |
| --- | --- | --- |
| `PART_SplitterPanel` | `Splitter` | public template part，重命名需要同步主题、实现、测试和文档。 |
| `PART_HandleLine` | `SplitterHandle` | internal theme part，用于线条视觉，不作为用户 API。 |
| `PART_DragBar` | `SplitterHandle` | internal theme part，用于 pointer drag，不作为用户 API。 |
| `PART_Grip` | `SplitterDragBar` | internal theme part，用于 grip 视觉，不作为用户 API。 |
| collapse buttons | `SplitterHandle` | internal theme part，用于折叠操作。 |

## 6. 交互与事件处理

交互规则：

- Pointer drag 只从 `SplitterDragBar` 进入。
- `SplitterHandle` 只转发 drag event、dragger double-click 和 collapse request。
- `SplitterPanel` 是唯一可修改面板尺寸和折叠状态的内部 owner。
- `Splitter` 只负责对外抛 resize 事件。

dragger 双击事件语义：

- 双击时间和距离由 Avalonia 平台输入系统计算为 `ClickCount`，控件不维护固定毫秒计时器。
- 第一次按下和释放保留原有 drag started/completed 语义；第二次按下不再启动 drag。
- `DraggerDoubleClicked` 在第二次左键释放后触发，捕获丢失时取消。
- 双击通知直接沿所属 drag bar、handle、panel 和 Splitter 传递，不使用冒泡 routed event，避免嵌套 Splitter 通知外层。
- 双击通知独立于 `IsDragEnabled`；控件不自动恢复 `Size`、`DefaultSize` 或 `IsCollapsed`。

resize 事件语义：

- `ResizeStarted` 在某个 handle 开始拖拽时触发。
- `ResizeDelta` 在尺寸变化过程中触发，lazy 模式下应符合延迟提交语义。
- `ResizeCompleted` 在拖拽结束后触发。
- 事件参数暴露 handle index 和尺寸快照，不暴露 internal handle 对象。

禁用拖拽语义：

- `Splitter.IsResizable=False` 的面板会影响相邻 handle 的 `IsDragEnabled`。
- `IsDragEnabled=False` 时 drag bar 不处理 pointer pressed/moved/released。
- 禁用拖拽不等于隐藏分割线；视觉仍应表达面板边界。

## 7. 内部算法与关键流程

维护者需要重点关注：

- `RefreshPanelsAndHandles`：从用户 children 派生 internal handle。
- `UpdateHandleState`：根据相邻面板、折叠状态和尺寸约束决定 drag/collapse 能力。
- measure/arrange 流程：把 `Dimension`、`HandleSize` 和剩余空间归一为实际尺寸。
- drag 流程：实时或 lazy 地更新尺寸。
- collapse 流程：寻找可见边界、可恢复面板和尺寸 owner。

布局算法不应把视觉线条厚度混入 handle 占位尺寸。可见线条变粗时，只应该影响 handle 内部视觉，不应该让面板布局发生额外跳动，除非用户显式改变 `HandleSize`。

## 8. 资源、性能与 AOT 边界

资源边界：

- 不通过运行时反射扫描 public API、Token 或 Gallery 示例数据。
- 不把可静态声明的模板结构迁移到 C# 动态创建。
- handle、drag bar、button 的事件订阅必须在模板重套用或 handle 移除时解绑。
- drag bar 仅持有一个待释放双击状态；该状态在 release 和 pointer capture lost 时清除，不使用 timer 或全局输入监听。
- `_trackedPanels` 中的面板属性订阅必须在面板离开时解绑。

性能边界：

- 面板集合变化时只重建必要的 handle 结构，避免把业务子控件重新包装成不可追踪结构。
- 拖拽过程中避免创建临时视觉对象。
- lazy 模式下拖拽反馈应复用现有 drag bar 状态，不引入额外弹层或全局输入监听。
- 分割线样式变化应通过 Avalonia 属性和模板绑定流动，不使用手动遍历视觉树刷新。

AOT 边界：

- API 与 Token 契约不通过运行时反射生成。
- Source generator 生成文件不手工编辑。
- 文档、Gallery 和源码发生冲突时，应修复源文档或结构化数据。

## 9. 维护不变量

维护 Splitter 时不得破坏：

- Public API、默认值、事件顺序和 Gallery 可观察行为。
- `HandleSize` 作为 hit area 的语义。
- `SplitterPanel` 作为尺寸与折叠状态 owner 的语义。
- internal handle template part 的绑定关系和事件释放路径。
- `DraggerDoubleClicked` 的释放时机、零基 handle index、第二次 drag start 抑制、禁用拖拽可用性和嵌套隔离。
- Semantic Part 契约：`root` / `panel` / `dragger` 的名称、selector class、`SelectorRoute`、`ContractType` 与
  cardinality（完整定义见 [Splitter Semantic Part 契约](semantic-part.md)）。
- 主题静态 marker（`semantic-scope-panel` / `semantic-dragger`）与运行时 marker（`semantic-panel` /
  `semantic-scope-handle`）的放置位置与回收路径。
- Light/Dark、Browser/Desktop 和不同方向下的主题一致性。
- API 契约摘要、Token 语义、ShowCase 示例和控件文档的一致性。
- 根框架外观 API（`Background`、`BorderBrush`、`BorderThickness`、`CornerRadius`、`BorderDashArray`、
  `BorderDashOffset`）直接 TemplateBinding 到 `SplitterTheme` 的 `Frame`，不经过 `SplitterPanel` 或
  internal handle 转发。

新增分割线样式能力时必须遵守：

- API 定义在 `Splitter`，不定义在 internal handle 上作为用户入口。
- `LineThickness` 不替代 `HandleSize`。
- `LineCornerRadius` 同时作用于 `PART_HandleLine` 和 `PART_Grip`。
- 默认值来自 Splitter Token 或 SharedToken，保证现有视觉不变。
- API 契约摘要、Token 语义、ShowCase 示例和回归测试同步更新。

## 10. 测试与验证

推荐验证：

- 纯文档改动运行 `git diff --check` 并检查相对链接。
- 控件 API 或行为变更运行 `tests/AtomUI.Desktop.Controls.Tests`。
- 双击行为测试覆盖释放时机、handle index、重复 drag start、禁用拖拽、捕获丢失、handle 重建解绑和嵌套隔离。
- Semantic Part 契约变更运行 `tests/AtomUI.Desktop.Controls.Tests` 中的 `SplitterSemanticPartTests`，
  并走查 Gallery Semantic Parts 标签页的预览与描述。
- Gallery 示例或源码片段变更运行 `tests/AtomUIGallery.Tests` 中 Splitter 相关测试。
- 主题变更检查 `SplitterTheme.axaml`、`SplitterHandleTheme.axaml`、`SplitterDragBarTheme.axaml` 中 TemplateBinding、TokenResource、selector 和 semantic marker 是否一致。
- AOT、生成器或动态数据路径变更按 Gallery NativeAOT 发布流程验证。
