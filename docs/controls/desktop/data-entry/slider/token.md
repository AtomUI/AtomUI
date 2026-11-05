# Slider Token 设计

本文档定义 `AtomUI.Desktop.Controls.SliderToken` 的 Slider 专属语义、分类和使用边界。控件 Token 通用规范见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。Slider 架构见 [Slider 桌面版架构设计](overview.md)，多 handle 模型见 [Slider 多 Handle 设计](multi-handle-design.md)，实现原理见 [Slider 桌面版实现原理](implementation.md)，变化记录见 [Slider Changelog](changelog.md)。

## 1. 定位

SliderToken 是 Slider 的控件级 Token scope，定义轨道尺寸、rail 尺寸、thumb 尺寸、mark 尺寸、track / rail / mark / thumb 颜色、outline 和 orientation padding。

SliderToken 不承载：

- `Value`、`RangeValues`、`Minimum`、`Maximum`、`TickFrequency` 等实例数值。
- `DisabledHandles`、`IsDraggableTrack` 和当前拖动状态。
- `Marks` 集合、mark 文本和命中状态。
- Tooltip 文本、focus、hover、pressed 和 pointer capture 状态。
- `TrackBarBrush`、`TracksBrush` 这类实例级画刷。

这些状态由 Slider、SliderTrack、SliderThumb、Gallery ViewModel 和主题 selector 分别持有。

## 2. Token 分类

### 2.1 轨道与布局尺寸

- `SliderTrackSize`
- `RailSize`
- `SliderPaddingHorizontal`
- `SliderPaddingVertical`
- `MarginPartWithMark`

`SliderTrackSize` 表示轨道区域推荐尺寸，`RailSize` 表示 rail 厚度。水平和垂直 padding 为 thumb、rail 和 mark 提供布局空间。

### 2.2 Mark

- `MarkSize`
- `MarkBorderColor`
- `MarkBorderColorHover`
- `MarkBorderColorActive`

Mark Token 服务 mark 点的尺寸和边框状态色。Mark 文本使用 Slider 字体属性和 SharedToken。

### 2.3 Thumb 尺寸

- `ThumbSize`
- `ThumbCircleSize`
- `ThumbCircleSizeHover`
- `ThumbCircleBorderThickness`
- `ThumbCircleBorderThicknessHover`
- `ThumbOutlineThickness`

`ThumbSize` 容纳 thumb 圆点、边框和 outline；圆点和 hover 圆点使用独立尺寸 Token。

### 2.4 Thumb 颜色

- `ThumbCircleBorderColor`
- `ThumbCircleBorderHoverColor`
- `ThumbCircleBorderActiveColor`
- `ThumbCircleBorderColorDisabled`
- `ThumbOutlineColor`

这些 Token 同时服务普通 thumb 和动态多 handle thumb。单个 disabled handle 通过 SliderThumb 的 `:disabled` selector 消费 disabled Token。

### 2.5 Rail 与 Track 颜色

- `RailBg`
- `RailHoverBg`
- `TrackBg`
- `TrackHoverBg`
- `TrackBgDisabled`

`TrackBg` 是默认 `TrackBarBrush`，`TrackBgDisabled` 用于整体 disabled Slider。`TracksBrush` 没有独立 Token，默认为空，仅由实例或外部主题设置。

## 3. 控件专项模型中的 Token 使用

```text
SharedToken
    ↓
SliderToken 的 DependencyDefaultsV2 默认计划
    ↓
SliderTheme / SliderTrackTheme / SliderThumbTheme
    ↓
SliderTrack.Render + SliderThumb.Render
```

主题关系：

| Token 分类 | 消费点 |
| --- | --- |
| 轨道尺寸 | `SliderTrackTheme.axaml`、SliderTrack measure / arrange。 |
| Orientation padding | `SliderTheme.axaml` 根据 horizontal / vertical 设置 SliderTrack.Padding。 |
| Rail / Track 颜色 | SliderTheme 设置默认 TrackGrooveBrush 和 TrackBarBrush。 |
| Mark 颜色 | SliderTheme 和 SliderTrackTheme 设置 mark brush。 |
| Thumb 尺寸 | SliderThumbTheme 设置 Width、Height、ThumbCircleSize 和边框。 |
| Thumb disabled 颜色 | SliderThumbTheme 的 `:disabled` selector。 |
| Thumb outline | SliderThumbTheme 的 hover / focus selector。 |

`TracksBrush` 和 `TrackBarBrush` 的整体 / 局部职责由 Slider 多 handle 模型定义，不由 Token 区分。渐变、业务颜色和每个示例的颜色变化属于实例配置或 Gallery ViewModel。

## 4. 控件家族影响

SliderToken 只服务 Slider、SliderTrack 和 SliderThumb，不被 NumericUpDown、Progress、ScrollBar 或其他轨道控件直接消费。

需要同时影响多个轨道类控件的视觉语义时，应确认其是否属于 SharedToken，而不是直接复用 SliderToken。

## 5. 兼容性要求

`SliderToken 的 DependencyDefaultsV2 默认计划` 使用 SharedToken 推导：

- rail 的固定默认厚度（可由 Own 覆盖）。
- thumb 圆点尺寸。
- hover 圆点和边框增量。
- active、hover、disabled 颜色。
- outline 颜色和透明投影。
- horizontal / vertical padding。

Token 不参与 handle 值排序、disabled 边界、整体轨道 offset 或 Tooltip 格式化。修改 Token 名称、默认值或主题引用时，必须同步 SliderTheme、SliderTrackTheme、SliderThumbTheme 和对应的 Light / Dark 验证。

## 6. 验证策略

| 改动类型 | 验证要求 |
| --- | --- |
| 轨道尺寸 | 验证 horizontal / vertical、thumb 居中、mark 和 Tooltip。 |
| Thumb 尺寸 | 验证普通、hover、focus、drag、partial disabled 和 all disabled。 |
| 颜色 Token | 验证普通、hover、active、disabled、included / not included 和 dark theme。 |
| Padding | 验证有无 mark 的 horizontal / vertical 布局和文本不重叠。 |
| 动态 handle | 验证所有动态 thumb 使用相同 Token 和主题状态。 |
| Token 生成 | 检查 SliderTokenKind、AXAML 引用、默认值计算和 token.md 语义一致。 |

## 派生默认值与兼容字段

25字段保留原类型和资源编号，采用 DependencyDefaultsV2。ThumbSize 依赖实际 ThumbCircleSizeHover、ThumbCircleBorderThicknessHover、ThumbOutlineThickness；ThumbOutlineColor 依赖实际 active 色；方向 padding 依赖实际 SliderTrackSize。显式派生字段本身继续优先。

24字段服务现有 rail/track/mark/handle 及方向分支。MarginPartWithMark 保留导出和默认计算，当前没有布局消费者；mark 标签留位由实际文本度量承担，不额外叠加这项历史 margin。Slider、公开 SliderTrack/SliderThumb 保留各自 Global 身份和共享 Slider Own；组合主题明确提供的输入继续由 owner 负责。
