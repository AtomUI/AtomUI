# CT15 Tag 限定视觉审查（2026-10-31）

状态：**表内26对及列明派生影响已获用户限定批准**。只申请下列26对旧新有意变化，不代表JIT/AOT微差或完整Tag/CT15通过，不授权提交。见[工程结果](tag-consumers-results.md)、[作者另步自审](tag-consumers-review.md)。

固定1000×1200、scale1、四算法内容面、正常motion、完整同坐标RGBA；700ms+12连续等帧、5s上限，无mask/crop/align或通用容差。数字为变化像素/max通道差。前5行各4对、top行4对、bottom行2对，共26对。

| 场景/有意行为 | Light | Dark | Compact | DarkCompact |
| --- | --- | --- | --- | --- |
| 9Own配置：close37实际生效；其余已有效的Own保持原角色 | 288382/max255 | 289901/max255 | 2916/max255 | 2930/max128 |
| 合法close0进入真实glyph，保留节点/关闭事件与完整文字 | 1612/max255 | 1618/max108 | 1480/max255 | 1486/max108 |
| Tag/Checkable/Group各自明确Global字体/边框/圆角/状态色/spacing | 408809/max255 | 415137/max255 | 396501/max255 | 402965/max255 |
| 原生Courier17/Orange/padding覆盖同时保留明确Global实际目标 | 405923/max255 | 412272/max255 | 393675/max255 | 400165/max255 |
| 清原生回明确Global值 | 408808/max255 | 415137/max255 | 396502/max255 | 402971/max255 |
| 独立滚动大矩阵top，原Own极值/Group100裁剪保持 | 277073/max255 | 278441/max255 | 2916/max255 | 2930/max128 |
| 大矩阵bottom：自然内容高度变化带来滚动条thumb变化；底部全部颜色/文本保留 | 112/max38 | 112/max197 | 0（不请求） | 0（不请求） |

## 兼容影响必须一并审查

TagCloseIconSize属于Tag Own，TagTheme的派生SelectTag也继承close入口。在明确Tag close37的大值配方中，普通明暗两种Select多选chip因glyph宽增长从一行换成两行；已有SelectTag固定高度/字号和原裁剪仍保持，过大glyph沿原高度被裁剪。Compact下宽度预算不同，未发生同样换行。close0则隐藏glyph而保持关闭节点/语义。上述是本表Own/0与大矩阵top对中可见的**新增配置派生影响**，不能只批准普通Tag而隐藏它；不修改Select源码、不放宽输入高度、不改写CT13既定批准结论。

其余8Own不因本包重定向：DefaultBg/DefaultColor只走原默认分支，SolidTextColor只走default Solid；命名palette/状态分类/HSL保持。36默认/type/key/slot相同，默认4帧旧新0；原生与四专用Style按实际目标匹配。自然高度/字号增长不通过缩小文字、减少66项矩阵、去掉Group100高度或裁剪解决。

## 不在批准范围

旧新164对133精确0、31非零；仅上述26对请求。其余5对排除：DarkCompact checked2/max3，以及large-restore Light277073/max255、Dark278431/max255、Compact2916/max255、DarkCompact2924/max128（含已知有意区域，但恢复存在微差，整帧不列请求）。

JIT/AOT164对159精确0，5对微差不批准：Compact checked3/max3、Dark global14/max3、DarkCompact global9/max1/global-clear9/max3/variant-filled9/max3。两版各44项主恢复仅22零、AOT23零，剩余5–36像素/max1–4；候选/AOT大矩阵DarkCompact恢复12/max1，旧另有Dark20/max1。数字与阶段详见工程结果，全部保留，不以旧版也存在或量小自动豁免。

三版4配方独立A/A及1DIP恢复全0，检错全非零（73503/73388/73471/73354、max255）。真实OS窗口/输入、Browser、未测极值及完整Tag/CT15仍未验证或未通过，Headless/Skia与macOS AOT进程不替代这些边界。


## 清理后精确出口

18临时case/3源25850字节、只读图扫描、1090取证及原外部driver/会话已清理后，精确Tag62+Select4共66PASS/0Skip（2s）。NET8 Release Desktop及Controls引用链0 warning/0 error，19.42s。最终3文件短期日志目录登记后删除；当前无Tag临时验证源/媒体/驱动或会话残留，无全量回归/提交。

84匹配验证、managed/AOT各120检查；164跨运行159零、旧新164中133零。26对已获限定批准（含Tag Close37对派生SelectTag的宽度/换行与原高度裁剪影响），其余整帧、恢复微差和完整Tag/CT15保留。18源/4Control成员0、纯整理指纹同，36默认/type/key/slot保持。正式6文档、8metadata/9Own、总表/自审同步；TagCloseIconSize按实际消费者裁为Active，NeedsDecision8→7，仅字段裁定，不等于视觉或家族批准。


限定批准登记：2026-10-31，用户明确回复“批准表内26对及列明派生影响，其余未通过与完整CT15保留”（request_user_input_async / call_5cea4f7e59a243dcab027c42807d9848 / item 0）。仅26对与列明SelectTag继承close37/0导致的换行/原高度裁剪影响获批准；恢复、5对跨运行微差、原CT13批准边界、OS/Browser及完整Tag/CT15保留，不授权提交。
