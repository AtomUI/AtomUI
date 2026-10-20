# 应用资源宿主验证

本文拥有[应用资源宿主设计](application-resource-host.md)的源码对齐状态、验收范围与证据。
设计文档描述目标契约；本页区分目标、临时探针和正式实现，不能把其中一种证据替代另一种。

## 1. 当前实现与发布状态

本文记录 6.2.7 的共同宿主实现及 2026-10-20 的发布前验证；实际发布状态以对应 GitHub Release 与 NuGet 版本为准。
验证对象为 `41b7205a9e0f` 加本次
ApplicationScope / ThemeManager 工作区改动，依赖实际解析为 Avalonia 12.1.4、.NET 10、macOS arm64。

| 位置 | 当前实现 |
| --- | --- |
| ApplicationScope | 同一语言 Provider 插入根 Styles.Resources 合并集合起始位置，首次挂载位于回滚范围内 |
| ThemeManager | 只插入自己的 Resources；原根 Context Style 独立进入 Application.Styles |
| 框架前缀 | `[主题 Resources, 语言 Provider, 已有应用合并字典…]`，已有应用相对顺序与覆盖保持 |
| 释放 | 所有者记录实际目标；即使宿主后来替换字典，也从原位置卸载；资源通知抛错后继续释放兄弟 Provider |
| 开发者接入 | compiled App.axaml 可正常同时声明 FluentTheme 和 Application.Resources，无须后置补挂 |

源码入口为 [ApplicationScope](../../../../src/AtomUI.Core/ApplicationScope.cs)、
[ThemeManager](../../../../src/AtomUI.Core/Theme/ThemeManager.cs) 和
[WindowTheme](../../../../src/AtomUI.Desktop.Controls/Window/Themes/WindowTheme.axaml)。
覆盖迁移与旧产物限制见[开发者指南](../../../guides/theming/framework-theme-integration.md)。
独立 Style.Resources 同键覆盖的优先级变化已进入中英文 [6.2.7 迁移说明](../../../releases/6.2.7-api-changes.zh-CN.md)。

## 2. 本次实现证据

### 2.1 RED → GREEN

| 验证 | 修复前 | 修复后 |
| --- | --- | --- |
| 8 个临时 Core 资源/生命周期用例 | 6 失败、2 通过 | 8 全通过；额外强化首个子字典通知抛错场景后仍通过 |
| 原始 compiled AXAML 集成，JIT | 5 个目标断言失败 | 16 个断言通过 |
| 同一原始集成，裁剪 + NativeAOT | 同样 5 个断言失败 | 16 个断言通过 |
| 保持正确 UI 的旧版兼容接入基线 | JIT/AOT 均 16 个断言通过 | 用于下面性能对照，未用错误 UI 作基线 |
| Core Theme / Localization / Startup 精确测试 | 本次不是全量基线重跑 | 341 通过（含 8 个临时用例） |
| 官方主题资产专项 | 既有工具 | 370 个真实资产物化，应用 scope 完成释放 |

5 个失败分别是窗口装饰归属、语言资源键、初始动态语言、切换后的动态语言和移除用户覆盖后的装饰归属。
集成保留显式根 ResourceDictionary、FluentTheme、用户 StaticResource/DynamicResource 与两套按钮，不改变复现条件。
通过断言还包括 Light/Dark 往返、Avalonia variant、发布诊断、用户显式覆盖及移除后的恢复。
Core 覆盖应用已有合并字典次序、原宿主被替换后的卸载、owner 失败回滚、独立 ThemeManager 初始化、LTR/RTL 与根 Context。
原始报告见 [#506](https://github.com/AtomUI/AtomUI/issues/506) 和 [#507](https://github.com/AtomUI/AtomUI/issues/507)。

NativeAOT 通过正式生成式注册与 ILLink/ILC 管线；测量两侧显式打开 `EventSourceSupport=true`。没有新增 IL 裁剪告警；
两侧链接器均提示本机 Homebrew OpenSSL/Brotli 的最低 macOS 版本高于发布目标，该提示不构成旧系统兼容性认证。

### 2.2 原生观察与边界

真实 JIT 原生窗口外框 700×440；已打开截图并检查：标题与原生窗口按钮完整、初始 OK 文本可见、用户字体/前景生效，
AtomUI 圆角边框按钮与 Fluent 按钮均可见，无内容丢失或裁剪。NativeAOT 的 10 轮独立原生进程全部完成渲染批次并退出。

输入工具无法把测试进程置前，明确返回“未发送输入”，因此关闭/全屏等真实交互未计为通过。Windows/Linux 原生、
Prism 实机宿主及任意第三方主题组合未在本机认证；不能用这些资源断言代替对应平台的标题栏和弹层交互验收。

### 2.3 测试处置

长期套件只调整既有 ApplicationScopeTests、ThemeStartupTests 的物理挂载断言并加强原有行为案例，不增加永久用例数量。
临时 8 例、外部集成/性能 Demo、基线源文件注入目标及专用产物在验证后删除。截图及辅助缓存由取证工具清理，
审计结果 `removed=true`、`audit_error=null`、无 findings。清理后的最终模块结果见第 5 节。

## 3. 正确性验收矩阵

正式复现使用真实编译的 App.axaml，同时保留 FluentTheme 声明与显式 Application.Resources 根字典。
修复前必须因目标问题失败；不能用手工挪动资源集合代替原始复现。

| 范围 | 必须验证 |
| --- | --- |
| 初始消费 | AXAML 静态引用、Initialize 内创建 Shell、首次显示时主题和语言均完整 |
| 外部主题 | 基础主题不同声明位置、共享基础键来源、重复注册检查 |
| 用户覆盖 | Application.Resources、根 Styles.Resources 直接条目及已有合并字典，原相对顺序保持 |
| 覆盖兼容性 | 独立 Style.Resources 的同键优先级按目标契约变化；迁移示例有效 |
| 公开样式入口 | 普通 Setter、公开主题属性和生成的 Semantic Part Style 正常命中 |
| 本地化 | 服务和强类型资源查询、DynamicResource、LTR/RTL 往返、实例复用及通知 |
| 主题 | Light/Dark、跟随系统、局部 ThemeConfigProvider、Application 变体与发布诊断 |
| 生命周期 | 正常/重复 Dispose、挂载中途失败、AddOwner/通知抛错、第三方对象保留及可回收性 |
| 包与事务 | 包启用诊断、OnCommit/OnFirstUse、运行时追加、失败回滚与提交视图隔离 |
| 开放资源 | 未知键、手写 Provider、动态新增键和有序 fallback 不被跳过，外部资源不误触发包错误 |
| 控件场景 | AtomUI/原生控件、标题栏、全屏、Popup、Dialog、滚动和独立 Window |
| 发布 | 受影响目标的真实裁剪/NativeAOT 消费及生成式注册，不只普通 JIT 构建 |

Headless 验证资源行为和生命周期，原生运行验收画面与交互。平台结果按实际执行记录，不能从 macOS 或 Headless
推断 Windows/Linux 全部通过。服务对象存在不能代替资源键仍可查的断言。

## 4. 性能与惰性加载验收

### 4.1 基线

- 纯 AtomUI 与修复前基线比较，验证独立使用没有退化。
- 混合主题与具有正确画面、语言的兼容接入基线比较，保持同样的可见控件、资源值和最终 UI 树。
- 记录兼容接入差异，不能以错误标题栏或缺失文本减少的工作量作为性能收益。
- 同机、相同平台后端、相同探针边界和负载；JIT、R2R、NativeAOT 分开记录。

### 4.2 方法与判定

正式采样至少 10 轮交错独立进程，报告中位数、区间和配对差值；沿用
[启动验证规范](../startup/verification.md)中的相应门槛，不降低验收线。

| 指标 | 要求 |
| --- | --- |
| Commit、主窗体和完整 Shell 首批次 | 通过相应既有 JIT/AOT 门槛，无稳定可测量退化 |
| 分配与常驻结构 | 单独报告每应用宿主固定开销；不得复制资源树或增加每控件结构 |
| 启动、首次 Popup、主题切换的资产物化 | 对照正确 UI 基线，解释额外加载，不能无理由物化整套未用资产 |
| 已知键、外部键、miss | 分别记录耗时、分配、候选访问与开放资产构造，保留动态加键结果 |
| 响应与动画计时 | Dispatcher 初始化边界和响应性要求不退步 |

若查询前移带来不可接受的额外物化或耗时退化，调整宿主/键路由后重验。不得通过跳过合法 fallback、关闭功能、
全量预加载、减少原场景控件或提高阈值过关。没有前后采样时不宣称性能无损或速度提升。

### 4.3 2026-10-20 实测

同机独立进程交错各 10 轮、每变体预热 1 次，不剔除样本；基线和候选使用同一探针与平台边界。
纯 AtomUI 对比原实现，混合应用对比在 Initialize 中将 Fluent 移至起始位置并迁移旧语言字典的正确兼容实现。

| 场景 / 指标 | 基线中位数 ms〔min–max〕 | 候选中位数 ms〔min–max〕 | 配对中位差 ms |
| --- | ---: | ---: | ---: |
| 纯 AtomUI JIT / 进程到完整窗口首批次 | 819.465〔794.487–1133.468〕 | 817.241〔783.842–902.021〕 | −5.194 |
| 纯 AtomUI JIT / Commit | 119.956〔115.449–128.646〕 | 119.205〔114.269–125.560〕 | −2.915 |
| 混合 JIT / 进程到完整窗口首批次 | 935.366〔905.386–967.174〕 | 931.268〔916.242–990.763〕 | −0.053 |
| 混合 JIT / Commit | 100.433〔98.705–113.605〕 | 99.915〔96.356–103.421〕 | −1.669 |
| 混合 NativeAOT / 进程到完整窗口首批次 | 264.595〔221.043–286.394〕 | 253.083〔213.853–289.738〕 | −9.296 |
| 混合 NativeAOT / Commit | 2.434〔2.218–2.697〕 | 2.460〔2.218–2.582〕 | +0.054 |

每组 20 个正式样本全部有效，但全部触发脚本背景负载告警（load 约 21，超过 CPU 数一半）。以上是本机对照，
不据此宣称统计意义上的加速或完整发布性能达标。纯 JIT 的 Commit 两侧均高于既有 100 ms 线，未降低门槛；
该轮的完整窗口不是 Gallery Shell；本轮完整 Gallery 对照见第 4.4 节。相对 6.2.5 的全部历史发布门槛不由这组数据认证。

- 资产物化相同：纯 JIT 为 Desktop 12；混合 JIT 为 Desktop 14 + Common 2；混合 AOT 为 Desktop 13 + Common 2。
  不跨发布模式比较绝对资产数，未增加未用包或全套资产预加载。
- 混合进程总分配中位数：JIT 7,925,836 → 7,915,480 B；AOT 6,911,708 → 6,947,004 B。
  AOT 配对中位差为 +2,960 B；总进程分配包含调度与渲染波动，不等同于宿主结构分配。
- 已知基础键 / 外部键 / miss 各预热 1,000 次再查询 20,000 次；JIT 分配分别为
  12,000,792 / 14,080,472 / 15,360,688 B，AOT 为 52,486,448 / 54,085,440 / 55,365,440 B；各模式两侧完全一致。
  查询原有成本仍在，不能称为零分配。单次查询总耗时未作为正式性能提升证据。
- 常驻结构只新增 Scope 的目标引用及 Manager 的目标/根 Style 引用，64 位字段存储约 24 B/应用（不含对象对齐）；
  复用原字典、Provider 与根 Style。根 Styles.Resources 由 Avalonia 按需创建，无每控件结构、持续监听或查询代理。
- 最长 UI 间隔中位数：纯 JIT 42.371 → 42.276 ms；混合 JIT 43.440 → 49.814 ms；混合 AOT 8.496 → 28.058 ms。
  含启动早期绘制及背景调度，不能据此断言交互已全面达标。独立配置边界验收两侧各 119 个定时器样本：
  中位间隔 16.273 → 16.216 ms、最大 19.916 → 18.908 ms，登记/平台/Dispatcher 时序断言均通过。

### 4.4 v6.2.7 发布阶段的完整 Gallery 对照

对照 `41b7205a9e0f` 的宿主实现与本次修复；两侧均为完整 WorkspaceWindow/Overview，不删控件或资源。
JIT 与 NativeAOT 各 20 轮、每变体预热 1 次，交错执行并保留全部 80 个样本。采样期间停止本地构建与测试。
每个样本都完成渲染及正常退出，视觉节点固定为 338，首帧后探针数均不少于 20。
探针在主窗体首个请求的合成批次完成时直接开始采样，持续至完整 Shell 后至少 650 ms，包含首帧后的排队延迟。

临时源码替换最初晚于 MSBuild 输入缓存计算，预检发现新旧哈希相同，因此没有启动正式采样。
临时 hook 前移至编译依赖缓存计算前后，重新核验基线旧字段不存在、候选新字段存在；候选 Core 与校验包逐字节一致。
正式样本只采用以下已区分的产物，不纳入此前编译期间的冒烟时延：

| 产物 | 基线 SHA-256 前缀 | 候选 SHA-256 前缀 |
| --- | --- | --- |
| JIT Core | 3d25d03caf1cca7d | dd189a485f570ffe |
| NativeAOT 可执行文件 | 47847fe4ede1cac1 | 01cdd93bc9c818b4 |

| 模式 / 指标 | 基线中位数 ms〔min–max〕 | 候选中位数 ms〔min–max〕 | 配对中位差 ms |
| --- | ---: | ---: | ---: |
| JIT / 主窗体首批次 | 1699.528〔1590.693–1889.832〕 | 1713.122〔1612.190–1870.006〕 | -10.518 |
| JIT / 完整 Shell 首批次 | 1809.541〔1715.353–2005.733〕 | 1821.857〔1731.829–1978.628〕 | -2.454 |
| JIT / Commit | 111.785〔106.239–120.736〕 | 113.893〔106.120–125.581〕 | +1.200 |
| JIT / 首帧后最大 UI 间隔 | 48.843〔37.071–67.793〕 | 48.736〔35.835–92.435〕 | +0.232 |
| AOT / 主窗体首批次 | 303.610〔276.681–439.104〕 | 297.890〔286.326–792.332〕 | -3.414 |
| AOT / 完整 Shell 首批次 | 335.112〔308.821–481.585〕 | 329.897〔316.411–813.356〕 | -1.724 |
| AOT / Commit | 8.096〔7.646–9.483〕 | 8.182〔7.424–8.968〕 | +0.020 |
| AOT / 首帧后最大 UI 间隔 | 35.410〔22.764–62.656〕 | 30.928〔18.066–78.512〕 | +0.689 |

完整 Shell 的配对中位差满足本次相对基线不退步线，但不能据此声称统计加速或零成本。JIT Commit 两侧均超过
既有 100 ms 预算，AOT 两侧均低于 20 ms；未降低门槛，也不把所有性能项标为通过。JIT 分段中位数：
Localization 32.343→32.393、OwnedServices 7.003→7.076、Theme 51.554→52.389、Initializers 19.291→19.464 ms。
该预算缺口在基线已经存在，本次未扩展为启动编译器重构；当前数据也未认证相对 6.2.5 的全部历史发布门槛。

采样开始前 CPU 空闲约 70.5%，但 load 均值仍为约 14.8（10 核）；全部 80 个样本触发负载告警。
因此保留环境限制，不把噪声或接近零的配对差值解释成绝对无损结论。首帧后最大间隔配对差均不到 1 ms，
各组候选更快轮数均为 10/20，未观察到此前冒烟中数百毫秒的持续差异。

资产数各模式、各变体都相同：Common 3、Desktop 32、GalleryBase 2。总进程分配中位数：
JIT 25,496,084→25,487,332 B（配对 −4,656 B）；AOT 37,299,892→37,269,600 B（配对 −79,456 B）。
AOT 可执行文件增加 16,528 B，约 0.019%；它是固定代码/元数据变化，不是每控件分配。

标准 `PublishToLocal.ps1` 另外生成未接入测量探针的 osx-arm64 NativeAOT Gallery，原生产物检查通过，
InstallerConfig 版本为 6.2.7。基线、候选及标准发布均保留既有 ReactiveUI IL2104/IL3053 与本机 Homebrew
最低 macOS 链接版本提示；没有新增本模块的 IL 裁剪告警。包 API 和布局的实际结果由
[6.2.7 迁移说明](../../../releases/6.2.7-api-changes.zh-CN.md)拥有：不声明与 6.2.6 的生成组二进制兼容，
完整保留 10 条预期 CP0001，20 个包的布局与目标框架检查通过。

本轮还用仓库专用 launcher 验证运行路径和 PID。CUA 对确切可执行路径返回 Invalid app，按本地调试规范停止
视觉自动化，未回退到同名或已安装应用；因此完整 Gallery 的截图/真实点击验收未认证。第 2.2 节的混合窗口
截图观察与本节的渲染/性能数值分别保留各自的证据边界。

## 5. 最终模块验证与交付边界

临时验证和最终模块检查遵守[测试价值与生命周期](../../../engineering/development/test-value-and-lifecycle.md)及
[按影响选择验证](../../../engineering/workflows/affected-verification.md)。普通模块迭代不主动执行全量回归，不将先前发布的全量结果当成本次验证。
本次 6.2.7 发布阶段另外执行 `scripts/run-full-regression.sh`，最终 SUMMARY 为 15 个项目、6,514 项全部通过，
耗时 12:25（同机上次 11:49）。本地化消费项目构建期间触发一次 120 s 无完成告警，按用户要求继续等待，该项目最终 5/5 通过。

最终命令显式限定两个 Core 源文件及两个既有测试文件：

```bash
python3 scripts/verification/test.py run --scope iterate \
  --path src/AtomUI.Core/ApplicationScope.cs \
  --path src/AtomUI.Core/Theme/ThemeManager.cs \
  --path tests/AtomUI.Core.Tests/Localization/ApplicationScopeTests.cs \
  --path tests/AtomUI.Core.Tests/Theme/ThemeStartupTests.cs
```

清理后的最终结果：`iteration-passed`，4 个明确输入、51 个测试类、Core 366 项与 Desktop 6 项全部通过，
合计 372 项；本轮发布清理后的构建与测试共 64.542 s。现有两个测试文件的 Fact/Theory 数分别保持 9 与 10，永久用例净增为零。
22 篇改动文档的 211 个本地链接可解析，`git diff --check` 通过。

实现完成不等于已提交、已发布或已获全平台原生与完整 Gallery 发布性能认证。
