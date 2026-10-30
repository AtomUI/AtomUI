# ProgressBar Token 设计

本文档定义 `AtomUI.Desktop.Controls.ProgressBarToken` 的 ProgressBar 专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables、预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。ProgressBar 整体架构见 [ProgressBar 桌面版架构设计](overview.md)，内部实现原理见 [ProgressBar 桌面版实现原理](implementation.md)，设计和契约变化记录见 [ProgressBar Changelog](changelog.md)。

## 1. 定位

ProgressBarToken 是 ProgressBar 家族的控件级 Token scope。它定义默认进度色、剩余轨道色、圆形文本和图标最小尺寸、线形状态图标尺寸、线形额外信息间距和线形内部 padding。

ProgressBarToken 不承载以下状态：

- `Value`、`Minimum`、`Maximum`、`Percentage` 等实例进度值。
- `Status`、`IsIndeterminate`、completed、disabled 等运行时状态。
- `PercentPosition`、`Orientation`、`StepCount`、`Steps`、`DashboardGapPosition` 等布局或形态状态。
- `StrokeBrush`、`SuccessStrokeBrush`、`TrailColor` 等实例画刷覆盖。
- `IndicatorThickness`、`ChunkWidth`、`ChunkHeight`、显式 `Width` / `Height` 等实例尺寸覆盖。

这些状态分别由 public API、internal effective state、主题 selector 和模板几何布局逻辑处理。

## 2. Token 分类

### 2.1 颜色 Token

- `DefaultColor`
- `RemainingColor`
- `CircleTextColor`

`DefaultColor` 是普通进度段默认颜色，默认来自 SharedToken `ColorInfo`。`RemainingColor` 是剩余轨道颜色，默认来自 SharedToken `ColorFillSecondary`。`CircleTextColor` 表示圆形进度文本颜色语义，默认来自 SharedToken `ColorText`。

状态色不在 ProgressBarToken 中重复定义：

- 成功色使用 SharedToken `ColorSuccess`。
- 异常色使用 SharedToken `ColorError`。
- disabled 颜色使用 SharedToken disabled 语义。

### 2.2 线形形态 Token

- `LineBorderRadius`
- `LineExtraInfoMargin`
- `LineProgressPadding`

`LineBorderRadius` 表示线形进度胶囊形圆角语义。默认模板的 rail/track/success `Border` 根据 `StrokeLineCap` 选择胶囊或直角视觉，Token 仍属于线形主题契约。

`LineExtraInfoMargin` 表示线形进度条和百分比文本或状态图标之间的距离。`LineProgressPadding` 表示内嵌百分比文本和进度条边缘之间的内部 padding。

### 2.3 线形状态图标 Token

- `LineInfoIconSize`
- `LineInfoIconSizeSM`

这两个 Token 控制线形进度状态图标尺寸。Large 使用 `LineInfoIconSize`，Middle 和 Small 使用 `LineInfoIconSizeSM`。状态图标颜色由 SharedToken 成功、错误和 disabled 语义控制。

### 2.4 圆形内部信息 Token

- `CircleMinimumTextFontSize`
- `CircleMinimumIconSize`

圆形进度会根据圆形实际尺寸动态计算内部文本字号和状态图标尺寸，并用这两个 Token 作为下限，避免小尺寸圆形中文本或图标失去可读性。

### 2.5 内部 Token 槽位

`ProgressStepMinWidth`、`ProgressStepMarginInlineEnd` 和 `ProgressActiveMotionDuration` 当前属于 `ProgressBarToken` 声明的内部槽位。当前主题和 token.md 没有把它们作为可观察视觉契约入口。维护时不得依赖这些槽位改变现有渲染，启用任何槽位前必须同步主题、token.md 和控件文档。

## 3. 控件专项模型中的 Token 使用

Token 使用路径：

```text
SharedToken
    ↓
ProgressBarToken.CalculateTokenValues()
    ↓
AbstractProgressBarTheme / AbstractLineProgressTheme / AbstractCircleProgressTheme
    ↓
direct render + template part layout
```

主题使用关系：

| Token 分类 | 主要消费点 |
| --- | --- |
| 默认颜色 | `AbstractProgressBarTheme.axaml` 设置默认 `StrokeBrush` 和 `GrooveBrush`。 |
| 线形间距 | `AbstractLineProgressTheme.axaml` 设置 `LineExtraInfoMargin` 和 `LineProgressPadding`。 |
| 线形图标 | `AbstractLineProgressTheme.axaml` 根据 `EffectiveSizeType` 设置 `LineInfoIconSize` 和图标宽高。 |
| 圆形内部信息 | `AbstractCircleProgressTheme.axaml` 设置圆形文本和图标最小尺寸。 |
| 状态色 | `AbstractProgressBarTheme.axaml` 和图标 selector 使用 SharedToken 成功、错误、disabled 颜色。 |

默认值计算：

- `DefaultColor` 使用 SharedToken 信息色。
- `RemainingColor` 使用 SharedToken 次级填充色。
- `LineExtraInfoMargin` 使用小号水平 padding。
- `LineProgressPadding` 使用极小统一 padding 的一半。
- `CircleMinimumTextFontSize` 使用小号字体减小值。
- `CircleMinimumIconSize` 使用 XS 尺寸。
- 图标尺寸使用 SharedToken icon size。

## 4. 控件家族影响

同一个 ProgressBarToken scope 服务四个具体控件：

- `ProgressBar`
- `StepsProgressBar`
- `CircleProgress`
- `DashboardProgress`

Token 变更影响范围：

- 普通线形进度的默认颜色、剩余轨道色、文本间距和内嵌文本 padding。
- 步骤线形进度的默认颜色、文本间距和状态图标尺寸。
- 圆形进度的内部文本和图标最小尺寸。
- 仪表盘进度的内部文本和图标最小尺寸。
- Gallery ProgressBar 示例和 token.md 语义说明。

ProgressBarToken 不应被 Slider、Spin、Steps、Skeleton 或其他反馈控件直接消费。跨控件共享语义应进入 SharedToken，而不是复用 ProgressBarToken。

## 5. 兼容性要求

ProgressBarToken 属于 ProgressBar 家族主题契约。即使 `ProgressBarToken` 是 internal 类型，生成的 `ProgressBarTokenKind`、AXAML resource 使用点和 token.md 语义说明已经形成稳定依赖。

Token 变更要求：

- 不擅自重命名既有 Token。
- 不擅自删除既有 Token。
- 不把实例进度值、状态、方向、分段数量或百分比位置迁移为 Token。
- 不在 ProgressBarToken 中复制 SharedToken 已经表达的成功、错误、disabled 状态色。
- `LineInfoIconSize` 和 `LineInfoIconSizeSM` 必须继续能容纳线形状态图标。
- `CircleMinimumTextFontSize` 和 `CircleMinimumIconSize` 必须继续作为圆形内部信息的下限。
- `LineExtraInfoMargin` 和 `LineProgressPadding` 不得改变 `PercentPosition` 的语义，只能改变间距。
- 需要破坏性变更时，必须先说明影响范围并获得授权。

## 6. 验证策略

| 改动类型 | 验证要求 |
| --- | --- |
| 新增 ProgressBarToken | 检查生成的 `ProgressBarTokenKind`、AXAML 引用、token.md 语义说明和默认值计算。 |
| 修改默认颜色 | 验证 normal、active、success、exception、completed、disabled 和 dark theme。 |
| 修改线形间距 | 验证水平、垂直、内嵌、外部起点、中间、终点百分比布局。 |
| 修改图标尺寸 | 验证线形、步骤线形、圆形和仪表盘的成功 / 异常图标不裁剪。 |
| 修改圆形最小尺寸 | 验证 Large / Middle / Small、显式宽高和小尺寸圆形中的文本与图标可见性。 |
| 删除或重命名 Token | 默认不允许；如获授权，需同步所有 AXAML 引用、生成文件、Token 类型、生成数据和 token.md和控件文档。 |
