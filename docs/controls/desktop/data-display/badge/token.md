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
| 尺寸与密度 | 控件高度、宽度、图标尺寸、内容最小尺寸。 | CountBadge `IndicatorHeight`、`IndicatorHeightSM`、`TextFontSize`、`TextFontSizeSM`；DotBadge `DotSize` |
| 间距与布局 | padding、margin、gap、offset、popup content padding。 | RibbonBadge `Offset`、`TextPadding`；DotBadge `LabelMargin`；CountBadge `TextPadding` |
| 颜色与状态视觉 | 文本、背景、边框、hover、selected、active、disabled 视觉。 | CountBadge `TextColor`、`IndicatorColor`、`ShadowColor`；DotBadge `DotColor`、`ShadowColor` |
| 结构与装饰 | 圆角、阴影、指示器、折角、弹层和装饰线相关变量。 | CountBadge `CornerRadius`、`CornerRadiusSM`、`ShadowSize`；DotBadge `ShadowSize`；RibbonBadge `CornerTransform`、`CornerDarkenAmount` |

未出现在上表中的 Token 仍按源码中的组件语义维护，不按代码顺序机械分类。

## 3. 控件专项模型中的 Token 使用

Badge 的控件专项模型通过 Theme 消费 Token：

- C# 控件负责状态归一和伪类同步。
- AXAML/ControlTheme 负责把 Token 映射到背景、前景、边框、padding、尺寸和动效。
- Token 默认值从 SharedToken 派生，不直接读取控件实例状态。
- Token 类型、生成数据和 token.md 应显式维护，不依赖运行时反射扫描。

### 3.1 读取方式与预设色

- 三个徽标的指示器（`CountBadgeAdorner`、`DotBadgeAdorner`、`DotBadgeIndicator`、`RibbonBadgeAdorner`）没有 Token
  identity，各自只属于一个宿主，主题以宿主的 `CountBadgeTokenResource`、`DotBadgeTokenResource`、`RibbonBadgeTokenResource`
  显式读取 Own Token 与 Global Token（含 `MotionDurationMid`、DotBadge 状态色、Ribbon 的 `ColorPrimary`、`BorderRadiusSM`、
  `ColorTextLightSolid`）。带装饰目标时指示器位于 AdornerLayer，经逻辑父级（徽标本身）解析资源，`Controls[CountBadge]`
  等覆盖同样到达。
- 桌面主题把圆点和数量徽标的阴影颜色与扩展量交给各自的实际绘制部件。圆点指示器和数量 Adorner 分别按原 Own 键成组读取，整组变更时只计算一次阴影。
- Ribbon Adorner 按同一作用域成组读取 `Offset`、`CornerTransform` 和 `CornerDarkenAmount`。公开的 `Offset` 仍按调用方配置叠加。
- `BadgeColor`、`DotColor`、`RibbonColor` 的预设色名对应 antd `genPresetColor` 的 `darkColor`（色板第 6 级），取自徽标所在
  主题快照的预设色板（暗色主题为暗色色板），主题变化时重算；预设色不跟随 `ColorPrimary`。其他文本按颜色解析；未设置或无效时
  清除内部值，回到主题的 `IndicatorColor`、状态色或 `DotColor`、`ColorPrimary`。共享的色板查找位于
  `src/AtomUI.Controls/Primitives/PresetColorPalette.cs`，Tag 与 FloatButton 徽标同样使用，ToolTip 将在其家族迁移中复用。
- `CornerTransform` 的覆盖值按矩阵格式书写（`1,0,0,0.75,0,0` 即 antd `scaleY(0.75)`）。

### 3.2 RibbonBadge Token 语义

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

## 2026-11-20 新增 Token

默认值与新增前的外观一致；PrimeNG 主题在各自控件阶段中赋值。

| Token | 默认值 | 用途 |
| --- | --- | --- |
| `TextFontWeight` | `Normal` | CountBadge 计数文本字重 |
