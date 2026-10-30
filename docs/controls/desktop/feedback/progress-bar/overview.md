# ProgressBar 桌面版架构设计

本文档定义 `AtomUI.Desktop.Controls.ProgressBar` 家族桌面版的最新设计定位、公共契约、状态模型、视觉主题关系和兼容边界。通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，公开主题区域见 [ProgressBar Semantic Part 契约](semantic-part.md)，内部实现原理见 [ProgressBar 桌面版实现原理](implementation.md)，ProgressBar Token 的专项设计见 [ProgressBar Token 设计](token.md)，设计和契约变化记录见 [ProgressBar Changelog](changelog.md)。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/Feedback/ProgressBar` |
| 控件状态 | Stable |

ProgressBar 是 AtomUI 桌面反馈体系中的进度展示控件，用于表达任务完成比例、状态结果、成功阈值、分段进度和环形进度。它继承 Avalonia `RangeBase` 的 `Minimum`、`Maximum` 和 `Value` 语义，并扩展 `Status`、`SizeType`、成功段、文字格式、线帽、方向、分段、圆形和仪表盘模型。

ProgressBar 的职责是展示已经由外部业务计算好的进度值。它不负责启动任务、取消任务、调度异步流程、估算剩余时间、处理下载队列或持有业务任务状态。需要任务生命周期、错误重试或进度聚合时，应由业务 ViewModel 或服务层维护，并把当前值写入 ProgressBar。

## 2. 设计语言

ProgressBar 的设计语言来自轨道、已完成段、状态色、状态图标和百分比文本的组合。

| 维度 | 含义 | 典型表达 |
| --- | --- | --- |
| 进度范围 | 在有限区间中展示完成比例。 | `Minimum`、`Maximum`、`Value`、`Percentage`。 |
| 视觉形态 | 用不同几何表达进度。 | `ProgressBar`、`StepsProgressBar`、`CircleProgress`、`DashboardProgress`。 |
| 进度方向 | 线形和步骤进度支持水平或垂直方向。 | `Orientation`、`:horizontal`、`:vertical`。 |
| 状态结果 | 表达普通、活动、成功、异常状态。 | `Status`、`:completed`、状态图标。 |
| 成功阈值 | 在整体进度中标记成功段。 | `SuccessThreshold`、`SuccessStrokeBrush`。 |
| 信息展示 | 显示百分比或自定义格式文本。 | `IsProgressInfoVisible`、`ProgressTextFormat`、`PercentPosition`。 |
| 尺寸语义 | 根据大中小规格或显式厚度计算视觉尺寸。 | `SizeType`、`IndicatorThickness`、`ChunkWidth`、`ChunkHeight`。 |
| 动效 | 进度值和颜色变化可以带 transition。 | `IsMotionEnabled`。 |

ProgressBar 的主视觉必须是进度本身。百分比文本、状态图标和成功阈值是辅助反馈，不应改变 `Value` 的业务含义。

## 3. API 与契约模型

ProgressBar 家族由一个共享基类模型和四个具体控件组成。

| 控件 | 基类 | 语义 |
| --- | --- | --- |
| `ProgressBar` | `AbstractGeneralProgressBar` | 普通线形进度条，支持水平、垂直、内外百分比文本和成功段。 |
| `StepsProgressBar` | `AbstractGeneralStepsProgressBar` | 线形分段进度条，以固定 chunk 表达离散进度。 |
| `CircleProgress` | `AbstractGeneralCircleProgress` | 圆形进度条，支持普通圆弧和圆形分段。 |
| `DashboardProgress` | `AbstractGeneralDashboardProgress` | 仪表盘进度条，支持缺口方向、缺口角度和分段。 |

共享 RangeBase API：

| API | 类型 | 语义 |
| --- | --- | --- |
| `Minimum` | `double` | 进度范围最小值。 |
| `Maximum` | `double` | 进度范围最大值。 |
| `Value` | `double` | 当前进度值。 |
| `Percentage` | `double` | 只读百分比，按 `(Value - Minimum) / (Maximum - Minimum) * 100` 计算。 |

共享 Progress API：

| API | 类型 | 语义 |
| --- | --- | --- |
| `IsIndeterminate` | `bool` | 是否处于不确定进度状态，并同步 `:indeterminate` 伪类。 |
| `IsProgressInfoVisible` | `bool` | 是否显示百分比文本或状态图标区域。 |
| `ProgressTextFormat` | `string` | 百分比文本格式，默认 `{0:0}%`。 |
| `StrokeBrush` | `IBrush?` | 已完成段画刷。 |
| `TrailColor` | `Color?` | 剩余轨道颜色，设置后覆盖默认 `GrooveBrush`。 |
| `StrokeLineCap` | `PenLineCap` | 已完成段端点形态，默认 `Round`。 |
| `SizeType` | `SizeType` | 大中小规格，默认 `Large`。 |
| `Status` | `ProgressStatus` | 普通、成功、异常、活动状态。 |
| `IndicatorThickness` | `double` | 显式指示器厚度；`NaN` 时由尺寸规格计算。 |
| `SuccessThreshold` | `double` | 成功段阈值；`NaN` 时不绘制成功段。 |
| `SuccessStrokeBrush` | `IBrush?` | 成功段画刷。 |
| `IsMotionEnabled` | `bool` | 是否启用进度值和颜色 transition。 |
| `ExceptionCompletedIcon` | `PathIcon?` | 异常状态图标。 |
| `SuccessCompletedIcon` | `PathIcon?` | 成功状态图标。 |

线形扩展 API：

| API | 控件 | 类型 | 语义 |
| --- | --- | --- | --- |
| `Orientation` | `ProgressBar`、`StepsProgressBar` | `Orientation` | 水平或垂直布局。 |
| `PercentPosition` | `ProgressBar` | `PercentPosition` | 百分比文本在进度条内外及起点、中间、终点的定位。 |
| `PercentPosition` | `StepsProgressBar` | `LinePercentAlignment` | 步骤进度百分比文本的起点、中间、终点定位。 |
| `Steps` | `StepsProgressBar` | `int` | 步骤数量，最小为 1。 |
| `StepsStrokeBrush` | `StepsProgressBar` | `List<IBrush>?` | 每个已完成步骤的自定义画刷。 |
| `ChunkWidth` | `StepsProgressBar` | `double` | 步骤块宽度，`NaN` 时由 `SizeType` 计算。 |
| `ChunkHeight` | `StepsProgressBar` | `double` | 步骤块高度，`NaN` 时由 `IndicatorThickness` / `StrokeThickness` 计算。 |

圆形和仪表盘扩展 API：

| API | 控件 | 类型 | 语义 |
| --- | --- | --- | --- |
| `StepCount` | `CircleProgress`、`DashboardProgress` | `int` | 圆弧分段数量，0 表示普通连续圆弧。 |
| `StepGap` | `CircleProgress`、`DashboardProgress` | `double` | 分段间隔角度，默认 2。 |
| `DashboardGapPosition` | `DashboardProgress` | `DashboardGapPosition` | 仪表盘缺口方向，默认 `Bottom`。 |
| `GapDegree` | `DashboardProgress` | `double` | 仪表盘缺口角度，范围 `[0, 295]`，默认 75。 |

稳定 template part：

| Template Part | 类型 | 职责 |
| --- | --- | --- |
| `PART_LayoutTransformControl` | `LayoutTransformControl` | 承载百分比文本并在垂直内嵌模式中旋转。 |
| `PART_PercentageLabel` | `Label` | 显示格式化后的进度文本。 |
| `PART_ExceptionCompletedIconPresenter` | `IconPresenter` | 显示异常状态图标。 |
| `PART_SuccessCompletedIconPresenter` | `IconPresenter` | 显示成功状态图标。 |

稳定伪类：

| 伪类 | 语义 |
| --- | --- |
| `:indeterminate` | `IsIndeterminate=true`。 |
| `:completed` | `Value` 等于 `Maximum`。 |
| `:horizontal` | 线形进度为水平布局。 |
| `:vertical` | 线形进度为垂直布局。 |
| `:labelinner` | `ProgressBar.PercentPosition.IsInner=true`。 |
| `:labelinner-start` | 内嵌百分比位于起点。 |
| `:labelinner-center` | 内嵌百分比位于中间。 |
| `:labelinner-end` | 内嵌百分比位于终点。 |

ProgressBar 当前不实现 `ICustomizeSizeTypeAware`。自定义尺寸通过 `IndicatorThickness`、`Width`、`Height`、`ChunkWidth` 和 `ChunkHeight` 等现有属性表达。

## 4. 行为与状态模型

基础进度状态流：

```text
Minimum / Maximum / Value
      ↓
CalculateProgressRatio(value)
      ↓
Percentage + Indicator geometry
      ↓
render groove / progress / success segment + extra info
```

状态语义：

- `Percentage` 始终以 `Minimum` 为基准，不直接使用 `Value / Maximum`。
- `Value == Maximum` 时进入 completed 状态，设置 `IsCompleted` 和 `:completed`。
- `Status=Success` 使用成功色和成功图标；`Status=Exception` 使用错误色和异常图标。
- `Status=Normal` 和 `Status=Active` 在完成前使用默认进度色，完成后使用成功色。
- `IsIndeterminate` 只表达不确定进度伪类，不改变 `Value`、`Percentage` 或 RangeBase 值。
- `IsProgressInfoVisible=false` 隐藏文本和状态图标区域，但不影响轨道和进度绘制。
- disabled 状态使用禁用轨道、禁用进度色和禁用文本色。

尺寸状态：

- 线形进度默认根据 `SizeType` 映射到大、中、小三档厚度。
- `IndicatorThickness` 有值时覆盖线形和圆形的计算厚度。
- 圆形进度默认尺寸由 `SizeType` 映射为 `120 / 90 / 60`，显式 `Width` / `Height` 会参与最终尺寸。
- `StepsProgressBar` 使用 `Steps`、chunk 尺寸和固定 chunk 间距计算总尺寸。
- `StepsProgressBar.ChunkHeight` 与 `IndicatorThickness` 保持同步，是现有兼容契约的一部分。

百分比文本：

- `ProgressTextFormat` 格式化 `Percentage`。
- `ProgressBar.PercentPosition.IsInner=true` 时，文本布局跟随已完成进度区域。
- 垂直内嵌文本通过 `LayoutTransformControl` 旋转。
- 非内嵌完成态显示状态图标时，百分比文本隐藏。
- 圆形进度根据可用内圆区域决定百分比文本或状态图标是否可见。

## 5. 视觉与主题模型

ProgressBar 的默认视觉由抽象主题和具体控件主题组合：

| 主题 | 职责 |
| --- | --- |
| `AbstractProgressBarTheme.axaml` | 共享模板入口、默认对齐、动效、状态色、completed / disabled 状态。 |
| `AbstractLineProgressTheme.axaml` | 线形模板、百分比 label、状态图标、线形尺寸和图标尺寸。 |
| `ProgressBarTheme.axaml` | 普通线形控件主题、内嵌百分比伪类、垂直文本旋转、内嵌图标隐藏规则。 |
| `StepsProgressBarTheme.axaml` | 步骤线形控件主题、方向对齐和百分比位置对应图标对齐。 |
| `AbstractCircleProgressTheme.axaml` | 圆形家族共享 Token、Shape stroke 和居中状态图标 selector，不拥有 concrete Template。 |
| `CircleProgressTheme.axaml` | 圆形进度 leaf Theme 和 concrete Template，拥有静态 Semantic marker。 |
| `DashboardProgressTheme.axaml` | 仪表盘进度 leaf Theme 和 concrete Template，拥有静态 Semantic marker。 |

进度视觉由模板中的真实 Semantic target 承载，owner 继续负责全部值、尺寸和角度计算：

- 普通线形 rail/track 由静态 `Border` target 表达，可选成功段保持内部 `Border` 附加层；`LineProgressPanel`
  是这些盒视觉和 indicator 的唯一布局所有者。
- 步骤进度按 `Steps` 创建 track `Rectangle` target，并根据 `Percentage` 切换每个 target 的 active/remaining Brush；steps 不公开 rail。
- 圆形进度用静态 Shape 承载完整圆或组合分段圆弧。
- 仪表盘进度用静态 Shape 承载带缺口的连续或组合分段圆弧。
- 状态图标和百分比文本统一位于 indicator Panel 内，不参与进度几何计算。

ProgressBarToken 提供默认进度色、剩余轨道色、圆形文字和图标最小尺寸、线形文本间距、线形内部 padding 和线形图标尺寸。状态色主要来自 SharedToken。

## 6. 控件家族或集成关系

ProgressBar 属于 Feedback 控件，与 Spin、Skeleton、Alert、Message 等共同表达系统反馈。ProgressBar 展示确定或不确定进度，Spin 展示等待中状态，Skeleton 展示内容加载占位，Alert / Message 展示结果信息。

内部家族关系：

- `AbstractProgressBar` 是 RangeBase、状态、文本、图标、动效和主题契约 owner。
- `AbstractLineProgress` 承载方向、线形模板信息、线形图标尺寸和线形伪类。
- `AbstractGeneralProgressBar` 承载普通线形进度、成功段、内外百分比布局和颜色可读性计算。
- `AbstractGeneralStepsProgressBar` 承载线形步骤块布局和步骤绘制。
- `AbstractCircleProgress` 承载圆形尺寸、圆形文本和图标布局。
- `AbstractGeneralCircleProgress` 承载圆形普通圆弧和圆形分段绘制。
- `AbstractGeneralDashboardProgress` 承载仪表盘缺口、角度和分段绘制。

ProgressBar 不参与 Form 值提交，不实现选择、输入、弹出层或集合数据模型。

## 7. 兼容性不变量

维护 ProgressBar 时必须保持以下不变量：

- 不修改 `ProgressBar`、`StepsProgressBar`、`CircleProgress`、`DashboardProgress` 的既有 public API、默认值、Template Part、伪类和 Token 名称。
- 不删除或重命名四个 owner 的 `root`、`body`、`rail`、`track`、`indicator`，也不改变 selector route、ContractType 或 cardinality。
- `Value`、`Minimum`、`Maximum` 必须继续遵循 `RangeBase` 语义。
- `Percentage` 和所有绘制位置必须以 `(value - Minimum) / (Maximum - Minimum)` 为基准。
- `IsIndeterminate` 不得写入或重置 `Value`。
- `Status=Exception`、`Status=Success` 和 completed 状态的图标显示规则不擅自改变。
- `SuccessThreshold=NaN` 时不绘制成功段；有值时按 `[Minimum, Maximum]` 裁剪。
- `ProgressTextFormat` 只影响文本展示，不参与真实值计算。
- `PercentPosition` 只影响百分比文本和图标位置，不改变进度值。
- `StepsProgressBar` 的 chunk 绘制数量必须由 `Percentage` 推导，不直接按 `Value` 推导。
- `CircleProgress` 和 `DashboardProgress` 的角度必须继续由共享进度比例推导。
- `IsMotionEnabled=false` 必须禁用默认 transition。
- 重新套用模板后必须重新获取文本和图标 template part，并按当前状态刷新进度。
- Token 名称和语义不擅自重命名、删除或迁移为实例属性。

如果实现某项能力时无法保持这些不变量，应先停止实现，说明原因、影响范围、替代方案和迁移方式，并获得授权。

## 8. 专项模型

### 8.1 PercentPosition 模型

`PercentPosition` 只服务普通 `ProgressBar`。它由 `IsInner` 和 `Alignment` 组成：

- `IsInner=false`：百分比文本在进度条外部，按起点、中间、终点布局。
- `IsInner=true`：百分比文本位于已完成进度区域内，线形进度会根据当前 indicator rect 计算文本位置。
- 垂直内嵌模式下，文本通过 `LayoutTransformControl` 旋转为横向可读文本。

`StepsProgressBar.PercentPosition` 使用 `LinePercentAlignment`，不支持 `IsInner`。

### 8.2 成功阈值模型

`SuccessThreshold` 表示在同一个 `Minimum` / `Maximum` 坐标系中的成功段终点。绘制前会裁剪到当前范围内，然后复用共享进度比例计算成功段长度或角度。

成功阈值不改变 `Status`，也不影响 `Value`。它只是额外绘制一段使用 `SuccessStrokeBrush` 的进度视觉。

### 8.3 分段模型

分段模型有两类：

- `StepsProgressBar.Steps`：线形 chunk 数量，按 `Percentage` 决定已完成 chunk 数。
- `CircleProgress.StepCount` / `DashboardProgress.StepCount`：圆弧分段数量，按角度和 `StepGap` 绘制离散弧段。

分段数量为 0 的圆形和仪表盘控件使用连续圆弧。线形步骤进度的 `Steps` 最小值为 1。

### 8.4 仪表盘缺口模型

`DashboardProgress` 使用 `DashboardGapPosition` 和 `GapDegree` 计算绘制起始角和总跨度。`GapDegree` 被限制在 `[0, 295]`，保证仪表盘仍保留可见进度弧。

### 8.5 Semantic Part

`ProgressBar`、`StepsProgressBar`、`CircleProgress` 和 `DashboardProgress` 各自拥有独立 descriptor，并对齐上游 Progress
语义结构的 `root`、`body`、`rail`、`track`、`indicator` 命名集：

| Public owner | Part | 职责摘要 |
| --- | --- | --- |
| 四个 public owner | `root` | 对应控件实例，承载 public 状态、尺寸、颜色和样式作用域。 |
| 四个 public owner | `body` | 承载进度几何与信息区域的稳定主体布局。 |
| `ProgressBar`、`CircleProgress`、`DashboardProgress` | `rail` | 表达未完成轨道；官方 steps 模式不存在 rail。 |
| 四个 public owner | `track` | 表达当前进度；Steps 的全部 runtime Rectangle 都属于 track。 |
| 四个 public owner | `indicator` | 统一承载百分比、成功图标和异常图标的替代呈现。 |

完整 Selector、Style Type、ContractType、数量语义、尺寸基线、官方 Gallery 映射与排除边界见
[ProgressBar Semantic Part 契约](semantic-part.md)。

## 9. 文档导航与验证策略

关联文档：

- [ProgressBar 桌面版实现原理](implementation.md)
- [ProgressBar Semantic Part 契约](semantic-part.md)
- [ProgressBar Token 设计](token.md)
- [ProgressBar Changelog](changelog.md)

验证策略：

| 层次 | 验证内容 |
| --- | --- |
| Public API | 检查四个具体控件、控件文档 API 摘要和源码属性保持一致。 |
| 行为状态 | 验证 RangeBase 值、`Percentage`、`IsIndeterminate`、completed、status、success threshold 和 disabled。 |
| AXAML | 验证四类主题入口、template part、Semantic marker、伪类 selector、状态图标和内嵌百分比布局。 |
| Token | 验证 `ProgressBarTokenKind`、主题引用和 Token 类型、生成数据和 token.md 语义说明一致。 |
| 渲染 | 验证水平、垂直、步骤、圆形、仪表盘、分段、线帽、渐变画刷，以及线形 `Border`、步骤 `Rectangle`、圆形/仪表盘 `Shape` target。 |
| 文档 | 运行 `git diff --check`，并检查控件文档相对链接存在。 |
