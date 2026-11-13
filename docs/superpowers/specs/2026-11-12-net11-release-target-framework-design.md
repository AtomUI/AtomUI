# .NET 11 SDK 构建与 Release 三目标框架发布设计

日期：2026-11-12。状态：已确认（Release 方案 A；发布路径去版本绑定方案 1）。来源：[AtomUI/AtomUI#538](https://github.com/AtomUI/AtomUI/issues/538)。

## 意图

VS 更新后部分开发机只剩 .NET SDK 11.0.100。仓库 `global.json`（`10.0.300` + `latestFeature`）禁止跨主版本前滚，
源码构建直接 NETSDK1141 失败。目标：

1. 只装 SDK 11 的环境可以构建 AtomUI 源码（修复 #538）。
2. Release 发布的 NuGet 包同时提供 `net11.0`、`net10.0`、`net8.0` 三个 `lib/` 目标框架，由 CI 用 SDK 11 统一构建。

## 已验证事实（release/6.0，SDK 11.0.100，macOS arm64）

- `global.json` 改为 `latestMajor` 后解析到 11.0.100；Debug（net10.0）与 Release（net10.0+net8.0）构建 0 错误。
- 临时把 `net11.0` 加入 Release 目标框架，整解决方案 Release 构建 0 错误，产出 `net11.0`。
- `AtomUI.Build.Tasks.Tests`（91）、`AtomUI.Generator.Tests`（463）、`AtomUI.TypeMap.Linker.Tests`（89）全部通过。
- 注册路由 `AtomUI.Registration.Framework.targets` 对 `>= 10.0` 统一走 OfficialTypeMap，net11.0 无需新增分支。
- `AtomUI.Toolchain` 固定 `net10.0` 且 `RollForward=Major`，只有 .NET 11 运行时也能执行，`tools/net10.0` 布局不变。

## 目标 / 非目标 / 反目标

- 目标：SDK 10（≥10.0.300）与 SDK 11 都能构建源码；CI Release 包必含三个框架，缺一即失败。
- 目标：net8 裁剪（ILLink8）、Native8、条件桥接（托管 / NativeAOT）三条发布路径不绑定 SDK 版本或补丁版本，
  与 6.2.8 普通 TypeMap 路径同一原则：按引擎能力与主版本准入，由现有冻结、hash、回执与产物验证兜底。
- 非目标：Debug / 开发框架（`AtomUIDevelopTargetFramework=net10.0`）不变；Gallery 仍以 net10.0 发布；
  Toolchain 仍编译为 net10.0；平台准入范围（Native8 仅 osx-arm64 宿主，桥接仅 osx-arm64 / browser-wasm）不变，
  它是平台验收边界而非版本绑定；不在包内自带 ILLink / ILC 引擎。
- 反目标：只装 SDK 10 的贡献者因本改动无法构建；CI 在 SDK 选错时静默产出少一个框架的包；
  去掉版本白名单后引擎不兼容却静默产出错误的注册结果（必须明确失败）。

## 方案（已选 A）

| 方案 | 做法 | 取舍 |
|---|---|---|
| **A（选定）** | `global.json` 保持 `10.0.300` + `latestMajor`；SDK ≥ 11 时 Release 自动追加 `net11.0`；CI 打包门禁强制三框架 | SDK 10 贡献者不受影响；本地 SDK 10 的 Release 只出两框架，以 CI 产物为准 |
| B | `global.json` 升到 `11.0.100`，`net11.0` 无条件加入 | 结果确定，但所有人必须装 SDK 11，且 Native8/桥接 10.0.300 门禁整体失效需重新准入 |
| C | `latestMajor` + `net11.0` 无条件加入 | 只装 SDK 10 的 Release 构建报 NETSDK1045，违反反目标，排除 |

## 设计

### 1. SDK 解析

`global.json`：`"version": "10.0.300"`, `"rollForward": "latestMajor"`。已在本地生效。最低仍为 10.0.300；
机器上装有更高主版本时自动使用最高版本。

### 2. 目标框架（`build/ProjectDefaults.props`）

新增 `AtomUILatestTargetFramework=net11.0`。Release 下当 `$(NETCoreSdkVersion)` 主版本 ≥ 11 时，
`AtomUITargetFrameworks` 追加它；否则保持 `net10.0;net8.0`。Debug 不变。用属性而非字面量，
与现有 `AtomUIDevelopTargetFramework` / `AtomUIProductionTargetFramework` 命名一致。

### 3. 打包门禁

- `scripts/BuildNuGetPackages.ps1:109` 与 `scripts/verification/PackageFrameworks.ps1:38` 的 Release 期望集改为
  `net8.0, net10.0, net11.0`。门禁在 CI 打包后执行，SDK 选错时直接失败（满足反目标）。
  本地用 SDK 10 运行该脚本的 Release 打包将按设计失败，并在报错中提示需要 SDK 11。
- 期望集只在一处定义（`PackageFrameworks.ps1` 导出），`BuildNuGetPackages.ps1` 复用，避免两处漂移。
- `scripts/verification/package-layout-allowlist.json`：为发布所用基线版本登记各 RuntimeProduct 包新增的
  `lib/net11.0/*`，reason 写“新增 .NET 11 目标框架”。
- package validation（ApiCompat）：新增 TFM 不是破坏性变更，预期通过；以首次 CI 运行为准。

### 4. CI 工作流

- `release-nuget-packages.yml`：`setup-dotnet` 同时安装 `10.0.x` 与 `11.0.x`，打包前打印 `dotnet --version`
  并断言主版本为 11。
- `verify-affected.yml`：在 `global-json-file` 之外追加 `11.0.x`，让 PR 校验与发布使用同一 SDK。
- `release-gallery.yml`（6 个平台作业）：同样安装 `10.0.x` + `11.0.x`，Gallery 用 SDK 11 构建、仍发布 net10.0。

### 5. 文档

- README / README.zh-CN 构建前提：SDK ≥ 10.0.300 可构建；Release 三框架打包需 SDK 11；NuGet 消费支持 .NET 8/10/11。
  （README 当前有未提交的 6.2.14 发版改动，实施时在其基础上追加，不覆盖。）
- Windows / Linux NativeAOT 发布文档的 `global.json` 片段与 SDK 版本说明同步；顺手修正 Linux 文档的 `10.0.301` 笔误。
- CHANGELOG 与对应版本 `docs/releases/<version>-api-changes(.zh-CN).md`：记录新增 net11.0 目标框架与 SDK 前滚策略。

### 6. 发布路径去版本绑定（方案 1：同主版本准入，验证兜底）

背景事实：SDK 为 net8 选用的 ILLink / ILC 包随 SDK 前移（10.0.300→8.0.27、10.0.403→8.0.33、11.0.100→8.0.34）。
实测 SDK 11 下普通 net8 裁剪发布直接 ATOMUIREG006 失败；把 ILLink 临时钉回 8.0.27 后发布并运行成功。
`dotnet/runtime` v8.0.27→v8.0.34 在 `src/coreclr/tools/`（ILC）与 `src/tools/illink/` 下零改动，
JIT-EE GUID 恒为 `0x4bceb905`，8.0.27 精确值只是身份白名单。targets 内钉包版本在首次 NuGet restore 时不可见，排除。

| 位置 | 现状 | 改为 |
|---|---|---|
| `Tasks/Net8LinkHostTasks.cs` | ILLink Tasks/engine InformationalVersion 必须等于 `8.0.27+commit`；后端常量必须相等 | 两者为同一官方 8.x（程序集 `illink, Version=8.0.0.0`、微软公钥、版本 ≥8.0.0 且主版本 8、Tasks 与引擎版本一致）；后端声明能力改为主版本标识 |
| `Tasks/Net8LinkHostTasks.cs` HostConfig、`build/AtomUI.Net8.ILLink.Task.cs` | `LatestPatch` 于 net10 | `Major`（只向上前滚，不会落到 .NET 8 运行时） |
| `Backends/ILLink8/ConditionalRegistrationBackend.cs` | 实际 ILLink 必须 `8.0.27+commit`，宿主主版本必须 10 | 实际 ILLink 主版本 8；宿主主版本 ≥10；回执记录实际版本 |
| `build/AtomUI.Registration.Native8.targets:38` | SDK 10.0.300、runtime 8.0.27 | 只保留 net8.0 与 full trimming |
| `Tasks/Native8HostTasks.cs` | ILC 包目录名必须 `8.0.27`；能力清单 `nativeToolPackageVersion` 必须 `8.0.27` | ILC 包为 `runtime.osx-arm64.microsoft.dotnet.ilcompiler/8.x`；能力清单的 `nativeToolPackageVersion` 主版本与之相同；编译器启动加 `exec --roll-forward Major` |
| `build/AtomUI.Registration.Bridge.targets:19,145` | SDK 10.0.300、TFM 恰为 net10.0、runtime 10.0.8 | 去掉 SDK 与 runtime 补丁条件；TFM 改为 OfficialTypeMap 家族（≥10）；RID、full trimming、单模块等非版本条件保留 |
| `Backends/ILLink10/ConditionalBridge/FrameworkBindingPolicy.cs` | runtime 包目录名 `10.0.8` / Mono `10.0.10`；RuntimeList 版本 `"10.0"` | 包名族不变，版本目录解析为 ≥10.0；RuntimeList `TargetFrameworkVersion` 与包主次版本一致且 ≥10.0 |
| `Backends/ILLink10/ConditionalBridge/BridgeProgram.cs` | 宿主主版本必须 10 | ≥10 |

不变：各路径的输入冻结、内容 hash、linked/published 回执、产物独立验证与平台准入。ILC 自身在 JIT-EE 接口
不匹配时启动即失败，不会产出注册结果。`OfficialTypeMapToolchain.Capability` 字符串是 ABI 标识，不改。
同主版本判定抽一个共享 helper（与 `OfficialTypeMapToolchain.IsSupportedLinker` 同形），各处复用，并补单元测试：
接受 8.0.27 / 8.0.33 / 8.0.34、拒绝 7.x / 9.x / 非微软公钥 / Tasks 与引擎版本不一致；桥接接受 10.0.x / 11.0.x 包、拒绝 9.0。

## 风险与已知限制

- 将来若某个 8.x 补丁改了 ILLink 内部 API，后端在 Install/Mark 阶段抛错或产物验证失败，发布明确失败而非静默。
  .NET 8 已于 2026-11-10 停止支持，8.0 补丁线基本封闭，此风险很低。
- 只装 .NET 11 运行时的机器：工具宿主改为 Major 前滚后可运行；net8 应用自身使用 SDK 恢复的官方 8.0.x runtime 包。
- `LangVersion=latest` 在 SDK 11 下对全部框架启用 C# 15；本地全框架构建已通过，作为已知差异记录。
- Gallery 在 SDK 11 下的 NativeAOT 发布：本地验证 osx-arm64；其余 5 个平台以首次 CI Gallery 运行为准。
  若某平台失败，回退为该作业固定 SDK 10（作业内改写 `global.json` 为 `latestFeature`），不阻塞 NuGet 发布。

## 交给实施者决定 / 我的判断

- 发布版本：我的判断是随 6.2.14 之后的下一个版本发布，allowlist 以 6.2.14 为基线登记。如需并入正在准备的
  6.2.14，基线改为上一个已发布版本。
- 实施位置：主工作树有未提交的 6.2.14 发版改动，实施在基于 `release/6.0` 的独立 worktree 中进行，不与发版改动混合。
- 验证范围：Release 三框架构建、打包门禁脚本及其 Pester 测试、上述三个工具链测试项目、osx-arm64 Gallery AOT 发布；
  SDK 11.0.100 与 10.0.403 下各跑 osx-arm64 的 net8 裁剪、Native8、桥接托管与桥接 NativeAOT 发布并运行；
  本机无 wasm workload，browser 桥接以单元测试与首次 CI 为准。不跑全量回归，除非另行要求。
- 状态：方案 1 已由用户选定（2026-11-12）。
