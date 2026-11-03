# CT13 FU3：七公开字体族限定审查

2026-10-30，用户已明确回复“批准五表限定范围，保留 CT25/FW1”。本表限定视觉已批准；批准来源 request_user_input_async / call_5f004b68efc344048653a3d95ae55694 / item 0。范围与原始计数保持，CT25/FW1保留，无Git授权。只覆盖本表及最终汇总引用，不含其他字体、平台、Git或通用容差。

900×900、父Courier New/23、七公开owner（Form、FormItem、Decorator、Upload、DropArea、Trigger、DropZone）、原Font Sample文字及原几何保持。Form/Item Label14、正文23、DropArea Header16/Sub14不因本字体族包改变；各owner有效FontFamily=Times New Roman进入实际字形。

| 场景 | Light变化像素/最大通道差 | Dark变化像素/最大通道差 |
| --- | ---: | ---: |
| 七owner显式字体族配置 | 11859 / 224 | 11821 / 199 |
| 其余默认、AA、检错复位、清除、父字体改变/返回、Local/清除18对 | 0 / 0 | 0 / 0 |

作者已查看明色配置与暗色默认。102卡片的原有长文字裁剪不在本包修复范围。表内暗色默认仍为历史FU3结果；最终Decorator/DropZone正文及表单冒号可读性另由FU4表明确修复，不拿旧默认图代替最终图。

15字体优先级/继承/重建/GC通过，最终与FU4联合52专项通过；真实Mac managed/AOT各196字体检查，另614出口检查，NET8与成员门禁通过。全部临时清理及最后模块结果统一记录于[家族出口](input-family-exit-results.md)。

本次批准由[最后五表汇总](input-final-visual-review.md)统一记录；CT13已在既定批准范围完成，原始FAIL与独立CT25/FW1不改写。
