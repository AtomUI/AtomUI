# ButtonSpinner 桌面版实现原理

本文档描述 ButtonSpinner 桌面版的内部实现范围、源码职责、状态流、生命周期、资源边界和维护规则。公共设计与 API 契约见 [ButtonSpinner 桌面版架构设计](overview.md)，公开语义区域契约见 [ButtonSpinner Semantic Part 契约](semantic-part.md)，变化记录见 [ButtonSpinner Changelog](changelog.md)。涉及控件 Token 的实现应同时阅读 [ButtonSpinner Token 设计](token.md)。

## 1. 实现定位

本文档覆盖 ButtonSpinner 的控件实现、主题接入、状态同步和 Gallery 可见维护边界。具体属性注册、默认值、绘制细节和 AXAML selector 仍应直接阅读源码；本文只记录维护者必须理解的稳定结构和不变量。

## 2. 源码文件结构

主要源码文件：

- `src/AtomUI.Desktop.Controls/ButtonSpinner/ButtonSpinner.cs`
- `src/AtomUI.Desktop.Controls/ButtonSpinner/ButtonSpinner.SemanticParts.cs`
- `src/AtomUI.Desktop.Controls/ButtonSpinner/ButtonSpinnerContentPanel.cs`
- `src/AtomUI.Desktop.Controls/ButtonSpinner/ButtonSpinnerDecoratedBox.cs`
- `src/AtomUI.Desktop.Controls/ButtonSpinner/ButtonSpinnerHandle.cs`
- `src/AtomUI.Desktop.Controls/ButtonSpinner/ButtonSpinnerPseudoClass.cs`
- `src/AtomUI.Desktop.Controls/ButtonSpinner/ButtonSpinnerToken.cs`
- `src/AtomUI.Desktop.Controls/ButtonSpinner/Themes/ButtonSpinnerDecoratedBoxTheme.axaml`
- `src/AtomUI.Desktop.Controls/ButtonSpinner/Themes/ButtonSpinnerHandleTheme.axaml`
- `src/AtomUI.Desktop.Controls/ButtonSpinner/Themes/ButtonSpinnerTheme.axaml`

职责边界：

- 控件主文件保留 public/protected API、Avalonia 属性注册、事件和主要生命周期入口。
- Theme 文件负责静态视觉结构、template part、selector 和资源绑定。
- Token 文件只提供组件视觉变量，不保存实例状态。
- Gallery 文件只展示用法和示例，不作为运行时逻辑 owner。

## 3. 核心类职责

- `ButtonSpinner`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `ButtonSpinnerContentPanel`：布局面板，负责测量、排列、虚拟化或集合内容布局。
- `ButtonSpinnerDecoratedBox`：模板协作类型，承载内容展示、宿主或视觉边界。
- `ButtonSpinnerHandle`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `ButtonSpinnerToken`：控件 Token scope，负责从全局 token 派生控件语义变量。

核心协作规则：

- 控件实例是 public API 和运行时状态 owner。
- Template part 是视觉协作对象，生命周期必须受 `OnApplyTemplate` 或模板加载流程管理。
- 数据对象、选项对象、任务对象或节点对象只保存业务数据，不应反向持有不可释放的视觉对象。
- 弹层、窗口、计时器、异步 loader 和全局管理器必须有明确关闭、解绑或释放路径。

## 4. 状态与数据流

ButtonSpinner 的状态流遵循下面路径：

```text
Public API / ItemsSource / Command / Event
  -> 控件实例状态
  -> internal state / effective state / pseudo-class
  -> template part property / AXAML selector
  -> renderer / popup / adorner / Gallery observable behavior
```

源码中的状态入口按以下语义维护：

- 内容与数据：`ContentLeftShift`、`ContentPadding`、`ContentRightShift`、`InnerLeftContent`、`InnerLeftContentTemplate`、`InnerRightContent`、`InnerRightContentTemplate`、`LeftAddOnTemplate`、`RightAddOnTemplate`、`SpinnerContent`。
- 交互与状态：`IsButtonSpinnerFloatable`、`IsButtonSpinnerVisible`、`IsHandleFloatable`、`IsMotionEnabled`、`IsShowHandle`、`IsSpinEnabled`、`Status`。
- 视觉与布局：`HandleOffset`、`SizeType`、`SpinnerBorderThickness`、`SpinnerHandleWidth`、`StyleVariant`。
- 其他稳定入口：`ButtonSpinnerLocation`、`HandleOpacity`、`LeftAddOn`、`RightAddOn`。

维护要求：

- 外部设置的 Avalonia 属性必须在模板应用前后保持一致。
- 集合、选择、展开、过滤、分页、上传任务或异步 loader 必须能处理 reset、replace 和 clear。
- 伪类和 internal state 必须从单一 owner 推导，避免双向同步导致循环更新。
- overview.md 的 API 契约说明应与源码实际状态流一致。

## 5. 生命周期与模板接入

生命周期规则：

- 构造阶段只注册必要状态，不依赖 template part。
- 模板应用时获取 part、建立事件订阅和绑定，并先释放旧 part 订阅。
- 控件卸载、弹层关闭、窗口关闭、集合替换或 container recycle 时释放事件订阅和资源宿主。
- DynamicResource、TokenResourceBinder 或 C# binding 必须有明确 owner 和释放点。
- Browser 和 Desktop 宿主下的主题加载顺序不得影响 public API 语义。

稳定 template part 接入点：

- `PART_ContentFrame`：承载根视觉、边框、背景或尺寸基线。
- `PART_DecoratedBox`：稳定模板协作入口，重命名前必须同步主题和实现。
- `PART_DecreaseButton`：承载用户触发入口、导航或关闭动作。
- `PART_IncreaseButton`：承载用户触发入口、导航或关闭动作。
- `PART_SpinnerHandle`：稳定模板协作入口，重命名前必须同步主题和实现。

## 6. 交互与事件处理

浮动步进按钮的悬停观察按当前输入根隔离，命中判断使用输入位置转换后的装饰框局部坐标。本窗口退出或装饰框卸载时关闭悬停反馈；其他窗口的同坐标输入不参与按钮显示和内容偏移。

ButtonSpinner 的交互事件应从输入源收敛到控件级语义事件：

- Pointer、keyboard、focus 和 command 事件不应绕过 Avalonia 基础控件语义。
- 没有弹层职责的路径不应引入额外 popup 或全局输入捕获。
- 非集合控件不应通过隐藏集合状态模拟业务数据。
- 值提交或命令触发必须保持继承控件的事件顺序。

当前没有抽取到控件专属 public 事件；交互语义主要通过继承事件、命令、属性变化和 Gallery 可观察行为体现。

## 7. 内部算法与关键流程

维护者需要重点关注以下流程：

- API 默认值到 effective state 的归一。
- Template part 重新应用时的状态回放。
- 主题资源、Token 和 SharedToken 计算后的视觉更新。
- 内容、命令和视觉状态在模板节点之间的同步。
- 动效启停、初始加载阶段 transition 抑制和卸载取消。

浮动侧内容偏移由 `ButtonSpinnerDecoratedBox` 统一拥有。只有有效启用、显示 Handle、允许浮动且 hover 时才产生偏移，其余状态的目标偏移为零。侧向距离依据 Handle 宽度和已有内容 padding 计算，并保留半个侧 padding 作为间距；自定义 Handle 宽度和小尺寸均遵守同一规则。消费方应读取已有偏移的当前有效动画值，避免重新计算 hover 位移并启动第二条动画。

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

维护 ButtonSpinner 时不得破坏：

- Public API、默认值、事件顺序和 Gallery 可观察行为。
- Template part 名称、ControlTheme key、伪类和资源 key。
- 旧 template part、事件订阅、Popup/Flyout/Window host 和 collection view 的释放路径。
- Light/Dark、Browser/Desktop 和不同 SizeType 下的主题一致性。
- 控件文档、源码 public surface、Token 类型或生成数据与源码契约的一致性。

## 10. Semantic Part marker 放置与校验边界

公开语义区域的完整契约见 [semantic-part.md](semantic-part.md)。实现侧必须理解 marker 落在哪个主题资产，以及
为什么这样放置才能通过生成器的静态校验：

| Part | marker 所在资产 | 放置节点 | 校验方式 |
| --- | --- | --- | --- |
| `content` | `ButtonSpinnerDecoratedBoxTheme.axaml` | 帧模板主内容 presenter | `CrossNestedOwners`，锚点 `.semantic-scope-frame` 之后的第二个 `/template/` |
| `innerLeftContent` | `ButtonSpinnerDecoratedBoxTheme.axaml` | 帧模板内容左槽 presenter | 同上 |
| `innerRightContent` | `ButtonSpinnerDecoratedBoxTheme.axaml` | 帧模板内容右槽 presenter | 同上 |
| `actions` | `ButtonSpinnerTheme.axaml` | `SpinnerContent` 属性元素子树内的 `ButtonSpinnerHandle` | 宿主模板本地校验（`/template/ .semantic-scope-frame >> .semantic-actions`） |
| `increaseButton` | `ButtonSpinnerHandleTheme.axaml` | 手柄模板增加按钮 | `CrossNestedOwners`，以 `.semantic-actions` 为锚点解析到 `ButtonSpinnerHandle` 主题 |
| `decreaseButton` | `ButtonSpinnerHandleTheme.axaml` | 手柄模板减少按钮 | 同上 |

关键约束：

- marker 必须落在「ControlTheme `TargetType` 等于被声明 owner」的资产中，或在 `CrossNestedOwners` 路由下经锚点
  可解析的嵌套 owner 主题中；否则生成器的模板校验看不到该 marker。帧节点的 marker 因此不能留在
  `ButtonSpinnerDecoratedBoxTheme.axaml` 之外，也不能把 `actions` 的 marker 移到 `ButtonSpinnerHandleTheme.axaml`
  之外——`.semantic-actions` 同时是按钮部件的解析锚点。
- `ButtonSpinnerTheme.axaml` 的 `semantic-scope-frame` 与 `NumericUpDownSpinnerTheme.axaml` 的同名锚点各自保持
  恰好一个，`NumericUpDownSemanticPartTests` 对此有断言。
- **禁止复用 `semantic-prefix` / `semantic-suffix`：** `NumericUpDown` 的 `prefix` route 是宽松后代
  （`/template/ .semantic-scope-spinner >> .semantic-prefix`），而 ButtonSpinner 的帧节点位于 `NumericUpDownSpinner`
  子树内；复用会让该 route 命中两个节点，破坏 `NumericUpDown` 已发布的 `Single` 契约（其测试用 `.Single()` 解析）。
  因此帧内容槽使用与自身公开 API 同名的 `semantic-inner-left-content` / `semantic-inner-right-content`。
- `ButtonSpinnerHandle.Render` 使用 internal `SpinnerBorderThickness` 决定描边线宽，`BorderThickness` / `Padding`
  不参与绘制；`actions` 的定制面因此只包含 `Background` / `BorderBrush` / `CornerRadius` / `Opacity`。
- `NumericUpDownSpinner` 的 Spinner 模式使用自己的模板与 `PART_IncreaseButton` / `PART_DecreaseButton` 节点，它们
  属于 `NumericUpDown` owner，不在 ButtonSpinner 的 Semantic 契约内；该区域若要开放，需在 `NumericUpDown` 家族
  另行完成 Gate A。

## 11. 测试与验证

推荐验证：

- 纯文档改动运行 `git diff --check` 并检查相对链接。
- 控件 API 或行为变更运行对应 `tests/AtomUI.Desktop.Controls.Tests` 或专用包测试。
- Semantic Part 变更额外运行 `NumericUpDownSemanticPartTests`，确认帧节点新增 marker 后 `NumericUpDown` 的
  `prefix` / `input` / `suffix` / `clear` 唯一目标解析与 `Single` 计数未被破坏。
- 布局型 Setter 按 `semantic-part.md` §6 的尺寸基线与手柄占位、`ContentRightShift` 位移、帧裁剪协调结果验证。
- DataGrid 相关变更运行 `tests/AtomUI.Desktop.Controls.DataGrid.Tests`。
- Gallery 示例或源码片段变更运行 `tests/AtomUIGallery.Tests`。
- AOT、生成器或动态数据路径变更按 Gallery NativeAOT 发布流程验证。

## 根表面主题投射

尺寸预设先提供内部默认参数，再由原生 Padding/CornerRadius 决定有效值；内容 inset 与 handle 的方向/占位分别处理。

几何默认、原生优先级和组合边界见[共享输入表面](../../data-entry/input-control-architecture-design.md#81-根表面几何与预设默认值)。

附加区域的背景、描边、三档padding/行高与内容面板SpacingXXS由实际公开owner的Effective Global提供。内部SizeTypeAwareIconPresenter拥有附加图标的尺寸默认；模板IconPresenter通过TemplateBinding消费宽高，并保留自身公开画刷identity。Custom只保留基础IconSize，显式宽高和child本地值遵循原生优先级。NumericUpDown使用的内部NumericUpDownSpinner未独立注册，其共享附加区域沿树归NumericUpDown。
