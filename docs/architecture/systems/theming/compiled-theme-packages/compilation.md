# 编译主题包：构建与产物协议

> 状态：已收敛为静态声明生成，尚未发布；本次本地范围验证已完成。范围和共同语义见[总览](overview.md)；
> 本文拥有构建输入、生成协议和保留边界，验证状态见[验证文档](verification.md#8-当前验证状态)。

## 1. 构建输入与权威事实

包内主题以显式 `AtomUIThemeDefinition` 项进入构建。输入集合至少包括：

| 输入 | 必须保留的事实 |
|---|---|
| 原 XML | 原始字节摘要、逻辑资源身份、Link/路径、namespace/schema 版本、位置映射 |
| 包声明 | 稳定包身份、定义程序集、声明版本、支持平台、主题 Id 与注册顺序 |
| Global/Control Token 契约 | 定义程序集 + 完整 owner metadata name + Catalog/Id，Token 名称/值类型/stage/key、默认值契约 |
| 算法声明 | 有序算法身份、Control 算法模式与声明 appearance；实际求值保留在运行时 |
| 值能力 | literal 解析、合法构造、typed copy/freeze/thaw、资源投影与不可变性契约 |
| 构建目标 | XML 协议、生成 ABI、字面量能力、目标框架及相关编译选项 |

源身份使用稳定包/资源路径，产物不写开发机绝对路径、构建时间或进程随机 hash。错误定位可以引用源文件，
发布元数据采用规范化逻辑路径。摘要针对一次有界读取的同一字节快照，不能以时间戳或文件长度替代。

构建沿用 XML v1 的 DTD/外部实体/网络禁止、文档/元素/Token 容量和唯一性检查。没有 include/import、
脚本或环境变量求值扩展；复杂度和产物膨胀也受显式预算约束，不穷举任意颜色、字号和字体的笛卡尔积。

## 2. 工具分工

Core 保留现有 ThemeCompiler、ThemeConfig、ThemeSnapshot、事务及缓存。算法使用既有 Avalonia/Core 类型与
数学实现，只在运行时求值。动态 XML 继续由原 Reader/Binder 处理。

- Toolchain Tasks 源链接 Core 的 Reader/XSD、`BuiltInThemeLiteralCodec`、运行时 converter 与必要值类型。
  字面量调用与动态 XML 相同的 converter，再传输规范字符串与类型化 primitive payload；不执行算法或用户 codec。
- 本工程 Token 类型到 Roslyn 编译时才可见，因此 worker 为每个不同字面量计算有界的内置类型候选集，
  同一任务内缓存重复文本；传输最多 32 个候选、8 MiB 总字节，并限制复合值深度和元素数。
  Generator 按已校验的值类型选候选，仅构造 C# 表达式，不再维护字面量解析器。
- worker 私有依赖 Avalonia 的托管程序集以复用真实值语义；这些依赖随 `tools/net10.0` 分发，
  不加载产品程序集，不需要桌面宿主，不成为应用运行时依赖，也不建立 Core/Generator 自举循环。
- 主题算法、调色板派生与颜色明暗调整均留在 Core；工具链不维护第二份算法实现。
- Tasks/Generator 是构建依赖，不进入应用运行时输出；Core 不引用构建工具程序集。

现有 Token/schema/registration 生成在共同 Compilation transform 中建立事实。主题生成必须共享这一值模型，
或消费确定的前置契约快照；不得依赖另一个 Source Generator 在本轮生成的 `.g.cs` 可见性或执行顺序。
增量缓存不保留 Compilation、SemanticModel、ISymbol、打开的流或用户项目对象。

### 2.1 同工程与依赖包

同工程 Token/主题声明使用当前编译的符号模型校验，发射引用最终可访问声明的代码，最终 C# 编译负责链接。
它不要求先编译当前程序集再运行该程序集里的 evaluator，因此没有自举循环。

依赖包必须随包导出版本化的中立构建契约。现有 schema fingerprint 本身不足以表示完整的字面量、类型与
可访问工厂信息。契约由拥有 Token 的项目生成并与其运行期 descriptor 使用同一事实源；不能反射读取
internal 生成类、继承层次或运行期 registry 来补事实。

跨程序集调用仅使用受控公开生成 ABI 或合法可访问入口。internal Token 实现由其所属控件包封装，主题包不能
绕过可访问性直接构造。元数据可描述完整包，但不能因此创建全控件 Type 或 factory 的静态聚合表。

## 3. 编译阶段

```text
有界 XML 快照 + 导出 Token 契约
  → Reader/XSD 与位置诊断
  → 语义绑定：owner / token / type / stage / algorithm / appearance
  → 类型化静态声明与合法 runtime 参数边界
  → 类型化默认赋值 + manifest + 控件契约
  → 生成 C# 及正常编译、打包、链接验证
```

### 3.1 语义绑定

构建期解析静态 Token 名称和值，检查精确 owner、Own/Global 名称冲突、算法模式和声明 appearance。
匹配按稳定身份完成，不分配可持久化的应用控件 slot；registry-dependent 装配留到实际注册阶段。

静态覆盖保留 Seed/Map/Alias/Own 的阶段标签及声明来源，不提前合并应用初始配置、运行时请求或父 scope。不能根据“当前没有使用某字段”删除合法覆盖，因为后续控件与配置仍可能消费它。

### 3.2 支持的值生成

支持域由显式 codec 契约决定。内置数值、布尔、颜色、枚举、时长、几何等可生成类型化值/数据；
字体只生成不可变描述，具体字体匹配、文件打开和原生 face 必须留给运行宿主。

FontFamily 是经明确授权的构造边界例外：当前依赖只公开字符串/Uri+字符串构造，生成代码可调用标准构造函数，
保留其内部字体描述解析。构建期验证描述并保留来源 URI、完整 fallback 名称及顺序；不得反射写入私有状态、
替换成 `$Default` 或丢弃名称。这个例外不允许重新运行主题 XML Reader/Binder 或 descriptor 的 Format→Parse，
也不允许构建期访问 FontManager。性能报告单独列出 FontFamily 构造及其保留的内部解析，不宣称该部分为零。

静态支持域同时具备：

1. 构建期 literal 验证与确定性类型化表达。
2. 与运行期相同的范围、溢出、枚举及非有限数处理。
3. 无静态字符串往返的 typed clone/freeze/thaw。
4. 与现有资源 key、CLR 值类型和投影语义一致的 materializer。
5. 对嵌套数组、集合和可变对象明确的防御性复制/拒绝规则。

只生成 `Color.Parse("...")` 等运行期静态解析调用不算完成类型化编译。也不能在恢复全局 builder
或首次 Own 计算时经 `Format → Parse` 把成本带回来。运行时用户传入的字符串覆盖仍可正常解析，计费单独记录。
任意第三方 evaluator 内部行为不属于“框架生成路径无静态字符串解析”的保证；必须明确标为 opaque runtime 调用。

跨程序集入口、编译时名称与枚举键的双重核对、实际 owner 程序集以及失败 writer 的关闭规则见
[生成 ABI v1](../../../../reference/theming/compiled-theme-abi-v1.md)。

没有合法构造/冻结表达的自定义静态字面量在构建时失败，不把字符串存入产物等运行时偷偷补解析。作者可以明确
选择动态 XML 来源；这种选择不是编译成功的等价结果。

### 3.3 生成工厂

生成程序描述必要契约并写入类型化静态默认覆盖。现有运行时按阶段合并有效配置、执行算法、冻结结果和资源投影。
静态路径不创建 ThemeDocument/BoundThemeDefinition 往返模型；Catalog 的统一内部定义视图需支持两种来源，
默认配置与根/局部合并接口不能要求所有静态包重新构造旧 Binder 对象。

保留当前惰性 Control 边界。生成程序只描述显式控件覆盖；运行时按实际注册的 descriptor 提供 Own 构造入口，
不在主题选择时调用所有 evaluator。
可共享的全局数据保持一份；不为每个控件生成一整份全局资源表。菜单 manifest 与大 payload 分离，
静态构造函数不能遍历并物化所有主题、控件资源或 palette。

## 4. 产物身份与兼容性

产物要区分以下身份，不复用一个含义模糊的 Version：

| 身份 | 用途 |
|---|---|
| SourceIdentity / SourceRevision / 原始摘要 | 来源与诊断/刷新证据 |
| ProgramDigest | 已类型化定义、依赖契约与生成语义的内容身份 |
| Generated ABI | Core 能否正确解释元数据并调用工厂 |
| Token/codec/projector 契约 | 类型、阶段、资源投影和可变性是否匹配 |
| RegistryRevision + 实例身份 | 当前装配位置、缓存与控件绑定的有效域 |

持久摘要使用版本化、确定编码和规范顺序；不能使用进程 HashCode、Type.GetHashCode 或运行期 slot 作为文件身份。
摘要用于索引和快速拒绝，运行期等价判断仍保留必要的结构校验，不因 hash 相等直接合并不等输入。

| 不匹配 | 行为 |
|---|---|
| XML/schema/静态 literal 错误 | 构建失败，删除或使旧输出不可用 |
| 必备 ABI、schema、owner 或 codec 不兼容 | 包注册/候选绑定失败；诊断明确指向包、主题和契约 |
| 生成程序契约不受当前 Core 支持 | 明确失败，不调用未知生成入口 |
| 外部动态 XML 错误 | 沿用现行来源/目录/主题事务失败规则 |

程序集版本不能代替内容身份；AOT/单文件不能依赖运行时再读 DLL 文件来计算兼容性。
构建时输出的契约证明与实际发布视图必须一致，由打包/发布检查验证。

## 5. 增量构建、确定性与交付

增量 key 覆盖 XML、XSD/Reader、Token 构建契约、字面量解析与生成器/ABI、目标参数及相关编译选项。
依赖包 Token 契约改变时，即使 XML 字节没变也必须重新校验。只以 XML mtime 增量判断不合格。

输出按目标框架、配置和需要隔离的目标域分开；并行构建不能共享可写暂存文件。生成先写候选文件，全部校验
通过后替换最终输出；失败不能继续编译上一次成功生成的 `.g.cs` 或数据文件。

跨包构建分工：

- 控件包构建导出契约；主题包引用已解析的版本化契约。
- 本工程符号生成类型化注册与默认赋值；不依赖本工程先产出可执行 DLL。
- Tasks 与 Generator 之间的私有传输版本为 4，包含声明、位置、摘要、诊断及真实 Core codec 产生的有界类型化候选值；保留记录/字节限额和 payload 摘要校验。
  旧版本输入明确拒绝，重新生成后再编译；此版本不改变运行时生成 ABI v1。
- Generator 在共同 transform 中校验完整 Token 契约，不读取另一 Generator 的本轮输出。
- NuGet 同时验证运行入口、契约元数据、必要构建工具和产物摘要；工具程序集不进入运行时输出。

记录原始输入、契约身份和最终输出摘要，能证明产物来自本次构建。源码位置映射只存必要信息，
不能把整个 Compilation、源码工程或绝对目录保存在运行时 manifest。

## 6. 裁剪与 AOT

生成器不分析应用 usage，不构造应用依赖闭包；裁剪仍由真实 ILLink/ILC 与现有逐控件注册后端完成。
编译型主题路径在 net8 条件后端、现代 TypeMap、Browser 裁剪/AOT 下使用相同工厂与值语义。

- 包注册只启用声明的主题能力，不自动调用 UseDesktopControls/其他可选控件包。
- 完整构建契约使用中立 metadata，不产生全控件 managed Type/factory 根表。
- 全局主题和公共元数据不引用每个控件的默认资源 factory；默认 Own 由实际保留的控件 descriptor 承接。
- XML 显式 Control 覆盖生成 owner-bearing identity，owner Type 必须被真实验证/使用，复用现有 typed key 的
  保留证据；不得用无用途 `typeof` 或枚举裸整数伪造保留。
- 显式覆盖某控件本来就是主题的必要依赖。只为这些 owner 产生真实引用，不把其所在包的所有兄弟控件保留。
- 真实发布视图必须满足所有显式要求；没有注册必要包/owner 时失败，不能忽略 XML 条目。动态 XML 的任意
  字符串本身不成为 root，应用需要按现有动态主题规则确保目标已注册/保留。
- 提交时 registry 不满足定义要求的处理由[总览](overview.md#9-控件要求与启动提交)规定；AOT 保留与运行期是否
  已注册是两个不同条件，不能混为一谈。
- 平台能力与控件/资产声明共用既有模型；构建宿主不解析字体服务或物化目标平台资源。

验收同时比较同一已保留 schema 下两条编译路径的值，以及加入主题包前后的实际保留集合。不能因值相同就忽略
意外扩大的二进制、工厂集合或首次 JIT 工作量；保留差异只能来自明确的主题 Control 要求。
