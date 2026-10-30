# ListView Semantic Part 契约

本文档定义 `ListView` 对应用公开的 Semantic Part、Selector、类型约束、数量语义、分割线基线和定制边界。ListView 的整体设计见
[ListView 桌面版架构设计](overview.md)，descriptor 与真实模板节点映射见 [ListView 桌面版实现原理](implementation.md)，
系统级规则见 [AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Semantic Parts

ListView 公开 `root`、`item` 与 `groupHeader` 三个职责区域，与上游稳定 Semantic DOM（`Listy` 组件的
`classNames` / `styles` 均为 `{ root?, item?, groupHeader? }`）对齐。`root` 由生成器为带非 root Part 的 owner 隐式加入，
不生成 Style；`item` 与 `groupHeader` 是运行时由 ListView 创建的容器 Part。

#### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `ListView` |
| Part | `root` |
| Selector | ListView 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `ListView` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | ListView owner |
| 职责 | 根语义区域，即滚动容器，承载字体、行高、相对定位、外框与外框闭合边界；对应上游 `.ant-listy`。 |
| 相关 API | `ItemsSource`、`ItemTemplate`、`Height`、`SizeType`、`IsBorderless`、`IsGroupEnabled`、`GroupPropertySelector`、`GroupItemTemplate` |
| 相关 Token | `ListViewToken`、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `item`

| 字段 | 值 |
| --- | --- |
| Owner | `ListView` |
| Part | `item` |
| Selector | `.semantic-item` |
| SelectorRoute | `> .semantic-item` |
| Style Type | `ListViewItemStyle` |
| ContractType | `ListViewItem` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | 每个非分组 `ListViewItem` 容器 |
| 职责 | 条目元素，设置内间距、底部分割线与悬浮背景；对应上游 `.ant-listy-item`。 |
| 相关 API | `SizeType`、`ItemHoverBg`、`ItemSelectedBg`、`ItemClickMode` |
| 相关 Token | `ItemPaddingSM`、`ItemPadding`、`ItemPaddingLG`、`ItemHoverBgColor`、`ColorSplit`、`ControlItemBgHover` |
| 稳定性 | stable since 6.2.0 |

#### `groupHeader`

| 字段 | 值 |
| --- | --- |
| Owner | `ListView` |
| Part | `groupHeader` |
| Selector | `.semantic-group-header` |
| SelectorRoute | `> .semantic-group-header` |
| Style Type | `ListViewGroupHeaderStyle` |
| ContractType | `ListViewItem` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | 每个分组标题容器（专用 `GroupHeaderItem`） |
| 职责 | 分组标题元素，设置标题文字与背景；对应上游 `.ant-listy-group-header`。 |
| 相关 API | `IsGroupEnabled`、`GroupPropertySelector`、`GroupItemTemplate` |
| 相关 Token | `GroupHeaderColor`、`ColorBgContainer`、`ColorFillAlter`、`FontWeightStrong` |
| 稳定性 | stable since 6.2.0 |

`root` 是隐式 Part，不声明 `.semantic-root` marker。`item` 与 `groupHeader` 的 marker 在容器创建路径一次性建立，
`PrepareContainerForItemOverride` 按容器类型幂等补齐；容器角色由类型决定（分组标题使用专用 `GroupHeaderItem`），
prepare、restore、recycle 与分组开关都不切换 marker。

`ContractType` 只定义 Setter 可以稳定依赖的最低 public 类型，并通过 `x:SetterTargetType` 提供 AXAML 编译期类型
上下文；它不参与 `.semantic-*` 的身份匹配。`groupHeader` 的 `ContractType` 是 `ListViewItem`（`GroupHeaderItem` 的
public 基类），因为 `GroupHeaderItem` 是 internal 类型，不能作为公共 Setter 依赖的最低类型。

## 2. Part 说明

### 2.1 root

`root` 是 ListView owner 本身，在 ListView 实例的整个生命周期内始终存在，并且每个 ListView 恰好一个。

它负责：

- 承载 `ItemsSource`、`SizeType`、`IsBorderless`、分组、分页、空状态与操作态等公共状态和 API。
- 承载 `:empty`、`:singleitem` 等伪类。
- 提供最终 `Background`、`BorderBrush`、`BorderThickness`、`CornerRadius`、`Padding`、`Margin` 和字体等根视觉属性。
- 作为 `item`、`groupHeader` owner-scoped Selector 的作用域边界。

root 表面投影到模板中的 `Frame` / `ScrollViewer`：root 外框默认使用 `ColorSplit`（与条目分割线同色），表达“外框即
列表闭合线”；root `Frame` 开启 `ClipContentToCornerRadius`，条目 hover / selected 背景等溢出圆角内边缘的内容被
裁剪，不在圆角口袋区溢出。

适合通过 root 定制 ListView 整体背景、外框颜色与宽度、圆角、padding、字体和对齐状态。需要根据状态改变根样式时，
应在 owner Selector 上组合公开属性或伪类。

root 不表示模板中的 `PixelAlignedBorder#Frame`、`Spin`、`PART_ScrollViewer` 或任一 presenter；这些节点的名称、数量、
层级与内容裁剪实现值不属于 root 契约。

### 2.2 item

`item` 表示一个非分组数据条目容器，每个 `ListViewItem` 恰好一个 marker，cardinality 为 `Multiple`。marker
`.semantic-item` 在 `ListViewItem` 创建路径一次性添加，prepare 幂等补齐；虚拟化回收、分组开关与状态切换不增删 marker。

它负责：

- 设置条目内间距（`ItemPaddingSM` / `ItemPadding` / `ItemPaddingLG`，仅水平 padding）。
- 设置条目底部分割线（默认 1 DIP `ColorSplit` 底边线）。
- 设置 hover / selected 背景（`ItemHoverBg` / `ItemSelectedBg`）与前景状态。
- 承载 `Content` / `ContentTemplate` 的最终展示。

分割线由模板中的 `SplitLineFrame` 节点承载，绑定容器的 `EffectiveBorderThickness` 与 `IsSplitLineEffectiveVisible`，
因此 Semantic Style 对 `ListViewItem.BorderThickness`（底部值）与 `BorderBrush` 的覆盖直接作用到分割线。分割线可见性
由 ListView 状态机决定：无 `BottomPagination` 时最后一项的底部分割线被抑制，由 root 外框下边缘承担闭合线；
配置 `BottomPagination` 时最后一项保留分割线（分隔条目区与分页器）；`IsBorderless=true` 时也保留（外框消失后由
分割线承担闭合线）。Semantic Style 不参与抑制判定，也不能绕过最后一项抑制。

适合定制 `Background`、`BorderBrush`、`BorderThickness`、`Padding`、`Foreground`、`FontSize`、`MinHeight`、
`Opacity` 等属性。条目表面保持直角：默认主题不设置条目 `CornerRadius`，不建议把条目绘制成圆角卡片。

item 不公开模板中的 `SelectedIndicator`、`ContentPresenter` 节点，也不公开 `ItemTemplate` 生成的用户子树。

### 2.3 groupHeader

`groupHeader` 表示一个分组标题容器。分组标题使用专用容器类型 `GroupHeaderItem`（internal，继承 `ListViewItem`，
`IsGroupItem` 恒为 true），marker `.semantic-group-header` 在容器创建路径一次性建立，cardinality 为 `Multiple`。

它负责：

- 展示组标题文字（`GroupHeaderColor`、`FontWeightStrong`）。
- 提供组标题背景与内间距（`GroupHeaderPadding`）。
- 在分组开关、排序、过滤与分页组合下保持同一 Part 身份。

组标题不应用普通条目的 hover / selected 背景，也不显示底部分割线（`IsGroupItem=true` 时分割线状态机关闭）。

适合定制 `Foreground`、`Background`、`Padding`、`FontWeight`、`FontSize`、`Opacity` 等属性。groupHeader 不公开
`GroupItemTemplate` 生成的用户子树，也不公开 `GroupHeaderItem` 具体类型（internal）与组标题绘制细节。

## 3. Selector 用法

应用级样式先限定 ListView owner，再通过生成的 Semantic Style 进入 Part。生成类型已经封装 owner 类型保护和
`SelectorRoute`，用户不需要复制模板路径：

```xml
<Application.Styles>
    <Style Selector="atom|ListView">
        <atom:ListViewItemStyle x:SetterTargetType="atom:ListViewItem">
            <Setter Property="Background" Value="#FAFAFA" />
            <Setter Property="BorderThickness" Value="0,0,0,2" />
        </atom:ListViewItemStyle>

        <atom:ListViewGroupHeaderStyle x:SetterTargetType="atom:ListViewItem">
            <Setter Property="FontWeight" Value="SemiBold" />
        </atom:ListViewGroupHeaderStyle>
    </Style>
</Application.Styles>
```

对特定 ListView class 或状态定制时，把 class、属性或伪类放在 owner 一侧：

```xml
<Style Selector="atom|ListView.semantic-custom:empty">
    <atom:ListViewItemStyle x:SetterTargetType="atom:ListViewItem">
        <Setter Property="Opacity" Value="0.6" />
    </atom:ListViewItemStyle>
</Style>
```

不得把 `ContractType` 写入 Part Selector。以下写法不属于公共契约：

- `atom|ListViewItem.semantic-item` 或 `:is(atom|ListViewItem).semantic-item`。
- 直接复制 `> .semantic-item` / `> .semantic-group-header` route 作为用户主路径；route 只属于 descriptor 与生成
  Style 的实现元数据。
- 依赖 `PART_*`、internal 类型、Name 或视觉祖先顺序。
- 通过 Semantic Style 设置 `BorderThickness` 试图恢复最后一项被抑制的分割线。

## 4. 状态与数量语义

数量契约以已实例化的 AtomUI 内置容器为边界。`item` 与 `groupHeader` 是 `RuntimeCreated` Part：marker 随容器实例
存在，不随 item 数据迁移；虚拟化滚动只实例化可视容器，marker 数量等于已实例化容器数量。

| 场景 | root | item | groupHeader | 说明 |
| --- | --- | --- | --- | --- |
| 普通列表 N 条 | 1 | N | 0 | 每个非分组 `ListViewItem` 一个 `semantic-item`。 |
| 分组列表（G 组） | 1 | N | G | 每个 `GroupHeaderItem` 一个 `semantic-group-header`。 |
| 空集合 | 1 | 0 | 0 | `:empty`，不创建条目容器。 |
| 容器回收 / 复用 | 1 | 不变 | 不变 | marker 随容器实例，回收与重新 prepare 不增删 marker。 |
| 集合追加 / 删除 | 1 | 随容器数 | 随容器数 | 新容器创建时建 marker；容器释放时 marker 随之消失。 |
| 分组开关切换 | 1 | 不变 | 随容器数 | 既有容器角色由类型决定，不切换 marker。 |
| hover / selected / disabled | 1 | N | G | 状态切换只改变有效视觉属性，不增删 marker。 |

## 5. 尺寸与分割线基线

ListView 通过 `SizeType` 提供 Large、Middle / Custom、Small 三档基线：

- root 圆角来自 SharedToken（`BorderRadiusLG` / `BorderRadius` / `BorderRadiusSM`）。
- 条目最小高度来自 SharedToken（`ControlHeightLG` / `ControlHeight` / `ControlHeightSM`）。
- 条目 padding 来自 `ItemPaddingLG` / `ItemPadding` / `ItemPaddingSM`，均为仅水平 padding。

条目表面保持直角，主题不设置条目 `CornerRadius`。`ContentPadding` 与 `ItemMargin` 默认均为 `Thickness(0)`：条目
直接贴合 root 外框内边缘，条目之间不留垂直间距，列表紧凑感由条目高度与分割线表达。

分割线基线：

- 默认 1 DIP（SharedToken `LineWidth`）`ColorSplit` 底边线，由条目 `BorderThickness` / `BorderBrush` 驱动。
- 最后一项抑制：无 `BottomPagination` 时最后一项不显示底部分割线，root 外框下边缘承担闭合线；`BottomPagination`
  或 `IsBorderless` 时保留。
- 组标题恒不显示分割线。

Semantic Style 覆盖 `item` 的 `BorderThickness` 底部值时作用于分割线宽度；覆盖 `Padding`、`MinHeight` 等布局属性时，
应同时验证三档 SizeType、最后一项抑制与 hover / selected 背景不溢出圆角内边缘。

## 6. 定制边界

以下区域明确不属于 ListView Semantic Part：

- `SelectedIndicator`、选中指示图标与选中状态视觉。
- 顶部 / 底部分页器 presenter、`Spin` 操作态遮罩、`EmptyIndicator` 与空状态节点。
- 用户 `ItemTemplate` / `GroupItemTemplate` 创建的子树。
- `SplitLineFrame` 节点本身：分割线通过 `ListViewItem` 的 `BorderThickness` / `BorderBrush` 与状态机表达，模板节点
  名称与数量不属于契约。
- Listy 内部节点（`-sticky` / `-fixed` / `-holder` / `-group-section` / `-scrollbar`）的对应物。
- `PART_*` 名称、internal 类型与模板层级。

最后一项分割线抑制由 ListView 状态机按容器视图位置与 `BottomPagination` / `IsBorderless` 决定，Semantic Style 设置
属性不能恢复被抑制的分割线，也不能把分割线状态写进样式。

Semantic Style 服从 Avalonia 原生属性优先级。Part Setter 命中只证明目标属性已生效；如果最终布局仍被 owner 或中间
节点的 Height、Min/Max、Padding、Margin、覆盖约束，应按跨节点布局约束排查，不能把它解释为 Semantic Style 优先级
失效。

## 7. 兼容性与验证

删除或重命名 Part、修改 selector class、收窄 `ContractType`、改变 cardinality，或者让任一内置模板缺少 marker，均
属于公共主题契约变更。

验证至少覆盖：

- descriptor 中只有 `root`、`item`、`groupHeader`，字段值与本文表格一致。
- N 个条目时 `semantic-item` 共 N 个、`semantic-group-header` 等于组数；空集合为 0。
- 分组开关、容器复用 / 回收与 items 变化不增删既有容器的 marker。
- root 表面投影生效：外框使用 `ColorSplit`，hover / selected 背景被 `Frame` 的 `ClipContentToCornerRadius` 圆角内容裁剪约束在圆角内边缘内。
- 分割线规则：无 `BottomPagination` 时最后一项无分割线且外框下边缘为单一闭合线；`BottomPagination` 或
  `IsBorderless` 时最后一项保留分割线；追加 / 删除条目后原最后一项恢复、新最后一项抑制；组标题无分割线。
- 三档 SizeType 下条目直角、贴边与分割线规则一致。
- owner-scoped Semantic Style 与 `x:SetterTargetType` 可以编译并命中对应最低 public 类型。
- 默认主题不消费 `.semantic-*`，未声明用户 Semantic Style 时不增加 selector activator。
- Generator 静态输出和 NativeAOT 路径不依赖反射或运行时扫描。
