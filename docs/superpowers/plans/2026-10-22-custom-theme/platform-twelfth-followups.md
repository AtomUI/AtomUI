# 第十二轮候选：ILC8 构建日志的子进程生命周期

沿既有feature/custom-theme及checkout，不创建分支／worktree／subagent，不发布NuGet，不自行提交。C7色谱研发另行，不混入本平台候选。

## 实际前证与边界

第十一轮Actions40168600469已经结束：三NativeAOT、两Browser、Mac／Linux选测全部成功，后两者各6290项。Windows前八项目5664项通过，本地化其余4项通过，Official完整四消费者依次构建exit0／0／0／预期1且消费断言完成；只在清理build-host.log时IOException。三OS本地化journal各54条、27个开始／完成全部配对，没有取消／超时。

Windows Restart Manager实际返回status0，文件为fixture/artifacts/tools/registration/net8/ilc/build-host.log，Owner PID5944／5124／5164，应用均.NET Host；开始时间分别06:31:17.152270／.152352／.168234 UTC，处于Pack AtomUI.Generator阶段（06:30:39.120开始）。runner最终逐个终止同三PID的dotnet遗留进程。没有记录这些PID的命令行，因此不能把它们的具体MSBuild／compiler角色当实测事实。Windows主消费异常为空，清理失败仍保留原IOException，未吞错或重试删除。

build_host.py是该日志唯一写入者；其subprocess.run把实际dotnet build的stdout／stderr转至日志，却没有关闭嵌套构建的持久服务器／节点复用。外层fixture已有--disable-build-servers和-nr:false，不自动变成这个新CLI进程的命令行参数。依据微软[dotnet build](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-build)与[MSBuild nodeReuse](https://learn.microsoft.com/en-us/visualstudio/msbuild/msbuild-command-line-reference)，拟在日志所属的实际子进程调用边界显式禁用持久构建服务器和节点复用，使其子节点随该次构建结束。

这是有现场证据支持的修复候选，Mac不能模拟Windows拒绝删除语义；先验证真实配方生成的进程调用与实际编译，再用获批Windows CI核实文件句柄退出。不能仅据本机包消费通过就称Windows问题已修复。

## 执行与验证

1. 搜索既有测试；当前没有直接覆盖build_host.py日志所属子进程参数的测试。临时Python探针放系统临时目录，导入真实配方、使用已经存在的定版runtime源码（不clone），在实际subprocess.run调用边界截获dotnet参数。源校验、项目生成和日志打开照常；截获点返回明确非零终止，不产生伪成功bundle。测试后恢复唯一生成的thin csproj原字节、删除探针及输出。
2. 原调用缺少两项生命周期参数应RED；最小候选仅添加--disable-build-servers及-nr:false，不改源版本／依赖／build目标／锁／bundle身份校验规则或现有异常，源码哈希照常进入配方身份，重复验证GREEN。
3. 使用现有定版源码进行真实managed-only编译、验证产物身份／复用／实际日志无自身子进程残留；需要本机当前实际包消费时只跑精确Official专项。所有本机结果限定Mac，Windows句柄仍等CI。
4. 清理临时测试／编译产物，运行改动所属工具检查及最终精确模块，文档同步并冻结独立快照，再申请Git提交／推送／三OS两Browser验证。不得包含C7、临时源码、媒体或所有产物。

## 已完成的本机验证与清理

临时探针通过真实配方、既有定版源码及实际进程调用边界取到完整命令：原两项均失败（0.819秒），候选两项均通过（0.760秒）。非零17照常返回，未构造伪成功bundle；每次唯一生成的thin csproj均逐字节恢复。该检查证明命令生命周期参数，不冒充Windows句柄复现，Windows现场失败仍是实际前证。

实际managed-only编译成功，9个bundle文件逐项SHA与capability相同；真实ilc.dll --help退出0，同源--ensure实际报告reused=true。Mac对精确build-host.log的lsof无占用；这不代替Windows文件锁证据。外部真实编译产物及独立临时探针已经删除，文件SHA、真实命令／输出、identity及清理记录保留为文本，thin csproj已恢复。

临时验证清理后，8项工具策略检查13.476秒通过；在已提交0c6d8445679e产品基线上运行完整Official四消费者约141秒通过，14个进程／28条journal全配对，系统临时fixture已删除。C7两产品源先按精确SHA保存并暂回HEAD，验证后逐字节恢复，因此上述包消费证据不把C7未审查实现包含在内。没有本地change／full，没有修改任何超时、原消费断言、bundle身份规则或源码版本。

作者自审：产品变更仅build_host.py新增一条说明及两个CLI参数，SDK支持边界未扩大，未引入运行时依赖、全局进程操作或异常吞并。下一次Windows CI仍是修复确认门槛；当前不能宣布Windows已修复。

## 待审五文件快照

本文件、platform-eleventh-followups.md、tasks.md、正式Toolchain build-and-verification.md及build_host.py。拟在既有feature/custom-theme创建一个提交、推送已有远端分支，再用AtomUI凭证运行三OS／两Browser验证Actions，full=false。排除C7、临时测试及全部产物，不创建分支／worktree，不发布NuGet。具体blob／整体SHA保存于外部SDD，不在本文自引用；尚未获新Git授权，不执行提交或调度。
