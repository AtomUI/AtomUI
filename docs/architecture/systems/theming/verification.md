# 精确主题设计的实现与验证边界

本文维护[Own有效求值](own-token-evaluation.md)、[品牌配方](brand-recipes.md)与[Control渲染](control-rendering.md)的源码符合程度和验收边界。正式设计合同不等于实现完成；执行规则由[主题验收规范](../../../engineering/development/theming/verification.md)拥有。

## 1. 当前实现事实

| 领域 | 当前源码边界 | 状态 |
| --- | --- | --- |
| Control求值 | descriptor保留WholeEvaluatorV1并增加DependencyDefaultsV2；Compiler新版先写显式Own／存在位，再求依赖默认；旧版整体后覆盖不变 | 本分支机制已实现并有聚焦／跨包AOT证据；95个产品Token尚未迁移，不代表真实控件消费已修复 |
| 定义继承 | 旧版仍验证virtual/base链；新版读取同／跨程序集静态默认声明、验证图并生成扁平计划及独立revision | 有跨包protected默认、重定义、非法声明和实际运行证据；逐产品依赖边仍待家族迁移验收 |
| Own兼容别名 | 显式属性及中立生成表；动态/XML/compiled在各层绑定归一，Compiler求值后投影历史字段 | 机制及DataGrid17对已实现；实际消费者和产品Window/Skia代表视觉已验，不代表CT-18完整家族或原生Popup/最终显示面通过 |
| Token提交 | 五阶段事务、稳定Context、scope graph、CommitCore引用交换与Publish诊断 | Token合同保持；内部有序资产输入已进入root/scope统一表现协调，已接公开版本1注册/manifest；品牌编辑和全覆盖工具仍待后续验收 |
| Part生成 | 生成无参专用Style、Nesting与声明route | 既有入口；临时原型已证明可作为资格root的子规则命中产品Card，已通过显式注册资产接入产品品牌装配 |
| 托管样式资格 | Core已有internal资产准备、惰性Context表现状态、Application/Provider/Window宿主lease及分阶段更新原语 | 宿主已有产品接线；Card专用Style/10代frame、PopupRoot和Overlay的Headless验证及默认代表Skia比较有证据。已接有效输入继承/重置、NoOp区分、批次协调、宿主完整stamp及版本1表现结果/有界清理；公开选择/manifest已接入同一事务，平台/全控件仍待后续验收 |
| 配方与工具 | Token定义、编译包、动态来源、Gallery主题/局部示例及Part工具 | 已实现版本1配方选择及完整/TokenOnly往返；编辑器与AI入口仍待后续任务 |
| Control消费 | 各包存在模板、代码与自绘消费者 | 尚未完成逐Control/Active Own/依赖边/Part的全范围运行验收 |

源码入口：[ThemeCompiler](../../../../src/AtomUI.Core/Theme/Compilation/ThemeCompiler.cs)、[descriptor](../../../../src/AtomUI.Core/Theme/Schema/ControlTokenDescriptor.cs)、[事务](../../../../src/AtomUI.Core/Theme/ThemeTransaction.cs)、[Scope](../../../../src/AtomUI.Core/Theme/ThemeScope.cs)、[Semantic Style生成](../../../../src/AtomUI.Generator/SemanticParts/SemanticPartManifestWriter.cs)。

资格原型在实际Avalonia12.1.4/.NET10 Desktop Headless环境中9/9通过，临时源/AXAML已清理。它发现候选公共资源仍可见、仅移除Style可能保留模板frame两项原生边界；准备检查与宿主退役现已在内部产品组件接入，并以实际Card模板及两类Popup宿主复核。内部批次已有激活/恢复/退役故障及清理重试证据；性能、公开品牌全链路、原生OS最终显示面及全控件视觉仍未验收。优先级和装配约束由[品牌配方](brand-recipes.md#2-配置继承与托管样式资格)拥有。

内部资产工厂保留原selector并追加类型/资格，避免擦除原生AXAML状态。准备时重新执行原生nesting校验，完整验证后才用弱键标记，拒绝已用/挂载/共享或带公共资源的Style图。源码入口：[资产](../../../../src/AtomUI.Core/Theme/Styling/ThemeStyleAsset.cs)、[图准备](../../../../src/AtomUI.Core/Theme/Styling/ThemeStyleGraph.cs)、[激活资格](../../../../src/AtomUI.Core/Theme/Styling/ThemeStyleQualification.cs)。

[Context表现状态](../../../../src/AtomUI.Core/Theme/Styling/ThemeStyleContext.cs)、[宿主lease](../../../../src/AtomUI.Core/Theme/Styling/ThemeStyleHostLease.cs)和[阶段更新](../../../../src/AtomUI.Core/Theme/Styling/ThemeStyleUpdate.cs)已经由Manager初始化、Provider及现有独立窗口lease使用。根锚点匹配独立TopLevel；实际styling祖先已有同Context资产时不在Window重复发布，以保持应用规则优先级。候选保持旧资格，局部表现Commit只换引用，退役/恢复错误可由内部原语报告，已完成句柄不保留旧图。

现行Token请求已通过同一Manager协调品牌阶段，继承/重置、NoOp身份、全部root/scope/catalog/package入口、stamp/重入与有界清理见[协调接口](brand-recipes.md#41-统一表现协调的现行接口)。版本1manifest校验及跨runtime/包候选边界见[配方接口](brand-recipes.md#61-版本1具体入口与校验)。当前不改变XML v1/编译ABI v1；这些机制不构成完整控件家族、NativeAOT或原生无闪烁通过结论。

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

统一表现协调另见[现行接口](brand-recipes.md#41-统一表现协调的现行接口)。内部专项覆盖仅资产变更、继承/重置、工厂重入及跨Context捕获、异步准备期宿主修改、package configure前捕获、失败事件顺序、scope warning保留结果、恢复/退役故障与完成句柄GC。普通Card及已打开/重新打开的Headless PopupRoot/Overlay使用真实编译专用Style；属性检查在实际宿主布局后进行，预布局旧值不冒充最终显示面失败。默认代表Skia同已提交基线零像素差异，品牌10次往返恢复亦零像素差异。以上范围不覆盖所有平台/控件或原生闪动；不可扩展为整体项目验收完成。
