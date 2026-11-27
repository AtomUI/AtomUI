# 启动注册方案全局复核（2026-10-13）

## 范围与结论

源码基线：`ea77bcb72`。本次只分析、测量并调整设计文档，没有修改产品实现。
检查范围包括 Generator 片段与 descriptor 输出、包收集/冻结/合并、Core registry、Semantic、
生成主题绑定、资源查询、快照、两阶段事务以及 AOT 后端边界。

推荐继续采用完整包注册与既有按需资源/快照，但应将方案具体化为：
**同一份 descriptor，明确的构建边界，有限的生成代码优化。**
现阶段没有证据支持另建惰性描述符系统、应用依赖扫描器或注册索引解释器。
也没有证据证明直接移除名单可以保持当前首帧耗时。

## 本次测量

重新构建 Desktop Release net10.0，0 warnings / 0 errors。
使用系统临时目录中的独立 .NET 10 控制台探针，通过反射调用当前产物的内部生成收集方法与 registry 构造。
反射只用于取证，不是建议的产品实现；方法解析和 builder 构造位于收集计时外。
测量线程的分配计数不代表进程 RSS 或全部 JIT/native 分配。

收集探针：6 个独立进程，交替执行 Critical→Deferred 与 Deferred→Critical，每进程再执行一遍作为
已缓存路径。以下为两段时间之和的中位数，不能把各自中位数相加替代：

| 当前 Desktop 收集路径 | 时间 | 当前线程分配 |
| --- | ---: | ---: |
| 独立进程首次执行，两段合计 | 206.916 ms | 1,761,160 B |
| 同进程再次执行，两段合计 | 0.506 ms | 69,840 B |

当前两段合计包含 183 个 Control descriptor、99 个 Semantic descriptor、315 个资产和315 个资源注册。
首次与再次执行的差异混合了 JIT、类型/代码加载、不可变缓存填充及相关首次构造；没有使用采样分析器，
不能把全部差值归因于 JIT。此处“首次”不意味着操作系统磁盘冷缓存。

另用独立进程做分项探针，各3次，结果为中位数：

| 独立探针 | 时间 | 说明 |
| --- | ---: | --- |
| Token factory | 69.363 ms | 直接调用183个工厂 |
| Semantic factory | 20.428 ms | 同一探针接着调用99个工厂 |
| Asset factory | 38.552 ms | 同一探针接着调用315个工厂；不构造实际主题字典 |
| 完整 registry 首次构造 | 17.142 ms | Common+Desktop 已收集，global schema已创建，使用包指纹组合入口 |
| 完整 registry 再次构造 | 1.304 ms | 同一不可变描述输入 |

分项探针受执行顺序、公共代码预热与反射调用数量影响。不是无侵入的耗时归因，不能相加为
完整注册成本，也不能与206.916 ms作差来声称某个具体函数占比。
没有测量当前方案的 Gallery 首帧、Ready、发布AOT、Browser或新方案端到端性能。

原始数据与被测DLL SHA-256保存在 `.superpowers/sdd/2026-10-13-registration-design-audit/`。
临时工程、源码和可执行产物已在分析结束时删除。

## 代码层面的具体发现

### 1. 已存在的优化不能再次计算收益

- `TypeMapRegistrationWriter.WriteCachedDescriptor` 已按类型/资产缓存不可变 descriptor。
- `ControlThemeAssetDescriptor.CreateGeneratedPreSorted` 已有生成数据预排序路径。
- `ThemeSchemaRegistry.ToOrderedArray` 已对有序输入使用线性检查。
- `TokenDescriptor` 已共享 codec/accessor，避免每Token一套委托。
- `StaticThemeResourceProvider.ResourceTable` 已有资源键索引，并保留开放资源的有序回退。
- `ThemeCompiler.CompileControls` 已创建惰性cell，重用父cell；无OwnToken且无配置时使用空快照。

因此新方案的主要维护收益来自删除手工分区及其状态，而不是“再实现一次按需加载”。

### 2. 重复工作可从现有边界内消除

`ControlPackageRegistration` 对资源、控件、资产、Semantic分别复制/排序/校验；
`ThemeManagerBuilder.MergeContributionsInto` 又对资源排序；registry再复制/检查/绑定slot。

其中部分属于合法的不同边界：公开可变输入的防御性复制、跨包冲突、最终slot分配不能省略。
应先明确已冻结数组的所有权和规范顺序，在内部消费路径复用；不能以“生成数据可信”为由删除所有验证。

资产跨片段引用当前会通过 `HasSameMetadata` 重复比较。生成工厂已复用同一实例，
对象同一性的快速返回可能避免无意义的序列遍历，属于小范围候选优化；不同实例仍需结构比较。

### 3. 冷启动与热注册不是同一个问题

六进程数据说明，热态收集循环不是当前首要问题。生成片段、缓存getter、Token/Semantic/资产工厂的
首次执行与构造应优先剖析。可以评估减少重复生成方法体、共享普通辅助实现、缩减临时对象；
不要默认引入二进制schema解释器、通用VM或并行初始化调度器。

### 4. 编译期排序有明确边界

包内稳定顺序和包内schema可在生成期确定；最终应用的保留集合、跨包冲突和registry dense slot
仍取决于实际注册包与裁剪结果。不能生成全局固定slot，也不能让全包Type/工厂静态表成为链接根。
生成契约指纹使用metadata身份，而部分运行期顺序使用真实类型身份，不能直接假定两种顺序一致。

### 5. 分区是可删除的复杂度，事务不全是

关键集/余量collector、余量segment、消费标记、未完整包指纹分支可以随迁移退场。
显式Deferred可选包仍会改变registry，主题/语言原子提交、取消和稳定owner保护仍需要保留。
不能把删除包内分区描述成“一次性删除所有Stage 2演化代码”。

## 备选方案比较

| 方案 | 首次执行成本 | 维护性和语义 | 结论 |
| --- | --- | --- | --- |
| 手写关键集或改成特性 | 可减少首帧前构造 | 依赖闭包继续人工维护 | 排除 |
| 自动推导应用首帧闭包 | 可能减少构造，但未证明完整 | 动态创建/跨包复杂，重复链接器职责 | 排除 |
| 所有描述符按需创建 | 能推迟更多首次构造 | 增加查询状态、校验时机和错误处理复杂度 | 不作为基础架构；仅在证据充分时局部考虑 |
| 完整注册现有descriptor，复用既有惰性资源和快照 | 首帧承担完整元数据构造，必须测量和优化 | 边界最清楚、无人工依赖表、改动可控 | 当前推荐 |
| 独立原生加载窗，框架之后一次初始化 | 有利于快速显示加载界面，但本次未测 | 改变首屏产品形态与窗口交接，不能等价于完整AtomUI主窗首帧 | 只有产品明确接受时才考虑 |

若完整注册的冷态成本优化后仍不满足首帧预算，需要在预编译部署、首屏交互范围或局部惰性描述之间
做有数字支撑的取舍。不能同时承诺任意完整AtomUI首屏、几乎零注册耗时、以及没有任何延迟初始化复杂度。

## 建议执行顺序

1. 固定当前端到端基线，明确首帧/Ready/首次页面/内存预算。
2. 优化现有生成代码的首次执行与冻结数据的内部消费，逐项A/B，保持公共入口防御性校验。
3. 移除包内分区，验证同包任意控件首阶段可用，进行完整端到端比较。
4. 只有证据显示必要时才讨论局部描述延迟，不默认扩大架构。

此次是方案分析，不是重构验收；没有运行全量回归，也没有创建提交。


## 实施后补充验收

本次改动已删除名单、双collector与Core余量段，保留旧ABI签名作为同次顺序收集适配。
复用原有descriptor缓存、资源与快照惰性路径；包内有序数组复制一次，同实例metadata快速去重。
尝试用BCL LazyInitializer替换生成缓存发布代码，8组交错进程采样从216.734ms/1,756,496B变为
236.598ms/1,822,360B；该试验已撤回，没有将新增委托开销引入最终实现。

清理后的focused测试：Core79、Generator118、Desktop17、Gallery26，共240项通过、0跳过。
net8与net10 Release构建通过。Release Gallery由用户确认红绿灯、顶部应用名、窗内菜单、五款主题色及
未用控件页面均正常。NativeAOT osx-arm64产物校验与5秒启动检查通过；Browser Release裁剪版在IAB实际
渲染Gallery首页，控制台无错误。未运行全量回归；Browser AOT与net8发布本轮未执行。

NativeAOT首轮遭遇SDK csc进程139退出；限制DOTNET_PROCESSOR_COUNT=2后同源发布通过。
现有ReactiveUI trim/AOT、Homebrew dylib最低系统版本警告仍存在。本次没有通过修改产品源码或屏蔽诊断解决编译器崩溃。

### 空窗口端到端对照

基线是隔离worktree中的ea77bcb72，当前是本次工作树。独立临时程序引用两侧真实项目，采用相同的
800×500 AtomUI Window、Render哨兵、默认主题、en-US、AlibabaSans和Desktop包；两侧均设置空Deferred块，
等待IAtomUIStartup.Completion后在ApplicationIdle退出。它测量框架注册与窗口路径，不包含Gallery shell。
每侧先预热一个独立进程，再做6组交错独立进程采样；未设置DOTNET_PROCESSOR_COUNT运行探针。
下表为自Main中的Stopwatch启动计时；不含托管Main之前的进程/运行时启动时间，也不代表显示器presentation。

| 指标（中位数） | 基线 | 当前 | 差值 |
| --- | ---: | ---: | ---: |
| UseAtomUI同步段 | 247.88 ms | 370.72 ms | +122.84 ms |
| Window.Opened | 804.96 ms | 938.86 ms | +133.90 ms |
| 首次Render | 849.92 ms | 986.47 ms | +136.55 ms |
| Framework Ready | 1169.70 ms | 1161.03 ms | −8.67 ms |

**结论：这次替换有明确的JIT首帧成本，完整就绪基本相当。** 维护性改善来自移除人工依赖表和内部段状态；
不能把本次重构称为首帧提速。NativeAOT端到端性能没有新旧对照，不能将JIT差值外推到AOT。
若产品要求恢复旧版JIT首帧预算，应重新评估局部描述延迟或部署方式，不能静默恢复名单。

原始样本和DLL哈希在`.superpowers/sdd/2026-10-13-complete-registration/`。
临时测试、A/B探针、端到端程序、发布目录、浏览器页与服务器均清理；基线worktree归档。

最终自审另外修正旧双collector入口的失败短路：第一段吞掉collection异常时也不得执行第二段。临时用例RED→GREEN，清理后再次运行Core定向验证。Native/Browser发布验收先于此项旧入口异常路径修正，普通生成注册与裁剪映射未改变。
