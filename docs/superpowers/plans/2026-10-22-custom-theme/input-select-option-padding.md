# CT13 S2：Select候选项OptionPadding

源码事实：公开Own.OptionPadding按有效Global.ControlPaddingHorizontal及(ControlHeight-FontHeight)/2计算，但候选项主题没有引用，实际继承ListView三档ItemPadding。仅Select创建SelectCandidateListItem，TreeSelect/Cascader使用各自候选项，不扩大本包修改。

本包按字段既有“选项内间距”合同接通真实候选项Padding，使用SelectTokenResource；不重写默认公式、不新增字段或WhenExplicit旁路。默认字段值与旧ListView padding不一致，必须实际取证，不能仅由数值推导像素变化。当前自然44对表明默认及清除仍0差异：旧12,0与有效Own12,5在既有32行高/垂直居中下绘制相同；显式配置与0有具体文字位置变化，须单独审查。若未获批准，本包保持开放。

Ruling：按已公开字段语义修复实际消费，不将其归Legacy或仅显式值生效——字段名称、公式与正式文档共同指向候选项内间距；代价是配置文字位置会改变，需具体视觉批准。源码初步推测默认也会改变，实际证据已纠正该推测，不把属性差异当像素差异。

- [x] R：真实Popup四尺寸Own非均匀及0反例，ListView配置干扰；清除回有效默认，实例/专用Style优先级另核。
- [x] B：保留内容、根尺寸、原DisplayPageSize与滚动条/viewport、键盘和选择轨迹；先原版明暗完整帧、A/A/1DIP检错与复位，固定上下限和标题。
- [x] I：在候选项已有派生主题选择器中声明Padding，不改C#数据/输入事件、框体或其他ListView消费者。
- [x] V：专项GREEN、旧新完整帧、实际managed/AOT/NET8；按具体默认/配置差异独立审查，不借NU3批准。
- [x] C：删除临时测试/外部工程/媒体后精确Select模块验证、文档及作者另步自审。无Git，完整家族/CT25继续开放。

Local implementation/validation/cleanup complete; S2 exact visual scope approved call_ecb156e340b04c4283629d0797214f27:0. See [results](input-select-option-padding-results.md).
