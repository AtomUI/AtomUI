# CT15 FloatButton 四条原最终恢复限定调查表

详细证据唯一入口：[原历史/真实保留命令结果](float-button-original-restore-results.md)。没有产品修正，不申请像素通过、原65对扩大或完整CT15通过。

| ID | 完整原路径 | 原managed/AOT最终恢复 | 本轮原重放 / 最终观测 | 当前真实保留输入 |
| --- | --- | --- | --- | --- |
| FR1 | Light default→Own/Global/Native/badge/Group/RTL/template/reattach/input/scroll→end-restore | 20/max4 / 17/max3 | 15/max4 / 20/max4 | 21个Border有效字段/资源相同，命令原始字节相同 |
| FR2 | Dark同完整原历史 | 11/max3 / 11/max3 | 11/max3 / 8/max3 | 同上 |
| FR3 | Compact同完整原历史 | 8/max1 / 8/max1 | 8/max1 / 8/max1 | 21个有效字段/资源相同；一个透明Frame仅结构体填充字节不同 |
| FR4 | Dark+Compact同完整原历史 | 2/max1 / 2/max1 | 2/max1 / 2/max1 | 21个Border有效字段/资源/原始字节相同 |

原1100×900/窄宽长文/原滚动数据/受控开关/真实点击/正常motion保留；三批均200帧、72检查68PASS/4严格FAIL。真实Own正对照每算法检出15个Border输入变化。只读诊断通过公开ElementComposition取实际保留命令，再读取其字段；没有重绘、修改状态或近似模型。不同批次9静态帧非零已披露，不能宣称观测完全无扰动。诊断仅JIT，没有新AOT发布；原AOT失败证据保留，不冒称同一新诊断已AOT通过。

仅提议FR1–FR4作为CT25/FW2独立子记录继续调查，非零仍FAIL，不认定与既有FW2同根因，不改官方依赖/FW1/像素容差。原32业务往返、115旧新/34cross、平台、极值及完整CT15均排除；这些不随四条调查归属而关闭。待清理与精确门禁后请求有限裁决，不授权提交。

清理：官方audit clean/findings=[]，cleanup removed=true/621项（620取证文件及清单）；外部160文件/618351353字节、驱动/build/marker已删除。无项目内临时测试输入；清理后精确FloatButton门禁随后登记。

清理后匹配Debug测试程序集精确 `FullyQualifiedName~AtomUI.Desktop.Controls.Tests.FloatButton`：46PASS/0FAIL/0Skip，2s。无新产品或测试编译输入，沿用同源码NET8零警告20.57s；没有新AOT发布、全量回归或Git提交。限定调查表待用户裁决。

## 限定调查归属批准

2026-11-02，用户选择“1. 仅批准FR1–FR4交CT25/FW2独立子记录继续调查；非零仍FAIL，不认定与原FW2同根因”（request_user_input_async / call_d57ed2ca38d943fbbcd5d84f74a1c7cc / item 0）。仅本表四条原最终恢复新增调查归属；其他往返/跨运行与完整CT15、不提交约束不变。
