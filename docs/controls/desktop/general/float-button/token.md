# FloatButton Token 设计

本文档定义 FloatButton 相关控件 Token 的专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。FloatButton 整体架构见 [FloatButton 桌面版架构设计](overview.md)，内部实现原理见 [FloatButton 桌面版实现原理](implementation.md)，设计和契约变化记录见 [FloatButton Changelog](changelog.md)。

## 1. 定位

FloatButton Token 只表达组件级视觉变量，例如尺寸、间距、颜色、圆角、阴影、图标尺寸和弹层边界。Token 不承载运行时选择、展开、加载、错误、上传任务、过滤条件或业务状态。

当前 Token scope：

- `FloatButtonToken`，scope id 为 `FloatButton`，源码位于 `src/AtomUI.Desktop.Controls/FloatButton/FloatButtonToken.cs`。

## 2. Token 分类

Token 按控件语义分类维护：

| 分类 | 语义 | 代表 Token |
| --- | --- | --- |
| 尺寸与密度 | 控件高度、宽度、图标尺寸、内容最小尺寸。 | `FloatButtonSize`、`FloatButtonIconSize`、`DescriptionLineHeight` |
| 间距与布局 | padding、margin、gap、offset、popup content padding。 | `SquareBadgeOffset`、`CircleBadgeOffset`、`FloatOffsetX`、`FloatOffsetY` |
| 颜色与状态视觉 | 文本、背景、边框、hover、selected、active、disabled 视觉。 | `PrimaryColor` |
| 结构与装饰 | 圆角、阴影、指示器、弹层和装饰线相关变量。 | `DescriptionLineHeight` |

未出现在上表中的 Token 仍按源码中的组件语义维护，不按代码顺序机械分类。

## 3. 控件专项模型中的 Token 使用

FloatButton 的控件专项模型通过 Theme 消费 Token：

- C# 控件负责状态归一和伪类同步。
- AXAML/ControlTheme 负责把 Token 映射到背景、前景、边框、padding、尺寸和动效。
- Token 默认值从 SharedToken 派生，不直接读取控件实例状态。
- Token 类型、生成数据和 token.md 应显式维护，不依赖运行时反射扫描。
- BackTop 进度环不新增Own；原LineWidthBold/ColorBorderSecondary/ColorPrimary角色现在由实际BackTopFloatButton owner Global解析，避免跳过局部identity。

## 4. 控件家族影响

调整 FloatButton Token 时必须评估以下范围：

- `FloatButton`
- `FloatButtonGroup`
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

## CT15 消费与兼容分类

8默认采用DependencyDefaultsV2/revision1，四算法32值/type/key/slot保持。当前六Active：FloatButtonSize进入Circle宽高/Square宽与最小高，FloatButtonIconSize进入IconPresenter，PrimaryColor用于Primary前景/图标，DescriptionLineHeight用于Square描述，FloatOffsetX/Y进入三Host原定位。两按钮两shape及三Host真实TopLeft73,91/Native81已验证。

CircleBadgeOffset/SquareBadgeOffset已获限定Reserved分类批准，当前仅兼容单标量声明，无既定二维映射。真实BadgeOffset为公开Point，原Circle/Square×Count/Dot四配方与Native优先保持；明确Own37无Point消费事实不等于宣称字段可定制通过。[Reserved兼容保留提案](../../../../superpowers/plans/2026-10-22-custom-theme/float-button-token-ruling.md)列明历史、替代入口、复核条件与代价，2026-10-31已获限定批准，两项不再列Own未决，但不缩原8字段或274总输入，不宣称已消费。

字体保留原FontSizeSM角色与Native值，Family可选继承；原颜色、阴影、圆角/间距按实际Float/BackTop/Group owner投射。代码创建badge仅做原Local准备期typed relay，membership退出释放；BadgeColor实例层清空/无效回Token而不是null。当前验证与完整视觉/平台出口见[执行记录](../../../../superpowers/plans/2026-10-22-custom-theme/float-button-consumers.md)，不以默认迁移关闭家族。


CT15清理后限定验收：FloatButton精确46项通过、NET8零警告零错误，临时49用例/媒体/驱动已清理；65对具体静态及两offset分类仍待用户审查，完整家族不因此关闭。详见[正式限定结果](../../../../superpowers/plans/2026-10-22-custom-theme/float-button-consumers-results.md)。


2026-10-31：65对具体静态（含列明派生换行/裁剪）与两scalar offset Reserved兼容分类已获用户限定批准；两scalar保持未消费，以公开BadgeOffset Point定制，完整家族/CT15及其他边界保留。该批准不授权提交。
