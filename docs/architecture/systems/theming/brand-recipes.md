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

### 2.1 工厂与激活键

工厂拥有托管规则的资格边界：保留资产root原有条件，再追加注册exact Control类型的原生类型selector及继承激活属性的`PropertyEquals`条件。不能忽略注册owner类型，也不能静默擦除资产原有条件；嵌套状态规则继续声明在AXAML中。编译AXAML的原生嵌套selector需要明确父类型，不能强制资产使用selector-free root来换取装配便利。既有生成的无参专用Part Style继续作为root的子Style，通过`Nesting()`和声明route接到同一个资格，不要求为每个Part复制资格属性或重新查找节点。

这里的原生类型selector遵循Avalonia的`StyleKey`语义，不是运行期CLR identity解析器。不得用匹配到的StyleKey替代Token注册的真实Control identity；显式复用其他StyleKey的Control需要单独核定视觉owner合同，不能从父集合猜测其Token owner。

规范化配方的内容身份与实际激活键分离。内容身份用于比较等价/修订；激活键是不透明的引用身份，属于一个context的一代表现状态。两个scope即使内容相同也不能共用激活键，否则祖先和子scope的规则可能同时命中。同名键也不能因字符串相等而误命中。继承子有自己的键，并为该键装配完整有效集合；重置子缺失的Setter由基准主题提供，不能靠将父Setter值重新写到子节点“抵消”。

激活键由ThemeContext拥有，scope根和跨根lease负责继承入口。稳定Token context不因换品牌而更换identity；Token Snapshot不保存这些UI对象。工厂每次返回新的、未挂载的Style图；不同host/lease不得复用已经有owner或parent的可变Style实例。以上是装配合同，当前产品接入状态见[验证边界](verification.md)。

准备检查覆盖整张受支持Style图的父关系、独占性和资源；赋值root资格后重新触发Avalonia原生nesting校验。子规则若带独立selector而不以Nesting连接root，会绕过资格，必须拒绝；不能仅检查父规则。已准备节点以弱键记录，禁止移除后再次用于其他代/host，同时不由记录反向保留已退役Style。最小profile只接受原生Style及其派生专用Style节点，未知容器不被静默遍历放行。此检查不替代注册资产的owner、Setter/模板能力、AOT与跨根生命周期校验。

### 2.2 原生优先级的明确合同

Avalonia的资格属性条件产生activator，托管root规则为`BindingPriority.StyleTrigger`，并非无条件`Style`。专用Part的class route本身也产生activator。采用原生selector意味着以下关系，不能另建引擎按“CSS specificity”重新排序：

| 输入关系 | 原生行为 |
| --- | --- |
| 实例局部值与资格规则 | `LocalValue`覆盖资格规则，清除局部值后恢复有效Style |
| 无条件应用Style与资格规则 | 普通`Style`优先级较低；即使host更近，也不能据此覆盖资格trigger |
| 同host的资格/状态/应用class规则 | 同为trigger时按原生顺序，后规则优先；条件退出恢复前规则 |
| 更近host的应用class/专用Part Style与资格规则 | 同为trigger时更近host优先；普通应用规则也能命中重置scope，不被品牌系统清除 |
| ControlTheme状态trigger与应用层资格trigger | 原生frame类型区分仍生效；不能假定ControlTheme状态会自动压过应用层资格Setter |

默认、hover、disabled等主要视觉仍优先通过Token与ControlTheme完成。配方选择用Style覆盖某个属性时，必须显式审查该属性的状态覆盖，不能依靠原ControlTheme状态替它兜底。自定义者需要覆盖品牌时，可用实例值或符合原生优先级的应用class/专用Part Style；不宣传无条件类型Style总能覆盖。无品牌托管规则时不改变既有样式的优先级。

### 2.3 Context 与宿主的装配边界

Context 的表现状态按首次实际使用惰性创建，不在纯 Token 准备、Snapshot 或 CommitCore 中创建带可观察属性的对象。一个 Context 的一代状态拥有独立引用资格与完整有效资产描述；每个需要承载资产的实际宿主创建自己的原生图。运行期选择的 Context 资格通过原生 Style Setter 的强类型 observable 绑定，订阅随 frame 退役，无反射绑定路径或模板节点属性回写。

根应用只向无逻辑父级的 TopLevel 提供根 Context/资格，不能覆盖有 Popup 父级的独立可视根。Provider 和独立窗口 lease 管理自己的资格入口；Popup 的 styling/inheritance parent 已能到达实际 owner 时沿该链继承。如果同 Context 的资产宿主已在真实 styling parent 链中，只安装资格锚点，禁止再在更近的 Window 复制资产以意外提升优先级。不同 Context 的跨根宿主仍需要独占资产图。

候选组在旧组之后、后续应用规则之前插入。退役重挂保留组必须保持原生索引，不能把品牌移动到整个列表最后；应用规则对象及其相对顺序不变。初始化未完成、清理未完成或已有待恢复/退役更新的宿主不能开始下一次更新。完成的句柄须释放旧图、宿主和 Context 引用；失败的更新保留明确且有界的重试责任，不得把恢复失败当作普通成功。以上内部装配原语不等于完整品牌事务入口，当前实现范围由验证文档记录。

## 3. 资源隔离与跨根

候选样式使用未激活资格准备。未命中selector不表示Style.Resources也被隔离，因此不得在准备期把候选公共key资源提前挂入祖先搜索链。模板/静态资产使用明确的不可变引用或按活动generation投影的现有资源入口，保持词法、include与应用覆盖合同。

稳定结构的最小装配profile采用显式资产引用：在接触任何host前验证候选Style图，拒绝非空公共Resources、MergedDictionaries和ThemeDictionaries；递归检查子规则，不只检查外层Style或`HasResources`（主题字典可能未计入该标志）。不支持的容器/include必须拒绝，或由已注册的构建资产机制展开并完成同等检查，不能跳过未知节点。该限制仅针对托管候选的公共资源发布，普通应用Styles不受此准备检查接管。

资源验证不能被解释成完整激活事务：工厂返回的图由框架持有，在准备至退役期间不得被外部修改；后续支持活动generation资源投影须另有装配/恢复证据。模板内部词法资源和已注册的静态引用由其实际资产所有者保持，不通过把候选公共key提前挂载来实现。

Native/Overlay均由实际owner lease接线Token context、托管样式及资格，在显示前完成；已打开host随owner变化更新。迁移、关闭、失败打开和重复释放有对称清理。无owner的独立窗口仍按根主题合同运行，不能推断调用点。

## 4. 协调阶段与现行Token事务边界

现行Token事务的CommitCore仅做无通知引用交换；Publish不可回滚，异常保持Committed并通过PublishDiagnostics报告。品牌协调不得把原生Style装配放入CommitCore，也不得在Token已经Publish后静默恢复旧Snapshot并改称普通Failed。

品牌操作由同一ThemeManager串行协调；目标顺序如下，Token结果与品牌表现完整性分别报告。表现使用独立的版本化结果；现行运行时边界见下文，公开配方注册和 manifest 仍由 CT-09 提供：

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

### 4.1 统一表现协调的现行接口

`ThemeTransitionResult.Presentation` 与 `ThemeCatalogReloadResult.Presentation` 是独立的 `ThemePresentationResult`（`Version=1`）。`Unchanged` 表示本次未更新托管表现；`Applied` 只在提交并完成退役后报告；`Restored` 表示提交前恢复或待清理责任重试完成；`Degraded` 表示仍有未完成的恢复/退役。Token 的 `Status` 不承担表现完整性：提交后退役失败保持 `Committed`，同时给出 `Presentation.Degraded`。

`IThemeManager.Presentation` 与 `ThemePresentationChanged` 公开最近的表现操作结果；`ThemeConfigProvider` 提供对应局部结果/事件。根更新会通知实际受影响的 Provider。既有 `ThemeChanged` 仍属于 Token Publish，不应把该事件当作表现已经退役的信号；根 `ThemeChangeFailed` 在框架恢复尝试结束后发出。普通观察者 warning 不抹去或降级已完成的表现结果。

`RetryPresentationCleanup()` 在原 UI 线程重试准确的待清理对象；Manager Dispose 也会执行一次重试。失败句柄由 Manager 保留，不因外层 Provider/Window 清空字段而丢失；同一对象重试不增加条目，未清理时禁止新的主题变更和 managed host 接入。可在 Manager Dispose 后、原 Dispatcher 仍可用时显式重试。此责任界限不保证恢复任意用户回调副作用或已经结束的平台。

内部 `ThemeStyleBatch` 协调 root/FollowSystem、scope 注册/修改的所有参与 Context。捕获发生在异步 Token Prepare 之前；工厂执行前逐 Context 复核原捕获，防止前一个工厂扩大后一个 Context 的 host 集合。接线后的宿主 stamp 同时比较真实 styling 祖先链与原生样式组引用/顺序，不能仅比较登记计数。所有参与项校验完成后，`CommitPrepared` 只交换代/有效输入引用。

Catalog 重载与 package append 当前继续消费既有不可变资产输入，不重建相同表现。Catalog 在后台准备前捕获宿主，package 在 configure 之前捕获，提交前复核；package 的候选 Token 读取与回滚不会切换表现代际。它们当前没有新增隐式品牌来源；独立 manifest 接入不得绕开这些边界。

资产输入当前是 `ThemeConfig` 的内部只读段，Token snapshot/缓存保持 Token-only。继承 scope 保留父有序资产，本层同 ID 替换放在本层顺序；`Inherit=false` 清空父输入。相同规范 descriptor 集合保留表现代际；仅资产变化仍运行完整协调。descriptor 的规范 ID/revision/factory 注册和能力验证由 CT-09 接入，公开数据入口及其校验见[版本1接口](#61-版本1具体入口与校验)；内部资产段不允许从JSON直接构造工厂。`IThemeManager` 的新增成员提供默认拒绝实现，旧 Token-only 外部实现仍可编译；使用表现协议须显式实现相应能力，默认不会假报成功。最终二进制/AOT兼容审计继续由 CT-25/CT-26 跟踪。

## 5. 失败与恢复

- 准备/接线失败：不提交Token，按实际已发生变更逆序清理，保留旧资格；不能假定抛错API没有修改集合或owner。
- 预激活失败或再校验失效：在Token提交前逐host恢复旧资格及lease，汇总恢复结果。恢复成功才可报告旧品牌保持；恢复失败明确报告表现未完整恢复，不能只给普通失败并暗示外观无变化。
- Token CommitCore之后：遵守现行Committed/PublishDiagnostics。破坏样式激活、绑定或资源传播的异常，以及表现核对失败，必须使品牌整体结果报告降级/未完整应用；不改写Token结果为Failed。普通观察者异常只形成独立发布诊断，没有表现失效证据时不据此判定品牌降级。
- 退役失败：记录可重试清理责任及受影响host，限制未退役资产数量；不得无界积累旧Style、lease或Visual。无法恢复的host须修复/重建后才能继续承诺完整应用。

恢复只覆盖框架持有的资格/资源/lease，不能回滚任意用户回调副作用。该协调需要显式的表现状态和版本化结果扩展，不能复用现有Token结果表达所有阶段。接线/Setter激活错误与普通发布监听器异常必须区分。

退役先使旧资格失活，再清理原生Style frame与owner。不能只从`Styles`集合移除对象就宣称模板节点已经释放：当前Avalonia版本的移除遍历和新增失效遍历覆盖不同，模板frame可能仍持有旧Style。最小原型已验证“按原索引重新挂载保留的活动Style图”可通过原生新增失效路径清除该残留；产品实现仍须在各实际host证明同一UI调度段内完成、原生顺序/实例/交互保持，并验证失败恢复、性能和释放。不用反射调用私有清理API、寻找节点改属性或重建整页替代这项验收，也不将此原型解释为已修复任意应用样式的原生移除行为。

## 6. 配方格式与完整导出

现有XML v1和编译主题ABI v1继续承载原有Token/算法语义，不添加未经版本化的样式元素或默认含义。品牌组合manifest独立版本化，表达Token内容、已注册资产引用/修订、Control要求及平台能力。

完整导出保留全部生效表达；Token-only导出明确标识范围。导入目标缺少必需资产、版本或已启用Control时诊断，不能丢掉样式后声称等价。任意对象、C#/脚本和运行期AXAML均不属于数据导入能力。

### 6.1 版本1具体入口与校验

`AtomUI.Theme.Branding.ThemeStyleAssetRegistration`由`IThemeManagerBuilder.AddThemeStyleAsset`或`IAtomUIBuilder`同名扩展显式登记。ID/revision规范描述符指向编译工厂和已启用的Control owner；登记不执行工厂，也不隐式启用可选包。同ID/revision不得替换工厂，不同修订不得更换owner。package append的候选注册表随既有fence提交或回滚；仅新增登记不激活表现。

`ThemeStyleCapabilities`是协议能力要求。当前接受`QualifiedStyles`、`CompiledTemplates`、`RegisteredStaticAssets`；`ScopedResources`保留但明确拒绝。编译ControlTemplate/DataTemplate/ControlTheme Setter须声明模板能力；空Property、direct/read-only Setter以及已有资源图禁区在接线前拒绝。这些标记不代表已经通过任意OS/AOT验收，也不允许运行期从JSON加载静态资源URI。

`ThemeBrandRecipes.Read(runtime,json)`解析并对当前runtime校验，返回不可变`ThemeBrandRecipeReadResult`及诊断；`Write(recipe)`确定性输出版本1JSON。`ApplyAsync`每次复核后通过既有`ApplyThemeAsync`事务应用；`CreateConfig`返回同样校验并绑定的配置，供`ThemeConfigProvider.Config`使用。绑定要求随配置保留，在根请求去重之前、后台准备及局部编译快捷返回之前复核，不能因旧请求命中NoOp或Token为空而绕过基础修订检查。跨runtime传递已绑定配置时必须重新满足目标注册表的规范资产引用；同名同修订不代表目标runtime已登记该工厂。package append在候选Token编译/发布前检查有效根层及所有LastValid局部配置，拒绝使活动配方要求失效的候选；不兼容追加须先解除或替换相关配方，不把失效留到下一次主题更新。非本runtime实现明确拒绝配方协议。

`ExportCurrent(runtime,id,revision,kind)`从已提交有效Token输入及规范有序资产导出。`Complete`保留所有资产；`TokenOnly`不含资产且在文件内显式标记。导出配置使用`Inherit=false`与已规范化的全局/Control Token和算法，绑定精确schema修订、基础定义ContentDigest及ProgramDigest；不使用机器路径/修改时间作为可移植要求。缺少规范登记的内部资产不能伪报完整导出。导入目标必须满足精确基础要求，再由现有Normalizer处理Token、owner、别名和值类型。

文件固定包含`version`、`kind`、`id`、`revision`、`themeId`、`requires`、`config`、`assets`。`requires`为`schema/definition/program/capabilities`；`config`为`inherit/algorithms/tokens/controls`，每个Control保存`catalog/id/algorithm/algorithms/tokens`；每个资产保存`id/revision`。算法区分`null`与空数组，Control模式沿用`Unspecified/Disabled/Global/Custom`。读取使用手工JsonDocument/Utf8JsonWriter，不做反射对象序列化；输入最多1MiB、深度16、2048个Control、256个资产、32768个Token项、单字符串65536字符，拒绝未知/重复成员、版本及能力，不能降级丢弃未知表达后报告成功。

配方选择使用独立的`ThemeBrandSelection`，由`GetSelection(runtime)`读取。选择ID/revision、有效资产及修订和Token输入共同决定请求等价；纯选择变化提交选择引用并复用已有图，资产内容变化才准备新图。失败保留旧选择；局部继承与`Inherit=false`重置同步作用于选择和资产。Token snapshot仍不持有Style、模板或Visual。上述是现行接口边界；全部控件品牌覆盖、平台能力与产品视觉出口仍以任务表实际验收为准。

## 7. 验证合同

必须覆盖父品牌有padding、重置子只设置foreground的反例；同时覆盖嵌套/兄弟、public子Control、重挂载、新建/已打开Native及Overlay、资源不提前泄漏、资格与原生优先级。

故障注入覆盖AddOwner/RemoveOwner、资格激活/回退、stamp失效、Setter与资源通知、退役和重复关闭；分别检查Token结果、表现结果、所有host的实际状态及释放，不能只检查最后一个返回值。

Ant默认、shadcn、Bootstrap与零圆角/粗边框/大字号/紧凑/关闭动效配方贯穿实际渲染验证。实现证据不足时不将目标合同写成已经支持完整品牌原子切换。
