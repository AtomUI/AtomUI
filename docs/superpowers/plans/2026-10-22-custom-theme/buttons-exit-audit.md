# CT-12 当前逐 owner 出口核对

最终状态：2026-10-30，用户已明确回复“批准本表限定范围，保留 CT25”（request_user_input_async / call_a2fe796a8df0481aadd7e4393060b87e / item 0）。公开BoxShadow工程、默认原生验证、清理后189项及限定视觉批准已全部完成，CT12在既定批准范围完成并关闭。原始24/max1、两对3968/max2及其他既定历史仍按CT25保留，不设通用容差，不扩大其他平台/背景/恢复模式，无Git授权。 早期阶段描述与原始FAIL保留，当前不再待V11/采样/视觉批准。见[完成结果](buttons-box-shadow-finish-results.md)。


2026-10-30状态更新：公开BoxShadow已选定/实现，默认原生managed及AOT各1236检查与52帧/12跨运行0、清理后189项/NET8/成员门禁已完成。当前仅待[最后限定视觉汇总](buttons-box-shadow-final-visual-review.md)，本文件旧V11待选或待采样描述保留为历史，不能作为当前状态。


2026-10-29，当前feature/custom-theme及未提交候选。已导出字段/业务及当前未修改产品的原生无输入矩阵通过，[总出口](buttons-family-exit-results.md)记录156专项、清理后179项与两运行各482原生检查。原始问题表复核另发现V11预设色阴影缺稳定几何入口，CT12仍待其公开合同及[两对混合宿主细线](buttons-native-exit-review.md)核定，不把原始FAIL或CI自动换算为通过。

当前五个Own终端为Button50、Dropdown46、Hyper17、Option19、Split1，共133。历史首轮110未含后来增加的Dropdown23，不覆盖历史计数。CSV当前128有效字段为RuntimeConsumerVerifiedLocal、5历史字段为CompatibilityRetained；既有V2及后续消费者分包合并到总出口。导出字段全数消费不代表未导出的V11入口已经完成。

| 公开owner | Own | 当前出口证据 | 保留边界 |
| --- | ---: | --- | --- |
| Button | 50 | 45有效字段实际消费/清除，5历史兼容；字号/继承/字重/shape/variant/loading、输入/GC及最终原生 | 混合宿主上边缘阴影两对待审；长寿命Flyout框架关系及旧RGB1归CT25 |
| DropdownButton | 46 | 全46字段实际消费/隔离；额外内容、真实展开/关闭/更换、专用Style、GC及最终原生 | 所列混合宿主阴影细线待审；其他Host归CT25 |
| HyperLinkButton | 17 | 全17字段实际消费；状态/Frame绘制/loading、优先级/重模板/重挂/输入/GC及最终原生 | 真实平台Launcher及其他Host仍由CT25验证；不新增Part或IsGhost语义 |
| SplitButton | 1 | 实际Flyout间距/更换、主操作点击、子Button独立身份/优先级、专用Style/GC及最终原生 | 固定Custom宽140既有溢出CT20；子Button所列阴影细线待审 |
| IconButton | 0 | 独立Global、动态时长、实际输入/内容替换/内容本地画刷优先、GC及最终原生 | 其他Host及完整平台输入CT25 |
| ToggleIconButton | 0 | 四状态画刷、checked输入/恢复、内容替换/画刷优先、GC及最终原生 | 其他Host及完整平台输入CT25 |
| OptionButton | 19 | 全19字段与实际Group依赖，checked/hover/pressed/disabled、样式优先级、GC及最终原生 | 历史有限RGB1恢复归CT25 |
| OptionButtonGroup | 0 | 选择/方向/尺寸/Outline与Solid、生成容器替换和重挂另一Group、GC及最终原生 | 其他Host及完整平台输入CT25 |
| Commons.SimpleButton | 0 | 用户核定原生兼容边界；原生属性/输入/内容/GC及最终原生；不新增注册身份 | 其他Host及共享平台专项CT25 |

基础/抽象实现按实际公开owner追踪，不自动为抽象实现增造身份。SimpleButton的目录类型key保留，不能把它当成可配置的已注册Control identity。

## 必须保留的原始失败与限制

- Flyout移动恢复边缘Light9/Dark5个RGB1像素：与禁用新订阅、仅调用原API的A/B一致；同阶段旧新0，但原恢复非零保持。见[剩余消费者](buttons-remaining-consumers-results.md)。
- Button/Option的既有4/2像素RGB1恢复差异继续保留，未设置通用容差，见[消费者结果](button-consumers-results.md)。
- 长寿命且仍被Button.Flyout赋值的Flyout通过Avalonia基类Opened/Closed关联旧Button；新订阅已释放，禁用新增实现仍失败。保持官方12.1.4的决定没有修复此关系，CT25继续。
- 早期原生稳定性40帧构建早于后续字体/状态修复。当前分包managed/AOT成功不替代最后产物的原生显示面或所有平台。
- Hyper状态同步夹具2654像素失败及root首候选默认值3FAIL均已定位并保留；前者用等时异步重采且硬断言目标，后者Frame空值回退。不得改写为原始全绿。

当前后续：已导出字段/消费/业务和本机限定原生矩阵通过；CT12等待V11公开阴影定制入口合同及两对混合宿主细线核定，原始时序调查继续CT25；CT20处理已明确尺寸问题。没有创建提交；作者自审受仅主agent约束，无独立评审。
