# ButtonSpinner Token 设计

本文档定义 ButtonSpinner 相关控件 Token 的专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。ButtonSpinner 整体架构见 [ButtonSpinner 桌面版架构设计](overview.md)，内部实现原理见 [ButtonSpinner 桌面版实现原理](implementation.md)，设计和契约变化记录见 [ButtonSpinner Changelog](changelog.md)。

## 1. 定位

ButtonSpinner Token 只表达组件级视觉变量，例如尺寸、间距、颜色、圆角、阴影、图标尺寸和弹层边界。Token 不承载运行时选择、展开、加载、错误、上传任务、过滤条件或业务状态。

当前 Token scope：

- `ButtonSpinnerToken`，scope id 为 `ButtonSpinner`，源码位于 `src/AtomUI.Desktop.Controls/ButtonSpinner/ButtonSpinnerToken.cs`。

## 2. Token 分类

Token 按控件语义分类维护：

| 分类 | 语义 | 代表 Token |
| --- | --- | --- |
| 尺寸与密度 | 控件高度、宽度、图标尺寸、内容最小尺寸。 | `ControlWidth`、`HandleWidth`、`HandleIconSize` |
| 间距与布局 | padding、margin、gap、offset、popup content padding。 | 按源码 Token 语义维护 |
| 颜色与状态视觉 | 文本、背景、边框、hover、selected、active、disabled 视觉。 | `HandleBg`、`HandleActiveBg`、`HandleHoverColor`、`HandleBorderColor`、`FilledHandleBg` |
| 结构与装饰 | 圆角、阴影、指示器、弹层和装饰线相关变量。 | 按源码 Token 语义维护 |

未出现在上表中的 Token 仍按源码中的组件语义维护，不按代码顺序机械分类。

## 3. 控件专项模型中的 Token 使用

ButtonSpinner 的控件专项模型通过 Theme 消费 Token：

- C# 控件负责状态归一和伪类同步。
- AXAML/ControlTheme 负责把 Token 映射到背景、前景、边框、padding、尺寸和动效。
- Token 默认值从 SharedToken 派生，不直接读取控件实例状态。
- Token 类型、生成数据和 token.md 应显式维护，不依赖运行时反射扫描。

## 4. 控件家族影响

调整 ButtonSpinner Token 时必须评估以下范围：

- `ButtonSpinner`
- `ButtonSpinnerHandle`
- `ButtonSpinnerDecoratedBox`
- 对应 Gallery ShowCase 的示例和源码片段。
- Light/Dark 主题、Browser/Desktop 主题和 Compact/Form/Popup 集成场景。

## 5. 兼容性要求

- 不删除或重命名已生成的 TokenKind、TokenResource key 和 AXAML 引用。
- 不把实例状态、交互状态或 `EffectiveXxx` 状态写成 Token。
- 不在 Token 中展开颜色、variant 和状态的组合矩阵；组合关系应由 Theme selector 表达。
- Token 默认值变更必须同步评估 Gallery 示例和截图可观察外观。
- 如需引入新 Token，必须同步 Token 类型、生成资源、主题引用和本文档。

## 6. 承诺面裁决

2026-11-08 输入家族迁移按[承诺面](../../../../engineering/development/control-token-guidelines.md#own-token-承诺面)裁决：

| Token | 裁决 |
| --- | --- |
| `ControlWidth` | Reserved：对应 antd InputNumber `controlWidth`；ButtonSpinner 与 NumericUpDown 按内容或宿主布局取宽，主题默认宽度会覆盖全部布局，暂不应用 |

主题读法：`ButtonSpinnerTheme` 的全局值以 `OwnerTokenResource` 读取（它也是 NumericUpDownSpinner 的基础主题，owner 为
NumericUpDown）；`ButtonSpinnerDecoratedBoxTheme`、`ButtonSpinnerHandleTheme`、`SizeTypeAwareIconPresenter` 的全局值读取
所属控件的有效值，Handle 的 Own Token 与按下图标色读取 ButtonSpinner 的值（增减按钮是注册控件 IconButton）。
全部 Active Own Token 与消费的 Global Token 由 `InputFamilyEffectTests` 逐项验证生效。

## 7. 验证策略

| 改动类型 | 验证要求 |
| --- | --- |
| Token 文档 | `git diff --check`，检查相对链接存在。 |
| Token 默认值 | 运行对应控件测试，走查 Light/Dark 和 Browser 主题。 |
| Token 名称或数量 | 检查 generated TokenResource key、AXAML 引用和 token.md。 |
| 主题映射 | 走查 hover、pressed、selected、disabled、loading 等状态视觉。 |
