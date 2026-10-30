# 构建接线、工具链与发布交付

入口：[总体方案](overview.md)。本文所有新增资产路径和模块名均为拟定方案。

## 1. 框架和后端选择

模式由最终应用的 TFM、真实 publish 阶段、PublishTrimmed/PublishAot/RunAOTCompilation、RuntimeIdentifier 和引擎能力共同确定。
产品包构建时的 Debug/Release 宏不得决定消费应用的裁剪路径。

| 输入 | 选择 |
| --- | --- |
| 产品库 Debug | net10.0 |
| 产品库 Release | net8.0 + net10.0 |
| net8 普通应用、未裁剪发布 | 完整收集；不启动后端分析 |
| net8 trimmed CoreCLR | 受控 ILLink8 host + 条件后端 |
| net8 NativeAOT | 受控 ILC8 host + 对应 scanner/codegen 后端 |
| net10 desktop / browser | 保持现有官方 TypeMap / Browser 后端选择 |

库项目只提供定义，不抢占最终应用的分析所有权。同一最终应用只选择一个实际引擎流程；再逐输入包检查生成 ABI，
按照[混合包矩阵](backends.md)桥接或拒绝，不能只看应用 TFM 跳过 net8-only 包。
不能因多个传递包引用重复运行。
仅 `PublishTrimmed=true` 的普通 build 不等于已执行发布；门禁在 SDK 实际链接/编译目标上接线。

## 2. 初版工具布局

初版采用现有 NuGet build/tool 白名单机制内聚分发，避免依赖“第一次 restore 之后再临时添加工具包”的二次恢复链。
共享新增的托管后端资产示意：

```text
tools/registration/net8/
  contract.json
  host/                         # 受控 .NET 工具宿主与 runtimeconfig
  illink/                       # AtomUI ILLink8 adapter
  ilc/                          # 源码构建的 ILC8 托管组件与适配
buildTransitive/
  AtomUI.Registration.Net8.targets
  AtomUI.Registration.targets    # 保持统一选择入口
```

这些文件由现有 Generator 和携带注册构建资产的产品包共用同一清单。只有确定属于工具的程序集进入 tools/；
不得进入 lib/、产品 runtime dependency 或最终应用 publish 目录。不能额外手动维护另一份 package/project 清单。

初版会增加携带该工具集的包下载体积，应在发布报告记录压缩/未压缩增量；接受这一成本以保证冷 NuGet 消费单次正常恢复可用。
未来若拆独立工具包，必须先完成 restore-time 注入、PrivateAssets/IncludeAssets、传递引用和离线锁定验证，另行评审后再拆。
本方案不要求用户手动添加内部 compiler package。

## 3. 托管宿主与原生依赖

设计目标是工具宿主使用现有 .NET 10 构建环境，目标仍为官方 .NET 8 runtime/reference/native assets。
ILLink8 在 .NET10 宿主下已在原型验证；源码构建 ILC8 的 .NET10 宿主兼容仍属于 P1 必验项目，不能从 SDK10 构建成功推断。

ILC8 所需 jitinterface、clrjit、objwriter 等原生依赖从 SDK 已恢复的官方匹配 ILC host/target 包解析，不把任意系统目录或
“最新版本”作为 fallback。便携托管组件可共享，但原生依赖选择必须区分 host RID 与 target RID。
不全局设置用户的 IlcToolsPath，不修改全局 NuGet cache；只在当前受控 publish 的目标执行区间替换工具调用。

若特定组合不能在既有宿主上安全运行，应停止该组合交付并返回工具分发设计评审，不能暗中切换应用到 net10 runtime。

## 4. 能力清单

工具包携带签名/散列可核验的 capability manifest，至少包含：

```text
BackendId / BackendAbi / MetadataFormats
RuntimeSourceCommit / PatchSetHash
ManagedCompilerAssemblyHashes / AdapterHashes
SupportedHostRuntime + HostRid / TargetRid
SDKResolutionRules
ExpectedReferencePack / RuntimePack / ILCompiler / ILLink identities
ExpectedNativeHelperHashes
SupportedOptimizationModes / PreinitModes / FeatureProfile
EvidenceMatrixId
```

初始验证基线来源见[原型证据](evidence.md)，具体补丁版本属于可更新的能力数据，不成为永久架构常量。
未知组合一律不自动接纳。不能用 `>=8.x`、`>=10.x` 或相同程序集名称证明编译器内部 ABI 兼容。

正常 servicing 流程为：更新来源与 hash → 构建后端 → 完整相关矩阵 → 更新 capability 清单 → 审核产物 → 发布。
原生 helper、编译器组件、目标运行时不能各自升级。复用官方安全修复的补丁差异必须可追踪。

## 5. 构建阶段与接线

1. **Restore**：沿用 SDK 正常目标框架/runtime/native tool 包解析。候选工具全部来自已解析的包/源码构建资产。
2. **Compile**：普通生成器写片段、候选记录和入口；不运行完整应用分析，不读取旧 usage Sidecar。
3. **Prepare publish**：选模式，验证实际工具与资产，快照引用图、记录/入口和 publish 特性；建立独立输出目录。
4. **Analyze/materialize**：执行对应后端。NET8 NativeAOT 不插入 ILLink 预裁剪来代替 ILC；具体闭包由[后端设计](backends.md)拥有。
5. **Verify output**：核对无残留入口、选择/产物关系、内容 hash 和实际消费路径。
6. **Package/publish commit**：仅在全部步骤成功后提升输出与回执；中断和失败不可留下可复用的成功标记。

targets 以真实 SDK 执行阶段为接线点，显式声明 DependsOnTargets 顺序。不得依赖多个 AfterTargets 的偶然排序。
源码工具项目构建只由仓库既有构建入口承担；运行中的 Build.Tasks worker 不发起嵌套 restore/build。

## 6. 增量与并发不变量

区分两级身份，避免用尚未编译出的 DLL 内容决定它自己的编译路径：

1. 编译域按规范项目身份、配置、TFM、RID 和必要构建变体隔离 bin/obj；在普通编译前即可确定。
2. Compile 完成后冻结实现程序集及资源，计算 publish 输入摘要；分析、桥接副本、物化和最终 staging 按该摘要隔离。

发布输入身份至少覆盖最终应用、全部参与分析的实现程序集、编译型资源、候选记录、roots/substitutions、TFM、RID、
平台/特性开关、优化/调试/预初始化配置、工具及 native helper、后端配置和全部相关 MSBuild 导入。

- 内容使用 hash；时间戳、文件长度或 DLL 路径不能替代。
- 编译域和发布域均不得共享可写输出；通过不可变快照复制/内容寻址建立联系，并显式重定向后续消费路径。
  原型已出现只隔离 obj 导致旧 bin/native 二进制复用的反例。
- 同一输入域的 writer 串行持有锁；不同输入域可并行。SourceGenerator 缓存不得跨 Compilation 持有 Roslyn 对象。
- 完整成功后原子提交；失败、取消、异常与进程退出清理 staging 目录及锁。
- no-op 可复用，但需核对输入、工具和最终产物内容；任一变化使所有受影响分析/物化阶段失效。
- 日志、版本清单等纯诊断输出不能成为额外业务根。

## 7. 回执格式与消费校验

每次输出统一 receipt，包含 Backend/ABI、工具链清单摘要、输入摘要、Group 请求/选择摘要、阶段状态、输出 hash 与最终
编译/部署消费文件身份。NativeAOT 回执必须区分 scanner 结果、codegen/object 与最终 native executable，不能复用 ILLink 回执。

状态只允许单向提升：`prepared -> analyzed -> materialized -> verified -> consumed`。
任何阶段失败不得写 consumed。最终 SDK 输出/打包任务确认消费的是本次 verified 输出；不能只验证一个旁路样例。

报告 schema 可共享，但后端证据不能互换。用户侧摘要应明确普通构建、托管裁剪、NativeAOT、浏览器 AOT 和真实运行的区别。

## 8. 发布包验收

正式包清单增加明确 `PackageRole`：RuntimeProduct、Analyzer、BuildTool、Language、Template。角色在唯一清单中维护，
不能通过 csproj 某行是否恰好等于 `$(AtomUITargetFrameworks)` 来判断要不要验收；未知角色使打包失败。

RuntimeProduct 的 Release 包必须同时包含真实 `lib/net8.0`、`lib/net10.0` DLL 和正确 nuspec 依赖组；Debug 仅 net10.0。
检查 assembly TargetFrameworkAttribute、程序集版本/身份和每组依赖，不能仅检查文件名。其他角色按显式清单校验其原目标。

同时保留包 API 与布局基线比较。目标框架矩阵是独立绝对契约，即使上一版已经错误也不能放过。
包中缺失后端、携带多份不一致后端、工具泄漏进运行时输出、语言/Analyzer 框架漂移均失败。
新增隐藏 ABI 和 tools 布局必须进入发布迁移审计，不能通过 suppression 隐藏真实 API 破坏。

## 9. 平台交付范围

首个正式 Desktop 验收目标为 Windows/macOS/Linux 的既有受支持 x64/arm64 组合；host-target 组合逐项声明、逐项验证，
不默认承诺所有 cross-compile 笛卡尔组合。没有平台证据时标记未完成，不把本机 osx-arm64 结果推广为全平台。

生成器仍保持原 netstandard 目标；开发与性能工具使用开发框架；Browser/Mobile 的既有宿主和发布策略不由本次双 TFM 扩大。
版本号与正式发布动作在所有验收通过后单独决定；已有已推送标签和 NuGet 包不移动、不覆盖。
