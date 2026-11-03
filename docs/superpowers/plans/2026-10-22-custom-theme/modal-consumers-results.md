# CT15 Dialog / MessageBox 当前消费者结果（2026-10-31）

范围Dialog 11 metadata/17 Own、MessageBox 2 metadata/2 Own；137/274输入与当前NeedsDecision11不变。指定原目录feature/custom-theme单agent，保留其他未提交改动，无分支/worktree/提交。CT12/13/14关闭与CT25/FW1、官方Avalonia12.1.4边界保持。当前为限定消费者切片，完整Modal/CT15未关闭。

## 工程与合同

- 19 Own迁DependencyDefaultsV2 Revision1，全部保留实际Active消费者；四算法76条名称/type/key/slot/原值逐条一致；本包19实际资源key的兼容证据包含在上述76条中，未修改资源key生成机制。Dialog.MinHeight声明有效HeaderPadding为强类型上游：ControlHeightLG+HeaderPadding.Top+Bottom+UniformlyMarginXS，明确MinHeight（含0）仍优先。例ControlHeightLG41、margin7、HeaderPadding=1,20,3,4得到72。
- Surface与OverlayPresenter复用IControlThemeGlobalOwnerSource指向实际公共Dialog或MessageBox；Global资源按真实锚点解析，internal Surface不作为未注册public identity回退key。未挂载阶段安全Shared回退，进入真实树后重新解析；不增scope/identity，不改CT11资源桥、逻辑成员或ThemeContextLease。
- Surface可选正文FontFamily/FontSize/ColorText复用Core有限真实继承源和存在位选择；未配置保持实际Host字体。标题仍HeaderFontSize/HeaderColor，独立按钮保留各自角色。模板可达绑定全部AXAML；全局圆角/阴影、header间距/字重、caption颜色/尺寸、mask和MessageBox状态glyph按真实终端owner消费。
- Dialog/MessageBox root是零尺寸会话控制器，不把继承的FontSize/Foreground等新增为Host转发API。旧版未生根controller为$Default/12/Black，Window实际Host/Surface为系统字体/14/#e0000000；保留实际原生继承。原内容和生成的专用Part承担当层原生定制，Host*及结构下限、按钮/关闭/焦点/键盘/滚动/裁剪合同保持。

本包产品增量九文件：Surface、OverlayPresenter、两Token与五AXAML；两个控制器的成员纯移动/region整理之外没有修改Session/调度/拖动resize算法。原目录回放56源，finally恢复后56/56文本相同。C#实际触及四文件/两Control，成员审计0；纯方法移动指纹、字段与注册初始化顺序相同，未触及文件的既有成员问题不扩大处理。

## 有效验证与探针纠正

初始7项3FAIL/4PASS包含一项“控制器字体必须转发”的错误合同假设，只有真实Surface Global和MinHeight上游两项是有效RED；扩展11项7FAIL中三项Global/一项依赖有效，另外三项仍属转发假设。正式semantic-part root合同核对后撤销该假设，实验relay/diagnostic/SerialDisposable helper全部删除。官方同值PropertyChanged边界只记实验观察，不扩入FW1或新增产品旗标。

最终第一组21临时+276既有精确namespace共**297PASS/0FAIL/0Skip**（测试报告Duration22s；前一16临时组合292PASS/23.4285s）。包含19 Own正值/合法0/清除、76默认记录、具体终端Global、原生实际正文、两套各8专用Style的声明/关键Setter/真实目标、两Host及原内容/尺寸、热更新与回收；原276覆盖会话veto/取消/异步关闭、键盘/焦点、结构尺寸、popup分层与原释放等既定合同。

补充回收首次四FAIL在旧版与候选同现；只刷新已关闭窗口仍FAIL。诊断找到provider保留的合成树旧节点，探针在移除controller后立即关闭窗口，没有完成导航渲染。单变量增加移除后仍可见窗口的一次正常Dispatcher/渲染稳定步骤，四组合各Dialog/Surface/Content三个WeakReference全部释放，实际叶子Global Menlo/23/Purple、专用body 23/Navy与清除14同时通过。没有为这次探针纠正改产品。独立Style声明用例锁定16专用类与关键Setter、无Name/Loaded/Unloaded或code-behind节点设置回退。

作者另步补查mask/shadow/header间距和caption真实框架pointer hover/pressed/恢复。初始四项2PASS/2FAIL仅因探针把原IconSize14写成16；进入Green/Orange/Purple/Red状态的实际断言均已通过，不将该默认假设计作产品RED。纠正原IconSize预期为14后，四项全部PASS/0Skip（4s）；实际hover/pressed走可见Surface中的框架MouseMove/MouseDown/MouseUp，关闭按钮原Global恢复与原16组静态主帧证据分别记录，不冒充Overlay原始命中边界已通过。

## 实际Skia与NativeAOT边界

当前Mac osx-arm64进程，官方Headless平台+实际Skia（UseHeadlessDrawing=false、原region dirty clipping），分别走Overlay与WindowDialogPresenter/Headless Window；**不等同NSWindow原生chrome、IME或OS真实指针验收**。owner960×740、Host请求520×420、140高可见竖滚动条/24行原文、完整标题与两标准按钮保持。四算法×两owner×两Host×11阶段=176帧，至少700ms和12相同像素帧、上限5s，正常Motion/Dispatcher；不裁剪、遮罩、对齐或增容差。

managed和NativeAOT各**224主检查0失败**，旧版相同完整操作历史224检查32FAIL。旧新**160/176精确0**，另16均为明确Global配置；JIT/AOT **176/176精确0**。六类业务默认恢复×16场景共96对精确0。专用Part与Own在旧版已能命中的场景不重复算新修复；scroll.Offset是公开API路径，不冒充原始滚轮输入。

第一版独立1DIP错误覆盖产品Surface定位RenderTransform，清除后Overlay八对243359–336324像素恢复失败；主场景无该扰动。纠正为仅移动原本无变换的semantic-container并ClearValue恢复，主采样不重跑、不挑帧消差。旧/候选/AOT各16组A/A全0，1DIP全部检出，16组恢复全0；每场景三版本检测数字完全相同，范围9341–11965像素、通道最大198–235。只改探针校准分支，保留错误首轮记录，不归因产品/FW1。

NativeAOT发布与运行完成，无IL警告；保留五条既有Homebrew库macOS26/target12 linker警告。未更换官方依赖。完整16对有意变化见[限定视觉表](modal-consumers-visual-review.md)，已获用户限定批准。

## 仍开放的出口

现有276与本包验证不证明所有配方/RTL/持续loading与确认loading/自然尺寸极值的全交叉、真实原生窗口输入/OS chrome或完整Modal出口；这些按既有合同继续。HeaderPadding下游默认修复有运行资源/实际消费者证据，本固定Host矩阵未单独建立自然最小高度变化的视觉表。任何未覆盖项、其他家族、Timeline端点、既有Message/Notification P2与原始非零继续保留。既有批准不转成通用像素容差。

## 清理与最终精确验收

25个临时用例的5份源、共30819 bytes已删除（第一组4份24847 bytes、补充1份5972 bytes），无新增常驻用例。912 PNG+912树记录+manifest共1825取证文件已清理；清理前后媒体审计均clean、Git候选无发现，早期空取证run另清1 manifest。外部工程与会话材料清理数见下方最终登记。

清理后显式filter `FullyQualifiedName~AtomUI.Desktop.Controls.Tests.Dialog.|FullyQualifiedName~AtomUI.Desktop.Controls.Tests.MessageBox.`：**276PASS/0FAIL/0Skip，测试Duration22s、命令40.799s**；NET8 Release **0 Warning/0 Error，20.63s**（命令20.833s）；最终成员4文件/2Control/0问题，快照与采样前完全相同，56源候选文本精确一致。全部构建/采样持共享run.lock，无锁争用，无全量回归。

两CSV分别13 metadata/19 Own更新，实际CT15仍137/274、NeedsDecision11；正式五文档及总表同步，清理后另步作者自审复读九产品文件的完整增量与默认/owner/Part/继承/释放路径。作者单agent自审，非独立评审；没有搁置Critical/Important，完整家族缺口按上节保留，不作为minor豁免。未申请整包关闭，无Git授权。

最终外部清理登记：外部driver工程434文件；整会话材料593文件（包含该工程、临时源码备份、对照及探针失败日志）已在文字结论保存后删除。会话marker同时删除，无临时媒体链接残留。Git diff --check另步通过；未提交。


限定批准登记：2026-10-31，用户明确回复“批准表内16对Global变化，其余未通过与完整CT15保留”（request_user_input_async / call_62f69b5d81a14a9ab06d7d6f5bb64392 / item 0）。仅Modal表内16对Global获限定批准，真实OS输入及完整Modal/CT15、其他未通过保留，不授权Git提交。


2026-11-01 Modal原生输入/Tab修复：原生Overlay Tab进入owner input/opener，匹配原AtomUI Window宿主有效RED4项1PASS/3FAIL；仅在既有IsModal=True AXAML Style声明TabNavigation=Cycle，modeless默认Continue与热切换/嵌套LTR-RTL保持。7临时+276原模块283PASS；最终同源managed/AOT Dialog/MessageBox×Overlay/NSWindow各261PASS/exit0，Cocoa点击/选择/输入/Tab、async veto/Escape/重开Enter/恢复焦点、520×420Surface/140viewport/24行/23Menlo保持。旧原生偶发输入/焦点失败和shutdown观察者坏批留账，不声称全修。7临时/1源8644字节、38取证、425会话1125334263字节清理audit clean；清理后精确276PASS/0Skip17s、NET8零警告零错误19.98s。见modal-native-input.md；只有生产主题3行导航增量，原CT11 owner绑定保留，完整Modal/CT15和既定FW1/FW2边界不变，不提交。
