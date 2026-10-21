# 浏览器链接架构

本文定义 AtomUI Browser/Mono 的 TypeMap 编译期转换和发布边界。本地实现、产品验收与发布状态由
[AOT 与裁剪架构](aot-and-trimming.md#1-状态与事实边界)统一说明。
工具实现边界见 [TypeMap 链接工具](../../modules/typemap-linker/overview.md)，标记与版本格式见
[TypeMap ABI 契约](../../reference/aot/typemap-contract.md)。

## 1. 必需的发布后端

Browser trimmed interpreter 与 Browser AOT 必须自动精细选择注册片段。选定的 Mono 产品后端不依赖运行时 TypeMapping
查询，而是在官方 ILLink 完成 TypeMap 标记后，将 AtomUI 自己的 map accessor 转换为已选静态映射。

这是构建期能力，不是浏览器运行时反射。后端不扫描裁剪残留 Attribute，不自行计算依赖闭包，不修改 BCL，也不读取上游
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
