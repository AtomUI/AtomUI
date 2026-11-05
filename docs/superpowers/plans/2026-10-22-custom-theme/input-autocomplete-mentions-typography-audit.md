# CT13 AM3：复合输入字体接续审计

2026-10-30。AM1默认机制已完成；AM2指标工程/清理/聚焦完成、48个popup视觉待用户批准。现有目录/分支、单agent，不提交。下一步先建立字体真实失败及默认继承表，不把Global合法但未消费一概判为bug。

## 当前源码事实

- 三AutoComplete外观和Mentions的root没有FontFamily/FontSize主题入口或实际继承父桥。
- 私有AutoCompleteLineEditBox/SearchEditBox/TextAreaBox/MentionTextArea复用公共输入主题及StyleKey；真实文本在其模板的TextPresenter。
- LineEdit/Search的可选字体与TextArea的三档预设不同，不能统一成一个会改变默认的字体模型。
- 原根FontSize/FontFamily局部值、父继承、各档与私有输入实际字形先分别量取；候选文本与独立public子Control另记，不从root值推断。
- FontFamily/FontSize均复用既有Global与标准属性，不新增Own/公开旗标。实现方案必须在失败、默认与优先级事实明确后细化。

## 审计顺序

- [x] 三AutoComplete+Mentions，中号实际TextPresenter对各自Global.FontFamily/FontSize的配置核对（父Courier New/23、目标Times New Roman/29），先有效失败。
- [x] 明暗、四尺寸、父字体变化、实例Local/普通Style与清除，记录root/private输入/真实文本的默认、当前值和实际typeface。
- [x] 单独测foreign LineEdit/SearchEdit/TextArea配置是否进入私有资产，核实owner与模板投射，不因StyleKey字符串直接改Core。
- [x] 据实际模型明确可选字体、预设fallback、标准API投射、专用Input Style优先级及0/清除/重模板/GC合同，再写最小实现计划。
- [x] 保持未配置默认/原输入和焦点；新增视觉恢复具体限定审查，完整CT13持续开放。

执行结果与仍待批准范围见[AM3结果](input-autocomplete-mentions-typography-results.md)及[限定审查](input-autocomplete-mentions-typography-visual-review.md)。独立字体分包不代替完整CT13。
