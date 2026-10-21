# 按目标框架选择注册后端

入口：[Toolchain 概览](overview.md)。本文拥有生成器与发布入口共同遵守的框架分流规则。
规则是：框架决定引擎家族，实际 API、工具和平台能力决定能否继续；能力不满足时明确失败。

## 框架规则

| 目标 | 编译期注册声明 | 裁剪/NativeAOT 发布 |
| --- | --- | --- |
| `.NETCoreApp 8.0` | ConditionalRecord 兼容 ABI | 自动选择对应的 ILLink8 或 ILC8 兼容后端 |
| `.NETCoreApp 10.0` 及以上 | 目标框架官方 TypeMap ABI | 使用与目标版本匹配的官方 ILLink/ILC；不进入 net8 引擎 |
| 其他框架或版本 | 有注册需求时诊断 `ATOMUIREG006` | 明确拒绝，不生成或执行兼容回退 |

普通非裁剪执行仍使用完整注册路径；这是正常运行模式，不是裁剪失败后的恢复手段。
框架分类不改变 Debug 只构建 net10、Release 构建 net8/net10 的产品矩阵。

SDK 入口使用解析后的框架身份和版本，正确处理 `net10.0-windows` 等平台后缀；不按短名称前缀猜测版本。
生成器读取 `TargetFramework`、`TargetFrameworkIdentifier` 和 `TargetFrameworkVersion`，检查声明是否一致。
独立 Roslyn 调用没有这些属性时，只能从实际 `System.Object` 所在的官方框架引用推断；不能通过“找不到 TypeMap”推断 net8。

## 生成与发布的两道校验

现代目标在生成前检查真正用到的官方 API：`TypeMap<TGroup>(string, Type, Type)`、
`TypeMapAssemblyTarget<TGroup>(string)` 和 `TypeMapping.GetOrCreateExternalTypeMapping<TGroup>()`。
框架身份、目标 major、可访问性、泛型约束、构造参数及返回签名必须满足生成调用；同名用户类型不能充当官方能力。
缺失或不兼容时报告 `ATOMUIREG006`，不发出 ConditionalRecord 来绕过问题。

net8 选择兼容 ABI，即使引用中出现同名 TypeMap 类型也不改变引擎家族。
既有控制片段、主题工厂、注册冻结和依赖分析算法不因分流而变化。

发布时校验实际工具，而非只看 SDK 名称。官方桌面 ILLink 的名称、公钥身份、程序集 major 和信息版本必须满足目标；
NativeAOT 编译器报告版本也必须匹配目标 major。版本还须不低于目标 major/minor 和 TypeMap 引入的 `10.0.0`。
因此 net11 不能借一个 10.x 编译器通过检查，net10 也不会因为“11 更高”而接受错误的目标工具版本。

现代 TypeMap 发布默认支持 .NET 10 及后续稳定版本，不以逐版本测试或 SDK/补丁白名单作为准入条件。
Browser 物化检查实际官方 ILLink、转换阶段和消费产物；WASM 工具版本跟随 SDK，不逐项比对其 targets 定义。
混合 ABI 转换继续保留独立的精确 SDK/runtime/平台门禁；不满足对应能力时失败，不改用 net8 后端。

## 旧包桥接独立于引擎选择

现代发布先探测实际输入的注册 ABI。纯官方包验证通过后不转换、不要求桥接工具；发现旧 net8 ConditionalRecord
时，才在能力允许的组合中转换副本。转换只调整 ABI，最终依赖闭包仍由目标版本的官方引擎计算。

Windows NativeAOT 的 SDK 引用集合也包含没有 CLR 元数据的原生 PE DLL。探测仅检查具有有效 CLR 元数据和
程序集 manifest 的输入；原生 PE 不参与注册协议探测，SDK 的原始输入集合保持不变。损坏的 PE、存在 CLR header
但缺少元数据，以及没有程序集 manifest 的托管模块仍明确失败，不能通过忽略解析异常来跳过旧包或错误输入。

探测按每个包自己的 TFA 与框架引用 major 验证官方 accessor。例如 net11 应用引用正常 net10 控件包时，
该包仍按其 net10 身份验证；不能把所有包一律当成应用的目标版本。没有记录、也没有合法官方协议的包明确失败。
转换的具体能力范围见 [.NET 8 条件后端](../../architecture/foundations/aot-net8-conditional-backends.md#net-10-消费-net8-only-包)。

`AtomUIEnableNet8ManagedRegistration`、`AtomUIEnableNet8NativeRegistration` 和
`AtomUIEnableConditionalRegistrationBridge` 不再控制路由，也无需设置。旧值不能使现代目标进入 net8 后端，
或让必须桥接的旧包跳过处理。应用继续使用正常 `PublishTrimmed`、`PublishAot` 等 SDK 发布参数。

## 实现入口与验证

- `Common/Registration/RegistrationFrameworkPolicy.cs`：Generator/worker 共用的中立框架分类。
- Generator 的 `RegistrationFrameworkResolver`：目标声明与实际官方符号能力校验。
- `build/AtomUI.Registration.Framework.targets`：集中 SDK 家族、模式选择与发布拒绝规则。
- `ValidateRegistrationToolchainTask`：实际官方工具与目标版本的匹配检查。
- `BridgeInputProbe`：只读输入协议探测，不进行依赖选择。

框架矩阵要覆盖 net8、未支持版本、现代版本、平台后缀、旧开关和未知能力的拒绝。
未来版本的策略/PE 模拟只证明分流逻辑；发布支持仍需相应 SDK、参考程序集和真实引擎产物的独立证据。
共享模型与 SourceGenerator 检查不能代替实际发布、运行及未用片段删除检查。
