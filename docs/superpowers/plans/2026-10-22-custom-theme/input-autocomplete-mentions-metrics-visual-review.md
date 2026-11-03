# CT13 AM2：候选行高与 padding 预算限定视觉

2026-10-30，用户已明确回复“批准五表限定范围，保留 CT25/FW1”。本表限定视觉已批准；批准来源 request_user_input_async / call_5f004b68efc344048653a3d95ae55694 / item 0。范围与原始计数保持，CT25/FW1保留，无Git授权。

范围：650×740产品Window，明/暗局部Provider（外部Window仍亮色），三种AutoComplete外观与Mentions、八条原选项、DisplayCandidateCount3、宽320及原输入/标题。非聚焦输入、正常motion；同实例完整历史AA/标题1DIP复位→每个popup普通→Own48→Own48+Padding10→Global高度48/间距8→Own0/padding0→默认恢复→关闭。不减数据或移除高度约束，不裁剪/重对齐比较，不设通用容差。

**本表包含默认外观修复。** 默认名义预算补齐最终padding：96→100，内容viewport92→96，候选项最小高度仍32，三行恢复完整。默认/清除的popup整体及阴影下边随之变化，输入本体不变。

配置Own.OptionHeight48时，实际行由原32恢复48，均匀普通行viewport144/extent384。Own48+Padding10预算应164且viewport仍144；旧预算滞后，实际行也未跟随。Global高度48/间距8时应预算152/viewport144；0/0时原残留8高度的内容/阴影归零，business popup open和原数据保持。任意更大字体/逐行专用Style不在“三行完整”保证内。

每主题29个历史阶段，共116张/58对，10对输入本体/AA/检错复位/关闭全0；48对对应四外观、两主题的6个popup状态。以下保留每对计数。

| Theme/control/stage | Changed RGBA pixels | Max channel delta |
| --- | ---: | ---: |
| light/AutoComplete-popup | 12560 | 230 |
| light/AutoComplete-height | 1800 | 165 |
| light/AutoComplete-padding | 18795 | 165 |
| light/AutoComplete-global | 14238 | 165 |
| light/AutoComplete-zero | 16382 | 28 |
| light/AutoComplete-default | 12560 | 230 |
| light/AutoCompleteSearchEdit-popup | 12557 | 69 |
| light/AutoCompleteSearchEdit-height | 1800 | 165 |
| light/AutoCompleteSearchEdit-padding | 18796 | 165 |
| light/AutoCompleteSearchEdit-global | 14239 | 165 |
| light/AutoCompleteSearchEdit-zero | 16418 | 28 |
| light/AutoCompleteSearchEdit-default | 12557 | 69 |
| light/AutoCompleteTextArea-popup | 12558 | 45 |
| light/AutoCompleteTextArea-height | 1800 | 165 |
| light/AutoCompleteTextArea-padding | 18796 | 165 |
| light/AutoCompleteTextArea-global | 14239 | 165 |
| light/AutoCompleteTextArea-zero | 16420 | 28 |
| light/AutoCompleteTextArea-default | 12558 | 45 |
| light/Mentions-popup | 5358 | 45 |
| light/Mentions-height | 3525 | 224 |
| light/Mentions-padding | 10665 | 224 |
| light/Mentions-global | 8700 | 224 |
| light/Mentions-zero | 8164 | 28 |
| light/Mentions-default | 5358 | 45 |
| dark/AutoComplete-popup | 6175 | 149 |
| dark/AutoComplete-height | 1793 | 145 |
| dark/AutoComplete-padding | 18151 | 214 |
| dark/AutoComplete-global | 10179 | 195 |
| dark/AutoComplete-zero | 12388 | 224 |
| dark/AutoComplete-default | 6175 | 149 |
| dark/AutoCompleteSearchEdit-popup | 8611 | 195 |
| dark/AutoCompleteSearchEdit-height | 1793 | 145 |
| dark/AutoCompleteSearchEdit-padding | 18798 | 214 |
| dark/AutoCompleteSearchEdit-global | 14240 | 195 |
| dark/AutoCompleteSearchEdit-zero | 12339 | 224 |
| dark/AutoCompleteSearchEdit-default | 8611 | 195 |
| dark/AutoCompleteTextArea-popup | 12566 | 195 |
| dark/AutoCompleteTextArea-height | 1793 | 145 |
| dark/AutoCompleteTextArea-padding | 18798 | 214 |
| dark/AutoCompleteTextArea-global | 14240 | 195 |
| dark/AutoCompleteTextArea-zero | 13724 | 224 |
| dark/AutoCompleteTextArea-default | 12566 | 195 |
| dark/Mentions-popup | 5366 | 195 |
| dark/Mentions-height | 3520 | 198 |
| dark/Mentions-padding | 10669 | 214 |
| dark/Mentions-global | 8703 | 198 |
| dark/Mentions-zero | 7024 | 235 |
| dark/Mentions-default | 5366 | 195 |

作者已查看明色AutoComplete普通前后、Mentions普通前后、暗色Mentions大padding及0前后：行文字和候选强调保留，默认末行不再少4DIP，配置时三行正确随Own变高，0不再残留窄条；没有用隐藏内容规避正高度场景。

同版本AA与标题复位严格0；1DIP检错每侧1906像素。12项功能/输入/Style/GC、联合18项及当前Mac managed/AOT各543通过，NET8零17.52s；8临时源/427工程文件/162媒体文件和工具fixture均已清理，最终28+1项及成员门禁通过。

只申请本表默认/配置及恢复的具体变化，不包含其他字体、组/复杂模板、其他平台、原生最终像素或完整CT13；不含Git和通用容差。

本次批准由[最后五表汇总](input-final-visual-review.md)统一记录；CT13已在既定批准范围完成，原始FAIL与独立CT25/FW1不改写。
