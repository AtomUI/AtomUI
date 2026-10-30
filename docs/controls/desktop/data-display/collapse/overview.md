# Collapse 桌面版架构设计

本文档定义 `Collapse` 桌面版的最新设计定位、公共契约、状态模型、视觉主题关系和兼容边界。通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，内部实现原理见 [Collapse 桌面版实现原理](implementation.md)，Semantic Part 契约见 [Collapse Semantic Part 契约](semantic-part.md)，Collapse Token 的专项设计见 [Collapse Token 设计](token.md)，设计和契约变化记录见 [Collapse Changelog](changelog.md)。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/DataDisplay/Collapse` |
| 控件状态 | Stable |

Collapse 是 AtomUI 桌面控件体系中的折叠面板控件，用于在多个可展开内容区之间展示和隐藏内容。

Collapse 不负责树控件、菜单或 Tabs 内容切换。这些职责应由业务层、组合控件或更专用的 AtomUI 控件承担。

主要源码入口：

- `src/AtomUI.Desktop.Controls/Collapse`

## 2. 设计语言

Collapse 的设计语言围绕控件职责、可观察状态和主题契约组织，而不是围绕模板节点组织。

| 维度 | 含义 | Collapse 中的表达 |
| --- | --- | --- |
| 产品语义 | 控件在界面中承担的稳定职责。 | Collapse 是 AtomUI 桌面控件体系中的折叠面板控件，用于在多个可展开内容区之间展示和隐藏内容。 |
| 内容承载 | 用户数据、展示内容、集合项或操作入口如何进入控件。 | `AddOnContent`、`AddOnContentTemplate`、`ContentPadding`、`ExpandIcon`、`ExpandIconPosition`、`HeaderPadding`、`IsShowExpandIcon`、`ItemContentPadding` 等 9 项。 |
| 状态反馈 | public API、内部状态和伪类如何形成用户可感知反馈。 | selection/checked/active、motion、visual option。 |
| 主题语义 | ControlTheme、SharedToken、控件 Token 和模板绑定如何表达视觉。 | Collapse Token + ControlTheme。 |

## 3. API 与契约模型

Collapse 的公共契约由 public/protected 类型成员、Avalonia 属性、事件、命令、template part、伪类、ControlTheme key 和资源 key 共同组成。维护时应先确认这些契约是否已经被源码、Gallery 示例或文档暴露。

核心 public surface 按语义分组维护：

| 契约组 | 代表成员 | 维护含义 |
| --- | --- | --- |
| 内容与数据 | `AddOnContent`、`AddOnContentTemplate`、`ContentPadding`、`ExpandIcon`、`ExpandIconPosition`、`HeaderPadding`、`IsShowExpandIcon`、`ItemContentPadding`、`ItemHeaderPadding` | 定义控件展示内容、输入数据、模板或业务对象入口。 |
| 选择与集合 | `IsSelected` | 维护选择、展开、过滤、分页、分组或集合状态。 |
| 交互与状态 | `IsAccordion`、`IsBorderless`、`IsGhostStyle`、`IsMotionEnabled` | 表达用户可观察状态、可用性、清除、加载或反馈语义。 |
| 视觉与布局 | `SizeType` | 影响尺寸、位置、颜色、形状、密度和模板视觉变量。 |
| 其他稳定入口 | `TriggerType` | 保留为 public surface，变更前需确认 Gallery 和用户 XAML 依赖。 |

当前没有抽取到控件专属 public 事件；交互通知主要来自继承事件、命令或 Gallery 可观察状态。

主要公开类型与枚举：

- 类型：`Collapse`、`CollapseItem`。
- 枚举：`CollapseExpandIconPosition`、`CollapseTriggerType`。

稳定 template part：

| Template Part | 类型 | 职责 |
| --- | --- | --- |
| `PART_AddOnContentPresenter` | `?` | 展示用户内容、文本、图标或模板化数据。 |
| `PART_ContentViewport` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |
| `PART_ContentPresenter` | `?` | 展示用户内容、文本、图标或模板化数据。 |
| `PART_ExpandButton` | `?` | 承载用户触发入口、导航或关闭动作。 |
| `PART_Frame` | `?` | 承载根视觉、边框、背景或尺寸基线。 |
| `PART_HeaderDecorator` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |
| `PART_HeaderPresenter` | `?` | 展示用户内容、文本、图标或模板化数据。 |
| `PART_ItemsPresenter` | `ItemsPresenter` | 展示用户内容、文本、图标或模板化数据。 |
| `PART_MainLayout` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |

当前未抽取到控件专属伪类；主题主要依赖 Avalonia 标准伪类、模板绑定和内部 StyledProperty。

### 3.5 Semantic Part 契约

`Collapse` 公开与上游 Collapse 稳定 Semantic DOM 对齐的五个 Semantic Part，完整契约见 [Collapse Semantic Part 契约](semantic-part.md)：

| Part | Selector | AtomUI 节点 | Cardinality | 定制方式 |
| --- | --- | --- | --- | --- |
| `root` | 控件本身 | `Collapse` owner（表面投影到 `PART_Frame`） | `Single` | owner 选择器 + 公开属性 |
| `header` | `.semantic-header` | 每个面板的 `PART_HeaderDecorator` | `Multiple` | `CollapseHeaderStyle` |
| `icon` | `.semantic-icon` | 每个面板的 `PART_ExpandButton` | `Multiple` | `CollapseIconStyle` |
| `title` | `.semantic-title` | 每个面板的 `PART_HeaderPresenter` | `Multiple` | `CollapseTitleStyle` |
| `body` | `.semantic-body` | 每个面板的 `PART_ContentFrame` | `Multiple` | `CollapseBodyStyle` |

`header`、`icon`、`title`、`body` 是运行时生成 Part：节点位于运行时 `CollapseItem` 容器的模板内，marker 静态声明于
`CollapseItemTheme.axaml`，路由从 owner 出发经一步 `>`（逻辑树）直达 `.semantic-scope-item` 容器，再以 `/template/` 进入
容器模板；`semantic-scope-items` / `semantic-scope-panel` 标识 items host 链。marker 不随展开、禁用、尺寸档、
视觉模式切换与容器回收增删。定制摘要：

- 状态型定制（展开/收起、禁用、ghost、borderless、TriggerType、ExpandIconPosition、SizeType）通过 owner 公开属性完成，
  不改变 marker 数量。
- 局部视觉定制通过生成的 Semantic Style 完成，`ContractType` 收缩到公开类型（`PixelAlignedBorder` / `IconButton` /
  `ContentPresenter`），internal 节点不作为公共依赖类型。
- 布局型 Setter（固定 `Height` / `Width` / Min/Max 等）不作为公共定制路径：面板高度由 content motion 的动效（layout
  transform 缩放 + 稳定态显式高度）与尺寸档 Padding/字体链驱动，见 [semantic-part.md §5](semantic-part.md#5-尺寸基线)。
- `CollapseItem` 容器、item shell 分隔线、`AddOnContent` 附加内容与动效 actor 不属于 Semantic Part，见
  [semantic-part.md §6](semantic-part.md#6-定制边界)。

## 4. 行为与状态模型

Collapse 的状态流按以下路径收敛：

```text
Public API / inherited command / item source / user input
  -> 控件实例状态
  -> effective state / pseudo-class / template property
  -> ControlTheme selector / presenter / renderer
  -> Gallery 可观察行为
```

`Collapse` 继续使用 Avalonia `SelectingItemsControl` 的 selection model 作为唯一展开状态 owner，`CollapseItem.IsSelected` 是该状态投影到容器后的公开绑定入口。控件不得维护 active-key 集合、当前展开项缓存或另一套展开状态。

状态维护规则：

- 普通模式使用 `Multiple | Toggle`：每个 item 可独立展开和收起。
- 手风琴模式使用 `Single | Toggle`：打开目标项时关闭旧项，点击当前项时允许全部收起。
- 普通模式切换到手风琴模式时，按视觉索引保留第一个已展开项，保证切换后的单一展开状态确定且稳定。
- Header、Icon、keyboard 和 pointer 输入最终进入同一个 selection 操作，不在输入处理器中直接维护展开状态。
- Disabled 或不可交互状态优先屏蔽 pointer、keyboard 和 motion，不改变 selection。
- 内容可见性、箭头方向和动效目标只从 `IsSelected` 派生；模板节点之间不得双向同步展开状态。
- 模板重套用、items reset/replace/clear 和模式切换后必须保持 selection model、容器与内容视觉一致。

普通模式与手风琴模式使用 Core 共用内容展开机制：
内容按正常尺寸排版，通过高度和透明度呈现收放，反转从当前帧接续。手风琴在 selection 提交时同步产生旧项收起和
新项展开目标，两项使用同一进度交换空间，互斥不依赖动画完成事件。关闭动效及模板生命周期边界直接投影当前状态，
释放仅限该机制拥有的动画和内部布局控制，保留自定义尺寸与变换。
共享设计来源：`docs/architecture/systems/control-infrastructure/content-expansion.md`。

## 5. 视觉与主题模型

Collapse 的视觉模型由控件模板、ControlTheme、SharedToken 和必要的控件 Token 共同构成。

| 主题文件 | 职责 |
| --- | --- |
| `CollapseItemTheme.axaml` | 定义集合项、容器项或局部单元的状态视觉。 |
| `CollapseTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |

Collapse 使用 `CollapseToken` 作为控件 Token scope。Token 只表达组件视觉语义，不承载 selection/checked/active、motion、visual option 运行时状态。

Collapse 的分隔线采用结构化所有权：

- `PART_Frame` 绘制外框、圆角并裁剪整体内容。
- 非末 `CollapseItem` 的 item shell 固定绘制底部分隔线。
- 默认 bordered 模式下，`PART_ContentFrame` 固定绘制内容顶部边线。
- Borderless 模式保留 item 间分隔线，但不绘制外框和内容顶部边线。
- Ghost 模式不绘制外框、item 分隔线和内容顶部边线。
- 分隔线厚度不得依赖 `IsSelected`、动效进行状态或动效完成时机。

主题维护规则：

- 不删除或重命名已经稳定的 ControlTheme key、template part、伪类和资源 key。
- 不把可由 AXAML 表达的模板状态迁移为 C# 动态创建视觉。
- 不把 hover、pressed、selected、expanded、loading、filter、popup open 等运行时状态写入 Token。
- Browser 或平台特化主题必须保持同一 API 的语义一致。

## 6. 控件家族或集成关系

Collapse 与同分类控件共享尺寸、状态、Token、Gallery 展示和验证规则。组合或派生控件应显式说明哪些 API 被继承、覆盖或不支持。

主要协作类型：

- `Collapse`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `CollapseItem`：集合项、节点或容器类型，承载单项状态和模板协作。
- `CollapseToken`：控件 Token scope，负责从全局 token 派生控件语义变量。

集成关系：

- 与 ThemeManager、SharedToken、ControlTheme、控件文档和 Gallery ShowCase 示例保持一致。
- 涉及 ItemsSource、Popup、Flyout、Window、Form 或 CompactSpace 的路径必须保持生命周期释放和数据状态同步。
- 源码目录中的共享基类和内部协作类型形成维护边界，不能只修改桌面包装类而忽略共享状态 owner。

## 7. 兼容性不变量

维护 Collapse 时必须保持以下不变量：

- 不擅自新增、删除、重命名或改变 public/protected API、Avalonia 属性、事件和默认值。
- 不破坏 template part、伪类、ControlTheme key、Token 名称和资源 key。
- 不改变 Gallery 已展示的 XAML 用法、默认外观、交互顺序和状态优先级。
- Template part 重新应用、集合替换、弹层关闭、窗口失活和控件 detach 时必须释放旧订阅和资源宿主。
- 不通过隐藏延迟、强制刷新或吞异常掩盖状态同步问题。
- 不引入运行时反射扫描作为 API、Token 或数据路径发现机制。
- 文档只描述当前稳定设计；历史变化记录在 `changelog.md`。
- Semantic Part 的五个区域（`root`、`header`、`icon`、`title`、`body`）、selector class、ContractType、cardinality 与
  marker 放置属于主题兼容契约；删除、重命名、收窄类型或让内置模板缺少 marker 都是破坏性变更。
- `header`/`icon`/`title`/`body` 的 marker 静态声明于 `CollapseItemTheme.axaml`，scope marker 在默认 ItemsPanel 与容器
  创建路径一次性建立；任何状态切换、容器回收、items 集合变化与模板重应用都不得增删 marker；默认主题不得消费
  `.semantic-*` selector。
- root 表面投影（`Background`/`BorderBrush`/`BorderThickness`/`CornerRadius`/`Padding` → `PART_Frame`）属于公共契约；
  运行时 marker 通过静态 AXAML class 与既有创建路径添加，不引入 VisualTree 搜索、反射或运行时 AXAML 解析，保持
  NativeAOT 友好。

## 8. 专项模型

### 8.1 选择与当前项模型

Collapse 的展开状态由 Avalonia selection model 单独拥有。普通模式采用可切换多选，手风琴模式采用可切换单选；`CollapseItem.IsSelected`、内容可见性和箭头方向都是 selection 的下游投影。模式切换、集合替换、清空和模板重套用不能创建第二个状态 owner。

### 8.2 动效模型

Collapse 的动效只处理 content 的布局展开、裁剪、透明度和最终可见性，不改变 selection 或边框语义。
其共享契约以完整内容高度为展开目标，以零为收起目标；内容 frame 的 padding 和顶边线一起参与裁剪，item shell
底线和标题不进入内容动画。等高手风琴内容保持总占位稳定，不等高内容在两个稳定高度之间连续变化。

初始加载、关闭动效、template reapply 和卸载直接按最新 `IsSelected` 收敛；播放中的反向请求先保存当前呈现值，
再取消旧请求并接续插值。公共机制不保存 active-key 或第二份展开集合。完整算法和首尾帧约束见
[内容展开与收起动效设计](../../../../architecture/systems/control-infrastructure/content-expansion.md)。

### 8.3 分隔线模型

外框、item 底部分隔线和内容顶部边线分别由固定视觉节点拥有。最后一项只去除 item 底线；内容收起时顶部边线随 content 一起被裁剪和隐藏。父控件不得订阅动效状态后重算 header/content 边框，也不得为边框厚度添加 transition。

### 8.4 视觉选项模型

Collapse 的视觉选项通过 public API 归一为 theme variables、伪类或模板绑定。Token 保存组件语义值，不能保存实例运行时状态或业务色值。

## 9. 文档导航与验证策略

关联文档：

- [Collapse 桌面版实现原理](implementation.md)
- [内容展开与收起动效设计](../../../../architecture/systems/control-infrastructure/content-expansion.md)
- [Collapse Semantic Part 契约](semantic-part.md)
- [Collapse Token 设计](token.md)
- [Collapse Changelog](changelog.md)

验证策略：

| 改动类型 | 验证要求 |
| --- | --- |
| 文档改动 | 运行 `git diff --check`，检查相对链接存在。 |
| Public API | 覆盖属性默认值、事件触发、命令和继承语义。 |
| 状态模型 | 覆盖 selection/checked/active、motion、visual option、disabled、hover、pressed、focus 以及控件特有状态。 |
| 内容动效设计 | 逐帧验证等高/不等高手风琴、首尾时钟边界、快速反转、嵌套尺寸变化、固定分隔线和原有滚动/命中约束。 |
| AXAML/Theme | 检查 template part、伪类、资源 key、Light/Dark 主题和 Browser 主题。 |
| Token | 检查 TokenKind、AXAML token resource、Token 类型、生成数据和 token.md和文档同步。 |
| Semantic Part | 检查 descriptor 数量/顺序/字段、marker 数量与类型、容器回收后的 marker 身份，见 [semantic-part.md §7](semantic-part.md#7-兼容性与验证)。 |
| Gallery | 走查对应 ShowCase 示例和源码片段入口。 |
