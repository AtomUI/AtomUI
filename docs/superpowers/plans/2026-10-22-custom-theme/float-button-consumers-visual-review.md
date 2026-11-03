# CT15 FloatButton 限定视觉审查（已获限定批准）

表内65对旧新非零且managed/AOT精确一致的静态变化，以及[两个legacy scalar offset的Reserved分类](float-button-token-ruling.md)已获用户限定批准；下列原申请条件与排除范围保持。不是完整FloatButton/CT15批准，不授权提交。137metadata/274Own分母保持；6 Active消费者与2已批准Reserved不能写作8 Active。CT11共享机制、CT12/13/14既定批准与CT25/FW1边界不重写。

## 条件与可见影响

同一完整历史/同坐标RGBA零容差、四算法；主窗口1100×900，Host800×600。保留原35/40窄宽按钮、Original text/first/second、原280×180滚动视口与1400高内容、受控IsOpen和原菜单方向。正常motion、真实Headless输入/命中链、静态至少750ms和12相同帧；定时帧单列，不混作静态通过。

六专用Style：两Icon Orange33、两Content Purple/Padding11,7、Group trigger Pink/list AliceBlue/Corner13/Padding17,11。Global FontSizeSM23在原窄宽使长正文逐字符换行，Group菜单越过视口上缘、沿原Window边界裁剪；这是明确列明的派生影响，没有删文字/改宽度/降低字号避开。Own主帧展示尺寸67和描述行高23；Icon31、PrimaryColor实际分支另由临时目标断言验证，主帧的Part图标33优先，不伪称其也可见。

BackTop原active但actor隐藏，回顶后残余Offset并压住后续500；当前正常显示、精确0/progress0及后续500/Caller绑定650通过。Group Padding clear位置残留22/34 DIP两反例已修；未知恢复微差没有作为修复成功豁免。

## 申请范围

每格为旧新不同像素数/max通道差；列入的65格跨运行均0。破折号格不申请。

| 静态状态及限定原因 | Light | Dark | Compact | DarkCompact |
| --- | --- | --- | --- | --- |
| BackTop active 正常可见 | —（跨运行3/max4，保留） | 5158/200 | 6933/255 | 4695/200 |
| 六专用Style实际命中 | 27606/255 | 22059/255 | 33146/255 | 26819/255 |
| Own尺寸67/描述行高23；图标仍Part33优先 | 26793/255 | 22051/255 | 33748/255 | 27603/255 |
| Global字体23/颜色/间距与原窄宽裁剪 | 116027/255 | 85984/255 | 106572/255 | 78431/255 |
| 原生Courier17优先及上述Part | 110173/255 | 82732/255 | 106482/255 | 78341/255 |
| 清原生回显式Global23 | 116027/255 | 85984/255 | 106572/255 | 78431/255 |
| Circle及真实BackTop模板 | 18563/233 | 11452/237 | 17449/233 | 12603/236 |
| 真实两按钮Count | 15736/255 | 11054/255 | 18745/255 | 13530/255 |
| 退休旧Count/唯一Dot | 15756/255 | 11080/255 | 18765/255 | 13556/255 |
| 实例颜色#123456 | 15812/255 | 11134/255 | 18821/255 | 13610/255 |
| 清实例色回原Error Token | 16120/255 | 11442/255 | 19129/255 | 13918/255 |
| 菜单Right位置与Part Padding | 15041/255 | —（跨运行3/max29，保留） | 16300/255 | 11523/255 |
| 清空原子项，保留Part Padding | 3909/45 | —（跨运行3/max29，保留） | 3682/44 | 1620/237 |
| 实际回顶精确0与原内容 | 15317/255 | 10636/255 | 18323/255 | 13115/255 |
| 真实Host BackTop active可见 | 3861/233 | 4771/233 | 2610/233 | 3263/233 |
| Host Own73/91与原布局 | 7051/233 | 9933/233 | 7051/233 | 9933/233 |
| 原生X81保留 | 7051/233 | 9933/233 | 7051/233 | 9933/233 |

## 未通过与未申请

- 三版各200帧；旧新20零/180非零（148静态/32定时），本表仅65静态，其余115旧新非零（83静态/32定时）不申请。全部16独立校准与4Host hidden旧新零。
- managed/AOT200对166零/34非零；168静态162零/6非零、32定时4零/28非零。6静态：Light default3/max4；Dark children-empty/menu-right/children-restore/menu-restore各3/max29，group-open6/max29。未指派CT25/FW1，未设容差。上述排除格与其它恢复状态均保留。
- 72运行检查：旧44PASS/28FAIL，候选/AOT各68PASS/4最终恢复FAIL。候选L/D/C/DC最终20/max4、11/max3、8/max1、2/max1；AOT17/max3、11/max3、8/max1、2/max1。[完整主往返账本](float-button-consumers-results.md)保留32个候选非零，不把其他检查通过当恢复通过。
- 正常定时show与scroll实际时刻全部记录；跨运行时相未通过，独立A/A/1DIP检错/恢复均有效。真实OS/Browser、完整自然极值/GC及未挂载时Target再替换的全部订阅出口无完整本批证据，完整FloatButton和CT15继续保留。

49临时用例/3源32518字节、777取证和537原会话文件已清理；最终46项精确模块PASS/0Skip、NET8零警告零错误，最终6个会话文件也已清理。表内65对与两字段Reserved分类已获限定批准，具体登记见末。原smoke/geometry/raw、fixed和扩展92项15失败及各无效候选保留文字，不被新图覆盖。


限定批准登记：2026-10-31，用户明确回复“批准65对及两项Reserved，其余保留”（request_user_input_async / call_e7be1ded2cd04cea8d90b50314dba528 / item 0）。仅FloatButton表内65对明确静态（含列明原窄宽换行/窗口上缘裁剪）与CircleBadgeOffset/SquareBadgeOffset两项Reserved兼容分类获批准；两字段仍无scalar消费者，不改写成Active/ConsumerVerified，不缩分母。NeedsDecision由5降为3，仅余GroupBox三字段。其余115旧新非零、34跨运行非零、恢复/平台/GC/极值及完整FloatButton/CT15保留，CT12/13/14既定批准和CT25/FW1不变，不授权提交。
