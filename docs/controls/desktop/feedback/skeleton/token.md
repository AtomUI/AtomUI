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
| 颜色与状态视觉 | 文本、背景、边框、hover、selected、active、disabled 视觉。 | `GradientFromColor`、`GradientToColor` |
| 结构与装饰 | 圆角、阴影、指示器、弹层和装饰线相关变量。 | `BlockRadius`、`ParagraphLineHeight`、`ParagraphLineRoundCornerRadius` |

未出现在上表中的 Token 仍按源码中的组件语义维护，不按代码顺序机械分类。

## 3. 控件专项模型中的 Token 使用

Skeleton 的控件专项模型通过 Theme 消费 Token：

- C# 控件负责状态归一和伪类同步。
- AXAML/ControlTheme 负责把 Token 映射到背景、前景、边框、padding、尺寸和动效。
- Token 默认值从 SharedToken 派生，不直接读取控件实例状态。
- Token 类型、生成数据和 token.md 应显式维护，不依赖运行时反射扫描。

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

## 依赖默认计划与实际消费

SkeletonToken 保留15字段及原默认/资源编号，使用 DependencyDefaultsV2。Own 上游显式值先确定，派生默认随后求值；显式下游继续优先。

| 字段 | 默认依赖与实际消费者 |
| --- | --- |
| GradientFromColor / GradientToColor | 有效 Global ColorFillContent / ColorFill；静态背景及扫光端点颜色 |
| LoadingBackgroundStart / Middle / End | 两个 Own 渐变色；保留400%渐变坐标和三组 stop，作为实际动画关键帧 |
| LoadingMotionDuration | 原1.4秒；活动层周期，更新后重建本实例动画 |
| TitleHeight | Global ControlHeight/2；实际 SkeletonTitle，独立于段落行高 |
| ParagraphLineHeight / ParagraphLineRoundCornerRadius | Global ControlHeight/2；圆角默认依赖实际 Own 行高/2，服务行及 round 分支 |
| BlockRadius | Global BorderRadiusSM；非 round 行的实际 CornerRadius |
| ParagraphMarginTop / AvatarMarginRight | 原 Global margin 组合；组合模板中实际 paragraph/avatar |
| ImageSize / ImageContainerSize / ImageContainerMaxSize | ImageSize 默认 ControlHeight×1.5；后两项分别依赖实际 ImageSize×2/×4，服务图像图标、容器及上限 |

图像占位 glyph 的未配置色保持 #bfbfbf；明确的 SkeletonImage Global ColorTextPlaceholder 或实例 Foreground 可以到达真实图标。Node 承载调用方内容，不套用这项图像 glyph 前景回退。几何比例、段落内容和静态默认数据不作为额外 Own。
