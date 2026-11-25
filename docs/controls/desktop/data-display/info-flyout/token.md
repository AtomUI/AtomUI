# InfoFlyout Token 设计

本文档定义 InfoFlyout 相关控件 Token 的专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。InfoFlyout 整体架构见 [InfoFlyout 桌面版架构设计](overview.md)，内部实现原理见 [InfoFlyout 桌面版实现原理](implementation.md)，设计和契约变化记录见 [InfoFlyout Changelog](changelog.md)。

## 1. 定位

InfoFlyout Token 只表达组件级视觉变量，例如尺寸、间距、颜色、圆角、阴影、图标尺寸和弹层边界。Token 不承载运行时选择、展开、加载、错误、上传任务、过滤条件或业务状态。

当前 Token scope：

- `FlyoutHostToken`，scope id 为 `FlyoutHost`，源码位于 `src/AtomUI.Desktop.Controls/Flyouts/FlyoutHostToken.cs`。
- `TreeViewFlyoutPresenterToken`，scope id 为 `TreeViewFlyoutPresenter`，源码位于 `src/AtomUI.Desktop.Controls/Flyouts/TreeViewFlyoutPresenterToken.cs`。

## 2. Token 分类

Token 按控件语义分类维护：

| 分类 | 语义 | 代表 Token |
| --- | --- | --- |
| 尺寸与密度 | 控件高度、宽度、图标尺寸、内容最小尺寸。 | `PopupMinWidth`、`PopupMaxWidth`、`PopupMinHeight`、`PopupMaxHeight` |
| 间距与布局 | padding、margin、gap、offset、popup content padding。 | `MarginToAnchor`、`HorizontalOffset`、`VerticalOffset`、`PopupContentPadding` |
| 颜色与状态视觉 | 文本、背景、边框、hover、selected、active、disabled 视觉。 | `PopupBgColor` |
| 结构与装饰 | 圆角、阴影、指示器、弹层和装饰线相关变量。 | `OverlayHostShadow`、`PopupRootShadow`、`PopupBgColor`、`PopupContentPadding`、`PopupMinWidth`、`PopupMaxWidth` |

未出现在上表中的 Token 仍按源码中的组件语义维护，不按代码顺序机械分类。

## 3. 控件专项模型中的 Token 使用

InfoFlyout 的控件专项模型通过 Theme 消费 Token：

- C# 控件负责状态归一和伪类同步。
- AXAML/ControlTheme 负责把 Token 映射到背景、前景、边框、padding、尺寸和动效。
- Token 默认值从 SharedToken 派生，不直接读取控件实例状态。
- Token 类型、生成数据和 token.md 应显式维护，不依赖运行时反射扫描。

## 4. 控件家族影响

调整 InfoFlyout Token 时必须评估以下范围：

- `FlyoutHost`
- `Flyout`
- `MenuFlyout`
- `TreeViewFlyout`
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

FlyoutHost 的全局动效读取自身有效值；MenuFlyout / TreeViewFlyout 的 pointer placement offset 在打开会话内读取 placement target 作用域中的 FlyoutHost Own Token，关闭、detach 时释放绑定。TreeViewFlyoutPresenter 的 `PopupBgColor` 设置 presenter 默认背景，经现有 TemplateBinding 传到箭头容器；显式根 Background 保持优先。

受保护的 `BindPointerPlacementOffsets(object, object)` 只把 `FlyoutHostTokenKind` 规范化为 FlyoutHost Own key；SharedTokenKind、自定义枚举/字符串和已经类型化的 key 在 placement target 作用域中保持原义。已打开的同一 flyout 可直接 `ShowAt(newTarget)`：旧会话先关闭并释放绑定，再在新目标作用域中建立定位绑定；无需调用方先 Hide。同目标关闭动效中的 ShowAt 仍取消关闭并复用当前会话。

## 箭头框体 Token（`ArrowDecoratedBoxToken`）

Flyout 的弹层表面 `FlyoutPresenter` 是 `ArrowDecoratedBox`，其外观 Token 属于共享的 `ArrowDecoratedBoxToken`
（scope id `ArrowDecoratedBox`，源码 `src/AtomUI.Desktop.Controls/Primitives/ArrowDecoratedBox/ArrowDecoratedBoxToken.cs`），
同一组 Token 也作用于 PopupConfirm、Picker 面板、下拉菜单、树形 flyout、Tour、DataGrid 筛选 flyout 与 ColorPicker 面板：

| Token | 派生来源 | 语义 |
| --- | --- | --- |
| `ArrowSize` | `SizePopupArrow / 1.3` | 箭头尺寸（底边宽度；箭头长度约为其一半）。 |
| `ContentPadding` | `PaddingXS` | 框体内边距。 |
| `ContentBorderColor` | `ColorBorderSecondary` | 框体与箭头的边框色。 |
| `ContentBorderThickness` | `0` | 边框宽度；非零时框体自绘一条环绕框体与箭头的整体描边（箭头底边无横线），模板容器 `Border` 只保留布局宽度。 |
| `ContentMinHeight` | `ControlHeight` | 框体最小高度。 |
| `ArrowStrokeColor` / `ArrowStrokeThickness` | 7% 黑 / 1 | 无边框时箭头自身的描边。 |

Ant Design 的 `ContentBorderThickness` 默认为 0。ToolTip 主题将自身箭头框体的 `BorderThickness` 固定为 0。
PrimeNG 的取值包从 Popover 配方读取框体内边距和边框颜色，框体描边厚度使用当前 `LineWidth`。
[PrimeNG 弹层取值](../../../../../src/DesignLanguages/PrimeNG/AtomUI.DesignLanguages.PrimeNG/README.md#overlays-popovers-select-panels-menus)

## 独立 Token 身份与共享计算

公开 TreeViewFlyoutPresenter 的 PopupCornerRadius 独立计算，和 Popup 仅共享 AbstractPopupCornerToken 的定义与取值公式（Ant Design：共享辅助类 `PopupCornerTokenCalculator`）；Controls[Popup] 的圆角不再改变这个公开 presenter。
