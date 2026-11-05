# CT16 有限家族出口

本节原批准范围和证据保持。2026-11-03后续输入增高时popup偏上反馈的复现、修复及单独P1位置审查见[位置复核](date-time-popup-height-follow-up.md)；只影响RangeTimePicker放置，不重新执行或扩大其他已完成项。

> 执行：主 agent 使用 executing-plans，当前目录／feature/custom-theme，不建分支／worktree、不委派、不提交。用户明确授权自行选择实现路线，复用既有设计和批准，不重复设计确认。

**Goal:** 闭合既有22项metadata、51个Own字段及时间／日期家族适用状态的当前工程出口。

**Architecture:** 复用D1、D2-A/B、D3-A/B、D4-A及已批准D4-C；补缺失的实际owner Global投射，保留公开子Control身份、原默认、字段／ID／slot与专用Style优先级。非法行高按循环算法的正有限合同处理，不向其他字段扩张范围。

**Spec:** [既有日期计划](date-time-consumers.md)、[CT15最终边界](feedback-final-exit-results.md)、[页头批准](date-viewer-header-results.md)。

## 完成条件及顺序

- [x] 恢复原始数据／尺寸／操作历史，核对22项metadata和51字段；已迁51字段不重迁，原已批准页头不重开。
- [x] 时间：真实四种Picker的TimeView／Presenter Global、行高0／负／非有限与合法正数；列循环、浏览分数、候选／提交和键鼠保持。
- [x] 日期：DatePicker3／DateViewer18／Calendar11／Lunar11逐字段实际消费；Calendar／Lunar组合Global及字体、日期终端状态出口，身份／scope／Style／释放。
- [x] 合批当前Mac managed／NativeAOT与明暗Skia原场景；先工程验证，再对必要默认变化提供限定审查。像素整帧严格RGBA0，历史失败及未配对帧不计PASS。V1已获限定批准。
- [x] 删除本次临时验证／工程／媒体，持共享产物采样锁执行清理后精确模块与必要NET8；同步正式文档、CSV、任务表及作者自审。

## 排除范围与接口

CT12–15既定批准范围不重开。复用compat/Avalonia.Skia，不改官方NuGet缓存／native，不扩大像素容差；CT25／FW1／FW2既定问题及其他平台保留原边界。无全量回归。共享产物锁使用既有`.artifacts/verification/run.lock`，构建和采样由同一父进程持有，不在采样期间改DLL。

时间与日期共用OwnerTokenResource及原生Style优先级；Calendar专用Cell需跨公开DateViewer找到Calendar组合owner，但DatePanel本身的公开DateViewer身份保留。后续验证消费上述同一接口，不另建默认求值／资源引擎。新验证放所属测试Temporary/CT16Exit，独立驱动及原始日志在系统临时目录；本文件仅存文字结论。

## 执行结果

2026-11-03：初始目录／分支／HEAD=11e16846e已核对，既有未提交改动基线保存于会话临时目录。22=9 ProductControl+12 InternalTarget+1 OtherBoundary；51=TimePicker8+DatePicker3+DateViewer18+Calendar11+Lunar11。四个已验证日期依赖与八个时间字段复用，剩余日期逐字段消费不因V2迁移自动通过。共享输出锁非阻塞获取成功。

### 本轮工程证据（持续更新）

- 原始有效RED：15/15失败（9个非法行高、4实际Picker的TimeView Global、普通/农历Calendar组合）；夹具的错误API名和AXAML缺逗号属于工程坏批，不计产品失败。
- 新字体/Custom八项均取得原实现失败；43个日期Own逐字段使用实际主题节点、配置/清除，不以资源值替代。正常motion经实际render timer运行后，66项为65PASS/1FAIL；WorkdayMarkerColor仍被模板内HolidayMarkerColor压住。将普通marker背景移入ControlTheme Style后，原工作日单项PASS（4s），默认工作日标记颜色修复需要最终限定视觉审查。
- 页头实际线宽两个有效RED（3预期/1实际）；最初Single误取多个PixelAlignedBorder的两项不是有效RED。普通/农历业务条真实label的两项有效RED，去除命令式固定字号/白色，改由Calendar主题附加排版属性提供后两项PASS。页头/footer复用内部OwnerTokenResource的可选原生IValueConverter，未新增公开Token或ID。
- 仅StyledProperty验证不能挡住资源绑定非法行高：0导致Invalid Arrange，-1实际进入TimeView；不能据直接SetValue的9项PASS关闭边界。TimePickerToken在求值setter验证正有限数后，同一两项非法Own配置PASS，既有绑定保留上次有效值/时间；不向其他几何字段扩张规则。
- 新可选字体四类真实root/text在长寿命provider/窗口下GC均释放；页头/业务label合计四项PASS。实际四Picker隔离/重挂检查仍在跟踪：第一TimePicker移到B后owner/view Context同为B、完整链正确，但背景White而非B配置。只增视觉attach事件未解决；正在验证同context/key时去重导致未重新建立原生资源查询的单变量假设，尚不计PASS。
- 当前成员审计发现既有Calendar英文region、属性注册交错、DirectProperty backing位置及接口方法排列问题，外加本轮新增继承接口位置引出的结构要求。原始126项与精确member inventory留在会话临时目录；计划以纯成员移动/region调整单独核对，不混改成员体或初始化表达式。

重挂根因闭合：临时Core绑定key追踪证明初次创建链是TimeView→Presenter→Picker；重挂后链插入已注册ArrowDecoratedBox，实际选中`AtomUI:ArrowDecoratedBox`并读White，而三个目标对TimePicker key的查询均为B的#abcdef。撤回视觉attach/去重两次无效修补及全部埋点，只对TimeView/时间及日期Presenter主题增加InfoPickerInput anchor。原四Picker身份隔离/实际重挂、旧scope不再影响、四root/text GC及0/负/NaN/±Infinity Own拒绝，合批10PASS/0Skip（49s）。这不是重试、延迟或强制重绘修复。

最终消费核对还覆盖Calendar Fullscreen顶线与业务条文字内边距：普通/农历四项有效RED，LineWidthBold5实际仍2、PaddingXS17实际仍8。接线仅增加内部panel的LabelPadding输入与已有Calendar主题的有限线宽转换，不增加Own字段；业务条的半高端帽和lane几何常量保留其算法职责。正式默认仍为顶线2、水平padding8，需以同轨迹旧新画面核对。

农历文字行高另按既有明确合同核定：不是任意几何极值扩张。最初把ThemeChangeFailed事件作为判据的五项夹具不符合已落地的惰性终端协议，原实现/候选都不发整体失败事件，该批不算有效RED。已改为实际scoped终端查询须抛出ArgumentOutOfRangeException、真实文字保持原行高及清除恢复；只临时还原LunarCalendarToken原字节重跑，再finally恢复候选。时间/农历非法终端均不得描述为provider整体snapshot回滚；正式文档按查询时拒绝及既有绑定保留有效值说明。

### 修正驱动的有效静态出口

同步RunJobs驱动在采样时仍有真实IsAnimating及当前色/目标色差，单纯补render tick也未结束，原非零批保留且不计静态PASS。改用真实Dispatcher.MainLoop、异步步骤及实际动画结束判据，正常motion未禁用，原数据/尺寸/状态轨迹保持；采样后活动动画记录0。两源码/两主题共36项AA、1 DIP检错及恢复全部符合；检错6195至51645像素。142对旧新整帧中96对RGBA0；16对为明确配置消费变化，30对为工作日marker每帧32像素的预期修复。viewer和time其余兼容帧非零0。102行唯一明暗默认/资源ID/slot完全相等；本机managed主场景166项、尺寸/Custom112项均通过。尺寸/Custom为四Picker各Small/Middle/Large/Custom应用Style、实际打开/关闭及返回预设，共60个静态采样。

### 家族合同覆盖索引

| 范围 | 有限出口与复用证据 |
| --- | --- |
| TimePicker / RangeTimePicker / 公开TimeViewCell | D1的8 Own、D2-A逻辑行/分数浏览/显式选择优先、D2-B独立Cell身份及ListView兼容、D3-A两项可选范围Own；本轮四种宿主Global隔离/重挂、循环非法行高、预设/Custom、真实wheel与编译专用Style。 |
| DatePicker / RangeDatePicker | 三Own逐字段；日期/周/月/季度/年、确认/草稿/起终端、时间单面板及日期双面板沿既有测试与D3/D4-C，新的Global锚点、预设/Custom和弹层明确验证。 |
| DateViewer / RangeDateViewer | 18 Own逐字段配置/清除；Compact/Content、范围/预览/周列、导航/键盘/禁用及模板沿既有模块；D4-C批准不重开。新可选字体/清除/本地优先级、单侧线宽及专用Cell Style。 |
| Calendar / LunarCalendar | 各11 Own逐字段，Mini/Fullscreen、Month/Year、业务条、周末/节假日/工作日、默认与自定义模板沿既有模块；组合Global跨公开DateViewer、字体、顶线、业务标签与marker有效消费。 |
| 内部12项与CalendarRangeBar工具边界 | 以实际public owner归属，CSV原NeedsDecision项已核定；TimeView/Panel无新identity，公开viewer/子Button/ComboBox保持自身身份；DualMonth包装器只负责范围箭头几何，CalendarRangeBar仍为scoped数据对象。 |
| 退出与回收 | 配置/清除、scope兄弟与重挂、四root/text在长寿命provider下释放；既有重套模板旧部件保留及平台/物理输入/原生最终像素按CT25/FW1/FW2保留，不计已修复。 |

上述索引只归并证据，不新增所有平台、任意极值或Cartesian组合门槛。22 metadata=9产品+12内部+1工具边界；51 Own=43 Active+6 Derived+2 OptionalOverride，无新增/删除/改名字段。87临时验收在最终候选源码下87PASS/0FAIL/0Skip（7m13s），完成后删除；新增永久测试目标为0。

### 作者自审

本轮仅作者另步自审，不冒充独立评审。核对26个产品源码文件：公开API/Own名称类型/资源ID无新增删除；三个主类的成员重排逐成员代码tokens保持，移动的实例字段初始化表达式及相对依赖无改变，未拆文件；6文件/6控件成员审计0问题。新状态只有两个严格行高backing field、一个内部LabelPadding布局输入；复用现有ControlTheme中的有限转换器，无新引擎/框架/补丁标志。最终Setter在AXAML，生成业务段的padding由布局应用，时间分隔线绑定保留实际宿主/物理render-scale转换并在detach释放。Core只增加可选原生IValueConverter输入，两次无效通知修补和全部埋点已撤回。

专用Part定制只用编译AXAML的8类生成Style，核对关键Foreground/CornerRadius Setter及无Name/Loaded/Unloaded回退；时间行保留原整数Tag业务值。配置清除、兄弟/重挂与长寿命provider下root/text释放有实际证据。旧重套模板保留、历史strict失败/未配对帧、CT25/FW1/FW2保持原账。compat 58个manifest文件SHA全匹配；当前JIT、compat输出与AOT ILC输入管理DLL SHA一致为`10698d07908708848d68259568a69ce1c203df0dc7591241c54f1d68418de309`，ILC rsp明确指向compat项目，官方缓存/native未改。既有非本任务未提交文件哈希无变化。

### 原生DPI断言纠正

首轮AOT发布成功，无IL/CS警告，5条既有Homebrew部署版本链接警告保留。Headless明暗8场景（主家族及尺寸/Custom）全部运行通过，AOT/JIT共202对整帧RGBA0，48项校准符合，102默认/ID/slot相同。第一AOT二进制SHA为`b8caa07674094e2f2584ddac5a8c6f090dcee6f9ab0335e6f0a6d59220bf9af7`。

实际NSWindow managed浅色viewer/calendar已通过；time组在分隔线断言失败。原消费者使用render-scale物理像素换算，真实scale=2时3像素应为1.5 DIP，夹具错误要求3 DIP；这不是像素容差或产品线宽修改。仅将期望改为3/RenderScaling，原失败不计产品通过，保留202对Headless结果和两组原生结果；修正宿主必须增量重链AOT，产品源码不变。原生客户区受系统约束（viewer为1560×957），几何/配置检查不等于请求1220/1300高度的原生最终像素验收。

### 限定视觉审查 V1（已批准）

默认变化仅为农历工作日标记正确消费WorkdayMarkerColor，不再被模板内HolidayMarkerColor覆盖。本批固定2026-10-10为调休工作日，Mini和Fullscreen各一处4×4 marker：整帧比较每主题32个RGBA像素变化，变化区域全部位于两个marker，区域之外变化0。位置分别为(1513,209)及(1512,689)，单个4×4；未放宽容差、遮罩或裁剪比较。

| 主题 | 原错误节假日色 | 修复后工作日色 | 整帧变化 |
| --- | --- | --- | ---: |
| Light | #FFFF4D4F | #72000000 | 32 |
| Dark | #FFDC4446 | #72FFFFFF | 32 |

Main同轨迹96个兼容帧RGBA0，16个明确配置消费帧为任务指定的Global/字体/线宽等生效差异；30个带工作日marker帧各32像素。新参数的预期外观已按实际目标、实例优先级和专用Style审查，默认修复另取得下列限定批准。AOT/JIT的202个主场景及Custom静态帧全部RGBA0，默认/ID/slot102行相同。已查看明暗日历、时间行高/AM/PM滚动条、Content viewer及Custom实际弹层；原始尺寸/内容/业务条/裁剪和正常motion保持。

批准依据：[主题验收规范](../../../engineering/development/theming/verification.md#判定与差异处置)明确“任何未解释或未经批准的视觉差异都阻断该Control及受影响任务的验收”。V1仅申请上述默认工作日marker颜色修复，不覆盖CT25/FW1/FW2历史失败、未配对图、其他平台或Git提交。

批准登记：2026-11-03，问题ID为`["request_user_input_async","call_88b5f9a8ca4a411e95ffce8441abd9f4",0]`。请求原文：“是否批准 CT16 的限定视觉 V1：农历工作日标记从误用的节假日色改为 WorkdayMarkerColor？Light/Dark 各仅32像素变化，两个4×4标记之外0变化；不包含提交或 CT25/FW1/FW2 扩大验收。”用户明确回复：“批准这项限定修复”。仅覆盖本节V1；其他索引引用本登记，不复制批准原话。

### 当前 Mac 原生最终专项

实际NSWindow、scale=2的明暗viewer/calendar/time/custom共8场景，managed与NativeAOT各258项检查、200项动画结束检查全部通过。四Picker的Small/Middle/Large、Custom应用Style（padding13/radius17）、实际打开/关闭及回到预设均通过；旧scale错误断言保留为夹具失败，不计产品修复。修正宿主增量重链64.177s，复用同源产品库，二进制SHA为`8cbdd459cf58946eebc03f6cbb365a130d53f53d436381dd5b792b330fdddea6`；无IL/CS警告，仍为同5条Homebrew链接部署版本警告。原生专项结束后源文件逐字相同，所有会话窗口/进程正常退出。

原生客户区受系统约束，原始请求的高度没有移除或缩小来规避；本专项认证所列配置、几何、业务和生命周期路径，未声称完整请求尺寸的OS最终显示面像素一致或物理输入/IME验收。Headless 202对静态AOT/JIT0与本段原生行为证据分别记录，不混算为同一像素结果。

### 清理后的最终门禁

87临时case所在5文件（31,538字节）、外部驱动/源码备份/两次发布等612文件（1,618,782,199字节）及1,037个取证文件（53,931,140字节）均已删除；媒体Git审计clean。二进制未进入项目、报告或提交；会话进程已退出，净增常驻测试0。最终文字门禁会话也已在记录后删除。

持既有共享输出锁，在已清理工作树执行：

| 精确命令/范围 | 结果 |
| --- | --- |
| `dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj -c Release --framework net10.0 --no-restore --filter 'FullyQualifiedName~TimePicker\|FullyQualifiedName~DatePicker\|FullyQualifiedName~DateViewer\|FullyQualifiedName~Calendar'` | 357 PASS / 0 FAIL / 0 Skip，22s |
| `dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj -c Release --framework net10.0 --no-restore --filter 'FullyQualifiedName~ThemeTokenResourceProviderTests\|FullyQualifiedName~ThemeContextResourceBridgeTests\|FullyQualifiedName~TokenResourceExtensionTests'` | 15 PASS / 0 FAIL / 0 Skip，175ms |
| `dotnet build src/AtomUI.Desktop.Controls/AtomUI.Desktop.Controls.csproj -c Release --framework net8.0 --no-restore` | 含依赖，0警告/0错误，23.08s |
| `git diff --check` | PASS |

合计372项清理后精确门禁，不含87临时项、同组复跑或历史分包数量；没有跑全量或affected-wide。22 metadata/51 Own逐项归属与实际消费者路径、四类正式文档、CSV及任务行已同步；本次CSV实有722行metadata（其中CT16固定22）及1221个Own，主键均唯一，源SHA与已验收候选相同。V1取得上列明确批准后，CT16既定范围完成，无剩余工程子项；CT12–CT15既定结论及CT25/FW1/FW2边界未重开。无分支/worktree/子agent/提交。
