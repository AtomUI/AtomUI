# Collapse Token 设计

本文档定义 Collapse 相关控件 Token 的专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。Collapse 整体架构见 [Collapse 桌面版架构设计](overview.md)，内部实现原理见 [Collapse 桌面版实现原理](implementation.md)，设计和契约变化记录见 [Collapse Changelog](changelog.md)。

## 1. 定位

Collapse Token 只表达组件级视觉变量，例如尺寸、间距、颜色、圆角、阴影、图标尺寸和弹层边界。Token 不承载运行时选择、展开、加载、错误、上传任务、过滤条件或业务状态。

当前 Token scope：

- `CollapseToken`，scope id 为 `Collapse`，源码位于 `src/AtomUI.Desktop.Controls/Collapse/CollapseToken.cs`。

## 2. Token 分类

Token 按控件语义分类维护：

| 分类 | 语义 | 代表 Token |
| --- | --- | --- |
| 尺寸与密度 | 控件高度、宽度、图标尺寸、内容最小尺寸。 | 按源码 Token 语义维护 |
| 间距与布局 | padding、margin、gap、offset、popup content padding。 | `HeaderPadding`、`ContentPadding`、`CollapseHeaderPaddingSM`、`CollapseHeaderPaddingLG`、`CollapseContentPaddingSM`、`CollapseContentPaddingLG` |
| 颜色与状态视觉 | 文本、背景、边框、hover、selected、active、disabled 视觉。 | `HeaderBg`、`ContentBg`、`CollapsePanelBorderRadius` |
| 结构与装饰 | 圆角、阴影、指示器、弹层和装饰线相关变量。 | `CollapsePanelBorderRadius` |

未出现在上表中的 Token 仍按源码中的组件语义维护，不按代码顺序机械分类。

## 3. 控件专项模型中的 Token 使用

折叠项在自身作用域按尺寸档成组读标题和内容内边距。`Custom` 使用 `Middle` 的默认值。实例值优先于 owner 值，owner 值优先于 Token 默认值。

Collapse 的控件专项模型通过 Theme 消费 Token：

- C# 控件负责状态归一和伪类同步。
- AXAML/ControlTheme 负责把 Token 映射到背景、前景、边框、padding、尺寸和动效。
- Token 默认值从 SharedToken 派生，不直接读取控件实例状态。
- Token 类型、生成数据和 token.md 应显式维护，不依赖运行时反射扫描。
- 全局 Token（`ColorBorder`、`BorderThickness`、`ColorTextHeading`、`IconSizeSM`、`FontSize*`、`FontHeight*`、`MotionDurationSlow`、
  `EnableMotion`、`ColorTextDisabled`）经 `CollapseTokenResource` 读取，`Controls[Collapse]` 覆盖生效。`CollapseItem` 是 Token owner
  透明部件（`[TokenOwnerTransparent]`），主题同样读 Collapse 的 Token，`Controls[CollapseItem]` 不产生效果；面板项内的展开按钮是注册的
  `IconButton`，保留自己的 identity（图标尺寸仍由 Collapse 的 `IconSizeSM` 设置）。
- `HeaderBg` 是面板头部背景，`ContentBg` 是面板内容区背景（antd `headerBg` / `contentBg`）；Borderless 内容区改用 `HeaderBg`，Ghost 头部与
  内容区都透明（`ColorTransparent`）。

## 4. 控件家族影响

调整 Collapse Token 时必须评估以下范围：

- `Collapse`
- `CollapseItem`
- 对应 Gallery ShowCase 的示例和源码片段。
- Light/Dark 主题、Browser/Desktop 主题和 Compact/Form/Popup 集成场景。

## 5. 兼容性要求

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

## 2026-11-20 新增 Token

默认值与新增前的外观一致；PrimeNG 主题在各自控件阶段中赋值。

| Token | 默认值 | 用途 |
| --- | --- | --- |
| `HeaderColor` | `ColorTextHeading` | 头部文字颜色 |
| `HeaderActiveColor` | `ColorTextHeading` | 展开项头部文字颜色 |
| `HeaderFontWeight` | `Normal` | 头部字重，设在语义 header 上 |
