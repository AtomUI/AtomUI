# 全局焦点效果与键盘导航排查（2026-11-11）

本文档是排查记录，不包含任何代码修改。对照基准是 Ant Design 6.7.0：`ReferenceProjects/ant-design`，以及其中 `components/style/index.tsx` 的 `genFocusOutline`，规格为 3px `colorPrimaryBorder` 描边、向外偏移 1px，并跟随圆角。

## 证据来源

- **静态排查**：逐个读取 `src/AtomUI.Desktop.Controls`（含 DataGrid、ColorPicker、Extras）的主题和 C# 源码，并与 antd 组件样式逐项对照。rc-* 包不在本地，相关键盘行为依据 rc 的已知行为或 WAI-ARIA APG 推断，表格中标为“推断”。
- **运行时探测**：
  - 宿主是 atom:Window，截图方式为原生 `screencapture -l`，运行时 DLL 的 SHA256 与仓库产物一致。
  - 控件全部取自 Gallery 各 ShowCase 的标准预览 AXAML，编译后放入页面，与真实用法一致。
  - 逐站真实按 Tab，记录焦点落点、全局默认方框是否挂上，并截取焦点前后对比图。
  - 运行期间窗口失去过 2 次激活（用户截图时）。焦点落点和方框挂载状态不受影响，截图仅作参考。
  - 证据目录：会话 scratchpad 下的 `gallery-focus-evidence`。
- **反射探测作废**：最初用反射批量创建控件的那一轮，控件没有正常的数据和尺寸，结论不可信，已全部作废。

## 一、焦点效果

### 1. 共享根因

修好这几处，能覆盖下面大部分条目。

| 根因 | 现状 | Ant Design | 证据 |
| --- | --- | --- | --- |
| 全局 `DefaultFocusAdorner` | `AdornerLayer/Themes/AdornerLayerTheme.axaml`：方角 `Border`，没有向外偏移，不跟随圆角；`AdornerLayer` 默认裁剪 | 圆角描边，向外偏移 1px，3px 宽 | 运行时：CheckBox、Radio、Switch、Rate、FloatButton、Segmented、Pagination、Breadcrumb、Collapse 等都画出方框 |
| `LineWidthFocus` | `AtomUI.Core/.../DesignToken.cs:120` 为 `LineWidth * 2` = 2px | `lineWidth * 3` = 3px（`theme/util/alias.ts:91`） | 静态 |
| 焦点目标范围 | 没有重写 `GetTemplateFocusTarget()`，方框框住整个控件 | CheckBox、Radio 只描边勾选框或圆点；Collapse 只描边标题；Tab 只描边标签 | 运行时：CheckBox、Radio 的方框连同文字一起框住 |
| 内部部件成为 Tab 停靠点 | 箭头按钮、展开按钮、关闭按钮、DecoratedBox、日期格子都能拿到焦点，并且各自画方框 | 这些部件不单独成为停靠点 | 运行时：见第 3 节 |

### 2. 控件清单

“运行时”表示已用 Gallery 标准示例实测确认，“静态”表示只从代码判断。

| 控件 | 现状 | Ant Design 期望 | 证据 |
| --- | --- | --- | --- |
| CheckBox（含 DataGrid 选择列） | 方角框框住勾选框和文字 | 只在勾选框外围画圆角描边 | 运行时 |
| RadioButton | 方角框框住整个控件 | 圆点外围画圆形描边 | 运行时 |
| OptionButton | 方角框 | 圆角描边 | 静态 |
| ToggleSwitch | 方角框 | 胶囊形描边 | 运行时 |
| Rate | 方角框框住整排星星 | 星星上 1px 虚线描边并放大 | 运行时 |
| FloatButton、BackTop | 圆形按钮外画方角框 | 按钮描边 | 运行时 |
| Segmented | 每个选项各一个方角框，父级还会裁剪内容 | 选项描边 | 运行时 |
| Pagination | 每个页码各一个方角框，导航面板会裁剪内容 | 页码圆角描边 | 运行时 |
| Breadcrumb | 方角框 | 链接描边 | 运行时 |
| Collapse | 方框框住标题和展开后的正文；展开按钮又是一个停靠点 | 只描边标题 | 运行时 |
| Expander | 只有展开按钮能拿到焦点，并画方框 | 标题描边 | 运行时 |
| Tag（可关闭） | 关闭按钮画方框 | 关闭按钮描边 | 运行时 |
| TabControl、TabStrip | 方角框；关闭按钮和滚动菜单按钮另成停靠点 | 标签描边；卡片式 offset 为 -3 | 运行时、静态 |
| Steps | 方角框，并且聚焦时出现悬停底色 | 描边 | 静态（运行时没有 Tab 停靠点，见第 3 节） |
| HyperLinkButton | 方角框 | 链接描边 | 静态 |
| Button、DropdownButton | 没有方框，但模板内的 FocusVisual 画在边框内侧，2px，没有间距 | 3px，向外偏移 1px，跟随圆角 | 静态 |
| Alert、Drawer、Notification、ImagePreviewer、Splitter、Transfer 中的 IconButton | 方角框 | 描边 | 运行时（Alert）、静态 |
| Dialog、Tour 的关闭按钮 | 方角框 | 描边 | 静态 |
| NavMenu、ContextMenu 菜单项 | 根节点画方角框；键盘活动项只有底色，没有描边 | 菜单项描边 | 运行时、静态 |
| TreeView | 键盘活动项只有底色 | 列表有键盘焦点时，活动节点外加描边 | 静态（已知差距） |
| Carousel 指示点 | 方角框 | 没有描边 | 运行时 |
| SliderThumb | 已经有 antd 风格的手柄放大和阴影，上面又叠了一个方框 | 只有手柄视觉 | 运行时 |
| DateViewerCell（Calendar、各类选择器的日期面板） | 自带 2px 边框，又叠加方框 | 日期格没有焦点描边 | 运行时 |
| ColorPicker 触发器 | 方角框；弹层打开时没有 controlOutline 外发光 | 弹层打开时显示外发光 | 静态（运行时截图位置偏移，未能确认） |
| ColorSlider | 两个停靠点：整条滑轨一个方框，手柄又一个方框 | 手柄 1px 主色描边 | 静态 |
| OtpLineEdit | 当前格的外发光正确，但整组外面又有一个方框 | 只有当前格外发光 | 静态 |
| DataGridRowExpander | 边框变色正确，又叠加方框 | 只有边框变色 | 静态 |
| ColorSpectrum | 样式引用了模板里不存在的 `Ellipse#FocusEllipse`，是一条无效样式 | — | 静态 |
| DataGrid 列头 | `PART_FocusVisual` 没有任何样式会让它显示 | 排序文字变为主色 | 静态 |

### 3. 上次提交（7e40bc14a）的遗漏

这几项在运行时确认，是上次输入家族焦点修复没有覆盖到的：

- **范围日期选择器、范围时间选择器**：Tab 焦点落在内部的 `AddOnDecoratedBox#PART_AddOnDecoratedBox` 上，在圆角边框上叠画方框。上次的聚焦矩阵只测了单日期选择器的输入框。
- **ComboBox（可编辑）**：焦点落在 `ComboBoxTextBox#PART_EditableTextBox` 上，Select 内部出现方框，和之前 TreeSelect 搜索框是同一类问题。
- **Mentions**：一次 Tab 要停 3 站（根控件、`MentionTextArea`、`TextAreaDecoratedBox`），其中 2 站画方框。AutoComplete 设置了 `IsTabStop=False`，没有这个问题。

### 4. Tab 停靠点过多（运行时）

| 控件 | 一次 Tab 循环的停靠数 | 说明 |
| --- | --- | --- |
| Calendar | 40（37 次画方框） | 年份和月份选择各自包含 DecoratedBox 与箭头按钮；每个日期格都是一个停靠点 |
| DateViewer | 38（36 次画方框） | 32 个日期格，加上翻页按钮 |
| Pagination | 6 | 每个页码都是停靠点 |
| Segmented | 6 | 每个选项都是停靠点；APG 规定组内只保留一个 |
| SplitButton | 2 | 符合预期 |

以下控件在探测中没有出现停靠点，需要单独确认：
- Steps、TabStrip、ListView（示例没有数据）、Splitter、ButtonSpinner、NumericUpDown。
- 其中 NumericUpDown 与 Mentions 在同一页。Mentions 之后 Tab 循环提前结束，有可能是焦点被困在 Mentions 内部，尚未确认。

### 5. 运行时发现的布局缺陷：Calendar

- **现象**：用标准写法 `<atom:Calendar />`，只要给它的高度小于它需要的高度（默认全屏模式约 790 DIP；实测给 420 DIP 即可复现），就会出现两个问题：
  - 第一周日期压在星期表头上；
  - 选中格的底色向下覆盖两行，看上去像整列被高亮。
- **根因**：`DateViewer/Internal/DatePanel.cs` 的日期格容器使用 `GridLength.Star` 行，高度不足时行高被压到约 54 DIP。而全屏模式下 `PART_CellInner` 的 `MinHeight` 是 `EffectiveFullCellMinHeight`，约 116 DIP，格子内容在行里垂直居中、上下溢出。
- **期望**：Ant Design 的日期行不会小于格子的最小高度；日历保持自身高度，由外层滚动。

## 二、键盘导航

### P1：核心操作无法用键盘完成

| 控件 | 现状 | 期望 | 建议 |
| --- | --- | --- | --- |
| DatePicker、RangeDatePicker | 只在指针抬起时打开（`InfoPickerInput.cs:604-625`）；焦点留在输入框，`DatePanel` 的方向键和 `DateViewerCell` 的 Enter/Space 处理永远收不到按键 | ↓/Enter 打开下拉，方向键选日期，PgUp/PgDn 切换月份，Enter 提交，Esc 关闭；输入文本可解析（推断） | 参照 TreeSelect：焦点留在输入框，按键转发给 `DatePanel` |
| ColorPicker | 触发器只响应 `PointerPressed`（`AbstractColorPicker.cs:485-488`）；`ColorSlider` 可以聚焦，但没有任何按键处理 | Enter/Space 打开；色相和透明度滑块响应方向键 | 处理 Click 或 Enter/Space；把 `Slider.cs:249-283` 的按键逻辑移植过来 |
| Upload | `UploadTrigger` 和 `UploadDropZone` 只响应指针；内部 Button 的 Click 没有人处理 | Enter/Space 打开文件对话框（推断） | 处理内部 Button 冒泡上来的 Click 事件 |

### P2：明显缺少按键

| 控件 | 缺口 |
| --- | --- |
| Drawer | 没有 Esc，也没有焦点进入、困住和恢复（antd 的 `keyboard` 默认为 true） |
| Dialog、Modal | Esc 只会触发“取消”按钮；没有取消按钮或使用自定义页脚时 Esc 无效；Tab 是否在弹窗内循环未确认 |
| Tour | 没有 Esc 和 ←/→ |
| ImagePreviewer（窗口模式） | 缺少 Esc（浮层模式有） |
| ListBox、ListView | 列表项都设了 `Focusable=False`，列表本身也不能聚焦，导致 `OnKeyDown` 和输入即跳转（typeahead）的代码永远执行不到 |
| Transfer | 继承了 ListView 的问题；勾选框是单向绑定，可能和选中状态不同步（未确认） |
| RadioButtonGroup、OptionButtonGroup、Segmented | 每一项都是停靠点；方向键只移动焦点，不改变选中值（APG 规定组内只保留一个停靠点，方向键同时移动并选中） |
| Carousel | ←/→ 不能切换；指示点可以聚焦，但无法激活 |
| DataGrid | 单元格导航完整，但列头不可达，无法用键盘排序或筛选 |
| TimePicker | 方向键无法在时、分、秒列之间移动；↓ 不能打开下拉 |

### P3：细节补全

- **Select、AutoComplete、Mentions 的候选列表**：缺少 Home/End、PgUp/PgDn，以及按 Tab 提交。
- **NavMenu**：缺少 Home/End。
- **Tabs**：可关闭标签不支持 Delete 关闭。
- **Slider（范围模式）**：Home/End 以 `Value` 而不是当前聚焦的滑块计算，疑似缺陷，未确认。
- **Rate**：没有处理 RTL 翻转，也没有设置 `Handled`。
- **ToggleSwitch**：缺少 ←/→。
- **Pagination**：缺少 Space。
- **Splitter**：无法用键盘调整大小。
- **悬停触发的弹层**：无法用键盘打开。

### 已经基本达标

Cascader、ComboBox、Menu/ContextMenu、Collapse、Steps、CheckableTag、NumericUpDown、Tabs 溢出菜单、TreeView、TreeSelect。

## 三、建议的修复顺序

1. **焦点描边基础设施**：
   - 把 `DefaultFocusAdorner` 改成符合 `genFocusOutline` 的圆角外描边：偏移 1px，`ColorPrimaryBorder`，宽度取 `LineWidthFocus`，不被裁剪；
   - 把 `LineWidthFocus` 改为 3px；
   - 为 CheckBox、Radio、Collapse、Tab 重写 `GetTemplateFocusTarget()`。

   这一步一次覆盖第一部分的大部分条目。
2. **清理停靠点和重复视觉**：
   - 内部箭头、展开、关闭按钮设 `IsTabStop=False`；
   - 已经有自身焦点视觉的控件设空 `FocusAdorner`：SliderThumb、DateViewerCell、OtpLineEdit、DataGridRowExpander、Carousel 指示点；
   - 补上第一部分第 3 节的遗漏。
3. **修 Calendar 行高**：日期行不得小于格子的最小高度。
4. **键盘 P1**：DatePicker、ColorPicker、Upload。可以抽出共享的“弹层触发器键盘处理”和“焦点留在输入框、按键转发”机制。
5. **键盘 P2**：
   - 共享的弹窗键盘处理，覆盖 Dialog、Drawer、Tour、ImagePreviewer；
   - 组内单一停靠点（roving focus），覆盖 Radio、OptionButton、Segmented、Carousel 指示点；
   - 让 ListBox、ListView 本身成为唯一焦点目标。

## 修复记录：焦点描边基础（第 1 步）

对应第三部分第 1 条，同时补上第一部分第 3 节列出的遗漏。

### 改动

- **共享焦点描边**：新增 internal `FocusOutlineAdorner`（`AdornerLayer/FocusOutlineAdorner.cs`），`DefaultFocusAdorner` 改用它，规格如下：
  - 宽 `LineWidthFocus`、色 `ColorFocusBorder`，向外偏移 `AdornerLayerToken.FocusOutlineOffset`（`LineWidth`）；
  - 圆角等于目标圆角加外扩量，并夹到外扩后矩形短边的一半；
  - 关闭 `AdornerLayer.IsClipEnabled`，因为 Avalonia 的装饰器裁剪从目标自身边界开始计算，`TemplatedControl` 默认裁剪会把描边整个切掉。
  - `AdornerLayerToken.FocusVisualMargin` 改为 `FocusOutlineOffset`。
- **形状接口**：在 `AtomUI.Controls.Primitives` 新增 internal 接口 `IFocusOutlineShapeProvider`，由以下控件实现：
  - `Button`：提供 `EffectiveCornerRadius`，覆盖圆形和胶囊形；
  - `RadioIndicator`：正圆；
  - `AbstractToggleSwitch`：胶囊形。
- **`LineWidthFocus`**：从 `LineWidth * 2` 改为 `LineWidth * 3`。
  - 原先借用它的范围选择器激活条（`TimePickerToken`、`InfoPickerInputToken`）和 TreeView 拖拽指示线（`TreeViewToken`）改为引用 `LineWidthBold`。Ant Design 中这两处本来就是 `lineWidthBold`（2px），视觉不变。
  - TreeView 这条只记录在本日志里，因为工作区里 `tree-view/changelog.md` 有用户尚未提交的改动。
- **描边范围**：用 `TemplatedControl.IsTemplateFocusTarget` 把描边限定在部件上，包括 CheckBox 和 Radio 的指示器、CollapseItem 的 `PART_HeaderDecorator`。
- **按钮家族**：移除模板内画在边框内侧的 `Border#FocusVisual`，以及空 `FocusAdorner` 设置，改用共享描边。
- **遗漏补齐**：
  - `InputControlFrameTheme` 统一设置空 `FocusAdorner`，覆盖所有可聚焦外框，包括单 / 范围日期选择器、单 / 范围时间选择器、LineEdit、TextArea、ButtonSpinner 等。
  - `ComboBoxTextBoxTheme` 设置空 `FocusAdorner`。工作区里 ComboBox 的文档有用户尚未提交的改动，所以这条不写进 ComboBox 的 changelog。
  - `TextAreaDecoratedBox` 设置 `IsTabStop=False`。
  - `Mentions` 宿主默认 `IsTabStop=False`。

### 验证

- **无头测试**：
  - `FocusOutlineAdornerTests` 共 15 项，覆盖：
    - 键盘聚焦挂上描边、指针聚焦不挂；
    - 描边不被裁剪，宽 3、偏移 1；
    - 圆形按钮、Radio、Switch 的描边形状；
    - CheckBox、Collapse 的描边范围；
    - 7 类输入家族宿主下，每个可到达的停靠点都不出现描边。
  - 更新了 `ButtonKeyboardNavigationTests` 和 `ButtonFocusVisualRenderingTests`。
- **测试项目**（均在最终改动前一版代码上跑）：Core 366、Controls.Shared 201、Controls 222、Rendering 77、Desktop.Controls 4218、DataGrid 324、Gallery 412，全部通过。最终加入 ToggleSwitch 形状后，又重跑了 Controls 222 和描边、开关相关的 28 项。
- **脚本视觉验收**：42/42 PASS。
  - 宿主为 atom:Window，原生截图，控件写法取自 Gallery，运行时 DLL 与仓库产物一致，窗口全程保持激活，门禁 A/A=0、扰动 2591、还原 0。
  - 亮暗两种主题下：
    - 普通、圆形、胶囊按钮、DropdownButton、CheckBox、Radio、Switch、FloatButton、CollapseItem、Segmented 选项、Breadcrumb：上沿外 1 DIP 间隙后为 3 DIP 的 `ColorPrimaryBorder`（亮 `#91CAFF`，暗 `#15325B`），再往外为背景；
    - LineEdit、RangeDatePicker、DatePicker、TimePicker、可编辑 ComboBox、Mentions：没有焦点描边。
  - 前后对比总览图已人工复核形状，证据在会话 scratchpad 的 `focus-outline-evidence` 中。

### 未覆盖（属于后续步骤）

- 第三部分第 2 条：清理内部按钮的停靠点（Collapse、Expander 和 Tag 的展开、关闭按钮）；SliderThumb、DateViewerCell 等已经有自身焦点视觉的控件，去掉叠加的描边。
- 卡片式标签需要 -3px 的内收描边；TreeView、NavMenu 的键盘活动项还没有描边。
- 第三部分第 3–5 条。

## 修复记录：停靠点、重复描边、卡片标签与活动项描边（第 2 步）

对应第三部分第 2 条，以及第 1 步“未覆盖”中的卡片标签和 TreeView / NavMenu 活动项。

### 改动

- **共享描边能力**：
  - `FocusOutlineAdorner.IsInset` 附加属性：描边画在目标内侧（偏移为负的描边宽度），对应 `genFocusOutline(token, -3)`；负外扩时圆角下限为 0，矩形为空时不绘制。
  - 圆角来源新增一级：焦点目标自身不提供形状时，向模板宿主的 `IFocusOutlineShapeProvider` 取形状。
  - 新增 internal `ManagedFocusOutline`：给不持有焦点的元素（宿主持有焦点时的活动项）显示同一个 `DefaultFocusAdorner`，从目标自己的装饰层构建，弹层内的项画在弹层的装饰层中。
- **停靠点**：
  - Collapse：按 `TriggerType` 二选一。Header 模式展开按钮 `IsTabStop=False`；Icon 模式 `CollapseItem` 不是停靠点，只保留展开按钮（此模式只有它能切换）。
  - Expander：Header 成为停靠点并成为焦点目标，Enter / Space 切换；Icon 模式只保留展开按钮。Expander 按展开方向与状态提供 Header 贴住的外框圆角，避免方角描边戳出圆角外框。
  - Tag：核对 Ant Design 后确认关闭图标本身就是 `role=button`、`tabIndex=0`，Tag 本体不可聚焦，AtomUI 现状已符合，只补测试，不改代码。审计表里“清理 Tag 关闭按钮停靠点”的说法不准确。
- **去掉重复描边**：
  - SliderThumb、OtpLineEdit、DataGridRowExpander、Carousel 指示点设置空 `FocusAdorner`；
  - Carousel 指示点 `:focus-visible` 复用悬停透明度，因为 Ant Design 指示点 `outline: none`，不加的话键盘焦点完全不可见。
  - 日期格分两种情况：
    - 默认 `DateViewerCellTheme`（DateViewer 与各日期选择器面板）没有自身焦点视觉，改为把描边收窄到 `PART_ValueFrame`；
    - Calendar 全屏格保留顶部主色线、不加描边；Calendar 卡片格原本的 2px 主色内边框在选中的主色块上看不出来，改用共享描边包围 `PART_CellInner`。
- **标签页**：线型标签描边按钮行（图标与文字），卡片标签在 `Frame` 上内收描边；TabControl 与 TabStrip 一致。
- **活动项描边**：
  - TreeView：自身持有焦点且活动项可见时描边 `PART_HeaderContentFrame`；TreeSelect 下拉（焦点在输入框）只显示背景。
  - NavMenu：交互处理器维护“键盘焦点可见”状态（导航键进入；指针按下、焦点离开菜单含弹层后退出）。没有活动项时描边菜单根，有活动项后描边移到该项 header；菜单根 `FocusAdorner` 置空。
- **验收中发现并修复的既有缺陷**：
  - Calendar 卡片格日期数字偏上：数字位于内部网格 `Auto` 行，墨迹上方约 3.5 DIP、下方约 10.5 DIP。没有自定义格子内容时改为跨两行居中，修复后上下各 7 DIP。
  - OtpLineEdit 当前格外发光被裁剪：`OtpLineEdit` 和 `ItemsControl#PART_CellsHost` 默认裁剪，上、下、左三侧外发光被切掉。两者都改为 `ClipToBounds=False`。
  - 补记第 1 步因工作区冲突没写的 changelog：TreeView `DragIndicatorLineWidth`、ComboBox 内部编辑框空 `FocusAdorner`。

### 验证

- **无头测试**：
  - 新增 `FocusOutlineTargetTests` 22 项、`DataGridRowExpanderFocusTests` 1 项、Calendar 卡片格居中 2 项、OTP 外发光不裁剪 1 项；TreeSelect 测试补充“下拉不描边”断言。
  - 停靠点用真实 Tab 键逐站统计。
  - 抽查了三处修复（Collapse 停靠点、滑块手柄、卡片内收）和日历居中：临时撤掉后对应测试失败，恢复后通过。
- **测试项目**：在最终代码上串行运行受影响的项目：Desktop.Controls 4243、DataGrid 325、Rendering 77、Gallery 412，全部通过。没有运行全量回归（仓库规定只在明确要求时运行）。
- **脚本视觉验收**：53/53 PASS。
  - 宿主为 atom:Window，原生截图，控件写法取自 Gallery，Tab / 方向键经框架输入路由驱动；每个主题重建页面，因为活动项按设计在失焦后保留。
  - 运行时 DLL 一致，窗口全程激活，门禁 A/A=0、扰动 7022、还原 0。
  - 亮暗两种主题覆盖：
    - Collapse 两种模式的停靠点与描边、Expander 描边与圆角及 Enter、Tag 关闭按钮；
    - 滑块与 OTP 无描边且自身视觉生效、OTP 外发光四边一致；
    - 线型与卡片标签、走马灯指示点；
    - TreeView 活动项随方向键移动且旧描边清除；
    - Calendar 卡片格描边与数字居中、DateViewer 格；
    - NavMenu 根到活动项的转移、弹层子项描边。
  - 中途作废过两轮：一轮窗口被失活，一轮运行时 OTP 被点击导致门禁失败；最终结果来自一次无干扰的完整运行。

### 未覆盖

- TabControl / TabStrip 的关闭按钮与滚动菜单按钮仍是独立停靠点（审计第一部分第 2 节），本步未处理。
- ColorSlider 两个停靠点、ColorSpectrum 无效样式、DataGrid 列头焦点视觉，属于审计中的其他条目。
- 第三部分第 3–5 条（Calendar 行高、键盘 P1 / P2）。
