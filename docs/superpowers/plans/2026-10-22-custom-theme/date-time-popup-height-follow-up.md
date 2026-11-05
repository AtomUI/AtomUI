# CT16 输入高度与 popup 位置复核

2026-11-03用户观察：“输入框高度变高后 popup 的弹出位置偏上”；无法确定四种输入中的具体一种或打开/增高顺序。复用[家族出口](date-time-family-exit.md)的1660×800、四输入、固定10:15:30与2026-10-15数据及正常motion。已完成项目与V1批准不重开；本记录只跟踪该新反馈。

## 有限验收

- 原四种Picker：Middle打开 → 保持打开切Custom（应用Style padding13/radius17）→ Middle恢复；关闭后Custom再打开作单变量对照。范围时间另核对End端，保留水平端点、箭头、内容、滚动、pin及已提交值。
- 读取实际owner/frame/PlacementTarget/popup child边界、祖先变换/裁剪和最终放置。底部未翻转时，popup与可见输入框的垂直间距不随输入高度意外缩小；只对有失败证据的依赖范围修复。
- 最小根因修复、原场景前后及对应模块检查；临时源/驱动/媒体清理后精确验收，不跑全量，不创建分支/worktree/子agent或提交。

## 当前证据

最初夹具TextBox类型名歧义的编译失败不计产品证据。范围时间使用活动Textbox作PlacementTarget，范围日期覆盖端点钩子而保留整体目标；这只是待几何重放核实的线索，不先修改事件或放置逻辑。Avalonia依赖12.1.4的源码读取精确commit `2d79ba55fae1b1e71c69f93a668080c9029e6a31`，未把本地checkout HEAD混用为版本依据。

原四输入、相同完整轨迹已恢复。TimePicker、DatePicker、RangeDatePicker的frame高32→50，popup整体下移18，gap始终4。RangeTimePicker的活动Textbox固定高22，仅随padding居中下移9，popup也只移9：Middle frame底82、popup顶85（gap3），Custom frame底100、popup顶94（gap−6）；重新打开Custom及End端同样偏上。未翻转，窗口内空间足够，祖先/裁剪/变换均有完整记录。以实际MarginToAnchor8为判据取得有效RED：Middle预期gap8、实际3。

第一次原型发布相对Textbox的扩大PlacementRect，实际值正确（0,−5,159,32），但位置不变。精确框架源码证明CalculateAnchorRect会与PlacementTarget.Bounds相交，扩大rect被裁回Textbox。这一失败保留，不加offset/延时补丁。候选改用原可见AddOnDecoratedBox作为target，PlacementRect的X/Width来自当前Start/End文本框，Y/Height来自实际ContentFrame，并在ControlTheme绑定最终值；跨模板arranged坐标由原控件发布，无新公开API/Token/订阅。

同一原场景候选通过：RangeTime Middle/Custom/恢复/重新打开/End的gap均8，popup Y分别90/108/90/108/108/90；Start/End及不同高度的所有X与原版相同，其他三Picker不变。默认Middle需向下修正5 DIP，Custom向下修正14 DIP；该默认可见变化将在工程、渲染和必要专项完成后提供限定审查。成员审计原/候选均1文件/1控件/0问题。

最终目标绑定已迁回AXAML，以LocalValue优先级的CompiledBinding读取同模板可见框，PlacementRect继续TemplateBinding读取运行期布局输出。原名称通过x:Static写入时编译器无法解析ElementName，改成相同字面名称PART_AddOnDecoratedBox；没有新增或改名PART。原基类端点钩子会用Textbox覆盖目标，因此范围时间只保留其同等端点方向/indicator行为，不再调用该目标设置分支；模板重建也读取声明式新目标，不写回旧Textbox。

追加边界：原窗口内仅将范围时间下移至y550，正确向上翻转，frame顶590与popup底582保持8 DIP；恢复位置及有效原模板的两次重新构建后，End矩形及gap8保持。最初空模板缺少必需PART_AddOnDecoratedBox的失败属于无效夹具，不计产品失败；有效重建复用原IControlTemplate.Build及其namescope，不用近似模板。

明暗同轨迹48对旧新整帧中32对RGBA0，其余16对仅范围时间的popup移动（含阴影）；所有四输入的尺寸/内容/交互及正常motion保持。12项A/A、1 DIP/恢复全部符合，检错Light6768、Dark6809像素，未裁剪/遮罩/放宽容差。最终成员审计仍为1文件/1控件/0问题，公开API/Own/资源ID未变化。

首次NativeAOT/Skia明色24帧对JIT全部RGBA0。补充NSWindow原生运行在End操作误调用Headless鼠标API而抛Compositor未初始化，属于驱动后端不匹配，不计产品失败/通过。原生分支改为实际Textbox编辑API触发End编辑，不直接写内部活动端；Headless仍走真实框架鼠标路径。NSWindow managed明暗完整轨迹随后通过，scale2下gap8，含向上翻转及有效模板恢复；原生物理输入不由该API路径证明。修正驱动仅增量重链验证宿主，产品库源码保持不变，已通过Headless图不重复发布验证。

最终NativeAOT/Skia明暗48帧与JIT逐帧RGBA0。NSWindow managed和NativeAOT两主题完整20阶段几何重放均通过，所有RangeTime高度/端点gap8，原其他三输入gap4，向上翻转及有效模板恢复通过。两次发布/增量重链均无IL/CS警告，仅原5条Homebrew系统部署版本链接警告；产品源码不因驱动后端修正而变化。首轮133项TimePicker/DatePicker精确检查通过（11s），最终清理后重新门禁另记。

## 限定位置变化审查 P1（已认可）

仅RangeTimePicker：默认32 DIP frame下popup从Y85移到Y90，Custom50 DIP下从Y94移到Y108；Start/End水平坐标不变，显示内容、输入框尺寸、滚动条、正常motion和提交值不变。向上翻转时同样以完整框体保留8 DIP间距。修复前后明暗默认两帧变化Light22,971／Dark13,657像素，均为该popup及阴影的垂直移动；Custom为Light32,410／Dark24,184像素。其他三输入与独立校准32帧整帧0。该审查只覆盖本次位置纠正，不重开原V1颜色批准、其他CT已完成项或CT25/FW1/FW2。

## 清理与最终验收

未新增常驻测试。仓库外驱动/源码备份/两次发布等491文件（1,611,440,018字节）和193个取证文件（5,867,311字节）已删除；媒体Git审计clean，无项目内二进制或截图，原未提交改动保留。原生会话进程与窗口均退出。

清理后持共享产物锁执行`dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj -c Release --framework net10.0 --no-restore --filter 'FullyQualifiedName~TimePicker|FullyQualifiedName~DatePicker'`，133PASS/0FAIL/0Skip（11s）；Desktop Release NET8含依赖构建0警告/0错误（27.66s），`git diff --check`通过。没有跑全量。作者自审确认只有RangeTimePicker代码与主题变更，无通用Popup偏移补丁、计时器/订阅/重绘或新Own/API；实际目标和矩形均由ControlTheme绑定，代码只发布所需布局输出。

批准登记：2026-11-03，问题ID为`["request_user_input_async","call_7d18ea1c06a14349ae35fff3de774574",0]`。请求原文：“是否认可 P1 位置修复：仅范围时间 popup 默认向下修正5 DIP、输入框增高到50 DIP时向下修正14 DIP，使其始终与完整输入框保持配置的8 DIP间距？Start/End水平位置、输入尺寸和其他三种Picker不变；向上翻转同样纠正间距。”用户明确回复：“认可这项限定位置修复”。仅覆盖本记录位置修复，不包含Git提交或平台边界扩大。

工程、本机专项、清理后门禁和P1限定审查均已完成，该位置反馈关闭；不把该局部反馈扩为已完成家族/平台任务重开。
