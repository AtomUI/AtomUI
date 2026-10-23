# Button Token 设计

本文档定义 `AtomUI.Desktop.Controls.ButtonToken` 的 Button 专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。Button 整体架构见 [Button 桌面版架构设计](overview.md)，内部实现原理见 [Button 桌面版实现原理](implementation.md)，设计和契约变化记录见 [Button Changelog](changelog.md)。

## 1. 定位

ButtonToken 是 Button 的组件级设计变量层。它把全局设计体系中的颜色、尺寸、间距、字体和阴影转换为 Button 可消费的语义值。

ButtonToken只定义Button自身的Own资源；Dropdown、Hyper、Split、Option使用各自identity与Own定义。组合控件内部确实为Button的子实例可以消费Button资源，但不能把父控件的Own配置复制到Button或反向借用。历史额外内容字段不代表Button已经拥有一个额外内容区域。

ButtonToken 不承载 `IsPressed`、`IsPointerOver`、`IsLoading`、`EffectiveColor`、`EffectiveVariant` 等实例状态。这些状态由 Button 状态模型和主题变量处理。

## 2. Token 分类

ButtonToken 当前按 Button 语义分为七类。

### 2.1 排版 Token

- `FontWeight`
- `ContentFontSize`
- `ContentFontSizeLG`
- `ContentFontSizeSM`
- `ContentLineHeight`
- `ContentLineHeightLG`
- `ContentLineHeightSM`

字体大小默认来自Button的Effective Global。Own默认函数按显式依赖顺序执行：字体进入布局行高，行高进入纵向内边距；显式下游独立保留。行高单位为DIP，是默认尺寸计算的布局指标；内容的原生段落排版仍可通过Content专用Style定制。

`FontWeight`保留既有double资源类型及400默认读值，接受整数1–1000。其渲染为可选Own覆盖：未配置时保持原生祖先字重，显式400也可覆盖Bold祖先，清除后恢复继承。最终设置位于ControlTheme，实例属性及普通应用Style继续遵循原生优先级。资源缺省与Setter回退的区别见[Core资源合同](../../../../modules/core/configured-token-resources.md)。

### 2.2 尺寸与间距 Token

- `ContentPadding`
- `ContentPaddingLG`
- `ContentPaddingSM`
- `CirclePadding`
- `IconOnyPadding`
- `IconOnyPaddingLG`
- `IconOnyPaddingSM`
- `IconMargin`
- `ExtraContentItemSpacing`
- `ExtraContentMargin`
- `ExtraContentMarginLG`
- `ExtraContentMarginSM`
- `GutterToFlyout`

用于 Button 的尺寸密度、icon-only 内边距、圆形按钮内边距、DropdownButton 额外内容和下拉菜单间距。

`SizeType=Custom` 不新增 ButtonToken。Custom 复用 `Middle` 档的字体、Padding、圆角和 icon Token，不继承 Button
预设档的 `MinHeight` 基线；实例级定制通过 Button 现有布局、排版以及 `IconWidth`、`IconHeight` 属性完成，Token 仍只
表达稳定设计指标，不决定 Custom 的最终高度。

### 2.3 Icon Token 与 SharedToken 依赖

ButtonToken 提供：

- `OnlyIconSize`
- `OnlyIconSizeLG`
- `OnlyIconSizeSM`

SharedToken 依赖：

- `IconSize`
- `IconSizeLG`
- `IconSizeSM`

`IconSizeLG`、`IconSize`、`IconSizeSM` 是用户 icon 与普通 loading icon 的默认尺寸，也用于非 loading 的 icon-only 用户图标。`OnlyIconSizeLG`、`OnlyIconSize`、`OnlyIconSizeSM` 只用于 `:icononly:loading` 状态下的 loading icon 默认尺寸，不改变普通 icon-only 用户图标的既有视觉。

Theme 把上述 ButtonToken 或 SharedToken 资源设置到 Button 自身的 `IconWidth`、`IconHeight`，模板中的用户 icon 与 loading icon 再通过 `TemplateBinding` 消费。Token 只提供默认值；Button 实例上的本地 `IconWidth`、`IconHeight` 具有更高优先级，并同时覆盖两个图标节点。宽高属性相互独立，Token 默认值为正方形不限制用户设置非正方形尺寸。

### 2.4 默认按钮颜色 Token

- `DefaultColor`
- `DefaultBg`
- `DefaultBorderColor`
- `DefaultBorderColorDisabled`
- `DefaultHoverBg`
- `DefaultHoverColor`
- `DefaultHoverBorderColor`
- `DefaultActiveBg`
- `DefaultActiveColor`
- `DefaultActiveBorderColor`
- `BorderColorDisabled`

用于普通动作的文本、背景、边框及 hover / active / disabled 状态。该组 Token 服务 `ButtonType=Default`、`ButtonType=Dashed` 和 `Color=Default` 的基础视觉。

### 2.5 主色与危险 Token

- `PrimaryColor`
- `DangerColor`
- `PrimaryShadow`
- `DangerShadow`

用于主动作和危险动作的前景与阴影。危险语义优先级高，不应通过普通样式覆盖隐式消失。

### 2.6 Ghost、Text、Link Token

- `DefaultGhostColor`
- `GhostBg`
- `DefaultGhostBorderColor`
- `SolidTextColor`
- `TextTextColor`
- `TextTextHoverColor`
- `TextTextActiveColor`
- `TextHoverBg`
- `LinkHoverBg`

用于 ghost 背景策略、文本按钮、链接按钮和实心按钮前景策略。Ghost 是背景呈现策略，Text / Link 是低强调动作表达。

Text Token 必须保持以下语义：

- `ButtonType=Text` 等价于 `Color=Default + Variant=Text`，文字使用 `TextTextColor`、
  `TextTextHoverColor` 和 `TextTextActiveColor`，默认都来自 `ColorText`，不隐式跟随品牌主色。
- `TextHoverBg` 默认来自 `ColorFillTertiary`；pressed 背景使用 `ColorFill`。
- `Color=Primary + Variant=Text` 的 normal、hover、pressed 文字分别来自 `ColorPrimary`、
  `ColorPrimaryHover`、`ColorPrimaryActive`，背景分别为透明、`ColorPrimaryBg`、`ColorPrimaryBorder`。
- Danger Text 的 hover 背景使用 `ColorErrorBg`，pressed 背景使用 `ColorErrorBgActive`。

### 2.7 结构协同 Token

- `DefaultShadow`
- `GroupBorderColor`

DefaultShadow进入默认按钮实际阴影。GroupBorderColor的历史含义对应组内接缝，当前Button没有Ant旧Button.Group等价区域；不得把它改成整个Button的BorderBrush或静默影响CompactSpace。该字段保留ID/类型/默认值，兼容处置待审查，不列为已验收的有效定制入口。

## 3. Button 多彩模型中的 Token 使用

Button 多彩模型不在 ButtonToken 中展开所有颜色与 variant 组合。

ButtonToken 保留 Button 组件常用语义值，例如 default、primary、danger、text、link、shadow、padding。Preset color 的 base / hover / active / light / shadow 来源于全局 palette 或通用 preset palette resource。

Button 颜色状态按以下职责使用 Token：

```text
ButtonToken
  提供 default / primary / danger / text / link 等组件语义值

Palette
  提供 preset color 色阶

Effective color resolver
  根据 EffectiveColor 选择 ButtonToken、SharedToken 或 Palette 值

Variant resolver
  将颜色变量映射到文字、背景、边框和阴影变量

ControlTheme
  只把最终主题变量投影到 normal、hover、pressed 和 disabled 视觉
```

ButtonToken 不定义 `BlueSolidHoverBg`、`PurpleFilledActiveBg` 这类组合 Token。

## 4. Button 家族影响

ButtonToken 当前被 Button 家族主题共同引用。Token 变更必须评估：

- 默认 Button
- DropdownButton
- SplitButton
- HyperLinkButton
- IconButton 相关主题

如果某个值只服务特定家族控件，应确认它是否仍属于 Button 体系共享语义。只有共享语义值才应进入 ButtonToken。

Button 家族控件支持 Custom 尺寸时，应复用 Middle 档中适用于该控件的非高度 Token，并由各自主题明确是否提供高度
基线；设置本地属性时由本地值覆盖。DropdownButton 继承 Button 的 `IconWidth`、`IconHeight` 语义，但其
`OpenIndicator` 保持独立尺寸职责。不得为家族控件私自复制一组 Custom 专属 Token，除非该值已经证明是稳定的家族级
语义。

## 5. 兼容性要求

ButtonToken 属于 Button 主题契约。即使 `ButtonToken` 是 internal 类型，生成的 `ButtonTokenKind` 和 AXAML resource 使用点已经形成稳定依赖。

ButtonToken 变更要求：

- 不擅自重命名既有 Token。
- 不擅自删除既有 Token。
- 不改变既有 Token 的语义含义。
- 不把实例状态迁移到 Token。
- 不在 ButtonToken 中硬编码 Button 私有 preset 色表。
- 需要破坏性变更时，必须先说明影响范围并获得授权。

## 6. 验证策略

| 改动类型 | 验证要求 |
| --- | --- |
| 新增 ButtonToken | 检查生成的 `ButtonTokenKind`、AXAML 引用和默认值计算。 |
| 修改 ButtonToken 计算 | 覆盖 light / dark 主题，检查 Button 与 Button 家族视觉。 |
| 删除或重命名 ButtonToken | 默认不允许；如获授权，需同步所有 AXAML 引用和生成文件。 |
| 多彩按钮 Token 调整 | 验证 default、primary、danger、preset color 与各 variant 的状态映射。 |
| Icon Token 投影调整 | 覆盖 Large / Middle / Small / Custom、普通 icon-only、icon-only loading、本地 `IconWidth` / `IconHeight` 覆盖，以及桌面 / Browser / DropdownButton 模板。 |

## 波纹 Global 输入

`WaveSizeMotionDuration` / `WaveOpacityMotionDuration` 使用实际 Button 的 Effective Global，默认300/350ms。`WaveSizeEasingCurve` / `WaveOpacityEasingCurve` 默认 CubicEaseOut。显式时长精确生效，EnableMotion=false使二者归零。详见[Core动效合同](../../../../modules/core/transition-durations.md)；完整Button Own V2及家族验收仍单独执行。

## FontFamily 的精确与继承合同

本控件的类型化Global FontFamily在正规化配置直接包含该字段时进入根渲染；未配置则保留真实InheritanceParent的字体，清除恢复当前祖先。Shared字体配置同样生效，显式配置与缺省不按值相等猜测。FontFamily Setter由ControlTheme MultiBinding提供，控件只暴露内部只读继承源；原生Style/local及资源覆盖保持。接口观察在分离时释放父订阅，重挂刷新，不以逻辑父代替受保护的真实继承父。详细机制见[显式Token资源](../../../../modules/core/configured-token-resources.md)。

## 显式可选消费与兼容

DefaultBorderColorDisabled用于EffectiveColor=Default、EffectiveVariant=Outlined/Dashed的禁用描边。显式配置时覆盖通用BorderColorDisabled；未配置/清除时继续消费通用值。其他配色/变体保持原状态规则。存在位与普通资源值分开，资源字典覆盖和实例BorderBrush保留原生优先级；最终Setter在ControlTheme中。

GutterToFlyout用于Button.Flyout关联的AtomUI Flyout/MenuFlyout，只有Flyout实际Target为该Button且该Own明确配置时投射MarginToAnchor，支持0与打开期间更新。未配置保留旧Flyout默认0；Token历史默认资源值4不自动改变已有弹层位置。Flyout本地MarginToAnchor优先，RequestedPlacement=null仍保留Avalonia默认定位模式。原生Avalonia Flyout没有这个Margin合同，不注入。非Visual Flyout不在模板子树，代码绑定有具体原因注释，跟随Target并在关闭/替换/分离释放。共享Flyout只使用当前Target owner。

ExtraContentItemSpacing、ExtraContentMargin/SM/LG没有对应Button实际区域；保持资源兼容，不能作为已生效能力展示。与GroupBorderColor的正式兼容决策和工作台可编辑状态须在最终交付审查明确，不以“保留字段”冒充严格Token接入完成。
