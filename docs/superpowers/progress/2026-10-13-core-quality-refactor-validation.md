# AtomUI.Core 质量重构验证记录

基于 2026-10-13 源码审查与当前未提交工作树，执行计划为 `docs/superpowers/plans/2026-10-13-core-quality-refactor.md`。
本次重构与验证已完成；验证阶段未提交，随后按用户明确授权整理提交。公开启动契约由用户明确批准破坏性重构。

## 实现与证据

| 范围 | 实施结果 | 定向证据 |
|---|---|---|
| 成功封存与能力检查 | Freeze 检查失败/未完成包；启动专用配置在入口检查阶段，关闭不代替成功校验 | 四项初始临时 RED → GREEN；连同既有启动/主题/注册/目录用例 79/79 |
| 事务执行 | schema 后台准备，进入原执行门后读取有效主题和 scope；主题/语言提交及全部通知在门内 | 重入事件版本保持一致 |
| 排队请求 | 注册演化沿用用户请求代际；目录刷新进门后捕获完整当前基线 | 两项固定排队次序 RED → GREEN；含既有事务/目录用例 27/27 |
| 构建去重 | 初次/增量共用贡献合并、规范化配置与目录准备；Semantic 候选构建一次 | 既有分区、资源顺序、语义与目录用例；最终 Core 153/153 包含对应模块 |
| 资源所有权 | 尝试挂载即记录，部分挂载/解绑抛错仍独立清理每个资源和 bridge | 10 失败+1 对照 → 11/11；临时删除后 35/35 |
| 集合/Motion | 集合弱订阅、每次 motion 的取消和清理所有权、Move 动画复用、UnFollow 解绑 | 初始 9 失败+1 对照；扩展 17/17；清理异常额外 6 RED → 7 GREEN；临时删除后 Core 1/1、Desktop 最后 7/7 |
| 缓存/编译 | 不可变结构键复用，按需恢复全局 builder，冷输入和键计费 | 3 个预算遗漏 RED → GREEN；删除 7 个临时用例后 54/54 |
| 宿主就绪 | 构造/登记/Start；关闭同步取消释放 lifetime；Hide 作废已请求帧 | lifetime/描边 2 RED → GREEN，Hide-after-batch 1 RED → GREEN；合并就绪/查询用例 23/23 |
| Icon | 默认主描边初始化到 Stroke 槽 | 默认元数据画刷 RED → GREEN |
| 统一启动契约 | 用户批准 IAtomUIStartup：State、Completion、ProgressChanged；移除旧阶段事件，迁移 Gallery | 旧行为 3 RED；Core 契约/旧用例 37 GREEN，Gallery 真实场景 8 GREEN；清理后 Core 20/20、Gallery 5/5 |

所有新增探针遵守 Temporary 生命周期；各子任务与 root 启动探针已删除。10 月 13 日质量审查的临时探针未转为常驻测试；此前分阶段实现的契约测试保留情况见文末。
本表包含重叠执行，不相加宣称独立测试总数；未运行全量或 affected-wide 回归。

## 微基准

相同输入、Debug 构建，修改前后各 10 个独立测试进程；每进程先 warmup 20 次。
重复 key 2000 次、复用全局但无 Own 查询 500 次、编译加首次 Own 查询 100 次。
计时为 Stopwatch，分配为当前线程 GC allocated bytes；表中为进程均值的中位数。

| 场景 | 前 / 后耗时 | 前 / 后分配 |
|---|---|---|
| 重复结构键 | 5.094 / 0.684 µs | 1240 / 32 B |
| 全局复用、零 Own 查询 | 43.339 / 1.167 µs | 4344 / 296 B |
| 编译加首次 Own 查询 | 57.617 / 39.222 µs | 9360 / 9168 B |

首轮新增计费曾使首次 Own 回退到 73.326 µs；改为复用不可变根资源/调色板估算后消除该回退。
这些数据不代表 Gallery 启动时间或 Release/AOT 性能百分比。

## 性能候选的处置

- 保留有界缓存扫描；没有证据支持增加增量订阅图。
- 保留跨 registry revision 禁止共享控件 cell 的安全边界；未实现高复杂度的 slot/计算分离。
- 保留 protected DrawPens 的现有创建契约；尚无兼容且实测受益的替代方案，不声称其分配已消除。
- 未添加紧凑完整 schema 的新生成协议或 ABI；需要独立证明真实裁剪集合和第三方贡献可达性。
- 显式非激活宿主继续使用 ShowActivated=false；没有通过固定计时器伪造操作系统激活证据。

## 收尾门

- [x] 统一启动契约与 Gallery 迟窗口验证、原始异常及独立等待取消。
- [x] 最终审查有效问题处理；临时用例和专用辅助工具清理。
- [x] 清理后的最终模块验证、net8/net10、适用 NativeAOT 与原生 Gallery 验证。
- [x] 更新最终命令、通过数量、限制和文件清理结果。

过程证据保存在 `.superpowers/audits/core-quality-refactor-2026-10-13/`；具体命令见各子目录的 evidence/progress/validation 记录。

## 终态与额外分配

独立审查修复后又以真实执行门排队验证取消：旧顺序先检查已 Dispose 的 Manager，会错误抛出
ObjectDisposedException；先检查调用方传入的启动取消令牌后，Completion 保持 Canceled。
该用例 RED 1/1 → GREEN 46/46（含既有用例）；删除后对应 Core 精确验证 45/45，697 ms。

统一状态对象每个应用额外分配 owner、TaskCompletionSource 与 Task。净分配测量：100 次预热、10,000 次构造、
GC.GetAllocatedBytesForCurrentThread，结果 **128 B/应用**，不包含用户回调和注册工作；测量源码已删除。
这项 API 改造有明确的一次性成本，不将它描述为零成本，也不把 compiler 微基准换算为整机启动提升。

最终取消检查调整后、全部临时探针删除后，Core 综合模块过滤 **153/153（2.6384 s）**。
最终源码的 Release/net8 与两条 AOT 路径均已重新构建并执行验证。

当前工作区 Release Gallery（PID 48355、精确 `.artifacts/bin/Release/net10.0/AtomUIGallery.Desktop`）
由用户确认红绿灯、macOS 顶部应用名、窗内菜单、五款主题色及切换全部正常。CUA 精确路径绑定失败，
未改用其他同名应用。验证进程已由 launcher 按已记录 PID 和路径校验后停止。
最后的检查顺序调整只影响等待执行门时关闭的取消路径，该路径另有运行复现。

## 最终构建与原生结果

| 验证 | 结果 |
|---|---|
| Gallery Release net10.0 | 成功，0 warning / 0 error，35.42 s |
| StartupPerformance Release net8.0（包含 Core/Controls/Desktop） | 成功，0 warning / 0 error，16.69 s；无 ATOMUIREG009 |
| Gallery 官方 PublishToLocal NativeAOT osx-arm64 | 发布成功，脚本产物检查通过；最终精确产物存活 8 s、无异常输出，验证后终止自己的 PID |
| 非 staged StartupPerformance NativeAOT | 发布成功，未含 CoreCLR/JIT 标志文件；实际首次 Render 后 exit 0 |
| 非 staged StartupPerformance net8 | 实际首次 Render 后 exit 0 |
| 原生 Gallery UX | 用户确认红绿灯、顶部应用名、主菜单、五款主题及切换正常 |
| 独立源码审查 | 事务排队/旧目录基线与 Motion 清理问题已 RED→GREEN；最终无 P1/P2 阻塞项 |

NativeAOT 保留已有 ReactiveUI IL2104/IL3053 聚合警告，以及本机 Homebrew dylib 最低 macOS 版本链接警告；
没有通过屏蔽诊断解决，也不声明 AOT 零 warning。最小 harness 初次发布缺少 ssl 搜索路径，验证时临时复用仓库
已有 `build/MacOSHomebrewNativeAot.targets`，同一工程局部启用 PublishAot；临时 csproj 改动已逐字恢复。

单次 smoke 的 Main→first Render 为 net8 883.193 ms、NativeAOT 238.421 ms；用途是确认真实渲染与退出，
没有统计分布，也没有新的整机启动前后百分比结论。微基准、框架就绪和完整 Gallery Shell 不混为同一指标。

最终 Core 命令是 `dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj --framework net10.0 --filter ...`，
过滤 18 个已改动模块的测试类：ThemeManagerTransaction、ApplicationScope、ApplicationLocalizationStartup、
ControlRegistrationRuntime、ThemeStartup、ThemeCatalogReload、ThemeTokenResourceProvider、ThemeContextResourceBridge、
BoundedThemeCache、TypedThemeSnapshotCache、ControlCompilationCache、DenseThemeCompiler、ThemeConfigImmutability、
ThemeConfigNormalizer、ThemeSnapshot、ThemeScopeAndTransactionPerformance、ThemeResourceLookupPerformance、AbstractMotion。
完整展开的命令保存在 `final-commands.txt`，测试名在 `final-core.log` 及各子任务 evidence/progress 记录。
Gallery 既有布局定向测试 5/5；Desktop 集合/动画消费测试按变更模块验证，最终动画 7/7，集合换源两例保持通过。

## 交付处置

- 临时测试、专用 helper 与发布工作目录已删除，全部验证进程结束；临时探针没有转为常驻测试。
- 性能工具 csproj 的临时 AOT 条件与本机 linker import 已恢复，未留下性能探针或额外项目引用。
- 正式启动/主题/资源/模块/控件架构文档与待发布 API 迁移说明已更新；没有更改包版本。
- `git diff --check` 和文档本地链接检查通过；没有运行全量回归；提交遵循用户后续明确授权。
- 当前工作树继续保留本轮及原有启动改动供用户审查。

## 用户授权提交前的清理与测试保留

本次再次全仓核查源码、测试工程和前期执行目录。产品代码没有遗留临时计时/分配埋点，Temporary 测试已清空。
另外删除前期遗留的三个测试工程备份（theme-menu-tests-input.csproj、gallery-tests-input.csproj、
generator-tests-input.csproj）；历史测量数据与验证日志继续作为证据保存，不进入产品或测试构建。

删除对象：ThemeSchemaRegistry.GetControlSharedResourceKey 两个测试专用重载、对应 ConcurrentDictionary，
以及 ControlShared_Key_Is_Boxed_Once_Per_Registry_Slot 用例。
删除原因：生产调用只使用公开的 identity/type 资源键入口；该用例只固定已无生产消费者的装箱缓存身份。
保留保障：其余资源值、覆盖、隔离、fallback 测试改为使用生产的 ControlTokenResourceKey.Global 入口，
既有零分配查询测试保持不变。没有新增 public API，也没有改变公开 key 值类型的布局。
清理后精确验证 ThemeTokenResourceProviderTests、ThemeResourceLookupPerformanceTests、ThemeSchemaRegistryTests：
**28/28 通过，1.7702 s**。这次只删除测试专用代码，不重跑已经完成的整套发布/平台检查。

### 常驻测试准入说明

此前分阶段实现相对 HEAD 新增了 8 个测试方法，另将一个既有目录测试扩展出 staged 参数场景。
此次删除一个无生产契约的缓存身份用例后，相对 HEAD 的执行场景净增为 8。之前“没有净新增”的表述只适用于
质量审查新写的临时探针，不能用于整个待提交差异；此处明确更正。

| 保留组 | 独立契约与每次全局执行的理由 | 最近定向运行成本 |
|---|---|---|
| ApplicationScope 的首帧等待、两种 prepare 失败、initializer 拒绝、观察者异常，共 5 项 | 调度与跨主题/语言原子性是通用启动入口保证；普通主题事务测试不经过这条编排，编译器/ApiCompat 检不出错误发布或回调副作用。用共享 Headless 用例覆盖所有宿主的框架协议，避免每次依赖原生手验或重型发布。 | 各 1–18 ms |
| 本地化 Stage 2 后切换语言，1 项 | 证明新增 catalog、bundle 和语言集合已进入同一可切换快照集；现有非演化语言切换无法发现退回 Stage 1 数据的错误。纯 Headless 查询即可发现数据丢失，不需要真实平台发布。 | 145 ms |
| 分区资源全局顺序和单次挂载，1 项 | 包主体/余量段有交错优先级和相同资产，需证明最终可查询结果与语义均正确；生成源码可编译不证明运行时去重与覆盖顺序。它保护共享注册合并机制，替代各控件重复验证。 | 7 ms |
| conditional 后端分期回退，1 项 | 保护 net8 的完整注册与 REG009 warning 契约；正常 net10 生成测试不能发现后端选择退回 error 或遗漏收集入口。直接 writer 测试覆盖诊断和两种输出，无外部 build/publish。 | 15 ms |
| 既有目录通知测试增加 staged 参数，1 个执行场景 | 复用原测试的目录内容/根状态断言，确保新主题目录可见且通知原子；普通手动 Reload 通过不能覆盖启动演化遗漏通知。无需另建菜单 UI 测试。 | 4 ms |

这些用例保护独立的长期行为，既有测试或编译/生成诊断不能替代；每项均远低于 1 s，合计本轮成本约 202 ms。
保留它们是测试准入例外，不将复现探针、计时程序或文件改名检查升级为常驻保障。
