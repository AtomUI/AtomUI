# Carousel Token 设计

本文档定义 Carousel 相关控件 Token 的专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。Carousel 整体架构见 [Carousel 桌面版架构设计](overview.md)，内部实现原理见 [Carousel 桌面版实现原理](implementation.md)，设计和契约变化记录见 [Carousel Changelog](changelog.md)。

## 1. 定位

Carousel Token 只表达组件级视觉变量，例如尺寸、间距、颜色、圆角、阴影、图标尺寸和弹层边界。Token 不承载运行时选择、展开、加载、错误、上传任务、过滤条件或业务状态。

当前 Token scope：

- `CarouselToken`，scope id 为 `Carousel`，源码位于 `src/AtomUI.Desktop.Controls/Carousel/CarouselToken.cs`。

## 2. Token 分类

Token 按控件语义分类维护：

| 分类 | 语义 | 代表 Token |
| --- | --- | --- |
| 尺寸与密度 | 控件高度、宽度、图标尺寸、内容最小尺寸。 | `IndicatorWidth`、`IndicatorHeight`、`IndicatorActiveWidth`、`ArrowSize` |
| 间距与布局 | padding、margin、gap、offset、popup content padding。 | `IndicatorGap`、`PaginationOffset`、`ArrowOffset` |
| 颜色与状态视觉 | 文本、背景、边框、hover、selected、active、disabled 视觉。 | `IndicatorActiveWidth` |
| 结构与装饰 | 圆角、阴影、指示器、弹层和装饰线相关变量。 | `IndicatorWidth`、`IndicatorHeight`、`IndicatorGap`、`IndicatorActiveWidth` |

未出现在上表中的 Token 仍按源码中的组件语义维护，不按代码顺序机械分类。

## 3. 控件专项模型中的 Token 使用

Carousel 的控件专项模型通过 Theme 消费 Token：

- C# 控件负责状态归一和伪类同步。
- AXAML/ControlTheme 负责把 Token 映射到背景、前景、边框、padding、尺寸和动效。
- Token 默认值从 SharedToken 派生，不直接读取控件实例状态。
- Token 类型、生成数据和 token.md 应显式维护，不依赖运行时反射扫描。

## 4. 控件家族影响

调整 Carousel Token 时必须评估以下范围：

- `Carousel`
- `CarouselPage`
- `CarouselPagination`
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

## CT15 依赖与真实消费

七Own迁DependencyDefaultsV2/revision1，四算法28默认/type/key/slot保持。IndicatorWidth/Height为普通点尺寸，IndicatorActiveWidth为选中点宽，IndicatorGap进入真实分页StackPanel.Spacing；PaginationOffset仍控制分页边缘与未显式指定箭头时的原边距。ArrowSize进入导航glyph；ArrowOffset仅显式Own激活（含0），缺值回原分页offset，不能把其默认SpacingXS强施到旧12边距。Native优先和clear均按原属性系统处理。

组合导航/分页/indicator的Global由Carousel拥有；独立CarouselNavButton保留自身Global身份，CarouselPage保持自身字体Global/Native及正常继承。命名颜色、内容页背景和用户文本仍属原内容，不能统一替换成主题色。正常持续动画、旧新/AOT与清理后验收尚在执行，见[CT15记录](../../../../superpowers/plans/2026-10-22-custom-theme/carousel-consumers.md)。

CT15阶段验证：26聚焦+2持续工作用例通过，managed/AOT各48，112静态跨运行精确0；临时材料已清理，最终3精确用例/NET8零警告通过。44对有意静态变化待限定审查，4跨运行时相/16旧新持续态和完整家族边界保留，见[结果](../../../../superpowers/plans/2026-10-22-custom-theme/carousel-consumers-results.md)。
