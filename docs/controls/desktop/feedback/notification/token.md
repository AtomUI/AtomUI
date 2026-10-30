# Notification Token 设计

本文档定义 Notification 相关控件 Token 的专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。Notification 整体架构见 [Notification 桌面版架构设计](overview.md)，内部实现原理见 [Notification 桌面版实现原理](implementation.md)，设计和契约变化记录见 [Notification Changelog](changelog.md)。

## 1. 定位

Notification Token 只表达组件级视觉变量，例如尺寸、间距、颜色、圆角、阴影、图标尺寸和弹层边界。Token 不承载运行时选择、展开、加载、错误、上传任务、过滤条件或业务状态。

当前 Token scope：

- internal `NotificationCardToken`，scope id 为 `NotificationCard`，源码位于 `src/AtomUI.Desktop.Controls/Notifications/NotificationCardToken.cs`。

## 2. Token 分类

Token 按控件语义分类维护：

| 分类 | 语义 | 代表 Token |
| --- | --- | --- |
| 尺寸与密度 | 控件高度、宽度、图标尺寸、内容最小尺寸。 | `NotificationIconSize`、`NotificationCloseButtonSize`、`NotificationProgressHeight`、`NotificationWidth` |
| 间距与布局 | padding、margin、gap、offset、popup content padding。 | `NotificationPadding`、`NotificationIconMargin`、`NotificationSectionSpacing`、`NotificationTitlePadding`、`NotificationActionsMargin`、`NotificationCloseButtonPadding`、`NotificationCloseButtonMargin`、`NotificationProgressMargin`、`NotificationTopMargin`、`NotificationBottomMargin` |
| 颜色与状态视觉 | 文本、背景、边框、hover、selected、active、disabled 视觉。 | `NotificationBg`、`NotificationProgressBg`、`NotificationProgressTrackBg` |
| 结构与装饰 | 圆角、阴影、指示器、弹层和装饰线相关变量。 | 按源码 Token 语义维护 |

未出现在上表中的 Token 仍按源码中的组件语义维护，不按代码顺序机械分类。

## 3. 控件专项模型中的 Token 使用

Notification 的控件专项模型通过 Theme 消费 Token：

- C# 控件负责状态归一和伪类同步。
- AXAML/ControlTheme 负责把 Token 映射到背景、前景、边框、padding、尺寸和动效。
- Token 默认值从 SharedToken 派生，不直接读取控件实例状态。
- Token 类型、生成数据和 token.md 应显式维护，不依赖运行时反射扫描。

`NotificationProgressBg` 使用主色边框 hover 色到主色的线性渐变。`NotificationCloseButtonSize` 控制关闭按钮的固定命中尺寸；
hover 和 pressed 背景由 SharedToken 的文本背景状态色在主题 selector 中表达，不引入额外控件 Token。

卡片间距 Token 直接取全局 Token 的对应值：

- `NotificationPadding`：卡片内边距，上下左右对称，垂直 `paddingMD`、水平 `paddingLG`
  （上游 `padding: paddingMD paddingLG`）。
- `NotificationIconMargin`：`wrapper` 内 icon 与 section 的间距，右 `marginSM`（上游 `gap: marginSM`）。
- `NotificationSectionSpacing`：`section` 内标题与描述的间距 `marginXS`（上游 `gap: marginXS`）。
- `NotificationTitlePadding`：标题的右侧留白 `paddingLG`，为覆盖层 `close` 预留空间（上游 `.notice-closable` 的
  `padding-inline-end`）。描述不与关闭按钮同排，按上游 `.notice-title + .notice-description` 不预留。
- `NotificationActionsMargin`：`actions` 区域的上边距 `marginSM`（上游 `margin-top: marginSM`）。
- `NotificationCloseButtonMargin`：`close` 覆盖层偏移，上 `paddingMD`、右 `paddingLG`
  （上游 `top: notificationPaddingVertical`、`inset-inline-end: notificationPaddingHorizontal`）。
- `NotificationProgressMargin`：`progress` 覆盖层左右内缩 `BorderRadiusLG`（上游 `left/right: borderRadiusLG`）。

`NotificationProgressTrackBg`（`ColorFillQuaternary`）是 progress 的底槽色，对齐上游
`&, &::-webkit-progress-bar { background-color: rgba(0, 0, 0, 0.04) }`；`NotificationProgressBg` 是叠加在其上的
彩色进度值。两者缺一都会让进度视觉不完整。

卡片自身不承担列表边缘间隔。`WindowNotificationManager.Padding` 的默认值按 Position 从
`NotificationTopMargin` / `NotificationBottomMargin` 选择：顶部为 `(marginLG, marginLG, marginLG, 0)`，
底部为 `(marginLG, 0, marginLG, marginLG)`。模板把 Padding 绑定到 presenter.Margin，应用的显式 Padding
可覆盖默认值。`NotificationPadding` 只控制卡片 `ContentBox` 内边距。

Stack 的展开 gap 16 与折叠 offset 8 来自 SharedToken 的统一 margin 尺度，由内部 presenter/panel 管理，
不作为 `ItemsControl` 类型的 `listContent` 的公开 Setter。三层 scale 是共享布局行为，不属于
`NotificationCardToken`。Stack 状态、层索引、可见项数和剩余时长均是运行时状态，不能写入 Token。

## 4. 控件家族影响

调整 Notification Token 时必须评估以下范围：

- `Notification`
- `NotificationCard`
- `WindowNotificationManager`
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
