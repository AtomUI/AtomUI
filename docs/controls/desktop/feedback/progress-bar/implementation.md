# ProgressBar 桌面版实现原理

本文档描述 ProgressBar 家族桌面版的内部基类分层、进度归一化、Semantic target 投影、百分比文本布局、状态图标、Token 边界和维护规则。公共设计与 API 契约见 [ProgressBar 桌面版架构设计](overview.md)，公开主题区域见 [ProgressBar Semantic Part 契约](semantic-part.md)，Token 语义见 [ProgressBar Token 设计](token.md)，变化记录见 [ProgressBar Changelog](changelog.md)。

## 1. 实现定位

ProgressBar 的实现基于 Avalonia `RangeBase`。AtomUI 负责把 `Minimum`、`Maximum`、`Value` 转换为百分比、线形尺寸、圆形角度、步骤数量、状态图标和真实 Semantic target。

本文档覆盖 `AtomUI.Controls` 中的抽象基类和 `AtomUI.Desktop.Controls` 中的四个具体控件。具体几何构造、token 默认值和 AXAML selector 仍应直接阅读源码；本文只记录维护者必须理解的稳定结构、状态流和不变量。

## 2. 源码文件结构

共享实现位于 `src/AtomUI.Controls/ProgressBar/`：

- `ProgressBarEnums.cs`：`ProgressStatus`、`DashboardGapPosition`、`PercentPosition`、`LinePercentAlignment`。
- `ProgressBarPseudoClass.cs`：ProgressBar 家族使用的伪类常量。
- `AbstractProgressBar.cs`：RangeBase 集成、共享 public API、百分比计算、状态伪类、模板 part、动效入口和 visual invalidation 调度。
- `AbstractLineProgress.cs`：线形方向、线形 extra info 测量、方向伪类、线形默认状态图标。
- `AbstractGeneralProgressBar.cs`：普通线形进度的测量、布局、rail/track/success rect、内外百分比位置和百分比文字颜色。
- `LineProgressPanel.cs`：普通线形 rail/track/success `Border` 与 indicator 的唯一布局所有者。
- `AbstractGeneralStepsProgressBar.cs`：线形步骤进度的 chunk 尺寸、步骤视觉同步和百分比文本布局。
- `StepsProgressPanel.cs`：步骤 runtime Rectangle 的数量同步、Brush 投影、track/indicator 布局和 owner 订阅生命周期。
- `AbstractCircleProgress.cs`：圆形尺寸、圆形文本和状态图标布局、圆形 thickness 计算。
- `CircleProgressPanel.cs`：圆形与仪表盘 Path viewport 的统一 Measure/Arrange owner。
- `AbstractGeneralCircleProgress.cs`：圆形 rail/track/success 的连续或分段 Geometry。
- `AbstractGeneralDashboardProgress.cs`：仪表盘缺口角度和 rail/track/success Geometry。

桌面具体控件位于 `src/AtomUI.Desktop.Controls/ProgressBar/`：

- `ProgressBar.cs`：注册 `ProgressBarToken` scope 的普通线形控件入口。
- `StepsProgressBar.cs`：注册 token scope 的步骤线形控件入口。
- `CircleProgress.cs`：注册 token scope 的圆形控件入口。
- `DashboardProgress.cs`：注册 token scope 的仪表盘控件入口。
- `*.SemanticParts.cs`：四个 public owner 的静态 Semantic Part 声明。
- `ProgressBarToken.cs`：控件级 Token 默认值计算。
- `Themes/*.axaml`：共享主题、线形主题、圆形共享 style、具体 leaf Template 和静态 Semantic marker。

Gallery 示例位于 `controlgallery/AtomUIGallery/ShowCases/Feedback/ProgressBar/`。

## 3. 核心类职责

`AbstractProgressBar` 是状态 owner。它持有共享 public API、`Percentage` direct property、`EffectiveSizeType`、`StrokeThickness`、body/indicator 与状态图标 template part，并调度几何或步骤视觉刷新。所有值到比例的计算都应经过 `CalculateProgressRatio`。

`AbstractLineProgress` 负责线形进度的方向和文本测量基础。它同步 `:horizontal` / `:vertical`，并根据宽高变化重新计算 effective size type。

`AbstractGeneralProgressBar` 负责普通线形进度条。它根据 orientation 和 `PercentPosition` 计算 rail、track、success 和额外信息 rect，
并在内嵌文本模式下选择可读文本颜色。`LineProgressPanel` 消费这些计算结果，将静态 `Border` target 和 indicator 排列到最终 Bounds；
owner 不直接对子节点调用 `Measure` 或 `Arrange`。

`AbstractGeneralStepsProgressBar` 负责步骤线形进度。它以 `Steps`、`ChunkWidth`、`ChunkHeight` 和固定间距计算总尺寸，并把每个步骤投影为 runtime track `Rectangle`。全部步骤 target 始终保留 track marker，完成状态只切换 Brush，逐项完成色继续消费 `StepsStrokeBrush`。
`StepsProgressPanel` 消费步骤轨道和额外信息 rect，并作为 track 与 indicator 的唯一布局所有者；owner 不在自身
`ArrangeOverride` 中再次排列 indicator。Steps 百分比始终位于轨道外部，标签直接继承 owner 的 `Foreground`；
只有支持内嵌百分比的普通 line 使用 `PercentageLabelColor` 做背景可读性计算。

`AbstractCircleProgress` 负责圆形尺寸与中心信息布局。它根据 `SizeType`、`Width`、`Height` 和 `IndicatorThickness` 计算圆形实际尺寸和 stroke thickness，并设置圆形内部文本 / 图标大小。Circle 与 Dashboard 模板中的 `CircleProgressPanel` 是 rail、track、success `Path` 的唯一布局所有者；三个 Path 使用 `body size - StrokeThickness` 的居中视口，Geometry 只描述该局部视口内部的连续或分段圆弧。owner 不直接对子 Path 调用 `Measure` 或 `Arrange`。

`AbstractGeneralCircleProgress` 和 `AbstractGeneralDashboardProgress` 负责圆弧 Geometry。它们把进度比例转换为角度，并根据 `StepCount` / `StepGap` 决定连续 PathGeometry 或组合分段 Geometry，再投影到静态 Shape。

四个桌面 public 控件的 C# 主类只注册 token scope，不持有额外运行时状态；`*.SemanticParts.cs` 只提供生成期公共主题元数据。

## 4. 状态与数据流

共享数据流：

```text
RangeBase.Minimum / Maximum / Value
      ↓
AbstractProgressBar.CalculateProgressRatio(value)
      ↓
Percentage
      ↓
NotifyUpdateProgress()
      ↓
text / angle / step visual / geometry invalidation
```

`CalculateProgressRatio` 是进度归一化的唯一共享入口。维护线形长度、成功阈值、圆形角度、仪表盘角度和步骤数量时，必须使用该比例或已由该比例计算出的 `Percentage`。

状态流：

```text
Value change
  ↓
UpdateProgress()
  ↓
Percentage + NotifyUpdateProgress()
  ↓
IsCompleted + pseudo classes
```

`IsCompleted` 是 internal visual state，按 `Value == Maximum` 更新。它驱动 completed 主题 selector、状态图标显示和测量刷新。

`TrailColor` 是 public 覆盖入口。设置后写入 internal `GrooveBrush`；清空后释放该 local value，让主题资源重新生效。

`SizeType` 写入 `EffectiveSizeType`。线形控件还会根据显式宽高在模板应用时重新计算 effective size type。圆形控件则用 `SizeType` 推导默认圆形尺寸。

## 5. 生命周期与模板接入

`AbstractProgressBar.OnApplyTemplate` 获取共享信息 template part：

- `PART_LayoutTransformControl`
- `PART_PercentageLabel`
- `PART_ExceptionCompletedIconPresenter`
- `PART_SuccessCompletedIconPresenter`

普通线形和圆形抽象实现分别获取 concrete leaf Template 中的 body、rail、track、success target；Steps 获取
`StepsProgressPanel` body。获取 part 后立即调用 `NotifyEffectSizeTypeChanged()`、`UpdateProgress()` 或对应 visual refresh，保证模板创建后
尺寸、文本、状态和 Geometry 同步。

默认 concrete Template 中的 rail、track 和内部 success target 是最终绘制节点：普通线形使用 `Border`，圆形和仪表盘使用
`Path`，步骤形态使用 runtime `Rectangle`。`AbstractProgressBar.Render` 只在旧自定义模板没有提供对应模板视觉时执行兼容回退；
默认模板不会同时执行 owner 直接绘制和模板 target 绘制。Semantic Style 与 Gallery 高亮因此共享同一个可见目标，不存在透明样式代理或额外属性桥接。

`AbstractLineProgress.OnApplyTemplate` 在共享模板接入后：

- 根据显式 `Height` 或 `Width` 计算线形 effective size type。
- 计算 `_extraInfoSize`。
- 同步方向伪类。
- 为线形状态图标设置默认 `CloseCircleFilled` 和 `CheckCircleFilled`。

`AbstractCircleProgress.OnApplyTemplate` 获取 rail/track/success Path，刷新 Geometry，并为圆形状态图标设置默认
`CloseOutlined` 和 `CheckOutlined`。

圆形主题采用 concrete leaf Template ownership：`AbstractCircleProgressTheme` 只维护共享 Token 和按 `PART_*` Name 命中的
Shape/Icon selector；`CircleProgressTheme` 与 `DashboardProgressTheme` 各自声明完整 `ControlTemplate` 和静态 Semantic marker。
生成器据此直接验证每个 public owner 的模板契约，不依赖抽象 Theme 中可继承但无法归属到 concrete owner 的模板。修改圆形主体结构时
必须同步两个 leaf Template，并保持同名 body/rail/track/indicator route 一致。`CircleProgressPanel` 根据最终 body size 与
`StrokeThickness` 计算居中的 Path viewport；Geometry 使用从 `(0, 0)` 开始的局部坐标。不得改回由 `Canvas` 按 Geometry DesiredSize
从左上角排列，也不得由 owner 重复排列 Path，否则 rail/track 的内缩量或中心会与 body 分离。

构造阶段的 `TransitionInitializationSession` 通过公开属性覆盖抑制自动过渡，`OnLoaded` 在当前基线提交后完成初始化；
detach 复位会话，重挂载仍直接呈现当前进度并恢复最新 Transitions 来源。此路径不调用框架私有开关或投递开启任务。

普通线形、圆形和仪表盘只重新获取静态模板 target，不建立事件订阅或 C# relay binding。Steps 在模板应用、`Steps`、`Value`、
尺寸和方向变化时同步 runtime Rectangle；重套模板或 detach 时从旧 body 清理步骤节点及其 logical/visual parent。

ProgressBar 家族没有 popup、async 或外部资源 host 生命周期。Semantic marker 由静态 AXAML 或 concrete owner 生成常量建立，
不扫描 VisualTree，不查询运行时 registry，也不组装 selector 字符串。

## 6. 交互与事件处理

ProgressBar 是展示型控件，没有 pointer、keyboard、focus 编辑、选择、拖拽、popup 或集合事件模型。

可观察行为来自属性变化：

- `Value`、`Minimum`、`Maximum`、`ProgressTextFormat` 改变时刷新 `Percentage` 和文本。
- `Value` 改变时同步 completed 伪类和 internal completed 状态。
- `IsIndeterminate` 改变时同步 `:indeterminate`。
- `Width` / `Height` 改变时触发额外信息可见性和圆形尺寸刷新。
- `PercentPosition` 改变时同步内嵌文本伪类。
- `Orientation` 改变时同步方向伪类并重新计算最小厚度。

## 7. 内部算法与关键流程

### 7.1 进度归一化

所有进度几何都从同一公式出发：

```text
ratio = (value - Minimum) / (Maximum - Minimum)
Percentage = ratio * 100
```

当 `Maximum` 和 `Minimum` 几乎相等时，比例退化为 1.0。成功阈值绘制前会裁剪到 `[Minimum, Maximum]`，再进入同一比例计算。

### 7.2 线形进度盒视觉

`AbstractGeneralProgressBar` 提供 groove、track、success 和 indicator 的纯几何计算。模板 body 使用 `LineProgressPanel`，由它在自己的
`ArrangeOverride` 中把 rail、track、success `Border` 和 indicator 排列到这些 rect。线形模板只有这一处子节点布局入口；owner
只在值、范围、方向、尺寸、文本位置、成功阈值或线帽变化时失效 panel，不在自身布局阶段二次测量或排列模板子节点。

rail/track/success 使用 `Border.Background` 绘制盒视觉，圆角由 `Border.CornerRadius` 表达。它们不是 Avalonia `Shape`，因此不使用
`Stretch` 或 `RenderedGeometry`，也不会在 Bounds 之外再次执行几何缩放。track 根据 groove 长度和当前比例得到最终 Bounds；过小的
track 仍按 `StrokeLineCap` 阈值隐藏，避免显示不可见的小块。

当 `SuccessThreshold` 有效时，panel 按同一轨道基线排列内部 success `Border`。该节点不带公开 Semantic marker，
`SuccessStrokeBrush` 继续是唯一颜色入口。`StrokeLineCap=Round` 时 rail/track/success 使用半厚度圆角，其他线帽使用直角。

### 7.3 线形百分比布局

普通线形控件使用 `GetProgressBarRect` 和 `GetExtraInfoRect` 配合计算进度条区域和文本区域。

外部百分比模式会为 label 或状态图标预留起点、中间或终点空间。内嵌模式会先计算当前 indicator rect，再把文本放在已完成进度区域内。垂直内嵌模式会交换文本尺寸，并通过主题旋转文本。

line 和 steps 的 indicator Panel 分别由 `LineProgressPanel` 和 `StepsProgressPanel` 直接排列到 `GetExtraInfoRect`；circle 和 dashboard 的 indicator Panel 使用 owner 宽度并按当前
文本或状态图标高度垂直居中。Panel 的 Bounds 表达可见信息区域，不承担整个 owner 的坐标容器职责。

内嵌文本颜色由当前背景色可读性决定。当前实现只对 solid brush 进行可读性计算；渐变画刷不会参与该颜色推导。

### 7.4 步骤线形视觉

`AbstractGeneralStepsProgressBar` 使用固定 `DEFAULT_CHUNK_SPACE` 作为步骤间距。步骤视觉数量由 `Steps` 决定，已完成数量按
`Math.Round(Steps * Percentage / 100)` 计算。每个 runtime Rectangle 创建时加入生成的 track marker；完成状态只切换 Brush，
不切换 Part 身份。`StepsProgressBar` 不创建或声明 rail target。

`StepsProgressPanel.ArrangeOverride` 在同一轮布局中排列全部 track 和 indicator。不得在 owner 再增加 indicator 的手动
`Measure` / `Arrange`，否则延迟内容首次加入已附着 VisualTree 时会因父子排列顺序产生不同结果，并在后续 Tab 往返或二次布局后
才偶然恢复。Steps label 不绑定普通 line 专用的 `PercentageLabelColor`，而是从 owner 继承 `Foreground`，保证默认、禁用和
主题覆盖状态下都具有可见文字颜色。

`StepsStrokeBrush` 可以为已完成 chunk 提供逐项画刷。索引超出提供数量时回退到 `StrokeBrush`。

`ChunkWidth=NaN` 表示自动宽度，必须始终从 `EffectiveSizeType` 读取 `14/6/2`，不能把计算值写回 public property。
`ChunkHeight` 缺省时使用 `StrokeThickness`。`ChunkHeight` 和 `IndicatorThickness` 双向同步是当前兼容行为，维护时不得用
suppression flag 打断该状态流。

### 7.5 圆形进度 Geometry

圆形进度计算当前圆形 rect，并向内 deflate 半个 stroke thickness。普通模式为 rail Shape 生成完整椭圆 Geometry；分段模式按
`(360 - StepGap * StepCount) / StepCount` 生成组合分段 Geometry。两种状态始终复用同一个 rail target。

`NotifyUpdateProgress` 把当前 `Value` 转换成 `IndicatorAngle`。普通 track Geometry 从 -90 度开始；分段 track Geometry 按已完成段
数量组合多个 arc。两种状态始终复用同一个 track target。

成功阈值投影到内部 success Shape；普通模式使用单一 arc，分段模式使用组合成功段 Geometry。

### 7.6 仪表盘 Geometry

仪表盘使用 `DashboardGapPosition` 和 `GapDegree` 计算起始角和总跨度：

- Bottom：从底部缺口两侧开始。
- Left / Top / Right：按对应方向旋转缺口。
- 总跨度为 `360 - GapDegree`。

普通模式生成一个带缺口的 arc Geometry。分段模式按 `(360 - GapDegree - StepGap * StepCount) / StepCount` 计算每段角度并
生成组合 Geometry。rail/track/success target 数量保持稳定。

`GapDegree` 改变和 `DashboardGapPosition` 改变时重新计算角度 pair。`Value` 改变时只更新 `IndicatorAngle`。

## 8. 资源、性能与 AOT 边界

ProgressBar 不依赖运行时反射、字符串 binding、动态类型扫描或 C# relay binding。模板状态主要通过 template part、
`TemplateBinding`、style selector、线形盒布局、Shape Geometry 和有界的步骤视觉同步完成。

资源边界：

- 默认颜色来自 `ProgressBarToken` 和 SharedToken。
- `TrailColor` 会写入 `GrooveBrush` local value；清空后恢复主题资源。
- 状态图标由 `IconPresenter` 承载，默认图标在模板应用后以 template priority 设置。

性能边界：

- 普通线形、圆形和仪表盘始终复用固定数量模板 target，不按 Value 或 StepCount 创建节点。
- 圆形和仪表盘复用 Geometry/Path target，只在尺寸、角度、分段或颜色相关状态变化时更新。
- 只有 `StepsProgressBar` 按 `Steps` 建立有界 runtime Rectangle；Value 变化复用节点，不重建整个列表。
- 文本尺寸通过 `_extraInfoSize` 缓存，尺寸、状态或文本相关属性变化时刷新。
- 动效只绑定 `Value`、`StrokeBrush` 和 `Foreground` transition。

AOT 边界：

- 不新增反射读取 public API、token 或 template part。
- 新增 Token 必须走 `ProgressBarToken` 和 generator 支持的 token kind。
- API 与 Token 契约应在控件文档、源码 public surface、Token 类型或生成数据中维护，不依赖运行时扫描。

## 9. 维护不变量

内部重构必须保持以下不变量：

- `AbstractProgressBar` 是进度比例、状态伪类、completed 状态和共享 template part 的 owner。
- 所有 value-to-geometry 算法必须使用 `CalculateProgressRatio` 或 `Percentage`。
- 线形和圆形成功阈值必须与普通进度使用同一个 `Minimum` / `Maximum` 坐标系。
- `IsIndeterminate` 只更新伪类，不改变真实进度值。
- `IsProgressInfoVisible=false` 不影响进度绘制。
- `Status=Exception` 非内嵌线形场景显示异常图标，不格式化百分比文本。
- completed 状态的文本隐藏、图标显示和成功色 selector 不擅自改动。
- 线形方向伪类和内嵌 label 伪类必须跟随属性变化同步。
- 圆形控件显式 `Width` / `Height` 和 stretch alignment 仍参与最终圆形尺寸计算。
- `StepCount=0` 的圆形和仪表盘必须保持连续圆弧模式。
- `StepsProgressBar.Steps` 最小值为 1，chunk 尺寸最小值为 1。
- `ChunkWidth=NaN` 必须保持自动状态；运行期 SizeType 变化不能遗留首次 Large 计算值。
- 四个 owner 已声明的 body/rail/track/indicator marker 必须与 descriptor route、ContractType 和 cardinality 一致。
- rail/track marker 必须位于直接绘制最终视觉的可见 target：普通线形使用 `Border`，圆形和仪表盘使用 `Shape`；indicator marker 必须保持非零的实际信息 Bounds。
- 成功阈值附加视觉、label、状态 IconPresenter 和几何实现类型不得提升为额外公开 Part。
- 新增 binding、event handler 或 resource host 时必须定义释放位置。

## 10. Semantic Part 映射

| Owner | body | rail | track | indicator |
| --- | --- | --- | --- | --- |
| `ProgressBar` | 静态 `LineProgressPanel` | 静态 Border | 静态 Border | 静态 Panel |
| `StepsProgressBar` | 静态 Panel | 不声明 | runtime Rectangle，数量始终等于 Steps | 静态 Panel |
| `CircleProgress` | 静态 Panel | 静态 Shape，连续或组合分段 Geometry | 静态 Shape，连续或组合分段 Geometry | 静态 Panel |
| `DashboardProgress` | 静态 Panel | 静态 Shape，连续或组合分段缺口 Geometry | 静态 Shape，连续或组合分段缺口 Geometry | 静态 Panel |

四个 concrete owner 的同名 Part 不跨 owner 复用；`StepsProgressBar` 按官方 steps Semantic DOM 不声明 rail。
静态 marker 使用 `Classes.semantic-*="True"`；Steps runtime track marker 使用 `StepsProgressBarSemanticParts.TrackClass`，
不写重复字符串。

body 是 owner 模板 target；已声明的 rail、track 和 indicator 是 body 的直接 logical child，descriptor route 固定为
`/template/ .semantic-body > .semantic-<part>`。默认 ControlTheme 不消费 `.semantic-*`，只使用既有属性、伪类、Name 和 Token。

## 11. 测试与验证

现有验证入口：

- `tests/AtomUI.Desktop.Controls.Tests/ProgressBar/ProgressBarBehaviorTests.cs`
- `tests/AtomUIGallery.Tests/ShowCases/ProgressBarShowCasePageTests.cs`
- `tests/AtomUIGallery.Tests/ShowCases/ProgressBarShowCaseExamples.snapshot`

重点验证场景：

- `Minimum != 0` 时 `Percentage`、线形成功段、内嵌百分比位置、圆形角度和仪表盘角度。
- `IsIndeterminate` 运行期切换后的 `:indeterminate` 伪类。
- `Status=Normal/Active/Success/Exception` 与 completed 状态的图标和颜色。
- `IsProgressInfoVisible=false` 下文本和图标隐藏但进度仍绘制。
- `Orientation=Horizontal/Vertical` 下线形和步骤布局。
- `PercentPosition` 内外、起点、中间、终点布局。
- `Steps`、`StepCount`、`StepGap`、`ChunkWidth`、`ChunkHeight`。
- `DashboardGapPosition` 和 `GapDegree`。
- `StrokeLineCap`、渐变 `StrokeBrush`、`TrailColor`、`SuccessStrokeBrush`。
- `SizeType`、显式 `IndicatorThickness`、显式圆形宽高。
- `StepsProgressBar.ChunkWidth=NaN` 时 Large/Middle/Small 自动宽度为 `14/6/2`，运行期切换不会写回 public property。
- Steps 示例延迟加入已附着 VisualTree 后，水平/垂直 Start、Center、End indicator 在第一轮布局即位于正确轨道外侧或中间位置，不依赖 Tab 往返触发二次布局。
- Steps 百分比 label 在水平/垂直 Start、Center、End 下具有格式化文本、非零布局和非透明 owner `Foreground`。
- 四个 descriptor、十五个生成 Style Type、静态 marker、Steps runtime track marker 和 selector 命中。
- body/rail/track/indicator 的布局型 Setter 不破坏 owner Measure/Arrange、内嵌 indicator 或圆形中心点。
- 默认 line 的 root/body 覆盖轨道与 indicator，rail 只覆盖完整轨道，track 只覆盖完成区域，indicator 只覆盖百分比或状态信息。
- 线形 rail、track、success 必须是 `Border`，其 Bounds 在初次布局、状态变化和 resize 后与 owner 几何计算一致；模板中不得重新引入 `Shape.Stretch`，owner 不得手动布局 panel 子节点。
- 上游官方 Semantic Preview 的四种类型、80%、5 steps、Gradient 开关、toolbar 交叉轴居中和 Semantic custom 六个百分比值。
- disabled 状态和 `IsMotionEnabled=false`。

文档改动至少运行 `git diff --check`，并检查 `overview.md`、`implementation.md`、`token.md`、`changelog.md` 之间的相对链接存在。
