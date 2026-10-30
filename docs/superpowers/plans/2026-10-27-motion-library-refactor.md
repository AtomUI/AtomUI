# Motion Library Refactor Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task in the approved worktree. Do not create another branch/worktree or commit without a separate user instruction.

**Goal:** 用可扩展 Motion 继承体系替换 Actor 依赖，统一执行/终态和布局接入，校准内置轨迹并完成定向与真实 Gallery 验证。

**Architecture:** Core 拥有定义、编译后的轨迹与单次运行；真实 Control 是目标，必要布局能力由 Controls 提供。原生时钟为可靠执行基础，只有完成/替代/命中通过能力验证的组合才能进入合成路径。控件继续拥有宿主、业务状态与事件。

**Tech Stack:** 当前仓库 .NET/Avalonia、C#、编译 AXAML、既有 Headless 和原生 Gallery 工具；不新增第三方运行时依赖。

**Spec:** [正式 Motion 架构](../../architecture/systems/motion/overview.md)，阅读该入口下全部专题；讨论记录不是唯一合同来源。

## Global Constraints

- 当时在 `feature/motion` 分支的独立工作树中开发，来源 `6c647fef191d25d3e697b6e77db922b2eb907aa7`，创建已获用户明确“我同意”；该分支已于 2026-10-30 线性合并入 `release/6.0`，分支与工作树均已删除。
- 用户允许不兼容旧 Motion API；布局、输入、滚动、绑定、Semantic Part、宿主和业务合同不得隐式降级。
- 公共基类 Motion 与具体子类；命名空间 AtomUI.Animations；AXAML 统一 atom:，禁止恢复 Effect、静态预设或通用 Actor 模型。
- 一个目标一个显式请求；并行通过 ParallelMotion；子项 FillMode 只接受默认 Both，组合自身控制整体结束。
- 核心 PlayAsync 不等待布局；NotReady 不替代已有运行；外部取消恢复基线，替代按共同通道交接。
- 内置有限入离场默认 Both；终态记录与运行资源分开，Reset/下一成功请求/detach/owner 失效释放终态。
- Transitions 在 ControlTheme style setter；可等价声明的绑定优先 AXAML；语义定制使用专用 Style。
- 禁止程序集扫描、反射绑定、全局宽泛 animator 注册或 Core 对 Controls/Desktop 反向依赖。
- 新验证默认写 tests/<owning-project>/Temporary/MotionRefactor，完成后删除其 helper/fixture；默认无净新增永久测试。
- 只执行显式模块路径或精确过滤的定向验证；不自行运行 change/full；不创建 commit/amend/rebase。
- 60/120FPS 必须有冻结环境与真实呈现证据；未指定最低性能设备不得以开发机或批次 Rendered 代替保证。

## Review Focus

- 首 tick 前取消 Forward/Both 不得将默认零值或旧终点写回新请求。
- 覆盖期间绑定/主题变化，取消与 Reset 必须恢复最新有效基线。
- Popup 准备/离场与模板换代并发，不闪首帧、不关闭新宿主、不吞掉重新打开。
- 固定父尺寸和嵌套滚动中，实际视口、子项命中与绘制必须一致。
- 第三方属性/曲线及组合不得依赖 Desktop 注册，不因宽泛 ITransform handler 改变行为。

## Files and Ownership

| 范围 | 文件与职责 |
| --- | --- |
| 正式文档 | docs/architecture/systems/motion/*.md、Core/Controls 模块入口与架构导航 |
| 新 Core 定义 | src/AtomUI.Core/Animations/Motion/{Motion,MotionContext,MotionBuilder,MotionChannel,MotionResult}.cs |
| Core 执行 | 同目录 Playback/：请求身份、轨迹快照、属性覆盖、原生执行、终态与附加接入 |
| 内置动效 | 同目录 BuiltIn/：Fade/Zoom/Slide/Move/Expand/Collapse 与 Sequence/Parallel |
| 布局能力 | Core 内公开能力合同；src/AtomUI.Controls/Primitives 下实际内容视口原语 |
| 旧基础设施 | src/AtomUI.Core/MotionScene、Controls/Primitives/Motions 与 PopupMotionActor，消费者迁移后删除 |
| 消费者 | Popup、Dialog、Drawer、TreeView、Collapse、Expander、NavMenu、FloatButton、Badge、FeedbackStack、直接派生消费者及 Themes |
| 验证 | 既有 Core Motion 测试、Desktop Motion/Popup/TreeView 等测试、第三方临时 fixture、Gallery 动效探针 |

## Task 0 正式合同与基线

- [x] 创建 Motion 正式系统文档及导航，明确目标/当前差异，无 superpowers 事实依赖。
- [x] 按独立文档审查补齐内置规格与组合子项 FillMode。
- [x] 运行现有 Core AbstractMotionTests，基线 1 passed/0 failed。
- [x] 检查正式文档全部相对链接、atom:、状态区分和 git diff --check。

## Task 1 框架能力与运行合同原型

**Files:** tests/AtomUI.Core.Tests/Temporary/MotionRefactor/MotionCapabilityTests.cs；按需临时原生 Gallery 场景；证据写本计划旁的 progress 记录。

**Interfaces:** 消费当前 Avalonia 公共 API；产出原生/合成能力矩阵、布局能力签名和后续执行器所需的明确限制，不修改平台依赖默认值。

- [x] 先写可撤销属性覆盖验证：底层 opacity 在覆盖期间 .8→.6，覆盖 .3，释放后 .6，绑定继续有效。
- [x] 验证原生 Animation 的 Delay、None/Forward/Backward/Both、首 tick 前取消、同刻新请求与完成行为；首 tick 默认值风险已由原型复现，其余随新执行器验证。
- [x] 核验合成的完成/故障、当前呈现、替代、变换后命中；没有可靠信号的组合不得标为支持。
- [x] 验证局部 SetAnimator 可精确覆盖指定 setter，不需要 Desktop 初始化；源码发现静态 setter 字典持有，核心不采用逐次临时注册路径。
- [x] 验证同一原生时钟可表达多曲线/时长/关键帧与组合保持；布局能力通过独立无友元消费验证。
- [x] Run: `dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj --framework net10.0 --filter FullyQualifiedName~MotionCapabilityTests`；逐项记录实际结果，明确使用路径。

## Task 2 定义 构建与轨迹验证

**Files:** Core Animations/Motion 下定义文件、不可变内部轨迹；临时 MotionDefinitionTests。

**Produces:** 非 virtual `Task<MotionResult> Motion.PlayAsync(Control target, CancellationToken cancellationToken = default)`；受保护 `Build(MotionBuilder builder, in MotionContext context)`；只读请求标识/Outcome/WasAnimated 结果。

- [x] 先写测试验证独立目标快照、参数变更只影响后继请求、非法时间/数值在属性写入前失败。
- [x] 实现 Opacity、TranslateX/Y、Scale/ScaleX/Y、Rotation 的 double 通道与 From/To/关键帧，统一 Scale 与轴 Scale 冲突校验。
- [x] 构建同步且仅准备一次，Context 无可改写目标；配置与目标操作验证 UI 线程。
- [x] 实现正式规格中的贝塞尔与家族默认参数、DIP/比例解析、Origin；密集采样与端点对照原始参数。
- [x] Run: Core 项目仅过滤临时 MotionDefinitionTests；运行现有 StateMotionContractTests 确认独立初始化未被接管。

## Task 3 单次执行 终态与显示隐藏

**Files:** Core Motion/Playback、Motion 附加入口；临时 MotionPlaybackTests/MotionBindingTests。

**Consumes:** Task 1 能力矩阵、Task 2 轨迹与结果；**Produces:** 不依赖 Actor 的真实 Control 播放、Reset、请求当前性核对和类型化通道适配。

- [x] 先写测试验证同一目标整体替代、NotReady 不抢占、共有通道连续及旧独有通道恢复。
- [x] 实现每目标请求身份与独立资源，不引入进程级强引用目标字典。
- [x] 按验证过的原生能力播放；取消、异常、detach、零时长统一收敛，正常完成不依赖 easing 值或 watchdog。
- [x] 实现 FillMode 端点转交及终态覆盖，Reset 活动时拒绝；新成功请求和 detach 释放旧终态，不复活旧变换。
- [x] 写同步通知重入与清理异常测试，确保旧 continuation 不能修改新 owner。
- [x] 实现 atom:Motion.IsVisible/Enter/Leave 的声明接入、首次稳定基线和目标生命周期；不增加通用视觉包装。
- [x] Run: 精确过滤 MotionPlaybackTests/MotionBindingTests；保留源码路径和运行资源计数证据。

## Task 4 内置类与组合 第三方消费

**Files:** Core Motion/BuiltIn、组合编译；第三方临时测试 fixture；现有 Core Motion 测试同步。

**Produces:** 正式规格全部实际需要的 Motion 子类、SequenceMotion/ParallelMotion 与强类型属性扩展。

- [x] 先写家族方向、端点、原点和默认曲线测试；确认旧 Move 中段/单方向缩放不再出现。
- [x] 实现各家族，定义不持有运行资源；公用构建逻辑不演变成另一套继承执行框架。
- [x] 测试组合 Delay、父 Duration 缩放、子项 Both 限制、段间保持、Parallel 同通道拒绝和整体取消。
- [x] 实现属性精确适配和自定义曲线路径，不依赖全局类型扫描/注册。
- [x] 独立程序集继承 Motion、构建轨迹、编译 atom: AXAML 并播放，验证没有友元依赖。
- [x] Run: Core 相关精确过滤及 fixture 构建；同步已替代的旧 Core Motion 测试，避免保留实现镜像测试。

## Task 5 目标与布局消费者迁移

**Files:** Controls 内容视口；Core 布局合同；Desktop Popup/Dialog/Drawer、Collapse/Expander/NavMenu/TreeView、FloatButton/Badge/FeedbackStack、对应 Themes/语义路由/Gallery 与现有测试。

- [x] 先写原触发条件的失败证据，覆盖普通视觉无 Actor、固定父约束/嵌套滚动下实际视口命中、TreeView 反向。
- [x] 实现布局原语，只负责自然内容、期望占位、实际视口和完整排列；Core 不反向依赖 Controls。
- [x] 将三控件的合理 ContentExpansion 算法迁入能力模型，保留动态目标原截止时间、四方向锚定与舍入。
- [x] 普通视觉模板复用真实节点；需要边界时静态声明，AXAML 可等价绑定一并迁移，语义 Style 路由按真实树更新。
- [x] Popup 保留显示前准备与关闭意图核对；Dialog/Drawer 的组件参数按正式规格单列校准；不把物理宿主交给 Core。
- [x] BackTop/TreeView/FeedbackStack 清理各自的取消 guard 缺陷，保持业务事件和键盘合同。
- [ ] 运行每次实际修改模块的 iterate 明确 --path；Gallery 变化精确验证对应页面，Light/Dark/方向走查。

## Task 6 删除旧体系与最终验收

**Files:** 删除已无消费者的 MotionScene/Actor 基础设施；同步正式 verification 与模块现状、Gallery/API/主题与迁移说明。

- [x] 搜索 source/AXAML/tests/generator 确认无旧 Actor/AbstractMotion 路径，移除旧全局 animator 依赖与死几何；不留长期双实现。
- [x] 清理本计划临时 tests、fixture 和专用产物；更新原有高价值契约测试，不默认新增永久套件。
- [x] 对清理后的实际改动模块执行 iterate/精确过滤；记录当前工作树结果，不自行运行全量回归。
- [ ] 用原生 Gallery 测首次/重复/并发/反向、布局输入、资源归零；真实呈现与 batch 观测分开。
- [ ] 验证声明平台、NativeAOT/裁剪与第三方 fixture；没有最低设备/平台证据时明确未验证，不能声称已保证60/120FPS。
- [x] 独立全分支代码审查，修复重要问题并提供 RED→GREEN；保持用户 review 前不提交。

## Execution Ledger

Task 0: 正式文档已建立，独立审查两项缺口已修订；Core AbstractMotionTests 基线 1/1，未改产品源码。

Task 0: complete — 七个正式页面与模块/架构导航通过链接和命名检查；独立文档复核 nothing important left；无提交。

Task 1: 原型 MotionCapabilityTests 4/4 passed：覆盖恢复最新绑定、首 tick Forward 默认值、非视觉 Animatable 框架时钟、局部 animator。

Task 1: Ruling: 先采用每次执行独立的非视觉 Animatable 进度源，复用框架全局时钟 — 原型证明可用且隔离旧取消回写，不增加 Control/Actor；合成当前值与可靠完成尚未验证，不提前启用。若帧预算不达标需继续优化后端，不能宣布60FPS通过。

Task 1: Ruling: Animation.SetAnimator 的静态字典持有 setter，不能对每次运行创建临时注册 — 原生进度使用内置 double animator，自定义轨迹在类型化通道求值，不引入无界注册保留。

Task 2/3: 核心首批 RED→GREEN — 新 API 断言先因缺少 Motion 失败；5 个真实播放契约先因缺少新类型失败，加入独立进度/呈现覆盖/请求身份后与原型共10项通过。

Task 3: 边界 RED→GREEN — Forward Delay 冻结底层绑定、初始投影重入错误标记 WasAnimated、跨家族继承旧 Scale、零时长停在起点、完成后重播变 no-op 均先出现失败，分别修正延后覆盖、真实时钟启动计数、按通道身份交接与中性基线、零时长采样顺序及仅活动请求 retarget。

Task 2/3: 最新定向验证 `dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj --framework net10.0 --filter 'FullyQualifiedName~MotionDefinitionTests|FullyQualifiedName~MotionPlaybackTests|FullyQualifiedName~MotionCapabilityTests|FullyQualifiedName~AbstractMotionTests' --no-restore --verbosity minimal` → 15 passed/0 failed。任务仍未完整完成：完整轨迹家族、组合、第三方、布局与消费者接入待做；临时测试尚未到清理阶段。

Task 1: Ruling: 当前原生变换路径不接管独立 RenderTransform 来源，准备期明确拒绝并要求模板隔离 — 公共属性通知不暴露被覆盖来源的所有变化，不能用轮询/持续帧回调维持终态期间的动态合成。普通透明度绑定覆盖已验证保留并恢复最新值；合成变换能力继续单独验证。

Execution: 用户未授权提交，阶段记录与最终评审针对当前未提交工作树；不执行技能流程中的自动 commit 步骤。

Task 2/3/4: 轨迹家族、组合、类型化属性、自定义子类、附加可见性与布局能力已实现；Core 精确过滤45 passed/0 failed。独立第三方 AXAML、播放和外部布局能力 fixture 输出 THIRD_PARTY_AXAML_PLAYBACK_LAYOUT_OK（exit0）；尚未完成 AOT 与原生帧预算验收。

Task 5: Expander/Collapse 改为 ContentViewport 与主题声明式 Motion，原有几何、反向和滚动输入39项通过，公共布局原语临时验证2项通过；行为和语义迁移的 detach 单项已通过，待合并过滤验证。

Task 5: Ruling: detach 验证实际视觉树挂载与资源释放，重新挂载仍严格核对业务 IsExpanded。Avalonia Visual.OnDetachedFromVisualTreeCore 调用 UpdateIsEffectivelyVisible(true)，该属性不代表目标仍挂载。旧 Actor 固定 alpha0 的实现断言改为恢复 underlying opacity1；固定父约束、命中与滚动合同保持严格验证。

Task 5: NavMenu Inline 已迁移主题声明和真实内容视口，旧定向22项通过。TreeView/TreeSelect/Transfer 共享视口已迁移，反向测试先因缺少视口失败再通过；移除 TreeView 丢弃反向请求与 switcher 播放期禁点 guard。Expander、Collapse、NavMenu、TreeView 状态回放/语义/几何合并精确过滤214 passed/0 failed。

Task 5: BackTop 已移除专用显隐执行器、两个方向 guard 及 Actor 包装，直接在既有 Panel 上声明 Fade；快速反向临时验证先失败后通过，与原进度环及语义测试合计32 passed/0 failed。

Task 5: Popup、Flyout、FlyoutHost、ContextMenu 与 DataGrid 过滤弹层的定义类型已迁移；Popup 使用真实 PopupSurface : Panel、单次布局准备、作用域预隐藏与双重关闭身份核对。Popup Lifecycle/Margin 与迁移临时验证34 passed/0 failed。Direct Popup 旧测试的未挂载模拟 Actor 已替换为实际宿主主题和视觉表面；保留无逻辑 owner 条件，补上 visual attach 就绪路径，未就绪/无表面的宿主按稳定关闭合同处理。

Task 5: FloatButtonGroup 使用静态 Border 隔离 list 自定义变换，去除独立取消源和两套显隐异步执行器；定义一次性捕获 trigger DIP 尺寸，时长和启用状态由主题绑定。四方向快速反向、实际矩阵连续性及既有 overlay/语义/BackTop 合并过滤51 passed/0 failed。

Task 5: Drawer 使用面板 Border 与遮罩两个真实目标，移除旧 Actor、Move 参数误用和基于 Task.Delay 的收尾。按正式组件规格校准面板 .7↔1、±面板尺寸、CSS ease 与 Slow 遮罩；模板按方向约束实际表面主轴。新增临时测试先证明缺少新目标、位移尺寸错误、关闭期间禁用/换模板不收敛，再逐项修正。与既有布局、窗口迁移、push 和语义测试合并37 passed/0 failed；最终清理后的重验尚未执行。

Task 5: Message/Notification 的实际阴影 Frame 已直接作为目标，移除内部 Actor 包装。FeedbackCardMotion 改为 Build 轨迹，协调层不再持有两套 CTS、过渡订阅或等待旧任务清理；复用 Core 的连续替代。原有轨迹/协调契约测试迁为真实 Border 与全局受控时钟，合并临时验证30 passed/0 failed。初始 opacity 通知中重入 Dispose 的覆盖泄漏已先复现再修正。manager、真实关闭和语义集成的扩大定向验证仍在进行。

Task 5: Feedback 六类精确过滤（轨迹、协调、真实卡片关闭、manager 堆叠、Notification Semantic Part、临时迁移）82 passed/0 failed。原有八个关闭用例迁为两种真实卡片的四组契约场景，保留数量与独立检错范围；关闭中换模板/禁用从已呈现的入场中段触发，避免将未显现且已处于离场终点的 no-op 当作仍需播放。

Task 5: Badge Count/Dot 迁为共享 BadgeMotionSession 与 Motion 子类；私有表面隔离语义节点的独立变换。standalone Dot 的静态显示路径已显式保留；同 layer/element 反向不再 remove/add。目标分离和挂载连续性均有先失败证据，Badge/FloatButton semantic、尺寸与可见性定向41 passed/0 failed。剩余实际旧 Actor 消费者为 Overlay Dialog。

Task 5: Ruling: Overlay Dialog 暂时统一使用已验证的原生 Motion 时钟 — 当前 Avalonia 公共 CompositionAnimation 没有动画完成通知；旧路径依靠提交后 duration+32ms DispatcherTimer，不能提供可靠结束、替代和逻辑命中合同。移除这一猜测完成路径，不提前宣称新合成后端可用；若原生 Gallery 帧预算不达标，需要继续优化后端，不能用定时器或删特效恢复“通过”。

Task 5: Overlay Dialog 已改为实际遮罩目标和私有 Border，语义 wrapper 留在内部 ContentPresenter，原有专用 Style route 保持不变。表面普通 Zoom .2、入场 Slow/离场 Mid、无显式锚点使用面板中心，去除旧内容重复淡出。三个异步生命周期操作先登记任务再执行，关闭先替代入场再取消旧令牌。临时真实目标/框架时钟/反向/释放测试先失败后通过；原有 Dialog 集成迁移正在验证。

Task 5: Dialog 六类精确过滤98 passed/0 failed；删除旧体系后的 Native Window / 生命周期、Notification render-only、反馈层回收合并过滤86 passed/0 failed。原五个 compositor 实现隔离用例替换为框架时钟、首尾帧、冻结原点及反向合同，测试数量不增加。

Task 6: 旧 MotionScene、Controls.Primitives.Motions 与基础 TransformOperations 全局注册已删除。Core 原有“等待最长过渡”用例迁为“Parallel 等待最长子项”，连同临时能力/算法/生命周期验证48 passed/0 failed。新增故障验证证明初始默认值投影缺失与原始异常被清理异常覆盖，已分别 RED→GREEN；非法布局方向准备期拒绝原本已成立，不重复改实现。

Task 6: Release Gallery 构建0警告0错误，已核对 worktree绝对BINARY与PID并正常停止。CUA getApp(绝对BINARY) 返回 Invalid app，按 gallery-local-debug 强制规则停止视觉自动化，未改用名字/Bundle ID。Gallery 四类精确过滤12 passed/0 failed；Badge 几何在同一 adorner 坐标系保持原右上锚点，专用 Style 与高亮合同不放宽。当前进行原生 Gallery 3进程采样，采样期间不并行构建/测试；AOT、临时测试清理、清理后验证及最终独立审查尚未完成。

Task 6: 原生 all 采样首进程96动作完成；第二进程 BorderBeam observe 无中间值，整组 FAIL，未删除失败以挑选通过。改为聚焦本次关键消费路径：submenu、sidebar 分别3进程、每方向24动作，通过中间值和批次完整性检查。子菜单属性间隔中位约8.32ms，Rendered批次中位约8.22/8.27ms，close P99约21.05ms；侧栏Rendered中位8.33ms、P99收缩10.41/展开12.33ms，展开layout pass P99约13.05ms。批次不是display present，未声明60/120FPS或最低设备达标。

Task 6: scripted-visual-acceptance 临时 GalleryMotionVisualTests 1 passed，以真实 Gallery ExpanderBasicShowCase、产品主题、1100×800窗口和Skia合成帧执行4次框架鼠标点击，覆盖展开、收起中反向、同实例回到折叠；宽度保持、自然内容和高度恢复断言均通过。已实际查看闭合/中间/展开3帧：字号与换行一致，中间帧裁剪而非缩放、透明度随动、边框与相邻内容自然推进；视觉 PASS仅覆盖该场景，不等于原生macOS交互或屏幕present。evidence audit clean，媒体清理与临时测试删除仍待执行。

Task 6: 媒体 evidence cleanup removed=true/files_removed=4/audit clean，未留仓库内取证二进制。Gallery NativeAOT osx-arm64 发布通过；依赖 ReactiveUI trim/AOT 汇总及本机 Homebrew库 macOS部署版本链接警告保留，未归因于Motion。第三方AOT fixture 同步Pump曾在JIT/AOT同时超时，改为真实Dispatcher.MainLoop后运行成功，非AOT特有故障；扩展到Padding属性消费时发现 IsValidValue(object) 的IL2026，改为T类型验证器避免反射转换，再次发布无新Core IL警告。

Final: 独立只读review完成，Request changes三项均进入一次修复pass：P1 IsOpen绑定关闭/反向、P1关闭期间表面换代、P2合法AddOwner属性被拒。临时4个Popup原复现和2个Core直接/组合用例均先失败；修正后Core50 passed、Popup40 passed。Completing阶段的属性重开及显式取消也真实进入排队稳定关闭后验证通过。现有Popup两个用例扩展覆盖绑定false/true和Pending/Playing换模板，现有Core最长子项用例合并AddOwner Padding/恢复保障，不增加长期用例。

Final: Ruling: 关闭期间旧Popup模板已销毁时采用新表面预隐藏后的稳定关闭，不重播完整离场 — 旧呈现不可继续，当前关闭意图必须完成；代价是该罕见换模板路径不保留剩余离场时长。

Final: Ruling: 平台display present、最低硬件与原生macOS鼠标体验仍不背书；仅报告实际Native批次/属性与Skia程序输入证据 — 未冻结环境或缺失工具能力不能被另一种数据替代；代价是本次不提供60/120FPS正式保证。

Final: Ruling: Popup换目标首帧准备已覆盖Opened之后并采用可重入释放的作用域，但原生肉眼闪烁时序尚无独立截图证据；Completing完整Flyout链只由既有路径和直接生命周期场景保障 — 代价是这些平台级交互仍需专项走查。

Final: Ruling: ContentViewport四方向、固定父约束、输入/滚动已有定向证明，不扩大到所有DPI/Margin/MinMax/自身固定尺寸笛卡尔组合；原生时钟异常停滞未复现，不加入成功计时watchdog — 代价是未验证组合与框架停滞仍是支持范围边界，取消/detach仍负责释放。

Final: minor (deferred): none identified by reviewer. No second reviewer pass; important fixes certified by targeted RED→GREEN and final cleaned-tree verification. No commit authorized.

Final: 第三方 osx-arm64 NativeAOT 发布和实际执行通过，输出 THIRD_PARTY_AXAML_PLAYBACK_LAYOUT_AOT_OK；AddOwner Thickness 通道暴露的 Core IL2026 已修正为强类型 ValidateValue 调用，再发布无该警告。JIT/AOT fixture 使用实际 Dispatcher.MainLoop 驱动，不把同步 RunJobs 轮询超时误判为 AOT 差异。

Final: 四个测试工程的 Temporary/MotionRefactor 目录、第三方 fixture、专用测试结果与 Skia 截图已删除；截图审计 clean。既有用例扩展覆盖审查问题，改动文件的永久 Fact/Theory 方法数 353→349（8个关闭场景合并为4个双数据理论用例），无净增用例。

Final: Ruling: 旧 AtomUI.Performance 保留当前目标的统计/诊断迁移，不扩展修复其基线已存在的入口命名空间和旧 Tab ScrollViewer 编译缺陷；原生 GalleryPerformance 独立验证 — 避免把无关基础迁移混入 Motion；代价是旧 Headless 诊断工程本次不能作为运行通过证据。

Final: 清理后的 Core MotionExecutionTests + StateMotionContractTests 5 passed；Desktop 三组精确过滤分别255、171、280 passed；Gallery 四类8 passed；DataGridFilterDialogPopupTests 4 passed。生产模块与 Gallery 均为本 worktree 构建，未运行全量回归。

Final: 最终 Gallery Release/osx-arm64 NativeAOT 发布 exit0，NativeAOT output validation passed，file 确认 Mach-O arm64；当前源码无 Motion/Core AOT 警告。ReactiveUI IL2104/IL3053 和本机 Homebrew dylib 部署版本警告保留。第三方已实际执行，Gallery 此处证明发布校验，不冒充平台肉眼运行验收。

Final workflow: 按用户要求保留 feature/motion 与当前 worktree 交 review，不提交、合并或推送。原 checkout release/6.0 未改。保留本计划的只读审查包供 review，不在尚未提交时销毁唯一过程记录。功能迁移与清理验收完成，原生 UI 自动化、最低设备及跨平台实际呈现门禁仍按正式 verification 页列明，不能勾成通过。

Final: Controls FixedAvaloniaMemberRetentionTests 清理后重建并通过1项，最后的 Controls 测试程序集不再包含临时 Motion fixture。当前最终定向结果合计724项（Core5、Controls1、Desktop706、Gallery8、DataGrid4），不叠加先前重复运行或已删除临时用例。58个改动 Markdown 无失效相对链接，src/tests 旧运行类型引用0、临时 Motion 测试目录0、git diff --check 通过。没有延期 minor；平台验收边界按 Ruling 保留。原 worktree 无改动，HEAD 仍6c647fef1。
