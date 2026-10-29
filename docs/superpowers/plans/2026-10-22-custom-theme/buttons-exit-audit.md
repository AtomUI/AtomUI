# CT-12 当前逐 owner 出口核对

2026-10-27，当前feature/custom-theme及未提交候选。完整家族仍开放；本表区分当前已落地证据与继续出口，避免把局部通过或CI当成家族验收。

当前五个Own终端为Button50、Dropdown46、Hyper17、Option19、Split1，共133。历史首轮110未含后来增加的Dropdown23，不覆盖历史计数。CSV中125字段记录V2默认求值、3字段记录后续配置消费者、5字段兼容保留；逐字段绘制及状态结论继续依实际消费链和报告核定。

| 公开owner | Own | 当前已有证据 | 未关闭出口 |
| --- | ---: | --- | --- |
| Button | 50 | V2/字体依赖/显式字重/形状、Global/变体颜色、禁用描边、Flyout实际Target间距、loading开关；五历史字段保留 | 全字段/组合状态最终映射、最后产物原生显示面；长寿命Flyout关联旧Button；原始边缘差异 |
| DropdownButton | 46 | 独立Global与新增23颜色/阴影Own隔离、字体、固定Height、loading开关 | 所有弹层/业务状态及最后原生显示面、完整字段矩阵 |
| HyperLinkButton | 17 | 字体/排版默认、loading、icon-only/danger、root绘制、禁用Own实际描边；同实例/优先级/模板/重挂与当前Mac AOT | 全部导航/业务及跨Host最终矩阵；不新增Semantic Part合同 |
| SplitButton | 1 | 两子按钮组合边界、边框、字体、分隔线及原生值优先 | Custom固定Width140时既有子项溢出归CT20；组合弹层/关闭、完整状态与最后显示面 |
| IconButton | 0 | 独立Global、图标尺寸/颜色、Transition时长、Spin/Pulse投射与开关、内容Icon本地优先 | 完整输入/动态内容/生命周期及跨Host出口 |
| ToggleIconButton | 0 | 四个既有状态画刷实际消费、checked/input保持、Global及Transition | 完整内容更换/释放与最后显示面、跨Host |
| OptionButton | 19 | V2、比例/字体/留白依赖、背景显式回退、选择/hover/pressed/disabled与Group隔离 | 原始小像素差异及最后显示面；完整字段/组合状态映射 |
| OptionButtonGroup | 0 | 独立Global、组几何/方向、继承字体、选择/状态与Item隔离 | 回收/生成容器、完整组组合/生命周期/跨Host矩阵 |
| Common.SimpleButton | 0 | 用户核定原生兼容包装；默认ContentControl模板、原生属性委托、三点点击及默认/定制画面0差异、当前Mac AOT | 无独立AtomUI身份/Own；其他Host与共享基础设施最终证据随CT25 |

基础/抽象实现按实际公开owner追踪，不自动为抽象实现增造身份。SimpleButton的目录类型key保留，不能把它当成可配置的已注册Control identity。

## 必须保留的原始失败与限制

- Flyout移动恢复边缘Light9/Dark5个RGB1像素：与禁用新订阅、仅调用原API的A/B一致；同阶段旧新0，但原恢复非零保持。见[剩余消费者](buttons-remaining-consumers-results.md)。
- Button/Option的既有4/2像素RGB1恢复差异继续保留，未设置通用容差，见[消费者结果](button-consumers-results.md)。
- 长寿命且仍被Button.Flyout赋值的Flyout通过Avalonia基类Opened/Closed关联旧Button；新订阅已释放，禁用新增实现仍失败。保持官方12.1.4的决定没有修复此关系，CT25继续。
- 早期原生稳定性40帧构建早于后续字体/状态修复。当前分包managed/AOT成功不替代最后产物的原生显示面或所有平台。
- Hyper状态同步夹具2654像素失败及root首候选默认值3FAIL均已定位并保留；前者用等时异步重采且硬断言目标，后者Frame空值回退。不得改写为原始全绿。

当前后续：完整消费/状态映射与未解释视觉差异持续归CT12/CT25；CT20处理已明确尺寸问题。其他家族按依赖顺序继续，不用这些跨任务出口形成相互等待。没有创建提交；作者自审受仅主agent约束，无独立评审。
