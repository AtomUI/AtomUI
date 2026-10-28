# ComboBox 桌面版实现原理

本文档描述 ComboBox 桌面版的内部实现范围、源码职责、状态流、生命周期、资源边界和维护规则。公共设计与 API 契约见 [ComboBox 桌面版架构设计](overview.md)，输入表面共享状态见 [输入控件共享架构设计](../../data-entry/input-control-architecture-design.md)，候选列表状态契约见 [候选列表统一交互设计](../../data-entry/select/candidate-interaction-design.md)，变化记录见 [ComboBox Changelog](changelog.md)。涉及控件 Token 的实现应同时阅读 [ComboBox Token 设计](token.md)。

Popup 接入边界：`ComboBox` 负责业务状态和内容准备，template Popup 负责实际显示。模板重建或宿主切换时必须先释放旧 relay，再绑定新的 Popup；普通外点、Escape、失焦和业务关闭在 pinned 状态下被拦截，detach、窗口销毁、跨 TopLevel 和无效锚点必须走生命周期关闭并释放 Popup host。完整状态机见 [Popup 钉住打开设计](../../other/popup/popup-pinned-open-design.md)。

**弹层开合由控件以代码接管，模板不声明 `IsOpen` 绑定。** Avalonia 只在弹层打开瞬间读取一次 `IsLightDismissEnabled`
创建 light-dismiss 遮罩，该属性无变更回调，打开后再抑制撤不掉已创建的遮罩层；而遮罩会吞掉除
`OverlayInputPassThroughElement`（本控件为输入帧）以外的全部命中点。模板绑定会在模板充气阶段、即控件抑制遮罩**之前**
打开弹层，使钉住场景留下整页拦截输入的遮罩。因此 `PART_Popup` 不声明 `IsOpen`：`OnApplyTemplate` 先 relay 钉住状态并
抑制遮罩，之后才在 `IsDropDownOpen` 已为 true 时补开弹层；`IsDropDownOpen` 变更经 `OpenPopup` / `ClosePopup` 开合；
原由 TwoWay 绑定承担的「弹层自行关闭回写」改由 `Popup.Closed` 处理器显式补齐。该形态与共享 `AbstractSelect` 一致，
回归护栏见 `ComboBoxPinnedPopupOverlayTests`。

## 1. 实现定位

本文档覆盖 ComboBox 的控件实现、主题接入、状态同步和 Gallery 可见维护边界。具体属性注册、默认值、绘制细节和 AXAML selector 仍应直接阅读源码；本文只记录维护者必须理解的稳定结构和不变量。

## 2. 源码文件结构

主要源码文件：

- `src/AtomUI.Desktop.Controls/ComboBox/ComboBox.cs`
- `src/AtomUI.Desktop.Controls/ComboBox/ComboBox.SemanticParts.cs`（Semantic Part 声明；owner 本体为 `partial`）
- `src/AtomUI.Desktop.Controls/ComboBox/ComboBoxHandle.cs`
- `src/AtomUI.Desktop.Controls/ComboBox/ComboBoxItem.cs`
- `src/AtomUI.Desktop.Controls/ComboBox/ComboBoxReflectionExtensions.cs`
- `src/AtomUI.Desktop.Controls/ComboBox/ComboBoxTextBox.cs`
- `src/AtomUI.Desktop.Controls/ComboBox/ComboBoxToken.cs`
- `src/AtomUI.Desktop.Controls/Tooltip/OverflowTip.cs`
- `src/AtomUI.Desktop.Controls/ComboBox/Themes/ComboBoxHandleTheme.axaml`
- `src/AtomUI.Desktop.Controls/ComboBox/Themes/ComboBoxItemTheme.axaml`
- `src/AtomUI.Desktop.Controls/ComboBox/Themes/ComboBoxTextBoxTheme.axaml`
- `src/AtomUI.Desktop.Controls/ComboBox/Themes/ComboBoxTheme.axaml`

职责边界：

- 控件主文件保留 public/protected API、Avalonia 属性注册、事件和主要生命周期入口。
- Theme 文件负责静态视觉结构、template part、selector 和资源绑定。
- Token 文件只提供组件视觉变量，不保存实例状态。
- Gallery 文件只展示用法和示例，不作为运行时逻辑 owner。

## 3. 核心类职责

- `ComboBox`：模板协作类型，承载内容展示、宿主或视觉边界。
- `ComboBoxHandle`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `ComboBoxItem`：集合项、节点或容器类型，承载单项状态和模板协作。
- `ComboBoxToken`：控件 Token scope，负责从全局 token 派生控件语义变量。

核心协作规则：

- 控件实例是 public API 和运行时状态 owner。
- Template part 是视觉协作对象，生命周期必须受 `OnApplyTemplate` 或模板加载流程管理。
- 数据对象、选项对象、任务对象或节点对象只保存业务数据，不应反向持有不可释放的视觉对象。
- 弹层、窗口、计时器、异步 loader 和全局管理器必须有明确关闭、解绑或释放路径。

## 4. 状态与数据流

ComboBox 的状态流遵循下面路径：

```text
Public API / ItemsSource / Command / Event
  -> 控件实例状态
  -> internal state / effective state / pseudo-class
  -> template part property / AXAML selector
  -> renderer / popup / adorner / Gallery observable behavior
```

源码中的状态入口按以下语义维护：

- 内容与数据：`ContentLeftAddOn`、`ContentLeftAddOnTemplate`、`ContentRightAddOn`、`ContentRightAddOnTemplate`、`FilterValue`、`FilterValueSelector`、`LeftAddOnTemplate`、`OptionFontSize`、`RightAddOnTemplate`。
- 选择与集合：`SelectedItem`、`SelectedIndex`、`DropDownDisplayPageSize`、`Filter`、`IsFilterEnabled`。
- 交互与状态：`IsAllowClear`、`IsMotionEnabled`、`ShouldUseOverlayPopup`、`Status`、`FormStatus`、`IsShowOverflowTip`、`OverflowTipDelay`、`OverflowTipPlacement`。
- 视觉与布局：`SizeType`、`StyleVariant`。
- 其他稳定入口：`LeftAddOn`、`RightAddOn`。

维护要求：

- 外部设置的 Avalonia 属性必须在模板应用前后保持一致。
- 集合、选择、展开、过滤、分页、上传任务或异步 loader 必须能处理 reset、replace 和 clear。
- `IFormItemAware` 的值读写直接映射到 `SelectedItem`：`SetFormValue(value)` 保留对象实例并设置选择，`GetFormValue()` 返回选择对象，`ClearFormValue()` 清空选择。
- 非编辑态选中内容溢出提示由 `OverflowTip` 托管，只在 `SelectedContentPresenter` 视觉溢出时写入 `ToolTip.Tip`，延迟和位置分别映射到 `ToolTip.ShowDelay` 与 `ToolTip.Placement`；非编辑态显示节点以外层 `AddOnDecoratedBox` 作为 `PlacementTarget`，避免 tooltip 左边按内部文本 padding 对齐；编辑态输入文本仍由 `PART_EditableTextBox` 自己承载，不自动开启该提示。
- 伪类和 internal state 必须从单一 owner 推导，避免双向同步导致循环更新。
- popup 候选必须由单一 active candidate owner 驱动：鼠标命中可用 `ComboBoxItem` 时只迁移候选，不滚动、不提交；`Up` / `Down` 复用同一状态并允许滚动，`Enter` 从 active candidate 写入真实 `SelectedItem`。容器 recycle、ItemsSource / filter 变化和 popup close 必须清理旧投影。
- overview.md 的 API 契约说明应与源码实际状态流一致。

## 5. 生命周期与模板接入

生命周期规则：

- 构造阶段只注册必要状态，不依赖 template part。
- 模板应用时获取 part、建立事件订阅和绑定，并先释放旧 part 订阅。`Popup.Opened` / `Popup.Closed` 与钉住 relay 绑定
  必须在同一处成对退订再订阅，避免模板重应用后残留旧弹层的处理器。
- 模板应用末尾必须在抑制遮罩之后补开弹层（`IsDropDownOpen` 已为 true 时），否则弹层不会出现——模板不再有 `IsOpen` 绑定。
- 控件卸载、弹层关闭、窗口关闭、集合替换或 container recycle 时释放事件订阅和资源宿主。
- `FormFeedback.ValidateStatus` 属于外部对象订阅，在 logical detach 时释放，并在 logical attach 时按当前 feedback 状态重新建立。
- DynamicResource、TokenResourceBinder 或 C# binding 必须有明确 owner 和释放点。
- Browser 和 Desktop 宿主下的主题加载顺序不得影响 public API 语义。

稳定 template part 接入点：

- `PART_ComboBoxHandle`：稳定模板协作入口，重命名前必须同步主题和实现。
- `PART_ContentRightAddOnPresenter`：展示用户内容、文本、图标或模板化数据。
- `PART_EditableTextBox`：承载文本输入、过滤、显示或编辑入口。
- `PART_EmptyIndicator`：展示指示器、进度、分页或状态反馈。
- `PART_FormFeedBack`：稳定模板协作入口，重命名前必须同步主题和实现。
- `PART_ItemsPresenter`：展示用户内容、文本、图标或模板化数据。
- `PART_OpenIndicatorButton`：承载用户触发入口、导航或关闭动作。
- `PART_Popup`：承载弹层宿主、打开关闭或候选内容。
- `PART_TextPresenter`：展示用户内容、文本、图标或模板化数据。

## 5.1 Semantic Part 节点 ownership 与 marker 映射

公共契约见 [ComboBox Semantic Part 契约](semantic-part.md)。本节记录 marker 的物理归属、路由依据与生命周期不变量。

owner 与模板边界：

- `ComboBox` 是唯一 Semantic owner，直接派生 Avalonia `ComboBox`，不共享 `AbstractSelect` / `AbstractAutoComplete`
  基类。`ComboBoxHandle`、`ComboBoxTextBox`、`ComboBoxItem` 都不持有独立 descriptor。
- 三层模板边界决定了 route 形态：owner 模板（`ComboBoxTheme.axaml`）、嵌套控件模板（`ComboBoxHandleTheme.axaml`、
  `ComboBoxTextBoxTheme.axaml`）和模板内 Popup 的独立视觉根（`PopupFrame` 子树）。

marker 归属表：

| Part | marker 位置 | 注入方式 | 路由依据 |
| --- | --- | --- | --- |
| `prefix` | `ComboBoxTheme.axaml` 投影给 `ContentLeftAddOn` 的 `AddOnContentPresenter` | 静态 `Classes.semantic-prefix="True"` | 经 `.semantic-scope-input` / `.semantic-scope-prefix`（由共享 `AddOnDecoratedBoxTheme` 提供）跨模板边界 |
| `frame` | **共享** `AddOnDecoratedBoxTheme.axaml` 的 `PART_ContentFrame`（`AddOnDecoratedBoxContentFrame`） | 静态，`CrossNestedOwners=true` | 宿主模板 `AddOnDecoratedBox` 标 `.semantic-scope-input`，第二个 `/template/` 进入共享模板 |
| `content` | decorated box 内容 `Panel` | 静态 | `/template/ .semantic-content` |
| `placeholder` | `PlaceholderText`（`atom:TextBlock`） | 静态 | `/template/ .semantic-content > .semantic-placeholder` |
| `input` | `PART_EditableTextBox`（`ComboBoxTextBox`） | 静态 | `/template/ .semantic-content > .semantic-input` |
| `suffix` | `ContentRightAddOn` 的 `StackPanel` | 静态 | 经 `.semantic-scope-suffix` 跨模板边界 |
| `indicator` | `PART_OpenIndicatorButton`（`ComboBoxHandleTheme.axaml`） | 静态，`CrossNestedOwners=true` | 宿主模板 `PART_ComboBoxHandle` 标 `.semantic-scope-handle`，`>>` 起步跨入嵌套控件模板 |
| `popup.root` | `PART_Popup` 的直接子节点 `PopupFrame` | 静态，`CrossVisualRoot=true` | `/template/ .semantic-popup-root` |
| `popup.list` | `PopupFrame` 内 `atom:ScrollViewer` | 静态，`CrossVisualRoot=true` | `/template/ .semantic-popup-root >> .semantic-popup-list` |
| `popup.empty` | `PART_EmptyIndicator` | 静态，`CrossVisualRoot=true` | `/template/ .semantic-popup-root >> .semantic-popup-empty` |
| `popup.listItem` | `ComboBoxItem` 容器实例本身 | 运行时注入生成常量，`CrossVisualRoot` + `RuntimeCreated` + `Multiple` | 以 `popup.list` 为锚点，经 `>>` 命中容器 |

关于两类 marker 的关键约束：

- **静态 marker 只允许 `Classes.semantic-*="True"`**。`Classes="semantic-*"` 字面量写法、`False` 或绑定值由
  `ATOMUIGEN029` 拒绝。本契约需要 ComboBox 在自身模板上新增**两个** scope 锚点（`semantic-scope-*` 是只用于路由、
  不发布为 Part 的中间标记）：
  - `.semantic-scope-input` 标在 `ComboBoxTheme.axaml` 的 `AddOnDecoratedBox` 节点上（Gate B 已添加）；
  - `.semantic-scope-handle` 标在 `ComboBoxTheme.axaml` 的 `PART_ComboBoxHandle` 节点上（Gate B 已添加）。
  其余两个锚点复用共享 `AddOnDecoratedBoxTheme` 的既有 `.semantic-scope-prefix` / `.semantic-scope-suffix`，无需新增。
- **`prefix` 需要一次模板投影改造**。改造前 `ComboBoxTheme.axaml` 把 `ContentLeftAddOn` 直接以属性
  `TemplateBinding` 交给 `AddOnDecoratedBox`，没有可挂 marker 的元素。Gate B 已改为元素形式
  `<atom:AddOnDecoratedBox.ContentLeftAddOn><atom:AddOnContentPresenter Classes.semantic-prefix="True" Content="{CompiledBinding $parent[atom:ComboBox].ContentLeftAddOn}" ContentTemplate="{CompiledBinding $parent[atom:ComboBox].ContentLeftAddOnTemplate}" .../></atom:AddOnDecoratedBox.ContentLeftAddOn>`，
  与 `Select` / `LineEdit` / `InfoPickerInput` 的既有写法一致（三处都如此）。该改造让 `ContentLeftAddOn` 多经过一层
  `AddOnContentPresenter`，**行为语义不变**（仍是同一 ContentControl 投影同一属性值），但属布局可达路径变更，必须由
  Gate B 测试与真机视觉验收确认无尺寸/对齐位移；`.semantic-scope-prefix` 锚点位于共享
  `AddOnDecoratedBoxTheme` 的 `PART_ContentLeftAddOn` 上，无需新增。
  注意 `suffix` 不需要同样改造：其 `ContentRightAddOn` 已经是元素形式（`StackPanel`），可直接挂 marker。
- **`frame` 的 marker 位于共享主题，但 owner 作用域由生成 Selector 保证**。marker 加在 `AddOnDecoratedBoxTheme` 的
  既有 `PART_ContentFrame` 节点上（不新增节点、不改共享结构），因此 `Select` / `LineEdit` / `InfoPickerInput` 等同样
  复用该主题的控件在此节点上也会带 `.semantic-frame`。这不构成串味：生成的 `ComboBoxFrameStyle` 以 `Nesting()`（owner）
  起手，只有挂在 `ComboBox` 上的语义 Style 才能命中。该形态与 `ToolTip` 的 `container` / `arrow`
  由共享 `ArrowDecoratedBoxTheme` 承载完全一致。
- **`frame` 需要新增 `.semantic-scope-input` 锚点**，但不需要新增 `.semantic-scope-frame` 之类的中转锚点：route 只有
  「锚点 → 第二个 `/template/`」两段。修改共享主题时必须确认该 marker 与 `.semantic-scope-prefix` /
  `.semantic-scope-suffix` 一并保留，`SharedAddOnThemePath` 上的模板 marker 断言会守住这一点。

- **`popup.listItem` 的 marker 必须打在容器实例上，不能写进 `ComboBoxItemTheme.axaml` 的模板内部节点**。原因是解析器
  要求「携带 marker 的元素本身是 `ContractType`」，标在模板内部节点（如 `ContentPresenter`）与容器 `ContractType`
  不符，Part 将永不命中（系统设计 §8.3 记录的首版失败原因）。同理 item 模板不因语义标记被复制，`BasedOn` 链保持原样。
- **注入点必须幂等且成对覆盖两条容器路径**。ComboBox 的 `NeedsContainerOverride` 对用户显式提供的 `ComboBoxItem`
  返回 `false`（`recycleKey = null`），这类容器不经过 `CreateContainerForItemOverride`，因此
  `PrepareContainerForItemOverride` 也要注入一次；两处都用 `Classes.Add`，重复添加同一 class 是幂等操作，
  与 `TabStrip`、`Breadcrumb`、`TreeView` 的既有写法一致。`ClearContainerForItemOverride` 不摘除 marker：
  marker 表达容器身份而非状态，摘除会在回收复用时造成命中抖动。

状态与数量不变量：

- `placeholder` / `input` / `popup.list` / `popup.empty` 的可见性由状态切换，节点与 marker 常驻，因此都是 `Single`；
  `IsEditable` 在 `placeholder` 与 `input` 之间切换可见性，过滤结果在 `popup.list` 与 `popup.empty` 之间切换。
- `popup.listItem` 为 `Multiple`，数量随 `ItemsSource` 与过滤结果变化，但单个容器的 marker 身份不随
  hover / active candidate / selected / disabled / 过滤隐藏 变化。
- 弹层四部件只在弹层打开后存在于真实视觉树；未打开时不命中属预期，不是契约缺失。
- 模板重应用（`OnApplyTemplate`）后所有静态 marker 由新模板重建，运行时 marker 由容器路径重新注入，两者都不得依赖
  旧实例。

### 5.2 根表面定制（root surface brush relay）

输入族约定：应用对输入控件根表面（`BorderBrush` / `Background`）的定制必须压过共享帧的状态机。可见外框由共享
`AddOnDecoratedBox` 帧绘制、其状态机（rest / hover / focus / status）拥有 `BorderBrush`，因此 owner 上的
`BorderBrush` 默认落不到帧上。ComboBox 直接派生 Avalonia `ComboBox`，不在 `AbstractTextInput` /
`AbstractSelect` 继承链上，`AbstractSelect.RelayRootSurfaceBrush` 覆盖不到它，故 `ComboBox.cs` 自持一份同名中继。

中继规则：

- `OnApplyTemplate` 取得 `AddOnDecoratedBox` 后，立即对 `BorderBrush` / `Background` 各调用一次
  `RelayRootSurfaceBrush`，保证模板应用**之前**设置的定制值也能生效。
- `OnPropertyChanged` 监听这两个属性，运行时改动即时重放；写入方式为
  `_addOnDecoratedBox.SetValue(property, value, BindingPriority.LocalValue)`，`LocalValue` 是压过帧状态机的关键。
- 每个属性各自带一个接管标记（`_isFrameBorderBrushRelayed` / `_isFrameBackgroundRelayed`）。owner 值被清空时，只在
  自身确有接管时才 `ClearValue` 并把属性交还状态机，避免误清内层 owner 的中继（与 `ButtonSpinner` 的接管标记同形）。
- 模板必须把 `IsMotionEnabled="{TemplateBinding IsMotionEnabled}"` 接到 `AddOnDecoratedBox`。帧动效未接线时过渡常开，
  **进行中的 `SolidColorBrush` 过渡优先级高于中继写入的 `LocalValue`**，会让运行中的根边框定制静默失效；模板应用前
  赋值不存在过渡基线，因此只在运行时改值才暴露该缺陷。回归护栏见 `ComboBoxRootSurfaceRelayTests`。

与 `frame` 部件的分工：根表面中继是**输入族的标准根定制入口**（改输入框边框色，作用于 owner 根）；`frame` 部件是
**语义选择器入口**（可用 `.semantic-frame` 的专用 Style 精确命中帧节点，并覆盖状态无关的边框宽度、圆角、背景等）。
两者都作用于同一个 `PART_ContentFrame` 节点，但入口形态与优先级不同：中继写入 `LocalValue`，语义 Style 走
`Style` 优先级，因此根中继会压过 `frame` 专用 Style。契约边界见
[ComboBox Semantic Part 契约](semantic-part.md) §5。

## 6. 交互与事件处理

ComboBox 的交互事件应从输入源收敛到控件级语义事件：

- Pointer、keyboard、focus 和 command 事件不应绕过 Avalonia 基础控件语义。
- 弹层、窗口或 overlay 类路径必须稳定处理打开、关闭、取消、重复打开和宿主失活。
- 集合类路径必须稳定处理 container prepare、clear、过滤、分组和虚拟化回收。
- 输入类路径必须保持 Form、validation、clear、placeholder 和键盘行为一致。
- `FormStatus` 由 `IFormItemAware.NotifyValidateStatus` 写入并绑定到 `AddOnDecoratedBox`；它不能覆盖 `Status`，也不能绕过 `DataValidationErrors` 另建 native error。`AddOnDecoratedBox` 通过共享 `EffectiveStatus` 负责 warning/error 视觉投影。
- Form 值变化通知可以由展示值变化触发，但真实表单值 owner 始终是 `SelectedItem`，不能使用 `SelectionBoxItem` 或 `ToString()` 作为替代状态。

当前没有抽取到控件专属 public 事件；交互语义主要通过继承事件、命令、属性变化和 Gallery 可观察行为体现。

## 7. 内部算法与关键流程

维护者需要重点关注以下流程：

- API 默认值到 effective state 的归一。
- Template part 重新应用时的状态回放。
- 主题资源、Token 和 SharedToken 计算后的视觉更新。
- ItemsSource、selection、checked、expanded、filter、paging 或 upload task 的集合同步。
- 动效启停、初始加载阶段 transition 抑制和卸载取消。

实现文档不逐行解释私有方法。若某个私有算法成为稳定维护入口，应在本节补充算法不变量，而不是把代码复述为说明书。

## 8. 资源、性能与 AOT 边界

资源和 AOT 约束：

- 不通过运行时反射扫描 public API、Token 或 Gallery 示例数据。
- 不把可静态声明的模板结构迁移到 C# 动态创建。
- 异步加载、上传、弹层和窗口生命周期必须能取消或释放。
- 缓存对象必须与控件、窗口、弹层或数据 owner 生命周期一致。
- Source generator 生成文件不手工编辑；需要修改时改输入源或 generator。

性能边界：

- 控件应优先复用 Avalonia 原生虚拟化、模板绑定和资源系统。
- 避免为每次状态变化创建不必要的视觉对象、订阅或动画对象。
- 大集合控件必须保证 container recycle 后不会泄漏旧 item 状态。

## 9. 维护不变量

维护 ComboBox 时不得破坏：

- Public API、默认值、事件顺序和 Gallery 可观察行为。
- Template part 名称、ControlTheme key、伪类和资源 key。
- Semantic Part 的 selector class、route、`ContractType`、cardinality 与 marker 归属；静态 marker 与运行时 marker 的
  注入点必须与 [ComboBox Semantic Part 契约](semantic-part.md) 一致（含 `popup.listItem` 打在容器实例上、在两条容器路径
  幂等注入的约束）。
- 旧 template part、事件订阅、Popup/Flyout/Window host 和 collection view 的释放路径。
- Light/Dark、Browser/Desktop 和不同 SizeType 下的主题一致性。
- 控件文档、源码 public surface、Token 类型或生成数据与源码契约的一致性。

## 10. 测试与验证

推荐验证：

- 纯文档改动运行 `git diff --check` 并检查相对链接。
- 控件 API 或行为变更运行对应 `tests/AtomUI.Desktop.Controls.Tests` 或专用包测试。
- Semantic Part 变更运行 `tests/AtomUI.Desktop.Controls.Tests/ComboBox/ComboBoxSemanticPartTests.cs`：断言 descriptor
  的 Part 数量/顺序/字段（含 `frame` 的自定义边框色压过主题状态色）、静态 marker 集合、运行时容器 marker 的 prepare/clear/recycle 稳定性、生成专用 Style 恰好命中
  一个节点，并断言不存在 code-behind 属性回退。
- 弹层部件变更需覆盖打开 / 关闭 / 重新打开与模板重应用后 marker 保持（用 public `IsPopupPinnedOpen` 钉住取证）。
- Gallery Semantic 预览变更运行 `tests/AtomUIGallery.Tests/ShowCases/ComboBoxSemanticPartHighlightTests.cs`。预览按 AtomUI 标准方式钉住
  模板内 Popup，12 个部件的 marker 全部在场并被断言；钉住状态下还断言**不存在可见的 light-dismiss 遮罩层**，并覆盖
  卡片真实悬停高亮（触发侧 `root` / `frame` / `indicator` 与弹层侧 `popup.root` / `popup.list` / `popup.listItem`）。
  遮罩层若残留会吞掉卡片悬停，使预览高亮整体失效——抑制时机契约见 [语义契约 §6.1](semantic-part.md)。
- 钉住遮罩或弹层开合变更运行 `tests/AtomUI.Desktop.Controls.Tests/ComboBox/ComboBoxPinnedPopupOverlayTests.cs`。
- DataGrid 相关变更运行 `tests/AtomUI.Desktop.Controls.DataGrid.Tests`。
- Gallery 示例或源码片段变更运行 `tests/AtomUIGallery.Tests`。
- AOT、生成器或动态数据路径变更按 Gallery NativeAOT 发布流程验证。

## 状态动效初始化

`ComboBoxItem` 在构造阶段建立共享初始化会话，初始候选及选中背景直接建立；加载后恢复过渡，回收清理前复位。弹层打开、焦点和后续候选变化仍遵循各自的正常动效语义。
共享目标与生命周期契约见 [状态动效初始化架构](../../../../architecture/systems/control-infrastructure/transition-initialization.md)。
