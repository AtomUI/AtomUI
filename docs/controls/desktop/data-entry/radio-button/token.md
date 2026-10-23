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
| 颜色与状态视觉 | 文本、背景、hover、checked、active、disabled 视觉。 | `DotColorDisabled`、`RadioColor`、`ButtonColor`、`ButtonSolidCheckedBackground` |
| 结构与装饰 | 圆角、阴影、指示器、弹层和装饰线相关变量。 | 按源码 Token 语义维护 |

未出现在上表中的 Token 仍按源码中的组件语义维护，不按代码顺序机械分类。

## 3. 控件专项模型中的 Token 使用

RadioButton 的控件专项模型通过 Theme 消费 Token：

- C# 控件负责状态归一和伪类同步。
- AXAML/ControlTheme 负责把 Token 映射到背景、前景、边框、padding、尺寸和动效。
- Token 默认值从 SharedToken 派生，不直接读取控件实例状态。
- Token 类型、生成数据和 token.md 应显式维护，不依赖运行时反射扫描。
- OptionButtonGroup 的方向、位置和边框几何由 C# 状态与 Theme selector 表达，不为 Horizontal/Vertical 展开 Token 矩阵。
- Group 和 Item 的预设高度与圆角分别使用自身有效 Global 的 `ControlHeight*` 和 `BorderRadius*`；方向能力不增加组件专属高度或圆角 Token。

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

## 6. OptionButton 精确默认依赖与消费

`OptionButtonToken` 的 19 字段使用 `DependencyDefaultsV2`、Revision 1。各尺寸字体先求有效 Own，再派生 `ContentLineHeight*`，最后派生 `ContentPadding*`；显式下游值（包括零）不被默认公式覆盖。这里的行高是**比例**，与 Button 的 DIP 行高契约不同。

| 输入 | 默认与消费 |
| --- | --- |
| `ContentFontSize` / `SM` / `LG` | Global `FontSize` / `FontSize` / `FontSizeLG`；映射 Item 对应尺寸字体 |
| `ContentLineHeight*` | 根据对应有效字体计算比例；内容行框高度为字体乘比例 |
| `ButtonPadding` | 基础留白，默认 16,0；普通/Large水平减描边，Small水平减半后减描边；纵向基础留白加对应尺寸行框居中余量，各边非负 |
| `ContentPadding*` | Middle/Large/Small分别读取对应高度、字体和行高；显式值直接进入 Item。Large默认资源从 15,0 修正为 15,7，默认既有固定高度渲染保持一致 |
| `ButtonColor` | 两种外观的未选中前景 |
| `ButtonBackground` | 显式配置后进入未选中背景及Outline选中背景的回退；缺省仍透明，保留非白父背景 |
| `ButtonCheckedBackground` | 显式配置后覆盖Outline选中背景；缺省沿用普通背景。Solid选中使用专属语义色 |
| `ButtonSolidCheckedColor` / `Background` / `HoverBackground` / `ActiveBackground` | Solid选中各交互状态的实际前景/背景 |
| `ButtonCheckedBgDisabled` / `ButtonCheckedColorDisabled` | 两种外观选中且禁用时的背景/前景 |

两个可选背景仍保留原 `ColorBgContainer` 默认资源读值；渲染使用 [WhenExplicit](../../../../modules/core/configured-token-resources.md) 存在位与普通资源绑定组合，不能用默认资源值推断是否显式配置。原生本地属性和应用 Style 保持 Avalonia 优先级。

`OptionButtonGroup` 与公开子 `OptionButton` 的 Global identity 分别解析。Group 控制组合外框、水平固定高度和选中边框；Item 控制自身字体、留白和状态色。仅给 Item 配高度不会解除 Group 的固定高度约束。两个 Transition 集合在 ControlTheme style Setter 中消费 Item 的动态 `MotionDurationMid`，不把初始化资源值固化到共享 Transition。

## FontFamily 的精确与继承合同

本控件的类型化Global FontFamily在正规化配置直接包含该字段时进入根渲染；未配置则保留真实InheritanceParent的字体，清除恢复当前祖先。Shared字体配置同样生效，显式配置与缺省不按值相等猜测。FontFamily Setter由ControlTheme MultiBinding提供，控件只暴露内部只读继承源；原生Style/local及资源覆盖保持。接口观察在分离时释放父订阅，重挂刷新，不以逻辑父代替受保护的真实继承父。详细机制见[显式Token资源](../../../../modules/core/configured-token-resources.md)。

OptionButton与OptionButtonGroup分别拥有该入口；Group字体通过正常继承进入未单独配置的Item，Item显式配置保持自身身份。
