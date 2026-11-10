# NumericUpDown Token 设计

本文档定义 `AtomUI.Desktop.Controls.NumericUpDownToken` 的 NumericUpDown 专属语义、分类、使用范围和兼容边界。共享输入表面分层见 [输入控件共享架构设计](../input-control-architecture-design.md)，控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。NumericUpDown 整体架构见 [NumericUpDown 桌面版架构设计](overview.md)，内部实现原理见 [NumericUpDown 桌面版实现原理](implementation.md)，设计和契约变化记录见 [NumericUpDown Changelog](changelog.md)。

## 1. Scope 与边界

`NumericUpDownToken` 与 `ButtonSpinnerToken` 继承 `AbstractButtonSpinnerToken`。它们各自按有效 Global Token 计算
Own 值；`Controls[ButtonSpinner]` 不改变 NumericUpDown 的步进柄，NumericUpDown 的配置也不改变独立 ButtonSpinner。

## 2. Own Token

- `HandleWidth`、`HandleIconSize`：浮动步进柄的宽度与上下箭头尺寸。
- `HandleBg`、`FilledHandleBg`、`HandleActiveBg`、`HandleHoverColor`、`HandleBorderColor`：正常、Filled、按下、悬停与边框视觉。
- `FilledHandleBg` 根据该终端的有效 `HandleBg` 派生；显式 `HandleBg` 先保留，再计算派生值。

NumericUpDown 没有 `InputFontSize*` Own Token，也没有承诺默认控件宽度。已有 `ButtonSpinner.ControlWidth` 仍为 Reserved，
不向新终端复制这个无消费者字段。

## 3. 输入与步进柄

输入框、frame、禁用/错误/警告、focus shadow 与动效读取 NumericUpDown 的有效 Global Token。共享柄的具体样式输入由终端
AXAML 设置；嵌套的 IconButton 仍保留独立身份，宿主明确控制柄图标几何与状态色。

内嵌 `EmbeddedTextBox` 保持 `SizeType=Custom`、无额外 chrome，字号由 NumericUpDown 提供：Small 与 Middle 使用
`FontSize`（默认14），Large 使用 `FontSizeLG`（默认16）。`IsCustomFontSize` 与调用方显式字号仍可接管。内部编辑器不再把
宿主默认的 `IsCustomFontSize=false` 传回自己的 Custom 分支并覆盖宿主的大号字号。

`Mode=Spinner` 的加减按钮使用普通 `IconSize`，不使用浮动柄的 `HandleIconSize` 或 `HandleWidth`；分隔线、悬停与按下分别使用
该 NumericUpDown 的 `HandleBorderColor`、`HandleHoverColor`、`HandleActiveBg`。现有尺寸、内容避让与滚动行为不变。

## 4. 兼容与验证

原先以 `Controls[ButtonSpinner]` 定制 NumericUpDown 步进柄的配置应移到 `Controls[NumericUpDown]`，字段名称不变。
验证覆盖真实悬停显示的浮动柄、inline Spinner、三档字号、显式派生基值和两身份隔离；不把读取成功当成视觉效果。
