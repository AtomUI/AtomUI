# CT15 ToolTip / PopupConfirm 当前消费者结果（2026-10-31）

范围3 metadata/13 Own；输入仍137/274，三项历史字段已按实际资源/布局/动效验证登记为Reserved，当前NeedsDecision8。单agent在指定原目录feature/custom-theme执行，无分支/worktree/提交。CT12/13/14既定关闭、CT25/FW1原始问题、官方Avalonia12.1.4保持。

## 当前工程

ToolTip 6 Own与PopupConfirm 7 Own迁DependencyDefaultsV2 Revision1，四算法52默认/type/key/slot不变。ToolTip五Active，MotionDuration保留历史资源，实际动效仍Owner.Global.MotionDurationMid；PopupConfirm五Active，PopupMinWidth/PopupMinHeight保留原240/80资源，不施加新布局下限。明确Own/0/透明/清除及空描述的actions margin覆盖保持。

ToolTip根承接原可见字号14/浅色文本角色，原生字体/前景进入实际文字，FontFamily保留真实宿主继承或明确终端Global；内部ArrowDecoratedBox不遮挡公开owner角色。确认容器复用真实PopupConfirm owner代理与Core字体继承机制，正文/标题/状态glyph按角色消费明确Global，原生用户Control内容保持。两个实际跨根内容复用既有ThemeContextResourceBridge，logical attach/context变化建立、detach退役，不新建scope或调度机制。

ToolTip的弹层绑定与当前part订阅分开持有，换模板退役旧part，热TipHostWidth取代一次性快照，箭头/文本布局向新part应用当前选定来源。颜色订阅与取值使用同一打开时选定source，重开重选；Preset优先Color，临时覆盖帧保留底层Native/动态资源绑定，清除/关闭/模板退役释放。Closed通知者仍能读取当次宿主，清理后不通过AdornedControl反向保留旧host。

模板重建的真实Skia暴露共享ShadowsAwareContainer仍绑定旧Border/Arrow：新container圆角17而frame变0，阴影消失。现有订阅集改为每次TemplateApplied重取当前mask几何并释放旧绑定；不改shadow数值/裁剪/尺寸/输入。该共享文件纳入本包前源码回放，不改写其他家族批准。

## 证据与裁决

首轮10项6有效FAIL/4PASS；只加资源发布桥后4FAIL/6PASS，证明资源查询已新而实际值仍旧的通知缺口。字体与模板订阅修复后10PASS；13项扩展中TipHostWidth快照新增有效RED，修复后与59既有联合72PASS。V2四算法52记录逐条相同。

生命周期5项初始1FAIL/4PASS：保留provider下两关闭图、旧箭头回收与专用Style声明已通过；复用Tip反向保留旧host修复后5PASS。实际框架Enter确认/取消、各一次事件/关闭2PASS，原图标默认→自定义→null机制本已PASS，未重复研发。

颜色热更新与原Native Purple恢复新增有效RED；覆盖期间资源更新Orange、清除后揭露Orange且随后Navy仍更新，两项PASS。原选源合同新增回归也取得RED（原host变Green误取新实例Blue），现复用同一固定source，重开才重选。共享mask几何17→frame0取得有效RED。最终第一组合计84PASS/0Skip（25临时+59既有，8s）；修正过滤器后既有Popups.PopupShadowTests另8PASS/0Skip（696ms）。这些是有执行记录的两组，不冒称单次92。

四文件初始78成员问题通过方法/region纯整理归零，类型/成员/初始化指纹相同、using集合相同；共享文件另有原21问题按原注册/字段初始化顺序整理。采样后最后一处backing field邻接已纯结构整理，保留原注册/字段初始化序列；五文件/四Control最终0问题，与采样前成员/类型/初始化快照精确相同。

## 首轮和新版Skia

Headless平台+实际Skia、原region dirty clipping、1200×900/scale1，正常Motion及Dispatcher，保留两个ToolTip、完整确认标题/描述/两个按钮、固定140高/180宽ScrollViewer、可见竖条和24行原文。四配方×30阶段=120主帧，每帧至少700ms+12相同像素帧、超时5s。公开scroll.Offset只证明API路径，不冒充原始滚轮；当前明确使用Overlay，不冒充OS原生Popup输入。

首轮92主检查通过但模板后阴影消失，后续恢复2114–2120/max27，另颜色恢复每配方2/max1；全部保留。独立四组A/A与1DIP恢复0、检测4581–4591/max189–216。修复后重新冻结源码/完整重跑，120帧已完成；阴影/模板恢复归零，仍有四颜色恢复各2/max1、Compact取消按钮恢复1/max1未通过。未扩大容差/剪裁/对齐/遮罩，旧版与AOT同完整历史对照仍进行中。

## 尚待工作

旧版/NativeAOT精确对照、共享纯结构整理、临时源码清理、精确模块与NET8已完成，具体数据见后续最终登记；媒体/工程清理及限定视觉状态按最终登记维护。完整家族/真实OS Popup/共享框体其余Global与其他家族仍开放，不以默认迁移或本包工程通过关闭CT15。

## 作者另步审查记录

已按本包前快照逐文件复读七产品文件增量。未改变PopupConfirm原default/custom/null图标工厂和事件/按钮尺寸合同；试探“ClearValue会永久破坏图标relay”的假设被有效动态图标用例否定，没有据假设重写该机制。新增绑定只有运行时附加来源/布局能力无法等价放入ControlTheme的部分，已给具体注释；稳定字体/角色值在AXAML，专用Part定制均为生成类，code-behind仅加载fixture。Scope桥、颜色覆盖帧和part订阅有对应退役与GC证据。

默认/Global/Part关键图像已查看：正文自然换行、标题/描述/两个按钮、固定高度富内容及可见竖滚动条仍在；新阴影正常恢复。Global字号23和专用Style增大确认框自然宽度，右侧仍按原popup容量约束，原描述及按钮未被本驱动删除/缩短；本场景不代替任意极值/OS输入的家族出口。用户自定义Navy/Purple等实际配置原样呈现，不以换表面/换色掩盖边界。作者自审不是独立评审，无子agent；当前未申请完整CT15关闭。

当前旧新完整对照已完成：候选92检查0FAIL，旧版92检查52FAIL；120主对48精确0、68有意候选与4颜色恢复各2/max1分列。Compact取消恢复的1/max1在旧版与候选相同坐标同现，原始未通过保留。根getter最终对照见下节：字号始终14，仅Foreground角色归位；四默认旧新精确0。

## 最终证据与清理登记

managed/NativeAOT各92主检查0FAIL；旧版92检查52FAIL。JIT/AOT120/120主对精确0，旧新48/120零、68有意与四颜色恢复各2/max1分列。旧/候选/AOT各四组独立A/A与1DIP恢复全0，检测分别4589/4581/4591/4583像素、max216/216/189/189，三版本逐配方数字相同。NativeAOT无IL警告，保留既有五条Homebrew库macOS26/target12 linker警告，未更换官方依赖。

根getter专项是独立属性观测，不把零业务用例分支记为测试通过。实际旧版/候选四配方Root FontSize均14；Root Foreground由明色#e0000000/暗色#d8ffffff改为原实际正文White。原实际文字旧新14/White且默认像素0，只申请此根前景归位，不申请不存在的12→14变化。辅助观测只增加外部driver的values分支，未改主采样操作；原目录16源finally恢复后与最终候选16/16文本精确相同。

25临时用例的4份源共35911 bytes已删除，无新增常驻用例。清理后精确filter：Tooltip namespace、PopupConfirm namespace与Popups.PopupShadowTests；67PASS/0FAIL/0Skip，测试Duration2s、命令38.766s。NET8 Release零Warning/零Error，18.68s（命令18.874s）；成员5文件/4Control/0问题，最后纯结构整理前后快照完全相同。所有构建/采样先持共享run.lock，无全量回归。后续getter只读回放未改变最终源码内容，Debug模块验收与NET8证据按相同输入复用，不重复扩大测试。

取证包含首轮失败与最终三版本，共544 PNG/544树记录/manifest，1089文件，清理审计及外部工程总数在最后登记。正式两家族十文档、共享Popup实现/changelog、CSV/总表同步。当前限定表68对及根前景归位已获用户限定批准；四颜色恢复和Compact业务恢复微差仍未通过。完整共享框体/OS Popup/其他平台和其余家族继续，不以本包关闭CT15。

最终外部清理：driver工程434文件；整会话材料551文件（包含工程、原始非零/首轮/临时源备份及辅助getter观测）已在保存文字数值后删除，marker同时删除。1089媒体已清理，审计clean且无Git候选；十正式文档及共享Popup两文档相对链接核对通过，Git diff --check通过。未提交。


限定批准登记：2026-10-31，用户明确回复“批准表内68对及根前景归位，其余未通过与完整CT15保留”（request_user_input_async / call_a541f8393ca141a9afd55def80071fb0 / item 0）。仅表内68对与ToolTip根Foreground角色归位获限定批准，根字号原本14不变；四颜色恢复各2/max1、两版Compact同坐标1/max1及完整家族/CT15/OS边界保留，不授权Git提交。


2026-11-01 ToolTip/PopupConfirm原生出口：默认400ms hover、两种native popup/Overlay真实Root，Tooltip120/900与Confirm140/900原长文/箭头/两按钮保持；应用内Cocoa hover/leave、实际取消确认各一次、Escape light dismiss、已打开Global23→25均验证。最终同源managed/AOT各75PASS/exit0；首hover未打开原因未证实保留，初诊断反射IL2026产物未执行，移除后零IL/CS warning重发，无suppression。无产品源改动；17取证及439会话1220947773字节已清理audit clean，最终精确67PASS/0Skip/1s，复用当前相同源码NET8零警告19.98s。旧四颜色/Compact恢复、FW1/FW2原限与完整CT15保留，见tooltip-popup-confirm-native-input.md，不提交。
