# Breadcrumb 桌面版实现原理

本文档描述 Breadcrumb 桌面版的内部实现范围、源码职责、状态流、生命周期、资源边界和维护规则。公共设计与 API 契约见 [Breadcrumb 桌面版架构设计](overview.md)，Semantic Part 运行时契约见 [Breadcrumb Semantic Part 契约](semantic-part.md)，变化记录见 [Breadcrumb Changelog](changelog.md)。涉及控件 Token 的实现应同时阅读 [Breadcrumb Token 设计](token.md)。

## 1. 实现定位

本文档覆盖 Breadcrumb 的控件实现、主题接入、状态同步和 Gallery 可见维护边界。具体属性注册、默认值、绘制细节和 AXAML selector 仍应直接阅读源码；本文只记录维护者必须理解的稳定结构和不变量。

## 2. 源码文件结构

主要源码文件：

- `src/AtomUI.Desktop.Controls/Breadcrumb/Breadcrumb.cs`
- `src/AtomUI.Desktop.Controls/Breadcrumb/Breadcrumb.SemanticParts.cs`
- `src/AtomUI.Desktop.Controls/Breadcrumb/BreadcrumbItem.cs`
- `src/AtomUI.Desktop.Controls/Breadcrumb/BreadcrumbItemData.cs`
- `src/AtomUI.Desktop.Controls/Breadcrumb/BreadcrumbItemsPanel.cs`
- `src/AtomUI.Desktop.Controls/Breadcrumb/BreadcrumbNavigateEventArgs.cs`
- `src/AtomUI.Desktop.Controls/Breadcrumb/BreadcrumbSeparatorManager.cs`
- `src/AtomUI.Desktop.Controls/Breadcrumb/BreadcrumbPseudoClass.cs`
- `src/AtomUI.Desktop.Controls/Breadcrumb/BreadcrumbToken.cs`
- `src/AtomUI.Desktop.Controls/Breadcrumb/IBreadcrumbItemData.cs`
- `src/AtomUI.Desktop.Controls/Breadcrumb/Themes/BreadcrumbItemTheme.axaml`
- `src/AtomUI.Desktop.Controls/Breadcrumb/Themes/BreadcrumbTheme.axaml`

职责边界：

- 控件主文件保留 public/protected API、Avalonia 属性注册、事件和主要生命周期入口。
- 语义声明文件只承载 `Breadcrumb` 的 runtime semantic contract，不写模板节点或主题样式。
- Theme 文件负责静态视觉结构、template part、selector 和资源绑定。
- Token 文件只提供组件视觉变量，不保存实例状态。
- Gallery 文件只展示用法和示例，不作为运行时逻辑 owner。

## 3. 核心类职责

- `Breadcrumb`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `BreadcrumbItem`：集合项、节点或容器类型，承载单项状态和模板协作。
- `BreadcrumbItemData`：数据、状态或行为协作类型，维护集合同步和事件路径。
- `BreadcrumbToken`：根 owner 的八项 Own scope，以 V2 静态默认函数从有效 Global 派生颜色和内距；没有 Own→Own 依赖，明确下游配置不被默认求值覆盖。
- `BreadcrumbSeparatorManager`：内部协作类型，独占兄弟分隔符的创建、绑定、布局同步与销毁生命周期，`Breadcrumb` 只在条目状态更新与模板重套用时转发入口。

核心协作规则：

- 控件实例是 public API 和运行时状态 owner。
- Template part 是视觉协作对象，生命周期必须受 `OnApplyTemplate` 或模板加载流程管理。
- 数据对象、选项对象、任务对象或节点对象只保存业务数据，不应反向持有不可释放的视觉对象。
- 弹层、窗口、计时器、异步 loader 和全局管理器必须有明确关闭、解绑或释放路径。

## 4. 状态与数据流

Breadcrumb 的状态流遵循下面路径：

```text
Public API / ItemsSource / Command / Event
  -> 控件实例状态
  -> internal state / effective state / pseudo-class
  -> template part property / AXAML selector
  -> renderer / popup / adorner / Gallery observable behavior
```

源码中的状态入口按以下语义维护：

- 内容与数据：`Icon`、`SeparatorTemplate`。
- 交互与状态：`IsMotionEnabled`。
- 视觉与布局：`Background`、`BorderBrush`、`BorderThickness`、`CornerRadius`、`Padding`。
- 其他稳定入口：`NavigateContext`、`NavigateUri`、`Separator`。

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
- 运行时 semantic marker：`item` 标记在 container 创建与 prepare 时应用，`separator` 标记在 `Breadcrumb` 创建兄弟分隔
  `ContentPresenter` 时应用，container recycle 后不泄漏旧标记；只添加控件自己持有的 marker，不触碰应用侧自行添加的 class。
- 兄弟分隔符生命周期：`BreadcrumbSeparatorManager` 依据条目数维护 N-1 个分隔符（`Breadcrumb` 在 `UpdateItemStates`
  末尾与集合变化时合并为一个可取消的 Dispatcher 更新）；每个分隔符是 `Breadcrumb` 的逻辑子级（经
  `AttachSeparatorLogicalChild`/`DetachSeparatorLogicalChild` 挂载）、`BreadcrumbItemsPanel` 的视觉子级，
  `Content`/`ContentTemplate` 绑定到前一条目容器的 `Separator`/`SeparatorTemplate`，默认前景色与间距经
  `TokenResourceBinder` 绑定 `SeparatorColor`/`SeparatorMargin`；模板重套用、条目清除或容器回收时先释放绑定，
  再移除视觉与逻辑挂载。相同分隔符集合不重新挂载视觉节点，不重复触发布局失效；新增/删除项只修改差异。
  owner 视觉卸载时取消待调度更新、解除集合订阅并释放所有分隔符，重新挂载时按当前条目重建。
- `BreadcrumbItemsPanel` 使用已经包含 Margin 的 `DesiredSize` 测量与分配布局槽；具体 Margin 与垂直对齐由
  Avalonia `ArrangeCore` 处理，面板不再重复加算偏移或间距。
- DynamicResource、TokenResourceBinder 或 C# binding 必须有明确 owner 和释放点。
- Browser 和 Desktop 宿主下的主题加载顺序不得影响 public API 语义。

稳定 template part 接入点：

- 当前没有显式 template part；维护时仍需检查 ControlTheme key、资源 key 和继承模板契约。

## 6. 交互与事件处理

Breadcrumb 的交互事件应从输入源收敛到控件级语义事件：

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
- Root frame 布局与绘制：`Padding + BorderThickness` 构成 frame inset，`MeasureOverride` 用 inset 收缩测量约束并外扩 desired size，`ArrangeOverride` 用 inset 收缩排布区域，`Render` 通过 `BorderRenderHelper` 绘制背景、边框与圆角。
- Runtime semantic marker：`item` 标记由 `CreateContainerForItemOverride` 与 `PrepareContainerForItemOverride` 应用，
  `separator` 标记由 `Breadcrumb` 的兄弟分隔符创建路径应用，模板重套用或 container recycle 时重建，不依赖内置主题中的
  静态 `.semantic-*` 标记。
- 兄弟分隔符交错布局：`BreadcrumbItemsPanel` 按「容器 0、分隔符 0、容器 1、分隔符 1、…、容器 N-1」的顺序水平测量与
  排列；容器保留在 panel 的 `Children` 中由条目生成器独占管理，分隔符只作为 panel 的视觉子级参与渲染，不污染生成器
  基于面板位置的容器索引。

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

维护 Breadcrumb 时不得破坏：

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

## 状态动效初始化

`BreadcrumbItem` 使用构造阶段建立的共享初始化会话，完整初始外观及加载完成后恢复过渡；模板变更和分离时复位。
共享目标与生命周期契约见 [状态动效初始化架构](../../../../architecture/systems/control-infrastructure/state-motion-initialization.md)。
