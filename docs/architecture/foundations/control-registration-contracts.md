# Control 注册契约

本文拥有 AtomUI 控件注册的类型、Token、Semantic Part、主题资产与资源依赖契约。
采用状态和实现进度见 [AOT 与裁剪架构](aot-and-trimming.md#1-状态与事实边界)；条件映射与包引导见
[TypeMap 注册架构](aot-typemap-registration.md)。以下模型定义目标行为，不表示对应源码已完成迁移。

## 1. 类型与资产分离

```text
ControlContract
  ControlType / MetadataName
  Optional TokenIdentity / Token schema
  Optional Semantic contract
  TriggerTypes[]
  PlatformAvailability
  AssetReferences[]

ControlRegistrationFragment
  FragmentId
  ControlType
  Optional ControlTokenDescriptor factory
  Optional ControlSemanticDescriptor factory
  Asset factories/metadata

ThemeAsset
  AssetId / URI / factory symbol
  Optional ResourceClassIdentity  // 编译期资源 CLR 身份
  ResourceAvailability / EffectiveAvailability
  ExportedThemes[]                 // 导出 key 与 TargetType
  RequiredTokenOwners[]            // identity 与实际 owner Type
  Optional SemanticThemeBindings[]
  LexicalResourceScopes / ExplicitIncludes
  ExternalDynamicKeys[]
  Phase / Order / schema fingerprint
```

Control、资源资产和 Package 分别表达类型契约、不可分割资源与服务启用边界。源码目录不决定保留关系；模型不包含 UnitId、
OwnerUnitId、ReferencedUnitIds、Package/Directory 粒度或应用依赖边。

Roslyn Symbol 只服务当前生成变换。增量缓存使用稳定 metadata name、值模型和路径，不持有 Compilation/SemanticModel。
类型身份包含定义程序集和完整 metadata name；不能用不带命名空间的名字消除歧义。

## 2. Control 片段与保留条件

发现可被生成代码正常引用的 public/internal 非泛型 Control，包括 nested 类型。至少拥有 Token、Semantic Part 或导出主题
之一才生成片段；没有独立内容的派生类型依靠基类与实际 IL 引用，不生成空片段。

internal presenter 可以只拥有主题资源，不要求 ControlTokenDescriptor，也不能伪造公开 Token identity。
资源独占片段还支持正常可访问的非泛型 `StyledElement` 主题目标（例如窗口装饰）；以真实目标类型作为条件，
只贡献已导出的主题资产。这不扩大 Token/普通 Control schema 的适用类型，也不为非 Control 目标制造 Token descriptor。
有公开 Global Token 配置、Own Token 或 Semantic 契约的 Control 继续拥有对应 descriptor。
开放泛型 Control 使用可确定的非泛型主题 owner；没有可用 owner 时在普通构建诊断。不可访问的私有类型不能通过反射绕过
生成代码可访问性要求，主题契约必须具备正常可访问的 owner。

片段的条件类型包括：

- Control 本身。
- 生成的 TokenResourceExtension、TokenKey，以及存在 Own Token 时的 OwnTokenKind。
- 每个生成的专用 Semantic Style。

不同条件使用独立 TypeMap key，运行时按代理类型去重。生成 Identity 通过第 4 节的真实 owner 数据产生保留证据，不能把
只访问静态 `XxxTokens` 类当成充分条件。枚举变成裸整数或只参与常量运算不代表使用了控件资源。

平台可用性是 ControlContract 的普通产品属性，实际 guard 必须位于片段内、descriptor/factory 引用之前；生成规则由
[平台分支](aot-typemap-registration.md#7-平台分支)拥有。

## 3. 主题导出与资产校验

### 3.1 Control 到资产的关联

默认主题关系来自资产实际导出的 Control；Semantic Theme 属性关系另由已解析的普通 AXAML 赋值证明：

- ResourceDictionary 顶层 ControlTheme，或语义上继承 ControlTheme 的具体实例，读取其 key 与 TargetType。
- 顶层命名主题与 `{x:Type ...}` 主题均有效，不能只保留默认 type key。
- standalone 默认 typed theme 按默认主题契约保留 deferred 字典；可静态构造时由生成工厂通过 `AddDeferred` 注册，其他情况使用编译型 deferred wrapper。
- 嵌套 ControlTemplate.TargetType、局部定制主题不建立全局反向关联。
- standalone 非默认 theme class 通过实际构造引用保留；需要独立 metadata 时使用 theme-class 对应的 asset-only fragment，
  不把它挂到无关 Control 的默认片段。

例如 SearchButtonTheme 的 TargetType 是 Button，但它服务 SearchEdit 的定制主题。仅使用 Button 不能因此保留 SearchEdit。
判断元素类型必须使用语义继承关系，不能只匹配 XML 元素名 `ControlTheme`。

TopLevelMenuItemTheme、TabItemTheme/CardTabItemTheme、TabStripItemTheme/CardTabStripItemTheme 等顶层命名主题按其
TargetType 随片段保留，不需要分析 C# 字符串查找。

### 3.2 多目标与纯资源资产

一个字典导出多个目标时保持资产完整：任一目标保留其 factory，编译后的 factory 真实引用其他目标，由官方 linker 完成闭包。
多个片段引用同一 AssetId 只挂载一次。CascaderViewFilterListTheme、TourTheme、TabOverflowMenuTheme 是此类资产的例子。
不要为模拟文件粒度重新建立目录 Unit。

纯支持资源通过普通显式 include 或 Provider core 引用保留，不创建虚拟 Control，不依赖整个 Package 恰好已经加载。
同一包的真正公共资源仍由明确的 core 路径提供；不得把 owner 解析失败自动解释成公共资源。

### 3.3 Asset schema

编译期资产模型记录可选资源 CLR 身份、资源自身域与经导出目标校验后的有效域；声明归属与计算规则由
[平台分支](aot-typemap-registration.md#7-平台分支)拥有。来源收敛已实现，验证状态见
[AOT 与裁剪架构](aot-and-trimming.md#1-状态与事实边界)。资源类不替代导出 TargetType，不改变主题所有权或保留条件。
这些编译期事实用于校验和生成 guard，不增加运行时 descriptor 字段、序列化平台清单或新的 ABI/fingerprint 格式。

主题资产以 ExportedThemes 与 RequiredTokenOwners 取代单一 OwnerIdentity：

- ExportedThemes.TargetType 可以是没有 Token descriptor 的 internal Control。
- RequiredTokenOwners 必须对应实际注册的 Token descriptor，并验证 type/identity 一致。
- SemanticThemeBindings 明确 owner、property 和 target。只读取 typed ControlTheme 的 Setter 或 typed 实例的属性赋值，解析 inline theme、已知 theme class 或词法/include 可见的 StaticResource；文件名、目录、全包唯一同名属性均不构成证据。
- 限定属性元素必须先解析 XML 命名空间与 owner 类型，再匹配真实 instance property slot。附加属性不等于同名实例属性；真正 override 共享基类 slot，`new`/隐藏属性保持不同 slot。继承的 public 可读写 ControlTheme 属性使用同一解析规则。
- 同一份已解析 AXAML 与类型/资源索引同时服务 Semantic Part 校验和注册模型。未赋值属性保留声明的 ContractType metadata，不生成资产保留边；无法静态解析的已赋值语义主题报告普通诊断。
- 资产 URI、AssetId、导出 key 与 factory 定义必须一致，冲突明确失败。
- 校验在所有包收集之后执行，不受片段访问顺序影响。

ControlThemeAssetManifest 与 ThemeSchemaRegistry 的校验、schema fingerprint、revision 必须反映新模型。
资源键 schema fingerprint 继续承担主题与 schema 一致性检查；它不等于已退役的消费 Sidecar contract hash。

普通生成资产通过隐藏的 `ControlThemeAssetDescriptor.CreateGenerated(...)` 分两层验证。编译期 contract fingerprint
使用独立 domain `AtomUI.GeneratedControlThemeAssetContract` / format `1`，记录资产标识、规范化 URI、导出、owner、
语义绑定、稳定 CLR metadata full name（namespace 与 nested `+`）以及按 slot 排列的编译期 global Token 名称快照。
该层按 metadata name 独立排序，不能预测引用程序集对应的运行时实现程序集。

Core 先重算并核对这份编译期 contract，再只对被选中资产的实际 `typeof` 值物化最终资源键指纹。
最终指纹、revision 与 owner 验证仍使用完整运行时 Type/AssemblyQualifiedName，保留完整类型身份。
ThemeSchemaRegistry/ControlThemeAssetManifest 使用实际已注册的 global schema 独立验证最终指纹；物化时不得查询当前
Core globals 替代生成代码携带的快照，以免旧 schema 自认证通过。两个层次复用同一 Core 数值/字符串 FNV 编码。
显式 fingerprint 构造函数及陈旧指纹拒绝契约保持不变。
资产 owner 不再充当实际 Token 查找作用域，ThemeCompiler 与 ThemeScope 继续使用 Token 契约。

## 4. Token 身份与去重前规范化

### 4.1 可验证的 owner

生成的身份携带真实类型数据：

```csharp
public static readonly ControlTokenIdentity Identity =
    ControlTokenIdentity.ForControl(typeof(Button), "AtomUI", "Button");
```

`ForControl` 保存 owner Type；schema 查找、资源键解析和配置校验实际使用它验证对应 descriptor。
禁止使用可被消除的空 `typeof` 表达式作为保留手段。owner 不是序列化配置的新字段，也不是用户必须填写的 AOT 选项。

Equals、GetHashCode、`==`、`!=` 只比较 Catalog/Id。不能在 record struct 增加 Type 自动属性后继续依赖默认 equality。
`new ControlTokenIdentity(catalog, id)` 仍为配置或查找数据，只匹配已经保留的 schema；任意字符串不自动成为动态 root。

### 4.2 Canonicalization 先于去重

同一字符串身份携带冲突 owner 时必须失败，不能接受先出现者。以下路径共享同一规范化规则：

- ThemeConfigBuilder.WithControl。
- 配置规范化、继承与合并。
- RequiredTokenOwners 的收集与合并。
- 相关 identity-keyed Dictionary、集合及 Distinct 操作。

规范化发生在按 identity 去重之前：

| 左右身份信息 | 结果 |
| --- | --- |
| 两边都没有 owner | 保持字符串身份 |
| 只有一边有 owner | 提升为带 owner 的 canonical identity |
| 两边 owner 相同 | 合并且保留 owner |
| 两边 owner 不同 | 明确失败 |

Dictionary 对等值 key 的 value 替换不会更新原 key。需要提升 owner 时应移除原 key 后重新插入，或使用能明确保留 canonical
key 的容器；不能先丢弃 owner，再期待最终 registry 检查发现冲突。冻结阶段仍验证 canonical owner 与实际 descriptor 一致。

### 4.3 Token 资源键

生成的 TokenResourceExtension 的 global 分支使用携带控件 Identity 的资源键；own 分支返回原有 OwnTokenKind 装箱键。
Own 分支必须保持与直接 boxed enum 资源键相同的查找身份，使 Control/Window 的局部 Resources 覆盖、动态更新和
移除后的作用域回退保持一致；不能用 owner 包装键替换这一公共查找身份。枚举类型的条件 TypeMap 映射必须保留。

typed Identity 独立保留真实 Control Type。显式 `ControlTokenResourceKey.Own(identity, key)` 查询继续严格校验
Identity owner、资源键所属 Control 与目标 descriptor 的一致性；错误 owner 不能因字符串 equality 相同被忽略。

## 5. Semantic Part 同一事实源

每个 Control 生成单项语义 descriptor factory；非裁剪全量与选中片段调用同一 factory。
ordinary generator 共享 ControlContract 值模型，或使用确定命名的 partial hook 衔接，不能读取另一生成器的输出反推事实。

每个专用 Semantic Style 是 owner 片段的独立保留条件。Style 可能只含 Nesting/route，不能假设使用它会自然保留 owner。
Control、Token、Semantic 和资产由同一注册过程收集；Semantic registry 随应用启动冻结，Style 构造函数不能追加注册。

保持原有专用 Style、selector route 与公开定制契约，不引入 code-behind 属性设置作为回退。
Semantic Part 的业务模型继续由 [Semantic Part 系统设计](../systems/theming/semantic-parts.md)拥有。

## 6. 资源作用域与动态输入

解析依赖只处理当前作用域、词法父作用域，以及明确的常量 ResourceInclude/MergedDictionaries，并保留声明顺序。
不搜索任意应用字符串或全包同名 key 来猜测来源。

包实现主题中的 StaticResource 必须在其声明的资源依赖范围内可解释；缺失或模糊来源报告位置与候选。
作者通过普通本地声明、显式 include 或公共 core 表达依赖，不添加 AOT 配置。顶层导出主题 key 不自动成为全包可见来源；
其他资产的字符串命名主题也必须通过普通 ResourceInclude 建立真实依赖。新增/重命名未 include 的同名资产不改变可见域。

一个严格的 typed default 例外保留正常环境主题替换：`{StaticResource {x:Type T}}` 编译为真实 `ldtoken T`；
只有已注册顶层默认导出的 key Type、TargetType、引用 Type 的完整身份完全相同，且提供者平台域覆盖消费域时，
才能用该 Type 对应的条件片段证明提供者会保留。此验证不插入局部字典，因此 Application 对默认 type-key 主题的
普通替换仍生效。任意 Type key/不同 TargetType 配对、字符串 key、缺失映射或不覆盖的平台域都不满足此例外。
局部同名资源可以合法存在且保持自身作用域，不能全局合并。Expander 的 converter 必须按预期在正常资源作用域提供，
不能任选其他主题中的 StringToTextBlockConverter 作为隐式依赖。

raw DynamicResource 按以下默认规则分类：

1. 本地或 include 可见的 key 属于该资源作用域。
2. 包 core 明确提供的 key 属于公共包资源。
3. 不在上述明确可见范围中的 key 记为 `ExternalDynamicKey`，按普通开放宿主查找处理，不自动激活控件。

宿主输入按普通主题/API 契约说明，不引入裁剪名单。未 include 的私有字典存在同名 key，只能提供疑似环境依赖的诊断线索，
不能证明依赖，不能单独导致构建错误，也不能覆盖合法宿主输入契约。
Common 的 TextControlSelectionHighlightColor 是此类开放输入的例子。

full/trimmed 等价性以相同宿主输入契约得到满足为前提；未使用私有字典不承诺继续充当隐式宿主资源。
动态 URI、运行时 AXAML 不在编译型主题自动精细保留承诺内，必须走已知静态工厂或明确的动态边界，不能扩大为全包兜底。

类型保留循环与资源构造循环不同：前者由官方链接器收敛；A include B、B 再 include A 是资源构造循环，必须在普通资源编译期
报错。运行时不能递归注册、重试或借 deferred wrapper 掩盖这个循环。

## 7. 资源提交与优先级

隐藏资源记录包含 AssetId、Phase、Order、factory。各包只暂存记录；全部包收集后统一校验，再创建和挂载。
跨包按真实 `PackageCommitOrdinal`，包内按 `(Phase, Order, AssetId)` 固定排序。
Bootstrap 补齐替换原包快照并保留 provider/ordinal；partial 包无完整 SchemaFingerprint。详见[注册分期](lazy-control-registration.md)。

Order 来自正常包资源契约和显式包含次序，不能来自 TypeMap 查询次序、candidate key 字母序、程序集扫描次序或所选控件数量。
嵌套 MergedDictionaries 的声明次序保存在 factory 内。默认主题 export key 冲突必须有明确覆盖契约，否则构建诊断。

同 AssetId 只挂一次；同 AssetId 对应不同 metadata/factory 必须失败。片段收集阶段不调用资源工厂，默认 typed theme
仍延迟到首次资源查找才构造，并按字典缓存。可静态调用的作者资源字典直接构造；精确默认 typed theme 的静态回调只
构造既有编译主题类并赋原 `TargetType`，不解释 AXAML 或提前物化主题。无资源类字典与不满足直接构造条件的资源继续
使用编译型 wrapper。包内资源排序不等于 Control 类型拓扑排序，不能重新引入运行时依赖图。

不为消除转发层而给作者的 classless 字典添加生成 `x:Class`：markup extension 可以观察原始字典根，替换根 CLR 类型
可能改变资源值。原始 AXAML、URI、Link、词法作用域、owner 生命周期与合并优先级保持不变；工厂构造策略不参与既有
资源 contract fingerprint，也不改变资产身份和排序。

非裁剪与裁剪执行中，共同保留资产的相对优先级和选中控件的观察结果必须一致；不要求裁剪产物包含全部未用资源。
跨包收集和冻结的总体时序由 [注册生命周期](aot-and-trimming.md#6-注册生命周期)拥有。

### 7.1 分级演化的身份和资源顺序

分级注册不改变上述顺序与冲突语义。跨阶段资源/查询目标由
[惰性物化与资源查询](../systems/theming/lazy-materialization.md)拥有；统一提交由
[分级启动演化](staged-startup-evolution.md)拥有。

- 每代 registry 的 Revision 证据必须覆盖该代实际贡献集合。空包集合、全指纹包以及混合
  手写包均适用；清除 request 去重键不能代替更新 schema Revision 和快照缓存身份。
- 当证据无法描述新增贡献时，先回退实际 schema 的完整计算；不得继续使用 Stage 1 的包列表生成 Stage 2 Revision。
- 包完整收集后提供完整指纹；真实裁剪路径根据实际 schema 计算，保留裁剪/平台选中集合的正确性。
- 数字 slot 只在单代 registry 内稳定，跨代共享必须校验 exact identity、CLR type、schema 与计算输入。
- 同一资产跨片段/跨阶段只挂一次；新增资源按完整排序键合并，且静态层相对根 Token provider 的优先级保持不变。
- 旧分区 ABI 保留签名，但两段在同次注册中完整收集，失败不发布半包；新生成代码使用 RegisterPackage，阶段选择在片段 Add 前执行。
  旧 generated package 的支持边界必须包含
  实际 descriptor/fingerprint 协议版本，不能仅根据方法仍存在就宣称所有旧二进制免重编。

注册优化优先复用现有 descriptor 和不可变集合。最终保留集合与 registry slot 由运行期实际组合确定；
不得用包级全控件 Type/factory 表破坏官方 TypeMap/条件后端的精细选择。

兼容边界（2026-10-12 验证）：本次资源键工厂是增量 ABI；同 schema 协议的消费端没有该工厂时，生成器回退
旧四参构造函数。它不扩展为任意历史 Generator/Core 混用承诺。以当前生成器直接引用已发布 Core 6.2.4
会缺少此前优化已经引入的 `SchemaPrefixState`、`CreateGeneratedPreSorted` 和 `PackageSchemaFingerprint`，
构建无法通过。Generator/Core 应使用匹配版本；pre-b355 生成资产的指纹协议和第三方包须分别核对、必要时重编，
不能仅凭旧构造函数仍在就宣称免重编。

## 8. 关键反例与验证

- Button 不因为 SearchButtonTheme 保留 SearchEdit；增加同目录无关控件不扩大集合。
- internal presenter 无 Token descriptor 时主题仍能应用；命名主题完整，多目标资产只挂一次。
- Token-only、Identity-only、Semantic Style-only 均保留正确 owner。
- raw→typed、typed→raw、正确 typed→错误 typed 及反序均保留 owner 信息或按规则失败。
- 字符串身份与 typed identity equality/hash 相同，真实 descriptor owner 不匹配仍失败。
- 同名局部 converter 不串用；显式 include 和覆盖顺序不因裁剪改变。
- 宿主提供 Accent 后，新增未使用且未 include 的私有同名字典，不改变合法构建结果或保留集合。
- 缺失包内 StaticResource 来源和 include 构造循环均有构建失败对照。
- Browser 不可用片段在工厂引用前进行平台判断，未用桌面 factory 不留在产物中。
- 跨包 RequiredTokenOwners 允许依赖稍后提交的包，但缺失或错误依赖必须在统一冻结前失败。
