# Tag Token 设计

本文档定义 Tag 相关控件 Token 的专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。Tag 整体架构见 [Tag 桌面版架构设计](overview.md)，内部实现原理见 [Tag 桌面版实现原理](implementation.md)，设计和契约变化记录见 [Tag Changelog](changelog.md)。

## 1. 定位

Tag Token 只表达组件级视觉变量，例如尺寸、间距、颜色、圆角、阴影、图标尺寸和弹层边界。Token 不承载运行时选择、展开、加载、错误、上传任务、过滤条件或业务状态。

当前 Token scope：

- `TagToken`，scope id 为 `Tag`，源码位于 `src/AtomUI.Desktop.Controls/Tag/TagToken.cs`。

## 2. Token 分类

Token 按控件语义分类维护：

| 分类 | 语义 | 代表 Token |
| --- | --- | --- |
| 尺寸与密度 | 控件高度、宽度、图标尺寸、内容最小尺寸。 | `TagFontSize`、`TagLineHeight`、`TagIconSize`、`TagCloseIconSize` |
| 间距与布局 | padding、margin、gap、offset、popup content padding。 | `TagPadding`、`TagTextPaddingInline` |
| 颜色与状态视觉 | 文本、背景、边框、Variant 基础视觉和 disabled 视觉。 | `DefaultBg`、`DefaultColor`、`SolidTextColor` |
| 结构与装饰 | 圆角、阴影、指示器、弹层和装饰线相关变量。 | `TagLineHeight` |

未出现在上表中的 Token 仍按源码中的组件语义维护，不按代码顺序机械分类。

## 3. 控件专项模型中的 Token 使用

Tag 的控件专项模型通过 Theme 消费 Token：

- C# 控件负责状态归一和伪类同步。
- AXAML/ControlTheme 负责把 Token 映射到背景、前景、边框、padding、尺寸和动效。
- Token 默认值以DependencyDefaultsV2/revision1从owner Global派生，不直接读取控件实例状态。
- Token 类型、生成数据和 token.md 应显式维护，不依赖运行时反射扫描。

`TagToken` 提供 Default 颜色和 Variant 基础视觉所需的组件语义值。`SolidTextColor` 根据 `ColorBgSolid` 的亮度选择黑色或白色，用于 Default Solid Tag 的文字对比度；Preset、Status 和 Custom 的颜色组合不复制到 Token 中，而由控件状态归一和 Theme selector 组合完成。

`CheckableTag` 复用 Tag 家族的字号、行高、Icon 尺寸、圆角和内容间距语义，并从CheckableTag自身Global读取primary、text、hover、pressed、focus和disabled颜色。checked 是控件实例状态，不新增 checked Token，也不复用普通 Tag 的 `Color × Variant` 矩阵。

`CheckableTagGroup` 不定义控件 Token。`ItemSpacing`和`LineSpacing`的默认值来自Group自身Global.SpacingXS；Options、IsMultiple、CheckedItem(s) 和内部 selection 均不进入 Token scope。

颜色组合遵循以下边界：

```text
TagToken / SharedToken / PresetPalette
    -> 控件视觉计算或 AXAML selector
    -> Foreground / Background / BorderBrush
```

Token 不保存 `TagColor`、`Variant` 或实例颜色值，也不为每个 `Color × Variant` 组合新增独立 Token。

## 4. 控件家族影响

调整 Tag Token 时必须评估以下范围：

- `Tag`
- `CheckableTag`
- `CheckableTagGroup` 及其内部 item container。
- 对应 Gallery ShowCase 的示例和源码片段。
- Light/Dark 主题、Browser/Desktop 主题和 Compact/Form/Popup 集成场景。

## 5. 兼容性要求

- 不删除或重命名已生成的 TokenKind、TokenResource key 和 AXAML 引用。
- 不把实例状态、交互状态或 `EffectiveXxx` 状态写成 Token。
- 不在 Token 中展开颜色、variant 和状态的组合矩阵；组合关系应由 Theme selector 表达。
- Token 默认值变更必须同步评估 Gallery 示例和截图可观察外观。
- `Variant=Filled` 是 Tag 的默认视觉契约；Filled、Solid、Outlined 的组合关系不能通过删除边框厚度表达。
- CheckableTag 只能消费可复用的 Tag/SharedToken 视觉语义，不能把 IsChecked、IsMultiple 或 Group 选择值写入 Token。
- CheckableTagGroup 保持无专属 Token；只有出现不能由 TagToken 或 SharedToken 表达的稳定组件视觉语义时才允许扩展 Token scope。
- 如需引入新 Token，必须同步 Token 类型、生成资源、主题引用和本文档。

## 6. 验证策略

| 改动类型 | 验证要求 |
| --- | --- |
| Token 文档 | `git diff --check`，检查相对链接存在。 |
| Token 默认值 | 运行对应控件测试，走查 Light/Dark 和 Browser 主题。 |
| Token 名称或数量 | 检查 generated TokenResource key、AXAML 引用和 token.md。 |
| 主题映射 | 走查 `Color × Variant`、CheckableTag 二态、Group 间距、Light/Dark、透明边框、Icon 和 CloseIcon 视觉。 |


## 7. CT15 实际消费者

| Own | 默认输入（保持原公式） | 真实目标/分支 |
| --- | --- | --- |
| DefaultBg | ColorFillTertiary在ColorBgContainer上合成 | default Filled/Outlined背景 |
| DefaultColor | ColorText | default Filled/Outlined前景 |
| SolidTextColor | 按ColorBgSolid亮度取黑/白 | 仅default Solid前景；不重写preset/status/custom角色 |
| TagFontSize | FontSizeSM | Tag字体、Checkable家族字号回退 |
| TagLineHeight | FontHeightSM | Tag文字行高、Checkable MinHeight |
| TagIconSize | FontSizeIcon | 真实IconPresenter尺寸 |
| TagCloseIconSize | IconSizeXS | close IconWidth/Height及真实glyph |
| TagPadding | SizeXS-1，水平Thickness | Tag/Checkable内容padding |
| TagTextPaddingInline | UniformlyPaddingXXS，水平Thickness | Tag文本padding、Checkable图标间距 |

普通Tag的颜色状态计算读取实际注册GetType/StyleKey owner的effective Global，命名palette仍用原共享preset数据与原1/3/6/7索引，保留信息别名与HSL公式。Native Brush、Font与padding、本owner Global、Own及专用Part的覆盖需按真实状态验证，不能把字段名相似等同重定向消费者。

Tag、CheckableTag、CheckableTagGroup拥有各自Global身份；Group无独立Own，不把Options/选择/checked/Variant放入配置。Checkable字号以TagFontSize为兼容回退，仅明确本owner FontSizeSM时覆盖；时长描述符仍由既有TokenTransitions解析owner，不替换成不存在的markup类。合法Icon/close/padding0与clear已在真实目标验证，未据名字给其他字段施加统一范围。


CT15当前仅[限定表](../../../../superpowers/plans/2026-10-22-custom-theme/tag-consumers-visual-review.md)26对及列明SelectTag派生影响已获批准；恢复/跨运行微差、CT13既定边界和完整Tag/CT15保留，不授权提交。
