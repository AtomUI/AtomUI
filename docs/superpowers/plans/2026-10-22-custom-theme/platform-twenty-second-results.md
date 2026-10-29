# 第二十二轮 Actions 实际结果

用户批准27文件快照`b8d186b2ef8cf509740aea8b55bd53e8edbae332edb799ba4a41076c0b7447b0`后，已一次提交并推送`346732602a2657d3333282a2c10bdafdb405d0ac`。提交内27个blob与批准快照逐一相同，没有额外文件或amend。

## 运行身份

- [Actions 40428777879](https://github.com/AtomUI/AtomUI/actions/runs/40428777879)，attempt1，`completed / success`，8/8 job成功。
- 时间为2026-10-27 08:49:03–09:05:28（Asia/Shanghai）。
- 三NativeAOT、两Browser的实际产物源码SHA均与提交相同；没有重复调度、取消、重试或发布NuGet。
- 既有workflow使用`full=false`、`all_platforms=true`、`browser=true`，base为`19eef870b9db62b1eb1c3b0897619ee8d466af46`。

## 实际结果

| 路径 | 结果 |
| --- | --- |
| Windows-X64选测 | 13项目，6405/6405通过 |
| Linux-X64选测 | 13项目，6405/6405通过 |
| macOS-ARM64选测 | 13项目，6405/6405通过 |
| 三OS NativeAOT fixture | 各26条实际PRODUCT_CHECK，PRODUCT_PASS selected |
| Browser interpreted / AOT | 各23条实际运行检查，DOM断言完成、受控退出，SwiftShader |

三OS各实际物化370份主题资产；各27组本地化start/completed完整配对，26阶段exit0，`Build active-corrupted consumer`为预期损坏feed反例exit1。

八份文本产物全部取回并核对。两份产物首次下载失败，随后仅以无缓存请求恢复缺失的文本产物读取，没有重新运行Actions；zip只在内存中解压，未保存zip或媒体。

## 范围与授权

本轮包含Tab本机切片和此前记录，不包含后续Avalonia coercion根因调查的实现。选测仍列出conditional-registration-engines、native-aot、package-layout专项义务；fixture只证明自身路径。

本轮成功不关闭直接原生Styles.Remove、NavMenu首次展开、HorizontalMargin语义、各家族原生最终像素、完整CT11–CT19/CT25。CT24继续暂缓。第二十二批提交/推送/一次Actions授权已执行完毕，后续改动和新提交需另审具体范围。

本地文本receipt为`.superpowers/sdd/platform-validation/twenty-second-execution.json`、`ci-twenty-second-status.json`、`ci-twenty-second-summary.json`及相同前缀的提取清单。
