# Rate Token 设计

本文档定义 Rate 相关控件 Token 的专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。Rate 整体架构见 [Rate 桌面版架构设计](overview.md)，内部实现原理见 [Rate 桌面版实现原理](implementation.md)，设计和契约变化记录见 [Rate Changelog](changelog.md)。

## 1. 定位

Rate Token 只表达组件级视觉变量，例如尺寸、间距、颜色、圆角、阴影、图标尺寸和弹层边界。Token 不承载运行时选择、展开、加载、错误、上传任务、过滤条件或业务状态。

当前 Token scope：

- `RateToken`，scope id 为 `Rate`，源码位于 `src/AtomUI.Desktop.Controls/Rate/RateToken.cs`。

## 2. Token 分类

Token 按控件语义分类维护：

| 分类 | 语义 | 代表 Token |
| --- | --- | --- |
| 尺寸与密度 | 控件高度、宽度、图标尺寸、内容最小尺寸。 | `StarSize`、`StarSizeSM`、`StarSizeLG` |
| 间距与布局 | padding、margin、gap、offset、popup content padding。 | 按源码 Token 语义维护 |
| 颜色与状态视觉 | 文本、背景、边框、hover、selected、active、disabled 视觉。 | `StarColor`、`StarHoverScale`、`StarBg` |
| 结构与装饰 | 圆角、阴影、指示器、弹层和装饰线相关变量。 | 按源码 Token 语义维护 |

未出现在上表中的 Token 仍按源码中的组件语义维护，不按代码顺序机械分类。

## 3. 控件专项模型中的 Token 使用

Rate 的控件专项模型通过 Theme 消费 Token：

- C# 控件负责状态归一和伪类同步。
- AXAML/ControlTheme 负责把 Token 映射到背景、前景、边框、padding、尺寸和动效。
- Token 按字段定义从 Effective Global 派生或采用明确的内容默认值，不读取控件实例状态。
- Token 类型、生成数据和 token.md 应显式维护，不依赖运行时反射扫描。

## 4. 控件家族影响

调整 Rate Token 时必须评估以下范围：

- `Rate`
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

## 字符 glyph 字体

`CharacterFontFamily: FontFamily` 的字段默认值保持 `FontFamily.Default`；它是字符内容字体 Own，不把 Global.FontFamily 复制成 Own 默认。只有明确 Own 才投射该值，否则明确 Global 或实际继承提供根字体。char/string 两条绘制路径在字体更新后重建快照，SVG Icon Character 保留自身矢量内容。当前7字段均采用依赖默认计划。

## 默认计划与字符字体

7字段使用 DependencyDefaultsV2，名称/类型/key/slot及默认不变。StarColor 复用现有标准黄色预设缓存，并按当前 appearance 选择明暗色；StarBg、三档 StarSize 从该 owner 的有效 Global 取得，StarHoverScale 保留原1.2。

字符字体依次选择明确 CharacterFontFamily Own、明确 Global FontFamily、真实继承。实例 FontFamily 仍由原生优先级覆盖。字符的 em 尺寸继续跟随 StarSize 对应的星形几何，Root FontSize 不替代星形尺寸。图标内容与字符内容保持原语义，焦点虚线及 glyph 裁剪属于固定绘制几何。
