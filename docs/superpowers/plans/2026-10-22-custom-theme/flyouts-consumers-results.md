# CT15 Flyouts 当前结果（2026-11-01）

输入4metadata/11Own，原17源冻结。四算法44默认/type/key/slot逐字保持。原9例4默认PASS/5 Global有效RED：Host popup背景白而非#123456、Family Alibaba Sans而非Menlo、Size14而非23，Standalone Menu/Tree字体亦未取各自Global。

V2R1及Host/Menu/Tree声明式可选字体后26所选25PASS/1背景FAIL，Host字体自然跨现有完整继承进入实际popup，无需重绑。首脚本误把Menu接口追加EventArgs导致编译失败，已放回正确控制类，不算产品RED。

Presenter复用Arrow StyleKey保持；真实Host祖先存在时补带Style优先的owner背景资源订阅，运行时跨根不在Host模板内，代码注释具体原因，Native/专用Style优先仍需专项验证。首未注册Presenter fallback产生4既有异常，修正后26PASS/0Skip（9临时+17既有，6s）；无Host最终直接保留原Arrow主题不新增订阅/identity。44默认精确同。当前Own/Pointer/尺寸与scroll/Native/专用Style/动态退出/Skia/AOT/清理后验收仍待，完整家族/CT15不关闭；旧批准与CT25/FW1不改。


四专用AXAML实际覆盖/clear与typed形态通过。原探针误把FilledColor当Color而实际为IBrush编译失败，修正颜色观测、不计产品RED；测试内定制均通过专用Style class，不以节点设置颜色回退。Tree六字段初NullRef已拆为有效断言：真实Arrow.Background为null，原inner Own Setter被TemplateBinding Background压住。将Own背景放Presenter根、沿原TemplateBinding进入box，其他约束和内容保留；有效默认背景null→PopupBgColor是具体修正，必须列限定审查，不能默认自批。

Pointer Menu/Tree两有效RED，Horizontal期望23却8；以placement target Control上下文读取qualified FlyoutHost Own键，保留Style优先、Pointer唯一消费、Native41/clear与非Pointer归零。resource context仅打开期间持有并在释放清空，换target重新接线。修复后31PASS/0Skip（14临时+17既有，18s），44默认/type/key/slot精确保持；新增跨scope retarget/关闭重开与Host Native字体同实例验证中。完整家族/CT15不关闭。


跨scope两个有效RED：ShowAt(second)期望41/43却0，base.ShowAtCore关闭旧host释放了提前装上的新lease。改为target不同时先CloseForLifecycle再Ensure新context，既有Native/非Pointer合同保留；34PASS/0Skip（17临时+17既有，25s），重开取新53/59，Host字体热更新/Native/clear同popup内容不变通过。下一Host Own margin/shadows与retiredFlyout边界后成员/正常Skia/AOT；44默认后续精确核对，完整CT15不关闭。


当前35PASS/0FAIL/0Skip（18临时+17既有，29s），44默认/type/key/slot逐字同；Host margin19/两shadow、Native5/clear和旧Flyout退休不再跟producer31已通过。原内容/Source对象未替换规避触发。下一成员审查、正常Skia普通/Pointer/tree高度约束/输入与AOT，临时清理及最终门槛仍待。


成员阶段14源/7注册类型37→0，完整指纹同，注册数组/静态与实例字段序列各自保持。Flyout.cs原BOM造成脚本span偏1丢左括号，已在只读审查捕获、还原，未进入产品构建，不计RED。整理后35模块通过。

真实树点击额外有效RED：原100%命中标题TextBlock、行792×28/point(44,58)，底层TreeViewItem.Click=1，而Presenter公开事件=0。原代码错误按MenuItem订阅；改为统一接收TreeViewItem冒泡Click，不持有逐项递归delegate，保持原树准备，36PASS/0Skip（19临时+17既有，30s）。

普通Skia首Host完整轨迹所有前置及恢复检查通过，Pointer Menu各offset/Native/非Pointer/scope通过；Tree Own max233通过，下一wheel探针找不到可滚动host。已看40原行/233高图，仅前8行裁出；独立原条件断言ScrollViewer不存在有效RED。当前只在TreeViewFlyoutPresenter模板加入现有ScrollViewer，保留40行/原minmax/Auto竖bar/chaining/deferred设置，验证中；原smoke失败帧保留，不按First()异常本身冒称产品前证。

滚动修复后普通Skia Light49帧/29检查全部通过，已看Own/scroll/Global/专用Style实际图；竖向bar可见且真实wheel改变可见行。临时测试进一步锁定40行、233预算、bar可见与末端继续向下wheel时Offset恰等Extent-Viewport，最终37PASS/0Skip（20临时+17既有，32s）；44默认/type/key/slot精确保持。三版四算法分进程重放中，旧版明/暗各到缺ScrollViewer处异常，原Global/Own失败与缺帧必须留账，不作完整旧版成功结论。

第一轮完整结果仅作修复前账本：候选/AOT各196帧、116检查全PASS，旧版各36帧/20检查5FAIL，共144帧、缺52，四组在缺ScrollViewer处异常。144可比old/new90零/54非零；196跨运行184零/12定时非零，180静态全零。但逐帧审查发现此前断言不足：offset只读Flyout属性，打开中的实际Popup仍旧值；Host重模板图只有anchor、弹层消失，原版亦然，不能用相同失败图证明兼容。

补强实际Popup断言后Menu/Tree两有效RED：Native HorizontalOffset设41，Flyout属性41但Popup仍23；本地官方源码显示PositionPopup只在打开时复制。修复只在打开时把热Own/Native offset通过SetCurrentValue回放当前Popup，不重开/换内容/丢焦点；运行时Popup不在ControlTheme链内，代码注释原因。关闭后live provider仍存活的两项NoInlining弱引用检查PASS，不由此推导所有资源/GC出口已闭合。

补强Host重模板独立用例有效RED：IsPopupPinnedOpen仍true，原scroll对象和500偏移仍在，但flyout.IsOpen=false。增加OnApplyTemplate后复用既有QueuePinnedOpen，待新anchor挂载后按原guard协调。首编译缺Primitives import已补，不计产品RED；最终验证中。新驱动加入实际Popup与重模板断言，并在tree wheel恢复前将pointer移出，三版将同轨迹重放，旧批不与新批混算。

补强后40PASS/0FAIL/0Skip（23临时+17既有，37s）；末次成员审查14源/7注册类型0问题。第二轮旧版四组各36帧/25检查10FAIL，仍144主帧/缺52，在原ScrollViewer缺失处退出；额外旧版独立cal-only四组各4帧/4检查全部PASS，16帧不填补主历史。候选17源已finally精确恢复并冻结，最终candidate2/aot2采样中。


## Final replay evidence

40 focused cases passed (23 temporary + 17 existing, 37s). Final candidate2/aot2: 196 frames and 144 checks each, all PASS; 180 static + 16 timed frames. All 180 static cross-runtime pairs are exactly zero; total 184 zero/12 timed nonzero. Old2: 144 main frames, 100 checks/40 FAIL, four original missing-scroll exceptions. Each algorithm lacks the same 13 main frames; no replacement images. old2cal independently adds 16 frames/16 PASS, not main-history recovery.

144 comparable old/new pairs: 80 zero/64 nonzero; 48 intentional static proposed in the bounded table, 4 micro and 12 timed excluded. All 48 intended pairs have zero managed/AOT delta. Same-instance main restoration comparisons: 60 each, 56 zero and 4 global-clear micro; host retemplate now zero. Four post-click tree reopens are separate changed-input states (selected row retained and pointer moved outside the original anchor); they are not restoration assertions. Main host/pointer final checks and independent calibration PASS.

Missing main states per algorithm: cal-a, cal-b, cal-restore, cal-shift, pointer-end-restore, pointer-tree-click, pointer-tree-native, pointer-tree-normal, pointer-tree-reopen, pointer-tree-restored, pointer-tree-scope, pointer-tree-scroll-restore, pointer-tree-scroll.

Old2 failed check suffixes (each repeated across four algorithms): host-global, host-retemplate-open-content, menu-own-offset, menu-popup-hot-own, menu-popup-hot-native, menu-popup-normal-zero, menu-reopen, menu-retarget-scope, tree-own-offset, tree-popup-own.

Excluded exact deltas:

| Comparison | Frame | Pixels | Max | BBox |
| --- | --- | ---: | ---: | --- |
| old-new | compact-host-global-clear.png | 6 | 1 | [277, 108, 523, 294] |
| old-new | compact-host-open-0.png | 8328 | 131 | [294, 15, 506, 295] |
| old-new | compact-host-open-128.png | 20991 | 213 | [257, 96, 543, 324] |
| old-new | compact-host-open-64.png | 3694 | 53 | [270, 107, 530, 314] |
| old-new | dark-host-global-clear.png | 5 | 1 | [273, 101, 527, 296] |
| old-new | dark-host-open-0.png | 27531 | 113 | [304, 15, 496, 281] |
| old-new | dark-host-open-128.png | 3392 | 120 | [266, 101, 534, 318] |
| old-new | dark-host-open-64.png | 40566 | 158 | [278, 110, 522, 310] |
| old-new | darkcompact-host-global-clear.png | 5 | 1 | [277, 109, 523, 296] |
| old-new | darkcompact-host-open-0.png | 4942 | 117 | [305, 15, 495, 284] |
| old-new | darkcompact-host-open-128.png | 50344 | 184 | [268, 108, 532, 325] |
| old-new | darkcompact-host-open-64.png | 8795 | 164 | [279, 116, 521, 316] |
| old-new | light-host-global-clear.png | 6 | 1 | [273, 100, 527, 294] |
| old-new | light-host-open-0.png | 10536 | 135 | [289, 15, 511, 295] |
| old-new | light-host-open-128.png | 6141 | 68 | [252, 93, 548, 325] |
| old-new | light-host-open-64.png | 18730 | 191 | [263, 97, 537, 316] |
| managed-aot | compact-host-open-0.png | 17019 | 128 | [295, 15, 505, 295] |
| managed-aot | compact-host-open-128.png | 22842 | 214 | [256, 95, 544, 325] |
| managed-aot | compact-host-open-64.png | 21532 | 186 | [270, 15, 530, 314] |
| managed-aot | dark-host-open-0.png | 6250 | 111 | [304, 15, 496, 281] |
| managed-aot | dark-host-open-128.png | 53952 | 184 | [264, 99, 536, 325] |
| managed-aot | dark-host-open-64.png | 5363 | 160 | [277, 15, 523, 314] |
| managed-aot | darkcompact-host-open-0.png | 27257 | 116 | [308, 15, 492, 282] |
| managed-aot | darkcompact-host-open-128.png | 51495 | 185 | [268, 107, 532, 325] |
| managed-aot | darkcompact-host-open-64.png | 1346 | 59 | [283, 15, 517, 300] |
| managed-aot | light-host-open-0.png | 17863 | 131 | [291, 15, 509, 294] |
| managed-aot | light-host-open-128.png | 23426 | 214 | [252, 88, 548, 325] |
| managed-aot | light-host-open-64.png | 25146 | 187 | [266, 15, 534, 314] |

NativeAOT: {"path": "/private/var/folders/5w/_dgb_6815871tzq0vy_x_w1w0000gn/T/atomui-ct15-flyouts-hpgxqd6w/publish2/AtomUI.Desktop.Controls.Tests", "bytes": 46708688, "sha256": "a051cf1aeb08d08b3517605e4a66ea437c589b08a588848f6c64188e063c6a36"}. Published and executed from the same resolved publish2 path; official Avalonia12.1.4 unchanged, 5 existing Homebrew ld warnings, 0 warning IL lines.

Independent old2cal A/A and restore all zero; +1DIP Light1392/Dark1432/Compact1384/DarkCompact1416 pixels, max224. Candidate/AOT independent A/A and restore also zero, +1DIP all positive; original wrapper transform null and unset guard preserved, ClearValue used to restore.

13 state sheets across four algorithms were visually inspected, using full frames for all quantitative comparisons. Original large tree clips at the window top in both versions; Own233 keeps 40 rows with visible scroll bar and real wheel; dark Own green/white contrast and Compact offset8->4 are explicitly disclosed. Real OS/Browser, complete focus/platform/extreme/GC exits and continuous phase equality remain open.

Cleanup measured counts (all removed): {"temporary": {"files": 3, "bytes": 25325}, "evidence": {"files": 1191, "bytes": 16904847}, "driver": {"files": 320, "bytes": 582984954}, "publish": {"files": 15, "bytes": 219130261}, "publish2": {"files": 15, "bytes": 219135018}, "session": {"files": 486, "bytes": 1023085074}, "session_removed": {"files": 488, "bytes": 1023085510}}. Evidence audit clean; official cleanup succeeded. 23 temporary cases / 3 source files deleted, no permanent additions. Only this task's registered media, driver/two publishes/session were removed. Final cleaned verification pending.


清理后最终验收（2026-11-01）：共享锁空闲后独占执行，Flyouts既有17PASS/0FAIL/0Skip（用例760ms，命令31.37s）；NET8 Desktop Release 0警告/0错误（构建19.83s，命令20.01s）。23临时/3源25325字节、1191取证、320driver/两publish各15、488原会话文件已清理，最后6文件及final marker也删除。git diff --check通过。48对及列明有效默认纠正待限定批准，原40失败/52缺帧、4恢复micro/12跨运行定时、平台/完整出口及CT15保留；不改CT12/13/14批准及CT25/FW1，无提交。


## 六包限定批准登记

2026-11-01 用户明确回复“仅批准六表现有条目及列明纠正，其余保留”（request_user_input_async / call_08c452ba85d7442ab817b3cf6c7dcce6 / item 0）。仅Flyouts48对及表列两项有效默认纠正、Collapse12对、Descriptions16对、GroupBox40对、Statistic12对、QRCode16对获得限定视觉批准。各表列明的裁剪、低对比、原失败、缺帧、恢复与持续态、平台及完整家族/CT15均保留，不授权提交；GroupBox三Reserved另按call_c8732844a7a841caacc4385b24e13bd3/0既有批准，不扩大。
