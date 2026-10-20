# LineEdit 桌面版架构设计

本文档定义 `AtomUI.Desktop.Controls.LineEdit` 输入控件家族的最新设计定位、公共契约、状态模型、视觉主题关系和兼容边界。输入控件共享分层见 [输入控件共享架构设计](../input-control-architecture-design.md)，通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，公开主题区域见 [LineEdit Semantic Part 契约](semantic-part.md)，内部实现原理见 [LineEdit 桌面版实现原理](implementation.md)，Token 专项设计见 [LineEdit Token 设计](token.md)，设计和契约变化记录见 [LineEdit Changelog](changelog.md)。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/DataEntry/LineEdit` |
| 控件状态 | Stable |

LineEdit 是 AtomUI 桌面数据录入体系中的文本输入控件家族，用于承载单行文本、密码输入、搜索输入和多行文本输入。它以 Avalonia `TextBox` 文本编辑能力为基础，通过 `AbstractTextInput` 统一文本输入逻辑，通过 `InputControlFrame` 统一输入表面视觉，再接入 AtomUI 的尺寸、native validation error 投射、清除按钮、密码 reveal、字数统计、Addon、CompactSpace、Form 和 Token 体系。

LineEdit 家族包含以下稳定入口：

| 类型 | 定位 |
| --- | --- |
| `AbstractTextInput` | 公开但不作为直接实例化入口的抽象文本输入基类，统一尺寸、输入表面状态、清除、reveal、字数统计、Form、native validation、CompactSpace 和文本 viewport 度量。 |
| `TextBox` | AtomUI 基础文本框，继承 `AbstractTextInput`，提供单行文本编辑、内部左右内容和基础输入模板。 |
| `LineEdit` | 标准单行输入框，继承 `AbstractTextInput`，提供外部 AddOn、单行布局和搜索/组合控件复用入口。 |
| `SearchEdit` | 搜索输入框，在 `LineEdit` 基础上加入搜索按钮、搜索按钮样式、加载态和搜索事件；独立契约见 [SearchEdit 桌面版架构设计](../search-edit/overview.md)。 |
| `TextArea` | 多行输入框，继承 `AbstractTextInput`，复用输入状态、尺寸、清除、字数统计、Form 和 TextArea 专属 resize 模型。 |
| `InputControlFrame` | internal 输入表面组合控件，统一 variant、effective status、边框、背景、圆角、阴影、交互伪类、CompactSpace 和动效；不作为公共控件入口。 |

继承与组合关系固定为：

```text
Avalonia.Controls.TextBox
        ↓
AbstractTextInput
   ├── TextBox
   ├── LineEdit
   └── TextArea

LineEdit → SearchEdit
TextBox  → EmbeddedTextBox
LineEdit → AutoCompleteLineEditBox
TextArea → AutoCompleteTextAreaBox

InputControlFrame
        ↓
AddOnDecoratedBox
        ├── SearchEditDecoratedBox
        ├── TextAreaDecoratedBox
        ├── SelectAddOnDecoratedBox
        ├── TreeSelectAddOnDecoratedBox
        ├── CascaderAddOnDecoratedBox
        └── ButtonSpinnerDecoratedBox
```

`AbstractTextInput` 只承载文本输入逻辑和状态归一；`InputControlFrame` 只承载输入表面视觉。`AddOnDecoratedBox` 及其派生类型负责 AddOn、内部内容和专用布局扩展，不重复声明输入表面状态 owner。

LineEdit 家族只负责文本输入与文本相关辅助操作，不负责结构化选择、日期时间选择、数值步进、异步候选数据管理、富文本编辑或表达式计算。

## 2. 设计语言

LineEdit 的设计语言来自可编辑文本表面和输入辅助内容的组合。

| 维度 | 含义 | 典型表达 |
| --- | --- | --- |
| 文本语义 | 用户输入或查看原始文本。 | `Text`、`PlaceholderText`、`IsReadOnly`、`PasswordChar`。 |
| 输入密度 | 控件在表单和工具栏中的尺寸等级。 | `SizeType=Large/Middle/Small/Custom`。 |
| 输入表面 | 边框和背景的视觉强度。 | `Outlined`、`Filled`、`Borderless`、`Underlined`。 |
| 反馈状态 | 输入的校验和业务状态。 | `Default`、`Error`、`Warning`。 |
| 辅助内容 | 外部附加、内部前后缀、清除、密码显示、字数统计和 Form feedback。 | AddOn、`InnerLeftContent`、`InnerRightContent`、`IsAllowClear`、`IsEnableRevealButton`、`IsShowCount`。 |
| 专用输入 | 搜索、多行、自动高度和可拖拽调整高度。 | `SearchEdit`、`TextArea`、`IsAutoSize`、`IsResizable`。 |

`Custom` 尺寸不是第四套预设 Token。它以 `Middle` 作为未显式设置时的视觉基线，并允许用户通过 `Height`、`FontSize`、`Padding` 等常规属性覆盖实际尺寸。

## 3. API 与契约模型

LineEdit 家族的公共 API 由 Avalonia 文本编辑 API 与 AtomUI 输入扩展 API 组成。Avalonia API 的文本编辑、选择、滚动、密码、换行、只读和数据校验语义必须保持兼容；AtomUI 扩展只负责外观、辅助内容和集成能力。

基础文本 API：

| API | 语义 |
| --- | --- |
| `Text` | 当前输入文本，支持双向绑定。 |
| `PlaceholderText` | 空文本状态下的占位提示。 |
| `IsReadOnly` | 文本可见但禁止编辑。 |
| `PasswordChar` / `RevealPassword` | 密码输入和 reveal 状态。 |
| `MaxLength` | 文本最大长度，字数统计显示依赖该值。 |
| `SelectionStart` / `SelectionEnd` / `CaretIndex` | 文本选择和光标位置。 |

AtomUI 输入扩展 API：

| API | 所属类型 | 语义 |
| --- | --- | --- |
| `SizeType` | `TextBox`、`LineEdit`、`SearchEdit`、`TextArea` | 输入尺寸密度，类型为 `CustomizableSizeType`。 |
| `IsCustomFontSize` | `TextBox`、`LineEdit`、`SearchEdit` | 为 `true` 时不由 SizeType 样式覆盖 `FontSize`。 |
| `IsAllowClear` | `TextBox`、`LineEdit`、`SearchEdit`、`TextArea` | 是否允许展示清除按钮。 |
| `ClearIcon` | `TextBox`、`LineEdit`、`SearchEdit`、`TextArea` | 清除按钮图标。 |
| `IsEnableRevealButton` | `TextBox`、`LineEdit`、`SearchEdit` | 是否展示密码 reveal 按钮。 |
| `IsShowCount` | `TextBox`、`LineEdit`、`SearchEdit`、`TextArea` | 是否展示字数统计。 |
| `IsMotionEnabled` | 全家族 | 是否启用内部按钮动效。 |
| `StyleVariant` | `TextBox`、`LineEdit`、`SearchEdit`、`TextArea` | 输入表面样式，由 `AbstractTextInput` 统一归一并传递给 `InputControlFrame`。 |
| `Status` | `TextBox`、`LineEdit`、`SearchEdit`、`TextArea` | 显式输入反馈状态；最终视觉由 `InputControlFrame.EffectiveStatus` 计算，native validation error 仍以 `DataValidationErrors` 为唯一真源。 |
| `LeftAddOn` / `LeftAddOnTemplate` | `LineEdit`、`SearchEdit` | 外部左侧附加内容和模板。 |
| `RightAddOn` / `RightAddOnTemplate` | `LineEdit` | 外部右侧附加内容和模板；SearchEdit 的右侧外部 add-on 位置由搜索按钮占用。 |
| `InnerLeftContent` / `InnerRightContent` | `TextBox`、`LineEdit`、`TextArea` | 继承的内部左右内容，支持文本、控件和隐式 DataTemplate；内容不随编辑区滚动，置空时不占布局空间。 |
| `InnerLeftContentTemplate` / `InnerRightContentTemplate` | `LineEdit`、`TextArea` | 内部前后缀模板。 |

`TextBox` 的内部左右内容位于输入边框内，不提供 `LineEdit` 的外部 AddOn 能力。左右内容跟随 `VerticalContentAlignment`，右侧内容与清除、密码 reveal、Form feedback 和字数统计共用辅助区。设置内部左右内容时，非空辅助区与文本编辑区之间采用 `SpacingXXS` 间距；两侧内容均为空时保留原有编辑区和辅助按钮布局。`TextBox` 不增加专用的内容模板属性，数据对象通过标准 `DataTemplates` 渲染。

密码掩码沿用 `PasswordChar` 指定字符的字体字形和原生文本布局。需要常规圆点外观时可设置 `PasswordChar="●"`；星号 `*` 在字体行框中通常偏上，不通过平移整个文本 presenter 补偿，以保持光标、选择和命中位置一致。

Root 表面定制 API：

| API | 语义 |
| --- | --- |
| `Background` / `BorderBrush` | `TemplatedControl` 标准属性，是输入表面 root 定制的唯一通道，不引入平行定制属性。owner 上出现任何有效值（本地值、应用层 Style Setter 或 `DynamicResource` 求值结果）时，由 `AbstractTextInput` 以 LocalValue 中继到 `InputControlFrame` 同名属性，优先级高于 frame 状态机：定制期间 hover / pressed / focus 引起的边框变色让位，focus 的 `BoxShadow` 光晕作用在独立属性槽、不受边框定制影响。清除定制（置 `null`）后 frame 恢复主题状态机。语义与内联样式（本地值优先于状态类）一致。 |

SearchEdit 专项 API：

| API | 语义 |
| --- | --- |
| `SearchButtonStyle` | 搜索按钮样式，支持 `Default` 和 `Primary`。 |
| `SearchButtonText` | 搜索按钮文字。 |
| `IsOperating` | 搜索进行中状态；显示按钮 loading 并阻止重复搜索请求。 |
| `IsSearchOnEnterEnabled` | 是否允许 Enter 键触发搜索请求，默认启用。 |
| `SearchRequested` | 按钮或 Enter 键触发的搜索请求路由事件。 |

TextArea 专项 API：

| API | 语义 |
| --- | --- |
| `Lines` | 固定行数模式下的显示行数。 |
| `MinLines` / `MaxLines` | 自动高度和 resize 的行数边界。 |
| `IsAutoSize` | 是否根据文本内容自动调整高度。 |
| `IsResizable` | 是否展示右下角 resize handle。 |

稳定 template part：

| Template Part | 类型 | 所属主题 | 职责 |
| --- | --- | --- | --- |
| `PART_InputControlFrame` | `InputControlFrame` / `AddOnDecoratedBox` 派生类型 | 全家族输入主题 | 输入表面、边框、背景、状态视觉、CompactSpace 和动效承载；派生 decorated box 只增加 AddOn 或专用布局。 |
| `PART_ScrollViewer` / `ScrollViewer` | `ScrollViewer` | 全家族 | 文本滚动区域。 |
| `PART_TextPresenter` | `InputTextPresenter` | 全家族 | 文本显示、光标、选择和密码 reveal。 |
| `Placeholder` | `TextBlock` | 全家族 | 空文本占位提示。 |
| `PART_ClearButton` | `InputClearIconButton` | 全家族 | 清除当前文本。 |
| `PART_RevealButton` | `RevealButton` | `TextBox`、`LineEdit`、`SearchEdit` | 切换密码 reveal。 |
| `FormFeedBack` / `PART_FormFeedBack` | `ContentPresenter` | `LineEdit`、`TextBox`、`TextArea` | Form feedback 内容承载。 |
| `InnerRightContentPresenter` / `PART_InnerRightContentPresenter` | `ContentPresenter` | `LineEdit`、`SearchEdit`、`TextArea` | 内部右侧内容承载。 |
| `TextCountIndicator` | `TextBlock` | 全家族 | 字数统计显示。 |
| `PART_ResizeHandle` | `ResizeHandle` | `TextAreaTheme` | TextArea 高度拖拽入口。 |

## 4. 行为与状态模型

LineEdit 家族的交互优先级：

```text
Disabled
> Native Error
> Form Error
> Form Warning
> Explicit Warning
> Explicit Error
> Focus
> PointerOver
> Normal
```

输入状态由三个来源组成：

| 来源 | owner | 语义 |
| --- | --- | --- |
| `NativeValidationStatus` | `DataValidationErrors` | Avalonia 原生校验结果；`Error` 是 native error 的唯一视觉真源。 |
| `FormStatus` | Form 集成层 | Form 写入的 warning、success、validating 及其 feedback；Form error 通过 Form-owned native validation entry 写入 `DataValidationErrors`。 |
| `ExplicitStatus` | `AbstractTextInput.Status` | 用户显式设置的 error/warning 请求，不覆盖 native error，也不修改 Form 自己写入的状态。 |

有效状态由 `InputControlFrame` 计算：

```text
Native Error / Form Error
    > Form Warning
    > Explicit Warning
    > Explicit Error
    > Default
```

Form reset 只清理 Form 自己写入的状态；`Status=Error` 不写入 native error；`Warning` 伪类只在最终有效状态为 Warning 时出现。

`Disabled` 表示不可交互，文本使用 disabled 文本色，内部按钮不可作为操作入口。`IsReadOnly` 保持文本可见和可选中，但不允许编辑或清除。

清除按钮有效状态：

```text
TextBox / LineEdit / SearchEdit:
IsEffectiveShowClearButton =
  IsAllowClear
  && !IsReadOnly
  && !AcceptsReturn
  && !string.IsNullOrEmpty(Text)

TextArea:
IsEffectiveShowClearButton =
  IsAllowClear
  && !IsReadOnly
  && !string.IsNullOrEmpty(Text)
```

Form 集成以 `Text` 作为表单值。错误校验状态以 Avalonia `DataValidationErrors` 为真源，Form validator 产生的 error 应写入同一 native validation 通道；`IFormItemAware.NotifyValidateStatus` 只负责同步 `Warning`、`Success`、`Validating` 等 Form 扩展状态和 feedback 可见性。feedback 内容通过 `IFormItemFeedbackAware` 进入模板中的 feedback presenter。

CompactSpace 只影响相邻输入框之间的有效圆角和边框折叠，不改变文本编辑语义。

## 5. 视觉与主题模型

LineEdit 家族使用输入壳体和文本 presenter 分层：

| 主题 | 职责 |
| --- | --- |
| `AbstractTextInput` shared template contract | 统一文本输入属性、placeholder、clear/reveal/count、Form feedback、native validation、CompactSpace 和 viewport metrics 接入。 |
| `InputControlFrameTheme.axaml` | Outlined、Filled、Borderless、Underlined、hover、focus-within、pressed、disabled、error、warning、corner、shadow、CompactSpace 和 motion。 |
| `TextBoxTheme.axaml` | 基础文本编辑模板、文本 presenter、TextBox 专属 padding、clear/reveal、字数统计和 frame 组合。 |
| `LineEditTheme.axaml` | 单行文本布局、外部 AddOn、内部前后缀和 `InputControlFrame` 组合。 |
| `SearchEditTheme.axaml` | 搜索输入模板、搜索按钮状态传递和 `SearchEditDecoratedBox` 组合。 |
| `SearchEditDecoratedBoxTheme.axaml` | 搜索按钮与 frame 的一体化布局；不重复实现 frame 状态 selector。 |
| `TextAreaTheme.axaml` | 多行文本、字数统计、固定行数和 `InputControlFrame` 组合。 |
| `TextAreaDecoratedBoxTheme.axaml` | TextArea 内部 padding、右侧附加内容、scroll viewer 和 resize 相关布局。 |
| `InputClearIconButtonTheme.axaml` / `RevealButtonTheme.axaml` | 内部 action 按钮视觉。 |

共享输入表面值由 `SharedToken` 和 `InputControlFrameTheme` 消费。`TextBoxToken`、`LineEditToken`、`TextAreaToken` 只保留各自稳定的文本尺寸、padding 和多行 resize 专属语义，不再承载输入表面边框、背景、focus shadow、error/warning 或 disabled 状态。

## 6. 控件家族或集成关系

LineEdit 是 Data Entry 文本输入家族的根入口，与 NumericUpDown、DatePicker、TimePicker、Select、TreeSelect、ComboBox 等控件共享 `SizeType`、`StyleVariant`、`Status`、Addon、Form 和 CompactSpace 语义。

集成关系：

- Avalonia `TextBox`：提供文本编辑、选择、滚动、密码和基础输入事件。
- `AbstractTextInput`：统一文本输入扩展属性、状态来源、Form/native validation、清除、字数统计、CompactSpace 和 viewport metrics。
- `InputControlFrame`：提供输入表面和有效状态视觉；不保存文本值，不管理 AddOn 内容。
- `AddOnDecoratedBox`：在 frame 基础上提供外部 AddOn、内部前后缀和内容布局。
- `SearchEditDecoratedBox`：在 AddOnDecoratedBox 基础上加入搜索按钮布局和点击回调，不复制 frame selector。
- `TextAreaDecoratedBox`：在 AddOnDecoratedBox 基础上加入多行输入 padding、scroll viewer 接入和 resize 约束。
- `IFormItemAware` / `IFormItemFeedbackAware`：提供表单值、Form 扩展状态和 feedback 内容接入；error 由 `DataValidationErrors` 投射。
- `ICompactSpaceAware`：提供 CompactSpace 中圆角和边框折叠协同。

## 7. 兼容性不变量

维护 LineEdit 家族时必须保持以下不变量：

- 不改变 Avalonia `TextBox` 的 `Text`、选择、光标、密码、滚动和只读语义。
- 不擅自新增、删除、重命名或改变 public API、template part、Token 名称或主题 key。
- `SizeType=Custom` 以 `Middle` 作为未显式覆盖时的默认视觉基线。
- `IsCustomFontSize=true` 时不得由 SizeType 样式覆盖用户设置的 `FontSize`。
- 清除按钮只在有效状态为 true 时显示，且清除动作进入统一 `Clear()` 语义。
- `InputControlFrame` 是所有输入表面状态 selector、边框、背景、focus shadow、error/warning、disabled 和 motion 的唯一 owner。
- `AddOnDecoratedBox` 及其派生类型只承载 AddOn、内部内容和专用布局，不复制 frame 的状态计算或视觉 selector。
- `StyleVariant`、`Status`、`SizeType`、Form 状态和 native validation 必须先由 `AbstractTextInput` 归一，再通过稳定绑定传给 frame。
- 输入主题不在 owner 层携带 `Background` / `BorderBrush` 默认值，frame 主题是 rest 态取值的唯一来源；`TextBox` 模板不以 `TemplateBinding` 绑定这两个属性。owner 值是“用户是否定制 root 表面”的判定输入，恢复任何 owner 层默认都会使中继判定失效。
- root `Background` / `BorderBrush` 中继语义保持不变：owner 有值 → frame 以 LocalValue 接管（定制期间该属性槽的交互态变色冻结），owner 置空 → frame `ClearValue` 恢复状态机；focus 反馈依赖 `BoxShadow` 独立属性槽，不随边框定制失效。
- `SearchEdit.IsOperating=true` 时按钮和 Enter 键不重复触发 `SearchRequested`。
- `TextArea.Lines` 必须遵守 `MinLines` / `MaxLines`，resize 不得突破行数边界。
- Form feedback 订阅必须在 detach 时释放。
- TextPresenter 的 margin、placeholder、selection、caret 和 disabled 文本色属于输入模板契约，不应在业务控件中用 magic width 补偿。
- LineEdit 的 `root`、`prefix`、`input`、`suffix`、`clear`、`count` Part 名称、route、最低 public `ContractType` 与 `Single` 数量语义必须保持稳定。
- `SearchEdit` 与 `TextArea` 拥有各自注册的 descriptor（见 Semantic Part 契约 1.7 / 1.8）；`TextBox` 不因继承或模板复用自动获得家族 descriptor，扩展其契约必须单独评审 public owner 边界。

## 8. 专项模型

### 8.1 Custom SizeType 模型

`CustomizableSizeType.Custom` 表示用户接管尺寸属性。主题层仍把 `Custom` 归入 `Middle` 的默认 line height、corner radius 和 font size 分支；用户显式设置 `Height`、`FontSize`、`Padding` 等属性时，由 Avalonia 属性优先级和 `IsCustomFontSize` 控制最终效果。

### 8.2 SearchEdit 模型

SearchEdit 将搜索按钮视为输入框的一部分。搜索按钮从 `InputControlFrame.EffectiveStatus`、`StyleVariant`、`SizeType` 和 `IsEnabled` 接收组合视觉；native validation error 优先投射到 frame 与按钮状态色，之后才回退到 Form/显式状态。`SearchButtonStyle` 只控制按钮风格，不改变文本编辑行为。

### 8.3 TextArea 高度模型

TextArea 支持固定行数、自动高度和拖拽 resize。固定行数模式下，控件根据字体、line height 和文本 presenter/scroll viewer 的垂直间距计算高度。resize 模式记录拖拽开始时的高度，并把高度限制在 `MinLines` / `MaxLines` 对应的范围内。

### 8.4 Semantic Part

`LineEdit` 公开 `root`、`prefix`、`input`、`suffix`、`clear`、`count` 六个职责区域。所有 Part 均为 `Single`；prefix、
suffix、clear、count 的内容或可见性变化不增删 marker。完整 selector route、`ContractType`、尺寸矩阵和定制边界见
[LineEdit Semantic Part 契约](semantic-part.md)。

`LineEdit`、`SearchEdit` 与 `TextArea` 各自注册独立 descriptor：`SearchEdit` 额外提供 `button` Part，
`TextArea` 提供 `textarea` Part。placeholder、reveal、Form feedback、外部 AddOn 和用户内容模板子树仍不属于
Semantic Part；完整 selector route、`ContractType` 与定制边界见
[LineEdit 家族 Semantic Part 契约](semantic-part.md)。

## 9. 文档导航与验证策略

关联文档：

- [LineEdit 桌面版实现原理](implementation.md)
- [LineEdit Semantic Part 契约](semantic-part.md)
- [LineEdit Token 设计](token.md)
- [LineEdit Changelog](changelog.md)

验证策略：

| 层次 | 验证内容 |
| --- | --- |
| 文档 | `overview.md`、`implementation.md`、`semantic-part.md`、`token.md`、`changelog.md` 链接有效，Data Entry 分类入口包含 LineEdit。 |
| C# 状态 | 清除按钮、字数统计、Form value、Form feedback、CompactSpace、SearchEdit loading 和 TextArea resize。 |
| AXAML/Theme | template part、Semantic marker / route、variant、status、focus、disabled、SizeType、Custom size、AddOn、TextArea resize handle。 |
| Token | `LineEditToken`、`TextAreaToken`、生成的 TokenKind、Token 类型、生成数据和主题引用一致。 |
| Gallery | 走查 Semantic Preview / Style、基础用法、尺寸、variant、AddOn、清除、密码、前后缀、状态、SearchEdit、TextArea、自动高度、字数统计和 resize 示例。 |
