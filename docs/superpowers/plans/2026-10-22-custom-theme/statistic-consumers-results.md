# CT15 Statistic 执行结果（2026-11-01）

执行地点为既有feature/custom-theme目录，单agent，不建分支/worktree、不提交、不全量，保留此前改动和批准边界。

原运行注册已验证：AbstractStatistic / Statistic / StatisticCountUp / TimerStatistic均各有Global身份，Own数量分别0/2/0/0；没有把三个Global-only类型伪造为无身份，也不新增Own fragment。原四算法共8条默认/type/key/slot：普通Title14/Content24，Compact按原全局派生，完整导出精确保持。共享Own仍由Statistic拥有，Timer/CountUp原资源引用共享这两字段。

首轮19所选（14临时+5既有）12PASS/7有效FAIL，失败为Statistic与Timer局部Global FontFamily各1、原title/content颜色各1、Standalone CountUp自己的Global Family/Heading各1及Statistic原Root/Content spacing1。原两个Own对Statistic/Timer与CountUp的真实消费通过，不重复或改写成失效。

成员8源/4含注册属性类型7问题→0，纯整理前后成员token指纹、注册序列与全部static/instance字段顺序精确相同：Statistic三运行字段移出属性区，CountUp public AnimatingValue注册/包装归位，Timer保留原public Formatter字段形态并移出属性区。没有把field改成property或改写已有计时生命周期。

V2R1保留两Own原Global.FontSize/FontSizeHeading3依赖；AbstractStatistic与CountUp复用真实InheritanceParent可选字体，Title/Content heading按实际owner接入Global，Statistic两种spacing角色转Owner。CountUp保留原Own ContentFontSize默认和Transitions（Style Setter中），未暂停或替换官方动效。修后首轮19PASS/0Skip（8s），四算法8默认与4组身份导出精确相同。

增强验证中：CountUp自己的明确Global字号、六专用Semantic Style真实命中/clear与专用类/Setter/no-code-fallback，及Statistic原生Value输入、Timer原生Prefix输入的重模板焦点/键盘。当前未提前计为通过；正常Skia/AOT、清理和最终模块/NET8尚待。137metadata/274Own、其他未通过与完整CT15不关闭。

增强24所选21PASS/3有效FAIL：CountUp明确Global FontSize37仍24、Statistic原生Value与Timer原生Prefix输入重模板后focus均null。六专用Style命中/clear与静态形态通过。CountUp用原Own ContentFontSize作为无明确配置的fallback、明确自身Global才覆盖，复用三输入字号converter；AbstractStatistic复用已验证的弱focus/generation/同TopLevel/后代/current focus空守卫，卸载取消，不强持有内容、不覆盖后来用户focus，不改计时逻辑。修后24PASS（12s），同默认/身份导出保持。

完整增强32所选31PASS/1FAIL（22s）：唯一失败为Own Title/Content字号0触发官方Avalonia12.1.4文本layout的ArgumentOutOfRangeException，消息Invalid FontRenderingEmSize、ActualValue0。独立原源码回放只跑零/正极值两例，同样1PASS/1FAIL且零异常完全一致；finally已恢复候选源，不替换官方依赖、不夹值、不把它当新产品RED修复。字号合同为正值，零按原边界拒绝，临时验证调整为明确检查同异常与清除恢复；正极值1/97已通过，原始失败留账。原生标题29/Purple与Icon27×31/Orange优先、格式/分隔符/自定义Formatter/原生内容优先、后来用户focus、Timer显隐暂停/结束事件一次/退挂timer退休重建其余31均通过。

零异常改为原合同断言后32PASS（23s），包括异常后清除恢复24。为正常渲染检查最终重挂业务，继续加入两项结束态，均有效RED：同一已结束倒计时在RefreshDuration改变或detach/reattach后变为正计时，RemainingTime由0增大。根因是BuildTimer每次重建都按此刻Value>Now重分类。新增target-classification dirty位只在实际Value变更时置位；重建timer保留原方向/完成标记，显隐与旧clock退休机制保持。新Value为过去仍按原正计时，完成事件不重复。

修后34所选全PASS（29临时+5既有，26s），四算法8默认/type/key/slot及4组身份导出精确保持。新增target dirty位是实例运行状态，不进Token；未改原public Formatter字段或事件签名，也未替换官方动效/依赖。接下来成员最终审查、源冻结与正常Skia旧新/AOT，不提前填写视觉/清理门槛。

新增接口在protected override前的5个位置问题已纯整理到生命周期之后。该整理前后语义token指纹、注册及全部static/instance字段序列精确相同，最终成员8源/4含注册属性类型0问题。冻结13文件、实际8文件diff；驱动最终构建0警告/0错误（16.41s）。

候选正常Skia四算法各53帧/35检查全PASS，共212帧/140检查，32静态与21持续态每算法分账。静态有三实际身份同窗（Statistic固定值、Timer真实到期00:00、独立CountUp收敛），覆盖Own31/43、正极值1/97、Global/native/六专用Style及clear、原格式/业务、显隐/RTL/重模板和重挂。两个独立原生输入（Statistic.Content与Timer.Prefix）各6帧/算法，真实产品TextBox，不冻结caret；另9帧默认正常计数/计时、hidden/resume、模板、重挂及Skeleton loading持续态。当前Global Motion使用已有TokenTransitions按实际owner解析的共享机制，不重做CT11；真实CountUp值推进与timer推进/显隐暂停通过，不将某帧相位当完整周期证明。

Light默认/Global/正极值/原生输入全图已查看。窗口720×680与控件宽520保持，97字号的原字符串/单位沿原窗口边界裁剪，字号1标题难以辨认，未加宽或缩写业务内容；输入原220宽viewport内文字按原滚动/裁剪保持，完整Text/caret另有断言。正在旧版同历史及AOT，默认像素、恢复、跨运行与限定表尚待，不预记通过。


旧版同完整历史四算法各53帧/35检查，各11FAIL，共212帧/140检查96PASS44FAIL，无缺帧。有效失败为Global fonts/count-size/colours/spacing、结束态final-business及两原生输入各focus-retained/keyboard/identity；原样保留，不以candidate补旧业务。RGBA旧新212对128零84非零，其中128静态116零12明确变化；72持续态非零另账。四默认均0；候选52主恢复0，旧版48零/4reattach 280/279/280/279像素（max224/199/224/199），A/A及restore各4零，1DIP1425/1427/1425/1427检出。四contact sheet、暗Global/native-global、Light六Part及暗reattach全图已查看，原裁剪/显式低对比列限定表。

CountUp真实Global动效消费独立临时1PASS（1s）：MotionDurationVerySlow=00:00:01到实际DoubleTransition.Duration，Property仍AnimatingValue，clear恢复原duration；仅复用CT11机制，无新产品修改。总不同验证用例34+1，30临时+5既有，不冒称一次整批35。

AOT确切publish/AtomUI.Desktop.Controls.Tests为46493168字节，SHA256 b23cfb1d05f457e233e06d9b35d4757986fe43bab093f1f957a79ec2690de402；发布0 IL/CS/注册诊断，5条已知Homebrew/macOS26 dylib与target12链接警告单列。当前四算法验收中，不预记跨运行或清理门槛。


最终AOT同四算法各53帧/35检查PASS，共212帧/140检查；128静态跨运行全部0，表内12对也全部0。完整212对166零/46持续态非零，分为32计数/计时/Skeleton实时帧与14caret/输入帧；84持续态中38零/46非零，不冻结motion或放宽容差。候选/AOT各52主恢复全0，独立A/A与restore各4全0，+1DIP四组均检出。三版128静态各最短761/765/764ms、至少12次raw连续相同。

保留cross持续态 L：live-125 794/max224；live-425 774/max224；live-initial 1218/max224；live-loading-clear 789/max224；live-loading 301/max224；live-reattach 804/max224；live-shown 437/max224；live-template 478/max224；timer-input-cycle 514/max5；timer-input-template 521/max4；value-input-cycle 524/max19。

保留cross持续态 D：live-125 939/max199；live-425 311/max199；live-initial 1291/max199；live-loading-clear 690/max199；live-loading 307/max199；live-reattach 648/max199；live-shown 144/max199；live-template 313/max199；timer-input-cycle 524/max15；timer-input-template 514/max4；value-input-template 520/max10。

保留cross持续态 C：live-125 590/max224；live-425 711/max224；live-initial 1432/max224；live-loading-clear 1106/max224；live-loading 369/max224；live-reattach 991/max224；live-shown 459/max224；live-template 265/max224；timer-input-cycle 507/max4；timer-input-template 507/max4；value-input-cycle 509/max6；value-input-template 514/max10。

保留cross持续态 DC：live-125 1022/max199；live-425 1072/max199；live-initial 1366/max199；live-loading-clear 862/max199；live-loading 127/max199；live-reattach 994/max199；live-shown 326/max199；live-template 340/max199；timer-input-cycle 515/max8；timer-input-template 511/max8；value-input-cycle 515/max11；value-input-template 505/max4。

AOT暗Global/结束态重挂、Compact可见原生输入和实际计数推进原图已查看；源码13冻结逐字保持，AOT确切hash/大小见前。官方audit clean；30临时3源22642字节、636帧+4contact sheets+manifest待清理，随后精确StatisticSemanticPartTests5项及NET8，不提前记通过。35不同用例为34+1，未再扩大运行范围。平台/完整周期/GC/所有极值与完整家族/CT15继续保留。


临时判断完成并清理：30临时用例、3源22642字节；641取证文件（636帧+4contact sheets+manifest）官方audit clean后cleanup删除；driver414/publish15文件及原会话534文件/1380229325字节删除，原marker清理。零新增永久测试；清理后精确模块/NET8在独立小临时会话执行，结束后同样删除。


清理后最终验收：共享锁独占下精确StatisticSemanticPartTests（Debug/net10）5PASS/0FAIL/0Skip（416ms，命令26.96s）；NET8 Desktop Release 0警告/0错误（18.08s，命令18.31s）。30临时3源22642字节、641取证、534原会话已删除，独立最终会话4文件与marker记录后删除。正式五文档、CSV4metadata/2Own、四总表及作者自审同步；12明确静态仅待限定视觉批准，原44失败、72旧新/46cross持续态、字号边界、平台/GC/极值与完整CT15保留，未提交。


## 六包限定批准登记

2026-11-01 用户明确回复“仅批准六表现有条目及列明纠正，其余保留”（request_user_input_async / call_08c452ba85d7442ab817b3cf6c7dcce6 / item 0）。仅Flyouts48对及表列两项有效默认纠正、Collapse12对、Descriptions16对、GroupBox40对、Statistic12对、QRCode16对获得限定视觉批准。各表列明的裁剪、低对比、原失败、缺帧、恢复与持续态、平台及完整家族/CT15均保留，不授权提交；GroupBox三Reserved另按call_c8732844a7a841caacc4385b24e13bd3/0既有批准，不扩大。


最终结果：六不同场景Debug均PASS（原四12.4786s/新增二8.5300s），Release均PASS（四13.0868s/二7.4010s）；没有产品反例或源码修改，不把重复运行计为更多用例。6临时case/1源5368字节与7会话文件25204字节已删除，无媒体。清理后实际重建精确Statistic5+QRCode8+Empty13+Spin16+Marquee1共43PASS/0Skip/3.8264s；最终产品源码未变，复用Drawer最终NET8零警告零错误16.76s。git diff --check通过。作者另步自审确认provider/window保持存活与NoInlining观测，没有强持有被测对象；只补本六条对象退出，不关闭其他恢复/持续态/平台/极值或完整CT15。
