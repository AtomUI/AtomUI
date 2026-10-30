# 原型证据与推断边界

入口：[总体方案](overview.md)。日期：2026-10-05。本文件只记录本次已发生的验证。

## 1. 来源与身份

本地参考源码目录未命中 dotnet/runtime 后，使用官方仓库：

- [.NET 8 源码](https://github.com/dotnet/runtime/tree/v8.0.27)，提交 `a6bde67c455f2ac219988c7a66171631090b6f65`。
- [.NET 10 对照源码](https://github.com/dotnet/runtime/tree/v10.0.8)。
- 原型宿主由 SDK 10.0.300 构建；Native compiler host 基于原始 net8 工具组件，实际目标运行时为 .NET 8.0.27。
- 托管扩展加载 shipped ILLink 8.0.27，执行宿主为 .NET 10.0.8；发布并运行的应用仍为 .NET 8.0.27。

版本用于复现此次证据，不是永久支持范围。后续 capability manifest 必须记录实际组件与原生依赖身份。

## 2. NativeAOT 结果

源码编译器适配添加真实条件依赖节点，在 scanner 固定点后将 app-owned dispatch 特化成普通 net8 静态调用。
CoreLib、runtime、jitinterface、clrjit、objwriter 使用匹配的官方 .NET 8 资产，未修改。

| 检查 | 结果 |
| --- | --- |
| 同源 stock ILC8 空入口基线 | 注册为空，程序按预期失败 |
| 接入条件图后的 12 个场景 | 全部运行成功，均报告 .NET 8.0.27 |
| 官方 .NET 10 TypeMap 同场景对照 | 12/12 选择集合一致 |
| 原生符号检查 | 选中 Register 方法存在，其余片段消失；未用 factory 标记缺失 |
| Static-only | 方法体真实保留，注册片段未被误选 |
| 无根循环 / 未请求入口 | 未选择，不发生全量保留 |
| 错程序集完整身份 / 禁用 scanner | 明确失败，无 fallback |

两个重要失败被保留为边界证据：

1. 仅隔离 obj 而共享 bin/native 会误复用其他场景的二进制。修正后所有矩阵按完整输出域隔离，早期结果不计入证明。
2. 静态构造函数预初始化会把中性空入口提前执行掉。加入仅针对该入口的解释屏障后，直接和经 helper/传递依赖的 cctor 场景通过。

正式比较摘要见 [prototype-results.json](prototype-results.json)。这份摘要可以独立阅读，不依赖临时目录存在。

## 3. Managed trimming 结果

真实 ILLink8 MarkStep 扩展选择并执行 9 个片段，独立重读程序集确认 8 个未用/无根/未启用类型缺失。
覆盖直接构造、替代 Token/Style 条件、反射、类型检查、晚到达条件、factory 传递依赖和有根循环。
StaticOnly 本体保留而 factory 删除；未请求 Group 对应 factory 删除。未知 ABI、缺失 factory 均在发布阶段失败。

没有修改 linker/runtime 源码，但这只说明已测条件可由现有扩展点表达；不能推导全部泛型/事件时序均无需补丁。

## 4. 尚未证明

原型为合成单程序集、单 Group、无参静态方法，Token/Style 为替代条件类。
真实 AtomUI builder 参数、代理与资源 ABI、AXAML、多程序集/Group、泛型/转发、完整动态反射、无 scanner、全部 RID、
冷 NuGet 工具分发、增量/并发和 servicing 都未获得产品级证明。

无 scanner 的原生 dispatch table 是本方案新增的待验证设计，不能从本次 12 场景结果推断成立。
Native compiler 在 .NET10 工具宿主下运行也需单独确认；本次“SDK10 构建”不是该项运行证明。

## 5. 外部机制依据

- [ILC8 条件依赖处理](https://github.com/dotnet/runtime/blob/v8.0.27/src/coreclr/tools/aot/ILCompiler.DependencyAnalysisFramework/DependencyAnalyzer.cs#L191)：已有图内条件边与工作队列。
- [ILC10 ExternalTypeMap 条件](https://github.com/dotnet/runtime/blob/v10.0.8/src/coreclr/tools/aot/ILCompiler.Compiler/Compiler/DependencyAnalysis/ExternalTypeMapNode.cs#L34)：条件类型节点关联映射目标。
- [ILC10 scanner/冻结阶段](https://github.com/dotnet/runtime/blob/v10.0.8/src/coreclr/tools/aot/ILCompiler/Program.cs)：官方优化路径也在 scanner 后固定映射。
- [ILLink/NativeAOT 差异反例](https://github.com/dotnet/runtime/blob/v10.0.8/src/tools/illink/test/Mono.Linker.Tests.Cases/Reflection/TypeMap.cs#L404)：不能将 ILLink 的较宽选择集合直接认作 ILC 等价结果。
- [NativeAOT 工具链构建说明](https://github.com/dotnet/runtime/blob/v10.0.8/docs/workflow/building/coreclr/nativeaot.md)：compiler/runtime/framework 是配套工具链，不以替换单个版本号获得兼容。

## 6. 材料处置

临时 compiler checkout、可运行夹具、隔离缓存和大型构建产物已删除。非运行形式的补丁、完整日志与身份记录留在
`/tmp/atomui-net8-conditional-backend-evidence-20261005/`，用于进一步调查，不是正式构建输入。
该临时目录可能被系统清理；正式长期事实以本方案中的摘要、后续验收和进入版本控制的产品实现为准。
