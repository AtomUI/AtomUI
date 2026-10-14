# 控件注册生成

> 本文描述本地已实现的普通生成器与资源平台声明读取。实现、交付验收与发布状态见
> [AOT 与裁剪架构](../../architecture/foundations/aot-and-trimming.md)。生成接口示例不代表当前已发布 API。

## 1. 模块边界

普通 `AtomUI.Generator` 在控件包编译时生成控件契约、主题资产和条件映射，在消费编译时生成包引用引导。
普通与裁剪发布使用同一份输出事实。官方 ILLink/ILC 负责最终类型可达性；本模块不建立应用使用分析、调用图或注册依赖图。

系统时序属于 [TypeMap 注册体系](../../architecture/foundations/aot-typemap-registration.md)，类型与资源语义属于
[控件注册契约](../../architecture/foundations/control-registration-contracts.md)，跨工具符号属于
[TypeMap 生成 ABI](../../reference/aot/typemap-contract.md)。

## 2. 输入模型

| 输入 | 生成器读取的事实 |
| --- | --- |
| Control 声明 | 完整 CLR 类型、可访问性、基类、正常平台可用性声明 |
| Token 声明 | 可选 Own Token、继承后的 schema、真实 owner 类型与 identity |
| Semantic Part 声明 | owner、属性、路由、专用 Style 和逐控件语义 descriptor |
| 包内编译型 AXAML | 顶层主题导出、词法资源作用域、显式常量 include、资产构造与覆盖顺序、资源 CLR 身份绑定 |
| 资源平台声明 | 资源 CLR 类型及其包含类型、程序集的标准平台声明；无资源类时使用资产所属程序集域 |
| 普通包身份 | 包资源身份及程序集身份，不包含入口方法名 |
| 已解析程序集引用 | `ControlPackageMarkerAttribute` 及其具体 Group/ABI |

只为至少拥有主题资源、Token 或语义内容的 Control 创建片段。public/internal 及可访问的 nested 类型均参与；
没有独立契约的派生控件不生成空片段。internal presenter 可以只拥有资源，不要求公开 Token descriptor。
可访问且非泛型的 `StyledElement` 主题目标也可形成资源独占片段：使用真实类型条件且仅注册导出资产；
不得因此扩大 Token descriptor 的类型边界。
开放泛型使用明确的非泛型主题 owner；不可访问或无法确定的 owner 在包编译时诊断，不用反射绕过。

包、命名空间或文件夹都不构成保留整个控件集合的理由。新增一个未使用的同目录控件不得扩大已用控件的注册集合。

普通包未显式设置 `AtomUIThemeControlCatalog` 时，Control Token catalog 使用定义程序集名，并写入相同的
程序集 catalog metadata 供引用方读取。内置仓库显式配置 `AtomUI`；第三方同短名控件因此不占用内置 Token identity。

## 3. 输出模型

每个逻辑控件片段只收集该控件的可选 Token descriptor、Semantic descriptor 和资产引用。片段不递归注册其他控件。
多目标资源字典保持为一个资产：其真实编译后类型引用参与官方闭包，多个片段通过 AssetId 去重挂载。

生成输出包括：

- 单项 Token/Semantic descriptor factory，以及静态资源 factory 和元数据。
- 带自身 Attribute 的 `ControlRegistrationFragmentAttribute` 派生代理，使用逻辑 `FragmentId`。
- 每个保留条件独立的 TypeMap key，以及只包含字符串的候选 key 表。
- 包 marker、可跨程序集引用的隐藏公共 Group、具体闭合 Group 的 accessor。
- `GeneratedControlPackageRegistration.Register` helper 与普通非裁剪的完整片段入口。

Control、TokenResourceExtension、TokenKey、OwnTokenKind 和生成的专用 Semantic Style 均按真实契约成为条件。
生成的 Own TokenResourceExtension 保持 boxed OwnTokenKind 查找键，与现有局部 Resources 字典相等；typed Identity
独立保存 owner，不改变正常资源覆盖优先级。编译后的资源扩展必须能读取既有枚举键，而不只通过生成文本快照。
同一片段的不同条件必须使用不同 key；不能用相同 key 的多条声明模拟 OR。
生成的 typed identity 实际保存并验证 owner 类型，使仅使用生成 identity 的代码也具有真实类型依赖。

完整与选中路径调用同一单项 factory，不能先调用全量 manifest 再过滤。
资产 factory 调用隐藏的 `ControlThemeAssetDescriptor.CreateGenerated`，传入 typed 元数据、编译期 global Token 名称快照
和独立 contract fingerprint。编译证据使用稳定 metadata name；最终完整运行时 Type 身份由 Core 对当前被选中资产
物化后验证，不能把 `System.Runtime` 等引用程序集身份猜测或硬编码映射为实现程序集。共享 schema 表只能含字符串。候选 key 表不能保存 Control Type、代理实例或 factory
delegate；Group 和共享生成类不能通过静态初始化保留全部控件。

条件 TypeMap 声明预期产生的裁剪诊断只在对应生成代码位置局部处理并说明理由；不得全局关闭 IL2026/IL3050。

注册 helper 对各后端统一发射单个 `Collect`，不读取首帧关键集配置、不按简单控件名分区。
`[BootstrapRegistration]` 读取真实控件或导出资源类 Symbol；仅在存在标记的包内生成逐片段 `IsBootstrap` 常量。
既有 asset export、Token owner 与语义 owner 关系在生成期闭合。无标记包使用旧默认完整执行；无贡献标记报告
`ATOMUIREG010`。普通和裁剪路径均先经 `AddFragment` 再访问 descriptor 工厂；net8 `CollectFull` 精确模板不变。
阶段选择不增加 TypeMap/ConditionalRecord 条件，不把未保留片段重新引入。
生成 descriptor 缓存与资源/快照惰性创建保留；Core 在同步 Build 或 Stage 2 补齐同一包快照，详见
[注册分期](../../architecture/foundations/lazy-control-registration.md)。

## 4. 主题导出与资源

Control 到资产的关联来自顶层导出的 ControlTheme key/TargetType。嵌套模板 TargetType、局部定制主题或 standalone 非默认
theme class 不能反向挂到无关控件的默认片段。命名主题与默认 typed theme 都必须保留其原本契约。
SemanticThemeBindings 仅来自实际 typed 属性赋值，由同一份 transform-local XML/类型/资源索引提供给语义校验和注册。
文件名与属性名相同、全包只有一个同名属性均不是归属证据；未赋值属性使用声明的 ContractType metadata。

资源模型记录 ExportedThemes、RequiredTokenOwners、可选 SemanticThemeBindings、AssetId、Phase、Order 和 factory。
RequiredTokenOwners 使用实际 owner Type；身份相等与 owner canonicalization 按
[控件注册契约](../../architecture/foundations/control-registration-contracts.md)验证，不能先按字符串身份去重再丢失冲突信息。

只解析本地、词法父作用域、显式常量 ResourceInclude/MergedDictionaries 和明确的公共包资源。
MergedDictionaries 中的内联 ResourceDictionary 递归参与其实际本地作用域，按正常的后项优先顺序查询；
其中私有 key 不提升为包级导出，也不会对另一资产自动可见。没有明确可见来源的 raw
DynamicResource 按开放宿主资源记录，不扫描所有私有字典找同名 key。StaticResource 的包内来源无法解释时报告具体位置。
真实 `{x:Type T}` 的默认主题引用仅在 key Type、TargetType 和引用 Type 完整相同、已有条件片段且提供者平台域覆盖
消费域时可由 `ldtoken T` 证明保留；不创建局部 include，从而保持 Application 的 type-key 主题替换。字符串或
任意不同目标的 Type key 不具有这项保证。
动态 URI/运行时 AXAML 不扩大为自动全包保留。
显式常量 `avares` include 指向已解析引用程序集时，是有明确来源、内容对当前编译不透明的资源边界；
其作用仅限该 include 实际可见的词法/父/include 路径，不会让另一文件的缺失 key 自动通过。
生成器检查程序集可解析性，不声称证明外部二进制内的 URI/key 或跨二进制循环；具体内容由 Avalonia 普通编译/加载验证。
跨程序集 include 的 NativeAOT/Browser 执行与未使用类型缺失仍需实际发布测试。

`GenerateThemeAssetWrappersTask` 继续提供编译型 fallback，原 AXAML 与 AdditionalFiles 身份不变。生成器在当前程序集
绑定资源类并检查可访问的无参构造函数：作者 ResourceDictionary 可以直接构造；精确默认 typed theme 单项导出可以使用
`ResourceDictionary.AddDeferred` 的静态回调，回调只构造原编译主题类并赋导出的 `TargetType`。构造不可访问、required
成员未由构造函数满足、obsolete 或隐藏 `TargetType` 等不满足直接 C# 构造条件的情况保留 wrapper。
资源类型、构造函数或外层类型上的 `ExperimentalAttribute`、`RequiresPreviewFeaturesAttribute` 同样保留 wrapper，
避免新增 C# 调用点的编译器或 SDK 分析器诊断；不抑制原 AXAML 诊断。此路径不使用反射或
运行时 URI 加载，主题构造仍在首次查找发生；不将任意 AXAML 属性转换为 C# 初始化器。

片段收集阶段不执行资源 factory；所有包收集完成后，
按真实 PackageCommitOrdinal 与包内 `(Phase, Order, AssetId)` 统一处理。显式嵌套 include 的顺序留在 factory 内。

平台限制生成在片段的真实代码分支中，先判断平台，再引用 descriptor/semantic/resource factory。
公共 builder 的事后 predicate 不能替代链接器可见的平台分支。

### 4.1 资源平台声明

平台语义统一由[类型与资源平台契约](../../architecture/foundations/aot-typemap-registration.md#7-平台分支)定义。
Control 与资源 CLR 类型读取标准 `SupportedOSPlatformAttribute` / `UnsupportedOSPlatformAttribute`。
资源域从本程序集实际资源类取得；没有资源类时取资产所属程序集域，再与导出目标域校验。旧 MSBuild 平台列表输入已删除。
该变更不改现有平台区间算法、TypeMap key、注册 ABI 或资源 fingerprint。

绑定流程为：复用已解析 AXAML → 确定资源 CLR 身份 → 在当前 Compilation 绑定 symbol → 复用
`PlatformAvailability` → 由 `AssetAvailability` 校验导出与 owner → 生成已有 guard。显式 `x:Class` 无法唯一绑定时
报告资产位置和类型身份，不能按全平台继续。无资源类时仍读取资产所属程序集的声明。

| 源码入口 | 实现责任 |
| --- | --- |
| `ThemeAssets/ThemeAssetInput.cs` | 删除两个自定义平台列表字段，保留 Path/Text/Directory/Link 等普通资源输入 |
| `ThemeAssets/ThemeAssetInfo.cs` | 统一提取 ResourceDictionary 与 ControlTheme 的资源 CLR 身份，删除旧平台列表；保留现有默认 typed theme 判定与 wrapper 规则 |
| `TokenResourceKeyGenerator.cs` | 删除两个平台 metadata 的读取/转发，复用同一份 XML 与资源事实，不另起扫描管线 |
| `Registration/RegistrationModelBuilder.cs` | 用完整类型身份解析资源类；将资源域接入既有 AssetAvailability、默认 type-key 与语义资源校验 |
| `Registration/PlatformAvailability.cs` | 复用类型声明绑定、域比较及 guard 生成；提供同语义的程序集域入口，不引入第二套平台规则 |

表中路径相对于 `src/AtomUI.Generator/`。资源类身份提取不得把任意 `x:Class` 字典误判成默认 typed ControlTheme，
也不得因新增资源类改变资源归属。已有强类型主题复用其类；新建资源类必须由普通 Avalonia 资源加载路径实际使用。
普通默认主题仍使用 deferred 资源契约，收集期间不加载字典。MSBuild 接线见
[构建与打包](../../architecture/foundations/build-and-packaging.md#主题资产的平台输入)。

## 5. 包入口与引用引导

包作者正常调用生成的 `Register(builder, createProvider, prepare = null, complete = null)`；具体示例见
[第三方控件包指南](../../guides/theming/third-party-control-packages.md)。helper 负责注册状态检查和按既定顺序调用生命周期。
生成器不解析作者入口方法体、不要求入口 Attribute，也不生成字符串方法身份。

包普通编译输出 `ControlPackageMarkerAttribute(string packageId, Type mapGroup, int abiVersion)`。
消费端只读取已解析程序集 marker，按完整程序集身份验证、确定性去重并生成具体 Group 的 assembly target。
不读取应用方法体，不要求 ProjectReference 因发布重编译为特殊模式，也不恢复预编译 consumer DLL 的使用记录。

声明 target 只让链接器知道映射来源。未调用入口的可选包不能因其 marker 被发现而执行 provider、语言注册或 initializer。
相同包身份来自不相容程序集、无效 Group 或 ABI 不匹配必须明确失败，不选择“第一份可用”输入。

## 6. 增量生成与协作

候选筛选面向包的声明与资源输入，不扫描应用调用点。每个变换输出可比较的不可变值，包含必要的 metadata name、契约值和
规范化来源位置。Compilation/SemanticModel/symbol 只用于当前绑定，不进入持久缓存结果。

Token、主题和 Semantic writer 共享普通契约模型，或使用由完整 metadata name 确定的 partial hook。
不能依靠生成器执行顺序或读取彼此生成文件。哈希只用于稳定生成符号，不重新引入跨消费程序集协议或应用计划。

资源平台绑定复用当前变换的 XML、类型解析与可用域缓存；缓存键使用完整类型身份。不得为读取资源声明重复解析 AXAML、
启动额外 MSBuild worker、扫描磁盘或建立应用使用分析。Compilation、资源类型声明、程序集声明和 AXAML 的变更必须
进入现有增量依赖；无关输入不变时生成输出确定，验证增量失效与输出复用。

## 7. 构建工具协作

`AtomUI.Toolchain` 的 Tasks profile 保留资源 wrapper、Localization 和隔离任务宿主，产物继续名为
`AtomUI.Build.Tasks.dll`。普通生成器及这些构建资产由产品 NuGet 自动分发，
不进入 lib/runtime/publish 输出。

Browser 发布由 [TypeMap Linker](../typemap-linker/overview.md)在官方标记完成后转换自有 accessor。
生成器输出其识别 marker 与预先可达的 helper 引用；本模块不替该后端计算选中集合。
普通非裁剪与发布路径通过链接器可识别的 feature switch 选择，不依赖包编译时的 Debug/Release 常量。

## 8. 验证契约

生成测试覆盖 public/internal/nested、无 Own Token、资源独占片段、typed identity、Token-only 与 Semantic Style-only。
资源测试覆盖默认/命名主题、多目标资产、局部同名 key、显式 include、开放 DynamicResource、平台限制和资源构造循环诊断。

平台来源测试覆盖 ResourceDictionary/ControlTheme 的资源类声明、包含类型/程序集域、无资源类、非法 `x:Class`、
同短名不同程序集类型、平台域不一致和 owner 不覆盖的用例；沿用版本区间及 iOS/MacCatalyst 既有用例。
同时覆盖只改变 C# 平台 attribute 或程序集 attribute、AXAML 内容不变的增量更新，避免复用陈旧的资源域。
资源重命名/移动后的域保持、同目录无关资源不继承限制、默认 typed theme 判定与 wrapper 语义不变也必须有回归对照。

消费测试覆盖直接/传递 ProjectReference、普通预编译 adapter 和干净 NuGet cache；应用与第三方源码不手写 TypeMap。
验证可选包未启用时无副作用、公共重复入口在 prepare 前失败、不同 builder 独立、错误 builder 不能继续启动。

最终发布验证必须执行 ThemeManager 启动、真实模板、Semantic Part 和主题切换，并证明未用 control/proxy/factory 被删除。
Browser 的 trimmed interpreter 与 AOT 均为独立门禁；生成源码快照和构建成功不能替代运行验证。
