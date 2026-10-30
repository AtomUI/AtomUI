# Descriptions 桌面版实现原理

本文档描述 Descriptions 桌面版的数据物化、媒体断点订阅、生成视觉子项、Semantic Part 节点映射、Grid 布局算法、边框状态同步和维护边界。公共设计与 API 契约见 [Descriptions 桌面版架构设计](overview.md)，完整 Semantic Part 公共契约见 [Descriptions Semantic Part 契约](semantic-part.md)，Token 语义见 [Descriptions Token 设计](token.md)，变化记录见 [Descriptions Changelog](changelog.md)。

## 1. 实现定位

Descriptions 的实现重点是把 `DescriptionItem` 集合转换为一组内部控件，并根据响应式列数、布局方向、边框模式、span 和填充规则写入 `PART_GridLayout` 的 row、column 和 column span。

本文档只描述 Descriptions 相关实现结构，不重新说明 `Grid`、`ResponsiveInt`、`TemplatedControl` 或 Token 系统的通用机制。

## 2. 源码文件结构

主要源码：

- `src/AtomUI.Desktop.Controls/Descriptions/Descriptions.cs`：公共 API、Items 集合、ItemsSource 物化、媒体断点订阅、生成视觉子项和布局算法。
- `src/AtomUI.Desktop.Controls/Descriptions/DescriptionItem.cs`：非视觉 `AvaloniaObject` 描述项模型和 `DescriptionItems` 集合类型。
- `src/AtomUI.Desktop.Controls/Descriptions/DescriptionDefaultItem.cs`：普通项和纵向边框项的内部承载控件。
- `src/AtomUI.Desktop.Controls/Descriptions/DescriptionBorderedCell.cs`：水平边框模式 label/content cell 的共享内部基类。
- `src/AtomUI.Desktop.Controls/Descriptions/DescriptionBorderedItemLabel.cs`：水平边框模式 label cell 类型。
- `src/AtomUI.Desktop.Controls/Descriptions/DescriptionBorderedItemContent.cs`：水平边框模式 content cell 类型。
- `src/AtomUI.Desktop.Controls/Descriptions/DescriptionsToken.cs`：Descriptions 控件 Token。
- `src/AtomUI.Desktop.Controls/Descriptions/Themes/*.axaml`：根模板、普通项模板、边框 cell 模板和 token 样式。

## 3. 核心类职责

`Descriptions` 是状态协调器。它持有 `Items` 集合，监听集合变化，感知媒体断点，生成内部视觉子控件，并执行 Grid 布局。

`DescriptionItem` 是非视觉 `AvaloniaObject` 描述项模型。它承载 item 级 Avalonia 属性，但不进入视觉树；属性绑定、属性变化订阅和生成视觉映射都由 owning `Descriptions` 管理。

`DescriptionDefaultItem` 是普通布局和纵向边框布局的内部控件。它承载 Header/Content、冒号显示、布局方向、边框状态和有效边框厚度。

`DescriptionBorderedCell` 是水平边框模式 label/content cell 的共享内部基类，负责尺寸、行高、最后行/列状态和有效边框厚度。

`DescriptionBorderedItemLabel` 和 `DescriptionBorderedItemContent` 是主题区分 label/content 的 marker 类型。它们没有额外逻辑，但其类型名称是主题选择器和视觉语义边界。

Descriptions 是唯一 Semantic Part public owner。`DescriptionItem` 与所有内部生成控件都不建立独立 descriptor；完整 Part
定义见 [Descriptions Semantic Part 契约](semantic-part.md)。

## 4. 状态与数据流

ItemsSource 流：

```text
ItemsSource changed
  → if ItemsSource != null
  → Items.Clear()
  → CollectItemsSourceItems()
  → Items.AddRange(description items)
  → collection changed
  → generated controls + layout
```

当前实现只在 `ItemsSource` 非 null 时重新物化数据。把 `ItemsSource=null` 改成清空 `Items` 属于可观察行为变化，不能混入结构优化。

Items 集合流：

```text
Items.CollectionChanged
  Add    → attach item property subscriptions
         → insert generated controls at grid child index
  Remove → detach item property subscriptions
         → remove generated controls by original collection index
  Reset  → detach all item property subscriptions
         → clear generated controls and item mappings
  Move / Replace → NotSupportedException
  → DoLayoutChildren()
  → InvalidateMeasure()
```

DescriptionItem 属性流：

```text
DescriptionItem.Label / Content changed
  → update generated label/content presenter

DescriptionItem.Span / IsFilled changed
  → DoLayoutChildren()
  → InvalidateMeasure()
```

`DescriptionItem` 不参与视觉树，因此它的 property changed handler 不能反向持有旧生成视觉。生成视觉重建前必须先解除旧 item 订阅并清空 item 到 visual 的映射。

媒体断点流：

```text
OnAttachedToVisualTree
  → MediaQueryHost.FindOwner(this)
  → subscribe MediaBreakPointChanged
  → resolve columns
  → layout

OnDetachedFromVisualTree
  → unsubscribe MediaBreakPointChanged
```

生成视觉流：

```text
Layout=Horizontal && IsBordered=true
  → DescriptionBorderedItemLabel + DescriptionBorderedItemContent

otherwise
  → DescriptionDefaultItem
```

## 5. 生命周期与模板接入

构造阶段：

- 注册 `DescriptionsToken.ScopeProvider`。
- 订阅默认 `Items.CollectionChanged`。
- 对默认 `Items` 中已有 item 建立属性变化订阅时，必须与集合替换、reset、detach 或模板重建释放路径配对。

附加到视觉树：

- 查找最近的 `IMediaBreakAwareControl`。
- 保存 `_mediaOwner` 和 `_breakPoint`。
- 订阅 `MediaBreakPointChanged`。
- 根据当前断点更新列数并布局。

脱离视觉树：

- 解除 `_mediaOwner.MediaBreakPointChanged`。
- 清空 `_mediaOwner`。

模板接入：

- `RootFrame` 通过 `TemplateBinding` 消费 owner 的 `Background`、`BorderBrush`、`BorderThickness`、`CornerRadius` 和 `Padding`，
  并包裹 Header 与 `ContentFrame`，使 root Semantic Part 表达完整控件表面。
- `OnApplyTemplate()` 获取 `PART_GridLayout`。
- 根据当前 `Items` 重建生成视觉。
- 根据当前断点更新列数并布局。

`Items` 属性替换时必须从旧集合解绑，再订阅新集合；如果模板已经接入且控件附加到视觉树，则清空旧视觉、生成新视觉并重新布局。

`DescriptionItem` 生命周期：

- item 进入 `Items` 时，由 `Descriptions` 订阅该 item 的 Avalonia property changed。
- item 离开 `Items`、集合 reset、`Items` 替换、模板重新应用或控件 detach 时，必须解除该 item 的属性订阅。
- 生成视觉重建时，必须先释放旧 generated control 与 item 的映射，再按当前布局模式建立新映射。
- 任何从 item 属性转接到 generated control 的 binding、事件或 disposable，都必须归属同一个 item attach 生命周期。

## 6. Semantic Part 实现映射

`Descriptions` descriptor 包含 `root`、`header`、`title`、`extra`、`label` 和 `content`。root 由生成器隐式加入；其余 Part
使用 `SemanticPartAttribute` 声明。

静态根模板映射：

| Part | Marker 节点 | ContractType | 创建方式 |
| --- | --- | --- | --- |
| `header` | `HeaderLayout` | `DockPanel` | 根 ControlTemplate 静态创建。 |
| `title` | `HeaderPresenter` | `ContentPresenter` | 根 ControlTemplate 静态创建。 |
| `extra` | `ExtraPresenter` | `ContentPresenter` | 根 ControlTemplate 静态创建。 |

这些节点使用 `Classes.semantic-*="True"` 静态 marker。Header/Extra 为空只改变 `HeaderLayout.IsVisible` 或 presenter 内容，不
删除节点，因此三者均为 `Single + RuntimeCreated=false`。

运行时 item 映射：

| 模式 | `label` marker | `content` marker |
| --- | --- | --- |
| Horizontal + non-bordered | `DescriptionDefaultItem` 模板的 Label presenter | `DescriptionDefaultItem` 模板的 Content presenter |
| Horizontal + bordered | label cell 模板的 ContentPresenter | content cell 模板的 ContentPresenter |
| Vertical + non-bordered | `DescriptionDefaultItem` 模板的 Label presenter | `DescriptionDefaultItem` 模板的 Content presenter |
| Vertical + bordered | `DescriptionDefaultItem` 模板的 Label presenter | `DescriptionDefaultItem` 模板的 Content presenter |

四种模式的 target 类型统一为 `ContentPresenter`。水平 bordered 的 `DescriptionBorderedItemLabel` 和
`DescriptionBorderedItemContent` 仍负责 cell 的有效边框厚度，但 `Padding`、`Background`、`Foreground` 和文本排版基线
必须通过 `TemplateBinding` 投影到内部 ContentPresenter；外层 `PixelAlignedBorder` 只绘制边框。这样默认基线和用户
Semantic Setter 作用于同一个目标节点，`Padding` 是覆盖预设 cell padding，而不是在外层 padding 内再次叠加。

内部 item control 在 `OnApplyTemplate()` 中通过当前 NameScope 获取目标 presenter，并使用生成的 class 常量添加
`semantic-label` / `semantic-content` marker：`DescriptionDefaultItem` 同时标记 Label 与 Content presenter；水平 bordered 的
label/content cell 各自标记自己的 `ContentPresenter`。每个已物化 item 必须产生一对目标；空集合没有 item-scoped target。
`Layout` 或 `IsBordered` 变化会销毁旧 target 并创建新 target，不改变公共 Part 身份。

marker 接入必须幂等：每次 `OnApplyTemplate()` 只标记当前 NameScope 中的 presenter，不缓存或重新挂接旧模板节点。
`RuntimeCreated=true` 表示 marker 需要随运行时物化的 item template 接入，不表示 Semantic Part 新建 presenter、wrapper 或
其他视觉节点。

Selector 边界由 Avalonia 12 的真实模板关系决定：

- header/title/extra 的直接 `TemplatedParent` 是 `Descriptions`；对应生成 Style 封装单段 `/template/ .semantic-*` route，
  用户只在 Descriptions owner Style 中嵌套 `DescriptionsHeaderStyle`、`DescriptionsTitleStyle` 或 `DescriptionsExtraStyle`。
- `PART_GridLayout` 增加 `.semantic-scope-items`；每个直接生成的 item host 增加 `.semantic-scope-item`。两者只承担稳定 route，
  不进入 descriptor，也不表示额外公共 Part。
- label/content 的完整 route 为
  `/template/ .semantic-scope-items > .semantic-scope-item /template/ .semantic-label|content`。
- Avalonia 12 的普通 Style 支持多个 `/template/`；每段分别按直接 `TemplatedParent` 匹配。只有 ControlTheme 内嵌 Style 拒绝
  多个 template selector，本 route 不进入默认 ControlTheme Style。
- 不得使用 logical descendant。它会沿全部 LogicalParent 祖先匹配，并穿透 Extra 的 Button 或 content 中嵌套的同类型
  Descriptions。也不能使用内部类型 selector，或给 `.semantic-*` 添加 `ContractType` 类型前缀。

Semantic marker 只在 descriptor 声明和目标节点创建处出现。默认主题不能使用 `.semantic-*` 驱动 AtomUI 基线视觉，避免未
声明用户 Semantic Style 时引入额外动态 class selector activator。

## 7. 交互与事件处理

Descriptions 不处理 pointer、keyboard、focus、drag/drop、popup 或 command 事件。

实现中需要维护的事件路径只有：

- `Items.CollectionChanged`
- `IMediaBreakAwareControl.MediaBreakPointChanged`
- Avalonia property changed

property changed 分发：

- `ItemsSource`：非 null 时物化到 `Items`。
- `IsBordered`：水平模式下重建生成视觉，并重新计算列数与布局。
- `Header` / `Extra`：同步 `IsHeaderLayoutVisible`。
- `ColumnInfo`：当前有断点时重新计算列数与布局。
- `Layout`：控件已附加到视觉树时重建生成视觉，并重新布局。
- `DescriptionItem.Label` / `Content`：更新对应 generated control 的 label/content。
- `DescriptionItem.Span` / `IsFilled`：重新计算布局和测量。

## 8. 内部算法与关键流程

### 8.1 有效列数

控件先解析展示列数：

```text
columns = ColumnInfo?.Resolve(currentBreakPoint, fallback)
       ?? fallback
```

默认 fallback：

```text
ExtraSmall = 1
Small = 2
ExtraExtraExtraLarge = 4
others = 3
```

再转换为内部有效列数：

```text
Layout=Horizontal && IsBordered
  → effectiveColumns = columns * 2
otherwise
  → effectiveColumns = columns
```

### 8.2 生成子控件索引

Grid 子控件索引由当前布局模式决定：

```text
horizontal bordered: itemIndex * 2
otherwise: itemIndex
```

布局时必须使用 `for` 循环中的 `itemIndex`。不能使用 `Items.IndexOf(item)` 作为布局主路径，因为生成视觉的顺序语义来自集合位置；item 对象引用只用于维护当前生成周期内的订阅和映射。

### 8.3 水平边框布局

每个 item 生成 label cell 和 content cell：

```text
label cell:
  row = current row
  column = current column
  column += 1

content cell:
  span = GetItemSpan(item.Span) * 2 - 1
  span = clamp(span, 1, effectiveColumns - column)
  if last item or IsFilled: span = effectiveColumns - column
  column += span
```

当 `column >= effectiveColumns` 时，当前 content cell 标记为最后列，进入下一行。

### 8.4 普通和纵向布局

普通水平、纵向非边框和纵向边框都使用 `DescriptionDefaultItem`。布局算法对这些模式使用相同的 span 计算：

```text
span = GetItemSpan(item.Span)
span = clamp(span, 1, effectiveColumns - column)
if last item or IsFilled: span = effectiveColumns - column
```

纵向布局还会同步 `IsLastColumn` 和 `IsLastRow`，用于 `DescriptionDefaultItem` 计算有效边框厚度。

### 8.5 最后一行和边框厚度

布局完成后，控件遍历生成视觉并根据 `Grid.GetRow(control) == rowCount - 1` 标记最后行。

内部 item 根据 `IsLastRow`、`IsLastColumn` 和 `BorderThickness` 计算 `EffectiveBorderThickness`：

- 非最后行、非最后列：保留右边和下边。
- 非最后行、最后列：保留下边。
- 最后行、非最后列：保留右边。
- 最后行、最后列：不保留内部边框。

### 8.6 生成视觉重建

`IsBordered` 和 `Layout` 会改变生成视觉的类型和数量，因此不能只重新布局。水平边框模式和普通模式之间切换时必须先清空 `PART_GridLayout.Children`，再按当前 `Items` 重建视觉。

## 9. 资源、性能与 AOT 边界

Descriptions 不依赖运行时反射发现模板结构。模板协作通过固定 part 名称、显式内部类型和 Avalonia property binding 完成。

资源与生命周期边界：

- `DescriptionsToken` 通过 token generator 注册，Theme 通过 `DescriptionsTokenResource` 使用。
- 生成的内部控件是 Visual，Token resource 由主题和视觉树生命周期管理。
- `DescriptionItem` 是非视觉 `AvaloniaObject`，只能承载 item 级数据属性和普通 binding target 语义。
- `IsShowColon`、`IsBordered` 和 `SizeType` 的内部同步使用 Avalonia property binding，不使用 `BindUtils.RelayBind`。
- 媒体断点事件订阅由 `OnAttachedToVisualTree` / `OnDetachedFromVisualTree` 配对管理。
- 不在 `Render`、layout hot path 或数据物化路径中读取全局资源。
- descriptor 和 selector class 由生成器静态产生；控件运行时不读取 registry、不扫描模板、不遍历 VisualTree 查找 marker。
- item marker 只在内部控件模板接入时添加到既有 presenter，不为 Semantic Part 增加 wrapper、subscription、binding、
  VisualTree 查询或独立视觉节点。
- `Items` 增删与布局模式重建仍按现有 O(n) 生成视觉成本执行；Semantic Part 不增加新的渐近复杂度。

### 9.1 非视觉 DescriptionItem 的泄露边界

`DescriptionItem` 继承 `AvaloniaObject` 后，binding expression、property changed subscription 和资源查找都有了真实生命周期成本。实现必须把 acquire/release 写成对称路径，不能只依赖 GC 或视觉树清理。

需要防范的泄露链：

```text
DescriptionItem.PropertyChanged subscription
  → Descriptions / generated control
  → old ShowCase visual tree
```

```text
generated control / presenter binding
  → old DescriptionItem
  → old owner / content visual
```

```text
Application.ResourcesChanged
  → DynamicResourceExpression
  → DescriptionItem.ValueStore
  → owner / generated control / ShowCase
```

标准防范规则：

- item attach 时注册属性变化订阅，item detach 时解除同一个订阅。
- generated control 的 binding、事件和 disposable 归属 generated control 或 item attach 生命周期；视觉重建前必须统一释放。
- item 到 generated control 的映射只服务当前模板和布局周期；生成视觉清理路径必须同时清理映射和 item 订阅。
- 主题资源优先绑定在生成出来的 Visual 控件上，让视觉树生命周期管理 `DynamicResource`。
- `DescriptionItem` 上禁止直接放 `DynamicResource` 或 token-resource binding，除非它实现 scoped `IResourceHost` / `IThemeVariantHost`，并有 owner attach/detach 与 WeakReference 测试。

### 9.2 DynamicResource Scoped Host 规则

如果 `DescriptionItem` 必须承载动态资源，它必须作为 owning `Descriptions` 的 scoped resource host：

- `TryGetResource()` 先查询 owning `Descriptions`，再 fallback 到 `Application.Current`。
- owner 变化时先 unsubscribe 旧 owner 的 `ResourcesChanged` / `ActualThemeVariantChanged`，再保存并订阅新 owner。
- owner detach、Items remove、Items reset、template reapply 和控件 detach 都必须能释放 owner 订阅。
- owner 资源和主题变化需要通过 `DescriptionItem` 转发，确保动态资源仍能更新。
- 必须补 WeakReference 测试，模拟 XAML `DynamicResource` anchor，证明 item 离开集合后不会被 `Application.ResourcesChanged` root 住。

AOT 边界：

- 不新增字符串 path binding、反射扫描、`Activator.CreateInstance(Type)` 或动态成员访问。
- API 与 Token 契约由控件文档、源码 public surface、Token 类型或生成数据维护。
- `ItemsSource` 只接受已经构造好的 `DescriptionItem`，不做基于反射的对象属性展开。

### 9.3 Semantic Part 尺寸基线

Descriptions 实现 `ISizeTypeAware`，默认 `SizeType=Large`，只支持 `Large`、`Middle`、`Small`。它不实现
`ICustomizableSizeTypeAware`，也没有 `Custom` 尺寸分支或固定根 `Height` / `MinHeight`。

尺寸映射：

| 模式 | Large | Middle | Small |
| --- | --- | --- | --- |
| 非 bordered 根 Grid | `Spacing` RowSpacing | `SpacingSM` RowSpacing | `SpacingXS` RowSpacing |
| bordered label/content presenter | `ItemPaddingLG` | `ItemPadding` | `ItemPaddingSM` |

所有生成视觉必须从 owner 的同一个 `SizeType` 事实源获得完整尺寸基线。水平边框 cell 已绑定 owner `SizeType`；
`DescriptionDefaultItem` 同样必须绑定 owner `SizeType`，否则纵向 bordered 会停留在内部默认尺寸档，导致 Semantic Padding 与
SizeType 组合建立在错误基线上。

尺寸回归必须分别创建 Large/Middle/Small 的纵向 bordered Descriptions，读取生成 `DescriptionDefaultItem` 的有效
SizeType/Padding，证明三档跟随 owner。水平 bordered 回归还必须读取 target ContentPresenter 的有效 Padding，证明默认值来自
对应尺寸档，Semantic Padding Setter 会覆盖该值而不是与外层 cell padding 叠加。根因修复必须发生在 SizeType 传播和
presenter 基线投影路径，不得通过固定 Height、示例专属 Padding、额外 Margin 或 Gallery 样式掩盖。

label/content 的 Semantic Setter 命中后按 Avalonia 原生优先级覆盖同一目标属性。布局型 Setter 还必须验证 presenter、cell
外框、Grid 列宽与 root Measure/Arrange 的组合结果；这属于跨节点布局协调，不建立第二套 Semantic 优先级。

## 10. 维护不变量

内部重构必须保持以下不变量：

- `Items` 集合订阅必须在集合替换时成对解除和重新订阅。
- `DescriptionItem` 属性订阅必须在 item remove、reset、Items 替换、template reapply 和 detach 时释放。
- item 到 generated control 的映射必须随生成视觉重建清理，不能保留旧视觉控件。
- `MediaBreakPointChanged` 必须在 detach 时解除。
- `IsBordered` 和 `Layout` 变化必须重建生成视觉。
- 水平边框模式的 grid child 数量必须是 `Items.Count * 2`。
- 非水平边框模式的 grid child 数量必须是 `Items.Count`。
- 布局必须按集合位置定位 item，不能按值查找。
- `IsFilled` 和最后 item 必须填满当前行剩余列。
- `IsShowColon` 必须能在生成视觉存在期间重复更新。
- `HeaderLayout` 固定存在，通过 `IsHeaderLayoutVisible` 控制显示。
- `RootFrame` 必须完整投影 owner 的标准表面属性并包住 Header 与内容区；默认属性值必须保持现有未定制布局和视觉不变。
- descriptor 只公开 `root/header/title/extra/label/content`，不能把 frame、grid、cell、冒号或 separator 提升为 Part。
- header/title/extra marker 必须由根模板静态提供；所有生成模式必须为每个 item 提供一个 label 和一个 content marker。
- `DescriptionDefaultItem` 与水平 bordered cell 必须从 owner 绑定同一个 `SizeType`，不能依赖内部默认值。
- 四种布局中的 label/content target 必须保持 `ContentPresenter`；水平 bordered cell 只负责边框几何，Padding、Background、
  Foreground 和文本排版基线必须投影到 target presenter。
- 内部 marker 类型 `DescriptionBorderedItemLabel` 和 `DescriptionBorderedItemContent` 不能作为“空类”随意删除；它们承担主题选择器边界。
- `DescriptionItem` 不能升级成视觉控件，也不能永久持有 generated control、owner container 或 ShowCase。
- 非视觉 `DescriptionItem` 承载动态资源前必须补 scoped resource host 和资源生命周期测试。

## 11. 测试与验证

验证范围：

- `DescriptionsResponsiveLayoutTests`：响应式列数、span 解析、边框切换重建视觉、重复内容 item 按位置布局。
- `DescriptionsSemanticPartTests`：descriptor 字段、静态 marker、四种布局的运行时 label/content marker、空集合、集合增删和布局重建 cardinality。
- Selector 测试：header/title/extra 使用一个 `/template/` 命中；label/content 使用 descriptor 的完整多段 route 命中，并
  拒绝把内部类型、ContractType 或 logical descendant 写进 selector；嵌套 Button 与嵌套 Descriptions 均不得被外层命中。
- 尺寸回归：Large/Middle/Small 在水平 bordered 与纵向 bordered 生成视觉中使用同一 owner SizeType 基线；水平 bordered
  target presenter 获得唯一一层默认 Padding，Semantic Padding/Background/Foreground 覆盖不被 cell 外层基线架空。
- `--verify-descriptions-states`：Header/Extra 显隐、集合生命周期、item 属性变化、布局和边框切换、冒号绑定、媒体断点订阅释放。
- 生命周期测试：item remove/reset/Items 替换/template reapply/detach 后，旧 `DescriptionItem`、旧 generated control、binding expression 和资源订阅不被保留。
- 如果允许 `DescriptionItem` 使用 `DynamicResource`，必须增加 WeakReference 测试、owner resource 优先级测试和 resource update 测试。
- Gallery Descriptions 示例：Basic、Border、Custom Size、Responsive、Vertical、Vertical Border、Row。
- Gallery Semantic Part 示例：同数据、同标题和同 bordered 配置下纵向展示 Small 与默认 Large 两个实例；两者的 root 均使用
  `Padding=10`，Small label 为黑色，默认 Large 额外使用 `#CDC1FF` 一像素 root 边框、`8` 圆角和 `#A294F9` label。
- 修改 Token 或主题时验证 源码片段和 Gallery 示例视觉。
- 修改控件实现时运行 `tests/AtomUI.Desktop.Controls.Tests` 中的 Descriptions 相关测试，并按影响范围扩大到完整 Desktop 控件测试。
- 文档改动运行 `git diff --check`，并检查相对链接存在。
