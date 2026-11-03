# CT15 GroupBox 原Global恢复限定调查表

状态：实际输入快照、完整managed/AOT、清理和精确门禁完成，GB1–GB4已获限定调查归属批准。详细证据仅见[原恢复结果](group-box-original-restore-results.md)。不申请新几何、3Reserved改Active、完整家族/CT15通过或提交。

| ID | 原完整历史中的路径 | 合成器仍失败 | 真实背景/边框输入快照恢复 | 1DIP正对照 | managed/AOT |
| --- | --- | --- | --- | --- | --- |
| GB1 | Light Global→Native→Native-clear | 4像素/max1 | 0 | 701/max255 | 对应整帧0，仍4/max1 |
| GB2 | Dark 同原完整历史 | 7像素/max1 | 0 | 701/max255 | 同上，仍7/max1 |
| GB3 | Compact 同原完整历史 | 9像素/max1 | 0 | 700/max255 | 同上，仍9/max1 |
| GB4 | Dark+Compact 同原完整历史 | 9像素/max1 | 0 | 700/max255 | 同上，仍9/max1 |

原420×200/长标题/90视口900extent、原41阶段×四算法完整操作与正常motion保持。只复制真实Geometry与不可变画刷，原历史结束后才绘制复制数据；没有计算新形状或搭近似控件。记录前后164张原图全0，无观测扰动。真实几何缓存与字段此前已验证恢复一致。

managed/AOT各64检查PASS，164原帧+12输入快照+4正对照共180对跨运行全0。原合成器非零仍严格FAIL，不用输入快照0替代呈现门禁；没有独立框架最小反例，不宣称完整框架定因或与既有FW2同根因。

AOT21729376字节，SHA256 `3104192772be555d89f4b182740c8d3d0bd2ba6c7418b00d51c0264132dcfd56`；0 IL/CS警告，五条既有Homebrew最低OS linker warning单列。701取证官方audit/cleanup clean后删除，外部450文件/1072590443字节及driver/发布/marker删除；精确GroupBox19PASS/0Skip、1s。无产品/测试源变化，匹配NET8证据复用。

仅提议GB1–GB4交CT25/FW2独立子记录继续调查，保留上述原严格FAIL与限制，不增容差、不打重绘补丁、不改官方依赖/FW1，不自动吞并其他家族。原40对视觉/3Reserved批准、其他边界及完整CT15保持。

## 限定调查归属批准

2026-11-02，用户选择“1. 仅批准GB1–GB4交CT25/FW2独立子记录继续调查；非零仍FAIL，不认定与原FW2同根因”（request_user_input_async / call_f24956aa080d42ff94485d804c7aa428 / item 0）。仅本表四路径新增独立调查记录；原40对/3Reserved及其他边界、完整CT15和不提交约束保持。
