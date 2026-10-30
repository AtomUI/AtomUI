# Slider 桌面版架构设计

本文档定义 `AtomUI.Desktop.Controls.Slider` 桌面版的设计定位、公共契约、状态模型、视觉主题关系和维护边界。通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，多 handle 专项模型见 [Slider 多 Handle 设计](multi-handle-design.md)，Semantic Part 契约见 [Slider Semantic Part 契约](semantic-part.md)，内部实现原理见 [Slider 桌面版实现原理](implementation.md)，Token 语义见 [Slider Token 设计](token.md)，变化记录见 [Slider Changelog](changelog.md)。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/DataEntry/Slider` |
| 控件状态 | Stable |

Slider 是桌面数据录入体系中的连续或离散数值选择控件。它支持单值模式和任意数量 handle 的 Range 模式，并通过轨道、thumb、mark、键盘和 Tooltip 表达数值选择。

Slider 继承 Avalonia `RangeBase`，使用 `Minimum`、`Maximum`、`Value`、`SmallChange` 和 `LargeChange` 的基础语义。Range 模式不使用独立的起止值类型，而是通过 `RangeValues` 维护有序的多值集合。

## 2. 设计语言

| 维度 | 含义 | 典型表达 |
| --- | --- | --- |
| 数值范围 | 用户在有限区间中选择值。 | `Minimum`、`Maximum`、`Value`。 |
| 多点范围 | 多个 thumb 表达多个边界和活动 segment。 | `IsRangeMode`、`RangeValues`。 |
| Handle 状态 | 每个 thumb 可独立 disabled、聚焦和拖动。 | `DisabledHandles`。 |
| 整体平移 | 首尾 handle 之间的范围整体移动。 | `IsDraggableTrack`。 |
| 操作方向 | 数值沿水平或垂直轨道变化。 | `Orientation`。 |
| 方向语义 | 视觉方向与数值增长方向可反转。 | `IsDirectionReversed`。 |
| 离散步进 | 数值吸附到固定 tick。 | `IsSnapToTickEnabled`、`TickFrequency`。 |
| 轨道语义 | 局部 segment 和整体活动范围可独立绘制。 | `TrackBarBrush`、`TracksBrush`。 |
| 刻度说明 | mark 展示关键点和标签。 | `Marks`、`SliderMark`、`IsIncluded`。 |
| 反馈展示 | 每个 handle 的 Tooltip 展示当前值。 | `ValueFormatTemplate`。 |

## 3. API 与契约模型

### RangeBase 数值 API

| API | 类型 | 语义 |
| --- | --- | --- |
| `Minimum` | `double` | 允许的最小值。 |
| `Maximum` | `double` | 允许的最大值。 |
| `Value` | `double` | 单值模式的当前值。 |
| `SmallChange` | `double` | 方向键单次调整量。 |
| `LargeChange` | `double` | PageUp / PageDown 单次调整量。 |

### Slider API

| API | 类型 | 语义 |
| --- | --- | --- |
| `Orientation` | `Orientation` | 轨道方向，默认 `Horizontal`。 |
| `IsDirectionReversed` | `bool` | 反转数值增长方向。 |
| `IsSnapToTickEnabled` | `bool` | 是否把指针和键盘输入吸附到 tick。 |
| `IsRangeMode` | `bool` | 是否使用 `RangeValues` 和多 handle 视觉。 |
| `RangeValues` | `IReadOnlyList<double>?` | Range 模式下所有 handle 的当前值，至少两个值。 |
| `DisabledHandles` | `IReadOnlyList<bool>?` | 按索引禁用指定 handle。 |
| `IsDraggableTrack` | `bool` | 是否允许整体活动范围平移。 |
| `TrackBarBrush` | `IBrush?` | 相邻 handle 之间局部轨道的画刷。 |
| `TracksBrush` | `IBrush?` | 首个到最后一个 handle 的整体轨道画刷。 |
| `Marks` | `List<SliderMark>?` | 刻度点和标签集合。 |
| `ValueFormatTemplate` | `string` | Tooltip 文本格式，默认 `{0:0}`。 |
| `IsIncluded` | `bool` | 是否绘制活动轨道和 mark 激活态。 |
| `IsMotionEnabled` | `bool` | 是否启用轨道、thumb 和 mark 动效。 |
| `IsWaveSpiritEnabled` | `bool` | 是否启用 wave spirit 能力。 |

Range 模式使用示例：

```xml
<atom:Slider
    IsRangeMode="True"
    RangeValues="0, 10, 20" />
```

```xml
<atom:Slider
    IsRangeMode="True"
    RangeValues="20, 50, 80"
    DisabledHandles="False, True, False"
    IsDraggableTrack="True" />
```

`RangeValues` 进入控件后会过滤非法值、裁剪到最小最大值并按数值升序归一化。RangeValues 少于两个值时使用 `[Minimum, Minimum]`。`DisabledHandles` 缺失项为 enabled，超出 handle 数量的项忽略。集合采用快照语义，修改时重新赋值集合实例。

### 模板部件与伪类

| Template Part | 类型 | 职责 |
| --- | --- | --- |
| `PART_Track` | `SliderTrack` | 动态 handle 管理、轨道和 mark 渲染、布局和拖动几何。 |

| 伪类 | 语义 |
| --- | --- |
| `:horizontal` | 当前为水平布局。 |
| `:vertical` | 当前为垂直布局。 |
| `:pressed` | Slider 处于 pressed 状态。 |

SliderThumb 是 SliderTrack 管理的动态视觉节点，不作为固定数量的公共 Template Part 暴露。

Semantic Part 由 `Slider` 单一 owner 公开：`root`、`rail`、`tracks`、`track`、`handle` 五个 Part 对齐上游
`SliderSemanticType`（`classNames` / `styles` 均为 `{ root?, tracks?, track?, rail?, handle? }`）。rail / tracks /
track 由 `SliderTrack` 代码创建的元素节点承载，handle 由动态 `SliderThumb` 承载，mark 与 Tooltip 不属于 Semantic
Part。完整契约见 [Slider Semantic Part 契约](semantic-part.md)。

## 4. 行为与状态模型

Slider 的核心状态流：

```text
Minimum / Maximum
      ↓
Value 或归一化后的 RangeValues
      ↓
SliderTrack.EffectiveRangeValues
      ↓
动态 SliderThumb[] + RenderContextData
      ↓
thumb arrange + track render + Tooltip
```

单值模式：

```text
IsRangeMode=false
  Value → 一个 enabled handle
```

Range 模式：

```text
IsRangeMode=true
  RangeValues[0] → handle[0]
  RangeValues[1] → handle[1]
  RangeValues[n] → handle[n]
```

范围模式行为：

- 每两个相邻 handle 之间形成一个活动轨道段。
- 首个 handle 到最后一个 handle 形成整体活动范围。
- 点击轨道时选择最近的 enabled handle。
- disabled handle 不响应 pointer、keyboard 和 focus。
- disabled handle 作为 enabled handle 的固定边界。
- `IsDraggableTrack=true` 时整体平移所有 handle。
- 任意 handle disabled 时整体轨道拖动关闭。

键盘行为：

- `Left`、`Right`、`Up`、`Down` 使用 `SmallChange`。
- `PageUp`、`PageDown` 使用 `LargeChange`。
- `Home`、`End` 移动当前焦点 handle 到有效边界。
- `IsDirectionReversed` 影响 pointer 和 keyboard 的数值方向。

Form 集成：

- 单值模式 Form 值为 `double`。
- Range 模式 Form 值为 `IReadOnlyList<double>`。
- Form 设置值、pointer 写值和 keyboard 写值共用同一套归一化路径。

连续 pointer 输入使用轨道本地坐标直接映射为 `double` 值，默认不按整数或像素量化。只有 `IsSnapToTickEnabled=true` 时，pointer 和 keyboard 写入才在提交前吸附到 `TickFrequency` 对应的 tick。

## 5. 视觉与主题模型

Slider 的默认视觉由三层主题组成：

| 主题 | 职责 |
| --- | --- |
| `SliderTheme.axaml` | 根模板、SliderTrack、属性传递和方向状态。 |
| `SliderTrackTheme.axaml` | 轨道尺寸、rail 尺寸、mark 尺寸、mark 标签和轨道过渡。 |
| `SliderThumbTheme.axaml` | thumb 尺寸、圆点、边框、outline、focus、hover 和 disabled。 |

视觉模型：

- `SliderTrack` 管理 rail、整体活动范围、相邻 segment 与 mark 的**元素**节点并计算其几何：rail 元素（
  `TrackGrooveBrush`）、tracks 元素（`TracksBrush`）、segment 元素（`TrackBarBrush`）按值比例排列，mark 点与文本
  由 internal `SliderMarksElement` 自绘；四个区域元素化后与历史自绘几何一致，默认视觉不变。
- `SliderThumb` 绘制圆点、边框和 focus / hover outline。
- `TrackBarBrush` 只表达相邻 handle 之间的 segment。
- `TracksBrush` 只表达整体活动范围（Range 模式首尾 handle 之间；单值模式 `Minimum → Value`）。
- `IsIncluded=false` 时不绘制两种活动轨道和 mark 激活态。
- disabled handle 使用对应 thumb disabled token，不改变其他 handle 的颜色。
- 全部 handle disabled 时使用 Slider 整体 disabled 状态。
- 水平布局使用 `SliderPaddingHorizontal`，垂直布局使用 `SliderPaddingVertical`。

`TrackBarBrush` 和 `TracksBrush` 是实例级画刷，不属于 SliderToken。SliderToken 只提供默认视觉值。

## 6. 控件家族或集成关系

- `RangeBase`：提供基础数值、步进和自动化语义。
- `SliderTrack`：承载动态 handle、轨道、mark、范围布局和拖动几何。
- `SliderThumb`：承载单个 handle 的 pointer capture、drag、focus 和绘制。
- `IFormItemAware`：允许 Form 读取、设置和监听 Slider 值。
- `IMotionAwareControl`：统一动效开关。
- `SliderToken`：提供组件级尺寸、颜色、padding 和 outline token。

## 7. 兼容性不变量

- Range 模式唯一值源是 `RangeValues`，不建立起止 handle 特殊状态。
- `RangeValues` 必须经过有限性校验、范围裁剪和升序归一化。
- Range 模式至少保留两个 handle。
- disabled handle 不能被 pointer 或 keyboard 修改。
- 任意 disabled handle 存在时，整体活动范围不可拖动。
- `IsDraggableTrack` 平移所有 handle，并保持 handle 相对距离。
- `TrackBarBrush` 和 `TracksBrush` 的职责不能互换。
- `IsIncluded=false` 只影响活动轨道和 mark 激活视觉，不影响值计算。
- SliderTrack 负责动态 thumb 的创建、复用、回收和事件解绑。
- 模板重建必须释放旧事件订阅和拖动状态。
- SliderTrack detach 必须释放全局输入订阅。
- Tooltip 只展示格式化值，不改变 Form 值。
- Token 不承载 Value、RangeValues、DisabledHandles 或拖动状态。

## 8. 专项模型

Slider 的多 handle 专项模型由 [Slider 多 Handle 设计](multi-handle-design.md) 定义，覆盖有序值集合、按索引禁用、动态 thumb、相邻 segment、整体活动范围拖动、坐标算法和定制边界。

该专项模型不支持运行时通过交互增删 handle，也不提供 `editable`、`minCount` 或 `maxCount`。handle 数量完全由调用方重新赋值的 `RangeValues` 快照决定。

## 9. 文档导航与验证策略

关联文档：

- [Slider 多 Handle 设计](multi-handle-design.md)
- [Slider Semantic Part 契约](semantic-part.md)
- [Slider 桌面版实现原理](implementation.md)
- [Slider Token 设计](token.md)
- [Slider Changelog](changelog.md)

稳定 Gallery 示例包括：

- 多点组合。
- 禁用指定滑块。

验证重点：

| 层次 | 验证内容 |
| --- | --- |
| Public API | 检查 `RangeValues`、`DisabledHandles`、`IsDraggableTrack` 和画刷属性。 |
| 行为状态 | 验证多 handle、disabled handle、整体拖动、键盘、Tooltip、Form 和 tick。 |
| AXAML | 验证 `PART_Track`、动态 thumb、horizontal / vertical 和 disabled 样式。 |
| Token | 验证默认轨道、rail、thumb、mark、outline 和方向 padding。 |
| Gallery | 验证两个 Slider Showcase 的结构、绑定和交互。 |
| 文档 | 检查相对链接、命名一致性并运行 `git diff --check`。 |
