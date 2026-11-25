# Skeleton 桌面版实现原理

本文档描述 Skeleton 桌面版的内部实现范围、源码职责、状态流、生命周期、资源边界和维护规则。公共设计与 API 契约见 [Skeleton 桌面版架构设计](overview.md)，变化记录见 [Skeleton Changelog](changelog.md)。涉及控件 Token 的实现应同时阅读 [Skeleton Token 设计](token.md)。

## 1. 实现定位

本文档覆盖 Skeleton 的控件实现、主题接入、状态同步和 Gallery 可见维护边界。具体属性注册、默认值、绘制细节和 AXAML selector 仍应直接阅读源码；本文只记录维护者必须理解的稳定结构和不变量。

## 2. 源码文件结构

主要源码文件：

共享层（`src/AtomUI.Controls/Skeleton`，平台中立，Desktop 与 Mobile 共用）：

- `AbstractSkeleton.cs`：命名空间 `AtomUI.Controls`。`IsActive`、`IsMotionEnabled`、`MotionDuration`、`MotionEasingCurve`，以及共享微光：微光宿主判定、块登记、`MotionGate` 门控、合成层视觉的挂载与场景构建。
- `AbstractSkeletonLine.cs`、`AbstractSkeletonParagraph.cs`、`AbstractCompositeSkeleton.cs`：命名空间 `AtomUI.Controls.Commons`。行宽与圆角、段落行工厂、组合骨架的属性与按需占位。
- `SkeletonShimmerVisualHandler.cs`、`SkeletonShimmerMessages.cs`：渲染线程上的微光绘制与 UI 线程发给它的消息。
- `AbstractSkeletonToken.cs`：命名空间 `AtomUI.Controls.DesignTokens`，渐变色、时长、圆角与行高的共享 Token 抽象。

Desktop（`src/AtomUI.Desktop.Controls/Skeleton`）：

- `Skeleton.cs`、`SkeletonAvatar.cs`、`SkeletonButton.cs`、`SkeletonElement.cs`、`SkeletonImage.cs`、`SkeletonInput.cs`、`SkeletonLine.cs`、`SkeletonNode.cs`、`SkeletonParagraph.cs`、`SkeletonTitle.cs`
- `SkeletonToken.cs`
- `Skeleton.SemanticParts.cs`、`SkeletonAvatar.SemanticParts.cs`、`SkeletonButton.SemanticParts.cs`、`SkeletonInput.SemanticParts.cs`、`SkeletonImage.SemanticParts.cs`、`SkeletonNode.SemanticParts.cs`
- `Themes/AbstractSkeletonTheme.{axaml,cs}`、`Themes/SkeletonElementTheme.{axaml,cs}`、`Themes/SkeletonImageTheme.{axaml,cs}`、`Themes/SkeletonAvatarTheme.axaml`、`Themes/SkeletonButtonTheme.axaml`、`Themes/SkeletonInputTheme.axaml`、`Themes/SkeletonLineTheme.axaml`、`Themes/SkeletonNodeTheme.axaml`、`Themes/SkeletonParagraphTheme.axaml`、`Themes/SkeletonTheme.axaml`

职责边界：

- 控件主文件保留 public/protected API、Avalonia 属性注册、事件和主要生命周期入口。
- Theme 文件负责静态视觉结构、template part、selector 和资源绑定。
- Token 文件只提供组件视觉变量，不保存实例状态。
- Gallery 文件只展示用法和示例，不作为运行时逻辑 owner。
- Semantic descriptor 由各 public owner 的 `[SemanticPart]` 声明生成；模板只使用静态 `Classes.semantic-*="True"` marker。
- `SkeletonAvatar`、`SkeletonButton` 和 `SkeletonInput` 的 typed theme 保留现有 BasedOn 样式，并提供等价显式叶子模板，
  使生成器能够验证静态 marker，而不引入运行时发现路径。

## 3. 核心类职责

- `AbstractSkeleton`：所有骨架的共享基类。持有激活与动效属性；一组嵌套骨架中只有最外层的一个（微光宿主）运行微光，其余骨架只提供块。
- `AbstractSkeletonLine`：单行骨架，按最终宽度解析 `LineWidth`，模板根部件为 `PART_RootLayout`。
- `AbstractSkeletonParagraph`：段落骨架，按 `Rows` 通过 `CreateLine` 生成行，并以 `ApplyLineWidth`、`ApplyLineState` 同步宽度与状态。
- `AbstractCompositeSkeleton`：组合骨架（头像、标题、段落），`IsLoading` 为真时按 `PlaceholderTemplate` 构建占位，结束后丢弃。
- `SkeletonShimmerVisualHandler`：`CompositionCustomVisualHandler`，在渲染线程按块的圆角矩形裁剪并绘制一道亮带。
- `Skeleton`：Desktop 组合骨架，只增加 `AvatarSizeType`。
- `SkeletonLine`、`SkeletonTitle`、`SkeletonParagraph`：Desktop 行、标题与段落；段落的行是 `SkeletonLine`。
- `SkeletonElement` 及 `SkeletonButton`、`SkeletonInput`：带 `SizeType`、`IsBlock` 的元素骨架。
- `SkeletonAvatar`、`SkeletonImage`、`SkeletonNode`：头像、图片与自定义节点元素骨架。
- `AbstractSkeletonTheme`、`SkeletonElementTheme`、`SkeletonImageTheme`：可继承的 ControlTheme 类型入口。
- `SkeletonToken`：Desktop 控件 Token scope，继承 `AbstractSkeletonToken`。

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

| Template Part | 所在模板 | 职责 |
| --- | --- | --- |
| `PART_RootLayout` | 元素骨架、`Skeleton` | 元素骨架的根 `Panel`（行骨架按它排列实际行宽）；`Skeleton` 的根 `Border`，投影 Background、边框、圆角与 Padding。 |
| `PART_ContentLayer` | 元素骨架 | 块的底色层（`Border`），标记为微光块（`AbstractSkeleton.IsShimmerBlock="True"`），始终可见；独立使用时也是微光层。 |
| `PART_LineLayout` | `SkeletonParagraph` | 承载运行时生成的行。 |
| `PART_PlaceholderHost` | `Skeleton` | 承载 `ContentPresenter` 的 `Panel`；加载时占位插入到索引 0。 |
| `PART_ShimmerLayer` | `Skeleton` 的 `PlaceholderTemplate` | 占位根 `Grid`，组合骨架的微光层。 |
| `PART_Avatar`、`PART_Content`、`PART_Title`、`PART_Paragraph` | `Skeleton` 的 `PlaceholderTemplate` | 头像、标题与段落所在的列与子骨架。 |

`SkeletonTheme` 的控件模板只有根 `Border#PART_RootLayout` → `Panel#PART_PlaceholderHost` → `ContentPresenter`。加载占位写在
`PlaceholderTemplate` setter 中：两列 `Grid#PART_ShimmerLayer`，第一列的 `DockPanel.semantic-header` 只承载 `PART_Avatar`，第二列的
`StackPanel.semantic-section` 承载 `PART_Title` 和 `PART_Paragraph` 并填充剩余宽度，确保 Semantic Preview 高亮 `header` 时只覆盖头像
cell。占位只在 `IsLoading` 为真时存在：

- `IsLoading` 变为真且模板已应用、控件已附着时，`PlaceholderTemplate.Build(this)` 生成占位，`TemplatedControlUtils.ApplyTemplatedParent`
  把模板父级设为 `Skeleton`，再插入 `PART_PlaceholderHost` 的索引 0。模板绑定、`^ /template/ atom|SkeletonAvatar` 一类样式与
  `Classes.semantic-*` 标记因此照常生效；占位中的部件名在占位自己的名称作用域中查找。
- `IsLoading` 变为假、重新应用模板或脱离视觉树时，先停止微光，再移除并丢弃占位；重新附着且仍在加载时重新构建。
- 不加载时 `Skeleton` 的视觉子树只有根 `Border`、`PART_PlaceholderHost`、`ContentPresenter` 与内容本身。

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
- Semantic Part marker 不参与状态切换；元素骨架只有一个始终可见的 content layer，激活时只把它的背景交给微光绘制。
- `SkeletonParagraph` 作为唯一状态 owner，在创建运行时 `SkeletonLine` 与 Background 变化时直接同步行背景；
  不引入字符串绑定或额外订阅，行节点在 Paragraph 模板重建时随 Children 清理。
- `SkeletonTheme` 覆盖主控件根 Background 的默认值为透明，避免根 surface 与子占位层重复绘制；显式 Background 仍由根 Border 投影。
- `SkeletonLine` 按最终内容盒解析 `LineWidth`：像素值作为上限，百分比相对于最终宽度计算；内部占位层使用最终高度，
  不计入外部 Margin。`LineWidth` 变化使测量失效，确保运行时修改能够更新布局。

### 7.1 共享微光

一组嵌套骨架共享一道微光，只有微光宿主运行它：

- **块**：标记了 `AbstractSkeleton.IsShimmerBlock="True"` 的 `Border`。块附着到视觉树时登记到它的宿主，脱离或取消标记时注销。
  Desktop 的块是各元素模板的 `PART_ContentLayer`，段落的每一行、标题、头像各贡献一个块。
- **宿主**：块（或骨架）视觉祖先中最外层的合格 `AbstractSkeleton`，没有时骨架自己就是宿主。组合骨架只对其占位子树内的视觉合格；
  放在组合骨架 `Content` 中的骨架或块按组合骨架不存在来找宿主（再往外的合格骨架，否则自身），因此内容区里的骨架保留自己的微光。
- **微光层**：宿主按顺序查找模板部件 `PART_ShimmerLayer`（组合骨架在占位的名称作用域中查找）→ `PART_ContentLayer` → 骨架自身。
  宿主通过 `ElementComposition.SetElementChildVisual` 在微光层上挂一个 `CompositionCustomVisual`。挂在 `PART_ContentLayer` 上时，
  `SkeletonImage` 的图标、`SkeletonNode` 的 `ContentPresenter` 作为后续兄弟元素，仍画在微光之上。
- **运行条件**：`IsShimmerHost && IsActive && IsMotionEnabled && MotionDuration > 0 && MotionGate.ShouldRun`，组合骨架另需 `IsLoading`。
  `MotionGate` 只在宿主上创建，它在附着、祖先全部可见、活动来源处于活动状态时放行；`IsMotionEnabled` 写入门控，Desktop 主题把它绑定到
  `EnableMotion`。应用进入后台（`IActivatableLifetime` 报告 Background 停用）或注入的活动来源变为非活动时，微光停止。
- **启动**：宿主以 `BindingPriority.Animation` 把每个块的 `Border.Background` 设为 `null` 并保留返回的 `IDisposable`，使底色不在渐变下再画一次；
  收集块在微光层坐标中的圆角矩形，发送 `SkeletonShimmerScene` 后发送 `SkeletonShimmerStart`，并开始订阅宿主的 `LayoutUpdated`。
- **停止** 发送 `SkeletonShimmerStop`，释放全部背景抑制与配色观察，块回到主题底色。宿主退订 `LayoutUpdated`。运行中新登记的块立即被抑制，
  注销的块立即释放。
- **场景重建**：只在运行期间、宿主每次布局完成后比较块矩形与微光层尺寸；与上一次场景不同时才构建新的块数组并发送场景，宽度变化
  （旋转、字号变化）因此只重建一次，之后的帧不触发布局。`ShimmerFromColor`、`ShimmerToColor`、`MotionDuration`、`MotionEasingCurve`
  变化时，运行中的宿主重发场景而不重建块。
- **绘制**：`SkeletonShimmerScene(RoundedRect[] Blocks, double Width, Color From, Color To, TimeSpan Duration, Easing Easing)`
  中 `Blocks` 是微光层坐标中的块（圆角取块的 `CornerRadius`，相邻圆角之和超过边长时按比例缩小；不可见或零尺寸的块不在其中），
  每个场景一个新数组，发送后不再修改。Avalonia 的 `ImmediateDrawingContext` 不提供几何裁剪，处理器对每个块
  `PushClip(RoundedRect)`，再以 `Matrix.CreateTranslation(left, 0)` 平移后用缓存的不可变渐变填充该块。渐变为底色 @0 → 峰值 @0.316 → 底色 @1，
  宽 1.52W（绝对单位，两侧以底色延伸），W 为微光层宽度。亮带左缘 `left = −2W + 3W · easing(t)`，`t = (秒 mod D) / D`，
  峰值因此从 −1.52W 移到 +1.48W，默认曲线为 `SplineEasing(0.25, 0.1, 0.25, 1)`，Desktop 周期为 Token `LoadingMotionDuration`（1.4 s）。
  组合骨架中比微光层窄的块（标题、末行、头像）共用这条整行轨迹。停止后，或场景中没有可绘制的块时，处理器不再请求下一帧；
  带块的场景到达后恢复。
- **缓动的线程边界**：缓动在渲染线程求值。`SplineEasing` 的关键样条会缓存参数并惰性重建，控制点也可在 UI 线程修改，因此每个场景
  发送的是 `new SplineEasing(X1, Y1, X2, Y2)` 私有副本；Avalonia 其他内置缓动无状态，原样传递；自定义 `Easing` 原样传递，必须是纯函数。
- **颜色** 桌面主题指定 `GradientFromColor` 和 `GradientToColor`。微光宿主在启动时成组读取，配色变化只重发一次场景。场景取画刷颜色并把画刷不透明度并入 alpha。底色与块背景相同，静止帧与未运行时逐像素一致。
- **启动时机**：宿主等模板应用（组合骨架在应加载时还要等占位构建）后才启动，首次激活只创建一个处理器；附着、`OnLoaded`、模板应用与占位构建都会重新评估。
- **生命周期**：模板重新应用或占位重建改变微光层时，宿主先停止并把视觉从旧层取下，再在新层上创建，任一时刻每个宿主最多一个处理器；
  宿主脱离视觉树时停止、取下视觉并释放门控。`StartActiveAnimation`/`StopActiveAnimation` 保留为 protected，作用是请求宿主重新评估。

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
- 一组骨架只有一个微光源：一个合成层视觉、一个处理器，逐帧只在渲染线程计算值类型并压入结构体状态，零托管分配，不触发布局。
- 不加载的组合骨架不创建占位子树；占位、块登记与背景抑制都随 `IsLoading` 建立和释放。
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
- 控件 API 或行为变更按[单元测试与验收规范](../../../../engineering/development/test-value-and-lifecycle.md)完成临时验收。
- Skeleton 临时验收覆盖宿主判定、块登记、门控真值表、背景恢复、按需占位、场景重建、单处理器、亮带轨迹和逐帧零分配。
  Desktop 主题接入覆盖单一微光源、占位模板作用域、动效参数和元素块标记。
- 运行状态回归运行 `tools/performances/AtomUI.Performance` 的 `--verify-skeleton-states`。
- DataGrid 数据内核变更运行 `tests/AtomUI.Desktop.Controls.DataGrid.Tests`。受影响的 DataGrid 控件行为按场景临时验收。
- Gallery 示例或源码片段变更临时验收对应页面。
- AOT、生成器或动态数据路径变更按 Gallery NativeAOT 发布流程验证。
