# Shadcn 暗色 Gallery 视觉验收

本文记录暗色巡检批次的取证。Button 与输入边框的后续调整见 [Shadcn 严格对齐实施记录](2026-11-20-shadcn-strict-alignment.md)。

## 覆盖

实际 Gallery 的 Components 清点为 74 个控件路由及 Palette、ThemePresets、CustomizeTheme 三个主题路由。各路由以 Nova、Vega、Maia、Lyra、Mira、Luma、Sera、Rhea 的默认暗色配方驱动，核验主题已提交、正确示例已物化并进入可见区。

| 范围 | 证据 |
| --- | --- |
| Gallery 静态巡检 | 616／616 路由与风格组合，3901 个 Skia 视口画面。逐风格滚动覆盖 770 个示例卡，共 6160 次示例卡取证，以及 512 个 Semantic Parts 路由页签。逐页查看首屏对照并回看异常和相关样例。Icon 检查 Outlined／Filled／TwoTone 代表视口，Palette 与 ThemePresets 分别走原示例的外观入口。 |
| 数据录入 | 24 个真实输入场景 × 8 风格，共 192 组，覆盖输入、选择、勾选、禁用、确认、悬停和弹层。包括 AutoComplete 的实际建议、Cascader 叶节点、DatePicker／TimePicker、Select 含禁用项、TreeSelect、Mentions、ColorPicker、LineEdit／Password／SearchEdit／TextArea／OTP、CheckBox／RadioButton／ToggleSwitch、NumericUpDown、Rate、Slider、Transfer、Form 与 Upload 触发。Luma 原生补测 11 组，另测自定义品牌色的两个选中标记。Upload 的交互取证为触发器悬停。 |
| 导航、通用和反馈 | 14 个真实路由 × 8 风格，共 112 组、440 个稳定 Skia 状态帧。检查 Button、ComboBox、DropdownButton／SplitButton、Menu、分页、步骤、两种标签、Drawer／Modal／PopupConfirm、Message／Notification 的相应状态。Luma 原生复验包括独立 PopupRoot、控件状态以及实际打开与关闭。 |
| 数据展示 | Segmented、List、TreeView、DataGrid、Calendar 的同一目标分别采集普通、悬停、选中，8 风格共 40 组。DataGrid 采用示例的 Row Trigger；TreeView 在原 Basic 的可选 leaf 上逐风格真实点击。Descriptions Basic 的同一标签在 8 风格做了前后像素取样。 |
| Spinner 手柄与前序场景 | 两种 Spinner 的全风格、双外观、左右手柄和上下按压，128 个 Skia 及 24 个原生按压场景，另见[独立记录](2026-11-20-navigation-visual-refinements.md#buttonspinner-与-numericupdown-手柄全矩阵)。NavMenu、ToggleSwitch 的跨风格状态及弹层取证同见该记录。 |

静态巡检为修复前资产审查，修复后对受影响的原路径做定向重放。像素复拍校准示例为静态 Skia 0／16861／0、录入 Skia 0／2043／0 与原生 0／2160／0、导航原生 0／52730／0、数据展示 Skia 0／1331／0。三值依次为同状态、1 DIP 扰动及复位后的变化像素数。

## 已修复的异常与取证

| 范围 | 原场景前后证据 |
| --- | --- |
| 输入框底色 | 暗色 Input 语义色保留原有 Alpha，再按风格应用叠加比例。AutoComplete、Cascader、DatePicker、TimePicker、Form、LineEdit、Mentions、Select 的 64 个路由与风格组合按同一脚本前后采集。限定像素断言由 0／16 到 16／16。中心底色从 `#545454` 调至 Nova／Vega／Maia／Lyra／Mira 的 `#151515`、Luma／Rhea 的 `#1C1C1C`，Sera 为透明，在示例背景呈 `#0A0A0A`。占位文字 `#A1A1A1` 的对比度由 2.93:1 提升到 6.60–7.66:1。Luma 原生 Select、LineEdit 复拍吻合。 |
| CheckBox 与 RadioButton | 白标记配浅色选中填充时只有 1.26:1。八风格实际勾选和点选后，标记改用与主色配对的前景，`#171717` 对 `#E5E5E5` 实测 14.23:1，16／16 个样本通过。Luma 原生及自定义色 `#777200` 的 6.18:1 也通过。 |
| Button 与 DropdownButton | 暗色 Link 默认／悬停／按下改为 `#669BE6`／`#81ACEA`／`#5D8CD0`，实测 6.97／8.51／5.78:1。Danger 描边与虚线按钮悬停从 2.19 到 6.45:1，按下从 3.23 到 5.68:1。DropdownButton 实际弹层与悬停行的原生对照逐像素一致。 |
| ComboBox | Basic 普通／悬停／选中填充分别为 `#171717`／`#262626`／`#444444`，选中文字为 9.33:1。各主题真实选择一行后打开弹层并悬停相邻行，Luma 原生复验通过。 |
| Steps Filled Panel | 普通当前项的大面积背景由 `#E5E5E5` 改为 `#484848`，文字 8.76:1；错误当前项为 `#6B2D30`，文字 9.85:1。八风格切换当前项验证，以及 Luma 原生复验通过。 |
| TreeView 与 Calendar | TreeView Basic 的 leaf 在悬停和选中间八风格均为 0 个像素差，Calendar 全尺寸日期共用 `#262626` 底色。分离后，两者均为悬停 `#262626`、选中 `#444444`，16／16 组原路径重放及精确像素断言通过。TreeView 选中区拾取 2946–5062 个目标色实像素，Calendar 全尺寸日期为 55657–61537 个。 |
| Gallery 说明和 Descriptions | 页首说明与 Descriptions Basic “UserName” 标签，由 `#565656` 改用描述色 `#A1A1A1`。原 `#0A0A0A` 背景保留，精确像素对比从 2.70 到 7.66:1。Descriptions 八风格原标签的实色字形覆盖数前后相同。 |
| Gallery 示例 | FloatButton Basic／Placement 的大块纯白承载面改为主题容器色；Statistic In Card 底板 `#F0F2F5` 改为暗色 `#262626`；Form 自定义语义示例的 Username／Email 标签由 1.57 提升到 7.66:1。Nova、Luma、Sera 的匹配脚本共 9／9 路由复拍，15／15 像素断言通过。 |

## 仍发现的颜色与覆盖事项

| 位置 | 实测 |
| --- | --- |
| Steps Filled Panel 的非当前项 | 实际指针命中且出现悬停状态，目标区域变化为 0 像素。 |
| Select 展开图标 | Luma 原生展开状态为 `#565656`／`#1C1C1C`，输入面已修复，图标实测 2.32:1。 |
| Card 的 More、Upload 文件名及 DataGrid 的部分链接 | 仍能看到 `#1668DC`。该颜色在 `#171717`／`#262626` 背景的对比分别为 3.46／2.92:1。Button／DropdownButton 的 Link 是已定向验证的独立样式。 |
| Tag 和农历辅助文本 | 部分实色预设紫色、极客蓝的文字约 2.17／2.51:1。农历当前月辅助文本样本为 `#565656`／`#0A0A0A`，2.70:1。 |
| 显式语义部件示例 | Menu、Transfer、DataGrid、GroupBox 等示例的部分指定粉彩底与亮字约 1.02–1.09:1，属于示例局部样式。纯图像、二维码、色板和主按钮强调色按示例语义检查。 |
| 专项边界 | Sera 输入边框方向及 Luma／Rhea 透明边框规则需独立的布局取证。部分 Avatar、Masonry、ImagePreviewer 外部资源当时仍显示占位。静态巡检和上表列出的交互状态分别计入各自覆盖。 |

数据展示状态补记。Segmented 的悬停可见文字色变化，选中底为 `#171717`、普通底为 `#262626`。List 与 DataGrid 的主行填充在悬停和选中均为 `#262626`，List 的选中文字和指示样式有实测差异，DataGrid 选中勾选框也在实际点击后明确显示。

## 定向验证与处置

合批 Gallery 构建和最后的 Shadcn 模块构建均通过。去掉临时测试后，Shadcn 164 项、Gallery 30 项、TreeView／Calendar 控件 275 项、TreeView／Calendar／Descriptions 渲染 100 项通过。CheckBox／RadioButton 定向控件 21 项、渲染 45 项通过。Spinner 的独立定向验证为控件 70 项、渲染 119 项、Gallery 2 项。前述取证涉及的驱动、媒体和临时测试已清理，Git 工作区取证检查无新增二进制，`git diff --check` 通过。
