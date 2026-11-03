# CT15 Message 原历史限定调查归属表（已获有限调查归属批准）

本表仅申请：将表列28条当前业务恢复、4条原独立检错路径，以及2条无AtomUI队列重排反例，交CT25/FW2继续调查。仅上述34路径已获有限调查批准，原话与请求ID见末尾；不认定每个原始像素都已精确独立复现，不批准非零正确，不改FW1/像素容差，不关闭Message或完整CT15。原native3497/max2、旧批/其他36对跨运行差异和其他家族均排除。

完整过程见[原历史恢复](message-original-history.md)。保持原920×700、四消息类型、20阶段与normal motion；下表每格为managed像素/max : AOT像素/max。性质检查通过不能替代严格像素恢复。

| Recipe | Own | Global | Native | Part | Type | RTL | Stack |
| --- | --- | --- | --- | --- | --- | --- | --- |
| light | 27/29 : 51/29 | 34/29 : 50/29 | 29/29 : 51/29 | 34/29 : 50/29 | 34/29 : 50/29 | 2/1 : 24/1 | 21/11 : 35/11 |
| light-compact | 36/15 : 36/15 | 43/15 : 40/15 | 37/15 : 35/12 | 37/29 : 41/15 | 37/29 : 41/15 | 38/29 : 38/29 | 55/29 : 46/29 |
| dark | 24/23 : 24/23 | 42/33 : 35/23 | 26/23 : 26/23 | 33/23 : 42/33 | 33/23 : 42/33 | 33/12 : 33/12 | 53/39 : 58/39 |
| dark-compact | 31/12 : 31/12 | 56/33 : 56/33 | 34/12 : 32/12 | 44/26 : 55/33 | 44/26 : 55/33 | 25/23 : 25/23 | 46/26 : 55/28 |

| Independent calibration | managed restore | AOT restore | A/A |
| --- | --- | --- | --- |
| light | 2/1 | 43/15 | 0 |
| light-compact | 38/29 | 38/29 | 0 |
| dark | 33/12 | 33/12 | 0 |
| dark-compact | 25/23 | 25/23 | 0 |

Evidence and limits:

- All4 actual normal-motion calibration restore images equal the independent no-AtomUI static image in full RGBA0, for managed and AOT. Their initial images differ; the initial-entry paint history is material.
- With motion disabled only in separate causal scenes, all28 business restores are0; all80 managed/AOT scene images are0 cross-runtime,236 property checks each. This is a diagnostic control, never a proposed product fix.
- Starting without entry but restoring normal motion before the remaining original business history still produces residual pixels; subsequent queue position transitions also contribute. The managed RTL return restores0 in that branch.
- The no-AtomUI official Avalonia12.1.4 data-driven resize/position-transition model produces6/max11 in light and4/max12 in dark on restoration, reproduced by NativeAOT; background redraw0. Compact models restore0.18/20 model cross-runtime frames are0; two Own frames differ9/max20 and3/max11.
- The model reproduces the class of paint-history failure, not every original image or pixel set: its Own-state images differ from the actual images by21/35/22/34 pixels (max20/20/21/22). No exact framework-root-cause claim or blanket transfer is made.
- Current normal main managed/AOT236 property checks each,80 frames with44 zero/36 nonzero cross-runtime. Those36 cross-runtime differences are not implicitly approved or moved by this table.
- Earlier P1 old/new37 nonzero, cross-runtime39 nonzero, polluted88-frame ledger and native3497/max2 keep their original records and ownership unless separately decided.

No product/dependency/host-setting/tolerance changes. Dedicated Semantic Styles retain the original generated classes and setters. Engineering cleanup is complete; the final exact gate is running. This table is not yet submitted for approval.


当前清理：1191取证经官方audit/cleanup clean后删除，795会话文件2176410897字节及marker删除，仓库无新增临时测试。最终17项精确Message门禁PASS/0Skip/4.5267s，复用相同最终源码NET8零警告零错误17.41s；最终helper/marker记录后删除。现仅提交上述34路径的有限调查归属裁决。纯模型严格比较命令实际返回2（2PASS/2FAIL），没有将采样退出0冒称恢复通过。


## Message 原历史34路径限定调查批准登记

用户明确回复“仅批准表列34路径归CT25/FW2调查，其余保留”（request_user_input_async / call_8107eb29ed024cfbb7dd7fd40dec2aed / item 0）。仅message-original-visual-review.md表列28当前业务恢复、4独立检错路径及2无AtomUI队列重排反例交CT25/FW2继续调查。模型未精确复现原全部像素的限制保持，不宣称完整框架根因；非零仍FAIL、不增容差、不关产品动效、不改FW1。原native3497、其他36跨运行非零、旧失败批、其他家族与完整CT15均保留，不授权提交。
