# 第二十一轮 Actions 实际结果

2026-10-27重新读取 GitHub API 和本轮八份文本产物，核对已批准、已提交推送的第二十一批；未重复调度、取消或重试。

## 运行与源码身份

- [Actions 40291050592](https://github.com/AtomUI/AtomUI/actions/runs/40291050592)，attempt 1，`completed / success`，8/8 job成功。
- SHA为`278429b508275312d8592afed47806b38425e2cd`，NativeAOT和Browser产物的源码身份逐一相同。
- 运行时间为2026-10-26 16:15:52–16:36:25（Asia/Shanghai）。本轮包含Segmented第二十一批，不包含后续TabControl或NavMenu研发。

## 实际执行范围

| 路径 | 实际结果 |
| --- | --- |
| Windows-X64选测 | 13项目，6405/6405通过 |
| Linux-X64选测 | 13项目，6405/6405通过 |
| macOS-ARM64选测 | 13项目，6405/6405通过 |
| 三OS NativeAOT fixture | 各26条`PRODUCT_CHECK`，均`PRODUCT_PASS selected` |
| Browser interpreted / AOT | 各23条实际运行检查，DOM断言完成、受控退出，SwiftShader渲染 |

三个OS各有27组本地化子进程start/completed完整配对；其中26阶段exit 0，`Build active-corrupted consumer`预期损坏feed反例exit 1。三OS各实际物化370份主题资产并释放application scope。

## 验收边界

本轮没有发布NuGet。选测receipt仍列出`conditional-registration-engines`、`native-aot`、`package-layout`专项义务；独立fixture只证明其列明路径，不能据此关闭全部专项。

平台分包、编译及CI成功不等于完整控件家族验收。CT-11–CT-19、CT-25以及各家族原生最终显示面、适用输入和专项缺口继续开放；CT-24保持暂缓。第二十一批Git授权已执行完毕，后续提交仍须审查具体快照。

本地仅保存API状态、产物SHA、数值汇总及UTF-8文本，无下载zip留存；记录位于`.superpowers/sdd/platform-validation/ci-twenty-first-*`。
