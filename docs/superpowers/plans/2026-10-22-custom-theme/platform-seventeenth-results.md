# 第十七轮整合提交与实际平台结果

用户已明确批准[27 文件整合快照](platform-seventeenth-transfer-consolidation.md)，SHA 为 f277f370248e。已在现有 feature/custom-theme 创建单个提交 `ff0d3d5726f11cbf4f39cd977fe1d1bd85021d34` 并推送；27 个提交 blob 与获批快照逐个相符，没有夹带 NavMenu 产品修复、临时测试或取证媒体，没有创建分支／worktree，也没有发布 NuGet。原第十七批 14 文件已被此整合提交包含，不得再重复执行旧快照。

[Actions 40252951271](https://github.com/AtomUI/AtomUI/actions/runs/40252951271)以该提交运行，base=19eef870b9db62b1eb1c3b0897619ee8d466af46，full=false、all_platforms=true、browser=true。8 个 job 全部完成且成功，未取消或重跑。

| 实际 Host／模式 | 结果 | 证据范围 |
| --- | --- | --- |
| Windows X64 选测 | 6,405／6,405 | 13 个实际选测项目，无错误或失败 |
| Linux X64 选测 | 6,405／6,405 | 同一项目集合，无错误或失败 |
| macOS ARM64 选测 | 6,405／6,405 | 同一项目集合，上一轮 NavMenu 采样失败本轮未再出现 |
| 三 OS NativeAOT | 各 26 项运行检查，PRODUCT_PASS selected | 三份 source-sha 均为 ff0d3d572，实际 OS fixture |
| Browser interpreted／AOT | 各 23 项运行检查，实际 DOM 断言通过 | 两份 source_revision 相同，Chromium／SwiftShader，exit0 |

三 OS 均物化 370 个真实主题资产；本地化包消费各有 27 对 start／completed，26 阶段 exit0，损坏 feed 反例 Build active-corrupted consumer 按现有断言预期 exit1。不存在未配对子进程或由最后一轮成功覆盖旧轮失败的记账。

已逐项检查 GitHub job 结论、compact latest.json、源 SHA、原生 runtime 标记、Browser result 与本地化阶段记录；仅解压文本，没有把 ZIP、媒体或二进制写入项目。选测 receipt 仍列出 conditional-registration-engines／native-aot／package-layout 专项义务；本轮实际 fixture 与已有专项各证明其自身范围，不能把这份列表或选测成功当作所有条件引擎、所有包消费与控件最终显示面的验收。

本轮完成已批准的提交、推送和一次验证动作。完整 CT11–CT19／CT25、各家族输入／持续动效／最终原生像素仍开放；CT24 按用户要求暂缓。NavMenu 初始折叠首次展开的产品语义短设计仍待单独批准。
