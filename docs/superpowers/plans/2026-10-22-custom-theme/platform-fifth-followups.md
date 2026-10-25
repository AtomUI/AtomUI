# CT25 第五轮平台验证跟进

## 已执行授权

用户批准29文件快照ace3c6085566后，在既有feature/custom-theme／同一checkout创建975fc8e345cd（纯圆形成员布局）及57ae294d4788（Progress Own／圆形状态与W10–W12），逐blob核对29文件与快照SHA256全部相同后推送既有远端分支。未包含后来DataGrid G1、Circle B3临时反例或原Range生命周期失败，未创建分支／worktree、未发布NuGet。

使用AtomUI既有凭证运行[验证Actions](https://github.com/AtomUI/AtomUI/actions/runs/40100020360)，源57ae294d4788，base19eef870b9db，full=false／all_platforms=true／browser=true。

## 当前已完成结果

本轮8任务全部结束：三OS NativeAOT fixture及Browser interpreted／aot共5项通过；macOS selected的11项目6,272项全部通过，并实际加载370项theme assets、释放application scope。Windows／Linux selected失败，整轮结论failure。

W10的真实managed compiler fixture在Windows执行原91项全部通过、无skip，10.186s；原6项POSIX skip已消除。Mac GalleryBase209项全部通过，13.436s，覆盖W11／W12；Windows止于DataGrid、Linux止于Desktop，后续GalleryBase未运行，Linux W12仍需后续CI证据。

| 跟进 | 实际失败 | 初步边界及下一步 |
| --- | --- | --- |
| W13 | Linux Desktop 4127PASS／1FAIL：MasonryLayoutTests.Default_Layout_Property_Values_Are_Stable为Test Case Cleanup Failure，DefaultRenderLoop.Add→Compositor→HeadlessUnitTestSession.EnsureIsolatedApplication抛跨线程InvalidOperationException | 失败位于测试宿主创建／隔离，未取得ColumnGap等产品断言失败；需要定向复现与线程／隔离证据，不先改Masonry默认或关闭region绘制 |
| W14 | Windows DataGrid 307PASS／2FAIL之一：ChangingHeaderTemplate_ShouldUpdateRowsWithoutChangingDetailsLifecycle(Visible)，预期unloadedDetails=0，实际1 | 保留原640×400、标题列／双滚动／可见details与模板切换，先记录布局／可见行及卸载对象，区分准备未稳定与真实生命周期变化 |
| W15 | 同上另一失败：Realizing_Row_Details_Near_Viewport_Bottom_Keeps_First_Row_Offset_Normalized，捕获两次中间状态neg78>firstHeight52，offset4120，first65，last79／77，max4285／4253 | 保留展开5／13／23、滚到底／回滚及scrollbar事件原触发，先检查实际normalization与事件发布顺序；不删除事件断言或取消details规避 |

上述Windows DataGrid用的仍是本轮已提交的WholeV1 Token；当前工作区G1未推送，不能将这些失败归因于G1。Linux Desktop失败也不能从Cleanup日志直接认定Masonry产品逻辑错误。

## 执行约束

已收齐本轮3项失败清单，先用精确模块／类filter复现、修复和清理后验证。当前Mac G1清理后的34项包含W14两种details及W15方法，均通过；这只说明Mac未复现，不表示Windows已修复。不会重跑无改动整轮，也不会自行运行本地change／full。后续任何提交仍须按具体可审查快照取得批准，本轮29文件授权已执行完毕。

后续候选、受控反例、修复和清理后模块结果见[第六轮跟进](platform-sixth-followups.md)；W13／W15机制修复不等于原Linux／Windows CI已通过，W14仍待平台失败阶段信息。
