# .NET 8 条件注册后端

本文记录 .NET 8 条件依赖后端的实现、接线和交付边界。生成 ABI 的逐字段定义由
[ConditionalRecord 契约](../../reference/aot/conditional-registration-contract.md)拥有；注册生命周期继续遵守
[Control 注册契约](control-registration-contracts.md)。

## 状态与范围

产品库 Debug 为 net10.0，Release 为 net8.0 与 net10.0。普通非裁剪程序使用完整注册；裁剪程序必须由实际
ILLink/ILC 依赖图选择注册片段。禁止全包保留、源代码使用清单、独立近似调用图和 ILLink 预分析替代 ILC。

实现仍处于完整平台交付验收前。net8 发布按目标框架自动选择兼容后端，未知工具组合立即失败。目前的原生 SDK 接线限定
SDK 10.0.300、目标 runtime / ILC 8.0.27、macOS arm64 工具宿主，以及 osx-arm64 / osx-x64 完整应用目标。
Windows/Linux 原生消费和这些平台上的维护者编译器构建尚无验收证据，不能据本机成功宣布支持。
Browser 仍采用 .NET 10；不新增 net8 Browser 宿主。

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

## .NET 10 消费 net8-only 包

现代发布根据实际输入自动探测旧 ABI；仅在需要时桥接，不再需要显式开关。已接线路径限定 SDK 10.0.300 的
net10.0/osx-arm64 普通 trimmed 发布与 NativeAOT，以及 net10.0-browser/browser-wasm。
Browser 当前要求 `WasmEnableWebcil=false`；桥接后的托管 ReadyToRun/单文件发布尚未开放，不能将 net8 后端的
变换验收结果移用于此路径。原生桥接目前只接单模块可执行程序。

桥接器在最终分析前将通过身份、摘要和方法体校验的候选 ABI 转成官方 TypeMap，使用现有 proxy 与 factory，
不计算哪些片段可达。源包、NuGet 缓存和原编译 DLL 不修改，副本保留 original → transformed 内容链。
框架引用重定向仅采用已核验 SDK 框架映射；第三方程序集仍要求完整身份一致。

官方 ILLink、ILC10 和 Browser 各自消费转换后的输入。Browser 接在 TypeMap materializer 注入之后、签名捕获之前，
依次验证 linked、IL stripping / AOT 的最终结果及部署副本。纯 net10 快速路径不依赖 bridge 工具存在。

## 维护者打包

`scripts/registration/prepare-native8-compiler.py` 获取 `upstream.json` 中的定版官方源码，验证源码身份后构建
跨平台托管编译器 bundle。缓存按源码和本地适配器内容失效，带目录锁与原子替换。官方许可证和第三方声明随工具交付。
可用 `--source-root` 或 `ATOMUI_RUNTIME8_SOURCE` 复用已存在的精确源码；消费者不执行此步骤。

`BuildNuGetPackages.ps1` 与 Repository 的包准备入口在产品/Generator 收集包资产前准备完整工具集和 bundle。
Generator 直接 pack 也经过统一的 `AtomUIPrepareRepositoryPackageTools`，不在 Generator 项目复制另一套目标。
包校验使用唯一角色清单和实际 DLL/nuspec/工具内容，检查所有本地 Import。Release 双目标与 Debug 单目标分别验收；
历史 NuGet 版本与标签不因此重写。原生平台验证与普通打包成功必须分别报告。
