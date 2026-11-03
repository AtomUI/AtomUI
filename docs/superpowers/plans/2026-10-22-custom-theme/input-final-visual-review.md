# CT13 最后限定视觉汇总

2026-10-30。用户已明确批准本汇总全部五表限定范围，原话：“批准五表限定范围，保留 CT25/FW1”。批准来源：request_user_input_async / call_5f004b68efc344048653a3d95ae55694 / item 0。既有CHT/S7/CB5/AM5/FU2等批准保持。本次为明确视觉批准，不以此前“继续研发”代替。

| 表 | 限定变化 |
| --- | --- |
| [AM2候选指标](input-autocomplete-mentions-metrics-visual-review.md) | 48个popup状态：默认96→100、viewport92→96，Own行高/padding/0与恢复；其余10对0 |
| [AM3字体](input-autocomplete-mentions-typography-visual-review.md) | 6个Global/禁用/Local字体场景，8个多行root字号getter与原可见预设对齐；多行自然行框按真实字体增长，显式FontHeight仍优先 |
| [AM4占位/弹层/加载](input-autocomplete-mentions-surfaces-visual-review.md) | 8配置场景、两主题loading布局margin24→1/2/3/4、frame90→48，以及表内12对自然边缘1或5点/通道差1；直接40非配置0 |
| [FU3字体族](input-form-upload-typography-visual-review.md) | 七公开字体族两配置图11859/11821像素，18非配置0，原Label/Header/正文语义字号保持 |
| [FU4实际消费](input-family-exit-visual-review.md) | 有效字号、暗色正文/冒号可读性、四Upload列表配置及三选择输入选区；Circle配置清除与18非配置对已修复至0 |

批准只适用于五表列明的当前本机范围；AM3混合白表面限制、原始自然严格像素FAIL、官方Avalonia直接Style退役等既有CT25/FW1仍开放。不扩大字体/平台/其他模式或像素容差，不包含Git操作。五表之外未提出新的产品行为批准。

当前执行状态与最终清理/模块门禁统一见[CT13家族结果](input-family-exit-results.md)。

## 完成记录

五表限定视觉与既有工程、755项清理后精确验收/managed与AOT各810、临时清理共同构成CT13既定范围出口；CT13已完成并关闭。CT25/FW1按上列原始边界保留，不扩大其他字体/平台、恢复模式或像素容差；未提交。
