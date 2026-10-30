# TypeMap 生成 ABI

> 本文对应本地源码已实现的生成 ABI v1，不表示已发布版本或全部交付门槛已通过。
> 状态由 [AOT 与裁剪架构](../../architecture/foundations/aot-and-trimming.md#1-状态与事实边界)维护。

本契约连接普通生成器、Core 注册代码与 Browser 链接后端。它是工具生成接口，不是应用或控件包作者手写的扩展点。
系统行为由 [TypeMap 注册体系](../../architecture/foundations/aot-typemap-registration.md)和
[控件注册契约](../../architecture/foundations/control-registration-contracts.md)定义。

已实现的[资源平台声明收敛](../../architecture/foundations/aot-typemap-registration.md#7-平台分支)只调整普通生成器的
编译期事实来源；继续生成现有片段 guard，不改变本 ABI v1、Package marker、TypeMap key、资源 descriptor
或 contract fingerprint 格式。资源 CLR 平台声明不是应用需要填写的新注册参数，也不是 Browser 后端的新协议。

## 1. 包 marker

固定名称与构造参数为：

```csharp
ControlPackageMarkerAttribute(string packageId, Type mapGroup, int abiVersion)
```

普通生成器将 marker 写入控件包程序集。它包含：

| 参数 | 约束 |
| --- | --- |
| `packageId` | 非空、稳定的普通包资源身份；不编码入口方法或裁剪分组 |
| `mapGroup` | 本包唯一的具体 Group 类型，允许消费程序集的生成代码引用 |
| `abiVersion` | 普通生成器、Core 与后端共同识别的 ABI 版本；未知或不匹配时失败 |

Group 为隐藏公共类型，仅用于跨程序集生成代码引用，不含引用全包 descriptor、代理或 factory 的静态字段。
marker 不包含控件列表、入口方法名或应用使用记录，不产生任何启用副作用。

消费生成器从已解析程序集读取 marker，验证完整程序集身份、包身份与 Group 的对应关系，确定性去重后输出具体 Group 的
`TypeMapAssemblyTarget`。相同包身份对应不相容声明时失败，不能按路径或发现先后选择输入。
应用方法体、adapter DLL 方法体和编译型 AXAML 使用记录都不是此引导的输入。

## 2. 条件映射和 key

每条条件映射关联一个具体 Group、一个字符串 key、一个代理类型和一个真实 `trimTarget` 类型。
逻辑片段可以具有多个条件，但每个条件必须有独立 key；同 key 多条件不是受支持的 OR 表达。

key 由生成器把完整 Group 身份、Control 程序集限定身份和条件类型身份输入稳定 hash，编码为
`v1:<16 位十六进制>`。生成文本不重复携带完整程序集限定名，也不写绝对路径、机器信息或扫描序号。
同一 Group 内生成器同时保存短 key 到完整输入的映射；不同完整输入发生短 key 碰撞时报告 `ATOMUIREG005` 并停止生成，
不允许覆盖或依赖发现顺序。用户不填写或解析 key。

key 内容对 Core、官方 TypeMap 和 Browser 后端都是不透明字符串：attribute 与候选表由同一次包编译共同产生，后端只验证
同一 Group 内的字符串唯一性并原样写入 accessor。因此这次短 key 编码不改变 ABI v1 marker、accessor 或 helper 签名；
旧预编译包可以继续携带自己的长 key，新旧包不共享 Group dictionary。

候选 key 表只包含字符串。运行时只使用 `TryGetValue` 查询，不使用 TypeMap dictionary 的枚举、Count、Keys 或 Values。
先按代理 Type 去重，再激活片段，避免多个条件命中时重复创建 Attribute。
候选表不得包含 Control Type、代理实例或 factory delegate，以免全量保留。

条件来自实际契约，包括 Control、TokenResourceExtension、TokenKey、OwnTokenKind 和生成的专用 Semantic Style。
包身份/marker/候选字符串本身不能代替这些保留条件。

## 3. 片段代理

Core 的隐藏生成 ABI 采用以下形态：

```csharp
public abstract class ControlRegistrationFragmentAttribute : Attribute
{
    public abstract string FragmentId { get; }
    public abstract void Add(ControlPackageRegistrationBuilder builder);
}
```

片段在启动提交时由 Core 统一执行，不分阶段；net8 固定 collector 模板保持不变。

代理是带自身 Attribute 的 sealed 派生类型。查询到目标 Type 后，注册代码读取已知基类的 Attribute；
不对未知类型进行构造函数搜索或运行时程序集发现。

`FragmentId` 是包内逻辑片段身份，由包身份和真实控件/资源契约确定，不是目录、控件族或注册顺序。
同一片段的所有条件使用同一代理；同身份对应不相容内容必须失败，不能用去重掩盖冲突。

`Add` 只收集可选 Control/Token descriptor、Semantic descriptor 与资产引用，不调用其他片段，不创建控件，
不执行资源 factory，不执行包 initializer。多个片段引用同一资产通过 AssetId 去重；资产定义冲突仍须报告。
平台不可用分支必须在引用/构造 descriptor 和 factory 前退出，保留链接器可见的平台边界。

## 4. 映射 accessor

固定识别标记为：

```csharp
GeneratedTypeMapAccessorAttribute(Type group)
```

每个包的 accessor 必须内部、静态、非泛型、无参数，返回 `IReadOnlyDictionary<string, Type>`，
marker 的 Group 与方法体使用的具体 Group 一致。原始方法体直接调用
`TypeMapping.GetOrCreateExternalTypeMapping<ConcreteGroup>()`。
共享运行时代码接收已经取得的 dictionary；不能将查询藏进 `ReadMap<TGroup>()` 泛型封装。

ABI v1 的 helper 必须是 Group 自己的 `internal static BrowserMap` 嵌套类：

```csharp
public static Dictionary<string, Type> Create();
public static void Add(Dictionary<string, Type> map, string key, RuntimeTypeHandle handle);
[MethodImpl(MethodImplOptions.NoInlining)]
public static IReadOnlyDictionary<string, Type> Complete(Dictionary<string, Type> map);
```

`Create` 使用 `StringComparer.Ordinal`，`Add` 由 handle 取得 Type；`Complete` 保留独立调用边界。
accessor 以 `DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods, typeof(Group.BrowserMap))`
在 Mark 前保留三者及传递依赖。helper 不引用具体控件或代理；转换器不能在 Mark 后补入未知代码。

NativeAOT/CoreCLR 执行官方 accessor。Browser 后端在官方标记完成后、Sweep 前将其转换为仅含已选条目的实现，
并验证可达 accessor 全部转换。后端阶段、转换状态和产物检查见
[TypeMap Linker 模块](../../modules/typemap-linker/overview.md)。

## 5. 包注册 helper

生成命名空间中的内部 helper 签名如下；包作者在同一次编译中调用它，不手写或分发这些声明：

```csharp
internal static IAtomUIBuilder Register(
    IAtomUIBuilder builder,
    Func<IControlThemesProvider> createProvider,
    Action<IAtomUIBuilder>? prepare = null,
    Action<IAtomUIBuilder>? complete = null);
```

内部基础包依赖另有同签名 `Ensure`，已成功注册的包不重复执行；公共入口使用 `Register`。调用示例：

```csharp
GeneratedControlPackageRegistration.Register(
    builder,
    static () => new AcmeControlThemesProvider(),
    prepare: PreparePackageCore,
    complete: CompletePackageCore);
```

其契约是 provider factory 与显式生命周期顺序，而不是入口分析：

```text
builder 注册状态检查 → prepare → 创建 provider / 平台选择
→ 收集公共核心与选中片段 → 包内结构校验与暂存 → complete
→ 全部包入口完成后跨包校验、资源排序/挂载与冻结
```

同一 builder 的重复公共包入口及递归进入，在执行 prepare 前失败；内部基础包 Ensure 与公共 Use 区分。
失败的 builder 不能继续构建部分成功的应用，不同 builder 互相独立。
包暂存不要求尚未提交的其他包已经存在，跨包契约在全部入口完成后校验。

元数据引导、映射查询结果或可选包引用不替代调用入口。注册 helper 不自动启用已发现的其他包。
注册状态放在 builder 实例，不能建立持有应用/provider 的全局缓存。

## 6. 类型与资产身份

生成 ABI 使用完整程序集/类型身份验证符号，不能仅比较类型 FullName 或 scope 简称。
生成类型名和 FragmentId 的确定性不应依赖被选中控件数量或源码遍历顺序。

资源记录包含稳定 AssetId、Phase、Order、静态 factory 和契约元数据。真实 PackageCommitOrdinal 保持包提交顺序；
包内按 `(Phase, Order, AssetId)` 排序，显式嵌套包含的顺序保留在 factory 内。
TypeMap key 顺序和字典顺序不具有资源优先级含义。

Token identity 的 owner 数据、字符串相等语义及去重前 canonicalization 由
[控件注册契约](../../architecture/foundations/control-registration-contracts.md)所有，不能由 ABI reader 自行简化。
schema fingerprint 继续验证业务 schema，不与生成符号或 backend 版本混用。

隐藏的生成资产物化 ABI 为：

```csharp
[EditorBrowsable(EditorBrowsableState.Never)]
public static ControlThemeAssetDescriptor CreateGenerated(
    string assetId,
    Uri assetUri,
    IEnumerable<ControlThemeExportDescriptor> exportedThemes,
    IEnumerable<ControlTokenIdentity> requiredTokenOwners,
    IEnumerable<ControlThemeBindingDescriptor> semanticThemeBindings,
    IReadOnlyList<string> compiledGlobalTokenNames,
    ulong compiledContractFingerprint);
```

该方法位于 `AtomUI.Theme.Schema.ControlThemeAssetDescriptor`。它先核对带独立 domain/format 的编译期业务证据，
再以实际 Type 的完整运行时身份及编译期 global 名称快照物化最终指纹；实际 registry 的验证保持独立。
完整规则属于 [控件注册契约](../../architecture/foundations/control-registration-contracts.md)。
参数中的 Type 只来自当前选中资产的正常 `typeof` 引用，不允许共享 Type/factory 表或运行时程序集发现。

## 7. 失败与兼容边界

未知 ABI、非法 Group、不可访问类型、冲突 key/identity、错误资源依赖、缺失 helper、未知后端或未转换 accessor 均须明确失败。
诊断采用统一 `REG` 域及语义错误名称：`AmbiguousThemeExport`、`InvalidTokenOwner`、`InaccessibleRegistrationType`、
`InvalidResourceDependency`、`RegistrationIdentityConflict`、`UnsupportedRegistrationBackend`、`UnloweredTypeMapAccessor`。
以上语义依次对应 `ATOMUIREG001`–`ATOMUIREG007`；精确触发与 owner 见
[编译器诊断注册表](../../engineering/development/compiler-diagnostics-guidelines.md#typemap-注册诊断)。
ILLink 的公开诊断 API 使用 `IL6505`–`IL6507` 传输编号，消息保留相应 `ATOMUIREG005`–`ATOMUIREG007`。

用户与包作者不手写 TypeMap、marker、accessor 标记或后端参数。采用新 SDK 的控制包应重新构建以携带对应生成 ABI；
普通预编译 consumer/adapter 不要求使用记录，只需其引用的控制包可正确解析。
动态类型名与发布后插件仍遵守 .NET 的 AOT 边界，不能通过协议错误时保留全包来伪装自动精细裁剪。

## 8. 交付检查

包内携带编译后的映射/marker，普通生成器和 Browser 后端通过构建资产自动提供。构建工具不进入应用运行时输出。
发布必须证明 full 分支没有意外保留全包，并分别验证 Desktop NativeAOT、trimmed CoreCLR、Browser trimmed interpreter
与 Browser AOT。生成接口的源码快照仅证明 ABI 形状，不能替代链接产物与真实启动验证。
