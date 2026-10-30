# NavMenu 项激活事务设计

本文档定义 NavMenu 菜单项激活的统一提交契约：指针与键盘用户输入按固定顺序完成“选择 + 动作”，程序化 `SelectedItem` 则是只更新选择的独立入口。公共契约与状态模型见 [NavMenu 桌面版架构设计](overview.md)，实现结构与交互处理见 [NavMenu 桌面版实现原理](implementation.md)，按住与键盘 active 视觉使用的 Token 语义见 [NavMenu Token 设计](token.md)。

## 1. 设计定位

项激活事务定义“用户操作一个菜单项”在什么时刻、以什么顺序产生选择变化和动作派发。它覆盖两种用户输入入口（指针、键盘）、三种有效模式（Inline、Vertical、Horizontal）和两类激活目标（叶子节点、带子菜单的父节点）；程序化选择仅作为选择协调边界参与本文档。

该设计的职责边界：

- 定义激活的提交时机、合法性与取消路径。
- 定义选中状态、节点命令和路由事件的统一顺序。
- 定义事务期间的视觉反馈通道。
- 定义交互处理器、选择协调器与主题层各自拥有的事务职责。

它不负责路由切换、页面生命周期、popup 展开动画策略或 hover 打开子菜单的延迟模型；后者由 Default 交互策略独立维护。

## 2. 设计原则

- **有序提交**：叶子节点先提交选择，再派发命令与 `NavMenuItemClick`。流程同步执行，但允许用户回调重入；选择被重入替换后，原节点尚未发生的事件和动作必须停止。
- **按下只准备**：指针按下只建立待提交事务并移动输入焦点，不改变选择、不切换子菜单、不执行命令、不触发路由事件。
- **单一用户激活管线**：指针合法释放与键盘 Enter/Space 共用同一个激活提交入口和同一事件顺序；程序化选择只进入选择协调器。
- **可取消**：从按下到合法释放之间的一切中断路径都无副作用地结束事务，已提交选择保持不变。
- **状态职责分离**：待提交项、键盘 active、已提交选择、打开路径是互不混用的状态概念，各自拥有唯一 owner；事务状态不得写入 `SelectedItem` 或选中路径状态。
- **状态先于事件**：开始派发 `NavMenuNodeSelected` 前，公共 `SelectedItem` 必须仍指向事件节点；事件处理器自身仍可重入修改选择。
- **命中即提交，未命中即取消**：合法释放以释放点是否命中按下项 Header 的视觉子树为唯一判据，等价于按钮类控件"按下与释放落在同一目标才成立"的提交语义。

## 3. 专项模型与 Public API

### 3.1 交互状态概念

| 状态 | 语义 | Owner | 公开性 |
| --- | --- | --- | --- |
| Hover | 指针当前位置的命中项视觉。 | 平台命中测试与 header `:pointerover`。 | 主题选择器 |
| 待提交项（press candidate） | "如果发生合法释放就激活这个节点"的事务目标。 | 交互处理器。 | 否 |
| Pointer hold | 指针事务目标当前仍被指针命中，在捕获期间显示 selected 背景且不改变既有文字颜色，但不提交选择。 | 交互处理器的 pointer owner。 | 否 |
| Keyboard active | 键盘漫游的当前项，作为键盘导航锚点并贡献 active 视觉。 | 交互处理器的 keyboard owner。 | 否 |
| Selected / Selected path | 已提交的导航选择及其祖先路径。 | `NavMenuSelectionCoordinator`。 | `SelectedItem` |
| Open path | 已展开的 inline 分支或 popup 分支。 | inline collapsed coordinator 与各模式打开状态。 | 行为间接体现 |

pointer hold 与 keyboard active 是两个独立 owner，可以同时指向不同菜单项；前者投影到 `IsPointerHold` / selected 背景 Token，后者投影到 `IsKeyboardActive` / active Token，不能合并为同一状态 owner（见第 6 节）。

### 3.2 激活事务状态机

```text
Begin（按下）
  记录待提交项与建立事务的指针身份
  待提交项进入 pointer-hold selected-background 视觉
  以 item header 为目标建立显式指针捕获
  输入焦点移动到待提交项
      ↓
Pointer Commit（合法释放）
  清理事务状态与指针捕获
      ↓
  叶子节点 → 选择协调器提交 → 节点命令 → NavMenuItemClick
  父节点   → 模式相关展开动作 → 节点命令 → NavMenuItemClick

Keyboard Commit（Enter / Space）
  直接进入相同的目标分派与提交顺序，不创建或清理指针事务

Cancel（一切中断路径）
  清理事务状态与指针捕获
  无选择变化、无命令、无路由事件
```

指针交互契约（设当前已选中 A，用户操作项 B）：

| 阶段 | 选择状态 | 视觉状态 | 导航与命令 |
| --- | --- | --- | --- |
| 指针移入 B | A 保持选中 | B hover | 不触发 |
| 在 B 上按下 | A 保持选中 | B 的背景与 selected 相同，文字颜色保持按下前状态；既有 keyboard active 保持 | 不触发 |
| 按住移出 B | A 保持选中 | B 清除 pointer-hold selected-background 视觉 | 不触发 |
| 移回 B 后释放 | B 提交为选中 | B selected | 触发一次 |
| 在 B 外释放 | A 保持选中 | 恢复普通状态 | 不触发 |

按下会调用待提交项的真实 focus 入口；是否取得焦点由当前 mode 与主题的 `Focusable` 契约决定。该动作不改写 keyboard-active owner：Inline 模式继续由菜单根持有 Tab 焦点，既有键盘导航锚点不会因指针事务建立或取消而丢失。

### 3.3 提交顺序契约

叶子节点提交按以下固定顺序执行：

1. 结束指针事务字段并释放捕获；pointer-hold 背景保留到选择提交与用户回调完成后再清除。
2. 选择协调器更新选中路径与容器 `IsSelected` / `IsInSelectedPath`。
3. 更新 `NavMenu.SelectedItem`。
4. 确认 `SelectedItem` 仍是目标节点；若已被同步观察者改写，原提交标记为 superseded 并停止。
5. 触发 `NavMenuNodeSelected`。
6. 事件返回后再次确认 `SelectedItem` 仍是目标节点；若事件处理器改写选择，停止原节点后续动作。
7. 执行节点 `Command`。
8. 触发 `NavMenuItemClick`。

父节点提交按以下顺序执行，不修改 `SelectedItem`、不设置选中路径、不触发 `NavMenuNodeSelected`：

1. 清理指针事务。
2. 按 effective mode 执行展开动作（见第 4 节）。
3. 执行节点 `Command`。
4. 触发 `NavMenuItemClick`。

顺序不变量：

- `NavMenuNodeSelected` 开始派发时 `SelectedItem` 必须是事件节点；较早的事件处理器可以重入改写它，因此后续处理器应按普通可变状态契约读取当前值。
- 节点命令不能从 `NavMenuNodeSelected` 再次执行；命令唯一入口是激活提交本身。命令执行者必须是 Click 的实际 Source；无命令子项的冒泡事件不得触发祖先命令。
- 同一节点的重复提交不重复触发 `NavMenuNodeSelected`；`NavMenuItemClick` 每次合法激活都触发。
- 被同步选择变化 supersede 的原节点不执行尚未发生的命令或 `NavMenuItemClick`，避免形成部分 invocation。

### 3.4 Public API 语义

`SelectedItem` 表示已经提交的导航选择。程序化设置 `SelectedItem` 直接走选择协调器，不经由事务状态、节点命令或 `NavMenuItemClick`。事件 `NavMenuItemClick` 与 `NavMenuNodeSelected` 的名称、参数和冒泡语义保持不变；本设计为用户激活补充顺序和 superseded policy。不引入控制提交时机的公共开关，指针激活只有“合法释放提交”一种行为。

## 4. 模式策略矩阵

| 激活目标 | Inline | Vertical / Horizontal / inline collapsed |
| --- | --- | --- |
| 叶子节点 | 提交选择 + 动作（同一顺序契约）。 | 同左。 |
| 父节点 | 切换子菜单展开状态（已开则收起，未开则展开）。 | 确保子菜单 popup 打开（已打开则保持）。 |

hover 打开子菜单的延迟模型属于 Default 交互策略的独立流程，不进入激活事务：指针在父节点上悬停仍按既有延迟打开 popup，与按下、释放事务互不干扰。inline collapsed 状态使用 effective vertical 策略参与同一事务契约。

## 5. 架构、文件结构与职责

```text
NavMenuInteractionHandlerBase
  owns 激活事务状态（待提交项、建立事务的指针身份）
  owns 统一提交入口 CommitItemActivation
  owns 释放合法性与取消路径判定
  ↓ 按激活目标分派
InlineNavMenuInteractionHandler
  父节点激活 = 切换 inline 展开状态
DefaultNavMenuInteractionHandler
  父节点激活 = 确保 popup 打开
  独立拥有 hover 延迟打开 / 关闭流程
  ↓ 叶子节点提交
NavMenuSelectionCoordinator
  只拥有已提交选择状态，事务状态不进入
  返回目标选择是否仍为当前提交
  ↓ 命令与事件派发
NavMenuItem（ICommandSource）
  节点命令唯一执行者
```

- 交互处理器基类持有激活事务：待提交项与建立事务的指针身份共同描述一次事务，非空待提交项即代表事务存在，不维护额外的有效性布尔标记。
- `CommitItemActivation` 是鼠标合法释放与键盘 Enter/Space 的公共提交入口，内部按激活目标分派为叶子选择提交与父节点展开激活。叶子选择被同步重入替换时，选择协调器返回未提交结果，入口不再执行命令或派发 `NavMenuItemClick`。
- 选择协调器不感知事务：它接收的每次调用都是已判定合法的提交。
- 键盘漫游 coordinator 只移动 active/focus，不进入提交管线；方向键移动不构成激活。

## 6. Template、组合与集成契约

### 6.1 Pointer hold 视觉

事务期间待提交项的视觉反馈由 header 的 pointer-hold 状态承载，遵循：

- 只使用 selected 背景 Token（light：`ItemSelectedBg`；dark：`DarkItemSelectedBg`）；文字继续由按下前的普通/hover 规则决定，不切换到 selected 前景 Token，也不写入任何选择状态。
- 优先级低于真实 `Selected`，高于 hover/default；已选中项继续由真实 selected 状态表达。
- 背景变化使用 `MotionDurationSlow`（默认 300ms）和 `ItemBackgroundMotionEasing`（默认映射 CSS `ease` 等价曲线 `Spline(0.25,0.1,0.25,1)`）；hover 灰在 mouse down 后平滑进入 selected 背景，mouse up 才提交 selection。Base、Inline、Horizontal header 主题必须消费同一个 NavMenu Token，不直接构造 easing。
- pointer hold 与 keyboard active 使用独立 owner 和独立主题输入：`IsPointerHold` 只表达指针按住，`IsKeyboardActive` 只表达键盘漫游。清除其中一个 owner 不得覆盖另一个。
- 生命周期与事务状态机一致：按下置位、拖出清除、移回恢复、提交或取消时终结。

### 6.2 不依赖捕获期间的原生 hover

按下建立指针捕获后，平台在捕获期间只在命中元素与捕获元素一致时保持 pointerover 跟踪；命中待提交项内部子元素即视为不匹配并清除 hover。因此事务期间必须用 `IsPointerHold` 显式维持 selected-background pressed 视觉，不能直接依赖 `:pointerover`，也不能写入 keyboard active 或 selected 状态。该状态只覆盖背景，不覆盖文字颜色。捕获目标必须是带有 `Cursor=Hand` 的 item header，确保按住期间指针不会退回默认箭头。未按下时的普通 hover 语义不变。

视觉状态交接顺序也是契约：按下时先置 `IsPointerHold`、再建立捕获，避免捕获改变 `:pointerover` 时短暂回落到默认背景；终止时先解除 capture-lost 订阅并释放捕获、再清除 `IsPointerHold`，避免释放路径短暂出现透明背景。

### 6.3 主题边界

pointer-hold 视觉通过 header 内部 `IsPointerHold` 状态表达，不新增模板节点，不引入新的公共语义区域。事务状态不写 `IsKeyboardActive`、`IsSelected`、`IsInSelectedPath` 或任何 selected 主题输入。`NavMenuItem` 的 pressed 伪类保持过程状态语义，不作为事务视觉的输入。

## 7. 核心流程与生命周期

### 7.1 按下（Begin）

按下处理只执行以下步骤：

1. 确认主按钮、节点有效且属于当前菜单。
2. 取消可能存在的旧事务。
3. 记录待提交项与指针身份。
4. 待提交项进入 pointer-hold selected-background 视觉，文字颜色不变。
5. 以 item header 为目标建立显式指针捕获，保证控件外释放、窗口切换等路径都能收到终止事件，并保持按住期间的手型指针。
6. 尝试让待提交项取得真实焦点；不修改 keyboard-active owner。

不调用选择协调器、不执行命令、不触发路由事件、不切换 inline 展开状态。

### 7.2 释放（Commit 判定）

释放时重新命中测试，同时满足以下条件才提交：

- 是建立事务的同一个指针。
- 仍是主按钮释放。
- 释放点命中待提交项 Header 的视觉子树（按下与释放落在同一 Header 上）。
- 节点仍属于当前菜单，且自身与全部语义祖先仍然可用。
- 待提交项未被卸载、替换或遗忘。

命中判定使用释放点对待提交项 Header 视觉子树的包含检查，不使用捕获期间不可靠的 `IsPointerOver`。Inline 展开子项属于父容器视觉子树，但不属于父 Header；按下父 Header 后在子 Header 释放必须取消。

在调用命令和触发路由事件之前，必须先完成指针捕获释放与事务字段清理，避免命令执行过程中删除节点、切换页面或重入菜单时残留旧事务状态。

### 7.3 取消（Cancel）

以下路径统一取消事务，不产生选择变化、命令或 `NavMenuItemClick`：

- 释放点未命中待提交项（另一节点或菜单外）。
- 当前事务指针的捕获丢失（含窗口失活等平台路径）；其他指针的 capture-lost 事件忽略。
- 待提交项在按下后被移除或禁用。
- 菜单 detach 或交互处理器被替换。
- 非主按钮释放。
- 新的按下替代旧事务。

捕获清理只释放仍由事务记录的 Header 持有的捕获。若平台已将捕获转交另一控件，旧事务只移除自己的字段、订阅和 pointer-hold，不调用 `Capture(null)` 取消新控件的捕获。

容器回收、clear 与 rebind 通过统一遗忘入口通知交互处理器，使待提交项引用与 pointer-hold 视觉同步失效；该入口同时失效选择协调器和键盘 active 引用，与既有容器生命周期规则一致。

### 7.4 键盘提交

Enter/Space 对 active 项执行 `CommitItemActivation`，与指针合法释放走同一分派、同一顺序契约。带子菜单项的 Enter 优先执行展开或进入子菜单，不提交选择：Inline 父项走 `CommitItemActivation`（切换展开并派发命令与 `NavMenuItemClick`）；Vertical、Horizontal 及 inline collapsed 的父项 Enter 打开子菜单并把 active 移入第一个可交互子项，不派发命令与 `NavMenuItemClick`（键盘下降导航语义）。方向键移动不构成激活。

## 8. 资源、性能与 AOT 边界

激活事务新增的 `PointerCaptureLost` 订阅挂在实际捕获的 item header 上，只存在于按下到提交/取消之间，并在终止路径先解除再释放捕获。事务期间的主要操作为：

- 按下 / 释放更新固定数量的状态引用并建立或释放捕获。
- 按住阶段的移动处理从待提交项根执行一次 `GetVisualAt` first-hit 命中测试；找到首个可见命中后停止，不枚举全部命中视觉。
- 提交路径复用既有选择协调器与命令派发。

设计不使用反射、动态发现或运行时注册，保持 NativeAOT 可分析。事务状态由交互处理器字段持有，随 handler detach 与容器遗忘入口释放，不形成跨生命周期的强引用。该 first-hit 调整只有框架调用形态的结构收益；未进行前后计时，因此不声明页面加载或交互耗时百分比。

## 9. 兼容性与定制边界

以下契约为不可破坏边界：

- `SelectedItem` 只表示已提交选择；任何按下阶段的行为不得改变它。
- 指针激活只有"合法释放提交"一种行为，不提供 Press / Release 提交模式开关。
- 叶子提交顺序与父节点提交顺序（第 3.3 节）不得重排；`NavMenuNodeSelected` 开始派发前的 `SelectedItem` 必须是事件节点，事件处理器自身仍可重入修改选择。
- 键盘与指针提交必须共用同一提交入口与顺序。
- 程序化 `SelectedItem` 只提交选择，不得隐式执行节点命令或触发 `NavMenuItemClick`。
- 同步选择重入必须使用 superseded policy，不得发布已被 pre-event observer 替换的陈旧选择事件，也不得继续原节点尚未发生的 invocation。
- 取消路径清单（第 7.3 节）是封闭集合；新增中断路径必须归入取消语义，不得产生部分提交。
- pointer-hold 视觉必须使用与 selected 相同的背景 Token、不得覆盖文字颜色，也不得把 `IsPointerHold` 写入 `IsSelected`、`IsInSelectedPath` 或公共 `SelectedItem`。
- 替换 ControlTheme 的实现方承担：让 header `IsPointerHold` 视觉使用 selected 背景且保持原文字颜色，同时保持 pressed 与 committed selection 两套独立状态输入。

## 10. 验证要求

| 层次 | 验证内容 |
| --- | --- |
| 按下零副作用 | 已选中 A 时按下 B，`SelectedItem`、选中路径、`NavMenuItemClick`、`NavMenuNodeSelected` 与节点命令均不变。 |
| 提交 | 同项按下并释放：先更新选中与 `SelectedItem`，再触发 `NavMenuNodeSelected`，命令执行时公共状态已是新值，`NavMenuItemClick` 触发一次。 |
| 同步重入 | `SelectedItem` 观察者在提交中改写到另一节点：不发布原节点的陈旧选择事件，不执行原节点命令或 `NavMenuItemClick`；最终选择与事件节点一致。 |
| 重复提交 | 同一节点重复合法激活不重复触发 `NavMenuNodeSelected`，`NavMenuItemClick` 每次触发。 |
| 取消路径 | 释放到另一节点、释放到菜单外、当前指针捕获丢失、按下后移除或禁用节点、detach、非主按钮释放、新按下替代：均零提交；其他指针 capture-lost 不误取消。 |
| 移回提交 | 按下 B、拖出、移回 B 后释放：提交 B 且仅一次。 |
| 父节点激活 | Inline 父节点在合法释放时切换展开一次；Default 父节点在合法释放时打开 popup；按下阶段不切换。 |
| 键盘一致性 | Enter/Space 与指针释放的激活分派、事件顺序和命令执行一致。 |
| 视觉契约 | pointer-hold 与 keyboard-active 独立持有、分别投影；pointer-hold 的背景与 selected 完全一致、文字颜色不变且不提交选择，keyboard-active 使用 active Token；两者都低于真实 selected 优先级。 |
| Gallery 手工走查 | 导航在按下阶段右侧内容不变，合法释放后切换一次；未执行手工走查时不得将其写成自动验证结果。 |
