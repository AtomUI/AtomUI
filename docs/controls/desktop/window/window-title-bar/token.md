# WindowTitleBar Token 设计

本文档定义 `WindowTitleBarToken` 的组件视觉语义、Theme 消费范围和兼容边界。控件设计见 [WindowTitleBar 控件设计](overview.md)，实现数据流与标题几何见 [WindowTitleBar 实现原理](implementation.md)，契约变化见 [WindowTitleBar Changelog](changelog.md)，通用规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。

## 1. 定位

`WindowTitleBarToken` 是 scope id 为 `WindowTitleBar` 的 internal control token，源码位于 `src/AtomUI.Desktop.Controls/WindowTitleBar/WindowTitleBarToken.cs`。它从 `SharedToken` 计算标题栏和 caption button 的视觉变量，并通过生成的 `WindowTitleBarTokenResource` key 供 AXAML 使用。

Token 负责尺寸、间距、字体和状态颜色，不负责以下运行时语义：

- `TitleAlignment`、Leading/Title/Trailing 角色和布局公式。
- CSD、native chrome insets、WindowState 和 backend 能力。
- Logo、标题、add-on 或 caption button 的有效可见性。
- pointer capture、拖动、checked state 和窗口操作。

## 2. Token 分类

### 2.1 标题栏结构

| Token | 语义 | 主要消费者 |
| --- | --- | --- |
| `Height` | 常规标题栏高度。 | `WindowTitleBarTheme` |
| `TitleBarPadding` | 标题栏 managed 内容的左右内边距。 | `WindowTitleBarTheme`、标题布局 Panel |
| `HeaderHorizontalSpacing` | 非零 Leading/Trailing 操作区与 Title 之间的条件水平间距。 | WindowTitleBar、ImagePreviewer、全屏标题宿主 |
| `LogoAndLeftAddOnSpacing` | Windows/Linux Leading role 内同时有效的 Logo 与 LeftAddOn 之间的条件间距。 | WindowTitleBar Windows/Linux 模板 |
| `LogoAndTitleSpacing` | Title role 内同时可见的 Logo/Icon 与 Title 之间的条件间距。 | WindowTitleBar macOS 模板、ImagePreviewer、全屏标题宿主 |
| `LogoSize` | 默认 Logo presenter 尺寸。 | WindowTitleBar 和全屏标题宿主 |
| `TitleFontSize` | 标题字体尺寸。 | WindowTitleBar 和全屏标题宿主 |
| `TitleFontWeight` | 标题字体粗细。 | WindowTitleBar 和全屏标题宿主 |

`TitleBarPadding` 是原生安全边界后的 managed 内容间距；没有 native chrome inset 时从 frame 边缘起效。标题布局先应用 native chrome extent，再应用 `TitleBarPadding`，两段占位各自只计算一次。`Height`、`FullscreenCaptionButtonSize` 和 `HeaderHorizontalSpacing` 使用跨平台稳定值，不随紧凑密度算法缩小。`LogoAndLeftAddOnSpacing` 默认取当前 WindowTitleBar scope 的 `SpacingXXS`，默认主题下为 4 个逻辑像素。

`HeaderHorizontalSpacing` 不属于 add-on 固定 margin。对应操作区实测宽度为零时不应用该值；add-on 自身 margin 已由 DesiredSize 计入，不与该 Token 重复计算。`LogoAndLeftAddOnSpacing` 由 Windows/Linux Leading 容器消费，只在 `PART_Logo` 与 `PART_LeftAddOn` 两个 presenter 同时可见时出现；它不通过 `PART_LeftAddOn.Margin` 实现，也不作用于 macOS。`LogoAndTitleSpacing` 只作用于图标和标题同在 Title role 的宿主，不为隐藏或空 Logo/Icon、空 Title 保留占位。

### 2.2 Caption button 尺寸

| Token | 语义 | 主要消费者 |
| --- | --- | --- |
| `CaptionButtonIconSize` | 通用 caption action icon 尺寸。 | Linux/macOS buttons、Windows extended actions、全屏 buttons、WindowTitleBar AddOn buttons |
| `WindowsCaptionIconSize` | Windows minimize/maximize/close glyph 尺寸。 | `WindowsCaptionButton` |
| `CaptionButtonPadding` | managed caption button 内容 padding；Windows AddOn 的外部命中面仍由标题栏可用高度决定。 | `CaptionButtonTheme`、WindowTitleBar AddOn button themes |
| `CaptionButtonCornerRadius` | managed caption button 的正圆圆角，由 `CaptionButtonIconSize` 与 `CaptionButtonPadding` 推导。 | Linux/macOS WindowTitleBar AddOn button themes |
| `CaptionButtonBackgroundInset` | managed caption button 背景相对布局盒的内缩；Linux managed caption band 用 `SizeUnit / 2` 把 30 逻辑像素命中面收成 26 逻辑像素圆形背景。 | Linux `CaptionButtonGroupTheme`、Linux WindowTitleBar AddOn button themes、`CaptionButtonFrameTheme` |
| `CaptionGroupSpacing` | 非 Windows caption buttons、全屏操作和非 Windows AddOn button group 的可选语义间距。 | CaptionButtonGroup、全屏标题宿主、Linux/macOS WindowTitleBar Trailing 容器、应用 AddOn 容器 |
| `FullscreenCaptionButtonSize` | 全屏标题宿主中 caption button 的固定尺寸。 | Window drawn decorations、fullscreen popover |

Windows 原生风格 caption buttons 连续贴合排列，group spacing 为零；Windows AddOn 需要连续 hover 按钮带时，其应用容器同样设置零 spacing。Linux 与全屏操作可使用 `CaptionGroupSpacing`；Linux/macOS 标题栏在 Trailing 容器上固定使用该 Token 分隔 AddOn 操作区与 caption group，使圆形按钮之间保持同一语义间距，没有 AddOn 内容时不产生幽灵间距。`LeftAddOn` / `RightAddOn` 允许任意内容，因此容器内部 spacing 始终由应用所有，Theme 不强制覆盖。`CaptionButtonBackgroundInset` 属于 Theme 状态视觉，只内缩背景层，不改变按钮布局占用尺寸和命中面。

`CaptionButtonCornerRadius` 与 `CaptionButton` 按实测尺寸推导的圆角同源：内容尺寸固定为 icon + padding，默认主题下 30×30 得到 15。该 Token 在 Linux/macOS AddOn theme 生效，让 `WindowTitleBarButton` 与 `WindowTitleBarToggleButton` 和相邻系统 caption 按钮共用同一圆角来源；`CaptionButton` 自身保留实测推导，以覆盖全屏标题宿主对按钮尺寸的覆盖。`CaptionButtonBackgroundInset` 同时被 Linux `CaptionButtonGroupTheme` 与 Linux AddOn theme 消费，是 26 逻辑像素圆形背景的唯一来源；两个家族都通过 `CaptionButtonFrame` 把这个值应用到共享的背景层。Windows AddOn 维持直角铺满标题栏的方形交互面，不消费这两个 Token。

### 2.3 Active 与 inactive 视觉

| Token | 语义 |
| --- | --- |
| `ActiveColor` | 激活窗口的标题与 caption icon 颜色。 |
| `InactiveColor` | 非激活窗口的标题与 caption icon 颜色。 |
| `ActiveBgColor` | 通用 caption button 默认背景。 |
| `ActiveHoverBgColor` | 激活窗口中 caption button hover 背景。 |
| `ActivePressedBgColor` | caption button pressed 背景。 |
| `InactiveBgColor` | 非激活窗口中通用 caption button 背景。 |
| `InactiveHoverBgColor` | 非激活窗口中 caption button hover 背景。 |

active/inactive 是控件实例状态。Token 只提供对应颜色，实际选择由 `IsWindowActive`、`:pointerover`、`:pressed` 和 Theme selector 完成。

### 2.4 Windows close button

| Token | 语义 |
| --- | --- |
| `WindowsCloseButtonHoverColor` | Windows close button hover/pressed glyph 颜色。 |
| `WindowsCloseButtonHoverBgColor` | Windows close button hover 背景。 |
| `WindowsCloseButtonPressedBgColor` | Windows close button pressed 背景。 |

关闭按钮危险态只应用于 `WindowsCaptionButton.IsCloseButton=True`。其他 caption action 不消费该组颜色。

### 2.5 通用兼容颜色

| Token | 语义 |
| --- | --- |
| `ForegroundColor` | 通用标题栏前景色。 |
| `HoverBackgroundColor` | 通用文本操作 hover 背景。 |
| `PressedBackgroundColor` | 通用文本操作 pressed 背景。 |
| `CloseHoverBackgroundColor` | 通用 close action hover 背景。 |
| `ClosePressedBackgroundColor` | 通用 close action pressed 背景。 |

这些变量属于生成的 WindowTitleBar Token 契约。内置 caption themes 使用更精确的 active/inactive 与 Windows close 变量；通用兼容颜色仍保持可生成、可覆盖和不可随意重命名。

## 3. 控件专项模型中的 Token 使用

```text
SharedToken
  -> WindowTitleBarToken.CalculateTokenValues(isDarkMode)
  -> generated WindowTitleBarTokenKind / WindowTitleBarTokenResource
  -> WindowTitleBar, CaptionButton, Window and ImagePreviewer Themes
```

主要消费范围：

| Theme | 消费职责 |
| --- | --- |
| `WindowTitleBarTheme.axaml` | 标题栏高度、Padding、标题字体、Logo、active/inactive 前景、Windows/Linux Logo/LeftAddOn 间距和标题内容间距。 |
| `CaptionButtonGroupTheme.axaml` | 平台按钮 icon size 与 group spacing。 |
| `CaptionButtonTheme.axaml` | 系统通用按钮 padding、背景、active/inactive、hover、pressed 和 motion。 |
| `WindowTitleBarButtonTheme.axaml` | AddOn 普通图标按钮的尺寸、背景、active/inactive、hover、pressed、disabled 和 motion；Windows 分支使用方形直角交互面。 |
| `WindowTitleBarToggleButtonTheme.axaml` | AddOn checked/unchecked 图标按钮的尺寸、背景、active/inactive、hover、pressed、disabled 和 motion；Windows 分支使用方形直角交互面。 |
| `WindowsCaptionButtonTheme.axaml` | Windows glyph、hover/pressed 以及 close danger state。 |
| `WindowDrawnDecorationsTheme.axaml` | 全屏标题、Logo 和 caption operations。 |
| `FullscreenPopoverLayerTheme.axaml` | 非 CSD 全屏弹出标题栏。 |
| `ImagePreviewerTitleBarTheme.axaml` | 预览图标/标题间距和操作区间距；背景由 ImagePreviewer Token 覆盖。 |

标题栏默认背景来自 SharedToken `ColorBgContainer`；ImagePreviewer 标题栏背景来自 `ImagePreviewerToken.TitleBarBackgroundColor`。背景 ownership 不改变 WindowTitleBar 的内容与 caption Token scope。

## 4. 控件家族影响

调整 WindowTitleBar Token 时必须同时检查：

- 默认 `WindowTitleBar` 的 macOS、Windows 和 Linux 模板。
- 通用 `CaptionButton` 与 `WindowsCaptionButton`。
- `WindowDrawnDecorations` 和 `FullscreenPopoverLayer` 的全屏标题表面。
- `ImagePreviewerTitleBar` 的标题组和操作区。
- Light/Dark、active/inactive、hover、pressed、maximized 和 fullscreen 状态。

应用可以通过 AtomUI Token 覆盖机制定制视觉变量。应用不应通过 Token 改变标题对齐含义、CSD 判定、caption button 能力或 Template Part 结构。

## 5. 兼容性要求

- 不删除或重命名既有 TokenKind、TokenResource key 和 AXAML 消费名。
- 不把运行时状态、effective visibility 或 platform metrics 写入 Token。
- `LogoAndLeftAddOnSpacing`、`LogoAndTitleSpacing` 和 `HeaderHorizontalSpacing` 保持独立语义，不因当前默认值相近而复用同一个 Own Token。
- Token 默认值变化不得改变 caption button 命中区域与布局占用之间的约定。
- Token 源码变化必须同步生成资源、Theme 引用、结构测试和本文档。

## 6. 验证策略

- `TitleBarPadding` 变化验证四种显式标题对齐、native inset 和窄窗口退化。
- `LogoAndLeftAddOnSpacing` 变化验证 Windows/Linux 中两个 presenter 同时可见、任一 presenter 缺失或隐藏、动态替换以及 Leading 安全宽度更新。
- caption 尺寸或间距变化验证 Window、ImagePreviewer 和全屏标题宿主；Windows AddOn 额外验证交互面跟随标题栏高度且业务图标仍消费 `CaptionButtonIconSize`。
- 颜色变化验证 Light/Dark、active/inactive、hover、pressed 和 Windows close danger state。
- Token 名称或默认值变化核对 generated `WindowTitleBarTokenKind`、`WindowTitleBarTokenResource` 和全部 AXAML 引用。

文档改动执行 `git diff --check`。Token 默认值或 Theme 映射变化运行 `WindowTitleBarTokenTests`、`ImagePreviewerTitleBarThemeTests` 以及相关 Window 主题测试。
