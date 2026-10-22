# 主题基础设施与Token研发合同

> 本文定义研发要求及目标机制边界；不表示当前源码已经完成迁移或验收。

本文的适用范围、规范用语与事实边界见[全局规范入口](overview.md)。

本文拥有基础设施和Token相关的研发要求。当前合并、算法、snapshot、scope、事务与惰性规则以[主题运行时](../../../architecture/systems/theming/runtime.md)为准；Token声明以[Control Token规范](../control-token-guidelines.md)为准。以下条款是作者及评审要求，不独立改写这些系统语义。

Own依赖与兼容别名的正式语义见[有效值求解](../../../architecture/systems/theming/own-token-evaluation.md)，品牌集合及提交前预激活见[配方设计](../../../architecture/systems/theming/brand-recipes.md)。

### TH-01：统一运行时与唯一有效值来源

性质：现行合同整合。

- 全局、局部、静态主题包、动态文件、Gallery与AI生成配置必须进入同一配置/编译/提交体系。
- ThemeManager是唯一提交协调者，ThemeSnapshot是有效Token值的唯一真源；禁止控件、工具或主题工厂私自维护第二份活动主题。
- 编译workspace可以可变；已发布配置、snapshot及被共享的引用值必须满足不可变合同，不能从外部原位修改。
- identity、schema、默认算法、资产与配置各有明确revision；影响计算或投影的变化不能复用不兼容缓存。

检查：调用链与所有权审查、生成/动态路径差分、不可变输入及修订失效验证。

### TH-02：精确owner与正确作用域

性质：现行合同整合。

- 每个公开可主题化Control都有exact identity，没有Own也不例外；不得从CLR基类、TargetType、BasedOn或视觉祖先推断回退。
- 内部frame/presenter/非Visual对象由真实公开owner消费；组合中的真实public子Control保留自身identity，父owner负责明确的布局与状态投射。
- 本应响应Control级覆盖的值必须使用该Control的Effective Global；只有明确跟随当前context全局结果的语义才使用Shared读取。
- Global与Control、根与局部、兄弟scope之间不得串值。根请求与局部Provider的Inherit=false分别遵守现行基线，不能统一解释成同一种清空操作。
- 独立TopLevel与Native/Overlay路径按现行owner context/lease合同接线；离树、迁移owner、关闭和重复释放都有定义。

检查：同Global不同Control覆盖、嵌套与兄弟隔离、公开子Control、非Visual目标、已打开及新建跨根host。

### TH-03：Token分层、覆盖与派生语义

性质：现行分层＋需版本化的Own求值演进。

- Global按Seed/Map/Alias分阶段；算法链、Control算法策略、appearance与覆盖顺序必须有确定语义，复用runtime正式合同。
- 默认计算使用当前Control的Effective Global和已解析appearance，不从应用ThemeVariant再次猜测结果。
- 配置的“未指定”与显式0、合法null等值必须区分；输入规范化、依赖验证和值类型转换不得静默丢弃用户值。
- 新版Own依赖合同要求：未显式覆盖的下游默认值基于有效上游计算；显式覆盖的下游值保留。Own依赖明确声明、生成期验证，运行时执行强类型计算。
- 上一条必须通过版本化evaluator合同实现。现行整体CalculateTokenValues路径仍遵守[继承架构](../../../architecture/systems/theming/control-design-token-inheritance.md)的既有语义；禁止用重复执行整段计算、解析任意方法体、在各控件补一轮覆盖来假装获得新合同。
- 计算顺序不改变Token名称slot规则；仅移动定义层不改变schema。新旧求值版本、跨程序集继承、默认算法修订与迁移边界必须明确后才能推广。

检查：每条Own派生边分别验证上游覆盖、下游显式覆盖、恢复默认；同时验证明暗、Control算法与继承版本边界。

### TH-04：严格且最小的可配置契约

性质：现行Token定义约束＋新增分类/消费要求。

- Global表达通用设计语言；Own只表达控件独有且稳定的设计语义，不复制Global、不按模板节点穷举字段。
- 每个Active Own Token必须定义名称、类型、单位、合法范围、默认计算、owner、适用状态/尺寸、直接或间接消费者、动态更新及兼容规则。
- 可配置派生值、内部计算值、兼容别名、Reserved与Deprecated必须区分。兼容别名先逐配置层规范化到同一有效语义，再按正常优先级合并；同层冲突诊断，跨层正常覆盖，不维护过期副本。
- 对外已经导出的字段不能因C#是internal而直接删除。当前没有功能支撑的保留字段不能包装成已经生效的编辑项。
- 配置空间继续是完整Global加当前Own。合法未消费Global允许无视觉效果；禁止Global支持/消费白名单，禁止把引用扫描当作合法配置集合。

检查：schema/文档/工具一致性、每个Active Own的消费证据、别名冲突与弃用兼容验证。

### TH-08：完整品牌表达与工具同源

性质：新增产品合同，样式配方机制及协议需单独验证和版本化。

- 完整品牌表达包含实际使用的Token、Control覆盖、专用部件样式及必要模板/静态资产；导出不得丢掉生效表达。
- 框架托管品牌样式遵守有效scope的继承/重置；子品牌没有设置父品牌某个属性时，也不能意外继承已被重置的父规则。
- 普通应用祖先Styles继续按Avalonia规则运行；Inherit=false不等于删除任意用户样式。框架托管品牌规则与普通应用规则的责任必须明确。
- 手动、Gallery与AI只构造同一套类型化受限配置，经过校验、编译、预览和同一Manager应用；不得执行任意运行期C#/脚本/AXAML。
- 品牌切换对已有实例生效并保留输入、选择、焦点、滚动和业务状态；结构模板替换需单独定义状态保存责任。
- 样式准备、接线、激活、通知与清理的失败边界必须分别定义，验证原生selector资格、优先级、资源泄漏与跨根更新后，才能承诺完整品牌切换一致性。

检查：父设置padding、重置子只设置foreground的反例；完整导出再导入等价；普通应用Style边界、失败注入与所有受影响host验证。

### TH-11：AOT、资产、性能与错误语义

性质：现行合同整合；新品牌机制不得绕开现有边界。

- schema、typed identity、专用Style、资产工厂和注册以生成式/static入口接入；禁止运行时程序集扫描、反射造类型和主题脚本引擎。
- 静态包与动态输入遵守同一有效语义，严格维持词法资源、显式include、包启用、注册冻结和惰性物化边界；错误不得静默降级为另一条路径。
- schema/配置检查尽可能提前；惰性Control槽首查失败仍按当前正式合同报告，不因“精确主题”预先构造所有控件。
- Token事务维持现有CommitCore静默交换和Publish不可回滚语义，发布异常通过独立诊断报告。不得把已经通知的Token提交改称普通Failed。
- 品牌样式接线/预激活的恢复位于Token提交前，并有版本化表现结果及故障验证；提交后不回滚Token。具体阶段只由正式配方合同定义，不将原生Style修改塞入CommitCore。
- 性能改进需相同场景前后测量，包含首次使用、已有实例切换、嵌套与弹层、峰值/稳定对象数；性能目标及平台边界在具体任务预先定义。

检查：真实trim/NativeAOT按风险发布、静态/动态差分、故障结果/通知验证、惰性与缓存成本对照。
