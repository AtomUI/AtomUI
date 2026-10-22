# 精确主题设计的实现与验证边界

本文维护[Own有效求值](own-token-evaluation.md)、[品牌配方](brand-recipes.md)与[Control渲染](control-rendering.md)的源码符合程度和验收边界。正式设计合同不等于实现完成；执行规则由[主题验收规范](../../../engineering/development/theming/verification.md)拥有。

## 1. 当前实现事实

| 领域 | 当前源码边界 | 状态 |
| --- | --- | --- |
| Control求值 | descriptor保留WholeEvaluatorV1并增加DependencyDefaultsV2；Compiler新版先写显式Own／存在位，再求依赖默认；旧版整体后覆盖不变 | 本分支机制已实现并有聚焦／跨包AOT证据；95个产品Token尚未迁移，不代表真实控件消费已修复 |
| 定义继承 | 旧版仍验证virtual/base链；新版读取同／跨程序集静态默认声明、验证图并生成扁平计划及独立revision | 有跨包protected默认、重定义、非法声明和实际运行证据；逐产品依赖边仍待家族迁移验收 |
| Own兼容别名 | 显式属性及中立生成表；动态/XML/compiled在各层绑定归一，Compiler求值后投影历史字段 | 机制及DataGrid17对已实现；实际消费者和产品Window/Skia代表视觉已验，不代表CT-18完整家族或原生Popup/最终显示面通过 |
| Token提交 | 五阶段事务、稳定Context、scope graph、CommitCore引用交换与Publish诊断 | 现行Token合同；不是完整品牌表现协调的通过证据 |
| Part生成 | 生成无参专用Style、Nesting与声明route | 既有入口；临时原型已证明可作为资格root的子规则命中产品Card，尚无产品品牌装配入口 |
| 托管样式资格 | Core已有internal激活键/继承入口、资产工厂及Style图准备；ThemeContext/Provider尚无品牌状态或lease接线 | 临时验证15项及编译AXAML Card资产通过；CT-07原型另证10代frame清理。准备组件没有产品调用，不能宣称品牌选择/跨根事务已支持 |
| 配方与工具 | Token定义、编译包、动态来源、Gallery主题/局部示例及Part工具 | 不等于完整品牌样式选择、编辑/往返及AI入口已经交付 |
| Control消费 | 各包存在模板、代码与自绘消费者 | 尚未完成逐Control/Active Own/依赖边/Part的全范围运行验收 |

源码入口：[ThemeCompiler](../../../../src/AtomUI.Core/Theme/Compilation/ThemeCompiler.cs)、[descriptor](../../../../src/AtomUI.Core/Theme/Schema/ControlTokenDescriptor.cs)、[事务](../../../../src/AtomUI.Core/Theme/ThemeTransaction.cs)、[Scope](../../../../src/AtomUI.Core/Theme/ThemeScope.cs)、[Semantic Style生成](../../../../src/AtomUI.Generator/SemanticParts/SemanticPartManifestWriter.cs)。

资格原型在实际Avalonia12.1.4/.NET10 Desktop Headless环境中9/9通过，临时源/AXAML已清理，未改产品渲染路径。它同时发现两个需要产品装配处理的原生边界：未激活Style的公共资源仍可见；仅移除Style可能保留模板frame。原型已证明准备期完整资源结构拒绝，以及先失活、按原顺序重挂活动图可清除该frame；产品host的故障恢复、生命周期、性能与视觉仍未验收。优先级和装配约束由[品牌配方](brand-recipes.md#2-配置继承与托管样式资格)拥有。

内部资产工厂保留原selector并追加类型/资格，避免擦除原生AXAML状态。准备时重新执行原生nesting校验，完整验证后才用弱键标记，拒绝已用/挂载/共享或带公共资源的Style图。源码入口：[资产](../../../../src/AtomUI.Core/Theme/Styling/ThemeStyleAsset.cs)、[图准备](../../../../src/AtomUI.Core/Theme/Styling/ThemeStyleGraph.cs)、[激活资格](../../../../src/AtomUI.Core/Theme/Styling/ThemeStyleQualification.cs)。这些内部组件尚未参与现有主题提交，不改变已发布配方格式或现有Control渲染。

## 2. 机制证据要求

- Own：显式上下游、恢复继承、0/null、明暗/Control算法、别名同层冲突与跨层覆盖、同/跨程序集继承及版本混合；生成和真实AOT路径。
- 品牌：资格与原生优先级、父设置而重置子缺该Setter、候选资源不泄漏、已有实例/新旧Native与Overlay、stamp失效与重入。
- 故障：接线/激活/回退/Setter/资源通知/退役分别注入；验证Token与表现两种结果，现行Publish后不回滚；各host恢复和释放均核对。
- 消费：逐Control完整输入、状态、尺寸、自绘、缓存与公开定制；默认主题及原始输入/滚动/裁剪合同保持。优化重构另须按[视觉一致性硬门槛](../../../engineering/development/theming/verification.md#视觉一致性硬门槛)完成同输入前后无差异验收，不能只证明新定制生效。
- 性能/生命周期：同Host同场景前后测量、独立惰性槽、有界缓存、对象计数/WeakReference，不用RSS或测试数代替证据。

## 3. 平台与发布边界

Desktop、Browser、Native/Overlay和各OS证据分别记录，未验证平台不自动获得通过状态；同平台前后视觉比较及环境噪声处理按主题验收规范执行。几何、语义和配置数值仍须精确。Mobile尚无产品实现时，不提供空的已验收声明。

格式、生成ABI、公开接口及结果版本变更需真实包消费和发布验证。本分支已执行Own机制的跨项目NativeAOT osx-arm64临时程序（含实际Compiler及惰性槽）和.NET8构建；仅证明当前macOS26/.NET10 AOT及.NET8编译，不证明Native8、其他平台、真实产品控件的完整AOT发布或完整品牌。临时项目链接本机Homebrew库的较新OS警告不构成旧macOS兼容证据，已发布能力仍以发布记录为准。

## 4. 验收记录责任

共享机制的证据在对应实现/发布验证中维护；Control差异、消费者和生命周期由具体控件文档及交付说明维护。临时失败/通过验证执行后按规范清理，长期保障按独立价值保留；不建立永久逐测试登记库。

本地只运行实际改动模块的focused验证；全量回归仅在用户明确要求或发布流程执行。文档、源码实现、运行通过和平台发布分别报告。
