# AutoComplete Semantic Part 契约

本文档定义 AutoComplete 控件家族公开的 Semantic Part、Selector、类型约束、数量语义和定制边界。控件整体设计见
[AutoComplete 桌面版架构设计](overview.md)，真实模板、marker 映射与状态流见
[AutoComplete 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Semantic Parts 总览

AutoComplete 家族有三个 public owner：`AutoComplete`、`AutoCompleteSearchEdit` 与 `AutoCompleteTextArea`。三者
声明完全相同的 9 个语义键：`root`、`prefix`、`content`、`placeholder`、`input`、`clear`、`popup.root`、
`popup.list`、`popup.listItem`（与上游 AutoComplete 的语义 DOM 对齐，`popup.*` 对应上游 `popup`
分组的 `root` / `list` / `listItem`）。声明位于各 owner 同目录的 `*.SemanticParts.cs` partial 文件。

前 6 个宿主部件位于输入框模板内，且全部声明 `CrossNestedOwners=true`：`AutoCompleteSearchEdit` /
`AutoCompleteTextArea` 内嵌 `LineEdit` / `TextArea` 输入控件，宿主部件的 marker 由嵌套输入控件自身模板携带，
路由通过 `.semantic-scope-input` / `.semantic-scope-input-frame` scope 锚点从 owner 穿透到嵌套控件模板
（`/template/` 链不能直接跨越嵌套 owner，因此这些 Part 由生成 Style 借助 scope 路由命中）。弹层三部件
（`popup.root` / `popup.list` / `popup.listItem`）位于 owner 自有的 Popup 模板内，不涉及嵌套 owner。

各 Part 的 Selector、SelectorRoute、ContractType 以 `AutoComplete.SemanticParts.cs` 为准；下表为 owner
`AutoComplete` 的声明（SearchEdit / TextArea 声明同键，route 差异见 §1.2）：

### 1.1 部件表（`AutoComplete`）

| Part | SelectorClass | SelectorRoute | ContractType | Cardinality | Since |
| --- | --- | --- | --- | --- | --- |
| `root` | 不适用（owner 本身，无 marker） | 不适用 | `AutoComplete` | Single | 6.0 |
| `prefix` | `.semantic-prefix` | `/template/ .semantic-scope-input >> .semantic-scope-input-frame /template/ .semantic-scope-prefix > .semantic-prefix` | `ContentPresenter` | Single | 6.0 |
| `content` | `.semantic-content` | `/template/ .semantic-scope-input /template/ .semantic-content` | `Panel` | Single | 6.0 |
| `placeholder` | `.semantic-placeholder` | `/template/ .semantic-scope-input /template/ .semantic-content > .semantic-placeholder` | `TextBlock` | Single | 6.0 |
| `input` | `.semantic-input` | `/template/ .semantic-scope-input /template/ .semantic-input` | `TextPresenter` | Single | 6.0 |
| `clear` | `.semantic-clear` | `/template/ .semantic-scope-input >> .semantic-scope-input-frame /template/ .semantic-scope-suffix > .semantic-suffix > .semantic-clear` | `Button` | Single | 6.0 |
| `popup.root` | `.semantic-popup-root` | `/template/ .semantic-popup-root` | `Border` | Single | 6.0 |
| `popup.list` | `.semantic-popup-list` | `/template/ .semantic-popup-root > .semantic-popup-list` | `CandidateList` | Single | 6.0 |
| `popup.listItem` | `.semantic-popup-list-item` | `/template/ .semantic-popup-list >> .semantic-popup-list-item` | `CandidateListItem` | Multiple | 6.0 |

`prefix`、`content`、`placeholder`、`input`、`clear` 均声明 `CrossNestedOwners=true`；`popup.listItem` 声明
`RuntimeCreated=true`（候选条目运行时创建）。

### 1.2 嵌套输入控件 owner 的 route 差异

`AutoCompleteSearchEdit` 与 `AutoCompleteTextArea` 的声明与 §1.1 同键同 ContractType，仅两处 route 不同：

- `AutoCompleteSearchEdit.prefix`：输入框 frame 由 SearchEdit 主题承担，route 为
  `/template/ .semantic-scope-input >> .semantic-scope-input-frame /template/ .semantic-scope-prefix > .semantic-prefix`。
- `AutoCompleteTextArea.input`：文本域的可编辑区域 marker 为 `.semantic-textarea`，SelectorRoute 为
  `/template/ .semantic-scope-input /template/ .semantic-textarea`（SelectorClass 仍为 `semantic-input`，
  Part 身份与生成 Style 类型保持 `input` 命名）。

其余键（含 `clear`、`popup.*`）三个 owner 完全一致。

### 1.3 职责说明

- `root`：AutoComplete owner 本身，承载数据源、过滤、弹层与状态的组织边界，可定制 owner 级视觉属性。
- `prefix`：输入框框架的前缀区域（`ContentLeftAddOn` 宿主）。
- `content`：输入内容面板，承载文本呈现器与占位符。
- `placeholder`：输入为空时显示的占位符文本。
- `input`：可编辑输入区域的 `TextPresenter`。
- `clear`：后缀区域的清除按钮，`IsAllowClear` 启用且有输入时可见（`ClearIcon` 可定制）。
- `popup.root`：候选弹层根 `Border`，可定制弹层边框、背景与宽度。
- `popup.list`：弹层内候选列表容器（`CandidateList`）。
- `popup.listItem`：单个候选条目（`CandidateListItem`），每个选项运行时创建一个实例。

## 2. 数量语义

`root`、`prefix`、`content`、`placeholder`、`input`、`clear`、`popup.root`、`popup.list` 均为静态模板节点，
`Single`；状态变化（聚焦、有值、`IsAllowClear`、Status）只切换可见性或有效视觉值，不增删 marker。`clear` 未启用
`IsAllowClear` 时隐藏但 marker 仍存在。`popup.listItem` 为 `Multiple`：候选弹层打开时按过滤后选项数实例化，
关闭或空数据源为 0，条目回收后随复用重建，marker 保持不变。

## 3. 定制方式

生成的 Semantic Style 类型命名为 `<Owner><PartPathPascalCase>Style`，如
`AutoCompletePopupRootStyle`、`AutoCompletePopupListItemStyle`、`AutoCompleteTextAreaPrefixStyle`
（命名空间 `AtomUI.Theme.Styling`，AXAML 命名空间 `https://atomui.net`）。`root` 不生成 Style 类型，
owner 级 Setter 写在外层普通 Style 上。

推荐写法（owner 嵌套 Style + 语义 class，与 Gallery
`ShowCases/DataEntry/AutoComplete` 的"自定义语义结构的样式"示例一致）：

```xml
<StackPanel.Styles>
    <Style Selector="atom|AutoComplete.semantic-styles-demo">
        <atom:AutoCompletePopupRootStyle x:SetterTargetType="Border">
            <Setter Property="BorderBrush" Value="#1890FF" />
            <Setter Property="BorderThickness" Value="1" />
        </atom:AutoCompletePopupRootStyle>
        <atom:AutoCompletePopupListStyle x:SetterTargetType="TemplatedControl">
            <Setter Property="Background" Value="#D9F0F0F0" />
        </atom:AutoCompletePopupListStyle>
        <atom:AutoCompletePopupListItemStyle x:SetterTargetType="TemplatedControl">
            <Setter Property="TextElement.Foreground" Value="#272727" />
        </atom:AutoCompletePopupListItemStyle>
    </Style>
</StackPanel.Styles>
<atom:AutoComplete Classes="semantic-styles-demo" ... />
```

宿主部件的定制走对应的 `AutoComplete*Style` / `AutoCompleteSearchEdit*Style` / `AutoCompleteTextArea*Style`
前缀 Part Style（如 `AutoCompleteSearchEditPlaceholderStyle`、`AutoCompleteTextAreaClearStyle`），由生成 Style
的 `Nesting()` 跨越嵌套 owner 模板边界命中目标。不得使用以下写法：

- `.semantic-root`、`PART_*`、Name selector、internal 类型或视觉祖先顺序作为应用主题契约。
- 把 `Border.semantic-popup-root` 等 `ContractType` 写入 Part 身份 selector。
- 直接复制 `/template/ .semantic-scope-*` route；scope class 只用于生成 Style 的 owner-relative 路由。
- 穿过 `OptionTemplate`、`ContentLeftAddOnTemplate` 等用户模板继续匹配内部 Visual。

## 4. 定制边界

以下区域不属于 AutoComplete Semantic Part：

- **弹层 Popup 宿主与定位**：候选 Popup 的定位、钉住打开、动画由共享 Popup 契约承担，`popup.root` 只覆盖
  弹层内容根 Border 的视觉。
- **嵌套输入控件的家族契约**：`AutoCompleteSearchEdit` / `AutoCompleteTextArea` 内嵌 `LineEdit` / `TextArea`
  的其余内部区域（边框 frame、后缀指示等）由 Input 家族契约覆盖；宿主 6 部件通过 scope 路由发布，不重复
  嵌套控件未公开的区域。
- **候选条目内部**：`popup.listItem` 内由 `OptionTemplate` 生成的用户内容不发布 marker。
- `PART_*` 名称、internal 类型、`.semantic-scope-*` 路由标记与模板层级。

默认主题不消费 `.semantic-*` selector；静态 marker 只提供应用样式命中点，不改变默认属性优先级或增加状态订阅。
Semantic Style 服从 Avalonia 原生属性优先级。

## 5. 兼容性与验证

删除或重命名 Part、修改 selector class / route、收窄 `ContractType`、改变 cardinality，或让任一内置模板变体
缺少 marker，均属于公共主题契约变更。

验证至少覆盖：

- 三个 owner descriptor 均只包含 §1.1 的 9 个键，字段值与本文一致。
- owner 模板与嵌套输入控件模板携带对应 `semantic-*` marker；`popup.listItem` marker 由 `CandidateListItem`
  模板在运行时实例化路径携带。
- 生成的 `AutoComplete*Style` / `AutoCompleteSearchEdit*Style` / `AutoCompleteTextArea*Style` 可编译并跨越
  嵌套 owner 模板边界命中最低 public `ContractType`（见
  `tests/AtomUI.Desktop.Controls.Tests/AutoComplete/AutoCompleteSemanticPartTests.cs`）。
- `IsAllowClear`、Status、候选开关等状态变化不改变 marker 身份与数量语义。
- 默认主题不消费 `.semantic-*`，未声明用户 Semantic Style 时不增加 selector activator。
- Gallery Semantic Parts Tab 延迟创建 Preview，9 个 Part 均可解析高亮。
- descriptor、生成 Style 与 NativeAOT 路径使用编译期生成数据，不依赖运行时反射或 VisualTree 扫描。
