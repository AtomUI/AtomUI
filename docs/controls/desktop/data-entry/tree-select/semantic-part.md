# TreeSelect Semantic Part 契约

本文档定义 TreeSelect 控件公开的 Semantic Part、Selector、类型约束、数量语义和定制边界。控件整体设计见
[TreeSelect 桌面版架构设计](overview.md)，真实模板、marker 映射与状态流见
[TreeSelect 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Semantic Parts

TreeSelect 是唯一 Semantic owner，公开 13 个 Semantic Part（语义对齐：`item` / `itemContent` /
`itemRemove` 对应上游多选标签的 `item` 分组；`popup.*` 采用 AtomUI Select 家族的
`root` / `list` / `listItem` 命名，对应上游 TreeSelect `popup` 分组的 `root` / `item` /
`itemTitle` / `itemSwitcher`——弹层树节点的标题、切换器等内部槽位由 `TreeViewItem` 自身已声明的
`itemTitle` / `itemSwitcher` / `itemIcon` / `itemIndicator` Semantic Part 承载，不经 TreeSelect 重复
发布）。声明位于 `TreeSelect.SemanticParts.cs` partial 文件。

触发区部件的 marker 位于 `TreeSelectTheme.axaml` 宿主模板内：`prefix` / `suffix` 借用共享
`AddOnDecoratedBoxTheme` 的 `.semantic-scope-prefix` / `.semantic-scope-suffix` scope 锚点路由到宿主模板
投影给 decorated box 的内容节点（与 Select / Cascader 同构）；`content` / `placeholder` / `input`
直接标注宿主模板节点。`input` 额外在多选模式的运行时搜索框上注入同一 marker（共享
`SelectTagAwareTextBox` 创建搜索框时注入，Cascader 复用同一 marker），使过滤输入在非单选态同样可被
语义高亮与样式命中。`clear` / `item` / `itemContent` / `itemRemove` 声明 `CrossNestedOwners=true`：
`clear` 的物理节点在共享 `SelectHandle` 自有模板内；多选标签的物理节点在共享标签机制内——`item`
的标记由 `SelectTagAwareTextBox` 在标签容器创建时注入，`itemContent` / `itemRemove` 的标记位于共享
`TagTheme` 模板（`SelectTag : Tag` 复用其模板），二者经 `item` 部件（RuntimeCreated，
ContractType=`Tag`）承转主题链完成校验。弹层三部件位于 owner 自有的 Popup 模板内：`popup.root`
标注在宿主模板的 `PopupFrame` 静态节点上，`popup.list`（候选树）与 `popup.listItem`（树节点容器）
的 marker 在运行时容器创建路径注入。

#### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `TreeSelect` |
| Part | `root` |
| Selector | TreeSelect 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `TreeSelect` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | TreeSelect owner |
| 职责 | TreeSelect root 是树数据源、选择/勾选、过滤、弹层与状态的组织边界。 |
| 相关 API | 全部 TreeSelect public API |
| 相关 Token | TreeSelectToken、SharedToken |
| 稳定性 | stable since 6.2.0 |

**根边框与背景定制：** 可见外框由共享 `AddOnDecoratedBox` 帧绘制，其状态机拥有 `BorderBrush` 与
`Background`。owner 的 `BorderBrush` / `Background` 会以 LocalValue 中继到帧节点生效（对齐上游 root
border/background 语义与输入族 `AbstractTextInput.RelayRootSurfaceBrush` 行为），因此在 owner-scoped Style
里直接写根 `Setter Property="BorderBrush"` / `Property="Background"` 即可定制外框：

```xml
<Style Selector="atom|TreeSelect.semantic-root-border">
    <Setter Property="BorderBrush" Value="#1677FF" />
    <Setter Property="Background" Value="#F0F5FF" />
</Style>
```

定制期间该属性槽的 hover / focus / open 变色冻结（LocalValue 优先于帧状态机），focus 的 `BoxShadow` 光晕不受
影响；置空（或不设置）后恢复帧状态机。中继带接管标记：仅当本控件实际写入过该槽位时才在置空时清除，因此嵌套
owner 不会被外层清除。


#### `prefix`

| 字段 | 值 |
| --- | --- |
| Owner | `TreeSelect` |
| Part | `prefix` |
| Selector | `.semantic-prefix` |
| SelectorRoute | `/template/ .semantic-scope-input /template/ .semantic-scope-prefix > .semantic-prefix` |
| Style Type | `TreeSelectPrefixStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `TreeSelectTheme.axaml` 中投影给 `TreeSelectAddOnDecoratedBox.ContentLeftAddOn` 的 `AddOnContentPresenter`（经 `$parent[atom:TreeSelect]` 编译绑定呈现公共 API 值） |
| 职责 | 选择框内容前缀区域，承载 `ContentLeftAddOn` 用户内容，在内容框内联展示。 |
| 相关 API | `ContentLeftAddOn`、`ContentLeftAddOnTemplate` |
| 相关 Token | SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `content`

| 字段 | 值 |
| --- | --- |
| Owner | `TreeSelect` |
| Part | `content` |
| Selector | `.semantic-content` |
| SelectorRoute | `/template/ .semantic-content` |
| Style Type | `TreeSelectContentStyle` |
| ContractType | `Panel` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `TreeSelectTheme.axaml` 中承载占位符、单选过滤框与多选标签容器的 Panel |
| 职责 | 选择内容面板，组织占位符、过滤输入、选中结果与多选标签的布局。 |
| 相关 API | `PlaceholderText`、`IsFilterEnabled`、`IsMultiple` |
| 相关 Token | SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `placeholder`

| 字段 | 值 |
| --- | --- |
| Owner | `TreeSelect` |
| Part | `placeholder` |
| Selector | `.semantic-placeholder` |
| SelectorRoute | `/template/ .semantic-content > .semantic-placeholder` |
| Style Type | `TreeSelectPlaceholderStyle` |
| ContractType | `TextBlock` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `TreeSelectTheme.axaml` 的 `PlaceholderText` TextBlock |
| 职责 | 未选择任何项时显示的占位符文本。 |
| 相关 API | `PlaceholderText`、`PlaceholderForeground` |
| 相关 Token | SharedToken（ColorTextPlaceholder） |
| 稳定性 | stable since 6.2.0 |

#### `input`

| 字段 | 值 |
| --- | --- |
| Owner | `TreeSelect` |
| Part | `input` |
| Selector | `.semantic-input` |
| SelectorRoute | `/template/ .semantic-content > .semantic-input` |
| Style Type | `TreeSelectInputStyle` |
| ContractType | `TextBox` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | 单选过滤输入 `PART_SingleFilterInput`；多选模式下同一 marker 由共享 `SelectTagAwareTextBox` 注入到运行时创建的行内搜索框 |
| 职责 | 过滤模式的搜索输入框（单选态为模板节点，多选态为标签区运行时搜索框）。 |
| 相关 API | `IsFilterEnabled`、`FilterValue` |
| 相关 Token | SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `suffix`

| 字段 | 值 |
| --- | --- |
| Owner | `TreeSelect` |
| Part | `suffix` |
| Selector | `.semantic-suffix` |
| SelectorRoute | `/template/ .semantic-scope-input /template/ .semantic-scope-suffix > .semantic-suffix` |
| Style Type | `TreeSelectSuffixStyle` |
| ContractType | `StackPanel` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `TreeSelectTheme.axaml` 中投影给 `ContentRightAddOn` 的水平 StackPanel（含最大数量指示、自定义后缀与 SelectHandle） |
| 职责 | 选择框后缀区域，承载最大数量指示、用户后缀内容与选择 handle。 |
| 相关 API | `ContentRightAddOn`、`ContentRightAddOnTemplate`、`SuffixIcon`、`MaxCount` |
| 相关 Token | SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `clear`

| 字段 | 值 |
| --- | --- |
| Owner | `TreeSelect` |
| Part | `clear` |
| Selector | `.semantic-clear` |
| SelectorRoute | `>> .semantic-scope-handle /template/ .semantic-clear` |
| Style Type | `TreeSelectClearStyle` |
| ContractType | `Button` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossNestedOwners | `true` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | 共享 `SelectHandle` 模板内的清除按钮（`SelectHandleTheme.axaml`） |
| 职责 | 后缀 handle 内的清除按钮，启用 `IsAllowClear` 时渲染。 |
| 相关 API | `IsAllowClear` |
| 相关 Token | SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `item`

| 字段 | 值 |
| --- | --- |
| Owner | `TreeSelect` |
| Part | `item` |
| Selector | `.semantic-item` |
| SelectorRoute | `>> .semantic-scope-tags >> .semantic-item` |
| Style Type | `TreeSelectItemStyle` |
| ContractType | `Tag` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossNestedOwners | `true` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | 多选标签容器（`SelectTagAwareTextBox` 创建 `SelectTag` 时注入 marker） |
| 职责 | 多选模式下选择器中的选中标签。 |
| 相关 API | `SelectedItems`、`IsMultiple`、`MaxTagCount` |
| 相关 Token | SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `itemContent`

| 字段 | 值 |
| --- | --- |
| Owner | `TreeSelect` |
| Part | `itemContent` |
| Selector | `.semantic-item-content` |
| SelectorRoute | `>> .semantic-scope-tags >> .semantic-item /template/ .semantic-item-content` |
| Style Type | `TreeSelectItemContentStyle` |
| ContractType | `TextBlock` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossNestedOwners | `true` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | 共享 `TagTheme.axaml` 模板内的内容节点 |
| 职责 | 选中标签内的文本内容。 |
| 相关 API | 无（随 `item` 呈现） |
| 相关 Token | SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `itemRemove`

| 字段 | 值 |
| --- | --- |
| Owner | `TreeSelect` |
| Part | `itemRemove` |
| Selector | `.semantic-item-remove` |
| SelectorRoute | `>> .semantic-scope-tags >> .semantic-item /template/ .semantic-item-remove` |
| Style Type | `TreeSelectItemRemoveStyle` |
| ContractType | `IconButton` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossNestedOwners | `true` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | 共享 `TagTheme.axaml` 模板内的移除按钮 |
| 职责 | 选中标签内的移除按钮。 |
| 相关 API | 无（随 `item` 呈现） |
| 相关 Token | SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `popup.root`

| 字段 | 值 |
| --- | --- |
| Owner | `TreeSelect` |
| Part | `popup.root` |
| Selector | `.semantic-popup-root` |
| SelectorRoute | `/template/ .semantic-popup-root` |
| Style Type | `TreeSelectPopupRootStyle` |
| ContractType | `Border` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `TreeSelectTheme.axaml` 中 `PART_Popup` 的静态子节点 `PopupFrame` |
| 职责 | 候选弹层的根边框节点，承载弹层内容根视觉。 |
| 相关 API | `PopupContentPadding`、`MaxPopupHeight`、`EffectivePopupWidth` |
| 相关 Token | PopupToken、TreeSelectToken（MinPopupWidth） |
| 稳定性 | stable since 6.2.0 |

#### `popup.list`

| 字段 | 值 |
| --- | --- |
| Owner | `TreeSelect` |
| Part | `popup.list` |
| Selector | `.semantic-popup-list` |
| SelectorRoute | `/template/ .semantic-popup-root >> .semantic-popup-list` |
| Style Type | `TreeSelectPopupListStyle` |
| ContractType | `Control` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `EnsurePopupContent` 运行时创建的 `TreeSelectTreeView`（创建时注入 marker） |
| 职责 | 弹层内的候选树容器，承载树形候选数据、展开与勾选状态。 |
| 相关 API | `ItemsSource`、`TreeViewToggleType`、`IsDefaultExpandAll` |
| 相关 Token | TreeSelectToken |
| 稳定性 | stable since 6.2.0 |

#### `popup.listItem`

| 字段 | 值 |
| --- | --- |
| Owner | `TreeSelect` |
| Part | `popup.listItem` |
| Selector | `.semantic-popup-list-item` |
| SelectorRoute | `/template/ .semantic-popup-root >> .semantic-popup-list-item` |
| Style Type | `TreeSelectPopupListItemStyle` |
| ContractType | `TemplatedControl` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `TreeSelectTreeView` 容器创建路径生成的 `TreeViewSelectTreeViewItem`（构造时注入 marker，覆盖任意嵌套层级与回收） |
| 职责 | 候选树中的单个树节点条目，运行时创建。 |
| 相关 API | 无（随 `popup.list` 呈现） |
| 相关 Token | TreeSelectToken、TreeViewToken |
| 稳定性 | stable since 6.2.0 |

## 2. 职责与存在条件

- `prefix` / `suffix` 的路由借用共享 `AddOnDecoratedBoxTheme` 的 `.semantic-scope-prefix` /
  `.semantic-scope-suffix` 锚点，marker 本体在宿主模板内，不声明 `CrossNestedOwners`。
- `clear` 的锚点 `.semantic-scope-handle` 位于宿主模板 SelectHandle 节点；`>>` 首步进覆盖锚点节点位于
  owner 模板属性值子树（无 `TemplatedParent` 传播）的场景，生成器沿锚点类型链在 `SelectHandleTheme`
  校验 marker 数量与类型。
- `item` / `popup.list` / `popup.listItem` 为 `RuntimeCreated=true`：多选标签由共享
  `SelectTagAwareTextBox` 运行时创建，候选树由 `EnsurePopupContent` 运行时创建，树节点容器由
  `TreeViewSelectTreeViewItem` 构造路径注入，生成器豁免宿主模板 marker 校验，由控件行为测试兜底。
- `itemContent` / `itemRemove` 的路由经中间跳步 `.semantic-item` 承转：该跳步是同控件已声明的
  RuntimeCreated 部件 `item`（ContractType=`Tag`），生成器以 sibling 部件的 ContractType 继续解析主题链
  至 `TagTheme` 完成静态校验；运行时按 descendant + `/template/` 命中标签实例的模板节点。
- `popup.root` 是 `PART_Popup` 的静态直接子节点，承载弹层内容根视觉；`popup.list` 由代码在
  `EnsurePopupContent` 中装入该根节点，其 `TemplatedParent` 显式设为 TreeSelect，保证跨视觉根路由可达。
- **单选 / 多选存在条件**（与上游一致：槽位集合统一，实例化随模式变化）：
  - 单选：`input` 命中模板内 `PART_SingleFilterInput`（过滤态可见）；`item` / `itemContent` /
    `itemRemove` 不实例化（无标签）；`placeholder` 在选择为空时可见。
  - 多选：`input` 命中 `SelectTagAwareTextBox` 运行时创建的行内搜索框；`item` 系随选中项集合在标签区
    创建与销毁；`placeholder` 在选择为空时可见。
  - `prefix` / `suffix` / `clear` / `popup.*` 在两种模式下行为一致；`clear` 仅在 `IsAllowClear=true`
    且存在可清除选择时可见。

## 3. 数量语义

`root`、`prefix`、`content`、`placeholder`、`input`、`suffix`、`clear`、`itemContent`、`itemRemove`、
`popup.root`、`popup.list` 均为静态模板节点或每弹层唯一容器，`Single`；状态变化（`IsMultiple`、
`IsFilterEnabled`、`IsAllowClear`、选择非空、disabled）只切换可见性或有效视觉值，不增删 marker。
`item` / `popup.listItem` 为 `Multiple`：`item` 随选中项集合在标签区创建与销毁；`popup.listItem`
条目数量随树数据与展开/过滤结果变化，任意嵌套层级统一注入 marker，容器回收复用时 marker 保持不变。

## 4. Selector 用法

生成的 Semantic Style 类型命名为 `TreeSelect<PartPathPascalCase>Style`，如
`TreeSelectPopupRootStyle`、`TreeSelectPopupListItemStyle`、`TreeSelectClearStyle`（命名空间
`AtomUI.Theme.Styling`，AXAML 命名空间 `https://atomui.net`）。`root` 不生成 Style 类型，owner 级
Setter 写在外层普通 Style 上。

推荐写法（owner 嵌套 Style + 语义 class，与 Gallery
`ShowCases/DataEntry/TreeSelect` 的「自定义语义结构的样式」示例一致，对齐上游 object / function styles 示例）：

```xml
<StackPanel.Styles>
    <Style Selector="atom|TreeSelect.semantic-styles-demo">
        <atom:TreeSelectPrefixStyle x:SetterTargetType="ContentPresenter">
            <Style Selector="^ atom|Icon">
                <Setter Property="StrokeBrush" Value="#1890FF" />
            </Style>
        </atom:TreeSelectPrefixStyle>
        <atom:TreeSelectSuffixStyle x:SetterTargetType="StackPanel">
            <Setter Property="TextElement.Foreground" Value="#1890FF" />
        </atom:TreeSelectSuffixStyle>
    </Style>
    <Style Selector="atom|TreeSelect.semantic-styles-filled-demo">
        <atom:TreeSelectPrefixStyle x:SetterTargetType="ContentPresenter">
            <Style Selector="^ atom|Icon">
                <Setter Property="StrokeBrush" Value="#722ED1" />
            </Style>
        </atom:TreeSelectPrefixStyle>
        <atom:TreeSelectSuffixStyle x:SetterTargetType="StackPanel">
            <Setter Property="TextElement.Foreground" Value="#722ED1" />
        </atom:TreeSelectSuffixStyle>
        <atom:TreeSelectPopupRootStyle x:SetterTargetType="Border">
            <Setter Property="BorderBrush" Value="#722ED1" />
            <Setter Property="BorderThickness" Value="1" />
        </atom:TreeSelectPopupRootStyle>
    </Style>
</StackPanel.Styles>
<atom:TreeSelect Classes="semantic-styles-demo"
                 ContentLeftAddOn="{antdicons:AntDesignIconProvider Kind=MehOutlined}" ... />
<atom:TreeSelect Classes="semantic-styles-demo semantic-styles-filled-demo"
                 StyleVariant="Filled"
                 ContentLeftAddOn="{antdicons:AntDesignIconProvider Kind=MehOutlined}" ... />
```

`prefix` 为文本内容时，`TreeSelectPrefixStyle` 上的 `TextElement.Foreground` 可着色前缀文本；`prefix`
为 `Icon` 时，AtomUI 图标由 `StrokeBrush` / `FillBrush` 驱动而非 `Foreground`，因此图标前缀在
`TreeSelectPrefixStyle` 内嵌套 `<Style Selector="^ atom|Icon">` 设置 `StrokeBrush` 着色（上游
`prefix.color` 依赖 `currentColor`，AtomUI 图标尚未桥接 `TextElement.Foreground` 到 `StrokeBrush`）。
`suffix` 的生成 Style 命中 `SelectHandle` 内的 `IconPresenter`，其 `IconBrush` 已桥接
`TextElement.Foreground`，故 `TreeSelectSuffixStyle.TextElement.Foreground` 可同时着色下拉箭头与
后缀内容。

`clear` / `prefix` / `suffix` 的生成 Style 由 `Nesting()` 经 scope 锚点跨越模板边界命中目标。不得使用
以下写法：

- `.semantic-root`、`PART_*`、Name selector、internal 类型或视觉祖先顺序作为应用主题契约。
- 把 `Border.semantic-popup-root` 等 `ContractType` 写入 Part 身份 selector。
- 直接复制 `/template/ .semantic-scope-*` route；scope class 只用于生成 Style 的 owner-relative 路由。
- 穿过 `ItemTemplate`、`LeftAddOnTemplate` 等用户模板继续匹配内部 Visual。

## 5. 定制边界

以下区域不属于 TreeSelect Semantic Part：

- **多选标签的内部实现节点**：`item` / `itemContent` / `itemRemove` 只承诺标签容器与模板内内容 /
  移除节点；标签的更多内部结构由共享 Tag 机制拥有，不经 TreeSelect 发布。
- **弹层树节点内部结构**：树节点的标题、切换器、图标、勾选框、指示器由 `TreeViewItem` 自身的
  `itemTitle` / `itemSwitcher` / `itemIcon` / `itemIndicator` Semantic Part 发布（对应上游
  `popup.itemTitle` / `popup.itemSwitcher` 槽位），不经 TreeSelect 重复声明；`popup.listItem` 只承诺
  节点容器本体。
- **展开图标 / loading 图标 / 勾选模式**：由 `SuffixIcon`、`SuffixLoadingIcon`、`TreeViewToggleType`
  等属性与行为 API 承担（上游同样以 prop 而非 semantic key 发布）。
- **空状态**：候选树的 empty indicator 由 `TreeView` 的 empty 展示 API 承担，不发布 Part。
- **弹层 Popup 宿主与定位**：候选 Popup 的定位、钉住打开、动画由共享 Popup 契约承担，`popup.root` 只
  覆盖弹层内容根 Border 的视觉。
- `PART_*` 名称、internal 类型、`.semantic-scope-*` 路由标记与模板层级。

默认主题不消费 `.semantic-*` selector；静态 marker 只提供应用样式命中点，不改变默认属性优先级或增加状态
订阅。Semantic Style 服从 Avalonia 原生属性优先级。

## 6. 兼容性与验证

删除或重命名 Part、修改 selector class / route、收窄 `ContractType`（含把公共基类承诺收窄为具体实现
类型）、改变 cardinality，或让任一内置模板变体缺少 marker，均属于公共主题契约变更。

与上游 antd TreeSelect 的对照差异（有意保持）：

- 上游 `TreeSelectSemanticType` 未声明 `clear` 槽（清除按钮样式归入 `suffix` 描述）；AtomUI Select
  家族将 `clear` 作为独立 Part 发布（物理节点在共享 `SelectHandle` 模板内），TreeSelect 保持一致。
- 上游 `popup` 分组使用 `item` / `itemTitle` / `itemSwitcher`；AtomUI 采用 Select 家族统一的
  `popup.list` / `popup.listItem`，树节点内部槽位由 `TreeViewItem` 契约的
  `itemTitle` / `itemSwitcher` / `itemIcon` / `itemIndicator` 覆盖。
- 上游单选/多选共用同一槽位集合，仅实例化范围不同；AtomUI 行为一致（见 §2）。

验证至少覆盖：

- owner descriptor 只包含 §1 的 13 个 Part，字段值与本文一致。
- `TreeSelectTheme.axaml` 携带 `prefix` / `content` / `placeholder` / `input` / `suffix` /
  `popup.root` 的静态 marker 与 `clear` / `item` / 弹层路由所需的 scope 锚点；
  `SelectHandleTheme.axaml` 携带 `clear` marker；`TagTheme.axaml` 携带 `itemContent` /
  `itemRemove` marker。
- `item` / `input`（多选）/ `popup.list` / `popup.listItem` 的 marker 在运行时创建路径注入
  （`SelectTagAwareTextBox` 标签与搜索框创建、`TreeSelect.EnsurePopupContent` 候选树创建、
  `TreeViewSelectTreeViewItem` 构造）。
- 生成的 `TreeSelect*Style` 可编译并命中目标节点（见
  `tests/AtomUI.Desktop.Controls.Tests/TreeSelect/TreeSelectSemanticPartTests.cs`）。
- 弹层展开与嵌套层级（`IsDefaultExpandAll`）下 `popup.listItem` marker 覆盖子级容器；重开与模板重应用
  后弹层部件 marker 保持。
- 状态变化（单选/多选、过滤、`IsAllowClear`、disabled）不改变 marker 身份与数量语义。
- 默认主题不消费 `.semantic-*`，未声明用户 Semantic Style 时不增加 selector activator。
- Gallery Semantic Parts Tab 延迟创建 Preview，弹层钉住常开并以多选预选呈现标签，13 个 Part 均可解析
  高亮。
- descriptor、生成 Style 与 NativeAOT 路径使用编译期生成数据，不依赖运行时反射或 VisualTree 扫描。
