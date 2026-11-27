# 多设计语言控件主题首次装载重构实施计划

日期 2026-11-27。实施分支 `feature/refactor-theme-system`，起点为 `release/6.0` 的 `0c95ee2d2`。本计划随该分支的 v1.0 规范执行。

**交付目标。** 使用正式 NuGet 包的应用，在 JIT 与 NativeAOT 下均能让用户主题优先，只在首次需要隐式主题时物化当前作用域选中的资产。桌面 Button 用作首个真实包试点，各已登记控件的类型键沿用同一装载路径。实现可以在功能分支推进，启动性能继续作为放行门槛。[主规范 §5.1](../../engineering/development/multi-language-control-contracts.md#51-在第一次消费时选定实现)

**已有证据。** Avalonia 12.2.0 托管适配原型已在 macOS arm64 NativeAOT 通过 14 个场景，涵盖公开 BasedOn、应用 Style 优先、双作用域与落选工厂零构造。真实 AtomUI 作用域的 JIT 验证使用了可计数的试验资产。可复用这组预期；实际打包、真实 Button 导出优先级和完整窗口耗时是本轮工程需要回答的问题。[原型证据](../specs/2026-11-27-multi-design-language-control-customization-design.md#43-首次装载联合验证)

## 任务与依赖

| 任务 | 验收归属 | 启动条件 | 通过后交付 | 当前状态 |
| --- | --- | --- | --- | --- |
| T1 正式 NuGet 消费接入 | 打包与托管样式接入 | 已有联合原型结论 | 同一版本的候选包、独立消费者的 JIT 与 ILC 运行证据 | 通过，见 T1 记录 |
| T2 留存改造前的启动基线 | Gallery 启动性能 | 已知改造前的产品提交，可与 T1 同期准备 | 可重跑的基线产物、场景、环境与基线样本 | 已留存，见 T2 记录 |
| T3 真实 Button 试点 | 控件、主题包与 Core 资源装载 | T1 消费通过，T2 基线留存 | 通过优先级、成本、交互及裁剪验收的 Button 候选 | 通过，见值包登记复验记录 |
| T4 验证完整窗口启动 | Gallery 启动性能 | T2 的基线产物、代码与功能验收收尾 | 两种发布形态的配对结论和响应性结果 | `.5` 高负载记录保留，独占测量最后执行 |
| T5 迁移 | 主题运行时和控件维护者 | T3 已通过。T4 用于放行 | 统一装载接入、表单按钮主题与各边界验收 | 已实施，按钮家族正式包功能验收通过 |

T1 和 T2 可以并行准备。修改产品运行时代码前先锁定 T2 的基线输入。采样时独占测量机，错开 NuGet 构建和 NativeAOT 编译。后续复用同一份基线二进制与场景；比较时仍将基线与候选交错运行。

## T1　打通正式 NuGet 的 JIT 与 NativeAOT 消费

优先评估可分发的上游托管接入。需要固定 Avalonia 12.2.0 适配时，在打包或应用构建期准备带明确身份的应用私有产物，使 JIT 加载和 ILC 编译得到同一实现。只交付一种可维护的接入。作用域选择留在 Core，构建集成复用既有 Toolchain，业务控件及设计语言包沿用原有公共契约。[打包入口](../../../scripts/BuildNuGetPackages.ps1) · [正式包消费验证](../../engineering/workflows/affected-verification.md)

| 验收项 | 通过条件 |
| --- | --- |
| T1.1 包的来源与还原 | 经仓库 Release 打包入口生成带唯一候选版本的 nupkg。从独立最小应用以普通 `PackageReference` 消费，使用干净包缓存与 HTTP 缓存。每个发布配置只还原一次，后续构建或发布使用 `--no-restore`。记录包版本、哈希、SDK、TFM、RID 及解析到的 Avalonia 身份。整个过程无需 ProjectReference、手工替换 DLL 或修改共享缓存。 |
| T1.2 JIT 执行 | 运行 Release 消费程序。已启用的最小主题目标被应用普通 Style 完整覆盖时，断言用户模板实际出现、框架隐式解析和默认工厂均为 0；移除覆盖后能显示缺省模板。记录实际加载的程序集身份，与包输出核对。 |
| T1.3 NativeAOT 执行 | 先用已有证据对应的 `net10.0 / osx-arm64` 发布并直接运行原生可执行文件，重复 T1.2 的断言。查 ILC 输入清单，确认实际编入的实现与候选包一致；构建工具不进入应用运行时依赖。此结果仅覆盖所测目标。 |
| T1.4 兼容边界 | 未启用的消费者保留 Avalonia 原生主题行为。若采用固定版本适配，对一个不匹配的 Avalonia 输入给出可定位的构建诊断。记录受支持的版本、产物身份及其分发方式。若任一发布形态需要用户手工修补，T1 保持未通过，T2 可继续。 |

### T1 正式包消费记录（2026-11-27）

| 验收项 | 结果 |
| --- | --- |
| T1.1 | 用仓库 Release 构建和 pack 入口生成 `6.3.0-themefirstload.1`，范围为 Generator、Localization、Core、Ant Design 四个消费相关包。复用正式 v6.3.0 的 ILC8 构建资产并校验 manifest 与文件哈希，ILC8 配方在两个源码点之间一致。独立消费者在空的包和 HTTP 缓存里从候选源及 nuget.org 还原；最终项目每种配置一份 restore，随后 `publish --no-restore`。已核对 assets 清单的包版本与依赖类型。 |
| T1.2 | .NET SDK 10.0.403、运行时 10.0.14、`net10.0 / osx-arm64`。消费者使用生成的应用启动入口，实际提交 Ant Design 配置。Release JIT 中用户主题实际构建命名部件时，隐式解析/默认工厂为 **0/0**。移除应用 Style 后，默认命名部件出现，累计为 **1/1**。适配程序集 MVID 为 `cb712233-d897-4ff4-8ce0-622e68207dce`。 |
| T1.3 | 同一配置发布 macOS arm64 NativeAOT 并直接运行，重复得到 **0/0 → 1/1**。ILC 唯一一条 Avalonia.Base 引用指向应用中间目录生成的副本，SHA-256 为 `ae5c700498db10e3ccd02a8eea3770546c40e5bcf2e504c51ae7b2decfb67a23`，与 JIT 加载的文件一致。发布产物和 JIT 运行依赖清单未包含构建工具。 |
| T1.4 | 关闭构建开关后，Release 输出的 Avalonia.Base 与官方 NuGet 输入逐字节一致。应用 Style 仍胜出，官方隐式路径构造过一次默认主题。适配产物未为实例注册处理时，JIT 与 AOT 同样保留该行为。真实 12.1.6 NuGet 负例在构建期报 `ATOMUITHEME001` 并指明程序集位置。 |

两种发布方式都在应用构建期生成私有适配副本，保留 Avalonia CLR 装载身份。该副本由 AtomUI 构建工具生成，使用官方 Avalonia 12.2.0 作为输入。当前只验收 `net10.0 / osx-arm64` 的消费路径。T1 的参与类型和计数工厂属于验收驱动。实际 ThemeContext、包内 Button 导出位置、用户资源竞争及缓存失效仍由 T3 验收。

候选包保存在 worktree 的 `.artifacts/theme-first-load/packages/`。同一目录树下的 `consumer-results/identity.txt` 记录包及程序集哈希。`Consumer.ilc.rsp`、JIT/AOT 运行日志、资产清单及两种可执行文件也保存在 `consumer-results/`。临时项目、无效的早期适配和发布中间文件已清理。 清理后运行定向测试，ILLink10 相关用例 102/102、构建消费入口 5/5 通过。

## T2　留存可重跑的启动基线

1. **T2.1 固定产物。** 在运行时代码改动前记录 JIT 基线提交、用于 NativeAOT 对比的上一正式版本、SDK/RID、系统、渲染后端、发布参数及探针位置。优先复用身份和配置相符的现有产物；缺失时按原提交构建。两份基线均须实际启动，并留存可复跑产物的位置及哈希。
2. **T2.2 固定场景。** 分别准备 Ant-only、三种语言已启用且首屏只使用 Ant、Shadcn-only。两侧使用相同业务内容、窗口树、探针及发布模式；Shadcn-only 的首屏要能确认真实 Button 已呈现。记录各场景实际启用及显示的语言，以及 JIT/R2R 配置。
3. **T2.3 验收记录。** 使用既有采样器记录首个渲染批次、首次 UI 泵、最长 UI 间隔和 Gallery 完整 Shell 首个渲染批次。每个基线场景能复跑，原始样本可定位，配置、产物和指标能一一对应，T2 才通过。T4 直接沿用这些口径。[启动系统验收线](../../architecture/systems/startup/verification.md#2-验收线) · [启动采样器](../../../tools/performances/AtomUI.StartupPerformance/README.md)

### T2 基线留存记录（2026-11-27）

- macOS arm64，MacBookPro18,2，系统 27.2.1（26B131），Xcode SDK 26.5，.NET SDK 11.0.100，运行时与 ILC 10.0.14。构建基线时使用同一测量入口和原生 Gallery 后端。JIT/R2R 取 `0c95ee2d2`（Avalonia 12.2.0），AOT 取上一正式版 `v6.3.0`（`582bc6004`，Avalonia 12.1.6）。两侧均关闭外部用户主题目录，三种配置均在完整 Gallery Shell 显示同一个 Button。
- 两个可执行产物、对应原生依赖的哈希、探针、配置和所有采样位于 worktree 下 `.artifacts/theme-first-load/baseline/`。从 worktree 根运行 `python3 tools/performances/AtomUI.StartupPerformance/run_samples.py --config .artifacts/theme-first-load/baseline/variants.json --output .artifacts/theme-first-load/baseline/recheck --rounds 10 --warmup 1 --timeout 18` 可复跑。真实采样数据是该目录下 `samples/samples.json` 和 `samples/summary.json`。
- JIT 候选基线可执行文件 SHA-256 为 `6bce523f86eb6b1154d1a2aaf2112a8d271bef628f2ec807bb143b9831f7b26d`，AOT 正式版基线为 `14232a1f7e92ed4d9a9d33dda466b76efe546da4efb58f7a2d05ac0e817f1587`。JIT 启用了 Release 发布的 ReadyToRun 与单文件模式。AOT 使用正式发布的默认 EventSource 设置，因此此批 AOT 不含内核 Commit 事件值。
- 60/60 个正式进程正常退出。完整 Shell 首批中位数（Ant-only / 三语言以 Ant 启动 / Shadcn-only）分别为 JIT **851.18 / 846.21 / 836.31 ms**、AOT **406.17 / 420.44 / 417.02 ms**。样本保留首次 UI 泵、最长 UI 间隔和启动配置。主机一分钟负载为 25.54–45.98，采样器标记 60 个高负载样本。T4 在资源空闲时复用产物按同样配置和候选交错复测，用当轮配对数据作放行判断。

## T3　用真实 Button 验证资源装载

试点使用包内真实的 `AtomUI.Desktop.Controls.Button`、编译 AXAML 和实际资源挂载位置。Ant 路径保留既有 Button 体验；Shadcn 提供一份可辨识且独立于默认模板的 Button 主题。复用已有注册、ThemeContext 快照、主题缓存和失效机制。诊断计数放在测试侧的真实工厂和模板构建入口。只有试点暴露出现有设施无法表达的需求时才扩展实现。[Button 现有模板](../../../src/AtomUI.Desktop.Controls/Buttons/Themes/ButtonTheme.axaml) · [Button 既有契约](../../controls/desktop/general/button/overview.md)

| 验收项 | 输入与可观察结果 |
| --- | --- |
| T3.1 独立覆盖和缺省 | 在全新消费进程中只启用 Shadcn，显示一个 Button。先断言实际部件来自 Shadcn，再核对默认主题工厂/建树为 0/0、Shadcn 为 1/1。另以 Ant 缺省启动，断言原模板部件、视觉状态和鼠标/键盘激活结果符合基线，命令或点击事件各按既有契约触发。 |
| T3.2 来源优先级 | 在真实包的导出位置，与应用普通 Style、父模板对子控件指定的 Theme、应用或窗口类型键、第三方资源提供者逐一竞争。胜者身份和实际部件均正确。用户完整主题胜出时框架候选均为 0/0；移除后才首次取当前语言。实例非空 Theme 优先，显式 null 转入隐式查找，ClearValue 后绑定及样式按原生顺序接管。 |
| T3.3 公开键与 BasedOn | 编译两份派生 AXAML，同用 `BasedOn="{StaticResource {x:Type atom:Button}}"` 引用已导出的真实基础对象（`atom` 映射到 AtomUI 桌面控件）。继承模板时基础工厂/建树为 1/1；自带 Template 时为 1/0，派生部件实际出现。只改 Template 的应用仍得到基础主题中未覆盖的 Setter。 |
| T3.4 双作用域和生命周期 | 同一明暗变体下，外层 Shadcn、内层 Ant 的 Button 同时命中各自模板；去掉用户覆盖、往返切换语言及重挂载后仍命中正确。仅改 Token、有效模板身份未变时建树增量为 0。关闭窗口后，新增的订阅与旧控件引用可以释放。 |
| T3.5 正式产物与裁剪 | 在 T1 的独立包消费路径上运行 JIT 和 NativeAOT Button 试点。用实际切换后的部件和代表性业务交互证明功能。对本次主题导出的 TypeMap 变更按主规范核对隔离的 Token-only、Semantic Style-only 正例以及未使用控件、未查询语言包的负例；记录 ILC 闭包和运行结果。配置、依赖及输入未变的既有证据可复用。[NativeAOT 验收](../../engineering/development/multi-language-control-contracts.md#62-必败红灯red-green验收门禁) |

T3 任一必要断言失败，就在对应的打包、资源优先级或控件归属处修复后复验。试点期间只允许 Button 及其实际依赖进入新路径。动效内核重写、公共属性扩充及其他控件迁移另按各自规范排期。

### T3 真包试点记录（2026-11-27）

在 `net10.0 / osx-arm64` 使用 .NET SDK 10.0.403、运行时与 ILC 10.0.14。仓库 Release 构建及 pack 产生候选 `6.3.0-themefirstload.2` 的 15 个依赖包。独立应用经 PackageReference 还原，Release JIT 和 NativeAOT 的最终产物各运行 14 个真实 Button 场景。真实工厂计数来自 StartupEventSource 的主题资产物化事件，模板计数和来源由 TemplateApplied 及实际视觉部件断言。

| 任务 | 结果 |
| --- | --- |
| T3.1 | Shadcn-only 的 Ant 工厂/建树为 **0/0**，Shadcn 为 **1/1**。Ant 缺省的 Ant 工厂/建树为 **1/1**，Shadcn 为 **0/0**。两条路径都验证了鼠标和键盘各触发一次点击。 |
| T3.2 | 应用普通 Style、父模板部件 Style、应用或窗口类型键及外部 IResourceProvider 胜出，两个框架候选工厂均为 **0 次**。已测移除应用 Style 和窗口资源。实例与普通 Style 显式 null 均进入隐式查找。 |
| T3.3 | 原包在标准 Button 上复现了基础变体 Style 遮住派生主题的普通 Template Setter。候选把标准模板移到主题 Setter，保留虚线变体的状态选择器。编译 AXAML 的 BasedOn 引用同一个公开基础对象。继承模板时 Ant 工厂/建树为 **1/1**，自行替换时为 **1/0**。 |
| T3.4 | 同窗同明暗变体下，外层 Shadcn、内层 Ant 各选各自的模板。切换语言、只改 Token、切换期间设置显式主题并清除，以及重挂载均通过。显式主题屏蔽隐式选择期间清除过期缓存，不提前物化新候选。订阅沿既有卸载生命周期释放。关闭窗口后的对象回收尚未单独量测。 |
| T3.5 | NativeAOT 的 Token-only（实际宽度 16）和生成 Semantic Style-only（owner AtomUI:Button）通过。全文 ILC map 证实可达的 Shadcn Button 包装工厂保留，两个只启用 Ant 的产物虽然引用 Shadcn NuGet，仍裁除了该工厂。未使用控件负例 QRCode 未通过，未启用的 Shadcn 取值规则也仍被保留。 |

**未使用控件的裁剪阻塞。** 试点及已发布 `6.3.0` 的原包对照均在最终 ILC map 中保留了 QRCode 的注册片段方法、主题工厂方法和 Ant Design 取值规则方法。原包使用相同的 Button Token-only 输入。取值包的登记会生成整个控件包的描述符数组。每项 `ControlTokenRuleDescriptor.Create` 都通过强类型 Token 取得控件身份并实例化规则。已启用的 Ant Design 因此为未消费控件建立了静态引用。另外，应用的 StartupImage 静态引用候选值包的登记委托。按语言过滤仅发生于运行时，所以 Ant-only 产物中还可见 Shadcn 的 QRCode 规则。两个裁剪问题共用值包登记入口。T3.5 已证明 Shadcn 导出按需保留，未使用控件负例仍失败。T4 与批量迁移等待这条规则登记依赖解决，并在相同 NuGet 消费场景重新通过负例。[规则生成器](../../../src/AtomUI.Generator/DesignLanguages/ControlTokenRuleRegistrationGenerator.cs) · [规则描述符](../../../src/AtomUI.Core/Theme/Schema/ControlTokenRuleDescriptor.cs) · [StartupImage 生成器](../../../src/AtomUI.Generator/Startup/StartupImageWriter.cs)

原生发布产物、14 个场景的最终运行日志、独立资产清单、包哈希和 ILC 方法证据位于 worktree 的 `.artifacts/theme-first-load/t3-results/`。方法证据文件为 `trim-evidence.json`，对应完整 map 的 SHA-256 已登记。发布候选包位于 `.artifacts/theme-first-load/t3-packages/`。临时消费工程和原始图已清理。清理后的 Core 定向测试 39/39、Shadcn 控件规则定向测试 47/47 通过。

### T3 值包登记复验（2026-11-27）

候选 `6.3.0-themefirstload.4` 由 Release 打包入口构建，覆盖此验收的 net10.0 资产。独立消费端仍引用真实桌面控件包及两套设计语言值包，使用 SDK 10.0.403、运行时和 ILC 10.0.14、macOS arm64。两层条件映射分别按设计语言 Token 类型和控件 Token 类型选择。值包的 Group 通过生成的 `TypeMapAssemblyTarget` 让最终 ILC 发现跨程序集映射。普通启动沿用现有注册语义。[值包登记规则](../../architecture/foundations/aot-typemap-registration.md#nativeaot-值包登记)

- JIT、NativeAOT 分别通过原有 14 个 Button 场景及新增的关窗回收场景，共 15/15。关窗后完成 Headless 合成队列，确认 Button 和作用域各收到一次卸载，窗口、作用域和控件均可回收。关闭适配的 NativeAOT 对照也可回收。
- NativeAOT 的 Token-only 与 Semantic Style-only 正例通过，另以 Token-only 验证了应用的值包自动发现。ILC 产物仍保留 Ant Button 规则。三个 Ant-only 产物均保留 Shadcn 包引用输入，产物没有 Shadcn 规则方法和独立模板工厂。双语言产物保留 Shadcn Button 规则。
- 所有候选中，QRCode 的注册片段、主题工厂和规则符号计数均为 0。相同 Token-only 用例的原生主程序由 48,779,704 字节降至 24,036,808 字节，减少 50.7%。保持原来的 Token 值 16。
- 生成器定向测试 20/20、框架定向测试 39/39 通过。实际运行日志、候选包哈希、ILC 文件摘要、输入和验证记录保存在 worktree 的 `.artifacts/theme-first-load/value-rules/results/`。裁剪证据为 `ilc-retention.json`。临时消费项目及原始 map 已清理。

## T4　判定启动性能

- **T4.1 同机配对。** 使用 T2 固定的三种场景，每个变体至少 10 轮独立进程，基线和 T3 候选交错运行，排除预热并保留全部样本。每组比较同一场景；在完整 Shell 首批确认目标内容已经呈现。
- **T4.2 放行判据。** Release JIT 的 `Launch.ToShellFirstBatch` 配对中位差（候选减基线）须 ≤ 0 ms；Gallery NativeAOT 完整 Shell 首批不劣于上一正式版本。同时满足既有完整首帧、内核 Commit、首帧后响应性门槛。若首次点击或打开承担了原先的启动工作，补测该操作，须无可重复的退化。[正式判据与测量口径](../../engineering/development/multi-language-control-contracts.md#启动性能准入)
- **T4.3 异常处理。** 缺样本或噪声不能支持结论时记为待验；超线时按 `Phase.*` 和首次主题物化定位，修复后复测受影响配置。性能结果绑定候选的包哈希；对影响启动的实现改动重新取证。T4 未通过时保留发布门槛。

### T4 首轮配对采样（2026-11-27）

三种配置均沿用 T2 的窗口树、发布模式、探针和固定基线。候选对应 `6.3.0-themefirstload.4`。Shadcn 配置通过正式入口启用组件值与模板，实际确认了该语言的 Button 进入首个完整 Shell 渲染批次。每个配置的基线和候选交错各运行 10 个独立进程，两种发布形态共 120/120 次成功。负载记录为 22.14–37.26，采样器标记全部样本为高负载。

| 完整 Shell 首批，候选减基线 | Ant-only | 三语言启用，以 Ant 启动 | Shadcn-only |
| --- | ---: | ---: | ---: |
| Release JIT/R2R 配对中位差 | +19.62 ms | +5.79 ms | −33.74 ms |
| NativeAOT 配对中位差 | −4.88 ms | +3.95 ms | +4.50 ms |

该批数据未满足 T4.2 的无退步判据。JIT 三组内核 Commit 中位数分别为 66.91、48.50 和 42.33 ms。包含 Shadcn 的应用配置显式登记值包，`Phase.Configure` 配对中位差约 +30 ms、`Phase.Commit.Theme` 约 −24 ms，两段变化均为 10/10 同方向。Ant-only 的 Commit 配对差为 +0.22 ms。NativeAOT 使用 T2 的默认 EventSource 设置，当前采样未提供 AOT Commit 计时。环境负载和分段迁移使当前结果不足以归因，也不足以放行。待低负载重用同一组基线及候选产物取得配对数据，批量迁移继续等待 T4。

正式样本、配置、两种候选、探针和哈希保存在 worktree 的 `.artifacts/theme-first-load/value-rules/t4/`。配对指标见 `paired.json`，原始记录见 `samples/samples.json`，实际环境与哈希见 `samples/summary.json`。Gallery 临时探针已从工作树清理。

### T4 第二轮配对采样（2026-11-27）

复用首轮配置与产物，20 个可执行文件和原生依赖的 SHA-256 均与首轮一致。两种发布形态、三种配置各完成 10 对，120/120 次成功。一分钟负载为 45.26–80.37，120 个样本均被采样器标为高负载。每 5 秒观察一次后台构建和测试进程，29 次观测中有 15 次检测到活动。这些观测用于说明采样环境。

| 完整 Shell 首批，候选减基线 | Ant-only | 三语言启用，以 Ant 启动 | Shadcn-only |
| --- | ---: | ---: | ---: |
| Release JIT/R2R 配对中位差 | +17.41 ms | −21.07 ms | +16.26 ms |
| NativeAOT 配对中位差 | +28.02 ms | −0.26 ms | +8.61 ms |

JIT Ant-only 与 NativeAOT Shadcn-only 两轮均为正差，其余四组的方向在两轮之间发生变化。JIT Ant-only 的 `Phase.Configure` 和 `Phase.Commit` 配对中位差分别为 +0.35 和 −0.20 ms，首批正差仍待归因。启用 Shadcn 的 JIT 配置再次出现配置阶段约 +30 至 +31 ms、Commit 约 −22 ms 的变化。现有结果支持保留 T4 待低负载确认，T5 继续等待。

复测记录保存在 `.artifacts/theme-first-load/value-rules/t4/rerun-20261127-145850/`，含配对指标、完整样本、产物哈希和后台活动观察。

### T4 迁移候选配对采样（2026-11-27）

当前源码按正式发布参数分别生成 Gallery JIT/R2R 和 NativeAOT。两种产物均通过三种配置的真实窗口冒烟检查，包括 Shadcn-only 的真实 Shadcn Button。复用 T2 固定基线，启用 Shadcn 的候选仍通过正式入口登记它的桌面值与模板。六组各运行 10 对，120/120 次正常退出，负载为 27.32–80.29，120 个样本均被采样器标为高负载。

| 完整 Shell 首批，候选减基线 | Ant-only | 三语言启用，以 Ant 启动 | Shadcn-only |
| --- | ---: | ---: | ---: |
| Release JIT/R2R 配对中位差 | +30.89 ms | +11.93 ms | +21.38 ms |
| NativeAOT 配对中位差 | +15.72 ms | +57.55 ms | +18.91 ms |

六组均超出无退步验收线。JIT 内核 Commit 的候选中位数为 68.36、48.95、42.93 ms，均在 100 ms 以内。最长 UI 间隔的配对中位差为 −0.76、+6.19、+4.07 ms。AOT 保留正式发布的 EventSource 设置，因此本次未量到 AOT Commit。JIT 启用 Shadcn 的两组再次出现 Configure 增加 29.56–31.13 ms、Commit 缩短 23.12–23.89 ms，两个分段在各自 10 对内方向一致。对每对样本先合计 Configure 与 Commit 后计算中位差，两组分别增加 8.01 和 5.11 ms。Ant-only 的 JIT Commit 配对差为 −0.47 ms。当前结果不足以确定六组的启动差来自同一因素，T4 待低负载确认及分段定位。正式样本、配置、产物哈希和响应性记录在 `.artifacts/theme-first-load/migration/t4/`。临时 Gallery 探针已清理。

用相同二进制各加测 10 对整进程 CPU 时间。JIT Ant-only、AOT 三语言和 AOT Shadcn-only 的配对中位差分别为 +5、+10、+5 ms。该计时包含首帧后的探针观察期，说明其结果无法解释各启动阶段所占时间。复核发现上一正式版 AOT 包的 Avalonia 为 12.1.6，当前候选为 12.2.0；已用 tag 的依赖声明和发布目录中的原生库哈希核对。上一正式版仍是产品放行基线。这组跨版本差值不能单独归因于主题适配。CPU 观测见同目录 `cpu-check.json`。

另以相同 Avalonia 12.2.0 的 `.4` 试点候选对照 `.5` 迁移候选，隔离本批迁移的影响。40 个进程正常退出，负载为 26.49–38.20。JIT Ant-only 的完整 Shell 首批配对中位差为 −15.66 ms，AOT 三语言为 +14.67 ms。此诊断使用不同对照，放行仍以 T2 基线为准。记录见同目录 `migration-cost/`。

按后续确认的执行顺序，先完成代码迁移与功能验收。正式性能复测集中安排在最后，开始前确认测量机的独占窗口。上述高负载记录保留，T4 仍待通过。

## T5　按控件家族迁移

已登记类型通过统一的包元数据接入。为需要独立结构的控件提供语言候选，优先复用现有部件与资源。仅换 Token 已能满足设计的控件保留现有模板。各家族按受影响的公开键和实际交互取证。

- **T5.1 每批功能验收。** 复用 T3 的来源优先级、落选工厂和 Ant 缺省断言；为本批新增的特有交互或模板部件补一个能暴露回归的用例。应用现有属性、事件和专用部件样式仍可用。
- **T5.2 回归与处置。** 只复测该批影响的模块与包；触及共用接入、启动登记或裁剪条件时，分别重开对应的 T1、T4 或 T3.5 验收。该批不通过时保留已认证家族，撤回或修正该批的接入。新增全局服务、缓存或常驻监听须先给出真实消费者及度量依据。

### 表单按钮

覆盖 `SubmitButton` 与 `ResetButton`。两者通过各自的公开类型键提供主题，复用 Button 的外观和模板资源。默认主题保留 Primary/Default 类型和 Submit/Reset 本地化文案，Shadcn 候选分别从对应控件的 Token owner 取值。另以 DropdownButton 和 SplitButton 验证弹层和双目标交互。[提交主题](../../../src/AtomUI.Desktop.Controls/Form/Themes/SubmitButtonTheme.axaml) · [重置主题](../../../src/AtomUI.Desktop.Controls/Form/Themes/ResetButtonTheme.axaml)

| 验收项 | 首批判据 |
| --- | --- |
| 类型键与主题来源 | 用两套语言的正式 JIT 和 NativeAOT 消费各自的公开键。在同窗双作用域中断言实际模板和 Token owner，应用 Style 或实例 Theme 优先，清除覆盖后按当前作用域恢复。编译 AXAML 显式 BasedOn 能访问对应基础对象。 |
| 首次装载 | 独立 Shadcn 主题只构造获选资产及共用外观。应用完整主题胜出时，框架落选候选的工厂和建树均为 0。显式 BasedOn 的基础工厂单独计数。 |
| 表单交互 | 复用 Gallery 的验证表单布局。无效输入时，启用 `IsWatchValidateResult` 的提交按钮不可激活。有效输入后，鼠标和键盘激活各产生一次对应表单动作。重置恢复初值，应用对文案和命令的覆盖仍有效。 |
| 登记与裁剪 | 用实际包核对两种控件各自的主题和规则注册。选择一个与试点真正无依赖关系的控件作为裁剪反例，并复用 T3 对未启用语言规则的断言。 |

用户要求完成实现后，T5 在现有功能分支中推进。两轮高负载采样保留原结论。现有类型键的参与资格来自已经启用的包所声明的主题导出，资源或作用域变化复用 Avalonia 的隐式主题失效。默认视觉由既有主题提供，语言模板由各语言包按需贡献。

表单探针在启用和关闭作用域主题适配时都复现了同一个旧问题。应用 Style 移除后，FormActionsItem 中的按钮保留该 Style 设置的 Theme。目标位于样式继承树中，FormActionsItem 的普通逻辑子项集合没有该模板分支。适配沿 Avalonia 的样式继承树完成移除，重新解析当前作用域的主题。[Gallery 表单场景](../../../controlgallery/AtomUIGallery/ShowCases/DataEntry/Form/Views/FormShowCase.axaml)

### T5 正式包验收（2026-11-27）

使用 Release 打包入口生成的 `6.3.0-themefirstload.5` 与官方 Avalonia `12.2.0`，在独立 `PackageReference` 消费端验收 `net10.0 / osx-arm64`。JIT 与 NativeAOT 各通过六项场景。

| 场景 | 两种产物的实际结果 |
| --- | --- |
| Ant-only、Shadcn-only | 真实表单按钮命中各自模板。独立 Shadcn 模板胜出时，Ant 对应工厂为 0。默认主题保留表单按钮各自的类型和文案。 |
| 应用覆盖 | 用户主题胜出期间，两个框架候选工厂均为 0。移除 Style 后，FormItem 模板里的按钮及两个弹层按钮都恢复当前语言主题。 |
| 双作用域 | 同窗切换语言并清除实例 `Theme` 后，两种表单按钮在各自作用域恢复对应的模板。SplitButton 的两个内部按钮也读取当前作用域。 |
| 编译 AXAML 的 BasedOn | `SubmitButton` 的派生主题保留基底属性并应用自带模板，`ResetButton` 的派生主题复用基底模板。两者分别可引用公开类型键下的原主题。 |
| 按钮家族交互 | 无效表单保持禁用，提交的鼠标和键盘激活各触发一次，重置恢复初值。SplitButton 的主按钮提交点击，次按钮仅打开弹层，DropdownButton 打开菜单。 |

独立 Token-only 正例仍得到 Button 取值 `16`。完整 ILC map 保留 Ant Button 规则，QRCode 注册片段、主题工厂和规则、未启用的 Shadcn 规则均为 0；Shadcn 仍在消费者的输入依赖中。生成器 79 个定向用例通过，覆盖共享模板的 Semantic PART 解析、缺失部件诊断和启动登记。包 SHA-256、消费解析清单、适配副本哈希和裁剪统计见 `.artifacts/theme-first-load/migration/results/acceptance.json`，同目录保留运行记录。

临时表单用例、消费工程和 Gallery 探针已清理。共享模板资源的两项断言并入现有 Semantic PART 生成器用例，保护所有控件包共用的编译契约。AXAML 编译器能接受缺失 Semantic PART 标记的模板，真实包只能验证恰好存在的标记，无法长期保护这个诊断的正反两面。用例复用同一个生成器测试入口，各运行一次内存编译，无窗口或发布进程。清理后定向模块验证与 `git diff --check` 通过。

## 证据登记

执行时在本计划任务表更新状态，并在对应任务下记录候选提交或包哈希、配置与环境、实际入口、关键结果和产物位置。原型数字保留在既有实验记录；原始样本、二进制和日志放在构建产物或 CI 中。验收通过的任务写明适用范围，未运行的任务保持待开始或待验。
