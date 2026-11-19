# ColorPicker Token 设计

本文档定义 ColorPicker 相关控件 Token 的专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。ColorPicker 整体架构见 [ColorPicker 桌面版架构设计](overview.md)，内部实现原理见 [ColorPicker 桌面版实现原理](implementation.md)，设计和契约变化记录见 [ColorPicker Changelog](changelog.md)。

## 1. 定位

ColorPicker Token 只表达组件级视觉变量，例如尺寸、间距、颜色、圆角、阴影、图标尺寸和弹层边界。Token 不承载运行时选择、展开、加载、错误、上传任务、过滤条件或业务状态。

当前有四个独立 scope：`ColorPicker`、`GradientColorPicker`、`ColorPickerView`、`GradientColorPickerView`。前两个只拥有 trigger 实际消费的五项，后两个拥有编辑面板实际消费的字段；抽象 Token 层共享公式，各终端按自己的 TokenKind 守卫显式基值，不借用其他公开控件的快照。

`ColorPickerThemeInputs` 是内部可继承样式输入，各终端 AXAML 读取自己的 Own 值，再传给共享的色块、色谱、预设项和滑块主题。它不产生运行时 Token 别名。

## 2. Token 分类

Token 按控件语义分类维护：

| 分类 | 语义 | 代表 Token |
| --- | --- | --- |
| Trigger 外观 | trigger 内边距、文字外边距、禁用色块透明度。 | `TriggerPadding`、`TriggerTextMargin`、`ColorBlockDisabledOpacity` |
| 色块装饰 | trigger 与面板分别拥有实际使用的阴影和透明格尺寸。 | `ColorBlockInnerShadows`、`TransparentBgSize` |
| 面板几何 | 色谱、预设区、预览与滑块尺寸。 | `ColorPickerWidth`、`ColorSpectrumHeight`、`ColorPickerHandlerSize*`、`ColorPickerSlider*`、`ColorPickerPreviewSize`、`ColorPickerPreset*`、`CheckedMarkSize` |
| 面板装饰与间距 | 指示圈明暗、内阴影及滑块外边距。 | `ColorPickerHandlerLightColor`、`ColorPickerHandlerDarkColor`、`ColorPickerInsetShadow`、`SliderContainerMargin` |
| 编辑器布局与字号 | Alpha 宽度、数字按钮宽度和明确的紧凑字号。 | `ColorPickerAlphaInputWidth`、`ColorPickerInputNumberHandleWidth`、`EditorFontSize` |

两个原未消费字段均为 Active：Alpha 宽度控制真实 `PART_AlphaInput`，默认保持原控件的 48px（旧无效字段默认 44px）；handle 宽度控制真实 NumericUpDown 浮动按钮，默认仍为 16px。`EditorFontSize` 默认保持原紧凑编辑器的 11px。固定 262px 输入行和 48px 数值通道属于布局装配常量。

`ColorPickerHandlerSizeSM` 派生滑块大小与 thumb 大小；`ColorPickerSliderTrackSize` 派生预览尺寸；`ColorPickerPresetColorSize` 派生勾选标记。显式覆盖先参与这些默认派生。业务颜色、HSV/RGB 端点和 Alpha 是数据，不随主题色替换；空色斜线读取 `ColorError` / `LineWidthBold`，预设色勾选前景读取 owner 的 `ColorTextSecondary` / `ColorWhite`。

## 3. 控件专项模型中的 Token 使用

ColorPicker 的控件专项模型通过 Theme 消费 Token：

- C# 控件负责状态归一和伪类同步。
- AXAML/ControlTheme 负责把 Token 映射到背景、前景、边框、padding、尺寸和动效。
- Token 默认值从本终端有效全局 Token 派生，不直接读取控件实例状态。
- Token 类型、生成数据和 token.md 应显式维护，不依赖运行时反射扫描。

色块通过同一 `OwnerTokenSet` 读取宿主的空色斜线和棋盘格画刷。色块与滑块直接读取作用域继承的棋盘格尺寸，由同一缓存生成图案。
带文字的 `ColorPicker` 从逻辑起始边使用触发器的边框厚度与 `TriggerPadding`。颜色文字与色块之间由 `TriggerTextMargin` 控制。

确认操作区的布局与可见性由 ControlTheme 表达，间距使用 View 有效全局 Token，按钮视觉复用 Button 主题。`IsNeedConfirm`、候选值、输入有效性及确认/取消结果属于实例交互状态，不进入 ColorPicker Token；该能力不增加专属 Token。提交与取消语义见 [ColorPicker 确认与取消设计](confirmation-design.md)。

## 4. 控件家族影响

四终端的 Own 和有效全局覆盖互不串用。Popup View 的 EnableMotion 默认独立生效；trigger 的 IsMotionEnabled=false 是宿主禁用门控，用户显式设置 View.IsMotionEnabled 本地值仍拥有更高优先级。Popup 中公开 View 保持自己的身份；其中公开 NumericUpDown、LineEdit、ComboBox 和 Button 也保留各自身份。数值编辑器的 11px 字号由父 View 明确指定，用户本地 FontSize 仍优先；此场景不能声称 NumericUpDown.FontSize 会穿透父字号。

调整 ColorPicker Token 时必须评估以下范围：

- `ColorPicker`
- `GradientColorPicker`
- `ColorPickerView`
- `ColorSpectrum`
- `ColorSlider`
- 对应 Gallery ShowCase 的示例和源码片段。
- Light/Dark 主题、Browser/Desktop 主题和 Compact/Form/Popup 集成场景。

## 5. 兼容性要求

本轮 Own 键迁移如下；旧 ColorPicker 面板覆盖应移到实际 `ColorPickerView` 或 `GradientColorPickerView`，不保留跨控件别名。两个 trigger 通过自己的 scope 分别配置。

| 旧 `ColorPicker` 字段 | 当前实际终端 |
| --- | --- |
| `ColorBlockDisabledOpacity` | `ColorPicker` / `GradientColorPicker` trigger |
| `ColorBlockInnerShadows` | 四终端分别拥有 |
| `TransparentBgSize` | 四终端分别拥有 |
| `TriggerPadding` | `ColorPicker` / `GradientColorPicker` trigger |
| `TriggerTextMargin` | `ColorPicker` / `GradientColorPicker` trigger |
| `ColorPickerWidth` | `ColorPickerView` / `GradientColorPickerView` 面板 |
| `ColorSpectrumHeight` | `ColorPickerView` / `GradientColorPickerView` 面板 |
| `ColorPickerInsetShadow` | `ColorPickerView` / `GradientColorPickerView` 面板 |
| `ColorPickerHandlerSizeLG` | `ColorPickerView` / `GradientColorPickerView` 面板 |
| `ColorPickerHandlerSize` | `ColorPickerView` / `GradientColorPickerView` 面板 |
| `ColorPickerHandlerSizeSM` | `ColorPickerView` / `GradientColorPickerView` 用它派生滑块和滑块手柄尺寸 |
| `ColorPickerHandlerLightColor` | `ColorPickerView` / `GradientColorPickerView` 面板 |
| `ColorPickerHandlerDarkColor` | `ColorPickerView` / `GradientColorPickerView` 面板 |
| `ColorPickerSliderSize` | `ColorPickerView` / `GradientColorPickerView` 面板 |
| `ColorPickerSliderTrackSize` | `ColorPickerView` / `GradientColorPickerView` 面板 |
| `ColorPickerSliderThumbSize` | `ColorPickerView` / `GradientColorPickerView` 面板 |
| `ColorPickerPreviewSize` | `ColorPickerView` / `GradientColorPickerView` 面板 |
| `ColorPickerAlphaInputWidth` | `ColorPickerView` / `GradientColorPickerView` 面板 |
| `ColorPickerInputNumberHandleWidth` | `ColorPickerView` / `GradientColorPickerView` 面板 |
| `ColorPickerPresetColorSize` | `ColorPickerView` / `GradientColorPickerView` 面板 |
| `ColorPickerPresetPanelWidth` | `ColorPickerView` / `GradientColorPickerView` 面板 |
| `CheckedMarkSize` | `ColorPickerView` / `GradientColorPickerView` 面板 |
| `ColorPickerPresetColorGroupPadding` | `ColorPickerView` / `GradientColorPickerView` 面板 |
| `SliderContainerMargin` | `ColorPickerView` / `GradientColorPickerView` 面板 |

新增 `EditorFontSize` 只属于两个 View。没有删除为无效项或保留为 Reserved 的字段。

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
