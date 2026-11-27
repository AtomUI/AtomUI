# TreeView 键盘导航研发日志（2026-11-11）

状态：已实现（TreeView + TreeSelect），测试通过，文档已与实现同步；未提交。依赖版本：Avalonia 12.1.x。

来源：[AtomUI/AtomUI#535](https://github.com/AtomUI/AtomUI/issues/535)（feature request）——TreeView 希望支持键盘操作：上下键切换选中项，左右键展开 / 收起。

## 结论摘要

TreeView 当前完全不响应方向键。原因不是“少写了按键分支”，而是焦点链路在源头被切断：TreeView 和 TreeViewItem 都不可聚焦，按键事件到不了树；而 Avalonia 基类的键盘导航又依赖“把焦点移到节点”，和 AtomUI 的不可聚焦节点、`IsSelectable`、过滤、异步加载约束互相冲突。

采用的方案：TreeView 作为树内唯一焦点目标，内部维护“键盘活动项”（与 Ant Design rc-tree 的 `activeKey` 对齐，与仓库内 `CascaderView` 的 keyboard candidate 模式一致），重写按键、Tab 和 GotFocus 路径，不再使用 Avalonia 基于节点焦点的导航。最终设计见 [TreeView 键盘导航设计](../../controls/desktop/data-display/tree-view/keyboard-navigation-design.md)。

## 根因

### 1. 焦点链路断开

| 位置 | 现状 | 后果 |
| --- | --- | --- |
| `TreeViewItemTheme.axaml` 样式 | `TreeViewItem.Focusable=False` | 节点永远拿不到焦点。 |
| `TreeViewItemTheme.axaml` 模板 | `TreeViewItemHeader` 的 `Focusable="{TemplateBinding Focusable}"` | header 跟随为 False。 |
| `TreeView` | 未设置 `Focusable`，Avalonia `TreeView` 也未覆盖默认值（`InputElement` 默认 False） | 树本身也不可聚焦，KeyDown 不会路由到 TreeView。 |
| `TreeViewItemHeaderTheme.axaml` | `ToggleCheckbox` / `ToggleRadio` 未设置 Focusable，继承 `ToggleButton` 默认 True | 点击勾选框会把焦点抢到勾选框上。 |

### 2. Avalonia 基类导航不能直接复用

Avalonia `TreeView` / `TreeViewItem` 的键盘模型：

- `TreeView.OnKeyDown`：方向键时，如果有 `SelectedItem`，就算出下一个容器并调用 `next.Focus(NavigationMethod.Directional)`；`OnGotFocus` 收到 Directional 焦点后再更新选择。如果没有 `SelectedItem`，直接 `SelectedItem = ItemsView[0]`。
- `TreeViewItem.OnKeyDown`：节点自己持有焦点时，Left / Right / Enter / + / - / * / ÷ 负责展开收起。
- `ICustomKeyboardNavigation.GetNext`（显式接口实现）：Tab 进入时返回选中容器或第一个容器；当焦点元素就是 TreeView 自身时，由于 `IsVisualAncestorOf` 不包含自身，又会返回容器。

直接放开焦点会出现的问题：

1. 节点不可聚焦时 `Focus()` 失败：第一次按 ↓ 只会选中第一项，之后不再移动。
2. `IsSelectable=false` 时，`SelectedItem = ItemsView[0]` 或焦点驱动的选择会写入 `IsSelected`，触发 `TreeViewItem.OnPropertyChanged` 中的 `InvalidOperationException("The IsSelectable property value is False ...")`。
3. `GetContainerInDirection` 不跳过被过滤隐藏（`IsVisible=false`）和被禁用的节点。
4. 右键展开只设置 `IsExpanded`，不会触发异步加载；异步加载入口只在 `NodeSwitcherButton.Toggle` 中。
5. Tab 钩子会把焦点导向不可聚焦的容器，并在 TreeView 自身持有焦点时继续返回容器，Tab 无法正常离开。
6. TreeSelect 下拉时焦点留在搜索框，“移动焦点到节点”的模型在那里天然不可用。

## 方案比较

| 方案 | 做法 | 问题 | 结论 |
| --- | --- | --- | --- |
| A：恢复 Avalonia 焦点模型 | 把 TreeViewItem 改为可聚焦，逐项修补 OnGotFocus、过滤、禁用、异步加载 | 一棵树变成 N 个 Tab 停靠点；需要重写 OnGotFocus 规避 IsSelectable 异常；节点焦点视觉要新增；TreeSelect 仍不可用；header 内的用户内容和节点争抢焦点 | 不采用 |
| B：TreeView 维护键盘活动项 | TreeView 是唯一焦点目标，节点保持不可聚焦；活动项由 TreeView 内部维护；宿主通过 `HandleKeyDown` 转发 | 需要自己实现遍历、Tab 钩子和活动项视觉 | **采用** |

采用 B 的理由：

- 仓库已有同构先例：`CascaderView.HandleKeyDown` + keyboard candidate；`Select` 用 `HandleKeyDown` 向候选列表转发。
- 与 Ant Design rc-tree（`activeKey` + `treenode-active` 样式）对齐，符合 AtomUI 只做 Ant Design 体系的方向。
- 不改变节点的不可聚焦设计，不影响 header 内用户可聚焦内容。
- TreeSelect 可以直接复用。

## 关键设计决策

| 决策 | 选择 | 理由 |
| --- | --- | --- |
| 选择是否跟随方向键 | 单选且 `ToggleType=None` 时跟随；多选、勾选模式、`IsSelectable=false` 时只移动活动项，Enter / Space 提交 | 满足 #535 的“上下切换选中项”诉求，也是 Windows 原生树的习惯；多选 / 勾选模式下浏览不应改动业务结果（WAI-ARIA Tree 规范）。 |
| 是否新增公共属性 | 不新增 | 策略可以完全从 `IsSelectable` / `SelectionMode` / `ToggleType` 推导；遵守 TreeView “不擅自新增 public API”的不变量。 |
| 活动项是否公开 | 内部状态 | 应用通过 selected / checked / expanded 观察结果即可。 |
| Enter 的语义 | 提交（勾选入口有效则切换勾选，否则选择），不再沿用 Avalonia 的“Enter 切换展开” | 与 rc-tree 一致；展开由左右键负责。 |
| 键盘提交是否派发 `ItemClicked` | 不派发 | `ItemClicked` 表达指针点击；键盘结果通过 SelectionChanged / CheckedItemsChanged 观察。 |
| 禁用节点 | 跳过 | 与 `CascaderView` 跳过禁用项一致。 |
| 活动项视觉 | 新增伪类 `:keyboard-active`（TreeViewItem + Header 同步），复用 `NodeHoverBg`，只在键盘交互中显示 | 与 rc-tree 的 active 背景一致；不新增 Token；WholeLine 模式需要行级呈现。 |
| 展开入口 | 把 `NodeSwitcherButton.Toggle` 中的“未加载则发加载请求，否则切换”提取到 `TreeViewItem` 统一入口 | 键盘与指针共用一条路径，异步加载、动效、`IsAutoExpandParent` 行为一致。 |
| Tab | TreeView 重新实现 `ICustomKeyboardNavigation`，进入时落在自身，离开时交给默认顺序 | 基类是显式接口实现，无法 override，只能在派生类重新声明接口。 |
| 来源过滤 | 只处理 Source 为 TreeView 自身或宿主转发的按键 | header 内用户输入框的左右键、空格不能被树拦截。 |

## 本次文档变更

- 新增 `docs/controls/desktop/data-display/tree-view/keyboard-navigation-design.md`。
- 更新 `tree-view/overview.md`：键盘行为、`:keyboard-active` 伪类、集成关系（不再声明继承 Avalonia 键盘 / 焦点语义）、兼容性不变量、§8.8、验证策略。
- 更新 `tree-view/implementation.md`：源码结构（`TreeView.KeyboardNavigation.cs`）、键盘导航流、异步加载流入口、键盘与焦点处理、§7.8 活动项维护、维护不变量、验证范围。
- 更新 `tree-view/token.md`：活动项复用 `NodeHoverBg`。
- 更新 `tree-view/changelog.md`：2026-11-11 条目。
- 更新 `data-display/overview.md` 分类入口：登记键盘导航设计文档。

首轮文档先于实现书写；实现完成后已按实现修正（见下文“实现记录”），控件文档与源码一致。

## 实现记录（2026-11-11）

原“待实现”清单已完成，以下记录实现中新增或修正的决策。

### 原计划


1. `TreeView.KeyboardNavigation.cs`：活动项、遍历（先序 / 跳过规则 / 刚展开节点的单次布局）、按键分派（RTL）、提交策略、SelectAll、`OnKeyDown` / `OnGotFocus` 重写、`ICustomKeyboardNavigation` 重新实现、`HandleKeyDown` 宿主入口、活动项生命周期（收起上移、Clear 清空、detach 清空）。
2. `TreeViewItem`：统一展开请求入口；`:keyboard-active` 伪类；`OnKeyDown` 不处理按键。`NodeSwitcherButton.Toggle` 改为调用该入口。
3. 主题：TreeView `Focusable=True`；Header 固定 `Focusable="False"`；`ToggleCheckbox` / `ToggleRadio` `Focusable="False"`；`:keyboard-active` 在三种 `NodeHoverMode` 下的背景样式。
4. `TreeViewPseudoClass` 新增常量。
5. TreeSelect：下拉打开时转发按键到 `TreeSelectTreeView.HandleKeyDown`，关闭时清活动项；同步 TreeSelect 文档。
6. 测试：新增 `tests/AtomUI.Desktop.Controls.Tests/TreeView/TreeViewKeyboardNavigationTests.cs`，覆盖设计文档 §10。
7. Gallery：TreeView ShowCase 增加键盘操作示例（单选、勾选、异步加载）。
8. 视觉验收：用 `atom:Window` 宿主 Demo 检查 `:keyboard-active` 的 Light / Dark 与三种 hover mode。

### 实现中新增的决策

| 决策 | 原因 |
| --- | --- |
| TreeView 设置空 `FocusAdorner` | AtomUI 全局有默认焦点框，Tab 进入时会框住整棵树；活动项背景已经表达键盘位置（Ant Design 同样不框整树）。 |
| 新增内部开关 `IsKeyboardMoveSelectionEnabled`，TreeSelect 下拉树关闭 | 单选“选择跟随”在 TreeSelect 下拉中会让浏览直接改写 TreeSelect 的值（并可能触发关闭）；宿主模式下只允许 Enter / Space 提交，与 rc-tree-select 一致。 |
| 提交前检查 `IsHeaderEffectivelyEnabled` | TreeSelect 的最大选择数通过样式禁用 header（而非节点），只检查节点可用性会让键盘绕过上限。改为检查 header，是通用规则，不需要 TreeSelect 专用钩子。 |
| 活动项清理挂在 `TreeViewItem.OnDetachedFromVisualTree` | 容器清理分别发生在 TreeView 与 TreeViewItem 两层的 `ClearContainerForItemOverride`；容器离开视觉树是两者的共同出口，一处覆盖回收、换源、拖拽重建。 |
| 没有有效活动项时第一次导航键只确定活动项 | 宿主转发场景没有 GotFocus，第一次按键需要先落到当前选中节点；与 rc-tree 首次按键定位 activeKey 一致。 |
| TreeSelect 对 TextBox 来源的按键走 tunnel | 搜索框会自己消费 ↑ / ↓ / Enter，bubble 阶段收不到；做法与 Cascader 一致。 |
| `FloatableTreeView`（TreeViewFlyout）、`TransferTreeView` 保持可聚焦 | 它们没有宿主输入框，自身获得焦点后直接使用键盘导航。 |

### 验证

- `TreeViewKeyboardNavigationTests` 22 项、`TreeSelectKeyboardNavigationTests` 3 项通过。
- 相关范围（TreeView / TreeSelect / Transfer / Flyout / SemanticPart / Theme / Cascader）1079 项通过。
- `AtomUIGallery.Tests` 412 项通过。
- 测试过程中发现：把 `TextBox` 直接作为 `TreeViewItem.Header` 不会进入视觉树（header 走 `ItemTemplate`），来源过滤的测试改为从 header 子元素冒泡 KeyDown 验证。

### 脚本视觉验收

- 宿主为 `atom:Window` 的临时 Demo（仓库外，已删除），采样方式为原生 `screencapture -l`（NSWindow），RenderScaling=2；运行时 `AtomUI.Desktop.Controls.dll` 与 `.artifacts` 产物 SHA256 一致。
- 采样门禁：同状态两次截图 diff=0；TreeView 平移 1 DIP 时 diff=7264；恢复后 diff=0。
- Light / Dark × `Default` / `Block` / `WholeLine`，每组 4 项断言（像素 + 状态），共 24 项，加 2 项主题切换，全部 PASS（容差：通道 ≤ 8）：
  - Cmd+↓ 移到的活动项填充 = `NodeHoverBg`（Light `#F5F5F5`，Dark `#262626`），相邻节点保持背景。
  - Enter 选中后填充 = `NodeSelectedBg`（Light `#E6F4FF`，Dark `#15325B`），selected 优先。
  - 指针点击另一节点后，两个节点都没有 `:keyboard-active`，原活动项恢复背景。
  - 键盘再次移动时高亮恢复；焦点移出后高亮消失。
- 关键帧已人工查看：Default 只在内容框、Block 拉伸到剩余宽度、WholeLine 覆盖整行，与 hover 分层一致。
- 证据（截图与 results.md）保存在会话临时目录，未放入仓库。

## TreeSelect 弹层验收与输入家族焦点样式（2026-11-11）

### 发现

对 TreeSelect 下拉弹层补做脚本视觉验收，并按反馈对齐 Ant Design 的焦点样式。过程中发现五个问题：

1. **弹层活动项不显示**：`TreeSelectTreeViewItemTheme` 替换了节点 Template，没有把 `IsKeyboardActive` 投射到 header。节点上有 `:keyboard-active`，但负责绘制的 header 没有，所以弹层里看不到活动项背景。原有无头测试只断言了 `KeyboardActiveItem`，没有覆盖到这一点。
2. **全局方框**：输入家族宿主 `Focusable=True` 又没有关闭 `FocusAdorner`，Tab 聚焦时全局 `DefaultFocusAdorner` 会画一个方角框；嵌入式 `SelectFilterTextBox` 也会在外框内部画一个。Ant Design 的输入控件没有这类框。
3. **focus 外发光从未显示**：`BorderRenderHelper` 的几何路径丢弃了 `BoxShadow`，而输入 frame 固定使用 `InnerBorderEdge` / `OuterBorderEdge`，一定走这条路径。即使阴影画出来，Avalonia 12 中 `TemplatedControl` 默认 `ClipToBounds=True`，宿主也会把 frame 外侧的阴影裁掉。NumericUpDown 的 content frame 甚至没有绑定 `BoxShadow`。
4. **修复渲染后暴露出多余阴影**：主题给 `Filled` / `Underlined`、内嵌（Borderless）文本框和 OTP Borderless 格子都配了 focus 阴影，以前因为画不出来没有暴露。Ant Design 只给 `Outlined` 加 `activeShadow`。
5. **多选标签前多出 11 DIP**：模板把内层 `AddOnContentPresenter` 作为外层 add-on 的内容。外层只判断 Content 是否为 null，所以即使内层是空的，外层仍然可见，于是多选前缀缩进（7）加上面板间距（4）被保留下来。Select 同样受影响。

### 修复

- `TreeSelectTreeViewItemTheme` 投射 `IsKeyboardActive`，header 设为 `Focusable=False`。`TreeSelectKeyboardNavigationTests` 新增 header 伪类断言：去掉修复时该断言失败，加上修复后通过。
- `BorderRenderHelper` 的几何路径先以外边框边缘圆角矩形绘制外阴影。
- focus 阴影只在 `Outlined` 下设置，涉及 InputControlFrame、LineEdit、SearchEdit、TextArea、TextBox 和 OTP。
- 输入家族宿主设空 `FocusAdorner` 和 `ClipToBounds=False`，涉及 TextBox、LineEdit、SearchEdit、TextArea、Select、TreeSelect、Cascader、ComboBox、InfoPickerInput、RangeInfoPickerInput、RangeDatePicker、RangeTimePicker、ButtonSpinner、NumericUpDown。`SelectFilterTextBox` 也设空 `FocusAdorner`。
- `TextAreaDecoratedBox` 的裁剪下移到 `Border#TextAreaContentFrame`。
- `ButtonSpinnerDecoratedBox` 的 content frame 补上 `BoxShadow` 绑定。
- `AddOnContentPresenter` 的可见性跟随作为内容的控件的 `IsVisible`。

### 脚本视觉验收

两套验收都使用 atom:Window 宿主和原生 `screencapture -l` 截图，并且都满足以下条件：

- 运行期间窗口保持激活。中途有其他应用抢走焦点的轮次已判为无效：系统 `Deactivated` 会关闭下拉弹层，导致后续按键错位。
- 运行时 DLL 的 SHA256 与仓库产物一致。
- Demo 已删除。

1. **TreeSelect 弹层**：52/52 PASS。
   - 门禁：A/A=0，扰动 28189，还原 0。
   - 覆盖 Single、Checkable、Filter 三种模式，亮色和暗色：打开时无残留活动项；第一次 ↓ 建立活动项并显示 `NodeHoverBg`；浏览不提交；Enter 提交并关闭，或 Space 勾选且下拉保持打开；重开后恢复到已选或已勾选节点，单选时 selected 优先；Esc 关闭并清除活动项。
   - 焦点：Outlined 和 Error 外发光为 `#E6F4FF` / `#FFF2F0`（亮）、`#15325B` / `#2C1618`（暗），3–8 DIP 外为背景，无方框；搜索框外侧没有第二圈；LineEdit Outlined 有外发光，Filled 无外发光。
   - 标签：左偏移从 16 DIP 降到 5 DIP（1 边框 + 4 内边距）。
2. **输入家族聚焦矩阵**：20/20 PASS。Select、Cascader、DatePicker、TimePicker、NumericUpDown、SearchEdit、TextArea、TreeSelect、LineEdit 在亮暗两种主题下，外侧 2 DIP 都是 `ColorControlOutline` 合成色，3–8 DIP 外都是背景。

证据保存在会话 scratchpad 的 `issue535-treeselect-acceptance-evidence` 和 `issue535-focus-matrix-evidence` 中。

## 风险

- `TreeView` 改为可聚焦后会进入 Tab 顺序，可能影响已有页面的 Tab 行为；应用可通过 `Focusable=False` / `IsTabStop=False` 退出。需要在 changelog 和发布说明中注明。
- `Enter` 不再切换展开（Avalonia 原行为）；由于此前键盘导航完全不可用，实际影响有限。
- 勾选框改为不可聚焦后，依赖 Tab 到勾选框再按空格的用法失效，由 TreeView 的 Space 提交代替。
- 共享渲染修复会让所有走几何路径、且设置了 `BoxShadow` 的 `DashedBorder` 开始显示阴影。排查范围内只有输入 frame 和 `ContentType=Grid` 的 Card 受影响；两者都是主题本来想要的效果，但 Card 尚未做视觉确认。
- 输入家族宿主改为 `ClipToBounds=False`。如果应用依赖宿主裁剪来截断溢出的自定义 add-on 内容，现在需要自行裁剪。
- ComboBox、AutoComplete、Mentions、RangeDatePicker、RangeTimePicker、OTP 没有进入聚焦矩阵，结论依据的是共享主题推断，尚未截图确认。
