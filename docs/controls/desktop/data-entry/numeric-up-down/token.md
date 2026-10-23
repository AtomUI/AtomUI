# NumericUpDown Token 设计

本文档定义 `AtomUI.Desktop.Controls.NumericUpDownToken` 的 NumericUpDown 专属语义、分类、使用范围和兼容边界。共享输入表面分层见 [输入控件共享架构设计](../input-control-architecture-design.md)，控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。NumericUpDown 整体架构见 [NumericUpDown 桌面版架构设计](overview.md)，内部实现原理见 [NumericUpDown 桌面版实现原理](implementation.md)，设计和契约变化记录见 [NumericUpDown Changelog](changelog.md)。

## 1. 定位

NumericUpDownToken 是 NumericUpDown 的控件级 Token scope。它继承 `ButtonSpinnerToken`，以独立 `NumericUpDown` scope 提供步进 Handle、字体和尺寸语义；输入表面边框、背景、圆角、focus shadow、error/warning、disabled 和 motion 统一由 `InputControlFrameTheme` 与 `SharedToken` 提供。

该设计使 NumericUpDown 能复用 ButtonSpinner 输入壳体体系，同时保留控件级 Token scope。生成的 `NumericUpDownTokenKind` 表达 NumericUpDown scope 下可展示和可覆盖的 Token；默认主题中的输入壳体和 Handle 仍通过 `ButtonSpinnerTokenResource` 消费共享 ButtonSpinner 语义值。

NumericUpDownToken 不承载以下状态：

- `Value`、`Text`、`StringValue`、`Minimum`、`Maximum`、`Increment` 等实例数值状态。
- `IsStringMode`、`IsKeyboardEnabled`、`IsAllowClear` 等行为状态。
- `IsPointerOver`、`IsPressed`、`IsFocused`、`IsEnabled` 等交互状态。
- `EffectiveContentPadding`、`HandleOpacity`、`HandleOffset` 等模板运行时状态。

这些状态分别由 C# 状态模型、共享输入主题和 ButtonSpinnerDecoratedBox 内部属性处理。

## 2. Token 分类

NumericUpDownToken 当前继承 ButtonSpinnerToken 的 Token 集合，按 NumericUpDown 语义分为四类。

### 2.1 控件尺寸 Token

- `ControlWidth`
- `InputFontSize`
- `InputFontSizeLG`
- `InputFontSizeSM`

用于 NumericUpDown 默认宽度和不同 `CustomizableSizeType` 下的输入文本字号。`Custom` 不新增专属字号 Token，未显式覆盖时复用 `Middle` 分支；用户显式接管字号时由 `IsCustomFontSize` 控制内部 `TextBox` 是否接受 `SizeType` 字号样式。字号语义来自输入控件家族，不直接依赖 NumericUpDown 的数值精度或格式化状态。

### 2.2 浮动 Stepper Handle 尺寸 Token

- `HandleWidth`
- `HandleIconSize`

用于 `Mode=Input` 浮动步进 Handle 的宽度和上下箭头图标尺寸。`HandleWidth` 同时影响浮动 Handle 进入/隐藏时的偏移和内容避让距离，因此属于结构性 Token，不应作为普通装饰值随意调整。`HandleIconSize` 服务右侧浮动 Handle 的小号上下箭头，不用于 `Mode=Spinner` 的 inline 加减号 action。

### 2.3 Stepper Handle 颜色 Token

- `HandleBg`
- `FilledHandleBg`
- `HandleActiveBg`
- `HandleHoverColor`
- `HandleBorderColor`

用于步进 Handle 的普通背景、Filled 变体背景、按下背景、hover 图标色和边框色。Filled 变体必须使用 `FilledHandleBg`，避免 Handle 与外层 Filled 输入表面割裂。Disabled 状态背景和文本色使用 SharedToken 的 disabled 语义，不在 NumericUpDownToken 中定义独立 disabled Token。

### 2.4 输入壳体继承 Token

NumericUpDownToken 继承自 `ButtonSpinnerToken`，而 `ButtonSpinnerToken` 消费 LineEdit 输入家族的文本尺寸语义。输入表面边框、背景、圆角、focus shadow、error/warning、disabled 和 motion 不进入 NumericUpDown 或 ButtonSpinner Token 继承链。

## 3. 控件专项模型中的 Token 使用

NumericUpDown 的专项模型主要由 ButtonSpinner Handle 和输入壳体承担。

```text
SharedToken / LineEditToken
        ↓
ButtonSpinnerToken
        ↓
NumericUpDownToken scope
        ↓
NumericUpDownTheme / NumericUpDownSpinnerTheme / ButtonSpinner themes
        ↓
TextBox + ButtonSpinnerHandle visual state
```

NumericUpDownTheme 直接使用的 Token：

- `HandleWidth`：通过 `ButtonSpinnerTokenResource` 设置 `SpinnerHandleWidth` 默认值。
- SharedToken `EnableMotion`：设置 `IsMotionEnabled` 默认值。
- SharedToken `ColorTextPlaceholder`：设置占位符颜色。

ButtonSpinner 相关主题使用的 Token：

- `HandleWidth`：控制浮动 Handle 宽度。
- `HandleIconSize`：控制浮动 Handle 内部上下箭头尺寸。
- `HandleBg`：控制普通 Handle 背景。
- `FilledHandleBg`：控制 Filled Handle 背景。
- `HandleActiveBg`：控制按下状态背景。
- `HandleHoverColor`：控制 hover 图标色。
- `HandleBorderColor`：控制 Handle 边框色。

NumericUpDown 不为 `Error`、`Warning`、`Disabled`、`Focused` 或 `PointerOver` 状态定义独立组合 Token。这些状态通过 `InputControlFrameTheme`、SharedToken、LineEdit 文本尺寸 Token 和 ButtonSpinner 布局主题映射完成。

Spinner mode 不新增专属 Token。`Mode=Spinner` 的三段式拨轮结构复用输入控件家族的 padding、标准图标尺寸和已有 Handle 状态色：

- `SharedToken.InputPadding` / `InputPaddingLG` / `InputPaddingSM`：控制左右 action 段和中间输入段的横向 padding，使 action 宽度随输入尺寸自适应。
- `SharedToken.IconSize`：控制 `MinusOutlined` / `PlusOutlined` 图标尺寸，保持加减号为普通 action 图标。
- `HandleBorderColor`：控制左右 action 段与中间输入段之间的分隔线颜色。
- `HandleHoverColor`：控制左右 action 段 hover 图标色。
- `HandleActiveBg`：控制左右 action 段 pressed 背景。

`Mode=Spinner` 不使用 `HandleWidth` 计算左右 action 段宽度，也不使用 `HandleIconSize` 计算 `MinusOutlined` / `PlusOutlined` 图标尺寸。普通背景跟随 frame 输入表面；Outlined、Filled、Borderless、Underlined、Disabled、Focused 等输入表面状态由 `InputControlFrameTheme` 统一负责。

`Mode=Spinner` 的外层 content frame 不消费输入 padding Token。输入 padding 只由左右 action 段和中间 TextBox 消费，避免外层 frame padding 与内部 padding 叠加，破坏标准输入控件高度。

只有当现有输入壳体 Token 和 Handle 状态色无法表达稳定、可复用的 spinner mode 语义时，才允许讨论新增 NumericUpDown 专属 Token。新增前必须说明现有 Token 不足、影响范围和兼容边界。

## 4. 控件家族影响

NumericUpDownToken 与 ButtonSpinnerToken 共享同一套输入 Handle 语义。Token 变更必须评估以下控件或主题：

- `NumericUpDownTheme.axaml`
- `NumericUpDownSpinnerTheme.axaml`
- `ButtonSpinnerTheme.axaml`
- `ButtonSpinnerDecoratedBoxTheme.axaml`
- `ButtonSpinnerHandleTheme.axaml`
- `ComboBoxTheme.axaml` 及其 ButtonSpinner 派生用法
- NumericUpDown、ButtonSpinner、ComboBox 的 token.md 语义说明和 Gallery 示例

如果某个 Token 只服务 NumericUpDown 的数值编辑模型，例如格式化、精度或 string mode，不应直接加入 ButtonSpinnerToken 继承链。数值编辑状态属于控件实例状态，不属于控件 Token。

## 5. 兼容性要求

NumericUpDownToken 属于 NumericUpDown 主题契约。即使 `NumericUpDownToken` 是 internal 类型，生成的 `NumericUpDownTokenKind`、AXAML resource 使用点和 token.md 语义说明已经形成稳定依赖。

Token 变更要求：

- 不擅自重命名既有 Token。
- 不擅自删除既有 Token。
- 不改变 `HandleWidth`、`HandleIconSize`、`HandleBg`、`FilledHandleBg`、`HandleActiveBg`、`HandleHoverColor`、`HandleBorderColor` 的语义。
- 不把实例数值状态、解析状态、清除按钮状态或交互状态迁移为 Token。
- 不在 NumericUpDownToken 中复制 preset 色表或硬编码业务颜色。
- 需要破坏性变更时，必须先说明影响范围并获得授权。

## 6. 验证策略

| 改动类型 | 验证要求 |
| --- | --- |
| 新增 NumericUpDownToken | 检查生成的 `NumericUpDownTokenKind`、AXAML 引用、token.md 语义说明和默认值计算。 |
| 修改 Handle Token 默认值 | 覆盖 NumericUpDown、ButtonSpinner、ComboBox 的普通、Filled、Disabled、hover 和 pressed 状态；`Mode=Spinner` 需额外验证分隔线、hover 图标色和 pressed 背景。 |
| 修改尺寸 Token | 验证 `Large`、`Middle`、`Small`、`Custom` 尺寸下的文本、Handle、内容避让和 CompactSpace 边框折叠。 |
| 删除或重命名 Token | 默认不允许；如获授权，需同步所有 AXAML 引用、生成文件、Token 类型、生成数据和 token.md和控件文档。 |
| 调整数值输入状态 | 不应修改 Token；应通过 C# 状态模型或主题变量验证。 |

## 根几何与 Effective Global 消费

公开 owner 的 `Padding`、`BorderThickness`、`CornerRadius` 默认值从该 Control 的 Effective Global 读取，再以原生主题优先级投射到实际框体。共享框体继续负责状态色、focus/hover shadow、variant 与 CompactSpace；不能把它描述为跳过 owner 的全局几何入口。字号和其他消费链按各控件当前模板分别定义。本次未增加 Own 字段或改变 Token identity。详见 [共享输入架构的根几何合同](../input-control-architecture-design.md#81-根表面几何与预设默认值)。
