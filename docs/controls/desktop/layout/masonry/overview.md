# Masonry 桌面版架构设计

本文档定义 `AtomUI.Desktop.Controls.Masonry` 桌面版的最新设计定位、公共契约、布局状态模型、主题边界和兼容要求。通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，公共语义区域见 [Masonry Semantic Part 契约](semantic-part.md)，内部实现原理见 [Masonry 桌面版实现原理](implementation.md)，设计和契约变化记录见 [Masonry Changelog](changelog.md)。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/Layout/Masonry` |
| 控件状态 | Stable |

Masonry 是桌面端瀑布流布局控件，用于将高度不一致的内容块按列组织。它支持稳定列归属和每轮 shortest-column 重排两种策略，适合图片墙、卡片流、示例集合、资源列表等内容高度不可预先统一的场景。

`Masonry` 派生自 Avalonia `ItemsControl`，同时承担数据绑定入口和子项布局元数据 attached property 容器（`atom:Masonry.Column`、`atom:Masonry.Span`）两种职责。

Masonry 是布局控件，不是数据源管理器、卡片控件、图片加载器、虚拟化列表、滚动容器或内容渲染器。它只负责对子元素进行测量、列分配和排列，不拥有子元素内容，也不改变子元素的业务语义。

Masonry 支持两种内容提供方式：

- 直接放置子元素：在 `atom:Masonry` 的内容中放置任意 `Control`，子元素本身即作为容器，不产生 Masonry 主动插入的视觉包装层。
- 数据绑定：通过 `ItemsSource` 绑定数据集合，由 `ItemsControl` 固有机制生成容器。集合监听、容器生成和增删同步由基类承担。

滚动由外层 `ScrollViewer` 承担，图片加载由图片控件或外部加载库承担。

## 2. 设计语言

Masonry 表达的是“密集但有秩序”的内容浏览体验。它利用可变高度卡片形成自然节奏，避免为了对齐网格而牺牲内容本身的信息密度。

设计语言要求：

- 首次自动分配以当前最短列降低初始列高差；后续由 `LayoutStrategy` 在视觉连续性和实时列高平衡之间取舍。
- 逻辑上保留子元素顺序，保证读屏、键盘导航、数据绑定容器生成顺序和源集合顺序一致。
- 不为子项强加卡片外观。子项可以是 Card、Image、ShowCaseItem 或任意 `Control`。
- 不插入额外视觉包装层，使 VisualTree 保持最小。
- 间距只表达布局节奏，不表达组件语义颜色、阴影或卡片样式。

## 3. API 与契约模型

Masonry 的公共 API 采用 Avalonia 原生属性模型。需要 XAML 设置、Style 设置、绑定、动画或主题参与的状态必须定义为 `StyledProperty` 或 attached property。

核心布局属性：

| API | 类型 | 默认值 | 语义 |
| --- | --- | --- | --- |
| `ColumnCount` | `int` | `0` | 固定列数；`<= 0` 时进入容器自适应列数。 |
| `ColumnInfo` | `ResponsiveInt?` | `null` | 按媒体断点变化的列数。 |
| `MinColumnWidth` | `double` | `320d` | 自适应列数下的最小列宽。 |
| `MaxColumnCount` | `int` | `4` | 自适应列数下的最大列数。 |
| `ColumnGap` | `double` | `16d` | 水平列间距。 |
| `RowGap` | `double` | `16d` | 垂直行间距。 |
| `Gutter` | `ResponsiveGutter?` | `null` | 按媒体断点变化的水平/垂直间距。 |
| `LayoutStrategy` | `MasonryLayoutStrategy` | `StableColumns` | 控制自动子项在后续布局中保持原列，或每次重新按最短列分配。 |

响应式属性遵循 [AtomUI 响应式机制设计](../../../../architecture/systems/control-infrastructure/responsive.md)。Masonry 不复制断点定义、解析顺序或 partial map 继承规则；它只在 effective state 层提供列数和间距 fallback。

列数优先级：

1. `ColumnInfo` 在当前断点命中显式配置时，使用响应式列数。
2. `ColumnInfo` 未命中且 `ColumnCount > 0` 时，使用固定列数。
3. 前两者均未命中时，使用 `MinColumnWidth + MaxColumnCount + AvailableWidth` 的容器自适应列数。

间距优先级：

1. `Gutter` 在当前断点命中显式配置时，使用响应式水平/垂直间距。
2. `Gutter` 未命中时，使用 `ColumnGap` 与 `RowGap`。

子项布局元数据：

| API | 类型 | 语义 |
| --- | --- | --- |
| `Masonry.Column` | `int?` attached property | 指定子项所在列，列索引按 `0` 起始。 |
| `Masonry.Span` | `MasonryItemSpan` attached property | `Auto` 普通瀑布流项；`Full` 整行项。 |

attached property 的消费对象是 `MasonryPanel.Children` 中被测量和排列的 item container。直接放置子元素时写在子元素本身；`ItemsSource` 场景应写在 generated container 上，例如通过 `ItemContainerTheme` 绑定。

布局策略：

| 场景 | `StableColumns`（默认） | `Reflow` |
| --- | --- | --- |
| 首次成功 Arrange | 自动子项按当前最短列分配；Arrange 完成后，该结果成为后续布局保持的列归属。 | 自动子项按当前最短列分配，不保留跨布局列归属。 |
| 有效列数不变的 resize | 已提交的 item container 保持原列；重新测量宽高并按列内顺序更新纵向位置。 | 按本轮测得高度从头执行 shortest-column 分配，部分或全部自动子项可能换列。 |
| 子项内容或 DesiredSize 变化 | 已提交 container 保持原列；只更新尺寸、列高和后续子项纵向位置。 | 从头重新计算自动列分配。 |
| 新增 item container | 已存在 container 保持原列；新 container 进入当前最短列。 | 包含新增项在内从头重新计算。 |
| 删除 item container | 其余现存 container 保持原列；删除项不再参与后续布局。 | 对剩余自动子项从头重新计算。 |
| 有效列数变化 | 既有列归属不再适用，全部自动子项按新列数重新计算。 | 按新列数从头重新计算。 |
| `Masonry.Column` / `Masonry.Span` 变化 | attached property 继续优先于自动策略，并重新建立全部自动列归属。 | attached property 继续优先于自动策略，本轮按新规则重新计算。 |
| 策略切换 | 放弃既有稳定列归属；切换后的下一次布局按目标策略执行。 | 放弃既有稳定列归属；后续不再复用列归属。 |

`StableColumns` 的“首次”指 item container 第一次完成 Arrange，而不是图片、网络内容或其他异步内容全部加载完成。若占位内容与最终内容高度不同，首次 Arrange 时的高度决定初始列归属；后续加载只改变该列中的尺寸和纵向位置。需要始终追求当前列高平衡的场景应显式选择 `Reflow`，图片墙希望初始分配也接近最终高度时应提供稳定的宽高比或等价尺寸约束。

默认策略无需声明：

```xml
<atom:Masonry />
```

经典 shortest-column 重排需要显式选择：

```xml
<atom:Masonry LayoutStrategy="Reflow" />
```

Semantic Parts 摘要：Masonry 公开 `root` 与 `item` 两个职责区域。`root` 是 Masonry owner 本身；`item` 是每个已准备的 item container，直接子元素模式下是用户提供的直接 `Control`，`ItemsSource` 模式下是 generated `ContentPresenter`。完整 Selector、ContractType、cardinality 和定制边界由 [Masonry Semantic Part 契约](semantic-part.md) 维护。

事件模型：

```csharp
public event EventHandler<MasonryLayoutChangedEventArgs>? LayoutChanged;
```

`LayoutChanged` 是有效布局分配通知，不是像素级滚动或动画事件。它只表达 item container 顺序、有效列和整行状态的变化。`StableColumns` 下仅有尺寸或纵向位置变化时不派发；`Reflow` 也只有最终有效列分配实际变化时才派发。

## 4. 行为与状态模型

Masonry 本身没有 hover、pressed、disabled、loading 或 checked 等交互状态。它必须完整保留子元素自身的命中测试、焦点、键盘导航、拖拽、上下文菜单和动画行为。Masonry 自身为 item 提供布局动效：新 item 入场淡入、既有 item 位置变化时滑动、移除的 item 在原位置淡出（时长来自 `motionDurationSlow` / `motionDurationFast` token，全局禁用动效时退化为瞬时；机制见 [Masonry 桌面版实现原理](implementation.md)），item 自身的交互动画不受影响，动效结束后 item 的样式基值自动接管。

有效状态由响应式属性、兼容属性、可用宽度和子项 attached property 共同决定。状态归一发生在 C# 布局层，AXAML 不承担列数、间距或子项位置计算。

布局状态包括：

- 有效列数。
- 有效列宽。
- 有效水平和垂直间距。
- 子项显式列。
- 子项是否整行。
- 最终有效列分配。
- 自动列分配策略，以及 `StableColumns` 下按 item container 实例维护的已提交列归属。

`Tab`、读屏顺序、logical children 顺序和 item container 顺序必须保持 `ItemsControl` 源集合顺序。两种列分配策略都只改变视觉位置，不重排 logical order，也不改变数据绑定容器生成顺序。

## 5. 视觉与主题模型

Masonry 的默认主题装配 root chrome `PixelAlignedBorder`、`ItemsPresenter`、离场 ghost 层与 internal `MasonryPanel`，并把 Masonry 的布局属性与 root chrome 属性分别传递给对应层。Theme 不绘制子项外观，不通过 selector 计算列数和位置。Semantic Part marker 不写入主题静态节点；`.semantic-item` 由 Masonry 在 item container 准备阶段补齐。

默认视觉树：

```text
Masonry (ItemsControl, default ItemsPanel = MasonryPanel)
└─ PixelAlignedBorder#PART_RootBorder
   └─ Grid
      ├─ ItemsPresenter
      │  └─ MasonryPanel (internal)
      │     ├─ <子元素>
      │     ├─ ContentPresenter → <子元素>
      │     └─ ...
      └─ Canvas#PART_MotionGhostLayer (离场淡出托管，不参与命中测试)
```

视觉树要求：

- `Masonry` 和 `MasonryPanel` 节点只承担布局与容器装配职责；root chrome 由 `PixelAlignedBorder` 承载。
- 子项视觉结构完全由子项控件或 `ItemTemplate` 负责。
- 不通过模板节点实现列、行、占位或测量辅助对象。
- 不通过透明 Border 扩展命中区域。
- 不通过不可见控件缓存测量结果。
- 不为子项主动插入额外视觉包装层。

Masonry 当前不定义专属 Token，不需要创建 `token.md`。Masonry 的间距和列宽属于实例布局状态，不应迁移为控件 Token。root chrome 仅复用 `ItemsControl` 已有的 `Background`、`BorderBrush`、`BorderThickness`、`CornerRadius` 与 `Padding`，不引入独立 token 边界。item 动效时长复用全局 motion token（`MotionDurationSlow` / `MotionDurationFast`），由 ControlTheme Setter 馈入 internal 属性，同样不属于 Masonry 专属 Token。

## 6. 控件家族或集成关系

Masonry 属于 Layout 分类，与 `FlexPanel`、`Row`、`Space` 等布局能力共同构成 AtomUI 的桌面布局基础。

集成关系：

- `ItemsControl`：承载 `Items`、`ItemsSource`、`ItemTemplate`、`ItemContainerTheme` 和 `ItemsPanel`。
- 响应式系统：通过共享响应式值表达断点列数和断点间距。
- Gallery：承载图片墙、卡片流、响应式示例和可变高度内容。
- 外层 `ScrollViewer`：承担滚动语义。

Masonry 不提供虚拟化语义。瀑布流虚拟化涉及滚动偏移、容器回收、动态高度预测和键盘导航补偿，不能在 Masonry 中隐式开启。

## 7. 兼容性不变量

优化或扩展 Masonry 时必须保持以下不变量：

- 不改变子元素 logical order、visual child order 和 ItemsControl 容器生成顺序。
- 除了发布契约要求的 `.semantic-item` marker，不修改子元素 `DataContext`、内容、样式类、主题或资源作用域。
- 不为子项主动插入额外视觉包装层。
- 不要求用户为子项提供 key。
- 不把 Masonry 变成 ScrollViewer、数据源管理器或卡片外观组件。
- 不隐式引入虚拟化、延迟生成或容器回收语义。
- 不通过 AXAML selector 承担列数和位置计算。
- 不改变 `ColumnInfo`、`ColumnCount` 与容器自适应列数之间的优先级。
- 不改变 `Gutter` 与 `ColumnGap` / `RowGap` 之间的优先级。
- 不在 `Gutter` 未声明垂直维度时把有效垂直间距隐式改为 `0`。
- `LayoutStrategy` 的默认值保持 `StableColumns`；经典 shortest-column 重排必须通过 `Reflow` 显式选择。
- `StableColumns` 在有效列数不变时按 item container 引用保持已提交列归属，不得退化为按索引或数据项值关联。
- 不把 `Masonry.Column`、`Masonry.Span` 的读取对象从 item container 隐式改为 `ItemTemplate` 内部元素。
- 不破坏继承自 `ItemsControl` 的 `ItemsPanel` 公共契约。
- `MasonryPanel` 保持 `internal`，仅作为 `Masonry` 的默认 `ItemsPanel` 装配。

如果实现某项能力时无法保持这些不变量，应先停止实现，说明原因、影响范围、替代方案和迁移方式，并获得授权。

## 8. 专项模型

### 8.1 响应式模型

`ColumnInfo` 对齐 参考 Masonry `columns` 语义，`Gutter` 对齐 参考 Masonry `gutter` 语义。两者只在当前断点命中显式配置时覆盖兼容属性，未命中时继续使用控件自身 fallback。

### 8.2 整行项模型

`Masonry.Span=Full` 表示子项占据整行，适用于标题、说明块、分组分隔、宽幅图或需要打断瀑布流节奏的内容。整行项不改变前后子项的 logical order。

### 8.3 子项容器元数据模型

Masonry 的布局元数据属于 item container，而不是数据对象或模板内部视觉元素。该规则保证直接子元素和 `ItemsSource` 两种模式都通过同一套 `MasonryPanel.Children` 计算路径布局。

## 9. 文档导航与验证策略

关联文档：

- [Masonry Semantic Part 契约](semantic-part.md)
- [Masonry 桌面版实现原理](implementation.md)
- [Masonry Changelog](changelog.md)
- [AtomUI 响应式机制设计](../../../../architecture/systems/control-infrastructure/responsive.md)

Token 说明：

- Masonry 当前没有专属 `token.md`；Token 边界按第 5 节视觉与主题模型、SharedToken、控件家族 Token 或主题资源说明 Token 边界。

验证策略：

| 改动类型 | 验证要求 |
| --- | --- |
| 文档改动 | `git diff --check`。 |
| C# 布局改动 | 覆盖固定列数、自适应列数、响应式列数、间距、整行项、显式列、不可见子项、无限宽度和 `.semantic-item` marker 生命周期。 |
| ItemsControl 集成 | 验证直接子元素、`ItemsSource`、`ItemContainerTheme`、`ItemTemplate`、`MasonryItemStyle` 和替换 `ItemsPanel`。 |
| 事件改动 | 验证 `LayoutChanged` 派发避开 layout pass，且只在有效布局分配变化时触发。 |
| Gallery 改动 | 走查图片、异步内容、高度变化、响应式和整行项示例。 |
