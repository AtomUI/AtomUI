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
| 间距与布局 | padding、margin、gap、offset、popup content padding。 | `FloatOffsetX`、`FloatOffsetY` |
| 颜色与状态视觉 | 文本、背景、边框、hover、selected、active、disabled 视觉。 | `PrimaryColor` |
| 结构与装饰 | 圆角、阴影、指示器、弹层和装饰线相关变量。 | `FloatButtonShadows`、`DescriptionLineHeight` |
| 颜色与状态视觉 | 文本、背景、边框、hover、selected、active、disabled 视觉。 | `DefaultBg`、`DefaultHoverBg`、`DefaultActiveBg`、`DefaultIconColor`、`PrimaryColor` |

未出现在上表中的 Token 仍按源码中的组件语义维护，不按代码顺序机械分类。

## 3. 控件专项模型中的 Token 使用

FloatButton 的控件专项模型通过 Theme 消费 Token：

- C# 控件负责状态归一和伪类同步。
- AXAML/ControlTheme 负责把 Token 映射到背景、前景、边框、padding、尺寸和动效。
- Token 默认值从 SharedToken 派生，不直接读取控件实例状态。
- Token 类型、生成数据和 token.md 应显式维护，不依赖运行时反射扫描。
- BackTop 进度环从所属 BackTop 作用域成组读取三个 Effective Global Token。线宽取 `LineWidthBold`，轨道色取 `ColorBorderSecondary`，进度色取 `ColorPrimary`。

徽标位置由 `BadgeOffset` 属性的按形状默认值表达。原 `SquareBadgeOffset`、`CircleBadgeOffset` 两个 Token 没有任何消费者，
已于 2026-11-08 删除（见仓库 `CHANGELOG.md` 的 Breaking Changes）。

`AbstractFloatButtonTheme` 由 FloatButton 与 BackTopFloatButton 共用，其中的 Global Token 经 `OwnerTokenResource` 读取实际
公开 owner 的有效值；图标颜色跟随按钮 `Foreground`，因为 `IconPresenter` 本身是带 Token 身份的公开控件，在它上面读取
`OwnerTokenResource` 会落到 IconPresenter 自己的身份。

浮动按钮族的阴影由自有 Token `FloatButtonShadows` 表达（默认取全局 `BoxShadowsSecondary`，Ant Design 渲染不变）：
`AbstractFloatButtonTheme` 的独立按钮与嵌入圆钮、`FloatButtonGroupTheme` 与 `FloatButtonGroupHostTheme` 的方形容器均读该
Token，设计语言算法（如 `AtomUI.Themes.PrimeNG` 的 `PrimeNGFloatButtonTokens`）可按语言覆盖，PrimeNG 下无阴影。

默认（Default）类型的表面与前景同样由自有槽位表达：`DefaultBg` 画在底层 `BackgroundFrame`，`DefaultHoverBg` /
`DefaultActiveBg` 由 `:pointerover` / `:pressed` 样式块覆盖上层 `Frame`，`DefaultIconColor` 是按钮 `Foreground`（图标
跟随）。Ant Design 默认值分别为 `ColorBgElevated` / `ColorBgTextHover` / 透明 / `ColorText`，渲染与引入槽位前一致；
FloatButton 族的其它身份（BackTop、Group、Host）主题经 `FloatButtonTokenResource` 读同一组槽位，因此家族表面随语言
统一：方形胶囊容器（`FloatButtonItemsControl` 的 `[Shape=Square]`）的填充也读 `DefaultBg`。PrimeNG 下这组槽位取官方
`button.secondary`：官方没有“白色无阴影”按钮，白底默认会与白卡片融为一体；容器同理，去掉阴影后必须由填充表达表面。

## 4. 控件家族影响

Shadcn 的 Default 取同风格普通 Button 的描边和悬停配色。背景叠在 `ColorBgElevated` 上，覆盖浮动按钮后面的滚动内容。
描边读取 FloatButton owner 的 `ColorBorder` 与 `BorderThickness`。独立 FloatButton、BackTop 和组内圆钮各自绘制描边，
方形组通过容器绘制一个外框，组内按钮仍由现有分隔线区分。

Vega 将 `shadow-xs` 作为描边默认阴影。Primary 仍响应调用方显式提供的 `FloatButtonShadows`，包括与 Vega 默认值相同的覆盖。

Shadcn 的独立方形按钮有说明时按内容测量，以常规 `ControlHeight` 为最小尺寸，纯图标按钮使用 `FloatButtonSize`。说明布局读取该 owner 的字号、`PaddingLG` 和 `SpacingXS`，默认左右留白不超过 10 DIP，上下各 2 DIP。图标和文字横排，默认文本在可用宽度内显示至多两行，默认宽度上限为最小宽度的 2.5 倍。Luma、Maia 的带文字轮廓按实际高度形成胶囊，纯图标方钮的圆角由较小尺寸约束。显式尺寸、圆角及模板仍通过相同入口应用。

扩展排版的默认行高跟随风格字号。应用设置的 `DescriptionLineHeight` 优先，嵌入组内按钮继续使用原行高令牌并由组维护分隔。

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
