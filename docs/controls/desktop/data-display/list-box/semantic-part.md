# ListBox Semantic Part 契约

本文档定义 `ListBox` 对应用公开的 Semantic Part、Selector、类型约束、数量语义、分割线基线和定制边界。ListBox 的整体设计见
[ListBox 桌面版架构设计](overview.md)，descriptor 与真实模板节点映射见 [ListBox 桌面版实现原理](implementation.md)，
系统级规则见 [AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Semantic Parts

ListBox 公开 `root` 与 `item` 两个职责区域，与上游稳定 Semantic DOM（`Listy` 组件的
`classNames` / `styles` 均为 `{ root?, item?, groupHeader? }`）对齐。上游 `groupHeader` 不适用于 ListBox——ListBox
是轻量选择列表，只有选择、过滤与 CandidateList 基座职责，没有分组功能，不虚构 Part。`root` 由生成器为带非 root
Part 的 owner 隐式加入，不生成 Style；`item` 是运行时由 ListBox 创建的容器 Part。

#### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `ListBox` |
| Part | `root` |
| Selector | ListBox 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `ListBox` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | ListBox owner |
| 职责 | 根语义区域，即滚动容器，承载字体、行高、相对定位、外框与外框闭合边界；对应上游 `.ant-listy`。 |
| 相关 API | `ItemsSource`、`ItemTemplate`、`SizeType`、`IsBorderless`、`IsSelectable`、`SelectionMode` |
| 相关 Token | `ListBoxToken`、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `item`

| 字段 | 值 |
| --- | --- |
| Owner | `ListBox` |
| Part | `item` |
| Selector | `.semantic-item` |
| SelectorRoute | `> .semantic-item` |
| Style Type | `ListBoxItemStyle` |
| ContractType | `ListBoxItem` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | 每个 `ListBoxItem` 容器 |
| 职责 | 条目元素，设置内间距、底部分割线与悬浮背景；对应上游 `.ant-listy-item`。 |
| 相关 API | `SizeType`、`ItemHoverBg`、`ItemSelectedBg` |
| 相关 Token | `ItemPaddingSM`、`ItemPadding`、`ItemPaddingLG`、`ItemHoverBgColor`、`ColorSplit`、`ControlItemBgHover` |
| 稳定性 | stable since 6.2.0 |

`root` 是隐式 Part，不声明 `.semantic-root` marker。`item` 的 marker `.semantic-item` 在 `ListBoxItem` 创建路径
一次性添加，`PrepareContainerForItemOverride` 幂等补齐（覆盖用户直接提供容器与 `CandidateListItem` 派生容器的
路径）；`ListBoxItem` 只有一种身份，不存在切换。

`ContractType` 只定义 Setter 可以稳定依赖的最低 public 类型，并通过 `x:SetterTargetType` 提供 AXAML 编译期类型
上下文；它不参与 `.semantic-*` 的身份匹配。

## 2. Part 说明

### 2.1 root

`root` 是 ListBox owner 本身，在 ListBox 实例的整个生命周期内始终存在，并且每个 ListBox 恰好一个。

它负责：

- 承载 `ItemsSource`、`SizeType`、`IsBorderless`、选择、过滤与空状态等公共状态和 API。
- 承载继承 Avalonia `ListBox` 的选择、hover、focus 和 disabled 相关伪类语义。
- 提供最终 `Background`、`BorderBrush`、`BorderThickness`、`CornerRadius`、`Padding`、`Margin` 和字体等根视觉属性。
- 作为 `item` owner-scoped Selector 的作用域边界。

root 表面投影到模板中的 `Frame` / `ScrollViewer`：root 外框默认使用 `ColorSplit`（与条目分割线同色），表达“外框即
列表闭合线”；root `Frame` 开启 `ClipContentToCornerRadius`，条目 hover / selected 背景等溢出圆角内边缘的内容被
裁剪，不在圆角口袋区溢出。

适合通过 root 定制 ListBox 整体背景、外框颜色与宽度、圆角、padding、字体和对齐状态。需要根据状态改变根样式时，
应在 owner Selector 上组合公开属性或伪类。

root 不表示模板中的 `PixelAlignedBorder#Frame`、`PART_ScrollViewer`、`EmptyIndicator` 或任一 presenter；这些节点的
名称、数量、层级与内容裁剪实现值不属于 root 契约。

### 2.2 item

`item` 表示一个数据条目容器，每个 `ListBoxItem` 恰好一个 marker，cardinality 为 `Multiple`。marker `.semantic-item`
在 `ListBoxItem` 创建路径一次性添加，prepare 幂等补齐；虚拟化回收、过滤与选择状态切换不增删 marker。

它负责：

- 设置条目内间距（`ItemPaddingSM` / `ItemPadding` / `ItemPaddingLG`，仅水平 padding）。
- 设置条目底部分割线（默认 1 DIP `ColorSplit` 底边线）。
- 设置 hover / selected 背景（`ItemHoverBg` / `ItemSelectedBg`）与前景状态。
- 承载 `Content` / `ContentTemplate` 与过滤高亮文本的最终展示。

分割线由模板中的 `SplitLineFrame` 节点承载，绑定容器的 `EffectiveBorderThickness` 与 `IsSplitLineEffectiveVisible`，
因此 Semantic Style 对 `ListBoxItem.BorderThickness`（底部值）与 `BorderBrush` 的覆盖直接作用到分割线。分割线可见性
由 ListBox 状态机决定：最后一项的底部分割线被抑制，由 root 外框下边缘承担闭合线；`IsBorderless=true` 时保留
（外框消失后由分割线承担闭合线）。Semantic Style 不参与抑制判定，也不能绕过最后一项抑制。

适合定制 `Background`、`BorderBrush`、`BorderThickness`、`Padding`、`Foreground`、`FontSize`、`MinHeight`、
`Opacity` 等属性。条目表面保持直角：默认主题不设置条目 `CornerRadius`，不建议把条目绘制成圆角卡片。

item 不公开模板中的 `SelectedIndicator`、`ContentPresenter`、过滤高亮节点，也不公开用户 `ItemTemplate` 生成的子树。

## 3. Selector 用法

应用级样式先限定 ListBox owner，再通过生成的 Semantic Style 进入 Part。生成类型已经封装 owner 类型保护和
`SelectorRoute`，用户不需要复制模板路径：

```xml
<Application.Styles>
    <Style Selector="atom|ListBox">
        <atom:ListBoxItemStyle x:SetterTargetType="atom:ListBoxItem">
            <Setter Property="Background" Value="#FAFAFA" />
            <Setter Property="BorderThickness" Value="0,0,0,2" />
        </atom:ListBoxItemStyle>
    </Style>
</Application.Styles>
```

对特定 ListBox class 或状态定制时，把 class、属性或伪类放在 owner 一侧：

```xml
<Style Selector="atom|ListBox.semantic-custom:pointerover">
    <atom:ListBoxItemStyle x:SetterTargetType="atom:ListBoxItem">
        <Setter Property="Foreground" Value="#1677ff" />
    </atom:ListBoxItemStyle>
</Style>
```

不得把 `ContractType` 写入 Part Selector。以下写法不属于公共契约：

- `atom|ListBoxItem.semantic-item` 或 `:is(atom|ListBoxItem).semantic-item`。
- 直接复制 `> .semantic-item` route 作为用户主路径；route 只属于 descriptor 与生成 Style 的实现元数据。
- 依赖 `PART_*`、internal 类型、Name 或视觉祖先顺序。
- 通过 Semantic Style 设置 `BorderThickness` 试图恢复最后一项被抑制的分割线。

## 4. 状态与数量语义

数量契约以已实例化的 AtomUI 内置容器为边界。`item` 是 `RuntimeCreated` Part：marker 随容器实例存在，不随 item
数据迁移；虚拟化滚动只实例化可视容器，marker 数量等于已实例化容器数量。

| 场景 | root | item | 说明 |
| --- | --- | --- | --- |
| 普通列表 N 条 | 1 | N | 每个 `ListBoxItem` 一个 `semantic-item`。 |
| 空集合 | 1 | 0 | 显示空状态，不创建条目容器。 |
| 容器回收 / 复用 | 1 | 不变 | marker 随容器实例，回收与重新 prepare 不增删 marker。 |
| 集合追加 / 删除 | 1 | 随容器数 | 新容器创建时建 marker；容器释放时 marker 随之消失。 |
| 过滤隐藏未命中项 | 1 | 不变 | 隐藏条目只改变 `IsVisible`，不增删 marker。 |
| hover / selected / disabled | 1 | N | 状态切换只改变有效视觉属性，不增删 marker。 |

## 5. 尺寸与分割线基线

ListBox 通过 `SizeType` 提供 Large、Middle / Custom、Small 三档基线：

- root 圆角来自 SharedToken（`BorderRadiusLG` / `BorderRadius` / `BorderRadiusSM`）。
- 条目最小高度来自 SharedToken（`ControlHeightLG` / `ControlHeight` / `ControlHeightSM`）。
- 条目 padding 来自 `ItemPaddingLG` / `ItemPadding` / `ItemPaddingSM`，均为仅水平 padding。

条目表面保持直角，主题不设置条目 `CornerRadius`。`ContentPadding` 与 `ItemMargin` 默认均为 `Thickness(0)`：条目
直接贴合 root 外框内边缘，条目之间不留垂直间距，列表紧凑感由条目高度与分割线表达。

分割线基线：

- 默认 1 DIP（SharedToken `LineWidth`）`ColorSplit` 底边线，由条目 `BorderThickness` / `BorderBrush` 驱动。
- 最后一项抑制：最后一项不显示底部分割线，root 外框下边缘承担闭合线；`IsBorderless=true` 时保留。

Semantic Style 覆盖 `item` 的 `BorderThickness` 底部值时作用于分割线宽度；覆盖 `Padding`、`MinHeight` 等布局属性时，
应同时验证三档 SizeType、最后一项抑制与 hover / selected 背景不溢出圆角内边缘。

## 6. 定制边界

以下区域明确不属于 ListBox Semantic Part：

- `SelectedIndicator`、选中指示图标与选中状态视觉。
- 过滤高亮节点（`HighlightableTextBlock`）、`FilterHighlightForeground` 与 `FilterHighlightStrategy`。
- `EmptyIndicator` 与空状态节点。
- 用户 `ItemTemplate` 创建的子树。
- `SplitLineFrame` 节点本身：分割线通过 `ListBoxItem` 的 `BorderThickness` / `BorderBrush` 与状态机表达，模板节点
  名称与数量不属于契约。
- `PART_*` 名称、internal 类型与模板层级。

`CandidateList` / `CandidateListItem` 继承 ListBox 的容器生成与 item theme，条目视觉与分割线规则随 ListBox 契约
生效；它们不持有独立 Semantic descriptor。候选高亮、commit / cancel 与键盘导航是行为能力，不属于 Semantic Part。

最后一项分割线抑制由 ListBox 状态机按容器视图位置与 `IsBorderless` 决定，Semantic Style 设置属性不能恢复被抑制的
分割线，也不能把分割线状态写进样式。

Semantic Style 服从 Avalonia 原生属性优先级。Part Setter 命中只证明目标属性已生效；如果最终布局仍被 owner 或中间
节点的 Height、Min/Max、Padding、Margin、覆盖约束，应按跨节点布局约束排查，不能把它解释为 Semantic Style 优先级
失效。

## 7. 兼容性与验证

删除或重命名 Part、修改 selector class、收窄 `ContractType`、改变 cardinality，或者让任一内置模板缺少 marker，均
属于公共主题契约变更。

验证至少覆盖：

- descriptor 中只有 `root`、`item`，字段值与本文表格一致。
- N 个条目时 `semantic-item` 共 N 个；空集合为 0。
- 容器复用 / 回收与 items 变化不增删既有容器的 marker。
- root 表面投影生效：外框使用 `ColorSplit`，hover / selected 背景被 `Frame` 的 `ClipContentToCornerRadius` 圆角内容裁剪约束在圆角内边缘内。
- 分割线规则：最后一项无分割线且外框下边缘为单一闭合线；`IsBorderless` 时最后一项保留分割线；追加 / 删除条目后
  原最后一项恢复、新最后一项抑制。
- 三档 SizeType 下条目直角、贴边与分割线规则一致。
- owner-scoped Semantic Style 与 `x:SetterTargetType` 可以编译并命中对应最低 public 类型。
- `CandidateListItem` 容器仍公开同一 descriptor，marker 不跨容器泄漏。
- 默认主题不消费 `.semantic-*`，未声明用户 Semantic Style 时不增加 selector activator。
- Generator 静态输出和 NativeAOT 路径不依赖反射或运行时扫描。
