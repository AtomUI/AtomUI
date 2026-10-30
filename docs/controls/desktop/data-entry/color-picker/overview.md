# ColorPicker 桌面版架构设计

本文档定义 `ColorPicker` 桌面版的最新设计定位、公共契约、状态模型、视觉主题关系和兼容边界。通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，内部实现原理见 [ColorPicker 桌面版实现原理](implementation.md)，ColorPicker Token 的专项设计见 [ColorPicker Token 设计](token.md)，设计和契约变化记录见 [ColorPicker Changelog](changelog.md)。

纯色与渐变选择器通过 `IsNeedConfirm` 控制显式确认：面板编辑候选值，宿主 `Value` 保存提交值。完整模式矩阵、事件、取消及宿主边界见 [ColorPicker 确认与取消设计](confirmation-design.md)。

该控件的 Popup 钉住打开属于共享弹层契约，详见 [Popup 钉住打开设计](../../other/popup/popup-pinned-open-design.md)。`AbstractColorPicker` 提供 public `IsPopupPinnedOpen`：设置为 true 时保持 picker open state 并 relay 到 color panel Popup，使 Gallery 语义预览等场景可以持续展示 `popup.root` 部件；设置为 false 时只解除关闭拦截。控件卸载、锚点失效、TopLevel 改变和模板重建仍按共享生命周期规则清理。

ColorPicker 与 GradientColorPicker 各自公开 5 个 Semantic Part（`root`、`body`、`content`、`description`、`popup.root`），与上游 ColorPicker 的公开语义 API 逐一对齐；完整 Part 表、Selector 用法与定制边界见 [ColorPicker Semantic Part 契约](semantic-part.md)。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls.ColorPicker` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/DataEntry/ColorPicker` |
| 控件状态 | Stable |

ColorPicker 是 AtomUI 桌面控件体系中的颜色选择控件，用于选择纯色、透明度、预设色和渐变色。

ColorPicker 不负责通用绘图编辑器或业务调色板持久化协议。这些职责应由业务层、组合控件或更专用的 AtomUI 控件承担。

主要源码入口：

- `src/AtomUI.Desktop.Controls.ColorPicker`

## 2. 设计语言

ColorPicker 的设计语言围绕控件职责、可观察状态和主题契约组织，而不是围绕模板节点组织。

| 维度 | 含义 | ColorPicker 中的表达 |
| --- | --- | --- |
| 产品语义 | 控件在界面中承担的稳定职责。 | ColorPicker 是 AtomUI 桌面控件体系中的颜色选择控件，用于选择纯色、透明度、预设色和渐变色。 |
| 内容承载 | 用户数据、展示内容、集合项或操作入口如何进入控件。 | `ColorValue`、`ColorValueBrush`、`DefaultValue`、`EmptyColorText`、`GradientValue`、`IsTextVisible`、`MaxValue`、`MinValue` 等 10 项。 |
| 状态反馈 | public API、内部状态和伪类如何形成用户可感知反馈。 | open/close、collection/filter、input/value、motion、visual option。 |
| 主题语义 | ControlTheme、SharedToken、Control Own Token 和模板绑定如何表达视觉。 | ColorPicker Token + ControlTheme。 |

## 3. API 与契约模型

ColorPicker 的公共契约由 public/protected 类型成员、Avalonia 属性、事件、命令、template part、伪类、ControlTheme key 和资源 key 共同组成。维护时应先确认这些契约是否已经被源码、Gallery 示例或文档暴露。

核心 public surface 按语义分组维护：

| 契约组 | 代表成员 | 维护含义 |
| --- | --- | --- |
| 内容与数据 | `ColorValue`、`ColorValueBrush`、`DefaultValue`、`EmptyColorText`、`GradientValue`、`IsTextVisible`、`MaxValue`、`MinValue`、`Value`、`ValueSyncStrategy` | 定义控件展示内容、输入数据、模板或业务对象入口。 |
| 选择与集合 | `ColorModel`、`IsEmptyColorMode`、`IsPaletteGroupEnabled` | 维护选择、展开、过滤、分页、分组或集合状态。 |
| 交互与状态 | `IsNeedConfirm`、`IsActivated`、`IsAlphaEnabled`、`IsAlphaVisible`、`IsArrowVisible`、`IsClearEnabled`、`IsColorSpectrumSliderVisible`、`IsFormatEnabled`、`IsMotionEnabled`、`IsPerceptive`、`IsPointAtCenter` | 表达确认策略、用户可观察状态、可用性、清除和反馈语义。 |
| 视觉与布局 | `Color`、`ColorComponent`、`ColorSpectrumComponents`、`HsvColor`、`MarginToAnchor`、`Placement`、`PlacementAnchor`、`PlacementGravity`、`Shape`、`Size` 等 12 项 | 影响尺寸、位置、颜色、形状、密度和模板视觉变量。 |
| 动效与异步 | `MouseEnterDelay`、`MouseLeaveDelay` | 约束动效开关、异步加载、播放速度、超时和任务边界。 |
| 其他稳定入口 | `ActivatedThumb`、`Components`、`DecreaseButton`、`Format`、`IncreaseButton`、`MaxHue`、`MaxSaturation`、`Maximum`、`MinHue`、`MinSaturation` 等 14 项 | 保留为 public surface，变更前需确认 Gallery 和用户 XAML 依赖。 |

`ColorPicker.Value` 和 `GradientColorPicker.Value` 是用户拥有的提交值：纯色选择器使用 `Color?`，渐变选择器使用 `LinearGradientBrush?`。两个 `Value` CLR wrapper 均可公开设置，默认 `TwoWay` 绑定，并接入 Avalonia `DataValidationErrors`。Form clear 和外部绑定写入 `null` 直接清空提交值；面板清除在确认模式下先将候选值置为 `null`，确认后才清空提交值。清空提交值必须由 `Value=null` 驱动文字、色块和 Form 状态，不能只清空视觉。

`AbstractColorPicker.IsNeedConfirm` 是默认值为 `false` 的布尔 Avalonia 属性，由 `ColorPicker` 和 `GradientColorPicker` 共同继承。为 `false` 时保留原有值同步与关闭事件语义；为 `true` 时仅显式确认提交，并优先于纯色选择器的 `ValueSyncStrategy`。`ValueSyncStrategy` 默认 `Immediate`，`OnCompleted` 表示关闭时提交，两者均不是取消策略；渐变选择器不因此增加 `ValueSyncStrategy`。

`ValueChanged` / `GradientValueChanged` 报告提交值变化。确认模式下，面板编辑与取消不触发宿主值事件，确认非空值后 `ValueSelected` 触发一次；确认清空通过值变化事件报告 `null`，保持既有事件参数契约。默认模式保留原有关闭时 `ValueSelected` 的语义。

稳定事件包括 `ClearRequest`、`GradientActiveStopChanged`。事件触发顺序属于兼容契约，不能因内部状态重排而改变。

主要公开类型与枚举：

- 类型：`AbstractColorPicker`、`AbstractColorPickerSliderTrack`、`AbstractColorPickerView`、`AbstractColorSlider`、`AtomUIColorPickerThemesProvider`、`ColorBlock`、`ColorChangedEventArgs`、`ColorPicker`、`ColorPickerCollapse`、`ColorPickerInput`、`ColorPickerLangResourceExtension`、`ColorPickerPalette`、`ColorPickerPaletteColorSelectedEventArgs`、`ColorPickerPaletteGroup` 等 36 项。
- 枚举：`ColorFormat`、`ColorPickerLangResourceKind`、`ColorPickerTokenKind`、`ColorPickerValueSyncMode`。

稳定 template part：

| Template Part | 类型 | 职责 |
| --- | --- | --- |
| `PART_AlphaInput` | `?` | 承载文本输入、过滤、显示或编辑入口。 |
| `PART_BValueInput` | `?` | 承载文本输入、过滤、显示或编辑入口。 |
| `PART_CheckedMark` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |
| `PART_ClearColor` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |
| `PART_ColorFormatComboBox` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |
| `PART_ColorIndicator` | `?` | 展示指示器、进度、分页或状态反馈。 |
| `PART_ColorPreview` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |
| `PART_ColorPreviewFrame` | `?` | 承载根视觉、边框、背景或尺寸基线。 |
| `PART_ColorText` | `?` | 承载文本输入、过滤、显示或编辑入口。 |
| `PART_ColorTextPanel` | `?` | 承载集合项、布局面板或虚拟化内容。 |
| `PART_DecreaseButton` | `?` | 承载用户触发入口、导航或关闭动作。 |
| `PART_EmptyColorFrame` | `?` | 承载根视觉、边框、背景或尺寸基线。 |
| `PART_Frame` | `?` | 承载根视觉、边框、背景或尺寸基线。 |
| `PART_GValueInput` | `?` | 承载文本输入、过滤、显示或编辑入口。 |
| `PART_GradientColorSlider` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |
| `PART_HValueInput` | `?` | 承载文本输入、过滤、显示或编辑入口。 |
| `PART_HexValueInput` | `?` | 承载文本输入、过滤、显示或编辑入口。 |
| `PART_HsvValuePanel` | `?` | 承载集合项、布局面板或虚拟化内容。 |
| `PART_IncreaseButton` | `?` | 承载用户触发入口、导航或关闭动作。 |
| `PART_InnerEllipse` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |
| `PART_InputTarget` | `?` | 承载文本输入、过滤、显示或编辑入口。 |
| `PART_LayoutRoot` | `?` | 承载根视觉、边框、背景或尺寸基线。 |
| `PART_PaletteGroup` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |
| `PART_Popup` | `?` | 承载弹层宿主、打开关闭或候选内容。 |
| 其他 part | 10 项 | 参见源码和主题文件；维护时按同一生命周期规则检查。 |

控件专属或内部伪类包括 `ColorPickerPseudoClass.FlyoutOpen`、`ColorSliderPseudoClass.DarkSelector`、`ColorSliderPseudoClass.LightSelector`、`ColorSpectrumPseudoClass.DarkSelector`、`ColorSpectrumPseudoClass.LargeSelector`、`ColorSpectrumPseudoClass.LightSelector`、`ColorSpectrumPseudoClass.Pressed`、`DarkSelector=:dark-selector`、`FlyoutOpen=:flyout-open`、`LargeSelector=:large-selector`、`LightSelector=:light-selector`、`Pressed=:pressed`。这些伪类属于主题 selector 可观察契约，不能在未同步主题和 Gallery 的情况下重命名或删除。

## 4. 行为与状态模型

ColorPicker 的状态流按以下路径收敛：

```text
Public API / inherited command / item source / user input
  -> 控件实例状态
  -> effective state / pseudo-class / template property
  -> ControlTheme selector / presenter / renderer
  -> Gallery 可观察行为
```

状态维护规则：

- Disabled 或不可交互状态优先屏蔽 pointer、keyboard、motion 和提交类反馈。
- open/close、collection/filter、input/value、motion、visual option 状态由控件实例或明确的数据 owner 推导，不能在 template part 之间双向竞争。
- `IsPickerOpen` 是 picker 的业务打开状态，`Popup.IsOpen` 是物理宿主状态；pinned 期间普通关闭不能改变业务状态，锚点隐藏、detach 或 TopLevel 失效仍可关闭物理宿主，并在有效性恢复后重新打开。
- `Value`、trigger 色块、trigger 文本和 Form 值由宿主提交值派生。确认模式下，picker presenter 的输入与预览由独立候选值派生；候选值不能通过 TwoWay 绑定提前写回宿主。
- 确认模式下，取消、Escape、外点、失焦等普通关闭丢弃草稿；重开从当前提交值重新开始。外部值写入与确认配置变化重置草稿，生命周期结束清理草稿及订阅。钉住弹层中的确认与取消不改变钉住打开契约，详见确认专项设计。
- 模板重套用时必须把 public API 对应状态回放到新的 part、伪类和主题变量。
- 集合、弹层、异步、动效或窗口相关状态必须能处理 reset、close、cancel、detach 和 owner 释放。

## 5. 视觉与主题模型

ColorPicker 的视觉模型由控件模板、ControlTheme、SharedToken 和 ColorPicker Own Token 共同构成。

| 主题文件 | 职责 |
| --- | --- |
| `AbstractColorPickerTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `ColorBlockTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `ColorPickerPaletteGroupTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `ColorPickerTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `AbstractColorPickerSliderTrackTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `ColorPickerSliderTrackTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `ColorSliderTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `ColorSliderThumbTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `GradientColorPickerTrackTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `GradientColorSliderTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `AbstractColorPickerViewTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `ColorPickerInput.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `ColorPickerViewTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `ColorSpectrumTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `GradientColorPickerViewTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `GradientColorPickerTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `PaletteColorItemTheme.axaml` | 定义集合项、容器项或局部单元的状态视觉。 |

ColorPicker 拥有独立 Control identity；`ColorPickerToken` 只表达 ColorPicker Own Token 语义，不承载 open/close、collection/filter、input/value、motion 或 visual option 运行时状态。Control 级 Global Token 覆盖与 Own Token 通过 `ColorPickerTokenResource` 统一读取。

主题维护规则：

- 不删除或重命名已经稳定的 ControlTheme key、template part、伪类和资源 key。
- 不把可由 AXAML 表达的模板状态迁移为 C# 动态创建视觉。
- 不把 hover、pressed、selected、expanded、loading、filter、popup open 等运行时状态写入 Token。
- Browser 或平台特化主题必须保持同一 API 的语义一致。
- 确认模式的“取消 / 确定”操作区声明在面板 ControlTheme 中，默认模式不占布局空间；原有调色区、预设色、输入区和 trigger 尺寸保持原契约。按钮文案使用控件本地化资源，视觉复用 Button 与 SharedToken。

## 6. 控件家族或集成关系

ColorPicker 与同分类控件共享尺寸、状态、Token、Gallery 展示和验证规则。组合或派生控件应显式说明哪些 API 被继承、覆盖或不支持。

主要协作类型：

- `AbstractColorPicker`：跨平台或共享基类，承载公共 API、状态归一和模板生命周期。
- `AbstractColorPickerSliderTrack`：跨平台或共享基类，承载公共 API、状态归一和模板生命周期。
- `AbstractColorPickerSliderTrackTheme`：ControlTheme 类型入口，连接主题资源和控件类型。
- `AbstractColorPickerTheme`：ControlTheme 类型入口，连接主题资源和控件类型。
- `AbstractColorPickerView`：跨平台或共享基类，承载公共 API、状态归一和模板生命周期。
- `AbstractColorPickerViewTheme`：ControlTheme 类型入口，连接主题资源和控件类型。
- `AbstractColorSlider`：跨平台或共享基类，承载公共 API、状态归一和模板生命周期。
- `AbstractColorSliderTheme`：ControlTheme 类型入口，连接主题资源和控件类型。
- `AtomUIColorPickerThemesProvider`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `ColorBlock`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `ColorPicker`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `ColorPickerCollapse`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `ColorPickerInput`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `ColorPickerLangResourceExtension`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `ColorPickerPalette`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `ColorPickerPaletteGroup`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `ColorPickerPseudoClass`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `ColorPickerSliderTrack`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `ColorPickerToken`：ColorPicker Own Token builder，使用该 Control 的 Effective Global Token 计算默认值。
- `ColorPickerTokenResourceExtension`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `ColorPickerView`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `ColorSlider`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `ColorSliderThumb`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `ColorSpectrum`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- 其他 11 个内部类型按源码目录分层维护，修改前应先确认所有引用路径。

集成关系：

- 与 ThemeManager、SharedToken、ControlTheme、控件文档和 Gallery ShowCase 示例保持一致。
- 涉及 ItemsSource、Popup、Flyout、Window、Form 或 CompactSpace 的路径必须保持生命周期释放和数据状态同步。
- 源码目录中的共享基类和内部协作类型形成维护边界，不能只修改桌面包装类而忽略共享状态 owner。

## 7. 兼容性不变量

维护 ColorPicker 时必须保持以下不变量：

- 不擅自新增、删除、重命名或改变 public/protected API、Avalonia 属性、事件和默认值。
- 不破坏 template part、伪类、ControlTheme key、Token 名称和资源 key。
- 不改变 Gallery 已展示的 XAML 用法、默认外观、交互顺序和状态优先级。
- Template part 重新应用、集合替换、弹层关闭、窗口失活和控件 detach 时必须释放旧订阅和资源宿主。
- 不通过隐藏延迟、强制刷新或吞异常掩盖状态同步问题。
- 不引入运行时反射扫描作为 API、Token 或数据路径发现机制。
- 文档只描述当前稳定设计；历史变化记录在 `changelog.md`。

## 8. 专项模型

### 8.1 选择与当前项模型

ColorPicker 的提交值由宿主唯一拥有，打开期间候选值由面板编辑状态唯一拥有；颜色预览与已提交 trigger 分别投影对应状态。纯色、渐变、空值及确认模式的状态转换见 [确认与取消设计](confirmation-design.md)。

### 8.2 弹层与宿主模型

ColorPicker 涉及弹层、窗口或 overlay 宿主时，打开状态、取消事件、定位和宿主释放必须保持一致。重复打开、关闭、窗口失活和 template reapply 都必须释放旧宿主引用。

### 8.3 集合与数据同步模型

ColorPicker 的集合状态必须能处理 source replace、reset、clear 和 container recycle。业务数据对象不应反向持有视觉对象，虚拟化或懒创建路径必须在容器回收时清理旧状态。

### 8.4 动效模型

ColorPicker 的动效只表达状态变化反馈，不应改变 public API 语义。初始加载、禁用态和卸载路径应能抑制或取消动效，避免保留旧控件实例。

### 8.5 视觉选项模型

ColorPicker 的视觉选项通过 public API 归一为 theme variables、伪类或模板绑定。Token 保存组件语义值，不能保存实例运行时状态或业务色值。

## 9. 文档导航与验证策略

关联文档：

- [ColorPicker 桌面版实现原理](implementation.md)
- [ColorPicker 确认与取消设计](confirmation-design.md)
- [ColorPicker Semantic Part 契约](semantic-part.md)
- [ColorPicker Token 设计](token.md)
- [ColorPicker Changelog](changelog.md)

验证策略：

| 改动类型 | 验证要求 |
| --- | --- |
| 文档改动 | 运行 `git diff --check`，检查相对链接存在。 |
| Public API | 覆盖属性默认值、事件触发、命令和继承语义。 |
| 状态模型 | 覆盖 open/close、collection/filter、input/value、motion、visual option、disabled、hover、pressed、focus 以及控件特有状态。 |
| AXAML/Theme | 检查 template part、伪类、资源 key、Light/Dark 主题和 Browser 主题。 |
| Token | 检查 TokenKind、AXAML token resource、Token 类型、生成数据和 token.md和文档同步。 |
| Gallery | 走查对应 ShowCase 示例和源码片段入口。 |
