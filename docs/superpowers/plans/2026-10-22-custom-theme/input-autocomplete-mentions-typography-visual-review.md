# CT13 AM3：字体与多行root字号限定审查

2026-10-30，用户已明确回复“批准五表限定范围，保留 CT25/FW1”。本表限定视觉已批准；批准来源 request_user_input_async / call_5f004b68efc344048653a3d95ae55694 / item 0。范围与原始计数保持，CT25/FW1保留，无Git授权。只审本表明确变化；不批准完整CT13、其他字体/平台、混合主题表面缺陷、Git或通用容差。AM2亦已获本次汇总限定批准。

## 条件与可观察合同

四外观（AutoComplete、SearchEdit、TextArea、Mentions）×Large/Middle/Small/Custom，明暗、原文字15字符与245宽、1130×560窗口保持。普通/搜索未配置字号仍父继承23；多行默认可见16/14/12/14保持。多行root标准FontSize的主题值现与原可见预设对齐：原getter都23，现16/14/12/14（两root共8条）；标准Style/Local值仍优先。没有新增Own/API或参数范围。

配置字体：各公开owner Global.FontFamily=Times New Roman，FontSize/LG/SM=24/28/18，Custom24。标准Local字体：四root Arial31。实际GlyphRun核对均为对应物理字体。原多行行框22不足容纳Arial31的ink32已修复：默认行框按实际natural spacing增大，显式owner FontHeight20仍为20，清除恢复。完整默认/恢复等16对0，不改变默认可见字体。

## 精确6对变化

| 场景 | Light变化像素 / 最大通道差 | Dark变化像素 / 最大通道差 | 可见结果 |
| --- | --- | --- | --- |
| 有效Global字体 | 42964 / 228 | 58281 / 235 | 四root真实字体/对应字号生效；多行自然行框增长 |
| 同配置禁用 | 58031 / 82 | 16218 / 189 | 文字及行框随有效字体；禁用状态保持；dark混合白表面可读性限制见下 |
| 标准Local Arial31 | 28167 / 224 | 62587 / 235 | root API进入真实文字；单行原宽与横向滚动/裁剪、多行行数/滚动保持 |

原/候选AA及扰动复位0，标题1DIP检出2015；每阶段正常动效且至少650ms与3稳帧。首候选的行框裁剪失败没有藏入容差，修复后的候选才用于上表。具体像素只标识本次范围，不设允许误差。

## 限制与验证

原dark局部provider位于light外层白背景，禁用文字旧/新均低对比不可辨。本包没有修复或批准这一既有缺陷；保留CT25/完整状态出口。单变量背景改匹配#141414后，禁用文字可见，实际字体保持；此为额外诊断，不替换原版/候选历史，也不修改产品背景或颜色。

24临时检查通过，包括真实输入/selection/caret/只读/禁用、4专用Input Style字体与LineHeight声明命中/撤回、8图GC；当前Mac managed/AOT各1686、NET8零警告、成员3文件2Control0问题。临时源、备份、原生工程及89媒体已清理，最终精确模块结果见[结果](input-autocomplete-mentions-typography-results.md)。

批准只覆盖表内6对配置/Local变化、默认可见字形保持前提下的8个多行root FontSize值对齐，以及默认行框增长和显式FontHeight仍优先的合同；不包括混合白表面问题、其余消费者、其他字体/平台、完整CT13或Git。

本次批准由[最后五表汇总](input-final-visual-review.md)统一记录；CT13已在既定批准范围完成，原始FAIL与独立CT25/FW1不改写。
