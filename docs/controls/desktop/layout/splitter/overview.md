# Splitter 桌面版架构设计

本文档定义 `Splitter` 桌面版的最新设计定位、公共契约、状态模型、视觉主题关系和兼容边界。通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，内部实现原理见 [Splitter 桌面版实现原理](implementation.md)，Splitter Token 的专项设计见 [Splitter Token 设计](token.md)，Semantic Part 契约见 [Splitter Semantic Part 契约](semantic-part.md)，设计和契约变化记录见 [Splitter Changelog](changelog.md)。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/Layout/Splitter` |
| 控件状态 | Stable |

Splitter 是 AtomUI 桌面控件体系中的分割面板控件，用于把一个区域拆成两个或多个可调整尺寸的面板。它负责面板尺寸计算、拖拽调整、延迟拖拽反馈、面板折叠状态和分割把手视觉。

Splitter 不负责窗口停靠系统、路由容器、业务布局状态持久化、面板内容装饰或复杂 IDE docking。子面板的内容、背景、内部圆角和业务状态应由用户提供的子控件负责。

主要源码入口：

- `src/AtomUI.Desktop.Controls/Splitter/Splitter.cs`
- `src/AtomUI.Desktop.Controls/Splitter/SplitterPanel.cs`
- `src/AtomUI.Desktop.Controls/Splitter/SplitterHandle.cs`
- `src/AtomUI.Desktop.Controls/Splitter/SplitterDragBar.cs`
- `src/AtomUI.Desktop.Controls/Splitter/Themes/`

## 2. 设计语言

Splitter 的设计语言是“低干扰的可调整边界”。分割线应明确表示可拖拽边界，但不能抢夺面板内容的视觉层级。折叠图标只在可折叠边界上表达动作，不应让普通分割线看起来像常驻工具栏。

| 维度 | 含义 | Splitter 中的表达 |
| --- | --- | --- |
| 产品语义 | 将一个区域拆成若干可调整子区域。 | `Children` 承载面板，`Orientation` 决定分割方向。 |
| 内容承载 | 面板内容由用户直接提供。 | 子控件通过 `Splitter.Size`、`Splitter.MinSize` 等附加属性参与布局。 |
| 状态反馈 | 拖拽、双击、可折叠、禁用拖拽和延迟反馈需要稳定的交互边界。 | resize 事件、`DraggerDoubleClicked`、handle hover/dragging 视觉和折叠按钮。 |
| 主题语义 | 外层容器与分割把手分开定制。 | `SplitterTheme` 承载根框架，`SplitterHandleTheme` 和 `SplitterDragBarTheme` 承载分割线和拖拽命中区。 |

## 3. API 与契约模型

Splitter 的公共契约由根控件 API、面板附加属性、折叠模型、事件和主题入口共同组成。`SplitterPanel`、`SplitterHandle` 和 `SplitterDragBar` 是 internal 协作对象，不属于用户可直接依赖的 public surface。

| 契约组 | 代表成员 | 维护含义 |
| --- | --- | --- |
| 根布局 | `Orientation`、`Children` | 定义面板排列方向和面板集合。 |
| 拖拽反馈 | `IsLazy`、`HandleSize` | 控制拖拽反馈时机和把手命中区域尺寸。 |
| 折叠入口 | `CollapsePreviousIcon`、`CollapseNextIcon`、`Splitter.Collapsible`、`Splitter.IsCollapsed` | 定义面板是否可折叠、折叠状态以及折叠图标。 |
| 面板尺寸 | `Splitter.Size`、`Splitter.DefaultSize`、`Splitter.MinSize`、`Splitter.MaxSize` | 定义子面板初始尺寸、默认尺寸和尺寸约束。 |
| 面板交互 | `Splitter.IsResizable` | 控制相邻边界是否允许拖拽调整。 |
| 事件 | `ResizeStarted`、`ResizeDelta`、`ResizeCompleted`、`DraggerDoubleClicked` | 把 internal drag 和分隔条双击转换为控件级事件。 |
| 主题入口 | `HandleSize`、控件 Token、根模板外观入口 | 区分命中区域、可见分割线和根框架外观。 |

样式能力边界：

- `HandleSize` 只表示分割把手的命中区域，不等同于可见分割线厚度。
- 可见分割线的厚度、圆角和 hover/dragging 颜色属于 handle 视觉模型，应通过根控件实例属性和 Token 映射到 internal handle，而不是要求用户样式化 internal 类型。
- Splitter 根框架可承接 `Background`、`BorderBrush`、`BorderThickness`、`CornerRadius`、`BorderDashArray`、`BorderDashOffset` 等基础外观属性；子面板圆角仍由用户提供的面板控件自行控制。
- 不新增 `PanelCornerRadius`、`PanelBackground` 这类统一改写子面板的属性，避免 Splitter 篡改用户内容树。

Semantic Part 契约：`Splitter` 公开 `root`、`panel`、`dragger` 三个 Semantic Part。`panel` 的 marker 由
`SplitterPanel` 运行时添加到用户面板，`dragger` 命中每个 handle 模板内的 `PART_DragBar`；二者均为
`RuntimeCreated` 的 Selector 型 Part，完整字段、路由与定制边界见 [Splitter Semantic Part 契约](semantic-part.md)。

稳定 template part：

| Template Part | 类型 | 稳定性 | 职责 |
| --- | --- | --- | --- |
| `PART_SplitterPanel` | `SplitterPanel` | public control template part | 承载子面板并生成 internal handle。 |
| `PART_HandleLine` | `Border` | internal handle template part | 展示可见分割线。 |
| `PART_DragBar` | `SplitterDragBar` | internal handle template part | 提供拖拽命中区和拖拽事件源。 |
| `PART_Grip` | `Border` | internal drag-bar template part | 展示拖拽区域内的 grip。 |
| `PART_CollapsePrevButton` | `IconButton` | internal handle template part | 触发前侧面板折叠或展开。 |
| `PART_CollapseNextButton` | `IconButton` | internal handle template part | 触发后侧面板折叠或展开。 |

`PART_HandleLine`、`PART_DragBar`、`PART_Grip` 和折叠按钮虽然是稳定 internal theme part，但不应作为用户自定义入口暴露。用户级定制应通过 `Splitter` public API、控件 Token、ControlTheme 和 Gallery 展示的稳定用法进入。

伪类模型：

- `Splitter` 根控件当前不定义专属伪类。
- `SplitterHandle` 使用 `:pointerover` 和 `:dragging` 驱动分割线颜色。
- `SplitterDragBar` 使用 `:dragging` 表达拖拽状态。
- `IconButton` 折叠按钮继续使用标准 pointer/pressed/disabled 视觉。

## 4. 行为与状态模型

Splitter 的状态流按以下路径收敛：

```text
Public API / attached panel properties / pointer drag / collapse button
  -> SplitterPanel layout state
  -> SplitterHandle drag and collapse state
  -> SplitterDragBar pointer state
  -> ControlTheme selector / template binding / resize events
  -> Gallery 可观察行为
```

行为规则：

- `Orientation=Vertical` 表示左右分割，handle 横向拖动；`Orientation=Horizontal` 表示上下分割，handle 纵向拖动。
- `HandleSize` 控制 handle 在布局中占用的命中区域，不能被可见线条厚度替代。
- `IsLazy=False` 时拖拽过程中实时调整面板尺寸；`IsLazy=True` 时拖拽过程中移动 drag bar，完成后提交尺寸。
- `Splitter.Size` 是用户可双向绑定的实际尺寸入口；`DefaultSize` 是未提供实际尺寸时的初始化入口。
- `MinSize`、`MaxSize`、`IsResizable` 和折叠状态共同决定某个 handle 是否可拖拽。
- 分隔条第二次左键按下不会再次启动 drag；第二次左键释放后，`DraggerDoubleClicked` 以零基 `HandleIndex` 通知所属 `Splitter`。
- `DraggerDoubleClicked` 不自动修改面板尺寸或折叠状态。应用可在处理器中把需要恢复的面板 `Size` 写回 `DefaultSize`，绑定场景应恢复绑定的数据源。
- `IsResizable=False` 只禁用拖拽，分隔条双击通知仍然可用；嵌套 Splitter 的通知不会传播到外层 Splitter。
- 折叠按钮只在相邻面板支持折叠或存在可恢复折叠面板时显示。
- resize 事件以 handle index 和当前尺寸快照暴露，不让用户直接依赖 internal handle 实例。

## 5. 视觉与主题模型

Splitter 的视觉模型由根控件模板、internal 面板和 handle 模板、SharedToken、控件 Token 共同构成。

| 主题文件 | 职责 |
| --- | --- |
| `SplitterTheme.axaml` | 定义 `Splitter` 根模板、外层 Frame 和 `PART_SplitterPanel`。 |
| `SplitterHandleTheme.axaml` | 定义可见分割线、拖拽命中区和折叠按钮的组合结构。 |
| `SplitterDragBarTheme.axaml` | 定义拖拽命中区、grip 尺寸、grip 圆角和方向 cursor。 |

视觉语义拆分：

- 根框架：承载 Splitter 整体背景、边框、圆角和裁剪。
- 命中区域：由 `HandleSize` 和 `SplitBarHandleSize` 定义，保证拖拽易用性。
- 可见分割线：由 line thickness、line corner radius 和 line color 定义，不能反向改变命中区域。
- grip：由 drag bar 模板展示，使用较短尺寸提示可拖拽。
- 折叠按钮：依赖 `SplitterPanelCollapsible` 和 hover/press 状态显示，不参与面板尺寸计算。

主题维护规则：

- 不删除或重命名已经稳定的 ControlTheme key、template part、伪类和资源 key。
- 不把可由 AXAML 表达的模板状态迁移为 C# 动态创建视觉。
- 不把 hover、dragging、collapsed 等运行时状态写入 Token。
- 调整线条厚度时必须同时检查 `PART_HandleLine` 和 `PART_Grip`，避免可见线条与拖拽提示尺寸不一致。

## 6. 控件家族或集成关系

Splitter 的运行时组合关系如下：

| 类型 | 可见性 | 职责 |
| --- | --- | --- |
| `Splitter` | public | 公开 API、Children、附加属性、resize 事件和 Token scope。 |
| `SplitterPanel` | internal | 管理用户面板、生成 handle、计算尺寸、处理折叠与拖拽。 |
| `SplitterHandle` | internal | 表达相邻面板之间的交互边界，协调 drag bar 和折叠按钮。 |
| `SplitterDragBar` | internal | 继承 Thumb，提供 pointer drag 源和方向 cursor。 |
| `SplitterPanelCollapsible` | public data model | 描述面板可折叠方向、显示策略和折叠尺寸。 |
| `SplitterResizeEventArgs` | public event args | 暴露 resize 事件中的 handle index 和尺寸快照。 |
| `SplitterDraggerDoubleClickedEventArgs` | public event args | 暴露双击的零基 handle index，不携带 internal handle。 |
| `SplitterToken` | internal design token | 从 SharedToken 派生组件级视觉变量。 |

集成关系：

- Gallery ShowCase 展示基本、水平、嵌套、禁用调整、折叠、多面板和 lazy 场景。
- API 契约摘要是用户 API 文档的结构化来源。
- Token 类型、生成数据和 token.md是 Splitter Token 的结构化来源。
- Splitter 不参与 Form value、CompactSpace、Popup/Flyout 或 Window 生命周期。

## 7. 兼容性不变量

维护 Splitter 时必须保持以下不变量：

- 不改变 `Orientation`、`IsLazy`、`HandleSize`、附加尺寸属性、折叠属性和 resize 事件的默认语义。
- 不改变 `DraggerDoubleClicked` 在第二次左键释放后触发、抑制第二次 drag start、只通知直接所属 Splitter 且不自动 reset 的语义。
- 不把 `HandleSize` 重新定义为可见分割线厚度。
- 不让 internal `SplitterPanel`、`SplitterHandle`、`SplitterDragBar` 成为用户必须引用的样式 API。
- 不删除或重命名 `PART_SplitterPanel`，也不随意重命名 internal handle template part。
- 不改变 Gallery 已展示的 XAML 用法、默认外观、交互顺序和状态优先级。
- 不通过延迟刷新、吞异常或特殊 Gallery 判断掩盖布局状态问题。
- 不引入运行时反射扫描作为 API、Token 或 Gallery 示例发现机制。
- 文档只描述当前稳定设计和维护规则；历史变化记录在 `changelog.md`。

## 8. 专项模型

### 8.1 面板尺寸模型

Splitter 的面板尺寸由用户面板上的附加属性描述：

- `Size` 是可绑定的当前尺寸。
- `DefaultSize` 用于缺省初始化，也可以作为应用处理 `DraggerDoubleClicked` 时显式恢复 `Size` 的目标值。
- `MinSize` / `MaxSize` 限制拖拽和折叠恢复后的尺寸。
- `IsResizable=False` 会让关联 handle 不接受拖拽，但不移除视觉分割线。
- `IsCollapsed` 是可绑定折叠状态，必须和尺寸快照保持一致。

尺寸值使用 `Dimension` 表达，支持固定值和百分比。布局算法必须在容器尺寸变化、面板集合变化、折叠恢复和 lazy 提交时保持约束一致。

### 8.2 折叠模型

`SplitterPanelCollapsible` 描述某个面板是否支持折叠、折叠目标尺寸和折叠图标显示策略。折叠行为由 `SplitterPanel` 根据相邻可见面板、可恢复面板和 `IsResizable` 共同决策。

折叠模型不允许跳过 `SplitterPanel` 直接操作 handle。handle 只负责接收按钮点击并向 panel 请求折叠或恢复。

### 8.3 分割线与圆角样式模型

Splitter 的样式能力划分为三层：

| 层级 | 职责 | 推荐入口 |
| --- | --- | --- |
| 根框架 | Splitter 整体背景、边框、虚线样式、圆角和裁剪。 | `Background`、`BorderBrush`、`BorderThickness`、`CornerRadius`、`BorderDashArray`、`BorderDashOffset` 的模板绑定。 |
| 可见分割线 | 分割线厚度、圆角、普通/hover/dragging 颜色。 | `LineThickness`、`LineCornerRadius`、控件 Token。 |
| 子面板内容 | 面板背景、面板圆角、内容 padding。 | 用户自己的子控件，例如 `Border`、`Card` 或业务布局容器。 |

新增实例级分割线样式能力时，必须把 API 定义在 `Splitter` 上，并通过 `SplitterPanel` 传递到 internal handle。不要要求用户引用 `SplitterHandle` 或 `SplitterDragBar`，也不要把 `HandleSize` 复用为线条厚度。

### 8.4 Semantic Part 模型

Splitter 通过 owner-scoped Semantic Selector 公开三个稳定语义区域：

| Part | AtomUI 节点 | 职责 |
| --- | --- | --- |
| `root` | `Splitter` | 分割容器根，承载面板集合、方向、附加属性 scope 与 resize 事件。 |
| `panel` | 用户面板子控件 | 可调整尺寸的内容面板，marker 由 `SplitterPanel` 运行时添加。 |
| `dragger` | `SplitterHandle` 模板中的 `SplitterDragBar#PART_DragBar` | 相邻面板之间的拖拽命中区。 |

`panel` / `dragger` 的数量随面板数量同步（N 个面板对应 N 个 `panel` 与 N-1 个 `dragger`），折叠、禁用拖拽、
方向与 lazy 切换只改变布局与状态，不改变 Part 数量。完整字段、SelectorRoute、数量语义与定制边界见
[Splitter Semantic Part 契约](semantic-part.md)。

## 9. 文档导航与验证策略

关联文档：

- [Splitter 桌面版实现原理](implementation.md)
- [Splitter Semantic Part 契约](semantic-part.md)
- [Splitter Token 设计](token.md)
- [Splitter Changelog](changelog.md)

验证策略：

| 改动类型 | 验证要求 |
| --- | --- |
| 文档改动 | 运行 `git diff --check`，检查相对链接存在。 |
| Public API | 覆盖属性默认值、resize 与双击事件触发、handle index、附加属性和继承语义。 |
| 状态模型 | 覆盖 drag、lazy、collapsed、disabled、hover、dragging 和折叠按钮显示。 |
| AXAML/Theme | 检查 `PART_SplitterPanel`、internal handle part、伪类、资源 key 和 Light/Dark 主题。 |
| Token | 检查 TokenKind、AXAML token resource、Token 类型、生成数据和 token.md和文档同步。 |
| Gallery | 走查对应 ShowCase 示例和源码片段入口，包括 `splitter-double-click-reset`。 |
