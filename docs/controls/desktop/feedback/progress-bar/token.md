# ProgressBar Token 设计

本文档定义进度条家族 Token（`ProgressBarToken`、`StepsProgressBarToken`、`CircleProgressToken`、`DashboardProgressToken` 及其抽象层）的专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables、预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。ProgressBar 整体架构见 [ProgressBar 桌面版架构设计](overview.md)，内部实现原理见 [ProgressBar 桌面版实现原理](implementation.md)，设计和契约变化记录见 [ProgressBar Changelog](changelog.md)。

## 1. 定位

进度条家族的四个控件各自拥有终端 Token（对应 antd Progress 的同一组组件 Token），通过无 identity 的抽象层共享定义与默认计算：

```text
AbstractProgressToken            DefaultColor, RemainingColor
├── AbstractLineProgressToken    LineExtraInfoMargin, LineInfoIconSize, LineInfoIconSizeSM
│   ├── ProgressBarToken         LineBorderRadius, LineProgressPadding, ProgressActiveMotionDuration (Reserved)
│   └── StepsProgressBarToken    ProgressStepMarginInlineEnd, ProgressStepMinWidth
└── AbstractCircleProgressToken  CircleTextColor, CircleMinimumTextFontSize, CircleMinimumIconSize
    ├── CircleProgressToken
    └── DashboardProgressToken
```

每个控件用自己的 Effective Global Token 计算自己的 Own Token，`Controls[CircleProgress].Tokens[ColorInfo]` 只改变
CircleProgress 的默认进度色，`Controls[ProgressBar].Tokens[DefaultColor]` 不再影响其他进度控件。

进度 Token 不承载以下状态：

- `Value`、`Minimum`、`Maximum`、`Percentage` 等实例进度值。
- `Status`、`IsIndeterminate`、completed、disabled 等运行时状态。
- `PercentPosition`、`Orientation`、`StepCount`、`Steps`、`DashboardGapPosition` 等布局或形态状态。
- `StrokeBrush`、`SuccessStrokeBrush`、`TrailColor` 等实例画刷覆盖。
- `IndicatorThickness`、`ChunkWidth`、`ChunkHeight`、显式 `Width` / `Height` 等实例尺寸覆盖。

这些状态分别由 public API、internal effective state、主题 selector 和模板几何布局逻辑处理。

## 2. Token 分类

### 2.1 颜色 Token（四个控件）

- `DefaultColor`：普通进度段默认颜色，默认来自 `ColorInfo`。
- `RemainingColor`：剩余轨道颜色，默认来自 `ColorFillSecondary`。

状态色不在进度 Token 中重复定义：成功色使用 `ColorSuccess`，异常色使用 `ColorError`，disabled 使用
`ColorBgContainerDisabled`、`ControlItemBgActiveDisabled` 与 `ColorTextDisabled`。

### 2.2 线形 Token（ProgressBar、StepsProgressBar）

- `LineExtraInfoMargin`：进度条和百分比文本或状态图标之间的距离，默认 `ControlPaddingHorizontalSM`。
- `LineInfoIconSize` / `LineInfoIconSizeSM`：状态图标尺寸。Large 使用 `LineInfoIconSize`，Middle 和 Small 使用
  `LineInfoIconSizeSM`。

### 2.3 ProgressBar Token

- `LineBorderRadius`：圆头（`StrokeLineCap=Round`）线形进度的 rail / track / success 圆角，默认 100（胶囊形；此前主题
  硬编码 `CornerRadius=1000`，对高度小于 200px 的进度条两者都是胶囊）。
- `LineProgressPadding`：内嵌百分比文本和进度条边缘之间的内部 padding，默认 `UniformlyPaddingXXS / 2`。
- `ProgressActiveMotionDuration`：Reserved，类型 `TimeSpan`，默认 2.4s（antd `progressActiveMotionDuration: '2.4s'`）。
  Active 状态的轨道闪光动画尚未实现，Active 与 Normal 渲染相同；类型先与 antd 对齐，激活时不再是破坏性变更。

### 2.4 StepsProgressBar Token

- `ProgressStepMarginInlineEnd`：相邻步骤之间的间距（double），默认 `UniformlyMarginXXS / 2`（2）。
- `ProgressStepMinWidth`：步骤最小宽度，默认等于 `ProgressStepMarginInlineEnd`。自动宽度和显式 `ChunkWidth` 都不小于它。

### 2.5 圆形 Token（CircleProgress、DashboardProgress）

- `CircleTextColor`：圆形百分比文本颜色，默认 `ColorText`（对应 antd `circleTextColor`）。
- `CircleMinimumTextFontSize`：按圆形尺寸计算的文本字号下限，默认 `FontSizeSM - 2`。
- `CircleMinimumIconSize`：按圆形尺寸计算的状态图标尺寸下限，默认 `SizeXS`。

两个下限变化后会立即重新计算字号与图标尺寸。

## 3. 控件专项模型中的 Token 使用

Token 使用路径：

```text
Effective Global Token（每个进度控件自己的 identity）
    ↓
XxxToken.CalculateTokenValues()（抽象层与终端）
    ↓
ProgressBarTheme / StepsProgressBarTheme / CircleProgressTheme / DashboardProgressTheme（Own Token）
AbstractProgressBarTheme / AbstractLineProgressTheme / AbstractCircleProgressTheme（OwnerTokenResource 读全局 Token）
    ↓
direct render + template part layout
```

主题使用关系：

| Token 分类 | 主要消费点 |
| --- | --- |
| 默认颜色 | 各控件主题设置默认 `StrokeBrush` 和 `GrooveBrush`。 |
| 线形间距与图标 | 线形控件主题设置 `LineExtraInfoMargin`、`LineProgressPadding` 和内部 `PresetLineInfoIconSize(SM)`；`AbstractLineProgressTheme.axaml` 按 `EffectiveSizeType` 选择图标尺寸。 |
| 步骤间距 | `StepsProgressBarTheme.axaml` 设置内部 `ChunkSpace` 和 `MinChunkWidth`。 |
| 圆形内部信息 | 圆形控件主题设置文本颜色和文本、图标最小尺寸；计算出的字号和图标尺寸经内部属性由 `AbstractCircleProgressTheme.axaml` 投射到 `FontSize` 和状态图标。 |
| 状态色 | `AbstractProgressBarTheme.axaml` 以 `OwnerTokenResource` 读取成功、错误、disabled 颜色；状态图标画刷先写入进度控件的内部画刷再向下绑定（IconPresenter 有自己的 identity）。 |
| 动效 | `AbstractProgressBarTheme.axaml` 的 `Value`、`StrokeBrush`、`Foreground` 过渡以 `atom:TokenTransitions` 声明，按接收的进度控件解析。 |

文本几何最小尺寸（`MinWidth` / `MinHeight`）由内部 `FallbackMinWidth` / `FallbackMinHeight` 经主题 Setter 投射，
本地值和应用样式保持更高优先级。

## 4. 控件家族影响

Token 变更影响范围：

- `ProgressBar`：默认颜色、剩余轨道色、圆角、文本间距、内嵌文本 padding、状态图标尺寸。
- `StepsProgressBar`：默认颜色、剩余轨道色、步骤间距和最小宽度、文本间距、状态图标尺寸。
- `CircleProgress` / `DashboardProgress`：默认颜色、剩余轨道色、文本颜色、文本和图标最小尺寸。
- Gallery ProgressBar 示例和 token.md 语义说明。

进度 Token 不应被 Slider、Spin、Steps、Skeleton 或其他反馈控件直接消费。跨控件共享语义应进入 Global Token。

### 4.1 迁移记录（Unreleased）

- `ProgressBar.CircleTextColor`、`ProgressBar.CircleMinimumTextFontSize`、`ProgressBar.CircleMinimumIconSize` 移到
  `CircleProgress` / `DashboardProgress`；`ProgressBar.ProgressStepMarginInlineEnd`、`ProgressBar.ProgressStepMinWidth` 移到
  `StepsProgressBar`，`ProgressStepMarginInlineEnd` 改为 double。
- `CircleTextColor`、`LineBorderRadius`、`ProgressStepMarginInlineEnd`、`ProgressStepMinWidth` 由未消费改为 Active；
  圆头线形进度的圆角由主题硬编码 `CornerRadius=1000` 改为消费 `LineBorderRadius`（默认 100）。
  `ProgressActiveMotionDuration` 标为 Reserved，并由 `double` 改为 `TimeSpan`（默认 2.4s）。

## 5. 兼容性要求

进度 Token 属于进度条家族主题契约。即使各 Token 类型是 internal 类型，生成的 `XxxTokenKind`、AXAML resource 使用点和 token.md 语义说明已经形成稳定依赖。

Token 变更要求：

- 不擅自重命名既有 Token。
- 不擅自删除既有 Token。
- 不把实例进度值、状态、方向、分段数量或百分比位置迁移为 Token。
- 不在进度 Token 中复制 Global Token 已经表达的成功、错误、disabled 状态色。
- `LineInfoIconSize` 和 `LineInfoIconSizeSM` 必须继续能容纳线形状态图标。
- `CircleMinimumTextFontSize` 和 `CircleMinimumIconSize` 必须继续作为圆形内部信息的下限。
- `LineExtraInfoMargin` 和 `LineProgressPadding` 不得改变 `PercentPosition` 的语义，只能改变间距。
- 需要破坏性变更时，必须先说明影响范围并获得授权。

## 6. 验证策略

| 改动类型 | 验证要求 |
| --- | --- |
| 新增进度 Token | 检查生成的 `XxxTokenKind`、AXAML 引用、token.md 语义说明、默认值计算和 `StepsProgressFamilyEffectTests` 生效测试。 |
| 修改默认颜色 | 验证 normal、active、success、exception、completed、disabled 和 dark theme。 |
| 修改线形间距 | 验证水平、垂直、内嵌、外部起点、中间、终点百分比布局。 |
| 修改图标尺寸 | 验证线形、步骤线形、圆形和仪表盘的成功 / 异常图标不裁剪。 |
| 修改圆形最小尺寸 | 验证 Large / Middle / Small、显式宽高和小尺寸圆形中的文本与图标可见性。 |
| 删除或重命名 Token | 默认不允许；如获授权，需同步所有 AXAML 引用、生成文件、Token 类型、生成数据和 token.md和控件文档。 |
