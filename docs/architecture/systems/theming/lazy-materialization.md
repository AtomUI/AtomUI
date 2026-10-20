# 主题惰性物化与资源查询

状态：2026-10-12 工作树已实现下述 cell、稳定资源层、键索引和增量复用；最终裁剪与测量验收见实施记录。
本页定义查询、缓存和所有权边界，不把单测通过等同于全部平台验收。
主题基础语义见[主题运行时](runtime.md)，启动时的一次性提交见[启动生命周期](../startup/lifecycle.md)。

## 1. 两类惰性的成本边界

| 对象 | 必须先准备 | 可以按需执行 | 不能作出的承诺 |
|---|---|---|---|
| 控件快照 | 配置规范化、身份/slot 映射、依赖关系和复用判定 | 对实际查询的控件计算 Own Token 与差量 | 同步主题提交成功不代表所有控件 evaluator 已成功 |
| 静态资源资产 | 资产身份、排序、导出证据、宿主与候选查询表 | 首次真实需要时构造编译型字典/主题 | 没有完整 key 证据时，无法保证任意 miss 都零构造 |

保留有序游标解析控件配置的线性路径。没有测量依据时，不把一次 O(C) 状态扫描改成大量逐控件字典、闭包或
独立调度任务。昂贵计算只为消费的 K 个控件执行，但查询、冻结和索引成本仍需计入总工作。

## 2. 独立惰性槽与历史快照释放

`LazyControlSnapshots` 的槽位数组共享独立 `ControlSnapshotCell`，不保留完整父 snapshot：

```text
Snapshot A: [Cell(Button), Cell(Input-v1), Cell(Menu)]
Snapshot B: [Cell(Button), Cell(Input-v2), Cell(Menu)]
```

- 同一 registry 下未变化的控件直接共享相同 cell；追加包跨 registry 时已物化的旧 cell 继续共享，
  冷 cell 则以最新 registry 重建惰性入口，避免旧 cell 的未执行工厂保活历代完整包索引。
- 索引路径收到稀疏变更集时直接共享未变化槽；deferred 变更集保守包含已物化槽时，新 cell 只保存上次的
  值与单控件配置，在该槽下次被查询时比较配置再决定复用或重新计算。未访问的冷控件不会为复用判定提前运行 resolver。
- cell 不持有整个前代 ThemeSnapshot，不沿历史版本递归寻找值。新快照不承担为旧快照保活的责任。
- 编译输入包含 exact 控件身份、配置、有效全局输入与算法证据；同一 cell 的首次物化采用安全发布和 single-flight。
- 不把“引用写入不会撕裂”等同于完整的跨线程发布证明。锁、Volatile 或成熟 lazy primitive 的选择由并发验收锁定。
- 无未物化消费者后释放只用于编译的 builder/config 工作集；纯输入允许合法共享，但不得持有 Visual、Provider 或事件。
- 保留现有失败时点：控件计算失败在首查暴露，不进入成功缓存。是否缓存异常对象/允许重复执行属于单独契约决策，
  不能因替换 lazy primitive 静默改变。

验证必须包含超过缓存 entry 上限的连续唯一配置更新、所有控件均不复用、长期未访问槽、全部槽已物化、局部 scope
detach 和持有一个合法旧读取视图。不能只观察 cache.Count；应验证被驱逐且不再使用的旧快照可以回收。

## 3. 跨 registry 的身份与复用

slot 仅在单个 registry revision 内稳定。增加一个按身份排序靠前的控件就可能移动后续 slot。

- 复用首先验证 exact Control identity 与 CLR type，不通过“slot 相同且未越界”推断身份相同。
- 最小安全实现可以拒绝跨 registry 复用；后续实现通过 identity 找到旧 slot，再比较控件 schema 和计算输入。
- 无 Own Token、无配置的快速路径仍遵守身份与空差量约束，不能借用其他控件具有覆盖值的旧快照。
- 独立 cell 的计算身份与数组位置分离；若结果仍携带 `ControlSlot`，必须保证它指向当前 registry，不能将重排前
  的位置字段暴露给新快照消费者。

## 4. 资源层稳定与全局优先级

逻辑优先级从高到低为：正常控件/作用域/应用覆盖、当前 context 的 Token provider、按包与资产顺序解析的静态
主题资源。静态层内部仍按 `PackageCommitOrdinal`、`(Phase, Order, AssetId)` 和嵌套字典既有次序处理。
本节只定义 AtomUI 主题字典内部的顺序。应用与外部主题之间的宿主层级由
[应用资源宿主设计](application-resource-host.md)拥有，源码迁移与验收状态见
[资源宿主验证](application-resource-host-verification.md)。不能据内部 provider 顺序推断全部外部 Style.Resources 覆盖保持不变。

ThemeManager 只挂载一个稳定的 `StaticThemeResourceProvider`。它持有不可变的有序资源表，在演化时交换表引用：

- 静态层相对 Token provider 的优先级始终不变；重建静态集合不能使普通 Token 查询先遍历整个静态链。
- 已存在真实字典不再次 detach/attach，不重复挂载已存在的宿主。
- 同 AssetId/同 metadata 的资源只出现一次，冲突在 Prepare 失败；已有与增量有序序列线性归并。
- 交换准备好的查询表不调用工厂、用户代码或 owner 回调。统一 Publish 阶段只发出一次该资源根的变更通知。
- owner 关系由代理统一管理，加载前后语义一致；RemoveOwner 必须清理代理与内层关系。
- 字典作用域 DynamicResource 与 owner 查询必须保持正常宿主语义，不能通过永久不给内层字典 owner 来省掉通知。
  内层通知经明确的宿主桥在 Prepare/CommitCore 范围内收集，到 Publish 再传播；普通运行时更新仍正常传播。
- 不能把 MergedDictionaries.Clear/Add 或内层 AddOwner 当作普通无副作用列表操作。

`ThemeResourceOwnerBridge` 向外部 host 转发查询；不能额外先查内部字典或兄弟资产，否则会压过应用覆盖。
初次加载与 owner 转换时抑制向外重复通知，局部绑定仍更新；运行期修改继续向真实宿主通知。

### 4.1 所有权失败与释放

静态资源层和惰性代理直接实现 `IResourceProvider` 的所有权协议，挂载失败后不保留 provider 的 owner。
`OwnerChanged`、资源绑定通知以及内层 `AddOwner` 都是可能抛错的外部回调，不能把基类在回调前已经写入的 owner
视作完成挂载。

- 在调用 `AddOwner` 前登记本次尝试的资源；原生字典即使先设置 Owner 再抛错，也参加 Prepare 回滚。
- Prepare 失败只释放不属于当前已提交查询表的资源，保持活动资源与查询优先级。整个静态层挂载失败则释放全部
  已登记资源和宿主桥，之后可以重新挂载。
- 每个资源、宿主资源事件和主题变体事件独立释放；一个回调失败不跳过兄弟清理。先退出内部 owner/加载状态，
  在 finally 中移除挂载记录并清理宿主桥，再传播失败。
- 单个异常保留原对象和抛出堆栈；原始操作、释放或批量通知同时失败时汇总为 `AggregateException`，不以清理异常
  覆盖原始原因。批量通知失败同样参与回滚。
- 已挂载代理的首次加载失败会丢弃失败的内层实例和桥订阅，但保留代理的合法外层 owner，使下一次查询可重试。
  预加载代理挂载失败则保留已构造资源，并清除挂载关系，使下一次 AddOwner 可重试。

这些保证作用于 AtomUI 持有的关系与清理调用；自定义 provider 和事件访问器仍负责在自己的释放方法中解除其内部关系。

## 5. 按资源 key 路由候选资产

`ResourceTable` 索引为 `key → 按既有优先级排列的候选资产`，不以“某 key 最后一个资产”覆盖所有上下文语义。
键集合包含各 variant 和本地 include 的并集；实际 variant/词法查询仍委托原始资源 provider。

1. 对有完整证据的 key，直接访问候选资产，避免对所有资产做负向查询。
2. 默认 typed theme、命名主题、共享常量、显式 include 与 variant 分支均需进入证据模型。
3. `ExportedThemes` 只描述部分导出，不能据此判定一个资产对其他 key 必然 miss。
4. 未知/开放动态 key 保留语义正确的 fallback；不得因不在索引中吞掉合法资源。
5. 当前不缓存未知键的 miss；开放字典可在运行期加键。若以后增加负缓存，必须响应 generation、variant 与动态更新。
6. AOT 下索引由生成式事实和静态 factory 构造；不能用资源扫描、反射读取字典或全量物化来创建索引。

目标复杂度：索引建立 O(资源键与候选数)，已知键查询为均摊 O(1) 定位加实际候选数；未知键 fallback 成本单独报告。
不得宣称所有资源查询都 O(1)，也不得为大索引增加的常驻内存隐瞒成本。

生成器仅在证明完整键集合且消费端 Core 支持时调用已获批准的兼容入口
`ControlThemeResourceRegistration.CreateGenerated(..., IReadOnlyList<object> resourceKeys)`。
入口防御性复制集合；旧四参构造函数继续代表未知键。手写字典、未知表达式、外部/不透明 include 保留开放回退。
索引候选与未知资产按原 ordinal 合并查询，因此未知资产仍能覆盖已知 key。

## 6. 分离全局计算身份

全局复用通过 `HasSameGlobalSchema` 比较不可变 descriptor 引用，并比较有效编译输入；其范围限于全局 Token schema、算法、有效 seed/覆盖、appearance、
字体及全部影响全局值的配置。完整 registry 身份仍用于完整快照、控件 schema 和资产绑定。

- 如果全局计算输入相等（例如启动前 Prepare 的全局层），可共享已有的全局值、资源和 palette。
- 全局算法、全局 schema、主题有效全局值或相关配置变化必须使全局结果失效。
- 完整 ThemeDefinition 的修订若仅改变控件配置，是否允许细化比较要由结构化输入证明，不能只比较主题 ID。
- 配置能证明无控件级全局差量时，Shared Token 查询可直接使用根资源；Own Token 仍在第一次 Own 查询时编译。
  自定义 evaluator 或算法若可能影响该结果，应走完整控件路径。

规范化配置已按 schema slot 排序；构建结构键直接沿该顺序复制。不可变配置和绑定后 definition 的结构键
按对象身份弱缓存；不同实例仍执行完整内容比较，registry revision 仍参与完整快照/控件键。新的输入修订
创建新实例，不原地修改绑定值。弱缓存不保活输入或 registry。

复用全局值时，编译阶段不恢复可变 DesignToken。新的 lazy source 只保存当前冻结值、资源、palette 和
算法；首次需要执行控件编译时恢复一次 builder，并发首查安全发布。若控件结果缓存已命中，无需恢复。
source 不捕获 ThemeCompileInput 或父快照；全部消费者物化后，cell 释放 source。没有另设急切参考编译入口。

## 7. 内存计费与释放

缓存的 entry 限制与字节限制不能被 cell 的间接父链绕过。首次物化发布后，在 cell 锁之外通知所属缓存，
重新估算有界快照集合并执行驱逐。回调弱引用 cache，cell 不反向保活整个缓存。

重新计费成本为 O(缓存快照数 × 槽数)，不构造未访问槽；当前选择避免逐 cell 订阅者图。
缓存限额只约束缓存持有，不限制活动 context 或调用方合法保留的视图。

估算覆盖框架已知的 EffectiveConfig、Definition、来源字符串、Token 值和规范字符串、结构键数组，以及冷
cell 的配置与独立 source。每个快照内相同 source 计一次；source 预留恢复全局 builder 的值槽与 palette
成本。缓存 entry 将结构键和结果共同计费，过大输入即使尚未查询 Own Token 也不能绕过字节预算。冻结的根
资源/palette 与不可变输入的尺寸按对象身份复用，避免每次快照复用都重新扫描全局数据。
deferred merge 由框架自己保留的规范化父/当前配置也属于已知输入：按对象身份计费其冷控件 Token 值，
而不调用控件 resolver 或遍历任意用户闭包；嵌套合并对同一配置只计一次。

计费是保守估算，不是整个保留对象图的硬上限：共享值可能重复计费；registry 共享的 descriptor、算法与
委托闭包不递归计费。任意自定义对象只计固定壳体，内部数组、原生资源、可变 brush 内容和自定义闭包的实际
大小不在估算保证内。不得通过反射或调用用户代码遍历这些对象来扩大预算承诺。

共享 cell 可保守重复计费；不能仅用“单个 snapshot 的槽数有限”推出整个应用保留字节有界。固定 187 等控件数量
不属于内存契约，第三方注册、活动 scope 和历史配置数量均须参与模型。

启动提交完成后丢弃只供构建的指纹集合、builder 和 Prepare 的 catalog/registry 引用。运行时保留的描述符或
索引必须有后续查询用途；不要把临时 staging 集合作为第二套永久运行时数据库。

## 8. 验收矩阵

| 场景 | 必须证明 |
|---|---|
| 普通 Token 查询 | 不构造无关静态资产，Token provider 优先级不变 |
| 已知 typed/string key 和未知 key | 命中、覆盖、variant、include 与旧解析路径等价 |
| 已加载/未加载资产的 owner 生命周期 | 挂载/解绑对称，解绑后不再报告旧 owner，无按资产广播风暴 |
| registry slot 重排 | 新控件不会取得旧控件差量；身份与位置分开验证 |
| 连续唯一主题配置 | 缓存外不再需要的旧 snapshot 可回收，首次槽查询不随历史长度递归 |
| 并发首次查询和失败 evaluator | single-flight、安全发布、异常语义与资源所有权符合契约 |
| 非裁剪 / 裁剪产物 | 相同输入的查询结果、冲突诊断和共同保留资产的优先级一致 |

这些是目标门禁，已有构建、普通单测或 NativeAOT 进程存活证据不能替代原始 macOS UX 和生命周期验证。
