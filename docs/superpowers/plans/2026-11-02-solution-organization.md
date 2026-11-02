# Solution Organization Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 落实 Issue #527，减少解决方案树的平铺项目，并集中 Gallery 入口。

**Architecture:** 只通过 SLNX 逻辑文件夹归类，保留现有项目集合、路径及引用。分类文件夹使用无数字前缀的名称，展示顺序遵循 IDE 默认名称排序。

**Tech Stack:** .NET 10、SLNX、Markdown。

**Spec:** 本文“已批准方案”为本次实施与验收的唯一任务记录；来源为本聊天方案 review，用户于 2026-11-02 回复“确认。按照这个方案落实”，随后回复“开始做”。

## 已批准方案

| 逻辑文件夹 | 项目数及归属 |
| --- | --- |
| `Gallery` | 3；controlgallery 下的共享示例、Desktop、Browser |
| `Libraries` | 10；Native、Core、Controls.Shared、Controls、四个 Desktop 控件项目、GalleryBase 及其生成器 |
| `Localization` | 7；Localization、LanguagePack.Template，以及 `pt-BR` 子文件夹内的五个语言包 |
| `Fonts` | 2；两个字体项目 |
| `Icons` | 3；Icons.Shared、AntDesign 及其生成器 |
| `Tooling` | 2；Generator、Toolchain |
| `Tests` | 7；当前解决方案收录的测试项目 |

## Global Constraints

- 在当前分支和目录工作，不创建分支、worktree 或提交。
- 项目集合保持 34 项，不调整物理目录、csproj、依赖或命名空间。
- 不补录既有的八个未收录测试项目，不新增过滤解决方案。
- 分类文件夹不带数字前缀（用户实施中的明确修正）；中英文 README 同步说明从 `Gallery` 设置 Desktop 启动项目。
- 复用已有构建基础设施验证，不新增永久测试，不运行全量回归或 AOT 发布。

## Review Focus

- 逻辑移动后 Project Path 仍相对于解决方案根目录，34 个路径无遗漏、重复或改写。
- `pt-BR` 为 Localization 的逻辑子文件夹；SLNX 由 .NET CLI 真实解析。
- 两个 GalleryBase 项目归 Libraries，七个测试项目均归 Tests。
- README 提供可定位的 IDE 入口，现有 CLI 启动命令保持有效。
- 在可用 IDE 中复核展示；无法运行的 IDE 明确记录为未验证，不把分组当作加载提速证据。

## Task 1: 解决方案归类及启动指引

**Files:** 修改 `AtomUI.slnx`、`README.md`、`README.zh-CN.md`。

**Interfaces:** 消费现有 34 个项目路径；产出相同项目集合的分组 SLNX 和双语 IDE 启动指引。

- [x] 核对原始状态：34 个平铺项目、0 个逻辑文件夹，Gallery 已在 XML 头部。
- [x] 添加七个顶层分组及 `pt-BR` 子分组，更新两份 README。
- [x] 对照修改前项目集合，验证分组归属、路径存在性、CLI 解析与 README 指引。
- [x] 执行 `dotnet test tests/AtomUI.Generator.Tests/AtomUI.Generator.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~AtomUI.Generator.Tests.BuildInfrastructure --nologo --verbosity minimal` 和 `git diff --check`。
- [x] 审查最终 diff，并记录验证结果及可用 IDE 的展示证据。

## 执行记录

- Pre-flight: 单个任务，无跨任务接口。
- 执行方式：按用户已批准的方案在本聊天直接实施；不重复请求方案或执行方式确认。
- 变更类型：解决方案配置和文档调整，不涉及运行时、控件 API、资源生命周期或 AOT 代码。
- 验证范围调整：`iterate --path AtomUI.slnx` 的预览映射到五个测试项目、24 个类，包含本地化集成测试；已在构建阶段停止该轮，改用上述构建基础设施精确过滤，避免扩大范围。
- 结构验收：对照 `git show HEAD:AtomUI.slnx`，34/34 项目路径保持一致，均存在且无重复；七组项目数符合表格；所有分类文件夹均无数字前缀；`dotnet sln AtomUI.slnx list` 真实解析通过；双语指引与原 CLI 命令检查通过。
- 最终模块验证：上述定向测试首次含构建通过 54/54、0 失败、0 跳过（测试阶段 20 秒）；移除数字前缀后复用同一程序集，追加 `--no-build` 再验通过 54/54、0 失败、0 跳过（18 秒）。含构建轮次出现 NuGet 漏洞数据源不可达的 NU1900 警告，测试仍通过。
- IDE 验收：本机 Rider 已显示 Fonts(2)、Gallery(3)、Icons(3)、Libraries(10)、Localization(7)、Tests(7)、Tooling(2)，符合默认名称排序；Gallery 展开可见三个示例项目；Localization 展开可见 `pt-BR` 五项目子组及两个本地化项目。未运行 Windows Visual Studio，该平台的实际展示未验证。
- 审查：独立只读 reviewer 检查最终 SLNX、双语 README 和本计划，未发现可行动问题。Gallery 首位、未收录测试项目及过程中的待勾选状态均按用户最新修正与本次边界处理，不扩展范围。
- 清理及交付：未新增永久测试、临时测试或 Demo；中止验证轮次的专用临时目录已删除；`git diff --check` 通过；未运行全量回归、集成测试或 AOT 发布；未创建分支、worktree 或提交。

## 后续：Rider 运行配置恢复

- 用户报告原有十二个运行配置全部出现红叉；实际编辑器错误为 `Project is not specified`，项目种类为 `Unloaded`。Rider 实际窗口与本地 `AtomUI.sln.DotSettings.user` 均证实 34 个项目被标记为卸载；这是本地 IDE 状态，不在 SLNX 中。
- 单变量验证：备份本地用户设置及 workspace，仅移除 Desktop 的一条卸载标记并重新打开同一解决方案；实际窗口由 34 unloaded 变为 33 unloaded，Desktop 配置红叉消失，因果成立。
- 恢复其余 33 条卸载标记并重新打开后，实际窗口的 34 个项目全部加载，配置菜单中的十二个红叉全部消失，无需删除或重新创建原配置。
- 核对并保存 IDE 自动派生的路径：Desktop 使用 `.artifacts/bin/Debug/net10.0`；Browser 使用 `.artifacts/bin/Debug/net10.0-browser` 及当前 WebAssembly Pack `10.0.10`，替换旧 `output/bin` 和 `10.0.8` 缓存。默认配置仍为 Desktop。
- 验证：用户设置除移除卸载标记外逐字节保持一致，XML 可解析；workspace 中所有原配置名称和类型保持一致。验收针对配置有效性和项目输出映射，不据此声称应用或全部测试运行成功。
- 处置：修改仅涉及被 Git 忽略的本地用户设置、IDE 自动保存的 workspace 及本节记录；本次备份目录在验证后清理，没有新增测试或提交。
