# 弹层基类 Motion 配置统一实施计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans inline; one fresh whole-change review after implementation. No per-task implementer delegation.

**Goal:** 将正式文档中的族基类 OpenMotion/CloseMotion 和统一禁用策略落地，删除预设/镜像开关，证明可回收性与同条件成本后暂存供审查。

**Architecture:** 复用 Popup 两项 StyledProperty 注册。组件主题提供低优先级默认，实际 Popup 单向消费；默认工厂从只读几何产生普通内置定义，不保存控件。Core 活动请求处理动态全局禁用并正确完成，运行观察随请求释放。

**Tech Stack:** .NET 10 / Avalonia 12.1.4 / C# / compiled AXAML / xUnit Headless / existing native Gallery probe.

**Spec:** [正式弹层合同](../../architecture/systems/motion/popup-motion.md)、[播放协议](../../architecture/systems/motion/playback-and-lifecycle.md)。

## Global Constraints

- 当前 feature/motion，HEAD 4513cfd28；复用当前目录，不创建分支或 worktree；只暂存，不 commit/push。
- 保留前轮已暂存源码/57份文档和用户 PopupMotion.cs 的 using 删除；不得清空暂存区或撤销无关内容。
- 只新增四个既有族基类及 ComboBox/DropdownButton/SplitButton/ToolTip/MenuItem/内部 NavMenuItem 的同名入口，不在普通 Button、Menu/NavMenu/DataGrid 本体或 IMotionAwareControl 普遍添加。
- null 禁用，ClearValue 回默认，标准属性优先级；用户 Motion 不改写、不保存 target/owner/运行任务。
- 原生包络、物理关闭、取消许可、反向、hover、限高、滚动与 Semantic PART 保持；不新建视觉树/动画引擎/预设属性或专用动画类型。
- 临时测试在所属 Temporary/MotionUnification，验证后删除；媒体/日志/采样在系统 tmp，最后清理。只跑精确模块，不自主全量。
- 详细证据仅在 [执行记录](../progress/2026-11-03-popup-motion-unification.md)。

## Review Focus

- 元数据 null 与显式 null 的来源不同；默认与转发不能把其中一方错误吞掉。
- 控件保留、换模板时旧 Popup/Surface 回收；共享 Motion/已完成 Task 长期保留不反向保活页面。
- 动态全局禁用在 Delay/首帧/离场中正常完成；外部取消/替代/卸载不能伪装完成或关闭新宿主。
- 默认几何从真实宿主读取，反向保留活动原点；新配置/局部资源/RTL 和模板重建不读取旧快照。
- Flyout 更换、关闭通知重入先退役租约，物理关闭后释放定位，不能持有旧 owner 或回写用户配置。

## Task 1: 基线与 RED

- [x] 保存当前输入身份和运行成本，使用现有原生 probe 3进程、3warmup/10 measured、450ms，构建/采样不重叠。
- [x] 用真实控件反射/模板测试确认族入口缺失和旧配置属性存在；Core 真实资源作用域下动态禁用失败。
- [x] 写参数化临时测试和实际 AXAML/弱引用驱动，不保存永久验证框架。

## Task 2: Core 禁用收敛

**Files:** Core/Animations/Motion/{Motion.cs,AbstractMotion.cs,Playback/MotionPlayback.cs}。
**Interfaces:** 内部有效开关查询与当前请求完成操作；不增加公开属性/结果枚举或第二个状态机。

- [x] 复用现有 Phase/请求身份，动态禁用停帧、跳过 Delay、提交终点并 Completed；全局资源观察只在活动播放期间存在。
- [x] 同步初值/回调重入安全，完成/失败/取消/卸载统一释放，局部资源和重新启用正确。
- [x] 精确测试 RED→GREEN；验证定义/任务存活时目标可回收。

## Task 3: 默认装配与族 API

**Files:** Popup.cs/PopupMotion.cs/PopupUtils.cs、现有基类、MenuItem/NavMenuItem、对应 Themes。
**Interfaces:** 保留 PopupMotionPair 值模型、只读几何快照；删除 PopupMotionPreset/DefaultMotionPreset/IsMotionAllowed。纯工厂显式返回同种内置定义。族基类 AddOwner 公开配置，内部默认引用只作派生视图。

- [x] 几何变化与默认装配用编译 AXAML，单向 source view 不占用目标默认值；不能用 TemplateBinding 的不支持模式。
- [x] 准备阶段核对版本后使用最新有效定义；缓存按几何，反向只克隆内部默认，不修改用户对象。
- [x] null/绑定/ClearValue、家族默认/方向/RTL、原生包络合同通过。

## Task 4: 非视觉与组合接入

**Files:** Flyout/Host/MenuFlyout、DropdownButton/SplitButton、Tooltip、DataGrid filter、ColorPicker。
**Interfaces:** 默认选择为静态纯函数，类型差异通过内部虚方法返回工厂；不注册预设属性。运行时可替换对象才代码绑定，生命周期一致关系保留 [!]。

- [x] 外层配置→Flyout→Popup 单向，独立/托管切换和原有资源上下文正确。
- [x] 先关闭物理宿主再释放旧定位/配置；回调替换读取最新目标，避免重复默认和旧 owner。
- [x] 生命周期/绑定/原崩溃/hover 与原生几何验证；零时长和全局禁用不遗留宿主。

## Task 5: 内存、性能、Gallery 与发布

- [x] 用编译 AXAML 加载 DatePicker/Fade 示例；定义/任务长期存活、普通关闭、Delay/入场/离场卸载、换模板/换 Motion 均验证弱引用。
- [x] 真实 Gallery 导航对象与运行句柄计数；保留内容和布局条件，不以清空 DataContext/删除用户配置清理。
- [x] 同参数原生3进程对比与受控400tick实际内容布局/定义重建，报告动作成本及噪声，不宣称 display FPS。
- [x] 同步 Gallery 最小示例/四语资源（当前规范无 API/Token 表）；稳定源码集中 Release/NativeAOT，记录告警/范围。

## Task 6: 审阅、清理与暂存

- [x] 一次 fresh-context 整体审阅；Important/Critical 一次修复轮，逐项有效失败/通过，不重复审阅循环。
- [x] 删除临时测试/源码/媒体/原始数据，核对实际清理后模块；更新正式验证页，不把旧结果当新合同证据。
- [x] Source/AXAML/layout/link/diff 检查，保留用户修改，只暂存本轮授权范围，不提交。
