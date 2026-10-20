# ToggleSwitch 桌面版实现原理

本文档描述 ToggleSwitch 桌面版的内部布局、状态流转、模板接入、加载动画、内容绑定、Form 和 Token 使用。公共设计与 API 契约见 [ToggleSwitch 桌面版架构设计](overview.md)，Semantic Part 契约见 [ToggleSwitch Semantic Part 契约](semantic-part.md)，Token 语义见 [ToggleSwitch Token 设计](token.md)，变化记录见 [ToggleSwitch Changelog](changelog.md)。

## 1. 实现定位

ToggleSwitch 的 public 桌面类型位于 `AtomUI.Desktop.Controls`，实际交互和布局实现位于 `AtomUI.Controls.AbstractToggleSwitch`。桌面层通过 `ToggleSwitchToken` 和主题文件提供 参考设计体系 风格视觉。本文档聚焦 AtomUI 增强层，不重新说明 Avalonia `ToggleButton` 的基础点击和键盘切换算法。

## 2. 源码文件结构

主要源码：

- `src/AtomUI.Desktop.Controls/Switch/ToggleSwitch.cs`：public 桌面控件，注册 `ToggleSwitchToken` resource scope。
- `src/AtomUI.Controls/Switch/AbstractToggleSwitch.cs`：公共 API、测量、布局、内容绑定、loading、hit test、Form 和渲染主逻辑。
- `src/AtomUI.Controls/Switch/ToggleSwitchContentPanel.cs`：跟踪内容期望尺寸的内部 Canvas，独立拥有两个内容 presenter 的排列。
- `src/AtomUI.Controls/Switch/SwitchKnob.cs`：把手绘制、加载指示绘制、把手宽度动画和加载动画生命周期。
- `src/AtomUI.Desktop.Controls/Switch/ToggleSwitchToken.cs`：ToggleSwitch 控件级 Token。
- `src/AtomUI.Desktop.Controls/Switch/Themes/ToggleSwitchTheme.axaml`：主模板、状态 selector、SizeType 分支和 Token 引用。
- `src/AtomUI.Desktop.Controls/Switch/Themes/SwitchKnobTheme.axaml`：把手主题、加载动画周期和动效。

## 3. 核心类职责

`ToggleSwitch` 是桌面包对外入口。它不新增 API，也不重写布局和交互，只把实例接入 `ToggleSwitchToken` scope。

`AbstractToggleSwitch` 是状态和布局核心。它负责读取 `IsChecked`、`IsLoading`、`SizeType`、`OnContent`、`OffContent` 等状态，计算轨道尺寸、把手位置、内容偏移和 Form value。

`SwitchKnob` 是内部把手。它根据 `KnobSize` 和 `KnobRenderWidth` 绘制圆形或胶囊把手，在 loading 状态下绘制旋转 loading arc。

`WaveSpiritDecorator` 是视觉效果边界，只在 checked 状态变化且 `IsWaveSpiritEnabled=true` 时播放，不参与值状态计算。

## 4. 状态与数据流

基础状态流：

```text
IsChecked / IsPressed / SizeType / content / token
      ↓
MeasureOverride
      ↓
Track size + content desired size
      ↓
ArrangeOverride 的最终内容盒（已应用 Width/Height、Min/Max，不含 Margin）
      ↓
CalculateElementsOffset
      ↓
KnobRect + KnobMovingRect + OnContentOffset + OffContentOffset
      ↓
ToggleSwitchContentPanel 排列内容 + Render 绘制最终轨道
```

checked 变化时，如果启用 motion，控件先基于当前 groove size 重新计算元素位置，再由 `RectTransition` 和 `PointTransition` 执行动画。`_isCheckedChanged` 用于让 arrange 阶段在 checked 切换后使用 `KnobMovingRect`，保持过渡位置一致。

loading 状态流：

```text
IsLoading changed
  → HandleLoadingState
  → set cursor local value / dispose cursor local value
  → SwitchKnob.NotifyStartLoading / NotifyStopLoading
```

Form 状态流：

```text
Form.SetValue(bool?) → IsChecked
IsChecked changed    → Form ValueChanged
Form.GetValue()      → IsChecked
Form.ClearValue()    → IsChecked = null
```

## 5. 生命周期与模板接入

`AbstractToggleSwitch` 构造时创建 `StateMotionSession`，在挂载、分离及模板变更时复位。初次排列直接提交把手矩形与
内容偏移；owner 已加载且最终尺寸、把手排列有效后释放根控件的过渡覆盖。`SwitchKnob` 在模板构建时通过固定的
`StateMotion.Initialization="Owner"` 接入，与 owner 基线和自身 Loaded 汇合后恢复宽度过渡。主题只保留
`IsMotionEnabled` 条件，不订阅动态 Ready 属性；自定义过渡的绑定及最新集合也由属性系统保留并恢复。
初始化 checked 不排队波纹，已就绪的状态切换才生成反馈，执行时再次核对当前部件与生命周期，分离及模板替换时
取消 owner 持有的待执行操作。共享规则见
[状态动效初始化设计](../../../../architecture/systems/control-infrastructure/state-motion-initialization.md)，验证范围见
[状态动效初始化验证](../../../../architecture/systems/control-infrastructure/state-motion-initialization-verification.md)。

`OnApplyTemplate` 获取以下稳定 part：

- `PART_SwitchKnob`
- `PART_OnContentPresenter`
- `PART_OffContentPresenter`
- `PART_WaveSpirit`
- `PART_MainContainer`

模板接入后，控件把当前 `KnobSize` 写入 `SwitchKnob`，再根据当前 `IsLoading` 同步把手 loading 状态。模板 part 没有全局事件订阅，主要生命周期风险来自内容图标 relay binding 和 loading animation。

模板层级与 antd Switch 对齐：`PART_SwitchKnob` 位于外层 `Panel`（root 的直接子层），不在 `PART_MainContainer`（内容
`Canvas`，`ClipToBounds=True`）之内。内容 on/off presenter 的离屏裁剪依赖 `PART_MainContainer`，把手越出轨道边界的
自定义几何（负 `TrackPadding`、`KnobSize` 大于 `TrackHeight`）则不受该 Canvas 裁剪，例如 MUI 形态的 20px 把手
悬挂在 14px 轨道之外。此结构是内置模板的稳定保证，用户样式不得依赖 `PART_*` 或 `/template/` 重建。

Semantic Part marker 映射：`PART_SwitchKnob` 声明 `Classes.semantic-indicator="True"`，`PART_OnContentPresenter` 与
`PART_OffContentPresenter` 声明 `Classes.semantic-content="True"`，`root` 不声明 marker。三个 Part 与模板节点的映射是
静态 AXAML marker，不随 `IsChecked`、`IsLoading`、`SizeType` 或内容变化增删；轨道胶囊由 owner `Render` 直接绘制，没有
独立 track 节点，因此不对应任何 `.semantic-*` marker。完整契约见
[ToggleSwitch Semantic Part 契约](semantic-part.md)。

内容变化时，`SetupContent` 会释放旧 on/off 内容对应的 `CompositeDisposable`，再为新的 `PathIcon` / `Icon` 建立尺寸和前景色绑定。维护时必须保持内容替换和 disposable 释放成对出现。

`SwitchKnob` 在 `OnAttachedToVisualTree` 中根据 `_isLoading` 启动加载动画，在 `OnDetachedFromVisualTree` 中取消并释放 `CancellationTokenSource`。加载旋转使用共享无限动画 helper，并显式采用 `PlaybackBehavior.OnlyIfVisible`；ToggleSwitch 自身或 Visual 祖先不可见时 animation clock 暂停，重新可见后继续。

## 6. 交互与事件处理

Pointer press 流程：

```text
OnPointerPressed
  → if !IsLoading then base.OnPointerPressed
  → if knob not mid-animation
  → _isCheckedChanged=false
  → AdjustOffsetOnPressed
```

Pointer release 流程：

```text
OnPointerReleased
  → if !IsLoading then base.OnPointerReleased
  → AdjustOffsetOnReleased
  → InvalidateArrange
```

loading 状态下 pointer press/release 不进入基类逻辑，因此不会切换 `IsChecked`。键盘切换仍来自 `ToggleButton` 基类，维护 loading 行为时必须确认键盘路径也不产生业务不一致。

`HitTest(Point)` 只在控件 enabled 且非 loading 时返回 groove rect 命中结果。该接口用于自定义命中测试，不应把内容 presenter 或 wave 外扩区域作为可点击范围。

## 7. 内部算法与关键流程

### 7.1 测量与宽度计算

测量阶段分别测量 on/off content presenter，取二者最大宽度作为内容宽度；有内容时再叠加 `InnerMinMargin` 和
`InnerMaxMargin`，无内容时宽度收敛到 `TrackMinWidth`（与 antd 的 min-width 语义一致）。显式 `Width` 优先于内容
测量宽度（且不小于 `TrackMinWidth`）。期望高度使用 `TrackHeight`；把手、内容位置、绘制和命中区域统一使用最终排列内容盒。
内部内容画布汇报两个 presenter 的最大期望尺寸，使内容高度变化能够沿模板树触发 owner 重新测量。
owner 的偏移还依赖每个 presenter 的尺寸；画布测量完成时标记 owner 的排列失效，防止把手尺寸掩盖较小内容的尺寸变化。

`TrackHeight`、`TrackMinWidth`、`TrackPadding`、`KnobSize` 是 owner 级公开 StyledProperty（对应 antd Switch
`ComponentToken` 的 `trackHeight` / `trackMinWidth` / `trackPadding` / `handleSize`），默认值由 SizeType 主题分支从
`ToggleSwitchToken` 写入，Semantic Style 可以直接覆盖。`KnobSize` 变化时同步写入 `SwitchKnob.KnobSize`，四者均
`AffectsMeasure`。

### 7.2 把手位置

`HandleRect(bool isChecked, Size controlSize)` 根据 checked 状态计算把手左上角。unchecked 时把手位于左侧 `TrackPadding`，checked 时位于 `controlSize.Width - TrackPadding - handleSize`。按下时把手宽度乘以 `STRETCH_FACTOR`。

### 7.3 内容偏移

`ExtraInfoRect` 根据当前状态和内部边距计算内容区域。默认字符串的垂直中心取自 `TextBlock.TextLayout` 中
各字体运行的真实字形轮廓、基线和 shaping offset，而不是含下伸预留空间的字体行盒。因此 On / Off、中文、
带下伸或组合字符的标签按其可见字形居中，不写死下移量。计算复用已完成的排版，仅在测量阶段提取轮廓边界并保存偏移数值，每帧排列不生成 Geometry；
空白、无可用字形度量及显式或隐式自定义模板 / 控件内容保持期望尺寸居中。
checked 时 on content 进入可见区域，off content 移到右侧不可见区域；unchecked 时反向处理。pressed 内容位移从当前
把手几何推导，不累计旧偏移。内容偏移使用 `PointTransition` 过渡，并通过 AXAML TemplateBinding 传给内部内容画布；
画布是 presenter 的唯一排列 owner，负责普通布局和每一帧偏移动画。既有自定义模板仍可使用标准 Canvas，owner 保留原有定位兼容路径；默认模板通过专用画布处理内容尺寸失效与动画排列。

### 7.4 轨道绘制

`AbstractToggleSwitch.Render` 使用 `DrawPilledRect` 绘制轨道，并通过 `SwitchOpacity` 控制 disabled/loading 透明度。轨道背景来自 `GrooveBackground`，通常由主题 selector 设置。

### 7.5 把手与 loading 绘制

`SwitchKnob.Render` 根据 `KnobRenderWidth` 和高度判断绘制圆形或胶囊把手，填充色来自标准的 `Background` 属性（主题通过 `HandleBg` Token 设置，`.semantic-indicator` 可直接覆盖）。loading 状态下，按当前 `Rotation` 绘制 90 度 arc。`GetLoadIndicatorPen` 缓存 pen，并在 brush 引用变化时重建。

把手阴影由 `KnobBoxShadow`（主题从 `HandleShadow` Token 设置）派生：`SwitchKnob.OnPropertyChanged` 把
`BoxShadow` 转换为 `DropShadowEffect`，以 `BindingPriority.Style` 写入 `EffectProperty` 即时帧。同优先级下用户
样式帧后求值、后写入，因此 `.semantic-indicator` 的 `Effect` Setter 可以覆盖主题派生阴影；若 `KnobBoxShadow` 为
null 则清空派生效值。

## 8. 资源、性能与 AOT 边界

ToggleSwitch 不通过反射访问模板结构。模板结构由稳定 part 和 AXAML `TemplateBinding` 表达。

资源与生命周期边界：

- on/off 图标内容的 relay binding 必须在内容替换时释放。
- `IsLoading` 设置的 cursor local value 必须在退出 loading 时 dispose。
- `SwitchKnob` 的 loading `CancellationTokenSource` 必须在停止 loading 和 detach 时释放。
- 隐藏祖先下的 loading animation 不得继续推进 animation clock。
- Token 只表达尺寸、颜色、阴影、字体和加载动画周期，不承载 `IsChecked`、`IsLoading` 或内容实例状态。

当前图标内容绑定使用 C# relay binding，因为目标对象来自用户提供的 runtime content，不是稳定模板 part。模板内部固定关系应继续优先使用 AXAML binding 和 selector。

AOT 边界：

- Token 类型通过 generator 显式注册。
- API 与 Token 契约由控件文档、源码 public surface、Token 类型或生成数据维护。
- 文档中列出的 part 名称必须与 AXAML 和 C# 查找代码一致。

## 9. 维护不变量

内部重构必须保持以下不变量：

- `ToggleSwitch` public 类型保持轻量桌面入口，不把共享实现复制到 Desktop 包。
- `AbstractToggleSwitch` 继续作为 API 和状态核心。
- loading 不触发新切换，并释放 cursor local value 和 animation token。
- 内容图标 relay binding 在内容替换时释放。
- `MeasureOverride` 必须同时考虑 on/off content 的最大宽度。
- `SwitchKnob` loading animation detach 时必须取消。
- `SizeType=Custom` 默认与 Middle 视觉一致。
- `SwitchOpacity` 只表达 disabled/loading 视觉透明度，不改变 enabled 状态。
- WaveSpirit 只作为视觉反馈，不影响 `IsChecked`。

## 10. 测试与验证

验证范围：

- checked、unchecked、null 绑定和 Form 读写清空。
- loading 状态下 pointer 和命中测试不触发切换。
- loading attach/detach 不泄漏动画 token。
- `OnContent` / `OffContent` 为文本和图标时的测量、偏移、前景色和尺寸同步。
- Large / Middle / Small / Custom 下的高度、最小宽度、内容边距和图标尺寸。
- disabled、pointerover、pressed、checked 和 unchecked 主题 selector。
- WaveSpirit 开关不改变值状态。
- Gallery ToggleSwitch 示例、源码片段。
- 文档改动运行 `git diff --check`，并检查相对链接存在。
