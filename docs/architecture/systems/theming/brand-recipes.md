# 品牌主题配方与样式作用域设计

本文拥有Token与品牌样式的组合、托管样式资格、跨根装配和激活生命周期目标合同。Token计算与现行事务由[主题运行时](runtime.md)拥有，Part route由[Semantic Part](semantic-parts.md)拥有，生成与现有格式分别遵守[生成器](../../../modules/generator/semantic-part-generator.md)和[编译主题包](compiled-theme-packages/overview.md)。实现及验证状态见[验证边界](verification.md)。

## 1. 配方内容与所有权

品牌配方是逻辑交付单元，包含主题定义/算法策略、Global与Control覆盖、编译后的专用Part Styles、必要模板/静态资产、修订和能力要求。该名称不表示已经存在同名公开C#类型或已发布文件格式。

ThemeManager统一协调选择和应用；Snapshot仍是有效Token值的唯一真源。样式资产是UI线程拥有的原生Avalonia对象，不把可变Style、Visual或ResourceDictionary塞入纯Token Snapshot。

配方引用显式注册的生成资产及实际Control要求，不隐式启用可选包，不执行外部脚本或运行期AXAML。相同规范化配方和资产修订形成可比较的样式集合身份，内容变化使相关缓存和选择失效。

配方区分初始化装配、稳定结构热切换及结构模板替换的能力。稳定切换规则不修改业务选择、数据源或ThemeConfig；结构替换若会创建/删除scope或需要迁移编辑器内部状态，必须有单独的owner状态与拓扑合同及证据，不能套用稳定结构路径或通过重建整页隐瞒差异。目标Host不具备相应能力时诊断，不静默丢弃模板资产。

## 2. 配置继承与托管样式资格

Token配置继续遵守现行根请求/局部Provider的不同Inherit=false基线。托管品牌样式以相同的有效scope输入确定继承、替换和重置后的完整集合。

仅合并样式列表不能阻止祖先StyleHost继续匹配。托管规则必须按**实际目标Control owner的有效样式集合**获得应用资格，再通过生成的Part route进入节点：

- 资格不能仅依靠祖先Scope挂class；嵌套重置改变子owner资格后，父配方规则即使被枚举也不再命中。
- 需要继承的规则以子有效集合资格生成，不能只保留父集合键的规则。
- 资格状态由现有ThemeContext所有权管理，通过必要的继承激活状态及跨根lease传播；它不是Token值转运属性，也不推断Control identity。
- public子Control保持自己的identity和局部覆盖；internal Part按声明route归属，不能为了命中伪造owner。
- 普通应用Styles继续按Avalonia原生合同执行，Inherit=false不清除任意应用规则。

生成工厂以受控资格组合原生selector及专用Style。selector中的属性条件可能影响原生激活与BindingPriority，因此必须验证实例属性、局部/全局专用Style及状态规则的实际覆盖关系；不能宣称加一个条件就天然保持所有优先级。资格方案未通过这些合同前，不得宣称品牌隔离可交付。

## 3. 资源隔离与跨根

候选样式使用未激活资格准备。未命中selector不表示Style.Resources也被隔离，因此不得在准备期把候选公共key资源提前挂入祖先搜索链。模板/静态资产使用明确的不可变引用或按活动generation投影的现有资源入口，保持词法、include与应用覆盖合同。

Native/Overlay均由实际owner lease接线Token context、托管样式及资格，在显示前完成；已打开host随owner变化更新。迁移、关闭、失败打开和重复释放有对称清理。无owner的独立窗口仍按根主题合同运行，不能推断调用点。

## 4. 协调阶段与现行Token事务边界

现行Token事务的CommitCore仅做无通知引用交换；Publish不可回滚，异常保持Committed并通过PublishDiagnostics报告。品牌协调不得把原生Style装配放入CommitCore，也不得在Token已经Publish后静默恢复旧Snapshot并改称普通Failed。

品牌操作由同一ThemeManager串行协调；目标顺序如下，Token结果与品牌表现完整性分别报告。以下表现状态为领域状态，不是已存在的枚举/API：

| 阶段 | 工作与边界 |
| --- | --- |
| Capture/Prepare | 捕获scope/host/stamp和旧表现状态，准备候选Token与未激活样式；不替换活动集合，不执行破坏性Styles.Replace |
| 接线 | UI线程创建候选lease并挂载未激活规则；记录实际完成步骤；失败逐host清理，Token尚未提交 |
| 表现预激活 | UI线程无await地切换资格，保留旧规则/lease；此步可能触发原生属性通知，不能称为静默Commit；Token仍为完整旧代 |
| 再校验/CommitCore | 重新验证generation、拓扑和host stamp；成功才一次性交换全部Token引用及选中配方状态，继续现行无通知要求 |
| Publish | 按现行顺序通知appearance、资源和事件，使活动样式获得新Token；所有通知边界独立诊断，不回滚已提交Token |
| 退役/完成 | 确认托管成功路径下一受控渲染前版本一致，再报告品牌应用完成；旧规则已无资格，随后释放旧lease并清理 |

预激活的同步回调可能观察到完整旧Token与正在切换的样式；不承诺任意用户同步回调或强制同步渲染观察整体UI原子性。正常托管路径必须在同一UI调度段完成预激活、校验、提交和发布，不跨await泄漏半成品帧；不承诺多个OS窗口同一物理vsync。

请求等价/合并同时包含Token输入和有效样式集合/修订；不能只因Token缓存命中就丢弃样式变化。Token语义未变时可以保留其NoOp结果，但品牌整体只有Token、样式与选中状态均等价才为NoOp。纯Token缓存继续按其真实依赖复用；表现资产缓存按集合、修订、registry/Host能力隔离，不跨scope共享Visual/owner实例。

## 5. 失败与恢复

- 准备/接线失败：不提交Token，按实际已发生变更逆序清理，保留旧资格；不能假定抛错API没有修改集合或owner。
- 预激活失败或再校验失效：在Token提交前逐host恢复旧资格及lease，汇总恢复结果。恢复成功才可报告旧品牌保持；恢复失败明确报告表现未完整恢复，不能只给普通失败并暗示外观无变化。
- Token CommitCore之后：遵守现行Committed/PublishDiagnostics。破坏样式激活、绑定或资源传播的异常，以及表现核对失败，必须使品牌整体结果报告降级/未完整应用；不改写Token结果为Failed。普通观察者异常只形成独立发布诊断，没有表现失效证据时不据此判定品牌降级。
- 退役失败：记录可重试清理责任及受影响host，限制未退役资产数量；不得无界积累旧Style、lease或Visual。无法恢复的host须修复/重建后才能继续承诺完整应用。

恢复只覆盖框架持有的资格/资源/lease，不能回滚任意用户回调副作用。该协调需要显式的表现状态和版本化结果扩展，不能复用现有Token结果表达所有阶段。接线/Setter激活错误与普通发布监听器异常必须区分。

## 6. 配方格式与完整导出

现有XML v1和编译主题ABI v1继续承载原有Token/算法语义，不添加未经版本化的样式元素或默认含义。品牌组合manifest独立版本化，表达Token内容、已注册资产引用/修订、Control要求及平台能力。

完整导出保留全部生效表达；Token-only导出明确标识范围。导入目标缺少必需资产、版本或已启用Control时诊断，不能丢掉样式后声称等价。任意对象、C#/脚本和运行期AXAML均不属于数据导入能力。

## 7. 验证合同

必须覆盖父品牌有padding、重置子只设置foreground的反例；同时覆盖嵌套/兄弟、public子Control、重挂载、新建/已打开Native及Overlay、资源不提前泄漏、资格与原生优先级。

故障注入覆盖AddOwner/RemoveOwner、资格激活/回退、stamp失效、Setter与资源通知、退役和重复关闭；分别检查Token结果、表现结果、所有host的实际状态及释放，不能只检查最后一个返回值。

Ant默认、shadcn、Bootstrap与零圆角/粗边框/大字号/紧凑/关闭动效配方贯穿实际渲染验证。实现证据不足时不将目标合同写成已经支持完整品牌原子切换。
