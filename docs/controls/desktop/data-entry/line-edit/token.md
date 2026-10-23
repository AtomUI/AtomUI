# LineEdit Token 设计

本文档定义 `TextBoxToken`、`LineEditToken` 与 `TextAreaToken` 在 LineEdit 输入家族中的专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。LineEdit 整体架构见 [LineEdit 桌面版架构设计](overview.md)，内部实现原理见 [LineEdit 桌面版实现原理](implementation.md)，设计和契约变化记录见 [LineEdit Changelog](changelog.md)。

## 1. 定位

LineEdit 输入家族使用三个控件级 Token scope：

| Token | Scope | 职责 |
| --- | --- | --- |
| `TextBoxToken` | `TextBox` | TextBox 专属内容 padding。 |
| `LineEditToken` | `LineEdit` | 单行输入框字号。 |
| `TextAreaToken` | `TextArea` | 多行输入框字号、右侧附加 padding 和 resize handle 视觉。 |

TextBox / LineEdit / TextArea Token 不承载文本值、placeholder、清除状态、密码 reveal、Form 状态、SearchEdit 运行状态、focus/hover/pressed 状态或 CompactSpace 运行时状态。输入表面边框、背景、圆角、shadow、error/warning、disabled 和 motion 由 `InputControlFrameTheme` 与各公开owner的Effective Global处理（SharedToken定义基础字段，不表示模板可以绕过owner直接读取）；运行时状态由 `AbstractTextInput`、Form 和 frame 状态模型处理。

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

三个Own默认值分别派生自有效Global FontSize/FontSizeLG/FontSizeSM。LineEdit自身在相应Own或Global字号被显式配置时消费Own最终资源，Custom使用中号；未配置时保留真实继承父的原生字号，因此不能把Large默认强改成16。`IsCustomFontSize=true`退出字号主题，LocalValue与普通用户Style仍按原生优先级覆盖。清除配置、改变父字号及重挂载必须恢复真实继承。raw Own资源在该档显式配置激活后参与最终取值；不根据资源数值猜测配置来源。

字体族使用LineEdit有效Global FontFamily的可选显式配置；未配置时同样保留原生继承。SelectFilterTextBox当前借用LineEdit字号是单列待迁移问题，不把它写成LineEdit公开owner接入成功，也不以修改Small默认字号消除身份问题。SearchEdit有独立主题和Global身份，本分包不对它注入LineEdit配置。

### 2.3 TextArea 字号 Token

- `TextArea.FontSize`
- `TextArea.FontSizeLG`
- `TextArea.FontSizeSM`

这些 Token 服务 `TextAreaTheme.axaml` 中多行输入不同尺寸下的默认字体大小。TextArea 的行高使用 SharedToken 的 `FontHeight`、`FontHeightLG`、`FontHeightSM`。

### 2.4 TextArea 右侧附加 padding Token

- `TextArea.RightAddOnPadding`
- `TextArea.RightAddOnPaddingLG`
- `TextArea.RightAddOnPaddingSM`

这些 Token 用于 TextArea 内部右侧内容与边框之间的空隙。它们根据 SharedToken 的水平 padding 和线宽计算，避免右侧 clear / feedback / resize 相关内容与文本区域重叠。

### 2.5 TextArea resize Token

- `TextArea.ResizeIndicatorLineColor`
- `TextArea.ResizeHandleSize`

这些 Token通过既有末层ResizeHandle控制TextArea右下角实际指示线颜色与命中/比例尺寸，描边来自TextArea有效Global LineWidth。它们只服务 `IsResizable=true` 的拖拽入口，不影响 TextArea 的自动高度算法或固定行数测量。

## 3. 控件专项模型中的 Token 使用

LineEdit 家族 Token 使用路径：

```text
SharedToken
   ↓
InputControlFrameTheme
   ↓
TextBoxTheme / LineEditTheme / SearchEditDecoratedBoxTheme / TextAreaDecoratedBoxTheme

TextBoxToken / LineEditToken / TextAreaToken
   ↓
各自文本尺寸、padding 和 resize 专属布局
```

输入表面的背景、hover/focus shadow、error/warning 和 disabled 状态由共享 `InputControlFrameTheme` 提供；边框厚度和圆角默认从公开 owner 的 Effective Global 进入原生属性，再投射到框体。TextBox 的内容 padding 由 `TextBoxTokenResource` 进入 `TextBoxTheme.axaml`；LineEdit 的单行字号由 `LineEditTokenResource` 消费；TextArea 的字号、右侧 padding 和 resize 视觉由 `TextAreaTokenResource` 及相关布局主题消费。

TextArea 的多行字号由 `TextAreaTokenResource` 进入 `TextAreaTheme.axaml`。TextArea右侧padding由装饰框主题消费；resize颜色/尺寸/描边由TextAreaTheme声明到实际末层ResizeHandle，绘制与命中使用同一Bounds。

SearchEdit 不定义独立 Own Token，但保留独立 Control Effective Global identity。它复用 `AbstractTextInput` 的状态与 LineEdit 的文本尺寸，输入表面由 `InputControlFrame` 统一表达，搜索按钮使用 Button 体系和 SearchButton 主题表达。

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

## 6. 验证策略

| 改动类型 | 验证要求 |
| --- | --- |
| 修改 TextBox padding Token | 验证 TextBox 的 Large/Middle/Small/Custom padding、clear/reveal 对齐，并评估依赖 AtomUI TextBox 的组合控件。 |
| 修改 LineEdit 字号 Token | 验证 LineEdit、SearchEdit 的 Large/Middle/Small/Custom 字号、line height、placeholder 和 clear/reveal 对齐。 |
| 修改 TextArea 字号 Token | 验证 Lines、MinLines、MaxLines、IsAutoSize 和 IsResizable 下的高度计算。 |
| 修改 TextArea padding Token | 验证 clear button、Form feedback、InnerRightContent、字数统计和文本区域不重叠。 |
| 修改 resize Token | 验证 resize handle 可见性、拖拽命中、线条颜色和尺寸。 |
| 删除或重命名 Token | 默认不允许；如获授权，需同步 AXAML 引用、生成文件、Token 类型、生成数据和 token.md和控件文档。 |
| 文档改动 | 运行 `git diff --check`，检查文档链接存在。 |

## 根几何与 Effective Global 消费

公开 owner 的 `Padding`、`BorderThickness`、`CornerRadius` 默认值从该 Control 的 Effective Global 读取，再以原生主题优先级投射到实际框体。共享框体继续负责状态色、focus/hover shadow、variant 与 CompactSpace；不能把它描述为跳过 owner 的全局几何入口。字号和其他消费链按各控件当前模板分别定义。本次未增加 Own 字段或改变 Token identity。详见 [共享输入架构的根几何合同](../input-control-architecture-design.md#81-根表面几何与预设默认值)。

## 公开输入owner的Global资源

TextBox/LineEdit/TextArea/SearchEdit自身模板的caret/selection/placeholder、状态文字、spacing、motion以及既有字体/行高读取，各自使用typed Global。字段仍来自共享Schema定义，不复制为Own，不把一个身份的配置传给另一个身份。内部frame与按钮等独立或复用owner继续按真实树拓扑逐项验证；四个根主题迁移不能代表它们完成。

## 默认求值协议

本Own定义使用DependencyDefaultsV2、Revision1，默认工厂以有效Global为输入，各字段没有Own之间的依赖；显式Own覆盖优先。名称、类型、公共资源编号及既有单位/范围保持，不因迁移另建别名。Global变化时重新计算未显式覆盖的默认值，清除Own后恢复对应公式。
