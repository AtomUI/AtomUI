# Linux Avalonia 12.1.5 验收与菜单淡出修复计划

> **For agentic workers:** 使用 superpowers:executing-plans 在当前分支逐项执行；不提交、不创建新 worktree。

**Goal:** 验证升级在 Linux Wayland 的实际表现，复现并修复用户追加指出的菜单底色与菜单项淡出不一致。

**Architecture:** 复用现有测试与真实 Wayland 宿主，按升级提交的源码影响追踪选测。菜单问题先用现有 PopupSurface 和真实菜单模板做像素对照，定位共同的透明度合成边界，再作最小共享修复。

**Tech Stack:** Avalonia 12.1.5、NWayland 0.11.0、net10.0、Skia、GNOME Wayland；linux-x64 NativeAOT 发布运行时 10.0.14。

**Spec:** [升级源码评估](../specs/2026-11-05-avalonia-12.1.5-upgrade-design.md)；本次用户要求 Linux Wayland 验收，并追加报告所有 Menu 可能存在关闭时菜单项背景残留。

## Global Constraints

- 当前 checkout 与 feature/upgrade-12.1.5 保持不变，保留原有五份未跟踪文档。
- 不修改依赖版本、公共 API、菜单尺寸/颜色/交互和动效参数，不关闭动效规避问题。
- 只运行定向测试和本机平台专项，不运行全量回归或默认排除的本地化集成测试。
- 新验证代码位于系统临时目录或测试项目 Temporary/Avalonia1215，完成后删除。
- 源码修复使用 AXAML 优先；发布完成后才可判定 NativeAOT，平台探针不能冒充人工视觉/原生指针验收。
- 本次请求已授权验收与问题处理，在当前会话内执行，最终交用户审查；不创建 commit。

## Review Focus

- Overlay 与 PopupRoot 都应以同一整体透明度合成菜单背景、项目、文字与阴影。
- 保留分数缩放、阴影范围、嵌套定位与开关动效；不得以改变原场景让复现消失。
- 新失败必须对比升级前相同环境，不把既有 Linux 测试问题归因于新依赖。
- NativeAOT 要运行最终产物，保留真实私有成员与 DataGrid 编辑绑定验证。
- 单屏环境不能证明多输出混合缩放、重连、硬件输入和所有渲染驱动已通过。

## Task 1：影响面与现有测试

- [x] 以 031135f11..00197a821 核对改动清单；选测计划无 ownership gap。
- [x] 按实际消费链执行 10 项目/226 类精确 filter；记录全部失败。
- [x] 在系统临时目录的 12.1.4 源码快照复跑相同失败类，25 项均为相同基线失败。

## Task 2：菜单合成根因

**Files:** 临时 `tests/AtomUI.Desktop.Controls.Rendering.Tests/Temporary/Avalonia1215/MenuOpacityTests.cs`；确认后最小修改 `src/AtomUI.Desktop.Controls/Popup/Themes/{OverlayPopupHostTheme,PopupRootTheme}.axaml` 与 `NavMenu/Themes/NavMenuItemTheme.axaml`。

- [x] 通过真实 ContextMenu 模板比较打开、隐藏与 25% 透明度像素，预期与整组线性混合相差不超过 3/255；预期 64、实际 110，2/2 失败。
- [x] 对同一场景仅改变透明度合成方式作 A/B，实际恢复 64，确认根因和正确设置边界。
- [x] 在两种宿主的 ControlTheme 模板父级实施最小修复，复跑像素测试及 Popup/Menu/NavMenu/Motion 精确模块测试。RenderOptions 为 CLR attached API，不能放入 Setter；native 复用 presenter，overlay 增加一个必要的静态合成父节点。
- [x] 删除临时测试及专用产物，最终模块验证并检查公共契约、AOT与资源生命周期。

## Task 3：Linux 原生与发布

- [x] Debug Gallery 构建 0 warning/error；launcher 确认 checkout 内精确可执行路径和 PID，运行后停止。
- [x] Wayland 真实 Dispatcher 主循环验证初始化前 timer 和 16ms timer。
- [x] 原生 Wayland 首帧分数缩放、三轮嵌套阴影 Popup、坐标往返、resize、呈现反馈和关闭清理。
- [x] 帧节奏开关单变量对照；X11/XWayland 场景执行并用 12.1.4 对照，保留 IBus 清理错误限制。
- [x] 源码稳定后 Linux NativeAOT 发布，核对 runtime/compiler 10.0.14、ELF、反射保留与真实启动。
- [x] 独立审查改动并完成文字验收报告，明确所有剩余项，删除本次临时工程、媒体和进程。

## 验收结果

深色选中 Inline NavMenu 同源偏差经红绿验证修复；最终清理后的模块 726/726。升级选测 2298 项中 25 项与 12.1.4 基线失败完全一致。Wayland 主路径与真实 Linux NativeAOT 启动/帧呈现通过，仍有原生视觉、硬件输入、多输出及 X11 IME 等限制。见[完整验收记录](../progress/2026-11-05-linux-avalonia-1215-acceptance.md)。

## 追加：源码、动效时钟与性能评估

使用 writing-plans / executing-plans 继续当前验收，仅研究和临时测量，不修改产品源码或依赖版本，不提交。

- [x] 重新核对上游 339 文件完整清单，逐段阅读 Dispatcher、渲染/合成、Wayland、Skia 与布局/文本性能相关生产 diff；将“未变更”和“没有影响”分开。
- [x] 推导 Dispatcher 毫秒时间轴、GlobalClock 相对时间、Wayland CLOCK_MONOTONIC 纳秒呈现时间的转换关系，核对删除旧 hook 的必要性及剩余风险。
- [x] 在系统临时目录对比 12.1.4 基线、12.1.5 提交态和当前菜单修复态，保持配置、宿主、运行库和观察方法一致；Rendered/Processed 不作为屏幕呈现时间，失效批次观察不作为性能成功证据。
- [x] 记录真实测试范围、噪声、回归或待验证项，清理临时副本和进程，更新源码评估结论；不把功能通过解释为性能无回退。

追加评估记录见[真实源码、时钟与性能复核](../progress/2026-11-05-avalonia-1215-clock-performance-source-audit.md)。记录包含产物混用的纠正和两轮隔离补测；研究/记录任务已完成，全面性能无回退、GPU/AOT 稳态、精准呈现及多输出门禁仍未关闭。
