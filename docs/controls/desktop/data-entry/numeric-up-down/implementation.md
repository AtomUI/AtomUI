# NumericUpDown 桌面版实现原理

本文档描述 NumericUpDown 桌面版的内部数值同步、string mode、模板切换、ButtonSpinner 接入和 Form / CompactSpace 集成。共享输入分层见 [输入控件共享架构设计](../input-control-architecture-design.md)，公共设计与 API 契约见 [NumericUpDown 桌面版架构设计](overview.md)，Token 语义见 [NumericUpDown Token 设计](token.md)，变化记录见 [NumericUpDown Changelog](changelog.md)，Semantic Part 契约见 [NumericUpDown Semantic Part 契约](semantic-part.md)。

## 1. 实现定位

NumericUpDown 的实现基于 Avalonia `NumericUpDown`，AtomUI 通过 `InputControlFrame` / `ButtonSpinner` 负责输入表面、Addon、浮动 Handle、清除按钮、string mode、Form、CompactSpace 和 spinner 展示模式。实现文档聚焦状态同步和模板接入，不重新说明 Avalonia 基类的数值算法。

## 2. 源码文件结构

主要源码：

- `src/AtomUI.Desktop.Controls/NumericUpDown/NumericUpDown.cs`：public API、数值同步、custom size、string mode、清除按钮、键盘处理、Form / CompactSpace / Motion 接口。
- `src/AtomUI.Desktop.Controls/NumericUpDown/NumericUpDown.SemanticParts.cs`：Semantic Part 声明（`root`、`prefix`、`input`、`suffix`、`clear`）。
- `src/AtomUI.Desktop.Controls/NumericUpDown/NumericUpDownInputPanel.cs`：同一次排列中分配编辑器、后缀与浮动操作区的空间。
- `src/AtomUI.Desktop.Controls/NumericUpDown/NumericUpDownSpinner.cs`：internal Spinner 子控件，复用 ButtonSpinner 的数值步进语义并承载 NumericUpDown 专用 inline 模板。
- `src/AtomUI.Desktop.Controls/NumericUpDown/NumericUpDownToken.cs`：NumericUpDown Token scope。
- `src/AtomUI.Desktop.Controls/NumericUpDown/Themes/NumericUpDownTheme.axaml`：输入模式和 spinner 模式外层模板、状态传递。
- `src/AtomUI.Desktop.Controls/NumericUpDown/Themes/NumericUpDownSpinnerTheme.axaml`：NumericUpDownSpinner 的 inline 模板、分隔线、加减按钮和按钮状态视觉。
- `src/AtomUI.Desktop.Controls/ButtonSpinner/ButtonSpinner.cs`：输入壳体和 spin 入口。
- `src/AtomUI.Desktop.Controls/ButtonSpinner/ButtonSpinnerDecoratedBox.cs`：在 shared frame 上扩展浮动 Handle、Addon、CompactSpace 和 spinner 布局。
- `src/AtomUI.Desktop.Controls/ButtonSpinner/ButtonSpinnerHandle.cs`：Handle 按钮和上下箭头。
- `src/AtomUI.Desktop.Controls/Input/TextBox.cs`：文本编辑器。
- `src/AtomUI.Desktop.Controls/Input/InputClearIconButton.cs`：清除按钮。

## 3. 核心类职责

`NumericUpDown` 负责把 Avalonia 数值模型与 AtomUI 输入体系连接起来。它不复制 ButtonSpinner、TextBox 或 AddOnDecoratedBox 的视觉状态 selector。

`ButtonSpinner` 是 NumericUpDown 的输入组合边界，复用 `InputControlFrame` 的外观和 `EffectiveStatus`，负责外部 AddOn、内部内容、浮动 Handle 和 spin 入口；它不重新声明 variant/status/error/warning/disabled。

`NumericUpDownSpinner` 是 NumericUpDown 专用的 internal 子控件。它以 `ButtonSpinner` 为基础主题，在 `IsButtonSpinnerFloatable=False` 时提供 inline 三段式模板，并在自己的主题中维护 `PART_DecreaseButton`、`PART_IncreaseButton` 的视觉。NumericUpDown 父主题只传递属性和内容，不进入该子控件的模板内部。

`TextBox#PART_TextBox` 负责文本编辑、placeholder、只读、数据校验和 disabled 文本色。NumericUpDown 不应让内层 TextBox 绘制独立边框。

## 4. 状态与数据流

数值同步流：

```text
TextBox text input
      ↓
NumericUpDown Text / TextConverter / ParsingNumberStyle
      ↓
Value
      ↓
FormatString / NumberFormat / TextConverter
      ↓
display Text
```

String mode 同步流：

```text
IsStringMode=true
  Text changed        → StringValue = raw text
  StringValue changed → Text = raw text, Value = parsed decimal? or null
  Value changed       → StringValue = raw decimal text
```

输入壳体状态：

- `SizeType` 类型为 `CustomizableSizeType`，传递给 `ButtonSpinner`、`TextBox` 和内部 Handle；`Custom` 由主题按 `Middle` 默认分支处理。
- `IsCustomFontSize` 通过模板接入后的 relay binding 传递给内部 `TextBox`，只参与字号覆盖判断，不参与数值状态计算。
- `StyleVariant`、`Status`、`FormStatus` 和 native validation 由 NumericUpDown 的输入适配层归一后传递给 `InputControlFrame`，再由 `ButtonSpinner` 和 Handle 消费；NumericUpDown 不另建一套与 shared frame 并行的输入表面状态源。
- `ShowButtonSpinner` 传递给 `ButtonSpinner.IsButtonSpinnerVisible`；默认输入模式由它控制浮动 Handle，spinner 模式由它控制左右 action 段。
- `Mode` 只驱动模板选择和模式专用 part 接入，不参与数值状态计算。
- CompactSpace 状态只作为输入壳体集成状态，不形成公开 API。

## 5. 生命周期与模板接入

`OnApplyTemplate` 获取跨模式稳定 part：`PART_Spinner`、`PART_TextBox`、`PART_ClearButton`、`PART_InnerRightContentPresenter`。`Mode=Spinner` 模板中的 `PART_DecreaseButton` 和 `PART_IncreaseButton` 属于 `NumericUpDownSpinner` 的模板，由继承的 `ButtonSpinner` spin 语义接入；NumericUpDown 本体不直接持有或订阅这两个按钮。

模板替换时必须解除旧 `TextBox.KeyDown`、清除按钮 `Click` 和内容区 relay binding。运行时切换 `Mode` 会触发模板重建，控件本体上的 `Value`、`Text`、`StringValue`、`Status`、`SizeType` 和 `IsCustomFontSize` 等状态通过绑定保留；旧 TextBox 的焦点、光标位置和选区属于旧模板实例，不作为跨模板强契约。

`SetupTemplatePartBindings` 负责无法直接模板绑定的内部状态：清除按钮有效可见性、属性元素子树中的左侧内容，以及输入布局面板的当前右侧避让量。图标、动效、右侧内容和文本框字号状态直接在 ControlTheme 中绑定。代码绑定由当前模板的 `CompositeDisposable` 持有，并在下一次模板接入前释放。清除按钮和内部 `TextBox` 的事件订阅由成对 part setter 负责。

`Mode=Input` 默认模板不能创建 spinner 模式左右按钮。`Mode=Spinner` 的按钮、分隔线和专用布局节点只在启用 spinner 模式时实例化。两个模板分支都必须消费继承的 `ShowButtonSpinner`，不能用写死的模板值覆盖用户设置。

两个模板变体还承载 Semantic Part marker：`PART_Spinner` 携带 `.semantic-scope-spinner`，`PART_TextBox` 携带
`.semantic-input`，`InnerLeftContent` 包装 presenter 携带 `.semantic-prefix`，后缀 `StackPanel` 携带
`.semantic-suffix`，`PART_ClearButton` 携带 `.semantic-clear`。所有 marker 使用静态
`Classes.semantic-*="True"` 声明，不使用 Binding 或运行时赋值。`prefix` / `suffix` / `clear` 的 route 依次穿越
spinner 的前缀内容子树；`suffix` 和 `clear` 保留在 NumericUpDown 自己的模板内容中，分别通过
`/template/ .semantic-suffix` 和 `/template/ .semantic-clear` 选择。替换内层布局面板不会改变这些节点的
TemplatedParent、类型或 marker，两种模式仍提供相同的 Semantic Part 集合。完整契约见 [NumericUpDown Semantic Part 契约](semantic-part.md)。

## 6. 交互与事件处理

步进行为沿 Avalonia 基类的 `OnSpin` / `DoIncrement` / `DoDecrement` 语义更新 `Value`，并遵守 `Minimum`、`Maximum`、`Increment`、`IsReadOnly` 与 `AllowSpin`。

`IsKeyboardEnabled=false` 只拦截 `Up`、`Down`、`PageUp` 和 `PageDown` 的步进行为，不改变普通文本输入、焦点和鼠标行为。

清除按钮只在 `IsAllowClear=true`、`IsReadOnly=false` 且 `Text` 非空时显示。清除动作将 `Value` 置为 `null`，并通过基类文本和值同步机制更新显示状态。

`Mode=Spinner` 下左右按钮点击继续进入统一 spin 语义，不在 NumericUpDown 内直接执行 `Value += Increment` 或 `Value -= Increment`。

## 7. 内部算法与关键流程

### 7.1 String Mode

String mode 保存 raw text，同时尝试同步可计算 `Value`。内部同步必须避免 `Text`、`Value` 和 `StringValue` 之间递归更新。

用户设置自定义 `TextConverter` 时，关闭 string mode 后必须恢复用户转换器。编辑焦点在 TextBox 内时显示文本应尽量保持 raw text；失焦或显示格式刷新时才应用格式化策略。

### 7.2 浮动 Handle

浮动 Handle 由 `ButtonSpinnerDecoratedBox` 控制：

- normal 状态默认隐藏。
- pointer hover 输入壳体时显示。
- 显示时通过 `HandleOpacity` 和 `HandleOffset` 动画进入，不改变控件整体布局尺寸。
- 仅在有效启用、显示 Handle、允许浮动且 hover 时产生侧内容偏移；关闭 Handle 或离开该状态时，目标偏移归零。
- 偏移按 Handle 宽度扣除已有内容 padding，并保留半个侧 padding 作为间距；不能仅按 padding 倍数推导，以免小尺寸或自定义 Handle 宽度时重叠。
- disabled 清除 hover 状态，并保持 `HandleOpacity=0`。

两个模式的编辑器与后缀由内部 `NumericUpDownInputPanel` 共同排列。它消费装饰框 `ContentRightShift` 的当前有效动画值，先在排列宽度中扣除右侧避让量，再使用 DockPanel 分配后缀和编辑器；外层尺寸与自然期望尺寸不随动画追加宽度。编辑器右边界不得跨入后缀交互区域，后缀也不得进入浮动 Handle 的操作区域。

后缀不再单独声明 RenderTransform 或过渡，输入布局也不启动第二条动画。避让属性直接使 Arrange 失效；控件保留布局舍入，几何验证应考虑设备像素舍入。长文本由内部文本编辑器按真实 viewport 处理滚动和光标位置。

### 7.3 Spinner Mode 按需模板

`NumericUpDownSpinnerTheme.BasedOn` 使用真实 `{x:Type ButtonSpinner}` key，生成器按相同 key/TargetType 的
默认导出与类型条件证明基础主题保留。查找保持环境作用域：Application.Resources 中对 ButtonSpinner 主题的替换
仍被派生主题继承；不得为声明依赖而加入会遮蔽该替换的局部 include。content padding、派生模板和控件局部属性优先级保持不变。

Spinner mode 使用独立模板创建三段式结构。左右 action 按钮的宽度通过编译绑定跟随自身实际高度，保持正方形；按钮内 padding 为零，标准尺寸图标在其中居中。分隔线由外层段边框另行占位，不计入按钮本体的正方形尺寸。Small、Middle、Large 和自定义高度均使用同一几何规则，不使用浮动 Handle 的固定宽度或输入横向 padding 决定按钮宽度。

spinner 模式的外层 content frame 必须保持零 padding，避免与左右 action 段和中间输入段自身 padding 叠加，导致控件高度超过输入控件标准高度。

左右按钮 enabled 状态由 `AllowSpin`、`IsEnabled`、`IsReadOnly` 和当前值是否达到 min/max 共同决定。

左右 action 段可见性由 `ShowButtonSpinner` 控制；隐藏时不应占用三段式布局宽度，也不改变中间输入段的数值和文本同步语义。

### 7.4 Form 集成

Form 集成流：

```text
Form.SetValue(decimal?) → NumericUpDown.Value
NumericUpDown.ValueChanged → Form value changed
Form.ClearValue() → NumericUpDown.Value = null
DataValidationErrors → InputControlFrame.EffectiveStatus
Form.ValidateStatus → FormStatus → InputControlFrame + feedback
```

Form 不直接使用 `StringValue`。需要提交原始字符串时，由业务层绑定 `StringValue`。

## 8. 资源、性能与 AOT 边界

NumericUpDown 不通过反射访问 ButtonSpinner、TextBox 或 AddOnDecoratedBox 内部状态。跨控件协同通过公开属性、稳定 template part 和接口完成。

`Mode=Input` 是默认路径，必须避免 spinner 模式额外节点和额外按钮事件订阅。共享视觉问题应修在 `InputControlFrameTheme` 或 ButtonSpinner 布局主题层；NumericUpDown 专用的 inline 按钮视觉应修在 NumericUpDownSpinnerTheme 中，不能在 NumericUpDown 主题中复制跨模板 selector。

Token 通过动态资源进入主题。NumericUpDown 不把实例状态、当前值、文本、按钮 enabled 状态或模板切换状态写入 Token。

`IsCustomFontSize` 只通过局部绑定影响内部 `TextBox` 的字号样式选择，不应引入全局资源、反射访问或跨模板缓存。

## 9. 维护不变量

内部重构必须保持以下不变量：

- `Value`、`Text`、`StringValue` 同步不递归、不丢失 raw text。
- string mode 内部同步标志必须通过成对 helper 设置和恢复，不能在多个调用点手写进入/退出逻辑。
- `SizeType=Custom` 以 `Middle` 作为未显式覆盖时的默认视觉基线。
- `IsCustomFontSize=true` 不能被内部 `TextBox` 的 `SizeType` 字体样式覆盖。
- `IsKeyboardEnabled=false` 只影响步进快捷键。
- `Mode=Input` 不承担 spinner 模式成本。
- `Mode=Spinner` 不复制数值增减算法。
- `ShowButtonSpinner=false` 同时隐藏默认输入模式浮动 Handle 和 spinner 模式左右 action 段。
- NumericUpDown 本体不直接订阅 spinner 模式左右按钮事件。
- 清除按钮和 `InnerRightContent` 顺序不变。
- 禁用态隐藏浮动 Handle，并命中 disabled 文本色。
- Filled Handle 背景来自 `FilledHandleBg`。
- CompactSpace 状态传递给输入壳体，不在 NumericUpDown 中另写边框折叠规则。

## 10. 测试与验证

验证范围：

- `IsStringMode` 开关时 `TextConverter` 保存与恢复。
- Large / Middle / Small / Custom 尺寸下 `ButtonSpinner`、内部 `TextBox`、Handle 和 spinner action 对齐。
- `IsCustomFontSize=true` 时用户设置的 `FontSize` 不被内部 `TextBox` 的 `SizeType` 样式覆盖。
- `StringValue`、`Text`、`Value` 的双向同步和高精度 raw text 保留。
- `IsKeyboardEnabled=false` 对方向键和 PageUp / PageDown 的拦截。
- 清除按钮可见性和清除行为。
- Form 设置、读取、清空、native validation error 投射和扩展状态映射。
- CompactSpace 中边框厚度、圆角和位置变化。
- `Mode=Input` 默认视觉树不创建 spinner 左右按钮。
- `Mode=Spinner` 下左右按钮、min/max、disabled、read-only 和 `AllowSpin=false` 状态。
- `ShowButtonSpinner=false` 在 `Mode=Input` 和 `Mode=Spinner` 两种模板中都能隐藏步进入口，并支持运行时切换。
- 模板切换后新 part 重新接入，旧 part 事件解绑。
- 文档改动运行 `git diff --check`。

## 根表面主题投射

Input 模式投射根 inset 与外框几何；Spinner 模式保留左右 action 段的布局，根 padding 进入文本区。

几何默认、原生优先级和组合边界见[共享输入表面](../input-control-architecture-design.md#81-根表面几何与预设默认值)。
