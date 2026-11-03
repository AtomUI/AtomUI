# CT15 本轮续作出口增量

当前新增[Core Icon几何快照修复](icon-retained-geometry-investigation.md)已完成工程/一次AOT/临时清理与78+7精确模块、新NET8零警告21.16s；[共享默认规则及20例证](icon-geometry-snapshot-visual-review.md)已获限定批准。Tag六恢复、Avatar两open恢复、三cross仍FAIL；原20.57s收据仅代表此前源码，完整CT15不通过。

当前新增[独立Dot显隐修复](badge-original-visibility-plan.md)也已完成：原88项12FAIL→88PASS、34聚焦、一次AOT160cross全0；所有临时清理后Badge32与新NET8零警告18.78s通过。[20对显示及原布局恢复](badge-visibility-visual-review.md)已获限定批准，RTL/Compact end等微差与完整CT15保持；原Count基线误判已纠正。

工作位置仍为原 custom-theme 目录与 feature/custom-theme；单 agent，无分支/worktree/提交/全量。137 metadata/274 Own 不变，当前Own决策状态256 ConsumerVerified、10 ReservedDocumented、7 ApprovedScopeReused、1 CompatibilityRetained；字段分类不等于完整家族关闭。

| 本轮生产增量 | 有效前证与结果 | 清理后门禁 |
| --- | --- | --- |
| Modal Overlay仅IsModal=True声明TabNavigation=Cycle | 原Tab逃逸、匹配宿主4项1PASS/3FAIL→7临时+276原模块283PASS；原生managed/AOT各261 | 276PASS，NET8零警告19.98s；见modal-native-input.md |
| BackTop仅attach时获取新Target.ScrollChanged | 两严格GC FAIL→3PASS/AOT21，原120/900/400阈值/450ms及Space回顶保持 | Float46+Carousel3共49PASS，NET8零警告28.37s；见float-button-target-retirement.md |
| Tour共享mask三绑定由当前逻辑owner持有并退出释放 | 两GC FAIL，公开Dispose因果成立→5PASS/AOT20；自审发现转交重复[1,0]通知，补有效RED→一次[0] | 末次Tour/placement29PASS，NET8零警告17.41s；见tour-retirement-audit.md。早期AOT20仅绑定增量证据，末次通知排序未冒称重新发布 |
| Drawer百分比OpenOn.SizeChanged只在attach获取 | 两严格GC FAIL→4PASS/AOT18，Right/Bottom百分比、旧目标退出/重挂/120viewport900extent保持 | Drawer37PASS，NET8零警告16.76s；见drawer-target-retirement.md |

GroupBox三Reserved只补XML说明；原先Core Icon/Spin端点与Badge Count退出修复保留。没有因Message图元探针去修改Core Icon或CT11动效。发现的显式Clip=false/实体Background组合只保留诊断，不改写已关闭CT12结论。

本轮额外补证均已清理：Carousel正常3000/700ms两圈及显隐/停止（3临时/AOT38）；Marquee150/450真实两圈；六展示对象Timer/CountUp/QRCode/Empty/Spin/Marquee严格Debug+Release退出（最终43）；ImagePreviewer single/group关闭移除/直接移除且caller图片/集合存活（Debug/Release各4，最终117）。这些不覆盖其余像素/平台边界。

调查归属：原FW2表3纯模型/24Timeline路径保持；用户call_cd2233cdfe1848fc889d777f91b52754:0仅新增批准timeline-original-part-visual-review.md所列4原P3-B组合/4Content0.7路径交FW2调查。非零仍FAIL，FW1、容差与完整CT15不改。Message原历史34路径已获call_8107eb29ed024cfbb7dd7fd40dec2aed:0有限调查批准；原native3497、旧批与36跨运行非零不在提案内。

当前持续工作：[出口台账](feedback-exit-ledger.md)为唯一实时状态索引；本轮FloatButton原四最终恢复完成真实保留命令定位与清理后46项，FR1–FR4仅获FW2独立调查归属，非零仍FAIL。Alert AD1–4与QRCode QR1–8也已获各表有限调查归属，非零仍FAIL；当前为Collapse原Global恢复的真实命令核对；不重复模型或AOT，不把归属批准当CT15完成。


2026-11-02后续两个实际自绘消费者缺陷：Dot只ShadowColor热更新已完成工程/40项/一次AOT96/64cross0/清理后32/NET8，并获24对限定批准；Card子项只安排位置的分隔线缺失已完成工程/28项/AOT240/120cross0/清理后20/最终NET8零警告20.57s，48对新视觉已批准。详细证据只见[Dot](badge-shadow-color-results.md)和[Card](card-action-geometry-results.md)。原未通过仍保留，不关闭CT15、不提交。当前转入Avatar原overflow完整历史，未启动新媒体/工程；源记录9417/9633及背景修订9876/9948/9956已定位，不能跳过修订恢复近似场景。

当前有限裁决：TR1–TR7、AC1/AC2及NM1–NM4已分别获FW2独立调查归属，非零仍FAIL，不认定共同根因；[CK1–CK8持续态口径](feedback-clock-criterion-review.md)仅获相位未配对／视觉无法判定分类，不算PASS或关闭CT15。原话与请求ID集中各权威表。[Dot明确隐藏组合](badge-hidden-flag-results.md)已完成四managed状态、临时清理后Badge32及新NET8零46.44s；18.78s NET8仅为之前源码历史。

本轮后续仅两项状态增量：Dot初始隐藏行为限定批准见其权威结果；CK9–CK13仅获输入未配对分类，不计PASS/FW2。Float原完整200/72仍68PASS4FAIL，12比较观测字段相同、16不同；唯一焦点对照未归零，停止该假设。401取证/144会话已清理，精确46通过，同源NET8零46.44s限定复用。完整CT15仍开放。

新增实际邻接功能反例：[Count零值target/Ribbon隐藏](badge-hidden-targets-results.md)4FAIL→4PASS，保留原Count归一、真实target点击/尺寸/隐藏重挂和Ribbon全文。8媒体已查看/9文件及4临时清理，精确Badge32/新NET8零17.67s，成员2/2/0；无新AOT，行为已获限定批准（badge-hidden-targets-results.md），完整CT15不关闭。
