# Linux Gallery Native AOT 发布维护手册

这份文档记录 AtomUI Gallery Desktop 在 Linux 上发布 `linux-x64` Native AOT 时的工具链要求、推荐命令、验证方法和实际遇到的问题。它是给维护者排障用的，不替代 [AtomUI AOT 编程规范](../development/aot-programming-guidelines.md)。Windows 平台的对应手册见 [Windows NativeAOT 发布](windows-native-aot-publish.md)。

最后验证日期：2026-06-11。本次验证环境：Ubuntu 24.04.4 LTS，x86_64。

## 结论先行

Linux 上 `controlgallery/AtomUIGallery.Desktop` 的 Native AOT 发布需要三类条件同时满足：

- .NET SDK 版本要满足仓库 `global.json`：10.0.300 或更高版本（含 SDK 11）。
- 机器必须安装本地原生工具链：`clang`（或 `gcc`）、对应的 linker，以及 `zlib`。Native AOT 在 Linux 上用 `clang` 调用平台 linker，缺少时会直接报错。
- 如果把 restore 和 publish 分开，restore 必须显式带上 `Configuration=Release` 和 `PublishAot=true`，否则 `project.assets.json` 不会恢复 `Microsoft.DotNet.ILCompiler`，后续 `publish --no-restore` 可能退化为普通 self-contained 发布。

真正进入 Native AOT 时，publish 日志中会出现：

```text
Generating native code
```

最终发布目录不应该包含：

```text
libcoreclr.so
System.Private.CoreLib.dll
AtomUIGallery.Desktop.dll
```

注意和 Windows 的区别：Linux Native AOT 产物里**不应该有托管入口 dll**（`AtomUIGallery.Desktop.dll`）。如果它存在，几乎可以确定是普通 self-contained 发布。

本次成功产物位于：

```text
artifacts/publish/AtomUIGallery.Desktop-linux-x64-nativeaot/
```

本次验证到的关键产物：

| 项 | 结果 |
| --- | --- |
| 主程序 | `AtomUIGallery.Desktop`（无扩展名 ELF 可执行文件） |
| RID | `linux-x64` |
| 主程序类型 | `ELF 64-bit LSB pie executable, x86-64, dynamically linked, stripped` |
| 主程序大小 | 约 `57.74 MB`（60,544,280 字节） |
| 不含符号的可分发文件 | 3 个，约 `72 MB` |
| 调试符号（`.pdb` + `.dbg`） | 约 `113 MB` |
| 完整发布目录 | 14 个文件，约 `184 MB` |
| `libcoreclr.so` | 不存在 |
| `System.Private.CoreLib.dll` | 不存在 |
| 托管 `AtomUIGallery.Desktop.dll` | 不存在 |
| 冒烟测试 | 可启动，10 秒内未崩溃 |

`.dbg`（native 调试符号）和 `.pdb` 建议单独归档用于崩溃分析，不作为默认分发内容。可分发的三个文件是：

```text
AtomUIGallery.Desktop
libHarfBuzzSharp.so
libSkiaSharp.so
```

`libSkiaSharp.so` 和 `libHarfBuzzSharp.so` 是 Avalonia 渲染所需的原生依赖，必须和主程序一起分发。

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

这份配置是跨平台共享的，和 Windows 手册一致。Linux 和 Windows 的差异只在工具链和产物形态，不在项目配置。

## 推荐发布命令

一般情况下，直接运行：

```bash
dotnet publish controlgallery/AtomUIGallery.Desktop/AtomUIGallery.Desktop.csproj \
  -c Release \
  -r linux-x64 \
  -p:GalleryPublishTrimmed=true \
  -p:GalleryPublishAot=true \
  -v:minimal
```

如果要把产物输出到独立目录：

```bash
dotnet publish controlgallery/AtomUIGallery.Desktop/AtomUIGallery.Desktop.csproj \
  -c Release \
  -r linux-x64 \
  -p:GalleryPublishTrimmed=true \
  -p:GalleryPublishAot=true \
  -o artifacts/publish/AtomUIGallery.Desktop-linux-x64-nativeaot \
  -v:minimal
```

如果要发布到 ARM64（例如树莓派或 ARM 服务器），把 RID 换成 `linux-arm64`。注意 Native AOT 默认不做交叉编译，`linux-arm64` 产物要在 ARM64 机器上发布，或者额外配置交叉编译工具链。

如果遇到 NuGet 锁、网络重试或想明确区分 restore 和 AOT 编译耗时，使用两段式：

```bash
dotnet restore controlgallery/AtomUIGallery.Desktop/AtomUIGallery.Desktop.csproj \
  -r linux-x64 \
  -p:Configuration=Release \
  -p:GalleryPublishTrimmed=true \
  -p:GalleryPublishAot=true \
  -v:minimal

dotnet publish controlgallery/AtomUIGallery.Desktop/AtomUIGallery.Desktop.csproj \
  -c Release \
  -r linux-x64 \
  --no-restore \
  -p:GalleryPublishTrimmed=true \
  -p:GalleryPublishAot=true \
  -o artifacts/publish/AtomUIGallery.Desktop-linux-x64-nativeaot \
  -v:minimal
```

重点是第一条 restore 命令里的：

```bash
-p:Configuration=Release -p:PublishAot=true
```

缺少它们时，`project.assets.json` 可能没有 `Microsoft.DotNet.ILCompiler` 和 `runtime.linux-x64.Microsoft.DotNet.ILCompiler`。后续即使 `publish --no-restore` 成功，也可能只是普通 self-contained 发布。

Native AOT 链接阶段（`Generating native code` 之后）耗时较长，本次在普通开发机上整体发布需要数分钟。建议在后台或带足够超时的环境里执行，不要因为命令长时间没有新输出就误判为卡死。

## 本地工具链准备

### .NET SDK

仓库当前 `global.json` 要求：

```json
{
  "sdk": {
    "version": "10.0.300",
    "rollForward": "latestMajor",
    "allowPrerelease": false
  }
}
```

`latestMajor` 选用本机已安装的最高正式版 SDK（含 SDK 11），最低为 10.0.300；预览版 SDK 不参与选择。

检查当前 SDK：

```bash
dotnet --info
```

如果开发机默认 `dotnet` 版本不满足，可以安装到用户目录后用完整路径发布：

```bash
installDir="$HOME/.dotnet"
curl -sSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
chmod +x /tmp/dotnet-install.sh
/tmp/dotnet-install.sh --version 10.0.300 --install-dir "$installDir" --architecture x64
"$installDir/dotnet" --info
```

### 原生工具链（clang / linker / zlib）

Linux Native AOT 需要本地 C 工具链来调用平台 linker。只装 .NET SDK 不够。本次验证环境已具备：

| 项 | 本次验证版本 |
| --- | --- |
| clang | `18.1.3`（Ubuntu） |
| gcc | `13.3.0`（Ubuntu） |
| zlib | `libz.so` 存在于 `/lib/x86_64-linux-gnu/` |

Debian / Ubuntu 安装：

```bash
sudo apt-get update
sudo apt-get install -y clang zlib1g-dev
```

Fedora / RHEL 系：

```bash
sudo dnf install -y clang zlib-devel
```

Alpine：

```bash
apk add clang build-base zlib-dev
```

缺少 `clang` 或 linker 时，publish 会在 `Generating native code` 之后报链接相关错误（详见“常见问题”）。

### 运行期图形依赖

发布产物是图形应用，运行（包括冒烟测试）需要：

- X11 或 Wayland 显示环境（本次验证使用 `DISPLAY=:0`）。
- `libX11`、`libgtk-3` 等系统库。无头服务器上可以用 `xvfb-run` 提供虚拟显示。

桌面环境一般默认具备这些库。CI 或服务器上做冒烟测试时，安装：

```bash
sudo apt-get install -y libx11-6 libgtk-3-0 xvfb
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

```bash
rg -n "Microsoft.DotNet.ILCompiler|runtime.linux-x64.Microsoft.DotNet.ILCompiler" \
  .artifacts/AtomUIGallery.Desktop/obj/project.assets.json
```

### 文件验证

检查发布目录。Linux Native AOT 的关键判据是：主程序是裸 ELF 可执行文件，且没有托管入口 dll。

```bash
dir="artifacts/publish/AtomUIGallery.Desktop-linux-x64-nativeaot"

echo "=== 不应存在的文件 ==="
for f in libcoreclr.so System.Private.CoreLib.dll AtomUIGallery.Desktop.dll; do
  if [ -e "$dir/$f" ]; then echo "PRESENT (BAD): $f"; else echo "absent (good): $f"; fi
done

echo "=== 主程序类型 ==="
file "$dir/AtomUIGallery.Desktop"
```

本次 Native AOT 正确结果：

```text
absent (good): libcoreclr.so
absent (good): System.Private.CoreLib.dll
absent (good): AtomUIGallery.Desktop.dll
AtomUIGallery.Desktop: ELF 64-bit LSB pie executable, x86-64, ..., stripped
```

如果目录里出现下面任何一个文件，说明大概率不是 Native AOT 产物，而是普通 self-contained 产物：

```text
libcoreclr.so
System.Private.CoreLib.dll
AtomUIGallery.Desktop.dll
```

其中托管入口 `AtomUIGallery.Desktop.dll` 是最直观的判据：真正的 Native AOT 把入口编译进了原生可执行文件，不会再有同名托管 dll。

### 分发体积统计

```bash
dir="artifacts/publish/AtomUIGallery.Desktop-linux-x64-nativeaot"

echo "可分发（排除符号）MB:"
du -cm --exclude='*.pdb' --exclude='*.dbg' "$dir"/* | tail -1

echo "调试符号（.pdb + .dbg）MB:"
du -cm "$dir"/*.pdb "$dir"/*.dbg 2>/dev/null | tail -1

echo "完整目录 MB:"
du -sm "$dir"
```

本次结果：

```text
可分发（排除符号）  约 72 MB（3 个文件）
调试符号            约 113 MB
完整目录            约 184 MB（14 个文件）
```

可分发的三个文件：`AtomUIGallery.Desktop`、`libSkiaSharp.so`、`libHarfBuzzSharp.so`。分发时务必带上两个 `.so`，否则程序启动会因找不到原生渲染库而失败。

### 启动冒烟测试

有显示环境时（本机 `DISPLAY=:0`）：

```bash
dir="artifacts/publish/AtomUIGallery.Desktop-linux-x64-nativeaot"
DISPLAY=:0 "$dir/AtomUIGallery.Desktop" > /tmp/gallery-smoke.log 2>&1 &
pid=$!
sleep 10
if kill -0 "$pid" 2>/dev/null; then
  echo "STILL RUNNING after 10s (good)"
  kill -TERM "$pid"; sleep 1; kill -KILL "$pid" 2>/dev/null
else
  wait "$pid"; echo "EXITED EARLY with code $?"
  tail -20 /tmp/gallery-smoke.log
fi
```

无头服务器上用虚拟显示：

```bash
xvfb-run -a "$dir/AtomUIGallery.Desktop" &
```

本次冒烟测试结果：

- 进程成功启动。
- 主窗口标题为 `AtomUI 桌面控件库`。
- 10 秒内未崩溃退出。
- 测试进程已关闭。

如果需要人工检查 UI，直接运行：

```bash
artifacts/publish/AtomUIGallery.Desktop-linux-x64-nativeaot/AtomUIGallery.Desktop
```

## 常见问题和处理

### Native AOT 链接失败：找不到 clang 或 linker

症状（`Generating native code` 之后）：

```text
error : Platform linker ('clang'/'gcc') not found in PATH. Ensure you have all the
required prerequisites documented at https://aka.ms/nativeaot-prerequisites
```

或链接阶段报缺少 `zlib` 等系统库。

原因：

- 机器上没有 `clang` / `gcc` 或对应 linker。
- 缺少 `zlib` 开发包。

处理：

```bash
# Debian / Ubuntu
sudo apt-get install -y clang zlib1g-dev
# Fedora / RHEL
sudo dnf install -y clang zlib-devel
```

安装后确认：

```bash
clang --version
```

### 发布成功但不是 Native AOT

症状：

- publish 成功。
- 没有 `Generating native code`。
- 发布目录包含 `libcoreclr.so`、`System.Private.CoreLib.dll`，或托管 `AtomUIGallery.Desktop.dll`。
- 主 `AtomUIGallery.Desktop` 很小，只是个 apphost 启动器，而不是几十 MB 的原生可执行文件。

原因：

- restore 阶段没有带 Release/AOT 属性。
- `project.assets.json` 中没有 ILCompiler 包。
- 后续 `publish --no-restore` 复用了错误 assets。

处理：

```bash
dotnet restore controlgallery/AtomUIGallery.Desktop/AtomUIGallery.Desktop.csproj \
  -r linux-x64 \
  -p:Configuration=Release \
  -p:PublishAot=true \
  -v:minimal
```

确认 assets 中有：

```text
Microsoft.DotNet.ILCompiler
runtime.linux-x64.Microsoft.DotNet.ILCompiler
```

然后再执行：

```bash
dotnet publish controlgallery/AtomUIGallery.Desktop/AtomUIGallery.Desktop.csproj \
  -c Release \
  -r linux-x64 \
  --no-restore \
  -v:minimal
```

### `NU1301` 网络错误

症状：

```text
error NU1301: 无法加载源 https://api.nuget.org/v3/index.json 的服务索引。
```

原因：

- 当前执行环境拦截或禁用了网络访问。
- 在受限沙箱内 restore 会表现为连接被拒绝。

处理：

- 在正常开发终端执行 restore/publish。
- 或使用经过批准的网络执行环境。
- 不要先怀疑包版本或项目引用，先确认网络权限。

### 运行时报找不到 `libSkiaSharp.so`

症状：

```text
Unable to load shared library 'libSkiaSharp' or one of its dependencies.
```

原因：

- 只拷贝了主可执行文件，漏掉了同目录的 `.so`。

处理：

- 分发时把 `libSkiaSharp.so` 和 `libHarfBuzzSharp.so` 和主程序放在同一目录。
- 不要只复制 `AtomUIGallery.Desktop` 单个文件。

### 冒烟测试在无头环境立即退出

症状：

- 在服务器或 CI 上启动后立即退出。
- 日志里有 X11 / display 相关错误。

原因：

- 没有可用的显示环境。

处理：

```bash
sudo apt-get install -y xvfb
xvfb-run -a artifacts/publish/AtomUIGallery.Desktop-linux-x64-nativeaot/AtomUIGallery.Desktop
```

### ReactiveUI trim/AOT warning

本次 Native AOT 编译成功，但日志中仍有第三方包内部汇总 warning：

```text
ReactiveUI.Avalonia.dll : warning IL2104
ReactiveUI.Avalonia.dll : warning IL3053
ReactiveUI.dll : warning IL2104
ReactiveUI.dll : warning IL3053
```

这些 warning 来自 `ReactiveUI` / `ReactiveUI.Avalonia` 包内部（本次为 `ReactiveUI 23.2.28`、`ReactiveUI.Avalonia 12.0.3`）。当前处理原则：

- 不把它们当成 AtomUI 自身源码 warning。
- 发布可以继续。
- 每次升级 ReactiveUI 或改变 Gallery activation/binding 路径后，都要重新做真实 Native AOT publish 和启动 smoke test。

这一点和 Windows 手册记录的现象一致。

### 调试符号文件 `.dbg`

Linux Native AOT 会额外生成 `AtomUIGallery.Desktop.dbg`（本次约 110 MB），这是分离出来的 native 调试符号。处理原则：

- 不作为默认分发内容。
- 单独归档用于崩溃分析（配合 `gdb` / `addr2line`）。
- 统计可分发体积时排除 `.dbg` 和 `.pdb`。

## 维护检查清单

发布前：

- [ ] `dotnet --info` 使用仓库要求的 SDK。
- [ ] `clang`（或 `gcc`）、linker、`zlib` 可用。
- [ ] `controlgallery/AtomUIGallery.Desktop.csproj` Release 配置包含 `PublishAot=true`。
- [ ] 如果分步执行，restore 带 `-p:Configuration=Release -p:PublishAot=true`。
- [ ] `project.assets.json` 包含 `Microsoft.DotNet.ILCompiler` 和 `runtime.linux-x64.Microsoft.DotNet.ILCompiler`。

发布中：

- [ ] 日志出现 `Generating native code`。
- [ ] 没有 linker / clang not found 错误。
- [ ] 没有 `NU1301` 网络错误。

发布后：

- [ ] 发布目录不包含 `libcoreclr.so`。
- [ ] 发布目录不包含 `System.Private.CoreLib.dll`。
- [ ] 发布目录不包含托管 `AtomUIGallery.Desktop.dll`。
- [ ] `AtomUIGallery.Desktop` 是裸 ELF 可执行文件（`file` 验证），大小符合预期，不是几百 KB 的 apphost。
- [ ] `libSkiaSharp.so` 和 `libHarfBuzzSharp.so` 随主程序一起存在。
- [ ] 可执行文件能启动并保持运行（有显示环境或 `xvfb-run`）。
- [ ] `.dbg` / `.pdb` 符号和可分发文件分开统计。
- [ ] `git status` 中只有预期配置或文档变化。

## 参考链接

- [.NET Native AOT deployment overview](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/)
- [Native AOT prerequisites](https://aka.ms/nativeaot-prerequisites)
- [Windows Gallery Native AOT 发布维护手册](windows-native-aot-publish.md)
- [AtomUI AOT 编程规范](../development/aot-programming-guidelines.md)
