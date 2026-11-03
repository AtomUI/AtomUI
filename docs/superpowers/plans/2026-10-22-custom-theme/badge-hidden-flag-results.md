# CT15 Dot 显示准备尊重明确隐藏标志

本次为刚完成显示恢复修复的直接邻接状态风险：调用方BadgeIsVisible=false时，切换外层IsVisible不应显示指示器。使用原公开Dot/VisualLayerManager/Window API，完整原状态文字，target/standalone两模式，不改内部actor或样式。

临时两theory有效前证：target PASS、standalone FAIL（508ms），外层重新可见后BadgeIsVisible仍false但实际DotIndicator祖先链全部可见。根因是OnPropertyChanged对IsVisible/BadgeIsVisible两事件只读change.NewValue，忽略当前BadgeIsVisible。最小增量仅将`if (badgeIsVisible)`改为`if (badgeIsVisible && BadgeIsVisible)`；正常BadgeIsVisible=true的原显示准备分支及false关闭路径与上次已批准实现相同。

同两临时+既有Badge32合计34PASS/0Skip（796ms）只是初始门禁；追加target原对象仍在树和显式恢复断言后，两例均FAIL，不得用前一轮绿色覆盖。隐藏初始状态没有创建adorner，因而target也未挂载；在attach和隐藏时变更target两处保留真实target。显隐通知明确区分BadgeIsVisible事件与外层IsVisible事件，后者必须同时尊重当前BadgeIsVisible。该步为33PASS/1FAIL，独立模式显式恢复仍失败。

实际树诊断：owner=True/True、Panel与DotBadgeAdorner都已挂载，adorner逻辑父为DotBadge、视觉父为Panel，但均0×0且无模板子树。唯一变量在测试中调用owner.InvalidateMeasure后两例均PASS（1s），证明动态普通Control子项增删后没有失效测量。生产修正将InvalidateMeasure放在AttachChild/DetachChild实际成员变化处，测试移除此干预；同类Ribbon现有Prepare/Hide也明确失效测量。没有添加强制ApplyTemplate、延迟刷新或新标志。正进行两模式完整文字/真实尺寸/隐藏重挂恢复及既有Badge32验证。

本次无新API/字段/订阅/反射/模板/资源，但已超出最初仅布尔门禁估计，包含动态子项布局失效与隐藏target挂载。上次AOT只能按不受影响的原显示路径引用，不宣称本次组合已执行AOT。当前源NET8需实际重建，临时两case/一源清理后精确Badge32，单源成员审查仍必需；不自动全量或提交。原像素FAIL和FW1/FW2范围不变，20对既有批准不扩大为本次视觉批准或完整CT15。

最终临时验证为同一theory的四组合：standalone/target × motion false/true；保留原初始隐藏、owner显隐及显式恢复，追加隐藏时更换target、旧target离树/新target挂载、完整文字、非零DesiredSize、actor最终Opacity=1、隐藏离树重挂后的再次恢复。四例PASS/0Skip（2s），正常动效两例在真实窗口存活时等待400ms并排空Dispatcher，不停止产品clock。先前34项合批（2临时+原32）亦PASS/1s。单文件单控件成员门禁0问题。

新增只有布尔条件、现有target挂载与公开InvalidateMeasure调用；未新增动态发现、类型注册、资源或绑定，按风险不为这四个组合重复发布旧160图AOT。旧AOT仅说明未变显示准备机制；四组合本轮执行证据明确限于managed。临时四case/一源6015字节已删除，清理后精确Badge32与NET8正在执行。原像素/平台边界及完整CT15继续开放。

清理后最终收据：精确`FullyQualifiedName~AtomUI.Desktop.Controls.Tests.Badge.`实际重建32PASS/0FAIL/0Skip，792ms（命令13.05s）；Desktop NET8 Release实际重建0警告/0错误，46.44s（命令46.63s）。两命令由同一checkout非阻塞独占锁串行保护。新增临时源码0，无采样媒体或外部驱动。无新增永久测试、AOT发布或提交。作者自审确认不改Token、专用Style、默认文字/目标尺寸、原motion时长或原像素判据；只恢复明确隐藏语义、target布局和首次显示。完整CT15未通过。

随后仅补可见审查，复用同一managed测试宿主和初始隐藏操作，两模式各hidden/shown/reshown共六张320×200整帧，2PASS/439ms。完整六图均已打开：独立模式隐藏为空，首次及重挂显示均为红点与完整Original complete status；target为原48×48透明Border，红点沿原右上位置，窗口上缘已有裁剪保留，没有挪动或加padding掩盖。两组shown→reshown整RGBA均0/max0；这是本场景恢复证据，不是默认迁移/AOT/其他Badge微差通过。临时截图代码调用旧Bitmap.Save产生一条obsolete警告，仅在将删除的临时源；生产NET8仍零警告。新增媒体和临时两case须清理后再验精确32，产品源码未变，不重复NET8。

补充清理收据：官方audit clean/findings=[]，cleanup删除六媒体与manifest共7文件；临时两case/一源2674字节已删除。再次真实重建精确Badge32PASS/0Skip，903ms，无编译警告。生产源码保持与46.44s NET8收据相同，无外部会话或残留媒体。此后仅保留本权威文字记录。

## 限定批准登记

2026-11-02，用户选择“1. 仅批准上述隐藏/首次显示/target恢复行为，其余边界保留”（request_user_input_async / call_35a2dd2ea9c746b087faee73ab4b366f / item 0）。仅批准本问题列明的外层显隐尊重BadgeIsVisible=false、隐藏时保留原target、首次显式显示恢复模板/完整文字/尺寸。未新发AOT、原右上裁剪、其他像素FAIL、FW1/FW2与完整CT15保留，不授权提交。其他表只引用本登记，不扩大已有20对显示恢复批准。
