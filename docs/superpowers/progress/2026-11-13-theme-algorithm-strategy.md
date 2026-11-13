# 主题算法策略化 执行台账

- 规格：`docs/superpowers/specs/2026-11-13-theme-algorithm-strategy-design.md`
- 计划：`docs/superpowers/plans/2026-11-13-theme-algorithm-strategy.md`
- 分支：`feature/design-language`，基线 `release/6.0`

每个任务结束时登记：提交号、测试结果、性能数字、与计划的偏差。裁决写成「裁决：决定 — 原因 — 判断错了的代价」。

## 执行前

### 用户的决定（2026-11-13）

- 执行方式：子代理逐任务执行，子代理一律使用 Opus。
- 提交方式：每个任务审阅通过后由主控提交到本分支，不推送；实现者不提交。
- 变基：先变基到 `release/6.0` 再开始。

### 变基到改写历史后的 `release/6.0`

`release/6.0` 在 2026-11-13 被改写过提交信息（内容未变：旧基点 `c6820d19b` 与新历史中的对应提交 `65e266c76` 树完全相同），之后新增到 `f11459de0`（Avalonia 12.1.6）。

- 做法：`git rebase --onto release/6.0 c6820d19b`，只重放本分支自己的 226 个提交。变基前的分支顶端保留在本地分支 `backup/design-language-pre-rebase-20261113`。
- 上游新增 9 个提交：Windows 渲染循环按 DWM 出帧、SDK 11 构建支持、生成器包定义静态状态调整、Native8 文档、Toolchain ILLink8 兼容、README、TextArea 高度修复、6.2.14 发版准备、Avalonia 12.1.6。
- 两边都改过的文件 9 个。真正冲突的只有 `CHANGELOG.md` 与 `CHANGELOG.zh-CN.md`，冲突两次，都是本分支的 `Unreleased` 条目与上游新增的 `6.2.14` 段落落在同一位置。
- 裁决：`Unreleased` 段保留在 `6.2.14` 段之上 — 本分支的条目尚未发布 — 判断错了只需挪动段落。
- 核对：变基后两份 CHANGELOG 的 `Unreleased` 段与变基前逐行相同，`6.2.14`、`6.2.13` 段与上游逐行相同；全仓没有残留的冲突标记。`TextAreaDecoratedBox.cs` 及其主题 axaml 由 Git 自动合并。
- 变基后分支领先 `release/6.0` 226 个提交，落后 0。

### 变基后的全量回归（执行基线）

- `scripts/run-full-regression.sh`：`[SUMMARY] TOTAL: 12317 个测试全部通过, 总耗时 15:19`（14 个项目，未含默认排除的本地化集成测试）。
- `AtomUI.Themes.Tests` 不在全量脚本的默认项目里，单独运行：27/27 通过。之后凡涉及主题包的任务都要单独跑它。

### 计划的执行前修订

- PrimeNG 改造用的编译快照不放进 `docs/superpowers`（该目录只放 Markdown），改放仓库外的临时目录，不提交。
- 计划里的文件行号取自变基之前，只作定位提示。

## 任务记录

