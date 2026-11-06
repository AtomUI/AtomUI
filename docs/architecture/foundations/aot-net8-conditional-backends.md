# .NET 8 条件注册后端

本文记录 .NET 8 条件依赖后端的实现、接线和交付边界。生成 ABI 的逐字段定义由
[ConditionalRecord 契约](../../reference/aot/conditional-registration-contract.md)拥有；注册生命周期继续遵守
[Control 注册契约](control-registration-contracts.md)。

## 状态与范围

产品库 Debug 为 net10.0，Release 为 net8.0 与 net10.0。普通非裁剪程序使用完整注册；裁剪程序必须由实际
ILLink/ILC 依赖图选择注册片段。禁止全包保留、源代码使用清单、独立近似调用图和 ILLink 预分析替代 ILC。

net8 发布按目标框架自动选择兼容后端，未知工具组合立即失败。目前的原生 SDK 接线限定
SDK 10.0.300、当前目标 runtime / ILC 8.0.33：macOS arm64 工具宿主对应 osx-arm64 / osx-x64 完整应用，
Windows x64 工具宿主对应 win-x64 完整应用。工具宿主运行在 .NET 10；不因此改变目标 runtime。
Windows 已完成维护者编译和原始 46 项机制/运行矩阵；正常包、冷缓存消费与最终清理验收按
[Windows 交付记录](../../superpowers/progress/2026-11-05-windows-theme-acceptance.md)分别记账，不能只凭移除平台门禁宣布完成。
本轮修补后的8.0.33实际验收为Windows x64；既有macOS证据来自8.0.27，不能移用于新版本。
Linux 原生消费和维护者构建、其他 Windows 架构仍无本轮验收证据。
Browser 仍采用 .NET 10；不新增 net8 Browser 宿主。

### 外部应用的修补包选择

仓库通过自己的 `Directory.Build.targets` 默认选择已验收的修补包；NuGet 注册资产不替外部应用更改 SDK 的包选择。
NuGet 还原会设置 `ExcludeRestorePackageImports=true`，因此包内修改版本会造成还原和构建不一致。
固定 SDK 10.0.300 下的 Native8 应用应在自己的工程或 `Directory.Build.targets` 声明：

```xml
<PropertyGroup>
  <RuntimeFrameworkVersion>8.0.33</RuntimeFrameworkVersion>
</PropertyGroup>
<ItemGroup>
  <KnownILCompilerPack Update="Microsoft.DotNet.ILCompiler">
    <ILCompilerPackVersion Condition="'%(KnownILCompilerPack.TargetFramework)' == 'net8.0'">8.0.33</ILCompilerPackVersion>
  </KnownILCompilerPack>
</ItemGroup>
```

Windows net10 **原生**旧包桥接对应全局 `RuntimeFrameworkVersion=10.0.14`；同时将上述 compiler 条件改为
`net10.0`、版本改为 `10.0.14`。

**托管裁剪**应用通过 framework 元数据选择 runtime，避免全局属性同时覆盖 ILLink 工具版本：

```xml
<ItemGroup>
  <FrameworkReference Update="Microsoft.NETCore.App" RuntimeFrameworkVersion="8.0.33" />
</ItemGroup>
```

net10 Windows 托管桥接将元数据版本改为 `10.0.14`。这保留 ILLink8/8.0.27 和 ILLink10/10.0.8；
桥接门槛检查 SDK 实际解析的 Windows runtime pack 为唯一的10.0.14，仍验证物理pack、manifest和签名。
该区别来自[固定 SDK 的工具包选择实现](https://raw.githubusercontent.com/dotnet/sdk/v10.0.300/src/Tasks/Microsoft.NET.Build.Tasks/ProcessFrameworkReferences.cs)。
普通非桥接消费遵循应用选择的 SDK/runtime。
restore 与 publish 必须使用相同 RID、Configuration、SelfContained 和裁剪/AOT 属性；独立 restore 后使用
`publish --no-restore` 可验证这两个阶段确实消费同一组输入。版本不符合已验收组合时仍由严格门槛拒绝。

## 定义与依赖闭包

生成器为 net8 输出仅含字符串的 package、fragment、condition 记录，包括完整程序集身份、稳定片段身份、
候选类型和规范 thunk。读取记录本身不保留候选类型。进入包的 selected collector 才请求相应 Group；
同一片段多个条件为 OR，没有外部根的循环不会自行激活。

- `AtomUI.Toolchain/Backends/ILLink8` 扩展实际 MarkStep，将真实类型相关事件映射到已请求 Group，选择后的 thunk
  通过正常 MarkMethod 加入同一闭包。完成标记后物化调用，随后执行正常 Sweep。
- `AtomUI.Toolchain/Backends/ILC8` 基于定版官方源码构建托管宿主，在 scanner 和无 scanner 的实际依赖图中安装
  条件节点与固定 dispatch。输出表只引用已分析入口，保留正常 builder 参数和注册生命周期。
- `AtomUI.Toolchain/Common/Registration` 共享中立协议；其 `Cecil` 子目录负责实体绑定、身份和方法体模板校验，
  不拥有应用依赖分析。

上述源码由唯一 `AtomUI.Toolchain.csproj` 及其后端构建配方维护。ILLink8 与 ILLink10 仍选择不同 profile，
ILC8 的上游工程只在隔离缓存中生成；项目合并不共享引擎状态或降低选择精度。目录和产物契约见
[Toolchain 模块](../../modules/toolchain/overview.md)。

普通 Core getter 保持原语义。后端核验其 Core/BCL 身份与布尔模板后，仅在发布视图中折叠裁剪模式为 true。
未物化 selected 入口抛错；不能把失败变成空注册或完整注册。

ILC8 必须在 scanner 导入调用者前折叠该分支；只替换 getter 方法体会让保守扫描器继续遍历未裁剪注册分支。
适配器复用定版官方 `FeatureSwitchManager` 的常量替换、分支及异常处理逻辑，仅为已通过语义校验的精确 getter
提供 true 替换。维护者配方生成其源码副本，只将替换查找方法开放为 protected virtual 挂钩；上游受跟踪源码保持不变。
其他 feature switch 和 substitution 继续交给官方实现，预初始化屏障也使用同一处理后的 IL provider；不引入另一套可达性分析。

ILC8 的按需机制专项也使用实际 ConditionalRecord、输入格式 2 和独立 invocation，不再保留开发期 manifest
解析器。夹具先预编译两个包程序集，各自一个 Group，并保留 scanner/无 scanner、优化开关、OR 去重、
组门禁、静态构造器与异常传播、静态成员负例、未用工厂删除、原生表/符号和 .NET 8 实际运行检查。
泛型消费保持 `new GenericControl<Payload>()`，以真实泛型依赖闭包中的非泛型 `Payload` 为合法候选；
不扩展 format 1 的候选类型域，也不增加人为根。专项仍按需运行，不进入默认全局测试集合。

## SDK 接线与产物证据

托管裁剪与原生发布按框架及 `PublishTrimmed` / `PublishAot` 自动分流，不再需要 net8 专用选择开关。
现代框架只能选择官方引擎；完整规则见[框架分流](../../modules/toolchain/framework-routing.md)。
消费者使用包内工具，无需下载或构建 runtime 源码；工具宿主要求 .NET 10，最终应用仍使用官方 .NET 8 runtime。

发布输入格式 2 冻结最终实现 DLL、PDB、根规则、引用、自定义步骤及其声明的依赖、配置和工具。
实际引擎参数重定向到这些副本；记录 hash 后继续读取原路径不构成冻结。内容缓存可以共享，每次发布的报告、
linked 输出和验证状态独立。普通 build 不触发发布分析。

托管输出验证独立读取最终 DLL，确认 selected 调用序列、候选记录删除与输入身份。复制发布后再核对部署内容。
ReadyToRun 和单文件路径分别记录编译/封装输入与输出；单文件读取实际 bundle 目录和解压后的条目验证内容。
不能用 ILLink 阶段成功替代后续变换和复制成功。

原生编译器的托管 bundle 与 SDK 恢复的官方 native helper 合成受控工具域；runtime、CoreLib 和 native helper
保持官方版本。每次调用由 SDK 生成 GUID，object、exports、输入报告和 executable 放入该调用独立目录。
linked 阶段固定首次验证 hash，published 阶段核对调用身份、原始回执身份和实际发布副本，不能重新认证被改写的副本。
清理只处理本次拥有的路径，拒绝越界与符号链接跳转。当前每次启用后端的 publish 都重新编译，避免 SDK 时间戳漏算额外输入。

Windows 入口为隔离的 ASCII `.cmd` 与私有配置，不改变共享控制台代码页；SDK 参数仍交给正常 ILC/CppLinker。
冻结后的有效 ILC 参数通过调用域内独占创建的 UTF-8 响应文件交给官方解析器，避免完整产品引用集合超过 Windows
命令行上限；解析期间持有共享只读句柄并核对内容摘要。Windows 托管 linked 输出目录及 SDK 候选统一使用合法的扩展绝对路径，
使原有的存在性筛选、移除项分类、依赖清单和复制共用同一物理目录；不替换 SDK 各阶段，也不绕过部署内容验证。
空格、Unicode、`&` 和 `!` 路径有实际执行证据；含 `%` 的路径在 SDK 展开边界明确拒绝。
文件系统身份使用 Windows 忽略大小写比较，并仅归一化合法的扩展 DOS/UNC 前缀；符号、程序集身份和协议仍严格区分大小写。
实际编译器映射的 PE 必须逐字节匹配冻结文件，仅允许 Windows 页对齐产生的精确全零尾部。
最终 PE32+ 必须为 AMD64 原生 executable，精确 DATA export 的 RVA 和完整 nonce payload 均须匹配；
CLR header、转发导出、重复或越界导出不能作为证明。linked 与 published 回执独立且不可覆盖，复制失败保留已验证原件。

## .NET 10 消费 net8-only 包

现代发布根据实际输入自动探测旧 ABI；仅在需要时桥接，不再需要显式开关。已接线路径限定 SDK 10.0.300 的
net10.0/osx-arm64、net10.0/win-x64 普通 trimmed 发布与 NativeAOT，以及 net10.0-browser/browser-wasm。
Windows 桥接要求 Windows x64 宿主、目标 runtime10.0.14；其他组合仍由各自精确能力门禁控制。
Browser 当前要求 `WasmEnableWebcil=false`；桥接后的托管 ReadyToRun/单文件发布尚未开放，不能将 net8 后端的
变换验收结果移用于此路径。原生桥接目前只接单模块可执行程序。

桥接器在最终分析前将通过身份、摘要和方法体校验的候选 ABI 转成官方 TypeMap，使用现有 proxy 与 factory，
不计算哪些片段可达。源包、NuGet 缓存和原编译 DLL 不修改，副本保留 original → transformed 内容链。
框架引用重定向仅采用已核验 SDK 框架映射；第三方程序集仍要求完整身份一致。

旧 Core 也作为独立发布视图处理：按其原 net8 getter 的严格模板与四状态语义验证后，绑定已校验的目标 CoreLib10，
将发布副本中的裁剪模式规范化为 true。普通 build 与原包缓存保持原件；NativeAOT 转换清单把该 Core 纳入原件、转换副本、
引擎输入和部署内容链，托管 ILLink 路径要求其实际采用 Link action。现代 Core 仍使用官方 FeatureSwitchDefinition。

官方 ILLink、ILC10 和 Browser 各自消费转换后的输入。Browser 接在 TypeMap materializer 注入之后、签名捕获之前，
依次验证 linked、IL stripping / AOT 的最终结果及部署副本。纯 net10 快速路径不依赖 bridge 工具存在。

Windows ILC 的原生 PE 引用仅从原始 SDK `IlcReference` 集合准入，作为额外引用冻结并保留原元数据；
实际引擎必须消费精确冻结路径与 hash，不能回读缓存原件、缺项或混入外来引用。托管引用仍核对完整程序集身份；
`RuntimeList` 标成 Native 的 CoreLib 具有 CLR 元数据，因此按托管程序集验证。包 ID 使用 NuGet 的精确忽略大小写比较，
不按包名前缀放行。桥接 linked/published 证明采用分别创建的不可变 sidecar，发布复制还须匹配首次 linked 证明及原始输出。
Windows CLR 返回的合法扩展 DOS/UNC 加载路径与普通冻结路径按同一文件系统身份比较；实际加载的桥接程序集和 Cecil
仍须匹配精确冻结路径成员、完整程序集身份与内容摘要，不能用路径归一化放行其他副本或回读缓存原件。
现代原生桥接将冻结编译器目录以扩展路径交给原 SDK。版本验证选择真实 `ilc.exe`，长路径采用经实际验证的 SDK 命令边界，
仅执行固定 `--version` 并核对目标文件摘要；不依赖未设置的私有扩展名属性，不放宽版本检查，也不另寻短路径副本。
MSVC 对过长原生输出的限制不能仅靠扩展路径前缀解决。Windows 原生桥接的可变调用目录使用同一后端根下的
`runs/<GUID>`，冻结输入和工具仍在原指纹缓存；减少冗余父目录层级，同时保留独立调用、内容身份、越界检查和两阶段证明。
其他后端保留原目录规则，使用者的项目位置保持不变。
原生输出仍受本机 MSVC 的路径能力限制，本轮证据针对交付记录中的项目深度。

## 维护者打包

`scripts/registration/prepare-native8-compiler.py` 获取 `upstream.json` 中的定版官方源码，验证源码身份后构建
跨平台托管编译器 bundle。缓存按源码和本地适配器内容失效，带目录锁与原子替换。官方许可证和第三方声明随工具交付。
可用 `--source-root` 或 `ATOMUI_RUNTIME8_SOURCE` 复用已存在的精确源码；消费者不执行此步骤。

`BuildNuGetPackages.ps1` 与 Repository 的包准备入口在产品/Generator 收集包资产前准备完整工具集和 bundle。
Generator 直接 pack 也经过统一的 `AtomUIPrepareRepositoryPackageTools`，不在 Generator 项目复制另一套目标。
包校验使用唯一角色清单和实际 DLL/nuspec/工具内容，检查所有本地 Import。Release 双目标与 Debug 单目标分别验收；
历史 NuGet 版本与标签不因此重写。原生平台验证与普通打包成功必须分别报告。
