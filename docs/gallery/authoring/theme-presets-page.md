# Gallery 主题预设页

主题预设页复刻 ant.design 首页预览区的切换体验：一组常用控件、顶部一排预设入口，点一下整页换一种观感。它展示的
是[预设主题包](../../architecture/systems/theming/compiled-theme-packages/overview.md#11-预设主题包)，不是设计语言
切换；页面不含任何预设专有的控件代码或模板。

## 页面组成

```text
ThemePresetsShowCase
+-- 预设条        来自 IThemeManager.AvailableThemes 中由主题包注册的主题；每项一个图标与名称
+-- 正交开关      暗色、紧凑；与预设自由组合
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

## 预设图标与文案

每个预设的图标与名称由 Gallery 维护，不写在主题包里：主题包只有 `ThemeInfo` 的 Id、Name 与声明主色。Gallery 按
`ThemeId` 前缀映射图标；没有映射时用主题声明主色画一个圆点。文案进入本 ShowCase 的 XLIFF，遵守
[Gallery 组织规范](organization.md)。

## 验收

- 每个预设包单独引用时页面可切换、无异常；暗色与紧凑组合正常。
- PrimeNG 的 Aura / Lara / Nora 切换后，字段高度、圆角、按钮字重、焦点形态与
  [PrimeNG 映射表](../../architecture/systems/theming/compiled-theme-packages/overview.md#11-预设主题包)一致。
- 页面不依赖任何预设专有代码：删除全部主题包引用后页面仍可运行。

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
