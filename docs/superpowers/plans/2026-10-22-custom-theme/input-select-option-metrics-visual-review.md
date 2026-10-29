# CT13 S6：Select 候选字体与行高视觉审查

范围：官方 Avalonia 12.1.4、macOS arm64、580×480、Skia 合成器与直接绘制；Light/Dark、Large/Middle/Small/Custom，同一实例与相同完整操作历史。固定八条原始选项、宽300、DisplayPageSize=3；没有移除标题、内容、最大高度或滚动条件。

本包仅申请下表36对配置差异：Own.OptionFontSize=21 / OptionHeight=54、实例OptionFontSize=27、清除实例、OptionHeight=0，以及同配置下点击选中后的状态。两种绘制逐对计数相同。其余24对（默认、清除、最终恢复、A/A、检错及恢复）旧新RGBA差异均0。不得把此表作为其他场景的容差。

实际变化：Own/实例场景的行高32→54、三行viewport96→162；自然行高配置最小值32→0、实际行高32→24、viewport96→72。文字字体按Own21或实例27显示，原选择仍选中第二项；根坐标、尺寸、八条内容、尺寸档位和选中状态记录60/60一致。默认及清除后实际字体14、行高32、viewport96。

作者已查看明暗、配置、自然行高、选中及恢复的关键截图：字号、行间距和弹层高度随指定输入变化，标题与输入框宽度保持，三行文字完整。此为作者独立复核，不是外部评审。原生API专项钉住弹层，不证明原生像素、raw输入、普通失焦关闭；分组、非均匀模板、其他配置和完整家族仍开放。

| 场景 | 变化像素 | 最大RGBA通道差 |
| --- | ---: | ---: |
| Light-Large-own | 29310 | 165 |
| Light-Large-local | 31356 | 165 |
| Light-Large-clear-local | 29310 | 165 |
| Light-Large-natural | 21821 | 165 |
| Light-Middle-own | 29310 | 165 |
| Light-Middle-local | 31356 | 165 |
| Light-Middle-clear-local | 29310 | 165 |
| Light-Middle-natural | 21821 | 165 |
| Light-Small-own | 29310 | 165 |
| Light-Small-local | 31356 | 165 |
| Light-Small-clear-local | 29310 | 165 |
| Light-Small-natural | 21821 | 165 |
| Light-Custom-own | 29310 | 165 |
| Light-Custom-local | 31356 | 165 |
| Light-Custom-clear-local | 29310 | 165 |
| Light-Custom-natural | 21821 | 165 |
| Light-ready-click | 31356 | 165 |
| Light-selected | 43972 | 227 |
| Dark-Large-own | 32282 | 156 |
| Dark-Large-local | 33983 | 156 |
| Dark-Large-clear-local | 32282 | 156 |
| Dark-Large-natural | 16716 | 160 |
| Dark-Middle-own | 32282 | 156 |
| Dark-Middle-local | 33983 | 156 |
| Dark-Middle-clear-local | 32282 | 156 |
| Dark-Middle-natural | 16716 | 160 |
| Dark-Small-own | 32282 | 156 |
| Dark-Small-local | 33983 | 156 |
| Dark-Small-clear-local | 32282 | 156 |
| Dark-Small-natural | 16716 | 160 |
| Dark-Custom-own | 32282 | 156 |
| Dark-Custom-local | 33983 | 156 |
| Dark-Custom-clear-local | 32282 | 156 |
| Dark-Custom-natural | 16716 | 160 |
| Dark-ready-click | 33983 | 156 |
| Dark-selected | 46615 | 199 |

审查状态：用户已明确批准本表36对限定范围（call_53954a594aa54c6f8039198e6e4d33ae:0）；不含Git授权，不扩大此前已批准范围。媒体查看后清理，仅保留文字和逐对计数。
