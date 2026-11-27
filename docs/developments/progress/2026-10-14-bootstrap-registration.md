# BootstrapRegistration 实施与验证记录（2026-10-14）

设计：[控件注册分期与按需创建](../../architecture/foundations/lazy-control-registration.md)。

## 实施

- 新增 `[BootstrapRegistration]`，在 25 个 Desktop 控件与 5 个本地资源类上声明首阶段职责。生成结果选中 30/319 个片段；其余在 Stage 2 收集。没有声明的包保持完整注册。
- 源生成器按现有资产 export、Token owner、语义 owner 关系补齐本包阶段，不建立应用调用图，不改变裁剪根。无贡献标记报告 ATOMUIREG010。
- Core 在片段 Add 前分流。包补齐替换原快照，沿用 provider、commit ordinal 与统一资源排序；partial 指纹为空，完整包才恢复完整指纹。同步启动在返回前补齐。
- 收集/补齐失败均封存 builder，清空已发布和未发布 pending；初始化配置失败、取消和 Dispose 同样释放 continuation。首次资源工厂仍按查询执行。
- 既有 csproj AtomUICriticalControls 移除保持；旧 partitioned ABI 维持顺序完整收集适配。应用入口不变。

## 普通 JIT 对照

同一本机 macOS arm64、Release net10 Gallery，baseline 为本任务开始时完整注册工作树的实编产物；bootstrap 为本任务新产物。两份产物包含相同既有 Gallery timeline；没有启用 ReadyToRun。每版预热一次，之后 6 组交错进程；采样期间无本任务构建并发。单位 ms，表中为各组中位数。

| 指标 | 完整注册 | Bootstrap | 差值 |
| --- | ---: | ---: | ---: |
| Desktop 注册 | 186.80 | 55.95 | -130.85 |
| UseAtomUI 同步段 | 438.75 | 284.90 | -153.85 |
| 窗口首次 Render | 1372.95 | 1140.45 | -232.50 |
| 框架 Ready | 1807.60 | 1729.65 | -77.95 |
| Shell Ready | 2050.75 | 1971.75 | -79.00 |

首次 Render 从 Main 内探针起算，代表托管渲染回调，不是进程创建或显示器 presentation。此处使用实际 Gallery，不能把其绝对值与前一轮 800×500 骨架窗相混。记录的是单机样本，不能承诺所有机器固定减少同样的毫秒数。最后的未发布失败清理修复只调整异常路径，不改变采样成功路径的片段集合与执行顺序。

### 首 Render / Ready 原始样本

| 组 | baseline Render | bootstrap Render | baseline Ready | bootstrap Ready |
| --- | ---: | ---: | ---: | ---: |
| 1 | 1408.8 | 1173.5 | 1819.8 | 1745.4 |
| 2 | 1309.0 | 1333.7 | 1705.4 | 1933.5 |
| 3 | 1294.6 | 1138.5 | 1800.2 | 1713.9 |
| 4 | 1473.9 | 1142.4 | 1931.4 | 1661.3 |
| 5 | 1404.8 | 1130.2 | 1815.0 | 1649.9 |
| 6 | 1341.1 | 1107.4 | 1708.7 | 1768.3 |

## 测试生命周期与审查

- 临时 RED→GREEN：首次分流、缺失 foreign theme、封存后修改指纹、补齐失败清理、配置失败清理、收集未发布时 payload 保留。
- 临时覆盖：两阶段终态/同步路径、资源排序与 provider 稳定、去重、服务一次、partial 指纹、无效/非继承标记、多目标资产、同包及跨包 owner、应用真实取消并保留 builder。
- 独立审查 P2：收集器未发布即失败时仍持有 pending。修复后直接抛错、捕获 mutation 异常和缺资源工厂三种场景的 WeakReference 均可回收，后续 mutation 被拒绝。
- 修复后 Core 专项 82/82；生成器 123/123；Desktop 15/15。这里包含 16+5+1 个临时验证案例。临时源码、helper 和目录已全部删除；最终常驻测试净新增为 0。保留既有测试对新内部契约和两种后端生成形态的调整。

## 验证状态

- net10 Release Gallery、net8 Release Desktop：构建成功，均 0 warnings / 0 errors。
- 当前工作区 Release Gallery PID 56299：用户确认启动红绿灯、macOS 顶部应用名、窗内主菜单、五款主题、暗色/紧凑和新页面全部正常。精确路径 CUA 不能绑定，未用同名应用代替。进程已按 launcher 身份停止。
- 清理后聚焦验证：Core 66/66、Generator 118/118、Desktop 14/14；全部通过，无跳过。采用精确 filter 与已构建的当前 Debug 依赖，未运行全量回归。
- macOS arm64 NativeAOT：PublishToLocal.ps1 成功，产物验证通过；精确原生二进制（Mach-O arm64）以 timeline 启动，记录到 StartupReady/ShellReady 后 exit 0。此轮是正确性 smoke，不是发布包性能 A/B。
- 发布保留既有 ReactiveUI / ReactiveUI.Avalonia 的 IL2104、IL3053 和本机 Homebrew 动态库部署版本警告；未发现新增注册诊断。
- Browser Release trimmed interpreter：发布成功，TypeMap link inputs 已重新生成；本机临时 HTTP 服务加载真实产物，IAB 中 Overview 完整渲染，控制台 error 列表为空。测试标签页已关闭。

## 决策及范围

- 沿用用户正在审阅的工作区，保留已有改动；实现与验证完成后按用户明确授权提交。
- IsBootstrap 是 source marker 必需的隐藏生成 ABI 增量，旧片段默认 true；没有新增应用配置参数。
- 跨包 owner 保持真实 registry 校验：首阶段引用的 foreign owner 也必须已注册，否则明确失败；不跨程序集推测自动闭合。
- 测量时使用工作区已有的 Gallery timeline；提交前其调用已由其他工作清理，遗留的未跟踪探针文件不纳入本次提交。本次自建临时测试与测量产物已清理。
- Native/Browser、性能、真实 UI 三类证据分别验证；静态审查不替代实际发布或用户界面验收。

## 收尾

本次临时测试及专用目录已删除；测量/发布副本与临时服务已清理。保留上述原始样本和验证结论。`git diff --check` 通过。
