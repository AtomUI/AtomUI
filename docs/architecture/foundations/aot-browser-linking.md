# 浏览器链接架构

本文定义 AtomUI Mono 运行时（Browser 与 iOS/tvOS/Mac Catalyst/Android）的 TypeMap 编译期转换和发布边界。
Browser 是首个接入的 Mono 宿主，第 1–7 节以它为基准；其他 Mono 宿主的接入差异见[第 8 节](#8-其他-mono-宿主)。本地实现、产品验收与发布状态由
[AOT 与裁剪架构](aot-and-trimming.md#1-状态与事实边界)统一说明。
工具实现边界见 [TypeMap 链接工具](../../modules/typemap-linker/overview.md)，标记与版本格式见
[TypeMap ABI 契约](../../reference/aot/typemap-contract.md)。

## 1. 必需的发布后端

Browser trimmed interpreter 与 Browser AOT 必须自动精细选择注册片段。选定的 Mono 产品后端不依赖运行时 TypeMapping
查询，而是在官方 ILLink 完成 TypeMap 标记后，将 AtomUI 自己的 map accessor 转换为已选静态映射。

Mono 运行时不实现 `TypeMapping.GetOrCreateExternalTypeMapping<TGroup>()`，因此任何 Mono 宿主只要打开裁剪注册开关，
就必须同时接入此后端。这是构建期能力，不是浏览器运行时反射。后端不扫描裁剪残留 Attribute，不自行计算依赖闭包，不修改 BCL，也不读取上游
内部 TypeMapHandler。官方链接器负责“保留哪些类型”，AtomUI 工具只负责“把已确定的自有映射转换为可执行代码”。

custom-step 扩展入口不等于跨版本稳定 ABI。工具链支持范围必须由构建资产检查，并用真实 Browser 解释与 AOT 执行验证。
工具缺失或不兼容时发布失败，不转为全包注册或跳过转换。普通非裁剪 Browser Debug 使用同一事实源的完整片段调用，
这是正常模式，不是发布回退。

## 2. 生成 accessor 与 helper

每个包生成一个内部、静态、非泛型、返回 `IReadOnlyDictionary<string, Type>` 的 accessor。原始方法体直接调用本包的
具体 Group，使用 `GeneratedTypeMapAccessorAttribute(group)` 标记：

```csharp
[GeneratedTypeMapAccessor(typeof(AcmeMap))]
[DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods, typeof(AcmeMap.BrowserMap))]
internal static IReadOnlyDictionary<string, Type> GetTypeMap()
    => TypeMapping.GetOrCreateExternalTypeMapping<AcmeMap>();
```

示意中的 helper 名称不构成用户 API；精确生成形态由隐藏 ABI 管理。
Group 内部的 BrowserMap 是已知的非泛型闭合 helper，只创建和填充 string→Type 字典，不引用任何具体 Control 或代理。
helper 及其传递实现必须在 Mark 前通过静态证据保留，转换器不能在 Mark 后引入尚未分析的新依赖。

NativeAOT/CoreCLR 不加载该转换器，accessor 继续使用官方 TypeMap。
共享 runtime 只接受已获取的 dictionary，不在泛型 `ReadMap<TGroup>()` 中查询 TypeMap。

## 3. 转换阶段与算法边界

转换步骤位于官方 MarkStep 完成后、Sweep 之前。后端只处理已解析、带
`ControlPackageMarkerAttribute(packageId, mapGroup, abiVersion)` 的包及其已可达自有 accessor：

1. 验证工具链、执行阶段、Package marker、accessor 与 helper ABI。
2. 以完整程序集/类型身份确定 accessor 的具体 Group，不只比较 FullName 或 scope 简称。
3. 读取该 Group 中由官方 `Annotations.IsMarked` 标记的 TypeMap 声明。
4. 验证每个选中目标与所需 helper 均已保留，检查重复 key、身份冲突和异常程序集状态。
5. 按确定顺序重写自有 accessor，只引用当前已选目标。
6. 清理旧方法体的局部变量、异常区间与调试序列点，写入转换完成标识。
7. 后置检查所有可达自有 accessor 均已转换，且不再调用 Mono 运行时 TypeMapping。

转换结果等价于：

```csharp
var map = AcmeMap.BrowserMap.Create();
AcmeMap.BrowserMap.Add(map, "dependency", typeof(DependencyRegistration).TypeHandle);
AcmeMap.BrowserMap.Add(map, "used", typeof(UsedRegistration).TypeHandle);
return AcmeMap.BrowserMap.Complete(map);
```

不能把全部候选代理写入新方法，也不能因为一个 key 命中就把同包其他代理保留。未用类型仍由后续 Sweep 删除。
片段与资源闭包仍由官方标记结果决定；此步骤不读取应用 C#/AXAML、不遍历消费方法体，也不运行另一轮 linker。

空应用或未启用包可以合法地没有可达 accessor。门禁必须区分“没有待转换 accessor”与“存在待转换 accessor 但步骤没有执行”，
不能仅凭零转换数量判成功或判失败。目标部署程序集必须接受实际链接处理；copy/skip 状态不能伪装为成功转换。

## 4. 构建顺序与自动接入

新注册构建资产只负责模式选择、feature switch、工具加载、增量输入与产物门禁。
Browser AOT 的固定顺序为：

```text
普通编译
→ 官方 ILLink 标记
→ AtomUI 已选映射转换
→ Sweep 与链接产物验证
→ Mono AOT
→ Browser bundling
```

不能在 Mono AOT 后修改 IL，也不能以未裁剪程序集替换已经验证的链接产物。
只在 Browser/Mono 的真实裁剪发布加载转换工具；普通构建不得承担发布后端成本。

工具随产品 NuGet 的 buildTransitive 资产自动交付，用户不手配额外 trimmer 参数。
多个产品包携带的同版本工具只注入一次；版本不兼容时明确失败，不允许多个转换器顺序改写同一 accessor。
不向 ProjectReference 传播旧 linked context，不要求普通消费类库额外生成发布清单。

## 5. 增量与工具链契约

custom-step DLL、其依赖、配置和 ABI 版本全部进入链接增量输入。不能只依赖 SDK 默认项目输入判断链接产物可复用。
后端工具内容或配置变化必须重新链接；输入未变化的 no-op 发布应复用已验证产物。
这些内容证据继续使用逐字节 SHA-256；实现以流读取文件，不通过时间戳或长度替代内容校验。单次后端执行可复用分阶段的
Cecil 类型/方法索引，但 Sweep 后和输出重读必须建立独立索引，不能让 Sweep 前对象替代最终产物验证。

TypeMap 从 .NET 10 引入，默认支持该版本及后续稳定版本，不维护 SDK、补丁、完整构建号或上游参数定义白名单。
WASM 工作负载和运行时按 SDK 的默认规则匹配；AtomUI 验证实际转换阶段、官方标记结果和自有 accessor/helper ABI。
签名与回执绑定实际工具身份、版本和文件内容，工具升级会触发重新链接。
独立的旧包 ABI 桥接按其自身工具链准入规则验证。

真实转换失败、ABI 不匹配或产物验证失败时停止发布并给出可定位诊断，不恢复全包模式、运行时 Attribute 扫描或旧控件注册分析。
验证覆盖最低基线与当前稳定版本的代表性发布、缺失工具负例、增量重新链接及真实浏览器执行；
无需在使用新的 SDK 版本前逐一验收或把未单独测试作为拒绝发布的条件。

## 6. 失败语义与产物边界

以下情况均不能生成被标记为成功的发布产物：

- 缺失或损坏转换器，工具/marker/accessor/helper ABI 不兼容。
- helper 或映射目标未被官方标记，出现跨程序集身份冲突或重复 key。
- 转换阶段错误、不可处理的程序集 action、残留可达 accessor。
- 已转换 accessor 仍调用 Mono TypeMapping，或转换完成标识与产物不一致。
- 使用陈旧缓存绕过本次工具链或后端输入检查。

诊断只检查自有生成 ABI，不扩展为任意应用方法体的使用分析。
`AtomUI.TypeMap.Linker.dll` 由统一 Toolchain 项目的 ILLink10 profile 产出；它与普通 Generator、Tasks worker
以及 ILLink8 的编译依赖、输出和执行宿主保持隔离。其 DLL、依赖和配置属于构建工具，
不得进入产品 lib/runtime 依赖、应用输出或发布目录。

## 7. 验证矩阵

两个浏览器执行模式均须通过相同的真实产品契约：

- 已用 Control、间接模板/Token 依赖与 Semantic Part 正常注册，unused Control/proxy/factory 缺失。
- 跨程序集 target、传递 target、多 Group、同代理多条件、空 map、保留循环均正确。
- 可选包未启用不创建 Provider 或执行 initializer。
- 内部 presenter、命名主题、多目标资产、host DynamicResource 与主题切换正确。
- 缺失转换器、ABI 不匹配、缺 helper、残留 accessor、copy/skip 状态均有失败对照。
- 后端 DLL/依赖/配置变化使链接失效，未变化时不重复发布工作。
- 真正 AOT 构建证据与浏览器运行证据同时存在；仅能打开页面或解释执行成功不代表 AOT 验收通过。

必须检查最终部署程序集与资源保留结果，不能只检查运行日志中的注册数量。
单程序集机制验证不替代 NuGet 冷消费、完整 Browser Gallery 和跨平台产品验收。
总体发布与旧机制退役门槛见 [AOT 与裁剪架构](aot-and-trimming.md#8-验证与交付门槛)。

## 8. 其他 Mono 宿主

iOS、tvOS、Mac Catalyst 与 Android 默认使用 Mono 运行时，链接链路同样是“官方 ILLink（带平台 SDK 自己的 custom
steps）→ Mono AOT/JIT”。生成器对所有目标都输出同一 BrowserMap helper，后端按 Group 解析，与平台无关；差异只在
构建接入。

### 8.1 接入判定

构建资产在目标执行期统一计算“Mono 后端是否接入”，因为 `UseMonoRuntime`、`TrimMode` 与 `PublishTrimmed` 由平台 SDK
在 props 或目标中设置，不能在 AtomUI props 阶段求值：

| 条件 | Browser | Apple（ios/tvos/maccatalyst）与 Android |
| --- | --- | --- |
| Mono 宿主识别 | `RuntimeIdentifier == browser-wasm` | `UseMonoRuntime == true` 且目标平台匹配 |
| 后端接入 | `PublishTrimmed == true` | `PublishTrimmed == true`、`RunILLink != false` 且 `TrimMode != copy` |
| 接入后的准入错误 | 非 .NET 10 官方 TypeMap、`TrimMode != full` 或未真正运行 ILLink | 非 .NET 10 官方 TypeMap 或未真正运行 ILLink |

`AtomUI.Registration.Trimmed=true` 只在选择确实发生时写入：非 Mono 目标（CoreCLR、NativeAOT，包括 iOS NativeAOT）
维持官方 TypeMap 路径；Mono 目标只在后端接入时写入。iOS SDK 对 Mono 每次构建都运行 ILLink，Debug 也不例外，
所以不能用“ILLink 将要运行”代替“选择已经发生”。`TrimMode=copy`（x64 模拟器、`MtouchInterpreter` 等 iOS `None`
链接模式）不裁剪任何程序集，属于普通非裁剪模式，直接收集全部片段，不是发布回退。

桌面 ILLink/ILC 工具链校验只用于官方 TypeMap 路径；所有 Mono 宿主由后端链接任务校验实际 ILLink 身份。

### 8.2 partial 链接

AtomUI 产品程序集默认 `IsTrimmable=true`，所以 iOS 默认的 `SdkOnly`（`TrimMode=partial`）也会链接它们，选择可以发生，
不要求用户改为 `full`。后端按每个承载可达 accessor 的程序集的实际 action 校验：必须为 Link，copy/skip 状态报错，
不以全局 `TrimMode` 推断。第三方包（包括 AtomUI.Mobile.Controls 这类下游控件包）若携带 AtomUI 注册片段，必须同样
声明为可裁剪；否则 partial 发布失败，而不是回退为全量注册。

官方 ILLink 只从 action 为 Link 或 CopyUsed 的 `EntryPoint` 根程序集开始解析 `TypeMapAssemblyTarget`；Copy 状态的
入口程序集不会登记为 TypeMap 入口，所有 Group 都不会标记任何条目。partial 模式下不可裁剪的应用程序集正是 Copy，
因此构建资产在后端接入且入口程序集原本为 Copy 时，把它改为 CopyUsed：应用代码仍整体保留，只恢复 TypeMap 入口。
已显式设置 Link 或其他 action 的入口程序集保持不变。后端另行校验：存在可达 accessor 而 ILLink 没有 TypeMap
入口程序集时报错，不能把“TypeMap 未解析”物化成空映射。

### 8.3 与平台 custom steps 共存

平台 SDK 也通过 `_TrimmerCustomSteps` 注入步骤。iOS 在 MarkStep 之前注入 `CoreTypeMapStep`、`ManagedRegistrarStep`
等预标记步骤，Sweep 之前注入 `ManagedRegistrarLookupTablesStep`，其余步骤在 Sweep 之后或输出之前。AtomUI 转换器仍以
`AfterStep="MarkStep"` 注入：它只改写自有 accessor，不引入新依赖，也不读取平台步骤的状态；平台在 Mark 与 Sweep 之间的
步骤只处理 NSObject/registrar 映射，不与 AtomUI 的已选映射交叉。

平台 SDK 提供给自身 custom steps 的不透明配置文件（iOS 的 `custom-linker-options.txt`）进入链接增量输入；
`MtouchLink`、`MtouchInterpreter`、`AndroidLinkMode` 进入链接签名配置，避免平台链接模式变化后复用陈旧回执。

### 8.4 产物消费校验

链接输出校验（`_RunILLink` 之后）与 Browser 相同。消费校验必须挂在平台读取链接产物之前、任何 IL 剥离或 AOT 之前：

| 平台 | 消费校验挂接点 | 被校验的程序集集合 | 状态 |
| --- | --- | --- | --- |
| Browser | `_WasmAotCompileApp;_WasmGenerateAppBundle` 之前 | `_WasmAssembliesInternal` | 已实现 |
| Apple 打包输入 | `_ComputeStripAssemblyIL` 之前（`ComputeFilesToPublish` 之后） | `ResolvedFileToPublish` 中的非卫星 `.dll` | 已实现 |
| Apple Mono AOT 输入 | `_AOTCompile` 之前，仅 `_RunAotCompiler == true` | `_AssembliesToAOT` | 已实现 |
| Android | Mono AOT 与打包之前，目标名待在 Microsoft.Android.Sdk 中确认 | 待确认 | 未实现 |

iOS arm64 模拟器 Debug 也运行 Mono AOT（dedup 模式），只有 x64 模拟器与 macOS 不运行；没有 AOT 时仍由打包输入
校验覆盖。Android 已接入转换与链接输出校验，但消费校验挂接点和真实运行均未验证，不能视为已验收平台。

### 8.5 验证矩阵

在第 7 节的产品契约之外，Mono 移动宿主至少覆盖：

1. iOS arm64 模拟器 Debug（partial）与 Release：应用启动；`AtomUI.TypeMap.receipt.json` 存在且与链接产物绑定；
   最终程序集中已转换的 accessor 不再调用 `TypeMapping`。
2. 用到的控件（含模板间接依赖、Token、Semantic Part）正常注册；未用控件的代理与工厂不在最终程序集中。
3. iOS x64 模拟器或 `MtouchInterpreter`（`TrimMode=copy`）：`Trimmed` 未打开，走全量收集并能启动。
4. 负例：缺少后端、ABI 不匹配、承载 accessor 的程序集未链接、入口程序集未作为 TypeMap 入口、后端未执行，构建都会失败，
   而不是生成可运行的全量产物或空映射。
5. Android 安装 SDK 后按同一矩阵单独验证，iOS 结论不代表 Android。
