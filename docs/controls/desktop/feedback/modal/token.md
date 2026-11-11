# Modal Token 设计

本文档定义 Modal 相关控件 Token 的专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。Modal 整体架构见 [Modal 桌面版架构设计](overview.md)，内部实现原理见 [Modal 桌面版实现原理](implementation.md)，设计和契约变化记录见 [Modal Changelog](changelog.md)。

## 1. 定位

Modal Token 只表达组件级视觉变量，例如尺寸、间距、颜色、圆角、阴影、图标尺寸和弹层边界。Token 不承载运行时选择、展开、加载、错误、上传任务、过滤条件或业务状态。

当前 Token scope：

- `DialogToken`，scope id 为 `Dialog`，源码位于 `src/AtomUI.Desktop.Controls/Dialog/DialogToken.cs`。

## 2. Token 分类

Token 按控件语义分类维护：

| 分类 | 语义 | 代表 Token |
| --- | --- | --- |
| 尺寸与密度 | 正文 viewport 基线、控件高度、宽度和图标尺寸。 | `MinHeight`、`MinWidth`、`LogoSize`、`HeaderFontSize`、`CloseBtnSize` |
| 间距与布局 | padding、margin、gap、offset、popup content padding。 | `HeaderMarginBottom`、`HeaderPadding`、`ContentPadding`、`WindowContentPadding`、`FooterPadding`、`FooterMarginTop`、`LoadingIndicatorMargin` |
| 颜色与状态视觉 | 文本、背景、边框、hover、selected、active、disabled 视觉。 | `HeaderBg`、`HeaderColor`、`ContentBg`、`FooterBg` |
| 结构与装饰 | 圆角、阴影、指示器、弹层和装饰线相关变量。 | `LoadingIndicatorMargin` |

未出现在上表中的 Token 仍按源码中的组件语义维护，不按代码顺序机械分类。

## 3. 控件专项模型中的 Token 使用

Modal 的控件专项模型通过 Theme 消费 Token：

- C# 控件负责状态归一和伪类同步。
- AXAML/ControlTheme 负责把 Token 映射到背景、前景、边框、padding、尺寸和动效。
- Token 默认值从 SharedToken 派生，不直接读取控件实例状态。
- `DialogToken.MinWidth/MinHeight` 定义结构性最小尺寸中的正文 viewport 基线（框尺寸 = Token + 正文内边距 + 边框，默认 200 + 48 = 248），不直接等同于最终 Surface minimum。最终下限还需要组合可见 Header、Footer、调用方 `HostMin*` 与 host capacity；完整算法见 [Modal 宿主尺寸与 Resize 设计](host-sizing-design.md)。
- `MessageBoxToken.MinWidth/MinHeight` 的语义不同：它们是整框尺寸（框尺寸 = max(Token, 正文内边距 + 边框)，默认 410，与 antd ConfirmDialog 的整框 `width` 一致），由 `MessageBox` 的 `UsesWholeFrameMinimum` 选择；两个终端各用自己的语义。
- `WindowContentPadding` 仅由 Window 宿主的正文样式消费：原生标题栏替代 Overlay Header 后，为正文补齐上下留白，同时保持左右 `ContentPadding` 的密度。
- `MessageBoxToken.MinWidth` 可以提高 MessageBox 的有效最小宽度，但不能降低 Dialog 的结构性最小高度。
- Token 类型、生成数据和 token.md 应显式维护，不依赖运行时反射扫描。

### 主题读取

- `Dialog`、`MessageBox`、`DialogButtonBox` 是注册控件，主题以各自的 `XxxTokenResource` 读取全局 Token（动效开关等）。
- Surface 部件（`DialogSurface`、`OverlayDialogHeader`、`DialogCaptionButton`、`OverlayDialogMask`、`OverlayDialogPresenter`）
  没有 Token 身份，Overlay presenter 以 Dialog 为继承父级，部件继承 Dialog（或 MessageBox）为 Token owner，以
  `OwnerTokenResource` 读取全局 Token（`BorderRadiusLG`、`BoxShadows`、`FontWeightStrong`、`MotionDurationMid/Slow`、
  `ColorBgMask`、标题栏按钮的 `ColorIcon` / `ColorBgText*` / `BorderRadiusSM`、`IconSize`、`SpacingSM`），
  `Controls[Dialog]` 与 `Controls[MessageBox]` 分别作用于各自的 surface。共享 AbstractDialogToken 的 Own 由 Dialog / MessageBox 各自终端主题读入具体 surface 输入。
- `MessageBoxContent` 只属于 MessageBox，以 `MessageBoxTokenResource` 显式读取状态色与图标间距。对话框正文位于 surface 的
  `Skeleton` 中；Skeleton 是 Token owner 透明控件，正文内容继承 Dialog（或 MessageBox）为 owner。
- 最大化的 Overlay surface 通过 `^[IsDialogMaximized=True]` 状态 Setter 去掉圆角，不再由 presenter 写本地值；恢复后回到主题圆角。
- `OverlayDialogResizer` 的 12 处尺寸字面量是透明拖拽热区的布局常量，标注 `<!-- layout -->`。
- 主题包定制（PrimeNG 预设）：Dialog 圆角经 `Controls[Dialog].BorderRadiusLG`，正文内边距 `ContentPadding`，标题字号
  `HeaderFontSize`，标题字重 `FontWeightStrong`，均有生效测试。

MessageBox 具有独立的完整 surface Own，MinWidth 默认410 且为整框语义（Dialog 默认200 为正文 viewport 基线，框默认 248），StyleIconSize 为 MessageBox 专属。Window/Overlay surface 都显式跟随真实 Dialog/MessageBox owner；原生窗口装饰继续使用 Window 身份。

## 4. 控件家族影响

调整 Modal Token 时必须评估以下范围：

- `Dialog`
- `DialogOptions`
- `DialogButtonBox`
- 对应 Gallery ShowCase 的示例和源码片段。
- Light/Dark 主题、Browser/Desktop 主题和 Compact/Form/Popup 集成场景。

## 5. 兼容性要求

- 不删除或重命名已生成的 TokenKind、TokenResource key 和 AXAML 引用。
- 不把实例状态、交互状态或 `EffectiveXxx` 状态写成 Token。
- 不在 Token 中展开颜色、variant 和状态的组合矩阵；组合关系应由 Theme selector 表达。
- Token 默认值变更必须同步评估 Gallery 示例和截图可观察外观。
- 调用方 `HostMin*` 是实例尺寸请求，不是 Token，也不能把 effective minimum 降低到 `DialogToken.MinWidth/MinHeight` 正文 viewport 基线以下。
- 如需引入新 Token，必须同步 Token 类型、生成资源、主题引用和本文档。

## 6. 验证策略

| 改动类型 | 验证要求 |
| --- | --- |
| Token 文档 | `git diff --check`，检查相对链接存在。 |
| Token 默认值 | 运行对应控件测试，走查 Light/Dark 和 Browser 主题。 |
| Token 名称或数量 | 检查 generated TokenResource key、AXAML 引用和 token.md。 |
| 主题映射 | 走查 hover、pressed、selected、disabled、loading 等状态视觉。 |

## 独立 Token 身份与共享计算

原 `Controls[Dialog]` 配置不再改变 MessageBox。窗口 surface 通过有释放路径的现有 owner forwarding 连接真实 dialog；Own 样式输入沿逻辑父链跨根传播，关闭/释放 presenter 时清理绑定。
