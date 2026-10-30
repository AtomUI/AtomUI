# Card 桌面版实现原理

本文档描述 Card 桌面版的内容类型识别、Actions 同步、Grid 内容容器生命周期、Tabs 内容桥接、Semantic Part 节点映射、主题状态同步和维护边界。公共设计与 API 契约见 [Card 桌面版架构设计](overview.md)，完整 Semantic Part 公共契约见 [Card Semantic Part 契约](semantic-part.md)，Token 语义见 [Card Token 设计](token.md)，变化记录见 [Card Changelog](changelog.md)。

## 1. 实现定位

Card 的实现重点是把 Header/Extra/Cover/Content/Actions 组合成一个稳定面板，并根据直接内容类型选择不同 body 视觉。Card 自身不维护业务数据模型，也不管理 tab 选择、grid item 业务状态或 action 命令执行。

本文档只描述 Card 相关实现结构，不重新说明 `HeaderedContentControl`、`ItemsControl`、`TabControl`、`Skeleton`、Avalonia Grid 或 Token 系统的通用机制。

## 2. 源码文件结构

主要源码：

- `src/AtomUI.Desktop.Controls/Card/Card.cs`：根 Card 公共 API、Actions 集合、内容类型识别、Header/Content 边框和圆角派生状态、Size/Motion 内容同步。
- `src/AtomUI.Desktop.Controls/Card/CardActionPanel.cs`：内部操作区，承载 Actions，维护 UniformGrid children，并渲染分隔线。
- `src/AtomUI.Desktop.Controls/Card/CardActionButton.cs`：Card 操作区推荐按钮类型，提供主题 key 边界。
- `src/AtomUI.Desktop.Controls/Card/CardMetaContent.cs`：头像、标题和描述内容的元信息承载控件。
- `src/AtomUI.Desktop.Controls/Card/CardGridContent.cs`：Card grid 内容的 ItemsControl，负责 Grid panel 行列定义同步和容器准备/清理。
- `src/AtomUI.Desktop.Controls/Card/CardGridItem.cs`：Grid item 容器，负责 content、row/column/span 和 hover/size/motion 状态。
- `src/AtomUI.Desktop.Controls/Card/CardTabsContent.cs`：Card tabs 内容桥接控件，负责把 Items 集合复制到内部 TabControl。
- `src/AtomUI.Desktop.Controls/Card/CardPseudoClass.cs`：Card 稳定伪类常量。
- `src/AtomUI.Desktop.Controls/Card/CardToken.cs`：Card 控件 Token。
- `src/AtomUI.Desktop.Controls/Card/Themes/*.axaml`：根主题、操作区、操作按钮、Meta、Grid、Tabs 等主题资源。

## 3. 核心类职责

`Card` 是状态协调器。它继承 `HeaderedContentControl`，负责公共属性、Headerless 伪类、Actions 同步、ContentType 派生、CardToken scope 注册和初始动效屏蔽。

`CardActionPanel` 是内部 action layout 和 render owner。它持有内部 `Actions` 集合，模板接入后将集合写入 `UniformGrid`，并在 `Render` 中绘制操作区顶部边线和纵向分隔线。

`CardGridContent` 是 grid 内容的 ItemsControl。它生成或复用 `CardGridItem`，把 `ColumnDefinitions` / `RowDefinitions` 同步到 ItemsPresenter 的 Grid panel，并为 `PrepareCardGridItem` 提供 container 生命周期 disposable。

`CardGridItem` 是 grid item 容器。它把 `Row`、`Column`、`RowSpan`、`ColumnSpan` 的变化写入 Avalonia Grid attached properties。

`CardTabsContent` 是 Card 与 TabControl 的桥接层。它承载 XAML `Items`，也暴露 `TabItemsSource` 和 `TabItemTemplate`，模板中真正的选择和内容显示由 `TabControl` 负责。

`CardMetaContent` 和 `CardActionButton` 是主题语义类型。它们逻辑很薄，但类型本身是用户和主题可依赖的契约。

Card 的尺寸/状态基线必须先于布局型 Semantic Setter 确定。`Large`、`Middle`、`Small` 的 Header/Body Token、Extra 内容、
Headerless、Loading、Meta/Grid/Tabs 和 Actions 共同构成完整布局规格；`.semantic-header` 与 `.semantic-body` 只能覆盖
选定规格之上的增量属性，不能把不同尺寸档位的 Padding、字体或 MinHeight 混合使用。

## 4. 状态与数据流

内容类型流：

```text
Card.Content changed
  -> Dispose old content state bindings
  -> inspect direct content
  -> ContentType = Meta / Grid / Tabs / Default
  -> bind SizeType/IsMotionEnabled to Grid or Tabs content when needed
  -> ConfigureContentCornerRadius()
  -> ConfigureHeaderBorderThickness()
```

`ContentType` 是内部 DirectProperty。它只由根 Card 根据直接 `Content` 类型维护，主题根据该状态选择 body padding、header border 和 corner radius。

根模板中的 `Frame` 是实际内容边框容器，Header、Cover、Body 和 Actions 都在其 border 内缘参与布局。内部 Header/Actions
分隔线不得与外框使用覆盖式同级叠放，否则自定义外框颜色后，内部横线会覆盖外框交点。

Actions 流：

```text
Card.Actions.CollectionChanged
  Add/Remove/Replace/Move
  -> PART_ActionPanel.Actions
  -> IsActionsPanelVisible = Actions.Count > 0
```

`CardActionPanel.Actions.CollectionChanged` 再同步到 `UniformGrid.Children`，并设置 `UniformGrid.Columns = Actions.Count`。

Grid 内容流：

```text
CardGridContent.ColumnDefinitions / RowDefinitions
  -> SyncGridPanelProperties()
  -> ItemsPresenter.Panel as Grid
  -> grid.ColumnDefinitions / grid.RowDefinitions
```

Grid item 容器流：

```text
PrepareContainerForItemOverride
  -> CardGridItem.Content / ContentTemplate
  -> bind SizeType / IsEnabled / IsMotionEnabled
  -> PrepareCardGridItem(..., CompositeDisposable)

ClearContainerForItemOverride
  -> dispose prepared container state
```

Tabs 内容流：

```text
CardTabsContent.Items.CollectionChanged
  -> PART_TabControl.Items Add/Remove/Replace/Move
```

`TabItemsSource` 和 `TabItemTemplate` 通过 TemplateBinding 直接交给内部 TabControl。

## 5. 生命周期与模板接入

Card 构造阶段：

- 注册 `CardToken.ScopeProvider`。
- 订阅 `Actions.CollectionChanged`。`Actions` 是 Card 自身持有的生命周期集合，不需要在普通 detach 中解除。

Card 初始化和加载：

- `OnInitialized()` 调用 `DisableTransitions()`，避免初始样式应用触发过渡。
- `OnLoaded()` 通过 dispatcher 在加载完成后启用过渡。

Card 模板接入：

- `OnApplyTemplate()` 清空旧 `CardActionPanel.Actions`。
- 获取 `PART_ActionPanel`。
- 将当前 `Actions` 复制到新的 action panel。
- 重新配置 ContentType、Content corner radius 和 Header border。

内容状态绑定：

- `CardTabsContent` 和 `CardGridContent` 作为直接内容时，Card 使用 `BindUtils.RelayBind` 同步 `SizeType` 和 `IsMotionEnabled`。
- 该绑定无法放在 Card 根模板中表达，因为绑定目标是运行时 `Content` 对象而非稳定 template part。
- `_contentStateBindings` 是这组绑定的 owner，内容重新配置时必须先释放旧绑定。

GridContent 生命周期：

- `OnApplyTemplate()` 获取 `PART_ItemsPresenter`，调用 `ApplyTemplate()` 后同步 Grid 行列定义。
- `PrepareContainerForItemOverride()` 每次准备容器前先释放该容器旧状态，防止 recycle 时保留旧 binding 或订阅。
- `ClearContainerForItemOverride()` 释放对应 `CompositeDisposable`，再调用 base。

TabsContent 生命周期：

- 构造阶段订阅自有 `Items.CollectionChanged`。
- `OnApplyTemplate()` 清空旧内部 TabControl items，获取 `PART_TabControl`，再复制当前 Items。
- `Items` 是控件自有集合，订阅随控件实例生命周期存在。

## 6. Semantic Part 实现映射

Card 家族只为 `Card` 和 `CardMetaContent` 生成独立 descriptor。两个 owner 的 root 由生成器隐式加入；其余 Part 使用
`SemanticPartAttribute` 声明，并由内置 ControlTemplate 在模板初始化阶段静态设置 marker。

### 6.1 Card 映射

| Part | Selector class | ContractType | 真实节点 | 创建方式 | 数量与生命周期 |
| --- | --- | --- | --- | --- | --- |
| `root` | 无 | `Card` | Card owner | 用户或框架创建 | owner 生命周期内始终一个。 |
| `header` | `semantic-header` | `DashedBorder` | `HeaderFrame` PixelAlignedBorder | Card 根模板静态创建 | 模板实例内一个；headerless 只隐藏。 |
| `title` | `semantic-title` | `ContentPresenter` | `TitlePresenter` | Card 根模板静态创建 | 模板实例内一个；Header 为空时保留。 |
| `extra` | `semantic-extra` | `ContentPresenter` | `HeaderExtra` | Card 根模板静态创建 | 模板实例内一个；Extra 为空时保留。 |
| `cover` | `semantic-cover` | `Border` | `CoverFrame` | Card 根模板静态创建 | 模板实例内一个；Cover 为空时保留。 |
| `body` | `semantic-body` | `Border` | `CardContent` | Card 根模板静态创建 | 模板实例内一个；Loading 与 ContentType 切换不替换。 |
| `actions` | `semantic-actions` | `TemplatedControl` | `PART_ActionPanel` CardActionPanel owner 节点 | Card 根模板静态创建 | 模板实例内一个；Actions 为空时只隐藏。 |

`actions` marker 停留在 Card 根模板中的 action panel owner 节点。实现不得为了定制背景或分隔线连续进入
`CardActionPanel` 的第二个 ControlTemplate；默认 action panel theme 应把可公开视觉值投影到 marker owner 可以覆盖的属性。

### 6.2 CardMetaContent 映射

| Part | Selector class | ContractType | 真实节点 | 创建方式 | 数量与生命周期 |
| --- | --- | --- | --- | --- | --- |
| `root` | 无 | `CardMetaContent` | CardMetaContent owner | 用户或框架创建 | owner 生命周期内始终一个。 |
| `section` | `semantic-section` | `Control` | 标题与描述外层 DockPanel | Meta 根模板静态创建 | 模板实例内一个；内容为空时保留。 |
| `avatar` | `semantic-avatar` | `ContentPresenter` | `AvatarContentPresenter` | Meta 根模板静态创建 | 模板实例内一个；Avatar 为空时保留。 |
| `title` | `semantic-title` | `ContentPresenter` | `TitleContentPresenter` | Meta 根模板静态创建 | 模板实例内一个；Header 为空时保留。 |
| `description` | `semantic-description` | `ContentPresenter` | `DescriptionContentPresenter` | Meta 根模板静态创建 | 模板实例内一个；Content 为空时保留。 |

Card 与 CardMetaContent 的同名 `semantic-title` 通过 descriptor owner 和 owner-scoped Selector 隔离。marker 不传播到用户
`HeaderTemplate`、`ContentTemplate`、Avatar 控件模板或其他内容子树。

### 6.3 非 owner 边界

`CardGridContent`、`CardGridItem`、`CardTabsContent` 和 `CardActionButton` 不声明 Card 家族 descriptor：

- Card body marker 只表示主体 frame，不把运行时 ItemsControl container 或 TabControl 子树纳入 Card owner。
- Card actions marker 只表示操作组整体，不把单个 action 或 CardActionButton 模板纳入 Card owner。
- 这些 public 类型的 API、Theme 和生命周期保持独立；不能因为它们位于 Card 视觉树中就自动获得 Part identity。

所有 marker 使用静态 `Classes.semantic-*="True"`。默认 Card Themes 不以 `.semantic-*` 作为内部样式 selector，因此未声明
用户 Semantic Style 时不增加 selector activator、Binding、订阅或运行时 VisualTree 查询。

## 7. 交互与事件处理

Card 自身不处理 keyboard、focus、pointer capture、drag/drop 或 popup 事件。

交互路径主要由主题和内部控件负责：

- `IsHoverable` 通过 selector 设置 cursor、pointer over 阴影和边框厚度。
- `CardActionPanel` pointer over 改变 Foreground。
- `CardActionButton` pointer over 改变 IconBrush。
- Tab 选择由内部 `TabControl` 处理。
- Loading 状态由 `Skeleton` 处理。

集合事件路径：

- `Card.Actions.CollectionChanged`
- `CardActionPanel.Actions.CollectionChanged`
- `CardTabsContent.Items.CollectionChanged`

这些路径只同步视觉集合，不执行业务命令。

## 8. 内部算法与关键流程

### 8.1 ContentType 配置

内容类型配置必须先释放旧内容状态绑定，再按当前直接内容设置内部状态：

```text
CardMetaContent -> Meta
CardTabsContent -> Tabs + bind state
CardGridContent -> Grid + bind state
others/null -> Default
```

`Default` 回退是必要状态，防止从 Grid/Tabs/Meta 切回普通内容后主题仍保留旧 padding、border 或 corner radius 分支。

`StyleVariant=Borderless` 的默认零边框由 Theme selector 设置 `BorderThickness=0`，`EffectiveBorderThickness` 只同步 owner 当前
`BorderThickness`。这样默认 Borderless 仍无边框，而更高优先级的 owner-scoped Semantic root style 可以显式恢复边框；实现不得在
C# 中按 variant 把派生边框厚度永久封死为零。

### 8.2 Header 边框和内容圆角

Card 根据 `ContentType` 计算两个内部状态：

```text
ContentType=Grid
  HeaderBorderThickness = 0
  EffectiveCornerRadius = top corners only

otherwise
  HeaderBorderThickness = bottom border
  EffectiveCornerRadius = CornerRadius
```

Borderless 和 Outlined 的实际边框厚度由 `ConfigureContentBorderThickness()` 根据 `StyleVariant` 计算。

### 8.3 Headerless 伪类

Headerless 条件：

```text
Header == null
&& HeaderTemplate == null
&& Extra == null
&& ExtraTemplate == null
```

该伪类只表示 Header 区域是否有可展示入口，不受 Cover、Content、Actions 或 Loading 影响。

### 8.4 Grid item 准备和清理

`CardGridContent` 复用 Avalonia ItemsControl 容器机制：

- 非 Visual item 且 `CardGridItem.Content` 未显式设置时，将 item 作为 content。
- `ItemTemplate` 非空时同步到 `CardGridItem.ContentTemplate`。
- Size、Enabled 和 Motion 状态通过 Avalonia property binding 同步到容器。
- `PrepareCardGridItem` 允许派生类追加容器级绑定或订阅，追加内容必须放入传入的 `CompositeDisposable`。

容器 recycle 或清理时，`CompositeDisposable` 必须被释放，防止旧 item 状态泄漏到新 item。

### 8.5 Actions 渲染

`CardActionPanel` 使用 `UniformGrid` 均分 action。Render 阶段用当前 `Actions` 子项的 bounds 绘制：

- 顶部横线。
- 每个非最后 action 右侧纵向分隔线。

分隔线颜色和粗细来自 `BorderBrush` 和 `BorderThickness.Left`。

## 9. 资源、性能与 AOT 边界

Card 不依赖运行时反射发现模板结构。模板协作通过固定 part 名称、强类型控件和 Avalonia property binding 完成。

资源与生命周期边界：

- `CardToken` 通过 token generator 注册，Theme 通过 `CardTokenResource` 使用。
- `CardActionButton`、`CardGridItem`、`CardMetaContent` 等是 Visual 控件，Token resource 由主题和视觉树生命周期管理。
- `Card` 到动态 `Content` 的 Size/Motion 同步使用 `BindUtils.RelayBind`，因为目标不是稳定 template part；绑定 owner 是 `_contentStateBindings`。
- `CardGridContent.PrepareCardGridItem` 的扩展 disposable 由 `_cardGridItemDisposables` 管理，并在 container clear/recycle 时释放。
- 不在 layout hot path 中读取全局资源或创建反射路径。
- Semantic descriptor 由 Generator 静态生成，marker 随模板节点一次性初始化；Control 包运行时不查询 registry，也不扫描
  VisualTree。
- Card 和 CardMetaContent 的 marker 都位于既有静态节点，不创建额外 Visual、Binding、订阅或跨 VisualRoot host。
- Frame 作为既有边框节点直接承载内容，不新增视觉节点；该结构同时避免内部 Header/Actions 分隔线覆盖外框。

AOT 边界：

- 不新增字符串 path binding、`ReflectionBinding`、assembly scan、`Activator.CreateInstance(Type)` 或动态成员访问。
- API 与 Token 契约由控件文档、源码 public surface、Token 类型或生成数据维护。
- Card 的动态内容同步只使用 `AvaloniaProperty` 强类型绑定。

## 10. 维护不变量

内部重构必须保持以下不变量：

- `ContentType` 必须覆盖 Meta、Grid、Tabs 和 Default 四个分支。
- 重新配置内容类型前必须释放旧内容状态绑定。
- `CardGridContent` 的行列定义 wrapper 必须读写 `ColumnDefinitionsProperty` 和 `RowDefinitionsProperty`。
- `PrepareCardGridItem` 传入的 `CompositeDisposable` 必须有清理路径。
- `CardActionPanel` 换模板时必须清空旧 panel children，避免 action visual 同时挂到多个父级。
- `CardTabsContent` 换模板时必须清空旧 TabControl items，再复制当前 Items。
- `Actions`、`CardTabsContent.Items` 的 Reset 行为当前为 `NotSupportedException`，不能在结构整理中静默改变。
- 初始 transition 禁用/加载后启用的顺序不能在未验证视觉影响时移除。
- 空实现或薄实现的主题语义类型不能随意删除。
- `Card` descriptor 必须只包含 `root/header/title/extra/cover/body/actions`，`CardMetaContent` descriptor 必须只包含
  `root/section/avatar/title/description`。
- Card 与 CardMetaContent 的同名 title marker 必须由 owner Selector 隔离，不能使用类型前缀编码 Part identity。
- headerless、空 Cover、空 Actions、Loading 和 ContentType 切换只能改变内容或可见性，不能改变静态 Part cardinality。
- 根 Frame 必须承载 Card 内部 DockPanel，使 Header 与 Actions 分隔线从外框内缘开始；不得恢复为同级覆盖绘制。
- Borderless 的零边框必须保留为 Theme 默认值，不能在 C# 派生状态中阻止 owner-scoped root style 覆盖 `BorderThickness`。
- Card actions marker 不能进入 internal CardActionPanel 的第二个模板边界；Grid、Tabs 和 action child 不得被 Card owner 穿透。
- 默认 Card Themes 不得消费 `.semantic-*`，避免为未使用该能力的 Card 增加动态 selector 成本。
- 布局型 Semantic Style 必须建立在一套完整的 `SizeType` 分支上；未覆盖属性继续使用该分支的 Token 基线，不得在示例中
  局部拼接不同尺寸档位的 Header `MinHeight`、字体与 Padding。

## 11. 测试与验证

验证范围：

- `CardBehaviorTests`：CardGridContent 行列定义 wrapper、ContentType 回退、旧内容 SizeType 绑定释放、GridItem container disposable 释放。
- `CardBehaviorTests` 还验证 Frame 内缘布局、Borderless 默认无边框，以及 owner-scoped root style 可恢复 Borderless 外框。
- `CardSemanticPartTests`：验证 `Card` 与 `CardMetaContent` descriptor、静态 marker、真实模板 cardinality、Headerless、Loading、Meta/Grid/Tabs、动态 Actions 和同名 title owner 隔离。
- Gallery Card 示例：Basic、NoBorder、Simple、CustomizedContent、CardInColumn、GridCard、InnerCard、LoadingCard、WithTabs、MoreContentConfiguration 和 `v6.1.3` Semantic Part 样例。
- `CardShowCasePageTests`：Card ShowCase 页面结构、本地化、源码片段、示例 snapshot，以及 Semantic 样例完整尺寸基线。
- 修改运行时代码时运行 `tests/AtomUI.Desktop.Controls.Tests` 中 Card 相关测试，并按影响范围扩大到完整 Desktop 控件测试。
- 修改 Gallery 或 Gallery 示例或源码片段时运行 `tests/AtomUIGallery.Tests` 中 Card ShowCase 相关测试。
- Semantic Part 验证覆盖两个 descriptor、静态 marker、空区域、Loading、Meta/Grid/Tabs、动态 Actions、owner 隔离和未声明
  owner 排除边界。
- 文档改动运行 `git diff --check`，并检查相对链接存在。
