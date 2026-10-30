# TabControl Token 设计

本文档定义 TabControl 相关控件 Token 的专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。TabControl 整体架构见 [TabControl 桌面版架构设计](overview.md)，内部实现原理见 [TabControl 桌面版实现原理](implementation.md)，设计和契约变化记录见 [TabControl Changelog](changelog.md)。

## 1. 定位

TabControl Token 只表达组件级视觉变量，例如尺寸、间距、颜色、圆角、阴影、图标尺寸和弹层边界。Token 不承载运行时选择、展开、加载、错误、上传任务、过滤条件或业务状态。

当前 Token scope：

- `TabControlToken`，scope id 为 `TabControl`，源码位于 `src/AtomUI.Desktop.Controls/TabControl/TabControlToken.cs`。

## 2. Token 分类

Token 按控件语义分类维护：

| 分类 | 语义 | 代表 Token |
| --- | --- | --- |
| 尺寸与密度 | 控件高度、宽度、图标尺寸、内容最小尺寸。 | `CardSize`、`TitleFontSize`、`TitleFontSizeLG`、`TitleFontSizeSM`、`InkBarThickness` |
| 间距与布局 | padding、margin、gap、offset、popup content padding。 | `CardPadding`、`CardPaddingSM`、`CardPaddingLG`、`HorizontalMargin`、`HorizontalItemMargin`、`HorizontalItemPadding` |
| 颜色与状态视觉 | 文本、背景、边框、hover、selected、active、disabled 视觉。 | `CardBg`、`InkBarColor`、`ItemColor`、`ItemHoverColor`、`ItemSelectedColor` |
| 结构与装饰 | 圆角、阴影、指示器、弹层和装饰线相关变量。 | `MenuIndicatorPaddingHorizontal`、`MenuIndicatorPaddingVertical`、`MenuEdgeThickness`、`BoxShadowTabsOverflowLeft`、`BoxShadowTabsOverflowRight`、`BoxShadowTabsOverflowTop`、`BoxShadowTabsOverflowBottom` |

未出现在上表中的 Token 仍按源码中的组件语义维护，不按代码顺序机械分类。

## 3. 控件专项模型中的 Token 使用

TabControl 的控件专项模型通过 Theme 消费 Token：

- C# 控件负责状态归一和伪类同步。
- AXAML/ControlTheme 负责把 Token 映射到背景、前景、边框、padding、尺寸和动效。
- Token 默认值从 SharedToken 派生，不直接读取控件实例状态。
- Token 类型、生成数据和 token.md 应显式维护，不依赖运行时反射扫描。
- `VerticalItemGutter` 表达默认 Line Tab 在 `Left` / `Right` placement 下的紧凑相邻间距；Card Tab 不消费该 Token，而继续使用 `CardGutter` 维持独立卡片节奏。
- `VerticalItemPadding` 保留给 Card Tab 的垂直 placement padding 语义；默认 Line Tab 的 `Left` / `Right` padding 由默认 Line theme 内部保持紧凑，不新增公开 Token。
- overflow Popup shell、默认菜单与 Gallery 搜索模板继续复用 Popup、Menu、SharedToken 和输入控件主题；`OverflowPopupTemplate` 只改变内容模板，不创建实例级 Token 状态。
- `MenuEdgeThickness` 表达 start/end overflow shadow 载体在主轴上的厚度，默认派生自 `EffectiveGlobalToken.ControlHeight`。载体位于 viewport 外侧，此值不占用页签宽度，也不决定阴影向内容区渐淡的深度。
- 四个 `BoxShadowTabsOverflow*` Token 使用普通外阴影并对齐 Ant Design Tabs 的方向参数：left 为 `(offsetX: 10, blur: 8, spread: -8)`，right 为 `(-10, 0, 8, -8)`，top 为 `(0, 10, 8, -8)`，bottom 为 `(0, -10, 8, -8)`，颜色统一为 8% 黑色。`Top` / `Bottom` placement 消费 left/right，`Left` / `Right` placement 消费 top/bottom；Theme 将透明载体放在 viewport 外侧，内侧面贴合溢出边界，并裁剪交叉轴及更多按钮方向的输出；这些参数保持逻辑 DIP；专用 edge renderer 仅在 Skia 绘制时补偿 offset 的变换顺序。必须连同载体几何、1×/2× 实际暗化强度验证，不能只断言属性值。
- edge indicator 的当前可见性、滚动 offset 和 placement 是控件实例状态，不得写入 Token；Token 只保存方向性装饰值。
- `InkBarThickness` 表达选中指示墨条的厚度，默认从 SharedToken 的 `LineWidthBold` 派生；按实例定制时通过控件 `Resources` 覆盖 `TabControlTokenKind.InkBarThickness`，与 `InkBarColor` 的定制方式一致。

## 4. 控件家族影响

调整 TabControl Token 时必须评估以下范围：

- `TabControl`
- `CardTabControl`
- `TabStrip`
- `CardTabStrip`
- `TabItem`
- `BaseTabControl`
- 对应 Gallery ShowCase 的示例和源码片段。
- Light/Dark 主题、Browser/Desktop 主题和 Compact/Form/Popup 集成场景。

## 5. 兼容性要求

- 不删除或重命名已生成的 TokenKind、TokenResource key 和 AXAML 引用。
- 不把实例状态、交互状态或 `EffectiveXxx` 状态写成 Token。
- 不在 Token 中展开颜色、variant 和状态的组合矩阵；组合关系应由 Theme selector 表达。
- Token 默认值变更必须同步评估 Gallery 示例和截图可观察外观。
- 如需引入新 Token，必须同步 Token 类型、生成资源、主题引用和本文档。
- 四向 overflow shadow Token 属于 TabControl family；`TabControl`、`CardTabControl`、`TabStrip` 与 `CardTabStrip` 必须通过共享 `TabScrollViewerTheme` 消费同一组值。

## 6. 验证策略

| 改动类型 | 验证要求 |
| --- | --- |
| Token 文档 | `git diff --check`，检查相对链接存在。 |
| Token 默认值 | 运行对应控件测试，走查 Light/Dark 和 Browser 主题。 |
| Token 名称或数量 | 检查 generated TokenResource key、AXAML 引用和 token.md。 |
| 主题映射 | 走查 hover、pressed、selected、disabled、loading 等状态视觉。 |
| Overflow shadow | 覆盖四个 owner × 四个 placement，断言普通外阴影的方向、offset、blur、spread、颜色、尺寸及 start/end 可见性；同时验证透明绘制载体与viewport 阴影层裁剪边界与真实合成像素。 |
