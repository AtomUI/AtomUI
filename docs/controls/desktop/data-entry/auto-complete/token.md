# AutoComplete Token 设计

本文档定义 AutoComplete 相关控件 Token 的专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。AutoComplete 整体架构见 [AutoComplete 桌面版架构设计](overview.md)，内部实现原理见 [AutoComplete 桌面版实现原理](implementation.md)，设计和契约变化记录见 [AutoComplete Changelog](changelog.md)。

## 1. 定位

AutoComplete Token 只表达组件级视觉变量，例如尺寸、间距、颜色、圆角、阴影、图标尺寸和弹层边界。Token 不承载运行时选择、展开、加载、错误、上传任务、过滤条件或业务状态。

当前 Token scope：

- `AutoCompleteToken`，scope id 为 `AutoComplete`，源码位于 `src/AtomUI.Desktop.Controls/AutoComplete/AutoCompleteToken.cs`。

四个既有字段采用 DependencyDefaultsV2、声明修订1；静态默认只读取有效Global或原常量，没有Own依赖边。字段名称、类型、公开资源ID与内部slot保持。AutoCompleteSearchEdit/TextArea保留各自Global身份，本身没有另建Own定义；共享候选主题继续读取这套既有AutoComplete家族参数。

| 字段 | 默认与语义 |
| --- | --- |
| `OptionHeight` | 有效Global.ControlHeight；普通候选项的名义最小行高，同时参与行数预算。 |
| `PopupContentPadding` | 四边有效Global.UniformlyPaddingXXS/2；参与实际内容内缩与最大高度预算。 |
| `MinPopupWidth` | 120 DIP。 |
| `MaxPopupWidth` | 200 DIP，仅不跟随锚点宽度分支使用。 |

字体不是新增Own字段。三个公开AutoComplete外观分别使用自身有效Global.FontFamily及FontSize/SM/LG；普通/搜索未配置保留父字号继承，多行使用原可见三档预设并使root FontSize与其一致。标准root Style/Local字体优先。多行默认行框至少容纳实际字体自然行距，对应有效Global.FontHeight/SM/LG显式配置时仍为精确行高；不把字体消费扩成新的Own协议。

输入占位文字及弹层背景读取公开owner有效Global.ColorTextPlaceholder与Global.ColorBgElevated。均为既有Global字段，不新增Own、key或公式。

私有复用输入主题中的Global由其实际公开AutoComplete变体提供，包括正文、错误/警告/禁用文字、caret与selection；复用主题的字段名和原状态selector不变。标准root Foreground与专用Input Style继续按实际目标上的原生优先级生效。既有共享输入Own配方不转投、不生成新别名，真正公开子Button仍消费自身identity；不把父Global投射扩为整棵子树的统一资源替换。

## 2. Token 分类

Token 按控件语义分类维护：

| 分类 | 语义 | 代表 Token |
| --- | --- | --- |
| 尺寸与密度 | 控件高度、宽度、图标尺寸、内容最小尺寸。 | `OptionHeight`、`MinPopupWidth`、`MaxPopupWidth` |
| 间距与布局 | padding、margin、gap、offset、popup content padding。 | `PopupContentPadding` |
| 颜色与状态视觉 | 文本、背景、边框、hover、selected、active、disabled 视觉。 | 按源码 Token 语义维护 |
| 结构与装饰 | 圆角、阴影、指示器、弹层和装饰线相关变量。 | `PopupContentPadding`、`MinPopupWidth`、`MaxPopupWidth` |

未出现在上表中的 Token 仍按源码中的组件语义维护，不按代码顺序机械分类。

## 3. 控件专项模型中的 Token 使用

AutoComplete 的控件专项模型通过 Theme 消费 Token：

- C# 控件负责状态归一和伪类同步。
- AXAML/ControlTheme 负责把 Token 映射到背景、前景、边框、padding、尺寸和动效。
- Token默认值从有效Global派生或使用固定边界，不直接读取控件实例状态。
- Token 类型、生成数据和 token.md 应显式维护，不依赖运行时反射扫描。

## 4. 控件家族影响

调整 AutoComplete Token 时必须评估以下范围：

- `AutoComplete`
- 内部 `AutoCompleteLineEditBox` 输入资产，不是独立公开identity。
- `AutoCompleteSearchEdit`
- `AutoCompleteTextArea`
- 对应 Gallery ShowCase 的示例和源码片段。
- Light/Dark 主题、Browser/Desktop 主题和 Compact/Form/Popup 集成场景。

## 5. 兼容性要求

默认候选主题通过现有 `ItemContainerTheme` 将OptionHeight送入真实行的MinHeight；`MaxPopupHeight = OptionHeight × DisplayCandidateCount + PopupContentPadding.Top + PopupContentPadding.Bottom`。行高、行数与padding任一变化都重算。均匀普通行且自然高度不超过该最小值时形成对应名义行数；大字体、复杂模板或专用行Style可以使实际行更高，不承诺任意定制下仍完整显示同样行数。0保留资源与预算语义，不把文字自然高度强制写成0。

`AutoCompletePopupListItemStyle` 等各公开变体的专用Style按原生优先级覆盖默认行高，退出资格后恢复当前有效Own；父主题不代码查找行后写尺寸。

- 不删除或重命名已生成的 TokenKind、TokenResource key 和 AXAML 引用。
- 不把实例状态、交互状态或 `EffectiveXxx` 状态写成 Token。
- 不在 Token 中展开颜色、variant 和状态的组合矩阵；组合关系应由 Theme selector 表达。
- Token 默认值变更必须同步评估 Gallery 示例和截图可观察外观。
- 如需引入新 Token，必须同步 Token 类型、生成资源、主题引用和本文档。

## 6. 验证策略

| 改动类型 | 验证要求 |
| --- | --- |
| Token 文档 | `git diff --check`，检查相对链接存在。 |
| Token 默认值 | 运行对应控件测试，走查 Light/Dark 和 Browser 主题。 |
| Token 名称或数量 | 检查 generated TokenResource key、AXAML 引用和 token.md。 |
| 主题映射 | 走查 hover、pressed、selected、disabled、loading 等状态视觉。 |
