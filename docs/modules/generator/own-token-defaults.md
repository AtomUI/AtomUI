# Own Token 默认值生成设计

本文拥有依赖默认值合同的生成输入、跨程序集元数据、扁平计划、确定性与AOT输出。有效值语义由[Own求值架构](../../architecture/systems/theming/own-token-evaluation.md)拥有，现行类型/属性继承见[继承架构](../../architecture/systems/theming/control-design-token-inheritance.md)。实现状态见[主题验证边界](../../architecture/systems/theming/verification.md)。

## 1. 输入与版本

Generator显式区分现行整体evaluator和依赖默认值合同。旧定义不因升级Generator而静默改变求值语义；新终端及其复用链完整选择新合同，禁止混合执行旧整体方法与新逐Token计划。

新声明事实包括目标Token、常量默认或可调用静态默认函数、参数与Own依赖、值类型、默认重定义和evaluator修订。标记、元数据和生成入口需作为版本化构建契约交付；本文的模型名称不表示已存在同名Attribute或公开方法。

默认上下文只提供Effective Global及已解析appearance，Own上游作为强类型参数传递。不得传整个builder以绕过依赖声明，也不得读取方法体推导任意数据流或在构建期执行第三方代码。

## 2. 定义链与跨程序集

抽象层导出纯构建元数据，不产生identity、运行时slot或独立descriptor。Generator通过Roslyn符号读取已编译声明，不加载程序集执行代码。

终端使用partial承载生成求值入口，直接调用声明函数；共享函数必须对终端生成代码可访问。元数据明确声明类型、方法签名、返回/参数类型及依赖Token，不以仅方法名猜测重载。终端合并整个链的默认贡献，显式重定义之外的重复与不可访问输入诊断。

属性扁平化继续按现行规则，不因默认函数存在而允许隐藏属性、跨owner继承identity或基类descriptor fallback。新合同不再要求调用`base.CalculateTokenValues`；旧合同的base验证继续生效。

## 3. 生成管线与输出

1. 合并终端属性及默认声明，验证版本、类型、访问性、唯一性和参数映射。
2. 规范化显式别名关系，验证未知目标、循环及语义冲突；配置层合并语义仍由Core拥有。
3. 拓扑排序Own默认依赖并生成确定的直接赋值/调用代码；循环和未知依赖抑制部分产物，避免级联无效输出。
4. 生成evaluator版本与revision、默认/依赖元数据和原有typed key/投影的兼容组合。
5. 将直接函数调用、真实owner、schema和必要资产纳入原有单项factory/TypeMap保留条件。

运行时只有强类型顺序执行，不构造依赖图、不反射、不为每个Token分配闭包。Own slot继续按名称，拓扑顺序不进入slot分配；只移动声明层不改变终端schema revision，默认算法变化进入evaluator revision。

## 4. 增量与缓存

类型/依赖/函数签名、重定义、访问性、版本或相关元数据变化使受影响终端输出失效。默认实现语义变化必须同步提升声明的evaluator revision，不能仅依赖签名摘要。

生成器缓存稳定值模型、metadata name与规范化来源，不跨Compilation保存symbol、SemanticModel或Visual。各输出来自同一源模型，不读取其他Generator产物形成隐式生成顺序。

## 5. 诊断与兼容

诊断包含owner、Token、声明来源及依赖链；metadata无源码位置时包含完整类型/方法身份。新的诊断编号按[编译诊断规范](../../engineering/development/compiler-diagnostics-guidelines.md)分配，不把本文条款编号当作实际编译错误码。

旧schema、资源键、编译主题ABI及Token属性声明在未版本化变更前保持；构建期默认元数据扩展不意味着可以向XML v1或ABI v1加入未经定义的字段。跨程序集提供新抽象层的包必须发布匹配构建契约与Generator版本。

## 6. 验证

覆盖常量/函数默认、显式上下游、重定义、访问性、循环、非法参数、混合版本、同/跨程序集、声明层移动、修订失效及确定输出。验证生成evaluator的运行语义与动态配置一致，并通过真实包消费/trim/NativeAOT；不得以生成文本包含某个方法名代替行为证据。

## 7. CT-03接口到生成产物

具体Attribute、上下文、`IControlTokenDefaults`及descriptor签名统一由[Own求值接口](../../architecture/systems/theming/own-token-evaluation.md#9-ct-03具体构建与运行接口)拥有，不再定义另一套模型名称。

- 新版终端必须声明partial和`IControlTokenDefaults`；生成其`public static EvaluateDefaults(AbstractControlDesignToken, in ControlTokenDefaultContext, ReadOnlySpan<ulong>)`。旧生成器缺少实现时由C#编译失败，不能降级整体evaluator。
- `ControlTokenPropertyWalker`保留字段扁平化，额外生成脱离symbol的默认计划：版本、层revision、按名称字段、默认方法完整类型/方法名、Own参数映射、重定义与确定evaluator revision。只在当前Compilation内使用symbol做类型/可访问性检查，不将其保存在增量输出中。
- 仅生成静态拓扑顺序；每步先读对应存在位，再直接赋值并调用强类型函数。拓扑不改变Own slot；descriptor factory直接引用该入口，没有反射或逐字段delegate。
- Core的完整Global schema同时生成`ControlTokenGlobalValues`只读getter，每个合法Global字段均包含。复用typed codec的复制合同，不能返回mutable Global builder、活动资源或共享mutable brush。
- 旧版仍执行原Calculation validator及原factory输出。新版诊断覆盖版本／partial／接口／revision、默认声明／覆盖／可访问性、参数／返回类型和未知／环依赖，沿用诊断集中管理与编号规范；错误终端不发布可用的部分默认计划。
- 默认元数据来自普通CLI Attribute与方法签名；跨程序集通过Roslyn读取，不另建运行期反射注册表。启动schema及编译主题ABI继续按原值域合同处理，不将evaluator版本摘要伪装为Token schema变更。

## Own别名生成

`ControlTokenAliasModel`读取同/跨程序集的属性Attribute，验证直接规范目标、精确类型/nullability、终端partial/静态接口及禁止手写入口；错误使用`ATOMUIGEN044`。输出只保留名称对，不持有Roslyn symbol。`ControlTokenAliasesWriter`生成终端`IControlTokenAliases.GetAliases()`及中立只读表；schema factory直接引用后者，元数据读取不执行Token构造或静态初始化。

Own slot仍包含全部历史字段且按名称排序。V2默认计划跳过别名的默认要求和求值，拒绝以别名为默认目标或Own输入，规范字段步骤仍使用原slot。最终历史值投影由Compiler完成，生成器不维护另一套运行时图，也不更改XML/compiled格式。
