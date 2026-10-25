# CT25 第七轮修复与提交审查

本轮使用既有feature/custom-theme提交ef09198b9与AtomUI凭证调度GitHub Actions；Windows实际宿主为windows-latest远端runner，本机macOS没有运行Windows操作系统。Mac复现属于Mac证据，不替代Windows验证。

## 本批审查结论

本批63个文件，沿现有feature/custom-theme／checkout，HEAD为ef09198b9。本批同时收拢此前已批准视觉的DataGrid静止指针、Picker模板会话、渐变文字及ColorPicker依赖／输入／旧formatter修复，并加入完整失败TRX摘要、W14观察阶段、Gallery真实Skia宿主及保留原滚动触发的测试输入适配、已批准Steps标签1 DIP修复。全部媒体／临时测试源和本轮Linux临时环境已清理；报告保留数值、SHA及明确平台范围。

提交分为两步：第一步只移动AbstractColorPickerView成员和步骤进度四个常量（两个文件的Roslyn成员token与HEAD完全相同，注册表达式／顺序和两个画刷初始化顺序保持）；第二步提交本批最终实现、测试适配和文档。两个提交合计63个唯一文件，无新增常驻case；本机审查补丁为.superpowers/sdd/platform-validation/seventh-layout-only.patch，最终文件／SHA清单为seventh-review-snapshot.json。该清单在审批前冻结，不创建新分支或worktree，不发布NuGet。

最终聚焦证据：Mac／本机Linux arm64 Gallery各412通过；W14原RowHeaderRecycle模块各9通过。四常量移位后当前Desktop受影响ColorPicker／TimePicker／Icon／Tooltip／Progress共111项8.128s、Gallery Progress5项4.392s通过；NET8 Release ColorPicker依赖图17.950s、零警告／错误；Python runner模块22项10.385s通过。源产品18文件／11 Control的成员审计只有Steps原四常量位置问题，移位后Steps复审零问题，其他17文件结果不变。git diff检查通过。作者自审，无独立agent评审。

原同SHA Mac Actions重试通过，3 NativeAOT／2 Browser也已通过；当前新源码尚未推送，实际Windows及新提交的五运行时仍待Actions，不将本机Linux arm64或Mac代称Windows。既有逐场景视觉限定严格沿原批准范围，不新设容差。完整CT11–CT19／CT25仍按各自未决出口推进，本批提交不等于关闭完整家族或所有进行中任务。

提交门禁来自AGENTS.md第148行：“未经明确指令不得提交”。本批待用户明确批准两个提交、推送及验证Actions；此前授权已经执行完毕。

## W14 当前原始失败

[第六轮运行](https://github.com/AtomUI/AtomUI/actions/runs/40149666278)的Windows Grid选测308通过、1失败。ChangingHeaderTemplate_ShouldUpdateRowsWithoutChangingDetailsLifecycle(Visible)报：`initial layout: row=3, first=0, last=3, viewport=341, ready=Ready, stale=False, measure=True, arrange=False`。记录阶段证明回调发生在Show期间，尚未执行header模板替换；没有把该证据写成产品或测试已修复。

同项目原W15方法本轮未失败。Windows后续测试项目在Grid失败后未被执行，主题资产物化步骤也跳过，不能将308通过扩大为全Windows门禁通过。

下一步核对实际初始容器估算／测量、详情加载与回收栈，确定这是合法首次布局虚拟化还是错误卸载；未取得反例与因果证据前不放宽断言或修改产品。

三OS NativeAOT与两Browser fixture通过；Mac／Linux选测当时仍在运行。完整CT25与家族验收未完成。不创建分支或worktree，不发布NuGet，本轮之外修改未获Git提交批准。

## Mac选测的新失败

Mac已有8个项目共5664项通过：Build91、Shared200、Controls27、Core367、Grid309、Rendering77、Desktop4128、Generator465。Localization Integration的Official_PtBr_Package_Graph_Is_Consumable失败，输出定位为准备Native8编译器时git fetch dotnet/runtime无法连接github.com:443；其余4项通过。该连接失败不证明产品逻辑已通过，也不据此改动产品或吞掉测试；待全部作业结束后可对相同提交的Mac作业重试确认。后续项目及主题资产因前序失败尚未取得本轮结论。

## Linux与首轮总结果

第六轮首轮已结束：三OS NativeAOT及两Browser fixture通过，三个OS选测失败。Linux的Desktop4128项与Grid309项均通过，原W13隔离失败及W15本轮没有重现；Localization5项和GalleryBase209项也通过。Linux后续Gallery394项中375通过、19失败，均为Semantic预览高亮未创建（期望1／2／6个高亮而为0等）。汇总确认19项失败；当前latest错误和job日志的dotnet输出都只保留尾部，已提取12个明确失败方法，前7项名称仍缺失，尚不能称完整失败清单。下一轮必须在失败TRX删除前保存全部失败名称与消息，不能靠推断补齐。尚未确定几何、输入命中或生命周期原因，未修改事件路由、窗口尺寸或断言。

## 本轮执行顺序与验收点

用户2026-10-25要求完成全部进行中任务：CT11–CT19、CT25。未开始的CT20–CT23／CT26保持后续范围，CT24继续暂缓。沿当前分支与checkout，不使用subagent；提交审查、平台证据及视觉批准边界保持。

Pre-flight: 失败摘要生产者为verification runner，消费者为CI latest.json及诊断；新增字段必须是version1的可选附加数据，不能让失败结果获得成功receipt。Gallery输入探针消费者是现有Semantic预览及Headless宿主；保持1280×900原窗口、原页面／滚动约束、标题和真实hover，不用直接SetHoveredPart或改变尺寸替代输入。W14只按实际启动／模板替换阶段归因，未验证前不放宽断言。

### Task 1: 在临时TRX清理前保留完整失败明细

**接口：** latest.json添加失败项目、每项名称／class／outcome／message／stack及解析错误；原status、退出码、成功receipt合同不变，原始TRX与日志继续临时清理。

1. 系统临时Python探针运行真实runner与现有进程边界fixture，制造19失败及超过16k日志，断言全部失败名在清理后报告中保留。
   Expected: 原版因缺少失败明细而RED，不以fixture或导入错误为前证。
2. 最小修改evidence与runner读取实际失败TRX；没有或损坏TRX时保留主错误，不掩盖命令失败。
   Expected: 19明细、异常／无TRX、非零退出和清理行为GREEN；不创建成功receipt。
3. 删除临时探针后运行`python3 -B -m unittest discover -s scripts/verification/tests -p test_runner.py -v`。
   Expected: 当前runner模块检查通过，临时测试与raw evidence清理；这不是仓库全量回归。

### Task 2: Gallery高亮的真实输入几何诊断

**接口：** pointer→真实命中行→PointerEntered→EffectivePart→session／target→AdornerLayer。探针只读状态，不以代码定制Semantic Part。

1. 临时保留List／Segmented原触发顺序，记录窗口／卡片／祖先矩阵、viewport／clip、实际命中、pointer进入及session；只将是否推进真实render作为单变量。
   Expected: 确认Mac可复现与否，并明确卡住的边界；不能以Mac通过推断Linux通过。
2. 仅在根因与有效RED确认后实施对应所有者最小修复。
   Expected: 保留布局、滚动和完整行hover合同；必要的远端诊断先准备具体提交快照再申请批准。
3. 删除探针后执行受影响现有Gallery高亮精确filter，涉及产品渲染时补自驱动Skia及真实Mac宿主。
   Expected: 所有原断言保留；视觉变化按原规范逐项审查，原生与Headless分列。

### Task 3: W14及家族闭合

先依据首轮Windows启动阶段卸载记录核查真实容器／详情生命周期，建立相同触发反例；不将初始布局事件自动等同header切换事件。结合Task2完整平台出口推进CT25。随后逐Control补齐CT19、CT18及CT17–CT12已有计划的Active Own、Global、Part、状态／尺寸／恢复与显示生命周期，最后关闭CT11全owner动效和CT25最终性能／平台边界。每个家族以实际验收出口关闭，不以代码迁移或共享修复代替。范围外需明确合同决策；不自动降低验收标准。

## 完整失败明细实现与本机诊断

Task1的3项临时运行反例（19失败且日志超16KB、损坏TRX、无TRX）原版全失败，候选全通过；删除临时探针后原runner模块22项9.654s通过，无常驻case新增。latest.json新增failed_test_results，保留完整非通过结果、计数及明确的不完整标记；失败仍非零退出且没有成功receipt，原始TRX／日志依然清理。源码及文档尚未提交，不能声称已从旧CI补回缺失的7项。

Task2的List原方法与两个只读几何探针在Mac共3项通过，输入点(1017.5,416.5)、原窗口1280×900；PointerEntered=1、卡片IsPointerOver=true、实际命中Border#PART_Row、EffectivePart／session存在且1高亮。是否额外推进render的单变量都通过，未据此推断Linux修复。

新发现本机Docker服务和已有Linux/arm64 Ubuntu24.04 SDK10.0.402镜像可用，运行时10.0.13；只读挂载已有构建，不复制源码、创建分支或worktree。首次VSTest试运行因xUnit尝试启动Mac Mach-O apphost而Exec format error，未运行用例，属于跨宿主驱动准备失败；改查程序集自带runner，通过dotnet直接运行托管DLL，既有Mac产物保持只读。

## Mac同SHA重试与Linux输入因果结论

40149666278同SHA的Mac选测重试已完成success；原首轮网络失败保留记录，不再记为当前Mac未决。Windows W14和Linux后续修复尚须下一轮实际CI确认。

Linux/arm64本机容器的原宿主使用HeadlessDrawing stub；真实非CSD WindowVisualLayerClip的圆角StreamGeometry在stub FillContains返回false，卡片中心虽然在clip Bounds内却命中底层Panel，没有PointerEntered。普通矩形正对照通过，不能扩写为所有geometry均失效。只改Gallery测试宿主为现有Grid／GalleryBase相同的Skia真实几何后，List输入链恢复，27项25通过。

余下Tour popup.description和Timeline第二组itemHeader的原输入分别Y885.5／886.5，已越过窗口真实内容底边855；内部pane消费BringIntoView请求，外层尚未完全显露该预览。单变量先对所属SemanticPartPreview执行BringIntoView、再滚动卡片后27/27通过，66条阶段记录中每个after实际命中卡片且原高亮数断言保留。1280×900、原页面标题、内外viewport、scrollbar及产品事件逻辑均未改；测试加实际InputHitTest命中断言，避免以后对不可见卡片发hover。原始19项CI缺失的前7个名称仍不可从旧日志补回，本次本机实际复现单列。

准备过程Exec format error、缺libfontconfig以及首次只挂程序集导致source lookup缺失均记为驱动准备失败，未混入产品反例。正式本机容器只读挂载既有checkout，使用Linux/arm64 Ubuntu24.04 SDK10.0.402／runtime10.0.13，不能替代CI Linux/x64。临时镜像仅安装fontconfig／DejaVu，验证后删除自建镜像；宿主与媒体／探针清理结果随最终检查回填。

## W14有效前证及候选

保留原Grid640×400／Window680×440、100条记录、五列与详情模板，只对详情高度做30–60单变量采样。Mac默认30没有启动卸载；37／38各在Show初始布局卸载row3，viewport346、measure=true／arrange=false。调用栈为ComputeScrollBarsLayout→UpdateDisplayedRows→RemoveNonDisplayedRows→回收Row→UnloadDetailsTemplate，没有任何header setter。详情37下原有Visible用例有效RED，消息与Windows同为initial layout回收row3（Windows viewport341，不能把Mac数据当Windows）。

候选只校准测试观察阶段：Show完成后订阅Loading／UnloadingRowDetails；两次header模板替换均保留零卸载，并新增零加载及相同显示行实例断言。原始详情30与边界37各两项GREEN，临时变量接线和探针已删除，清理后原RowHeaderRecycle模块结果回填后再申请远端验证。产品DataGrid生命周期代码未因此改动，Windows最终结果仍待实际Actions。

## 真实字体下新增的两项产品边界

切换宿主后Gallery整个项目412项中403通过、9失败：其中3项为本轮新增的pre-hover命中断言在compositor更新前读取旧命中树（Headless MouseMove本身先RunJobsAndRender），已按框架时序推进真实render再查询；部分长列表／水平页签检查原先只在固定宽度stub字体下溢出，真实字体内容可容纳，正在保留原尺寸、标题、卡片数和滚动链并补明确长文本输入。

另两项保留为产品反例：StepsProgressBar原有Gallery检查发现末端indicator越界1 DIP，同已批准普通ProgressBar的取整根因但尚不扩大视觉批准；单一Semantic预览在真实字体下短列表自然DesiredHeight比剩余高度少36 DIP，违反既有“填满剩余高度”的合同。前者只对取整后的实际留位锚定，后者只让有限可用高度的宽布局报告该高度，多个预览无界测量和compact布局不变。两者均先保留旧源码、验证候选及同轨迹默认像素，再单独提交所示视觉变化供批准；当前不算视觉验收完成。

### 短列表合同纠正

进一步核对docs/gallery/authoring/semantic-part-preview.md第177行，短列表自然高度是现行明确合同。前述36 DIP候选扩展不应成为产品修复，已经撤回并恢复原LayoutPanel源码，未提交或申请视觉批准。Steps原测试依赖stub字体把文案测得足够长；补明确长说明文本后继续锁定“超出空间时填满／内部滚动”，原窗口／尺寸／标题／行数及短窗口边界不变。原型layout图片只作已撤回调查记录，不能当通过证据。首次明暗捕获仅切RequestedThemeVariant，未切算法，属于无效暗色准备；不据此声明暗色覆盖，改用完整Default+Dark配置作用域重采StepsProgress。

## 测试宿主与输入适配的最终责任边界

真实Skia并不保持Headless固定字体度量。自然页面只断言pane不超过400上限、实际溢出与可见滚动条一致；卡片不足时仍自然展开，不强造滚动。滚轮链路用例保留1280×480、原内外高度、标题与卡片数，通过公开PartDescriptions给说明追加三行来稳定保留原溢出触发条件；水平Tab滚轮用例给同一三项Header追加长文字保证横向溢出，不以缩小窗口／隐藏滚动条改变UX。

Tour／Timeline测试先按实际吸顶区高度把预览滚到可视区，再BringIntoView卡片；吸顶宿主可能被提升到AdornerLayer，需在Window的实际视觉树中查找同一命名宿主。原新增命中断言必须在真正render后查询compositor命中树；框架Headless MouseMove入口自身也是RunJobsAndRender→事件→RunJobsAndRender，不能以过早查询为产品失败。适配后这两类、TabControl输入与页面检查10项7.219s通过；整个Gallery最终结果仍待临时源清理后检查。

W14临时源已清理后，原RowHeaderRecycleIssue475模块Mac9项4.397s通过。它只证明校准后的当前Mac观察合同，Windows启动阶段与后续项目仍待新提交的真实Actions。

## 清理后的Mac结果

当前Gallery整个受影响项目412项54.335s通过（精确namespace filter，非仓库全量回归），此前9项均已核对合同并处理。LayoutPanel保持原源码，没有短列表产品扩展；StepsProgressBar仅取整锚点修复。明暗最终采样完整切换算法，20项比较全通过，默认Light238／Dark239像素已获用户明确批准；临时源码／旧源备份及最终24张媒体均已删除，文字SHA和数值保留。W14原模块9项通过，实际Windows仍待CI。

## 已批准执行

用户明确批准本批两个提交、推送及验证Actions。纯布局提交f0549a3a9与最终实现提交5c7c4a2cc均已创建，63个最终Git blob与dd13dc4a4c15审查快照逐一相同，随后推送既有远端feature/custom-theme并确认远端SHA。验证运行https://github.com/AtomUI/AtomUI/actions/runs/40155448673，full=false、三OS／两Browser，无NuGet发布。首轮已结束：Mac选测6290项及五运行时通过；真实Windows Grid309全通过，Desktop剩11项测试合同失败；Linux Desktop4128全通过，后续本地包消费总体600000ms超时，当前请求同SHA复验。三组测试诊断与42项清理后结果见[第八轮后续](platform-eighth-followups.md)。此后新研发不沿用本批提交授权。
