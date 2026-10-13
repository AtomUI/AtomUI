# AtomUI.Toolchain 模块概览

`AtomUI.Toolchain` 统一维护 AtomUI 的构建任务、公共编译期模型、AOT/裁剪后端和工具准备配方。
唯一源码工程是 `src/AtomUI.Toolchain/AtomUI.Toolchain.csproj`；构建产物服务于开发、发布和打包，不进入应用运行时依赖。

该模块解决的是构建工具的职责和交付分散问题。它把普通编译所需的 worker 与发布时才需要的引擎适配区分开，
在一个工程中管理源码和构建入口，同时保留真实依赖分析所需的版本隔离。产品库 Debug 仅 net10.0、Release 同时
包含 net8.0/net10.0 的规则由[构建与打包架构](../../architecture/foundations/build-and-packaging.md)拥有。

## 模块边界

- `AtomUI.Generator` 保持独立的 netstandard2.0 Roslyn Analyzer 身份，负责生成应用/类库编译所需的代码。
- Toolchain 的 Tasks worker 负责构建文件、发布输入和产物验证；引擎适配器负责对应 ILLink/ILC 阶段的接入。
- `Common` 提供共同的数据格式和校验逻辑；应用可达性仍由实际 ILLink/ILC 计算。
- Repository 构建入口准备和分发工具；NuGet 消费应用使用包内工具，不构建编译器源码。

包内 `AtomUIThemeDefinition` 由 Tasks worker 在编译前以权威 Reader/XSD 校验并生成中立 AdditionalFile，Generator
据 Token 契约生成类型化主题程序。Core 的 syntax-document 特例已由此共同路径替代，原 XML 仍可作为 AvaloniaResource
显式读取。Tasks 只源链接 Core 中依赖 BCL 的 Reader/XSD 及必要语法模型，校验有界原始 XML；私有版本 3
中间输入只传输声明文本、位置、摘要与诊断。Tasks 不执行主题算法或复制运行时配置/快照模型；静态字面量解析与
C# 生成归 Generator。产物身份与校验见[编译主题包](../../architecture/systems/theming/compiled-theme-packages/compilation.md)。

`GenerateThemeAssetWrappersTask` 保留编译型资源与 deferred wrapper 输出，作为生成器不能直接静态构造时的 fallback；
不重写作者的 classless AXAML，不改变 AvaloniaXaml/AdditionalFiles 的 URI、Link 或源身份。Generator 可为已绑定且
可合法构造的作者字典、精确默认 typed theme 选择直接或 `AddDeferred` 工厂；此选择属于生成器，不由任务猜测 C#
构造函数可访问性。字典与主题的延迟语义见[控件注册契约](../../architecture/foundations/control-registration-contracts.md)。

## 源码职责

以下路径均相对于 `src/AtomUI.Toolchain/`：

| 目录 | 职责 |
| --- | --- |
| `Tasks/` | 隔离任务进程、资源与语言包任务、工具解析、SDK 输入准备和最终产物校验 |
| `Common/Registration/` | 候选记录读取、严格协议、内容身份、输入快照和调用域校验 |
| `Common/Registration/Cecil/` | Cecil 实体绑定、类型转发、方法体模板和 Core 发布视图校验 |
| `Common/ThemeDefinitions/` | XML 声明与诊断的私有构建传输，供 Tasks 与 Generator 协作 |
| `Common/Localization/` | 与 Generator 共用的 XLIFF、语言包和中立诊断模型 |
| `Common/SourceGeneration/` | 构建期代码身份与命名等中立辅助逻辑 |
| `Backends/ILLink8/` | 在真实 ILLink8 Mark 闭包中选择片段，Sweep 前物化选中调用 |
| `Backends/ILLink10/` | Browser TypeMap 物化及 net8-record 到官方 TypeMap 的分析前桥接 |
| `Backends/ILC8/` | 定版 ILC8 条件依赖适配、原生输出证明、维护者构建配方与专项入口 |

## 正式文档导航

| 文档 | 所有权 |
| --- | --- |
| [设计思路与职责边界](design.md) | 为什么建设 Toolchain、单工程多产物的理由、Generator 边界、Avalonia 借鉴和保精度约束 |
| [按目标框架选择注册后端](framework-routing.md) | net8/现代框架分流、实际能力门禁及独立的旧包 ABI 桥接 |
| [构建、交付与验证](build-and-verification.md) | Tasks/ILLink/ILC 构建配置、构建命令、输出隔离、工具准备时序、维护入口与验证要求 |
| [构建与打包架构](../../architecture/foundations/build-and-packaging.md) | 跨项目 Repository/NuGet 接线、唯一资产白名单和运行时隔离 |
| [ILLink10 后端详解](../typemap-linker/overview.md) | Browser 标记后物化、工具身份和回执 |
| [.NET 8 条件后端](../../architecture/foundations/aot-net8-conditional-backends.md) | ILLink8/ILC8 机制、SDK 接线与现行支持范围 |
| [条件注册协议](../../reference/aot/conditional-registration-contract.md) | ConditionalRecord ABI、输入快照和输出协议 |
| [本地化构建](../../architecture/systems/localization/generation-and-build.md) | XLIFF、静态语言包和 Generator/Tasks 的语义分工 |
| [编译主题包](../../architecture/systems/theming/compiled-theme-packages/compilation.md) | 有界 XML 校验、声明传输、产物身份与校验 |

构建成功、单元测试通过和特定平台的实际发布是不同层次的证据。Toolchain 的工程组织不扩大既有平台支持；
当前能力边界由 [AOT 与裁剪架构](../../architecture/foundations/aot-and-trimming.md)及各后端文档记录。
