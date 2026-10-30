# Masonry Semantic Part 契约

本文档定义 `AtomUI.Desktop.Controls.Masonry` 对应用公开的 Semantic Part、Selector、类型约束、数量语义和定制边界。
Masonry 的整体设计见 [Masonry 桌面版架构设计](overview.md)，descriptor 与真实容器生命周期见
[Masonry 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Owner 边界

Masonry 是单一 owner：`Masonry` 公开 `root` 和 `item` 两个 Part，与 Ant Design 6.6.0 稳定版 Masonry 的公开
Semantic DOM 对齐。上游 Masonry 的 API 同时暴露 `classNames` 与 `styles`，其 Semantic DOM 表只包含：

- `root`：组件根元素。
- `item`：每个瀑布流条目元素。

AtomUI 不扩展 `container`、`panel`、`column`、`gap`、`card`、`image`、`theme` 或 loading 状态 Part。`MasonryPanel`、
`ItemsPresenter`、用户 `ItemTemplate` 生成的子树、Card / Image / Skeleton 等嵌套控件各自拥有独立职责，不属于 Masonry
的公共 Semantic Part。

AtomUI 映射：

- `root` → `Masonry` owner 本身，作为布局属性、ItemsControl 公共入口和 item Selector 作用域。
- `item` → 每个已准备好的 item container。直接子元素模式下，用户放入的直接 `Control` 是 item target；`ItemsSource`
  模式下，Avalonia `ItemsControl` 生成的 `ContentPresenter` 是 item target。Masonry 不新增包装层。

`root` 是隐式 Part，不添加 `.semantic-root`。`item` marker `.semantic-item` 在 `PrepareContainerForItemOverride` 中幂等补齐，
只表示 Masonry item 身份，不改变 item 的数据、内容、模板、资源作用域或布局算法。

## 2. Semantic Parts

### 2.1 `root`

| 字段 | 值 |
| --- | --- |
| Owner | `Masonry` |
| Part | `root` |
| Selector | Masonry 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `Masonry` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| ThemePropertyName | 不适用 |
| AtomUI 节点 | Masonry owner |
| 职责 | 瀑布流布局根区域，承载列数、间距、响应式、root chrome、ItemsControl 输入和 item Selector 作用域。 |
| 相关 API | `Items`、`ItemsSource`、`ItemTemplate`、`ItemContainerTheme`、`ItemsPanel`、`ColumnCount`、`ColumnInfo`、`MinColumnWidth`、`MaxColumnCount`、`ColumnGap`、`RowGap`、`Gutter`、`Background`、`BorderBrush`、`BorderThickness`、`CornerRadius`、`Padding`、`LayoutChanged` |
| 相关 Token | 无专属 Token；间距和列宽是实例布局状态。 |
| 稳定性 | stable since 6.2.0 |

### 2.2 `item`

| 字段 | 值 |
| --- | --- |
| Owner | `Masonry` |
| Part | `item` |
| Selector | `.semantic-item` |
| SelectorRoute | `> .semantic-item` |
| Style Type | `MasonryItemStyle` |
| ContractType | `Control` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| ThemePropertyName | 不适用 |
| AtomUI 节点 | 每个 item container：直接子元素或 generated `ContentPresenter`。 |
| 职责 | 表示一个被 Masonry 测量、分配列并排列的条目容器。 |
| 相关 API | `Items`、`ItemsSource`、`ItemTemplate`、`ItemContainerTheme`、`Masonry.Column`、`Masonry.Span` |
| 相关 Token | 无专属 Token；item 外观由 item 自身控件或模板负责。 |
| 稳定性 | stable since 6.2.0 |

`ContractType=Control` 是有意选择：Masonry 同时支持用户直接提供任意 `Control` 作为条目，以及 `ItemsSource` 场景下由
Avalonia 生成 `ContentPresenter`。因此 `MasonryItemStyle` 只能稳定依赖 `Control` 共有属性，例如 `Margin`、`Opacity`、
`Width`、`Height`、`MinHeight`、`MaxHeight`、`HorizontalAlignment`、`VerticalAlignment`、`RenderTransform`、
`RenderTransformOrigin` 和 `IsVisible`。需要设置 `Background`、`BorderBrush`、`CornerRadius` 或 Card 专属属性时，应继续定制
item 自身控件或 `ItemTemplate` 内部控件，而不是扩大 Masonry item 的 `ContractType`。

## 3. Part 说明

### 3.1 root

`root` 是 Masonry 控件实例本身，每个 Masonry 恰好一个，在控件生命周期内始终存在。

它负责：

- 承载瀑布流布局 public API、响应式列数与间距 fallback。
- 承载默认主题中的 root chrome `PixelAlignedBorder`，通过继承自 `ItemsControl` 的 `Background`、`BorderBrush`、`BorderThickness`、`CornerRadius` 和 `Padding` 提供根视觉边界。
- 承载 `ItemsControl` 的数据输入、模板、容器主题和 `ItemsPanel` 替换入口。
- 作为 `item` owner-scoped Selector 的作用域边界。
- 通过 `LayoutChanged` 暴露有效列分配变化。

root 适合定制 Masonry 实例的布局参数、root chrome、外部尺寸、Margin、Alignment、可见性和整体透明度。列数、间距与 root chrome 应通过 Masonry public API 或 root style setter 设置，而不是通过 item Selector 反推布局算法。

root 不表示 `ItemsPresenter#PART_ItemsPresenter`、internal `MasonryPanel`、某个列容器、滚动宿主或 Gallery 页面中的卡片
包装。替换 `ItemsPanel` 后，root 与 item 语义仍存在，但 Masonry-specific shortest-column 布局语义由调用方替换的面板负责。

### 3.2 item

`item` 表示 Masonry 直接测量和排列的 item container。cardinality 为 `Multiple`，数量等于当前已实例化并已准备的 item
container 数量。

它负责：

- 表示参与 `MasonryPanel.Children` 布局计算的一个条目容器。
- 承载 `Masonry.Column` 与 `Masonry.Span` attached property 的有效读取位置。
- 允许应用在 owner 作用域内统一设置条目级通用 Control 属性。
- 在直接子元素和 `ItemsSource` 两种模式下保持同一个 Part 身份。

直接子元素模式下，`item` marker 加在用户放入的直接 `Control` 上；`ItemsSource` 模式下，marker 加在 generated
`ContentPresenter` 上。Masonry 不向 item 外层插入额外 Border、Panel 或 presenter。唯一的例外是离场动效：item 从集合移除后，
容器会被临时移入模板中的 `PART_MotionGhostLayer`，由控件创建的瞬态 ghost host `Border` 承载并在原位置淡出；淡出期间
item 保留 `.semantic-item` marker（对齐 antd CSSMotion leave 期间节点仍在 DOM），淡出结束或容器重加入集合后立即归还，
该 ghost host 是瞬态承载节点，不属于 Masonry 的语义部件，不得作为定制目标。

`item` 适合定制 `Margin`、`Opacity`、`RenderTransform`、对齐、尺寸约束和可见性。布局型 Setter 会参与 Masonry 的下一次测量
与列分配：例如改变 `Height`、`MinHeight`、`Margin` 或 `IsVisible` 可能改变 shortest-column 结果，这是预期的 Avalonia 布局
行为，不是 Semantic Style 优先级问题。

`item` 不公开以下内容：

- `ItemTemplate` 创建的用户子树。
- Card、Image、Skeleton、TextBlock 等嵌套控件的内部 Semantic Part。
- `MasonryPanel`、列高度数组、布局缓存、测量矩形和 `LayoutChanged` 快照。
- `Masonry.Column` / `Masonry.Span` 的计算结果或列索引可视化节点。

## 4. Selector 用法

应用级样式先限定 Masonry owner，再通过生成的 Semantic Style 进入 Part。生成类型封装 owner-relative route，用户不需要复制
`> .semantic-item`：

```xml
<Application.Styles>
    <Style Selector="atom|Masonry.semantic-demo">
        <Setter Property="Background" Value="#FAFAFA" />
        <Setter Property="BorderBrush" Value="#D9D9D9" />
        <Setter Property="BorderThickness" Value="1" />
        <Setter Property="CornerRadius" Value="12" />
        <Setter Property="Padding" Value="16" />
        <Setter Property="ColumnGap" Value="20" />
        <Setter Property="RowGap" Value="20" />
        <atom:MasonryItemStyle x:SetterTargetType="Control">
            <Setter Property="Opacity" Value="0.9" />
            <Setter Property="RenderTransformOrigin" Value="50%,50%" />
        </atom:MasonryItemStyle>
    </Style>
</Application.Styles>
```

对特定 Masonry class 或状态定制时，把 class、属性或伪类放在 owner 一侧：

```xml
<Style Selector="atom|Masonry.semantic-demo[ColumnCount=4]">
    <atom:MasonryItemStyle x:SetterTargetType="Control">
        <Setter Property="Margin" Value="0" />
    </atom:MasonryItemStyle>
</Style>
```

当 item 本身是 `Border` 这类具备边框 chrome 的直接子控件时，`MasonryItemStyle` 可以直接命中该 `Border`；`ItemsSource` 生成的 `ContentPresenter` 仍只适合使用通用 `Control` 属性。

不得把 `ContractType` 写入 Part Selector。以下写法不属于公共契约：

- `Control.semantic-item`、`ContentPresenter.semantic-item` 或 `atom|Card.semantic-item`。
- 直接复制 `> .semantic-item` route 作为用户主路径。
- 依赖 `PART_ItemsPresenter`、`MasonryPanel`、visual parent 层级或 item container 的具体 CLR 类型。
- 通过 Masonry item style 穿透到 `ItemTemplate` 内部控件。

## 5. 状态与数量语义

| 场景 | root | item | 说明 |
| --- | --- | --- | --- |
| 空集合 | 1 | 0 | 没有 item container。 |
| 直接子元素 N 个 | 1 | N | 每个直接 `Control` 一个 `.semantic-item` marker。 |
| `ItemsSource` N 条 | 1 | N | 每个 generated `ContentPresenter` 一个 `.semantic-item` marker。 |
| 集合追加 / 删除 | 1 | 随已实例化容器数变化 | marker 随容器生命周期创建和释放。 |
| `Masonry.Span=Full` | 1 | 不变 | 整行项只改变布局矩形，不改变 Part 身份。 |
| `Masonry.Column` 改变 | 1 | 不变 | 显式列只改变列分配，不增删 marker。 |
| item `IsVisible=false` | 1 | 不变 | container 仍有 marker，但不参与布局；Preview 高亮会按可见性过滤目标。 |
| item 移除（离场淡出期） | 1 | 数量随已释放容器变化 | 被移除容器进入 ghost 层原位淡出（`LeaveMotionDuration`，默认 100ms），淡出期间保留 marker；淡出完成或重加入即释放。禁用动效（时长为 0）时立即可见移除。 |
| 替换 `ItemsPanel` | 1 | 不变 | item identity 仍由 Masonry 准备；shortest-column 布局由替换后的面板决定。 |

Masonry 当前不引入虚拟化或容器回收语义；未来如引入虚拟化，`item` 数量应改为“已实例化容器数量”，并同步更新本文档、实现与测试。

## 6. 尺寸、间距与布局基线

Masonry 没有 SizeType 档位。布局基线由以下属性决定：

- `ColumnCount` / `ColumnInfo`：有效列数。
- `MinColumnWidth` / `MaxColumnCount`：自适应列数 fallback。
- `ColumnGap` / `RowGap` / `Gutter`：水平和垂直间距。
- item `DesiredSize`、`Margin`、`IsVisible`、`Masonry.Column`、`Masonry.Span`：条目测量和列分配输入。

Semantic `item` style 不参与列数与间距解析。需要改变列数或间距时应设置 root public API；需要改变条目测量尺寸时，才使用
`MasonryItemStyle` 或 item 自身模板。

## 7. 定制边界

以下区域明确不属于 Masonry Semantic Part：

- internal `MasonryPanel`、列高度数组、布局缓存和算法中间状态。
- `ItemsPresenter#PART_ItemsPresenter` 与替换 `ItemsPanel` 的具体实现节点。
- 离场动效的 `PART_MotionGhostLayer` 与瞬态 ghost host `Border`（由控件创建，仅承载淡出，不属于语义部件）。
- `ItemTemplate` 生成的子树和用户直接放入 item 内部的子控件。
- Card、Image、Skeleton、Button 等嵌套控件的 Semantic Part。
- Gallery ShowCase 的图片加载 skeleton、动态删除按钮、说明文本和示例外层容器。

删除或重命名 `item`、修改 `.semantic-item`、收窄 `ContractType`、改变 `item` cardinality 语义，或让已准备 item container 缺少
marker，均属于公共主题契约变更。

## 8. 验证要求

验证至少覆盖：

- descriptor 只公开 `root`、`item`，字段值与本文一致。
- `MasonryTheme.axaml` 不声明静态 semantic marker；`item` 只来自容器准备路径。
- 直接子元素和 `ItemsSource` 两种模式均产生一一对应的 `.semantic-item` marker，且不引入额外包装层。
- `MasonryItemStyle` 通过 owner-scoped route 命中真实 item container，且不穿透嵌套控件模板。
- `Masonry.Column`、`Masonry.Span`、`IsVisible`、集合增删、替换 `ItemsPanel` 和响应式断点变化不破坏 marker 生命周期。
- item 动效：入场淡入与位置滑动以 Animation 优先级临时驱动 `Opacity` / `RenderTransform`，完成后释放并回落用户样式基值；离场淡出期间容器由 ghost 层托管并保留 marker，释放后回归基值。对 item 应用 `MasonryItemStyle` 的 `Opacity` / `RenderTransform` Setter 时，动效结束后必须呈现 Setter 值。
- 默认布局、响应式、动态 item、Gallery Semantic Preview 保持一致。
- Gallery Semantic Preview 不要求 Masonry 关闭 `ClipToBounds`，也不改变 root border、item 容器或布局祖先的 `Clip`。高亮由
  GalleryBase 的统一 `SemanticPartAdorner` 处理：其 ancestor clipping 关闭，外扩 marker 在自身 Bounds 内绘制，因此 Masonry
  的 root、item 和薄尺寸条目都必须使用通用 Preview 规则验证四边描边完整可见。
