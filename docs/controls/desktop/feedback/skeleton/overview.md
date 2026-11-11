# Skeleton 桌面版架构设计

本文档定义 `Skeleton` 桌面版的最新设计定位、公共契约、状态模型、视觉主题关系和兼容边界。通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，内部实现原理见 [Skeleton 桌面版实现原理](implementation.md)，Skeleton Token 的专项设计见 [Skeleton Token 设计](token.md)，设计和契约变化记录见 [Skeleton Changelog](changelog.md)。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/Feedback/Skeleton` |
| 控件状态 | Stable |

Skeleton 是 AtomUI 桌面控件体系中的骨架屏控件，用于在内容加载前展示结构占位。

Skeleton 不负责 Spin 加载器、真实数据列表或动画演示控件。这些职责应由业务层、组合控件或更专用的 AtomUI 控件承担。

主要源码入口：

- `src/AtomUI.Controls/Skeleton`：共享抽象（`AbstractSkeleton`、`AbstractSkeletonLine`、`AbstractSkeletonParagraph`、`AbstractCompositeSkeleton`）、共享微光与 Token 抽象。
- `src/AtomUI.Desktop.Controls/Skeleton`：Desktop 控件、主题与 `SkeletonToken`。

## 2. 设计语言

Skeleton 的设计语言围绕控件职责、可观察状态和主题契约组织，而不是围绕模板节点组织。

| 维度 | 含义 | Skeleton 中的表达 |
| --- | --- | --- |
| 产品语义 | 控件在界面中承担的稳定职责。 | Skeleton 是 AtomUI 桌面控件体系中的骨架屏控件，用于在内容加载前展示结构占位。 |
| 内容承载 | 用户数据、展示内容、集合项或操作入口如何进入控件。 | `AvatarShape`、`AvatarSize`、`AvatarSizeType`、`Content`、`ContentTemplate`、`HorizontalContentAlignment`、`IsShowAvatar`、`IsShowTitle` 等 12 项。 |
| 状态反馈 | public API、内部状态和伪类如何形成用户可感知反馈。 | selection/checked/active、loading/async、motion、visual option。 |
| 主题语义 | ControlTheme、SharedToken、控件 Token 和模板绑定如何表达视觉。 | Skeleton Token + ControlTheme。 |

## 3. API 与契约模型

Skeleton 的公共契约由 public/protected 类型成员、Avalonia 属性、事件、命令、template part、伪类、ControlTheme key 和资源 key 共同组成。维护时应先确认这些契约是否已经被源码、Gallery 示例或文档暴露。

核心 public surface 按语义分组维护：

| 契约组 | 代表成员 | 维护含义 |
| --- | --- | --- |
| 内容与数据 | `AvatarShape`、`AvatarSize`、`AvatarSizeType`、`Content`、`ContentTemplate`、`HorizontalContentAlignment`、`IsShowAvatar`、`IsShowTitle`、`ParagraphRows`、`Rows` 等 12 项 | 定义控件展示内容、输入数据、模板或业务对象入口。 |
| 选择与集合 | `IsActive` | 维护选择、展开、过滤、分页、分组或集合状态。 |
| 交互与状态 | `IsBlock`、`IsLoading`、`IsRound`、`IsShowParagraph` | 表达用户可观察状态、可用性、清除、加载或反馈语义。 |
| 视觉与布局 | `LastLineWidth`、`LineWidth`、`ParagraphLastLineWidth`、`Shape`、`Size`、`SizeType` | 影响尺寸、位置、颜色、形状、密度和模板视觉变量。 |
| 动效与异步 | `IsMotionEnabled`、`MotionDuration`、`MotionEasingCurve` | 微光开关（主题绑定 `EnableMotion`）、周期与曲线。 |

当前没有抽取到控件专属 public 事件；交互通知主要来自继承事件、命令或 Gallery 可观察状态。

主要公开类型与枚举：

- 共享类型：`AbstractSkeleton`（`AtomUI.Controls`）、`AbstractSkeletonLine`、`AbstractSkeletonParagraph`、`AbstractCompositeSkeleton`（`AtomUI.Controls.Commons`）。
- Desktop 类型：`Skeleton`、`SkeletonAvatar`、`SkeletonButton`、`SkeletonElement`、`SkeletonImage`、`SkeletonInput`、`SkeletonLine`、`SkeletonNode`、`SkeletonParagraph`、`SkeletonTitle`。
- 枚举：`SkeletonButtonShape`。

稳定 template part：

| Template Part | 类型 | 职责 |
| --- | --- | --- |
| `PART_Avatar` | `SkeletonAvatar` | 占位模板中的头像。 |
| `PART_Content` | `StackPanel` | 占位模板中承载标题与段落的列。 |
| `PART_ContentLayer` | `Border` | 元素骨架的底色层与微光块，始终可见。 |
| `PART_LineLayout` | `Panel` | 段落行的容器。 |
| `PART_Paragraph` | `SkeletonParagraph` | 占位模板中的段落。 |
| `PART_PlaceholderHost` | `Panel` | `Skeleton` 中承载内容与按需占位的容器。 |
| `PART_RootLayout` | `Panel` / `Border` | 承载根视觉、边框、背景或尺寸基线。 |
| `PART_ShimmerLayer` | `Grid` | `Skeleton` 占位根，组合骨架的微光层。 |
| `PART_Title` | `SkeletonTitle` | 占位模板中的标题。 |

`Skeleton` 的占位（头像、标题、段落）由 internal `PlaceholderTemplate` 在 `IsLoading` 为真时按需构建，模板父级是 `Skeleton`；
不加载时占位不存在，`PART_Avatar`、`PART_Title`、`PART_Paragraph` 等部件也不存在。

当前未抽取到控件专属伪类；主题主要依赖 Avalonia 标准伪类、模板绑定和内部 StyledProperty。

## 4. 行为与状态模型

Skeleton 的状态流按以下路径收敛：

```text
Public API / inherited command / item source / user input
  -> 控件实例状态
  -> effective state / pseudo-class / template property
  -> ControlTheme selector / presenter / renderer
  -> Gallery 可观察行为
```

状态维护规则：

- Disabled 或不可交互状态优先屏蔽 pointer、keyboard、motion 和提交类反馈。
- selection/checked/active、loading/async、motion、visual option 状态由控件实例或明确的数据 owner 推导，不能在 template part 之间双向竞争。
- 模板重套用时必须把 public API 对应状态回放到新的 part、伪类和主题变量。
- 集合、弹层、异步、动效或窗口相关状态必须能处理 reset、close、cancel、detach 和 owner 释放。

## 5. 视觉与主题模型

Skeleton 的视觉模型由控件模板、ControlTheme、SharedToken 和必要的控件 Token 共同构成。

| 主题文件 | 职责 |
| --- | --- |
| `AbstractSkeletonTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `SkeletonAvatarTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `SkeletonButtonTheme.axaml` | 定义局部操作入口、按钮或 handle 的状态视觉。 |
| `SkeletonElementTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `SkeletonImageTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `SkeletonInputTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `SkeletonLineTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `SkeletonNodeTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `SkeletonParagraphTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `SkeletonTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |

Skeleton 使用 `SkeletonToken` 作为控件 Token scope。Token 只表达组件视觉语义，不承载 selection/checked/active、loading/async、motion、visual option 运行时状态。

主题维护规则：

- 不删除或重命名已经稳定的 ControlTheme key、template part、伪类和资源 key。
- 不把可由 AXAML 表达的模板状态迁移为 C# 动态创建视觉。
- 不把 hover、pressed、selected、expanded、loading、filter、popup open 等运行时状态写入 Token。
- Browser 或平台特化主题必须保持同一 API 的语义一致。

## 6. 控件家族或集成关系

Skeleton 与同分类控件共享尺寸、状态、Token、Gallery 展示和验证规则。组合或派生控件应显式说明哪些 API 被继承、覆盖或不支持。

主要协作类型：

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

集成关系：

- 与 ThemeManager、SharedToken、ControlTheme、控件文档和 Gallery ShowCase 示例保持一致。
- 涉及 ItemsSource、Popup、Flyout、Window、Form 或 CompactSpace 的路径必须保持生命周期释放和数据状态同步。
- 源码目录中的共享基类和内部协作类型形成维护边界，不能只修改桌面包装类而忽略共享状态 owner。

## 7. 兼容性不变量

维护 Skeleton 时必须保持以下不变量：

- 不擅自新增、删除、重命名或改变 public/protected API、Avalonia 属性、事件和默认值。
- 不破坏 template part、伪类、ControlTheme key、Token 名称和资源 key。
- 不改变 Gallery 已展示的 XAML 用法、默认外观、交互顺序和状态优先级。
- Template part 重新应用、集合替换、弹层关闭、窗口失活和控件 detach 时必须释放旧订阅和资源宿主。
- 不通过隐藏延迟、强制刷新或吞异常掩盖状态同步问题。
- 不引入运行时反射扫描作为 API、Token 或数据路径发现机制。
- 文档只描述当前稳定设计；历史变化记录在 `changelog.md`。

## 8. 专项模型

### 8.1 选择与当前项模型

Skeleton 的当前项状态必须由单一 owner 推导。public 选择属性、集合项容器和伪类之间只能做单向同步，集合替换、清空和模板重套用时必须回放当前状态。

### 8.2 动效模型

激活的骨架以一道从左向右扫过的亮带表达加载。一组嵌套骨架共享一道亮带：最外层的骨架（微光宿主）在合成层上统一绘制，组合骨架中
比整行窄的块（标题、末行、头像）与整行对齐；放在组合骨架 `Content` 中的骨架保留自己的亮带。亮带峰值每个周期从 −1.52W 移到 +1.48W
（W 为微光层宽度），周期取 `MotionDuration`（Token `LoadingMotionDuration`，1.4 s），曲线取 `MotionEasingCurve`，未设置时为
`SplineEasing(0.25, 0.1, 0.25, 1)`。

亮带只在骨架附着、自身与祖先全部可见、应用处于活动状态、`IsMotionEnabled` 为真（主题绑定 `EnableMotion`）、`IsActive` 为真、
`MotionDuration` 大于 0 时运行，组合骨架另需 `IsLoading` 为真；条件不满足时块停在主题底色上，与未激活时的静止帧一致。实现见
[Skeleton 桌面版实现原理 · 共享微光](implementation.md#71-共享微光)。

### 8.3 视觉选项模型

Skeleton 的视觉选项通过 public API 归一为 theme variables、伪类或模板绑定。Token 保存组件语义值，不能保存实例运行时状态或业务色值。

## 9. 文档导航与验证策略

关联文档：

- [Skeleton 桌面版实现原理](implementation.md)
- [Skeleton Token 设计](token.md)
- [Skeleton Semantic Part 契约](semantic-part.md)
- [Skeleton Changelog](changelog.md)

`SkeletonAvatar`、`SkeletonButton`、`SkeletonInput`、`SkeletonImage` 和 `SkeletonNode` 的 `root/content` descriptor 与
主控件分开维护，完整契约见 [Skeleton Semantic Part 契约](semantic-part.md)。

验证策略：

| 改动类型 | 验证要求 |
| --- | --- |
| 文档改动 | 运行 `git diff --check`，检查相对链接存在。 |
| Public API | 覆盖属性默认值、事件触发、命令和继承语义。 |
| 状态模型 | 覆盖 selection/checked/active、loading/async、motion、visual option、disabled、hover、pressed、focus 以及控件特有状态。 |
| AXAML/Theme | 检查 template part、伪类、资源 key、Light/Dark 主题和 Browser 主题。 |
| Token | 检查 TokenKind、AXAML token resource、Token 类型、生成数据和 token.md和文档同步。 |
| Gallery | 走查对应 ShowCase 示例和源码片段入口。 |
