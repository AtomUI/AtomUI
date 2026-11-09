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
| 间距与布局 | padding、margin、gap、offset、popup content padding。 | `CardPadding`、`CardPaddingSM`、`CardPaddingLG`、`HorizontalItemMargin`、`HorizontalItemPadding`、`HorizontalMargin`（保留） |
| 颜色与状态视觉 | 文本、背景、边框、hover、selected、active、disabled 视觉。 | `CardBg`、`InkBarColor`、`ItemColor`、`ItemHoverColor`、`ItemSelectedColor` |
| 结构与装饰 | 圆角、阴影、指示器、弹层和装饰线相关变量。 | `MenuIndicatorPaddingHorizontal`、`MenuIndicatorPaddingVertical`、`MenuEdgeThickness`（保留）、`BoxShadowTabsOverflowLeft`、`BoxShadowTabsOverflowRight`、`BoxShadowTabsOverflowTop`、`BoxShadowTabsOverflowBottom` |

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
- `HorizontalMargin` 是保留 Token（`[ReservedToken]`，对应 antd Tabs `horizontalMargin`，antd 用它的底边作为页签栏与内容的间距）：这里该间距由
  `TabAndContentGutter` 表达，没有元素读取它。
- `MenuEdgeThickness` 是保留 Token（`[ReservedToken]`，antd 没有对应 Token，antd 以 `controlHeight` 作为阴影伪元素宽度）：start/end overflow
  shadow 载体位于 viewport 外侧，只有其外阴影进入页签区，载体厚度没有可见语义；主题以布局常量（32，需大于阴影负扩展）给出载体厚度。
- 四个 `BoxShadowTabsOverflow*` Token 使用普通外阴影并对齐 Ant Design Tabs 的方向参数：left 为 `(offsetX: 10, blur: 8, spread: -8)`，right 为 `(-10, 0, 8, -8)`，top 为 `(0, 10, 8, -8)`，bottom 为 `(0, -10, 8, -8)`，颜色统一为 8% 黑色。`Top` / `Bottom` placement 消费 left/right，`Left` / `Right` placement 消费 top/bottom；Theme 将透明载体放在 viewport 外侧，内侧面贴合溢出边界，并裁剪交叉轴及更多按钮方向的输出；这些参数保持逻辑 DIP；专用 edge renderer 仅在 Skia 绘制时补偿 offset 的变换顺序。必须连同载体几何、1×/2× 实际暗化强度验证，不能只断言属性值。
- edge indicator 的当前可见性、滚动 offset 和 placement 是控件实例状态，不得写入 Token；Token 只保存方向性装饰值。
- `InkBarThickness` 表达选中指示墨条的厚度，默认从 SharedToken 的 `LineWidthBold` 派生；按实例定制时通过控件 `Resources` 覆盖 `TabControlTokenKind.InkBarThickness`，与 `InkBarColor` 的定制方式一致。
  主题或 `Controls[TabControl]` 改变厚度时墨条立即重新布局（此前要等下一次选择或尺寸变化）；卡片圆角（按尺寸取 `BorderRadius*`）变化时卡片页签同样立即重算。

## 3.1 Token 读取与作用域

- TabControl 的 Own Token 由全部页签主题以 `TabControlTokenResource` 读取：`Controls[TabControl]` 的 Own 覆盖（以及派生它们的全局覆盖）
  同时作用于 CardTabControl、TabStrip、CardTabStrip（抽象 Token 层由 Task 10.10 处理）。
- 各控件自己读取的全局 Token 以自己的 TokenResource 读取：`TabControl`、`CardTabControl`、`TabStrip`、`CardTabStrip`（页头间距、卡片圆角、
  卡片边框与添加按钮）、`TabItem` / `TabStripItem`（图标尺寸、按下/禁用文字色、卡片背景与边框）。
- 无身份的共享部件 `BaseTabControl` / `BaseTabStrip` 基础主题、`TabScrollViewer` 与溢出菜单以 `OwnerTokenResource` 读取宿主的有效全局
  Token；溢出菜单所在 Popup 为 Token owner 透明，`Controls[TabStrip]` 等作用于其溢出菜单。溢出按钮与菜单项关闭按钮是注册的 IconButton，
  由无身份部件读取宿主值后向下绑定。
- 页签项使用命名主题（`TabItemTheme` / `CardTabItemTheme` / `TabStripItemTheme` / `CardTabStripItemTheme`），从页签项自身所在资源作用域查找，
  局部资源中的同名主题优先于应用资源；命名主题不注入 owner，页签项继承宿主为 owner（过渡时长随宿主）。

## 4. 控件家族影响

调整 TabControl Token 时必须评估以下范围：

- `TabControl`
- `CardTabControl`
- `TabStrip`
- `CardTabStrip`
- `TabItem`
- `TabStripItem`
- `BaseTabControl`
- `BaseTabStrip`
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
| 任意 Token | `NavigationFamilyEffectTests`（TabControl / CardTabControl / TabStrip / CardTabStrip / TabItem / TabStripItem 行与隔离测试）全绿。 |
| Overflow shadow | 覆盖四个 owner × 四个 placement，断言普通外阴影的方向、offset、blur、spread、颜色、尺寸及 start/end 可见性；同时验证透明绘制载体与viewport 阴影层裁剪边界与真实合成像素。 |
