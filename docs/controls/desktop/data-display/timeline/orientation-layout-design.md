# Timeline 方向与布局设计

本文档定义 Timeline 在垂直、水平、Start、End、Alternate、Reverse、RTL、隐藏项和 Pending 组合下的稳定布局契约。公共设计入口见 [Timeline 桌面版架构设计](overview.md)，内部实现职责见 [Timeline 桌面版实现原理](implementation.md)，视觉变量边界见 [Timeline Token 设计](token.md)。

## 1. 设计定位

Timeline 使用两个正交维度描述布局：

- `Orientation` 决定事件沿垂直轴还是水平轴排列。
- `Mode` 决定 Label 和 Content 位于时间轴交叉轴的 Start、End 或交替侧。

同一个 `Timeline`、`TimelineItem`、布局 Panel、Indicator renderer 和 ControlTheme 同时支持两种方向。方向差异只改变测量、排列和轴线绘制策略，不创建水平专用控件，也不依赖上层 `Steps` 控件实现。

## 2. 设计原则

- Timeline 是方向、Mode、视觉顺序和 Pending 相邻状态的唯一 owner。
- `Start`、`End` 使用逻辑位置语义，不使用固定物理 Left/Right 表达公共契约。
- Item、Panel、Indicator 和 Theme 只消费 Timeline 单向投影的状态，不反向修改 owner 状态。
- 水平 Timeline 的所有可见项等宽，节点共用一条水平轴线。
- 隐藏项不占布局槽位，也不参与 Alternate、首尾或 Pending 相邻状态计算。
- `IsReverse` 只反转主轴视觉顺序，不改变 Mode 的交叉轴语义。
- 窄宽度通过项内文本换行退化，不自动改变 Orientation，也不创建内部水平滚动。
- 垂直和水平布局复用现有模板节点，不复制 ControlTemplate 或新增方向专用视觉树。

## 3. 专项模型与 Public API

### 3.1 Public API

Timeline 使用 Avalonia 的 `Orientation`，默认沿垂直方向排列：

```csharp
public Orientation Orientation { get; set; } = Orientation.Vertical;
```

Timeline 的 Mode 定义为：

```csharp
public enum TimelineMode
{
    Start,
    End,
    Alternate
}
```

`Mode` 默认值为 `TimelineMode.Start`。Timeline 不提供单个 `TimelineItem` 的 placement 属性，也不定义第二个 Start/End 枚举。

### 3.2 稳定术语

| 术语 | 定义 |
| --- | --- |
| 主轴 | TimelineItem 依次排列的方向；Vertical 时为纵轴，Horizontal 时为横轴。 |
| 交叉轴 | 与主轴垂直的方向，用于表达 Start、End 和 Alternate。 |
| 源顺序 | Items/ItemsSource 生成容器后的集合顺序。 |
| 视觉顺序 | 过滤隐藏项并应用 `IsReverse` 后的最终显示顺序。 |
| 视觉索引 | Item 在视觉顺序中的零基索引。 |
| Effective Mode | 将 `Alternate` 按视觉索引解析后得到的 `Start` 或 `End`。 |
| 双侧布局 | Label 和 Content 分居轴线两侧，并为所有 Item 保留一致的轴线位置。 |

## 4. 方向与模式策略

### 4.1 布局矩阵

| Orientation | Mode | 无 Label | 存在 Label |
| --- | --- | --- | --- |
| `Vertical` | `Start` | Indicator 位于逻辑起始侧，Content 位于结束侧。 | Label 位于起始侧，Content 位于结束侧。 |
| `Vertical` | `End` | Content 位于逻辑起始侧，Indicator 位于结束侧。 | Content 位于起始侧，Label 位于结束侧。 |
| `Vertical` | `Alternate` | 第一可见项按 Start 排列，后续按 End、Start 交替。 | 使用同一交替规则，Label 始终位于 Content 的相对侧。 |
| `Horizontal` | `Start` | Indicator 在上，Content 在下。 | Label 在上，Content 在下。 |
| `Horizontal` | `End` | Content 在上，Indicator 在下。 | Content 在上，Label 在下。 |
| `Horizontal` | `Alternate` | 第一可见项的 Content 在下，后续上下交替。 | 第一可见项的 Label 在上、Content 在下，后续交换。 |

只要任一可见 Item 声明了 `Label`，整个 Timeline 使用 Label 布局，使所有 Item 的 Indicator 保持同轴。没有 Label 的 Item 在对应 Label 区域保留空位，不改变其他 Item 的轴线位置。

### 4.2 FlowDirection

- Vertical + Start 在 LTR 下映射到左侧，在 RTL 下映射到右侧。
- Vertical + End 在 LTR 下映射到右侧，在 RTL 下映射到左侧。
- Horizontal + Start 固定表示轴线下方，Horizontal + End 固定表示轴线上方。
- FlowDirection 可以镜像水平 Timeline 的主轴阅读顺序，但不交换上方和下方。

### 4.3 Reverse、隐藏项与 Pending

- `IsReverse=false` 时视觉顺序等于过滤隐藏项后的源顺序。
- `IsReverse=true` 时视觉顺序是过滤隐藏项后的源顺序反转。
- Alternate 始终从视觉顺序索引 0 的 Start 开始计算。
- `IsFirst` 和 `IsLast` 基于视觉顺序，不基于原始 ItemCount。
- `NextIsPending` 表示视觉顺序中的下一项是 Pending Item。
- Pending Item 作为普通可见 Item 参与等宽、首尾和 Alternate 计算。
- Pending Item 不可见时，不参与任何视觉顺序派生状态。

## 5. 架构与职责

| 类型或文件 | 稳定职责 |
| --- | --- |
| `AbstractTimeline` | 拥有 Orientation、Mode、IsReverse、Items 和 Pending，计算视觉顺序并向 Item 单向投影状态。 |
| `AbstractTimelineItem` | 保存用户内容和 Timeline 投影的内部状态，在 Label 或可见性变化时请求 owner 重新计算。 |
| `TimelineStackPanel` | 沿主轴测量和排列可见 Item，处理 Reverse、Spacing 和水平等宽槽位。 |
| `TimelineItemPanel` | 将 Label、Indicator、Content 按 Orientation 和 Effective Mode 排列。 |
| `TimelineIndicator` | 根据 Orientation、首尾状态、节点尺寸和自定义图标边界绘制节点与连接线。 |
| `TimelineTheme.axaml` | 组合 ScrollViewer、ItemsPresenter 和 TimelineStackPanel，并传递 Orientation。 |
| `TimelineItemTheme.axaml` | 组合 Label、Indicator、Content，并传递 Orientation、Mode 和顺序状态。 |
| `TimelineIndicatorTheme.axaml` | 提供节点图标模板和 Indicator 视觉资源。 |

Timeline 的共享实现位于 `AtomUI.Controls`，不能引用位于 `AtomUI.Desktop.Controls` 的 Steps Panel 或 Steps 状态类型。桌面包只负责公开 `Timeline`/`TimelineItem` 类型、主题和 Token。

## 6. Template 与组合契约

默认组合结构为：

```text
Timeline
  -> Border#Frame
     -> ScrollViewer#ScrollViewer
        -> ItemsPresenter#ItemsPresenter
           -> TimelineStackPanel

TimelineItem
  -> TimelineItemPanel#RootLayout
     -> TimelineSectionPanel#Section
        -> StackPanel#Header
           -> TextBlock#Label
        -> TimelineIndicator#Indicator
           -> Border#PART_Rail
           -> Border#PART_Dot
           -> Border#PART_IconHost
              -> IconPresenter#PART_IconPresenter
        -> ContentPresenter#ContentPresenter
```

`Orientation` 从 Timeline 传递给 TimelineStackPanel 和每个 TimelineItem，再由 Item 模板传递给 TimelineItemPanel 与 TimelineIndicator。`Mode` 和视觉顺序状态沿同一路径单向传递。

模板维护必须遵守：

- 保留 `RootLayout`、`Section`、`Header`、`Label`、`Indicator`、`PART_Rail`、`PART_Dot`、`PART_IconHost`、
  `PART_IconPresenter` 和 `ContentPresenter` 的稳定职责。
- Vertical 专属的 item bottom padding 和 Pending 大间距不能泄漏到 Horizontal 布局。
- Horizontal Indicator 沿横向 Stretch；Vertical Indicator 保持固定的交叉轴尺寸。
- Mode 和 Orientation 差异通过属性、selector 和 Panel 布局表达，不复制模板。
- Timeline 的内部 ScrollViewer 不提供水平滚动；有限宽度由 TimelineStackPanel 分配给等宽 Item。

## 7. 核心算法与失效规则

### 7.1 视觉顺序归一

每次结构状态变化时按以下顺序计算：

1. 从已生成的 TimelineItem 容器中筛选 `IsVisible=true` 的项。
2. 保持源顺序或根据 `IsReverse` 反转，形成视觉顺序。
3. 按视觉索引设置 `IsOdd`、`IsFirst` 和 `IsLast`。
4. 将所有 `NextIsPending` 重置，再根据视觉邻接关系设置 Pending 前一项。
5. 检查可见项是否包含 Label，并把统一的 `IsLabelLayout` 投影到全部 Item。

该算法的输入只包括容器顺序、Item 可见性、Label、IsReverse 和 Pending Item 身份。Panel 不允许使用原始 ItemCount 再次推导视觉索引。

### 7.2 Effective Mode

Effective Mode 按视觉索引计算：

```text
Mode == Start     -> Start
Mode == End       -> End
Mode == Alternate -> visualIndex % 2 == 0 ? Start : End
```

`IsOdd` 只表达视觉索引是否为奇数。布局代码使用一个共享解析入口得到 Effective Mode，避免 Measure、Arrange、alignment 和 renderer 分别解释 Alternate。

### 7.3 水平等宽分配

设可见项数量为 `N`、Panel 有限可用宽度为 `W`、项间距为 `S`：

```text
slotWidth = max(0, W - S * (N - 1)) / N
```

- `N=0` 时 DesiredSize 为空尺寸，不执行除法。
- 每个可见 Item 使用相同 slotWidth 测量和排列。
- Panel 高度取所有可见 Item DesiredHeight 的最大值，并以该高度排列每个 Item。
- 不可见 Item 不计入 N，也不占用 Spacing。
- 可用宽度为无限值时，各 Item 按自然宽度测量，Panel DesiredWidth 为可见项宽度与 Spacing 之和。
- 最终 Arrange 宽度与 Measure 宽度不同时，使用 final width 重新计算槽位，避免累计误差。

### 7.4 Item 测量与排列

Vertical 布局沿用 Label、Indicator、Content 的水平三段模型。存在 Label 或 Mode 为 Alternate 时，轴线两侧获得等宽区域；单侧 Start/End 模式只为 Content 和 Indicator 分配空间。

Horizontal 布局分为：

- 单侧布局：无可见 Label 且 Mode 不是 Alternate。Start 按 Indicator、gap、Content 排列；End 按 Content、gap、Indicator 排列。
- Label 堆叠布局：存在可见 Label 且 Mode 不是 Alternate。Start 按 Indicator、gap、Label、gap、Content 依次向下排列（轴线在上，Label 与 Content 在下并对轴线水平居中）；End 按 Label、gap、Content、gap、Indicator 排列（轴线在下）。Label 与 Content 各取自然高度，无 Label 的 Item 收缩为单侧布局的尺寸。
- Alternate 布局：Mode 为 Alternate。Panel 使用上方区域、Indicator 区域、下方区域，并根据 Effective Mode 交换 Label 与 Content，Label 与 Content 对轴线水平居中。

Alternate 布局中，每个 Item 使用 Label 和 Content 高度的较大值作为单侧 extent。DesiredHeight 由两倍 side extent、Indicator extent 和两侧 gap 组成；TimelineStackPanel 再取所有 Item 的最大高度，确保节点在同一水平轴线上。文本宽度始终受 slotWidth 约束，因此长文本在 Item 内换行。

### 7.5 Indicator 轴线几何

- Vertical 模式的 rail 覆盖节点中心上方和下方的轴线区间。
- Horizontal 模式的 rail 覆盖节点中心主轴前方和后方。
- 垂直方向对齐上游 rail 分段语义：每段从自身节点底边延伸到下一节点顶边（贯穿 item 边界），线紧贴
  节点并掩膜遮盖、视觉连续；末项（含单 Item）rail 收缩为零尺寸，语义框以节点为界。
- 水平方向 rail 贯穿相邻 item 连成完整轴线（节点位于 item 中心、单段只能覆盖到 item 边缘）：首项从
  自身节点起、末项止于自身节点、中间项全宽贯通，交叉区域由节点掩膜遮盖。
- 内置圆点使用 `IndicatorDotSize` 和 `IndicatorDotBorderWidth` 计算中心与盒尺寸。
- 自定义图标使用 `PART_IconHost` 的实际 Bounds 中心作为 rail 裁剪点，不透明宿主掩膜 rail。
- Orientation、首尾状态、图标、尺寸变化必须使对应 Measure 或 Arrange 失效。

### 7.6 重新计算与重新布局

| 变化 | 必须执行的动作 |
| --- | --- |
| Items 增删、替换、Reset、容器索引变化 | 重算视觉顺序、Label 布局、首尾和 Pending 邻接。 |
| Item `IsVisible` 或 `Label` 变化 | 重算可见项视觉顺序或 Label 布局。 |
| `IsReverse` 或 Pending 变化 | 重算视觉顺序和 Pending 邻接。 |
| `Mode` 变化 | 重新 Measure/Arrange Item，不改变源集合顺序。 |
| `Orientation` 变化 | 重新 Measure/Arrange Timeline、Item 和 Indicator，并重新 Render 轴线。 |
| FlowDirection 变化 | 重新排列逻辑 Start/End，不改变 Effective Mode。 |

## 8. 资源、性能与 AOT 边界

- 视觉顺序计算为 O(N)，只在集合、可见性、Label、Reverse 或 Pending 结构状态变化时执行，不进入 Render 热路径。
- 水平 Measure 和 Arrange 均为 O(N)，不创建方向专用 Visual，也不复制 Item 模板。
- TimelineIndicator 不再自绘：rail、圆点与图标宿主都是模板元素，几何在 Arrange 阶段计算，无 Pen 缓存。
- Orientation 和 Mode 使用静态 AvaloniaProperty 注册及模板绑定，不使用反射、动态类型发现或运行时扫描。
- Item 通过 owner 回调触发结构重算，不为每个 Item 建立无释放路径的长期订阅。
- 设计不新增 timer、异步任务、Popup、DynamicResource owner 或 NativeAOT 动态入口。

## 9. 兼容性与定制边界

- `TimelineMode` 只包含 `Start`、`End`、`Alternate`；`Left`、`Right` 和重复值别名不属于契约。
- `Orientation` 默认 `Vertical`，`Mode` 默认 `Start`。
- `IndicatorLeftModeMargin` 和 `IndicatorRightModeMargin` 的逻辑命名分别由 `IndicatorStartModeMargin` 和 `IndicatorEndModeMargin` 取代，不保留旧资源别名。
- `TimelineItem` 没有独立 placement owner；自定义主题不能通过局部 Item 状态覆盖 Timeline 的全局 Mode。
- ControlTheme key、现有 Template Part 和顺序伪类保持稳定。
- 应用替换 Item 模板时，必须继续向布局 Panel 和 Indicator 传递 Orientation、Mode、首尾及顺序状态。
- 水平等宽、项内换行、不自动切换方向和不创建内部水平滚动属于可观察行为。

## 10. 验证要求

| 层级 | 必须覆盖的行为 |
| --- | --- |
| API | TimelineMode 精确值、Orientation/Mode 默认值和运行时切换。 |
| 状态 | 可见项视觉索引、Alternate 首项、Reverse、隐藏项、首尾、Label 和 Pending 邻接。 |
| Panel | Vertical 既有布局、Horizontal 等宽、Spacing、零项、无限宽和 final width 重算。 |
| Item | Orientation x Start/End/Alternate x Label 矩阵、长文本换行和轴线对齐。 |
| Indicator | 垂直/水平首尾 rail 裁剪、内置圆点几何、图标宿主掩膜与 `:icon-present` 切换。 |
| Theme | Orientation/Mode 传递、方向 selector、Template Part、Light/Dark 和 Browser/Desktop 一致性。 |
| RTL | 垂直 Start/End 左右镜像，水平上下语义保持不变。 |
| Gallery | Start、End、Alternate 三种水平示例以及动态 Mode 示例。 |
| AOT | Gallery NativeAOT publish 不产生新增 binding、reflection 或 trimming 问题。 |
