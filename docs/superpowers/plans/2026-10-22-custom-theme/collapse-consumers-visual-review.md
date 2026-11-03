# CT15 Collapse 限定视觉审查（2026-11-01）

状态：表内12对已获限定批准；其余边界保留。仅申请下表12对Global/native/native-clear明确非零变化，完整Collapse/CT15仍开放。实际Global字体/原语义标题、正文、边框、IconSizeSM进入真实目标；原生Courier17/Orange正文仍优先，标题保留其明确Global字号23/标题色角色，clear恢复Global。没有默认资源值、类型、key或slot纠正；四默认旧新零。

明确保留原520宽、长Header/Extra及正文原slot裁剪。字体23和图标31使自然高度增长，长文末端可裁到半glyph；不自行改换行/省略/宽度。暗色显式#123456标题/#102030正文在暗底低对比，同样列明，不暗改颜色算法。Own及四专用Style阶段旧新零，其运行验证不扩大为本表有意变化。

三版各220帧完整同历史；候选/AOT各128检查全PASS，204静态跨运行全0（下表12对亦全0），11跨运行定时非零不批准。三版各76主业务恢复零，另各4个Global->native-clear为5/max1，保持原未通过。最终75临时阶段73PASS/2GC FAIL，两项回收失败及原Standalone/Debug/Release账本均保留，不归入FW1。临时清理及清理后50项/NET8验收已完成（详见文末），待限定批准。

比较：800x600完整同坐标RGBA、零容差，无裁剪/缩放/遮罩/对齐。3张四算法静态对照与关键全图、4张三版motion表已由作者查看；对照缩略图仅辅助观察，所有数值取原帧。

| 算法 | 阶段 | 变化像素 | 最大通道差 | BBox | 跨运行 |
| --- | --- | ---: | ---: | --- | ---: |
| compact | global | 32337 | 239 | [140, 70, 660, 348] | 0 |
| compact | native-clear | 32339 | 239 | [140, 70, 660, 348] | 0 |
| compact | native | 31598 | 255 | [140, 70, 660, 338] | 0 |
| dark | global | 33553 | 204 | [140, 70, 660, 396] | 0 |
| dark | native-clear | 33553 | 204 | [140, 70, 660, 396] | 0 |
| dark | native | 32823 | 235 | [140, 70, 660, 386] | 0 |
| darkcompact | global | 32342 | 204 | [140, 70, 660, 348] | 0 |
| darkcompact | native-clear | 32342 | 204 | [140, 70, 660, 348] | 0 |
| darkcompact | native | 31599 | 235 | [140, 70, 660, 338] | 0 |
| light | global | 33550 | 239 | [140, 70, 660, 396] | 0 |
| light | native-clear | 33552 | 239 | [140, 70, 660, 396] | 0 |
| light | native | 32822 | 255 | [140, 70, 660, 386] | 0 |

不申请其他12旧新/11跨运行定时非零、Global恢复micro、GC失败、真实OS/Browser/完整焦点/极值/家族出口及完整CT15。既有CT12/13/14批准及CT25/FW1原始问题继续保留，不换官方依赖、不扩大像素容差、不授权提交。


清理后最终验收（2026-11-01）：共享锁空闲独占后，Collapse既有50PASS/0FAIL/0Skip（用例1s，命令27.62s）；NET8 Desktop Release 0警告/0错误（构建18.27s，命令18.45s）。25临时用例/3源21834字节、723取证、414driver/15最终publish及518原会话文件已删除，6最终文件和两个marker也清理，audit clean、git diff --check通过。原75所选73PASS/2GC FAIL保留，不能用最终既有50绿覆盖原失败。仅12明确静态待限定批准，4 Global恢复5/max1、12旧新/11cross定时、GC/平台/完整家族与CT15未通过边界保持；CT12/13/14和CT25/FW1不改，不提交。


## 六包限定批准登记

2026-11-01 用户明确回复“仅批准六表现有条目及列明纠正，其余保留”（request_user_input_async / call_08c452ba85d7442ab817b3cf6c7dcce6 / item 0）。仅Flyouts48对及表列两项有效默认纠正、Collapse12对、Descriptions16对、GroupBox40对、Statistic12对、QRCode16对获得限定视觉批准。各表列明的裁剪、低对比、原失败、缺帧、恢复与持续态、平台及完整家族/CT15均保留，不授权提交；GroupBox三Reserved另按call_c8732844a7a841caacc4385b24e13bd3/0既有批准，不扩大。
