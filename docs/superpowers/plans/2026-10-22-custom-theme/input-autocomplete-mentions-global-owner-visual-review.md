# CT13 AM5：正文、光标与选区限定审查

2026-10-30，用户明确回复“批准 AM5 本表限定范围，保留 CT25”（call_137bdbb52074425385b7e76e4759ef93:0）。本表限定范围已批准，原始CT25调查保持；不代表完整CT13、其他平台、Git或通用容差。

四root各四尺寸×四variant，原245宽/Sample文本、1130×560完整窗口保持。有效Global：Light正文#a1287d、Dark正文#f5b8e4；错误Light#ae1122/Dark#ff9f9f；警告Light#855000/Dark#f7d477；禁用Light#767676/Dark#959595；选区#19599d、选中文字#fff0a0。标准Local为Green。所有正文/caret/选区输入均由实际公开owner提供，原Filled/Borderless状态文字优先关系保持。

## 自然48个有意变化场景

| owner/主题 | 配置 | 错误 | 警告 | 禁用 | Local | 选择 |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| AutoComplete Light | 5376 | 5328 | 5376 | 5376 | 5376 | 5856 |
| AutoComplete Dark | 5379 | 5376 | 5376 | 5352 | 5376 | 5856 |
| SearchEdit Light | 5376 | 5328 | 5376 | 6066 | 5376 | 5856 |
| SearchEdit Dark | 5376 | 5376 | 5376 | 6044 | 5376 | 5856 |
| TextArea Light | 5438 | 5378 | 5436 | 5436 | 5438 | 6346 |
| TextArea Dark | 5438 | 5436 | 5436 | 5402 | 5438 | 6346 |
| Mentions Light | 5449 | 5378 | 5436 | 5436 | 5449 | 6357 |
| Mentions Dark | 5451 | 5436 | 5436 | 5402 | 5451 | 6359 |

表内数值已按最终owner绑定实现重新采样并核对，为本次逐对变化像素，不是允许误差。最大通道差43～221，原始每对值在结果记录中保留；不把被否决原型通过当最终实现或用户批准。

## 默认与可观察值

自然44对默认/恢复等全RGBA0，1536对观测中的实际字号/Bounds保持。Dark的64个初始root Foreground getter由#e0000000对齐原实际正文#d8ffffff；实际默认文字颜色不变，标准Style/Local进入正文。这项可观察getter对齐同属本表范围。

## 四个自然清除边缘

| 场景 | 像素 / 最大通道差 |
| --- | --- |
| TextArea Light local-cleared | 2 / 1 |
| TextArea Dark local-cleared | 2 / 2 |
| Mentions Light local-cleared | 13 / 4 |
| Mentions Dark local-cleared | 15 / 6 |

严格0仍FAIL。原坐标/颜色保留，CT25继续；最终直接多行子集24对非配置均0，24对配置变化按各自后端单列。AA与1DIP复位必须零且检错有效，不能据本表放宽其他像素。

39专项、managed/AOT各6117、NET8零及清理后Core22已记录；480PNG与临时源/工程均清理，最终精确模块结果见[结果](input-autocomplete-mentions-global-owner-results.md)。原生资源覆盖、专用Style及实际选中字形均有验证。被否决的私有资源provider与Core snapshot辅助方法已撤回，最终无实例Token字典或新主题作用域。

批准仅限表内48场景、64个Dark root getter对齐及四对自然边缘，保留CT25；其他Own/Global/Part/状态、Form/Upload与完整CT13仍另验。
