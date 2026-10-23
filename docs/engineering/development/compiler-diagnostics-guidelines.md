# AtomUI 编译期诊断规范

本文档定义 AtomUI 编译期 warning / error 的统一维护规则，适用于 Roslyn analyzer、source generator diagnostic、AOT 兼容诊断，以及后续 AXAML、Theme、Token 等可在编译期发现的问题。

编译期诊断的目标是把可静态证明的问题尽早暴露给开发者。诊断不是运行时异常说明，也不是一次性 bug 修复记录。

## 适用范围

新增或修改以下能力时必须遵循本文档：

- `DiagnosticAnalyzer` 报告的 warning / error。
- source generator 通过 `ReportDiagnostic` 报告的问题。
- AOT、trimming、动态成员访问、字符串 path 等编译期兼容性检查。
- 后续 AXAML、Theme、Token、资源注册等静态检查。

专项文档只描述领域背景和触发场景；诊断 ID、命名、severity、编码组织、测试要求统一维护在本文档中。

## 诊断 ID 规则

AtomUI 诊断 ID 使用固定格式：

```text
ATOMUI<DOMAIN><NNN>
```

示例：

```text
ATOMUIAOT001
ATOMUIGEN001
ATOMUIXAML001
ATOMUITOKEN001
```

规则：

- `ATOMUI` 是固定项目前缀。
- `DOMAIN` 使用大写英文领域名，不加下划线、点号或连字符。
- `NNN` 使用三位数字，从 `001` 开始递增。
- ID 一旦发布，不能复用，不能改变语义。
- 诊断废弃时在注册表中标记 deprecated，不把旧 ID 分配给新问题。

.NET / Roslyn 生态通常使用 `CS8602`、`CA1852`、`IDE0005`、`IL2026`、`SYSLIB0011`、`RS2008` 这类“字母前缀 + 数字”格式。AtomUI 也采用这种格式，避免 `ATOMUI_AOT_001`、`ATOMUI.AOT.001`、`ATOMUI-AOT-001`。

这种格式更适合 `.editorconfig` 和 `#pragma`：

```ini
dotnet_diagnostic.ATOMUIAOT001.severity = error
```

```csharp
#pragma warning disable ATOMUIAOT001
```

## 领域前缀

已保留的领域前缀：

| Prefix | 领域 | 用途 |
|---|---|---|
| `AOT` | AOT / trimming | NativeAOT、trimming、动态成员访问、字符串 path |
| `GEN` | Source generator | generator 输入、生成失败、生成物一致性 |
| `XAML` | AXAML | AXAML 静态结构、绑定、资源引用 |
| `TOKEN` | Theme / Token | Token 定义、注册、资源键、主题约束 |
| `LOC` | Localization | Language Catalog、XLIFF、语言元数据和静态语言包 |
| `REG` | Control Registration | TypeMap 控件/资源契约、Package bootstrap、工具链和浏览器后端诊断；已分配 `001`–`015` |
| `LINK` | 历史 Linked Registration | Deprecated / reserved，仅保留已发布 ID 的历史语义，不用于新注册体系 |

新增领域前缀前必须先更新本文档，说明用途和 owner。

## 当前诊断注册表

本表记录已有诊断。TypeMap 本地实现与验证状态见 [AOT 与裁剪架构](../../architecture/foundations/aot-and-trimming.md)。
`REG` 错误语义与历史 `LINK` ID 分列在后文。

[资源平台声明收敛](../../architecture/foundations/aot-typemap-registration.md#7-平台分支)已实现：平台域错误继续使用
`ATOMUIREG006`，输入来源改为 Control/资源 CLR 类型及程序集的标准声明。显式资源 `x:Class` 无法解析或身份不合法时，
使用 `ATOMUIREG003`，带 AXAML 位置、类名和所属程序集，不得当作无声明继续。域不一致报告资产、冲突目标或 Token owner。
资源类校验限于 ResourceDictionary/ControlTheme 根；其他合法 AXAML 根不产生这类错误。退役的 `LINK` ID 不复用。

| ID | Category | Severity | Trigger | Fix | Owner |
|---|---|---|---|---|---|
| `ATOMUITOKEN001` | Token | Error | 编译主题输入无效、超限，或构建中间产物不能可靠生成 | 修正 XML/输入路径及容量，清理失效输出后重建 | Compiled theme input task |
| `ATOMUITOKEN002` | Token | Error | 编译主题的 Token/owner/算法契约、元数据或静态字面量不受支持或不匹配 | 按带源位置的诊断修正定义与依赖；显式选择支持的编译值域或动态来源 | Compiled theme generator |
| `ATOMUIREG001` | Registration | Error | 顶层主题导出目标、key 不明确或重复 | 声明可解析的 target 和唯一 Type/string key | Registration generator |
| `ATOMUIREG002` | Registration | Error | TokenResource 没有唯一且可访问的实际 owner | 修正 namespace 或 Token 契约 | Registration generator |
| `ATOMUIREG003` | Registration | Error | 主题目标不可访问、开放泛型或不是 StyledElement；资源 x:Class 不能绑定到本程序集的可访问具体资源类 | 使用可访问的具体主题目标 | Registration generator |
| `ATOMUIREG004` | Registration | Error | include 非常量、缺失、构造环，或 StaticResource 没有词法/显式 include 来源或可验证默认 type-key 条件（全包同名字符串导出不算依赖） | 修正 URI、依赖或资源作用域 | Registration generator |
| `ATOMUIREG005` | Registration | Error | package、Group、Token owner、资产或生成符号身份冲突 | 保留唯一且一致的完整身份 | Registration generator / TypeMap linker |
| `ATOMUIREG006` | Registration | Error | marker ABI/Group 无效、目标框架不受支持或与官方 TypeMap API/实际工具版本不匹配、平台字符串/资源域不兼容，或链接后端不受支持 | 重新构建匹配 ABI 的包/后端并修正平台或链接配置 | Registration generator / TypeMap linker |
| `ATOMUIREG007` | Registration | Error | 可达 TypeMap accessor 未正确转换，或 Sweep/输出产物与转换结果不一致 | 启用匹配后端，重新链接并验证生成 accessor/helper | TypeMap linker |
| `ATOMUIREG008` | Registration | Error | Native8 发布的隔离输出、SDK/runtime、trim 或 whole-program 输入不满足已验收边界 | 使用受支持的 Native8 发布参数并保持 registration scope 输出隔离 | Native8 build backend |
| `ATOMUIREG009` | Registration | 已退役 | 历史启动分区后端降级诊断，片段统一在启动提交时执行，不再产生 | 删除旧 `AtomUICriticalControls` 配置；编号保留，不复用 | Registration generator |
| `ATOMUIREG010` | Registration | 已退役 | 历史 `BootstrapRegistration` 标记校验；分级启动删除后该标记不存在，不再产生 | 删除旧标记；编号保留，不复用 | Registration generator |
| `ATOMUIREG011` | Registration | Error | 应用引用包清单缺失、版本不兼容、编码损坏或程序集身份不一致 | 用当前生成器重建诊断指出的二进制包 | Startup image linker |
| `ATOMUIREG012` | Registration | Error | 应用包、Control、资产、导出或语义依赖冲突 | 保留唯一身份，修正 owner 和资产声明 | Startup image linker |
| `ATOMUIREG013` | Registration | Error | 二进制或本项目编译主题所需 owner、Token 类型、key、阶段或能力与应用契约不匹配 | 以应用实际 schema 重建主题，程序集版本号单独变化不构成错误 | Startup image linker |
| `ATOMUIREG014` | Registration | Error | 语言模块/目录/翻译槽冲突、格式文本无效或静态声明支持语言缺少翻译覆盖 | 修正二进制语言数据并补齐该语言在最终 en-US 回退之前的目录覆盖 | Startup image linker |
| `ATOMUIREG015` | Registration | Warning | 具体 Application 不能自动实现启动镜像 host | 使用顶层非泛型 partial Application，或显式 UseStartupImage 并手动 Attach | Startup image linker |
| `ATOMUIAOT001` | AOT | Warning | 已知 data member path 的 model type，但找不到 generated accessor | 给类型、接口或基类添加 `[GenerateDataMemberAccessors]`，或显式传入 `IDataMemberAccessorDescriptor` | DataMemberAccessors |
| `ATOMUIAOT002` | AOT | Warning | model type 有 generated accessor，但目标 path 不会被生成 | 改成可访问实例属性、修正 path，或显式传入 descriptor | DataMemberAccessors |
| `ATOMUIAOT003` | AOT | Warning | 使用字符串常量 path，编译期无法确认 model type | 使用 `nameof(Type.Property)`，或显式传入 descriptor | DataMemberAccessors |
| `ATOMUIGEN001` | Generator | Warning | `[GenerateScopedResourceHost]` 标记的类型不是非泛型 partial 顶层 class | 改为非泛型 `partial class`，或移除该 attribute | ScopedResourceHost |
| `ATOMUIGEN002` | Generator | Warning | `[GenerateScopedResourceHost]` 标记的类型未继承 `AvaloniaObject` | 只在非 Visual `AvaloniaObject` 描述对象上使用该 attribute | ScopedResourceHost |
| `ATOMUIGEN003` | Generator | Warning | `[GenerateScopedResourceHost]` 标记的类型继承了 `Control`、`StyledElement` 或 `Visual` | Visual 控件应使用视觉树资源宿主，不使用该 attribute | ScopedResourceHost |
| `ATOMUIGEN004` | Generator | Warning | `[GenerateScopedResourceHost]` 标记的类型已经实现 `IResourceHost` 或 `IThemeVariantHost` | 删除手写实现后再使用该 attribute，或移除该 attribute | ScopedResourceHost |
| `ATOMUIGEN013` | Generator | Error | Control Design Token 继承链包含未标记或具体中间层，或没有到达 `AbstractControlDesignToken` | 显式标记每个非根层，并把中间层改为 `abstract` | ControlDesignToken |
| `ATOMUIGEN020` | Generator | Error | 具体 `[ControlDesignToken]` 终端未声明 `sealed` | 封闭终端；需要复用时另建标记的抽象定义层 | ControlDesignToken |
| `ATOMUIGEN021` | Generator | Error | Control Design Token 定义层或终端使用泛型 | 改为非泛型 Token 类型 | ControlDesignToken |
| `ATOMUIGEN022` | Generator | Error | 继承链中的属性同名、隐藏或形成重复 schema key | 只保留一个具有稳定语义的属性声明 | ControlDesignToken |
| `ATOMUIGEN023` | Generator | Error | Own Token 属性为 static、indexer、显式接口实现、访问器不可访问、`init`-only、`required` 或 virtual/abstract/override | 改为生成代码可访问的非 virtual 实例普通 get/set 属性，或使用 `[NotTokenDefinition]` 排除辅助属性 | ControlDesignToken |
| `ATOMUIGEN024` | Generator | Error | 后续层 `CalculateTokenValues` 没有把唯一的直接 base 调用放在 block body 第一条并原样转发参数 | 以 `base.CalculateTokenValues(isDarkMode);` 开始方法，再计算本层值 | ControlDesignToken |
| `ATOMUIGEN025` | Generator | Error | 抽象 Control Design Token 定义层名称不以非空 `Token` 后缀结束 | 使用表达家族语义的 `*Token` 名称 | ControlDesignToken |
| `ATOMUIGEN026` | Generator | Error | Control Design Token 声明为嵌套类型 | 把 Token 移到 namespace 下作为顶级类；泛型容器中的嵌套 Token 仍使用 `ATOMUIGEN021` | ControlDesignToken |
| `ATOMUIGEN040` | Generator | Error | Own默认求值版本未知／整链混用、revision非法、新版终端缺partial或静态接口、手写新入口或整体evaluator | 整链选择同一版本；新版每层revision为正，终端使用partial及`IControlTokenDefaults`，由生成器实现入口 | ControlTokenDefaults |
| `ATOMUIGEN041` | Generator | Error | 默认目标缺失／重复、继承重定义标记不符、默认函数对终端不可访问，或字段缺少有效默认 | 明确唯一默认，显式替换继承默认并提供可访问静态函数 | ControlTokenDefaults |
| `ATOMUIGEN042` | Generator | Error | 默认函数返回类型、context或Own参数类型／数量／ref形态不符，或函数为实例／泛型／async | 使用精确返回类型、首个`in ControlTokenDefaultContext`及按声明顺序的强类型Own参数 | ControlTokenDefaults |
| `ATOMUIGEN043` | Generator | Error | 默认Own输入未知或依赖成环 | 使用同终端字段，按诊断链移除循环；不修改按名称分配的slot | ControlTokenDefaults |
| `ATOMUIGEN044` | Generator | Error | Own别名未知/非Own目标、类型或nullable不符、自身/链/环/重复，终端缺partial或IControlTokenAliases，手写别名入口或V2默认读写别名 | 使用同终端直接规范字段；保留历史slot/key，入口交由Generator；默认只依赖规范字段 | ControlTokenAliases |
| `ATOMUIGEN045` | Generator | Error | Token资源ID合同版本/行/容量无效、重复或非连续ID、字段/范围缺失或删除 | 保留已登记ID，为新增字段追加ID，修正对应附加文件行；移除须显式迁移审查 | TokenResourceOrdinals |
| `ATOMUIGEN027` | Generator | Error | Semantic Part 的 Control/Part 声明、名称、path、class、cardinality 或 customization 非法 | 按 Semantic Part 声明契约修正 `[SemanticPart]` | SemanticParts |
| `ATOMUIGEN028` | Generator | Error | 同一 Control 的 Part name、path 或 selector class 重复 | 为每个 Part 使用唯一名称、path 和 `.semantic-*` class | SemanticParts |
| `ATOMUIGEN029` | Generator | Error | Semantic Part `ContractType` 不是 public `StyledElement` | 使用稳定且公开可引用的 Avalonia 类型 | SemanticParts |
| `ATOMUIGEN030` | Generator | Error | `SelectorAndTheme` 缺少合法的 public get/set `ControlTheme` 属性契约 | 修正 `ThemePropertyName`、属性类型、public setter 或 `ContractType` | SemanticParts |
| `ATOMUIGEN031` | Generator | Warning | Semantic Part 未声明引入版本 | 设置非空 `Since` | SemanticParts |
| `ATOMUIGEN032` | Generator | Error | 某个 ControlTemplate 的 marker 数量不满足 cardinality | 为每个适用模板补齐或移除 marker，或修正 cardinality | SemanticParts |
| `ATOMUIGEN033` | Generator | Error | marker 节点类型不能赋值给 `ContractType` | 把 class 放到兼容节点，或重新设计稳定 ContractType | SemanticParts |
| `ATOMUIGEN034` | Generator | Error | Control 声明静态 Part，但不存在适用的可分析 ControlTemplate | 提供叶子 ControlTheme/ControlTemplate，或仅在真实动态创建场景使用 `RuntimeCreated` | SemanticParts |
| `ATOMUIGEN035` | Generator | Error | 同一 ControlTemplate 节点同时承担多个 Semantic Part | 将不同公开职责拆分到独立节点，每个节点只保留一个 Semantic Part marker | SemanticParts |
| `ATOMUIGEN036` | Generator | Error | 已声明 Part 的 `Classes.semantic-*` marker 使用了 false 或动态值 | 使用静态 `Classes.semantic-*="True"`；状态变化通过属性或伪类表达 | SemanticParts |
| `ATOMUIGEN037` | Generator | Error | Semantic Style 的完整 CLR identity 与另一生成候选、当前程序集已有类型或 canonical XML namespace 下可见的引用程序集 public 类型冲突 | 调整 Control 或 Part path 命名，或移除占用 `AtomUI.Theme.Styling` 生成 identity 的类型 | SemanticParts |
| `ATOMUIGEN038` | Generator | Error | Semantic Part 的 `Since` 不是 `major.minor.patch` 三段式发布版本（例如写成版本线 `6.0` / `6.2`，或带 `v` 前缀、预发布后缀、段数不符） | 写成具体发布版本，例如 `6.2.0` | SemanticParts |
| `ATOMUIGEN039` | Generator | Error | 具体 Application 手写了 `RegisterServices` 重写，但方法体内没有调用 `ApplicationStartup.Attach` | 在 `base.RegisterServices()` 之后调用 `ApplicationStartup.Attach(this)`，或删除手写重写让生成器生成 | ApplicationStartupHook |
| `ATOMUIGEN101` | Generator | Warning | Gallery source code display generator 发现参与默认源码匹配的 `ShowCasePanel` 缺少 `Name` | 给 `ShowCasePanel` 设置稳定 `Name`，或使用显式源码 key 规则 | GallerySourceCodeDisplay |
| `ATOMUILOC001` | Localization | Error | AtomUI 固定语言数据记录的 schema、标识符、BCP 47 标签或元数据无效 | 按数据 schema 修正发生错误的具体记录 | LanguageTags |
| `ATOMUILOC002` | Localization | Error | AtomUI 固定语言数据包含重复的属性标识符或规范 BCP 47 标签 | 删除重复记录并为每个属性和标签保留唯一映射 | LanguageTags |
| `ATOMUILOC003` | Localization | Error | `[LanguageCatalog]` 声明不是可生成的稳定 enum 契约 | 使用 public 非泛型 enum、至少一个隐式顺序资源项且不使用 `[Flags]` | LocalizationGenerator |
| `ATOMUILOC004` | Localization | Error | Catalog 成员缺少显式正整数 ID，或 ID 重复、越界 | 为每个成员分配唯一且永久保留的显式正 `Int32` ID | LocalizationGenerator |
| `ATOMUILOC005` | Localization | Error | XLIFF 文档不符合 AtomUI 支持的 XLIFF 2.1 结构或语言元数据规则 | 修正 XML namespace、版本、语言标签、file/unit/segment 结构或翻译状态 | LocalizationGenerator |
| `ATOMUILOC006` | Localization | Error | XLIFF 的 Catalog identity、unit 或源契约与代码声明不匹配 | 使用目标 Catalog 导出的模板同步 file ID、unit Key、源文本、占位符和 fingerprint | LocalizationGenerator |
| `ATOMUILOC007` | Localization | Error | 翻译文本不可发布或 CompositeFormat 参数契约无效 | 提供 translated target，并保持源/目标占位符索引与格式语法一致 | LocalizationGenerator |
| `ATOMUILOC008` | Localization | Error | 应用类型无法实现生成式语言 bootstrap | 保留唯一的非抽象 partial Avalonia Application host，或移除应用级语言输入 | LocalizationGenerator |
| `ATOMUILOC009` | Localization | Error | 静态语言包包含运行时代码/二进制、非法路径、缺失必需 metadata、未满足 Verified 要求或混合目标语言 | 删除运行时资产，并使用模板生成的声明式 contentFiles/buildTransitive 包结构 | AtomUI.Build.Tasks |
| `ATOMUILOC010` | Localization | Warning | 静态语言包没有取得目标模块的权威 `en-US` 契约，打包只能执行延迟契约校验 | 添加作者期 `PrivateAssets=all` 组件 PackageReference；社区包也可保留 Deferred 并由消费应用完成完整校验 | AtomUI.Build.Tasks |

独立 ILLink 适配器使用公开 `CreateCustomErrorMessage` API，因此传输编号为 `IL6505`、`IL6506`、`IL6507`，
分别对应消息中的 `ATOMUIREG005`、`ATOMUIREG006`、`ATOMUIREG007`。REG 保持统一语义编号；不得通过私有 API 改写 ILLink 前缀。

`ATOMUIGEN005` 和 `ATOMUIGEN006` 原本约束 `[ControlDesignToken]` 类型上的 `public const ID`，该手工 ID 契约已
删除，因此这两个诊断不在新主题架构中复用。无参数 `[ControlDesignToken]` 本身继续保留，只负责标记 Own Token
类型，不携带 Control 类型或 identity。Control identity 由可主题化 Control 和主题资产约定生成；相关歧义、Token
继承、Global/Own 重名、资产依赖和注册错误应使用新的独立诊断 ID。

## TypeMap 注册诊断

`REG` 是当前控件注册领域的诊断前缀。以下语义依次对应注册诊断编号（`ATOMUIREG009` 已退役），
精确编号见上表；Core 启动时才可判断的冲突使用运行时异常。旧 `LINK` ID 不复用。
类型与资源行为以 [控件注册契约](../../architecture/foundations/control-registration-contracts.md)为准，
发布 ABI 以 [TypeMap 契约](../../reference/aot/typemap-contract.md)为准。

| 错误语义 | 可静态证明的触发条件 | 处理 | 修复方向 | owner |
| --- | --- | --- | --- | --- |
| `AmbiguousThemeExport` | 主题导出、TargetType 或默认主题归属不唯一 | Error | 明确默认导出/自定义主题边界，不按目录或短类型名猜归属 | 普通控件注册 Generator |
| `InvalidTokenOwner` | identity 与实际 owner Type 冲突，或必要 Token owner 不符合契约 | Error；动态合并冲突由运行时校验 | 修正 owner/identity，并在身份去重前完成 canonicalization | 普通 Generator / Core 注册校验 |
| `InaccessibleRegistrationType` | 生成片段无法正常访问 owner、代理或 factory，或开放泛型没有明确主题 owner | Error | 提供可正常访问的非泛型契约，不以反射绕过 | 普通控件注册 Generator |
| `InvalidResourceDependency` | 包内 StaticResource 无法在声明作用域解析，include 非法或存在资源构造循环 | Error | 修正本地资源和显式 include；宿主开放 DynamicResource 不按内部依赖误报 | 主题资产 Generator |
| `RegistrationIdentityConflict` | Package、Group、key、FragmentId、AssetId 或导出定义出现不兼容冲突 | Error | 保持唯一且一致的身份；同 key 不用多个 trimTarget 表达 OR | 普通 Generator / 浏览器链接后端 |
| `UnsupportedRegistrationBackend` | 未验收工具链、错误后端 ABI、非预期链接阶段或程序集处理状态 | Error，阻止发布 | 使用已验收组合或完成后端适配，不能全包兜底 | 浏览器链接后端 / 构建集成 |
| `UnloweredTypeMapAccessor` | 应转换的可达自有 accessor 遗漏，或仍调用目标后端不可执行的 TypeMapping API | Error，阻止发布 | 修正自动接线、helpers 保留和转换完整性 | 浏览器链接后端 / 产物验证 |

同名的无关私有资源不能单独触发 `InvalidResourceDependency`；开放宿主 DynamicResource 与包内必需资源必须按普通
资源作用域区分。运行时才可确认的缺包、跨包 owner、资源和语义错误由启动校验报告，不伪装为应用 usage 编译诊断，
也不为复刻旧诊断重新引入消费 DLL 调用图分析。

## 历史 LINK 诊断保留表

下列 ID 全部标记为 **retired / reserved**，不分配给新语义。当前源码已移除其生产者；仅用于识别历史版本的日志。
遇到这些 ID 应检查构建工具版本并保留原始诊断证据，不能把增加旧 root、恢复
Sidecar 或全包 fallback 作为 TypeMap 的修复方案。

| ID | 历史语义，仅用于识别旧日志 | 历史 owner |
| --- | --- | --- |
| `ATOMUILINK001` | 应用静态计划 owner 不唯一 | LinkedPublishGenerator |
| `ATOMUILINK002` | 动态使用无法解析，旧管线扩大包保留范围 | LinkedPublishGenerator |
| `ATOMUILINK003` | 缺少兼容旧 manifest | LinkedPublishGenerator |
| `ATOMUILINK004` | 旧显式 root 无法解析 | LinkedPublishGenerator |
| `ATOMUILINK005` | 旧粒度、Unit、公共资源声明或输入来源冲突 | LinkedPublishGenerator / Build.Tasks |
| `ATOMUILINK006` | 旧 linked-registration manifest 不兼容或损坏 | Build.Tasks |
| `ATOMUILINK007` | 动态 AXAML/资源来源触发旧包级保留 | LinkedPublishGenerator |
| `ATOMUILINK008` | 旧使用分析判断包入口未调用 | LinkedPublishGenerator |
| `ATOMUILINK009` | 旧注册入口 Attribute 或方法签名无效 | OrdinaryGenerator |
| `ATOMUILINK010` | 动态使用未被旧静态计划覆盖 | LinkedPublishGenerator |
| `ATOMUILINK011` | 旧 Attribute 与项目 metadata 的 Unit 声明不一致 | LinkedPublishGenerator |
| `ATOMUILINK012` | 同一文件内旧 Unit 声明冲突 | LinkedPublishGenerator |

## Severity 规则

- 默认使用 `Warning`。
- 只有生成结果必然错误、编译产物不可用、或继续编译会掩盖严重错误时才使用 `Error`。
- 对迁移期可能大量触发的问题，先使用 `Warning`，再由具体项目通过 `.editorconfig` 升级为 `Error`。
- 不要用 analyzer warning 替代运行时 guard。编译期无法证明安全时，运行时仍要有明确异常或该领域允许的兼容边界；控件注册和 Browser 发布禁止自动全包 fallback。

## 代码组织

Roslyn 诊断定义集中放在：

```text
src/AtomUI.Generator/Diagnostics/
├── AtomUIDiagnosticIds.cs
├── AtomUIDiagnosticCategories.cs
└── AtomUIDiagnosticDescriptors.cs
```

规则：

- Roslyn diagnostic ID 必须来自 `AtomUIDiagnosticIds`。
- Roslyn category 必须来自 `AtomUIDiagnosticCategories`。
- `DiagnosticDescriptor` 优先定义在 `AtomUIDiagnosticDescriptors`。
- analyzer 可以放在对应领域目录，例如 `DataMemberAccessors/`、`DesignToken/`、`Language/`。
- analyzer 不要在业务文件中散落硬编码 ID、category 或重复 descriptor。

浏览器链接后端不能为了复用诊断定义而加载 Roslyn Generator。跨工具共享的 ID/消息契约应采用无 Roslyn/ILLink
运行时依赖的共同定义或生成输入，仍以本注册表为唯一分配来源；工具适配层分别形成 Roslyn diagnostic 或构建错误。

## Descriptor 编码规则

每个 `DiagnosticDescriptor` 必须满足：

- `Id` 使用 `AtomUIDiagnosticIds` 常量。
- `Category` 使用 `AtomUIDiagnosticCategories` 常量。
- `Title` 使用英文短句，不加句号。
- `MessageFormat` 使用英文，必须包含触发对象，并给出修复方向。
- `DefaultSeverity` 按本文档 severity 规则选择。
- `isEnabledByDefault` 默认 `true`。
- `Location` 指向最小可修复语法节点，例如 path、attribute、类型名，而不是整个文件或整个 class。
- 一个 diagnostic 只表达一个根因，不把多个修复方向混到一个 ID 中。

## Analyzer 编码规则

- 只报告可以静态证明的问题；无法证明时可以报告“不可验证”类诊断，但不能编造具体类型或 path。
- 避免对 generated code 报告诊断，除非诊断专门检查 generated code。
- 不要在 analyzer 中执行昂贵的全编译扫描；优先注册具体 syntax / symbol action。
- 不要依赖源码字符串匹配判断语义；使用 `SemanticModel`、`ISymbol`、`ITypeSymbol` 等 Roslyn API。
- 诊断位置要落在用户需要修改的表达式上。
- analyzer 自身不能引入运行时依赖，必须保持可作为 analyzer 包加载。
- 浏览器链接后端诊断由独立编译工具产生，不把 ILLink/Cecil 依赖装入普通 Roslyn Generator；错误 ID 仍由本注册表统一管理。

## 测试规则

每个新增 diagnostic 必须包含：

- 正例：应报告，并断言 ID、数量和关键 message。
- 反例：正确写法不报告。
- 边界：无法静态证明时不误报，或按设计报告“不可验证”类诊断。
- 如果诊断与 AOT 相关，至少覆盖 generated accessor、显式 descriptor 或等价修复路径中的一种。

测试应放在对应 generator 测试项目中：

```text
tests/AtomUI.Generator.Tests/
```

浏览器后端还必须覆盖实际转换阶段、工具链/ABI 不匹配、helpers 缺失和残留 accessor 的失败对照；不能只断言诊断文本而
跳过真实发布产物检查。

测试中如果故意验证该领域允许的 fallback 或错误路径，需要用局部 `#pragma warning disable <ID>` 标明意图，不要在项目级别全局 suppress。

## 文档要求

新增 diagnostic 时必须同步更新：

- 本文档的诊断注册表。
- 对应领域文档，例如 AOT 规则写到 [aot-programming-guidelines.md](aot-programming-guidelines.md)。
- 如果诊断需要用户迁移，补充修复示例。
