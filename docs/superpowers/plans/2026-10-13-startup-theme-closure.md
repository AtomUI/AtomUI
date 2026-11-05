# 首阶段主题依赖补齐实施计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans. Steps use checkbox syntax for tracking.

**Goal:** 修复分阶段启动中 Linux/Windows 标题栏、AddOn 及同类首阶段控件的主题遗漏。
**Architecture:** 保留现有 TypeMap 分区与 opt-in 启动机制，补齐 Desktop 关键集的递归模板依赖。复用真实包注册测试保护首阶段资源，并以临时 Headless 测试验证首阶段可见树及第二阶段演化。
**Tech Stack:** C#、Avalonia 12、.NET 10、xUnit、AXAML。
**Spec:** 本次用户要求及 `docs/architecture/foundations/startup-and-registration.md` 的控件粒度分区契约。

## Global Constraints

- 不变更公共 API、主题结构、按钮行为或资源生命周期；不创建提交。
- 复用当前干净 checkout；仅运行明确模块路径或精确 filter，不运行全量。
- 临时测试在 `tests/AtomUI.Desktop.Controls.Tests/Temporary/StartupThemeClosure/`，验证后删除并重新构建测试产物。
- AOT 保留既有生成式分区路径；补充关键名称不新增运行时反射。

## Review Focus

- Linux/Windows/macOS 分支及全屏/置顶按钮均有首帧主题。
- 普通和切换 AddOn 的共享 frame 在首阶段即可绘制。
- CheckBox/RadioButton、动态选项容器、菜单浮层及滚动条辅助主题递归完整。
- 不误把整个 Desktop 包提前加载；第二阶段仍保留业务控件。
- 同步注册与分阶段完成后的既有控件模板保持一致。

## Task 1: 审计与失败证据

- [x] 从所有首阶段主题递归扫描跨 namespace 的元素、类型资源和 Token 依赖，并核查动态创建路径。
- [x] 临时测试首阶段所需主题完整性以及各平台按钮/AddOn 的实际图标、背景部件。
- [x] 精确运行临时测试，确认因遗漏主题失败。

## Task 2: 补齐关键集并复用契约测试

- [x] 修改 `src/AtomUI.Desktop.Controls/AtomUI.Desktop.Controls.csproj`，加入已确认依赖。
- [x] 扩展 `tests/AtomUI.Desktop.Controls.Tests/Theme/TypedPackageEntryTests.cs` 现有资源断言，防止遗漏重现，并验证业务资产仍延后。
- [x] 更新启动文档对跨平台及内部主题依赖的维护要求。
- [x] 重跑同一临时测试，验证首阶段及第二阶段前后行为。

## Task 3: 清理、模块验证与审查

- [x] 清理临时测试；精确运行 TypedPackageEntryTests 与 Window/菜单/选项/滚动条相关现有验证。
- [x] 使用本 checkout 的 Gallery 启动器验证 Linux 原始窗口，记录可用的视觉证据及平台限制。
- [x] 独立代码审查，处理重要发现，运行 `git diff --check`，向用户交付未提交差异。

## 执行证据

- 审计确认 9 个缺失主题；递归 AXAML（含 Common namespace）与匹配控件代码创建路径复核未发现额外遗漏。
- 修复前临时 22 例均失败；修复后同一 22 例与 TypedPackageEntryTests 6 例共 28 例通过。
- 临时测试已删除；永久测试只扩展已有用例，净新增 0。
- 使用当前干净 checkout，遵循工作区复用要求；无提交、无整包提前注册。

- 最终清理后精确模块验证 368/368 通过，git diff --check 通过。
- Release Gallery 构建 0 warnings / 0 errors；核对 executable、PID 1652106、X11 window PID 后截图确认五个标题栏按钮恢复，测试进程已停止。
- 独立 reviewer 静态检查 36 个主题/include 及动态容器，未发现额外遗漏。
- NativeAOT 首次并行构建 MSB4166；改为单节点禁用 build servers 重试，重试成功；存在 ReactiveUI / ReactiveUI.Avalonia IL2104、IL3053 依赖警告。

- NativeAOT ELF 与 runtime 缺席标记验证通过；X11/Wayland 各 5 秒启动存活，X11 截图确认按钮；临时发布产物与本次进程已清理。
- 未执行原生 Windows/macOS 验证，已验证对应 Headless 模板分支。
