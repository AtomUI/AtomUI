# AtomUI 多设计语言控件与主题定制方案

2026-11-27 · 待架构评审

后续架构讨论明确了控件公共属性、两级行为体系、多设计语言动效与可替换模板的正式解耦边界。本文保留现有实现上的控件评估与修复依据。下文关于共享模板和延后语言级模板选择的建议，以[多设计语言控件设计与实现规范](../../engineering/development/multi-language-control-contracts.md)中的正式架构为准。

## 1. 产品定位与改造目标

Ant Design 继续作为 AtomUI 的主要设计语言和默认参考。Ant Design 的颜色、尺寸、密度、形状和状态细节通过自身取值包在真实控件上验收。Shadcn、PrimeNG 和第三方语言通过同一公开契约给出各自的设计值。控件包负责交互语义、布局不变量、模板和渲染实现。应用负责实例属性、容器布局、自定义内容与可选的局部样式。

本次改造补齐的是**控件对设计语言的消费契约**。现有四类包、单遍求值、局部 `ThemeContext` 和静态主题资产登记继续承担原有职责。保持现有控件类型和主要设计语言基线，按证据修复通用缺陷。对确实有多语言差异的设计维度，补最小且能真正生效的公开入口。组件库默认路径的视觉节点、订阅和绘制成本必须可核验。依据见[设计语言架构](../../architecture/systems/theming/design-languages.md)、[控件 Token 规范](../../engineering/development/control-token-guidelines.md)和[控件注册契约](../../architecture/foundations/control-registration-contracts.md)。

方案以 Int 上报的十项问题为首批验收场景，并扫描本仓公共、桌面及可选控件包，优先复核跨控件路径和高频消费者。移动端只评估本仓公共基元的消费影响。具体控件迁移按触发证据推进。实施结束应满足三点。

- 每条上游问题可追踪到一个行为修复、一个明确的设计能力、一个现有公开入口，或有明确重新立项条件的待复现项。
- 至少一个外部语言消费路径用真实 NuGet 包和公开契约实现首批新增设计能力，Ant Design、Shadcn、PrimeNG 有相应旧基线对照。
- 主路径默认行为、覆盖与清除、局部主题传播、相关交互、生命周期及性能均有证据，热路径保持静态、可裁剪和可测量。

## 2. 定制入口和所有权

| 需求 | 入口 | 责任与边界 |
| --- | --- | --- |
| 值正确却绘制、布局或交互错误 | 控件逻辑、公共基元或 ControlTheme 修复 | 保持现有业务状态和公开语义，修复同一消费者 |
| 语言间颜色、尺寸、形状、排版和动效配方不同 | 现有 Global Token 或 Control Own Token | 跨控件的稳定语义优先复用全局词汇。仅有局部语义时归所属控件。各语言写值，控件共同消费 |
| 应用实例、容器或业务逻辑控制尺寸、位置、内容和状态 | 已有 Avalonia 属性、内容模板、ItemsPanel 或控件公共属性 | Token 提供可主题化的默认值。调用方通过原生属性优先级覆盖，清除后恢复当前作用域的有效默认值 |
| 应用修改一个稳定部位的外观 | 生成的 Semantic Part 专用 Style | 部件确有对外语义，并能验证模板变体中的命中。适用的公开子控件可用现有强类型 Part Theme |
| 一个控件确有不同的视觉结构 | 控件包内受限的静态模板变体。应用也可显式替换 ControlTheme | 先证明值、内容模板和局部样式无法忠实实现目标，再评审结构。内置变体保持同一状态模型、公共部件和键盘交互 |

共享模板可以根据有效属性、状态或有类型的视觉模式选择声明式样式。设计语言取值包继续只提供规则，控件模板不读取语言 ID。需要完整结构替换时，按现有资产规则编译和登记，由控件提供有界变体或由应用显式挂载。应用替换模板时自行承担必要 `PART_*`、焦点目标和所承诺的 Semantic Part。此次十项问题都可以通过现有定制层次解决，无需增加语言到模板的运行时分派或第二套样式系统。若以后出现必须由语言选择完全不同结构的真实场景，单独审查资产选择、作用域切换、状态保持及 AOT 保留成本。依据见[模板变体和应用模板契约](../../architecture/systems/theming/semantic-parts.md#8-template-集成)和[ControlTheme 资产契约](../../engineering/development/control-token-guidelines.md#controltheme-asset)。

一个新 Token 要同时说明使用场景、值域与单位、默认及状态、所属控件、实际消费点、属性覆盖关系，以及修改后应出现的可观察变化。主题值按现有规则求值，控件代码只处理有效值、几何和状态。应用需要直接控制的值复用现有属性，确无等价入口时再评审新属性。只有模板或渲染协作需要的输入维持 internal。公开的 Semantic Part 以用户能理解的部位为单位，内部包装节点和新尺寸公式不自动成为公共契约。用户的 Semantic Style 走生成式类及 Avalonia 原生优先级。

## 3. 上报问题的处置

上一轮以 AtomUI 6.3.0（`582bc6004`）和 Avalonia 12.1.6 运行 Int Gallery 原样例做了 Headless Skia 检查，本方案对照 `release/6.0` 的 `91c061ebd` 核对消费链。表内技术决策是拟实施规格。当前已有数字仅作为复现基线。

| 来源 | 根因和处置 | 交付验收 |
| --- | --- | --- |
| P0-1 按钮文字 | **修复共享布局。** 外层容器定位图文组，呈现器负责在自己的槽内定位内容。把现有 `VerticalContentAlignment` 同时投射到 Button 各模板 `PART_ContentPresenter` 的 `VerticalAlignment` 与 `VerticalContentAlignment`（避免呈现器自身保持 `Stretch` 导致纯 `string` 生成的内部 `TextBlock` 被图标撑高并贴顶绘制；调用方显式设 `Stretch` 时仍保留拉伸能力），并核对 DropdownButton 的实际模板与基模板继承路径。SplitButton 以 `Center` 为主题默认，把其内容对齐投射到主按钮，副区沿用自己的图标布局。Submit、Reset 经主题继承验证。HyperLinkButton、OptionButton 的独立模板分别以实际行为为准。 | Int Large「Build」仍高 32 DIP，20 DIP 图标相对位置稳定，文字由原来的 20 DIP 拉伸槽恢复到约 17 DIP 并居中。检查在前/在后图标、加载、无图标、带自定义内容，以及显式 `Center`/`Stretch` 对齐覆盖和清除。 |
| P0-2 页签文字 | **用跨字体与小尺寸基线复现并与 P0-1 同批收敛。** 原 Int 编辑器页签在 16 DIP 槽里使用 Inter 13px（自然行高约 15.73 DIP）导致偏差被巧合掩盖；改用非 Inter 系统字体（如 macOS `SF Pro Text` / Windows `Segoe UI` 13px）或 `Small` 档 12px 字号配 16 DIP 图标单变量复现，区分布局槽拉伸与字形/图标视觉重心。确认与 P0-1 同源后，同步把 `BaseTabItemTheme` 中 `PART_ContentPresenter` 的垂直对齐接通，并核对普通与卡片模板及各放置方向。 | 非 Inter 字体与 `Small` 档（12px + 16px 图标）均无贴顶偏移，修复同时保留 `HeaderTemplate`、关闭按钮、键盘焦点与原有布局几何。 |
| P0-3 按钮默认贴顶 | **保持默认，使用现有容器布局契约。** 在宽松行内由标题栏、表单或工具栏定义子控件对齐。移除 Setter 会回到 Stretch，复测中 28 DIP 按钮长至 40 DIP。 | Gallery 和 Int 的代表标题栏、工具栏通过 `VerticalAlignment` 居中，仍保持各自尺寸。全库默认变更需作为独立行为提案评审。 |
| P0-4 竖向分隔线 | **先修端点取整，再补齐垂直长度与场景语义。** `AbstractSeparator` 拥有实际线轨几何。按设备像素推导同一中心和两端，奇偶不匹配时选与容器同奇偶的最接近长度，优先维持清晰端点。拟新增 `VerticalLineLength`，复用已支持编译的 `Dimension`：百分比相对有效垂直槽，绝对值使用 DIP；在拉伸或显式高度约束下按 `Min(有效长度, 可用高度)` 居中绘制。现有语言默认 60%，保留字体决定的自然高度。评审时一并明确“工具栏（对齐 14–16 DIP 图标）”与“行内文本（约 0.9em）”两类场景的长度分化（评估是否联动 `SizeType` SM/Middle/LG 或明确标准调用约定），避免单个默认值在两类场景顾此失彼。 | 原 13 DIP 外框、8 DIP 线轨的上下不对称可回归。比较 1×、一个分数缩放及工具栏/行内两条实际路径。覆盖 60% 和绝对长度、拉伸裁剪、属性覆盖、布局更新、横向分隔线。 |
| P0-5 局部暗色黑字 | **在内容表面成对声明背景与前景，并补齐文本交互控件的默认前景绑定。** `ThemeConfigProvider` 切换资源和变体，具体表面拥有实际底色。标准中性表面在现有 Border 或其它宿主上声明匹配的背景及 `TextElement.Foreground`，都从同一作用域取值，避免在透明作用域中无条件覆盖继承色。窗体、已自有主题的弹层（含 `DrawerInfoContainer`）优先补齐表面契约；同时复核 `TreeView`（含自定义 `StackPanel` Header 的未选中 `TreeViewItem`）、`ListBox`、`TabControl`、`GroupBox`、`Card` 等自带文本展示职责的控件，确保其常规未选中态直接绑定当前作用域的 `ColorText` 而非依赖外层窗体继承。应用自定义表面同样用现有 AXAML 入口与官方示例。 | 浅外层/深内层、同明暗但用不同语言、只覆盖密度或局部 Token、清除和重挂载、显式文字色、TreeView 自定义 Header 及 Drawer 实测均无暗底黑字。表面内控件的选中和禁用前景保持各自优先级。 |
| P0-6a 圆角内容裁切 | **复用现有外框，并评估开放通用圆角裁切入口。** Splitter 模板已经使用拥有 `ClipContentToCornerRadius` 的 `PixelAlignedBorder`。在根裁切有效且外框圆角非零时，通过主题投射启用子内容裁切。普通直角实例沿原矩形路径。先核对同一内边缘几何在 Arrange、子替换和缩放变化的更新条件，避免在 Splitter 拖拽时对未变化的裁切输入重建几何。同步评估将经过验证且保留外部 `BoxShadow` 的 `ClipContentToCornerRadius` 能力作为 `AtomUI.Controls` 公开基元或附加属性开放，避免下游应用与 Gallery 为圆角卡片、浮层底栏和封面图各自重复实现裁切容器。 | 实际分栏页面保留外框、拖拽和折叠；外圆角可见区域与命中一致。验证边角、拖动靠近边缘、根裁切显式关闭、非零边厚，以及现有外框能力保留外部阴影；若开放公开入口则补通用卡片/阴影对照验收。 |
| P0-6b Splitter 方向 | **补齐 API 注释和 Gallery 约定。** `Orientation` 表达分隔条方向，保留 `Vertical` 左右分栏和 `Horizontal` 上下分栏。 | 两种现有 XAML 的布局轴、指针拖动轴和键盘方向保持一致。若真实消费需要面板轴语义，单独评审命名清楚的入口及与既有属性冲突时的行为。 |
| P1-1 ToggleSwitch 圆角 | **新增两个真实渲染输入。** `TrackCornerRadius` 和 `HandleCornerRadius` 拟复用 `Dimension`。百分比相对各自绘制矩形的短边，50% 还原现有胶囊并随实际尺寸变化。Int 可给 3 DIP 等绝对值。轨道、内描边及键盘焦点从同一有效外圆角派生，滑块在正常与按压伸展状态按实际矩形计算，并核对矩形滑块与轨道在 `TrackPadding` 下的同心圆角关系与像素对齐。 | 三个现有语言保有基线形状，Int 正确绘制小圆角矩形。样式切换后重绘，覆盖开关、按下、禁用、焦点、正常与紧凑密度；改半径单独断言几何和像素变化。 |
| P1-2 Segmented 外框和焦点 | **接通可配置的外框。** 新增 `TrackBorderColor` 与 `TrackBorderThickness`，通过默认主题设置控件已有 `BorderBrush` 与 `BorderThickness`，现有 Frame 消费并安排内容内框。默认透明且宽度 0。背景仍由 owner 绘制，滑块的布局约束应用同一内边界。聚焦线宽、颜色和偏移先走已有焦点装饰器。 | Int 的 1 DIP 外框可单独取值。选中滑块在移动过程中不盖边框，零边厚不改变现有尺寸。键盘导航、原有 Tab 停靠和方向键行为保留。只有原 Figma 场景证明需整组焦点环时，再明确焦点目标映射。 |
| P1-3 ProgressBar 不确定态 | **修复直线进度条的状态契约。** 保留 `Value`，由既有进度段和布局路径显示独立于 `Value` 的不确定态指示段。用现有 Motion/合成时钟管理运行，动效关闭时显示静态忙碌段。不确定态由主题的 `:indeterminate` Setter 默认隐藏自动信息，显式属性和 `ProgressInfoContent` 维持当前覆盖约定。 | 原例从仅显示底轨或 `0%` 变为可辨别的忙碌状态。改变 `Value` 不驱动不确定态进度段。切回确定态恢复最新值和自动信息，覆盖运行期切换、禁用动效、显隐、替换模板、脱离，以及自定义进度信息。圆形、仪表盘和分段进度条按各自契约核对，本次实施范围是直线进度条。 |
| P1-4 Icon 着色 | **补宿主到图标的 AOT 友好着色契约与禁用态退化策略。** 在 `AtomUI.Controls` 公共层（`Icon` 基类强类型 `StyledProperty` 或零反射公开接口）与 `IconPresenter` 之间增加有类型的着色模式，区分保持既有行为、保留图标调色板和用已解析前景给单色图标着色；模式枚举保持宿主无关，以便后续平滑扩展至 `MenuItem`、`TreeViewItem`、`ListBoxItem` 等选中态反白宿主。按钮模板按有效视觉状态声明模式，画刷仍走现有路径。同时明确当宿主采用 `DisabledByOpacity=false` 时禁用态下多色图标的退化策略（单色图标用禁用前景色着色，多色图标保留图层并通过禁用不透明度降对比度，避免禁用按钮内多色图标仍保持 100% 鲜艳）。普通 PathIcon 和未实现该契约的旧图标继续使用现有画刷行为，应用显式提供的画刷优先。 | 先验收 Button 和 SplitButton 主副区域。Int 原色、填充表面、强调色、禁用态（含 `DisabledByOpacity=false` 下的单色与多色图标）、非白色高对比前景、显式画刷和多色图标都可区分。图标替换、重套模板、脱离及局部切换后无旧模式残留。通过后移除 Int 对这些已标注宿主的色值推断。 |

控件与场景依据见 [Button 模板](../../../src/AtomUI.Desktop.Controls/Buttons/Themes/ButtonTheme.axaml)、[普通页签模板](../../../src/AtomUI.Desktop.Controls/TabControl/Themes/BaseTabItemTheme.axaml)、[分隔线布局](../../../src/AtomUI.Controls/Separator/AbstractSeparator.cs)、[圆角外框](../../../src/AtomUI.Controls/Primitives/DashedBorder.cs)、[ToggleSwitch 渲染](../../../src/AtomUI.Controls/Switch/AbstractToggleSwitch.cs)、[Segmented 渲染](../../../src/AtomUI.Controls/Segmented/AbstractSegmented.cs)、[进度条布局](../../../src/AtomUI.Controls/ProgressBar/LineProgressPanel.cs)和[图标呈现器](../../../src/AtomUI.Controls/Icon/IconPresenter.cs)。

## 4. 现有控件的全库扫描与复核

扫描以当前 Git 跟踪的源码为范围，覆盖 1,559 个 C# 文件和 384 个 AXAML 文件。按内容对主题属性、几何绘制、状态投射和资源生命周期做文本筛查，随后复核下表候选的真实消费路径。Common 和 Desktop 中分别有 10 和 338 个 AXAML，DataGrid 和 ColorPicker 各有 18 个。Extras 当前只有包注册与元数据。文件数表示筛查范围，以下结论按源码可确认的层次标注，性能和像素结果仍需执行对应验收。

| 优先级 | 控件与证据 | 根因处置与立项条件 |
| --- | --- | --- |
| 近期，源码确认重复工作 | **IconPresenter 首次挂载。** 预设 `Icon` 时 `AddIcon` 立即调用 `ConfigureIcon`，首次 attach 又调用一次。每次重建 7 条 AtomUI Icon 绑定，普通 PathIcon 是 3 条。常见 Button、Menu、Tab 等均使用该呈现器。[IconPresenter 绑定路径](../../../src/AtomUI.Controls/Icon/IconPresenter.cs#L65) | 统一绑定所有权，使同一图标在首次可用时只建立一次有效连接。先验证离树 Measure、首帧画刷状态、旧对象释放及替换与重挂载路径。量测同场景的实际分配，再接入 P1-4 着色模式。 |
| 近期，源码确认非目标输入成本 | **SliderTrack 全局输入。** 每个已挂载 Track 先尝试把原始指针坐标转换成自身坐标，再判断是否是五种按下事件。指针移动能走到转换却不会使用结果。[Slider 订阅](../../../src/AtomUI.Desktop.Controls/Slider/SliderTrack.cs#L405) · [坐标转换](../../../src/AtomUI.Core/Controls/VisualExtensions.cs#L130) | 先按事件类型过滤，再转换坐标，范围小且无需改公开 API。验收单值、多滑块、窗内外点击、拖拽和键盘焦点。只有实测证明订阅分发仍有明显成本时，才收窄订阅到真正需要失焦检测的生命周期。 |
| 本期联动，源码确认覆盖风险 | **ColorPicker 的色相、透明度和渐变滑轨圆角。** 滑轨和滑块尺寸变化会重新写 `CornerRadius`，模板直接读取该属性。滑块内层还独立按宽度计算圆角。已有 Token 可调轨道和滑块大小。[滑轨半径更新](../../../src/AtomUI.Desktop.Controls.ColorPicker/ColorSlider/AbstractColorSlider.cs#L79) · [滑块半径更新](../../../src/AtomUI.Desktop.Controls.ColorPicker/ColorSlider/ColorSliderThumb.cs#L37) · [轨道尺寸 Token](../../../src/AtomUI.Desktop.Controls.ColorPicker/AbstractColorPickerViewToken.cs#L41) | 复用 ToggleSwitch 批次中的默认几何与显式覆盖分离规则。自动胶囊按实际绘制区域算成有效半径，显式圆角在 resize 和主题切换后仍有效。先确认结构与公开可达入口。取值包确有稳定风格需求时在 ColorPickerView 的 Token 层提供输入，保留棋盘底和渐变为自身内容语义。 |
| 本期联动，需原场景复现 | **Drawer 自定义内容前景。** `DrawerInfoContainer` 绘制 `ColorBgElevated`，标题、内容和页脚所在分支依赖继承的前景。Drawer 通过逻辑父节点连接 owner。PopupRoot 和 OverlayPopupHost 已有可对照的前景 Setter。[Drawer 表面和文字](../../../src/AtomUI.Desktop.Controls/Drawer/Themes/DrawerInfoContainerTheme.axaml#L29) · [Drawer 逻辑挂载](../../../src/AtomUI.Desktop.Controls/Drawer/DrawerContainer.cs#L624) · [Overlay Popup 前景](../../../src/AtomUI.Desktop.Controls/Popup/Themes/OverlayPopupHostTheme.axaml#L25) | 让 Drawer 成为 P0-5 的真实弹层验收候选。用局部暗色和浅窗体打开含普通 TextBlock 的 Drawer。确认继承错误后在既有表面 owner 补前景，验证外部文字、显式前景、关闭和重开。 |
| 设计语言能力，源码确认固定值 | **Line Tab / TabStrip 竖向内边距。** 两套模板在 Left/Right 的三档尺寸都固定为 `8,4`。Card 有自己的 `VerticalItemPadding`。横排的尺寸档已由资源绑定扩展映射到不同 Token。[Line Tab 竖向模板](../../../src/AtomUI.Desktop.Controls/TabControl/Themes/TabItemTheme.axaml#L30) · [TabStrip 竖向模板](../../../src/AtomUI.Desktop.Controls/TabControl/Themes/TabStrip/TabStripItemTheme.axaml#L33) · [资源映射](../../../src/AtomUI.Desktop.Controls/TabControl/TabTokenResourceBindingExtension.cs#L112) | 有真实竖向密度差异时，在 Line 家族提供一个有消费者的竖向 padding Token，现有语言依旧给 `8,4`。依实际验收决定是否需要三档。应用局部调整优先试现有尺寸和 Semantic Part。该项可独立于 P0-2 的字形偏差。 |
| 设计语言能力，源码确认形状边界 | **Slider 滑块。** Rail 和 Track 已有可用的 Border Semantic Part，默认圆角由轨道厚度计算。滑块本体及焦点轮廓在 `Render` 直接画圆。[Slider 部件](../../../src/AtomUI.Desktop.Controls/Slider/Slider.SemanticParts.cs#L6) · [轨道圆角](../../../src/AtomUI.Desktop.Controls/Slider/SliderTrack.cs#L1005) · [滑块绘制](../../../src/AtomUI.Desktop.Controls/Slider/SliderThumb.cs#L229) | 轨道局部样式沿用已有入口。取得确实需要圆角矩形滑块的设计后，再评审一个同时驱动滑块和焦点轮廓的形状输入。多滑块命中、移动约束、标记与轨道绘制各自保有原语义。 |
| 聚焦验证，源码确认重复声明 | **DataGrid 分组表头对齐。** 组对象把对齐传给 Header，模板呈现器又同时有模板绑定和一个直接写 Left/Center 的 Setter。普通列头的默认值位于 Header owner。[组对象投射](../../../src/AtomUI.Desktop.Controls.DataGrid/Column/DataGridColumnGroupItem.cs#L283) · [分组表头模板](../../../src/AtomUI.Desktop.Controls.DataGrid/Themes/DataGridColumnGroupHeaderTheme.axaml#L13) · [普通表头默认值](../../../src/AtomUI.Desktop.Controls.DataGrid/Themes/DataGridColumnHeaderTheme.axaml#L83) | 先测分组头显式 Right/Top 和清除后的默认对齐，确定实际属性优先级。若 Setter 冗余或覆盖错误，收敛成 owner 上一个默认与模板内一处转发。验证冻结组、排序筛选共存和两级分组头。 |
| 性能候选，先采样 | **DataGrid 冻结列阴影。** 单元格模板各带一个默认隐藏的阴影节点，表头和 HeaderView 也有独立阴影节点。宽度与平移均固定为 30 DIP，画刷形状已有主题输入。[单元格阴影节点](../../../src/AtomUI.Desktop.Controls.DataGrid/Themes/DataGridCellTheme.axaml#L37) · [左右边界投射](../../../src/AtomUI.Desktop.Controls.DataGrid/Themes/DataGridCellTheme.axaml#L81) | 先在真实冻结列场景统计已实现单元格、阴影节点、分配和帧时间。成本成立才评估把共享边界的绘制交给现有 Grid 宿主层。必须保留左右冻结、变高行、表头、层叠顺序、虚拟化及不同滚动位置的裁切。30 DIP 只有在外部语言确有独立宽度需求时才评估入契约。 |
| 性能候选，先采样 | **DateViewer 布局快照。** DatePanel 每次 Arrange 都组合几何并创建数组，之后才比较新旧布局。日期范围的共享悬停另有原始输入回调。[日期布局快照](../../../src/AtomUI.Desktop.Controls/DateViewer/Internal/DatePanel.cs#L426) · [共享悬停路径](../../../src/AtomUI.Desktop.Controls/DateViewer/Internal/DatePanel.cs#L398) | 优先测同一月历多次布局的分配和 GeometryChanged 频率。有实际收益时延后已发布快照的复制，保持消费者获得的快照稳定。双月跨区悬停、遮罩和日期定位按原行为验证，输入订阅调整单独取证。 |

CheckBox、RadioButton 已分别为标签槽设置居中并开放标签及指示器部件，CardGridItem、ListViewItem 和 DataGridCell 也已转发现有内容对齐，维持各自模板。ScrollViewer 的零圆角页按键属于点击区域，轨道内边距已有 Token。DataGrid 和 ColorPicker 的包仍独立，以它们的 UI 语义和性能阈值验收。原生 caption 与 DPI/缩放相关的几何只在平台证据成立时调整。[CheckBox 标签](../../../src/AtomUI.Desktop.Controls/CheckBox/Themes/CheckBoxTheme.axaml#L22) · [RadioButton 标签](../../../src/AtomUI.Desktop.Controls/RadioButton/Themes/RadioButtonTheme.axaml#L27) · [ListViewItem 模板](../../../src/AtomUI.Desktop.Controls/ListView/Themes/ListViewItemTheme.axaml#L20) · [ScrollBar 模板](../../../src/AtomUI.Desktop.Controls/ScrollViewer/Themes/ScrollBarTheme.axaml#L29)

### 4.1 完整覆盖与未使用路径的实测

代码基线为 91c061ebd，现象复核使用本地 Debug/net10.0 产物、Avalonia 12.2.0 和 Headless。临时进程只注册 Shadcn 与桌面控件，加载的设计语言程序集只有 AtomUI.DesignLanguages.Shadcn。下列计数度量对象与依赖，并非耗时或内存收益。

| 入口 | 实际观察 | 对设计的影响 |
| --- | --- | --- |
| 初次按类型键取主题 | 资源先于控件挂载。同一最小宿主用延迟工厂分别统计默认主题/默认树、语言主题/语言树。只用默认时为 1/1、0/0。框架默认仍挂在公开类型键、根 Style 另选语言时为 **1/0、1/1**。把类型键的延迟入口直接指向已选语言时为 **0/0、1/1**。语言主题显式 BasedOn 默认且覆盖 Template 时为 **1/0、1/1**。 | 单作用域机制实验。必须在取资源前选择来源，还要保住用户资源与多作用域优先级。已有登记与延迟工厂可以用于验证，无需为此新增运行时统计服务。 |
| 8 个普通 Shadcn 按钮 | 8 个按钮均关闭波浪、均未加载。各自仍有 1 个 WaveSpiritDecorator、1 组已初始化的波浪 Token 观察对象、1 个不可见的 LoadingOutlined。 | 优先迁移交互反馈，关闭时去掉每实例专属观察。加载图标检查是否能只延迟具体字形，同时保留稳定 Semantic Part 及第一次进入加载态的正确呈现。隐藏节点数不能当成运行中的动画数。[Button 模板](../../../src/AtomUI.Desktop.Controls/Buttons/Themes/ButtonTheme.axaml) · [现有图标部件契约](../../controls/desktop/general/button/semantic-part.md) |
| ProgressBar 与 Select | 进度值 30、信息区关闭时，ProgressBar 的成功、异常图标均已存在。Select 首次关闭时候选列表为空，首次打开才构造。 | 状态替代内容应先区分必要锚点与具体内容工厂。进度图标还须考虑公开 getter 的兼容性。Select 是保留轻量壳、延迟重内容的现有正例。[进度图标默认值](../../../src/AtomUI.Controls/ProgressBar/AbstractProgressBar.cs) · [Select](../../../src/AtomUI.Desktop.Controls/Select/Select.cs) |
| 资源查找与作用域 | 复用两项现有定向测试，2/2 通过。无键清单的测试字典在首次命中时只建 1 个；随后查询不存在的键，6 个测试字典全部已构造。空继承作用域复用父快照。 | 生成资产提供完整键索引，不能为了判断 miss 加载无关字典。未知的外部资源提供者依然保留查找机会。用于验证全部资产的显式入口独立于普通启动。[查找实现](../../../src/AtomUI.Core/Theme/Resources/StartupResourceProvider.cs) · [资源工厂对照](../../../tests/AtomUI.Core.Tests/Registration/ControlRegistrationRuntimeTests.cs) |
| 弹层默认动效 | 源码核对：PopupMotionDefaults 同一个静态类型初始化 14 个预设定义。无配置 owner 的 Flyout 在创建 Popup 时先调用默认开闭定义工厂，再把结果以 Style 优先级写入，显式 null 或自定义值可以遮住该结果。 | 这会产生未被选中的**定义对象**。按槽位解析后再构造，显式 null 仍关闭该方向，ClearValue 后首次需要默认轨迹时再取。静态定义与实际播放、帧订阅分别计数。[预设定义](../../../src/AtomUI.Desktop.Controls/Popup/PopupMotion.cs) · [Flyout 默认配置](../../../src/AtomUI.Desktop.Controls/Flyouts/Flyout.cs) |

同类风险还包括先构造默认行为再挂语言行为、切 Token 时重建同一模板、未用的单元格装饰按每个已实现容器重复创建。前两项纳入新解析器的冷/热路径验收，DataGrid 冻结阴影的取证条件沿用上表。当前结果支持定向处理，尚未运行新架构的完整语言覆盖或 NativeAOT 产物。

### 4.2 首次装载的源头对照

Avalonia 12.2.0（5d9e6d1b），独立 Headless 进程，两个相同类型的控件。窗口资源先于内容挂载。外层类型键延迟选择 Shadcn，内层类型键延迟选择默认；工厂和 ControlTemplate 内部分别计数。应用提供的用户主题在需要的对照中显式构造。下表数字顺序均为默认、Shadcn、用户。

| 单变量场景 | 主题工厂数 | 视觉树构建数 | 结论 |
| --- | --- | --- | --- |
| 双作用域，无用户覆盖 | 1 / 1 / 0 | 1 / 1 / 0 | 两个作用域都得到预期主题 |
| 增加 Application.Resources 同名类型键 | 1 / 1 / 1 | 1 / 1 / 0 | 就近类型键遮住原本应优先于框架的用户资源 |
| 应用普通 Style 给 Theme 显式赋值 | 1 / 1 / 1 | 0 / 0 / 2 | 最终两个用户视觉树都正确，但两个框架候选已在隐式查找时构造 |
| 挂载前两个实例都显式指定用户 Theme | 0 / 0 / 1 | 0 / 0 / 2 | 原生已有有效 Theme 时能跳过隐式工厂；该对照不作为框架写 LocalValue 的实现方案 |

这两组原包对照限定了接入位置。键选择须发生在首次取值之前，应用样式仍须胜过框架默认。仅新增局部类型键会破坏资源优先级，仅靠应用基线 Style 又迟于原生隐式查找。

### 4.3 首次装载联合验证

2026-11-27，在 AtomUI `0c95ee2d2` 工作树上，以官方 Avalonia / Avalonia.Headless 12.2.0 做对照。原包 `lib/net10.0/Avalonia.Base.dll` 与 NuGet 归档中的字节一致，SHA-256 为 `007ab726f2bd1f90fb275c90b11f7dc8dda33e0afae382e538f251f30438098d`。该版本在逻辑挂载时先重查隐式主题，然后应用主题和普通 Style。资源提供者收到键和变体，根级提供者拿不到发起查找的控件。[官方 StyledElement 源码](https://github.com/AvaloniaUI/Avalonia/blob/5d9e6d1b0e87337f74f233bead580db186412357/src/Avalonia.Base/StyledElement.cs#L819-L865) · [官方资源遍历](https://github.com/AvaloniaUI/Avalonia/blob/5d9e6d1b0e87337f74f233bead580db186412357/src/Avalonia.Base/Controls/ResourceNodeExtensions.cs)

**对照方法。** 共用一个窗口，外层 Shadcn、内层 AntDesign，两区明暗变体相同。每个场景重新创建资源提供者和计数器，分别在未缓存主题的构造入口与模板的 Build 入口计数。官方原包侧保留公开类型键，使用最低优先的应用 Style 提供作用域主题。候选侧在应用私有的 12.2.0 程序集副本中用 Mono.Cecil 接入托管样式引擎，无需基线 Style。两侧运行同一组输入。表内每格依次为 **主题工厂 / 模板建树**，四元组顺序为 **公开基础、外层语言、内层语言、用户**。用户主题在需要时由场景显式提供。

| 输入 | 官方原包 + 基线 Style | 定向托管适配 |
| --- | --- | --- |
| 应用普通 Style 为两个控件指定完整主题 | `1,0,0,1 / 0,0,0,2` | `0,0,0,1 / 0,0,0,2` |
| 无用户覆盖，同一窗口两个作用域 | `1,1,1,0 / 0,1,1,0` | `0,1,1,0 / 0,1,1,0` |
| 仅内层控件有用户类型键 | `1,1,0,1 / 0,1,0,1` | `0,1,0,1 / 0,1,0,1` |
| 外层显式 BasedOn 并继承基础模板，内层选自己的语言 | `1,0,1,0 / 1,0,1,0` | `1,0,1,0 / 1,0,1,0` |
| 同一 BasedOn，外层改用自己声明的 Template | `1,0,1,0 / 0,0,1,0` | `1,0,1,0 / 0,0,1,0` |

最后一行的派生模板由编译 AXAML 定义，另外断言它的命名 Border 已实际出现；四个候选的模板计数不含这棵派生视觉树。两条 BasedOn 用例均核对基础对象与公开类型键返回的对象为同一实例。应用或局部普通 Style 胜出时，候选侧还断言框架隐式解析调用数为 0。移除应用 Style 或应用类型键后，两控件分别呈现各自语言模板。实例显式主题和应用合并字典的覆盖也分别通过。

**最终原型怎样接入。**

1. 在 `ApplyStyling` 和 `GetEffectiveTheme` 的入口，只对本应用已启用且参与解析的控件，在第一次需要时复用原生样式批次，先附加父模板的样式，再匹配普通 Style。批次结束后读取有效 Theme。利用既有应用标志避免重复匹配；初始化中的控件延至 EndInit 再求隐式值。
2. 保留非空 Theme 快速返回和 Avalonia 的隐式缓存。只替换冷缓存上的 `TryFindResource` 调用，向解析器传入发起控件；命中用户资源时取其值，轮到已挂载的框架提供者时，才按起点所在作用域取资产。常规资源读取和编译 AXAML 的词法 StaticResource 保持原路径。移除覆盖和作用域变化复用原生隐式主题失效。输入和输出分别是当前来源与获选主题，无需给业务控件设置 LocalValue。
3. 一版只跳过首次隐式查找的试验在 `Theme=null` 时丢失模板。另一版只提前匹配普通 Style，父模板对子控件指定完整 Theme 时，内层语言工厂仍执行 1 次。最终版把两个来源都纳入结算。实例或普通 Style 显式置 null 时，内层语言主题各为 1 次工厂、1 次建树，切换作用域后更新；父模板给子控件指定完整 Theme 时，框架三方均为 0，用户为 1/1。初始化批次和参与类型在应用关闭此机制时的原生回退也通过。

**验证范围。** JIT 运行了八个共同场景及五个候选边界场景。Release NativeAOT 使用 .NET SDK 10.0.403、运行时 10.0.14 和 `osx-arm64`。官方原包二进制通过九个共同场景，适配二进制通过这九项及五个边界场景。核对了两份 ILC 输入路径并实际运行发布产物。另以真实 AtomUI Window、Button 子类、启用两套语言包的应用和内层 ThemeConfigProvider 在 JIT 重跑八个共同场景，使用可计数的主题资产，并在当前实例上往返切换语言。

**生产边界。** 原型的资源定位针对已知挂载结构；真实语言包的 `ThemeManager.Resources`、第三方提供者的查询顺序及失效订阅生命周期仍需在正式集成时验证。适配落点属于托管样式系统。AtomUI.Native 的窗口钩子无法改变此处的执行顺序。优先采用上游接入；固定版本的应用私有适配还要解决可分发产物的身份、版本校验和 ILC 输入一致性。此次 Headless 工厂计数不测量完整窗口启动耗时。发布候选仍按现有标准比较 Release Gallery 完整 Shell 的 JIT 配对中位差和 AOT 同机交错采样。[主规范](../../engineering/development/multi-language-control-contracts.md#51-在第一次消费时选定实现) · [启动验收线](../../architecture/systems/startup/verification.md#2-验收线)

## 5. 消除同类问题的方法

在当前 Common 与 Desktop 源码内，文本筛查找到 344 个含 `<ControlTheme` 的 AXAML 文件、149 个提及 `ContentPresenter` 的 AXAML 文件，以及 87 个出现 `VerticalContentAlignment` 的 AXAML 文件。三组统计有交集，文件数仅用于建立候选范围。举一反三时按下表检查真正生效的属性和模板，命中字符串本身不算缺陷。

| 同类问题 | 已核对的参照 | 扩展检查 |
| --- | --- | --- |
| 父级属性设置成功，最终呈现器漏接 | SelectCandidateListItem 的内容呈现器已绑定 `VerticalContentAlignment`。Button 内的布局组有绑定，最终呈现器无绑定。 | 首批核对 Button 家族、普通和卡片 Tab、CheckBox、RadioButton。相同的属性名要先确认含义及数据流。只修同时满足语义相同、有失效证据的模板。 |
| 复合控件中的子控件从错误 owner 取值 | SearchEdit 在自己的模板里传入强类型 `SearchButtonTheme`，子节点保持 Button 的 identity。 | 核对 SplitButton、DropdownButton 和 SearchEdit 的图标、圆角、边框、附属按钮及状态投射。组合控件拥有组合视觉和布局，通用子控件拥有自身基础视觉。 |
| 绘制代码把特定语言的比例固化成能力边界 | ToggleSwitch 已有多语言轨道边框和按压伸展策略。Segmented 已分别消费轨道和选中滑块的圆角。 | 从可见且已知存在语言差异的常量入手，着重检查内外轨尺寸、焦点轮廓、描边、裁切和状态色配对。先看现有 Global Token、公共属性或共享基元，再决定增加 Own Token。 |
| 局部资源切换了，颜色或生命周期仍指向旧根 | Provider 具有局部资源和变体。普通 Popup/Flyout 通过逻辑父级获得上下文，独立 TopLevel 已有 lease/bridge 机制。 | 用暗色内容表面和一个真实弹层检查启用、切换、替换根、关闭及脱离。新颜色需求复用现有桥，避免给任意文本节点增加主题订阅。 |
| 视觉与命中裁切不一致，或定制进入高密度热路径 | ListView 等已有圆角外框和容器回收机制。Semantic Part 已区分静态 marker 与使用 Style 后形成的 selector activator。 | 用 Splitter 验证边角几何。用 ListView 作回收对照，检查激活范围和挂接释放。DataGrid、ColorPicker 只在确有共享消费链被改变时纳入验收。 |
| 状态存在，但只更新属性或伪类 | ProgressBar 上报同时暴露了百分比和指示器路径的问题。 | 检查新增或改动的状态是否有完整的状态到呈现链，包括进入、退出、同状态数据变化、禁用动效及无障碍状态。 |

这些检查适用于新控件和改造过的控件家族。复用现有 Own Token 消费分析、视觉字面量诊断、Semantic Part 路径校验和主题生效测试。组合语义、字体光学对齐、裁切命中、焦点等需要实际渲染或运行期断言。能从现有生成事实稳定判定、且已在不同消费者出现的结构问题，才评估增加范围明确的构建诊断。相关参照见 [SearchEdit 主题入口](../../../src/AtomUI.Desktop.Controls/Input/Themes/SearchEditTheme.axaml)、[竖向与列表裁切契约](../../controls/desktop/data-display/list-view/implementation.md#78-条目分割线与圆角内容裁剪)和 [主题生效测试](../../engineering/development/theme-effect-tests.md)。

## 6. 性能、生命周期和兼容性门槛

启动按[主规范的启动性能准入](../../engineering/development/multi-language-control-contracts.md#启动性能准入)单独验收。先固定改造前基线，再用相同首屏对比候选，覆盖单语言和登记多语言但只使用一种的启动路径。

| 面向 | 可验收要求 |
| --- | --- |
| 默认模板 | 值类能力复用现有视觉节点和主题路径。Button 对齐、竖线长度、ToggleSwitch 绘制、Segmented 外框均不为每种语言增加常驻节点。简单静态备选样式留在 AXAML。增加绑定须对应真实消费点，记录每个新增绑定的 owner、默认路径的分配和变化时机。 |
| 热路径 | 保持生成式强类型资源键和现有零托管分配的资源命中约束。输入变化触发最小必要的 Measure、Arrange 或 Render 失效。几何、画笔等复用现有对象或按有效输入更新。待机控件无新增帧回调，动画仅在有效状态和可见期间工作。 |
| 作用域与释放 | 用现有主题快照、取值规则和资源宿主传递有效值。可在 AXAML 等价表达的绑定在 ControlTheme 内完成。动态子图标等跨对象路径先记录无法声明式投射的原因，并对称处理替换、重套模板、脱离及回收。主题对象、容器和桥的回收沿用现有验证方式。 |
| 实测 | 复用真实性能工具。至少按适用范围记录首个布局、代表页面实例化或批量 Measure/Arrange、主题冷切换和缓存命中、每实例分配及视觉节点/订阅数。动画和裁切另测活动进度条或持续拖动的帧时间及分配。相同配置交错多样本，记录基线、候选、中位数与尾延迟，噪声内变化不宣称收益。默认路径出现可复现退化则修正或拆分该项。 |
| 外部语言与发布 | 用生成器核查 Active Own Token 所有启用语言都写值，且作用域配置、显式覆盖和清除有效。更新同批匹配的控件包、语言取值包及生成元数据。新增槽会改变 schema digest，Int 等外部取值包须对新版契约重新构建；现有旧包组合继续用旧版。用正式 NuGet 包消费、ApiCompat 和适用 AOT/裁剪验证确认新组合，复用既有诊断。 |

架构约束和工具见 [资源查询与生命周期](../../architecture/systems/theming/runtime.md#184-资源查询热路径)、[Semantic Part 性能](../../architecture/systems/theming/semantic-parts.md#12-性能与-aot)、[性能采样入口](../../../tools/performances/README.md)、[Token schema digest](../../../src/AtomUI.Core/Theme/Schema/TokenSchemaDigestAttribute.cs)。

## 7. 实施顺序与验收归属

| 批次与归属 | 交付 | 前置条件和退出门槛 |
| --- | --- | --- |
| A · 桌面控件及布局基元 | Button 呈现器对齐（`VerticalAlignment` 与 `VerticalContentAlignment`）、TabItem 非 Inter/Small 基线复现与同批收敛、Separator 端点、Splitter 方向文档。记录 P0-3 的现有容器用法。 | 复用原报告和已有测量。以原场景补可失败验证，修复后保留尺寸、交互和适用主题基线。 |
| B · 主题表面与基础渲染 | 明确内容表面颜色契约，落实一个现有官方主题表面示例，复核 Drawer 以及 `TreeView`/`ListBox`/`TabControl`/`GroupBox`/`Card` 的常规未选中态前景绑定。Splitter 接通已有裁切、处理拖拽时几何更新，并评审公开通用圆角裁切入口。 | 颜色验证依赖局部 scope 和真实弹层宿主。裁切验证依赖原分栏交互及圆角命中证据。两项可以独立验收。 |
| C · 桌面控件和对应取值包 | Separator 长度（含工具栏与行内文本场景区分）、ToggleSwitch 圆角、Segmented 外框。 | 先评审值域和公开契约。Separator 消费 A 的端点算法。同一控件的 schema、模板/绘制、各语言赋值与验证同批合入。 |
| D · 进度条控件 | 直线 ProgressBar 不确定态。 | 独立于 A、B、C，先固定状态与显式值优先级；默认及活动路径通过交互、生命周期和动效成本检查。 |
| E · 图标基础设施与按钮消费者 | 复核并消除 IconPresenter 首次挂载的重复绑定。AOT 友好的可选着色契约与禁用态多色图标退化策略，Button/SplitButton 首批接入。 | 绑定所有权与生命周期先验收。着色类型通过公共契约评审后进入消费端。Button 模板与 A 共享写入点，按顺序合入。 |
| F · 包与集成验收 | 将新版包与代表语言最小实际消费、Int 原页面及必要平台证据汇总。 | 只等待实际合入的 A 至 E 范围。**所有引发 `TokenSchemaDigest` 变更的批次（批次 C 及批次 E 若含新 Token）合并在同一次版本发布中合批出包**，避免外部取值包（如 Int）连续多次应对 Schema Digest 破坏性升级。以发布用构建一次合批验证包匹配、主题生效、AOT 和改动涉及的性能。 |
| 独立 · 高价值后续项 | Slider 输入过滤可以独立实现。ColorPicker 圆角复核复用 C 的几何决策。剩余候选按第 4 节取证。 | 性能项需在真场景测量，设计能力项需确认真实取值需求。未选入本批的独立项不影响 F 的完成。 |

首批主题验证使用 Ant Design 作主要视觉对照，Shadcn、PrimeNG 验证同一控件可消费不同规则，Int 验证外部包路径与原始报告。明暗、密度、状态及平台覆盖按受影响功能选取。新增控件临时用例按规范清理后做最终模块聚焦验证。共用内核测试只保留符合准入的契约。视觉和性能采样复用现有工具，证据记录对应源码、配置和渲染环境。待实施项保持待验收，评审通过后再把确立的通用规则同步到正式架构与控件文档。依据见[测试和验收标准](../../engineering/development/test-value-and-lifecycle.md)和[模块聚焦验证](../../engineering/workflows/affected-verification.md)。
