# LineEdit 家族 Semantic Part 契约

本文档定义 LineEdit 输入控件家族公开的 Semantic Part、Selector、类型约束、数量语义和定制边界，覆盖 public owner
`LineEdit`、`SearchEdit` 与 `TextArea`。家族整体设计见
[LineEdit 桌面版架构设计](overview.md)，真实模板、状态投影与尺寸基线见
[LineEdit 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Semantic Parts

`LineEdit` 公开 `root`、`prefix`、`input`、`suffix`、`clear`、`count` 六个职责区域（§1.1–1.6）；`SearchEdit`
公开 `root`、`prefix`、`input`、`suffix`、`clear`、`button`（§1.7）；`TextArea` 公开 `root`、`textarea`、`clear`、
`count`（§1.8）。每个 descriptor 只属于各自的 public owner；`TextBox` 与 internal `AddOnDecoratedBox` 不注册独立
descriptor，也不能通过继承关系自动获得其他 owner 的 owner-scoped Semantic Style。

### 1.1 `root`

| 字段 | 值 |
| --- | --- |
| Owner | `LineEdit` |
| Part | `root` |
| Selector | LineEdit 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `LineEdit` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | LineEdit owner |
| 职责 | 承载文本值、输入状态、尺寸、variant、验证状态和 owner-scoped Semantic Style 入口。 |
| 相关 API | `Text`、`SizeType`、`StyleVariant`、`Status`、`IsEnabled`、`IsReadOnly`、`IsAllowClear`、`IsShowCount`、`Background`、`BorderBrush` |
| 相关 Token | SharedToken、`LineEditToken` |
| 稳定性 | stable since 6.2.0 |

`root` 是控件自身，不声明 `.semantic-root` marker。它适合定制 LineEdit 整体 `Background`、`BorderBrush`、
`BorderThickness`、`Opacity`、对齐和尺寸约束；其中 `Background` / `BorderBrush` 由 `AbstractTextInput` 以 LocalValue
中继到 `InputControlFrame` 生效——定制期间该属性槽的 hover / pressed / focus 变色冻结，focus 的 `BoxShadow` 光晕不受
影响，置空后恢复 frame 状态机。variant、effective status、CompactSpace 与 native validation 的状态归一仍由
`AbstractTextInput` 和 `InputControlFrame` 负责。

### 1.2 `prefix`

| 字段 | 值 |
| --- | --- |
| Owner | `LineEdit` |
| Part | `prefix` |
| Selector | `.semantic-prefix` |
| SelectorRoute | `/template/ .semantic-scope-input-frame /template/ .semantic-scope-prefix > .semantic-prefix` |
| Style Type | `LineEditPrefixStyle` |
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

`prefix` 是 LineEdit 模板中的稳定 presenter。internal `AddOnContentPresenter` 保留 template-only 场景的 child 创建与可见性
语义，同时通过 public 基类 `ContentPresenter` 约束 Setter。`InnerLeftContent=null` 且 template 也为 null 时 presenter 仍属于
静态模板结构；模板内容变化不改变 Part 身份。适合定制 `Opacity`、`Margin`、`Padding`、对齐和 presenter 级排版属性。
`InnerLeftContentTemplate` 创建的用户子树不属于 LineEdit Semantic Part。

### 1.3 `input`

| 字段 | 值 |
| --- | --- |
| Owner | `LineEdit` |
| Part | `input` |
| Selector | `.semantic-input` |
| SelectorRoute | `/template/ .semantic-input` |
| Style Type | `LineEditInputStyle` |
| ContractType | `TextPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `InputTextPresenter#PART_TextPresenter` |
| 职责 | 绘制当前文本、光标、选择范围和密码 reveal 结果。 |
| 相关 API | `Text`、`CaretIndex`、`SelectionStart`、`SelectionEnd`、`PasswordChar`、`RevealPassword` |
| 相关 Token | `FontSize`、`LineHeight`、选择色与 caret 资源 |
| 稳定性 | stable since 6.2.0 |

`input` 的最低 public `ContractType` 是 Avalonia `TextPresenter`，而不是 internal 实现细节。它适合定制 `Opacity`、
`FontSize`、`FontWeight`、`FontStyle`、`TextAlignment` 和局部 Margin。文本 viewport、选择布局、caret 与密码显示仍属于文本输入
内核，不通过 Semantic Style 改写其 ownership。

### 1.4 `suffix`

| 字段 | 值 |
| --- | --- |
| Owner | `LineEdit` |
| Part | `suffix` |
| Selector | `.semantic-suffix` |
| SelectorRoute | `/template/ .semantic-scope-input-frame /template/ .semantic-scope-suffix > .semantic-suffix` |
| Style Type | `LineEditSuffixStyle` |
| ContractType | `StackPanel` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | 内部后缀布局 `StackPanel` |
| 职责 | 组织 clear、reveal、Form feedback、内部右侧内容和 count 的横向布局。 |
| 相关 API | `InnerRightContent`、`InnerRightContentTemplate`、`IsAllowClear`、`IsEnableRevealButton`、`IsShowCount` |
| 相关 Token | `UniformlyPaddingXXS`、输入尺寸 padding |
| 稳定性 | stable since 6.2.0 |

`suffix` 是稳定的布局区域，不等于用户 `InnerRightContent` 本身。适合定制 `Spacing`、`Opacity`、`Margin`、对齐和布局方向；
clear 与 count 仍拥有各自更窄的 Part。reveal、Form feedback 和用户右侧内容没有独立 Semantic Part，其内部结构也不由
`suffix` 契约继续展开。

### 1.5 `clear`

| 字段 | 值 |
| --- | --- |
| Owner | `LineEdit` |
| Part | `clear` |
| Selector | `.semantic-clear` |
| SelectorRoute | `/template/ .semantic-scope-input-frame /template/ .semantic-scope-suffix > .semantic-suffix > .semantic-clear` |
| Style Type | `LineEditClearStyle` |
| ContractType | `Avalonia.Controls.Button` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `InputClearIconButton#PART_ClearButton` |
| 职责 | 提供清除当前文本的操作入口。 |
| 相关 API | `IsAllowClear`、`ClearIcon`、`IsReadOnly`、`Text` |
| 相关 Token | clear 按钮主题与 SharedToken |
| 稳定性 | stable since 6.2.0 |

`clear` 节点始终存在，`IsEffectiveShowClearButton` 只切换可见性。它适合定制 `Opacity`、`Margin`、`Padding`、`Cursor` 和
Button 级交互属性；清除命令仍必须进入 `NotifyClearButtonClicked()` / `Clear()` 的统一行为，不通过样式替换文本状态源。

### 1.6 `count`

| 字段 | 值 |
| --- | --- |
| Owner | `LineEdit` |
| Part | `count` |
| Selector | `.semantic-count` |
| SelectorRoute | `/template/ .semantic-scope-input-frame /template/ .semantic-scope-suffix > .semantic-suffix > .semantic-count` |
| Style Type | `LineEditCountStyle` |
| ContractType | `TextBlock` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `TextBlock#TextCountIndicator` |
| 职责 | 展示当前文本长度与 `MaxLength` 的计数文案。 |
| 相关 API | `IsShowCount`、`Text`、`MaxLength` |
| 相关 Token | `ColorTextPlaceholder`、字体与行高资源 |
| 稳定性 | stable since 6.2.0 |

`count` 节点始终存在，`IsShowCount=false` 只切换可见性。它适合定制 `Foreground`、`FontSize`、`FontWeight`、`Opacity`、
`Margin` 和对齐；计数格式和刷新时机由 `AbstractTextInput` 维护，不属于 Semantic Style。

### 1.7 `SearchEdit`

`SearchEdit` 注册独立 descriptor，Part 集合为 `root`、`prefix`、`input`、`suffix`、`clear`、`button`。`prefix`、
`input`、`suffix`、`clear` 的 selector、route、`ContractType` 与 LineEdit 同名 Part 一致，只是 owner-scoped Style
类型换为 `SearchEdit*` 前缀（如 `SearchEditInputStyle`，`SetterTargetType` 仍为最低 public 类型）。以下只列出差异字段：

| 字段 | `button` |
| --- | --- |
| Owner | `SearchEdit` |
| Part | `button` |
| Selector | `.semantic-button` |
| SelectorRoute | `/template/ .semantic-scope-input-frame /template/ .semantic-button` |
| Style Type | `SearchEditButtonStyle` |
| ContractType | `Button`（AtomUI） |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | internal `SearchEditDecoratedBox` 模板内的 `atom:Button#PART_RightAddOn` |
| 职责 | 承载搜索动作按钮的根视觉、文字与图标（loading 状态沿用按钮自身的 loading 呈现）。 |
| 相关 API | `SearchButtonStyle`、`SearchButtonText`、`SearchButtonTheme`、`IsOperating` |
| 稳定性 | stable since 6.2.0 |

`button` 的 marker 由 `SearchEditDecoratedBox` 在模板应用后通过 C# 追加（RuntimeCreated 契约），因此主题资产内没有
静态 `Classes.semantic-button` 声明。`SearchEdit` 不提供 `count` Part：其模板不包含计数指示器。`root` 不生成 Style；
用户定制搜索按钮整体背景 / 边框时应作用在 `button` Part 而不是 `root`。

### 1.8 `TextArea`

`TextArea` 注册独立 descriptor，Part 集合为 `root`、`textarea`、`clear`、`count`。`clear`、`count` 的 selector、
route、`ContractType` 与 LineEdit 同名 Part 一致（`TextArea*` Style 前缀）；`root` 由生成器隐式补齐。以下只列出
差异字段：

| 字段 | `textarea` |
| --- | --- |
| Owner | `TextArea` |
| Part | `textarea` |
| Selector | `.semantic-textarea` |
| SelectorRoute | `/template/ .semantic-textarea` |
| Style Type | `TextAreaTextareaStyle` |
| ContractType | `TextPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `InputTextPresenter#PART_TextPresenter` |
| 职责 | 承载多行文本的输入、光标、选择与换行展示。 |
| 相关 API | `Text`、`Lines`、`MinLines`、`MaxLines`、`IsAutoSize`、`IsResizable` |
| 稳定性 | stable since 6.2.0 |

`TextArea` 的 `count` 位于 owner 模板底部（DockPanel 下缘），route 为默认 `/template/ .semantic-count`；`clear`
位于右侧 addon 区，route 与 LineEdit 同形。`TextArea` 不提供 `prefix` / `suffix` Part；resize handle 与
`Placeholder` 文本不属于 Semantic Part。

## 2. Selector 用法

生成的 Style Type 已封装 LineEdit owner 类型保护和跨 `AddOnDecoratedBox` 模板的 `SelectorRoute`，应用不直接复制
`/template/` 路径：

```xml
<Style Selector="atom|LineEdit.semantic-style-demo">
    <Setter Property="Background" Value="#F0F5FF" />
    <Setter Property="BorderBrush" Value="#597EF7" />

    <atom:LineEditPrefixStyle x:SetterTargetType="ContentPresenter">
        <Setter Property="Opacity" Value="0.82" />
        <Setter Property="Margin" Value="2,0" />
    </atom:LineEditPrefixStyle>
    <atom:LineEditInputStyle x:SetterTargetType="TextPresenter">
        <Setter Property="Opacity" Value="0.88" />
    </atom:LineEditInputStyle>
    <atom:LineEditSuffixStyle x:SetterTargetType="StackPanel">
        <Setter Property="Spacing" Value="8" />
    </atom:LineEditSuffixStyle>
    <atom:LineEditClearStyle x:SetterTargetType="Button">
        <Setter Property="Opacity" Value="0.72" />
    </atom:LineEditClearStyle>
    <atom:LineEditCountStyle x:SetterTargetType="TextBlock">
        <Setter Property="Foreground" Value="#531DAB" />
        <Setter Property="FontWeight" Value="SemiBold" />
    </atom:LineEditCountStyle>
</Style>
```

不得使用以下写法：

- `.semantic-root`、`PART_*`、Name selector、internal 类型或视觉祖先顺序作为应用主题契约。
- 把 `ContentPresenter.semantic-prefix`、`TextPresenter.semantic-input` 等 `ContractType` 写入 Part 身份 selector。
- 直接复制多层 `/template/ .semantic-scope-*` route；这些 scope class 只用于生成 Style 的 owner-relative 路由。
- 穿过 `InnerLeftContentTemplate` / `InnerRightContentTemplate` 创建的用户内容继续匹配内部 Visual。
- 把 `TextBox` 当作 LineEdit / SearchEdit / TextArea 的 descriptor owner；`SearchEdit` 与 `TextArea` 的
  契约属于各自 owner，不因继承或模板复用自动获得 LineEdit descriptor。

## 3. 状态与数量语义

六个 Part 均为 `Single`。prefix、suffix、clear、count 使用静态模板节点，状态变化只切换内容、可见性或有效视觉值，不增删
marker，也不改变对象身份。

| 场景 | root | prefix | input | suffix | clear | count | 说明 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 默认空输入 | 1 | 1 | 1 | 1 | 1（隐藏） | 1（按配置） | 静态节点均已实例化。 |
| 有文本且 `IsAllowClear=true` | 1 | 1 | 1 | 1 | 1（可见） | 1 | clear 只改变可见性。 |
| `InnerLeftContent=null` | 1 | 1 | 1 | 1 | 1 | 1 | prefix presenter 保持存在。 |
| `InnerRightContent=null` | 1 | 1 | 1 | 1 | 1 | 1 | suffix 保持存在。 |
| `IsShowCount=false` | 1 | 1 | 1 | 1 | 1 | 1（隐藏） | count marker 不变。 |
| disabled / read-only | 1 | 1 | 1 | 1 | 1 | 1 | 只改变交互和有效视觉。 |
| Error / Warning / focus / hover | 1 | 1 | 1 | 1 | 1 | 1 | 状态由 owner/frame 投影，marker 不变。 |

## 4. 尺寸基线

Semantic marker 接入不得改变 LineEdit 当前预设尺寸。测试固定以下内置模板基线，单位为 Avalonia layout unit：

| SizeType | Outlined | Filled | Borderless | Underlined |
| --- | ---: | ---: | ---: | ---: |
| Large | 40 | 40 | 38 | 39 |
| Middle | 32 | 32 | 30 | 31 |
| Small | 22 | 22 | 20 | 21 |

`Custom` 继续使用 Middle 作为未显式覆盖时的视觉基线。prefix 和 suffix 的自然高度必须不超过 owner 的有效高度；应用通过
Semantic Style 设置固定 `Width` / `Height`、大额 Margin 或 Padding 时，应自行评估文本 viewport、clear/count 排列和
CompactSpace 几何，不能把偏离基线解释为 descriptor 问题。

## 5. 定制边界

以下区域明确不属于 LineEdit Semantic Part：

- 外部 `LeftAddOn` / `RightAddOn` 区域及其用户模板内容。
- placeholder、scroll viewer、selection、caret、密码 reveal 按钮和 Form feedback。
- `InnerRightContentTemplate` / `InnerLeftContentTemplate` 创建的用户子树。
- `InputControlFrame` 的边框绘制节点、CompactSpace 几何、effective status 和 motion actor。
- `TextArea` resize handle、`TextBox` 基础模板和 `OtpLineEdit` 单元格；`SearchEdit` 的搜索按钮由
  `SearchEdit` 自身的 `button` Part 覆盖（见 1.7）。
- `PART_*` 名称、internal 类型、`.semantic-scope-*` 路由标记与模板层级。

默认主题不消费 `.semantic-*` selector；静态 marker 只提供应用样式命中点，不改变默认属性优先级或增加状态订阅。
Semantic Style 服从 Avalonia 原生属性优先级，布局结果仍可能受 owner、frame、scroll viewer 和 presenter 的 Min/Max、Padding、
Margin 与裁剪约束。

## 6. 兼容性与验证

删除或重命名 Part、修改 selector class / route、收窄 `ContractType`、改变 cardinality，或者让内置 LineEdit 模板缺少任一
marker，均属于公共主题契约变更。

验证至少覆盖：

- `LineEdit` descriptor 只有 `root`、`prefix`、`input`、`suffix`、`clear`、`count`，字段值与本文一致；
  `SearchEdit` 注册 `root`、`prefix`、`input`、`suffix`、`clear`、`button`；`TextArea` 注册
  `root`、`textarea`、`clear`、`count`；`OtpLineEdit` 注册 `root`、`cellList`、`cell`、`separator`
  （`cell`、`separator` 为 `Multiple` + RuntimeCreated），字段值与 `otp-line-edit/overview.md` 一致。
- LineEdit 模板的五个公开 marker 与三个内部 route scope 静态存在，prefix 使用 `AddOnContentPresenter` 保持 template-only 语义，root 不声明 `.semantic-root`。
- 生成的 `LineEdit*Style` 可以编译并跨 LineEdit / AddOnDecoratedBox 两层模板命中最低 public `ContractType`。
- clear、count、prefix、suffix 在内容、可见性、read-only、disabled、status 与 focus 变化时保持对象身份和 `Single` 数量。
- Large / Middle / Small × Outlined / Filled / Borderless / Underlined 尺寸矩阵保持本文基线。
- prefix 同时支持 `InnerLeftContent`、`InnerLeftContentTemplate` 和 template-only 输入，不丢失内容。
- 默认主题不消费 `.semantic-*`，未声明用户 Semantic Style 时不增加 selector activator。
- Gallery Semantic Parts Tab 延迟创建 Preview，展示 LineEdit 六 Part、LineEdit 密码模式（Password）六 Part、
  SearchEdit 六 Part（含 RuntimeCreated 搜索按钮）、TextArea 四 Part 与 OtpLineEdit 四 Part（`cell`、
  `separator` 为 Multiple + RuntimeCreated）的强类型 Style 示例，且每个可静态解析的 Part 均可高亮解析唯一目标。
- descriptor、生成 Style 与 NativeAOT 路径使用编译期生成数据，不依赖运行时反射或 VisualTree 扫描。
