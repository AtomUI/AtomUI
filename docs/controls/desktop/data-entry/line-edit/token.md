# LineEdit Token 设计

本文档定义 `TextBoxToken`、`LineEditToken` 与 `TextAreaToken` 在 LineEdit 输入家族中的专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。LineEdit 整体架构见 [LineEdit 桌面版架构设计](overview.md)，内部实现原理见 [LineEdit 桌面版实现原理](implementation.md)，设计和契约变化记录见 [LineEdit Changelog](changelog.md)。

## 1. 定位

LineEdit 输入家族使用三个控件级 Token scope：

| Token | Scope | 职责 |
| --- | --- | --- |
| `TextBoxToken` | `TextBox` | TextBox 专属内容 padding。 |
| `LineEditToken` | `LineEdit` | 单行输入框字号。 |
| `TextAreaToken` | `TextArea` | 多行输入框字号、右侧附加 padding 和 resize handle 视觉。 |

TextBox / LineEdit / TextArea Token 不承载文本值、placeholder、清除状态、密码 reveal、Form 状态、SearchEdit 运行状态、focus/hover/pressed 状态或 CompactSpace 运行时状态。输入表面边框、背景、圆角、shadow、error/warning、disabled 和 motion 由 `InputControlFrameTheme` 与 `SharedToken` 处理；运行时状态由 `AbstractTextInput`、Form 和 frame 状态模型处理。

## 2. Token 分类

### 2.1 TextBox 专属文本布局 Token

- `TextBox.ContentPaddingLG`
- `TextBox.ContentPadding`
- `TextBox.ContentPaddingSM`

这些 Token 只服务 `TextBoxTheme.axaml` 中 TextBox 自身内容 padding。TextPresenter 的基础 margin 属于模板布局常量；输入表面边框、背景、圆角、hover/focus shadow、error/warning 和 disabled 视觉不属于 `TextBoxToken`。

### 2.2 LineEdit 字号 Token

- `LineEdit.InputFontSize`
- `LineEdit.InputFontSizeLG`
- `LineEdit.InputFontSizeSM`

这些 Token 保持 LineEdit 单行输入框字号契约，默认值对齐 antd Input：`InputFontSize` 取 `FontSize`，`InputFontSizeLG`
取 `FontSizeLG`，`InputFontSizeSM` 取 `InputFontSize`（小号输入框不缩小字号；显式覆盖 `InputFontSize` 时小号随之变化，
除非同时显式设置 `InputFontSizeSM`）。`LineEditTheme` 在 `IsCustomFontSize=False` 时按 `SizeType` 把它们写入 LineEdit 的 `FontSize`
（`Custom` 取中档），`IsCustomFontSize=True` 保留继承或本地字号；复用 LineEdit 主题的组合控件（AutoComplete 的输入框、
Pagination 的快速跳转框）随之使用。SearchEdit 是独立 identity，读自己的 `FontSize` / `FontSizeLG` 有效值
（小号同样取 `FontSize`），不借用 LineEdit 的 Own Token。

### 2.3 TextArea 字号

TextArea 没有字号 Own Token：`TextAreaTheme.axaml` 按 `SizeType` 读取 TextArea 的有效 Global Token
`FontSize`、`FontSizeLG`、`FontSizeSM`，行高读取 `FontHeight`、`FontHeightLG`、`FontHeightSM`（`TextAreaTokenResource`），
`Controls[TextArea]` 的覆盖生效。

### 2.4 TextArea 右侧附加 padding Token

- `TextArea.RightAddOnPadding`
- `TextArea.RightAddOnPaddingLG`
- `TextArea.RightAddOnPaddingSM`

这些 Token 用于 TextArea 内部右侧内容与边框之间的空隙。它们根据 SharedToken 的水平 padding 和线宽计算，避免右侧 clear / feedback / resize 相关内容与文本区域重叠。

### 2.5 TextArea resize Token

- `TextArea.ResizeIndicatorLineColor`
- `TextArea.ResizeHandleSize`

这些 Token 控制 TextArea 右下角 resize handle 的线条颜色和尺寸。它们只服务 `IsResizable=true` 的拖拽入口，不影响 TextArea 的自动高度算法或固定行数测量。
`ResizeHandle` 叠放在输入表面之上，自己绘制两条斜线指示：命中区与指示边长都是 `ResizeHandleSize`，线条按边长比例排布，
线宽读取 TextArea 的有效 `LineWidth`。

## 3. 控件专项模型中的 Token 使用

LineEdit 家族 Token 使用路径：

```text
所属公开控件（TextBox / LineEdit / SearchEdit / TextArea …）的 Effective Global Token
   ↓ OwnerTokenResource（经继承的 ThemeScope.TokenOwner）
InputControlFrameTheme / AddOnDecoratedBoxTheme / 各 decorated box 主题
   ↓
TextBoxTheme / LineEditTheme / SearchEditTheme / TextAreaTheme

TextBoxToken / LineEditToken / TextAreaToken
   ↓
各自文本尺寸、padding 和 resize 专属布局
```

输入表面的边框、圆角、背景、hover/focus shadow、error/warning 和 disabled 状态由 `InputControlFrameTheme` 以
`OwnerTokenResource` 读取所属公开控件的有效 Global Token，`Controls[LineEdit]` 等覆盖只改变该控件的输入表面；
各控件主题不再为 hover / focus 重复设置表面颜色。`TextBoxTheme` 同样以 `OwnerTokenResource` 读取全局值：它也是
NumericUpDown、选择器与筛选框内嵌编辑器（EmbeddedTextBox 及其派生类）的基础主题，这些编辑器没有 identity，
跟随宿主控件。TextBox 的内容 padding 由 `TextBoxTokenResource` 进入 `TextBoxTheme.axaml`；LineEdit 的单行字号由
`LineEditTokenResource` 消费；TextArea 的字号、右侧 padding 和 resize 视觉由 `TextAreaTokenResource` 及相关布局主题消费。

宿主根上设置的 `BorderBrush` / `Background` 经模板绑定进入输入表面的内部属性 `HostBorderBrush` / `HostBackground`，
由 `InputControlFrameTheme` 在全部状态色之后投射（antd `styles.root` 语义），未设置时交还状态机；控件代码不再以本地值
写入内部表面。

TextArea 的多行字号由 `TextAreaTokenResource` 进入 `TextAreaTheme.axaml`。TextArea 右侧 padding 和 resize 视觉由
`TextAreaDecoratedBoxTheme.axaml`、`ResizeHandleTheme.axaml` 消费；TextArea 的内容呈现器位于注册控件 ScrollViewer 之内，
它的行高与小号内容边距显式读取 `TextAreaTokenResource`。

SearchEdit 不定义 Own Token。它复用 `AbstractTextInput` 的状态，字号读取自己的有效 Global Token，输入表面由
`InputControlFrame` 统一表达，搜索按钮使用 Button 体系和 SearchButton 主题表达：按钮视觉读取 Button 有效值，
拼接圆角与 Text 型按钮的搜索语义色读取 SearchEdit 有效值。

## 4. 控件家族影响

TextBoxToken 影响以下控件或主题：

- `TextBoxTheme.axaml`
- 使用 AtomUI `TextBox` 作为内部输入框的组合控件

LineEditToken 影响以下控件或主题：

- `LineEditTheme.axaml`
- `SearchEditTheme.axaml`
- LineEdit / SearchEdit token.md 语义说明和 Gallery 示例

TextAreaToken 影响以下控件或主题：

- `TextAreaTheme.axaml`
- `TextAreaDecoratedBoxTheme.axaml`
- `ResizeHandleTheme.axaml`
- TextArea token.md 语义说明和 Gallery 示例

如果调整 TextBoxToken 的内容 padding，必须评估基础 TextBox 以及依赖 AtomUI TextBox 的组合控件。输入表面边框、圆角、hover/focus shadow 和状态视觉的变更应评估 `InputControlFrameTheme` 及全部 decorated box 派生控件。DatePicker / TimePicker 等内部 picker 输入框在自定义尺寸路径中可能显式绑定字体并设置 `IsCustomFontSize=true`，不应依赖隐藏宽度补偿适配字号变化。

如果调整 LineEditToken 字号，必须评估 LineEdit、SearchEdit 以及复用 LineEdit 输入壳体的组合控件。

## 5. 兼容性要求

TextBoxToken、LineEditToken 与 TextAreaToken 属于输入控件家族的主题契约。即使 Token 类型是 internal，生成的 TokenKind、AXAML resource 使用点和 token.md 语义说明已经形成稳定依赖。

Token 变更要求：

- 不把实例状态、交互状态、Form 状态、搜索运行状态或文本内容状态迁移为 Token。
- 不在 TextBoxToken、LineEditToken 或 TextAreaToken 中复制 InputControlFrame 的边框、背景、圆角、focus shadow、error/warning、disabled 或 motion 状态 Token。
- 不在 TextAreaToken 中表达自动高度、当前行数或 resize 当前拖拽高度。
- `SizeType=Custom` 不新增专属字号 Token；它默认复用 Middle 分支，由用户显式属性完成自定义。
- 任何 Token scope 调整都必须同步其主题资源、generator 注册、控件文档和相关组合控件文档。

## 6. 承诺面裁决

2026-11-08 输入家族迁移按[承诺面](../../../../engineering/development/control-token-guidelines.md#own-token-承诺面)裁决了
没有消费者的 Own Token，并评估了新增 Own Token：

| Token | 裁决 |
| --- | --- |
| `LineEdit.InputFontSize`、`InputFontSizeLG`、`InputFontSizeSM` | Active：LineEditTheme 按 SizeType 设置 LineEdit 字号（此前 LineEdit 没有字号预设，大号与小号沿用继承字号）；`InputFontSizeSM` 默认改为 `InputFontSize`，对齐 antd |
| `TextArea.ResizeHandleSize` | Active：resize handle 的命中区与指示尺寸 |
| `ButtonSpinner.ControlWidth` | Reserved，见 ButtonSpinner token.md |
| 输入家族 `HoverBorderColor`（PrimeNG 的悬停边框色需求） | 不新增：悬停边框由无身份的共享输入表面绘制，基元只能读取全局 Token；要让每个输入控件一致消费，需要在 LineEdit、SearchEdit、TextArea、TextBox、ButtonSpinner、NumericUpDown 以及 Select、Cascader、TreeSelect、DatePicker 等全部输入表面宿主上各加同名 Own Token，并在各自主题里按状态重新投射到表面，这正是本次迁移移除的重复状态样式。迁移后 `Controls[X].Tokens[ColorPrimaryHover]` 只改变 X 的悬停边框，已能表达该需求（例如设为 slate-400） |

全部 Active Own Token 与消费的 Global Token（含共享输入基元经 `OwnerTokenResource` 读取的全局值）由
`tests/AtomUI.Desktop.Controls.Rendering.Tests/Effect/Families/InputFamilyEffectTests` 逐项验证生效。

## 7. 验证策略

| 改动类型 | 验证要求 |
| --- | --- |
| 修改 TextBox padding Token | 验证 TextBox 的 Large/Middle/Small/Custom padding、clear/reveal 对齐，并评估依赖 AtomUI TextBox 的组合控件。 |
| 修改 LineEdit 字号 Token | 验证 LineEdit、SearchEdit 的 Large/Middle/Small/Custom 字号、line height、placeholder 和 clear/reveal 对齐。 |
| 修改 TextArea 字号 Token | 验证 Lines、MinLines、MaxLines、IsAutoSize 和 IsResizable 下的高度计算。 |
| 修改 TextArea padding Token | 验证 clear button、Form feedback、InnerRightContent、字数统计和文本区域不重叠。 |
| 修改 resize Token | 验证 resize handle 可见性、拖拽命中、线条颜色和尺寸。 |
| 删除或重命名 Token | 默认不允许；如获授权，需同步 AXAML 引用、生成文件、Token 类型、生成数据和 token.md和控件文档。 |
| 文档改动 | 运行 `git diff --check`，检查文档链接存在。 |
