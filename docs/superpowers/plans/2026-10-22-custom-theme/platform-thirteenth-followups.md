# 第十三轮候选：Linker 反例验证的文件生命周期

现有feature/custom-theme、现有checkout，主agent独立执行，不创建分支／worktree，不发布NuGet。C7另行，不包含本次平台候选；本批尚未获得Git提交／推送／Actions授权。

## 实际 Windows 失败与定位

[第十二轮Actions40172623312](https://github.com/AtomUI/AtomUI/actions/runs/40172623312)的Windows本地化集成5项已通过，包括实际Official四消费者及fixture清理；build-host.log原占用失败未复现。Mac／Linux选测各6405项、三OS NativeAOT及两Browser通过。Windows选测5993项中5992通过，仅Linker的CorruptEmittedContractCannotReceiveASuccessReceipt(exception-region)在OutputFixture.Dispose删除Empty.dll时UnauthorizedAccessException；后续Gallery项目未运行，不计通过。

该反例先生成真实无效异常处理IL，再用可收集AssemblyLoadContext.LoadFromAssemblyPath加载实际输出文件，JIT调用Query并断言TargetInvocationException的InnerException是InvalidProgramException；finally只调用Unload，然后在同一方法内执行磁盘验证及fixture删除。Unload仅启动协作卸载，仍存活的Assembly／Method／异常等引用可延后释放；[微软卸载说明](https://learn.microsoft.com/en-us/dotnet/standard/assembly/unloadability)明确删除文件须等实际卸载完成。当前CI没有此Empty.dll的句柄owner记录，不能把推断写成已采集到的PID证据；它与上一轮子进程日志文件是不同对象和路径。

## 最小合同与验证步骤

1. 保留现有13种坏ABI反例、实际生成／变异的PE与PDB、真实JIT异常、磁盘receipt验证及原目录删除行为。真实Windows CI失败作为前证，不冒称Mac能复现Windows文件删除语义。
2. 仅将exception-region的运行时加载改为从该同一输出文件读出的内存字节，真实LoadFromStream后仍执行相同Query及异常断言；避免让运行时拥有fixture输出路径的映射。不加GC循环、重试、忽略异常、延时或删除兜底，不改变生产materializer与输出验证器。
3. 运行现有CorruptEmittedContractCannotReceiveASuccessReceipt精确13项，确认JIT与坏ABI仍真实执行，再跑所属Linker模块清理后精确验证。没有新建测试或临时探针，无新增永久用例。Mac通过只说明该平台的合同保持，实际Windows确认仍必须由下一轮CI给出。
4. 自审最小差异、git diff --check及状态文档，冻结独立快照后申请提交／推送／三OS两Browser验证；不得纳入C7或任何产物，不复用第十二批已经执行的Git授权。

## 本机验证及审查

已有13项坏输出用例在修改前的Mac基线通过（447ms），候选保持13项通过（424ms）；这不计Windows RED／GREEN，Windows前证仍是实际CI失败，GREEN尚待CI。首次--no-restore因本机该项目缺assets而在发现测试前退出，已保留准备日志并正常restore重跑，不算产品失败。最终所属Linker精确模块89项通过（约1秒），没有新增测试、临时hook、fixture、GC、重试或清理例外。

作者自审：diff仅改变exception-region的运行时镜像来源及原finally排版；真实无效IL、JIT调用、InvalidProgramException、原输出验证和receipt拒绝断言均保留，其他12种坏ABI不变，生产Toolchain代码未改。现有本机模块比CI选测多1项，按实际命令及发现数记录，不写成新增测试。准确Windows结果必须在新一轮CI确认；本轮Git提交、推送与调度仍需用户明确批准。

拟审四文件：本文件、platform-twelfth-followups.md、tasks.md及MaterializeTypeMapsTests.cs。排除C7所有源码／文档、临时验证和全部产物，不创建分支／worktree，不发布NuGet。

## 用户授权后的执行记录

用户明确批准第十三批4文件提交、推送及验证Actions。按冻结快照c8f0fcbf1398创建提交f24a9aba7710f3d192f50c6c3eb8e9a488deb087，逐项核对树中的blob及父提交b8325380f671，再推送既有feature/custom-theme并确认远端SHA。已调度[第十三轮Actions40175124825](https://github.com/AtomUI/AtomUI/actions/runs/40175124825)，三OS／两Browser、full=false，不发布NuGet，C7未纳入。此为执行后的未提交文字补充，不改写获批提交；Windows原失败与整轮最终结果仍待CI。

## 第十三轮实际结果

整轮已完成，七个 job 成功，仅 Windows 选测失败。Mac／Linux 各 6405 项、三平台 NativeAOT 与两种 Browser 通过；Windows 先前 12 项目 5993 项全部通过，包括原坏 IL 反例和 Linker 选测 88 项。原 Empty.dll 删除失败本轮未复现，本地化实际消费与清理继续通过；三平台子进程记录各 27 组 start／completed 完整配对。

Windows 随后的 Gallery 项目 412 项中 411 通过，唯一失败为 `TourSemanticPartHighlightTests.Tour_Semantic_Preview_Highlights_Parts_With_Pinned_Popup_And_Mask` 的首张 popup.root 卡片命中检查。保留完整失败几何：卡片中心 (1013.5,373.5)，卡片矩阵平移 (788,338)，实际命中相邻 PART_Row 矩阵平移 (788,267)，均 scale1。此证据尚不证明事件处理错误或产品修复；按原几何、裁剪与实际命中继续定位，不削弱原滚动／悬停合同。Windows 本轮合计 6404／6405 通过，不能报告整个 CT25 完成。

已在本轮结束后按既有第十四批授权调度 [第十四轮 Actions40176913761](https://github.com/AtomUI/AtomUI/actions/runs/40176913761)，源码为已批准并推送的 3e7efb341eebc9d28a1604c14bcbda5a4fb5b54b；C8 未包含在内。
