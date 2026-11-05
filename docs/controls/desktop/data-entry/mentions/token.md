# Mentions Token 设计

本文档定义 `AtomUI.Desktop.Controls.MentionsToken` 的专属语义、分类、使用范围和兼容边界。共享输入表面分层见 [输入控件共享架构设计](../input-control-architecture-design.md)，控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。Mentions 整体架构见 [Mentions 桌面版架构设计](overview.md)，内部实现原理见 [Mentions 桌面版实现原理](implementation.md)，设计和契约变化记录见 [Mentions Changelog](changelog.md)。

## 1. 定位

MentionsToken 是 Mentions 的控件级 Token scope，只描述候选弹层的结构尺寸。输入框本体由内部 `MentionTextArea` 复用 TextArea / `AbstractTextInput` / `InputControlFrame` / SharedToken 体系；候选列表项的选择、hover、disabled 和文本状态由 CandidateList 主题处理。

三个既有字段采用DependencyDefaultsV2、声明修订1，只有有效Global与常量输入，没有Own依赖边；名称、类型、公开资源ID和slot保持。求值不读取实例输入/选择或Popup状态。

MentionsToken 不承载以下状态：

- `Value`、`FilterValue`、`TriggerPrefix`、`OptionsSource`、`OptionsAsyncLoader` 等数据状态。
- `IsDropDownOpen`、`IsLoading`、`IsReadOnly`、`IsAutoSize`、`Status` 等运行状态。
- 候选项 selected、hover、disabled、commit、cancel 等交互状态。
- Popup 当前 offset、placement、实际高度或异步加载结果。

输入本体字体不是新增Own字段。Mentions使用自身有效Global.FontFamily及FontSize/SM/LG，未配置字体族保留父继承，字号保持原可见三档预设并使root FontSize与其一致。标准root Style/Local字体优先。多行默认行框至少容纳实际字体自然行距，对应有效Global.FontHeight/SM/LG显式配置时仍为精确行高；不把字体消费扩成新的Own协议。

输入占位文字及弹层背景读取Mentions有效Global.ColorTextPlaceholder与Global.ColorBgElevated；加载指示器布局margin读取Global.MarginLG。均为既有Global字段，不新增Own、key或公式。

私有MentionTextArea复用主题中的Global转投Mentions，包括正文、错误/警告/禁用文字、caret与selection；字段和原状态selector保持。标准Foreground与专用Input Style遵守实际目标上的原生优先级。TextArea既有共享Own配方不转投或别名化，真正公开子Control保留自身identity，不进行整棵子树的资源覆盖。

## 2. Token 分类

### 2.1 候选弹层 padding Token

- `PopupContentPadding`

该Token控制PopupFrame内部padding，并参与MaxPopupHeight计算；padding热更新会立即重算预算。默认值为有效Global.UniformlyPaddingXXS/2的四边等距padding。

### 2.2 候选项高度 Token

- `OptionHeight`

该Token表示普通候选项的名义最小行高，默认值为有效Global.ControlHeight。父主题通过现有ItemContainerTheme复用CandidateListItemTheme，将它声明式送入真实候选行MinHeight，并结合DisplayCandidateCount与padding计算最大高度。自然高度更大的字体/模板或专用Style可以高于该最小值；名义页数不承诺任意定制下的完整行数。0保持资源与预算语义，不强写文字自然高度为0。

### 2.3 候选弹层最小宽度 Token

- `MinPopupWidth`

该 Token 控制候选弹层最小宽度，默认值为 `120`。实际弹层宽度仍受 popup placement、候选内容和布局约束影响。

## 3. 控件专项模型中的 Token 使用

MentionsToken 使用路径：

```text
Effective Global / constants
   ↓
MentionsToken
   ↓
MentionsTheme.axaml
   ↓
PopupFrame + MaxPopupHeight + CandidateList viewport
```

主题使用点：

- `ItemHeight="{atom:MentionsTokenResource OptionHeight}"`
- `PopupContentPadding="{atom:MentionsTokenResource PopupContentPadding}"`
- `MinPopupWidth="{atom:MentionsTokenResource MinPopupWidth}"`

`MaxPopupHeight` 不是 Token。它由 `ItemHeight`、`DisplayCandidateCount` 和 `PopupContentPadding` 在控件实例上计算，属于运行时布局结果。

## 4. 控件家族影响

MentionsToken 只影响 Mentions 候选弹层，不直接影响 AutoComplete、Select、TreeSelect、ComboBox 或普通 TextArea。虽然这些控件也有 popup padding、option height 和 min popup width 语义，但它们各自拥有独立 Token scope。

调整 MentionsToken 时必须评估：

- `MentionsTheme.axaml`
- Mentions token.md 语义说明
- `MentionsShowCasePageTests`
- 候选弹层 loading、候选列表可视高度和 popup 最小宽度

## 5. 兼容性要求

MentionsToken 属于 Mentions 主题契约。即使 `MentionsToken` 是 internal 类型，生成的 `MentionsTokenKind`、AXAML resource 使用点和 token.md 语义说明已经形成稳定依赖。

Token 变更要求：

- 不擅自重命名或删除 `PopupContentPadding`、`OptionHeight`、`MinPopupWidth`。
- 不把实例数据、过滤状态、loading 状态、候选选择状态或 Form 状态迁移为 Token。
- 不在 MentionsToken 中复制 TextArea 输入边框、字体、清除按钮或校验状态 Token。
- 不把 `DisplayCandidateCount` 变成 Token；它是实例级行为属性。
- 需要破坏性变更时，必须先说明影响范围并获得授权。

## 6. 验证策略

| 改动类型 | 验证要求 |
| --- | --- |
| 修改 `PopupContentPadding` | 验证 popup 内边距、loading 布局、候选项裁剪和 `MaxPopupHeight`。 |
| 修改 `OptionHeight` | 验证候选项高度、键盘选中项可见性和 `DisplayCandidateCount` 对弹层高度的影响。 |
| 修改 `MinPopupWidth` | 验证短候选、长候选、Top/Bottom placement 和 Gallery 示例弹层宽度。 |
| 删除或重命名 Token | 默认不允许；如获授权，需同步 AXAML 引用、生成文件、Token 类型、生成数据和 token.md和控件文档。 |
| 文档改动 | 运行 `git diff --check`，检查文档链接存在。 |
