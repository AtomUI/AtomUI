# Expander 桌面版架构设计

本文档定义 `AtomUI.Desktop.Controls.Expander` 桌面版的最新设计定位、公共契约、状态模型、视觉主题关系和兼容边界。通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，内部实现原理见 [Expander 桌面版实现原理](implementation.md)，Semantic Part 契约见 [Expander Semantic Part 契约](semantic-part.md)，Expander Token 的专项设计见 [Expander Token 设计](token.md)，设计和契约变化记录见 [Expander Changelog](changelog.md)。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/DataDisplay/Expander` |
| 控件状态 | Stable |

Expander 是桌面端数据展示类单面板折叠容器，用于在有限空间内展示一段可展开或收起的内容。它以 Avalonia `Expander` 为基础，扩展 AtomUI 的尺寸、展开图标、附加内容、触发区域、边框模式、Ghost 模式、展开方向、动效和 Token 体系。

Expander 的职责是管理一个 Header 与一个 Content 区域之间的展开关系。它不是多面板集合控件，不负责手风琴互斥展开、列表虚拟化、数据项生成、表单校验、远程加载或复杂主从详情关系。需要多面板协调时应使用 Collapse；需要静态分组时应使用 GroupBox；需要列表或树形数据展示时应使用 ListView、TreeView 或 DataGrid。

## 2. 设计语言

Expander 的设计语言来自 参考设计体系的轻量折叠面板：Header 表达当前内容主题，展开图标表达折叠状态和方向，Content 承载可延迟阅读的内容。

| 维度 | 含义 | 典型表达 |
| --- | --- | --- |
| 层级收纳 | Header 始终可见，Content 按状态显示或隐藏。 | `Header`、`Content`、`IsExpanded`。 |
| 展开方向 | 内容可以向下、向上、向左或向右展开。 | `ExpandDirection`。 |
| 触发区域 | 可配置整行 Header 触发或仅图标触发。 | `TriggerType`。 |
| 图标语义 | 展开图标表示折叠状态、方向和可操作入口。 | `ExpandIcon`、`IsShowExpandIcon`、`ExpandIconPosition`。 |
| 视觉强度 | 普通、Borderless 和 Ghost 模式表达不同容器强度。 | `IsBorderless`、`IsGhostStyle`。 |
| 密度 | 尺寸和自定义 padding 控制 Header/Content 的空间密度。 | `SizeType`、`HeaderPadding`、`ContentPadding`。 |

Expander 应保持数据展示控件的克制视觉。Header 不是工具栏，AddOnContent 只承担轻量辅助入口，不改变折叠容器的主语义。

## 3. API 与契约模型

Expander 的公共契约分为继承的 Avalonia `Expander` 契约和 AtomUI 扩展契约。

继承契约：

| API | 语义 |
| --- | --- |
| `Header` / `HeaderTemplate` | Header 区域内容和模板。 |
| `Content` / `ContentTemplate` | 可展开内容区域和模板。 |
| `IsExpanded` | 展开状态。 |
| `ExpandDirection` | 展开方向，支持 `Down`、`Up`、`Left`、`Right`。 |
| `IsEnabled` | 禁用后 Header 和展开按钮进入禁用状态。 |
| `BorderThickness` | 普通模式根边框厚度来源。 |

AtomUI 扩展契约：

| API | 类型 | 语义 |
| --- | --- | --- |
| `SizeType` | `CustomizableSizeType` | 控件尺寸，支持 `Large`、`Middle`、`Small`、`Custom`。`Custom` 默认沿用 Middle 主题分支，显式 `HeaderPadding` / `ContentPadding` 可覆盖默认 spacing。 |
| `IsShowExpandIcon` | `bool` | 是否显示展开图标。 |
| `ExpandIcon` | `PathIcon?` | 自定义展开图标；为空时使用 `RightOutlined`。 |
| `AddOnContent` / `AddOnContentTemplate` | `object?` / `IDataTemplate?` | Header 右侧辅助内容及模板。 |
| `IsGhostStyle` | `bool` | 使用 Ghost 视觉，移除容器边框并使用内容背景作为 Header 背景。 |
| `IsBorderless` | `bool` | 使用无边框视觉，移除容器边框。 |
| `TriggerType` | `ExpanderTriggerType` | 触发区域，`Header` 表示 Header 区域可触发，`Icon` 表示仅图标可触发。 |
| `ExpandIconPosition` | `ExpanderIconPosition` | 展开图标位置，支持 `Start` 和 `End`。 |
| `HeaderPadding` | `Thickness?` | 显式 Header 内边距；为空时由 SizeType 和 ExpanderToken 决定。 |
| `ContentPadding` | `Thickness?` | 显式 Content 内边距；为空时由 SizeType 和 ExpanderToken 决定。 |
| `IsMotionEnabled` | `bool` | 是否启用展开/收起动效。 |

稳定 template part：

| Template Part | 类型 | 职责 |
| --- | --- | --- |
| `PART_Frame` | `PixelAlignedBorder` | 根边框、矩形裁剪与圆角裁剪（`ClipToBounds` + `ClipContentToCornerRadius`）和整体布局承载。 |
| `PART_MainLayout` | `DockPanel` | Header 与 Content 的 dock 布局。 |
| `PART_HeaderLayoutTransform` | `LayoutTransformControl` | 横向展开方向下旋转 Header。 |
| `PART_HeaderDecorator` | `PixelAlignedBorder` | Header 背景和 padding 承载，也是 Header 点击范围；不绘制 Header/Content 分隔线。 |
| `PART_HeaderLayout` | `Grid` | 展开图标、Header、AddOnContent 的列布局。 |
| `PART_ExpandButton` | `IconButton` | 展开图标显示和图标触发入口。 |
| `PART_HeaderPresenter` | `ContentPresenter` | Header 内容和模板承载。 |
| `PART_AddOnContentPresenter` | `ContentPresenter` | AddOnContent 内容和模板承载。 |
| `PART_ContentViewport` | `ContentViewport` | Content 分隔线和 Content 的共同展开/收起动效承载。 |
| `PART_ContentPresenter` | `ContentPresenter` | Content 内容、模板和 padding 承载。 |

`PART_ContentViewport` 内包含一个未命名 `PixelAlignedBorder`。该视觉固定拥有 Content 靠近 Header 一侧的分隔线，但不作为新的 template part 或 selector 契约暴露。

稳定伪类和主题状态：

| 伪类 / selector 状态 | 语义 |
| --- | --- |
| `:expanded` | 继承展开状态，用于展开图标旋转。 |
| `:up` / `:down` / `:left` / `:right` | 展开方向状态。 |
| `:custom-header-padding` | `HeaderPadding` 非空，Header padding 和图标间距使用显式值。 |
| `:custom-content-padding` | `ContentPadding` 非空，Content padding 使用显式值。 |
| `[IsBorderless=True]` / `[IsGhostStyle=True]` | 视觉强度分支。 |
| `[TriggerType=Header]` / `[TriggerType=Icon]` | Cursor 和点击路径分支。 |

Expander 没有专用 routed event 或 command。展开状态通过继承的 `IsExpanded` 表达。

### 3.5 Semantic Part 契约

`Expander` 公开与上游 Collapse 面板稳定 Semantic DOM 对齐的五个 Semantic Part，完整契约见 [Expander Semantic Part 契约](semantic-part.md)：

| Part | Selector | AtomUI 节点 | Cardinality | 定制方式 |
| --- | --- | --- | --- | --- |
| `root` | 控件本身 | `Expander` owner（根边框投影到 `PART_Frame`） | `Single` | owner 选择器 + 公开属性 |
| `header` | `.semantic-header` | `PixelAlignedBorder#PART_HeaderDecorator` | `Single` | `ExpanderHeaderStyle` |
| `icon` | `.semantic-icon` | `IconButton#PART_ExpandButton` | `Single` | `ExpanderIconStyle` |
| `title` | `.semantic-title` | `ContentPresenter#PART_HeaderPresenter` | `Single` | `ExpanderTitleStyle` |
| `body` | `.semantic-body` | `ContentPresenter#PART_ContentPresenter` | `Optional` | `ExpanderBodyStyle` |

Expander 是单面板控件，五个 Part 全部是 `ExpanderTheme.axaml` 单一模板内的静态节点，`TemplatedParent` 为 Expander owner
本身。因此 `header`、`icon`、`title`、`body` 都声明 `RuntimeCreated=false`、`CrossVisualRoot=false`，生成 Style 路由是单一的
`/template/` 边界，没有 `.semantic-scope-*` 中间锚点，也没有容器创建、prepare/clear/recycle 或跨视觉根路径。

`body` 是唯一声明 `Optional` 的 Part：它位于 `PART_ContentViewport` 内部，而该 actor 折叠稳定态下 `IsVisible=false`，
其内部 `ContentPresenter` 在首次呈现前不挂接视觉子级，因此 `body` 节点在从未展开的实例中从视觉树缺席；首次展开后节点
物化并保持存在。`header` / `icon` / `title` 是常驻节点，任何状态下都恰好命中一个。除 `body` 的这一条呈现历史差异外，
marker 不随展开、禁用、方向、尺寸档、图标位置、触发模式与视觉模式切换增删。定制摘要：

- 状态型定制（展开/收起、展开方向、禁用、Ghost、Borderless、TriggerType、ExpandIconPosition、SizeType、自定义 padding）
  通过 owner 公开属性完成，不改变 marker 数量。
- 局部视觉定制通过生成的 Semantic Style 完成，`ContractType` 收缩到公开类型（`PixelAlignedBorder` / `IconButton` /
  `ContentPresenter`），internal 节点不作为公共依赖类型。
- 布局型 Setter（固定 `Height` / `Width` / Min/Max 等）不作为公共定制路径：头部与内容高度由尺寸档 Padding/字体链与
  content motion 的尺寸进度共同驱动，见 [semantic-part.md §5](semantic-part.md#5-尺寸基线)。
- Header/Content 分隔线（`PART_ContentViewport` 内未命名 `PixelAlignedBorder`）、`PART_AddOnContentPresenter`、模板结构
  节点、动效 actor 与根表面背景/圆角/内边距都不属于 Semantic Part，见
  [semantic-part.md §6](semantic-part.md#6-定制边界)。

## 4. 行为与状态模型

核心状态流：

```text
Header pointer / ExpandButton click
        ↓
TriggerType gate
        ↓
IsExpanded
        ↓
Content motion / stable visibility
        ↓
PART_ContentViewport.IsVisible
```

触发语义：

- `TriggerType=Header` 时，鼠标左键按下且命中 `PART_HeaderDecorator` 会切换 `IsExpanded`。
- `TriggerType=Icon` 时，Header 点击不切换状态，只能通过 `PART_ExpandButton.Click` 切换。
- 禁用状态下模板将 `IsEnabled` 传递给 `PART_ExpandButton`，主题同步禁用前景。

展开方向：

- `ExpandDirection=Down`：Header 停留顶部，Content 向下展开。
- `ExpandDirection=Up`：Header dock 到底部，Content 向上展开。
- `ExpandDirection=Left` / `Right`：Header 通过 `LayoutTransformControl` 旋转，整体对齐到对应侧。
- 展开图标在 `:expanded` 下按展开方向旋转，必须与内容运动方向一致。

动效状态：

- `IsMotionEnabled=false` 时可见性入口立即同步 `PART_ContentViewport.IsVisible`，释放 Motion 的透明度覆盖。
- Core 共用内容展开机制在上下方向改变内容视口高度，左右方向改变宽度，内容按正常尺寸排版并由视口裁剪。
- 播放中反转必须从当前已呈现的尺寸和透明度接续；最终状态以最新 `IsExpanded` 为准，旧请求不能覆盖新请求。
- 四个方向共同保持内容与标题的锚定边、分隔线相邻关系、自然内容尺寸及首尾帧连续性。具体执行路径由本控件实现文档描述。

共享设计来源：`docs/architecture/systems/control-infrastructure/content-expansion.md`。

自定义 padding：

```text
HeaderPadding != null
  → :custom-header-padding
  → PART_HeaderDecorator.Padding = HeaderPadding
  → EffectiveExpandButtonMargin 按 HeaderPadding.Left/Right 推导

ContentPadding != null
  → :custom-content-padding
  → PART_ContentPresenter.Padding = ContentPadding
```

自定义 HeaderPadding 分支必须保持展开图标与 Header 文本的水平间距和垂直居中关系，不应继续套用默认尺寸 token 的图标间距。

## 5. 视觉与主题模型

Expander 的默认视觉由 `ExpanderTheme.axaml` 和 `ExpanderToken` 共同定义。

主题职责：

- 提供 Header、Content、ContentViewport 和 Frame 的稳定模板结构。
- 根据 `SizeType` 选择 Header/Content padding 和字体大小。
- 根据 `ExpandDirection` 设置 Header dock、旋转和图标旋转。
- Content 靠近 Header 的一侧固定承担分隔线，分隔线不依赖 `IsExpanded` 或 motion 时序。
- 根据 `IsBorderless` / `IsGhostStyle` 同时移除根边框和 Content 分隔线，并保持既有背景规则。
- 根据 `TriggerType` 设置可点击区域 cursor。
- 根据自定义 padding 伪类覆盖 Header/Content padding 和展开图标间距。

Token 关系：

```text
SharedToken
   ↓
ExpanderToken
   ↓
ExpanderTheme
   ↓
Frame + Header + ExpandButton + Content Border + Content
```

SharedToken 提供全局边框、字体、动效时长、图标大小和基础颜色。ExpanderToken 提供 Header/Content padding、Header/Content 背景、圆角和展开图标默认外边距。

## 6. 控件家族或集成关系

Expander 属于 Data Display 分类，是单面板折叠容器。

集成关系：

- Avalonia `Expander`：继承 Header、Content、IsExpanded 和 ExpandDirection 基础语义。
- Motion：提供内容动效基础设施；共用执行职责由[内容展开与收起动效设计](../../../../architecture/systems/control-infrastructure/content-expansion.md)定义，Expander 保留 `IsExpanded`、方向和模板接入所有权。
- Button/Icon：`PART_ExpandButton` 使用 AtomUI `IconButton` 和 默认 `RightOutlined` 默认图标。
- Token 系统：通过 `ExpanderToken.ScopeProvider` 注册控件 Token 资源作用域。
- Gallery：通过 Basic、Size、Borderless、Ghost、Custom Padding、Direction、Nested、No Arrow、Icon Position 和 Trigger 示例展示契约。

Expander 与 Collapse 的边界：

- Expander 管理单个 Header/Content 对。
- Collapse 管理多个 CollapseItem，并承担多面板和手风琴语义。

## 7. 兼容性不变量

维护 Expander 时必须保持以下不变量：

- 公共 API 名称、类型、默认值和继承语义不能在未授权情况下改变。
- `Header`、`Content`、`IsExpanded`、`ExpandDirection` 继续遵守 Avalonia `Expander` 语义。
- `PART_Frame`、`PART_HeaderDecorator`、`PART_ExpandButton`、`PART_HeaderPresenter`、`PART_AddOnContentPresenter`、`PART_ContentViewport`、`PART_ContentPresenter` 的名称和外部协作语义保持稳定。
- `PART_HeaderDecorator` 只负责 Header 背景和 padding，不承担 Header/Content 分隔线。
- Header/Content 分隔线必须位于 `PART_ContentViewport` 内部，并随 Content 自然显示、裁剪和隐藏。
- `TriggerType=Icon` 时 Header 点击不能切换 `IsExpanded`。
- `TriggerType=Header` 时 Header 区域点击应切换 `IsExpanded`。
- 默认 `ExpandIcon` 为空时必须补齐 `RightOutlined`。
- `IsBorderless` 和 `IsGhostStyle` 必须让有效根边框和 Content 分隔线厚度都为 `0`。
- `IsMotionEnabled=false` 必须直接进入稳定显示/隐藏状态，不留下 motion 临时值。
- 动画取消、模板重套用和 detach 时不能保留旧 motion actor 的运动属性或未释放 cancellation。
- `SizeType=Custom` 默认沿用 Middle 尺寸分支，显式 padding 覆盖默认 token。
- Token 名称、伪类名称和 template selector 入口不能擅自删除或重命名。

如果实现某项能力时无法保持这些不变量，应先停止实现，说明原因、影响范围、替代方案和迁移方式，并获得授权。

## 8. 专项模型

### 8.1 展开方向模型

展开方向同时影响 Header 的 dock、Header 横向旋转、内容 motion 方向和展开图标旋转。维护时必须把这四个维度作为一个模型处理，不能只修改其中一个视觉分支。

### 8.2 触发区域模型

`TriggerType` 是行为契约，不只是 cursor 样式。`Header` 分支允许 Header 区域切换状态，`Icon` 分支只允许图标按钮切换状态。测试应覆盖 Header 点击和 Icon 点击的差异。

### 8.3 自定义 padding 模型

`HeaderPadding` / `ContentPadding` 是实例级显式覆盖，不是 Token。显式 HeaderPadding 下，展开图标与文字之间的间距应由 HeaderPadding 的对应方向推导，以保持自定义紧凑场景下的视觉对称。

### 8.4 Motion 归一模型

动效的目标状态由 `IsExpanded` 单独拥有。公共机制负责尺寸、透明度、内容测量和执行生命周期，不给 Expander 增加
手风琴状态或公开动画配置。时长继续消费 `MotionDurationSlow` 的有效值，收放使用统一进度曲线。

横向展开保持完整内容的排版宽度，不能逐帧用缩小的视口宽度重新换行。Down / Right 以内容起边锚定，Up / Left
以靠近标题的尾边锚定，分隔线随内容保持在对应边。仅改变方向时按新方向和当前目标建立稳定布局，释放旧轴临时值。

取消前先保存当前画面，旧执行失效后才能取消并接续新动画；关动效、模板重套用和 detach 直接归一到当前目标。
尺寸接管同时覆盖高度与宽度，清理仅解除机制自身的内部进度和布局接入，保留模板原有尺寸、变换及绑定。完整算法及验证边界见
[内容展开与收起动效设计](../../../../architecture/systems/control-infrastructure/content-expansion.md)。

### 8.5 结构化分隔线模型

Header/Content 分隔线由未命名的 Content `PixelAlignedBorder` 拥有，不由 Header 或 `IsExpanded` selector 控制。默认 bordered 模式下，分隔线位于 Content 靠近 Header 的一侧：`Down` 为顶边、`Up` 为底边、`Left` 为右边、`Right` 为左边。Borderless 和 Ghost 模式下，根边框和分隔线均为零，既有背景规则保持不变。

分隔线与 Content 一起位于 `PART_ContentViewport` 内，因此收起时随 Content 自然隐藏。实现不得通过透明 Brush、边框 transition、延迟刷新或 motion completion 回调维护分隔线。

## 9. 文档导航与验证策略

关联文档：

- [Expander 桌面版实现原理](implementation.md)
- [Expander Semantic Part 契约](semantic-part.md)
- [内容展开与收起动效设计](../../../../architecture/systems/control-infrastructure/content-expansion.md)
- [Expander Token 设计](token.md)
- [Expander Changelog](changelog.md)

验证策略：

| 层次 | 验证内容 |
| --- | --- |
| Public API | `SizeType`、`IsShowExpandIcon`、`ExpandIcon`、`AddOnContent`、`IsGhostStyle`、`IsBorderless`、`TriggerType`、`ExpandIconPosition`、`HeaderPadding`、`ContentPadding`、`IsMotionEnabled`。 |
| 状态行为 | Header/Icon 触发差异、禁用状态、展开方向、嵌套 Expander、动画中状态切换、模板重套用和 detach。 |
| 内容动效设计 | 四方向逐帧验证尺寸、文字排版、锚定边及分隔线；覆盖首尾 tick、反转、关动效、方向切换和原有滚动约束。 |
| AXAML / Template | 稳定 template part、方向 selector、padding 伪类、图标位置 selector、禁用前景和 Header/Content padding。 |
| 圆角裁剪 | `PART_Frame` 的 `ClipContentToCornerRadius` 开启且圆角非零，header 位于角落圆角区域内；不透明 Part 背景不溢出圆角。headless 宿主会把裁剪降级为不应用，实际效果需在真实 Skia 后端（Gallery 桌面宿主）视觉验证，见 [implementation.md §4.2](implementation.md#42-圆角裁剪part-背景不溢出圆角)。 |
| Token | Header/Content padding、背景、圆角、展开图标 margin 与 Token 类型、生成数据和 token.md 语义说明一致。 |
| Semantic Part | 检查 descriptor 数量/顺序/字段、marker 数量与类型、五个 Part 在状态切换下的命中数量，见 [semantic-part.md §7](semantic-part.md#7-兼容性与验证)。 |
| Gallery | Basic、Size、Borderless、Ghost、Custom Padding、Direction、Nested、No Arrow、Icon Position、Trigger 示例。 |
| 文档 | 运行 `git diff --check`，检查相对链接存在。 |
