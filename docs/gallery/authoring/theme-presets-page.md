# Gallery 主题预设页

主题预设页复刻 ant.design 首页预览区的切换体验：一组常用控件、顶部一排预设入口，点一下整页换一种观感。它展示的
是[预设主题包](../../architecture/systems/theming/compiled-theme-packages/overview.md#11-预设主题包)，不是设计语言
切换；页面不含任何预设专有的控件代码或模板。

## 页面组成

```text
ThemePresetsShowCase
+-- 预设条        来自 IThemeManager.AvailableThemes 中由主题包注册的主题；每项一个图标与名称
+-- 正交开关      暗色、紧凑；与预设自由组合
+-- 主色色块      五个 Tailwind 500 色块（PrimeNG 预设前置 Noir）+ 预览定义自身种子；"恢复预设主色"清除选择；固定主色状态的预设禁用并说明
+-- 预览区        ThemeConfigProvider 包裹的固定控件集合
\-- 复制主题      导出预览区当前有效主题为 XML v1 文本
```

预览区的控件集合固定，覆盖主要家族：Button（各 variant）、LineEdit / NumericUpDown / Select / DatePicker、
Checkbox / Radio / Switch / Slider、Tabs、Steps、Progress、Tag / Badge / Alert、Card、Table（DataGrid 已启用时）、
Dialog 与 Notification 触发按钮。集合变化要同步本文。

## 行为

- 预设条只列出来自主题包的主题，不列用户目录主题；无主题包时只显示 DaybreakBlue 与 Gallery 自带色板主题。
- 点击预设将对应主题的完整配置应用到预览区的 `ThemeConfigProvider`，不改变 Gallery 全局主题；全局主题切换仍在设置页。
- 暗色、紧凑开关选择同包的模式 profile，Provider 使用该 profile 自己的算法链（例如 `PrimeNG:Aura.Dark`）；只有缺少对应 profile 的主题才改写为内置 Dark / Compact 算法。
- 页面切换预设时不重建控件树；主题事务保证预览区不出现新旧状态混合帧。
- "复制主题"导出的是预览区 Provider 的有效配置，可直接放进用户主题目录。

## 主色色块

色块演示主题算法跟随种子：选中一个色块，只把预览配置里的 `ColorPrimary` 种子换成该颜色，其余声明、算法链与
模式 profile 不变。主题没有显式声明的派生色（悬停、按下、焦点环、选中底色等）由算法从新种子重新推导；
主题显式声明的值优先于算法，不会跟随种子（见下文"固定主色状态的预设"）。Progress、"processing" 状态的 Tag 与
Info 类型的 Alert 用的是信息色（`ColorInfo`），色块不改变它，所以这几处颜色不随色块变化。

以下"预览定义"指预览区实际编译的定义：存在对应模式 profile 时是该 profile（如 `PrimeNGAura.Dark`），否则是基础定义。

- 色块固定为 Tailwind 500 色阶：翡翠绿 `#10B981`、蓝 `#3B82F6`、紫罗兰 `#8B5CF6`、玫瑰红 `#F43F5E`、
  琥珀 `#F59E0B`（与 `PrimeNGTailwindPalettes` 的 500 档一致）；预览定义的设计语言为 `PrimeNG.*` 时最前面另有
  Noir（墨黑）`#020617`，即 primeng.dev 的默认主色，也是 PrimeNG 预设自身的种子。之后是预览定义自身的种子，即它声明的
  `ColorPrimary`（未声明时为 Ant 默认 `#1677FF`）；与前五个重复时不再列出。自身种子随预设、暗色、紧凑切换而重算，
  例如 PrimeNG Aura 的 noir 种子亮色为 `#020617`，暗色为 `#FAFAFA`。PrimeNG 算法把种子当作 500 档，命中 Tailwind 500 时取转录色阶，
  其它颜色走生成色阶；内置 Ant 主题只声明 `ColorPrimary`，色块同样设置它们的种子。
- 视图模型：`PrimarySwatches`（颜色列表）、`PrimarySwatchItems`（带本地化名称与选中状态的条目）、
  `CanChoosePrimary`（预览定义是否允许选色）、`SelectedPrimary`（`null` 表示使用预览定义自身种子，此时预览配置与
  未加色块前完全相同，是同一个缓存实例）、`ResetPrimary` 命令（清除选择）。
- 选择等于当前自身种子的颜色等同于清除（`SelectedPrimary` 保持 `null`，不产生新配置）。预览定义变化时，选中的五个
  固定色块之一保留；其余选择（上一个定义的自身种子等）回退为 `null`。
- 选中标记表示当前生效的种子（预览定义自身种子或所选色块），不一定等于渲染出的主色：PrimeNG 的 Aura 与 Lara 暗色
  取该种子色阶的 400 档，Nora 亮色取 600 档，Nora 暗色取种子本身（500 档）。
- 每个色块是一个单选按钮：可 Tab 聚焦、空格选中，自动化名称与提示为颜色名称（进入本页 XLIFF）；
  色块填充色是种子数据，不是主题 Token。色块行使用与相邻行相同的 `WrapPanel`，窄宽度下换行。
- 复制主题时选中的主色作为 `ColorPrimary` Token 写进 XML（替换种子，不重复），导出仍通过主题 XSD 校验。

### 固定主色状态的预设

预览定义只要显式声明了 `ColorPrimary` 以外的任何 `ColorPrimary*` Token（全局或控件级，如 `ColorPrimaryHover`、
`ColorPrimaryBg`、`ColorPrimaryBorder`），这些状态就不会跟随种子，换种子只会得到不协调的预览。此时
`CanChoosePrimary` 为 `false`：色块行仍显示但禁用，旁边显示一行本地化说明，`SelectedPrimary` 为 `null` 且设置无效，
"恢复预设主色"按钮禁用。判断只看预览定义的 Token，不看包名或主题名。当前 Shadcn 的 Zinc / Slate / Stone 所有
profile 都属于这种情况；切回其它预设后色块行恢复可用。

## 预设图标与文案

每个预设的图标与名称由 Gallery 维护，不写在主题包里：主题包只有 `ThemeInfo` 的 Id、Name 与声明主色。Gallery 按
`ThemeId` 前缀映射图标；没有映射时用主题声明主色画一个圆点。文案进入本 ShowCase 的 XLIFF，遵守
[Gallery 组织规范](organization.md)。

## 验收

- 每个预设包单独引用时页面可切换、无异常；暗色与紧凑组合正常。
- PrimeNG 的 Aura / Lara / Nora 切换后，字段高度、圆角、按钮字重、焦点形态与
  [PrimeNG 映射表](../../architecture/systems/theming/compiled-theme-packages/overview.md#11-预设主题包)一致。
- 页面不依赖任何预设专有代码：删除全部主题包引用后页面仍可运行。
- PrimeNG Aura 选蓝色色块后，主按钮悬停 `#2563EB`、按下 `#1D4ED8`，焦点边框等于主色；暗色下主色 `#60A5FA`、
  悬停 `#93C5FD`；清除后回到翡翠绿种子。
- Shadcn 预设的色块行禁用并显示说明；PrimeNG 切换暗色时自身种子色块随 profile 重算（如 Aura 的 noir 由 `#020617` 变为 `#FAFAFA`）并显示为选中。

## 模式 profile 与局部隔离

同一编译主题包中，存在基础 ID 时，`.Dark`、`.Compact`、`.DarkCompact` 定义属于该基础预设族，
预设条只显示基础入口。选择身份始终保持基础 ID；模式组合选择同包实际存在的 profile 并沿用它的算法链，缺失时使用
基础主题与既有 Dark / Compact 算法。此规则不依赖厂商名称；图标映射是唯一按名称前缀区分的展示逻辑。

`ThemeInfo` 和 `ThemeConfigProvider` 目前没有公开的包来源与局部 ThemeId 输入。Gallery 的内部适配器集中读取
已提交编译 Catalog 的包切片及定义 revision，将完整声明的全局、控件和 Own Token 转为 `Inherit=false` 的
既有 `ThemeConfig`；不会临时切换根主题。编译 Catalog 更新时清空配置缓存。该内部桥接的维护边界限定在
Gallery，不扩展 Core 公开 API，也不在运行时读取 XML 或反射发现主题包。

复制内容包含当前实际 profile ID、外观、算法和完整局部声明，值经现有 Token formatter 规范化，再组成 XML v1；
没有控件级声明时不输出 `Controls`（XSD 要求其中至少有一个 `Control`）。
例如 Aura 暗色紧凑模式复制为 `PrimeNGAura.DarkCompact`，重新读取会得到相同的算法派生结果。
