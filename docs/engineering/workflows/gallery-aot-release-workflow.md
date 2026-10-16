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

## AtomUITools Checkout

发布流程会拉取 private 仓库 `AtomUI/AtomUITools` 的 `develop` 分支来构建打包工具。由于这是跨仓库 checkout，`GITHUB_TOKEN` 不能默认读取该仓库，必须传入 `secrets.ACCESS_TOKEN`。

`AtomUITools` 和主仓库同属 `AtomUI` 组织，但标准 `GITHUB_TOKEN` 仍不应假定可读取另一个 private 仓库。`ACCESS_TOKEN` 应是 GitHub PAT 或 fine-grained token，并至少具备 `AtomUI/AtomUITools` 的只读 Contents 权限。这个 token 过期、未授权到该仓库，或没有配置到当前发布仓库的 Actions secrets 时，`actions/checkout` 会在 `Fetching the repository` 阶段失败。

`AtomUITools` 源码 checkout 到 `AtomUIToolsSourceDir`，构建输出写入 `AtomUIToolsBinDir`。这两个目录不能只靠大小写区分；Windows 和 macOS runner 的文件系统通常大小写不敏感，`AtomUITools` 与 `atomuitools` 会指向同一个目录。

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
brew install create-dmg openssl@3
```

GitHub-hosted macOS runner 可能预置 `aws/tap` 和 `azure/bicep`。Gallery 发布不依赖这些 tap，workflow 在安装包依赖前先 untap，避免 Homebrew tap trust 检查把无关第三方 tap 的预警或强制校验带进发布流程。

`AtomUIGallery.Desktop.csproj` 保留 Apple Silicon 和 Intel Homebrew OpenSSL linker 搜索路径。

Linux AppImage 任务安装：

```bash
sudo apt-get install -y fuse libfuse2 patchelf desktop-file-utils file clang zlib1g-dev
```

`clang` 和 `zlib1g-dev` 是 Linux NativeAOT 链接所需的本地工具链依赖。

Linux AppImage 打包后必须包含根目录 `AppRun`。`release-gallery.yml` 在 `AtomUITools.Linux create-appimage` 之后调用 `controlgallery/AtomUIGallery.Desktop/scripts/EnsureAppImageLauncher.ps1`：

- 检查 `AtomUIGallery.AppDir/usr/bin/AtomUIGallery.Desktop` 是否存在。
- 如果 `AtomUIGallery.AppDir/AppRun` 缺失，生成标准 launcher，设置 `LD_LIBRARY_PATH` 到 `usr/lib`，再执行 Gallery 可执行文件。
- 用同名 AppImage 重新调用 `appimagetool`，避免上传旧的缺入口产物。
- 从最终 AppImage 中提取 `AppRun` 并执行 shell 语法检查。缺少 `AppRun` 时直接让 workflow 失败。

## 维护检查

修改 Gallery 发布 workflow 或 `PublishToLocal.ps1` 后至少运行：

```bash
ruby -e 'require "yaml"; YAML.load_file(".github/workflows/release-gallery.yml"); puts "ok"'
pwsh -NoLogo -NoProfile -Command '$errors = $null; $null = [System.Management.Automation.Language.Parser]::ParseFile("controlgallery/AtomUIGallery.Desktop/scripts/PublishToLocal.ps1", [ref]$null, [ref]$errors); if ($errors.Count) { $errors | Format-List; exit 1 }'
pwsh -NoLogo -NoProfile -Command '$errors = $null; $null = [System.Management.Automation.Language.Parser]::ParseFile("controlgallery/AtomUIGallery.Desktop/scripts/EnsureAppImageLauncher.ps1", [ref]$null, [ref]$errors); if ($errors.Count) { $errors | Format-List; exit 1 }'
pwsh -NoLogo -NoProfile -Command '[xml](Get-Content -Path "controlgallery/AtomUIGallery.Desktop/AtomUIGallery.Desktop.csproj" -Raw) | Out-Null'
git diff --check
```

真实 NativeAOT publish 仍需要对应 GitHub runner、签名 secrets 和平台工具链。
