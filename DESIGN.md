---
version: alpha
name: AtomUI
description: Ant Design 6 visual language implemented for Avalonia and .NET, with semantic tokens, scoped themes, and typed customization.
---

# AtomUI Design Language

[简体中文（默认）](DESIGN.md) | [English](DESIGN.en-US.md)

## Overview

AtomUI 在 Avalonia/.NET 上实现 **Ant Design 6 的设计语言**，面向表单、数据表格、导航、工作台等信息密集型应用。
本文以 [Ant Design DESIGN.md](https://github.com/ant-design/ant-design/blob/master/DESIGN.md) 为母版，遵循
[Google DESIGN.md 格式](https://github.com/google-labs-code/design.md/blob/main/docs/spec.md)，供设计师、开发者和 AI 共同使用。

四项设计价值贯穿界面生成与审查：

- **自然（Natural）**：遵循用户熟悉的操作与阅读习惯，桌面界面支持清晰的键盘焦点和合理的指针反馈。
- **确定（Certain）**：输入、选择、加载、成功、失败等状态明确可辨，让用户知道操作结果和下一步。
- **意义（Meaningful）**：用视觉强调传达任务优先级，装饰服务于信息、操作和状态。
- **生长（Growing）**：相同规则能够覆盖简单表单、复杂数据界面和逐步扩展的业务模块。

视觉原则继承 Ant Design；API、默认输出和平台适配以 AtomUI 已实现的契约为准。本文 YAML 描述未加应用覆盖的
`DaybreakBlue + ThemeAlgorithm.Default` 浅色基准，通常使用控件默认中号尺寸。暗色和紧凑模式由算法生成。
与上游的差异在相关章节说明，不能据此把实现偏差推广为新的设计原则。移动端控件包仍属目标架构，不属于本文已实现能力。

YAML 是供设计工具读取的可移植视觉投影，**不是 AtomUI 主题配置文件**。其中 `px` 对应 Avalonia 的设备独立单位
（DIP），不代表物理像素；实际缩放由平台和 Avalonia 处理。产品代码应消费 Token 和控件 API。

## Colors

品牌主色组织主要操作、链接、选中态和焦点；成功、警告、错误、信息色表达状态。预设色板适合分类和数据可视化。
保留明确的主要操作层次，状态同时通过文字、图标或结构表达，避免仅靠颜色传递关键信息。

```yaml
colors:
  primary: '#1677FF'
  primary-hover: '#4096FF'
  primary-active: '#0958D9'
  primary-container: '#E6F4FF'
  success: '#52C41A'
  success-container: '#F6FFED'
  warning: '#FAAD14'
  warning-container: '#FFFBE6'
  error: '#FF4D4F'
  error-container: '#FFF2F0'
  info: '#1677FF'
  info-container: '#E6F4FF'
  surface: '#FFFFFF'
  surface-container: '#FAFAFA'
  surface-layout: '#F5F5F5'
  surface-elevated: '#FFFFFF'
  on-surface: '#000000E0'
  on-surface-variant: '#000000A5'
  on-surface-description: '#00000072'
  on-surface-disabled: '#0000003F'
  on-primary: '#FFFFFF'
  outline: '#D9D9D9'
  outline-variant: '#F0F0F0'
  hover-fill: '#0000000F'
  disabled-fill: '#0000000A'
  inverse-surface: '#000000D8'
  inverse-text: '#FFFFFF'
  mask: '#00000073'
```

| 文档语义 | AtomUI Token 或计算来源 |
| --- | --- |
| `primary` / `primary-hover` / `primary-active` / `primary-container` | `ColorPrimary` / `ColorPrimaryHover` / `ColorPrimaryActive` / `ColorPrimaryBg` |
| `success` / `warning` / `error` / `info` 及其 `*-container` | `ColorSuccess` / `ColorWarning` / `ColorError` / `ColorInfo` 及对应 `*Bg` |
| `surface` / `surface-layout` / `surface-elevated` | `ColorBgContainer` / `ColorBgLayout` / `ColorBgElevated` |
| `surface-container` | `ColorFillAlter` 在 `ColorBgContainer` 上合成的浅色实色，DataGrid 表头等使用 |
| 四级文字色 | `ColorText` / `ColorTextSecondary` / `ColorTextDescription` / `ColorTextDisabled` |
| `outline` / `outline-variant` | `ColorBorder` / `ColorBorderSecondary` |
| `hover-fill` / `disabled-fill` | `ColorBgTextHover` / `ColorBgContainerDisabled` |
| `on-primary` / `inverse-text` | `ColorTextLightSolid` |
| `inverse-surface` / `mask` | `ColorBgSpotlight` / `ColorBgMask` |

透明文字和填充应保留 alpha，使其与有色背景自然叠合。YAML 使用 CSS 的 `#RRGGBBAA`，Avalonia 八位色值使用
`#AARRGGBB`；例如 `#000000E0` 对应 AXAML 的 `#E0000000`。AtomUI 以字节保存透明度，文字 alpha 实际为
224/255、165/255、114/255、63/255；它们对应上游约 0.88、0.65、0.45、0.25 的层级，不应再按显示灰度反推 Seed。

预设色板基色：blue `#1677FF`、purple `#722ED1`、cyan `#13C2C2`、green `#52C41A`、magenta `#EB2F96`、
red `#F5222D`、orange `#FA8C16`、yellow `#FADB14`、volcano `#FA541C`、geekblue `#2F54EB`、gold `#FAAD14`、lime `#A0D911`。
使用现有色板及派生梯度建立分类，不为单个页面随意增加视觉强调色。

默认品牌色并不保证所有文字组合达到 WCAG AA；白字配 `#1677FF` 就需要按目标字号评估。严格无障碍场景应通过
全局或 Control Token 调整成套状态色，并在真实背景上检查文字、焦点和错误反馈。格式检查通过不等于无障碍验收通过。
透明前景与背景的对比度应先在真实表面上合成；仅比较 RGB 通道会误报。描述文字色与遮罩是通用语义，组件字段未逐一引用。

## Typography

继承 Ant Design 的紧凑排版：正文基准 14 DIP，常用正文与控件使用 400 字重，标题类强调使用 600。
选中态优先通过既有颜色、指示线和组件状态表达；具体控件允许使用其定义的选中态字重，例如 Select 选项。

```yaml
typography:
  display-lg: { fontFamily: 'Alibaba Sans, Segoe UI, Segoe UI Symbol, Helvetica Neue, Noto Sans, Noto Sans CJK SC, 文泉驿正黑, Microsoft YaHei, PingFang SC, sans-serif', fontSize: 38px, fontWeight: 600, lineHeight: 46px }
  headline-lg: { fontFamily: 'Alibaba Sans, Segoe UI, Segoe UI Symbol, Helvetica Neue, Noto Sans, Noto Sans CJK SC, 文泉驿正黑, Microsoft YaHei, PingFang SC, sans-serif', fontSize: 30px, fontWeight: 600, lineHeight: 38px }
  headline-md: { fontFamily: 'Alibaba Sans, Segoe UI, Segoe UI Symbol, Helvetica Neue, Noto Sans, Noto Sans CJK SC, 文泉驿正黑, Microsoft YaHei, PingFang SC, sans-serif', fontSize: 24px, fontWeight: 600, lineHeight: 32px }
  headline-sm: { fontFamily: 'Alibaba Sans, Segoe UI, Segoe UI Symbol, Helvetica Neue, Noto Sans, Noto Sans CJK SC, 文泉驿正黑, Microsoft YaHei, PingFang SC, sans-serif', fontSize: 20px, fontWeight: 600, lineHeight: 28px }
  title-lg: { fontFamily: 'Alibaba Sans, Segoe UI, Segoe UI Symbol, Helvetica Neue, Noto Sans, Noto Sans CJK SC, 文泉驿正黑, Microsoft YaHei, PingFang SC, sans-serif', fontSize: 16px, fontWeight: 600, lineHeight: 24px }
  title-md: { fontFamily: 'Alibaba Sans, Segoe UI, Segoe UI Symbol, Helvetica Neue, Noto Sans, Noto Sans CJK SC, 文泉驿正黑, Microsoft YaHei, PingFang SC, sans-serif', fontSize: 14px, fontWeight: 600, lineHeight: 22px }
  body-lg: { fontFamily: 'Alibaba Sans, Segoe UI, Segoe UI Symbol, Helvetica Neue, Noto Sans, Noto Sans CJK SC, 文泉驿正黑, Microsoft YaHei, PingFang SC, sans-serif', fontSize: 16px, fontWeight: 400, lineHeight: 24px }
  body-md: { fontFamily: 'Alibaba Sans, Segoe UI, Segoe UI Symbol, Helvetica Neue, Noto Sans, Noto Sans CJK SC, 文泉驿正黑, Microsoft YaHei, PingFang SC, sans-serif', fontSize: 14px, fontWeight: 400, lineHeight: 22px }
  body-sm: { fontFamily: 'Alibaba Sans, Segoe UI, Segoe UI Symbol, Helvetica Neue, Noto Sans, Noto Sans CJK SC, 文泉驿正黑, Microsoft YaHei, PingFang SC, sans-serif', fontSize: 12px, fontWeight: 400, lineHeight: 20px }
```

标题字号来自 `FontSizeHeading1..5`；正文来自 `FontSizeLG`、`FontSize`、`FontSizeSM`；强调字重来自 `FontWeightStrong`。
这些是角色组合，不是额外的 AtomUI Token。不要为对齐名称而创建第二套字号或把所有控件的字号强制设成相同值。

字体是明确的平台适配：运行时 `FontFamily` 首项为 `fonts:AlibabaSans#Alibaba Sans`，末项为 Avalonia `$Default`。
YAML 将其投影为可移植字族名及 `sans-serif` 回退。Alibaba Sans 需要字体包注册；默认链的中文候选来自系统字体，
不能推断应用已经内置中文字体。需要确定字形时显式注册字体包并配置 `FontFamily`，详见[字体族解析](docs/architecture/systems/typography/font-family-resolution.md)。

`RelativeLineHeight*` 是倍率，Avalonia `TextBlock.LineHeight` 是绝对长度。上表已换算为长度；消费运行时 Token 时应将
对应字号与倍率相乘。`FontHeight*` 提供正文的已取整文本高度。修改 `FontSize` 会重新派生字号阶梯；Dark 不改变字号。
当前 Compact 保留基准字号，与上游的紧凑字号派生存在[已知差异](docs/architecture/systems/typography/tokens-and-derivation.md)。

## Layout

以 4 DIP 为基本节奏，按任务关系分组：标签贴近字段，相关操作靠近内容，章节之间留出更明显的间隔。
密集数据通过对齐、分组和稳定行高保持可读性；长内容保留滚动、裁剪和浮层边界，布局调整应维持完整交互。

```yaml
spacing:
  unit: 4px
  xs: 4px
  sm: 8px
  step-12: 12px
  md: 16px
  step-20: 20px
  lg: 24px
  xl: 32px
  xxl: 48px
  control-height-sm: 24px
  control-height: 32px
  control-height-lg: 40px
```

`unit` 对应 `SizeUnit`；4/8/12/16/20/24/32/48 对应 `SizeXXS`、`SizeXS`、`SizeSM`、`Size`、`SizeMD`、`SizeLG`、`SizeXL`、`SizeXXL`。
这里保留上游 `spacing.sm = 8` 的可移植命名，不能把它直接理解为 AtomUI 的 `SizeSM = 12`。
应用布局读取 `UniformlyPadding*`、`UniformlyMargin*` 等适用 Token；控件内部几何使用对应 Own Token。

32/24/40 是 `ControlHeight` / `ControlHeightSM` / `ControlHeightLG` 的标准高度基准；实际控件可能使用 `MinHeight`，
并受内容和布局约束影响。选择组件尺寸 API，避免把所有高度固定成 32。边框补偿产生的 7、11、15 等内边距有真实
几何意义，应保留 Token 派生关系；4 DIP 节奏不意味着每个模板坐标都必须是 4 的倍数。

页面背景、内容容器、浮层分别使用 `ColorBgLayout`、`ColorBgContainer`、`ColorBgElevated`。浅色下后两者同为白色，
仍需保留语义区别，以便暗色、品牌主题和浮层层次正确演进。响应式结构根据可用空间和任务调整，使用现有断点 Token，
不把 Web 页面宽度或 Gallery 展示布局强加给桌面窗口。

## Elevation & Depth

继承 Ant Design 的克制层次：普通内容主要通过边框、间距和色阶分组；弹出面板、提示和对话框通过阴影表达浮起。
遮罩表达模态关系。以下为默认快照中的 Avalonia 阴影语法，字段为偏移、模糊、可选扩散和颜色：

| AtomUI Token | 默认值 |
| --- | --- |
| `BoxShadows` | `0 6 16 #14000000, 0 3 6 -4 #1F000000, 0 9 28 8 #0D000000` |
| `BoxShadowsSecondary` | `0 6 16 #1A000000, 0 3 6 -4 #1F000000, 0 9 28 8 #12000000` |
| `BoxShadowsTertiary` | `0 1 2 #08000000, 0 1 6 -1 #05000000, 0 2 4 #05000000` |

这些值由 AtomUI Alias 计算产生，不能根据上游相似名称假定每个阴影完全相同。Card 等控件还可能定义自己的阴影及变体。
浮层沿用框架的定位、焦点、遮罩和关闭行为，避免通过随意增加阴影或 ZIndex 掩盖宿主、命中或裁剪问题。

动效为状态变化建立连续性：`MotionDurationFast` 为 100 ms，`MotionDurationMid` 为 200 ms，
`MotionDurationSlow` 为 300 ms，`MotionDurationVerySlow` 为 800 ms。复用控件与 MotionScene 已有曲线和转场；
AtomUI 当前没有与上游 `motionEase*` 同名的全局 Token，不能直接移植 CSS 参数名。
`EnableMotion=false` 使主题动效时长归零；点击波纹由 `EnableWaveSpirit` 控制。
主题作者将 `Transitions` 声明在 ControlTheme 的 Style Setter 中，遵循[主题定制契约](docs/guides/theming/customization.md)。

## Shapes

继承 Ant Design 的圆角层级：常规输入和按钮保持清晰轮廓，容器略大，小标签更紧凑。

```yaml
rounded:
  none: 0px
  sm: 2px
  md: 4px
  DEFAULT: 6px
  lg: 8px
```

`DEFAULT` / `lg` / `md` / `sm` 对应 `BorderRadius` / `BorderRadiusLG` / `BorderRadiusSM` / `BorderRadiusXS`；
`none` 表示直角。圆形头像、状态点、圆形按钮使用相应控件的形状 API，不能把某个极大圆角值视为通用运行时 Token。
同一分组中的形状应有一致层级，品牌定制通过基准圆角及组件能力表达。具体控件可能拥有独立圆角 Token，例如 ToolTip。

## Components

以下原型记录默认浅色视觉。只列出能由当前 Token 和主题资产确认的属性；省略项由控件自身处理。
`padding` 使用 CSS 的上/右/下/左顺序；复制到 AXAML 前应转换为 Avalonia 的左/上/右/下顺序。

```yaml
components:
  button-primary: { backgroundColor: '{colors.primary}', textColor: '{colors.on-primary}', typography: '{typography.body-md}', rounded: '{rounded.DEFAULT}', height: '{spacing.control-height}', padding: 4px 15px }
  button-primary-hover: { backgroundColor: '{colors.primary-hover}' }
  button-primary-active: { backgroundColor: '{colors.primary-active}' }
  button-default: { backgroundColor: '{colors.surface}', textColor: '{colors.on-surface}', typography: '{typography.body-md}', rounded: '{rounded.DEFAULT}', height: '{spacing.control-height}', padding: 4px 15px }
  button-default-hover: { textColor: '{colors.primary-hover}' }
  button-disabled: { backgroundColor: '{colors.disabled-fill}', textColor: '{colors.on-surface-disabled}' }
  input-field: { backgroundColor: '{colors.surface}', textColor: '{colors.on-surface}', typography: '{typography.body-md}', rounded: '{rounded.DEFAULT}', height: '{spacing.control-height}' }
  select-field: { backgroundColor: '{colors.surface}', textColor: '{colors.on-surface}', typography: '{typography.body-md}', rounded: '{rounded.DEFAULT}', height: '{spacing.control-height}', padding: 4px 11px }
  select-option-selected: { backgroundColor: '{colors.primary-container}', textColor: '{colors.on-surface}', typography: '{typography.title-md}' }
  card: { backgroundColor: '{colors.surface}', textColor: '{colors.on-surface}', rounded: '{rounded.lg}', padding: 12px }
  card-large: { backgroundColor: '{colors.surface}', textColor: '{colors.on-surface}', rounded: '{rounded.lg}', padding: 24px }
  modal: { backgroundColor: '{colors.surface-elevated}', textColor: '{colors.on-surface}', rounded: '{rounded.lg}' }
  menu-item-selected: { backgroundColor: '{colors.primary-container}', textColor: '{colors.primary}', typography: '{typography.body-md}' }
  tabs-tab-active: { textColor: '{colors.primary}', typography: '{typography.body-md}' }
  table-header: { backgroundColor: '{colors.surface-container}', textColor: '{colors.on-surface}', typography: '{typography.title-md}', padding: 16px }
  tag: { backgroundColor: '{colors.surface-layout}', textColor: '{colors.on-surface}', typography: '{typography.body-sm}', rounded: '{rounded.md}', padding: 0px 7px }
  tooltip: { backgroundColor: '{colors.inverse-surface}', textColor: '{colors.inverse-text}', typography: '{typography.body-md}', rounded: '{rounded.md}', padding: 8px 12px }
  dropdown-item-hover: { backgroundColor: '{colors.hover-fill}', textColor: '{colors.on-surface}' }
  alert-success: { backgroundColor: '{colors.success-container}', textColor: '{colors.on-surface}', rounded: '{rounded.lg}', padding: 8px 12px }
  alert-warning: { backgroundColor: '{colors.warning-container}', textColor: '{colors.on-surface}', rounded: '{rounded.lg}', padding: 8px 12px }
  alert-error: { backgroundColor: '{colors.error-container}', textColor: '{colors.on-surface}', rounded: '{rounded.lg}', padding: 8px 12px }
  alert-info: { backgroundColor: '{colors.info-container}', textColor: '{colors.on-surface}', rounded: '{rounded.lg}', padding: 8px 12px }
  badge-status-error: { backgroundColor: '{colors.error}', width: 6px, height: 6px }
  tag-blue: { backgroundColor: '{colors.primary-container}', textColor: '{colors.primary-active}', typography: '{typography.body-sm}', rounded: '{rounded.md}', padding: 0px 7px }
```

| 设计原型 → AtomUI | 视觉与交互规则 |
| --- | --- |
| Button → `Button` | 主要操作使用主要色，辅助操作降低强调；保留默认、hover、pressed、focus、disabled、loading 的差别。使用 `ButtonType` 或 `Color` / `Variant` API。 |
| Input → `LineEdit` | 输入与按钮保持尺度一致；边框、焦点和阴影表示输入位置，标签、帮助文字与错误反馈形成稳定结构。焦点不靠 placeholder 表达。 |
| Select → `Select` | 单选触发器与输入体系一致；列表选中态和 hover 有不同含义，多选使用控件自身标签、换行和尺寸规则。 |
| Checkbox / Radio / Switch → `CheckBox` / `RadioButton` / `ToggleSwitch` | 区分多选、互斥选择和即时开关；保留 checked、适用的 indeterminate、键盘焦点及禁用反馈。 |
| Card → `Card` | 通过标题、内容、操作区组织相关信息；边框和变体表达层次，内部控件遵循共同间距。 |
| Modal → `Dialog` 家族 | 通过浮层表面和遮罩明确模态关系，清楚表达确认与取消；窗口与 overlay 形态遵守各自宿主和布局契约。 |
| Navigation Menu → `NavMenu` | 用选中项表达当前位置；竖向/内联选中项使用主色浅底，水平导航使用其指示结构。桌面命令 `Menu` / `ContextMenu` 是另一种职责。 |
| Tabs → `TabControl` | 标准标签页以主色文字和指示线表达激活状态；`CardTabControl` 有独立的卡片式结构，不能用标准标签页规则抹掉其背景。 |
| Table → `DataGrid` | 表头与数据行层次清晰，排序、选中和 hover 各有状态；按需启用独立 DataGrid 包，遵循其数据与列 API。 |
| Tag / Badge → `Tag` / `DotBadge` 等 | Tag 表达分类，Badge 表达计数或状态；重要状态配合可理解的文字，不单独依赖色点。 |
| Alert → `Alert` | 状态图标、浅色背景和正文协同传达反馈；普通型与带描述型使用各自内边距。 |
| Tooltip / Popover → `ToolTip` / `Flyout`、`FlyoutHost` | 提示使用逆色表面，交互内容通过现有 Flyout 体系承载；定位、箭头、焦点和关闭交给控件。 |
| Dropdown item → `MenuItem` 等命令项 | hover 表示当前指针目标，保持文字可读；按适用场景使用框架已有的键盘和子菜单行为。 |

**当前实现与上游原型的对应差异：**

- 上游 Card 原型的 24 DIP 内容留白对应 AtomUI `Card` 的大号 `BodyPaddingLG`；默认中号 `BodyPadding` 为 12，小号为 8。
- 上游 Tooltip 原型内边距为纵向 6、横向 8；AtomUI `ToolTip.ContentPadding` 当前为纵向 8、横向 12。
- Button 的默认 Own Token `ContentPadding` 为 AXAML `15,4,15,4`，YAML 为 `4px 15px`，包含边框和内容行高补偿。
- Dialog 的窗口内容内边距当前为 `24,16,24,16`，overlay 内容区为 `24,0,24,0`，头尾区另有布局；不套用上游单一 Modal padding。
- 默认 `Tag` 背景为 `ColorBgLayout`；分类色变体来自对应色板。`DataGrid` 默认尺寸为 Large，表头使用合成的 `HeaderBg` 和 16 DIP 单元格内边距。

这些差异描述当前输出，不能据此修改 Ant Design 的通用原则。新界面优先使用现有控件；需要更接近上游原型时，通过
公开尺寸、Token 或专用 Semantic Part Style 明确表达，运行时行为和布局仍需验证。

## Do's and Don'ts

- **Do** 先组织任务与信息层次，再选择控件。一次明确决策突出一个主要操作，辅助操作保持从属关系。
- **Do** 使用角色 Token 描述表面、文字、边框和状态，修改少量 Seed 后让算法派生完整色阶。
- **Don't** 在页面上复制一套浅色 Brush、手工反转颜色，或用无关色板替代错误、成功等功能语义。
- **Do** 保留键盘焦点、禁用、加载、错误、选中和指针反馈，兼顾长文本、缩放和滚动。
- **Don't** 为视觉一致而删除滚动条、裁剪边界、弹层定位或控件原有交互，也不要仅凭颜色表达关键信息。
- **Do** 使用组件尺寸和派生间距，保持相邻控件的圆角与尺度关系；保留边框补偿等合理几何值。
- **Do** 用生成的专用 Semantic Part Style 定制稳定部件，并显式声明 `x:SetterTargetType`。
- **Don't** 按 `PART_*`、偶然模板层级或 Loaded 回调查找节点后设置视觉属性；完整结构变化使用公开主题入口。
- **Do** 在 ControlTheme 能等价表达时声明样式与模板绑定；`Transitions` 放在 Style Setter 中。
- **Don't** 为定制主题扫描程序集、反射 Token 属性或在资源查询时重算 Token；保留生成式注册和 AOT 边界。
- **Do** 将 YAML 视作默认视觉参考，实际实现通过 AtomUI API 消费主题，并在真实内容、状态和宿主中验证。

## Customization

Ant Design 的 Seed 派生、组件覆盖和嵌套主题能力，在 AtomUI 中通过不可变 `ThemeConfig`、Control 配置和
`ThemeConfigProvider` 表达。AXAML ControlTheme 由 Avalonia 编译；运行时计算 Token 并发布快照。
包内 `.theme.xml` 可生成类型化声明，外部动态 XML 使用受限 Reader/Binder，最终进入同一运行时计算与提交路径。

```text
ThemeDefinition + ThemeConfig + generated schema
  -> Normalize / Merge -> Seed / ordered algorithms / Map / Alias
  -> ThemeSnapshot -> scoped resource provider -> ControlTheme / visual states
  -> per-Control effective globals + Own Tokens (materialized on demand)
```

`ThemeSnapshot` 是有效 Token 值的唯一真源；主题切换先准备候选状态，再统一提交和通知，保持根与局部作用域一致。
静态模板、资源键和生成注册在切换时保持稳定。Web 的 CSS Variable、样式注入和 `zeroRuntime` 是另一种输出机制，
不能作为 AtomUI API 使用。完整架构见[主题运行时](docs/architecture/systems/theming/runtime.md)与[编译主题包](docs/architecture/systems/theming/compiled-theme-packages/overview.md)。

### 选择定制入口

| 需求 | AtomUI 入口 |
| --- | --- |
| 改品牌、基础字号、圆角和全局状态 | `ThemeConfigBuilder.WithToken`，优先覆盖 Seed |
| 暗色、紧凑及组合 | `WithAlgorithms`，当前算法身份为 `Default`、`Dark`、`Compact` |
| 仅改变某类 Control | `WithControl` + `ControlThemeConfigBuilder`，使用生成的 `XxxTokens.Identity` |
| 页面或区域建立局部主题 | `ThemeConfigProvider.Config`，绑定或替换完整不可变配置 |
| 调整稳定视觉部件 | 生成的专用 Semantic Part Style，如 `ButtonContentStyle`、`CardBodyStyle` |
| 完整替换样式、模板或可替换 public 子控件 | `ControlTheme` 或对应 Semantic Part Theme 属性 |

### 全局配置与算法

以下代码用于已经通过 `UseAtomUI` 初始化的应用；`application` 为当前 `Application`，在 UI 线程发起请求。

```csharp
using AtomUI;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.DesignTokens;

var config = new ThemeConfigBuilder()
    .WithToken(nameof(DesignToken.ColorPrimary), "#00B96B")
    .WithAlgorithms(ThemeAlgorithm.Default, ThemeAlgorithm.Dark, ThemeAlgorithm.Compact)
    .Build();
var result = await application.GetThemeManager()!.ApplyThemeAsync(
    new ThemeRequest(IThemeManager.DEFAULT_THEME_ID, config, ThemeTransitionReason.UserRequest));
// 由调用方处理 result；失败不能假定主题已经切换。
```

Token 按 key 合并；算法列表**整体替换**。只给局部 Compact 不会自动保留父 Dark，需要显式提供完整组合。
配置不支持原位修改集合后自动生效；更新时构造并替换完整值。局部 `Inherit=false` 回到库默认值，切断父配置与所选主题继承；
根请求的 `Inherit=false` 保留库默认值与当前所选主题定义，仅丢弃较低层的应用初始覆盖。

### Control 覆盖

```csharp
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.DesignTokens;

var buttonConfig = new ThemeConfigBuilder()
    .WithControl(ButtonTokens.Identity, new ControlThemeConfigBuilder()
        .WithAlgorithm(ControlAlgorithmMode.Global)
        .WithToken(nameof(DesignToken.ColorPrimary), "#00B96B")
        .Build())
    .Build();
```

该配置只改变 Button identity 的有效 Token，不创建 Button 内容树的主题作用域。Control 可覆盖自身 Own Token
和任意已注册 Global Token；Own Token 名称由生成的 `XxxTokenKey`、控件 Token 文档和 schema 确认。
`Unspecified` 继承父策略，最终默认 `Disabled`；`Disabled` 不因 Control Seed 覆盖重派生 Map/Alias；
`Global` 使用作用域算法重新派生；`Custom` 使用显式有序算法链，当前仍由上述内置算法身份组成。

### 局部作用域与资源读取

以下 AXAML 片段共用 View 根的 `xmlns="https://github.com/avaloniaui"`、`xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"`
和 `xmlns:atom="https://atomui.net"` 声明；编译绑定使用所在 View 的 `x:DataType`。`Config` 绑定 ViewModel 的不可变配置属性：

```xml
<atom:ThemeConfigProvider Config="{Binding LocalThemeConfig}">
    <atom:Button Content="保存" ButtonType="Primary" />
</atom:ThemeConfigProvider>
```

AXAML `{atom:SharedTokenResource ColorPrimary}` 读取当前 ThemeContext 的全局值；
`{atom:ButtonTokenResource ColorPrimary}` 读取 Button 的有效全局值，`ButtonTokenResource` 也可读取其 Own Token。
后者显式表达 Control 身份，不能根据 `TargetType`、继承或 `BasedOn` 推断。

普通 Popup 通过逻辑关系继承主题；独立 TopLevel 需要框架显式绑定 owner context。使用带 owner 的公开反馈入口，
无 owner 静态 Dialog、Notification 等只能使用根主题，不能从调用位置推断局部主题。

### Semantic Part 定制

```xml
<atom:Card Header="账户信息">
    <atom:Card.Styles>
        <Style Selector="atom|Card">
            <atom:CardBodyStyle x:SetterTargetType="Border">
                <Setter Property="Padding" Value="24" />
            </atom:CardBodyStyle>
        </Style>
    </atom:Card.Styles>
    <TextBlock Text="账户内容" />
</atom:Card>
```

这通过专用 Style 明确采用 24 DIP 内容留白。生成器负责模板、逻辑后代或跨视觉根的 SelectorRoute；
应用通过专用 Style 访问稳定契约，不自行拼接 route 或在 code-behind 操作目标节点。
完整契约见[Semantic Part 系统](docs/architecture/systems/theming/semantic-parts.md)，使用路径见[主题定制指南](docs/guides/theming/customization.md)。

## Sources & Maintenance

格式、设计语言和实现事实分别由以下来源维护；本文件是面向设计与应用生成的提炼，不替代完整 API 或架构文档。
上游参考核对日期为 **2026-10-26**；`version: alpha` 表示 DESIGN.md 格式版本，不表示 AtomUI 产品成熟度或包版本。

| 内容 | 正式来源 |
| --- | --- |
| 上游设计语言与使用意图 | [Ant Design DESIGN.md](https://github.com/ant-design/ant-design/blob/master/DESIGN.md)、[官方使用说明](https://ant.design/docs/react/design-md/) |
| 格式与工具规则 | [Google DESIGN.md](https://github.com/google-labs-code/design.md)、[格式规范](https://github.com/google-labs-code/design.md/blob/main/docs/spec.md) |
| 默认主题与 Seed | [DaybreakBlue](src/AtomUI.Core/Assets/Themes/DaybreakBlue.theme.xml)、[DesignToken](src/AtomUI.Core/Theme/DesignTokens/Definitions/DesignToken.cs)、[Seed 定义](src/AtomUI.Core/Theme/DesignTokens/Definitions/DesignToken.Seed.cs) |
| Token 派生及实际值顺序 | [算法](src/AtomUI.Core/Theme/Algorithms/DefaultThemeVariantCalculator.cs)、[ThemeCompiler](src/AtomUI.Core/Theme/Compilation/ThemeCompiler.cs)、[字体派生](docs/architecture/systems/typography/tokens-and-derivation.md) |
| 主题作用域、资源与发布 | [主题系统入口](docs/architecture/systems/theming/overview.md)、[资源宿主](docs/architecture/systems/theming/application-resource-host.md)、[惰性物化](docs/architecture/systems/theming/lazy-materialization.md) |
| 组件原型与 Own Token | [控件文档入口](docs/controls/desktop/overview.md)及 `src/AtomUI.Desktop.Controls` 中对应 Token / Themes；DataGrid 由独立包拥有 |

维护时遵循：

1. 默认 Seed、算法、字体、组件 Token 或主题 Setter 改变时，同步核对这里的实际值、映射、状态和差异说明。
2. 新增设计原则先核对 Ant Design 意图；平台适配说明原因，已知偏差保持明确标注，代码修复另行验证和评审。
3. 用实际快照核对派生值，用主题资产确认消费位置；不能用 Gallery 的应用覆盖代替库默认基准。
4. 新例子使用当前公开 API、生成的 Token 入口和专用 Semantic Part Style；保持 NativeAOT 友好。
5. 通过 `@google/design.md` 的 lint 检查结构与引用，检查仓库相对链接及示例编译；对比度警告结合背景与状态人工复核。
6. 中文 `DESIGN.md` 为默认版本，英文 `DESIGN.en-US.md` 同步维护；两版章节、Token、数值和代码示例保持一致。
