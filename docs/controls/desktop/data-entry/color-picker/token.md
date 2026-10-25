# ColorPicker Token 设计

本文档定义 ColorPicker 相关控件 Token 的专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。ColorPicker 整体架构见 [ColorPicker 桌面版架构设计](overview.md)，内部实现原理见 [ColorPicker 桌面版实现原理](implementation.md)，设计和契约变化记录见 [ColorPicker Changelog](changelog.md)。

## 1. 定位

ColorPicker Token 只表达组件级视觉变量，例如尺寸、间距、颜色、圆角、阴影、图标尺寸和弹层边界。Token 不承载运行时选择、展开、加载、错误、上传任务、过滤条件或业务状态。

当前 Token scope：

- `ColorPickerToken`，scope id 为 `ColorPicker`，源码位于 `src/AtomUI.Desktop.Controls.ColorPicker/ColorPickerToken.cs`。

## 2. Token 分类

Token 按控件语义分类维护：

| 分类 | 语义 | 代表 Token |
| --- | --- | --- |
| 尺寸与密度 | 控件高度、宽度、图标尺寸、内容最小尺寸。 | `ColorPickerWidth`、`ColorSpectrumHeight`、`ColorPickerHandlerSizeLG`、`ColorPickerHandlerSize`、`ColorPickerHandlerSizeSM`、`ColorPickerSliderSize` |
| 间距与布局 | padding、margin、gap、offset、popup content padding。 | `ColorPickerPresetColorGroupPadding`、`TriggerPadding`、`TriggerTextMargin`、`SliderContainerMargin` |
| 颜色与状态视觉 | 选择环明暗颜色和禁用色块透明度。 | `ColorPickerHandlerLightColor`、`ColorPickerHandlerDarkColor`、`ColorBlockDisabledOpacity` |
| 结构与装饰 | 圆角、阴影、指示器、弹层和装饰线相关变量。 | `ColorPickerInsetShadow`、`ColorBlockInnerShadows` |

未出现在上表中的 Token 仍按源码中的组件语义维护，不按代码顺序机械分类。

### 默认依赖与明确覆盖

24个字段使用版本化依赖默认计划，保持名称、类型、slot和既有资源编号。每个默认只在本层有效合并结果没有明确提供该字段时执行；下游默认读取已经合并的上游Own与ColorPicker Effective Global，而不是重新读取库内常量。0是明确值，不能作为“未配置”的标记。

| 下游默认 | 有效输入与公式 | 现有消费者 |
| --- | --- | --- |
| `CheckedMarkSize` | `ColorPickerPresetColorSize × 0.6` | 预设色项的勾号Width／Height |
| `ColorPickerSliderSize` | `ColorPickerHandlerSizeSM` | 纯色及渐变滑块Height |
| `ColorPickerSliderThumbSize` | `ColorPickerHandlerSizeSM + LineWidth × 2` | 滑块拇指Width／Height |
| `ColorPickerPreviewSize` | `ColorPickerSliderTrackSize × 2 + UniformlyMarginSM` | 面板内ColorPreview色块Size |

明确设置下游时，该值优先；只提供上游时，以上现行公式随其更新；删除覆盖后重新求默认。默认Global下四项依次为14.399999999999999、12、14、28，保留原浮点运算结果而不增加取整。字体、边框、阴影及其它尺寸默认继续读取各自原有Global输入，不增加截断或最小值规则。

`ColorPickerAlphaInputWidth`和`ColorPickerInputNumberHandleWidth`保留原名称、double类型、44／16默认值及资源编号。输入模板对7个NumericUpDown声明式消费按钮宽度，默认仍为16；Alpha宽度只在Own明确提供时应用该值，未明确配置继续历史实际宽度48，明确44或0均生效，删除配置恢复48。HSV／RGB三通道输入宽度仍为48，字体11及间距5保留。输入容器默认宽262；Alpha实际可见且明确宽于48时，以262加上超出48的部分自然扩展，避免固定宽度通道覆盖Alpha；隐藏Alpha或删除配置回到262。可选资源缺失时由局部纯转换器返回兼容默认48，避免Unset遮盖先前Setter而产生NaN；不把资源默认44误称为历史控件宽度，也不增设最小值或取整。

### 共享绘制角色

空值斜线与色谱选择环复用有效Global LineWidthBold，默认2 DIP，明确0表示不绘制对应笔画。空值斜线的颜色复用明确提供的ColorError角色；未明确提供及撤回配置时保留历史红色，其颜色不由默认ColorError替换。显式角色经过当前算法计算后消费，明暗模式不能假定等于原始输入色。

这些角色不新增Own字段或资源编号。触发色块按所属公开Picker身份解析，Popup中的选择环按独立公开View身份解析；宿主、View及其他owner的专属Global各自隔离，general Global和作用域继承仍适用。选择环的明暗笔画颜色继续使用现有ColorPickerHandlerLightColor／ColorPickerHandlerDarkColor，禁用色块透明度继续由ColorBlockDisabledOpacity控制。

## 3. 控件专项模型中的 Token 使用

ColorPicker 的控件专项模型通过 Theme 消费 Token：

- C# 控件负责状态归一和伪类同步。
- AXAML/ControlTheme 负责把 Token 映射到背景、前景、边框、padding、尺寸和动效。
- Token 默认值从 SharedToken 派生，不直接读取控件实例状态。
- Token 类型、生成数据和 token.md 应显式维护，不依赖运行时反射扫描。

确认操作区的布局与可见性由 ControlTheme 表达，间距复用 SharedToken，按钮视觉复用 Button 主题。`IsNeedConfirm`、候选值、输入有效性及确认/取消结果属于实例交互状态，不进入 ColorPicker Token；该能力不增加专属 Token。提交与取消语义见 [ColorPicker 确认与取消设计](confirmation-design.md)。

## 4. 控件家族影响

调整 ColorPicker Token 时必须评估以下范围：

- `ColorPicker`
- `GradientColorPicker`
- `ColorPickerView`
- `ColorSpectrum`
- `ColorSlider`
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
