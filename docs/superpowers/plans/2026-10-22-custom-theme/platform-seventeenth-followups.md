# 第十七批：TreeView 默认求值与 NavMenu 动效采样

范围是现有 `feature/custom-theme`／当前 checkout 的 TreeView N3-T 实现与文档、NavMenu 既有动效测试采样及第十六轮结果。没有新分支／worktree、NuGet 发布、产品 NavMenu 动效修改或测试重试；本批尚未提交，不沿用已执行的第十六批授权。

## 第十六轮实际结果

[Actions40207571681](https://github.com/AtomUI/AtomUI/actions/runs/40207571681)，HEAD `204c44149fdb0af5c7678b3f580a9e9948c56310`，8 job 全部结束，7 成功、Mac 选测失败。Windows／Linux 各 6405/6405，通过；各自 Gallery 412 项包含在选测中。三 OS NativeAOT 与 Browser interpreted／AOT 均成功。

Mac 已执行 5199 项：六个完成模块 1071/1071，Desktop 4127/4128；唯一记录失败为 `NavMenuInlineCollapsedTests.InlineCollapsed_Toggle_With_Motion_Animates_Width_Instead_Of_Jumping_To_Target`，展开时 `WaitForWidthBetween` 最后读到 300，不满足 `<300`。之后 1206 项未执行，不能报告为通过或断言后续没有问题。该轮本地化阶段只在 Windows／Linux 各收齐 27 对 start/completed：26 项 exit0，损坏 feed 反例 `Build active-corrupted consumer` 按现有 `ATOMUILOC006` 断言预期 exit1；Mac 没有进入该模块。

已收集完整 compact failure／stack／各平台结果，未取消或重跑该轮。执行授权已用完，新一轮仅在具体快照获批准后运行。

## NavMenu 采样调查与修正

本机单独运行原动效用例 1/1，通过。旧测试在 toggle 后两次 RunJobs／Sleep20ms，再开始读中间宽度；调度或一次 dispatcher drain 可以在下一次读取前越过已出现的实际布局帧。不能凭旧轮日志证明该历史帧的确切调度原因，本批先证明并修复可复现的观察缺口。

Temporary 探针保留原 420×240 Window、Width300、默认折叠宽度及真实展开→收起→展开历史。独立 Bounds 事件观察者先确认实际宽度严格位于端点之间，再推进到相同最终端点，随后调用旧采样器。收起与展开均以对应最终宽度 48／300 失败；两个关闭动效、直接跳到端点的负对照均被旧采样器拒绝。此前 Window 命名空间准备错误、仅在 drain 结束后读值及初始折叠 fixture 的未成功观察不算有效 RED，日志保留；初始折叠入口不在本批修复结论内。

修改仅在既有测试：采样器先订阅实际 `BoundsProperty`，再执行同一个 toggle；记录严格 `(lower+0.5, upper-0.5)` 内的布局宽度，保留 1 秒上限与最终宽度／base Width300／内部动画值清理断言，finally 解除观察者。不改变任何产品计时、动效长度、事件或主题，不增大超时、不重试、不放宽端点。

同四探针加原既有用例 5/5，通过：两个延迟读取保留中间帧，两个无动效直接跳变仍失败。这是实际布局采样证据，不声称原生像素或每次历史 CI 原因已证明。临时 C# 已删除，常驻用例净新增 0，已有用例继续提供相同动效合同保障。

## TreeView N3-T

详见[默认求值结果](container-tree-defaults-results.md)：12 个 Global-only 默认接入 V2，明暗 24 项实际值／类型／key 枚举数值／dense slot 不变；原 scope 消费本已正常，不能描述成消费 bug 修复。两个目录选中色归兼容保留，资源和配置往返保留，没有新目录树交互。正式四文档与 12 条 Token／1 条控件 CSV 同步，其余行保持。

168 帧中 56 A/A／24 恢复／28 新旧均 0，56 组 1 DIP 全检出，108 对资源／几何／状态相等；首轮 provider 算法与实际明暗 Window 补充证据分别保留。Mac managed／NativeAOT 各 304 项资源检查通过，不等于原生最终像素。两份临时源、外部工程、169 个媒体文件和七个专用 helper／源码备份已删除，最终 TreeView57 项及 Desktop NET8 Release 零 warning／error 通过。

## 最终清理、审查与提交边界

NavMenu／TreeView 清理后精确模块 365/365，通过，0 Skip，测试 12 秒；此前带临时探针的 369 项通过仅作诊断。最终单一 TreeView 产品 SHA 与已测 NET8 输入相同；NavMenu 产品源／Theme 与 HEAD 完全相同。字段目录逐行核对只改 12+1 行；作者另步自审已完成，不声称独立评审。本批 201 个文档链接有效；完整 Token 文件成员检查 0 问题（0 个 Avalonia Control），`git diff --check` 通过。最终快照另行冻结。

拟一个提交，包含本批冻结快照中的 TreeView 单文件产品实现、NavMenu 单文件测试采样、正式文档、目录及状态记录。所有临时源、外部工程、媒体与产物排除。实际跨平台修正结论须等获批推送后的新 Actions，完整 CT14／CT19／CT25、C9 首轮 AOT 输入缺失及其他家族仍开放。

## 冻结范围（14 文件）

- `docs/controls/desktop/data-display/tree-view/changelog.md`
- `docs/controls/desktop/data-display/tree-view/implementation.md`
- `docs/controls/desktop/data-display/tree-view/overview.md`
- `docs/controls/desktop/data-display/tree-view/token.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/container-consumers.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/container-tree-defaults-results.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/container-tree-defaults.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/control-inventory.csv`
- `docs/superpowers/plans/2026-10-22-custom-theme/platform-seventeenth-followups.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/platform-sixteenth-followups.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/tasks.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/token-contract-review.csv`
- `src/AtomUI.Desktop.Controls/TreeView/TreeViewToken.cs`
- `tests/AtomUI.Desktop.Controls.Tests/NavMenu/NavMenuInlineCollapsedTests.cs`

没有临时测试、外部驱动、图片或构建产物；计划一个提交。冻结清单保存在本地审查收据，批准前不暂存／提交／推送／调度。
