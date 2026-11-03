# CT15 Tag 当前剩余七比较限定调查表

本表仅复用已清理的本轮[Core快照根因与三版结果](icon-retained-geometry-investigation.md)，不重采样、不新发AOT、不改变Tag或CT13派生。Core共同规则/20例证已批准，不等于下列像素通过。

| ID | 精确原完整历史比较 | 当前JIT及AOT结果 |
| --- | --- | --- |
| TR1 | Dark default→own→own-zero→own-clear | 24/max1，两运行相同 |
| TR2 | Compact同路径 | 2/max1，两运行相同 |
| TR3 | Compact global→native→native-clear | 5/max1，两运行相同 |
| TR4 | Dark+Compact default→own→own-zero→own-clear | 21/max1，两运行相同 |
| TR5 | Dark+Compact global→native→native-clear | 7/max2，两运行相同 |
| TR6 | Dark+Compact原独立大矩阵large-top→large-bottom→large-restore | 12/max1，两运行相同 |
| TR7 | Compact group-scroll同帧JIT/AOT | 4/max1，bbox(115,174)–(176,177) |

原10723/10790/10858完整Tag主历史、独立校准及大矩阵均保持；1000×1200、66色×Variant、原完整文字、Group180×100/12项、Select两项、正常motion和真实输入不删改。JIT/AOT原主104/校准4/大矩阵12性质通过；本轮52个明确恢复46零/表列6非零，所有未通过继续保留。Core/Tag/Avatar总356cross353零，TR7是其中一个，不能因353零而放行。

限制：332份状态sidecar旧新及跨运行同帧逐字相同，**不是证明每个A→B→A前后全部有效值相同**。本轮没有保留上述六恢复各像素的完整节点归属/内部server/绘制缓存证明；没有独立最小框架反例，不能断言它们与旧FW2同根因或都是框架错误。只将其作为可能外部绘制问题的独立调查提案，不提出根因修复结论；如证据要求更高，继续留CT15阻塞。

工程及清理证据已完成：六Core临时用例通过，AOT46808144字节/0 IL/CS警告；2070取证+清单、822会话/3233947432字节、全部driver/发布/marker及六临时源均删除。清理后Tag/Avatar/Icon精确78及Common Avatar7通过；后续Dot显隐一处调用不经过本Tag场景，未变Core专项按其范围复用，当前最终NET8零警告18.78s。没有新增临时工程、永久测试、全量回归或提交。

仅提议TR1–TR7交CT25/FW2独立子记录继续调查，非零仍FAIL、不认定同根因；不改官方依赖/FW1/容差。原Tag其他历史恢复/旧新/五cross账本、CT13派生及完整Tag/CT15均排除，不自动吞并其他路径。

## 限定裁决登记

2026-11-02，用户选择“1. 仅批准TR1–TR7交CT25/FW2独立子记录继续调查；非零仍FAIL，不认定同根因”（request_user_input_async / call_cf1ff601c6bc454c97b38099dfa1ed5e / item 0）。仅本问题/本表范围生效；未列路径、证据不足处、原始失败与完整CT15、不提交约束保持。

## 2026-11-03 实质修复候选补证

Tag最终原主/大矩阵/校准120性质、180静态已在同一临时软件Skia候选上JIT/AOT和完整server参考全0；原TR1–TR7历史FAIL不改写，项目官方依赖未采用，当前不能据候选通过关闭产品/CT15。完整原因、源码/产物身份和最后验收只见[唯一结果](feedback-region-sdk-results.md)，不重复采样或另提归属选择。
