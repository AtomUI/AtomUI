# CT15 Modal 原生输入出口

复用Modal原13metadata/19Own、297+4验证、176 Headless/Skia与AOT相同帧及16对Global批准；不重复默认迁移/Part/桥接/GC。本轮只补真实macOS NSWindow与Overlay的焦点、键盘、异步veto和原内容/几何出口。

960×740 AtomUI owner，Dialog请求Surface520×420（不含Window chrome），原单行TextBox、140高ScrollViewer/24行全文、两标准按钮、normal motion保持。应用内构造AppKit NSEvent，按真实元素TranslatePoint映射native窗口内坐标，经NSApplication/AvnView进入Avalonia.Native；不是外部物理键盘或CUA。

每宿主通过真实pointer打开，记录实际TopLevel/native handle，断言Surface尺寸及viewport/extent/全文；click输入、native Cmd+A/字符输入、Tab焦点不越过会话；第一次Escape异步否决保持打开，允许后Escape关闭一次并恢复原打开按钮焦点；同一controller/content重开用Enter接受。公开Dialog scope Global23/Menlo进入真实Surface。所有操作与只读观测分开，不找Part设置外观。

捕获原生完整窗口矩形用screencapture -o（保留内部对话框/阴影和caption，不含外部OS窗口阴影），不做后处理裁剪/对齐/遮罩。失败保留，不据Headless结果推定原生通过。先Default两宿主最小实际运行，随后按有效反例/依赖补齐；临时驱动/媒体清理后精确模块验收，完整Modal/CT15不因本切片通过而关闭。


## 原生有效反例与修复合同

首Default两宿主99检查96PASS/3个Overlay焦点归属FAIL。实际owner input/opener被Tab访问：Overlay step0/1真实焦点祖先为owner StackPanel→ThemeConfigProvider→Window；step2为OverlayDialogPresenter。第三项原断言只允许Surface过窄，应按整个会话cross-root判定；真实泄漏为前两项，不把修正观测当产品修复。Window三Tab均在native会话内；两宿主原选择/编辑、异步veto/关闭一次/焦点返回/同controller重开Enter、520×420 Surface（Window client520×460）、140viewport/576extent/24行、23/Menlo实际Surface均通过。

第二仅加焦点日志批再次记录两个owner越界；捕获后Key目标活跃状态未确认，Escape未投递、veto断言false并最终5s timeout，保留为该批输入就绪缺口，不推导新的关闭产品bug。驱动改在每次Key之前用公开Activate/150ms及IsActive观测，仍走真实Cocoa事件。

完整UX合同：modal Overlay的Tab/ShiftTab循环必须限制在实际Overlay cross-root（可包含既有Focusable presenter），不访问owner input/opener或其他会话；非modal继续允许离开；运行中切IsModal即时启停；原内容、Surface尺寸、scrollbars/viewport/extent、标准Enter/Escape、async veto和close后恢复opener保持。嵌套会话以当前焦点所在topmost会话为边界。没有键盘事件重路由、清内容、改高度或关闭motion的许可。

源码证据：OverlayDialogPresenter已有IsModal的真实订阅，ControlTheme未声明任何modal Tab约束；DialogSurface已Focusable=true，不能误判为缺Focus。可由既有IsModal条件Style表达KeyboardNavigation.TabNavigation=Cycle，应优先放AXAML，保留非modal默认Continue与原CT11 MotionDuration owner绑定。


## Headless有效前后证据

前三批普通Avalonia Window场景中input.Focus=false，未获得原交互，应视为fixture/host未复现，不把这些失败计为Tab产品RED；加入正常render tick仍不足。改为与原生原触发相同AtomUI Window、保留960×740/520×420/140viewport/24行及motion，取得有效4项1PASS/3FAIL（14.60s命令）：正反16次Tab各访问owner、modal导航实际Continue；modeless可离开的正对照PASS。

最小修正只在OverlayDialogPresenterTheme已有IsModal=True条件Style内设置KeyboardNavigation.TabNavigation=Cycle；保留原CT11 MotionDuration owner改动，无C#事件路由/新API/尺寸/内容/动画变动。原四项修后4PASS（14.99s命令）。追加嵌套modal LTR/RTL、各12次交替Tab与ShiftTab及关闭后恢复下层输入，两项PASS；最终6PASS/0Skip、用例20s。原生修后回放正在执行，不预记通过。


## 扩展身份与失败批次分账

加入MessageBox独立Global身份后，四场景均保持520×420 Surface；Window client520×460，viewport140/extent576，24行。Headless首MessageBox Escape失败来自fixture未显式设置Cancel（Information样式默认NoButton），设置与原生完全相同的Default Ok/Escape Cancel后，单独重开/标准键/恢复1PASS（8.88s命令），不改MessageBox产品代码。

最终7临时与原Dialog/MessageBox276共283PASS/0Skip（42s）。其中nested LTR/RTL、modeless及实时IsModal保持。修后native Dialog两宿主115PASS；扩展四场景首批MessageBox Overlay焦点恢复false、重开Enter5s timeout保留。随后带命中/焦点及重开截图的四场景233PASS；不能仅凭该次通过宣称旧失败修复。

去掉重开截图额外等待、每Key检查AppKit keyWindow匹配后四场景261检查260PASS/1 Window MessageBox恢复当前焦点null。只读native活跃/记忆焦点日志下一批四关闭都正确，却MessageBox Window选择/编辑两断言FAIL，261中259PASS/2；不改产品焦点逻辑，不把native当前焦点null盲目当成需强制激活owner。

按官方AvnView坐标公式，用真实view convertPoint原点替代clientHeight近似，并记录实际PointerPressed坐标；本轮所有原点Y恰等于clientHeight，实际到达坐标与目标逐次相同，不把该等价映射写为旧失败根因。四场景261检查全PASS，但关闭main时只读Deactivated日志在native handle释放后仍访问它，驱动异常退出250；产品检查结果与驱动退出失败分开，不能计完整成功。已用明确handle为空分支修正观察者释放边界，无catch吞错；同源AOT正在执行。

旧native偶发焦点/输入失败尚未定位，不归FW1/FW2、不声明已修复。唯一生产修正仍是modal Overlay Tab循环。最终AOT、同源退出验证、临时清理及清理后模块/NET8尚待完成。


## 最终同源原生结果

修正日志在native handle释放后仅报告无handle而不访问已释放对象；最终NativeAOT与同源managed DLL均四场景261PASS/0FAIL、进程exit0。两者均保留无重开截图额外等待、每Key的IsActive/AppKit keyWindow匹配、真实AvnView坐标及实际PointerPressed位置只读观测。坐标原点Y在本机仍等于clientHeight，不能把该等价重写说为旧失败已修复。

覆盖Dialog/MessageBox×Overlay/NSWindow、原生点击打开/选择/输入/3个Tab、异步拒绝第一次Escape/允许后关闭一次/恢复按钮、同controller/content重开Enter、跨根退役；Surface520×420、Window client520×460、owner960×740、140viewport/576extent/24行及23/Menlo Global保持。headless七临时含嵌套LTR/RTL、modeless与热IsModal/MessageBox重开及原276共283PASS。旧原生233PASS、261中1焦点FAIL、261中2编辑FAIL、以及最后一次261PASS但shutdown日志exit250，全部原账保留；没有产品焦点恢复代码修改，不把最终绿倒写为这些偶发条件被定位/修复。

AOT27149832字节，SHA256 934352c697fa95e2e56f31f9f619f44aac583084368aafd02fdcaf49857b982c；零IL/CS warning，五条原Homebrew最低OS linker warning单列。应用内合成Cocoa经Avalonia.Native，不冒充外部物理输入/其他OS/IME全合同。已查看真实Overlay及Window/MessageBox图；原大字体与固定宽度/140视口下的长行裁剪保持完整数据，不宣称任意极值无裁剪。

唯一生产差异为modal条件Style的Tab循环；没有新公开API/Own/Part/字段/订阅/clock。当前进入临时清理、清理后276与NET8验收，不预记最终通过；完整Modal/CT15及旧未通过仍保留。


## 清理后精确验收与作者自审

2026-11-01 Modal原生输入/Tab修复：原生Overlay Tab进入owner input/opener，匹配原AtomUI Window宿主有效RED4项1PASS/3FAIL；仅在既有IsModal=True AXAML Style声明TabNavigation=Cycle，modeless默认Continue与热切换/嵌套LTR-RTL保持。7临时+276原模块283PASS；最终同源managed/AOT Dialog/MessageBox×Overlay/NSWindow各261PASS/exit0，Cocoa点击/选择/输入/Tab、async veto/Escape/重开Enter/恢复焦点、520×420Surface/140viewport/24行/23Menlo保持。旧原生偶发输入/焦点失败和shutdown观察者坏批留账，不声称全修。7临时/1源8644字节、38取证、425会话1125334263字节清理audit clean；清理后精确276PASS/0Skip17s、NET8零警告零错误19.98s。见modal-native-input.md；只有生产主题3行导航增量，原CT11 owner绑定保留，完整Modal/CT15和既定FW1/FW2边界不变，不提交。

最终filter=FullyQualifiedName~AtomUI.Desktop.Controls.Tests.Dialog.|FullyQualifiedName~AtomUI.Desktop.Controls.Tests.MessageBox.；先持共享产物非阻塞独占锁，实际重建清理树。AOT与managed均exit0，不以261内部检查掩盖任何驱动退出异常。没有新增常驻测试、没有运行全量、没有新分支/worktree/委派或提交。作者另步自审：AXAML优先、非modal不受影响、没有新生命周期资源/事件转发、原尺寸与焦点返回保持；非独立评审。Default原生通过不代替其他算法原生/外部物理输入/IME、全部自然极值或旧未定位失败。
