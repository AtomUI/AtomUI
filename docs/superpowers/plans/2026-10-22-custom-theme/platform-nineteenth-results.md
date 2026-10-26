# 第十九轮实际平台结果

用户批准第十九批16文件快照4e0dbc3e188c，已在现有feature/custom-theme单提交并推送`0b5e70c71e7580572364a28c9e2b5fec82adc978`；16个blob与获批快照逐项相符，包含Pagination分包，不含后续Expander或待批NavMenu产品改动。

[Actions 40274888121](https://github.com/AtomUI/AtomUI/actions/runs/40274888121)已完成，8个job全部成功。base仍为19eef870b9db62b1eb1c3b0897619ee8d466af46，full=false、all_platforms=true、browser=true；未取消、重试或发布NuGet。

| 实际Host／模式 | 结果 |
| --- | --- |
| Windows X64／Linux X64／macOS ARM64选测 | 各13项目、6405／6405，无失败／错误 |
| 三OS NativeAOT fixture | 各26项PRODUCT_CHECK及PRODUCT_PASS selected |
| Browser interpreted／AOT | 各23项PRODUCT_CHECK、实际DOM断言、exit0，Chromium／SwiftShader |

所有job的head SHA、原生source-sha及Browser source_revision均为0b5e70c71。三OS各370主题资产物化成功；本地化消费各27对start／completed，stage／PID对应，26阶段exit0，唯一预期损坏feed反例Build active-corrupted consumer为exit1。8份compact artifact仅在内存读取zip并保存文本，没有保存媒体、压缩包或可执行文件。

选测仍列出conditional-registration-engines／native-aot／package-layout专项义务；各fixture只证明实际执行范围，不作为全部条件引擎、包消费、控件输入、持续动效或原生最终像素的验收。完整CT11–CT19／CT25仍开放，CT24按用户决定暂缓；本轮没有验证尚未提交的Expander迁移。
