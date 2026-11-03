# CT15 Badge 消费者计划（2026-10-31）

承接[Avatar/Tag/Badge核对](avatar-tag-badge-consumers.md)，输入13metadata/20Own（Count11、Dot5、Ribbon4）。Tag仍在持锁AOT取证，本阶段只读实际源/五正式文档和准备反例，不并发构建、不改Badge产品源。137/274不缩，CT12/13/14批准、CT25/FW1与官方依赖保留，FloatButton只复用CT11动效归属，不借Badge关闭交叉家族。

三个public owner没有自身ControlTemplate：Count/Dot standalone为普通子树，target mode的visual parent为AdornerLayer而logical/style owner必须保持原Badge；Ribbon始终inline。生成的CountBadgeIndicatorStyle、DotBadgeIndicatorStyle、RibbonBadgeIndicatorStyle、RibbonBadgeContentStyle是唯一定制入口；不以取节点赋值替代route，不把内部Adorner或DecoratedTarget升级为public Part。

先验证Count两圆角默认依赖有效Own.IndicatorHeight/SM与显式CornerRadius优先，V2R1保持80条四算法默认/type/key/slot。其他18字段逐实际paint/measure/layout消费者核对，Ribbon Offset两个坐标、immutable CornerTransform、Darken15与三角折角合同保持，不因兼容分类擅自丢失绘制。

| AC | 驱动与独立预期 |
| --- | --- |
| B1 默认/依赖 | Count11+Dot5+Ribbon4四算法80记录相同；有效Own高度影响默认角、明确角覆盖；0/clear按实际字段合同 |
| B2 owner/字体 | 三public owner和实际Adorner/indicator的Global、原生字体/前景、Hot配置与资源发布到真实目标，不误用内部独立Global |
| B3 专用Part | 声明式四Style覆盖standalone/target、Count/Dot跨根与Ribbon inline；marker数量/owner隔离，不穿透DecoratedTarget内嵌Badge |
| B4 原业务 | Count负值归一/0/ShowZero/Overflow/极值/大小档；Dot完整Status/Processing持续动画/显式颜色与切模式；Ribbon Start/End/目标/长短文/完整折角 |
| B5 生命周期 | 显隐与退出取消、祖先可见、换目标/重挂/延迟AdornerLayer、旧目标/Adorner/owner绑定释放，DecoratedTarget内容保持 |
| B6 视觉/出口 | 相同完整历史Skia/AOT、Compact/RTL/代表极值、正常持续动效单独轨迹，独立A/A/1DIP无通用容差；清理后精确模块/NET8/文档CSV总表/作者自审，限定批准后仍保留完整CT15 |

当前仅初始合同与顺序，不表示20Own已迁/通过。既有Semantic、Ribbon fold、原生Adorner与motion模块证据先复用，实际变更的精确验证仍必须执行；不自行全量回归、不创建分支/worktree/agent或提交。

首轮11项5有效FAIL/6PASS（9s）：Count Default明确高度42但角仍20；Small明确27但角仍14；Count Global FontFamily Menlo仍原字体；Dot Success明确Orange仍#ff52c41a；Ribbon Global表面/文字/角未到目标。明确Corner0优先及清空、Native TextElement附加字体与完整DecoratedTarget保留、三identity四算法80默认/type/key/slot导出通过。尚未改产品。

圆角依赖复用既有ControlTokenDefault的Own参数机制（Dialog.MinHeight先例），不增加context.Own新API。内部Adorner Globals复用既有IControlThemeGlobalOwnerSource公共owner代理，目标自身没有ControlTemplate/常规Font属性，不凭空添加新公开字体API；原生附加TextElement字体的实际继承合同保留。SharedLabel/Indicator查找仅观测，不以代码写Part属性定制。

首修复后11PASS/0Skip（9s），三token默认V2R1及两Own依赖、Adorner公共owner代理使原5反例通过。补Native附加Courier+Global Menlo组合取得候选回归：实际Menlo覆盖Courier，不能拿初轮11通过关闭。为保留Avalonia真实原生优先级，将字体Setter移到三个public owner的无模板ControlTheme（复用原theme资源文件），用TextElement附加属性表达；内部Adorner只继承有效owner字体并复用Global颜色代理，不用新flags/旧Local恢复系统。原Adorner字体方案已另存快照以便单变量核对；当前12项重验与80默认导出进行中。public owner依旧没有ControlTemplate，不改变DecoratedTarget内容/状态/布局。

public owner无模板字体Theme方案12PASS/0Skip（10s），Native Courier优先、clear回Global Menlo，原11项继续通过。下一补真实DecoratedTarget/跨根、20Own与专用Part的动态和退出合同；public owner字体Style是否影响原目标的继承布局也要实际核对，不能只凭12项关闭。当前80默认导出对照见会话材料，完整Skia/AOT与清理后模块仍未完成。

16临时+32既有Badge共48PASS/0Skip（15s），Count11/Dot5/Ribbon3 Own实际消费者、附加Native目标字体隔离均通过；之前BoxShadows误用LINQ.First是临时编译错误，已按真实indexer修正，不计产品RED。Ribbon第四Own CornerTransform补以现有Matrix codec的1,0,0,0.5,0,0验证真实Value，不能以默认ToString仅输出类型名冒充矩阵值完整验证。

加入四个生成专用Style后49项48PASS/1有效FAIL：Count standalone indicator Width应53实际NaN；定位器已限定真实Count/Dot Adorner，排除同名节点误判。实际standalone逻辑父为辅助Panel，破坏公开direct-child route；target mode仍public owner。修复仅Count/Dot standalone的逻辑/style owner及对应child membership：保留Panel视觉挂载级联、先public owner logical attach，再Panel接管visual，退役同时清owner membership；Loaded兜底不视觉重挂，避免取消现有show motion。route/ContractType不改、不复制Style/不以代码定制Part。当前49项重验中，未提前宣布通过。

逻辑owner修复后17临时+32既有共49PASS/0Skip（15s），四专用Style在Count/Dot standalone+target与Ribbon两模式均真实命中；Ribbon第四Own immutable矩阵0.5与80默认精确比较通过，不依赖类型名ToString。当前新增target mode同一Adorner热Own/Global、三类保留detach Adorner的owner/Scope弱引用退出验证，正常motion等待700ms后判定，未为取证关闭动效。完整子族仍不关闭。

退出补验53项49PASS/4有效FAIL（21s）：target mode同一Count Adorner热Own后height仍20而非42；三类保留detach Adorner时owner均未回收，已等待700ms实际时钟，不是提前判断动效。针对两类根因分别复用资源发布bridge与typed投射句柄：Count/Dot Adorner在logical attach/context变更建立既有ThemeContextResourceBridge、detach移除dispose；三public owner把原Indexer投射改持有CompositeDisposable，detach与Dot模式reset释放，attach按当前状态恢复。保持原Local准备合同，代码说明AXAML不能等价的时机/优先级。不新增全局引擎、flags或公开API。首次编译漏AtomUI.Theme using，已补齐，仅为实现错误，不计产品RED；重验进行中。

bridge与句柄首轮重验53项52PASS/1FAIL（21s）：target热Own/Global与Dot、Ribbon旧owner/Scope回收均通过；仅Count退出仍保留owner。没有把52局部通过当家族完成，继续定位Count独有的show-ready/动效callback或其他引用，不增加delay或关闭motion。当前80默认与Part四Style继续保持，尚无Badge视觉批准。

Count独有路径审查：standalone HideAdorner(false)最终设置owner.IsVisible=false，再进入OnPropertyChanged的HideAdorner(IsMotionEnabled&&IsLoaded)；detach阶段IsLoaded可以仍true，导致无clock的隐藏任务重新排队并捕获owner。增加仅在当前真实visual attach时允许退出motion的计算守卫，attached正常退出/旧数值保留不变；无新状态字段。以原Count GC及整个53项验证该根因，未将代码推断当最终通过。

Count附加guard A/B后仍同一GC FAIL，未证明它是本保留问题根因，已撤回该产品guard，不留推测性触发点补丁。继续临时只读强链扫描（跳过弱引用/反射元数据）定位Count真实保留边；Dot/Ribbon及target更新已通过的结果保留，不据此提前关闭。

Count实际强链（临时只读扫描）为CountBadgeAdorner→MotionActor.Presenter→Panel→inheritance child Border→Border._borderVisual→CompositionVisual._parent若干层→CompositionDrawListVisual.Visual(CountBadge)。不是仍存Indexer投射，Dot/Ribbon释放已通过；Count假设guard撤回。此为本Count出口的未通过记录，不自动归入/扩大CT25或FW1、不改其原问题/批准边界、不替换官方依赖、不写反射去清私有composition parent。后续Skia/AOT与逐状态证据继续，本项不能算已解决，完整Badge/CT15不关闭。

当前53所选52PASS/1 Count GC FAIL，另临时三个GC单选2PASS/1FAIL。继续成员归档、完整业务/跨根/持续processing动效与最终限定视觉工程；不在此向用户询问是否继续。

成员归档22C#源/7注册Control有69项区域/顺序问题，按原声明体纯整理。初次脚本未考虑DotBadgeIndicator原文件首BOM被Roslyn读取时剥除而Python保留的offset差，遗漏一枚class开括号导致语法检查失败；已只修正该括号/编码，其他6源完整快照已相同，不作为产品RED。继续审查完整成员/注册/初始化指纹，未以语法错误快照宣称通过。

成员22源/7注册Control从69问题纯整理至0，完整type/member/注册/初始化/导入指纹逐条同。整理后53项仍52PASS/1 Count composition链FAIL，未把失败丢弃。当前准备固定1000×1050、四算法实际背景、真实Skia/正常motion，standalone+target六主owner和四Style两模式并列；所有20Own/依赖/0/clear、Global与Native优先、完整Count/Dot/Ribbon业务、RTL/祖先显隐/重挂、目标键鼠及极值，相同历史旧新/AOT；独立A/A/1DIP。Processing现有源只实现状态颜色，无专门循环pulse，采样真实行为，不为通过凭空新增持续动效或关闭原show/hide；持续时间轨迹另外记录实际帧。

最终当前候选22C#源/7Control成员0，完整纯整理指纹同，源26文件快照已保存。临时53项仍52PASS/Count composition链1FAIL原样保留。外部Probe Release构建0警告0错误（18.12s），已进入四算法实际Skia候选/独立校准，750ms+12等帧/6s上限、1000×1050、完整目标文本/四专用Style及原Badge动效开关均保持；尚未取得视觉结论，不提前将Count出口失败计入通过。

候选首轮真实Skia主64检查4FAIL（四配方target-pointer均只见既有keyboard计数1，鼠标未新增），独立校准4检查0FAIL。主场景保留180×90完整Button目标、Ribbon原位置/文本/裁剪，未为让输入成功挪开目标或去掉Overlay。按AGENTS输入几何纪律，新增原区域left/center/right三点真实指针与完整hit/ancestor/bounds/clip日志，先定位再改事件/命中。此4FAIL不能隐藏或按视觉局部通过关闭；first media保留，后续修复须全版本同历史重放。

Ribbon指针三点实际hit均为Window的全区Panel，未命中Ribbon Adorner，不能凭“看起来被覆盖”直接修改Badge.IsHitTestVisible或事件逻辑。保留180×90与原文本，追加同一Button移除Badge的单变量A/B判断是否为裸Avalonia Button缺少产品Theme/hit surface；先证因果再改，不以换点/去限制消差。

单变量移除Ribbon后，原裸Avalonia Button中心仍hit Window.Panel且click delta0，否证Badge覆盖因果。该宿主未带原生Avalonia Button Theme，键盘事件可走而没有完整鼠标命中表面。临时目标改同180×90/同文本的AtomUI Button，不修改Badge hit-test/事件/height/clip；新旧/AOT均将采用同一产品目标与完整历史重放。原候选4FAIL/独立校准及完整媒体354文件前缀pre-input-host保留，不按产品bug或已通过记录；指针三点重验进行中。


产品Button三点复验1PASS（1s）：原180×90/原内容/Ribbon位置不变，三点均沿目标TextBlock→ContentPresenter→Button→RibbonBadge命中，实际3次click。无需修改Badge hit-test或事件；候选与旧新/AOT渲染统一改相同产品目标全轨迹重放。原裸Button4FAIL批与A/B失败记录保留，不能当产品修复。

产品目标统一重放当前候选主64/0FAIL，独立校准继续。为区分原默认和用户Part Style修复，补独立plain recipe：六Fixture owner在attach前移除用户ct15 class但保留所有原内容/目标/尺寸与标准semantic marker，只捕获无定制默认整帧；主轨迹动作与内容不变。三版本均补同一recipe，不能把含专用Style的“default”帧误宣称无定制默认0差异。首次host失败材料前缀354文件保留。

产品目标统一版本候选主64/0FAIL，校准4/0FAIL，原裸Button4失败完全分开保留，不改Badge事件逻辑。当前旧源同历史及AOT运行继续，plain无定制独立recipe和原专用Style命中修复分别记录；所有默认/Global/Part/资源退出结论仍按实际范围，不以局部through值替整包。

产品Button统一轨迹：候选主64/0FAIL、独立校准4/0FAIL；旧源主64/16FAIL、校准4/0FAIL，旧失败集中同target热Own、两高度→圆角依赖及公共owner字体。旧源26文件finally恢复候选完成，AOT构建进行中。另plain默认recipe将在三版本独立补齐，原“default”含用户四Style，不能混称无定制默认。Count composition链资源退出1FAIL和原Pixel边界继续保留。

首轮产品目标managed/AOT主各64/0FAIL、独立校准各4/0FAIL，AOT plain4/0FAIL。恢复像素显示colors-clear与最终business不回原帧；检查实际实例颜色方法：Count/Dot把CalculateColor(null)的null永久Local写入，Ribbon null/无效输入保留旧Local。新增三族同Adorner颜色clear→原Token独立反例；先获得有效RED再修复，不把颜色恢复问题当微差豁免。当前plain旧portable分支单独补验，Root源仍候选26快照相同。

实例颜色clear三族独立RED：Count/Dot underlying brush成为null（不是Token），Ribbon仍#123456而非原#1677ff。改为可释放instance color frame（复用原CalculateColor/preset映射，保持明确实例颜色优先），不覆写底层AXAML token binding；clear/无效输入dispose后恢复原token，detach释放、reattach/重建按当前public值恢复，Dot reset同步释放。无新公开属性、无全局状态。当前含三颜色反例及产品目标geometry的57项精确模块重验中；原Count composition链FAIL继续保留。

颜色frame首次编译触发nullable-as-error：reattach传递公开null颜色，原private-protected参数string注解不准确；改为string?表明既有clear语义（不改CLR签名），不抑制警告。该编译失败不计产品RED；仍以三独立clear反例、全部57项和Count未通过边界判结果。

颜色修复57项56PASS/1 Count composition链FAIL（25s），三原clear反例已GREEN，产品Button三点目标测试PASS；80默认保持。成员最终22/7/0，颜色功能修改单独记录，不伪称与功能修改前全快照相同。原颜色失败三版本资料1080文件已前缀pre-color-clear保留，原host354文件也保留；将最终候选、旧源、AOT主/校准/plain三recipe同历史全重放，未挑旧批零帧替代。


最终处置：2026-10-31 CT15 Badge当前切片：[结果](badge-consumers-results.md)、[48对限定视觉已批准](badge-consumers-visual-review.md)、[作者另步自审](badge-consumers-review.md)。13metadata/20Own全Active V2R1、80默认/type/key/slot保持；真实owner字体/跨根发布/专用Style/实例颜色clear与投射退役修复。57所选56PASS、Count composition GC1FAIL保留；managed/AOT各72检查、164跨运行全零，4无定制默认旧新零，显隐/重挂及校准恢复非零不豁免。25临时用例3源26658字节、2524取证、419driver/15publish（原会话共537文件）已清理，清理后既有Badge32PASS/0Skip及NET8零警告零错误23.02s，成员22-7-0。NeedsDecision7与137/274不变；仅申请48对明确变化，完整Badge/CT15和CT25/FW1边界保留。下一按实际依赖推进ImagePreviewer/Carousel/Result，无Git提交。


限定批准登记：2026-10-31，用户明确回复“批准表内48对，其余未通过与完整CT15保留”（request_user_input_async / call_d223a18435b5484787082d74b8ee393e / item 0）。仅Badge表内48对有意变化获批准；Count composition GC1FAIL、显隐/重挂/校准恢复非零、其余108对及完整Badge/CT15继续保留，不授权提交。


后续Count退出：[独立结果](badge-count-retirement-results.md)已分别定位standalone composition成员和layer association时机，两源最小修正、normal motion前失败后通过、managed/AOT各108检查及72静态零变化、清理后32/NET8零完成。原57中GC1FAIL及后续8原版FAIL不抹除；此仅修正这两个限定保留路径，不改原48批准或其他恢复/平台/极值/完整Badge与CT15边界。
