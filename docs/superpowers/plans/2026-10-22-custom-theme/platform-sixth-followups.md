# CT25 第六轮候选与第五轮失败跟进

继续既有feature/custom-theme／同一checkout，不创建分支或worktree，不发布NuGet。第五轮[Actions](https://github.com/AtomUI/AtomUI/actions/runs/40100020360)已结束：Mac selected 6272通过、三OS AOT与两Browser通过，Windows／Linux selected失败。前批29文件的提交授权已执行完毕；本批尚无Git提交授权。

## W15 重叠视口的事件状态

保留1120×760 Window、1040×620 Grid、80订单、嵌套详情、展开5／13／23和滚到底／回滚／24次2DIP轨迹。原默认行高在Mac未复现；单变量显式44／52行高各取得2次有效失败，60通过。52例在scrollbar Value／Maximum通知时first32、neg74、firstHeight53，调用栈分别指向CommitOverlappingRangeViewport的首次ApplyRangeVerticalOffset与ComputeScrollBarsLayout。

候选只在该重叠视口容器提交点，先执行既有首行偏移归一化，再发布滚动条；不改逐行measure的容器遍历、原尺寸、详情、滚动条及事件断言。原4行高／2详情header检查6项通过；扩展5种header组合加4行高和2视觉捕获共11项通过、73.422s。W15旧新只切换DataGrid.Virtualization.cs，既有G1／B3和其他源码保持不变，单文件当前字节备份／finally恢复校验一致。

### 原画面及资格控制

初次捕获的old 5／new 8组AA非零，全部在标题Window文字bbox(535,15)–(585,26)，表格一致。实际WindowTitleBar有Foreground过渡，原三帧门槛过早；保留该失败计数并查看差异后，清理194文件。窗口正常motion不关闭，Grid原IsMotionEnabled=false保持。

新门槛读取实际Visual的所有Transitions，等待至少max(500ms, Duration+Delay+16ms)、全部属性无动画，再要求3次相同compositor帧。Application／Window均为实际明暗，scale1，main／AA／1DIP为独立完整历史；扰动只把Grid左Margin+1、右-1并核实原点40→41。

合格两版各84帧，共168帧。84对旧新全图RGBA精确0、所有几何及行/offset/max/viewport记录相同；56组AA全部0，56组1DIP全部检出（21,358–28,398像素）。已查看真实明色默认、原滚动底部及旧新暗色回滚后裁剪详情；原嵌套内容、滚动条与裁剪保留。没有mask、配准、容差或默认视觉变化需要批准。

最终媒体193文件按marker清理、audit无发现；删除3临时C#，8份TRX压缩为计数／有效失败／SHA256后删除，净新增常驻case0。临时驱动的xUnit同步task／CancellationToken提示仅属于已删除驱动，未写入产品。清理后的模块、NET8及Windows出口见后续结果。

## W13 图片回调所属dispatcher

原Linux失败发生于Headless隔离应用建立，未取得Masonry默认值断言失败。实际Avalonia.Headless12.1.4、net10 DLL SHA256为29cbfa10e0a4631ae9c762eb718d72a82419656419fb513a9d77f1741e8f70de；运行时重置会清空全局UI dispatcher，最先创建者成为UI。

临时受控反例只清空实际s_uiThread，保留线程store／队列／服务，finally恢复。新Thread分别执行Full／Thumbnail／AsyncImage进度回调，活动／disposed共6项，旧版6项全部因工作线程安装全局UI dispatcher而失败。候选entry保存创建UI线程的Dispatcher.CurrentDispatcher，controller使用所属Visual.Dispatcher；进度及成功／失败提交不再从后台查询全局UI。未新增公共API、反射、订阅或资源，原generation、取消和结果lease校验保留。

同6项及既有34项ImagePreviewer加载和1项Masonry精确方法共41通过，32.312s。临时反射接缝已删除，2份TRX压缩后删除，净新增常驻case0。该证据证明独立的隔离竞态机制已修复，不表示已经查明原CI那一次后台回调来源，或Linux原失败已经复跑通过。

## W14 保留断言，补失败上下文

Mac原Collapsed／Visible及额外44／52／60行高共5组合，没有模板切换卸载，不能宣称Windows已修复。原常驻方法只把卸载计数改为携带阶段／行号／视口和布局状态的失败集合，仍从Show之前开始记录且不清零，两次必须为空的断言保持；无新增case或文件日志。下次Windows若失败可区分初始化、替换或恢复阶段。

## G1与Progress B3

[DataGrid G1](data-grid-default-results.md)为78字段V2默认计划切片，312项默认值／类型／ID／slot一致、216帧严格对照及真实managed／AOT各37已完成；G2／G3仍开放。

[Progress B3](progress-circle-fit-results.md)为当前百分比／完整字体及激活图标适配和异常退出恢复切片，44 RED→GREEN、用户批准所示视觉变化、真实Mac managed／AOT各130、清理后42和NET8通过。完整家族和原生最终像素仍开放。

## 当前出口

清理后AsyncImage／Avatar原15项9.218s、ImagePreviewer加载与Masonry精确方法35项31.183s、DataGrid详情／行头／range虚拟化／paging-grouping／Token／Semantic原73项31.069s全部通过。NET8首次因前序net10测试restore覆盖assets而缺net8目标，0.494s准备失败；开启正常restore后DataGrid及Controls／Desktop依赖Release net8构建25.349s成功，0警告／0错误，源码未因该准备问题改变。9个修改C#整文件成员审计0问题；测试准入audit15项目通过，20份相关文档本地链接无缺失，diff检查通过。

提交前另补“已排队滚动立即RefreshRows”的44／52行高两项检查，29.395s均通过。源码在构造nextIndex后已先恢复viewport anchor，再计算desiredOffset，未取得所担心的两次应用偏移反例，因此没有推测性修改。该临时源码／TRX已删除，产品SHA256仍为68e19fa5bd0960a37d14e579a5a8eca7a4f90c5c23003f960dea3e5133053c03，与168帧合格取证相同；删除后原Windows两个失败方法的精确3项13.889s通过；Mac通过不替代Windows结论。

其他平台的原始失败、原生最终像素及完整CT任务仍开放；本批不包含既有Range生命周期失败临时测试或任何媒体／二进制。

## 本批提交审查

本批拟在现有feature/custom-theme创建一个提交，随后推送已有远端分支，用AtomUI既有凭证运行full=false／all_platforms=true／browser=true验证Actions。内容为DataGrid G1默认计划、Progress B3已批准视觉修复、W13所属dispatcher／W15提交顺序及W14原断言的失败上下文，连同正式文档和进度目录。

作者按requesting-code-review模板完成自审，遵循用户不使用subagent的要求；这是作者自审，不记作独立评审。范围内Critical／Important／Minor均0；公共API、Token字段／slot／ID保持，样式仍声明在ControlTheme，无代码Semantic样式回退；新回调所有权无反射或新增订阅，W15不从首行扫描总行数。既有常驻case数量不增加，单次反例与专用媒体均清理。

明确保持开放：W14原Windows失败尚未修复；W13独立机制证据不是原CI后台调用来源归因；W13／W15目标OS复跑、原生最终像素、DataGrid G2／G3、Progress其余形态及完整CT任务均不由本次审查代替。这些项继续研发或由下一轮CI提供具体证据，不记作本包验收通过。

所有临时测试、Range生命周期失败反例、工程／媒体／二进制排除；不创建分支／worktree，不发布NuGet。之前29文件授权已执行，本批依据AGENTS.md:148“未经明确指令不得提交”等待新的明确提交指令。等待期间下列审查文件字节冻结，manifest记录完整SHA256；其他已授权工作继续。

文件清单（30项）：

- `docs/architecture/systems/image-loading/pipeline-and-lifecycle.md`
- `docs/controls/desktop/data-display/data-grid/changelog.md`
- `docs/controls/desktop/data-display/data-grid/implementation.md`
- `docs/controls/desktop/data-display/data-grid/overview.md`
- `docs/controls/desktop/data-display/data-grid/token.md`
- `docs/controls/desktop/data-display/image-previewer/changelog.md`
- `docs/controls/desktop/data-display/image-previewer/implementation.md`
- `docs/controls/desktop/feedback/progress-bar/changelog.md`
- `docs/controls/desktop/feedback/progress-bar/implementation.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/control-inventory.csv`
- `docs/superpowers/plans/2026-10-22-custom-theme/data-grid-consumers.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/data-grid-default-results.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/drawing-consumers.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/platform-fifth-followups.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/platform-sixth-followups.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/progress-circle-fit-results.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/progress-token-consumers.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/tasks.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/token-contract-review.csv`
- `docs/superpowers/plans/2026-10-22-custom-theme/visual-coverage.csv`
- `src/AtomUI.Controls/ImageLoading/ImageLoadController.cs`
- `src/AtomUI.Controls/ProgressBar/AbstractCircleProgress.cs`
- `src/AtomUI.Controls/ProgressBar/CircleProgressIndicatorPanel.cs`
- `src/AtomUI.Desktop.Controls.DataGrid/DataGrid.Virtualization.cs`
- `src/AtomUI.Desktop.Controls.DataGrid/DataGridToken.Defaults.cs`
- `src/AtomUI.Desktop.Controls.DataGrid/DataGridToken.cs`
- `src/AtomUI.Desktop.Controls/ImagePreviewer/ImagePreviewEntry.cs`
- `src/AtomUI.Desktop.Controls/ProgressBar/Themes/AbstractCircleProgressTheme.axaml`
- `tests/AtomUI.Desktop.Controls.DataGrid.Tests/Theme/DataGridTokenAlignmentTests.cs`
- `tests/AtomUI.Desktop.Controls.DataGrid.Tests/Virtualization/DataGridRowHeaderRecycleIssue475Tests.cs`
