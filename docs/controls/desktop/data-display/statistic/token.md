# Statistic Token 设计

本文档定义 Statistic 相关控件 Token 的专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。Statistic 整体架构见 [Statistic 桌面版架构设计](overview.md)，内部实现原理见 [Statistic 桌面版实现原理](implementation.md)，设计和契约变化记录见 [Statistic Changelog](changelog.md)。

## 1. 定位

Statistic Token 只表达组件级视觉变量，例如尺寸、间距、颜色、圆角、阴影、图标尺寸和弹层边界。Token 不承载运行时选择、展开、加载、错误、上传任务、过滤条件或业务状态。

当前 Token scope：

- `StatisticToken`，scope id 为 `Statistic`，源码位于 `src/AtomUI.Desktop.Controls/Statistic/StatisticToken.cs`。

## 2. Token 分类

Token 按控件语义分类维护：

| 分类 | 语义 | 代表 Token |
| --- | --- | --- |
| 尺寸与密度 | 控件高度、宽度、图标尺寸、内容最小尺寸。 | `TitleFontSize`、`ContentFontSize` |
| 间距与布局 | padding、margin、gap、offset、popup content padding。 | 按源码 Token 语义维护 |
| 颜色与状态视觉 | 文本、背景、边框、hover、selected、active、disabled 视觉。 | 按源码 Token 语义维护 |
| 结构与装饰 | 圆角、阴影、指示器、弹层和装饰线相关变量。 | 按源码 Token 语义维护 |

未出现在上表中的 Token 仍按源码中的组件语义维护，不按代码顺序机械分类。

## 3. 控件专项模型中的 Token 使用

Statistic 的控件专项模型通过 Theme 消费 Token：

- C# 控件负责状态归一和伪类同步。
- AXAML/ControlTheme 负责把 Token 映射到背景、前景、边框、padding、尺寸和动效。
- Token 默认值从 SharedToken 派生，不直接读取控件实例状态。
- Token 类型、生成数据和 token.md 应显式维护，不依赖运行时反射扫描。

## 4. 控件家族影响

调整 Statistic Token 时必须评估以下范围：

- `Statistic`
- `TimerStatistic`
- `StatisticCountUp`
- 对应 Gallery ShowCase 的示例和源码片段。
- Light/Dark 主题、Browser/Desktop 主题和 Compact/Form/Popup 集成场景。

## 5. 兼容性要求

- 不删除或重命名已生成的 TokenKind、TokenResource key 和 AXAML 引用。
- 不把实例状态、交互状态或 `EffectiveXxx` 状态写成 Token。
- 不在 Token 中展开颜色、variant 和状态的组合矩阵；组合关系应由 Theme selector 表达。
- Token 默认值变更必须同步评估 Gallery 示例和截图可观察外观。
- 如需引入新 Token，必须同步 Token 类型、生成资源、主题引用和本文档。

## 6. 验证策略

CT15 将 `StatisticToken` 迁移到 DependencyDefaultsV2 revision 1。`TitleFontSize` 与 `ContentFontSize` 分别依赖 Statistic Global 的 `FontSize`、`FontSizeHeading3`；四算法原默认、类型、key、slot 保持。两字段由 Statistic 身份持有，TimerStatistic 与 StatisticCountUp 沿用原限定 Own 资源，没有为三个 Global-only metadata 新增 Own fragment。

字号是正值。官方 Avalonia 12.1.4 对 0 字号会在文本布局抛出 `Invalid FontRenderingEmSize`；旧新实测一致，不将 0 解释为折叠文本或静默夹值。原生 `ContentFontSize`、控件字体和专用部件 Style 继续按各自目标的 Avalonia 优先级生效。CountUp 未明确配置自身 Global 字号时保持原共享 Own 内容字号；明确自身 Global `FontSize` 才覆盖此 fallback。

标题颜色、内容 heading 与 Statistic 原两种 spacing 角色在 ControlTheme 使用实际 owner Global。Root 可选字体保留真实继承，标题/内容字号仍是原两 Own 角色，不把 Root 字号等同于所有部件字号。范围与未通过项见 [CT15 Statistic 结果](../../../../superpowers/plans/2026-10-22-custom-theme/statistic-consumers-results.md)。

| 改动类型 | 验证要求 |
| --- | --- |
| Token 文档 | `git diff --check`，检查相对链接存在。 |
| Token 默认值 | 运行对应控件测试，走查 Light/Dark 和 Browser 主题。 |
| Token 名称或数量 | 检查 generated TokenResource key、AXAML 引用和 token.md。 |
| 主题映射 | 走查 hover、pressed、selected、disabled、loading 等状态视觉。 |
