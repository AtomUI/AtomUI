# NumericUpDown Semantic Part 契约

本文档定义 `NumericUpDown` 公开的 Semantic Part、Selector、类型约束、数量语义和定制边界。控件整体设计见
[NumericUpDown 桌面版架构设计](overview.md)，真实模板、状态投影与尺寸基线见
[NumericUpDown 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Semantic Parts

`NumericUpDown` 公开 `root`、`prefix`、`input`、`suffix`、`clear` 五个职责区域（§1.1–1.5）。每个 Part 均为
`Single`，且在 `Mode=Input` 与 `Mode=Spinner` 两个内置模板变体中提供相同的 marker 与 route。internal
`NumericUpDownSpinner`、`ButtonSpinnerDecoratedBox` 与 `EmbeddedTextBox` 不注册独立 descriptor，也不能通过模板
复用自动获得其他 owner 的 owner-scoped Semantic Style。

### 1.1 `root`

| 字段 | 值 |
| --- | --- |
| Owner | `NumericUpDown` |
| Part | `root` |
| Selector | NumericUpDown 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `NumericUpDown` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | NumericUpDown owner |
| 职责 | 承载数值、尺寸、variant、验证状态和 owner-scoped Semantic Style 入口。 |
| 相关 API | `Value`、`FormatString`、`SizeType`、`StyleVariant`、`Status`、`IsEnabled`、`IsReadOnly`、`IsAllowClear`、`Increment`、`Maximum`、`Minimum` |
| 相关 Token | SharedToken、`NumericUpDownToken`、`ButtonSpinnerToken` |
| 稳定性 | stable since 6.2.0 |

`root` 是控件自身，不声明 `.semantic-root` marker。它适合定制 NumericUpDown 整体 `BorderBrush`、`Opacity`、对齐和
尺寸约束；`BorderBrush` 会以 LocalValue 中继到输入 frame 生效（对齐 LineEdit 与上游 `styles.root.borderColor`
语义）——定制期间该属性槽的 hover / focus 变色冻结，focus 的 `BoxShadow` 光晕不受影响，置空后恢复 frame 状态机。
variant、effective status 与 CompactSpace 的状态归一仍由共享 frame 结构负责。

### 1.2 `prefix`

| 字段 | 值 |
| --- | --- |
| Owner | `NumericUpDown` |
| Part | `prefix` |
| Selector | `.semantic-prefix` |
| SelectorRoute | `/template/ .semantic-scope-spinner /template/ .semantic-scope-frame /template/ .semantic-scope-prefix > .semantic-prefix` |
| Style Type | `NumericUpDownPrefixStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | internal `AddOnContentPresenter`（最低 public 类型为 `ContentPresenter`） |
| 职责 | 承载 `InnerLeftContent` 与 `InnerLeftContentTemplate` 的最终呈现。 |
| 相关 API | `InnerLeftContent`、`InnerLeftContentTemplate` |
| 相关 Token | `SpacingXXS`、输入尺寸 padding |
| 稳定性 | stable since 6.2.0 |

`prefix` 是 NumericUpDown 模板中的稳定 presenter，通过 `ButtonSpinner.InnerLeftContent` 传递并由共享 frame 结构的
content 前缀槽呈现，两个模板变体的呈现槽一致。`InnerLeftContent=null` 且 template 也为 null 时 presenter 仍属于
静态模板结构；适合定制 `Opacity`、`Margin`、`Foreground` 和 presenter 级排版属性。
`InnerLeftContentTemplate` 创建的用户子树不属于 NumericUpDown Semantic Part。

### 1.3 `input`

| 字段 | 值 |
| --- | --- |
| Owner | `NumericUpDown` |
| Part | `input` |
| Selector | `.semantic-input` |
| SelectorRoute | `/template/ .semantic-input` |
| Style Type | `NumericUpDownInputStyle` |
| ContractType | `TextBox`（AtomUI public 控件） |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | internal `EmbeddedTextBox#PART_TextBox`（最低 public 类型为 `TextBox`） |
| 职责 | 承载数值文本的编辑表面，包含字体、文本对齐、光标与选择呈现。 |
| 相关 API | `Text`、`PlaceholderText`、`IsReadOnly`、`IsStringMode`、`FormatString`、`IsKeyboardEnabled` |
| 相关 Token | `FontSize`、文本与 caret 资源 |
| 稳定性 | stable since 6.2.0 |

`input` 的最低 public `ContractType` 是 AtomUI `TextBox`，而不是 internal `EmbeddedTextBox` 实现细节。与 LineEdit 的
`input`（TextPresenter）不同，NumericUpDown 的文本编辑表面由内嵌 `TextBox` 承担，marker 位于 owner 模板内的
`EmbeddedTextBox#PART_TextBox` 节点上，因此在 `Mode=Input` 与 `Mode=Spinner` 两个变体中使用同一条默认 route。
它适合定制 `Foreground`、`FontSize`、`Opacity`、`TextAlignment` 等文本级属性；数值解析、字符串模式与键盘行为仍由
`NumericUpDown` 拥有，不通过 Semantic Style 改写。

### 1.4 `suffix`

| 字段 | 值 |
| --- | --- |
| Owner | `NumericUpDown` |
| Part | `suffix` |
| Selector | `.semantic-suffix` |
| SelectorRoute | `/template/ .semantic-scope-spinner /template/ .semantic-scope-frame /template/ .semantic-scope-suffix > .semantic-suffix` |
| Style Type | `NumericUpDownSuffixStyle` |
| ContractType | `StackPanel` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | 内部后缀布局 `StackPanel` |
| 职责 | 组织 clear 与 `InnerRightContent` 的横向布局。 |
| 相关 API | `InnerRightContent`、`InnerRightContentTemplate`、`IsAllowClear` |
| 相关 Token | `UniformlyPaddingXXS`、输入尺寸 padding |
| 稳定性 | stable since 6.2.0 |

`suffix` 是稳定的布局区域，不等于用户 `InnerRightContent` 本身。适合定制 `Spacing`、`Opacity`、`Margin` 和对齐；
clear 与用户右侧内容仍各有边界，用户内容子树不由 `suffix` 契约继续展开。

### 1.5 `clear`

| 字段 | 值 |
| --- | --- |
| Owner | `NumericUpDown` |
| Part | `clear` |
| Selector | `.semantic-clear` |
| SelectorRoute | `/template/ .semantic-clear` |
| Style Type | `NumericUpDownClearStyle` |
| ContractType | `Avalonia.Controls.Button` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `InputClearIconButton#PART_ClearButton`（输入段内右缘，位于内部 + / 浮动 handle 之前） |
| 职责 | 提供清空当前数值的操作入口。 |
| 相关 API | `IsAllowClear`、`ClearIcon`、`IsReadOnly`、`Text` |
| 相关 Token | clear 按钮主题与 SharedToken |
| 稳定性 | stable since 6.2.0 |

`clear` 节点始终存在于两个模板变体的输入段内（与 `input` 同级，紧贴输入文本右缘），`IsEffectiveShowClearButton` 只切换可见性。它适合定制 `Opacity`、`Margin`、`Cursor` 和
Button 级交互属性；清除命令仍进入 `NotifyClearButtonClicked()` 的统一行为。

## 2. 模板变体与 route

`NumericUpDown` 有两个内置模板变体，Semantic 契约在两个变体中完全一致：

| 变体 | 触发条件 | spinner 呈现 | route 链 |
| --- | --- | --- | --- |
| Input 模式 | 默认（`IsButtonSpinnerFloatable=True`） | `ButtonSpinner` 基础模板的浮动 handle | `.semantic-scope-spinner` → `.semantic-scope-frame` → `.semantic-scope-prefix` / `.semantic-scope-suffix` |
| Spinner 模式 | `Mode=Spinner` | `NumericUpDownSpinner` 自有模板的 +/− 按钮 | 与 Input 模式同链 |

Spinner 模式的 `NumericUpDownSpinner` 模板把 `InnerLeftContent` / `InnerRightContent` 通过共享 frame 结构的
content 前缀 / 后缀槽呈现，与 Input 模式共用同一套 `.semantic-scope-*` 结构标记，因此每个 Part 只有一条
`SelectorRoute`，不随变体变化。`.semantic-scope-*` 是路由边界，不进入公开 Part 表。

## 3. Selector 用法

生成的 Style Type 已封装 NumericUpDown owner 类型保护和跨 spinner / frame 结构的 `SelectorRoute`，应用不直接复制
`/template/` 路径：

```xml
<Style Selector="atom|NumericUpDown.semantic-style-demo">
    <Setter Property="BorderBrush" Value="#597EF7" />

    <atom:NumericUpDownPrefixStyle x:SetterTargetType="ContentPresenter">
        <Setter Property="Foreground" Value="#597EF7" />
    </atom:NumericUpDownPrefixStyle>
    <atom:NumericUpDownInputStyle x:SetterTargetType="{x:Type atom:TextBox}">
        <Setter Property="Foreground" Value="#1677FF" />
    </atom:NumericUpDownInputStyle>
    <atom:NumericUpDownSuffixStyle x:SetterTargetType="StackPanel">
        <Setter Property="Opacity" Value="0.82" />
    </atom:NumericUpDownSuffixStyle>
    <atom:NumericUpDownClearStyle x:SetterTargetType="Button">
        <Setter Property="Opacity" Value="0.72" />
    </atom:NumericUpDownClearStyle>
</Style>
```

不得使用以下写法：

- `.semantic-root`、`PART_*`、Name selector、internal 类型或视觉祖先顺序作为应用主题契约。
- 把 `ContentPresenter.semantic-prefix`、`TextBox.semantic-input` 等 `ContractType` 写入 Part 身份 selector。
- 直接复制多层 `/template/ .semantic-scope-*` route；这些 scope class 只用于生成 Style 的 owner-relative 路由。
- 穿过 `InnerLeftContentTemplate` / `InnerRightContentTemplate` 创建的用户内容继续匹配内部 Visual。
- 把 `ButtonSpinner`、`ButtonSpinnerDecoratedBox` 或 `EmbeddedTextBox` 当作 descriptor owner；它们的契约属于
  `NumericUpDown` owner。

## 4. 状态与数量语义

五个 Part 均为 `Single`。prefix、suffix、clear 使用静态模板节点，状态变化只切换内容、可见性或有效视觉值，不增删
marker，也不改变对象身份。`Mode` 切换会重建模板子树，两个变体重新实例化后仍提供相同的 Part 集合与 marker。

| 场景 | root | prefix | input | suffix | clear | 说明 |
| --- | --- | --- | --- | --- | --- | --- |
| 默认空值 | 1 | 1 | 1 | 1 | 1（隐藏） | 静态节点均已实例化。 |
| 有值且 `IsAllowClear=true` | 1 | 1 | 1 | 1 | 1（可见） | clear 只改变可见性。 |
| `InnerLeftContent=null` | 1 | 1 | 1 | 1 | 1 | prefix presenter 保持存在。 |
| `InnerRightContent=null` | 1 | 1 | 1 | 1 | 1 | suffix 保持存在。 |
| `Mode` 切换 | 1 | 1（重建） | 1（重建） | 1（重建） | 1（重建） | 变体模板提供相同 Part。 |
| disabled / read-only | 1 | 1 | 1 | 1 | 1 | 只改变交互和有效视觉。 |
| Error / Warning / focus / hover | 1 | 1 | 1 | 1 | 1 | 状态由 owner/frame 投影，marker 不变。 |

## 5. 定制边界

以下区域明确不属于 NumericUpDown Semantic Part：

- 增减按钮：Spinner 模式的 `PART_IncreaseButton` / `PART_DecreaseButton` 与 Input 模式的 `ButtonSpinnerHandle`
  浮动 handle 及其内部按钮。
- 外部 `LeftAddOn` / `RightAddOn` 区域及其用户模板内容。
- placeholder、`ButtonSpinnerContentPanel`、 CompactSpace 几何、effective status 和 motion actor。
- `InnerRightContentTemplate` / `InnerLeftContentTemplate` 创建的用户子树。
- `PART_*` 名称、internal 类型、`.semantic-scope-*` 路由标记与模板层级。

增减按钮区域计划以兼容方式作为新的 Optional / Single Part 开放；在此之前其内部节点不属于公共契约。

默认主题不消费 `.semantic-*` selector；静态 marker 只提供应用样式命中点，不改变默认属性优先级或增加状态订阅。
Semantic Style 服从 Avalonia 原生属性优先级，布局结果仍可能受 owner、frame 与 presenter 的 Min/Max、Padding、
Margin 与裁剪约束。

## 6. 兼容性与验证

删除或重命名 Part、修改 selector class / route、收窄 `ContractType`、改变 cardinality，或者让任一内置模板变体缺少
marker，均属于公共主题契约变更。

验证至少覆盖：

- `NumericUpDown` descriptor 只有 `root`、`prefix`、`input`、`suffix`、`clear`，字段值与本文一致。
- 两个模板变体（Input / Spinner）各自包含 `semantic-prefix`、`semantic-suffix`、`semantic-clear`、
  `semantic-input` 与 `semantic-scope-spinner` marker；spinner frame 结构提供 `semantic-scope-frame`、
  `semantic-scope-prefix`、`semantic-scope-suffix` 结构标记；root 不声明 `.semantic-root`。
- 生成的 `NumericUpDown*Style` 可以编译并跨 owner、spinner 与共享 frame 结构命中最低 public `ContractType`，
  且 Input 与 Spinner 模式命中相同 Part 集合。
- clear、prefix、suffix 在内容、可见性、read-only、disabled、status 与 focus 变化时保持对象身份和 `Single` 数量。
- 默认主题不消费 `.semantic-*`，未声明用户 Semantic Style 时不增加 selector activator。
- Gallery Semantic Parts Tab 延迟创建 Preview，展示 Input 与 Spinner 模式的强类型 Style 示例，且每个可静态解析的
  Part 均可高亮解析唯一目标。
- descriptor、生成 Style 与 NativeAOT 路径使用编译期生成数据，不依赖运行时反射或 VisualTree 扫描。
