# CT15 ImagePreviewer 当前静态31帧限定调查表

状态：完整历史/AOT和全部临时清理完成，117项精确门禁随后登记。不申请像素通过或完整CT15关闭。唯一详细证据：[原静态出口](image-previewer-static-exit-plan.md)。

仅提议下表31个明确静态帧路径作为CT25/FW2独立子记录继续调查，列出当前跨运行及同JIT重复值。所有非零仍FAIL，不认定与既有FW2同根因，不改变官方依赖、像素容差或加入产品重绘补丁。

| ID | 原完整历史帧名 | 当前JIT/AOT像素/max | 同JIT重复像素/max |
| --- | --- | --- | --- |
| IP1 | compact-native-group-next.png | 4/1 | 4/1 |
| IP2 | compact-native-group-reset.png | 88/7 | 59/7 |
| IP3 | compact-native-group-waiting-retained.png | 47/7 | 41/7 |
| IP4 | compact-native-single-business-restore.png | 0/0 | 4/1 |
| IP5 | compact-native-single-next.png | 4/1 | 4/1 |
| IP6 | compact-native-single-reset.png | 118/7 | 92/6 |
| IP7 | compact-native-single-waiting-retained.png | 68/7 | 65/6 |
| IP8 | compact-overlay-group-business-restore.png | 0/0 | 4/1 |
| IP9 | compact-overlay-group-next.png | 0/0 | 4/1 |
| IP10 | compact-overlay-group-waiting-retained.png | 0/0 | 4/1 |
| IP11 | compact-overlay-single-business-restore.png | 4/1 | 4/1 |
| IP12 | dark-native-group-reset.png | 103/20 | 96/20 |
| IP13 | dark-native-group-waiting-retained.png | 81/20 | 55/20 |
| IP14 | dark-native-single-reset.png | 37/20 | 53/7 |
| IP15 | dark-native-single-waiting-retained.png | 28/20 | 34/7 |
| IP16 | darkcompact-native-group-business-restore.png | 4/1 | 4/1 |
| IP17 | darkcompact-native-group-next.png | 4/1 | 0/0 |
| IP18 | darkcompact-native-group-reset.png | 109/7 | 46/13 |
| IP19 | darkcompact-native-group-waiting-retained.png | 51/7 | 35/13 |
| IP20 | darkcompact-native-single-business-restore.png | 4/1 | 4/1 |
| IP21 | darkcompact-native-single-next.png | 0/0 | 4/1 |
| IP22 | darkcompact-native-single-reset.png | 77/7 | 80/20 |
| IP23 | darkcompact-native-single-waiting-retained.png | 55/7 | 51/20 |
| IP24 | darkcompact-overlay-group-business-restore.png | 4/1 | 0/0 |
| IP25 | darkcompact-overlay-group-next.png | 4/1 | 0/0 |
| IP26 | darkcompact-overlay-single-business-restore.png | 0/0 | 4/1 |
| IP27 | darkcompact-overlay-single-waiting-retained.png | 4/1 | 4/1 |
| IP28 | light-native-group-reset.png | 65/7 | 118/7 |
| IP29 | light-native-group-waiting-retained.png | 57/7 | 108/7 |
| IP30 | light-native-single-reset.png | 79/18 | 119/7 |
| IP31 | light-native-single-waiting-retained.png | 35/18 | 68/7 |

三次完整448帧历史各200性质PASS；当前JIT/AOT384静态359零/25FAIL，64持续态均非零；同JIT384静态356零/28FAIL，并集31。原29中7本轮零/22非零，另3个新cross帧，不替代原历史结论。64份状态文本三版逐字同。实际保留命令探针仅在JIT副批读取，AOT没有该探针。

实际native1344×896、Overlay900×650，保留原全部内容/专用Style/Own/Global/Native/状态/图像/变换/重开/正常motion。PopupRoot0.93、Footer0.8透明度为原明确输入。没有新图像或动画模型、没有产品源码变化；同JIT副批28FAIL已披露，不宣称观测无扰动。

像素示例位于真实导航与底部圆角面，图像内容没有变化。Light native-single reset79/max18分为左导航15/右导航29/底部35；waiting35/max18仅底部。44个实际框体reset/waiting记录中42相同，两项差别是预期的导航启用/背景变化；页码1/3→3/3，所以不能把整块footer作同输入恢复断言。上述观察没有证明所有隐含server输入相同或完整框架根因。

排除：全部64持续态、原171旧新非零、原29中未列路径、OS/Browser/输入、其他家族与完整CT15。不批准非零正确或既有FW2同根因，不授权提交。

Cleanup:officialaudit clean/findings=[],removed1601(1600evidence/records+manifest);external449files/1380330922bytes,driver/publish/marker removed.No project temporary test inputs.Product source freeze unchanged;117focused gate follows,matchingNET8zero18.78s reused.No full regression or commit.

最终清理后精确ImagePreviewer：117PASS/0FAIL/0Skip、12s；没有产品源码修改，匹配NET8零警告18.78s复用，不重发AOT或全量。1600取证+清单及外部449文件/1380330922字节、所有driver/publish/marker已清理；无新增永久测试或提交。31静态路径有限调查待用户裁决，非零仍FAIL。

## 限定调查归属批准

2026-11-02，用户选择“1. 仅批准表列IP1–IP31交CT25/FW2独立子记录继续调查；非零仍FAIL，不认定与原FW2同根因”（request_user_input_async / call_9e655f84b3c9485081b24f637009f2b2 / item 0）。仅31明确静态帧新增独立调查记录；原其他29路径/171旧新/64持续态、OS/Browser/输入、完整CT15及不提交约束保持。
