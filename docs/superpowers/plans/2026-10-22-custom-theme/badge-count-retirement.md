# CT15 Badge Count 退出保留后续定位（准备）

Spin 最终冻结取证期间仅只读核对，不修改 Badge、重做已批准48对或复跑已完成家族。保留 badge-consumers 的原始57所选56PASS/1 Count GC FAIL；保留 detached Adorner 时 Count owner 不回收，Dot/Ribbon 对照通过。此前真实强链为 CountBadgeAdorner → MotionActor.Presenter → Panel → inheritance child Border → CompositionVisual parent 若干层 → CountBadge；移出guard假设已撤回，不重复该补丁。

下一执行：用原行为/原独立可视根恢复最小临时探针，分离 attached/detached、保留/不保留adorner、真实Dispatcher/Skia提交与正常motion。创建/销毁放NoInlining边界，固定GC轮数后一次读取弱引用，避免IsAlive读取自身制造延长存活。保持实际原目标、数值/动画、视口及已批准Style；不关闭motion或清除私有composition字段使回收通过。

先复现并用真实公开树/绑定/动画membership证据证明因果，再决定产品最小修复；没有因果就保留失败，不自动归入/扩大CT25/FW1，不替换官方依赖。必要临时强链观测只读，不入产品。修复若可行须前失败/后通过、适当渲染/释放验证、临时删除与清理后精确Badge模块/NET8。此准备不称Count/Badge/CT15已通过，不授权提交。

当前恢复最小探针：NoInlining创建/退出、正常motion与真实Skia、固定五轮GC后一次读弱引用；Count保留/不保留×公开composition解除/不解除四对照，Dot/Ribbon保留正对照。仅临时观测内部runtime host，写入只使用公开CompositionContainerVisual.Children.Remove，未修改产品或私有字段。编译缺Controls namespace修正不计产品RED；实际六所选正在执行。

首六项5PASS/1FAIL：仅Count保留adorner且不作public解除时ownerAlive=true；不保留Count、public解除的Count、Dot/Ribbon对照均回收。公开composition成员在visual/logical parent均null后仍before=true/after=true，public Children.Remove返回true且使Count owner回收。没有写私有parent。首次motion未显式指定、退出后getter为false，故不提前外推正常motion；补显式IsMotionEnabled=true并在退出后正常推进35帧，再固定GC五轮对照中。

参考读取精确Avalonia12.1.4 tag的Visual.Composition：DetachFromCompositor只移除额外ChildCompositionVisual并清drawlist/mask、自身CompositionVisual置null，普通composition子成员同步由后续renderer处理。Count运行host为项目创建Panel，下一步仅在该真实membership退出时核对公开容器可否同步解除已退休子成员；不触及框架私有字段/依赖或FW1边界。必须实际motion对照支持后才改产品。

正常motion六对照再次5PASS/1FAIL：Count持有且不public解除失败，beforeMotion=true、退出前后public成员true；public解除后回收。最小产品修正只在Count项目自建runtime host的真实membership退出时，在视觉/逻辑移除后同步public CompositionContainerVisual.Children.Remove；没有私有字段写入、guard掩盖或新helper。38所选全PASS（6临时+32既有，10s）。追加正常hide/show三轮、重挂三轮、standalone/target往返三轮与64×32按钮左/中/右真实mouse及Space，41所选全PASS（9临时+32既有，28s）。Ribbon无motion API、VisualLayerManager与完整物理键参数的夹具编译纠正不计产品RED。

外部Release首组22帧/27检查26PASS/1新增target GC FAIL，standalone已回收；不提前关闭整个Badge。独立target探针2所选1PASS/1FAIL，仍只有持有adorner时ownerAlive；private只读强链为MotionActor.Presenter→Border._borderVisual→composition parents→_adornedVisual→CountBadge，UI adorned/parent/visual/focus均null。没有映射成FW1或写私有字段。

单变量public AdornerLayer.SetAdornedElement(null)在仍附着layer时执行，1PASS/2s、owner回收。精确12.1.4 AdornerLayer.UpdateAdornedElement只有在composition visual仍存在时才能更新AdornedVisual；旧顺序先移出视觉树导致public属性清空时错过旧呈现节点。第二最小修正仅把同一public SetAdornedElement移到Remove之前，保持动画/布局/内容与逻辑清理。35所选全PASS（3 target临时+32既有，5s），含target持有/不持有与早清对照。累计44不同用例（12临时+32既有），不是一次44批。先9后3临时分别1源8229/4673字节已删除。

最终两源冻结，外部旧新/AOT需重跑同完整可见/输入/GC历史。首22帧/target失败留作pre-target-fix账本，不复用为最终产物；原Badge57中GC1FAIL及新两次target FAIL历史均保留，当前不宣称完整Badge/CT15通过。


已实际查看四算法完整原新/AOT图面，无新增有意像素变化。12临时2源12902字节已删除；304取证文件（pre-target-fix22帧/最终264主帧/四拼图/TSV/manifest）经官方audit/cleanup删除，findings=[]/removed=true；外部driver411文件/publish15文件、原492会话共1087861353字节删除。最终两源冻结逐字一致，成员两源/两控件0问题。只剩独立最终门禁会话，精确Badge32与NET8完毕后删除，不保留强链扫描器、驱动或媒体。


清理后精确Badge真实重建32PASS/0FAIL/0Skip、759ms（命令30.58s）；Desktop NET8 Release覆盖Common零警告零错误19.66s（命令19.89s）。两次均先获得共享输出非阻塞独占锁，最终run.py/cleanup.json/module.log/net8.log四文件随后删除。正式实现/changelog、因果/结果/自审、两CSV及总表同步，无全量/提交/分支/worktree/子agent。仅这两个已定位保留路径在当前限定条件修正；原57中1GC FAIL及新原版8GC FAIL保留历史，其他Badge恢复/平台/极值与完整家族/CT15仍开放。
