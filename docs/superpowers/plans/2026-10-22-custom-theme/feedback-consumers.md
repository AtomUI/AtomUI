# CT-15 反馈与展示控件执行计划

2026-11-03：本计划既定范围已完成，137/274与26组合同保持；C1–C6逐项证据、最终门禁和原失败/平台保留范围见[最终有限出口](feedback-final-exit-results.md)。以下为执行时的原计划，不将P1或任何局部分包单独视为整体完成。

沿现有feature/custom-theme单agent执行，不创建分支/worktree、不提交。目录137项metadata、274个Own字段为范围，覆盖Alert/Card/反馈弹层、Timeline、Avatar/Tag/Badge/FloatButton、ImagePreviewer/Carousel/Result等实际终端与内部目标；一个修复不代替整族验收。

## P1 Timeline 的真实部件与默认输入

源码入口：Common TimelineIndicator及Desktop TimelineToken、TimelineItemTheme、TimelineIndicatorTheme。L03在Arrange写Dot.BorderThickness/CornerRadius本地值，L04在OnApply把DefaultIndicatorColor复制进IndicatorColor；T19的末项MinHeight没有消费者。先真实编译专用TimelineItemIconStyle/ContentStyle及公开Timeline/Item取得失败，保持方向、模式、pending/reverse、原root尺寸、rail溢出和可见内容。

- 将dot描边/半径及颜色fallback放ControlTheme，保留原boxSize=dotSize+borderWidth及半径一半；代码只保留实际layout。用有限转换器/编译绑定，不反射、不找Part写样式、不加时钟。IconHost按自身已测量盒排列，证明专用border/padding实际进入盒并保持未配置几何。
- 默认色保持原Primary，未配置IndicatorColor保持null并实时fallback；明确颜色、动态默认、清除和图标同实例更新。内部Global选择实际公开TimelineItem owner，独立primitive按Shared回退；不伪造identity。Default输入等价的现有代码绑定一并审查，Direct layout状态若不能Style表达，记录具体原因。
- Timeline11字段迁V2Revision1，ItemPaddingBottomLG明确依赖有效ItemPaddingBottom；其余按原Global/常量公式，明暗22个默认及公开ID/slot不变。未配置末项旧MinHeight0必须保持，历史Token资源默认48保留；LastItemContentMinHeight定义为明确Own配置才启用的OptionalOverride，非继承属性Unset回退0。最后可见item的content在两方向生效，0/清除及专用Style优先级验证，不把默认48自动施加导致旧布局变化。
- 先验证Setter/owner/依赖失败，再Skia旧新默认/配置/恢复、专用Style命中、真实布局与输入、GC及当前Mac NativeAOT。Pending布局用实际Pending状态，静态自定义PendingIcon矩阵与默认循环动画分开记录，不暂停默认循环制造稳定帧。其他平台及完整持续动效归CT25。
- 临时测试在Temporary/CT15Timeline，驱动/图像在外部临时位置；验证后删除，清理后Common/Desktop Timeline精确filter、NET8、成员审计、正式文档/CSV与作者自审。

P1实现、专项、清理及最终聚焦验证已完成，证据见 [结果](timeline-consumers-results.md)。P2继续根排版/其余Global、逐Own配置/极值和完整状态；其余控件的Own、Global、尺寸、弹层、完整业务状态和原生最终显示面仍待各自开工，不能计作CT15完成。

## P2 Timeline 排版、Global 与其余 Own 消费

基于 P1 完成后的确切源码，先采同一继承父节点的亮暗默认与已有公开字体配置，取得实际 consumer 失败。Timeline 与公开 TimelineItem 分别消费有效 Global FontFamily、FontSize、ColorText；未明确配置时保留真实 InheritanceParent 的字体与前景，不自动改变旧暗色文字。复用 Core 有限类型化继承订阅，扩展无 SizeType 的三输入字号与前景选择；ControlTheme 拥有 Setter，控件只提供受保护继承父的只读接口。公开本地值及专用 Title／Content Style 优先，清除、重挂、兄弟隔离与 GC 必须验证。

根已有背景／边框颜色读取 Timeline Global，section spacing 读取公开 TimelineItem Global，不自动增加边框或圆角。11 Own 在实际 dot／icon／rail、末项、pending、三模式、两方向逐字段验证明确值及零值，保留原始尺寸与内容。真实 Skia 比较旧新默认、A/A、1 DIP、A→B→A及输入，并执行当前 Mac NativeAOT。临时源与媒体清理后，再运行 Core helper、Desktop Timeline 与既有字号消费者的精确检查、NET8、成员和文档审查。默认颜色变更仍须单独审查，不以默认一致证明可读性。

P2已完成本包实现、专项、清理与最终聚焦验证，见 [排版／Global／Own结果](timeline-typography-results.md)。P3仍需Compact/RTL、完整业务和持续动效、极值及原生最终面；其他反馈展示控件继续，不据此关闭CT15。

## P3 Timeline 家族出口（2026-10-30 继续）

沿当前目录及分支单agent执行，不新增分支/worktree、不提交，既有改动保持。137 metadata／274 Own逐条作为输入；CSV原分类不是完成证据。CT12／13／14既定关闭与CT25／FW1边界保持。ButtonSpinner复用CT13完整批准；FloatButton复用CT11共享动效，完整CT15合同仍需核对。

- [ ] 真实业务矩阵：两方向、三模式、LTR/RTL、Reverse、隐藏/标签、pending增改移除、集合替换及重挂；同实例与原高度/滚动条/内容保持。
- [ ] Default/Dark/Compact/Dark+Compact下验证11 Own依赖、0/极值/清除、公开Item Global与原生/专用Part优先级；不把没有SizeType的Timeline虚构为S/M/L/Custom控件。
- [ ] 只对有效反例实施根因修复；临时验证位于Desktop Temporary/CT15TimelineExit，观测只读Part。模板可达绑定优先AXAML；代码运行期容器关系须说明释放归属。
- [ ] 正常默认pending持续动画与静态矩阵分开，保留同时间轨迹和A/A、1DIP、恢复检错；当前Mac Skia/NativeAOT与系统最终面分别记录。
- [ ] 清理临时源/媒体/专用产物，精确Timeline模块验收、NET8、成员及作者另步自审；同步正式文档/CSV/总表。新视觉变化另作具体限定审查。

后续按依赖推进Alert/Card、Message/Notification/Dialog、ToolTip/PopupConfirm、Avatar/Tag/Badge、ImagePreviewer/Carousel/Result，再核对其余目录项。任何局部分包通过都不关闭整个CT15。

P3-A两项根因修复的工程、18临时清理、52精确模块、NET8零警告及成员2/2/0已完成，见[状态切片结果](timeline-exit-state-results.md)；[8对末项限定视觉](timeline-exit-state-visual-review.md)已获限定批准。其他恢复/跨运行非零与完整业务、同时间持续轨迹仍开放，P3复选项不整体勾选，不以局部分包关闭Timeline或CT15。范围及交叉批准按[137/274核对](feedback-family-remainder.md)继续。

2026-10-30，用户明确回复“批准表内 8 对，其余未通过与完整 CT15 保留”（request_user_input_async / call_2f2081133b664f489dfdebc37e25baf6 / item 0）。仅P3-A表内末项rail/间距8对已获限定批准；其他非零、完整P3/Timeline/CT15及CT25/FW1边界保持，未授权提交。

## P3-B 帧推进与持续时间线补证

P3-A限定批准已登记，其他原始FAIL保留。单变量最小probe证明RunJobs+Thread.Sleep遗漏Dispatcher定时器：默认#e0000000，custom/clear均停#e20b030b；改为相同一秒正常DispatcherFrame后custom#ff802080、clear#e0000000。只改验证驱动，不改产品、依赖或容差。

- 用正常Dispatcher帧循环重放原1320×760、六根/四算法/16阶段，保留Button、长内容、原Height300、Part及真实输入。旧源码只回放P3-A两文件，持有共享产物锁，finally恢复并逐hash核对。
- 持续pending复用CT11已有的临时真实Compositor Stopwatch输入方法，在递增0/125/300/750/1000/1125/1300/1750/2000ms比较默认1s Spin的完整帧；单独保留真实时钟连续运行检查，不用静止相位证明实际循环运行。不修改正式源码、缓存程序集或时钟实现。
- 临时验证放Temporary/CT15TimelineP3B；外部工程/媒体放系统临时目录。验证/审图后删除，精确Timeline模块清理后验收；新增产品修改才需对应RED→GREEN及NET8增量构建。原NSWindow/其他平台及CT25/FW1问题保持独立。

P3-B补证已完成并清理：[结果](timeline-frame-pump-results.md)。旧新56/64为0、余8对为既定批准，JIT/AOT64/64为0，各440项检查通过；持续轨迹旧新52对0。严格周期端点候选/旧版各4FAIL及Part退出max1继续保留，不改写为通过。清理后Timeline52，产品源码未新增修改；继续Alert/Card实际消费者。

Alert工程切片已完成：[计划](alert-consumers.md)、[结果](alert-consumers-results.md)。1 metadata/10 Own有效消费者、两OptionalOverride、21 owner输入/根字体/图标Style优先级及V2R1完成；34验证、Mac managed/AOT各820检查、清理后9项/NET8零/成员2-1-0，临时25/434工程/795取证已清理。[28对限定视觉](alert-consumers-visual-review.md)已获限定批准，额外微差未通过；下一实际依赖为Card，完整CT15不关闭。

2026-10-30，用户明确回复“批准 Alert 表内28对，其余未通过与完整CT15保留”（request_user_input_async / call_8790c4384212492cba2b1512c1e8b335 / item 0）。仅表内28对有意Own/Global/Style变化已获限定批准；额外4旧新/1AOT微差、P3未通过及完整CT15保留，不授权Git提交。


2026-10-31 Card工程切片：[计划](card-consumers.md)、[结果](card-consumers-results.md)。7 metadata/22 Own、18Active/4Reserved，五文字根和真实owner、Small Grid/Tabs外padding、Native/Part实际投射及自带容器释放完成；56验证，Mac managed/AOT各1048检查、88对静态像素0。清理后精确验收见结果；[32对及两属性默认纠正](card-consumers-visual-review.md)已获限定批准，恢复非零与完整Card/CT15开放。下一依赖Message/Notification/Dialog/MessageBox。


2026-10-31 Message P1：[计划](message-consumers.md)、[结果](message-consumers-results.md)。六Own有效默认、五Active/一Reserved，Card/manager owner与正文Native角色、默认类型glyph/原初始null恢复及运行状态绑定释放；51验证，managed/AOT各236主+4独立检错+12持续Loading检查。35/80旧新及41/80跨运行零，额外微差和检测器/业务恢复仍FAIL；[8对字体/Native和root有效值](message-consumers-visual-review.md)已获限定批准，完整Message/CT15不关闭。P2共享几何/Host与Notification/Dialog依赖继续。


2026-10-31 Notification工程切片：[计划](notification-consumers.md)、[结果](notification-consumers-results.md)。3 metadata/17 Own，68默认/类型/key/slot保持；进度重挂、总量/remaining、动态需求刷新及两owner/Native/运行状态释放，55验证、managed/AOT各444检查、76主对0。38旧新0，[36有意与root有效值](notification-consumers-visual-review.md)已获限定批准，两暗恢复4/max16不通过。共享P2及完整Notification/CT15开放，继续Dialog/MessageBox等依赖。


Card限定批准登记：2026-10-31，用户明确回复“批准表内32对和两项属性默认纠正，其余未通过与完整CT15保留”（request_user_input_async / call_f0de5e7e830f44efaa6cd74e2c0072e0 / item 0）。仅本表32对和列明两项有效值/属性默认纠正获限定批准；其他原始非零、P2及完整家族/CT15保留，不授权Git提交。


Message P1限定批准登记：2026-10-31，用户明确回复“批准表内8对及root两项有效值归位，其余未通过与完整CT15保留”（request_user_input_async / call_8fffdadfe1c0457483143eea833656e6 / item 0）。仅本表8对和列明两项有效值/属性默认纠正获限定批准；其他原始非零、P2及完整家族/CT15保留，不授权Git提交。


Notification限定批准登记：2026-10-31，用户明确回复“批准表内36对及root两项有效值归位，其余未通过与完整CT15保留”（request_user_input_async / call_0421214e34e1401e90c4b4e835ef3c4e / item 0）。仅本表36对和列明两项有效值/属性默认纠正获限定批准；其他原始非零、P2及完整家族/CT15保留，不授权Git提交。


2026-10-31 Modal当前切片：[计划](modal-consumers.md)、[结果](modal-consumers-results.md)。13metadata/19Own全Active V2R1，76默认/type/key/slot保持，实际公共终端Global/正文/双Host/专用Part与资源释放；297+4验证、managed/AOT各224主检查、JIT/AOT176对全0、48校准A/A/恢复全0且1DIP全检出。25临时5源、1825取证已清理，最终276/NET8零/成员4-2-0；[16对Global](modal-consumers-visual-review.md)已获限定批准。Headless Window不替代NSWindow/OS输入，完整Modal的RTL/持续态/自然极值出口和完整CT15保留。下一依赖[ToolTip/PopupConfirm](tooltip-popup-confirm-consumers.md)：3metadata/13Own、三历史未消费字段先按现合同运行核定，不重复CT11动效或已关闭家族；当前NeedsDecision11不提前减数。


限定批准登记：2026-10-31，用户明确回复“批准表内16对Global变化，其余未通过与完整CT15保留”（request_user_input_async / call_62f69b5d81a14a9ab06d7d6f5bb64392 / item 0）。仅Modal表内16对Global获限定批准，真实OS输入及完整Modal/CT15、其他未通过保留，不授权Git提交。


2026-10-31 ToolTip/PopupConfirm当前切片：[结果](tooltip-popup-confirm-consumers-results.md)，3metadata/13Own，10Active/3Reserved，四算法52默认/type/key/slot保持；scope发布、真实字体/Global、模板part/宽度/颜色更新与关闭释放、共享mask模板跟踪完成。84+8验证、managed/AOT各92检查、120跨运行主对0、12校准全通过；25临时/1089取证清理，最终67/NET8零/成员5-4-0。仅[68对和根前景归位](tooltip-popup-confirm-consumers-visual-review.md)已获限定批准（根字号原本14未变）；四颜色恢复各2max1、两版Compact同坐标1max1继续保留。137/274不变，NeedsDecision余8；共享框体/OS Popup及完整家族/CT15未关闭。下一[Avatar/Tag/Badge](avatar-tag-badge-consumers.md)跨Controls/Desktop按实际依赖推进；FloatButton仍只复用CT11共享动效，不算CT12完整批准。


限定批准登记：2026-10-31，用户明确回复“批准表内68对及根前景归位，其余未通过与完整CT15保留”（request_user_input_async / call_a541f8393ca141a9afd55def80071fb0 / item 0）。仅表内68对与ToolTip根Foreground角色归位获限定批准，根字号原本14不变；四颜色恢复各2/max1、两版Compact同坐标1/max1及完整家族/CT15/OS边界保留，不授权Git提交。


2026-10-31 CT15 Avatar当前切片：[结果](avatar-consumers-results.md)、[48对限定视觉已批准](avatar-consumers-visual-review.md)、[作者另步自审](avatar-consumers-review.md)。3metadata/11Own全Active V2R1、44默认/type/key/slot与自然文字14保持；连续Custom/Circle、字体/owner/Group投射、membership退出及真实overflow热资源发布修复，39匹配验证、managed/AOT各100检查，136跨运行134零；两版主业务恢复各36零。closed/往返微差全部保留。18临时/1843取证/411driver/551原会话清理，最终Controls7/Desktop14及NET8零、成员5-2-0。仅48有意变化已获用户限定批准；完整Avatar/CT15、OS/Browser/Popup过程时相保留。下一[Tag](tag-consumers.md)已有4有效RED/5PASS、36默认导出，尚未改产品；NeedsDecision8及137/274不变，无Git。


限定批准登记：2026-10-31，用户明确回复“批准表内48对，其余未通过与完整CT15保留”（request_user_input_async / call_7b292d7ef454420fb8fc0603ac3e3f4c / item 0）。仅本表48对有意变化获批准；closed/往返微差、OS/Browser/Popup过程时相及完整Avatar/CT15继续保留，不授权提交。


2026-10-31 CT15 Tag：[结果](tag-consumers-results.md)、[26对限定视觉已批准](tag-consumers-visual-review.md)、[作者另步自审](tag-consumers-review.md)。8metadata/9Own全Active V2R1、36默认/type/key/slot保持；真实字体/owner/关闭glyph、container退役强链修复，84匹配验证、managed/AOT各120，164跨运行159零；原微差及派生输入大值影响保留，不改CT13批准。18临时/1090取证/外部driver与会话清理，最终66/NET8零、成员18-4-0。NeedsDecision余7，137/274不变；仅26有意帧已获限定批准，完整Tag/CT15不关闭。下一[Badge](badge-consumers.md)13metadata/20Own仅准备反例，未改源，无Git。


限定批准登记：2026-10-31，用户明确回复“批准表内26对及列明派生影响，其余未通过与完整CT15保留”（request_user_input_async / call_5cea4f7e59a243dcab027c42807d9848 / item 0）。仅26对与列明SelectTag继承close37/0导致的换行/原高度裁剪影响获批准；恢复、5对跨运行微差、原CT13批准边界、OS/Browser及完整Tag/CT15保留，不授权提交。


2026-10-31 CT15 Badge当前切片：[结果](badge-consumers-results.md)、[48对限定视觉已批准](badge-consumers-visual-review.md)、[作者另步自审](badge-consumers-review.md)。13metadata/20Own全Active V2R1、80默认/type/key/slot保持；真实owner字体/跨根发布/专用Style/实例颜色clear与投射退役修复。57所选56PASS、Count composition GC1FAIL保留；managed/AOT各72检查、164跨运行全零，4无定制默认旧新零，显隐/重挂及校准恢复非零不豁免。25临时用例3源26658字节、2524取证、419driver/15publish（原会话共537文件）已清理，清理后既有Badge32PASS/0Skip及NET8零警告零错误23.02s，成员22-7-0。NeedsDecision7与137/274不变；仅申请48对明确变化，完整Badge/CT15和CT25/FW1边界保留。下一按实际依赖推进ImagePreviewer/Carousel/Result，无Git提交。


限定批准登记：2026-10-31，用户明确回复“批准表内48对，其余未通过与完整CT15保留”（request_user_input_async / call_d223a18435b5484787082d74b8ee393e / item 0）。仅Badge表内48对有意变化获批准；Count composition GC1FAIL、显隐/重挂/校准恢复非零、其余108对及完整Badge/CT15继续保留，不授权提交。


ImagePreviewer current checkpoint: [execution](image-previewer-consumers.md). 13 metadata/14 Own, 56 default records unchanged, 142 focused checks passed (25 temporary/117 existing), member audit 27 files/12 controls/0 issues with exact pure-move fingerprints. Public owner/optional switch size/Global-font/real dedicated styles/Overlay bridge repairs implemented; normal-Skia old/new/AOT replay is in progress. Temporary cleanup and final verification pending; full family and CT15 remain open. Next [Carousel](carousel-consumers.md) is read-only preparation, no new product changes during shared sampling.


2026-10-31 CT15 ImagePreviewer：[结果](image-previewer-consumers-results.md)、[104对限定视觉已批准](image-previewer-consumers-visual-review.md)、[作者另步自审](image-previewer-consumers-review.md)。13metadata/14Own V2R1、56默认/type/key/slot保持；可选切换尺寸、公开owner/Global字体、七部件专用Style与Overlay桥修复。142临时阶段全通过（25临时+117既有）；managed/AOT各200检查、448帧，384静态355跨运行零、29静态及64持续态非零保留。三版各32恢复全零，独立A/A/恢复零、1DIP各408/max155检出。3临时源18995字节、1401取证、原505会话（含driver414/publish15）已清理；清理后117PASS/0Skip及NET8零警告零错误22.76s，成员27-12-0纯移动指纹同。NeedsDecision余6，137/274不变。仅申请104明确变化，其他非零、OS/Browser/输入/极值及完整ImagePreviewer/CT15保留；下一Carousel6/7按有效反例推进，无提交。


限定批准登记：2026-10-31，用户明确回复“批准表内104对，其余未通过与完整CT15保留”（request_user_input_async / call_5593304c893e46a296348b6914a561c3 / item 0）。仅ImagePreviewer表内104对明确变化获批准；其余171对旧新非零、29静态/64持续态跨运行非零、OS/Browser/输入/极值及完整ImagePreviewer/CT15继续保留，不授权提交。


2026-10-31 CT15 Carousel：[结果](carousel-consumers-results.md)、[44对限定视觉已批准](carousel-consumers-visual-review.md)、[作者另步自审](carousel-consumers-review.md)。6metadata/7Own V2R1、28默认/type/key/slot保持；可选ArrowOffset、Global/字体/通知、空集、投射/进度/模板与caller-owned页退出修复。26聚焦+2持续工作检查通过，managed/AOT各48/128帧，全部112静态跨运行0、候选/AOT各44恢复0；4跨运行时相与16旧新持续态保留。25临时1源17042字节、481取证、原516会话（含driver408/publish15）清理；清理后3PASS/0Skip（含两项原持续工作）及NET8零警告零错误18.04s。成员最终9-4-0，两阶段纯移指纹同。NeedsDecision余5，137/274不变；仅44明确静态申请批准，平台/输入/极值/完整家族与CT25/FW1保留。下一Result2/15，无提交。


Result当前切片：[执行](result-consumers.md)、[结果](result-consumers-results.md)、[作者自审](result-consumers-review.md)。2metadata/15Own，60默认精确保持；19匹配验证通过，候选四算法Skia68检查通过，旧新/AOT进行中。字体/Global/脱离状态回放修复，五专用Style及七状态/原插画内容保留，临时清理和最终模块/NET8尚未完成，不关闭家族。之后按[剩余交叉输入](feedback-cross-family-consumers.md)继续，不重复ButtonSpinner/既有批准机制。


限定批准登记：2026-10-31，用户明确回复“批准表内44对，其余未通过与完整CT15保留”（request_user_input_async / call_aa6a1cc8835941ae997eada0a60a4e92 / item 0）。仅Carousel表内44对静态变化获批准；其余16对旧新持续态、4对跨运行时相、平台/输入/极值及完整Carousel/CT15继续保留，不授权提交。


2026-10-31 CT15 Result：[结果](result-consumers-results.md)、[60对限定视觉已批准](result-consumers-visual-review.md)、[作者另步自审](result-consumers-review.md)。2metadata/15Own V2R1、60默认/type/key/slot保持；字体/Global/attach回放、逐glyph Template优先typed relay与退出修复。24匹配验证（15临时+9既有）、managed/AOT各72检查/132帧，跨运行132全0、候选/AOT各32恢复0、四默认旧新0。原pre-glyph384帧/18恢复失败账本保留，最终旧132中60有意差异含Native glyph27×31/Orange优先修正。3临时源16187字节、846取证、原501会话（含driver414/publish15）已清理；清理后9PASS/0Skip及NET8零警告零错误17.21s。成员8-2-0，早期7项纯移指纹同、后功能变化单列。NeedsDecision5与137/274不变；资源GC判据不足/平台/自然极值及完整Result/CT15保留，不扩CT25/FW1。下一按既定归属推进FloatButton13/8与剩余交叉，无提交。


限定批准登记：2026-10-31，用户明确回复“批准表内60对及列明原生优先修正，其余边界与完整CT15保留”（request_user_input_async / call_99a149043b344680ba0a26e4e74cb239 / item 0）。仅Result表内60对与自定义glyph27×31/Orange原生优先修正获批准；原pre-glyph384帧/18失败文本、GC判据不足、平台/极值/完整Result与CT15继续保留，不授权提交。


2026-10-31 CT15 FloatButton：[结果](float-button-consumers-results.md)、[65对限定视觉已批准](float-button-consumers-visual-review.md)、[两offset Reserved已批准](float-button-token-ruling.md)、[作者另步自审](float-button-consumers-review.md)。13metadata/8Own V2R1、32默认/type/key/slot精确保持；6Active真实消费者、2legacy scalar保留未决，NeedsDecision5不减。95临时阶段通过（49临时+46既有），BackTop正常显隐/精确滚动/Caller绑定、badge/成员退出、真实Style与Group reflow修复；managed/AOT各72检查68PASS/4最终恢复FAIL，200跨运行166零（静态162/168零，6静态/28定时非零保留）。仅65明确静态与两Reserved提案待限定审查，原文字/窄宽/字体23换行及窗口上缘裁剪列明；其余115旧新非零、主恢复、平台/极值/GC及完整家族未关闭。3临时源32518字节、777取证、537原会话（含driver414/publish15）与6最终文件清理；清理后Float46PASS/0Skip、NET8零警告零错误25.23s，成员19源/10含注册类型0问题。137/274与CT12/13/14既定批准、CT25/FW1边界不变，不提交。下一[Drawer3/13](drawer-consumers.md)已只读准备，按实际反证继续。


限定批准登记：2026-10-31，用户明确回复“批准65对及两项Reserved，其余保留”（request_user_input_async / call_e7be1ded2cd04cea8d90b50314dba528 / item 0）。仅FloatButton表内65对明确静态（含列明原窄宽换行/窗口上缘裁剪）与CircleBadgeOffset/SquareBadgeOffset两项Reserved兼容分类获批准；两字段仍无scalar消费者，不改写成Active/ConsumerVerified，不缩分母。NeedsDecision由5降为3，仅余GroupBox三字段。其余115旧新非零、34跨运行非零、恢复/平台/GC/极值及完整FloatButton/CT15保留，CT12/13/14既定批准和CT25/FW1不变，不授权提交。


2026-11-01 CT15 Drawer：[结果](drawer-consumers-results.md)、[20对限定视觉已批准](drawer-consumers-visual-review.md)、[作者自审](drawer-consumers-review.md)。3metadata/13Own V2R1、52默认精确保持；73聚焦（35临时+38既有），Global/可选字体、标题省略和两层重模板回放修复。原预修复588帧/8中途失败、校准失败47帧及旧AOT错批196帧留账。最终有效三版各196帧，候选/AOT各116检查、164静态跨运行0、各68恢复0。20明确静态已获限定批准，21旧新/20跨运行定时非零保留。35临时3源25362字节、1518取证、550原会话与4最终文件清理；清理后38PASS/0Skip、NET8零警告17.97s，成员7-3-0。137/274与Own未决3保持，完整家族/CT15/平台/GC/极值及CT25/FW1保留，不提交。下一[Tour7/9](tour-consumers.md)，未启动余35metadata/71Own。


2026-11-01 CT15 Tour：[结果](tour-consumers-results.md)、[44静态已获限定批准](tour-consumers-visual-review.md)、[作者自审](tour-consumers-review.md)。7metadata/9Own全Active V2R1、36默认精确保持；60聚焦（31临时+29既有）通过，退投射/动态margin/12专用Style/动作及模板/目标跟随与失锚修复。非CSD最终flip报告按实际margin输入校正，CSD/旧批准不改；成员16源/7控件0问题。候选/AOT各212帧/112检查，cross198零/14非零，各80主恢复零；旧版同动作模板异常只到140帧、缺72不能称完整，old独立cal16帧另计。仅44明确静态已获限定批准，2静态micro/12跨运行定时/16旧新定时及缺帧后历史/自然极值/GC/平台/完整CT15保留。31临时3源29446字节、1448取证、614原会话（driver415/两publish各15）与4最终文件清理；清理后29PASS/0Skip、NET8零警告17.12s。137/274与Own未决3保持，下一[Flyouts4/11](flyouts-consumers.md)，未完成当前家族之外未启动余28metadata/62Own，不提交。


2026-11-01 CT15 Flyouts：[结果](flyouts-consumers-results.md)、[48对待限定视觉审查](flyouts-consumers-visual-review.md)、[作者自审](flyouts-consumers-review.md)。4metadata/11Own全Active V2R1、44默认/type/key/slot保持；40聚焦（23临时+17既有）通过，实际owner字体/背景、跨scope与热Popup offset、真实Tree click/scroll和pinned重模板修复。候选/AOT各196帧/144检查PASS，180静态cross0、12定时非零；旧版144主帧/100检查40FAIL、缺52，独立16cal不填主历史。48明确静态全cross0，其他4恢复micro/12定时、原裁剪与暗Own配色、平台/完整出口保留；有效背景null→Own及Compact Pointer8→4列明。23临时3源25325字节、1191取证、488原会话（driver320/两publish各15）与6最终文件清理；最终Flyouts17PASS/0Skip、NET8零警告19.83s，成员14源/7控件0。137/274与Own未决3保持，完整家族/CT15及CT25/FW1开放；下一[Collapse2/15](collapse-consumers.md)，当前家族之外未启动24metadata/51Own，不提交。


2026-11-01 CT15 Collapse：[结果](collapse-consumers-results.md)、[12静态待限定视觉审查](collapse-consumers-visual-review.md)、[作者自审](collapse-consumers-review.md)。2metadata/15Own全Active V2R1、60默认精确保持；75临时阶段73PASS/2GC FAIL，Global字体/原角色、runtime item退休和四专用Style验证完成，GC保留。三版各220帧，候选/AOT各128检查PASS，204静态cross全0、11定时非零；old/new196零24非零，仅12明确静态待审。三版各76主恢复0，另4 Global恢复5/max1；原长文/Extra/520宽/裁剪与暗色显式低对比列明，GC原始和各独立/整批失败不映射FW1。25临时3源21834字节、723取证、518原会话（driver414/最终publish15）及6最终文件清理；清理后50PASS/0Skip与NET8零警告18.27s，成员5源/2控件17→0纯指纹和字段序列同。137/274与Own未决3保持，完整家族/CT15开放；下一[Descriptions6/10](descriptions-consumers.md)，其他未启动22metadata/36Own，不提交。


2026-11-01 CT15 Descriptions：[结果](descriptions-consumers-results.md)、[16静态待限定视觉审查](descriptions-consumers-visual-review.md)、[作者自审](descriptions-consumers-review.md)。6metadata/10Own全Active V2R1、40默认精确保持；运行只有Root独立Own10、五模型/内部视觉无独立fragment，维持原身份不缩输入。81聚焦PASS（48临时+33既有），Global/角色、generated cell退休/Native parent/focus/集合/offtree快照修复。主三版164静态、候选/AOT124检查PASS，静态cross全0及56+4恢复0；仅16有意变化待审。原主Native缺40/异常保留，无模板原生探针不算glyph证据；独立可见TextBox候选/AOT44帧44检查/cross全0，原初始4后异常/缺40不填。520宽三列窄槽/长Wrap/下缘裁剪及暗低对比列明，平台/完整出口/CT15开放。48临时3源24053字节、1033取证、569原会话（driver415/两publish各15）与6最终文件清理；清理后33PASS/0Skip、NET8零警告18.03s，成员8源/4注册属性类型16→0纯指纹和字段序列同。137/274与Own未决3保持，下一[GroupBox1/7](group-box-consumers.md)，其他未启动16metadata/26Own，不提交。


2026-11-01 CT15 GroupBox：[结果](group-box-consumers-results.md)、[40静态待限定视觉审查](group-box-consumers-visual-review.md)、[三Reserved待裁决](group-box-token-ruling.md)、[作者自审](group-box-consumers-review.md)。1metadata/7Own V2R1、28默认保持，四Active真实消费；三scalar resolved0/非零但无几何/RGBA消费，仍NeedsDecision。42聚焦PASS（23临时+19既有），实际Global/字体/图标owner与固定外框child-only缺口重绘修复，观察者旧0/当前1退休通过。三版各164帧，候选/AOT各64检查、164cross全0，68主恢复0，Global恢复4/7/9/9 max1保留。40有意静态含28缺口修复/12Global与原生；固定420宽长标题裁剪/暗半标题低对比列明，正文offset180视口外不夸大glyph证据。23临时3源19113字节、545取证、503原会话（driver414/publish15）及6最终文件清理；清理后19PASS/0Skip和NET8零警告18.08s，成员3源/1属性类型0问题。137/274、Own未决3与既定批准保持，完整家族/CT15开放；下一[Empty2/6](empty-consumers.md)，其他未启动15metadata/19Own，不提交。


2026-11-01 CT15 Empty：[结果](empty-consumers-results.md)、[20静态待限定视觉审查](empty-consumers-visual-review.md)、[作者自审](empty-consumers-review.md)。2metadata/6Own全Active V2R1、24默认/type/key/slot保持；37聚焦PASS（24临时+13既有），实际Global/可选字体/三专用Style、离树图片和原生Footer弱焦点回放修复。三版各180帧，候选/AOT各96检查PASS，156静态cross全0、56主恢复及独立A/A/restore各4全0；仅20明确静态已获限定批准。旧32FAIL、19旧新与10跨运行输入持续态非零、原暗色低对比及平台/完整路径/极值/GC/完整CT15保留。24临时3源16381字节、545取证、508原会话（driver414/publish15）及4最终文件清理；清理后13PASS/0Skip（573ms，命令27.72s）、NET8零警告18.85s，成员6源/1含注册属性类型0问题。137/274与Own未决3保持，下一[Statistic4/2](statistic-consumers.md)，其他未启动13metadata/13Own，不提交。


2026-11-01 CT15 Statistic：[结果](statistic-consumers-results.md)、[12静态待限定视觉审查](statistic-consumers-visual-review.md)、[作者自审](statistic-consumers-review.md)。4metadata/2Own V2R1、8默认/type/key/slot保持，四Global身份Own数量0/2/0/0；34+1个不同用例PASS（30临时+5既有），实际Global/字体/六专用Style、原生输入弱焦点与完成倒计时重建分类修复。三版各212帧，候选/AOT各140检查PASS，128静态cross全0、52主恢复及独立A/A/restore各4全0；仅12明确静态待审。旧44FAIL、72旧新/46跨运行持续态非零、原字号0官方异常/正极值裁剪/显式暗低对比及平台/GC/完整CT15保留。30临时3源22642字节、641取证、534原会话（driver414/publish15）及4最终文件清理；清理后5PASS/0Skip（Debug416ms，命令26.96s）与NET8零警告18.08s，成员8源/4含注册属性类型0问题。137/274与Own未决3保持，下一[QRCode2/2](qr-code-consumers.md)，其他未启动9metadata/11Own，不提交。


2026-11-01 CT15 QRCode：[结果](qr-code-consumers-results.md)、[16静态待限定视觉审查](qr-code-consumers-visual-review.md)、[作者自审](qr-code-consumers-review.md)。2metadata/2Own V2R1、8默认/type/key/slot保持；43不同用例PASS（40所选+11补充中3新，35临时+8既有），原Global/字体/专用Cover、空Value清码、Bitmap/刷新退休与原生输入弱focus修复。错误背景480帧与未运行publish排除，修正scope后有效三版各240帧，候选/AOT各140检查PASS，176静态cross全0，四默认0；仅16明确静态待审。三版主恢复52零/8微差（L/C两清除各1/max2，D/DC各12/max1），旧44FAIL/46旧新与41cross持续态非零、原裁剪/方角Cover/低对比/ECI/平台/GC/完整CT15保留。35临时3源24025字节、1221取证、566原会话（driver415/两publish各15）及4最终文件清理；最终8PASS/0Skip（Debug706ms，命令29.01s）、NET8零警告19.04s，成员6源/1注册属性类型0。137/274与Own未决3保持，下一[MarqueeLabel2/2](marquee-label-consumers.md)，未启动7metadata/9Own，不提交。


限定批准登记：2026-11-01，用户明确回复“批准表内20对，其余边界保留”（request_user_input_async / call_cf78d1ab3bc3431289c5246103fe223e / item 0）。仅Drawer表内20对明确静态变化获批准；其余原失败、缺帧、恢复/持续态、平台/GC/极值和完整家族/CT15继续保留，不授权提交。


限定批准登记：2026-11-01，用户明确回复“批准表内44对，其余边界保留”（request_user_input_async / call_e83d092a248045288230fca221d69589 / item 0）。仅Tour表内44对明确静态变化获批准；其余原失败、缺帧、恢复/持续态、平台/GC/极值和完整家族/CT15继续保留，不授权提交。 Primary大min仍沿原生slide/重叠，不批准“极值不重叠”。


2026-11-01 CT15 MarqueeLabel：[结果](marquee-label-consumers-results.md)、[8静态已获限定批准](marquee-label-consumers-visual-review.md)、[作者自审](marquee-label-consumers-review.md)。2metadata/2Own V2R1、8默认/type/key/slot保持；22所选PASS（21临时+既有1），Owner Global/原生Style、真实宽度重建和旧完成退休修复。首批空遮挡43帧排除，有效三版各172帧，managed/AOT各100检查PASS、112静态cross全0、各44主恢复与独立A/A/restore各4全0。仅8明确静态已获限定批准；48旧新/55跨运行持续态非零，原4倍时长、0速度异常、窄宽与字体23裁剪及平台/GC/完整CT15保留。21临时1源12070字节、564取证、500原会话（driver408/publish15）及4最终文件清理；最终10PASS/0Skip（637ms，命令36.86s）、NET8零警告21.98s，成员3源/1属性类型0问题。137/274与GroupBox未决3不变，下一[Spin5/7](spin-consumers.md)，未启动只余5metadata/7Own，不提交。


限定批准登记：2026-11-01，用户明确回复“批准表内8对，其余边界保留”（request_user_input_async / call_492fb90497c742dfbc2801dac1500c71 / item 0）。仅MarqueeLabel表内8对Global/跨scope字体颜色变化获批准；48旧新/55跨运行持续态非零、首批43帧排除、原4倍时长/0速度异常、裁剪、平台/GC/完整CT15继续保留，不授权提交。


2026-11-01 CT15 Spin：[结果](spin-consumers-results.md)、[46主静态已获限定批准](spin-consumers-visual-review.md)、[作者自审](spin-consumers-review.md)。5metadata/7Own V2R1，四算法28默认保持；50所选全PASS（34临时+既有16），Owner字体/Global、仅模板自定义、原生glyph尺寸/Style热更新与原内容弱focus修复。最终三版各172帧、managed/AOT各124检查PASS、136静态cross全0；独立无Controls默认12对旧新/cross零，各56主恢复与A/A/restore各4零。旧124中28FAIL、32旧新/32cross持续态及8独立cal旧新非零、OS/Browser/GC/完整周期与完整CT15保留。86错误驱动帧及480 pre-hot-style帧独立留账。34临时3源18621字节、1114取证、原540会话（driver420/两publish各15）及4最终文件清理；最终16PASS/0Skip（663ms，命令36.54s）、NET8零警告21.80s（命令22.05s），成员8源/2控件0。137/274与GroupBox Own未决3保持；实际未启动家族归零，复用ButtonSpinner4/8的CT13批准，不代表CT15完成。下一回到Timeline P3持续端点/Part恢复与[Count退出保留](badge-count-retirement.md)等真实出口，无提交。


限定批准登记：2026-11-01，用户明确回复“批准表内46对及列明原生优先修正，其余边界保留”（request_user_input_async / call_4176ecaa6e314f4da281e28a8657ee2f / item 0）。仅Spin表内46对主静态与列明glyph尺寸/普通Style优先修正获批准；32持续态cross非零、8独立cal旧新非零、平台/GC/完整周期及完整家族/CT15保留，不授权提交。


2026-11-01 CT15 P3-C：[端点结果](timeline-endpoint-results.md)、[96独立端点已获限定批准](timeline-endpoint-visual-review.md)、[作者自审](timeline-endpoint-review.md)。官方12.1.4显式0首key在整周期产生NaN，原8Timeline+4Native有效FAIL；terminal-only4和RecordingEasing1单变量定位。Core Icon用明确0隐式起点+唯一2π终点修正，70所选+时钟访问1项PASS；不改周期/easing/显隐/内容/布局/依赖。三版各752帧、managed/AOT各648检查PASS，旧504PASS/144FAIL；720受控相位及16校准cross全0，96独立变化+64同帧重复，其余576原新受控/校准零，正常clock各15非零保留。18临时2源14672字节、2817取证、原470会话（driver412/publish15）及4最终文件清理；最终53PASS/0Skip（命令30.80s）、Core NET8零警告3.37s。成员1源/1控件24→0纯指纹/字段序列同。只关闭当前消费者端点缺陷的工程验证，不改写旧FAIL或CT12/13/14、CT25/FW1；Part恢复/平台/完整家族与CT15仍开放，137/274及Own未决3不变。下一核对Spin完整循环与已记录Count退出保留，不重复已批准静态分包。


限定批准登记：2026-11-01，用户明确回复“批准表内96对，其余边界保留”（request_user_input_async / call_d2f7bcbd00ab46e7b6cd3610f38aa4bb / item 0）。仅P3-C表内96个独立端点变化获批准；64张second为相同图像复核，不新增产品状态。各15真实clock非零、Part恢复max1、原失败账本、平台及完整P3/CT15继续保留。官方显式0模型NaN仍可复现，不改CT25/FW1，不授权提交。


2026-11-01 CT15 Spin循环：[结果](spin-cycle-results.md)、[308独立变化已获限定批准](spin-cycle-visual-review.md)、[作者自审](spin-cycle-review.md)。原12旋转/初始opacity/稳态FAIL，次正规模型7PASS/1FAIL，最小正规模型8PASS；实际36+8不同用例通过（28临时+既有16），保持原Alternate/Delay/曲线与Native合同。最终三版各2372帧，managed/AOT各3004检查PASS，旧2624PASS/380FAIL，完整无缺帧；2356受控/就绪/显隐/校准cross零，16真实clock非零保留。old-new592非零=308独立变化+268同帧复核+16真实时相；原1160就绪不足帧独立排除，内容/过渡未删除。28临时1源10258字节、9375取证、原469会话（driver412/publish15）和4最终文件清理；最终16PASS/0Skip（581ms，命令31.29s）、NET8零警告21.04s，成员1-1-0。137/274及Own未决3、此前46静态和P3-C96限定批准保持，CT12/13/14与CT25/FW1不改写，完整家族/CT15开放。下一按[Count退出保留](badge-count-retirement.md)继续，不提交。


限定批准登记：2026-11-01，用户明确回复“批准表内308对，其余边界保留”（request_user_input_async / call_122c5e1eef6b4d7ca6e0c98bd9c0c55b / item 0）。仅Spin循环表内308独立起点/端点/错峰/重新可见变化获批准，51行整帧等价合并及268同帧second复核不扩大产品状态。16真实clock非零、原12FAIL/模型及就绪坏批、其他恢复/平台/GC与完整CT15保留；原46静态/P3-C96批准不扩大，不授权提交。


2026-11-01 CT15 Count退出：[因果/执行](badge-count-retirement.md)、[结果](badge-count-retirement-results.md)、[72静态零变化核对](badge-count-retirement-visual-review.md)、[作者自审](badge-count-retirement-review.md)。原standalone保留adorner GC与新增layer target GC分别由公开composition成员滞后、public association清理时机定位；只在项目成员退出同步public关系及先清association后离树，不写private字段或改依赖。41及target35所选PASS，累计44不同用例（12临时+既有32）；正常三轮show/hide/重挂/模式往返和64×32原按钮左中右点击+Space到达。最终三版各88帧，managed/AOT各108检查含20normal回收全PASS，旧100PASS/8GC FAIL；72静态old-new/cross全0，无新增有意视觉变化，13旧新/16cross真实时相非零保留。12临时2源12902字节、304取证、原492会话（driver411/publish15）及4最终文件清理；最终32PASS/0Skip（759ms，命令30.58s）、NET8零警告19.66s，成员2-2-0。原57中Count GC失败保持历史，当前两已定位路径已修正，不关闭其他Badge/CT15边界；137/274、Own未决3、原48批准、CT12/13/14及CT25/FW1不改写。下一核对Collapse原回收失败与其他真实出口，不提交。


2026-11-01 CT15 Collapse回收复现核对：[结果](collapse-retirement-audit.md)。八不同严格NoInlining条件Debug/Release四批全PASS，原内容/520宽/120viewport900extent/motion及Global/Own/专用Style实际命中和资格退出保持；本轮未复现旧失败、不改产品、不把旧75中2GC FAIL或Debug/Release不一致倒写通过。8临时3源5529字节与14原会话清理，最终精确50PASS/0Skip（命令16.44s），复用相同源码NET8零警告证据。旧GC仍未定位，其他恢复/平台/原12视觉等待与完整CT15继续保留，无全量/Git。


2026-11-01 CT15 P2：[共享归属与几何核对](feedback-stack-p2-ownership-results.md)。正式架构和CT10已有归属将16/8及背板定义为共享产品几何，不推导未经批准的Spacing映射/新Own，不重复研发。无源变化，精确managerPadding/panelGeometry37PASS/0Skip（900ms，命令10.74s），临时会话清理；该终态几何门禁不替代真实host/正常motion/像素/其他业务平台。原Message/Notification非零与完整P2/CT15继续开放，137/274保持，无全量/Git。


## GroupBox三项兼容分类批准登记

2026-11-01 用户明确回复“批准三项 Reserved，其余边界保留”（request_user_input_async / call_c8732844a7a841caacc4385b24e13bd3 / item 0）。仅TextPaddingInline、OrientationMarginPercent、VerticalMarginInline归ReservedDocumented；保持名称/类型/默认/key/slot及原布局，无几何消费者，不标Active/ConsumerVerified。40对视觉仍待限定批准，恢复4/7/9/9 max1及其他家族/完整CT15边界保留，不授权提交。CT15 Own未决由3减为0，原137metadata/274Own及CT12/13/14、CT25/FW1不变。


## 六包限定批准登记

2026-11-01 用户明确回复“仅批准六表现有条目及列明纠正，其余保留”（request_user_input_async / call_08c452ba85d7442ab817b3cf6c7dcce6 / item 0）。仅Flyouts48对及表列两项有效默认纠正、Collapse12对、Descriptions16对、GroupBox40对、Statistic12对、QRCode16对获得限定视觉批准。各表列明的裁剪、低对比、原失败、缺帧、恢复与持续态、平台及完整家族/CT15均保留，不授权提交；GroupBox三Reserved另按call_c8732844a7a841caacc4385b24e13bd3/0既有批准，不扩大。


## Empty限定批准登记

2026-11-01 用户明确回复“仅批准 Empty 表内20对，其余保留”（request_user_input_async / call_630c3c2e47fe460594f8888610f2e347 / item 0）。仅Empty表内20对Global/跨scope字体颜色获限定批准；原32失败、19旧新/10跨运行输入持续态非零、原裁剪/低对比、平台/完整家族与CT15保留，不授权提交。此前没有引用明确回复的“已批准”摘要不作为批准来源，本条为明确登记。


2026-11-01 Result严格退出补证：六不同临时用例Debug/Release均PASS，包含provider/window持续保留、当前Presenter持父链正对照及释放、保留已替换caller glyph而owner退出；实际scope93/23、Native27×31和120viewport900extent保持。首次Debug2回收观测FAIL由断言暂存Presenter定位，NoInlining观测后4PASS；不改产品、不把原INCONCLUSIVE倒写为修复。单临时源6584字节已删除，清理后精确Result9PASS/0Skip、745ms；产品未变，复用相同最终源码NET8零警告。原384/18、其他恢复/平台/极值和完整CT15保留，见result-retirement-audit.md。


2026-11-01 P2原生宿主补证：native NSWindow960×720/scale2，managed/AOT完整各147及固定激活各148检查，六位置/变高gap16、原NoWrap全文/显式换行、Cocoa click/Space、五卡片hover/gap/leave、1500ms真实悬停剩余计时、四算法RTL通过。应用内合成Cocoa经Avalonia.Native，不冒充外部物理/其他OS输入。完整OS阴影画布随激活变化，跨运行像素不通过；正常入场初Frame与稳定绘制3497/max2差异定位，初始frame重绘触发、Window重绘无效，纯Border/淡入不复现，不归FW1或扩大容差。248取证/447原会话(411driver)/两AOT已清理，audit clean；清理后精确feedback padding/panel/motion与GroupBox70PASS/0Skip、1s，NET8零警告零错误21.10s。见feedback-native-p2.md；无反馈运行代码改变，GroupBox仅三Reserved XML说明同步。原失败、完整P2/CT15及CT25/FW1保留，不提交。


2026-11-01 Timeline P3 Part恢复定位：四算法/八专用Style32独立A-B-A，32AA0、实际命中/实例/几何恢复；8 icon/title恢复0，其他24严格非零。明暗item/rail采集值相同，背景根重绘均0。无AtomUI独立Avalonia12.1.4工程managed/AOT各20检查17PASS/3恢复FAIL，rail各14/max30、Opacity19/max14，15帧cross全0；公开开关试验未消除，不改产品/依赖/容差。23临时case/3源28395字节、446取证及339会话848665821字节已清理；最终精确Timeline52PASS/0Skip/1s，产品未变复用NET8零警告21.10s。FW2限定归属提案待用户批准，原P3-B102/82、FW1/CT25及CT12/13/14和完整CT15保留，见timeline-part-restore-audit.md、feedback-partial-redraw-visual-review.md，不提交。


## FW2限定归属批准登记

2026-11-01 用户明确回复“仅批准表列路径归 CT25/FW2，其余保留”（request_user_input_async / call_58d9ca164cbe4d43b4254cc21caa6b70 / item 0）。仅feedback-partial-redraw-visual-review.md列出的3个无AtomUI独立反例及24个当前Timeline局部恢复路径归CT25/FW2继续调查；非零仍FAIL，不设置或扩大任何像素容差，不修改既有FW1。原P3-B102/102/82/82、其他家族/native/平台/GC及完整CT15均保留，不授权提交。


2026-11-01 Message恢复定位：四配方×entry/shift/Own/Global的16独立场景，实际值/几何/四card身份恢复、AA0；managed/AOT各224检查220PASS/4严格零恢复FAIL，76帧66cross零/10非零，16完整背景重绘cross全0且每配方四条历史的重绘图彼此0。原正常初始/局部恢复仍非零，不把重绘当修复；新Message路径尚不在已批准FW2范围，原P1与native3497/max2均保留。4临时case/1源9518字节、331取证、427会话1068266516字节已清理，audit clean；最终精确Message Semantic/managerPadding17PASS/0Skip/1s，产品未变复用NET8零警告21.10s。见message-restore-audit.md，完整Message/CT15未关闭，不提交。


2026-11-01 Notification恢复定位：四算法/六卡/1280×900/384宽MaxHeight380及4/10公开输入保持，colour/global八A-B-A。所有数值/gradient stops/几何/原引用/命中通过；managed/AOT各128检查124PASS/4严格恢复FAIL，40原帧cross全0，各配方两完整背景重绘参考彼此0。原初始/局部恢复仍非零，旧两暗图4/max16不倒写；新两像素位置为卡片外缘/阴影，非进度输入丢失。不在原FW2批准表，未改归属。4临时/1源10908字节、174取证、427会话1107631045字节已清理，audit clean；清理后精确29PASS/0Skip/980ms，产品未变复用NET8零警告21.10s；见notification-restore-audit.md，完整家族/CT15保留，无提交。


2026-11-01 Modal原生输入/Tab修复：原生Overlay Tab进入owner input/opener，匹配原AtomUI Window宿主有效RED4项1PASS/3FAIL；仅在既有IsModal=True AXAML Style声明TabNavigation=Cycle，modeless默认Continue与热切换/嵌套LTR-RTL保持。7临时+276原模块283PASS；最终同源managed/AOT Dialog/MessageBox×Overlay/NSWindow各261PASS/exit0，Cocoa点击/选择/输入/Tab、async veto/Escape/重开Enter/恢复焦点、520×420Surface/140viewport/24行/23Menlo保持。旧原生偶发输入/焦点失败和shutdown观察者坏批留账，不声称全修。7临时/1源8644字节、38取证、425会话1125334263字节清理audit clean；清理后精确276PASS/0Skip17s、NET8零警告零错误19.98s。见modal-native-input.md；只有生产主题3行导航增量，原CT11 owner绑定保留，完整Modal/CT15和既定FW1/FW2边界不变，不提交。


2026-11-01 ToolTip/PopupConfirm原生出口：默认400ms hover、两种native popup/Overlay真实Root，Tooltip120/900与Confirm140/900原长文/箭头/两按钮保持；应用内Cocoa hover/leave、实际取消确认各一次、Escape light dismiss、已打开Global23→25均验证。最终同源managed/AOT各75PASS/exit0；首hover未打开原因未证实保留，初诊断反射IL2026产物未执行，移除后零IL/CS warning重发，无suppression。无产品源改动；17取证及439会话1220947773字节已清理audit clean，最终精确67PASS/0Skip/1s，复用当前相同源码NET8零警告19.98s。旧四颜色/Compact恢复、FW1/FW2原限与完整CT15保留，见tooltip-popup-confirm-native-input.md，不提交。


2026-11-01 Marquee正常周期补证：原220×28/Clip/全文及4×时长保持，default150/native450两组2临时PASS；AOT2874检查PASS/exit0，各两次实际回绕与offset有限/范围、隐藏600ms冻结/显示恢复通过。default实测周期13053.818ms，AOT13047.828ms；native450实测4374.079ms/AOT4373.230ms，期望13053.333/4351.111ms，误差界来自实际采样间隔，非像素容差。不冻结clock或作跨时相RGBA0声明。2临时/1源4618字节、16取证、303会话448595997字节已清理audit clean；最终精确原Marquee显隐模块1PASS/0Skip25ms（本轮不是旧10项），产品未变复用NET8零警告19.98s。原48/55持续态非零、4倍语义/0速度异常、平台/GC与完整CT15保留，见marquee-cycle-audit.md，不提交。


2026-11-01 Carousel正常周期补证：三临时用例PASS（45.1134s），default3000/native700真实页序均0,1,2,0,1,2,0；同源AOT38检查PASS/exit0。隐藏暂停/恢复/公开停止、有限末页与空集均验证，原caller-owned内容/120viewport900extent和normal motion保持。3临时/1源5375字节、5取证和310会话757588934字节已清理audit clean，清理后Carousel3项PASS（与Float46共49项/6.9191s）。无Carousel产品源修改/新视觉批准，原16旧新/4cross持续态、平台/极值与完整CT15保留，见carousel-cycle-audit.md。


2026-11-01 BackTop Target退出：两条NoInlining有效GC RED，唯一ConfigureTarget订阅条件限定attach后，三临时用例PASS/10.8843s，同源AOT21检查PASS/exit0；原120viewport900extent/450ms/400阈值/全文/滚动条、换目标/旧目标退出/reattach/Space回顶和之后可滚动保持。AOT46245192字节SHA256 bcacc57aff05811cd365287dc12aae28b662d9a175c31e7b281a9164c68c325a，零IL/CS、五既有Homebrew linker warning。3临时/1源5170字节及309会话757297643字节已清理，无新增媒体；清理后Float46+Carousel3共49PASS/0Skip/6.9191s；NET8最终源码构建中。仅修正这两条资源获取路径，不关闭原恢复/其他GC/平台/极值与完整CT15。见float-button-target-retirement.md，不提交。


清理后最终门禁：Float46与Carousel3精确共49PASS/0Skip/6.9191s；Desktop NET8 Release最终源码零警告零错误28.37s。全程共享run.lock非阻塞独占，无全量。成员1源/2控件0问题，确切原未提交快照到当前仅一个条件替换；注册/初始化/API/Token/布局均无增量。作者另步自审，非独立审查；订阅获取/退出与再次挂载配对，未改原动效模型或已批准家族。两条具体泄漏已修正，其他保留项不倒写。最终门禁helper/marker随读取清理，不自动提交。


2026-11-01 Tour共享mask退出：原两条严格GC FAIL，公开Dispose三绑定单变量使两条均释放，ClearValue无效留账。当前owner持CompositeDisposable并在close/detach/transfer释放，5临时PASS/20.5281s、同源AOT20检查PASS/exit0；重开/Step当前颜色/目标移动30/第二owner接管及旧owner不干扰均通过。5临时1源6866字节与311会话757466582字节已清理，无新增媒体；清理后实际Tour21+placement8共29PASS/0Skip，NET8零警告零错误17.61s、成员1/1/0。见tour-retirement-audit.md。只关闭当前共享mask持有链，不倒写旧缺72帧/18旧新/14cross非零或其他边界；原44限定批准/完整CT15保持，不提交。


清理后最终Drawer37PASS/0Skip/4.5875s，Desktop NET8 Release0警告0错误16.76s。成员1源/1控件0问题、git diff --check通过；相对此前未提交源码仅获取条件增量，无注册/初始化/API/布局改变。作者另步自审，非独立审查。最终helper/marker读取后清理，当前两具体泄漏退出通过；其他原恢复/定时/平台/极值与完整CT15继续。


最终结果：六不同场景Debug均PASS（原四12.4786s/新增二8.5300s），Release均PASS（四13.0868s/二7.4010s）；没有产品反例或源码修改，不把重复运行计为更多用例。6临时case/1源5368字节与7会话文件25204字节已删除，无媒体。清理后实际重建精确Statistic5+QRCode8+Empty13+Spin16+Marquee1共43PASS/0Skip/3.8264s；最终产品源码未变，复用Drawer最终NET8零警告零错误16.76s。git diff --check通过。作者另步自审确认provider/window保持存活与NoInlining观测，没有强持有被测对象；只补本六条对象退出，不关闭其他恢复/持续态/平台/极值或完整CT15。


四不同场景Debug4PASS/0Skip/17.7252s，Release4PASS/0Skip/17.5788s；实际cross root均ImagePreviewerDialog（当前Headless Window，不是NSWindow/Browser）。公开IsOpen/IsCurrentLoaded/cross root打开判据均通过；关闭移除或打开直接移除后root退出且owner可回收。调用方32×16 borrowed WriteableBitmap和ObservableCollection仍保持活着，原标题/Source不变，退出后调用方继续Add成功；不释放调用方图片/不清集合制造回收。

无产品失败/源码修改。4临时case/1源3877字节已删除，无媒体；清理后真实重建精确ImagePreviewer117PASS/0Skip/15.2674s。当前产品源码未变，复用同源NET8零警告零错误16.76s，git diff --check通过。最终会话/marker读取后清理，作者另步自审非独立审查。该四条本体GC不关闭旧171旧新/29静态及64持续态cross非零、平台/极值或完整CT15。


清理结果：当前10临时case/2源14466字节已删除；官方evidence cleanup clean/findings=[]、removed=true/550文件。外部111会话文件569551767字节（含独立工程/原失败与坏模型日志）及marker删除。清理后实际重建MessageSemanticPartTests13PASS/0Skip/3.7454s；首namespace误写Feedback而Padding实际为FeedbackStack，未把未匹配项计入，随后同一重建程序集精确Padding4PASS/0Skip/3.1545s，合计17不同既有用例。未改产品源码，复用相同最终源码NET8零警告零错误16.76s。最终helper/marker随读取清理，无新常驻测试、无全量或提交。

作者另步自审：纯模型默认图0只用于确认图元等价，独立裁剪错误已纠正并保留反证；不将未复现产品历史的模型解释为框架修复。原真实恢复仍FAIL，当前无新视觉批准或FW2归属请求，完整Message/CT15保持开放。


通知顺序补验已闭环：1有效RED原[1,0]→修复后1临时+21Tour共22PASS/0Skip/7.6984s；2390字节单临时源已删除。清理后真实重建Tour/placement29PASS/0Skip/5.7930s，最终源码Desktop NET8 Release0警告0错误17.41s，成员1/1/0与diff check通过。原AOT20明确保留为此前mask绑定增量证据，本次仅既有调用顺序/重复通知修正，没有新增AOT敏感路径，不冒称本次重新发布。最终会话/marker随读取清理。作者自审补验发现并纠正本轮回归，不影响其他既定批准/原失败、未关闭完整CT15。


最终清理后精确Timeline52PASS/0Skip/4.5166s，实际重建测试程序集；本轮无产品源码改动，复用相同最终源码NET8零警告零错误17.41s。1130取证经官方audit/cleanup clean删除，外部454会话文件1374532913字节、全部恢复的历史命令/驱动/发布产物与marker均删除；最后helper读取后清理。没有执行历史命令中的Git或旧源码替换，没有新增永久测试/全量/分支/worktree/提交。

作者另步自审：原主批64/440保持完整；组合/Content/排除Content与重绘分别分账，透明懒滚动条节点例外明确披露；208跨运行/96只读图全0不替代恢复失败。8路径限定归属表已完成工程、清理、精确门禁，等待用户是否交FW2调查的明确裁决，未自动修改原归属、FW1或完整P3/CT15结论。


## 原 P3-B 八路径限定归属批准登记

用户明确回复“仅批准表列8路径归CT25/FW2，其余保留”（request_user_input_async / call_cd2233cdfe1848fc889d777f91b52754 / item 0）。仅timeline-original-part-visual-review.md列出的4条原P3-B组合恢复102/102/82/82及4条原值Content0.7恢复124/102/98/82交CT25/FW2继续调查；非零仍FAIL，不增像素容差、不改既有FW1，不把透明滚动条懒节点差别抹去。其他原故障账本、家族/原生平台/完整P3与CT15保留，不授权提交。此前3独立模型/24路径批准范围不自动扩成其他家族。


最终清理后精确MessageSemanticPart13+FeedbackManagerPadding4共17PASS/0Skip/4.5267s，实际重建；本轮无产品源码变化，复用相同最终源码NET8零警告零错误17.41s。1191取证与795会话2176410897字节已清理audit clean，最后helper/marker记录后删除；没有新增永久测试、全量或Git操作。作者另步自审，非独立审查；原动效/内容/滚动/选择/关闭合同未改，独立模型与真实主批限制明确分账。仅34条有限调查路径提案待用户裁决，完整CT15及其他排除项保持开放。


## Message 原历史34路径限定调查批准登记

用户明确回复“仅批准表列34路径归CT25/FW2调查，其余保留”（request_user_input_async / call_8107eb29ed024cfbb7dd7fd40dec2aed / item 0）。仅message-original-visual-review.md表列28当前业务恢复、4独立检错路径及2无AtomUI队列重排反例交CT25/FW2继续调查。模型未精确复现原全部像素的限制保持，不宣称完整框架根因；非零仍FAIL、不增容差、不关产品动效、不改FW1。原native3497、其他36跨运行非零、旧失败批、其他家族与完整CT15均保留，不授权提交。


清理后精确Card20PASS/0Skip/4.7581s，实际重建测试程序集；本轮无生产源码变化，复用相同最终源码NET8零警告零错误17.41s。官方cleanup910取证、外部790会话2173296792字节与全部恢复/诊断/发布源码已删除，marker删除；最终helper记录后清理。作者另步自审：原88图完整，单变量clear范围限制已纠正、Frame重录因果与独立差分精确对应，严格4FAIL未改为PASS；无永久测试/全量/分支/worktree/提交。

仅card-original-visual-review.md表列40路径提出FW2继续调查，尚待用户明确裁决。原312/110非零仍FAIL、旧新Global完整对比与其他原边界/完整CT15不变，不将当前调查当整个家族关闭。


最终清理后NotificationCardTheme/NotificationSemanticPart精确29PASS/0Skip/4.4735s，实际重建；本轮无生产源码变化，复用相同最终源码NET8零警告零错误17.41s。313取证经官方audit/cleanup clean后删除，外部438会话1374918333字节、全部历史恢复/原驱动/发布文件与marker删除；最终helper记录后清理。作者另步自审确认旧新两对与候选自身恢复/后续新ABA4FAIL不混报，没有以新绿倒写原账本。仅两对旧残留消失的限定接受待用户裁决，完整CT15继续。


## Card 40路径限定调查批准登记

用户明确回复“仅批准表列40路径归CT25/FW2调查，其余保留”（request_user_input_async / call_c8333768d41d4ba692fea52d54ee7dea / item 0）。仅card-original-visual-review.md表列原32恢复、4同值Frame重录及4独立图元路径交CT25/FW2继续调查；非零仍FAIL，不增容差、不打产品重绘补丁、不改FW1/官方依赖。原旧新Global非零、其他边界与完整CT15保留，不授权提交。


## Notification 原两对恢复限定接受登记

用户明确回复“仅接受表列两对恢复差异，其余保留”（request_user_input_async / call_fdb5ce1da6f04f16b707b0e5d6b02d29 / item 0）。仅notification-original-visual-review.md列出的原Dark/Dark+Compact旧新global-restore各4/max16，作为旧版清除残留消失、候选回到共同默认的两对差异获限定接受。不宣称渲染根因已定，不把历史FAIL改写成当时PASS；后续新ABA4FAIL、平台/其他边界与完整CT15保留，不授权提交。


当前续作清理后Notification精确29PASS/0Skip/4.3151s，实际重建测试程序集；无生产源码变化，复用相同最终源码NET8零警告零错误17.41s。4临时理论/单源11785字节、460取证、779外部会话2174450601字节和marker已删除，audit clean；最后helper记录后清理。作者另步自审确认原正常4FAIL、no-motion3FAIL、独立模型7FAIL均保留，不把独立复现或跨运行0当像素PASS。18路径有限调查提案待用户裁决，未自动纳入FW2；原两个旧新差异的接受、其他家族/平台/完整CT15保持各自边界。


## Notification 新 ABA 18路径有限调查批准登记

用户明确回复“仅批准表列18路径归CT25/FW2调查，其余保留”（request_user_input_async / call_3c0dd46bb06948bfa8d56cec286a3aff / item 0）。仅notification-aba-visual-review.md所列8真实ABA、3真实初始静态重绘、4纯入场、3纯静态交CT25/FW2继续调查；非零仍FAIL，不增容差、不关产品动效、不加产品重绘补丁。两暗色独立入场各差1像素的限制保持，不认定原全部框架根因；其他边界、FW1/官方依赖与完整CT15均保留，不授权提交。
