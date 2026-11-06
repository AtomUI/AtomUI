# Windows 11 Gallery Native AOT 发布维护手册

这份文档记录 AtomUI Gallery Desktop 在 Windows 11 上发布 `win-x64` Native AOT 时的工具链要求、推荐命令、验证方法和本次实际遇到的问题。它是给维护者排障用的，不替代 [AtomUI AOT 编程规范](../development/aot-programming-guidelines.md)。

历史基线日期：2026-06-11。当前分支复验日期：2026-11-06。

2026-11-05 使用正式 `PublishToLocal.ps1 -runtime win-x64 -buildType Release -publishAot true` 完成当前
`feature/custom-theme` 发布、恢复资产与原生产物检查，并实际打开 Gallery、主题菜单和工作台。
主程序为 100,100,608 字节，SHA256 为 `39b49e90bfaae8b498fb3ed91c29e2e6a35e3b9544be5bfa54902691f0346573`；
LargeType/深色实际应用、Ant/明色恢复及隔离预览均经过原生交互核对，输入和选择保留。保留固定 ILC10.0.8 的
NU1903，以及 ReactiveUI/ReactiveUI.Avalonia 的 IL2104、IL3053 警告，不能称为零警告发布。这是修补前的历史结果。
2026-11-06 已将Windows原生编译器/runtime修补至10.0.14（Native8为8.0.33），并迁移Reactive依赖与显式路由解析；
正式脚本发布已消除上述诊断。实际NativeAOT Gallery路由宿主在.NET10.0.14上通过78路由/316检查，
涉及465种实际控件类型；原生进程无CoreCLR。依赖及源码影响见
[迁移说明](../../modules/toolkits-gallery-base/reactive-dependency-migration.md)与[修补评估](../../superpowers/audits/2026-11-06-windows-servicing/assessment.md)。
当前完整范围、清理与模块门禁见 [Windows 验收记录](../../superpowers/progress/2026-11-05-windows-theme-acceptance.md)。
下文旧产物体积与“本次”排障过程属于 2026-06-11 基线，不替代当前结果。

## 结论先行

Windows 11 上 `controlgallery/AtomUIGallery.Desktop` 的 Native AOT 发布需要三类条件同时满足：

- .NET SDK 版本要满足仓库 `global.json`，当前为 `10.0.300`。
- Windows 机器必须安装 Visual Studio 2022 或更新版本的 Desktop development with C++ workload。命令行安装时 workload ID 是 `Microsoft.VisualStudio.Workload.VCTools`。
- 如果把 restore 和 publish 分开，restore 必须显式带上 `Configuration=Release` 和 `PublishAot=true`，否则 `project.assets.json` 不会恢复 `Microsoft.DotNet.ILCompiler`，后续 `publish --no-restore` 可能退化为普通 self-contained 发布。

真正进入 Native AOT 时，publish 日志中会出现：

```text
Generating native code
```

最终发布目录不应该包含：

```text
coreclr.dll
System.Private.CoreLib.dll
```

本次成功产物位于：

```text
artifacts/publish/AtomUIGallery.Desktop-win-x64-nativeaot/
```

本次验证到的关键产物：

| 项 | 结果 |
| --- | --- |
| 主程序 | `AtomUIGallery.Desktop.exe` |
| RID | `win-x64` |
| 主程序大小 | 约 `59.34 MB` |
| 不含 PDB 的可分发文件 | 4 个，约 `77.31 MB` |
| PDB 调试符号 | 约 `296.07 MB` |
| 完整发布目录 | 17 个文件，约 `373.38 MB` |
| `coreclr.dll` | 不存在 |
| `System.Private.CoreLib.dll` | 不存在 |
| 冒烟测试 | 可启动，10 秒内未崩溃 |

PDB 文件建议单独归档用于崩溃分析，不作为默认分发内容。

## 项目配置

`controlgallery/AtomUIGallery.Desktop/AtomUIGallery.Desktop.csproj` 的 Release 配置应保持 AOT 兼容性声明；发布属性由发布命令显式传入：

```xml
<PropertyGroup Condition="'$(Configuration)' == 'Release'">
    <IsAotCompatible>true</IsAotCompatible>
    <IsTrimmable>true</IsTrimmable>
</PropertyGroup>
```

NativeAOT 发布时使用 `-p:GalleryPublishTrimmed=true -p:GalleryPublishAot=true`。这样普通 Release build 不会因为项目默认属性而启动发布 analyzer 或自包含 runtime 输出。
发布命令必须显式传入这两个 Gallery 属性；不要把全局 `PublishAot=true` 传给整个解决方案。

## 推荐发布命令

一般情况下，直接运行：

```powershell
dotnet publish .\controlgallery\AtomUIGallery.Desktop\AtomUIGallery.Desktop.csproj `
  -c Release `
  -r win-x64 `
  -p:GalleryPublishTrimmed=true `
  -p:GalleryPublishAot=true `
  -v:minimal
```

如果要把产物输出到独立目录：

```powershell
dotnet publish .\controlgallery\AtomUIGallery.Desktop\AtomUIGallery.Desktop.csproj `
  -c Release `
  -r win-x64 `
  -p:GalleryPublishTrimmed=true `
  -p:GalleryPublishAot=true `
  -o .\artifacts\publish\AtomUIGallery.Desktop-win-x64-nativeaot `
  -v:minimal
```

如果遇到 NuGet 锁、网络重试或想明确区分 restore 和 AOT 编译耗时，使用两段式：

```powershell
dotnet restore .\controlgallery\AtomUIGallery.Desktop\AtomUIGallery.Desktop.csproj `
  -r win-x64 `
  -p:Configuration=Release `
  -p:GalleryPublishTrimmed=true `
  -p:GalleryPublishAot=true `
  --disable-parallel `
  -m:1 `
  /nr:false `
  -v:minimal

dotnet publish .\controlgallery\AtomUIGallery.Desktop\AtomUIGallery.Desktop.csproj `
  -c Release `
  -r win-x64 `
  --no-restore `
  -p:GalleryPublishTrimmed=true `
  -p:GalleryPublishAot=true `
  -o .\artifacts\publish\AtomUIGallery.Desktop-win-x64-nativeaot `
  -m:1 `
  /nr:false `
  -v:minimal
```

重点是第一条 restore 命令里的：

```powershell
-p:Configuration=Release -p:PublishAot=true
```

缺少它们时，`project.assets.json` 可能没有 `Microsoft.DotNet.ILCompiler` 和 `runtime.win-x64.Microsoft.DotNet.ILCompiler`。后续即使 `publish --no-restore` 成功，也可能只是普通 self-contained 发布。

## 本地工具链准备

### .NET SDK

仓库当前 `global.json` 要求：

```json
{
  "sdk": {
    "version": "10.0.300",
    "rollForward": "latestFeature"
  }
}
```

检查当前 SDK：

```powershell
dotnet --info
```

本次 Windows 11 验证使用的 SDK 安装在用户目录：

```text
C:\Users\CHINBOY\.dotnet-sdk\10.0.300-x64\dotnet.exe
```

如果开发机默认 `dotnet` 不是 `10.0.300`，可以临时使用完整路径发布，或者安装到用户目录：

```powershell
$installDir = Join-Path $env:USERPROFILE ".dotnet-sdk\10.0.300-x64"
$script = Join-Path $env:TEMP "dotnet-install.ps1"
Invoke-WebRequest "https://dot.net/v1/dotnet-install.ps1" -OutFile $script
& $script -Version 10.0.300 -InstallDir $installDir -Architecture x64
& (Join-Path $installDir "dotnet.exe") --info
```

如果首次运行 SDK 时出现 workload manifest 校验异常，可以更新该 SDK 的 workload manifests：

```powershell
dotnet workload update
```

本次曾遇到：

```text
验证工作负载时遇到问题。有关详细信息，请运行 "dotnet workload update"。
```

更新后 restore 才能继续暴露真实 NuGet 或项目错误。

### Visual Studio Build Tools

Windows Native AOT 需要平台 linker。只装 .NET SDK 不够；缺少 C++ 工具链时会报：

```text
Platform linker not found. Ensure you have all the required prerequisites documented at https://aka.ms/nativeaot-prerequisites,
in particular the Desktop Development for C++ workload in Visual Studio.
```

本次机器一开始没有：

```text
cl.exe
link.exe
vswhere.exe
```

安装 Visual Studio Build Tools 2026 后验证通过：

| 项 | 本次验证版本 |
| --- | --- |
| Visual Studio Build Tools | `18.7.0` |
| MSVC | `14.51.36231` |
| Windows SDK | `10.0.26100.0` |
| `link.exe` | `C:\Program Files (x86)\Microsoft Visual Studio\18\BuildTools\VC\Tools\MSVC\14.51.36231\bin\Hostx64\x64\link.exe` |

命令行安装方式：

```powershell
$installer = ".\artifacts\codex\vs_buildtools.exe"
Invoke-WebRequest "https://aka.ms/vs/stable/vs_buildtools.exe" -OutFile $installer
& $installer --quiet --wait --norestart --nocache `
  --add Microsoft.VisualStudio.Workload.VCTools `
  --includeRecommended
```

注意：

- 安装器可能很快返回，但 `vs_buildtools.exe` 进程仍在后台安装。不要重复启动第二个安装器，先检查或等待该进程结束。
- 安装完成后用 `vswhere` 检查实例。
- 本次安装后 `isRebootRequired=false`，不需要重启。

验证命令：

```powershell
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
& $vswhere -all -products * -format json

Get-ChildItem "C:\Program Files (x86)\Microsoft Visual Studio" -Filter link.exe -Recurse
Get-ChildItem "C:\Program Files (x86)\Microsoft Visual Studio" -Filter cl.exe -Recurse
Get-ChildItem "C:\Program Files (x86)\Windows Kits\10\Lib" -Directory
```

## Native AOT 产物验证

发布成功不一定等于 Native AOT 成功。需要同时验证日志和文件结构。

### 日志验证

真正 Native AOT 会出现：

```text
Generating native code
```

如果日志只有普通项目输出，且没有这行，优先检查 restore assets 是否包含 ILCompiler。

检查方式：

```powershell
rg -n "Microsoft.DotNet.ILCompiler|runtime.win-x64.Microsoft.DotNet.ILCompiler" `
  .\.artifacts\AtomUIGallery.Desktop\obj\project.assets.json
```

### 文件验证

检查发布目录：

```powershell
$dir = ".\artifacts\publish\AtomUIGallery.Desktop-win-x64-nativeaot"
$files = Get-ChildItem $dir -File

[PSCustomObject]@{
  FileCount = $files.Count
  TotalMB = [math]::Round((($files | Measure-Object Length -Sum).Sum / 1MB), 2)
  HasCoreClr = Test-Path (Join-Path $dir "coreclr.dll")
  HasPrivateCoreLib = Test-Path (Join-Path $dir "System.Private.CoreLib.dll")
}

Get-Item (Join-Path $dir "AtomUIGallery.Desktop.exe") |
  Select-Object FullName, Length, LastWriteTime,
    @{Name="FileVersion"; Expression={$_.VersionInfo.FileVersion}},
    @{Name="ProductVersion"; Expression={$_.VersionInfo.ProductVersion}}
```

本次 Native AOT 正确结果：

```text
HasCoreClr        False
HasPrivateCoreLib False
```

如果目录里出现下面文件，说明大概率不是 Native AOT 产物，而是普通 self-contained 产物：

```text
coreclr.dll
System.Private.CoreLib.dll
```

### 分发体积统计

```powershell
$dir = ".\artifacts\publish\AtomUIGallery.Desktop-win-x64-nativeaot"
$files = Get-ChildItem $dir -File
$distribution = $files | Where-Object Extension -ne ".pdb"

[PSCustomObject]@{
  DistributionFileCount = $distribution.Count
  DistributionMB = [math]::Round((($distribution | Measure-Object Length -Sum).Sum / 1MB), 2)
  SymbolsMB = [math]::Round(((($files | Where-Object Extension -eq ".pdb") | Measure-Object Length -Sum).Sum / 1MB), 2)
}
```

本次结果：

```text
DistributionFileCount 4
DistributionMB        77.31
SymbolsMB             296.07
```

### 启动冒烟测试

```powershell
$exe = ".\artifacts\publish\AtomUIGallery.Desktop-win-x64-nativeaot\AtomUIGallery.Desktop.exe"
$process = Start-Process -FilePath $exe -WorkingDirectory (Split-Path -Parent $exe) -PassThru
Start-Sleep -Seconds 10
$process.Refresh()

if ($process.HasExited) {
  throw "Native AOT Gallery exited early with code $($process.ExitCode)."
}

Stop-Process -Id $process.Id -Force
```

本次冒烟测试结果：

- 进程成功启动。
- 主窗口标题为 `AtomUI 桌面控件库`。
- 10 秒内未崩溃退出。
- 测试进程已关闭。

如果需要人工检查 UI，直接运行：

```powershell
.\artifacts\publish\AtomUIGallery.Desktop-win-x64-nativeaot\AtomUIGallery.Desktop.exe
```

## 常见问题和处理

### `Platform linker not found`

症状：

```text
Microsoft.NETCore.Native.Windows.targets(...): error : Platform linker not found.
```

原因：

- 机器上没有 Visual Studio C++ 工具链。
- `link.exe` 不在 Native AOT 可发现的位置。

处理：

- 安装 Visual Studio 2022 或更新版本的 Desktop development with C++ workload。
- Build Tools 命令行 workload ID：`Microsoft.VisualStudio.Workload.VCTools`。
- 安装后确认 `link.exe` 和 Windows SDK 都存在。

### 发布成功但不是 Native AOT

症状：

- publish 成功。
- 没有 `Generating native code`。
- 发布目录包含 `coreclr.dll` 或 `System.Private.CoreLib.dll`。
- 主 `AtomUIGallery.Desktop.exe` 很小，本次普通 self-contained 误判时只有约 `0.30 MB`。

原因：

- restore 阶段没有带 Release/AOT 属性。
- `project.assets.json` 中没有 ILCompiler 包。
- 后续 `publish --no-restore` 复用了错误 assets。

处理：

```powershell
dotnet restore .\controlgallery\AtomUIGallery.Desktop\AtomUIGallery.Desktop.csproj `
  -r win-x64 `
  -p:Configuration=Release `
  -p:PublishAot=true `
  --disable-parallel `
  -m:1 `
  /nr:false `
  -v:minimal
```

确认 assets 中有：

```text
Microsoft.DotNet.ILCompiler
runtime.win-x64.Microsoft.DotNet.ILCompiler
```

然后再执行：

```powershell
dotnet publish .\controlgallery\AtomUIGallery.Desktop\AtomUIGallery.Desktop.csproj `
  -c Release `
  -r win-x64 `
  --no-restore `
  -v:minimal
```

### `NU1301` 且连接到 `127.0.0.1:9`

症状：

```text
error NU1301: 无法加载源 https://api.nuget.org/v3/index.json 的服务索引。
由于目标计算机积极拒绝，无法连接。 (127.0.0.1:9)
```

原因：

- 当前执行环境拦截或禁用了网络访问。
- 在 Codex/受限沙箱内 restore 会表现为连接本地拒绝。

处理：

- 在正常开发终端执行 restore/publish。
- 或使用经过批准的网络执行环境。
- 不要先怀疑包版本或项目引用，先确认网络权限。

### NuGetScratch lock

症状：

```text
无法获取对 "C:\Users\<user>\AppData\Local\Temp\NuGetScratch\lock\..." 的锁定文件访问权限
```

原因：

- 之前的 NuGet/MSBuild/dotnet 进程异常退出，留下不可访问锁文件。
- 多个 restore 并行争同一个 scratch lock。

处理优先级：

1. 检查并停止残留的 `dotnet` / `MSBuild` 进程。
2. 给本次 restore 指定独立 `TEMP` / `TMP`。
3. restore 使用 `--disable-parallel -m:1 /nr:false`。
4. 确认没有相关进程持有锁后，再考虑清理旧 scratch lock。

推荐模板：

```powershell
$repo = (Resolve-Path ".").Path
$env:TEMP = Join-Path $repo "artifacts\codex\aot-restore-tmp"
$env:TMP = $env:TEMP
New-Item -ItemType Directory -Force $env:TEMP | Out-Null

dotnet restore .\controlgallery\AtomUIGallery.Desktop\AtomUIGallery.Desktop.csproj `
  -r win-x64 `
  -p:Configuration=Release `
  -p:PublishAot=true `
  --disable-parallel `
  -m:1 `
  /nr:false `
  -v:minimal
```

### `Access to the path is denied`

可能出现的位置：

```text
AtomUIGallery.Desktop.GlobalUsings.g.cs
AtomUIGallery.Desktop.csproj.FileListAbsolute.txt
.artifacts\<Project>\obj\*.tmp
```

常见原因：

- 受限沙箱没有写入 `.artifacts/obj` 的权限。
- 另一个构建进程或 IDE 持有文件。
- 并行 restore/publish 在同一个中间目录写临时文件。

处理：

- 关闭正在运行的 Gallery。
- 检查残留 `dotnet` / `MSBuild` 进程。
- 在正常终端或批准环境执行。
- 使用 `-m:1 /nr:false` 降低 MSBuild 节点复用和文件竞争。

### Visual Studio 安装器返回很快

症状：

- 安装命令很快退出。
- `vswhere` 仍找不到实例。
- 后台还能看到 `vs_buildtools.exe`。

原因：

- bootstrapper 外层返回了，但真正安装还在后台进程里进行。

处理：

```powershell
Get-Process vs_buildtools -ErrorAction SilentlyContinue
Wait-Process -Name vs_buildtools
```

如果没有权限等待进程，就手动观察任务管理器或重新执行 `vswhere` 直到实例出现。

### ReactiveUI trim/AOT warning

本次 Native AOT 编译成功，但日志中仍有第三方包内部汇总 warning：

```text
ReactiveUI.Avalonia.dll : warning IL2104
ReactiveUI.Avalonia.dll : warning IL3053
ReactiveUI.dll : warning IL2104
ReactiveUI.dll : warning IL3053
```

这些 warning 来自 `ReactiveUI` / `ReactiveUI.Avalonia` 包内部。当前处理原则：

- 不把它们当成 AtomUI 自身源码 warning。
- 发布可以继续。
- 每次升级 ReactiveUI 或改变 Gallery activation/binding 路径后，都要重新做真实 Native AOT publish 和启动 smoke test。

### GeneratedFiles 出现在 `git status`

构建产出的 `GeneratedFiles/**/*.g.cs` 默认已加入 `.gitignore`，不会作为工作区源码变更；只有 GalleryBase 中被结构测试读取的少量快照保留跟踪。

如果本地仍看到已跟踪快照显示 modified，先确认 `git diff` 是否为空，再刷新 Git 索引：

```bash
git update-index --refresh
```

处理：

- 先看 `git diff --stat` 和具体 `git diff`。
- 没有内容差异时，不要把这些 generated file 状态当成真实源码修改。
- 如果 generator 本身有改动，则必须同时检查生成物内容是否稳定复现。

## 维护检查清单

发布前：

- [ ] `dotnet --info` 使用仓库要求的 SDK。
- [ ] `link.exe`、`cl.exe`、Windows SDK 可发现。
- [ ] `controlgallery/AtomUIGallery.Desktop.csproj` Release 配置包含 `PublishAot=true`。
- [ ] 如果分步执行，restore 带 `-p:Configuration=Release -p:PublishAot=true`。
- [ ] `project.assets.json` 包含 `Microsoft.DotNet.ILCompiler` 和 `runtime.win-x64.Microsoft.DotNet.ILCompiler`。

发布中：

- [ ] 日志出现 `Generating native code`。
- [ ] 没有 `Platform linker not found`。
- [ ] 没有 `NU1301` 网络错误。
- [ ] 没有 NuGetScratch lock 或 `.artifacts/obj` 写入权限错误。

发布后：

- [ ] 发布目录不包含 `coreclr.dll`。
- [ ] 发布目录不包含 `System.Private.CoreLib.dll`。
- [ ] `AtomUIGallery.Desktop.exe` 大小符合预期，不是几百 KB 的 apphost。
- [ ] 可执行文件能启动并保持运行。
- [ ] PDB 和可分发文件分开统计。
- [ ] `git status` 中只有预期配置或文档变化。

## 参考链接

- [.NET Native AOT deployment overview](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/)
- [Visual Studio Build Tools workload and component IDs](https://learn.microsoft.com/en-us/visualstudio/install/workload-component-id-vs-build-tools?view=visualstudio)
- [AtomUI AOT 编程规范](../development/aot-programming-guidelines.md)
