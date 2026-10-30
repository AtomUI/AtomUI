# Gallery NativeAOT Release Workflow

这份文档记录 Gallery 发布 workflow 的 NativeAOT 开关、发布脚本职责和验证规则。平台 NativeAOT 工具链细节仍以 [Windows NativeAOT 发布](../platforms/windows-native-aot-publish.md)、[Linux NativeAOT 发布](../platforms/linux-native-aot-publish.md) 和 [AOT 编程规范](../development/aot-programming-guidelines.md) 为准。

## 目标

Gallery 发布管理员手动运行 `.github/workflows/release-gallery.yml` 时，可以选择是否使用 NativeAOT 发布。正式发布默认使用 NativeAOT；取消勾选只用于临时排障或兼容性验证。

## Workflow 输入

`release-gallery.yml` 的 `workflow_dispatch.inputs` 包含：

```yaml
PublishAot:
   description: 'Publish Gallery with NativeAOT'
   type: boolean
   default: true
```

所有 Gallery 平台发布任务都把该输入传给 `controlgallery/AtomUIGallery.Desktop/scripts/PublishToLocal.ps1`：

```powershell
-publishAot '${{ inputs.PublishAot }}'
```

## 安装包打包脚本

安装包由仓库内 `controlgallery/AtomUIGallery.Desktop/scripts/packaging/` 的 PowerShell 脚本生成，不依赖外部私有工具，
也不需要跨仓库 checkout 的 token：

| 脚本 | 产物 | 平台 |
| --- | --- | --- |
| `New-GalleryZip.ps1` | `{Name}-{Version}.{BuildStamp}-{rid}.zip`，仅含 `*.exe` 与 `*.dll` | Windows |
| `New-GalleryAppImage.ps1` | `{Name}-{rid}-{Version}.{BuildStamp}.AppImage` | Linux |
| `New-GalleryDmg.ps1` | `{Name}-{Version}.{BuildStamp}-{rid}.dmg`，可选 `-CertIdentity` 签名 | macOS |

三个脚本共用 `GalleryPackaging.Common.ps1`，参数为 `-ConfigPath`（`PublishToLocal.ps1` 输出的 `config/InstallerConfig.xml`，
仓库模板中的 `__VERSION__` 会被拒绝）、`-PackagesPath`、`-OutputPath`、`-Runtime`、`-BuildStamp`。
`release-gallery.yml` 的 `Prepare` job 按 UTC 计算一次 `MMdd` 构建后缀并传给全部平台 job，同一次发布的产物后缀一致；
本机运行省略 `-BuildStamp` 时取当前 UTC 日期。

外部工具（appimagetool、linuxdeploy、AppImage type2-runtime、create-dmg）在 `packaging-tools.json` 中固定版本与 SHA256，
脚本下载后校验，不使用 `continuous` 构建或未锁定的 Homebrew 版本。appimagetool 通过 `--runtime-file` 使用清单中的
runtime，否则它会自行下载 continuous runtime 并嵌入 AppImage。升级工具时只修改该清单的版本、URL 与 SHA256，并完整运行一次
Gallery 发布 workflow 验证全部平台。

macOS 签名沿用既有顺序：`Contents/MacOS` 下文件按序号（Ordinal）排序逐个签名、主程序最后，再签 bundle，最后
`codesign --verify --deep --strict`。本机无法授权 Finder 自动化时可加 `-SkipFinderLayout` 跳过 DMG 窗口布局，CI 不使用。

## 发布脚本职责

`PublishToLocal.ps1` 统一处理 AOT 和非 AOT 两条路径。
脚本先检查发布模式与输出路径，再清理旧产物。输出路径不能包含 `.`、`..`、通配符或指向目录的符号链接，
不能是文件系统根、用户主目录、仓库根及其上级；仓库内仅允许写入 `.artifacts/gallery_published` 或
`binaries/gallery_published` 的子目录。已有的非空输出目录须包含脚本写入的
`.atomui-gallery-publish-root` 标记，否则脚本拒绝覆盖。首次发布可使用不存在或为空的目录；
后续同一路径发布会替换先前由该脚本创建的产物。

当 `publishAot` 为 `true`：

- 要求 `buildType` 为 `Release`。
- 先执行显式 restore，并传入 `-p:Configuration=Release -p:GalleryPublishTrimmed=true -p:GalleryPublishAot=true`。
- `AtomUIGallery.Desktop.csproj` 将 `GalleryPublishTrimmed` 和 `GalleryPublishAot` 映射到本项目的发布属性。不要在命令行直接传全局 `PublishAot=true`，否则 source generator 等非最终可执行项目也会收到该属性，并可能触发 `NETSDK1207`。
- 检查 `project.assets.json` 包含 `Microsoft.DotNet.ILCompiler`，并存在目标 RID 对应的 restore target。
- 再执行 `dotnet publish --no-restore`，并显式传入 `-p:GalleryPublishTrimmed=true -p:GalleryPublishAot=true`。
- 检查输出目录里不存在普通 self-contained runtime 标志文件。

当 `publishAot` 为 `false`：

- Release 使用 self-contained single-file，并默认启用 ReadyToRun；`-publishReadyToRun false` 可构建对照产物。
  Debug 非 AOT 发布必须显式传 `-publishReadyToRun false`，避免误以为生成了 R2R。
- `AtomUIGallery.Desktop.csproj` 仅在 Gallery 发布入口把 `GalleryPublishReadyToRun` 映射到 SDK 属性，Toolchain 仍保持
  portable net10.0，不继承应用 RID/R2R。
- 该模式不作为默认正式发布形态。

脚本在两种模式开始前只清理当前 Gallery 应用 RID 中间目录里的 `linked/` 与 `R2R/`。SDK 的普通 `clean` 不会移除这两处；
从裁剪发布切换到未裁剪 R2R 时，旧的 ReactiveUI 产物曾让新调用方遇到 `MissingMethodException`。这些目录仅含可重建的
发布中间文件；清理不会触及源码、NuGet 缓存或 Toolchain 的独立输出。

## 产物校验规则

NativeAOT 发布成功不只看 `dotnet publish` 退出码。脚本还会拒绝包含下列文件的输出目录：

```text
coreclr.dll
clrjit.dll
libcoreclr.so
libcoreclr.dylib
libclrjit.so
libclrjit.dylib
System.Private.CoreLib.dll
AtomUIGallery.Desktop.dll
```

这些文件出现时，说明产物很可能退化成普通 self-contained 发布。

## 平台前置条件

macOS 任务安装：

```bash
brew untap --force aws/tap azure/bicep
brew install openssl@3
```

create-dmg 不通过 Homebrew 安装，由 `New-GalleryDmg.ps1` 按工具清单下载。Finder AppleScript 布局偶发超时时脚本最多重试 3 次，
每次失败都清理中间的 `rw.*.dmg`；最终未生成 DMG 时任务失败。

GitHub-hosted macOS runner 可能预置 `aws/tap` 和 `azure/bicep`。Gallery 发布不依赖这些 tap，workflow 在安装包依赖前先 untap，避免 Homebrew tap trust 检查把无关第三方 tap 的预警或强制校验带进发布流程。

`AtomUIGallery.Desktop.csproj` 保留 Apple Silicon 和 Intel Homebrew OpenSSL linker 搜索路径。

Linux AppImage 任务安装：

```bash
sudo apt-get install -y fuse libfuse2 patchelf desktop-file-utils file clang zlib1g-dev
sudo apt-get install -y libxcb1 libxcb-shape0 libxcb-shape0-dev xvfb
```

`xvfb` 供 AppImage 冒烟测试使用；linuxdeploy、appimagetool 与 runtime 不再由 workflow 手动下载。

`clang` 和 `zlib1g-dev` 是 Linux NativeAOT 链接所需的本地工具链依赖。

`New-GalleryAppImage.ps1` 在调用 appimagetool 前就生成完整的 AppDir：根目录 `AppRun`（设置 `LD_LIBRARY_PATH`
到 `usr/lib` 后执行 Gallery）、`.desktop`、普通文件形式的 `.DirIcon`；iconset 各尺寸另放入
`usr/share/icons/hicolor/<N>x<N>/apps/AtomUIGallery.png`（linuxdeploy 缺少主题图标时会失败），linuxdeploy 之后把根目录
`AtomUIGallery.png` 还原为 512 像素原图；并统一权限（目录 755、文件 644，`AppRun`、主程序与
`usr/lib` 下共享库 755），以满足 appimagetool 的 AppDir 校验。AppImage 只打包一次，随后解包检查 `AppRun`、`.desktop`、
`.DirIcon` 与主程序。workflow 另外在 `xvfb-run` 下运行解包后的 `AppRun` 20 秒作为冒烟测试（`timeout -k 10s 20s`，进程不响应 SIGTERM 时强制结束）。
全部安装包上传步骤设置 `if-no-files-found: error`，打包未生成产物时发布失败。

## 维护检查

修改 Gallery 发布 workflow 或 `PublishToLocal.ps1` 后至少运行：

```bash
ruby -e 'require "yaml"; YAML.load_file(".github/workflows/release-gallery.yml"); puts "ok"'
pwsh -NoLogo -NoProfile -Command '$errors = $null; $null = [System.Management.Automation.Language.Parser]::ParseFile("controlgallery/AtomUIGallery.Desktop/scripts/PublishToLocal.ps1", [ref]$null, [ref]$errors); if ($errors.Count) { $errors | Format-List; exit 1 }'
pwsh -NoLogo -NoProfile -Command 'Get-ChildItem controlgallery/AtomUIGallery.Desktop/scripts/packaging/*.ps1 | ForEach-Object { $errors = $null; $null = [System.Management.Automation.Language.Parser]::ParseFile($_.FullName, [ref]$null, [ref]$errors); if ($errors.Count) { $errors | Format-List; exit 1 } }'
pwsh -NoLogo -NoProfile -Command 'Get-Content controlgallery/AtomUIGallery.Desktop/scripts/packaging/packaging-tools.json -Raw | ConvertFrom-Json | Out-Null'
pwsh -NoLogo -NoProfile -Command '[xml](Get-Content -Path "controlgallery/AtomUIGallery.Desktop/AtomUIGallery.Desktop.csproj" -Raw) | Out-Null'
git diff --check
```

真实 NativeAOT publish 仍需要对应 GitHub runner、签名 secrets 和平台工具链。
