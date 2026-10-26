# Breadcrumb Token 设计

本文档定义 Breadcrumb 相关控件 Token 的专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。Breadcrumb 整体架构见 [Breadcrumb 桌面版架构设计](overview.md)，内部实现原理见 [Breadcrumb 桌面版实现原理](implementation.md)，设计和契约变化记录见 [Breadcrumb Changelog](changelog.md)。

## 1. 定位

Breadcrumb Own 表达条目／链接／分隔符的颜色及内距，不保存导航目标、当前条目、hover、分隔符实例或 motion 状态。

当前 Token scope：

- `BreadcrumbToken`，identity 为 `AtomUI:Breadcrumb`，有八项 Own，源码位于 `src/AtomUI.Desktop.Controls/Breadcrumb/BreadcrumbToken.cs`。

`BreadcrumbItem` 和运行时 separator 消费根 owner 的资源，不新增独立 Own identity。图标宽高使用 Shared 的 `IconSizeSM`，不存在 Breadcrumb Own `IconSize`。

## 2. Token 分类

- `ItemColor`：根前景、普通条目前景及普通图标颜色，默认读取有效 Global `ColorTextDescription`。
- `LastItemColor`：没有导航能力的末项前景，默认读取 `ColorText`。有导航能力的末项仍走链接分支。
- `LinkColor`：可导航条目内容呈现器前景，默认读取 `ColorTextDescription`；item 级前景定制不覆盖该模板节点的链接色。
- `LinkHoverColor`：同一 hover 分支的内容及图标颜色，默认读取 `ColorText`。
- `LinkHoverBgColor`：可导航条目 hover 背景，默认读取 `ColorBgTextHover`。
- `SeparatorColor`：兄弟分隔符 presenter 前景，默认读取 `ColorTextDescription`。
- `BreadcrumbItemContentPadding`：条目内容内距，四边均为 `Global.UniformlyPaddingXXS`。
- `SeparatorMargin`：分隔符外距，默认为 `(Global.UniformlyMarginXXS, 0)`。

内距单位是 DIP，明确零值保留为有效配置；不增加统一钳制。Uniformly 数值字段与独立的 `PaddingXXS`／`MarginXXS` Thickness 字段分开，不能仅凭相近名称推断别名关系。root frame、圆角和图标尺寸继续遵守 Shared／Semantic Part／实例属性的既有边界。

## 3. 控件专项模型中的 Token 使用

八项默认以 `DependencyDefaultsV2`／Revision1 的静态默认函数求值，只读取有效 Global，没有 Own→Own 依赖。Control 专属 Global 进入该 owner 的有效输入，明确 Own 优先，移除后恢复当前有效 Global 派生值。

六项由原 `BreadcrumbTheme`／`BreadcrumbItemTheme` 的 Setter 和选择器消费。`SeparatorColor`／`SeparatorMargin` 由 `BreadcrumbSeparatorManager` 绑定到运行时创建的兄弟 `ContentPresenter`；owner 管理其逻辑挂载、模板重套与卸载释放。此迁移不改变原绑定、节点拓扑、优先级或状态时序。

## 4. 控件家族影响

调整 Breadcrumb Token 时必须评估以下范围：

- `Breadcrumb`
- `BreadcrumbItem`
- `BreadcrumbItemData`
- 对应 Gallery ShowCase 的示例和源码片段。
- Light/Dark 主题、Browser/Desktop 主题和 Compact/Form/Popup 集成场景。

## 5. 兼容性要求

- 不删除或重命名已生成的 TokenKind、TokenResource key 和 AXAML 引用。
- 保留八项原默认公式、枚举数值与 dense slot、明确 Own／透明色／零值／撤回和同实例重挂；枚举数值和 slot 分别核对。
- 不把实例状态、交互状态或 `EffectiveXxx` 状态写成 Token。
- 不在 Token 中展开颜色、variant 和状态的组合矩阵；组合关系应由 Theme selector 表达。
- Token 默认值变更必须同步评估 Gallery 示例和截图可观察外观。
- 如需引入新 Token，必须同步 Token 类型、生成资源、主题引用和本文档。

## 6. 验证策略

| 改动类型 | 验证要求 |
| --- | --- |
| Token 文档 | `git diff --check`，检查相对链接存在。 |
| Token 默认值 | 核对真实 Compiler 明暗资源、类型、key 与 slot，覆盖 Global、Control Global、八项 Own、零值、撤回及重挂。 |
| Token 名称或数量 | 检查 generated TokenResource key、AXAML 引用和 token.md。 |
| 主题映射 | 走查普通／末项／链接／hover、分隔符 N−1；在真实主题事务后的产品 Window 做同历史 A/A、1 DIP 和新旧比较。 |

专用 Part 定制使用生成的 `BreadcrumbItemStyle`／`BreadcrumbSeparatorStyle`，不通过 Loaded 后回写节点绕开契约。Headless、实际原生窗口、NativeAOT 和各平台结果分别记录，不互相替代。
