# Cascader Semantic Part 契约

本文档定义 Cascader 控件公开的 Semantic Part、Selector、类型约束、数量语义和定制边界。控件整体设计见
[Cascader 桌面版架构设计](overview.md)，真实模板、marker 映射与状态流见
[Cascader 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Semantic Parts

Cascader 是唯一 Semantic owner，公开 13 个 Semantic Part（与上游 Cascader 的 Semantic Part
语义对齐：`item` / `itemContent` / `itemRemove` 对应上游多选标签的 `item` 分组，`popup.*` 对应上游
`popup` 分组的 `root` / `list` / `listItem`）。声明位于 `Cascader.SemanticParts.cs` partial 文件。

触发区部件的 marker 位于 `CascaderTheme.axaml` 宿主模板内：`prefix` / `suffix` 借用共享
`AddOnDecoratedBoxTheme` 的 `.semantic-scope-prefix` / `.semantic-scope-suffix` scope 锚点路由到宿主模板
投影给 decorated box 的内容节点（与 LineEdit 同构）；`content` / `placeholder` / `input` 直接标注宿主模板
节点。`clear` / `item` / `itemContent` / `itemRemove` 声明 `CrossNestedOwners=true`：`clear` 的物理节点在
共享 `SelectHandle` 自有模板内；多选标签的物理节点在共享标签机制内——`item` 的标记由
`SelectTagAwareTextBox` 在标签容器创建时注入，`itemContent` / `itemRemove` 的标记位于共享 `TagTheme`
模板（`SelectTag : Tag` 复用其模板），二者经 `item` 部件（RuntimeCreated，ContractType=`Tag`）承转主题链
完成校验。弹层三部件位于 owner 自有的
Popup 模板内：`popup.root` 标注在宿主模板节点上，`popup.list` 的根列与过滤列表 marker 位于
`CascaderViewTheme.axaml`，`popup.list` 子级列与 `popup.listItem` 的 marker 在运行时容器创建路径注入。

#### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `Cascader` |
| Part | `root` |
| Selector | Cascader 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `Cascader` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | Cascader owner |
| 职责 | Cascader root 是数据源、选择、过滤、弹层与状态的组织边界。 |
| 相关 API | 全部 Cascader public API |
| 相关 Token | CascaderToken、SharedToken |
| 稳定性 | stable since 6.2.0 |

**根边框与背景定制：** 可见外框由共享 `AddOnDecoratedBox` 帧绘制，其状态机拥有 `BorderBrush` 与
`Background`。owner 的 `BorderBrush` / `Background` 会以 LocalValue 中继到帧节点生效（对齐上游 root
border/background 语义与输入族 `AbstractTextInput.RelayRootSurfaceBrush` 行为），因此在 owner-scoped Style
里直接写根 `Setter Property="BorderBrush"` / `Property="Background"` 即可定制外框：

```xml
<Style Selector="atom|Cascader.semantic-root-border">
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
| Owner | `Cascader` |
| Part | `prefix` |
| Selector | `.semantic-prefix` |
| SelectorRoute | `/template/ .semantic-scope-input /template/ .semantic-scope-prefix > .semantic-prefix` |
| Style Type | `CascaderPrefixStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `CascaderTheme.axaml` 中投影给 `CascaderAddOnDecoratedBox.ContentLeftAddOn` 的 `AddOnContentPresenter`（经 `$parent[atom:Cascader]` 编译绑定呈现公共 API 值） |
| 职责 | 选择框内容前缀区域，承载 `ContentLeftAddOn` 用户内容，在内容框内联展示。 |
| 相关 API | `ContentLeftAddOn`、`ContentLeftAddOnTemplate` |
| 相关 Token | SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `content`

| 字段 | 值 |
| --- | --- |
| Owner | `Cascader` |
| Part | `content` |
| Selector | `.semantic-content` |
| SelectorRoute | `/template/ .semantic-content` |
| Style Type | `CascaderContentStyle` |
| ContractType | `Panel` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `CascaderTheme.axaml` 中 decorated box 内容面板 |
| 职责 | 选择内容面板，承载占位符、单选结果文本、搜索输入与多选标签盒。 |
| 相关 API | `PlaceholderText`、`SelectedOptionPath`、`EffectiveSelectedOptions` |
| 相关 Token | SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `placeholder`

| 字段 | 值 |
| --- | --- |
| Owner | `Cascader` |
| Part | `placeholder` |
| Selector | `.semantic-placeholder` |
| SelectorRoute | `/template/ .semantic-content > .semantic-placeholder` |
| Style Type | `CascaderPlaceholderStyle` |
| ContractType | `TextBlock` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `CascaderTheme.axaml` 中 `PlaceholderText` |
| 职责 | 未选择任何项时显示的占位符文本。 |
| 相关 API | `PlaceholderText`、`PlaceholderForeground` |
| 相关 Token | SharedToken（ColorTextPlaceholder） |
| 稳定性 | stable since 6.2.0 |

#### `input`

| 字段 | 值 |
| --- | --- |
| Owner | `Cascader` |
| Part | `input` |
| Selector | `.semantic-input` |
| SelectorRoute | `/template/ .semantic-content > .semantic-input` |
| Style Type | `CascaderInputStyle` |
| ContractType | `TextBox` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `CascaderTheme.axaml` 中 `PART_SingleFilterInput`（internal `SelectFilterTextBox`，公共契约承诺 Avalonia `TextBox`） |
| 职责 | 过滤模式（`IsFilterEnabled`）下渲染的搜索输入框；非过滤态隐藏但模板节点存在。 |
| 相关 API | `IsFilterEnabled`、`Filter`、`FilterValue` |
| 相关 Token | SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `suffix`

| 字段 | 值 |
| --- | --- |
| Owner | `Cascader` |
| Part | `suffix` |
| Selector | `.semantic-suffix` |
| SelectorRoute | `/template/ .semantic-scope-input /template/ .semantic-scope-suffix > .semantic-suffix` |
| Style Type | `CascaderSuffixStyle` |
| ContractType | `StackPanel` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `CascaderTheme.axaml` 中 `ContentRightAddOn` StackPanel |
| 职责 | 内容后缀区域，承载最大数量指示、用户 `ContentRightAddOn` 与选择 handle。 |
| 相关 API | `ContentRightAddOn`、`ContentRightAddOnTemplate`、`MaxCount` |
| 相关 Token | SharedToken `ColorTextQuaternary`（默认前景） |
| 稳定性 | stable since 6.2.0 |

suffix 区默认前景色为 `ColorTextQuaternary`，下拉指示箭头（SelectHandle 内图标）与后缀内容跟随该颜色：
在 `CascaderSuffixStyle` 上设置 `TextElement.Foreground` 即可同时定制箭头与后缀内容颜色（例如
`#1890FF` 蓝色箭头，与上游语义样式示例一致）。清除按钮、表单校验反馈与最大数量指示各自维护颜色，
不随该前景色变化。

#### `clear`

| 字段 | 值 |
| --- | --- |
| Owner | `Cascader` |
| Part | `clear` |
| Selector | `.semantic-clear` |
| SelectorRoute | `>> .semantic-scope-handle /template/ .semantic-clear` |
| Style Type | `CascaderClearStyle` |
| ContractType | `Button` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `SelectHandleTheme.axaml` 中 `PART_ClearButton`（internal `InputClearIconButton`，公共契约承诺 Avalonia `Button`） |
| 职责 | 后缀 handle 内的清除按钮，`IsAllowClear` 启用、非空选择且输入区 hover / pressed 时可见。 |
| 相关 API | `IsAllowClear`、`Clear()` |
| 相关 Token | SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `item`

| 字段 | 值 |
| --- | --- |
| Owner | `Cascader` |
| Part | `item` |
| Selector | `.semantic-item` |
| SelectorRoute | `>> .semantic-scope-tags >> .semantic-item` |
| Style Type | `CascaderItemStyle` |
| ContractType | `Tag` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | internal `SelectTag` 实例（`SelectTagAwareTextBox` 创建标签容器时注入 marker），公共契约承诺 public `Tag` |
| 职责 | 多选模式下选择器中的选中标签。 |
| 相关 API | `IsMultiple`、`SelectedOptions`、`MaxTagCount`、`IsResponsiveTagMode` |
| 相关 Token | SelectToken（MultipleItemBg、MultipleItemHeight*） |
| 稳定性 | stable since 6.2.0 |

#### `itemContent`

| 字段 | 值 |
| --- | --- |
| Owner | `Cascader` |
| Part | `itemContent` |
| Selector | `.semantic-item-content` |
| SelectorRoute | `>> .semantic-scope-tags >> .semantic-item /template/ .semantic-item-content` |
| Style Type | `CascaderItemContentStyle` |
| ContractType | `TextBlock` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `TagTheme.axaml` 中 `TagTextLabel`（`SelectTag : Tag` 复用其模板） |
| 职责 | 选中标签内的文本内容。 |
| 相关 API | 无（随 tag 展示） |
| 相关 Token | TagToken（TagLineHeight） |
| 稳定性 | stable since 6.2.0 |

#### `itemRemove`

| 字段 | 值 |
| --- | --- |
| Owner | `Cascader` |
| Part | `itemRemove` |
| Selector | `.semantic-item-remove` |
| SelectorRoute | `>> .semantic-scope-tags >> .semantic-item /template/ .semantic-item-remove` |
| Style Type | `CascaderItemRemoveStyle` |
| ContractType | `IconButton` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `TagTheme.axaml` 中 `PART_CloseButton`（`IsClosable=false` 时隐藏但模板节点存在） |
| 职责 | 选中标签内的移除按钮。 |
| 相关 API | `IsClosable`（经标签机制） |
| 相关 Token | SharedToken（IconSizeXS） |
| 稳定性 | stable since 6.2.0 |

#### `popup.root`

| 字段 | 值 |
| --- | --- |
| Owner | `Cascader` |
| Part | `popup.root` |
| Selector | `.semantic-popup-root` |
| SelectorRoute | `/template/ .semantic-popup-root` |
| Style Type | `CascaderPopupRootStyle` |
| ContractType | `Border` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `CascaderTheme.axaml` 中 `PopupFrame` |
| 职责 | 级联菜单弹层根 `Border`，可定制弹层边框、背景与宽度。 |
| 相关 API | `MaxPopupHeight`、`EffectivePopupWidth`、`PopupContentPadding` |
| 相关 Token | PopupTokenResource（PopupCornerRadius）、SharedToken（ColorBgElevated） |
| 稳定性 | stable since 6.2.0 |

#### `popup.list`

| 字段 | 值 |
| --- | --- |
| Owner | `Cascader` |
| Part | `popup.list` |
| Selector | `.semantic-popup-list` |
| SelectorRoute | `/template/ .semantic-popup-root > .semantic-scope-view /template/ .semantic-scope-frame >> .semantic-popup-list` |
| Style Type | `CascaderPopupListStyle` |
| ContractType | `Control` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | internal `CascaderViewLevelList`（根列为 `CascaderViewTheme.axaml` 静态节点，子级列运行时创建）与 `CascaderViewFilterList`（过滤替代实现），公共契约承诺 `Control` |
| 职责 | 弹层内的一条级联菜单列；过滤模式下由过滤结果列表替代，同一时刻至多一类可见。 |
| 相关 API | `ExpandTrigger`、`Filter` |
| 相关 Token | CascaderToken（ControlItemWidth、MenuPadding） |
| 稳定性 | stable since 6.2.0 |

#### `popup.listItem`

| 字段 | 值 |
| --- | --- |
| Owner | `Cascader` |
| Part | `popup.listItem` |
| Selector | `.semantic-popup-list-item` |
| SelectorRoute | `/template/ .semantic-popup-root > .semantic-scope-view /template/ .semantic-scope-frame >> .semantic-popup-list-item` |
| Style Type | `CascaderPopupListItemStyle` |
| ContractType | `TemplatedControl` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | public `CascaderViewItem`（层级列条目）与 internal `CascaderViewFilterListItem`（过滤结果条目），公共祖先承诺 `TemplatedControl` |
| 职责 | 菜单列中的单个选项条目，均为运行时容器创建，虚拟化回收复用时 marker 保持。 |
| 相关 API | `OptionTemplate`、`IsAllowSelectParent` |
| 相关 Token | CascaderToken（OptionPadding、HeaderHeight、OptionSelectedBg） |
| 稳定性 | stable since 6.2.0 |

`ContractType` 不参与 selector 匹配，只约束 `x:SetterTargetType` 与兼容性下界；实现节点为 internal 类型时，
公共契约承诺到最低 public 基类（`input`→`TextBox`、`item`→`Tag`、`popup.list`→`Control`、
`popup.listItem`→`TemplatedControl`）。

## 2. 职责与存在条件

- `prefix` / `suffix` 的路由借用共享 `AddOnDecoratedBoxTheme` 的 `.semantic-scope-prefix` /
  `.semantic-scope-suffix` 锚点，marker 本体在宿主模板内，不声明 `CrossNestedOwners`。
- `clear` 的锚点 `.semantic-scope-handle` 位于宿主模板 SelectHandle 节点；`>>` 首步进覆盖锚点节点位于
  owner 模板属性值子树（无 `TemplatedParent` 传播）的场景，生成器沿锚点类型链在 `SelectHandleTheme`
  校验 marker 数量与类型。
- `item` / `popup.list` / `popup.listItem` 为 `RuntimeCreated=true`：标签容器、子级菜单列、层级条目与
  过滤条目均在运行时创建，生成器豁免宿主模板 marker 校验，由控件行为测试兜底。
- `itemContent` / `itemRemove` 的路由经中间跳步 `.semantic-item` 承转：该跳步是同控件已声明的
  RuntimeCreated 部件 `item`（ContractType=`Tag`），生成器以 sibling 部件的 ContractType 继续解析主题链
  至 `TagTheme` 完成静态校验；运行时按 descendant + `/template/` 命中标签实例的模板节点。

## 3. 数量语义

`root`、`prefix`、`content`、`placeholder`、`input`、`suffix`、`clear`、`itemContent`、`itemRemove`、
`popup.root` 均为静态模板节点，`Single`；状态变化（`IsMultiple`、`IsFilterEnabled`、`IsAllowClear`、选择
非空、disabled、`IsClosable`）只切换可见性或有效视觉值，不增删 marker。`item` / `popup.list` /
`popup.listItem` 为 `Multiple`：`item` 随选中项集合在标签区创建与销毁；`popup.list` 弹层打开时根列 +
已展开子级列同时存在，收起或 `ResetInteractionState()` 后只剩根列，过滤态下由过滤列表替代；
`popup.listItem` 条目数量随选项与过滤结果变化，虚拟化回收复用容器时 marker 保持不变。

## 4. Selector 用法

生成的 Semantic Style 类型命名为 `Cascader<PartPathPascalCase>Style`，如 `CascaderPopupRootStyle`、
`CascaderPopupListItemStyle`、`CascaderClearStyle`（命名空间 `AtomUI.Theme.Styling`，AXAML 命名空间
`https://atomui.net`）。`root` 不生成 Style 类型，owner 级 Setter 写在外层普通 Style 上。

推荐写法（owner 嵌套 Style + 语义 class，与 Gallery
`ShowCases/DataEntry/Cascader` 的「自定义语义结构的样式」示例一致）：

```xml
<StackPanel.Styles>
    <Style Selector="atom|Cascader.semantic-styles-demo">
        <atom:CascaderPopupRootStyle x:SetterTargetType="Border">
            <Setter Property="BorderBrush" Value="#1890FF" />
            <Setter Property="BorderThickness" Value="1" />
        </atom:CascaderPopupRootStyle>
        <atom:CascaderPopupListItemStyle x:SetterTargetType="TemplatedControl">
            <Setter Property="TextElement.Foreground" Value="#272727" />
        </atom:CascaderPopupListItemStyle>
        <atom:CascaderPlaceholderStyle x:SetterTargetType="TextBlock">
            <Setter Property="Foreground" Value="#BFBFBF" />
        </atom:CascaderPlaceholderStyle>
    </Style>
</StackPanel.Styles>
<atom:Cascader Classes="semantic-styles-demo" ... />
```

`clear` / `prefix` / `suffix` 的生成 Style 由 `Nesting()` 经 scope 锚点跨越模板边界命中目标。不得使用
以下写法：

- `.semantic-root`、`PART_*`、Name selector、internal 类型或视觉祖先顺序作为应用主题契约。
- 把 `Border.semantic-popup-root` 等 `ContractType` 写入 Part 身份 selector。
- 直接复制 `/template/ .semantic-scope-*` route；scope class 只用于生成 Style 的 owner-relative 路由。
- 穿过 `OptionTemplate`、`LeftAddOnTemplate` 等用户模板继续匹配内部 Visual。

## 5. 定制边界

以下区域不属于 Cascader Semantic Part：

- **多选标签的内部实现节点**：`item` / `itemContent` / `itemRemove` 只承诺标签容器与模板内内容 /
  移除节点；标签的更多内部结构（图标、边框 frame 等）由共享 Tag 机制拥有，不经 Cascader 发布。
- **展开图标 / loading 图标 / 勾选框**：由 `ExpandIcon`、`LoadingIcon` 属性与 `IsMultiple` 行为 API
  承担（上游同样以 prop 而非 semantic key 发布）。
- **空状态**：`CascaderView` 的 empty indicator 由 `EmptyIndicator` / `EmptyIndicatorTemplate` API
  承担，不发布 Part。
- **弹层 Popup 宿主与定位**：候选 Popup 的定位、钉住打开、动画由共享 Popup 契约承担，`popup.root` 只覆盖
  弹层内容根 Border 的视觉。
- `PART_*` 名称、internal 类型、`.semantic-scope-*` 路由标记与模板层级。

默认主题不消费 `.semantic-*` selector；静态 marker 只提供应用样式命中点，不改变默认属性优先级或增加状态
订阅。Semantic Style 服从 Avalonia 原生属性优先级。

## 6. 兼容性与验证

删除或重命名 Part、修改 selector class / route、收窄 `ContractType`（含把公共基类承诺收窄为具体实现
类型）、改变 cardinality，或让任一内置模板变体缺少 marker，均属于公共主题契约变更。

验证至少覆盖：

- owner descriptor 只包含 §1 的 13 个 Part，字段值与本文一致。
- `CascaderTheme.axaml` 携带 `prefix` / `content` / `placeholder` / `input` / `suffix` / `popup.root`
  的静态 marker 与 `clear` / `item` / 弹层路由所需的 scope 锚点；`CascaderViewTheme.axaml` 携带
  `popup.list` 静态 marker（根列、过滤列表）；`SelectHandleTheme.axaml` 携带 `clear` marker；
  `TagTheme.axaml` 携带 `itemContent` / `itemRemove` marker。
- `item` / `popup.list` 子级列 / `popup.listItem` 的 marker 在运行时容器创建路径注入
  （`SelectTagAwareTextBox` 标签创建、`CascaderView` 子级列创建、`CascaderViewLevelList` /
  `CascaderViewFilterList` 容器创建）。
- 生成的 `Cascader*Style` 可编译并命中目标节点（见
  `tests/AtomUI.Desktop.Controls.Tests/Cascader/CascaderSemanticPartTests.cs`）。
- 钉住弹层在打开前抑制 light-dismiss 遮罩；重开与数据源重置后弹层部件 marker 保持（见
  `tests/AtomUI.Desktop.Controls.Tests/Cascader/CascaderPinnedPopupTests.cs`）。
- 状态变化（单选/多选、过滤、`IsAllowClear`、disabled）不改变 marker 身份与数量语义。
- 默认主题不消费 `.semantic-*`，未声明用户 Semantic Style 时不增加 selector activator。
- Gallery Semantic Parts Tab 延迟创建 Preview，弹层钉住常开并以多选预选呈现标签，13 个 Part 均可解析
  高亮。
- descriptor、生成 Style 与 NativeAOT 路径使用编译期生成数据，不依赖运行时反射或 VisualTree 扫描。
