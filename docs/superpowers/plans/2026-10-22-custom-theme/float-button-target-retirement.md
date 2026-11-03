# CT15 BackTop Target 退出核对

沿原13metadata/8Own、65视觉与两Reserved批准，不重复CT11动效。当前源码ConfigureTarget在未attach或detach后赋新Target时仍订阅ScrollChanged；OnDetached仅清当时Target，因此活着的ScrollViewer可能保留从未挂载/已退出的按钮。先用保留原目标窗口、120高viewport/900extent/可见滚动条的NoInlining弱引用反例和普通attach/detach正对照验证。未取得有效RED不改产品。

AC：未挂载/脱离后公开替换Target不得建立外部长期订阅；已挂载替换必须从旧目标退出且新目标仍更新进度/active；重新挂载必须恢复当前目标并正常滚动/回顶，保留默认450ms动画、400阈值、全文与scroll约束。临时测试在Temporary/CT15BackTopTarget，源码/媒体仓库外；串行独占共享锁。修复若成立只限定订阅的membership，不改默认/API/Token/布局，不用弱事件掩盖归属。


有效RED：三用例1PASS/2FAIL，11.4825s；未挂载赋Target与detach后替换Target两条NoInlining回收均仍存活，已attach换目标/旧目标不再更新/reattach进度正对照PASS。唯一生产改动为ConfigureTarget的新目标订阅加_isAttachedToVisualTree条件；UpdateTargetState仍立即读取，attach原有去重订阅保持，不新增字段/资源/弱事件/几何/Token。首次GREEN编译失败因我在编译读取文件清单后删除另一Carousel临时源，CS2001为验证编排错误；未报为产品失败或通过。后续清理只在编译退出后执行。


修复后三临时用例3PASS/0Skip，10.8843s；两条严格NoInlining回收、公开目标切换/旧目标退出/重新挂载进度与Space实际点击一次、默认450ms回顶到精确0后再次滚动恢复均通过。临时输入新增的两次API/namespace编译错误单列为驱动诊断；不作为产品RED。成员审查1源/2控件/0问题。正在同源AOT与最终清理门禁。


2026-11-01 BackTop Target退出：两条NoInlining有效GC RED，唯一ConfigureTarget订阅条件限定attach后，三临时用例PASS/10.8843s，同源AOT21检查PASS/exit0；原120viewport900extent/450ms/400阈值/全文/滚动条、换目标/旧目标退出/reattach/Space回顶和之后可滚动保持。AOT46245192字节SHA256 bcacc57aff05811cd365287dc12aae28b662d9a175c31e7b281a9164c68c325a，零IL/CS、五既有Homebrew linker warning。3临时/1源5170字节及309会话757297643字节已清理，无新增媒体；清理后Float46+Carousel3共49PASS/0Skip/6.9191s；NET8最终源码构建中。仅修正这两条资源获取路径，不关闭原恢复/其他GC/平台/极值与完整CT15。见float-button-target-retirement.md，不提交。


清理后最终门禁：Float46与Carousel3精确共49PASS/0Skip/6.9191s；Desktop NET8 Release最终源码零警告零错误28.37s。全程共享run.lock非阻塞独占，无全量。成员1源/2控件0问题，确切原未提交快照到当前仅一个条件替换；注册/初始化/API/Token/布局均无增量。作者另步自审，非独立审查；订阅获取/退出与再次挂载配对，未改原动效模型或已批准家族。两条具体泄漏已修正，其他保留项不倒写。最终门禁helper/marker随读取清理，不自动提交。
