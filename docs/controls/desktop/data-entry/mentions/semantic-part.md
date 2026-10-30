# Mentions Semantic Part 契约

本文档定义 Mentions 控件公开的 Semantic Part、Selector、类型约束、数量语义和定制边界。控件整体设计见
[Mentions 桌面版架构设计](overview.md)，真实模板、marker 映射与状态流见
[Mentions 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Semantic Parts

Mentions 是唯一 Semantic owner，公开 9 个 Semantic Part。声明位于 `Mentions.SemanticParts.cs` partial 文件。

Mentions 的输入表面整体委托给内部 `MentionTextArea : TextArea`，因此 `prefix` / `content` / `placeholder` /
`input` / `clear` 五个输入区部件复用 AutoCompleteTextArea 的跨嵌套 owner 路由：`MentionTextArea` 节点在宿主模板
上标注 `.semantic-scope-input` 锚点，随后跨入共享 `TextAreaTheme` 与 `TextAreaDecoratedBoxTheme` 的模板边界命中
真实 marker。`prefix` 的 marker 本体是宿主模板 `TextAreaTheme.axaml` 里投影给 `TextAreaDecoratedBox.ContentLeftAddOn`
的 `AddOnContentPresenter`，其经共享 `TextAreaDecoratedBoxTheme` 的 `.semantic-scope-prefix` 锚点路由可达。弹层三部件
与 AutoComplete 同构：`popup.root` 是宿主模板 `PopupFrame` 静态 Border，`popup.list` 是模板内 `CandidateList`
静态节点，`popup.listItem` 的 marker 由 `CandidateList` 容器创建路径注入。

#### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `Mentions` |
| Part | `root` |
| Selector | Mentions 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `Mentions` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | Mentions owner |
| 职责 | Mentions root 是文本值、触发符、候选数据、过滤、弹层与 Form 状态的组织边界。 |
| 相关 API | 全部 Mentions public API |
| 相关 Token | MentionsToken、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `prefix`

| 字段 | 值 |
| --- | --- |
| Owner | `Mentions` |
| Part | `prefix` |
| Selector | `.semantic-prefix` |
| SelectorRoute | `/template/ .semantic-scope-input >> .semantic-scope-input-frame /template/ .semantic-scope-prefix > .semantic-prefix` |
| Style Type | `MentionsPrefixStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `TextAreaTheme.axaml` 中投影给 `TextAreaDecoratedBox.ContentLeftAddOn` 的 `AddOnContentPresenter` |
| 职责 | 输入内容前缀区域，承载 `ContentLeftAddOn` 用户内容，在内容框内联展示。 |
| 相关 API | `ContentLeftAddOn`、`ContentLeftAddOnTemplate` |
| 相关 Token | SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `content`

| 字段 | 值 |
| --- | --- |
| Owner | `Mentions` |
| Part | `content` |
| Selector | `.semantic-content` |
| SelectorRoute | `/template/ .semantic-scope-input /template/ .semantic-content` |
| Style Type | `MentionsContentStyle` |
| ContractType | `Panel` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `TextAreaTheme.axaml` 中 `ContentLayout` 内容面板 |
| 职责 | 输入内容面板，承载占位符和多行文本 presenter。 |
| 相关 API | `Value`、`PlaceholderText`、`Lines`、`MinLines`、`MaxLines`、`IsAutoSize` |
| 相关 Token | SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `placeholder`

| 字段 | 值 |
| --- | --- |
| Owner | `Mentions` |
| Part | `placeholder` |
| Selector | `.semantic-placeholder` |
| SelectorRoute | `/template/ .semantic-scope-input /template/ .semantic-content > .semantic-placeholder` |
| Style Type | `MentionsPlaceholderStyle` |
| ContractType | `TextBlock` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `TextAreaTheme.axaml` 中 `Placeholder` TextBlock |
| 职责 | 空文本状态下的占位符文本。 |
| 相关 API | `PlaceholderText`、`PlaceholderForeground`（经 TextArea） |
| 相关 Token | SharedToken（ColorTextPlaceholder） |
| 稳定性 | stable since 6.2.0 |

#### `input`

| 字段 | 值 |
| --- | --- |
| Owner | `Mentions` |
| Part | `input` |
| Selector | `.semantic-textarea` |
| SelectorRoute | `/template/ .semantic-scope-input /template/ .semantic-textarea` |
| Style Type | `MentionsInputStyle` |
| ContractType | `TextPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `TextAreaTheme.axaml` 中 `InputTextPresenter`（`PART_TextPresenter`，internal，公共契约承诺 Avalonia `TextPresenter`） |
| 职责 | 多行文本编辑 presenter，承载 `Value` 文本与 caret/selection 状态。 |
| 相关 API | `Value`、`IsReadOnly`、`IsAutoFocus` |
| 相关 Token | SharedToken（ColorText、SelectionBackground、CaretBrush） |
| 稳定性 | stable since 6.2.0 |

#### `clear`

| 字段 | 值 |
| --- | --- |
| Owner | `Mentions` |
| Part | `clear` |
| Selector | `.semantic-clear` |
| SelectorRoute | `/template/ .semantic-scope-input >> .semantic-scope-input-frame /template/ .semantic-scope-suffix > .semantic-suffix > .semantic-clear` |
| Style Type | `MentionsClearStyle` |
| ContractType | `Button` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `TextAreaTheme.axaml` 中 `InputClearIconButton`（`PART_ClearButton`，internal，公共契约承诺 Avalonia `Button`） |
| 职责 | 输入区后缀内的清除按钮，`IsAllowClear` 启用且内容非空时可见。 |
| 相关 API | `IsAllowClear`、`ClearIcon` |
| 相关 Token | SharedToken（IconSizeXS） |
| 稳定性 | stable since 6.2.0 |

#### `popup.root`

| 字段 | 值 |
| --- | --- |
| Owner | `Mentions` |
| Part | `popup.root` |
| Selector | `.semantic-popup-root` |
| SelectorRoute | `/template/ .semantic-popup-root` |
| Style Type | `MentionsPopupRootStyle` |
| ContractType | `Border` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `MentionsTheme.axaml` 中 `PopupFrame`（`PART_Popup` 直接子节点） |
| 职责 | 候选弹层根 `Border`，可定制弹层边框、背景、宽度与圆角。 |
| 相关 API | `MaxPopupHeight`、`MinPopupWidth`、`PopupContentPadding` |
| 相关 Token | PopupTokenResource（PopupCornerRadius）、SharedToken（ColorBgElevated） |
| 稳定性 | stable since 6.2.0 |

#### `popup.list`

| 字段 | 值 |
| --- | --- |
| Owner | `Mentions` |
| Part | `popup.list` |
| Selector | `.semantic-popup-list` |
| SelectorRoute | `/template/ .semantic-popup-root >> .semantic-popup-list` |
| Style Type | `MentionsPopupListStyle` |
| ContractType | `CandidateList` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `MentionsTheme.axaml` 中 `PART_CandidateList`（public `CandidateList`） |
| 职责 | 弹层内候选列表容器，承载过滤后的候选项。 |
| 相关 API | `OptionsSource`、`Filter`、`FilterValueSelector`、`OptionTemplate` |
| 相关 Token | MentionsToken（OptionHeight）、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `popup.listItem`

| 字段 | 值 |
| --- | --- |
| Owner | `Mentions` |
| Part | `popup.listItem` |
| Selector | `.semantic-popup-list-item` |
| SelectorRoute | `/template/ .semantic-popup-list >> .semantic-popup-list-item` |
| Style Type | `MentionsPopupListItemStyle` |
| ContractType | `CandidateListItem` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | public `CandidateListItem`（`CandidateList.CreateContainerForItemOverride()` 注入 marker） |
| 职责 | 候选列表中的单个选项条目，运行时容器创建，虚拟化回收复用时 marker 保持。 |
| 相关 API | `OptionTemplate`、`DisplayCandidateCount` |
| 相关 Token | SharedToken |
| 稳定性 | stable since 6.2.0 |

`ContractType` 不参与 selector 匹配，只约束 `x:SetterTargetType` 与兼容性下界；实现节点为 internal 类型时，
公共契约承诺到最低 public 基类（`input`→`TextPresenter`、`clear`→`Button`）。

## 2. 职责与存在条件

- `prefix` / `content` / `placeholder` / `input` / `clear` 声明 `CrossNestedOwners=true`，物理 marker 位于共享
  `TextAreaTheme` 与 `TextAreaDecoratedBoxTheme` 内；宿主模板只在 `MentionTextArea` 节点标注
  `.semantic-scope-input` 锚点。生成器沿锚点类型链（`MentionTextArea` → `TextArea` 主题）校验 marker 数量与类型。
- `popup.root` 是 `PART_Popup` 的静态直接子节点，承载弹层内容根视觉；`popup.list` 是模板内静态 `CandidateList`，
  两者均为每弹层唯一容器，`Single`。
- `popup.listItem` 为 `RuntimeCreated=true` + `Multiple`，marker 由共享 `CandidateList` 容器创建路径注入，生成器
  豁免宿主模板 marker 校验，由控件行为测试兜底。虚拟化回收复用容器时 marker 保持不变。

## 3. 数量语义

`root`、`prefix`、`content`、`placeholder`、`input`、`clear`、`popup.root`、`popup.list` 均为静态模板节点或每弹层
唯一容器，`Single`；状态变化（`IsAllowClear`、`IsReadOnly`、`IsDropDownOpen`、`IsLoading`、disabled）只切换
可见性或有效视觉值，不增删 marker。`popup.listItem` 为 `Multiple`，条目数量随选项与过滤结果变化。

## 4. Selector 用法

生成的 Semantic Style 类型命名为 `Mentions<PartPathPascalCase>Style`，如 `MentionsPopupRootStyle`、
`MentionsPopupListItemStyle`、`MentionsPrefixStyle`（命名空间 `AtomUI.Theme.Styling`，AXAML 命名空间
`https://atomui.net`）。`root` 不生成 Style 类型，owner 级 Setter 写在外层普通 Style 上。

推荐写法（owner 嵌套 Style + 语义 class）：

```xml
<StackPanel.Styles>
    <Style Selector="atom|Mentions.semantic-styles-demo">
        <atom:MentionsPrefixStyle x:SetterTargetType="ContentPresenter">
            <Style Selector="^ atom|Icon">
                <Setter Property="StrokeBrush" Value="#1890FF" />
            </Style>
        </atom:MentionsPrefixStyle>
        <atom:MentionsPopupRootStyle x:SetterTargetType="Border">
            <Setter Property="BorderBrush" Value="#722ED1" />
            <Setter Property="BorderThickness" Value="1" />
        </atom:MentionsPopupRootStyle>
    </Style>
</StackPanel.Styles>
<atom:Mentions Classes="semantic-styles-demo"
               ContentLeftAddOn="{antdicons:AntDesignIconProvider Kind=MehOutlined}" ... />
```

`prefix` 为 `Icon` 时，AtomUI 图标由 `StrokeBrush` / `FillBrush` 驱动而非 `Foreground`，因此在
`MentionsPrefixStyle` 内嵌套 `<Style Selector="^ atom|Icon">` 设置 `StrokeBrush` 着色。`input` 为文本
presenter，直接使用 `TextElement.Foreground` 或 `Foreground` 定制文本颜色。

`prefix` / `content` / `placeholder` / `input` / `clear` 的生成 Style 由 `Nesting()` 经 scope 锚点跨越模板边界
命中目标。不得使用以下写法：

- `.semantic-root`、`PART_*`、Name selector、internal 类型或视觉祖先顺序作为应用主题契约。
- 把 `Border.semantic-popup-root` 等 `ContractType` 写入 Part 身份 selector。
- 直接复制 `/template/ .semantic-scope-*` route；scope class 只用于生成 Style 的 owner-relative 路由。
- 穿过 `OptionTemplate` 等用户模板继续匹配内部 Visual。

## 5. 定制边界

以下区域不属于 Mentions Semantic Part：

- **输入表面的内部实现节点**：`content` / `input` / `placeholder` 只承诺共享 TextArea 模板的内容面板、
  presenter 与占位节点；边框 frame、滚动条、resize handle 等由共享 TextArea 机制拥有，不经 Mentions 发布。
- **候选列表的选中 / hover / disabled 状态**：由共享 `CandidateList` 与 `CandidateListItem` 主题承担，不发布
  为 Mentions Part。
- **loading 指示**：弹层 loading 由 `IsLoading` 行为 API 承担，`LoadingIndicator` 不发布 Part。
- **弹层 Popup 宿主与定位**：候选 Popup 的定位、钉住打开、动画由共享 Popup 契约承担，`popup.root` 只覆盖
  弹层内容根 Border 的视觉。
- `PART_*` 名称、internal 类型、`.semantic-scope-*` 路由标记与模板层级。

默认主题不消费 `.semantic-*` selector；静态 marker 只提供应用样式命中点，不改变默认属性优先级或增加状态
订阅。Semantic Style 服从 Avalonia 原生属性优先级。

## 6. 兼容性与验证

删除或重命名 Part、修改 selector class / route、收窄 `ContractType`（含把公共基类承诺收窄为具体实现
类型）、改变 cardinality，或让任一内置模板变体缺少 marker，均属于公共主题契约变更。

验证至少覆盖：

- owner descriptor 只包含 §1 的 9 个 Part，字段值与本文一致。
- `MentionsTheme.axaml` 携带 `semantic-scope-input`、`semantic-popup-root`、`semantic-popup-list` 静态 marker；
  共享 `TextAreaTheme.axaml` 与 `TextAreaDecoratedBoxTheme.axaml` 携带 `prefix` / `content` / `placeholder` /
  `input` / `clear` 相关 marker 与 scope 锚点。
- `popup.listItem` 的 marker 在 `CandidateList.CreateContainerForItemOverride()` 容器创建路径注入。
- 生成的 `Mentions*Style` 可编译并命中目标节点（见
  `tests/AtomUI.Desktop.Controls.Tests/Mentions/MentionsSemanticPartTests.cs`）。
- 钉住弹层在打开前抑制 light-dismiss 遮罩；重开与模板重应用后弹层部件 marker 保持。
- 状态变化（`IsAllowClear`、`IsReadOnly`、`IsDropDownOpen`、`IsLoading`、disabled）不改变 marker 身份与数量语义。
- 默认主题不消费 `.semantic-*`，未声明用户 Semantic Style 时不增加 selector activator。
- descriptor、生成 Style 与 NativeAOT 路径使用编译期生成数据，不依赖运行时反射或 VisualTree 扫描。
