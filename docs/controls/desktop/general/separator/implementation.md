# Separator 桌面版实现原理

本文档描述 Separator 桌面版的内部实现范围、源码职责、状态流、生命周期、资源边界和维护规则。公共设计与 API 契约见 [Separator 桌面版架构设计](overview.md)，变化记录见 [Separator Changelog](changelog.md)。涉及控件 Token 的实现应同时阅读 [Separator Token 设计](token.md)。

## 1. 实现定位

本文档覆盖 Separator 的控件实现、主题接入、状态同步和 Gallery 可见维护边界。具体属性注册、默认值、绘制细节和 AXAML selector 仍应直接阅读源码；本文只记录维护者必须理解的稳定结构和不变量。

## 2. 源码文件结构

主要源码文件：

- `src/AtomUI.Controls/Separator/AbstractSeparator.cs`
- `src/AtomUI.Controls/Separator/SeparatorEnums.cs`
- `src/AtomUI.Controls/Separator/SeparatorPseudoClass.cs`
- `src/AtomUI.Controls/Separator/SeparatorRail.cs`
- `src/AtomUI.Desktop.Controls/Separator/Separator.cs`
- `src/AtomUI.Desktop.Controls/Separator/Separator.SemanticParts.cs`
- `src/AtomUI.Desktop.Controls/Separator/SeparatorToken.cs`
- `src/AtomUI.Desktop.Controls/Separator/Themes/SeparatorTheme.axaml`

职责边界：

- 控件主文件保留 public/protected API、Avalonia 属性注册、事件和主要生命周期入口。
- Theme 文件负责静态视觉结构、template part、selector 和资源绑定。
- Token 文件只提供组件视觉变量，不保存实例状态。
- Gallery 文件只展示用法和示例，不作为运行时逻辑 owner。
- Semantic descriptor 由 `Separator` 的 `[SemanticPart]` 声明生成；模板只使用静态 `Classes.semantic-*="True"` marker。

## 3. 核心类职责

- `AbstractSeparator`：跨平台或共享基类，承载公共 API、状态归一和模板生命周期。
- `Separator`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `SeparatorRail`：连接线渲染控件，按 `Variant` 生成 dash style 并绘制单段水平或垂直线。
- `SeparatorToken`：控件 Token scope，负责从全局 token 派生控件语义变量。
- `VerticalSeparator`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。

核心协作规则：

- 控件实例是 public API 和运行时状态 owner。
- Template part 是视觉协作对象，生命周期必须受 `OnApplyTemplate` 或模板加载流程管理。
- 数据对象、选项对象、任务对象或节点对象只保存业务数据，不应反向持有不可释放的视觉对象。
- 弹层、窗口、计时器、异步 loader 和全局管理器必须有明确关闭、解绑或释放路径。

## 4. 状态与数据流

Separator 的状态流遵循下面路径：

```text
Public API / ItemsSource / Command / Event
  -> 控件实例状态
  -> internal state / effective state / pseudo-class
  -> template part property / AXAML selector
  -> renderer / popup / adorner / Gallery observable behavior
```

源码中的状态入口按以下语义维护：

- 内容与数据：`Title`、`TitleColor`、`TitlePosition`。
- 交互与状态：`IsPlain`。
- 视觉与布局：`LineColor`、`LineWidth`、`Orientation`、`OrientationMargin`、`SizeType`、`Variant`。

水平间距由 `SeparatorTheme.axaml` 单向映射：水平作用域先设置 Middle block margin 作为基础值，随后仅由
`Small`、`Middle`、`Large` selector 覆盖为对应 Token。`Custom` 不进入预设 selector，因此实例 `Margin` 或
owner-scoped Style 可以接管。该映射不依赖 `:has-title`，标题状态不会改变尺寸语义。

当 Separator 位于其他控件的 ControlTemplate 内且需要紧凑结构间距时，owner 把子控件设为 `Custom`，并在
owner ControlTheme 中通过模板作用域 selector 设置 `Margin`。不要只在模板节点写 `Margin` 并假定其优先级高于
Separator 自身主题 setter。

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

- `PART_Title`：稳定模板协作入口，承载标题文本，重命名前必须同步主题和实现。
- `PART_RailStart`：连接线起始段节点，承载标题左侧（或无标题/垂直时的整段）连接线，重命名前必须同步主题和实现。
- `PART_RailEnd`：连接线结束段节点，承载标题右侧连接线，无标题或垂直时归零，重命名前必须同步主题和实现。

`SeparatorTheme` 的模板根节点是一个 `Panel`，内含 `PART_RailStart`、`PART_Title`、`PART_RailEnd` 三个子节点。
`PART_RailStart` / `PART_RailEnd` 使用静态 `Classes.semantic-rail="True"` marker，`PART_Title` 使用静态
`Classes.semantic-content="True"` marker。连接线由 `SeparatorRail.Render` 在各自 rail 节点内绘制，布局由
`AbstractSeparator.ArrangeOverride` 归位：带标题水平时两段分别占标题两侧，无标题水平时 `PART_RailStart` 占满宽度、
`PART_RailEnd` 归零，垂直时 `PART_RailStart` 占满高度（上下各留 20% 内缩）、`PART_RailEnd` 归零。`Variant` 只切换
rail 绘制 `Pen` 的 dash style。因此 `root` 之外公开 `rail`（`Multiple`）与 `content`（`Single`）两个 Semantic
Part，与上游 Ant Design Divider 的 `rail` / `content` 语义对齐；连接线既可通过 `SeparatorRailStyle` 定制，也可
走 `LineColor` / `LineWidth` / `Variant` 公共 API。

`PART_Title` 始终存在于模板中，`Title` 为空或 `Orientation=Vertical` 时仅切换 `IsVisible`，不改变 Semantic
Part marker 身份或数量；rail 节点始终存在，归零尺寸不改变 Part 数量。

## 6. 交互与事件处理

Separator 的交互事件应从输入源收敛到控件级语义事件：

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
- 状态变化时避免创建不必要的视觉对象、订阅或动画对象。

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

维护 Separator 时不得破坏：

- Public API、默认值、事件顺序和 Gallery 可观察行为。
- Template part 名称、ControlTheme key、伪类和资源 key。
- 旧 template part、事件订阅、Popup/Flyout/Window host 和 collection view 的释放路径。
- Light/Dark、Browser/Desktop 和不同 SizeType 下的主题一致性。
- 控件文档、源码 public surface、Token 类型或生成数据与源码契约的一致性。

## 10. 测试与验证

推荐验证：

- 纯文档改动运行 `git diff --check` 并检查相对链接。
- 控件 API 或行为变更运行对应 `tests/AtomUI.Desktop.Controls.Tests` 或专用包测试。
- DataGrid 相关变更运行 `tests/AtomUI.Desktop.Controls.DataGrid.Tests`。
- Gallery 示例或源码片段变更运行 `tests/AtomUIGallery.Tests`。
- AOT、生成器或动态数据路径变更按 Gallery NativeAOT 发布流程验证。
