# 精确主题设计的实现与验证边界

本文维护[Own有效求值](own-token-evaluation.md)、[品牌配方](brand-recipes.md)与[Control渲染](control-rendering.md)的源码符合程度和验收边界。正式设计合同不等于实现完成；执行规则由[主题验收规范](../../../engineering/development/theming/verification.md)拥有。

2026-11-05 新增 Windows x64 本机验收与平台交付，当前证据及剩余边界统一见
[Windows 验收记录](../../../superpowers/progress/2026-11-05-windows-theme-acceptance.md)。原生输入、最终显示、RTB、
机制专项和冷包消费分别登记；2026-11-06 已依据原记录重建31-owner/40控件、880×700、12阶段场景并通过 Windows 托管及 NativeAOT 原生验证。
这是对应执行时源码快照和指定宿主的实际验证；不声称找回历史脚本或证明其布局/像素逐字身份，不改写原 mixed-host 失败。
托管与原生截图相等只证明两条渲染路径一致，仍须检查图标的实际语义及选中/未选中状态。
`PathIcon` 的填充轮廓必须有面积；开放折线可能被隐式闭合成三角形，独立线段可能完全不可见。
下述各家族未验范围、G1/FW1、FW2 采用范围、CT19 具体差异及 CT24 暂缓不因新增平台证据自动关闭。

## 1. 当前实现事实

| 领域 | 当前源码边界 | 验证状态与限制 |
| --- | --- | --- |
| Control求值与继承 | WholeEvaluatorV1与DependencyDefaultsV2共存；V2先保留显式Own/存在位，再计算依赖默认；同/跨程序集生成扁平计划与revision | 机制、真实包与具名AOT路径已有证据；产品按各家族批准范围接入，当前81个`*Token.cs`显式声明DependencyDefaultsV2（含SplitButton、Steps），其余默认/旧版定义继续兼容，不再沿用“95个尚未迁移”的早期状态。独立Steps的82字段也已迁移V2R1并验证真实配置/清除/再应用，保留三个历史显示fallback，不将所有字段都宣称为无兼容条件 |
| Own兼容别名 | 显式属性及中立生成表；dynamic/XML/compiled归一后求值并投影历史字段 | DataGrid家族的实际消费者、默认计划和具名原生路径已按限定范围验收；Legacy/Reserved保持其具体合同 |
| Token与表现提交 | 五阶段事务、稳定Context、scope graph、CommitCore引用交换、Publish诊断与表现结果 | Application/Provider/Window租约、继承/重置、NoOp、stamp/重入、故障恢复及有界清理有验证；不同平台和宿主分别认证 |
| Semantic Part与品牌资格 | 专用Style生成、Nesting/route、显式资产工厂、原selector追加类型/资格与独立宿主图 | 已进入产品品牌装配；专用Style命中、优先级、配置/清除和实际树拓扑按具体控件核验，不以Card原型替代全部家族 |
| 配方与手动工作台 | 版本1配方选择、完整/TokenOnly导出、schema检查、草稿/隔离预览/应用、文件保存、diff/恢复/取消 | CT22/CT23手动闭环已按有限范围验收；AI服务CT24继续暂缓 |
| Control消费与默认外观 | 模板、代码及自绘消费者，Shared/Owner/Own职责和现有ControlTheme优先级 | 以各家族的实际字段、状态和限定视觉批准为准。724目录与1244字段是分类口径，不能换算为每个节点/状态/平台均有独立像素证据 |
| Style退役兼容 | 官方Avalonia12.1.4保持；Native共享hook在实际主题宿主与Popup session补齐来源退役 | 原全局类型Style/Split及Application/Provider/实际独立Popup、Overlay作用域已有NET8/NET10/Mac NativeAOT验证；无类型选择器退役有具名成本对照，普通类型维持原量级 |
| 性能与生命周期 | Owner绑定定义按Control弱键复用，不缓存当前Context/key/value；退役/重挂及弱目标合同保持 | 原95处消费负载成本已量化并获得具体有限接受；没有普遍加速结论。保留已分离子对象时旧Window回收已定位Border合成缓存并由Native脱离hook修复，弱目标/重挂验证通过 |

源码入口：[ThemeCompiler](../../../../src/AtomUI.Core/Theme/Compilation/ThemeCompiler.cs)、[descriptor](../../../../src/AtomUI.Core/Theme/Schema/ControlTokenDescriptor.cs)、[事务](../../../../src/AtomUI.Core/Theme/ThemeTransaction.cs)、[Scope](../../../../src/AtomUI.Core/Theme/ThemeScope.cs)、[Semantic Style生成](../../../../src/AtomUI.Generator/SemanticParts/SemanticPartManifestWriter.cs)。

托管资产工厂保留原selector并追加资格，准备时执行nesting校验，完整验证后弱键标记；拒绝已用/挂载/共享或带公共资源的Style图。
[资产](../../../../src/AtomUI.Core/Theme/Styling/ThemeStyleAsset.cs)、[图准备](../../../../src/AtomUI.Core/Theme/Styling/ThemeStyleGraph.cs)、[资格](../../../../src/AtomUI.Core/Theme/Styling/ThemeStyleQualification.cs)分别拥有输入、拓扑和激活边界。
[表现状态](../../../../src/AtomUI.Core/Theme/Styling/ThemeStyleContext.cs)、[宿主lease](../../../../src/AtomUI.Core/Theme/Styling/ThemeStyleHostLease.cs)和[阶段更新](../../../../src/AtomUI.Core/Theme/Styling/ThemeStyleUpdate.cs)由Manager、Provider及跨根lease接入；实际祖先已有同Context资产时，Window不重复发布，以保持应用规则优先级。

现行Token与品牌请求使用同一Manager协调，格式和表现接口见[品牌配方](brand-recipes.md#41-统一表现协调的现行接口)。XML v1/编译ABI v1保持，完成句柄不保留旧图，Publish后错误按结果报告；这些事实不替代未覆盖宿主、外部物理输入或各OS最终显示面的证据。

## 2. 机制证据要求

- Own：显式上下游、恢复继承、0/null、明暗/Control算法、别名同层冲突与跨层覆盖、同/跨程序集继承及版本混合；生成和真实AOT路径。
- 品牌：资格与原生优先级、父设置而重置子缺该Setter、候选资源不泄漏、已有实例/新旧Native与Overlay、stamp失效与重入。
- 故障：接线/激活/回退/Setter/资源通知/退役分别注入；验证Token与表现两种结果，现行Publish后不回滚；各host恢复和释放均核对。
- 消费：逐Control完整输入、状态、尺寸、自绘、缓存与公开定制；默认主题及原始输入/滚动/裁剪合同保持。优化重构另须按[视觉一致性硬门槛](../../../engineering/development/theming/verification.md#视觉一致性硬门槛)完成同输入前后无差异验收，不能只证明新定制生效。
- 性能/生命周期：同Host同场景前后测量、独立惰性槽、有界缓存、对象计数/WeakReference，不用RSS或测试数代替证据。

## 3. 平台与发布边界

Desktop、Browser、Native/Overlay和各OS证据分别记录，未验证平台不自动获得通过状态；同平台前后视觉比较及环境噪声处理按主题验收规范执行。几何、语义和配置数值仍须精确。Mobile尚无产品实现时，不提供空的已验收声明。

格式、生成ABI、公开接口及结果版本变更需真实包消费验证。当前分支已有具名包/条件后端、Browser、Linux专项及Mac NET8/NET10/NativeAOT证据，分别保留其源码、依赖、目标与配置身份；旧产物不能认证后续修改的输入。Windows专项继续开放，其他平台的具体通过不替代Windows。

Mac本地NativeAOT链接Homebrew较新dylib的部署版本警告不构成旧macOS兼容证明；冻结ILCompiler的NU1903告警保留，不宣称零警告。验收、正式发布和NuGet publication分别记账，尚未发布的改动不写成已发布能力。

## 4. 验收记录责任

共享机制的证据在对应实现/发布验证中维护；Control差异、消费者和生命周期由具体控件文档及交付说明维护。临时失败/通过验证执行后按规范清理，长期保障按独立价值保留；不建立永久逐测试登记库。

本地只运行实际改动模块的focused验证；全量回归仅在用户明确要求或发布流程执行。文档、源码实现、运行通过和平台发布分别报告。

统一表现协调的具体接口见[品牌配方](brand-recipes.md#41-统一表现协调的现行接口)，Native私有兼容边界见[Native架构](../../../modules/native/architecture.md#avalonia-1214-兼容-hook)。交付记录按家族/任务保存实际源码身份、失败/通过与具体批准；不将旧PendingScene、SourceChecked标签直接换算为未完成数量，也不将同输入未验证路径批量改成PASS。
