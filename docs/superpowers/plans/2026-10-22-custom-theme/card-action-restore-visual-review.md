# CT15 Card安排往返24路径限定调查表

状态：当前原完整120帧/240性质通过；清理与Card20门禁待完成，不申请像素或完整CT15通过。详细证据：[原安排往返](card-action-restore-results.md)。

| ID | 四算法/尺寸/RTL条件 | left→right→left严格像素/max | bbox | Action子树状态恢复 |
| --- | --- | --- | --- | --- |
| CA1 | compact-large-False | 18/1 | [80, 321, 480, 330] | 相同 |
| CA2 | compact-large-True | 18/1 | [80, 321, 480, 330] | 相同 |
| CA3 | compact-middle-False | 18/1 | [80, 321, 480, 330] | 相同 |
| CA4 | compact-middle-True | 18/1 | [80, 321, 480, 330] | 相同 |
| CA5 | compact-small-False | 18/1 | [80, 321, 480, 330] | 相同 |
| CA6 | compact-small-True | 18/1 | [80, 321, 480, 330] | 相同 |
| CA7 | dark-large-False | 15/24 | [80, 321, 480, 327] | 相同 |
| CA8 | dark-large-True | 14/22 | [80, 321, 480, 327] | 相同 |
| CA9 | dark-middle-False | 15/24 | [80, 321, 480, 327] | 相同 |
| CA10 | dark-middle-True | 14/22 | [80, 321, 480, 327] | 相同 |
| CA11 | dark-small-False | 15/24 | [80, 321, 480, 327] | 相同 |
| CA12 | dark-small-True | 14/22 | [80, 321, 480, 327] | 相同 |
| CA13 | darkcompact-large-False | 10/1 | [80, 321, 480, 330] | 相同 |
| CA14 | darkcompact-large-True | 9/1 | [80, 321, 480, 330] | 相同 |
| CA15 | darkcompact-middle-False | 10/1 | [80, 321, 480, 330] | 相同 |
| CA16 | darkcompact-middle-True | 9/1 | [80, 321, 480, 330] | 相同 |
| CA17 | darkcompact-small-False | 10/1 | [80, 321, 480, 330] | 相同 |
| CA18 | darkcompact-small-True | 9/1 | [80, 321, 480, 330] | 相同 |
| CA19 | light-large-False | 15/2 | [80, 321, 480, 327] | 相同 |
| CA20 | light-large-True | 15/2 | [80, 321, 480, 327] | 相同 |
| CA21 | light-middle-False | 15/2 | [80, 321, 480, 327] | 相同 |
| CA22 | light-middle-True | 15/2 | [80, 321, 480, 327] | 相同 |
| CA23 | light-small-False | 15/2 | [80, 321, 480, 327] | 相同 |
| CA24 | light-small-True | 15/2 | [80, 321, 480, 327] | 相同 |

原400×240Card/完整header/body、两个Action、Width120/Height32、Blue/1专用Style、三档/两方向/模板与命中、独立plain全部保持。没有产品变更、额外重绘或新模型。当前JIT一完整批；原实现验收AOT240/120cross0只按其原源码范围复用，不冒充本轮新发AOT。

差异位于Card底部圆角，当前样本没有落在蓝色divider；24个Action子树位置/大小/可见/Opacity/Clip/背景边框/字体文本恢复一致。未读取本轮整Card外框全部server/cache数据，没有完整框架定因或独立反例，不认定与原Card40路径同根因。

仅提议CA1–CA24交CT25/FW2独立子记录继续调查；非零仍FAIL，不改官方依赖/FW1/容差。原24旧新left-restore比较、其他Card/平台及完整CT15排除，不因48对分隔线批准扩大。不授权提交。

Cleanup:officialaudit clean/findings=[],removed169(168evidence+manifest);external141files/618167776bytes,all drivers/marker removed.No product/project temporary sources,matchingNET8zero18.78s reused;focusedCard20gate follows.No full regression/commit.

最终清理后精确Card20PASS/0FAIL/0Skip、866ms；产品源未改，匹配NET8零警告18.78s复用。169取证/141会话均清理；仅CA1–24当前self-restore有限调查待裁决，非零仍FAIL，原old/new及完整CT15保持，无全量或提交。

## 限定调查归属批准

2026-11-02，用户选择“1. 仅批准CA1–CA24当前self-restore交CT25/FW2独立子记录调查；非零仍FAIL，不认定与原Card或FW2同根因”（request_user_input_async / call_07f1a5e29f9d4b85abb6d5f74661719f / item 0）。仅当前24 self-restore新增独立调查，原24 old/new、原Card40路径/其他边界、完整CT15及不提交约束保持。
