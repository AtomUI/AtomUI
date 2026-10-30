# AtomUI 启动性能分析与优化进展

初次分析：2026-10-10。实测与源码同步：2026-10-11。

**当前结论：Windows 上的一轮启动优化已实施并提交，有可重复的收益，但改善幅度有限。**
空窗口探针中，启动至根 Render 回调缩短约 202–209 ms（约 11%–13%）；AtomUI Window 场景仍需约 1.66 s。
这证明该轮重构有效，不表示启动等待已消除，也不支持原先把多个预期收益相加后的乐观目标。

对应生产提交为 `914ce40ef`（`perf(Core): 优化初始化算法并减少启动耗时与分配`），
其前一轮 Core 优化为 `ee1db4001`。本次文档同步核对的 HEAD 为 `106a9659d`。
原报告中的“改动未提交”是测量当时的状态；当前不再沿用该状态描述。

正式契约仍由[启动与注册链路](../../architecture/foundations/startup-and-registration.md)维护；
六项热点的实际处置与原方案修正见[启动热点与方案处置](2026-10-10-startup-hotspots-and-fix-designs.md)。

## 证据来源与适用范围

| 记录 | 场景与指标 | 本文用途 |
| --- | --- | --- |
| [Windows 优化结果](2026-10-11-windows-startup-algorithm-results.md)及[原始证据](2026-10-11-windows-startup-algorithm-evidence.json) | Release 空窗口；启动、初始化、Opened、根 Render、分配；每版本每场景 10 个独立进程 | 当前这轮优化的主要收益依据 |
| [前一轮 Core 优化结果](2026-10-10-core-startup-performance-results.md) | macOS Gallery 等价配置；CoreCLR / NativeAOT 的 UseAtomUI 调用；每产物 16 个独立进程 | 资源键、descriptor 绑定和 Manifest 优化的历史依据 |
| 本文“历史空窗口基线” | macOS 空窗口；Debug / Release 各 3 次 | 保留初次观察，不作为 Windows 优化的对照组 |

本次更新复核已有证据，没有重新执行启动采样。Windows JSON 的 60 条原始样本重新计算后与下表一致。
其 48 项源码清单中 47 项与当前工作树逐字节一致；Desktop 项目文件不一致，原报告另行记录了用户当时保留的
Debug 排除配置。因此这些数字属于当时的测量产物，不能称为当前 checkout 的重新实测结果。
二进制清单、探针哈希、配置和完整样本由原始证据保存。

三组记录的操作系统、配置、计时边界和源码基线不同，不能跨表相减、累加改善率，或把 macOS NativeAOT
结果当成这轮 Windows NativeAOT 的结果。

## Windows 当前一轮实测

### 方法与边界

- Windows 10.0.26200，SDK 10.0.401，CoreCLR 10.0.12，Avalonia 12.1.3，Release / net10.0，Win32，800×600。
- 基线产物来自 `ee1db40012292f3b4105a7810507e2475a177f23` 对应测量工作区；候选使用相同探针和项目选项。
  原始报告保留 baseline / candidate 的 105 文件清单及探针哈希，基线没有在采样时重新构建。
- 三种场景：纯 Avalonia、AtomUI 初始化＋Avalonia Window、AtomUI 初始化＋AtomUI Window。
  每版本每场景 10 次，共 60 个进程；每轮交换基线/候选先后顺序，串行采集，未剔除离群值。
- 独立新进程不等于清空 OS 文件缓存；用户后台进程与 OS 缓存未受控。采样期间没有任务自身的并行构建或测试。
- 启动时间包含进程/运行时启动及启动器开销；根 Render 是 Opened 后根哨兵的绘制回调，
  不证明整个子树渲染完成、合成器提交或屏幕实际呈现，更不等同于首屏可交互。
- 初始化分配为该区间的进程总托管分配，不是峰值 RSS、存活内存或整个启动周期的分配。

### 启动至根 Render 回调

下表均为中位数，单位 ms；改善率为 `(基线 - 优化后) / 基线`，差值由未舍入样本计算。

| 场景 | 基线 | 优化后 | 减少耗时 | 改善率 |
| --- | ---: | ---: | ---: | ---: |
| 纯 Avalonia 对照 | 530.947 | 531.143 | -0.196 | -0.04% |
| AtomUI 初始化＋Avalonia Window | 1571.324 | 1369.487 | 201.837 | 12.85% |
| AtomUI 初始化＋AtomUI Window | 1864.418 | 1655.117 | 209.300 | 11.23% |

两种 AtomUI 场景在各自 10 个配对轮次中均更快；纯 Avalonia 对照整体基本不变。
这是本组样本的方向一致性，不扩展为所有机器、配置的统计保证，也不据 10 个样本宣称稳定 P95。

### 初始化耗时与分配

| 指标 | 基线中位数 | 优化后中位数 | 改善率 |
| --- | ---: | ---: | ---: |
| AtomUI 初始化＋Avalonia Window：初始化耗时 | 1063.499 ms | 857.540 ms | 19.37% |
| AtomUI 初始化＋AtomUI Window：初始化耗时 | 1077.029 ms | 864.555 ms | 19.73% |
| AtomUI 初始化＋Avalonia Window：初始化托管分配 | 9,203,016 B | 5,270,280 B | 42.73% |
| AtomUI 初始化＋AtomUI Window：初始化托管分配 | 9,212,828 B | 5,273,260 B | 42.76% |

分配下降约 43%，但端到端回调等待只下降约 11%–13%。用户仍会经历明显启动等待；不能用分配改善率替代启动提速比例。
本轮是多项改动的组合 A/B，以下单项微测量只用于解释成本，不能把总收益重复归给某一项。

## 当前源码中的候选处置

| 初始候选 | 当前状态 | 已知边界 |
| --- | --- | --- |
| 控件共享资源键二维表预创建 | 前一轮已改为 registry 私有的按需缓存 | 生产资源查询路径与全局键表保留，不重复计入 Windows 本轮收益 |
| descriptor 绑定、资产指纹重复排序 | 已复用规范化 OwnTokens、registry 有序表和可信 Manifest 路径 | 任意输入仍独立复制、规范化及校验 |
| Gallery 重复完整配置 | RegisterViews 使用只读路由元数据；场景记录完整配置构建 7→1 次 | 78 条路由仍是工厂；应用配置独立，ViewLocator.Map 的复制成本仍在 |
| 语言 Snapshot 重复扫描 | Windows 轮次保留按需扩展候选链及部分重复扫描减量 | 来源优先级、稀疏互补、英文最终回退及全部支持语言验证保留 |
| 主题读取与缓冲区 | Core 自有内置主题转为构建期 Reader/XSD 校验；其他来源使用 4 KiB 池化读取缓冲 | 不能让所有 avares 来源免校验；实际字节上限、摘要、reload 识别仍在 |
| 控件全局 Token 复制 | 确定的生成式 schema 使用强类型复制，优化调色板复制及部分资源投影 | 仍保留独立可变副本；自定义 accessor 保留原语义 |
| 静态主题资源包装 | 符合条件的 typed theme 用 AddDeferred，部分已有 x:Class 字典直接构造 | Windows 记录 Desktop 315 项中 49 项 deferred、4 项 direct、262 项保留包装；没有通用惰性代理 |
| 包注册与 schema 指纹 | FNV 零后缀计算、操作内类型名缓存、owner 合并及注册分配等已优化 | 保留协议、逐资产证据、跨包 owner/资源契约；没有改为抽查 |

源码入口与单项证据见[六项热点处置](2026-10-10-startup-hotspots-and-fix-designs.md)及
[Windows 逐项结果](2026-10-11-windows-startup-algorithm-results.md)。
前一轮 macOS 的两版语言候选曾撤回；不能据此说当前 Localization 未优化，Windows 保留的是后续方案。

## 优化后仍有成本的阶段

Windows 报告对候选二进制副本做了两轮 Cecil 调用点插桩，每轮 3 个新进程。以下保留第二轮全部 3 个样本，
不删除明显更慢的首个样本，其原因尚未确定。单位均为 ms。

| 调用边界 | 进程 1 | 进程 2 | 进程 3 |
| --- | ---: | ---: | ---: |
| 完整 UseAtomUI 调用 | 1808.225 | 865.929 | 842.934 |
| configure callback / UseDesktopControls | 1058.307 | 326.370 | 324.896 |
| LocalizationBuilder.Build | 33.732 | 33.653 | 32.347 |
| ThemeManagerBuilder.Build | 54.470 | 53.571 | 54.471 |
| Owned services | 10.531 | 10.594 | 10.719 |
| ApplicationScope.InitializeApplication | 384.837 | 397.106 | 375.761 |
| ↳ 默认配置规范化 | 4.609 | 5.912 | 4.269 |
| ↳ 内置主题 Catalog | 14.728 | 23.459 | 15.193 |
| ↳ 静态资源挂载 | 183.207 | 186.835 | 176.225 |
| ↳ 首次 ApplyThemeAsync | 170.633 | 169.650 | 168.718 |
| Desktop runtime initializer | 39.245 | 38.894 | 38.663 |
| AtomUI Window 构造 | 82.602 | 73.737 | 73.748 |
| Framework Show 调用 | 1691.108 | 335.755 | 340.080 |

这是有扰动的诊断数据：区间包含首次调用/JIT，父级间隙还包含调用方、装载及日志成本；带箭头的行属于
ApplicationScope.InitializeApplication，不能与父级相加。它们不能直接从未插桩 A/B 总耗时中扣除，
也不能全部解释为可消除的算法工作。

后两个进程说明注册、资源挂载和首次主题应用仍值得优先细分，但尚未证明新的重复算法或进一步收益：
注册包含 Common prepare、provider、生成式收集、Build/AddPackage 与语言登记；普通非裁剪收集直接调用已知生成类型。
首次 ApplyThemeAsync 包含配置合并、编译/缓存与提交，不能把约 170 ms 全归为调色板计算或 Token 复制。
Window 构造与 Show 也要继续单列，不能把余量一概算入 AtomUI 初始化或宣布不可优化。

## 历史空窗口基线（macOS，2026-10-10）

初次骨架为 en-US 单语言、默认主题、AlibabaSans、UseDesktopControls、空白 Avalonia Window。
记录基线为 `ee1db4001` 加当时 ThemeSchemaRegistry 字符指纹与 ILC8 条件注册后端工作区改动。
更早的源码分析从 `f0e9b4133` 工作区开始，不能与上述计时基线混用。临时探针已删除。

| 配置 | Main→UseAtomUI 返回 | Main→Opened | Main→Dispatcher Idle |
| --- | ---: | ---: | ---: |
| Release，3 次均值 | 约 1.04 s | 约 1.21 s | 约 1.29 s |
| Debug，3 次均值 | 约 1.05 s | 约 1.28 s | 约 1.37 s |

这些是原记录的里程碑观测，不是当前源码的 macOS 复测，也不是实际显示首帧或首屏可交互证据。
原方法文字同时写“Opened 后退出”和 Idle 数据，退出时序需恢复探针核实，本文不补造解释。

原阶段记录中的 UseDesktopControls 约 230 ms、Builder 约 87 ms、MountStaticResources 约 140–195 ms、
首次 ApplyThemeAsync 约 120–160 ms 可以保留为定位线索，但与“UseAtomUI 同步段合计 460–490 ms”没有自洽的
包含关系说明。原 68% / 35% 占比及该合计不再作为有效成本分解；不能将其相加预测整体收益。

原 Gallery 约 1.8 s、优化后约 1.0–1.2 s 都属于按配置推算，未测量真实 Gallery 首屏。
旧文“1.0–1.2ms”为单位错误；本次撤下该目标，不以修正单位的方式把推算升级为实测。

## 前一轮 Core 优化的历史收益

[前一轮报告](2026-10-10-core-startup-performance-results.md)使用 macOS arm64、SDK 10.0.300、
Release / net10.0 和 Gallery 等价配置，OS 缓存预热，每产物 16 个独立进程。指标仅为 UseAtomUI 调用中位数：

| 模式 | 基线 | 优化后 | 改善率 |
| --- | ---: | ---: | ---: |
| CoreCLR 内核初始化 | 748.224 ms | 735.350 ms | 1.72% |
| osx-arm64 NativeAOT 内核初始化 | 82.021 ms | 74.634 ms | 9.01% |

当时保留资源键、descriptor 绑定和 Manifest 三项优化；已有字符指纹和 ILC8 工作区改动固定在各产物中，不计入该收益。
这些结果不是本轮 Windows 的前后对比，也不能证明 Windows NativeAOT 已验收。

## 后续研究与验收

1. 固定当前 Windows 产物、探针、窗口/字体/语言/主题配置，复用相同计时边界建立下一轮基线；
   如测真实 Gallery，另建默认路由及首屏交互场景，不把空窗口收益直接外推。
2. 优先细分包注册、资源挂载、首次主题应用，区分算法、JIT/装载与 owner 挂载成本；每次只验证一个因素。
   有成本不等于有安全可消除的工作，不为阶段预先承诺固定节省毫秒数。
3. 同时检查首帧主题完整性、首次导航、弹层、语言/主题切换，避免只把启动成本移到首次交互。
   保留应用隔离、资源优先级、失败行为和 AOT/裁剪契约。
4. 复用或编写临时差分验证，按项目测试生命周期清理后做明确模块路径的 focused 验证；
   未有用户指令不运行全量回归。每轮用同样采样策略比较，无有效收益或有行为回归时撤回性能改动。

Windows 原报告仍有原生 Gallery 视觉走查、缺少原生链接器导致的 NativeAOT 发布/运行、
3 个依赖符号链接权限的测试，以及原基线同样存在的 24 B 分配断言失败未闭环。
当时 Windows 临时证据目录的清理也记录为待处理；本次没有连接原环境或重新确认目录状态。
实际 full-trim consumer 与 Browser 的已完成验证不能替代这些缺口。

本次只同步两份分析文档、复核保存样本及源码引用；没有新增产品优化、重新跑性能或补齐上述运行验收。
