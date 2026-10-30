# 统一事实、条件元数据与注册 ABI

本文为[总体方案](overview.md)的契约设计。用户批准实施后，format 1 的具体字段、编码与方法模板已收敛到
[条件注册 ABI](../../../reference/aot/conditional-registration-contract.md)；发布和平台验收仍按本方案门槛推进。

## 1. 事实模型

沿用现有 ControlContract、ControlRegistrationFragment、ThemeAsset 和 PlatformAvailability 的事实来源。
新增的是后端中立的关联视图，不是应用依赖图：

```text
PackageFact
  PackageId / DefiningAssemblyIdentity / GroupIdentity
  RegisterEntry / EnsureEntry / CollectEntry
  Fragments[] / Conditions[] / ContractVersion

FragmentFact
  FragmentId / DefiningAssemblyIdentity
  ProxyTypeIdentity / StaticRegistrationMethodIdentity / CollectionOrderKey
  ControlIdentity? / TokenFactory? / SemanticFactory?
  AssetReferences[] / PlatformAvailability

ConditionFact
  ConditionId / GroupIdentity / FragmentId
  TriggerTypeIdentity / TriggerKind
```

TriggerKind 只说明 Control、Token、Style、Identity-owner 等来源，不能自行决定是否保留。
所有条件最终使用正常 CLR 类型证据，交给真实引擎判断；不把常量枚举值、任意字符串、目录名或包引用当作类型使用。

片段 Add 的实际 IL 承载依赖。不得另行从上述 AssetReferences 建立替代 ILC/ILLink 的近似闭包；它们用于生成、校验与归因。
依赖事实与注册事务仍服从[控件注册契约](../../../architecture/foundations/control-registration-contracts.md)。

## 2. 稳定身份与规范化

| 对象 | 身份要求 |
| --- | --- |
| 程序集 | Name、Version、Culture、PublicKeyToken 的规范化完整身份；实际实现文件再绑定内容散列 |
| 类型 | 定义程序集身份 + namespace/nested metadata name + 递归 TypeSpec；不以简单名匹配 |
| 方法 | 定义类型 + name + generic arity + calling convention + return/parameter types |
| Group | PackageId + 定义程序集 + 生成 Group 身份；包内确定、全图无冲突 |
| Fragment | 现有片段身份加所属 Group；多个条件指向同一片段时只执行一次 |
| Condition | Group、触发类型、片段及契约版本的确定性标识 |

身份解析必须区分引用程序集与实现程序集：先解析正常 type forwarding，再验证最终定义身份和合法引用来源。
不要求 ref/implementation 文件 hash 相同；它们的职责与身份配对必须记录。冲突不能用“第一个匹配”解决。

身份系统分成三层：原始声明身份、正常 forwarding 解析后的定义身份、当前引擎的条件键。后端不得用序列化字符串相等
代替引擎条件判断。方法签名需要描述完整 TypeSpec，但“能够描述类型”不自动意味着该类型可作为候选条件。

候选 format v1 沿用当前控件注册范围：条件为可访问的非泛型 named/nested 类型，嵌套 owner 也不得含未绑定泛型参数。
闭合泛型 Control 仍通过现有可确定的非泛型主题 owner 表达；不新增开放泛型控件契约。此限制对两个 TFM 的普通生成器一致生效，
不是 net8 专属降级。应用 IL 中的泛型、数组、byref 等真实使用仍必须由引擎完整分析。

| 输入 | 定义与后端投影规则 |
| --- | --- |
| 非泛型 T / nested T+N 候选 | 完整定义身份；ILLink 使用正确相关类型事件，ILC 使用 NecessaryTypeSymbol(T) |
| facade 转发的 T | 先验证合法 forwarding 链并归一到定义 T；同一类型不得重复选择，非法身份冲突失败 |
| 应用中的 G<T> / G<U> | 保留完整不同实例输入，不能为了注册合并成 G<>；由引擎判断其中候选 T/U 是否具有相关使用 |
| 应用中的 T[]、T&、T* | 不直接把外层结构等同于 T 条件；依照当前引擎对元素、类型检查、实例化/反射的实际规则 |
| 候选记录直接声明 G<int>、array、pointer/byref | format v1 明确拒绝；若将来扩展条件域，需同时定义两个后端的投影和 .NET10 基准后升级协议 |
| 裸整数枚举常量 | 没有类型条件证据；不能凭数值恢复 owner |

泛型参数晚到达、canonical sharing、去虚拟化和未装箱值类型属于必验的应用使用语义，不能因候选域非泛型而跳过。

输出按稳定身份排序，不能依赖文件枚举顺序、绝对路径或编译线程调度。Hash 只做校验/索引，碰撞必须核对完整身份。

## 3. net8 元数据传输

采用程序集级构建专用记录，建议新增隐藏生成 ABI：

```csharp
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ConditionalRegistrationRecordAttribute : Attribute
{
    public ConditionalRegistrationRecordAttribute(int format, string kind, string identity, string payload);
}
```

这是签名设计，正文不是可编译实现。`format=1`；kind 限定 `package`、`fragment`、`condition`。
identity 为记录稳定身份；payload 为规范 UTF-8 JSON 字符串。每条记录独立，避免全包巨型单字符串。
字段名称、必需性和最大长度作为 schema 定版，超出边界给出诊断；不能因此回退全包。

示例 condition payload：

```json
{
  "group": "Acme.Controls/default",
  "fragment": "Acme.Controls/Button",
  "trigger": {
    "assembly": "Acme.Controls, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
    "metadataName": "Acme.Controls.Button"
  },
  "kind": "control"
}
```

package 记录包含定义程序集、Group、收集入口完整签名、CollectorTemplateId、规范语义方法体契约、记录集合摘要及所需后端能力。
fragment 记录包含当前 self-attributed proxy 身份、唯一静态注册入口、收集顺序键和片段身份；condition 只指向 fragment。
示例中的 group/fragment 是当前定义程序集和 package 记录内的局部 ID；解析后必须成为完整复合身份，不能全局按字符串碰撞合并。

Group CLR 类型、Collect/CollectSelected、proxy 和 thunk 必须定义在记录所属程序集内；候选 trigger 可以引用合法外部定义。
禁止把外部 internal 方法指定为本包 thunk。跨包依赖由正常入口/可访问 IL 引用表达，不能用元数据绕过 CLR 可见性。

Source Generator 不生成最终 IL hash：它尚未看到 C# 编译和 AXAML 后处理结果。CollectorTemplateId 标识允许的结构模板，
后端在解析成员身份后验证调用目标、参数流、分支与异常区域等语义结构，只允许协议规定的 Debug nop/局部变量等差异。
实际编译后 IL/程序集内容 hash 由 publish 快照计算。记录集合摘要按规范记录排序，排除其自身摘要字段后计算，避免自引用。

记录不包含 `System.Type` 参数、RuntimeTypeHandle、factory delegate、代理实例或应用已用列表。
反序列化严格拒绝重复 JSON 字段、重复/冲突记录、未知格式、超长输入、非法身份和缺失目标。
格式 1 不静默忽略未知语义字段；将来的可选扩展先通过版本协商定义。

生成器直接写入 C# assembly attributes，因此无需“先编译再生成包 Sidecar”。冷 NuGet 和预编译 DLL 自带候选定义。
后端读取正常编译引用图中的记录，不扫描任意磁盘目录；正式分析前隔离这些 attribute 实例，避免默认 attribute
处理造成额外根。非裁剪运行不读取或实例化它们。发布输出应移除已消费记录，诊断信息只写构建回执。

该 Attribute 技术上属于跨程序集可见类型，虽然隐藏于正常 IDE 使用，也必须纳入 API/ABI 版本和迁移审计。
已有 net10 TypeMap ABI v1 保持原状；net8 ConditionalRecord format v1 与它独立定版，不共享隐含兼容假设。

## 4. 强类型静态注册入口

每个 net8 片段生成唯一 thunk：

```text
internal static void Register_<StableId>(ControlPackageRegistrationBuilder builder)
```

thunk 通过 `builder.AddFragment(new Proxy())` 进入现有校验核心，再执行当前片段逻辑，收集 Token、Semantic 和资源记录。
它不实例化控件、不挂载主题、不执行包 initializer；不通过反射查找方法或构造器。不能直接 `new Proxy().Add(builder)`
绕过 FragmentId 校验、不同 proxy 冲突、去重和失败状态。若实施需要专门的无反射入口，也必须复用同一校验/失败核心。
平台 guard 必须在 descriptor/factory 引用之前，与 net10 使用同一平台事实。

每个 Package 生成受控 Collect 入口与完整/选中实现：

```text
Register / Ensure -> ControlRegistrationRuntime -> Collect(builder)
normal Collect -> CollectFull(builder)
trim/AOT owned rewrite -> CollectSelected(builder)
```

普通构建保留完整注册。实际裁剪发布必须在分析前将已识别 Collect 的 IL 切换到选中路径，断开完整集合的根；
net8 不依赖运行时不存在的 FeatureSwitchDefinition 特性，也不依赖产品包的 DEBUG 宏选择消费方发布行为。

原始 CollectSelected 使用不可当作有效结果的 fail-closed stub。后端对原始签名、marker 和 CollectorTemplateId 的结构契约校验后，
才在分析视图中提供中性方法体，在最终输出中物化选中结果。未知方法体、不完整改写或可达残留 stub 必须失败。
空集合是合法结果，但必须带有本次成功分析证据；不能把没有加载后端误当成空集合。

## 5. Group、顺序与去重

请求 Group 指该发布引擎证明其收集入口可达，不表示运行时每条路径都会执行。
引用包、解析 metadata 或仅存在 Group CLR 类型，不得请求 Group。真实 UseXxxControls 调用与其分支仍由官方分析器处理。

同 Group 多条件采用 OR；同一 fragment 只进入结果一次。跨 Group 不擅自合并，即使方法名相似。
片段收集顺序键由共同生成事实按现有 emitter 的规范顺序产生；两个后端使用同一顺序，不另行按 hash/key 大小排序。
运行时资源优先级仍按既有 PackageCommitOrdinal、Phase、Order、AssetId 定义。
片段顺序不得替代资源声明顺序。正式等价验证需证明去重不改变包初始化、错误和资源覆盖行为。

## 6. 包级生命周期与故障

保留现有顺序：检查 builder/重入 → prepare 依赖包 → 创建 provider → 收集选中片段 → 提交包记录 → complete。
全部包收集后统一验证 owner、schema、资源冲突，物化资源并冻结 registry。

可选包未启用时不能因为候选定义而创建 provider、语言服务或 initializer。
同包重复注册、跨包失败传播、失败 builder 不可复用、dispose 与异常包装保持当前契约。
后端仅改变收集集合，不能把运行时错误改为吞错、重试、延迟注册或按控件首次使用补注册。

## 7. 动态边界

正常 .NET trimming roots、DynamicallyAccessedMembers 和明确的反射依赖声明由实际引擎处理；不再要求用户维护 AtomUI 控件列表。
任意运行时插件、动态程序集和动态 AXAML 不具备无声明的静态完备性。沿用既有动态输入契约，必要时明确诊断；
不能假称支持，也不能默默扩大为全包注册。两个 TFM 在同样的动态前提下接受同样的业务输入。
