# TypeMap 替换：类型与资源契约

> 过程记录：本轮结论已同步到 [正式 AOT 架构](../../architecture/foundations/aot-and-trimming.md)。本文保留方案比较、实验或迁移过程，不再作为平行的长期规范；事实以正式文档为准。


> 配套 [总体设计](2026-09-27-aot-typemap-replacement-design.md)。字段/API 为拟议形态，不表示已经实现。

## 1. 普通生成模型

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
  ExportedThemes[]                 // 导出 key + TargetType
  RequiredTokenOwners[]            // identity + 实际 owner Type
  Optional SemanticThemeBindings[]
  LexicalResourceScopes / ExplicitIncludes
  Phase / Order / schema fingerprint
```

类型模型与资源模型分开。删除 UnitId、OwnerUnitId、ReferencedUnitIds、目录归属、Package/Directory 粒度和应用依赖边。
Symbol 仅用于当前生成变换；稳定增量值使用 metadata name、值模型和路径，不缓存 Compilation/SemanticModel。

## 2. Control 与资源片段

发现可访问的 public/internal 非泛型 Control，包括 nested 类型；至少有主题/Token/语义内容才生成片段。
没有独立内容的派生控件无需空片段，依赖基类与实际 IL 引用。开放泛型控件使用已有非泛型主题 owner；无法确定 owner 时普通构建诊断。
需要从程序集级生成代码引用的私有类型不采用反射绕过；要求其主题契约具备正常可访问的 owner。

internal presenter 可以只有主题资源，没有 ControlTokenDescriptor。不要伪造公开 Token identity，也不要保留整个目录绕过校验。
已有公开 Global Token 配置/Own Token/Semantic 契约的控件继续有 descriptor。

## 3. 主题导出规则，防止错误反向保留

建立 Control → asset 关联的条件是该资产**导出**该 Control 的主题：

- ResourceDictionary 顶层 ControlTheme，或语义上继承 ControlTheme 的具体实例；读取它的 key 与 TargetType。
- 顶层命名主题与 `{x:Type ...}` 主题均支持。
- standalone 默认 typed theme 按明确默认主题契约生成 deferred wrapper。
- 嵌套 ControlTemplate.TargetType、局部定制主题不建立全局反向关联。
- standalone 非默认自定义 theme class 由实际构造引用保留；如需要 metadata，用 theme-class → asset-only fragment，不挂到无关默认 Control。

实际反例：`Input/Themes/SearchButtonTheme.axaml` TargetType 是 Button，却是 SearchEdit 的定制主题。
若据此把它挂到 Button 默认片段，会使普通 Button 反向保留 SearchEdit。新模型必须有这个负向回归。

现有命名主题不得丢失：TopLevelMenuItemTheme、TabItemTheme/CardTabItemTheme、TabStripItemTheme/CardTabStripItemTheme。
它们按顶层导出的 TargetType 随对应片段保留，不需要分析 C# 字符串查找。

三个实际多目标字典（CascaderViewFilterListTheme、TourTheme、TabOverflowMenuTheme）保持资产完整：任一目标保留 factory，
factory 引用其他目标，官方 linker 完成闭包。AssetId 去重保证只挂一次。

## 4. Asset schema 调整

现有 ControlThemeAssetDescriptor 的单一 OwnerIdentity 改为 ExportedThemes 与 RequiredTokenOwners。

- ExportedThemes 的 TargetType 不要求存在 Token descriptor。
- RequiredTokenOwners 必须存在正确的 descriptor，并验证 owner type 与 identity 一致。
- SemanticThemeBindings 明确 owner/property/target，不借文件目录猜归属。
- 校验发生于全部片段收集后，不受注册访问顺序影响。
- 更新 ControlThemeAssetManifest、ThemeSchemaRegistry 的验证与 fingerprint/revision；ThemeCompiler、ThemeScope 不需要因为此变更重写。

源码核查：OwnerIdentity 主要用于资产校验、筛选与 fingerprint，不是实际 Token 查找的作用域依据。
迁移保留 schema fingerprint 的业务作用，删除的是 Sidecar contract hash，不混淆两者。

## 5. Token identity 具有可验证的 owner

生成的身份使用真实类型数据：

```csharp
public static readonly ControlTokenIdentity Identity =
    ControlTokenIdentity.ForControl(typeof(Button), "AtomUI", "Button");
```

ForControl 保存 owner Type；Core 在 schema 查找、资源键解析和配置校验时实际验证它，不是空 typeof 防裁剪占位。
owner 信息不成为序列化后的配置格式，不暴露为需要用户填写的字段。

必须显式保持字符串身份相等语义：Equals、GetHashCode、==、!= 只比较 Catalog/Id。不能在现有 record struct 上简单增加
Type 自动属性后继续依赖自动生成 equality。

`new ControlTokenIdentity(catalog,id)` 仍是配置/查找数据，只匹配已保留 schema，不承诺任意字符串是动态 root。
同一字符串 identity 携带冲突 owner Type 时在使用/合并时明确报错，不悄悄接受第一个。

校验必须发生在 identity-keyed 去重之前。ThemeConfigBuilder.WithControl、配置规范化/合并、RequiredTokenOwners 合并以及
所有相关 Dictionary/Distinct 路径共用 canonicalization：两边都有 owner 且不同则失败；仅一边有 owner 时提升为带 owner 的
canonical key，必要时移除原等值 key 后重新插入。Dictionary 默认保留原 key，不能只替换 value 后寄望最终校验发现冲突。
raw→typed、typed→raw、正确 typed→错误 typed 和反序都要测试；最终 registry 仍验证 owner 与真实 descriptor 一致。

Token 扩展的 global 与 own 分支均生成带 owner 的资源键；Own 分支不再仅返回无上下文 enum。
继续支持直接 boxed enum 资源键，因此映射保留条件包含 Control、TokenResourceExtension、TokenKey、OwnTokenKind。
把 enum 转成裸整数或做常量运算不意味着使用控件资源，不需要保留控件。

## 6. Semantic Part 同一事实源

每个 Control 生成一个语义 descriptor factory；full 与选中片段调用同一 factory。
ordinary 生成器共享 ControlContractModel，或用确定命名的 partial hook 衔接，不让生成器读取另一个生成器的输出。

每个生成的专用 Semantic Style 也是 owner 片段的 trimTarget。当前这些 Style 可以只含 Nesting/route，没有 owner 类型引用，
不能假设样式类型使用会自动保留 owner。

builder 同步收集 Control/Token、Semantic、asset。Semantic registry 在启动冻结；不能在 Style 构造函数中追加注册。
现有专用 Style 与 selector 路由不改变，不引入 code-behind 样式回退。

## 7. 普通 AXAML 作用域就是资源依赖契约

资源解析只处理本地作用域、词法父作用域和明确的常量 ResourceInclude/MergedDictionaries，保留其声明顺序。
同包纯辅助资源由显式 include 或正常 Provider core 引用，不发明虚拟 Control，也不依赖整个包恰好已注册。

包实现主题中的 StaticResource 必须在其声明的资源依赖范围内可解释；无法解释的包内来源报告位置与缺失/模糊候选。
DynamicResource 的默认分类无需 AOT 配置：本地/include 可见 key 属于该资源作用域；包 core 明确提供的 key 属于公共包资源；
不在这些明确可见范围中的 raw DynamicResource，按普通开放宿主查找处理，不自动激活控件。生成模型将它记录为 ExternalDynamicKey，
外部宿主输入在文档/API 中作为普通主题输入说明，不引入裁剪名单。
其他未 include 私有字典存在同名 key，只能作为疑似环境依赖的诊断线索，不能证明依赖或单独导致构建错误；合法宿主输入契约优先。
确实需要某个包内资源的实现，应通过普通 include、本地声明或公共 core 表达，不能把 full 模式偶然挂载它当成可靠契约。
full/trimmed 等价性以相同宿主输入契约得到满足为前提，不承诺未使用私有字典继续充当隐式宿主资源。
动态 URI/运行时 AXAML不属于编译型主题的精细自动保留承诺，必须走已知静态工厂或显式动态边界。

源码实查：Desktop 338 份 Theme AXAML 中，42 个字面量 Static/DynamicResource 引用有 37 个本地可见、4 个由显式 include
提供；Expander 的 StringToTextBlockConverter 是一个实际环境依赖。该 key 在十个其他字典中存在且配置不同，应在 Expander
按预期声明本地资源，不能任选一个同名字典作为隐式依赖。

Card/CardActionPanel 的同名 CornerRadiusFilterConverter 配置也不同：局部重复 key 可以合法，禁止把它们全局合并。
Common 的 TextControlSelectionHighlightColor 是宿主 DynamicResource 示例，不能误报成内部资源丢失。

## 8. 资源优先级

隐藏注册记录包含 AssetId、Phase、Order、factory；各包只收集，全部包收集后统一校验并创建/挂载。
跨包按真实 PackageCommitOrdinal 保持调用产生的顺序；包内固定排序为 `(Phase, Order, AssetId)`，full 与 TypeMap 使用同一规则。Order 来自正常包资源契约/显式包含次序，不来自
TypeMap 字典、candidate key 顺序、程序集扫描次序或被选中控件数量。

显式嵌套 MergedDictionaries 的次序保存在 factory 内。默认控件主题的 export key 冲突须有明确覆盖契约，否则构建诊断。
多个片段引用同一 AssetId 只挂一次；同 AssetId 的元数据/factory 定义冲突报错。
deferred wrappers 继续保留，避免收集阶段提前解析尚未挂载的资源。

等价性口径：full 与 trimmed 共同保留资产的相对优先级相同，选中控件观察结果相同；不要求 trimmed 包含所有未使用资源。

## 9. 必须具备的反例测试

- Button 不因为 SearchButtonTheme 保留 SearchEdit。
- 增加同目录但未使用控件，原保留集合不变。
- internal presenter 无 Token descriptor，主题仍可应用。
- TabItem 的两个命名主题均可查找；Tour 多目标资产只挂一次。
- Token-only、Semantic Style-only 与 generated Identity-only 正常激活 owner。
- 字符串 identity 的 equality/hash 与 typed identity 相同；错误 owner 校验失败。
- 同名局部 converter 不串用，显式 include 与覆盖次序稳定。
- 外部 DynamicResource 可以由宿主提供；缺失包内 StaticResource 不被 full fallback 隐藏。
- 宿主提供 Accent 后，新增一个未使用、私有同名 Accent 字典，不使原合法主题构建失败或改变其保留集合。
- 注册类型保留的循环由官方链接器收敛，运行时片段不递归注册。
- A.axaml include B.axaml 再 include A.axaml 是资源构造循环，普通资源编译应诊断，不能声称 linker 能解决运行时递归。
- Browser 禁用片段不通过先构造 descriptor/factory 再过滤，意外保留桌面资源。

## 10. 平台可用性也必须进入静态代码

现有 Browser exclusion 表和平台 Provider 选择属于产品契约，迁移时保持其行为。
将可静态声明的控制类型平台限制整理为普通平台可用性元数据（本包类型优先采用 .NET Supported/UnsupportedOSPlatform
声明；外部类型的包级主题覆盖由包自己的平台资源声明表达），由生成器写入对应片段方法内的真实平台分支。
一般跨平台控件不需要作者增加配置。

重要顺序：先判断平台，再引用/构造该片段的 descriptor、语义工厂和资源工厂。单纯在公共 builder 的运行时 predicate
中排除已经构造的记录，不能让链接器删除其工厂。即使 TypeMap 因一个静态类型引用保留了代理，代理内不可达的平台分支也应可删除。
任一资产同时导出不兼容的平台目标而无法形成一致 factory 时，必须拆为正常平台资源或诊断，不能悄悄丢掉其中一个可用主题。

## 11. 需要修改的主要位置

- `DesignToken/ControlThemeInfo.cs`：用完整 ControlContract 取代 public-only/目录 Unit 模型。
- `ResourceKeyClassWriter.cs`、`ControlTokenIdentity.cs`、`ControlTokenResourceKey.cs`、`ThemeConfigBuilder.cs` 及配置规范化/合并：真实 owner 数据与去重前 canonicalization。
- `ThemeAssetInfo.cs` / `ThemeAssetManifestGenerator.cs`：语义化导出、作用域/包含关系，不扫描任意应用字符串。
- `GeneratedThemeSchemaWriter.cs` / `SemanticPartManifestWriter.cs`：逐项共同 factory、trigger 与 proxy。
- `ControlThemeAssetDescriptor.cs` / `ControlThemeAssetManifest.cs` / `ThemeSchemaRegistry.cs`：新资产校验/fingerprint。
- 旧 AotTrim builder 改为中性的 ControlPackageRegistrationBuilder；统一语义收集和资源提交。
