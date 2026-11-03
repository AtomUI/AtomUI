# CT15 Collapse回收后续定位（2026-11-01）

原75所选73PASS/2GC FAIL、Debug反转与原/候选Release各1PASS/1FAIL账本均保留，不映射FW1。Count两条限定保留路径已另包定位修正，不据此推定Collapse相同原因。

新最小探针采用NoInlining创建/移除/退出，正常motion、原520宽/长header/120高视口900extent、固定五轮GC后单次读取弱引用；保留/不保留caller item×有/无scope四种条件。Debug四项全部PASS/10s；移出后parent/visual/containerOwner均null、同一content与viewport120保持。此新证据尚未复现旧失败，不作为原FAIL修正或家族关闭。

Release同条件重验中，再核对实际局部Global/Own和专用Semantic Style路径。当前未修改Collapse产品源码，保留此前所有未提交改动。不为猜测添加清理，不用调整Root高度/内容/禁用motion规避。

首启动命令误指临时路径在build前失败，已按marker实际canonical路径纠正，不计产品证据。后续判定/去留/清理与精确Collapse模块必须完成；本轮不自行跑全量、提交、新分支/worktree/agent。


本轮最终结论：八个不同NoInlining探针（基础保留/不保留×有/无scope四项，Global/Own+四专用Style保持/退出四项）分别在Debug/Release四批均通过，基础每批10s、定制每批11s；没有复现旧GC。原同一content、520宽、长header、120viewport/900extent和正常motion保持；退出parent/visual/containerOwner均null。无产品源码变化，不把新的严格通过倒写成旧75中2GC FAIL已修复，也不归框架或FW1。

8临时3源5529字节、14文件47042字节原会话已删除；无媒体/外部driver/AOT新增。清理后精确CollapseControl真实重建50PASS/0FAIL/0Skip、1s（命令16.44s），共享输出非阻塞独占锁。无产品变化，沿用相同最终源码已通过的Desktop NET8零警告19.66s证据，不无意义重跑。最终run.py/cleanup.json/module.log三文件会话删除，git diff --check通过。旧GC原触发/判据仍不足，保留未通过状态，当前仅收敛复现条件；完整Collapse/CT15及CT25/FW1不改写。

## 原关闭历史恢复与单变量定位

按新的迭代规则先恢复原场景，查回本会话原记录24416/24458/24485：800×600 AtomUI Window，单个简单header/body，IsMotionEnabled=false，原十五Own配置；Show后正常500ms DispatcherFrame，移除item后再500ms，Close后只有RunJobs，然后退出NoInlining helper并五轮GC。原先八探针是移除root后保留活窗口、520长header/120视口900extent及正常motion，不能代替这一关闭历史。

确切原关闭历史当前Debug首批1PASS/1FAIL，Release两项均FAIL。只增加弱Window只读观测，不改变生命周期；唯一操作变量是helper返回后、GC前是否正常泵500ms DispatcherFrame。Debug/Release均相同：无泵时保留/不保留caller item两项皆WindowAlive=true、OwnerAlive=true；有泵时两项皆WindowAlive=false、OwnerAlive=false。每配置四条件2PASS/2FAIL，原失败仍保留。没有检查WeakReference.Target或在GC循环条件中临时获取目标。

这排除了“这两条原失败证明caller item在正常关闭工作排空后保留旧Collapse”的推论；它们观察到关闭工作未排空时整个Window/Content仍存活。关闭后正常Dispatcher排空的两条当前退出判据通过，与八条活窗口移除root的独立退出证据互补；不宣称已查明Window短时保留的具体内部队列，也不把旧FAIL倒写为通过或归入FW1/FW2。产品源码无修改，不新增AOT发布或NET8重复构建；既有匹配产品源码的验证继续有效。临时源码删除后执行精确CollapseControl验收，清理结果另见下方最终收尾。

本轮临时4个参数条件/1源4053字节已删除；共享产物非阻塞独占锁下，清理后真实重建Debug/net10精确CollapseControl为50PASS/0FAIL/0Skip、1s。无永久新增测试、生产源码增量或媒体；外部日志/只读事实/marker已按本会话身份核对后清理。git diff --check及相关入口链接检查通过。原模块NET8/AOT不因纯探针重复发布，完整CT15仍按有限出口清单推进。
