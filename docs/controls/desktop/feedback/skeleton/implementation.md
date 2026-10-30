# Skeleton 桌面版实现原理

本文档描述 Skeleton 桌面版的内部实现范围、源码职责、状态流、生命周期、资源边界和维护规则。公共设计与 API 契约见 [Skeleton 桌面版架构设计](overview.md)，变化记录见 [Skeleton Changelog](changelog.md)。涉及控件 Token 的实现应同时阅读 [Skeleton Token 设计](token.md)。

## 1. 实现定位

本文档覆盖 Skeleton 的控件实现、主题接入、状态同步和 Gallery 可见维护边界。具体属性注册、默认值、绘制细节和 AXAML selector 仍应直接阅读源码；本文只记录维护者必须理解的稳定结构和不变量。

## 2. 源码文件结构

主要源码文件：

- `src/AtomUI.Desktop.Controls/Skeleton/AbstractSkeleton.cs`
- `src/AtomUI.Desktop.Controls/Skeleton/Skeleton.cs`
- `src/AtomUI.Desktop.Controls/Skeleton/SkeletonAvatar.cs`
- `src/AtomUI.Desktop.Controls/Skeleton/SkeletonButton.cs`
- `src/AtomUI.Desktop.Controls/Skeleton/SkeletonElement.cs`
- `src/AtomUI.Desktop.Controls/Skeleton/SkeletonImage.cs`
- `src/AtomUI.Desktop.Controls/Skeleton/SkeletonInput.cs`
- `src/AtomUI.Desktop.Controls/Skeleton/SkeletonLine.cs`
- `src/AtomUI.Desktop.Controls/Skeleton/SkeletonNode.cs`
- `src/AtomUI.Desktop.Controls/Skeleton/SkeletonParagraph.cs`
- `src/AtomUI.Desktop.Controls/Skeleton/SkeletonTitle.cs`
- `src/AtomUI.Desktop.Controls/Skeleton/SkeletonToken.cs`
- `src/AtomUI.Desktop.Controls/Skeleton/Skeleton.SemanticParts.cs`
- `src/AtomUI.Desktop.Controls/Skeleton/SkeletonAvatar.SemanticParts.cs`
- `src/AtomUI.Desktop.Controls/Skeleton/SkeletonButton.SemanticParts.cs`
- `src/AtomUI.Desktop.Controls/Skeleton/SkeletonInput.SemanticParts.cs`
- `src/AtomUI.Desktop.Controls/Skeleton/SkeletonImage.SemanticParts.cs`
- `src/AtomUI.Desktop.Controls/Skeleton/SkeletonNode.SemanticParts.cs`
- `src/AtomUI.Desktop.Controls/Skeleton/Themes/AbstractSkeletonTheme.axaml`
- `src/AtomUI.Desktop.Controls/Skeleton/Themes/AbstractSkeletonTheme.cs`
- `src/AtomUI.Desktop.Controls/Skeleton/Themes/SkeletonAvatarTheme.axaml`
- `src/AtomUI.Desktop.Controls/Skeleton/Themes/SkeletonButtonTheme.axaml`
- `src/AtomUI.Desktop.Controls/Skeleton/Themes/SkeletonElementTheme.axaml`
- `src/AtomUI.Desktop.Controls/Skeleton/Themes/SkeletonElementTheme.cs`
- `src/AtomUI.Desktop.Controls/Skeleton/Themes/SkeletonImageTheme.axaml`
- `src/AtomUI.Desktop.Controls/Skeleton/Themes/SkeletonImageTheme.cs`
- `src/AtomUI.Desktop.Controls/Skeleton/Themes/SkeletonInputTheme.axaml`
- `src/AtomUI.Desktop.Controls/Skeleton/Themes/SkeletonLineTheme.axaml`
- `src/AtomUI.Desktop.Controls/Skeleton/Themes/SkeletonNodeTheme.axaml`
- `src/AtomUI.Desktop.Controls/Skeleton/Themes/SkeletonParagraphTheme.axaml`
- `src/AtomUI.Desktop.Controls/Skeleton/Themes/SkeletonTheme.axaml`

职责边界：

- 控件主文件保留 public/protected API、Avalonia 属性注册、事件和主要生命周期入口。
- Theme 文件负责静态视觉结构、template part、selector 和资源绑定。
- Token 文件只提供组件视觉变量，不保存实例状态。
- Gallery 文件只展示用法和示例，不作为运行时逻辑 owner。
- Semantic descriptor 由各 public owner 的 `[SemanticPart]` 声明生成；模板只使用静态 `Classes.semantic-*="True"` marker。
- `SkeletonAvatar`、`SkeletonButton` 和 `SkeletonInput` 的 typed theme 保留现有 BasedOn 样式，并提供等价显式叶子模板，
  使生成器能够验证静态 marker，而不引入运行时发现路径。

## 3. 核心类职责

- `AbstractSkeleton`：跨平台或共享基类，承载公共 API、状态归一和模板生命周期。
- `AbstractSkeletonTheme`：ControlTheme 类型入口，连接主题资源和控件类型。
- `Skeleton`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `SkeletonAvatar`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `SkeletonButton`：动作触发类型，负责点击、导航或局部操作状态。
- `SkeletonElement`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `SkeletonElementTheme`：ControlTheme 类型入口，连接主题资源和控件类型。
- `SkeletonImage`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `SkeletonImageTheme`：ControlTheme 类型入口，连接主题资源和控件类型。
- `SkeletonInput`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `SkeletonLine`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `SkeletonNode`：集合项、节点或容器类型，承载单项状态和模板协作。
- `SkeletonParagraph`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `SkeletonTitle`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `SkeletonToken`：控件 Token scope，负责从全局 token 派生控件语义变量。

核心协作规则：

- 控件实例是 public API 和运行时状态 owner。
- Template part 是视觉协作对象，生命周期必须受 `OnApplyTemplate` 或模板加载流程管理。
- 数据对象、选项对象、任务对象或节点对象只保存业务数据，不应反向持有不可释放的视觉对象。
- 弹层、窗口、计时器、异步 loader 和全局管理器必须有明确关闭、解绑或释放路径。

## 4. 状态与数据流

Skeleton 的状态流遵循下面路径：

```text
Public API / ItemsSource / Command / Event
  -> 控件实例状态
  -> internal state / effective state / pseudo-class
  -> template part property / AXAML selector
  -> renderer / popup / adorner / Gallery observable behavior
```

源码中的状态入口按以下语义维护：

- 内容与数据：`AvatarShape`、`AvatarSize`、`AvatarSizeType`、`Content`、`ContentTemplate`、`HorizontalContentAlignment`、`IsShowAvatar`、`IsShowTitle`、`ParagraphRows`、`Rows` 等 12 项。
- 选择与集合：`IsActive`。
- 交互与状态：`IsBlock`、`IsLoading`、`IsRound`、`IsShowParagraph`。
- 视觉与布局：`LastLineWidth`、`LineWidth`、`ParagraphLastLineWidth`、`Shape`、`Size`、`SizeType`。
- 动效与异步：`MotionDuration`、`MotionEasingCurve`。

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

- `PART_ActiveAnimationLayer`：稳定模板协作入口，重命名前必须同步主题和实现。
- `PART_Avatar`：稳定模板协作入口，重命名前必须同步主题和实现。
- `PART_Content`：稳定模板协作入口，重命名前必须同步主题和实现。
- `PART_ContentLayer`：稳定模板协作入口，重命名前必须同步主题和实现。
- `PART_LineLayout`：稳定模板协作入口，重命名前必须同步主题和实现。
- `PART_Paragraph`：稳定模板协作入口，重命名前必须同步主题和实现。
- `PART_RootLayout`：承载根视觉、边框、背景或尺寸基线。
- `PART_Title`：稳定模板协作入口，重命名前必须同步主题和实现。

`SkeletonTheme` 的加载占位模板使用两列 `Grid`：第一列的 `DockPanel.semantic-header` 只承载
`PART_Avatar`，第二列的 `StackPanel.semantic-section` 承载 `PART_Title` 和 `PART_Paragraph`，并填充剩余宽度。
两者是同一布局容器的 sibling，确保 Semantic Preview 高亮 `header` 时只覆盖头像 cell；`ContentPresenter` 仍作为加载完成后的
独立 sibling 显示用户内容。

## 6. 交互与事件处理

Skeleton 的交互事件应从输入源收敛到控件级语义事件：

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
- Semantic Part marker 不参与状态切换；active 与普通 content layer 都静态存在，`IsVisible` 只决定当前可观察 target。
- `SkeletonParagraph` 作为唯一状态 owner，在创建运行时 `SkeletonLine` 与 Background 变化时直接同步行背景；
  不引入字符串绑定或额外订阅，行节点在 Paragraph 模板重建时随 Children 清理。
- `SkeletonTheme` 覆盖主控件根 Background 的默认值为透明，避免根 surface 与子占位层重复绘制；显式 Background 仍由根 Border 投影。
- `SkeletonLine` 按最终内容盒解析 `LineWidth`：像素值作为上限，百分比相对于最终宽度计算；内部占位层使用最终高度，
  不计入外部 Margin。`LineWidth` 变化使测量失效，确保运行时修改能够更新布局。

Active shimmer 由挂在模板叶子节点 `PART_ActiveAnimationLayer` 上的样式动画（`Style.Animations`）驱动，图层 `Background` 由动画直接写入。不得改回在控件上手工调用 `Animation.RunAsync`：Avalonia 的 `RunAsync` 禁止 `IterationCount.Infinite`（返回 faulted Task，未观察时会在 GC 阶段触发 `TaskScheduler.UnobservedTaskException`），样式动画是 Avalonia 公开 API 中唯一原生支持 Infinite 且不创建 Task 的单实例持续动画路径。动画必须显式采用 `PlaybackBehavior.OnlyIfVisible`，使 Skeleton 自身或任一 Visual 祖先不可见时暂停时钟；重新可见后由 Avalonia 恢复当前动画，不为每次可见性变化重建动画对象。动画 target 必须是模板叶子节点而非控件自身，避免样式作用域级联到嵌套的 Skeleton 子控件。

实现文档不逐行解释私有方法。若某个私有算法成为稳定维护入口，应在本节补充算法不变量，而不是把代码复述为说明书。

## 8. 资源、性能与 AOT 边界

资源和 AOT 约束：

- 不通过运行时反射扫描 public API、Token 或 Gallery 示例数据。
- 不通过 VisualTree 扫描维护 Semantic Part；Gallery Preview 使用生成 descriptor 和 owner-scoped marker 解析。
- 不把可静态声明的模板结构迁移到 C# 动态创建。
- 异步加载、上传、弹层和窗口生命周期必须能取消或释放。
- 缓存对象必须与控件、窗口、弹层或数据 owner 生命周期一致。
- Source generator 生成文件不手工编辑；需要修改时改输入源或 generator。

性能边界：

- 控件应优先复用 Avalonia 原生虚拟化、模板绑定和资源系统。
- 避免为每次状态变化创建不必要的视觉对象、订阅或动画对象。
- 隐藏页面中保留的 active Skeleton 不得继续推进 shimmer 或产生 UI 线程 animation tick。
- 大集合控件必须保证 container recycle 后不会泄漏旧 item 状态。

## 9. 维护不变量

维护 Skeleton 时不得破坏：

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
