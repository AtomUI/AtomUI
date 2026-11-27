# Int 设计语言视觉验收标准

## 基准与适用范围

本次对标用户指定的 [Int UI Kit (Community) 总览](https://www.figma.com/design/YoomwBGFM2Nxzl4OPXhwcb/Int-UI-Kit--Community-?node-id=6222-73687)，于 2026-11-25 核对。总览的 Design basics、Components 与各组件页的 States、Content、Themes 是取样入口。组件示例使用 AtomUI 自有文案和数据，验收对照组件本身的视觉角色、尺寸、状态和交互。

Int 的取值覆盖 `AtomUI.Controls`、`AtomUI.Desktop.Controls`、`AtomUI.Desktop.Controls.ColorPicker` 和 `AtomUI.Desktop.Controls.DataGrid`。主窗口和工具窗口参考用于 AtomUI 窗口、标题栏和分隔控件。IDE 工作台、停靠管理、原生编辑器和 JetBrains 产品标识另有产品边界。

- **Figma 基准**采用 [颜色](https://www.figma.com/design/YoomwBGFM2Nxzl4OPXhwcb/Int-UI-Kit--Community-?node-id=305-26005)、[排版](https://www.figma.com/design/YoomwBGFM2Nxzl4OPXhwcb/Int-UI-Kit--Community-?node-id=733-46971) 和 [间距](https://www.figma.com/design/YoomwBGFM2Nxzl4OPXhwcb/Int-UI-Kit--Community-?node-id=7990-79465) 页的角色、组件结构和状态。数值取自图层属性或标注，画布缩放后的像素只用于定位。
- **平台运行时基准**固定 IntelliJ Platform [`idea/263.7307.200`，`c91871ae`](https://github.com/JetBrains/intellij-community/tree/c91871ae2864649d952825cff72569e3db0e7ac6/platform/platform-resources/src/themes)。Figma 未定义的运行时状态和紧凑尺寸，以该提交的主题资源和 [IntelliJ UI 指南](https://plugins.jetbrains.com/docs/intellij/ui-guidelines-welcome.html) 补足。Islands 专属颜色、标签背景和面板尺寸以 [Islands 指南](https://plugins.jetbrains.com/docs/intellij/supporting-islands-theme.html) 与该提交的主题资源为准。每项同名指标如果数值不同，在对应场景记录注明引用值及适用变体。
- **授权**。JetBrains 发布的 [Int UI Kit](https://www.figma.com/community/file/1227732692272811382/int-ui-kit) 和 [Int UI Icons](https://www.figma.com/community/file/1227729570033544559/int-ui-icons) 标注 [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/)。分发改编素材时注明作者、来源、许可及改动。引用 [Jewel](https://github.com/JetBrains/jewel#license) 或平台源码时，逐文件确认 Apache 2.0 及第三方声明。产品商标按 [JetBrains 品牌规范](https://www.jetbrains.com/company/brand/) 处理。

## 验收等级

- **A 对标**。官方有相同的控件与用途。按具体变体和状态测量几何、前景与背景、边界、字重和字阶、焦点与禁用。每个结论对应一个 Figma 节点或官方运行时键。
- **B 协调**。官方有可借用的角色或基础组件。独有部件使用 Int 的色阶、间距、字阶和状态语义。按默认、主要交互和禁用实拍审阅，说明借用的角色和 AtomUI 扩展。ColorPicker 归此等级，DataGrid 的表头、单元格和选行场景对标 Table。

取值包的 README 为每个控件 Token 契约记录等级和来源。A 的等级按场景判定，例如 DataGrid 的表头和选行可对标，分页扩展可按 B 验收。

## 可测量的视觉条件

| ID | 观测项 | 通过条件 |
| --- | --- | --- |
| INT-V01 | 文字与输入 | 界面字体 Inter，编辑器样例 JetBrains Mono。Figma 默认 UI 13/16、正文段落 13/18、小字 12/16、H1 20/24、H2 16/20，默认代码 13/22、小号代码 12/22，单位为字号/行高。Figma 中标为 Medium 的常规字重在运行实现中按该页标注使用 Regular，Semibold 和 Bold 保持对应层级。运行时指南的 H1 为正文 +5，只有采用该运行参考的场景使用相应字号。 |
| INT-V02 | 颜色与层级 | 每个状态核对背景、文字、图标、边框及焦点角色。Token 的不透明值与选定参考一致。真窗平坦区域对期望合成色的通道容差 ≤ 8，取样避开字缘、边缘抗锯齿和阴影。编辑器色板与 UI 色板单独取值。 |
| INT-V03 | 几何与布局 | 以 Figma 图层和标注记录控件高度、内边距、圆角、图标槽和相邻间距。逻辑尺寸误差 ≤ 1 DIP，关键对齐边在换算和布局舍入后相差 ≤ 1 个物理像素。保留截图中标注的控件内、表单内和外部三类间距。 |
| INT-V04 | 状态与可用性 | 切换前后界面颜色与几何均符合相同状态。焦点始终可辨，错误聚焦、失焦选中、禁用各保留对应状态。鼠标、键盘、弹层开合和滚动断言以控件公开状态为证。 |
| INT-V05 | 密度与变体 | 桌面默认和紧凑密度各验 Light 与 Dark。若声明独立 Islands 风格，对其运行时独有的选中标签、区域边界和背景另验。紧凑变化以实际图层值或运行时键为依据，文字和图标应完整可读。 |

## 组件场景

| 场景 | 官方参照 | 可观察验收项 |
| --- | --- | --- |
| 按钮及组合操作 | [Button](https://www.figma.com/design/YoomwBGFM2Nxzl4OPXhwcb/Int-UI-Kit--Community-?node-id=733-46953) | Default Button 的 Primary 与 Secondary，Split Button 的两个动作区域，Slim Button 的紧凑边界。各自核对 Active、Hovered、Pressed、Focused 和 Disabled。 |
| 文本、下拉和搜索 | [Input field](https://www.figma.com/design/YoomwBGFM2Nxzl4OPXhwcb/Int-UI-Kit--Community-?node-id=3622-84458) · [Combo box](https://www.figma.com/design/YoomwBGFM2Nxzl4OPXhwcb/Int-UI-Kit--Community-?node-id=3595-81748) · Kit 的 Dropdown、Search | 对照 Default、Focused、Validated、Validated focused、Disabled，复核提示、占位符、内联图标、下拉箭头与弹层选行。用真实键盘输入确认错误聚焦保留错误语义和焦点。 |
| 二值与多值选择 | [Checkbox](https://www.figma.com/design/YoomwBGFM2Nxzl4OPXhwcb/Int-UI-Kit--Community-?node-id=3590-81582) · Kit 的 Radio button、Toggle、Segmented control | 复选框覆盖未选、已选、混合 × Active、Hovered、Focused、Invalid、Disabled。Radio 和 Toggle 对照各自状态板，含选中后焦点、键盘操作和二态切换。 |
| 标签及工具栏 | [Tab](https://www.figma.com/design/YoomwBGFM2Nxzl4OPXhwcb/Int-UI-Kit--Community-?node-id=1114-53696) · Kit 的 Toolbar、Group header、Link | 普通标签和编辑器标签各核对 Default、Hovered、Selected、Inactive、关闭图标和文本。Islands 同时核对选中标签的背景与边界。工具栏核对图标、分组、禁用和切换项。 |
| 树和表格 | [Tree](https://www.figma.com/design/YoomwBGFM2Nxzl4OPXhwcb/Int-UI-Kit--Community-?node-id=1113-52835) · [Table](https://www.figma.com/design/YoomwBGFM2Nxzl4OPXhwcb/Int-UI-Kit--Community-?node-id=8022-79218) | 树核对默认、活跃和非活跃选中、层级缩进及展开箭头，含无图标、带图标、提示和复选框的行。表格核对行高、表头、细线、选中行及带下拉的单元格。固定视口验证滚动条和整行命中。 |
| 弹层及反馈 | [Popup](https://www.figma.com/design/YoomwBGFM2Nxzl4OPXhwcb/Int-UI-Kit--Community-?node-id=58-8661) · Kit 的 Context menu、Dialog、Tooltip、Alert、Banner、Notification | 测量面板前景、阴影、边框、内边距、行距及悬停选中。覆盖菜单快捷键列、子菜单、键盘焦点与 Esc 关闭，确认关闭后的焦点恢复。通知按状态对照图标、色彩、正文和操作区。 |
| 进度、滚动与窗口 | Kit 的 Progress bar、Loader、Scrollbar、[Main window](https://www.figma.com/design/YoomwBGFM2Nxzl4OPXhwcb/Int-UI-Kit--Community-?node-id=217-51116)、[Tool window](https://www.figma.com/design/YoomwBGFM2Nxzl4OPXhwcb/Int-UI-Kit--Community-?node-id=1145-53999) | 验证进度可见性、滚动条轨道和滑块、普通及失焦状态的标题栏。工作区域按采用的风格核对边界与圆角。可见分隔线和有效拖拽区分别测量。 |

## 执行与证据

1. 固定 macOS 桌面宿主、实际字体、客户区 DIP、渲染比例和取样区域。用 `AtomUI.Desktop.Controls.Window` 承载实际模板与弹层，用公开属性、命令和输入路由触发每个状态。
2. 采集现有语言的实现前基线及 Int 实现后的场景。用同状态 A/A 零差异、1 DIP 扰动可见、恢复后零差异验证采样链。首选真实窗口帧，记录窗口身份、二进制、平台、`ClientSize` 和 `RenderScaling`。
3. 在全部 Gallery 展示页跑浅色、深色及声明密度下的脚本遍历。使用上表定位每个 A 对标场景并核对真窗帧，对 B 协调项审阅实际模板和主要交互。只有发生模板或 Token 消费路径变化时，对受影响的 Token 补充对应状态的单变量扰动。参见 [主题生效测试](../../engineering/development/theme-effect-tests.md)。
4. 每个场景记录验收 ID、Figma 节点或运行时键、环境与变体、操作、预期、实测、容差和状态。色值、几何、状态等必要断言全部通过后才标记通过。未测平台、字体回退和渲染差异写在该场景证据中。
5. 临时驱动、截图和日志存临时目录，验证和复核后清理。仓库中保留证据结论和必要数值。清理后再按 [受影响范围验证](../../engineering/workflows/affected-verification.md) 运行模块检查。

本文件记录基准和通过条件。Int 原型的真窗脚本、对标结果和产物验证待执行。
