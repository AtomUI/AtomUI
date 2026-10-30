# Splitter Semantic Part 契约

本文档定义 `AtomUI.Desktop.Controls.Splitter` 对应用公开的 Semantic Part、Selector、类型约束、数量语义和定制边界。
Splitter 的整体设计见 [Splitter 桌面版架构设计](overview.md)，descriptor 与真实模板/运行时节点映射见
[Splitter 桌面版实现原理](implementation.md)，Token 语义见 [Splitter Token 设计](token.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Owner 边界

Splitter 是单一 owner：`Splitter` 公开 `root`、`panel`、`dragger` 三个 Part，对应上游 Splitter 的稳定语义区域
`.ant-splitter`（根容器）、`.ant-splitter-panel`（面板）与 `.ant-splitter-bar`（相邻面板之间的拖动条）。AtomUI 不扩展
额外 Part：可见分割线、grip、折叠按钮均不属于 Semantic Part。

AtomUI 映射：

- `root` → `Splitter` owner 本身。
- `panel` → 用户提供的面板子控件。`SplitterPanel` 在跟踪面板时添加运行时 marker，面板实例即 Part 节点，无
  额外包装元素。
- `dragger` → 每个 internal `SplitterHandle` 模板中的 `SplitterDragBar#PART_DragBar`。handle 由 `SplitterPanel`
  在相邻面板之间动态创建，drag bar 是 handle 模板内的静态节点，marker 由主题静态声明。

以下类型不持有独立 Semantic descriptor：

- `SplitterPanel` 是 internal 协作模板部件（`PART_SplitterPanel`），承载布局、折叠与 handle 生成，不发布 Part。
- `SplitterHandle` 是 internal 交互边界，其公开语义通过 `Splitter` 的 `dragger` Part 表达，不单独声明 descriptor。
- `SplitterDragBar` 是 internal 拖拽入口，作为 `dragger` Part 的节点出现，不单独声明 descriptor。

所有 Part 的 `Since` 统一为 `6.2.0`。

## 2. Semantic Parts

### 2.1 `root`

| 字段 | 值 |
| --- | --- |
| Owner | `Splitter` |
| Part | `root` |
| Selector | Splitter 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `Splitter` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | Splitter owner |
| 职责 | 分割容器根：承载 `Children` 面板集合、`Orientation` 方向语义、附加面板属性 scope、resize 事件与 Token scope；作为全部 Part 的 owner-scoped Selector 作用域边界。对应上游 `.ant-splitter`。 |
| 相关 API | `Orientation`、`IsLazy`、`HandleSize`、`LineThickness`、`LineCornerRadius`、`Splitter.Size`、`Splitter.IsResizable`、`Splitter.IsCollapsed`、`Splitter.Collapsible`、`ResizeStarted` / `ResizeDelta` / `ResizeCompleted` |
| 相关 Token | `SplitBarHandleSize`、SharedToken |
| 稳定性 | stable since 6.2.0 |

### 2.2 `panel`

| 字段 | 值 |
| --- | --- |
| Owner | `Splitter` |
| Part | `panel` |
| Selector | `.semantic-panel` |
| SelectorRoute | `/template/ .semantic-scope-panel > .semantic-panel` |
| Style Type | `SplitterPanelStyle` |
| ContractType | `Control` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | 用户提供的面板子控件（`SplitterPanel` 跟踪时添加运行时 marker） |
| 职责 | 统一表示可调整尺寸的内容面板：面板背景、边框、裁剪与排版入口；面板尺寸与约束由附加属性驱动。对应上游 `.ant-splitter-panel`。 |
| 相关 API | `Splitter.Size`、`Splitter.DefaultSize`、`Splitter.MinSize`、`Splitter.MaxSize`、`Splitter.IsResizable`、`Splitter.IsCollapsed`、`Splitter.Collapsible` |
| 相关 Token | 无专属 Token（面板内容与外观属于用户内容容器） |
| 稳定性 | stable since 6.2.0 |

### 2.3 `dragger`

| 字段 | 值 |
| --- | --- |
| Owner | `Splitter` |
| Part | `dragger` |
| Selector | `.semantic-dragger` |
| SelectorRoute | `/template/ .semantic-scope-panel > .semantic-scope-handle /template/ .semantic-dragger` |
| Style Type | `SplitterDraggerStyle` |
| ContractType | `Thumb`（`AtomUI.Controls.Primitives.Thumb`） |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | 每个 internal `SplitterHandle` 模板中的 `SplitterDragBar#PART_DragBar` |
| 职责 | 统一表示相邻面板之间的拖拽命中区：drag 输入入口、方向 cursor 与 grip 视觉宿主；命中区域尺寸由 handle 布局写入。对应上游 `.ant-splitter-bar`。 |
| 相关 API | `IsLazy`、`HandleSize`、`LineThickness`、`LineCornerRadius` |
| 相关 Token | `SplitTriggerSize`、`SplitBarDraggableSize`、`HandleLineThickness`、`HandleLineColor`、`HandleLineHoverColor`、`HandleLineDragColor` |
| 稳定性 | stable since 6.2.0 |

### 2.4 marker 放置与路由

`root` 是隐式 Part，不声明 `.semantic-root` marker。非 root Part 使用两层 marker 表达路由：

- 静态 marker 在主题中声明：
  - `SplitterTheme.axaml` 在 `SplitterPanel#PART_SplitterPanel` 上声明 `Classes.semantic-scope-panel`。
  - `SplitterHandleTheme.axaml` 在 `SplitterDragBar#PART_DragBar` 上声明 `Classes.semantic-dragger`。
  - `SplitterDragBarTheme.axaml` 不声明任何 `.semantic-*` marker。
- 运行时 marker 由 `SplitterPanel` 维护：
  - `CreateHandle` 为每个新创建的 `SplitterHandle` 添加 `semantic-scope-handle` 类。
  - `SyncTrackedPanels` 为新跟踪的用户面板添加 `semantic-panel` 类，并用 `_semanticPanelMarkersAdded`
    记录「由 Splitter 添加」的实例集合；面板离开时只移除 Splitter 自己添加的 marker，用户预先声明的
    `semantic-panel` 类保留。

路由语义：

- `panel` 从 owner 出发单跳进入 `PART_SplitterPanel`（scope class），再取直接子面板上的 `.semantic-panel`。
- `dragger` 跨越两层模板：先从 owner 模板进入 scope panel，经 `.semantic-scope-handle` 定位每个动态 handle，
  再进入 handle 自身模板命中 `PART_DragBar` 上的 `.semantic-dragger`。`CrossVisualRoot=false` 表示 handle
  模板仍在同一视觉树内，不涉及 popup、overlay 或独立 visual root。

## 3. Part 说明

### 3.1 root

`Splitter.root` 是 Splitter owner 本身，每个 Splitter 实例恰好一个。它承载面板集合、方向语义、附加面板属性
scope、resize 事件与 Token scope；根框架视觉由 `SplitterTheme` 模板表达。适合通过 root 定制整体
`Background`、`BorderBrush`、`BorderThickness`、`CornerRadius`、`BorderDashArray`、`BorderDashOffset`
与布局型 `Width` / `Height` / `Margin`。

root 不表示 `PART_SplitterPanel`、任何 internal handle、drag bar、可见分割线、grip 或折叠按钮；这些节点的名称、
数量与层级由各自的 Part 契约或内部实现约束定义。

### 3.2 panel

`panel` 表示用户提供的内容面板，数量与 Splitter 的面板子控件一致（N 个面板 N 个 marker）。marker 由
`SplitterPanel` 在跟踪面板时添加，节点即用户控件本身，没有额外包装元素。面板尺寸、约束与折叠语义由
`Splitter.Size`、`Splitter.DefaultSize`、`Splitter.MinSize`、`Splitter.MaxSize`、`Splitter.IsResizable`、
`Splitter.IsCollapsed`、`Splitter.Collapsible` 附加属性驱动，marker 只提供主题命中入口，不参与布局计算。

适合定制 `Background`、`BorderBrush`、`CornerRadius`、`Opacity` 等视觉属性。布局型固定 `Width` / `Height` /
`Margin` / Min / Max Setter 不作为 `panel` 的公共定制路径：面板几何由附加属性与 `SplitterPanel` 布局算法
写入，固定几何会绕过尺寸归一。

### 3.3 dragger

`dragger` 表示相邻面板之间的拖拽命中区，数量与 handle 一致（N 个面板 N-1 个）。每个 handle 的模板恰好包含
一个 `SplitterDragBar#PART_DragBar`，marker 由 `SplitterHandleTheme` 静态声明；handle 由 `SplitterPanel`
动态创建与销毁，marker 随 handle 实例增删。命中区域尺寸由 handle 布局写入（`HandleSize` 与
`SplitBarHandleSize` / `SplitTriggerSize`），可见线条与 grip 由 `LineThickness` / `LineCornerRadius` 驱动。

适合定制 `Background`、`BorderBrush`、`Opacity` 等视觉属性。布局型固定 `Width` / `Height` / `Margin` Setter
不作为 `dragger` 的公共定制路径：命中区域尺寸由布局写入，固定几何会破坏拖拽易用性与面板占位。

### 3.4 面板与 handle 状态的关系

- `Splitter.IsCollapsed`、`Splitter.IsResizable`、`Splitter.Collapsible` 变化只影响布局、handle 能力与按钮显示，
  不增删 `panel` / `dragger` 节点或 marker。
- `Children` 增删面板时：`panel` marker 随被跟踪面板增删（用户预声明的类保留）；handle 数量随相邻面板数量
  变化重建，`semantic-scope-handle` 与 `dragger` marker 随新实例。
- `Orientation` 切换只改变布局方向与 cursor，不增删节点或 marker。
- 可见分割线 `PART_HandleLine`、grip `PART_Grip` 与折叠按钮不属于 Semantic Part，其状态变化不影响三个 Part
  的数量与 marker。

## 4. Selector 用法

应用级样式先限定 owner，再通过生成的 Semantic Style 进入 Part。生成类型封装 owner 类型保护与
`SelectorRoute`，用户不需要复制模板路径：

```xml
<Application.Styles>
    <Style Selector="atom|Splitter">
        <atom:SplitterPanelStyle x:SetterTargetType="Control">
            <Setter Property="Background" Value="#F0F0F0" />
        </atom:SplitterPanelStyle>
        <atom:SplitterDraggerStyle x:SetterTargetType="atom:Thumb">
            <Setter Property="Background" Value="#1677FF" />
        </atom:SplitterDraggerStyle>
    </Style>
</Application.Styles>
```

对特定 class 或状态定制时，把 class、属性或伪类放在 owner 一侧：

```xml
<Style Selector="atom|Splitter.semantic-custom:pointerover">
    <atom:SplitterDraggerStyle x:SetterTargetType="atom:Thumb">
        <Setter Property="Background" Value="#91CAFF" />
    </atom:SplitterDraggerStyle>
</Style>
```

不得把 `ContractType` 写入 Part Selector。以下写法不属于公共契约：

- `atom|Splitter.semantic-panel`、`Control.semantic-panel` 或 `:is(Control).semantic-panel` 等类型变体。
- 直接复制 `/template/ .semantic-*` route 作为用户主路径；route 只属于 descriptor 与生成 Style 的实现元数据。
- 依赖 `PART_SplitterPanel`、`PART_DragBar`、internal 类型（`SplitterPanel`、`SplitterHandle`、
  `SplitterDragBar`）、Name 或视觉祖先顺序。
- 依赖 `semantic-scope-panel` / `semantic-scope-handle` 这两个 scope 类；scope 类只用于路由解析，不作为
  定制入口。

## 5. 状态与数量语义

数量契约以已实例化的内置节点为边界。root 为 `Single`，panel / dragger 为 `Multiple`；panel 与 dragger 都是
`RuntimeCreated` Part，marker 随节点实例存在。

| 场景 | root | panel | dragger | scope panel | scope handle | 说明 |
| --- | --- | --- | --- | --- | --- | --- |
| N 个面板 | 1 | N | N-1 | 1 | N-1 | 相邻面板之间各一个 handle。 |
| 0 个面板 | 1 | 0 | 0 | 1 | 0 | 无 handle，scope panel 仍在。 |
| 增加面板 | 1 | 随 N | 随 N-1 | 1 | 随 N-1 | 新面板 marker 由 `SplitterPanel` 添加。 |
| 移除面板 | 1 | 随 N | 随 N-1 | 1 | 随 N-1 | Splitter 添加的 marker 移除；用户预加类保留。 |
| `IsCollapsed` 切换 | 1 | N | N-1 | 1 | N-1 | 只改变布局尺寸与按钮显示。 |
| `IsResizable` 切换 | 1 | N | N-1 | 1 | N-1 | 只改变 handle 拖拽能力。 |
| `Orientation` 切换 | 1 | N | N-1 | 1 | N-1 | 只改变方向与 cursor。 |
| `IsLazy` 切换 | 1 | N | N-1 | 1 | N-1 | 只改变尺寸提交时机。 |

## 6. 尺寸基线

Splitter 没有 `SizeType` 分档，视觉基线由 `SplitterToken` 与全局 token 常量表达：

- 命中区域：`HandleSize` public API、`SplitBarHandleSize`、`SplitTriggerSize`。
- 可见分割线与 grip：`LineThickness` / `LineCornerRadius` public API、`HandleLineThickness`、`SplitBarSize`、
  `SplitBarDraggableSize`。
- 折叠按钮：`SplitBarCollapseOffset`、`SplitBarCollapseOffsetNegative`、`SplitBarCollapseCrossOffset`、
  `HandleIconSize`。
- 颜色状态：`HandleLineColor`、`HandleLineHoverColor`、`HandleLineDragColor`、`HandleIconColor`、
  `HandleIconHoverColor`、`HandleIconPressedColor`。

`panel` 没有控件级尺寸基线：面板几何由附加属性与布局算法驱动。`dragger` 的命中区域尺寸由 handle 布局写入，
Semantic Style 覆盖视觉属性时不影响命中区域与面板占位。

## 7. 定制边界

以下区域明确不属于 Splitter Semantic Part：

- 可见分割线 `PART_HandleLine` 与 grip `PART_Grip`（internal theme part，通过 `LineThickness` /
  `LineCornerRadius` public API 与 Token 定制）。
- 折叠按钮 `PART_CollapsePrevButton` / `PART_CollapseNextButton`（通过 `CollapsePreviousIcon` /
  `CollapseNextIcon` 与 `Splitter.Collapsible` 定制）。
- `SplitterPanel#PART_SplitterPanel`（internal 协作模板部件，布局 / 折叠 / handle 生成 owner）。
- internal 类型 `SplitterPanel`、`SplitterHandle`、`SplitterDragBar` 及 `semantic-scope-*` 类。
- 面板内容子树：面板内部结构、圆角与内容 padding 由用户子控件负责。
- `PART_*` 名称、internal 类型、Name 与模板层级。

Semantic Style 服从 Avalonia 原生属性优先级。Part Setter 命中只证明目标属性已生效；如果最终视觉仍被面板
折叠、handle 可见性或布局约束影响，应按跨节点布局约束排查，不能把它解释为 Semantic Style 优先级失效。

## 8. 兼容性与验证

删除或重命名 Part、修改 selector class、收窄 `ContractType`、改变 cardinality、修改 `SelectorRoute` 语义，或者
让任一内置节点缺少 marker，均属于公共主题契约变更。

验证至少覆盖：

- `Splitter` descriptor 只有 `root`、`panel`、`dragger`，字段值与本文表格一致（`panel` 的 `ContractType` 为
  `Control`，`dragger` 为 `AtomUI.Controls.Primitives.Thumb`；两者均 `RuntimeCreated=true` 且携带显式
  `SelectorRoute`）。
- `SplitterPanel`、`SplitterHandle`、`SplitterDragBar` 不持有独立 descriptor。
- 主题 marker：`SplitterTheme.axaml` 只声明 `semantic-scope-panel`，`SplitterHandleTheme.axaml` 只声明
  `semantic-dragger`，`SplitterDragBarTheme.axaml` 不声明任何 `.semantic-*` marker。
- 运行时数量：N 个面板产生 N 个 `semantic-panel` 与 N-1 个 `semantic-dragger`；`Children` 增删同步 marker；
  移除面板时 Splitter 添加的 marker 被移除、用户预加的 `semantic-panel` 类保留。
- `IsCollapsed`、`IsResizable`、`Collapsible`、`Orientation`、`IsLazy` 切换不增删任何 Part 节点或 marker。
- owner-scoped Semantic Style（生成的 `SplitterPanelStyle` / `SplitterDraggerStyle`）与 `x:SetterTargetType`
  可以编译并命中对应最低 public 类型。
- 默认主题不消费 `.semantic-*`，未声明用户 Semantic Style 时不增加 selector activator。
- Generator 静态输出和 NativeAOT 路径不依赖反射或运行时扫描。
