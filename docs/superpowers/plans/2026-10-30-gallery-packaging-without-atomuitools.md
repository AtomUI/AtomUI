# Gallery 打包去除 AtomUITools Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 用仓库内 PowerShell 脚本取代私有 AtomUITools，生成与现状一致的 Gallery zip / AppImage / DMG。

**Architecture:** `controlgallery/AtomUIGallery.Desktop/scripts/packaging/` 下一个公共脚本 + 三个平台入口脚本 + 外部工具清单；
`release-gallery.yml` 新增 `prepare` job 统一构建后缀，各平台 job 按阶段逐个切换到新脚本。

**Tech Stack:** pwsh 7、GitHub Actions、linuxdeploy、appimagetool、create-dmg、codesign、System.IO.Compression。

**Spec:** `docs/superpowers/specs/2026-10-30-gallery-packaging-without-atomuitools-design.md`

## Global Constraints

- 产物文件名：zip `{Name}-{Version}.{BuildStamp}-{rid}.zip`；AppImage `{Name}-{rid}-{Version}.{BuildStamp}.AppImage`；DMG `{Name}-{Version}.{BuildStamp}-{rid}.dmg`。
- 外部工具版本：appimagetool `1.9.1`、linuxdeploy `1-alpha-20250213-2`、create-dmg `v1.3.0`，下载后校验 SHA256。
- `Info.plist` 键与 DMG 布局参数逐项照抄 spec 第 2 节；签名：`MacOS` 下文件序号排序、主程序最后，`codesign --force -s <id>`，再签 bundle，`codesign --verify --deep --strict`。
- 不新增公证、hardened runtime、MSI；不修改 `PublishToLocal.ps1` 与 `InstallerConfig.*.xml`；Linux 图标保持平铺到 `usr/share/icons`。
- `BuildStamp` 为 workflow 启动时的 UTC `MMdd`，由 `prepare` job 输出；脚本缺省时取当前 UTC 日期。
- 新测试为临时测试，放 `/private/tmp/atomui-test-packaging/`，验证后删除；不在 `.superpowers/` 写任何内容。
- 每个阶段一个提交，提交与推送需用户明确授权；P4 之前不修改 AtomUITools 仓库。

## Review Focus

- `InstallerConfig.xml` 中 `<Version>` 已由 `PublishToLocal.ps1` 替换为真实版本：脚本必须读取发布目录中的 config，不读仓库模板（模板为 `__VERSION__`）。
- 发布目录额外包含 `.pdb`、`config/`、`Assets/` 等：zip 只收 `*.dll`/`*.exe`，AppImage 只收主程序与 `*.so`，DMG 只收主程序与 `*.dylib`（与旧工具一致）。
- AppImage 在 CI 中 FUSE 不可用时的自检：用 `--appimage-extract` 而非直接运行。
- macOS 未提供 `CertIdentity`（本机复现）时跳过签名但仍生成 DMG。
- 旧/新产物对等对比必须基于同一提交、同一 `BuildStamp`。

---

### Task 1: P0 固定 appimagetool 并补发 6.2.10 Linux 包

**Files:**
- Modify: `.github/workflows/release-gallery.yml`（两处 `releases/download/continuous/appimagetool-*.AppImage`）

- [ ] **Step 1:** 两处 URL 改为 `releases/download/1.9.1/appimagetool-{x86_64|aarch64}.AppImage`。
- [ ] **Step 2:** 用户授权后提交 `fix(CI): 固定 appimagetool 为 1.9.1 以避免上游 continuous 校验变更` 并推送 `release/6.0`。
- [ ] **Step 3:** 触发 `release-gallery.yml`：`--ref release/6.0 -f TargetBranch=v6.2.10 -f BuildConfiguration=Release -f PublishAot=true`。
  Expected: 6 个 job 全部 success，Linux artifact 含 `AtomUIGallery-linux-{x64|arm64}-6.2.10.MMdd.AppImage`。

### Task 2: P1 公共脚本、工具清单、统一构建后缀与 Windows 切换

**Files:**
- Create: `controlgallery/AtomUIGallery.Desktop/scripts/packaging/GalleryPackaging.Common.ps1`
- Create: `controlgallery/AtomUIGallery.Desktop/scripts/packaging/packaging-tools.json`
- Create: `controlgallery/AtomUIGallery.Desktop/scripts/packaging/New-GalleryZip.ps1`
- Modify: `.github/workflows/release-gallery.yml`（新增 `prepare` job；两个 Windows job 的打包步骤）
- Test: `/private/tmp/atomui-test-packaging/GalleryPackaging.Tests.ps1`（临时 Pester）

**Interfaces:**
- Produces（`GalleryPackaging.Common.ps1`，dot-source 使用）：
  - `Read-GalleryInstallerConfig -Path <string>` → `[pscustomobject]`，字段 `Name, AssemblyName, Version, AppBundleIdentifier, Title, Company, CompanyFullName, CompanyUrl, Copyright, AppIcon, InstallerApplicationIcon, Background`；缺 `Name`/`AssemblyName`/`Version` 抛错。
  - `Get-GalleryBuildStamp [-BuildStamp <string>]` → 合法 `MMdd` 原样返回，空值返回当前 UTC `MMdd`，非 4 位数字抛错。
  - `Get-GalleryArtifactName -Kind <zip|appimage|dmg> -Name -Version -BuildStamp -Runtime` → Global Constraints 中的文件名。
  - `Invoke-GalleryCommand -FilePath <string> -ArgumentList <string[]> [-WorkingDirectory <string>]`：非零退出码抛错。
  - `Install-GalleryPackagingTool -Name <string> -Architecture <string> -Destination <string>` → 下载 `packaging-tools.json` 中对应 URL，SHA256 不符抛错，返回本地路径。
- Produces（`New-GalleryZip.ps1`）参数：`-ConfigPath -PackagesPath -OutputPath -Runtime [-BuildStamp]`。

- [ ] **Step 1: 写失败测试**

```powershell
# 临时验证，功能完成后删除
BeforeAll { . "$env:ATOMUI_REPO/controlgallery/AtomUIGallery.Desktop/scripts/packaging/GalleryPackaging.Common.ps1" }
Describe 'Get-GalleryArtifactName' {
    It 'zip'      { Get-GalleryArtifactName -Kind zip -Name AtomUIGallery -Version 6.2.10 -BuildStamp 1030 -Runtime win-x64 | Should -Be 'AtomUIGallery-6.2.10.1030-win-x64.zip' }
    It 'appimage' { Get-GalleryArtifactName -Kind appimage -Name AtomUIGallery -Version 6.2.10 -BuildStamp 1030 -Runtime linux-x64 | Should -Be 'AtomUIGallery-linux-x64-6.2.10.1030.AppImage' }
    It 'dmg'      { Get-GalleryArtifactName -Kind dmg -Name AtomUIGallery -Version 6.2.10 -BuildStamp 1030 -Runtime osx-arm64 | Should -Be 'AtomUIGallery-6.2.10.1030-osx-arm64.dmg' }
}
Describe 'Get-GalleryBuildStamp' {
    It 'keeps valid stamp' { Get-GalleryBuildStamp -BuildStamp 0101 | Should -Be '0101' }
    It 'rejects invalid'   { { Get-GalleryBuildStamp -BuildStamp 13x } | Should -Throw }
    It 'defaults to UTC'   { Get-GalleryBuildStamp | Should -Be ([DateTime]::UtcNow.ToString('MMdd')) }
}
Describe 'Read-GalleryInstallerConfig' {
    It 'reads published dmg config' {
        $c = Read-GalleryInstallerConfig -Path "$TestDrive/c.xml"   # 写入含 Name/AssemblyName/Version=6.2.10/AppIcon/Background 的样本
        $c.Version | Should -Be '6.2.10'; $c.AppIcon | Should -Be 'Assets/Images/AtomUIGallery'
    }
    It 'rejects template version' { { Read-GalleryInstallerConfig -Path "$TestDrive/t.xml" } | Should -Throw }  # Version=__VERSION__
}
```

- [ ] **Step 2:** `ATOMUI_REPO=$PWD pwsh -NoProfile -c "Invoke-Pester /private/tmp/atomui-test-packaging -Output Detailed"`。Expected: FAIL（脚本不存在）。
- [ ] **Step 3:** 实现 `GalleryPackaging.Common.ps1`；`packaging-tools.json` 写入三种工具各架构的 URL 与实测 SHA256（下载后用 `Get-FileHash -Algorithm SHA256` 计算）。版本校验：`Version` 必须是 SemVer，`__VERSION__` 抛错。
- [ ] **Step 4:** 同 Step 2 命令。Expected: 全部 PASS。
- [ ] **Step 5:** 实现 `New-GalleryZip.ps1`（spec 4.1）。本机验证：`dotnet publish` 一个 win-x64 非 AOT 输出或以伪目录（含 `a.dll`、`b.exe`、`c.pdb`、`config/InstallerConfig.xml`）运行，Expected: zip 只含 `a.dll`、`b.exe`，文件名符合规则。
- [ ] **Step 6:** workflow：新增 `prepare` job（`ubuntu-latest`，输出 `build_stamp: $(date -u +%m%d)`）；两个 Windows job `needs: prepare`，打包步骤改为
  `pwsh $galleryScriptsPath/packaging/New-GalleryZip.ps1 -ConfigPath "$dir/config/InstallerConfig.xml" -PackagesPath "$dir/packages" -OutputPath "...zip/x64" -Runtime win-x64 -BuildStamp ${{ needs.prepare.outputs.build_stamp }}`；
  Windows job 删除 AtomUITools 检出与编译步骤。其余平台 job 也 `needs: prepare`，暂不改打包方式。
- [ ] **Step 7:** 授权后提交 `feat(Gallery): 用仓库内脚本生成 Windows zip 并统一构建后缀`、推送，触发 Gallery workflow（`TargetBranch=release/6.0`）。
  Expected: Windows 两个 job success；下载 zip 与 Task 1 的 6.2.10 zip 对比条目清单（仅版本后缀可不同）。

### Task 3: P2 Linux AppImage 切换

**Files:**
- Create: `controlgallery/AtomUIGallery.Desktop/scripts/packaging/New-GalleryAppImage.ps1`
- Delete: `controlgallery/AtomUIGallery.Desktop/scripts/EnsureAppImageLauncher.ps1`
- Modify: `.github/workflows/release-gallery.yml`（两个 Linux job）

**Interfaces:**
- Consumes: Task 2 公共函数。
- Produces: 参数 `-ConfigPath -PackagesPath -OutputPath -Runtime [-BuildStamp]`；输出 AppImage 与 `{Name}.AppDir`（保留供检查）。

- [ ] **Step 1:** 实现 spec 4.2 第 1–9 步。`AppRun` 内容取自现 `EnsureAppImageLauncher.ps1` 的 here-string；权限：`find AppDir -type d -exec chmod 755`、`-type f -exec chmod 644`，再对 `AppRun`、`usr/bin/{AssemblyName}`、`usr/lib/*.so` `chmod 755`；`appimagetool` 调用设置 `ARCH`；工具通过 `Install-GalleryPackagingTool` 获取。
- [ ] **Step 2:** 产物自检（spec 4.2 第 9 步）写在脚本末尾：`--appimage-extract` 到临时目录，断言 `AppRun`（`sh -n` 通过）、`{Name}.desktop`、`.DirIcon`（普通文件）、`usr/bin/{AssemblyName}` 存在，否则抛错。
- [ ] **Step 3:** workflow：Linux job 删除 appimagetool / linuxdeploy 的手动下载、AtomUITools 检出与编译，打包步骤改调新脚本；删除 `EnsureAppImageLauncher.ps1` 调用与文件；新增冒烟步骤
  `xvfb-run -a timeout 20s ./squashfs-root/AppRun || test $? -eq 124`（进程存活 20 秒视为通过）。
- [ ] **Step 4:** 授权后提交 `feat(Gallery): 用仓库内脚本生成 Linux AppImage`、推送并触发 Gallery workflow。
  Expected: Linux 两个 job success；对比 Task 1 AppImage 的 extract 清单：新增 `AppRun`（旧包由补救脚本生成，同样存在），其余文件一致，权限符合 Step 1。

### Task 4: P3 macOS DMG 切换

**Files:**
- Create: `controlgallery/AtomUIGallery.Desktop/scripts/packaging/New-GalleryDmg.ps1`
- Modify: `.github/workflows/release-gallery.yml`（两个 macOS job）

**Interfaces:**
- Consumes: Task 2 公共函数。
- Produces: 参数 `-ConfigPath -PackagesPath -OutputPath -Runtime [-BuildStamp] [-CertIdentity]`。

- [ ] **Step 1:** 实现 spec 4.3 第 1–5 步；`Info.plist` 用 here-string 按 spec 第 2 节键顺序生成后 `plutil -lint`；签名顺序 `Sort-Object { $_.Name -eq $AssemblyName }, { $_.FullName }`（Ordinal）；create-dmg 解压到临时目录后调用其 `create-dmg` 脚本。
- [ ] **Step 2:** 本机验证（osx-arm64）：先 `PublishToLocal.ps1 -publishAot true` 到 `.artifacts/gallery_published/pkg-check`，再运行新脚本（不传 `-CertIdentity`），Expected: DMG 生成；挂载后 `.app` 内容为主程序 + `*.dylib` + `Resources/AtomUIGallery.icns` + `Info.plist`；`plutil -p Info.plist` 键值与 spec 一致；`open` 启动 Gallery 成功后关闭。
- [ ] **Step 3:** 本机签名验证：用本机任一 codesigning 身份（`security find-identity -v -p codesigning`）运行，Expected: `codesign --verify --deep --strict` 通过。无可用身份时记录跳过。
- [ ] **Step 4:** workflow：macOS job 删除 `brew install create-dmg openssl@3`、tap 处理、AtomUITools 检出与编译；打包步骤改调新脚本并传 `-CertIdentity "${{ secrets.CERT_IDENTITY }}"`；保留 keychain 导入与解锁步骤。
- [ ] **Step 5:** 授权后提交 `feat(Gallery): 用仓库内脚本生成 macOS DMG`、推送并触发 Gallery workflow。
  Expected: macOS 两个 job success；下载 DMG，`hdiutil attach` 后对比 Task 1 DMG 的 `.app` 文件清单、`Info.plist`、`codesign -dv` 的 Identifier/Authority。

### Task 5: P4 清理与文档

**Files:**
- Modify: `.github/workflows/release-gallery.yml`（删除 `AtomUITools*` 环境变量及残留引用）
- Modify: `docs/engineering/workflows/gallery-aot-release-workflow.md`（AtomUITools Checkout 一节改为打包脚本说明与工具清单维护方式）
- Delete: `/private/tmp/atomui-test-packaging/`

- [ ] **Step 1:** `grep -rn "AtomUITools\|ACCESS_TOKEN" .github controlgallery docs/engineering` Expected: 无结果（`docs/superpowers` 历史记录除外）。
- [ ] **Step 2:** 更新文档：脚本入口与参数、`packaging-tools.json` 升级流程（改版本与 SHA256 后跑一次完整 Gallery 发布）、`BuildStamp` 来源。
- [ ] **Step 3:** 删除临时 Pester 测试目录；`git diff --check`。
- [ ] **Step 4:** 授权后提交 `chore(CI): 移除 Gallery 发布对 AtomUITools 的依赖并更新文档`、推送，触发一次完整 Gallery workflow（`TargetBranch=release/6.0`）。Expected: 6 个 job 全部 success，6 个产物使用同一 `BuildStamp`。
- [ ] **Step 5:** 向用户报告：是否删除 `ACCESS_TOKEN` secret 由用户确认其他 workflow 无引用后决定。
