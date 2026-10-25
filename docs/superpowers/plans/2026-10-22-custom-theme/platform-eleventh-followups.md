# 第十一轮候选：平台失败的根因与取证

执行记录：用户已批准13文件快照4025598cde51，已创建提交0c6d8445679e并核对全部blob，推送已有远端分支，远端SHA一致。[第十一轮Actions](https://github.com/AtomUI/AtomUI/actions/runs/40168600469)正在运行三OS／两Browser，full=false，不发布NuGet；下文待审措辞为冻结前记录。本批授权已执行，后续ColorSpectrum不在提交中。

最终结果：八个job均结束，三NativeAOT／两Browser、Mac／Linux选测通过，后两者各6290项；仅Windows选测失败。Windows前八项目5664项及本地化其余4项通过，Official四消费断言完成，仅清理build-host.log失败；实际Restart Manager返回三个.NET Host占用PID5944／5124／5164，同三dotnet随后出现在runner遗留清理记录。三OS各54条journal全部配对。后续consumer构建Windows约6／6／5秒、Linux约6／6／6秒、Mac约4／4／3秒，负例各exit1；Linux本轮未再超时，Mac原Tour本轮通过，但没有据此声称那一次命中失败根因已查明。[第十二轮候选](platform-twelfth-followups.md)继续Windows子进程生命周期，不放宽清理或包断言。

沿现有feature/custom-theme／checkout，无新分支、worktree或subagent，不发布NuGet，不自行提交。第十轮全部job已结束，授权已用完；新的Git快照须完成本地审查后单独提交用户批准。

## 已知失败与待证明假设

- Linux：600秒总限发生在最后active-corrupted构建开始约24秒后；先前三个build为153.67／128.37／129.40秒。四个真实消费者和负例必须保留。源码ProjectReference只移除全局OutputPath，却继承各consumer不同的RestorePackagesPath；它可能反复重写共享源码obj的NuGet输入，削弱复用。先用实际ProjectReference工厂和真实MSBuild还原私有consumer／共享source的路径合同，再检验完整Official包图；没有有效证据前不称其为已定位的超时根因，不改变600／120／300秒上限。
- Windows：import路径断言及Grid309已通过。所有27个子进程开始／完成配对，最后预期负例exit1，最终清理仍因build-host.log占用失败。现有Mac子进程探针未复现句柄；不据此猜测修改Toolchain。需要在实际失败现场读取文件占用PID／应用名并保留原始异常。只读查询不关闭、重启或杀掉占用者。
- Mac：Tour语义卡片popup.root在hover前未命中，本机原1280×900的两项原流程通过。必须保留舞台、mask、Popup、sticky、两个滚动层及原断言；先补齐center前后、实际hit、完整变换／裁剪链等证据，不将诊断增强称根因修复。

## 执行顺序

1. 临时CT25SourceNuget访问真实私有ProjectReference工厂，两个consumer使用不同私有NuGet根，断言各自隔离而共享source的restore输入不受consumer根影响。先RED，再最小候选，再相同GREEN。
2. 对完整Official包图验证全部四消费者、精确诊断、输出及清理；记录真实阶段和源码重建线索。前证和后证不等同于跨平台性能保证。
3. Windows现场取证若采用Restart Manager，只调用资源登记／占用列表和结束查询会话；参照[微软RmRegisterResources](https://learn.microsoft.com/en-us/windows/win32/api/restartmanager/nf-restartmanager-rmregisterresources)、[RM_PROCESS_INFO](https://learn.microsoft.com/en-us/windows/win32/api/restartmanager/ns-restartmanager-rm_process_info)，不调用Shutdown／Restart。确认具体方案后用临时反例验证失败信息不丢失；Windows原生调用仍须真实CI。
4. Tour按原场景取证，只有原始触发条件下有因果RED/GREEN时才标记修复。新的测试默认临时，最终删除，清理后精确模块验证。C6独立视觉审查和平台失败分别记录，不能相互代替。

## 候选与已取得证据

### 源码恢复输入与重复构建

最初按实际ProjectReference工厂、两私有consumer恢复同一source，原版source的packages根随consumer变化，两种根与预期一个根不符，反例失败。仅在ProjectReference.GlobalPropertiesToRemove追加RestorePackagesPath仍失败，已撤回；SDK10.0.300的NuGet.targets有独立的全局恢复覆盖流程，不能把普通MSBuild子项目元数据当恢复图隔离边界。

最终把RestorePackagesPath放在消费项目自身PropertyGroup，并在实际RestoreConsumer中把feed／tooling-feed作为消费项目自身RestoreAdditionalProjectSources保存。命令行不再把这两类值施加到整个源码图，固定的全局fallback路径保留。真实工厂及恢复方法的临时验证分别证明：每consumer仍使用自己的私有packages和指定feed，共享source保持同一packages根、不消费损坏feed，两个source的完整project.assets.json SHA相同。feed扩展前的对应反例失败；候选四项聚焦验证2.8826秒通过（其中三项属于清理诊断）。全部原始失败、无效候选与日志SHA保留，不把编译准备错误算产品RED。

只做上述隔离后，本机完整Official测试两次通过（330秒及293秒量级），但full之后的partial／dormant仍各用约52–54秒重建共享源码。没有将其称为Linux超时已修复。改为full首先真实构建所有源码模块，partial／dormant／active三个消费者以BuildProjectReferences=false复用这批产物，仍分别恢复私有包图并真实编译各自consumer、执行完整原断言。没有提前构建空替代物或删除负例。

额外临时测试用实际CreateConsumerProject、RestoreConsumer、BuildConsumerUnchecked和独立真实Source项目取证：原版第二个consumer又请求一次source Build，预期1而实际2，有效RED；候选两个consumer各自产生自己的dll且复制Source.dll，source仅Build一次。删除source bin／obj后第三个consumer明确失败，不能静默重建源码或把旧consumer输出当成功；候选1项8.2559秒通过。该调度只用于这个先full后逐consumer的专项fixture，不影响产品或其他构建。

### Windows占用与原异常

Windows前证是第十轮真实清理IOException：错误只指出build-host.log，既无完整剩余路径和占用者，也可能覆盖先前消费断言。Mac的文件锁语义不能复现Windows拒绝删除，因此不声称本机验证了实际占用或其根因。

新增测试基础设施LanguagePackTestCleanup，仅在清理失败时列出剩余build-host.log，用Windows Restart Manager只读查询PID、进程开始FILETIME、应用／服务名及原始状态码，并结束查询会话；没有Shutdown／Restart、kill、删除重试、吞错或放宽超时。若原消费已失败，AggregateException同时保留原异常和带诊断的清理异常；查询失败也只追加诊断而不替代原异常。成功清理路径仍一次Directory.Delete。

临时三项检查验证原异常对象保留、无主异常时的清理链、实际嵌套目录清理及已移除目录、668字节Win32 Unicode结构布局和非Windows不调用原生API；与源码恢复边界共四项通过。真正Windows查询和文件占用根因仍待下一次获批CI，不把这些本机检查等同Windows验收。原生字段依据[RM_PROCESS_INFO](https://learn.microsoft.com/en-us/windows/win32/api/restartmanager/ns-restartmanager-rm_process_info)、查询依据[RmGetList](https://learn.microsoft.com/en-us/windows/win32/api/restartmanager/nf-restartmanager-rmgetlist)。

### Tour命中取证

不改卡片、Popup、mask、sticky、滚动层、1280×900及原命中／高亮断言。仅在原hover前命中失败时，追加原中心、渲染后中心及各自实际hit、theme／scale、card／sticky、全部视觉祖先Bounds／矩阵／clip／ZIndex／hit-test／滚动viewport-extent-offset、逻辑祖先和hit祖先。中心重算只用于失败信息，鼠标仍使用原中心；未增加等待／重试或强制布局。

临时单变量正对照只禁用目标卡片的IsHitTestVisible，真实原流程产生同类命中断言失败：原版信息缺完整几何，验证失败；候选诊断可解析，且原两项真实流程通过，合计3项4.0443秒。删除临时源码后原两项4.7431秒通过。该正对照证明取证能力，不能证明真实Mac CI那一次的根因，更不能标记Tour修复完成。

## 清理与审查边界

四份临时验证源码及其目录已删除，外部小工程在各测试finally中删除；常驻测试净新增0。C6的两产品文件和两个专用文档一并作为候选，C6视觉及59对限定已获用户批准，媒体已清理；未新增公开API、Own或资源编号。C5／第十批状态文档只纠正已批准／已提交的历史状态。

最终清理后的完整Official专项通过，测试约136秒、总执行137.4秒；14个子进程的28条开始／完成journal全部配对，无取消或超时。full构建66.663秒，partial3.909秒、dormant3.967秒，active3.194秒且预期exit1／ATOMUILOC006；四个消费者及全部原断言均执行，fixture系统临时目录已实际删除。仅列本机观测，不推定其他平台用时或Linux问题已关闭。

Tour清理后2项4.7431秒、Official清理后1项通过；C6清理后23项、NET8 Release零警告／错误及1控件成员审计复用同一产品SHA的有效记录，两产品SHA仍为ab2c9d86fb09／a4061aecd87a。最终diff检查通过，新文档相对链接无缺失；作者自审确认没有放宽包消费／部分图／错误诊断或原UI断言，未增加新常驻测试。新增Windows互操作仅测试基础设施，未经真实Windows现场运行的边界明确保留，不列为已验证平台。

未调整600秒总限、120秒普通子进程、300秒build子进程，未运行本地change／full。Windows原生查询、Linux总限和Mac原始Tour失败仍需真实CI；完整CT19及CT25均保持进行中。新的13文件Git快照尚未批准，不能自行提交或调度新Actions。

## 待审13文件范围

- 两产品文件：ColorView/ColorPickerInput.cs及Themes/ColorView/ColorPickerInput.axaml。
- 正式文档：ColorPicker implementation.md及changelog.md。
- 测试基础设施：OfficialPtBrLanguagePackTests.cs、LanguagePackTestCleanup.cs、TourSemanticPartHighlightTests.cs；不新增用例。
- 阶段文档：本记录、platform-tenth-followups.md、tasks.md、color-slider-bitmap-order.md、color-picker-input-lifecycle.md及color-picker-input-lifecycle-results.md。

拟在现有feature/custom-theme创建一个提交，推送已有远端分支并用AtomUI凭证运行三OS／两Browser验证Actions，full=false。排除后续ColorSpectrum研发、临时验证、媒体及所有产物；不创建分支／worktree，不发布NuGet。具体blob清单与整体快照SHA保存在外部SDD；本记录不含自身SHA，避免自引用。提交／推送／调度均等待用户对该快照的明确批准。
