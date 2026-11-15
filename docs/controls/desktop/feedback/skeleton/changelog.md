# Skeleton Changelog

本文档记录 Skeleton 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-11-14

- Performance
  - `MotionGate` subscribes to the owner's visual ancestors only while the owner itself is visible. Hidden owners (for example the placeholder Skeleton in every Gallery ShowCase item) used to subscribe to every ancestor; the gate's result is unchanged because a hidden owner can never run.

## 2026-11-11

- Fix
  - 微光只绘制在块的边框内缘以内（按 `BorderThickness` 收缩矩形与圆角）；通过 Semantic Part Style 设置非零边框时，描边在动画期间保持可见。

## 2026-11-10

- API
  - Skeleton 家族的平台中立部分下沉到共享层 `AtomUI.Controls`，Desktop 类改为继承它们，类名不变：`AbstractSkeleton`（`IsActive`、`MotionDuration`、`MotionEasingCurve`、`IsMotionEnabled`、`StartActiveAnimation`/`StopActiveAnimation`、附加属性 `IsShimmerBlock` 与共享微光宿主）、`AbstractSkeletonLine`（`LineWidth`、`IsRound`、模板部件 `PART_RootLayout`）、`AbstractSkeletonParagraph`（`Rows`、`LastLineWidth`、`LineWidths`、`IsRound`、模板部件 `PART_LineLayout`，行元素由 `CreateLine`、`ApplyLineWidth`、`ApplyLineState` 提供）、`AbstractCompositeSkeleton`（`IsLoading`、`IsShowAvatar`、`IsShowTitle`、`IsShowParagraph`、`IsRound`、`TitleWidth`、`ParagraphRows`、`ParagraphLastLineWidth`、`ParagraphLineWidths`、`AvatarShape`、`AvatarSize`、`Content` 及内容模板与对齐、internal `IsContentVisible`、`PlaceholderTemplate`、`Placeholder`）。后三者位于 `AtomUI.Controls.Commons`。
  - `AbstractSkeleton` 的命名空间由 `AtomUI.Desktop.Controls` 改为 `AtomUI.Controls`；XAML 同属 `https://atomui.net`，AXAML 源码不受影响，只引用 `AtomUI.Desktop.Controls` 的 C# 代码需要补 `using AtomUI.Controls`。迁移见 [6.2.14 迁移指南](../../../../releases/6.2.14-api-changes.zh-CN.md)。
  - `SkeletonLine : AbstractSkeletonLine`、`SkeletonTitle : SkeletonLine`、`SkeletonParagraph : AbstractSkeletonParagraph`、`Skeleton : AbstractCompositeSkeleton`。`SkeletonParagraph` 的行元素仍为 `SkeletonLine`，宽度、激活、圆角、背景的同步规则不变。`AvatarSizeType` 与 `AvatarSizeTypeProperty` 留在 `Skeleton`。
  - 新增 internal 的 Token 抽象层 `AbstractSkeletonToken`（`AtomUI.Controls.DesignTokens`）：`GradientFromColor`、`GradientToColor`、`LoadingMotionDuration`、`BlockRadius`、`TitleHeight`、`ParagraphLineHeight` 及其取值计算；Desktop `SkeletonToken` 继承它并保留其余成员。
  - 新增 `AbstractSkeleton.IsMotionEnabled`（`MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner`，默认 `true`）。微光只在骨架附着、自身与祖先全部可见、应用处于活动状态、`IsMotionEnabled`、`IsActive` 为真且 `MotionDuration` 大于 0 时运行，组合骨架另需 `IsLoading`。
  - 新增附加属性 `AbstractSkeleton.IsShimmerBlock`（宿主类型 `Border`）及其 `Get`/`Set` 访问器，标记微光块。它们是模板基础设施，标注 `EditorBrowsable(Never)`；声明为 public 是因为 XAML 编译器只经 public 成员解析附加属性，其他程序集中的骨架模板（Desktop、Mobile）需要在 AXAML 中标记块。
  - 删除 internal 的 `LoadingBackgroundStart`、`LoadingBackgroundMiddle`、`LoadingBackgroundEnd` 属性与模板部件 `PART_ActiveAnimationLayer`；新增 internal 的 `ShimmerFromColor`、`ShimmerToColor`（画刷，颜色 Token 以纯色画刷发布）、`IsShimmerRunning`（direct 属性）、`IsShimmerHost`、`ShimmerBlockCount`，以及 `AbstractCompositeSkeleton` 的 internal `PlaceholderTemplate`、`Placeholder` 与模板部件 `PART_PlaceholderHost`、`PART_ShimmerLayer`。
  - protected `StartActiveAnimation`、`StopActiveAnimation` 签名不变，作用改为请求微光宿主重新评估是否运行。
  - `MotionEasingCurve` 在渲染线程求值。`SplineEasing` 带可变状态，每个场景发送的是它的私有副本；Avalonia 其他内置缓动无状态，原样传递；自定义 `Easing` 也原样传递，必须是纯函数（`Ease` 不写状态，赋值后不再修改）。
  - `Skeleton` 的 Semantic Part `header`、`section`、`avatar`、`title`、`paragraph` 声明为 `RuntimeCreated`，选择器路径 `/template/ .semantic-*`；名称、类型、数量与生成的 Style 不变。
- Theme
  - 微光改为“一组骨架一道亮带”：一组嵌套骨架中最外层的骨架作为宿主，在微光层（`PART_ShimmerLayer` → `PART_ContentLayer` → 自身）上挂一个合成层自定义视觉，渲染线程按每个块的圆角矩形裁剪绘制同一道亮带；运行时以动画优先级把块的背景设为 `null`，停止时释放，块回到主题底色。Avalonia 的 `ImmediateDrawingContext` 不提供几何裁剪，场景因此携带块的 `RoundedRect` 数组，逐块 `PushClip`。
  - 组合骨架只对其占位子树内的视觉充当宿主；放在 `Content` 中的骨架按组合骨架不存在来找宿主，保留自己的亮带。
  - `AbstractSkeletonTheme` 及所有元素模板（Avatar、Button、Input、Image、Node）删除 `PART_ActiveAnimationLayer`，`PART_ContentLayer` 标记为微光块且始终可见；`AbstractSkeletonTheme` 设置 `IsMotionEnabled = {atom:SharedTokenResource EnableMotion}`、`ShimmerFromColor`/`ShimmerToColor` 取 `GradientFromColor`/`GradientToColor`，不再引用 `LoadingBackground*`。
  - `SkeletonTheme` 的占位 `Grid` 移入 `PlaceholderTemplate` setter（命名 `PART_ShimmerLayer`），语义类留在原元素上；控件模板只剩根 `Border` → `Panel#PART_PlaceholderHost` → `ContentPresenter`，占位在 `IsLoading` 为真时构建并插入索引 0，结束、重新应用模板或脱离视觉树时丢弃。
  - Desktop 主题的颜色、尺寸与 Token 取值不变；`SkeletonToken` 生成的 `SkeletonTokenKind` 成员集合与改动前一致（15 项），生成的 Token 资源键文件与改动前逐字节相同。
- Fix
  - `SkeletonParagraph.LastLineWidth` 的默认值由 1%（`Dimension` 的百分比取值为 0–100）改为 100%：单独使用的 `SkeletonParagraph` 不设置宽度时，最后一行此前只有 1% 宽，现为整行宽度。
  - 骨架或其祖先隐藏、应用进入后台（`IActivatableLifetime` 报告 Background 停用）或关闭动效（`EnableMotion`）时微光停止；此前样式动画只在不可见时暂停时钟，动效开关对骨架无效。
  - `IsActive` 为真但 `MotionDuration` 为 0 的元素此前显示透明的动画层，现在显示主题底色。
  - 首次激活时宿主等模板（组合骨架还要等占位）就绪后才启动微光，合成层视觉只创建一次，不先挂在骨架自身再移走；场景中没有可绘制的块时处理器不再请求下一帧，出现块后恢复。
- Desktop 影响
  - 源码兼容：类型简名不变，既有公共成员都保留，签名、访问级别与静态性不变；新增的公共成员是 `IsMotionEnabled` 与标注 `EditorBrowsable(Never)` 的 `IsShimmerBlock` 附加属性及其访问器。默认值除 `SkeletonParagraph.LastLineWidth`（1% → 100%）外不变（`ParagraphRows` 2、`TitleWidth` 50%、`ParagraphLastLineWidth` 61%、`SkeletonTitle.LineWidth` 50%、`SkeletonParagraph.Rows` 2、`IsLoading` `false`）；`Skeleton.IsLoadingProperty` 等静态属性字段通过继承仍可按原名访问。
  - 行为修复：单独使用的 `SkeletonParagraph` 的最后一行此前为 1% 宽，现为整行宽度；组合的 `Skeleton` 不受影响，它通过 `ParagraphLastLineWidth`（默认 61%）为段落提供最后一行宽度。
  - 二进制不兼容之处只有 `AbstractSkeleton` 的命名空间；这些属性字段的声明类型改为共享基类，直接按字段名读取的反射代码需要从基类取。
  - 公共 API 快照：仓库外的反射工具分别读取改动前（工作区快照 `93990a8ec085`，以 `git archive` 导出到仓库外构建）与改动后的 `AtomUI.Desktop.Controls.dll`、`AtomUI.Controls.dll`，对全部公开类型输出“类型简名、基类简名链、接口、展开继承后的 public/protected 成员签名”，按类型简名合并两个程序集后逐类型比对。`Skeleton`、`SkeletonAvatar`、`SkeletonButton`、`SkeletonElement`、`SkeletonImage`、`SkeletonInput`、`SkeletonLine`、`SkeletonNode`、`SkeletonParagraph`、`SkeletonTitle`、`AbstractSkeleton`（另含 `SkeletonToken`、`SkeletonButtonShape`）的差异只有：`AbstractSkeleton` 的命名空间；基类链插入（`SkeletonLine`、`SkeletonTitle` 增加 `AbstractSkeletonLine`，`SkeletonParagraph` 增加 `AbstractSkeletonParagraph`，`Skeleton` 增加 `AbstractCompositeSkeleton`）；上述 11 个骨架类型各新增 `IsMotionEnabled`（含 `IsMotionEnabledProperty`）与 `IsShimmerBlock`（含 `IsShimmerBlockProperty`、`GetIsShimmerBlock`、`SetIsShimmerBlock`）；`SkeletonParagraph` 新增来自 `AbstractSkeletonParagraph` 的 protected `CreateLine`、`ApplyLineWidth`、`ApplyLineState`。没有成员被删除，也没有成员改变签名、访问级别或静态性。
  - 部件名常量 `AbstractSkeletonLine.RootLayoutPart` 与 `AbstractSkeletonParagraph.LineLayoutPart` 为 internal（共享层与 Mobile 经 `InternalsVisibleTo` 使用），与其余部件名常量一致，`SkeletonLine`、`SkeletonTitle`、`SkeletonParagraph` 不增加公开常量。
  - 组合骨架中比整行窄的块（标题 50%、末行 61%、头像）的亮带改为与整行对齐；满宽行与独立使用的单个元素轨迹不变（峰值从 −1.52W 移到 +1.48W，周期 1.4 s）。
  - 静止帧逐像素不变（V6 回归）：同一个无头渲染驱动（Skia，仓库外）分别引用改动前与改动后的树，浅色、深色各渲染 Skeleton 的 31 个场景（组合骨架的头像、标题、段落各组合，圆形、方形与自定义尺寸的头像，自定义标题与末行宽度、逐行宽度，非加载内容、嵌套内容，独立的段落、行、标题，Button、Avatar、Input、Image、Node 的尺寸与形状变体及混排），`IsActive=false` 的静止帧共 62 张逐像素比对，60 张完全一致；差异只有独立 `SkeletonParagraph`（默认参数）的最后一行（宽度由 1% 变为 100%，浅色 8,372 像素、深色 8,446 像素，仅在该行所在的 y 范围内），这是上文列出的修复。同一驱动对改动前的树连续渲染两次，结果逐像素相同，渲染是确定的。改动后 `IsActive=true` 且 `IsMotionEnabled=false` 的帧与改动前未激活的帧比对，结果相同（同样只有上述末行差异）。
  - 运行开始的一帧：把 `MotionDuration` 设为极大值使亮带停在 t = 0 的位置（亮带在所有块左侧），与静止帧比对，共 34 个场景有差异：浅色最大通道差 1 级，全部落在块的抗锯齿边缘；深色最大通道差 2 级，其中 Avatar、Button（含它们的混排）的块内部有整块 1 级差（例如 47 → 48、45 → 46），其余元素与 Card 加载指示器只在抗锯齿边缘有 1–2 级差。亮带扫过时块内按设计变化，块外只有抗锯齿边缘不超过 2 级的通道差。
  - 并发动画数：带头像、标题、两行段落的组合骨架从 4 个样式动画变为 1 个微光源；不带头像的组合骨架（标题 + 3 行）从 4 变为 1；3 行的独立段落从 3 变为 1。
  - 视觉元素数：`IsLoading=false` 的 `Skeleton`（内容为一个 `TextBlock`）的视觉后代从 10 个变为 4 个，开启 `IsShowAvatar` 时同为 10 → 4；`SkeletonLine` 从 3 个变为 2 个。
  - 占位部件只在加载时存在：`IsLoading=false` 时 `PART_Avatar`、`PART_Title`、`PART_Paragraph` 及 `header`/`section` 等 Semantic Part 不在视觉树中。
  - 依赖 `PART_ActiveAnimationLayer` 或 `LoadingBackground*` 的自定义模板与样式需要改用 `PART_ContentLayer` 加 `AbstractSkeleton.IsShimmerBlock="True"`。
  - 待清理：`SkeletonToken` 的 `LoadingBackgroundStart`、`LoadingBackgroundMiddle`、`LoadingBackgroundEnd` 已不被主题引用，只为保持生成的 Token 资源键而保留；删除它们属于公共 API 变化，留待后续版本处理。
- Tests
  - 新增 `SkeletonSinkTests`（共享层）：行工厂按 `Rows`、`LineWidths`、`LastLineWidth` 生成各行宽度并在 `Rows` 变化时重建；宽度与状态变化只更新已有行；`Rows` 小于 1 被拒绝；共享默认值（含 `LastLineWidth` 为 100%）；`IsContentVisible` 跟随 `IsLoading`；内容的逻辑子元素维护。
  - 新增 `SkeletonSinkDesktopTests`（Desktop）：类型继承关系、Desktop 默认值不变、`SkeletonTokenKind` 成员名集合不变、单独使用的 `SkeletonParagraph` 最后一行为整行宽度。
  - 既有 `SkeletonBehaviorTests`、`SkeletonSemanticPartTests` 补 `using AtomUI.Controls`；其中依赖样式动画与 `PART_ActiveAnimationLayer` 的用例改为断言共享微光的同等语义，见下文。
  - 新增 `SkeletonShimmerTests`（共享层）：亮带轨迹（线性缓动下峰值 −1.52W → +1.48W，默认曲线 `SplineEasing(0.25,0.1,0.25,1)`）、亮带画刷几何、采样与逐块绘制 1,000 帧零托管分配、处理器跟随场景/启动/停止消息、独立段落自为宿主、门控真值表（动效开关、附着、可见、活动来源、时长）、停止后块背景恢复且无动画优先级值、不加载时不创建占位、组合骨架单一微光源、内容区中的骨架保留自己的微光（`Skeleton_in_content_keeps_its_own_shimmer`）、快速切换 `IsLoading` 保持单一微光源、宽度变化只重建一次、颜色与时序变化重发场景、重新应用模板保持单一处理器、场景中的 `SplineEasing` 是私有副本、首次激活只创建一个处理器、没有块时不请求帧。
  - 新增 `SkeletonShimmerDesktopTests`：Desktop 组合骨架单一微光源与块数、不加载时的视觉元素数、占位模板作用域（模板父级、`/template/` 样式、语义类、模板绑定）、主题动效参数与颜色、各元素模板唯一且始终可见的微光块、内容区中的骨架保留自己的微光。
  - `SkeletonBehaviorTests` 中检查样式动画的三个用例改为断言新宿主的同等语义：`Loading_Background_Uses_AntDesign_Shimmer_Geometry` 断言亮带画刷取 Token 的两种颜色、宽 1.52W，且起止峰值与保留的 `LoadingBackgroundStart/End` 画刷描述的位置一致；`Active_Animation_Is_Applied_To_Template_Layer_And_Released_When_Inactive` 断言激活时内容层背景被动画优先级的 `null` 覆盖、合成层视觉挂在内容层上，取消激活后背景恢复；`Active_Animation_Pauses_When_Skeleton_Is_Effectively_Invisible` 断言祖先隐藏与关闭动效时微光停止、重新可见后恢复。
  - `SkeletonBehaviorTests.Image_And_Node_Templates_Do_Not_Cover_Active_Background_With_Content_Background` 改为断言 Image、Node 模板只有标记为微光块的 `PART_ContentLayer`，图标与 `ContentPresenter` 是其后的兄弟元素，运行时微光挂在内容层上；`SkeletonSemanticPartTests.Active_Content_Alternatives_Preserve_Semantic_Marker_Identity` 改为断言只有一个 `semantic-content` 层，激活前后同一实例且始终可见，并且它就是微光层。
  - 性能工具 `--verify-skeleton-states` 改为经反射读取 `IsShimmerHost`、`IsShimmerRunning`、`ShimmerBlockCount`、`ShimmerVisual`、`Placeholder` 验证运行状态。
  - V6 回归：`dotnet test tests/AtomUI.Controls.Tests` 222 项、`tests/AtomUI.Desktop.Controls.Tests` 4,179 项、`tests/AtomUIGallery.Tests` 412 项（含 `SkeletonShowCasePageTests`、`SpaceShowCasePageTests`、`CardShowCasePageTests`，Skeleton、Space、Card 三个展示页均可打开）全部通过，`controlgallery/AtomUIGallery.Desktop` 构建通过。

## 2026-10-18

- Fixed
  - Drive the active shimmer with an owner-scoped style animation on the template `PART_ActiveAnimationLayer`, removing the `Animation.RunAsync` + `IterationCount.Infinite` path that raised `TaskScheduler.UnobservedTaskException` during GC.

## 2026-10-03

- Fixed
  - SkeletonLine 按最终内容盒解析像素/百分比 LineWidth，排除外部 Margin；LineWidth 变化触发重新测量。

## 2026-08-25

- Fixed
  - Pause active shimmer when Skeleton is effectively invisible, including visibility inherited from hidden Visual ancestors.
- Docs
  - Document the effective-visibility lifecycle and hidden-page performance invariant.

## 2026-08-15

- Semantic Parts
  - Add Ant Design-aligned Semantic Part descriptors for `Skeleton` and its public element owners.
  - Add static template markers and generated owner-scoped style contracts without runtime discovery.
  - Document the dual static content layers used by normal and active Skeleton states.
- Gallery
  - Add the Semantic Parts Preview and Ant Design-aligned custom styling example using AtomUI `v6.1.3` version metadata.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Skeleton`.
  - Align generated output paths with `controls/skeleton/index-cn.md` and `controls/skeleton/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete Skeleton desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish Skeleton desktop architecture documentation under `docs/controls/desktop/feedback/skeleton/overview.md`.
  - Add Skeleton implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add Skeleton control-level changelog.
  - Add Skeleton Token documentation covering SkeletonToken.
