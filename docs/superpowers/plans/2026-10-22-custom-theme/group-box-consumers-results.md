# CT15 GroupBox 当前结果（2026-11-01）

输入1metadata/7Own，4原源冻结。原14临时7PASS/7有效Global RED：四算法28默认导出通过，三个旧scalar配置0/非零确实改变resolved值，但左/中/右标题原几何均无变化；其余7项Root/标题/正文/图标/边框/背景Global尚未进入实际目标。NeedsDecision3保持，不自行当Reserved批准。

既有正式token文档已称三字段fieldset语义保留，但这不是本计划中的用户裁决；后续以实际无消费者证据、兼容名称/类型/key/slot/默认及替代方案提出限定提案。其余四Own继续实际Header/body几何，不把em/比例scalar强接到现有Thickness。

当前迁移V2 revision1并保留7字段/28默认，Root用实际InheritanceParent的可选字体/前景入口，AXAML Shared角色改现有OwnerTokenResource；代码只补内部继承源，不新增public属性/token或改渲染几何。验证中。四Active逐目标、专用Style/原生/动态Header gap、正常Skia/AOT、清理后模块/NET8与作者自审仍待，完整家族/CT15未关闭。

首次迁移后33所选32PASS/1FAIL（14临时+19既有，12s），仅Icon未取GroupBox Global，实际Width16而非31。IconPresenter是有独立身份的公共primitive，GroupBox模板里的三处Icon输入以AnchorControlType=GroupBox归属当前composition，不改它的独立主题/身份；已在AXAML补锚点。其余Root/标题/正文/边框/背景Global通过，原三scalar保持真实无消费，NeedsDecision仍3。

锚定后33通过，追加四Active左中右真实几何/原生Padding、四专用Style实际命中/clear/静态形态和原生字体/标题覆盖后39所选全PASS（20临时+19既有，22s），28默认/type/key/slot精确同。

独立绘制探针先整组Own更新PASS，随后固定外框单变量Native字号/位置两项1PASS/1有效RED：HeaderTitlePosition Left→Right后owner.Render次数仍1，而字号37会重绘。不能以整包资源更新触发重绘掩盖child-only缺口变化。Arrange后仅当最后绘制cache的border/header绝对bounds与当前不同，InvalidateVisual；原border/background geometry公式、renderScale rounding与现有字段不改。验证中，Normal Skia/AOT与Reserved提案最终证据仍待。

仅在owner Arrange补失效后41所选40PASS/1FAIL，位置仍不触发owner安排；不能把该尝试当修复。改为观察当前Header的LayoutUpdated后检查绝对缺口/border bounds，只有几何变化才失效；同一原公式抽成helper，模板替换/attach/detach配对接线与退休，原因写注释（ControlTheme无法从child完成布局通知owner绘制）。首编译缺VisualTree import已补，不计产品RED，重验中。三个scalar的[兼容裁决提案](group-box-token-ruling.md)仍待限定批准，CSV不改NeedsDecision。

补观察后41所选全PASS（22临时+19既有，24s），固定外框的Native字号/右移两单变量均触发实际绘制。正以已核对框架的LayoutUpdated backing delegate观测模板/卸载旧Target退休与重挂只订阅一次（仅临时测试反射，不进入产品/AOT驱动）；28默认精确复核后成员/正常Skia仍待。

追加观察者配对验证后42所选全PASS（23临时+19既有，26s），旧模板/卸载target=0、当前/重挂=1，28默认/type/key/slot精确保持。成员3源/1含注册属性类型0问题，候选4源冻结；正常Skia宽420高200、原scroll900/viewport90、左右/透明/RTL/Own/Global/三scalar 0/非零与回放主轨迹已开始。Reserved提案仍未批准，NeedsDecision3保持。

首Skia驱动按PART_ContentPresenter名字查Single，原嵌套ScrollViewer也有同名part，探针异常，非产品RED；已用TemplatedParent=真实owner限定目标，不删除原嵌套滚动或更换内容，重放中。

修探针后Light41帧/16检查全PASS，已看默认/右移/Global/专用Style全图，原420×200/长标题/scroll900-view90预算保留，Global23/Part27的原槽位裁剪列明。两个scalar配置帧与默认完整RGBA零，非仅属性相等。三版重放中，原明/暗各41帧/16检查14PASS2Global FAIL，均无缺帧、独立cal通过；候选/AOT及精确像素尚待。下一[Empty2/6](empty-consumers.md)只读准备，Own未决3保持。


## Final measured replay

Three versions164 static frames each,complete identical history without missing frames. Old64 checks56PASS/8Global FAIL;candidate/AOT64PASS each. Old/new124zero40intentional nonzero;managed/AOT164 all0. All defaults and both scalar0/nonzero stages old/new0. Main restoration68 each all0;candidate/AOT global->native-clear4/7/9/9pixels,max1 retained,old versions0. Independent AA/restore0 and +1DIP positive.

AOT identity: {"path": "/private/var/folders/5w/_dgb_6815871tzq0vy_x_w1w0000gn/T/atomui-ct15-groupbox-gowffoar/publish-final/AtomUI.Desktop.Controls.Tests", "bytes": 46476624, "sha256": "12a019be62600004d5060c6988908343ce1fbf682b2a338cfcb4af4c40a129a9"}. Published/executed from the same resolved path;official Avalonia12.1.4 unchanged,5 existing Homebrew ld warnings,0 warning IL lines.

| Version | Comparison | Pixels | Max | BBox |
| --- | --- | ---: | ---: | --- |
| old | light-cal-shift | 3275 | 224 | [190, 232, 611, 420] |
| old | dark-cal-shift | 3801 | 235 | [0, 40, 611, 600] |
| old | compact-cal-shift | 3291 | 224 | [190, 228, 611, 420] |
| old | darkcompact-cal-shift | 3817 | 235 | [0, 40, 611, 600] |
| candidate | light-cal-shift | 3275 | 224 | [190, 232, 611, 420] |
| candidate | light-global-to-native-clear | 4 | 1 | [602, 251, 609, 254] |
| candidate | dark-cal-shift | 3801 | 235 | [0, 40, 611, 600] |
| candidate | dark-global-to-native-clear | 7 | 1 | [601, 251, 609, 256] |
| candidate | compact-cal-shift | 3291 | 224 | [190, 228, 611, 420] |
| candidate | compact-global-to-native-clear | 9 | 1 | [599, 245, 610, 252] |
| candidate | darkcompact-cal-shift | 3817 | 235 | [0, 40, 611, 600] |
| candidate | darkcompact-global-to-native-clear | 9 | 1 | [599, 245, 610, 252] |
| aot | light-cal-shift | 3275 | 224 | [190, 232, 611, 420] |
| aot | light-global-to-native-clear | 4 | 1 | [602, 251, 609, 254] |
| aot | dark-cal-shift | 3801 | 235 | [0, 40, 611, 600] |
| aot | dark-global-to-native-clear | 7 | 1 | [601, 251, 609, 256] |
| aot | compact-cal-shift | 3291 | 224 | [190, 228, 611, 420] |
| aot | compact-global-to-native-clear | 9 | 1 | [599, 245, 610, 252] |
| aot | darkcompact-cal-shift | 3817 | 235 | [0, 40, 611, 600] |
| aot | darkcompact-global-to-native-clear | 9 | 1 | [599, 245, 610, 252] |

Cleanup counts (all removed): {"temporary": {"files": 3, "bytes": 19113}, "evidence": {"files": 545, "bytes": 5073272}, "driver": {"files": 414, "bytes": 1161394500}, "publish-final": {"files": 15, "bytes": 217866425}, "session": {"files": 501, "bytes": 1379546522}, "session_removed": {"files": 503, "bytes": 1379546957}}. Audit clean, official cleanup succeeded.23 temporary cases/3 sources deleted,0 permanent additions; original RED/failed attempts retained above. Final cleaned verification pending.


清理后最终验收（2026-11-01）：共享锁空闲独占后，GroupBox既有19PASS/0FAIL/0Skip（用例581ms，命令26.97s）；NET8 Desktop Release 0警告/0错误（构建18.08s，命令18.25s）。23临时用例/3源19113字节、545取证、414driver/15publish及503原会话文件已删除，6最终文件和两个marker也清理，audit clean、git diff --check通过。三版164静态/cross0，68主恢复0；Global恢复4/7/9/9 max1保留。仅40明确静态及三Reserved兼容提案待限定批准，NeedsDecision仍3，原裁剪/低对比/平台/完整出口及CT15保留；CT12/13/14和CT25/FW1不改，不提交。


## GroupBox三项兼容分类批准登记

2026-11-01 用户明确回复“批准三项 Reserved，其余边界保留”（request_user_input_async / call_c8732844a7a841caacc4385b24e13bd3 / item 0）。仅TextPaddingInline、OrientationMarginPercent、VerticalMarginInline归ReservedDocumented；保持名称/类型/默认/key/slot及原布局，无几何消费者，不标Active/ConsumerVerified。40对视觉仍待限定批准，恢复4/7/9/9 max1及其他家族/完整CT15边界保留，不授权提交。CT15 Own未决由3减为0，原137metadata/274Own及CT12/13/14、CT25/FW1不变。


## 六包限定批准登记

2026-11-01 用户明确回复“仅批准六表现有条目及列明纠正，其余保留”（request_user_input_async / call_08c452ba85d7442ab817b3cf6c7dcce6 / item 0）。仅Flyouts48对及表列两项有效默认纠正、Collapse12对、Descriptions16对、GroupBox40对、Statistic12对、QRCode16对获得限定视觉批准。各表列明的裁剪、低对比、原失败、缺帧、恢复与持续态、平台及完整家族/CT15均保留，不授权提交；GroupBox三Reserved另按call_c8732844a7a841caacc4385b24e13bd3/0既有批准，不扩大。

## 2026-11-02 原恢复路径定位

恢复本会话26428 Program及26226专用AXAML/代码、26458同名part观测修正，原完整41阶段×四算法164帧/64检查PASS。Global→native-clear严格残留再次为4/7/9/9像素、max1，bbox分别(602,251)-(609,254)、(601,251)-(609,256)、两Compact均(599,245)-(610,252)。保留原420×200/长标题/90视口900extent及完整操作历史；这不是用新场景代替原失败。

Light精确前缀后单变量诊断16检查通过：Global/native-clear的owner点190,220、420×200，header点19,11/382×31，title点248,233/430×27，borderY26.5、描边3/圆角13、背景#D9F7BE/描边#876543一致；native-clear仍4/max1，仅owner.InvalidateVisual即归零，完整背景往返亦0。没有在产品增加重绘补丁。

临时UnsafeAccessor只读字段探针13检查通过：Global/native/native-clear实际与缓存border均(0,26.5,420,173.5)，gap(19,11,382,31)，背景/边框Geometry实例始终相同，Bounds分别(3,29.5,414,167.5)/(0,26.5,420,173.5)。观测批仍复现4/max1，排除当前私有几何缓存被重建为不同内容的假设；观测不进入产品或永久测试。

独立官方12.1.4绘制对照复制确定性几何算法，以TextBlock重放Menlo23→Courier27→Menlo23；分别检验共用/独立inner Geometry，以及补齐真实TemplatedControl的ClipToBounds=true。三种对照A/A、清除、owner/背景重绘均0，未复现真实4像素；不据此宣称原框架根因或自动归FW2。原纯工程首CPM缺Skia版本项、Panel.Render sealed编译错误已修正，仅属临时驱动失败，不记产品RED。已查看原Global/native-clear全图，原裁剪与内容保持。

该独立模型分支停止扩展；原4/7/9/9仍未通过。无GroupBox生产修改，不新增AOT/NET8发布，匹配产品收据复用；当前进行临时材料清理与精确GroupBox门禁，不关闭家族或CT15。

收尾：256取证文件官方audit/cleanup clean/findings=[]后删除；外部driver/模型/只读字段观测及marker均已清理，项目内无临时编译输入。独占共享产物锁，以匹配清理后程序集运行精确GroupBox为19PASS/0FAIL/0Skip、584ms；NET8/AOT按相同产品源码的原范围复用，无额外发布或全量。
