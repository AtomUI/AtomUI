# ProgressBar Semantic Part 契约

本文档定义 `ProgressBar`、`StepsProgressBar`、`CircleProgress` 和 `DashboardProgress` 对应用公开的 Semantic Part、
Selector、类型约束、数量语义和定制边界。ProgressBar 的整体设计见
[ProgressBar 桌面版架构设计](overview.md)，真实模板节点、绘制单元和尺寸协调见
[ProgressBar 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Semantic Parts

四个 public 控件各自拥有独立 descriptor。Part 名称与上游 Progress Semantic DOM 保持一致：
`root`、`body`、`rail`、`track`、`indicator`。官方 `steps` 模式明确不存在 rail，因此 `StepsProgressBar` 只声明
`root`、`body`、`track`、`indicator`；不得为统一表面虚构一个始终不存在的 rail。其余同名 Part 表达一致的产品职责，
不表示四个 owner 共享运行时节点、几何或状态。

### 1.1 `ProgressBar`

#### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `ProgressBar` |
| Part | `root` |
| Selector | ProgressBar 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `ProgressBar` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | 普通线形进度 owner |
| 职责 | 承载进度范围、状态、尺寸、方向、文本位置和 Semantic Style 作用域。 |
| 相关 API | 全部 `ProgressBar` public API |
| 相关 Token | ProgressBarToken、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `body`

| 字段 | 值 |
| --- | --- |
| Owner | `ProgressBar` |
| Part | `body` |
| Selector | `.semantic-body` |
| SelectorRoute | `/template/ .semantic-body` |
| Style Type | `ProgressBarBodyStyle` |
| ContractType | `Panel` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | 线形进度主体布局 |
| 职责 | 统一承载 rail、track、成功段和 indicator 的布局边界。 |
| 相关 API | `Orientation`、`PercentPosition`、`IsProgressInfoVisible` |
| 相关 Token | `LineExtraInfoMargin`、`LineProgressPadding` |
| 稳定性 | stable since 6.2.0 |

#### `rail`

| 字段 | 值 |
| --- | --- |
| Owner | `ProgressBar` |
| Part | `rail` |
| Selector | `.semantic-rail` |
| SelectorRoute | `/template/ .semantic-body > .semantic-rail` |
| Style Type | `ProgressBarRailStyle` |
| ContractType | `Border` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | 未完成轨道盒视觉 |
| 职责 | 表达线形进度的完整剩余轨道。 |
| 相关 API | `TrailColor`、`StrokeLineCap`、`IndicatorThickness` |
| 相关 Token | `RemainingColor` |
| 稳定性 | stable since 6.2.0 |

#### `track`

| 字段 | 值 |
| --- | --- |
| Owner | `ProgressBar` |
| Part | `track` |
| Selector | `.semantic-track` |
| SelectorRoute | `/template/ .semantic-body > .semantic-track` |
| Style Type | `ProgressBarTrackStyle` |
| ContractType | `Border` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | 当前进度盒视觉 |
| 职责 | 表达由 `Value` 计算出的线形已完成区域。 |
| 相关 API | `Value`、`StrokeBrush`、`StrokeLineCap`、`IndicatorThickness` |
| 相关 Token | `DefaultColor`、SharedToken 状态色 |
| 稳定性 | stable since 6.2.0 |

#### `indicator`

| 字段 | 值 |
| --- | --- |
| Owner | `ProgressBar` |
| Part | `indicator` |
| Selector | `.semantic-indicator` |
| SelectorRoute | `/template/ .semantic-body > .semantic-indicator` |
| Style Type | `ProgressBarIndicatorStyle` |
| ContractType | `Panel` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | 百分比或状态信息区域 |
| 职责 | 统一承载格式化百分比、成功图标和异常图标的替代呈现。 |
| 相关 API | `IsProgressInfoVisible`、`ProgressTextFormat`、`PercentPosition`、`Status`、完成图标 API |
| 相关 Token | 文本、图标尺寸和状态色 Token |
| 稳定性 | stable since 6.2.0 |

### 1.2 `StepsProgressBar`

#### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `StepsProgressBar` |
| Part | `root` |
| Selector | StepsProgressBar 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `StepsProgressBar` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | 步骤线形进度 owner |
| 职责 | 承载进度范围、步骤数量、逐步画刷、尺寸、方向和 Semantic Style 作用域。 |
| 相关 API | 全部 `StepsProgressBar` public API |
| 相关 Token | ProgressBarToken、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `body`

| 字段 | 值 |
| --- | --- |
| Owner | `StepsProgressBar` |
| Part | `body` |
| Selector | `.semantic-body` |
| SelectorRoute | `/template/ .semantic-body` |
| Style Type | `StepsProgressBarBodyStyle` |
| ContractType | `Panel` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | 步骤进度主体布局 |
| 职责 | 统一承载步骤 track 和 indicator；steps 不创建 rail。 |
| 相关 API | `Orientation`、`Steps`、`PercentPosition`、`IsProgressInfoVisible` |
| 相关 Token | `LineExtraInfoMargin` |
| 稳定性 | stable since 6.2.0 |

#### `track`

| 字段 | 值 |
| --- | --- |
| Owner | `StepsProgressBar` |
| Part | `track` |
| Selector | `.semantic-track` |
| SelectorRoute | `/template/ .semantic-body > .semantic-track` |
| Style Type | `StepsProgressBarTrackStyle` |
| ContractType | `Rectangle` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | 全部步骤块 |
| 职责 | 表达全部步骤单元；完成状态只决定每个 target 使用进度色还是 rail 色。 |
| 相关 API | `Value`、`Steps`、`StepsStrokeBrush`、`StrokeBrush`、`TrailColor` |
| 相关 Token | `DefaultColor`、`RemainingColor`、SharedToken 状态色 |
| 稳定性 | stable since 6.2.0 |

#### `indicator`

| 字段 | 值 |
| --- | --- |
| Owner | `StepsProgressBar` |
| Part | `indicator` |
| Selector | `.semantic-indicator` |
| SelectorRoute | `/template/ .semantic-body > .semantic-indicator` |
| Style Type | `StepsProgressBarIndicatorStyle` |
| ContractType | `Panel` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | 百分比或状态信息区域 |
| 职责 | 统一承载步骤进度的百分比、成功图标和异常图标。 |
| 相关 API | `IsProgressInfoVisible`、`ProgressTextFormat`、`PercentPosition`、`Status` |
| 相关 Token | 文本、图标尺寸和状态色 Token |
| 稳定性 | stable since 6.2.0 |

### 1.3 `CircleProgress`

#### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `CircleProgress` |
| Part | `root` |
| Selector | CircleProgress 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `CircleProgress` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | 圆形进度 owner |
| 职责 | 承载进度范围、圆形尺寸、分段状态和 Semantic Style 作用域。 |
| 相关 API | 全部 `CircleProgress` public API |
| 相关 Token | ProgressBarToken、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `body`

| 字段 | 值 |
| --- | --- |
| Owner | `CircleProgress` |
| Part | `body` |
| Selector | `.semantic-body` |
| SelectorRoute | `/template/ .semantic-body` |
| Style Type | `CircleProgressBodyStyle` |
| ContractType | `Panel` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | 圆形进度主体布局 |
| 职责 | 统一承载圆形 rail、track、成功弧段和居中 indicator。 |
| 相关 API | `SizeType`、`Width`、`Height`、`StepCount`、`StepGap` |
| 相关 Token | 圆形尺寸与信息 Token |
| 稳定性 | stable since 6.2.0 |

#### `rail`

| 字段 | 值 |
| --- | --- |
| Owner | `CircleProgress` |
| Part | `rail` |
| Selector | `.semantic-rail` |
| SelectorRoute | `/template/ .semantic-body > .semantic-rail` |
| Style Type | `CircleProgressRailStyle` |
| ContractType | `Shape` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | 圆形未完成轨道几何 |
| 职责 | 表达连续圆或分段圆的完整剩余轨道。 |
| 相关 API | `TrailColor`、`IndicatorThickness`、`StepCount`、`StepGap` |
| 相关 Token | `RemainingColor` |
| 稳定性 | stable since 6.2.0 |

#### `track`

| 字段 | 值 |
| --- | --- |
| Owner | `CircleProgress` |
| Part | `track` |
| Selector | `.semantic-track` |
| SelectorRoute | `/template/ .semantic-body > .semantic-track` |
| Style Type | `CircleProgressTrackStyle` |
| ContractType | `Shape` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | 圆形当前进度几何 |
| 职责 | 表达由 `Value` 计算出的连续或分段圆弧。 |
| 相关 API | `Value`、`StrokeBrush`、`StrokeLineCap`、`IndicatorThickness`、`StepCount`、`StepGap` |
| 相关 Token | `DefaultColor`、SharedToken 状态色 |
| 稳定性 | stable since 6.2.0 |

#### `indicator`

| 字段 | 值 |
| --- | --- |
| Owner | `CircleProgress` |
| Part | `indicator` |
| Selector | `.semantic-indicator` |
| SelectorRoute | `/template/ .semantic-body > .semantic-indicator` |
| Style Type | `CircleProgressIndicatorStyle` |
| ContractType | `Panel` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | 圆心信息区域 |
| 职责 | 统一承载圆心百分比、成功图标和异常图标。 |
| 相关 API | `IsProgressInfoVisible`、`ProgressTextFormat`、`Status`、完成图标 API |
| 相关 Token | `CircleMinimumTextFontSize`、`CircleMinimumIconSize` |
| 稳定性 | stable since 6.2.0 |

### 1.4 `DashboardProgress`

#### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `DashboardProgress` |
| Part | `root` |
| Selector | DashboardProgress 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `DashboardProgress` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | 仪表盘进度 owner |
| 职责 | 承载进度范围、缺口、尺寸、分段状态和 Semantic Style 作用域。 |
| 相关 API | 全部 `DashboardProgress` public API |
| 相关 Token | ProgressBarToken、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `body`

| 字段 | 值 |
| --- | --- |
| Owner | `DashboardProgress` |
| Part | `body` |
| Selector | `.semantic-body` |
| SelectorRoute | `/template/ .semantic-body` |
| Style Type | `DashboardProgressBodyStyle` |
| ContractType | `Panel` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | 仪表盘进度主体布局 |
| 职责 | 统一承载仪表盘 rail、track、成功弧段和居中 indicator。 |
| 相关 API | `SizeType`、`Width`、`Height`、`DashboardGapPosition`、`GapDegree` |
| 相关 Token | 圆形尺寸与信息 Token |
| 稳定性 | stable since 6.2.0 |

#### `rail`

| 字段 | 值 |
| --- | --- |
| Owner | `DashboardProgress` |
| Part | `rail` |
| Selector | `.semantic-rail` |
| SelectorRoute | `/template/ .semantic-body > .semantic-rail` |
| Style Type | `DashboardProgressRailStyle` |
| ContractType | `Shape` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | 仪表盘未完成轨道几何 |
| 职责 | 表达带指定缺口的连续或分段剩余轨道。 |
| 相关 API | `TrailColor`、`IndicatorThickness`、`DashboardGapPosition`、`GapDegree`、`StepCount`、`StepGap` |
| 相关 Token | `RemainingColor` |
| 稳定性 | stable since 6.2.0 |

#### `track`

| 字段 | 值 |
| --- | --- |
| Owner | `DashboardProgress` |
| Part | `track` |
| Selector | `.semantic-track` |
| SelectorRoute | `/template/ .semantic-body > .semantic-track` |
| Style Type | `DashboardProgressTrackStyle` |
| ContractType | `Shape` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | 仪表盘当前进度几何 |
| 职责 | 表达由 `Value` 计算出的带缺口连续或分段圆弧。 |
| 相关 API | `Value`、`StrokeBrush`、`StrokeLineCap`、`DashboardGapPosition`、`GapDegree` |
| 相关 Token | `DefaultColor`、SharedToken 状态色 |
| 稳定性 | stable since 6.2.0 |

#### `indicator`

| 字段 | 值 |
| --- | --- |
| Owner | `DashboardProgress` |
| Part | `indicator` |
| Selector | `.semantic-indicator` |
| SelectorRoute | `/template/ .semantic-body > .semantic-indicator` |
| Style Type | `DashboardProgressIndicatorStyle` |
| ContractType | `Panel` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | 仪表盘中心信息区域 |
| 职责 | 统一承载仪表盘百分比、成功图标和异常图标。 |
| 相关 API | `IsProgressInfoVisible`、`ProgressTextFormat`、`Status`、完成图标 API |
| 相关 Token | `CircleMinimumTextFontSize`、`CircleMinimumIconSize` |
| 稳定性 | stable since 6.2.0 |

所有 root 都是隐式 Part，不添加 `.semantic-root`。四个 owner 均不跨 VisualRoot，也不提供 Semantic Part Theme。
`ProgressBar`、`CircleProgress` 和 `DashboardProgress` 的非 root Part 是静态模板 target；`StepsProgressBar` 只有 track
按 `Steps` 创建、重用或清理 runtime Rectangle。

## 2. Part 说明

### 2.1 `root`

root 是对应 public 控件实例，负责 `Minimum`、`Maximum`、`Value`、状态、尺寸、颜色、方向或圆形形态，并限定其余 Part 的
Semantic Style 作用域。适合定制 owner 的 Margin、Opacity、对齐、整体 Effect 和 public 属性组合。

root 不等于当前模板的 body 布局容器、内部几何缓存或状态图标节点。应用不得通过 `.semantic-root`、抽象基类 selector 或内部实现类型访问 root。

### 2.2 `body`

body 是每个 owner 唯一的主体 `Panel`，统一承载该 owner 已声明的 rail、track、内部成功段和 indicator。它始终存在，不因
`IsProgressInfoVisible`、`Value`、状态、分段模式或方向变化而替换。

适合定制 Background、Opacity、Effect、ClipToBounds 和不改变几何基线的视觉属性。Margin、Padding、Width、Height、Min/Max
等布局 Setter 会参与 owner 的 Measure/Arrange，必须覆盖全部尺寸档和方向验证，不能用固定高度修补某一示例。

### 2.3 `rail`

rail 表示未完成轨道。普通线形有一个静态 `Border`，圆形和仪表盘各有一个静态 `Shape`；圆形分段和仪表盘分段由同一个 Shape 的组合几何表达，
不因 `StepCount` 增加视觉 target 数量。`StepsProgressBar` 不声明 rail；其未完成颜色仍由每个 track target 的状态和
`TrailColor` / `RemainingColor` 决定。

普通线形 target 是直接绘制剩余轨道的可见 `Border`，使用 `Background` 和 `CornerRadius` 定制；圆形与仪表盘 target 是直接绘制完整轨道 Geometry 的可见 Shape，使用 `Stroke` 定制。
Semantic Style 直接作用于最终可见节点，不经过 owner 绘制代理。
Circle 与 Dashboard 的 rail Shape Bounds 按 `body size - StrokeThickness` 内缩，并与 body 共享中心；该边界对应 Ant Design SVG 中
`radius = 50 - strokeWidth / 2` 的圆几何范围。不得让 Shape 铺满 body，也不得按 Geometry 的原始坐标从左上角偏移。
几何尺寸、方向、圆弧角度和步骤数量仍由 owner API 计算；Semantic Style 不替代 `IndicatorThickness`、`ChunkWidth`、
`ChunkHeight`、`StepCount`、`StepGap`、`GapDegree` 或 `DashboardGapPosition`。

### 2.4 `track`

track 表示当前进度填充。普通线形有一个静态 `Border`，圆形和仪表盘各有一个静态 `Shape`；步骤线形进度为每个步骤提供一个 runtime
Rectangle，所以 track 为 `Multiple` 且数量始终等于 `Steps`。已完成步骤消费 `StepsStrokeBrush` 或 `StrokeBrush`，未完成
步骤消费 rail 色；所有步骤 target 都响应同一个 track Semantic Style，与上游 steps 的实际 Semantic DOM 一致。

普通线形 target 是直接绘制当前填充区域的可见 `Border`；步骤形态使用真实 runtime `Rectangle`；圆形与仪表盘 target
使用直接绘制当前进度 Geometry 的可见 Shape。所有 track target 的 Semantic Style 都直接作用于最终可见节点。
Circle 与 Dashboard 的 track Shape 与 rail 使用同一个内缩 viewport 和中心点；当前进度比例只改变 Geometry，不改变 target 的布局 Bounds。
成功阈值层属于进度计算的内部附加层，不是第二个公开 track target，也不单独发布 `success` Part；
`SuccessThreshold` 和 `SuccessStrokeBrush` 仍是它的唯一 public 定制入口。

### 2.5 `indicator`

indicator 是每个 owner 唯一的信息 `Panel`，统一包裹百分比 label、成功图标和异常图标。内部三个呈现节点按状态切换可见性，
但不改变 indicator target 身份。`IsProgressInfoVisible=false` 时 indicator 隐藏但不销毁，因此 cardinality 保持 `Single`。

普通 line 和 steps 的 indicator Bounds 等于当前百分比文本或状态图标区域；circle 和 dashboard 的 indicator 是穿过圆心、
高度跟随当前文本或图标的水平信息带。它不再使用覆盖整个 owner 的布局 Canvas Bounds，因此 Gallery 高亮对应实际信息区域。

适合定制继承文本属性、Foreground、Background、Opacity、Effect 和对齐。`ProgressTextFormat`、`PercentPosition`、完成状态图标
选择与小尺寸可见性仍由 owner 控制；label、`LayoutTransformControl` 和两个 `IconPresenter` 不再分别提升为公开 Part。

## 3. Selector 用法

四个 owner 使用各自生成的 Style Type。生成类型封装 `/template/` 和 body 的直接子节点 route，应用不复制 route：

```xml
<Style Selector="atom|ProgressBar.semantic-custom">
    <atom:ProgressBarRailStyle x:SetterTargetType="Border">
        <Setter Property="Background" Value="#F0F0F0" />
        <Setter Property="CornerRadius" Value="8" />
    </atom:ProgressBarRailStyle>
    <atom:ProgressBarTrackStyle x:SetterTargetType="Border">
        <Setter Property="Background" Value="#1677FF" />
        <Setter Property="CornerRadius" Value="8" />
    </atom:ProgressBarTrackStyle>
    <atom:ProgressBarIndicatorStyle x:SetterTargetType="Panel">
        <Setter Property="TextElement.Foreground" Value="#1677FF" />
    </atom:ProgressBarIndicatorStyle>
</Style>
```

圆形 owner 的 rail/track 使用 Shape 的 Stroke：

```xml
<Style Selector="atom|CircleProgress.semantic-custom">
    <atom:CircleProgressRailStyle x:SetterTargetType="Shape">
        <Setter Property="Stroke" Value="#F0F0F0" />
    </atom:CircleProgressRailStyle>
    <atom:CircleProgressTrackStyle x:SetterTargetType="Shape">
        <Setter Property="Stroke" Value="#1677FF" />
    </atom:CircleProgressTrackStyle>
</Style>
```

不得使用以下写法：

- `.semantic-root`、`PART_*`、内部 Canvas、内部成功层或 Shape Name selector。
- `AbstractProgressBar`、`AbstractLineProgress`、`AbstractCircleProgress` 等抽象实现类型作为 public owner。
- 复制 `/template/ .semantic-body > .semantic-*` route 作为主要用户写法。
- 把步骤 runtime Shape 的索引、创建顺序或具体 Shape 子类当作稳定契约。
- 用 Semantic Style 改写 `Value`、`Steps`、角度、缺口、文本格式或状态 ownership。

## 4. 状态与数量语义

| 场景 | body | rail | track | indicator |
| --- | --- | --- | --- | --- |
| `ProgressBar` 任意值或状态 | 1 | 1 | 1 | 1，按 `IsProgressInfoVisible` 显隐 |
| `StepsProgressBar`，0 个完成步骤 | 1 | 不适用 | `Steps`，全部使用 rail 色 | 1 |
| `StepsProgressBar`，部分完成 | 1 | 不适用 | `Steps`，按完成状态选择 Brush | 1 |
| `StepsProgressBar`，全部完成 | 1 | 不适用 | `Steps`，全部使用进度色 | 1 |
| `CircleProgress.StepCount=0` | 1 | 1，连续几何 | 1，连续几何 | 1 |
| `CircleProgress.StepCount>0` | 1 | 1，组合分段几何 | 1，组合分段几何 | 1 |
| `DashboardProgress.StepCount=0` | 1 | 1，连续缺口几何 | 1，连续缺口几何 | 1 |
| `DashboardProgress.StepCount>0` | 1 | 1，组合分段缺口几何 | 1，组合分段缺口几何 | 1 |
| `IsProgressInfoVisible=false` | 1 | 不变 | 不变 | 1，隐藏 |
| status/completed 切换 | 1 | 不变 | 不变 | 1，内部呈现切换 |
| 模板重套用 | 1 个新 target | 已声明 rail 的 owner 各有 1 个新 target | 静态 owner 为 1；Steps 重建 `Steps` 个 target | 1 个新 target |

步骤 runtime target 创建时只使用生成的 track selector class 常量，`Value` 变化只更新 Brush 和布局，不更换 Part 身份。
`Steps`、模板重套用或 detach 后必须清理不再使用的 Rectangle，不保留旧 logical/visual parent、订阅或样式状态。

## 5. 尺寸与状态基线

| Owner | 默认尺寸含义 | 自动尺寸映射 | 显式覆盖 | Semantic 布局约束 |
| --- | --- | --- | --- | --- |
| `ProgressBar` | `SizeType=Large` | Large/Middle/Small track thickness 为 `8/6/4` | `IndicatorThickness`；横向 Height 或纵向 Width 可影响 effective size | body、rail、track Setter 不得破坏外部/内嵌 indicator 的预留空间 |
| `StepsProgressBar` | `SizeType=Large` | 自动 `ChunkWidth` 为 `14/6/2`；默认 `ChunkHeight=8` | `ChunkWidth`、`ChunkHeight`、`IndicatorThickness` | 自动宽度不得写回 public `ChunkWidth`，尺寸档切换必须重新计算；runtime Rectangle 间距保持 2 |
| `CircleProgress` | `SizeType=Large` | Large/Middle/Small 直径为 `120/90/60`；thickness 按实际直径计算且最小 3 | `Width`、`Height`、`IndicatorThickness` | rail/track Bounds 等于 `floor(body size - StrokeThickness)` 并与 body 同心；Geometry 在该局部视口内表达圆弧 |
| `DashboardProgress` | `SizeType=Large` | 与 Circle 相同，弧跨度再扣除 `GapDegree` | `Width`、`Height`、`IndicatorThickness`、`GapDegree` | rail/track 使用相同内缩视口和中心点，Geometry 共享缺口方向与起始角 |

最小尺寸失败回归是 `StepsProgressBar` 在 `ChunkWidth=NaN` 时从 Large 切换到 Middle/Small：当前实现把首次计算结果写回
`ChunkWidth`，导致后续尺寸档仍使用 Large 的 14。实现必须保留 `NaN` 作为“自动”状态，并从 `EffectiveSizeType` 计算
`14/6/2`；不得通过 Gallery 固定 `ChunkWidth`、Width、Height 或 Margin 绕过该回归。

## 6. 上游 Gallery 映射

Semantic Preview 严格映射官方 `_semantic.tsx`：

- 类型分段只包含 `line`、`steps`、`circle`、`dashboard`。
- 进度固定为 `80`。
- steps 固定为 `5`。
- 预览区域高度固定为 `200` 且宽度占满；circle 与 dashboard 不设置额外 size，使用控件默认尺寸。
- dashboard 不设置额外 gap degree，使用控件默认值。
- 只提供官方 `Gradient` 开关；关闭时使用默认 track，开启时使用 `0% #108ee9`、`100% #87d068`。
- 不增加状态、尺寸、文本、方向、动画或自定义颜色控制。

Semantic custom 示例严格映射官方 `style-class.tsx` 和 `style-class.md`：

- 只展示 `10`、`20`、`40`、`60`、`80`、`99` 六个线形 ProgressBar，保持同一顺序。
- 六个实例都保持默认 indicator 显示，不增加 `showInfo` 条件。
- classNames 只映射 `root`、`rail`、`track`；root padding 固定为 `2`，圆角固定为 `8`。
- 每个实例按 `hue = 200 - 2 * percent` 计算 track 从 `hsla(hue, 85%, 65%, 1)` 到
  `hsla(hue + 30, 90%, 55%, 0.95)` 的向右线性渐变。
- track 圆角固定为 `8`，颜色变化 transition 固定为 `all 0.3s ease`；rail 固定为
  `rgba(0, 0, 0, 0.1)` 和圆角 `8`。
- 外层只使用官方 `large` 间距，不增加标题文本、状态图标、隐藏条件或其他进度形态。

AtomUI 使用 owner-scoped AXAML Style 和生成的 Semantic Style Type 表达相同结果，不引入 React `classNames` / `styles` API，
也不把普通 Examples Tab 中的十九个既有示例混入 Semantic custom 示例。上游示例事实发生变化时，必须重新核对类型、默认值、
颜色、间距和 motion，再同步 Gallery 回归测试；长期契约不依赖本地上游源码路径或行号。

## 7. 定制边界

以下区域不属于 ProgressBar Semantic Part：

- `PART_LayoutTransformControl`、`PART_PercentageLabel`、两个完成状态 `IconPresenter` 及其内部 Icon。
- 成功阈值的附加视觉、Geometry、Pen、PathFigure、ArcSegment 和步骤索引。
- `EffectiveSizeType`、`StrokeThickness`、`GrooveBrush`、`IndicatorAngle` 和文本尺寸缓存。
- `ProgressBarToken`、SharedToken 资源查找、transition 与 pseudo-class 同步。
- runtime 步骤 Rectangle 的容器类型、回收策略、索引和内部 Brush precedence。

默认 ControlTheme 不消费 `.semantic-*` 作为自身样式机制。Semantic marker 不改变 hit testing、automation owner、焦点、值计算、
状态图标选择或动效时序。

## 8. 兼容性与验证

删除或重命名 Part、修改 selector class/route、收窄 `ContractType`、改变 cardinality/runtime 语义、让不同几何状态丢失 marker，
或让默认主题依赖 `.semantic-*`，均属于公共主题契约变更。

验证至少覆盖：

- `ProgressBar`、`CircleProgress`、`DashboardProgress` 各有五个 Part，`StepsProgressBar` 有四个 Part，descriptor 字段与本文一致。
- 十五个非 root 生成 Style Type 可编译并命中真实 target。
- 普通线形、圆形和仪表盘每个模板各有四个静态 marker target；普通线形 rail/track 为 `Border`，圆形/仪表盘 rail/track 为 `Shape`；Steps body/indicator 静态，track 数量始终等于 `Steps`。
- Large/Middle/Small、显式厚度、显式圆形宽高、水平/垂直、内嵌/外部 indicator 的 Measure/Arrange 基线；Circle/Dashboard rail 与 track 的 Bounds 必须比 body 小一个 `StrokeThickness` 并共享中心。
- `StepsProgressBar` 自动 `ChunkWidth` 在运行期切换尺寸档时保持 `NaN` public 状态并使用 `14/6/2` effective width。
- `Minimum != 0`、成功阈值、分段圆/仪表盘、逐步骤 Brush、status/completed 和 disabled 状态不改变既有结果。
- 步骤 runtime Rectangle 在 Value/Steps 变化、模板重套用和 detach 后数量、track marker、logical parent 与资源状态正确。
- 默认主题不消费 `.semantic-*`，生产控件不扫描 VisualTree、不查询 registry、不使用反射或动态 AXAML。
- Gallery Semantic Preview 首次选择 Semantic Parts Tab 后才创建四个 Preview 壳；VisualTree 同一时刻只接入当前类型 owner，切回
  Examples 后全部释放。四种类型与 Gradient 状态严格对应官方示例。
- Gallery Semantic custom 示例只包含官方六个百分比值和官方样式映射。
- NativeAOT 不依赖运行时类型发现、反射或动态代码。
