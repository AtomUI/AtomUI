# Gallery 主题预设页

主题预设页用一组常用控件展示 PrimeNG、Shadcn 的语言风格与已注册的编译主题。选择入口会更新页面内的控件预览，
Gallery 的全局主题仍由主菜单控制。

## 页面组成

```text
ThemePresetsShowCase
+-- 预设条        PrimeNG 与 Shadcn 风格、编译主题包中的具名主题；每项一个图标与名称
+-- 正交开关      暗色、紧凑；与预设自由组合
+-- 主色色块      语言的主色色块与预设自身种子；"恢复预设主色"清除选择；固定主色状态的预设禁用并说明
+-- 基础色块      Shadcn 的七组 BaseColor 色块
+-- 预览区        ThemeConfigProvider 包裹的固定控件集合
\-- 复制主题      导出预览区当前有效主题为 XML v2 文本
```

预览区的控件集合固定，覆盖主要家族：Button（各 variant）、LineEdit / NumericUpDown / Select / DatePicker、
Checkbox / Radio / Switch / Slider、Tabs、Steps、Progress、Tag / Badge / Alert、Card、Table（DataGrid 已启用时）、
Dialog 与 Notification 触发按钮。集合变化要同步本文。

## 行为

- 随 Gallery 启用的语言提供 11 个风格入口，依次为 PrimeNG Aura、Lara、Nora 和 Shadcn Luma、Lyra、Maia、Mira、
  Nova、Rhea、Sera、Vega。两种语言都启用时默认预览 PrimeNG Aura。已注册的包内具名主题排在语言风格之后。
  没有语言风格入口时，内置的 Daybreak Blue 也显示在预设条上。
- 点击入口会为预览区的 `ThemeConfigProvider` 构造局部配置。语言风格使用该语言的隐式定义，
  具名主题使用其编译后的声明。预设条的选择身份在两者之间互相独立。
- 暗色、紧凑开关写入预览配置的 `Appearance` 与 `Density`：v2 的一个主题覆盖全部变体，变体差异由 `<When>` 条件段表达，
  不再有 `.Dark`、`.Compact` 模式 profile。主题自己固定了明暗或密度时以主题为准；语言没有声明 `DesignDensity.Compact` 时
  紧凑开关不起作用。
- 页面切换预设时不重建控件树；主题事务保证预览区不出现新旧状态混合帧。
- "复制主题"导出的是预览区 Provider 的有效配置，可直接放进用户主题目录。

## 主色色块

色块演示设计语言的推导跟随种子：选中一个色块，只把预览配置里的 `ColorPrimary` 种子换成该颜色，其余声明与变体
不变。主题没有显式声明的派生色（悬停、按下、焦点环、选中底色等）由语言的全局推导从新种子重新推导；
主题显式声明的值优先于推导，不会跟随种子（见下文"固定主色状态的预设"）。Progress、"processing" 状态的 Tag 与
Info 类型的 Alert 用的是信息色（`ColorInfo`），色块不改变它，所以这几处颜色不随色块变化。

预览定义提供当前明暗与密度下的取值。具名主题合并基础段与匹配的 `<When>` 条件段，语言风格直接使用注册的推导规则。

- 色块固定为 Tailwind 500 色阶：翡翠绿 `#10B981`、蓝 `#3B82F6`、紫罗兰 `#8B5CF6`、玫瑰红 `#F43F5E`、
  琥珀 `#F59E0B`（与 `PrimeNGTailwindPalettes` 的 500 档一致）；预览定义的设计语言为 PrimeNG 时最前面另有
  Noir（墨黑）`#020617`，即 primeng.dev 的默认主色。之后是预览主题自身的种子，即它在基础段声明的 `ColorPrimary`
  （`ThemeInfo.AccentColor`，未声明时使用该语言的默认主色）；与前五个重复时不再列出。自身种子只随预设切换，不随暗色、
  紧凑开关变化（条件段里的 `ColorPrimary` 不进入 `AccentColor`）。PrimeNG 的推导把种子当作 500 档，命中 Tailwind 500 时取
  转录色阶，其它颜色走生成色阶；Ant Design 的 Daybreak Blue 只声明 `ColorPrimary`，色块同样设置它的种子。
- 视图模型：`PrimarySwatches`（颜色列表）、`PrimarySwatchItems`（带本地化名称与选中状态的条目）、
  `CanChoosePrimary`（预览主题是否允许选色）、`SelectedPrimary`（`null` 表示使用预览定义自身种子，此时预览配置与
  未加色块前完全相同，是同一个缓存实例）、`ResetPrimary` 命令（清除选择）。
- 选择等于当前自身种子的颜色等同于清除（`SelectedPrimary` 保持 `null`，不产生新配置）。预览定义变化时，选中的五个
  固定色块之一保留；其余选择（上一个定义的自身种子等）回退为 `null`。
- 选中标记表示当前生效的种子（预览定义自身种子或所选色块），不一定等于渲染出的主色：PrimeNG 的 Aura 与 Lara 暗色
  取该种子色阶的 400 档，Nora 亮色取 600 档，Nora 暗色取种子本身（500 档）。
- 每个色块是一个单选按钮：可 Tab 聚焦、空格选中，自动化名称与提示为颜色名称（进入本页 XLIFF）；
  色块填充色是种子数据，不是主题 Token。色块行使用与相邻行相同的 `WrapPanel`，窄宽度下换行。
- 复制主题时选中的主色作为 `ColorPrimary` Token 写进 XML（替换种子，不重复），导出仍通过主题 XSD 校验。
- Shadcn 另外提供 Neutral、Stone、Zinc、Mauve、Olive、Mist、Taupe 七个基础色。主题未声明基础色时选中语言默认的
  Neutral。更改后会将 `BaseColor` 写入局部预览及导出的 `LanguageTokens`。

### 固定主色状态的预设

`CanChoosePrimary` 只在两条都满足时为 `true`：预览主题的语言把 `ColorPrimary` 声明为可调种子
（`DesignLanguageInfo.AdjustableSeeds`），且主题没有用 `Locked="true"` 锁定它（`ThemeInfo.LockedSeeds`）。主题显式声明了由
`ColorPrimary` 派生的状态（`ColorPrimaryHover`、`ColorPrimaryBg`、`ColorPrimaryBorder` 等）时，这些状态不会跟随种子，主题作者
应同时锁定 `ColorPrimary`；未锁定时 Binder 给出 `ATMTHM3006` 警告。`CanChoosePrimary` 为 `false` 时色块行仍显示但禁用，旁边
显示一行本地化说明，`SelectedPrimary` 为 `null` 且设置无效，"恢复预设主色"按钮禁用。判断只看语言的可调种子与主题的锁定
声明。切回其它预设后色块行恢复可用。基础色行按同一规则判断 Shadcn 的 `BaseColor` 是否可调。

## 预设图标与文案

语言风格入口名沿用原有的语言及风格拼写。具名主题使用 `ThemeInfo.Name`。Gallery 按预览 ID 前缀映射图标，
没有映射时用主题声明主色画一个圆点。页面通用文案进入本 ShowCase 的 XLIFF，遵守
[Gallery 组织规范](organization.md)。

## 验收

- 两种语言启用时显示原有的 11 个入口及默认 Aura。每个入口支持暗色和紧凑的组合。
- PrimeNG 的 Aura / Lara / Nora 切换后，字段高度、圆角、按钮字重、焦点形态与
  [PrimeNG 映射表](../../architecture/systems/theming/compiled-theme-packages/overview.md#11-语言包自带的具名主题)一致。
- 语言风格由已注册的语言提供，独立于对应的具名主题包。具名主题仍可携带自定义取值和锁定的种子。
- PrimeNG Aura 选蓝色色块后，主按钮悬停 `#2563EB`、按下 `#1D4ED8`，焦点边框等于主色；暗色下主色 `#60A5FA`、
  悬停 `#93C5FD`。清除后回到 Noir 种子。
- 锁定了 `ColorPrimary` 的预设色块行禁用并显示说明；切换暗色、紧凑时自身种子色块保持不变并仍显示为选中。

## 变体与局部隔离

一个 v2 主题覆盖它所属语言的全部变体。暗色、紧凑开关在选中的局部预览内调整变体，预览名称保持稳定。

`ThemeInfo` 和 `ThemeConfigProvider` 目前没有公开的包来源与局部 ThemeId 输入。Gallery 的内部适配器
`ThemePresetCatalog` 从已提交编译 Catalog 读取注册的语言风格和具名主题，为所选入口构造 `Inherit=false` 的 `ThemeConfig`。
配置写入语言、风格、明暗、密度及所选的可调种子。具名主题还在配置中写入该变体所匹配的全局 Token、语言 Token 和控件条目。
编译 Catalog 更新时清空配置缓存。预览配置只提交到页面的 Provider。

复制内容是预览配置对应的 v2 主题：`Language`、`Style`、`Appearance`、`Density` 根属性，`Tokens`、`LanguageTokens` 与
`Controls`，值经现有 Token formatter 规范化；没有控件级声明时不输出 `Controls`（XSD 要求其中至少有一个 `Control`）。导出
固定了当时的明暗与密度，保留具名主题的种子锁定声明。语言风格的导出 ID 与已注册的主题冲突时，使用可用的
`.Preview` 后缀，必要时再加序号。格式见[主题定义 XML v2](../../reference/theming/theme-definition-xml-v2.md)。
