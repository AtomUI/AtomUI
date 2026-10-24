# CT-25 第四轮跨平台验证后续

状态：调查及整改中，不自动提交，不创建分支／worktree，不发布NuGet。用户批准的34文件快照已准确形成fc9896fb6和e9bc9e707两个提交并推送，授权已执行完毕；后续改动另行交审查。

## 完整运行结果

[Actions 40089581608](https://github.com/AtomUI/AtomUI/actions/runs/40089581608)，source=e9bc9e707，base=19eef870b9db62b1eb1c3b0897619ee8d466af46，full=false，all_platforms=true，browser=true。运行已经结束，没有取消或局部重跑掩盖失败。

- 三OS真实NativeAOT fixture、两种Browser fixture全部通过。
- Windows选测在Build.Tasks证据出口停止：total91、executed85、passed85、failed0；未执行的6项正是TypeMapPublishGateTests中因POSIX假编译器而在Windows主动Skip的版本矩阵。
- Mac和Linux都通过前9个选测项目，随后GalleryBase失败；Gallery项目之后的选测不能推定通过。
- Mac GalleryBase为206通过／1失败：IconGalleryTests.Text_Changes_Are_Debounced_And_Use_The_Latest_Query，预期一次reload而实际0。
- Linux GalleryBase为204通过／3失败：GalleryStickyTabsHostStickyMirrorTests的Selection_Is_Live、Clicks_Reach_The_Real_Tabs、Restores_The_Strip，实际SelectedIndex均停在0。

这是CI选择范围的结果，不称为全量回归。原第三轮Desktop和Windows清单故障已不在本轮失败列表中；不能因此把所有任务或Windows后续项目填作完成。

## W10：假编译器的跨平台执行

先保留严格TRX证据门槛，不把Skip当Pass或放宽计数。现有6项数据继续验证同样的编译器版本／目标／TypeMap边界；将仅支持POSIX的shell假编译器替换成可真实执行的托管apphost探针。优先在系统临时目录生成并清理，一类只构建一次，逐case使用独立version文件；不增加常驻case、不向产品注入测试分支，不引入新的测试项目或全量运行入口。已在现有TypeMapPublishGateTests中实现，一类构建一次系统临时net10 apphost，六组原数据逐项复制独立版本文件。只在测试中构建，无生产代码注入、无外部NuGet依赖，constructor失败及Dispose均清理自有工程。Mac／Linux各14项全部执行通过；Windows实际执行仍待下一轮CI，不提前声称平台通过。

## W11：IconGallery debounce

先以现有精确用例复现，记录DispatcherTimer是否启动、请求时间、实际Tick和reload；验证原120ms debounce以及最新查询、显式搜索取消、detach和主题切换。现有测试用固定200ms MainLoop窗口；不能直接加任意长sleep或放松一次reload的结果。原用例在Mac聚焦及五次独立进程重复中通过；只读计时记录确认timer绑定UI dispatcher，最后文本约23ms，reload约186ms。保留120ms产品时长，临时加入240ms高优先级UI工作后，同一固定200ms MainLoop得到相同的0次reload，退出时timer仍enabled；改为有界DispatcherFrame等待可观察reload后，同一负载约315ms仅reload一次。由此证明原测试把固定退出时刻当成了后台回调完成保证；尚无CI当时的队列轨迹，不将具体负载来源写成事实。正式测试使用5秒失败保护和5ms条件观测，正常通过即退出，仍断言不能立即reload、最终一次且使用最新查询。显式搜索／detach／主题切换取消的原断言保留。产品计时与搜索源码未改。

## W12：Sticky tabs 的真实输入

保留1000×600窗口、200高header、20×60内容、400滚动位置、真实鼠标事件及取消吸顶场景。先检查item／container Bounds、到window矩阵、viewport／extent／offset、裁剪祖先、elevation z-order与实际pointer hit，再做单变量对照。不得以直接设置SelectedIndex、移除高度限制或绕过滚动／吸顶替代输入验收。原HeadlessDrawing宿主在Linux精确复现3失败／2通过。只读记录：点击(314,87)及恢复后的(314,287)均位于页签内，920×488 viewport、400／0 offset、8 DIP可见垂直滚动条正常；祖先WindowVisualLayerClip的Bounds=(36,27,929,529)，但FillContains返回false，实际hit退到窗口外层Panel。Mac没有这层Linux模板裁剪，命中实际Frame。

仅把测试宿主切为UseSkia／UseHeadlessDrawing=false，保留产品圆角裁剪、窗口大小／标题／滚动条件；同Bounds的实际几何FillContains变为true，真实鼠标点击命中页签TextBlock，5项通过。真实字体使tab信息条高由41变42、extent1441变1442，viewport和scrollbar不变，未把所有几何称为相同。最初Skia准备因容器缺libfontconfig而5项初始化错误，安装该原生依赖后同一已构建测试5项通过，该轮不计产品RED。Linux arm64容器不冒充CI Linux x64。正式原有点击测试补左／中／右位置、真实hit祖先和hover，保留viewport／extent／offset及可见滚动条断言；两个readback调用现在释放返回bitmap。未修改产品输入或裁剪代码。

## 共同出口

各失败以聚焦测试修复；临时验证和专用产物先清理，再运行改动模块的精确filter或明确path的iterate及必要NET8／打包检查。完成审查后申请新的提交／推送／验证授权；本轮不自行重跑CI，也不运行本地change或full。

## 本包验证与测试处置

清理后的精确filter覆盖IconGallery、StickyHost、maximize及相关zoom八类42项，Mac31.233秒／Linux36.521秒全部通过；TypeMapPublishGateTests原14项Mac7.631秒／Linux5.525秒全部执行通过，无Skip。范围不是整个GalleryBase项目或本地全量。11个修改C#文件成员审计0问题；测试准入审计15项目通过。原Range生命周期失败临时源仍属另案，未混入本次提交范围。

临时只读trace、队列负载注入和CT25Compiler临时目录已删除。apphost fixture合并到原有TypeMapPublishGateTests文件，净新增常驻case0；它仅支持既有六项真实子进程的编译器版本门禁，跨平台跳过会使严格TRX证据不完整，已有纯字符串合同用例不能验证真实工具执行。每类只构建一次无依赖控制台项目，Mac原14项整次含构建7.631秒、Linux5.525秒；未新增测试项目或全局入口。该成本用于保留既有核心发布门禁，不能用字符串mock替代实际进程。Linux当前Progress源码的同11类204项48.073秒通过，Mac对应204项30.802秒通过。专用Docker及匿名产物volume已删除，751,486,484字节专用产物随volume释放，TRX已转为紧凑计数／失败信息／SHA256文本记录并删除原文件；Windows、CI后续选测与全部平台最终验收待批准提交后的新运行。

## 本次提交审查范围

本轮共29个文件；拟在现有feature/custom-theme创建两个提交：先仅应用AbstractCircleProgress的45成员token相同布局patch，再提交Progress13字段V2／两Own接线、圆形尺寸和结果状态修复、两个测试宿主选项、W10–W12及文档。全部所示默认／配置视觉变化已经用户独立批准，本项Git审查不扩大视觉许可。P1的字号／图标极值、其余字段、Global、原生最终像素和全家族继续开放。

自审：新增绑定符合ControlTheme优先；内部计算通过只读属性投射，不写本地字号／Part尺寸；语义indicator为实际Panel，公开route和ContractType不变，compiled专用Style命中及退出恢复已有证明。未增加资源编号／重复Own identity、代码样式回退、生产事件订阅或生产计时变更。纯布局patch SHA256为16eb6b69bd4d814c0a75fb35c8fc8a41145182dee5e4b31f3692607703790ace，先独立成提交。未发现本批范围内新增阻断问题；Windows真实执行、原生像素及完整CT验收不由本次聚焦结果代替。

明确排除：Range生命周期临时失败、全部临时测试／工程／媒体／产物。只推送已有远端分支并运行三OS／两Browser验证workflow，不创建本地／远端新分支或worktree，不发布NuGet。前批34文件授权已执行完毕，本批等待用户按AGENTS.md:148明确批准后才提交／推送／调度。文件字节快照与完整SHA256保存在本地审查manifest；等待批准期间不修改这些文件。

文件清单：

- `docs/controls/desktop/feedback/progress-bar/changelog.md`
- `docs/controls/desktop/feedback/progress-bar/implementation.md`
- `docs/controls/desktop/feedback/progress-bar/token.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/control-inventory.csv`
- `docs/superpowers/plans/2026-10-22-custom-theme/drawing-consumers.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/platform-fourth-followups.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/platform-third-followups.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/progress-circle-results.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/progress-rounding-results.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/progress-token-consumers.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/progress-token-results.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/tasks.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/token-contract-review.csv`
- `docs/superpowers/plans/2026-10-22-custom-theme/visual-coverage.csv`
- `src/AtomUI.Controls/ProgressBar/AbstractCircleProgress.cs`
- `src/AtomUI.Controls/ProgressBar/CircleProgressIndicatorPanel.cs`
- `src/AtomUI.Controls/ProgressBar/CircleProgressPanel.cs`
- `src/AtomUI.Desktop.Controls/ProgressBar/ProgressBarThemeBindings.cs`
- `src/AtomUI.Desktop.Controls/ProgressBar/ProgressBarToken.cs`
- `src/AtomUI.Desktop.Controls/ProgressBar/Themes/AbstractCircleProgressTheme.axaml`
- `src/AtomUI.Desktop.Controls/ProgressBar/Themes/CircleProgressTheme.axaml`
- `src/AtomUI.Desktop.Controls/ProgressBar/Themes/DashboardProgressTheme.axaml`
- `src/AtomUI.Desktop.Controls/ProgressBar/Themes/ProgressBarTheme.axaml`
- `tests/AtomUI.Build.Tasks.Tests/TypeMapPublishGateTests.cs`
- `tests/AtomUI.Desktop.Controls.Tests/AvaloniaTestApp.cs`
- `tests/AtomUI.Toolkits.GalleryBase.Tests/AvaloniaTestApp.cs`
- `tests/AtomUI.Toolkits.GalleryBase.Tests/Controls/GalleryStickyTabsHostStickyMirrorTests.cs`
- `tests/AtomUI.Toolkits.GalleryBase.Tests/Controls/IconGalleryTests.cs`
- `tests/AtomUI.Toolkits.GalleryBase.Tests/Controls/ShowCaseZoomSuppressesStickyElevationTests.cs`
