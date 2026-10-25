# Toolchain 构建、交付与验证

入口：[模块概览](overview.md)。设计动机见[设计思路](design.md)。本文面向维护构建工具的开发者；
产品包的跨项目注入规则与资产白名单由[构建与打包架构](../../architecture/foundations/build-and-packaging.md)拥有。

## 1. Profile 与输出隔离

`AtomUIToolchainProfile` 决定工具源码和引擎依赖，`Configuration` 决定 Debug/Release 编译配置，二者不能混用。
未指定 profile 时默认为 Tasks。三个托管 profile 均以 net10.0 为工具宿主：

| Profile | 工具产物 | 默认输出目录（相对 `.artifacts/bin/<Configuration>/toolchain/`） |
| --- | --- | --- |
| `Tasks` | `AtomUI.Build.Tasks.dll` 及 worker 依赖 | `tasks/net10.0/` |
| `ILLink8` | `AtomUI.Registration.ILLink8.dll` | `illink8/net10.0/` |
| `ILLink10` | `AtomUI.TypeMap.Linker.dll`、CLI 配置及 Cecil | `illink10/net10.0/` |

应用的框架和 RID 在调用边界隔离：Repository 为 Toolchain 的项目引用设置 `GlobalPropertiesToRemove`，
显式 MSBuild 调用使用同一属性列表的 `RemoveProperties`。Toolchain 自身通过 `TreatAsLocalProperty` 固定
`TargetFramework=net10.0`，清空工具的 `TargetFrameworks`、`RuntimeIdentifier(s)`，并关闭 AOT、裁剪、单文件、
ReadyToRun 与 self-contained。即使 NuGet 恢复路径丢失项目引用元数据，工具仍保持可移植身份。
Repository 不再改写 NuGet 的路径/恢复图项：这类改写曾删除应用本身的 RID，使普通 RID JIT 发布缺少 assets 目标。
应用的 RID、R2R、裁剪与 AOT 参数继续属于应用发布图，不影响工具输出目录。
Gallery 发布脚本在同一 RID 切换 AOT、裁剪 R2R 与普通 R2R 前，会清理应用的 `linked/`、`R2R/` 中间目录；
SDK 的 `dotnet clean` 不移除这些目录，直接复用可能把旧裁剪 DLL 混入新发布。此清理不涉及 Toolchain profile 输出。

每个 profile 的恢复根为 `.artifacts/AtomUI.Toolchain/obj/<Profile>/`；配置/TFM 的编译中间文件位于其下。
独立的 NuGet imports、assets 与输出避免 ILLink8/10 的同名依赖相互覆盖。旧 DLL 名称和已有命名空间属于内部
分发、任务和 custom-step 契约，保留名称不表示仍有多个源码项目。

## 2. 按用途选择构建入口

以下命令均在仓库根目录执行：

```sh
# 普通构建：只准备 Tasks worker。
dotnet build src/AtomUI.Toolchain/AtomUI.Toolchain.csproj

# 发布后端准备：默认 Tasks + ILLink8 + ILLink10。
dotnet build src/AtomUI.Toolchain/AtomUI.Toolchain.csproj -t:BuildManagedToolchain -c Release

# 完整维护者工具准备：上述托管工具 + ILC8 bundle。
dotnet build src/AtomUI.Toolchain/AtomUI.Toolchain.csproj -t:BuildToolchain -c Release
```

单独检查某个后端时，可使用正常的 restore/build 命令：

```sh
dotnet build src/AtomUI.Toolchain/AtomUI.Toolchain.csproj -p:AtomUIToolchainProfile=ILLink8
dotnet build src/AtomUI.Toolchain/AtomUI.Toolchain.csproj -p:AtomUIToolchainProfile=ILLink10
```

若需要 worker 加指定后端，使用默认 Tasks profile 的准备入口：

```sh
dotnet build src/AtomUI.Toolchain/AtomUI.Toolchain.csproj -t:BuildManagedToolchain -p:AtomUIToolchainBackendProfiles=ILLink10
```

`BuildManagedToolchain` 先恢复后端，再以新的 MSBuild 项目实例读取生成的 NuGet imports 并编译。
在同一个尚未读到 package imports 的项目实例里串接 `Restore;Build`，会使首次构建找不到已恢复的引擎引用。
不能用热缓存下成功代替这一入口的冷验证。

同一路径的 `ProjectReference AdditionalProperties` 也不足以让 NuGet 自动恢复自定义 profile。后端测试采用默认
Toolchain 的 build-only 引用，再通过上述准备入口构建需要的后端，并引用对应产物；不要求测试使用者预先执行隐含步骤。

## 3. 三种使用场景

| 场景 | 准备与执行 |
| --- | --- |
| 普通源码 build | Generator 的 build-only 引用准备 Tasks；不准备全部后端或原生编译器源码 |
| 源码 publish | Repository 在首次读取后端前按框架准备工具：现代目标准备 ILLink10，net8 托管准备 ILLink8，net8 NativeAOT 准备 worker 与原生 bundle |
| NuGet 消费 | restore 获取包内工具，SDK targets 调用它们；不构建 Toolchain 或下载 compiler 源码 |

准备必须早于具体后端的输入准备、转换和签名捕获。仅与其他任务并列声明 `BeforeTargets=_RunILLink`，不能保证
它们之间的先后顺序；Browser 的一些步骤还发生在更早阶段。实际挂接点以
`build/AtomUI.Repository.targets` 为准，调整时要用没有后端输出的首次 publish 验证。

正常 pack 由 `AtomUIPrepareRepositoryPackageTools` 在产品或 Generator 收集资产之前准备完整工具链。
Generator 自身不维护一套独立 pack 准备逻辑。`pack --no-build` 只消费已准备资产并校验完整性；
`scripts/BuildNuGetPackages.ps1` 先准备工具，再构建产品、以相同配置打包和校验。

只指定 `PublishTrimmed` 的普通 build 不等于 publish。工具执行阶段必须与真实 SDK 发布阶段对齐，不能在普通编译中
提前消费裁剪输出，也不能让应用的 NoBuild/NoRestore 标志破坏源码发布所需的工具准备。

## 4. ILC8 维护者准备

`PrepareNativeCompiler` 调用 `scripts/registration/prepare-native8-compiler.py`，根据
`src/AtomUI.Toolchain/Backends/ILC8/upstream.json` 的精确来源和版本构建受控 bundle。可通过
`AtomUINative8CompilerSourceRoot` 或 `ATOMUI_RUNTIME8_SOURCE` 指定已有的匹配源码；不匹配的源码不能复用。

自动准备源码时，在临时 Git checkout 中启用仓库局部的 `core.longpaths`，再检出定版源码，避免 Windows
工作区与 staging 路径叠加后超过传统路径长度限制。不修改全局 Git 配置；已有 checkout 仍须通过精确 HEAD
和源码完整性校验，缺失或被修改的源码不会被自动修复或忽略。

默认 bundle 位于 `.artifacts/tools/registration/net8/ilc/managed-bundle`，由
`AtomUINative8ManagedCompilerBundle` 配置。配方校验源码身份、适配器/公共源码内容与既有产物，使用锁和 staging
目录管理构建与发布；原生 helper 由消费发布的 SDK 按受支持版本恢复，不混入通用托管 bundle。

依赖恢复必须在空包缓存下成立。现行配方使用定版依赖所需的 dnceng 源和 nuget.org，不修改使用者的全局 NuGet
配置，也不依赖维护者本机已缓存的 NETStandard.Library。源码许可证和第三方声明随编译器工具交付。
上游编译器构建显式隔离 `Directory.Packages.props` 导入和中央版本管理，避免默认源码位于仓库 `.artifacts/` 下时
向上继承产品包配置；其显式定版依赖仍由编译器配方维护。
维护者配方的实际 `dotnet build` 子进程显式使用 `--disable-build-servers` 和 `-nr:false`，
使该次构建的节点随构建结束，避免继承的 `build-host.log` 句柄越过日志所属调用的生命周期。
这些参数只约束该子进程，不调用全局 build-server shutdown；配方源码哈希照常参与 bundle 身份与复用校验。
后端机制和平台限制见 [.NET 8 条件后端](../../architecture/foundations/aot-net8-conditional-backends.md)。

## 5. 维护时检查哪些入口

| 变更 | 需要一起核对的入口 |
| --- | --- |
| 新增任务或修改参数 | `Tasks/`、`TaskProcessHost`、`build/AtomUI.Build.Tasks.Process.cs`、统一任务声明及任务协议测试 |
| 新增构建产物或依赖 | `.csproj` 的对应 profile、`build/AtomUI.Repository.props` 白名单、工具解析与冻结 bundle、冷包消费 |
| 修改公共注册模型 | `Common/Registration` 及实际编译该源码的所有相关后端，不只构建默认 Tasks |
| 修改引擎适配或上游版本 | 对应后端、capability/输入身份、真实标记或原生生成、最终运行和删除断言 |
| 调整 targets 顺序 | 普通 build、冷 source publish、直接 pack、`--no-build` 和多包重复导入 |

通用 `UsingTask` 只在 `build/AtomUI.Toolchain.Tasks.targets` 声明，各 feature target 通过导入 guard 复用。
新增功能不另建 worker 或另一份工具列表。任务进程及文件生命周期的完整约束见
[构建任务架构](../../architecture/foundations/build-and-packaging.md#构建任务与文件生命周期)。

## 6. 验证和支持声明

本模块按[受影响验证规则](../../engineering/workflows/affected-verification.md#conditional-registration-backends)选择已有测试与专项。
默认 Tasks profile 编译通过不能证明其他 profile 已编译；单测通过也不能替代最终发布证据。

- 工程验证：从空恢复目录构建所需 profile，确认 bin/obj、依赖和普通 build 边界正确。
- 包验证：检查实际 DLL TFM、依赖组、工具白名单与 Import 闭包；新缓存只正常 restore，不补写 ZIP 或包缓存。
- 精度验证：实际执行资源/注册场景，同时检查未用片段删除；各引擎分别计算依赖，不能拿另一引擎的结果充数。
- 消费验证：核对最终 DLL、变换产物或原生程序与本次输入、链接结果和发布副本之间的身份链。
- 失败验证：缺工具、未知协议、输入篡改、过期或跨调用产物不能留下成功回执。

测试工程、探针和专用产物按[测试生命周期](../../engineering/development/test-value-and-lifecycle.md)清理。
阶段测试计数与一次性日志属于实施记录，不在本模块概览中充当长期支持承诺；当前 SDK、runtime、RID 和未验收边界
由正式 AOT 架构及后端文档维护。
