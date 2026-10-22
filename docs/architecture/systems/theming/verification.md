# 精确主题设计的实现与验证边界

本文维护[Own有效求值](own-token-evaluation.md)、[品牌配方](brand-recipes.md)与[Control渲染](control-rendering.md)的源码符合程度和验收边界。正式设计合同不等于实现完成；执行规则由[主题验收规范](../../../engineering/development/theming/verification.md)拥有。

## 1. 当前实现事实

| 领域 | 当前源码边界 | 状态 |
| --- | --- | --- |
| Control求值 | `ThemeCompiler`先计算整体默认，再应用Own覆盖；descriptor为单个整体evaluator | 现行合同已存在；依赖默认值目标尚未迁移 |
| 定义继承 | Generator验证抽象/终端并扁平属性，默认链由virtual/base调用形成 | 现行继承已实现；新依赖元数据/终端计划尚无实现证据 |
| Token提交 | 五阶段事务、稳定Context、scope graph、CommitCore引用交换与Publish诊断 | 现行Token合同；不是完整品牌表现协调的通过证据 |
| Part生成 | 生成无参专用Style、Nesting与声明route | 既有入口；品牌集合资格工厂和激活生命周期尚需实现验证 |
| 配方与工具 | Token定义、编译包、动态来源、Gallery主题/局部示例及Part工具 | 不等于完整品牌样式选择、编辑/往返及AI入口已经交付 |
| Control消费 | 各包存在模板、代码与自绘消费者 | 尚未完成逐Control/Active Own/依赖边/Part的全范围运行验收 |

源码入口：[ThemeCompiler](../../../../src/AtomUI.Core/Theme/Compilation/ThemeCompiler.cs)、[descriptor](../../../../src/AtomUI.Core/Theme/Schema/ControlTokenDescriptor.cs)、[事务](../../../../src/AtomUI.Core/Theme/ThemeTransaction.cs)、[Scope](../../../../src/AtomUI.Core/Theme/ThemeScope.cs)、[Semantic Style生成](../../../../src/AtomUI.Generator/SemanticParts/SemanticPartManifestWriter.cs)。

## 2. 机制证据要求

- Own：显式上下游、恢复继承、0/null、明暗/Control算法、别名同层冲突与跨层覆盖、同/跨程序集继承及版本混合；生成和真实AOT路径。
- 品牌：资格与原生优先级、父设置而重置子缺该Setter、候选资源不泄漏、已有实例/新旧Native与Overlay、stamp失效与重入。
- 故障：接线/激活/回退/Setter/资源通知/退役分别注入；验证Token与表现两种结果，现行Publish后不回滚；各host恢复和释放均核对。
- 消费：逐Control完整输入、状态、尺寸、自绘、缓存与公开定制；默认主题及原始输入/滚动/裁剪合同保持。优化重构另须按[视觉一致性硬门槛](../../../engineering/development/theming/verification.md#视觉一致性硬门槛)完成同输入前后无差异验收，不能只证明新定制生效。
- 性能/生命周期：同Host同场景前后测量、独立惰性槽、有界缓存、对象计数/WeakReference，不用RSS或测试数代替证据。

## 3. 平台与发布边界

Desktop、Browser、Native/Overlay和各OS证据分别记录，未验证平台不自动获得通过状态；同平台前后视觉比较及环境噪声处理按主题验收规范执行。几何、语义和配置数值仍须精确。Mobile尚无产品实现时，不提供空的已验收声明。

格式、生成ABI、公开接口及结果版本变更需真实包消费和发布验证；本设计文档更新没有执行产品功能测试或NativeAOT发布，不能据此更新已发布能力。

## 4. 验收记录责任

共享机制的证据在对应实现/发布验证中维护；Control差异、消费者和生命周期由具体控件文档及交付说明维护。临时失败/通过验证执行后按规范清理，长期保障按独立价值保留；不建立永久逐测试登记库。

本地只运行实际改动模块的focused验证；全量回归仅在用户明确要求或发布流程执行。文档、源码实现、运行通过和平台发布分别报告。
