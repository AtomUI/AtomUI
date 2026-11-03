# CT15 GroupBox 限定视觉审查（2026-11-01）

状态：表内40对已获限定批准；其余边界保留。仅申请下表40对有意变化：28对修正固定外框下居中/右移、Own/透明/RTL和空标题后的旧缺口残留；12对Global/native/native-clear进入实际字体/颜色/边框/圆角/图标目标。三scalar的Reserved兼容分类另见[group-box-token-ruling.md](group-box-token-ruling.md)，仍NeedsDecision，不因视觉通过自动批准。

保留原420×200、长Header、scroll900/view90/offset180、原透明缺口和renderScale算法。Global23/Part27与Native Header27仍沿原宽度裁剪；fieldset标题上半部保持原透明背景，暗色显式#102030/原生Purple在暗底低对比已列明。正文在offset180视口外，表中背景/标题/缺口有视觉证据，正文Font/Foreground由有效值验证，不夸大正文glyph覆盖。

三版各164完整静态，候选/AOT各64检查PASS，164cross全0、下表40对cross全0；旧新版124零/40非零。三版各68主恢复0；candidate/AOT Global->native-clear明4/暗7/Compact9/暗Compact9像素、max1仍未通过，不批准为容差。独立A/A/restore0、1DIP检出。42聚焦PASS，临时材料已清理，清理后19项与NET8零警告通过（详见文末）。

800x600完整同坐标RGBA零容差，无遮罩/对齐/裁剪。10张四算法对照与关键全图由作者查看；缩略图只辅助观察，全部数值取原帧。

| 算法 | 阶段 | 变化像素 | 最大通道差 | BBox | managed/AOT |
| --- | --- | ---: | ---: | --- | ---: |
| compact | center | 352 | 19 | [199, 233, 512, 235] | 0 |
| compact | global | 76621 | 239 | [190, 226, 610, 420] | 0 |
| compact | header-empty | 394 | 19 | [227, 233, 424, 235] | 0 |
| compact | native-clear | 76621 | 239 | [190, 226, 610, 420] | 0 |
| compact | native | 76977 | 255 | [190, 226, 610, 420] | 0 |
| compact | own-center | 212 | 19 | [210, 247, 533, 249] | 0 |
| compact | own-right | 424 | 19 | [210, 247, 586, 249] | 0 |
| compact | right | 708 | 19 | [199, 233, 601, 235] | 0 |
| compact | rtl-right | 708 | 19 | [199, 233, 601, 235] | 0 |
| compact | transparent-right | 708 | 19 | [199, 233, 601, 235] | 0 |
| dark | center | 320 | 23 | [207, 237, 512, 239] | 0 |
| dark | global | 74924 | 227 | [190, 230, 610, 420] | 0 |
| dark | header-empty | 394 | 23 | [235, 237, 432, 239] | 0 |
| dark | native-clear | 74924 | 227 | [190, 230, 610, 420] | 0 |
| dark | native | 75242 | 247 | [190, 230, 610, 420] | 0 |
| dark | own-center | 212 | 23 | [210, 247, 533, 249] | 0 |
| dark | own-right | 424 | 23 | [210, 247, 586, 249] | 0 |
| dark | right | 644 | 23 | [207, 237, 593, 239] | 0 |
| dark | rtl-right | 644 | 23 | [207, 237, 593, 239] | 0 |
| dark | transparent-right | 644 | 23 | [207, 237, 593, 239] | 0 |
| darkcompact | center | 352 | 23 | [199, 233, 512, 235] | 0 |
| darkcompact | global | 76621 | 227 | [190, 226, 610, 420] | 0 |
| darkcompact | header-empty | 394 | 23 | [227, 233, 424, 235] | 0 |
| darkcompact | native-clear | 76621 | 227 | [190, 226, 610, 420] | 0 |
| darkcompact | native | 76976 | 247 | [190, 226, 610, 420] | 0 |
| darkcompact | own-center | 212 | 23 | [210, 247, 533, 249] | 0 |
| darkcompact | own-right | 424 | 23 | [210, 247, 586, 249] | 0 |
| darkcompact | right | 708 | 23 | [199, 233, 601, 235] | 0 |
| darkcompact | rtl-right | 708 | 23 | [199, 233, 601, 235] | 0 |
| darkcompact | transparent-right | 708 | 23 | [199, 233, 601, 235] | 0 |
| light | center | 320 | 19 | [207, 237, 512, 239] | 0 |
| light | global | 74924 | 239 | [190, 230, 610, 420] | 0 |
| light | header-empty | 394 | 19 | [235, 237, 432, 239] | 0 |
| light | native-clear | 74924 | 239 | [190, 230, 610, 420] | 0 |
| light | native | 75243 | 255 | [190, 230, 610, 420] | 0 |
| light | own-center | 212 | 19 | [210, 247, 533, 249] | 0 |
| light | own-right | 424 | 19 | [210, 247, 586, 249] | 0 |
| light | right | 644 | 19 | [207, 237, 593, 239] | 0 |
| light | rtl-right | 644 | 19 | [207, 237, 593, 239] | 0 |
| light | transparent-right | 644 | 19 | [207, 237, 593, 239] | 0 |

不申请恢复micro、未覆盖OS/Browser/更广极值/完整资源出口及完整GroupBox/CT15，不改CT12/13/14批准或CT25/FW1，不替换官方依赖、不扩大像素容差，不授权提交。


清理后最终验收（2026-11-01）：共享锁空闲独占后，GroupBox既有19PASS/0FAIL/0Skip（用例581ms，命令26.97s）；NET8 Desktop Release 0警告/0错误（构建18.08s，命令18.25s）。23临时用例/3源19113字节、545取证、414driver/15publish及503原会话文件已删除，6最终文件和两个marker也清理，audit clean、git diff --check通过。三版164静态/cross0，68主恢复0；Global恢复4/7/9/9 max1保留。仅40明确静态及三Reserved兼容提案待限定批准，NeedsDecision仍3，原裁剪/低对比/平台/完整出口及CT15保留；CT12/13/14和CT25/FW1不改，不提交。


## 六包限定批准登记

2026-11-01 用户明确回复“仅批准六表现有条目及列明纠正，其余保留”（request_user_input_async / call_08c452ba85d7442ab817b3cf6c7dcce6 / item 0）。仅Flyouts48对及表列两项有效默认纠正、Collapse12对、Descriptions16对、GroupBox40对、Statistic12对、QRCode16对获得限定视觉批准。各表列明的裁剪、低对比、原失败、缺帧、恢复与持续态、平台及完整家族/CT15均保留，不授权提交；GroupBox三Reserved另按call_c8732844a7a841caacc4385b24e13bd3/0既有批准，不扩大。
