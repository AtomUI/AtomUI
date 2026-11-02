# DataGrid Selection Trigger Implementation Plan

> **For agentic workers:** Use `superpowers:executing-plans` and TDD for the task below.

**Goal:** 对齐选择框驱动的表格左键选择行为，避免误点行或选择列空白导致已有多选丢失。

**Architecture:** 继续由 `DataGridSelectionState` 持有选择集合；在当前单元格的鼠标输入决策处按 `SelectTriggerType` 决定是否生成选择动作。焦点、current 和编辑流程继续执行。

**Tech Stack:** C# / Avalonia 12 / .NET 10 / xUnit Headless，必要的 Skia 渲染验收。

**Spec:** 本文“契约与范围”；依据本聊天先前评估及用户“结合现在 AtomUI 最新的源码……跟 ant design 对齐”的执行授权。

## 契约与范围

- `SelectIndicator`（现有默认值）：左键点击选择框切换选中状态；点击普通单元格或选择列空白不改变选择集合，包括 Ctrl/Shift 点击。
- `Cell`：保留普通点击选中当前行、Ctrl 扩展/取消、Shift 范围选择和指示器增删。
- `SelectionMode=None` 不产生选择；`Single` 的 radio 切换仍只保留一行。
- Gallery 选择示例使用 `SelectIndicator`，说明 AtomUI 的实际属性与点击规则。
- 只涉及左键选择入口、Gallery 示例和控件文档；不新增 API，不改右键、键盘、滚动、布局、主题、Token、数据源模型或整行累加多选。

## Global Constraints

- 在当前分支和目录修改；不创建 branch/worktree，提交仅按用户明确授权执行。
- 新验证默认临时，存入所属测试工程 `Temporary/Issue418/`；清理后只运行修改模块的 focused 验证。
- 只在本文保留过程证据；截图、构建日志和临时辅助文件放系统临时目录，完成后删除。
- 纯输入分支调整，无新绑定、反射、资源或订阅；不触发 NativeAOT 发布义务。

## Review Focus

- 非指示器点击仍到达单元格事件，允许 current/focus 更新，不能用吞掉输入解决。
- 选中行与未选中行、选择列空白的命中区域都保持原有几何。
- 指示器增删、header 全选和单选切换保持选择状态及视觉投影一致。
- `Cell` 的 Ctrl/Shift 行选择保持可用。
- `None` 模式不能因点击选择框绕过禁选。

### Task 1: 修正选择入口并同步示例和文档

**Files:** `DataGrid.CurrentCell.cs`；Gallery `DataGridShowCase.axaml` 及四种语言的 `SelectionDescription`；DataGrid `overview.md`、`implementation.md`、`changelog.md`；所属测试工程临时用例。

**Interfaces:** 消费既有 `SelectTriggerType`、`SelectionMode`、`Selection`；不增加接口。

- [x] 用真实 Headless 鼠标事件复现：勾选两行，点击选中/未选中行和选择列空白，断言仍保留两行及 checkbox 状态，并观测命中与事件。
- [x] 运行精确 filter，确认因选择集合改变失败；同时覆盖指示器、single/none 和 Cell 修饰键行为。
- [x] 合并重复的指示器判断，把普通行选择分支限制到 `Cell`；同步 Gallery 和现有控件文档。
- [x] 同一临时用例转绿；在真实 Gallery 示例上执行框架输入和 Skia 截图复核。
- [x] 删除临时测试和取证产物，执行 DataGrid/Gallery 所修改模块的 focused 验证、成员审计和 `git diff --check`。
- [x] 独立审查当前未提交 diff，交用户审查。

## 执行与证据

Pre-flight: no shared interfaces；单项工作，无跨任务依赖。项目用户规则覆盖通用 skill 的新 worktree、提交、隐藏工作目录和全量测试默认行为。

### 复现与修复证据

- 参考来源：本地 `/Users/chinboy/Projects/ReferenceProjects/ant-design`，`211c6ed29b0d2abd18485e293681f8a4e415240b`，`components/table/demo/row-selection.md`、`hooks/useSelection.tsx`。默认 checkbox 点击改变选择，行点击需要额外实现。
- 初始工作树干净；生产代码修改前，真实 Gallery `DataGridShowCase` 在 1280×884 Headless Window 中勾选 John Brown、Jim Green 后，点击 Joe Black 选择列空白，选择从 `[1, 2]` 变为 `[3]`，目标断言失败。输入坐标分别为 `(72,743)`、`(72,796)`、`(94,849)`；实际命中前两次为 checkbox 内部 `PixelAlignedBorder`，第三次为单元格 `Border`，选择单元格 Bounds 为 48×52。
- 原场景命令：`dotnet test tests/AtomUIGallery.Tests/AtomUIGallery.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~DataGridSelectionShowCaseTests`。修复前 1 项失败；修复后同一用例 1/1 通过（5 秒），使用 Skia + HeadlessDrawing=false，捕获稳定真实帧后人工查看。
- 视觉观察：原页面保留列、三行数据、扩展/单选开关、外层滚动和实际大小；两行蓝色选中背景与 checkbox 均在误点空白及普通单元格后保留，第三行仍未选中。没有改变 checkbox、单元格命中几何或关闭输入。验证后端为 Headless/Skia，不声明原生 macOS/Windows 后端已验证。
- 精确交互命令：`dotnet test tests/AtomUI.Desktop.Controls.DataGrid.Tests/AtomUI.Desktop.Controls.DataGrid.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~DataGridSelectionTriggerTests|FullyQualifiedName~DataGridSingleSelectionIssue461Tests|FullyQualifiedName~DataGridSelectionColumnInitialStateTests'`。12 项临时验证及 6 项既有用例共 18/18 通过（3 秒）。涵盖选中/未选中行、选择列 padding、Ctrl/Shift、current/column 与事件到达、指示器增删、header 全选/清空、Single/None 和 Cell 语义。
- 复现驱动首次编译校准了 `RawInputModifiers`，宿主补齐真实布局和 render tick 后才采信失败；header 验证从空选择开始执行全选，未改变产品已有半选点击规则。这些驱动校准不算产品失败证据。
- 测试处置：两份临时测试均删除，临时 Skia 宿主改动恢复；未新增永久测试、helper 或项目。现有选择契约测试继续保留。
- 成员审计：`check-member-layout.py --path src/AtomUI.Desktop.Controls.DataGrid/DataGrid.CurrentCell.cs`，1 文件、0 个独立 control 声明（既有 partial 实现文件），0 问题；未重排注册、字段或初始化次序。
- 独立只读审查：无可操作问题；按本任务明确范围未扩展到右键、键盘、滚动和 NativeAOT 发布。

### 最终工作树验收

- 命令：`python3 scripts/verification/test.py run --scope iterate --path src/AtomUI.Desktop.Controls.DataGrid/DataGrid.CurrentCell.cs --path controlgallery/AtomUIGallery/ShowCases/DataDisplay/DataGrid`。
- 结果：`iteration-passed`，124.153 秒；runner 按这两个显式模块路径选择 50 个测试类，DataGrid 316 项、Desktop 的关联契约 28 项、GalleryBase 生成器关联契约 10 项、Gallery 9 项，共 363 项通过；没有运行 change scope、affected-wide 或全量回归。
- 三份控件文档的相对链接无缺失，四份 XLIFF XML 有效且 `SelectionDescription` 的 source 一致；`git diff --check` 通过。
- 截图工具 audit 无发现；cleanup 返回 `removed=true`、`audit_error=null`。临时用例、专用目录和 Skia 宿主改动已清理，无新增永久测试或后台 GUI 进程。临时命令日志在证据摘要落盘后删除。
- AOT/资源评估：只有既有枚举和选择动作的同步分支发生变化，无新 API、绑定、订阅、反射、动态发现或资源 owner；本次不要求 NativeAOT 发布。

状态：实现与验收完成；用户查看临时 Demo 后已明确要求提交本次改动。

### 用户手动验收 Demo

- 用户要求启动临时 Demo 供手动验收；位置 `/private/tmp/atomui-issue418-demo-sD1jJE`，项目外保留至本轮人工验收结束。
- 引用当前工作区 DataGrid 和 Localization 源码，Avalonia 12.1.4 / net10.0；两侧分别为默认 `SelectIndicator` 与显式 `Cell`，各含原示例三行数据、单选/多选切换、预选前两行、清空及实际选中状态。
- `dotnet build /private/tmp/atomui-issue418-demo-sD1jJE/Issue418Demo.csproj --configuration Debug --nologo --verbosity minimal`：成功，0 warning / 0 error；Demo 中 DataGrid DLL 的 SHA-256 与当前仓库构建产物相同。
- 原生 Demo 曾启动，PID 64973，经实际可执行文件路径核对；启动记录 `DEMO_READY rows=3+3`。用户要求提交时，确认该进程已自行退出并删除项目外 Demo 目录；临时验收工程和相关输出全部清理。
