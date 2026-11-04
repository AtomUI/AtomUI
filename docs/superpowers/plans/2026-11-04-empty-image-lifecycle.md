# Empty 图片生命周期修复 Implementation Plan

> **For agentic workers:** Use superpowers:executing-plans in the current checkout. Follow the checkbox steps; do not create a branch/worktree or commit.

**Goal:** 修复同一 Empty 实例从 ContentControl 摘出再挂回后图案消失，并满足已确认的图片状态维护契约。

**Architecture:** `AbstractEmpty` 单一维护来源校验、图片生成及结果提交。模板接入、重挂和挂树属性变更共用更新规则；资源暂缺保留结果，相同结果不重置 SVG。

**Tech Stack:** .NET 10、Avalonia 12.1.4、现有 Headless/Skia、xUnit。

**Spec:** [Empty 实现原理](../../controls/desktop/data-display/empty/implementation.md)，尤其第 6、8、11 节。

## Global Constraints

- 在当前分支、当前目录执行；不创建分支/worktree，不提交，不改其他任务的文件。
- 不新增公开 API、Token、模板节点、绑定、订阅、缓存、刷新标记、延时或重试。
- 临时验证放所属测试项目 `Temporary/Issue529`，媒体放系统临时目录，验收后清理。
- 只运行精确测试与显式路径的模块 iterate，不运行 change/full 或额外 NativeAOT 发布。
- 详细证据只记录在 [进度与验收](../progress/2026-11-04-empty-image-lifecycle.md)。

## Review Focus

- 离树期间修改来源或主题：重挂读取最新输入，不能沿用旧图。
- 颜色资源暂缺：保留已有结果，不破坏随后恢复资源的路径。
- 三种来源全部为空：主动清空生效，不能残留旧图。
- 同一结果重复同步：不出现 `Source=null` 的中间态；重套模板后只更新新 part。
- 同一模板实例身份与布局：原始摘挂后图片、描述、Footer、尺寸和 Semantic target 保持契约。

## Task 1: 失败复现与共享基类修复

**Files:**
- Modify: `src/AtomUI.Controls/Empty/AbstractEmpty.cs`
- Temporary test: `tests/AtomUI.Desktop.Controls.DataGrid.Tests/Temporary/Issue529/EmptyImageLifecycleTests.cs`
- Temporary instrumentation: DataGrid 测试宿主启用真实 Skia，完成后精确还原。

**Interfaces:** 保持 `CheckImageSource()`、`SetupImage()` 私有；新增继承生命周期 `OnAttachedToVisualTree(VisualTreeAttachmentEventArgs)` 的 override，不新增使用方契约。

- [x] 用单一参数化驱动保留空源、窗口尺寸、同一实例 `Content=null -> 原实例`。7 场景断言图片及描述/布局、part 身份；补充资源暂缺、自定义来源、离树变更、明确清空与重复提交。
- [x] 精确运行临时测试，确认原始失败来自 `Source=null` 与图形尺寸归零；保存并查看前证截图。
- [x] 在共享基类集中来源校验；重挂同步；预设先检查资源并生成字符串再提交；使用现有 Svg 属性保存最终结果，不预清空 Source。
- [x] 重跑相同驱动，确认原失败转绿；检查 Skia 前后图像与布局，运行成员布局审计并处理触及文件内的结构发现。

临时命令：`dotnet test tests/AtomUI.Desktop.Controls.DataGrid.Tests/AtomUI.Desktop.Controls.DataGrid.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~Temporary.Issue529 --logger 'console;verbosity=detailed'`。

## Task 2: 清理、模块验证与审查

- [x] 确认临时测试全部删除，不晋升为常驻测试；还原宿主、删除媒体与专用临时产物。
- [x] 同步 Empty changelog，记录实际实现；检查正式文档与最终代码一致。
- [x] 运行 `python3 scripts/verification/test.py run --scope iterate --path src/AtomUI.Controls/Empty --path src/AtomUI.Desktop.Controls/Empty`，确认非零发现数及计划内全部通过。
- [x] 对 DataGrid 空态保留精确消费检查：`dotnet test tests/AtomUI.Desktop.Controls.DataGrid.Tests/AtomUI.Desktop.Controls.DataGrid.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~DataGridEmptyTitleFocusTests`。
- [x] 按执行技能要求进行一次只读独立审查，范围限本修复；完成必要修正后按影响范围验证。
- [x] `git diff --check`，记录红绿、清理后模块结果、AOT 判断、实际平台边界，交用户审查，不提交。
