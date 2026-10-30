# Windows 构建任务 DLL 锁定排查与验证

日期：2026-09-11。本文保留当时的项目与工具名称；其中旧注册生成器不再属于已采纳的 TypeMap 目标架构。当前架构规则由 [构建与打包](../../architecture/foundations/build-and-packaging.md) 维护。

## 根因与变更

Rider 构建 `AtomUI.Controls` 时，常驻 MSBuild 进程持有 `AtomUI.Build.Tasks.dll` 的影子副本，复制重试后报
`MSB3027`/`MSB3021`，同时导致 `.artifacts` 无法完整删除。现场进程中确实加载了十多份影子任务 DLL。
哈希目录没有消除文件句柄生命周期：确定性编译可产生相同内容、不同时间戳，Copy 仍可能尝试覆盖已加载的副本。

本次移除 shadow staging 和 ShadowKey 盖章。SDK 通过
[RoslynCodeTaskFactory](https://learn.microsoft.com/en-us/visualstudio/msbuild/msbuild-roslyncodetaskfactory)
编译源码适配器；业务任务运行在每次调用后退出的 .NET 10 进程中。属性、item 和诊断通过纯值协议传输。
取消和异常路径也等待 worker 退出，然后清理临时请求目录。

兼容性变化：构建工具要求 .NET 10 SDK/runtime，原有 Build Tasks 进程内加载和影子目录入口不再保留。
产品库的目标框架不变。NuGet 同步交付源码适配器、worker、运行配置与依赖；LinkedPublish Generator 自己复制
Analyzer 所需依赖，避免依赖旧输出目录里的残留文件。

## 验证结果

在 Windows、.NET SDK 10.0.401 上执行以下验证，未运行全仓测试。

| 验证 | 结果 |
| --- | --- |
| 修改前，在同一 MSBuild 进程中执行任务后强制覆盖 DLL | 稳定失败，`MSB3021` |
| 修改后，同一进程内连续执行、覆盖 DLL、删除工具目录 | 通过 |
| 执行中取消 worker，再打开其独占文件句柄 | 通过 |
| 从空工具输出目录打包，再执行包内任务与取消验证 | 通过 |
| 原报错 worktree：Controls build → Build.Tasks Rebuild → Controls build，保留节点复用 | 三次均通过，零警告、零错误 |
| Build Tasks 模块，排除下述既有 XLIFF 格式失败 | 93 项通过 |
| Generator 模块 | 473 项通过 |
| Gallery win-x64 NativeAOT publish | 完成任务链路和托管编译，平台链接阶段受本机缺少 C++ linker 阻塞 |

对应命令：

```powershell
pwsh -NoProfile -File scripts/verification/verify-build-task-isolation.ps1
pwsh -NoProfile -File scripts/verification/verify-build-task-isolation.ps1 -BuildPackage
dotnet test tests/AtomUI.Build.Tasks.Tests/AtomUI.Build.Tasks.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName!~Repository_Xliff_Files_Use_Canonical_Writer_Output'
dotnet test tests/AtomUI.Generator.Tests/AtomUI.Generator.Tests.csproj --framework net10.0 --no-restore
dotnet build src/AtomUI.Controls/AtomUI.Controls.csproj -nodeReuse:true
dotnet build src/AtomUI.Build.Tasks/AtomUI.Build.Tasks.csproj --no-restore -target:Rebuild -nodeReuse:true
dotnet build src/AtomUI.Controls/AtomUI.Controls.csproj --no-restore -nodeReuse:true
dotnet publish controlgallery/AtomUIGallery.Desktop/AtomUIGallery.Desktop.csproj -c Release -r win-x64 -p:GalleryPublishTrimmed=true -p:GalleryPublishAot=true
git diff --check
```

Build Tasks 全模块初次运行的唯一失败是
`Xliff21WriterTests.Repository_Xliff_Files_Use_Canonical_Writer_Output`，指向 BorderBeam 的 `en-US.xlf`。
已使用修改前保留的 netstandard2.0 任务 DLL 重算并确认同样不符合 canonical 格式；本次没有修改该 XLIFF 或 writer。
后续执行范围明确排除这一项，不将其计为通过。

## 现场迁移

修复同步到主工作区与原报错的 ImagePreviewer worktree，保留其中既有的控件修复。核实并停止原日志指定、仍持有
旧影子 DLL 的 MSBuild 进程 12464，释放已经存在的文件锁；没有结束 Rider 界面进程。
该操作只处理旧实现遗留的进程状态，新执行链路不依赖构建前结束 IDE、全局关闭节点复用或定期清理影子目录。
