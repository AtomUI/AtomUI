# AtomUI 的 PrimeNG 设计语言严格对齐方案

> 分批执行。每批先在实际 Gallery 用脚本取得失败证据，再修正控件所属的主题规则或通用控件能力，最后用同一脚本验收并运行定向测试。

## 目标与范围

AtomUI.Themes.PrimeNG 的 Aura、Lara、Nora 三个家族，各有浅色、深色、紧凑浅色、紧凑深色，共 12 套配置，尽可能还原官方最新版 PrimeNG 设计语言。默认主色沿用 primeng.dev 首次访问时的 noir，另以一种彩色种子做代表回归。以现有 AtomUI 控件、Token、ControlTheme 与 PrimeNG Token 算法为载体，保留应用对主题 Token 和语义部件的定制入口。Ant Design 与 Shadcn 的渲染保持不变。

**官方基线**（2026-11-20 在 npm 核对，与 2026-11-15 一轮相同）：primeng 22.1.5、`@primeuix/themes` 3.1.1、`@primeuix/styled` 1.2.0。收尾时重新核对，如有新版先更新本节再改实现。

**权威顺序**

1. [primeng.dev 主题文档](https://primeng.dev/theming/styled)：根字号 16px、标准预设，`*-compat` 是 14px 旧版过渡预设。
2. `@primeuix/themes` 3.1.1 发布包源码：`presets/aura|lara|nora` 的 primitive、semantic、`colorScheme.light/dark` 与各组件 Token。
3. 内置浏览器在 primeng.dev 量取计算样式与 `--p-*` 变量，作为实际渲染核对。

沿用的既有成果见 [2026-11-15 对齐进度](../progress/2026-11-15-primeng-design-language-alignment.md)、[FloatButton 阴影](../progress/2026-11-17-primeng-floatbutton-shadow.md) 与 Message／Notification 阴影记录。bugfix/shadcn 引入的通用能力（显式 Token 探测、控件外观设置、输入框透明／无边框／仅下边框外观、按钮外观配方、阴影过渡、焦点环越界修正）优先复用。

## 控件映射

| 类别 | AtomUI 控件 | PrimeNG 组件 |
| --- | --- | --- |
| 按钮 | Button、DropdownButton、SplitButton、FloatButton、ButtonSpinner | Button（severity、outlined、text、link、raised、rounded）、SplitButton、SpeedDial、InputNumber 按钮 |
| 表单 | LineEdit、TextArea、SearchEdit、NumericUpDown、OtpLineEdit、Mentions、AutoComplete | InputText、Textarea、IconField、InputNumber、InputOtp、AutoComplete |
| 选择 | Select、TreeSelect、Cascader、ComboBox、CheckBox、RadioButton、ToggleSwitch、Slider、Rate、Segmented、ColorPicker | Select、MultiSelect、TreeSelect、CascadeSelect、Checkbox、RadioButton、ToggleSwitch、Slider、Rating、SelectButton、ColorPicker |
| 日期时间 | DatePicker、RangeDatePicker、TimePicker、Calendar | DatePicker（含时间与内联） |
| 数据 | DataGrid、ListView、ListBox、TreeView、Transfer、Pagination、Tag、Badge、Avatar、Image、Timeline | Table、Listbox、Tree、PickList、Paginator、Tag、Badge、OverlayBadge、Avatar、Image、Timeline |
| 容器 | Card、Collapse、Expander、GroupBox、TabControl、TabStrip、Splitter、Separator | Card、Accordion、Panel、Fieldset、Tabs、Splitter、Divider |
| 导航 | Menu、ContextMenu、NavMenu、Breadcrumb、Steps、DropdownButton 菜单 | Menu、Menubar、ContextMenu、PanelMenu、TieredMenu、Breadcrumb、Stepper |
| 反馈与浮层 | Dialog、Drawer、Message、Notification、Alert、Tooltip、Popover、PopupConfirm、ProgressBar、Spin、Skeleton、Upload | Dialog、Drawer、Toast、Message、Tooltip、Popover、ConfirmPopup、ProgressBar、ProgressSpinner、Skeleton、FileUpload |

官方没有对应组件的控件（全屏 Calendar、LunarCalendar、Tour、Statistic、Descriptions、Result、Empty、Watermark、QRCode、BorderBeam、Masonry 等）按 PrimeUIX 的语义层推导：表面色阶、`content`、`formField`、`overlay`、`list`、`navigation`、`mask`、`focusRing`。这些推导与 PrimeUIX 本身缺失的紧凑密度一起登记为不等价项，写明推导来源。

## 阶段

### P0 基线与对照表

- [ ] 获取 3.1.1 发布包（放在临时目录，只当数据读取），按"组件 × 预设 × 浅深色"导出取值表：颜色、尺寸、圆角、内边距、字号字重、边框、阴影、焦点环、过渡。
- [ ] 与 AtomUI 当前 12 套配置的有效 Token 逐项比较，生成差异清单。
- [ ] 内置浏览器在 primeng.dev 抽查每个组件的计算样式，确认取值表与官方实际渲染一致。

### P1 Gallery 全量脚本巡检

- [ ] 覆盖 Gallery 全部 showcase 路由与每个示例，不抽查。矩阵为 Aura／Lara／Nora × 浅深色 6 组全量，紧凑 6 组与彩色种子做全路由代表回归。
- [ ] 每个控件驱动其支持的状态：常态、悬停、按下、聚焦（真实 Tab）、选中、禁用、只读、错误／警告、展开弹层、动画结束。
- [ ] 与 primeng.dev 对应组件的同状态并排比较，缺陷逐项登记：配置、路由、示例、状态、区域坐标、实测值、官方值。

### P2 分批修复

1. 全局规则：字号行高、焦点环、表单字段（内边距、三态边框、filled 变体、禁用与只读底色）、弹层、列表选项、导航项、遮罩。
2. 按钮家族：severity 与 outlined、text、link、raised、rounded 的映射，SplitButton、FloatButton、ButtonSpinner。
3. 表单与选择控件。
4. 日期时间与数据控件。
5. 容器、导航、反馈与浮层。
6. 无对应组件的控件按推导规则统一。

### P3 每批验收

- 实际 Gallery 使用 `atom:Window` 宿主，真实指针与键盘，`screencapture -l` 原生截图。先过有效性门：同状态复拍零差异，1 DIP 扰动可检出，复位后零差异。
- 区域对比按项目的视觉验收容差判定：功能正确、关键区域与官方取值一致、通道差 ≤ 8 的边缘差异单独记录。
- Ant Design 与 Shadcn 的同路由同状态做前后对照，要求零像素差异。
- 运行受影响控件的定向测试。

### P4 收尾

- 新代码的性能与资源泄露审查：视觉零差异，所有订阅、绑定和缓存都有释放路径。
- 全量回归，与基线失败清单比较。
- 更新 PrimeNG README、控件 Token 文档、changelog、架构文档与验收记录。
- 按控件分组提交，合并方式由用户决定。

## 工程边界

只修改 PrimeNG Token 算法与控件中通用、可配置的能力，不在控件里写死 PrimeNG 分支，不为 Gallery 做视觉补丁。新增公开 Token 或属性须保持 Ant Design 默认值不变。显式应用资源、局部资源与 ThemeConfigProvider 的覆盖优先级是交付条件。临时驱动、截图与临时用例按项目规则清理，`docs/developments` 只保存 Markdown。

## 进展与验收记录（2026-11-20）

### 已完成

- 基线：primeng 22.1.5、@primeuix/themes 3.1.1、@primeuix/styles 3.1.1，noir 默认主色；官方 Token 表解析为每预设 × 明暗的平面表，
  与 AtomUI Token 导出逐项比对（80 项一致，Lara 焦点环 2 项为预期差异：AtomUI 以 3.2px 环近似 Lara 的光晕）。
- 全局：状态种子（green / orange / sky 500）、Nora 规则表修正、焦点环宽度与偏移、`ControlItemBgHover`、卡片圆角与阴影、
  Lara 弹窗阴影。
- 控件阶段：表单字段、按钮族、选择类、日期时间、数据展示、容器与导航、反馈与浮层，按家族分文件实现（详见包 README
  「Control stages of all presets」）。
- 控件能力（新增 Own Token，默认值等于原外观）：按钮 recipe 拆分开关、Pagination 页码项、Collapse / Expander 头部颜色与字重、
  DateViewer 单元格圆角 / 今天填充 / 标题与星期行、日期时间弹层箭头开关、Tag / CountBadge / Tabs / GroupBox / Steps /
  菜单分组标题字重、Alert 严重级文字色与字重、CheckBox / RadioButton 悬停边框、Slider 滑块填充。
- 2026-11-21 Small 输入框高度统一（所有主题）：输入框文本行高统一为 `FontHeight`，PrimeNG `InputPaddingSM` 按它推导，ComboBox 去掉
  旧补偿外边距；Small LineEdit / NumericUpDown / ComboBox / Select / 选择器全部等于 `ControlHeightSM`（Ant 24、Aura 28、Lara 32）。
  ColorPicker 输入行对齐，Alpha 宽度保持 30px 文本视口（PrimeNG 50px），修复 "100%" 被裁切。
- 2026-11-21 原生窗口验收中发现并修复：SearchEdit 主题切换后高度只增不减（按钮高度绑定自身边界的反馈环）；PrimeNG Tooltip
  改用自己的 7px 无描边箭头（此前共用 Popover 的 19px 描边箭头，偏大且交界有浅色缝）。
- 2026-11-21 Small 字号对齐：SearchEdit（新增 `SearchEditToken`，并补上 form field 规则）、NumericUpDown、Select / Cascader /
  TreeSelect 新增 `InputFontSizeSM`（默认 `FontSize`），PrimeNG 取 `FontSizeSM` 12px。箭头几何按 antd `genRoundedArrow` 重做
  （直角尖、真圆弧尖角、底部内凹衔接、描边不含底边），PrimeNG Tooltip 箭头 8×4 直角、`ToolTipToken.MarginToAnchor` 0.5，
  主体离锚点 4px；Popover 箭头 `2 × gutter` 直角。PrimeNG 微调手柄图标 10.5、列宽 = 图标 + 两侧 `InputPadding.Left`。
- 2026-11-21 primeng.dev 实测对齐：进度条默认（Large）18、等高行；Aura Select 选项圆角 4 / 间距 2；Aura Toast 1px 边框
  （Notification / Message）。Nora 浅色弹层与卡片加 surface 200 边框（有意偏离官方透明边框），Nora 卡片图标取实底色。
- 2026-11-21 Toast 类型表面：NotificationCard / MessageCard 新增类型背景、边框、图标、文字、关闭图标 Token（默认等于原外观）；
  PrimeNG Aura / Lara 浅色着底，Lara / Nora 左侧 6px 色条，Nora 实底。原生弹层右侧 1px 透明缝修复（`ShadowsAwareContainer`
  在框体 Bounds 变化时重新排版，阴影裁切不再停在旧宽度）。
- 2026-11-21 Nora 动画：PrimeUIX `transition.duration` 0s 只管状态颜色，组件进出场动画（`@primeuix/styles` 固定值）所有预设
  一致。Nora 去掉 profile 的 `EnableMotion false`，全局 `MotionDuration*` 置 0，由 `PrimeNGMotion` 在弹层、对话框、抽屉、
  折叠、Tabs、走马灯、通知、Tooltip、开关、Spin 的作用域内恢复各自时长。
- 2026-11-21 逐项修复（用户 review）：Nora 下拉面板间隙 2、Nora placeholder surface 500、Splitter 手柄三个预设可见、Nora 描边
  OptionButton 选中填充、Transfer 行间 2px、通知无类型卡片三个预设描边且色条改为内阴影（修复 Lara / Nora 卡片丢失阴影与描边）、
  PrimeNG Slider 手柄尺寸包含焦点环（修复拖动时变方）。Collapse Nora 标题实测 51/52（官方 51，差 1px 来自 22px 行高）。
- 2026-11-21 Collapse 标题与 Expander 等高（用户要求，偏离 accordion 16 内边距）；Aura 下 Cascader、AutoComplete 选项圆角 4、
  行间 2px 与 Select 对齐（新增 `OptionMargin` / `OptionBorderRadius`），TreeSelect 面板内边距 4 4 4 2。
- 2026-11-21 Select 系下拉间隙三个预设统一与 DropdownButton 对齐（实测均为 4 DIP 可见间隙），AutoComplete 候选行高与 Select 选项逐行对齐。
- 2026-11-21 placeholder 三个预设统一更浅：浅色 surface 400、暗色 surface 500（官方为 muted 文字色）；列表勾选图标改取选中文字色。

### 验收方式

- 页面巡检：Gallery 全部 79 个路由 × 6 个 PrimeNG profile 渲染整页截图（atom:Window 宿主），0 个路由失败；按家族生成对照拼图，
  与 primeng.dev 在线演示的实测尺寸对照（Paginator、Menu、InputNumber、DatePicker 等）。
- 弹层巡检：在各自 Gallery 页面打开 DatePicker、RangeDatePicker、TimePicker、RangeTimePicker、Select、ComboBox、Cascader、
  TreeSelect、AutoComplete、Mentions 的弹层截图，6 个 PrimeNG profile 与 Ant 对照。
- 回归：每次控件能力改动后，Ant（DaybreakBlue 明 / 暗）与 Shadcn（Luma、Sera、Vega.Dark）受影响路由与改动前逐像素比对；
  已知动画路由（Alert 跑马灯、Tag、Spin、Skeleton 等）以 A/A 对照确认为噪声。巡检中发现并修正一处回归：Alert 字重最初设在
  标题部件上，挡住了 section 语义样式继承的字重，改为设在 Alert 自身。

### 已知差异（未实现）

- 日期面板：月 / 年分为两个按钮、圆形前后翻页按钮、范围选择逐格圆形高亮、页脚 Today / Clear 次要文本按钮、其他月份 0.6 透明度
  （当前为 surface 500 文本）。
- Rating 描边星形、Menubar 外框、Tabs 的 Lara / Nora 特有样式、SelectButton 各预设差异、FloatButton 对应 SpeedDial 尺寸、
  输入框 Aura 静止阴影。
- DataGrid 位于独立程序集，PrimeNG 包未引用，未纳入本轮。
