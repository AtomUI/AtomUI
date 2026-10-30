# Tag Semantic Part 契约

本文档定义 Tag 家族对应用公开的 Semantic Part、选择器、类型约束、数量语义和定制边界。Tag 的整体设计见
[Tag 桌面版架构设计](overview.md)，CheckableTag 选择与组合机制见 [CheckableTag 与 CheckableTagGroup 选择模型设计](checkable-tag-design.md)，
descriptor 与真实模板节点映射见 [Tag 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Semantic Parts

Tag 家族由两个独立 owner 公开 Semantic Part，与上游稳定 Semantic DOM 对齐：

- `Tag` 公开 `root`、`icon`、`content` 与 `close` 四个职责区域，对齐上游 `TagSemanticType`
  （`classNames` / `styles` 均为 `{ root?, icon?, content?, close? }`）。上游 `root` 消费于 `.ant-tag` 根节点，
  `icon` 消费于图标节点，`content` 消费于文本 span（仅当图标与文本并存时上游才为文本包 span），`close` 消费于
  `.ant-tag-close-icon`。
- `CheckableTagGroup` 公开 `root` 与 `item` 两个职责区域，对齐上游 `CheckableTagGroupSemanticType`
  （`classNames` / `styles` 均为 `{ root?, item? }`）。上游 `root` 消费于 `.ant-tag-checkable-group` 根节点，
  `item` 消费于每个 `.ant-tag-checkable-group-item` 选项容器。
- 上游 `CheckableTag` 没有独立 Semantic DOM Props（不消费 `useMergeSemantic`），AtomUI 同样不为其声明
  descriptor；其职责通过 `CheckableTagGroup` 的 `item` Part 对外公开。

AtomUI 六个 Part 随本次 Semantic Part 改造同时公开，descriptor 的 `Since` 统一为 `6.2.0`。

以下类型不持有独立 Semantic descriptor：

- `CheckableTag` 是 ToggleButton 派生的公开选项容器，上游无对应 Semantic DOM Props，容器职责由
  `CheckableTagGroup` 的 `item` Part 表达（与 `SegmentedItem`、`ListBoxItem`、`CollapseItem` 同一决策）。
- `AbstractTag`、`AbstractCheckableTag`、`AbstractCheckableTagGroup` 是跨平台共享基类，不是对应用公开的独立
  owner。
- `CheckableTagItemsControl` 是 internal 的 items host，承载 Group 的 SelectionModel 与容器生成；它不作为
  公开 owner，只作为 `item` route 的中间作用域跳点。

### 1.1 `Tag`

#### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `Tag` |
| Part | `root` |
| Selector | Tag 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `Tag` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | Tag owner（表面投影到 `PixelAlignedBorder#Frame`） |
| 职责 | Tag root 是颜色类别、Variant、内容与关闭入口的统一 owner；根表面（背景、边框、圆角、字体、内边距）投影到 `Frame`。 |
| 相关 API | `TagColor`、`Variant`、`Text`、`Icon`、`CloseIcon`、`IsClosable`、`Closed` |
| 相关 Token | `DefaultBg`、`DefaultColor`、`TagFontSize`、`TagPadding`、`SolidTextColor`、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `icon`

| 字段 | 值 |
| --- | --- |
| Owner | `Tag` |
| Part | `icon` |
| Selector | `.semantic-icon` |
| SelectorRoute | `/template/ .semantic-icon` |
| Style Type | `TagIconStyle` |
| ContractType | `IconPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | Tag 模板中的 `IconPresenter#IconPresenter` |
| 职责 | 统一表示 Tag 的前置图标区域：图标尺寸、画刷（随 root 前景）与可见性；对应上游 Tag 的 `icon` 语义键。 |
| 相关 API | `Icon` |
| 相关 Token | `TagIconSize` |
| 稳定性 | stable since 6.2.0 |

#### `content`

| 字段 | 值 |
| --- | --- |
| Owner | `Tag` |
| Part | `content` |
| Selector | `.semantic-content` |
| SelectorRoute | `/template/ .semantic-content` |
| Style Type | `TagContentStyle` |
| ContractType | `TextBlock` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | Tag 模板中的 `TextBlock#TagTextLabel` |
| 职责 | 统一表示 Tag 的文本区域：文本呈现、行高、图文/文关内联间距；对应上游 Tag 的 `content` 语义键。 |
| 相关 API | `Text` |
| 相关 Token | `TagLineHeight`、`TagTextPaddingInline` |
| 稳定性 | stable since 6.2.0 |

#### `close`

| 字段 | 值 |
| --- | --- |
| Owner | `Tag` |
| Part | `close` |
| Selector | `.semantic-close` |
| SelectorRoute | `/template/ .semantic-close` |
| Style Type | `TagCloseStyle` |
| ContractType | `IconButton` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | Tag 模板中的 `IconButton#PART_CloseButton` |
| 职责 | 统一表示 Tag 的关闭入口：关闭图标尺寸、画刷（随 root 前景）与可见性；承载 `Closed` 事件触发；对应上游 `.ant-tag-close-icon`。 |
| 相关 API | `CloseIcon`、`IsClosable`、`Closed` |
| 相关 Token | `TagCloseIconSize`、SharedToken（`IconSizeXS`） |
| 稳定性 | stable since 6.2.0 |

### 1.2 `CheckableTagGroup`

#### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `CheckableTagGroup` |
| Part | `root` |
| Selector | CheckableTagGroup 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `CheckableTagGroup` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | CheckableTagGroup owner（模板直接承载 `PART_CheckableTagItems`） |
| 职责 | CheckableTagGroup root 是 Options 数据、单选/多选模式、公开选择值与 Form 语义的统一 owner。 |
| 相关 API | `Options`、`ItemTemplate`、`IsMultiple`、`CheckedItem`、`CheckedItems`、`DefaultCheckedItem`、`DefaultCheckedItems`、`ItemSpacing`、`LineSpacing`、`Orientation`、`IsMotionEnabled`、`CheckedChanged` |
| 相关 Token | SharedToken（`SpacingXS`、`EnableMotion`） |
| 稳定性 | stable since 6.2.0 |

#### `item`

| 字段 | 值 |
| --- | --- |
| Owner | `CheckableTagGroup` |
| Part | `item` |
| Selector | `.semantic-item` |
| SelectorRoute | `/template/ .semantic-scope-items > .semantic-item` |
| Style Type | `CheckableTagGroupItemStyle` |
| ContractType | `CheckableTag` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | 每个 `CheckableTag` 容器 |
| 职责 | 统一表示 Group 内单个可交互选项：背景/前景选择视觉、内边距、圆角、光标与 checked/hover/pressed/disabled 状态；对应上游 `.ant-tag-checkable-group-item`。 |
| 相关 API | `CheckableTag.Content`、`CheckableTag.Icon`、`CheckableTag.IsChecked`、`IsMultiple` |
| 相关 Token | `TagFontSize`、`TagLineHeight`、`TagPadding`、`TagIconSize`、SharedToken（`ColorPrimary*`、`ColorTextLightSolid`、`ColorFillSecondary`、`ColorTextDisabled`） |
| 稳定性 | stable since 6.2.0 |

`root` 是隐式 Part，不声明 `.semantic-root` marker。Tag 的 `icon`、`content`、`close` marker 声明在
`TagTheme.axaml` 模板内的 `IconPresenter#IconPresenter`、`TextBlock#TagTextLabel` 与
`IconButton#PART_CloseButton` 节点上，静态存在，随 owner 模板实例化；descriptor 声明 `RuntimeCreated=false`，
由生成器按 owner 主题资产静态校验。

`item` 是 `RuntimeCreated` Part：`.semantic-item` marker 在 `CheckableTag` 容器创建路径一次性添加，prepare
幂等补齐；`.semantic-scope-items` 是 route 中间跳点（不是 Part），静态声明在 `CheckableTagGroupTheme.axaml`
模板的 `CheckableTagItemsControl#PART_CheckableTagItems` 节点上。Group 不是 ItemsControl，选项容器
`CheckableTag` 的逻辑父级是 internal 的 `CheckableTagItemsControl`，因此 route 必须先经 `/template/` 进入
Group 模板命中 scope-items 跳点，再经 `>` 一步到达容器——与 `Descriptions` 的
`/template/ .semantic-scope-items > .semantic-scope-item` 链同一模式。

`ContractType` 只定义 Setter 可以稳定依赖的最低 public 类型，并通过 `x:SetterTargetType` 提供 AXAML 编译期
类型上下文；它不参与 `.semantic-*` 的身份匹配。

## 2. Part 说明

### 2.1 Tag root

`root` 是 Tag owner 本身，在 Tag 实例的整个生命周期内始终存在，并且每个 Tag 恰好一个。

它负责：

- 承载 `TagColor`、`Variant`、`Text`、`Icon`、`CloseIcon`、`IsClosable` 与 `Closed` 事件等公共状态和 API。
- 承载颜色类别状态机：`Default` / `Preset` / `Status` / `Custom` 四种颜色类别经伪类
  `:preset-color` / `:status-color` / `:custom-color` 表达，最终计算为 `Background` / `Foreground` /
  `BorderBrush`（`BindingPriority.Template`）。
- 提供根视觉属性：背景、边框、圆角（`BorderRadiusSM`）、字体（`FontSizeSM`）与内边距（`TagPadding`），表面
  投影到模板中的 `Frame`。

适合通过 root 定制 Tag 整体背景、前景、边框、圆角与 padding。颜色状态机以 `Template` 优先级写回
`Background` / `Foreground` / `BorderBrush`，低于用户 selector Style 的 `StyleTrigger` 与本地值优先级，因此
root 样式 setter 会统一覆盖 Default/Preset/Status/Custom 全部颜色分支；移除样式或本地值后，状态机恢复计算
（见 6 节）。

root 不表示模板中的 `Frame`、`DockPanel` 布局节点本身；这些节点的名称、数量与层级不属于 root 契约。

### 2.2 Tag icon / content / close

三个静态 Part 各对应 Tag 模板中的一个常驻节点，cardinality 均为 `Single`：

- `icon` 承载 `Icon` 的呈现，尺寸 `TagIconSize`，画刷随 root 前景；`Icon` 为 null 时节点折叠（`IsVisible` 数据
  驱动），marker 不移除。
- `content` 承载 `Text` 的文本呈现，行高 `TagLineHeight`，内联间距 `TagTextPaddingInline`；节点常驻，`Text`
  为空时渲染空文本（仍保留内联 padding 占位）。
- `close` 承载关闭入口（`CloseIcon`，默认 `CloseOutlined`），尺寸 `IconSizeXS`，画刷随 root 前景；
  `IsClosable=false` 时节点折叠，marker 不移除；点击派发冒泡的 `Closed` 事件。

节点可见性全部由数据驱动，Semantic Style 覆盖可见性（`IsVisible`）会绕过 `Icon` / `IsClosable` 状态机，属于
不推荐用法。适合定制 `icon` / `close` 的 `Width` / `Height` / `IconBrush` 与 `content` 的 `LineHeight` /
`Padding` 等属性。

### 2.3 CheckableTagGroup root

`root` 是 CheckableTagGroup owner 本身，每个 Group 恰好一个。

它负责：

- 承载 `Options`、`ItemTemplate`、`IsMultiple`、`CheckedItem(s)` 与 Default 初始化等公共状态和 API。
- 承载单选/多选模式、选项集合生命周期与选择同步状态机。
- 作为 `item` owner-scoped Selector 的作用域边界（经模板 scope-items 跳点进入容器层）。

root 模板直接承载 `PART_CheckableTagItems`，无 Frame/边框表面；Group 根视觉定制主要涉及对齐、间距与裁剪。

### 2.4 CheckableTagGroup item

`item` 表示 Group 内一个选项容器，每个 `CheckableTag` 恰好一个 marker，cardinality 为 `Multiple`。marker 在
容器创建路径一次性添加，prepare 幂等补齐；容器生命周期内不增删 marker。

它负责：

- 承载 `Content` / `ContentTemplate` / `Icon` 的最终布局与呈现。
- 设置 checked / unchecked / hover / pressed / disabled 状态视觉（背景与前景）。
- 承载 `:checked`、`:pointerover`、`:pressed`、`:focus-visible`、`:disabled` 伪类与键盘交互（Space 切换）。

适合定制 `Background`、`Foreground`、`CornerRadius`、`Padding`、`MinHeight`、`FontSize`、`Cursor` 等属性。
`IsMultiple` 模式切换、Options 集合重置与选择变化不增删 marker（见 4 节）。

item 不公开 CheckableTag 模板中的 `Frame`、`DockPanel`、`FocusVisual` 节点，也不公开用户 `ItemTemplate`
生成的子树。

## 3. Selector 用法

应用级样式先限定对应 owner，再通过生成的 Semantic Style 进入 Part。生成类型已经封装 owner 类型保护和
`SelectorRoute`，用户不需要复制模板路径：

```xml
<Application.Styles>
    <Style Selector="atom|Tag">
        <atom:TagIconStyle x:SetterTargetType="atom:IconPresenter">
            <Setter Property="IconBrush" Value="#1677FF" />
        </atom:TagIconStyle>
        <atom:TagContentStyle x:SetterTargetType="TextBlock">
            <Setter Property="FontWeight" Value="SemiBold" />
        </atom:TagContentStyle>
        <atom:TagCloseStyle x:SetterTargetType="atom:IconButton">
            <Setter Property="IconBrush" Value="#ff4d4f" />
        </atom:TagCloseStyle>
    </Style>
    <Style Selector="atom|CheckableTagGroup">
        <atom:CheckableTagGroupItemStyle x:SetterTargetType="atom:CheckableTag">
            <Setter Property="CornerRadius" Value="6" />
        </atom:CheckableTagGroupItemStyle>
    </Style>
</Application.Styles>
```

对特定 class 或状态定制时，把 class、属性或伪类放在 owner 一侧：

```xml
<Style Selector="atom|Tag.semantic-custom">
    <atom:TagIconStyle x:SetterTargetType="atom:IconPresenter">
        <Setter Property="IconBrush" Value="#52c41a" />
    </atom:TagIconStyle>
</Style>
```

不得把 `ContractType` 写入 Part Selector。以下写法不属于公共契约：

- `atom|TextBlock.semantic-content` 或 `:is(atom|CheckableTag).semantic-item`。
- 直接复制 `/template/ .semantic-icon` 或 `/template/ .semantic-scope-items > .semantic-item` route 作为用户
  主路径；route 只属于 descriptor 与生成 Style 的实现元数据。
- 依赖 `PART_*`、internal 类型、Name 或视觉祖先顺序。
- 通过 `close` 的 Semantic Style 设置 `IsVisible` 绕过 `IsClosable` 状态机。
- 通过 root Semantic Style 覆盖颜色后期待只有部分颜色类别变化：一个 root setter 会统一覆盖
  Default/Preset/Status/Custom 全部分支。

## 4. 状态与数量语义

数量契约以已实例化的 AtomUI 内置容器为边界。`item` 是 `RuntimeCreated` Part：marker 随容器实例存在，不随
option 数据迁移。

| 场景 | Tag root | icon / content / close | Group root | item | 说明 |
| --- | --- | --- | --- | --- | --- |
| 默认 Tag（无图标、不可关闭） | 1 | 1 / 1 / 1 | 不适用 | 不适用 | `icon`、`close` 节点折叠但 marker 保留。 |
| 图标 Tag | 1 | 1 / 1 / 1 | 不适用 | 不适用 | `icon` 可见，其余同左。 |
| 可关闭 Tag | 1 | 1 / 1 / 1 | 不适用 | 不适用 | `close` 可见，点击派发 `Closed`。 |
| Group N 个选项 | 不适用 | 不适用 | 1 | N | 每个 `CheckableTag` 一个 `semantic-item`。 |
| Group 空 Options | 不适用 | 不适用 | 1 | 0 | 不创建容器。 |
| 选项增删 / 集合重置 | 不适用 | 不适用 | 1 | 随容器数 | 新容器创建时建 marker；容器释放时 marker 消失。 |
| 选择变化 / checked 切换 | 不适用 | 不适用 | 1 | N | 状态切换只改变有效视觉属性，不增删 marker。 |
| 单选 ↔ 多选模式转换 | 不适用 | 不适用 | 1 | N | `IsMultiple` 转换只改变选择模型，不增删 marker。 |
| `IsClosable` 切换 | 1 | 1 / 1 / 1 | 不适用 | 不适用 | `close` 可见性切换，marker 不增删。 |

## 5. 尺寸基线

Tag 家族没有 `SizeType` 分档，视觉基线由全局 token 常量表达：

- Tag 字体 `FontSizeSM`、行高 `FontHeightSM`，CheckableTag 最小高度 `TagLineHeight`。
- Tag/CheckableTag 内边距 `TagPadding`，文本内联间距 `TagTextPaddingInline`，图标尺寸 `TagIconSize`
  （`FontSizeIcon`），关闭图标 `IconSizeXS`。
- 圆角统一 `BorderRadiusSM`；Group 选项间距与行距为 `SpacingXS`。

Semantic Style 覆盖 `content` 的 `LineHeight` / `Padding` 或 `icon` / `close` 尺寸时，应验证 Tag 的最小高度
由行高与 padding 推导、不溢出圆角内边缘，并验证 CheckableTag 的 checked 背景在覆盖 `Padding` / `MinHeight`
后仍完整覆盖内容区。

## 6. 定制边界

以下区域明确不属于 Tag Semantic Part：

- Tag 颜色算法：`TagColor` × `Variant` 的 Default/Preset/Status/Custom 计算与
  `:preset-color` / `:status-color` / `:custom-color` 伪类是行为状态，不是 Part；颜色以 `Template` 优先级写回
  `Background` / `Foreground` / `BorderBrush`，低于用户 selector Style 的 `StyleTrigger` 与本地值优先级，
  因此用户 root 样式 setter 统一覆盖全部颜色分支，移除后状态机恢复——覆盖颜色是受支持的定制入口。
- `CheckableTag` 容器自身不作为独立 owner：上游无 Semantic DOM Props，其职责由 `item` Part 表达。
- `CheckableTagItemsControl`（internal items host）、`WrapPanel`、`PART_ItemsPresenter`、`PART_CheckableTagItems`
  等模板结构节点；`.semantic-scope-items` 只作为 route 跳点，不是 Part。
- `FocusVisual`（CheckableTag 键盘焦点视觉）、用户 `ItemTemplate` 子树。
- `AbstractTag` / `AbstractCheckableTag` / `AbstractCheckableTagGroup` 跨平台基类与 internal 状态属性。
- `TagStatus` 状态色、`PresetColorType` 预设色与 `CloseIcon` 的具体图标内容。

Semantic Style 服从 Avalonia 原生属性优先级。Part Setter 命中只证明目标属性已生效；Tag 颜色状态机以
`Template` 优先级写入根颜色属性，用户 selector Style 的 setter（`StyleTrigger`）与本地值优先级高于它，因此
用户样式统一覆盖全部颜色分支，移除后状态机恢复。颜色算法仍是行为状态而非 Part，覆盖颜色是受支持的
定制入口，与上游一致（用户 Semantic Style 覆盖基础颜色）。

## 7. 兼容性与验证

删除或重命名 Part、修改 selector class、收窄 `ContractType`、改变 cardinality，或者让任一内置模板缺少
marker，均属于公共主题契约变更。

验证至少覆盖：

- descriptor 中 `Tag` 只有 `root`、`icon`、`content`、`close`；`CheckableTagGroup` 只有 `root`、`item`，字段
  值与本文表格一致；`CheckableTag`、基类与 internal items host 不持有 descriptor。
- Tag 各形态（默认 / 图标 / 可关闭 / 图标+可关闭）下 marker 数量恒为 root=1、icon=1、content=1、close=1；
  折叠节点 marker 保留。
- `IsClosable` 切换只改变 `close` 可见性，不增删 marker；点击关闭派发 `Closed`。
- Group N 个选项时 `semantic-item` 共 N 个；空 Options 为 0；选项增删 / 集合重置后 marker 随容器实例；既有
  容器 marker 不增删。
- checked 切换、单选 ↔ 多选模式转换不增删 marker。
- owner-scoped Semantic Style（`TagIconStyle` / `TagContentStyle` / `TagCloseStyle` /
  `CheckableTagGroupItemStyle`）与 `x:SetterTargetType` 可以编译并命中对应最低 public 类型。
- `item` 三段 route（owner /template/ scope-items > container）在生成 Style 中命中每个 CheckableTag 容器。
- 颜色状态机（TagColor × Variant）与 Semantic Style 边界：root 样式 setter 统一覆盖全部颜色分支，样式
  移除后状态机恢复。
- 默认主题不消费 `.semantic-*`，未声明用户 Semantic Style 时不增加 selector activator。
- Generator 静态输出和 NativeAOT 路径不依赖反射或运行时扫描。
