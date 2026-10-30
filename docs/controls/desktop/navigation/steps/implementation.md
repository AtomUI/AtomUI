# Steps 桌面版实现原理

本文档描述 Steps 桌面版的单一状态投影、容器生命周期、统一语义模板、布局 Panel、受控交互、Wave 和 Progress 实现边界。公共设计与 API 契约见 [Steps 桌面版架构设计](overview.md)，Semantic Part 契约见 [Steps Semantic Part 契约](semantic-part.md)，Token 语义见 [Steps Token 设计](token.md)，变化记录见 [Steps Changelog](changelog.md)。

## 1. 实现定位

Steps 的实现目标是在 `ItemsControl` 容器体系内，把根输入和 item 显式状态确定性投影为视觉状态。实现不依赖 Selection、模板应用顺序、VisualTree attach 顺序或上一次计算结果。

本文档覆盖 `Steps`、`StepsItem`、`StepsItemIndicator`、三个 internal LayoutPanel、Panel item frame 和三个主题文件的稳定职责。通用 ItemsControl、TokenResource、Motion 和 PathIcon 实现不在本文档重复说明。

## 2. 源码文件结构

主要源码：

- `src/AtomUI.Desktop.Controls/Steps/Steps.cs`：public API、事件、容器生成、根输入分发和 item 状态协调。
- `src/AtomUI.Desktop.Controls/Steps/Steps.SemanticParts.cs`：`Steps` 的 Semantic Part descriptor（`root` + `item` + 七个 item 子 Part）。
- `src/AtomUI.Desktop.Controls/Steps/StepsItem.cs`：public item 契约、internal 派生状态、owner 生命周期和激活入口。
- `src/AtomUI.Desktop.Controls/Steps/StepsItemIndicator.cs`：Indicator 状态、Wave part、Progress 绘制和渲染失效。
- `src/AtomUI.Desktop.Controls/Steps/StepsPanel.cs`：item 间水平 flex、Navigation/Panel 等宽、Inline 和垂直 stack 布局。
- `src/AtomUI.Desktop.Controls/Steps/StepsItemLayoutPanel.cs`：Indicator、Section、Connector、NavigationArrow、PanelArrow 和 NavigationActiveIndicator 的 item 内布局。
- `src/AtomUI.Desktop.Controls/Steps/StepsItemSectionPanel.cs`：Header、SubHeader 与 Content 正文节点的分组测量与排列。
- `src/AtomUI.Desktop.Controls/Steps/StepsPanelArrow.cs`：internal 可拉伸 Panel 楔形箭头绘制控件，负责 LTR/RTL 三角形和边框。
- `src/AtomUI.Desktop.Controls/Steps/StepsPanelItemFrame.cs`：Panel item 的视觉外框；Filled 非首项使用左侧 notch 几何裁剪，Outlined 保持完整边框。
- `src/AtomUI.Desktop.Controls/Steps/StepsToken.cs`：Steps 控件 Token。
- `src/AtomUI.Desktop.Controls/Steps/Themes/StepsTheme.axaml`：根模板和 StepsPanel。
- `src/AtomUI.Desktop.Controls/Steps/Themes/StepsItemTheme.axaml`：统一 item 语义模板和状态样式。
- `src/AtomUI.Desktop.Controls/Steps/Themes/StepsItemIndicatorTheme.axaml`：统一 Indicator、Dot、Icon、Progress 和 Wave 模板。

测试目录：

- `tests/AtomUI.Desktop.Controls.Tests/Steps`：状态、Items、交互、Wave、Progress、布局、生命周期和 Semantic Part 回归测试。

Gallery 目录：

- `controlgallery/AtomUIGallery/ShowCases/Navigation/Steps`：示例、源码片段和本地化资源。

## 3. 核心类职责

### 3.1 Steps

`Steps : ItemsControl` 是状态协调 owner：

- 保存根 public 输入。
- 保存 ItemHeaderForeground、ItemSubHeaderForeground 和 ItemRailBackground，并投影到全部已实现 item 容器。
- 创建和清理 `StepsItem` 容器。
- 根据 index 计算 StepNumber 和 AutomaticStatus。
- 维护每个 item 指向下一个 item 的 ConnectorStatus。
- 接收 item 激活并发出 `CurrentChangeRequested`。
- 不保存第二套当前步骤状态、根级页面内容投影或视觉缓存集合。

### 3.2 StepsItem

`StepsItem : HeaderedContentControl` 是单项 public 容器：

- 承载 Header、SubHeader、Content、Icon 和 nullable Status。
- 通过 internal StyledProperty 保存 owner 投影的 Header、SubHeader 和 Connector 语义画刷覆盖。
- 保存 StepNumber、IsCurrent、AutomaticStatus、EffectiveStatus、ConnectorStatus、IsFirst、IsLast 和 CanInvoke 的 internal 投影。
- 处理 pointer、keyboard、focus 和 hover。
- 把激活请求交给 owner，不写入根 Current。

### 3.3 StepsItemIndicator

Indicator 是 internal-observable 视觉控件：

- 展示步骤编号、完成图标、错误图标、Dot、OutlineDot 或自定义 Icon。
- 在有效条件成立时绘制 Progress ring。
- 持有当前模板中的 `PART_WaveSpirit`，并只响应 item 的真实 pointer click 调用。
- 不监听 IsCurrent 变化播放 Wave。
- `Type=OutlineDot` 直接抑制 Wave 播放，只保留可点击请求和 hover/transition 视觉。

### 3.4 LayoutPanel

- `StepsPanel` 只排列 StepsItem，不读取 Status、不生成视觉。
- `StepsPanel` 在 `Type=Panel` 时忽略请求的垂直方向，强制水平排列并为所有可见 item 分配等宽单元。
- `StepsPanel.Offset` 只在 Inline 水平等宽布局中保留前置空单元，不影响状态编号。
- `StepsPanel.HorizontalContentAlignment` 只在垂直 Navigation 布局中控制 item 列的水平对齐，默认居中。
- `StepsItemLayoutPanel` 只排列固定语义子节点和正文区域，不读取 Current、不修改 item 属性；Panel 下隐藏 Indicator/Connector 的节点不进入有效几何，ItemWrapper 覆盖完整单元，非首项内容按 `panel-padding + item-base-width` 内缩，PanelArrow 溢出到相邻单元外侧；垂直 item 间距由面板自身测量高度承担，不放进内容区域 padding。
- `StepsItemSectionPanel` 是 item 正文区域（Header、SubHeader、Content）的分组容器：完成三个正文节点的测量与排列，承担 heading 同行/换行决策与垂直标题布局的对齐，向 `StepsItemLayoutPanel` 上报 HeadingHeight 与标题行右缘（HeadingLineRight）。
- `StepsPanelItemFrame` 只承担 Panel item 的形状职责：Filled 非首项按箭头宽度裁出左侧 notch，避免后续 item 的矩形背景盖住前一项箭头；Outlined 不裁剪主体，只由箭头边框覆盖共享接缝。

三个 Panel 都通过 `AffectsMeasure` / `AffectsArrange` 响应相关布局属性，不依赖根控件手工重建 Grid definitions。

## 4. 状态与数据流

### 4.1 根输入

```text
Current
Initial
Status
Percent
Type
Orientation
TitlePlacement
SizeType
IsItemClickable
Offset
HorizontalContentAlignment
IsMotionEnabled
ItemHeaderForeground
ItemSubHeaderForeground
ItemRailBackground
```

状态输入和展示输入分开处理。只有 Current、Initial、根 Status、item Status 和 item index 参与 EffectiveStatus 计算。三个 Item 语义样式属性属于展示输入，不参与 EffectiveStatus、ConnectorStatus、布局或交互计算。

### 4.2 Item 投影

```text
StepNumber = Initial + index

AutomaticStatus =
    StepNumber == Current ? root Status :
    StepNumber < Current  ? Finish :
                            Wait

EffectiveStatus = item Status ?? AutomaticStatus
IsCurrent       = StepNumber == Current
```

每次协调完整写入所有派生值，不保留依赖旧状态的分支。公开 `Status` 从不被根控件覆盖；根控件只写 internal AutomaticStatus。

### 4.3 Connector 投影

item `i` 的 ConnectorStatus 等于 item `i + 1` 的 EffectiveStatus。Connector 表达当前步骤指向下一个步骤的目标状态，最后一个 item 通过 IsLast 隐藏 Connector。

根输入变化时线性刷新全部已实现容器。单个 item Status 变化时刷新自身 EffectiveStatus，并刷新前一个 item 的 ConnectorStatus。

### 4.4 Progress 投影

```text
IsProgressFrameReserved =
    Percent.HasValue
    && Type is Default or Navigation

IsProgressVisible =
    IsProgressFrameReserved
    && IsCurrent
    && EffectiveStatus == Process
    && Icon == null
```

`IsProgressFrameReserved` 负责所有 item 统一预留 Progress 外圈尺寸；`IsProgressVisible` 只负责当前 process item 是否绘制 Progress ring。Icon、Type、Percent、IsCurrent 或 EffectiveStatus 变化都必须重新计算 Progress 投影状态。

### 4.5 数据项路径

- `item is StepsItem`：直接使用，不创建包装容器。
- 普通数据项：创建 StepsItem，把数据项写入 Content，把 Steps.ItemTemplate 写入 ContentTemplate。
- 数据源 ItemTemplate 负责渲染完整文字区域；Indicator 和 Connector 仍由容器主题管理。

### 4.6 语义样式投影

实例级 item 语义样式沿固定边界单向投影：

```text
Steps public nullable semantic brush
    -> StepsItem internal StyledProperty
    -> StepsItemTheme
    -> StepsItem 自身模板中的对应语义节点
```

`Steps` 是三项公开输入的唯一 owner。容器准备时为直接 `StepsItem` 和普通数据项生成的 `StepsItem` 建立相同的普通 Avalonia binding；运行时属性变化由既有 binding 更新全部已实现容器，不重新准备容器、不重建模板，也不触发状态重算；容器清理时释放 owner 投影，避免直接 item 或回收容器保留上一个 owner 的值。

internal StyledProperty 只作为根控件与 item 自有主题之间的强类型传递通道，不是新的 public item API。`StepsItemTheme.axaml` 仅在值非 `null` 时覆盖 Header、SubHeader 或 Connector 的 Token 结果；清空公开属性后必须恢复当前 Type 和 EffectiveStatus 的完整 Token 视觉。

## 5. 组合结构模型

### 5.1 控件角色图

```text
Steps (public)
└── ItemsPresenter#PART_ItemsPresenter (template-stable)
    └── StepsPanel (internal-observable)
        └── StepsItem (public container)
            └── StepsItemLayoutPanel (internal-observable)
                ├── StepsPanelItemFrame#ItemWrapper (internal-observable)
                ├── StepsItemIndicator#PART_Indicator (template-stable)
                │   └── WaveSpiritDecorator#PART_WaveSpirit (internal-observable)
                ├── StepsItemSectionPanel#Section (internal-observable)
                │   ├── ContentPresenter#HeaderPresenter (internal-observable)
                │   ├── ContentPresenter#SubHeaderPresenter (internal-observable)
                │   └── ContentPresenter#ContentPresenter (internal-observable)
                ├── PixelAlignedBorder#Connector (internal-observable)
                ├── PathIcon#NavigationArrow (internal-observable)
                ├── StepsPanelArrow#PanelArrow (internal-observable)
                └── PixelAlignedBorder#NavigationActiveIndicator (internal-observable)
```

### 5.2 协作节点

| 节点 | 类型 | 来源 | 生命周期 owner | 影响的 public API | 稳定性 | Agent 使用边界 |
| --- | --- | --- | --- | --- | --- | --- |
| `Steps` | public control | `Steps.cs` / `StepsTheme.axaml` | application visual tree | 全部根 API | public | 用户可直接使用。 |
| `StepsItem` | public container | `StepsItem.cs` / `StepsItemTheme.axaml` | Steps container lifecycle | item API | public | 用户可直接声明。 |
| `StepsPanel` | layout panel | `StepsTheme.axaml` | ItemsPresenter | Type、Orientation | internal-observable | 只用于理解布局，不作为用户 API。 |
| `StepsItemLayoutPanel` | layout panel | `StepsItemTheme.axaml` | StepsItem template | Type、Orientation、TitlePlacement | internal-observable | 不直接依赖或替换。 |
| `StepsItemSectionPanel` | layout panel | `StepsItemTheme.axaml` | StepsItem template | Type、Orientation、TitlePlacement、Header、SubHeader、Content | internal-observable | 不直接依赖或替换。 |
| `PART_Indicator` | indicator | `StepsItemTheme.axaml` | StepsItem template | Icon、Status、Percent、Wave | template-stable | 自定义主题必须保留。 |
| `PART_WaveSpirit` | wave decorator | `StepsItemIndicatorTheme.axaml` | Indicator template | IsMotionEnabled、pointer click | internal-observable | 不由用户直接调用。 |
| `Connector` | border | `StepsItemTheme.axaml` | StepsItem template | ConnectorStatus、Type | internal-observable | ConnectorStatus 来自下一个 item EffectiveStatus。 |
| `NavigationArrow` | path icon | `StepsItemTheme.axaml` | StepsItem template | Type、Orientation、SizeType | internal-observable | 只在 Navigation 类型可见。 |
| `ItemWrapper` | Panel item frame | `StepsItemTheme.axaml` / `StepsPanelItemFrame.cs` | StepsItem template | Type、PanelVariant、IsFirst、SizeType | internal-observable | Filled notch 只在 Panel 类型非首项裁出；Inline 时为 antd v6 `-item-wrapper` 同构的 hover 背景（铺满条目格宽，块方向 padding 来自 `InlineItemPadding` token 绑定的 layout panel `Padding`：上 `paddingXS+lineWidth`、下 0，`borderRadiusSM`）；Outlined 保留边框主体。 |
| `PanelArrow` | internal control | `StepsItemTheme.axaml` / `StepsPanelArrow.cs` | StepsItem template | Type、PanelVariant、SizeType、FlowDirection | internal-observable | 只在 Panel 非末项可见。 |
| `NavigationActiveIndicator` | border | `StepsItemTheme.axaml` | StepsItem template | Type、Orientation、IsCurrent | internal-observable | 只在 Navigation 当前项可见。 |

## 6. 生命周期与模板接入

### 6.1 容器准备

准备容器时：

1. 设置 `Owner` 和当前 index。
2. 计算 StepNumber、IsFirst、IsLast、AutomaticStatus、EffectiveStatus 和 IsCurrent。
3. 按下一个 item 的 EffectiveStatus 计算当前 item 的 ConnectorStatus。
4. 普通数据项接入 Content / ContentTemplate。
5. 建立三项 item 语义样式从 owner 到容器 internal StyledProperty 的投影。

初始实现每个容器只执行 O(1) 初始化，不在每次 ContainerPrepared 时遍历全部 Items。

### 6.2 Index 和集合变化

- Add/Remove/Move：重新编号受影响区间并更新边界 Connector。
- Replace：清理旧容器后准备新容器。
- Reset：按当前 public 输入完整重建已实现容器投影。
- Current、Initial 或根 Status 变化：O(n) 刷新已实现容器的状态投影。
- 任一 item 语义样式变化：由容器已有 binding 更新对应 internal StyledProperty，不重算状态、不重新准备容器或重建模板。

### 6.3 容器清理

清理容器时必须：

- `Owner = null`。
- `ItemIndex = -1`。
- 清除 owner 写入的 internal 派生值和数据项 Content 映射。
- 释放并清空 owner 写入的 Header、SubHeader 和 Connector 语义样式投影。
- 让 Indicator visual detach；WaveSpiritDecorator 在 detach 中取消自身 animation owner。

外部继续持有被移除 StepsItem 时，它不能保留旧 Steps owner。

### 6.4 模板接入

`Steps` 不重写 `OnApplyTemplate()`；根模板通过 `ItemsPresenter.ItemsPanel` 直接接入 `StepsPanel`，模板生命周期不修改 Current、Initial 或 item 状态。

`StepsItem.OnApplyTemplate()` 和 `StepsItemIndicator.OnApplyTemplate()` 在获取新 part 前先清空旧引用。AXAML Ancestor Binding 的建立和释放由 Avalonia template 生命周期管理。

`StepsTheme.axaml`、Gallery 和外部应用不得通过 `/template/` selector 访问 `HeaderPresenter`、`SubHeaderPresenter`、`Connector` 或其他 `StepsItem` 内部节点。`StepsItemTheme.axaml` 进入 `StepsItem` 自身模板并消费 internal StyledProperty 属于控件自身的合法主题边界。

不创建根级步骤页面内容 observable、长期 Relay Binding、全局事件订阅或 detach 后仍存活的 CompositeDisposable。

### 6.5 Semantic marker 接入点

- `Steps` 的 `item` descriptor 为运行时标记（`RuntimeCreated = true`），路由 `> .semantic-item`。
  `Steps.PrepareContainerForItemOverride` 在容器准备时对每个 `StepsItem` 写入 `semantic-item` marker，
  直接声明的 `StepsItem`、普通数据项生成的容器以及回收复用后重新准备的容器遵循同一规则；容器移除后
  marker 随容器离开 owner 范围。
- 七个 item 子 Part（`itemWrapper`、`itemIcon`、`itemTitle`、`itemSubtitle`、`itemSection`、`itemContent`、
  `itemRail`）是 `StepsItemTheme.axaml` 内的静态 marker，分别声明在 `ItemWrapper`（`StepsPanelItemFrame`）、
  `PART_Indicator`（`StepsItemIndicator`）、`HeaderPresenter`、`SubHeaderPresenter`、`Section`
  （`StepsItemSectionPanel`）、`ContentPresenter` 与 `Connector`（`PixelAlignedBorder`）上。子 Part 路由为
  `> .semantic-item /template/ .semantic-item-x`：`StepsItem` 是 ItemsControl 的逻辑子节点而非 owner 的
  模板子节点，路由必须先经逻辑 `>` 到达容器，再经 `/template/` 进入 item 模板；应用不手写该路径，由生成的
  子 Part Style 封装。
- `StepsTheme.axaml` 根模板在 `PART_ItemsPresenter` 外包裹 TemplateBind 根视觉属性的 `atom:DashedBorder`：
  `Background` / `BackgroundSizing` / `BorderBrush` / `BorderThickness` / `CornerRadius` / `Padding`
  来自 `TemplatedControl`，`StrokeDashArray` / `StrokeDaskOffset` 来自 `Steps` 新增的 `BorderDashArray` /
  `BorderDashOffset`，使 root Part 的边框（含虚线）、背景与内边距定制可渲染；默认值不改变既有外观。
- `itemIcon` 的默认全圆不再由 `StepsItemIndicator.OnSizeChanged` 以 local value 写入（local value
  优先级会阻止 Semantic Style setter）；`StepsItemIndicatorTheme.axaml` 以 style 优先级设置 CornerRadius
  类型 Token `IconContainerCornerRadius`（`new CornerRadius(IconContainerSize)`），Semantic Style 的
  Setter 以更高优先级直接覆盖。

## 7. 交互与事件处理

### 7.1 Pointer

- 整个 StepsItem 是命中区域。
- 同一 item 内完成 press/release 才形成 click。
- 移出释放、失去 capture 或取消不触发。
- Clickable、root enabled 和 item enabled 共同决定 CanInvoke。

pointer click 首先调用 Indicator.PlayWave，再在目标 StepNumber 不等于 Current 时发出 CurrentChangeRequested。事件处理器是否更新 Current 不影响本次 click Wave。
`Type=OutlineDot` 仍走同一激活路径，但 Indicator 会拒绝播放 Wave，因此请求语义和动画语义保持解耦。

### 7.2 Keyboard

- CanInvoke item 可获得焦点。
- Enter/Space 在目标不是当前 item 时发出 CurrentChangeRequested。
- keyboard 路径不调用 Indicator.PlayWave。
- Space 被标记 handled，避免宿主滚动。

### 7.3 Focus、Hover 和 Disabled

CanInvoke=false 时不进入 Tab 焦点序列，不显示 hand cursor 和 clickable hover 视觉。视觉使用标准 focus-visible、pointerover 和 disabled 伪类，不创建 Selection 伪类。

## 8. 内部算法与关键流程

### 8.1 根状态刷新

根状态刷新是确定性 O(n) 投影。它不检查 IsLoaded、VisualTree attachment 或 template part 是否存在，也不使用 suppression flag、Dispatcher 延迟或强制刷新。

### 8.2 布局算法

`StepsPanel` 按有效方向选择测量路径；Panel 即使请求垂直方向也使用水平测量。水平测量分两遍：第一遍以无限宽度测量可见 item，得到自然宽度作为 flex basis；随后计算整组自然宽度，并以它与可用宽度的较小值分配第二遍测量份额，使受限文本换行并上报完整行高。

整组自然宽度遵循实际排列几何：普通水平标题布局为「最大非末项宽度 × 非末项数量 + 末项宽度」；Navigation、Panel 和垂直标题布局为「最大 item 宽度 × 单元数量」；Inline 的单元数量额外包含非负 `Offset`。除 Panel 外，单元宽度保留 `MinItemWidth` 下限。隐藏 item 不参与自然宽度，空集合返回零尺寸。

`DesiredSize` 只包含所需单元和 Inline 前导单元，不包含父容器提供的额外伸展空间。Arrange 仍以实际 `finalSize.Width` 使用共享份额算法；缓存只在最终宽度等于第二遍测量宽度、可见 item 数量相同时复用，否则根据本轮自然 basis 重新分配。这样自然尺寸宿主不会被 Steps 撑满，固定宽度宿主仍能伸展；受限布局继续保留压缩与换行（份额模型见 overview.md 8.7）。

水平份额规则：

- Horizontal Default + horizontal title：宽容器时非末 item 等额伸展、末 item 保持内容宽度（既有伸展语义不变）；容器不足时所有 item 按自然宽度比例连续收缩（flex-shrink 语义），收缩下限为 `MinItemWidth`，触底的 item 冻结并退出分配，其余 item 继续分摊剩余缺口。容器比 item 数量 × 下限还窄时，item 停在下限并溢出容器。
- Horizontal Dot / OutlineDot / vertical title / Inline：item 等宽，indicator 居中，rail 从当前 indicator 指向下一项；Inline 的 dot 上方 rail 连通，content 不参与显示，`Offset` 在可见 item 前方保留同等数量的空 item 单元。
- Horizontal Navigation：item 等宽。
- Panel：强制水平 item 等宽；Indicator 和 Connector 隐藏，PanelArrow 在非末项外侧绘制可拉伸楔形。
- 单 item：取内容宽度与可用宽度的较小值，且不低于 `MinItemWidth`。
- Inline：按 inline + dot + vertical-title 组合排列，item 等宽，dot 上方 rail 连通，content 不参与显示；`Offset` 会在可见 item 前方保留同等数量的空 item 单元。
- Vertical：按 DesiredSize 顺序堆叠。

`MinItemWidth` 由主题绑定 `IconContainerSize` token，作为水平 item 的收缩下限。

水平可见 Connector 的自然尺寸必须包含线段本身与 Margin。`StepsItemTheme` 通过 style setter 将现有 `SharedToken.UniformlyMargin` 提供给 internal `StepsItemLayoutPanel.PreferredHorizontalConnectorLength`。水平标题布局把该长度计入标题后方的 Connector extent；垂直标题布局同时预留 indicator 宽度、两侧 `HorizontalConnectorGap` 和线段长度。隐藏 Connector（含末项、Navigation、Panel）不预留线段。该值仅影响自然测量，不设置 Connector 的 `MinWidth`，受限排列仍可将线段压缩至零；资源变化通过面板的 measure invalidation 重新计算自然尺寸。

`StepsItemLayoutPanel` 的水平标题路径以相同宽度测量和排列 body：测量时先把 indicator 按完整 item 宽度测量，再把正文区域 `StepsItemSectionPanel` 按「item 宽度 − indicator − spacing」测量，section 内部以同一宽度完成 Header / SubHeader / Content 的测量与排列，与排列时的 body 可用宽度一致。否则当份额落在文本自然宽度的邻近区间（份额 ≥ 文本自然宽度、但份额 − icon 区 < 文本自然宽度）时，文本会被测成单行、排列时再被压成更窄的一行——以裁剪代替换行。

heading 行（Header 与 SubHeader 并排）的同行/换行决策由 `StepsItemSectionPanel` 在测量与排列中共享：测量阶段以 body 可用宽度测量 Header 与 SubHeader，若 `Header 宽度 + SubHeader 宽度 > body 宽度`，heading 高度取 `Header 高度 + SubHeader 高度`，body 宽度取三者宽度的最大值；排列阶段用同一条件判定，放不下时 SubHeader 换到 Header 下方独占一行并按测量宽度排列，而不是被安排成 `body 宽度 − Header 宽度` 的剩余宽度而裁成「00:0」。heading 高度随之增高，内容行整体下移，indicator 与 heading 块垂直居中。垂直标题布局中 section 先由 `StepsItemLayoutPanel` 按有效标题布局定位于 indicator 下方或右侧，再由自身完成逐行居中或贴边排列。

`StepsItemLayoutPanel` 根据 Type、Orientation 和 EffectiveTitlePlacement 排列固定语义节点与正文区域，正文区域内部的标题行/内容行分组排列由 `StepsItemSectionPanel` 完成，两者通过共享的 `ResolveTitlePlacement` 解析同一有效标题布局。Connector 的方向和伸展范围由布局 Panel 决定，状态由 item 投影决定；item 压缩到标题行宽度以下时 rail 收缩为零宽。

### 8.3 Indicator 和 Progress

Indicator 使用单一模板切换 number、finish mark、error mark、实心 dot、空心 dot 和 custom icon。Progress ring 在 Indicator.Render 中绘制。
`OutlineDot` 复用 Dot 的 `DotSize`、`DotCurrentSize`、`DotLineThickness` 和状态 Dot 色；主题只把状态色投射到 `BorderBrush`，并保持 `Background=Transparent`。

Progress 外径由 `IconSize` / `IconSizeSM` 与 `ProgressFramePadding` / `ProgressFramePaddingSM` 推导，不保存重复的固定 Progress size。有 `Percent` 时，支持 Progress 的 item 统一预留外圈尺寸，只在当前 process item 上绘制 groove 和 arc。

Percent coercion：

```text
null             -> null
NaN / Infinity   -> null
value < 0        -> 0
value > 100      -> 100
其他             -> value
```

所有 Render 输入进入 AffectsRender；只有影响尺寸的属性进入 AffectsMeasure。Progress Pen 使用可复用实例，避免 render 热路径分配。

## 9. 资源、性能与 AOT 边界

资源边界：

- TokenResource 和 SharedToken 只提供视觉值，不保存实例状态。
- 根展示属性使用 AXAML Ancestor Binding 投影到 item 和 internal panel。
- 固定模板关系使用 TemplateBinding、Ancestor Binding 和 selector，不使用字符串路径反射。
- item 语义样式使用静态注册的 StyledProperty 和普通 Avalonia binding 投影，不生成运行时 selector，也不依赖内部节点名称从控件外穿透模板。

性能边界：

- 单容器准备 O(1)。
- 单 item Status 变化 O(1)。
- 根状态变化和 Reset O(n)。
- 不维护第二份 item 列表、状态字典或延迟更新队列。
- 三个 Panel 在 Measure/Arrange 中不创建视觉，不修改 public 状态。

AOT 边界：

- 不新增运行时反射扫描、动态类型注册或编译期不可分析的 binding 路径。
- 不通过反射访问 Wave 播放状态；测试使用 internal 可观察入口或渲染结果。
- 新 internal panel 由静态 AXAML 和显式类型引用创建。
- 三项语义样式投影不使用反射、动态属性发现或运行时类型扫描。

## 10. 维护不变量

- Current 是唯一当前步骤输入；不得引入第二套选择状态或双向同步。
- 每次状态协调必须完整覆盖派生状态，不依赖旧值。
- item public Status 不被根控件写入或覆盖。
- EffectiveStatus 是所有状态视觉的唯一输入。
- Connector 使用下一个 item EffectiveStatus。
- 垂直 Steps 的 item 间距属于 item 内部测量空间，最后一个 item 必须清零；不得用 Content padding 或 StepsPanel 外部 spacing 代替。
- Initial 不在 OnApplyTemplate 或 attach 中写入 Current。
- Offset 不参与状态编号、Current 归一或 item 状态计算；它只改变 Inline 布局前置占位。
- 根级步骤页面内容投影和内容订阅不得重新引入。
- pointer click 是 Wave 的唯一触发源；Current 变化不能播放 Wave。
- OutlineDot click 不能播放 Wave；该例外必须在 Indicator 层兜住，避免 pointer、keyboard 或未来激活入口绕过。
- 每个主题只维护一套语义模板。
- 外部代码不得通过深层 selector 修改 StepsItem 内部节点；实例级 Header、SubHeader 和 Connector 定制由根控件三项 nullable 语义 API 进入。
- 语义样式的 `null` 值必须完整回退 Token；容器清理和重新准备不得残留旧 owner 的显式值。
- Semantic Part descriptor、`semantic-item` 运行时 marker 与七个静态 `semantic-item-*` marker 的同步规则、`> .semantic-item /template/ .semantic-item-x` 容器边界路由形状以及生成的 Steps*Style 类型保持稳定；`itemIcon` 默认圆角只能由主题 style 优先级提供，代码不得再以 local value 写入。
- StepsPanel、StepsItemLayoutPanel 和 StepsItemSectionPanel 只负责布局。
- 水平布局的 item 收缩与文本换行遵循 overview.md 8.7 弹性模型的份额算法与 `IconContainerSize` 收缩下限；测量与排列共用份额算法，自然尺寸不包含父容器的富余空间；受限布局必须保留完整换行高度，任何路径不得以裁剪代替换行。
- 水平标题 heading 行的同行/换行决策由测量与排列共用同一判定条件；Header 与 SubHeader 并排放不下时，SubHeader 必须换到 Header 下方独占一行并保持测量宽度，不得裁成剩余宽度。
- 水平标题路径的 body 子项（Header / SubHeader / Content）必须按排列时的 body 可用宽度（item 宽度 − indicator − spacing）测量，不得按完整 item 宽度测量；否则份额落在文本自然宽度的邻近区间时会以裁剪代替换行。
- 宽容器的既有伸展语义（非末 item 等额伸展、末 item 内容宽、单 item 内容宽）不得随压缩能力回归。
- 容器清理必须释放 Owner，模板重套必须释放旧 part 引用。
- Percent、Icon、Type 和 EffectiveStatus 运行时变化必须立即更新 Progress。

## 11. 测试与验证

状态：

- Current 正向、反向、重复、低于 Initial 和高于所有 item。
- Initial 非零以及 Indicator 编号。
- 根 Status 运行时变化。
- item nullable Status 的 Wait、Process、Finish、Error 覆盖。
- Add、Remove、Replace、Move、Reset。
- 属性在加载前、加载后和模板重套后设置。

交互与 Wave：

- Indicator、Header、SubHeader 和 Content click。
- 非当前 item click 产生 Wave 和 request。
- 当前 item click 只有 Wave。
- OutlineDot click 只产生 request，不产生 Wave。
- 程序化 Current 不产生 Wave。
- pointer cancel、root/item disabled、不可点击和 motion disabled。
- Enter/Space 产生 request 但不产生 Wave。

Progress：

- null、0、100、越界、NaN 和 Infinity。
- 四种 EffectiveStatus、自定义 Icon、Dot、OutlineDot 和 Inline。
- Render 输入变化触发 InvalidateVisual。

布局：

- `Type x Orientation x TitlePlacement x SizeType`。
- Inline Offset 前置占位。
- 语义节点唯一、Bounds 有效、无重叠、Connector 正确。
- 运行时布局切换和动态 Items。
- 弹性压缩：窄容器 item 按自然宽度比例收缩、item 总宽填满容器、`MinItemWidth` 收缩下限、极窄容器触底溢出。
- 文本换行：压缩后标题、副标题与描述文本在受限宽度下换行，不以裁剪代替换行。
- 宽容器伸展回归：非末 item 等额伸展、末 item 与单 item 内容宽不变；自然尺寸请求不随宿主富余宽度增长。

生命周期：

- 移除 item 后 Owner 释放。
- 模板重套不改变 Current。
- 旧 Indicator/Wave part 不被保留。
- detach/reattach 和数据容器回收不保留旧状态。

语义样式：

- 三项公开属性默认均为 `null`，默认渲染与现有状态、类型和 Inline Token 完全一致。
- 分别设置 Header、SubHeader 和 Connector 覆盖时只影响对应语义节点。
- 运行时修改和清空属性立即更新已实现容器，并在清空后恢复当前 Token 视觉。
- 直接 StepsItem、普通数据项生成容器、移除后复用和数据容器回收遵循同一投影与清理规则。
- Gallery 不包含针对 StepsItem 内部节点的 `/template/` selector。

Semantic Part：

- descriptor 排序、路由、ContractType、Cardinality 与 registry 反向查询。
- `StepsItemTheme.axaml` 六个静态 marker 与 `StepsTheme.axaml` 语义约束。
- 运行时 `semantic-item` marker 覆盖直接 item、数据生成容器与回收复用。
- 生成的 `StepsItemIconStyle` 等 Style 经路由应用，并覆盖 `IconContainerCornerRadius` 默认全圆。
- 根 `BorderDashArray` / `BorderDashOffset` 默认中性，并 TemplateBind 传播到根模板 `atom:DashedBorder`。
- Gallery 语义预览、样式示例与四语言本地化文案。

收尾运行 `StepsSemanticPartTests` / `StepsRootFrameTests` 与 Steps 定向测试、相邻 Desktop Controls 测试、Gallery 测试（含 `StepsShowCasePageTests`）和 `git diff --check`。
