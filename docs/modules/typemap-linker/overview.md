# AtomUI.Toolchain 的 ILLink10 后端

> 后端、产品源码/buildTransitive 自动接线及冷 NuGet 消费已在本地实现和验证；不表示已发布或全部平台已验收。总体状态见
> [AOT 与裁剪架构](../../architecture/foundations/aot-and-trimming.md#1-状态与事实边界)。

本页详解统一 [Toolchain 项目](../toolchain/overview.md)的 `Backends/ILLink10`。其内部产物仍名为
`AtomUI.TypeMap.Linker.dll`，在 Browser/Mono 裁剪发布的独立链接宿主中执行。它在官方 ILLink 完成条件类型映射的标记后，
把 AtomUI 自有 map accessor 转换为仅含已选目标的静态映射。Browser 的 trimmed interpreter 与 AOT 都必须使用此后端，
保持与 Desktop 相同的自动精细裁剪契约。

## 模块所有权

| 边界 | 职责 |
| --- | --- |
| 普通 `AtomUI.Generator` | 输出包 marker、条件 TypeMap、具体 Group 的 accessor、预先保留的 helper |
| 官方 ILLink | 计算类型可达性和 TypeMap 条件标记闭包 |
| Toolchain 的 `ILLink10` profile | 读取官方已标记结果，验证生成 ABI，转换自有 accessor 并验证转换完整性 |
| 构建资产 | 自动接入后端、检查工具链、维护增量输入和发布门禁 |
| Core 注册代码 | 查询映射、收集片段、统一校验/挂载/冻结 |

完整顺序属于 [Browser 链接架构](../../architecture/foundations/aot-browser-linking.md)，符号与字段属于
[TypeMap 生成 ABI](../../reference/aot/typemap-contract.md)。本模块不分析应用 C#/AXAML/IL 调用图，不计算另一套依赖闭包，
不改写 BCL，不在运行时扫描裁剪后 Attribute。

## 处理阶段

转换必须在官方 Mark 完成、Sweep 之前进行。Browser AOT 的顺序为：

```text
普通编译 → ILLink 标记 → AtomUI 映射转换与验证 → Sweep → Mono AOT → bundling
```

步骤仅处理已解析、带有效包 marker 的程序集及其中可达的自有 accessor。每个 accessor 由
`GeneratedTypeMapAccessorAttribute(Type group)` 标识，并绑定一个具体 Group；不通过方法名或源码文本猜测目标。

转换流程为：

1. 验证工具链组合、执行阶段、程序集身份、marker ABI、accessor 及 helper 签名。
2. 读取对应 Group 中由官方标记保留的映射声明，检查 key 和目标类型的有效性。
3. 验证被写入的代理、字典 helper 及其传递依赖已经在 Mark 阶段保留。
4. 按确定顺序改写 accessor，只添加已选 key/代理类型；清理原方法的局部变量、异常区间和调试序列点。
5. 记录转换完成状态，确认所有可达的自有 accessor 均已转换，且其方法体不再调用 Mono 不支持的 TypeMapping API。

转换后的结果只包含已选映射，不得把全部候选代理写入方法。合法的空 map 与零可达 accessor 都允许；
存在应转换的 accessor 却未运行转换器必须失败。部署程序集若处于不支持的 copy/skip 处理状态，不得报告转换成功。

## 与生成器的 ABI

accessor 是内部、静态、非泛型、无参数方法，返回 `IReadOnlyDictionary<string, Type>`。转换前的方法体直接调用
具体 Group 的官方 TypeMapping API。字典 helper 是不引用具体控件/代理的闭合实现；它及其传递依赖必须在 Mark 前可达。

本模块在 Mark 后不能引入新的、尚未分析的辅助代码。纯 net10 NativeAOT/CoreCLR 使用官方映射后端，不加载此转换器。
同一普通 NuGet 输出必须支持这些后端，不能按包编译时的 Debug/Release 常量固化选择。
net10 消费 net8-only 包时的分析前 ABI 转换另见[混合 ABI 桥接](../../architecture/foundations/aot-net8-conditional-backends.md#net-10-消费-net8-only-包)；
这一步不替代任何正式引擎的依赖闭包。

## 分发与增量构建

产品 NuGet 自动交付后端工具及构建接线；应用和第三方作者不添加 custom-step 命令参数。
工具不进入 `lib/`、运行时依赖、应用输出或 publish 目录。同一构建消费多个产品包时只加载一份一致版本的转换器，
冲突版本明确失败，不连续执行不同版本转换同一 accessor。

custom-step DLL、依赖、配置、生成 ABI 与经验证的工具链身份全部属于链接增量输入。
后端改变必须重新链接；输入不变时复用已经完成后置验证的产物。不能假定 SDK 默认的链接输入已经覆盖扩展 DLL。

custom-step 接口随工具链演进，发布构建只能使用已经验证的组合。未知 ABI、工具缺失或损坏、helper 缺失、
未完成转换都阻止发布；不退回整包注册，也不跳过后置检查。

## 诊断与验证

使用统一 `REG` 诊断域的语义错误名称，包括 `UnsupportedRegistrationBackend`、`UnloweredTypeMapAccessor` 和
`RegistrationIdentityConflict`。对应 `ATOMUIREG006`、`ATOMUIREG007` 和 `ATOMUIREG005`；ILLink 公开诊断 API 的传输编号分别为
`IL6506`、`IL6507`、`IL6505`，消息保留 REG 编号。

必须分别验证 Browser trimmed interpreter 与 `RunAOTCompilation=true` 的真实发布及浏览器运行，覆盖：

- 单/跨程序集、传递 assembly target、多 Group、多条件共用代理、循环类型引用与空 map。
- 普通预编译 adapter、干净 NuGet cache、可选包未启用以及自动工具分发。
- 真实 AtomUI 启动、模板、Token、Semantic Part、主题切换与首次实例化前冻结。
- 未用控件、代理和 factory 在链接后产物中缺失；转换方法不含未用代理和不受支持的调用。
- 删除/损坏转换器、错误 ABI、缺 helper、残留 accessor 和不支持工具链的失败对照。
- 后端 DLL 改动触发重新链接，合法 no-op 构建不重复转换。

浏览器页面能启动不等于通过精细裁剪验证，AOT 构建成功也不等于完成浏览器运行验证。

## 独立工具入口与验证回执

工具项目为 `src/AtomUI.Toolchain/AtomUI.Toolchain.csproj`，选择 `AtomUIToolchainProfile=ILLink10`，
源码位于 `Backends/ILLink10`，目标 `net10.0`。它不引用 Core、Generator 或产品运行时，
并隔离发布调用者传播的 trim/AOT 属性。ILLink/Cecil 编译引用来自定版还原包：ILLink 由对应链接宿主提供，
Cecil 随离线桥接 CLI 的工具闭包交付。所有工具依赖都不得复制进应用。

Browser 映射物化入口仍为 `AtomUI.TypeMap.Linker.MaterializeTypeMapsStep`。构建资产通过
`_TrimmerCustomSteps` 的 `AfterStep=MarkStep` 注入，并传入 `AtomUITypeMapBackend` custom-data。
能力标识属于工具版本契约，由实现中的 `ToolchainContract` 定义；未知或缺失标识导致错误。
后端另行读取当前已加载 ILLink 的完整程序集身份与 informational version，不能只相信调用者传入的字符串。
TypeMap 从 .NET 10 引入，现代发布默认支持 .NET 10 及后续稳定版本，不按 SDK、功能带、补丁或构建号逐个准入。
Browser 目标框架允许 `net10.0-browser` 及以后版本，使用 SDK 选择的官方 ILLink 与 WASM 工作负载。
后端只检查官方链接器身份与 TypeMap 引入版本的下限；不读取或逐项比对链接器 targets 的参数定义，
也不校验 WASM 任务程序集的信息版本。保留 Mark/Sweep/Output 阶段、已标记映射和 accessor/helper ABI 验证，
并在链接后与 AOT/bundle 消费前验证实际产物。工具链出现真实的不兼容时按错误适配，不预先屏蔽未知的新版本。
历史能力值 `atomui-typemap-v1-illink-10.0.8` 保留为不透明的 ABI 标识，不再代表 ILLink 补丁约束。
签名与回执记录实际加载的 ILLink 版本，工具文件内容进入增量指纹；升级工具后必须重新链接。
net10 消费 net8-only 包的独立条件桥接仍有自己的 SDK/runtime 门禁，本次 Browser 后端规则不扩大其准入范围。

Browser 后端的转换检查独立于桌面工具链规则。纯现代桌面 trimmed CoreCLR 与 NativeAOT 只消费官方
TypeMap 契约，不加载本工具，也不校验 SDK 版本字符串；它们只要求实际 ILLink（官方身份）与 NativeAOT 编译器的
版本匹配目标框架，最低为 TypeMap 引入的 `10.0.0`，不设置已验证补丁号下限或后续目标框架上限。
工具自身使用 `Major` 运行时前滚策略，新 SDK 环境不必额外安装原 .NET 10 工具宿主。
验证以最低基线与当前稳定工具链的代表性场景为主，无需为每个 SDK 补丁逐一验收后才允许发布。

后端在筛选 marked accessor **之前**检查所有发现的标记包是否实际采用 `Link` action。
`Skip` 等 action 不保证方法有 Mark 记录，因此先筛 `IsMarked` 会把未转换包误判成零 slot。
工具不在 Mark 后改变 action，也不再次执行 Mark。

后端按验证阶段建立实例级元数据索引。同一输入程序集的嵌套类型和方法在读取阶段只展开一次；Sweep 后使用新的延迟索引，
避免用 Sweep 前的 Cecil 定义掩盖已删除成员；OutputStep 后重读的程序集再以其自身定义建立索引。索引只复用元数据枚举结果，
不缓存或重新计算 `Annotations.IsMarked` 的可达性结论。

`AtomUITypeMapReceipt` custom-data 可指定绝对回执路径。工具开始时删除旧回执，保存原始输入哈希，
通过实例持有的 `MethodDefinition` 在 Sweep 后检查，然后在公开 `OutputStep` 后重新读取自己的输出方法，
核对完成方法体并写入 `output-verified` JSON。回执只含工具/ABI身份、已验证 package/Group/accessor 身份及
输入/输出/方法体哈希，不是运行时注册输入。零 slot 的成功执行也写回执，从而与步骤未运行区分。
自动构建接线必须要求匹配本次工具与输入的回执；不能把“文件存在”当作有效性证明。
工具、输入、符号、回执和输出文件均以流式 SHA-256 读取，哈希内容与格式不变，并避免为大文件分配等长字节数组。

回执的程序集输出哈希绑定 **ILLink 输出和实际 AOT 输入**。Mono AOT 随后可能剥离已编译方法的 IL，
因此最终部署程序集使用独立的 metadata/运行验证，不要求其字节哈希与 AOT 前相同。构建门禁应在 AOT 前
校验回执，并保证后续没有替换已经校验的输入。

可重复的机制 fixture 位于 `tests/AtomUI.TypeMap.Linker.Tests/Fixtures`：它直接复用 Core ABI Attribute 源码，
用最小测试 builder 验证多包、条件别名、片段体触发的跨包间接依赖、空 map、PDB 及部署保留结果。
它不声称覆盖真实 AtomUI 控件、模板、Token 或产品 NuGet 接线；这些仍需产品发布验收。
