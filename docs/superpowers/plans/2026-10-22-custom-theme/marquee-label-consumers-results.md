# CT15 MarqueeLabel 执行结果（2026-11-01）

原feature/custom-theme目录单agent执行，不建分支/worktree、不提交、不全量，保留此前所有未提交改动及批准边界。

输入2metadata/2Own，原CycleSpace200、DefaultSpeed150，四算法8条default/type/key/slot冻结且迁移后精确同。没有公开Semantic manifest，未虚构Part；Base为Avalonia.TextBlock派生，保持原文本、测量、Clip与绘制。CT11 OnlyIfVisible机制与其既有限定证据复用，未把原4倍时长公式改成新速度语义。

首轮12所选（11临时+既有Marquee_Animation_Pauses_When_Effectively_Invisible 1）8PASS/4有效FAIL：Marquee Global FontFamily/FontSize/ColorText未进入实际文字，普通owner Style的CycleSpace17/MoveSpeed61被代码Template优先级绑定挡住。两个Own与Local原生值已有实际消费，不误报为未接线。首次临时编译manager可空检查已补，不计产品RED。

原C#Root资源绑定可由ControlTheme等价表达，按强约束迁入新MarqueeLabelTheme.axaml，默认Own与可选Root字体/前景声明式解析，真实InheritanceParent桥接，不改Base动画/渲染。新资产沿既有同一Control注册导出，未新增metadata或Own identity；两Own迁V2R1。修后12PASS（4s），8默认精确同，原Local与普通Style优先级通过。成员初始3源/1含注册属性类型0问题。

正在真实Skia帧推进的运行态反例：文本或字体变动后动画终点应与当前文本宽度+间距一致，热Own变化旧动画完成不能清除当前动画，退出CTS退休。此阶段未提前记通过；正常旧新/AOT、清理及最终精确模块/NET8、正式文档/CSV/总表和限定视觉仍待，完整CT15不关闭。

真实帧推进精确3例均有效RED：变更文字后终点仍-1070.576，当前应-1945.044；字号变更后终点仍-1070.576，当前应-1630.232；热Own后旧completion将新_animation字段清为null。旧任务只在animation与CTS实例仍相同才清字段，cleanup同步退休running；Measure始终按当前TextLayout.Width核对并在宽度变化时重建，悬停不启动，拟合时复位缓存。原4倍时长、OnlyIfVisible、绘制/裁剪/文本保持。

修后15所选全PASS（14临时+既有1，5s），同一热Own当前animation不被清除且CTS退出退休。下一用真实InputHitTest/MouseMove验证220×28原视口左/中/右三个位置悬停、原可见offset保持、悬停字体重排与离开恢复，不通过直接写IsPointerOver或改变背景/尺寸/裁剪绕过。

真实hover三例（x=1/110/219）均PASS（2s）：InputHitTest确为同一owner，Bounds220×28、ClipToBounds=True、完整长文本保持；IsPointerOver后running false且实际PivotOffset精确等于保存的可见offset，字体23重排仍暂停，移至窗口外区后恢复running。没有加背景或改尺寸/裁剪/事件路由规避命中。共18不同用例（17临时+既有1），原位暂停本来可维持，未据猜测添加新的phase改写；宽度重建的hover guard保留该合同。正常Skia/AOT、清理与最终门槛仍待。

参数边界4PASS（2s）：CycleSpace0/31/999、正速度77/1/320进入实际duration并clear，原4×max(1,distance/speed×1000)精确保持；0速度抛OverflowException且clear恢复150。持锁只回放原AbstractMarqueeLabel验证同一0速度用例PASS（962ms），finally恢复候选，确认原不支持边界，不夹值或合成停止态。

完整22所选全PASS（21临时+既有1，10s），四算法8默认/type/key/slot精确同。此处复跑是边界/基类修复后的完整模块聚焦，不运行其他RecurringVisualWork用例或全量。建立MarqueeLabel正式overview/implementation/token/changelog四文档及分类入口链接，无公开Semantic manifest故不伪造semantic-part.md；原独立Gallery页面缺失如实记录，Alert组合归属保持。正在最终成员审查及正常Skia/AOT准备，未提前填写清理与视觉门槛。

最终成员3源/1含注册属性类型0问题，冻结4文件（含新增Theme）；原回放须删除新增Theme再构建，finally恢复，不能把新Theme混入旧版。初始驱动构建零警告（23.48s），Light43帧/24属性检查PASS但首张原图主标签不可见：第二scope空Border也绑了不透明背景，挡住第一scope。该43帧全部留账不作视觉通过，仅此Light一组，无其他算法/旧新/AOT。

修正只移除空第二宿主的背景投影，主Grid已在外层同算法scope持有正确背景；不改产品、原文本、尺寸/裁剪/动画或容差。增加真实InputHitTest必须为owner的断言，先重采可见Light再继续，不以属性通过接受空白图。此前22功能用例与8默认/3-1-0成员证据独立有效。

可见驱动重建0警告/0错误（21.28s）；candidate2/Light fresh完整43帧/25检查PASS，默认原图确认完整文字可见，实际命中owner与背景断言通过。每算法28静态（24主状态+4独立cal）、15正常滚动/hover/显隐/RTL持续态；后者不暂停取得静态通过。父宿主在同scope提供原生字体/前景，默认继承合同明确；第二空宿主不再绘制遮挡背景。正在其余三算法，随后old2原回放删除新增Theme、最终AOT，同一新驱动完整历史不拼首批。

有效candidate2四算法各43帧/25检查PASS，共172帧/100检查，112静态与60持续态分账。44主恢复全0；真实背景/owner命中、完整短文、普通Style优先级、Scope迁移及正常长文本参数/文字/字体更新、220×28 clip、三处hover与恢复/重挂均通过。old2持锁删除新Theme再按三原源构建、finally恢复四候选源，正在同完整历史回放；不据此预记旧新像素或AOT。


可见old2四算法各43帧/25检查，各6FAIL，共172帧/100检查76PASS24FAIL，无缺帧。失败为global-fonts/global-colour/regular-native-style/regular-native-style-own/current-scope/hover-font-stopped，完整保留。有效RGBA旧新172对116零/56非零，其中112静态104零/8明确Global及scope文字变化，48持续态非零另账；四default零。两版44主恢复全0，独立A/A/restore各4零，1DIP1284/1282/1284/1282检出（max224/199/224/199）。四contact sheet已查看，短文600×28完整、长文220×28原裁剪与暗显式低对比列入限定表；当前AOT发布中，未预记跨运行/清理门槛。

AOT确切publish/AtomUI.Desktop.Controls.Tests为46476592字节，SHA256 110d4d8d46bd3119655f6ec1f6ae05c5c026a8e908a6a23d0195b05a79943411，官方Avalonia12.1.4未改。发布0 IL/CS/注册诊断，5条已知Homebrew/macOS26 dylib与target12链接警告单列；aot2从该路径四fresh进程运行中，未提前记通过。Spin只读依赖准备另文，未在采样中修改其他产品源或共享产物。


最终aot2四算法各43帧/25检查PASS，共172帧/100检查；112静态cross全0，表内8对也全0。完整172对117零/55持续态非零，60持续态5零/55非零，不暂停默认滚动或放宽容差。三版各44主恢复全0，独立A/A/restore各4零，1DIP全部检出；三版112静态最短759/761/760ms，至少12次raw连续相同。

保留cross持续态 L：live-125 1433/max125；live-font 1082/max71；live-hover-centre 750/max71；live-hover-font 750/max71；live-hover-left 1082/max71；live-hover-right 750/max71；live-initial 110/max64；live-own 318/max68；live-reattach 709/max71；live-resumed 1025/max71；live-rtl-clear 256/max71；live-rtl 341/max71；live-shown 1485/max182；live-text 807/max73。

保留cross持续态 D：live-125 843/max77；live-font 348/max75；live-hover-centre 54/max76；live-hover-font 54/max76；live-hover-left 348/max75；live-hover-right 54/max76；live-initial 623/max74；live-own 930/max77；live-reattach 556/max79；live-resumed 1247/max79；live-rtl-clear 1737/max199；live-rtl 1644/max199；live-shown 1507/max182；live-text 790/max77。

保留cross持续态 C：live-125 1304/max130；live-font 581/max71；live-hover-centre 371/max69；live-hover-font 371/max69；live-hover-left 581/max71；live-hover-right 371/max69；live-initial 549/max73；live-own 281/max67；live-reattach 794/max71；live-resumed 167/max68；live-rtl-clear 730/max71；live-rtl 470/max69；live-shown 1163/max71。

保留cross持续态 DC：live-125 1573/max199；live-font 892/max75；live-hover-centre 571/max76；live-hover-font 571/max76；live-hover-left 892/max75；live-hover-right 571/max76；live-initial 1491/max178；live-own 676/max77；live-reattach 1122/max79；live-resumed 484/max79；live-rtl-clear 809/max79；live-rtl 595/max76；live-shown 479/max79；live-text 288/max75。

四候选源逐字保持；AOT正确暗默认/Global与可见长文裁剪/hover原图已查看，官方evidence audit clean，正式4文档相对链接无缺失。首遮挡43帧+有效三版516帧+4contact sheets+manifest共564取证待清理；随后精确Marquee既有1与Alert9、NET8，不预记门槛通过。平台/全部周期/极值/GC与完整CT15保持。


判断后清理完成：21临时用例、1源12070字节删除；官方audit clean后cleanup删除564取证（43遮挡帧+516有效三版帧+4contact sheets+manifest）。driver408/publish15文件及原会话500文件/1379345925字节删除，原marker清理。零新增永久测试；最终Marquee1+Alert9/NET8在独立小会话完成后同样删除。


清理后精确模块：Debug/net10筛选Marquee有效显隐1项与Alert9项，10PASS/0FAIL/0Skip，637ms（命令36.86s）；NET8 Release桌面构建0警告0错误，21.98s（命令22.33s）。两次均先获得共享产物非阻塞独占锁。最终会话run.py/cleanup.json/module.log/net8.log共4文件随后清理；无新增永久测试、不运行全量、不提交。


限定批准登记：2026-11-01，用户明确回复“批准表内8对，其余边界保留”（request_user_input_async / call_492fb90497c742dfbc2801dac1500c71 / item 0）。仅MarqueeLabel表内8对Global/跨scope字体颜色变化获批准；48旧新/55跨运行持续态非零、首批43帧排除、原4倍时长/0速度异常、裁剪、平台/GC/完整CT15继续保留，不授权提交。


2026-11-01 Marquee正常周期补证：原220×28/Clip/全文及4×时长保持，default150/native450两组2临时PASS；AOT2874检查PASS/exit0，各两次实际回绕与offset有限/范围、隐藏600ms冻结/显示恢复通过。default实测周期13053.818ms，AOT13047.828ms；native450实测4374.079ms/AOT4373.230ms，期望13053.333/4351.111ms，误差界来自实际采样间隔，非像素容差。不冻结clock或作跨时相RGBA0声明。2临时/1源4618字节、16取证、303会话448595997字节已清理audit clean；最终精确原Marquee显隐模块1PASS/0Skip25ms（本轮不是旧10项），产品未变复用NET8零警告19.98s。原48/55持续态非零、4倍语义/0速度异常、平台/GC与完整CT15保留，见marquee-cycle-audit.md，不提交。
