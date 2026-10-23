# CT-12 剩余按钮消费链分包

继续现有已授权家族计划，主agent独立实现，不提交。先关闭两条已有公开字段消费链，再处理缺少实际区域的历史字段决策；不为消除unused凭空增加额外内容或改变CompactSpace边框。

## 合同与方案

- DefaultBorderColorDisabled：只在默认配色Outlined/Dashed禁用按钮中作为显式可选覆盖；未配置时仍使用BorderColorDisabled，保留历史通用禁用配置，其他配色/变体不串用。ControlTheme选择器与内部转换器声明，普通Style/LocalValue保持原生优先级。
- GutterToFlyout：Button关联AtomUI Flyout及其MenuFlyout，且该Flyout实际Target是该Button时，显式配置的Own间距进入MarginToAnchor。未配置保留原Flyout默认0；不把Token默认4强制写入从而改变旧弹层位置。Flyout自身本地值优先；普通Avalonia Flyout无对应Margin合同不注入。RequestedPlacement为null仍保留原生定位语义，不为了演示擅自切换定位模式。
- 非Visual Flyout不在Button模板树，AXAML Setter无法绑定另一个对象；代码兜底须写明原因，订阅actual Target，关闭/替换/分离释放绑定；共享Flyout复用到另一个Button时只消费当前Target owner，不让旁边Button抢占。
- Button ExtraContent四字段无对应额外区域、GroupBorderColor对应Ant旧Button.Group而本库目前只有CompactSpace，不能盲目作用到整个边框。保留原字段/ID，形成具体兼容决策交最终审查，不自批永久豁免。

## 执行与验收

- [x] 临时测试先复现禁用特定描边、打开Flyout间距未消费；原默认/通用配置为正对照。
- [x] 实现后验证scope更新/清除、显式0、raw resource与LocalValue优先级、Flyout共享/替换/关闭/重开/分离/GC；原生和Overlay实际host按适用检查。
- [x] 改前后脚本化明暗默认禁用/弹层画面0未解释差异，A/A和1DIP正对照；新配置生效、同状态恢复0。真实输入保持，不用桌面自动化。
- [x] 当前Mac NativeAOT实际配置/恢复，删除临时源/媒体/驱动后明确filter聚焦验证、NET8构建、成员/文档/diff及作者自审。

执行细节、既有9/5像素边缘现象与长寿命Flyout关系见[结果](buttons-remaining-consumers-results.md)；x仅覆盖这两条消费链，不代表历史五字段或家族整体批准。
