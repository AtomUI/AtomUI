# 真实依赖分析与后端设计

入口：[总体方案](overview.md)。事实与 ABI 由[契约文档](contracts.md)定义。

## 1. 闭包模型

设 E 为当前发布引擎，R 为入口、框架、声明式动态 roots 等正常根，G 为 Group，F 为片段，C 为条件。
选择规则为：

```text
SelectE(G, F) = RequestedE(G) AND Any(RelevantE(C), C belongs to G/F)
DE = least fixed point of OfficialDependenciesE(R + selected fragment entrypoints)
```

RelevantE 是引擎语义，不等于“类型名出现在输入”“有一条元数据引用”或“Annotations.IsMarked 为真”。
ILLink 与 ILC 的具体类型节点不同。所有后端必须遵守相同产品关联，但不得混用两个引擎的中间状态。

计算是当前引擎图内的单调标记：片段被选择后，其正常 IL 依赖再次进入引擎工作队列。
有根循环收敛；无根循环不自启动。禁止把多次重写程序再编译的结果并集冒充这个最小闭包。
不实现另一套 Control/Asset 依赖遍历器，也不从旧 Sidecar usage 推测闭包。

## 2. 引擎适配接口

以下为责任接口，不承诺直接暴露给用户：

| 操作 | 责任 |
| --- | --- |
| LoadAndValidateDefinitions | 只读取候选定义，解析完整身份，验证 schema、方法体和后端能力 |
| NormalizeCollectEntry | 实际 publish 分析前切换受控入口，断开 full roots |
| ObserveGroupRequest | 从引擎证明的入口可达性激活 Group |
| BindConditionalEdges | 将条件和片段接到引擎自身节点/事件 |
| CloseGraph | 由引擎处理新增片段 IL，直到其正式闭包阶段结束 |
| MaterializeSelected | 仅写已经选择并完成分析的调用/数据 |
| VerifyConsumedOutput | 检查已选择项、未用项、残留 stub、身份和最终消费路径 |

候选解析本身不得标记条件、片段或工厂。解析定义引用与保留代码必须严格分离。
请求顺序、条件先到达/Group 后到达、Group 先到达/条件后到达必须得到相同结果。

## 3. .NET 8 托管裁剪

### 3.1 阶段

```text
ResolveReferences / compile
  -> resolve exact ILLink8/backend
  -> pre-Mark: validate records, isolate metadata, normalize collectors
  -> Mark: group requests + relevant-type events + selected factories
  -> official fixed point
  -> pre-Sweep: selected dispatch materialization
  -> Sweep / Output
  -> reread output + verify + receipt
```

优先采用定版 ILLink8 MarkStep 派生扩展；普通 IMarkHandler 的一次类型回调不足以覆盖全部条件状态变化。
需要覆盖实际实例化、反射可见、类型检查、数组元素、泛型相关状态、晚到达属性与对应的额外处理阶段。
所有 hook 必须与目标引擎的生命周期对应，保持 base 行为和工作队列处理顺序。

若完整语义所需通知不可通过现有扩展点可靠获得，采用小范围定版 linker patch。不能用周期性 IsMarked 扫描近似，
不能在模型无法表达时选择全部候选。扩展与补丁的界限由语义矩阵证明，不由“少改几行”决定。

### 3.2 标记与物化

选中的强类型 thunk 使用正常 MarkMethod 加入引擎。实例化、虚调用、泛型、字段、factory 和编译后的 AXAML 继续走官方逻辑。
每个选中方法必须已完成分析，才能进入物化集合；不允许仅记录 selected=true 而跳过其依赖。

物化时按去重后的片段顺序发出 `ldarg builder; call Register_<id>`。辅助构造、异常、dispatch 方法签名和所需类型必须已在
Mark 阶段被分析。不能在 Mark 后新增未知 helper，不能在 Sweep 后补依赖。
输出后重新读取程序集；不得仅复用 Sweep 前 Cecil 对象或检查内存中的“已选”集合。

仅静态方法使用的类型即使在产物中存在，也不必激活条件映射。类型仅因签名/泛型元数据保留与实际相关使用必须分别测试。

## 4. .NET 8 NativeAOT：scanner 路径

### 4.1 条件节点

复用固定版本 ILC 的真实依赖分析器，增加自有条件节点：

```text
declaration node
  -- conditional on reachable CollectSelected method --> group node
group node
  -- conditional on NecessaryTypeSymbol(trigger) --> registration thunk entrypoint
thunk body
  -- ordinary ILC dependencies --> more type/method/asset nodes
```

declaration node 可以作为轻量构建根，但其无条件依赖中不得出现候选类型、thunk 或 full collector。
条件表保存 TypeDesc/MethodDesc 是编译器内索引，不代表将它们加入 Mark；此边界需专门负例验证。

Relevant8 的映射以官方 ILC 条件节点为依据。多条件到同一 thunk 合并；同名不同程序集必须独立解析。
Group 未请求时，即使 trigger 已有运行时类型节点，也不能使对应片段进入闭包。

### 4.2 scanner 与代码生成之间的契约

1. 入口规范化、条件根和对应 ILProvider 在 scanner 启动之前安装。
2. scanner 完成自身固定点后，读取实际条件节点和 thunk 标记状态；不得从诊断字符串推断类型。
3. 任何已选 thunk 未被 scanner 完整分析都视为后端缺陷并中止。
4. 将已选集合冻结，将 CollectSelected 特化为普通强类型调用；不再次依据应用源码生成使用列表。
5. codegen 消费同一输入快照与冻结结果，编译普通 net8 调用。阶段间数据必须绑定源码/工具/输入 hash。
6. 最终检查方法、factory 与资源保留结果；不能把报告成功等同于原生二进制正确。

此方法与官方 .NET 10 的“scanner 分析后冻结映射”阶段职责一致，但两版内部节点实现不同，不能机械复制节点类名称。
不同优化结果必须按产品条件和具体引擎语义审查，不能自行添加保守整包根。

### 4.3 预初始化屏障

原型已证明：若解释器将中性注册入口当成空操作，静态构造函数可能被错误预计算，Group 请求随之消失。

正式后端必须给所有相关 PreinitializationManager 使用专用视图：当解释执行到受控选中入口时，停止对该路径预初始化。
scanner 仍获得分析用方法体，codegen 获得最终方法体。屏障仅作用于受控入口及其解释到达路径，不全局关闭预初始化。

覆盖直接 cctor、cctor→helper、generic cctor、跨程序集 cctor、早期/后期 manager、嵌套异常和静态字段初始化。
不允许用延时执行、隐藏副作用或强制所有 cctor 运行来替代此契约。

## 5. 无 scanner codegen 路径

这一模式**不由现有原型证明，但属于完整交付的必需项**。scanner 开关与优化配置独立建模，不能将无 scanner 简化为 Debug。
当前原型拒绝 `--noscan` 是实验安全边界，不是最终产品策略。
正式实现不能为了获得声明上的支持而全局强开优化、关闭调试能力或把空入口当作正确结果。

设计方向是把同一条件关系接到实际 codegen 依赖图，并让注册数据在图稳定后输出。因为 accessor 的机器码可能早于图收敛生成，
不得沿用“生成后重写方法体”的方式。拟定为原生 dispatch table：

- 在图分析前安装不含候选引用的固定调用循环 IL/模板；循环及 helper、managed calli、表地址 intrinsic 的依赖由本次 codegen 图正常处理。
- Group 查询使对应数据节点可达；片段入口仍是由真实条件节点激活的条件依赖。
- 数据节点只在实际闭包稳定后的输出阶段写入已选强类型注册入口地址。
- helper 使用既定 managed calling convention，所有 thunk 必须具有精确的 builder 参数/void 返回签名。
- 条件被选中时同时建立目标平台需要的可取地址入口、调用约定及调用支持依赖；不能只检查普通方法体已经生成。
- 表不构成选择依据；它是最终闭包的输出。其重定位只能指向上述已分析入口，输出阶段不能创造新根。

拟定原生数据 ABI：版本、pointer width、entry count 和已选入口重定位序列；字段对齐、endianness、地址生命周期、异常传播、
GC 安全及 managed calli 由 host/target 组合明确规定。相同 ABI 可作为 scanner 路径未来统一输出形式，但首轮不为统一而重写
已验证的静态 dispatch。

**此原生表与 intrinsic/地址获取接线尚未验证。** P1 必须在官方 net8 CoreLib 不变的前提下，证明无 scanner 正反场景、
跨架构地址/调用约定和运行行为。若该设计不能成立，应返回架构评审决定替代实现；不能跳过此模式后宣称同等能力。

## 6. .NET 10、混合包 ABI 与 Browser

.NET 10 继续生成现有 TypeMap attributes、candidate keys、proxy 与查询入口。普通非裁剪和官方 trimmed/NativeAOT 行为保持。
验证时对比生成源码、ABI 和实际产物；不能只依据新后端开关默认为 false。

后端选择是“最终引擎 × 每个输入包生成 ABI”的判断。net10 应用合法引用 net8-only 第三方包，因此必须提供前置 ABI 桥接：

| 最终引擎 / 输入包 | 处理 |
| --- | --- |
| net8 / ConditionalRecord v1 | 使用本方案的 net8 条件后端 |
| net10 / 官方 TypeMap ABI v1 | 现有官方路径 |
| net10 / net8 ConditionalRecord v1 | 在隔离 publish 副本中转换为官方 net10 TypeMap，再由官方引擎选择 |
| 同程序集同时携带冲突协议、未知协议/旧退役协议 | 分析前失败；不走 CollectFull fallback |
| net8 / 仅 net10 资产 | 由正常框架兼容检查或入口门禁拒绝，不伪造兼容 |

桥接必须发生在依赖分析前：验证 net8 记录与原始入口，将条件投影为官方 TypeMap attributes，target 使用同一已有
self-attributed proxy；补入 ABI v1 查询/helper/candidate-key 接线并规范化 Collect。使用当前解析的 net10 BCL/Core 定义，
不在 net8 包源码中伪造 System 命名空间 API，也不修改全局 NuGet 缓存或原包。

已有 proxy 足以表达 shared fragment；桥接不通过另建同 FragmentId 的代理再调用 AddFragment 嵌套包装，避免类型冲突。
候选记录隔离与强名称/符号信息处理须跟随正常 linker 输入规则，并记录 original→transformed 身份/hash 链。
该桥接尚未被原型验证，是 P2 的必需交付项；未通过时不能将 net10→net8-only 冷 NuGet 消费标为已支持。

Browser/Mono 的现有处理仍在官方 ILLink Mark 后物化已选 TypeMap。net8-only 包须先完成同样的 net10 前置桥接，
之后交给现有 Browser 转换器，不在其 Mark 中混入 ILLink8/ILC8 后端。既有 SDK/workload、receipt、平台域与主题覆盖契约保持。
本方案不新增 net8 Browser 或 Mobile AOT 支持承诺。

## 7. 调试与可观测输出

构建侧可输出规范化选择报告：工具链身份、输入快照、Group 请求、Condition→Fragment 边、选择原因、片段去重、
平台拒绝、最终 thunk/factory 位置以及未用项检查。报告不写凭据，不把绝对机器路径作为缓存身份。

诊断图用于解释，不能成为下一次发布的唯一语义输入。DGML 字符串解析、二进制 strings 和注册数量只能辅助，
不能代替强类型节点证据、程序集/方法身份或最终资源检查。

## 8. 错误分类

诊断统一归入现有注册诊断体系；实施时分配稳定编号，评审稿不伪造已经存在的编号。

| 分类 | 示例 | 处理 |
| --- | --- | --- |
| Contract | 版本未知、签名/IL 不匹配、记录冲突 | 普通编译可见时提前失败，否则 publish 失败 |
| Toolchain | 引擎/native helper/runtime pack 未验收组合 | 分析前失败 |
| Analysis | 已选 thunk 未分析、非法阶段、预初始化丢请求 | 不写成功回执，不发布 |
| Materialization | 新增未知依赖、残留 stub、重复入口 | 最终输出拒绝 |
| Integrity | hash/回执/消费路径不一致、陈旧缓存 | 重新计算或失败；不得复用旧成功状态 |
