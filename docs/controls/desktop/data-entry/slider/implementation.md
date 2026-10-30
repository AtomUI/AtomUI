# Slider 桌面版实现原理

本文档描述 Slider 桌面版的源码职责、动态 thumb、轨道元素与渲染、交互路径、Tooltip、Form 集成和资源生命周期。公共设计与 API 契约见 [Slider 桌面版架构设计](overview.md)，Semantic Part 契约见 [Slider Semantic Part 契约](semantic-part.md)，多 handle 专项模型见 [Slider 多 Handle 设计](multi-handle-design.md)，Token 语义见 [Slider Token 设计](token.md)，变化记录见 [Slider Changelog](changelog.md)。

## 1. 实现定位

Slider 基于 Avalonia `RangeBase`。`Slider` 是公共值、交互会话和 Form 状态 owner；`SliderTrack` 是动态 thumb、轨道元素、布局和输入几何 owner；`SliderThumb` 是单个 handle 的视觉与 pointer capture 节点。rail、整体活动范围与相邻 segment 由 `SliderTrack` 管理的代码创建 `Border` 元素承载（Semantic Part `rail` / `tracks` / `track`），mark 点与文本由 internal `SliderMarksElement` 自绘。当前实现不建立 `HandleState` 数据类型，值、thumb 和渲染几何分别由 `EffectiveRangeValues`、`SliderThumb` 列表和 `RenderContextData` 按索引关联。

本文档只记录需要跨文件维护的稳定职责和流程。私有辅助方法、具体 Token 默认值和自动化协议细节仍以源码、[Slider Token 设计](token.md) 和测试为准。

## 2. 源码文件结构

- `src/AtomUI.Desktop.Controls/Slider/Slider.cs`：公共属性、Range 值归一化入口、pointer / keyboard 交互协调、Tooltip、Form、数据校验和自动化 peer 创建。
- `src/AtomUI.Desktop.Controls/Slider/Slider.SemanticParts.cs`：`Slider` 的 Semantic Part 声明（`root` 隐式，`rail` / `tracks` / `track` / `handle` 显式）。
- `src/AtomUI.Desktop.Controls/Slider/SliderTrack.cs`：`EffectiveRangeValues` 投影、动态 thumb 生命周期、rail / tracks / segment 元素与 mark 元素管理、handle 布局、轨道几何换算。
- `src/AtomUI.Desktop.Controls/Slider/SliderThumb.cs`：单个 handle 的 pointer capture、drag routed event、focus / pressed / disabled 状态和绘制。
- `src/AtomUI.Desktop.Controls/Slider/SliderMarksElement.cs`：mark 点与标签的 internal 自绘元素（非 Semantic Part）。
- `src/AtomUI.Desktop.Controls/Slider/SliderRangeMath.cs`：Range 值归一化、handle 边界、整体 offset、值与坐标比例及 segment 几何的纯计算。
- `src/AtomUI.Desktop.Controls/Slider/SliderToken.cs`：Slider Token scope、尺寸、颜色、padding 和 outline 默认值计算。
- `src/AtomUI.Desktop.Controls/Slider/SliderAutomationPeer.cs`：Slider 自动化 peer。
- `src/AtomUI.Desktop.Controls/Slider/SliderThumbAutomationPeer.cs`：单个动态 thumb 的自动化 peer。
- `src/AtomUI.Desktop.Controls/Slider/Themes/SliderTheme.axaml`：根模板、`PART_Track`、属性传递和方向样式。
- `src/AtomUI.Desktop.Controls/Slider/Themes/SliderTrackTheme.axaml`：track 尺寸、mark Token 和轨道 transition。
- `src/AtomUI.Desktop.Controls/Slider/Themes/SliderThumbTheme.axaml`：thumb 尺寸、边框、outline、focus、hover、disabled 和布局取整策略。
- `tests/AtomUI.Desktop.Controls.Tests/Slider/SliderBehaviorTests.cs`：值计算、动态 thumb、pointer、布局和生命周期回归。
- `tests/AtomUI.Desktop.Controls.Tests/Slider/SliderSemanticPartTests.cs`：Semantic Part descriptor、marker 数量、路由命中、元素几何与生命周期回归。
- `controlgallery/AtomUIGallery/ShowCases/DataEntry/Slider/`：Slider Gallery 示例、ViewModel 和本地化资源。

## 3. 核心类职责

| 类型 | 职责 | 状态边界 |
| --- | --- | --- |
| `Slider` | 暴露公共 API；持有当前拖动目标和整体轨道拖动快照；提交 `Value` 或 `RangeValues`；同步 Tooltip、Form 与校验状态。 | 不持有动态 thumb 集合或渲染矩形。 |
| `SliderTrack` | 将值集合投影为动态 `SliderThumb`；管理 rail / tracks / segment `Border` 元素与 mark 元素；计算 rail、handle、segment、整体活动范围和 mark 几何；同步元素背景与可见性。 | 不拥有 Form 值；`EffectiveRangeValues` 只投影当前 Slider 状态。 |
| `SliderThumb` | 捕获 pointer、产生 drag routed event、绘制单个 handle 并表达 focus / pressed / disabled。 | `HandleIndex` 仅用于回到值集合索引，不保存业务值。 |
| `SliderMarksElement` | 自绘 mark 点与标签（internal，非 Semantic Part）。 | 只接收 SliderTrack 计算的矩形与画刷，不读取值状态。 |
| `SliderRangeMath` | 提供无 UI 状态的有限性、排序、裁剪、边界和几何计算。 | 不读取控件、主题或 input manager。 |
| Automation peers | 将 Slider 和 thumb 投影到 Avalonia 自动化层。 | 不参与值归一化或视觉节点管理。 |

## 4. 状态与数据流

单值与 Range 模式共享以下投影路径：

```text
Slider.Value / Slider.RangeValues
  ↓
RangeValues coerce -> SliderRangeMath.NormalizeRangeValues()
  ↓
SliderTheme TemplateBinding
  ↓
SliderTrack.EffectiveRangeValues
  ├── EnsureThumbs() -> SliderThumb[index]
  ├── EnsureSegmentElements() -> Border[index]（semantic-track）
  ├── ArrangeOverride() -> rail / tracks / segment Bounds + thumb Bounds + RenderContextData
  └── Slider.ConfigureTemplateThumbTips() -> Tooltip
```

单值模式的 `EffectiveRangeValues` 为 `[Value]`；Range 模式使用归一化后的 `RangeValues`，空值或少于两个值时回退为 `[Minimum, Minimum]`。`DisabledHandles[index]` 在 `EnsureThumbs()` 中投影为对应 thumb 的 `IsEnabled`，并在 `SliderRangeMath` 中限制移动入口；实现不复制一份独立的 disabled 状态模型。

`RangeValues` 采用快照语义。pointer、keyboard 和 Form 写入都提交新的完整列表，并由同一 coerce 路径执行有限性检查、范围裁剪和升序归一化。连续 pointer 输入保留 `double` 值；只有 `IsSnapToTickEnabled=true` 时，`Slider` 才在提交前调用 tick 吸附逻辑。

## 5. 组合结构模型

默认主题的稳定模板层级为：

```text
Slider
└── SliderTrack#PART_Track
    ├── Border（semantic-rail，代码创建）
    ├── Border（semantic-tracks，代码创建）
    ├── Border[0..n]（semantic-track segment，代码创建）
    ├── SliderMarksElement（mark 点与标签，internal）
    └── SliderThumb[0..n]（semantic-handle，动态创建）
```

`SliderTheme.axaml` 只声明 `PART_Track`。rail / tracks 元素在 `SliderTrack` attach 时创建一次，segment 元素随
`EffectiveRangeValues` 数量同步（单值 1 个、Range N-1 个），`SliderThumb` 按 handle 数量动态加入 logical tree 和
visual tree；这些节点都设置外层 `Slider` 为 TemplatedParent，因此 `SliderTheme` 的模板 selector 与
`/template/ .semantic-*` 语义路由都从 `Slider` 单跳命中。

| 节点 | 类型 | 来源 | 生命周期 owner | 影响的 public API | 稳定性 | Agent 使用边界 |
| --- | --- | --- | --- | --- | --- | --- |
| Slider | public control | `Slider.cs` / `SliderTheme.axaml` | 调用方与 Avalonia 控件树 | 全部 Slider API | public | 用户可直接创建和绑定。 |
| `PART_Track` | `SliderTrack` | `SliderTheme.axaml` | Slider template | Template Part、轨道画刷、方向和 Range 状态 | template-stable | 自定义主题必须保留名称与类型。 |
| rail / tracks 元素 | `Border` | `SliderTrack` attach 时创建 | `SliderTrack` | Semantic Part `rail` / `tracks` | stable since 6.2.0 | 通过 `SliderRailStyle` / `SliderTracksStyle` 定制，不得依赖 internal 创建路径。 |
| segment 元素 | `Border` | `SliderTrack` 按值数量同步 | `SliderTrack` | Semantic Part `track` | stable since 6.2.0 | 通过 `SliderTrackStyle` 定制。 |
| mark 元素 | `SliderMarksElement` | `SliderTrack` attach 时创建 | `SliderTrack` | `Marks` 视觉 | internal-observable | 不属于 Semantic Part，可用于理解行为，不得依赖。 |
| 动态 thumb | `SliderThumb` | `SliderTrack.EnsureThumbs()` | `SliderTrack` | Semantic Part `handle`、handle 数量、disabled、Tooltip 和交互 | stable since 6.2.0 | 通过 `SliderHandleStyle` 定制，不得依赖固定数量或固定名称。 |
| `RenderContextData` | private geometry cache | `SliderTrack.cs` | `SliderTrack` | 轨道和 mark 的可观察几何 | private | 仅作为实现维护依据，不是扩展 API。 |

## 6. 生命周期与模板接入

1. `Slider.OnApplyTemplate()` 先释放旧 pointer handler，并从旧 `SliderTrack` 解绑 `ThumbsChanged`。
2. Slider 查找唯一稳定 Template Part `PART_Track`，设置内部交互所有权并订阅动态 thumb 变化。
3. `SliderTrack` attach 后订阅全局 input process、创建 rail / tracks / mark 元素、创建当前数量的 segment 与 thumb，并计算 mark 尺寸。
4. `EnsureThumbs()` 只在 handle 数量变化时创建或移除视觉节点；数量不变时更新 `HandleIndex` 和 `IsEnabled`。
5. `EnsureSegmentElements()` 只在 segment 数量变化（单值 1、Range N-1）时创建或移除 `Border` 元素；rail / tracks 元素恒存在。
6. 创建 thumb / 元素时设置外层 Slider 为 templated parent、同步 `IsMotionEnabled`，并订阅 drag 事件（thumb）；运行时 motion 变化会同步到已有 thumb。
7. 移除 thumb / segment 时解绑事件、移出 logical / visual tree 并清除 templated parent。
8. `SliderTrack` detach 时释放全局 input subscription、清空动态 thumb 与 segment 元素并移除 rail / tracks / mark 元素；Slider detach 或模板重建时释放 pointer handler、拖动会话和旧 track 订阅。

Tooltip 文本和 placement 由 Slider 在模板应用、thumb 数量变化、值、格式或方向变化时同步。动态 thumb 自身不拥有 Tooltip 文本状态。rail / tracks / segment 元素与 mark 元素不可命中测试（`IsHitTestVisible=false`），pointer 命中与 mark 点击仍由 Slider / SliderTrack 的几何计算路径处理。元素化后 `SliderTrack` 不再自绘轨道几何（无 DrawList 可供合成器命中测试），因此实现 `ICustomHitTest`（`HitTest` 返回 `Bounds` 全区域命中）保持自身作为 pointer 输入的命中目标，轨道点击与拖动路由与自绘时期一致。

## 7. 交互与事件处理

滑块外点焦点处理只接收轨道所属活输入根的事件。输入位置先转换为轨道局部坐标，轨道和滑块区域直接在同一坐标系判断，不重复叠加 `Bounds.Position`；轨道不得通过异窗原始事件主动改变焦点；正常窗口切换和原生焦点路由仍由框架负责。

`SliderThumb` 在 pointer press 时记录本地坐标并捕获 pointer，move 时产生 `DragDelta`，release 或 capture lost 时产生 `DragCompleted`。Slider 在 tunnel 阶段监听 pointer press / move / release，以一个 owner 处理轨道点击、handle 拖动和整体活动范围拖动，避免 Slider 与 SliderTrack 同时提交值。

轨道按下路径：

1. 判断左键和 `PART_Track` 是否可用。
2. 若命中允许拖动的整体活动范围，保存初始 pointer 值和 `RangeValues` 快照。
3. 否则选择最近的 enabled thumb；直接命中 thumb 时保留 pointer 与 thumb 值的相对 offset。
4. mark 命中优先使用 mark 值，否则使用 pointer 映射出的连续 `double` 值。
5. 仅在启用 tick 吸附时量化，然后提交单值或完整 Range 值快照。
6. release、capture lost、disabled、detach 或模板重建结束拖动状态。

键盘根据当前 focused thumb 的 `HandleIndex` 选择 Range 目标。方向键和 Page 键使用 `SmallChange` / `LargeChange`，`Home` / `End` 使用当前 handle 的有效边界；disabled thumb 不进入焦点和键盘修改路径。

## 8. 内部算法与关键流程

### 值归一化与 handle 移动

`NormalizeRangeValues()` 对每个值执行有限性检查和 `[Minimum, Maximum]` 裁剪，仅在需要时创建、排序新快照；合法且已排序的快照保持原实例。`MoveHandle()` 只替换目标索引，并以相邻 handle 或全局边界限制目标值，因此 handle 不交叉且重复值合法。

### 值与坐标映射

`GetRailRect()` 从可用尺寸中扣除 thumb 尺寸，`ValueToCenterPoint()` 和 `ValueFromPoint()` 在该 rail 局部坐标中互相映射。水平模式使用 X 轴，垂直模式反转 Y 轴，`IsDirectionReversed` 再反转数值方向。坐标和值计算均使用 `double`，默认 thumb 主题关闭布局取整，使 fractional center 不被量化到物理像素阶梯。

### 轨道几何与元素排列

`SliderTrack.ArrangeOverride` 从 `EffectiveRangeValues` 生成 `RenderContextData`：单值模式生成 Minimum 到 Value 的一个 segment；Range 模式为每对相邻值生成 `SegmentRects`，并生成整体活动范围 `TrackRangeRect`（单值模式为 `Minimum → Value`，Range 模式为首值到末值）。随后按子元素顺序排列：

1. rail 元素（`semantic-rail`）：Bounds = `RailRect`，胶囊 `CornerRadius` = rail 厚度一半，背景 = `TrackGrooveBrush`。
2. tracks 元素（`semantic-tracks`）：Bounds = `TrackRangeRect`，背景 = `TracksBrush`。
3. segment 元素（`semantic-track`，单值 1 个 / Range N-1 个）：Bounds = 对应 `SegmentRects` 项，背景 = `TrackBarBrush`。
4. mark 元素：接收 mark 点 / 标签矩形与画刷，自绘圆点与文本。
5. thumb：按 `ValueToCenterPoint` 排列，自绘圆点与 outline。

`IsIncluded=false` 时 tracks 与全部 segment 元素 `IsVisible=false`（rail 不受影响）。轨道与元素画刷由
`SliderTrack` 在属性变化时同步推送；`SliderTrackTheme` 的 brush transition 继续作用于 `TrackGrooveBrush` /
`TrackBarBrush` / `MarkBorderBrush`，动画中间值随属性变化推送到元素。几何与胶囊 `CornerRadius` 由控件按布局写入
（`CornerRadius` 使用 Style 优先级，用户 Semantic Style 可覆盖）。元素化前后默认 token 下的几何、画刷与绘制顺序
一致，默认视觉不变。

### 整体活动范围拖动

整体拖动会话由 Slider 保存初始 pointer 值和完整值快照。每次 move 计算共享 offset，并通过 `ApplyTrackOffset()` 把 offset 裁剪到 `Minimum - first` 与 `Maximum - last`。所有 handle 使用同一 offset；存在任意 disabled handle 时不启动该会话。

## 9. 资源、性能与 AOT 边界

- 动态 thumb 的创建、事件订阅、templated parent 和释放由 `SliderTrack` 成对管理。
- Slider 的模板 handler、SliderTrack 的全局 input subscription 和 pointer capture 都有明确释放入口。
- 外部 `RangeValues` 与 `DisabledHandles` 不建立集合变更订阅，调用方必须替换快照触发更新。
- pointer move 不创建或替换 thumb；只提交值快照并触发布局、Tooltip 与渲染更新。
- `RenderContextData`、pen 和 mark 文本度量由 SliderTrack 持有，不进入公共状态。
- 实现不使用反射、动态类型发现、字符串属性路径或运行时程序集扫描，保持 trimming 和 NativeAOT 友好。
- `UseLayoutRounding=false` 只应用于默认 `SliderThumbTheme`，用于保留连续拖动得到的 fractional Bounds；自定义主题需要自行保持同一平滑布局契约。

## 10. 维护不变量

- Slider 是公共值、Form 状态和交互会话 owner；SliderTrack 是动态节点、元素、布局、渲染和输入几何 owner。
- Range 模式唯一值源是 `RangeValues`，不建立固定 start / end handle 或并行 `HandleState` 模型。
- `RangeValues` 必须经过有限性检查、范围裁剪和升序归一化，并保留重复值和 handle 数量。
- 连续 pointer 拖动必须保留 `double` 精度；仅 `IsSnapToTickEnabled=true` 时按 tick 量化。
- disabled handle 不可被 pointer 或 keyboard 修改，并作为相邻 handle 的移动边界。
- 任意 disabled handle 存在时，整体活动范围不可拖动。
- `TracksBrush` 只绘制整体活动范围（单值 `Minimum → Value`、Range 首尾值）；`TrackBarBrush` 只绘制相邻 handle segment。
- `PART_Track` 是唯一固定 Slider Template Part；动态 thumb 与 segment 元素不得重新变成固定命名部件。
- rail / tracks 元素每 Slider 恒一个，segment 元素数量只随值数量变化；`IsIncluded=false` 只切换 tracks / segment 可见性，不增删节点或 marker。
- 动态 thumb 与元素必须继承外层 Slider 的模板状态，并实时同步 motion 状态。
- rail / tracks / segment / mark 元素不可命中测试；pointer 与 mark 命中仍走 SliderTrack 几何计算路径。
- 模板重建和 detach 必须释放事件、pointer capture、全局 input subscription、拖动会话与全部动态节点。

## 11. 测试与验证

- `SliderBehaviorTests`：纯值计算、有限性、排序、重复值、disabled boundary、整体 offset、动态 thumb 创建/回收、templated parent、motion 同步、pointer capture 和 fractional pointer 布局。
- `SliderSemanticPartTests`：descriptor 字段、rail / tracks / track / handle marker 数量与生命周期、`/template/ .semantic-*` 路由命中、元素几何与既有 rail / segment / range 几何一致、`IsIncluded` / `IsRangeMode` / `RangeValues` 快照 / `DisabledHandles` / orientation 切换下 marker 稳定性、单值模式 `TracksBrush` 对齐行为、元素不可命中测试与主题静态 marker 断言。
- `RemainingFormValueBindingTests`：单值与 Range Form 值和数据验证。
- `AtomUIGallery.Tests` Slider 页面测试：稳定 Showcase、绑定、禁用指定 handle、多点组合结构、Semantic Part 预览与自定义语义结构的样式示例。
- Gallery 走查：horizontal / vertical、reverse、marks、tick、Light / Dark、多点组合、禁用指定滑块，以及 100% / 125% / 150% 缩放下的 thumb 平滑度与边框清晰度。
- 文档验证：运行目标测试、Gallery build 和 `git diff --check`。
