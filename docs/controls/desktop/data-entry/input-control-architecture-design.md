# 输入控件共享架构设计

组合根排版已接通控制 Effective Global 与原生样式优先级：选择、日期时间、数值和公开 ButtonSpinner 的字体族未明确配置时沿真实继承父更新，根前景经模板到达实际输入与框体。NumericUpDown 保持原真实文本 14 DIP 默认，内部 EmbeddedTextBox 不再覆盖根字号；内部 NumericUpDownSpinner 不接受公开 ButtonSpinner 的可选默认。各 owner 的三档差异及释放合同见[共享主题消费](../../../modules/controls/shared-theme-consumers.md)。

本文档定义 AtomUI 桌面数据录入控件共享的文本输入逻辑层、输入表面视觉层、状态归一、组合控件边界、Token 资源边界和 Form/native validation 集成规则。`LineEdit` 输入家族的具体 API 与 TextArea 行为见 [LineEdit 桌面版架构设计](line-edit/overview.md)；Select、NumericUpDown、DatePicker、TimePicker、Cascader、TreeSelect 等控件只在各自文档中描述业务状态和专用布局，不重复定义本架构的输入表面规则。

## 1. 设计定位

AtomUI 的输入控件由两个正交层组成：

```text
Avalonia.Controls.TextBox
        ↓
AbstractTextInput
        ↓
TextBox / LineEdit / TextArea
        ↓
InputControlFrame
        ↓
AddOnDecoratedBox 及其专用派生类型
```

`AbstractTextInput` 是文本输入逻辑和共有状态的唯一 owner；`InputControlFrame` 是输入表面视觉和有效状态的唯一 owner。`Select`、`ComboBox`、`NumericUpDown`、日期/时间选择器等组合控件保留各自的业务状态，但必须通过同一套 `FormStatus`、`DataValidationErrors` 和 frame 绑定接入输入表面。各层通过稳定的属性绑定、template part 和接口协作，不能互相读取对方的私有模板节点，也不能形成第二套状态源。

## 2. 设计原则

1. 文本值、选择、光标、密码、滚动、placeholder、清除、reveal、字数统计、Form value 和文本 viewport 度量由 `AbstractTextInput` 或其具体文本控件维护。
2. 边框、背景、圆角、focus shadow、hover、pressed、disabled、error、warning、CompactSpace 和 motion 由 `InputControlFrame` 维护。
3. `AddOnDecoratedBox` 只负责 AddOn、内部前后缀、操作区和专用布局；派生 decorated box 不重复实现 frame 的状态归一或主题 selector。
4. `DataValidationErrors` 是 native error 的唯一真源。显式 `Status` 不写入 native validation；Form warning 进入 `FormStatus`，不修改 `ExplicitStatus`，由统一有效状态优先级决定最终视觉。
5. Token 只表达稳定的组件值，不承载文本值、实例状态、Form 状态、搜索运行状态或当前交互状态。
6. 内部 frame 不作为公共 Semantic Part；公共控件只暴露稳定的输入、placeholder、clear、reveal、feedback 和 count 语义区域。
7. 模板结构通过 AXAML、TemplateBinding、compiled binding、稳定 part 和生成器注册表达，不使用运行时反射扫描或动态主题发现。

## 3. 逻辑层模型

### 3.1 `AbstractTextInput`

`AbstractTextInput` 是公开但不作为直接实例化入口的抽象文本输入基类，统一拥有以下能力：

- `StyleVariant`、`Status`、`SizeType`、`IsMotionEnabled`。
- `IsAllowClear`、`ClearIcon`、密码 reveal 和字数统计。
- placeholder、Form value 读写、Form feedback 接入和 native validation 订阅。
- `FormStatus`、CompactSpace 状态和文本 viewport 度量。
- 清除按钮有效性、模板重应用、旧 part 解绑和通用清除行为。

具体控件只增加自己的语义：`TextBox` 提供基础单行编辑，`LineEdit` 提供 AddOn 与单行布局，`TextArea` 提供多行、固定行数、自动高度和 resize。`SearchEdit` 继承 `LineEdit`；`EmbeddedTextBox` 继承 `TextBox`；AutoComplete 的文本输入 box 继承对应的 `LineEdit` 或 `TextArea`。

### 3.2 `InputControlFrame`

`InputControlFrame` 是 internal composition control，统一接收：

- `StyleVariant`、`SizeType`、`IsMotionEnabled`。
- `Status`、`FormStatus` 和绑定到 frame 的 `DataValidationErrors`；其中三者分别对应显式状态、Form 扩展状态和 native validation 状态。
- `IsEnabled`、focus-within、pointer over、pressed 和 CompactSpace 状态。

它输出 `EffectiveStatus` 和视觉伪类，不拥有 `Text`、候选集合、搜索任务、Form value 或 AddOn 内容。

## 4. 状态模型

状态来源严格分为三类：

| 状态来源 | owner | 说明 |
| --- | --- | --- |
| `NativeValidationStatus` | `DataValidationErrors` | native error 的唯一来源；外部绑定错误和 Form-owned error 共享该通道。 |
| `FormStatus` | Form 集成层 | Form 的 warning、success、validating 和 feedback 投影；Form reset 只清理 Form 自己写入的状态。 |
| `ExplicitStatus` | `AbstractTextInput.Status` | 用户显式的 error/warning 请求；不写入 native validation，也不清除 native error。 |

有效状态优先级为：

```text
Native Error / Form Error
    > Form Warning
    > Explicit Warning
    > Explicit Error
    > Default
```

只有最终 `EffectiveStatus=Warning` 时才设置 `:warning`。`FormStatus=Success/Validating` 主要驱动 feedback 和 Form 扩展视觉，不伪造 native error，也不改变显式 error/warning 的优先级。`Status=Error` 不能覆盖或伪造 `DataValidationErrors`；native error 出现时，warning、success 或显式状态只能作为被遮蔽的低优先级来源保留。

## 5. 输入表面与变体

`InputControlFrame` 统一表达四种输入表面：

| `StyleVariant` | frame 语义 |
| --- | --- |
| `Outlined` | 边框、圆角、hover/focus shadow 和 CompactSpace 边框折叠。 |
| `Filled` | 填充背景、无独立外框的状态变化和相邻布局折叠。 |
| `Borderless` | 透明背景、透明边框、无 focus shadow；适用于嵌入式文本输入。 |
| `Underlined` | 只保留下划线状态，状态色和动效仍由 frame 统一处理。 |

`EmbeddedTextBox` 使用 `StyleVariant=Borderless` 表达无 chrome 语义：背景透明、边框透明、`BorderThickness=0`、不绘制 focus shadow，也不重复绘制外部 frame。

## 6. 组合控件边界

`AddOnDecoratedBox` 是 `InputControlFrame` 的布局扩展。以下派生类型只扩展各自内容，不重新拥有输入表面状态：

- `SearchEditDecoratedBox`：搜索按钮和搜索输入一体化布局。
- `TextAreaDecoratedBox`：多行 padding、scroll viewer 和 resize 布局。
- `SelectAddOnDecoratedBox`、`TreeSelectAddOnDecoratedBox`、`CascaderAddOnDecoratedBox`：选择结果、handle、tag 和 popup 入口布局。
- `ButtonSpinnerDecoratedBox`：步进 action、handle 和数值输入布局。

NumericUpDown、Select、ComboBox、TreeSelect、Cascader、DatePicker、TimePicker、SearchEdit、AutoComplete 和 Mentions 等控件可以拥有各自的业务状态和按钮/弹层状态，但其输入表面状态必须投射到同一个 `InputControlFrame` 语义，不得在专用 decorated box、Button 或内部 TextBox 中复制另一套 error/warning/disabled/variant owner。

## 7. Form 与验证数据流

```text
Form.SetValue / user input
        ↓
AbstractTextInput value owner
        ↓
IFormItemAware.ValueChanged

Form validator error
        ↓
Form-owned DataValidationErrors entry
        ↓
NativeValidationStatus
        ↓
InputControlFrame.EffectiveStatus

Form warning/success/validating
        ↓
FormStatus
        ↓
InputControlFrame + FormFeedback
```

Form 不维护与 native validation 并行的 error 真源。Form reset、重新验证和 detach 只能移除 Form 自己写入的 error、`FormStatus`、feedback 订阅和资源，不得清理外部 binding 或业务代码写入的 native error。

## 8. Token、主题和 AOT 边界

- `InputControlFrameTheme`提供输入表面状态与绘制；颜色、边框、add-on预设padding及动效开关通过OwnerTokenResource消费实际公开owner的有效Global，内部资产不建立重复identity或Own。未挂到公开owner时回退共享资源。
- `TextBoxToken`、`LineEditToken`、`TextAreaToken` 只提供文本尺寸、padding 和 TextArea resize 等稳定差异。
- 选择、搜索、日期、时间和数值控件的 Own Token 只表达各自业务布局，不复制 frame 的通用边框和状态 Token。
- `InputControlFrame` 是 internal composition control，不新增公共 Semantic Part 或运行时反射入口。
- 新增基类、frame、主题叶子和 Token resource 由 AtomUI Generator 显式注册到主题 asset/descriptor；不使用程序集扫描、字符串类型发现或动态主题 fallback。
- 控件局部 Token scope 允许通过控件主题把 `ColorPrimary`、hover 或透明背景等值投射到 frame；这类 selector 只改变 frame 的主题输入，必须继续以 `EffectiveStatus`、focus、hover 和 variant 为条件，不能重新计算或保存状态。

### 8.1 根表面几何与预设默认值

公开 owner 的 `Padding`、`BorderThickness`、`CornerRadius` 通过 ControlTheme 投射到实际表面。
几何默认值读取该 owner 的 Effective Global；有 Own 内容 padding 的 TextBox 使用自己的 ContentPadding 系列。
Avalonia 元数据默认值与主题生效后的 getter 不等同：主题提供实际默认几何，实例值和应用样式使用原生优先级覆盖。
内部 frame 的尺寸选择只能产生预设默认值，不能以 size trigger 覆盖 owner 的 TemplateBinding。

| 组合 | 几何职责 |
| --- | --- |
| TextBox | Padding、边宽和 CompactSpace 派生圆角投射到 InputControlFrame。 |
| LineEdit / SearchEdit / ComboBox / Select / Cascader / TreeSelect | Padding 投射到内容 frame；边宽、圆角继续经过 AddOn/variant/CompactSpace 的既有几何分配。 |
| DatePicker / TimePicker 及范围入口 | 具体 owner 提供 Effective Global 默认值，共享 InfoPicker 模板只投射输入。 |
| 活动范围指示条 | 共享RangeInfoPickerInput发布实际活动输入宽度、X及框体底边；ControlTheme绑定Rectangle几何。TimePicker的两个范围Own明确配置只覆盖RangeTimePicker，缺省／清除保留原通用InfoPickerInput Own。 |
| TextArea | 外框保持零 padding；owner padding 进入内层内容区，保留右侧操作区、滚动和 resize 的空间分配。 |
| ButtonSpinner / NumericUpDown | 内容 inset 与 handle 占位分离；数值 Spinner 模式保持 action 段无额外 padding，文本区使用 owner inset。 |

`InputControlFrame.SizeCornerRadius`、`AddOnDecoratedBox.PresetContentPadding` 与
`ButtonSpinner` 的内部 preset 参数只存放 size 所选默认值；有效几何仍由原生属性系统决定。
这些不是额外的 Token identity，也不是第二套主题或状态机。

多选 Custom 的紧凑 padding 是主题默认函数：读取当前 Height、边宽、variant 和既有多选 Token，
在 owner Padding 默认层计算，再让实例/应用样式覆盖。禁止在内部 frame 里再次压缩已生效的用户 padding。
可见 inset 和标签的内在测量预算分别维护：垂直测量预留至少保留名义 preset，需要更大的实际 inset 时同步约束内容，
避免无边框/下划线的默认标签因紧凑 inset 而被拉高。标签最小尺寸、Height 和 Min/Max 约束继续生效。

根画刷未提供时不接管 frame 状态；显式画刷沿既有 LocalValue 合同覆盖 hover 等默认状态，清除后恢复。
声明式替换必须同时保留初始缺省、非空和 clear 行为；不能用始终写入 null 的绑定代替条件投射。

SearchEdit 的搜索按钮高度跟随最终输入表面；自然高度测量使用当前输入/AddOn 内容与按钮最小高度，
不能把按钮上一次已排列高度反馈为新的输入最小高度。padding 或字体恢复后必须能够缩回原尺寸。

## 9. 兼容性与定制边界

- `TextBox`、`LineEdit`、`TextArea` 及其派生控件继续保留各自的文本编辑、单行、多行和业务布局语义；共有输入状态只能通过 `AbstractTextInput` 与 `InputControlFrame` 的分层契约表达。
- `InputControlFrame`、`AddOnDecoratedBox` 及其专用派生类型属于内部组合实现。它们可以作为稳定 template part 存在，但不作为用户直接依赖的公共 Semantic Part，也不向外暴露内部 `TextPresenter`、`ScrollViewer` 或 NameScope。
- 主题定制通过 `InputControlFrameTheme`、控件主题和 Token resource 完成。专用 decorated box 可以增加布局和业务内容 selector，但不能替换 frame 的 variant、effective status、error、warning、disabled 或 motion owner。
- 输入表面的 root 定制通道是 owner 上的标准 `Background` / `BorderBrush`：`AbstractTextInput` 把 owner 有效值以 LocalValue 中继到 frame 同名属性（共享 `TemplatedControl` 属性实例），定制期间该属性槽的交互态变色冻结，置空后 `ClearValue` 恢复状态机；focus 的 `BoxShadow` 反馈作用在独立属性槽。四个输入主题不在 owner 层携带这两个属性的默认值，`TextBox` 模板不以 `TemplateBinding` 绑定它们——owner 值同时是“是否定制”的判定输入，不引入平行定制属性。
- `StyleVariant` 的四种值、`DataValidationErrors` 的 native error 语义、Form-owned error 的清理边界和 `EffectiveStatus` 优先级属于所有输入表面的共同契约；内部模板重组不得引入第二套状态源。
- `InputControlFrame` 的公共协作只依赖稳定属性、绑定和 template part。消费方不得通过运行时反射、程序集扫描或 VisualTree 遍历推断内部输入结构。

## 10. 维护与验证

维护输入控件时必须同时验证：

| 层次 | 验证内容 |
| --- | --- |
| 逻辑层 | 文本值、选择、清除、reveal、count、Form value、native validation、FormStatus 和 viewport metrics。 |
| frame 层 | 四种 `StyleVariant`、EffectiveStatus 优先级、hover/focus/pressed/disabled、CompactSpace、motion 和 Light/Dark 主题。 |
| 组合层 | AddOn、搜索按钮、handle、tag、popup、resize 和内部内容布局不重复绘制 frame。 |
| 生命周期 | template reapply 负责旧 part、模板事件和模板 binding 的替换；logical attach/detach 负责外部 owner、Form feedback、popup 和窗口级订阅的获取与释放；Form reset 只清理 Form-owned 状态。 |
| AOT/文档 | generator registration、主题 asset、Semantic Part 边界、overview/implementation/token/changelog 术语一致。 |

具体控件文档只描述本控件的业务状态和专用模板；当它引用输入表面、状态优先级、Form error 或 Token 共享规则时，以本文档为唯一共享定义。

关联文档：

- [Data Entry 控件入口](overview.md)
- [LineEdit 桌面版架构设计](line-edit/overview.md) / [实现原理](line-edit/implementation.md) / [Token 设计](line-edit/token.md)
- [SearchEdit 桌面版架构设计](search-edit/overview.md) / [实现原理](search-edit/implementation.md)
- [AutoComplete 桌面版架构设计](auto-complete/overview.md) / [实现原理](auto-complete/implementation.md)
- [Mentions 桌面版架构设计](mentions/overview.md) / [实现原理](mentions/implementation.md) / [Token 设计](mentions/token.md)
- [NumericUpDown 桌面版架构设计](numeric-up-down/overview.md) / [实现原理](numeric-up-down/implementation.md) / [Token 设计](numeric-up-down/token.md)
- [Select 桌面版架构设计](select/overview.md) / [实现原理](select/implementation.md) / [Token 设计](select/token.md)
- [TreeSelect 桌面版架构设计](tree-select/overview.md) / [实现原理](tree-select/implementation.md) / [Token 设计](tree-select/token.md)
- [Cascader 桌面版架构设计](cascader/overview.md) / [实现原理](cascader/implementation.md) / [Token 设计](cascader/token.md)
- [DatePicker 桌面版架构设计](date-picker/overview.md) / [实现原理](date-picker/implementation.md) / [Token 设计](date-picker/token.md)
- [TimePicker 桌面版架构设计](time-picker/overview.md) / [实现原理](time-picker/implementation.md) / [Token 设计](time-picker/token.md)
- [Form 桌面版架构设计](form/overview.md) / [实现原理](form/implementation.md)
- [OtpLineEdit 桌面版架构设计](otp-line-edit/overview.md) / [实现原理](otp-line-edit/implementation.md) / [Token 设计](otp-line-edit/token.md)

### 根前景与字体

TextBox/LineEdit/TextArea/SearchEdit 的 Foreground 默认使用具体公开 owner 的 Control Global ColorText，投射到框体后由文本继承。不能让框体的 Shared ColorText 覆盖原生根 Foreground。实际文本仍支持原生 FontFamily/FontSize/FontWeight/FontStyle；input 专用 Style 的附加 TextElement 字体/前景 Setter 保留原生优先级。其他组合 owner 的字体、独立子控件和控制级字体字段分别归其控件家族合同，不用这四项 API 证据代替全部组合路径通过。

### 内部资产的主题查询路线

InputControlFrame、InputClearIconButton及RevealButton没有独立公开主题身份。主题内使用OwnerTokenResource：优先沿TemplatedParent，内容子树没有该关系时沿逻辑Parent，直到真实注册的公开Control；公开子Button继续保有自己的identity。不能因为内部节点位于ContentRightAddOn而直接读Shared，也不把LineEdit身份配置复制给SearchEdit。

AddOnDecoratedBox的附加区背景/描边、三档内边距/行高、槽间距、后缀默认前景与状态文字/图标色遵循同一路线。派生ButtonSpinnerContentPanel的间距同样来自实际owner。SearchEdit的ContentLayout是DockPanel，没有间距属性；TextArea没有外侧addon。NumericUpDownSpinner是内部未独立注册节点，其附加区归NumericUpDown；实际公开ButtonSpinner仍使用自己的identity。

运行时包装PathIcon的SizeTypeAwareIconPresenter在自身ControlTheme上消费实际owner的三档IconSize，并通过TemplateBinding投射Width/Height。Custom退出尺寸预设，只保留基础IconSize，原生显式宽高及child显式值仍由Avalonia处理优先级。模板中的公开IconPresenter继续使用自身画刷身份，不能从其self查询外层输入的尺寸。

资源绑定在上下文、当前父/模板父和逻辑挂载变化时重新解析，并按上下文/资源键去重；资源值仍由Avalonia原生observable与绑定优先级处理，不代码设置最终样式。只读解析依赖注册元数据与实际树拓扑，不能通过单个AXAML静态AncestorType覆盖全部复用owner；编译绑定避免反射，分离/重挂载由原生绑定生命周期释放。未为每条资源订阅所有祖先，也不建立全局强引用控件缓存。

框体仍独立归一EffectiveStatus与四种variant，显式根Background/BorderBrush仍走原LocalValue中继；清除后恢复状态。清除按钮的IconSize、ColorTextQuaternary/Tertiary与按下ColorText，密码显示按钮的IconSize、ColorIcon/Hover及disabled色均来自同一真实owner。TextArea模板只有clear和反馈区，没有RevealButton；不能因继承了相关API便声称模板提供密码按钮。
