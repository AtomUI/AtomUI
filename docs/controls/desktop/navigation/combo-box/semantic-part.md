# ComboBox Semantic Part 契约

本文档定义 ComboBox 控件公开的 Semantic Part、Selector、类型约束、数量语义和定制边界。控件整体设计见
[ComboBox 桌面版架构设计](overview.md)，真实模板、marker 映射与状态流见
[ComboBox 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 0. 准入定性（必读）

ComboBox 的 Semantic Part 改造**不是一次 §2.1 准入 Gate 通过**，本契约不得被引用为 Gate 通过的先例。

上游 Ant Design 6.6.0 稳定发布源码中**没有**公开的 `ComboBox` owner，§2.1 第 1 条对 ComboBox 不成立；原排除判定中
「`Select` 的 internal combobox mode 不能作为公开 owner」这一条**继续有效**。本次纳入的依据是用户指令，加上
ComboBox 自身就是职责完整、可独立定制的 public owner：

- ComboBox 直接派生 Avalonia `ComboBox`，**不复用** `Select` 的 internal combobox mode，也不共享 AtomUI
  `AbstractSelect` / `AbstractAutoComplete` 基类。
- 它自有输入框装饰、下拉 handle、模板内 Popup 和候选容器创建路径，具备独立发布语义区域的事实基础。

因此本契约的区域分组与命名**参考**上游 `Select` 已公开的 `classNames` / `styles` 分组（`prefix` / `suffix` /
`placeholder` / `input` / `popup` 等词汇），但只发布 ComboBox 自身确实拥有的区域，**不为对齐上游而发明 ComboBox
不存在的键**。ComboBox 是本仓库唯一的「非 Gate 通过」准入例外，**不得被引用为 Gate 通过的先例**。范围记录见
[全量改造设计 §2.4](../../../../superpowers/specs/2026-08-12-semantic-part-control-rollout-design.md) 的 2026-09-16 范围变更段。

## 1. Semantic Parts

ComboBox 是唯一 Semantic owner，公开 12 个 Semantic Part：隐式 `root` 加 11 个非 root 部件（`prefix`、`frame`、
`content`、`placeholder`、`input`、`suffix`、`indicator`、`popup.root`、`popup.list`、`popup.listItem`、`popup.empty`）。
11 个非 root 部件各生成一个 `ComboBox*Style` 类型，`root` 不生成。声明位于 `ComboBox.SemanticParts.cs` partial 文件。

触发区部件的 marker 位于 `ComboBoxTheme.axaml` 宿主模板内：`prefix` / `suffix` 借用共享
`AddOnDecoratedBoxTheme` 的 `.semantic-scope-prefix` / `.semantic-scope-suffix` scope 锚点路由到宿主模板
投影给 decorated box 的内容节点（与 `Select` / `TreeSelect` / `Cascader` 同构）；`content` / `placeholder` /
`input` 直接标注宿主模板节点。`indicator` 声明 `CrossNestedOwners=true`：其物理节点在 ComboBox 自有的
internal `ComboBoxHandle` 模板内，经宿主模板的 `.semantic-scope-handle` 锚点跨入。弹层四部件位于 owner 自有的
模板内 Popup：`popup.root` 标注在宿主模板 `PopupFrame` 静态节点上，`popup.list` / `popup.empty` 是
`PopupFrame` 内的静态节点，`popup.listItem` 的 marker 在运行时容器创建路径注入。

#### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `ComboBox` |
| Part | `root` |
| Selector | ComboBox 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `ComboBox` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| CrossNestedOwners | `false` |
| AtomUI 节点 | ComboBox owner |
| 职责 | ComboBox root 是数据源、选择、过滤、弹层与输入框状态的组织边界。 |
| 相关 API | 全部 ComboBox public API |
| 相关 Token | ComboBoxToken、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `prefix`

| 字段 | 值 |
| --- | --- |
| Owner | `ComboBox` |
| Part | `prefix` |
| Selector | `.semantic-prefix` |
| SelectorRoute | `/template/ .semantic-scope-input /template/ .semantic-scope-prefix > .semantic-prefix` |
| Style Type | `ComboBoxPrefixStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| CrossNestedOwners | `false` |
| AtomUI 节点 | `ComboBoxTheme.axaml` 中投影给 `AddOnDecoratedBox.ContentLeftAddOn` 的 `AddOnContentPresenter`（经 `$parent[atom:ComboBox]` 编译绑定呈现公共 API 值） |
| 职责 | 内容框内联前缀区域，承载 `ContentLeftAddOn` 用户内容。 |
| 相关 API | `ContentLeftAddOn`、`ContentLeftAddOnTemplate` |
| 相关 Token | SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `frame`

| 字段 | 值 |
| --- | --- |
| Owner | `ComboBox` |
| Part | `frame` |
| Selector | `.semantic-frame` |
| SelectorRoute | `/template/ .semantic-scope-input /template/ .semantic-frame` |
| Style Type | `ComboBoxFrameStyle` |
| ContractType | `PixelAlignedBorder` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| CrossNestedOwners | `true` |
| AtomUI 节点 | 共享 `AddOnDecoratedBoxTheme.axaml` 中 `AddOnDecoratedBoxContentFrame`（`Name="PART_ContentFrame"`）；锚点 `.semantic-scope-input` 位于 ComboBox 宿主模板的 `AddOnDecoratedBox` 节点 |
| 职责 | **输入框边框盒**——outlined / filled / underlined 变体的最内层边框盒，决定输入框的边框颜色、边框宽度、圆角与背景。这是应用最常需要的定制点（如把默认灰边框改成品牌色）。 |
| 相关 API | `StyleVariant`、`SizeType`、`Status`、`FormStatus`；内部 `EffectiveStatus` 驱动状态色 |
| 相关 Token | SharedToken（`ColorBorder`、`ColorPrimary`、`ColorError`、`ColorWarning`、`BorderRadius*`） |
| 稳定性 | stable since 6.2.0 |

#### `content`

| 字段 | 值 |
| --- | --- |
| Owner | `ComboBox` |
| Part | `content` |
| Selector | `.semantic-content` |
| SelectorRoute | `/template/ .semantic-content` |
| Style Type | `ComboBoxContentStyle` |
| ContractType | `Panel` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| CrossNestedOwners | `false` |
| AtomUI 节点 | `ComboBoxTheme.axaml` decorated box 内容面板（`PlaceholderText`、`SelectedContentPresenter`、`PART_EditableTextBox` 的公共父节点） |
| 职责 | 输入内容面板，承载占位符、非编辑态选中内容与编辑态输入框三个互斥/叠加节点。 |
| 相关 API | `PlaceholderText`、`SelectionBoxItem`、`SelectionBoxItemTemplate`、`IsEditable` |
| 相关 Token | SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `placeholder`

| 字段 | 值 |
| --- | --- |
| Owner | `ComboBox` |
| Part | `placeholder` |
| Selector | `.semantic-placeholder` |
| SelectorRoute | `/template/ .semantic-content > .semantic-placeholder` |
| Style Type | `ComboBoxPlaceholderStyle` |
| ContractType | `Avalonia.Controls.TextBlock` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| CrossNestedOwners | `false` |
| AtomUI 节点 | `ComboBoxTheme.axaml` 中 `PlaceholderText`（`atom:TextBlock`，基类为 `Avalonia.Controls.TextBlock`） |
| 职责 | 未选择任何项且非编辑态时显示的占位符文本。 |
| 相关 API | `PlaceholderText` |
| 相关 Token | SharedToken（节点以 `Opacity=0.3` 弱化，见定制边界） |
| 稳定性 | stable since 6.2.0 |

#### `input`

| 字段 | 值 |
| --- | --- |
| Owner | `ComboBox` |
| Part | `input` |
| Selector | `.semantic-input` |
| SelectorRoute | `/template/ .semantic-content > .semantic-input` |
| Style Type | `ComboBoxInputStyle` |
| ContractType | `Avalonia.Controls.TextBox` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| CrossNestedOwners | `false` |
| AtomUI 节点 | `ComboBoxTheme.axaml` 中 `PART_EditableTextBox`（internal `ComboBoxTextBox : Avalonia.Controls.TextBox`） |
| 职责 | `IsEditable=true` 时渲染的可编辑 / 过滤输入框；非编辑态节点仍存在但 `IsVisible=false`。 |
| 相关 API | `IsEditable`、`Text`、`IsFilterEnabled`、`FilterValue` |
| 相关 Token | SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `suffix`

| 字段 | 值 |
| --- | --- |
| Owner | `ComboBox` |
| Part | `suffix` |
| Selector | `.semantic-suffix` |
| SelectorRoute | `/template/ .semantic-scope-input /template/ .semantic-scope-suffix > .semantic-suffix` |
| Style Type | `ComboBoxSuffixStyle` |
| ContractType | `StackPanel` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| CrossNestedOwners | `false` |
| AtomUI 节点 | `ComboBoxTheme.axaml` 中 `ContentRightAddOn` 的 `StackPanel`（承载 `PART_ContentRightAddOnPresenter`、`PART_FormFeedBack`、`PART_ComboBoxHandle`） |
| 职责 | 内容框右侧后缀区，承载用户后缀内容、Form 校验反馈与下拉指示器。 |
| 相关 API | `ContentRightAddOn`、`ContentRightAddOnTemplate`、`FormFeedback` |
| 相关 Token | SharedToken；`TextElement.Foreground` 默认取 `ColorTextQuaternary`（共享 `AddOnDecoratedBoxTheme` 提供，为本 Part 的下界） |
| 稳定性 | stable since 6.2.0 |

#### `indicator`

| 字段 | 值 |
| --- | --- |
| Owner | `ComboBox` |
| Part | `indicator` |
| Selector | `.semantic-indicator` |
| SelectorRoute | `>> .semantic-scope-handle /template/ .semantic-indicator` |
| Style Type | `ComboBoxIndicatorStyle` |
| ContractType | `IconButton` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| CrossNestedOwners | `true` |
| AtomUI 节点 | `ComboBoxHandleTheme.axaml` 中 `PART_OpenIndicatorButton`（`atom:IconButton`，图标 `DownOutlined`）；锚点 `.semantic-scope-handle` 位于宿主模板的 `PART_ComboBoxHandle` |
| 职责 | 下拉展开指示器（箭头按钮）；同时是鼠标点击展开/收起的触发入口。 |
| 相关 API | `IsDropDownOpen`、`IsEnabled`、`IsMotionEnabled` |
| 相关 Token | ComboBoxToken（`HandleHoverColor` 驱动 hover 态箭头色） |
| 稳定性 | stable since 6.2.0 |

#### `popup.root`

| 字段 | 值 |
| --- | --- |
| Owner | `ComboBox` |
| Part | `popup.root` |
| Selector | `.semantic-popup-root` |
| SelectorRoute | `/template/ .semantic-popup-root` |
| Style Type | `ComboBoxPopupRootStyle` |
| ContractType | `Border` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| RuntimeCreated | `false` |
| CrossNestedOwners | `false` |
| AtomUI 节点 | `ComboBoxTheme.axaml` 中 `PART_Popup` 的直接子节点 `PopupFrame` |
| 职责 | 弹层框体，承载候选列表与空态，决定弹层圆角、背景与内边距边界。 |
| 相关 API | `MaxDropDownHeight`、`PopupContentPadding`（internal）、`EffectivePopupWidth`（internal） |
| 相关 Token | SharedToken（`ColorBgElevated`）、PopupToken（`PopupCornerRadius`） |
| 稳定性 | stable since 6.2.0 |

#### `popup.list`

| 字段 | 值 |
| --- | --- |
| Owner | `ComboBox` |
| Part | `popup.list` |
| Selector | `.semantic-popup-list` |
| SelectorRoute | `/template/ .semantic-popup-root >> .semantic-popup-list` |
| Style Type | `ComboBoxPopupListStyle` |
| ContractType | `Avalonia.Controls.ScrollViewer` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| RuntimeCreated | `false` |
| CrossNestedOwners | `false` |
| AtomUI 节点 | `ComboBoxTheme.axaml` 中 `PopupFrame` 内的 `atom:ScrollViewer`（内部为 `PART_ItemsPresenter`） |
| 职责 | 候选列表滚动区，`IsVisible` 与空态互斥，是候选项的排布与滚动边界。 |
| 相关 API | `MaxDropDownHeight`、`ItemsPanel`、`ScrollViewer.IsLiteMode`、`ScrollViewer.AllowAutoHide` |
| 相关 Token | SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `popup.listItem`

| 字段 | 值 |
| --- | --- |
| Owner | `ComboBox` |
| Part | `popup.listItem` |
| Selector | `.semantic-popup-list-item` |
| SelectorRoute | `/template/ .semantic-popup-root >> .semantic-popup-list-item` |
| Style Type | `ComboBoxPopupListItemStyle` |
| ContractType | `ComboBoxItem` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| RuntimeCreated | `true` |
| CrossNestedOwners | `false` |
| AtomUI 节点 | 运行时容器：`CreateContainerForItemOverride` / `PrepareContainerForItemOverride` 创建的 `ComboBoxItem` 实例本身（marker 不写在 `ComboBoxItemTheme.axaml` 模板内部节点上，理由见 §2） |
| 职责 | 单个候选项容器，承载项内容、选中视觉、hover/active 候选视觉、禁用态与过滤隐藏。 |
| 相关 API | `ItemsSource`、`ItemTemplate`、`SelectedItem`、`SelectedIndex`、`ItemHeight`（internal）、`OptionFontSize` |
| 相关 Token | ComboBoxToken（`ItemColor`、`ItemBgColor`、`ItemHoverColor`、`ItemHoverBgColor`、`ItemSelectedColor`、`ItemSelectedBgColor`、`ItemPadding`、`ItemMargin`） |
| 稳定性 | stable since 6.2.0 |

#### `popup.empty`

| 字段 | 值 |
| --- | --- |
| Owner | `ComboBox` |
| Part | `popup.empty` |
| Selector | `.semantic-popup-empty` |
| SelectorRoute | `/template/ .semantic-popup-root >> .semantic-popup-empty` |
| Style Type | `ComboBoxPopupEmptyStyle` |
| ContractType | `Border` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| RuntimeCreated | `false` |
| CrossNestedOwners | `false` |
| AtomUI 节点 | `ComboBoxTheme.axaml` 中 `PART_EmptyIndicator`（内含 `atom:Empty`） |
| 职责 | 生效过滤模式下无匹配项时显示的弹层空态区；`IsVisible` 由 `IsEffectiveEmptyVisible` 驱动，与 `popup.list` 互斥。**仅在 `IsEditable=true` + `IsFilterEnabled=true` + 存在过滤值 + 无匹配项时可见**：未开启过滤时 `IsEffectiveEmptyVisible` 被强制为 `false`，此时弹层显示的是空的列表区而非空态区，空态节点仍存在但隐藏。 |
| 相关 API | `IsEditable`、`IsFilterEnabled`、`Text` / `FilterValue`；`IsEffectiveEmptyVisible`（internal，由上述状态推导） |
| 相关 Token | SharedToken（`Padding`） |
| 稳定性 | stable since 6.2.0 |

## 2. 职责与存在条件

- `prefix` / `suffix` 的路由借用共享 `AddOnDecoratedBoxTheme` 的 `.semantic-scope-prefix` /
  `.semantic-scope-suffix` 锚点：这两个 scope class 标注的是 `AddOnDecoratedBox` 自身模板内的
  `PART_ContentLeftAddOn` / `PART_ContentRightAddOn` presenter，因此不声明 `CrossNestedOwners`。路由首段所需的
  `.semantic-scope-input` 由 ComboBox 在自身 `AddOnDecoratedBox` 节点上承载。
- `frame` 的 marker 位于**共享** `AddOnDecoratedBoxTheme.axaml` 的 `PART_ContentFrame` 节点，因此声明
  `CrossNestedOwners=true`：`.semantic-scope-input` 锚点之后的第二个 `/template/` 进入 AddOnDecoratedBox 自身模板。
  这与 `ToolTip` 的 `container` / `arrow` 由共享 `ArrowDecoratedBoxTheme` 承载是同一形态（先例）。共享主题中的 marker
  不新增任何节点，只标注既有 `PART_ContentFrame`，因此 `Select` / `LineEdit` 等复用同一主题的控件也会带上该 class；
  它们各自的 `frame` 契约（是否发布、route 如何写）由各自控件决定，互不冲突（见下方定制边界）。
- `indicator` 的路由首段所需的 `.semantic-scope-handle` 必须由 ComboBox 在自身 `PART_ComboBoxHandle` 节点上新增
  （当前没有）；该锚点节点被赋给 `AddOnDecoratedBox.ContentRightAddOn` 属性值子树，不参与 `TemplatedParent` 传播，
  所以路由以 `>>` 起步。
- `prefix` 要求宿主模板把 `ContentLeftAddOn` 从属性 `TemplateBinding` 投影为元素形式的
  `AddOnContentPresenter`（marker 挂在该 presenter 上），与 `Select` / `LineEdit` / `InfoPickerInput` 的既有写法一致；
  `suffix` 的 `ContentRightAddOn` 已是元素形式（`StackPanel`），无需改造。详见
  [实现原理 §5.1](implementation.md) 的 marker 归属表与新增锚点清单。
- `popup.listItem` 是 `RuntimeCreated=true`：marker 在容器创建路径注入。ComboBox 的
  `NeedsContainerOverride` 对用户显式提供的 `ComboBoxItem` 返回 `false`，因此必须在
  `CreateContainerForItemOverride`（新建路径）与 `PrepareContainerForItemOverride`（复用/外部提供路径）两处
  幂等注入，`ClearContainerForItemOverride` 不摘除 marker（与 `TabStrip`、`Breadcrumb`、`TreeView` 同型）。
- `placeholder` / `input` / `popup.list` / `popup.empty` 的存在条件是**状态性隐藏而非节点缺失**：`IsVisible` 在
  `IsEditable` / `IsEffectiveEmptyVisible` 变化时切换，节点与 marker 本身常驻，因此数量语义为 `Single`。
  `IsEffectiveEmptyVisible` 只在生效过滤模式（`IsEditable` + `IsFilterEnabled` + 过滤值 + 无匹配项）下为真；
  未开启过滤时它被强制为 `false`，因此「候选为空」并不等于「空态可见」。
- `popup.root` 是 `PART_Popup` 的静态直接子节点；Avalonia 12 的模板内 Popup 为内容传播 `TemplatedParent`，
  故 `popup.*` 在弹层打开后仍可由 owner-scoped Style 命中（系统设计 §9.1）。
- 弹层四部件只有在弹层打开时才进入真实视觉树；未打开时生成 Style 不命中任何节点，属预期。

## 3. 数量语义

`root`、`prefix`、`frame`、`content`、`placeholder`、`input`、`suffix`、`indicator`、`popup.root`、`popup.list`、
`popup.empty` 均为每模板唯一节点，`Single`。状态变化（`IsEditable`、`IsFilterEnabled`、`SelectedItem` 非空、
disabled、`Status`/`FormStatus`、`IsDropDownOpen`、过滤结果不同）只切换可见性或有效视觉值，**不增删 marker**。

`popup.listItem` 为 `Multiple`：条目数量随 `ItemsSource` 与过滤结果变化；虚拟化或容器回收复用容器时 marker
保持稳定，容器从一个 owner 转移到另一个 owner 时不遗留旧状态。

## 4. Selector 用法

生成的 Semantic Style 类型命名为 `ComboBox<PartPathPascalCase>Style`：`ComboBoxPrefixStyle`、`ComboBoxFrameStyle`、
`ComboBoxContentStyle`、`ComboBoxPlaceholderStyle`、`ComboBoxInputStyle`、`ComboBoxSuffixStyle`、
`ComboBoxIndicatorStyle`、`ComboBoxPopupRootStyle`、`ComboBoxPopupListStyle`、`ComboBoxPopupListItemStyle`、
`ComboBoxPopupEmptyStyle`（命名空间 `AtomUI.Theme.Styling`，AXAML 命名空间 `https://atomui.net`）。
`root` 不生成 Style 类型，owner 级 Setter 写在外层普通 Style 上。

推荐写法（owner 嵌套 Style + 语义 class）：

```xml
<StackPanel.Styles>
    <Style Selector="atom|ComboBox.semantic-styles-demo">
        <atom:ComboBoxPrefixStyle x:SetterTargetType="ContentPresenter">
            <Style Selector="^ atom|Icon">
                <Setter Property="StrokeBrush" Value="#1890FF" />
            </Style>
        </atom:ComboBoxPrefixStyle>
        <atom:ComboBoxSuffixStyle x:SetterTargetType="StackPanel">
            <Setter Property="TextElement.Foreground" Value="#1890FF" />
        </atom:ComboBoxSuffixStyle>
        <atom:ComboBoxIndicatorStyle x:SetterTargetType="IconButton">
            <Setter Property="IconBrush" Value="#1890FF" />
        </atom:ComboBoxIndicatorStyle>
        <atom:ComboBoxPopupRootStyle x:SetterTargetType="Border">
            <Setter Property="BorderBrush" Value="#722ED1" />
            <Setter Property="BorderThickness" Value="1" />
        </atom:ComboBoxPopupRootStyle>
    </Style>
</StackPanel.Styles>
<atom:ComboBox Classes="semantic-styles-demo"
               ContentLeftAddOn="{antdicons:AntDesignIconProvider Kind=MehOutlined}" ... />
```

`prefix` 为文本内容时，`ComboBoxPrefixStyle` 上的 `TextElement.Foreground` 可着色前缀文本；`prefix` 为 `Icon`
时，AtomUI 图标由 `StrokeBrush` / `FillBrush` 驱动而非 `Foreground`，因此图标前缀在 `ComboBoxPrefixStyle` 内
嵌套 `<Style Selector="^ atom|Icon">` 设置 `StrokeBrush` 着色（与 `Select` 一致，上游 `prefix.color` 依赖
`currentColor`，AtomUI 图标尚未桥接 `TextElement.Foreground` 到 `StrokeBrush`）。

`placeholder` 的 `Opacity=0.3` 与 `suffix` 容器的 `TextElement.Foreground` 默认值来自内置主题：用户
Semantic Style 在目标节点上以更高优先级覆写，不需要也不应修改内置主题来定制。

不得使用以下写法：

- `.semantic-root`、`PART_*`、Name selector、internal 类型或视觉祖先顺序作为应用主题契约。
- 把 `Border.semantic-popup-root` 等 `ContractType` 写入 Part 身份 selector。
- 直接复制 `/template/ .semantic-scope-*` route；scope class 只用于生成 Style 的 owner-relative 路由。
- 穿过 `ItemTemplate`、`ContentLeftAddOnTemplate`、`ContentRightAddOnTemplate` 等用户模板继续匹配内部 Visual。

## 5. 定制边界

以下区域**不属于** ComboBox Semantic Part：

- **清除按钮**：`IsAllowClear` 虽由 `TextBox.IsAllowClearProperty` 转挂为 ComboBox public API，但 ComboBox
  全控件**未实现**该能力——模板中没有清除按钮节点，代码中没有清除逻辑（`Select` / `LineEdit` 的清除按钮在各自
  handle 模板或后缀区内，ComboBox 没有对应节点）。发布该 Part 会产生永不命中的幽灵契约，因此**不发布**。
  若后续实现清除能力，再作为新增 Part 提交（新增是非破坏性变更）。
- **非编辑态选中内容节点**（`SelectedContentPresenter`）：不单独发布。它由 `content` 覆盖，需要精确着色时在
  `ComboBoxContentStyle` 内用嵌套 `<Style Selector="^ ContentPresenter">` 定位。当前 `placeholder` 与
  `SelectedContentPresenter` 的可见性互斥由模板的 `IsEditable` 分支管理，未发布独立 Part 以保持 `Select`
  家族的区域集合一致。
- **Form 校验反馈节点**（`PART_FormFeedBack`）：`Select`、`LineEdit` 均未发布，ComboBox 同样不发布；其视觉由
  `FormFeedback` 与 `AddOnDecoratedBox` 的 `EffectiveStatus` 投影承担。
- **共享主题 marker 与 owner 作用域**：`frame` 的 marker 物理位于共享 `AddOnDecoratedBoxTheme`，但生成 Style 的
  路由以 owner（`ComboBox`）为起点（`Nesting()` 起手），因此只有挂在 `ComboBox` 上的语义 Style 才会命中。同页的
  `Select` / `LineEdit` 等控件虽然在该节点上也带 `.semantic-frame`，但它们的语义 Style 选择器以各自 owner 起手，
  不会被 ComboBox 的样式命中，反之亦然。**不得**为了跨控件统一而改造共享主题的节点结构或移除该 marker。
- **`frame` 不发布状态子部件**：hover / focus / error / warning / disabled 的状态色由 `AddOnDecoratedBox` 的
  `:outline` / `:filled` / `:underlined` 与 `EffectiveStatus` 在共享主题中驱动，`frame` 只承诺静态边框盒本身。
  应用定制静态边框色时注意：聚焦或校验态的状态色优先级高于静态语义 Setter 的场景由 Avalonia 原生样式优先级决定
  （系统设计 §7），需要连状态色一起覆盖时应在语义 Style 中同时覆盖对应状态选择器。
- **根表面定制走 owner 属性，不新增 Part**：改「输入框边框颜色」这类最常见的需求，标准入口是直接设置
  `ComboBox.BorderBrush` / `Background`——控件已把这两个根表面画刷以 `LocalValue` 中继到共享输入帧，压过帧的
  rest / hover / focus / status 状态机。这是输入族的统一约定（`Select`、`ButtonSpinner`、`OtpLineEdit` 等同样如此），
  与 `frame` 部件的关系如下：
  - **根中继**（`ComboBox.BorderBrush` / `Background`）作用于 owner 根，写入 `LocalValue`，用于「整个输入框换一个颜色」；
  - **`frame` 专用 Style**（`ComboBoxFrameStyle`）精确命中帧节点，写入 `Style` 优先级，用于状态无关的边框宽度、圆角、
    背景等需要区分于 owner 根的场景。
  两者都落在同一个 `PART_ContentFrame` 节点上，但 `LocalValue` 高 `Style` 一级，因此**同时使用时根中继胜出**。
  需要连状态色一并覆盖时用 `frame` 语义 Style 里的状态选择器，而不是根中继。中继的接管标记、清空回退与
  `IsMotionEnabled` 接线要求见 [ComboBox 实现](implementation.md) §5.2。

- **内置 AddOn 区**（`LeftAddOn` / `RightAddOn` 的 `PixelAlignedBorder`）：由共享 `AddOnDecoratedBox` 契约与
  其 `ContentLeftAddOn` / `ContentRightAddOn` 投影承担，ComboBox 不重复声明嵌套 owner 的区域。
- **`ComboBoxItem` 作为独立 owner**：不注册 descriptor。`ComboBoxItem` 模板内只有一个纯 `ContentPresenter`，
  容器级样式（背景、前景、内边距、圆角、hover/selected 视觉）已由 `popup.listItem` 完整覆盖；注册它会增加一份
  无独立职责的契约（对照：`TabStripItem` 有自己的 close/icon/label 才独立注册，`ListBoxItem` 不注册）。
- **空态内部视觉**（`atom:Empty` 的插图与描述）：`Empty` 有自身语义 owner，`popup.empty` 只承诺承载它的
  `Border` 节点的外框与内边距。
- **弹层 Popup 宿主与定位**（`PART_Popup`、阴影、`MarginToAnchor`、钉住打开、light-dismiss）：由共享 Popup
  契约承担，`popup.root` 只覆盖弹层内容框体的视觉。
- `PART_*` 名称、internal 类型、`.semantic-scope-*` 路由标记与模板层级。

默认主题不消费 `.semantic-*` selector；静态 marker 只提供应用样式命中点，不改变默认属性优先级或增加状态订阅。
Semantic Style 服从 Avalonia 原生属性优先级。

## 6. 兼容性与验证

删除或重命名 Part、修改 selector class / route、收窄 `ContractType`（含把公共基类承诺收窄为具体实现类型）、
改变 cardinality，或让任一内置模板变体缺少 marker，均属于公共主题契约变更。

验证至少覆盖：

- owner descriptor 只包含 §1 的 12 个 Part（`root` + 11 个非 root），字段值与本文一致。
- `ComboBoxTheme.axaml` 携带 `prefix` / `content` / `placeholder` / `input` / `suffix` / `popup.root` /
  `popup.list` / `popup.empty` 的静态 marker，以及 `indicator` 路由所需的 `.semantic-scope-input`、
  `.semantic-scope-suffix`、`.semantic-scope-handle` scope 锚点；`ComboBoxHandleTheme.axaml` 携带
  `indicator` marker；共享 `AddOnDecoratedBoxTheme.axaml` 携带 `frame` marker。
- `popup.listItem` 的 marker 在运行时容器创建路径注入，`CreateContainerForItemOverride` 与
  `PrepareContainerForItemOverride` 一致，prepare/clear/recycle 后 marker 不丢失、不重复。
- 生成的 `ComboBox*Style` 可编译并恰好命中一个目标节点；`root` 不生成 Style。
- 弹层打开、关闭、重新打开后 `popup.*` 四部件 marker 保持；模板重应用后所有 marker 保持。
- `IsPopupPinnedOpen` 可钉住弹层且自动抑制 light-dismiss（`IsLightDismissEnabled=false`）；**属性被抑制不等于遮罩未创建**：
  钉住打开必须不留下可见的 `LightDismissOverlayLayer`，且弹层自行关闭后 `IsDropDownOpen` 同步回写（见
  `ComboBoxPinnedPopupOverlayTests`）。
- `IsEditable` 切换后 `input` 与 `placeholder` 节点的命中稳定。
- 状态变化（`IsEditable`、`IsFilterEnabled`、过滤结果、选择变化、disabled、`Status`）不改变 marker 身份与数量语义。
- 默认主题不消费 `.semantic-*`，未声明用户 Semantic Style 时不增加 selector activator。
- 根表面 `BorderBrush` / `Background` 定制压过帧的 hover / rest 状态机，清空后交还状态机，模板应用前设置的值同样生效，
  未定制时保持主题 rest 状态；帧已接收 owner 的 `IsMotionEnabled`（见 `ComboBoxRootSurfaceRelayTests`）。
- 不存在以 Name / Loaded / Unloaded 事件处理器为特征的 code-behind 属性回退。
- Gallery Semantic Parts 页签延迟创建 Preview，12 个 Part 均有预览卡片且 marker 均在场（见 §6.1）。

### 6.1 Gallery 预览与弹层钉住

Gallery 语义预览按 AtomUI 标准方式钉住模板内 Popup（`IsPopupPinnedOpen="True"` + `IsDropDownOpen="True"`，并开启
`IsEditable` / `IsFilterEnabled` 以便 `input` 部件可见）。`IsPopupPinnedOpen` 与 `Select` / `AutoComplete` / `Mentions`
等控件一致为 **public**。

预览测试断言三个层面（`tests/AtomUIGallery.Tests/ShowCases/ComboBoxSemanticPartHighlightTests.cs`）：

1. 12 个部件各有预览卡片，且每个部件的 marker 在树中至少存在一个目标节点；`popup.*` 必须位于弹层宿主层而不是触发侧树内。
2. 钉住状态下 `Popup.IsOpen=true` 且 `IsLightDismissEnabled=false`，并且**不得存在可见的 light-dismiss 遮罩层**。
3. 预览卡片悬停必须产生真实高亮（触发侧与弹层侧部件都覆盖），离开页签后高亮会话释放。

#### 弹层开合由控件接管（遮罩抑制时机契约）

**产品控件负责在弹层打开前抑制 light-dismiss 遮罩。** Avalonia 只在弹层打开的瞬间读取一次
`IsLightDismissEnabled` 来决定是否创建遮罩层，该属性**没有变更回调**，因此打开之后再改属性撤不掉已创建的遮罩；
而遮罩层的 `HitTest` 会吞掉除 `OverlayInputPassThroughElement` 以外的所有命中点。若在遮罩创建之后才抑制，钉住场景
会留下一个只拦截输入、无法消除的遮罩层挡住整页交互，预览卡片随之无法悬停。

因此 ComboBox 的模板**不声明** `IsOpen` 模板绑定（`ComboBoxTheme.axaml` 的 `PART_Popup`）：

- 模板充气阶段的绑定求值会早于控件的遮罩抑制打开弹层，正是残留遮罩的成因；
- 弹层开合改由 `ComboBox` 以代码跟随 `IsDropDownOpen` 接管（`OpenPopup` / `ClosePopup`），`OnApplyTemplate` 先
  relay 钉住状态并抑制遮罩，**之后**才在 `IsDropDownOpen` 已为 true 时补开弹层；
- 模板上原有的 TwoWay `IsOpen` 绑定同时承担了「弹层自行关闭回写业务状态」，移除后由 `Popup.Closed` 处理器显式补齐；
  钉住期间该回传改为重新收敛回打开，以保持「外点/Escape 不改变有效打开状态」的钉住契约。

这与共享 `AbstractSelect` 的既有约定一致（`Select` / `Cascader` / `TreeSelect` 的模板同样不声明 `IsOpen` 绑定）。
回归护栏见 `tests/AtomUI.Desktop.Controls.Tests/ComboBox/ComboBoxPinnedPopupOverlayTests.cs`。

Gallery 语义预览的授权写法与「产品控件负责抑制」的完整说明见
[语义部件预览](../../../../gallery/authoring/semantic-part-preview.md)；Popup 首次打开竞态与钉住遮罩的排查记录见
[Semantic Part Popup 首次打开生命周期竞态案例](../../../../engineering/case-studies/semantic-part-popup-first-open-lifecycle-case-study.md)。

#### 预览数据模型的文本表示要求

预览为让 `input` 部件可见而开启 `IsEditable`，此时输入框显示的是 `ComboBox.Text`。Avalonia 通过
`TextSearch.GetEffectiveText` 从选中项推导该文本，取值链为 `TextSearch.Text`（项是控件时）→ `TextSearch.TextBinding`
→ `ItemsControl.DisplayMemberBinding` → `IContentControl.Content.ToString()`（项是 `IContentControl` 时）→
`object.ToString()`。

因此当预览用**自定义数据模型**（而非 `ComboBoxItem` 字符串内容）作为 `ItemsSource` 时，该模型必须提供可读的文本表示，
否则末级回落会渲染类型全名（曾出现 `AtomUIGallery.ShowCases.ComboBox.ComboBoxItemData` 显示在输入框内）。两种做法都可接受：

- 让模型重写 `ToString()` 返回显示文本（本页预览采用）；或
- 设置 `DisplayMemberBinding` / `TextSearch.TextBinding` 指向文本属性。

注意弹层行文本与输入框文本是**两条独立呈现路径**：行文本由 `ItemTemplate` 决定，输入框文本由上述推导链决定。
只配 `ItemTemplate` 时弹层行显示正常，输入框仍会回落——这正是本次缺陷只出现在输入框的原因。回归护栏见
`ComboBoxSemanticPartHighlightTests.ComboBox_Semantic_Preview_Editable_Input_Shows_The_Item_Text`。

#### `IsPopupPinnedOpen` 的可见性

`IsPopupPinnedOpen` 及其 Avalonia 属性字段原为 `internal`，本次改造提升为 `public`，与
`Select` / `AbstractSelect`、`AutoComplete`、`Mentions`、`ColorPicker`、`InfoPickerInput`、`Menu`、`NavMenu`、
`DropdownButton`、`SplitButton`、`Tour`、`FlyoutHost` 的既有声明一致（标准 XML 注释也逐字对齐）。

`tests/AtomUI.Desktop.Controls.Tests/Popup/PopupPinnedOpenContractTests.cs` 中名为
`Direct_Popup_Pinned_Open_Contracts_Are_Internal` 的 Theory **在列表中包含 ComboBox 并断言该契约必须为 internal**，
但该 Theory 与 `Leaf_Controls_Inherit_The_Contract_From_Their_Semantic_Owner` **实际执行 0 个用例**（`MemberData`
未产出数据），因此从未真正强制。同一列表内的其他成员（`AbstractSelect`、`AbstractAutoComplete`、
`InfoPickerInput`、`Mentions`、`Menu`、`NavMenu`、`DropdownButton`、`SplitButton`、`Tour`、`FlyoutHost`）
**均已声明为 public**，与该 Theory 的断言直接矛盾——即「必须 internal」并非现行标准，该 Theory 是失效的陈旧断言。
本次以「同族控件的一致性」为准提升为 public，未改测试。

实现与测试入口：`tests/AtomUI.Desktop.Controls.Tests/ComboBox/ComboBoxSemanticPartTests.cs`；Gallery 示例见
`controlgallery/AtomUIGallery/ShowCases/Navigation/ComboBox`。视觉验收步骤与结论见
`docs/superpowers/specs/` 下的 ComboBox 验收记录（真机截图由用户回传）。
