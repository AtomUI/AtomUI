# Skeleton Token 设计

本文档定义 Skeleton 相关控件 Token 的专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。Skeleton 整体架构见 [Skeleton 桌面版架构设计](overview.md)，内部实现原理见 [Skeleton 桌面版实现原理](implementation.md)，设计和契约变化记录见 [Skeleton Changelog](changelog.md)。

## 1. 定位

Skeleton Token 只表达组件级视觉变量，例如尺寸、间距、颜色、圆角、阴影、图标尺寸和弹层边界。Token 不承载运行时选择、展开、加载、错误、上传任务、过滤条件或业务状态。

当前 Token scope：

- `SkeletonToken`，scope id 为 `Skeleton`，源码位于 `src/AtomUI.Desktop.Controls/Skeleton/SkeletonToken.cs`。

## 2. Token 分类

Token 按控件语义分类维护：

| 分类 | 语义 | 代表 Token |
| --- | --- | --- |
| 尺寸与密度 | 控件高度、宽度、图标尺寸、内容最小尺寸。 | `TitleHeight`、`ParagraphLineHeight`、`ImageSize`、`ImageContainerSize`、`ImageContainerMaxSize` |
| 间距与布局 | padding、margin、gap、offset、popup content padding。 | `ParagraphMarginTop`、`AvatarMarginRight` |
| 颜色与状态视觉 | 文本、背景、边框、hover、selected、active、disabled 视觉。 | `GradientFromColor`、`GradientToColor`、`ImageColor` |
| 结构与装饰 | 圆角、阴影、指示器、弹层和装饰线相关变量。 | `BlockRadius`、`ParagraphLineHeight`、`ParagraphLineRoundCornerRadius` |

未出现在上表中的 Token 仍按源码中的组件语义维护，不按代码顺序机械分类。

`GradientFromColor` 是占位块底色与微光亮带底色，`GradientToColor` 是亮带峰值色，主题把它们写入 internal 的 `ShimmerFromColor`、
`ShimmerToColor`；`LoadingMotionDuration` 是亮带周期。`LoadingBackgroundStart`、`LoadingBackgroundMiddle`、`LoadingBackgroundEnd`
描述 Ant Design 的 400% 背景平移关键帧，默认由两种渐变色派生（两种渐变色被显式覆盖时，派生画刷使用覆盖值）；Desktop 主题不再引用它们，
它们决定已生成的 Token 资源键，因此保留。`TitleHeight` 是标题行高度（antd `titleHeight`），`ParagraphLineHeight` 是段落行高度
（antd `paragraphLiHeight`）。`ImageColor` 是图片占位图标的填充色（AtomUI 扩展；antd 固定为 `#bfbfbf`，默认值相同），取代主题中的
颜色字面量。

## 3. 控件专项模型中的 Token 使用

Skeleton 的控件专项模型通过 Theme 消费 Token：

- C# 控件负责状态归一和伪类同步。
- AXAML/ControlTheme 负责把 Token 映射到背景、前景、边框、padding、尺寸和动效。
- Token 默认值从 SharedToken 派生，不直接读取控件实例状态。
- Token 类型、生成数据和 token.md 应显式维护，不依赖运行时反射扫描。

### 3.1 主题读法与 Token owner

- `Skeleton` 是 Token owner 透明控件（`[TokenOwnerTransparent]`）：它包裹的已加载内容（Card / Dialog 正文、Statistic 数值）保持宿主的
  owner，宿主 `Controls[X]` 作用于内容中的无 identity 部件及其过渡时长。Skeleton 自身视觉全部以 `SkeletonTokenResource` 显式读取，
  占位部件是注册控件。
- `SkeletonAvatar`、`SkeletonButton`、`SkeletonInput`、`SkeletonImage`、`SkeletonNode`、`SkeletonLine`、`SkeletonParagraph` 以各自 identity
  读取全局 Token（尺寸、圆角、行间距），以 `SkeletonTokenResource` 读取 Skeleton Own Token。`SkeletonElementTheme`（SkeletonButton /
  SkeletonInput 的基础主题）与 `SkeletonImageTheme`（SkeletonNode 的基础主题）带 `<!-- owner: -->` 标记，以 `OwnerTokenResource` 读取。
- `SkeletonTitle` 与 SkeletonLine 共用主题（StyleKey 为 SkeletonLine），主题经 internal `IsTitle` 标记为标题行应用 `TitleHeight`。

## 4. 控件家族影响

调整 Skeleton Token 时必须评估以下范围：

- `Skeleton`
- `SkeletonAvatar`
- `SkeletonParagraph`
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
