# Masonry 桌面版实现原理

本文档描述 Masonry 桌面版的内部布局引擎、两种自动列分配策略、响应式状态解析、item container 元数据、Semantic Part marker 和布局变化通知。公共设计与 API 契约见 [Masonry 桌面版架构设计](overview.md)，Semantic Part 契约见 [Masonry Semantic Part 契约](semantic-part.md)，变化记录见 [Masonry Changelog](changelog.md)。

## 1. 实现定位

Masonry 的实现由公开 `ItemsControl` 外壳和 internal `Panel` 布局引擎组成。`Masonry` 暴露布局属性、attached property 和事件；`MasonryPanel` 负责实际 `MeasureOverride` / `ArrangeOverride`。

实现文档聚焦布局计算、稳定列快照、响应式订阅、container 元数据读取、`.semantic-item` marker 生命周期和事件派发，不描述子项控件自身的渲染、图片加载或业务状态。

## 2. 源码文件结构

主要源码：

- `src/AtomUI.Desktop.Controls/Masonry/Masonry.cs`：公开控件类型、布局属性、attached property、`LayoutChanged` 事件入口和 item container prepare marker。
- `src/AtomUI.Desktop.Controls/Masonry/MasonryLayoutStrategy.cs`：公开布局策略枚举，定义稳定列与经典重排语义。
- `src/AtomUI.Desktop.Controls/Masonry/Masonry.SemanticParts.cs`：`item` Semantic Part descriptor；`root` 由生成器隐式补齐。
- `src/AtomUI.Desktop.Controls/Masonry/MasonryPanel.cs`：internal 布局引擎，执行测量、排列、item 动效（入场/滑动）、响应式断点监听和布局结果比较。
- `src/AtomUI.Desktop.Controls/Masonry/MasonryItemTransformAnimator.cs`：`ITransform?` 关键帧插值器，支撑 RenderTransform 滑动动画。
- `src/AtomUI.Desktop.Controls/Masonry/MasonryItemSpan.cs`：子项 span 枚举。
- `src/AtomUI.Desktop.Controls/Masonry/MasonryLayoutChangedEventArgs.cs`：布局结果事件参数。
- `src/AtomUI.Desktop.Controls/Masonry/Themes/MasonryTheme.axaml`：默认 ControlTheme，装配 root chrome `PixelAlignedBorder`、`ItemsPresenter`、离场 ghost 层和 `MasonryPanel`，并提供动效时长 token Setter。

## 3. 核心类职责

`Masonry` 是公共 API 与 Semantic owner 宿主，不直接计算子项矩形。它定义 `ColumnCount`、`ColumnInfo`、`MinColumnWidth`、`MaxColumnCount`、`ColumnGap`、`RowGap`、`Gutter`、`LayoutStrategy` 和 `Masonry.Column` / `Masonry.Span`，并通过继承自 `ItemsControl` 的 `Background`、`BorderBrush`、`BorderThickness`、`CornerRadius`、`Padding` 驱动 root chrome。`PrepareContainerForItemOverride` 负责为已准备 item container 补齐 `.semantic-item` marker。

`MasonryPanel` 是默认布局引擎。它只读取自身 `Children` 中的 item container，不进入 `ItemTemplate` 内部查找 attached property。这样直接子元素和 `ItemsSource` 两种模式可以共享同一布局路径。

`MasonryLayoutChangedEventArgs` 表达有效布局分配，不表达像素矩形。业务 key 由用户数据模型维护，事件只暴露 item container、顺序索引、有效列和整行状态。

## 4. 状态与数据流

布局状态流：

```text
Masonry layout properties
  ColumnCount / ColumnInfo / MinColumnWidth / MaxColumnCount
  ColumnGap / RowGap / Gutter / LayoutStrategy
      ↓ theme binding
MasonryPanel properties
      ↓
Resolve effective breakpoint values
      ↓
Resolve effective column count, gaps and LayoutStrategy
      ↓
Measure children and build candidate layout
      ↓
Arrange children and commit StableColumns snapshot
      ↓
Compare effective assignment snapshot and post LayoutChanged outside layout pass

Masonry root chrome properties
  Background / BorderBrush / BorderThickness / CornerRadius / Padding
      ↓ theme binding
PixelAlignedBorder#PART_RootBorder properties
```

响应式属性只在当前断点命中显式配置时覆盖兼容属性。`ColumnInfo` 未命中时回退 `ColumnCount` 或容器自适应列数；`Gutter` 未命中或某个维度未声明时回退 `ColumnGap` / `RowGap`。

`MasonryPanel` 同时拥有两类不同生命周期的布局状态：

- Measure→Arrange 候选布局：保存本轮矩形、有效列、整行标记和有效宽度，只跨越当前布局周期。
- `StableColumns` 已提交快照：保存 item container 引用到有效列的映射，可跨越后续布局周期，直到策略或列归属规则失效。

候选布局用于避免同一 Measure→Arrange 周期重复计算；稳定列快照用于定义跨 resize 和内容尺寸变化的列归属。两者不得合并为同一缓存，也不得在 Measure 阶段提交稳定状态。

Semantic Part 数据流：

```text
Items / ItemsSource
      ↓
Avalonia ItemsControl container resolution
      ↓
Masonry.PrepareContainerForItemOverride
      ↓
container.Classes += MasonrySemanticParts.ItemClass
      ↓
MasonryPanel.Children layout + owner-scoped MasonryItemStyle matching
```

`Masonry.SemanticParts.cs` 只声明 `item`；生成器为 Masonry 隐式补齐 `root` descriptor、`MasonrySemanticParts` 常量和 `MasonryItemStyle`。

## 5. 生命周期与模板接入

`MasonryTheme.axaml` 通过默认 `ItemsPanel` 装配 `MasonryPanel`，并用 `PixelAlignedBorder` 作为 root chrome 承载层。列数、间距和 `LayoutStrategy` 等布局属性通过 binding 从 `Masonry` 传递到 `MasonryPanel`，root chrome 属性则直接绑定到 `PixelAlignedBorder`，避免在 `Masonry` 中重复布局或 chrome 计算。

`MasonryPanel` 在进入可用视觉树后查找最近的媒体断点宿主，订阅断点变化并触发 `InvalidateMeasure`。宿主替换、detached 或控件释放时必须解除订阅。detached 同时清除候选布局和稳定列快照，不能通过字典继续持有已离开布局引擎的 item container。

Masonry 不 override `ItemsControl` 的 `NeedsContainer` 或 `CreateContainer`，不主动创建包装容器。两种内容提供方式的容器层级仍由 Avalonia 基类决定。Masonry 只 override `PrepareContainerForItemOverride`，先委托 `base` 完成原生准备，再幂等添加 `.semantic-item` class；该 marker 覆盖直接子元素和 `ItemsSource` generated `ContentPresenter` 两条路径。

## 6. 交互与事件处理

Masonry 不拦截 pointer、keyboard、focus、drag/drop、context menu 或子项动画事件。所有交互保留给子项控件和外层页面。

`LayoutChanged` 派发必须避开 Avalonia layout pass。布局过程中只记录最新有效分配快照；需要通知时通过 dispatcher 或等价机制延后派发，避免用户 handler 修改属性造成 layout re-entry。事件比较不包含像素矩形，因此 `StableColumns` 下仅有宽高或纵向位置变化时不派发；`Reflow` 重新计算后若有效列分配未变化，也不派发。

空集合也是有效布局状态。从非空布局变为空布局时派发一次空结果；布局持续为空时不重复派发。

## 7. 内部算法与关键流程

布局算法由 Avalonia `MeasureOverride` 与 `ArrangeOverride` 驱动：

```text
MeasureOverride(availableSize)
  resolve width
  resolve effective columns and gaps
  for each visible child in order:
    resolve span and explicit column
    measure child with column width or full width
    resolve automatic column through LayoutStrategy
    compute rect and update per-column heights
  cache rects for arrange
  return Size(resolvedWidth, maxColumnHeight)

ArrangeOverride(finalSize)
  reuse the Measure layout when the effective width is unchanged
  otherwise remeasure and recompute for the final width
  arrange each child using cached rect
  commit StableColumns container-to-column assignments
  compare the public LayoutChanged assignment snapshot
  return finalSize
```

`MeasureOverride` 保存本次测量生成的完整 `MasonryLayout`（矩形、列分配、整行标记、有效宽度和有效列数）。`ArrangeOverride` 先解析最终尺寸对应的有效宽度；如果与 Measure 结果一致，直接复用该结果，不再重复遍历 item 和列高集合。若宿主以无限宽度 Measure、再以有限视口宽度 Arrange，子项必须按最终列宽重新 Measure 后计算布局，不能复用基于另一宽度产生的 DesiredSize。候选缓存只跨越当前 Measure→Arrange 周期，Arrange 消费后立即释放。

有效列数：

```text
if ColumnInfo hits current breakpoint:
  EffectiveColumnCount = ColumnInfo
else if ColumnCount > 0:
  EffectiveColumnCount = ColumnCount
else:
  EffectiveColumnCount = floor((AvailableWidth + EffectiveColumnGap) / (MinColumnWidth + EffectiveColumnGap))
  EffectiveColumnCount = clamp(EffectiveColumnCount, 1, MaxColumnCount)
```

有效列宽：

```text
EffectiveColumnWidth =
  (AvailableWidth - EffectiveColumnGap * (EffectiveColumnCount - 1)) / EffectiveColumnCount
```

排列规则：

1. 按 `Children` 顺序处理子元素。
2. 不可见子元素不参与列高计算。
3. 普通子元素以有效列宽、无限高度测量。
4. `Masonry.Span=Full` 子元素以完整有效宽度、无限高度测量。
5. 指定 `Masonry.Column` 的子元素放入有效列；未指定列的子元素按有效 `LayoutStrategy` 解析列。
6. 多列等高时选择索引最小列。
7. 整行项从当前最高列之后开始，并将所有列高度同步到该子项底部。

自动列策略：

```text
Reflow
  every CalculateLayout call:
    assign each automatic item to the current shortest column
    do not read or commit cross-layout column assignments

StableColumns
  no committed assignment or changed effective column count:
    assign automatic items with the shortest-column algorithm
  unchanged effective column count:
    reuse the committed column for every existing item container
    assign only previously unseen item containers to the current shortest column
  after Arrange:
    commit the item-container-to-column snapshot
```

`Reflow` 保留经典 shortest-column 语义，但“重新计算”不等于每次都会产生不同结果；只有宽度、测量高度、item 集合或布局规则使最短列选择发生变化时，最终列归属才会变化。

稳定列快照以直接 item container 实例为 key，不使用数据索引或业务 key。这样删除或插入子项时，仍存在的 container 不会因索引移动而换列；新 container 才参与最短列选择。快照只在 Arrange 完成后提交，Measure 中间结果不成为下一轮布局的稳定状态。提交时清除旧字典并从本次可见、非整行 item container 的最终列分配重建快照，因此已删除或变为不可见的 container 不会继续保留；不可见 container 再次显示时作为未提交项进入当前最短列。

`StableColumns` 不等待异步内容进入最终状态。图片占位、延迟文本或其他内容第一次完成 Arrange 后，当前列分配即成为已提交状态；后续 DesiredSize 变化重新计算列高、矩形和同列后续子项的纵向位置，但不改变现存 container 的列。若调用方需要初始列平衡接近最终内容，应提供宽高比、稳定占位尺寸或显式选择 `Reflow`。

稳定列失效规则：

| 触发条件 | 稳定列处理 | 下一次布局 |
| --- | --- | --- |
| `LayoutStrategy` 改变 | 立即清空快照。 | 按目标策略重新计算；切换到 `StableColumns` 时在 Arrange 后提交新快照。 |
| 有效列数改变 | 旧列索引不再复用。 | 全部自动子项按新列数重新计算并提交。 |
| 直接 item container 的 `Masonry.Column` 或 `Masonry.Span` 改变 | 清空完整快照并 InvalidateMeasure。 | attached property 优先，其他自动子项按当前策略重新计算。 |
| 可用宽度、子项 DesiredSize 或间距改变，但有效列数不变 | 保留快照。 | 现存 container 保持列，只重新计算尺寸和纵向位置。 |
| 新增或删除 item container | 保留仍存在 container 的提交列。 | 新 container 进入当前最短列；删除项在提交重建时移除。 |
| item container 变为不可见 | 本轮提交时从快照移除。 | 再次可见时作为未提交项进入当前最短列。 |
| `MasonryPanel` detached | 清空快照和候选布局。 | 重新接入后从无提交状态开始。 |

Measure 与 Arrange 的布局结果必须对同一输入保持一致。缓存不能跨越影响布局的属性变更、子元素变更或可用宽度变更。

## 8. Item 动效（对齐 Ant Design 6.6.3）

Masonry 为 item 提供三类动效，语义与 antd 6.6.3 Masonry 对齐：入场淡入（appear fade-in）、位置滑动（position glide）与
离场淡出（leave fade-out）。RTL 场景下排列矩形按 `x' = width - rect.Right` 镜像，ghost 托管位置同样镜像。

时长与缓动来源：

- `MotionDuration`（入场/滑动）：ControlTheme Setter 取 `MotionDurationSlow` token，默认 300ms。
- `LeaveMotionDuration`（离场）：ControlTheme Setter 取 `MotionDurationFast` token，默认 100ms。
- 缓动为 `CubicEaseOut`，等价 antd `motionEaseOut = cubic-bezier(0.215, 0.61, 0.355, 1)`。
- 全局 `EnableMotion=false` 时 token 值归零，全部动效退化为瞬时（无预置、无动画、立即移除）。

驱动模型（`MasonryPanel`）：

```text
ArrangeOverride
  → ApplyItemMotion(child, visualRect)
      首次排列 → StartAppearMotion：Opacity 0→1（新项只淡入，不滑动）
      位置变化 → StartGlideMotion：RenderTransform translate(old-new)→0（既有项只滑动，不淡入）
      入场淡入激活中的项不做位置过渡（antd `-fade` 项语义）
```

动效偏移来自旧/新 Arrange 矩形的运行时布局状态，ControlTheme 无法表达，因此在面板代码驱动；时长值由 ControlTheme 从
token Setter 提供（`MasonryTheme.axaml`）。起始值以 Animation 优先级预置避免首帧跳变；结束后通过 `SetValue` 返回的
`IDisposable` 句柄释放——`AvaloniaObject.SetValue` 对非 LocalValue 优先级的 `UnsetValue` 是静默忽略的，必须 Dispose
句柄才能回落用户样式基值（样式值/本地值在动效结束后自动接管）。滑动中的子项再次变位时，先冻结读取当前视觉平移，取消
旧动画后从当前视觉位置续滑。

`RenderTransform`（`ITransform?`）在 Avalonia 动画注册表中没有关键帧插值器，直接对其做关键帧动画会抛
"No animator registered"；`MasonryPanel` 静态构造通过 `Animation.RegisterCustomAnimator<ITransform?, MasonryItemTransformAnimator>`
注册插值器（对齐既有 `MotionTransformOptionsAnimator` 对 `TransformOperations` 的处理，非 `TransformOperations` 值按
Identity 插值）。注册只影响"关键帧动画 ITransform 属性"这一此前必然抛异常的场景。

离场淡出（ghost 层托管，`Masonry`）：

```text
item 从集合移除
  → MasonryPanel.ChildrenChanged(Remove)   ← override，base 之后收集被移除子项
  → Masonry.NotifyItemsRemoved
  → 控件创建瞬态 ghost host Border，承载被移除容器，置于模板 PART_MotionGhostLayer 的原位置
  → Opacity 1→0 淡出（LeaveMotionDuration）
  → 释放：host.Child 置空并从 ghost 层移除
```

ghost host 是控件创建的瞬态承载节点，不属于语义部件；淡出期间容器保留 `.semantic-item` marker（antd CSSMotion leave
期间节点仍在 DOM）。容器在淡出期内重加入集合时，面板在 `base.ChildrenChanged`（VisualChildren 同步，含视觉父级校验）
之前调用 `Masonry.TryReleaseMotionGhost` 归还容器，避免"已有视觉父级"异常；`Masonry` detached 或模板重应用时清空全部
ghost。`Children` 集合的 `ResetBehavior.Remove` 保证 `Clear()` 产生逐项 Remove 事件，离场路径统一，无需 Reset 分支。

## 9. 资源、性能与 AOT 边界

Masonry 不使用反射读取 item template 内部元素，不创建不可见测量控件，不通过透明元素扩展命中区域。Semantic Part 常量、descriptor 和 `MasonryItemStyle` 均由源生成器在编译期生成，不依赖运行时程序集扫描。

稳定列快照只存储直接 item container 引用和列索引，由 `MasonryPanel` 单独持有。每次 Stable Arrange 后重建快照，detached 时清空；不复制业务数据，不要求稳定 key，也不改变 ItemsControl 容器生命周期。

响应式 resolver 必须保持纯计算属性，不依赖 VisualTree、主题资源或控件实例默认值。控件级 fallback 始终保留在 Masonry effective state 层。

布局变化通知比较维度只包含 item container 数量、顺序、有效列和 `IsFullSpan`。不把像素矩形作为事件比较维度，可以避免宽度变化、字体渲染或子项高度细微变化造成高频事件。

## 10. 维护不变量

内部重构必须保持以下不变量：

- `MasonryPanel` 保持 internal。
- `MasonryPanel` 只读取直接 child 上的 `Masonry.Column` 和 `Masonry.Span`。
- 直接子元素模式不额外包装子项；`.semantic-item` marker 加在用户直接子 `Control` 上。
- `ItemsSource` 模式通过基类生成 `ContentPresenter`，Masonry 不重写容器生成；`.semantic-item` marker 加在 generated container 上。
- `MasonryItemStyle` 的 `ContractType` 保持 `Control`，不得收窄到 `ContentPresenter` 或任何具体 item 控件。
- root chrome 由默认主题中的 `PixelAlignedBorder` 承载；`Background`、`BorderBrush`、`BorderThickness`、`CornerRadius` 和 `Padding` 仍然属于 Masonry 的 inherited root 属性，不要改成 item 属性。
- 响应式断点变化只触发布局失效，不在断点回调中执行完整布局或派发事件。
- Measure→Arrange 同一有效宽度必须复用已测量的布局结果；不得在正常布局周期中无条件重复执行第二次 `O(items × columns)` 计算。
- 布局缓存不得跨越宽度、断点或下一次 Measure；Arrange 消费后必须释放缓存引用。
- `LayoutChanged` 不在 layout pass 内同步派发。
- `LayoutChanged` 只比较 item container 数量、顺序、有效列和整行状态，不因像素矩形、列宽或列内纵向位置变化派发。
- `StableColumns` 的持久状态只由 `MasonryPanel` 拥有，并按 item container 引用关联；不得退化为按索引保存。
- `StableColumns` 只在 Arrange 后提交分配；Measure 不得修改已提交快照。
- `StableColumns` 不等待异步内容加载完成；调用方通过尺寸约束控制首次分配依据。
- `Reflow` 每次布局计算都从当前高度状态执行 shortest-column 分配，不读取稳定列快照。
- 替换 `ItemsPanel` 等价于替换布局引擎，Masonry-specific 布局语义不再由默认面板保证。
- item 动效时长只来自 ControlTheme token Setter（`MotionDurationSlow` / `MotionDurationFast`）；动画优先级持有必须能释放回用户样式基值，释放 `SetValue` 预置必须 Dispose 返回句柄。
- 新项只入场淡入不滑动；既有项位置变化只滑动不淡入；入场淡入激活中的项不做位置过渡；移除项由 ghost 层在原位置淡出且保留 marker。
- ghost host 由控件创建，不参与命中测试；容器重加入必须在 `base.ChildrenChanged` 之前释放 ghost；`Masonry` detached 时清空全部 ghost。
- RTL 镜像只作用于排列的视觉矩形与 ghost 托管位置，不改变逻辑顺序与布局算法。

## 11. 测试与验证

验证范围：

- 默认值和无效值归一：列数、列宽、最大列数、水平/垂直间距。
- 响应式：`ColumnInfo`、`Gutter`、partial breakpoint map、水平/垂直维度独立 fallback。
- 布局：shortest-column、显式列、整行项、不可见子项、无限宽度、tie-break。
- 策略：默认 `StableColumns`、同列数 resize 和 DesiredSize 变化保持列、增删 container 保持现存列、`Reflow` 重新计算、列数及 attached property 变化重建分配、策略切换清空快照。
- 容器：直接子元素、`ItemsSource`、`ItemContainerTheme`、替换 `ItemsPanel`、`.semantic-item` marker 和 `MasonryItemStyle` route。
- 事件：有效布局分配变化、空集合通知、重复通知合并、layout pass 外派发。
- 动效：时长 token 管道、入场起点预置与结束后样式基值回落、滑动偏移与释放、续滑、零时长退化、离场 ghost 原位淡出/释放/重加入归还/Clear 全托管、RTL 镜像。
- Gallery：图片加载、异步高度变化、响应式示例、整行项示例、Semantic Part Preview 和 Semantic Style 示例。
- 文档改动：运行 `git diff --check`。
