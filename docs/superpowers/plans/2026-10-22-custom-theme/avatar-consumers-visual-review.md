# CT15 Avatar / AvatarGroup 限定视觉审查（2026-10-31）

当前为工程完成后拟申请的限定表，**表内48对已获用户限定批准**。只审查下列48对有意变化，不代表完整Avatar、平台、P3或CT15通过，不授权Git提交。结果与清理/精确验收见[工程结果](avatar-consumers-results.md)，作者另步自审见[自审记录](avatar-consumers-review.md)。

固定900×700、RenderScaling1、Desktop Window中的4算法内容scope；内容承载面读取配方ColorBgContainer。正常motion、真实Skia、最少700ms加12等帧、5s上限，完整同坐标RGBA，无mask/crop/align/容差。四列各为一对旧新帧，数值为变化像素/max通道差；每行4对，共12行48对。

| 场景与有意行为 | Light | Dark | Compact | DarkCompact |
| --- | --- | --- | --- | --- |
| Own Middle47/Icon23；动态Circle半径随宽 | 1948/max255 | 1963/max241 | 2144/max255 | 2159/max255 |
| Own Large61/Icon31；动态Circle半径随宽 | 2311/max255 | 2321/max241 | 2507/max255 | 2517/max255 |
| Own Small29/Icon17；动态Circle半径随宽 | 1549/max255 | 1558/max241 | 1785/max255 | 1794/max255 |
| Avatar Menlo23/Square9，Group边框3 | 1405/max111 | 1512/max213 | 1227/max106 | 1293/max177 |
| 原生字体/宽64保留；有效Square9/Group3和Circle32 | 2200/max111 | 2302/max213 | 2074/max106 | 2144/max177 |
| 清原生值回明确Global字体/圆角/边框 | 1405/max111 | 1512/max213 | 1227/max106 | 1293/max177 |
| Custom48的Circle半径24 | 910/max235 | 913/max210 | 1002/max235 | 1005/max210 |
| 同实例Custom48→72，box/icon/图片裁剪与排版同步 | 11729/max235 | 11826/max235 | 10876/max235 | 10977/max235 |
| Group投射Custom72，子项Circle36 | 26465/max235 | 26717/max235 | 27160/max235 | 27434/max235 |
| 同实例Group72→96，子项尺寸/半径同步 | 37164/max235 | 37412/max235 | 37895/max235 | 38159/max235 |
| 完整长Unicode文本Custom0→160，恢复可见box/原Viewbox | 28904/max235 | 28991/max235 | 26991/max235 | 27072/max235 |
| 正在打开的overflow热Own：两个Avatar47与真实颜色/spacing19 | 15071/max255 | 11348/max255 | 13872/max255 | 10933/max255 |

默认4对及合法Own0、清空Own/Global、Native字体入口、RTL/fold/重挂/模板与图片恢复的未列帧不请求变更默认。44默认/type/key/slot保持；原文字root与真实Text均14，未修改三TextFontSize的现有Icon角色。公开root Style与原生属性仍需优先。图片/完整文字/Gap/Viewbox/Clip不缩减，借用图不dispose。

## 排除项

当前旧新136对中85对精确0、48对为本表有意变化，另3对关闭后的微差不在请求内：Light closed80/max3、Compact closed53/max12、DarkCompact closed44/max13。它们未经批准且未通过，保留原值；不能用本表批准豁免。AOT与managed136对134精确0，另两对closed微差排除；三版校准通过。工程39项、清理后精确21项/NET8零警告零错误及全部临时清理完成，见结果文末。

真实OS窗口/chrome/输入、Browser、Popup过程时相、未执行的自然极值和完整家族/CT15继续保留。首轮浅宿主暗scope取证背景不足，整批单列保留历史，最终表只据承载面修正后相同完整历史回放；不选择旧批中更小的差异数字。


最终NativeAOT主76/overflow20/校准4均0FAIL，与managed136对中134精确0；Light closed80/max3、Dark closed49/max2不在批准内。三版各36主业务恢复全0。补充popup往返单列：每版open-default→open-restore普通明12/max8、普通暗12/max7非零，两Compact为0；三版default→closed依次为旧[92/max8,61/max7,53/max12,0]、候选[12/max8,61/max7,0,44/max13]、AOT[92/max8,12/max7,0,44/max13]（顺序Light/Dark/Compact/DarkCompact）。这些恢复微差全部未通过，未设新容差；同版本已出现的值不自动豁免。三版各4配方独立A/A与1DIP恢复全0，检错数字与max完全相同。


限定批准登记：2026-10-31，用户明确回复“批准表内48对，其余未通过与完整CT15保留”（request_user_input_async / call_7b292d7ef454420fb8fc0603ac3e3f4c / item 0）。仅本表48对有意变化获批准；closed/往返微差、OS/Browser/Popup过程时相及完整Avatar/CT15继续保留，不授权提交。
