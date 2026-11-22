# RadioButton Token 设计

本文档定义 RadioButton 与 OptionButton 控件家族相关控件 Token 的专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。控件家族整体架构见 [RadioButton 桌面版架构设计](overview.md)，内部实现原理见 [RadioButton 桌面版实现原理](implementation.md)，OptionButtonGroup 的方向布局见 [OptionButtonGroup 方向布局设计](option-button-group-orientation-design.md)，设计和契约变化记录见 [RadioButton Changelog](changelog.md)。

## 1. 定位

RadioButton 和 OptionButton Token 只表达组件级视觉变量，例如指示器尺寸、内容 Padding、字体和交互状态颜色。Token 不承载 checked/selected、Orientation、GroupPositionTrait、EffectiveCornerRadius 或容器 Bounds 等运行时状态。

当前 Token scope：

- `RadioButtonToken`，scope id 为 `RadioButton`，源码位于 `src/AtomUI.Desktop.Controls/RadioButton/RadioButtonToken.cs`。
- `OptionButtonToken`，scope id 为 `OptionButton`，源码位于 `src/AtomUI.Desktop.Controls/OptionButtonGroup/OptionButtonToken.cs`。

## 2. Token 分类

Token 按控件语义分类维护：

| 分类 | 语义 | 代表 Token |
| --- | --- | --- |
| 尺寸与密度 | 单选指示器和按钮内容排版尺寸。 | `RadioSize`、`DotSize`、`ContentFontSize`、`ContentFontSizeLG`、`ContentFontSizeSM` |
| 间距与布局 | 指示器文字间距和按钮内容 Padding。 | `DotPadding`、`TextMargin`、`Padding`、`PaddingLG`、`PaddingSM` |
| 颜色与状态视觉 | 文本、背景、hover、checked、active、disabled 视觉。 | `DotColorDisabled`、`RadioColor`、`ButtonColor`、`ButtonSolidCheckedBackground`、`ButtonOutlineCheckedBackground`、`ButtonOutlineCheckedColor` |
| 结构与装饰 | 圆角、阴影、指示器、弹层和装饰线相关变量。 | 按源码 Token 语义维护 |

未出现在上表中的 Token 仍按源码中的组件语义维护，不按代码顺序机械分类。

## 3. 控件专项模型中的 Token 使用

RadioButton 的控件专项模型通过 Theme 消费 Token：

- C# 控件负责状态归一和伪类同步。
- AXAML/ControlTheme 负责把 Token 映射到背景、前景、边框、padding、尺寸和动效。
- Token 默认值从 SharedToken 派生，不直接读取控件实例状态。
- Token 类型、生成数据和 token.md 应显式维护，不依赖运行时反射扫描。
- OptionButtonGroup 的方向、位置和边框几何由 C# 状态与 Theme selector 表达，不为 Horizontal/Vertical 展开 Token 矩阵。
- Group 和 Item 的预设高度与圆角继续使用 SharedToken 的 `ControlHeight*` 和 `BorderRadius*`；方向能力不增加组件专属高度或圆角 Token。
- `ButtonPadding` 对应 antd Radio `buttonPaddingInline`，提供默认与大号 `ContentPadding` / `ContentPaddingLG` 的左右分量
  （只取 Left / Right；默认 `PaddingContentHorizontal - LineWidth`），小号沿用 `ContentPaddingSM`。
- `ButtonBackground`、`ButtonCheckedBackground` 对应 antd Radio `buttonBg`、`buttonCheckedBg`，当前为 Reserved：
  OptionButtonGroup 在选项之下绘制分隔线与选中边框，不透明的选项背景会盖住它们，因此选项保持透明；分组改为在选项之上绘制
  边框后再接通这两个 Token。
- `ContentLineHeight*` 是相对对应字号的行高倍数；`ContentPaddingLG`、`ContentPaddingSM` 的垂直内边距分别由
  `ControlHeightLG` / `ControlHeightSM` 与同档字号、行高计算。
- `RadioBgColor`（antd 内部 `radioBgColor`）是选中单选框的背景色，默认 `ColorPrimary`；`RadioColor` 是选中圆点颜色。Shadcn 暗色使用与底色配对的 `ColorTextLightSolid`。
  `RadioIndicator` 按原 Own 键成组读取 `DotSize`、`RadioSize` 和 `DotPadding`，计算动画使用的圆点尺寸。主题投射指示器的宽高和状态颜色。RadioButtonGroup 的选项保留 RadioButton 自身 identity。

## 4. 控件家族影响

调整 RadioButton Token 时必须评估以下范围：

- `RadioButton`
- `RadioButtonGroup`
- `OptionButton`
- `OptionButtonGroup`
- 对应 Gallery ShowCase 的示例和源码片段。
- Light/Dark 主题、Browser/Desktop 主题和 Compact/Form/Popup 集成场景。

## 5. 兼容性要求

- 不删除或重命名已生成的 TokenKind、TokenResource key 和 AXAML 引用。
- 不把实例状态、交互状态或 `EffectiveXxx` 状态写成 Token。
- 不在 Token 中展开颜色、variant 和状态的组合矩阵；组合关系应由 Theme selector 表达。
- 不增加 Horizontal/Vertical、First/Middle/Last/OnlyOne 或选中索引专属 Token。
- `SizeType=Custom` 通过主题基础值和实例定制实现，不能新增 Custom 专属固定 Token。
- Token 默认值变更必须同步评估 Gallery 示例和截图可观察外观。
- 如需引入新 Token，必须同步 Token 类型、生成资源、主题引用和本文档。

## 6. 验证策略

| 改动类型 | 验证要求 |
| --- | --- |
| Token 文档 | `git diff --check`，检查相对链接存在。 |
| Token 默认值 | 运行对应控件测试，走查 Light/Dark 和 Browser 主题。 |
| Token 名称或数量 | 检查 generated TokenResource key、AXAML 引用和 token.md。 |
| 主题映射 | 走查 hover、pressed、checked、disabled、Outline/Solid、Horizontal/Vertical 和三档 SizeType。 |

## 2026-11-20 新增 Token

默认值与新增前的外观一致；PrimeNG 主题在各自控件阶段中赋值。

| Token | 默认值 | 用途 |
| --- | --- | --- |
| `IndicatorHoverBorderColor` | `ColorPrimary` | 未选中指示器悬停边框 |
