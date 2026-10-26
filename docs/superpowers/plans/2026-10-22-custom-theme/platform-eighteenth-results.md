# 第十八轮实际平台结果

用户明确批准第十八批13文件快照 f8e4279bfc31；已在现有 feature/custom-theme 单提交并推送 `93ef4ba8dc8804fbef2f02db892340a7954b868f`，逐个 blob 与获批内容相符，未包含 Pagination、NavMenu 产品改动或取证产物。

[Actions 40259455495](https://github.com/AtomUI/AtomUI/actions/runs/40259455495)已完成，8个 job 全部成功。输入沿用 base19eef870b9db62b1eb1c3b0897619ee8d466af46，full=false、all_platforms=true、browser=true，未取消或重跑，没有发布 NuGet。

| 实际 Host／模式 | 结果 |
| --- | --- |
| Windows X64、Linux X64、macOS ARM64 选测 | 各13项目、6405／6405，无失败／错误 |
| 三 OS NativeAOT fixture | 各26项运行检查，PRODUCT_PASS selected |
| Browser interpreted／AOT | 各23项运行检查，实际DOM断言通过、exit0，Chromium／SwiftShader |

所有原生 source-sha、Browser source_revision 及 GitHub job 的 head SHA 均对应93ef4ba8d。三OS各物化370个真实主题资产；本地化消费各27对 start／completed 完整，26阶段exit0，损坏feed反例 Build active-corrupted consumer 按现有断言预期exit1。

compact证据仅解压文本，源码、计数、运行标记与子进程阶段已核对。选测仍列有 conditional-registration-engines／native-aot／package-layout 专项义务，各fixture仅证明其所执行范围；不把整轮成功换算成所有条件引擎、全部包消费、控件输入或原生最终像素的验收。完整CT11–CT19／CT25仍开放，CT24按用户要求暂缓。
