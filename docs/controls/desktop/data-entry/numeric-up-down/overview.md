# NumericUpDown 桌面版架构设计

本文档定义 `AtomUI.Desktop.Controls.NumericUpDown` 桌面版的最新设计定位、公共契约、数值状态模型、视觉主题关系和兼容边界。共享输入分层见 [输入控件共享架构设计](../input-control-architecture-design.md)，通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，内部实现原理见 [NumericUpDown 桌面版实现原理](implementation.md)，NumericUpDown Token 的专项设计见 [NumericUpDown Token 设计](token.md)，设计和契约变化记录见 [NumericUpDown Changelog](changelog.md)。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/DataEntry/NumberUpDown` |
| 控件状态 | Stable |

NumericUpDown 是 AtomUI 桌面数据录入体系中的数值输入控件，用于在文本输入、步进按钮、键盘和鼠标滚轮之间建立统一的十进制数值编辑体验。它以 Avalonia `NumericUpDown` 为基础，保留原生数值、格式化、步进和范围约束语义，并接入 AtomUI 的输入外观、Addon、CompactSpace、Form 和 Token 体系。

NumericUpDown 的职责是编辑单个 `decimal?` 数值，或在 string mode 下以 `StringValue` 保存高精度原始输入文本并同步可解析的 `Value`。它不承担表达式计算、单位换算、多值范围输入、校验消息展示、数据源管理或异步选择职责。

Gallery 中该控件以 `NumberUpDown` 页面展示；实际控件类型为 `AtomUI.Desktop.Controls.NumericUpDown`。

## 2. 设计语言

NumericUpDown 的设计语言来自输入框与微调按钮的组合。

| 维度 | 含义 | 典型表达 |
| --- | --- | --- |
| 数值语义 | 输入值是可增减的数值，而不是普通文本。 | `Value`、`Minimum`、`Maximum`、`Increment`、`FormatString`。 |
| 输入密度 | 控件在表单、工具栏或紧凑布局中的尺寸等级。 | `Large`、`Middle`、`Small`、`Custom`。 |
| 输入表面 | 控件边框和背景的视觉强度。 | `Outlined`、`Filled`、`Borderless`。 |
| 反馈状态 | 输入的校验和业务状态。 | `Default`、`Error`、`Warning`。 |
| 展示模式 | 控件以输入框或三段式拨轮呈现。 | `Mode=Input`、`Mode=Spinner`。 |
| 辅助内容 | 输入框前后附加说明、单位、协议、图标或清除入口。 | `LeftAddOn`、`RightAddOn`、`InnerLeftContent`、`InnerRightContent`、`IsAllowClear`。 |

步进能力是输入框语义的一部分，不是独立操作按钮组。`Input` 模式使用右侧浮动 Handle，在普通状态下提供数值增减入口；`Spinner` 模式使用左减号、中间输入、右加号的三段式结构。

## 3. API 与契约模型

NumericUpDown 的公共 API 由继承的数值编辑 API 与 AtomUI 输入扩展 API 组成。继承 API 的语义必须与 Avalonia `NumericUpDown` 保持一致，AtomUI 扩展只负责外观、辅助内容和集成能力。

数值编辑 API：

| API | 类型 | 语义 |
| --- | --- | --- |
| `Value` | `decimal?` | 控件的数值值，支持双向绑定和数据校验。 |
| `Text` | `string?` | 当前显示文本，受转换器、格式化和输入状态影响。 |
| `Minimum` / `Maximum` | `decimal` | 可接受数值范围。 |
| `Increment` | `decimal` | 单次步进增减值。 |
| `ClipValueToMinMax` | `bool` | 文本转换为数值时是否裁剪到范围。 |
| `NumberFormat` | `NumberFormatInfo?` | 数值格式化和解析使用的区域格式。 |
| `FormatString` | `string` | 非编辑状态下的显示格式。 |
| `ParsingNumberStyle` | `NumberStyles` | 文本解析规则。 |
| `TextConverter` | `IValueConverter?` | 自定义 `Text` 与 `Value` 的双向转换器。 |
| `AllowSpin` | `bool` | 是否允许按钮、键盘和鼠标滚轮触发步进。 |
| `ShowButtonSpinner` | `bool` | 是否显示步进按钮；`Mode=Input` 控制浮动 Handle，`Mode=Spinner` 控制左右 action 段。 |

String mode API：

| API | 类型 | 语义 |
| --- | --- | --- |
| `IsStringMode` | `bool` | 启用原始字符串值保存模式。 |
| `StringValue` | `string?` | string mode 下的原始输入文本。 |

AtomUI 输入扩展 API：

| API | 类型 | 语义 |
| --- | --- | --- |
| `Mode` | `NumericUpDownMode` | 控件展示模式，默认 `Input`，可设置为 `Spinner`。 |
| `SizeType` | `CustomizableSizeType` | 输入框尺寸密度；`Custom` 未显式覆盖时以 `Middle` 为视觉基线。 |
| `IsCustomFontSize` | `bool` | 为 `true` 时内部 `TextBox` 不由 `SizeType` 字号样式覆盖 `FontSize`。 |
| `StyleVariant` | `InputControlStyleVariant` | 输入表面样式。 |
| `Status` | `InputControlStatus` | 显式输入反馈状态；最终视觉由 `InputControlFrame.EffectiveStatus` 计算，native validation error 以 `DataValidationErrors` 为唯一真源。 |
| `IsAllowClear` | `bool` | 是否展示清除按钮。 |
| `ClearIcon` | `PathIcon?` | 清除按钮图标。 |
| `IsKeyboardEnabled` | `bool` | 是否允许方向键和 PageUp / PageDown 触发步进。 |
| `IsMotionEnabled` | `bool` | 是否启用输入壳体、Handle 和清除按钮相关动效。 |
| `LeftAddOn` / `RightAddOn` | `object?` | 外部前后附加内容。 |
| `LeftAddOnTemplate` / `RightAddOnTemplate` | `object?` | 外部附加内容模板；实际模板契约为 `IDataTemplate?`。 |
| `InnerLeftContent` / `InnerRightContent` | `object?` | 内部前后缀内容。 |
| `InnerLeftContentTemplate` / `InnerRightContentTemplate` | `object?` | 内部前后缀模板；实际模板契约为 `IDataTemplate?`。 |

稳定 template part：

| Template Part | 类型 | 职责 |
| --- | --- | --- |
| `PART_Spinner` | `ButtonSpinner` | `InputControlFrame` / AddOnDecoratedBox 组合、外部 AddOn、内部前后缀、步进入口和 CompactSpace 布局承载。 |
| `PART_TextBox` | `TextBox` | 文本输入、占位符、只读、数据校验和文本双向绑定。 |
| `PART_ClearButton` | `InputClearIconButton` | 清除 `Value` 的内部按钮。 |
| `PART_InnerRightContentPresenter` | `ContentPresenter` | 用户 `InnerRightContent` 的内部右侧内容承载。 |
| `PART_DecreaseButton` | `IconButton` | `Mode=Spinner` 下触发减小步进。 |
| `PART_IncreaseButton` | `IconButton` | `Mode=Spinner` 下触发增加步进。 |

## 4. 行为与状态模型

NumericUpDown 的交互优先级：

```text
Disabled
> ReadOnly
> Spin availability
> Pressed
> PointerOver
> Focus
> Normal
```

`Disabled` 表示控件不可交互，TextBox 文本使用禁用文字色，输入壳体使用禁用背景，浮动 Handle 不显示，清除按钮不应作为可操作入口出现。

`IsReadOnly` 保留文本可见但禁止编辑和步进。`AllowSpin=false` 禁止按钮、键盘和鼠标滚轮步进，但不等价于禁用控件。`IsKeyboardEnabled=false` 只拦截 `Up`、`Down`、`PageUp` 和 `PageDown` 的步进行为。

数值状态：

```text
Text input
  ↓ parse by TextConverter or ParsingNumberStyle
Value
  ↓ format by TextConverter / FormatString / NumberFormat
Text display
```

String mode 保留原始输入文本，并在能解析时同步 `Value`。Form 集成仍以 `Value` 作为表单值。

清除按钮有效状态：

```text
IsEffectiveShowClearButton =
  IsAllowClear
  && !IsReadOnly
  && !string.IsNullOrEmpty(Text)
```

`Mode` 只改变展示结构，不改变 `Value`、`Text`、`Minimum`、`Maximum`、`Increment`、`AllowSpin`、`ShowButtonSpinner`、键盘、滚轮、Form 或 string mode 的数值语义。

`SizeType=Custom` 进入自定义尺寸路径。用户未显式设置 `Height`、`FontSize`、`Padding` 等尺寸属性时，主题层应以 `Middle` 作为默认视觉基线；用户显式接管 `FontSize` 时，应通过 `IsCustomFontSize=true` 防止内部 `TextBox` 的 `SizeType` 字号样式覆盖用户设置。

## 5. 视觉与主题模型

Input 模式投射根 inset 与外框几何；Spinner 模式保留左右 action 段的布局，根 padding 进入文本区。

几何默认、原生优先级和组合边界见[共享输入表面](../input-control-architecture-design.md#81-根表面几何与预设默认值)。


NumericUpDown 采用按需模板模型。`Mode=Input` 使用默认输入框模板，以 `ButtonSpinner` 作为输入壳体；`Mode=Spinner` 使用独立三段式拨轮模板，只在用户显式启用 spinner 模式时创建左右步进按钮和分隔视觉。

视觉层级要求：

- `Mode=Input` 的默认模板不得预埋 spinner 模式左右按钮或无职责 wrapper；`ShowButtonSpinner=false` 时必须隐藏浮动 Handle。
- `Mode=Spinner` 使用独立 `ControlTemplate`，不通过同一模板内两套视觉树加 `IsVisible` 切换实现；`ShowButtonSpinner=false` 时必须隐藏左右 action 段。
- `ButtonSpinner` 是默认输入组合边界，复用 `InputControlFrame` 的输入表面和有效状态，不应被普通 `Border` 或 `Grid` 包装替代。
- `PART_TextBox` 的 `BorderThickness=0` 是为了避免内层 TextBox 与外层输入壳体重复绘制边框。
- `PART_ClearButton` 与 `PART_InnerRightContentPresenter` 共用内部右侧 stack，必须保留顺序：清除按钮在用户内部右侧内容之前。
- 浮动 Handle 由 `ButtonSpinnerDecoratedBox` 控制透明度和偏移，不应在 NumericUpDown 模板中动态创建或移除。
- 编辑器和后缀由同一个输入布局面板协调：后缀避让浮动 Handle 时，编辑器的实际可交互宽度同步收窄，不能依靠层级覆盖保持点击。
- Spinner 模式左右 action 按钮按实际高度维持正方形，分隔线在按钮之外占位，图标保持居中。

状态视觉由共享主题分层承担：

| 主题 | 职责 |
| --- | --- |
| `NumericUpDownTheme.axaml` | 装配控件结构和传递状态。 |
| `NumericUpDownSpinnerTheme.axaml` | 装配 NumericUpDown 专用 inline spinner 模板及其 action 按钮状态。 |
| `ButtonSpinnerTheme.axaml` | 装配 spinner 壳体和 Handle 内容。 |
| `ButtonSpinnerDecoratedBoxTheme.axaml` | 输入壳体、Addon、浮动 Handle 透明度和偏移。 |
| `ButtonSpinnerHandleTheme.axaml` | Handle 背景、边框、图标尺寸和交互视觉。 |
| `TextBoxTheme.axaml` | 文本编辑器、placeholder、disabled 文本色和内部文本 presenter。 |
| `InputControlFrameTheme.axaml` | 输入 variant、effective status、focus、hover、pressed、error、warning、disabled 和 motion 外观。 |

## 6. 控件家族或集成关系

NumericUpDown 属于 Data Entry 控件，与 LineEdit、TextBox、TextArea、Select、ComboBox、DatePicker、TimePicker 等控件共享 `InputControlFrame` 的输入表面、状态、variant、Addon 和 Form 集成语义；其数值、格式化和步进状态仍由 NumericUpDown 自身拥有，内部文本编辑器按文本输入家族契约接入。

集成关系：

- Avalonia `NumericUpDown`：继承数值编辑、格式化、步进、事件和基础文本同步语义。
- `ButtonSpinner`：提供 frame 上的步进 Handle、Addon、CompactSpace 和 spinner 布局；不重新定义输入状态优先级。
- AtomUI `TextBox`：提供文本输入、占位符、禁用文本色和清除按钮相关基础能力。
- `IFormItemAware`：允许 Form 读取、设置、清空 `Value`；error 通过 `DataValidationErrors` 投射到 `InputControlFrame`，warning 等扩展状态通过 `FormStatus` / feedback 表达。
- `ICompactSpaceAware`：允许 CompactSpace 统一边框折叠和圆角。
- `IMotionAwareControl`：统一动效开关。
- `ICustomizableSizeTypeAware`：接入支持 `Custom` 的输入尺寸模型。

## 7. 兼容性不变量

维护 NumericUpDown 时必须保持以下不变量：

- 不修改继承自 Avalonia `NumericUpDown` 的数值、格式化、步进和事件契约。
- `ShowButtonSpinner` 必须同时作用于 `Mode=Input` 的浮动 Handle 和 `Mode=Spinner` 的左右 action 段。
- 不擅自新增、删除、重命名或改变 AtomUI public API。
- `Mode=Input` 默认行为和渲染效果不变；spinner 模式不能让默认用户承担额外视觉树或额外交互订阅成本。
- `Mode=Spinner` 只改变展示结构，不改变数值解析、格式化、步进、Form、CompactSpace 或 string mode 语义。
- `SizeType=Custom` 必须以 `Middle` 作为未显式覆盖时的默认视觉基线。
- `IsCustomFontSize=true` 时不得由内部 `TextBox` 的 `SizeType` 字体样式覆盖用户设置的 `FontSize`。
- `IsStringMode=true` 时必须保留原始 `StringValue`。
- `IsKeyboardEnabled=false` 只屏蔽步进快捷键，不屏蔽普通文本输入。
- 清除按钮只在允许清除、非只读且文本非空时显示。
- 禁用态下浮动 Handle 不显示；禁用文本必须使用 disabled 文本色。
- Filled 变体下 Handle 背景必须使用 `FilledHandleBg`。
- Template part 名称不变。
- CompactSpace 下的有效边框厚度、圆角和位置协同不变。
- Token 名称和语义不擅自重命名、删除或迁移为实例状态。

如果实现某项能力时无法保持这些不变量，应先停止实现，说明原因、影响范围、替代方案和迁移方式，并获得授权。

## 8. 专项模型

### 8.1 Custom SizeType 模型

`CustomizableSizeType.Custom` 表示用户接管 NumericUpDown 的尺寸属性。主题层仍把 `Custom` 归入 `Middle` 的 line height、corner radius 和 font size 默认分支；用户显式设置 `Height`、`FontSize`、`Padding` 等属性时，由 Avalonia 属性优先级和 `IsCustomFontSize` 控制最终效果。

`SizeType` 必须同时传递给 `ButtonSpinner`、内部 `TextBox` 和 Handle 相关主题，使 `Input` 与 `Spinner` 两种模式在 Large / Middle / Small / Custom 下共享同一输入尺寸语义。

### 8.2 String Mode 模型

String mode 的核心目标是保存用户输入文本，同时尽可能同步可计算数值。`StringValue` 是原始输入状态，`Value` 是可计算状态，仅在文本能解析为 `decimal` 时同步。

### 8.3 浮动 Handle 模型

NumericUpDown 使用 ButtonSpinner 的浮动 Handle 模型。Handle normal 状态默认隐藏，pointer hover 输入壳体时显示；禁用态不显示 Handle；Filled 状态的 Handle 背景由 `FilledHandleBg` 控制。

### 8.4 Spinner Mode 模型

Spinner mode 对齐 参考 InputNumber 的 `mode="spinner"` 设计：同一个数值输入控件在保留数值语义的前提下，切换为左减号、中间输入、右加号的三段式展示。

### 8.5 Form 集成模型

NumericUpDown 通过 `IFormItemAware` 暴露表单值能力。Form 集成只以 `Value` 作为表单值，不直接使用 `StringValue`。

### 8.6 Semantic Part

`NumericUpDown` 公开 `root`、`prefix`、`input`、`suffix`、`clear` 五个职责区域。所有 Part 均为 `Single`，且在
`Mode=Input` 与 `Mode=Spinner` 两个模板变体中提供相同的 marker 与 route；`Mode` 切换重建模板子树后仍提供相同
Part 集合。完整 selector route、`ContractType`、模板变体矩阵和定制边界见
[NumericUpDown Semantic Part 契约](semantic-part.md)。

增减按钮区域（Spinner 模式的 +/− 按钮与 Input 模式的浮动 handle）尚未开放为 Semantic Part，其内部节点不属于
公共契约。

## 9. 文档导航与验证策略

关联文档：

- [NumericUpDown 桌面版实现原理](implementation.md)
- [NumericUpDown Semantic Part 契约](semantic-part.md)
- [NumericUpDown Token 设计](token.md)
- [NumericUpDown Changelog](changelog.md)

`prefix`、`suffix` 与共享 frame 结构的 content 前缀 / 后缀槽对齐；`input` 由内嵌 `TextBox` 承担并在两个模板变体中
使用同一条默认 route。增减按钮、外部 AddOn、placeholder 与用户内容模板子树不属于 Semantic Part。完整契约见
[NumericUpDown Semantic Part 契约](semantic-part.md)。

验证策略：

| 层次 | 验证内容 |
| --- | --- |
| 文档 | `overview.md`、`implementation.md`、`token.md`、`changelog.md` 链接有效。 |
| C# 状态 | custom size、string mode、键盘步进开关、清除按钮、Form、CompactSpace 状态同步。 |
| AXAML/Theme | template part、variant、disabled、error、warning、Input / Spinner 模板切换、Handle 和 clear button。 |
| Token | `NumericUpDownToken`、ButtonSpinnerToken 复用和 Token 类型、生成数据和 token.md 语义说明一致。 |
| Gallery | 走查基础用法、custom size、string mode、键盘行为、鼠标滚轮、最小最大值、小数步进、尺寸、变体、禁用、前后缀、清除按钮和状态示例。 |
