# 按改动影响选择验证

本文定义 AtomUI 日常验证入口、影响选择规则和结果有效性。Bug 修复采用“精确复现 → 模块迭代 → 测试去留与清理 → 最终模块验证”；
不能把 `dotnet test AtomUI.slnx` 当作默认或完整测试清单。测试项目由仓库实际 `.csproj` 发现，不依赖解决方案是否收录。

测试是否长期保留及 Bug 修复完成条件由[测试价值与生命周期规范](../development/test-value-and-lifecycle.md)定义。
临时复现测试验证完成后默认删除，最终模块验证认证清理后的工作树；缺少有效证据、测试处置或清理不能宣称修复完成。
全量仅在用户明确要求或发版时运行，不是每次修复的默认收尾步骤。

## 日常入口

在仓库根目录使用 Python 3.9+；工具仅使用标准库。测试与构建沿用仓库 .NET SDK 和现有 VSTest/xUnit。

```bash
# 查看本地暂存、未暂存、新增、删除和重命名输入对应的计划
python3 scripts/verification/test.py plan

# Bug 修复开发过程：必须显式限制到缺陷模块，可重复传入 --path
python3 scripts/verification/test.py run --scope iterate \
  --path src/AtomUI.Desktop.Controls/Popup \
  --path tests/AtomUI.Desktop.Controls.Tests/Popup

# 全量回归：仅在用户明确要求或发版时运行；Bug 修复与普通改动只验证所修改的模块
scripts/run-full-regression.sh

# 分支/CI affected 比较：必须显式提供比较基线
python3 scripts/verification/test.py run --scope change --base origin/main

# 需要额外验证时只追加，不替换自动选择的测试
python3 scripts/verification/test.py run --scope iterate \
  --path src/AtomUI.Desktop.Controls/ScrollViewer \
  --include-test 'tests/AtomUI.Desktop.Controls.Tests/ScrollViewer/**/*.cs'
```

`origin/main` 是基线参数示例，使用实际目标分支。`--base` 使用与 HEAD 的 merge-base；默认 HEAD 只比较工作区，
工作区干净时报告 `no-changes`，不会声称已验证整个分支。所有运行认证**当前工作树**；暂存版本与工作树不同的文件
单独列出，不能用这份报告声称暂存快照已经验证。

`plan --path <path>` 可以反复传入路径试算，输出明确标记为 preview。`run --scope iterate --path <path>` 是实际的
Bug 修复模块验证，只认证这些显式路径；它不读取工作树中的其他改动来扩大范围，也不能作为 change/full 证据。
`--json` 输出全部测试类、源文件、命中理由、编译项目、覆盖缺口和专项义务。

CLI 硬拒绝以下误用：iterate 未传 `--path`、focused `--path` 搭配 change/full、本地无 `--base` 的 change、以及 agent
通过 `test.py run --scope full` 发起全量。`--tests-only` 保留给 CI test phase；agent 不得把它作为本地绕过参数。
iterate 只使用模块自身的所有权、符号和测试文件关系，不自动扩大到跨模块显式规则和消费者传播；这些关系用于
显式分支/CI change 计划或已获授权的全量。适用专项义务按独立流程验证，不因 focused 通过而免除，也不自动触发全量。

明确请求或发版执行的全量若出现失败，当前全量进程继续执行以一次收集完整失败清单。每个失败分别回到精确测试和 focused 模块验证；
所有失败局部转绿后，只再运行一次最终全量确认。不得在每个失败修复后分别启动全量，也不得因全量失败而丢弃已经收集的其他项目结果。

## 选择依据

策略位于 `scripts/verification/test-policy.json`；实现位于 `scripts/verification/affected/`。

1. **所有权**：维护生产源码根与测试源文件模式，按控件家族或基础设施子域分组。基础控件与 Desktop 同一家族
   可以共享所有权，测试项目不必与生产项目同名。
2. **消费关系**：C# 类型、扩展方法与 AXAML 资源声明提供正向影响线索。项目依赖方向限制运行时消费关系，
   向外传播以实际改动文件的新旧声明为起点，保留直接控件家族的测试；消费者仅由实际引用文件的声明选择，
   不连带同目录中无关类型。直接消费者进入验证范围，继承关系继续传播。组合控件的整个公开类型不会无限传播，否则一个 Button 容易把所有
   窗口及窗口内所有控件重新带入全量。
3. **源码读取测试**：测试代码引用的输入文件名建立独立索引，不受 `ProjectReference` 限制。它只增加选择，
   不用于排除测试；同名文件可能保守地多选。文件读取关系优先于普通文档忽略规则。
4. **显式共享规则**：主题扫描、Popup 清单、Catalog/XLIFF、Gallery 结构、共享生成源码、图片加载、资源生命周期、
   没有项目引用的本地化集成测试等，使用明确的附加规则。目录扫描和动态依赖不能仅靠符号匹配推断。
5. **保守退回**：直接改动没有精确测试归属时运行完整所有者测试组并报告原因。没有对应测试的间接消费者纳入编译，
   在报告中明确标记为只有编译证据。无法找到所有者、规则引用不存在的 suite 时，报告 gap 并拒绝运行成功。

符号分析是保守的静态辅助，不是完整 C#/XAML 语义分析器，也不保证发现任意反射、配置拼接、资源 key 计算或
外部程序的依赖。新增这类路径必须增加显式策略，评审时解释其消费边界。发布/显式全量基线发现选测遗漏后，应先把
遗漏关系加入策略和工具回归测试，再修复产品问题，避免相同路径再次漏选。

测试类从真实 C# 声明发现，支持 partial、嵌套类、块命名空间和组合 Fact/Theory 属性，不从文件名猜类名。
测试 helper、全局初始化、AssemblyInfo、snapshot、测试资源等发生变化时，扩大到所属测试项目。

## 测试与构建成本

runner 用临时 solution 汇总选中项目，一次构建共享依赖；构建保留各项目声明的 TFM，不能把 netstandard 生成器
强制构建成 net10。随后测试使用 net10 与 `--no-build --no-restore`，并在 runsettings 中保存过滤器，避免命令行
长度限制。Restore 交给增量构建判断，不通过永久 `--no-restore` 隐藏新依赖。

构建与测试串行持有当前 checkout 的 OS 文件锁。保持原有 UI 程序集的串行测试约束，不通过关闭隔离约束提速。
该锁只协调统一入口；外部直接 `dotnet build` 不遵守锁，所以执行器同时检查 build 后与测试结束时的产物指纹。

无新改动时可以复用成功 receipt，但必须同时匹配：

- HEAD、暂存差异、所有 Git 管理/未忽略输入的当前内容；删除和新文件也参与。
- 完整测试计划、配置、SDK 信息、Python、平台和环境变量指纹。
- 选中构建图与测试运行依赖的产物内容，包括生成器与其他 TFM；产物被删除、替换或重建后重新验证。
  集成测试新建的独立 fixture 产物不属于被锁定的测试依赖，允许正常生成。

环境变量只保存摘要，不写入报告。`--fresh` 禁用 receipt 复用；CI 始终使用 fresh。
规划过程中、规划后、验证过程中输入变化都不能认证成功；缓存复用时也重新检查输入。

## 结果契约

报告位于忽略的 `.artifacts/verification/latest.json`；`receipt.json` 保存最近一份可复用成功证据，
`history.json` 最多保留 100 条耗时记录，CLI 的总耗时包含仓库发现、审计和影响规划。`metrics` 命令显示历史。日志、临时 solution、runsettings、TRX 全部放在
系统临时目录，正常退出、测试失败和可处理的中断后清理；强制杀进程后遵循系统临时目录清理策略。

| 状态 | 含义 | 退出码 |
| --- | --- | --- |
| `passed` | 本次计划内检查完成，不代表整个仓库已验证 | 0 |
| `reused` | 相同输入/配置/产物匹配已有通过证据，保留原验证时间 | 0 |
| `iteration-passed` | 指定模块验证通过，不代表 change/full；Bug 修复还需满足测试价值规范的完成门禁 | 0 |
| `tests-passed` | 显式 `--tests-only` 的测试阶段通过，专项义务仍单独列出 | 0 |
| `no-changes` | 当前比较范围没有改动，没有执行测试 | 0 |
| `pending` | 测试通过，但仍有专项验证义务 | 2 |
| `blocked` | 影响规则有缺口或指向不存在的 suite | 2 |
| `failed` | 构建、测试、证据或快照一致性失败 | 1 |

测试进程退出 0 不足以成功：必须有 TRX，选中测试数大于零，每个选中类有实际通过的结果，计数与逐项结果一致，
不存在失败、跳过或未执行结果。默认不把跳过的测试折算为通过。新增平台 skip 必须明确调整验证范围或提供对应
平台证据，不能通过忽略返回码解决。

普通文档可以得到不运行 .NET 测试的计划；被测试或生成器消费的文档先按输入关系选择验证。
每次收尾仍执行 `git diff --check`，并保留原始 UX/手动验证要求。

## 专项义务

发布注册、生成器、共享编译源码、产品项目配置、native 能力、打包资产等会产生 `obligations`。这些义务不因为
单元测试通过而消失，runner 返回 pending；单元测试证据可以复用，但不代表专项已完成。执行报告指定的真实 publish、package 或平台
验证，按 [AOT 编程规范](../development/aot-programming-guidelines.md)、
[Gallery 发布流程](gallery-aot-release-workflow.md) 和受影响平台文档汇报独立证据。

`check --id <计划中的义务 ID> -- <验证命令及参数>` 只执行与策略中该义务的 `verifier` 数组**完全相同**的命令，
记录当前输入/环境、完整命令、退出状态、耗时和输出摘要。多步骤义务必须先实现并审查覆盖全部步骤的验证脚本，
再将其完整 argv 注册到策略；不能用 quick 命令、单独 publish、启动进程或说明文字冒充完整验证。
当前发布/平台义务跨越多个现有手动流程，策略没有为它们预注册未经验证的统一 verifier；默认保留 pending，
按现有专项流程提供独立证据。工具拒绝未注册命令，不提供临时绕过。注册 verifier 的策略修改本身也需评审和验证。
随后再次执行同一 `run`，已通过的单测可以复用，当前输入下通过的专项证据附在报告中；失败或输入改变后的旧证据
不能满足义务。CI 需要专项检查时，同样在 `run` 前调用对应 `check`，使用已经验证的发布/平台脚本。

工具不提供 `--ignore-obligations` 或仅凭文字声明生成成功 receipt 的开关。NativeAOT publish 也不等于 UI 行为验证；
hover、滚动、裁剪、资源释放等仍保留原始复现条件和交互/生命周期检查。

依赖与 SDK 变更需要目标平台/TFM 验证；全量回归仍只在用户明确要求或发版时运行，focused 执行不会为了扩大范围自动运行全量。

## 全量、CI 和维护

```bash
# 仅在用户明确要求或版本发布时使用
scripts/run-full-regression.sh

# 维护影响规则时的快速检查
python3 scripts/verification/test.py audit
python3 scripts/verification/test.py self-test
python3 scripts/verification/test.py metrics
```

`.github/workflows/verify-affected.yml` 在 PR 上按基线选择测试，保存短期紧凑报告；手动 workflow 的 full test phase
是 CI 例外，仍使用 `test.py --tests-only` 生成机器可读证据。agent 本地最终全量只使用带进度、ETA、继续收集失败的脚本。
全量脚本失败时通过 `[DIAGNOSTICS]` 输出系统临时目录中的项目日志路径，保留完整异常、断言与清理阶段的栈；
不得只根据测试名推断失败原因。成功项目日志自动清理，失败日志在完成排障后清理，不写入仓库测试产物目录。
手工入口的 `all_platforms` 与 `browser` 默认关闭。前者额外运行 Windows/Linux/macOS 的选中测试和真实 OS NativeAOT 产品夹具，后者分别发布 trimmed interpreter／Mono AOT 并由 Chrome 执行 Browser 夹具。Browser 必须在实际页面 `pre#result` 出现 `PRODUCT_PASS browser` 并有对应 TypeMap 回执；页面加载或编译完成不算运行通过。只上传文本日志／回执，不发布 NuGet。夹具运行只证明该夹具合同，完整视觉、交互、性能和各家族义务仍单独验证。

CI 没有定期全量任务，不改仓库 branch protection；是否要求该 job 通过由仓库管理员配置。
默认 job 保留 `Selected tests and impact report` 名称，手工多平台时追加运行平台，使用 `--tests-only`，成功只表示选中的测试阶段通过；
专项义务仍保留在 JSON 中，必须由发布/平台检查单独覆盖。这个 job 不承担发布可用性 gate，不能把它的绿色结果
当成 NativeAOT、package 或平台行为已经验证。普通本地 `run` 不带此参数，专项未满足时仍返回 pending/退出码 2。

新增测试项目必须有生产路径映射；新增生产根必须有所有者或显式规则。audit 校验全仓库映射与测试项目发现，
不会因为当前 PR 没有触碰遗漏路径就放过新缺口。修改选择器时运行真实临时 Git 仓库和进程边界测试；测试必须
证明漏选、旧产物、空结果或错误输入会被识别，不能只断言配置文件包含某行文字。

收尾报告说明实际范围、命令、通过数量、耗时、复用情况、只有编译证据的消费者和未完成专项；Bug 修复同时提供
测试价值规范要求的测试处置与清理记录。CLI 成功仅是执行证据，不自动完成价值审查和修复验收。
若精确选择频繁退回整个 owner，先完善对应功能域与共享规则，再考虑拆分测试项目。按测试价值规范删除低价值或
临时测试，但不得靠削弱仍有效的契约或原始复现条件降低耗时；清理同步维护映射，不能忽略 gap 或零测试。

<a id="conditional-registration-backends"></a>

## Conditional registration backends

`src/AtomUI.Toolchain/Backends/ILLink8`、`src/AtomUI.Toolchain/Backends/ILC8`、
`src/AtomUI.Toolchain/Common/Registration` 与 `scripts/registration` 有独立的策略所有权。
这四个区域的 `tests: []` 表示目前没有可以直接证明其编译器闭包语义的常驻单测；
它不是忽略项，也不把其他后端的测试冒充为这些实现的覆盖。计划保留
`conditional-registration-engines`、`native-aot` 和 `package-layout` 专项义务；
即使编译成功或没有可选单测，也不能把结果称为后端验收通过。

已有测试继续属于其真实实现：`AtomUI.TypeMap.Linker.Tests` 验证官方 net10 后端的
元数据、物化和回执契约；`AtomUI.Build.Tasks.Tests` 验证工具身份、任务进程、链接输入
和发布回执；生成器测试验证生成的公共注册契约。修改这些模块时，现有区域映射仍选择
对应测试。Toolchain 的 Common 由相应 profile 编译复用，需验证真正消费被改文件的 Tasks、ILLink8、ILLink10
或 ILC8 构建入口。统一项目的默认 Tasks build 不等于验证其他 profile；只有 Common 的零测试计划不提供
全部工具 profile 的编译或运行证据。

`src/AtomUI.Toolchain` 根区域维护单一项目和 profile 接线，继续选择已有的任务宿主、生成器注入边界与
TypeMap 后端测试。`Common/Localization` 和 `Common/SourceGeneration` 分别选择其已有语言模型、代码命名与
资源 wrapper 测试；最长路径归属使这些规则不会吞并 ILLink8/ILC8 的按需专项边界。
策略中的 `toolchain-consolidated-source-deletions` 仅为工程合并保留旧路径删除的影响选择：Git 仍跟踪的
旧文件删除会触发原有 Tasks/TypeMap 测试。它不表示旧目录或项目仍存在，也不创建兼容源码副本。

专项按实际改动选择，不能用 .NET 10 单测或手工标记来注销 .NET 8 引擎义务：

| 影响范围 | 必须执行的专项与现有入口 |
| --- | --- |
| ILLink8 hook、Common binder、Core8 发布视图 | 固定 ILLink 8.0.27 的真实 SDK trim；覆盖实例化、反射/type checks、泛型晚到达、static/signature-only 否定、传递闭包/循环/组请求、选中 thunk 的 builder 参数，以及实际执行和输出 PE；输入/模板变坏必须在 Mark 前失败。构建入口及冷消费要求见 [Toolchain 构建与验证](../../modules/toolchain/build-and-verification.md)和注册契约。 |
| ILC8 adapter、driver、upstream 或 maintainer bootstrap | 按 `src/AtomUI.Toolchain/Backends/ILC8/README.md` 构建固定源码/工具，并执行 `verification/verify_backend.py --compiler-output <owned-output>`；scanner 与 no-scanner、优化、原生表/GC/预初始化分别验证。该机制脚本只证明其实际执行的组合，正式字符串 ABI、SDK 输入、资源和冷包仍须独立消费。 |
| net10 混合 ABI bridge、SDK 输入/输出阶段 | 用预编译 net8-only 包验证所改 managed/NativeAOT/Browser 路径，记录原件→转换副本→实际引擎输入→正式输出→部署字节；纯 net10 不依赖桥工具且不改写；伪造 accessor、缺协议、未知模板和工具冲突须拒绝。Browser AOT 的 IL stripping 是新的编译输出阶段，不能与转换前 DLL 哈希混为一谈。 |
| 工具打包或消费 targets | 正常构建的 nupkg、全新包/HTTP cache、一次 restore、随后 `publish --no-restore`；不手工补 cache/包文件，不在消费者里构建 runtime 或工具。核对应用运行依赖不含工具，验证实际最终输出、平台启动与资源行为。 |

以上义务包含依赖主机和平台的多阶段检查，当前没有一个固定、无参数命令能完整验证全部
组合，因此没有在策略中把某个部分脚本登记为完整 `verifier`。runner 会保留 pending；
交付记录须逐项说明实际命令、输入版本、运行结果、未验证平台及临时测试清理。
不得注入旧日志或伪造 specialist receipt 使 pending 消失。临时消费者和探针仍按测试生命周期
规范在验证后删除；必要的短日志与版本摘要不进入默认测试集合。
