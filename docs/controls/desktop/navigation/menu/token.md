# Menu Token 设计

本文档定义 Menu 相关控件 Token 的专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。Menu 整体架构见 [Menu 桌面版架构设计](overview.md)，内部实现原理见 [Menu 桌面版实现原理](implementation.md)，设计和契约变化记录见 [Menu Changelog](changelog.md)。

## 1. 定位

Menu Token 只表达组件级视觉变量，例如尺寸、间距、颜色、圆角、阴影、图标尺寸和弹层边界。Token 不承载运行时选择、展开、加载、错误、上传任务、过滤条件或业务状态。

当前 Token scope：

- `MenuToken`，scope id 为 `Menu`，源码位于 `src/AtomUI.Desktop.Controls/Menu/MenuToken.cs`。

## 2. Token 分类

Token 按控件语义分类维护：

| 分类 | 语义 | 代表 Token |
| --- | --- | --- |
| 尺寸与密度 | 控件高度、宽度、图标尺寸、内容最小尺寸。 | `MenuPopupMinWidth`、`MenuPopupMaxWidth`、`ItemHeight`、`ItemIconSize`、`TopLevelItemFontSizeSM` |
| 间距与布局 | padding、margin、gap、offset、popup content padding。 | `MenuPopupContentPadding`、`ItemMargin`、`ItemIconMarginInlineEnd`、`GroupTitlePadding`、`ItemPaddingInline`、`TopLevelItemPaddingSM`、`TopLevelItemPadding` |
| 颜色与状态视觉 | 文本、背景、边框、hover、selected、active、disabled 视觉。 | `MenuPopupBgColor`、`ItemColor`、`KeyGestureColor`、`ItemHoverColor`、`ItemDisabledColor`、`DangerItemColor` |
| 结构与装饰 | 圆角、阴影、指示器、弹层和装饰线相关变量。 | `MenuPopupContentPadding`、`MenuPopupMinWidth`、`MenuPopupMaxWidth`、`MenuPopupBgColor`、`ItemBorderRadius`、`TopLevelItemBorderRadiusSM` |

未出现在上表中的 Token 仍按源码中的组件语义维护，不按代码顺序机械分类。

要点：

- 菜单栏顶层项：默认文字 `TopLevelItemColor`、背景 `TopLevelItemBg`（默认透明，菜单栏自身绘制背景）；悬浮
  `TopLevelItemHoverColor` / `TopLevelItemHoverBg`；展开（菜单栏的选中项）`TopLevelItemSelectedColor` / `TopLevelItemSelectedBg`，
  默认与悬浮相同。
- 图标与文字间距 `ItemIconMarginInlineEnd`（默认 `UniformlyMarginXXS`，与此前的 `ItemMargin` 右侧值相同）同时作用于顶层项与弹层项的图标。
- `MenuPopupContentPadding` 控制菜单栏首层及嵌套弹层的内容内边距，标签溢出菜单复用此值。Shadcn 在原有上下间距的基础上，
  保留至少 `UniformlyPaddingXXS` 的留白，默认每侧 4 DIP。Lyra 和 Sera 的直角弹层因此也会在首末项的高亮与边框之间留白。
- 分组标题内间距 `GroupTitlePadding`（默认 `UniformlyPadding, UniformlyPaddingXS`，即此前写死的 16,8）。
- 危险项：`MenuItem.IsDanger`、`MenuItemData.IsDanger`（`IMenuItemData.IsDanger` 默认 `false`）标记，文字 `DangerItemColor`，悬浮文字
  `DangerItemHoverColor`。
- `MenuTearOffHeight` 已删除（从未消费，antd 无对应 Token，菜单没有 tear-off 项）。

## 3. 控件专项模型中的 Token 使用

Menu 的控件专项模型通过 Theme 消费 Token：

- C# 控件负责状态归一和伪类同步。
- AXAML/ControlTheme 负责把 Token 映射到背景、前景、边框、padding、尺寸和动效。
- Token 默认值从 SharedToken 派生，不直接读取控件实例状态。
- Token 类型、生成数据和 token.md 应显式维护，不依赖运行时反射扫描。

## 4. Token 读取与作用域

Menu、ContextMenu、MenuFlyoutPresenter 各有独立 Own。20个真实弹层字段由 AbstractMenuPopupToken 复用定义（Ant Design 的取值规则在共享辅助类 `MenuPopupTokenCalculator`），具体内部输入连接实际宿主与菜单项。TopLevel* 只属于 Menu，ContextMenuOffsetX/Y 只属于 ContextMenu。各自的有效全局值派生各自 Own，来源 Menu 覆盖不进入其他两种弹层。菜单栏顶层项主题 `TopLevelMenuItemTheme` 从 Menu
所在的资源作用域查找，局部资源中同名主题优先于应用资源。

独立 `MenuFlyout` 的原生和 Overlay Popup 默认阴影都取定位目标作用域内的 `MenuFlyoutPresenter.MenuPopupShadows`，应用设置的阴影属性可覆盖默认值。
通过 `FlyoutHost` 承载时使用宿主转发的阴影属性。菜单弹层的边框由 `MenuPopupBorderColor` 和 `MenuPopupBorderThickness` 共同决定，Ant Design 默认厚度为 0。

## 5. 控件家族影响

调整 Menu Token 时必须评估以下范围：

- `Menu`
- `MenuItem`
- `ContextMenu`
- `MenuFlyout`
- `MenuFlyoutPresenter`
- 对应 Gallery ShowCase 的示例和源码片段。
- Light/Dark 主题、Browser/Desktop 主题和 Compact/Form/Popup 集成场景。

## 6. 兼容性要求

- 不删除或重命名已生成的 TokenKind、TokenResource key 和 AXAML 引用。
- 不把实例状态、交互状态或 `EffectiveXxx` 状态写成 Token。
- 不在 Token 中展开颜色、variant 和状态的组合矩阵；组合关系应由 Theme selector 表达。
- Token 默认值变更必须同步评估 Gallery 示例和截图可观察外观。
- 如需引入新 Token，必须同步 Token 类型、生成资源、主题引用和本文档。

## 7. 验证策略

| 改动类型 | 验证要求 |
| --- | --- |
| Token 文档 | `git diff --check`，检查相对链接存在。 |
| Token 默认值 | 运行对应控件测试，走查 Light/Dark 和 Browser 主题。 |
| Token 名称或数量 | 检查 generated TokenResource key、AXAML 引用和 token.md。 |
| 主题映射 | 走查 hover、pressed、selected、disabled、loading 等状态视觉。 |
| 任意 Token | `NavigationFamilyEffectTests`（Menu / MenuItem / MenuItemGroup / MenuSeparator / ContextMenu / MenuFlyoutPresenter 行与隔离测试）全绿。 |

## 独立 Token 身份与共享计算

迁移 ContextMenuOffsetX/Y 到 Controls[ContextMenu]；其他弹层字段以实际 Menu/ContextMenu/MenuFlyoutPresenter 配置。菜单项内公开动作控件与各自独立的全局身份不做资源别名映射。

## 2026-11-20 新增 Token

默认值与新增前的外观一致；PrimeNG 主题在各自控件阶段中赋值。

| Token | 默认值 | 用途 |
| --- | --- | --- |
| `TitleFontWeight` | `Normal` | `MenuItemGroupToken`：分组标题字重，由 MenuItemGroup 主题直接读取 |
