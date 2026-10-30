# Slider Semantic Part 契约

本文档定义 `AtomUI.Desktop.Controls.Slider` 对应用公开的 Semantic Part、Selector、类型约束、数量语义和定制边界。
Slider 的整体设计见 [Slider 桌面版架构设计](overview.md)，多 handle 专项模型见 [Slider 多 Handle 设计](multi-handle-design.md)，
descriptor 与真实模板/运行时节点映射见 [Slider 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Owner 边界

Slider 是单一 owner：`Slider` 公开 `root`、`rail`、`tracks`、`track`、`handle` 五个 Part，与上游稳定 Semantic DOM
一一对应。上游基线为 6.6.0 稳定发布的 `SliderSemanticType`（`classNames` / `styles` 均为
`{ root?, tracks?, track?, rail?, handle? }`）。该类型在 `Slider/index.tsx` 中经 `useMergeSemantic` 合并
`ConfigProvider` 与组件级语义值后传给 `@rc-component/slider`（antd 6.6.1 依赖 `~1.1.1`），由 rc-slider 实际消费：

- `root` 消费于根节点 `.ant-slider`（`className` / `style` 与 `rootClassName` 合并；带 `-disabled` / `-vertical` /
  `-horizontal` / `-with-marks` 变体）。
- `rail` 消费于直接子节点 `.ant-slider-rail`（背景轨道）。
- `tracks` 消费于 `.ant-slider-tracks`：整体活动范围容器，从首个值到末值（单值模式从 `startPoint ?? min` 到当前
  值）。上游是条件节点——仅当应用提供 `classNames.tracks` 或 `styles.tracks` 时才渲染该 wrapper，各 `track` segment
  始终与其并列渲染。
- `track` 消费于每个 `.ant-slider-track` segment（单值模式 1 个；Range 模式每对相邻值 1 个，带 `-1` / `-2` 索引
  变体与 `-draggable` 变体）。
- `handle` 消费于每个 `.ant-slider-handle`（单值模式 1 个；Range 模式每值 1 个，带 `-1` / `-2` 索引与 `-dragging` /
  `-dragging-delete` / `-disabled` 变体；上游可见圆点为 `::after` box-shadow 实现）。

上游 `SliderSemanticType` 只有上述五个键，不包含 mark 点 / 标签（`.ant-slider-dot` / `.ant-slider-mark`）、steps、
Tooltip 与 Form 校验。AtomUI 不扩展额外 Part：mark、Tooltip 均不属于 Semantic Part。

AtomUI 映射与结构改造（internal、public API 不变）：

- `root` → `Slider` owner。
- `rail` / `tracks` / `track` → `SliderTrack` 内新增的代码创建 `Border` 元素。改造前这三个区域是 `SliderTrack.Render`
  的自绘几何（`DrawGroove` / `DrawTrackBars`），没有 Visual 节点；本次改造把三者元素化（几何与画刷不变），使每个
  Part 都是可被 Selector 命中的稳定节点。
- `handle` → `SliderTrack` 动态创建的公开 `SliderThumb` 节点（已有元素，无需结构改造）。
- mark 点 / 标签移入 internal `SliderMarksElement` 子元素（非 Part，仅承载原自绘代码），保证元素化后
  rail → tracks → track → mark → thumb 的视觉层级与现状一致。

以下类型不持有独立 Semantic descriptor：

- `SliderTrack` 是 internal 协作模板部件（`PART_Track`），承载几何、交互与动态节点，不发布 Part。
- `SliderThumb` 是公开类型，但其公开语义通过 `Slider` 的 `handle` Part 表达，与上游单一 `Slider` owner 一致，
  不单独声明 descriptor。
- `SliderMarksElement`、`SliderRangeMath`、automation peers 是内部协作类型。

与上游 DOM 的三处结构差异已确认并接受：

1. 上游 `tracks` 是「仅被定制时才渲染」的条件节点；AtomUI 恒渲染该元素（默认画刷为 null 时透明、无视觉差异），
   使 Part 拥有稳定 marker，语义样式在单值与 Range 两种模式下都可用。上游单值模式下 `tracks` 覆盖
   `min → 当前值` 跨度，AtomUI 单值模式的 `tracks` 元素同样覆盖 `Minimum → Value`（此前 `TracksBrush` 在单值模式
   不绘制，本次随元素化对齐上游，仅影响显式设置 `TracksBrush` 的单值场景）。
2. 上游 DOM 顺序为 rail → tracks → track → steps → handle → mark（mark 文本位于 handle 之上）；AtomUI 保持现状
   mark 位于 thumb 之下（元素化后子元素顺序 rail → tracks → track → mark → thumb），默认视觉不变。
3. 上游 handle 圆点由 `::after` box-shadow 表达；AtomUI 圆点由 `SliderThumb` 自绘（`Background` + `BorderBrush` +
   `OutlineBrush`），`handle` 的 Semantic Setter 作用于 `SliderThumb` 的公开视觉属性。

所有 Part 的 `Since` 统一为 `6.2.0`（上游版本徽标 root 为 5.23.0、其余为 5.10.0，AtomUI 使用自身版本基线）。

## 2. Semantic Parts

### 2.1 `root`

| 字段 | 值 |
| --- | --- |
| Owner | `Slider` |
| Part | `root` |
| Selector | Slider 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `Slider` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | Slider owner |
| 职责 | 数值选择控件根：承载 `Minimum` / `Maximum` / `Value` / `RangeValues` 值状态、方向、轨道与 mark 配置、键盘与 pointer 交互会话、Tooltip 与 Form 集成；作为全部 Part 的 owner-scoped Selector 作用域边界。对应上游 `.ant-slider`。 |
| 相关 API | `Orientation`、`IsDirectionReversed`、`IsSnapToTickEnabled`、`TickFrequency`、`IsRangeMode`、`RangeValues`、`DisabledHandles`、`IsDraggableTrack`、`TrackBarBrush`、`TracksBrush`、`Marks`、`IsIncluded`、`ValueFormatTemplate`、`IsMotionEnabled` |
| 相关 Token | `SliderPaddingHorizontal`、`SliderPaddingVertical`、SharedToken |
| 稳定性 | stable since 6.2.0 |

### 2.2 `rail`

| 字段 | 值 |
| --- | --- |
| Owner | `Slider` |
| Part | `rail` |
| Selector | `.semantic-rail` |
| SelectorRoute | `/template/ .semantic-rail` |
| Style Type | `SliderRailStyle` |
| ContractType | `Border` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `SliderTrack` 内代码创建的 rail `Border` 元素（几何 = `GetRailRect`，胶囊圆角，背景 = `TrackGrooveBrush`） |
| 职责 | 统一表示背景轨道区域：rail 画刷、胶囊圆角与过渡；对应上游 `.ant-slider-rail`。 |
| 相关 API | `TrackGrooveBrush`（SliderTrack）、`IsEnabled`、`Orientation` |
| 相关 Token | `RailBg`、`RailHoverBg`、`RailSize` |
| 稳定性 | stable since 6.2.0 |

### 2.3 `tracks`

| 字段 | 值 |
| --- | --- |
| Owner | `Slider` |
| Part | `tracks` |
| Selector | `.semantic-tracks` |
| SelectorRoute | `/template/ .semantic-tracks` |
| Style Type | `SliderTracksStyle` |
| ContractType | `Border` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `SliderTrack` 内代码创建的整体活动范围 `Border` 元素（Range 模式首值→末值；单值模式 `Minimum → Value`；背景 = `TracksBrush`） |
| 职责 | 统一表示整体活动范围容器：Range 模式覆盖首尾 handle 之间的整段跨度，单值模式覆盖最小到当前值的跨度；对应上游 `.ant-slider-tracks`。 |
| 相关 API | `TracksBrush`、`IsRangeMode`、`IsIncluded`、`IsDraggableTrack` |
| 相关 Token | 无专属 Token（默认画刷为 null） |
| 稳定性 | stable since 6.2.0 |

### 2.4 `track`

| 字段 | 值 |
| --- | --- |
| Owner | `Slider` |
| Part | `track` |
| Selector | `.semantic-track` |
| SelectorRoute | `/template/ .semantic-track` |
| Style Type | `SliderTrackStyle` |
| ContractType | `Border` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `SliderTrack` 内代码创建的活动 segment `Border` 元素（单值模式 1 个：`Minimum → Value`；Range 模式每对相邻值 1 个；背景 = `TrackBarBrush`） |
| 职责 | 统一表示相邻 handle 之间的活动轨道段：segment 画刷、胶囊圆角与过渡；对应上游 `.ant-slider-track`。 |
| 相关 API | `TrackBarBrush`、`RangeValues`、`IsIncluded`、`IsDraggableTrack` |
| 相关 Token | `TrackBg`、`TrackHoverBg`、`TrackBgDisabled`、`SliderTrackSize` |
| 稳定性 | stable since 6.2.0 |

### 2.5 `handle`

| 字段 | 值 |
| --- | --- |
| Owner | `Slider` |
| Part | `handle` |
| Selector | `.semantic-handle` |
| SelectorRoute | `/template/ .semantic-handle` |
| Style Type | `SliderHandleStyle` |
| ContractType | `SliderThumb` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | 每个动态 `SliderThumb` 节点 |
| 职责 | 统一表示滑块控制点：圆点背景、边框、outline、hover / focus / pressed / disabled 视觉与 Tooltip 宿主；对应上游 `.ant-slider-handle`。 |
| 相关 API | `SliderThumb.OutlineBrush`、`SliderThumb.OutlineThickness`、`SliderThumb.ThumbCircleSize`、`DisabledHandles`、`ValueFormatTemplate` |
| 相关 Token | `ThumbSize`、`ThumbCircleSize`、`ThumbCircleSizeHover`、`ThumbCircleBorderColor`、`ThumbCircleBorderActiveColor`、`ThumbCircleBorderColorDisabled`、`ThumbCircleBorderThickness`、`ThumbCircleBorderThicknessHover`、`ThumbOutlineColor`、`ThumbOutlineThickness` |
| 稳定性 | stable since 6.2.0 |

### 2.6 marker 放置与路由

`root` 是隐式 Part，不声明 `.semantic-root` marker。非 root Part 全部 `RuntimeCreated=true`，marker 在节点创建时用
生成常量一次性添加：

- `rail`、`tracks` 元素在 `SliderTrack` attach 时创建一次，恒存在；`track` segment 元素随 `EffectiveRangeValues`
  数量同步（单值 1 个、Range N-1 个），数量变化时增删节点。
- `handle` marker 在 `SliderTrack.AddThumb` 时添加；thumb 数量随 handle 数量同步，回收 / 重建路径 marker 随实例。
- 三个主题文件（`SliderTheme.axaml` / `SliderTrackTheme.axaml` / `SliderThumbTheme.axaml`）不声明任何静态
  `.semantic-*` marker；所有 marker 均为运行时创建。
- mark 点 / 标签（`SliderMarksElement`）不携带任何 semantic marker。

路由：四个运行时 Part 的节点都由 `SliderTrack` 创建并设置外层 `Slider` 为 TemplatedParent（与既有
`SliderThumb` 一致），因此从 `Slider` owner 出发均为单跳 `/template/ .semantic-*`。生成器对
`RuntimeCreated=true` 的 Part 只要求显式 `SelectorRoute`，不按 owner 主题资产做静态校验。

## 3. Part 说明

### 3.1 root

`Slider.root` 是 Slider owner 本身，每个 Slider 实例恰好一个。它承载值状态归一（`Value` / `RangeValues` 的
coerce、裁剪与排序）、方向与 tick 语义、交互会话（pointer / keyboard）、Tooltip 同步与 Form 集成；轨道默认视觉由
`SliderTheme` 模板与 token 表达。适合通过 root 定制整体 `Width` / `Height` / `Margin` / `Padding`、`Cursor` 与
字体排版；`IsDirectionReversed`、`IsRangeMode`、`IsIncluded` 等状态通过公开属性与伪类表达。

root 不表示模板中的 `SliderTrack#PART_Track`、任何 rail / tracks / track / mark 元素或动态 thumb；这些节点的名称、
数量与层级由各自的 Part 契约定义。

### 3.2 rail

`rail` 表示背景轨道区域，每个 Slider 恰好一个，恒存在。其几何（`GetRailRect`）、胶囊圆角与背景
（`TrackGrooveBrush`）由 `SliderTrack` 计算与维护；`Orientation`、`IsDirectionReversed`、thumb 尺寸与 padding
只改变几何与画刷，不增删节点。适合定制 `Background`（语义样式可覆盖默认 `RailBg` / hover `RailHoverBg`）与
`Opacity`。几何与胶囊 `CornerRadius` 由控件按布局写入（Style 优先级），用户 Semantic Style 可以覆盖
`CornerRadius`，但覆盖后不再保证胶囊形状。

### 3.3 tracks

`tracks` 表示整体活动范围容器，每个 Slider 恰好一个，恒存在。Range 模式覆盖首值到末值、单值模式覆盖
`Minimum` 到当前值；默认 `TracksBrush` 为 null 时元素透明。`IsIncluded=false` 时元素隐藏但 marker 仍在（与上游
移除节点不同，AtomUI 保持 marker 恒在约定）。适合定制 `Background`、`Opacity`；与 `track` 并列渲染，`track`
位于其上，二者同时设置背景时 `track` 覆盖 `tracks`（与上游 DOM 顺序一致）。

### 3.4 track

`track` 表示相邻 handle 之间的活动轨道段。单值模式恒 1 个（`Minimum → Value`）；Range 模式每对相邻值 1 个
（N 个 handle 产生 N-1 个 segment）。`RangeValues` 快照变化、`IsRangeMode` 切换时元素随数量增删；`IsIncluded=false`
时全部隐藏但 marker 仍在。适合定制 `Background`（默认 `TrackBg` / hover `TrackHoverBg` / disabled
`TrackBgDisabled`）与 `Opacity`。

### 3.5 handle

`handle` 表示滑块控制点，数量与 handle 一致（单值 1 个、Range N 个）。每个 `SliderThumb` 自绘圆点
（`Background` + `BorderBrush`）与 outline 环（`OutlineBrush` + `OutlineThickness`），`hover` / `focus` 放大圆点与
outline、`disabled` 换用禁用色。`DisabledHandles[index]` 只改变对应 thumb 的 `IsEnabled`，不增删节点或 marker。
适合定制 `BorderBrush`、`Background`、`OutlineBrush`、`OutlineThickness`、`ThumbCircleSize`；与上游
`styles.handle` 的 `boxShadow` 对应的是 AtomUI 的 outline 环（`OutlineBrush` / `OutlineThickness`）。

### 3.6 轨道元素与状态的关系

- `IsIncluded=false`：`tracks` 与全部 `track` 元素隐藏（marker 仍在），`rail`、`handle` 不受影响，值计算不变。
- `IsDraggableTrack=true`：只影响整体活动范围拖动入口与 thumb 拖动的值计算，不增删任何 Part 节点。
- `Orientation` / `IsDirectionReversed`：只改变几何与伪类，不增删节点或 marker。
- `DisabledHandles`：只改变对应 thumb 的 `IsEnabled` 与整体拖动可用性，不增删节点或 marker。
- `Marks`：mark 点 / 标签不是 Semantic Part，其增删不影响五个 Part 的数量与 marker。

## 4. Selector 用法

应用级样式先限定 owner，再通过生成的 Semantic Style 进入 Part。生成类型已经封装 owner 类型保护与
`SelectorRoute`，用户不需要复制模板路径：

```xml
<Application.Styles>
    <Style Selector="atom|Slider">
        <atom:SliderRailStyle x:SetterTargetType="Border">
            <Setter Property="Background" Value="#F0F0F0" />
        </atom:SliderRailStyle>
        <atom:SliderTracksStyle x:SetterTargetType="Border">
            <Setter Property="Opacity" Value="0.5" />
        </atom:SliderTracksStyle>
        <atom:SliderTrackStyle x:SetterTargetType="Border">
            <Setter Property="Background" Value="#1677FF" />
        </atom:SliderTrackStyle>
        <atom:SliderHandleStyle x:SetterTargetType="atom:SliderThumb">
            <Setter Property="BorderBrush" Value="#1677FF" />
            <Setter Property="OutlineBrush" Value="#401677FF" />
            <Setter Property="OutlineThickness" Value="3" />
        </atom:SliderHandleStyle>
    </Style>
</Application.Styles>
```

对特定 class 或状态定制时，把 class、属性或伪类放在 owner 一侧：

```xml
<Style Selector="atom|Slider.semantic-custom:pointerover">
    <atom:SliderTrackStyle x:SetterTargetType="Border">
        <Setter Property="Background" Value="#91CAFF" />
    </atom:SliderTrackStyle>
</Style>
```

不得把 `ContractType` 写入 Part Selector。以下写法不属于公共契约：

- `atom|Slider.semantic-rail`、`Border.semantic-rail` 或 `:is(Border).semantic-rail` 等类型变体。
- 直接复制 `/template/ .semantic-*` route 作为用户主路径；route 只属于 descriptor 与生成 Style 的实现元数据。
- 依赖 `PART_Track`、internal 类型（`SliderTrack`、`SliderMarksElement`）、Name 或视觉祖先顺序。

## 5. 状态与数量语义

数量契约以已实例化的内置节点为边界。五个 Part 中 root / rail / tracks 为 `Single`，track / handle 为 `Multiple`；
rail / tracks / track / handle 都是 `RuntimeCreated` Part，marker 随节点实例存在。

| 场景 | root | rail | tracks | track | handle | 说明 |
| --- | --- | --- | --- | --- | --- | --- |
| 单值默认 | 1 | 1 | 1 | 1 | 1 | tracks 默认透明，span 为 `Minimum → Value`。 |
| Range N 个值 | 1 | 1 | 1 | N-1 | N | 每个 segment 相邻值成对，tracks 覆盖首尾。 |
| `IsIncluded=false` | 1 | 1 | 1（隐藏） | N-1（隐藏） | N | 节点与 marker 恒在，只切换可见性。 |
| `RangeValues` 快照增删 | 1 | 1 | 1 | 随 N-1 | 随 N | 节点随数量增删，marker 随实例。 |
| `IsRangeMode` 切换 | 1 | 1 | 1 | 随模式 | 随模式 | 单值 1 个 track / handle，Range 按 N。 |
| `DisabledHandles` | 1 | 1 | 1 | N-1 | N | 只改变 thumb `IsEnabled`。 |
| `Orientation` / reverse 切换 | 1 | 1 | 1 | N-1 | N | 只改变几何与伪类。 |
| 空值 / `RangeValues=null` | 1 | 1 | 1 | 1 | 1 | Range 回退 `[Minimum, Minimum]`。 |
| `Marks` 增删 | 1 | 1 | 1 | N-1 | N | mark 不属于 Semantic Part。 |

## 6. 尺寸基线

Slider 没有 `SizeType` 分档，视觉基线由 `SliderToken` 与全局 token 常量表达：

- rail 厚度 `RailSize`，thumb 尺寸 `ThumbSize` / 圆点 `ThumbCircleSize`，mark 点 `MarkSize`。
- 活动轨道默认厚度 `SliderTrackSize`；水平 padding `SliderPaddingHorizontal`，垂直 padding `SliderPaddingVertical`。
- 轨道画刷默认 `RailBg` / `RailHoverBg`，segment 画刷默认 `TrackBg` / `TrackHoverBg` / `TrackBgDisabled`。
- thumb 边框 `ThumbCircleBorderThickness` / hover `ThumbCircleBorderThicknessHover`，outline
  `ThumbOutlineThickness`，hover / focus 圆点放大 `ThumbCircleSizeHover`。

rail / tracks / track 的几何（Bounds）与胶囊 `CornerRadius` 由 `SliderTrack` 按布局计算写入；Semantic Style 覆盖
`Background` / `Opacity` 时不影响几何。布局型固定 `Width` / `Height` / `Margin` / Min/Max Setter 不作为
rail / tracks / track 的公共定制路径：轨道几何由值比例与 rail 尺寸驱动，固定几何会绕过值布局。`handle` 的
`Width` / `Height` 同理（thumb 尺寸由 `ThumbSize` 驱动），推荐定制 `ThumbCircleSize`、边框与 outline。

## 7. 定制边界

以下区域明确不属于 Slider Semantic Part：

- mark 点与 mark 标签（`SliderMarksElement` 自绘，`Marks` / `SliderMark` 数据）。
- thumb 的 Tooltip（`ToolTip` 宿主文本与 placement）与 `ValueFormatTemplate` 格式化。
- `SliderTrack#PART_Track`（internal 协作模板部件，几何 / 交互 / 动态节点 owner）。
- Form 集成（`IFormItemAware`）与数据校验错误（`DataValidationErrors`）。
- `SliderRangeMath`、automation peers 等内部协作类型。
- `PART_*` 名称、internal 类型、Name 与模板层级。

Semantic Style 服从 Avalonia 原生属性优先级。Part Setter 命中只证明目标属性已生效；如果最终视觉仍被
`IsIncluded` 可见性、`IsDraggableTrack` 拖动几何、thumb 尺寸或 mark 布局约束，应按跨节点布局约束排查，不能把它
解释为 Semantic Style 优先级失效。

## 8. 兼容性与验证

删除或重命名 Part、修改 selector class、收窄 `ContractType`、改变 cardinality，或者让任一内置节点缺少 marker，均
属于公共主题契约变更。

验证至少覆盖：

- `Slider` descriptor 只有 `root`、`rail`、`tracks`、`track`、`handle`，字段值与本文表格一致（非 root Part 均为
  `RuntimeCreated=true` 且携带显式 SelectorRoute `/template/ .semantic-*`；`rail` / `tracks` / `track` 的
  `ContractType` 为 `Border`，`handle` 为 `SliderThumb`）。
- `SliderTrack`、`SliderThumb`、`SliderMarksElement` 不持有独立 descriptor。
- 单值模式 marker：rail / tracks / track / handle 各 1；Range N 值：rail / tracks 各 1、track N-1、handle N；
  `IsRangeMode` 切换与 `RangeValues` 快照增删同步节点数量，marker 随实例增删。
- 三个主题文件不声明静态 `.semantic-*` marker；mark 节点不携带 semantic marker。
- `IsIncluded=false`、`DisabledHandles`、`Orientation` / reverse、`IsDraggableTrack`、`Marks` 增删只改变有效视觉
  属性、可见性与伪类，不增删 rail / tracks 节点，track / handle 数量只随值数量变化。
- 元素化后默认视觉不变：rail / tracks / track 元素几何与既有 `GetRailRect` / `SegmentRects` / `TrackRangeRect`
  一致，胶囊圆角等于 rail 厚度一半，画刷与绘制顺序（rail → tracks → track → mark → thumb）不变。
- `TracksBrush` 单值模式行为对齐上游：span 为 `Minimum → Value`（此前单值模式不绘制，属对齐性行为变化）。
- owner-scoped Semantic Style（生成的 Style 类型）与 `x:SetterTargetType` 可以编译并命中对应最低 public 类型。
- 默认主题不消费 `.semantic-*`，未声明用户 Semantic Style 时不增加 selector activator。
- Generator 静态输出和 NativeAOT 路径不依赖反射或运行时扫描。
