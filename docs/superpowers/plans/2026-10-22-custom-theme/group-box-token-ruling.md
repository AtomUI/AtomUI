# CT15 GroupBox 三个scalar的限定裁决提案（2026-11-01）

状态：三项ReservedDocumented已获限定批准；下文提案/待审为批准前历史。`token-contract-review.csv`中三字段继续NeedsDecision；不因既有文档写过“fieldset语义保留值”就当作本计划批准。1metadata/7Own与总137/274不变。

建议保留为ReservedDocumented：维持名称、double类型、资源key/slot、默认公式和配置解析，明确当前无几何消费者；不把它们改写成Active/ConsumerVerified。与FloatButton已批准两scalar的兼容思路相同，但其批准不自动授权本三字段。

| 字段 | 原默认/单位 | 源码与当前运行证据 | 为什么不强接现有几何 |
| --- | --- | --- | --- |
| TextPaddingInline | 1.0 em | 只定义/默认，Own0/2.5均进入resolved值；Left原几何不变 | 实际HeaderContentPadding是独立Thickness，默认Global PaddingXXS；em1启用会随字体变成另一套padding，既非同型alias也非默认迁移 |
| OrientationMarginPercent | 0.05，0～1比例语义 | 只定义/默认，Own0/0.67进入resolved值；Center原几何不变 | 实际左中右由HeaderTitlePosition+HeaderContainerMargin和布局决定，百分比乘谁/哪一边尚无发布合同 |
| VerticalMarginInline | Global.UniformlyMarginXS，DIP | 只定义/默认，Own0/37进入resolved值；Right原几何不变 | GroupBox没有对应纵向分隔线消费者，不能覆盖现有Header margin或body padding |

当前四Active：ContentPadding、HeaderContainerMargin、HeaderIconMargin、HeaderContentPadding；左中右、原生覆盖/clear均进入实际目标。三scalar的几何不变只证明无消费，不等于完整GroupBox、视觉或出口通过；正常Skia三版与清理后门槛须在审查前补齐。

替代方案：

1. 删除字段：会改变既有生成键、配置与slot兼容，超出本次授权；不采用。
2. 立即启用：必须先定义em/百分比/纵向语义与RTL/尺寸/实际目标，可能改变默认布局，与现四Active冲突；不能假装只接线。
3. 标为Alias：现无同型/同义目标，无法证明等价，不采用。
4. 保留当前未决：可继续，但不能把未决数降为0或写为已消费。

未来启用条件：另立版本化方案，明确目标、计算单位/比例基准、优先级、默认兼容与迁移说明；提供原/新各状态和极值、RTL/Compact、动态/退出/资源证据并取得独立批准。当前只申请保持兼容的Reserved分类，不授权实现新几何或提交，不扩大CT25/FW1。


最终三版各164帧，同历史scalar-zero/scalar-custom阶段均与默认完整RGBA零差异；各自四算法解析/默认28记录保持。42聚焦及观察者模板/卸载退休验证通过，candidate/AOT各64检查/164cross全0。清理后门槛尚待补齐；本段仍是证据，不替代用户对三Reserved的批准。


清理后最终验收（2026-11-01）：共享锁空闲独占后，GroupBox既有19PASS/0FAIL/0Skip（用例581ms，命令26.97s）；NET8 Desktop Release 0警告/0错误（构建18.08s，命令18.25s）。23临时用例/3源19113字节、545取证、414driver/15publish及503原会话文件已删除，6最终文件和两个marker也清理，audit clean、git diff --check通过。三版164静态/cross0，68主恢复0；Global恢复4/7/9/9 max1保留。仅40明确静态及三Reserved兼容提案待限定批准，NeedsDecision仍3，原裁剪/低对比/平台/完整出口及CT15保留；CT12/13/14和CT25/FW1不改，不提交。


## GroupBox三项兼容分类批准登记

2026-11-01 用户明确回复“批准三项 Reserved，其余边界保留”（request_user_input_async / call_c8732844a7a841caacc4385b24e13bd3 / item 0）。仅TextPaddingInline、OrientationMarginPercent、VerticalMarginInline归ReservedDocumented；保持名称/类型/默认/key/slot及原布局，无几何消费者，不标Active/ConsumerVerified。40对视觉仍待限定批准，恢复4/7/9/9 max1及其他家族/完整CT15边界保留，不授权提交。CT15 Own未决由3减为0，原137metadata/274Own及CT12/13/14、CT25/FW1不变。
