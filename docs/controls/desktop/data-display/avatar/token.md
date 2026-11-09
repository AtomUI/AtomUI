# Avatar Token 设计

本文档定义 Avatar 相关控件 Token 的专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。Avatar 整体架构见 [Avatar 桌面版架构设计](overview.md)，内部实现原理见 [Avatar 桌面版实现原理](implementation.md)，设计和契约变化记录见 [Avatar Changelog](changelog.md)。

## 1. 定位

Avatar Token 只表达组件级视觉变量，例如尺寸、间距、颜色、圆角、阴影、图标尺寸和弹层边界。Token 不承载运行时选择、展开、加载、错误、上传任务、过滤条件或业务状态。

当前 Token scope：

- `AvatarToken`，scope id 为 `Avatar`，源码位于 `src/AtomUI.Controls/Avatar/AvatarToken.cs`。
- `AvatarGroupToken`，scope id 为 `AvatarGroup`，源码位于 `src/AtomUI.Desktop.Controls/Avatar/AvatarGroupToken.cs`。

## 2. Token 分类

Token 按控件语义分类维护：

| 分类 | 语义 | 代表 Token |
| --- | --- | --- |
| 尺寸与密度 | 控件高度、宽度、图标尺寸、内容最小尺寸。 | `ContainerSize`、`ContainerSizeLG`、`ContainerSizeSM`、`TextFontSize`、`TextFontSizeLG`、`TextFontSizeSM` |
| 间距与布局（AvatarGroup） | 折叠弹层中头像的间距和组内相邻头像的重叠宽度（antd Avatar `groupSpace` / `groupOverlapping`）。 | `AvatarGroup.GroupSpace`、`AvatarGroup.GroupOverlapping` |
| 颜色与状态视觉 | 文本、背景、边框、hover、selected、active、disabled 视觉。 | `GroupBorderColor`、`AvatarBg`、`AvatarColor` |
| 结构与装饰 | 圆角、阴影、指示器、弹层和装饰线相关变量。 | 按源码 Token 语义维护 |

未出现在上表中的 Token 仍按源码中的组件语义维护，不按代码顺序机械分类。

`TextFontSize*` 是图标头像的图标尺寸，默认值与 antd 6.7 的 `iconFontSize*` 相同（antd 6.7 另有取 `fontSize` 的 `textFontSize*`
作为文字字号）；AtomUI 的文字头像按容器缩放文字，不读取这组 Token。`GroupSpace` 与 `GroupOverlapping`
是 AvatarGroup 的 Own Token（`Controls[AvatarGroup]`），由 AvatarGroup 的有效全局 Token（`UniformlyMarginXXS` / `UniformlyMarginXS`）派生；
它们此前定义在 `AvatarToken` 上，但只有 Desktop 的 AvatarGroup 消费。

## 3. 控件专项模型中的 Token 使用

Avatar 的控件专项模型通过 Theme 消费 Token：

- C# 控件负责状态归一和伪类同步。
- AXAML/ControlTheme 负责把 Token 映射到背景、前景、边框、padding、尺寸和动效。
- Token 默认值从 SharedToken 派生，不直接读取控件实例状态。
- Token 类型、生成数据和 token.md 应显式维护，不依赖运行时反射扫描。

主题读法：`AvatarTheme`（AtomUI.Controls）以 `AvatarTokenResource` 读取全局 Token，`AvatarGroupTheme` 以 `AvatarGroupTokenResource` 读取。
组内头像保持 Avatar identity：组框颜色 `GroupBorderColor` 跟随 `Controls[Avatar]`；折叠信息头像的前景与背景读取 Avatar 的
`AvatarColor` / `AvatarBg`。

## 4. 控件家族影响

调整 Avatar Token 时必须评估以下范围：

- `Avatar`
- `AvatarGroup`
- 对应 Gallery ShowCase 的示例和源码片段。
- Light/Dark 主题、Browser/Desktop 主题和 Compact/Form/Popup 集成场景。

## 5. 兼容性要求

- 不删除或重命名已生成的 TokenKind、TokenResource key 和 AXAML 引用。
- 不把实例状态、交互状态或 `EffectiveXxx` 状态写成 Token。
- 不在 Token 中展开颜色、variant 和状态的组合矩阵；组合关系应由 Theme selector 表达。
- Token 默认值变更必须同步评估 Gallery 示例和截图可观察外观。
- 如需引入新 Token，必须同步 Token 类型、生成资源、主题引用和本文档。
- `AvatarToken` 由 `AtomUI.Controls` 拥有；Desktop 的 `AvatarGroupTheme` 只能消费它，不能在 Desktop 再定义同名 Token。

## 6. 验证策略

| 改动类型 | 验证要求 |
| --- | --- |
| Token 文档 | `git diff --check`，检查相对链接存在。 |
| Token 默认值 | 运行对应控件测试，走查 Light/Dark 和 Browser 主题。 |
| Token 名称或数量 | 检查 generated TokenResource key、AXAML 引用和 token.md。 |
| 主题映射 | 走查 hover、pressed、selected、disabled、loading 等状态视觉。 |
