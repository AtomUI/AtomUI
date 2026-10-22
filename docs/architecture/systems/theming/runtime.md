# AtomUI 主题系统架构

本文定义 AtomUI 主题系统的运行时模型、公开配置语义、主题来源解析、主题文件
处理、Catalog 刷新、Token 编译、动态作用域、资源发布、事件契约、AOT 边界和验收标准。

本文定义当前架构。既有兼容对象不得成为绕过这些约束的旁路。面向主题作者和 Control 开发者的分层讲解与定制
示例见 [AtomUI 主题定制指南](../../../guides/theming/customization.md)。

依赖默认值与完整品牌分别由[Own有效求值](own-token-evaluation.md)和[品牌配方](brand-recipes.md)定义目标合同；
[Control渲染](control-rendering.md)定义消费终点。它们不静默改写本文的整体evaluator、Token-only请求与五阶段结果，
源码迁移和验收边界见[验证文档](verification.md)。

包内 XML 类型化注册与默认赋值见[编译主题包](compiled-theme-packages/overview.md)。派生算法统一在运行时执行，
并遵守本文的配置、Snapshot、事务与资源语义；当前验证状态由编译主题包文档维护，已发布能力以对应发布记录为准。

## 1. 设计目标

主题系统必须满足以下不可协商的约束：

- 建立完整的 Seed Token、Map Token、Alias Token、Control Token、算法和嵌套配置模型。
- AtomUI 的公开 API、内部类型、资源键和文档统一使用 `Control` 术语。
- `ThemeSnapshot` 是运行时 Token 值的唯一真源。
- 全局主题和局部主题使用同一个配置合并器、编译器、缓存和提交协议。
- 主题变更先完成整个受影响作用域子树的编译，再在 UI 线程完成无通知引用交换；任何外部观察者只能在
  Publish 阶段看到完整新状态。
- `ThemeConfig`、`ControlThemeConfig` 和 `ThemeRequest` 是深度不可变值，动态更新通过替换完整配置表达。
- AtomUI Token appearance 与 Avalonia `ActualThemeVariant` 在根作用域和每个局部作用域中始终一致。
- 控件级配置只参与目标控件自身的 Token 计算，不形成控件内容树资源作用域。
- 每个主题作用域只拥有一个稳定的 Token 资源 Provider。
- 每个对外可主题化的 Control 都拥有独立 `ControlTokenIdentity`；TargetType、Control 继承和 ControlTheme
  `BasedOn` 都不能替代或推断该 identity。
- AXAML 中 `{atom:SharedTokenResource TokenName}` 永远读取全局 Token；需要响应 Control 级覆盖时统一使用生成的
  `{atom:XxxTokenResource TokenName}`，它同时读取该 Control 的 Effective Global Token 和 Own Token。
- ControlTheme 不携带 ambient Token scope；Token 所属 Control 必须由资源扩展名显式表达。
- Token 资源查询热路径不分配对象，不执行字符串解析、反射或 LINQ。
- 局部主题只复制发生变化的数据，不能按作用域复制完整全局 Token 和全部 Control Token 值。
- 缓存、作用域图、事件订阅和异步编译任务都必须有确定的容量或释放边界。
- 普通 Popup 通过 Avalonia 逻辑树继承主题；独立 TopLevel 必须通过显式 owner context lease 获得局部主题。
- 无 owner 的静态 Dialog、Notification 等 API 只能使用根主题，不能推断调用位置的局部上下文。
- 内置正常路径不使用运行时程序集扫描、`Activator.CreateInstance` 或 Token 属性反射。
- 内置资源、应用资源和用户配置目录通过统一的 `IThemeDefinitionResolver` 边界产生主题来源；Resolver 不解析
  Token，也不绕过标准 Reader、Binder 和 Compiler。
- 用户主题目录只允许显式启用，手动刷新必须原子更新 Catalog 与全部受影响 Snapshot；失败时旧状态完整保留。
- 第一个可见帧之前完成全局主题准备和挂载。
- 不保留旧主题系统的兼容对象、克隆查询 API、伪生命周期事件或可变资源旁路。

## 2. ThemeConfig 设计结论

### 2.1 主题语义与 Avalonia 映射

AtomUI 主题系统使用静态 `ControlTheme`、typed resource key、snapshot-backed `ResourceProvider` 和显式生命周期
宿主实现主题配置合并、Token 派生、算法替换、Control 级算法策略和嵌套作用域。本文定义这些能力的长期功能契约；
代码、配置模型、主题资产和其他文档必须与本文保持一致：

- `ThemeConfig` 对 Token 按 key 合并，对 Control 配置先按 identity、再按字段合并；`Algorithms` 整体替换。
- `Inherit=false` 切断父配置继承；root 运行时请求保留当前所选 `ThemeDefinition` 基线，局部 Provider 回到
  AtomUI library defaults。
- Token 按 Seed、Map、Alias、Control 四层派生；每个 Control 都可以覆盖当前 registry 中的任意 Global Token。
- 算法是有序 derivative 链；每个算法接收同一份 Seed 和前一算法的 Map 结果。
- 主题计算和 Control 计算按完整内容缓存，缓存身份包含算法链和所有会改变结果的配置。
- 嵌套主题依赖上下文隔离；脱离上下文的新渲染根不能自动获得调用点主题。
- Token-only更新保持样式结构稳定，只替换作用域内Token值；显式品牌配方的目标装配与资格变化遵守独立的[品牌合同](brand-recipes.md)。

AtomUI 主题系统不引入 CSS selector hash、CSS Variable 命名、style tag 注入、CSS 序列化、SSR 提取或 Web
生命周期。主题结构和局部作用域统一由静态 ControlTheme、稳定资源 key、snapshot-backed ResourceProvider、
Avalonia 逻辑树和显式资源宿主生命周期表达：

```text
Static ControlTheme + scoped snapshot resources
```

因此 AtomUI 不需要 `zeroRuntime` 或 hashed style 开关。ControlTheme 本来就在构建期编译，运行时只计算和发布
Token snapshot；品牌组合复用编译资产，不在此路径增加运行期CSS/AXAML解释器。

### 2.2 公开配置语义

AtomUI 使用统一的 `ThemeConfig` 描述全局主题和局部主题输入：

| 配置项 | 设计结论 |
|---|---|
| `ThemeConfig.Tokens` | 覆盖当前作用域的全局 Token |
| `ThemeConfig.Controls[ControlTokenIdentity]` | 覆盖目标 Control 的 Own Token 和当前 registry 中的任意 Global Token |
| `ThemeConfig.Algorithms` | 定义 Seed 到 Map、Alias 的有序算法链 |
| `ThemeConfig.Inherit` | 决定是否继承父作用域有效配置，默认 `true` |
| `ControlThemeConfig.Algorithm` | 定义目标 Control 的局部算法策略 |

AtomUI 的控件算法必须能表达四种状态：

1. 未指定：继承父配置中同一 Control 的算法策略。
2. 禁用：Control 内覆盖的 Seed Token 不重新派生 Map 和 Alias Token。
3. 使用全局算法：使用当前作用域的全局算法链重新派生该 Control 的有效 Token。
4. 自定义算法：使用 Control 自己声明的有序算法链。

继承链没有提供 Control 算法策略时，未指定最终解析为 AtomUI 默认策略“禁用”。

算法配置的领域模型是 `Unspecified / Disabled / Global / Custom` 四态策略。可空 `bool` 加自定义算法列表
虽然可以间接编码这些状态，但会把一个领域值拆成两个耦合字段，并产生布尔值与自定义列表同时声明等无效
组合。因此配置规范化后必须使用 `ControlAlgorithmMode` 枚举；Custom 的有序 `ThemeAlgorithm` 枚举值作为不可变
payload 保存，并且只允许在 Mode 为 `Custom` 时非空。`bool` 或 `bool?` 不能作为内部真源。

`ThemeConfig`、`ControlThemeConfig` 及其全部集合在构造完成后不可修改。公开构造器或
`ThemeConfigBuilder` 必须防御性复制输入；Builder 只是一次性构造工具，不进入 Provider、Manager、snapshot 或
缓存。运行时绑定通过替换 `ThemeConfigProvider.Config` 更新主题，不支持对已经提交的 Config 内部集合做原位
修改。

## 3. 总体架构

定义与编译路径：

```text
+--------------------------+     +-------------------------+     +---------------------+     +-----------------------+
| ThemeDefinitionResolvers | --> | ThemeDefinitionSources  | --> | ThemeDocumentReader | --> | ThemeDefinitionBinder | --+
+--------------------------+     +-------------------------+     +---------------------+     +-----------------------+   |
                                                                                                                        |
+-------------+     +-----------------------+                                |
| ThemeConfig | --> | ThemeConfigNormalizer | -------------------------------+
+-------------+     +-----------------------+                                |
                                                                             v
                                                                    +-------------------+
                                                                    | ThemeConfigMerger |
                                                                    +---------+---------+
                                                                              |
                                                                              v
+---------------------+        supplies descriptors                 +-------------------+
| ThemeSchemaRegistry | ------------------------------------------> | ThemeCompiler      |
+---------------------+                                             +---------+---------+
                                                                              |
                                                                              v
                                                                    +-------------------+
                                                                    | ThemeSnapshotCache |
                                                                    +-------------------+

ThemeSchemaRegistry 还为 Binder、Normalizer 和资源投影提供 descriptor。
```

运行时发布路径：

```text
+--------------------+        +--------------+
| ThemeSnapshotCache | -----> | ThemeManager |
+--------------------+        +------+-------+
                                    |
                         atomic snapshot commit
                                    |
                    +---------------+---------------+
                    |                               |
                    v                               v
          +-------------------+           +-----------------+
          | Root ThemeContext |           | ThemeScopeGraph |
          +---------+---------+           +--------+--------+
                    |                              |
                    |                              v
                    |                    +--------------------+
                    |                    | Child ThemeContexts |
                    |                    +----------+---------+
                    |                               |
                    +---------------+---------------+
                                    |
                                    v
                      +----------------------------+
                      | ThemeTokenResourceProvider |
                      +-------------+--------------+
                                    |
                                    v
                      +----------------------------+
                      | AXAML and Control runtime  |
                      +----------------------------+
```

系统分成七个边界：

- 来源层：Resolver 只发现可重开读取的主题定义来源，并提供稳定 source identity/revision。
- 定义层：统一读取所有来源并生成与运行时注册无关的语法模型。
- Schema 层：提供生成式 Token 元数据、强类型赋值器、资源投影器和 Control 身份。
- 配置层：绑定不可变公开配置，生成不可变规范化配置，并执行 AtomUI 主题配置合并。
- 编译层：纯计算 Token，输出不可变 `ThemeSnapshot`。
- 运行时层：管理根主题、作用域图、事务、并发、缓存和事件。
- 资源层：把当前 snapshot 投影到 Avalonia 资源查询，不拥有第二份 Token 状态。

## 4. 配置模型

### 4.1 公开输入

`ThemeConfig` 是面向 C#、AXAML 和绑定的不可变配置输入：

```text
ThemeConfig
+-- Inherit = true
+-- Algorithms: optional ordered ThemeAlgorithm list
+-- Tokens: global token overrides
\-- Controls[ControlTokenIdentity]
    \-- ControlThemeConfig
        +-- Algorithm: unspecified / disabled / global / custom
        \-- Tokens: control-own and global token overrides
```

`ThemeConfig` 使用只读集合和不可变 entry。C# 可以通过一次性 Builder 构造，AXAML 通常把 ViewModel 中的
不可变 Config 绑定到 Provider；需要内联声明时，由构建期配置扩展生成同一不可变值。不存在“修改已加入集合
的 setter 后自动编译”的隐式行为。

`ThemeConfigProvider` 只把以下变化视为新输入：

- `Config` 属性被替换。
- Provider attach、detach 或 reparent 导致父 ThemeContext 改变。

Provider 不订阅 Config 内部对象图。Capture 在 UI 线程读取一个深度不可变引用，因此后台 Prepare 不需要再次
复制，也不会观察到中途变化。旧 Config 在属性替换后不再被 Provider、Manager、Compiler、snapshot 或缓存
保留；缓存只保存规范化后的结构化 key。

`Algorithms=null` 表示“未指定”；非空集合表示显式算法链；空集合表示显式替换为 AtomUI 默认算法链。
Control 自定义算法链仍必须声明至少一个算法。

公开配置只接受 `ThemeAlgorithm`：

```csharp
new ThemeConfigBuilder()
    .WithAlgorithms(ThemeAlgorithm.Default, ThemeAlgorithm.Dark)
    .Build();
```

`ThemeConfig.Algorithms`、`ControlThemeConfig.Algorithms` 和 `ThemeState.Algorithms` 都是只读枚举列表。系统不提供
字符串 Builder 重载，也不在配置、状态、descriptor、registry 或 cache key 中保存字符串算法身份。未定义的强制
转换枚举值由 Normalizer 拒绝。

`ThemeConfigProvider` 是轻量局部 ThemeVariant 宿主，只公开一个 `Config` 和继承的内容子节点。旧的
`SharedTokenSetters`、`ControlTokenInfoSetters`、字符串算法集合以及 Token 查询属性不属于新架构。

### 4.2 规范化配置

`ThemeConfigNormalizer` 对一次不可变输入完成以下工作：

- 解析并验证 Token key 和 value。
- 把 Control id 解析为 `ControlTokenIdentity`。
- 保留算法的“未指定”状态。
- 将公开 identity 和 value 绑定为当前 registry revision 下不可变、有确定顺序的值对象。
- 计算稳定的结构 fingerprint。
- 返回 diagnostics，不抛出缺少上下文的反射异常。

编译器只接收 `NormalizedThemeConfig`。Normalizer 不读取 Visual、ResourceHost 或全局服务。

### 4.3 合并规则

`ThemeConfigMerger` 是全局和局部主题唯一的合并入口：

配置优先级从低到高固定为：

```text
ThemeSchemaRegistry / AtomUI library defaults
-> selected ThemeDefinition
-> builder/application initial config
-> runtime ThemeRequest.Config
-> parent scope effective config
-> local ThemeConfigProvider.Config
```

根事务先把所选 `ThemeDefinition` 应用到 AtomUI library defaults，再应用 builder/application initial config。
`ThemeRequest.Config` 为 `null` 时保留该根配置；非空时按 `Inherit` 合并：`Inherit=true` 继续继承应用初始
配置，`Inherit=false` 只丢弃较低优先级的应用初始 config，以“AtomUI library defaults + 当前所选
ThemeDefinition”为根基线。选择某个主题后，runtime config 不能通过 `Inherit=false` 意外删除该主题定义。

局部 `ThemeConfigProvider` 的父输入是父 `ThemeContext` 的完整有效配置。局部 `Inherit=false` 的基线是
AtomUI library defaults，不继承父作用域、当前所选主题定义、应用初始配置或 runtime override；随后只应用
当前 Provider 的本地配置。

- `Inherit=true` 时以父作用域的有效配置为基础。
- `Inherit=false` 时使用上述入口对应的基线，不隐式复制父有效配置。
- `Config=null` 等价于不可变空配置 `Inherit=true`，只共享父有效结果。
- 当前 `Tokens` 按 key 覆盖父 `Tokens`。
- `Controls` 先按 `ControlTokenIdentity` 合并，再按 Token key 合并。
- Control 的算法未指定时继承父策略；显式 disabled、global 或 custom 时替换父策略。
- 当前 `Algorithms` 为 `null` 时继承基线算法；显式提供列表时整体替换，不和基线算法拼接；空列表先规范化为
  AtomUI 默认算法链。
- 主题定义、应用级配置和运行时请求都先转换为规范化配置，再按同一规则合并。

运行时算法列表是整体替换，不和父层或 definition 的算法链拼接。编译器始终从 Light baseline 重新折叠当前
有效算法链，因此父 Dark + 子 Compact 的结果是 Light Compact，而不是保留父 Dark。要得到 Dark Compact，
必须显式提供完整算法链。全局算法 descriptor 的 appearance effect 按顺序折叠，最后一个非 `Preserve`
effect 决定结果。Control 自定义算法只影响该 Control 的 Token 和 Control appearance，不改变所在
ThemeContext 的 Avalonia ThemeVariant。

Control 的 `Tokens` 使用一个扁平集合表达 Own Token 和 Control 级 Global Token 覆盖。每个 Control 天然可以
覆盖当前 registry 中的任意 Global Token，不维护或推导 Global Token 白名单。Schema Binder 先在当前
`ControlTokenDescriptor.OwnTokens` 中查找名称，未命中时再在完整 Global Token schema 中查找；两处都未命中
才产生未知 Token 错误。Own Token 与任意 Global Token 禁止同名，因此分类没有歧义。一个合法 Global Token
即使未被当前 Control 的计算或主题资产消费也允许配置，结果可能没有视觉效果，但不能影响其他 Control 或真正的
Global Token snapshot。

## 5. Theme Schema 与注册

> 本节注册与资产模型对应本地已实现的 TypeMap 契约，验证与发布状态见 [AOT 与裁剪架构](../../foundations/aot-and-trimming.md#1-状态与事实边界)。

`ThemeSchemaRegistry` 在应用主题初始化前一次性构建。Control、Token 和主题资产只来自源生成结果；
主题算法可以通过显式 `ThemeAlgorithmDescriptor` 注册：

```text
ThemeSchemaRegistry
+-- GlobalTokenSchema
|   \-- token name, kind, stage, value codec, token accessor, resource key
+-- ControlTokenDescriptor[ControlTokenIdentity]
|   \-- exact CLR type, factory, own token schema, evaluator, resource projector
+-- ControlThemeAssetManifest
|   \-- asset identity, exported themes, required Token owners, semantic bindings, resource order
\-- ThemeAlgorithmDescriptor[ThemeAlgorithm]
    \-- revision, evaluator, AOT factory, appearance effect
```

每个 `ThemeAlgorithmDescriptor` 必须声明稳定的 `ThemeAlgorithm` 枚举值和显式 revision/version。算法实现、默认
参数、依赖 Token 或输出语义发生变化时必须提升 revision，不能只保留相同枚举值让旧缓存继续命中。当前封闭集合
只有 `Default`、`Dark` 和 `Compact`；不支持第三方自由字符串算法 ID。

算法实现使用 `ThemeAlgorithmAttribute(ThemeAlgorithm, revision, appearanceEffect)` 声明元数据。源生成器读取
Roslyn enum typed constant 并生成枚举 Descriptor 构造表达式；运行时不反射扫描算法，也不建立字符串注册路径。
增加算法时必须追加新的显式枚举值，并同步算法实现、Attribute、XML XSD 枚举和契约测试。

算法 descriptor 的求值契约固定为：

```text
Evaluate(effectiveSeed, previousMap?) -> nextMap
```

同一算法链中的 `effectiveSeed` 引用和值保持不变；第一个算法接收 `previousMap=null`，后续算法接收前一个算法
输出的 Map。算法不得修改 Seed 或 previous Map，必须返回新的不可变语义结果或只在本次编译 workspace 中写入
目标 builder。Dark、Compact 在没有 previous Map 时可以内部调用 Default，但这种 fallback 属于算法实现，
不能改变通用链契约。

`ThemeSchemaRegistry.Revision` 根据排序后的完整 descriptor schema 确定性计算，输入至少包含 Global Token
schema、Control identity、Control CLR type、Own Token name/kind/stage/value type/resource key schema、主题资产
结构、算法枚举值/revision 及其外观声明。ControlTheme 或 C# 实现消费了哪些 Global Token 不属于 schema，也不进入
revision。Control Own Token 属性声明在终端类型还是显式标记的抽象定义层、定义层所在程序集和继承深度也不进入
revision。注册顺序不影响 revision；任何会改变配置绑定、Token 类型或资源投影结构的 descriptor 变化都必须改变
revision。

Registry 在首个 snapshot 编译前冻结。Manager 构建完成后不能追加 Token、Control、算法或主题资产
descriptor；可选控件包必须在 `UseAtomUI` Builder 阶段完成显式注册。

Token schema slot 与冻结时绑定的 Control slot 只保证在同一 registry revision 生命周期内稳定。不同 revision 可以重新
分配 slot，公开协议、持久化配置和跨版本缓存不得依赖整数 slot 不变，而应使用稳定 identity 并在绑定后解析
当前 revision 的 slot。

源生成器必须为每个内置、对外可主题化的 Control 生成以下内容；Control 没有 Own Token 时仍生成 identity、
descriptor 和资源扩展，只省略 Own Token builder/schema：

- 稳定的 `ControlTokenIdentity`，以 catalog/control id 保持字符串相等语义；生成身份附带实际使用和校验的 owner Type。
- 发现无参数 `[ControlDesignToken]` 标记的可选 Own Token 类型；标记的抽象类型只贡献
  定义，标记的 `sealed` 具体类型是终端 Own Token。Attribute 和抽象定义层都不参与 Control identity 或资产关联。
- Own Token 的强类型读写与 Avalonia resource value 投影；没有 Own Token 时为空。生成代码不为每个 Token 生成委托：
  隐藏 ABI `TokenDescriptor.CreateGenerated(name, slot, stage, codec, resourceKey, accessor)` 接收按值类型共享的
  `TokenValueCodec<T>.Instance`（Parse/Format）和按 Token 类生成的 `TokenValueAccessor` 子类（按 descriptor slot
  switch 读写与投影，投影仍用属性静态类型选择 `ThemeResourceValue.Project` 重载）。accessor 只由所属 descriptor
  工厂引用，随未用控件一起裁剪；包级 lambda 静态缓存不再随全部 Token 保留。公开委托构造函数保留，旧生成代码无需重编。
- 可选 Own Token builder 的直接构造委托。
- Control CLR type 和 Own Token schema；descriptor 不包含 Global Token 消费白名单。
- 生成式 `XxxTokens.Identity`、`XxxTokenKey` 和 `XxxTokenResourceExtension`；Token key 必须强类型化，不能在
  ProvideValue 热路径解析字符串。
- 每个资产的 AssetId/URI、导出的默认或命名主题、RequiredTokenOwners、Semantic Theme 关联和确定资源顺序。
- 每个 Control 的生成式 Semantic Part descriptor、Selector class、ContractType 和 cardinality。
- 每个包的生成式注册 helper 和 manifest；由包作者的公开 `UseXxxControls()` 入口一次注册完整 descriptor 和资产，
  不要求逐 Control 手工注册。

`XxxTokenResource` 是 Control identity 的显式 AXAML 边界。它使用同一种语法读取 Effective Global Token 和
Own Token；两者的分类只存在于 schema、配置和编译层，不泄漏到 Setter 调用语法。

包注册直接传递包含 exact CLR type 与 identity 的完整描述符。Registry 同时建立 exact `Type -> descriptor/slot`
和 `ControlTokenIdentity -> descriptor/slot` 两张不可变映射，不允许从基类、`TargetType` 或 `BasedOn` 回退推断。
内置路径删除
`Activator.CreateInstance`、`Type.GetProperties`、`PropertyInfo.GetValue/SetValue` 和枚举反射。

Control Design Token CLR 继承只在生成期复用 Own Token 定义。Generator 把显式标记的抽象链扁平化到终端
descriptor，运行时不保存声明层、继承深度或基类 identity。完整契约见
[Control Design Token 继承架构](control-design-token-inheritance.md)。

第三方控件包调用统一的生成式包注册 helper，作者只维护 Provider 和正常生命周期逻辑。普通生成器提供逐控件片段、
TypeMap 条件及包引导信息；Runtime 只查询映射和收集记录，官方 linker 计算保留闭包。内部资源控件不被迫拥有公开 Token
身份；普通与裁剪路径调用同一 descriptor factory。完整接入见[第三方控件包指南](../../../guides/theming/third-party-control-packages.md)。

带 owner 的 identity 在配置/资产/注册集合去重之前规范化：raw 与 typed 合并保留 owner，冲突 owner 立即失败，最终 registry
再次验证实际 ControlType。不得让 Dictionary 保留旧的 raw key 而丢失新输入的类型约束。

所有包先收集后统一校验跨包依赖；资源按 PackageCommitOrdinal 和包内声明规则排序。全量与选中路径共同保留的资产具有同样的
相对优先级，开放宿主资源输入遵守相同契约。注册表冻结后不再追加片段。详细事实由
[控件与资源注册契约](../../foundations/control-registration-contracts.md)拥有。

## 6. 主题文件

### 6.1 文件结构

主题定义 XML v1 使用版本化 namespace、显式 identity 和 Control 术语：

```xml
<?xml version="1.0" encoding="utf-8"?>
<Theme xmlns="https://atomui.net/schemas/theme/v1"
       Id="DaybreakBlue"
       Name="Daybreak Blue"
       Appearance="Light"
       IsDefault="true">
  <Algorithms>
    <Algorithm Id="Default" />
  </Algorithms>
  <Tokens>
    <Token Name="ColorPrimary" Value="#1677FF" />
    <Token Name="ColorLink" Value="#1677FF" />
  </Tokens>
  <Controls>
    <Control Catalog="AtomUI" Id="Button" Algorithm="Global">
      <Tokens>
        <Token Name="ColorPrimary" Value="#0958D9" />
        <Token Name="ContentFontSize" Value="14" />
      </Tokens>
    </Control>
  </Controls>
</Theme>
```

规则如下：

- namespace `https://atomui.net/schemas/theme/v1` 是格式版本，必须显式声明。
- `Theme.Id`、`Appearance` 和顶层 `Algorithms` 必须显式声明。
- `Appearance` 是从 library Light baseline 折叠 definition 算法 effect 后的最终断言，不是覆盖算法结果的独立
  开关。
- 算法使用有序 `Algorithm` 元素，不使用逗号分隔字符串。
- Control 使用 `(Catalog, Id)` 组成稳定身份。
- Control 的 `Algorithm` 省略表示未指定，`Disabled` 表示禁用，`Global` 表示使用全局算法链。
- Control 自定义算法使用子元素 `Algorithms`，并且不能同时声明 `Algorithm` 属性。
- Token 是空元素，必须通过 `Value` 属性提供非空值，不允许正文值或类型分类属性。

完整格式、词法、安全限制、diagnostic 和版本演进规则见
[主题定义 XML v1 规范](../../../reference/theming/theme-definition-xml-v1.md)，机器可读标准见
[AtomUI Theme Definition XML Schema v1](../../../reference/theming/schemas/atomui-theme-v1.xsd)。

### 6.2 两阶段处理

`ThemeDocumentReader` 只负责：

- 使用禁止 DTD、外部实体和网络解析的 validating `XmlReader`，并显式启用 XSD identity constraint。
- 通过进程级复用的 v1 XSD 校验 namespace、结构、属性、容量和局部唯一性。
- 保留源码位置并执行 XML 元素总数等 Reader 级资源限制。
- 输出不依赖当前 Control 注册集合的 `ThemeDocument`。

显式编译的包内 XML 在构建时由 Toolchain Tasks worker 链接同一 Reader 源码和权威 XSD 校验，向 Generator 提供
有界中立输入、source location 和原始字节 SHA-256；无效输入阻止构建。运行时使用生成的类型化程序，
不创建 syntax document 或初始化运行时 XSD。原始 avares 资源仍可保留；显式动态 XML 继续经过标准 Reader。

`ThemeDefinitionBinder` 负责：

- 使用 `ThemeSchemaRegistry` 解析 Token，并按 Reader 已转换的 `ThemeAlgorithm` 枚举绑定算法 descriptor。
- 将字符串值一次性转换为目标类型。
- 校验未知 Token、未知 Control、算法冲突、Appearance 一致性和跨字段语义。
- 输出不可变、typed `ThemeDefinition` 和结构化 diagnostics。

Reader 不得因为某个可选控件包没有安装而修改或过滤语法树。Catalog 保存完整 diagnostic，并决定某个
定义是否可用于当前 registry。`ThemeDefinitionRevision` 是由 Catalog 生成的结构化值，至少包含稳定 source
identity、source revision 和内容摘要：文件来源使用实际字节内容摘要，不能只使用时间戳；包内资源包含包版本、
资源路径和内容摘要；程序化来源必须显式提供 revision。来源字节或程序化定义语义变化时 revision 必须变化。
有效缓存可复用同一内容摘要的解析结果，以及同一真实 registry 实例上的绑定结果；驱逐后允许重新准备，不能承诺
进程生命周期内只执行一次。更换 registry 时不能复用旧 slot；可在有界缓存命中时复用已有 `ThemeDocument`。

### 6.3 Theme Definition Resolver

动态 XML 来源通过 Resolver 发现。Resolver 只描述“有哪些来源以及如何读取原始字节”，不得自行执行 XML 解析、
Token 绑定、算法求值或资源发布。公开契约使用 `Definition` 而不是 `Config` 术语，因为 `ThemeConfig` 已经专指
运行时 Token/Algorithm 覆盖；主题 XML 是包含 Id、Name、Appearance、默认标记和 revision 的完整 definition。

```csharp
public interface IThemeDefinitionResolver
{
    string Id { get; }
    bool SupportsReload { get; }

    ThemeDefinitionResolveResult Resolve(
        ThemeDefinitionResolveContext context);
}

public interface IThemeDefinitionSource
{
    string SourceIdentity { get; }
    string SourceRevision { get; }

    Stream OpenRead();
}
```

`ThemeDefinitionResolveResult` 包含不可变 source 列表和结构化 diagnostics；Resolver 不用异常表示普通的来源不可用
或目录读取失败。`ThemeDefinitionResolveContext` 至少包含已冻结的 Application Id、应用配置根目录、是否为运行时
刷新以及当次 resolver generation。Resolver 返回后，ThemeManager 防御性复制结果，不保留 Resolver 的可变集合。

启动必须在第一个可见帧前同步得到初始主题，因此 Resolver 合约是同步的。运行时手动刷新由 ThemeManager 在后台
线程调用同一同步合约，再进入统一 Reader/Binder/Compiler；不为了运行时刷新把应用启动改成先挂载空主题的异步
流程。

启动期只有实现 `IBackgroundThemeDefinitionResolver` 的自定义来源才参与提前后台 Prepare；普通来源在需要 Splash
时先于 UI 线程同步 Prepare，否则随初始提交解析。该可选接口声明 `Resolve` 和返回来源的 `OpenRead` 可在后台调用，
且可与超时后的 UI 回退并发。
运行时刷新仍在后台调用 `SupportsReload=true` 的来源，因此可刷新来源也必须具备相应线程安全性；只能在 UI 线程
访问的来源应返回 `SupportsReload=false`。

标准 Resolver 分为：

- `AvaloniaAssetThemeDefinitionResolver`：由应用或控件包显式列出 `avares://` 主题，必须成功，不支持刷新。
- `UserDirectoryThemeDefinitionResolver`：读取应用用户配置目录中的 `*.theme.xml`，允许启动降级，支持手动刷新。

Builder 公开注册入口为：

```csharp
void AddThemeDefinitionResolver(IThemeDefinitionResolver resolver);
void AddCompiledThemePackage(CompiledThemePackage package);
void WithApplicationId(string applicationId);
void UseUserThemeDirectory();
void UseUserThemeDirectory(string directory);
```

Resolver 与编译包使用显式来源联合；注册 Id 在一次 Builder 中必须非空且唯一。Core 生成的默认主题包最先注册，
应用 Resolver/编译包按显式注册顺序追加。编译包不伪造 OpenRead 字节流；两者共用 Catalog 的冲突、唯一默认与刷新规则。
不执行程序集扫描、资源目录枚举或基于命名约定的类型发现。

### 6.4 Application Id 与用户主题目录

`UseAtomUI(Application, ...)` 在创建 Builder 时，默认从具体 Application 类型所在程序集的简单名称解析
Application Id：

```csharp
application.GetType().Assembly.GetName().Name
```

这只是无配置默认值，不扫描程序集。`WithApplicationId(...)` 可以覆盖它；显式值优先。`Application.Name` 是平台
显示名称，可能为空、包含空格或随品牌文案变化，不参与默认 App Id 推断。

Application Id 必须是单个安全路径段，只允许 ASCII 字母、数字、`.`、`_` 和 `-`，不得为空、`.`、`..`，也不得
包含目录分隔符。解析结果在 Builder 完成时冻结。启用用户主题但无法得到合法 App Id 时构建失败并给出明确
diagnostic。

无显式目录时，`UseUserThemeDirectory()` 使用：

```text
Environment.SpecialFolder.ApplicationData/{ApplicationId}/Themes
```

对应 Windows `%APPDATA%`、macOS `~/Library/Application Support`，以及 Linux 的 XDG 配置根目录。显式目录重载
只改变根路径，不改变文件、安全、冲突或刷新语义。Browser/WASM 不隐式启用用户目录；没有普通文件系统能力的
宿主只有在提供自己的 Resolver 时才能加入外部主题。

用户目录只枚举顶层 `*.theme.xml`，按规范化完整路径 ordinal 排序，不递归、不跟随 symlink/reparse point。
默认最多 128 个文件、总原始字节 32 MiB；单文件仍受 Reader 的 4 MiB 和元素/Token 上限约束。目录不存在时
Resolver 可以创建空目录；创建或枚举失败产生 diagnostic，不能退回当前工作目录或应用安装目录。
Resolver 在枚举前冻结规范化后的目录真实路径；打开文件时使用平台 no-follow 语义，并从实际读取流再次执行总
字节预算。路径检查结果不能代替文件句柄约束，避免检查后替换为链接时越过主题目录边界。

### 6.5 来源优先级与冲突

来源顺序只决定 `AvailableThemes` 的稳定展示顺序，不提供覆盖语义：

```text
Core compiled package -> application compiled packages / XML resolvers -> user directory
```

- 任意来源之间重复 `Theme.Id` 都是错误，不使用 first-wins、last-wins 或静默覆盖。
- 可刷新 Resolver 不得提供 `IsDefault=true` 的 definition；默认主题只能来自不可刷新、随应用发布的来源。
- 合并后的静态 Catalog 必须恰好有一个默认主题。
- 用户主题只能引用当前冻结 registry 已注册的 Token、Control 和 Algorithm；XML 不能加载代码、程序集、脚本、
  include/import、网络资源或环境变量表达式。
- Core 和应用资源错误属于损坏的发布产物，阻止应用启动；用户目录首次加载错误不阻止应用启动，初始 Catalog
  降级为全部不可刷新来源，并通过 `IThemeManager.ThemeCatalogDiagnostics` 暴露本次错误。

用户目录是一个刷新单元。任一用户主题 XML、目录容量、重复 Id、默认主题声明或 Binder 校验失败时，本次用户
目录解析失败，不提交有效子集，也不维护逐文件 Last Known Good。这样目录内容、候选 Catalog 和刷新结果只有一个
明确版本。

### 6.6 Catalog 构建

Catalog 对外部 Resolver 结果执行以下管线：

```text
Resolver source
    -> bounded byte copy + SHA-256
    -> ThemeDocumentReader + v1 XSD
    -> ThemeDefinitionBinder + frozen ThemeSchemaRegistry
    -> PreparedThemeDefinition + ThemeDefinitionRevision
    -> immutable CompiledThemeCatalog
```

`ThemeDefinitionRevision.SourceIdentity` 来自 source identity；`SourceRevision` 来自 Resolver 的结构化 revision；
`ContentDigest` 必须根据本次实际读取的字节计算。文件长度或修改时间只能筛选缓存候选，不能代替内容摘要。
Catalog 构建期间只使用本次复制的字节，避免文件枚举后修改造成 Reader 与 digest 针对不同内容。

包内 `AtomUIThemeDefinition` 使用构建期类型化入口，直接在实际 registry 校验契约并产生同一 PreparedThemeDefinition，
不创建 ThemeDocument 或重复解析静态值。Core 默认主题与 Gallery 产品主题已按此路径接线，原始 avares XML 仍可显式
读取。程序摘要和实际 registry 身份共同限定复用边界；生成 ABI 与实施验收状态见
[编译主题包](compiled-theme-packages/overview.md)及[生成 ABI](../../../reference/theming/compiled-theme-abi-v1.md)。

外部 bounded byte copy 使用小型池化 scratch buffer，完整读取到 EOF，不依赖流的 Length/CanSeek/Position；
实际字节数决定限额和摘要。用户目录完整枚举并规范化路径，只保留上限内的路径；超限保留精确计数和后续枚举
错误优先级，合法目录仍按 ordinal 顺序处理。

## 7. 编译器

`ThemeCompiler` 是无 UI、无资源宿主、无全局服务访问的纯计算入口：

```text
ThemeCompileInput
  -> merge effective config
  -> initialize effective Seed Token
  -> run ordered Evaluate(seed, previousMap) algorithms
  -> apply Map overrides
  -> format Alias Token
  -> apply Alias overrides and AtomUI final normalization
  -> resolve each registered Control slot state (cheap config comparison)
  -> freeze global token values and resources
  -> ThemeSnapshot (control slots compile on first query)
```

编译阶段允许使用可变 builder，因为算法天然需要逐步派生。目标是将其生命周期限制在一次编译，并且不通过
公开 API 返回。惰性 cell 持有独立编译 source；全局结果复用时，仅首次实际编译控件才恢复可变 builder，
不捕获历史 snapshot。详见[惰性物化设计](lazy-materialization.md)。

全局计算顺序固定为：

1. 从 AtomUI library Seed defaults、ThemeDefinition 和有效配置构造唯一的 effective Seed。
2. 将同一 effective Seed 依次传给算法链；第一个算法的 previous Map 为空，后续算法接收前序 Map。
3. 对最终 Map 应用显式 Map Token override。
4. 由 Map 生成 Alias Token，再应用显式 Alias Token override。
5. 执行 AtomUI 最终语义规范化。`MotionEnabled=false` 时，Motion 服务被禁用且全部标准 duration Token 强制为
   `TimeSpan.Zero`；`WaveEnabled=false` 时 Wave 行为不创建动画。该后置条件不依赖单个控件是否正确消费时长。

每个 Control 的计算顺序为：

1. 以全局最终 Token 为基线，接受并叠加该 Control 配置中的任意已注册 Global Token 覆盖。
2. `Disabled` 时只保留直接覆盖，不重新派生；`Global` 或 `Custom` 时，从 Control effective Seed 按相应算法链
   重新生成 Map 和 Alias。
3. 在重新派生后应用该 Control 的 Map、Alias override 和 Motion 最终规范化。
4. 使用 Control 的有效全局 Token 计算 Control 自身默认 Token。
5. 应用 Control 自身 Token override。
6. 根据全局 appearance 和 Control 算法 effect 计算 Control appearance；它只提供给该 Control evaluator，不
   修改 ThemeContext appearance。
7. 生成 Effective Global Token 差量、Own Token 值和统一的 Effective Control Token 资源投影。

Control 默认 Token evaluator 必须接收 Control appearance 和 Effective Global Token，不能继续只读取全局
`IsDark`。

控件槽位编译契约：`ApplyTheme` 返回时全局 token 与资源完整提交；控件级 `ControlThemeSnapshot`
按槽位惰性编译，首次查询时经同一 `CompileControl` 函数与同一缓存键编译，结果值与急切编译逐位
相等；编译结果由现有 Token 值、算法、资源投影与缓存契约测试验证，不保留未被执行的急切参考实现。
首帧内首次查询的控件一次性支付内联编译成本，具体耗时依赖控件与运行环境。
跨快照复用独立 cell，物化结果不保留历史 snapshot，见[惰性槽生命周期设计](lazy-materialization.md#2-独立惰性槽与历史快照释放)。
失败语义同步后移：控件级编译错误（不支持的算法模式、evaluator 抛出等）不再使
`ApplyTheme` 整体失败，而是在首次查询该槽位时抛出、后续每次查询重复抛出。

任何 validation、conversion 或 algorithm 错误都返回失败结果，不返回部分 snapshot——该原子性指
全局维度（Seed/算法链/全局 token/资源）；控件维度的失败面按槽位延后到首查（见上方契约段）。

## 8. ThemeSnapshot

`ThemeSnapshot` 是 Token 编译结果和资源发布单元：

```text
ThemeSnapshot
+-- ThemeId / DefinitionRevision / ContentFingerprint
+-- Algorithms[(id, revision)] / Appearance
+-- RegistryRevision
+-- EffectiveConfig
+-- GlobalTokenValues: dense immutable TokenValueTable
+-- GlobalResources
+-- PresetColorPalettes
\-- Controls: LazyControlSnapshots[registry control slot]  (compile on first query)
    +-- Appearance
    +-- EffectiveGlobalTokenDelta
    +-- ControlTokenValues: dense immutable TokenValueTable
    \-- ControlResources
```

具体约束：

- Token schema 由生成器分配在当前 registry revision 内稳定的 slot；完整 Token 值表使用稠密不可变数组，不使用
  object-key 字典保存每个值。
- Control 表按 registry slot 存放引用，`ControlTokenIdentity` 只用于配置边界和诊断。
- Control 表是按槽位惰性编译的容器（只暴露 Count 与索引器，不实现枚举）；构造期解析槽位状态
  （已物化 / 复用独立 cell / 待编译），保留字节估算为构造期基线加首访物化增量。缓存在插入时采样
  `EstimatedRetainedBytes`，物化后通过弱 cache 通知回灌增量并重新检查预算。单快照槽数有限不构成整个应用的
  保留上界证明；完整计费与释放契约见[内存计费与释放](lazy-materialization.md#7-内存计费与释放)。
- 稀疏覆盖、Control Effective Global Token 差量和 Avalonia 资源投影使用不可变集合或 `FrozenDictionary`。
- 颜色资源在 snapshot 中已经转换为不可变 Brush。
- palette、集合和其他引用值必须在冻结前深复制为不可变值。
- 不保存 Control、Visual、Provider、事件或订阅。独立 source 保留当前全局输入及按需 workspace；复用全局值时
  不急切恢复 builder，非复用路径可持有已有 builder。cell 物化成功后释放 source 引用；最后依赖 cell 释放后，
  source/builder 可回收，不保留父 snapshot 链。source 弱引用 control cache，合法旧视图不会经缓存保活其他历史输入；
  关闭与纯值首查边界见[生命周期契约](compiled-theme-packages/execution-and-lifetime.md#8-实现边界与后续变更要求)。
- 不提供 `SharedToken`、`ControlToken`、`Clone` 或可变字典查询 API。
- imperative 消费者通过只读 Token resolver 查询 snapshot 值。
- `ContentFingerprint` 是规范化内容的快速 hash，不是无碰撞的唯一身份。snapshot 的语义相等性必须比较对应的
  结构化不可变 key，不使用父 snapshot 的递增版本表达语义身份。
- `Appearance` 是 `Light` 或 `Dark` 的已提交结果，是根 Application 和局部 ThemeVariantScope 发布
  Avalonia variant 的唯一依据；不能通过算法名称字符串猜测。

## 9. 缓存

`ThemeSnapshotCache` 使用结构化 value key：

- 完整的不可变 `ThemeDefinition` 结构化 key，包括 definition revision 和 typed 内容。
- 完整的不可变 `NormalizedThemeConfig` 结构化 key。
- 有序算法 descriptor `(id, revision)`。
- `ThemeSchemaRegistry` revision 与实际绑定身份；相同元数据的不同委托实现不能共享结果。

父主题的影响已经包含在合并后的有效配置中，因此 cache key 不包含父 snapshot 的发布序号。

缓存要求：

- fingerprint 只用于定位候选 bucket 或加速拒绝；命中后必须比较结构化不可变 key。有限位 hash 相同但结构不同
  的 definition、config、算法链或 schema 不能共享结果。
- 等价内容返回同一个 snapshot 引用。
- 并发等价请求只执行一次编译。
- 失败结果不进入缓存。
- 缓存由 ThemeManager 实例拥有，使用强引用 LRU，并同时受 entry 数量和保守
  `EstimatedRetainedBytes` 上限约束；达到任一上限即回收最久未访问 entry，不区分 active/inactive。
- snapshot 的 retained bytes 根据稠密 Token 表、Control 引用表、差量、资源条目、palette 和不可变 Brush 的
  确定性保守公式估算，不使用进程 RSS，也不声称等于 CLR 精确对象大小。
- 活动 ThemeContext 自己强持有当前 snapshot。cache eviction 只删除复用入口，不影响已发布 snapshot，因此不
  需要 pin、PinCount 或反向 snapshot-key 表。
- Provider 实例不进入缓存，不在不同资源宿主之间共享。
- 不建立无上限的 Brush、字符串、resource key 或 fingerprint intern pool。

single-flight 编译记录与已完成 LRU entry 分离。记录在成功后才进入 LRU；失败、取消或 superseded 结果立即
释放。默认完整 snapshot cache 上限为 32 entry 和 32 MiB estimated retained bytes；Control compilation
cache 上限为 2048 entry 和 32 MiB。应用可以调小；扩大默认值必须提供 benchmark、峰值对象数和 Gallery
导航内存证据。

## 10. ThemeManager 与全局主题

`ThemeManager` 是唯一主题运行时协调者和唯一允许发布 snapshot 的对象。具体类型是 internal Avalonia
`Styles` 宿主并实现公开的 `IThemeManager`；同一个实例同时挂载到 Application Styles 和主题服务容器，不再
创建同生命周期的一对一 `ThemeEngine` 或第二份 Theme Token 状态。

`ThemeManager` 只拥有长期运行时状态和编排职责：当前 `ThemeState`、根 `ThemeContext`、`ThemeScopeGraph`、
请求 generation/队列、Application ThemeVariant 发布以及结果事件。每个请求的阶段状态属于独立
`ThemeTransaction`；规范化、合并、编译、缓存和事件调用分别委托给对应对象。合并运行时所有者不等于把这些
职责的实现代码并入 `ThemeManager`。

作为 `Styles` 宿主，Manager 挂载根 Token Provider、Control themes 和语言资源入口；语言切换及资源构建委托给
Language 子系统，不进入 ThemeTransaction，也不能修改 `ThemeState`。Manager 不实现 Token 算法、XML 读取、
schema binding 或资源查询细节。

不存在供多个 Manager 共享的 headless engine，也不存在一个 Manager 切换多个 Styles 宿主的场景。纯计算复用
由 Compiler 和 Cache 提供，局部主题复用由 ThemeContext 和 ThemeScopeGraph 提供，因此额外 Engine 不形成
有效边界，只会引入 facade 转发、事件桥接和双向生命周期协调。

新公开模型由以下对象组成：

- `ThemeInfo`：Catalog 中可选择主题的只读元数据；`AccentColor` 只投影 Theme Definition 中显式绑定成功的
  `ColorPrimary`，未声明时为 `null`，不从运行时 Snapshot 或算法派生结果反向推断。
- `ThemeRequest`：主题 id、`ThemeConfig` 和变更原因。
- `ThemeState`：当前已提交主题的 id、算法、`Appearance`、仅用于观察和快速比较的 fingerprint，以及
  transition id。
- `ThemeTransitionResult`：一次请求的 transition id、`Committed / NoOp / Superseded / Failed` 状态、已提交
  `ThemeState`、失败 diagnostics/exception 以及独立的 `PublishDiagnostics`。
- `IThemeManager.ApplyThemeAsync(...)`：返回 `Task<ThemeTransitionResult>` 的运行时主题请求入口。
- `ThemeCatalogReloadResult`：一次 Catalog 刷新的 generation、`Committed / NoOp / Superseded / Failed` 状态、
  新主题列表、diagnostics、publish diagnostics 和 exception。
- `IThemeManager.ReloadThemesAsync(...)`：只重新执行 `SupportsReload=true` 的 Resolver，并原子更新 Catalog 与
  当前主题作用域图。
- `IThemeManager.ThemeCatalogChanged`：成功提交新 Catalog 后的结果事件。
- `IThemeManager.ThemeCatalogDiagnostics`：最近一次启动加载或手动刷新的只读 diagnostics。
- `IThemeManager.CurrentTheme`：只读 `ThemeState`。
- `IThemeManagerBuilder.WithInitialTheme(...)`：首帧主题 id 和 ThemeConfig 的唯一配置入口。

删除 `Theme` 和 `ITheme`。Catalog 只保存 definition，不为 Default、Dark、Compact 预先创建 Theme 对象。
每个请求都由 definition、配置和算法构成规范化输入，因此同一主题的新 runtime override 必须重新查询缓存
或编译。

AtomUI 主题身份与 Avalonia `ThemeVariant` 解耦：

- AtomUI 使用 `ThemeRequest` 表达主题 id 和算法。
- `Application.RequestedThemeVariant` 只根据已提交 snapshot 设置为 Avalonia Light 或 Dark。
- Compact 和自定义主题 id 不编码进 `ThemeVariant` 名称。
- `Application.RequestedThemeVariant` 是 ThemeManager 的发布输出；应用不得把它作为第二个主题写入口。
- 跟随系统主题是一种明确的 Manager 策略，不通过双向绑定或让 Application 长期保持
  `ThemeVariant.Default` 实现。

FollowSystem 在 Builder 阶段获得 Light 和 Dark 两个完整、不可变的 root request 模板。Manager 监听平台外观
变化，先根据目标外观 Capture/Prepare 对应 snapshot，再在 Publish 阶段显式设置
`Application.RequestedThemeVariant=Light/Dark`。平台先改变 appearance、AtomUI 后编译 Token 的中间状态不得
暴露。初始系统 appearance 必须在首个 snapshot 编译前解析。

Root ThemeContext 通过 ThemeManager 中的全局 TopLevel style 把稳定的 `ThemeScope.ContextProperty` 注入每个
TopLevel；不能假设 inheritable AvaloniaProperty 会从 `Application` 自动传到独立 TopLevel。由 owner lease
创建的局部 TopLevel 使用 local value 覆盖该根 style 值。

Motion 和 Wave 配置也是 ThemeConfig Token override。任何入口改变这些值时都生成新请求，不直接修改
`ResourceDictionary`。

ThemeManager 还保存最后一次已提交的完整 root `ThemeRequest`。Catalog 刷新导致当前 definition 内容变化或当前
Theme Id 被删除时，Manager 使用该 request 的不可变 `ThemeConfig` 重新编译或回退，确保 Dark、Compact、Motion
和其他 runtime override 不因刷新而丢失。

## 11. 动态作用域

### 11.1 稳定 ThemeContext

`ThemeContext` 是稳定的作用域身份，包含：

- 当前 `ThemeSnapshot` 的原子引用。
- 唯一的 `ThemeTokenResourceProvider`。
- scope identity 和只读 appearance。
- Publish 阶段使用的内部 context changed 通知。

`ThemeContext` 不复制或修改 Token。snapshot 仍是 Token 值唯一真源。

`ThemeScope.ContextProperty` 是 inheritable attached property，在树上传递稳定 context，而不是每次主题变化都
替换 attached snapshot。根值由全局 TopLevel style 注入；局部值由 ThemeConfigProvider 设置。没有局部
Provider 时，控件解析到根 `ThemeContext`。

`ThemeConfigProvider` 继承 Avalonia `ThemeVariantScope`，从而同时拥有局部 AtomUI ThemeContext 和局部
Avalonia variant 边界。它的 Resources 在 attach 期间只挂载一个稳定 Token Provider；更新时不替换 Provider，
只在 CommitCore 替换 ThemeContext snapshot，并在 Publish 设置 `RequestedThemeVariant` 为 snapshot 的 Light
或 Dark appearance。

### 11.2 ThemeScopeGraph

`ThemeManager` 拥有一个活动 `ThemeScopeGraph`：

```text
+---------------------------+
| ThemeManager              |
| owns topology and commits |
+-------------+-------------+
              |
              v
+---------------------------+
| Root ThemeContext         |
| snapshot + provider       |
+-------------+-------------+
              |
       +------+------+
       |             |
       v             v
+-------------+ +-------------+
| Scope A     | | Scope B     |
| context A   | | context B   |
+------+------+ +-------------+
       |
   +---+---+
   |       |
   v       v
+----------+ +----------+
| Scope A1 | | Scope A2 |
+----------+ +----------+
```

每个 `ThemeScopeNode` 保存：

- 单调递增且本次注册唯一的 `RegistrationId`，防止 detach/reattach ABA。
- `ParentRegistrationId` 和子节点集合。
- 每次 Config 替换递增的 `ConfigRevision`。
- 当前不可变规范化配置和 `LastValidConfig`。
- 稳定 ThemeContext、Provider 和 ThemeConfigProvider。

`ThemeScopeGraph.TopologyRevision` 在 attach、detach 或 reparent 时递增。Capture 必须记录 root generation、
TopologyRevision，以及每个受影响节点的 `(RegistrationId, ParentRegistrationId, ConfigRevision)`。Prepare 结果
提交前必须逐项验证；任一值变化都使旧结果成为 `Superseded`，不能提交到新拓扑。

`ThemeConfigProvider` 通过 ThemeManager 的 internal scope registration contract 注册节点；attach 时注册，
detach 时注销，父级变化或内容重新挂接时更新图关系。Provider 负责提交当前可观察配置变化，但不通过静态
`ThemeManager.Current` 查找具体实现，不自行监听父 snapshot，不直接调用 compiler，也不自行实现事件队列。

全局主题变化时，ThemeManager 使用新根 snapshot 和各节点最后一次有效配置，按拓扑顺序准备整个作用域图。
局部配置变化时，只准备该节点及其后代。

所有根和局部事务共用一个 Manager-wide scheduler 和 Commit gate。同一 Manager 同时最多有一个 Prepare
管线和一个 CommitCore；新请求按作用域合并，根请求覆盖尚未提交的全部后代准备请求，从而限制编译峰值并
消除根、局部交错提交。

配置输入无效时：

- 当前作用域保留已发布 snapshot。
- `LastValidConfig` 不更新。
- 派发失败事件和 diagnostics。
- 后续父主题变化使用 `LastValidConfig`，避免该作用域永久冻结在旧全局主题。
- 首次 attach 尚无已发布局部 snapshot 时，使用父 snapshot 作为安全回退，并把空 `Inherit=true` 配置作为
  fallback；无效局部输入不能造成空资源或首帧崩溃。

普通 Popup、Flyout 只要保留 Avalonia 逻辑父级，就自然继承 `ThemeScope.ContextProperty`、资源查询链和
ActualThemeVariant，不创建额外 bridge。

独立 Window、Dialog、Notification host 和其他新 TopLevel 必须通过 owner 创建
`ThemeContextLease`。Lease 在宿主生命周期内：

- 在 TopLevel 设置 owner 的 ThemeContext local value 和 snapshot appearance。
- 挂载一个宿主私有 `ThemeContextResourceBridge`，只委托给 owner context，不复制 Token。
- 订阅 owner context Publish 通知并向该 TopLevel 传播最小资源通知。
- owner 改变、窗口关闭或 API dispose 时对称移除 bridge、属性值和订阅。

AtomUI Desktop `Window` 在调用 Avalonia 的 `Show`、`Show(owner)` 或 `ShowDialog(owner)` 之前完成 lease 获取，
因此 `WindowOpenedEvent`、样式应用和首个布局都只能看到完整的 Context、显式 Light/Dark variant 和私有 bridge。
带 owner 的重载从 owner 解析 Context；无 owner 的根 Window 使用 Root ThemeContext。若 Avalonia 打开流程同步失败，
本次新建 lease 必须在异常离开前释放；`OnClosed` 再执行幂等释放，不能依赖外部调用者补清理。

`ThemeTokenResourceProvider` 仍是每个 scope 唯一的规范 Provider；ResourceBridge 是独立资源根的非拥有代理，
不进入 cache，也不形成第二份 Token 状态。无 owner 的静态 API 只能显式租用 Root ThemeContext，并应提供
diagnostic，不能根据调用栈、当前焦点或全局静态变量猜测局部上下文。

## 12. 事务与发布顺序

一次主题事务分为 Capture、Prepare、CommitCore、Publish、Complete 五个阶段：

```text
+-------------+  +-------------+  +-------------+  +-------------+  +-------------+
| Capture     |->| Prepare     |->| CommitCore  |->| Publish     |->| Complete    |
| UI thread   |  | pure work   |  | UI thread   |  | UI thread   |  | result task |
+-------------+  +-------------+  +-------------+  +-------------+  +-------------+
| immutable   |  | merge/cache |  | silent swap |  | variants    |  | committed / |
| graph stamp |  | compile all |  | all contexts|  | resources   |  | no-op / ... |
| affected set|  | staged data |  | state refs  |  | events      |  |             |
+-------------+  +-------------+  +-------------+  +-------------+  +-------------+
```

Capture 完成不可变 Config 绑定、scope stamp 和 affected set；Prepare 只执行纯合并、single-flight cache 和
完整子树编译。进入 CommitCore 前，Manager 在 UI 线程再次验证 generation、TopologyRevision 和全部 node
stamp。

CommitCore 只执行不会调用外部代码的引用和值交换：一次性替换所有受影响 ThemeContext snapshot、
ThemeState 和内部 appearance。它不设置 AvaloniaProperty、不调用 `RaiseResourcesChanged`、不派发 context 或
结果事件。实现必须预先分配 staged 数组并完成所有可失败工作，使 CommitCore 成为无失败、无分配、无通知的
有限赋值序列。

`Application.RequestedThemeVariant`、局部 `ThemeVariantScope.RequestedThemeVariant`、context changed、
ResourceProvider 通知和结果事件都属于不可回滚 Publish。Publish 开始前所有 context 已指向完整新 snapshot，
因此 Avalonia 同步回调也只能读取完整新状态。不能再承诺在 `RequestedThemeVariant` 或资源通知之后静默回滚。

Publish 的每个外部通知边界必须独立捕获、记录并继续后续通知。观察者异常不把已提交事务改写成 Failed；结果
仍为 `Committed`，并携带 publish warning diagnostics。只有 Capture、Prepare 或 CommitCore 前的 stamp/invariant
验证失败可以返回 `Failed` 或 `Superseded`，且此时尚未交换任何已发布引用。

同一作用域的新请求使用 generation 标识。旧的异步准备结果在提交前发现 generation 已过期时直接丢弃。
提交期间发生的重入请求进入队列，同一作用域尚未执行的请求合并为最新请求。

`ApplyThemeAsync` 的请求完成语义固定如下：

- `Committed`：新 snapshot 已完整提交，全部 Publish 边界均已按顺序尝试；资源或观察者异常作为
  `PublishDiagnostics` 返回，不改变该状态。
- `NoOp`：规范化后的目标状态与当前状态结构相等；不创建 snapshot，不发送资源通知或结果事件。
- `Superseded`：请求的 generation 在 Commit 前被更新请求替代；不发布该请求的状态，也不能伪装成成功。
- `Failed`：Capture、Prepare 或 CommitCore 前验证失败；旧状态从未被修改，`ThemeChangeFailed` 已派发。

主题转换自身的失败通过 `Failed` 结果表达，不以异常代替结果。调用参数违反 API 前置条件仍可以同步抛出；调用
方取消等待时，仅该调用返回取消完成，不把底层 transaction 改写为 `Failed`。

等价并发请求加入同一个 in-flight transaction，共享相同 transition id 和 Prepare/Commit 结果。调用方传入的
`CancellationToken` 只取消该调用方的等待，不得取消已经开始的 Commit，也不得把其他等待者的请求取消。
Prepare 阶段只有在所有等待者都已取消且事务尚未进入 Commit 时，Manager 才可以取消底层工作；新 generation
替代旧 generation 时，仍按 `Superseded` 完成未取消的旧等待者。

结果顺序也属于公开契约：`Committed` 先按顺序尝试 variant、context、资源通知和 `ThemeChanged` 的全部 Publish
边界，再携带可能的 warnings 完成对应 Task；`Failed` 先派发 `ThemeChangeFailed`，然后对应 Task 才完成；
`NoOp` 和 `Superseded` 不派发主题结果事件，直接完成结果。

### 12.1 Theme Catalog 手动刷新事务

`ReloadThemesAsync()` 与主题切换共享同一个 Manager mutation scheduler、generation 和 Commit gate，不能在
ThemeTransaction 之外直接替换 `_compiledThemeCatalog`。一次刷新同样遵循五阶段：

1. **Capture（UI thread）**：捕获 Catalog generation、最后一次已提交 root request、当前 ThemeState、
   ScopeGraph stamp 和全部可刷新 Resolver 的不可变快照。
2. **Resolve/Prepare（background）**：重新执行可刷新 Resolver，读取和绑定全部候选来源，合并不可刷新定义，
   构建候选 Catalog。当前 Theme Id 仍存在时使用候选 definition 重编译；不存在时选择静态默认主题，并保留当前
   request 的 `ThemeConfig`。随后按拓扑准备全部局部 snapshot。
3. **CommitCore（UI thread）**：复核 manager generation、Catalog generation、TopologyRevision 和全部 node
   stamp；一次性交换 Catalog、根/局部 snapshot、ThemeState、最后 request 和 cache key。该阶段不通知、不分配、
   不调用 Resolver 或观察者。
4. **Publish（UI thread）**：snapshot 发生变化时依次发布 Application/局部 variant、资源和 `ThemeChanged`；随后
   发布 `ThemeCatalogChanged`。所有事件开始前 `AvailableThemes`、`CurrentTheme` 和全部 Context 已经指向同一代
   状态。
5. **Complete**：返回 `ThemeCatalogReloadResult`。调用方取消只取消自己的等待，不撤销已进入 Commit 的刷新。

刷新结果语义：

- `Committed`：候选 Catalog 或当前有效 snapshot 发生结构变化并完整提交。
- `NoOp`：Resolver 结果、definition revision 和当前 snapshot 均与已提交状态相同，不发布事件。
- `Superseded`：新的主题切换、局部配置更新或 Catalog 刷新改变 generation/stamp，本次不提交。
- `Failed`：Resolver、读取、绑定、冲突、编译或 Commit 前验证失败；旧 Catalog、ThemeState 和全部 snapshot 未
  修改。

用户目录整批原子：任一文件失败时刷新返回 `Failed`，不提交有效子集。第一次启动没有旧用户 Catalog 可保留时，
用户 Resolver 失败只排除整个用户来源层，应用使用完整静态 Catalog 启动，并把 diagnostics 保存到
`ThemeCatalogDiagnostics`。Core/应用资源 Resolver 失败仍阻止启动。

如果用户删除当前主题且其余候选定义全部有效，刷新提交静态默认 Theme Id，并复用最后 request 的 runtime
`ThemeConfig`。这属于 `Committed`，同时发布 `ThemeChanged` 和 `ThemeCatalogChanged`。不保留“CurrentTheme 不在
AvailableThemes 中”的悬空状态。

事件处理期间发起新请求遵守现有重入队列。`ThemeCatalogChanged` 观察者异常只形成 publish warning，不回滚已经
提交的 Catalog；刷新结果仍为 `Committed`。

## 13. 资源解析

根 ThemeContext 的 Provider 在 ThemeManager 初始化时挂载一次。每个局部 ThemeContext 也只拥有一个
Provider。主题切换不追加 `MergedDictionaries`，不创建新的 Provider。

Root Provider 位于 ThemeManager.Resources，由根 Styles.Resources 宿主挂入应用；局部 Provider 由对应
ThemeConfigProvider.Resources 拥有；独立 TopLevel 通过自己的 ResourceBridge 委托给 owner context。Provider 和 bridge 都忽略调用方传入的
自定义 AtomUI theme id，只使用已提交 snapshot；Avalonia 的 `ThemeVariant` 参数仍用于同一资源树中的普通
ThemeDictionary 查询。

查询规则：

```text
SharedTokenResource(X)
    -> GlobalResources[X]

XxxTokenResource(X), X is any registered Global Token
    -> ControlEffectiveGlobalDelta[X]
    -> miss: GlobalResources[X]

XxxTokenResource(X), X is Own Token
    -> ControlOwnResources[X]
```

普通应用内容和明确要求所有 Control 永远共享同一个值的样式使用 `SharedTokenResource`。ControlTheme 中希望
响应 Control 级覆盖的全局 Token 必须使用对应的 `XxxTokenResource`。

### 13.1 AXAML 访问契约

Token 资源的公开 AXAML 语法固定为：

```xml
<!-- 永远读取全局 Token。 -->
<Setter Property="Background" Value="{atom:SharedTokenResource ColorErrorBg}" />

<!-- 读取 Alert 的 Effective Global Token。 -->
<Setter Property="Foreground" Value="{atom:AlertTokenResource ColorText}" />

<!-- 读取 Alert Own Token。 -->
<Setter Property="Padding" Value="{atom:AlertTokenResource DefaultPadding}" />
```

`XxxTokenResource` 的位置参数是生成的强类型 `XxxTokenKey`，不是字符串。主题作者可选择 Xxx Own Token 和当前
Global Token schema 中的全部候选。Global Token 与 Own Token 禁止同名，因此
`{atom:AlertTokenResource ColorText}` 不需要额外写 `Global=` 或 `Own=`。

`SharedTokenResource` 永远映射全局 key，不读取 ambient identity。`XxxTokenResource` 在构造时已经绑定生成的
`XxxTokens.Identity`，并按 Token key 的 schema 分类为 Effective Global Token 或 Own Token。ProvideValue 和
资源查询热路径都不解析字符串、不遍历 Visual、不查找 templated parent，也不推断 TargetType。

ControlTheme 不声明 `ControlTokenScope.Identity`。强类型 Token key 在 AXAML 编译期验证 Global/Own Token 名称；
资产生成器只分析 owner、引用的 Control identity、URI、资源结构和 Semantic Part Theme 资产关系，不收集 Global
Token 消费关系。Semantic Part Generator 独立分析 Control 声明和 `.semantic-*` marker；
`EffectiveGlobalToken.X`、`XxxTokenResource X` 和 C# Binding 都不会扩展 schema。

- 引用 Own Token 时必须属于显式 Xxx Control。
- 引用任意已注册 Global Token 都合法，不产生 dependency manifest。
- Semantic Part Theme 可以显式引用 owner Control 和真实 Part Control 的 TokenResource。
- Selector-only Semantic Part 不创建 Theme asset、Token identity 或运行时样式字典。
- 不存在的 Token、重复资产 URI、未注册 identity 或不兼容的 Part `TargetType` 在构建期失败。

运行时只加载生成结果并再次对照当前 RegistryRevision，不重新扫描程序集或 AXAML 文本。ThemeManager 必须在
解析 ThemeConfig 前完成所有 descriptor 和资产 manifest 注册并冻结 schema。

### 13.2 C# 访问契约

C# 必须显式区分普通 Global Token 与当前 Control 的 Effective Global Token：

```csharp
TokenResourceBinder.CreateGlobalTokenBinding(
    this, ForegroundProperty, SharedTokenKind.ColorPrimary);

TokenResourceBinder.CreateControlTokenBinding(
    this, ForegroundProperty, SharedTokenKind.ColorPrimary);
```

内部对象使用 `owner` 重载，由公开、已注册的 Control 提供 identity 和 ThemeContext。命令式读取通过
`ControlTokenAccessor.Capture(owner)` 获取同一 snapshot 中的 `GetEffectiveGlobal<T>()` 与经过 owner 校验的
`GetOwn<T>()`。Binding 跟随 ThemeContext 发布自动更新；命令式路径在通知后重新 capture。实现不得创建只用于
转运 Token 的隐藏 StyledProperty，也不得在运行时观察资源访问来推导 schema。
非 Visual `AvaloniaObject` 的程序化绑定优先使用它自己的 scoped `IResourceHost`，其次使用其
`IResourceProvider` owner；没有局部资源宿主时才使用应用全局资源。所有路径都兑现传入的 converter 与
`BindingPriority`，调用方持有并释放返回的 `IDisposable`。全局资源 observable 省略 `ThemeVariant` 时跟随应用
实际变体变化；显式传入变体才固定到该变体。

基类代码读取 Effective Global Token 时按实例 exact CLR type 解析，因此注册的派生 Control 使用自己的 identity。
Own Token 始终显式属于某一终端 Control，不随 Control CLR 继承或 Token 定义继承动态替换。在目标继承模型中，Token
定义继承由 Generator 在生成期扁平化；未注册 exact type 使用 Control Effective API 时必须明确失败，不能退回基类 identity。internal part
不创建公开 identity，由公开 owner 负责绑定。

### 13.3 强类型 Token key 编码

`XxxTokenKey` 继续同时提供全部 Global Token 和 Xxx Own Token 的 AXAML 提示，但生成代码不得为每个 Control
重复一套完整 Global Token switch。Global key 使用与 `SharedTokenKind` 对应的非负 slot，Own key 使用
`-1 - OwnTokenSlot`；`XxxTokenResourceExtension` 通过一次整数分支映射到 Effective Global resource key 或 Own
resource key。数值 slot 只在当前生成 schema 内有效，不能持久化或作为跨版本协议。

### 13.4 ControlTheme Asset、Semantic Part 与 Part Theme

一个标准 Control 使用约定式文件结构：

```text
Rating/
+-- Rating.cs
+-- RatingToken.cs            optional, only when Own Tokens exist
\-- Themes/
    \-- RatingTheme.axaml
```

`Themes/**/*.axaml` 由构建集成自动作为输入；生成器按 Control、可选 Token 类型和主题资产约定建立 identity、
descriptor 与 manifest。只有一个 ControlTheme 时不增加 Theme Module、聚合 AXAML 或包装 ResourceDictionary；
多个主题资产直接放在同一 `Themes/` 目录并进入同一个生成 manifest。

这里的“不增加包装层”约束针对 Control 开发者维护的源码。构建系统仍负责把每个叶子资产转换为可由包级
Provider 确定性加载的生成资源：

- 根节点是 `ResourceDictionary` 的叶子使用生成的外层 ResourceDictionary，通过固定 `avares://` URI 包含原资产。
- 根节点是默认 typed `ControlTheme` 的叶子先生成一个 deferred ResourceDictionary，在其中按 `TargetType` key
  实例化 typed theme；再生成可由 C# Provider 构造的外层 ResourceDictionary 引用该 deferred dictionary。
- Semantic Part typed theme 不进入包级默认资源集合，由 owner 的强类型 `ControlTheme?` 属性实例化或替换。

typed theme 的两层生成包装是 Avalonia 资源加载边界，不是 Control 作者需要创建或维护的 Theme Module。生成代码、
deferred AXAML 和 manifest 必须由同一资产输入确定性产生；运行时只构造生成资源，不解析 AXAML 目录或反射 theme
类型。

Control 的稳定视觉区域通过 `.semantic-*` Selector 和生成式 `ControlSemanticDescriptor` 公开。Descriptor 记录
Part name、path、ContractType、cardinality、customization 和跨视觉根信息；运行时样式匹配仍完全由 Avalonia
Selector 完成。完整模型见 [Semantic Part 系统设计](semantic-parts.md)。

稳定且允许用户完整替换的 public 子 Control 可以额外通过强类型 `ControlTheme?` 属性公开，例如
`SearchEdit.SearchButtonTheme`。这类 Semantic Part Theme 不创建 Token identity，也不使用字符串 Part 字典。
Part 保留自己的 identity；Part Theme 可以显式消费 owner 和 Part 两个 `XxxTokenResource`，分别表达组合语义与
基础视觉。

Popup、Overlay、普通 presenter 和 item container 不因跨 VisualRoot 或重复出现而自动获得 Theme 属性。它们首先
使用 Selector；只有存在完整 ControlTheme 替换契约时才进入 `SelectorAndTheme`。

SearchEdit 必须直接组合 public Button，不能创建一个继承 Button 却借用 LineEdit Token 的 internal
SearchButton。Button 基础视觉只消费 `ButtonTokenResource`；布局、边框拼接、圆角、状态投射和搜索行为由
SearchEdit 及其 `SearchButtonTheme` 负责。TargetType 与 Token identity 因而不会再出现多重归属。

控件实例不创建 Token `ResourceDictionary`，不注册 Control 自己的资源作用域，也不订阅全局主题事件。
Control 配置只修改当前 `ThemeContext` 中指定 `ControlTokenIdentity` 的 Effective Control Token，不覆盖全局
Token，也不由某个控件实例创建私有子作用域。当前 `ThemeContext` 子树中的所有同 identity Control，
包括位于用户 Content 或嵌套控件中的实例，都使用该 Control 配置；其他 Control identity 和子
`ThemeConfigProvider` 的独立 ThemeContext 按各自有效配置解析。

## 14. Imperative Token 消费

少数控件需要在 C# 中计算 palette、断点或运行时绘制值。它们通过
`ControlTokenAccessor.Capture(owner)` 原子捕获一次读取视图；Core 内部由 `ThemeTokenResolver` 实现：

- Accessor 必须以明确且已注册 exact CLR type 的 Control owner 为入口；不能通过
  `ThemeManager.Current`、当前焦点、调用栈或 Visual 全局扫描猜测作用域。
- Accessor 从 owner 继承的稳定 `ThemeContext` 原子读取一次 snapshot；同一次布局、绘制或计算必须只使用该
  snapshot，不能在多个 Token 查询之间重新读取 context 而混合两个事务的值。
- 普通 Global Token 走明确的 global API；Control Effective Global Token 使用 `SharedTokenKind` 调用
  `GetEffectiveGlobal<T>()`。
- Own Token 使用生成的 `XxxTokenKind` 调用 `GetOwn<T>()`；Registry 必须验证资源 key 属于 accessor 的 exact
  Control slot，不能读取其他 Control 的 Own Token。
- palette 读取 snapshot 中不可变的 `PresetColorPalettes`。
- Accessor/Resolver 不缓存第二份 Token 对象图，也不把 snapshot、Provider 或 owner 放入静态缓存。
- 需要动态更新时订阅当前 `ThemeContext` 的内部 Publish 通知，并在 detach、re-template 或 owner 变化时释放
  subscription；控件不订阅 `ThemeManager.ThemeChanged`。
- 独立 TopLevel 中的 imperative 查询使用 lease 注入的同一个 owner ThemeContext。无 owner 的静态 API 只能
  显式解析 Root ThemeContext，并遵守根主题语义。

优先把固定视觉结果编译为 Control 资源。只有值确实参与控件运行时算法时才使用 resolver。

## 15. 事件契约

新主题系统只保留结果事件：

- `ThemeChanged`：事务已完成 CommitCore，variant、context 和资源通知已经发布后派发；携带 request、scope、
  transition id、已提交 `ThemeState` 以及派发到该事件之前累计的 Publish warning diagnostics。
- `ThemeChangeFailed`：Capture、Prepare 或 CommitCore 前的 stamp/invariant 验证失败，且任何已发布引用都尚未
  交换时派发；携带 request、scope、transition id、diagnostics 和 exception。

不提供可取消的 Changing 事件，也不保留 Created、AboutToLoad、Loaded、AboutToChange、Unload 等伪生命周期。

事件由统一 `ThemeEventDispatcher` 派发：

- 每个订阅者独立调用。
- 一个订阅者异常不阻止后续订阅者。
- Publish 中设置 AvaloniaProperty、context 通知、资源通知或事件订阅者抛出的异常统一转换为 warning
  diagnostic，并通过 Avalonia Logger 记录完整上下文。
- Publish warning 不触发 `ThemeChangeFailed`，不回滚 snapshot、variant 或已发出的通知，也不把成功提交改写为
  主题失败。
- `ThemeChanged` 订阅者自身产生的 warning 无法回填给已经开始执行的同一事件参数；它必须进入最终
  `ThemeTransitionResult.PublishDiagnostics`，供请求调用方观察。
- 事件处理期间发起的请求遵守 ThemeManager 重入队列。

全局事件由 `IThemeManager` 暴露；局部结果由对应 `ThemeConfigProvider` 暴露。两者使用相同 event args 和
派发规则。内部 `ThemeContext` changed 通知只用于 ResourceBridge 和 imperative 消费者，不是第二套公开主题
生命周期事件。

## 16. 启动时序

内核提交（见[启动生命周期](../startup/lifecycle.md#4-提交一次)）时的主题启动顺序固定为：

1. Builder 解析并冻结 Application Id，收集 Theme Definition Resolver、算法 descriptor、Control theme asset
   manifest、不可变初始 root request 模板和语言配置。
2. 构建并冻结 `ThemeSchemaRegistry`；在此时拒绝重复 Control identity、无效 descriptor、资产 identity/URI
   冲突以及 manifest 与 registry 不一致。
3. 同步执行不可刷新 Resolver；如启用用户主题目录，再同步执行用户 Resolver。Catalog 统一读取并绑定全部成功
   来源，确定静态默认主题和 definition revision。静态来源失败终止启动；用户来源失败降级到静态 Catalog 并
   保存 diagnostics。
4. 如果启用 FollowSystem，先同步读取初始系统 appearance，再从 Light/Dark 不可变 request 模板中选择首帧
   request；不得先按 Default 编译后再补一次 Dark/Light 切换。
5. 同步完成初始 request 的 Capture/Prepare，生成第一个完整根 `ThemeSnapshot`；启动前后台 Prepare 产出的全局层
   作为可复用父层传入，不重复编译全局层。
6. 使用该 snapshot 创建 Root ThemeContext 和稳定 Token provider；唯一 ThemeManager 持有完整主题资源字典，
   语言 Provider 与服务关系由独立 LocalizationHost 持有，不归 ThemeManager 管理。
7. 服务绑定及应用级资源/Style 挂载采用[应用资源宿主设计](application-resource-host.md)：主题 Resources 与根
   Context Style 分开挂载，语言 Provider 接入共同资源宿主。实现证据与平台验收边界见
   [资源宿主验证](application-resource-host-verification.md)。
8. 显式设置与 snapshot appearance 一致的 Avalonia Light/Dark variant，并且只发布一次初始 Token 资源通知。
9. 执行与主题生命周期无关的模块初始化。

不得先挂载空资源层再异步补主题，也不得通过 ThemeLoaded 事件修补 DefaultFont、断点或其他 Token。

全部包片段在提交时一次补齐；资源字典与控件快照按需创建。主题侧资源索引、全局计算身份拆分与惰性 cell 由
[惰性物化与资源查询](lazy-materialization.md)定义。

独立 `TopLevel` 的首帧还包括平台窗口在 Avalonia 完成 `ApplyStyling` 前可能展示的客户区表面。宿主必须在调用
平台 `Show` 前挂载目标 `ThemeContext`、设置与 snapshot 一致的显式 variant，并从当前作用域 Snapshot 同步读取窗口
背景 Token，预热 `Background` 与透明度回退背景。该预热只能使用低于用户 local value 的可释放临时值，不创建
资源 observable 或订阅，并在同步样式应用完成后立即释放，由正式 `ControlTheme` 接管；不能依赖 Avalonia
`Window` 的白色默认值，也不能保留第二套长期背景状态。

`ThemeConfigProvider` 首次 attach 也必须在其内容进入首个可见帧前同步解析父 ThemeContext、准备局部 snapshot、
初始化稳定 Provider，并设置与 snapshot 一致的 `RequestedThemeVariant`。首次 Config 无效时直接使用父 snapshot
和父 appearance 作为安全回退；不能先暴露空 context，再依赖后续异步事务纠正。

## 17. 线程与生命周期

- `ThemeConfig`、`ControlThemeConfig` 和 `ThemeRequest` 深度不可变。配置输入只在 UI 线程捕获完整引用；Provider
  只观察 `Config` 属性替换，不订阅 Config 内部集合或 setter 对象。
- ThemeCompiler 不访问 UI 对象，可以在后台准备运行时变更。
- 初始根主题和 ThemeConfigProvider 首次 attach 同步准备，避免首帧闪烁。
- 运行时更新可以后台编译，但 CommitCore、Publish 和 Complete 排序必须回到 UI 线程。
- 手动 Catalog 刷新在后台执行 Resolver、文件读取、Reader、Binder 和编译；Resolver 及 source stream 不进入
  CommitCore，也不能被 snapshot、Catalog 或事件参数长期持有。
- Catalog 刷新与根/局部主题事务共享 scheduler 和 generation，不能并行提交两代 Catalog/snapshot。
- Capture 的 generation、TopologyRevision 和 node stamp 只描述当次不可变图快照；Prepare 不得持有可变
  ScopeNode 集合。提交前 graph stamp 不一致时必须丢弃 staged result，不能尝试修补后提交。
- Scope attach、detach、reparent 和 Content 替换必须在 ThemeScopeGraph 中对称登记和释放。
- snapshot 不持有作用域、Control、Visual 或事件订阅。
- Provider 生命周期归所属 ThemeContext，不进入全局静态缓存。
- Root ThemeContext 生命周期归 ThemeManager；局部 ThemeContext/Provider 生命周期归已注册
  ThemeConfigProvider；Manager dispose 时按拓扑逆序释放局部注册、bridge 和根资源宿主。
- 普通 Popup/Flyout 依靠逻辑父级继承，不创建 lease。独立 TopLevel 的 `ThemeContextLease` 和
  `ThemeContextResourceBridge` 由宿主 owner 关系拥有；owner 改变、关闭、异常打开或 dispose 都必须对称替换或
  释放 local value、资源挂载和 context subscription。
- lease 只强持有其活动宿主和 owner context，不反向让 owner Visual、已关闭 TopLevel 或旧 snapshot 超过宿主
  生命周期存活。
- `UserDirectoryThemeDefinitionResolver` 不创建 `FileSystemWatcher`，不持有打开文件句柄。每次启动或显式刷新
  枚举一次目录，读取完成即释放 stream；ThemeManager dispose 后不保留 Resolver、source、路径集合或旧 Catalog。

## 18. 性能与内存模型

### 18.1 数据布局

源生成器为全局 Token、每个 Control Token 和注册 Control 分配稳定的整数 slot。编译器内部和 snapshot
使用 slot 索引，不在热路径使用 Token name、`PropertyInfo` 或 object-key 字典。

数据布局遵循以下原则：

- 全局 Token 值只在 snapshot 中保存一次。
- Control snapshot 只保存 Control 自身 Token 值和相对全局 Token 的有效差量。
- 不为每个 Control 保存一份完整全局 Token 表。
- 未修改的 palette、不可变 Brush、全局 Token 表和 Control snapshot 在父子 snapshot 之间共享引用。
- Control 表可以复制固定长度的引用数组，但不得复制未变化 Control 的值表和资源表。
- `ThemeSnapshot` 不缓存面向兼容 API 的第二份对象图。

目标内存复杂度是：

```text
root snapshot = O(global token slots + all control-own slots + control deltas)
local scope   = O(global changes + changed control values + control reference table)
runtime       = O(active scopes + bounded cache + one in-flight transaction)
```

禁止退化为 `O(scope count * control count * global token count)` 的完整 Token 克隆模型。

### 18.2 增量编译与结构共享

`ThemeConfigMerger` 除有效配置外还输出确定的 change set：

- 算法或全局 Token 改变时，重新计算全局表，并重新计算拥有 Own Token 或有效 Control 配置的 Control；没有
  Own Token 且没有配置的 Control 可以继续复用只执行 Global fallback 的空差量 snapshot。这里不使用消费白名单。
- 只修改某个 Control 配置时，仅重新计算该 Control。
- 只修改局部作用域 Button 时，复用父 snapshot 的全局表、palette、资源表和其他 Control snapshot。
- 配置规范化后与当前有效配置相等时直接 no-op，不创建 snapshot，不通知资源。
- 空配置且 `Inherit=true` 的 ThemeConfigProvider 可以直接共享父 snapshot。

单 Control 局部覆盖的结构共享结果如下：

```text
Parent snapshot P
+-- Global table G
+-- Control[Button] B1
+-- Control[Input]  I1
\-- Control[...]    N1

Child snapshot C (Button override only)
+-- Global table G       same reference as P
+-- Control[Button] B2   new value
+-- Control[Input]  I1   same reference as P
\-- Control[...]    N1   same references as P
```

编译缓存分两级：

- 完整 snapshot cache：复用完全等价的有效主题。
- Control compilation cache：以 global token 结构化 key、Control identity、Control config 结构化 key、
  algorithm `(id, revision)` 和 registry revision 为 key，复用跨作用域的 Control 计算结果；fingerprint 只用于
  候选定位，命中后仍执行结构相等比较。

两级缓存共用容量策略，失败和取消结果不缓存。

默认容量由 `ThemeCacheOptions` 固定为：

- 完整 snapshot cache：最多 32 个 entry，最多 32 MiB `EstimatedRetainedBytes`。
- Control compilation cache：最多 2048 个 entry，最多 32 MiB `EstimatedRetainedBytes`。

每个 entry 在插入前按第 9 节的保守公式计算 retained bytes；共享引用允许重复计入以维持上界保守性，但不能
通过抽样 RSS、GC generation 或固定“每个 Token 一个单位”替代。任一 entry 单独超过对应 bytes 上限时可以返回
给当前事务使用，但不得进入 LRU。

容量可以由应用显式调小；扩大默认值必须附带 benchmark、峰值对象计数和 Gallery 导航内存结果。

### 18.3 编译分配

- 编译前根据 schema 中已知的 Token 和 Control 数量预分配数组及稀疏字典容量。
- 减少临时 builder 的完整复制是优化目标；当前 Control 计算仍调用 `CopyBuilder`，尚未实现 copy-on-write global
  token overlay。编译主题包的类型化复制必须保持值隔离，不能把这个目标当作现有零复制保证。
- 大型临时数组可以由有界 workspace pool 复用，但 pooled buffer 绝不进入 snapshot。
- 取消、异常和 stale generation 路径必须在 `finally` 中归还 workspace。
- 同一 ThemeManager 同时只运行一个根事务准备管线；后续请求合并，避免并发编译造成内存峰值。
- 初始同步编译完成后及时释放 ThemeDocument 中不再需要的 XML 对象和临时字符串。

### 18.4 资源查询热路径

`ThemeTokenResourceProvider.TryGetResource` 是控件布局和样式更新的热路径，要求：

- global、Control Effective Global 和 Control Own 命中查询均为零托管分配。
- `SharedTokenResourceExtension` 只使用预生成的全局 key；`XxxTokenResourceExtension` 使用预生成 identity 和
  强类型 Token key。资源查询时不创建 key、不拼接字符串，也不解析 Control identity。
- AXAML Control Effective Token key 包含稳定 identity 和 `SharedTokenKind`；C# owner Binding key 包含 exact CLR
  type 和 `SharedTokenKind`。Provider 通过冻结的 identity/type map 解析当前 registry slot，不使用继承回退。
- `SharedTokenKind` 对应的全局资源 key 在 `ThemeSchemaRegistry` 冻结时预装箱并按 slot 保存；Control global 查询
  和 C# Binding key 在扩展或 Binding 建立时只创建一次，禁止在每次查询时重新装箱枚举。
- 查询只执行类型分支、冻结 map 查询、slot 索引和最多一次稀疏差量 fallback。
- 已注册合法 key 的资源 miss 不创建 diagnostics、异常或临时集合；未注册 exact Control type 属于契约错误并在
  首次解析时明确失败，不作为正常 miss 处理。
- 一次事务对每个受影响 Provider 最多调用一次 `RaiseResourcesChanged()`。

资源通知使用最小通知前沿：

- 根事务只通知 Root Provider；Application/TopLevel 的既有资源传播会使普通逻辑树中的局部 Provider 重新查询，
  不再逐个通知所有后代 scope。
- 局部事务只通知 affected subtree 的最高变化节点 Provider；其后代 snapshot 已在 CommitCore 替换，随父通知
  重新查询。
- 脱离该逻辑树的活动 ThemeContextResourceBridge 需要各通知一次，因为它们是独立资源根。
- appearance 变化的每个显式 ThemeVariantScope 仍必须设置自己的 RequestedThemeVariant；子 scope 的 local
  value 会阻断父 variant 继承，不能只更新根。

### 18.5 生命周期与可回收性

ThemeScopeGraph 在注册有效期间强持有 ScopeNode 和对应 ThemeConfigProvider；Provider detach 时必须通过
注册 token 同步移除节点和全部边。Runtime 不保存已 detach 节点历史。

- 一个活动 ThemeConfigProvider 对应一个 ScopeNode、ThemeContext 和 ResourceProvider。
- ScopeNode 除已 attach 的 ThemeConfigProvider 外不能单独持有 Content、后代 Control 或其他 Visual。
- ThemeConfigProvider 持有 registration disposable；detach、reparent 和异常 attach 路径都必须释放它。
- 一个活动独立 TopLevel 对应一个 `ThemeContextLease` 和一个宿主私有 `ThemeContextResourceBridge`；bridge 只持有
  owner ThemeContext 和通知 subscription，不复制 snapshot 或 Token 表。
- owner 变化必须先建立新 lease 的完整 local context/variant/resource bridge，再释放旧 lease；关闭或创建失败
  后不得在 owner context 的订阅列表中残留 bridge。
- imperative Token 订阅返回 `IDisposable`，由控件在 detach、re-template 或 owner 变化时释放。
- 非 Visual `AvaloniaObject` 使用 DynamicResource 时必须有 scoped `IResourceHost` 和对称 release 路径。
- 普通 Popup、Flyout 和逻辑树内 overlay 不额外创建 ThemeContext 传播订阅；仅独立资源根使用 lease/bridge。
- static cache 不得持有 ThemeContext、Provider、Control 或 Visual。schema、descriptor、Type 与无捕获委托也可能
  保活可卸载程序集；相关绑定应由所属注册域/合法视图持有或使用可回收结构，不得建立进程级强根。
- 事件 dispatcher 不保留已完成 transaction、event args 或 invocation list。

旧 snapshot 的存活必须由活动 context、有界 cache、当前事务候选或调用者合法持有的读取视图解释。
cache 预算不限制调用者主动保留的视图；这些视图不能经父链或缓存额外保活无关历史版本。无法解释的额外存活
snapshot 视为内存泄漏。

### 18.6 性能验证

主题系统必须建立可重复的 benchmark 和 allocation 场景：

- 冷启动：registry、主题文件绑定、默认主题编译和首次挂载。
- 热启动：相同 definition 和 config 的缓存命中。
- 全局切换：Default、Dark、Compact、Dark + Compact。
- 局部变更：只覆盖一个 Control、只覆盖一个全局 Token、十层嵌套作用域。
- 高频变更：连续 100 次配置更新后的请求合并和 stale result 丢弃。
- 拓扑竞争：Prepare 期间执行 scope detach、reattach、reparent 和 Config 替换，验证旧 graph stamp 从不提交。
- FollowSystem：启动时分别模拟 Light/Dark，并连续切换平台 appearance，验证不存在 `Default` variant 或
  snapshot/variant 中间不一致。
- 资源查询：global、Control Effective Global、Control Own 的 hit、fallback 和 miss。
- 独立 TopLevel：根 owner、局部 owner、owner 替换和无 owner 静态 API 的资源与 appearance 语义。
- 生命周期：100 次 Provider attach/detach、Popup/Flyout open/close、独立 TopLevel lease/bridge 创建释放和
  Gallery 页面导航。

验收指标至少包括：

- `TryGetResource` 稳态查询为 0 B/op。
- no-op 请求不创建 snapshot、Provider、Context，不派发资源或主题事件。
- 单 Control 局部覆盖只产生一个新 Control snapshot，其他 Control snapshot 引用保持不变。
- 重复更新后每个作用域仍只有一个 Provider。
- 两级 cache 的 entry 数量和 `EstimatedRetainedBytes` 始终不超过各自配置上限；超大单 entry 不进入 LRU。
- attach/detach 后通过 WeakReference 和强制 GC 验证 ScopeNode、ThemeContext、Provider 及旧 Content 可回收。
- 独立 TopLevel 关闭或 owner 替换后，通过 WeakReference 和订阅计数验证 lease、bridge、旧 owner Visual 和旧
  snapshot 可回收。
- Gallery 随机导航稳定后旧 ShowCase、DynamicResourceExpression 和主题作用域对象数量不单调增长。
- RSS 只作为辅助信息；对象数量和可达链才是内存泄漏判据。

## 19. AOT 约束

- 内置 Token 注册和 schema 全部由源生成器生成。
- 不运行时扫描程序集或 Token 属性。
- 不使用 `Activator.CreateInstance` 创建内置 Control Token。
- 不使用字符串属性路径 Binding 完成主题配置或 Token 查询。
- Application Id 默认值只读取具体 Application 类型所在程序集的简单名称，不扫描已加载程序集。
- 内置和应用资源 Resolver 只使用显式 `avares://` URI；用户 Resolver 只枚举显式配置目录的顶层
  `*.theme.xml`，不反射发现 Resolver、Theme 或 Token。
- 手动刷新不使用 `FileSystemWatcher`、动态代码生成或运行时类型构造。
- 第三方 Control 的 identity、Token descriptor 和资源投影必须由 AtomUI generator 生成并通过真实包级入口注册；默认
  Package 粒度必须完整保留内部 Control 和主题资源。算法 descriptor 只使用 `ThemeAlgorithm` 的封闭成员，并提供显式
  revision/version。
- 生成器测试必须验证 exact CLR type/identity、Own Token、统一强类型 key、常量时间 key 映射、资源投影、资产
  manifest 和输出稳定性，并限制生成源码体积回归。
- 主题系统完成后必须执行真实 Gallery NativeAOT publish，不能只依赖 analyzer。

## 20. 模块组织

主题源码按稳定职责边界组织，不为少量运行时入口创建目录，也不保留 `Catalog`、`Scope`、`Engine`、
`ControlThemes`、`Styling`、`Transitions` 或 `TokenSystem` 这种过细、语义重叠或已经失真的目录。

```text
Theme/
|
+-- IThemeManager.cs                  公开主题管理入口
+-- IThemeManagerBuilder.cs           公开注册入口
+-- ThemeManager.cs                   唯一运行时协调者、snapshot 发布者和 Styles 宿主
+-- ThemeManagerBuilder.cs            启动组装器
+-- ThemeConfigProvider.cs            局部主题配置宿主
+-- ThemeTransaction.cs               Capture/Prepare/CommitCore/Publish/Complete 事务
+-- ThemeCatalogReloadTransaction.cs  Catalog 与全部受影响 Snapshot 的原子刷新事务
+-- ThemeState.cs                     当前已提交主题状态
+-- ThemeTransitionResult.cs          事务结果和值状态
+-- ThemeCatalogReloadResult.cs       Catalog 刷新结果和值状态
+-- ThemeChangedEventArgs.cs          成功结果事件
+-- ThemeChangeFailedEventArgs.cs     失败结果事件
+-- ThemeCatalogChangedEventArgs.cs   Catalog 成功刷新事件
+-- ThemeDiagnostic.cs                结构化诊断
+-- ThemeContext.cs                   稳定主题上下文
+-- ThemeContextLease.cs              独立 TopLevel 的 owner context 生命周期租约
+-- ThemeContextResourceBridge.cs     独立资源根到 owner context 的稳定资源代理
+-- ThemeScope.cs                     唯一可继承的 Context attached property
+-- ThemeScopeGraph.cs                局部主题作用域图
+-- ThemeScopeNode.cs                 作用域节点
+-- ThemeRequest.cs                   主题变更请求
+-- ThemeEventDispatcher.cs           结果事件派发
+-- ThemeExceptions.cs                公开错误类型
+-- ThemeAppearance.cs                snapshot 的 Light/Dark 外观
+-- ThemeInfo.cs                      可用主题元数据
|
+-- Algorithms/
|   +-- 算法 descriptor、attribute、接口与内置算法
|   +-- Default、Dark、Compact 计算器
|   \-- 调色板、颜色映射和算法计算辅助
|
+-- Configuration/
|   +-- ThemeConfig、ControlThemeConfig
|   +-- ControlAlgorithmMode
|   +-- Normalizer、Merger 和 immutable normalized config
|   \-- 一次性 immutable config builder 与结构化 value key
|
+-- Definitions/
|   +-- Resolver/source contracts、内置/avares/用户目录 Resolver
|   +-- CompiledThemeCatalog、ThemeInfo、definition revision 和默认主题选择
|   +-- XML reader、syntax document 和 diagnostics
|   \-- Binder、typed ThemeDefinition 和 bind result
|
+-- Schema/
|   +-- ThemeSchemaRegistry
|   +-- TokenDescriptor、ControlTokenDescriptor
|   +-- ThemeAlgorithmDescriptor
|   +-- ControlThemeAssetDescriptor 和 asset manifest contracts
|   \-- identity、stage、parser 和 resource projector contracts
|
+-- Compilation/
|   +-- CompileInput、Normalizer output consumption
|   +-- ThemeCompiler、TokenValueTable
|   +-- ThemeSnapshot、ControlThemeSnapshot
|   \-- bounded snapshot/control compilation caches
|
+-- Resources/
|   +-- ThemeTokenResourceProvider、ThemeTokenResolver
|   +-- SharedTokenResource、生成式 XxxTokenResource 和 Effective Control Token keys
|   +-- Control theme provider contracts and generated package registration
|   +-- BaseControlTheme and manifest-backed provider loading
|   \-- snapshot lookup、style/template 和 Control theme 加载辅助
|
+-- DesignTokens/
|   +-- global/control compile-only builders
|   +-- Token definitions and kind metadata
|   \-- Token value converters
```

`AtomUI.Generator` 根据 Control、Token 定义、Semantic Part 声明和 AXAML 资产生成
`ControlThemeAssetDescriptor`、`ControlSemanticDescriptor`、引用 identity manifest、`XxxTokens.Identity`、
`XxxTokenKey` 与 `XxxTokenResourceExtension`，并通过构建 analyzer 校验 ResourceInclude、ThemeDictionary、
ControlTheme、Semantic Part marker 和可选 Part Theme。运行时 `Theme/Schema` 只消费生成结果，不解析 AXAML 文本，
也不通过反射补全缺失 identity。

`Definitions` 同时拥有主题来源和主题定义，不再用单独的 `Catalog` 目录把一次加载流程拆开。
`ThemeManager`、事务、上下文和作用域图都属于主题子系统的核心运行时入口，直接位于 `Theme` 根目录；
这些类型共享同一个 snapshot 提交边界，但不需要额外的 `ThemeEngine` 长期对象或物理目录表达这一事实。

物理目录与 CLR namespace 必须严格一一对应：

| 物理边界 | CLR namespace |
|---|---|
| `Theme/*.cs` | `AtomUI.Theme` |
| `Theme/Algorithms/**` | `AtomUI.Theme.Algorithms` |
| `Theme/Compilation/**` | `AtomUI.Theme.Compilation` |
| `Theme/Configuration/**` | `AtomUI.Theme.Configuration` |
| `Theme/Definitions/**` | `AtomUI.Theme.Definitions` |
| `Theme/Resources/**` | `AtomUI.Theme.Resources` |
| `Theme/Schema/**` | `AtomUI.Theme.Schema` |
| `Theme/DesignTokens/**` | `AtomUI.Theme.DesignTokens` |

职责目录下用于拆分 partial class 或文件数量的组织子目录不继续创建 CLR namespace，例如
`Theme/DesignTokens/Definitions/**` 仍使用 `AtomUI.Theme.DesignTokens`。根目录不保留 `.Scope`、
`.Transitions` 等已经被物理结构删除的 namespace。
`Theme/Algorithms` 与 `AtomUI.Theme.Algorithms` 一一对应：`ThemeAlgorithm`、算法 contract/attribute、内置
calculator、调色板生成、预设色、`ColorMap` 和计算辅助均归该命名空间所有。调色板不是独立运行时子系统，
因此不保留 `AtomUI.Theme.Palette`；算法实现也不属于资源样式层，因此不保留
`AtomUI.Theme.Styling` 兼容命名空间。
AXAML Control theme 的发现、聚合和加载属于主题资源所有权，统一位于 `AtomUI.Theme.Resources`，不参与 Token
编译。`SharedTokenKind`、Token Resource markup extension 和相关生成代码同样输出到该 namespace；不保留
`AtomUI.Theme.Styling`。Token builder、Token kind、Token attribute 和 value converter 统一位于
`AtomUI.Theme.DesignTokens`，不保留 `AtomUI.Theme.Tokens` 或 `AtomUI.Theme.TokenSystem`。源生成器必须使用
相同映射，禁止构建后重新生成旧 namespace。

依赖方向固定为：Definitions/Configuration/Schema 提供输入，Compilation 产生 snapshot，根目录的
ThemeManager 提交 snapshot，Resources 只读取已提交 snapshot。Compilation 不依赖 Avalonia 资源宿主，Resources
不执行编译，ThemeConfigProvider 不拥有编译器。每个程序集生成自己的
`AtomUI.Generated.<OwnerAssemblyIdentifier>.GeneratedThemeSchema`，程序集标识中的非标识符字符统一
替换为下划线，禁止多个程序集生成相同全限定类型名，也禁止把生成 namespace 嵌套在可能同名的 Control
类型下面。

## 21. 禁止的架构旁路

以下旁路不属于当前架构，禁止重新引入：

- `Theme`、`ITheme` 以及预创建 Theme variant pool。
- `ThemeCoordinator` 以及一对一 `ThemeManager`/`ThemeEngine` facade 分层；事务队列和当前状态只有 Manager 一个
  长期所有者。
- `Theme.SharedToken`、`Theme.GetControlToken()`、`ThemeConfigProvider.SharedToken`、
  `ThemeConfigProvider.ControlTokens` 和所有 clone compatibility map。
- `SharedTokenSetters`、`ControlTokenInfoSetters`、字符串算法集合、Token 查询属性以及对可变配置对象图的
  collection/property changed 订阅。
- snapshot 中的 `DesignToken`、`IControlDesignToken` 和对应 Clone API。
- `IDesignToken`、`IControlDesignToken` 中面向运行时修改和资源构建的接口。
- `ThemeScope.SnapshotProperty` 和基于 snapshot attached value 的父级订阅。
- `TokenFinderUtils` 和沿 StylingParent 查找旧 Provider 的逻辑。
- `ThemeCreated`、`ThemeAboutToLoad`、`ThemeLoaded`、`ThemeLoadFailed`、`ThemeAboutToChange`。
- 对 Theme ResourceDictionary 的 Motion/Wave 直接修改。
- 自定义主题 id 编码的 Avalonia ThemeVariant。
- `ControlTokenRegistration(Type)`、反射 schema、Activator fallback 和对应 AOT suppress。
- `ControlTokenScope.Identity` 及其旧 Catalog/Id 形式、ControlTheme ambient scope，以及按 TargetType、Control
  继承、ControlTheme `BasedOn` 或运行时反射推断 Control identity 的逻辑。
- Control Token identity、descriptor、配置、snapshot 或资源值跨 Control 继承，以及任何 Token 基类 identity
  fallback。显式标记抽象层的源码定义继承是生成期机制，遵循
  [Control Design Token 继承架构](control-design-token-inheritance.md)。
- Own Token 与 Global Token 同名，以及按 Global Token 消费白名单限制 Control 配置的 schema。
- `ControlThemeAssets` glob Attribute、逐 Theme Module、手工 asset manifest 和只为聚合主题创建的额外 AXAML。
- cache pin、父 snapshot Version cache key 和字符串拼接 cache key。
- 旧主题 XML 的 `ControlTokens/ControlToken` 结构。

这些内容不建立 adapter，也不能以 obsolete、兼容层或内部入口的形式继续存在。

## 22. 验收标准

### 22.1 语义

- 全局 Tokens、Controls、Algorithms 和 Inherit 遵守本文定义的配置合并规则。
- 顶层 ThemeConfigProvider 默认继承当前全局主题。
- 嵌套 Provider 正确合并父 Tokens、Control 配置和算法四态。
- 局部 `Inherit=false` 从 AtomUI library defaults 开始，不继承当前主题定义或父作用域配置。
- Control 覆盖不修改全局 Token，也不创建实例私有作用域；同一 ThemeContext 中全部同 identity Control
  使用一致的覆盖结果。
- 同一主题的新 runtime override 会产生新有效配置，不复用过期 Theme 对象。
- `Algorithms=null` 继承基线算法，空算法列表显式替换为 AtomUI 默认算法链。
- `ThemeConfig`、`ControlThemeConfig`、`ThemeRequest` 及其全部集合深度不可变；替换 Provider.Config 是唯一局部
  动态配置入口，修改旧输入对象不可能触发事务。
- 所有全局和 Control 算法严格遵守 `Evaluate(sameEffectiveSeed, previousMap?) -> nextMap`；后续算法不能把前一
  Map 当作新的 Seed，也不能修改输入。
- 根和每个 `ThemeConfigProvider` 的 snapshot appearance 始终等于其 Avalonia
  `ActualThemeVariant`；Control custom algorithm 的 appearance 不泄漏到作用域 variant。
- 无 owner 的静态 Dialog、Notification 等 API 始终使用 Root ThemeContext，并产生可诊断的根主题语义，不能
  根据当前焦点猜测局部作用域。
- 公开 Resolver 只产生 source，不产生 `ThemeConfig`、`ThemeDocument`、Token 或 snapshot；所有来源使用标准
  Reader/Binder/Compiler。固定内置 XML 的 Reader 阶段在构建时完成，Binder/Compiler 仍在运行时执行。
- 默认 Application Id 来自具体 Application 类型程序集名，显式 `WithApplicationId` 可稳定覆盖；
  `Application.Name` 不参与路径身份。
- 用户主题目录固定为应用配置根下 `{ApplicationId}/Themes`，不递归、不跟随链接、不访问网络。
- 用户主题不得覆盖静态 Theme Id、不得声明默认主题；任意重复 Id 都产生确定 diagnostic。

### 22.2 事务

- Dark + Compact 只产生一次完整提交。
- 根主题变化和所有受影响局部作用域在通知前完成引用替换。
- 配置失败、编译失败和 stale result 不发布部分状态。
- `ApplyThemeAsync` 分别返回可区分的 `Committed`、`NoOp`、`Superseded` 和 `Failed` 结果；被替代请求不返回
  成功；`Failed` 只发生在任何已发布引用交换之前。
- Capture 后 TopologyRevision、RegistrationId、ParentRegistrationId 或 ConfigRevision 任一变化都会使 staged
  result 成为 `Superseded`，旧结果绝不提交到新拓扑。
- Publish 开始后不回滚。AvaloniaProperty、资源通知或观察者异常被记录为 publish warning，事务仍返回
  `Committed`，最终结果携带完整 `PublishDiagnostics`。
- 等价并发请求共享一次 Prepare/Commit；单个等待者取消不取消其他等待者或已经开始的 Commit。
- `ThemeChanged` 或 `ThemeChangeFailed` 的派发先于对应 transition Task 完成。
- 重入和高频更新不会递归提交，latest request 语义确定。
- 订阅者异常不阻断提交或其他订阅者。
- FollowSystem 在首帧前解析初始系统 appearance，运行期切换只发布完整 Light/Dark snapshot 和对应显式
  variant，不出现 `ThemeVariant.Default` 或新旧状态混合帧。
- `ReloadThemesAsync` 对用户目录执行整批原子刷新；任一文件失败时旧 Catalog 与全部 snapshot 保持不变。
- Catalog 刷新提交前同时验证 Catalog generation 和 ScopeGraph stamps；任何观察者只能看到同一代 Catalog、
  CurrentTheme 和 root/local snapshots。
- 当前用户主题被删除时刷新原子回退静态默认主题，并保留最后一次 runtime ThemeConfig。
- Catalog 内容相等时返回 `NoOp`，较新主题或刷新请求使旧刷新返回 `Superseded`。
- `ThemeCatalogChanged` 在完整提交与必要的资源/ThemeChanged 发布后派发，先于 reload Task 完成；订阅者异常只形成
  publish warning。

### 22.3 资源与生命周期

- 根作用域和每个局部作用域始终只有一个 Token ResourceProvider。
- 重复更新不增长 `MergedDictionaries`。
- 控件实例不创建 Control Token 资源字典。
- `{atom:SharedTokenResource ...}` 永远读取全局值；`{atom:XxxTokenResource ...}` 统一读取 Xxx Control 的
  Effective Global Token 和 Own Token，两者没有 ambient 行为。
- global、Control delta hit、global fallback 和 Own Token 均返回正确值；不同 Control identity 的同名 Global
  Token 覆盖不串值。
- 每个 Control 都能覆盖 registry 中任意 Global Token；未知名称失败，合法但未消费的 Global Token 允许存在且
  不得泄漏到其他 Control 或真正的 Global snapshot。
- Token key 具有 AXAML 智能提示和编译期校验；不存在 `Global=`、`Own=` 或字符串 Token 名参数。
- AtomUI Control 都具有可静态验证的 Semantic Part descriptor；名称、class、ContractType、cardinality 或模板变体
  marker 不一致时构建失败。
- AtomUI ControlTheme 资产都具有可静态验证的 owner、引用 Control identities 和可选 Semantic Part Theme 资产
  关系；identity、引用或 TargetType 不一致时在主题注册前失败。
- Popup、Flyout、Dialog 和窗口覆盖层继承 owner ThemeContext。
- 独立 `TopLevel` 在平台 `Show` 前已获得目标 ThemeContext、显式 Light/Dark variant 和作用域窗口背景，暗色初始
  主题不会暴露 Avalonia 的白色默认客户区；临时首帧值不覆盖用户 local value、不建立资源订阅，并在样式接管后释放。
- 普通 Popup/Flyout 依靠逻辑树自然继承；独立 TopLevel 通过一个可释放 lease 和宿主私有 ResourceBridge 继承
  owner context。owner 替换后资源、imperative Token 和 variant 同时切换。
- detach、reparent、关闭和回收后不保留旧作用域或 Control。
- 独立 TopLevel 关闭、owner 替换和异常创建后不保留旧 bridge、lease、owner Visual 或 context subscription。
- 替换 `ThemeConfigProvider.Config` 后旧配置和旧集合变化不再触发 Capture；detach 后配置订阅全部释放。

### 22.4 性能、内存与 AOT

- 同一 definition revision 只成功读取一次，同一 `(definition revision, registry revision)` 只成功绑定一次；
  来源内容变化使 Reader/Binder 缓存失效，descriptor revision 变化只使 Binder/Compiler 缓存失效。
- 用户目录文件数、总字节和单文件字节均受限；超限、symlink/reparse point、目录逃逸和读取竞态有覆盖测试。
- NativeAOT 下可以使用默认程序集名解析 Application Id、读取用户目录、手动刷新并切换到用户主题。
- 等价有效配置只编译一次。
- 人工构造相同 fingerprint、不同结构化配置的碰撞测试不会错误复用 snapshot 或 Control 编译结果。
- 不同 registry revision 允许 slot 重新分配，绑定、缓存和资源查询仍解析到当前 revision 的正确 slot。
- 单 Control 配置变化复用全局表和其他 Control snapshot。
- Token 资源查询热路径保持零分配。
- cache 和旧 snapshot 存活数量满足本文定义的容量上界。
- ThemeConfigProvider、Popup、Flyout 和 Gallery 导航生命周期 WeakReference 测试通过。
- 正常主题切换路径不执行 Token 反射或 Activator。
- Core、Controls、Desktop、DataGrid、ColorPicker、Extras、GalleryBase 和 Gallery 相关测试通过。
- Release 多目标构建和 Gallery NativeAOT publish 通过。
- `git diff --check` 通过，仓库搜索不存在已删除旧 API 的引用。
