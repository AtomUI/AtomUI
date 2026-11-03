# CT15 Message 检测/业务恢复继续核对

复用P1已完成的51项、有效Own/Global/字体/Part/关闭与GC证据，不新增同义消费者测试，不重复共享Stack或16/8归属。当前只针对P1独立1DIP恢复及主业务恢复非零（见message-consumers-results.md）取新的独立定位证据。

原分支/目录单agent，无分支/worktree/提交/全量。原920×700、四Information/Success/Warning/Error、零时长、原文本/图标/Shadow/Padding、normal motion保持。四算法，每个独立场景保留同manager/四card；entry只比较正常入场后A/A与完整背景重绘，shift执行1DIP/clear，Own与Global分别A-B-A且实际值/几何/实例恢复。完整背景重绘仅作为最后单变量诊断，不污染初始基线，不移除业务步骤或直接将旧非零归新FW2。

复用真实Skia/normal Dispatcher、>=750ms及12连续完整帧、零通用容差；采集原值与恢复值，观察差异位置。资源回收/键盘/关闭已由P1和原生P2提供，继续复用不重复研发。临时验证完成后清理与精确模块验收，未取得因果证据不改产品，也不借新最小场景倒写旧失败。


## 当前实际结果

四算法×entry/shift/Own/Global共16个独立场景，920×700、四原类型/全文/图标、正常motion保留。每组A/A0，shift/Own/Global的实际变化可检出；四card原引用、全部采集的有效值、全树geometry/transform、颜色/字体/padding在恢复后相同。entry未修改状态；最后只重绘真实背景Border作诊断。

临时四theory均因最后严格零判据FAIL，非数值/布局/实例断言失败。不是重新研发P1消费者/GC。当前表为恢复/初始完整重绘差异，不把完整重绘当成产品修复：

| 配方 | entry，正常初始→完整重绘 | shift恢复 | Own恢复 | Global恢复 |
| --- | --- | --- | --- | --- |
| Default，测试宿主 | 29/max15 | 29/max15 | 31/max15 | 32/max15 |
| Dark，测试宿主 | 9/max12 | 9/max12 | 11/max12 | 24/max12 |
| Compact，测试宿主 | 0 | 44/max29 | 36/max30 | 38/max30 |
| Dark+Compact，测试宿主 | 28/max23 | 28/max23 | 18/max23 | 28/max23 |

全部16场景最后完整背景重绘图，在各配方内四条不同历史之间彼此精确0；不能因此把原始正常初始/局部恢复非零当通过。差异图已查看，位于卡片/glyph细边缘；不按“噪声”放行。

## 独立驱动与AOT

从同一临时场景生成独立控制台驱动（明确使用自有小型断言，无xUnit/Shouldly依赖），只以源码ProjectReference引用Desktop Controls、官方Headless12.1.4和同一Skia/normal frame pump。managed/AOT各224检查中220PASS/4严格零恢复FAIL，均按FAIL返回2；所有实际值/几何/原引用/Own与Global命中断言通过。16场景/76帧完整，无缺帧。

76个managed/AOT完整RGBA对：66零、10非零；16张root-redraw全部cross0，每配方独立四历史的root-redraw也全部0。10个cross非零明确为：L global-restored2/max15、own-custom15/max23、own-restored2/max15；D global-restored16/max2、own-custom17/max25；C global-restored4/max3、own-custom31/max23；DC global-restored18/max12、own-custom21/max25、own-restored2/max12。其余相同，零通用容差、无后处理裁剪/遮罩/对齐。

驱动初次/var与/private/var别名导致7个MSB3202引用路径错误；改为真实canonical目录并清理专用obj解决，不修改仓库引用。初App缺partial导致ATOMUIREG015，改partial后零warning构建，不用suppression。AOT发布零IL/CS warning，五条既有Homebrew最低OS linker warning保留。原P1失效88帧、80主帧35旧新/41cross零、检测/业务恢复及原native3497/max2全部原账保留，不以本新16场景替代其历史。

目前只取得新的完整重绘/局部恢复差异定位证据，Message不在已批准FW2原表范围，尚未调整归属。临时源/媒体/独立工程清理及清理后精确Message验收继续。

AOT产物：21364288字节，SHA256 f2ac3cf425891e256bbcbb198002406115f8f0c01f72011545559d1fb5405872。


## 清理后验收与作者自审

2026-11-01 Message恢复定位：四配方×entry/shift/Own/Global的16独立场景，实际值/几何/四card身份恢复、AA0；managed/AOT各224检查220PASS/4严格零恢复FAIL，76帧66cross零/10非零，16完整背景重绘cross全0且每配方四条历史的重绘图彼此0。原正常初始/局部恢复仍非零，不把重绘当修复；新Message路径尚不在已批准FW2范围，原P1与native3497/max2均保留。4临时case/1源9518字节、331取证、427会话1068266516字节已清理，audit clean；最终精确Message Semantic/managerPadding17PASS/0Skip/1s，产品未变复用NET8零警告21.10s。见message-restore-audit.md，完整Message/CT15未关闭，不提交。

清理后确实重建测试程序集，精确filter为MessageSemanticPartTests与FeedbackManagerPaddingTests，不把旧P1的29项当作本轮17项；无新增永久测试、无全量。所有本轮进程退出，外部驱动/两种运行取证均删除，最终单run.py/marker也清理。作者另步自审，非独立审查；没有将旧或新原始FAIL倒写为通过，没有调整FW2归属或通用容差。
