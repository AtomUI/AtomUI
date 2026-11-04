# 弹层组件动效实现计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [x]`) syntax for tracking.

**Goal:** 完成已定稿的 Ant Design 对应弹层动效接入，保持原始 UX，并以可比较测量约束布局、分配与资源成本。

**Architecture:** 组件主题选择内部规格；Popup 根据实际定位与表面几何提供只读默认 Motion 对，唯一配置 owner 的公开属性负责覆盖。复用现有内置定义、PopupSurface 与 MotionPlayback，不重建阴影或动画引擎。

**Tech Stack:** C#/.NET 10，Avalonia 12.1.4，AtomUI AXAML，xUnit v3/Headless，现有 Gallery/性能工具。

**Spec:** [正式弹层动效架构](../../architecture/systems/motion/popup-motion.md)、[组件规格](../../architecture/systems/motion/built-in-motion.md#弹层组件选择)。

## Global Constraints

- 用户已要求“开始全面重构，在实现的时候要特别注意动画的性能”；在当前 release/6.0 与当前目录执行，不创建分支/worktree，不提交。
- 保留先前正式文档改动。只覆盖已定稿矩阵；Mentions/Tour、Modal/Drawer、Inline 重构不扩展进本任务。
- 动画不改变内容尺寸、限高、滚动链、焦点、键盘与 Semantic PART。主题可表达的关系留在 AXAML。
- 新测试先放 tests/AtomUI.Desktop.Controls.Tests/Temporary/PopupMotion/，完成 RED/GREEN 后清理；最终 focused 检查显式指定模块，禁止自主全量回归。
- 详细过程/证据仅记录 [本轮进度](../progress/2026-11-02-popup-motion-implementation.md)；截图、采样和日志放系统临时目录并在验收后清理。
- 此任务是默认行为接入与性能约束，不以“旧无动画”作为动画帧时的性能对照；运行开销对照使用同轨迹、同内容、同宿主的显式动画基线。

## Review Focus

- 实际翻转/RTL 与 shadow buffer：选择器、独立菜单、Dropdown 子菜单和 Tooltip 原点分别验证。
- 显式 null、自定义 Motion、绑定更新与 ClearValue：默认值不能吞掉用户配置或回写配置来源。
- 布局通知和透明准备可能同步重入：旧请求不能播放、关闭或清理新 Surface。
- FlyoutHost/独立 Flyout 切换、正常关闭与失败回滚：资源绑定不能保留旧 owner。
- 快速反向、禁用中收敛及多弹层：不逐帧布局/创建定义，不修改共享配置，不泄漏运行资源。

## Task 1: 原始入口与性能基线

**Files:** Existing tests/AtomUI.Desktop.Controls.Tests/Popup/PopupLifecycleTests.cs、Dialog/DialogPopupControlFamilyTests.cs；Create Temporary/PopupMotion/PopupDefaultMotionTests.cs。
**Interfaces:** 消费真实主题、ManualMotionClock 与现有控件 API；产出可复用参数化原始场景及 baseline 数字。

- [x] 先创建真实主题驱动：选择器/四类 Picker/菜单/Flyout/ContextMenu 的默认 Motion 类型、开关与时长；至少证明 Select、Flyout、ContextMenu 因空值失败。
- [x] exact filter 执行 RED，区分目标失败与编译/测试驱动错误；记录原始 11 条入口和清理路径。
- [x] 同一临时驱动测量显式 Slide/ZoomBig 的定义分配、稳定播放时布局次数、关闭与反向；保留方法、参数与样本口径。

## Task 2: 共享规格、几何和主题默认

**Files:** src/AtomUI.Desktop.Controls/Popup/PopupMotion.cs（内部 enum/值模型）、PopupUtils.cs、Popup.cs、Themes/PopupTheme.axaml；临时测试目录。
**Interfaces:** `PopupMotionPreset`、`PopupMotionGeometry`、`PopupMotionPair`；`PopupUtils.CreateDefaultMotions(PopupMotionPreset, in PopupMotionGeometry)`；Popup 内部 `DefaultMotionPreset` 和只读 `DefaultMotions`。

- [x] 测试标准方向、菜单来源/RTL、alignment 公式及内容框→表面 DIP，先验证 RED。
- [x] 在 PopupUtils 实现纯计算，复用既有 Slide/ZoomBig；按有效几何复用默认定义，不保存控件或全局 target 缓存。
- [x] PopupTheme 以默认优先级绑定 DefaultMotions；通用 Popup 的 None 仍为空，用户本地/样式/绑定与显式 null 保持优先级。
- [x] Popup 从真实 Surface/Child/placement 生成几何；布局后重新读取有效定义，再按现有请求代际播放，断言 GREEN。

## Task 3: 模板型选择器、Picker 和菜单

**Files:** ComboBox/Select/Cascader/TreeSelect/AutoComplete、Primitives/InfoPickerInput、Menu、NavMenu 的相关 Themes；必要的内部菜单上下文；对应临时参数化测试。
**Interfaces:** 消费 Task 2 的内部规格属性；使用现有 Mode、IsTopLevel、MenuSemanticLevel，不新增公共 API。

- [x] 用真实 PART_Popup 验证完整模板矩阵与覆盖规则，新增差异先 RED。
- [x] 主题接入 SelectSlide、HorizontalMenuSlide、MenuZoomBig、DropdownSubmenuZoomBig；删除可由主题取代的默认代码赋值。
- [x] 检查头部菜单与嵌套菜单、InlineCollapsed、RTL、上下翻转；保留 Inline 与候选/滚动原始合同，相关断言 GREEN。

## Task 4: Flyout/Host/ContextMenu、Tooltip 和可选包

**Files:** Flyouts/Flyout.cs、FlyoutHost.cs、MenuFlyout.cs、Menu/ContextMenu.cs 及主题；Tooltip、ColorPicker、DataGrid 筛选 Flyout 相关源码与主题。
**Interfaces:** 默认 Motion 对只读视图单向从 Popup 到唯一配置 owner；公开有效 Motion 单向从 owner 到 Popup。独立 Flyout 使用 Style 默认，托管 Host 使用自身主题默认。

- [x] 独立/托管、更换 owner、显式空值、自定义和普通关闭的失败测试先执行。
- [x] 创建运行时非视觉接入，注释 AXAML 不可行原因并定义释放 owner；取消竞争默认与直接覆盖用户值的 ConfigureMotion。
- [x] Tooltip Fast、Popover/ColorPicker Mid、ContextMenu/菜单/树筛选 Dropdown 规格全部接入；正常关闭和失败回滚释放 scoped 资源，验证 GREEN。

## Task 5: 中断、输入、释放和性能收敛

**Files:** Popup.cs、PopupMotion.cs 与必要 scoped 接入；复用既有 Motion/Popup/Flyout 生命周期测试和临时验证。
**Interfaces:** 复用 _motionVersion、PopupCloseState 与 Motion 当前请求；只在内部默认反向时沿用活动原点。

- [x] 快速开关、关闭中重开、禁用、零时长、模板/目标失效及通知重入先 RED；自定义原点不被强制改写。
- [x] Overlay 离场输入覆盖可撤销；native 输入与阴影协议复用；几何真实需要时才处理 native 动画包络，保持内容屏幕坐标。
- [x] 同轨迹基线与实现合批测量：动作级分配、布局/帧通知、首帧和关闭资源；三个无收益性能试验停止该纯优化分支，不缩短时长或删除效果达标。
- [x] 一次编译后集中运行所有临时契约，确认无逐帧默认重建或布局链回归。

## Task 6: 最终验收与清理

**Files:** 必要 Gallery 临时场景与现有性能工具；正式 verification/changelog；本轮进度记录。
**Interfaces:** 消费稳定源码和已验证产物，媒体/原始日志不进入 docs/superpowers。

- [x] 复用本地 Gallery 启动技能和脚本视觉验收，原始限高/内容/操作保持不变；可访问的平台分别取证，未验证范围如实记录。
- [x] 源码稳定后集中执行需要的 Release/NativeAOT 发布、功能与性能采样，不按小实验重复发布。
- [x] 按 executing-plans 做一次独立整体代码审阅；重要发现取得 RED/GREEN 后修复。
- [x] 删除临时测试、专用探针和产物，清理后显式模块 iterate/exact filter 验证，git diff --check；更新实现差距与实际验证状态。
- [x] 报告实际功能结果、性能数字/比较口径、测试处置及残余平台限制；等待用户审查，不提交。
