# Separator Token 设计

本文档定义 Separator 相关控件 Token 的专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。Separator 整体架构见 [Separator 桌面版架构设计](overview.md)，内部实现原理见 [Separator 桌面版实现原理](implementation.md)，设计和契约变化记录见 [Separator Changelog](changelog.md)。

## 1. 定位

Separator Token 只表达组件级视觉变量，例如尺寸、间距、颜色、圆角、阴影、图标尺寸和弹层边界。Token 不承载运行时选择、展开、加载、错误、上传任务、过滤条件或业务状态。

当前 Token scope：

- `SeparatorToken`，scope id 为 `Separator`，源码位于 `src/AtomUI.Desktop.Controls/Separator/SeparatorToken.cs`。

## 2. Token 分类

Token 按控件语义分类维护：

| 分类 | 语义 | 代表 Token |
| --- | --- | --- |
| 尺寸与密度 | 控件高度、宽度、图标尺寸、内容最小尺寸。 | 按源码 Token 语义维护 |
| 间距与布局 | padding、margin、gap、offset、popup content padding。 | `TextPaddingInline`、`OrientationMarginPercent`、`VerticalMarginInline`、`HorizontalMarginBlockSM`、`HorizontalMarginBlock`、`HorizontalMarginBlockLG` |
| 颜色与状态视觉 | 文本、背景、边框、hover、selected、active、disabled 视觉。 | 按源码 Token 语义维护 |
| 结构与装饰 | 圆角、阴影、指示器、弹层和装饰线相关变量。 | 按源码 Token 语义维护 |

未出现在上表中的 Token 仍按源码中的组件语义维护，不按代码顺序机械分类。

## 3. 控件专项模型中的 Token 使用

Separator 的控件专项模型通过 Theme 消费 Token：

- C# 控件负责状态归一和伪类同步。
- AXAML/ControlTheme 负责把 Token 映射到背景、前景、边框、padding、尺寸和动效。
- Token 默认值从 SharedToken 派生，不直接读取控件实例状态。
- 水平分隔线的 `HorizontalMarginBlockSM`、`HorizontalMarginBlock`、`HorizontalMarginBlockLG` 分别从 SharedToken 的 `UniformlyMarginXS`、`UniformlyMargin`、`UniformlyMarginLG` 派生；默认对应 `8px`、`16px`、`24px`，并同时适用于带标题和无标题布局。
- `CustomizableSizeType.Custom` 不引入第四组间距 Token。Theme 使用 `HorizontalMarginBlock` 作为基础回退值，并把最终 `Margin` 的接管权留给实例或 owner-scoped Style。
- Token 类型、生成数据和 token.md 应显式维护，不依赖运行时反射扫描。

## 4. 控件家族影响

调整 Separator Token 时必须评估以下范围：

- `Separator`
- `VerticalSeparator`
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

## 动态消费与兼容导出

7字段保留原类型/默认/key/slot，采用 DependencyDefaultsV2。TextPaddingInline、OrientationMarginPercent 更新会重新排列实际 rail；VerticalMarginInline 和字体尺寸变化使测量失效。横向三档 margin 仍同时用于有/无标题，Custom 使用基础回退并允许调用方接管。

HorizontalWithTextGutterMargin 保留导出和默认值，当前没有几何消费者，不叠加到既有三档 margin。标题和 rail 的 Global 语义从 Separator 实例解析；VerticalSeparator 保留现有 StyleKey/身份回退，不另造 Token 家族。
