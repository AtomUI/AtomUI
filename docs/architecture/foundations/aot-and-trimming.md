# AOT 与裁剪架构

本文是 AtomUI 控件自动注册与裁剪的总体架构、实施状态和产品交付门槛的正式所有者。

## 1. 状态与事实边界

**状态：TypeMap 主体源码迁移与旧注册路径退役已完成；资源平台声明收敛已实现；正式交付验收尚未完成，未发布。**

当前源码采用逐 Control 注册片段与独立主题资产，由官方链接器计算保留闭包。NativeAOT/CoreCLR 使用官方 TypeMap；
Browser/Mono 在官方 ILLink 标记之后将已选映射转换为静态映射。普通包入口、产品 buildTransitive 与独立冷 NuGet 消费
已经接入该实现。旧 usage、Sidecar、Unit、应用 Plan 与 LinkedPublish analyzer 已移除，没有可切回的 legacy backend。

2026-09-28 源码已采用[类型与资源平台契约](aot-typemap-registration.md#7-平台分支)：控件的普通 .NET 平台声明
自动约束其导出主题，主题自身的额外限制由正常资源 CLR 类型声明。项目主题路径排除列表、
`AtomUISupportedOSPlatforms` / `AtomUIUnsupportedOSPlatforms` 自定义元数据及其读取/传输字段均已删除。
无资源类时使用资产所属程序集域，显式非法资源类报告错误；其他合法 AXAML 根不按注册资源类校验。
这是现有 TypeMap 管线的输入契约收敛，不恢复旧依赖分析，也不改变一行包入口、Browser 精细裁剪或运行时注册 ABI。
本次资源平台声明收敛的生成器 449 项、独立目录受影响测试 6,392/6,392 项通过，构建产物指纹保持不变。
Desktop 全部资产工厂生成文本与迁移前一致，包括有效平台域、身份、顺序与 fingerprint。真实 Complex 样例的
普通桌面、NativeAOT 与 fulltrim CoreCLR 均启动并验证 AtomUI 窗口、标题栏和 OTP 模板；Browser interpreter/AOT
在真实 IAB 各通过 20 项运行检查，17 个桌面主题资产的工厂在两个部署模式中均缺失。冷 NuGet 作者、直接/传递/二进制
消费和浏览器运行通过；20 包完整性、相对 6.1.8 的布局以及打包任务隔离检查通过。
统一 runner 的单元测试与产物稳定性校验通过；其状态仍为 pending，因为 NativeAOT/package-layout 专项不能自动回填，
上述专项另有实际发布和打包命令证据，不手工伪造通过回执。本次是受影响范围验收，不等于全量或全部平台验收。源码责任见[控件注册生成器](../../modules/generator/control-registration.md#4-主题导出与资源)，
迁移步骤见[6.2.2 迁移说明](../../releases/6.2.2-api-changes.zh-CN.md#资源平台声明收敛)。

“源码已迁移”与“全部交付门槛通过”分别记录。2026-09-28 修复局部枚举键覆盖与注册契约后的真实桌面 Button/Window 样例在默认配置下为
20.21 MiB，同配置 full 注册为 44.40 MiB；`OptimizationPreference=Size` 下分别为 19.77 MiB 与 43.57 MiB。
两组均满足至少缩小 40% 的相对门槛，但均未达到 18 MiB 的绝对门槛，不能宣称体积验收完成。
样例仅使用 AlibabaSans，没有中文字体包；不得以移除正常全球化、图片服务或诊断能力满足门槛。

2026-09-29 在相同 SDK、RID、字体与配置下，固定反射保留收窄和短 TypeMap key 合并后的默认 selected/full 主程序为
20,182,000 / 45,001,952 bytes，selected 缩小 55.15%；`OptimizationPreference=Size` 下为
19,717,728 / 44,139,696 bytes，selected 缩小 55.33%。两组均通过运行检查和 40% 相对门槛；默认 selected 距
18 MiB 门槛仍差 1,307,632 bytes（1.25 MiB），Size selected 仍差 843,360 bytes（0.80 MiB），因此绝对体积门槛仍未通过。

上述 MiB 门槛已由 [8.1 体积评判标准](#81-体积评判标准) 取代，以上数字作为历史记录保留。2026-09-29 Token 描述符改用共享
codec/accessor 后，同口径（macOS 26.6.2、SDK 10.0.300、ILCompiler 10.0.8、Avalonia 12.1.3、osx-arm64、默认配置、仅
AlibabaSans）实测：Minimal selected 19,715,664 bytes（较 20,182,000 减少 466,336），full 注册 43,304,544 bytes，
selected 为 full 的 45.53%；FluentBaseline 16,573,248 bytes；框架增量 Δ = 3,142,416 bytes（AtomUI/Fluent 1.19，仅报告）。
三者均运行通过。此 Δ 记为首个棘轮记录，但归因门槛尚未通过：已归因的大项为 AtomUI 程序集约 1.42 MB（其中语言核心的
标准语言定义与 LanguageTags 约 250 KB）和默认图片服务网络栈约 1.0 MB（Net.Http/Security、Cryptography、Asn1、Sockets），
Fluent 侧则多出主题与 Avalonia 模板约 2.44 MB；两个已知缺口是 Desktop 10 个未用控件语言目录（估算 30–40 KB，
仍在 complete 阶段整包注册）以及 Regex、Immutable、Numerics 等较小 BCL 项尚未追到具体使用方。Size 档本轮未重测。

此前 Gallery Tab overflow shadow 渲染理论在原始基线及迁移分支均出现过失败，尚未确定成因；
本次受影响渲染组 77/77 通过，但没有通过修改阴影实现来消除此历史问题。
该失败不通过修改 Tab 几何或削弱像素断言绕过。完整 Browser Gallery、Windows/Linux/iOS 与较旧 macOS 的运行验证
不在本次本地主机证据内。NativeAOT 本机链接记录了 Homebrew 系统库的部署版本警告。

本次本地验证范围如下；构建成功与 UI 走查分别记录：

| 范围 | 当前证据 |
| --- | --- |
| 真实桌面 Minimal/Complex | NativeAOT 与 trimmed CoreCLR 均启动、创建模板并通过冻结/资源契约检查 |
| 独立 Browser Complex | trimmed interpreter 与真实 AOT 在 Codex IAB 执行 20 项模板、Token、Semantic、主题/scoped 更新、NumericUpDown 应用级主题替换及冻结检查；linked/deployed 未用工厂缺失；no-op 保持签名与回执 |
| 冷消费 | 独立包作者、直接/传递/预编译 binary adapter、冷 Browser 消费通过；跨资产字符串缺失诊断、显式 include 与纯 type-key 的普通/真实 full-trim 运行及未用目标缺失均通过；20 个当前包的旧协议与工具分发审计通过 |
| 同目录未使用控件 | 裁剪前有真实 TypeMap、代理与资产工厂；NativeAOT 增量 16,512 B，裁剪后代码/工厂缺失 |
| Gallery Desktop | NativeAOT publish 与精确路径进程启动通过；UI 工具无法绑定该可执行路径，视觉走查未完成 |

上一轮 TypeMap 主体注册修复的 Generator 418 项、Core 377 项及 NumericUpDown/标题栏定向 59 项验证通过。
上一轮保留的广域受影响验证为 7,105/7,106；已知 shadow 渲染失败、Gallery 视觉限制和未运行平台未被这次定向验证消除。
Chrome 自动化在本轮不可用，Browser 执行证据来自真实 IAB；此前 Chrome 记录属于原快照。

第 8 节门槛继续有效；工具链升级、跨平台验收、体积门槛和已知渲染失败必须分别有证据，才能声明对应范围完成。
本地包仍使用仓库现有版本号，此次迁移没有发布或改写历史版本事实。

## 2. 文档所有权

| 文档 | 正式职责 |
| --- | --- |
| [TypeMap 注册架构](aot-typemap-registration.md) | 条件映射、代理、Package marker、应用引导、执行模式与平台可用域 |
| [Control 注册契约](control-registration-contracts.md) | 类型、Token、Semantic Part、主题导出、资源作用域及优先级 |
| [浏览器链接架构](aot-browser-linking.md) | 已选映射转换、链接阶段、构建与失败边界 |
| [控件注册生成器](../../modules/generator/control-registration.md) | 普通生成器的职责与实现入口 |
| [TypeMap 链接工具](../../modules/typemap-linker/overview.md) | 浏览器构建工具的模块边界 |
| [TypeMap ABI 契约](../../reference/aot/typemap-contract.md) | 隐藏生成 ABI、标记与版本化协议 |
| [.NET 8 条件后端](aot-net8-conditional-backends.md) | 真实 ILLink8/ILC8 接线、混合 ABI、工具分发与当前支持边界 |
| [AOT 编程规范](../../engineering/development/aot-programming-guidelines.md) | 日常实现与审查规则 |
| [第三方 Control Package 指南](../../guides/theming/third-party-control-packages.md) | 包作者正常接入步骤 |

## 3. 目标与不变量

1. 应用保持正常包入口，例如 `builder.UseDesktopControls()`，不列出裁剪控件。
2. 第一方与第三方使用同一普通生成模型；作者不配置 TypeMap、裁剪 Unit、Sidecar、linker XML 或 AOT 控件名单。
3. 控件契约与资源资产分别建模，包和源码目录不决定保留粒度。
4. 依赖闭包由官方 ILLink/ILC 计算，不分析应用 C#/AXAML 使用，不另建跨 DLL 调用图或应用注册计划。
5. 运行时只查询已确定映射、激活已知代理、收集记录并提交；不扫描程序集或执行依赖图遍历。
6. 所有包收集完成后统一校验、排序、挂载和冻结；Token slots 与 Semantic registry 在首次控件实例化前固定。
7. 主题切换、scoped Token、模板创建均不追加注册，也不在资源缺失时重试完整注册。
8. Browser 裁剪解释执行与 Browser AOT 均精细选择；转换步骤不可用时发布失败。
9. Language、Provider、图片服务、Global Token、算法与 initializer 保持各自明确的生命周期顺序。
10. 构建工具不进入应用运行时部署；普通非裁剪执行和裁剪执行复用同一片段事实源。

任意运行时类型名、动态代码和发布后插件遵守 .NET AOT 的静态可达性限制。不通过悄悄保留整包掩盖动态边界。

## 4. 系统结构

```mermaid
flowchart TD
    Source[Control / Token / Semantic Part / AXAML] --> Generator[普通 AtomUI.Generator]
    Generator --> Facts[片段 / 资源工厂 / TypeMap / Package marker]
    References[已解析程序集引用] --> Bootstrap[仅读 Package marker 的应用引导]
    Bootstrap --> Targets[TypeMapAssemblyTarget]
    Facts --> Linker[官方 ILLink / ILC 可达性闭包]
    Targets --> Linker
    Linker --> Native[官方 NativeAOT / CoreCLR TypeMap]
    Linker --> Materializer[Browser 已选映射转换]
    Native --> Collection[包入口收集片段和工厂]
    Materializer --> Collection
    Collection --> Freeze[跨包校验 / 资源排序挂载 / 冻结]
```

每个具有 Token、Semantic Part 或导出主题的 Control 可以拥有片段。internal presenter 可以只有资源，不能为了注册而
伪造公开 Token identity。共享一个不可分割字典的多个 Control 通过真实工厂引用形成保留关系，资产按 AssetId 去重。

Common、Desktop、DataGrid、ColorPicker、Extras、GalleryBase 均采用此模型。Common 的图片加载、codec 与语言仍属于
Package Core，其控件主题也按实际契约选择；不能继续用 Common 整包注册绕过新模型。

## 5. 构建与消费模式

| 模式 | 注册执行 |
| --- | --- |
| 普通 Debug、非裁剪 Release，包括 Browser Debug | 直接收集全部片段，共用单项工厂与资源排序 |
| trimmed CoreCLR、Desktop NativeAOT | 官方 TypeMap 选择片段 |
| Browser trimmed interpreter、Browser AOT | 官方 ILLink 标记，再由浏览器后端转换已选映射 |

非裁剪全量是正常执行模式，不能作为裁剪发布失败后的恢复路径。模式由构建资产与官方 linker feature switch 选择，
不能由产品包的 Debug/Release 编译常量决定；同一个 NuGet 包必须能服务不同消费发布模式。

产品库 Debug 只构建 `net10.0`，Release 同时构建 `net8.0` 和 `net10.0`；浏览器宿主仍为 `net10.0-browser`。
上述官方 TypeMap 精细裁剪路径用于 .NET 10。.NET 8 从相同片段事实生成字符串候选 ABI，普通执行收集完整集合，
裁剪发布则由受控 ILLink8 / ILC8 在真实依赖图内选择片段，不引用运行时不存在的 TypeMap API，也不回退到完整注册。
后端按[目标框架规则](../../modules/toolchain/framework-routing.md)自动选择；工具组合、发布阶段和平台门禁见 [.NET 8 条件后端](aot-net8-conditional-backends.md)。
普通 .NET 8 构建成功不能证明其裁剪/NativeAOT 发布通过。该实现尚未完成跨平台正式交付验收。

源码、直接或传递 ProjectReference、普通预编译 adapter DLL、NuGet 消费均使用已解析包标记完成引导。
不要求消费类库导出 usage 清单，不沿 ProjectReference 传播旧 linked context，也不恢复旧 DLL 的方法体使用分析。
可选包的 marker 仅说明映射身份；只有真实 `UseXxxControls()` 才启用 Provider、语言和 initializer。

## 6. 注册生命周期

每次公共包入口执行以下顺序：

```text
检查 builder 状态及同包重复/递归进入
→ prepare（明确调用基础包或准备包服务）
→ 创建 Provider，按平台选择可用片段
→ 收集包 core 与片段的 descriptor、语义和资源 factory
→ 检查包内身份/记录结构并暂存 ControlPackageRegistration
→ 分配 PackageCommitOrdinal
→ complete（语言注册与 initializer 配置）
```

所有配置入口返回后，Build/InitializeApplication 统一验证跨包 RequiredTokenOwners、语义依赖和完整 schema，再按实际包提交
顺序及包内资源顺序创建、挂载资源并冻结。包暂存时不要求后续包已经提供其依赖；资源工厂也不能在本包提交时提前执行。

Common 在 Desktop 前、扩展包在基础包后的顺序由真实入口调用决定，不改为包名字母序。资源优先级的精确定义由
[Control 注册契约](control-registration-contracts.md#7-资源提交与优先级)拥有。

同一 builder 第二次调用相同公共包入口必须在 prepare 前失败；递归进入正在注册的同包也失败。内部基础包依赖需要 Ensure
语义时使用明确的内部操作，不能悄悄改变公共重复 Use 的契约。多个 builder 的状态独立，禁止静态缓存持有 builder 或 Provider。

注册失败使该 builder 无法继续 Build 成部分成功的应用。错误不能通过重试全量、late registration 或延迟 initializer 隐藏。

直接调用 `IThemeManagerBuilder.AddControlPackage` 也在同一 builder 上记录校验/暂存失败。
包、Token 或 Semantic descriptor/provider 冲突在写入集合前检查；捕获异常后继续 Build 或添加包仍失败。
此边界不重复进入生成入口的 Enter 阶段，独立 builder 的状态互不影响。

## 7. 静态边界与诊断

正常 Control、Token、生成的 Identity、Semantic Style、编译型 AXAML 与静态工厂都产生真实保留证据。
字符串 identity 只查询已保留 schema；任意字符串动态创建不自动激活控件。
确需动态选择时使用正常的类型化 root 或工厂契约，真实保存并校验 Type，不恢复 UnitRoot/PackageRoot 字符串协议。

普通包生成阶段检查主题导出、Token owner、资源依赖和可访问性。启动阶段检查跨包身份、选中资源及语义依赖。
可选包未启用时给出明确缺包或主题错误，不自动启用它。不能为复刻旧的应用 usage 诊断重新引入跨 DLL 扫描。

诊断 ID 由项目统一注册表分配；旧协议 ID 废弃后不复用。未知后端 ABI、不可解释的编译型资源输入和未转换 accessor
属于构建错误。宿主 DynamicResource 按开放主题输入处理，具体分类见 [资源契约](control-registration-contracts.md#6-资源作用域与动态输入)。

## 8. 验证与交付门槛

正式迁移必须同时证明行为、裁剪与构建产物正确：

| 维度 | 必须证据 |
| --- | --- |
| 执行模式 | 非裁剪、trimmed CoreCLR、Desktop NativeAOT、Browser trimmed、Browser AOT |
| 消费方式 | 同程序集、直接/传递源码引用、预编译 adapter、干净 NuGet 缓存 |
| 契约入口 | Control-only、Token-only、Identity-only、Style-only、internal presenter、泛型控件的非泛型 owner |
| 资源 | 默认/命名/多目标主题、局部及 include 作用域、宿主 DynamicResource、resource-only、确定覆盖顺序 |
| 生命周期 | 首次实例化前冻结、跨包依赖、原始 Package Core 顺序、重复/递归/失败、多个 builder 隔离 |
| 稳态 | 模板、主题切换、scoped Token 与 Semantic Part 正常且 schema 不变化 |
| Browser 后端 | 工具缺失/损坏/未知 ABI/残留 accessor 失败，正确增量失效，解释与 AOT 均细粒度 |
| 裁剪 | unused Control/proxy/factory 缺失，新增同目录无关控件不扩大集合，全量 manifest 不意外 root |
| 分发 | 构建工具自动注入且只注入一次，构建工具和旧协议产物不进入运行时部署 |

fixture 必须实际初始化 ThemeManager、解析资源、创建模板并切换主题。只构造 builder 或输出排序后的注册快照不能证明
冻结时序、资源优先级和 UI 行为。负向保留断言不能通过 `typeof(UnusedControl)` 将被检查类型自行保留。

### 8.1 体积评判标准

体积不使用绝对 MiB 门槛。主程序中 BCL、运行时与 Avalonia 占大头，会随 SDK/Avalonia 升级变化且不受 AtomUI 控制；
固定数值既区分不了“AtomUI 变大”和“工具链变了”，也说明不了剩余字节是否合理。标准只衡量 AtomUI 增加了什么、是否都有来由。

所有比较固定 SDK、RID、Avalonia 版本、配置、字体策略与原生库路径；记录 NativeAOT 主程序和排除调试符号后的 payload，
只在同一口径的程序之间相减。

| 条目 | 类型 | 规则 |
| --- | --- | --- |
| 对照组 | 定义 | `tests/AtomUI.Registration.Fixtures/FluentBaseline`：官方 Avalonia 模板（FluentTheme + Inter），与 Minimal 相同的 600×500 窗口、StackPanel 与一个 Button，相同 fixture 构建设置，不含 AtomUI |
| 框架增量 Δ | 指标 | Minimal selected 主程序 − FluentBaseline 主程序 |
| 归因 | 硬门槛 | ILC map 中 Δ 必须归到已使用控件的契约或文档列明的 Package Core 能力（主题/Token 运行时、注册、语言核心、默认图片服务及其网络栈、默认字体）；未用控件的类型、代理、工厂与本地化目录由 Inspector `selected` 模式证明缺失；归不进去的字节是缺陷，修复而不是放宽 |
| 棘轮 | 硬门槛 | 验收记录 Δ 与工具链身份（SDK、ILC、Avalonia）；同一工具链下 Δ 只能持平或下降，上升必须写明新增的具名能力并重新记录；升级工具链时两侧同时重测，比较 Δ 而不是绝对大小 |
| 机制检测 | 硬门槛 | selected ≤ 同口径 full 注册的 60%；新增同目录未用控件后其代码与工厂缺失，主程序增量 ≤ 256 KiB。两者检测裁剪机制是否失效（正常约 45% 与数十 KiB，失效时接近 100% 与整控件体积），不是体积目标 |
| 报告项 | 不设门槛 | AtomUI/Fluent 比值、`OptimizationPreference=Size` 数据 |

macOS arm64 的 Mach-O 段按 16 KiB 页对齐，主程序字节存在页量化；细粒度比较用 map 节点长度之和，主程序字节按页容差比较。
改变规则需要同口径的新证据；本节取代此前的 18 MiB、16 MiB 与“同场景 Fluent 的 125%”门槛。

上线前还须审计源码、项目、脚本、正式文档、nupkg 和构建产物：旧应用 usage/Sidecar/UnitEdge/SCC/Plan、旧 analyzer、
旧 linked MSBuild 传播均退出活动路径。不能只禁用 target 而继续编译分发旧工具。

退役范围仅限旧控件注册分析体系。资源 deferred wrapper、Localization 构建工具、Build Tasks 进程隔离、数据访问器生成、
有效 trimming annotations、字体/图标和原生发布设施继续保留。类型化注册不负责裁剪任意原生文件。
