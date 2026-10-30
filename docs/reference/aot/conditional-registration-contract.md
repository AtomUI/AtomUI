# .NET 8 条件注册生成 ABI

本文拥有 ConditionalRecord format 1 的具体编码、成员模板及校验规则。生成端与读取端已在工作树实现；
这不表示发布 targets、NuGet 分发或全部平台已经验收。整体方案与交付门槛见
[多目标 AOT 设计](../../superpowers/specs/2026-10-05-aot-dependency-analysis-design/overview.md)。
已有 .NET 10 [TypeMap ABI](typemap-contract.md)保持独立。

## 1. 载体与身份

每条候选事实写成程序集 Attribute：

```text
AtomUI.Registration.ConditionalRegistrationRecordAttribute(
    int format, string kind, string identity, string payload)
```

该类型定义在当前解析的 AtomUI.Core 中。读取器核对其完整程序集身份及精确构造签名，直接读取 PE metadata；
不得加载消费程序集或执行 Attribute 构造函数。format 固定为 1，kind 只能为 package、fragment、condition。
命名参数、未知版本、重复 kind/identity、缺失字符串均失败。

一份程序集最多 100,000 条记录；单个 identity 最多 65,536 个 UTF-16 code unit，payload 最多 1,048,576。
这些上限是编译期协议边界；超限不能扩大保留集或忽略记录。

类型身份由以下两个字符串构成：

```json
{"assembly":"Demo, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null","metadataName":"Demo.Host+Button"}
```

assembly 必须为规范完整身份，包含 Name、Version、Culture、PublicKeyToken。嵌套类型使用 `+`。
格式 1 候选域为非泛型 named/nested 类型；不接受泛型、数组、指针或 byref 候选。
应用中的泛型、数组和动态依赖仍由实际引擎完整分析，不能将候选域限制误读为忽略这类应用 IL。
完整 AQN 是 `metadataName + ", " + assembly`，不是运行时反射查找指令。

## 2. 方法身份

方法对象必须恰好包含：

| 字段 | 值 |
| --- | --- |
| declaringType | 完整类型身份 |
| name | 实际成员名 |
| genericArity | 整数 0 |
| callingConvention | `static` |
| returnType | 正常编译绑定的 System.Void 完整身份 |
| parameters | 一个元素：当前 Core 的 ControlPackageRegistrationBuilder 完整身份 |

读取器验证结构；绑定器进一步验证实际方法定义、CLR 可见性、所属程序集、正常 forwarding、返回值和参数。
仅检查名字为 System.Void 或 builder 不足以证明实际签名正确。

## 3. 三类记录

### Package

每个 format 1 定义程序集恰好一个 package。identity 为 Group 的完整 AQN。

| payload 字段 | 规则 |
| --- | --- |
| packageId | 普通产品包身份 |
| assembly | 当前定义程序集完整身份 |
| group | 本程序集的 Group 类型身份 |
| collect / collectFull / collectSelected | 同一生成承载类型的三个方法身份 |
| collectorTemplateId | `atomui.collector.v1` |
| requiredCapability | `atomui.conditional.v1` |
| recordsHash | 第 4 节定义的 SHA-256 小写十六进制值 |

### Fragment

identity 为 `group AQN + "\n" + fragmentId`。

| payload 字段 | 规则 |
| --- | --- |
| group | 所属 Group 完整 AQN |
| fragmentId | 已有 `packageId:control AQN` 运行时片段身份 |
| proxy | 同一程序集已有 self-attributed 片段代理 |
| registrationMethod | 同程序集强类型注册 thunk |
| order | 从 0 开始的连续整数，沿用共同 Controls 事实顺序 |

不得复用 proxy、thunk 或 order 表达不同片段；thunk 也不能与任一 collector 别名。

### Condition

identity 为 `fragment identity + "\n" + trigger AQN + "\n" + kind`。

| payload 字段 | 规则 |
| --- | --- |
| group | 所属 Group 完整 AQN |
| fragment | 本程序集已声明片段记录的 identity |
| trigger | 条件类型完整身份，可来自合法外部定义 |
| kind | `control`、`token` 或 `style`，只描述来源 |

每个片段至少一个条件。相同片段的多个条件为 OR，运行时只收集一次；kind 本身没有根化语义。
所有跨记录关系都要校验，不能将不存在的片段当空集合。

## 4. 规范 JSON 与摘要

所有对象字段按 ordinal 名称排序，数组保持语义顺序，不输出空白。
字符串只转义双引号、反斜杠和 U+0000–001F；控制字符统一用小写 `\uXXXX`，其他 Unicode 原样编码为 UTF-8。
format 1 仅使用对象、数组、字符串和 int32 数值；拒绝重复字段、未知字段、null、布尔值及浮点数。

摘要输入为按 ordinal `(kind, identity)` 排序的记录数组，每个元素为：

```text
{"format":1,"identity":...,"kind":...,"payload":{...}}
```

仅在计算摘要时删除 package.payload.recordsHash 字段，再对规范 JSON UTF-8 字节计算 SHA-256。
摘要不包含自身，不是编译后 IL hash；实际程序集/IL 内容身份由 publish 输入快照记录。

## 5. 生成成员模板

```text
Collect(builder)         -> CollectFull(builder)
CollectFull(builder)     -> 按 order 调用所有 Register_<stableId>(builder)
CollectSelected(builder) -> throw InvalidOperationException(固定未物化诊断)
Register_<stableId>      -> builder.AddFragment(new 对应Proxy())
```

完整/选中路径共用 thunk 与片段的 Add 实现。不得直接调用 Proxy.Add 绕过 builder 的去重、冲突和失败状态校验。
平台 guard 留在片段中，在 descriptor/factory 引用之前执行；不改变主题与资源的原有业务事实。

绑定器在改写前核对这些方法体的调用目标、参数流和异常区域，并验证 proxy 继承及 self Attribute。
允许协议规定的 Debug nop 等无语义差异；不接受任意额外调用、重入分支或已经物化过的入口。
发布分析前将 Collect 切换到选中路径并断开完整集合根；没有有效分析结果时，不能把 selected stub 变成成功的空注册。

## 6. 发布输入快照

构建端冻结最终实现程序集，产生独立输入清单：

```text
format = 2
coreAssemblyIdentity = 当前 Core 完整身份
assemblies[] = { path, assemblyIdentity, sha256 }
additionalInputs[] = { path, sha256, kind }
analysisReportPath = 本次独立输出域中的报告路径
```

全部路径为绝对快照路径。清单覆盖 SDK 最终实现输入，不能只枚举链接器当时已加载的缓存。
绑定器核对文件 hash、完整身份与实际 resolver 消费内容；读取候选定义本身不能将它们标记为应用根。
format 1 的四字段输入仅属于早期机制实验，不能获得正式 SDK 的 verified/consumed 回执。

additionalInputs 的 kind 为 root-descriptor、reference、custom-step、custom-step-file、tool、symbol、configuration 或 opaque。
链接器实际读取的根描述符、引用和自定义步骤必须重定向到已冻结文件，不能仅复制一份作日志证据。
自定义步骤用显式 AtomUIBundleRoot 声明依赖目录，冻结时保留相对布局；不猜测未声明的磁盘依赖。
opaque 输入用 CustomDataKey 对接实际参数；SDK 自己生成的 link-attributes 参数按结构化输入重建。
未声明的路径型额外参数明确失败，不允许回到可变原路径。

同一内容的程序集快照可共享，PDB 与对应 DLL 保持邻接；每次分析具有独立的 run 目录、报告和 linked 输出。
PDB 内容、归属、额外规则和工具输入均参与内容身份。创建缓存的短锁不替代实际分析输出的隔离。

编译输出域先按项目/配置/TFM/RID 隔离；编译完成后才计算发布内容身份。内容、相关配置与额外工具/规则输入变化使缓存失效。
快照只证明 prepared 输入，不等于成功分析、最终产物验证或部署消费。Mark 回调中的 analyzed-materialized 报告也不是最终回执。

独立输出检查重新读取已链接程序集，核对 Collect→CollectSelected→所选 thunk 的实际调用与顺序，拒绝残留候选记录，
并绑定输出内容 hash。普通 CoreCLR 发布复制完成后再次对比实际部署 DLL，才写 consumed。
单文件、ReadyToRun、NativeAOT 和 Browser 的转换链需各自的消费证据，不能复用普通 DLL 复制的完成状态。

## 7. 发布模式 getter

net8 的正常 Core getter 仍使用 `AppContext.TryGetSwitch("AtomUI.Registration.Trimmed", out value) && value`。
真实裁剪/AOT 发布后端通过 `atomui.trimmed-switch.v1` 校验完整 Core/BCL 身份、签名和四种输入状态下的布尔语义，
再在发布视图中将 getter 规范化为 true。这样不依赖迟于 runtimeconfig 生成的 MSBuild setter，也不修改普通 build 的程序集。
未知 getter 模板、额外调用或不等价返回规则会被拒绝；net10 保留官方 FeatureSwitchDefinition 路径。
