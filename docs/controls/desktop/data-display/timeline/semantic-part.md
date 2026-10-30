# Timeline Semantic Part 契约

本文档定义 Timeline 对应用公开的 Semantic Part、选择器、类型约束、数量语义和定制边界。Timeline 的整体设计见
[Timeline 桌面版架构设计](overview.md)，方向与布局策略见 [Timeline 方向与布局设计](orientation-layout-design.md)，
descriptor 与真实模板节点映射见 [Timeline 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Semantic Parts

Timeline 以单一 owner `Timeline` 公开全部九个 Semantic Part，与上游稳定 Semantic DOM 一一对齐。上游
`TimelineSemanticType` 是 `StepsSemanticType` 去掉 `itemSubtitle` 后的九个语义键（`classNames` / `styles` 均为
`{ root?, item?, itemWrapper?, itemIcon?, itemSection?, itemHeader?, itemTitle?, itemContent?, itemRail? }`），
Timeline 委托 `Steps`（`type="dot"`）渲染：`root` 消费于 `<ol>` 根节点，`item` 消费于每个 `<li>` 容器，
`itemWrapper` 消费于容器内裹节点，`itemIcon` 消费于图标节点，`itemSection` 消费于包含 header 与 content 的
区域容器，`itemHeader` 消费于包含 title 与 rail 的头部容器，`itemTitle` 消费于标题节点，`itemContent` 消费于
内容节点，`itemRail` 消费于节点连接线元素。上游 DOM 嵌套为
`ol > li > wrapper > [icon, section > [header > [title, rail], content]]`（rc-steps `Step.tsx`，
npm `@rc-component/steps@1.2.2`，上游 6.4.5 依赖 `~1.2.2`）。

AtomUI 原 item 模板是扁平结构，且连接线由 `TimelineIndicator.Render` 直接绘制。为支持全部九个 Part，本次
改造把控件自身的视觉结构补齐为真实节点：

```text
Timeline (root)
  └─ TimelineItem (item, .semantic-item)
       └─ TimelineItemPanel#RootLayout (itemWrapper, .semantic-item-wrapper)
            └─ TimelineSectionPanel#Section (itemSection, .semantic-item-section)
                 ├─ StackPanel#Header (itemHeader, .semantic-item-header)
                 │    └─ TextBlock#Label (itemTitle, .semantic-item-title)
                 ├─ TimelineIndicator#Indicator (.semantic-indicator 跳点)
                 │    ├─ Border#PART_Rail (itemRail, .semantic-item-rail)
                 │    ├─ Border#PART_Dot (itemIcon, .semantic-item-icon，无 IndicatorIcon 时可见)
                 │    └─ Border#PART_IconHost (itemIcon, .semantic-item-icon，有 IndicatorIcon 时可见)
                 │         └─ IconPresenter#PART_IconPresenter（图标内容，非 Part）
                 └─ ContentPresenter#ContentPresenter (itemContent, .semantic-item-content)
```

与上游 DOM 的两处结构差异（Part 名称、数量与样式语义不变）：

- 上游 `rail` 元素 DOM 上位于 `header` 内（CSS absolute 定位到轴线）；AtomUI 的 rail 是
  `TimelineIndicator` 模板内的真实 `Border` 元素，与圆点/图标同属轴线列。Avalonia Panel 只能排列自己的直接
  子级，把 rail 放在 header 内无法由面板定位到轴线，因此 rail 归属轴线列是布局事实的忠实表达。
- 上游 `icon` 是 `wrapper` 的直接子级；AtomUI 的图标宿主位于 `TimelineIndicator`（section 内）。该层级差异
  不影响 Part 的寻址与样式语义。

`itemIcon` 的双元素承载与上游对齐：上游 icon 元素在无自定义 icon 时本身就是圆点（dot 尺寸 + 边框即圆环），
有自定义 icon 时是图标盒。AtomUI 对应地把 `.semantic-item-icon` 同时标记在 `Border#PART_Dot`（内置圆点，
无 `IndicatorIcon` 时可见）与 `Border#PART_IconHost`（图标宿主，有 `IndicatorIcon` 时可见）两个互斥可见的
Border 上，`itemIcon` 样式（如 `BorderBrush`）会作用到当前可见的那个 —— 与上游
`styles.itemIcon.borderColor` 同时能改圆点环色与图标盒边框的行为一致。

改造的视觉保真保证：圆点与连接线的绘制从 `TimelineIndicator.Render` 迁移为真实模板元素（`PART_Dot` 与
`PART_Rail`），rail 覆盖整条轴线、由不透明圆点/图标宿主掩膜出与原先完全一致的线段缺口；几何公式与
IsFirst/IsLast 裁剪边界逐项保留，默认主题下渲染结果像素级不变（见 2.5 节与
[Timeline 桌面版实现原理](implementation.md)）。

上游 Semantic DOM 文档还有第二个预览区块 "Timeline Items"（`items[].classNames` 逐项注入，
`root` / `wrapper` / `icon` / `section` / `header` / `title` / `content` / `rail` 八个键）。它是每项数据上的
classNames 注入机制，不是独立 owner；AtomUI Semantic Part 系统不提供逐项 classNames 注入 API，且 item 级
Part 的 cardinality 已经是 `Multiple`（天然覆盖每一项）。Gallery Semantic Parts 页签用两个
`SemanticPartPreview` 复刻上游的两个预览区块：`Timeline`（九卡，对应组件级 Part）与 `Timeline Items`
（两 item 预览 + 九卡短描述，对应上游逐项视图的 item 级 Part 呈现）；hover 高亮均为 owner 作用域
（覆盖预览内全部 item），逐项注入式高亮不适用。

AtomUI 九个 Part 随本次 Semantic Part 改造同时公开，descriptor 的 `Since` 统一为 `6.2.0`。

以下类型不持有独立 Semantic descriptor：

- `TimelineItem` 是公开 item 容器，上游 `TimelineSemanticType` 声明在 Timeline 组件上（`Timeline.Item` 只是
  兼容入口，无独立 Semantic DOM Props），容器职责由 Timeline 的 `item` 等 Part 表达（与 `SegmentedItem`、
  `ListBoxItem`、`CollapseItem` 同一决策）。
- `AbstractTimeline`、`AbstractTimelineItem` 是跨平台共享基类，不是对应用公开的独立 owner。
- `TimelineIndicator`、`TimelineItemPanel`、`TimelineSectionPanel`、`TimelineStackPanel` 是 internal 协作
  类型；其中 `TimelineIndicator` 模板节点只作为 `itemIcon` / `itemRail` route 的中间跳点
  （`.semantic-indicator`），不是 Part。

### 1.1 `Timeline`

#### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `Timeline` |
| Part | `root` |
| Selector | Timeline 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `Timeline` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | Timeline owner（表面投影到 `Border#Frame`） |
| 职责 | Timeline root 是 Items、Orientation、Mode、IsReverse、Pending 与可见项视觉顺序的统一 owner；根表面（背景、边框、圆角、内边距）投影到 `Frame`。 |
| 相关 API | `Items`、`ItemsSource`、`Orientation`、`Mode`、`IsReverse`、`Pending`、`PendingIcon` |
| 相关 Token | SharedToken（`ColorBorder`、`ColorBgContainer`） |
| 稳定性 | stable since 6.2.0 |

#### `item`

| 字段 | 值 |
| --- | --- |
| Owner | `Timeline` |
| Part | `item` |
| Selector | `.semantic-item` |
| SelectorRoute | `> .semantic-item` |
| Style Type | `TimelineItemStyle` |
| ContractType | `TimelineItem` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | 每个 `TimelineItem` 容器 |
| 职责 | 统一表示时间轴单个节点容器：单项 Label、Content、Indicator 的承载入口与视觉顺序派生状态的接收方；对应上游 `<li>`。 |
| 相关 API | `Label`、`Content`、`ContentTemplate`、`IndicatorIcon`、`IndicatorColor` |
| 相关 Token | `ItemPaddingBottom`、`ItemPaddingBottomLG`、`IndicatorStartModeMargin`、`IndicatorEndModeMargin`、`IndicatorMiddleModeMargin` |
| 稳定性 | stable since 6.2.0 |

#### `itemWrapper`

| 字段 | 值 |
| --- | --- |
| Owner | `Timeline` |
| Part | `itemWrapper` |
| Selector | `.semantic-item-wrapper` |
| SelectorRoute | `> .semantic-item /template/ .semantic-item-wrapper` |
| Style Type | `TimelineItemWrapperStyle` |
| ContractType | `Panel` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | TimelineItem 模板中的 `TimelineItemPanel#RootLayout` |
| 职责 | 统一表示节点内容包装容器：把 section 铺满自身并承载 IndicatorSpacing 等包装级布局状态；对应上游 item wrapper 节点。 |
| 相关 API | `Orientation`、`Mode`、`IsLabelLayout`、`IsOdd`（internal 投影） |
| 相关 Token | SharedToken（`UniformlyPaddingXS`） |
| 稳定性 | stable since 6.2.0 |

#### `itemIcon`

| 字段 | 值 |
| --- | --- |
| Owner | `Timeline` |
| Part | `itemIcon` |
| Selector | `.semantic-item-icon` |
| SelectorRoute | `> .semantic-item /template/ .semantic-indicator /template/ .semantic-item-icon` |
| Style Type | `TimelineItemIconStyle` |
| ContractType | `Border` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | TimelineIndicator 模板中的 `Border#PART_Dot` 与 `Border#PART_IconHost`（互斥可见） |
| 职责 | 统一表示节点图标区域：无 `IndicatorIcon` 时是内置圆点（`BorderBrush` 即圆环色），有 `IndicatorIcon` 时是图标宿主；对应上游 item icon 节点（上游同一元素两种形态）。 |
| 相关 API | `IndicatorIcon`、`IndicatorColor` |
| 相关 Token | `IndicatorSize`、`IndicatorDotSize`、`IndicatorDotBorderWidth`、SharedToken（`ColorPrimary`、`ColorBgContainer`） |
| 稳定性 | stable since 6.2.0 |

#### `itemSection`

| 字段 | 值 |
| --- | --- |
| Owner | `Timeline` |
| Part | `itemSection` |
| Selector | `.semantic-item-section` |
| SelectorRoute | `> .semantic-item /template/ .semantic-item-section` |
| Style Type | `TimelineItemSectionStyle` |
| ContractType | `Panel` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | TimelineItem 模板中的 `TimelineSectionPanel#Section` |
| 职责 | 统一表示节点区域容器：承载 header、Indicator 与 content 的方向化 Measure/Arrange（Alternate 双侧、同侧紧凑与水平 Label 堆叠模型）；对应上游 item section 节点。 |
| 相关 API | `Orientation`、`Mode`、`IsLabelLayout`、`IsOdd`（internal 投影） |
| 相关 Token | SharedToken（`UniformlyPaddingXS`） |
| 稳定性 | stable since 6.2.0 |

#### `itemHeader`

| 字段 | 值 |
| --- | --- |
| Owner | `Timeline` |
| Part | `itemHeader` |
| Selector | `.semantic-item-header` |
| SelectorRoute | `> .semantic-item /template/ .semantic-item-header` |
| Style Type | `TimelineItemHeaderStyle` |
| ContractType | `StackPanel` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | TimelineItem 模板中的 `StackPanel#Header` |
| 职责 | 统一表示节点头部容器：承载 title 文本与对齐方式；对应上游 item header 节点。 |
| 相关 API | `Label`、`Mode`、`Orientation` |
| 相关 Token | `ItemPaddingBottom`、`ItemPaddingBottomLG` |
| 稳定性 | stable since 6.2.0 |

#### `itemTitle`

| 字段 | 值 |
| --- | --- |
| Owner | `Timeline` |
| Part | `itemTitle` |
| Selector | `.semantic-item-title` |
| SelectorRoute | `> .semantic-item /template/ .semantic-item-title` |
| Style Type | `TimelineItemTitleStyle` |
| ContractType | `TextBlock` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | header 内的 `TextBlock#Label` |
| 职责 | 统一表示节点标题/时间标签区域：文本呈现、换行与下内边距；对应上游 item title 节点。 |
| 相关 API | `Label`、`Mode`、`Orientation` |
| 相关 Token | `ItemPaddingBottom`、`ItemPaddingBottomLG` |
| 稳定性 | stable since 6.2.0 |

#### `itemContent`

| 字段 | 值 |
| --- | --- |
| Owner | `Timeline` |
| Part | `itemContent` |
| Selector | `.semantic-item-content` |
| SelectorRoute | `> .semantic-item /template/ .semantic-item-content` |
| Style Type | `TimelineItemContentStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | section 内的 `ContentPresenter#ContentPresenter` |
| 职责 | 统一表示节点详细内容区域：`Content` / `ContentTemplate` 的呈现、受限宽度换行与下内边距；对应上游 item content 节点。 |
| 相关 API | `Content`、`ContentTemplate`、`Mode`、`Orientation` |
| 相关 Token | `ItemPaddingBottom`、`ItemPaddingBottomLG`、`LastItemContentMinHeight` |
| 稳定性 | stable since 6.2.0 |

#### `itemRail`

| 字段 | 值 |
| --- | --- |
| Owner | `Timeline` |
| Part | `itemRail` |
| Selector | `.semantic-item-rail` |
| SelectorRoute | `> .semantic-item /template/ .semantic-indicator /template/ .semantic-item-rail` |
| Style Type | `TimelineItemRailStyle` |
| ContractType | `Border` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | TimelineIndicator 模板中的 `Border#PART_Rail` |
| 职责 | 统一表示节点连接线（轴线轨道）：承载 IsFirst/IsLast 裁剪后的轨道条，厚度与颜色来自 `IndicatorTailWidth` / `IndicatorTailColor`；对应上游 item rail 节点。 |
| 相关 API | `IndicatorTailColor`、`IndicatorTailWidth`、`IndicatorColor` |
| 相关 Token | `IndicatorTailWidth`、`IndicatorTailColor`、`IndicatorDotSize` |
| 稳定性 | stable since 6.2.0 |

`root` 是隐式 Part，不声明 `.semantic-root` marker。`itemWrapper`、`itemSection`、`itemHeader`、
`itemTitle`、`itemContent` 的 marker 静态声明在 `TimelineItemTheme.axaml` 模板内；`itemIcon`、`itemRail` 的
marker 声明在 `TimelineIndicatorTheme.axaml` 模板内。这八个 Part 都位于运行时生成的 `TimelineItem` 容器
内部，descriptor 统一声明 `RuntimeCreated=true`，生成器不按 owner 主题资产做静态校验。

`item` 是 `RuntimeCreated` Part：`.semantic-item` marker 在容器创建与 prepare 路径幂等添加，Pending item
创建路径同样幂等补齐。Timeline 是 ItemsControl，`TimelineItem` 容器的逻辑父级就是 Timeline 本身（与
`ListView` / `ListBox` 的 `> .semantic-item` 同一模式），因此 route 不需要先经 `/template/` 进入内部 items
host，`>` 一步即可到达容器。

`itemIcon` 与 `itemRail` 的 route 需要两次 `/template/` 跳点：先进入 TimelineItem 模板命中
`TimelineIndicator#Indicator` 节点上的 `.semantic-indicator` 跳点（不是 Part），再进入 TimelineIndicator
模板命中对应 marker。

`ContractType` 只定义 Setter 可以稳定依赖的最低 public 类型，并通过 `x:SetterTargetType` 提供 AXAML 编译期
类型上下文；它不参与 `.semantic-*` 的身份匹配。`itemWrapper` / `itemSection` 的真实节点
`TimelineItemPanel` / `TimelineSectionPanel` 是 internal 类型，因此 ContractType 使用其最低公开基类
`Panel`（与 Calendar 对 internal cell 使用 `TemplatedControl` 同一决策）。

## 2. Part 说明

### 2.1 Timeline root

`root` 是 Timeline owner 本身，在 Timeline 实例的整个生命周期内始终存在，并且每个 Timeline 恰好一个。

它负责：

- 承载 `Items` / `ItemsSource`、`Orientation`、`Mode`、`IsReverse`、`Pending`、`PendingIcon` 等公共状态与
  API，并作为可见项视觉顺序（Alternate 奇偶、首尾、Pending 邻接）的唯一 owner。
- 提供根视觉属性：背景（`ColorBgContainer`）、边框（`ColorBorder`）、圆角与内边距，表面投影到模板中的
  `Border#Frame`。
- 作为 `item` 等 owner-scoped Selector 的作用域边界。

适合通过 root 定制 Timeline 整体背景、边框、圆角与 padding。root 不表示模板中的 `Frame`、
`ScrollViewer`、`ItemsPresenter` 或 `TimelineStackPanel` 节点本身；这些节点的名称、数量与层级不属于 root
契约。

### 2.2 Timeline item

`item` 表示时间轴上的一个节点容器，每个 `TimelineItem` 恰好一个 marker，cardinality 为 `Multiple`。marker
在容器创建与 prepare 路径幂等添加；容器生命周期内不增删 marker。

它负责：

- 承载单项 `Label`、`Content`、`ContentTemplate`、`IndicatorIcon`、`IndicatorColor` 的最终布局与呈现。
- 接收 owner 单向投影的方向、Mode、奇偶、首尾、Reverse、Pending 与 Label 布局状态，并投影到模板与伪类。

`item` 覆盖三条容器来源：用户显式声明的 `TimelineItem`、`ItemsSource` / 数据项自动生成的容器，以及
`Pending` 生成的 Pending item（`Timeline.CreatePendingItem` 创建的 `TimelineItem`，`IsPending=true`，
默认 `LoadingOutlined` 旋转图标）。三种来源的容器 marker 语义一致，与上游把 pending 归一为
`status=process` 的普通 item 同一决策。

适合定制 `Padding`、`Margin`、`MinHeight`、`Foreground`、`FontSize`、`Cursor` 等属性。Mode、Orientation、
`IsReverse`、Label 布局与 Pending 状态变化只改变有效视觉属性与伪类，不增删 marker（见 4 节）。

### 2.3 Timeline itemWrapper / itemSection / itemHeader

三个容器 Part 构成 item 模板的层级骨架，cardinality 均为 `Multiple`（每个容器各一个）：

- `itemWrapper` 承载 `TimelineItemPanel#RootLayout`：把 `TimelineSectionPanel` 铺满自身，是节点内容的包装
  根节点；对应上游 wrapper 节点。
- `itemSection` 承载 `TimelineSectionPanel#Section`：header、Indicator 与 content 的方向化 Measure/Arrange
  全部发生在该面板内（Alternate 双侧模型、同侧紧凑模型与水平 Label 堆叠模型），`Orientation`、`Mode`、
  `IsLabelLayout`、`IsOdd`、`IndicatorSpacing` 通过 TemplateBinding 流入；对应上游 section 节点。
- `itemHeader` 承载 `StackPanel#Header`：title 的头部容器；对应上游 header 节点。上游 header 还包含 rail
  元素，AtomUI 的 rail 归属轴线列（见 1 节差异说明）。垂直 Label 布局下 header 铺满所在列槽（对齐上游
  header 的 flex 列语义），`Label` 为空时 header 槽位保留且仍满足高亮资格（与上游空 title item 的 header
  框一致）。

适合定制 `itemWrapper` / `itemSection` 的 `Padding` / `Margin` 与 `itemHeader` 的对齐、间距等属性。

### 2.4 Timeline itemIcon / itemTitle / itemContent

三个内容 Part 各对应一个常驻节点，cardinality 均为 `Multiple`（每个容器各一个）：

- `itemIcon` 承载 `Border#PART_Dot` 与 `Border#PART_IconHost`（互斥可见）：无 `IndicatorIcon` 时内置圆点
  可见（背景 `ColorBgContainer`、`BorderBrush` 即圆环色），有 `IndicatorIcon` 时图标宿主可见（内含
  `IconPresenter`，主题设定尺寸 `IndicatorSize`，画刷随 `IndicatorColor`）。两个 Border 均带
  `.semantic-item-icon` marker，样式作用于当前可见者，与上游 icon 元素"圆点/图标盒同一元素"的行为一致。
- `itemTitle` 承载 `TextBlock#Label`：`Label` 的文本呈现，`TextWrapping=Wrap`，垂直 Label 布局下铺满
  header 槽位、文本朝轴线对齐；`Label` 为 null 时折叠为不可见（marker 保留在节点上，但不满足高亮资格，
  与上游空 title 的零高度行为一致）。
- `itemContent` 承载 `ContentPresenter#ContentPresenter`：`Content` / `ContentTemplate` 的呈现，字符串
  内容经 `StringToTextBlockConverter` 换行；垂直 Label 布局下铺满所在列槽、文本朝轴线对齐。

节点可见性全部由数据与方向状态驱动，Semantic Style 覆盖可见性会绕过数据状态机，属于不推荐用法。适合定制
`itemIcon` 的 `BorderBrush`（圆环色/图标盒边框）、`Background`、`CornerRadius`，`itemTitle` 与
`itemContent` 的 `LineHeight` / `Padding` / `Foreground` 等属性。

### 2.5 Timeline itemRail

`itemRail` 表示节点连接线（轴线轨道条），每个容器一个 `Border#PART_Rail` 元素，cardinality 为
`Multiple`。改造前连接线由 `TimelineIndicator.Render` 用 `DrawLine` 直接绘制；改造后迁移为真实
`Border` 元素，由 `TimelineIndicator` 的 Arrange 计算裁剪范围：

- 垂直方向：rail 位于轴线中心，宽度 `IndicatorTailWidth`、背景 `IndicatorTailColor`；对齐上游 rail 的
  分段语义——每段从自身节点底边延伸到下一节点顶边（贯穿 item 边界），线紧贴节点、由不透明节点掩膜遮盖，
  视觉连续；末项 rail 收缩为零尺寸，相邻段的语义框以节点为界（与上游 Semantic DOM 预览的分段框一致）。
- 水平方向：节点位于各 item 中心、items 左右并排，单段 rail 只能覆盖到 item 边缘（两节点连线的
  中点），因此水平 rail 贯穿相邻 item 连成完整轴线——首项从自身节点起、末项止于自身节点、中间项
  全宽贯通，交叉区域由不透明节点掩膜遮盖。
- 圆点/图标掩膜：内置圆点（`PART_Dot`，背景 `ColorBgContainer`、边框 `IndicatorColor`）或自定义图标的
  宿主框（`PART_IconHost`，`IndicatorIcon` 非空时背景 `ColorBgContainer`）不透明覆盖 rail 的圆点区域，
  形成与原线段绘制完全相同的缺口；默认主题下渲染结果像素级不变。
- 末项与单 item 的 rail 收缩为零尺寸元素，marker 保留。

适合定制 `Background`（连接线颜色）、`Width` / `Height`（厚度）、`CornerRadius` 等属性。`IndicatorTailColor` /
`IndicatorTailWidth` 属性与 Token 仍然是连接线的官方定制入口，Semantic Style 覆盖后优先级更高。

## 3. Selector 用法

应用级样式先限定 owner，再通过生成的 Semantic Style 进入 Part。生成类型已经封装 owner 类型保护和
`SelectorRoute`，用户不需要复制模板路径：

```xml
<Application.Styles>
    <Style Selector="atom|Timeline">
        <atom:TimelineItemStyle x:SetterTargetType="atom:TimelineItem">
            <Setter Property="Padding" Value="8,0" />
        </atom:TimelineItemStyle>
        <atom:TimelineItemSectionStyle x:SetterTargetType="Panel">
            <Setter Property="Margin" Value="4,0" />
        </atom:TimelineItemSectionStyle>
        <atom:TimelineItemIconStyle x:SetterTargetType="Border">
            <Setter Property="BorderBrush" Value="#1677FF" />
        </atom:TimelineItemIconStyle>
        <atom:TimelineItemHeaderStyle x:SetterTargetType="StackPanel">
            <Setter Property="Spacing" Value="2" />
        </atom:TimelineItemHeaderStyle>
        <atom:TimelineItemTitleStyle x:SetterTargetType="TextBlock">
            <Setter Property="FontWeight" Value="SemiBold" />
        </atom:TimelineItemTitleStyle>
        <atom:TimelineItemContentStyle x:SetterTargetType="ContentPresenter">
            <Setter Property="Foreground" Value="#595959" />
        </atom:TimelineItemContentStyle>
        <atom:TimelineItemRailStyle x:SetterTargetType="Border">
            <Setter Property="Background" Value="#91caff" />
        </atom:TimelineItemRailStyle>
    </Style>
</Application.Styles>
```

对特定 class 或状态定制时，把 class、属性或伪类放在 owner 一侧：

```xml
<Style Selector="atom|Timeline.semantic-custom">
    <atom:TimelineItemTitleStyle x:SetterTargetType="TextBlock">
        <Setter Property="Foreground" Value="#52c41a" />
    </atom:TimelineItemTitleStyle>
</Style>
```

不得把 `ContractType` 写入 Part Selector。以下写法不属于公共契约：

- `atom|TimelineItem.semantic-item` 或 `:is(atom|TimelineItem).semantic-item`。
- 直接复制 `> .semantic-item /template/ .semantic-item-wrapper` 或
  `> .semantic-item /template/ .semantic-indicator /template/ .semantic-item-icon` route 作为用户主路径；
  route 只属于 descriptor 与生成 Style 的实现元数据。
- 依赖 `PART_*`、internal 类型、Name 或视觉祖先顺序。
- 通过 Semantic Style 设置 `IsVisible` 绕过 `IndicatorIcon` / 圆点数据状态机。
- 依赖 `PART_*` 名称、internal 类型或视觉祖先顺序寻址单侧元素：`itemIcon` 的 Dot 与 IconHost 由
  `IndicatorIcon` 决定谁可见，样式同时作用于两者，不需要（也不应）分别寻址。

## 4. 状态与数量语义

数量契约以已实例化的 AtomUI 内置容器为边界。`item` 及其余八个 item 级 Part 都是 `RuntimeCreated` Part：
marker 随容器实例存在，不随数据项迁移。`itemIcon` 每个容器有两个 marker（`PART_Dot` 与 `PART_IconHost`
互斥可见，同一时刻只有一个参与渲染/高亮），其余 Part 每容器一个。

| 场景 | root | item | 其余七个 item 级 Part | 说明 |
| --- | --- | --- | --- | --- |
| 默认 Timeline N 个可见项 | 1 | N | 各 N | 每个容器一个 marker；title/content/icon 节点常驻，内容为空时 marker 保留（title 为空时节点折叠不可见）。 |
| 显式 `TimelineItem` 声明 | 1 | N | 各 N | 与生成容器语义一致。 |
| `ItemsSource` 数据项 | 1 | N | 各 N | 自动生成 `TimelineItem` 容器，marker 随容器就位。 |
| `Pending` 非空 | 1 | N+1 | 各 N+1 | Pending item 是普通 `TimelineItem`，marker 语义一致。 |
| 隐藏项（`IsVisible=false`） | 1 | 随容器数 | 随容器数 | 容器仍实例化、marker 保留；高亮与命中只针对有效可见实例。 |
| 空 Items 且无 Pending | 1 | 0 | 0 | 不创建容器。 |
| 项增删 / 集合重置 | 1 | 随容器数 | 随容器数 | 新容器创建时建 marker；容器释放时 marker 消失。 |
| Mode / Orientation 切换 | 1 | N | 各 N | 只改变布局与边距，不增删 marker。 |
| `IsReverse` 切换 | 1 | N | 各 N | 只改变视觉顺序投影，不增删 marker。 |
| Label 布局（`IsLabelLayout`）切换 | 1 | N | 各 N | 只改变 Margin 与排列，不增删 marker。 |
| 单 item（首尾同时成立） | 1 | 1 | 各 1 | rail 范围收缩为零尺寸，marker 保留。 |

## 5. 尺寸基线

Timeline 没有 `SizeType` 分档，视觉基线由 TimelineToken 与全局 token 常量表达：

- 节点指示器尺寸 `IndicatorSize`（`SizeMS`），内置圆点直径 `IndicatorDotSize`（8），节点边框宽度
  `IndicatorDotBorderWidth`。
- 连接线厚度 `IndicatorTailWidth`（`LineWidthBold`），颜色 `IndicatorTailColor`（`ColorSplit`）。
- 节点间距 `ItemPaddingBottom`（item 的下内边距，对齐上游 `li` 的 `paddingBottom`；指示器经 `AxisOverflow` 延伸
  到间距区保持 rail 连续），Pending 邻接升级为 `ItemPaddingBottomLG`，最后一个 Item 不保留（对齐上游
  `li:last-child`）。
  最小高度 `LastItemContentMinHeight`。
- Start/End/Alternate 指示器外边距分别为 `IndicatorStartModeMargin`、`IndicatorEndModeMargin`、
  `IndicatorMiddleModeMargin`。
- 根背景 `ColorBgContainer`、边框 `ColorBorder`；IndicatorSpacing 使用 SharedToken `UniformlyPaddingXS`。

Semantic Style 覆盖 `itemWrapper` / `itemSection` 的 `Padding` / `Margin`、`itemTitle` / `itemContent` 的
`LineHeight` / `Padding` 时，应验证垂直轴线的连接线仍贯穿节点中心、水平双侧模型的节点仍同轴，且
Pending 邻接大内边距语义不被覆盖破坏；覆盖 `itemRail` 的 `Background` / 厚度时应验证 rail 仍被圆点掩膜。

## 6. 定制边界

以下区域明确不属于 Timeline Semantic Part：

- 可见项视觉顺序算法（过滤隐藏项、`IsReverse`、Alternate 奇偶、首尾与 Pending 邻接）与
  `IsOdd` / `IsFirst` / `IsLast` / `NextIsPending` / `IsLabelLayout` / `IsPending` 等 internal 投影属性
  是行为状态，不是 Part。
- 内置圆点 `Border#PART_Dot` 与图标宿主框 `Border#PART_IconHost`：圆点是 `IndicatorIcon` 为 null 时的默认
  视觉，定制入口是 `IndicatorColor` / `IndicatorIcon`；宿主框是掩膜实现细节。
- `TimelineItem` 容器自身不作为独立 owner：上游无独立 Semantic DOM Props，其职责由 `item` 等 Part 表达。
- `TimelineIndicator`（`.semantic-indicator` 只作为 route 跳点）、`TimelineItemPanel`、
  `TimelineSectionPanel`、`TimelineStackPanel`、`ScrollViewer`、`ItemsPresenter`、`Border#Frame` 等模板
  结构节点。
- `AbstractTimeline` / `AbstractTimelineItem` 跨平台基类与 internal 状态属性。
- 用户 `ContentTemplate` 生成的子树、`PendingIcon` 的具体图标内容。
- 上游 "Timeline Items" 逐项 classNames 注入 API：AtomUI 无对应机制，item 级 Part 已 Multiple 覆盖
  （Gallery "Timeline Items" 预览是其结构呈现，不是注入 API 的对应物）。

Semantic Style 服从 Avalonia 原生属性优先级。Timeline 没有颜色状态机：节点颜色、边距与内边距由主题
selector 按方向、Mode、首尾与 Pending 邻接状态设置，用户 Semantic Style setter（`StyleTrigger`）覆盖主题
setter，样式移除后主题恢复。

## 7. 兼容性与验证

删除或重命名 Part、修改 selector class、收窄 `ContractType`、改变 cardinality，或者让任一内置模板缺少
marker，均属于公共主题契约变更。

验证至少覆盖：

- descriptor 中 `Timeline` 只有 `root`、`item`、`itemWrapper`、`itemIcon`、`itemSection`、`itemHeader`、
  `itemTitle`、`itemContent`、`itemRail`，字段值与本文表格一致；`TimelineItem`、基类与 internal 协作类型
  不持有 descriptor。
- 默认垂直 Start 模式下 marker 数量恒为 root=1、八个 item 级 Part 各 N；显式 `TimelineItem`、
  `ItemsSource` 生成容器与 `Pending` item 三种来源语义一致。
- Mode、Orientation、`IsReverse`、Label 布局与 Pending 切换只改变有效视觉属性，不增删 marker。
- 项增删、集合重置后 marker 随容器实例；隐藏项容器 marker 保留。
- 内置圆点（无 `IndicatorIcon`）与自定义 `IndicatorIcon` 两种形态下 `itemIcon` marker 恒在
  `PART_Dot` / `PART_IconHost` 两个 Border 上（每容器两个 marker，可见者随 `IndicatorIcon` 切换）。
- rail 元素几何：垂直/水平、首/中/尾、单 item 的裁剪范围与原线段绘制语义一致；`IsFirst` 上悬线、
  `IsLast` 下悬线不出现；圆点与图标掩膜生效。
- owner-scoped Semantic Style（九个生成 Style 类型）与 `x:SetterTargetType` 可以编译并命中对应最低 public
  类型。
- `itemIcon` / `itemRail` 三段 route（owner `>` 容器 `/template/` 跳点 `/template/` 目标）在生成 Style 中
  命中每个对应节点。
- 默认主题不消费 `.semantic-*`，未声明用户 Semantic Style 时不增加 selector activator。
- Generator 静态输出和 NativeAOT 路径不依赖反射或运行时扫描。
