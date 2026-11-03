# ImagePreviewer Token 设计

本文档定义 ImagePreviewer 相关控件 Token 的专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。ImagePreviewer 整体架构见 [ImagePreviewer 桌面版架构设计](overview.md)，内部实现原理见 [ImagePreviewer 桌面版实现原理](implementation.md)，设计和契约变化记录见 [ImagePreviewer Changelog](changelog.md)。

## 1. 定位

ImagePreviewer Token 只表达组件级视觉变量，例如尺寸、间距、颜色、圆角、阴影、图标尺寸和弹层边界。Token 不承载运行时选择、展开、加载、错误、上传任务、过滤条件或业务状态。

当前 Token scope：

- `ImagePreviewerToken`，scope id 为 `ImagePreviewer`，源码位于 `src/AtomUI.Desktop.Controls/ImagePreviewer/ImagePreviewerToken.cs`。

## 2. Token 分类

Token 按控件语义分类维护：

| 分类 | 语义 | 代表 Token |
| --- | --- | --- |
| 尺寸与密度 | 控件高度、宽度、图标尺寸、内容最小尺寸。 | `PreviewOperationSize`、`ImagePreviewSwitchSize`、`DialogMinWidth`、`DialogMinHeight`、`CoverImageWidth` |
| 间距与布局 | padding、margin、gap、offset、popup content padding。 | `FloatToolbarPadding`、`FloatToolbarIndicatorPadding` |
| 颜色与状态视觉 | 文本、背景、边框、hover、selected、active、disabled 视觉。 | `PreviewOperationColor`、`PreviewOperationHoverColor`、`PreviewOperationColorDisabled`、`MaskBgColor`、`TitleBarBackgroundColor`、`NavButtonBgColor` |
| 结构与装饰 | 圆角、阴影、指示器、弹层和装饰线相关变量。 | `FloatToolbarIndicatorPadding` |

未出现在上表中的 Token 仍按源码中的组件语义维护，不按代码顺序机械分类。

## 3. 控件专项模型中的 Token 使用

ImagePreviewer 的控件专项模型通过 Theme 消费 Token：

- C# 控件负责状态归一和伪类同步。
- AXAML/ControlTheme 负责把 Token 映射到背景、前景、边框、padding、尺寸和动效。
- Token 默认值从 SharedToken 派生，不直接读取控件实例状态。
- Token 类型、生成数据和 token.md 应显式维护，不依赖运行时反射扫描。
- 默认 loading/error 占位复用 Skeleton、Spin、SharedToken 和 `CoverImageWidth` 的视觉语义，不为 `Loading`、`Failed` 或网络图片失败新增实例状态 Token。
- `CoverImageWidth` 只能作为没有显式 `CoverWidth` / `CoverHeight` 且没有有效布局约束时的封面占位尺寸兜底；它不表达图片自然尺寸，也不表达失败状态。

## 4. 控件家族影响

调整 ImagePreviewer Token 时必须评估以下范围：

- `ImagePreviewer`
- `ImageGroupPreviewer`
- `ImagePreviewerDialog`
- 对应 Gallery ShowCase 的示例和源码片段。
- Light/Dark 主题、Browser/Desktop 主题和 Compact/Form/Popup 集成场景。

## 5. 兼容性要求

- 不删除或重命名已生成的 TokenKind、TokenResource key 和 AXAML 引用。
- 不把实例状态、交互状态或 `EffectiveXxx` 状态写成 Token。
- 不在 Token 中展开颜色、variant 和状态的组合矩阵；组合关系应由 Theme selector 表达。
- 不为默认失败文案、加载中状态或远程图片失败状态新增 Token；文案走语言资源，状态走 internal `ImagePreviewEntry` 的
  `ImageLoadState`，视觉由主题和 SharedToken 表达。
- Token 默认值变更必须同步评估 Gallery 示例和截图可观察外观。
- 如需引入新 Token，必须同步 Token 类型、生成资源、主题引用和本文档。

## 6. 验证策略

| 改动类型 | 验证要求 |
| --- | --- |
| Token 文档 | `git diff --check`，检查相对链接存在。 |
| Token 默认值 | 运行对应控件测试，走查 Light/Dark 和 Browser 主题。 |
| Token 名称或数量 | 检查 generated TokenResource key、AXAML 引用和 token.md。 |
| 主题映射 | 走查 hover、pressed、selected、disabled、loading 等状态视觉。 |

## CT15 dependency and consumer checkpoint

All fourteen fields retain their names, types, keys, slots and four-algorithm defaults under V2/revision1. ImagePreviewSwitchSize is an optional explicit override for navigation Width/Height; unset preserves the former automatic dimensions and native values win. The other fields retain their existing cover, toolbar, navigation, title and window-bound roles. Real hover/disabled properties and both entry/host shapes passed focused checks; full-family rendering/lifecycle acceptance is still open. See [CT15 execution](../../../../superpowers/plans/2026-10-22-custom-theme/image-previewer-consumers.md).

| Own | Original default input | Actual consumer / update responsibility |
| --- | --- | --- |
| PreviewOperationSize | Global.FontSizeIcon * 1.5 | toolbar and navigation icon dimensions, live resources |
| PreviewOperationColor | Global.ColorTextLightSolid @ .65 | operation icons and footer foreground |
| PreviewOperationHoverColor | Global.ColorTextLightSolid @ .85 | operation hover selectors |
| PreviewOperationColorDisabled | Global.ColorTextLightSolid @ .25 | disabled operation/navigation selectors |
| ImagePreviewSwitchSize | Global.ControlHeightLG | explicit-only navigation Width/Height, 0 supported; unset auto |
| MaskBgColor | black @ .30 | cover mask paint |
| DialogMinWidth | 710 | native preview window minimum, existing sizing rules retained |
| DialogMinHeight | 240 | native preview window minimum |
| TitleBarBackgroundColor | Global.ColorBorderSecondary; Light darken(1) | native title frame and title bar background |
| NavButtonBgColor | Global.ColorBgMask @ .10 | navigation, Overlay close and footer capsule paint |
| NavButtonBgHoverColor | Global.ColorBgMask @ .20 | navigation and Overlay close hover |
| FloatToolbarPadding | Global.UniformlyPaddingLG,0 | footer action frame padding |
| FloatToolbarIndicatorPadding | Global.UniformlyPaddingXS,0 | footer index frame padding |
| CoverImageWidth | 200 | loading/error placeholder fallback; natural image sizing remains independent |

Global roles retain their original semantics: solid text/icon colour, tertiary error colour, spacing/padding/margins and title icon size. Motion collections keep their existing declared duration kinds; the existing TokenTransitions owner binding resolves through the public composition owner. TitleFontSize remains the WindowTitleBar role rather than being replaced by body size. All fourteen fields are consumer-verified at the current checkpoint; full family acceptance still requires the remaining evidence.

CT15 verification checkpoint: 142 focused checks, managed/AOT200 each; 104 intentional pairs pending limited review. Temporary sources/media/driver cleaned; final117 focused tests and NET8 zero warnings passed. 29 static and64 timed cross-runtime differences, OS/Browser/input/extreme boundaries and full-family acceptance remain open. See [results](../../../../superpowers/plans/2026-10-22-custom-theme/image-previewer-consumers-results.md).
