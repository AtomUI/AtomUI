# ToggleSwitch Token 设计

本文档定义 `AtomUI.Desktop.Controls.ToggleSwitchToken` 的专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。ToggleSwitch 整体架构见 [ToggleSwitch 桌面版架构设计](overview.md)，内部实现原理见 [ToggleSwitch 桌面版实现原理](implementation.md)，设计和契约变化记录见 [ToggleSwitch Changelog](changelog.md)。

## 1. 定位

ToggleSwitchToken 是 ToggleSwitch 的控件级 Token scope，描述轨道尺寸、把手尺寸、内容边距、图标尺寸、开关颜色、禁用透明度、内容字体、把手阴影和加载指示。

ToggleSwitchToken 不承载以下状态：

- `IsChecked`、`IsLoading`、`IsPressed`、`IsPointerOver`、`IsEnabled` 等实例状态。
- `OnContent`、`OffContent` 或内容模板。
- Form value、校验状态或业务异步状态。
- 当前把手位置、内容偏移、加载旋转角度或动画运行状态。

## 2. Token 分类

### 2.1 轨道尺寸 Token

- `TrackHeight`
- `TrackHeightSM`
- `TrackMinWidth`
- `TrackMinWidthSM`
- `TrackPadding`

这些 Token 控制 ToggleSwitch 的胶囊轨道高度、最小宽度和内部 padding。普通尺寸根据 SharedToken 字号和行高计算，小号高度使用 `SharedToken.ControlHeight / 2`。

`AbstractToggleSwitch` 另公开同名的 owner 级属性 `TrackHeight`、`TrackMinWidth`、`TrackPadding`、`KnobSize`（对应 antd
Switch `ComponentToken` 的 `trackHeight` / `trackMinWidth` / `trackPadding` / `handleSize`）：Token 提供默认值与主题
分支，公开属性是 Semantic Style 的定制入口，二者同名但职责分层，参见
[ToggleSwitch Semantic Part 契约](semantic-part.md)。

### 2.2 把手 Token

- `HandleBg`
- `HandleShadow`
- `HandleSize`
- `HandleSizeSM`

这些 Token 控制把手背景、阴影和普通/小号尺寸。按下时把手宽度由运行时 `STRETCH_FACTOR` 拉伸，不作为 Token。

### 2.3 内容边距 Token

- `InnerMinMargin`
- `InnerMaxMargin`
- `InnerMinMarginSM`
- `InnerMaxMarginSM`

这些 Token 控制 on/off 内容在轨道内与把手之间的可用空间。控件测量会把内容最大宽度与这组边距合并，确保文本或图标内容不与把手重叠。

### 2.4 图标和字体 Token

- `IconSize`
- `IconSizeSM`
- `ExtraInfoFontSize`
- `ExtraInfoFontSizeSM`

这些 Token 用于内部文字和图标内容。`OnContent` / `OffContent` 为 `PathIcon` 或 `Icon` 时，控件把 `IconSize` 同步到图标宽高。

### 2.5 状态颜色和透明度 Token

- `SwitchColor`
- `SwitchDisabledOpacity`
- `OffStateLoadIndicatorColor`

`SwitchColor` 用于 checked 轨道和 checked 状态 loading 指示。unchecked 轨道和 hover 色来自 SharedToken。`SwitchDisabledOpacity` 用于 disabled 和 loading 状态整体透明度。`OffStateLoadIndicatorColor` 用于 unchecked loading 指示。

### 2.6 加载动画 Token

- `LoadingAnimationDuration`

该 Token 控制 `SwitchKnob` loading arc 的旋转周期。它不表达当前旋转角度或动画是否运行。

## 3. 控件专项模型中的 Token 使用

ToggleSwitchToken 使用路径：

```text
SharedToken
   ↓
ToggleSwitchToken
   ↓
ToggleSwitchTheme / SwitchKnobTheme
   ↓
AbstractToggleSwitch + SwitchKnob
```

尺寸分支：

```text
SizeType=Small
  → TrackHeightSM / TrackMinWidthSM / HandleSizeSM
  → InnerMinMarginSM / InnerMaxMarginSM / IconSizeSM / ExtraInfoFontSizeSM

SizeType=Middle|Large|Custom
  → TrackHeight / TrackMinWidth / HandleSize
  → InnerMinMargin / InnerMaxMargin / IconSize / ExtraInfoFontSize
```

`Custom` 当前复用普通尺寸分支。用户通过显式属性覆盖尺寸时，Token 仍作为默认值来源。

## 4. 控件家族影响

ToggleSwitchToken 直接影响：

- `ToggleSwitchTheme.axaml`
- `SwitchKnobTheme.axaml`
- `AbstractToggleSwitch` 的测量、布局和渲染结果
- `SwitchKnob` 的把手绘制和 loading animation
- Gallery ToggleSwitch 示例和 token.md 语义说明

ToggleSwitchToken 不影响 Checkbox、Radio、Segmented、Button 或 Select。这些控件有独立状态选择模型和 Token。

## 5. 兼容性要求

ToggleSwitchToken 属于 ToggleSwitch 主题契约。即使 `ToggleSwitchToken` 是 internal 类型，生成的 `ToggleSwitchTokenKind`、AXAML resource 使用点和 token.md 语义说明已经形成稳定依赖。

Token 变更要求：

- 不擅自重命名、删除或迁移既有 Token。
- 不把 `IsChecked`、`IsLoading`、`IsPressed`、内容实例或 Form 值迁移为 Token。
- 不新增未经授权的 `Custom` 专属 Token 组。
- 不把 unchecked hover 色复制到 ToggleSwitchToken；当前由 SharedToken selector 表达。
- 调整尺寸 Token 时必须同时验证内容测量、把手位置、loading arc 和 Gallery 示例。
- 需要破坏性变更时，必须先说明影响范围并获得授权。

## 6. 验证策略

| 改动类型 | 验证要求 |
| --- | --- |
| 修改轨道尺寸 Token | 验证普通、小号和 Custom 默认尺寸下高度、最小宽度、hit test 和轨道绘制。 |
| 修改把手 Token | 验证把手位置、按下拉伸、loading arc 居中和 checked/unchecked 两端对齐。 |
| 修改内容边距 Token | 验证文字、图标、较长 on/off 内容和小号内容不与把手重叠。 |
| 修改图标或字体 Token | 验证 text content、PathIcon、Icon 的字号、尺寸和前景色同步。 |
| 修改状态颜色或透明度 Token | 验证 checked、unchecked、hover、disabled、loading 和 loading indicator 颜色。 |
| 修改加载动画 Token | 验证 loading animation 周期、attach/detach 取消和停止 loading 状态。 |
| 删除或重命名 Token | 默认不允许；如获授权，需同步 AXAML 引用、生成文件、Token 类型、生成数据和 token.md和控件文档。 |
| 文档改动 | 运行 `git diff --check`，检查文档链接存在。 |
