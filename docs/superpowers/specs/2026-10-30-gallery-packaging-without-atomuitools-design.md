# Gallery 安装包打包去除 AtomUITools 设计

日期：2026-10-30。状态：待用户评估。触发：6.2.10 Gallery 发布中 Linux AppImage 两个 job 失败。

## 1. 背景

`release-gallery.yml` 的 6 个 job 都要跨仓库检出私有仓库 `AtomUI/AtomUITools`（`develop` 分支，依赖 `secrets.ACCESS_TOKEN`），
先编译该工具，再调用 `create-zip` / `create-appimage` / `create-dmg` 生成安装包。

6.2.10 的失败根因：

1. AtomUITools 在调用 appimagetool 前**没有创建 AppDir 根目录的 `AppRun`**；仓库内 `EnsureAppImageLauncher.ps1`
   一直在事后补 `AppRun` 并二次打包。
2. workflow 下载的是 appimagetool `continuous` 构建。2026-10-29 上游提交 `b082d2e`（PR #135）新增 AppDir 权限与
   `.DirIcon` 校验，缺少 `AppRun` 或权限不符时直接拒绝打包，第一次打包即失败，补救脚本没有机会运行。
3. 6.2.8 成功使用的是 continuous build 313（git `854e19e`），6.2.10 拿到 build 333（git `b082d2e`）；AtomUITools 版本
   （`0.0.7-build+6544785f`）与 linuxdeploy 版本两次相同。

另外发现：构建时间后缀 `MMdd` 由各 job 运行时本地时钟（UTC）各自生成，6.2.10 同一次发布出现
`6.2.10.1029-win-arm64.zip` 与 `6.2.10.1030-*.dmg` 不一致。

## 2. AtomUITools 现有职责（对齐基线）

| 平台 | 代码量 | 行为 |
| --- | --- | --- |
| Windows `create-zip` | 约 440 行 | 复制发布目录中的 `*.dll`、`*.exe` 到暂存目录，`ZipFile.CreateFromDirectory`（Optimal，不含根目录） |
| Linux `create-appimage` | 约 570 行 | 建 AppDir；主程序 `chmod +x`；`*.so` → `usr/lib`；`AppIcon` 目录内容 → `usr/share/icons`；`InstallerApplicationIcon` → `.DirIcon` 与 `{Name}.png`；生成 `{Name}.desktop`（根目录与 `usr/share/applications` 各一份）；`linuxdeploy --appdir`；`appimagetool <AppDir> <file>` |
| macOS `create-dmg` | 约 1170 行 | 建 `{Name}.app`（`Contents/MacOS`、`PlugIns`、`Resources`）；主程序与 `*.dylib` → `MacOS`；`{AppIcon}.icns` → `Resources`；写 `Info.plist`；`codesign --force -s <identity>` 逐个签 `MacOS` 下文件（按序号排序，主程序最后），再签 bundle，`codesign --verify --deep --strict` 校验；`create-dmg` 生成 DMG |
| 公共 | 约 510 行 | 解析 `InstallerConfig.xml`、日志、查找外部命令 |

产物命名（`appendVersion` / `appendBuildTime` 均开启）：

| 平台 | 规则 | 6.2.10 实例 |
| --- | --- | --- |
| Windows | `{Name}-{Version}.{MMdd}-{rid}.zip` | `AtomUIGallery-6.2.10.1030-win-x64.zip` |
| Linux | `{Name}-{rid}-{Version}.{MMdd}.AppImage` | `AtomUIGallery-linux-x64-6.2.8.1021.AppImage`（6.2.8） |
| macOS | `{Name}-{Version}.{MMdd}-{rid}.dmg` | `AtomUIGallery-6.2.10.1030-osx-arm64.dmg` |

`Info.plist` 键：`CFBundleDevelopmentRegion=English`、`CFBundleExecutable`、`CFBundleGetInfoString`、`CFBundleIconFile`、
`CFBundleIdentifier`、`CFBundleInfoDictionaryVersion=6.0`、`CFBundleLongVersionString`、`CFBundleName`、
`CFBundlePackageType=APPL`、`CFBundleShortVersionString`、`CFBundleSignature=????`、`CFBundleVersion`、
`CSResourcesFileMapped=true`、`NSHumanReadableCopyright`、`NSPrincipalClass=NSApplication`、`NSHighResolutionCapable=True`。

DMG 布局：`--volname {Title}`、`--background {Background}`、`--window-pos 200 120`、`--window-size 800 530`、
`--icon-size 100`、`--icon {Name}.app 230 230`、`--app-drop-link 600 230`。

## 3. 目标与非目标

目标：

- 移除对 AtomUITools 私有仓库、`ACCESS_TOKEN` 与工具编译步骤的依赖。
- 三种产物（zip / AppImage / DMG）的格式、文件名规则、包内结构、`Info.plist`、`.desktop`、签名方式与现有基线一致。
- 所有外部工具固定版本并校验 SHA256，不再使用 `continuous` 或未锁定的 Homebrew 版本。
- 一次发布的所有产物使用同一个构建时间后缀。
- 脚本可在本机复现（macOS 本机可跑 zip 与 DMG，Linux 可在容器或 CI 跑）。

非目标：

- 不新增 macOS 公证（notarization）、hardened runtime 或时间戳签名；不新增 Windows 安装器（MSI）。
- 不改变 `PublishToLocal.ps1` 的 NativeAOT 发布流程与 `InstallerConfig.*.xml` 的内容格式。
- 不引入 Velopack / PupNet 等第三方打包框架。

## 4. 总体设计

新增目录 `controlgallery/AtomUIGallery.Desktop/scripts/packaging/`：

| 文件 | 职责 |
| --- | --- |
| `GalleryPackaging.Common.ps1` | 读取 `InstallerConfig.xml`（只读用到的元素）、组合产物文件名、命令执行与退出码检查、外部工具下载与 SHA256 校验 |
| `packaging-tools.json` | 外部工具清单：名称、版本、各架构下载 URL、SHA256 |
| `New-GalleryZip.ps1` | Windows zip |
| `New-GalleryAppImage.ps1` | Linux AppImage（取代 `create-appimage` 与 `EnsureAppImageLauncher.ps1`） |
| `New-GalleryDmg.ps1` | macOS `.app` 与 DMG |

三个入口脚本统一参数：`-ConfigPath`、`-PackagesPath`、`-OutputPath`、`-Runtime`、`-BuildStamp`（`MMdd`），
DMG 另有 `-CertIdentity`（为空则不签名，与现状一致）。脚本只依赖 pwsh 7 与平台原生工具，不需要编译。

### 4.1 Windows zip

1. 暂存目录 `{OutputPath}/{Name}`，复制 `PackagesPath` 下的 `*.dll`、`*.exe`（与现状一致，不复制其他文件）。
2. `[System.IO.Compression.ZipFile]::CreateFromDirectory(stage, zip, Optimal, includeBaseDirectory: false)`。
3. 删除暂存目录，输出 `{Name}-{Version}.{BuildStamp}-{rid}.zip`。

### 4.2 Linux AppImage

1. 建 AppDir `{OutputPath}/{Name}.AppDir`：`usr/bin`、`usr/lib`、`usr/share/applications`、`usr/share/icons`。
2. 主程序复制到 `usr/bin`；`*.so` 复制到 `usr/lib`；`AppIcon` 目录内容复制到 `usr/share/icons`（保持现状）。
3. `InstallerApplicationIcon` 复制为根目录普通文件 `.DirIcon` 与 `{Name}.png`（不用符号链接，满足新版 `.DirIcon` 校验）。
4. 生成 `{Name}.desktop`（内容同现状，`Exec` 改为取 `AssemblyName` 而非硬编码），根目录与 `usr/share/applications` 各一份。
5. **打包前生成 `AppRun`**（沿用 `EnsureAppImageLauncher.ps1` 中的脚本内容：设置 `LD_LIBRARY_PATH` 后 `exec usr/bin/{AssemblyName}`）。
6. `linuxdeploy --appdir <AppDir>`（版本固定为现用 `1-alpha-20250213-2`）。
7. 统一权限：目录 `0755`、普通文件 `0644`、`AppRun` 与主程序及 `usr/lib/*.so` `0755`，满足新版 appimagetool 的权限检查。
8. `appimagetool <AppDir> {Name}-{rid}-{Version}.{BuildStamp}.AppImage`，设置 `ARCH`（`x86_64` / `aarch64`）。
9. 产物校验（取代 `EnsureAppImageLauncher.ps1`）：`--appimage-extract` 后确认 `AppRun`（`sh -n`）、`.desktop`、`.DirIcon`、
   `usr/bin/{AssemblyName}` 存在；只打包一次。

### 4.3 macOS DMG

1. 建 `{Name}.app/Contents/{MacOS,PlugIns,Resources}`；主程序（`chmod +x`）与 `*.dylib` → `MacOS`。
2. `{AppIcon}.icns`（无扩展名时补 `.icns`）→ `Resources`；配置了图标但文件不存在时失败（同现状）。
3. 按第 2 节键表生成 `Info.plist`，`plutil -lint` 校验。
4. `-CertIdentity` 非空时：`MacOS` 下全部文件按序号排序、主程序最后，逐个 `codesign --force -s <id>`；再签 bundle；
   `codesign --verify --deep --strict` 失败即中止。keychain 解锁仍在 workflow 中完成。
5. `create-dmg` 使用第 2 节相同参数生成 `{Name}-{Version}.{BuildStamp}-{rid}.dmg`。

`create-dmg` 不再 `brew install`，改为按 `packaging-tools.json` 下载固定版本的 release 源码包（它是纯 shell 脚本）并校验 SHA256；
`openssl@3` 仅为 AtomUITools 所需，一并移除。

### 4.4 外部工具锁定

| 工具 | 版本 | 来源 |
| --- | --- | --- |
| appimagetool | `1.9.1`（不再使用 continuous） | AppImage/appimagetool release，x86_64 与 aarch64 |
| linuxdeploy | `1-alpha-20250213-2`（现用版本） | linuxdeploy release |
| create-dmg | `v1.3.0`（2026-07-02，Homebrew 当前 stable，即 6.2.10 macOS job 实际使用的版本） | create-dmg/create-dmg release 源码包 |

每个下载都校验 `packaging-tools.json` 中的 SHA256。升级工具时只改这份清单并重新跑发布验证。

### 4.5 统一构建时间后缀

`release-gallery.yml` 新增首个 job `prepare`，按 UTC 计算一次 `BuildStamp`（`MMdd`）作为 job output，其余 6 个 job
`needs: prepare` 并传给脚本。脚本缺省该参数时取当前 UTC 日期，供本机复现使用。

### 4.6 Workflow 变更

- 删除每个 job 的 `Checkout AtomUITools code`、`Build AtomUITools` 步骤与 `AtomUITools*` 环境变量。
- 打包步骤改为调用 `packaging/New-Gallery*.ps1`；删除 `EnsureAppImageLauncher.ps1` 调用与脚本本身。
- macOS 去掉 `brew install create-dmg openssl@3` 与相关 tap 处理。
- 不再使用 `secrets.ACCESS_TOKEN`（是否删除该 secret 由仓库管理员在全部切换后决定）。
- 同步 `docs/engineering/workflows/gallery-aot-release-workflow.md` 中 AtomUITools Checkout 一节。

## 5. 验证

| 层次 | 内容 |
| --- | --- |
| 本机 | macOS 上跑 `New-GalleryZip.ps1`（用 win 发布目录或伪目录）与 `New-GalleryDmg.ps1`（签名用本机身份或不签），对比旧工具产物 |
| 对等对比 | 同一提交分别用旧工具与新脚本打包：zip 条目清单；AppImage `--appimage-extract` 后的文件清单、权限、`.desktop`、`AppRun`；DMG 挂载后 `.app` 文件清单与 `Info.plist` 逐键比较、`codesign -dv` 输出 |
| 冒烟 | macOS 本机启动 `.app`；Linux 在 CI 用 `xvfb-run` 启动 AppImage 数秒确认进程存活；Windows 解压后运行 exe |
| 脚本测试 | 文件名组合、构建后缀插入、配置解析写临时 Pester 测试，验证后按测试生命周期规范删除 |

## 6. 实施阶段（每阶段独立可回退）

| 阶段 | 内容 | 完成判据 |
| --- | --- | --- |
| P0（独立，立即） | 仅将 appimagetool 下载改为 `1.9.1`，重跑 6.2.10 Gallery 发布 | 两个 Linux job 成功，AppImage 可运行 |
| P1 | 新增 `packaging/` 公共脚本、工具清单、统一构建后缀；Windows 切换到新脚本 | 两个 Windows job 成功，zip 条目与旧产物一致 |
| P2 | Linux 切换，删除 `EnsureAppImageLauncher.ps1` | 两个 Linux job 成功，AppDir 对等对比通过，冒烟通过 |
| P3 | macOS 切换（create-dmg 锁定版本） | 两个 macOS job 成功，`Info.plist`/签名/DMG 布局对等，冒烟通过 |
| P4 | 删除全部 AtomUITools 步骤、环境变量与 `ACCESS_TOKEN` 使用，更新文档 | workflow 中无 AtomUITools 引用，完整发布一次成功 |

P1–P3 每阶段只切换一个平台，其他平台仍用 AtomUITools，出问题只回退该平台的提交。P4 之前不修改 AtomUITools 仓库。

## 7. 风险与对策

| 风险 | 对策 |
| --- | --- |
| macOS 签名顺序或范围与旧工具不一致导致校验失败 | 完全复制现有顺序与参数，`codesign --verify --deep --strict` 作为硬门禁，P3 单独进行 |
| create-dmg 在 runner 上依赖 Finder AppleScript 布局 | 与现状使用同一工具，仅改为锁定版本；create-dmg 对 AppleScript 失败不重试，脚本自行最多重试 3 次并清理中间镜像（实施后补充） |
| linuxdeploy 对 NativeAOT 产物的依赖部署行为变化 | 锁定现用版本，对等对比 `usr/lib` 清单 |
| 锁定的工具出现安全修复 | 只需更新 `packaging-tools.json` 与 SHA256，并跑一次完整发布验证 |

## 8. 规模估计

PowerShell 约 400–600 行（公共 100–150、zip 40、AppImage 150、DMG 200），workflow 净删除约 80 行；
P1–P4 各需一次完整 Gallery 发布运行验证。

## 9. 待确认事项

1. Linux 图标目录：保持现状（`AppIcon` 目录内容平铺到 `usr/share/icons`），还是借此改为标准
   `usr/share/icons/hicolor/<size>/apps/{Name}.png`（会改变 AppDir 内结构，不影响启动）。默认保持现状。
2. 构建时间后缀来源：workflow 启动时的 UTC 日期（默认），还是 tag 提交日期（同一 tag 重跑结果完全一致）。
3. P4 后是否删除仓库 secret `ACCESS_TOKEN`（确认无其他 workflow 使用后由管理员处理）。
4. P0 是否立即执行以完成 6.2.10 的 Linux 发布，还是等 P2 落地后再补发 Linux 包。
