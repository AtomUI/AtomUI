# .NET 11 SDK 构建、三框架发布与发布路径去版本绑定 Implementation Plan

> Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** SDK 10/11 都能构建 AtomUI；CI Release 包含 net8.0/net10.0/net11.0；net8 裁剪、Native8、条件桥接发布不再绑定 SDK 或补丁版本。

**Architecture:** `global.json` 用 `latestMajor`；`ProjectDefaults.props` 在 SDK ≥ 11 时为 Release 追加 net11.0，打包门禁强制三框架。
发布路径把精确版本白名单换成“官方身份 + 同主版本”判定（共享 helper，形同 `OfficialTypeMapToolchain.IsSupportedLinker`），
工具宿主改为 Major 前滚；冻结、hash、回执与产物验证保持不变。

**Tech Stack:** MSBuild targets/props、C#（AtomUI.Toolchain 三个 profile：Tasks / ILLink8 / ILLink10）、xUnit v3 + Shouldly、PowerShell 打包脚本与 Pester 式测试、GitHub Actions。

**Spec:** `docs/developments/specs/2026-11-12-net11-release-target-framework-design.md`

## Global Constraints

- 基于 `release/6.0`，在独立 worktree（分支 `feature/net11-sdk`）实施；主工作树的未提交 6.2.14 发版改动不得混入。
- `global.json`：`"version": "10.0.300"`, `"rollForward": "latestMajor"`。
- Debug 只出 `net10.0`；Release：SDK 主版本 ≥ 11 时 `net10.0;net8.0;net11.0`，否则 `net10.0;net8.0`。
- 打包门禁 Release 期望集 `net8.0, net10.0, net11.0`，只在 `scripts/verification/PackageFrameworks.ps1` 定义一次。
- Toolchain 仍编译为 net10.0；`OfficialTypeMapToolchain.Capability` 字符串不改。
- 平台准入不变：Native8 仅 osx-arm64 宿主 → osx-arm64/osx-x64；桥接托管仅 osx-arm64 与 browser-wasm，桥接原生仅 osx-arm64。
- 工具宿主前滚一律 `Major`（只向上，不落到 .NET 8）。
- 代码内 summary/注释用英文，与周边代码密度一致；不增加兼容回退路径。
- 不提交、不推送，除非用户明确要求；提交信息走 `commit-msg` skill，无任何 AI 署名。

## Review Focus

- 只装 .NET 11 运行时（无 .NET 10 运行时）的机器上，ILLink8 宿主、ILC8 编译器、桥接程序必须能启动 —— Task 3/4/5 各有 Major 前滚断言测试。
- ILLink Tasks 与引擎来自不同 8.x 补丁（如 Tasks 8.0.34、illink 8.0.33）必须拒绝 —— Task 3 测试覆盖。
- 桥接 runtime 包主次版本与 RuntimeList `TargetFrameworkVersion` 不一致（如 `11.0.0` 包配 `10.0` 清单）必须拒绝 —— Task 5 测试覆盖。
- 用 SDK 10 本地跑 Release 打包时门禁失败信息必须指出需要 SDK 11 —— Task 1 测试覆盖。
- 预发布 8.x / 11.x（`-preview` / `-rc`）informational 版本应按数字部分判定，不因后缀误拒 —— Task 3/5 测试覆盖。

---

### Task 1: SDK 前滚、Release 三框架与打包门禁

**Files:**
- Modify: `global.json`（已在主工作树改好，worktree 内同步）
- Modify: `build/ProjectDefaults.props:7-14`
- Modify: `scripts/verification/PackageFrameworks.ps1:38`
- Modify: `scripts/BuildNuGetPackages.ps1:109`
- Modify: `scripts/verification/package-layout-allowlist.json`
- Test: `scripts/verification/verify-package-layout.Tests.ps1`

**Interfaces:**
- Produces: MSBuild 属性 `AtomUILatestTargetFramework=net11.0`；PowerShell 函数 `Get-AtomUIExpectedProductFrameworks -BuildType <string> -> string[]`（定义于 `PackageFrameworks.ps1`，`Assert-AtomUIPackageFrameworks` 与 `BuildNuGetPackages.ps1` 共用）。

- [ ] **Step 1: 写失败测试** —— 在 `verify-package-layout.Tests.ps1` 末尾（沿用其 `Assert-Equal` 风格）新增场景：
  `Get-AtomUIExpectedProductFrameworks -BuildType Release` 等于 `@('net8.0','net10.0','net11.0')`，`-BuildType Debug` 等于 `@('net10.0')`；
  用一个仅含 `lib/net8.0`、`lib/net10.0` 的伪包调用 `Assert-AtomUIPackageFrameworks -BuildType Release` 必须抛错，消息包含 `net11.0` 与 `SDK 11`。
- [ ] **Step 2: 运行确认失败** —— `pwsh -NoProfile -File scripts/verification/verify-package-layout.Tests.ps1`，预期新场景失败（函数不存在）。
- [ ] **Step 3: 实现** —— `PackageFrameworks.ps1` 新增并导出 `Get-AtomUIExpectedProductFrameworks`；第 38 行改为调用它；缺框架时错误消息追加 `Release packages require .NET SDK 11 or later to produce net11.0.`；`BuildNuGetPackages.ps1:109` 改为调用它。
- [ ] **Step 4: 运行确认通过** —— 同 Step 2 命令，全部场景 PASS。
- [ ] **Step 5: 目标框架** —— `ProjectDefaults.props` 新增 `<AtomUILatestTargetFramework>net11.0</AtomUILatestTargetFramework>`；Release 行之后追加：
  当 `$([MSBuild]::VersionGreaterThanOrEquals('$(NETCoreSdkVersion)', '11.0'))` 时 `AtomUITargetFrameworks` 追加 `;$(AtomUILatestTargetFramework)`。
- [ ] **Step 6: 构建验证** —— SDK 11：`dotnet build AtomUI.slnx -c Release -nologo -v:m` exit 0 且 `.artifacts/bin/Release/net11.0` 存在。
  条件分支：`dotnet msbuild src/AtomUI.Core/AtomUI.Core.csproj -getProperty:AtomUITargetFrameworks -p:Configuration=Release` 输出 `net10.0;net8.0;net11.0`；
  追加 `-p:NETCoreSdkVersion=10.0.403` 输出 `net10.0;net8.0`；`-p:Configuration=Debug` 输出 `net10.0`。
- [ ] **Step 7: allowlist** —— 在 `package-layout-allowlist.json` 新增基线键 `"6.2.14"`：为每个 RuntimeProduct 包登记 `lib/net11.0/*`，reason `"Adds the .NET 11 target framework."`。包清单取自 `scripts/NuGetPackageProjects.ps1` 中 `PackageRole -eq 'RuntimeProduct'` 的项。
- [ ] **Step 8: 打包冒烟** —— `pwsh scripts/BuildNuGetPackages.ps1 -BuildType Release -PackageOutputDir <scratch>/pkgs`，输出含 `Verified Release product target frameworks: net8.0, net10.0, net11.0`。

### Task 2: CI 工作流使用 SDK 11

**Files:**
- Modify: `.github/workflows/release-nuget-packages.yml:41-44`
- Modify: `.github/workflows/verify-affected.yml:38-40`
- Modify: `.github/workflows/release-gallery.yml`（6 处 `Setup .NET 10`，行 66/149/223/290/352/397 附近）

**Interfaces:** 无代码接口；依赖 Task 1 的门禁。

- [ ] **Step 1:** 三个工作流的 `setup-dotnet` 改为同时安装（`verify-affected.yml` 保留 `global-json-file` 并加 `dotnet-version`）：
  ```yaml
  dotnet-version: |
    10.0.x
    11.0.x
  ```
  步骤名改为 `Setup .NET 10 and 11`。
- [ ] **Step 2:** `release-nuget-packages.yml` 在 `Build and pack` 之前新增 `Verify .NET SDK` 步骤（pwsh）：打印 `dotnet --version`，主版本 `< 11` 时 `throw "NuGet release packaging requires .NET SDK 11 or later."`。
- [ ] **Step 3: 校验** —— `python3 -c "import yaml,sys;[yaml.safe_load(open(f)) for f in sys.argv[1:]]" .github/workflows/*.yml` 无异常；若本机有 `actionlint` 则其 exit 0。

### Task 3: net8 裁剪（ILLink8）去版本绑定

**Files:**
- Create: `src/AtomUI.Toolchain/Common/Registration/Net8EngineToolchain.cs`
- Modify: `src/AtomUI.Toolchain/AtomUI.Toolchain.csproj`（ILLink8 profile 的 `Compile Include` 加入该文件；Tasks profile 已由 `Common/Registration/*.cs` 包含）
- Modify: `src/AtomUI.Toolchain/Tasks/Net8LinkHostTasks.cs:15-16,34-40,59,84-101`
- Modify: `src/AtomUI.Toolchain/Backends/ILLink8/ConditionalRegistrationBackend.cs:12-13,52-58`
- Modify: `src/AtomUI.Toolchain/Backends/ILLink8/Input/InitializeStep.cs:61`
- Modify: `build/AtomUI.Net8.ILLink.Task.cs:46-48`
- Modify: `src/AtomUI.Toolchain/Backends/ILLink8/README.md:5`
- Test: `tests/AtomUI.Build.Tasks.Tests/Net8EngineToolchainTests.cs`（新建）

**Interfaces:**
- Produces（namespace `AtomUI.Build.Tasks.Registration`, `internal static class Net8EngineToolchain`）：
  - `const int EngineMajor = 8;`
  - `const string BackendCapability = "atomui-illink8-v1";`
  - `bool IsSupportedLinker(string identity, string informationalVersion)` —— `illink`、程序集版本 `8.0.0.0`、微软公钥 `31BF3856AD364E35`、informational 数字部分主版本 8 且 ≥ 8.0.0。
  - `bool IsSameEngine(string tasksInformationalVersion, string linkerInformationalVersion)` —— 两者数字部分相等（忽略 `+commit`）。
  - `bool IsSupportedNativeCompilerPackage(string packageDirectory, string bundleNativeToolVersion)` —— 供 Task 4。
  - 版本解析复用 `OfficialTypeMapToolchain.TryParseVersion`。
- ILLink8 profile 不编译 `OfficialTypeMapToolchain.cs`：把 `TryParseVersion` 移到 `Net8EngineToolchain` 旁的共享位置，或在 ILLink8 profile 一并编译 `OfficialTypeMapToolchain.cs`——取后者（单文件、无引擎依赖）。

- [ ] **Step 1: 写失败测试** `Net8EngineToolchainTests`：
  ```csharp
  [Theory]
  [InlineData("illink, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", "8.0.27+a6bde67c", true)]
  [InlineData("illink, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", "8.0.34+0123abcd", true)]
  [InlineData("illink, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", "8.0.35-servicing.26500.1+ab", true)]
  [InlineData("illink, Version=10.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", "10.0.8", false)]
  [InlineData("illink, Version=7.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", "7.0.20", false)]
  [InlineData("illink, Version=8.0.0.0, Culture=neutral, PublicKeyToken=0123456789abcdef", "8.0.34", false)]
  [InlineData("illink, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", "10.0.8", false)]
  public void Linker_identity_requires_official_engine_major_8(string identity, string version, bool accepted)
      => Net8EngineToolchain.IsSupportedLinker(identity, version).ShouldBe(accepted);

  [Fact]
  public void Tasks_and_engine_must_be_one_servicing_release()
  {
      Net8EngineToolchain.IsSameEngine("8.0.34+aaa", "8.0.34+bbb").ShouldBeTrue();
      Net8EngineToolchain.IsSameEngine("8.0.34+aaa", "8.0.33+aaa").ShouldBeFalse();
  }
  ```
  另加 `Prepared_host_rolls_forward_by_major`：读取 `PrepareNet8LinkHostTask` 写出的 `atomui.net8-linkhost.runtimeconfig.json` 常量（把 `HostConfig` 改为 `internal const` 以便断言），`rollForward` 为 `Major`。
- [ ] **Step 2:** `dotnet test tests/AtomUI.Build.Tasks.Tests --filter FullyQualifiedName~Net8EngineToolchain` 预期编译失败（类型不存在）。
- [ ] **Step 3: 实现 helper** 于 `Net8EngineToolchain.cs`（顶部注释一行：同主版本准入，引擎内部 API 由后端安装与产物验证兜底）。
- [ ] **Step 4: 接线**
  - `Net8LinkHostTasks.cs`：删除 `LinkerVersion`；用 `IsSupportedLinker` + `IsSameEngine` 校验 `ILLink.Tasks.dll` 与 `illink.dll`，错误消息 `Controlled registration requires official ILLink Tasks and engine 8.x from one servicing release; found {tasks} / {engine}.`；`host-state.json` 的 `linkerVersion` 记录实际引擎版本；`ValidateBackend` 改读常量字段 `LinkerCapability` 并与 `Net8EngineToolchain.BackendCapability` 比较；`HostConfig` 的 `rollForward` 改 `Major`。
  - `ConditionalRegistrationBackend.cs`（ILLink8）：`LinkerProductVersion` / `RuntimeSourceCommit` 替换为 `public const string LinkerCapability = "atomui-illink8-v1";`；`ValidateToolchain` 用 `IsSupportedLinker(typeof(MarkStep).Assembly.FullName, actual)` 且 `Environment.Version.Major >= 10`。
  - `Input/InitializeStep.cs:61`：`backend` 记录 `typeof(MarkStep).Assembly` 的实际 informational 版本。
  - `AtomUI.Net8.ILLink.Task.cs`：`--roll-forward Major`，注释改为“只向上前滚到 .NET 10 及以后，不会选择 .NET 8 运行时”。
  - README 第 5 行改述为绑定官方 ILLink 8.x 同主版本。
- [ ] **Step 5:** 同 Step 2 命令 PASS；`dotnet test tests/AtomUI.Build.Tasks.Tests` 全部 PASS。
- [ ] **Step 6: 端到端（SDK 11）** —— 在 `tests/AtomUI.Registration.Fixtures/_TmpSdkProbe/` 建临时 net8.0 Exe（引用 `src/AtomUI.Desktop.Controls`、`Avalonia.Headless`，`Program.cs` 调 `UseAtomUI(b => b.UseDesktopControls())` 后创建 `Button` 并打印 `PROBE_PASS` + `RuntimeInformation.FrameworkDescription`）；
  `dotnet publish -c Release -r osx-arm64 -p:PublishTrimmed=true -p:TrimMode=full -o <scratch>/net8-trim` exit 0，运行输出 `PROBE_PASS .NET 8.0.34`。不再需要钉 ILLink。

### Task 4: Native8 去版本绑定

**Files:**
- Modify: `build/AtomUI.Registration.Native8.targets:38-39`
- Modify: `src/AtomUI.Toolchain/Tasks/Native8HostTasks.cs:60-65,75,106-109`
- Modify: `src/AtomUI.Toolchain/Backends/ILC8/README.md`（“Automatic .NET 8 routing currently admits …” 句）
- Test: `tests/AtomUI.Build.Tasks.Tests/Net8EngineToolchainTests.cs`

**Interfaces:**
- Consumes: Task 3 的 `Net8EngineToolchain.IsSupportedNativeCompilerPackage(string packageDirectory, string bundleNativeToolVersion)`：目录名为 `runtime.osx-arm64.microsoft.dotnet.ilcompiler` 的子目录、其版本目录可解析且主版本 8、与 `bundleNativeToolVersion` 主版本相同。

- [ ] **Step 1: 写失败测试**
  ```csharp
  [Theory]
  [InlineData("/p/runtime.osx-arm64.microsoft.dotnet.ilcompiler/8.0.27", "8.0.27", true)]
  [InlineData("/p/runtime.osx-arm64.microsoft.dotnet.ilcompiler/8.0.34", "8.0.27", true)]
  [InlineData("/p/runtime.osx-arm64.microsoft.dotnet.ilcompiler/10.0.15", "8.0.27", false)]
  [InlineData("/p/runtime.osx-x64.microsoft.dotnet.ilcompiler/8.0.34", "8.0.27", false)]
  [InlineData("/p/runtime.osx-arm64.microsoft.dotnet.ilcompiler/not-a-version", "8.0.27", false)]
  public void Native_compiler_package_requires_official_ilc_major_8(string dir, string bundle, bool accepted)
      => Net8EngineToolchain.IsSupportedNativeCompilerPackage(dir, bundle).ShouldBe(accepted);
  ```
  另加 `Native_compiler_launcher_rolls_forward_by_major`：把 `Native8HostTasks` 中生成启动脚本的代码抽成 `internal static string CreateLauncher(string dotnetHost, string cleanupArguments)`，断言输出包含 `exec --roll-forward Major`。
- [ ] **Step 2:** 运行 `--filter FullyQualifiedName~Native_compiler` 预期失败。
- [ ] **Step 3: 实现** —— `Native8HostTasks.cs`：第 61 行改用 `IsSupportedNativeCompilerPackage(nativePackage, info.nativeToolPackageVersion)`（先读能力清单再校验包，调整顺序），消息 `Native8 requires SDK-restored runtime.osx-arm64.microsoft.dotnet.ilcompiler 8.x; found {path}.`；第 75 行改为能力清单版本可解析且主版本 8；启动与清理命令改为 `dotnet exec --roll-forward Major "$base/host/ilc.dll"`。
  `Native8.targets:38` 条件改为 `'$(TargetFramework)' != 'net8.0' or '$(TrimMode)' != 'full'`，文本 `Native8 registration requires net8.0 and full trimming.`。
- [ ] **Step 4:** `dotnet test tests/AtomUI.Build.Tasks.Tests` 全部 PASS。
- [ ] **Step 5: 端到端（SDK 11 与 10.0.403）** —— 先确认 `.artifacts/tools/registration/net8/ilc/managed-bundle` 存在（不存在则 `dotnet build src/AtomUI.Toolchain/AtomUI.Toolchain.csproj -t:BuildToolchain`，需 runtime8 源码，缺失则记录为阻塞并报告）；
  对 Task 3 的临时探针 `LIBRARY_PATH=/opt/homebrew/lib:/opt/homebrew/opt/openssl@3/lib dotnet publish -c Release -r osx-arm64 -p:PublishAot=true -o <scratch>/net8-aot` exit 0，日志含 `[AtomUI ILC8] host=`，运行输出 `PROBE_PASS`。
  SDK 10.0.403 通过在探针目录放置 `global.json`（`"version": "10.0.403", "rollForward": "disable"`）复跑。

### Task 5: 条件桥接去版本绑定

**Files:**
- Modify: `build/AtomUI.Registration.Bridge.targets:19-20,145-146`
- Modify: `src/AtomUI.Toolchain/Backends/ILLink10/ConditionalBridge/FrameworkBindingPolicy.cs:46-65`
- Modify: `src/AtomUI.Toolchain/Backends/ILLink10/ConditionalBridge/BridgeProgram.cs:12-14`
- Modify: `src/AtomUI.Toolchain/Backends/ILLink10/ConditionalBridge/README.md:64`
- Test: `tests/AtomUI.TypeMap.Linker.Tests/FrameworkBindingPolicyTests.cs`（新建）

**Interfaces:**
- Produces（`FrameworkBindingPolicy` 内 `internal static`）：
  - `bool IsAdmittedRuntimePack(string packName, string packVersion)` —— 包名以 `Microsoft.NETCore.App.Runtime.` 开头（含 `Mono.browser-wasm`），版本可解析且 ≥ 10.0。
  - `bool IsMatchingRuntimeList(string targetFrameworkVersion, string packVersion)` —— 清单 `TargetFrameworkVersion` 解析 ≥ 10.0 且主次版本等于包版本主次。
  - 版本解析复用 `OfficialTypeMapToolchain.TryParseVersion`（ILLink10 profile 已编译该文件）。

- [ ] **Step 1: 写失败测试**
  ```csharp
  [Theory]
  [InlineData("Microsoft.NETCore.App.Runtime.osx-arm64", "10.0.8", true)]
  [InlineData("Microsoft.NETCore.App.Runtime.osx-arm64", "10.0.15", true)]
  [InlineData("Microsoft.NETCore.App.Runtime.osx-arm64", "11.0.0", true)]
  [InlineData("Microsoft.NETCore.App.Runtime.Mono.browser-wasm", "11.0.0-rc.2.26473.1", true)]
  [InlineData("Microsoft.NETCore.App.Runtime.osx-arm64", "9.0.23", false)]
  [InlineData("Some.Other.Pack", "10.0.8", false)]
  public void Runtime_pack_admission_uses_family_and_major(string name, string version, bool accepted)
      => Assert.Equal(accepted, FrameworkBindingPolicy.IsAdmittedRuntimePack(name, version));

  [Theory]
  [InlineData("10.0", "10.0.15", true)]
  [InlineData("11.0", "11.0.0", true)]
  [InlineData("10.0", "11.0.0", false)]
  [InlineData("9.0", "9.0.23", false)]
  public void Runtime_list_must_match_pack_framework(string listVersion, string packVersion, bool accepted)
      => Assert.Equal(accepted, FrameworkBindingPolicy.IsMatchingRuntimeList(listVersion, packVersion));
  ```
- [ ] **Step 2:** `dotnet test tests/AtomUI.TypeMap.Linker.Tests --filter FullyQualifiedName~FrameworkBindingPolicy` 预期失败。
- [ ] **Step 3: 实现** —— `Create` 中第 47-53 行改用 `IsAdmittedRuntimePack(pack.Parent.Name, pack.Name)`，消息 `The bridge requires an SDK-resolved Microsoft.NETCore.App runtime pack for .NET 10 or later; found {path}.`；第 62 行改用 `IsMatchingRuntimeList`。`BridgeProgram` 改为 `Environment.Version.Major < 10` 拒绝，用法文本 `.NET 10 or later`。
  `Bridge.targets:19` 条件去掉 `NETCoreSdkVersion`，TFM 判断改为 `'$(_AtomUIRegistrationTargetPlatform)' == ''`（桌面）配 `osx-arm64`、`'$(_AtomUIRegistrationTargetPlatform)' == 'browser'` 配 `browser-wasm`（家族已由外层 `OfficialTypeMap` 条件保证 ≥10）；文本 `The conditional bridge admits full managed trimming on osx-arm64 or browser-wasm for .NET 10 and later.`。
  `Bridge.targets:145` 去掉 `NETCoreSdkVersion`、`RuntimeFrameworkVersion`、`TargetFramework != 'net10.0'`，改为 `'$(_AtomUIRegistrationTargetPlatform)' != ''` 拒绝平台框架；文本 `The bridge NativeAOT route admits osx-arm64 single-module executables for .NET 10 and later.`。
- [ ] **Step 4:** `dotnet test tests/AtomUI.TypeMap.Linker.Tests` 与 `tests/AtomUI.Build.Tasks.Tests` 全部 PASS。
- [ ] **Step 5: 端到端（SDK 11 与 10.0.403）** —— 临时 `_TmpSdkProbeLib`（net8.0 类库，引用 `src/AtomUI.Desktop.Controls`，含一个继承 `AtomUI.Desktop.Controls.Button` 的公开控件）+ `_TmpSdkProbeApp`（net10.0 与 net11.0 各一次，引用该类库并实例化该控件）。
  `-p:PublishTrimmed=true -p:TrimMode=full` 与 `-p:PublishAot=true` 各发布一次 osx-arm64：exit 0，构建日志出现桥接回执路径 `AtomUIRegistration/bridge10`，运行输出 `PROBE_PASS`。browser-wasm 本机无 workload，不跑，在报告中注明。
- [ ] **Step 6: 清理** —— 删除全部 `_TmpSdkProbe*` 目录与 scratch 发布输出；`git status` 不含临时文件。

### Task 6: 文档与发布说明

**Files:**
- Modify: `README.md`、`README.zh-CN.md`（构建前提段，第 66-71 / 72-77 行附近）
- Modify: `docs/architecture/foundations/aot-net8-conditional-backends.md`（“状态与范围”“.NET 10 消费 net8-only 包”两段的版本限定句）
- Modify: `docs/engineering/platforms/windows-native-aot-publish.md`、`docs/engineering/platforms/linux-native-aot-publish.md`（`global.json` 片段与 SDK 版本句；修正 `10.0.301`）
- Modify: `docs/releases/6.2.4-api-changes*.md` 不改（历史记录）；新增条目写入下一版本的 CHANGELOG 与 `docs/releases/<next>-api-changes(.zh-CN).md`（若该文件尚不存在则在 CHANGELOG `Unreleased` 段落记录）
- Modify: `tests/AtomUI.Registration.Fixtures/README.md:49`、`LINK_INPUTS.md:3`（“repository-pinned SDK 10.0.300” → “SDK 10.0.300 or later”）

- [ ] **Step 1:** 按 spec 第 5 节与第 6 节逐处改写：源码构建需 SDK ≥ 10.0.300；Release 三框架打包需 SDK 11；NuGet 消费支持 .NET 8/10/11；net8 裁剪 / Native8 / 桥接按官方引擎同主版本准入、不设 SDK 或补丁白名单；平台准入范围不变。
- [ ] **Step 2:** `grep -rn "10\.0\.300\|8\.0\.27" README*.md docs/architecture docs/engineering tests/AtomUI.Registration.Fixtures/*.md src/AtomUI.Toolchain/Backends/*/README.md` 只剩“已验证组合/历史证据”语境的引用。
- [ ] **Step 3: 最终验证** —— SDK 11：Debug 与 Release 整解决方案构建 exit 0；`AtomUI.Build.Tasks.Tests`、`AtomUI.Generator.Tests`、`AtomUI.TypeMap.Linker.Tests` 全 PASS；Gallery `osx-arm64` NativeAOT 发布 exit 0 且能启动到主窗口（按 `atomui-gallery-local-debug` skill）。
- [ ] **Step 4:** 向用户汇报变更与验证结果，询问是否提交（提交信息经 `commit-msg` skill）。
