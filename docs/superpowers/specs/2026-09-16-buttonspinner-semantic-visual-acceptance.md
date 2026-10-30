# ButtonSpinner Semantic Part 改造 · 真机视觉验收步骤

> 状态：**待视觉验收**（尚未收到用户回传截图或录屏；按仓库全局强约束，未回传前不得宣称视觉验收通过）。
>
> ## 验收结论记录（文字证据）
>
> | 日期 | 证据 | 结论 | 修复记录 |
> | --- | --- | --- | --- |
> | — | 尚未收到用户回传 | 待验收 | — |

## 背景

本次改造为 `ButtonSpinner` 新增 Semantic Part 契约，涉及三处可能影响渲染的模板/行为变更，需要重点确认：

1. 帧主题（`ButtonSpinnerDecoratedBoxTheme.axaml`）在内容主 presenter 与内容左/右槽 presenter 上新增
   `Classes.semantic-content` / `Classes.semantic-inner-left-content` / `Classes.semantic-inner-right-content`
   静态标记。**预期不改变任何默认视觉**：AtomUI 内置主题不消费 `.semantic-*` selector，标记只在模板初始化时
   执行一次 `Classes.Set`，不创建 Binding、selector activator 或状态订阅。
2. 手柄主题（`ButtonSpinnerHandleTheme.axaml`）为 `PART_IncreaseButton` / `PART_DecreaseButton` 新增
   `Classes.semantic-increase-button` / `Classes.semantic-decrease-button` 标记，owner 模板为步进手柄新增
   `Classes.semantic-actions`。同样**预期不改变默认视觉**。
3. **根边框中继 + 帧动效接线（新增行为）**：owner 的 `BorderBrush` 现在会中继到共享输入帧。默认主题不给
   `ButtonSpinner` 设置 `BorderBrush`，因此**未设置该属性时行为与改造前完全一致**；只有应用显式设置时才冻结帧
   状态机的 hover / focus 边框变色（focus 的 `BoxShadow` 光晕不受影响）。
4. **帧 `IsMotionEnabled` 接线**：owner 的 `IsMotionEnabled` 现在会传到 `ButtonSpinnerDecoratedBox` 帧（此前只传到
   内部手柄）。这不只是动效开关问题——帧上的 `BorderBrush` 过渡若在运行中，其值优先级高于中继写入的 LocalValue，
   会让根边框定制**看似无效**。同一问题也存在于 `NumericUpDown` 两个模板，已一并修复。

新增能力为 Gallery 的 Semantic Parts 页签与一个使用生成 `<atom:ButtonSpinner*Style>` 类型的样式示例，
示例同时演示根边框颜色定制。本次验收范围仅限 Semantic Part 新增视觉与本轮变更点；不做与 6.1.7 基线的
全量回归走查（仅在怀疑 bug 时执行）。

## 操作路径与判定标准

准备：运行 AtomUIGallery（`controlgallery/AtomUIGallery.Desktop`）。
入口路径：左侧导航 **Components（组件）→ Navigation（导航）→ ButtonSpinner**。
页面标题为 `ButtonSpinner`，Header 中 Category 显示 `Navigation`、Status 显示 `Stable`。

### 步骤 1：Examples 面板回归（确认默认视觉未被改变）

| # | 操作 | 预期现象 | 截图范围 |
| --- | --- | --- | --- |
| 1.1 | 进入页面默认的 **Examples** 页签，向下滚动查看全部 8 个条目 | 全部条目正常渲染，无布局错位、无多余间距、无缺失内容 | 全景 1 张（可分段 2–3 张） |
| 1.2 | 查看 **ButtonSpinner sizes** 示例（Large / Middle / Small / Custom 四档） | 四档高度与圆角正常；`Custom` 档按 `Height="36"`、`SpinnerHandleWidth="28"` 呈现，与改造前一致 | 1 张 |
| 1.3 | 查看 **Variants** 与 **Disabled** 示例 | Outlined / Filled / Borderless 三种变体的边框与背景正常；禁用态视觉正常 | 2 张 |
| 1.4 | 查看 **Pre / Post tab** 与 **prefix and suffix** 示例 | 外部 `LeftAddOn="http://"` / `RightAddOn=".com"` 正常；内部 `InnerLeftContent` / `InnerRightContent`（`￥` / `RMB`、图标）位置正确 | 2 张 |
| 1.5 | 查看 **Status** 示例 | Error / Warning 状态的边框与图标着色正常 | 1 张 |

**重点判定：** 步骤 1.4 的 `InnerLeftContent` / `InnerRightContent` 是本次新增 marker 所在节点，
其位置与间距必须与改造前**完全一致**。若出现偏移，即为标记引入的回归缺陷。

### 步骤 2：Semantic Parts 页签（新功能）

| # | 操作 | 预期现象 | 截图范围 |
| --- | --- | --- | --- |
| 2.1 | 点击页面顶部 **Semantic Parts** 页签（首次进入时才创建内容） | 出现**一个** Preview（`ButtonSpinnerSemanticPreview`），列出 7 个 Part 条目 | 1 张 |
| 2.2 | 查看该 Preview 的 Part 列表 | 列出 7 项：`root`、`content`、`innerLeftContent`、`innerRightContent`、`actions`、`increaseButton`、`decreaseButton`，每项有说明文字 | 1 张 |
| 2.4 | 逐个 Hover 或 Pin `root` / `content` / `innerLeftContent` / `innerRightContent` 条目 | 对应区域高亮描边**四边完整可见**，无裁剪缺失；`innerLeftContent` 高亮应紧贴 `￥` 实际区域而非整段 | 4 张（每项 1 张） |
| 2.5 | 逐个 Hover 或 Pin `actions` / `increaseButton` / `decreaseButton` 条目 | `actions` 高亮覆盖整个步进手柄区域（含背景、分隔线、圆角）；`increaseButton` 只高亮上方 `+` 按钮，`decreaseButton` 只高亮下方 `−` 按钮，二者**不互相包含** | 3 张（每项 1 张） |
| 2.6 | 观察 Preview 中手柄的呈现 | 手柄**常驻可见并占位**（`IsButtonSpinnerFloatable` 默认 `False`，即控件默认外观）。本预览只演示默认的常驻手柄模式，不演示浮动模式 | 1 张 |
| 2.7 | 查看面板底部 **Custom Semantic Part styling** 示例 | 上组（Object）：**外框边框为蓝 `#1677FF`**、步进手柄背景为浅蓝 `#F0F5FF`、分隔线为蓝、`+`/`−` 图标为蓝、左侧 `Object` 文字为蓝；下组（Function）：同结构但用紫 `#722ED1` / 浅紫 `#F9F0FF`。两组均通过生成的 `ButtonSpinner*Style` 类声明式命中，外框颜色通过根 `BorderBrush` Setter 生效 | 1 张 |
| 2.8 | 在样式示例的两组控件上分别 hover、再点击使输入框获得焦点 | 外框颜色**保持**定制的蓝 / 紫（不被 hover / focus 状态机改回默认灰色）；焦点光晕（`BoxShadow`）正常出现。这是根边框中继的预期语义 | 2 张（hover 1 张、focus 1 张） |
| 2.9 | **动效接线验证（本次最关键）**：在样式示例上把指针移入 / 移出，观察外框与手柄的背景过渡 | 过渡动效仍正常播放、无跳变；如果这里出现「边框颜色不跟手 / 停在中途色」即为帧动效接线回归。本次把 `IsMotionEnabled` 补绑到帧上，是唯一可能影响默认动效的地方 | 录屏 1 段（优于截图） |

**重点判定：** 步骤 2.5 是本设计最需要人工确认的一项——`increaseButton` / `decreaseButton` 的 route 需要
跨越 `.semantic-actions` 锚点进入手柄自己的模板。若两个按钮的高亮同时亮起或高亮框覆盖整个手柄，
说明 route 未正确区分两个按钮。

> **浮动模式（`IsButtonSpinnerFloatable=True`）不在本页签演示范围内。** 该模式在静止态手柄是 `Opacity=0`
> 且外移，语义高亮需要额外的「静止态透明部件显现」机制（先例是 ImagePreviewer 的 `cover`，走
> `RestHidden` + `SemanticPartPreviewState`）；ButtonSpinner 的透明度位于手柄 presenter（目标的祖先）而非
> 目标自身，现有机制无法直接套用，故本次不演示。§2 记录的「三种呈现提供相同 Part 集合」由自动化测试
> （`ButtonSpinnerSemanticPartTests.Markers_Remain_Stable_When_Handle_State_Changes` 覆盖浮动/隐藏/停靠切换）
> 锁定，不以本页签为准。

### 步骤 3：交互与主题回归（可录屏替代逐项截图）

| # | 操作 | 预期现象 |
| --- | --- | --- |
| 3.1 | 在 Examples 页签的任一 ButtonSpinner 示例上：鼠标移入 / 移出 | 帧边框与背景按原动效过渡，无跳变、无残留 |
| 3.2 | 点击 `+` / `−` 各 2 次 | 手柄按原行为响应（默认常驻模式） |
| 3.3 | 切换窗口深浅色主题，重复步骤 2.2–2.7 | 两种主题下 Semantic Parts 页签与样式示例均无异常（背景、文字、高亮对比度正常） |
| 3.4 | 切换语言为 English / 繁體中文 / Português，重复步骤 2.1–2.2 | Preview 的各 Part 说明文字正确切换，无缺失或占位符 |

## 判定标准

- **步骤 1** 的渲染必须与改造前（`release/6.0` 基线）一致——本次改造对默认视觉应为零影响。
- **步骤 2** 为新增能力，判定以「高亮框准确框住语义区域、四边完整、互不串扰」为准。
- **步骤 3** 判定为交互与主题无回归。
- 未回传截图或录屏前，本改造保持标注「待视觉验收」，不宣称通过。

## 附：同批修复的输入族根边框定制（Select / TreeSelect / Cascader / OtpLineEdit）

本次一并修复了四个同类控件「owner 根 `BorderBrush` 不生效」的问题（原因与 ButtonSpinner 相同：可见外框由共享帧
绘制，状态机拥有边框属性）。这些是**修复**而非新增视觉，默认外观不变，验收重点是「定制生效 + 默认无回归」。

入口路径（均在左侧导航 **Components（组件）→ Data Entry（数据录入）**）：

| 控件 | Gallery 页面 | 观察项 |
| --- | --- | --- |
| Select | Data Entry → Select | 在 Examples 任一示例上应用根 `BorderBrush`（见下）后外框变色；未定制时与改造前一致 |
| TreeSelect | Data Entry → TreeSelect | 同上 |
| Cascader | Data Entry → Cascader | 同上 |
| OtpLineEdit | Data Entry → OtpLineEdit | 根 `BorderBrush` 令所有 cell 边框变色；`CellBorderBrush` 优先于根 `BorderBrush` |

判定要点：

1. **默认无回归（最重要）**：四个控件在未设置根 `BorderBrush` / `Background` 时，外框必须与改造前完全一致
   （rest / hover / focus / error / warning / disabled 各状态都不变）。本次把 `IsMotionEnabled` 补绑到
   Select 族的帧上，需确认 hover / focus 边框与背景的过渡动效仍正常、无跳变。
2. **定制生效**：设置根 `BorderBrush` 后外框变色，且 hover / focus 不再把颜色改回主题值；focus 光晕
   （`BoxShadow`）仍正常出现。
3. **OtpLineEdit 优先级**：`CellBorderBrush` 设置时压过根 `BorderBrush`；清空 `CellBorderBrush` 后回落到根
   `BorderBrush`，而不是回到主题 rest 值。
4. **Select 族 `Background`**：设置根 `Background` 后帧填充色变化；置空后恢复主题。
5. **OtpLineEdit 的 `Background` 不在本契约内**（故意的）：设置控件的 `Background` 不改变 cell 填充色。

> 完整证据与逐条断言见自动化测试 `SelectRootSurfaceRelayTests`（15 项）与
> `OtpLineEditRootSurfaceRelayTests`（4 项）；真机截图仍按上文流程由用户回传。

## 已完成的自动化证据（不等于视觉验收）

以下为文字证据，用于说明交付前已通过的自动化门禁；按全局强约束，它们**不能替代**上面的真机视觉验收：

| 验证项 | 结果 |
| --- | --- |
| `AtomUI.Desktop.Controls.Tests`（全量） | 4068/4068 通过 |
| `ButtonSpinnerSemanticPartTests`（新增 9 项） | 9/9 通过（含 descriptor、六类 marker、生成 Style 命中、状态稳定性、默认主题不消费 `.semantic-*`） |
| `NumericUpDown` 语义部件回归（共享帧护栏） | 44/44 通过 |
| `NumericUpDownRootSurfaceTests`（含两模式动效接线） | 7/7 通过 |
| `ButtonSpinnerRootSurfaceTests`（含动效接线与实时改边框） | 5/5 通过 |
| `AtomUIGallery.Tests`（全量） | 645/645 通过 |
| `AtomUI.Toolkits.GalleryBase.Tests` | 185/185 通过 |
| `AtomUI.Generator.Tests`（SemanticPart 过滤） | 54/54 通过 |
| `AtomUI.slnx` 全量构建 | 0 warning / 0 error |
| LLMS verify | 通过（79 控件 / 161 文件） |
| `git diff --check` | 干净 |
