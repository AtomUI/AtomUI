# Badge Token 设计

本文档定义 Badge 相关控件 Token 的专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。Badge 整体架构见 [Badge 桌面版架构设计](overview.md)，内部实现原理见 [Badge 桌面版实现原理](implementation.md)，设计和契约变化记录见 [Badge Changelog](changelog.md)。

## 1. 定位

Badge Token 只表达组件级视觉变量，例如尺寸、间距、颜色、圆角、阴影、图标尺寸和弹层边界。Token 不承载运行时选择、展开、加载、错误、上传任务、过滤条件或业务状态。

当前 Token scope：

- `CountBadgeToken`，源码位于 `src/AtomUI.Desktop.Controls/Badge/CountBadgeToken.cs`。
- `DotBadgeToken`，源码位于 `src/AtomUI.Desktop.Controls/Badge/DotBadgeToken.cs`。
- `RibbonBadgeToken`，源码位于 `src/AtomUI.Desktop.Controls/Badge/RibbonBadgeToken.cs`。

## 2. Token 分类

Token 按控件语义分类维护：

| 分类 | 语义 | 代表 Token |
| --- | --- | --- |
| 尺寸与密度 | 控件高度、宽度、图标尺寸、内容最小尺寸。 | `IndicatorHeight`、`IndicatorHeightSM`、`DotSize`、`TextFontSize`、`TextFontSizeSM`、`StatusSize` |
| 间距与布局 | padding、margin、gap、offset、popup content padding。 | `BadgeRibbonOffset`、`BadgeRibbonTextPadding`、`DotBadgeLabelMargin`、`CountBadgeTextPadding` |
| 颜色与状态视觉 | 文本、背景、边框、hover、selected、active、disabled 视觉。 | `BadgeTextColor`、`BadgeColor`、`BadgeColorHover`、`BadgeShadowColor` |
| 结构与装饰 | 圆角、阴影、指示器、折角、弹层和装饰线相关变量。 | `IndicatorHeight`、`IndicatorHeightSM`、`BadgeShadowSize`、`BadgeShadowColor`、`CountBadgeCornerRadius`、`CountBadgeCornerRadiusSM`、`BadgeRibbonCornerTransform`、`BadgeRibbonCornerDarkenAmount` |

未出现在上表中的 Token 仍按源码中的组件语义维护，不按代码顺序机械分类。

## 3. 控件专项模型中的 Token 使用

Badge 的控件专项模型通过 Theme 消费 Token：

- C# 控件负责状态归一和伪类同步。
- AXAML/ControlTheme 负责把 Token 映射到背景、前景、边框、padding、尺寸和动效。
- Token 默认值从 SharedToken 派生，不直接读取控件实例状态。
- Token 类型、生成数据和 token.md 应显式维护，不依赖运行时反射扫描。

### 3.1 RibbonBadge Token 语义

`RibbonBadgeToken` 只描述 Ribbon 默认视觉所需的静态指标。它不保存 `Placement`、public `Offset`、目标尺寸或实例
`RibbonColor`。运行时布局必须把这些 Token 作为输入，和 owner 当前状态共同计算最终几何。

| Token | 语义 |
| --- | --- |
| `Offset.X` | target mode 下 Ribbon 主体相对目标 Start/End 边界的水平外伸距离，并作为默认折角宽度输入。 |
| `Offset.Y` | target mode 下 Ribbon 主体相对目标上边缘的垂直避让距离，并作为默认折角高度输入。 |
| `CornerTransform` | 折角暗面几何的默认形状变换；实现使用时必须同步影响折角点位或完整 geometry cache key。 |
| `CornerDarkenAmount` | 折角暗面相对 `RibbonColor` 的加深量。 |
| `TextPadding` | Ribbon 文本主体内部 padding，不参与目标外伸或折角尺寸计算。 |

`Offset` 的两个坐标是兼容性语义，不应在实现中被模糊复用为任意位移。若默认视觉需要把水平外伸、垂直避让、折角宽度
和折角高度拆成独立 Token，必须保留现有 `Offset` key 的兼容意义，并同步 Theme、生成资源、Gallery 默认视觉和渲染回归。

## 4. 控件家族影响

调整 Badge Token 时必须评估以下范围：

- `CountBadge`
- `DotBadge`
- `RibbonBadge`
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
| Ribbon offset / 折角 Token | 验证 Start/End、target/standalone、长短文本和预设色/自定义色下的主体位置、折角三角形和背景区像素。 |
| 主题映射 | 走查 hover、pressed、selected、disabled、loading 等状态视觉。 |
