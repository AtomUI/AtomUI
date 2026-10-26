# 第二十轮实际平台结果

用户批准第二十批13文件快照07512eeba937，已在现有feature/custom-theme单提交并推送`6bd6076760e98bdf26f79260907df5fa105215ea`；13个blob逐项匹配，包含Expander默认函数及文档，不含后续Segmented产品改动或待批NavMenu方案。

[Actions 40278117820](https://github.com/AtomUI/AtomUI/actions/runs/40278117820)已完成，8个job全部成功。沿用base19eef870b9db62b1eb1c3b0897619ee8d466af46，full=false、all_platforms=true、browser=true；未取消／重试／发布NuGet。

| 实际Host／模式 | 结果 |
| --- | --- |
| Windows X64／Linux X64／macOS ARM64选测 | 各13项目、6405／6405，无失败／错误 |
| 三OS NativeAOT fixture | 各26项PRODUCT_CHECK及PRODUCT_PASS selected |
| Browser interpreted／AOT | 各23项PRODUCT_CHECK、实际DOM断言、exit0，Chromium／SwiftShader |

所有job head SHA、原生source-sha与Browser source_revision均为6bd607676。三OS各370主题资产物化通过，本地化消费各27对start／completed的stage与PID相符；26阶段exit0，唯一损坏feed反例Build active-corrupted consumer按原断言预期exit1。8份compact artifact只保存经检查的文本，zip仅在内存读取，没有保存取证媒体或可执行文件。

仍保留选测报告的conditional-registration-engines／native-aot／package-layout专项义务；fixture只证明实际执行范围，不替代所有条件引擎、包消费、控件完整输入／持续动效或原生最终像素。完整CT11–CT19／CT25仍开放，CT24暂缓；本轮不验证尚未提交的Segmented迁移。
