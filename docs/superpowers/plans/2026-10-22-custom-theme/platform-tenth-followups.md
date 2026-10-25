# 第十批跟进：C4/C5与平台选测

沿现有feature/custom-theme和checkout，不创建分支／worktree，不使用subagent，不发布NuGet。作者自审；无独立子agent评审。Git提交／推送／Actions必须按本批具体快照另获批准，前九批授权均不扩大。

## 问题及最终范围

- C4：既有空值斜线和选择环不消费有效颜色／线宽。四产品文件复用原资源宿主与ControlTheme，保留默认红2，明确0不绘制；无新公开API、Own、资源编号或订阅。ColorBlock纯成员布局与语义改动分开，相关初始及补充视觉已获用户批准，限定旧基线3像素严格按原表保留。
- C5：旧颜色／尺寸后台位图晚完成会覆盖当前结果，增加一个请求版本；离树改色后同实例回树仍旧位图，挂载边界重新发起运行时像素计算。没有新timer、disposable或视觉子树；旧任务释放自己的PooledList，不替换／dispose新位图。ColorSlider公共事件region为纯布局，另外分开。完整证据与挂载视觉待审范围见[color-slider-bitmap-order](color-slider-bitmap-order.md)。
- Grid：原测试Show只泵一次UI队列，却立即读取Task.Run完成后的分组头。受控请求重用原测试与真实LocalSource投影，旧1失败2.4091s，候选1通过2.2312s。只等待公开LoadState Ready或Error，再保持原非空和标记断言、数据、460×200 Grid、540×320 Window；无产品行为变更、Skip或延迟重试。
- Windows NuGet：实际Import保留NuGetPackageRoot、包／版本／buildTransitive文件，但采用反斜杠。既有XML读取只统一斜杠，完整路径相等断言不放宽；不接受其他宏、包、版本或文件。Windows旧CI失败为有效前证，本机真实聚合包消费是POSIX正对照，最终仍需实际Windows。
- Timeline：第八轮Mac一次itemHeader命中失败，第九轮Mac全部通过；本机原流程18个card命中且render前后坐标相同。只让失败消息包含实际hit、变换前后位置、可见／hit-test、裁剪、viewport／extent／offset、visual/logical祖先和sticky位置，不改变原1280×900、内容、滚动或hover断言，不把诊断增强称根因修复。

## 已有平台结果与未决边界

[第九轮](https://github.com/AtomUI/AtomUI/actions/runs/40160735288)的source769a26e28，三NativeAOT／两Browser及Mac／Linux各6290全通过，Windows Grid308/309。Windows第八轮的两个本地化失败（import路径、build-host.log清理占用）保留；第九轮未运行到它们。原两次Linux600秒总体超时不能由本轮一次成功倒推原因。本机句柄探针两种命令均未复现占用，未猜测修改Toolchain／超时或吞掉清理错误。

C6输入框离树改色后的旧HEX，原版与候选相同，列入后续CT19修复；不包含在本批产品改动，也不因此宣称完整家族通过。Gradient独立View无活动stop时Value赋值不等同于选定第一节点，挂载原生补充限公开ColorPickerView，不扩大证据。

## 验证及清理

- C4旧10项2通过8失败，候选10及补充5功能／生命周期全部通过；原队列GC对照和TimeViewCell controlOnly正对照独立记录。
- C5两个真实完成顺序反例均旧失败／候选通过；同实例挂载一项旧White、期望Blue，有效RED，候选GREEN。临时门闩／Task观测／测试／旧源和原生工程均已删除，常驻测试净新增0。
- C4初始240帧、补充范围及C5顺序已获视觉批准；752专用媒体已清理，数值／SHA保留。挂载补充60帧，20 A/A为0、20个1DIP全检出，默认／复位0；六对Alpha变化各1722像素，仅x282–497/y278–285，仍待单独批准。
- Mac正常两Picker流程managed／AOT各404项；增加挂载更新后的公开ColorPickerView明暗managed／AOT各82项，均读取实际Bitmap和物理track尺寸，不能代替原生最终截图验收。AOT的Homebrew macOS链接目标警告如实保留，不宣称旧macOS已支持。
- 最终临时源删除后：Desktop ColorPickerPinnedOpen／SemanticPart／TimePickerDisplayTime 23项5.0879s；Grid SemanticPart 12项2.8391s；Timeline原1项3.3227s；真实Aggregate NuGet消费1项46.9796s；Core Provider／ContextBridge 17项1.8225s。精确filter与日志在.sdd/platform-tenth-followups及color-picker-drawing中，不是本地全量／change scope。
- NET8 Release ColorPicker编译零警告／错误（25.15s MSBuild），Core／NET8记录的五份产品源SHA与当前逐一相等；这两项输入不含临时Desktop测试，未用临时assembly代替清理后的Desktop模块验证。
- 两触及控件Roslyn成员审计files2/controls2/issues0；两份纯布局逐成员token、imports和静态初始化相对顺序等价。无新忽略PropertyChanged的旗标、动态模板重建、全量实例化、反射旁路或全局缓存；不声明性能提升百分比。

## 提交方案

待挂载视觉批准与本批Git明确批准后，拟在现有分支创建两个提交：先ColorBlock／ColorSlider两文件纯成员布局，再其余C4/C5、三项既有测试修正／诊断和文档；推送已有远端feature/custom-theme，运行三OS／两Browser原验证Actions，full=false、不发布NuGet。不包含C6研发、临时验证、媒体、二进制或任何新分支／worktree。具体文件及最终blob快照随后写入本记录；未获批准前不执行提交。


## 具体17文件快照

- `docs/controls/desktop/data-entry/color-picker/changelog.md`
- `docs/controls/desktop/data-entry/color-picker/implementation.md`
- `docs/controls/desktop/data-entry/color-picker/token.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/color-picker-drawing-results.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/color-picker-drawing.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/color-slider-bitmap-order.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/platform-localization-timeout.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/platform-tenth-followups.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/tasks.md`
- `src/AtomUI.Core/Theme/Resources/OwnerTokenResourceExtension.cs`
- `src/AtomUI.Desktop.Controls.ColorPicker/ColorBlock.cs`
- `src/AtomUI.Desktop.Controls.ColorPicker/ColorSlider/ColorSlider.cs`
- `src/AtomUI.Desktop.Controls.ColorPicker/Themes/ColorBlockTheme.axaml`
- `src/AtomUI.Desktop.Controls.ColorPicker/Themes/ColorView/ColorSpectrumTheme.axaml`
- `tests/AtomUI.Desktop.Controls.DataGrid.Tests/Theme/DataGridSemanticPartTests.cs`
- `tests/AtomUI.Localization.IntegrationTests/LanguagePackEndToEndTests.cs`
- `tests/AtomUIGallery.Tests/ShowCases/TimelineSemanticPartHighlightTests.cs`

第一提交仅应用两份已经逐成员token校验的纯布局patch；第二提交的最终17个文件blob严格按.sdd/platform-tenth-followups/review-snapshot.json核对。当前index为空，已执行git apply --cached --check而未修改index。拟提交信息：refactor(ColorPicker): 规范绘制控件成员布局；fix(Theme): 修正颜色绘制与位图生命周期并校准平台选测。重挂六对Alpha视觉仍待单独批准，Git批准不替代该视觉批准，获得两者后才执行本批提交。
